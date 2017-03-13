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
    [AttributeUsage(AttributeTargets.Property)]
    public class DeprecatedAttribute : SchemeInputAttribute
    {
        public DeprecatedAttribute(string sinceVersion)
        {
            SinceVersion = sinceVersion;
        }

        public string SinceVersion { get; set; }

        public static bool IsDeprecated(string version)
        {
            if (HttpContext.Current.Items[RequestParser.REQUEST_VERSION] == null)
                return false;

            Version deprecationVersion = new Version(version);
            Version current = (Version)HttpContext.Current.Items[RequestParser.REQUEST_VERSION];

            return current.CompareTo(deprecationVersion) >= 0;
        }
    }
}