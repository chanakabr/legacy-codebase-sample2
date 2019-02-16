using ApiObjects;
using ApiObjects.Catalog;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.App_Start;
using WebAPI.ClientManagers.Client;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset wrapper
    /// </summary>
    [Serializable]
    public partial class KalturaAssetListResponse : KalturaListResponse
    {
        private static readonly List<string> OVERVIEW_INSTRUCTIONS = new List<string>()
        {
            "//Templae Overview",
            "//The first row shows (Label) ((identifier)) - (Field Type)",
            "// Label is the friendly field name. It can only be edited via the Operator Console UI",
            "// Identifier is the system name. It can not be edited via this form",
            "// Columns marked in * are required",
            "// Field Types:",
            "// Text fields are strings (example: The Godfather)",
            "// Numeric fields are integers and suppot a single value (example: 3600). Double integers are accepted (example 7.9)",
            "// Switch values should be TRUE or FALSE",
            "// Tags should be seperated by commas (example: drama, action, family)",
            "// Date&Time (format: dd/MM/yyyy hh:mm:ss)",
            "// The help text can be edited via the Operator Console"
        };

        private static readonly Dictionary<ExcelColumnType, Color> COLUMNS_COLORS = new Dictionary<ExcelColumnType, Color>()
        {
            { ExcelColumnType.Basic, Color.Orange },
            { ExcelColumnType.MetaText, Color.Green },
            { ExcelColumnType.MetaNumber, Color.Green },
            { ExcelColumnType.MetaBool, Color.Green },
            { ExcelColumnType.MetaDate, Color.Green },
            { ExcelColumnType.MetaMultilingual, Color.Green },
            { ExcelColumnType.Tag, Color.Green },
            { ExcelColumnType.File, Color.Blue },
            { ExcelColumnType.Image, Color.Gray },
            { ExcelColumnType.Rule, Color.Yellow }
        };

        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAsset> Objects { get; set; }

        internal override List<string> GetExcelOverviewInstructions()
        {
            return OVERVIEW_INSTRUCTIONS;
        }

        internal override Dictionary<ExcelColumnType, Color> GetExcelColumnsColors()
        {
            return COLUMNS_COLORS;
        }

        internal override Dictionary<string, KalturaExcelColumn> GetExcelColumns(int groupId, Dictionary<string, object> data = null)
        {
            Dictionary<string, KalturaExcelColumn> excelColumns = new Dictionary<string, KalturaExcelColumn>();

            var baseExcelColumns = base.GetExcelColumns(groupId, data);
            excelColumns.TryAddRange(baseExcelColumns);

            if (Objects == null || Objects.Count == 0)
            {
                return excelColumns;
            }

            var duplicates = Objects.GroupBy(x => x.getType()).Select(x => x.Key).ToList();
            if (duplicates.Count > 1)
            {
                return excelColumns;
            }
            int mediaType = duplicates[0];

            List<KalturaMediaFile> maxMediaFiles = new List<KalturaMediaFile>();
            if (Objects.Count(x => x.MediaFiles != null && x.MediaFiles.Count > 0) > 0)
            {
                var assetsWithMediaFiles = Objects.Where(x => x.MediaFiles != null && x.MediaFiles.Count > 0);
                foreach (var asset in assetsWithMediaFiles)
                {
                    if (asset.MediaFiles.Count > maxMediaFiles.Count)
                    {
                        maxMediaFiles = asset.MediaFiles;
                    }
                }
            }

            CatalogGroupCache catalogGroupCache;
            if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                return excelColumns;
            }

            if (!catalogGroupCache.AssetStructsMapById.ContainsKey(mediaType))
            {
                return excelColumns;
            }

            var imageTypeListResponse = ClientsManager.CatalogClient().GetImageTypes(groupId, false, null);
            if (!imageTypeListResponse.HasObjects())
            {
                return excelColumns;
            }

            // 1.TypeDescription
            var typeDescription = ExcelFormatter.GetHiddenColumn(ExcelColumnType.Basic, KalturaMediaAsset.TYPE_DESCRIPTION);
            excelColumns.TryAdd(typeDescription, new KalturaExcelColumn(ExcelColumnType.Basic, typeDescription, KalturaMediaAsset.TYPE_DESCRIPTION));

            // 2.TYPE
            var type = ExcelFormatter.GetHiddenColumn(ExcelColumnType.Basic, KalturaAsset.TYPE);
            excelColumns.TryAdd(type, new KalturaExcelColumn(ExcelColumnType.Basic, type, KalturaAsset.TYPE));

            // 3.EXTERNAL_ID
            var externalId = ExcelFormatter.GetHiddenColumn(ExcelColumnType.Basic, AssetManager.EXTERNAL_ID_META_SYSTEM_NAME);
            excelColumns.TryAdd(externalId, new KalturaExcelColumn(ExcelColumnType.Basic, externalId, CatalogManager.BasicMediaAssetMetasSystemNameToName[AssetManager.EXTERNAL_ID_META_SYSTEM_NAME]));

            var assetStruct = catalogGroupCache.AssetStructsMapById[mediaType];
            var topics = catalogGroupCache.TopicsMapById.Where(x => assetStruct.MetaIds.Contains(x.Key))
                .OrderBy(x => assetStruct.MetaIds.IndexOf(x.Key));

            // 4. METAS AND TAGS
            Dictionary<string, string> basicMetasToHelpText = new Dictionary<string, string>();
            foreach (var topic in topics)
            {
                if (!KalturaAsset.BASIC_METAS.Contains(topic.Value.SystemName))
                {
                    var columnType = KalturaValue.GetExcelMetaColumnType(topic.Value.Type);
                    if (columnType.HasValue)
                    {
                        var defaultHiddenName = ExcelFormatter.GetHiddenColumn(columnType.Value, topic.Value.SystemName);
                        excelColumns.TryAdd(defaultHiddenName, new KalturaExcelColumn(columnType.Value, defaultHiddenName, topic.Value.Name, topic.Value.HelpText));
                        if (topic.Value.Type == ApiObjects.MetaType.MultilingualString)
                        {
                            foreach (var lang in catalogGroupCache.LanguageMapByCode)
                            {
                                if (!lang.Value.IsDefault)
                                {
                                    var nonDefaultHiddenName = ExcelFormatter.GetHiddenColumn(columnType.Value, topic.Value.SystemName, lang.Key);
                                    var nonDefaultFriendlyName = ExcelFormatter.GetNonDefaultFriendlyColumnName(topic.Value.Name, lang.Key);
                                    excelColumns.TryAdd(nonDefaultHiddenName, new KalturaExcelColumn(columnType.Value, nonDefaultHiddenName, nonDefaultFriendlyName));
                                }
                            }
                        }
                    }
                }
                else
                {
                    basicMetasToHelpText.Add(topic.Value.SystemName, topic.Value.HelpText);
                }
            }

            // 5. STATUS
            if (basicMetasToHelpText.ContainsKey(AssetManager.STATUS_META_SYSTEM_NAME))
            {
                var status = ExcelFormatter.GetHiddenColumn(ExcelColumnType.MetaBool, AssetManager.STATUS_META_SYSTEM_NAME);
                excelColumns.TryAdd(status, new KalturaExcelColumn(ExcelColumnType.MetaBool,
                                                    status,
                                                    CatalogManager.BasicMediaAssetMetasSystemNameToName[AssetManager.STATUS_META_SYSTEM_NAME],
                                                    basicMetasToHelpText[AssetManager.STATUS_META_SYSTEM_NAME]));
            }

            // 6. DATES
            if (basicMetasToHelpText.ContainsKey(AssetManager.CATALOG_START_DATE_TIME_META_SYSTEM_NAME))
            {
                var catalogStartDate = ExcelFormatter.GetHiddenColumn(ExcelColumnType.MetaDate, AssetManager.CATALOG_START_DATE_TIME_META_SYSTEM_NAME);
                excelColumns.TryAdd(catalogStartDate, new KalturaExcelColumn(ExcelColumnType.MetaDate,
                                                    catalogStartDate,
                                                    CatalogManager.BasicMediaAssetMetasSystemNameToName[AssetManager.CATALOG_START_DATE_TIME_META_SYSTEM_NAME],
                                                    basicMetasToHelpText[AssetManager.CATALOG_START_DATE_TIME_META_SYSTEM_NAME]));
            }

            if (basicMetasToHelpText.ContainsKey(AssetManager.CATALOG_END_DATE_TIME_META_SYSTEM_NAME))
            {
                var catalogEndDate = ExcelFormatter.GetHiddenColumn(ExcelColumnType.MetaDate, AssetManager.CATALOG_END_DATE_TIME_META_SYSTEM_NAME);
                excelColumns.TryAdd(catalogEndDate, new KalturaExcelColumn(ExcelColumnType.MetaDate,
                                                    catalogEndDate,
                                                    CatalogManager.BasicMediaAssetMetasSystemNameToName[AssetManager.CATALOG_END_DATE_TIME_META_SYSTEM_NAME],
                                                    basicMetasToHelpText[AssetManager.CATALOG_END_DATE_TIME_META_SYSTEM_NAME]));
            }

            if (basicMetasToHelpText.ContainsKey(AssetManager.PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME))
            {
                var playbackStartDate = ExcelFormatter.GetHiddenColumn(ExcelColumnType.MetaDate, AssetManager.PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME);
                excelColumns.TryAdd(playbackStartDate, new KalturaExcelColumn(ExcelColumnType.MetaDate,
                                                    playbackStartDate,
                                                    CatalogManager.BasicMediaAssetMetasSystemNameToName[AssetManager.PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME],
                                                    basicMetasToHelpText[AssetManager.PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME]));
            }

            if (basicMetasToHelpText.ContainsKey(AssetManager.PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME))
            {
                var playbackEndDate = ExcelFormatter.GetHiddenColumn(ExcelColumnType.MetaDate, AssetManager.PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME);
                excelColumns.TryAdd(playbackEndDate, new KalturaExcelColumn(ExcelColumnType.MetaDate,
                                                    playbackEndDate,
                                                    CatalogManager.BasicMediaAssetMetasSystemNameToName[AssetManager.PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME],
                                                    basicMetasToHelpText[AssetManager.PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME]));
            }

            // 7. IMAGES
            excelColumns.TryAddRange(imageTypeListResponse.GetExcelColumns(groupId));

            // 8. FILES
            int maxMediaFilesCount = 0;
            if (maxMediaFiles != null)
            {
                maxMediaFilesCount = maxMediaFiles.Count;
            }
            KalturaMediaFileListResponse mediaFileListResponse = new KalturaMediaFileListResponse() { Files = maxMediaFiles };

            excelColumns.TryAddRange(mediaFileListResponse.GetExcelColumns
                (groupId, new Dictionary<string, object>() { { KalturaMediaFileListResponse.MAX_MEDIA_FILES, maxMediaFilesCount } }));

            // 9. RULES
            var geoRule = ExcelFormatter.GetHiddenColumn(ExcelColumnType.Rule, KalturaMediaAsset.GEO_RULE_ID);
            excelColumns.TryAdd(geoRule, new KalturaExcelColumn(ExcelColumnType.Rule, geoRule, KalturaMediaAsset.GEO_RULE_ID));

            var deviceRule = ExcelFormatter.GetHiddenColumn(ExcelColumnType.Rule, KalturaMediaAsset.DEVICE_RULE_ID);
            excelColumns.TryAdd(deviceRule, new KalturaExcelColumn(ExcelColumnType.Rule, deviceRule, KalturaMediaAsset.DEVICE_RULE_ID));

            return excelColumns;
        }
    }
}