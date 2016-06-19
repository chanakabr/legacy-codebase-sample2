using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI.Exceptions
{
    public class PartialSuccessException : ApiException
    {
        public PartialSuccessException() : base((int)StatusCode.Error, "error") { }

        public PartialSuccessException(int code, string msg) : base(code, !string.IsNullOrEmpty(msg) ? msg : "error") { }
    }
}