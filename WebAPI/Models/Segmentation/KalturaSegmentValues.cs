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
        [SchemeProperty()]
        public long Id { get; set; }

        /// <summary>
        /// Name of segment
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty()]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// The value of the segment
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty(PropertyName = "value")]
        [XmlElement(ElementName = "value")]
        [SchemeProperty()]
        public string Value { get; set; }

        /// <summary>
        /// Threshold - minimum score to be met for this specific value
        /// </summary>
        [DataMember(Name = "threshold")]
        [JsonProperty(PropertyName = "threshold")]
        [XmlElement(ElementName = "threshold")]
        [SchemeProperty()]
        public int Threshold { get; set; }
    }

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
        /// Threshold - minimum score to be met for all values in general (can be overriden)
        /// </summary>
        [DataMember(Name = "threshold")]
        [JsonProperty(PropertyName = "threshold")]
        [XmlElement(ElementName = "threshold")]
        [SchemeProperty()]
        public int Threshold { get; set; }

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