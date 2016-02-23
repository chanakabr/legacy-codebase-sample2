using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    //  KEY: device_data_<GID>_<UDID>
    public class DeviceNotificationData
    {
        public DeviceNotificationData()
        {
            this.SubscribedAnnouncements = new List<AnnouncementSubscription>();
        }

        public string UserId { get; set; }
        public string Udid { get; set; }
        public long UpdatedAt { get; set; }
        public string SubscriptionGuestExternalIdentifier { get; set; }

        public List<AnnouncementSubscription> SubscribedAnnouncements { get; set; }
    }
}
