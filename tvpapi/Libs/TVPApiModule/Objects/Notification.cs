using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.TvinciPlatform.Notification;

namespace TVPApiModule.Objects
{
    public class Notification
    {
        public Guid ID { get; set; }
        public long NotificationID { get; set; }
        public long NotificationRequestID { get; set; }
        public long NotificationMessageID { get; set; }
        public long UserID { get; set; }
        public NotificationMessageStatus Status { get; set; }
        public NotificationMessageType Type { get; set; }
        public string MessageText { get; set; }
        public string Title { get; set; }
        public DateTime PublishDate { get; set; }
        public string AppName { get; set; }
        public long DeviceID { get; set; }
        public string UdID { get; set; }   //maps as "Recipient" at the MessageBox wcf service. 
        public NotificationRequestAction[] Actions { get; set; }
        public NotificationMessageViewStatus ViewStatus { get; set; }
        public ExtraParameters TagNotificationParams { get; set; }
        public long nGroupID { get; set; }

        public Notification()
        {
            TagNotificationParams = new ExtraParameters();
        }
    }
}
