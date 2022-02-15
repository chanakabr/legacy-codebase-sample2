using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Watch history asset info
    /// </summary>
    [Serializable]
    [Obsolete]
    public partial class KalturaWatchHistoryAsset : KalturaOTTObject
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
        [OldStandardProperty("watched_date")]
        public long? LastWatched { get; set; }

        /// <summary>
        /// Boolean which specifies whether the user finished watching the movie or not
        /// </summary>
        [DataMember(Name = "finishedWatching")]
        [JsonProperty(PropertyName = "finishedWatching")]
        [XmlElement(ElementName = "finishedWatching")]
        [OldStandardProperty("finished_watching")]
        public bool? IsFinishedWatching { get; set; }
    }
}