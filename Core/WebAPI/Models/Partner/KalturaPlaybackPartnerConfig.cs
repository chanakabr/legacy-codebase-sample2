using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Clients;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Playback adapter partner configuration
    /// </summary>
    public partial class KalturaPlaybackPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// default adapter configuration for: media, epg,recording.
        /// </summary>
        [DataMember(Name = "defaultAdapters")]
        [JsonProperty("defaultAdapters")]
        [XmlElement(ElementName = "defaultAdapters", IsNullable = true)] 
        public KalturaDefaultPlaybackAdapters DefaultAdapters { get; set; }

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
    public partial class KalturaDefaultPlaybackAdapters : KalturaOTTObject
    {
        /// <summary>
        /// Default adapter identifier for media
        /// </summary>
        [DataMember(Name = "mediaAdapterId")]
        [JsonProperty("mediaAdapterId")]
        [XmlElement(ElementName = "mediaAdapterId")]
        [SchemeProperty(MinInteger = 1)]
        public long MediaAdapterId { get; set; }

        /// <summary>
        /// Default adapter identifier for epg
        /// </summary>
        [DataMember(Name = "epgAdapterId")]
        [JsonProperty("epgAdapterId")]
        [XmlElement(ElementName = "epgAdapterId")]
        [SchemeProperty(MinInteger = 1)]
        public long EpgAdapterId { get; set; }

        /// <summary>
        /// Default adapter identifier for recording
        /// </summary>
        [DataMember(Name = "recordingAdapterId")]
        [JsonProperty("recordingAdapterId")]
        [XmlElement(ElementName = "recordingAdapterId")]
        [SchemeProperty(MinInteger = 1)]
        public long RecordingAdapterId { get; set; }       
    }
}