using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class RecordingTaskData : BaseCeleryData
    {
        #region Consts

        public const string TASK = "distributed_tasks.process_recording_task";
        
        #endregion

        #region Data Members

        private eRecordingTask recordingTask;
        private long programId;
        private long recordingId;

        #endregion

        public RecordingTaskData(int groupId, eRecordingTask recordingTask, DateTime time, long programId, long recordingId)
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.ETA = time;

            this.recordingTask = recordingTask;
            this.programId = programId;
            this.recordingId = recordingId;

            this.args = new List<object>()
            {
                recordingTask.ToString(),
                programId,
                recordingId
            };
        }
    }
}
