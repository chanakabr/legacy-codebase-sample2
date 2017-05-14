using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Web;
using KLogMonitor;

namespace KlogMonitorHelper
{
    public class ContextData
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private OperationContext wcfContext = null;
        private HttpContext wsContext = null;
        private string defaultLog4netConfigFile = "log4net.config";

        public ContextData()
        {
            try
            {
                switch (KMonitor.AppType)
                {
                    case KLogEnums.AppType.WCF:

                        if (OperationContext.Current != null)
                            this.wcfContext = OperationContext.Current;
                        break;

                    case KLogEnums.AppType.WS:
                    default:

                        if (HttpContext.Current != null)
                            this.wsContext = HttpContext.Current;
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to copy context data to a new thread", ex);
            }
        }

        public void Load(string log4netConfigFile = null)
        {
            if (log4netConfigFile == null)
                log4netConfigFile = defaultLog4netConfigFile;

            try
            {
                switch (KMonitor.AppType)
                {

                    case KLogEnums.AppType.WCF:

                        // set log configuration files
                        KLogger.Configure(log4netConfigFile, KLogEnums.AppType.WCF);
                        KMonitor.Configure(log4netConfigFile, KLogEnums.AppType.WCF);


                        if (this.wcfContext != null)
                            OperationContext.Current = this.wcfContext;
                        break;

                    case KLogEnums.AppType.WS:
                    default:

                        // set log configuration files
                        KLogger.Configure(log4netConfigFile, KLogEnums.AppType.WS);
                        KMonitor.Configure(log4netConfigFile, KLogEnums.AppType.WS);

                        if (this.wsContext != null)
                            HttpContext.Current = this.wsContext;
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to load context data to a new thread", ex);
            }
        }
    }
}
