using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.IngestStatus
{
    public abstract partial class KalturaIngestProgramResultsByRefineFilter : KalturaIngestEpgProgramResultFilter
    {
        /// <summary>
        /// Comma seperated valid statuses - only 'FAILURE', 'WARNING' and 'SUCCESS' are valid strings. No repetitions are allowed.
        /// </summary>
        [DataMember(Name = "ingestStatusIn")]
        [JsonProperty("ingestStatusIn")]
        [XmlElement(ElementName = "ingestStatusIn")]
        [SchemeProperty(IsNullable = true)]
        public string IngestStatusIdIn { get; set; }

        /// <summary>
        /// Program EPG start date greater then this value. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "startDateGreaterThan")]
        [JsonProperty("startDateGreaterThan")]
        [XmlElement(ElementName = "startDateGreaterThan")]
        [SchemeProperty(IsNullable = true)]
        public long? StartDateGreaterThan { get; set; }

        /// <summary>
        /// Program EPG start date smaller than this value. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "startDateSmallerThan")]
        [JsonProperty("startDateSmallerThan")]
        [XmlElement(ElementName = "startDateSmallerThan")]
        [SchemeProperty(IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public long? StartDateSmallerThan { get; set; }
    }
}