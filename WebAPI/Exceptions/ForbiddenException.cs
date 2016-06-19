using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI.Exceptions
{
    public class ForbiddenException : ApiException
    {
        public ForbiddenException() : base((int)StatusCode.ServiceForbidden, "forbidden") { }

        public ForbiddenException(int code, string msg) : base(code, !string.IsNullOrEmpty(msg) ? msg : "forbidden") { }
    }
}