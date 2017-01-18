using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
     public class NotificationRequest
    {
        #region public Properties
        public long ID { get; set; }
        public long NotificationID { get; set; }
        public NotificationRequestStatus Status { get; set; }
        public long GroupID { get; set; }
        public long UserID { get; set; }
        public DateTime CreatedDate { get; set; }
        public NotificationRequestType Type { get; set; }
        public NotificationMessageType MessageType { get; set; }
        public string MessageText { get; set; }
        public string SmsMessageText { get; set; }
        public string PullMessageText { get; set; }
        public string Title { get; set; }
        public NotificationRequestAction[] Actions { get; set; }
        public NotificationTriggerType TriggerType { get; set; }
        public string mediaPicURL { get; set; }
        public string EmailTemplate { get; set; }
        public string DateFormat { get; set; }
        public SendVia sendVia { get; set; }
        public ExtraParams oExtraParams { get; set; }

        #endregion

        #region Constructors
        public NotificationRequest()
        {
        }

        public NotificationRequest(long id, long notificationID, NotificationRequestStatus status, long groupID, long userID, DateTime createdDate, NotificationRequestType type, NotificationMessageType messageType,
            string messageText, string smsMessageText, string pullMessageText, string title, NotificationRequestAction[] actions, NotificationTriggerType eTriggerType)
        {
            Initialize(id, notificationID, status, groupID, userID, createdDate, type, messageType, messageText, smsMessageText, pullMessageText, title, actions, eTriggerType);
        }
        public NotificationRequest(long id, long notificationID, NotificationRequestStatus status, long groupID, long userID, DateTime createdDate, NotificationRequestType type, NotificationMessageType messageType,
            string messageText, string smsMessageText, string pullMessageText, string title, NotificationRequestAction[] actions, NotificationTriggerType eTriggerType, SendVia oSendVia, ExtraParams extraParams)
        {
            Initialize(id, notificationID, status, groupID, userID, createdDate, type, messageType, messageText, smsMessageText, pullMessageText, title, actions, eTriggerType);
            sendVia.is_email = oSendVia.is_email;
            sendVia.is_sms = oSendVia.is_sms;
            sendVia.is_device = oSendVia.is_device;
            oExtraParams = extraParams;
        }


        public NotificationRequest(Notification objNotification, long userID)
        {
            sendVia = new SendVia();
            this.NotificationID = objNotification.ID;
            this.Status = NotificationRequestStatus.NotStarted;
            this.GroupID = objNotification.GroupID;
            this.UserID = userID;
            this.Type = (objNotification.IsBroadcast == true ? NotificationRequestType.BroadCast : NotificationRequestType.Simple);
            this.MessageType = objNotification.MessageType;
            this.MessageText = objNotification.MessageText;
            this.SmsMessageText = objNotification.SmsMessageText;
            this.PullMessageText = objNotification.PullMessageText;
            this.Title = objNotification.Title;
            this.TriggerType = (NotificationTriggerType)objNotification.TriggerType;
            this.Actions = objNotification.Actions;
        }


        public void Initialize(long id, long notificationID, NotificationRequestStatus status, long groupID, long userID, DateTime createdDate, NotificationRequestType type, NotificationMessageType messageType,
            string messageText, string smsMessageText, string pullMessageText, string title, NotificationRequestAction[] actions, NotificationTriggerType eTriggerType)
        {
            this.ID = id;
            this.NotificationID = notificationID;
            this.Status = status;
            this.GroupID = groupID;
            this.UserID = userID;
            this.CreatedDate = createdDate;
            this.Type = type;
            this.MessageType = messageType;
            this.MessageText = messageText;
            this.SmsMessageText = smsMessageText;
            this.PullMessageText = pullMessageText;
            this.Title = title;
            this.Actions = actions;
            this.TriggerType = eTriggerType;
            this.sendVia = new SendVia();
        }
        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Concat("ID: ", ID));
            sb.Append(String.Concat(" Notification ID: ", NotificationID));
            sb.Append(String.Concat(" Status: ", Status.ToString()));
            sb.Append(String.Concat(" GroupID: ", GroupID));
            sb.Append(String.Concat(" UserID: ", UserID));
            sb.Append(String.Concat(" CreatedDate: ", CreatedDate.ToString()));
            sb.Append(String.Concat(" Type: ", Type.ToString()));
            sb.Append(String.Concat(" MessageType: ", MessageType.ToString()));
            sb.Append(String.Concat(" SmsMessageText: ", SmsMessageText));
            sb.Append(String.Concat(" PullMessageText: ", PullMessageText));
            sb.Append(String.Concat(" Title: ", Title));
            if (Actions != null)
            {
                sb.Append(string.Format("Actions array length: {0} ", Actions.Length));
                for (int i = 0; i < Actions.Length; i++)
                {
                    sb.Append(String.Concat(" Action[", i, "]: "));
                    sb.Append(String.Concat(" Action ID: ", Actions[i].ID));
                    sb.Append(String.Concat(" Action Link: ", Actions[i].Link));
                    sb.Append(String.Concat(" Action Text: ", Actions[i].Text));
                }
            }
            else
            {
                sb.Append(" Actions: null ");
            }
            sb.Append(String.Concat(" TriggerType: ", TriggerType.ToString()));
            sb.Append(String.Concat(" Media Pic URL: ", mediaPicURL));
            sb.Append(String.Concat(" EmailTemplate: ", EmailTemplate));
            if (sendVia != null)
                sb.Append(string.Format(" SenaVia: SMS: {0} , Email: {1} , Device: {2}  ", sendVia.is_sms, sendVia.is_email, sendVia.is_device));
            else
                sb.Append(" SendVia: null ");

            return sb.ToString();
        }

    }
}
