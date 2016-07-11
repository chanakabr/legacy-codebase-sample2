using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.QueueObjects
{
    public class ExpiredRecordingData : BaseCeleryData
    {
        public const string TASK = "distributed_tasks.process_expired_recording";

        private long Id;
        private long RecordingId;
        private long RecordingExpirationEpoch;
        private DateTime RecordingExpirationDate;

        public ExpiredRecordingData(int groupId, long id, long recordingId, long recordingExpirationEpoch, DateTime recordingExpirationDate) :
            base(// id = guid
                 Guid.NewGuid().ToString(),
                // task = const
                 TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.Id = id;
            this.RecordingId = recordingId;
            this.RecordingExpirationEpoch = recordingExpirationEpoch;
            this.RecordingExpirationDate = recordingExpirationDate;

            this.args = new List<object>()
            {
                groupId,
                id,
                recordingId,
                recordingExpirationEpoch,
                recordingExpirationDate
            };
        }
    }
}
