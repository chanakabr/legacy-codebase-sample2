using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    public class DeviceAppNotification
    {
        public DeviceAppNotification()
        {
            AnnouncementSubscriptions = new List<AnnouncementSubscription>();
        }

        public string Udid { get; set; }
        public long SignInAtSec { get; set; }
        public List<AnnouncementSubscription> AnnouncementSubscriptions { get; set; }
    }
}
