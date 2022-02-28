using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaIngestProgramResultsByCompoundFilter : KalturaIngestProgramResultsByRefineFilter
    {
        /// <summary>
        /// Comma seperated channel id (the id of the linear channel asset that the program belongs to).
        /// Up to 20 ids are allowed.
        /// </summary>
        [DataMember(Name = "linearChannelIdIn")]
        [JsonProperty("linearChannelIdIn")]
        [XmlElement(ElementName = "linearChannelIdIn")]
        [SchemeProperty(IsNullable = true)]
        public string LinearChannelIdIn { get; set; }
    }
}