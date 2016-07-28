using ApiObjects.Response;
using ApiObjects.ScheduledTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class RecordingCleanupResponse : ScheduledTaskLastRunResponse
    {                
        public int DeletedRecordingOnLastCleanup { get; set; }
        public int DomainRecordingsUpdatedOnLastCleanup { get; set; }        

        public RecordingCleanupResponse() { }

        public RecordingCleanupResponse(DateTime lastSuccessfulCleanUpDate, int deletedRecordingOnLastCleanup, int domainRecordingsUpdatedOnLastCleanup, double nextRunIntervalInSeconds, ScheduledTaskType scheduledTaskType)
            : base(lastSuccessfulCleanUpDate, 0, nextRunIntervalInSeconds, scheduledTaskType)
        {            
            this.DeletedRecordingOnLastCleanup = deletedRecordingOnLastCleanup;
            this.DomainRecordingsUpdatedOnLastCleanup = domainRecordingsUpdatedOnLastCleanup;            
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString() + ", ");                        
            sb.Append(string.Format("DeletedRecordingOnLastCleanup: {0}, ", DeletedRecordingOnLastCleanup));
            sb.Append(string.Format("DomainRecordingsUpdatedOnLastCleanup: {0}", DomainRecordingsUpdatedOnLastCleanup));            

            return sb.ToString();        
        }
    }
}
