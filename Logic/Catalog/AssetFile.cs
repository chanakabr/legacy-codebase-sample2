using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Excel;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TVinciShared;

namespace Core.Catalog
{
    [Serializable]
    public class AssetFile : IExcelObject
    {
        #region Consts

        // MEDIA FILE EXCEL COLUMNS
        private const string CDN_LOCATION = "CDN Location";
        private const string EXTERNAL_ID = "externalId";
        private const string EXTERNAL_STORE_CODE = "External Store Code";
        private const string PLAYBACK_START_DATE = "Playback Start Date";
        private const string PLAYBACK_END_DATE = "Playback End Date";
        private const string CATALOG_END_DATE = "Catalog End Date";
        private const string DURATION = "Duration (Seconds)";
        private const string FILE_SIZE = "File Size (Kb)";
        private const string ALTERNATIVE_STREAMING_CODE = "Alternative Streaming Code";
        
        #endregion

        #region Data Members

        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("AssetId")]
        public long AssetId { get; set; }

        [JsonProperty("TypeId")]
        public int? TypeId { get; set; }

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

        [JsonProperty("CdnAdapaterProfileId")]
        public long? CdnAdapaterProfileId { get; set; }

        [ExcelColumn(ExcelColumnType.File, ALTERNATIVE_STREAMING_CODE)]
        [JsonProperty("AltStreamingCode")]
        public string AltStreamingCode { get; set; }

        [JsonProperty("AlternativeCdnAdapaterProfileId")]
        public long? AlternativeCdnAdapaterProfileId { get; set; }

        [JsonProperty("AdditionalData")]
        public string AdditionalData { get; set; }

        [JsonProperty("BillingType")]
        public long BillingType { get; set; }

        [JsonProperty("OrderNum")]
        public int? OrderNum { get; set; }

        [JsonProperty("Language")]
        public string Language { get; set; }

        [JsonProperty("IsDefaultLanguage")]
        public bool? IsDefaultLanguage { get; set; }

        [JsonProperty("OutputProtecationLevel")]
        public int OutputProtecationLevel { get; set; }

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

        #endregion

        #region Ctor's

        public AssetFile()
        {
            type = string.Empty;
        }

        public AssetFile(string typeName)
        {
            type = typeName;
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

            if (string.IsNullOrEmpty(this.type))
            {
                var mediaFileTypesListResponse = FileManager.GetMediaFileTypes(groupId);
                if (mediaFileTypesListResponse.HasObjects())
                {
                    var mediaFileType = mediaFileTypesListResponse.Objects.FirstOrDefault(x => x.Name.Equals(this.TypeId));
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

            // TODO SHIR - ASK IRA ABOUT VALUES IN AssetFile
            //if (!string.IsNullOrEmpty(this.cdnLocation))
            //{
            //    var cdnLocation = ExcelColumn.GetFullColumnName(this.type, null, CDN_LOCATION);
            //    excelValues.TryAdd(cdnLocation, this.cdnLocation);
            //}

            if (!string.IsNullOrEmpty(this.ExternalId))
            {
                var externalId = ExcelColumn.GetFullColumnName(this.type, EXTERNAL_ID);
                excelValues.TryAdd(externalId, this.ExternalId);
            }

            if (!string.IsNullOrEmpty(this.ExternalStoreId))
            {
                var externalStoreCode = ExcelColumn.GetFullColumnName(this.type, EXTERNAL_STORE_CODE);
                excelValues.TryAdd(externalStoreCode, this.ExternalStoreId);
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
            
            if (this.Duration.HasValue)
            {
                var duration = ExcelColumn.GetFullColumnName(this.type, DURATION);
                excelValues.TryAdd(duration, this.Duration);
            }
           
            if (this.FileSize.HasValue)
            {
                var fileSize = ExcelColumn.GetFullColumnName(this.type, FILE_SIZE);
                excelValues.TryAdd(fileSize, this.FileSize);
            }
            
            if (!string.IsNullOrEmpty(this.AltStreamingCode))
            {
                var altStreamingCode = ExcelColumn.GetFullColumnName(this.type, ALTERNATIVE_STREAMING_CODE);
                excelValues.TryAdd(altStreamingCode, this.AltStreamingCode);
            }
            
            return excelValues;
        }

        public void SetExcelValues(int groupId, Dictionary<string, object> columnNamesToValues, Dictionary<string, ExcelColumn> columns)
        {
            foreach (var columnValue in columnNamesToValues)
            {
                if (columns.ContainsKey(columnValue.Key))
                {
                    columns[columnValue.Key].Property.SetValue(this, columnValue.Value);
                }

                if (string.IsNullOrEmpty(this.type))
                {
                    this.type = columns[columnValue.Key].SystemName;
                }
            }

            //this.AssetId = assetId;
            if (!string.IsNullOrEmpty(this.type))
            {
                var mediaFileTypesListResponse = FileManager.GetMediaFileTypes(groupId);
                if (mediaFileTypesListResponse.HasObjects())
                {
                    var mediaFileType = mediaFileTypesListResponse.Objects.FirstOrDefault(x => x.Name.Equals(this.type));
                    if (mediaFileType != null)
                    {
                        this.TypeId = (int)mediaFileType.Id;
                    }
                }
            }
            // Url = mediaFile.CdnCode,
            //AltExternalId = mediaFile.AltCoGuid,
            this.IsActive = true;
        }

        #endregion
    }
}