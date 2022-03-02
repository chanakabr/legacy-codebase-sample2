using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaIngestProgramResultsByProgramIdsFilter : KalturaIngestEpgProgramResultFilter
    {
        /// <summary>
        /// Comma seperated program id (the unique ingested program id as it determined by Kaltura BE).
        /// Up to 20 ids are allowed.
        /// </summary>
        [DataMember(Name = "programIdIn")]
        [JsonProperty("programIdIn")]
        [XmlElement(ElementName = "programIdIn")]
        [SchemeProperty(IsNullable = true)]
        public string ProgramIdIn { get; set; }
    }
}