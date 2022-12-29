using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.MultiRequest
{
    /// <summary>
    /// Skips current request according to aggregation condition on given property 
    /// </summary>
    public partial class KalturaAggregatedPropertySkipCondition : KalturaPropertySkipCondition
    {
        /// <summary>
        /// The aggregation type on which the condition is based on
        /// </summary>
        [DataMember(Name = "aggregationType")]
        [JsonProperty("aggregationType")]
        [XmlElement(ElementName = "aggregationType")]
        public KalturaAggregationType AggregationType { get; set; }
    }
}