using RestfulTVPApi.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class Notification
    {
        public Guid id { get; set; }
        public long notification_id { get; set; }
        public long notification_request_id { get; set; }
        public long notification_message_id { get; set; }
        public long user_id { get; set; }
        public NotificationMessageStatus status { get; set; }
        public NotificationMessageType type { get; set; }
        public string message_text { get; set; }
        public string title { get; set; }
        public DateTime publish_date { get; set; }
        public string app_name { get; set; }
        public long DeviceID { get; set; }
        public string udid { get; set; }   //maps as "Recipient" at the MessageBox wcf service. 
        public List<NotificationRequestAction> actions { get; set; }
        public NotificationMessageViewStatus view_status { get; set; }
        public ExtraParameters tag_notification_params { get; set; }
        public long nGroupID { get; set; }

        public Notification()
        {
            tag_notification_params = new ExtraParameters();
        }
    }
}
