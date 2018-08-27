using Newtonsoft.Json;
using System.Collections.Generic;

namespace ApiObjects.Segmentation
{
    public class SegmentCondition
    {
    }

    public class SegmentUserDataCondition : SegmentCondition
    {
        [JsonProperty()]
        public string Field;

        [JsonProperty()]
        public string Value;
    }

    public class ContentScoreCondition : SegmentCondition
    {
        [JsonProperty()]
        public int Score { get; set; }

        [JsonProperty()]
        public List<ContentActionCondition> Actions { get; set; }
    }

    public class ContentActionCondition
    {
        [JsonProperty()]
        public ContentAction Action { get; set; }

        [JsonProperty()]
        public int Length { get; set; }

        [JsonProperty()]
        public int Multiplier { get; set; }
    }

    public class MonetizationScoredCondition : SegmentCondition
    {
        [JsonProperty()]
        public int Score { get; set; }

        [JsonProperty()]
        public List<MonetizationCondition> Actions { get; set; }
    }

    public class MonetizationCondition
    {
        [JsonProperty()]
        public MonetizationType Type;

        [JsonProperty()]
        public int MinimumPrice;

        [JsonProperty()]
        public int Multiplier;
    }
}