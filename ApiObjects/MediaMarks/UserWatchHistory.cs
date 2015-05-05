using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.MediaMarks
{
    public class UserWatchHistory
    {
        [JsonProperty("uid")]
        public int UserID { get; set; }

        [JsonProperty("assetId")]
        public int AssetId { get; set; }

        [JsonProperty("loc")]
        public int Location { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("ts")]
        public long LastWatch { get; set; }

        [JsonProperty("assetTypeId")]
        public int AssetTypeId { get; set; }

        public DateTime AssetUpdatedDate { get; set; }

        public bool IsFinishedWatching { get; set; }
    }
}
