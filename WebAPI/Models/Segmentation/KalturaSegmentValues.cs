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