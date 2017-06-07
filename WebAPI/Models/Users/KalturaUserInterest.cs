using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// User Interest
    /// </summary>
    public class KalturaUserInterest : KalturaOTTObject
    {
        /// <summary>
        /// Identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }        

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
    public class KalturaUserInterestTopic : KalturaOTTObject
    {
        /// <summary>
        /// Meta identifier
        /// </summary>
        [DataMember(Name = "metaId")]
        [JsonProperty("metaId")]
        [XmlElement(ElementName = "metaId")]
        public string MetaId { get; set; }

        /// <summary>
        /// Meta Value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value")]
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