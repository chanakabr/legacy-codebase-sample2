using Newtonsoft.Json;
using System;

namespace TVPApiModule.Objects.CRM
{
    [Serializable]
    [JsonObject(Id = "BuzzWeightedAverScore")]
    public class BuzzWeightedAverScoreDTO
    {
        [JsonProperty("WeightedAverageScore")]
        public double WeightedAverageScore;
        [JsonProperty("NormalizedWeightedAverageScore")]
        public double NormalizedWeightedAverageScore;
        [JsonProperty("UpdateDate")]
        [JsonConverter(typeof(ApiObjects.JsonSerializers.BaseTimeConverter))]
        public DateTime UpdateDate;
    }
}
