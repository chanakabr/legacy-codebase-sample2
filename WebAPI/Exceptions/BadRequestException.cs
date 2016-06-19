using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WebAPI.Managers.Models;
using WebAPI.Models;
using WebAPI.Models.General;

namespace WebAPI.Exceptions
{
    public class BadRequestException : ApiException
    {
        public BadRequestException() : base((int)StatusCode.BadRequest, "bad request") { }

        public BadRequestException(int code, string msg) : base(code, !string.IsNullOrEmpty(msg) ? msg : "bad request") { }
    }
}
