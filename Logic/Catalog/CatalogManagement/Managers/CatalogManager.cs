using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Tvinci.Core.DAL;

namespace Core.Catalog.CatalogManagement
{
    public class CatalogManager
    {

        #region Constants and Readonly

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal static readonly HashSet<string> TopicsToIgnore = Core.Catalog.CatalogLogic.GetTopicsToIgnoreOnBuildIndex();

        #endregion

        #region Private Methods

        private static Tuple<bool, bool> DoesGroupUsesTemplates(Dictionary<string, object> funcParams)
        {
            bool res = false;
            bool doesGroupUsesTemplates = false;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue && groupId.Value > 0)
                    {
                        doesGroupUsesTemplates = CatalogDAL.DoesGroupUsesTemplates(groupId.Value);
                        res = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("DoesGroupUsesTemplates failed params : {0}", funcParams != null ? string.Join(";",
                         funcParams.Select(x => string.Format("key:{0}, value: {1}", x.Key, x.Value.ToString())).ToList()) : string.Empty), ex);
            }

            return new Tuple<bool, bool>(doesGroupUsesTemplates, res);
        }

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
                    string associationTag = ODBCWrapper.Utils.GetSafeStr(dr, "ASSOCIATION_TAG");
                    long parentId = ODBCWrapper.Utils.GetLongSafeVal(dr, "PARENT_TYPE_ID");
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
                    result = new AssetStruct(id, name, namesInOtherLanguages, systemName, isPredefined, associationTag, parentId, 
                                                createDate.HasValue ? ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(createDate.Value) : 0,
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
                basicMetaIds = catalogGroupCache.TopicsMapBySystemName.Where(x => AssetManager.BasicMetasSystemNames.Contains(x.Key.ToLower())).Select(x => x.Value.Id).ToList();
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

        private static GenericResponse<AssetStruct> CreateAssetStructResponseFromDataSet(DataSet ds)
        {
            GenericResponse<AssetStruct> response = new GenericResponse<AssetStruct>();
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
                        response.Object = CreateAssetStruct(id, dt.Rows[0], assetStructTranslations);
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

                if (response.Object != null && ds.Tables.Count == 3)
                {
                    DataTable metasDt = ds.Tables[2];
                    if (response.Object != null && metasDt != null && metasDt.Rows != null && metasDt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in metasDt.Rows)
                        {
                            long assetStructId = ODBCWrapper.Utils.GetLongSafeVal(dr, "TEMPLATE_ID", 0);
                            long metaId = ODBCWrapper.Utils.GetLongSafeVal(dr, "TOPIC_ID", 0);
                            if (!response.Object.MetaIds.Contains(metaId))
                            {
                                response.Object.MetaIds.Add(metaId);
                            }
                        }
                    }
                }

                if (response.Object != null)
                {
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }

            return response;
        }

        private static List<AssetStruct> CreateAssetStructListFromDataSet(DataSet ds)
        {
            List<AssetStruct> response = null;
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

                            AssetStructMeta assetStructMeta = new AssetStructMeta()
                            {
                                AssetStructId = assetStructId,
                                MetaId = metaId,
                                IngestReferencePath = ODBCWrapper.Utils.GetSafeStr(dr, "INGEST_REFERENCE_PATH"),
                                ProtectFromIngest = ODBCWrapper.Utils.ExtractBoolean(dr, "PROTECT_FROM_INGEST"),
                                DefaultIngestValue = ODBCWrapper.Utils.GetSafeStr(dr, "DEFAULT_INGEST_VALUE"),
                                CreateDate = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE")),
                                UpdateDate = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(ODBCWrapper.Utils.GetDateSafeVal(dr, "UPDATE_DATE"))
                            };

                            idToAssetStructMap[assetStructId].AssetStructMetas.Add(metaId, assetStructMeta);
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

        private static GenericResponse<Topic> CreateTopicResponseFromDataSet(DataSet ds)
        {
            GenericResponse<Topic> response = new GenericResponse<Topic>();
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
                        response.Object = CreateTopic(id, dt.Rows[0], topicTranslations);
                    }
                    else
                    {
                        response.Status = CreateTopicResponseStatusFromResult(id);
                        return response;
                    }
                }

                if (response.Object != null)
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

        private static GenericListResponse<ApiObjects.SearchObjects.TagValue> CreateTagListResponseFromDataSet(DataSet ds)
        {
            GenericListResponse<ApiObjects.SearchObjects.TagValue> response = new GenericListResponse<ApiObjects.SearchObjects.TagValue>();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "id", 0);
                    if (id > 0)
                    {
                        EnumerableRowCollection<DataRow> translations = ds.Tables.Count == 2 ? ds.Tables[1].AsEnumerable() : new DataTable().AsEnumerable();
                        List<DataRow> tagTranslations = (from row in translations
                                                         select row).ToList();
                        response.Objects.Add(CreateTag(id, dt.Rows[0], tagTranslations));
                    }
                    else
                    {
                        response.Status = CreateTagResponseStatusFromResult(id);
                        return response;
                    }
                }
                else
                {
                    response.Status = new Status((int)eResponseStatus.TagDoesNotExist, eResponseStatus.TagDoesNotExist.ToString());
                    return response;
                }

                if (response.Objects != null)
                {
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            else
            {
                response.Status = new Status((int)eResponseStatus.TagDoesNotExist, eResponseStatus.TagDoesNotExist.ToString());
            }

            return response;
        }

        private static ApiObjects.SearchObjects.TagValue CreateTag(long id, DataRow dr, List<DataRow> tagTranslations)
        {
            ApiObjects.SearchObjects.TagValue result = null;
            if (id > 0)
            {
                string name = ODBCWrapper.Utils.GetSafeStr(dr, "value");
                int topicId = ODBCWrapper.Utils.GetIntSafeVal(dr, "topic_id", 0);
                DateTime? createDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "CREATE_DATE");
                DateTime? updateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "UPDATE_DATE");
                List<LanguageContainer> tagsInOtherLanguages = new List<LanguageContainer>();
                if (tagTranslations != null && tagTranslations.Count > 0)
                {
                    foreach (DataRow translationDr in tagTranslations)
                    {
                        string languageCode = ODBCWrapper.Utils.GetSafeStr(translationDr, "CODE3");
                        string translation = ODBCWrapper.Utils.GetSafeStr(translationDr, "TRANSLATION");
                        if (!string.IsNullOrEmpty(languageCode) && !string.IsNullOrEmpty(translation))
                        {
                            tagsInOtherLanguages.Add(new LanguageContainer(languageCode, translation));
                        }
                    }
                }

                result = new ApiObjects.SearchObjects.TagValue()
                {
                    value = name,
                    tagId = id,
                    topicId = topicId,
                    TagsInOtherLanguages = tagsInOtherLanguages,
                    createDate = createDate.HasValue ? ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(createDate.Value) : 0,
                    updateDate = updateDate.HasValue ? ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(updateDate.Value) : 0
                };
            }

            return result;
        }

        private static Status CreateTagResponseStatusFromResult(long result)
        {
            Status responseStatus = null;
            switch (result)
            {
                case -222:
                    responseStatus = new Status((int)eResponseStatus.TagAlreadyInUse, eResponseStatus.TagAlreadyInUse.ToString());
                    break;
                case -333:
                    responseStatus = new Status((int)eResponseStatus.TagDoesNotExist, eResponseStatus.TagDoesNotExist.ToString());
                    break;
                default:
                    responseStatus = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    break;
            }

            return responseStatus;
        }

        private static bool InvalidateCacheAndUpdateIndexForTagAssets(int groupId, long tagId, bool shouldUpdateRowsStatus, long userId)
        {
            bool result = true;
            try
            {
                DataSet ds;
                if (shouldUpdateRowsStatus)
                {
                    // update and get all assets with tag
                    ds = CatalogDAL.UpdateTagAssets(groupId, tagId, userId);
                }
                else
                {
                    // get all assets with tag
                    ds = CatalogDAL.GetTagAssets(groupId, tagId);
                }

                // preparing media list and epg
                List<int> mediaIds = null;
                List<int> epgIds = null;

                CreateAssetsListForUpdateIndexFromDataSet(ds, out mediaIds, out epgIds);

                result = InvalidateCacheAndUpdateIndexForAssets(groupId, false, mediaIds, epgIds);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed InvalidateCacheAndUpdateIndexForTagAssets for groupId: {0}, tagId: {1}", groupId, tagId), ex);
                result = false;
            }

            return result;
        }

        private static bool InvalidateCacheAndUpdateIndexForTopicAssets(int groupId, List<long> tagTopicIds, bool shouldDeleteTag, bool shouldDeleteAssets, List<long> metaTopicIds,
                                                                        long assetStructId, long userId)
        {
            bool res = true;
            try
            {
                DataSet ds;
                // update and get all assets
                ds = CatalogDAL.UpdateTopicAssets(groupId, tagTopicIds, shouldDeleteTag, shouldDeleteAssets, metaTopicIds, assetStructId, userId);

                // preparing media list and epg
                List<int> mediaIds = null;
                List<int> epgIds = null;

                CreateAssetsListForUpdateIndexFromDataSet(ds, out mediaIds, out epgIds);

                res = InvalidateCacheAndUpdateIndexForAssets(groupId, shouldDeleteAssets, mediaIds, epgIds);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed InvalidateCacheAndUpdateIndexForTopicAssets for groupId: {0}, assetStructId: {1}, tagTopicIds: {2}, metaTopicIds: {3}",
                            groupId, assetStructId, tagTopicIds != null ? string.Join(",", tagTopicIds) : string.Empty, metaTopicIds != null ? string.Join(",", metaTopicIds) : string.Empty), ex);
                res = false;
            }

            return res;
        }        

        private static void CreateAssetsListForUpdateIndexFromDataSet(DataSet ds, out List<int> mediaIds, out List<int> epgIds)
        {
            mediaIds = new List<int>();
            epgIds = new List<int>();

            int assetId = 0;
            int assetType = 0;

            if (ds != null && ds.Tables != null && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    assetId = ODBCWrapper.Utils.GetIntSafeVal(dr, "ASSET_ID");
                    assetType = ODBCWrapper.Utils.GetIntSafeVal(dr, "ASSET_TYPE");

                    if (assetType == (int)eObjectType.Media)
                    {
                        mediaIds.Add(assetId);
                    }

                    if (assetType == (int)eObjectType.EPG)
                    {
                        epgIds.Add(assetId);
                    }
                }
            }
        }
        
        private static List<AssetStructMeta> CreateAssetStructMetaListFromDT(DataTable dt)
        {
            List<AssetStructMeta> assetStructMetaList = null;

            if (dt != null && dt.Rows != null)
            {
                assetStructMetaList = new List<AssetStructMeta>(dt.Rows.Count);

                foreach (DataRow dr in dt.Rows)
                {
                    AssetStructMeta assetStructMeta = new AssetStructMeta()
                    {
                        AssetStructId = ODBCWrapper.Utils.GetLongSafeVal(dr, "TEMPLATE_ID"),
                        MetaId = ODBCWrapper.Utils.GetLongSafeVal(dr, "TOPIC_ID"),
                        IngestReferencePath = ODBCWrapper.Utils.GetSafeStr(dr, "INGEST_REFERENCE_PATH"),
                        ProtectFromIngest = ODBCWrapper.Utils.ExtractBoolean(dr, "PROTECT_FROM_INGEST"),
                        DefaultIngestValue = ODBCWrapper.Utils.GetSafeStr(dr, "DEFAULT_INGEST_VALUE"),
                        CreateDate = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE")),
                        UpdateDate = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(ODBCWrapper.Utils.GetDateSafeVal(dr, "UPDATE_DATE"))
                    };

                    assetStructMetaList.Add(assetStructMeta);
                }
            }

            return assetStructMetaList;
        }

        #endregion

        #region Internal Methods

        internal static bool InvalidateCacheAndUpdateIndexForAssets(int groupId, bool shouldDeleteAssets, List<int> mediaIds, List<int> epgIds)
        {
            bool result = true;
            if (mediaIds != null && mediaIds.Count > 0)
            {
                eAction action = shouldDeleteAssets ? eAction.Delete : eAction.Update;
                // update medias index
                if (!Core.Catalog.Module.UpdateIndex(mediaIds, groupId, action))
                {
                    result = false;
                    log.ErrorFormat("Error while update Media index. groupId:{0}, mediaIds:{1}", groupId, string.Join(",", mediaIds));
                }

                // invalidate medias
                foreach (int mediaId in mediaIds)
                {
                    result = AssetManager.InvalidateAsset(eAssetTypes.MEDIA, mediaId) && result;
                }
            }

            // TODO: need to update epg object in CB
            if (epgIds != null && epgIds.Count > 0)
            {
                // update epgs index
                if (!Core.Catalog.Module.UpdateEpgIndex(epgIds, groupId, eAction.Update))
                {
                    result = false;
                    log.ErrorFormat("Error while update Epg index. groupId:{0}, epgIds:{1}", groupId, string.Join(",", epgIds));
                }

                // invalidate epgs
                foreach (int epgId in epgIds)
                {
                    result = AssetManager.InvalidateAsset(eAssetTypes.EPG, epgId) && result;
                }
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

        public static GenericListResponse<AssetStruct> GetAssetStructsByIds(int groupId, List<long> ids, bool? isProtected)
        {
            GenericListResponse<AssetStruct> response = new GenericListResponse<AssetStruct>();
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
                    response.Objects = ids.Where(x => catalogGroupCache.AssetStructsMapById.ContainsKey(x)).Select(x => catalogGroupCache.AssetStructsMapById[x]).ToList();
                }
                else
                {
                    response.Objects = catalogGroupCache.AssetStructsMapById.Values.ToList();
                }

                if (isProtected.HasValue)
                {
                    response.Objects = response.Objects.Where(x => x.IsPredefined.HasValue && x.IsPredefined == isProtected.Value).ToList();
                }

                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetStructsByIds with groupId: {0} and ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return response;
        }

        public static GenericListResponse<AssetStruct> GetAssetStructsByTopicId(int groupId, long topicId, bool? isProtected)
        {
            GenericListResponse<AssetStruct> response = new GenericListResponse<AssetStruct>();
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

                    response.Objects = catalogGroupCache.AssetStructsMapById.Values.Where(x => x.MetaIds.Contains(topicId)).ToList();
                    if (isProtected.HasValue)
                    {
                        response.Objects = response.Objects.Where(x => x.IsPredefined.HasValue && x.IsPredefined == isProtected.Value).ToList();
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

        public static GenericResponse<AssetStruct> AddAssetStruct(int groupId, AssetStruct assetStructToadd, long userId)
        {
            GenericResponse<AssetStruct> result = new GenericResponse<AssetStruct>();
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
                InvalidateCatalogGroupCache(groupId, result.Status, true, result.Object);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddAssetStruct for groupId: {0} and assetStruct: {1}", groupId, assetStructToadd.ToString()), ex);
            }

            return result;
        }
        
        public static GenericResponse<AssetStruct> UpdateAssetStruct(int groupId, long id, AssetStruct assetStructToUpdate, bool shouldUpdateMetaIds, long userId)
        {
            GenericResponse<AssetStruct> result = new GenericResponse<AssetStruct>();
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

                // check if assets and index should be updated now
                bool shouldUpdateAssetStructAssets = shouldUpdateMetaIds;
                List<long> removedTopicIds = new List<long>();
                if (shouldUpdateAssetStructAssets && assetStruct.MetaIds != null && assetStructToUpdate.MetaIds != null)
                {
                    removedTopicIds = assetStruct.MetaIds.Except(assetStructToUpdate.MetaIds).ToList();
                    // if its just the oreder of the metas being changed on the asset struct we don't need to update the assets
                    if (removedTopicIds == null || removedTopicIds.Count == 0)
                    {
                        shouldUpdateAssetStructAssets = false;
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
                                                          metaIdsToPriority, userId);
                result = CreateAssetStructResponseFromDataSet(ds);

                if (result.Status != null && result.Status.Code == (int)eResponseStatus.OK && shouldUpdateAssetStructAssets && removedTopicIds != null && removedTopicIds.Count > 0)
                {
                    List<long> tagTopicIds = new List<long>();
                    List<long> metaTopicIds = new List<long>();
                    foreach (long topicId in removedTopicIds)
                    {
                        if (catalogGroupCache.TopicsMapById.ContainsKey(topicId))
                        {
                            Topic topicToRemoveFromAssetStruct = catalogGroupCache.TopicsMapById[topicId];
                            if (topicToRemoveFromAssetStruct.Type == MetaType.Tag)
                            {
                                tagTopicIds.Add(topicId);
                            }
                            else
                            {
                                metaTopicIds.Add(topicId);
                            }
                        }
                    }

                    if (!InvalidateCacheAndUpdateIndexForTopicAssets(groupId, tagTopicIds, false, false, metaTopicIds, id, userId))
                    {
                        log.ErrorFormat("Failed InvalidateCacheAndUpdateIndexForTopicAssets for groupId: {0}, assetStructId: {1}, tagTopicIds: {2}, metaTopicIds: {3}",
                                        groupId, id, string.Join(",", tagTopicIds), string.Join(",", metaTopicIds));
                    }
                }

                InvalidateCatalogGroupCache(groupId, result.Status, true, result.Object);
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
                    List<long> tagTopicIds = new List<long>();
                    List<long> metaTopicIds = new List<long>();
                    foreach (long topicId in assetStruct.MetaIds)
                    {
                        if (catalogGroupCache.TopicsMapById.ContainsKey(topicId))
                        {
                            Topic topicToRemoveFromAssetStruct = catalogGroupCache.TopicsMapById[topicId];
                            if (topicToRemoveFromAssetStruct.Type == MetaType.Tag)
                            {
                                tagTopicIds.Add(topicId);
                            }
                            else
                            {
                                metaTopicIds.Add(topicId);
                            }
                        }
                    }

                    if (!InvalidateCacheAndUpdateIndexForTopicAssets(groupId, tagTopicIds, false, true, metaTopicIds, id, userId))
                    {
                        log.ErrorFormat("Failed InvalidateCacheAndUpdateIndexForTopicAssets for groupId: {0}, assetStructId: {1}, tagTopicIds: {2}, metaTopicIds: {3}",
                                        groupId, id, string.Join(",", tagTopicIds), string.Join(",", metaTopicIds));
                    }

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

        public static GenericListResponse<Topic> GetTopicsByIds(int groupId, List<long> ids, MetaType type)
        {
            GenericListResponse<Topic> response = new GenericListResponse<Topic>();
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
                    response.Objects = ids.Where(x => catalogGroupCache.TopicsMapById.ContainsKey(x) && (type == MetaType.All || catalogGroupCache.TopicsMapById[x].Type == type))
                                                .Select(x => catalogGroupCache.TopicsMapById[x]).ToList();
                }
                else
                {
                    response.Objects = catalogGroupCache.TopicsMapById.Values.Where(x => type == MetaType.All || x.Type == type).ToList();
                }

                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetTopicsByIds with groupId: {0} and ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return response;
        }

        public static GenericListResponse<Topic> GetTopicsByAssetStructId(int groupId, long assetStructId, MetaType type)
        {
            GenericListResponse<Topic> response = new GenericListResponse<Topic>();
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
                            response.Objects = topicIds.Where(x => catalogGroupCache.TopicsMapById.ContainsKey(x) && (type == MetaType.All || catalogGroupCache.TopicsMapById[x].Type == type))
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

        public static GenericResponse<Topic> AddTopic(int groupId, Topic topicToAdd, long userId)
        {
            GenericResponse<Topic> result = new GenericResponse<Topic>();
            try
            {
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
                InvalidateCatalogGroupCache(groupId, result.Status, true, result.Object);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddTopic for groupId: {0} and topic: {1}", groupId, topicToAdd.ToString()), ex);
            }

            return result;
        }

        public static GenericResponse<Topic> UpdateTopic(int groupId, long id, Topic topicToUpdate, long userId)
        {
            GenericResponse<Topic> result = new GenericResponse<Topic>();
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
                
                /************** According to new TVM UI system name can not be modified so no need for this validation *************************************************/
                //if (topic.IsPredefined.HasValue && topic.IsPredefined.Value && topicToUpdate.SystemName != null && topicToUpdate.SystemName != topic.SystemName)
                //{
                //    result.Status = new Status((int)eResponseStatus.CanNotChangePredefinedMetaSystemName, eResponseStatus.CanNotChangePredefinedMetaSystemName.ToString());
                //    return result;
                //}

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

                Topic topic = new Topic(catalogGroupCache.TopicsMapById[id]);
                DataSet ds = CatalogDAL.UpdateTopic(groupId, id, topicToUpdate.Name, shouldUpdateOtherNames, languageCodeToName, topicToUpdate.GetFeaturesForDB(topic.Features),
                                                    topicToUpdate.ParentId, topicToUpdate.HelpText, userId);
                result = CreateTopicResponseFromDataSet(ds);
                InvalidateCatalogGroupCache(groupId, result.Status, true, result.Object);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateTopic for groupId: {0}, id: {1} and topic: {2}", groupId, id, topicToUpdate.ToString()), ex);
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
                    bool isTag = topic.Type == MetaType.Tag;
                    List<long> tagTopicIds = new List<long>();
                    List<long> metaTopicIds = new List<long>();
                    if (isTag)
                    {
                        ElasticsearchWrapper wrapper = new ElasticsearchWrapper();
                        Status deleteTopicFromEsResult = wrapper.DeleteTagsByTopic(groupId, catalogGroupCache, id);
                        if (deleteTopicFromEsResult == null || deleteTopicFromEsResult.Code != (int)eResponseStatus.OK)
                        {
                            log.ErrorFormat("Failed deleting topic from ElasticSearch, for groupId: {0} and topicId: {1}", groupId, id);
                        }

                        tagTopicIds.Add(id);
                    }
                    else
                    {
                        metaTopicIds.Add(id);
                    }

                    // shouldDelete = isTag on purpose, since we are in DeleteTopic, if its a tag then delete it, on UpdateAssetStruct we don't delete the tag itself
                    if (!InvalidateCacheAndUpdateIndexForTopicAssets(groupId, tagTopicIds, isTag, false, metaTopicIds, 0, userId))
                    {
                        log.ErrorFormat("Failed InvalidateCacheAndUpdateIndexForTopicAssets for groupId: {0} and topicId: {1}, isTag: {2}", groupId, id, isTag);
                    }

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

                List<string> tags = catalogGroupCache.TopicsMapBySystemName.Where(x => x.Value.Type == ApiObjects.MetaType.Tag && !TopicsToIgnore.Contains(x.Value.SystemName)).Select(x => x.Key).ToList();
                List<string> metas = catalogGroupCache.TopicsMapBySystemName.Where(x => x.Value.Type != ApiObjects.MetaType.Tag && !TopicsToIgnore.Contains(x.Value.SystemName)).Select(x => x.Key).ToList();
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
                    foreach (string meta in metas)
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
            bool result = false;
            try
            {
                string key = LayeredCacheKeys.GetDoesGroupUsesTemplatesCacheKey(groupId);
                if (!LayeredCache.Instance.Get<bool>(key, ref result, DoesGroupUsesTemplates, new Dictionary<string, object>() { { "groupId", groupId } }, groupId,
                    LayeredCacheConfigNames.DOES_GROUP_USES_TEMPLATES_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetDoesGroupUsesTemplatesCacheInvalidationKey(groupId) }))
                {
                    log.ErrorFormat("Failed getting DoesGroupUsesTemplates from LayeredCache, groupId: {0}", groupId);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DoesGroupUsesTemplates with groupId: {0}", groupId), ex);
            }

            return result;
        }

        public static bool CheckMetaExsits(int groupId, string metaName)
        {
            bool result = false;
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling CheckMetaExsits", groupId);
                    return result;
                }

                result = catalogGroupCache.TopicsMapBySystemName.ContainsKey(metaName) && catalogGroupCache.TopicsMapBySystemName[metaName].Type != MetaType.Tag;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed CheckMetaExsits for groupId: {0}, metaName: {1}", groupId, metaName), ex);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static List<ApiObjects.SearchObjects.TagValue> GetAllTagValues(int groupId)
        {
            List<ApiObjects.SearchObjects.TagValue> result = new List<ApiObjects.SearchObjects.TagValue>();

            CatalogGroupCache catalogGroupCache;
            if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetGroupAssets", groupId);
                return null;
            }

            int defaultLanguage = catalogGroupCache.DefaultLanguage.ID;
            DataSet dataSet = null;
            try
            {
                dataSet = CatalogDAL.GetGroupTagValues(groupId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error when getting group tag values for groupId {0}. ex = {1}", groupId, ex);
                dataSet = null;
            }

            if (dataSet == null)
            {
                return null;
            }

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0 &&
                dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null && dataSet.Tables[0].Rows.Count > 0)
            {
                // First table - default language, set default for other languages as well
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    int tagId = ODBCWrapper.Utils.ExtractInteger(row, "tag_id");
                    int topicId = ODBCWrapper.Utils.ExtractInteger(row, "topic_id");
                    string tagValue = ODBCWrapper.Utils.ExtractString(row, "tag_value");
                    DateTime createDate = ODBCWrapper.Utils.ExtractDateTime(row, "create_date");
                    DateTime updateDate = ODBCWrapper.Utils.ExtractDateTime(row, "update_date");

                    if (catalogGroupCache.TopicsMapById.ContainsKey(topicId))
                    {
                        Topic topic = catalogGroupCache.TopicsMapById[topicId];

                        result.Add(new ApiObjects.SearchObjects.TagValue()
                        {
                            createDate = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(createDate),
                            languageId = defaultLanguage,
                            tagId = tagId,
                            topicId = topicId,
                            updateDate = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(updateDate),
                            value = tagValue
                        });
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
                    DateTime createDate = ODBCWrapper.Utils.ExtractDateTime(row, "create_date");
                    DateTime updateDate = ODBCWrapper.Utils.ExtractDateTime(row, "update_date");

                    if (catalogGroupCache.TopicsMapById.ContainsKey(topicId))
                    {
                        Topic topic = catalogGroupCache.TopicsMapById[topicId];

                        result.Add(new ApiObjects.SearchObjects.TagValue()
                        {
                            createDate = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(createDate),
                            languageId = languageId,
                            tagId = tagId,
                            topicId = topicId,
                            updateDate = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(updateDate),
                            value = tagValue
                        });
                    }
                }
            }

            return result;
        }

        public static GenericResponse<ApiObjects.SearchObjects.TagValue> AddTag(int groupId, ApiObjects.SearchObjects.TagValue tag, long userId)
        {
            GenericResponse<ApiObjects.SearchObjects.TagValue> result = new GenericResponse<ApiObjects.SearchObjects.TagValue>();
            try
            {
                List<KeyValuePair<string, string>> languageCodeToName = new List<KeyValuePair<string, string>>();
                if (tag.TagsInOtherLanguages != null && tag.TagsInOtherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in tag.TagsInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.LanguageCode, language.Value));
                    }
                }

                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling AddTag", groupId);
                    return result;
                }

                if (catalogGroupCache.TopicsMapById != null && catalogGroupCache.TopicsMapById.Count > 0)
                {
                    bool topic = catalogGroupCache.TopicsMapById.ContainsKey(tag.topicId);
                    if (!topic)
                    {
                        result.Status.Code = (int)eResponseStatus.TopicNotFound;
                        result.Status.Message = eResponseStatus.TopicNotFound.ToString();
                        log.ErrorFormat("Error at AddTag. TopicId not found. GroupId: {0}", groupId);
                        return result;
                    }
                }

                DataSet ds = CatalogDAL.InsertTag(groupId, tag.value, languageCodeToName, tag.topicId, userId);
                GenericListResponse<ApiObjects.SearchObjects.TagValue> tagListResponse = CreateTagListResponseFromDataSet(ds);
                result.Object = tagListResponse.Objects.FirstOrDefault();
                result.Status = tagListResponse.Status;

                if (result.Status.Code != (int)eResponseStatus.OK)
                {
                    return result;
                }

                ElasticsearchWrapper wrapper = new ElasticsearchWrapper();
                result.Status = wrapper.UpdateTag(groupId, catalogGroupCache, result.Object);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddTag for groupId: {0} and tag: {1}", groupId, tag.ToString()), ex);
            }

            return result;
        }

        public static GenericResponse<ApiObjects.SearchObjects.TagValue> UpdateTag(int groupId, long id, ApiObjects.SearchObjects.TagValue tagToUpdate, long userId)
        {
            GenericResponse<ApiObjects.SearchObjects.TagValue> result = new GenericResponse<ApiObjects.SearchObjects.TagValue>();

            try
            {
                GenericListResponse<ApiObjects.SearchObjects.TagValue> tagByIdListResponse = GetTagListResponseById(groupId, id);
                result.Object = tagByIdListResponse.Objects.FirstOrDefault();
                result.Status = tagByIdListResponse.Status;

                if (result.Status.Code != (int)eResponseStatus.OK)
                {
                    return result;
                }
                
                List<KeyValuePair<string, string>> languageCodeToName = null;
                bool shouldUpdateOtherNames = false;
                if (tagToUpdate.TagsInOtherLanguages != null)
                {
                    shouldUpdateOtherNames = true;
                    languageCodeToName = new List<KeyValuePair<string, string>>();
                    foreach (LanguageContainer language in tagToUpdate.TagsInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.LanguageCode, language.Value));
                    }
                }

                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpdateTag", groupId);
                    return result;
                }

                if (catalogGroupCache.TopicsMapById != null && catalogGroupCache.TopicsMapById.Count > 0)
                {
                    bool topic = catalogGroupCache.TopicsMapById.ContainsKey(tagToUpdate.topicId);
                    if (!topic)
                    {
                        result.Status.Code = (int)eResponseStatus.TopicNotFound;
                        result.Status.Message = eResponseStatus.TopicNotFound.ToString();
                        log.ErrorFormat("Error at UpdateTag. TopicId not found. GroupId: {0}", groupId);
                        return result;
                    }
                }

                DataSet ds = CatalogDAL.UpdateTag(groupId, id, tagToUpdate.value, shouldUpdateOtherNames, languageCodeToName, tagToUpdate.topicId, userId);
                tagByIdListResponse = CreateTagListResponseFromDataSet(ds);
                result.Object = tagByIdListResponse.Objects.FirstOrDefault();
                result.Status = tagByIdListResponse.Status;

                if (result.Status.Code != (int)eResponseStatus.OK)
                {
                    return result;
                }

                if (!InvalidateCacheAndUpdateIndexForTagAssets(groupId, id, false, userId))
                {
                    log.ErrorFormat("Failed to InvalidateCacheAndUpdateIndexForTagAssets after UpdateTag for groupId: {0}, tagId: {1}", groupId, id);
                }

                ElasticsearchWrapper wrapper = new ElasticsearchWrapper();
                result.Status = wrapper.UpdateTag(groupId, catalogGroupCache, result.Object);

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateTag for groupId: {0}, id: {1} and tagToUpdate: {2}", groupId, id, tagToUpdate.ToString()), ex);
            }

            return result;
        }

        public static Status DeleteTag(int groupId, long tagId, long userId)
        {
            GenericResponse<ApiObjects.SearchObjects.TagValue> tagResponse = new GenericResponse<ApiObjects.SearchObjects.TagValue>();

            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                GenericListResponse<ApiObjects.SearchObjects.TagValue> tagListResponse = GetTagListResponseById(groupId, tagId);
                tagResponse.Object = tagListResponse.Objects.FirstOrDefault();
                tagResponse.Status = tagListResponse.Status;

                if (tagResponse.Status.Code != (int)eResponseStatus.OK)
                {
                    return tagResponse.Status;
                }

                if (CatalogDAL.DeleteTag(groupId, tagId, userId))
                {
                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    CatalogGroupCache catalogGroupCache;
                    if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling DeleteTag", groupId);
                        return result;
                    }

                    ElasticsearchWrapper wrapper = new ElasticsearchWrapper();
                    result = wrapper.DeleteTag(groupId, catalogGroupCache, tagId);

                    if (result.Code != (int)eResponseStatus.OK)
                    {
                        return result;
                    }

                    if (!InvalidateCacheAndUpdateIndexForTagAssets(groupId, tagId, true, userId))
                    {
                        log.ErrorFormat("Failed to InvalidateCacheAndUpdateIndexForTagAssets after UpdateTag for groupId: {0}, tagId: {1}", groupId, tagId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteTag for groupId: {0} and tagId: {1}", groupId, tagId), ex);
            }

            return result;
        }

        /// <summary>
        /// Returns all tag values for all languages by a given tag ID
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="tagId"></param>
        /// <returns></returns>
        public static GenericListResponse<ApiObjects.SearchObjects.TagValue> GetTagListResponseById(int groupId, long tagId)
        {
            GenericListResponse<ApiObjects.SearchObjects.TagValue> result = new GenericListResponse<ApiObjects.SearchObjects.TagValue>();

            try
            {
                DataSet ds = CatalogDAL.GetTag(groupId, tagId);
                result = CreateTagListResponseFromDataSet(ds);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetTagListResponseById for groupId: {0} and tagId: {1}", groupId, tagId), ex);
            }

            return result;
        }

        public static GenericListResponse<ApiObjects.SearchObjects.TagValue> SearchTags(int groupId, bool isExcatValue, string searchValue, int topicId, int searchLanguageId, int pageIndex, int pageSize)
        {
            GenericListResponse<ApiObjects.SearchObjects.TagValue> result = new GenericListResponse<ApiObjects.SearchObjects.TagValue>();
            CatalogGroupCache catalogGroupCache;
            if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling SearchTags", groupId);
                return result;
            }

            LanguageObj searchLanguage = null;
            if (searchLanguageId != 0)
            {
                catalogGroupCache.LanguageMapById.TryGetValue(searchLanguageId, out searchLanguage);

                if (searchLanguage == null)
                {
                    log.ErrorFormat("Invalid language id {0}", searchLanguageId);
                    return result;
                }
            }

            ApiObjects.SearchObjects.TagSearchDefinitions definitions = new ApiObjects.SearchObjects.TagSearchDefinitions()
            {
                GroupId = groupId,
                Language = searchLanguage,
                PageIndex = pageIndex,
                PageSize = pageSize,
                AutocompleteSearchValue = !isExcatValue && !string.IsNullOrEmpty(searchValue) ? searchValue.Trim() : string.Empty,
                ExactSearchValue = isExcatValue && !string.IsNullOrEmpty(searchValue) ? searchValue : string.Empty,
                TopicId = topicId
            };

            int totalItemsCount = 0;
            ElasticsearchWrapper wrapper = new ElasticsearchWrapper();
            List<ApiObjects.SearchObjects.TagValue> tagValues = wrapper.SearchTags(definitions, catalogGroupCache, out totalItemsCount);


            List<GenericListResponse<ApiObjects.SearchObjects.TagValue>> tagListResponseList = new List<GenericListResponse<ApiObjects.SearchObjects.TagValue>>();
            HashSet<long> tagIds = new HashSet<long>();

            foreach (ApiObjects.SearchObjects.TagValue tagValue in tagValues)
            {
                if (!tagIds.Contains(tagValue.tagId))
                {
                    //TODO SHIR - ASK LIOR IF THIS OK?
                    GenericListResponse<ApiObjects.SearchObjects.TagValue> tagListResponse = GetTagListResponseById(groupId, tagValue.tagId);
                    tagListResponseList.Add(tagListResponse);
                    tagIds.Add(tagValue.tagId);
                    result.Objects.AddRange(tagListResponse.Objects);

                    //GenericListResponse<ApiObjects.SearchObjects.TagValue> tagListResponse1 = GetTagListResponseById(groupId, tagValue.tagId);
                    //tagIds.Add(tagValue.tagId);
                    //result.Objects.AddRange(tagListResponse1.TagValues);
                }
            }

            result.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            result.TotalItems = totalItemsCount;

            return result;
        }

        public static GenericResponse<AssetStructMeta> UpdateAssetStructMeta(long assetStructId, long MetaId, AssetStructMeta assetStructMeta, int groupId, long userId)
        {
            GenericResponse<AssetStructMeta> response = new GenericResponse<AssetStructMeta>();

            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpdateAssetStructMeta", groupId);
                    return response;
                }

                if (!catalogGroupCache.AssetStructsMapById.ContainsKey(assetStructId))
                {
                    response.Status = new Status((int)eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString());
                    return response;
                }

                if (!catalogGroupCache.AssetStructsMapById[assetStructId].MetaIds.Contains(MetaId))
                {
                    response.Status = new Status((int)eResponseStatus.MetaDoesNotExist, eResponseStatus.MetaDoesNotExist.ToString());
                    return response;
                }

                DataTable dt = CatalogDAL.UpdateAssetStructMeta
                    (assetStructId, MetaId, assetStructMeta.IngestReferencePath, assetStructMeta.ProtectFromIngest, assetStructMeta.DefaultIngestValue, groupId, userId);
                
                List<AssetStructMeta> assetStructMetaList = CreateAssetStructMetaListFromDT(dt);

                if (assetStructMetaList != null)
                {
                    response.Object = assetStructMetaList.FirstOrDefault();
                }

                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                InvalidateCatalogGroupCache(groupId, response.Status, true, response.Object);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateAssetStructMeta for groupId: {0}, assetStructId: {1}, MetaId: {2} and assetStructMeta: {3}",
                                        groupId, assetStructId, MetaId, assetStructMeta.ToString()), ex);
            }

            return response;
        }
        
        public static GenericListResponse<AssetStructMeta> GetAssetStructMetaList(int groupId, long? assetStructId, long? metaId)
        {
            GenericListResponse<AssetStructMeta> response = new GenericListResponse<AssetStructMeta>();

            try
            {
                if (assetStructId.HasValue)
                {
                    CatalogGroupCache catalogGroupCache;
                    if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetAssetStructMetaList", groupId);
                        return response;
                    }

                    if (catalogGroupCache.AssetStructsMapById.ContainsKey(assetStructId.Value))
                    {
                        if (metaId.HasValue)
                        {
                            if (catalogGroupCache.AssetStructsMapById[assetStructId.Value].AssetStructMetas.ContainsKey(metaId.Value))
                            {
                                response.Objects.Add(catalogGroupCache.AssetStructsMapById[assetStructId.Value].AssetStructMetas[metaId.Value]);
                            }
                        }
                        else
                        {
                            response.Objects.AddRange(catalogGroupCache.AssetStructsMapById[assetStructId.Value].AssetStructMetas.Values);
                        }
                    }

                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    DataTable dt = CatalogDAL.GetAssetStructMetaList(groupId, metaId.Value);
                    response.Objects = CreateAssetStructMetaListFromDT(dt);
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetStructMetaList with groupId: {0}, assetStructId: {1} and metaId: {2}", groupId, assetStructId, metaId), ex);
            }

            return response;
        }

        #endregion
    }
}
