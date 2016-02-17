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
    }
}
