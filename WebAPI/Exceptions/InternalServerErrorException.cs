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
    public class InternalServerErrorException : ApiException
    {
        public InternalServerErrorException() : base((int)StatusCode.Error, "error") { }

        public InternalServerErrorException(int code, string msg) : base(code, !string.IsNullOrEmpty(msg) ? msg : "error") { }
    }
}
