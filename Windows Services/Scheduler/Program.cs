using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using KLogMonitor;

namespace Scheduler
{
    static class Program
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            try
            {
                // set monitor and log configuration files
                KMonitor.Configure("log4net.config", KLogEnums.AppType.WCF);
                KLogger.Configure("log4net.config", KLogEnums.AppType.WCF);

                TCMClient.Settings.Instance.Init();
            }
            catch (Exception ex)
            {
                log.Error("Scheduler Main", ex);
            }
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new TVM_Tasker()
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}
