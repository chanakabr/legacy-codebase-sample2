using ApiObjects.Rules;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ApiObjects
{
    public class BasePromotion
    {
        [JsonProperty(PropertyName = "conditions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<RuleCondition> Conditions { get; set; }

        public bool EvaluateConditions(IConditionScope scope)
        {
            if (Conditions != null && Conditions.Count > 0)
            {
                foreach (var condition in Conditions)
                {
                    if (!scope.Evaluate(condition))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public virtual int GetNumberOfRecurring()
        {
            return -1;
        }

        public virtual bool ShouldSaveHouseholdUsages()
        {
            return false;
        }
    }
}