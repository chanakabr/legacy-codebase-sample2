using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Managers.Models
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class NotificationBooleanCondition : NotificationCondition
    {
        [JsonProperty("conditions")]
        public List<NotificationCondition> Conditions
        {
            get;
            set;
        }

        [JsonProperty("operator")]
        public eBooleanConditionOperator Operator
        {
            get;
            set;
        }

        public override bool Evaluate(EventManager.KalturaEvent kalturaEvent, object eventObject)
        {
            bool result = true;

            if (this.Conditions != null && this.Conditions.Count > 0)
            {
                switch (this.Operator)
                {
                    // Perform AND operation between all conditions in list:
                    // only if all conditions in list are met, the result is true
                    case eBooleanConditionOperator.And:
                    {
                        result = true;

                        foreach (var condition in this.Conditions)
                        {
                            result &= condition.Evaluate(kalturaEvent, eventObject);
                        }

                        break;
                    }
                    // Perform AND operation between all conditions in list:
                    // if at least one of the conditions is met, the result is true
                    // start with false, and turn on the flag when the first condition ismet
                    case eBooleanConditionOperator.Or:
                    {
                        result = false;

                        foreach (var condition in this.Conditions)
                        {
                            bool tempResult = condition.Evaluate(kalturaEvent, eventObject);

                            if (tempResult)
                            {
                                result = true;
                                break;
                            }
                        }

                        break;
                    }
                    default:
                    break;
                }
            }

            return result;
        }
    }

    public enum eBooleanConditionOperator
    {
        And,
        Or
    }
}