using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.QueueObjects
{
    public class MessageAnnouncementData : BaseCeleryData
    {
        #region Consts

        public const string TASK = "distributed_tasks.process_message_announcements";
        
        #endregion

        #region Data Members
        
        private int groupId;
        private long startTime;
        private int messageAnnouncementId;

        #endregion

        public MessageAnnouncementData(int groupId, long startTime, int messageAnnouncementId)
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK)
        {
            // Basic member initialization
            this.groupId = groupId;
            this.startTime = startTime;
            this.messageAnnouncementId = messageAnnouncementId;

            this.args = new List<object>()
            {
                groupId,
                startTime,
                messageAnnouncementId
            };
        }
    }
}
