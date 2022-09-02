using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using TVinciShared;

namespace ApiLogic.Catalog.CatalogManagement.Managers
{
    public class AssetStructStructureManager : IExcelStructureManager
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
            "//The list of PPVs for a file type can be updated. To add a single PPV use the name only format: [PPV name] OR you can enter a single PPV together with dates:  [PPV name];[StartDate];[EndDate]. For Multiple PPVs you MUST enter each PPV name followed by dates.PPVs need to be separated by “;”: [PPVname];[StartDate];[EndDate];[PPV2 Name];[Start];[End]…",
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

        public static IBulkUploadStructureManager Instance(AssetStruct assetStruct) => new AssetStructStructureManager(assetStruct);

        public AssetStruct AssetStruct { get; }

        public AssetStructStructureManager(AssetStruct assetStruct)
        {
            AssetStruct = assetStruct;
        }

        public ExcelStructure GetExcelStructure(int groupId, Type objectType = null)
        {
            ExcelStructure excelStructure = null;

            if (AssetStruct.Id > 0)
            {
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out CatalogGroupCache catalogGroupCache))
                {
                    return excelStructure;
                }

                if (AssetStruct.TopicsMapBySystemName == null || AssetStruct.TopicsMapBySystemName.Count == 0)
                {
                    if (!catalogGroupCache.AssetStructsMapById.ContainsKey(AssetStruct.Id))
                    {
                        return excelStructure;
                    }

                    AssetStruct.Copy(catalogGroupCache.AssetStructsMapById[AssetStruct.Id]);

                    AssetStruct.TopicsMapBySystemName = catalogGroupCache.TopicsMapById.Where(x => AssetStruct.MetaIds.Contains(x.Key))
                                                                                .OrderBy(x => AssetStruct.MetaIds.IndexOf(x.Key))
                                                                                .ToDictionary(x => x.Value.SystemName, y => y.Value);
                }

                if (objectType == null)
                {
                    objectType = GetAssetType(AssetStruct);
                }

                var systemNameToExcelAttribute = ExcelManager.GetSystemNameToProperyData(objectType);
                var excelColumns = new Dictionary<string, ExcelColumn>();
                var mandatoryPropertyAndValueMap = new Dictionary<string, object>();

                // mediaType
                if (systemNameToExcelAttribute.ContainsKey(MediaAsset.MEDIA_ASSET_TYPE))
                {
                    var excelColumn = ExcelManager.GetExcelColumnByAttribute(systemNameToExcelAttribute[MediaAsset.MEDIA_ASSET_TYPE], MediaAsset.MEDIA_ASSET_TYPE);
                    var mediaAssetTypeColumnName = excelColumn.ToString();
                    excelColumns.TryAdd(mediaAssetTypeColumnName, excelColumn);
                    mandatoryPropertyAndValueMap.Add(mediaAssetTypeColumnName, AssetStruct.SystemName);
                }

                // EXTERNAL_ID
                if (systemNameToExcelAttribute.ContainsKey(Asset.EXTERNAL_ASSET_ID))
                {
                    var excelColumn = ExcelManager.GetExcelColumnByAttribute(systemNameToExcelAttribute[Asset.EXTERNAL_ASSET_ID], Asset.EXTERNAL_ASSET_ID);
                    excelColumns.TryAdd(excelColumn.ToString(), excelColumn);
                }

                // ENTRY_ID
                if (systemNameToExcelAttribute.ContainsKey(AssetManager.ENTRY_ID_META_SYSTEM_NAME))
                {
                    var excelColumn = ExcelManager.GetExcelColumnByAttribute(systemNameToExcelAttribute[AssetManager.ENTRY_ID_META_SYSTEM_NAME], AssetManager.ENTRY_ID_META_SYSTEM_NAME);
                    excelColumns.TryAdd(excelColumn.ToString(), excelColumn);
                }

                // METAS AND TAGS
                if (systemNameToExcelAttribute.ContainsKey(Asset.METAS) && systemNameToExcelAttribute.ContainsKey(Asset.TAGS))
                {
                    Dictionary<string, string> uniqueMetasToHelpText = new Dictionary<string, string>();

                    var languages = catalogGroupCache.LanguageMapByCode.OrderByDescending(x => x.Value.IsDefault);
                    foreach (var lang in languages)
                    {
                        foreach (var topic in AssetStruct.TopicsMapBySystemName)
                        {
                            if (topic.Value.SystemName.Equals(AssetManager.EXTERNAL_ID_META_SYSTEM_NAME) ||
                                topic.Value.Type == MetaType.ReleatedEntity ||
                                (!lang.Value.IsDefault && topic.Value.Type != MetaType.MultilingualString))
                            {
                                continue;
                            }

                            string language = lang.Value.IsDefault ? null : lang.Value.Code;

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
                    var mediaFileTypesListResponse = Core.Catalog.CatalogManagement.FileManager.Instance.GetMediaFileTypes(groupId);
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
                if (systemNameToExcelAttribute.ContainsKey(MediaAsset.GEO_RULE))
                {
                    var excelColumn = ExcelManager.GetExcelColumnByAttribute(systemNameToExcelAttribute[MediaAsset.GEO_RULE], MediaAsset.GEO_RULE);
                    excelColumns.TryAdd(excelColumn.ToString(), excelColumn);
                }

                if (systemNameToExcelAttribute.ContainsKey(MediaAsset.DEVICE_RULE))
                {
                    var excelColumn = ExcelManager.GetExcelColumnByAttribute(systemNameToExcelAttribute[MediaAsset.DEVICE_RULE], MediaAsset.DEVICE_RULE);
                    excelColumns.TryAdd(excelColumn.ToString(), excelColumn);
                }

                if (AssetStruct.IsLinearAssetStruct && systemNameToExcelAttribute.ContainsKey(LiveAsset.EXTERNAL_EPG_INGEST_ID))
                {
                    var excelColumn = ExcelManager.GetExcelColumnByAttribute(systemNameToExcelAttribute[LiveAsset.EXTERNAL_EPG_INGEST_ID], LiveAsset.EXTERNAL_EPG_INGEST_ID);
                    excelColumns.TryAdd(excelColumn.ToString(), excelColumn);
                }

                excelStructure = new ExcelStructure(excelColumns, OVERVIEW_INSTRUCTIONS, COLUMNS_COLORS, mandatoryPropertyAndValueMap);
            }

            return excelStructure;
        }

        private static Type GetAssetType(AssetStruct assetStruct)
        {
            Type objectType = null;
            if (assetStruct.IsLinearAssetStruct)
            {
                objectType = typeof(LiveAsset);
            }
            else if (assetStruct.IsProgramAssetStruct)
            {
                objectType = typeof(EpgAsset);
            }
            else
            {
                objectType = typeof(MediaAsset);
            }
            return objectType;
        }
    }
}