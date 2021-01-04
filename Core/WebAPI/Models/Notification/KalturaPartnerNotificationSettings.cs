using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    public partial class KalturaNotificationsPartnerSettings : KalturaOTTObject
    {
        /// <summary>
        /// Push notification capability is enabled for the account
        /// </summary>
        [DataMember(Name = "pushNotificationEnabled")]
        [JsonProperty("pushNotificationEnabled")]
        [XmlElement(ElementName = "pushNotificationEnabled", IsNullable = true)]
        [OldStandardProperty("push_notification_enabled")]
        public bool? PushNotificationEnabled { get; set; }

        /// <summary>
        /// System announcement capability is enabled for the account
        /// </summary>
        [DataMember(Name = "pushSystemAnnouncementsEnabled")]
        [JsonProperty("pushSystemAnnouncementsEnabled")]
        [XmlElement(ElementName = "pushSystemAnnouncementsEnabled", IsNullable = true)]
        [OldStandardProperty("push_system_announcements_enabled")]
        public bool? PushSystemAnnouncementsEnabled { get; set; }

        /// <summary>
        /// Window start time (UTC) for send automated push messages
        /// </summary>
        [DataMember(Name = "pushStartHour")]
        [JsonProperty("pushStartHour")]
        [XmlElement(ElementName = "pushStartHour", IsNullable = true)]
        [SchemeProperty(MinInteger = 0, MaxInteger = 24)]
        [OldStandardProperty("push_start_hour")]
        public int? PushStartHour { get; set; }

        /// <summary>
        /// Window end time (UTC) for send automated push messages
        /// </summary>
        [DataMember(Name = "pushEndHour")]
        [JsonProperty("pushEndHour")]
        [XmlElement(ElementName = "pushEndHour", IsNullable = true)]
        [SchemeProperty(MinInteger = 0, MaxInteger = 24)]
        [OldStandardProperty("push_end_hour")]
        public int? PushEndHour { get; set; }

        /// <summary>
        /// Inbox enabled
        /// </summary>
        [DataMember(Name = "inboxEnabled")]
        [JsonProperty("inboxEnabled")]
        [XmlElement(ElementName = "inboxEnabled", IsNullable = true)]
        public bool? InboxEnabled { get; set; }

        /// <summary>
        /// Message TTL in days
        /// </summary>
        [DataMember(Name = "messageTTLDays")]
        [JsonProperty("messageTTLDays")]
        [XmlElement(ElementName = "messageTTLDays", IsNullable = true)]
        [SchemeProperty(MinInteger = 1, MaxInteger = 90)]
        public int? MessageTTLDays { get; set; }

        /// <summary>
        /// Automatic issue follow notification
        /// </summary>
        [DataMember(Name = "automaticIssueFollowNotification")]
        [JsonProperty("automaticIssueFollowNotification")]
        [XmlElement(ElementName = "automaticIssueFollowNotification", IsNullable = true)]
        public bool? AutomaticIssueFollowNotification { get; set; }

        /// <summary>
        /// Topic expiration duration in days
        /// </summary>
        [DataMember(Name = "topicExpirationDurationDays")]
        [JsonProperty("topicExpirationDurationDays")]
        [XmlElement(ElementName = "topicExpirationDurationDays", IsNullable = true)]
        public int? TopicExpirationDurationDays { get; set; }

        /// <summary>
        /// Reminder enabled
        /// </summary>
        [DataMember(Name = "reminderEnabled")]
        [JsonProperty("reminderEnabled")]
        [XmlElement(ElementName = "reminderEnabled", IsNullable = true)]
        public bool? ReminderEnabled { get; set; }

        /// <summary>
        /// Offset time (UTC) in seconds for send reminder
        /// </summary>
        [DataMember(Name = "reminderOffsetSec")]
        [JsonProperty("reminderOffsetSec")]
        [XmlElement(ElementName = "reminderOffsetSec", IsNullable = true)]
        [SchemeProperty(MinInteger = 0, MaxInteger = 3600)]
        public int? ReminderOffset { get; set; }

        /// <summary>
        /// Push adapter URL
        /// </summary>
        [DataMember(Name = "pushAdapterUrl")]
        [JsonProperty("pushAdapterUrl")]
        [XmlElement(ElementName = "pushAdapterUrl", IsNullable = true)]
        public string PushAdapterUrl { get; set; }

        /// <summary>
        /// Churn mail template name
        /// </summary>
        [DataMember(Name = "churnMailTemplateName")]
        [JsonProperty("churnMailTemplateName")]
        [XmlElement(ElementName = "churnMailTemplateName", IsNullable = true)]
        public string ChurnMailTemplateName { get; set; }

        /// <summary>
        /// Churn mail subject
        /// </summary>
        [DataMember(Name = "churnMailSubject")]
        [JsonProperty("churnMailSubject")]
        [XmlElement(ElementName = "churnMailSubject", IsNullable = true)]
        public string ChurnMailSubject { get; set; }


        /// <summary>
        /// Sender email
        /// </summary>
        [DataMember(Name = "senderEmail")]
        [JsonProperty("senderEmail")]
        [XmlElement(ElementName = "senderEmail", IsNullable = true)]
        public string SenderEmail { get; set; }


        /// <summary>
        /// Mail sender name
        /// </summary>
        [DataMember(Name = "mailSenderName")]
        [JsonProperty("mailSenderName")]
        [XmlElement(ElementName = "mailSenderName", IsNullable = true)]
        public string MailSenderName { get; set; }

        /// <summary>
        /// Mail notification adapter identifier
        /// </summary>
        [DataMember(Name = "mailNotificationAdapterId")]
        [JsonProperty("mailNotificationAdapterId")]
        [XmlElement(ElementName = "mailNotificationAdapterId", IsNullable = true)]
        public long? MailNotificationAdapterId { get; set; }

        /// <summary>
        /// SMS capability is enabled for the account
        /// </summary>
        [DataMember(Name = "smsEnabled")]
        [JsonProperty("smsEnabled")]
        [XmlElement(ElementName = "smsEnabled", IsNullable = true)]
        public bool? SmsEnabled { get; set; }

        /// <summary>
        /// IOT capability is enabled for the account
        /// </summary>
        [DataMember(Name = "iotEnabled")]
        [JsonProperty("iotEnabled")]
        [XmlElement(ElementName = "iotEnabled", IsNullable = true)]
        public bool? IotEnabled { get; set; }

        /// <summary>
        /// Settings for epg notifications
        /// </summary>
        [DataMember(Name = "epgNotification")]
        [JsonProperty("epgNotification")]
        [XmlElement(ElementName = "epgNotification", IsNullable = true)]
        public KalturaEpgNotificationSettings EpgNotification { get; set; }
    }

    public partial class KalturaEpgNotificationSettings : KalturaOTTObject
    {
        /// <summary>
        /// EPG notification capability is enabled for the account
        /// </summary>
        [DataMember(Name = "enabled")]
        [JsonProperty("enabled")]
        [XmlElement(ElementName = "enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Specify which devices should receive notifications
        /// </summary>
        [DataMember(Name = "deviceFamilyIds")]
        [JsonProperty("deviceFamilyIds")]
        [XmlElement(ElementName = "deviceFamilyIds")]
        public string DeviceFamilyIds { get; set; }

        /// <summary>
        /// Specify which live assets should fire notifications
        /// </summary>
        [DataMember(Name = "liveAssetIds")]
        [JsonProperty("liveAssetIds")]
        [XmlElement(ElementName = "liveAssetIds")]
        public string LiveAssetIds { get; set; }

        /// <summary>
        /// The range (in hours), in which, EPG updates triggers a notification,
        /// every program that is updated and it’s starts time falls within this range shall trigger a notification
        /// </summary>
        [DataMember(Name = "timeRange")]
        [JsonProperty("timeRange")]
        [XmlElement(ElementName = "timeRange")]
        [SchemeProperty(MinInteger = 6, MaxInteger = 72)]
        public int TimeRange { get; set; }
    }


    [Obsolete]
    public partial class KalturaPartnerNotificationSettings : KalturaNotificationsPartnerSettings
    {
    }
}
