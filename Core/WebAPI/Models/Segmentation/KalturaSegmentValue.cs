using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Specific segment value
    /// </summary>
    public partial class KalturaSegmentValue : KalturaOTTObject
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
        /// Name of segment
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty()]
        public string Name { get; set; }

        /// <summary>
        /// The value of the segment
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty(PropertyName = "value")]
        [XmlElement(ElementName = "value")]
        [SchemeProperty()]
        public string Value { get; set; }
    }
}
