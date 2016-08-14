using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Users;

namespace WebAPI.Models.Catalog
{
    public class KalturaBookmark : KalturaSlimAsset
    {
        /// <summary>
        ///User object
        /// </summary>
        [DataMember(Name = "user")]
        [JsonProperty("user")]
        [XmlElement(ElementName = "user", IsNullable = true)]
        [Obsolete]
        public KalturaBaseOTTUser User { get; set; }

        /// <summary>
        ///User identifier
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty("userId")]
        [XmlElement(ElementName = "userId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public string UserId { get; set; }

        /// <summary>
        ///The position of the user in the specific asset (in seconds)
        /// </summary>
        [DataMember(Name = "position")]
        [JsonProperty("position")]
        [XmlElement(ElementName = "position")]
        [SchemeProperty(InsertOnly = true)]
        public int? Position { get; set; }

        /// <summary>
        ///Indicates who is the owner of this position
        /// </summary>
        [DataMember(Name = "positionOwner")]
        [JsonProperty("positionOwner")]
        [XmlElement(ElementName = "positionOwner", IsNullable = true)]
        public KalturaPositionOwner PositionOwner { get; set; }

        /// <summary>
        ///Specifies whether the user's current position exceeded 95% of the duration
        /// </summary>
        [DataMember(Name = "finishedWatching")]
        [JsonProperty("finishedWatching")]
        [XmlElement(ElementName = "finishedWatching")]
        public bool? IsFinishedWatching { get; set; }

        /// <summary>
        ///Insert only player data
        /// </summary>
        [DataMember(Name = "playerData")]
        [JsonProperty("playerData")]
        [XmlElement(ElementName = "playerData")]
        public KalturaBookmarkPlayerData PlayerData { get; set; }

        internal int getPosition()
        {
            return Position.HasValue ? Position.Value : 0;
        }
    }

    public class KalturaBookmarkPlayerData : KalturaOTTObject
    {
        /// <summary>
        /// Action
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty(PropertyName = "action")]
        [XmlArrayItem(ElementName = "action")]
        public KalturaBookmarkActionType action { get; set; }

        /// <summary>
        /// Average Bitrate
        /// </summary>
        [DataMember(Name = "averageBitrate")]
        [JsonProperty(PropertyName = "averageBitrate")]
        [XmlArrayItem(ElementName = "averageBitrate")]
        public int? averageBitRate { get; set; }

        /// <summary>
        /// Total Bitrate
        /// </summary>
        [DataMember(Name = "totalBitrate")]
        [JsonProperty(PropertyName = "totalBitrate")]
        [XmlArrayItem(ElementName = "totalBitrate")]
        public int? totalBitRate { get; set; }

        /// <summary>
        /// Current Bitrate
        /// </summary>
        [DataMember(Name = "currentBitrate")]
        [JsonProperty(PropertyName = "currentBitrate")]
        [XmlArrayItem(ElementName = "currentBitrate")]
        public int? currentBitRate { get; set; }

        /// <summary>
        /// Identifier of the file
        /// </summary>
        [DataMember(Name = "fileId")]
        [JsonProperty(PropertyName = "fileId")]
        [XmlArrayItem(ElementName = "fileId")]
        public long? FileId { get; set; }

        internal int getAverageBitRate()
        {
            return averageBitRate.HasValue ? averageBitRate.Value : 0;
        }

        internal int getCurrentBitRate()
        {
            return currentBitRate.HasValue ? currentBitRate.Value : 0;
        }

        internal int getTotalBitRate()
        {
            return totalBitRate.HasValue ? totalBitRate.Value : 0;
        }

        internal long getFileId()
        {
            return FileId.HasValue ? FileId.Value : 0;
        }
    }
}