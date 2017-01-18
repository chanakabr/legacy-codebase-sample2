using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.QueueObjects
{
    public class MessageReminderData : BaseCeleryData
    {
        #region Consts

        public const string TASK = "distributed_tasks.process_message_reminders";
        
        #endregion

        #region Data Members
        
        private int groupId;
        private long startTime;
        private int messageReminderId;

        #endregion

        public MessageReminderData(int groupId, long startTime, int messageReminderId)
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK)
        {
            // Basic member initialization
            this.groupId = groupId;
            this.startTime = startTime;
            this.messageReminderId = messageReminderId;

            this.args = new List<object>()
            {
                groupId,
                startTime,
                messageReminderId
            };
        }
    }
}
