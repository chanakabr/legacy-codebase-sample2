using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    // key NOTIFICATIONS_GID_USER_ID
    public class UserNotification
    {
        public List<DeviceNotification> DeviceNotifications { get; set; }
        public List<Announcement> Announcements { get; set; }
    }
}
