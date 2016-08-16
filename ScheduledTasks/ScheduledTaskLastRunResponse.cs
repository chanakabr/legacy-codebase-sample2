using ApiObjects;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScheduledTasks
{
    public abstract class ScheduledTaskLastRunResponse
    {        
        // Type to be filtered on views of CB
        public const int Type = 1000;
        public ApiObjects.Response.Status Status { get; set; }
        public DateTime LastRunDate { get; set; }
        public int ImpactedItemsOnLastRun { get; set; }
        public double NextRunIntervalInSeconds { get; set; }
        public ScheduledTaskType ScheduledTaskType { get; set; }
        protected const int RETRY_LIMIT = 5;

        public ScheduledTaskLastRunResponse() { }

        public ScheduledTaskLastRunResponse(DateTime lastRunDate, int impactedItems, double nextRunIntervalInSeconds, ScheduledTaskType scheduledTaskType)
        {
            this.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            this.LastRunDate = lastRunDate;
            this.ImpactedItemsOnLastRun = impactedItems;
            this.NextRunIntervalInSeconds = nextRunIntervalInSeconds;
            this.ScheduledTaskType = scheduledTaskType;
        }

        public abstract object GetLastRunDetails();

        public abstract string GetKey();

        public abstract bool SetLastRunDetails();

        public abstract bool SetNextRunIntervalInSeconds(double updatedNextRunIntervalInSeconds);
    }
}
