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
            KMonitor.Configure("log4net.config", KLogEnums.AppType.WS);
            KLogger.Configure("log4net.config", KLogEnums.AppType.WS);

            RouteTable.Routes.Add(new ServiceRoute("", new WebServiceHostFactory(), typeof(Service)));
            TCMClient.Settings.Instance.Init();

            WebAPI.Filters.AutoMapperConfig.RegisterMappings();
            WebAPI.Filters.EventNotificationsConfig.SubscribeConsumers();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // get request ID
            HttpContext.Current.Items[KLogMonitor.Constants.REQUEST_ID_KEY] = Guid.NewGuid().ToString();
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