using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Watch history asset wrapper
    /// </summary>
    [Serializable]
    public class KalturaWatchHistoryAssetWrapper : KalturaListResponse, KalturaIAssetable
    {
        /// <summary>
        /// WatchHistoryAssets Models
        /// </summary>
        [DataMember(Name = "assets")]
        [JsonProperty(PropertyName = "assets")]
        [XmlArray(ElementName = "assets")]
        [XmlArrayItem("item")] 
        public List<KalturaWatchHistoryAsset> WatchHistoryAssets { get; set; }

        public KalturaWatchHistoryAssetWrapper()
        {            
            WatchHistoryAssets = new List<KalturaWatchHistoryAsset>();
        }
    }

    /// <summary>
    /// Watch history asset info
    /// </summary>
    [Serializable]
    public class KalturaWatchHistoryAsset : KalturaOTTObject
    {
        /// <summary>
        /// AssetInfo Model
        /// </summary>
        [DataMember(Name = "asset")]
        [JsonProperty(PropertyName = "asset")]
        [XmlElement(ElementName = "asset")]
        public KalturaAssetInfo Asset { get; set; }

        /// <summary>
        /// Position in seconds of the relevant asset
        /// </summary>
        [DataMember(Name = "position")]
        [JsonProperty(PropertyName = "position")]
        [XmlElement(ElementName = "position")]
        public int Position { get; set; }

        /// <summary>
        /// Duration in seconds of the relevant asset
        /// </summary>
        [DataMember(Name = "duration")]
        [JsonProperty(PropertyName = "duration")]
        [XmlElement(ElementName = "duration")]
        public int Duration { get; set; }

        /// <summary>
        /// The date when the media was last watched
        /// </summary>
        [DataMember(Name = "watched_date")]
        [JsonProperty(PropertyName = "watched_date")]
        [XmlElement(ElementName = "watched_date")]
        public long LastWatched { get; set; }

        /// <summary>
        /// Boolean which specifies whether the user finished watching the movie or not
        /// </summary>
        [DataMember(Name = "finished_watching")]
        [JsonProperty(PropertyName = "finished_watching")]
        [XmlElement(ElementName = "finished_watching")]
        public bool IsFinishedWatching { get; set; }
    }
}