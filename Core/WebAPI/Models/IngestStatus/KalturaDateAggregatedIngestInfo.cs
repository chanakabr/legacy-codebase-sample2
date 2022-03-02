using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaDateAggregatedIngestInfo : KalturaOTTObject
    {
        /// <summary>
        /// 00:00 UTC of the date in question
        /// </summary>
        [DataMember(Name = "date")]
        [JsonProperty("date")]
        [XmlElement(ElementName = "date")]
        public long Date { get; set; }

        /// <summary>
        /// Aggregated error counters
        /// </summary>
        [DataMember(Name = "aggregatedErrors")]
        [JsonProperty("aggregatedErrors")]
        [XmlElement(ElementName = "aggregatedErrors")]
        public KalturaAggregatedIngestInfo AggregatedErrors { get; set; }
    }
}