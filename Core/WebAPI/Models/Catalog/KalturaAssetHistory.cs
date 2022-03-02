using Newtonsoft.Json;
using System;
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
    public partial class KalturaAssetHistory : KalturaOTTObject
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
        /// Asset identifier
        /// </summary>
        [DataMember(Name = "assetType")]
        [JsonProperty(PropertyName = "assetType")]
        [XmlElement(ElementName = "assetType")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaAssetType AssetType { get; set; }

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
}
