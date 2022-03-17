using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace WebAPI.Models.Notifications
{
    public partial class KalturaSubscriptionTrigger : KalturaTrigger
    {
        /// <summary>
        /// Subscription trigger type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public KalturaSubscriptionTriggerType Type { get; set; }

        /// <summary>
        /// Subscription trigger offset
        /// </summary>
        [DataMember(Name = "offset")]
        [JsonProperty(PropertyName = "offset")]
        [XmlElement(ElementName = "offset")]
        public long Offset { get; set; }
    }
}