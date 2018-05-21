using ApiObjects;
using ApiObjects.AssetLifeCycleRules;
using ApiObjects.Response;
using ApiObjects.Rules;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using ConfigurationManager;
using Core.Catalog.Request;
using Core.Catalog.Response;
using CouchbaseManager;
using DAL;
using GroupsCacheManager;
using KLogMonitor;
using KlogMonitorHelper;
using Newtonsoft.Json;
using QueueWrapper;
using ScheduledTasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;

namespace Core.Api.Managers
{
    public class AssetUserRuleManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string ASSET_USER_RULE_DOES_NOT_EXIST = "Asset user rule does not exist";
        private const string DELETE_ASSET_USER_RULE_FAILED = "failed to delete Asset user rule";
        private const string DELETE_USER_ASSET_USER_RULE_FAILED = "failed to delete Asset user rule from user";
        private const string ASSET_USER_RULE_FAILED_UPDATE = "failed to update Asset user rule";
        private const string ASSET_USER_RULE_NOT_FOUND = "No Asset User Rules found";
        
        internal static GenericListResponse<AssetUserRule> GetAssetUserRuleList(int groupId, long? userId)
        {
            GenericListResponse<AssetUserRule> response = new GenericListResponse<AssetUserRule>();

            try
            {

                List<long> assetUserRuleIds = new List<long>();

                if (!userId.HasValue)
                {
                    string key = LayeredCacheKeys.GetAssetUserRuleIdsGroupKey(groupId);

                    if (!LayeredCache.Instance.Get<List<long>>(key,
                                                               ref assetUserRuleIds,
                                                               GetAssetUserRuleIdsByGroupDB,
                                                               new Dictionary<string, object>() { { "groupId", groupId } },
                                                               groupId,
                                                               LayeredCacheConfigNames.GET_ASSET_USER_RULE_IDS_BY_GROUP,
                                                               new List<string>() { LayeredCacheKeys.GetAssetUserRuleIdsGroupInvalidationKey(groupId) }))
                    {
                        log.ErrorFormat("GetAssetUserRuleList - GetAssetUserRuleIdsByGroupDB - Failed get data from cache. groupId: {0}", groupId);
                        return response;
                    }
                }
                else
                {
                    if (!TryGetUserAssetUserRuleIds(userId.Value, groupId, out assetUserRuleIds))
                    {
                        return response;
                    }
                }

                if (assetUserRuleIds == null || assetUserRuleIds.Count == 0)
                {
                    response.Status = new Status((int)eResponseStatus.OK, ASSET_USER_RULE_NOT_FOUND);
                    return response;
                }

                Dictionary<string, string> keysToOriginalValueMap = new Dictionary<string, string>();
                Dictionary<string, List<string>> invalidationKeysMap = new Dictionary<string, List<string>>();
                
                foreach (long assetUserRuleId in assetUserRuleIds)
                {
                    string assetUserRuleKey = LayeredCacheKeys.GetAssetUserRuleKey(assetUserRuleId);
                    keysToOriginalValueMap.Add(assetUserRuleKey, assetUserRuleId.ToString());
                    invalidationKeysMap.Add(assetUserRuleKey, new List<string>() { LayeredCacheKeys.GetAssetUserRuleInvalidationKey(assetUserRuleId) });
                }

                Dictionary<string, AssetUserRule> fullAssetUserRules = null;
            
                // try to get full AssetUserRules from cache            
                if (LayeredCache.Instance.GetValues<AssetUserRule>(keysToOriginalValueMap, 
                                                                   ref fullAssetUserRules,
                                                                   GetAssetUserRulesCB, 
                                                                   new Dictionary<string, object>() { { "ruleIds", keysToOriginalValueMap.Values.ToList() } }, 
                                                                   groupId, 
                                                                   LayeredCacheConfigNames.GET_ASSET_USER_RULE, 
                                                                   invalidationKeysMap))
                {
                    if (fullAssetUserRules != null && fullAssetUserRules.Count > 0)
                    {
                        response.Objects = fullAssetUserRules.Values.ToList();
                    }
                }

                response.Status.Code = (int)eResponseStatus.OK;
                if (response.Objects == null || response.Objects.Count == 0)
                {
                    response.Status.Message = ASSET_USER_RULE_NOT_FOUND;
                }
                else
                {
                    response.Status.Message = eResponseStatus.OK.ToString();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed GetAssetUserRuleList groupID: {0}, ex: {1}", groupId, ex);
            }

            return response;
        }
        
        internal static GenericResponse<AssetUserRule> AddAssetUserRule(int groupId, AssetUserRule assetUserRuleToAdd)
        {
            GenericResponse<AssetUserRule> response = new GenericResponse<AssetUserRule>();
            assetUserRuleToAdd.GroupId = groupId;

            try
            {
                // add asset user rule in DB
                DataTable dt = ApiDAL.AddAssetRule(groupId, assetUserRuleToAdd.Name, assetUserRuleToAdd.Description, (int)AssetRuleType.AssetUserRule);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    assetUserRuleToAdd.Id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID");

                    // add asset user rule in CB
                    if (!ApiDAL.SaveAssetUserRuleCB(assetUserRuleToAdd))
                    {
                        log.ErrorFormat("Error while saving AssetUserRule. groupId: {0}, assetUserRuleId:{1}", groupId, assetUserRuleToAdd.Id);
                    }
                    else
                    {
                        LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAssetUserRuleIdsGroupInvalidationKey(groupId));
                        
                        response.Object = assetUserRuleToAdd;
                        response.Status.Code = (int)eResponseStatus.OK;
                        response.Status.Message = eResponseStatus.OK.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while adding new AssetUserRule. groupId: {0}, assetUserRule: {1}, ex: {2}", groupId, JsonConvert.SerializeObject(assetUserRuleToAdd), ex);
            }

            return response;
        }
        
        internal static GenericResponse<AssetUserRule> UpdateAssetUserRule(int groupId, long assetUserRuleId, AssetUserRule assetUserRuleToUpdate)
        {
            GenericResponse<AssetUserRule> response = new GenericResponse<AssetUserRule>();
            assetUserRuleToUpdate.Id = assetUserRuleId;
            assetUserRuleToUpdate.GroupId = groupId;

            try
            {
                // check if AssetUserRule exists in CB
                AssetUserRule oldAssetUserRule = ApiDAL.GetAssetUserRuleCB(assetUserRuleToUpdate.Id);
                
                if (oldAssetUserRule == null || oldAssetUserRule.Id == 0 || oldAssetUserRule.GroupId != groupId)
                {
                    response.Status.Code = (int)eResponseStatus.AssetUserRuleDoesNotExists;
                    response.Status.Message = ASSET_USER_RULE_DOES_NOT_EXIST;
                    return response;
                }

                // before saving AssetUserRule fill name,description,actions,conditions in case they are empty
                assetUserRuleToUpdate.FillEmpty(oldAssetUserRule);

                // update asset user rule in DB
                if (!ApiDAL.UpdateAssetRule(groupId, assetUserRuleToUpdate.Id, assetUserRuleToUpdate.Name, assetUserRuleToUpdate.Description))
                {
                    response.Status.Code = (int)eResponseStatus.Error;
                    response.Status.Message = ASSET_USER_RULE_FAILED_UPDATE;
                    return response;
                }
                
                // update asset user rule in CB           
                if (!ApiDAL.SaveAssetUserRuleCB(assetUserRuleToUpdate))
                {
                    log.ErrorFormat("Error while saving AssetUserRule. groupId: {0}, assetUserRuleId:{1}", groupId, assetUserRuleToUpdate.Id);
                }
                else
                {
                    // set invalidation keys
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAssetUserRuleInvalidationKey(assetUserRuleToUpdate.Id));

                    response.Object = assetUserRuleToUpdate;
                    response.Status.Code = (int)eResponseStatus.OK;
                    response.Status.Message = eResponseStatus.OK.ToString();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateAssetUserRule failed ex={0}, groupId={1}, AssetUserRuleId={2}", ex, groupId, assetUserRuleToUpdate.Id);
            }

            return response;
        }
        
        internal static Status DeleteAssetUserRule(int groupId, long assetUserRuleId)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            
            try
            {
                // check if AssetUserRule exists in CB
                AssetUserRule assetUserRule = ApiDAL.GetAssetUserRuleCB(assetUserRuleId);
                if (assetUserRule == null || assetUserRule.Id == 0 || assetUserRule.GroupId != groupId)
                {
                    response.Code = (int)eResponseStatus.AssetUserRuleDoesNotExists;
                    response.Message = ASSET_USER_RULE_DOES_NOT_EXIST;
                    return response;
                }
                
                // delete asset user rule in DB
                DataSet ds = ApiDAL.DeleteAssetUserRule(groupId, assetUserRuleId);
                
                if (ds == null || 
                    ds.Tables == null || 
                    ds.Tables.Count != 2 ||
                    ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "result") == 0)
                {
                    response.Code = (int)eResponseStatus.Error;
                    response.Message = DELETE_ASSET_USER_RULE_FAILED;
                    return response;
                }
               
                // delete assetUserRule from CB
                if (!ApiDAL.DeleteAssetUserRuleCB(assetUserRuleId))
                {
                    log.ErrorFormat("Error while delete AssetUserRules CB. groupId: {0}, assetUserRuleId:{1}", groupId, assetUserRuleId);
                }
                else
                {
                    // set invalidation keys
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAssetUserRuleIdsGroupInvalidationKey(groupId));
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAssetUserRuleInvalidationKey(assetUserRuleId));

                    DataTable dtDeletedUserToAssetUserRuleIds = ds.Tables[1];
                    if (dtDeletedUserToAssetUserRuleIds != null && dtDeletedUserToAssetUserRuleIds.Rows != null)
                    {
                        foreach (DataRow deletedUserToAssetUserRow in dtDeletedUserToAssetUserRuleIds.Rows)
                        {
                            long deletedUserId = ODBCWrapper.Utils.GetLongSafeVal(deletedUserToAssetUserRow, "USER_ID");
                            LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetUserToAssetUserRuleIdsInvalidationKey(deletedUserId));
                        }
                    }
                    
                    response.Code = (int)eResponseStatus.OK;
                    response.Message = eResponseStatus.OK.ToString();   
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteAssetUserRule failed ex={0}, groupId={1}, AssetUserRuleId={2}", ex, groupId, assetUserRuleId);
            }

            return response;
        }
        
        internal static Status AddAssetUserRuleToUser(long userId, long ruleId, int groupId)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                // check if AssetUserRule exists in CB
                AssetUserRule assetUserRule = ApiDAL.GetAssetUserRuleCB(ruleId);
                if (assetUserRule == null || assetUserRule.Id == 0 || assetUserRule.GroupId != groupId)
                {
                    response.Code = (int)eResponseStatus.AssetUserRuleDoesNotExists;
                    response.Message = ASSET_USER_RULE_DOES_NOT_EXIST;
                    return response;
                }

                // check user is not attached to this rule
                List<long> assetUserRuleIds = null;
                
                if (!TryGetUserAssetUserRuleIds(userId, groupId, out assetUserRuleIds))
                {
                    return response;
                }

                if (assetUserRuleIds != null && assetUserRuleIds.Contains(ruleId))
                {
                    response.Code = (int)eResponseStatus.UserAlreadyAttachedToAssetUserRule;
                    response.Message = "User already attached to this AssetUserRule";
                    return response;
                }

                // add user to asset user rule in DB
                DataTable dt = ApiDAL.AddAssetUserRuleToUser(userId, ruleId);

                if (dt != null && 
                    dt.Rows != null && 
                    dt.Rows.Count > 0 &&
                    ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "USER_ID") > 0)
                {
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetUserToAssetUserRuleIdsInvalidationKey(userId));

                    response.Code = (int)eResponseStatus.OK;
                    response.Message = eResponseStatus.OK.ToString();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("AddAssetUserRuleToUser failed ex={0}, groupId={1}, ruleId={2}, userId={3}", ex, groupId, ruleId, userId);
            }

            return response;
        }

        internal static Status DeleteAssetUserRuleFromUser(long userId, long ruleId, int groupId)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                // check if AssetUserRule exists in CB
                AssetUserRule assetUserRule = ApiDAL.GetAssetUserRuleCB(ruleId);
                if (assetUserRule == null || assetUserRule.Id == 0 || assetUserRule.GroupId != groupId)
                {
                    response.Code = (int)eResponseStatus.AssetUserRuleDoesNotExists;
                    response.Message = ASSET_USER_RULE_DOES_NOT_EXIST;
                    return response;
                }

                // check user is attached to this rule
                List<long> assetUserRuleIds = null;

                if (!TryGetUserAssetUserRuleIds(userId, groupId, out assetUserRuleIds))
                {
                    return response;
                }

                if (assetUserRuleIds == null || !assetUserRuleIds.Contains(ruleId))
                {   
                    response.Code = (int)eResponseStatus.AssetUserRuleDoesNotExists;
                    response.Message = "User is not attached to this AssetUserRule";
                    return response;
                }

                // delete asset user rule from user in DB
                if (!ApiDAL.DeleteAssetUserRuleFromUser(userId, ruleId))
                {
                    response.Code = (int)eResponseStatus.Error;
                    response.Message = DELETE_USER_ASSET_USER_RULE_FAILED;
                    return response;
                }

                // set invalidation keys
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetUserToAssetUserRuleIdsInvalidationKey(userId));

                response.Code = (int)eResponseStatus.OK;
                response.Message = eResponseStatus.OK.ToString();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteAssetUserRuleFromUser failed ex={0}, groupId={1}, ruleId={2}, userId={3}", ex, groupId, ruleId, userId);
            }

            return response;
        }

        private static Tuple<Dictionary<string, AssetUserRule>, bool> GetAssetUserRulesCB(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, AssetUserRule> result = new Dictionary<string, AssetUserRule>();

            try
            {
                if (funcParams != null && funcParams.ContainsKey("ruleIds"))
                {
                    List<string> ruleIds = funcParams["ruleIds"] != null ? funcParams["ruleIds"] as List<string> : null;

                    if (ruleIds != null && ruleIds.Count > 0)
                    {
                        foreach (string sRuleId in ruleIds)
                        {
                            long ruleId = long.Parse(sRuleId);

                            AssetUserRule assetUserRule = ApiDAL.GetAssetUserRuleCB(ruleId);
                            if (assetUserRule != null)
                            {
                                string assetUserRuleKey = LayeredCacheKeys.GetAssetUserRuleKey(assetUserRule.Id);
                                result.Add(assetUserRuleKey, assetUserRule);
                            }
                        }

                        res = result.Count == ruleIds.Count();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetAssetUserRulesCB failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, AssetUserRule>, bool>(result, res);
        }

        /// <summary>
        /// Get All Asset User Rule Ids by groupID from DB
        /// </summary>
        /// <param name="funcParams">groupId</param>
        /// <returns></returns>
        private static Tuple<List<long>, bool> GetAssetUserRuleIdsByGroupDB(Dictionary<string, object> funcParams)
        {
            List<long> assetUserRuleIds = null;

            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId") && funcParams.Count == 1)
                {
                    int? groupId = funcParams["groupId"] as int?;

                    if (groupId.HasValue)
                    {
                        DataTable dtAssetUserRules = ApiDAL.GetAssetUserRules(groupId.Value);
                        assetUserRuleIds = new List<long>();

                        if (dtAssetUserRules != null && dtAssetUserRules.Rows != null && dtAssetUserRules.Rows.Count > 0)
                        {
                            foreach (DataRow assetUserRuleRow in dtAssetUserRules.Rows)
                            {
                                long assetUserRuleId = ODBCWrapper.Utils.GetLongSafeVal(assetUserRuleRow, "ID");
                                assetUserRuleIds.Add(assetUserRuleId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetAssetUserRuleIdsByGroupDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<long>, bool>(assetUserRuleIds, assetUserRuleIds != null);
        }

        private static Tuple<List<long>, bool> GetUserToAssetUserRuleIdsDB(Dictionary<string, object> funcParams)
        {
            List<long> assetUserRuleIds = null;

            try
            {
                if (funcParams != null && funcParams.ContainsKey("userId") && funcParams.Count == 1)
                {
                    long? userId = funcParams["userId"] as long?;

                    if (userId.HasValue)
                    {
                        DataTable dtUserToAssetUserRules = ApiDAL.GetUserToAssetUserRules(userId.Value);
                        assetUserRuleIds = new List<long>();

                        if (dtUserToAssetUserRules != null && dtUserToAssetUserRules.Rows != null && dtUserToAssetUserRules.Rows.Count > 0)
                        {
                            foreach (DataRow userToAssetUserRuleRow in dtUserToAssetUserRules.Rows)
                            {
                                long assetUserRuleId = ODBCWrapper.Utils.GetLongSafeVal(userToAssetUserRuleRow, "RULE_ID");
                                assetUserRuleIds.Add(assetUserRuleId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetUserToAssetUserRuleIdsDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<long>, bool>(assetUserRuleIds, assetUserRuleIds != null);
        }

        private static bool TryGetUserAssetUserRuleIds(long userId, int groupId, out List<long> assetUserRuleIds)
        {
            assetUserRuleIds = new List<long>();

            string key = LayeredCacheKeys.GetUserToAssetUserRuleIdsKey(userId);

            if (!LayeredCache.Instance.Get<List<long>>(key,
                                                       ref assetUserRuleIds,
                                                       GetUserToAssetUserRuleIdsDB,
                                                       new Dictionary<string, object>() { { "userId", userId } },
                                                       groupId,
                                                       LayeredCacheConfigNames.GET_USER_TO_ASSET_USER_RULE_IDS,
                                                       new List<string>() { LayeredCacheKeys.GetUserToAssetUserRuleIdsInvalidationKey(userId) }))
            {
                log.ErrorFormat("TryGetUserAssetUserRuleIds - Failed get data from cache. groupId: {0}, userId: {1}", groupId, userId);
                return false;
            }

            return true;
        }
    }
}
