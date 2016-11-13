using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Catalog.Response
{
    public class UserWatchHistory : BaseObject
    {
        [JsonProperty("uid")]
        public int UserID { get; set; }

        //[JsonProperty("assetId")]
        //public string AssetId { get; set; }

        [JsonProperty("loc")]
        public int Location { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("ts")]
        public long LastWatch { get; set; }

        [JsonProperty("assetTypeId")]
        public int AssetTypeId { get; set; }

        //public DateTime AssetUpdatedDate { get; set; }

        public bool IsFinishedWatching { get; set; }

        [JsonProperty("epgId")]
        public long EpgId
        {
            get;
            set;
        }
    }
}
