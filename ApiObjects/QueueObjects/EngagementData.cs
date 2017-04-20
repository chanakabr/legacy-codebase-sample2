using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.QueueObjects
{
    public class EngagementData : BaseCeleryData
    {
        #region Consts

        public const string TASK = "distributed_tasks.process_engagement";

        #endregion

        #region Data Members

        private int groupId;
        private long startTime;
        private int engagementId;
        private int engagementBulkId;

        #endregion

        public EngagementData(int groupId, long startTime, int engagementId, int engagementBulkId)
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK)
        {
            // Basic member initialization
            this.groupId = groupId;
            this.startTime = startTime;
            this.engagementId = engagementId;
            this.engagementBulkId = engagementBulkId;

            this.args = new List<object>()
            {
                groupId,
                startTime,
                engagementId,
                engagementBulkId
            };
        }
    }
}
