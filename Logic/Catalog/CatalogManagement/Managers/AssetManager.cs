using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using ApiObjects.TimeShiftedTv;
using CachingProvider.LayeredCache;
using Core.Api.Managers;
using Core.Catalog.Response;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml;
using Tvinci.Core.DAL;
using TVinciShared;

namespace Core.Catalog.CatalogManagement
{
    public class AssetManager
    {
        #region Constants and Read-only

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string IS_NEW_TAG_COLUMN_NAME = "tag_id";
        private const string TITLE_META_NAME = "Title";
        private const string DESCRIPTION_META_NAME = "Description";
        private const string EXTERNAL_ID_META_NAME = "External ID";
        private const string MEDIAPREP_ID_META_NAME = "MediaPrep ID";
        private const string STATUS_META_NAME = "Status";
        private const string PLAYBACK_START_DATE_META_NAME = "Playback Start Date";
        private const string CATALOG_END_DATE_META_NAME = "Catalog End Date";
        private const string PLAYBACK_END_DATE_META_NAME = "Playback End Date";
        private const string CATALOG_START_DATE_META_NAME = "Catalog Start Date";
        private const string CREATE_DATE_META_NAME = "Creation Date";
        private const string ENTRY_ID_META_SYSTEM_NAME = "EntryID";
        private const string DEVICE_RULE_ID = "DeviceRuleId";
        private const string GEO_BLOCK_RULE_ID = "GeoBlockRuleId";
        private const string ACTION_IS_NOT_ALLOWED = "Action is not allowed";

        internal const string EXTERNAL_ID_META_SYSTEM_NAME = "ExternalID";
        internal const string NAME_META_SYSTEM_NAME = "Name";
        internal const string DESCRIPTION_META_SYSTEM_NAME = "Description";
        internal const string STATUS_META_SYSTEM_NAME = "Status";
        internal const string CATALOG_START_DATE_TIME_META_SYSTEM_NAME = "CatalogStartDateTime";
        internal const string CATALOG_END_DATE_TIME_META_SYSTEM_NAME = "CatalogEndDateTime";
        internal const string PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME = "PlaybackStartDateTime";
        internal const string PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME = "PlaybackEndDateTime";
        internal const string CREATE_DATE_TIME_META_SYSTEM_NAME = "CreateDate";

        internal const string CHANNEL_ID_META_SYSTEM_NAME = "ChannelId";
        internal const string MANUAL_ASSET_STRUCT_NAME = "Manual";
        internal const string DYNAMIC_ASSET_STRUCT_NAME = "Dynamic";
        internal const string EXTERNAL_ASSET_STRUCT_NAME = "External";
        private const string TOPIC_ID_COLUMN_NAME = "TOPIC_ID";
        private const string TABLE_NAME_BASIC = "BASIC";
        private const string TABLE_NAME_METAS = "METAS";
        private const string TABLE_NAME_TAGS = "TAGS";
        private const string TABLE_NAME_FILES = "FILES";
        private const string TABLE_NAME_IMAGES = "IMAGES";
        private const string TABLE_NAME_NEW_TAGS = "NEW_TAGS";
        private const string TABLE_NAME_UPDATE_DATE = "UPDATE_DATE";        
        private const string TABLE_NAME_RELATED_ENTITIES = "RELATED_ENTITIES";        
        private const string TABLE_NAME_LINEAR = "LINEAR";        
        private const string TABLE_NAME_GEO_AVAILABILITY = "GEO_AVAILABILITY";

        private static readonly Dictionary<string, string> BasicMediaAssetMetasSystemNameToName = new Dictionary<string, string>()
        {
            { NAME_META_SYSTEM_NAME, TITLE_META_NAME },
            { DESCRIPTION_META_SYSTEM_NAME, DESCRIPTION_META_NAME },
            { EXTERNAL_ID_META_SYSTEM_NAME, EXTERNAL_ID_META_NAME },
            { ENTRY_ID_META_SYSTEM_NAME, MEDIAPREP_ID_META_NAME },
            { STATUS_META_SYSTEM_NAME, STATUS_META_NAME },
            { PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME, PLAYBACK_START_DATE_META_NAME },
            { PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME, PLAYBACK_END_DATE_META_NAME },
            { CATALOG_START_DATE_TIME_META_SYSTEM_NAME, CATALOG_START_DATE_META_NAME },
            { CATALOG_END_DATE_TIME_META_SYSTEM_NAME, CATALOG_END_DATE_META_NAME },
            { CREATE_DATE_TIME_META_SYSTEM_NAME, CREATE_DATE_META_NAME }
        };

        internal static readonly Dictionary<string, string> BasicMetasSystemNamesToType = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { NAME_META_SYSTEM_NAME, MetaType.MultilingualString.ToString() },
            { DESCRIPTION_META_SYSTEM_NAME, MetaType.MultilingualString.ToString() },
            { EXTERNAL_ID_META_SYSTEM_NAME, MetaType.String.ToString() },
            { ENTRY_ID_META_SYSTEM_NAME, MetaType.String.ToString() },
            { STATUS_META_SYSTEM_NAME, MetaType.Bool.ToString() },
            { PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME, MetaType.DateTime.ToString() },
            { PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME, MetaType.DateTime.ToString() },
            { CATALOG_START_DATE_TIME_META_SYSTEM_NAME, MetaType.DateTime.ToString() },
            { CATALOG_END_DATE_TIME_META_SYSTEM_NAME, MetaType.DateTime.ToString() },
            { CREATE_DATE_TIME_META_SYSTEM_NAME, MetaType.DateTime.ToString() }
        };

        #endregion

        #region Internal Methods

        public static bool InvalidateAsset(eAssetTypes assetType, long assetId, [System.Runtime.CompilerServices.CallerMemberName] string callingMethod = "")
        {
            bool result = true;
            string invalidationKey = LayeredCacheKeys.GetAssetInvalidationKey(assetType.ToString(), assetId);
            if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
            {
                result = false;
                log.ErrorFormat("Failed to invalidate asset with id: {0}, assetType: {1}, invalidationKey: {2} after {3}", assetId, assetType.ToString(), invalidationKey, callingMethod);
            }

            return result;
        }

        internal static void InvalidateGroupLinearAssets(int groupId)
        {
            DataTable dt = CatalogDAL.GetGroupLinearMediaIds(groupId);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    InvalidateAsset(eAssetTypes.MEDIA, ODBCWrapper.Utils.GetLongSafeVal(dr, "MEDIA_ID"));
                }
            }
        }

        #endregion

        #region Private Methods

        private static GenericResponse<Asset> CreateMediaAssetResponseFromDataSet(int groupId, Dictionary<string, DataTable> tables, LanguageObj defaultLanguage, List<LanguageObj> groupLanguages)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();            

            DataRow basicDataRow = tables[TABLE_NAME_BASIC].Rows[0];
            long id = ODBCWrapper.Utils.GetLongSafeVal(basicDataRow, "ID", 0);
            if (id <= 0)
            {
                result.SetStatus(CreateAssetResponseStatusFromResult(id));
                return result;
            }
            
            result.Object = CreateMediaAsset(groupId, id, tables, defaultLanguage, groupLanguages);

            if (result.Object != null)
            {
                result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return result;
        }

        private static MediaAsset CreateMediaAsset(int groupId, long id, Dictionary<string, DataTable> tables, LanguageObj defaultLanguage, List<LanguageObj> groupLanguages, bool isForIndex = false)
        {
            MediaAsset result = null;
            CatalogGroupCache catalogGroupCache = null;

            if(!tables.ContainsKey(TABLE_NAME_BASIC))
            {
                log.WarnFormat("CreateMediaAsset didn't basic table. assetId {0}", id);
                return result;
            }

            DataRow basicDataRow = tables[TABLE_NAME_BASIC].Rows[0];
            long assetStructId = ODBCWrapper.Utils.GetLongSafeVal(basicDataRow, "ASSET_STRUCT_ID", 0);
            MediaType mediaType;
            if (!TryGetMediaTypeFromAssetStructId(groupId, assetStructId, out mediaType))
            {
                log.WarnFormat("media type (assetStruct) is not valid for media with Id: {0}, assetStructId: {1}", id, assetStructId);
                return result;
            }

            DateTime? createDate = ODBCWrapper.Utils.GetNullableDateSafeVal(basicDataRow, "CREATE_DATE");
            DateTime? finalEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(basicDataRow, "FINAL_END_DATE");
            DateTime? startDate = ODBCWrapper.Utils.GetNullableDateSafeVal(basicDataRow, "START_DATE");
            DateTime? endDate = ODBCWrapper.Utils.GetNullableDateSafeVal(basicDataRow, "END_DATE");
            DateTime? catalogStartDate = ODBCWrapper.Utils.GetNullableDateSafeVal(basicDataRow, "CATALOG_START_DATE");
            DateTime? updateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(basicDataRow, "UPDATE_DATE");
            string fallbackEpgIdentifier = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "EPG_IDENTIFIER");

            int inheritancePolicy = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "INHERITANCE_POLICY");
            AssetInheritancePolicy assetInheritancePolicy = (AssetInheritancePolicy)Enum.ToObject(typeof(AssetInheritancePolicy), inheritancePolicy);

            // Metas and Tags table
            List<Metas> metas = null;
            List<Tags> tags = null;
            List<RelatedEntities> RelatedEntitiesList = null;
            DataTable metasTable = new DataTable();
            DataTable tagsTable = new DataTable();
            DataTable relatedEntitiesTable = new DataTable();

            if (tables.ContainsKey(TABLE_NAME_METAS)) // was [1]
            {
                metasTable = tables[TABLE_NAME_METAS];
            }

            if (tables.ContainsKey(TABLE_NAME_TAGS)) // was [2]
            {
                tagsTable = tables[TABLE_NAME_TAGS];
            }
            
            if (!TryGetMetasAndTags(groupId, id, defaultLanguage.ID, groupLanguages, metasTable, tagsTable, ref metas, ref tags))
            {
                log.WarnFormat("CreateMediaAssetFromDataSet - failed to get media metas and tags for Id: {0}", id);
                return null;
            }

            // Files table
            List<AssetFile> files = null;
            if (!isForIndex && tables.ContainsKey(TABLE_NAME_FILES) && tables[TABLE_NAME_FILES]?.Rows.Count > 0)
            {
                files = FileManager.CreateAssetFileListResponseFromDataTable(groupId, tables[TABLE_NAME_FILES]);
                // get only active files
                files = files.Where(x => x.IsActive.HasValue && x.IsActive.Value).ToList();
            }

            // Images table
            List<Image> images = new List<Image>();
            if (tables.ContainsKey(TABLE_NAME_IMAGES) && tables[TABLE_NAME_IMAGES]?.Rows.Count > 0)
            {
                GenericListResponse<Image> imageResponse = ImageManager.CreateImageListResponseFromDataTable(groupId, tables[TABLE_NAME_IMAGES], true);
                if (!imageResponse.HasObjects())
                {
                    log.WarnFormat("CreateMediaAssetFromDataSet - failed to get images for Id: {0}", id);
                    return null;
                }

                // get only active images
                images = imageResponse.Objects.Any(x => x.Status == eTableStatus.OK) ? imageResponse.Objects.Where(x => x.Status == eTableStatus.OK).ToList() : new List<Image>();
            }

            List<Image> groupDefaultImages = ImageManager.GetGroupDefaultImages(groupId);
            HashSet<long> assetImageTypes = new HashSet<long>(images.Select(x => x.ImageTypeId).ToList());
            images.AddRange(groupDefaultImages.Where(x => !assetImageTypes.Contains(x.ImageTypeId)));

            // new tags
            if (tables.ContainsKey(TABLE_NAME_NEW_TAGS) && tables[TABLE_NAME_NEW_TAGS]?.Rows.Count > 0)
            {
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling CreateMediaAsset", groupId);
                    return result;
                }

                foreach (DataRow dr in tables[TABLE_NAME_NEW_TAGS].Rows)
                {
                    int topicId = ODBCWrapper.Utils.GetIntSafeVal(dr, "topic_id");
                    int tagId = ODBCWrapper.Utils.GetIntSafeVal(dr, "tag_id");
                    int languageId = ODBCWrapper.Utils.GetIntSafeVal(dr, "language_id");
                    string translation = ODBCWrapper.Utils.GetSafeStr(dr, "translation");
                    DateTime tagCreateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "create_date");
                    DateTime tagUpdateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "update_date");
                    ApiObjects.SearchObjects.TagValue tag = new ApiObjects.SearchObjects.TagValue()
                    {
                        createDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(tagCreateDate),
                        languageId = languageId,
                        tagId = tagId,
                        topicId = topicId,
                        updateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(tagUpdateDate),
                        value = translation
                    };

                    // Update Tag Index
                    ElasticsearchWrapper wrapper = new ElasticsearchWrapper();
                    Status updateTagResponse = wrapper.UpdateTag(groupId, catalogGroupCache, tag);
                    if (updateTagResponse == null || updateTagResponse.Code != (int)eResponseStatus.OK)
                    {
                        log.WarnFormat("CreateMediaAsset - failed to update tag, tag : {0}, addTagResult: {1}", tag.ToString(),
                                        updateTagResponse != null && updateTagResponse != null ? updateTagResponse.Message : "null");
                    }
                }
            }
            // update dates
            if (tables.ContainsKey(TABLE_NAME_UPDATE_DATE) && tables[TABLE_NAME_UPDATE_DATE]?.Rows.Count > 0)
            {
                DateTime? assetUpdateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(tables[TABLE_NAME_UPDATE_DATE].Rows[0], "UPDATE_DATE");
                // overide existing update with the max update date from all possible tables (from GetAssetUpdateDate stored procedure)
                if (assetUpdateDate.HasValue)
                {
                    updateDate = assetUpdateDate;
                }
            }

            // Handle new relatedEntities
            if (tables.ContainsKey(TABLE_NAME_RELATED_ENTITIES) && tables[TABLE_NAME_RELATED_ENTITIES]?.Rows.Count > 0)
            {
                if (!TryGetRelatedEntitiesList(groupId, id, tables[TABLE_NAME_RELATED_ENTITIES], ref RelatedEntitiesList))
                {
                    log.WarnFormat("CreateMediaAsset - failed to get media RelatedEntities for Id: {0}", id);
                    return null;
                }
            }

            string name = string.Empty;
            string description = null;
            List<LanguageContainer> namesWithLanguages = null;
            List<LanguageContainer> descriptionsWithLanguages = null;
            if (!ExtractMediaAssetNamesAndDescriptionsFromMetas(metas, ref name, ref description, ref namesWithLanguages, ref descriptionsWithLanguages))
            {
                log.WarnFormat("Name is not valid for media with Id: {0}", id);
                return result;
            }

            string entryId = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "ENTRY_ID");
            string coGuid = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "CO_GUID");
            bool isActive = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "IS_ACTIVE", 0) == 1;
            int? deviceRuleId = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "DEVICE_RULE_ID", -1);
            int? geoBlockRuleId = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "GEO_BLOCK_RULE_ID", -1);
            string userTypes = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "user_types");

            result = new MediaAsset(id, eAssetTypes.MEDIA, name, namesWithLanguages, description, descriptionsWithLanguages, createDate, updateDate, startDate, endDate, metas, tags, images, coGuid,
                                    isActive, catalogStartDate, finalEndDate, mediaType, entryId, deviceRuleId == -1 ? null : deviceRuleId, geoBlockRuleId == -1 ? null : geoBlockRuleId, files, userTypes,
                                    assetInheritancePolicy, fallbackEpgIdentifier);

            result.RelatedEntities = RelatedEntitiesList;

            // if media is linear we also have the epg channel table returned
            if (!isForIndex && tables.ContainsKey(TABLE_NAME_LINEAR) && tables[TABLE_NAME_LINEAR]?.Rows.Count > 0)
            {
                result = CreateLinearMediaAssetFromDataTable(groupId, tables[TABLE_NAME_LINEAR], result);
            }

            return result;
        }

        private static List<MediaAsset> CreateMediaAssets(int groupId, DataSet ds, LanguageObj defaultLanguage, List<LanguageObj> groupLanguages)
        {
            List<MediaAsset> result = null;
            try
            {
                if (ds == null || ds.Tables == null || ds.Tables.Count < 7)
                {
                    log.WarnFormat("CreateMediaAssets didn't receive dataset with 7 or more tables");
                    return result;
                }

                // Basic details table
                if (ds.Tables[0] == null || ds.Tables[0].Rows == null || ds.Tables[0].Rows.Count <= 0)
                {
                    log.WarnFormat("CreateMediaAssets - basic details table is not valid");
                    return result;
                }

                result = new List<MediaAsset>();
                
                var metasTable = GetDataRows(ds, 1);
                var tagsTable = GetDataRows(ds, 2);
                var filesTable = GetDataRows(ds, 3);
                var imagesTable = GetDataRows(ds, 4);
                var assetUpdateDateTable = GetDataRows(ds, 5);
                var linearMediasTable = GetDataRows(ds, 6);
                var relatedEntitiesTable = GetDataRows(ds, 7);

                foreach (DataRow basicDataRow in ds.Tables[0].Rows)
                {
                    int id = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "ID", 0);
                    if (id > 0)
                    {
                        Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();
                        DataTable basicDataTable = ds.Tables[0].Clone();
                        basicDataTable.ImportRow(basicDataRow);
                        tables.Add(TABLE_NAME_BASIC, basicDataTable);
                        tables.Add(TABLE_NAME_METAS, GetTableByAssetDataRows(metasTable, "ASSET_ID", id, ds, 1));
                        tables.Add(TABLE_NAME_TAGS, GetTableByAssetDataRows(tagsTable, "ASSET_ID", id, ds, 2));
                        tables.Add(TABLE_NAME_FILES, GetTableByAssetDataRows(filesTable, "MEDIA_ID", id, ds, 3));
                        tables.Add(TABLE_NAME_IMAGES, GetTableByAssetDataRows(imagesTable, "ASSET_ID", id, ds, 4));
                        tables.Add(TABLE_NAME_UPDATE_DATE, GetTableByAssetDataRows(assetUpdateDateTable, "ID", id, ds, 5));
                        tables.Add(TABLE_NAME_LINEAR, GetTableByAssetDataRows(linearMediasTable, "MEDIA_ID", id, ds, 6));
                        tables.Add(TABLE_NAME_RELATED_ENTITIES, GetTableByAssetDataRows(relatedEntitiesTable, "ASSET_ID", id, ds, 7));
                        
                        MediaAsset mediaAsset = CreateMediaAsset(groupId, id, tables, defaultLanguage, groupLanguages);
                        if (mediaAsset != null)
                        {
                            mediaAsset.IndexStatus = AssetIndexStatus.Ok;
                            result.Add(mediaAsset);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed CreateMediaAssets for groupId: {0}", groupId), ex);
            }

            return result;
        }

        private static EnumerableRowCollection<DataRow> GetDataRows(DataSet ds, int tableIndex)
        {
            var dataRows = new DataTable().AsEnumerable();
            if (ds.Tables.Count > tableIndex && ds.Tables[tableIndex] != null && ds.Tables[tableIndex].Rows != null && ds.Tables[tableIndex].Rows.Count > 0)
            {
                dataRows = ds.Tables[tableIndex].AsEnumerable();
            }

            return dataRows;
        }

        private static DataTable GetTableByAssetDataRows(EnumerableRowCollection<DataRow> dataRows, string assetIdColumn, int assetId, DataSet ds, int tableIndex)
        {
            var dataRowsByAsset = (from row in dataRows
                                   where (Int64)row[assetIdColumn] == assetId
                                   select row);

            if (dataRowsByAsset != null && dataRowsByAsset.Any())
            {
                return dataRowsByAsset.CopyToDataTable();
            }
            else if (ds.Tables.Count > tableIndex)
            {
                return ds.Tables[tableIndex].Clone();
            }

            return null;
        }

        private static Status ValidateMediaAssetForInsert(int groupId, CatalogGroupCache catalogGroupCache, ref AssetStruct assetStruct, MediaAsset asset, ref XmlDocument metasXmlDoc,
                                                          ref XmlDocument tagsXmlDoc, ref DateTime? assetCatalogStartDate, ref DateTime? assetFinalEndDate, ref XmlDocument relatedEntitiesXmlDoc, bool isFromIngest)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            HashSet<long> assetStructMetaIds = new HashSet<long>(assetStruct.MetaIds);

            if (asset.InheritancePolicy == AssetInheritancePolicy.Enable)
            {
                SetInheritedValue(groupId, catalogGroupCache, assetStruct, asset);
            }

            result = ValidateMediaAssetMetasAndTagsNamesAndTypes(groupId, catalogGroupCache, asset.Metas, asset.Tags, assetStructMetaIds, ref metasXmlDoc, ref tagsXmlDoc,
                                                                 ref assetCatalogStartDate, ref assetFinalEndDate, isFromIngest);
            if (result.Code != (int)eResponseStatus.OK)
            {
                return result;
            }

            // validate device rule id
            if (asset.DeviceRuleId.HasValue && (asset.DeviceRuleId.Value <= 0 || !TvmRuleManager.ValidateDeviceRuleExists(groupId, asset.DeviceRuleId.Value)))
            {
                result = new Status((int)eResponseStatus.DeviceRuleDoesNotExistForGroup, eResponseStatus.DeviceRuleDoesNotExistForGroup.ToString());
                return result;
            }

            // validate geoblock rule id
            if (asset.GeoBlockRuleId.HasValue && (asset.GeoBlockRuleId.Value <= 0 || !TvmRuleManager.ValidateGeoBlockRuleExists(groupId, asset.GeoBlockRuleId.Value)))
            {
                result = new Status((int)eResponseStatus.GeoBlockRuleDoesNotExistForGroup, eResponseStatus.GeoBlockRuleDoesNotExistForGroup.ToString());
                return result;
            }

            result = ValidateRelatedEntities(groupId, catalogGroupCache, assetStructMetaIds, asset.RelatedEntities, ref relatedEntitiesXmlDoc);

            if (result.Code != (int)eResponseStatus.OK)
            {
                return result;
            }

            return result;
        }        

        private static Status ValidateMediaAssetForUpdate(int groupId, CatalogGroupCache catalogGroupCache, ref AssetStruct assetStruct, MediaAsset asset, HashSet<string> currentAssetMetasAndTags,
                                                            ref XmlDocument metasXmlDocToAdd, ref XmlDocument tagsXmlDocToAdd, ref XmlDocument metasXmlDocToUpdate, ref XmlDocument tagsXmlDocToUpdate,
                                                            ref DateTime? assetCatalogStartDate, ref DateTime? assetFinalEndDate, ref XmlDocument relatedEntitiesXmlDocToAdd, 
                                                            ref XmlDocument relatedEntitiesXmlDocToUpdate, MediaAsset currentAsset, bool isFromIngest = false)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            HashSet<long> assetStructMetaIds = new HashSet<long>(assetStruct.MetaIds);

            // in case isFromIngest = true. need to filter out the protected metas and tags. Should not update their values.
            List<AssetStructMeta> protectedAssetStructMeta = new List<AssetStructMeta>();
            if (isFromIngest && assetStruct.AssetStructMetas != null && assetStruct.AssetStructMetas.Count > 0)
            {
                protectedAssetStructMeta = assetStruct.AssetStructMetas.Values.Where(x => x.ProtectFromIngest.HasValue && x.ProtectFromIngest.Value).ToList();
                if (protectedAssetStructMeta != null && protectedAssetStructMeta.Count > 0)
                {
                    List<long> protectedMetasAndTagsById = protectedAssetStructMeta.Select(x => x.MetaId).ToList(); ;
                    List<string> protectedMetasAndTagsByName = catalogGroupCache.TopicsMapById.Where(x => protectedMetasAndTagsById.Contains(x.Key)).Select(y => y.Value.SystemName).ToList();

                    asset.Metas.RemoveAll(x => protectedMetasAndTagsByName.Contains(x.m_oTagMeta.m_sName));
                    asset.Tags.RemoveAll(x => protectedMetasAndTagsByName.Contains(x.m_oTagMeta.m_sName));
                }
            }

            List<Metas> metasToAdd = asset.Metas != null && currentAssetMetasAndTags != null ? asset.Metas.Where(x => !currentAssetMetasAndTags.Contains(x.m_oTagMeta.m_sName)).ToList() : new List<Metas>();
            List<Tags> tagsToAdd = asset.Tags != null && currentAssetMetasAndTags != null ? asset.Tags.Where(x => !currentAssetMetasAndTags.Contains(x.m_oTagMeta.m_sName)).ToList() : new List<Tags>();
            List<RelatedEntities> relatedEntitiesToAdd = asset.RelatedEntities != null && currentAssetMetasAndTags != null ? asset.RelatedEntities.Where(x => !currentAssetMetasAndTags.Contains(x.TagMeta.m_sName)).ToList() : new List<RelatedEntities>();

            result = ValidateMediaAssetMetasAndTagsNamesAndTypes(groupId, catalogGroupCache, metasToAdd, tagsToAdd, assetStructMetaIds, ref metasXmlDocToAdd, ref tagsXmlDocToAdd,
                                                                    ref assetCatalogStartDate, ref assetFinalEndDate, isFromIngest);
            if (result.Code != (int)eResponseStatus.OK)
            {
                return result;
            }

            result = ValidateRelatedEntities(groupId, catalogGroupCache, assetStructMetaIds, relatedEntitiesToAdd, ref relatedEntitiesXmlDocToAdd);
            if (result.Code != (int)eResponseStatus.OK)
            {
                return result;
            }

            List<Metas> metasToUpdate = asset.Metas != null && currentAssetMetasAndTags != null ? asset.Metas.Where(x => currentAssetMetasAndTags.Contains(x.m_oTagMeta.m_sName)).ToList() : new List<Metas>();
            List<Tags> tagsToUpdate = asset.Tags != null && currentAssetMetasAndTags != null ? asset.Tags.Where(x => currentAssetMetasAndTags.Contains(x.m_oTagMeta.m_sName)).ToList() : new List<Tags>();
            List<RelatedEntities> relatedEntitiesToUpdate = asset.RelatedEntities != null && currentAssetMetasAndTags != null ? asset.RelatedEntities.Where(x => currentAssetMetasAndTags.Contains(x.TagMeta.m_sName)).ToList() : new List<RelatedEntities>();

            result = ValidateMediaAssetMetasAndTagsNamesAndTypes(groupId, catalogGroupCache, metasToUpdate, tagsToUpdate, assetStructMetaIds, ref metasXmlDocToUpdate, ref tagsXmlDocToUpdate,
                                                                    ref assetCatalogStartDate, ref assetFinalEndDate, isFromIngest);
            if (result.Code != (int)eResponseStatus.OK)
            {
                return result;
            }

            result = ValidateRelatedEntitiesLimitaion(relatedEntitiesToAdd, currentAsset.RelatedEntities);
            if (result.Code != (int)eResponseStatus.OK)
            {
                return result;
            }

            result = ValidateRelatedEntities(groupId, catalogGroupCache, assetStructMetaIds, relatedEntitiesToUpdate, ref relatedEntitiesXmlDocToUpdate);
            if (result.Code != (int)eResponseStatus.OK)
            {
                return result;
            }

            // validate device rule id
            if (asset.DeviceRuleId.HasValue && asset.DeviceRuleId.Value != 0)
            {
                if (asset.DeviceRuleId.Value < 0 || !TvmRuleManager.ValidateDeviceRuleExists(groupId, asset.DeviceRuleId.Value))
                {
                    return new Status((int)eResponseStatus.DeviceRuleDoesNotExistForGroup, eResponseStatus.DeviceRuleDoesNotExistForGroup.ToString());
                }
            }

            // validate geoblock rule id
            if (asset.GeoBlockRuleId.HasValue && asset.GeoBlockRuleId.Value != 0)
            {
                if (asset.GeoBlockRuleId.Value < 0 || !TvmRuleManager.ValidateGeoBlockRuleExists(groupId, asset.GeoBlockRuleId.Value))
                {
                    return new Status((int)eResponseStatus.GeoBlockRuleDoesNotExistForGroup, eResponseStatus.GeoBlockRuleDoesNotExistForGroup.ToString());
                }
            }            
           
            return result;
        }

        private static Status ValidateMediaAssetMetasAndTagsNamesAndTypes(int groupId, CatalogGroupCache catalogGroupCache, List<Metas> metas, List<Tags> tags, HashSet<long> assetStructMetaIds,
                                                                          ref XmlDocument metasXmlDoc, ref XmlDocument tagsXmlDoc, ref DateTime? assetCatalogStartDate,
                                                                          ref DateTime? assetFinalEndDate, bool isFromIngest)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            HashSet<string> tempHashSet = new HashSet<string>();
            if (metas != null && metas.Count > 0)
            {
                metasXmlDoc = new XmlDocument();
                XmlNode rootNode = metasXmlDoc.CreateElement("root");
                metasXmlDoc.AppendChild(rootNode);
                foreach (Metas meta in metas)
                {
                    var metaName = meta.m_oTagMeta.m_sName.Trim();

                    // validate duplicates do not exist
                    if (tempHashSet.Contains(metaName))
                    {
                        result.Message = string.Format("Duplicate meta sent, meta name: {0}", metaName);
                        return result;
                    }

                    tempHashSet.Add(metaName);

                    // validate meta exists on group
                    if (!catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(metaName)
                        || !catalogGroupCache.TopicsMapBySystemNameAndByType[metaName].ContainsKey(meta.m_oTagMeta.m_sType))
                    {
                        if (isFromIngest) { continue; }
                        
                        result.Message = string.Format("meta: {0} does not exist for group", metaName);
                        return result;
                    }

                    // validate meta exists on asset struct
                    if (!assetStructMetaIds.Contains(catalogGroupCache.TopicsMapBySystemNameAndByType[metaName][meta.m_oTagMeta.m_sType].Id))
                    {
                        if (isFromIngest) { continue; }

                        result.Message = string.Format("meta: {0} is not part of assetStruct", metaName);
                        return result;
                    }

                    Topic topic = catalogGroupCache.TopicsMapBySystemNameAndByType[metaName][meta.m_oTagMeta.m_sType];
                    // validate correct type was sent
                    if (topic.Type.ToString().ToLower() != meta.m_oTagMeta.m_sType.ToLower())
                    {
                        result = new Status((int)eResponseStatus.InvalidMetaType, string.Format("{0} was sent for meta: {1}", eResponseStatus.InvalidMetaType.ToString(), metaName));
                        return result;
                    }

                    // Validate meta values are correct and add to to metaXml
                    if (!IsMetaValueValid(meta, topic.Id, catalogGroupCache.DefaultLanguage.ID, catalogGroupCache.LanguageMapByCode, ref result, ref metasXmlDoc, ref rootNode,
                                            ref assetCatalogStartDate, ref assetFinalEndDate))
                    {
                        return result;
                    }
                }

                tempHashSet.Clear();
            }

            if (tags != null && tags.Count > 0)
            {
                tagsXmlDoc = new XmlDocument();
                XmlNode rootNode = tagsXmlDoc.CreateElement("root");
                tagsXmlDoc.AppendChild(rootNode);
                foreach (Tags tag in tags)
                {
                    var tagName = tag.m_oTagMeta.m_sName.Trim();

                    // validate duplicates do not exist
                    if (tempHashSet.Contains(tagName))
                    {
                        result.Message = string.Format("Duplicate tag sent, tag name: {0}", tagName);
                        return result;
                    }

                    tempHashSet.Add(tagName);

                    // validate tag exists on group
                    if (!catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(tagName)
                        || !catalogGroupCache.TopicsMapBySystemNameAndByType[tagName].ContainsKey(tag.m_oTagMeta.m_sType))
                    {
                        if (isFromIngest) { continue; }

                        result.Message = string.Format("tag: {0} does not exist for group", tagName);
                        return result;
                    }

                    // validate tag exists on asset struct
                    if (!assetStructMetaIds.Contains(catalogGroupCache.TopicsMapBySystemNameAndByType[tagName][tag.m_oTagMeta.m_sType].Id))
                    {
                        if (isFromIngest) { continue; }

                        result.Message = string.Format("tag: {0} is not part of assetStruct", tagName);
                        return result;
                    }

                    Topic topic = catalogGroupCache.TopicsMapBySystemNameAndByType[tagName][tag.m_oTagMeta.m_sType];
                    // validate correct type was sent
                    if (topic.Type != MetaType.Tag || topic.Type.ToString().ToLower() != tag.m_oTagMeta.m_sType.ToLower())
                    {
                        result = new Status((int)eResponseStatus.InvalidMetaType, string.Format("{0} was sent for meta: {1}", eResponseStatus.InvalidMetaType.ToString(), tagName));
                        return result;
                    }

                    // insert default language values into tagsXml
                    foreach (string tagValue in tag.m_lValues)
                    {
                        AddTopicLanguageValueToXml(tagsXmlDoc, rootNode, topic.Id, catalogGroupCache.DefaultLanguage.ID, tagValue);
                    }
                }
            }

            result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            return result;
        }

        private static Tuple<Dictionary<string, MediaAsset>, bool> GetMediaAssets(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, MediaAsset> result = new Dictionary<string, MediaAsset>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("ids") && funcParams.ContainsKey("isAllowedToViewInactiveAssets") && funcParams.ContainsKey("groupId"))
                {
                    List<long> ids;
                    int? groupId = funcParams["groupId"] as int?;
                    bool? isAllowedToViewInactiveAssets = funcParams["isAllowedToViewInactiveAssets"] as bool?;
                    if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                    {
                        ids = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => long.Parse(x)).ToList();
                    }
                    else
                    {
                        ids = funcParams["ids"] != null ? funcParams["ids"] as List<long> : null;
                    }

                    List<MediaAsset> mediaAssets = new List<MediaAsset>();
                    List<long> missingAssetIds = null;

                    if (ids != null && groupId.HasValue && isAllowedToViewInactiveAssets.HasValue)
                    {
                        CatalogGroupCache catalogGroupCache;
                        if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId.Value, out catalogGroupCache))
                        {
                            log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetMediaAssets", groupId);
                        }
                        else
                        {
                            DataSet ds = CatalogDAL.GetMediaAssets(groupId.Value, ids, catalogGroupCache.DefaultLanguage.ID, isAllowedToViewInactiveAssets.Value);                            
                            mediaAssets = CreateMediaAssets(groupId.Value, ds, catalogGroupCache.DefaultLanguage, catalogGroupCache.LanguageMapById.Values.ToList());

                            if(isAllowedToViewInactiveAssets.Value && mediaAssets?.Count != ids.Count)
                            {
                                if(mediaAssets == null)
                                {
                                    mediaAssets = new List<MediaAsset>();
                                }
                                // get missing asset Ids
                                missingAssetIds = ids.Where(i => !mediaAssets.Any(e => i == e.Id)).ToList();
                            }
                        }

                        res = true;
                    }

                    if (res)
                    {
                        result = mediaAssets.ToDictionary(x => LayeredCacheKeys.GetAssetKey(eAssetTypes.MEDIA.ToString(), x.Id), x => x);

                        if(missingAssetIds?.Count > 0)
                        {
                            foreach (var missingAssetId in missingAssetIds)
                            {
                                result.TryAdd(LayeredCacheKeys.GetAssetKey(eAssetTypes.MEDIA.ToString(), missingAssetId),
                                    new MediaAsset() { Id = missingAssetId, IndexStatus = AssetIndexStatus.Deleted, AssetType = eAssetTypes.MEDIA });

                                log.DebugFormat("Get Deleted MediaAsset {0}, groupId {1}", missingAssetId, groupId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetMediaAssets failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, MediaAsset>, bool>(result, res);
        }      

        private static List<MediaAsset> GetMediaAssetsFromCache(int groupId, List<long> ids, bool isAllowedToViewInactiveAssets)
        {
            List<MediaAsset> mediaAssets = null;
            try
            {
                if (ids == null || ids.Count == 0)
                {
                    return mediaAssets;
                }

                eAssetTypes assetType = eAssetTypes.MEDIA;
                Dictionary<string, MediaAsset> mediaAssetMap = null;
                Dictionary<string, string> keyToOriginalValueMap = LayeredCacheKeys.GetAssetsKeyMap(assetType.ToString(), ids);
                Dictionary<string, List<string>> invalidationKeysMap = LayeredCacheKeys.GetAssetsInvalidationKeysMap(assetType.ToString(), ids);

                if (!LayeredCache.Instance.GetValues<MediaAsset>(keyToOriginalValueMap, ref mediaAssetMap, GetMediaAssets, new Dictionary<string, object>()
                                                              { { "groupId", groupId }, { "ids", ids }, { "isAllowedToViewInactiveAssets", isAllowedToViewInactiveAssets } },
                                                              groupId, LayeredCacheConfigNames.GET_ASSETS_LIST_CACHE_CONFIG_NAME, invalidationKeysMap, true))
                {
                    log.ErrorFormat("Failed getting GetMediaAssetsFromCache from LayeredCache, groupId: {0}, ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty);
                }
                else if (mediaAssetMap != null)
                {
                    mediaAssets = mediaAssetMap.Values.ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetMediaAssetsFromCache with groupId: {0}, ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return mediaAssets;
        }

        private static GenericResponse<Asset> AddMediaAsset(int groupId, ref CatalogGroupCache catalogGroupCache, MediaAsset assetToAdd, bool isLinear, long userId,
                                                            bool isFromIngest = false)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            try
            {
                // validate assetStruct Exists
                AssetStruct assetStruct = null;
                if (assetToAdd.MediaType.m_nTypeID > 0 && catalogGroupCache.AssetStructsMapById.ContainsKey(assetToAdd.MediaType.m_nTypeID))
                {
                    assetStruct = catalogGroupCache.AssetStructsMapById[assetToAdd.MediaType.m_nTypeID];
                }
                else if (!string.IsNullOrEmpty(assetToAdd.MediaType.m_sTypeName) && catalogGroupCache.AssetStructsMapBySystemName.ContainsKey(assetToAdd.MediaType.m_sTypeName))
                {
                    assetStruct = catalogGroupCache.AssetStructsMapBySystemName[assetToAdd.MediaType.m_sTypeName];
                }
                else
                {
                    result.SetStatus(eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString());
                    return result;
                }

                // validate asset
                XmlDocument metasXmlDoc = null, tagsXmlDoc = null, relatedEntitiesXmlDoc = null;
                DateTime? assetCatalogStartDate = null, assetFinalEndDate = null;

                if (!assetToAdd.InheritancePolicy.HasValue)
                {
                    assetToAdd.InheritancePolicy = AssetInheritancePolicy.Enable;
                }

                Status validateAssetTopicsResult = ValidateMediaAssetForInsert(groupId, catalogGroupCache, ref assetStruct, assetToAdd, ref metasXmlDoc, ref tagsXmlDoc,
                                                                                ref assetCatalogStartDate, ref assetFinalEndDate, ref relatedEntitiesXmlDoc, isFromIngest);

                if (validateAssetTopicsResult.Code != (int)eResponseStatus.OK)
                {
                    result.SetStatus(validateAssetTopicsResult);
                    return result;
                }

                // Update asset catalogStartDate and finalEndDate
                assetToAdd.CatalogStartDate = assetCatalogStartDate;
                assetToAdd.FinalEndDate = assetFinalEndDate ?? assetToAdd.FinalEndDate;

                // Add Name meta values (for languages that are not default)
                ExtractBasicTopicLanguageAndValuesFromMediaAsset(assetToAdd, catalogGroupCache, ref metasXmlDoc, NAME_META_SYSTEM_NAME);

                // Add Description meta values (for languages that are not default)
                ExtractBasicTopicLanguageAndValuesFromMediaAsset(assetToAdd, catalogGroupCache, ref metasXmlDoc, DESCRIPTION_META_SYSTEM_NAME);

                DateTime startDate = assetToAdd.StartDate ?? DateTime.UtcNow;
                DateTime catalogStartDate = assetToAdd.CatalogStartDate ?? startDate;
                DateTime endDate = assetToAdd.EndDate ?? DateTime.MaxValue;
                DataSet ds = CatalogDAL.InsertMediaAsset(groupId, catalogGroupCache.DefaultLanguage.ID, metasXmlDoc, tagsXmlDoc, assetToAdd.CoGuid,
                                                        assetToAdd.EntryId, assetToAdd.DeviceRuleId, assetToAdd.GeoBlockRuleId, assetToAdd.IsActive,
                                                        startDate, endDate, catalogStartDate, assetToAdd.FinalEndDate, assetStruct.Id, userId, (int)assetToAdd.InheritancePolicy,
                                                        relatedEntitiesXmlDoc);

                Dictionary<string, DataTable> tables = null;
                Status status  = BuildTableDicAfterInsertMediaAsset(ds, out tables);
                if( status.Code != (int)eResponseStatus.OK)
                {
                    result.SetStatus(status);
                    return result;
                }
                result = CreateMediaAssetResponseFromDataSet(groupId, tables, catalogGroupCache.DefaultLanguage, catalogGroupCache.LanguageMapById.Values.ToList());

                if (result.HasObject() && result.Object.Id > 0 && !isLinear)
                {
                    // UpdateIndex
                    if (!isFromIngest)
                    {
                        bool indexingResult = IndexManager.UpsertMedia(groupId, (int)result.Object.Id);
                        if (!indexingResult)

                        {
                            log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after AddMediaAsset", result.Object.Id, groupId);
                        }

                        if (assetToAdd.IsActive.HasValue && assetToAdd.IsActive.Value)
                        {
                            Notification.Module.AddFollowNotificationRequestForOpc(groupId, (MediaAsset)result.Object, userId, catalogGroupCache);
                        }
                    }
                    
                    CatalogManager.UpdateChildAssetsMetaInherited(groupId, catalogGroupCache, userId, assetStruct, assetToAdd, null);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddMediaAsset for groupId: {0} and asset: {1}", groupId, assetToAdd.ToString()), ex);
            }

            return result;
        }       

        private static Status CreateAssetResponseStatusFromResult(long result)
        {
            Status responseStatus = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            switch (result)
            {
                case -111:
                    responseStatus = new Status((int)eResponseStatus.AssetExternalIdMustBeUnique, eResponseStatus.AssetExternalIdMustBeUnique.ToString());
                    break;
                default:
                    break;
            }

            return responseStatus;
        }
        
        private static bool IsMetaValueValid(Metas meta, long topicId, int defaultLanguageId, Dictionary<string, LanguageObj> LanguageMapByCode, ref Status resultStatus,
                                             ref XmlDocument metasXmlDoc, ref XmlNode rootNode, ref DateTime? assetCatalogStartDate, ref DateTime? assetFinalEndDate)
        {
            // Validate meta values are correct
            MetaType metaType;
            bool isValidMeta = false;
            bool isValidMetaValue = false;
            bool isMultilingualStringValue = false;
            if (!Enum.TryParse<MetaType>(meta.m_oTagMeta.m_sType, out metaType))
            {
                resultStatus = new Status((int)eResponseStatus.InvalidMetaType, string.Format("{0} was sent for meta: {1}", eResponseStatus.InvalidMetaType.ToString(), meta.m_oTagMeta.m_sName));
                return false;
            }

            switch (metaType)
            {
                case MetaType.String:
                    isValidMeta = true;
                    isValidMetaValue = true;
                    break;
                case MetaType.MultilingualString:
                    isValidMeta = true;
                    isValidMetaValue = true;
                    isMultilingualStringValue = true;
                    break;
                case MetaType.Number:
                    isValidMeta = true;
                    double doubleVal;
                    isValidMetaValue = double.TryParse(meta.m_sValue, out doubleVal);
                    break;
                case MetaType.Bool:
                    isValidMeta = true;
                    bool boolVal;
                    isValidMetaValue = BoolUtils.TryConvert(meta.m_sValue, out boolVal);
                    break;
                case MetaType.DateTime:
                    isValidMeta = true;
                    DateTime dateTimeVal;
                    isValidMetaValue = DateTime.TryParse(meta.m_sValue, out dateTimeVal);
                    break;                
                default:
                case MetaType.All:
                case MetaType.Tag:
                case MetaType.ReleatedEntity:
                    break;
            }

            if (!isValidMeta)
            {
                resultStatus = new Status((int)eResponseStatus.InvalidMetaType, string.Format("{0} was sent for meta: {1}", eResponseStatus.InvalidMetaType.ToString(), meta.m_oTagMeta.m_sName));
                return false;
            }

            if (!isValidMetaValue)
            {
                log.ErrorFormat("IsMetaValueValid-InvalidValueSentForMeta. metaName: {0}, metaType:{1}, metaValue:{2}, topicId:{3}.", 
                                meta.m_oTagMeta.m_sName, metaType, meta.m_sValue, topicId);
                resultStatus = new Status((int)eResponseStatus.InvalidValueSentForMeta, 
                                          string.Format("{0} metaName: {1}", eResponseStatus.InvalidValueSentForMeta.ToString(), meta.m_oTagMeta.m_sName));
                return false;
            }

            if (BasicMetasSystemNamesToType.ContainsKey(meta.m_oTagMeta.m_sName))
            {
                switch (meta.m_oTagMeta.m_sName)
                {
                    case CATALOG_START_DATE_TIME_META_SYSTEM_NAME:
                        DateTime catalogStartDate;
                        if (DateTime.TryParse(meta.m_sValue, out catalogStartDate))
                        {
                            assetCatalogStartDate = catalogStartDate;
                        }
                        else
                        {
                            log.ErrorFormat("IsMetaValueValid failed to parse {0} meta, value: {1}", CATALOG_START_DATE_TIME_META_SYSTEM_NAME, meta.m_sValue);
                        }
                        break;
                    case PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME:
                        DateTime finalEndDate;
                        if (DateTime.TryParse(meta.m_sValue, out finalEndDate))
                        {
                            assetFinalEndDate = finalEndDate;
                        }
                        else
                        {
                            log.ErrorFormat("IsMetaValueValid failed to parse {0} meta, value: {1}", CATALOG_END_DATE_TIME_META_SYSTEM_NAME, meta.m_sValue);
                        }
                        break;
                    default:
                        log.WarnFormat("IsMetaValueValid found basic meta that isn't on switch case, meta name: {0}", meta.m_oTagMeta.m_sName);
                        break;
                }
            }
            else
            {
                AddTopicLanguageValueToXml(metasXmlDoc, rootNode, topicId, defaultLanguageId, meta.m_sValue);
            }

            if (isMultilingualStringValue)
            {
                foreach (LanguageContainer language in meta.Value)
                {
                    if (LanguageMapByCode.ContainsKey(language.m_sLanguageCode3))
                    {
                        AddTopicLanguageValueToXml(metasXmlDoc, rootNode, topicId, LanguageMapByCode[language.m_sLanguageCode3].ID, language.m_sValue);
                    }
                }
            }

            return true;
        }

        private static void AddTopicLanguageValueToXml(XmlDocument metasXmlDoc, XmlNode rootNode, long topicId, int languageId, string value)
        {
            if (value != null)
            {
                XmlNode rowNode;
                XmlNode topicIdNode;
                XmlNode languageIdNode;
                XmlNode valueNode;
                rowNode = metasXmlDoc.CreateElement("row");
                topicIdNode = metasXmlDoc.CreateElement("topic_id");
                topicIdNode.InnerText = topicId.ToString();
                rowNode.AppendChild(topicIdNode);
                languageIdNode = metasXmlDoc.CreateElement("language_id");
                languageIdNode.InnerText = languageId.ToString();
                rowNode.AppendChild(languageIdNode);
                valueNode = metasXmlDoc.CreateElement("value");
                valueNode.InnerText = value;
                rowNode.AppendChild(valueNode);
                rootNode.AppendChild(rowNode);
            }
        }

        private static bool ExtractMediaAssetNamesAndDescriptionsFromMetas(List<Metas> metas, ref string name, ref string description, ref List<LanguageContainer> namesWithLanguages,
                                                                           ref List<LanguageContainer> descriptionsWithLanguages)
        {
            if (metas != null && metas.Count > 0)
            {
                Metas nameMeta = metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower() == NAME_META_SYSTEM_NAME.ToLower());
                if (nameMeta != null && !string.IsNullOrEmpty(nameMeta.m_sValue))
                {
                    name = nameMeta.m_sValue;
                    if (nameMeta.Value != null && nameMeta.Value.Length > 0)
                    {
                        namesWithLanguages = new List<LanguageContainer>(nameMeta.Value);
                    }

                    metas.Remove(nameMeta);
                }
                else
                {
                    return false;
                }

                Metas descMeta = metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower() == DESCRIPTION_META_SYSTEM_NAME.ToLower());
                if (descMeta != null)
                {
                    description = descMeta.m_sValue;
                    if (descMeta.Value != null && descMeta.Value.Length > 0)
                    {
                        descriptionsWithLanguages = new List<LanguageContainer>(descMeta.Value);
                    }

                    metas.Remove(descMeta);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private static void ExtractBasicTopicLanguageAndValuesFromMediaAsset(MediaAsset asset, CatalogGroupCache catalogGroupCache, ref XmlDocument xmlDoc, string basicTopicSystemName)
        {
            // Add Name meta values (for languages that are not default)
            if (catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(basicTopicSystemName) && BasicMetasSystemNamesToType.ContainsKey(basicTopicSystemName))
            {
                Topic topic = catalogGroupCache.TopicsMapBySystemNameAndByType[basicTopicSystemName][BasicMetasSystemNamesToType[basicTopicSystemName]];
                XmlNode rootNode;
                if (xmlDoc == null)
                {
                    xmlDoc = new XmlDocument();
                    rootNode = xmlDoc.CreateElement("root");
                    xmlDoc.AppendChild(rootNode);
                }
                else
                {
                    rootNode = xmlDoc.FirstChild;
                }

                if (basicTopicSystemName == NAME_META_SYSTEM_NAME && !string.IsNullOrEmpty(asset.Name))
                {
                    AddTopicLanguageValueToXml(xmlDoc, rootNode, topic.Id, catalogGroupCache.DefaultLanguage.ID, asset.Name);
                    if (asset.NamesWithLanguages != null && asset.NamesWithLanguages.Count > 0)
                    {
                        foreach (LanguageContainer language in asset.NamesWithLanguages)
                        {
                            if (catalogGroupCache.LanguageMapByCode.ContainsKey(language.m_sLanguageCode3))
                            {
                                AddTopicLanguageValueToXml(xmlDoc, rootNode, topic.Id, catalogGroupCache.LanguageMapByCode[language.m_sLanguageCode3].ID, language.m_sValue);
                            }
                        }
                    }
                }
                else if (basicTopicSystemName == DESCRIPTION_META_SYSTEM_NAME && asset.Description != null)
                {
                    AddTopicLanguageValueToXml(xmlDoc, rootNode, topic.Id, catalogGroupCache.DefaultLanguage.ID, asset.Description);
                    if (asset.DescriptionsWithLanguages != null && asset.DescriptionsWithLanguages.Count > 0)
                    {
                        foreach (LanguageContainer language in asset.DescriptionsWithLanguages)
                        {
                            if (catalogGroupCache.LanguageMapByCode.ContainsKey(language.m_sLanguageCode3))
                            {
                                AddTopicLanguageValueToXml(xmlDoc, rootNode, topic.Id, catalogGroupCache.LanguageMapByCode[language.m_sLanguageCode3].ID, language.m_sValue);
                            }
                        }
                    }
                }
            }
        }

        private static List<Asset> GetNpvrAssetsFromCache(int groupId, List<long> ids)
        {
            log.ErrorFormat("Opc account doesn't support recordings at the moment");
            throw new NotImplementedException();
        }

        private static List<Asset> GetAssetsFromCache(int groupId, List<KeyValuePair<eAssetTypes, long>> assets, bool isAllowedToViewInactiveAssets)
        {
            List<Asset> result = null;
            try
            {
                if (assets != null && assets.Count > 0)
                {
                    result = new List<Asset>();
                    List<long> mediaIds = assets.Where(x => x.Key == eAssetTypes.MEDIA).Select(x => x.Value).Distinct().ToList();
                    List<long> epgIds = assets.Where(x => x.Key == eAssetTypes.EPG).Select(x => x.Value).Distinct().ToList();
                    List<long> npvrIds = assets.Where(x => x.Key == eAssetTypes.NPVR).Select(x => x.Value).Distinct().ToList();
                    if (mediaIds != null && mediaIds.Count > 0)
                    {
                        List<MediaAsset> mediaAssets = GetMediaAssetsFromCache(groupId, mediaIds, isAllowedToViewInactiveAssets);
                        if (mediaAssets == null || mediaAssets.Count != mediaIds.Count)
                        {
                            List<long> missingMediaIds = mediaAssets == null ? mediaIds : mediaIds.Except(mediaAssets.Select(x => x.Id)).ToList();
                            log.WarnFormat("GetMediaAssetsFromCache didn't find the following mediaIds: {0}", string.Join(",", missingMediaIds));
                        }
                        else if (mediaAssets != null)
                        {
                            result.AddRange(mediaAssets);
                        }
                    }

                    if (epgIds != null && epgIds.Count > 0)
                    {
                        var epgAssetsFromCache = EpgAssetManager.GetEpgAssetsFromCache(epgIds, groupId, new List<string>() { "*" });
                        if (epgAssetsFromCache != null && epgAssetsFromCache.Count > 0)
                        {
                            result.AddRange(epgAssetsFromCache);
                        }
                    }

                    if (npvrIds != null && npvrIds.Count > 0)
                    {
                        var npvrAssetsFromCache = GetNpvrAssetsFromCache(groupId, npvrIds);
                        if (npvrAssetsFromCache != null)
                        {
                            result.AddRange(npvrAssetsFromCache);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetsFromCache with groupId: {0}, assets: {1}", groupId,
                                        assets != null ? string.Join(",", assets.Select(x => string.Format("{0}_{1}", x.Key, x.Value)).ToList()) : string.Empty), ex);
            }

            return result;
        }

        private static GenericResponse<Asset> UpdateMediaAsset(int groupId, ref CatalogGroupCache catalogGroupCache, MediaAsset currentAsset, MediaAsset assetToUpdate, bool isLinear,
                                                                long userId, bool isFromIngest = false, bool isForMigration = false , bool isFromChannel = false)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            Status status = null;
            try
            {
                if (!isForMigration)
                {
                    status = AssetUserRuleManager.CheckAssetUserRuleList(groupId, userId, currentAsset.Id);
                    if (status == null || status.Code == (int)eResponseStatus.ActionIsNotAllowed)
                    {
                        result.SetStatus(eResponseStatus.ActionIsNotAllowed, ACTION_IS_NOT_ALLOWED);
                        return result;
                    }
                }

                // validate asset
                XmlDocument metasXmlDocToAdd = null, tagsXmlDocToAdd = null, metasXmlDocToUpdate = null, tagsXmlDocToUpdate = null;
                XmlDocument relatedEntitiesXmlDocToAdd = null, relatedEntitiesXmlDocToUpdate = null;
                
                AssetStruct assetStruct = null;
                DateTime? assetCatalogStartDate = null, assetFinalEndDate = null;
                if (currentAsset.MediaType.m_nTypeID > 0 && catalogGroupCache.AssetStructsMapById.ContainsKey(currentAsset.MediaType.m_nTypeID))
                {
                    assetStruct = catalogGroupCache.AssetStructsMapById[currentAsset.MediaType.m_nTypeID];
                }

                HashSet<string> currentAssetMetasAndTags = new HashSet<string>(currentAsset.Metas.Select(x => x.m_oTagMeta.m_sName), StringComparer.OrdinalIgnoreCase);
                currentAssetMetasAndTags.UnionWith(currentAsset.Tags.Select(x => x.m_oTagMeta.m_sName));
                if (currentAsset.RelatedEntities != null)
                {
                    currentAssetMetasAndTags.UnionWith(currentAsset.RelatedEntities.Select(x => x.TagMeta.m_sName));
                }

                Status validateAssetTopicsResult = ValidateMediaAssetForUpdate(groupId, catalogGroupCache, ref assetStruct, assetToUpdate, currentAssetMetasAndTags, ref metasXmlDocToAdd,
                                                        ref tagsXmlDocToAdd, ref metasXmlDocToUpdate, ref tagsXmlDocToUpdate, ref assetCatalogStartDate, 
                                                        ref assetFinalEndDate, ref relatedEntitiesXmlDocToAdd, ref relatedEntitiesXmlDocToUpdate, currentAsset, isFromIngest);
                if (validateAssetTopicsResult.Code != (int)eResponseStatus.OK)
                {
                    result.SetStatus(validateAssetTopicsResult);
                    return result;
                }

                // Update asset catalogStartDate and finalEndDate
                assetToUpdate.CatalogStartDate = assetCatalogStartDate ?? assetToUpdate.CatalogStartDate;
                assetToUpdate.FinalEndDate = assetFinalEndDate ?? assetToUpdate.FinalEndDate;

                // Add Name meta values (for languages that are not default), Name can only be updated
                if (string.IsNullOrEmpty(currentAsset.Name))
                {
                    ExtractBasicTopicLanguageAndValuesFromMediaAsset(assetToUpdate, catalogGroupCache, ref metasXmlDocToAdd, NAME_META_SYSTEM_NAME);
                }
                else
                {
                    ExtractBasicTopicLanguageAndValuesFromMediaAsset(assetToUpdate, catalogGroupCache, ref metasXmlDocToUpdate, NAME_META_SYSTEM_NAME);
                }
                
                // Add Description meta values (for languages that are not default), Description can be updated or added
                if (currentAsset.Description == null && !string.IsNullOrEmpty(assetToUpdate.Description))
                {
                    ExtractBasicTopicLanguageAndValuesFromMediaAsset(assetToUpdate, catalogGroupCache, ref metasXmlDocToAdd, DESCRIPTION_META_SYSTEM_NAME);
                }
                else if (currentAsset.Description != null)
                {
                    ExtractBasicTopicLanguageAndValuesFromMediaAsset(assetToUpdate, catalogGroupCache, ref metasXmlDocToUpdate, DESCRIPTION_META_SYSTEM_NAME);
                }

                DateTime startDate = assetToUpdate.StartDate ?? (currentAsset.StartDate ?? DateTime.UtcNow);
                DateTime catalogStartDate = assetToUpdate.CatalogStartDate ?? (currentAsset.CatalogStartDate ?? DateTime.UtcNow);
                DateTime endDate = assetToUpdate.EndDate ?? (currentAsset.EndDate ?? DateTime.MaxValue);

                AssetInheritancePolicy inheritancePolicy = assetToUpdate.InheritancePolicy ?? (currentAsset.InheritancePolicy ?? AssetInheritancePolicy.Enable);
                
                // TODO - Lior. Need to extract all values from tags that are part of the mediaObj properties (Basic metas)
                DataSet ds = CatalogDAL.UpdateMediaAsset(groupId, assetToUpdate.Id, catalogGroupCache.DefaultLanguage.ID, metasXmlDocToAdd, tagsXmlDocToAdd, metasXmlDocToUpdate, tagsXmlDocToUpdate,
                                                        assetToUpdate.CoGuid, assetToUpdate.EntryId, assetToUpdate.DeviceRuleId, assetToUpdate.GeoBlockRuleId, assetToUpdate.IsActive, startDate,
                                                        endDate, catalogStartDate, assetToUpdate.FinalEndDate, userId, (int)inheritancePolicy, relatedEntitiesXmlDocToAdd, relatedEntitiesXmlDocToUpdate);

                Dictionary<string, DataTable> tables = null;
                status = BuildTableDicAfterUpdateMediaAsset(ds, assetToUpdate.Id, out tables);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    result.SetStatus(status);
                    return result;
                }

                result = CreateMediaAssetResponseFromDataSet(groupId, tables, catalogGroupCache.DefaultLanguage, catalogGroupCache.LanguageMapById.Values.ToList());
                if (!isForMigration && result != null && result.HasObject() && result.Object.Id > 0 && !isLinear)
                {
                    if (assetStruct.ParentId.HasValue && assetStruct.ParentId.Value > 0)
                    {
                        DataSet updateDS = UpdateAssetInheritancePolicy(groupId, userId, catalogGroupCache, assetStruct, inheritancePolicy, result.Object);
                        
                        if (updateDS != null)
                        {
                            status = BuildTableDicAfterUpdateMediaAsset(updateDS, assetToUpdate.Id, out tables);
                            if (!status.IsOkStatusCode())
                            {
                                result.SetStatus(status);
                                return result;
                            }
                            result = CreateMediaAssetResponseFromDataSet(groupId, tables, catalogGroupCache.DefaultLanguage, catalogGroupCache.LanguageMapById.Values.ToList());
                        }
                    }

                    // UpdateIndex
                    if (!isFromIngest)
                    {
                        if (!isFromChannel && (assetStruct.SystemName == MANUAL_ASSET_STRUCT_NAME || assetStruct.SystemName == DYNAMIC_ASSET_STRUCT_NAME || assetStruct.SystemName == EXTERNAL_ASSET_STRUCT_NAME))
                        {
                            if (assetStruct.TopicsMapBySystemName == null || assetStruct.TopicsMapBySystemName.Count == 0)
                            {
                                if (CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                                {
                                    assetStruct.TopicsMapBySystemName = catalogGroupCache.TopicsMapById.Where(x => assetStruct.MetaIds.Contains(x.Key))
                                                                      .OrderBy(x => assetStruct.MetaIds.IndexOf(x.Key))
                                                                      .ToDictionary(x => x.Value.SystemName, y => y.Value);
                                }
                            }

                            if (assetStruct.SystemName == EXTERNAL_ASSET_STRUCT_NAME)
                            {
                                UpdateExternalChannel(groupId, userId, result.Object as MediaAsset, assetStruct);
                            }
                            else
                            {
                                UpdateChannel(groupId, userId, result.Object as MediaAsset, assetStruct);
                            }
                        }

                        bool indexingResult = IndexManager.UpsertMedia(groupId, (int)result.Object.Id);
                        if (!indexingResult)
                        {
                            log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after UpdateMediaAsset", result.Object.Id, groupId);
                        }
                        else if (Core.Api.Managers.AssetRuleManager.IsGeoAssetRulesEnabled(groupId))
                        {
                            Catalog.Module.UpdateIndex(new List<int>() { (int)result.Object.Id }, groupId, eAction.GeoUpdate);
                        }
                    }

                    // update meta inherited
                    CatalogManager.UpdateChildAssetsMetaInherited(groupId, catalogGroupCache, userId, assetStruct, result.Object, currentAsset);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateAsset for groupId: {0} and asset: {1}", groupId, assetToUpdate.ToString()), ex);
            }

            return result;
        }       

        private static void UpdateAssetInheritancePolicy(int groupId, long userId, CatalogGroupCache catalogGroupCache, MediaAsset mediaAsset)
        {
            AssetStruct assetStruct = catalogGroupCache.AssetStructsMapById[mediaAsset.MediaType.m_nTypeID];
            if (assetStruct == null)
            {
                log.ErrorFormat("failed to get assetStruct {0} for groupId: {1} when calling UpdateAssetInheritancePolicy", mediaAsset.MediaType.m_nTypeID, groupId);
                return;
            }

            if (assetStruct.ParentId.HasValue && assetStruct.ParentId.Value > 0)
            {
                AssetInheritancePolicy assetInheritancePolicy = mediaAsset.InheritancePolicy ?? AssetInheritancePolicy.Enable;
                UpdateAssetInheritancePolicy(groupId, userId, catalogGroupCache, assetStruct, assetInheritancePolicy, mediaAsset);
            }
        }

        private static DataSet UpdateAssetInheritancePolicy(int groupId, long userId, CatalogGroupCache catalogGroupCache, AssetStruct assetStruct, AssetInheritancePolicy inheritancePolicy, Asset asset)
        {
            DataSet ds = null;
            if (inheritancePolicy == AssetInheritancePolicy.Enable)
            {
                var inherited = assetStruct.AssetStructMetas.Where(x => x.Value.IsInherited.HasValue && x.Value.IsInherited.Value).ToList();
                if (inherited != null && inherited.Count > 0)
                {
                    Asset parentAsset = GetParentAsset(groupId, catalogGroupCache, assetStruct, asset);
                    if (parentAsset != null)
                    {
                        foreach (var kvp in inherited)
                        {
                            Topic topic = catalogGroupCache.TopicsMapById[kvp.Value.MetaId];
                            if (topic.Type == MetaType.Tag)
                            {
                                Tags tag = asset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));
                                Tags parentTag = parentAsset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));

                                if (tag == null || (tag != null && !tag.Equals(parentTag)))
                                {
                                    inheritancePolicy = AssetInheritancePolicy.Disable;
                                    break;
                                }
                            }
                            else
                            {
                                Metas meta = asset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));
                                Metas parentMeta = parentAsset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));

                                if (meta == null || (meta != null && !meta.Equals(parentMeta)))
                                {
                                    inheritancePolicy = AssetInheritancePolicy.Disable;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (inheritancePolicy == AssetInheritancePolicy.Disable)
                {
                    //update inheritancePolicy
                    ds = CatalogDAL.UpdateMediaAsset(groupId, asset.Id, catalogGroupCache.DefaultLanguage.ID, null, null, null, null, null, null, null, null, null, null,
                                                       null, null, null, userId, (int)inheritancePolicy, null, null);
                }
            }

            return ds;
        }

        private static bool TryGetMediaTypeFromAssetStructId(int groupId, long assetStructId, out MediaType mediaType)
        {
            bool res = false;
            mediaType = null;
            CatalogGroupCache catalogGroupCache;
            if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetAssetStructsByIds", groupId);
                return res;
            }

            if (!catalogGroupCache.AssetStructsMapById.ContainsKey(assetStructId))
            {
                log.WarnFormat("assetStructId: {0} doesn't exist for groupId: {1}", assetStructId, groupId);
                return res;
            }

            mediaType = new MediaType(catalogGroupCache.AssetStructsMapById[assetStructId].SystemName, (int)assetStructId);
            return true;
        }

        private static bool TryGetMetasAndTags(int groupId, long mediaId, int defaultLanguageId, List<LanguageObj> groupLanguages, DataTable metasTable, DataTable tagsTable,
                                                ref List<Metas> metas, ref List<Tags> tags)
        {
            bool res = false;
            Dictionary<long, List<LanguageContainer>> topicIdToMeta = new Dictionary<long, List<LanguageContainer>>();
            Dictionary<long, LanguageObj> languagesDictionary = new Dictionary<long, LanguageObj>();

            foreach (var language in groupLanguages)
            {
                languagesDictionary[language.ID] = language;
            }

            foreach (DataRow dr in metasTable.Rows)
            {
                long topicId = ODBCWrapper.Utils.GetLongSafeVal(dr, "topic_id");
                long languageId = ODBCWrapper.Utils.GetLongSafeVal(dr, "language_id");
                string translation = ODBCWrapper.Utils.GetSafeStr(dr, "translation");

                if (languagesDictionary.ContainsKey(languageId))
                {
                    LanguageObj language = languagesDictionary[languageId];

                    if (!topicIdToMeta.ContainsKey(topicId))
                    {
                        topicIdToMeta.Add(topicId, new List<LanguageContainer>() { new LanguageContainer(language.Code, translation, language.IsDefault) });
                    }
                    else
                    {
                        topicIdToMeta[topicId].Add(new LanguageContainer(language.Code, translation, language.IsDefault));
                    }
                }
            }

            // TODO Lior - Remove TagId, not needed
            Dictionary<long, Dictionary<long, List<LanguageContainer>>> topicIdToTag = new Dictionary<long, Dictionary<long, List<LanguageContainer>>>();
            foreach (DataRow dr in tagsTable.Rows)
            {
                long topicId = ODBCWrapper.Utils.GetLongSafeVal(dr, "topic_id");
                long tagId = ODBCWrapper.Utils.GetLongSafeVal(dr, "tag_id");
                long languageId = ODBCWrapper.Utils.GetLongSafeVal(dr, "language_id");
                string translation = ODBCWrapper.Utils.GetSafeStr(dr, "translation");

                if (languagesDictionary.ContainsKey(languageId))
                {
                    LanguageObj language = languagesDictionary[languageId];

                    if (!topicIdToTag.ContainsKey(topicId))
                    {
                        topicIdToTag.Add(topicId, new Dictionary<long, List<LanguageContainer>>());
                        topicIdToTag[topicId].Add(tagId, new List<LanguageContainer>() { new LanguageContainer(language.Code, translation, language.IsDefault) });
                    }
                    else if (!topicIdToTag[topicId].ContainsKey(tagId))
                    {
                        topicIdToTag[topicId].Add(tagId, new List<LanguageContainer>() { new LanguageContainer(language.Code, translation, language.IsDefault) });
                    }
                    else
                    {
                        topicIdToTag[topicId][tagId].Add(new LanguageContainer(language.Code, translation, language.IsDefault));
                    }
                }
            }

            List<long> topicIds = new List<long>();
            topicIds.AddRange(topicIdToMeta.Keys.ToList());
            topicIds.AddRange(topicIdToTag.Keys.ToList());
            if (topicIds.Count > 0)
            {
                GenericListResponse<Topic> groupTopicsResponse = CatalogManager.GetTopicsByIds(groupId, topicIds, MetaType.All);
                if (groupTopicsResponse != null && groupTopicsResponse.Status != null && groupTopicsResponse.Status.Code == (int)eResponseStatus.OK
                    && groupTopicsResponse.Objects != null && groupTopicsResponse.Objects.Count > 0)
                {
                    metas = new List<Metas>();
                    tags = new List<Tags>();
                    foreach (Topic topic in groupTopicsResponse.Objects)
                    {
                        if (topic.Type == MetaType.Tag)
                        {
                            if (topicIdToTag.ContainsKey(topic.Id))
                            {
                                Dictionary<long, List<LanguageContainer>> topicTags = topicIdToTag[topic.Id];
                                List<LanguageContainer> defaultLanguageValues = topicTags.SelectMany(x => x.Value.Where(y => y.IsDefault)).ToList();
                                List<string> defaultValues = defaultLanguageValues.Select(x => x.m_sValue).ToList();
                                List<LanguageContainer[]> tagLanguages = topicIdToTag[topic.Id].Select(x => x.Value.Select(y => y).ToArray()).ToList();
                                tags.Add(new Tags(new TagMeta(topic.SystemName, topic.Type.ToString()), defaultValues, tagLanguages));
                            }
                        }
                        else
                        {
                            if (topicIdToMeta.ContainsKey(topic.Id))
                            {
                                IEnumerable<LanguageContainer> topicLanguages = null;
                                string defaultValue = topicIdToMeta[topic.Id].FirstOrDefault(x => x.IsDefault).m_sValue;
                                if (topic.Type == MetaType.MultilingualString)
                                {
                                    topicLanguages = topicIdToMeta[topic.Id].Where(x => !x.IsDefault).Select(x => x);
                                }

                                metas.Add(new Metas(new TagMeta(topic.SystemName, topic.Type.ToString()), defaultValue, topicLanguages));
                            }
                        }
                    }

                    res = true;
                }
            }

            return res;
        }

        private static Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> CreateGroupMediaMapFromDataSet(int groupId, DataSet ds, CatalogGroupCache catalogGroupCache)
        {
            // <assetId, <languageId, media>>
            Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> groupAssetsMap = new Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>>();

            try
            {
                if (ds == null || ds.Tables == null || ds.Tables.Count < 6)
                {
                    log.WarnFormat("CreateGroupMediaMapFromDataSet didn't receive dataset with 6 or more tables");
                    return null;
                }

                // Basic details table
                if (ds.Tables[0] == null || ds.Tables[0].Rows == null || ds.Tables[0].Rows.Count <= 0)
                {
                    log.WarnFormat("CreateGroupMediaMapFromDataSet - basic details table is not valid");
                    return null;
                }

                EnumerableRowCollection<DataRow> metas = new DataTable().AsEnumerable();
                // metas table
                if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                {
                    metas = ds.Tables[1].AsEnumerable();
                }

                EnumerableRowCollection<DataRow> tags = new DataTable().AsEnumerable();
                // tags table
                if (ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
                {
                    tags = ds.Tables[2].AsEnumerable();
                }

                EnumerableRowCollection<DataRow> fileTypes = new DataTable().AsEnumerable();
                // file types table
                if (ds.Tables[3] != null && ds.Tables[3].Rows != null && ds.Tables[3].Rows.Count > 0)
                {
                    fileTypes = ds.Tables[3].AsEnumerable();
                }

                // images table is returned empty

                EnumerableRowCollection<DataRow> assetUpdateDate = new DataTable().AsEnumerable();
                // update dates table
                if (ds.Tables[5] != null && ds.Tables[5].Rows != null && ds.Tables[5].Rows.Count > 0)
                {
                    assetUpdateDate = ds.Tables[5].AsEnumerable();
                }

                Dictionary<int, List<DataRow>> geoAvailability = new Dictionary<int, List<DataRow>>();
                // Geo Availability
                if (ds.Tables.Count > 6 && ds.Tables[6] != null && ds.Tables[6].Rows != null && ds.Tables[6].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[6].Rows)
                    {
                        int mediaId = ODBCWrapper.Utils.GetIntSafeVal(row, "MEDIA_ID");
                        int countryId = ODBCWrapper.Utils.GetIntSafeVal(row, "COUNTRY_ID");
                        if (mediaId > 0 && countryId > 0)
                        {
                            if (!geoAvailability.ContainsKey(mediaId))
                            {
                                geoAvailability.Add(mediaId, new List<DataRow>());
                            }
                            geoAvailability[mediaId].Add(row);
                        }
                    }
                }

                Dictionary<long, List<int>> linearChannelsRegionsMapping = null;
                if (catalogGroupCache.IsRegionalizationEnabled)
                {
                    linearChannelsRegionsMapping = CatalogManager.GetLinearMediaRegions(groupId);
                }

                foreach (DataRow basicDataRow in ds.Tables[0].Rows)
                {
                    int id = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "ID", 0);

                    try
                    {
                        if (id > 0 && !groupAssetsMap.ContainsKey(id))
                        {
                            Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();
                            DataTable basicDataTable = ds.Tables[0].Clone();
                            basicDataTable.ImportRow(basicDataRow);
                            tables.Add(TABLE_NAME_BASIC, basicDataTable);
                            if (metas.Any())
                            {
                                EnumerableRowCollection<DataRow> assetMetas = (from row in metas
                                                                               where (Int64)row["ASSET_ID"] == id
                                                                               select row);
                                if (assetMetas != null && assetMetas.Any())
                                {
                                    tables.Add(TABLE_NAME_METAS, assetMetas.CopyToDataTable());
                                }
                                else
                                {
                                    tables.Add(TABLE_NAME_METAS, ds.Tables[1].Clone());
                                }
                            }

                            if (tags.Any())
                            {
                                EnumerableRowCollection<DataRow> assetTags = (from row in tags
                                                                              where (Int64)row["ASSET_ID"] == id
                                                                              select row);
                                if (assetTags != null && assetTags.Any())
                                {
                                    tables.Add(TABLE_NAME_TAGS, assetTags.CopyToDataTable());
                                }
                                else
                                {
                                    tables.Add(TABLE_NAME_TAGS, ds.Tables[2].Clone());
                                }
                            }                          

                            if (assetUpdateDate.Any())
                            {
                                EnumerableRowCollection<DataRow> assetUpdateDateRow = (from row in assetUpdateDate
                                                                                       where (Int64)row["ID"] == id
                                                                                       select row);
                                if (assetUpdateDateRow != null && assetUpdateDateRow.Any())
                                {
                                    tables.Add(TABLE_NAME_UPDATE_DATE, assetUpdateDateRow.CopyToDataTable());
                                }
                                else
                                {
                                    tables.Add(TABLE_NAME_UPDATE_DATE, ds.Tables[5].Clone());
                                }
                            }                            

                            MediaAsset mediaAsset = CreateMediaAsset(groupId, id, tables, catalogGroupCache.DefaultLanguage, catalogGroupCache.LanguageMapById.Values.ToList(), true);
                            if (mediaAsset != null)
                            {
                                EnumerableRowCollection<DataRow> assetFileTypes = null;
                                if (fileTypes.Any())
                                {
                                    assetFileTypes = (from row in fileTypes
                                                      where (Int64)row["MEDIA_ID"] == id
                                                      select row);
                                }

                                Dictionary<int, ApiObjects.SearchObjects.Media> assets = CreateMediasFromMediaAssetAndLanguages(groupId, mediaAsset, assetFileTypes, catalogGroupCache, linearChannelsRegionsMapping);
                                if (geoAvailability.ContainsKey(id))
                                {
                                    foreach (DataRow row in geoAvailability[id])
                                    {
                                        int countryId = ODBCWrapper.Utils.GetIntSafeVal(row, "COUNTRY_ID");
                                        int isAllowed = ODBCWrapper.Utils.GetIntSafeVal(row, "IS_ALLOWED");

                                        if (isAllowed > 0)
                                        {

                                            foreach (var asset in assets.Values)
                                            {
                                                asset.allowedCountries.Add(countryId);
                                            }
                                        }
                                        else
                                        {
                                            foreach (var asset in assets.Values)
                                            {
                                                asset.blockedCountries.Add(countryId);
                                            }
                                        }
                                    }
                                }

                                // If no allowed countries were found for this media - use 0, that indicates that the media is allowed everywhere
                                foreach (ApiObjects.SearchObjects.Media media in assets.Values)
                                {
                                    if (media.allowedCountries.Count == 0)
                                    {
                                        media.allowedCountries.Add(0);
                                    }
                                }

                                groupAssetsMap.Add((int)mediaAsset.Id, assets);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error when creating group media map from data set - for media id {0} ; group {1} ; error = {2}", id, groupId, ex);
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed CreateGroupMediaMapFromDataSet for groupId: {0}", groupId), ex);
            }

            return groupAssetsMap;
        }

        private static Dictionary<int, ApiObjects.SearchObjects.Media> CreateMediasFromMediaAssetAndLanguages(int groupId, MediaAsset mediaAsset, EnumerableRowCollection<DataRow> assetFileTypes,
                                                                                                                CatalogGroupCache catalogGroupCache, Dictionary<long, List<int>> linearChannelsRegionsMapping)
        {
            Dictionary<int, ApiObjects.SearchObjects.Media> result = new Dictionary<int, ApiObjects.SearchObjects.Media>();
            // File Types + is free
            HashSet<int> fileTypes = null;
            HashSet<int> freeFileTypes = null;
            if (assetFileTypes != null && assetFileTypes.Any())
            {
                fileTypes = new HashSet<int>();
                freeFileTypes = new HashSet<int>();
                foreach (DataRow dr in assetFileTypes)
                {
                    int fileTypeId = ODBCWrapper.Utils.GetIntSafeVal(dr, "MEDIA_TYPE_ID");
                    bool isFree = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_FREE", 0) == 1;

                    if (fileTypeId > 0 && !fileTypes.Contains(fileTypeId))
                    {
                        fileTypes.Add(fileTypeId);
                    }

                    if (isFree && fileTypeId > 0 && !freeFileTypes.Contains(fileTypeId))
                    {
                        freeFileTypes.Add(fileTypeId);
                    }
                }
            }

            foreach (LanguageObj language in catalogGroupCache.LanguageMapById.Values)
            {
                string name = mediaAsset.Name;
                string description = mediaAsset.Description;
                Dictionary<string, string> metas = new Dictionary<string, string>();
                Dictionary<string, HashSet<string>> tags = new Dictionary<string, HashSet<string>>();
                if (!language.IsDefault)
                {
                    if (mediaAsset.NamesWithLanguages.Count(x => x.m_sLanguageCode3 == language.Code) == 1)
                    {
                        var nameWithLanguages = mediaAsset.NamesWithLanguages.FirstOrDefault(x => x.m_sLanguageCode3 == language.Code);
                        if (nameWithLanguages != null)
                        {
                            name = nameWithLanguages.m_sValue;
                        }
                    }

                    if (mediaAsset.DescriptionsWithLanguages.Count(x => x.m_sLanguageCode3 == language.Code) == 1)
                    {
                        var descriptionWithLanguages = mediaAsset.DescriptionsWithLanguages.FirstOrDefault(x => x.m_sLanguageCode3 == language.Code);
                        if (descriptionWithLanguages != null)
                        {
                            description = descriptionWithLanguages.m_sValue;
                        }
                    }
                }

                if (mediaAsset.Metas != null && mediaAsset.Metas.Count > 0)
                {
                    if (language.IsDefault)
                    {
                        metas = mediaAsset.Metas.Where(x => x.m_oTagMeta.m_sType != MetaType.DateTime.ToString()).ToDictionary(x => x.m_oTagMeta.m_sName, x => x.m_sValue);
                        // handle date metas
                        List<Metas> dateMetas = mediaAsset.Metas.Where(x => x.m_oTagMeta.m_sType == MetaType.DateTime.ToString()).ToList();
                        if (dateMetas != null && dateMetas.Count > 0)
                        {
                            foreach (Metas meta in dateMetas)
                            {
                                DateTime date;
                                if (DateTime.TryParse(meta.m_sValue, out date))
                                {
                                    metas.Add(meta.m_oTagMeta.m_sName, date.ToString("yyyyMMddHHmmss"));
                                }
                            }
                        }
                    }
                    else
                    {
                        List<Metas> languageMetas = mediaAsset.Metas.Where(x => x.Value != null && x.Value.Count(y => y.m_sLanguageCode3 == language.Code) == 1).ToList();
                        metas = languageMetas.ToDictionary(x => x.m_oTagMeta.m_sName, x => x.Value.Where(y => y.m_sLanguageCode3 == language.Code).Select(y => y.m_sValue).First());
                    }
                }

                if (mediaAsset.Tags != null && mediaAsset.Tags.Count > 0)
                {
                    if (language.IsDefault)
                    {
                        tags = mediaAsset.Tags.ToDictionary(x => x.m_oTagMeta.m_sName, x => new HashSet<string>(x.m_lValues, StringComparer.OrdinalIgnoreCase));
                    }
                    else
                    {
                        tags = mediaAsset.Tags.Where(x => x.Values != null).ToDictionary(x => x.m_oTagMeta.m_sName,
                                                                                        x => new HashSet<string>(x.Values.SelectMany(y => y.Where(z => z.m_sLanguageCode3 == language.Code)
                                                                                                                                        .Select(z => z.m_sValue)).ToList(), StringComparer.OrdinalIgnoreCase));
                    }
                }

                string now = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string max = DateTime.MaxValue.ToString("yyyyMMddHHmmss");

                ApiObjects.SearchObjects.Media media = new ApiObjects.SearchObjects.Media()
                {
                    m_nMediaID = (int)mediaAsset.Id,
                    m_sName = name,
                    m_sDescription = description,
                    m_nMediaTypeID = mediaAsset.MediaType.m_nTypeID,
                    m_nIsActive = mediaAsset.IsActive.HasValue && mediaAsset.IsActive.Value ? 1 : 0,
                    m_nGroupID = groupId,
                    m_sCreateDate = mediaAsset.CreateDate.Value.ToString("yyyyMMddHHmmss"),
                    m_sEndDate = mediaAsset.EndDate.HasValue ? mediaAsset.EndDate.Value.ToString("yyyyMMddHHmmss") : max,
                    m_sFinalEndDate = mediaAsset.FinalEndDate.HasValue ? mediaAsset.FinalEndDate.Value.ToString("yyyyMMddHHmmss") : max,
                    m_sStartDate = mediaAsset.StartDate.HasValue ? mediaAsset.StartDate.Value.ToString("yyyyMMddHHmmss") : now,
                    CatalogStartDate = mediaAsset.CatalogStartDate.HasValue ? mediaAsset.CatalogStartDate.Value.ToString("yyyyMMddHHmmss") : now,
                    m_sUpdateDate = mediaAsset.UpdateDate.HasValue ? mediaAsset.UpdateDate.Value.ToString("yyyyMMddHHmmss") : now,
                    m_sUserTypes = mediaAsset.UserTypes,
                    m_nDeviceRuleId = mediaAsset.DeviceRuleId.HasValue ? (int)mediaAsset.DeviceRuleId.Value : 0,
                    geoBlockRule = mediaAsset.GeoBlockRuleId.HasValue ? (int)mediaAsset.GeoBlockRuleId.Value : 0,
                    CoGuid = mediaAsset.CoGuid,
                    EntryId = mediaAsset.EntryId,
                    m_dMeatsValues = metas,
                    m_dTagValues = tags,
                    m_sMFTypes = fileTypes != null ? string.Join(";", fileTypes) : string.Empty,
                    freeFileTypes = freeFileTypes != null ? new List<int>(freeFileTypes) : new List<int>(),
                    isFree = freeFileTypes != null && freeFileTypes.Count > 0,
                    inheritancePolicy = (int)mediaAsset.InheritancePolicy,
                    allowedCountries = new List<int>(),
                    blockedCountries = new List<int>()
                };

                if (catalogGroupCache.IsRegionalizationEnabled)
                {
                    if (linearChannelsRegionsMapping != null && linearChannelsRegionsMapping.ContainsKey(media.m_nMediaID))
                    {
                        media.regions = linearChannelsRegionsMapping[media.m_nMediaID];
                    }
                    else
                    {
                        media.regions = new List<int>() { 0 };
                    }
                }

                result.Add(language.ID, media);
            }

            return result;
        }

        private static Status ValidateBasicTopicIdsToRemove(CatalogGroupCache catalogGroupCache, HashSet<long> topicIds)
        {
            Status result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            if (topicIds != null && topicIds.Count > 0 && catalogGroupCache.TopicsMapBySystemNameAndByType != null && catalogGroupCache.TopicsMapBySystemNameAndByType.Count > 0)
            {
                List<long> basicMetaIds = catalogGroupCache.TopicsMapBySystemNameAndByType.Where(x => BasicMetasSystemNamesToType.ContainsKey(x.Key)
                                                                                                && x.Value.ContainsKey(BasicMetasSystemNamesToType[x.Key]))
                                                                                                .Select(x => x.Value[BasicMetasSystemNamesToType[x.Key]].Id).ToList();
                if (basicMetaIds != null && basicMetaIds.Count > 0)
                {
                    List<long> basicMetaIdsToRemove = basicMetaIds.Intersect(topicIds).ToList();
                    if (basicMetaIdsToRemove != null && basicMetaIdsToRemove.Count > 0)
                    {
                        result = new Status((int)eResponseStatus.CanNotRemoveBasicMetaIds, string.Format("{0} for the following Meta Ids: {1}",
                                            eResponseStatus.CanNotRemoveBasicMetaIds.ToString(), string.Join(",", basicMetaIdsToRemove)));
                    }
                }
            }

            return result;
        }

        private static LiveAsset CreateLinearMediaAssetFromDataTable(int groupId, DataTable dt, MediaAsset mediaAsset, long? epgChannelId = null)
        {
            LiveAsset result = null;
            if (dt == null || dt.Rows == null || dt.Rows.Count != 1)
            {
                log.WarnFormat("CreateLinearMediaAssetResponseFromDataTable - returned table is not valid");
                return result;
            }

            DataRow dr = dt.Rows[0];
            TstvState enableCdvr = (TstvState)ODBCWrapper.Utils.GetIntSafeVal(dr, "ENABLE_CDVR");
            TstvState enableCatchUp = (TstvState)ODBCWrapper.Utils.GetIntSafeVal(dr, "ENABLE_CATCH_UP");
            TstvState enableStartOver = (TstvState)ODBCWrapper.Utils.GetIntSafeVal(dr, "ENABLE_START_OVER");
            TstvState enableTrickPlay = (TstvState)ODBCWrapper.Utils.GetIntSafeVal(dr, "ENABLE_TRICK_PLAY");
            TstvState enableRecordingPlaybackNonEntitledChannel = (TstvState)ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_recording_playback_non_entitled");
            long catchUpBuffer = ODBCWrapper.Utils.GetLongSafeVal(dr, "CATCH_UP_BUFFER", 0);
            long trickPlayBuffer = ODBCWrapper.Utils.GetLongSafeVal(dr, "TRICK_PLAY_BUFFER", 0);
            string externalIngestId = ODBCWrapper.Utils.GetSafeStr(dr, "CHANNEL_ID");
            string externalCdvrId = ODBCWrapper.Utils.GetSafeStr(dr, "CDVR_ID");
            LinearChannelType channelType = (LinearChannelType)ODBCWrapper.Utils.GetIntSafeVal(dr, "epg_channel_type");
            TimeShiftedTvPartnerSettings accountTstvSettings = ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
            if (!epgChannelId.HasValue)
            {
                epgChannelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
            }
            
            result = new LiveAsset(epgChannelId.Value, enableCdvr, enableCatchUp, enableStartOver, enableTrickPlay, enableRecordingPlaybackNonEntitledChannel,
                                   catchUpBuffer, trickPlayBuffer, externalIngestId, externalCdvrId, mediaAsset, accountTstvSettings, channelType);

            return result;
        }

        private static GenericResponse<Asset> AddLinearMediaAsset(int groupId, MediaAsset mediaAsset, LiveAsset linearMediaAssetToAdd, long userId)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            try
            {
                DataTable dt = CatalogDAL.InsertLinearMediaAsset(groupId, linearMediaAssetToAdd.EnableCdvrState, linearMediaAssetToAdd.EnableCatchUpState, linearMediaAssetToAdd.EnableRecordingPlaybackNonEntitledChannelState,
                                                                linearMediaAssetToAdd.EnableStartOverState, linearMediaAssetToAdd.EnableTrickPlayState, linearMediaAssetToAdd.BufferCatchUp, linearMediaAssetToAdd.BufferTrickPlay,
                                                                linearMediaAssetToAdd.ExternalCdvrId, linearMediaAssetToAdd.ExternalEpgIngestId, mediaAsset.Id, linearMediaAssetToAdd.ChannelType, userId);
                result.Object = CreateLinearMediaAssetFromDataTable(groupId, dt, mediaAsset);
                if (result.Object != null)
                {
                    result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());

                    // UpdateIndex
                    bool indexingResult = IndexManager.UpsertMedia(groupId, result.Object.Id);
                    if (!indexingResult)
                    {
                        log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after AddLinearMediaAsset", result.Object.Id, groupId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddLinearMediaAsset for groupId: {0} and asset: {1}", groupId, linearMediaAssetToAdd.ToString()), ex);
            }

            return result;
        }

        private static GenericResponse<Asset> UpdateLinearMediaAsset(int groupId, MediaAsset mediaAsset, LiveAsset linearMediaAssetToUpdate, long userId)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            try
            {
                DataTable dt = CatalogDAL.UpdateLinearMediaAsset(groupId, mediaAsset.Id, linearMediaAssetToUpdate.EnableCdvrState, linearMediaAssetToUpdate.EnableCatchUpState,
                                                                linearMediaAssetToUpdate.EnableRecordingPlaybackNonEntitledChannelState, linearMediaAssetToUpdate.EnableStartOverState,
                                                                linearMediaAssetToUpdate.EnableTrickPlayState, linearMediaAssetToUpdate.BufferCatchUp, linearMediaAssetToUpdate.BufferTrickPlay,
                                                                linearMediaAssetToUpdate.ExternalCdvrId, linearMediaAssetToUpdate.ExternalEpgIngestId, linearMediaAssetToUpdate.ChannelType, userId);
                result.Object = CreateLinearMediaAssetFromDataTable(groupId, dt, mediaAsset, linearMediaAssetToUpdate.EpgChannelId);
                
                if (result.Object != null)
                {
                    result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());

                    // UpdateIndex
                    bool indexingResult = IndexManager.UpsertMedia(groupId, result.Object.Id);
                    if (!indexingResult)
                    {
                        log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after UpdateLinearMediaAsset", result.Object.Id, groupId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateLinearMediaAsset for groupId: {0} and asset: {1}", groupId, linearMediaAssetToUpdate.ToString()), ex);
            }

            return result;
        }

        private static void SetInheritedValue(int groupId, CatalogGroupCache catalogGroupCache, AssetStruct assetStruct, MediaAsset asset)
        {
            // Add to asset.Metas, asset.Tags the missing values from parent
            var inherited = assetStruct.AssetStructMetas.Where(x => x.Value.IsInherited.HasValue && x.Value.IsInherited.Value).ToList();
            if (inherited != null && inherited.Count > 0)
            {
                Asset parentAsset = GetParentAsset(groupId, catalogGroupCache, assetStruct, asset);
                if (parentAsset == null)
                {
                    return;
                }

                List<Topic> topicsToInherit = new List<Topic>();
                foreach (var kvp in inherited)
                {
                    Topic topic = catalogGroupCache.TopicsMapById[kvp.Value.MetaId];
                    if (topic.Type == MetaType.Tag)
                    {
                        Tags tag = asset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));
                        Tags parentTag = parentAsset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));

                        if (tag != null)
                        {
                            if (!tag.Equals(parentTag))
                            {
                                asset.InheritancePolicy = AssetInheritancePolicy.Disable;
                                return;
                            }
                        }
                        else
                        {
                            topicsToInherit.Add(topic);
                        }
                    }
                    else
                    {
                        Metas meta = asset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));
                        Metas parentMeta = parentAsset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));

                        if (meta != null)
                        {
                            if (!meta.Equals(parentMeta))
                            {
                                asset.InheritancePolicy = AssetInheritancePolicy.Disable;
                                return;
                            }
                        }
                        else
                        {
                            topicsToInherit.Add(topic);
                        }
                    }
                }

                if (topicsToInherit.Count > 0 && assetStruct.ParentId.HasValue)
                {
                    foreach (var topic in topicsToInherit)
                    {
                        if (topic.Type == MetaType.Tag)
                        {
                            Tags tag = parentAsset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));
                            if (tag != null)
                            {
                                asset.Tags.Add(tag);
                            }
                        }
                        else
                        {
                            Metas meta = parentAsset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));
                            if (meta != null)
                            {
                                asset.Metas.Add(meta);
                            }
                        }
                    }
                }
            }
        }

        private static Asset GetParentAsset(int groupId, CatalogGroupCache catalogGroupCache, AssetStruct assetStruct, Asset asset)
        {
            Asset parentAsset = null;

            if (!assetStruct.ParentId.HasValue)
            {
                return parentAsset;
            }
            Topic parentConnectingTopic = catalogGroupCache.TopicsMapById[assetStruct.ConnectedParentMetaId.Value];
            Topic childConnectingTopic = catalogGroupCache.TopicsMapById[assetStruct.ConnectingMetaId.Value];

            string connectedValue = string.Empty;
            if (childConnectingTopic.Type == MetaType.Tag)
            {
                Tags t = asset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(childConnectingTopic.SystemName.ToLower()));
                if (t != null && t.m_lValues != null && t.m_lValues.Count > 0)
                {
                    connectedValue = t.m_lValues[0];
                }
            }
            else
            {
                Metas meta = asset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(childConnectingTopic.SystemName.ToLower()));
                if (meta != null)
                {
                    connectedValue = meta.m_sValue;
                }
            }

            if (!string.IsNullOrEmpty(connectedValue))
            {
                string filter = string.Format("(and asset_type='{0}' {1}='{2}')", assetStruct.ParentId.Value, parentConnectingTopic.SystemName, connectedValue);
                UnifiedSearchResult[] assets = Core.Catalog.Utils.SearchAssets(groupId, filter, 0, 0, false, true);
                if (assets != null && assets.Length > 0)
                {
                    GenericResponse<Asset> response = AssetManager.GetAsset(groupId, long.Parse(assets[0].AssetId), eAssetTypes.MEDIA, true);
                    if (response.Status.Code != (int)eResponseStatus.OK || response.Object == null)
                    {
                        log.ErrorFormat("Failed to get ");
                        return parentAsset;
                    }

                    parentAsset = response.Object;
                }
            }

            return parentAsset;
        }

        private static Status DeleteMediaAsset(int groupId, long mediaId, long userId, MediaAsset currentAsset, bool isFromChannel = false)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Status status = AssetUserRuleManager.CheckAssetUserRuleList(groupId, userId, mediaId);
            if (status == null || status.Code == (int)eResponseStatus.ActionIsNotAllowed)
            {
                result.Set((int)eResponseStatus.ActionIsNotAllowed, ACTION_IS_NOT_ALLOWED);
                return result;
            }

            if(currentAsset == null)
            {
                result.Set((int)eResponseStatus.AssetDoesNotExist, eResponseStatus.AssetDoesNotExist.ToString());
                return result;
            }           

            if (CatalogDAL.DeleteMediaAsset(groupId, mediaId, userId))
            {
                result.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                if(!isFromChannel)
                {
                    CatalogGroupCache catalogGroupCache;
                    if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpdateAsset", groupId);
                        return result;
                    }

                    AssetStruct assetStruct = null;
                    if (currentAsset.MediaType.m_nTypeID > 0 && catalogGroupCache.AssetStructsMapById.ContainsKey(currentAsset.MediaType.m_nTypeID))
                    {
                        assetStruct = catalogGroupCache.AssetStructsMapById[currentAsset.MediaType.m_nTypeID];
                    }

                    if (assetStruct.SystemName == MANUAL_ASSET_STRUCT_NAME || assetStruct.SystemName == DYNAMIC_ASSET_STRUCT_NAME || assetStruct.SystemName == EXTERNAL_ASSET_STRUCT_NAME)
                    {
                        if (assetStruct.TopicsMapBySystemName == null || assetStruct.TopicsMapBySystemName.Count == 0)
                        {
                            if (CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                            {
                                assetStruct.TopicsMapBySystemName = catalogGroupCache.TopicsMapById.Where(x => assetStruct.MetaIds.Contains(x.Key))
                                                                  .OrderBy(x => assetStruct.MetaIds.IndexOf(x.Key))
                                                                  .ToDictionary(x => x.Value.SystemName, y => y.Value);
                            }
                        }

                        if (assetStruct.SystemName == EXTERNAL_ASSET_STRUCT_NAME)
                        {
                            DeleteExternalChannel(groupId, userId, currentAsset, assetStruct);
                        }
                        else
                        {
                            DeleteChannel(groupId, userId, currentAsset, assetStruct);
                        }
                    }
                }

                // Delete Index
                bool indexingResult = IndexManager.DeleteMedia(groupId, (int)mediaId);
                if (!indexingResult)
                {
                    log.ErrorFormat("Failed to delete media index for assetId: {0}, groupId: {1} after DeleteAsset", mediaId, groupId);
                }
            }
            else
            {
                log.ErrorFormat("Failed to delete media asset with id: {0}, groupId: {1}", mediaId, groupId);
            }

            return result;
        }        

        private static int GetTotalAmountOfDistinctAssets(List<BaseObject> assets)
        {
            int result = 0;
            try
            {
                result += assets.Where(x => x.AssetType == eAssetTypes.MEDIA).Select(x => x.AssetId).Distinct().Count();
                result += assets.Where(x => x.AssetType == eAssetTypes.EPG).Select(x => x.AssetId).Distinct().Count();
                result += assets.Where(x => x.AssetType == eAssetTypes.NPVR).Select(x => x.AssetId).Distinct().Count();
            }
            catch (Exception ex)
            {
                log.Error("Failed GetTotalAmountOfDistinctAssets", ex);
            }

            return result;
        }

        private static void UpdateChannel(int groupId, long userId, MediaAsset asset, AssetStruct assetStruct)
        {
            // Check assetStruct catalogId existence          
            if (assetStruct.TopicsMapBySystemName.ContainsKey(AssetManager.CHANNEL_ID_META_SYSTEM_NAME))
            {
                if (asset != null && asset.Metas != null && asset.Metas.Count > 0)
                {
                    int channelId = 0;
                    var tagMeta = asset.Metas.Where(x => x.m_oTagMeta.m_sName == AssetManager.CHANNEL_ID_META_SYSTEM_NAME).FirstOrDefault();
                    if(tagMeta == null)
                    {
                        log.ErrorFormat("Error while update asset {0} channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    int.TryParse(tagMeta.m_sValue, out channelId);
                    if (channelId == 0)
                    {
                        log.ErrorFormat("Error while update asset {0} channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    GenericResponse<GroupsCacheManager.Channel> channelToUpdate = ChannelManager.GetChannelById(groupId, channelId, true, userId);

                    if (!channelToUpdate.HasObject())
                    {
                        log.ErrorFormat("Failed UpdateChannel. channel not found. groupId {0}, channelId {1}", groupId, channelId);
                        return;
                    }

                    GroupsCacheManager.Channel channel = channelToUpdate.Object;

                    channel.m_sName = asset.Name;
                    channel.NamesInOtherLanguages = asset.NamesWithLanguages;
                    channel.m_sDescription = asset.Description;
                    channel.DescriptionInOtherLanguages = asset.DescriptionsWithLanguages;

                    GenericResponse<GroupsCacheManager.Channel> channelUpdateResponse = ChannelManager.UpdateChannel(groupId, channel.m_nChannelID, channel, userId, false, true);

                    if (!channelUpdateResponse.IsOkStatusCode())
                    {
                        log.ErrorFormat("Failed update channelId {0}, groupId {1}, assetId {2}", channel.m_nChannelID, groupId, asset.Id);
                    }
                }
            }
        }

        private static void UpdateExternalChannel(int groupId, long userId, MediaAsset asset, AssetStruct assetStruct)
        {
            if (assetStruct.TopicsMapBySystemName.ContainsKey(AssetManager.CHANNEL_ID_META_SYSTEM_NAME))
            {
                if (asset != null && asset.Metas != null && asset.Metas.Count > 0)
                {
                    int channelId = 0;
                    var tagMeta = asset.Metas.Where(x => x.m_oTagMeta.m_sName == AssetManager.CHANNEL_ID_META_SYSTEM_NAME).FirstOrDefault();
                    if (tagMeta == null)
                    {
                        log.ErrorFormat("Error while update asset {0} External channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    int.TryParse(tagMeta.m_sValue, out channelId);
                    if (channelId == 0)
                    {
                        log.ErrorFormat("Error while update asset {0} External channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    //check external channel exist
                    ExternalChannel channelToUpdate = CatalogDAL.GetExternalChannelById(groupId, channelId);
                    if (channelToUpdate == null || channelToUpdate.ID <= 0)
                    {
                        log.ErrorFormat("Failed UpdateExternalChannel. External channel not found. groupId {0}, ExternalchannelId {1}", groupId, channelId);
                        return;
                    }

                    channelToUpdate.Name = asset.Name;

                    ExternalChannelResponse externalChannelResponse  = Api.api.SetExternalChannel(groupId, channelToUpdate, userId, true);

                    if (externalChannelResponse == null || externalChannelResponse.ExternalChannel == null)
                    {
                        log.ErrorFormat("Failed update channelId{0}, groupId {1}, channelId {2}", channelToUpdate.ID, groupId);
                    }
                }
            }            
        }

        private static void DeleteExternalChannel(int groupId, long userId, MediaAsset asset, AssetStruct assetStruct)
        {
            if (assetStruct.TopicsMapBySystemName.ContainsKey(AssetManager.CHANNEL_ID_META_SYSTEM_NAME))
            {
                if (asset != null && asset.Metas != null && asset.Metas.Count > 0)
                {
                    int channelId = 0;
                    var tagMeta = asset.Metas.Where(x => x.m_oTagMeta.m_sName == AssetManager.CHANNEL_ID_META_SYSTEM_NAME).FirstOrDefault();
                    if (tagMeta == null)
                    {
                        log.ErrorFormat("Error while delete asset {0} External channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    int.TryParse(tagMeta.m_sValue, out channelId);
                    if (channelId == 0)
                    {
                        log.ErrorFormat("Error while delete asset {0} External channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    //check external channel exist
                    ExternalChannel channelToUpdate = CatalogDAL.GetExternalChannelById(groupId, channelId);
                    if (channelToUpdate == null || channelToUpdate.ID <= 0)
                    {
                        log.ErrorFormat("Failed UpdateExternalChannel. External channel not found. groupId {0}, ExternalchannelId {1}", groupId, channelId);
                        return;
                    }

                    channelToUpdate.Name = asset.Name;

                    Status status = Api.api.DeleteExternalChannel(groupId, channelToUpdate.ID, userId, true);
                    if (status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Failed delete channelId {0}, groupId {1}, assetId {2}", channelId, groupId, asset.Id);

                    }                   
                }
            }

            return;
        }

        private static void DeleteChannel(int groupId, long userId, MediaAsset asset, AssetStruct assetStruct)
        {
            // Check assetStruct catalogId existence          
            if (assetStruct.TopicsMapBySystemName.ContainsKey(CHANNEL_ID_META_SYSTEM_NAME))
            {
                if (asset != null && asset.Metas != null && asset.Metas.Count > 0)
                {
                    int channelId = 0;
                    var tagMeta = asset.Metas.Where(x => x.m_oTagMeta.m_sName == CHANNEL_ID_META_SYSTEM_NAME).FirstOrDefault();
                    if (tagMeta == null)
                    {
                        log.ErrorFormat("Error while delete asset {0} channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    int.TryParse(tagMeta.m_sValue, out channelId);
                    if (channelId == 0)
                    {
                        log.ErrorFormat("Error while delete asset {0} channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    Status status = ChannelManager.DeleteChannel(groupId, channelId, userId, true);
                    if(status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Failed delete  channelId {0}, groupId {1}, assetId {2}", channelId, groupId, asset.Id);

                    }                   
                }
            }

            return;
        }

        private static Status ValidateRelatedEntities(int groupId, CatalogGroupCache catalogGroupCache, HashSet<long> assetStructMetaIds, 
            List<RelatedEntities> relatedEntitiesList, ref XmlDocument relatedEntitiesXmlDoc)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            HashSet<string> tempHashSet = new HashSet<string>();
            
            if (relatedEntitiesList?.Count > 0)
            {
                relatedEntitiesXmlDoc = new XmlDocument();                
                XmlNode rootNode = relatedEntitiesXmlDoc.CreateElement("root");
                relatedEntitiesXmlDoc.AppendChild(rootNode);

                foreach (RelatedEntities relatedEntities in relatedEntitiesList)
                {
                    // validate duplicates do not exist
                    if (tempHashSet.Contains(relatedEntities.TagMeta.m_sName))
                    {
                        result.Message = string.Format("Duplicate relatedEntities sent, relatedEntities name: {0}", relatedEntities.TagMeta.m_sName);
                        return result;
                    }

                    tempHashSet.Add(relatedEntities.TagMeta.m_sName);

                    //validate relatedEntity exists on group
                    if (!catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(relatedEntities.TagMeta.m_sName)
                        || !catalogGroupCache.TopicsMapBySystemNameAndByType[relatedEntities.TagMeta.m_sName].ContainsKey(relatedEntities.TagMeta.m_sType))
                    {
                        result.Message = string.Format("relatedEntities: {0} does not exist for group", relatedEntities.TagMeta.m_sName);
                        return result;
                    }

                    // validate meta exists on asset struct
                    if (!assetStructMetaIds.Contains(catalogGroupCache.TopicsMapBySystemNameAndByType[relatedEntities.TagMeta.m_sName][relatedEntities.TagMeta.m_sType].Id))
                    {
                        result.Message = string.Format("relatedEntities: {0} is not part of assetStruct", relatedEntities.TagMeta.m_sName);
                        return result;
                    }

                    Topic topic = catalogGroupCache.TopicsMapBySystemNameAndByType[relatedEntities.TagMeta.m_sName][relatedEntities.TagMeta.m_sType];
                    // validate correct type was sent
                    if (topic.Type.ToString().ToLower() != relatedEntities.TagMeta.m_sType.ToLower())
                    {
                        result = new Status((int)eResponseStatus.InvalidMetaType, string.Format("{0} was sent for relatedEntities: {1}", eResponseStatus.InvalidMetaType.ToString(), relatedEntities.TagMeta.m_sName));
                        return result;
                    }

                    string value = relatedEntities.Items?.Count > 0 ? JsonConvert.SerializeObject(relatedEntities.Items) : string.Empty;

                    AddRealtedEntitiesValueToXml(ref relatedEntitiesXmlDoc, rootNode, topic.Id, value);
                }
            }

            result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            return result;
        }       

        private static Status ValidateRelatedEntitiesLimitaion(List<RelatedEntities> relatedEntitiesToAdd, List<RelatedEntities> relatedEntitiesToUpdate)
        {
            var relatedEntitiesToAddCount = relatedEntitiesToAdd?.Count ?? 0;
            var relatedEntitiesToUpdateCount = relatedEntitiesToUpdate?.Count ?? 0;
            
            if (relatedEntitiesToAddCount + relatedEntitiesToUpdateCount > 5)
            {
                return new Status() { Code = (int)eResponseStatus.RelatedEntitiesExceedLimitation };
            }

            return Status.Ok;
        }

        private static void AddRealtedEntitiesValueToXml(ref XmlDocument relatedEntitiesXmlDoc, XmlNode rootNode, long topicId, string value)
        {
            if (value != null)
            {
                XmlNode rowNode;
                XmlNode topicIdNode;                
                XmlNode valueNode;
                rowNode = relatedEntitiesXmlDoc.CreateElement("row");
                topicIdNode = relatedEntitiesXmlDoc.CreateElement("topic_id");
                topicIdNode.InnerText = topicId.ToString();
                rowNode.AppendChild(topicIdNode);
                valueNode = relatedEntitiesXmlDoc.CreateElement("value");
                valueNode.InnerText = value;
                rowNode.AppendChild(valueNode);
                rootNode.AppendChild(rowNode);
            }
        }

        private static bool TryGetRelatedEntitiesList(int groupId, long id, DataTable relatedEntitiesTable, ref List<RelatedEntities> relatedEntitiesList)
        {
            if(relatedEntitiesTable?.Rows.Count > 0)
            {
                relatedEntitiesList = new List<RelatedEntities>();
                RelatedEntities relatedEntities = null;
                List<RelatedEntity> relatedEntityList = null;
                long topicId = 0;
                string value = string.Empty;
                Topic topic = null;

                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling TryGetRelatedEntitiesList", groupId);
                    return false;
                }

                foreach (DataRow dr in relatedEntitiesTable.Rows)
                {
                    topicId = ODBCWrapper.Utils.GetLongSafeVal(dr, "topic_id");
                    value = ODBCWrapper.Utils.GetSafeStr(dr, "value");

                    try
                    {
                        relatedEntityList = JsonConvert.DeserializeObject<List<RelatedEntity>>(value);
                    }
                    catch (Exception exc)
                    {
                        log.ErrorFormat("Error while DeserializeObject<List<RelatedEntity> at TryGetRelatedEntitiesList. topicId {0}, groupId {1}, assetId {2}. exc {3}",topicId, groupId, id, exc.Message);
                    }

                    if (catalogGroupCache.TopicsMapById.ContainsKey(topicId))
                    {
                        topic = catalogGroupCache.TopicsMapById[topicId];
                    }

                    relatedEntities = new RelatedEntities()
                    {
                        TagMeta = new TagMeta() { m_sName = topic.SystemName, m_sType = topic.Type.ToString() },
                        Items = relatedEntityList
                    };

                    relatedEntitiesList.Add(relatedEntities);
                }

                return true;
            }

            return false;
        }

        private static Status BuildTableDicAfterInsertMediaAsset(DataSet ds, out Dictionary<string, DataTable> tables)
        {
            tables = null;

            if (ds == null || ds.Tables == null)
            {
                log.ErrorFormat("BuildTableDicAfterInsertMediaAsset ds is empty");
                return new Status((int)eResponseStatus.Error);
            }

            // Basic details tables
            if (ds.Tables[0] == null || ds.Tables[0].Rows == null || ds.Tables[0].Rows.Count != 1)
            {
                log.WarnFormat("CreateAssetResponseFromDataSet - basic details table is not valid");
                return new Status((int)eResponseStatus.Error);
            }

            DataRow basicDataRow = ds.Tables[0].Rows[0];
            long id = ODBCWrapper.Utils.GetLongSafeVal(basicDataRow, "ID", 0);
            if (id <= 0)
            {
                return CreateAssetResponseStatusFromResult(id);                
            }

            if (ds.Tables.Count < 7)
            {
                log.WarnFormat("BuildTableDicAfterInsertMediaAsset didn't receive dataset with 6 or more tables");
                return null;
            }

            tables = new Dictionary<string, DataTable>();
            tables.Add(TABLE_NAME_BASIC, ds.Tables[0]);
            tables.Add(TABLE_NAME_METAS, ds.Tables[1]);
            tables.Add(TABLE_NAME_TAGS, ds.Tables[2]);
            tables.Add(TABLE_NAME_FILES, ds.Tables[3]);
            tables.Add(TABLE_NAME_IMAGES, ds.Tables[4]);
            tables.Add(TABLE_NAME_NEW_TAGS, ds.Tables[5]);
            tables.Add(TABLE_NAME_RELATED_ENTITIES, ds.Tables[6]);

            return new Status((int)eResponseStatus.OK);
        }

        private static Status BuildTableDicAfterUpdateMediaAsset(DataSet ds, long assetId, out Dictionary<string, DataTable> tables)
        {
            tables = null;
            if (ds == null)
            {
                log.ErrorFormat("BuildTableDicAfterUpdateMediaAsset ds is empty for assetId {0}", assetId);
                return new Status((int)eResponseStatus.Error);
            }

            // Basic details tables
            if (ds.Tables[0] == null || ds.Tables[0].Rows == null || ds.Tables[0].Rows.Count != 1)
            {
                log.WarnFormat("CreateAssetResponseFromDataSet - basic details table is not valid");
                return new Status((int)eResponseStatus.Error);
            }

            DataRow basicDataRow = ds.Tables[0].Rows[0];
            long id = ODBCWrapper.Utils.GetLongSafeVal(basicDataRow, "ID", 0);
            if (id <= 0)
            {
                return CreateAssetResponseStatusFromResult(id);
            }

            if (ds.Tables.Count < 7)
            {
                log.WarnFormat("BuildTableDicAfterUpdateMediaAsset didn't receive dataset with 6 or more tables assetId {0}", assetId);
                return new Status((int)eResponseStatus.Error);
            }

            tables = new Dictionary<string, DataTable>();
            tables.Add(TABLE_NAME_BASIC, ds.Tables[0]);
            tables.Add(TABLE_NAME_METAS, ds.Tables[1]);
            tables.Add(TABLE_NAME_TAGS, ds.Tables[2]);
            tables.Add(TABLE_NAME_FILES, ds.Tables[3]);
            tables.Add(TABLE_NAME_IMAGES, ds.Tables[4]);
            tables.Add(TABLE_NAME_NEW_TAGS, ds.Tables[5]);
            tables.Add(TABLE_NAME_RELATED_ENTITIES, ds.Tables[6]);

            return new Status((int)eResponseStatus.OK);
        }

        private static Status BuildTableDicAfterGetMediaAssetForElasitcSearch(DataSet ds, long mediaId, out Dictionary<string, DataTable> tables)
        {
            tables = null;
            if (ds == null || ds.Tables == null)
            {
                log.WarnFormat("GetMediaForElasticSearchIndex - dataset or tables are null. MediaId :{0}", mediaId);
                return new Status((int)eResponseStatus.Error);
            }

            // Basic details tables
            if (ds.Tables[0] == null || ds.Tables[0].Rows == null || ds.Tables[0].Rows.Count != 1)
            {
                log.WarnFormat("GetMediaForElasticSearchIndex - basic details table is not valid. MediaId :{0}", mediaId);
                return new Status((int)eResponseStatus.Error);
            }

            tables = new Dictionary<string, DataTable>();
            tables.Add(TABLE_NAME_BASIC, ds.Tables[0]);
            tables.Add(TABLE_NAME_METAS, ds.Tables[1]);
            tables.Add(TABLE_NAME_TAGS, ds.Tables[2]);
            tables.Add(TABLE_NAME_FILES, ds.Tables[3]);            
            tables.Add(TABLE_NAME_UPDATE_DATE, ds.Tables[5]);
            tables.Add(TABLE_NAME_GEO_AVAILABILITY, ds.Tables[6]);

            return new Status((int)eResponseStatus.OK);
        }

        private static Status ValidateNoneExistingTopicIdsToRemove(MediaAsset mediaAsset, HashSet<long> topicIds, CatalogGroupCache catalogGroupCache)
        {
            var result = Status.Ok;
            
            List<long> existingTopicsIds = mediaAsset.Metas.Where(x => catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(x.m_oTagMeta.m_sName)
                                                                        && catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName].ContainsKey(x.m_oTagMeta.m_sType))
                                                                        .Select(x => catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName][x.m_oTagMeta.m_sType].Id).ToList();
            existingTopicsIds.AddRange(mediaAsset.Tags.Where(x => catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(x.m_oTagMeta.m_sName)
                                                            && catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName].ContainsKey(x.m_oTagMeta.m_sType))
                                                            .Select(x => catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName][x.m_oTagMeta.m_sType].Id).ToList());
            if (mediaAsset.RelatedEntities != null)
            {
                existingTopicsIds.AddRange(mediaAsset.RelatedEntities.Where(x => catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(x.TagMeta.m_sName)
                                                                && catalogGroupCache.TopicsMapBySystemNameAndByType[x.TagMeta.m_sName].ContainsKey(x.TagMeta.m_sType))
                                                                .Select(x => catalogGroupCache.TopicsMapBySystemNameAndByType[x.TagMeta.m_sName][x.TagMeta.m_sType].Id).ToList());
            }

            List<long> noneExistingMetaIds = topicIds.Except(existingTopicsIds).ToList();
            if (noneExistingMetaIds != null && noneExistingMetaIds.Count > 0)
            {
                result.Set(eResponseStatus.MetaIdsDoesNotExistOnAsset, 
                           string.Format("{0} for the following Meta Ids: {1}", eResponseStatus.MetaIdsDoesNotExistOnAsset.ToString(), string.Join(",", noneExistingMetaIds)));
            }

            return result;
        }
        
        private static void RemoveNoneExistingTopicIds(HashSet<long> topicIds, Asset asset, CatalogGroupCache catalogGroupCache)
        {
            List<long> topicsToRemove = new List<long>();
            foreach (var topicId in topicIds)
            {
                if (!catalogGroupCache.TopicsMapById.ContainsKey(topicId))
                {
                    topicsToRemove.Add(topicId);
                    continue;
                }

                var topic = catalogGroupCache.TopicsMapById[topicId];

                if (topic.Type == MetaType.Tag)
                {
                    if (!asset.Tags.Any(x => x.m_oTagMeta.m_sName.Equals(topic.SystemName) && x.m_oTagMeta.m_sType.Equals(topic.Type.ToString())))
                    {
                        topicsToRemove.Add(topicId);
                    }
                    continue;
                }

                if (topic.Type == MetaType.ReleatedEntity)
                {
                    if (!asset.RelatedEntities.Any(x => x.TagMeta.m_sName.Equals(topic.SystemName) && x.TagMeta.m_sType.Equals(topic.Type.ToString())))
                    {
                        topicsToRemove.Add(topicId);
                    }
                    continue;
                }

                // all other metaTypes
                if (!asset.Metas.Any(x => x.m_oTagMeta.m_sName.Equals(topic.SystemName) && x.m_oTagMeta.m_sType.Equals(topic.Type.ToString())))
                {
                    topicsToRemove.Add(topicId);
                }
            }

            foreach (var topicToRemove in topicsToRemove)
            {
                topicIds.Remove(topicToRemove);
            }
        }

        #endregion

        #region Public Methods

        public static MediaObj GetMediaObj(int groupId, long id)
        {
            MediaObj result = null;
            try
            {
                if (id > 0)
                {
                    CatalogGroupCache catalogGroupCache;
                    if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetAsset", groupId);
                        return result;
                    }

                    // isAllowedToViewInactiveAssets = false due to backward compatibility 
                    List<Asset> assets = AssetManager.GetAssets(groupId, new List<KeyValuePair<eAssetTypes, long>>() { new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, id) }, false);
                    if (assets != null && assets.Count == 1)
                    {
                        result = new MediaObj(groupId, assets[0] as MediaAsset);
                        if (assets[0] is LiveAsset)
                        {
                            result.m_ExternalIDs = (assets[0] as LiveAsset).EpgChannelId.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAsset for groupId: {0} and id: {1}", groupId, id), ex);
            }

            return result;
        }

        public static GenericResponse<Asset> GetAsset(int groupId, long id, eAssetTypes assetType, bool isAllowedToViewInactiveAssets)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            try
            {
                if (id > 0 && assetType != eAssetTypes.UNKNOWN)
                {
                    List<Asset> assets = GetAssets(groupId, new List<KeyValuePair<eAssetTypes, long>>() { new KeyValuePair<eAssetTypes, long>(assetType, id) }, isAllowedToViewInactiveAssets);
                    if (assets == null || assets.Count != 1 || assets[0] == null)
                    {
                        log.ErrorFormat("Failed getting asset from GetAssetFromCache, for groupId: {0}, id: {1}, assetType: {2}", groupId, id, assetType.ToString());
                        result.SetStatus(eResponseStatus.AssetDoesNotExist, eResponseStatus.AssetDoesNotExist.ToString());
                    }
                    else
                    {
                        result.Object = assets[0];
                        result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAsset for groupId: {0}, id: {1}, assetType: {2}", groupId, id, assetType.ToString()), ex);
            }

            return result;
        }

        public static List<Asset> GetAssets(int groupId, List<KeyValuePair<eAssetTypes, long>> assets, bool isAllowedToViewInactiveAssets)
        {
            List<Asset> result = null;
            try
            {
                if (assets != null && assets.Count > 0)
                {
                    result = GetAssetsFromCache(groupId, assets, isAllowedToViewInactiveAssets);
                    if (result == null || result.Count != assets.Count)
                    {
                        log.ErrorFormat("Failed getting assets from GetAssetsFromCache, for groupId: {0}, assets: {1}", groupId,
                                        assets != null ? string.Join(",", assets.Select(x => string.Format("{0}_{1}", x.Key, x.Value)).ToList()) : string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssets for groupId: {0}, assets: {1}", groupId,
                                        assets != null ? string.Join(",", assets.Select(x => string.Format("{0}_{1}", x.Key, x.Value)).ToList()) : string.Empty), ex);
            }

            return result;
        }

        public static GenericListResponse<Asset> GetOrderedAssets(int groupId, List<BaseObject> assets, bool isAllowedToViewInactiveAssets)
        {
            GenericListResponse<Asset> result = new GenericListResponse<Asset>();

            try
            {
                if (assets != null && assets.Count > 0)
                {
                    List<KeyValuePair<eAssetTypes, long>> assetsToRetrieve = new List<KeyValuePair<eAssetTypes, long>>();
                    HashSet<string> items = new HashSet<string>();

                    Dictionary<string, RecordingSearchResult> recordingsMap = new Dictionary<string, RecordingSearchResult>();

                    foreach (var item in assets)
                    {
                        eAssetTypes assetType = item.AssetType;
                        string assetId = item.AssetId;

                        if (item.AssetType == eAssetTypes.NPVR)
                        {
                            RecordingSearchResult rsr = (RecordingSearchResult)item;
                            recordingsMap.Add(item.AssetId, rsr);
                            assetId = rsr.EpgId;
                            assetType = eAssetTypes.EPG;
                        }

                        string key = string.Format("{0}_{1}", assetType.ToString(), assetId);

                        if (!items.Contains(key))
                        {
                            items.Add(key);
                            assetsToRetrieve.Add(new KeyValuePair<eAssetTypes, long>(assetType, long.Parse(assetId)));                            
                        }
                    }

                    int totalAmountOfDistinctAssets = assetsToRetrieve.Count;

                    List<Asset> unOrderedAssets = GetAssets(groupId, assetsToRetrieve, isAllowedToViewInactiveAssets);

                    if (!isAllowedToViewInactiveAssets && (unOrderedAssets == null || unOrderedAssets.Count == 0))
                    {
                        result.SetStatus(eResponseStatus.OK);
                        return result;
                    }                   

                    string keyFormat = "{0}_{1}"; // mapped asset key format = assetType_assetId
                    Dictionary<string, Asset> mappedAssets = unOrderedAssets.ToDictionary(x => string.Format(keyFormat, x.AssetType.ToString(), x.Id), x => x);
                    foreach (BaseObject baseAsset in assets)
                    {
                        bool isNpvr = baseAsset.AssetType == eAssetTypes.NPVR;

                        Asset asset = null;
                        if (!isNpvr)
                        {
                            asset = mappedAssets[string.Format(keyFormat, baseAsset.AssetType.ToString(), baseAsset.AssetId)];
                        }
                        else if (recordingsMap.ContainsKey(baseAsset.AssetId))
                        {
                            asset = mappedAssets[string.Format(keyFormat, eAssetTypes.EPG.ToString(), recordingsMap[baseAsset.AssetId].EpgId)];
                        }

                        if(asset.IndexStatus == AssetIndexStatus.Deleted)
                        {
                            result.Objects.Add(asset);
                            continue;
                        }

                        if (!isAllowedToViewInactiveAssets
                            || isNpvr
                            || Math.Abs((baseAsset.m_dUpdateDate - asset.UpdateDate.Value).TotalSeconds) <= 1)
                        {
                            if (isNpvr)
                            {
                                RecordingAsset recordingAsset = new RecordingAsset((EpgAsset)asset);
                                recordingAsset.RecordingId = baseAsset.AssetId;
                                recordingAsset.RecordingType = recordingsMap[baseAsset.AssetId].RecordingType;

                                result.Objects.Add(recordingAsset);
                            }
                            else
                            {
                                result.Objects.Add(asset);
                            }
                        }
                        else
                        {
                            asset.IndexStatus = AssetIndexStatus.NotUpdated;
                            result.Objects.Add(asset);
                            log.DebugFormat("Get NotUpdated Asset {0}, groupId {1}", asset.Id, groupId);
                        }
                    }

                    result.SetStatus(eResponseStatus.OK);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetOrderedAssets for groupId: {0}, assets: {1}", groupId,
                                        assets != null ? string.Join(",", assets.Select(x => string.Format("{0}_{1}", x.AssetType.ToString(), x.AssetId)).ToList()) : string.Empty), ex);
            }

            return result;
        }

        /// <summary>
        /// Returns dictionary of [assetId, [language, media]]
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> GetMediaForElasticSearchIndex(int groupId, long mediaId)
        {
            // <assetId, <languageId, media>>
            Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> result = new Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>>();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetMediaForElasticSearchIndex", groupId);
                    return result;
                }

                DataSet ds = CatalogDAL.GetMediaAssetForElasitcSearch(groupId, mediaId, catalogGroupCache.DefaultLanguage.ID);

                Dictionary<string, DataTable> tables = null;
                Status status = BuildTableDicAfterGetMediaAssetForElasitcSearch(ds, mediaId, out tables);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    return result;
                }

                MediaAsset mediaAsset = CreateMediaAsset(groupId, mediaId, tables, catalogGroupCache.DefaultLanguage, catalogGroupCache.LanguageMapById.Values.ToList(), true);
                if (mediaAsset != null)
                {
                    EnumerableRowCollection<DataRow> assetFileTypes = null;
                    if (ds != null && ds.Tables != null && ds.Tables[3] != null && ds.Tables[3].Rows != null && ds.Tables[3].Rows.Count > 0)
                    {
                        assetFileTypes = ds.Tables[3].AsEnumerable();
                    }

                    Dictionary<long, List<int>> linearChannelsRegionsMapping = null;
                    if (catalogGroupCache.IsRegionalizationEnabled)
                    {
                        linearChannelsRegionsMapping = CatalogManager.GetLinearMediaRegions(groupId);
                    }

                    Dictionary<int, ApiObjects.SearchObjects.Media> assets = CreateMediasFromMediaAssetAndLanguages(groupId, mediaAsset, assetFileTypes, catalogGroupCache, linearChannelsRegionsMapping);

                    if (ds != null && ds.Tables != null && ds.Tables.Count > 6 && ds.Tables[6] != null && ds.Tables[6].Rows != null && ds.Tables[6].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[6].Rows)
                        {
                            int countryId = ODBCWrapper.Utils.GetIntSafeVal(row, "COUNTRY_ID");
                            if (countryId > 0)
                            {
                                int isAllowed = ODBCWrapper.Utils.GetIntSafeVal(row, "IS_ALLOWED");
                                if (isAllowed > 0)
                                {
                                    foreach (var asset in assets.Values)
                                    {
                                        asset.allowedCountries.Add(countryId);
                                    }
                                }
                                else
                                {
                                    foreach (var asset in assets.Values)
                                    {
                                        asset.blockedCountries.Add(countryId);
                                    }
                                }
                            }
                        }
                    }

                    // If no allowed countries were found for this media - use 0, that indicates that the media is allowed everywhere
                    foreach (var asset in assets.Values)
                    {
                        if (asset.allowedCountries.Count == 0)
                        {
                            asset.allowedCountries.Add(0);
                        }
                    }

                    result.Add((int)mediaAsset.Id, assets);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetMediaForElasticSearchIndex for groupId: {0}", groupId), ex);
            }

            return result;
        }        

        public static GenericResponse<Asset> AddAsset(int groupId, Asset assetToAdd, long userId, bool isFromIngest = false)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling AddAsset", groupId);
                    return result;
                }

                switch (assetToAdd.AssetType)
                {
                    case eAssetTypes.EPG:
                        if (assetToAdd is EpgAsset)
                        {
                            result = EpgAssetManager.AddEpgAsset(groupId, (assetToAdd as EpgAsset), userId, catalogGroupCache);
                        }
                        break;
                    case eAssetTypes.NPVR:
                        break;
                    case eAssetTypes.MEDIA:
                        bool isLinear = assetToAdd is LiveAsset;
                        MediaAsset mediaAssetToAdd = assetToAdd as MediaAsset;
                        if (mediaAssetToAdd != null)
                        {
                            result = AddMediaAsset(groupId, ref catalogGroupCache, mediaAssetToAdd, isLinear, userId, isFromIngest);
                            if (isLinear && result.HasObject())
                            {
                                LiveAsset linearMediaAssetToAdd = assetToAdd as LiveAsset;
                                result = AddLinearMediaAsset(groupId, result.Object as MediaAsset, linearMediaAssetToAdd, userId);
                            }
                        }
                        break;
                    default:
                    case eAssetTypes.UNKNOWN:
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddAsset for groupId: {0} and asset: {1}", groupId, assetToAdd.ToString()), ex);
            }

            return result;
        }

        public static GenericResponse<Asset> UpdateAsset(int groupId, long id, Asset assetToUpdate, long userId, bool isFromIngest = false,
                                                        bool isCleared = false, bool isForMigration = false, bool isFromChannel = false)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpdateAsset", groupId);
                    return result;
                }

                // validate that asset exist
                // isAllowedToViewInactiveAssets = true because only operator can update asset
                GenericResponse<Asset> oldAsset = null;

                if (!isForMigration && !isCleared)
                {
                    oldAsset = AssetManager.GetAsset(groupId, id, assetToUpdate.AssetType, true);

                    if (!oldAsset.HasObject())
                    {
                        return oldAsset;
                    }
                }

                assetToUpdate.Id = id;

                switch (assetToUpdate.AssetType)
                {
                    case eAssetTypes.EPG:
                        if (assetToUpdate is EpgAsset)
                        {
                            result = EpgAssetManager.UpdateEpgAsset
                                (groupId, (assetToUpdate as EpgAsset), userId, (oldAsset.Object as EpgAsset), catalogGroupCache);
                        }
                        break;
                    case eAssetTypes.NPVR:
                        break;
                    case eAssetTypes.MEDIA:
                        bool isLinear = assetToUpdate is LiveAsset;
                        MediaAsset mediaAssetToUpdate = assetToUpdate as MediaAsset;
                        MediaAsset currentAsset = null;

                        if (isForMigration || (isFromIngest && isCleared))
                        {
                            currentAsset = new MediaAsset()
                            {
                                MediaType = mediaAssetToUpdate.MediaType,
                                MediaAssetType = mediaAssetToUpdate.MediaAssetType,
                                Description = null
                            };
                        }
                        else
                        {
                            currentAsset = oldAsset.Object as MediaAsset;
                        }

                        // validate that existing asset is indeed linear media
                        if (isLinear && currentAsset.MediaAssetType != MediaAssetType.Linear)
                        {
                            result.SetStatus(eResponseStatus.AssetDoesNotExist, eResponseStatus.AssetDoesNotExist.ToString());
                            return result;
                        }

                        mediaAssetToUpdate.Id = id;
                        if (currentAsset != null && mediaAssetToUpdate != null)
                        {
                            result = UpdateMediaAsset(groupId, ref catalogGroupCache, currentAsset, mediaAssetToUpdate, isLinear, userId, isFromIngest, isForMigration, isFromChannel);
                            if (isLinear && result != null && result.Status != null && result.Status.Code == (int)eResponseStatus.OK)
                            {
                                LiveAsset linearMediaAssetToUpdate = assetToUpdate as LiveAsset;
                                result = UpdateLinearMediaAsset(groupId, result.Object as MediaAsset, linearMediaAssetToUpdate, userId);
                            }
                        }
                        break;
                    default:
                    case eAssetTypes.UNKNOWN:
                        break;
                }

                if (!isFromIngest && result.IsOkStatusCode())
                {
                    // invalidate asset
                    InvalidateAsset(assetToUpdate.AssetType, id);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateAsset for groupId: {0} , id: {1} , assetType: {2}", groupId, id, assetToUpdate.AssetType.ToString()), ex);
            }

            return result;
        }

        public static Status DeleteAsset(int groupId, long id, eAssetTypes assetType, long userId, bool isFromChannel = false)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                // validate that asset exist
                // isAllowedToViewInactiveAssets = true because only operator can delete asset
                List<Asset> assets = AssetManager.GetAssets(groupId, new List<KeyValuePair<eAssetTypes, long>>() { new KeyValuePair<eAssetTypes, long>(assetType, id) }, true);
                if (assets == null || assets.Count != 1 || assets[0] == null || assets[0].IndexStatus == AssetIndexStatus.Deleted)
                {
                    result.Set((int)eResponseStatus.AssetDoesNotExist, eResponseStatus.AssetDoesNotExist.ToString());
                    return result;
                }

                switch (assetType)
                {
                    case eAssetTypes.EPG:
                        result = EpgAssetManager.DeleteEpgAsset(groupId, id, userId);
                        break;
                    case eAssetTypes.NPVR:
                        break;
                    case eAssetTypes.MEDIA:
                        result = DeleteMediaAsset(groupId, id, userId, assets[0] as MediaAsset, isFromChannel);                        
                        break;
                    default:
                    case eAssetTypes.UNKNOWN:
                        break;
                }

                if (result.IsOkStatusCode())
                {
                    // invalidate asset
                    InvalidateAsset(assetType, id);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteTopic for groupId: {0} , id: {1} , assetType: {2}", groupId, id, assetType.ToString()), ex);
            }

            return result;
        }

        internal static bool ClearAsset(int groupId, long id, eAssetTypes assetType, long userId)
        {
            bool result = false;
            try
            {
                if (CatalogDAL.DeleteMediaAsset(groupId, id, userId, true))
                {
                    result = true;
                    InvalidateAsset(assetType, id);
                }
                else
                {
                    log.ErrorFormat("Failed to Clear media asset with id: {0}, groupId: {1}", id, groupId);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed ClearAsset for groupId: {0} , id: {1} , assetType: {2}", groupId, id, assetType.ToString()), ex);
            }

            return result;
        }

        public static Status RemoveTopicsFromAsset(int groupId, long id, eAssetTypes assetType, HashSet<long> topicIds, long userId, bool isFromIngest = false)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                // validate that asset exist
                // isAllowedToViewInactiveAssets = true because only operator can remove topics from asset 
                var currentAsset = AssetManager.GetAsset(groupId, id, assetType, true);
                if (!currentAsset.HasObject())
                {
                    result = new Status((int)eResponseStatus.AssetDoesNotExist, eResponseStatus.AssetDoesNotExist.ToString());
                    return result;
                }

                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling RemoveTopicsFromAsset", groupId);
                    return result;
                }

                // validate not trying to remove basic topicIds    
                var validateBasicTopicsResult = ValidateBasicTopicIdsToRemove(catalogGroupCache, topicIds);
                if (!validateBasicTopicsResult.IsOkStatusCode())
                {
                    result = validateBasicTopicsResult;
                    return result;
                }

                if (isFromIngest)
                {
                    RemoveNoneExistingTopicIds(topicIds, currentAsset.Object, catalogGroupCache);
                    if (topicIds.Count == 0)
                    {
                        result.Set(eResponseStatus.OK);
                        return result;
                    }
                }

                switch (assetType)
                {
                    case eAssetTypes.EPG:
                        result = EpgAssetManager.RemoveTopicsFromProgram(groupId, topicIds, userId, catalogGroupCache, currentAsset.Object);
                        break;
                    case eAssetTypes.MEDIA:
                        result = RemoveTopicsFromMediaAsset(groupId, id, topicIds, userId, catalogGroupCache, currentAsset.Object as MediaAsset, isFromIngest);
                        break;
                    case eAssetTypes.NPVR:
                    case eAssetTypes.UNKNOWN:
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed RemoveTopicsFromAsset for groupId: {0} , id: {1} , assetType: {2}", groupId, id, assetType.ToString()), ex);
            }

            return result;
        }
        
        private static Status RemoveTopicsFromMediaAsset(int groupId, long id, HashSet<long> topicIds, long userId, CatalogGroupCache catalogGroupCache, MediaAsset mediaAsset, bool isFromIngest)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                int dbAssetType = 0;
                
                // validate topicsIds exist on asset
                if (mediaAsset != null)
                {
                    if (!isFromIngest)
                    {
                        result = ValidateNoneExistingTopicIdsToRemove(mediaAsset, topicIds, catalogGroupCache);
                        if (!result.IsOkStatusCode()) { return result; }
                    }

                    List<long> tagIds = catalogGroupCache.TopicsMapById.Where(x => topicIds.Contains(x.Key) && x.Value.Type == MetaType.Tag
                                                                              && !CatalogManager.TopicsToIgnore.Contains(x.Value.SystemName)).Select(x => x.Key).ToList();
                    List<long> metaIds = catalogGroupCache.TopicsMapById.Where(x => topicIds.Contains(x.Key) && x.Value.Type != MetaType.Tag && x.Value.Type != MetaType.ReleatedEntity
                                                                              && !CatalogManager.TopicsToIgnore.Contains(x.Value.SystemName)).Select(x => x.Key).ToList();
                    List<long> releatedEntityIds = catalogGroupCache.TopicsMapById.Where(x => topicIds.Contains(x.Key) && x.Value.Type == MetaType.ReleatedEntity
                                                                              && !CatalogManager.TopicsToIgnore.Contains(x.Value.SystemName)).Select(x => x.Key).ToList();

                    if (CatalogDAL.RemoveMetasAndTagsFromAsset(groupId, id, dbAssetType, metaIds, tagIds, userId, releatedEntityIds))
                    {
                        if (metaIds?.Count > 0 || tagIds?.Count > 0)
                        {
                            CatalogManager.RemoveInheritedValue(groupId, catalogGroupCache, mediaAsset, metaIds, tagIds);
                        }

                        // invalidate asset
                        InvalidateAsset(eAssetTypes.MEDIA, id);

                        //Get updated Asset
                        var assetResponse = AssetManager.GetAsset(groupId, mediaAsset.Id, eAssetTypes.MEDIA, true);
                        if (assetResponse != null && assetResponse.HasObject() && assetResponse.Object is MediaAsset)
                        {
                            // if need UpdateAssetInheritancePolicy
                            UpdateAssetInheritancePolicy(groupId, userId, catalogGroupCache, assetResponse.Object as MediaAsset);
                        }

                        result.Set(eResponseStatus.OK);

                        // UpdateIndex
                        if (!isFromIngest)
                        {
                            bool indexingResult = IndexManager.UpsertMedia(groupId, id);
                            if (!indexingResult)
                            {
                                log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, type: {1}, groupId: {2} after RemoveTopicsFromMediaAsset", id, eAssetTypes.MEDIA.ToString(), groupId);
                            }
                        }
                    }
                    else
                    {
                        log.ErrorFormat("Failed to remove topics from asset with id: {0}, type: {1}, groupId: {2}", id, eAssetTypes.MEDIA.ToString(), groupId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed RemoveTopicsFromMediaAsset for groupId: {0} , id: {1} , assetType: {2}", groupId, id, eAssetTypes.MEDIA.ToString()), ex);
            }

            return result;
        }        

        /// <summary>
        /// Returns dictionary of [assetId, [language, media]] - use in remote task
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> GetGroupMediaAssets(int groupId, long nextId, long pageSize)
        {
            // <assetId, <languageId, media>>
            Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> groupMediaAssetsMap = new Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>>();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetGroupMediaAssets", groupId);
                    return groupMediaAssetsMap;
                }

                DataSet groupAssetsDs = CatalogDAL.GetGroupMediaAssets(groupId, catalogGroupCache.DefaultLanguage.ID, nextId, pageSize);
                groupMediaAssetsMap = CreateGroupMediaMapFromDataSet(groupId, groupAssetsDs, catalogGroupCache);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGroupMediaAssets for groupId: {0}", groupId), ex);
            }

            return groupMediaAssetsMap;
        }

        public static List<Topic> GetBasicMediaAssetTopics()
        {
            List<Topic> result = new List<Topic>();
            foreach (var meta in BasicMediaAssetMetasSystemNameToName)
            {
                Topic topicToAdd = new Topic(meta.Key, true, meta.Value);
                switch (meta.Key)
                {
                    case NAME_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.MultilingualString);
                        topicToAdd.SearchRelated = true;
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_MANDATORY);
                        break;
                    case DESCRIPTION_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.MultilingualString);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_TEXTAREA);
                        break;
                    case EXTERNAL_ID_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.String);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_READONLY);
                        break;
                    case ENTRY_ID_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.String);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        break;
                    case STATUS_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.Bool);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_MANDATORY);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_AVAILABILITY);
                        break;
                    case PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME:
                    case PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME:
                    case CATALOG_START_DATE_TIME_META_SYSTEM_NAME:
                    case CATALOG_END_DATE_TIME_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.DateTime);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_MANDATORY);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_AVAILABILITY);
                        break;
                    case CREATE_DATE_TIME_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.DateTime);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_MANDATORY);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_AVAILABILITY);
                        break;
                    default:
                        throw new Exception(string.Format("missing mapping for metaSystemName: {0} on GetBasicMediaAssetTopics", meta.Key));
                }

                result.Add(topicToAdd);
            }

            return result;
        }      

        #endregion
    }
}