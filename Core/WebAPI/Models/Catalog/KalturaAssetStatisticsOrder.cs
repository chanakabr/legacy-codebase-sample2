using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaAssetStatisticsOrder : KalturaBaseAssetOrder
    {
        /// <summary>
        /// Trending Days Equal
        /// </summary>
        [DataMember(Name = "trendingDaysEqual")]
        [JsonProperty("trendingDaysEqual")]
        [XmlElement(ElementName = "trendingDaysEqual", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinInteger = 1, MaxInteger = 366)]
        public int? TrendingDaysEqual { get; set; }

        /// <summary>
        /// order by meta asc/desc
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaAssetOrderByStatistics OrderBy { get; set; }
    }
}
