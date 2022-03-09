using Core.Api.Helper;
using Core.Api.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using TechTalk.SpecFlow;

namespace Core.Api
{
    public class Context
    {
        private HttpClient _httpClient;
        private string _baseUrl;
        private RequestFormat _requestFormat;
        private ScenarioContext _scenarioContext;

        public Context(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        public HttpClient HttpClient
        {
            get => _httpClient ?? (_httpClient = HttpClientFactory(_scenarioContext, RequestFormat));
            set => _httpClient = value;
        }

        public void SetBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public RequestFormat RequestFormat
        {
            get => _requestFormat;
            set
            {
                _requestFormat = value;
                _httpClient = HttpClientFactory(_scenarioContext, _requestFormat);
            }
        }

        public HttpClient HttpClientFactory(ScenarioContext scenarioContext, RequestFormat requestFormat)
        {
            return new HttpClient(scenarioContext, new Uri(_baseUrl), requestFormat);
        }

        public ErrorInfo LastError { get; private set; }

        public object LastResult { get; private set; }

        public HttpStatusCode StatusCode { get; private set; }

        public MediaTypeHeaderValue ContentType { get; private set; }

        public HttpResponseHeaders Headers { get; private set; }

        #region HttpMethods
        public TResult ExecutePostRequest<TRequest, TResult, TError>(string url, TRequest request, bool stopAtFailure = false, string errorMessage = null)
            where TResult : class
            where TError : class, IErrorDetails
        {

            var executionResult = HttpClient.ExecutePostRequest<TResult, TError>(url, request);
            SetLastResult(executionResult, stopAtFailure, errorMessage);
            return executionResult.Result;
        }

        public TResult ExecuteGetRequest<TResult, TError>(string url, bool stopAtFailure = false, string errorMessage = null)
            where TResult : class
            where TError : class, IErrorDetails
        {
            var executionResult = HttpClient.ExecuteGetRequest<TResult, TError>(url);
            SetLastResult(executionResult, stopAtFailure, errorMessage);
            return executionResult.Result;
        }

        public string ExecuteGetRequest<TError>(string url, bool stopAtFailure = false, string errorMessage = null)
            where TError : class, IErrorDetails
        {
            var executionResult = HttpClient.ExecuteGetRequest<TError>(url);
            SetLastResult(executionResult, stopAtFailure, errorMessage);
            return executionResult.Result;
        }

        public TResult ExecuteGetRequest<TResult, TError>(string url, IDictionary<string, object> queryParameters, bool stopAtFailure = false, string errorMessage = null)
            where TResult : class
            where TError : class, IErrorDetails
        {
            var executionResult = HttpClient.ExecuteGetRequest<TResult, TError>(ResolveQueryParameters(url, queryParameters));
            SetLastResult(executionResult, stopAtFailure, errorMessage);
            return executionResult.Result;
        }

        public string ExecuteGetRequest<TError>(string url, IDictionary<string, object> queryParameters, bool stopAtFailure = false, string errorMessage = null)
            where TError : class, IErrorDetails
        {
            var executionResult = HttpClient.ExecuteGetRequest<TError>(ResolveQueryParameters(url, queryParameters));
            SetLastResult(executionResult, stopAtFailure, errorMessage);
            return executionResult.Result;
        }

        public TResult ExecuteDeleteRequest<TResult, TError>(string url, bool stopAtFailure = false, string errorMessage = null)
            where TResult : class
            where TError : class, IErrorDetails
        {
            var executionResult = HttpClient.ExecuteDeleteRequest<TResult, TError>(url);
            SetLastResult(executionResult, stopAtFailure, errorMessage);
            return executionResult.Result;
        }

        public TResult ExecutePutRequest<TRequest, TResult, TError>(string url, TRequest request, bool stopAtFailure = false, string errorMessage = null)
            where TResult : class
            where TError : class, IErrorDetails
        {
            var executionResult = HttpClient.ExecutePutRequest<TResult, TError>(url, request);
            SetLastResult(executionResult, stopAtFailure, errorMessage);
            return executionResult.Result;
        }

        public TResult ExecutePutRequest<TResult, TError>(string url, StringContent request, bool stopAtFailure = false, string errorMessage = null)
            where TResult : class
            where TError : class, IErrorDetails
        {
            var executionResult = HttpClient.ExecutePutRequest<TResult, TError>(url, request);
            SetLastResult(executionResult, stopAtFailure, errorMessage);
            return executionResult.Result;
        }

        public TResult ExecuteJsonSerializedRequest<TRequest, TResult, TError>(string url, TRequest request, bool stopAtFailure = false, string errorMessage = null)
            where TResult : class
            where TError : class, IErrorDetails
        {
            var executionResult = HttpClient.ExecuteJsonSerializedRequest<TResult, TError>(url, request);
            SetLastResult(executionResult, stopAtFailure, errorMessage);
            return executionResult.Result;
        }

        #endregion

        #region PrivateMethods

        public void SetLastResult<TResult, TError>(RequestResult<TResult, TError> executionResult, bool stopAtFailure = false, string errorMessage = null)
            where TResult : class
            where TError : class
        {
            if (stopAtFailure)
            {
                Assert.That(!executionResult.HasError, errorMessage);
            }

            LastError = GetErrorInfo(executionResult);

            LastResult = executionResult.Result;
            ContentType = executionResult.ContentType;
            StatusCode = executionResult.StatusCode;
            Headers = executionResult.Headers;
        }

        private ErrorInfo GetErrorInfo<TResult, TError>(RequestResult<TResult, TError> executionResult)
            where TResult : class
            where TError : class
        {
            if (!executionResult.HasError)
            {
                return null;
            }
            else
            {
                return new ErrorInfo(executionResult.ServerErrorText, executionResult.StatusCode);
            }
        }

        private string ResolveQueryParameters(string url, IDictionary<string, object> queryParameters)
        {
            if (queryParameters == null)
            {
                return url;
            }

            if (!url.EndsWith("?"))
            {
                url += "?";
            }

            var nameValues = HttpUtility.ParseQueryString(url);

            foreach (var kv in queryParameters)
            {
                var value = Convert.ToString(kv.Value);

                if (!string.IsNullOrEmpty(value))
                {
                    nameValues.Set(kv.Key, DoubleUrlEncode(value));
                }
            }

            return HttpUtility.UrlDecode(nameValues.ToString());
        }

        private string DoubleUrlEncode(string value)
        {
            return HttpUtility.UrlEncode(HttpUtility.UrlEncode(value));
        }
        #endregion
    }
}
