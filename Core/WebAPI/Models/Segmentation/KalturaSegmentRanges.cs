using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Segmentation type that takes different ranges as segments
    /// </summary>
    public partial class KalturaSegmentRanges : KalturaBaseSegmentValue
    {
        /// <summary>
        /// Range source
        /// </summary>
        [DataMember(Name = "source")]
        [JsonProperty(PropertyName = "source")]
        [XmlElement(ElementName = "source")]
        [SchemeProperty()]
        public KalturaSegmentSource Source { get; set; }

        /// <summary>
        /// List of ranges for segmentation
        /// </summary>
        [DataMember(Name = "ranges", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "ranges", NullValueHandling = NullValueHandling.Ignore)]
        [XmlArray(ElementName = "values", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty()]
        public List<KalturaSegmentRange> Ranges { get; set; }
    }
}