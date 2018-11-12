using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Defines which sort is relevant to the condition or the segmentation
    /// </summary>
    public partial class KalturaSegmentSource : KalturaOTTObject
    {

    }

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

    /// <summary>
    /// Content based source (meta, tag etc.)
    /// </summary>
    public partial class KalturaContentSource : KalturaSegmentSource
    {
        /// <summary>
        /// Topic (meta or tag) name
        /// </summary>
        [DataMember(Name = "field")]
        [JsonProperty(PropertyName = "field")]
        [XmlElement(ElementName = "field")]
        [SchemeProperty()]
        public string Field { get; set; }
    }

    /// <summary>
    /// User dynamic data source
    /// </summary>
    public partial class KalturaUserDynamicDataSource : KalturaSegmentSource
    {
        /// <summary>
        /// Field name
        /// </summary>
        [DataMember(Name = "field")]
        [JsonProperty(PropertyName = "field")]
        [XmlElement(ElementName = "field")]
        [SchemeProperty()]
        public string Field { get; set; }
    }
}