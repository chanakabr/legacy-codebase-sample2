using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TVinciShared;

namespace Core.Catalog
{
    public class AssetStruct : IExcelStructure
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

        #region Data Members

        public long Id { get; set; }
        public string Name { get; set; }
        public List<LanguageContainer> NamesInOtherLanguages { get; set; }
        public string SystemName { get; set; }
        public List<long> MetaIds { get; set; }
        public bool? IsPredefined { get; set; }
        public long CreateDate { get; set; }
        public long UpdateDate { get; set; }
        /// <summary>
        /// Asset Struct Meta list (the key is the metaID)
        /// </summary>
        public Dictionary<long, AssetStructMeta> AssetStructMetas { get; set; }
        public HashSet<string> Features { get; set; }
        public long? ParentId { get; set; }
        public long? ConnectingMetaId { get; set; }
        public long? ConnectedParentMetaId { get; set; }
        public string PluralName { get; set; }

        // currently used only for internal use for migration and migrated accounts
        public bool IsLinearAssetStruct { get; set; }

        // currently used only for internal use for migration and migrated accounts
        public bool IsProgramAssetStruct { get; set; }

        [JsonIgnore()]
        public Dictionary<string, Topic> TopicsMapBySystemName;

        #endregion

        #region Ctor's

        public AssetStruct()
        {
            this.Id = 0;
            this.Name = string.Empty;
            this.NamesInOtherLanguages = new List<LanguageContainer>();
            this.SystemName = string.Empty;
            this.MetaIds = new List<long>();
            this.IsPredefined = null;
            this.ParentId = null;
            this.CreateDate = 0;
            this.UpdateDate = 0;
            this.AssetStructMetas = new Dictionary<long, AssetStructMeta>();
            this.Features = new HashSet<string>();
            this.ConnectingMetaId = 0;
            this.ConnectedParentMetaId = 0;
            this.PluralName = string.Empty;
        }

        public AssetStruct(long id, string name, List<LanguageContainer> namesInOtherLanguages, string systemName, bool isPredefined, long? parentId,
            long createDate, long updateDate, HashSet<string> features, long connectingMetaId, long connectedParentMetaId, string pluralName, bool isProgramAssetStruct)
        {
            this.Id = id;
            this.Name = name;
            this.NamesInOtherLanguages = new List<LanguageContainer>(namesInOtherLanguages);
            this.SystemName = systemName;
            this.MetaIds = new List<long>();
            this.IsPredefined = isPredefined;
            this.ParentId = parentId;
            this.CreateDate = createDate;
            this.UpdateDate = updateDate;
            this.AssetStructMetas = new Dictionary<long, AssetStructMeta>();
            this.Features = features != null ? new HashSet<string>(features, StringComparer.OrdinalIgnoreCase) : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this.ConnectingMetaId = connectingMetaId;
            this.ConnectedParentMetaId = connectedParentMetaId;
            this.PluralName = pluralName;
            this.IsProgramAssetStruct = isProgramAssetStruct;
        }

        public AssetStruct(AssetStruct assetStructToCopy)
        {
            Copy(assetStructToCopy);
        }

        #endregion

        public string GetCommaSeparatedFeatures()
        {
            if (this.Features != null && this.Features.Count > 0)
            {
                return string.Join(",", this.Features);
            }
            else
            {
                return string.Empty;
            }
        }

        public Status ValidateBasicMetaIds(CatalogGroupCache catalogGroupCache, bool isProgramStruct)
        {
            Status result = new Status((int)eResponseStatus.AssetStructMissingBasicMetaIds, eResponseStatus.AssetStructMissingBasicMetaIds.ToString());
            List<long> basicMetaIds = new List<long>();
            if (catalogGroupCache.TopicsMapBySystemNameAndByType != null && catalogGroupCache.TopicsMapBySystemNameAndByType.Count > 0)
            {
                if (isProgramStruct)
                {
                    basicMetaIds = catalogGroupCache.TopicsMapBySystemNameAndByType.Where(x => EpgAssetManager.BasicMetasSystemNamesToType.ContainsKey(x.Key)
                                                                                                    && x.Value.ContainsKey(EpgAssetManager.BasicMetasSystemNamesToType[x.Key]))
                                                                                                    .Select(x => x.Value[EpgAssetManager.BasicMetasSystemNamesToType[x.Key]].Id).ToList();
                }
                else
                {
                    basicMetaIds = catalogGroupCache.TopicsMapBySystemNameAndByType.Where(x => AssetManager.BasicMetasSystemNamesToType.ContainsKey(x.Key)
                                                                                                    && x.Value.ContainsKey(AssetManager.BasicMetasSystemNamesToType[x.Key]))
                                                                                                    .Select(x => x.Value[AssetManager.BasicMetasSystemNamesToType[x.Key]].Id).ToList();
                }

                if (this.MetaIds != null)
                {
                    List<long> noneExistingBasicMetaIds = basicMetaIds.Except(this.MetaIds).ToList();
                    if (noneExistingBasicMetaIds == null || noneExistingBasicMetaIds.Count == 0)
                    {
                        result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                    else
                    {
                        result = new Status((int)eResponseStatus.AssetStructMissingBasicMetaIds, string.Format("{0} for the following Meta Ids: {1}",
                                            eResponseStatus.AssetStructMissingBasicMetaIds.ToString(), string.Join(",", noneExistingBasicMetaIds)));
                    }
                }
            }

            return result;
        }

        public Status ValidateNoSystemNameDuplicationOnMetaIds(CatalogGroupCache catalogGroupCache)
        {
            Status result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            HashSet<string> metaSystemNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (long metaId in this.MetaIds)
            {
                if (!catalogGroupCache.TopicsMapById.ContainsKey(metaId))
                {
                    result = new Status((int)eResponseStatus.MetaIdsDoesNotExist, eResponseStatus.MetaIdsDoesNotExist.ToString());
                    return result;
                }

                if (metaSystemNames.Contains(catalogGroupCache.TopicsMapById[metaId].SystemName))
                {
                    result = new Status((int)eResponseStatus.AssetStructMetasConatinSystemNameDuplication, eResponseStatus.AssetStructMetasConatinSystemNameDuplication.ToString());
                    return result;
                }

                metaSystemNames.Add(catalogGroupCache.TopicsMapById[metaId].SystemName);
            }

            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("Id: {0}, ", Id));
            sb.AppendFormat("Name: {0},", Name);
            sb.AppendFormat("NamesInOtherLanguages: {0}, ", NamesInOtherLanguages != null && NamesInOtherLanguages.Count > 0 ?
                                                            string.Join(",", NamesInOtherLanguages.Select(x => string.Format("languageCode: {0}, value: {1}", x.LanguageCode, x.Value)).ToList()) : string.Empty);
            sb.AppendFormat("SystemName: {0}, ", SystemName);
            sb.AppendFormat("MetaIds: {0}, ", MetaIds != null ? string.Join(",", MetaIds) : string.Empty);
            sb.AppendFormat("IsPredefined: {0}, ", IsPredefined.HasValue ? IsPredefined.Value.ToString() : string.Empty);
            sb.AppendFormat("ParentId: {0}, ", ParentId.HasValue ? ParentId.Value.ToString() : string.Empty);
            sb.AppendFormat("CreateDate: {0}, ", CreateDate);
            sb.AppendFormat("UpdateDate: {0}", UpdateDate);
            sb.AppendFormat("AssetStructMetas: {0}, ", AssetStructMetas != null && AssetStructMetas.Count > 0 ?
                                                      string.Join(",", AssetStructMetas.Select(x => x.Value.ToString()).ToList()) : string.Empty);
            sb.AppendFormat("Features: {0}, ", (Features != null && Features.Count > 0) ? string.Join(",", Features) : string.Empty);
            sb.AppendFormat("ConnectingMetaId: {0}, ", ConnectingMetaId.HasValue ? ConnectingMetaId.Value.ToString() : string.Empty);
            sb.AppendFormat("ConnectedParentMetaId: {0}, ", ConnectedParentMetaId.HasValue ? ConnectedParentMetaId.Value.ToString() : string.Empty);
            sb.AppendFormat("PluralName: {0},", PluralName);
            sb.AppendFormat("IsProgramAssetStruct: {0},", IsProgramAssetStruct);

            return sb.ToString();
        }

        private void Copy(AssetStruct assetStructToCopy)
        {
            this.Id = assetStructToCopy.Id;
            this.Name = string.Copy(assetStructToCopy.SystemName);
            this.NamesInOtherLanguages = new List<LanguageContainer>(assetStructToCopy.NamesInOtherLanguages);
            this.SystemName = string.Copy(assetStructToCopy.SystemName);
            this.MetaIds = new List<long>(assetStructToCopy.MetaIds);
            this.IsPredefined = assetStructToCopy.IsPredefined;
            this.ParentId = assetStructToCopy.ParentId;
            this.CreateDate = assetStructToCopy.CreateDate;
            this.UpdateDate = assetStructToCopy.UpdateDate;
            this.AssetStructMetas = new Dictionary<long, AssetStructMeta>(assetStructToCopy.AssetStructMetas);
            this.Features = assetStructToCopy.Features != null ? new HashSet<string>(assetStructToCopy.Features, StringComparer.OrdinalIgnoreCase) : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this.ConnectingMetaId = assetStructToCopy.ConnectingMetaId;
            this.ConnectedParentMetaId = assetStructToCopy.ConnectedParentMetaId;
            this.PluralName = assetStructToCopy.PluralName;
            this.IsProgramAssetStruct = assetStructToCopy.IsProgramAssetStruct;
        }

        #region IExcelStructure

        public ExcelStructure GetExcelStructure(int groupId, Dictionary<string, object> data = null)
        {
            ExcelStructure excelStructure = null;

            if (this.Id > 0)
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    return excelStructure;
                }

                if (this.TopicsMapBySystemName == null || this.TopicsMapBySystemName.Count == 0)
                {
                    if (!catalogGroupCache.AssetStructsMapById.ContainsKey(Id))
                    {
                        return excelStructure;
                    }

                    Copy(catalogGroupCache.AssetStructsMapById[Id]);

                    this.TopicsMapBySystemName = catalogGroupCache.TopicsMapById.Where(x => this.MetaIds.Contains(x.Key))
                                                                                .OrderBy(x => this.MetaIds.IndexOf(x.Key))
                                                                                .ToDictionary(x => x.Value.SystemName, y => y.Value);
                }

                var systemNameToExcelAttribute = ExcelManager.GetSystemNameToProperyData(typeof(MediaAsset));
                var excelColumns = new Dictionary<string, ExcelColumn>();

                // mediaType
                if (systemNameToExcelAttribute.ContainsKey(MediaAsset.MEDIA_ASSET_TYPE))
                {
                    var excelColumn = ExcelManager.GetExcelColumnByAttribute(systemNameToExcelAttribute[MediaAsset.MEDIA_ASSET_TYPE], MediaAsset.MEDIA_ASSET_TYPE);
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
                    Dictionary<string, string> uniqueMetasToHelpText = new Dictionary<string, string>();

                    var languages = catalogGroupCache.LanguageMapByCode.OrderByDescending(x => x.Value.IsDefault);
                    foreach (var lang in languages)
                    {
                        foreach (var topic in this.TopicsMapBySystemName)
                        {
                            if (topic.Value.SystemName.Equals(AssetManager.EXTERNAL_ID_META_SYSTEM_NAME))
                            {
                                continue;
                            }

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
                    var imageTypeListResponse = Core.Catalog.CatalogManagement.ImageManager.GetImageTypes(groupId, false, null);
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
                if (systemNameToExcelAttribute.ContainsKey(MediaAsset.FILES))
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

                excelStructure = new ExcelStructure(excelColumns, OVERVIEW_INSTRUCTIONS, COLUMNS_COLORS);
            }

            return excelStructure;
        }

        #endregion
    }
}