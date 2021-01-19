using Newtonsoft.Json;
using System.Collections.Generic;

namespace DAL.DTO.Notification
{
    internal class EpgNotificationSettings
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
        [JsonProperty("device_families")]
        public IReadOnlyCollection<int> DeviceFamilyIds { get; set; }
        [JsonProperty("live_assets")]
        public IReadOnlyCollection<long> LiveAssetIds { get; set; }
        [JsonProperty("time_range")]
        public int TimeRange { get; set; }
    }
}
