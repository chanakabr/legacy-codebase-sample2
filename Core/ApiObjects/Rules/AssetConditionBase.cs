using System;
using Newtonsoft.Json;

namespace ApiObjects.Rules
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class AssetConditionBase : AssetRuleCondition
    {
    }
}