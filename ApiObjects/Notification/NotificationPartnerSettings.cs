using System;

namespace ApiObjects.Notification
{
    public class NotificationPartnerSettings
    {
        public bool? push_notification_enabled { get; set; }
        public bool? push_system_announcements_enabled { get; set; }
        public DateTime? PushStartTime { get; set; }
        public DateTime? PushEndTime { get; set; }

        public NotificationPartnerSettings() { }

        public NotificationPartnerSettings(bool? push_notification_enabled, bool? push_system_announcements_enabled)
        {
            this.push_notification_enabled = push_notification_enabled;
            this.push_system_announcements_enabled = push_system_announcements_enabled;
        }
    }
}
