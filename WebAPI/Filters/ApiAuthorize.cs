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
        public enum eRole { /* Placeholder */ }
        public eRole Role { get; set; }
        private KS ks;

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            string ksVal = HttpContext.Current.Request.QueryString["ks"];
            if (string.IsNullOrEmpty(ksVal))
            {
                ReturnUnauthorized(actionContext);
                return;
            }

            StringBuilder sb = new StringBuilder(ksVal);
            sb = sb.Replace("-", "+");
            sb = sb.Replace("_", "/");
            byte[] encryptedData = System.Convert.FromBase64String(sb.ToString());

            string encryptedDataStr = System.Text.Encoding.ASCII.GetString(encryptedData);

            string[] ksParts = encryptedDataStr.Split('|');

            int groupId = 0;
            if (ksParts.Length != 3 || ksParts[0] != "v2" || !int.TryParse(ksParts[1], out groupId))
            {
                ReturnUnauthorized(actionContext);
                return;
            }

            // get group secret
            Group group = GroupsManager.GetGroup(groupId);
            string adminSecret = group.AdminSecret;

            // build KS
            try
            {
                ks = KS.CreateKSFromEncoded(encryptedData, groupId, adminSecret);
            }
            catch (Exception)
            {
                ReturnUnauthorized(actionContext);
                return;
            }

            if (ks == null)
            {
                ReturnUnauthorized(actionContext);
                return;
            }

            if (!ks.IsValid)
            {
                ReturnForbidden(actionContext);
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

        private void ReturnUnauthorized(HttpActionContext actionContext)
        {
            HttpResponseMessage res = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            actionContext.Response = res;
            res.Content = new StringContent(((int)StatusCode.Unauthorized).ToString());
            res.ReasonPhrase = "Unauthorized";
        }

        private void ReturnForbidden(HttpActionContext actionContext)
        {
            HttpResponseMessage res = new HttpResponseMessage(HttpStatusCode.Forbidden);
            actionContext.Response = res;
            res.Content = new StringContent(((int)StatusCode.Forbidden).ToString());
            res.ReasonPhrase = "Forbidden";
        }
    }
}
