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

        public long Id { get; set; }        

        public long EpgId { get; set; }

        public string ChannelId { get; set; }

        public TstvRecordingStatus RecordingStatus { get; set; }

        public string ExternalRecordingId { get; set; }        

        public DateTime EpgStartDate { get; set; }

        public DateTime EpgEndDate { get; set; }

        public RecordingType Type { get; set; }       

        public int GetStatusRetries { get; set; }

        public long? ProtectedUntilDate { get; set; }

        public long? ViewableUntilDate { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public Recording()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            this.Id = 0;            
            this.Type = RecordingType.Single;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Status Code: {0}, Status Message: {1} ", Status.Code, Status.Message));
            sb.Append(string.Format("Id: {0}, ", Id));
            sb.Append(string.Format("EpgID: {0}, ", EpgId));
            sb.Append(string.Format("ChannelId: {0}, ", string.IsNullOrEmpty(ChannelId) ? "" : ChannelId));
            sb.Append(string.Format("RecordingStatus: {0}, ", RecordingStatus));
            sb.Append(string.Format("Type: {0}, ", Type));            
            sb.Append(string.Format("ExternalRecordingId: {0}, ", string.IsNullOrEmpty(ExternalRecordingId) ? "" : ExternalRecordingId));
            sb.Append(string.Format("StartDate: {0}, ", EpgStartDate != null ? EpgStartDate.ToString() : ""));
            sb.Append(string.Format("EndDate: {0}, ", EpgEndDate != null ? EpgEndDate.ToString() : ""));
            sb.Append(string.Format("GetStatusRetries: {0}, ", GetStatusRetries));
            sb.Append(string.Format("ViewableUntilDate: {0}, ", ViewableUntilDate.HasValue ? ViewableUntilDate.Value.ToString() : ""));
            sb.Append(string.Format("ProtectedUntilDate: {0}, ", ProtectedUntilDate.HasValue ? ProtectedUntilDate.Value.ToString() : ""));
            sb.Append(string.Format("CreateDate: {0}, ", CreateDate != null ? CreateDate.ToString() : ""));
            sb.Append(string.Format("UpdateDate: {0}, ", UpdateDate != null ? UpdateDate.ToString() : ""));            

            return sb.ToString();
        }

    }
}
