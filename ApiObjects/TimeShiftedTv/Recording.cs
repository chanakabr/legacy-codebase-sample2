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

        public string EpgChannelID { get; set; }

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

        public Recording(long epgID, string epgChannelID, DateTime startDate, DateTime endDate)
        {
            this.EpgID = epgID;
            this.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            this.RecordingID = 0;
            this.RecordingStatus = TstvRecordingStatus.Not_recorded;
            this.EpgChannelID = epgChannelID;
            this.StartDate = startDate;
            this.EndDate = endDate;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Status Code: {0}, Status Message: {1} ", Status.Code, Status.Message));
            sb.Append(string.Format("RecordingID: {0}, ", RecordingID));
            sb.Append(string.Format("EpgID: {0}, ", EpgID));
            sb.Append(string.Format("RecordingStatus: {0}, ", RecordingStatus));
            sb.Append(string.Format("EpgChannelID: {0}, ", string.IsNullOrEmpty(EpgChannelID) ? "" : EpgChannelID));
            sb.Append(string.Format("ExternalRecordingId: {0}, ", string.IsNullOrEmpty(ExternalRecordingId) ? "" : ExternalRecordingId));
            sb.Append(string.Format("StartDate: {0}, ", StartDate != null ? StartDate.ToString() : ""));
            sb.Append(string.Format("EndDate: {0}, ", EndDate != null ? EndDate.ToString() : ""));

            return sb.ToString();
        }

    }
}
