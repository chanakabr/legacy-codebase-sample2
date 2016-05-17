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
    [OldStandard("pushFollowEnabled", "push_follow_enabled")]
    public class KalturaNotificationSettings : KalturaOTTObject
    {
        /// <summary>
        /// push notification enabled
        /// </summary>
        [DataMember(Name = "pushNotificationEnabled")]
        [JsonProperty("pushNotificationEnabled")]
        [XmlElement(ElementName = "pushNotificationEnabled", IsNullable = true)]
        public bool? PushNotificationEnabled { get; set; }

        /// <summary>
        /// push follow enabled
        /// </summary>
        [DataMember(Name = "pushFollowEnabled")]
        [JsonProperty("pushFollowEnabled")]
        [XmlElement(ElementName = "pushFollowEnabled", IsNullable = true)]
        public bool? PushFollowEnabled { get; set; }
    }
}