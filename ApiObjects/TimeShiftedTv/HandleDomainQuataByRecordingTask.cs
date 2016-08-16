using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class HandleDomainQuataByRecordingTask
    {

        public long Id { get; set; }

        public long RecordingId { get; set; }

        public int GroupId { get; set; }

        public long ScheduledExpirationEpoch;

        public int OldRecordingDuration { get; set; }        

        public HandleDomainQuataByRecordingTask() {}        

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();            
            sb.Append(string.Format("Id: {0}, ", Id));
            sb.Append(string.Format("RecordingId: {0}, ", RecordingId));
            sb.Append(string.Format("GroupId: {0}, ", GroupId));            
            sb.Append(string.Format("scheduledExpirationEpoch: {0}, ", ScheduledExpirationEpoch));
            sb.Append(string.Format("RecordingDurationChange: {0}, ", OldRecordingDuration));
            return sb.ToString();
        }
    }
}
