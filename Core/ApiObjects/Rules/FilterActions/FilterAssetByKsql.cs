using System;
using Newtonsoft.Json;

namespace ApiObjects.Rules.FilterActions
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class FilterAssetByKsql : AssetRuleFilterAction
    {
        [JsonProperty("Ksql")]
        public string Ksql { get; set; }

        public FilterAssetByKsql()
        {
            Type = RuleActionType.FilterAssetByKsql;
        }
    }
}