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

        [JsonProperty(PropertyName = "location")]
        public int Location { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public long CreatedAt { get; set; }
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
