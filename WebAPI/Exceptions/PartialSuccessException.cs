using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Exceptions
{
    public class PartialSuccessException : ApiException
    {
        public PartialSuccessException() : base(HttpStatusCode.OK, (int)StatusCode.Error, "error") { }

        public PartialSuccessException(int code, string msg) : base(HttpStatusCode.OK, code, !string.IsNullOrEmpty(msg) ? msg : "error") { }
    }
}