using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.ScheduledTasks
{
    public class ExpiredRecordingScheduledTask
    {

        public long Id { get; set; }

        public long RecordingId { get; set; }

        public int GroupId { get; set; }

        public DateTime ScheduledExpirationDate;

        public long ScheduledExpirationEpoch;

        public ExpiredRecordingScheduledTask() {}

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();            
            sb.Append(string.Format("Id: {0}, ", Id));
            sb.Append(string.Format("RecordingId: {0}, ", RecordingId));
            sb.Append(string.Format("GroupId: {0}, ", GroupId));            
            sb.Append(string.Format("scheduledExpirationDate: {0}, ", ScheduledExpirationDate != null ? ScheduledExpirationDate.ToString() : ""));
            sb.Append(string.Format("scheduledExpirationEpoch: {0}, ", ScheduledExpirationEpoch));

            return sb.ToString();
        }
    }
}
