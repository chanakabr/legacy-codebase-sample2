using System;

namespace ApiObjects.Notification
{
    public class NotificationPartnerSettings
    {
        public bool? IsPushNotificationEnabled { get; set; }
        public bool? IsPushSystemAnnouncementsEnabled { get; set; }
        public int? PushStartHour { get; set; }
        public int? PushEndHour { get; set; }
        public bool? IsInboxEnabled { get; set; }
        public int? MessageTTL { get; set; }
        public bool? AutomaticIssueFollowNotifications { get; set; }

        public NotificationPartnerSettings()
        {
            // by default - AutomaticIssueFollowNotifications = true
            if (!AutomaticIssueFollowNotifications.HasValue)
                AutomaticIssueFollowNotifications = true;
        }   
    }
}
