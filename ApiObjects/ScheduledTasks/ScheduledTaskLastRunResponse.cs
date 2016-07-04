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
        public ApiObjects.Response.Status Status { get; set; }
        public DateTime LastSuccessfulRunDate { get; set; }
        public int ImpactedItems { get; set; }        
        public int NextRunIntervalInMinutes { get; set; }

        public ScheduledTaskLastRunResponse() { }

        public ScheduledTaskLastRunResponse(DateTime lastSuccessfulRunDate, int impactedItems, int nextRunIntervalInMinutes)
        {
            this.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            this.LastSuccessfulRunDate = lastSuccessfulRunDate;
            this.ImpactedItems = impactedItems;            
            this.NextRunIntervalInMinutes = nextRunIntervalInMinutes;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Status Code: {0}, Status Message: {1} ", Status.Code, Status.Message));
            sb.Append(string.Format("LastSuccessfulRunDate: {0}, ", LastSuccessfulRunDate.ToString()));
            sb.Append(string.Format("ImpactedItems: {0}, ", ImpactedItems));
            sb.Append(string.Format("NextRunIntervalInMinutes: {0}", NextRunIntervalInMinutes));

            return sb.ToString();
        }
    }
}
