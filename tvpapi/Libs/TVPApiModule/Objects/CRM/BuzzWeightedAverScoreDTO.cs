using ApiObjects.Statistics;
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
        public DateTime UpdateDate;

        public static BuzzWeightedAverScoreDTO ConvertToDTO(BuzzWeightedAverScore weightedAverScore)
        {
            if(weightedAverScore == null)
            {
                return null;
            }
            BuzzWeightedAverScoreDTO res = new BuzzWeightedAverScoreDTO()
            {
                NormalizedWeightedAverageScore = weightedAverScore.NormalizedWeightedAverageScore,
                WeightedAverageScore = weightedAverScore.WeightedAverageScore,
                UpdateDate = weightedAverScore.UpdateDate
            };
            
            return res;
        }
    }
}
