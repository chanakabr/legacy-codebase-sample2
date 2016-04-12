using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class Recording
    {

        public int RecordingID { get; set; }

        public TstvRecordingStatus RecordingStatus { get; set; }

        public Recording() { }

        public Recording(int recordingID, TstvRecordingStatus recordingStatus)
        {
            this.RecordingID = recordingID;
            this.RecordingStatus = recordingStatus;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("RecordingID: {0}, ", RecordingID));
            sb.Append(string.Format("RecordingStatus: {0}, ", RecordingStatus));

            return sb.ToString();
        }

    }
}
