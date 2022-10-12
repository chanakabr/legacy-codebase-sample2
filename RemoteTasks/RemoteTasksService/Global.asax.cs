using Phx.Lib.Appconfig;
using Phx.Lib.Log;
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
            string logDir = System.Environment.GetEnvironmentVariable("REMOTE_TASK_LOG_DIR");
            if (logDir != null)
            {
                logDir = System.Environment.ExpandEnvironmentVariables(logDir);
            }
            else
            {
                logDir = "c:\\log\\remote_tasks";
            }
            log4net.GlobalContext.Properties["LogDir"] = logDir;

            // set monitor and log configuration files
            KMonitor.Configure("log4net.config", KLogEnums.AppType.WS);
            KLogger.Configure("log4net.config", KLogEnums.AppType.WS);

            RouteTable.Routes.Add(new ServiceRoute("", new WebServiceHostFactory(), typeof(Service)));

            ApplicationConfiguration.Init();
            
            WebAPI.Filters.AutoMapperConfig.RegisterMappings();
            WebAPI.Filters.EventNotificationsConfig.SubscribeConsumers();

            // This line is here to avoid error while deserilizing json that was serlizied using net core with TypeNameHandling
            TVinciShared.AssemblyUtils.RedirectAssembly("System.Private.CoreLib", "mscorlib");
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // get request ID
            KLogger.SetRequestId(Guid.NewGuid().ToString());
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