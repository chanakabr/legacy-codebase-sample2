using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    // key NOTIFICATIONS_GID_USER_ID
    public class UserNotification
    {
        public UserNotification()
        {
            this.DeviceNotifications = new List<DeviceAppNotification>();
            this.Announcements = new List<Announcement>();
        }
        public List<DeviceAppNotification> DeviceNotifications { get; set; }
        public List<Announcement> Announcements { get; set; }
    }
}
