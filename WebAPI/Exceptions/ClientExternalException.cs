using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Exceptions
{
    public class ClientExternalException : ClientException
    {
        public int ExternalCode { get; set; }

        public string ExternalMessage { get; set; }

        public WebAPI.Exceptions.ApiException.ClientExternalExceptionType ClientExceptionType { get; set; }

        public ClientExternalException(WebAPI.Exceptions.ApiException.ClientExternalExceptionType clientExceptionType, int code = 1, string message = null, int externalCode = 1, string externalMessage = null)
            : base(code, message)
        {
            this.ClientExceptionType = clientExceptionType;
            this.ExternalCode = externalCode;
            this.ExternalMessage = externalMessage;            
        }

    }
}