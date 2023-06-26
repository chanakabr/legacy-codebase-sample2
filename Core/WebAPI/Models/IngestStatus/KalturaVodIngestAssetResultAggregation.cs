using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaVodIngestAssetResultAggregation : KalturaOTTObject
    {
        /// <summary>
        /// Ingest date of the first asset in the response list. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "ingestDateFrom")]
        [JsonProperty("ingestDateFrom")]
        [XmlElement(ElementName = "ingestDateFrom")]
        public long? IngestDateFrom { get; set; }

        /// <summary>
        /// Ingest date of the last asset in the response list. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "ingestDateTo")]
        [JsonProperty("ingestDateTo")]
        [XmlElement(ElementName = "ingestDateTo")]
        public long? IngestDateTo { get; set; }

        /// <summary>
        /// Number of assets which failed ingest. Calculated on the assets returned according to the applied filters.
        /// </summary>
        [DataMember(Name = "failureCount")]
        [JsonProperty("failureCount")]
        [XmlElement(ElementName = "failureCount")]
        public int FailureCount { get; set; }

        /// <summary>
        /// Number of assets which succeeded ingest without any warning. Calculated on the assets returned according to the applied filters.
        /// </summary>
        [DataMember(Name = "successCount")]
        [JsonProperty("successCount")]
        [XmlElement(ElementName = "successCount")]
        public int SuccessCount { get; set; }

        /// <summary>
        /// Number of files (not assets) which failed ingest and are reported by external none-WS_ingest entity. Calculated on the failed files returned according to the applied filters.
        /// </summary>
        [DataMember(Name = "externalFailureCount")]
        [JsonProperty("externalFailureCount")]
        [XmlElement(ElementName = "externalFailureCount")]
        public int ExternalFailureCount { get; set; }

        /// <summary>
        /// Number of assets which succeeded ingest, but with warnings. Calculated on the assets returned according to the applied filters.
        /// </summary>
        [DataMember(Name = "successWithWarningCount")]
        [JsonProperty("successWithWarningCount")]
        [XmlElement(ElementName = "successWithWarningCount")]
        public int SuccessWithWarningCount { get; set; }
    }
}