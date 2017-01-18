using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.Statistics
{
    [Serializable]
    [JsonObject(Id = "BuzzWeightedAverScore")]
    public class BuzzWeightedAverScore
    {
        [JsonProperty("weighted_average_score")]
        public double WeightedAverageScore;
        [JsonProperty("normalized_weighted_average_score")]
        public double NormalizedWeightedAverageScore;
        [JsonProperty("update_date")]
        [JsonConverter(typeof(ApiObjects.JsonSerializers.BaseTimeConverter))]
        public DateTime UpdateDate;
    }
}
