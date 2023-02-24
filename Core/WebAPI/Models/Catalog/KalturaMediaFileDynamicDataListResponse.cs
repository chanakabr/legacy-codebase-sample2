using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaMediaFileDynamicDataListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of media-file types
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaMediaFileDynamicData> Objects { get; set; }
    }
}
