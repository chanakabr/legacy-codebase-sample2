using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.QueueObjects
{
    public class UserNotificationData : BaseCeleryData
    {
        #region Consts

        public const string TASK = "distributed_tasks.process_initiate_notification_action";

        #endregion

        #region Data Members

        private int UserAction;
        private int UserId;
        private string Udid;
        private string PushToken;

        #endregion

        public UserNotificationData(int groupId, int userAction, int userId, string udid, string pushToken)
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.UserAction = userAction;
            this.UserId = userId;
            this.Udid = udid;
            this.PushToken = pushToken;

            this.args = new List<object>()
            {
                groupId,
                userAction,
                userId,
                udid,
                pushToken
            };
        }
    }
}
