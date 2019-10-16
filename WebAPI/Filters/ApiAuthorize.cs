using System;
using WebAPI.Managers;

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
