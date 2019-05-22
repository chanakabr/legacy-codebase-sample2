using Newtonsoft.Json;
using System;

namespace ApiObjects.Notification
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class SubscribeReference
    {
        [JsonProperty("Type")]
        public SubscribeReferenceType Type { get; protected set; }

    }

    public enum SubscribeReferenceType
    {
        Subscription = 0
    }
}