using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
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
using Tvinci.Core.DAL;

namespace Core.Catalog.CatalogManagement
{
    public class CatalogManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly List<string> basicMetasSystemNames = new List<string>() { "name", "description", "co_guid", "entry_id", "is_active", "playback_start_date", "playback_end_date", "catalog_start_date", "catalog_end_date" };

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

                        catalogGroupCache = new CatalogGroupCache(assetStructs, topics);
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
                if (assetStructToValidate.MetaIds != null && assetStructToValidate.MetaIds.Count >= basicMetasSystemNames.Count)
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
                case -111:
                    responseStatus = new Status((int)eResponseStatus.MetaNameAlreadyInUse, eResponseStatus.MetaNameAlreadyInUse.ToString());
                    break;
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

        private static MediaObj CreateMediaFromDataSet(int groupId, DataSet ds, LanguageObj defaultLanguage, List<LanguageObj> groupLanguages, Filter filter)
        {
            MediaObj result = null;
            if (ds == null || ds.Tables == null || ds.Tables.Count < 6)
            {
                log.WarnFormat("CreateMediaFromDataSet didn't receive dataset with 6 or more tables");
                return null;
            }

            // Basic details tables
            if (ds.Tables[0] == null || ds.Tables[0].Rows == null || ds.Tables[0].Rows.Count != 1)
            {
                log.WarnFormat("CreateMediaFromDataSet - basic details table is not valid");
                return null;
            }

            DataRow basicDataRow = ds.Tables[0].Rows[0];
            long id = ODBCWrapper.Utils.GetLongSafeVal(basicDataRow, "ID", 0);
            if (id <= 0)
            {
                return null;
            }

            result = new MediaObj();
            result.AssetId = id.ToString();
            string name = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "NAME");
            string description = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "DESCRIPTION");
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description))
            {
                log.WarnFormat("Name or description is not valid for media with Id: {0}", id);
                return null;
            }

            result.EntryId = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "ENTRY_ID");
            result.CoGuid = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "CO_GUID");
            List<LanguageContainer> names = new List<LanguageContainer>();
            names.Add(new LanguageContainer(defaultLanguage.Code, name, true));
            result.Name = names.ToArray();
            List<LanguageContainer> descriptions = new List<LanguageContainer>();
            descriptions.Add(new LanguageContainer(defaultLanguage.Code, description, true));
            result.Description = descriptions.ToArray();
            long assetStructId = ODBCWrapper.Utils.GetLongSafeVal(basicDataRow, "ASSET_STRUCT_ID", 0);
            if (!TryGetMediaTypeFromAssetStructId(groupId, assetStructId, out result.m_oMediaType))
            {
                log.WarnFormat("media type (assetStruct) is not valid for media with Id: {0}, assetStructId: {1}", id, assetStructId);
                return null;
            }

            result.m_dCreationDate = ODBCWrapper.Utils.GetDateSafeVal(basicDataRow, "CREATE_DATE");
            result.m_dFinalDate = ODBCWrapper.Utils.GetDateSafeVal(basicDataRow, "FINAL_END_DATE");
            result.m_dPublishDate = ODBCWrapper.Utils.GetDateSafeVal(basicDataRow, "PUBLISH_DATE");
            result.m_dStartDate = ODBCWrapper.Utils.GetDateSafeVal(basicDataRow, "START_DATE");
            result.m_dEndDate = ODBCWrapper.Utils.GetDateSafeVal(basicDataRow, "END_DATE");
            result.m_dCatalogStartDate = ODBCWrapper.Utils.GetDateSafeVal(basicDataRow, "CATALOG_START_DATE");

            // Last updated table
            if (ds.Tables[5] == null || ds.Tables[5].Rows == null || ds.Tables[5].Rows.Count != 1)
            {
                log.WarnFormat("CreateMediaFromDataSet - last updatedbasic  table is not valid, Id: {0}", id);
                return null;
            }
            
            DataRow lastUpdatedDr = ds.Tables[5].Rows[0];
            result.m_dUpdateDate = ODBCWrapper.Utils.GetDateSafeVal(lastUpdatedDr, "UPDATE_DATE");

            // Pictures table
            if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
            {
                bool picturesResult = false;
                result.m_lPicture = CatalogLogic.GetAllPic(groupId, (int)id, ds.Tables[1], ref picturesResult, groupId);
                if (!picturesResult)
                {
                    log.WarnFormat("CreateMediaFromDataSet - failed to get pictures for Id: {0}", id);
                    return null;
                }
            }

            // Files table
            if (ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
            {
                bool filesResult = false;
                Dictionary<int, List<string>> mediaFilePPVModules = mediaFilePPVModules = CatalogLogic.GetMediaFilePPVModules(ds.Tables[2]);
                // TODO - Lior - what to do with management data
                result.m_lFiles = CatalogLogic.FilesValues(ds.Tables[2], ref result.m_lBranding, filter.m_noFileUrl, ref filesResult, true, mediaFilePPVModules);
                if (!filesResult)
                {
                    log.WarnFormat("CreateMediaFromDataSet - failed to get files for Id: {0}", id);
                    return null;
                }
            }

            // Metas and Tags table
            DataTable metasTable = null;
            DataTable tagsTable = null;
            if (ds.Tables[3] != null && ds.Tables[3].Rows != null && ds.Tables[3].Rows.Count > 0)
            {
                metasTable = ds.Tables[3];
            }
            if (ds.Tables[4] != null && ds.Tables[4].Rows != null && ds.Tables[4].Rows.Count > 0)
            {
                tagsTable = ds.Tables[4];
            }

            if (!TryGetMetasAndTags(groupId, id, defaultLanguage.ID, groupLanguages, metasTable, tagsTable, ref result))
            {
                log.WarnFormat("CreateMediaFromDataSet - failed to get media metas and tags for Id: {0}", id);
                return null;
            }

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

        private static bool TryGetMetasAndTags(int groupId, long mediaId, int defaultLanguageId, List<LanguageObj> groupLanguages, DataTable metasTable, DataTable tagsTable, ref MediaObj media)
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

            // TODO - Lior - needs Ira help here in tags and meta
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
                    media.m_lTags = new List<Tags>();
                    media.m_lMetas = new List<Metas>();
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
                                media.m_lTags.Add(new Tags(new TagMeta(topic.SystemName, topic.Type.ToString()), defaultValues, tagLanguages));
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

                                media.m_lMetas.Add(new Metas(new TagMeta(topic.SystemName, topic.Type.ToString()), defaultValue, topicLanguages));
                            }
                        }                        
                    }

                    res = true;
                }
            }

            return res;
        }

        private static Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> CreateGroupMediaMapFromDataSet(int groupId, DataSet ds, LanguageObj defaultLanguage, List<LanguageObj> groupLanguages)
        {
            // <assetId, <languageId, media>>
            Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> groupAssetsMap = new Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>>();
            try
            {
                if (ds == null || ds.Tables == null || ds.Tables.Count < 5)
                {
                    log.WarnFormat("CreateGroupMediaMapFromDataSet didn't receive dataset with 5 or more tables");
                    return null;
                }

                // Basic details tables
                if (ds.Tables[0] == null || ds.Tables[0].Rows == null || ds.Tables[0].Rows.Count <= 0)
                {
                    log.WarnFormat("CreateGroupMediaMapFromDataSet - basic details table is not valid");
                    return null;
                }

                foreach (DataRow basicDataRow in ds.Tables[0].Rows)
                {                    
                    int id = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "ID", 0);
                    if (id > 0 && !groupAssetsMap.ContainsKey(id))
                    {
                        ApiObjects.SearchObjects.Media asset = BuildMediaFromDataRow(groupId, basicDataRow, id);
                        groupAssetsMap.Add(asset.m_nMediaID, new Dictionary<int, ApiObjects.SearchObjects.Media>() { { defaultLanguage.ID, asset } });
                    }
                }

                //get all the media files types for each mediaId that have been selected.
                if (ds.Tables[1] != null && ds.Tables[1].Columns != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[1].Rows)
                    {
                        int mediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                        string sMFT = ODBCWrapper.Utils.GetSafeStr(row, "media_type_id");
                        bool isTypeFree = ODBCWrapper.Utils.ExtractBoolean(row, "is_free");

                        if (groupAssetsMap.ContainsKey(mediaID) && groupAssetsMap[mediaID].ContainsKey(defaultLanguage.ID))
                        {
                            ApiObjects.SearchObjects.Media theMedia = groupAssetsMap[mediaID][defaultLanguage.ID];
                            theMedia.m_sMFTypes += string.Format("{0};", sMFT);
                            int mediaTypeId;
                            if (isTypeFree)
                            {
                                // if at least one of the media types is free - this media is free
                                theMedia.isFree = true;

                                if (int.TryParse(sMFT, out mediaTypeId))
                                {
                                    theMedia.freeFileTypes.Add(mediaTypeId);
                                }
                            }
                        }
                    }
                }

                // Metas and Tags table
                DataTable metasTable = null;
                DataTable tagsTable = null;
                if (ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
                {
                    metasTable = ds.Tables[2];
                }
                if (ds.Tables[3] != null && ds.Tables[3].Rows != null && ds.Tables[3].Rows.Count > 0)
                {
                    tagsTable = ds.Tables[3];
                }

                List<DataRow> defaultLanguageMetas = new List<DataRow>();
                List<DataRow> defaultLanguageTopics = new List<DataRow>();
                List<DataRow> otherLanguageMetas = new List<DataRow>();
                List<DataRow> otherLanguageTopics = new List<DataRow>();
                if (metasTable != null && metasTable.Rows != null && metasTable.Rows.Count > 0)
                {
                    defaultLanguageMetas = (from row in metasTable.AsEnumerable()
                                            where (Int64)row["LANGUAGE_ID"] == defaultLanguage.ID
                                            select row).ToList();
                    otherLanguageMetas = (from row in metasTable.AsEnumerable()
                                          where (Int64)row["LANGUAGE_ID"] != defaultLanguage.ID
                                          select row).ToList();
                }

                if (tagsTable != null && tagsTable.Rows != null && tagsTable.Rows.Count > 0)
                {
                    defaultLanguageTopics = (from row in tagsTable.AsEnumerable()
                                             where (Int64)row["LANGUAGE_ID"] == defaultLanguage.ID
                                             select row).ToList();
                    otherLanguageTopics = (from row in tagsTable.AsEnumerable()
                                           where (Int64)row["LANGUAGE_ID"] != defaultLanguage.ID
                                           select row).ToList();
                }

                // Fill Default Language Metas and Tags
                FillAssetMetasAndTags(groupId, defaultLanguageMetas, defaultLanguageTopics, ref groupAssetsMap);

                // Clone Assets
                foreach (int mediaId in groupAssetsMap.Keys)
                {                    
                    foreach (LanguageObj language in groupLanguages.Where(x => !x.IsDefault))
                    {
                        groupAssetsMap[mediaId].Add(language.ID, groupAssetsMap[mediaId][defaultLanguage.ID].Clone());
                    }
                }

                // Fill Other Language Metas and Tags
                FillAssetMetasAndTags(groupId, otherLanguageMetas, otherLanguageTopics, ref groupAssetsMap);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed CreateGroupMediaMapFromDataSet for groupId: {0}", groupId), ex);
            }

            return groupAssetsMap;
        }

        private static ApiObjects.SearchObjects.Media BuildMediaFromDataRow(int groupId, DataRow basicDataRow, int id)
        {
            ApiObjects.SearchObjects.Media asset = new ApiObjects.SearchObjects.Media();
            asset.m_nMediaID = id;
            asset.m_nWPTypeID = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "watch_permission_type_id");
            asset.m_nMediaTypeID = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "ASSET_STRUCT_ID", 0);
            if (!ValidateAssetStructIdExists(groupId, asset.m_nMediaTypeID))
            {
                log.WarnFormat("media type Id: {0} is not valid for media with Id: {1}", asset.m_nMediaTypeID, id);
            }

            asset.m_nGroupID = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "group_id");
            asset.m_nIsActive = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "is_active");
            asset.m_nDeviceRuleId = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "device_rule_id");
            asset.m_nLikeCounter = 0;
            asset.m_nViews = 0;
            asset.m_sUserTypes = ODBCWrapper.Utils.GetSafeStr(basicDataRow["user_types"]);
            asset.EntryId = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "ENTRY_ID");
            asset.CoGuid = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "CO_GUID");
            // by default - media is not free
            asset.isFree = false;
            asset.geoBlockRule = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "geo_block_rule_id");
            asset.m_sName = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "NAME");
            asset.m_sDescription = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "DESCRIPTION");
            if (string.IsNullOrEmpty(asset.m_sName) || string.IsNullOrEmpty(asset.m_sDescription))
            {
                log.WarnFormat("Name or description is not valid for media with Id: {0}", id);
            }

            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(basicDataRow, "create_date")))
            {
                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(basicDataRow, "create_date");
                asset.m_sCreateDate = dt.ToString("yyyyMMddHHmmss");
            }

            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(basicDataRow, "update_date")))
            {
                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(basicDataRow, "update_date");
                asset.m_sUpdateDate = dt.ToString("yyyyMMddHHmmss");
            }

            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(basicDataRow, "start_date")))
            {
                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(basicDataRow, "start_date");
                asset.m_sStartDate = dt.ToString("yyyyMMddHHmmss");
            }

            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(basicDataRow, "end_date")))
            {
                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(basicDataRow, "end_date");
                asset.m_sEndDate = dt.ToString("yyyyMMddHHmmss");

            }

            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(basicDataRow, "final_end_date")))
            {
                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(basicDataRow, "final_end_date");
                asset.m_sFinalEndDate = dt.ToString("yyyyMMddHHmmss");
            }

            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(basicDataRow, "catalog_start_date")))
            {
                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(basicDataRow, "catalog_start_date");
                asset.CatalogStartDate = dt.ToString("yyyyMMddHHmmss");
            }

            return asset;
        }

        private static bool ValidateAssetStructIdExists(int groupId, int assetStructId)
        {
            bool res = false;            
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
            
            return true;
        }

        private static void FillAssetMetasAndTags(int groupId, List<DataRow> metaRows, List<DataRow> tagRows, ref Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> groupAssetsMap)
        {            
            CatalogGroupCache catalogGroupCache;
            if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling TryGetAssetMetasAndTags", groupId);
                return;
            }

            Dictionary<long, List<LanguageContainer>> topicIdToMeta = new Dictionary<long, List<LanguageContainer>>();
            // TODO - Lior - need to handle basic meta's differently
            foreach (DataRow dr in metaRows)
            {
                int mediaId = ODBCWrapper.Utils.GetIntSafeVal(dr, "asset_id");
                long topicId = ODBCWrapper.Utils.GetLongSafeVal(dr, "topic_id");
                int languageId = ODBCWrapper.Utils.GetIntSafeVal(dr, "language_id");
                string translation = ODBCWrapper.Utils.GetSafeStr(dr, "translation");                
                if (groupAssetsMap.ContainsKey(mediaId) && catalogGroupCache.TopicsMapById.ContainsKey(topicId) && groupAssetsMap[mediaId].ContainsKey(languageId))
                {
                    groupAssetsMap[mediaId][languageId].m_dMeatsValues[catalogGroupCache.TopicsMapById[topicId].SystemName] = translation;
                }               
            }

            foreach (DataRow dr in tagRows)
            {
                int mediaId = ODBCWrapper.Utils.GetIntSafeVal(dr, "asset_id");
                long topicId = ODBCWrapper.Utils.GetLongSafeVal(dr, "topic_id");
                long tagId = ODBCWrapper.Utils.GetLongSafeVal(dr, "tag_id");
                int languageId = ODBCWrapper.Utils.GetIntSafeVal(dr, "language_id");
                string translation = ODBCWrapper.Utils.GetSafeStr(dr, "translation");
                if (groupAssetsMap.ContainsKey(mediaId) && catalogGroupCache.TopicsMapById.ContainsKey(topicId) && groupAssetsMap[mediaId].ContainsKey(languageId))
                {
                    if (!groupAssetsMap[mediaId][languageId].m_dTagValues.ContainsKey(catalogGroupCache.TopicsMapById[topicId].SystemName))
                    {
                        groupAssetsMap[mediaId][languageId].m_dTagValues.Add(catalogGroupCache.TopicsMapById[topicId].SystemName, new Dictionary<long, string>());
                    }

                    if (!groupAssetsMap[mediaId][languageId].m_dTagValues[catalogGroupCache.TopicsMapById[topicId].SystemName].ContainsKey(tagId))
                    {
                        groupAssetsMap[mediaId][languageId].m_dTagValues[catalogGroupCache.TopicsMapById[topicId].SystemName].Add(tagId, translation);
                    }
                }
            }
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
                            response.Topics = topicIds.Where(x => catalogGroupCache.TopicsMapById.ContainsKey(x)).Select(x => catalogGroupCache.TopicsMapById[x]).ToList();
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

        public static MediaObj GetAsset(int groupId, Group group, long id, Filter filter)
        {
            MediaObj result = null;
            try
            {
                GroupManager groupManager = new GroupManager();                
                if (group == null)
                {
                    log.WarnFormat("GetAsset - Failed to get Group object for groupId: {0}", groupId);
                    return result;
                }

                LanguageObj defaultLanguage = group.GetGroupDefaultLanguage();
                if (defaultLanguage == null || defaultLanguage.ID <= 0)
                {
                    log.WarnFormat("GetAsset - Failed to get defaultLanguage object for groupId: {0}", groupId);
                    return result;
                }

                DataSet ds = CatalogDAL.GetAsset(groupId, id, defaultLanguage.ID);
                result = CreateMediaFromDataSet(groupId, ds, defaultLanguage, group.GetLangauges(), filter);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAsset for groupId: {0} and id: {1}", groupId, id), ex);
            }

            return result;
        }

        public static Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> GetGroupAssets(int groupId, Group group)
        {
            // <assetId, <languageId, media>>
            Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> groupAssetsMap = new Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>>();
            try
            {
                if (group == null)
                {
                    log.WarnFormat("GetGroupAssets - Failed to get Group object for groupId: {0}", groupId);
                    return groupAssetsMap;
                }

                LanguageObj defaultLanguage = group.GetGroupDefaultLanguage();
                if (defaultLanguage == null || defaultLanguage.ID <= 0)
                {
                    log.WarnFormat("GetGroupAssets - Failed to get defaultLanguage object for groupId: {0}", groupId);
                    return groupAssetsMap;
                }

                DataSet groupAssetsDs = CatalogDAL.GetGroupAssets(groupId, defaultLanguage.ID);
                groupAssetsMap = CreateGroupMediaMapFromDataSet(groupId, groupAssetsDs, defaultLanguage, group.GetLangauges());
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

        #endregion

    }
}