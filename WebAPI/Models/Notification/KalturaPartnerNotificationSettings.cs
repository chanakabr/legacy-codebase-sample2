using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    [OldStandard("pushNotificationEnabled", "push_notification_enabled")]
    [OldStandard("pushSystemAnnouncementsEnabled", "push_system_announcements_enabled")]
    [OldStandard("pushStartHour", "push_start_hour")]
    [OldStandard("pushEndHour", "push_end_hour")]
    public class KalturaPartnerNotificationSettings : KalturaOTTObject
    {
        /// <summary>
        /// push notification enabled
        /// </summary>
        [DataMember(Name = "pushNotificationEnabled")]
        [JsonProperty("pushNotificationEnabled")]
        [XmlElement(ElementName = "pushNotificationEnabled", IsNullable = true)]
        public bool? PushNotificationEnabled { get; set; }

        /// <summary>
        /// push system announcements enabled
        /// </summary>
        [DataMember(Name = "pushSystemAnnouncementsEnabled")]
        [JsonProperty("pushSystemAnnouncementsEnabled")]
        [XmlElement(ElementName = "pushSystemAnnouncementsEnabled", IsNullable = true)]
        public bool? PushSystemAnnouncementsEnabled { get; set; }

        /// <summary>
        /// push start hour
        /// </summary>
        [DataMember(Name = "pushStartHour")]
        [JsonProperty("pushStartHour")]
        [XmlElement(ElementName = "pushStartHour", IsNullable = true)]
        public int? PushStartHour { get; set; }

        /// <summary>
        /// push end hour
        /// </summary>
        [DataMember(Name = "pushEndHour")]
        [JsonProperty("pushEndHour")]
        [XmlElement(ElementName = "pushEndHour", IsNullable = true)]
        public int? PushEndHour { get; set; }

        /// <summary>
        /// Inbox enabled
        /// </summary>
        [DataMember(Name = "inboxEnabled")]
        [JsonProperty("inboxEnabled")]
        [XmlElement(ElementName = "inboxEnabled", IsNullable = true)]
        public bool? InboxEnabled { get; set; }

        /// <summary>
        /// Inbox enabled
        /// </summary>
        [DataMember(Name = "messageTTL")]
        [JsonProperty("messageTTL")]
        [XmlElement(ElementName = "messageTTL", IsNullable = true)]
        public int? MessageTTL { get; set; }
    }
}
