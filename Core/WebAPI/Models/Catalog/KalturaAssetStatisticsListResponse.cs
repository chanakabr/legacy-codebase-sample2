using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// List of assets statistics
    /// </summary>
    [DataContract(Name = "KalturaAssetStatisticsListResponse", Namespace = "")]
    [XmlRoot("KalturaAssetStatisticsListResponse")]
    public partial class KalturaAssetStatisticsListResponse : KalturaListResponse
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaAssetStatistics> AssetsStatistics { get; set; }
    }
}