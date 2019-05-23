using Newtonsoft.Json;
using System;

namespace ApiObjects.Notification
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class SubscriptionSubscribeReference : SubscribeReference
    {
        [JsonProperty("SubscriptionId")]
        public long SubscriptionId { get; set; }

        public SubscriptionSubscribeReference()
        {
            this.Type = SubscribeReferenceType.Subscription;            
        }

        public override string GetSubscribtionReferenceId()
        {
            return string.Format("{0}_{1}", Type, SubscriptionId);
        }
    }
}