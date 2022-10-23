using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Asset order segment action
    /// </summary>
    [SchemeClass(Required = new[] { "name", "values" })]
    public partial class KalturaAssetOrderSegmentAction : KalturaBaseSegmentAction
    {
        /// <summary>
        /// Action name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(MinLength = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Action values
        /// </summary>
        [DataMember(Name = "values")]
        [JsonProperty(PropertyName = "values")]
        [XmlElement(ElementName = "values")]
        [SchemeProperty(MinItems = 1)]
        public List<KalturaStringValue> Values { get; set; }
    }
}