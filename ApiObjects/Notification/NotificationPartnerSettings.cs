using System;

namespace ApiObjects.Notification
{
    public class NotificationPartnerSettings
    {
        public bool? push_notification_enabled { get; set; }
        public bool? push_system_announcements_enabled { get; set; }
        public int? PushStartHour { get; set; }
        public int? PushEndHour { get; set; }

        public NotificationPartnerSettings() { }
    }
}
