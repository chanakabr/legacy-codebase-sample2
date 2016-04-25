using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class SearchRecording : Recording
    {

        public DateTime UpdateDate { get; set; }

        public SearchRecording()
            : base()
        {
        }

        public SearchRecording(Recording recording)
            :base()
        {
            this.Status = recording.Status;
            this.EpgID = recording.EpgID;
            this.RecordingID = recording.RecordingID;
            this.RecordingStatus = recording.RecordingStatus;
            this.ExternalRecordingId = recording.ExternalRecordingId;
            this.EpgStartDate = recording.EpgStartDate;
            this.EpgEndDate = recording.EpgEndDate;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append(string.Format("UpdateDate: {0}, ", UpdateDate != null ? UpdateDate.ToString() : ""));

            return sb.ToString();
        }
    }
}
