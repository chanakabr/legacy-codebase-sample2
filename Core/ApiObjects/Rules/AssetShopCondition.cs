using System;
using Newtonsoft.Json;

namespace ApiObjects.Rules
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class AssetShopCondition : AssetConditionBase
    {
        [JsonProperty("Value")]
        public string Value { get; set; }

        public AssetShopCondition()
        {
            Type = RuleConditionType.AssetShop;
        }
    }
}