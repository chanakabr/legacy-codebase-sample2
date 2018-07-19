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
    public class AssetRuleCondition
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
}
