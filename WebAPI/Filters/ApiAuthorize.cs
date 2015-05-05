using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Models;
using Couchbase.Extensions;

namespace WebAPI.Controllers
{
    internal class ApiAuthorizeAttribute : System.Web.Http.AuthorizeAttribute
    {
        [Flags]
        public enum eRole { Admin, User }
        public eRole Role { get; set; }
        private KS ks;

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var a= CouchbaseManager.GetInstance(CouchbaseBucket.Groups).StoreJson(Enyim.Caching.Memcached.StoreMode.Set, "oded", new Group() { AdminSecret = "fasdfasdfasdf" });
            var gro = CouchbaseManager.GetInstance(CouchbaseBucket.Groups).GetJson<Group>("oded");

            string ksVal = HttpContext.Current.Request.QueryString["ks"];
            //TODO: Change from checking emptiness to real KS structure / expiration
            if (string.IsNullOrEmpty(ksVal) || !(ks = KS.CreateKSFromEncoded(ksVal)).IsValid)
            {
                HttpResponseMessage res = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                actionContext.Response = res;
                res.Content = new StringContent(((int)StatusCode.Unauthorized).ToString());
                res.ReasonPhrase = "Unauthorized";
                return;
            }

            actionContext.Request.Properties.Add("KS", ks);

            base.OnAuthorization(actionContext);
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            return true;

            eRole role;
            //TODO: use the private KS above. when KS is completed, change this to extract from the KS object
            if (!Enum.TryParse(HttpContext.Current.Request.QueryString["ks"], false, out role) || ((Role & role) != role))
            {
                return false;
            }

            return true;
        }
    }
}
