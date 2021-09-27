using Newtonsoft.Json;

namespace DAL.DTO.Notification
{
    internal class LineupNotificationSettings
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }
}