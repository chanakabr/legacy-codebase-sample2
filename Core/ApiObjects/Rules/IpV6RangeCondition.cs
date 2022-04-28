using Newtonsoft.Json;
using System;

namespace ApiObjects.Rules
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class IpV6RangeCondition : AssetRuleCondition
    {
        [JsonProperty("fromIp")]
        public string FromIp { get; set; }

        [JsonProperty("toIp")]
        public string ToIp { get; set; }

        public IpV6RangeCondition()
        {
            this.Type = RuleConditionType.IP_V6_RANGE;
        }
    }
}