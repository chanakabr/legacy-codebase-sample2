using System;
using Newtonsoft.Json;

namespace ApiObjects.Rules.PreActionCondition
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class ShopPreActionCondition : BasePreActionCondition
    {
        [JsonProperty("shopAssetUserRuleId")]
        public int ShopAssetUserRuleId { get; set; }
    }
}