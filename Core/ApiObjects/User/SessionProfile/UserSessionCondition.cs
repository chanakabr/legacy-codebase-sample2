using ApiObjects.Rules;
using Newtonsoft.Json;
using System;

namespace ApiObjects.User.SessionProfile
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class UserSessionCondition : IUserSessionProfileExpression
    {
        [JsonProperty("Condition", TypeNameHandling = TypeNameHandling.Auto)]
        public RuleCondition Condition { get; set; }

        public bool Evaluate(IUserSessionConditionScope scope)
        {
            return scope.Evaluate(Condition);
        }
    }
}