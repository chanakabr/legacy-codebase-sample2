using ApiObjects.EventBus;
using Newtonsoft.Json;
using System;

namespace EpgNotificationHandler.DTO
{
    [Serializable]
    public class EpgUpdateMessage
    {
        [JsonProperty("header")]
        public UpdateHeader Header { get; set; }
        [JsonProperty("epg_channel_id")]
        public long EpgChannelId { get; set; }
        [JsonProperty("live_asset_id")]
        public long LiveAssetId { get; set; }
        [JsonProperty("start_date")]
        public long StartDate { get; set; }
        [JsonProperty("end_date")]
        public long EndDate { get; set; }
    }

    [Serializable]
    public class UpdateHeader
    {
        [JsonProperty("event_type")]
        public EventType EventType { get; set; }
        [JsonProperty("event_date")]
        public long EventDate { get; set; }
    }
}
