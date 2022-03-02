using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    [ListResponse("IngestStatusEpgProgramResult")]
    public partial class KalturaIngestStatusEpgProgramResultListResponse : KalturaListResponse
    {
        /// <summary>
        /// list of KalturaIngestEpgProgramResult
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaIngestEpgProgramResult> Objects { get; set; }
    }
}