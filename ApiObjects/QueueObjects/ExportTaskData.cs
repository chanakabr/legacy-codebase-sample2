using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.QueueObjects
{
    public class ExportTaskData : BaseCeleryData
    {
        public const string TASK = "distributed_tasks.process_export";

        private long taskId;
        private string version;

        public ExportTaskData(int groupId, long taskId, string version, DateTime eta) :
            base(// id = guid
                 Guid.NewGuid().ToString(),
                // task = const
                 TASK)
        {
            this.GroupId = groupId;
            this.ETA = eta;
            this.taskId = taskId;
            this.version = version;
            
            this.args = new List<object>()
            {
                groupId,
                taskId,
                version,
            };
        }
    }
}
