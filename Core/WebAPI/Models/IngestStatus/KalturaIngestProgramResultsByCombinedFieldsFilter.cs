using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaIngestProgramResultsByCombinedFieldsFilter : KalturaIngestProgramResultsByRefineFilter
    {
        /// <summary>
        /// String value to substring search by ProgramID or ExternalProgramID or LinearChannelID.
        /// Up to 20 ids are allowed.
        /// </summary>
        [DataMember(Name = "combinedFieldsValue")]
        [JsonProperty("combinedFieldsValue")]
        [XmlElement(ElementName = "combinedFieldsValue")]
        [SchemeProperty(IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string CombinedFieldsValue { get; set; }
    }
}
