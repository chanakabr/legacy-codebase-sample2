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
        public PartialSuccessException(ApiExceptionType type, params string[] parameters)
            : base(type, parameters)
        {
        }
    }
}