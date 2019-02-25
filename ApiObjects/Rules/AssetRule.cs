using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ApiObjects.Rules
{
    [Serializable]
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
        
        public bool HasCountryConditions()
        {
            if (this.Conditions != null && this.Conditions.Count > 0)
            {
                return this.Conditions.Exists(x => x.Type == RuleConditionType.Country);
            }

            return false;
        }
    }
    
    [Serializable]
    public class AssetRuleTypeMapping
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("TypeIdIn")]
        public List<int> TypeIdIn { get; set; }
    }
}
