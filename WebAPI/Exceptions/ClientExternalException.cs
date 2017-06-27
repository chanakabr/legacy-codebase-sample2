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

        public WebAPI.Exceptions.ApiException.ApiExceptionType ApiExceptionType { get; set; }

        public ClientExternalException(WebAPI.Exceptions.ApiException.ApiExceptionType apiExceptionType, int code = 1, string message = null, int externalCode = 1, string externalMessage = null)
            : base(code, message)
        {
            this.ApiExceptionType = apiExceptionType;
            this.ExternalCode = externalCode;
            this.ExternalMessage = externalMessage;            
        }

    }
}