using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    [OldStandard("pushNotificationEnabled", "push_notification_enabled")]
    [OldStandard("pushSystemAnnouncementsEnabled", "push_system_announcements_enabled")]
    [OldStandard("pushStartHour", "push_start_hour")]
    [OldStandard("pushEndHour", "push_end_hour")]
    public class KalturaNotificationsPartnerSettings : KalturaOTTObject
    {
        /// <summary>
        /// Push notification capability is enabled for the account
        /// </summary>
        [DataMember(Name = "pushNotificationEnabled")]
        [JsonProperty("pushNotificationEnabled")]
        [XmlElement(ElementName = "pushNotificationEnabled", IsNullable = true)]
        public bool? PushNotificationEnabled { get; set; }

        /// <summary>
        /// System announcement capability is enabled for the account
        /// </summary>
        [DataMember(Name = "pushSystemAnnouncementsEnabled")]
        [JsonProperty("pushSystemAnnouncementsEnabled")]
        [XmlElement(ElementName = "pushSystemAnnouncementsEnabled", IsNullable = true)]
        public bool? PushSystemAnnouncementsEnabled { get; set; }

        /// <summary>
        /// Window start time (UTC) for send automated push messages
        /// </summary>
        [DataMember(Name = "pushStartHour")]
        [JsonProperty("pushStartHour")]
        [XmlElement(ElementName = "pushStartHour", IsNullable = true)]
        [SchemeProperty(MinInteger = 0, MaxInteger = 24)]
        public int? PushStartHour { get; set; }

        /// <summary>
        /// Window end time (UTC) for send automated push messages
        /// </summary>
        [DataMember(Name = "pushEndHour")]
        [JsonProperty("pushEndHour")]
        [XmlElement(ElementName = "pushEndHour", IsNullable = true)]
        [SchemeProperty(MinInteger = 0, MaxInteger = 24)]
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

    }

    [Obsolete]
    public class KalturaPartnerNotificationSettings : KalturaNotificationsPartnerSettings
    {
    }
}
