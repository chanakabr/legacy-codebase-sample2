using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Asset order segment action
    /// </summary>
    public partial class KalturaAssetOrderSegmentAction : KalturaBaseSegmentAction
    {
        /// <summary>
        /// Action name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Action values
        /// </summary>
        [DataMember(Name = "values")]
        [JsonProperty(PropertyName = "values")]
        [XmlElement(ElementName = "values")]
        public List<KalturaStringValue> Values { get; set; }
    }
}