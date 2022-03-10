using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Monetization based source (purchases etc.)
    /// </summary>
    public partial class KalturaMonetizationSource : KalturaSegmentSource
    {
        /// <summary>
        /// Purchase type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        [SchemeProperty()]
        public KalturaMonetizationType Type { get; set; }

        /// <summary>
        /// Mathermtical operator to calculate
        /// </summary>
        [DataMember(Name = "operator")]
        [JsonProperty(PropertyName = "operator")]
        [XmlElement(ElementName = "operator")]
        [SchemeProperty()]
        public KalturaMathemticalOperatorType Operator { get; set; }

        /// <summary>
        /// Days to consider when checking the users' purchaes
        /// </summary>
        [DataMember(Name = "days")]
        [JsonProperty(PropertyName = "days")]
        [XmlElement(ElementName = "days")]
        [SchemeProperty()]
        public int Days { get; set; }
    }
}
