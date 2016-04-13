using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class Recording
    {
        
        public ApiObjects.Response.Status Status { get; set; }

        public long RecordingID { get; set; }

        public long EpgID { get; set; }

        public TstvRecordingStatus RecordingStatus { get; set; }

        public string ExternalRecordingId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public Recording(long epgID) 
        {
            this.EpgID = epgID;
            this.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            this.RecordingID = 0;
            this.RecordingStatus = TstvRecordingStatus.Not_recorded;
        }

        public Recording(ApiObjects.Response.Status status, int recordingID, TstvRecordingStatus recordingStatus, long epgID)
        {
            this.Status = status;
            this.RecordingID = recordingID;
            this.RecordingStatus = recordingStatus;
            this.EpgID = epgID;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Status Code: {0}, Status Message: {1} ", Status.Code, Status.Message));
            sb.Append(string.Format("RecordingID: {0}, ", RecordingID));
            sb.Append(string.Format("EpgID: {0}, ", EpgID));
            sb.Append(string.Format("RecordingStatus: {0}, ", RecordingStatus));

            return sb.ToString();
        }

    }
}
