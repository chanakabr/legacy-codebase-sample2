using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Core.Notification
{
    ///// <summary>
    ///// Represent notifiaction message that sent to the usesr devices,
    ///// mapped to a record in notifications_messages table at the db.
    ///// </summary>
    
    //[DataContract]
    //public class NotificationMessage
    //{
    //    #region public Properties
    //    [DataMember]
    //    public Guid  ID { get; set; }
    //    [DataMember]
    //    public long NotificationID { get; set; }
    //    [DataMember]
    //    public long NotificationRequestID { get; set; }
    //    [DataMember]
    //    public long NotificationMessageID { get; set; }
    //    [DataMember]
    //    public long UserID { get; set; }
    //    [DataMember]
    //    public NotificationMessageStatus Status { get; set; }
    //    [DataMember]
    //    public NotificationMessageType Type { get; set; }
    //    [DataMember]
    //    public string MessageText { get; set; }
    //    [DataMember]
    //    public string Title { get; set; }
    //    [DataMember]
    //    public DateTime PublishDate { get; set; }
    //    [DataMember]
    //    public string AppName { get; set; }
    //    [DataMember]
    //    public long DeviceID { get; set; }
    //    [DataMember]
    //    public string UdID { get; set; }   //maps as "Recipient" at the MessageBox wcf service. 
    //    [DataMember]
    //    public NotificationRequestAction[] Actions { get; set; }
    //    [DataMember]
    //    public NotificationMessageViewStatus ViewStatus { get; set; }
    //    [DataMember]
    //    public ExtraParams TagNotificationParams { get; set; }
    //    [DataMember]
    //    public long nGroupID { get; set; }
    //    #endregion

    //    #region Constructors
    //    public NotificationMessage(NotificationMessageType type, long notificationID, long notificationRequestID, long userID, NotificationMessageStatus status,
    //                               string messageText, string title, DateTime publishDate, string appName, long deviceID, string udid, NotificationRequestAction[] actions, ExtraParams tagParams, long groupID)
    //    {
    //        this.ID = Guid.NewGuid();
    //        this.NotificationID = notificationID;
    //        this.NotificationRequestID = notificationRequestID;
    //        this.UserID = userID;
    //        this.Status = status;
    //        this.Type = type;          
    //        this.MessageText = messageText;
    //        this.Title = title;
    //        this.PublishDate = publishDate;
    //        this.AppName = appName;
    //        this.DeviceID = deviceID;
    //        this.UdID = udid;
    //        this.Actions = actions;
    //        this.TagNotificationParams = tagParams;
    //        this.nGroupID = groupID;
    //    }

    //    public NotificationMessage(NotificationMessageType type, long notificationID, long notificationRequestID, long userID, NotificationMessageStatus status,
    //                               string messageText, string title, string appName, string udid, long deviceID, NotificationRequestAction[] actions, ExtraParams tagParams, long groupID)
    //    {
    //        this.ID = Guid.NewGuid();
    //        this.NotificationID = notificationID;
    //        this.NotificationRequestID = notificationRequestID;
    //        this.UserID = userID;
    //        this.Status = status;
    //        this.Type = type;
    //        this.MessageText = messageText;
    //        this.Title = title;
    //        this.AppName = appName;
    //        this.DeviceID = deviceID;
    //        this.UdID = udid;         
    //        this.PublishDate = DateTime.UtcNow;
    //        this.TagNotificationParams = tagParams;
    //        this.nGroupID = groupID;
    //    }
    //    public NotificationMessage(NotificationMessageType type, long notificationID, long notificationRequestID, long notificationMessageID, long userID, NotificationMessageStatus status,
    //                               string messageText, string title, DateTime publishDate, string appName, long deviceID, string udid, NotificationRequestAction[] actions, NotificationMessageViewStatus viewStatus, ExtraParams tagParams, long groupID)
    //    {
    //        this.ID =  Guid.NewGuid();
    //        this.NotificationID = notificationID;
    //        this.NotificationRequestID = notificationRequestID;
    //        this.NotificationMessageID = notificationMessageID;
    //        this.UserID = userID;
    //        this.Status = status;
    //        this.Type = type;
    //        this.MessageText = messageText;
    //        this.Title = title;
    //        this.PublishDate = publishDate;
    //        this.AppName = appName;
    //        this.DeviceID = deviceID;
    //        this.UdID = udid;
    //        this.Actions = actions;
    //        this.ViewStatus = viewStatus;
    //        this.TagNotificationParams = tagParams;
    //        this.nGroupID = groupID;
    //    }
    //    #endregion
    //}
}
