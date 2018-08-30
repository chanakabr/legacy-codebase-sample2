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
    /// Segmentation type which takes all values of a tag/meta as segments
    /// </summary>
    public partial class KalturaSegmentAllValues : KalturaSegmentValues
    {
        /// <summary>
        /// Segment names' format - they will be automatically generated
        /// </summary>
        [DataMember(Name = "nameFormat")]
        [JsonProperty(PropertyName = "nameFormat")]
        [XmlElement(ElementName = "nameFormat")]
        [SchemeProperty()]
        public string NameFormat { get; set; }
    }
}