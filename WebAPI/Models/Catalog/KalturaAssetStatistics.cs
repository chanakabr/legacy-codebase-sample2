using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset statistics
    /// </summary>
    public class KalturaAssetStatistics : KalturaOTTObject
    {
        /// <summary>
        /// Unique identifier for the asset
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty(PropertyName = "assetId")]
        [XmlElement(ElementName = "assetId")]
        [OldStandardProperty("asset_id")]
        public int AssetId { get; set; }

        /// <summary>
        /// Total number of likes for this asset
        /// </summary>
        [DataMember(Name = "likes")]
        [JsonProperty(PropertyName = "likes")]
        [XmlElement(ElementName = "likes")]
        public int Likes { get; set; }

        /// <summary>
        /// Total number of views for this asset
        /// </summary>
        [DataMember(Name = "views")]
        [JsonProperty(PropertyName = "views")]
        [XmlElement(ElementName = "views")]
        public int Views { get; set; }

        /// <summary>
        /// Number of people that rated the asset
        /// </summary>
        [DataMember(Name = "ratingCount")]
        [JsonProperty(PropertyName = "ratingCount")]
        [XmlElement(ElementName = "ratingCount")]
        [OldStandardProperty("rating_count")]
        public int RatingCount { get; set; }

        /// <summary>
        /// Average rating for the asset
        /// </summary>
        [DataMember(Name = "rating")]
        [JsonProperty(PropertyName = "rating")]
        [XmlElement(ElementName = "rating")]
        public double Rating { get; set; }

        /// <summary>
        /// Buzz score
        /// </summary>
        [DataMember(Name = "buzzScore")]
        [JsonProperty(PropertyName = "buzzScore")]
        [XmlElement(ElementName = "buzzScore", IsNullable = true)]
        [OldStandardProperty("buzz_score")]
        public KalturaBuzzScore BuzzAvgScore { get; set; }
    }

    /// <summary>
    /// List of assets statistics
    /// </summary>
    [DataContract(Name = "KalturaAssetStatisticsListResponse", Namespace = "")]
    [XmlRoot("KalturaAssetStatisticsListResponse")]
    public class KalturaAssetStatisticsListResponse : KalturaListResponse
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