using System;

namespace ApiObjects.Notification
{
    public class NotificationPartnerSettings
    {
        public int PartnerId { get; set; }
        public bool? IsPushNotificationEnabled { get; set; }
        public bool? IsPushSystemAnnouncementsEnabled { get; set; }
        public int? PushStartHour { get; set; }
        public int? PushEndHour { get; set; }
        public bool? IsInboxEnabled { get; set; }
        public int? MessageTTLDays { get; set; }
        public bool? AutomaticIssueFollowNotifications { get; set; }
        public int? TopicExpirationDurationDays { get; set; }
        public bool? IsRemindersEnabled { get; set; }
        public int? RemindersPrePaddingSec { get; set; }
        public string PushAdapterUrl { get; set; }
        public string EngagementMailTemplateName { get; set; }
        public string EngagementMailSubject { get; set; }
        public string SenderEmail { get; set; }
        public string MailSenderName { get; set; }

        public NotificationPartnerSettings()
        {
            // by default - AutomaticIssueFollowNotifications = true
            if (!AutomaticIssueFollowNotifications.HasValue)
                AutomaticIssueFollowNotifications = true;

            // by default - TopicExpirationDurationDays is a year
            if (!TopicExpirationDurationDays.HasValue)
                TopicExpirationDurationDays = 365;

            // by default - reminders are disabled
            if (!IsRemindersEnabled.HasValue)
                AutomaticIssueFollowNotifications = false;

            // by default - reminders pre padding is 300 seconds
            if (!RemindersPrePaddingSec.HasValue)
                TopicExpirationDurationDays = 300;
        }
    }
}
