using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TVinciShared;

namespace Core.Catalog
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class MediaAsset : Asset
    {
        #region Consts

        public override string DistributedTask { get { return "distributed_tasks.process_bulk_upload_media_asset"; } }
        public override string RoutingKey { get { return "PROCESS_BULK_UPLOAD_MEDIA_ASSET\\{0}"; } }

        // ASSET EXCEL COLUMNS
        public const string MEDIA_ASSET_TYPE = "Media Asset Type";
        public const string GEO_RULE_ID = "GeoBlockRuleId";
        public const string DEVICE_RULE_ID = "DeviceRuleId";
        public const string FILES = "FILES";

        #endregion

        #region Data Members

        [ExcelColumn(ExcelColumnType.AvailabilityMeta, AssetManager.CATALOG_START_DATE_TIME_META_SYSTEM_NAME, IsMandatory = true, IsUniqueMeta = true)]
        [JsonProperty("CatalogStartDate")]
        public DateTime? CatalogStartDate { get; set; }

        [ExcelColumn(ExcelColumnType.AvailabilityMeta, AssetManager.CATALOG_END_DATE_TIME_META_SYSTEM_NAME, IsMandatory = true, IsUniqueMeta = true)]
        [JsonProperty("FinalEndDate")]
        public DateTime? FinalEndDate { get; set; }

        [ExcelColumn(ExcelColumnType.Basic, MEDIA_ASSET_TYPE, IsMandatory = true)]
        [JsonProperty("MediaType")]
        public MediaType MediaType { get; set; }

        [JsonProperty("EntryId")]
        public string EntryId { get; set; }

        [ExcelColumn(ExcelColumnType.Rule, DEVICE_RULE_ID)]
        [JsonProperty("DeviceRuleId")]
        public int? DeviceRuleId { get; set; }

        [ExcelColumn(ExcelColumnType.Rule, GEO_RULE_ID)]
        [JsonProperty("GeoBlockRuleId")]
        public int? GeoBlockRuleId { get; set; }

        [ExcelColumn(ExcelColumnType.File, FILES)]
        [JsonProperty(PropertyName = "Files",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<AssetFile> Files { get; set; }

        [JsonProperty("UserTypes")]
        public string UserTypes { get; set; }

        [ExcelColumn(ExcelColumnType.AvailabilityMeta, AssetManager.STATUS_META_SYSTEM_NAME, IsMandatory = true, IsUniqueMeta = true)]
        [JsonProperty("IsActive")]
        public bool? IsActive { get; set; }

        [JsonProperty("MediaAssetType")]
        public MediaAssetType MediaAssetType { get; set; }

        [JsonProperty("InheritancePolicy")]
        public AssetInheritancePolicy? InheritancePolicy { get; set; }
        
        #endregion

        #region Ctor's

        public MediaAsset()
            : base()
        {
            this.CatalogStartDate = null;
            this.FinalEndDate = null;
            this.MediaType = new MediaType();
            this.EntryId = string.Empty;
            this.DeviceRuleId = null;
            this.GeoBlockRuleId = null;
            this.Files = new List<AssetFile>();
            this.UserTypes = string.Empty;
            this.IsActive = null;
            this.MediaAssetType = MediaAssetType.Media;
        }

        public MediaAsset(long id, eAssetTypes assetType, string name, List<LanguageContainer> namesWithLanguages, string description, List<LanguageContainer> descriptionsWithLanguages,
                        DateTime? createDate, DateTime? updateDate, DateTime? startDate, DateTime? endDate, List<Metas> metas, List<Tags> tags, List<Image> images, string coGuid, bool isActive,
                        DateTime? catalogStartDate, DateTime? finalEndDate, MediaType mediaType, string entryId, int? deviceRuleId, int? geoBlockRuleId, List<AssetFile> files, string userTypes, AssetInheritancePolicy assetInheritancePolicy)
            : base(id, assetType, name, namesWithLanguages, description, descriptionsWithLanguages, createDate, startDate, updateDate, endDate, metas, tags, images, coGuid)
        {
            this.CatalogStartDate = catalogStartDate;
            if (this.CatalogStartDate.HasValue)
            {
                this.Metas.Add(new Metas(new TagMeta(AssetManager.CATALOG_START_DATE_TIME_META_SYSTEM_NAME, MetaType.DateTime.ToString()), this.CatalogStartDate.Value.ToString()));
            }

            this.FinalEndDate = finalEndDate;
            if (this.FinalEndDate.HasValue)
            {
                this.Metas.Add(new Metas(new TagMeta(AssetManager.PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME, MetaType.DateTime.ToString()), this.FinalEndDate.Value.ToString()));
            }

            this.MediaType = new MediaType(mediaType.m_sTypeName, mediaType.m_nTypeID);
            this.EntryId = entryId;
            this.DeviceRuleId = deviceRuleId;
            this.GeoBlockRuleId = geoBlockRuleId;
            this.Files = files != null ? new List<AssetFile>(files) : new List<AssetFile>();
            this.UserTypes = userTypes;
            this.IsActive = isActive;
            this.MediaAssetType = MediaAssetType.Media;
            this.InheritancePolicy = assetInheritancePolicy;
        }

        public MediaAsset(MediaAsset mediaAssetToCopy)
            : base(mediaAssetToCopy)
        {
            this.CatalogStartDate = mediaAssetToCopy.CatalogStartDate;
            this.FinalEndDate = mediaAssetToCopy.FinalEndDate;
            this.MediaType = new MediaType(mediaAssetToCopy.MediaType.m_sTypeName, mediaAssetToCopy.MediaType.m_nTypeID);
            this.EntryId = mediaAssetToCopy.EntryId;
            this.DeviceRuleId = mediaAssetToCopy.DeviceRuleId;
            this.GeoBlockRuleId = mediaAssetToCopy.GeoBlockRuleId;
            this.Files = mediaAssetToCopy.Files != null ? new List<AssetFile>(mediaAssetToCopy.Files) : new List<AssetFile>();
            this.UserTypes = mediaAssetToCopy.UserTypes;
            this.IsActive = mediaAssetToCopy.IsActive;
            this.MediaAssetType = mediaAssetToCopy.MediaAssetType;
            this.InheritancePolicy = mediaAssetToCopy.InheritancePolicy;
        }

        public MediaAsset(int groupId, Response.MediaObj mediaObj)
            : base(mediaObj)
        {
            this.CatalogStartDate = mediaObj.m_dCatalogStartDate;
            this.FinalEndDate = mediaObj.m_dFinalDate;
            this.MediaType = new MediaType(mediaObj.m_oMediaType.m_sTypeName, mediaObj.m_oMediaType.m_nTypeID);
            this.EntryId = mediaObj.EntryId;
            if (!string.IsNullOrEmpty(mediaObj.DeviceRule))
            {
                this.DeviceRuleId = (int?)TvmRuleManager.GetDeviceRuleId(groupId, mediaObj.DeviceRule);
            }

            if (!string.IsNullOrEmpty(mediaObj.GeoblockRule))
            {
                this.GeoBlockRuleId = (int?)TvmRuleManager.GetGeoBlockRuleId(groupId, mediaObj.GeoblockRule);
            }

            this.Files = new List<AssetFile>();
            this.UserTypes = string.Empty;
            this.IsActive = mediaObj.IsActive;
            this.MediaAssetType = string.IsNullOrEmpty(mediaObj.m_ExternalIDs) ? MediaAssetType.Media : MediaAssetType.Linear;
        }

        #endregion

        #region IExcel Methods
        
        public override Dictionary<string, object> GetExcelValues(int groupId)
        {
            Dictionary<string, object> excelValues = new Dictionary<string, object>();
            var baseExcelValues = base.GetExcelValues(groupId);
            excelValues.TryAddRange(baseExcelValues);

            if (this.MediaType != null)
            {
                if (string.IsNullOrEmpty(this.MediaType.m_sTypeName) && this.MediaType.m_nTypeID > 0)
                {
                    CatalogGroupCache catalogGroupCache;
                    if (CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        if (catalogGroupCache.AssetStructsMapById.ContainsKey(this.MediaType.m_nTypeID))
                        {
                            this.MediaType.m_sTypeName = catalogGroupCache.AssetStructsMapById[this.MediaType.m_nTypeID].SystemName;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(this.MediaType.m_sTypeName))
                {
                    var excelColumn = ExcelColumn.GetFullColumnName(MEDIA_ASSET_TYPE, null, null, true);
                    excelValues.TryAdd(excelColumn, this.MediaType.m_sTypeName);
                }
            }

            DateTime? catalogStartDate = GetBasicMetaDate(this.CatalogStartDate, AssetManager.CATALOG_START_DATE_TIME_META_SYSTEM_NAME);
            if (catalogStartDate.HasValue)
            {
                var excelColumn = ExcelColumn.GetFullColumnName(AssetManager.CATALOG_START_DATE_TIME_META_SYSTEM_NAME, null, null, true);
                excelValues.TryAdd(excelColumn, catalogStartDate);
            }

            DateTime? finalEndDate = GetBasicMetaDate(this.FinalEndDate, AssetManager.CATALOG_END_DATE_TIME_META_SYSTEM_NAME);
            if (finalEndDate.HasValue)
            {
                var excelColumn = ExcelColumn.GetFullColumnName(AssetManager.CATALOG_END_DATE_TIME_META_SYSTEM_NAME, null, null, true);
                excelValues.TryAdd(excelColumn, finalEndDate);
            }

            foreach (var file in Files)
            {
                excelValues.TryAddRange(file.GetExcelValues(groupId));
            }

            if (this.DeviceRuleId.HasValue)
            {
                var excelColumn = ExcelColumn.GetFullColumnName(DEVICE_RULE_ID);
                excelValues.TryAdd(excelColumn, this.DeviceRuleId);
            }

            if (this.GeoBlockRuleId.HasValue)
            {
                var excelColumn = ExcelColumn.GetFullColumnName(GEO_RULE_ID);
                excelValues.TryAdd(excelColumn, this.GeoBlockRuleId);
            }

            if (this.IsActive.HasValue)
            {
                var excelColumn = ExcelColumn.GetFullColumnName(AssetManager.STATUS_META_SYSTEM_NAME, null, null, true);
                excelValues.TryAdd(excelColumn, this.IsActive);
            }

            return excelValues;
        }

        private DateTime? GetBasicMetaDate(DateTime? date, string metaDateSystemName)
        {
            DateTime? basicMetaDate = null;
            if (date.HasValue)
            {
                basicMetaDate = date;
            }
            else if (this.Metas != null && this.Metas.Count > 0)
            {
                var metaDate = Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.Equals(metaDateSystemName));
                if (metaDate != null)
                {
                    DateTime assetDate;
                    if (DateTime.TryParse(metaDate.m_sValue, out assetDate))
                    {
                        basicMetaDate = assetDate;
                    }
                }
            }

            return basicMetaDate;
        }
        
        public override void SetExcelValues(int groupId, Dictionary<string, object> columnNamesToValues, Dictionary<string, ExcelColumn> columns)
        {
            this.AssetType = eAssetTypes.MEDIA;

            CatalogGroupCache catalogGroupCache;
            if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                return;
            }
            
            HashSet<string> fileTypesSystemName = new HashSet<string>();
            Dictionary<string, List<LanguageContainer>> dicMetas = new Dictionary<string, List<LanguageContainer>>();
            Dictionary<string, ImageType> imageTypesMapBySystemName = ImageManager.GetImageTypesMapBySystemName(groupId);

            foreach (var columnValue in columnNamesToValues)
            {
                if (columns.ContainsKey(columnValue.Key))
                {
                    switch (columns[columnValue.Key].ColumnType)
                    {
                        case ExcelColumnType.Meta:
                            SetMetaByExcelValues(columnValue, columns[columnValue.Key], catalogGroupCache.DefaultLanguage.Code, ref dicMetas);
                            break;
                        case ExcelColumnType.Tag:
                            SetTagByExcelValues(columnValue, columns[columnValue.Key].SystemName, catalogGroupCache);
                            break;
                        case ExcelColumnType.Image:
                            SetImageByExcelValues(columnValue, columns[columnValue.Key], imageTypesMapBySystemName);
                            break;
                        case ExcelColumnType.Basic:
                            SetBasicByExcelValues(columnValue, catalogGroupCache);
                            break;
                        case ExcelColumnType.File:
                            var fileSystemName = columns[columnValue.Key].SystemName;
                            if (!fileTypesSystemName.Contains(fileSystemName))
                            {
                                fileTypesSystemName.Add(fileSystemName);
                                var fileValues = columnNamesToValues.Where(x => x.Key.StartsWith(fileSystemName)).ToDictionary(x => x.Key, x => x.Value);
                                var fileColumns = columns.Where(x => x.Key.StartsWith(fileSystemName)).ToDictionary(x => x.Key, x => x.Value);
                                SetFileByExcelValues(groupId, fileValues, fileColumns);
                            }
                            break;
                        case ExcelColumnType.Rule:
                            SetRuleByExcelValues(columnValue);
                            break;
                        case ExcelColumnType.AvailabilityMeta:
                            SetAvailabilityMetaByExcelValues(columnValue);
                            break;
                        default:
                            break;
                    }
                }
            }

            foreach (var item in dicMetas)
            {
                Metas metas = new Metas();
                if (catalogGroupCache.TopicsMapBySystemName.ContainsKey(item.Key))
                {
                    metas.m_oTagMeta = new TagMeta()
                    {
                        m_sName = item.Key,
                        m_sType = catalogGroupCache.TopicsMapBySystemName[item.Key].Type.ToString()
                    };
                }

                var defaultValue = item.Value.FirstOrDefault(x => x.IsDefault);
                if (defaultValue != null)
                {
                    metas.m_sValue = defaultValue.Value;
                }

                metas.Value = item.Value.ToArray();
                this.Metas.Add(metas);
            }
        }

        private void SetAvailabilityMetaByExcelValues(KeyValuePair<string, object> columnValue)
        {
            // StartDate
            var startDateColumnName = ExcelColumn.GetFullColumnName(AssetManager.PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME, null, null, true);
            if (columnValue.Key.Equals(startDateColumnName))
            {
                this.StartDate = DateUtils.ExtractDate(columnValue.Value.ToString(), ExcelManager.DATE_FORMAT);
                return;
            }

            // EndDate
            var endDateColumnName = ExcelColumn.GetFullColumnName(AssetManager.PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME, null, null, true);
            if (columnValue.Key.Equals(endDateColumnName))
            {
                this.EndDate = DateUtils.ExtractDate(columnValue.Value.ToString(), ExcelManager.DATE_FORMAT);
                return;
            }

            // CatalogStartDate
            var catalogStartDateColumnName = ExcelColumn.GetFullColumnName(AssetManager.CATALOG_START_DATE_TIME_META_SYSTEM_NAME, null, null, true);
            if (columnValue.Key.Equals(catalogStartDateColumnName))
            {
                this.CatalogStartDate = DateUtils.ExtractDate(columnValue.Value.ToString(), ExcelManager.DATE_FORMAT);
                return;
            }

            // FinalEndDate
            var finalEndDateColumnName = ExcelColumn.GetFullColumnName(AssetManager.CATALOG_END_DATE_TIME_META_SYSTEM_NAME, null, null, true);
            if (columnValue.Key.Equals(finalEndDateColumnName))
            {
                this.FinalEndDate = DateUtils.ExtractDate(columnValue.Value.ToString(), ExcelManager.DATE_FORMAT);
                return;
            }
        }

        private void SetFileByExcelValues(int groupId, Dictionary<string, object> fileValues, Dictionary<string, ExcelColumn> fileColumns)
        {
            if (fileValues != null && fileValues.Count > 0 && fileColumns != null && fileColumns.Count > 0)
            {
                var file = new AssetFile();
                file.SetExcelValues(groupId, fileValues, fileColumns);
                Files.Add(file);
            }
        }
        
        private void SetRuleByExcelValues(KeyValuePair<string, object> columnValue)
        {
            if (columnValue.Key.Equals(GEO_RULE_ID))
            {
                this.GeoBlockRuleId = StringUtils.TryConvertTo<int>(columnValue.Value.ToString());
            }
            else if (columnValue.Key.Equals(DEVICE_RULE_ID))
            {
                this.DeviceRuleId = StringUtils.TryConvertTo<int>(columnValue.Value.ToString());
            }
        }
        
        private void SetBasicByExcelValues(KeyValuePair<string, object> columnValue, CatalogGroupCache catalogGroupCache)
        {
            // MediaType
            var mediaTypeColumnName = ExcelColumn.GetFullColumnName(MEDIA_ASSET_TYPE, null, null, true);
            if (columnValue.Key.Equals(mediaTypeColumnName))
            {
                this.MediaType.m_sTypeName = columnValue.Value.ToString();
                if (catalogGroupCache.AssetStructsMapBySystemName.ContainsKey(this.MediaType.m_sTypeName))
                {
                    this.MediaType.m_nTypeID = (int)catalogGroupCache.AssetStructsMapBySystemName[this.MediaType.m_sTypeName].Id;
                }
                return;
            }

            // EXTERNAL_ID
            var externalIdColumnName = ExcelColumn.GetFullColumnName(EXTERNAL_ASSET_ID, null, null, true);
            if (columnValue.Key.Equals(externalIdColumnName))
            {
                this.CoGuid = columnValue.Value.ToString();
                return;
            }
        }

        #endregion

        #region IBulkUploadObject Methods

        public override BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, BulkUploadResultStatus status, int index, Status errorStatus)
        {
            BulkUploadMediaAssetResult bulkUploadAssetResult = new BulkUploadMediaAssetResult()
            {
                Index = index,
                ObjectId = Id > 0 ? Id : (long?)null,
                BulkUploadId = bulkUploadId,
                Status = status,
                Type = this.MediaType != null && this.MediaType.m_nTypeID > 0 ? this.MediaType.m_nTypeID : (int?)null,
                ExternalId = string.IsNullOrEmpty(this.CoGuid) ? null : this.CoGuid
            };

            if (errorStatus != null)
            {
                bulkUploadAssetResult.SetError(errorStatus);
            }
            return bulkUploadAssetResult;
        }
        
        public override bool Enqueue(int groupId, long userId, long bulkUploadId, BulkUploadJobAction jobAction, int resultIndex)
        {
            GenericCeleryQueue queue = new GenericCeleryQueue();
            var data = new BulkUploadItemData<MediaAsset>(this.DistributedTask, groupId, userId, bulkUploadId, jobAction, resultIndex, this);
            bool enqueueSuccessful = queue.Enqueue(data, string.Format(this.RoutingKey, groupId));

            return enqueueSuccessful;
        }
        
        #endregion
    }
}