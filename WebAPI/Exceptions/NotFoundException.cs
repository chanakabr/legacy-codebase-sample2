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
    public class NotFoundException : ApiException
    {
        public NotFoundException() : base((int)StatusCode.NotFound, "not found") { }

        public NotFoundException(int code, string msg) : base(code, !string.IsNullOrEmpty(msg) ? msg : "not found") { }
    }
}
