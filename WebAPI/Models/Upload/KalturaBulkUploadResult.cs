using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    public enum KalturaBulkUploadResultStatus
    {
        Error = 1,
        Ok = 2,
        InProgress = 3
    }

    /// <summary>
    /// Bulk Upload Result
    /// </summary>
    [Serializable]
    public abstract partial class KalturaBulkUploadResult : KalturaOTTObject
    {
        /// <summary>
        /// the result ObjectId (assetId, userId etc)
        /// </summary>
        [DataMember(Name = "objectId")]
        [JsonProperty("objectId")]
        [XmlElement(ElementName = "objectId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? ObjectId { get; set; }

        /// <summary>
        /// result index
        /// </summary>
        [DataMember(Name = "index")]
        [JsonProperty("index")]
        [XmlElement(ElementName = "index")]
        [SchemeProperty(ReadOnly = true)]
        public int Index { get; set; }

        /// <summary>
        /// Bulk upload identifier
        /// </summary>
        [DataMember(Name = "bulkUploadId")]
        [JsonProperty("bulkUploadId")]
        [XmlElement(ElementName = "bulkUploadId")]
        [SchemeProperty(ReadOnly = true)]
        public long BulkUploadId { get; set; }

        /// <summary>
        /// status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaBulkUploadResultStatus Status { get; set; }

        /// <summary>
        /// A list of errors
        /// </summary>
        [DataMember(Name = "errors")]
        [JsonProperty("error")]
        [XmlArray(ElementName = "error", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaMessage> Errors { get; set; }

        /// <summary>
        /// A list of warnings
        /// </summary>
        [DataMember(Name = "warnings")]
        [JsonProperty("warnings")]
        [XmlArray(ElementName = "warnings", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaMessage> Warnings { get; set; }
    }

    [Serializable]
    public abstract partial class KalturaBulkUploadAssetResult : KalturaBulkUploadResult
    {
        /// <summary>
        /// Identifies the asset type (EPG, Recording, Movie, TV Series, etc). 
        /// Possible values: 0 – EPG linear programs, 1 - Recording; or any asset type ID according to the asset types IDs defined in the system.
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public int? Type { get; set; }

        /// <summary>
        /// External identifier for the asset
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty(PropertyName = "externalId")]
        [XmlElement(ElementName = "externalId")]
        [SchemeProperty(ReadOnly = true)]
        public string ExternalId { get; set; }
    }

    [Serializable]
    public partial class KalturaBulkUploadMediaAssetResult : KalturaBulkUploadAssetResult
    {
    }

    [Serializable]
    public partial class KalturaBulkUploadXmlTvChannelResult : KalturaBulkUploadAssetResult
    {
        /// <summary>
        /// External channel id as parsed from xmlTv
        /// </summary>
        [DataMember(Name = "channelExternalId")]
        [JsonProperty(PropertyName = "channelExternalId")]
        [XmlElement(ElementName = "channelExternalId")]
        [SchemeProperty(ReadOnly = true)]
        public string ChannelExternalId { get; set; }

        /// <summary>
        /// List of inner kaltura channeles that the externalId from xmlTv is mapped to
        /// </summary>
        [DataMember(Name = "innerChannels")]
        [JsonProperty(PropertyName = "innerChannels")]
        [XmlArray(ElementName = "innerChannels", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaBulkUploadChannelResult> InnerChannels { get; set; }

    }

    [Serializable]
    public partial class KalturaBulkUploadChannelResult : KalturaBulkUploadResult
    {
        /// <summary>
        /// The internal kaltura channel id
        /// </summary>
        [DataMember(Name = "channelId")]
        [JsonProperty(PropertyName = "channelId")]
        [XmlElement(ElementName = "channelId")]
        [SchemeProperty(ReadOnly = true)]
        public int ChannelId { get; set; }

        /// <summary>
        /// List of programs that were ingested to the channel
        /// </summary>
        [DataMember(Name = "programs")]
        [JsonProperty(PropertyName = "programs")]
        [XmlArray(ElementName = "programs", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaBulkUploadMultilingualProgramAssetResult> Programs { get; set; }

    }

    [Serializable]
    public partial class KalturaBulkUploadMultilingualProgramAssetResult : KalturaOTTObject
    {
        /// <summary>
        /// Language code of the program ingested
        /// </summary>
        [DataMember(Name = "languageCode")]
        [JsonProperty(PropertyName = "languageCode")]
        [XmlElement(ElementName = "languageCode")]
        [SchemeProperty(ReadOnly = true)]
        public string LanguageCode { get; set; }

        /// <summary>
        /// Ingested program information
        /// </summary>
        [DataMember(Name = "program")]
        [JsonProperty(PropertyName = "program")]
        [XmlElement(ElementName = "program")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaBulkUploadProgramAssetResult Program { get; set; }
    }

    [Serializable]
    public partial class KalturaBulkUploadProgramAssetResult : KalturaBulkUploadResult
    {
        /// <summary>
        /// The programID that was created
        /// </summary>
        [DataMember(Name = "programId")]
        [JsonProperty(PropertyName = "programId")]
        [XmlElement(ElementName = "programId")]
        [SchemeProperty(ReadOnly = true)]
        public int? ProgramId { get; set; }

        /// <summary>
        /// The external program Id as was sent in the xmlTv file
        /// </summary>
        [DataMember(Name = "programExternalId")]
        [JsonProperty(PropertyName = "programExternalId")]
        [XmlElement(ElementName = "programExternalId")]
        [SchemeProperty(ReadOnly = true)]
        public string ProgramExternalId { get; set; }
    }
}