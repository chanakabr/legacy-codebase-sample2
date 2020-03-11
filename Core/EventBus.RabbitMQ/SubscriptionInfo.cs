using System;

namespace EventBus.RabbitMQ
{
    public class SubscriptionInfo : IEquatable<SubscriptionInfo>
    {
        public Type HandlerType { get; }
        public Type EventType { get; }

        public override string ToString()
        {
            return $"{{\"event\":\"{EventType.Name}\",\"handler\":\"{HandlerType.Name}\"}}";
        }

        public SubscriptionInfo(Type eventType, Type handlerType)
        {
            EventType = eventType;
            HandlerType = handlerType;
        }

        public bool Equals(SubscriptionInfo other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return HandlerType == other.HandlerType && EventType == other.EventType;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SubscriptionInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var handlerHash = HandlerType?.GetHashCode() ?? 0;
                var eventHash = EventType?.GetHashCode() ?? 0;
                return handlerHash ^ eventHash;
            }
        }
    }
}
