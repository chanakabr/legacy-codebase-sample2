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
        private const string ASSET_USER_RULE_FAILED_DELETE = "failed to delete Asset user rule";
        private const string ASSET_USER_RULE_FAILED_UPDATE = "failed to update Asset user rule";
        private const string ASSET_USER_RULE_NOT_FOUND = "No Asset User Rules found";
        
        internal static GenericListResponse<AssetUserRule> GetAssetUserRuleList(int groupId)
        {
            GenericListResponse<AssetUserRule> response = new GenericListResponse<AssetUserRule>();

            try
            {
                List<long> assetUserRuleIds = new List<long>();
                string key = LayeredCacheKeys.GetAllAssetUserRuleIdsKey(groupId);

                if (!LayeredCache.Instance.Get<List<long>>(key,
                                                           ref assetUserRuleIds,
                                                           GetAllAssetUserRuleIds,
                                                           new Dictionary<string, object>() { { "groupId", groupId } },
                                                           groupId,
                                                           LayeredCacheConfigNames.GET_ALL_ASSET_USER_RULE_IDS,
                                                           new List<string>() { LayeredCacheKeys.GetAllAssetUserRuleIdsInvalidationKey(groupId) }))
                {
                    log.ErrorFormat("GetAssetUserRuleList - Failed get data from cache. groupId: {0}", groupId);
                    return response;
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
                                                                   GetAssetUserRules, 
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
                response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed GetAssetUserRuleList groupID: {0}, ex: {1}", groupId, ex);
            }

            return response;
        }

        private static Tuple<Dictionary<string, AssetUserRule>, bool> GetAssetUserRules(Dictionary<string, object> funcParams)
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
                        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
                        
                        foreach (string sRuleId in ruleIds)
                        {
                            long ruleId = long.Parse(sRuleId);
                            string assetUserRuleKey = LayeredCacheKeys.GetAssetUserRuleKey(ruleId);

                            AssetUserRule assetUserRule = ApiDAL.GetAssetUserRuleCB(ruleId, assetUserRuleKey);
                            if (assetUserRule != null)
                            {
                                result.Add(assetUserRuleKey, assetUserRule);
                            }
                        }

                        res = result.Count == ruleIds.Count();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetAssetUserRules failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, AssetUserRule>, bool>(result, res);
        }

        /// <summary>
        /// Get All Asset User Rule Ids by groupID from DB
        /// </summary>
        /// <param name="funcParams">groupId</param>
        /// <returns></returns>
        private static Tuple<List<long>, bool> GetAllAssetUserRuleIds(Dictionary<string, object> funcParams)
        {
            List<long> assetUserRuleIds = new List<long>();

            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;

                    if (groupId.HasValue)
                    {
                        DataTable dtAssetUserRules = ApiDAL.GetAssetUserRules(groupId.Value);

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
                log.Error(string.Format("GetAllAssetUserRuleIds failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<long>, bool>(assetUserRuleIds, assetUserRuleIds.Count() > 0);
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
                    if (!ApiDAL.SaveAssetUserRuleCB(assetUserRuleToAdd, LayeredCacheKeys.GetAssetUserRuleKey(assetUserRuleToAdd.Id)))
                    {
                        log.ErrorFormat("Error while saving AssetUserRule. groupId: {0}, assetUserRuleId:{1}", groupId, assetUserRuleToAdd.Id);
                    }
                    else
                    {
                        LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAllAssetUserRuleIdsInvalidationKey(groupId));
                        
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
            string assetUserRuleKey = LayeredCacheKeys.GetAssetUserRuleKey(assetUserRuleToUpdate.Id);


            try
            {
                // check if AssetUserRule exists in CB
                AssetUserRule oldAssetUserRule = ApiDAL.GetAssetUserRuleCB(assetUserRuleToUpdate.Id, assetUserRuleKey);
                
                if (oldAssetUserRule == null || oldAssetUserRule.Id == 0 || oldAssetUserRule.GroupId != groupId)
                {
                    response.Status.Code = (int)eResponseStatus.AssetUserRuleDoesNotExists;
                    response.Status.Message = ASSET_USER_RULE_DOES_NOT_EXIST;
                    return response;
                }

                // update asset user rule in DB
                if (!ApiDAL.UpdateAssetRule(groupId, assetUserRuleToUpdate.Id, assetUserRuleToUpdate.Name, assetUserRuleToUpdate.Description))
                {
                    response.Status.Code = (int)eResponseStatus.Error;
                    response.Status.Message = ASSET_USER_RULE_FAILED_UPDATE;
                    return response;
                }

                // before saving AssetRule complete name,actions,conditions in case they are empty
                assetUserRuleToUpdate.FillEmpty(oldAssetUserRule);

                // update asset user rule in CB           
                if (!ApiDAL.SaveAssetUserRuleCB(assetUserRuleToUpdate, assetUserRuleKey))
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
                string assetUserRuleKey = LayeredCacheKeys.GetAssetUserRuleKey(assetUserRuleId);
                AssetUserRule assetUserRule = ApiDAL.GetAssetUserRuleCB(assetUserRuleId, assetUserRuleKey);
                if (assetUserRule == null || assetUserRule.Id == 0 || assetUserRule.GroupId != groupId)
                {
                    response.Code = (int)eResponseStatus.AssetUserRuleDoesNotExists;
                    response.Message = ASSET_USER_RULE_DOES_NOT_EXIST;
                    return response;
                }

                // delete asset user rule in DB
                if (!ApiDAL.DeleteAssetRule(groupId, assetUserRuleId))
                {
                    response.Code = (int)eResponseStatus.Error;
                    response.Message = ASSET_USER_RULE_FAILED_DELETE;
                    return response;
                }

                // delete assetUserRule from CB
                if (!ApiDAL.DeleteAssetUserRuleCB(assetUserRuleKey))
                {
                    log.ErrorFormat("Error while delete AssetUserRules CB. groupId: {0}, assetUserRuleId:{1}", groupId, assetUserRuleId);
                }
                else
                {
                    // set invalidation keys
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAllAssetUserRuleIdsInvalidationKey(groupId));
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAssetUserRuleInvalidationKey(assetUserRuleId));
                    
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
    }
}
