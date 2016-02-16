using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.QueueObjects
{
    public class MessageAnnouncementData : BaseCeleryData
    {
        #region Consts

        public const string TASK = "distributed_tasks.message_announcement_data";
        
        #endregion

        #region Data Members

        private string name;
        private string message;
        private DateTime startTime;
        private int status;
        private int recipients;

        #endregion

        public MessageAnnouncementData(int groupId, string name, string message, DateTime startTime, int status, int recipients)
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.name = name;
            this.message = message;
            this.startTime = startTime;
            this.status = status;
            this.recipients = recipients;

            this.args = new List<object>()
            {
                groupId,
                name,
                message,
                startTime,
                status,
                recipients
            };
        }
    }
}
