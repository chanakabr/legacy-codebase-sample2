using ApiObjects.Base;
using System.Collections.Generic;
using Newtonsoft.Json;
using ApiObjects.Rules;

namespace ApiObjects
{
    public class Promotion
    {
        public long DiscountModuleId { get; set; }
        public int? NumberOfRecurring { get; set; }

        [JsonProperty(PropertyName = "conditions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<Rules.RuleCondition> Conditions { get; set; }

        public bool EvaluateConditions(Rules.IConditionScope scope)
        {
            if (Conditions != null && Conditions.Count > 0)
            {
                foreach (var condition in Conditions)
                {
                    if (!condition.Evaluate(scope))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}