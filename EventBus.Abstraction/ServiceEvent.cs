using System;

namespace EventBus.Abstraction
{
    public abstract class ServiceEvent
    {
        public string RequestId { get; set; }
        public static string GetEventName(Type eventType) => $"{eventType.Namespace}.{eventType.Name}";
        public static string GetEventName(ServiceEvent e) => GetEventName(e.GetType());
    }
}
