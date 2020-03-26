using System;
using System.Collections.Generic;
using WebAPI.Managers.Models;

namespace WebAPI.Exceptions
{
    public class ClientException : Exception
    {
        public int Code { get; set; }
        public string ExceptionMessage { get; set; }
        public List<ApiObjects.KeyValuePair> Args { get; set; }

        public ClientException(ApiObjects.Response.Status responseStatus)
            : base()
        {
            this.Code = responseStatus.Code;
            this.ExceptionMessage = responseStatus.Message;
            this.Args = responseStatus.Args;
        }

        public ClientException(StatusCode statusCode)
            : base()
        {
            this.Code = (int)statusCode;
            this.ExceptionMessage = statusCode.ToString();
            this.Args = null;
        }

        public ClientException(int code = 1, string message = null, List<ApiObjects.KeyValuePair> args = null)
            : base()
        {
            this.Code = code;
            this.ExceptionMessage = message;
            this.Args = args;
        }
    }
}