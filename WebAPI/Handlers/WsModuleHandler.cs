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
            if (context.Request.ApplicationPath.ToLower().Contains("cas"))
            {
                context.Server.TransferRequest("~/ws_cas_module.asmx");
            }
            else if (context.Request.ApplicationPath.ToLower().Contains("billing"))
            {
                context.Server.TransferRequest("~/ws_billing_module.asmx");
            }
            else if (context.Request.ApplicationPath.ToLower().Contains("domain"))
            {
                context.Server.TransferRequest("~/ws_domains_module.asmx");
            }
            else if (context.Request.ApplicationPath.ToLower().Contains("pricing"))
            {
                context.Server.TransferRequest("~/ws_pricing_module.asmx");
            }
            else if (context.Request.ApplicationPath.ToLower().Contains("social"))
            {
                context.Server.TransferRequest("~/ws_social_module.asmx");
            }
            else if (context.Request.ApplicationPath.ToLower().Contains("users"))
            {
                context.Server.TransferRequest("~/ws_users_module.asmx");
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