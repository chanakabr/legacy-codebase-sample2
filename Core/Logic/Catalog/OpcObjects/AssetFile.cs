using ApiObjects.BulkUpload;
using ApiObjects.CDNAdapter;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.Catalog.CatalogManagement.Validators;
using TVinciShared;

namespace Core.Catalog
{
    [Serializable]
    public class AssetFile : IExcelObject
    {
        #region Consts
        
        // MEDIA FILE EXCEL COLUMNS
        private const string CDN = "CDN";
        private const string CDN_LOCATION = "CDN Location";
        private const string DURATION = "Duration (Seconds)";
        private const string EXTERNAL_ID = "External ID";
        private const string PLAYBACK_START_DATE = "Playback Start Date";
        private const string PLAYBACK_END_DATE = "Playback End Date";
        private const string CATALOG_END_DATE = "Catalog End Date";
        private const string PPVS = "PPVs";
        private const string EXTERNAL_STORE_CODE = "External Store Code";
        private const string ALTERNATIVE_CDN = "Alternative CDN";
        private const string ALTERNATIVE_CDN_LOCATION = "Alternative CDN Location";
        private const string FILE_SIZE = "File Size (Kb)";
        private const string LANGUAGE = "Language";
        private const string IS_DEFAULT_LANGUAGE = "Is Default Language";
        private const string LABELS = "Labels";
        public const string DYNAMIC_DATA = "DynamicData";
        
        #endregion

        #region Data Members

        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("AssetId")]
        public long AssetId { get; set; }

        [JsonProperty("TypeId")]
        public int? TypeId { get; set; }

        [ExcelColumn(ExcelColumnType.File, CDN_LOCATION)]
        [JsonProperty("Url")]
        public string Url { get; set; }

        [ExcelColumn(ExcelColumnType.File, DURATION)]
        [JsonProperty("Duration")]
        public long? Duration { get; set; }

        [ExcelColumn(ExcelColumnType.File, EXTERNAL_ID)]
        [JsonProperty("ExternalId")]
        public string ExternalId { get; set; }

        [JsonProperty("AltExternalId")]
        public string AltExternalId { get; set; }

        [ExcelColumn(ExcelColumnType.File, EXTERNAL_STORE_CODE)]
        [JsonProperty("ExternalStoreId")]
        public string ExternalStoreId { get; set; }

        [ExcelColumn(ExcelColumnType.File, CDN)]
        [JsonProperty("CdnAdapaterProfileId")]
        public long? CdnAdapaterProfileId { get; set; }

        [ExcelColumn(ExcelColumnType.File, ALTERNATIVE_CDN_LOCATION)]
        [JsonProperty("AltStreamingCode")]
        public string AltStreamingCode { get; set; }

        [ExcelColumn(ExcelColumnType.File, ALTERNATIVE_CDN)]
        [JsonProperty("AlternativeCdnAdapaterProfileId")]
        public long? AlternativeCdnAdapaterProfileId { get; set; }

        [JsonProperty("AdditionalData")]
        public string AdditionalData { get; set; }

        [JsonProperty("BillingType")]
        public long BillingType { get; set; }

        [JsonProperty("OrderNum")]
        public int? OrderNum { get; set; }

        [ExcelColumn(ExcelColumnType.File, LANGUAGE)]
        [JsonProperty("Language")]
        public string Language { get; set; }

        [ExcelColumn(ExcelColumnType.File, IS_DEFAULT_LANGUAGE)]
        [JsonProperty("IsDefaultLanguage")]
        public bool? IsDefaultLanguage { get; set; }

        [JsonProperty("OutputProtecationLevel")]
        public string OutputProtecationLevel { get; set; }

        [ExcelColumn(ExcelColumnType.File, PLAYBACK_START_DATE)]
        [JsonProperty("StartDate")]
        public DateTime? StartDate { get; set; }

        [ExcelColumn(ExcelColumnType.File, PLAYBACK_END_DATE)]
        [JsonProperty("EndDate")]
        public DateTime? EndDate { get; set; }

        [ExcelColumn(ExcelColumnType.File, FILE_SIZE)]
        [JsonProperty("FileSize")]
        public long? FileSize { get; set; }

        [JsonProperty("IsActive")]
        public bool? IsActive { get; set; }

        [ExcelColumn(ExcelColumnType.File, CATALOG_END_DATE)]
        [JsonProperty("CatalogEndDate")]
        public DateTime? CatalogEndDate { get; set; }

        [JsonProperty("type")]
        private string type;

        [ExcelColumn(ExcelColumnType.File, PPVS)]
        [JsonProperty("PpvModule")]
        public string PpvModule { get; set; }

        [JsonProperty("Opl")]
        public string Opl { get; set; }

        [ExcelColumn(ExcelColumnType.File, LABELS)]
        [JsonProperty("Labels")]
        public string Labels { get; set; }
        
        [ExcelColumn(ExcelColumnType.File, DYNAMIC_DATA)]
        [JsonProperty("DynamicData")]
        public IDictionary<string, IEnumerable<string>> DynamicData { get; set; }

        public DateTime? UpdateDate { get; set; }

        #endregion

        #region Ctor's

        public AssetFile()
        {
            type = string.Empty;
            Labels = string.Empty;
        }

        public AssetFile(string typeName)
        {
            type = typeName;
            Labels = string.Empty;
        }

        #endregion

        public string GetTypeName()
        {
            return type;
        }

        #region IExcelableObject Methods

        public Dictionary<string, object> GetExcelValues(int groupId)
        {
            Dictionary<string, object> excelValues = new Dictionary<string, object>();

            if (string.IsNullOrEmpty(this.type) && this.TypeId.HasValue)
            {
                var mediaFileTypesListResponse = FileManager.Instance.GetMediaFileTypes(groupId);
                if (mediaFileTypesListResponse.HasObjects())
                {
                    var mediaFileType = mediaFileTypesListResponse.Objects.FirstOrDefault(x => x.Id == this.TypeId.Value);
                    if (mediaFileType != null)
                    {
                        this.type = mediaFileType.Name;
                    }
                }
                if (string.IsNullOrEmpty(this.type))
                {
                    return excelValues;
                }
            }

            CDNAdapter adapter = null;
            if (CdnAdapaterProfileId.HasValue)
            {
                adapter = DAL.ApiDAL.GetCDNAdapter((int)CdnAdapaterProfileId.Value, false, groupId);
                if (adapter != null)
                {
                    var cdn = ExcelColumn.GetFullColumnName(this.type, CDN);
                    excelValues.TryAdd(cdn, adapter.SystemName);
                }
            }

            if (!string.IsNullOrEmpty(this.Url))
            {
                if(adapter != null && !string.IsNullOrEmpty(adapter.BaseUrl))
                {
                    this.Url = this.Url.Replace(adapter.BaseUrl, "");
                }
                
                var cdnLocation = ExcelColumn.GetFullColumnName(this.type, null, CDN_LOCATION);
                excelValues.TryAdd(cdnLocation, this.Url);
            }

            if (this.Duration.HasValue)
            {
                var duration = ExcelColumn.GetFullColumnName(this.type, DURATION);
                excelValues.TryAdd(duration, this.Duration);
            }

            if (!string.IsNullOrEmpty(this.ExternalId))
            {
                var externalId = ExcelColumn.GetFullColumnName(this.type, EXTERNAL_ID);
                excelValues.TryAdd(externalId, this.ExternalId);
            }

            if (this.StartDate.HasValue)
            {
                var playbackStartDate = ExcelColumn.GetFullColumnName(this.type, PLAYBACK_START_DATE);
                excelValues.TryAdd(playbackStartDate, this.StartDate);
            }

            if (this.EndDate.HasValue)
            {
                var playbackEndDate = ExcelColumn.GetFullColumnName(this.type, PLAYBACK_END_DATE);
                excelValues.TryAdd(playbackEndDate, this.EndDate);
            }

            if (this.CatalogEndDate.HasValue)
            {
                var catalogEndDate = ExcelColumn.GetFullColumnName(this.type, CATALOG_END_DATE);
                excelValues.TryAdd(catalogEndDate, this.CatalogEndDate);
            }

            if (!string.IsNullOrEmpty(this.PpvModule))
            {
                var ppvModule = ExcelColumn.GetFullColumnName(this.type, PPVS);
                excelValues.TryAdd(ppvModule, this.PpvModule);
            }

            if (!string.IsNullOrEmpty(this.ExternalStoreId))
            {
                var externalStoreCode = ExcelColumn.GetFullColumnName(this.type, EXTERNAL_STORE_CODE);
                excelValues.TryAdd(externalStoreCode, this.ExternalStoreId);
            }

            if (AlternativeCdnAdapaterProfileId.HasValue)
            {
                adapter = DAL.ApiDAL.GetCDNAdapter((int)AlternativeCdnAdapaterProfileId.Value, false, groupId);
                if (adapter != null)
                {
                    var cdn = ExcelColumn.GetFullColumnName(this.type, ALTERNATIVE_CDN);
                    excelValues.TryAdd(cdn, adapter.SystemName);
                }
            }

            if (!string.IsNullOrEmpty(this.AltStreamingCode))
            {
                if (adapter != null && !string.IsNullOrEmpty(adapter.BaseUrl))
                {
                    this.AltStreamingCode = this.AltStreamingCode.Replace(adapter.BaseUrl, "");
                }

                var cdnLocation = ExcelColumn.GetFullColumnName(this.type, null, ALTERNATIVE_CDN_LOCATION);
                excelValues.TryAdd(cdnLocation, this.AltStreamingCode);
            }

            if (this.FileSize.HasValue)
            {
                var fileSize = ExcelColumn.GetFullColumnName(this.type, FILE_SIZE);
                excelValues.TryAdd(fileSize, this.FileSize);
            }

            if (!string.IsNullOrEmpty(this.Language))
            {
                var language = ExcelColumn.GetFullColumnName(this.type, null, LANGUAGE);
                excelValues.TryAdd(language, this.Language);
            }

            if (this.IsDefaultLanguage.HasValue)
            {
                var isDefaultLanguage = ExcelColumn.GetFullColumnName(this.type, IS_DEFAULT_LANGUAGE);
                excelValues.TryAdd(isDefaultLanguage, this.IsDefaultLanguage);
            }

            if (!string.IsNullOrEmpty(Labels))
            {
                var labels = ExcelColumn.GetFullColumnName(type, innerSystemName: LABELS);
                excelValues.TryAdd(labels, Labels);
            }

            if (DynamicData != null)
            {
                foreach (var item in DynamicData)
                {
                    var dynamicDataKey = ExcelColumn.GetFullColumnName(type, innerSystemName: item.Key);
                    var value = string.Join(",", item.Value);
                    excelValues.TryAdd(dynamicDataKey, value);
                }
            }

            return excelValues;
        }

        public void SetExcelValues(int groupId, Dictionary<string, object> columnNamesToValues, Dictionary<string, ExcelColumn> columns, IExcelStructureManager structureObject)
        {
            var excelColumnValues = columnNamesToValues
                .Select(x => new ExcelColumnValue(x.Key, x.Value))
                .ToArray();
            foreach (var columnValue in excelColumnValues.Where(x => x.IsStatic))
            {
                try
                {
                    if (string.IsNullOrEmpty(type))
                    {
                        type = columns[columnValue.Key].SystemName;
                    }

                    var realType = columns[columnValue.Key].Property.PropertyType.GetRealType();

                    object convertedValue;
                    if (realType == DateUtils.DateTimeType || realType == DateUtils.NullableDateTimeType)
                    {
                        convertedValue = DateUtils.ExtractDate(columnValue.Value.ToString(), DateUtils.MAIN_FORMAT);
                    }
                    else if (columns[columnValue.Key].InnerSystemName == CDN
                             || columns[columnValue.Key].InnerSystemName == ALTERNATIVE_CDN)
                    {
                        var cdnAdapter = DAL.ApiDAL.GetCDNAdapterByAlias(groupId, columnValue.Value.ToString());
                        convertedValue = cdnAdapter == null
                            ? (object)null
                            : (long)cdnAdapter.ID;
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(columnValue.Value, realType);
                    }

                    if (convertedValue != null)
                    {
                        columns[columnValue.Key].Property.SetValue(this, convertedValue);
                    }
                }
                catch (Exception ex)
                {
                    var excelParserException = new ExcelParserException(ex, columnValue.Key, columnValue.Value);
                    throw excelParserException;
                }
            }

            if (!string.IsNullOrEmpty(type))
            {
                var mediaFileTypesListResponse = FileManager.Instance.GetMediaFileTypes(groupId);
                if (mediaFileTypesListResponse.HasObjects())
                {
                    var mediaFileType = mediaFileTypesListResponse.Objects.FirstOrDefault(x => x.Name.Equals(type));
                    if (mediaFileType != null)
                    {
                        if (!TypeId.HasValue)
                        {
                            TypeId = (int)mediaFileType.Id;
                        }

                        var allDynamicData = excelColumnValues
                            .Where(x => !x.IsStatic && x.Value != null)
                            .ToDictionary(x => x.PropertyName, x => x.Value.ToString()?.Split(',').AsEnumerable());
                        DynamicData = MediaFileValidator.Instance.GetValidatedDynamicData(mediaFileType, allDynamicData);
                    }
                }
            }

            IsActive = true;
        }

        private class ExcelColumnValue
        {
            public string Key { get; }
            public object Value { get; }
            public string PropertyName { get; }
            public bool IsStatic { get; }

            private readonly IReadOnlyCollection<string> _staticPropertyNames = new[]
            {
                CDN, CDN_LOCATION, DURATION,
                EXTERNAL_ID, PLAYBACK_START_DATE, PLAYBACK_END_DATE,
                CATALOG_END_DATE, PPVS, EXTERNAL_STORE_CODE,
                ALTERNATIVE_CDN, ALTERNATIVE_CDN_LOCATION, FILE_SIZE,
                LANGUAGE, IS_DEFAULT_LANGUAGE, LABELS
            };

            public ExcelColumnValue(string key, object value)
            {
                Key = key;
                Value = value;
                PropertyName = key.Split(':').Last();
                IsStatic = _staticPropertyNames.Contains(PropertyName);
            }
        }

        #endregion
    }
}