using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiObjects.Rules
{
    public interface ISegmentsConditionScope : IConditionScope
    {
        List<long> SegmentIds { get; set; }
        bool FilterBySegments { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class SegmentsCondition : RuleCondition
    {
        [JsonProperty("SegmentIds")]
        public List<long> SegmentIds { get; set; }

        public SegmentsCondition()
        {
            this.Type = RuleConditionType.Segments;
        }
    }
}