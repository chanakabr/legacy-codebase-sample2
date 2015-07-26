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
        private KS ks;
        private bool allowAnonymous;

        public ApiAuthorizeAttribute(bool AllowAnonymous = false)
            : base()
        {
            allowAnonymous = AllowAnonymous;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            string ksVal = HttpContext.Current.Request.QueryString["ks"];

            if (string.IsNullOrEmpty(ksVal) && !allowAnonymous)
            {
                throw new UnauthorizedException((int)StatusCode.ServiceForbidden, "Service Forbidden");
            }
            else if (allowAnonymous)
            {
                base.OnAuthorization(actionContext);
                return;
            }
            else
            {
                StringBuilder sb = new StringBuilder(ksVal);
                sb = sb.Replace("-", "+");
                sb = sb.Replace("_", "/");

                int groupId = 0;
                byte[] encryptedData = null;
                string encryptedDataStr = null;
                string[] ksParts = null;

                try
                {
                    encryptedData = System.Convert.FromBase64String(sb.ToString());
                    encryptedDataStr = System.Text.Encoding.ASCII.GetString(encryptedData);
                    ksParts = encryptedDataStr.Split('|');
                }
                catch (Exception ex)
                {
                    throw new UnauthorizedException((int)StatusCode.InvalidKS, "Wrong KS format");
                }

                if (ksParts.Length != 3 || ksParts[0] != "v2" || !int.TryParse(ksParts[1], out groupId))
                {
                    throw new UnauthorizedException((int)StatusCode.InvalidKS, "Wrong KS format");
                }

                // get group secret
                Group group = GroupsManager.GetGroup(groupId);
                string adminSecret = group.UserSecret;

                // build KS
                ks = KS.CreateKSFromEncoded(encryptedData, groupId, adminSecret);

                if (!ks.IsValid)
                {
                    throw new UnauthorizedException((int)StatusCode.InvalidKS, "KS expired");
                }

                ks.SaveOnRequest();

                base.OnAuthorization(actionContext);
            }
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (allowAnonymous)
                return true;

            return ks.IsValid;

            //eRole role;
            ////TODO: use the private KS above. when KS is completed, change this to extract from the KS object
            //if (!Enum.TryParse(HttpContext.Current.Request.QueryString["ks"], false, out role) || ((Role & role) != role))
            //{
            //    return false;
            //}

            //return true;
        }
    }
}
