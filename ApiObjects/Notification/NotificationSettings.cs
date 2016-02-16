using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    public class NotificationSettings
    {
        public bool? push_notification_enabled { get; set; }

        public NotificationSettings()
        {
        }

        public NotificationSettings(bool? push_notification_enabled)
        {
            this.push_notification_enabled = push_notification_enabled;
        }
    }
}
