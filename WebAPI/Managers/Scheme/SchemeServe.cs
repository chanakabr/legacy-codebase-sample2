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
    [AttributeUsage(AttributeTargets.Method)]
    public class SchemeServeAttribute : Attribute
    {
        public string ContentType { get; set; }

        public SchemeServeAttribute()
            : base()
        {
            ContentType = "application/json";
        }
    }
}