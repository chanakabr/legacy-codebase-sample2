using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Models;
using WebAPI.Models.General;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers;
using WebAPI.Filters;

namespace WebAPI.Controllers
{
    internal class ApiAuthorizeAttribute : Attribute
    {
        [Flags]
        public enum eRole { /* Placeholder */ }
        public eRole Role { get; set; }
        
        private bool silent;
        
        public ApiAuthorizeAttribute(bool Silent = false)
            : base()
        {
            silent = Silent;
        }

        public bool IsAuthorized(string service, string action)
        {
            RolesManager.ValidateActionPermitted(service, action, silent);
            return true;
        }
    }
}
