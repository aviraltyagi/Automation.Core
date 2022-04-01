using Core.Api.Helper;
using Core.Api.Utils;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Core.Api
{
    public class HttpClient
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly CookieContainer _cookiesContainer;
        private readonly RequestFormat _requestFormat;
        private readonly System.Net.Http.HttpClient _underlyingClient;
        private Dictionary<string, int> _counters = new Dictionary<string, int>();
        private readonly Dictionary<Assembly, Type[]> KnownTypes = new Dictionary<Assembly, Type[]>();

        private readonly IDictionary<RequestFormat, string> RequestMediaTypes =
            new Dictionary<RequestFormat, string>
            {
                { RequestFormat.Json, "application/json" },
                { RequestFormat.Xml ,"application/xml"}
            };

        public HttpClient(ScenarioContext scenarioContext, Uri baseUrl, RequestFormat requestFormat, CookieCollection cookie = null, Dictionary<string, string> customHeaders = null)
        {
            _scenarioContext = scenarioContext;
            if (baseUrl == null)
                throw new ArgumentNullException(nameof(baseUrl));
            var requestMediaType = GetValueOrDefault(RequestMediaTypes, requestFormat, default(string));

            if (string.IsNullOrWhiteSpace(requestMediaType))
                throw new NotImplementedException($"The operation for the enumeration value '{requestFormat}' is not implemented.");
            CustomHeaders = customHeaders ?? new Dictionary<string, string>();
            _cookiesContainer = new CookieContainer();

            if (cookie != null)
            {
                _cookiesContainer.Add(cookie);
            }

            var handler = new HttpClientHandler
            {
                CookieContainer = _cookiesContainer
            };

            BaseUrl = baseUrl;
            _requestFormat = requestFormat;

            _underlyingClient = new System.Net.Http.HttpClient(handler)
            {
                BaseAddress = BaseUrl
            };
            _underlyingClient.DefaultRequestHeaders.Accept.Clear();
            _underlyingClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(requestMediaType));

            if (Authorization != null)
                _underlyingClient.DefaultRequestHeaders.Authorization = Authorization;

            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        public Dictionary<string, string> CustomHeaders { get; private set; }
        public Uri BaseUrl { get; set; }
        public AuthenticationHeaderValue Authorization { get; set; }

        #region Http Methods

        public RequestResult<TResult, TError> PostParametersEncodedContent<TResult, TError>(string relativeUrl, Dictionary<string, string> parameters)
            where TResult : class
            where TError : class, IErrorDetails
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            var content = new FormUrlEncodedContent(parameters);

            return ExecuteRequestInternal<TResult, TError>(() => _underlyingClient.PostAsync(BaseUrl + relativeUrl, content), new FormUrlEncodedMediaTypeFormatter());
        }

        public RequestResult<TResult, TError> ExecutePostRequest<TResult, TError>(string relativeUrl, object request)
            where TResult : class
            where TError : class, IErrorDetails
        {
            return ExecuteRequestInternal<TResult, TError>(() => _underlyingClient.PostAsync(BaseUrl + relativeUrl, request, new JsonMediaTypeFormatter()), new JsonMediaTypeFormatter());
        }

        public RequestResult<TResult, TError> ExecuteGetRequest<TResult, TError>(string relativeUrl)
            where TResult : class
            where TError : class, IErrorDetails
        {
            return ExecuteRequestInternal<TResult, TError>(() => _underlyingClient.GetAsync(BaseUrl + relativeUrl), new JsonMediaTypeFormatter());
        }

        public RequestResult<string, TError> ExecuteGetRequest<TError>(string relativeUrl)
            where TError : class, IErrorDetails
        {
            Func<HttpContent, string> handler = content => content.ReadAsStringAsync().Result;

            return ExecuteRequestInternal<string, TError>(() => _underlyingClient.GetAsync(BaseUrl + relativeUrl), new[] { handler });
        }

        public RequestResult<TResult, TError> ExecutePutRequest<TResult, TError>(string relativeUrl, object request)
            where TResult : class
            where TError : class, IErrorDetails
        {
            return ExecuteRequestInternal<TResult, TError>(() => _underlyingClient.PutAsync(BaseUrl + relativeUrl, request, new JsonMediaTypeFormatter()), new JsonMediaTypeFormatter());
        }

        public RequestResult<TResult, TError> ExecuteDeleteRequest<TResult, TError>(string relativeUrl)
            where TResult : class
            where TError : class, IErrorDetails
        {
            return ExecuteRequestInternal<TResult, TError>(() => _underlyingClient.DeleteAsync(BaseUrl + relativeUrl), new JsonMediaTypeFormatter());
        }

        public RequestResult<TResult, TError> ExecuteJsonSerializedRequest<TResult, TError>(string relativeUrl, object request)
            where TResult : class
            where TError : class, IErrorDetails
        {
            var content = new StringContent(ToJson(request), Encoding.UTF8, "application/json");
            return ExecuteRequestInternal<TResult, TError>(() => _underlyingClient.PostAsync(BaseUrl + relativeUrl, content), new JsonMediaTypeFormatter());
        }

        #endregion

        #region Public Methods

        public Cookie[] GetCookies(string cookieName)
        {
            return _cookiesContainer.GetCookies(BaseUrl).Cast<Cookie>().Where(c => c.Name == cookieName).ToArray();
        }

        public void AddCookie(Cookie cookie)
        {
            if (cookie == null)
            {
                throw new ArgumentNullException("cookie");
            }

            _cookiesContainer.Add(cookie);
        }

        public void SetAuthorization(AuthorizationType authorizationType, string token)
        {
            string autorizationTypeText;

            switch (authorizationType)
            {
                case AuthorizationType.Basic:
                    autorizationTypeText = "Basic";
                    break;

                case AuthorizationType.Bearer:
                    autorizationTypeText = "Bearer";
                    break;

                case AuthorizationType.AuthId:
                    autorizationTypeText = "AuthId";
                    break;

                default:
                    throw new NotImplementedException($"The operation for the enumeration value '{authorizationType}' is not implemented.");
            }

            _underlyingClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(autorizationTypeText, token);
            Authorization = new AuthenticationHeaderValue(autorizationTypeText, token);
        }

        #endregion

        #region Private Methods

        private TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!dictionary.TryGetValue(key, out var obj))
            {
                obj = defaultValue;
            }

            return obj;
        }

        private RequestResult<TResult, TError> ExecuteRequestInternal<TResult, TError>(Func<Task<HttpResponseMessage>> func, MediaTypeFormatter formatter)
            where TResult : class
            where TError : class, IErrorDetails
        {
            var response = ExecuteRequestWithHeaderUpdateInternal(func);
            return response.IsSuccessStatusCode ? CreateResultFromSuccess<TResult, TError>(response) : CreateResultFromError<TResult, TError>(response);
        }

        private RequestResult<TResult, TError> ExecuteRequestInternal<TResult, TError>(Func<Task<HttpResponseMessage>> func, Func<HttpContent, TResult>[] responseHandlers)
            where TResult : class
            where TError : class, IErrorDetails
        {
            var response = ExecuteRequestWithHeaderUpdateInternal(func);
            return response.IsSuccessStatusCode ? CreateResultFromSuccess<TResult, TError>(response, responseHandlers) : CreateResultFromError<TResult, TError>(response);
        }

        private HttpResponseMessage ExecuteRequestWithHeaderUpdateInternal(Func<Task<HttpResponseMessage>> func)
        {
            // Code Cleanup
            string stepName = null;
            string testName = null;
            try
            {
                stepName = _scenarioContext?.StepContext?.StepInfo?.BindingMatch?.StepBinding?.Method?.Name;
                testName = TestContext.CurrentContext?.Test?.MethodName;
            }
            catch
            {
            }

            string testInfoString = testName == null || stepName == null ? null : $"BDD-{testName}-{stepName}";
            try
            {
                if (testInfoString != null)
                {
                    if (!_counters.ContainsKey(testInfoString))
                    {
                        _counters[testInfoString] = 1;
                    }

                    string correlationId = $"{testInfoString}-{_counters[testInfoString]}";

                    //_underlyingClient.DefaultRequestHeaders.Add(CorrelationIdHeader + Guid.NewGuid(), correlationId);
                    _counters[testInfoString]++;
                }

                foreach (var customHeader in CustomHeaders)
                {
                    _underlyingClient.DefaultRequestHeaders.Add(customHeader.Key, customHeader.Value);
                }

                return func().Result;
            }
            finally
            {
                //if (testInfoString != null)
                //{
                //    _underlyingClient.DefaultRequestHeaders.Remove(CorrelationIdHeader);
                //}

                foreach (var customHeader in CustomHeaders)
                {
                    _underlyingClient.DefaultRequestHeaders.Remove(customHeader.Key);
                }
            }
        }

        [DebuggerStepThrough]
        private RequestResult<TResult, TError> CreateResultFromError<TResult, TError>(HttpResponseMessage response)
            where TResult : class
            where TError : class, IErrorDetails
        {
            var errorText = response.Content.ReadAsStringAsync().Result;

            try
            {
                var error = new DataContractJsonSerializer(typeof(TError)).ReadObject(response.Content.ReadAsStreamAsync().Result) as TError;
                return new RequestResult<TResult, TError>(default(TResult), error, errorText, response.StatusCode, response.Headers);
            }
            catch
            {
                return new RequestResult<TResult, TError>(default(TResult), default(TError), errorText, response.StatusCode, response.Headers);
            }
        }

        [DebuggerStepThrough]
        private RequestResult<TResult, TError> CreateResultFromSuccess<TResult, TError>(HttpResponseMessage response)
            where TResult : class
            where TError : class, IErrorDetails
        {
            var allExceptions = new List<Exception>();
            foreach (var serializerMethod in GetDeserializers<TResult>())
            {
                TResult result;
                try
                {
                    result = serializerMethod(response.Content);
                }
                catch (Exception ex)
                {
                    if (IsFatal(ex))
                    {
                        throw;
                    }

                    allExceptions.Add(ex);
                    continue;
                }

                return new RequestResult<TResult, TError>(result, default(TError), null, response.StatusCode, response.Headers);
            }

            throw new AggregateException(
                "Unable to deserialize the server response. A proper deserialization method may be missing.",
                allExceptions);
        }

        [DebuggerStepThrough]
        private RequestResult<TResult, TError> CreateResultFromSuccess<TResult, TError>(HttpResponseMessage response, Func<HttpContent, TResult>[] constructorFunctions)
            where TResult : class
            where TError : class, IErrorDetails
        {
            foreach (var serializerMethod in constructorFunctions)
            {
                try
                {
                    TResult result = serializerMethod(response.Content);
                    return new RequestResult<TResult, TError>(result, default(TError), null, response.StatusCode,
                        response.Headers, response.Content.Headers.ContentType);
                }
                catch
                {
                    // ignored
                }
            }

            throw new NotImplementedException("Unable to deserialize server response.");
        }

        private Func<HttpContent, T>[] GetDeserializers<T>()
        {
            return new Func<HttpContent, T>[]
            {
                DeserializeUsingDataContractJsonSerializer<T>,
                DeserializeUsingJsonConvert<T>,
                DeserializeUsingReadAsAsync<T>,
                DeserializeUsingReadAsStringAsync<T>
            };
        }

        [DebuggerStepThrough]
        private T DeserializeUsingDataContractJsonSerializer<T>(HttpContent content)
        {
            return (T)new DataContractJsonSerializer(typeof(T), GetAssociatedKnownTypes(typeof(T)))
                .ReadObject(content.ReadAsStreamAsync().Result);
        }

        [DebuggerStepThrough]
        private T DeserializeUsingJsonConvert<T>(HttpContent content)
        {
            return JsonConvert.DeserializeObject<T>(content.ReadAsStringAsync().Result);
        }

        [DebuggerStepThrough]
        private T DeserializeUsingReadAsAsync<T>(HttpContent content)
        {
            return content.ReadAsAsync<T>().Result;
        }

        [DebuggerStepThrough]
        private T DeserializeUsingReadAsStringAsync<T>(HttpContent content)
        {
            if (typeof(T) != typeof(string))
            {
                return default(T);
            }

            return (T)(object)content.ReadAsStringAsync().Result;
        }

        private Type[] GetAssociatedKnownTypes(Type type)
        {
            Assembly assembly = type.Assembly;

            lock (KnownTypes)
            {
                if (KnownTypes.TryGetValue(assembly, out var result))
                {
                    return result;
                }

                result = FindKnownTypes(assembly);
                KnownTypes[assembly] = result;

                return result;
            }
        }

        private Type[] FindKnownTypes(Assembly assembly)
        {
            var exportedContractTypes = assembly
                .GetExportedTypes()
                .Where(t => t.IsClass && t.IsDefined(typeof(DataContractAttribute)))
                .ToArray();

            var abstractContractTypes = exportedContractTypes.Where(t => t.IsAbstract).ToArray();

            var knownTypes = new List<Type>(exportedContractTypes.Length);
            foreach (var type in exportedContractTypes)
            {
                if (type.IsClass && !type.IsAbstract
                                 && type.IsDefined(typeof(DataContractAttribute), false)
                                 && abstractContractTypes.Any(abstractType => abstractType.IsAssignableFrom(type)))
                {
                    knownTypes.Add(type);
                }
            }

            return knownTypes.ToArray();
        }

        private bool IsFatal(Exception exception)
        {
            switch (exception)
            {
                case ThreadAbortException _:
                case OperationCanceledException _:
                case OutOfMemoryException _:
                    return true;
                default:
                    return exception is StackOverflowException;
            }
        }

        private string ToJson(object instance)
        {
            var serializer = new DataContractJsonSerializer(instance.GetType(), GetAssociatedKnownTypes(instance.GetType()));

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, instance);
                return Encoding.Default.GetString(stream.ToArray());
            }
        }

        #endregion
    }

}
