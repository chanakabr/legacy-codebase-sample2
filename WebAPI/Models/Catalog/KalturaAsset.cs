using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset wrapper
    /// </summary>
    [Serializable]
    public class KalturaAssetListResponse : KalturaListResponse
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAsset> Objects { get; set; }
    }

    /// <summary>
    /// Asset info
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(KalturaProgramAsset))]
    [XmlInclude(typeof(KalturaMediaAsset))]
    abstract public class KalturaAsset : KalturaBaseAssetInfo, KalturaIAssetable
    {
        /// <summary>
        /// Dynamic collection of key-value pairs according to the String Meta defined in the system
        /// </summary>
        [DataMember(Name = "metas")]
        [JsonProperty(PropertyName = "metas")]
        [XmlElement("metas", IsNullable = true)]
        public SerializableDictionary<string, KalturaValue> Metas { get; set; }

        /// <summary>
        /// Dynamic collection of key-value pairs according to the Tag Types defined in the system
        /// </summary>
        [DataMember(Name = "tags")]
        [JsonProperty(PropertyName = "tags")]
        [XmlElement("tags", IsNullable = true)]
        public SerializableDictionary<string, KalturaMultilingualStringValueArray> Tags { get; set; }

        /// <summary>
        /// Date and time represented as epoch. For VOD – since when the asset is available in the catalog. For EPG/Linear – when the program is aired (can be in the future).
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty(PropertyName = "startDate")]
        [XmlElement(ElementName = "startDate")]
        public long? StartDate { get; set; }

        /// <summary>
        /// Date and time represented as epoch. For VOD – till when the asset be available in the catalog. For EPG/Linear – program end time and date
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty(PropertyName = "endDate")]
        [XmlElement(ElementName = "endDate")]
        public long? EndDate { get; set; }

        /// <summary>
        /// Enable cDVR
        /// </summary>
        [DataMember(Name = "enableCdvr")]
        [JsonProperty(PropertyName = "enableCdvr")]
        [XmlElement(ElementName = "enableCdvr")]
        public bool? EnableCdvr { get; set; }

        /// <summary>
        /// Enable catch-up
        /// </summary>
        [DataMember(Name = "enableCatchUp")]
        [JsonProperty(PropertyName = "enableCatchUp")]
        [XmlElement(ElementName = "enableCatchUp")]
        public bool? EnableCatchUp { get; set; }

        /// <summary>
        /// Enable start over
        /// </summary>
        [DataMember(Name = "enableStartOver")]
        [JsonProperty(PropertyName = "enableStartOver")]
        [XmlElement(ElementName = "enableStartOver")]
        public bool? EnableStartOver { get; set; }

        /// <summary>
        /// Enable trick-play
        /// </summary>
        [DataMember(Name = "enableTrickPlay")]
        [JsonProperty(PropertyName = "enableTrickPlay")]
        [XmlElement(ElementName = "enableTrickPlay")]
        public bool? EnableTrickPlay { get; set; }

        /// <summary>
        /// External identifier for the media file
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty(PropertyName = "externalId")]
        [XmlElement(ElementName = "externalId")]
        [JsonIgnore]        
        public string ExternalId { get; set; }
    }

    /// <summary>
    /// Program-asset info
    /// </summary>
    [Serializable]
    public class KalturaProgramAsset : KalturaAsset
    {
        /// <summary>
        /// EPG channel identifier
        /// </summary>
        [DataMember(Name = "epgChannelId")]
        [JsonProperty(PropertyName = "epgChannelId")]
        [XmlElement(ElementName = "epgChannelId")]
        public long? EpgChannelId { get; set; }

        /// <summary>
        /// EPG identifier
        /// </summary>
        [DataMember(Name = "epgId")]
        [JsonProperty(PropertyName = "epgId")]
        [XmlElement(ElementName = "epgId")]
        public string EpgId { get; set; }

        /// <summary>
        /// Ralated media identifier
        /// </summary>
        [DataMember(Name = "relatedMediaId")]
        [JsonProperty(PropertyName = "relatedMediaId")]
        [XmlElement(ElementName = "relatedMediaId")]
        public long? RelatedMediaId { get; set; }

        /// <summary>
        /// Unique identifier for the program
        /// </summary>
        [DataMember(Name = "crid")]
        [JsonProperty(PropertyName = "crid")]
        [XmlElement(ElementName = "crid")]
        public string Crid { get; set; }

        /// <summary>
        /// Id of linear media asset
        /// </summary>
        [DataMember(Name = "linearAssetId")]
        [JsonProperty(PropertyName = "linearAssetId")]
        [XmlElement(ElementName = "linearAssetId")]
        public long? LinearAssetId { get; set; }
    }

    /// <summary>
    /// Media-asset info
    /// </summary>
    [Serializable]
    public class KalturaMediaAsset : KalturaAsset
    {
        /// <summary>
        /// External identifiers
        /// </summary>
        [DataMember(Name = "externalIds")]
        [JsonProperty(PropertyName = "externalIds")]
        [XmlElement(ElementName = "externalIds")]
        public string ExternalIds { get; set; }

        /// <summary>
        /// Catch-up buffer
        /// </summary>
        [DataMember(Name = "catchUpBuffer")]
        [JsonProperty(PropertyName = "catchUpBuffer")]
        [XmlElement(ElementName = "catchUpBuffer")]
        public long? CatchUpBuffer { get; set; }

        /// <summary>
        /// Trick-play buffer
        /// </summary>
        [DataMember(Name = "trickPlayBuffer")]
        [JsonProperty(PropertyName = "trickPlayBuffer")]
        [XmlElement(ElementName = "trickPlayBuffer")]
        public long? TrickPlayBuffer { get; set; }

        /// <summary>
        /// Enable Recording playback for non entitled channel
        /// </summary>
        [DataMember(Name = "enableRecordingPlaybackNonEntitledChannel")]
        [JsonProperty(PropertyName = "enableRecordingPlaybackNonEntitledChannel")]
        [XmlElement(ElementName = "enableRecordingPlaybackNonEntitledChannel")]
        [SchemeProperty(ReadOnly = true)]
        public bool? EnableRecordingPlaybackNonEntitledChannel { get; set; }

        /// <summary>
        /// Asset type description 
        /// </summary>                
        [DataMember(Name = "typeDescription")]
        [JsonProperty(PropertyName = "typeDescription")]
        [XmlElement(ElementName = "typeDescription")]
        [JsonIgnore]        
        public string TypeDescription { get; set; }

        /// <summary>
        /// Entry Identifier
        /// </summary>
        [DataMember(Name = "entryId")]
        [JsonProperty(PropertyName = "entryId")]
        [XmlElement(ElementName = "entryId")]    
        public string EntryId { get; set; }

        /// <summary>
        /// Device rule
        /// </summary>
        [DataMember(Name = "deviceRule")]
        [JsonProperty(PropertyName = "deviceRule")]
        [XmlElement(ElementName = "deviceRule")]
        [JsonIgnore]        
        public string DeviceRule { get; set; }

        /// <summary>
        /// Geo block rule
        /// </summary>
        [DataMember(Name = "geoBlockRule")]
        [JsonProperty(PropertyName = "geoBlockRule")]
        [XmlElement(ElementName = "geoBlockRule")]
        [JsonIgnore]        
        public string GeoBlockRule { get; set; }

        /// <summary>
        /// Watch permission rule
        /// </summary>
        [DataMember(Name = "watchPermissionRule")]
        [JsonProperty(PropertyName = "watchPermissionRule")]
        [XmlElement(ElementName = "watchPermissionRule")]
        [JsonIgnore]
        public string WatchPermissionRule { get; set; }
    }

    /// <summary>
    /// Recording-asset info
    /// </summary>
    [Serializable]
    public class KalturaRecordingAsset : KalturaProgramAsset
    {
        /// <summary>
        /// Recording identifier
        /// </summary>
        [DataMember(Name = "recordingId")]
        [JsonProperty(PropertyName = "recordingId")]
        [XmlElement(ElementName = "recordingId")]
        public string RecordingId
        {
            get;
            set;
        }
    }
}