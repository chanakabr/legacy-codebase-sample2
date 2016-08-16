using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScheduledTasks
{
    public class NotificationCleanupResponse
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public NotificationCleanupResponse() { }

        //public NotificationCleanupResponse(DateTime lastRunDate, double nextRunIntervalInSeconds) : base (lastRunDate, 0, nextRunIntervalInSeconds, ApiObjects.ScheduledTaskType.
        //{

        //}
    }
}
