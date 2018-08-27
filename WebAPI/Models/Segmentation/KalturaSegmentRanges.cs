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
    /// Segment that is based on a range of values
    /// </summary>
    public partial class KalturaSegmentRange : KalturaOTTObject
    {
        /// <summary>
        /// Specific segment name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty()]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// Greater than or equals >=
        /// </summary>
        [DataMember(Name = "gte")]
        [JsonProperty(PropertyName = "gte")]
        [XmlElement(ElementName = "gte")]
        [SchemeProperty()]
        public double GreaterThanOrEquals { get; set; }

        /// <summary>
        /// Greater than >
        /// </summary>
        [DataMember(Name = "gt")]
        [JsonProperty(PropertyName = "gt")]
        [XmlElement(ElementName = "gt")]
        [SchemeProperty()]
        public double GreaterThan { get; set; }

        /// <summary>
        /// Less than or equals
        /// </summary>
        [DataMember(Name = "lte")]
        [JsonProperty(PropertyName = "lte")]
        [XmlElement(ElementName = "lte")]
        [SchemeProperty()]
        public double LessThanOrEquals { get; set; }

        /// <summary>
        /// Less than 
        /// </summary>
        [DataMember(Name = "lt")]
        [JsonProperty(PropertyName = "lt")]
        [XmlElement(ElementName = "lt")]
        [SchemeProperty()]
        public double LessThan { get; set; }
    }

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