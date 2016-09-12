using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaRecording : KalturaOTTObject
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
        /// Recording state: scheduled/recording/recorded/canceled/failed/does_not_exists/deleted
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
        public long AssetId { get; set; }

        /// <summary>
        /// Recording Type: single/season/series
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaRecordingType Type { get; set; }

        /// <summary>
        /// Specifies until when the recording is available for viewing. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "viewableUntilDate")]
        [JsonProperty("viewableUntilDate")]
        [XmlElement(ElementName = "viewableUntilDate")]
        [SchemeProperty(ReadOnly = true)]
        public long? ViewableUntilDate { get; set; }

        /// <summary>
        /// Specifies whether or not the recording is protected
        /// </summary>
        [DataMember(Name = "isProtected")]
        [JsonProperty("isProtected")]
        [XmlElement(ElementName = "isProtected")]
        [SchemeProperty(ReadOnly = true)]
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

    }

}