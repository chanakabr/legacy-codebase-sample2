using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVinciShared;

namespace WS_API
{
    public class PermittedModule : IHttpModule
    {
        public PermittedModule()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(Context_BeginRequest);
            context.PreRequestHandlerExecute += new EventHandler(Application_AuthenticateRequest);

        }

        public void Dispose()
        {
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;
            HttpApplication application = sender as HttpApplication;

            string sRefferer = "";
            if (context.Request.UrlReferrer != null)
                sRefferer = context.Request.UrlReferrer.Host;

            if (sRefferer.Trim().ToLower() == "localhost" || sRefferer.Trim().ToLower() == "admin.tvinci.com")
                return;
            if (!application.Request.IsSecureConnection)
            {
                string sIpAddress = context.Request.UserHostAddress;
                if (sIpAddress != "localhost" &&
                    sIpAddress != "127.0.0.1" &&
                    sIpAddress != "62.128.54.165" &&
                    sIpAddress != "62.128.54.168" &&
                    sIpAddress != "admin.tvinci.com" &&
                    sIpAddress != "80.179.194.132")
                {
                    Logger.Logger.Log("Block", "IP: " + sIpAddress + "|| Refferer: " + sRefferer, "Blocker");
                    context.Response.StatusCode = 403;
                }
            }
        }
        
        private void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;
            HttpApplication application = sender as HttpApplication;

            string sRefferer = "";
            if (context.Request.UrlReferrer != null)
                sRefferer = context.Request.UrlReferrer.Host;

            if (sRefferer.Trim().ToLower() == "localhost" || sRefferer.Trim().ToLower() == "admin.tvinci.com")
                return;
            if (!application.Request.IsSecureConnection)
            {
                string sIpAddress = context.Request.UserHostAddress;
                if (sIpAddress != "localhost" &&
                    sIpAddress != "127.0.0.1" &&
                    sIpAddress != "62.128.54.165" &&
                    sIpAddress != "62.128.54.168" &&
                    sIpAddress != "admin.tvinci.com" &&
                    sIpAddress != "80.179.194.132")
                {
                    Logger.Logger.Log("Block", "IP: " + sIpAddress + "|| Refferer: " + sRefferer, "Blocker");
                    context.Response.StatusCode = 403;
                }
            }
        }
    }
}
