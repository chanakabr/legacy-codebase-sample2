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

        public string ChannelId { get; set; }

        public TstvRecordingStatus RecordingStatus { get; set; }

        public string ExternalRecordingId { get; set; }        

        public DateTime EpgStartDate { get; set; }

        public DateTime EpgEndDate { get; set; }

        public Recording()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            this.RecordingID = 0;
            this.RecordingStatus = TstvRecordingStatus.DoesNotExist;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Status Code: {0}, Status Message: {1} ", Status.Code, Status.Message));
            sb.Append(string.Format("RecordingID: {0}, ", RecordingID));
            sb.Append(string.Format("EpgID: {0}, ", EpgID));
            sb.Append(string.Format("ChannelId: {0}, ", string.IsNullOrEmpty(ChannelId) ? "" : ChannelId));
            sb.Append(string.Format("RecordingStatus: {0}, ", RecordingStatus));            
            sb.Append(string.Format("ExternalRecordingId: {0}, ", string.IsNullOrEmpty(ExternalRecordingId) ? "" : ExternalRecordingId));
            sb.Append(string.Format("StartDate: {0}, ", EpgStartDate != null ? EpgStartDate.ToString() : ""));
            sb.Append(string.Format("EndDate: {0}, ", EpgEndDate != null ? EpgEndDate.ToString() : ""));

            return sb.ToString();
        }

    }
}
