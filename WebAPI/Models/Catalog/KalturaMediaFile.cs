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
    /// Asset file details
    /// </summary>
    [Serializable]
    public class KalturaAssetFile : KalturaOTTObject
    {
        /// <summary>
        /// URL of the media file to be played
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty(PropertyName = "url")]
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }
    }

    /// <summary>
    /// Media file details
    /// </summary>
    [Serializable]
    public class KalturaMediaFile : KalturaAssetFile
    {

        private const string OPC_MERGE_VERSION = "5.0.0.0";

        /// <summary>
        /// Unique identifier for the asset
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty(PropertyName = "assetId")]
        [XmlElement(ElementName = "assetId")]
        [OldStandardProperty("asset_id")]
        public int? AssetId { get; set; }

        /// <summary>
        /// File unique identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        /// <summary>
        /// Device types as defined in the system
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        [Deprecated(OPC_MERGE_VERSION)]
        public string Type { get; set; }


        /// <summary>
        /// Device types identifier as defined in the system
        /// </summary>
        [DataMember(Name = "typeId")]
        [JsonProperty(PropertyName = "typeId")]
        [XmlElement(ElementName = "typeId")]
        public int? TypeId { get; set; }

        /// <summary>
        /// URL of the media file to be played
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty(PropertyName = "url")]
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }

        /// <summary>
        /// Duration of the media file
        /// </summary>
        [DataMember(Name = "duration")]
        [JsonProperty(PropertyName = "duration")]
        [XmlElement(ElementName = "duration", IsNullable = true)]
        public long? Duration { get; set; }

        /// <summary>
        /// External identifier for the media file
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty(PropertyName = "externalId")]
        [XmlElement(ElementName = "externalId")]
        [OldStandardProperty("external_id")]
        public string ExternalId { get; set; }

        /// <summary>
        /// Alternative external identifier for the media file 
        /// </summary>
        [DataMember(Name = "altExternalId")]
        [JsonProperty(PropertyName = "altExternalId")]
        [XmlElement(ElementName = "altExternalId")]        
        public string AltExternalId { get; set; }

        /// <summary>
        /// Billing type
        /// </summary>
        [DataMember(Name = "billingType")]
        [JsonProperty(PropertyName = "billingType")]
        [XmlElement(ElementName = "billingType")]
        [JsonIgnore]
        [Deprecated(OPC_MERGE_VERSION)]
        public string BillingType { get; set; } 

        /// <summary>
        /// Quality
        /// </summary>
        [DataMember(Name = "quality")]
        [JsonProperty(PropertyName = "quality")]
        [XmlElement(ElementName = "quality")]        
        [JsonIgnore]
        [Deprecated(OPC_MERGE_VERSION)]
        public string Quality { get; set; }

        /// <summary>
        /// Handling type
        /// </summary>
        [DataMember(Name = "handlingType")]
        [JsonProperty(PropertyName = "handlingType")]
        [XmlElement(ElementName = "handlingType")]
        [JsonIgnore]
        public string HandlingType { get; set; }

        /// <summary>
        /// CDN name
        /// </summary>
        [DataMember(Name = "cdnName")]
        [JsonProperty(PropertyName = "cdnName")]
        [XmlElement(ElementName = "cdnName")]
        [JsonIgnore]
        public string CdnName { get; set; }

        /// <summary>
        /// CDN code
        /// </summary>
        [DataMember(Name = "cdnCode")]
        [JsonProperty(PropertyName = "cdnCode")]
        [XmlElement(ElementName = "cdnCode")]
        [JsonIgnore]
        public string CdnCode { get; set; }

        /// <summary>
        /// Alternative CDN code
        /// </summary>
        [DataMember(Name = "altCdnCode")]
        [JsonProperty(PropertyName = "altCdnCode")]
        [XmlElement(ElementName = "altCdnCode")]
        [JsonIgnore]
        public string AltCdnCode { get; set; }

        /// <summary>
        /// PPV Module 
        /// </summary>
        [DataMember(Name = "ppvModules")]
        [JsonProperty(PropertyName = "ppvModules")]
        [XmlElement(ElementName = "ppvModules")]
        [JsonIgnore]        
        public KalturaStringValueArray PPVModules { get; set; }

        /// <summary>
        /// Product code 
        /// </summary>
        [DataMember(Name = "productCode")]
        [JsonProperty(PropertyName = "productCode")]
        [XmlElement(ElementName = "productCode")]
        [JsonIgnore]
        [Deprecated(OPC_MERGE_VERSION)]
        public string ProductCode { get; set; }

        /// <summary>
        /// File size
        /// </summary>
        [DataMember(Name = "fileSize")]
        [JsonProperty(PropertyName = "fileSize")]
        [XmlElement(ElementName = "fileSize", IsNullable = true)]        
        public long? FileSize { get; set; }

        /// <summary>
        /// Additional Data
        /// </summary>
        [DataMember(Name = "additionalData")]
        [JsonProperty(PropertyName = "additionalData")]
        [XmlElement(ElementName = "additionalData")]
        public string AdditionalData { get; set; }

        /// <summary>
        /// Alternative streaming code
        /// </summary>
        [DataMember(Name = "altStreamingCode")]
        [JsonProperty(PropertyName = "altStreamingCode")]
        [XmlElement(ElementName = "altStreamingCode")]
        public string AltStreamingCode { get; set; }

        /// <summary>
        /// Alternative cdn adapter profile identifier
        /// </summary>
        [DataMember(Name = "alternativeCdnAdapaterProfileId")]
        [JsonProperty(PropertyName = "alternativeCdnAdapaterProfileId")]
        [XmlElement(ElementName = "alternativeCdnAdapaterProfileId", IsNullable = true)]
        public long? AlternativeCdnAdapaterProfileId { get; set; }

        /// <summary>
        /// EndDate
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty(PropertyName = "endDate")]
        [XmlElement(ElementName = "endDate")]
        public long? EndDate { get; set; }

        /// <summary>
        /// StartDate
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty(PropertyName = "startDate")]
        [XmlElement(ElementName = "startDate")]
        public long? StartDate { get; set; }

        /// <summary>
        /// ExternalStoreId
        /// </summary>
        [DataMember(Name = "externalStoreId")]
        [JsonProperty(PropertyName = "externalStoreId")]
        [XmlElement(ElementName = "externalStoreId")]
        public string ExternalStoreId { get; set; }

        /// <summary>
        /// IsDefaultLanguage
        /// </summary>
        [DataMember(Name = "isDefaultLanguage")]
        [JsonProperty(PropertyName = "isDefaultLanguage")]
        [XmlElement(ElementName = "isDefaultLanguage", IsNullable = true)]        
        public bool? IsDefaultLanguage { get; set; }

        /// <summary>
        /// Language
        /// </summary>
        [DataMember(Name = "language")]
        [JsonProperty(PropertyName = "language")]
        [XmlElement(ElementName = "language")]
        public string Language { get; set; }

        /// <summary>
        /// OrderNum
        /// </summary>
        [DataMember(Name = "orderNum")]
        [JsonProperty(PropertyName = "orderNum")]
        [XmlElement(ElementName = "orderNum", IsNullable = true)]
        public int? OrderNum { get; set; }

        /// <summary>
        /// OutputProtecationLevel
        /// </summary>
        [DataMember(Name = "outputProtecationLevel")]
        [JsonProperty(PropertyName = "outputProtecationLevel")]
        [XmlElement(ElementName = "outputProtecationLevel")]
        public string OutputProtecationLevel { get; set; }

        /// <summary>
        /// cdn adapter profile identifier
        /// </summary>
        [DataMember(Name = "cdnAdapaterProfileId")]
        [JsonProperty(PropertyName = "cdnAdapaterProfileId")]
        [XmlElement(ElementName = "cdnAdapaterProfileId", IsNullable = true)]
        [SchemeProperty(MinLong = 0)]
        public long? CdnAdapaterProfileId { get; set; }

        /// <summary>
        ///  The media file status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status", IsNullable = true)]
        public bool? Status { get; set; }

    }

    /// <summary>
    /// Media-file list
    /// </summary>
    [DataContract(Name = "Collections", Namespace = "")]
    [XmlRoot("Collections")]
    public class KalturaMediaFileListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of media-file types
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaMediaFile> Files { get; set; }
    }

}