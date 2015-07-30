using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;

namespace WebAPI.Filters
{
    public class KSExtractor : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            string ksVal = HttpContext.Current.Request.QueryString["ks"];

            if (ksVal == null)
            {
                base.OnActionExecuting(actionContext);
                return;
            }

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
            KS ks = KS.CreateKSFromEncoded(encryptedData, groupId, adminSecret);

            if (!ks.IsValid)
            {
                throw new UnauthorizedException((int)StatusCode.InvalidKS, "KS expired");
            }

            ks.SaveOnRequest();

            base.OnActionExecuting(actionContext);
        }
    }
}