using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaRecording : KalturaOTTObject
    {

        /// <summary>
        /// Kaltura unique ID representing the recording identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? Id { get; set; }

        /// <summary>
        /// Recording state: scheduled/recording/recorded/canceled/failed/deleted
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaRecordingStatus Status { get; set; }

        /// <summary>
        /// Kaltura unique ID representing the program identifier
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty("assetId")]
        [XmlElement(ElementName = "assetId")]
        [SchemeProperty(InsertOnly = true)]
        public long AssetId { get; set; }

        /// <summary>
        /// Recording Type: single/season/series
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        [SchemeProperty(RequiresPermission = (int)RequestType.WRITE, InsertOnly = true)]
        public KalturaRecordingType Type { get; set; }

        /// <summary>
        /// Specifies until when the recording is available for viewing. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "viewableUntilDate")]
        [JsonProperty("viewableUntilDate")]
        [XmlElement(ElementName = "viewableUntilDate", IsNullable = true)]
        public long? ViewableUntilDate { get; set; }

        /// <summary>
        /// Specifies whether or not the recording is protected
        /// </summary>
        [DataMember(Name = "isProtected")]
        [JsonProperty("isProtected")]
        [XmlElement(ElementName = "isProtected")]
        public bool IsProtected { get; set; }

        /// <summary>
        /// Specifies when was the recording created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the recording last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }
        
        /// <summary>
        /// Duration in seconds
        /// </summary>
        [DataMember(Name = "duration")]
        [JsonProperty("duration")]
        [XmlElement(ElementName = "duration", IsNullable = true)]
        [SchemeProperty(ReadOnly = true, IsNullable = true)]
        public long? Duration { get; set; }
    }
}