using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Clients;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Playback adapter partner configuration
    /// </summary>
    public partial class KalturaPlaybackPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// default adapter configuration for: vod, epg,recording.
        /// </summary>
        [DataMember(Name = "defaultPlayback")]
        [JsonProperty("defaultPlayback")]
        [XmlElement(ElementName = "defaultPlayback", IsNullable = true)] 
        public KalturaDefaultPlayback DefaultPlayback { get; set; }

        protected override KalturaPartnerConfigurationType ConfigurationType { get { return KalturaPartnerConfigurationType.Playback; } }

        internal override bool Update(int groupId)
        {
            Func<PlaybackPartnerConfig, Status> partnerConfigFunc =
                           (PlaybackPartnerConfig playbackPartnerConfig) =>
                               PartnerConfigurationManager.UpdatePlaybackConfig(groupId, playbackPartnerConfig);

            ClientUtils.GetResponseStatusFromWS<KalturaPlaybackPartnerConfig, PlaybackPartnerConfig>(partnerConfigFunc, this);

            return true;
        }
    }
    public partial class KalturaDefaultPlayback : KalturaOTTObject
    {
        /// <summary>
        /// Default adapter for vod
        /// </summary>
        [DataMember(Name = "vodAdapter")]
        [JsonProperty("vodAdapter")]
        [XmlElement(ElementName = "vodAdapter")]
        public long VodAdapter { get; set; }

        /// <summary>
        /// Default adapter for epg
        /// </summary>
        [DataMember(Name = "epgAdapter")]
        [JsonProperty("epgAdapter")]
        [XmlElement(ElementName = "epgAdapter")]
        public long EpgAdapter { get; set; }

        /// <summary>
        /// Default adapter for recording
        /// </summary>
        [DataMember(Name = "recordingAdapter")]
        [JsonProperty("recordingAdapter")]
        [XmlElement(ElementName = "recordingAdapter")]
        public long RecordingAdapter { get; set; }       
    }
}