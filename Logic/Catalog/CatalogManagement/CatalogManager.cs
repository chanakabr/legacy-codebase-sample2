using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
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

        #region Private Methods

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

        private static AssetStruct CreateAssetStructFromIdAndDataRow(long id, DataRow dr)
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
                    result = new AssetStruct(id, name, systemName, isPredefined, createDate.HasValue ? ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(createDate.Value) : 0,
                                             updateDate.HasValue ? ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(updateDate.Value) : 0);
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
                        response.AssetStruct = CreateAssetStructFromIdAndDataRow(id, dt.Rows[0]);
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

                if (response.AssetStruct != null && ds.Tables.Count == 2)
                {
                    DataTable metasDt = ds.Tables[1];
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
            if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
            {
                Dictionary<long, AssetStruct> idToAssetStructMap = new Dictionary<long, AssetStruct>();
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                        if (id > 0 && !idToAssetStructMap.ContainsKey(id))
                        {
                            AssetStruct assetStruct = CreateAssetStructFromIdAndDataRow(id, dr);
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

        private static Tuple<Dictionary<string, AssetStruct>, bool> GetAssetStructs(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, AssetStruct> result = new Dictionary<string, AssetStruct>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("assetStructIds") && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;                    
                    List<long> assetStructIds = null;                    
                    if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                    {
                        assetStructIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => long.Parse(x)).ToList();
                    }
                    else
                    {
                        assetStructIds = funcParams["assetStructIds"] != null ? funcParams["assetStructIds"] as List<long> : null;
                    }

                    if (assetStructIds != null && assetStructIds.Count > 0 && groupId.HasValue)
                    {                        
                        DataSet ds = CatalogDAL.GetAssetStructsByIds(groupId.Value, assetStructIds);
                        List<AssetStruct> assetStructs = CreateAssetStructListFromDataSet(ds);
                        if (assetStructs != null && assetStructs.Count > 0)
                        {
                            result = assetStructs.ToDictionary(x => LayeredCacheKeys.GetAssetStructKey(groupId.Value, x.Id), x => x);
                        }
                    }

                    res = result.Keys.Count() == assetStructIds.Count();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetAssetStructs failed params : {0}", funcParams != null ? string.Join(";",
                         funcParams.Select(x => string.Format("key:{0}, value: {1}", x.Key, x.Value.ToString())).ToList()) : string.Empty), ex);
            }

            return new Tuple<Dictionary<string, AssetStruct>, bool>(result, res);
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

        private static Topic CreateTopicFromIdAndDataRow(long id, DataRow dr)
        {
            Topic result = null;
            if (id > 0)
            {
                string name = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                string systemName = ODBCWrapper.Utils.GetSafeStr(dr, "SYSTEM_NAME");
                string topicType = ODBCWrapper.Utils.GetSafeStr(dr, "TOPIC_TYPE");
                MetaType type;
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(systemName) && Enum.TryParse<MetaType>(topicType, out type))
                {
                    string commaSeparatedFeatures = ODBCWrapper.Utils.GetSafeStr(dr, "FEATURES");
                    HashSet<string> features = null;
                    if (!string.IsNullOrEmpty(commaSeparatedFeatures))
                    {
                        features = new HashSet<string>(commaSeparatedFeatures.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    bool isPredefined = ODBCWrapper.Utils.ExtractBoolean(dr, "IS_BASIC");
                    //TODO: Lior -  get multipleValue from features and consider doing the same for isPredefined
                    bool multipleValue = false;
                    string helpText = ODBCWrapper.Utils.GetSafeStr(dr, "HELP_TEXT");
                    long parentId = ODBCWrapper.Utils.GetLongSafeVal(dr, "PARENT_TOPIC_ID", 0);
                    DateTime? createDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "CREATE_DATE");
                    DateTime? updateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "UPDATE_DATE");
                    result = new Topic(id, name, systemName, type, features, isPredefined, multipleValue, helpText, parentId,
                                        createDate.HasValue ? ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(createDate.Value) : 0,
                                        updateDate.HasValue ? ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(updateDate.Value) : 0);
                }
            }

            return result;
        }

        private static TopicResponse CreateTopicResponseFromDataTable(DataTable dt)
        {
            TopicResponse response = new TopicResponse();
            if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
            {
                long id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID", 0);
                if (id > 0)
                {
                    response.Topic = CreateTopicFromIdAndDataRow(id, dt.Rows[0]);
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

            return response;
        }

        private static List<Topic> CreateTopicListFromDataTable(DataTable dt)
        {
            List<Topic> response = null;
            if (dt != null && dt.Rows != null)
            {
                response = new List<Topic>();
                foreach (DataRow dr in dt.Rows)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                    if (id > 0)
                    {
                        Topic topic = CreateTopicFromIdAndDataRow(id, dr);
                        if (topic != null)
                        {
                            response.Add(topic);
                        }
                    }
                }
            }            

            return response;
        }

        private static Tuple<Dictionary<string, Topic>, bool> GetTopics(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, Topic> result = new Dictionary<string, Topic>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("topicIds") && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("topicType"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    int? topicType = funcParams["topicType"] as int?;
                    List<long> topicIds = null;
                    if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                    {
                        topicIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => long.Parse(x)).ToList();
                    }
                    else
                    {
                        topicIds = funcParams["topicIds"] != null ? funcParams["topicIds"] as List<long> : null;
                    }

                    if (topicIds != null && topicIds.Count > 0 && groupId.HasValue && topicType.HasValue)
                    {
                        DataTable dt = CatalogDAL.GetTopicByIds(groupId.Value, topicIds, topicType.Value);
                        List<Topic> topics = CreateTopicListFromDataTable(dt);
                        if (topics != null && topics.Count > 0)
                        {
                            result = topics.ToDictionary(x => LayeredCacheKeys.GetTopicKey(groupId.Value, x.Id), x => x);
                        }
                    }

                    res = result.Keys.Count() == topicIds.Count();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetTopics failed params : {0}", funcParams != null ? string.Join(";",
                         funcParams.Select(x => string.Format("key:{0}, value: {1}", x.Key, x.Value.ToString())).ToList()) : string.Empty), ex);
            }

            return new Tuple<Dictionary<string, Topic>, bool>(result, res);
        }

        #endregion

        #region Public Methods

        public static List<AssetStruct> TryGetAssetStructsFromCache(int groupId, List<long> ids)
        {
            List<AssetStruct> result = null;
            try
            {
                Dictionary<string, AssetStruct> assetStructsMap = null;
                Dictionary<string, string> keyToOriginalValueMap = LayeredCacheKeys.GetAssetStructsKeysMap(groupId, ids);
                Dictionary<string, List<string>> invalidationKeysMap = LayeredCacheKeys.GetAssetStructsInvalidationKeysMap(groupId, ids);
                if (!LayeredCache.Instance.GetValues<AssetStruct>(keyToOriginalValueMap, ref assetStructsMap, GetAssetStructs,
                    new Dictionary<string, object>() { { "groupId", groupId }, { "assetStructIds", ids } },
                    groupId, LayeredCacheConfigNames.GET_ASSET_STRUCTS_CACHE_CONFIG_NAME, invalidationKeysMap))
                {
                    log.ErrorFormat("Failed getting AssetStructs from LayeredCache, groupId: {0}, assetStructIds", groupId, string.Join(",", ids));
                }
                else if (assetStructsMap != null)
                {
                    result = assetStructsMap.Count > 0 ? assetStructsMap.Values.ToList() : new List<AssetStruct>();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetAssetStructsFromCache with groupId: {0} and ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return result;
        }

        public static AssetStructListResponse GetAssetStructsByIds(int groupId, List<long> ids)
        {
            AssetStructListResponse response = new AssetStructListResponse();
            try
            {
                response.AssetStructs = TryGetAssetStructsFromCache(groupId, ids);
                if (response.AssetStructs != null)
                {
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetStructsByIds with groupId: {0} and ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return response;
        }

        public static AssetStructListResponse GetAssetStructsByTopicIds(int groupId, List<long> topicIds)
        {
            AssetStructListResponse response = new AssetStructListResponse();
            try
            {
                DataSet ds = CatalogDAL.GetAssetStructsByTopicIds(groupId, topicIds);
                response.AssetStructs = CreateAssetStructListFromDataSet(ds);
                if (response.AssetStructs != null)
                {
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetStructsByTopicIds with groupId: {0} and topicIds: {1}", groupId, topicIds != null ? string.Join(",", topicIds) : string.Empty), ex);
            }

            return response;
        }        

        public static AssetStructResponse AddAssetStruct(int groupId, AssetStruct assetStructToadd, long userId)
        {
            AssetStructResponse result = new AssetStructResponse();
            try
            {
                // TODO: Lior - check that meta Ids exist
                if (assetStructToadd.MetaIds == null)
                {
                    result.Status = new Status((int)eResponseStatus.MetaIdsDoesNotExist, eResponseStatus.MetaIdsDoesNotExist.ToString());
                }
                //TODO: Lior - check that meta Ids exist
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
                DataSet ds = CatalogDAL.InsertAssetStruct(groupId, assetStructToadd.Name, assetStructToadd.SystemName, metaIdsToPriority, assetStructToadd.IsPredefined, userId);
                result = CreateAssetStructResponseFromDataSet(ds);
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
                List<AssetStruct> assetStructs = TryGetAssetStructsFromCache(groupId, new List<long>() { id });
                if (assetStructs == null || assetStructs.Count != 1)
                {
                    result.Status = new Status((int)eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString());
                    return result;
                }

                AssetStruct assetStruct = assetStructs.First();
                if (assetStruct.IsPredefined.HasValue && assetStruct.IsPredefined.Value && assetStructToUpdate.SystemName != null)
                {
                    result.Status = new Status((int)eResponseStatus.CanNotChangePredefinedAssetStructSystemName, eResponseStatus.CanNotChangePredefinedAssetStructSystemName.ToString());
                    return result;
                }

                List<KeyValuePair<long, int>> metaIdsToPriority = null;
                if (assetStructToUpdate.MetaIds != null || shouldUpdateMetaIds)
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

                DataSet ds = CatalogDAL.UpdateAssetStruct(groupId, id, assetStructToUpdate.Name, assetStructToUpdate.SystemName, shouldUpdateMetaIds, metaIdsToPriority, userId);
                result = CreateAssetStructResponseFromDataSet(ds);
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
                List<AssetStruct> assetStructs = TryGetAssetStructsFromCache(groupId, new List<long>() { id });
                if (assetStructs == null || assetStructs.Count != 1)
                {
                    result = new Status((int)eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString());
                    return result;
                }

                AssetStruct assetStruct = assetStructs.First();
                if (assetStruct.IsPredefined.HasValue && assetStruct.IsPredefined.Value)
                {
                    result = new Status((int)eResponseStatus.CanNotDeletePredefinedAssetStruct, eResponseStatus.CanNotDeletePredefinedAssetStruct.ToString());
                    return result;
                }

                if (CatalogDAL.DeleteAssetStruct(groupId, id, userId))
                {
                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteAssetStruct for groupId: {0} and assetStructId: {1}", groupId, id), ex);
            }

            return result;
        }

        public static List<Topic> TryGetTopicsFromCache(int groupId, List<long> ids, MetaType type)
        {
            List<Topic> result = null;
            try
            {
                Dictionary<string, Topic> topicsMap = null;
                Dictionary<string, string> keyToOriginalValueMap = LayeredCacheKeys.GetTopicsKeysMap(groupId, ids);
                Dictionary<string, List<string>> invalidationKeysMap = LayeredCacheKeys.GetTopicsInvalidationKeysMap(groupId, ids);
                if (!LayeredCache.Instance.GetValues<Topic>(keyToOriginalValueMap, ref topicsMap, GetTopics,
                    new Dictionary<string, object>() { { "groupId", groupId }, { "topicIds", ids }, { "topicType", (int)type } },
                    groupId, LayeredCacheConfigNames.GET_TOPICS_CACHE_CONFIG_NAME, invalidationKeysMap))
                {
                    log.ErrorFormat("Failed getting Topics from LayeredCache, groupId: {0}, topicIds", groupId, string.Join(",", ids));
                }
                else if (topicsMap != null)
                {
                    result = topicsMap.Count > 0 ? topicsMap.Values.ToList() : new List<Topic>();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetTopicsFromCache with groupId: {0} and ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return result;
        }

        public static TopicListResponse GetTopicsByIds(int groupId, List<long> ids, MetaType type)
        {
            TopicListResponse response = new TopicListResponse();
            try
            {
                response.Topics = TryGetTopicsFromCache(groupId, ids, type);
                if (response.Topics != null)
                {
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetTopicsByIds with groupId: {0} and ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return response;
        }

        public static TopicListResponse GetTopicsByAssetStructIds(int groupId, List<long> assetStructIds, MetaType type)
        {
            TopicListResponse response = new TopicListResponse();
            try
            {
                DataTable dt = CatalogDAL.GetTopicByAssetStructIds(groupId, assetStructIds, (int)type);
                response.Topics = CreateTopicListFromDataTable(dt);
                if (response.Topics != null)
                {
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetTopicsByAssetStructIds with groupId: {0} and assetStructIds: {1}", groupId, assetStructIds != null ? string.Join(",", assetStructIds) : string.Empty), ex);
            }

            return response;
        }

        public static TopicResponse AddTopic(int groupId, Topic topicToAdd, long userId)
        {
            TopicResponse result = new TopicResponse();
            try
            {
                //TODO: Lior - do something with features
                DataTable dt = CatalogDAL.InsertTopic(groupId, topicToAdd.Name, topicToAdd.SystemName, topicToAdd.Type, topicToAdd.GetCommaSeparatedFeatures(),
                                                      topicToAdd.IsPredefined, topicToAdd.ParentId, topicToAdd.HelpText, userId);
                result = CreateTopicResponseFromDataTable(dt);
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
                List<Topic> topics = TryGetTopicsFromCache(groupId, new List<long>() { id }, topicToUpdate.Type);
                if (topics == null || topics.Count != 1)
                {
                    result.Status = new Status((int)eResponseStatus.MetaDoesNotExist, eResponseStatus.MetaDoesNotExist.ToString());
                    return result;
                }

                Topic assetStruct = topics.First();
                if (assetStruct.IsPredefined.HasValue && assetStruct.IsPredefined.Value && topicToUpdate.SystemName != null)
                {
                    result.Status = new Status((int)eResponseStatus.CanNotChangePredefinedMetaSystemName, eResponseStatus.CanNotChangePredefinedMetaSystemName.ToString());
                    return result;
                }

                //TODO: Lior - do something with features

                //TODO: Lior - support changing system name for topic???
                // SAME for AssetStruct???

                DataTable dt = CatalogDAL.UpdateTopic(groupId, id, topicToUpdate.Name, topicToUpdate.SystemName, topicToUpdate.GetCommaSeparatedFeatures(),
                                                      topicToUpdate.ParentId, topicToUpdate.HelpText, userId);
                result = CreateTopicResponseFromDataTable(dt);
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
                List<Topic> assetStructs = TryGetTopicsFromCache(groupId, new List<long>() { id }, MetaType.All);
                if (assetStructs == null || assetStructs.Count != 1)
                {
                    result = new Status((int)eResponseStatus.MetaDoesNotExist, eResponseStatus.MetaDoesNotExist.ToString());
                    return result;
                }

                Topic assetStruct = assetStructs.First();
                if (assetStruct.IsPredefined.HasValue && assetStruct.IsPredefined.Value)
                {
                    result = new Status((int)eResponseStatus.CanNotDeletePredefinedMeta, eResponseStatus.CanNotDeletePredefinedMeta.ToString());
                    return result;
                }

                if (CatalogDAL.DeleteTopic(groupId, id, userId))
                {
                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteTopic for groupId: {0} and assetStructId: {1}", groupId, id), ex);
            }

            return result;
        }

        #endregion

    }
}