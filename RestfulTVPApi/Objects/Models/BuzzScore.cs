using Newtonsoft.Json;
using RestfulTVPApi.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Models
{
    public class BuzzScore
    {
        [JsonProperty(PropertyName = "normalized_avg_score")]
        public double NormalizedAvgScore { get; set; }

        [JsonProperty(PropertyName = "update_date")]
        public DateTime UpdateDate { get; set; }

        [JsonProperty(PropertyName = "avg_score")]
        public double AvgScore { get; set; }

        public static BuzzScore CreateFromObject(BuzzWeightedAverScore obj)
        {
            if (obj == null)
            {
                return null;
            }

            return new BuzzScore()
            {
                UpdateDate = obj.UpdateDate,
                NormalizedAvgScore = obj.NormalizedWeightedAverageScore,
                AvgScore = obj.WeightedAverageScore
            };
        }

    }
}
