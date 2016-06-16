using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class RecordingCleanupResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public DateTime LastSuccessfulCleanUpDate { get; set; }
        public int DeletedRecordingOnLastCleanup { get; set; }
        public int DomainRecordingsUpdatedOnLastCleanup { get; set; }

        public RecordingCleanupResponse() { }

        public RecordingCleanupResponse(DateTime lastSuccessfulCleanUpDate, int deletedRecordingOnLastCleanup, int domainRecordingsUpdatedOnLastCleanup)
        {
            this.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            this.LastSuccessfulCleanUpDate = lastSuccessfulCleanUpDate;
            this.DeletedRecordingOnLastCleanup = deletedRecordingOnLastCleanup;
            this.DomainRecordingsUpdatedOnLastCleanup = domainRecordingsUpdatedOnLastCleanup;            
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Status Code: {0}, Status Message: {1} ", Status.Code, Status.Message));
            sb.Append(string.Format("LastSuccessfulCleanUpDate: {0}, ", LastSuccessfulCleanUpDate.ToString()));
            sb.Append(string.Format("DeletedRecordingOnLastCleanup: {0}, ", DeletedRecordingOnLastCleanup));
            sb.Append(string.Format("DomainRecordingsUpdatedOnLastCleanup: {0}, ", DomainRecordingsUpdatedOnLastCleanup));

            return sb.ToString();        
        }
    }
}
