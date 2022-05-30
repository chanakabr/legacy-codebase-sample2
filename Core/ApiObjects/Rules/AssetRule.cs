using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ApiObjects.Rules
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class AssetRule : Rule
    {
        [JsonProperty(PropertyName = "Conditions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<RuleCondition> Conditions { get; set; }

        [JsonProperty(PropertyName = "Actions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<AssetRuleAction> Actions { get; set; }

        public bool IsConditionExists(RuleConditionType type)
        {
            if (Conditions != null && Conditions.Any())
            {
                return Conditions.Any(x => x.IsRuleConditionEquals(type));
            }

            return false;
        }

        public bool IsActionExists(RuleActionType type)
        {
            if (Actions != null && Actions.Any())
            {
                return Actions.Exists(x => x.Type.Equals(type));
            }

            return false;
        }
    }
}
