using ApiObjects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// User Interest
    /// </summary>
    public class KalturaUserInterest : KalturaOTTObject
    {
        /// <summary>
        /// Meta identifier
        /// </summary>
        [DataMember(Name = "metaId")]
        [JsonProperty("metaId")]
        [XmlElement(ElementName = "metaId")]
        public string MetaId { get; set; }

        /// <summary>
        /// Topic
        /// </summary>
        [DataMember(Name = "topic")]
        [JsonProperty("topic")]
        [XmlElement(ElementName = "topic")]
        public KalturaUserInterestTopic Topic { get; set; }
    }

    /// <summary>
    /// User interest topic
    /// </summary>
    public class KalturaUserInterestTopic
    {
        /// <summary>
        /// Meta identifier
        /// </summary>
        [DataMember(Name = "metaId")]
        [JsonProperty("metaId")]
        [XmlElement(ElementName = "metaId")]
        public string Value { get; set; }

        /// <summary>
        /// Parent topic
        /// </summary>
        [DataMember(Name = "parentTopic")]
        [JsonProperty("parentTopic")]
        [XmlElement(ElementName = "parentTopic")]
        public KalturaUserInterestTopic ParentTopic { get; set; }
    }
}