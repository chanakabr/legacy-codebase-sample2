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
    public class AssetRuleManager
    {
        private const string ASSET_RULE_NOT_EXIST = "Asset rule doesn't exist";
        private const string ASSET_RULE_FAILED_DELETE = "failed to delete Asset rule";
        private const string ASSET_RULE_FAILED_UPDATE = "failed to update Asset rule";
        private const string NO_ASSET_RULES_FOUND = "No Asset Rules found";

        private const int MAX_ASSETS_TO_UPDATE = 1000;

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static int DoActionRules()
        {
            int groupId = 0;
            List<long> rulesIds = null;
            List<int> result = new List<int>();

            try
            {
                // Get all rules of this group
                Dictionary<int, List<AssetRule>> allRules = GetRules(groupId, rulesIds);
                foreach (KeyValuePair<int, List<AssetRule>> pair in allRules)
                {
                    groupId = pair.Key;
                    Group group = new GroupManager().GetGroup(groupId);
                    if (group.isGeoAvailabilityWindowingEnabled)
                    {
                        List<AssetRule> rules = pair.Value;
                        Task<List<int>>[] tasks = new Task<List<int>>[rules.Count];

                        log.DebugFormat("Starting to do action on {0} asset rules for groupId = {1}", rules.Count, groupId);

                        ContextData contextData = new ContextData();

                        for (int ruleIndex = 0; ruleIndex < rules.Count; ruleIndex++)
                        {
                            tasks[ruleIndex] = new Task<List<int>>((obj) =>
                            {
                                contextData.Load();
                                return DoActionOnRule(rules[(int)obj], groupId, group);
                            }, ruleIndex);

                            tasks[ruleIndex].Start();
                        }

                        #region Finish tasks

                        Task.WaitAll(tasks);

                        foreach (var task in tasks)
                        {
                            if (task != null)
                            {
                                result.AddRange(task.Result);
                                task.Dispose();
                            }
                        }

                        result = result.Distinct().ToList();
                        if (result.Count > 0)
                        {
                            log.DebugFormat("going to update index for {0} assets", result.Count);

                            if (Catalog.Module.UpdateIndex(result, groupId, eAction.Update))
                            {
                                log.InfoFormat("Successfully updated index after asset rule for assets: {0}", string.Join(",", result));
                                return result.Count;
                            }
                            else
                            {
                                log.InfoFormat("Failed to update index after asset rule for assets", string.Join(",", result));
                                return 0;
                            }
                        }

                        List<long> ranRules = allRules.Values.SelectMany(ar => ar).Select(ar => ar.Id).ToList();
                        if (!ApiDAL.UpdateAssetRulesLastRunDate(groupId, ranRules))
                        {
                            log.ErrorFormat("Failed to update asset rule last run date, rule IDs = {0}", string.Join(", ", ranRules));
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in DoActionRules", ex);
            }

            return result.Count;
        }

        private static List<int> DoActionOnRule(AssetRule rule, int groupId, Group group)
        {
            List<int> result = new List<int>();

            log.DebugFormat("Starting to do action on ruleId = {0} ", rule.Id);

            try
            {
                List<int> modifiedAssetIds = new List<int>();
                
                // separate the country conditions and the ksql, 
                List<CountryCondition> countryConditions = rule.Conditions.Where(c => c.Type == AssetRuleConditionType.Country).Select(c => (CountryCondition)c).ToList();
                List<AssetCondition> assetConditions = rule.Conditions.Where(c => c.Type == AssetRuleConditionType.Asset).Select(c => (AssetCondition)c).ToList();

                string ksqlFilter = null;

                // concatenate the ksql with 'and'
                if (assetConditions != null && assetConditions.Count > 0)
                {
                    StringBuilder ksql = new StringBuilder("(and");
                    foreach (var assetCondition in assetConditions)
                    {
                        ksql.Append(" " + assetCondition.Ksql);
                    }
                    ksql.AppendFormat(")");

                    ksqlFilter = ksql.ToString();
                }

                // concatenate the countries lists with NOT and without not
                List<int> countries = new List<int>();

                if (countryConditions != null && countryConditions.Count > 0)
                {
                    foreach (var countryCondition in countryConditions)
                    {
                        if (countryCondition.Not)
                        {
                            countries.AddRange(GetAllCountriesBut(groupId, countryCondition.Countries));
                        }
                        else
                        {
                            countries.AddRange(countryCondition.Countries);
                        }
                    }

                    countries = countries.Distinct().ToList();
                }

                List<int> assetIds = new List<int>();

                foreach (int country in countries)
                {
                    foreach (var action in rule.Actions)
                    {
                        assetIds = new List<int>();
                        string actionKsqlFilter;

                        if (action.Type == RuleActionType.StartDateOffset)
                        {
                            // append the country and offset conditions
                            double totalOffset = CalcTotalOfssetForCountry(groupId, action, country);
                            actionKsqlFilter = string.Format("(and {0} start_date <= '{1}' allowed_countries != '{2}')", ksqlFilter, -1 * totalOffset, country);

                            UnifiedSearchResponse unifiedSearcjResponse = GetUnifiedSearchResponse(group, actionKsqlFilter);

                            if (unifiedSearcjResponse != null)
                            {
                                bool isSearchSuccessfull = unifiedSearcjResponse.status.Code == (int)eResponseStatus.OK;
                                if (isSearchSuccessfull && unifiedSearcjResponse.searchResults != null && unifiedSearcjResponse.searchResults.Count > 0)
                                {
                                    assetIds = unifiedSearcjResponse.searchResults.Select(asset => Convert.ToInt32(asset.AssetId)).ToList();

                                    // Apply rule on assets that returned from search
                                    if (ApiDAL.InsertMediaCountry(groupId, assetIds, country, true, rule.Id))
                                    {
                                        modifiedAssetIds.AddRange(assetIds);
                                        log.InfoFormat("Successfully added country: {0} to allowed countries for assrtRule: {1} on assets: {2}", country, rule.ToString(), string.Join(",", assetIds));
                                    }
                                    else
                                    {
                                        log.InfoFormat("Failed to add country: {0} to allowed countries for assrtRule: {1} on assets: {2}", country, rule.ToString(), string.Join(",", assetIds));
                                    }
                                }
                            }
                        }

                        if (action.Type == RuleActionType.EndDateOffset || action.Type == RuleActionType.Block)
                        {
                            if (action.Type == RuleActionType.EndDateOffset)
                            {
                                double totalOffset = CalcTotalOfssetForCountry(groupId, action, country);
                                actionKsqlFilter = string.Format("(and {0} end_date <= '{1}' blocked_countries != '{2}')", ksqlFilter, -1 * totalOffset, country);
                            }
                            else // block
                            {
                                actionKsqlFilter = string.Format("(and {0} blocked_countries != '{1}')", ksqlFilter, country);
                            }

                            UnifiedSearchResponse unifiedSearcjResponse = GetUnifiedSearchResponse(group, actionKsqlFilter);

                            if (unifiedSearcjResponse != null)
                            {
                                bool isSearchSuccessfull = unifiedSearcjResponse.status.Code == (int)eResponseStatus.OK;
                                if (isSearchSuccessfull && unifiedSearcjResponse.searchResults != null && unifiedSearcjResponse.searchResults.Count > 0)
                                {
                                    assetIds = unifiedSearcjResponse.searchResults.Select(asset => Convert.ToInt32(asset.AssetId)).ToList();

                                    // Apply rule on assets that returned from search
                                    if (ApiDAL.InsertMediaCountry(groupId, assetIds, country, false, rule.Id))
                                    {
                                        modifiedAssetIds.AddRange(assetIds);
                                        foreach (var assetId in assetIds)
                                        {
                                            string invalidationKey = LayeredCacheKeys.GetMediaCountriesInvalidationKey(assetId);
                                            if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                                            {
                                                log.ErrorFormat("Failed to set invalidation key on media countries key = {0}", invalidationKey);
                                            }
                                        }
                                        log.InfoFormat("Successfully added country: {0} to allowed countries for assrtRule: {1} on assets: {2}", country, rule.ToString(), string.Join(",", assetIds));
                                    }
                                    else
                                    {
                                        log.InfoFormat("Failed to add country: {0} to allowed countries for assrtRule: {1} on assets: {2}", country, rule.ToString(), string.Join(",", assetIds));
                                    }
                                }
                            }
                        }
                    }
                }
                
                return modifiedAssetIds;

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed doing actions of rule: groupId = {0}, ruleId = {1}, ex = {2}", groupId, rule.Id, ex);
                return result;
            }
        }

        private static IEnumerable<int> GetAllCountriesBut(int groupId, List<int> countryIds)
        {
            List<int> response = new List<int>();
            List<Country> countries = Core.Api.api.GetCountryListByIds(null, groupId);
            if (countries != null)
            {
                response = countries.Select(c => c.Id).Where(c => !countryIds.Contains(c)).ToList();
            }
            return response;
        }

        private static double CalcTotalOfssetForCountry(int groupId, AssetRuleAction action, int country)
        {
            TimeOffsetRuleAction offsetAction = (TimeOffsetRuleAction)action;
            double totalOffset = offsetAction.Offset;

            if (offsetAction.TimeZone)
            {
                totalOffset -= GetTimeZoneOffsetForCountry(groupId, country);
            }
            return totalOffset;
        }

        private static double GetTimeZoneOffsetForCountry(int groupId, int countryId)
        {
            List<Country> countries = Core.Api.api.GetCountryListByIds(new List<int>() { countryId }, groupId);

            if (countries == null || countries.Count == 0)
            {
                log.ErrorFormat("Failed to get countryId = {0}, groupId = {1}", countryId, groupId);
                return 0;
            }

            Country country = countries[0];

            if (string.IsNullOrEmpty(country.TimeZoneId))
            {
                return 0;
            }

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(country.TimeZoneId);
            if (tzi == null)
            {
                log.ErrorFormat("Failed to get time zone info by ID = {0} for country = {1}, groupId = {2}", country.TimeZoneId, countryId, groupId);
                return 0;
            }

            return tzi.GetUtcOffset(DateTime.UtcNow).TotalSeconds;
        }

        private static Dictionary<int, List<AssetRule>> GetRules(int groupId, List<long> rulesIds)
        {
            Dictionary<int, List<AssetRule>> rules = new Dictionary<int, List<AssetRule>>();

            GenericListResponse<AssetRule> ruleResponse = GetAssetRules(AssetRuleConditionType.Country);
            if (ruleResponse.Status.Code == (int)eResponseStatus.OK && ruleResponse.Objects != null)
            {
                if (groupId != 0)
                {
                    rules.Add(groupId, ruleResponse.Objects);
                }
                else
                {
                    foreach (var rule in ruleResponse.Objects)
                    {
                        if (!rules.ContainsKey(rule.GroupId))
                        {
                            rules.Add(rule.GroupId, new List<AssetRule>());
                        }

                        rules[rule.GroupId].Add(rule);
                    }
                }
            }

            return rules;
        }

        private static UnifiedSearchResponse GetUnifiedSearchResponse(Group group, string ksql)
        {
            // Initialize unified search request:
            // SignString/Signature (basic catalog parameters)
            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = ApplicationConfiguration.CatalogSignatureKey.Value;
            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

            // page size should be max_results so it will return everything
            int pageSize = MAX_ASSETS_TO_UPDATE; //ApplicationConfiguration.ElasticSearchConfiguration.MaxResults.IntValue;


            bool shouldIgnoreEndDate = ksql.ToLower().Contains("end_date");


            UnifiedSearchRequest unifiedSearchRequest = new UnifiedSearchRequest()
            {
                m_sSignature = sSignature,
                m_sSignString = sSignString,
                m_nGroupID = group.m_nParentGroupID,
                m_oFilter = new Core.Catalog.Filter()
                {
                    m_bOnlyActiveMedia = true,
                },
                m_nPageIndex = 0,
                m_nPageSize = pageSize,
                shouldIgnoreDeviceRuleID = true,
                shouldDateSearchesApplyToAllTypes = true,
                order = new ApiObjects.SearchObjects.OrderObj()
                {
                    m_eOrderBy = ApiObjects.SearchObjects.OrderBy.ID,
                    m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC
                },
                filterQuery = ksql,
                isInternalSearch = true,
                assetTypes = group.GetMediaTypes(),
                shouldIgnoreEndDate = shouldIgnoreEndDate
            };

            // Call catalog
            return unifiedSearchRequest.GetResponse(unifiedSearchRequest) as UnifiedSearchResponse;
        }

        /******************************/
        internal static GenericListResponse<AssetRule> GetAssetRules(AssetRuleConditionType assetRuleConditionType, int groupId = 0)
        {
            GenericListResponse<AssetRule> response = new GenericListResponse<AssetRule>();
            try
            {
                List<AssetRule> assetRules = new List<AssetRule>();
                string key = LayeredCacheKeys.GetAllAssetRulesKey();

                if (!LayeredCache.Instance.Get<List<AssetRule>>(key,
                                                                ref assetRules,
                                                                GetAllAssetRulesDB,
                                                                null,
                                                                groupId,
                                                                LayeredCacheConfigNames.GET_ALL_ASSET_RULES,
                                                                new List<string>() { LayeredCacheKeys.GetAllAssetRulesInvalidationKey() }))
                {
                    log.ErrorFormat("GetAssetRules - GetAllAssetRulesDB - Failed get data from cache. groupId: {0}", groupId);
                    return response;
                }

                if (assetRules == null || assetRules.Count == 0)
                {
                    response.SetStatus(eResponseStatus.OK, NO_ASSET_RULES_FOUND);
                    return response;
                }

                IEnumerable<AssetRule> filteredAssetRules = assetRules;

                if (groupId > 0)
                {
                    filteredAssetRules = assetRules.Where(x => x.GroupId == groupId);
                }

                if (filteredAssetRules == null)
                {
                    log.ErrorFormat("GetAssetRules - filteredAssetRules is empty. groupId: {0}", groupId);
                    return null;
                }

                Dictionary<string, string> keysToOriginalValueMap = new Dictionary<string, string>();
                Dictionary<string, List<string>> invalidationKeysMap = new Dictionary<string, List<string>>();

                foreach (AssetRule assetRule in filteredAssetRules)
                {
                    string assetRuleKey = LayeredCacheKeys.GetAssetRuleKey(assetRule.Id);

                    keysToOriginalValueMap.Add(assetRuleKey, assetRule.Id.ToString());
                    invalidationKeysMap.Add(assetRuleKey, new List<string>() { LayeredCacheKeys.GetAssetRuleInvalidationKey(assetRule.Id) });
                }

                Dictionary<string, AssetRule> fullAssetRules = null;

                // try to get full AssetRules from cache            
                if (LayeredCache.Instance.GetValues<AssetRule>(keysToOriginalValueMap,
                                                               ref fullAssetRules,
                                                               GetFullAssetRulesCB,
                                                               new Dictionary<string, object>() { { "ruleIds", keysToOriginalValueMap.Values.ToList() } },
                                                               groupId,
                                                               LayeredCacheConfigNames.GET_ASSET_RULE,
                                                               invalidationKeysMap))
                {
                    if (fullAssetRules != null && fullAssetRules.Count > 0)
                    {
                        response.Objects = fullAssetRules.Values.Where(x => x.Conditions.All(y => y.Type == assetRuleConditionType)).ToList();                            
                    }
                }

                if (response.Objects == null || response.Objects.Count == 0)
                {
                    response.SetStatus(eResponseStatus.OK, NO_ASSET_RULES_FOUND);
                }
                else
                {
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response.SetStatus(eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed GetAssetRules groupID: {0}, ex: {1}", groupId, ex);
            }

            return response;
        }
        
        internal static GenericResponse<AssetRule> AddAssetRule(int groupId, AssetRule assetRule)
        {
            GenericResponse<AssetRule> response = new GenericResponse<AssetRule>();
            try
            {
                if (assetRule == null)
                {
                    log.ErrorFormat("Error while AddAssetRule. assetRule is empty.");
                    return response;
                }

                assetRule.GroupId = groupId;
                DataTable dt = ApiDAL.AddAssetRule(groupId, assetRule.Name, assetRule.Description, (int)AssetRuleType.AssetRule);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    long assetRuleId = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID");
                    assetRule.Id = assetRuleId;

                    // Save assetRulesActions
                    if (!ApiDAL.SaveAssetRuleCB(groupId, assetRule))
                    {
                        log.ErrorFormat("Error while saving AssetRule. groupId: {0}, assetRuleId:{1}", groupId, assetRule.Id);
                        return response;
                    }

                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAllAssetRulesInvalidationKey());

                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                    response.Object = assetRule;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while adding new assetRule . groupId: {0}, assetRule: {1}, ex: {2}", groupId, JsonConvert.SerializeObject(assetRule), ex);
            }

            return response;
        }

        internal static Status DeleteAssetRule(int groupId, long assetRuleId)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                //check AssetRule exists
                AssetRule assetRule = ApiDAL.GetAssetRuleCB(assetRuleId);
                if (assetRule == null || assetRule.Id == 0 || groupId != assetRule.GroupId)
                {
                    response.Set((int)eResponseStatus.AssetRuleNotExists, ASSET_RULE_NOT_EXIST);
                    return response;
                }

                if (assetRule.HasCountryConditions())
                {
                    ResetMediaCountries(groupId, assetRule.Id);
                }
                
                if (!ApiDAL.DeleteAssetRule(groupId, assetRuleId))
                {
                    response.Set((int)eResponseStatus.Error, ASSET_RULE_FAILED_DELETE);
                    return response;
                }

                // delete assetRulesActions from CB
                if (!ApiDAL.DeleteAssetRuleCB(groupId, assetRuleId))
                {
                    log.ErrorFormat("Error while delete AssetRules CB. groupId: {0}, assetRuleId:{1}", groupId, assetRuleId);
                }

                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAllAssetRulesInvalidationKey());
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAssetRuleInvalidationKey(assetRuleId));

                response.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteAssetRule failed ex={0}, groupId={1}, AssetRuleId={2}", ex, groupId, assetRuleId);
            }

            return response;
        }
        
        internal static GenericResponse<AssetRule> UpdateAssetRule(int groupId, AssetRule assetRule)
        {
            GenericResponse<AssetRule> response = new GenericResponse<AssetRule>();
            try
            {
                //check AssetRule exists
                AssetRule oldAssetRule = ApiDAL.GetAssetRuleCB(assetRule.Id);
                assetRule.GroupId = groupId;
                if (assetRule == null || assetRule.Id == 0 || groupId != oldAssetRule.GroupId)
                {
                    response.SetStatus(eResponseStatus.AssetRuleNotExists, ASSET_RULE_NOT_EXIST);
                    return response;
                }
                
                if (!ApiDAL.UpdateAssetRule(groupId, assetRule.Id, assetRule.Name, assetRule.Description))
                {
                    response.SetStatus(eResponseStatus.Error, ASSET_RULE_FAILED_UPDATE);
                    return response;
                }

                // before saving AssetRule complete name,actions,conditions in case they are empty
                assetRule.FillEmpty(oldAssetRule);

                if (assetRule.HasCountryConditions())
                {
                    ResetMediaCountries(groupId, assetRule.Id);
                    //TODO Irena - run rule after update
                }

                // upsert dtUpdatedAssetRulesActions            
                if (!ApiDAL.SaveAssetRuleCB(groupId, assetRule))
                {
                    log.ErrorFormat("Error while saving AssetRule. groupId: {0}, assetRuleId:{1}", groupId, assetRule.Id);
                }
                else
                {
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAllAssetRulesInvalidationKey());
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAssetRuleInvalidationKey(assetRule.Id));

                    response.Object = assetRule;
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateAssetRule failed ex={0}, groupId={1}, AssetRuleId={2}", ex, groupId, assetRule.Id);
            }

            return response;
        }

        private static void ResetMediaCountries(int groupId, long ruleId)
        {
            DataTable mediaTable = ApiDAL.UpdateMediaCountry(groupId, ruleId);
            if (mediaTable != null && mediaTable.Rows != null && mediaTable.Rows.Count > 0)
            {
                List<int> mediaIds = new List<int>();
                foreach (DataRow dr in mediaTable.Rows)
                {
                    int mediaId = ODBCWrapper.Utils.GetIntSafeVal(dr, "MEDIA_ID");
                    mediaIds.Add(mediaId);

                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetMediaCountriesInvalidationKey(mediaId));
                }

                if (Catalog.Module.UpdateIndex(mediaIds, groupId, eAction.Update))
                {
                    log.InfoFormat("Successfully updated index after asset rule update for assets: {0}", string.Join(",", mediaIds));
                }
                else
                {
                    log.InfoFormat("Failed to update index after asset rule update for assets", string.Join(",", mediaIds));
                }
            }
        }

        private static Tuple<List<AssetRule>, bool> GetAllAssetRulesDB(Dictionary<string, object> funcParams)
        {
            List<AssetRule> assetRules = null;

            try
            {
                DataTable dtAssetRules = ApiDAL.GetAssetRulesDB();
                assetRules = new List<AssetRule>();

                if (dtAssetRules != null && dtAssetRules.Rows != null && dtAssetRules.Rows.Count > 0)
                {
                    foreach (DataRow assetRuleRow in dtAssetRules.Rows)
                    {
                        long assetRuleId = ODBCWrapper.Utils.GetLongSafeVal(assetRuleRow, "ID");
                        int groupId = ODBCWrapper.Utils.GetIntSafeVal(assetRuleRow, "GROUP_ID");
                        int assetRuleType = ODBCWrapper.Utils.GetIntSafeVal(assetRuleRow, "ASSET_RULE_TYPE");

                        AssetRule assetRule = new AssetRule()
                        {
                            Id = assetRuleId,
                        };

                        assetRules.Add(assetRule);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetAllAssetRulesDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<AssetRule>, bool>(assetRules, assetRules != null);
        }

        private static Tuple<Dictionary<string, AssetRule>, bool> GetFullAssetRulesCB(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, AssetRule> result = new Dictionary<string, AssetRule>();
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
                            AssetRule assetRule = ApiDAL.GetAssetRuleCB(ruleId);

                            if (assetRule != null)
                            {
                                string assetRuleKey = LayeredCacheKeys.GetAssetRuleKey(assetRule.Id);
                                result.Add(assetRuleKey, assetRule);
                            }
                        }

                        res = result.Count == ruleIds.Count();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetFullAssetRulesCB failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, AssetRule>, bool>(result, res);
        }
    }
}
