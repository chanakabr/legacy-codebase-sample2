using KLogMonitor;
using System;

namespace EventBus.Abstraction
{
    public abstract class ServiceEvent
    {
        public ServiceEvent()
        {
            this.RequestId = KLogger.GetRequestId();
        }

        public int GroupId { get; set; }
        public string RequestId { get; set; }
        public long UserId { get; set; }
        public static string GetEventName(Type eventType) => $"{eventType.Namespace}.{eventType.Name}";
        public static string GetEventName(ServiceEvent e) => GetEventName(e.GetType());

        public override string ToString()
        {
            try
            {
                return $"groupId {GroupId} requestId {RequestId} eventName: {GetEventName(this.GetType())}";
            }
            catch
            {
                return base.ToString();
            }
        }

        public string ToJSON()
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this).ToString();
            }
            catch (Exception ex)
            {
                return $"Could not serialize object: {ex}";
            }
        }
    }
}
