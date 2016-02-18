using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    public class DeviceNotification
    {
        public string Udid { get; set; }
        public long SignInAtSec { get; set; }
        public List<AnnouncementSubscriptions> AnnouncementSubscriptions { get; set; }
    }
}
