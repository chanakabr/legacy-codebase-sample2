using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Watch history asset wrapper
    /// </summary>
    [Serializable]
    public class KalturaAssetHistoryListResponse : KalturaListResponse
    {
        /// <summary>
        /// WatchHistoryAssets Models
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetHistory> Objects { get; set; }
    }

    /// <summary>
    /// Watch history asset info
    /// </summary>
    [Serializable]
    public class KalturaAssetHistory : KalturaOTTObject
    {
        /// <summary>
        /// Asset identifier
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty(PropertyName = "assetId")]
        [XmlElement(ElementName = "assetId")]
        [SchemeProperty(ReadOnly = true)]
        public long AssetId { get; set; }

        /// <summary>
        /// Position in seconds of the relevant asset
        /// </summary>
        [DataMember(Name = "position")]
        [JsonProperty(PropertyName = "position")]
        [XmlElement(ElementName = "position", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public int? Position { get; set; }

        /// <summary>
        /// Duration in seconds of the relevant asset
        /// </summary>
        [DataMember(Name = "duration")]
        [JsonProperty(PropertyName = "duration")]
        [XmlElement(ElementName = "duration", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public int? Duration { get; set; }

        /// <summary>
        /// The date when the media was last watched
        /// </summary>
        [DataMember(Name = "watchedDate")]
        [JsonProperty(PropertyName = "watchedDate")]
        [XmlElement(ElementName = "watchedDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? LastWatched { get; set; }

        /// <summary>
        /// Boolean which specifies whether the user finished watching the movie or not
        /// </summary>
        [DataMember(Name = "finishedWatching")]
        [JsonProperty(PropertyName = "finishedWatching")]
        [XmlElement(ElementName = "finishedWatching", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public bool? IsFinishedWatching { get; set; }
    }

    /// <summary>
    /// Watch history asset wrapper
    /// </summary>
    [Serializable]
    [Obsolete]
    public class KalturaWatchHistoryAssetWrapper : KalturaListResponse, KalturaIAssetable
    {
        /// <summary>
        /// WatchHistoryAssets Models
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaWatchHistoryAsset> Objects { get; set; }

        public KalturaWatchHistoryAssetWrapper()
        {
            Objects = new List<KalturaWatchHistoryAsset>();
        }
    }

    /// <summary>
    /// Watch history asset info
    /// </summary>
    [Serializable]
    [OldStandard("watchedDate", "watched_date")]
    [OldStandard("finishedWatching", "finished_watching")]
    [Obsolete]
    public class KalturaWatchHistoryAsset : KalturaOTTObject
    {
        /// <summary>
        /// AssetInfo Model
        /// </summary>
        [DataMember(Name = "asset")]
        [JsonProperty(PropertyName = "asset")]
        [XmlElement(ElementName = "asset", IsNullable = true)]
        public KalturaAssetInfo Asset { get; set; }

        /// <summary>
        /// Position in seconds of the relevant asset
        /// </summary>
        [DataMember(Name = "position")]
        [JsonProperty(PropertyName = "position")]
        [XmlElement(ElementName = "position")]
        public int? Position { get; set; }

        /// <summary>
        /// Duration in seconds of the relevant asset
        /// </summary>
        [DataMember(Name = "duration")]
        [JsonProperty(PropertyName = "duration")]
        [XmlElement(ElementName = "duration")]
        public int? Duration { get; set; }

        /// <summary>
        /// The date when the media was last watched
        /// </summary>
        [DataMember(Name = "watchedDate")]
        [JsonProperty(PropertyName = "watchedDate")]
        [XmlElement(ElementName = "watchedDate")]
        public long? LastWatched { get; set; }

        /// <summary>
        /// Boolean which specifies whether the user finished watching the movie or not
        /// </summary>
        [DataMember(Name = "finishedWatching")]
        [JsonProperty(PropertyName = "finishedWatching")]
        [XmlElement(ElementName = "finishedWatching")]
        public bool? IsFinishedWatching { get; set; }
    }
}