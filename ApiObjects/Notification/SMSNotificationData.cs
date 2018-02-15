using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ApiObjects.Notification
{
    public abstract class NotificationData
    {
        public NotificationData()
        {
            this.SubscribedAnnouncements = new List<NotificationSubscription>();
            this.SubscribedReminders = new List<NotificationSubscription>();
            this.SubscribedSeriesReminders = new List<NotificationSubscription>();
            this.SubscribedUserInterests = new List<NotificationSubscription>();
        }

        public NotificationData(int userId)
        {
            this.UserId = userId;
            this.SubscribedAnnouncements = new List<NotificationSubscription>();
            this.SubscribedReminders = new List<NotificationSubscription>();
            this.SubscribedSeriesReminders = new List<NotificationSubscription>();
            this.SubscribedUserInterests = new List<NotificationSubscription>();
        }

        public int UserId { get; set; }
        public long UpdatedAt { get; set; }
        public string SubscriptionExternalIdentifier { get; set; }

        public List<NotificationSubscription> SubscribedAnnouncements { get; set; }

        public List<NotificationSubscription> SubscribedReminders { get; set; }

        public List<NotificationSubscription> SubscribedSeriesReminders { get; set; }

        public List<NotificationSubscription> SubscribedUserInterests { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ulong cas { get; set; }
    }

    //  KEY: sms_data_<GID>_<UserId>
    public class SmsNotificationData : NotificationData
    {
        public SmsNotificationData(int userId) : base(userId)
        {
         
        }

        public string SMSExternalToken { get; set; }
    }

}
