using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaIngestEpgDetails : KalturaIngestEpg
    {
        /// <summary>
        /// Errors
        /// </summary>
        [DataMember(Name = "errors")]
        [JsonProperty("errors")]
        [XmlElement(ElementName = "errors")]
        public List<KalturaEpgIngestErrorMessage> Errors { get; set; }

        /// <summary>
        /// Aggregated counters
        /// </summary>
        [DataMember(Name = "aggregations")]
        [JsonProperty("aggregations")]
        [XmlElement(ElementName = "aggregations")]
        public KalturaIngestEpgDetailsAggregation Aggregations { get; set; }
    }
}