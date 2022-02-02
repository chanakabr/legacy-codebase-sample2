using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaAssetOrder : KalturaBaseAssetOrder
    {
        /// <summary>
        /// Order By
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty(PropertyName = "orderBy")]
        [XmlElement(ElementName = "orderBy")]
        public KalturaAssetOrderByType OrderBy { get; set; }
    }
}
