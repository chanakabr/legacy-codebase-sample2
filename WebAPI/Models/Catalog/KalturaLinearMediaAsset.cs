using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Linear media asset info
    /// </summary>
    [Serializable]
    public class KalturaLinearMediaAsset : KalturaMediaAsset
    {
        /// <summary>
        /// Id of epg channel
        /// </summary>
        [DataMember(Name = "epgChannelId")]
        [JsonProperty(PropertyName = "epgChannelId")]
        [XmlElement(ElementName = "epgChannelId")]
        [SchemeProperty(ReadOnly = true)]
        public long EpgChannelId { get; set; }

        /// <summary>
        /// Enable CDVR, configuration only
        /// </summary>
        [DataMember(Name = "enableCdvr")]
        [JsonProperty(PropertyName = "enableCdvr")]
        [XmlElement(ElementName = "enableCdvr")]
        public KalturaTstvState? EnableCdvr { get; set; }

        /// <summary>
        /// Enable catch-up, configuration only
        /// </summary>
        [DataMember(Name = "enableCatchUp")]
        [JsonProperty(PropertyName = "enableCatchUp")]
        [XmlElement(ElementName = "enableCatchUp")]
        public KalturaTstvState? EnableCatchUp { get; set; }

        /// <summary>
        /// Enable start over, configuration only
        /// </summary>
        [DataMember(Name = "enableStartOver")]
        [JsonProperty(PropertyName = "enableStartOver")]
        [XmlElement(ElementName = "enableStartOver")]
        public KalturaTstvState? EnableStartOver { get; set; }

        /// <summary>
        /// Catch-up buffer, configuration only
        /// </summary>
        [DataMember(Name = "catchUpBuffer")]
        [JsonProperty(PropertyName = "catchUpBuffer")]
        [XmlElement(ElementName = "catchUpBuffer")]
        public long? CatchUpBuffer { get; set; }

        /// <summary>
        /// Trick-play buffer, configuration only
        /// </summary>
        [DataMember(Name = "trickPlayBuffer")]
        [JsonProperty(PropertyName = "trickPlayBuffer")]
        [XmlElement(ElementName = "trickPlayBuffer")]
        public long? TrickPlayBuffer { get; set; }

        /// <summary>
        /// Enable Recording playback for non entitled channel, configuration only
        /// </summary>
        [DataMember(Name = "enableRecordingPlaybackNonEntitledChannel")]
        [JsonProperty(PropertyName = "enableRecordingPlaybackNonEntitledChannel")]
        [XmlElement(ElementName = "enableRecordingPlaybackNonEntitledChannel")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaTstvState? EnableRecordingPlaybackNonEntitledChannel { get; set; }

        /// <summary>
        /// Enable trick-play, configuration only
        /// </summary>
        [DataMember(Name = "enableTrickPlay")]
        [JsonProperty(PropertyName = "enableTrickPlay")]
        [XmlElement(ElementName = "enableTrickPlay")]
        public KalturaTstvState? EnableTrickPlay { get; set; }

        /// <summary>
        /// External identifier used when ingesting programs for this linear media asset
        /// </summary>
        [DataMember(Name = "externalIngestId")]
        [JsonProperty(PropertyName = "externalIngestId")]
        [XmlElement(ElementName = "externalIngestId")]
        public string ExternalIngestId { get; set; }

        /// <summary>
        /// External identifier for the CDVR
        /// </summary>
        [DataMember(Name = "externalCdvrId")]
        [JsonProperty(PropertyName = "externalCdvrId")]
        [XmlElement(ElementName = "externalCdvrId")]
        public string ExternalCdvrId { get; set; }

        /// <summary>
        /// Is CDVR enabled for this asset
        /// </summary>
        [DataMember(Name = "cdvrEnabaled")]
        [JsonProperty(PropertyName = "cdvrEnabaled")]
        [XmlElement(ElementName = "cdvrEnabaled")]
        [SchemeProperty(ReadOnly = true)]
        public bool CdvrEnabled { get; set; }

        /// <summary>
        /// Is catch-up enabled for this asset
        /// </summary>
        [DataMember(Name = "catchUpEnabled")]
        [JsonProperty(PropertyName = "catchUpEnabled")]
        [XmlElement(ElementName = "catchUpEnabled")]
        [SchemeProperty(ReadOnly = true)]
        public bool CatchUpEnabled { get; set; }

        /// <summary>
        /// Is start over enabled for this asset
        /// </summary>
        [DataMember(Name = "startOverEnabled")]
        [JsonProperty(PropertyName = "startOverEnabled")]
        [XmlElement(ElementName = "startOverEnabled")]
        [SchemeProperty(ReadOnly = true)]
        public bool StartOverEnabled { get; set; }

        /// <summary>
        /// buffer Catch-up
        /// </summary>
        [DataMember(Name = "bufferCatchUp")]
        [JsonProperty(PropertyName = "bufferCatchUp")]
        [XmlElement(ElementName = "bufferCatchUp")]
        [SchemeProperty(ReadOnly = true)]
        public long BufferCatchUp { get; set; }

        /// <summary>
        /// buffer Trick-play 
        /// </summary>
        [DataMember(Name = "bufferTrickPlay")]
        [JsonProperty(PropertyName = "bufferTrickPlay")]
        [XmlElement(ElementName = "bufferTrickPlay")]
        [SchemeProperty(ReadOnly = true)]
        public long BufferTrickPlay { get; set; }

        /// <summary>
        /// Is recording playback for non entitled channel enabled for this asset
        /// </summary>
        [DataMember(Name = "recordingPlaybackNonEntitledChannelEnabled")]
        [JsonProperty(PropertyName = "recordingPlaybackNonEntitledChannelEnabled")]
        [XmlElement(ElementName = "recordingPlaybackNonEntitledChannelEnabled")]
        [SchemeProperty(ReadOnly = true)]
        public bool RecordingPlaybackNonEntitledChannelEnabled { get; set; }

        /// <summary>
        /// Is trick-play enabled for this asset
        /// </summary>
        [DataMember(Name = "trickPlayEnabled")]
        [JsonProperty(PropertyName = "trickPlayEnabled")]
        [XmlElement(ElementName = "trickPlayEnabled")]
        [SchemeProperty(ReadOnly = true)]
        public bool TrickPlayEnabled { get; set; }
    }

    [Serializable]
    public enum KalturaTstvState
    {
        INHERITED = 0,
        ENABLED = 1,
        DISABLED = 2
    }

}