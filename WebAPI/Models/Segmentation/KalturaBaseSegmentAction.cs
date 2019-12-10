using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Base class that defines segment action
    /// </summary>
    public partial class KalturaBaseSegmentAction : KalturaOTTObject
    {
    }

    /// <summary>
    /// Asset order segment action
    /// </summary>
    public partial class KalturaAssetOrderSegmentAction : KalturaBaseSegmentAction
    {
        /// <summary>
        /// Action name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Action values
        /// </summary>
        [DataMember(Name = "values")]
        [JsonProperty(PropertyName = "values")]
        [XmlElement(ElementName = "values")]
        public List<KalturaStringValue> Values { get; set; }
    }

    /// <summary>
    /// Block playback action
    /// </summary>
    public partial class KalturaBlockPlaybackSegmentAction : KalturaBaseSegmentAction
    {
        /// <summary>
        /// KSQL
        /// </summary>
        [DataMember(Name = "ksql")]
        [JsonProperty(PropertyName = "ksql")]
        [XmlElement(ElementName = "ksql")]
        public string KSQL { get; set; }

        /// <summary>
        /// Block playback type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public KalturaBlockPlaybackType Type { get; set; }        
    }

    /// <summary>
    /// Block playback type
    /// </summary>
    public enum KalturaBlockPlaybackType
    {
        /// <summary>
        /// subscription
        /// </summary>
        Subscription,
        /// <summary>
        /// ppv
        /// </summary>
        PPV,
        /// <summary>
        /// boxet
        /// </summary>
        Boxet
    }
}