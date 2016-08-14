using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.ScheduledTasks
{
    public class ScheduledTaskLastRunResponse
    {
        // Type to be filtered on views of CB
        public const int Type = 1000;
        public ApiObjects.Response.Status Status { get; set; }
        public DateTime LastRunDate { get; set; }
        public int ImpactedItemsOnLastRun { get; set; }        
        public double NextRunIntervalInSeconds { get; set; }
        public ScheduledTaskType ScheduledTaskType { get; set; }        

        public ScheduledTaskLastRunResponse() { }

        public ScheduledTaskLastRunResponse(DateTime lastSuccessfulRunDate, int impactedItems, double nextRunIntervalInSeconds, ScheduledTaskType scheduledTaskType)
        {
            this.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            this.LastRunDate = lastSuccessfulRunDate;
            this.ImpactedItemsOnLastRun = impactedItems;            
            this.NextRunIntervalInSeconds = nextRunIntervalInSeconds;
            this.ScheduledTaskType = scheduledTaskType;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Status Code: {0}, Status Message: {1} ", Status.Code, Status.Message));
            sb.Append(string.Format("LastSuccessfulRunDate: {0}, ", LastRunDate.ToString()));
            sb.Append(string.Format("ImpactedItems: {0}, ", ImpactedItemsOnLastRun));
            sb.Append(string.Format("NextRunIntervalInSeconds: {0} ", NextRunIntervalInSeconds));
            sb.Append(string.Format("ScheduledTaskType: {0}", ScheduledTaskType.ToString()));

            return sb.ToString();
        }

    }
}
