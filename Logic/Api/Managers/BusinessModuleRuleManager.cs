using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Rules;
using CachingProvider.LayeredCache;
using Core.Api;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;

namespace APILogic.Api.Managers
{
    public class BusinessModuleRuleManager
    {
        private const string BUSINESS_MODULE_RULE_NOT_EXIST = "Business module rule doesn't exist";
        private const string BUSINESS_MODULE_RULE_FAILED_DELETE = "failed to delete business module rule";
        private const string BUSINESS_MODULE_RULE_FAILED_UPDATE = "failed to update business module rule";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        internal static Status DeleteBusinessModuleRule(int groupId, long businessModuleRuleId)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                //check if rule exists
                BusinessModuleRule businessModuleRule = ApiDAL.GetBusinessModuleRuleCB(businessModuleRuleId);
                if (businessModuleRule == null || businessModuleRule.Id == 0 || groupId != businessModuleRule.GroupId)
                {
                    response.Set((int)eResponseStatus.RuleNotExists, BUSINESS_MODULE_RULE_NOT_EXIST);
                    return response;
                }

                if (!ApiDAL.DeleteBusinessModuleRule(groupId, businessModuleRuleId))
                {
                    response.Set((int)eResponseStatus.Error, BUSINESS_MODULE_RULE_FAILED_DELETE);
                    return response;
                }

                // delete rule from CB
                if (!ApiDAL.DeleteBusinessModuleRuleCB(groupId, businessModuleRuleId))
                {
                    log.ErrorFormat("Error while deleting BusinessModuleRule from CB. groupId: {0}, businessModuleRuleId:{1}", groupId, businessModuleRuleId);
                }

                SetInvalidationKeys(groupId, businessModuleRuleId);
                response.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteBusinessModuleRule failed ex={0}, groupId={1}, businessModuleRuleId={2}", ex, groupId, businessModuleRuleId);
            }

            return response;
        }

        internal static GenericResponse<BusinessModuleRule> UpdateBusinessModuleRule(int groupId, BusinessModuleRule businessModuleRule)
        {
            GenericResponse<BusinessModuleRule> response = new GenericResponse<BusinessModuleRule>();
            try
            {
                businessModuleRule.GroupId = groupId;
                businessModuleRule.UpdateDate = DateUtils.GetUtcUnixTimestampNow();

                if (!ApiDAL.UpdateBusinessModuleRule(groupId, businessModuleRule.Id, businessModuleRule.Name, businessModuleRule.Description,
                    DateUtils.UtcUnixTimestampSecondsToDateTime(businessModuleRule.UpdateDate))) 
                {
                    response.SetStatus(eResponseStatus.Error, BUSINESS_MODULE_RULE_FAILED_UPDATE);
                    return response;
                }

                if (!ApiDAL.SaveBusinessModuleRuleCB(groupId, businessModuleRule))
                {
                    log.ErrorFormat("Error while saving BusinessModuleRule. groupId: {0}, businessModuleRuleId:{1}", groupId, businessModuleRule.Id);
                }
                else
                {
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetBusinessModuleRuleInvalidationKey(businessModuleRule.Id));
                    response.Object = businessModuleRule;
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateBusinessModuleRule failed ex={0}, groupId={1}, businessModuleRuleId={2}", ex, groupId, businessModuleRule.Id);
            }

            return response;
        }

        internal static GenericResponse<BusinessModuleRule> GetBusinessModuleRule(int groupId, long businessModuleRuleId)
        {
            GenericResponse<BusinessModuleRule> response = new GenericResponse<BusinessModuleRule>();
            try
            {
                BusinessModuleRule businessModuleRule = ApiDAL.GetBusinessModuleRuleCB(businessModuleRuleId);

                if (businessModuleRule == null || businessModuleRule.Id == 0 || groupId != businessModuleRule.GroupId)
                {
                    response.SetStatus(eResponseStatus.RuleNotExists, BUSINESS_MODULE_RULE_NOT_EXIST);
                    return response;
                }

                response.Object = businessModuleRule;
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetBusinessModuleRule failed ex={0}, groupId={1}, businessModuleRuleId={2}", ex, groupId, businessModuleRuleId);
            }

            return response;
        }

        internal static GenericResponse<BusinessModuleRule> AddBusinessModuleRule(int groupId, BusinessModuleRule businessModuleRuleToAdd)
        {
            GenericResponse<BusinessModuleRule> response = new GenericResponse<BusinessModuleRule>();
            try
            {
                businessModuleRuleToAdd.GroupId = groupId;
                DataTable dt = ApiDAL.AddBusinessModuleRule(groupId, businessModuleRuleToAdd.Name, businessModuleRuleToAdd.Description);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    businessModuleRuleToAdd.Id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID");
                    businessModuleRuleToAdd.CreateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0], "CREATE_DATE"));
                    businessModuleRuleToAdd.UpdateDate = businessModuleRuleToAdd.CreateDate;

                    if (!ApiDAL.SaveBusinessModuleRuleCB(groupId, businessModuleRuleToAdd))
                    {
                        log.ErrorFormat("Error while saving BusinessModuleRule. groupId: {0}, BusinessModuleRuleId:{1}", groupId, businessModuleRuleToAdd.Id);
                        return response;
                    }

                    SetInvalidationKeys(groupId);
                    response.Object = businessModuleRuleToAdd;
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while adding new businessModuleRule . groupId: {0}, businessModuleRule: {1}, ex: {2}", groupId, JsonConvert.SerializeObject(businessModuleRuleToAdd), ex);
            }

            return response;
        }

        internal static GenericListResponse<BusinessModuleRule> GetBusinessModuleRules(int groupId, ConditionScope filter, int pageIndex = 0, int pageSize = 0)
        {
            GenericListResponse<BusinessModuleRule> response = new GenericListResponse<BusinessModuleRule>();

            try
            {
                List<BusinessModuleRule> allBusinessModuleRules = GetAllBusinessModuleRules(groupId);

                List<BusinessModuleRule> filteredRules = new List<BusinessModuleRule>();

                if (allBusinessModuleRules != null && allBusinessModuleRules.Count > 0)
                {
                    log.DebugFormat("GetBusinessModuleRules filter properties - BusinessModuleId: {0}, BusinessModuleType: {1}, FilterByDate: {2}, FilterBySegments: {3}, SegmentIds: {4}",
                                    filter.BusinessModuleId,
                                    filter.BusinessModuleType.HasValue ? filter.BusinessModuleType.Value.ToString() : "null",
                                    filter.FilterByDate,
                                    filter.FilterBySegments,
                                    filter.SegmentIds != null ? string.Join(", ", filter.SegmentIds) : "null");

                    foreach (var rule in allBusinessModuleRules)
                    {
                        if (rule.Evaluate(filter))
                        {
                            filteredRules.Add(rule);
                        }
                    }
                }
                response.Objects = pageSize > 0 ? filteredRules.Skip(pageIndex * pageSize).Take(pageSize).ToList() : filteredRules;

                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                response.SetStatus(eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed GetBusinessModuleRules groupID: {0}, ex: {1}", groupId, ex);
            }

            return response;
        }

        public static List<BusinessModuleRule> GetAllBusinessModuleRules(int groupId)
        {
            List<BusinessModuleRule> allBusinessModuleRules = null;

            string allBusinessModuleRuleIdsKey = LayeredCacheKeys.GetAllBusinessModuleRuleIdsKey(groupId);
            List<long> ruleIds = null;
            if (!LayeredCache.Instance.Get<List<long>>(allBusinessModuleRuleIdsKey,
                                                                        ref ruleIds,
                                                                        GetAllBusinessModuleRulesDB,
                                                                        new Dictionary<string, object>() { { "groupId", groupId } },
                                                                        groupId,
                                                                        LayeredCacheConfigNames.GET_ALL_BUSINESS_MODULE_RULE_IDS,
                                                                        new List<string>() { LayeredCacheKeys.GetAllBusinessModuleRulesGroupInvalidationKey(groupId) }))
            {
                allBusinessModuleRules = null;
                log.ErrorFormat("GetAllBusinessModuleRules - GetAllBusinessModuleRulesDB - Failed get data from cache. groupId: {0}", groupId);
            }

            if (ruleIds.Count > 0)
            {
                Dictionary<string, string> keysToOriginalValueMap = new Dictionary<string, string>();
                Dictionary<string, List<string>> invalidationKeysMap = new Dictionary<string, List<string>>();

                foreach (long ruleId in ruleIds)
                {
                    string businessModuleRuleKey = LayeredCacheKeys.GetBusinessModuleRuleKey(ruleId);
                    keysToOriginalValueMap.Add(businessModuleRuleKey, ruleId.ToString());
                    invalidationKeysMap.Add(businessModuleRuleKey, new List<string>() { LayeredCacheKeys.GetBusinessModuleRuleInvalidationKey(ruleId) });
                }

                Dictionary<string, BusinessModuleRule> fullBusinessModuleRules = null;

                if (LayeredCache.Instance.GetValues<BusinessModuleRule>(keysToOriginalValueMap,
                                                                   ref fullBusinessModuleRules,
                                                                   GetBusinessModuleRulesCB,
                                                                   new Dictionary<string, object>() { { "ruleIds", keysToOriginalValueMap.Values.ToList() } },
                                                                   groupId,
                                                                   LayeredCacheConfigNames.GET_BUSINESS_MODULE_RULE,
                                                                   invalidationKeysMap))
                {
                    if (fullBusinessModuleRules != null && fullBusinessModuleRules.Count > 0)
                    {
                        allBusinessModuleRules = fullBusinessModuleRules.Values.ToList();
                    }
                }
            }

            return allBusinessModuleRules;
        }

        private static Tuple<Dictionary<string, BusinessModuleRule>, bool> GetBusinessModuleRulesCB(Dictionary<string, object> funcParams)
        {
            Dictionary<string, BusinessModuleRule> result = new Dictionary<string, BusinessModuleRule>();

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

                            BusinessModuleRule businessModuleRule = ApiDAL.GetBusinessModuleRuleCB(ruleId);
                            if (businessModuleRule != null)
                            {
                                string businessModuleRuleKey = LayeredCacheKeys.GetBusinessModuleRuleKey(businessModuleRule.Id);
                                result.Add(businessModuleRuleKey, businessModuleRule);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetBusinessModuleRulesCB failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, BusinessModuleRule>, bool>(result, true);
        }
        

        private static Tuple<List<long>, bool> GetAllBusinessModuleRulesDB(Dictionary<string, object> funcParams)
        {
            List<long> businessModuleRuleIds = new List<long>();

            if (funcParams != null && funcParams.Count == 1)
            {
                if (funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;

                    if (groupId.HasValue)
                    {
                        try
                        {
                            DataTable dtBusinessModuleRules = ApiDAL.GetBusinessModuleRulesDB(groupId.Value);

                            if (dtBusinessModuleRules != null && dtBusinessModuleRules.Rows != null && dtBusinessModuleRules.Rows.Count > 0)
                            {
                                log.Debug("GetAllBusinessModuleRulesDB - success");
                                foreach (DataRow businessModuleRuleRow in dtBusinessModuleRules.Rows)
                                {
                                    long id = ODBCWrapper.Utils.GetLongSafeVal(businessModuleRuleRow, "ID");
                                    businessModuleRuleIds.Add(id);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error(string.Format("GetAllBusinessModuleRulesDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
                        }
                    }
                }
            }

            return new Tuple<List<long>, bool>(businessModuleRuleIds, true);
        }

        private static void SetInvalidationKeys(int groupId, long? ruleId = null)
        {
            LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAllBusinessModuleRulesGroupInvalidationKey(groupId));

            if (ruleId.HasValue)
            {
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetBusinessModuleRuleInvalidationKey(ruleId.Value));
            }
        }
    }

    public class ConditionScope : IConditionScope, IBusinessModuleConditionScope, ISegmentsConditionScope, IDateConditionScope, IHeaderConditionScope, IIpRangeConditionScope, IAssetConditionScope
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public long BusinessModuleId { get; set; }

        public eTransactionType? BusinessModuleType { get; set; }

        public bool FilterByDate { get; set; }

        public bool FilterBySegments { get; set; }
       
        public List<long> SegmentIds { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public long Ip { get; set; }

        public long MediaId { get; set; }

        public List<BusinessModuleRule> GetBusinessModuleRulesByMediaId(int groupId, long mediaId)
        {
            List<BusinessModuleRule> allBusinessModuleRules = BusinessModuleRuleManager.GetAllBusinessModuleRules(groupId);
            List<BusinessModuleRule> businessRulesByMedia = new List<BusinessModuleRule>();

            string businessModuleRulesByMediaKey = LayeredCacheKeys.GetBusinessModuleRulesRulesByMediaKey(mediaId);
            string mediaInvalidationKey = LayeredCacheKeys.GetMediaInvalidationKey(groupId, mediaId);
            string allAssetRulesInvalidationKey = LayeredCacheKeys.GetAllBusinessModuleRulesGroupInvalidationKey(groupId);

            if (!LayeredCache.Instance.Get<List<BusinessModuleRule>>(businessModuleRulesByMediaKey,
                                                            ref businessRulesByMedia,
                                                            GetBusinessModuleRulesByMedia,
                                                            new Dictionary<string, object>()
                                                            {
                                                                        { "allBusinessModuleRules", allBusinessModuleRules },
                                                                        { "mediaId", mediaId },
                                                                        { "groupId", groupId }
                                                            },
                                                            groupId,
                                                            LayeredCacheConfigNames.GET_ASSET_RULES_BY_ASSET,
                                                            new List<string>()
                                                            {
                                                                        allAssetRulesInvalidationKey,
                                                                        mediaInvalidationKey
                                                            }))
            {
                log.ErrorFormat("GetBusinessModuleRulesByMediaId - Failed get data from cache. groupId: {0}", groupId);
            }

            return businessRulesByMedia;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (BusinessModuleId > 0)
            {
                sb.AppendFormat("BusinessModuleId: {0}; ", BusinessModuleId);
            }

            if (BusinessModuleType.HasValue)
            {
                sb.AppendFormat("BusinessModuleType: {0}; ", BusinessModuleType.Value);
            }

            if (FilterByDate)
            {
                sb.AppendFormat("FilterByDate: {0}; ", FilterByDate);
            }

            if (FilterBySegments)
            {
                sb.AppendFormat("FilterBySegments: {0}; ", FilterBySegments);
            }
            
            if (SegmentIds != null && SegmentIds.Count > 0)
            {
                sb.AppendFormat("SegmentIds: {0}; ", string.Join(",", SegmentIds));
            }

            if (Headers != null && Headers.Count > 0)
            {
                sb.AppendFormat("Headers: {0}; ", string.Join(",", Headers));
            }

            if (Ip > 0)
            {
                sb.AppendFormat("Ip: {0}; ", Ip);
            }
            
            return sb.ToString();
        }

        private static Tuple<List<BusinessModuleRule>, bool> GetBusinessModuleRulesByMedia(Dictionary<string, object> funcParams)
        {
            List<BusinessModuleRule> BusinessModuleRulesByAsset = new List<BusinessModuleRule>();

            try
            {
                if (funcParams != null && funcParams.Count == 3)
                {
                    if (funcParams.ContainsKey("allBusinessModuleRules") && funcParams.ContainsKey("mediaId") && funcParams.ContainsKey("groupId"))
                    {
                        int? groupId = funcParams["groupId"] as int?;
                        SlimAsset slimAsset = funcParams["mediaId"] as SlimAsset;
                        List<BusinessModuleRule> allBusinessModuleRules = funcParams["allBusinessModuleRules"] as List<BusinessModuleRule>;

                        if (slimAsset != null && allBusinessModuleRules != null && allBusinessModuleRules.Count > 0 && groupId.HasValue)
                        {
                            // Determine which AssetRules contains Conditions that they are typeof AssetCondition (have Ksql).
                            var assetRulesWithKsql = allBusinessModuleRules.Where(x => x.Conditions.Any(y => y is AssetCondition));

                            Parallel.ForEach(assetRulesWithKsql, (currRuleWithKsql) =>
                            {
                                StringBuilder ksqlFilter = new StringBuilder();

                                ksqlFilter.AppendFormat(string.Format("(and asset_type='media' media_id = '{0}' (or", slimAsset.Id));

                                foreach (var condition in currRuleWithKsql.Conditions)
                                {
                                    if (condition is AssetCondition)
                                    {
                                        AssetCondition assetCondition = condition as AssetCondition;
                                        ksqlFilter.AppendFormat(" {0}", assetCondition.Ksql);
                                    }
                                }

                                ksqlFilter.Append("))");

                                var assets = api.SearchAssets(groupId.Value, ksqlFilter.ToString(), 0, 0, true, 0, true, string.Empty, string.Empty, string.Empty, 0, 0, true);

                                // If there is a match, add rule to list
                                if (assets != null && assets.Count() > 0)
                                {
                                    BusinessModuleRulesByAsset.Add(currRuleWithKsql);
                                }
                            });

                            log.Debug("GetAssetRulesByAsset - success");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BusinessModuleRulesByAsset = null;
                log.Error(string.Format("GetAssetRulesByAsset faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<BusinessModuleRule>, bool>(BusinessModuleRulesByAsset, BusinessModuleRulesByAsset != null);
        }

    }
}
