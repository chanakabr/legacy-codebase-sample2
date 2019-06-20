using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ApiObjects.Rules
{
    [Serializable]
    public class BusinessModuleRule : Rule
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
        public List<BusinessModuleRuleAction> Actions { get; set; }

        [JsonProperty()]
        public long CreateDate { get; set; }

        [JsonProperty()]
        public long UpdateDate { get; set; }

        public bool Evaluate(IConditionScope scope)
        {
            if (Conditions != null && Conditions.Count > 0)
            {
                foreach (var condition in Conditions)
                {
                    scope.RuleId = this.Id;
                    
                    if (!condition.Evaluate(scope))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

    }

    [Serializable]
    public class BusinessModuleRuleType
    {
        [JsonProperty("BusinessModuleRuleId")]
        public long BusinessModuleRuleId { get; set; }

        [JsonProperty("ConditionsTypeIdIn")]
        public HashSet<RuleConditionType> ConditionsTypeIdIn { get; set; }

        [JsonProperty("ActionsTypeIdIn")]
        public HashSet<int> ActionsTypeIdIn { get; set; }
    }
}