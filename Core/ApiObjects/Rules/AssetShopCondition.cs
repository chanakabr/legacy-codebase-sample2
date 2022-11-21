using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects.Rules
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class AssetShopCondition : AssetConditionBase
    {
        [JsonProperty("Value")]
        public string Value { get; set; }

        [JsonProperty("Values")]
        public List<string> Values { get; set; }

        public AssetShopCondition()
        {
            Type = RuleConditionType.AssetShop;
        }
    }
}