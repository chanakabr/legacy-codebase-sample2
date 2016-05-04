using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    public class KalturaPartnerNotificationSettings : KalturaOTTObject
    {
        /// <summary>
        /// push notification enabled
        /// </summary>
        [DataMember(Name = "push_notification_enabled")]
        [JsonProperty("push_notification_enabled")]
        [XmlElement(ElementName = "push_notification_enabled",  IsNullable = true)]
        public bool? PushNotificationEnabled { get; set; }

        /// <summary>
        /// push system announcements enabled
        /// </summary>
        [DataMember(Name = "push_system_announcements_enabled")]
        [JsonProperty("push_system_announcements_enabled")]
        [XmlElement(ElementName = "push_system_announcements_enabled", IsNullable = true)]
        public bool? PushSystemAnnouncementsEnabled { get; set; }

        /// <summary>
        /// push start hour
        /// </summary>
        [DataMember(Name = "push_start_hour")]
        [JsonProperty("push_start_hour")]
        [XmlElement(ElementName = "push_start_hour", IsNullable = true)]
        public int? PushStartHour { get; set; }

        /// <summary>
        /// push end hour
        /// </summary>
        [DataMember(Name = "push_end_hour")]
        [JsonProperty("push_end_hour")]
        [XmlElement(ElementName = "push_end_hour", IsNullable = true)]
        public int? PushEndHour { get; set; }
    }
}
