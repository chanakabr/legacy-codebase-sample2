using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ThrowsAttribute : Attribute
    {
        public StatusCode? ApiCode { get; set; }
        public eResponseStatus? ClientCode { get; set; }

        public ThrowsAttribute(StatusCode code)
            : base()
        {
            ApiCode = code;
        }

        public ThrowsAttribute(eResponseStatus code)
            : base()
        {
            ClientCode = code;
        }
    }
}