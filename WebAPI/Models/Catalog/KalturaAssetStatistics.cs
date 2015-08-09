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
    /// <summary>
    /// Asset statistics
    /// </summary>
    public class KalturaAssetStatistics : KalturaOTTObject
    {
        /// <summary>
        /// Unique identifier for the asset
        /// </summary>
        [DataMember(Name = "asset_id")]
        [JsonProperty(PropertyName = "asset_id")]
        [XmlElement(ElementName = "asset_id")]
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
        [DataMember(Name = "rating_count")]
        [JsonProperty(PropertyName = "rating_count")]
        [XmlElement(ElementName = "rating_count")]
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
        [DataMember(Name = "buzz_score")]
        [JsonProperty(PropertyName = "buzz_score")]
        [XmlElement(ElementName = "buzz_score")]
        public KalturaBuzzScore BuzzAvgScore { get; set; }
    }
}