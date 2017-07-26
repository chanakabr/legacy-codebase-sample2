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
    public class SchemeBaseAttribute : Attribute
    {
        public Type BaseType { get; set; }

        public SchemeBaseAttribute(Type baseType)
        {
            BaseType = baseType;
        }
    }
}