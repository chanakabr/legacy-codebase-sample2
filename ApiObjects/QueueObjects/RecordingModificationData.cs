using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.QueueObjects
{
    public class RecordingModificationData : BaseCeleryData
    {
        public const string TASK = "distributed_tasks.process_modified_recording";

        private long Id;
        private long RecordingId;
        private long RecordingExpirationEpoch;
        private int OldRecordingDuration;

        public RecordingModificationData(int groupId, long id, long recordingId, long recordingExpirationEpoch, int oldRecordingDuration = 0) :
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
            this.OldRecordingDuration = oldRecordingDuration;

            this.args = new List<object>()
            {
                groupId,
                id,
                recordingId,
                recordingExpirationEpoch,
                oldRecordingDuration
            };
        }
    }
}
