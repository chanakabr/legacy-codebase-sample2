using ApiObjects;
using System;
using System.Collections.Generic;

namespace WebAPI.Exceptions
{
    public class ClientException : Exception
    {
        public int Code { get; set; }

        public string ExceptionMessage { get; set; }

        public List<ApiObjects.KeyValuePair> Args { get; set; }

        public ClientException(int code = 1, string message = null, List<ApiObjects.KeyValuePair> args = null)
            : base()
        {
            this.Code = code;
            this.ExceptionMessage = message;
            this.Args = args;
        }
    }
}