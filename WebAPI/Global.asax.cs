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
using ConfigurationManager;

namespace WebAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string FORM_URL_ENCODED = "application/x-www-form-urlencoded";

        protected void Application_Start()
        {
            InitializeLogging();

            // Configuration
            ConfigurationManager.ApplicationConfiguration.Initialize(true, true);

            GlobalConfiguration.Configure(WebApiConfig.Register);
            AutoMapperConfig.RegisterMappings();
            EventNotificationsConfig.SubscribeConsumers();
        }

        private static void InitializeLogging()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string apiVersion = string.Format("{0}_{1}_{2}", fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart);
            string logDir = System.Environment.GetEnvironmentVariable("API_LOG_DIR");
            if(logDir != null)
            {
                logDir = System.Environment.ExpandEnvironmentVariables(logDir);
            }
            else
            {
                logDir = "C:\\log\\RestfulApi";
            }

            log4net.GlobalContext.Properties["LogDir"] = logDir;
            log4net.GlobalContext.Properties["ApiVersion"] = apiVersion;

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
            KMonitor.SetAppType(KLogEnums.AppType.WS);
            KLogger.SetAppType(KLogEnums.AppType.WS);
            if (!Request.AppRelativeCurrentExecutionFilePath.ToLower().Contains("/api_v"))
            {
                // set appType to WCF only if we know its a WCF service
                if (Request.AppRelativeCurrentExecutionFilePath.ToLower().Contains(".svc"))
                {
                    KMonitor.SetAppType(KLogEnums.AppType.WCF);
                    KLogger.SetAppType(KLogEnums.AppType.WCF);
                }

                // initialize monitor and logs parameters
                string requestString = MonitorLogsHelper.GetWebServiceRequestString();
                if (!string.IsNullOrEmpty(requestString))
                {
                    if (requestString.ToLower().Contains("<soap"))
                    {
                        // soap request
                        MonitorLogsHelper.InitMonitorLogsDataWS(ApiObjects.eWSModules.USERS, requestString);
                    }
                    else if (Request.ContentType == FORM_URL_ENCODED)
                    {
                        string action = string.Empty;
                        if (!string.IsNullOrEmpty(Request.PathInfo) && Request.PathInfo.Length > 1)
                        {
                            action = Request.PathInfo.Substring(1);
                        }

                        MonitorLogsHelper.InitMonitorLogsDataFormUrlEncoded(action, requestString);
                    }
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
