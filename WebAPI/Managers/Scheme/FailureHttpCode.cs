using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;

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