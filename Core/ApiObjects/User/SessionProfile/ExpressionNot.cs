using Newtonsoft.Json;
using System;

namespace ApiObjects.User.SessionProfile
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class ExpressionNot : IUserSessionProfileExpression
    {
        [JsonProperty("Expression", TypeNameHandling = TypeNameHandling.Auto)]
        public IUserSessionProfileExpression Expression { get; set; }

        public bool Evaluate(IUserSessionConditionScope scope)
        {
            return !Expression.Evaluate(scope);
        }
    }
}