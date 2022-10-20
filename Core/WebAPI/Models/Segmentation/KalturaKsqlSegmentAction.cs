using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Segment action with ksql
    /// </summary>
    [SchemeClass(Required = new[] { "ksql" })]
    public abstract partial class KalturaKsqlSegmentAction : KalturaBaseSegmentAction
    {
        /// <summary>
        /// KSQL
        /// </summary>
        [DataMember(Name = "ksql")]
        [JsonProperty(PropertyName = "ksql")]
        [XmlElement(ElementName = "ksql")]
        [SchemeProperty(MinLength = 1)]
        public string KSQL { get; set; }
    }
}