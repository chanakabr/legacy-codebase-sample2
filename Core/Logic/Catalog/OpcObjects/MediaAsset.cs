using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiLogic.Catalog.CatalogManagement.Managers;
using TVinciShared;

namespace Core.Catalog
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class MediaAsset : Asset
    {
        #region Consts

        // ASSET EXCEL COLUMNS
        public const string MEDIA_ASSET_TYPE = "Media Asset Type";
        public const string GEO_RULE = "Geo Block Rule";
        public const string DEVICE_RULE = "Device Rule";
        public const string FILES = "FILES";

        #endregion

        #region Data Members

        [ExcelColumn(ExcelColumnType.AvailabilityMeta, AssetManager.CATALOG_START_DATE_TIME_META_SYSTEM_NAME, IsMandatory = true, IsUniqueMeta = true)]
        [JsonProperty("CatalogStartDate")]
        public DateTime? CatalogStartDate { get; set; }

        [ExcelColumn(ExcelColumnType.AvailabilityMeta, AssetManager.PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME, IsMandatory = true, IsUniqueMeta = true)]
        [JsonProperty("FinalEndDate")]
        public DateTime? FinalEndDate { get; set; }

        [ExcelColumn(ExcelColumnType.Basic, MEDIA_ASSET_TYPE, IsMandatory = true)]
        [JsonProperty("MediaType")]
        public MediaType MediaType { get; set; }

        [ExcelColumn(ExcelColumnType.Basic, AssetManager.ENTRY_ID_META_SYSTEM_NAME)]
        [JsonProperty("EntryId")]
        public string EntryId { get; set; }

        [ExcelColumn(ExcelColumnType.Rule, DEVICE_RULE)]
        [JsonProperty("DeviceRuleId")]
        public int? DeviceRuleId { get; set; }

        [ExcelColumn(ExcelColumnType.Rule, GEO_RULE)]
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
        
        public string FallBackEpgIdentifier { get { return fallbackEpgIdentifier; } }

        [JsonProperty("fallbackEpgIdentifier")]
        private string fallbackEpgIdentifier;

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
                        DateTime? catalogStartDate, DateTime? finalEndDate, MediaType mediaType, string entryId, int? deviceRuleId, int? geoBlockRuleId, List<AssetFile> files, string userTypes,
                        AssetInheritancePolicy assetInheritancePolicy, string fallbackEpgIdentifier)
            : base(id, assetType, name, namesWithLanguages, description, descriptionsWithLanguages, createDate, startDate, updateDate, endDate, metas, tags, images, coGuid)
        {
            this.CatalogStartDate = catalogStartDate;
            if (this.CatalogStartDate.HasValue)
            {
                this.Metas.Add(new Metas(new TagMeta(AssetManager.CATALOG_START_DATE_TIME_META_SYSTEM_NAME, MetaType.DateTime.ToString()), this.CatalogStartDate.Value.ToString(DateUtils.MAIN_FORMAT)));
            }

            this.FinalEndDate = finalEndDate;
            if (this.FinalEndDate.HasValue)
            {
                this.Metas.Add(new Metas(new TagMeta(AssetManager.PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME, MetaType.DateTime.ToString()), this.FinalEndDate.Value.ToString(DateUtils.MAIN_FORMAT)));
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
            this.fallbackEpgIdentifier = fallbackEpgIdentifier;
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
            this.fallbackEpgIdentifier = mediaAssetToCopy.fallbackEpgIdentifier;            
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
            this.MediaAssetType = MediaAssetType.Media;
            if (!string.IsNullOrEmpty(mediaObj.m_ExternalIDs))
            {
                int epg_channel_id = 0;
                if (int.TryParse(mediaObj.m_ExternalIDs, out epg_channel_id) && epg_channel_id > 0)
                {
                    this.MediaAssetType = MediaAssetType.Linear;
                }
                else
                {
                    this.fallbackEpgIdentifier = mediaObj.m_ExternalIDs;
                }
            }
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
                    if (CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
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

            DateTime? finalEndDate = GetBasicMetaDate(this.FinalEndDate, AssetManager.PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME);
            if (finalEndDate.HasValue)
            {
                var excelColumn = ExcelColumn.GetFullColumnName(AssetManager.PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME, null, null, true);
                excelValues.TryAdd(excelColumn, finalEndDate);
            }

            foreach (var file in Files)
            {
                excelValues.TryAddRange(file.GetExcelValues(groupId));
            }

            if (this.DeviceRuleId.HasValue)
            {
                var deviceRuleName = TvmRuleManager.GetDeviceRuleName(groupId, this.DeviceRuleId.Value);
                if (!string.IsNullOrEmpty(deviceRuleName))
                {
                    var excelColumn = ExcelColumn.GetFullColumnName(DEVICE_RULE);
                    excelValues.TryAdd(excelColumn, deviceRuleName);
                }
            }

            if (this.GeoBlockRuleId.HasValue)
            {
                var geoBlockRuleName = TvmRuleManager.GetGeoBlockRuleName(groupId, this.GeoBlockRuleId.Value);
                if (!string.IsNullOrEmpty(geoBlockRuleName))
                {
                    var excelColumn = ExcelColumn.GetFullColumnName(GEO_RULE);
                    excelValues.TryAdd(excelColumn, geoBlockRuleName);
                }
            }

            if (this.IsActive.HasValue)
            {
                var excelColumn = ExcelColumn.GetFullColumnName(AssetManager.STATUS_META_SYSTEM_NAME, null, null, true);
                excelValues.TryAdd(excelColumn, this.IsActive);
            }

            if (!string.IsNullOrEmpty(this.EntryId))
            {
                var excelColumn = ExcelColumn.GetFullColumnName(AssetManager.ENTRY_ID_META_SYSTEM_NAME, null, null, false);
                excelValues.TryAdd(excelColumn, this.EntryId);
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
                var metaDate = this.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.Equals(metaDateSystemName));
                if (metaDate != null)
                {
                    basicMetaDate = DateUtils.TryExtractDate(metaDate.m_sValue);
                }
            }

            return basicMetaDate;
        }

        public override void SetExcelValues(int groupId, Dictionary<string, object> columnNamesToValues, Dictionary<string, ExcelColumn> columns, IExcelStructureManager structureManager)
        {
            AssetType = eAssetTypes.MEDIA;
            var assetStruct = (structureManager as AssetStructStructureManager)?.AssetStruct;
            if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out var catalogGroupCache))
            {
                return;
            }

            HashSet<string> fileTypesSystemName = new HashSet<string>();
            Dictionary<string, List<LanguageContainer>> dicMetas = new Dictionary<string, List<LanguageContainer>>();
            Dictionary<string, ImageType> imageTypesMapBySystemName = Core.Catalog.CatalogManagement.ImageManager.GetImageTypesMapBySystemName(groupId);

            foreach (var columnValue in columnNamesToValues)
            {
                try
                {
                    if (columns.ContainsKey(columnValue.Key))
                    {
                        switch (columns[columnValue.Key].ColumnType)
                        {
                            case ExcelColumnType.Basic:
                                SetBasicByExcelValues(columnValue, assetStruct);
                                break;
                            case ExcelColumnType.File:
                                var fileSystemName = columns[columnValue.Key].SystemName;
                                if (!fileTypesSystemName.Contains(fileSystemName))
                                {
                                    fileTypesSystemName.Add(fileSystemName);
                                    var fileValues = columnNamesToValues.Where(x => x.Key.StartsWith(fileSystemName)).ToDictionary(x => x.Key, x => x.Value);
                                    var fileColumns = columns.Where(x => x.Key.StartsWith(fileSystemName)).ToDictionary(x => x.Key, x => x.Value);
                                    SetFileByExcelValues(groupId, fileValues, fileColumns, structureManager);
                                }
                                break;
                            case ExcelColumnType.Image:
                                SetImageByExcelValues(columnValue, columns[columnValue.Key], imageTypesMapBySystemName);
                                break;
                            case ExcelColumnType.Meta:
                                SetMetaByExcelValues(columnValue, columns[columnValue.Key], catalogGroupCache.GetDefaultLanguage().Code, ref dicMetas);
                                break;
                            case ExcelColumnType.Rule:
                                SetRuleByExcelValues(columnValue, groupId);
                                break;
                            case ExcelColumnType.Tag:
                                SetTagByExcelValues(columnValue, columns[columnValue.Key].SystemName, assetStruct.TopicsMapBySystemName, catalogGroupCache.GetDefaultLanguage().Code);
                                break;
                            default:
                                SetPropertyByExcelValue(columns[columnValue.Key].Property, columnValue.Value);
                                break;
                        }
                    }
                }
                catch (ExcelParserException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    var excelParserException = new ExcelParserException(ex, columnValue.Key, columnValue.Value);
                    throw excelParserException;
                }
            }

            foreach (var meta in dicMetas)
            {
                Metas currMeta = new Metas();
                if (assetStruct.TopicsMapBySystemName.ContainsKey(meta.Key))
                {
                    currMeta.m_oTagMeta = new TagMeta()
                    {
                        m_sName = meta.Key,
                        m_sType = assetStruct.TopicsMapBySystemName[meta.Key].Type.ToString()
                    };
                }

                var defaultIndex = meta.Value.FindIndex(x => x.IsDefault);
                if (defaultIndex != -1)
                {
                    var value = meta.Value[defaultIndex].m_sValue;
                    if (currMeta.m_oTagMeta != null && currMeta.m_oTagMeta.m_sType.Equals(MetaType.Bool.ToString()))
                    {
                        bool boolValue = bool.Parse(value);
                        value = boolValue ? "1" : "0";
                    }

                    meta.Value[defaultIndex].m_sValue = value;
                    currMeta.m_sValue = meta.Value[defaultIndex].m_sValue;
                }

                currMeta.Value = meta.Value.ToArray();
                this.Metas.Add(currMeta);
            }
        }

        private void SetRuleByExcelValues(KeyValuePair<string, object> columnValue, int groupId)
        {
            if (columnValue.Value == null) { return; }
            var ruleName = columnValue.Value.ToString();
            if (string.IsNullOrEmpty(ruleName)) { return; }

            var deviceRuleColumn = ExcelColumn.GetFullColumnName(DEVICE_RULE);
            if (columnValue.Key.Equals(deviceRuleColumn))
            {
                this.DeviceRuleId = (int?)TvmRuleManager.GetDeviceRuleId(groupId, ruleName);
                return;
            }

            var geoRuleColumn = ExcelColumn.GetFullColumnName(GEO_RULE);
            if (columnValue.Key.Equals(geoRuleColumn))
            {
                this.GeoBlockRuleId = (int?)TvmRuleManager.GetGeoBlockRuleId(groupId, ruleName);
                return;
            }
        }

        private void SetFileByExcelValues(int groupId, Dictionary<string, object> fileValues, Dictionary<string, ExcelColumn> fileColumns, IExcelStructureManager structureObject)
        {
            if (fileValues != null && fileValues.Count > 0 && fileColumns != null && fileColumns.Count > 0)
            {
                var file = new AssetFile();
                file.SetExcelValues(groupId, fileValues, fileColumns, structureObject);
                Files.Add(file);
            }
        }
        
        private void SetPropertyByExcelValue(PropertyInfo property, object value)
        {
            var realType = property.PropertyType.GetRealType();
            object convertedValue;
            if (realType == DateUtils.DateTimeType || realType == DateUtils.NullableDateTimeType)
            {
                convertedValue = DateUtils.ExtractDate(value.ToString(), DateUtils.MAIN_FORMAT);
            }
            else
            {
                convertedValue = Convert.ChangeType(value, realType);
            }

            if (convertedValue != null)
            {
                property.SetValue(this, convertedValue);
            }
        }

        private void SetBasicByExcelValues(KeyValuePair<string, object> columnValue, AssetStruct assetStruct)
        {
            // MediaType
            var mediaTypeColumnName = ExcelColumn.GetFullColumnName(MEDIA_ASSET_TYPE, null, null, true);
            if (columnValue.Key.Equals(mediaTypeColumnName))
            {
                this.MediaType.m_sTypeName = columnValue.Value.ToString();
                if (assetStruct.SystemName.Equals(this.MediaType.m_sTypeName))
                {
                    this.MediaType.m_nTypeID = (int)assetStruct.Id;
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

            // ENTRY_ID
            var entryIdColumnName = ExcelColumn.GetFullColumnName(AssetManager.ENTRY_ID_META_SYSTEM_NAME, null, null, false);
            if (columnValue.Key.Equals(entryIdColumnName))
            {
                this.EntryId = columnValue.Value.ToString();
                return;
            }
        }

        #endregion
    }
}