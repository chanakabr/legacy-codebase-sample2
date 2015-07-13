using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Utils
{
    public class GatewayTimeoutException : ApiException
    {
        public GatewayTimeoutException() : base(HttpStatusCode.InternalServerError, (int)StatusCode.Timeout, "timeout") { }

        public GatewayTimeoutException(int code, string msg) : base(HttpStatusCode.InternalServerError, code, !string.IsNullOrEmpty(msg) ? msg : "timeout") { }
    }
}
