using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ApiObjects.Notification
{
    //  KEY: device_data_<GID>_<UDID>
    public class DeviceNotificationData
    {
        public DeviceNotificationData()
        {
            this.SubscribedAnnouncements = new List<AnnouncementSubscription>();
            this.SubscribedReminders = new List<AnnouncementSubscription>();
        }

        public int UserId { get; set; }
        public string Udid { get; set; }
        public long UpdatedAt { get; set; }
        public bool IsLoggedIn { get; set; }
        public string SubscriptionExternalIdentifier { get; set; }

        public List<AnnouncementSubscription> SubscribedAnnouncements { get; set; }

        public List<AnnouncementSubscription> SubscribedReminders { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ulong cas { get; set; }
    }
}
