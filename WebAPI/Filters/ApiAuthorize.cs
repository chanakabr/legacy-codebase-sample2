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
    public class ApiAuthorizeAttribute : Attribute
    {
        [Flags]
        public enum eRole { /* Placeholder */ }
        public eRole Role { get; set; }
        public bool Silent { get; set; }
        
        public ApiAuthorizeAttribute(bool silent = false)
            : base()
        {
            Silent = silent;
        }

        public bool IsAuthorized(string service, string action)
        {
            RolesManager.ValidateActionPermitted(service, action, Silent);
            return true;
        }
    }
}
