using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    // TODO SHIR - delete when not need talk with Tan Tan!!!
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