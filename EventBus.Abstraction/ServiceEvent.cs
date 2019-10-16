using KLogMonitor;
using Newtonsoft.Json;
using System;

namespace EventBus.Abstraction
{
    public abstract class ServiceEvent
    {
        public ServiceEvent()
        {
            this.RequestId = KLogger.GetRequestId();
        }

        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("req_id")]
        public string RequestId { get; set; }

        private long _UserId;

        [JsonProperty("user_id")]
        public long UserId
        {
            get => _UserId;
            set => _UserId = value;
        }

        [JsonProperty("site_guid")]
        public string SiteGuid
        {
            get => _UserId.ToString();
            set => _UserId = long.Parse(value);
        }

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
