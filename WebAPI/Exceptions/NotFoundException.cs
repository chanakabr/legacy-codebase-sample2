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
        public static ApiExceptionType OBJECT_NOT_FOUND = new ApiExceptionType(StatusCode.NotFound, "@objectType@ not found", "objectType");
        public static ApiExceptionType OBJECT_ID_NOT_FOUND = new ApiExceptionType(StatusCode.NotFound, "@objectType@ id [@id@] not found", "objectType", "id");

        public NotFoundException(ApiExceptionType type, params string[] parameters)
            : base(type, parameters)
        {
        }
    }
}
