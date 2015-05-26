using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WebAPI.Models;

namespace WebAPI.Exceptions
{
    public class BadRequestException : ApiException
    {
        public BadRequestException() : base(HttpStatusCode.BadRequest, (int)Models.StatusCode.BadRequest, "bad request") { }

        public BadRequestException(int code, string msg) : base(HttpStatusCode.BadRequest, code, msg) { }
    }
}
