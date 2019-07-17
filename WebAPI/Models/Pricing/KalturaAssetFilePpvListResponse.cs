using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Asset file ppv list
    /// </summary>
    public partial class KalturaAssetFilePpvListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of asset files ppvs
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetFilePpv> AssetFilesPpvs { get; set; }
    }
}