using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI
{
    public class WsModuleHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string applicationPath = context.Request.ApplicationPath.ToLower();
            if (applicationPath.Contains("cas"))
            {
                context.Server.TransferRequest("~/ws_cas_module.asmx", true);
            }
            else if (applicationPath.Contains("billing"))
            {
                context.Server.TransferRequest("~/ws_billing_module.asmx", true);
            }
            else if (applicationPath.Contains("domain"))
            {
                context.Server.TransferRequest("~/ws_domains_module.asmx", true);
            }
            else if (applicationPath.Contains("pricing"))
            {
                context.Server.TransferRequest("~/ws_pricing_module.asmx", true);
            }
            else if (applicationPath.Contains("social"))
            {
                context.Server.TransferRequest("~/ws_social_module.asmx", true);
            }
            else if (applicationPath.Contains("users"))
            {
                context.Server.TransferRequest("~/ws_users_module.asmx", true);
            }
            else
            {
                context.Response.StatusCode = 404;
                context.Response.End();
            }

            context.Response.End();
        }
    }
}