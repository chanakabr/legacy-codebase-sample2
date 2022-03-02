using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaChannelAggregatedIngestInfo : KalturaOTTObject
    {
        /// <summary>
        /// The linear channel asset id
        /// </summary>
        [DataMember(Name = "linearChannelId")]
        [JsonProperty("linearChannelId")]
        [XmlElement(ElementName = "linearChannelId")]
        public long LinearChannelId { get; set; }

        /// <summary>
        /// Aggregated error counters
        /// </summary>
        [DataMember(Name = "aggregatedErrors")]
        [JsonProperty("aggregatedErrors")]
        [XmlElement(ElementName = "aggregatedErrors")]
        public KalturaAggregatedIngestInfo AggregatedErrors { get; set; }
    }
}