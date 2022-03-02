using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{

    /// <summary>
    /// Segment that is based on a range of values
    /// </summary>
    public partial class KalturaSegmentRange : KalturaOTTObject
    {
        /// <summary>
        /// Id of segment
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Systematic name of segment
        /// </summary>
        [DataMember(Name = "systematicName")]
        [JsonProperty(PropertyName = "systematicName")]
        [XmlElement(ElementName = "systematicName")]
        [SchemeProperty()]
        public string SystematicName { get; set; }


        /// <summary>
        /// Specific segment name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty()]
        public string Name { get; set; }

        /// <summary>
        /// Greater than or equals >=
        /// </summary>
        [DataMember(Name = "gte")]
        [JsonProperty(PropertyName = "gte")]
        [XmlElement(ElementName = "gte")]
        [SchemeProperty()]
        public double? GreaterThanOrEquals { get; set; }

        /// <summary>
        /// Greater than >
        /// </summary>
        [DataMember(Name = "gt")]
        [JsonProperty(PropertyName = "gt")]
        [XmlElement(ElementName = "gt")]
        [SchemeProperty()]
        public double? GreaterThan { get; set; }

        /// <summary>
        /// Less than or equals
        /// </summary>
        [DataMember(Name = "lte")]
        [JsonProperty(PropertyName = "lte")]
        [XmlElement(ElementName = "lte")]
        [SchemeProperty()]
        public double? LessThanOrEquals { get; set; }

        /// <summary>
        /// Less than 
        /// </summary>
        [DataMember(Name = "lt")]
        [JsonProperty(PropertyName = "lt")]
        [XmlElement(ElementName = "lt")]
        [SchemeProperty()]
        public double? LessThan { get; set; }

        /// <summary>
        /// Equals 
        /// </summary>
        [DataMember(Name = "equals")]
        [JsonProperty(PropertyName = "equals")]
        [XmlElement(ElementName = "equals")]
        [SchemeProperty()]
        public double? Equals { get; set; }
    }
}
