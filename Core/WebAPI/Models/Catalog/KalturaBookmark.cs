using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Users;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaBookmark : KalturaSlimAsset
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
        ///For external recordings will always be '0'
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
        [XmlElement(ElementName = "positionOwner")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaPositionOwner PositionOwner { get; set; }

        /// <summary>
        ///Specifies whether the user's current position exceeded 95% of the duration
        ///For external recordings will always be 'True'
        /// </summary>
        [DataMember(Name = "finishedWatching")]
        [JsonProperty("finishedWatching")]
        [XmlElement(ElementName = "finishedWatching")]
        [SchemeProperty(ReadOnly = true)]
        public bool? IsFinishedWatching { get; set; }

        /// <summary>
        ///Insert only player data
        /// </summary>
        [DataMember(Name = "playerData")]
        [JsonProperty("playerData")]
        [XmlElement(ElementName = "playerData")]
        public KalturaBookmarkPlayerData PlayerData { get; set; }

        /// <summary>
        /// Program Id
        /// </summary>
        [DataMember(Name = "programId")]
        [JsonProperty("programId")]
        [XmlElement(ElementName = "programId")]
        public long ProgramId { get; set; }
        
        /// <summary>
        /// Indicates if the current request is in reporting mode (hit)
        /// </summary>
        [DataMember(Name = "isReportingMode")]
        [JsonProperty("isReportingMode")]
        [XmlElement(ElementName = "isReportingMode")]
        public bool IsReportingMode { get; set; }
        
        /// <summary>
        /// Playback context type
        /// </summary>
        [DataMember(Name = "context")]
        [JsonProperty("context")]
        [XmlElement(ElementName = "context")]
        public KalturaPlaybackContextType? Context { get; set; }

        protected override void Init()
        {
            base.Init();
            this.IsReportingMode = false;
        }
    }
}
