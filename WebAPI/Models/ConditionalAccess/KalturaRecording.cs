using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaRecording : KalturaOTTObject
    {

        /// <summary>
        /// Kaltura unique ID representing the recording identifier
        /// </summary>
        [DataMember(Name = "recordingId")]
        [JsonProperty("recordingId")]
        [XmlElement(ElementName = "recordingId", IsNullable = true)]
        public long RecordingId { get; set; }

        /// <summary>
        /// Recording state: scheduled/recording/recorded/canceled/failed/does_not_exists/deleted
        /// </summary>
        [DataMember(Name = "recordingStatus")]
        [JsonProperty("recordingStatus")]
        [XmlElement(ElementName = "recordingStatus")]
        public KalturaRecordingStatus RecordingStatus { get; set; }

        /// <summary>
        /// Kaltura unique ID representing the program identifier
        /// </summary>
        [DataMember(Name = "epgId")]
        [JsonProperty("epgId")]
        [XmlElement(ElementName = "epgId")]
        public long EpgId { get; set; }

        /// <summary>
        /// Recording Type: single/series
        /// </summary>
        [DataMember(Name = "recordingType")]
        [JsonProperty("recordingType")]
        [XmlElement(ElementName = "recordingType")]
        public KalturaRecordingType RecordingType { get; set; }

        /// <summary>
        /// The date when the record is no longer available
        /// </summary>
        [DataMember(Name = "lastAvailabilityDate")]
        [JsonProperty("lastAvailabilityDate")]
        [XmlElement(ElementName = "lastAvailabilityDate")]
        public long LastAvailabilityDate { get; set; }

        /// <summary>
        /// Asset
        /// </summary>
        [DataMember(Name = "asset")]
        [JsonProperty("asset")]
        [XmlElement(ElementName = "asset", IsNullable = true)]
        public KalturaAssetInfo Asset { get; set; }
    }

}