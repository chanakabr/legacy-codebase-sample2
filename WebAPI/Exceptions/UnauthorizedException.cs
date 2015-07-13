using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Exceptions
{
    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException() : base(HttpStatusCode.OK, (int)StatusCode.Unauthorized, "unauthorized") { }

        public UnauthorizedException(int code, string msg) : base(HttpStatusCode.OK, code, !string.IsNullOrEmpty(msg) ? msg : "unauthorized") { }
    }
}