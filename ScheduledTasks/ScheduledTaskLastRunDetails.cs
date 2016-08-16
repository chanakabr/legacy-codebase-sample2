using ApiObjects;
using ApiObjects.Response;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScheduledTasks
{
    public abstract class ScheduledTaskLastRunDetails
    {        
        // Type to be filtered on views of CB
        public const int Type = 1000;
        public Status Status { get; set; }
        public DateTime LastRunDate { get; set; }
        public DateTime? EstimatedNextRunDate { get; set; }
        public int ImpactedItemsOnLastRun { get; set; }
        public double NextRunIntervalInSeconds { get; set; }
        public ScheduledTaskType ScheduledTaskType { get; set; }
        
        protected const int RETRY_LIMIT = 5;

        public ScheduledTaskLastRunDetails() { }

        public ScheduledTaskLastRunDetails(DateTime lastRunDate, int impactedItems, double nextRunIntervalInSeconds, ScheduledTaskType scheduledTaskType)
        {
            this.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            this.LastRunDate = lastRunDate;
            this.ImpactedItemsOnLastRun = impactedItems;
            this.NextRunIntervalInSeconds = nextRunIntervalInSeconds;
            this.ScheduledTaskType = scheduledTaskType;
            this.EstimatedNextRunDate = null;
        }

        public abstract ScheduledTaskLastRunDetails GetLastRunDetails();

        public abstract bool SetLastRunDetails();

        public abstract bool SetNextRunIntervalInSeconds(double updatedNextRunIntervalInSeconds);

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Status Code: {0} - Status Message: {1}", Status.Code, Status.Message));
            sb.Append(string.Format(", LastRunDate: {0}", LastRunDate.ToString()));
            sb.Append(string.Format(", ImpactedItems: {0}", ImpactedItemsOnLastRun));
            sb.Append(string.Format(", NextRunIntervalInSeconds: {0}", NextRunIntervalInSeconds));
            sb.Append(string.Format(", ScheduledTaskType: {0}", ScheduledTaskType.ToString()));

            return sb.ToString();
        }
    }
}
