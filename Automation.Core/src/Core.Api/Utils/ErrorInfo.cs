using System.Net;

namespace Core.Api.Utils
{
    public class ErrorInfo
    {
        internal ErrorInfo(string message, HttpStatusCode statusCode)
            : this(message, null, statusCode)
        {
        }

        internal ErrorInfo(string message, int[] errorCodes, HttpStatusCode statusCode)
        {
            Message = message;
            ErrorCodes = errorCodes;
            StatusCode = statusCode;
        }

        internal ErrorInfo(string message, int[] errorCodes, HttpStatusCode statusCode, string[] data)
        {
            Message = message;
            ErrorCodes = errorCodes;
            StatusCode = statusCode;
            Data = data;
        }

        internal string Message { get; private set; }

        internal int[] ErrorCodes { get; private set; }

        internal HttpStatusCode StatusCode { get; private set; }

        internal string[] Data { get; private set; }

        public override string ToString()
        {
            return $@"[{GetType().FullName}] Message: ""{Message}"", ErrorCodes: [{(ErrorCodes == null ? "(none)" : string.Join(", ", ErrorCodes))}], StatusCode: {StatusCode}";
        }
    }
}
