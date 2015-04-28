using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    internal class ApiAuthorizeAttribute : System.Web.Http.AuthorizeAttribute
    {
        [Flags]
        public enum eRole { Admin, User }
        public eRole Role { get; set; }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            //TODO: Change from checking emptiness to real KS structure / expiration
            if (string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["ks"]))
            {
                HttpResponseMessage res = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                actionContext.Response = res;
                res.Content = new StringContent(((int)StatusCode.Unauthorized).ToString());
                res.ReasonPhrase = "Unauthorized";
                return;
            }

            base.OnAuthorization(actionContext);
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            eRole role;
            //TODO: when KS is completed, change this to extract from the KS object
            if (!Enum.TryParse(HttpContext.Current.Request.QueryString["ks"], false, out role) || ((Role & role) != role))
            {
                return false;
            }

            return true;
        }
    }
}
