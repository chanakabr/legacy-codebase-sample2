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
    public class SchemeArgumentAttribute : SchemeInputAttribute
    {
        public string Name { get; set; }

        public bool RequiresPermission { get; set; }

        public SchemeArgumentAttribute(string name)
            : base()
        {
            Name = name;
            RequiresPermission = false;
        }

        private static string FirstCharacterToLower(string str)
        {
            if (String.IsNullOrEmpty(str) || Char.IsLower(str, 0))
                return str;

            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        private static string getServiceId(Type controller)
        {
            return FirstCharacterToLower(controller.Name.Replace("Controller", ""));
        }

        internal void Validate(MethodInfo methodInfo, string argument, object value)
        {
            string service = getServiceId(methodInfo.DeclaringType);
            string action = FirstCharacterToLower(methodInfo.Name);
            string name = string.Format("{0}.{1}.{2}", service, action, argument);

            base.Validate(name, value);

            if (RequiresPermission)
            {
                RolesManager.ValidateArgumentPermitted(service, action, argument);
            }
        }
    }
}