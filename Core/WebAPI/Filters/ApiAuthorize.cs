using System;
using WebAPI.Managers;

namespace WebAPI.Controllers
{
    public class ApiAuthorizeAttribute : Attribute
    {
        [Flags]
        public enum eRole { /* Placeholder */ }

        public eRole Role { get; set; }
        public eKSValidation KSValidation { get; set; }
        
        public ApiAuthorizeAttribute(eKSValidation validationState = eKSValidation.All)
            : base()
        {
            KSValidation = validationState;
        }

        public bool IsAuthorized(string service, string action)
        {
            RolesManager.ValidateActionPermitted(service, action, KSValidation);
            return true;
        }
    }
}
