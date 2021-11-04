using Newtonsoft.Json;

namespace NotificationHandlers.Common.DTO
{
    public abstract class NotificationMessage
    {
        [JsonProperty("header")]
        public UpdateHeader Header { get; set; }
    }
}