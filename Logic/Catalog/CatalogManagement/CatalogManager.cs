using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Catalog.Cache;
using Core.Catalog.Response;
using GroupsCacheManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Tvinci.Core.DAL;

namespace Core.Catalog.CatalogManagement
{
    public class CatalogManager
    {

        #region Constants and Readonly

        private const string NAME_META_SYSTEM_NAME = "Title";
        private const string DESCRIPTION_META_SYSTEM_NAME = "SummaryMedium";
        private const string EXTERNAL_ID_META_SYSTEM_NAME = "ExternalID";
        private const string ENTRY_ID_META_SYSTEM_NAME = "EntryID";
        private const string STATUS_META_SYSTEM_NAME = "Status";
        private const string PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME = "PlaybackStartDateTime";
        private const string PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME = "PlaybackEndDateTime";
        private const string CATALOG_START_DATE_TIME_META_SYSTEM_NAME = "CatalogStratDateTime";
        private const string CATALOG_END_DATE_TIME_META_SYSTEM_NAME = "CatalogEndDateTime";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly HashSet<string> basicMetasSystemNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            NAME_META_SYSTEM_NAME, DESCRIPTION_META_SYSTEM_NAME, EXTERNAL_ID_META_SYSTEM_NAME, ENTRY_ID_META_SYSTEM_NAME, STATUS_META_SYSTEM_NAME, PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME,
            PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME, CATALOG_START_DATE_TIME_META_SYSTEM_NAME, CATALOG_END_DATE_TIME_META_SYSTEM_NAME
        };

        #endregion

        #region Private Methods

        private static Tuple<CatalogGroupCache, bool> GetCatalogGroupCache(Dictionary<string, object> funcParams)
        {
            bool res = false;
            CatalogGroupCache catalogGroupCache = null;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue && groupId.Value > 0)
                    {
                        List<LanguageObj> languages = CatalogDAL.GetGroupLanguages(groupId.Value);
                        // return false if no languages were found or not only 1 default language found
                        if (languages == null || languages.Count == 0 || languages.Where(x => x.IsDefault).Count() != 1)
                        {
                            return new Tuple<CatalogGroupCache, bool>(catalogGroupCache, false);
                        }

                        DataSet topicsDs = CatalogDAL.GetTopicsByGroupId(groupId.Value);
                        List<Topic> topics = CreateTopicListFromDataSet(topicsDs);
                        // return false if no topics are found on group
                        if (topics == null || topics.Count == 0)
                        {
                            return new Tuple<CatalogGroupCache, bool>(catalogGroupCache, false);
                        }

                        DataSet assetStructsDs = CatalogDAL.GetAssetStructsByGroupId(groupId.Value);
                        List<AssetStruct> assetStructs = CreateAssetStructListFromDataSet(assetStructsDs);
                        // return false if no asset structs are found on group
                        if (assetStructs == null || assetStructs.Count == 0)
                        {
                            return new Tuple<CatalogGroupCache, bool>(catalogGroupCache, false);
                        }

                        catalogGroupCache = new CatalogGroupCache(languages, assetStructs, topics);
                        res = true;
                    }                    
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetCatalogGroupCache failed params : {0}", funcParams != null ? string.Join(";",
                         funcParams.Select(x => string.Format("key:{0}, value: {1}", x.Key, x.Value.ToString())).ToList()) : string.Empty), ex);
            }

            return new Tuple<CatalogGroupCache, bool>(catalogGroupCache, res);
        }

        private static void InvalidateCatalogGroupCache(int groupId, Status resultStatus, bool shouldCheckResultObject, object resultObject = null)
        {
            if (resultStatus != null && resultStatus.Code == (int)eResponseStatus.OK && (!shouldCheckResultObject || resultObject != null))
            {
                string invalidationKey = LayeredCacheKeys.GetCatalogGroupCacheInvalidationKey(groupId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for catalogGroupCache with invalidationKey: {0}", invalidationKey);
                }
            }
        }

        private static Status CreateAssetStructResponseStatusFromResult(long result, Status status = null)
        {
            Status responseStatus = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            switch (result)
            {
                case -111:
                    responseStatus = new Status((int)eResponseStatus.AssetStructNameAlreadyInUse, eResponseStatus.AssetStructNameAlreadyInUse.ToString());
                    break;
                case -222:
                    responseStatus = new Status((int)eResponseStatus.AssetStructSystemNameAlreadyInUse, eResponseStatus.AssetStructSystemNameAlreadyInUse.ToString());
                    break;
                case -333:
                    responseStatus = new Status((int)eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString());
                    break;
                default:
                    if (status != null)
                    {
                        responseStatus = status;
                    }

                    break;
            }

            return responseStatus;
        }

        private static AssetStruct CreateAssetStruct(long id, DataRow dr, List<DataRow> assetStructTranslations)
        {
            AssetStruct result = null;
            if (id > 0)
            {
                string name = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION");
                string systemName = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(systemName))
                {
                    bool isPredefined = ODBCWrapper.Utils.ExtractBoolean(dr, "IS_BASIC");
                    DateTime? createDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "CREATE_DATE");
                    DateTime? updateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "UPDATE_DATE");
                    List<LanguageContainer> namesInOtherLanguages = new List<LanguageContainer>();
                    if (assetStructTranslations != null && assetStructTranslations.Count > 0)
                    {
                        foreach (DataRow translationDr in assetStructTranslations)
                        {
                            string languageCode = ODBCWrapper.Utils.GetSafeStr(translationDr, "CODE3");
                            string translation = ODBCWrapper.Utils.GetSafeStr(translationDr, "TRANSLATION");
                            if (!string.IsNullOrEmpty(languageCode) && !string.IsNullOrEmpty(translation))
                            {
                                namesInOtherLanguages.Add(new LanguageContainer(languageCode, translation));
                            }
                        }
                    }
                    result = new AssetStruct(id, name, namesInOtherLanguages, systemName, isPredefined, createDate.HasValue ? ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(createDate.Value) : 0,
                                             updateDate.HasValue ? ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(updateDate.Value) : 0);
                }
            }

            return result;
        }

        private static Status ValidateBasicMetaIds(CatalogGroupCache catalogGroupCache, AssetStruct assetStructToValidate)
        {
            Status result = new Status((int)eResponseStatus.AssetStructMissingBasicMetaIds, eResponseStatus.AssetStructMissingBasicMetaIds.ToString());
            List<long> basicMetaIds = new List<long>();
            if (catalogGroupCache.TopicsMapBySystemName != null && catalogGroupCache.TopicsMapBySystemName.Count > 0)
            {
                basicMetaIds = catalogGroupCache.TopicsMapBySystemName.Where(x => basicMetasSystemNames.Contains(x.Key.ToLower())).Select(x => x.Value.Id).ToList();
                if (assetStructToValidate.MetaIds != null)
                {
                    List<long> noneExistingBasicMetaIds = basicMetaIds.Except(assetStructToValidate.MetaIds).ToList();
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

        private static AssetStructResponse CreateAssetStructResponseFromDataSet(DataSet ds)
        {
            AssetStructResponse response = new AssetStructResponse();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID", 0);
                    if (id > 0)
                    {
                        EnumerableRowCollection<DataRow> translations = ds.Tables.Count > 1 ? ds.Tables[1].AsEnumerable() : new DataTable().AsEnumerable();
                        List<DataRow> assetStructTranslations = (from row in translations
                                                           select row).ToList();
                        response.AssetStruct = CreateAssetStruct(id, dt.Rows[0], assetStructTranslations);
                    }
                    else
                    {
                        response.Status = CreateAssetStructResponseStatusFromResult(id);
                    }
                }
                /// assetStruct does not exist
                else
                {
                    response.Status = CreateAssetStructResponseStatusFromResult(0, new Status((int)eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString()));
                }

                if (response.AssetStruct != null && ds.Tables.Count == 3)
                {
                    DataTable metasDt = ds.Tables[2];
                    if (response.AssetStruct != null && metasDt != null && metasDt.Rows != null && metasDt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in metasDt.Rows)
                        {
                            long assetStructId = ODBCWrapper.Utils.GetLongSafeVal(dr, "TEMPLATE_ID", 0);
                            long metaId = ODBCWrapper.Utils.GetLongSafeVal(dr, "TOPIC_ID", 0);
                            if (!response.AssetStruct.MetaIds.Contains(metaId))
                            {
                                response.AssetStruct.MetaIds.Add(metaId);
                            }
                        }
                    }
                }

                if (response.AssetStruct != null)
                {
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }

            return response;
        }

        private static List<AssetStruct> CreateAssetStructListFromDataSet(DataSet ds)
        {
            List <AssetStruct> response = null;
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                Dictionary<long, AssetStruct> idToAssetStructMap = new Dictionary<long, AssetStruct>();
                DataTable dt = ds.Tables[0];
                EnumerableRowCollection<DataRow> translations = ds.Tables.Count > 2 ? ds.Tables[2].AsEnumerable() : new DataTable().AsEnumerable();
                if (dt != null && dt.Rows != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                        if (id > 0 && !idToAssetStructMap.ContainsKey(id))
                        {
                            List<DataRow> assetStructTranslations = (from row in translations
                                                               where (Int64)row["TEMPLATE_ID"] == id
                                                               select row).ToList();
                            AssetStruct assetStruct = CreateAssetStruct(id, dr, assetStructTranslations);
                            if (assetStruct != null)
                            {
                                idToAssetStructMap.Add(id, assetStruct);
                            }
                        }
                    }
                }

                DataTable metasDt = ds.Tables[1];
                if (metasDt != null && metasDt.Rows != null && metasDt.Rows.Count > 0 && idToAssetStructMap.Count > 0)
                {
                    Dictionary<long, Dictionary<int, long>> assetStructOrderedMetasMap = new Dictionary<long, Dictionary<int, long>>();
                    foreach (DataRow dr in metasDt.Rows)
                    {
                        long assetStructId = ODBCWrapper.Utils.GetLongSafeVal(dr, "TEMPLATE_ID", 0);
                        long metaId = ODBCWrapper.Utils.GetLongSafeVal(dr, "TOPIC_ID", 0);
                        int order = ODBCWrapper.Utils.GetIntSafeVal(dr, "ORDER", 0);
                        if (assetStructId > 0 && metaId > 0 && order > 0 && idToAssetStructMap.ContainsKey(assetStructId))
                        {
                            if (assetStructOrderedMetasMap.ContainsKey(assetStructId))
                            {
                                assetStructOrderedMetasMap[assetStructId][order] = metaId;
                            }
                            else
                            {
                                assetStructOrderedMetasMap.Add(assetStructId, new Dictionary<int, long>() { { order, metaId } });
                            }                            
                        }
                    }

                    foreach (AssetStruct assetStruct in idToAssetStructMap.Values)
                    {
                        if (assetStructOrderedMetasMap.ContainsKey(assetStruct.Id))
                        {
                            assetStruct.MetaIds = assetStructOrderedMetasMap[assetStruct.Id].OrderBy(x => x.Key).Select(x => x.Value).ToList();
                        }
                    }
                }

                response = idToAssetStructMap.Values.ToList();                
            }

            return response;
        }        

        private static Status CreateTopicResponseStatusFromResult(long result)
        {
            Status responseStatus = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            switch (result)
            {
                case -222:
                    responseStatus = new Status((int)eResponseStatus.MetaSystemNameAlreadyInUse, eResponseStatus.MetaSystemNameAlreadyInUse.ToString());
                    break;
                case -333:
                    responseStatus = new Status((int)eResponseStatus.MetaDoesNotExist, eResponseStatus.MetaDoesNotExist.ToString());
                    break;
                default:
                    break;
            }

            return responseStatus;
        }

        private static Topic CreateTopic(long id, DataRow dr, List<DataRow> topicTranslations)
        {
            Topic result = null;
            if (id > 0)
            {
                string name = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                string systemName = ODBCWrapper.Utils.GetSafeStr(dr, "SYSTEM_NAME");
                int topicType = ODBCWrapper.Utils.GetIntSafeVal(dr, "TOPIC_TYPE_ID", 0);                
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(systemName) && topicType > 0 && typeof(MetaType).IsEnumDefined(topicType))
                {
                    string commaSeparatedFeatures = ODBCWrapper.Utils.GetSafeStr(dr, "FEATURES");
                    HashSet<string> features = null;
                    if (!string.IsNullOrEmpty(commaSeparatedFeatures))
                    {
                        features = new HashSet<string>(commaSeparatedFeatures.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    bool isPredefined = ODBCWrapper.Utils.ExtractBoolean(dr, "IS_BASIC");                    
                    string helpText = ODBCWrapper.Utils.GetSafeStr(dr, "HELP_TEXT");
                    long parentId = ODBCWrapper.Utils.GetLongSafeVal(dr, "PARENT_TOPIC_ID", 0);
                    DateTime? createDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "CREATE_DATE");
                    DateTime? updateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "UPDATE_DATE");
                    List<LanguageContainer> namesInOtherLanguages = new List<LanguageContainer>();
                    if (topicTranslations != null && topicTranslations.Count > 0)
                    {
                        foreach (DataRow translationDr in topicTranslations)
                        {
                            string languageCode = ODBCWrapper.Utils.GetSafeStr(translationDr, "CODE3");
                            string translation = ODBCWrapper.Utils.GetSafeStr(translationDr, "TRANSLATION");
                            if (!string.IsNullOrEmpty(languageCode) && !string.IsNullOrEmpty(translation))
                            {
                                namesInOtherLanguages.Add(new LanguageContainer(languageCode, translation));
                            }
                        }
                    }

                    result = new Topic(id, name, namesInOtherLanguages, systemName, (MetaType)topicType, features, isPredefined, helpText, parentId,
                                        createDate.HasValue ? ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(createDate.Value) : 0,
                                        updateDate.HasValue ? ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(updateDate.Value) : 0);
                }
            }

            return result;
        }

        private static TopicResponse CreateTopicResponseFromDataSet(DataSet ds)
        {
            TopicResponse response = new TopicResponse();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID", 0);
                    if (id > 0)
                    {                        
                        EnumerableRowCollection<DataRow> translations = ds.Tables.Count == 2 ? ds.Tables[1].AsEnumerable() : new DataTable().AsEnumerable();
                        List<DataRow> topicTranslations = (from row in translations
                                                           select row).ToList();
                        response.Topic = CreateTopic(id, dt.Rows[0], topicTranslations);
                    }
                    else
                    {
                        response.Status = CreateTopicResponseStatusFromResult(id);
                    }
                }

                if (response.Topic != null)
                {
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }            

            return response;
        }

        private static List<Topic> CreateTopicListFromDataSet(DataSet ds)
        {
            List<Topic> response = null;
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable topicsDt = ds.Tables[0];                                
                EnumerableRowCollection<DataRow> translations = ds.Tables.Count == 2 ? ds.Tables[1].AsEnumerable() : new DataTable().AsEnumerable();
                if (topicsDt != null && topicsDt.Rows != null)
                {
                    response = new List<Topic>();
                    foreach (DataRow dr in topicsDt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                        if (id > 0)
                        {
                            List<DataRow> topicTranslations = (from row in translations
                                                               where (Int64)row["TOPIC_ID"] == id
                                                               select row).ToList();
                            Topic topic = CreateTopic(id, dr, topicTranslations);
                            if (topic != null)
                            {
                                response.Add(topic);
                            }
                        }
                    }
                }
            }            

            return response;
        }        

        private static AssetResponse CreateAssetResponseFromDataSet(int groupId, DataSet ds, LanguageObj defaultLanguage, List<LanguageObj> groupLanguages)
        {
            AssetResponse result = new AssetResponse();
            if (ds == null || ds.Tables == null)
            {
                log.WarnFormat("CreateAssetResponseFromDataSet - dataset or tables are null");
                return result;                
            }

            // Basic details tables
            if (ds.Tables[0] == null || ds.Tables[0].Rows == null || ds.Tables[0].Rows.Count != 1)
            {
                log.WarnFormat("CreateAssetResponseFromDataSet - basic details table is not valid");
                return result;
            }

            DataRow basicDataRow = ds.Tables[0].Rows[0];
            long id = ODBCWrapper.Utils.GetLongSafeVal(basicDataRow, "ID", 0);
            if (id <= 0)
            {
                result.Status = CreateAssetResponseStatusFromResult(id);
                return result;
            }

            result.Asset = CreateMediaAsset(groupId, id, ds, defaultLanguage, groupLanguages);

            if (result.Asset != null)
            {
                result.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return result;
        }

        private static MediaAsset CreateMediaAsset(int groupId, long id, DataSet ds, LanguageObj defaultLanguage, List<LanguageObj> groupLanguages)
        {
            MediaAsset result = null;
            
            if (ds.Tables.Count < 3)
            {
                log.WarnFormat("CreateMediaAssetFromDataSet didn't receive dataset with 6 or more tables");
                return result;
            }

            DataRow basicDataRow = ds.Tables[0].Rows[0];
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

            // Pictures table
            List<Picture> pictures = null;
            //if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
            //{
            //    bool picturesResult = false;
            //    pictures = CatalogLogic.GetAllPic(groupId, (int)id, ds.Tables[1], ref picturesResult, groupId);
            //    if (!picturesResult)
            //    {
            //        log.WarnFormat("CreateMediaAssetFromDataSet - failed to get pictures for Id: {0}", id);
            //        return null;
            //    }
            //}

            // Files table
            List<FileMedia> files = null;
            //if (ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
            //{
            //    bool filesResult = false;
            //    List<Branding> branding = new List<Branding>();
            //    Dictionary<int, List<string>> mediaFilePPVModules = mediaFilePPVModules = CatalogLogic.GetMediaFilePPVModules(ds.Tables[2]);
            //    // TODO - Lior - what to do with management data
            //    files = CatalogLogic.FilesValues(ds.Tables[2], ref branding, false, ref filesResult, true, mediaFilePPVModules);
            //    if (!filesResult)
            //    {
            //        log.WarnFormat("CreateMediaAssetFromDataSet - failed to get files for Id: {0}", id);
            //        return null;
            //    }
            //}

            // Metas and Tags table
            List<Metas> metas = null;
            List<Tags> tags = null;
            Dictionary<string, int> tagNameToIdMap = null;
            DataTable metasTable = new DataTable();
            DataTable tagsTable = new DataTable();
            if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
            {
                metasTable = ds.Tables[1];
            }
            if (ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
            {
                tagsTable = ds.Tables[2];
            }

            if (!TryGetMetasAndTags(groupId, id, defaultLanguage.ID, groupLanguages, metasTable, tagsTable, ref metas, ref tags, ref tagNameToIdMap))
            {
                log.WarnFormat("CreateMediaAssetFromDataSet - failed to get media metas and tags for Id: {0}", id);
                return null;
            }

            string name = string.Empty;
            string description = string.Empty;
            List<LanguageContainer> namesWithLanguages = null;
            List<LanguageContainer> descriptionsWithLanguages = null;
            if (!ExtractMediaAssetNamesAndDescriptionsFromMetas(metas, ref name, ref description, ref namesWithLanguages, ref descriptionsWithLanguages))
            {
                log.WarnFormat("Title is not valid for media with Id: {0}", id);
                return result;
            }

            string entryId = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "ENTRY_ID");
            string coGuid = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "CO_GUID");
            bool isActive = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "IS_ACTIVE", 0) == 1;
            int? deviceRuleId = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "DEVICE_RULE_ID", -1);
            int? geoBlockRuleId = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "GEO_BLOCK_RULE_ID", -1);
            string userTypes = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "user_types");
            result = new MediaAsset(id, eAssetTypes.MEDIA, name, namesWithLanguages, description, descriptionsWithLanguages, createDate, updateDate, startDate, endDate, metas, tags, pictures, coGuid,
                                    isActive, catalogStartDate, finalEndDate, mediaType, entryId, deviceRuleId == -1 ? null : deviceRuleId, geoBlockRuleId == -1 ? null : geoBlockRuleId, files, userTypes);

            return result;
        }            

        private static bool TryGetMediaTypeFromAssetStructId(int groupId, long assetStructId, out MediaType mediaType)
        {
            bool res = false;
            mediaType = null;
            CatalogGroupCache catalogGroupCache;
            if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
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
                                                ref List<Metas> metas, ref List<Tags> tags, ref Dictionary<string, int> tagNameToIdMap)
        {
            bool res = false;
            Dictionary<long, List<LanguageContainer>> topicIdToMeta = new Dictionary<long, List<LanguageContainer>>();
            foreach (DataRow dr in metasTable.Rows)
            {
                long topicId = ODBCWrapper.Utils.GetLongSafeVal(dr, "topic_id");
                long languageId = ODBCWrapper.Utils.GetLongSafeVal(dr, "language_id");
                string translation = ODBCWrapper.Utils.GetSafeStr(dr, "translation");
                LanguageObj language = groupLanguages.Where(x => x.ID == languageId).FirstOrDefault();
                if (!topicIdToMeta.ContainsKey(topicId))
                {

                    topicIdToMeta.Add(topicId, new List<LanguageContainer>() { new LanguageContainer(language.Code, translation, language.IsDefault) });
                }
                else
                {
                    topicIdToMeta[topicId].Add(new LanguageContainer(language.Code, translation, language.IsDefault));
                }
            }

            // TODO - Lior - Remove TagId, not needed
            Dictionary<long, Dictionary<long, List<LanguageContainer>>> topicIdToTag = new Dictionary<long, Dictionary<long, List<LanguageContainer>>>();
            foreach (DataRow dr in tagsTable.Rows)
            {
                long topicId = ODBCWrapper.Utils.GetLongSafeVal(dr, "topic_id");
                long tagId = ODBCWrapper.Utils.GetLongSafeVal(dr, "tag_id");
                long languageId = ODBCWrapper.Utils.GetLongSafeVal(dr, "language_id");
                string translation = ODBCWrapper.Utils.GetSafeStr(dr, "translation");
                LanguageObj language = groupLanguages.Where(x => x.ID == languageId).FirstOrDefault();
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

            List<long> topicIds = new List<long>();
            topicIds.AddRange(topicIdToMeta.Keys.ToList());
            topicIds.AddRange(topicIdToTag.Keys.ToList());
            if (topicIds.Count > 0)
            {
                TopicListResponse groupTopicsResponse = GetTopicsByIds(groupId, topicIds, MetaType.All);
                if (groupTopicsResponse != null && groupTopicsResponse.Status != null && groupTopicsResponse.Status.Code == (int)eResponseStatus.OK
                    && groupTopicsResponse.Topics != null && groupTopicsResponse.Topics.Count > 0)
                {
                    metas = new List<Metas>();
                    tags = new List<Tags>();                    
                    foreach (Topic topic in groupTopicsResponse.Topics)
                    {
                        if (topic.Type == MetaType.Tag)
                        {
                            if (topicIdToTag.ContainsKey(topic.Id))
                            {
                                Dictionary<long, List<LanguageContainer>> topicTags = topicIdToTag[topic.Id];
                                List<LanguageContainer> defaultLanugeValues = topicTags.SelectMany(x => x.Value.Where(y => y.IsDefault)).ToList();
                                List<string> defaultValues = defaultLanugeValues.Select(x => x.Value).ToList();
                                List<LanguageContainer[]> tagLanguages = topicIdToTag[topic.Id].Select(x => x.Value.Select(y => y).ToArray()).ToList();
                                tags.Add(new Tags(new TagMeta(topic.SystemName, topic.Type.ToString()), defaultValues, tagLanguages));
                            }
                        }
                        else
                        {
                            if (topicIdToMeta.ContainsKey(topic.Id))
                            {
                                List<LanguageContainer> topicLanguages = null;
                                string defaultValue = topicIdToMeta[topic.Id].Where(x => x.IsDefault == true).Select(x => x.Value).FirstOrDefault();
                                if (topic.Type == MetaType.MultilingualString)
                                {
                                    topicLanguages = topicIdToMeta[topic.Id];
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
                if (ds == null || ds.Tables == null || ds.Tables.Count < 3)
                {
                    log.WarnFormat("CreateGroupMediaMapFromDataSet didn't receive dataset with 3 or more tables");
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

                foreach (DataRow basicDataRow in ds.Tables[0].Rows)
                {
                    int id = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "ID", 0);
                    if (id > 0 && !groupAssetsMap.ContainsKey(id))
                    {
                        DataSet assetDataset = new DataSet();
                        DataTable basicDataTable = ds.Tables[0].Clone();
                        basicDataTable.ImportRow(basicDataRow);
                        assetDataset.Tables.Add(basicDataTable);
                        if (metas.Count() > 0)
                        {
                            EnumerableRowCollection<DataRow> assetMetas = (from row in metas
                                                                           where (Int64)row["ASSET_ID"] == id
                                                                           select row);                   
                            if (assetMetas != null && assetMetas.Count() > 0)
                            {                                
                                assetDataset.Tables.Add(assetMetas.CopyToDataTable());
                            }
                            else
                            {
                                assetDataset.Tables.Add(ds.Tables[1].Clone());
                            }
                        }

                        if (tags.Count() > 0)
                        {
                            EnumerableRowCollection<DataRow> assettags = (from row in tags
                                                                          where (Int64)row["ASSET_ID"] == id
                                                                          select row);
                            if (assettags != null && assettags.Count() > 0)
                            {
                                assetDataset.Tables.Add(assettags.CopyToDataTable());
                            }
                            else
                            {
                                assetDataset.Tables.Add(ds.Tables[2].Clone());
                            }
                        }

                        MediaAsset mediaAsset = CreateMediaAsset(groupId, id, assetDataset, catalogGroupCache.DefaultLanguage, catalogGroupCache.LanguageMapById.Values.ToList());
                        if (mediaAsset != null)
                        {
                            Dictionary<int, ApiObjects.SearchObjects.Media> assets = CreateMediasFromMediaAssetAndLanguages(groupId, mediaAsset, catalogGroupCache);
                            groupAssetsMap.Add((int)mediaAsset.Id, assets);
                        }
                    }

                    //get all the media files types for each mediaId that have been selected.
                    //if (ds.Tables[1] != null && ds.Tables[1].Columns != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                    //{
                    //    foreach (DataRow row in ds.Tables[1].Rows)
                    //    {
                    //        int mediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                    //        string sMFT = ODBCWrapper.Utils.GetSafeStr(row, "media_type_id");
                    //        bool isTypeFree = ODBCWrapper.Utils.ExtractBoolean(row, "is_free");

                    //        if (groupAssetsMap.ContainsKey(mediaID) && groupAssetsMap[mediaID].ContainsKey(defaultLanguage.ID))
                    //        {
                    //            ApiObjects.SearchObjects.Media theMedia = groupAssetsMap[mediaID][defaultLanguage.ID];
                    //            theMedia.m_sMFTypes += string.Format("{0};", sMFT);
                    //            int mediaTypeId;
                    //            if (isTypeFree)
                    //            {
                    //                // if at least one of the media types is free - this media is free
                    //                theMedia.isFree = true;

                    //                if (int.TryParse(sMFT, out mediaTypeId))
                    //                {
                    //                    theMedia.freeFileTypes.Add(mediaTypeId);
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed CreateGroupMediaMapFromDataSet for groupId: {0}", groupId), ex);
            }

            return groupAssetsMap;
        }

        private static Dictionary<int, ApiObjects.SearchObjects.Media> CreateMediasFromMediaAssetAndLanguages(int groupId, MediaAsset mediaAsset, CatalogGroupCache catalogGroupCache)
        {            
            Dictionary<int, ApiObjects.SearchObjects.Media> result = new Dictionary<int, ApiObjects.SearchObjects.Media>();            
            foreach (LanguageObj language in catalogGroupCache.LanguageMapById.Values)
            {
                string name = mediaAsset.Name;
                string description = mediaAsset.Description;
                Dictionary<string, string> metas = new Dictionary<string, string>();
                Dictionary<string, HashSet<string>> tags = new Dictionary<string, HashSet<string>>();
                if (!language.IsDefault)
                {
                    if (mediaAsset.NamesWithLanguages.Where(x => x.LanguageCode == language.Code).Count() == 1)
                    {
                        name = mediaAsset.NamesWithLanguages.Where(x => x.LanguageCode == language.Code).Select(x => x.Value).First();
                    }

                    if (mediaAsset.DescriptionsWithLanguages.Where(x => x.LanguageCode == language.Code).Count() == 1)
                    {
                        description = mediaAsset.DescriptionsWithLanguages.Where(x => x.LanguageCode == language.Code).Select(x => x.Value).First();
                    }
                }

                if (mediaAsset.Metas != null && mediaAsset.Metas.Count > 0)
                {
                    if (language.IsDefault)
                    {
                        metas = mediaAsset.Metas.ToDictionary(x => x.m_oTagMeta.m_sName, x => x.m_sValue);
                    }
                    else
                    {
                        List<Metas> languageMetas = mediaAsset.Metas.Where(x => x.Value != null && x.Value.Where(y => y.LanguageCode == language.Code).Count() == 1).ToList();
                        metas = languageMetas.ToDictionary(x => x.m_oTagMeta.m_sName, x => x.Value.Where(y => y.LanguageCode == language.Code).Select(y => y.Value).First());
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
                                                                                        x => new HashSet<string>(x.Values.SelectMany(y => y.Where(z => z.LanguageCode == language.Code)
                                                                                                                                        .Select(z => z.Value)).ToList(), StringComparer.OrdinalIgnoreCase));
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
                    m_dTagValues = tags           
                };

                result.Add(language.ID, media);
            }

            return result;
        }        

        private static Type GetMetaType(Topic topic)
        {
            Type res = typeof(string);
            switch (topic.Type)
            {
                case MetaType.String:
                case MetaType.MultilingualString:
                case MetaType.Tag:
                    res = typeof(string);
                    break;                
                case MetaType.Number:
                    res = typeof(double);
                    break;
                case MetaType.Bool:
                    res = typeof(int);
                    break;                    
                case MetaType.DateTime:
                    res = typeof(DateTime);
                    break;                    
                case MetaType.All:                    
                default:
                    break;
            }

            return res;
        }

        private static Status ValidateAsset(int groupId, CatalogGroupCache catalogGroupCache, ref AssetStruct assetStruct, MediaAsset asset, ref XmlDocument metasXmlDoc, ref XmlDocument tagsXmlDoc)
        {
            // TODO - Lior - change error codes and add new ones
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            HashSet<long> assetStructMetaIds = new HashSet<long>(assetStruct.MetaIds);
            result = ValidateAssetMetasAndTagsNamesAndTypes(catalogGroupCache, ref asset, assetStructMetaIds, ref metasXmlDoc, ref tagsXmlDoc);
            if (result.Code != (int)eResponseStatus.OK)
            {
                return result;
            }

            // validate device rule id
            if (asset.DeviceRuleId.HasValue && (asset.DeviceRuleId.Value <= 0 || !CatalogLogic.ValidateDeviceRuleExists(groupId, asset.DeviceRuleId.Value)))
            {
                result = new Status((int)eResponseStatus.DeviceRuleDoesNotExistForGroup, eResponseStatus.DeviceRuleDoesNotExistForGroup.ToString());
                return result;            
            }

            // validate geoblock rule id
            if (asset.GeoBlockRuleId.HasValue && (asset.GeoBlockRuleId.Value <= 0 || !CatalogLogic.ValidateGeoBlockRuleExists(groupId, asset.GeoBlockRuleId.Value)))
            {
                result = new Status((int)eResponseStatus.GeoBlockRuleDoesNotExistForGroup, eResponseStatus.GeoBlockRuleDoesNotExistForGroup.ToString());
                return result;
            }

            return result;
        }

        private static Status ValidateAssetMetasAndTagsNamesAndTypes(CatalogGroupCache catalogGroupCache, ref MediaAsset asset, HashSet<long> assetStructMetaIds, ref XmlDocument metasXmlDoc, ref XmlDocument tagsXmlDoc)
        {
            // TODO - Lior - change error codes and add new ones
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            HashSet<string> tempHashSet = new HashSet<string>();
            if (asset.Metas != null && asset.Metas.Count > 0)
            {
                metasXmlDoc = new XmlDocument();
                XmlNode rootNode = metasXmlDoc.CreateElement("root");
                metasXmlDoc.AppendChild(rootNode);
                foreach (Metas meta in asset.Metas)
                {
                    // validate duplicates do not exist
                    if (tempHashSet.Contains(meta.m_oTagMeta.m_sName))
                    {
                        result.Message = string.Format("Duplicate meta sent, meta name: {0}", meta.m_oTagMeta.m_sName);
                        return result;
                    }

                    tempHashSet.Add(meta.m_oTagMeta.m_sName);

                    // validate meta exists on group
                    if (!catalogGroupCache.TopicsMapBySystemName.ContainsKey(meta.m_oTagMeta.m_sName))
                    {
                        result.Message = string.Format("meta: {0} does not exist for group", meta.m_oTagMeta.m_sName);
                        return result;
                    }

                    // validate meta exists on asset struct
                    if (!assetStructMetaIds.Contains(catalogGroupCache.TopicsMapBySystemName[meta.m_oTagMeta.m_sName].Id))
                    {
                        result.Message = string.Format("meta: {0} is not part of assetStruct", meta.m_oTagMeta.m_sName);
                        return result;
                    }

                    Topic topic = catalogGroupCache.TopicsMapBySystemName[meta.m_oTagMeta.m_sName];
                    // validate correct type was sent
                    if (topic.Type.ToString().ToLower() != meta.m_oTagMeta.m_sType.ToLower())
                    {
                        result = new Status((int)eResponseStatus.InvalidMetaType, string.Format("{0} was sent for meta: {1}", eResponseStatus.InvalidMetaType.ToString(), meta.m_oTagMeta.m_sName));
                        return result;
                    }

                    // Validate meta values are correct and add to to metaXml
                    if (!IsMetaValueValid(ref asset, meta, topic.Id, catalogGroupCache.DefaultLanguage.ID, catalogGroupCache.LanguageMapByCode, ref result, ref metasXmlDoc, ref rootNode))
                    {
                        return result;
                    }
                }

                tempHashSet.Clear();
            }

            if (asset.Tags != null && asset.Tags.Count > 0)
            {
                tagsXmlDoc = new XmlDocument();
                XmlNode rootNode = tagsXmlDoc.CreateElement("root");
                tagsXmlDoc.AppendChild(rootNode);
                foreach (Tags tag in asset.Tags)
                {
                    // validate duplicates do not exist
                    if (tempHashSet.Contains(tag.m_oTagMeta.m_sName))
                    {
                        result.Message = string.Format("Duplicate tag sent, tag name: {0}", tag.m_oTagMeta.m_sName);
                        return result;
                    }

                    tempHashSet.Add(tag.m_oTagMeta.m_sName);

                    // validate tag exists on group
                    if (!catalogGroupCache.TopicsMapBySystemName.ContainsKey(tag.m_oTagMeta.m_sName))
                    {
                        result.Message = string.Format("tag: {0} does not exist for group", tag.m_oTagMeta.m_sName);
                        return result;
                    }

                    // validate tag exists on asset struct
                    if (!assetStructMetaIds.Contains(catalogGroupCache.TopicsMapBySystemName[tag.m_oTagMeta.m_sName].Id))
                    {
                        result.Message = string.Format("tag: {0} is not part of assetStruct", tag.m_oTagMeta.m_sName);
                        return result;
                    }

                    Topic topic = catalogGroupCache.TopicsMapBySystemName[tag.m_oTagMeta.m_sName];
                    // validate correct type was sent
                    if (topic.Type != MetaType.Tag || topic.Type.ToString().ToLower() != tag.m_oTagMeta.m_sType.ToLower())
                    {
                        result = new Status((int)eResponseStatus.InvalidMetaType, string.Format("{0} was sent for meta: {1}", eResponseStatus.InvalidMetaType.ToString(), tag.m_oTagMeta.m_sName));
                        return result;
                    }

                    // insert default language values into tagsXml
                    foreach (string tagValue in tag.m_lValues)
                    {
                        AddTopicLanguageValueToXml(tagsXmlDoc, rootNode, topic.Id, catalogGroupCache.DefaultLanguage.ID, tagValue);
                    }

                    // insert other language values into tagsXml
                    foreach (LanguageContainer[] languageArray in tag.Values)
                    {
                        foreach (LanguageContainer language in languageArray)
                        {
                            if (catalogGroupCache.LanguageMapByCode.ContainsKey(language.LanguageCode))
                            {
                                AddTopicLanguageValueToXml(tagsXmlDoc, rootNode, topic.Id,catalogGroupCache.LanguageMapByCode[language.LanguageCode].ID, language.Value);
                            }
                        }
                    }
                }
            }

            result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            return result;
        }

        private static bool IsMetaValueValid(ref MediaAsset asset, Metas meta, long topicId, int defaultLanguageId, Dictionary<string, LanguageObj> LanguageMapByCode, ref Status resultStatus,
                                                ref XmlDocument metasXmlDoc, ref XmlNode rootNode)
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
                    int intVal;
                    isValidMetaValue = int.TryParse(meta.m_sValue, out intVal);                    
                    break;
                case MetaType.DateTime:
                    isValidMeta = true;
                    DateTime dateTimeVal;
                    isValidMetaValue = DateTime.TryParse(meta.m_sValue, out dateTimeVal);
                    break;
                default:
                case MetaType.All:
                case MetaType.Tag:
                    break;
            }

            if (!isValidMeta)
            {
                resultStatus = new Status((int)eResponseStatus.InvalidMetaType, string.Format("{0} was sent for meta: {1}", eResponseStatus.InvalidMetaType.ToString(), meta.m_oTagMeta.m_sName));
                return false;
            }

            if (!isValidMetaValue)
            {
                resultStatus = new Status((int)eResponseStatus.InvalidValueSentForMeta, string.Format("{0} metaName: {1}", eResponseStatus.InvalidValueSentForMeta.ToString(), meta.m_oTagMeta.m_sName));
                return false;
            }

            if (basicMetasSystemNames.Contains(meta.m_oTagMeta.m_sName))
            {
                switch (meta.m_oTagMeta.m_sName)
                {
                    case ENTRY_ID_META_SYSTEM_NAME:
                        asset.EntryId = meta.m_sValue;
                        break;
                    case STATUS_META_SYSTEM_NAME:
                        bool isActive;
                        if (bool.TryParse(meta.m_sValue, out isActive))
                        {
                            asset.IsActive = isActive;
                        }
                        else
                        {
                            log.ErrorFormat("IsMetaValueValid failed to parse {0} meta, value: {1}", STATUS_META_SYSTEM_NAME, meta.m_sValue);
                        }               
                        break;
                    case CATALOG_START_DATE_TIME_META_SYSTEM_NAME:
                        DateTime catalogStartDate;
                        if (DateTime.TryParse(meta.m_sValue, out catalogStartDate))
                        {
                            asset.CatalogStartDate = catalogStartDate;
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
                            asset.FinalEndDate = finalEndDate;
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
                    if (LanguageMapByCode.ContainsKey(language.LanguageCode))
                    {
                        AddTopicLanguageValueToXml(metasXmlDoc, rootNode, topicId, LanguageMapByCode[language.LanguageCode].ID, language.Value);
                    }
                }
            }

            return true;
        }

        private static void AddTopicLanguageValueToXml(XmlDocument metasXmlDoc, XmlNode rootNode, long topicId, int defaultLanguageId, string value)
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
            languageIdNode.InnerText = defaultLanguageId.ToString();
            rowNode.AppendChild(languageIdNode);
            valueNode = metasXmlDoc.CreateElement("value");
            valueNode.InnerText = value;
            rowNode.AppendChild(valueNode);
            rootNode.AppendChild(rowNode);
        }

        private static bool ExtractMediaAssetNamesAndDescriptionsFromMetas(List<Metas> metas, ref string name, ref string description, ref List<LanguageContainer> namesWithLanguages,
                                                                            ref List<LanguageContainer> descriptionsWithLanguages)
        {
            if (metas != null && metas.Count > 0)
            {
                Metas nameMeta = metas.Where(x => x.m_oTagMeta.m_sName.ToLower() == NAME_META_SYSTEM_NAME.ToLower()).FirstOrDefault();
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

                Metas descMeta = metas.Where(x => x.m_oTagMeta.m_sName.ToLower() == DESCRIPTION_META_SYSTEM_NAME.ToLower()).FirstOrDefault();
                if (descMeta != null && !string.IsNullOrEmpty(descMeta.m_sValue))
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

        private static void ExtractTopicLanguageAndValuesFromMediaAsset(MediaAsset asset, CatalogGroupCache catalogGroupCache, ref XmlDocument xmlDoc, string topicSystemName)
        {
            // Add Name meta values (for languages that are not default)
            if (catalogGroupCache.TopicsMapBySystemName.ContainsKey(topicSystemName))
            {
                Topic topic = catalogGroupCache.TopicsMapBySystemName[topicSystemName];
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

                if (topicSystemName == NAME_META_SYSTEM_NAME)
                {
                    AddTopicLanguageValueToXml(xmlDoc, rootNode, topic.Id, catalogGroupCache.DefaultLanguage.ID, asset.Name);
                    if (asset.NamesWithLanguages != null && asset.NamesWithLanguages.Count > 0)
                    {
                        foreach (LanguageContainer language in asset.NamesWithLanguages)
                        {
                            if (catalogGroupCache.LanguageMapByCode.ContainsKey(language.LanguageCode))
                            {
                                AddTopicLanguageValueToXml(xmlDoc, rootNode, topic.Id, catalogGroupCache.LanguageMapByCode[language.LanguageCode].ID, language.Value);
                            }
                        }
                    }
                }
                else if (topicSystemName == DESCRIPTION_META_SYSTEM_NAME)
                {
                    AddTopicLanguageValueToXml(xmlDoc, rootNode, topic.Id, catalogGroupCache.DefaultLanguage.ID, asset.Description);
                    if (asset.DescriptionsWithLanguages != null && asset.DescriptionsWithLanguages.Count > 0)
                    {
                        foreach (LanguageContainer language in asset.DescriptionsWithLanguages)
                        {
                            if (catalogGroupCache.LanguageMapByCode.ContainsKey(language.LanguageCode))
                            {
                                AddTopicLanguageValueToXml(xmlDoc, rootNode, topic.Id, catalogGroupCache.LanguageMapByCode[language.LanguageCode].ID, language.Value);
                            }
                        }
                    }
                }
            }
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

        private static Tuple<Asset, bool> GetAsset(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Asset asset = null;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("id") && funcParams.ContainsKey("assetType"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    long? id = funcParams["id"] as long?;
                    eAssetTypes assetType;
                    if (groupId.HasValue && groupId.Value > 0 && id.HasValue && id.Value > 0 && Enum.TryParse<eAssetTypes>(funcParams["assetType"].ToString(), out assetType))
                    {
                        CatalogGroupCache catalogGroupCache;
                        if (!TryGetCatalogGroupCacheFromCache(groupId.Value, out catalogGroupCache))
                        {
                            log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetAsset", groupId);                            
                        }

                        switch (assetType)
                        {                                                            
                            case eAssetTypes.EPG:
                                break;
                            case eAssetTypes.NPVR:
                                break;
                            case eAssetTypes.MEDIA:
                                DataSet ds = CatalogDAL.GetAsset(groupId.Value, id.Value, catalogGroupCache.DefaultLanguage.ID);
                                asset = CreateMediaAsset(groupId.Value, id.Value, ds, catalogGroupCache.DefaultLanguage, catalogGroupCache.LanguageMapById.Values.ToList());
                                break;
                            case eAssetTypes.UNKNOWN:
                            default:
                                break;
                        }
                                               
                        res = asset != null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetAsset failed params : {0}", funcParams != null ? string.Join(";",
                         funcParams.Select(x => string.Format("key:{0}, value: {1}", x.Key, x.Value.ToString())).ToList()) : string.Empty), ex);
            }

            return new Tuple<Asset, bool>(asset, res);
        }

        private static Asset GetAssetFromCache(int groupId, long id, eAssetTypes assetType)
        {
            Asset result = null;            
            try
            {
                string key = LayeredCacheKeys.GetAssetKey(assetType.ToString(), id);
                if (!LayeredCache.Instance.Get<Asset>(key, ref result, GetAsset, new Dictionary<string, object>() { { "groupId", groupId }, { "id", id }, { "assetType", assetType.ToString() } }, groupId,
                    LayeredCacheConfigNames.GET_ASSET_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetAssetInvalidationKey(assetType.ToString(), id) }))
                {
                    log.ErrorFormat("Failed getting Asset from LayeredCache, groupId: {0}, id: {1}, assetType: {2}", groupId, id, assetType.ToString());                    
                }                
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetFromCache with groupId: {0}, id: {1}, assetType: {2}", groupId, id, assetType.ToString()), ex);
            }

            return result;
        }

        private static List<Asset> GetAssetsFromCache(int groupId, List<KeyValuePair<eAssetTypes, long>> assets)
        {
            List<Asset> result = null;
            try
            {
                if (assets != null && assets.Count > 0)
                {
                    result = new List<Asset>();
                    foreach (KeyValuePair<eAssetTypes, long> assetDetails in assets)
                    {
                        if (assetDetails.Key != eAssetTypes.UNKNOWN && assetDetails.Value > 0)
                        {
                            Asset asset = GetAssetFromCache(groupId, assetDetails.Value, assetDetails.Key);
                            if (asset != null)
                            {
                                result.Add(asset);
                            }
                            else
                            {
                                log.ErrorFormat("Failed getting one asset during GetAssetsFromCache,groupId: {0}, id: {1}, assetType: {2}", groupId, assetDetails.Value, assetDetails.Key.ToString());
                            }
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

        #endregion

        #region Public Methods

        public static bool TryGetCatalogGroupCacheFromCache(int groupId, out CatalogGroupCache catalogGroupCache)
        {
            bool result = false;
            catalogGroupCache = null;
            try
            {
                string key = LayeredCacheKeys.GetCatalogGroupCacheKey(groupId);
                if (!LayeredCache.Instance.Get<CatalogGroupCache>(key, ref catalogGroupCache, GetCatalogGroupCache, new Dictionary<string, object>() { { "groupId", groupId } }, groupId,
                    LayeredCacheConfigNames.GET_CATALOG_GROUP_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetCatalogGroupCacheInvalidationKey(groupId) }))
                {
                    log.ErrorFormat("Failed getting CatalogGroupCache from LayeredCache, groupId: {0}", groupId);
                }

                result = catalogGroupCache.IsValid();
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetCatalogGroupCache with groupId: {0}", groupId), ex);
            }

            return result;
        }

        public static AssetStructListResponse GetAssetStructsByIds(int groupId, List<long> ids, bool? isProtected)
        {
            AssetStructListResponse response = new AssetStructListResponse();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetAssetStructsByIds", groupId);
                    return response;
                }

                if (ids != null && ids.Count > 0)
                {
                    response.AssetStructs = ids.Where(x => catalogGroupCache.AssetStructsMapById.ContainsKey(x)).Select(x => catalogGroupCache.AssetStructsMapById[x]).ToList();
                }
                else
                {
                    response.AssetStructs = catalogGroupCache.AssetStructsMapById.Values.ToList();
                }

                if (isProtected.HasValue)
                {
                    response.AssetStructs = response.AssetStructs.Where(x => x.IsPredefined.HasValue && x.IsPredefined == isProtected.Value).ToList();
                }

                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetStructsByIds with groupId: {0} and ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return response;
        }

        public static AssetStructListResponse GetAssetStructsByTopicId(int groupId, long topicId, bool? isProtected)
        {
            AssetStructListResponse response = new AssetStructListResponse();
            try
            {
                if (topicId > 0)
                {
                    CatalogGroupCache catalogGroupCache;
                    if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetAssetStructsByTopicId", groupId);
                        return response;                        
                    }

                    response.AssetStructs = catalogGroupCache.AssetStructsMapById.Values.Where(x => x.MetaIds.Contains(topicId)).ToList();
                    if (isProtected.HasValue)
                    {
                        response.AssetStructs = response.AssetStructs.Where(x => x.IsPredefined.HasValue && x.IsPredefined == isProtected.Value).ToList();
                    }

                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetStructsByTopicIds with groupId: {0} and topicId: {1}", groupId, topicId), ex);
            }

            return response;
        }        

        public static AssetStructResponse AddAssetStruct(int groupId, AssetStruct assetStructToadd, long userId)
        {
            AssetStructResponse result = new AssetStructResponse();
            try
            {                
                CatalogGroupCache catalogGroupCache;                
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling AddAssetStruct", groupId);
                    return result;
                }
                
                if (catalogGroupCache.AssetStructsMapBySystemName.ContainsKey(assetStructToadd.SystemName))
                {
                    result.Status = new Status((int)eResponseStatus.AssetStructSystemNameAlreadyInUse, eResponseStatus.AssetStructSystemNameAlreadyInUse.ToString());
                    return result;
                }

                // validate basic metas         
                Status validateBasicMetasResult = ValidateBasicMetaIds(catalogGroupCache, assetStructToadd);
                if (validateBasicMetasResult.Code != (int)eResponseStatus.OK)
                {
                    result.Status = validateBasicMetasResult;
                    return result;
                }               

                // validate meta ids exist
                List<long> noneExistingMetaIds = assetStructToadd.MetaIds.Except(catalogGroupCache.TopicsMapById.Keys).ToList();
                if (noneExistingMetaIds != null && noneExistingMetaIds.Count > 0)
                {
                    result.Status = new Status((int)eResponseStatus.MetaIdsDoesNotExist, string.Format("{0} for the following Meta Ids: {1}",
                                                                                            eResponseStatus.MetaIdsDoesNotExist.ToString(), string.Join(",", noneExistingMetaIds)));
                    return result;
                }
                
                List<KeyValuePair<long, int>> metaIdsToPriority = new List<KeyValuePair<long, int>>();
                if (assetStructToadd.MetaIds != null && assetStructToadd.MetaIds.Count > 0)
                {
                    int priority = 1;
                    foreach (long metaId in assetStructToadd.MetaIds)
                    {
                        metaIdsToPriority.Add(new KeyValuePair<long, int>(metaId, priority));
                        priority++;
                    }
                }

                List<KeyValuePair<string, string>> languageCodeToName = new List<KeyValuePair<string, string>>();
                if (assetStructToadd.NamesInOtherLanguages != null && assetStructToadd.NamesInOtherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in assetStructToadd.NamesInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.LanguageCode, language.Value));
                    }
                }

                DataSet ds = CatalogDAL.InsertAssetStruct(groupId, assetStructToadd.Name, languageCodeToName, assetStructToadd.SystemName, metaIdsToPriority, assetStructToadd.IsPredefined, userId);
                result = CreateAssetStructResponseFromDataSet(ds);
                InvalidateCatalogGroupCache(groupId, result.Status, true, result.AssetStruct);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddAssetStruct for groupId: {0} and assetStruct: {1}", groupId, assetStructToadd.ToString()), ex);
            }

            return result;
        }

        public static AssetStructResponse UpdateAssetStruct(int groupId, long id, AssetStruct assetStructToUpdate, bool shouldUpdateMetaIds, long userId)
        {
            AssetStructResponse result = new AssetStructResponse();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpdateAssetStruct", groupId);
                    return result;
                }

                if (!catalogGroupCache.AssetStructsMapById.ContainsKey(id))
                {
                    result.Status = new Status((int)eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString());
                    return result;
                }

                AssetStruct assetStruct = new AssetStruct(catalogGroupCache.AssetStructsMapById[id]);
                if (assetStruct.IsPredefined.HasValue && assetStruct.IsPredefined.Value && assetStructToUpdate.SystemName != null && assetStruct.SystemName != assetStructToUpdate.SystemName)
                {
                    result.Status = new Status((int)eResponseStatus.CanNotChangePredefinedAssetStructSystemName, eResponseStatus.CanNotChangePredefinedAssetStructSystemName.ToString());
                    return result;
                }

                if (shouldUpdateMetaIds)
                {
                    // validate basic metas
                    Status validateBasicMetasResult = ValidateBasicMetaIds(catalogGroupCache, assetStructToUpdate);
                    if (validateBasicMetasResult.Code != (int)eResponseStatus.OK)
                    {
                        result.Status = validateBasicMetasResult;
                        return result;
                    }

                    // validate meta ids exist
                    List<long> noneExistingMetaIds = assetStructToUpdate.MetaIds.Except(catalogGroupCache.TopicsMapById.Keys).ToList();
                    if (noneExistingMetaIds != null && noneExistingMetaIds.Count > 0)
                    {
                        result.Status = new Status((int)eResponseStatus.MetaIdsDoesNotExist, string.Format("{0} for the following Meta Ids: {1}",
                                                                                                eResponseStatus.MetaIdsDoesNotExist.ToString(), string.Join(",", noneExistingMetaIds)));
                        return result;
                    }
                }

                List<KeyValuePair<long, int>> metaIdsToPriority = null;
                if (assetStructToUpdate.MetaIds != null && shouldUpdateMetaIds)
                {
                    metaIdsToPriority = new List<KeyValuePair<long, int>>();
                    int priority = 1;
                    foreach (long metaId in assetStructToUpdate.MetaIds)
                    {
                        metaIdsToPriority.Add(new KeyValuePair<long, int>(metaId, priority));
                        priority++;
                    }

                    // no need to update DB if lists are equal
                    if (assetStruct.MetaIds != null && assetStructToUpdate.MetaIds.SequenceEqual(assetStruct.MetaIds))
                    {
                        shouldUpdateMetaIds = false;
                    }
                }

                List<KeyValuePair<string, string>> languageCodeToName = null;
                bool shouldUpdateOtherNames = false;
                if (assetStructToUpdate.NamesInOtherLanguages != null)
                {
                    shouldUpdateOtherNames = true;
                    languageCodeToName = new List<KeyValuePair<string, string>>();
                    foreach (LanguageContainer language in assetStructToUpdate.NamesInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.LanguageCode, language.Value));
                    }
                }

                DataSet ds = CatalogDAL.UpdateAssetStruct(groupId, id, assetStructToUpdate.Name, shouldUpdateOtherNames, languageCodeToName, assetStructToUpdate.SystemName, shouldUpdateMetaIds,
                                                            metaIdsToPriority, assetStructToUpdate.IsPredefined, userId);
                result = CreateAssetStructResponseFromDataSet(ds);
                InvalidateCatalogGroupCache(groupId, result.Status, true, result.AssetStruct);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateAssetStruct for groupId: {0}, id: {1} and assetStruct: {2}", groupId, id, assetStructToUpdate.ToString()), ex);
            }

            return result;
        }

        public static Status DeleteAssetStruct(int groupId, long id, long userId)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling DeleteAssetStruct", groupId);
                    return result;
                }
                                
                if (!catalogGroupCache.AssetStructsMapById.ContainsKey(id))
                {
                    result = new Status((int)eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString());
                    return result;
                }

                AssetStruct assetStruct = new AssetStruct(catalogGroupCache.AssetStructsMapById[id]);
                if (assetStruct.IsPredefined.HasValue && assetStruct.IsPredefined.Value)
                {
                    result = new Status((int)eResponseStatus.CanNotDeletePredefinedAssetStruct, eResponseStatus.CanNotDeletePredefinedAssetStruct.ToString());
                    return result;
                }

                if (CatalogDAL.DeleteAssetStruct(groupId, id, userId))
                {
                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    InvalidateCatalogGroupCache(groupId, result, false);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteAssetStruct for groupId: {0} and assetStructId: {1}", groupId, id), ex);
            }

            return result;
        }

        public static TopicListResponse GetTopicsByIds(int groupId, List<long> ids, MetaType type)
        {
            TopicListResponse response = new TopicListResponse();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetTopicsByIds", groupId);
                    return response;
                }
                
                if (ids != null && ids.Count > 0)
                {
                    response.Topics = ids.Where(x => catalogGroupCache.TopicsMapById.ContainsKey(x) && (type == MetaType.All || catalogGroupCache.TopicsMapById[x].Type == type))
                                                .Select(x => catalogGroupCache.TopicsMapById[x]).ToList();
                }
                else
                {
                    response.Topics = catalogGroupCache.TopicsMapById.Values.Where(x => type == MetaType.All || x.Type == type).ToList();
                }

                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());                
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetTopicsByIds with groupId: {0} and ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return response;
        }

        public static TopicListResponse GetTopicsByAssetStructId(int groupId, long assetStructId, MetaType type)
        {
            TopicListResponse response = new TopicListResponse();
            try
            {
                if (assetStructId > 0)
                {
                    CatalogGroupCache catalogGroupCache;
                    if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetTopicsByAssetStructId", groupId);
                        return response;
                    }

                    if (catalogGroupCache.AssetStructsMapById.ContainsKey(assetStructId))
                    {
                        List<long> topicIds = catalogGroupCache.AssetStructsMapById[assetStructId].MetaIds;
                        if (topicIds != null && topicIds.Count > 0)
                        {
                            response.Topics = topicIds.Where(x => catalogGroupCache.TopicsMapById.ContainsKey(x) && (type == MetaType.All || catalogGroupCache.TopicsMapById[x].Type == type))
                                                            .Select(x => catalogGroupCache.TopicsMapById[x]).ToList();
                        }
                    }

                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetTopicsByAssetStructId with groupId: {0} and assetStructId: {1}", groupId, assetStructId), ex);
            }

            return response;
        }

        public static TopicResponse AddTopic(int groupId, Topic topicToAdd, long userId)
        {
            TopicResponse result = new TopicResponse();
            try
            {
                if (topicToAdd.MultipleValue.HasValue && topicToAdd.MultipleValue.Value && topicToAdd.Type != MetaType.Tag)
                {
                    result.Status = new Status((int)eResponseStatus.InvalidMutlipleValueForMetaType, "MultipleValue can only be set to true for KalturaMetaType - STRING");
                    return result;
                }

                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling AddTopic", groupId);
                    return result;
                }
                
                if (catalogGroupCache.TopicsMapBySystemName.ContainsKey(topicToAdd.SystemName))
                {
                    result.Status = new Status((int)eResponseStatus.MetaSystemNameAlreadyInUse, eResponseStatus.MetaSystemNameAlreadyInUse.ToString());
                    return result;
                }

                List<KeyValuePair<string, string>> languageCodeToName = new List<KeyValuePair<string, string>>();
                if (topicToAdd.NamesInOtherLanguages != null && topicToAdd.NamesInOtherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in topicToAdd.NamesInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.LanguageCode, language.Value));
                    }
                }

                DataSet ds = CatalogDAL.InsertTopic(groupId, topicToAdd.Name, languageCodeToName, topicToAdd.SystemName, topicToAdd.Type, topicToAdd.GetFeaturesForDB(),
                                                      topicToAdd.IsPredefined, topicToAdd.ParentId, topicToAdd.HelpText, userId);
                result = CreateTopicResponseFromDataSet(ds);
                InvalidateCatalogGroupCache(groupId, result.Status, true, result.Topic);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddTopic for groupId: {0} and topic: {1}", groupId, topicToAdd.ToString()), ex);
            }

            return result;
        }

        public static TopicResponse UpdateTopic(int groupId, long id, Topic topicToUpdate, long userId)
        {
            TopicResponse result = new TopicResponse();
            try
            {                
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpdateTopic", groupId);
                    return result;
                }

                if (!catalogGroupCache.TopicsMapById.ContainsKey(id))
                {
                    result.Status = new Status((int)eResponseStatus.MetaDoesNotExist, eResponseStatus.MetaDoesNotExist.ToString());
                    return result;
                }

                Topic topic = new Topic(catalogGroupCache.TopicsMapById[id]);
                if (topic.IsPredefined.HasValue && topic.IsPredefined.Value && topicToUpdate.SystemName != null && topicToUpdate.SystemName != topic.SystemName)
                {
                    result.Status = new Status((int)eResponseStatus.CanNotChangePredefinedMetaSystemName, eResponseStatus.CanNotChangePredefinedMetaSystemName.ToString());
                    return result;
                }

                List<KeyValuePair<string, string>> languageCodeToName = null;
                bool shouldUpdateOtherNames = false;
                if (topicToUpdate.NamesInOtherLanguages != null)
                {
                    shouldUpdateOtherNames = true;
                    languageCodeToName = new List<KeyValuePair<string, string>>();
                    foreach (LanguageContainer language in topicToUpdate.NamesInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.LanguageCode, language.Value));
                    }
                }

                DataSet ds = CatalogDAL.UpdateTopic(groupId, id, topicToUpdate.Name, shouldUpdateOtherNames, languageCodeToName, topicToUpdate.SystemName, topicToUpdate.GetFeaturesForDB(topic.Features),
                                                    topicToUpdate.IsPredefined, topicToUpdate.ParentId, topicToUpdate.HelpText, userId);
                result = CreateTopicResponseFromDataSet(ds);
                InvalidateCatalogGroupCache(groupId, result.Status, true, result.Topic);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateTopic for groupId: {0}, id: {1} and assetStruct: {2}", groupId, id, topicToUpdate.ToString()), ex);
            }

            return result;
        }

        public static Status DeleteTopic(int groupId, long id, long userId)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {                
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling DeleteTopic", groupId);
                    return result;
                }

                if (!catalogGroupCache.TopicsMapById.ContainsKey(id))
                {
                    result = new Status((int)eResponseStatus.MetaDoesNotExist, eResponseStatus.MetaDoesNotExist.ToString());
                    return result;
                }

                Topic topic = new Topic(catalogGroupCache.TopicsMapById[id]);
                if (topic.IsPredefined.HasValue && topic.IsPredefined.Value)
                {
                    result = new Status((int)eResponseStatus.CanNotDeletePredefinedMeta, eResponseStatus.CanNotDeletePredefinedMeta.ToString());
                    return result;
                }

                if (CatalogDAL.DeleteTopic(groupId, id, userId))
                {
                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    InvalidateCatalogGroupCache(groupId, result, false);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteTopic for groupId: {0} and assetStructId: {1}", groupId, id), ex);
            }

            return result;
        }

        public static MediaObj GetMediaObj(int groupId, long id)
        {
            MediaObj result = null;
            try
            {
                if (id > 0)
                {
                    CatalogGroupCache catalogGroupCache;
                    if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetAsset", groupId);
                        return result;
                    }

                    DataSet ds = CatalogDAL.GetAsset(groupId, id, catalogGroupCache.DefaultLanguage.ID);
                    MediaAsset mediaAsset = CreateMediaAsset(groupId, id, ds, catalogGroupCache.DefaultLanguage, catalogGroupCache.LanguageMapById.Values.ToList());
                    if (mediaAsset != null)
                    {
                        result = new MediaObj(groupId, mediaAsset);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAsset for groupId: {0} and id: {1}", groupId, id), ex);
            }

            return result;
        }

        public static AssetResponse GetAsset(int groupId, long id, eAssetTypes assetType)
        {
            AssetResponse result = new AssetResponse();
            try
            {
                if (id > 0 && assetType != eAssetTypes.UNKNOWN)
                {
                    result.Asset = GetAssetFromCache(groupId, id, assetType);
                    if (result.Asset == null)
                    {
                        log.ErrorFormat("Failed getting asset from GetAssetFromCache, for groupId: {0}, id: {1}, assetType: {2}", groupId, id, assetType.ToString());
                    }

                    result.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAsset for groupId: {0}, id: {1}, assetType: {2}", groupId, id, assetType.ToString()), ex);
            }

            return result;
        }

        public static AssetListResponse GetAssets(int groupId, List<KeyValuePair<eAssetTypes, long>> assets)
        {
            AssetListResponse result = new AssetListResponse();
            try
            {
                if (assets != null && assets.Count > 0)
                {
                    result.Assets = GetAssetsFromCache(groupId, assets);
                    if (result.Assets == null || result.Assets.Count != assets.Count)
                    {
                        log.ErrorFormat("Failed getting assets from GetAssetsFromCache, for groupId: {0}, assets: {1}", groupId,
                                        assets != null ? string.Join(",", assets.Select(x => string.Format("{0}_{1}", x.Key, x.Value)).ToList()) : string.Empty);
                    }

                    result.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssets for groupId: {0}, assets: {1}", groupId,
                                        assets != null ? string.Join(",", assets.Select(x => string.Format("{0}_{1}", x.Key, x.Value)).ToList()) : string.Empty), ex);
            }

            return result;
        }

        /// <summary>
        /// Returns dictionary of [assetId, [language, media]]
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> GetGroupAssets(int groupId)
        {
            // <assetId, <languageId, media>>
            Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> groupAssetsMap = new Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>>();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetGroupAssets", groupId);
                    return groupAssetsMap;
                }

                DataSet groupAssetsDs = CatalogDAL.GetGroupAssets(groupId, catalogGroupCache.DefaultLanguage.ID);
                groupAssetsMap = CreateGroupMediaMapFromDataSet(groupId, groupAssetsDs, catalogGroupCache);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGroupAssets for groupId: {0}", groupId), ex);
            }

            return groupAssetsMap;
        }

        public static HashSet<string> GetUnifiedSearchKey(int groupId, string originalKey, out bool isTagOrMeta, out Type type)
        {
            isTagOrMeta = false;
            type = typeof(string);
            HashSet<string> searchKeys = new HashSet<string>();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetUnifiedSearchKey", groupId);
                    return searchKeys;
                }

                List<string> tags = catalogGroupCache.TopicsMapBySystemName.Where(x => x.Value.Type == ApiObjects.MetaType.Tag && x.Value.MultipleValue.HasValue && x.Value.MultipleValue.Value).Select(x => x.Key).ToList();
                List<string> metas = catalogGroupCache.TopicsMapBySystemName.Where(x => !x.Value.MultipleValue.HasValue || !x.Value.MultipleValue.Value).Select(x => x.Key).ToList();
                if (originalKey.StartsWith("tags."))
                {
                    foreach (string tag in tags)
                    {
                        if (tag.Equals(originalKey.Substring(5), StringComparison.OrdinalIgnoreCase))
                        {
                            isTagOrMeta = true;
                            searchKeys.Add(originalKey.ToLower());
                            break;
                        }
                    }
                }
                else if (originalKey.StartsWith("metas."))
                {
                    foreach (var meta in metas)
                    {
                        if (meta.Equals(originalKey.Substring(6), StringComparison.OrdinalIgnoreCase))
                        {
                            isTagOrMeta = true;
                            searchKeys.Add(originalKey.ToLower());
                            break;
                        }
                    }
                }
                else
                {
                    foreach (string tag in tags)
                    {
                        if (tag.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                        {
                            isTagOrMeta = true;

                            searchKeys.Add(string.Format("tags.{0}", tag.ToLower()));
                            break;
                        }
                    }

                    foreach (string meta in metas)
                    {
                        if (meta.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                        {
                            isTagOrMeta = true;
                            type = catalogGroupCache.TopicsMapBySystemName.ContainsKey(meta) ? GetMetaType(catalogGroupCache.TopicsMapBySystemName[meta]) : typeof(string);
                            searchKeys.Add(string.Format("metas.{0}", meta.ToLower()));
                            break;
                        }
                    }
                }

                if (!isTagOrMeta)
                {
                    searchKeys.Add(originalKey.ToLower());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetUnifiedSearchKey for groupId: {0}, originalKey: {1}", groupId, originalKey), ex);               
            }

            return searchKeys;
        }

        public static AssetResponse AddAsset(int groupId, MediaAsset assetToAdd, long userId)
        {
            AssetResponse result = new AssetResponse();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling AddAsset", groupId);
                    return result;
                }

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
                    result.Status = new Status((int)eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString());
                    return result;
                }

                // validate asset
                XmlDocument metasXmlDoc = null;
                XmlDocument tagsXmlDoc = null;
                Status validateAssetTopicsResult = ValidateAsset(groupId, catalogGroupCache, ref assetStruct, assetToAdd, ref metasXmlDoc, ref tagsXmlDoc);
                if (validateAssetTopicsResult.Code != (int)eResponseStatus.OK)
                {
                    result.Status = validateAssetTopicsResult;
                    return result;
                }

                // Add Name meta values (for languages that are not default)
                ExtractTopicLanguageAndValuesFromMediaAsset(assetToAdd, catalogGroupCache, ref metasXmlDoc, NAME_META_SYSTEM_NAME);

                // Add Description meta values (for languages that are not default)
                ExtractTopicLanguageAndValuesFromMediaAsset(assetToAdd, catalogGroupCache, ref metasXmlDoc, DESCRIPTION_META_SYSTEM_NAME);

                DateTime startDate = assetToAdd.StartDate.HasValue ? assetToAdd.StartDate.Value : DateTime.UtcNow;
                DateTime catalogStartDate = assetToAdd.CatalogStartDate.HasValue ? assetToAdd.CatalogStartDate.Value : startDate;
                // TODO - Lior. Need to extract all values from tags that are part of the mediaObj properties (Basic metas)
                DataSet ds = CatalogDAL.InsertMediaAsset(groupId, catalogGroupCache.DefaultLanguage.ID, metasXmlDoc, tagsXmlDoc, assetToAdd.CoGuid, assetToAdd.EntryId, assetToAdd.DeviceRuleId,
                                                            assetToAdd.GeoBlockRuleId, assetToAdd.IsActive, startDate, assetToAdd.EndDate, catalogStartDate, assetToAdd.FinalEndDate, assetStruct.Id, userId);
                result = CreateAssetResponseFromDataSet(groupId, ds, catalogGroupCache.DefaultLanguage, catalogGroupCache.LanguageMapById.Values.ToList());
                if (result != null && result.Status != null && result.Status.Code == (int)eResponseStatus.OK && result.Asset != null && result.Asset.Id > 0)
                {                    
                    // UpdateIndex
                    if (!CatalogLogic.UpdateIndex(new List<long>() { result.Asset.Id }, groupId, eAction.Update))
                    {
                        log.ErrorFormat("Failed to UpdateIndex for assetId: {0}, groupId: {1}", result.Asset.Id, groupId);
                    }
                }                                                
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddAssetStruct for groupId: {0} and asset: {1}", groupId, assetToAdd.ToString()), ex);
            }

            return result;
        }

        public static Status DeleteAsset(int groupId, long id, eAssetTypes assetType, long userId)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                // check if asset exists???

                switch (assetType)
                {
                    case eAssetTypes.EPG:
                        break;
                    case eAssetTypes.NPVR:
                        break;
                    case eAssetTypes.MEDIA:
                        if (CatalogDAL.DeleteMediaAsset(groupId, id, userId))
                        {
                            result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());                            
                        }
                        else
                        {
                            log.ErrorFormat("Failed to delete media asset with id: {0}, groupId: {1}", id, groupId);
                        }
                        break;
                    default:
                    case eAssetTypes.UNKNOWN:
                        break;
                }

                // invalidate asset
                if (result.Code == (int)eResponseStatus.OK)
                {
                    if (!LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAssetInvalidationKey(assetType.ToString(), id)))
                    {
                        log.ErrorFormat("Failed to invalidate asset with id: {0}, assetType: {1}, invalidationKey: {2}", id, assetType.ToString(), LayeredCacheKeys.GetAssetInvalidationKey(assetType.ToString(), id));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteTopic for groupId: {0} and assetStructId: {1}", groupId, id), ex);
            }

            return result;
        }

        public static bool DoesGroupUsesTemplates(int groupId)
        {
            // TODO - Lior - change this to look somewhere and cache (after getting value from DB) and not try to get entire CatalogGroupCache object
            bool res = false;
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling DeleteTopic", groupId);
                    return res;
                }

                res = true;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DoesGroupUsesTemplates for groupId: {0}", groupId), ex);
            }

            return res;
        }

        /// <summary>
        /// [topic, [language, [tag id, tag value]]]
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static Dictionary<Topic, Dictionary<int, Dictionary<int, string>>> GetAllTagValues(int groupId)
        {
            var result = new Dictionary<Topic, Dictionary<int, Dictionary<int, string>>>();

            CatalogGroupCache catalogGroupCache;
            if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetGroupAssets", groupId);
                return null;
            }

            int defaultLanguage = catalogGroupCache.DefaultLanguage.ID;

            var tags = catalogGroupCache.TopicsMapBySystemName.Where(
                x => x.Value.Type == ApiObjects.MetaType.Tag && x.Value.MultipleValue.HasValue && x.Value.MultipleValue.Value).Select(x => x.Value).ToList();

            DataSet dataSet = CatalogDAL.GetGroupTagValues(groupId);

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0 && 
                dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null && dataSet.Tables[0].Rows.Count > 0)
            {
                // First table - default language, set default for other languages as well
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    int tagId = ODBCWrapper.Utils.ExtractInteger(row, "tag_id");
                    int topicId = ODBCWrapper.Utils.ExtractInteger(row, "topic_id");
                    string tagValue = ODBCWrapper.Utils.ExtractString(row, "tag_value");

                    if (catalogGroupCache.TopicsMapById.ContainsKey(topicId))
                    {
                        Topic topic = catalogGroupCache.TopicsMapById[topicId];

                        if (!result.ContainsKey(topic))
                        {
                            result[topic] = new Dictionary<int, Dictionary<int, string>>();
                        }

                        if (!result[topic].ContainsKey(defaultLanguage))
                        {
                            result[topic][defaultLanguage] = new Dictionary<int, string>();
                        }

                        result[topic][defaultLanguage][tagId] = tagValue;
                    }
                }
            }

            // Second table - translations
            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 1 &&
                dataSet.Tables[1] != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count > 0)
            {
                foreach (DataRow row in dataSet.Tables[1].Rows)
                {
                    int tagId = ODBCWrapper.Utils.ExtractInteger(row, "tag_id");
                    int topicId = ODBCWrapper.Utils.ExtractInteger(row, "topic_id");
                    int languageId = ODBCWrapper.Utils.ExtractInteger(row, "language_id");
                    string tagValue = ODBCWrapper.Utils.ExtractString(row, "tag_value");

                    if (catalogGroupCache.TopicsMapById.ContainsKey(topicId))
                    {
                        Topic topic = catalogGroupCache.TopicsMapById[topicId];

                        if (!result.ContainsKey(topic))
                        {
                            result[topic] = new Dictionary<int, Dictionary<int, string>>();
                        }

                        if (!result[topic].ContainsKey(languageId))
                        {
                            result[topic][languageId] = new Dictionary<int, string>();
                        }

                        // override the default value with the translation
                        result[topic][languageId][tagId] = tagValue;
                    }
                }
            }

            return result;
        }
    }

    #endregion

}