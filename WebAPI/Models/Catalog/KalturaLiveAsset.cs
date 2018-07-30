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
    public partial class KalturaLiveAsset : KalturaMediaAsset
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
        /// buffer Catch-up, configuration only
        /// </summary>
        [DataMember(Name = "bufferCatchUpSetting")]
        [JsonProperty(PropertyName = "bufferCatchUpSetting")]
        [XmlElement(ElementName = "bufferCatchUpSetting")]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL, MinLong = 0)]        
        public long? BufferCatchUp { get; set; }

        /// <summary>
        /// buffer Trick-play, configuration only
        /// </summary>
        [DataMember(Name = "bufferTrickPlaySetting")]
        [JsonProperty(PropertyName = "bufferTrickPlaySetting")]
        [XmlElement(ElementName = "bufferTrickPlaySetting")]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL, MinLong = 0)]        
        public long? BufferTrickPlay { get; set; }

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
        [DataMember(Name = "externalEpgIngestId")]
        [JsonProperty(PropertyName = "externalEpgIngestId")]
        [XmlElement(ElementName = "externalEpgIngestId")]
        [SchemeProperty(MinLength = 1, MaxLength = 255)]
        public string ExternalEpgIngestId { get; set; }

        /// <summary>
        /// External identifier for the CDVR
        /// </summary>
        [DataMember(Name = "externalCdvrId")]
        [JsonProperty(PropertyName = "externalCdvrId")]
        [XmlElement(ElementName = "externalCdvrId")]
        [SchemeProperty(MinLength = 1, MaxLength = 255)]
        public string ExternalCdvrId { get; set; }

        /// <summary>
        /// Is CDVR enabled for this asset
        /// </summary>
        [DataMember(Name = "enableCdvr")]
        [JsonProperty(PropertyName = "enableCdvr")]
        [XmlElement(ElementName = "enableCdvr")]
        [SchemeProperty(ReadOnly = true)]        
        public bool CdvrEnabled { get; set; }

        /// <summary>
        /// Is catch-up enabled for this asset
        /// </summary>
        [DataMember(Name = "enableCatchUp")]
        [JsonProperty(PropertyName = "enableCatchUp")]
        [XmlElement(ElementName = "enableCatchUp")]
        [SchemeProperty(ReadOnly = true)]        
        public bool CatchUpEnabled { get; set; }

        /// <summary>
        /// Is start over enabled for this asset
        /// </summary>
        [DataMember(Name = "enableStartOver")]
        [JsonProperty(PropertyName = "enableStartOver")]
        [XmlElement(ElementName = "enableStartOver")]
        [SchemeProperty(ReadOnly = true)]        
        public bool StartOverEnabled { get; set; }

        /// <summary>
        /// summed Catch-up buffer, the TimeShiftedTvPartnerSettings are also taken into consideration
        /// </summary>
        [DataMember(Name = "catchUpBuffer")]
        [JsonProperty(PropertyName = "catchUpBuffer")]
        [XmlElement(ElementName = "catchUpBuffer")]
        [SchemeProperty(ReadOnly = true)]
        public long SummedCatchUpBuffer { get; set; }

        /// <summary>
        /// summed Trick-play buffer, the TimeShiftedTvPartnerSettings are also taken into consideration
        /// </summary>
        [DataMember(Name = "trickPlayBuffer")]
        [JsonProperty(PropertyName = "trickPlayBuffer")]
        [XmlElement(ElementName = "trickPlayBuffer")]
        [SchemeProperty(ReadOnly = true)]
        public long SummedTrickPlayBuffer { get; set; }

        /// <summary>
        /// Is recording playback for non entitled channel enabled for this asset
        /// </summary>
        [DataMember(Name = "enableRecordingPlaybackNonEntitledChannel")]
        [JsonProperty(PropertyName = "enableRecordingPlaybackNonEntitledChannel")]
        [XmlElement(ElementName = "enableRecordingPlaybackNonEntitledChannel")]
        [SchemeProperty(ReadOnly = true)]
        public bool RecordingPlaybackNonEntitledChannelEnabled { get; set; }

        /// <summary>
        /// Is trick-play enabled for this asset
        /// </summary>
        [DataMember(Name = "enableTrickPlay")]
        [JsonProperty(PropertyName = "enableTrickPlay")]
        [XmlElement(ElementName = "enableTrickPlay")]
        [SchemeProperty(ReadOnly = true)]
        public bool TrickPlayEnabled { get; set; }

        /// <summary>
        /// channel type, possible values: UNKNOWN, DTT, OTT, DTT_AND_OTT
        /// </summary>
        [DataMember(Name = "channelType")]
        [JsonProperty(PropertyName = "channelType")]
        [XmlElement(ElementName = "channelType")]        
        public KalturaLinearChannelType? ChannelType { get; set; }

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

            if (BufferCatchUp == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "bufferCatchUpSetting");
            }

            if (BufferTrickPlay == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "bufferTrickPlaySetting");
            }

            if (string.IsNullOrEmpty(ExternalEpgIngestId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalEpgIngestId");
            }
        }

        internal void ValidateForUpdate()
        {
            if (ExternalEpgIngestId != null && ExternalEpgIngestId == string.Empty)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalEpgIngestId");
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

    [Serializable]
    public enum KalturaLinearChannelType
    {
        UNKNOWN = 0,
        DTT = 1,
        OTT = 2,
        DTT_AND_OTT = 3
    }

}