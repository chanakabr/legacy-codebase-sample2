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
    /// <summary>
    /// Recordings info wrapper
    /// </summary>
    [Serializable]
    public class KalturaRecordingListResponse : KalturaListResponse
    {
        /// <summary>
        /// Recordings
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaRecordingInfo> Objects { get; set; }

    }

        /// <summary>
    /// Recording Info
    /// </summary>
    [Serializable]
    public class KalturaRecordingInfo : KalturaOTTObject
    {
        /// <summary>
        /// Kaltura unique ID representing the recording identifier
        /// </summary>
        [DataMember(Name = "recording_id")]
        [JsonProperty("recording_id")]
        [XmlElement(ElementName = "recording_id")]
        public long RecordingId { get; set; }

        /// <summary>
        /// Recording state: Scheduled/Recording/Recorded/Canceled/Failed/DoesNotExists/Deleted
        /// </summary>
        [DataMember(Name = "recording_state")]
        [JsonProperty("recording_state")]
        [XmlElement(ElementName = "recording_state")]
        public KalturaRecordingStatus RecordingStatus { get; set; }

        /// <summary>
        /// Recording Type: Single/Series
        /// </summary>
        [DataMember(Name = "recording_type")]
        [JsonProperty("recording_type")]
        [XmlElement(ElementName = "recording_type")]
        public KalturaRecordingStatus RecordingType { get; set; }

        /// <summary>
        /// The date when the record is no longer available
        /// </summary>
        [DataMember(Name = "last_availability_date")]
        [JsonProperty("last_availability_date")]
        [XmlElement(ElementName = "last_availability_date")]
        public long LastAvailabilityDate { get; set; }

        /// <summary>
        /// Media Asset
        /// </summary>
        [DataMember(Name = "media_asset")]
        [JsonProperty("media_asset")]
        [XmlElement(ElementName = "media_asset", IsNullable = true)]
        public KalturaAssetInfo MediaAsset { get; set; }


    }

}