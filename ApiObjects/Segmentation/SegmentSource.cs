using Newtonsoft.Json;

namespace ApiObjects.Segmentation
{
    public class SegmentSource
    {
    }

    public class MonetizationSource : SegmentSource
    {
        [JsonProperty()]
        public MonetizationType Type;

        [JsonProperty()] public MathemticalOperatorType Operator;
    }

    public class ContentSource : SegmentSource
    {
        [JsonProperty()]
        public string Field;
    }

    public class UserDynamicDataSource : SegmentSource
    {
        [JsonProperty()]
        public string Field;
    }
}