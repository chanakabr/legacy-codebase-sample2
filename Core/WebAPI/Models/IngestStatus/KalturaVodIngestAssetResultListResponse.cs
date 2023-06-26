using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaVodIngestAssetResultListResponse : KalturaListResponse
    {
        /// <summary>
        /// list of KalturaVodIngestAssetResult
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaVodIngestAssetResult> Objects { get; set; }
    }
}