using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ApiObjects.Notification
{
    /// <summary>
    /// Represent notification object, 
    /// mapped to a record in notification table at the db
    /// </summary>
    public class Notification
    {

        #region public Properties
        public long ID { get; set; }
        public NotificationMessageType MessageType { get; set; }
        public int TriggerType { get; set; }
        public long GroupID { get; set; }
        public string MessageText { get; set; }
        public string SmsMessageText { get; set; }
        public string PullMessageText { get; set; }
        public string Title { get; set; }
        public bool IsActive { get; set; }
        public int Status { get; set; }
        public bool IsBroadcast { get; set; }
        public DateTime CreatedDate { get; set; }
        public NotificationRequestAction[] Actions { get; set; }
        //parameters for notification with specific values         
        public string key { get; set; }
        #endregion

        #region Constructor
        public Notification()
        {
        }
        public Notification(long id, NotificationMessageType messageType, int triggerType, long groupID, string messageText, string smsMessageText, string pullMessageText, string title,
                            bool isActive, int status, bool isBroadCast, DateTime createdDate, NotificationRequestAction[] actions, string sKey)
        {
            Initialize(id, messageType, triggerType, groupID, messageText, smsMessageText, pullMessageText, title, isActive, status, isBroadCast, createdDate, actions, sKey);
        }

        public void Initialize(long id, NotificationMessageType messageType, int triggerType, long groupID, string messageText, string smsMessageText, string pullMessageText, string title,
                            bool isActive, int status, bool isBroadCast, DateTime createdDate, NotificationRequestAction[] actions, string sKey)
        {
            this.ID = id;
            this.MessageType = messageType;
            this.TriggerType = triggerType;
            this.GroupID = groupID;
            this.MessageText = messageText;
            this.SmsMessageText = smsMessageText;
            this.PullMessageText = pullMessageText;
            this.Title = title;
            this.IsActive = isActive;
            this.Status = status;
            this.IsBroadcast = isBroadCast;
            this.CreatedDate = createdDate;
            this.Actions = actions;
            this.key = sKey;
        }

        public Notification(Notification n)
        {
            Initialize(n.ID, n.MessageType, n.TriggerType, n.GroupID, n.MessageText, n.SmsMessageText, n.PullMessageText, n.Title, n.IsActive, n.Status, n.IsBroadcast, n.CreatedDate, n.Actions, n.key);
        }

        #endregion
    }
}
