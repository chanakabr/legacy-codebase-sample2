using System;
using Newtonsoft.Json;

namespace ApiObjects.Rules.PreActionCondition
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class NoShopPreActionCondition : BasePreActionCondition
    {
    }
}