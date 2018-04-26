using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Filters;
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
        /// Enable CDVR, configuration only
        /// </summary>
        [DataMember(Name = "enableCdvrState")]
        [JsonProperty(PropertyName = "enableCdvrState")]
        [XmlElement(ElementName = "enableCdvrState")]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL)]
        public KalturaTimeShiftedTvState? EnableCdvrState { get; set; }

        /// <summary>
        /// Enable catch-up, configuration only
        /// </summary>
        [DataMember(Name = "enableCatchUpState")]
        [JsonProperty(PropertyName = "enableCatchUpState")]
        [XmlElement(ElementName = "enableCatchUpState")]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL)]
        public KalturaTimeShiftedTvState? EnableCatchUpState { get; set; }

        /// <summary>
        /// Enable start over, configuration only
        /// </summary>
        [DataMember(Name = "enableStartOverState")]
        [JsonProperty(PropertyName = "enableStartOverState")]
        [XmlElement(ElementName = "enableStartOverState")]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL)]
        public KalturaTimeShiftedTvState? EnableStartOverState { get; set; }

        /// <summary>
        /// Catch-up buffer, configuration only
        /// </summary>
        [DataMember(Name = "catchUpBuffer")]
        [JsonProperty(PropertyName = "catchUpBuffer")]
        [XmlElement(ElementName = "catchUpBuffer")]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL, MinLong = 0)]
        public long? CatchUpBuffer { get; set; }

        /// <summary>
        /// Trick-play buffer, configuration only
        /// </summary>
        [DataMember(Name = "trickPlayBuffer")]
        [JsonProperty(PropertyName = "trickPlayBuffer")]
        [XmlElement(ElementName = "trickPlayBuffer")]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL, MinLong = 0)]
        public long? TrickPlayBuffer { get; set; }

        /// <summary>
        /// Enable Recording playback for non entitled channel, configuration only
        /// </summary>
        [DataMember(Name = "enableRecordingPlaybackNonEntitledChannelState")]
        [JsonProperty(PropertyName = "enableRecordingPlaybackNonEntitledChannelState")]
        [XmlElement(ElementName = "enableRecordingPlaybackNonEntitledChannelState")]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL)]
        public KalturaTimeShiftedTvState? EnableRecordingPlaybackNonEntitledChannelState { get; set; }

        /// <summary>
        /// Enable trick-play, configuration only
        /// </summary>
        [DataMember(Name = "enableTrickPlayState")]
        [JsonProperty(PropertyName = "enableTrickPlayState")]
        [XmlElement(ElementName = "enableTrickPlayState")]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL)]
        public KalturaTimeShiftedTvState? EnableTrickPlayState { get; set; }

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

        internal void ValidateForInsert()
        {
            if (EnableCatchUpState == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "enableCatchUpState");
            }

            if (EnableCdvrState == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "enableCdvrState");
            }

            if (EnableRecordingPlaybackNonEntitledChannelState == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "enableRecordingPlaybackNonEntitledChannelState");
            }

            if (EnableStartOverState == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "enableStartOverState");
            }

            if (EnableTrickPlayState == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "enableTrickPlayState");
            }

            if (CatchUpBuffer == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "catchUpBufferState");
            }

            if (TrickPlayBuffer == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "trickPlayBufferState");
            }

            if (string.IsNullOrEmpty(ExternalIngestId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalIngestId");
            }

            if (string.IsNullOrEmpty(ExternalCdvrId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalCdvrId");
            }
        }

        internal void ValidateForUpdate()
        {
            if (ExternalIngestId != null && ExternalIngestId == string.Empty)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalIngestId");
            }

            if (ExternalCdvrId != null && ExternalCdvrId == string.Empty)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalCdvrId");
            }
        }
    }

    [Serializable]
    public enum KalturaTimeShiftedTvState
    {
        INHERITED = 0,
        ENABLED = 1,
        DISABLED = 2
    }

}