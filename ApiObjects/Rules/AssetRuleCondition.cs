using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Rules
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class AssetRuleCondition
    {
        [JsonProperty("Type")]
        public AssetRuleConditionType Type { get; protected set; }

        [JsonProperty("Description")]
        public string Description { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class AssetCondition : AssetRuleCondition
    {
        [JsonProperty("Ksql")]
        public string Ksql { get; set; }

        public AssetCondition()
        {
            Type = AssetRuleConditionType.Asset;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class CountryCondition : AssetRuleCondition
    {
        [JsonProperty("Not")]
        public bool Not { get; set; }

        [JsonProperty("Countries")]
        public List<int> Countries { get; set; }

        public CountryCondition()
        {
            this.Type = AssetRuleConditionType.Country;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class ConcurrencyCondition : AssetCondition
    {
        [JsonProperty("Limit")]
        public int Limit { get; set; }

        [JsonProperty("RestrictionPolicy")]
        public ConcurrencyRestrictionPolicy RestrictionPolicy { get; set; }

        public ConcurrencyCondition()
        {
            this.Type = AssetRuleConditionType.Concurrency;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class IpRangeCondition : AssetCondition
    {
        [JsonProperty("fromIp")]
        public string FromIp { get; set; }

        [JsonProperty("toIp")]
        public string ToIp { get; set; }

        [JsonProperty("ipFrom")]
        public long IpFrom { get; set; }

        [JsonProperty("ipTo")]
        public long IpTo { get; set; }

        public IpRangeCondition()
        {
            this.Type = AssetRuleConditionType.IP_RANGE;
        }
    }
}
