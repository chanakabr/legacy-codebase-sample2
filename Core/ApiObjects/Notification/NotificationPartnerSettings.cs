using System.Collections.Generic;

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
        public string ChurnMailTemplateName { get; set; }
        public string ChurnMailSubject { get; set; }
        public string SenderEmail { get; set; }
        public string MailSenderName { get; set; }
        public long? MailNotificationAdapterId { get; set; }
        public bool? IsSMSEnabled { get; set; }
        public bool? IsIotEnabled { get; set; }
        public EpgNotificationSettings EpgNotification { get; set; }
        public LineupNotificationSettings LineupNotification { get; set; }

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

    public class EpgNotificationSettings
    {
        public bool Enabled { get; set; } = false;
        public IReadOnlyCollection<int> DeviceFamilyIds { get; set; } = new List<int>(0);
        public IReadOnlyCollection<long> LiveAssetIds { get; set; } = new List<long>(0);
        public int BackwardTimeRange { get; set; } = 24;
        public int ForwardTimeRange { get; set; } = 24;
    }

    public class LineupNotificationSettings
    {
        public bool Enabled { get; set; } = false;
    }
}
