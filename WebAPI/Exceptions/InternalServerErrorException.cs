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
        public static ApiExceptionType INTERNAL_SERVER_ERROR = new ApiExceptionType(StatusCode.Error, "error");

        public static ApiExceptionType MISSING_CONFIGURATION = new ApiExceptionType(StatusCode.MissingConfiguration, "Missing configuration [@configuration@]", "configuration");

        public InternalServerErrorException()
            : this(INTERNAL_SERVER_ERROR)
        {
        }

        public InternalServerErrorException(ApiExceptionType type, params object[] parameters)
            : base(type, parameters)
        {
        }
    }
}
