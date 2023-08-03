using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaVodIngestAssetResultResponse : KalturaOTTObject
    {
        /// <summary>
        /// Errors
        /// </summary>
        [DataMember(Name = "result")]
        [JsonProperty("result")]
        [XmlElement(ElementName = "result")]
        public KalturaVodIngestAssetResultList Result { get; set; }

        /// <summary>
        /// Aggregated counters
        /// </summary>
        [DataMember(Name = "aggregations")]
        [JsonProperty("aggregations")]
        [XmlElement(ElementName = "aggregations")]
        public KalturaVodIngestAssetResultAggregation Aggregations { get; set; }
    }
}
