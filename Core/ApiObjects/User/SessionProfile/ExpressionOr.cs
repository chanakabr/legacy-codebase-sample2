using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiObjects.User.SessionProfile
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class ExpressionOr : IUserSessionProfileExpression
    {
        [JsonProperty(PropertyName = "Expressions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<IUserSessionProfileExpression> Expressions { get; set; }

        public bool Evaluate(IUserSessionConditionScope scope)
        {
            return Expressions.Any(e => e.Evaluate(scope));
        }
    }
}