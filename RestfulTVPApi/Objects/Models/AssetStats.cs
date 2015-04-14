using Newtonsoft.Json;
using RestfulTVPApi.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Models
{
    public class AssetStats
    {
        [JsonProperty(PropertyName = "asset_id")]
        public int AssetId { get; set; }

        [JsonProperty(PropertyName = "likes")]
        public int Likes { get; set; }

        [JsonProperty(PropertyName = "views")]
        public int Views { get; set; }

        [JsonProperty(PropertyName = "rating_count")]
        public int RatingCount { get; set; }

        [JsonProperty(PropertyName = "rating")]
        public double Rating { get; set; }

        [JsonProperty(PropertyName = "buzz_avg_score")]
        public BuzzScore BuzzAvgScore { get; set; }

        public static AssetStats CreateFromObject(AssetStatsResult obj)
        {
            return new AssetStats()
            {
                AssetId = obj.m_nAssetID,
                Likes = obj.m_nLikes,
                Views = obj.m_nViews,
                RatingCount = obj.m_nVotes,
                Rating = obj.m_dRate,
                BuzzAvgScore = BuzzScore.CreateFromObject(obj.m_buzzAverScore)
            };
        }
    }
}
