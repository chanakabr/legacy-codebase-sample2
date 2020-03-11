using System;
using System.Net;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class FailureHttpCodeAttribute : Attribute
    {
        public HttpStatusCode HttpStatusCode { get; set; }

        public FailureHttpCodeAttribute()
            : base()
        {
            HttpStatusCode = HttpStatusCode.InternalServerError;
        }

        public FailureHttpCodeAttribute(HttpStatusCode httpStatusCode)
            : base()
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}