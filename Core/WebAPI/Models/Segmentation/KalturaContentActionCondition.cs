using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Segmentation condition regarding content actions
    /// </summary>
    public partial class KalturaContentActionCondition : KalturaOTTObject
    {
        /// <summary>
        /// The relevant action to be examined
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty(PropertyName = "action")]
        [XmlElement(ElementName = "action")]
        public KalturaContentAction Action { get; set; }

        /// <summary>
        /// Optional - if action required specific length to be considered (in percentage or minutes)
        /// </summary>
        [DataMember(Name = "length")]
        [JsonProperty(PropertyName = "length")]
        [XmlElement(ElementName = "length")]
        [SchemeProperty(IsNullable = true)]
        public int? Length { get; set; }

        /// <summary>
        /// Optional - if action required specific length to be considered (in percentage or minutes)
        /// </summary>
        [DataMember(Name = "lengthType")]
        [JsonProperty(PropertyName = "lengthType")]
        [XmlElement(ElementName = "lengthType")]
        [SchemeProperty(IsNullable = true)]
        public KalturaContentActionConditionLengthType? LengthType { get; set; }

        /// <summary>
        /// Score multiplier - how much is a single action worth when considering the action
        /// </summary>
        [DataMember(Name = "multiplier")]
        [JsonProperty(PropertyName = "multiplier")]
        [XmlElement(ElementName = "multiplier")]
        public int Multiplier { get; set; }
    }
}