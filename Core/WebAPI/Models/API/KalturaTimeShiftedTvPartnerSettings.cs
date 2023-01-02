using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public partial class KalturaTimeShiftedTvPartnerSettings : KalturaOTTObject
    {
        /// <summary>
        /// Is catch-up enabled
        /// </summary>
        [DataMember(Name = "catchUpEnabled")]
        [JsonProperty("catchUpEnabled")]
        [XmlElement(ElementName = "catchUpEnabled", IsNullable = true)]
        [OldStandardProperty("catch_up_enabled")]
        public bool? CatchUpEnabled { get; set; }

        /// <summary>
        /// Is c-dvr enabled
        /// </summary>
        [DataMember(Name = "cdvrEnabled")]
        [JsonProperty("cdvrEnabled")]
        [XmlElement(ElementName = "cdvrEnabled", IsNullable = true)]
        [OldStandardProperty("cdvr_enabled")]
        public bool? CdvrEnabled { get; set; }

        /// <summary>
        /// Is start-over enabled
        /// </summary>
        [DataMember(Name = "startOverEnabled")]
        [JsonProperty("startOverEnabled")]
        [XmlElement(ElementName = "startOverEnabled", IsNullable = true)]
        [OldStandardProperty("start_over_enabled")]
        public bool? StartOverEnabled { get; set; }

        /// <summary>
        /// Is trick-play enabled
        /// </summary>
        [DataMember(Name = "trickPlayEnabled")]
        [JsonProperty("trickPlayEnabled")]
        [XmlElement(ElementName = "trickPlayEnabled", IsNullable = true)]
        [OldStandardProperty("trick_play_enabled")]
        public bool? TrickPlayEnabled { get; set; }

        /// <summary>
        /// Is recording schedule window enabled
        /// </summary>
        [DataMember(Name = "recordingScheduleWindowEnabled")]
        [JsonProperty("recordingScheduleWindowEnabled")]
        [XmlElement(ElementName = "recordingScheduleWindowEnabled", IsNullable = true)]
        [OldStandardProperty("recording_schedule_window_enabled")]
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
        [OldStandardProperty("catch_up_buffer_length")]
        public long? CatchUpBufferLength { get; set; }

        /// <summary>
        /// Trick play buffer length
        /// </summary>
        [DataMember(Name = "trickPlayBufferLength")]
        [JsonProperty("trickPlayBufferLength")]
        [XmlElement(ElementName = "trickPlayBufferLength", IsNullable = true)]
        [OldStandardProperty("trick_play_buffer_length")]
        public long? TrickPlayBufferLength { get; set; }

        /// <summary>
        /// Recording schedule window. Indicates how long (in minutes) after the program starts it is allowed to schedule the recording
        /// </summary>
        [DataMember(Name = "recordingScheduleWindow")]
        [JsonProperty("recordingScheduleWindow")]
        [XmlElement(ElementName = "recordingScheduleWindow", IsNullable = true)]
        [OldStandardProperty("recording_schedule_window")]
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

        /// <summary>
        ///  Quota Policy
        /// </summary>
        [DataMember(Name = "quotaOveragePolicy")]
        [JsonProperty(PropertyName = "quotaOveragePolicy", NullValueHandling = NullValueHandling.Ignore)]
        [XmlElement(ElementName = "quotaOveragePolicy", IsNullable = true)]
        public KalturaQuotaOveragePolicy? QuotaOveragePolicy { get; set; }

        /// <summary>
        ///  Protection Policy
        /// </summary>
        [DataMember(Name = "protectionPolicy")]
        [JsonProperty(PropertyName = "protectionPolicy", NullValueHandling = NullValueHandling.Ignore)]
        [XmlElement(ElementName = "protectionPolicy", IsNullable = true)]
        public KalturaProtectionPolicy? ProtectionPolicy { get; set; }

        /// <summary>
        /// The time in days for recovery recording that was delete by Auto Delete .
        /// </summary>
        [DataMember(Name = "recoveryGracePeriod")]
        [JsonProperty("recoveryGracePeriod")]
        [XmlElement(ElementName = "recoveryGracePeriod", IsNullable = true)]
        [SchemeProperty(MinInteger = 0)]
        public int? RecoveryGracePeriod { get; set; }

        /// <summary>
        ///  Is private copy enabled for the account
        /// </summary>
        [DataMember(Name = "privateCopyEnabled")]
        [JsonProperty("privateCopyEnabled")]
        [XmlElement(ElementName = "privateCopyEnabled", IsNullable = true)]
        public bool? PrivateCopyEnabled { get; set; }
        
        /// <summary>
        /// Quota in seconds 
        /// </summary>
        [DataMember(Name = "defaultQuota")]
        [JsonProperty("defaultQuota")]
        [XmlElement(ElementName = "defaultQuota", IsNullable = true)]
        [SchemeProperty(MinInteger = -1)]
        public int? DefaultQuota { get; set; }
        
        /// <summary>
        ///  Define whatever the partner enables the Personal Padding and Immediate / Stop recording services to the partner. Default value should be FALSE
        /// </summary>
        [DataMember(Name = "personalizedRecording")]
        [JsonProperty("personalizedRecording")]
        [XmlElement(ElementName = "personalizedRecording", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public bool? PersonalizedRecording { get; set; }
        
        /// <summary>
        ///  Define the max allowed number of parallel recordings. Default NULL unlimited
        /// </summary>
        [DataMember(Name = "maxRecordingConcurrency")]
        [JsonProperty("maxRecordingConcurrency")]
        [XmlElement(ElementName = "maxRecordingConcurrency", IsNullable = true)]
        [SchemeProperty(MinInteger = 0, MaxInteger = 9999, IsNullable = true)]
        public int? MaxRecordingConcurrency { get; set; }
        
        /// <summary>
        ///  Define the max grace margin time for overlapping recording. Default NULL 0 margin
        /// </summary>
        [DataMember(Name = "maxConcurrencyMargin")]
        [JsonProperty("maxConcurrencyMargin")]
        [XmlElement(ElementName = "maxConcurrencyMargin", IsNullable = true)]
        [SchemeProperty(MinInteger = 0, MaxInteger = 9999, IsNullable = true)]
        public int? MaxConcurrencyMargin { get; set; }
    }
}