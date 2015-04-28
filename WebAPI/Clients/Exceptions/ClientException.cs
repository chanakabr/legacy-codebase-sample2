using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Clients.Exceptions
{
    public class ClientException : Exception
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public ClientException(int code = 1, string message = null)
            : base()
        {
            Code = code;
            Message = message;
        }
    }
}