using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Routing;
using KLogMonitor;
using KlogMonitorHelper;
using WebAPI.App_Start;
using WebAPI.Exceptions;
using WebAPI.Filters;

namespace WebAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected void Application_Start()
        {
            TCMClient.Settings.Instance.Init();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AutoMapperConfig.RegisterMappings();
            EventNotificationsConfig.SubscribeConsumers();

            // build log4net partial file name
            string partialLogName = string.Empty;

            // try to get application name from virtual path
            string ApplicationAlias = HostingEnvironment.ApplicationVirtualPath;
            if (ApplicationAlias.Length > 2)
                partialLogName = ApplicationAlias.Substring(1);
            else
            {
                // try to get application name from application ID
                string applicationID = HostingEnvironment.ApplicationID;
                if (!string.IsNullOrEmpty(applicationID))
                {
                    var appIdArr = applicationID.Split('/');
                    if (appIdArr != null && appIdArr.Length > 0)
                        partialLogName = appIdArr[appIdArr.Length - 1];
                }
            }

            if (string.IsNullOrWhiteSpace(partialLogName))
            {
                // error getting application name - invent a log name
                partialLogName = Guid.NewGuid().ToString();
            }

            log4net.GlobalContext.Properties["LogName"] = partialLogName;

            // set monitor and log configuration files
            KMonitor.Configure("log4net.config", KLogEnums.AppType.WS);
            KLogger.Configure("log4net.config", KLogEnums.AppType.WS);
        }

        protected void Application_BeginRequest()
        {
            if (!Request.AppRelativeCurrentExecutionFilePath.ToLower().Contains("/api_v"))
            {
                // initialize monitor and logs parameters
                string requestString = MonitorLogsHelper.GetWebServiceRequestString();
                if (!string.IsNullOrEmpty(requestString) && requestString.ToLower().Contains("<soap"))
                {
                    // soap request
                    MonitorLogsHelper.InitMonitorLogsDataWS(ApiObjects.eWSModules.USERS, requestString);
                }
            }
            else
            {
                // get host IP
                if (HttpContext.Current.Request.UserHostAddress != null)
                    HttpContext.Current.Items[Constants.HOST_IP] = HttpContext.Current.Request.UserHostAddress;

            }
        }
    }
}
