using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WebAPI.Models;
using WebAPI.Models.General;

namespace WebAPI.Exceptions
{
    public class InternalServerErrorException : ApiException
    {
        public InternalServerErrorException() : base(HttpStatusCode.InternalServerError, (int)StatusCode.Error, "error") { }

        public InternalServerErrorException(int code, string msg) : base(HttpStatusCode.InternalServerError, code, !string.IsNullOrEmpty(msg) ? msg : "error") { }
    }
}
