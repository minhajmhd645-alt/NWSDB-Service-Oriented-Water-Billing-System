using System;
using System.Net;

namespace NWSDB.API.Exceptions
{
    /// <summary>
    /// Custom exception representing downstream Bank.API communication failures or business rejections.
    /// Carries the HttpStatusCode returned by Bank.API (or HttpStatusCode.BadGateway for network errors).
    /// </summary>
    public class BankApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public BankApiException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public BankApiException(string message, Exception innerException, HttpStatusCode statusCode = HttpStatusCode.BadGateway)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
