using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
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
        /// The backward range (in hours), in which, EPG updates triggers a notification,
        /// every program that is updated and it’s starts time falls within this range shall trigger a notification
        /// </summary>
        [DataMember(Name = "backwardTimeRange")]
        [JsonProperty("backwardTimeRange")]
        [XmlElement(ElementName = "backwardTimeRange")]
        [SchemeProperty(MinInteger = 0, MaxInteger = 72)]
        public int? BackwardTimeRange { get; set; }

        /// <summary>
        /// The forward range (in hours), in which, EPG updates triggers a notification,
        /// every program that is updated and it’s starts time falls within this range shall trigger a notification
        /// </summary>
        [DataMember(Name = "forwardTimeRange")]
        [JsonProperty("forwardTimeRange")]
        [XmlElement(ElementName = "forwardTimeRange")]
        [SchemeProperty(MinInteger = 0, MaxInteger = 72)]
        public int? ForwardTimeRange { get; set; }
    }
}