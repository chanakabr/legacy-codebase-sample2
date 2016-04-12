using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
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
        [XmlElement(ElementName = "recording_id")]
        public string RecordingId { get; set; }

        /// <summary>
        /// Recording state: Scheduled/Recording/Recorded/Canceled/Failed/DoesNotExists/Deleted
        /// </summary>
        [DataMember(Name = "recording_state")]
        [JsonProperty("recording_state")]
        [XmlElement(ElementName = "recording_state")]
        public KalturaRecordingStatus RecordingStatus { get; set; }

    }

}