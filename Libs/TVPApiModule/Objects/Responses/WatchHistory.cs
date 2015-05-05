using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class WatchHistoryAsset
    {
        [JsonProperty(PropertyName = "asset")]
        public AssetInfo Asset { get; set; }

        [JsonProperty(PropertyName = "position")]
        public int Position { get; set; }

        [JsonProperty(PropertyName = "duration")]
        public int Duration { get; set; }

        [JsonProperty(PropertyName = "watched_date")]
        public long LastWatched { get; set; }

        [JsonProperty(PropertyName = "finished_watching")]
        public bool IsFinishedWatching { get; set; }
    }

    public class WatchHistory
    {
        [JsonProperty(PropertyName = "assets")]
        public List<WatchHistoryAsset> Assets { get; set; }

        [JsonProperty(PropertyName = "total_items")]
        public int TotalItems { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }
    }
}
