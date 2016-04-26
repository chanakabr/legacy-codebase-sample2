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
        [DataMember(Name = "recording_id")]
        [JsonProperty("recording_id")]
        [XmlElement(ElementName = "recording_id", IsNullable = true)]
        public long RecordingId { get; set; }

        /// <summary>
        /// Recording state: scheduled/recording/recorded/canceled/failed/does_not_exists/deleted
        /// </summary>
        [DataMember(Name = "recording_state")]
        [JsonProperty("recording_state")]
        [XmlElement(ElementName = "recording_state")]
        public KalturaRecordingStatus RecordingStatus { get; set; }

        /// <summary>
        /// Kaltura unique ID representing the program identifier
        /// </summary>
        [DataMember(Name = "epg_id")]
        [JsonProperty("epg_id")]
        [XmlElement(ElementName = "epg_id")]
        public long EpgID { get; set; }

        /// <summary>
        /// Recording Type: single/series
        /// </summary>
        [DataMember(Name = "recording_type")]
        [JsonProperty("recording_type")]
        [XmlElement(ElementName = "recording_type")]
        public KalturaRecordingType RecordingType { get; set; }

        /// <summary>
        /// The date when the record is no longer available
        /// </summary>
        [DataMember(Name = "last_availability_date")]
        [JsonProperty("last_availability_date")]
        [XmlElement(ElementName = "last_availability_date")]
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