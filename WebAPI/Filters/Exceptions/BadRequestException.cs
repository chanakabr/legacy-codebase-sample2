using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WebAPI.Filters.Exceptions;
using WebAPI.Models;

namespace WebAPI.Filters
{
    public class BadRequestException : ApiException
    {
        public BadRequestException() : base(HttpStatusCode.BadRequest, Models.StatusCode.BadRequest, "bad request") { }

        public BadRequestException(StatusCode code, string msg) : base(HttpStatusCode.BadRequest, code, msg) { }
    }
}
