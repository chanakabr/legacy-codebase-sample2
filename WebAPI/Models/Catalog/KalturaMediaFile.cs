using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.App_Start;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{

    /// <summary>
    /// Asset file details
    /// </summary>
    [Serializable]
    public partial class KalturaAssetFile : KalturaOTTObject
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
    public partial class KalturaMediaFile : KalturaAssetFile
    {
        private const string OPC_MERGE_VERSION = "5.0.0.0";
        internal const string MEDIA_FILE_INDEX = "mediaFileIndex";

        // MEDIA FILE EXCEL COLUMNS
        internal const string FILE_TYPE = "Type";
        internal const string CDN_NAME = "CDN Name";
        internal const string CDN_CODE = "CDN Code";
        internal const string DURATION = "Duration";
        internal const string FILE_EXTERNAL_ID = "External Id";
        internal const string LANGUAGE = "Language";
        internal const string IS_DEFAULT_LANGUAGE = "Is Default Language";
        internal const string FILE_START_DATE = "Start Date";
        internal const string FILE_END_DATE = "End Date";
        internal const string EXTERNAL_STORE_ID = "External Store Id";
        internal const string ALT_CDN_CODE = "Alt CDN Code";
        internal const string ALTERNATIVE_CDN_ADAPTER_PROFILE_ID = "Alternative CdnAdapater Profile Id";

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
        /// Deprecated - Device types as defined in the system        
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        [SchemeProperty(ReadOnly = true)]
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

        /// <summary>
        /// Catalog end date
        /// </summary>
        [DataMember(Name = "catalogEndDate")]
        [JsonProperty(PropertyName = "CatalogEndDate")]
        [XmlElement(ElementName = "CatalogEndDate")]
        public long? CatalogEndDate { get; set; }

        internal override Dictionary<string, object> GetExcelValues(int groupId, Dictionary<string, object> data = null)
        {
            Dictionary<string, object> excelValues = new Dictionary<string, object>();

            var baseExcelValues = base.GetExcelValues(groupId, data);
            excelValues.TryAddRange(baseExcelValues);

            if (data != null && data.ContainsKey(MEDIA_FILE_INDEX))
            {
                int? mediaFileIndex = data[MEDIA_FILE_INDEX] as int?;

                if (mediaFileIndex.HasValue)
                {
                    var fileType = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, FILE_TYPE, null, mediaFileIndex.Value);
                    // TODO SHIR - ask ira if this is the true TYPE PROPERTY
                    excelValues.TryAdd(fileType, this.TypeId);
                    
                    var cdnName = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, CDN_NAME, null, mediaFileIndex.Value);
                    excelValues.TryAdd(cdnName, this.CdnName);
                    
                    var cdnCode = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, CDN_CODE, null, mediaFileIndex.Value);
                    excelValues.TryAdd(cdnCode, this.CdnCode);
                    
                    var duration = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, DURATION, null, mediaFileIndex.Value);
                    excelValues.TryAdd(duration, this.Duration);
                    
                    var fileExternalId = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, FILE_EXTERNAL_ID, null, mediaFileIndex.Value);
                    excelValues.TryAdd(fileExternalId, this.ExternalId);
                    
                    var language = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, LANGUAGE, null, mediaFileIndex.Value);
                    excelValues.TryAdd(language, this.Language);
                    
                    var isDefaultLanguage = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, IS_DEFAULT_LANGUAGE, null, mediaFileIndex.Value);
                    excelValues.TryAdd(isDefaultLanguage, this.IsDefaultLanguage);
                    
                    var startDate = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, FILE_START_DATE, null, mediaFileIndex.Value);
                    excelValues.TryAdd(startDate, this.StartDate);
                    
                    var endDate = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, FILE_END_DATE, null, mediaFileIndex.Value);
                    excelValues.TryAdd(endDate, this.EndDate);
                    
                    var externalStoreId = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, EXTERNAL_STORE_ID, null, mediaFileIndex.Value);
                    excelValues.TryAdd(externalStoreId, this.ExternalStoreId);
                    
                    var altCdnCode = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, ALT_CDN_CODE, null, mediaFileIndex.Value);
                    excelValues.TryAdd(altCdnCode, this.AltCdnCode);
                    
                    var alternativeCdnAdapaterProfileId = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, ALTERNATIVE_CDN_ADAPTER_PROFILE_ID, null, mediaFileIndex.Value);
                    excelValues.TryAdd(alternativeCdnAdapaterProfileId, this.AlternativeCdnAdapaterProfileId);
                }
            }

            return excelValues;
        }
    }

    /// <summary>
    /// Media-file list
    /// </summary>
    [DataContract(Name = "Collections", Namespace = "")]
    [XmlRoot("Collections")]
    public partial class KalturaMediaFileListResponse : KalturaListResponse
    {
        internal const string MAX_MEDIA_FILES = "maxMediaFiles";
        
        /// <summary>
        /// A list of media-file types
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaMediaFile> Files { get; set; }

        internal override Dictionary<string, KalturaExcelColumn> GetExcelColumns(int groupId, Dictionary<string, object> data = null)
        {
            Dictionary<string, KalturaExcelColumn> excelColumns = new Dictionary<string, KalturaExcelColumn>();

            var baseExcelColumns = base.GetExcelColumns(groupId, data);
            excelColumns.TryAddRange(baseExcelColumns);

            if (data != null && data.ContainsKey(MAX_MEDIA_FILES))
            {
                int? maxMediaFiles = data[MAX_MEDIA_FILES] as int?;

                if (maxMediaFiles.HasValue)
                {
                    if (maxMediaFiles.Value == 0)
                    {
                        maxMediaFiles = 1;
                    }

                    for (int i = 0; i < maxMediaFiles.Value; i++)
                    {
                        // FILE_TYPE
                        var fileType = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, KalturaMediaFile.FILE_TYPE, null, i);
                        excelColumns.TryAdd(fileType, new KalturaExcelColumn(ExcelColumnType.File, fileType, KalturaMediaFile.FILE_TYPE));

                        // CDN_NAME
                        var cdnName = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, KalturaMediaFile.CDN_NAME, null, i);
                        excelColumns.TryAdd(cdnName, new KalturaExcelColumn(ExcelColumnType.File, cdnName, KalturaMediaFile.CDN_NAME));

                        // CDN_CODE
                        var cdnCode = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, KalturaMediaFile.CDN_CODE, null, i);
                        excelColumns.TryAdd(cdnCode, new KalturaExcelColumn(ExcelColumnType.File, cdnCode, KalturaMediaFile.CDN_CODE));

                        // DURATION
                        var duration = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, KalturaMediaFile.DURATION, null, i);
                        excelColumns.TryAdd(duration, new KalturaExcelColumn(ExcelColumnType.File, duration, KalturaMediaFile.DURATION));

                        // FILE_EXTERNAL_ID
                        var fileExternalId = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, KalturaMediaFile.FILE_EXTERNAL_ID, null, i);
                        excelColumns.TryAdd(fileExternalId, new KalturaExcelColumn(ExcelColumnType.File, fileExternalId, KalturaMediaFile.FILE_EXTERNAL_ID));

                        // LANGUAGE
                        var language = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, KalturaMediaFile.LANGUAGE, null, i);
                        excelColumns.TryAdd(language, new KalturaExcelColumn(ExcelColumnType.File, language, KalturaMediaFile.LANGUAGE));

                        // IS_DEFAULT_LANGUAGE
                        var isDefaultLanguage = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, KalturaMediaFile.IS_DEFAULT_LANGUAGE, null, i);
                        excelColumns.TryAdd(isDefaultLanguage, new KalturaExcelColumn(ExcelColumnType.File, isDefaultLanguage, KalturaMediaFile.IS_DEFAULT_LANGUAGE));

                        // FILE_START_DATE
                        var startDate = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, KalturaMediaFile.FILE_START_DATE, null, i);
                        excelColumns.TryAdd(startDate, new KalturaExcelColumn(ExcelColumnType.File, startDate, KalturaMediaFile.FILE_START_DATE));

                        // FILE_END_DATE
                        var endDate = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, KalturaMediaFile.FILE_END_DATE, null, i);
                        excelColumns.TryAdd(endDate, new KalturaExcelColumn(ExcelColumnType.File, endDate, KalturaMediaFile.FILE_END_DATE));

                        // EXTERNAL_STORE_ID
                        var externalStoreId = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, KalturaMediaFile.EXTERNAL_STORE_ID, null, i);
                        excelColumns.TryAdd(externalStoreId, new KalturaExcelColumn(ExcelColumnType.File, externalStoreId, KalturaMediaFile.EXTERNAL_STORE_ID));

                        // ALT_CDN_CODE
                        var altCdnCode = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, KalturaMediaFile.ALT_CDN_CODE, null, i);
                        excelColumns.TryAdd(altCdnCode, new KalturaExcelColumn(ExcelColumnType.File, altCdnCode, KalturaMediaFile.ALT_CDN_CODE));

                        // ALTERNATIVE_CDN_ADAPTER_PROFILE_ID
                        var alternativeCdnAdapaterProfileId = ExcelFormatter.GetHiddenColumn(ExcelColumnType.File, KalturaMediaFile.ALTERNATIVE_CDN_ADAPTER_PROFILE_ID, null, i);
                        excelColumns.TryAdd(alternativeCdnAdapaterProfileId,
                                            new KalturaExcelColumn(ExcelColumnType.File, alternativeCdnAdapaterProfileId, KalturaMediaFile.ALTERNATIVE_CDN_ADAPTER_PROFILE_ID));
                    }
                }
            }

            return excelColumns;
        }
    }

}