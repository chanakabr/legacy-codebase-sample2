using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Exceptions
{
    public class ClientException : Exception
    {
        public int Code { get; set; }

        public string ExceptionMessage { get; set; }

        public List<KeyValuePair> Args { get; set; }

        public ClientException(int code = 1, string message = null, List<KeyValuePair> args = null)
            : base()
        {
            this.Code = code;
            this.ExceptionMessage = message;
            this.Args = args;
        }
    }
}