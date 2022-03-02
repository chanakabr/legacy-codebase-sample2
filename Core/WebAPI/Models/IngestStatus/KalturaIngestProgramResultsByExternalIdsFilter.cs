using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaIngestProgramResultsByExternalIdsFilter : KalturaIngestEpgProgramResultFilter
    {
        /// <summary>
        /// Comma seperated external program id.
        /// Up to 20 ids are allowed.
        /// </summary>
        [DataMember(Name = "externalProgramIdIn")]
        [JsonProperty("externalProgramIdIn")]
        [XmlElement(ElementName = "externalProgramIdIn")]
        [SchemeProperty(IsNullable = true)]
        public string ExternalProgramIdIn { get; set; }
    }
}