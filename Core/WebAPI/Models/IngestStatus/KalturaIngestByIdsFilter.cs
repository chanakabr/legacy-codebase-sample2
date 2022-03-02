using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaIngestByIdsFilter : KalturaFilter<KalturaIngestStatusOrderBy>
    {
        /// <summary>
        /// Comma seperated ingest profile ids
        /// </summary>
        [DataMember(Name = "ingestIdIn")]
        [JsonProperty("ingestIdIn")]
        [XmlElement(ElementName = "ingestIdIn")]
        [SchemeProperty(IsNullable = true, Pattern = @"^\d+(\s*,\s*\d+){0,19}$")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string IngestIdIn { get; set; }

        public override KalturaIngestStatusOrderBy GetDefaultOrderByValue()
        {
            return KalturaIngestStatusOrderBy.NONE;
        }
    }
}

