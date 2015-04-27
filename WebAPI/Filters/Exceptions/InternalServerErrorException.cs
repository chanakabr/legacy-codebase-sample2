using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WebAPI.Filters.Exceptions;
using WebAPI.Models;

namespace WebAPI.Filters
{
    public class InternalServerErrorException : ApiException
    {
        public InternalServerErrorException() : base(HttpStatusCode.InternalServerError, (int)Models.StatusCode.Error, "error") { }

        public InternalServerErrorException(int code, string msg) : base(HttpStatusCode.InternalServerError, code, msg) { }
    }
}
