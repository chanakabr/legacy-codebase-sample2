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
        public enum eRole { Admin, User }
        public eRole Role { get; set; }
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            eRole role;
            if (!Enum.TryParse(HttpContext.Current.Request.QueryString["ks"], false, out role) || Role != role)
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                response.ReasonPhrase = "Forbidden";
                response.Content = new StringContent(StatusCode.Forbidden.ToString());
                actionContext.Response = response;
            }
        }
    }
}
