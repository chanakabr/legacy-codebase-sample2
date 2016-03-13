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
            // init TCM
            TCMClient.Settings.Instance.Init();

            // set monitor and log configuration files
            KMonitor.Configure("log4net.config", KLogEnums.AppType.WCF);
            KLogger.Configure("log4net.config", KLogEnums.AppType.WCF);

        }
    }
}