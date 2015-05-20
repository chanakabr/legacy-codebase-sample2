using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models
{
    public class WatchHistoryAssetWrapper : BaseListWrapper
    {
        public List<WatchHistoryAsset> WatchHistoryAssets { get; set; }

        public WatchHistoryAssetWrapper()
        {
            WatchHistoryAssets = new List<WatchHistoryAsset>();
        }
    }

    public class WatchHistoryAsset
    {
        /// <summary>
        /// AssetInfo Model
        /// </summary>
        [DataMember(Name = "asset")]
        [JsonProperty(PropertyName = "asset")]
        public AssetInfo Asset { get; set; }

        /// <summary>
        /// Position in seconds of the relevant asset
        /// </summary>
        [DataMember(Name = "position")]
        [JsonProperty(PropertyName = "position")]
        public int Position { get; set; }

        /// <summary>
        /// Duration in seconds of the relevant asset
        /// </summary>
        [DataMember(Name = "duration")]
        [JsonProperty(PropertyName = "duration")]
        public int Duration { get; set; }

        /// <summary>
        /// The date when the media was last watched
        /// </summary>
        [DataMember(Name = "watched_date")]
        [JsonProperty(PropertyName = "watched_date")]
        public long LastWatched { get; set; }

        /// <summary>
        /// Boolean which specifies whether the user finished watching the movie or not
        /// </summary>
        [DataMember(Name = "finished_watching")]
        [JsonProperty(PropertyName = "finished_watching")]
        public bool IsFinishedWatching { get; set; }
    }
}