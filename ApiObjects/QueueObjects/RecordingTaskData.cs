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

        public const string TASK = "distributed_tasks.process_recording_mission";
        
        #endregion

        #region Data Members

        private eRecordingTask recordingTask;
        private long programId;
        private long recordingId;
        private DateTime epgStartDate;

        #endregion

        public RecordingTaskData(int groupId, eRecordingTask recordingTask, DateTime epgStartDate, DateTime etaDate, long programId, long recordingId)
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.ETA = etaDate;

            this.recordingTask = recordingTask;
            this.programId = programId;
            this.recordingId = recordingId;
            this.epgStartDate = epgStartDate;

            this.args = new List<object>()
            {
                groupId,
                recordingTask.ToString(),
                programId,
                recordingId,
                epgStartDate
            };
        }
    }
}
