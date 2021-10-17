using System;
using Newtonsoft.Json;

namespace ApiObjects.Rules.FilterActions
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class FilterAssetByKsql : AssetRuleAction
    {
        [JsonProperty("Ksql")]
        public string Ksql { get; set; }

        public FilterAssetByKsql()
        {
            Type = RuleActionType.FilterAssetByKsql;
        }
    }
}