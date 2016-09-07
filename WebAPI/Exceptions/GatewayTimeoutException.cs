using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI.Utils
{
    public class GatewayTimeoutException : ApiException
    {
        public GatewayTimeoutException(ApiExceptionType type, params string[] parameters)
            : base(type, parameters)
        {
        }
    }
}
