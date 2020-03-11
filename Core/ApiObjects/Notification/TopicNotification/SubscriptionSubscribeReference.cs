using System.Runtime.Serialization;

namespace ApiObjects.Notification
{
    [DataContract]
    public class SubscriptionSubscribeReference : SubscribeReference
    {
        [DataMember]
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