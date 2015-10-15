using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace RemoteTasksService
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            // set monitor and log configuration files
            KMonitor.Configure("log4net.config", KLogEnums.AppType.WCF);
            KLogger.Configure("log4net.config", KLogEnums.AppType.WCF);

            RouteTable.Routes.Add(new ServiceRoute("", new WebServiceHostFactory(), typeof(Service)));
            TCMClient.Settings.Instance.Init();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

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