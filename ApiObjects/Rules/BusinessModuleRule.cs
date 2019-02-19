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
        public List<RuleBaseCondition> Conditions { get; set; }

        [JsonProperty(PropertyName = "Actions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<ApplyDiscountModuleRuleAction> Actions { get; set; }

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
                    var evalCondition = condition as RuleCondition<IConditionScope>;
                    
                    if (!evalCondition.Evaluate(scope))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

    }
}

