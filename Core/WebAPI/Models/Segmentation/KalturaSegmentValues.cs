using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Segmentation type which takes certain values of a tag/meta as segments
    /// </summary>
    public partial class KalturaSegmentValues : KalturaBaseSegmentValue
    {
        /// <summary>
        /// Segment values source
        /// </summary>
        [DataMember(Name = "source")]
        [JsonProperty(PropertyName = "source")]
        [XmlElement(ElementName = "source")]
        [SchemeProperty()]
        public KalturaSegmentSource Source { get; set; }
        
        /// <summary>
        /// List of segment values
        /// </summary>
        [DataMember(Name = "values", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "values", NullValueHandling = NullValueHandling.Ignore)]
        [XmlArray(ElementName = "values", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty()]
        public List<KalturaSegmentValue> Values { get; set; }
    }
}