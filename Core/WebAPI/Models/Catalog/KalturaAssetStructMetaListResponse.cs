using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset Struct Metas list
    /// </summary>
    public partial class KalturaAssetStructMetaListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of asset struct metas
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetStructMeta> AssetStructMetas { get; set; }
    }
}