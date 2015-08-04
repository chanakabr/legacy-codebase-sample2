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
using Couchbase.Extensions;
using WebAPI.Models.General;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;

namespace WebAPI.Controllers
{
    internal class ApiAuthorizeAttribute : System.Web.Http.AuthorizeAttribute
    {
        [Flags]
        public enum eRole { /* Placeholder */ }
        public eRole Role { get; set; }
        private bool allowAnonymous;

        public ApiAuthorizeAttribute(bool AllowAnonymous = false)
            : base()
        {
            allowAnonymous = AllowAnonymous;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            KS ks = KS.GetFromRequest();

            if (ks == null && !allowAnonymous)
            {
                throw new UnauthorizedException((int)StatusCode.ServiceForbidden, "Service Forbidden");
            }
            else if (allowAnonymous)
            {
                base.OnAuthorization(actionContext);
                return;
            }
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (allowAnonymous)
                return true;

            KS ks = KS.GetFromRequest();
            return ks != null && ks.IsValid;
        }
    }
}
