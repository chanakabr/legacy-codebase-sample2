using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public class KalturaTimeShiftedTvPartnerSettings : KalturaOTTObject
    {
        /// <summary>
        /// Is catch-up enabled
        /// </summary>
        [DataMember(Name = "catch_up_enabled")]
        [JsonProperty("catch_up_enabled")]
        [XmlElement(ElementName = "catch_up_enabled", IsNullable = true)]
        public bool? CatchUpEnabled { get; set; }

        /// <summary>
        /// Is c-dvr enabled
        /// </summary>
        [DataMember(Name = "cdvr_enabled")]
        [JsonProperty("cdvr_enabled")]
        [XmlElement(ElementName = "cdvr_enabled", IsNullable = true)]
        public bool? CdvrEnabled { get; set; }

        /// <summary>
        /// Is start-over enabled
        /// </summary>
        [DataMember(Name = "start_over_enabled")]
        [JsonProperty("start_over_enabled")]
        [XmlElement(ElementName = "start_over_enabled", IsNullable = true)]
        public bool? StartOverEnabled { get; set; }

        /// <summary>
        /// Is trick-play enabled
        /// </summary>
        [DataMember(Name = "trick_play_enabled")]
        [JsonProperty("trick_play_enabled")]
        [XmlElement(ElementName = "trick_play_enabled", IsNullable = true)]
        public bool? TrickPlayEnabled { get; set; }

        /// <summary>
        /// Is recording schedule window enabled
        /// </summary>
        [DataMember(Name = "recording_schedule_window_enabled")]
        [JsonProperty("recording_schedule_window_enabled")]
        [XmlElement(ElementName = "recording_schedule_window_enabled", IsNullable = true)]
        public bool? RecordingScheduleWindowEnabled { get; set; }

        /// <summary>
        /// Catch-up buffer length
        /// </summary>
        [DataMember(Name = "catch_up_buffer_length")]
        [JsonProperty("catch_up_buffer_length")]
        [XmlElement(ElementName = "catch_up_buffer_length", IsNullable = true)]
        public long? CatchUpBufferLength { get; set; }

        /// <summary>
        /// Trick play buffer length
        /// </summary>
        [DataMember(Name = "trick_play_buffer_length")]
        [JsonProperty("trick_play_buffer_length")]
        [XmlElement(ElementName = "trick_play_buffer_length", IsNullable = true)]
        public long? TrickPlayBufferLength { get; set; }

        /// <summary>
        /// Recording schedule window. Indicates how long (in minutes) after the program starts it is allowed to schedule the recording
        /// </summary>
        [DataMember(Name = "recording_schedule_window")]
        [JsonProperty("recording_schedule_window")]
        [XmlElement(ElementName = "recording_schedule_window", IsNullable = true)]
        public long? RecordingScheduleWindow { get; set; }

    }
}