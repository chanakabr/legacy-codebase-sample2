using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace RestfulTVPApi
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            TCMClient.Settings.Instance.Init();

            //string catalogTcmConfigurationKey = "WebServices.CatalogService";
            //Tvinci.Data.Loaders.CatalogRequestManager.EndPointAddress = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}.{1}", catalogTcmConfigurationKey, "URL"));
            //Tvinci.Data.Loaders.CatalogRequestManager.SignatureKey = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}.{1}", catalogTcmConfigurationKey, "SignatureKey"));

            AppHost.Start();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                HttpContext.Current.Response.End();
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}