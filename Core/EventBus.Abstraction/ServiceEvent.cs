using System;

namespace EventBus.Abstraction
{
    public abstract class ServiceEvent
    {
        public int GroupId { get; set; }
        public string RequestId { get; set; }
        public long UserId { get; set; }
        public static string GetEventName(Type eventType) => $"{eventType.Namespace}.{eventType.Name}";
        public static string GetEventName(ServiceEvent e) => GetEventName(e.GetType());
    }
}
