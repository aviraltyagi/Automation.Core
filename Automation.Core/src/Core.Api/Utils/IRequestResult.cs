using System.Net;

namespace Core.Api.Utils
{
    internal interface IRequestResult
    {
        bool HasError
        {
            get;
        }

        string ServerErrorText
        {
            get;
        }

        HttpStatusCode StatusCode
        {
            get;
        }
    }
}
