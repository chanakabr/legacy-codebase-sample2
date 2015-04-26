using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models
{
    public class AssetStats
    {
        /// <summary>
        /// Asset ID
        /// </summary>
        [DataMember(Name = "asset_id")]
        [JsonProperty(PropertyName = "asset_id")]
        public int AssetId { get; set; }

        /// <summary>
        /// Likes
        /// </summary>
        [DataMember(Name = "likes")]
        [JsonProperty(PropertyName = "likes")]
        public int Likes { get; set; }

        /// <summary>
        /// Views
        /// </summary>
        [DataMember(Name = "views")]
        [JsonProperty(PropertyName = "views")]
        public int Views { get; set; }

        /// <summary>
        /// Rating count
        /// </summary>
        [DataMember(Name = "rating_count")]
        [JsonProperty(PropertyName = "rating_count")]
        public int RatingCount { get; set; }

        /// <summary>
        /// Rating
        /// </summary>
        [DataMember(Name = "rating")]
        [JsonProperty(PropertyName = "rating")]
        public double Rating { get; set; }

        /// <summary>
        /// Buzz score
        /// </summary>
        [DataMember(Name = "buzz_score")]
        [JsonProperty(PropertyName = "buzz_score")]
        public BuzzScore BuzzAvgScore { get; set; }
    }
}