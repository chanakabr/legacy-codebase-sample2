using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
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
    public class MediaAsset : Asset, IExcelStructure
    {
        private static readonly List<string> OVERVIEW_INSTRUCTIONS = new List<string>()
        {
            "//Template Overview",
            "//Batch upload to OPC is limited to 200 media assets per file",
            "//The media asset information is grouped and color-coded by OPC tabs: Metadata, Availability, Images, Playback Files, Rules",
            "//The first two columns indicate the media asset type. If you are updating an existing media asset, do not modify these columns",
            "//To create a new asset, enter the media asset type name (column A) and an asterisk (*) in column B",
            "//To create a new asset with a specific External ID, enter your chosen ID in column B",
            "//The first row of the media asset table includes the field identifier and field type (example: Name (Text Field))",
            "//Secondary languages are marked with “:“. (example: Name:spanish (Text Field))",
            "//You may edit this template to only include the fields you wish to update, but columns marked with * are mandatory",
            "//Field types information:",
            "//Text fields are strings (example: The Godfather)",
            "//Numeric fields are integers single value is supported (example: 3600). Double integers are also accepted (example 7.9)",
            "//Switch values should be TRUE or FALSE",
            "//Tags should be separated by commas (example: drama, action, family)",
            "//Date&Time (format: dd/mm/yyyy hh:mm:ss)",
            "//Playback file types columns include the file identifier and the field type as it appears in OPC (example: AndroidMain:External Id)",
            "//Image types columns include the image identifier and the field type as it appears in OPC (example: BoxCoverEnglish:Image URL)",
            "//PPV for file types can be updated using the PPV name separated by “;”. (example: [PPV name];[PPVname]. PPV with dates[PPV1];[Start];[End];[PPV2];[Start];[End]…)",
            "//Image columns are generated with no value by default. If you wish to update an image, enter the URL in the appropriate Image URL column",
            "//For rules, provide the rule name as it appears in OPC."
        };

        private static readonly Dictionary<ExcelColumnType, Color> COLUMNS_COLORS = new Dictionary<ExcelColumnType, Color>()
        {
            { ExcelColumnType.Basic, Color.Red },
            { ExcelColumnType.Meta, Color.FromArgb(51, 204, 51) }, // green
            { ExcelColumnType.Tag, Color.FromArgb(51, 204, 51) }, // green
            { ExcelColumnType.AvailabilityMeta, Color.FromArgb(255, 153, 0) }, //orange
            { ExcelColumnType.File,  Color.FromArgb(0, 204, 255) }, // Cyan
            { ExcelColumnType.Image, Color.FromArgb(204, 0, 102) }, // Purple
            { ExcelColumnType.Rule, Color.FromArgb(255, 234, 0) } // Yellow
        };

        #region Consts

        public override string DistributedTask { get { return "distributed_tasks.process_bulk_upload_media_asset"; } }
        public override string RoutingKey { get { return "PROCESS_BULK_UPLOAD_MEDIA_ASSET\\{0}"; } }

        // ASSET EXCEL COLUMNS
        private const string MEDIA_ASSET_TYPE = "Media Asset Type";
        private const string GEO_RULE_ID = "GeoBlockRuleId";
        private const string DEVICE_RULE_ID = "DeviceRuleId";
        private const string FILES = "FILES";

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
            
            if (this.CatalogStartDate.HasValue)
            {
                var excelColumn = ExcelColumn.GetFullColumnName(AssetManager.CATALOG_START_DATE_TIME_META_SYSTEM_NAME, null, null, true);
                excelValues.TryAdd(excelColumn, this.CatalogStartDate);
            }

            if (this.FinalEndDate.HasValue)
            {
                var excelColumn = ExcelColumn.GetFullColumnName(AssetManager.CATALOG_END_DATE_TIME_META_SYSTEM_NAME, null, null, true);
                excelValues.TryAdd(excelColumn, this.FinalEndDate);
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
        
        public static ExcelStructure GetExcelStructure(int groupId, Dictionary<string, object> data = null)
        {
            // TODO SHIR - SET in layered cache by MEDIA_TYPE and groupId
            ExcelStructure excelStructer = null;
            
            if (data != null && data.ContainsKey(BulkUploadAssetData.MEDIA_TYPE))
            {
                int? mediaType = data[BulkUploadAssetData.MEDIA_TYPE] as int?;

                if (mediaType.HasValue)
                {
                    CatalogGroupCache catalogGroupCache;
                    if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        return excelStructer;
                    }

                    if (!catalogGroupCache.AssetStructsMapById.ContainsKey(mediaType.Value))
                    {
                        return excelStructer;
                    }

                    var systemNameToExcelAttribute = ExcelManager.GetSystemNameToProperyData(typeof(MediaAsset));
                    var excelColumns = new Dictionary<string, ExcelColumn>();

                    // mediaType
                    if (systemNameToExcelAttribute.ContainsKey(MEDIA_ASSET_TYPE))
                    {
                        var excelColumn = ExcelManager.GetExcelColumnByAttribute(systemNameToExcelAttribute[MEDIA_ASSET_TYPE], MEDIA_ASSET_TYPE);
                        excelColumns.TryAdd(excelColumn.ToString(), excelColumn);
                    }
                    
                    // EXTERNAL_ID
                    if (systemNameToExcelAttribute.ContainsKey(Asset.EXTERNAL_ASSET_ID))
                    {
                        var excelColumn = ExcelManager.GetExcelColumnByAttribute(systemNameToExcelAttribute[Asset.EXTERNAL_ASSET_ID], Asset.EXTERNAL_ASSET_ID);
                        excelColumns.TryAdd(excelColumn.ToString(), excelColumn);
                    }
                    
                    // METAS AND TAGS
                    if (systemNameToExcelAttribute.ContainsKey(Asset.METAS) && systemNameToExcelAttribute.ContainsKey(Asset.TAGS))
                    {
                        var assetStruct = catalogGroupCache.AssetStructsMapById[mediaType.Value];
                        var topics = catalogGroupCache.TopicsMapById.Where(x => assetStruct.MetaIds.Contains(x.Key))
                            .OrderBy(x => assetStruct.MetaIds.IndexOf(x.Key));

                        Dictionary<string, string> uniqueMetasToHelpText = new Dictionary<string, string>();

                        var languages = catalogGroupCache.LanguageMapByCode.OrderByDescending(x => x.Value.IsDefault);
                        foreach (var lang in languages)
                        {
                            foreach (var topic in topics)
                            {
                                string language = null;
                                if (!lang.Value.IsDefault)
                                {
                                    if (topic.Value.Type != MetaType.MultilingualString)
                                    {
                                        continue;
                                    }

                                    language = lang.Value.Code;
                                }

                                if (systemNameToExcelAttribute.ContainsKey(topic.Value.SystemName) && 
                                    systemNameToExcelAttribute[topic.Value.SystemName].Item1.IsUniqueMeta)
                                {
                                    uniqueMetasToHelpText.TryAdd(topic.Value.SystemName, topic.Value.HelpText);
                                    continue;
                                }
                                
                                var topicAttribute = topic.Value.Type == MetaType.Tag ? systemNameToExcelAttribute[Asset.TAGS] : systemNameToExcelAttribute[Asset.METAS];
                                var topicColumn = ExcelManager.GetExcelColumnByAttribute(topicAttribute, topic.Value.SystemName, language, topic.Value.HelpText);
                                excelColumns.TryAdd(topicColumn.ToString(), topicColumn);
                            }
                        }

                        // Uniqe Metas (status and dates)
                        foreach (var uniqeMeta in uniqueMetasToHelpText)
                        {
                            var uniqeMetaColumn = 
                                ExcelManager.GetExcelColumnByAttribute(systemNameToExcelAttribute[uniqeMeta.Key],
                                                                       uniqeMeta.Key,
                                                                       null,
                                                                       uniqueMetasToHelpText[uniqeMeta.Key]);
                            excelColumns.TryAdd(uniqeMetaColumn.ToString(), uniqeMetaColumn);
                        }
                    }

                    // IMAGES
                    if (systemNameToExcelAttribute.ContainsKey(Asset.IMAGES))
                    {
                        var imageTypeListResponse = ImageManager.GetImageTypes(groupId, false, null);
                        if (imageTypeListResponse.HasObjects())
                        {
                            foreach (var imageType in imageTypeListResponse.Objects)
                            {
                                var excelColumn = ExcelManager.GetExcelColumnByAttribute(systemNameToExcelAttribute[Asset.IMAGES], imageType.SystemName, null, imageType.HelpText, Asset.IMAGES);
                                excelColumns.TryAdd(excelColumn.ToString(), excelColumn);
                            }
                        }
                    }

                    // FILES
                    if (systemNameToExcelAttribute.ContainsKey(FILES))
                    {
                        var mediaFileTypesListResponse = FileManager.GetMediaFileTypes(groupId);
                        if (mediaFileTypesListResponse.HasObjects())
                        {
                            var fileSystemNameToExcelAttribute = ExcelManager.GetSystemNameToProperyData(typeof(AssetFile));
                            foreach (var mediaFileType in mediaFileTypesListResponse.Objects)
                            {
                                foreach (var fileAttribute in fileSystemNameToExcelAttribute)
                                {
                                    var excelColumn = ExcelManager.GetExcelColumnByAttribute(fileAttribute.Value, mediaFileType.Name, null, mediaFileType.Description, fileAttribute.Key);
                                    excelColumns.TryAdd(excelColumn.ToString(), excelColumn);
                                }
                            }
                        }
                    }
                   
                    // RULES
                    if (systemNameToExcelAttribute.ContainsKey(MediaAsset.GEO_RULE_ID))
                    {
                        var excelColumn = ExcelManager.GetExcelColumnByAttribute(systemNameToExcelAttribute[MediaAsset.GEO_RULE_ID], MediaAsset.GEO_RULE_ID);
                        excelColumns.TryAdd(excelColumn.ToString(), excelColumn);
                    }

                    if (systemNameToExcelAttribute.ContainsKey(MediaAsset.DEVICE_RULE_ID))
                    {
                        var excelColumn = ExcelManager.GetExcelColumnByAttribute(systemNameToExcelAttribute[MediaAsset.DEVICE_RULE_ID], MediaAsset.DEVICE_RULE_ID);
                        excelColumns.TryAdd(excelColumn.ToString(), excelColumn);
                    }

                    excelStructer = new ExcelStructure(excelColumns, OVERVIEW_INSTRUCTIONS, COLUMNS_COLORS);
                }
            }

            return excelStructer;
        }

        ExcelStructure IExcelStructure.GetExcelStructure(int groupId, Dictionary<string, object> data)
        {
            return MediaAsset.GetExcelStructure(groupId, data);
        }
        
        public override void SetExcelValues(int groupId, Dictionary<string, object> columnNamesToValues, Dictionary<string, ExcelColumn> columns)
        {
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
            // mediaType
            if (columnValue.Key.Equals(MEDIA_ASSET_TYPE))
            {
                this.MediaType.m_sTypeName = columnValue.Value.ToString();
                if (catalogGroupCache.AssetStructsMapBySystemName.ContainsKey(this.MediaType.m_sTypeName))
                {
                    this.MediaType.m_nTypeID = (int)catalogGroupCache.AssetStructsMapBySystemName[this.MediaType.m_sTypeName].Id;
                }
            }
            // EXTERNAL_ID
            else if (columnValue.Key.Equals(Asset.EXTERNAL_ASSET_ID))
            {
                this.CoGuid = columnValue.Value.ToString();
            }
        }

        #endregion

        #region IBulkUploadObject Methods

        public override BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, BulkUploadResultStatus status, int index)
        {
            BulkUploadMediaAssetResult bulkUploadAssetResult = new BulkUploadMediaAssetResult()
            {
                Index = index,
                ObjectId = Id > 0 ? Id : (long?)null,
                BulkUploadId = bulkUploadId,
                Status = status,
                Type = this.MediaType != null && this.MediaType.m_nTypeID > 0 ? this.MediaType.m_nTypeID : (int?)null,
                ExternalId = this.CoGuid
            };

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