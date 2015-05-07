using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace WebAPI.Filters.Exceptions
{
    public class ForbiddenException : ApiException
    {
                public ForbiddenException() : base(HttpStatusCode.Forbidden, (int)Models.StatusCode.Forbidden, "Forbidden") { }

                public ForbiddenException(int code, string msg) : base(HttpStatusCode.Forbidden, code, msg) { }
    }
}