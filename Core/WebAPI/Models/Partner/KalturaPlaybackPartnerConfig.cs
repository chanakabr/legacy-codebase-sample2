using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Clients;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Playback adapter partner configuration
    /// </summary>
    public partial class KalturaPlaybackPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// Default adapter for vod
        /// </summary>
        [DataMember(Name = "vodDefaultAdapter")]
        [JsonProperty("vodDefaultAdapter")]
        [XmlElement(ElementName = "vodDefaultAdapter")]
        public long? VodDefaultAdapter { get; set; }

        /// <summary>
        /// Default adapter for epg
        /// </summary>
        [DataMember(Name = "epgDefaultAdapter")]
        [JsonProperty("epgDefaultAdapter")]
        [XmlElement(ElementName = "epgDefaultAdapter")]
        public long? EpgDefaultAdapter { get; set; }

        /// <summary>
        /// Default adapter for recording
        /// </summary>
        [DataMember(Name = "recordingDefaultAdapter")]
        [JsonProperty("recordingDefaultAdapter")]
        [XmlElement(ElementName = "recordingDefaultAdapter")]
        public long? RecordingDefaultAdapter { get; set; }

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
}