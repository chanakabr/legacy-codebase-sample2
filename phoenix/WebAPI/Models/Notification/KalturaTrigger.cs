using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notifications
{
    [Serializable]
    public partial class KalturaTrigger : KalturaOTTObject
    {
     
    }

    public partial class KalturaDateTrigger : KalturaTrigger
    {
        /// <summary>
        /// Trigger date
        /// </summary>
        [DataMember(Name = "date")]
        [JsonProperty(PropertyName = "date")]
        [XmlElement(ElementName = "date")]
        public long Date { get; set; }
    }

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

    public enum KalturaSubscriptionTriggerType
    {
        START_DATE = 0,
        END_DATE = 1
    }
}