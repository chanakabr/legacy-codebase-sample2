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
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class RuntimeSchemeArgumentAttribute : SchemeArgumentAttribute
    {
        private string Service;
        private string Action;

        public RuntimeSchemeArgumentAttribute(string name, string service, string action) : base(name)
        {
            Service = service;
            Action = action;
        }

        internal void Validate(object value)
        {
            if (value == null)
            {
                return;
            }

            string name = string.Format("{0}.{1}.{2}", Service, Action, Name);

            base.Validate(name, value);

            if (RequiresPermission)
            {
                RolesManager.ValidateArgumentPermitted(Service, Action, Name);
            }
        }   
    }
}