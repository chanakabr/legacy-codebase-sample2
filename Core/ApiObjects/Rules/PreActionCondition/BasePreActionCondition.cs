using System;
using Newtonsoft.Json;

namespace ApiObjects.Rules.PreActionCondition
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BasePreActionCondition
    {
    }
}