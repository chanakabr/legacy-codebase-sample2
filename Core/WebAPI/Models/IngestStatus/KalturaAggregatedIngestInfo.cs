using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaAggregatedIngestInfo : KalturaOTTObject
    {
        /// <summary>
        /// Number of results
        /// </summary>
        [DataMember(Name = "resultsCount")]
        [JsonProperty("resultsCount")]
        [XmlElement(ElementName = "resultsCount")]
        public long ResultsCount { get; set; }

        /// <summary>
        /// Number of results that include at least one error of severity TotalFailure
        /// </summary>
        [DataMember(Name = "totalFailureCount")]
        [JsonProperty("totalFailureCount")]
        [XmlElement(ElementName = "totalFailureCount")]
        public long TotalFailureCount { get; set; }

        /// <summary>
        /// Number of results that include no error with severity TotalFailure but at at least one error of severity PartialFailure
        /// </summary>
        [DataMember(Name = "partialFailureCount")]
        [JsonProperty("partialFailureCount")]
        [XmlElement(ElementName = "partialFailureCount")]
        public long PartialFailureCount { get; set; }

        /// <summary>
        /// Number of results that include at least one warning
        /// </summary>
        [DataMember(Name = "warningsCount")]
        [JsonProperty("warningsCount")]
        [XmlElement(ElementName = "warningsCount")]
        public long WarningCount { get; set; }
    }
}