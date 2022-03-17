using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Segment action with ksql
    /// </summary>
    public abstract partial class KalturaKsqlSegmentAction : KalturaBaseSegmentAction
    {
        /// <summary>
        /// KSQL
        /// </summary>
        [DataMember(Name = "ksql")]
        [JsonProperty(PropertyName = "ksql")]
        [XmlElement(ElementName = "ksql")]
        public string KSQL { get; set; }
    }
}