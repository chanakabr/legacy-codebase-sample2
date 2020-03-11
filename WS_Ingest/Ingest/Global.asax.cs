using ConfigurationManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace Ingest
{
    public class Global : System.Web.HttpApplication
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected void Application_Start(object sender, EventArgs e)
        {
            string apiVersion = System.Configuration.ConfigurationManager.AppSettings.Get("apiVersion");
            string logDir = System.Environment.GetEnvironmentVariable("INGEST_LOG_DIR");
            if (logDir != null)
            {
                logDir = System.Environment.ExpandEnvironmentVariables(logDir) + "\\" + apiVersion;
            }
            else
            {
                logDir = "c:\\log\\" + apiVersion + "\\ws_ingest";
            }
            log4net.GlobalContext.Properties["LogDir"] = logDir;
            
            // init TCM
            ApplicationConfiguration.Initialize(true);

            // set monitor and log configuration files
            KMonitor.Configure("log4net.config", KLogEnums.AppType.WCF);
            KLogger.Configure("log4net.config", KLogEnums.AppType.WCF);

        }
    }
}