using System.Net;
using System.Net.Http.Headers;

namespace Core.Api.Utils
{
    public class RequestResult<TResult, TError> : ExecutionResult<TResult, TError>, IRequestResult
        where TResult : class
        where TError : class
    {
        public RequestResult(TResult result, TError error, string serverErrorText, HttpStatusCode statusCode, HttpResponseHeaders headers, MediaTypeHeaderValue contentType = null)
            : base(result, error)
        {
            ServerErrorText = serverErrorText;
            StatusCode = statusCode;
            ContentType = contentType;
            Headers = headers;
        }

        public string ServerErrorText { get; private set; }

        public HttpStatusCode StatusCode { get; private set; }

        public MediaTypeHeaderValue ContentType { get; private set; }

        public HttpResponseHeaders Headers { get; private set; }

    }
}
