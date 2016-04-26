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
    public class KalturaNotificationSettings : KalturaOTTObject
    {
        /// <summary>
        /// push notification enabled
        /// </summary>
        [DataMember(Name = "push_notification_enabled")]
        [JsonProperty("push_notification_enabled")]
        [XmlElement(ElementName = "push_notification_enabled", IsNullable = true)]
        public bool? PushNotificationEnabled { get; set; }

        /// <summary>
        /// push follow enabled
        /// </summary>
        [DataMember(Name = "push_follow_enabled")]
        [JsonProperty("push_follow_enabled")]
        [XmlElement(ElementName = "push_follow_enabled", IsNullable = true)]
        public bool? PushFollowEnabled { get; set; }
    }
}