using System.Runtime.Serialization;
namespace ApiObjects.Notification
{
    [DataContract]
    [KnownType(typeof(SubscriptionSubscribeReference))]
    public abstract class SubscribeReference
    {
        [DataMember]
        public SubscribeReferenceType Type { get; protected set; }

        public abstract string GetSubscribtionReferenceId();
    }

    public enum SubscribeReferenceType
    {
        Subscription = 0
    }
}