using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    [OldStandard("catchUpEnabled", "catch_up_enabled")]
    [OldStandard("cdvrEnabled", "cdvr_enabled")]
    [OldStandard("startOverEnabled", "start_over_enabled")]
    [OldStandard("trickPlayEnabled", "trick_play_enabled")]
    [OldStandard("recordingScheduleWindowEnabled", "recording_schedule_window_enabled")]
    [OldStandard("catchUpBufferLength", "catch_up_buffer_length")]
    [OldStandard("trickPlayBufferLength", "trick_play_buffer_length")]
    [OldStandard("recordingScheduleWindow", "recording_schedule_window")]
    public class KalturaTimeShiftedTvPartnerSettings : KalturaOTTObject
    {
        /// <summary>
        /// Is catch-up enabled
        /// </summary>
        [DataMember(Name = "catchUpEnabled")]
        [JsonProperty("catchUpEnabled")]
        [XmlElement(ElementName = "catchUpEnabled", IsNullable = true)]
        public bool? CatchUpEnabled { get; set; }

        /// <summary>
        /// Is c-dvr enabled
        /// </summary>
        [DataMember(Name = "cdvrEnabled")]
        [JsonProperty("cdvrEnabled")]
        [XmlElement(ElementName = "cdvrEnabled", IsNullable = true)]
        public bool? CdvrEnabled { get; set; }

        /// <summary>
        /// Is start-over enabled
        /// </summary>
        [DataMember(Name = "startOverEnabled")]
        [JsonProperty("startOverEnabled")]
        [XmlElement(ElementName = "startOverEnabled", IsNullable = true)]
        public bool? StartOverEnabled { get; set; }

        /// <summary>
        /// Is trick-play enabled
        /// </summary>
        [DataMember(Name = "trickPlayEnabled")]
        [JsonProperty("trickPlayEnabled")]
        [XmlElement(ElementName = "trickPlayEnabled", IsNullable = true)]
        public bool? TrickPlayEnabled { get; set; }

        /// <summary>
        /// Is recording schedule window enabled
        /// </summary>
        [DataMember(Name = "recordingScheduleWindowEnabled")]
        [JsonProperty("recordingScheduleWindowEnabled")]
        [XmlElement(ElementName = "recordingScheduleWindowEnabled", IsNullable = true)]
        public bool? RecordingScheduleWindowEnabled { get; set; }

        /// <summary>
        /// Is recording protection enabled
        /// </summary>
        [DataMember(Name = "protectionEnabled")]
        [JsonProperty("protectionEnabled")]
        [XmlElement(ElementName = "protectionEnabled", IsNullable = true)]
        public bool? ProtectionEnabled { get; set; }

        /// <summary>
        /// Catch-up buffer length
        /// </summary>
        [DataMember(Name = "catchUpBufferLength")]
        [JsonProperty("catchUpBufferLength")]
        [XmlElement(ElementName = "catchUpBufferLength", IsNullable = true)]
        public long? CatchUpBufferLength { get; set; }

        /// <summary>
        /// Trick play buffer length
        /// </summary>
        [DataMember(Name = "trickPlayBufferLength")]
        [JsonProperty("trickPlayBufferLength")]
        [XmlElement(ElementName = "trickPlayBufferLength", IsNullable = true)]
        public long? TrickPlayBufferLength { get; set; }

        /// <summary>
        /// Recording schedule window. Indicates how long (in minutes) after the program starts it is allowed to schedule the recording
        /// </summary>
        [DataMember(Name = "recordingScheduleWindow")]
        [JsonProperty("recordingScheduleWindow")]
        [XmlElement(ElementName = "recordingScheduleWindow", IsNullable = true)]
        public long? RecordingScheduleWindow { get; set; }

        /// <summary>
        /// Indicates how long (in seconds) before the program starts the recording will begin
        /// </summary>
        [DataMember(Name = "paddingBeforeProgramStarts")]
        [JsonProperty("paddingBeforeProgramStarts")]
        [XmlElement(ElementName = "paddingBeforeProgramStarts", IsNullable = true)]
        [SchemeProperty(MinLong = 0)]
        public long? PaddingBeforeProgramStarts { get; set; }

        /// <summary>
        /// Indicates how long (in seconds) after the program ends the recording will end
        /// </summary>
        [DataMember(Name = "paddingAfterProgramEnds")]
        [JsonProperty("paddingAfterProgramEnds")]
        [XmlElement(ElementName = "paddingAfterProgramEnds", IsNullable = true)]
        [SchemeProperty(MinLong = 0)]
        public long? PaddingAfterProgramEnds { get; set; }

        /// <summary>
        /// Specify the time in days that a recording should be protected. Start time begins at protection request.
        /// </summary>
        [DataMember(Name = "protectionPeriod")]
        [JsonProperty("protectionPeriod")]
        [XmlElement(ElementName = "protectionPeriod", IsNullable = true)]
        [SchemeProperty(MinInteger = 0)]
        public int? ProtectionPeriod { get; set; }

        /// <summary>
        /// Indicates how many percent of the quota can be used for protection
        /// </summary>
        [DataMember(Name = "protectionQuotaPercentage")]
        [JsonProperty("protectionQuotaPercentage")]
        [XmlElement(ElementName = "protectionQuotaPercentage", IsNullable = true)]
        [SchemeProperty(MinInteger = 10, MaxInteger = 100)]
        public int? ProtectionQuotaPercentage { get; set; }

        /// <summary>
        /// Specify the time in days that a recording should be kept for user. Start time begins with the program end date.
        /// </summary>
        [DataMember(Name = "recordingLifetimePeriod")]
        [JsonProperty("recordingLifetimePeriod")]
        [XmlElement(ElementName = "recordingLifetimePeriod", IsNullable = true)]
        [SchemeProperty(MinInteger = 0)]
        public int? RecordingLifetimePeriod { get; set; }

        /// <summary>
        /// The time in days before the recording lifetime is due from which the client should be able to warn user about deletion.
        /// </summary>
        [DataMember(Name = "cleanupNoticePeriod")]
        [JsonProperty("cleanupNoticePeriod")]
        [XmlElement(ElementName = "cleanupNoticePeriod", IsNullable = true)]
        [SchemeProperty(MinInteger = 0)]
        public int? CleanupNoticePeriod { get; set; }

        /// <summary>
        /// Is recording of series enabled
        /// </summary>
        [DataMember(Name = "seriesRecordingEnabled")]
        [JsonProperty("seriesRecordingEnabled")]
        [XmlElement(ElementName = "seriesRecordingEnabled", IsNullable = true)]
        public bool? SeriesRecordingEnabled { get; set; }

         /// <summary>
        ///  Is recording playback for non-entitled channel enables 
        /// </summary>
        [DataMember(Name = "nonEntitledChannelPlaybackEnabled")]
        [JsonProperty("nonEntitledChannelPlaybackEnabled")]
        [XmlElement(ElementName = "nonEntitledChannelPlaybackEnabled", IsNullable = true)]
        public bool? NonEntitledChannelPlaybackEnabled { get; set; }

        /// <summary>
        ///  Is recording playback for non-existing channel enables 
        /// </summary>
        [DataMember(Name = "nonExistingChannelPlaybackEnabled")]
        [JsonProperty("nonExistingChannelPlaybackEnabled")]
        [XmlElement(ElementName = "nonExistingChannelPlaybackEnabled", IsNullable = true)]
        public bool? NonExistingChannelPlaybackEnabled { get; set; }
        

    }
}