using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI.Exceptions
{
    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException() : base((int)StatusCode.Unauthorized, "unauthorized") { }

        public UnauthorizedException(int code, string msg) : base(code, !string.IsNullOrEmpty(msg) ? msg : "unauthorized") { }
    }
}