using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace WebAPI.Filters.Exceptions
{
    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException() : base(HttpStatusCode.Unauthorized, (int)Models.StatusCode.Unauthorized, "Unauthorized") { }

        public UnauthorizedException(int code, string msg) : base(HttpStatusCode.Unauthorized, code, msg) { }
    }
}