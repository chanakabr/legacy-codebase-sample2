using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset Structs list
    /// </summary>
    public partial class KalturaAssetStructListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of asset structs
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetStruct> AssetStructs { get; set; }
    }
}