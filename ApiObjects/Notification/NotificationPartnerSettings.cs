using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    public class NotificationPartnerSettings
    {
        public bool? push_notification_enabled { get; set; }
        public bool? push_system_announcements_enabled { get; set; }
    }
}
