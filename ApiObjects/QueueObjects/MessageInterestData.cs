using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.QueueObjects
{
    public class MessageInterestData : BaseCeleryData
    {
        #region Consts

        public const string TASK = "distributed_tasks.process_message_interests";
        
        #endregion

        #region Data Members
        
        private int groupId;
        private long startTime;
        private int messageInterestId;

        #endregion

        public MessageInterestData(int groupId, long startTime, int messageInterestId)
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK)
        {
            // Basic member initialization
            this.groupId = groupId;
            this.startTime = startTime;
            this.messageInterestId = messageInterestId;

            this.args = new List<object>()
            {
                groupId,
                startTime,
                messageInterestId,
                base.RequestId
            };
        }
    }
}
