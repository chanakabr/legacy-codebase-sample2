using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaIngestByCompoundFilter : KalturaFilter<KalturaIngestStatusOrderBy>
    {
        /// <summary>
        /// A string that is included in the ingest file name
        /// </summary>
        [DataMember(Name = "ingestNameContains")]
        [JsonProperty("ingestNameContains")]
        [XmlElement(ElementName = "ingestNameContains")]
        [SchemeProperty(IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string IngestNameContains { get; set; }

        /// <summary>
        /// Comma seperated user ids
        /// </summary>
        [DataMember(Name = "ingestedByUserIdIn")]
        [JsonProperty("ingestedByUserIdIn")]
        [XmlElement(ElementName = "ingestedByUserIdIn", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string IngestedByUserIdIn { get; set; }

        /// <summary>
        /// Comma seperated valid stutuses
        /// </summary>
        [DataMember(Name = "ingestStatusIn")]
        [JsonProperty("ingestStatusIn")]
        [XmlElement(ElementName = "ingestStatusIn", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string IngestStatusIn { get; set; }

        /// <summary>
        /// Ingest created date greater then this value. . Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createdDateGreaterThan")]
        [JsonProperty("createdDateGreaterThan")]
        [XmlElement(ElementName = "createdDateGreaterThan", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public long? CreatedDateGreaterThan { get; set; }

        /// <summary>
        /// Ingest created date smaller than this value. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createdDateSmallerThan")]
        [JsonProperty("createdDateSmallerThan")]
        [XmlElement(ElementName = "createdDateSmallerThan", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public long? CreatedDateSmallerThan { get; set; }

        public override KalturaIngestStatusOrderBy GetDefaultOrderByValue()
        {
            return KalturaIngestStatusOrderBy.NONE;
        }
    }
}
