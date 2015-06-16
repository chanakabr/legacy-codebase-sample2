using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WebAPI.Models;
using WebAPI.Models.General;

namespace WebAPI.Exceptions
{
    public class NotFoundException : ApiException
    {
        public NotFoundException() : base(HttpStatusCode.NotFound, (int)StatusCode.NotFound, "Not Found") { }

        public NotFoundException(int code, string msg) : base(HttpStatusCode.NotFound, code, msg) { }
    }
}
