using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaIngestEpgDetailsAggregation : KalturaOTTObject
    {
        /// <summary>
        /// Array of aggregated information per channel that included in the ingest job in question
        /// </summary>
        [DataMember(Name = "linearChannels")]
        [JsonProperty("linearChannels")]
        [XmlElement(ElementName = "linearChannels")]
        public List<KalturaChannelAggregatedIngestInfo> LinearChannels { get; set; }

        /// <summary>
        /// Array of aggregated information per date that included in the ingest job in question
        /// </summary>
        [DataMember(Name = "dates")]
        [JsonProperty("dates")]
        [XmlElement(ElementName = "dates")]
        public List<KalturaDateAggregatedIngestInfo> Dates { get; set; }

        /// <summary>
        /// All aggregated counters
        /// </summary>
        [DataMember(Name = "all")]
        [JsonProperty("all")]
        [XmlElement(ElementName = "all")]
        public KalturaAggregatedIngestInfo All { get; set; }
    }
}