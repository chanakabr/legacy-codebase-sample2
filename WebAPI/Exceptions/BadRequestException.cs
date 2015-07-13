using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WebAPI.Models;
using WebAPI.Models.General;

namespace WebAPI.Exceptions
{
    public class BadRequestException : ApiException
    {
        public BadRequestException() : base(HttpStatusCode.OK, (int)StatusCode.BadRequest, "bad request") { }

        public BadRequestException(int code, string msg) : base(HttpStatusCode.OK, code, !string.IsNullOrEmpty(msg) ? msg : "bad request") { }
    }
}
