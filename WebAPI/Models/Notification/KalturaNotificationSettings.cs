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
    [OldStandard("pushFollowEnabled", "push_follow_enabled")]
    public class KalturaNotificationsSettings : KalturaOTTObject
    {
        /// <summary>
        /// Specify if the user want to receive push notifications or not
        /// </summary>
        [DataMember(Name = "pushNotificationEnabled")]
        [JsonProperty("pushNotificationEnabled")]
        [XmlElement(ElementName = "pushNotificationEnabled", IsNullable = true)]
        public bool? PushNotificationEnabled { get; set; }

        /// <summary>
        /// Specify if the user will be notified for followed content via push. (requires push_notification_enabled to be enabled)
        /// </summary>
        [DataMember(Name = "pushFollowEnabled")]
        [JsonProperty("pushFollowEnabled")]
        [XmlElement(ElementName = "pushFollowEnabled", IsNullable = true)]
        public bool? PushFollowEnabled { get; set; }
    }


    public class KalturaNotificationSettings : KalturaNotificationsSettings
    {
    }
}