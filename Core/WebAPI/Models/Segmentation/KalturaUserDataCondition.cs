using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// User data condition for segmentation
    /// </summary>
    public partial class KalturaUserDataCondition : KalturaBaseSegmentCondition
    {
        /// <summary>
        /// Field name
        /// </summary>
        [DataMember(Name = "field")]
        [JsonProperty(PropertyName = "field")]
        [XmlElement(ElementName = "field")]
        [SchemeProperty()]
        public string Field { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty(PropertyName = "value")]
        [XmlElement(ElementName = "value")]
        [SchemeProperty()]
        public string Value { get; set; }
    }
}