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

        private static IEnumerable<int> GetAllCountriesBut(int groupId, List<int> countries)
        {
            List<int> response = new List<int>();
            CountryLocaleResponse countrieResponse = Api.Module.GetCountryList(groupId, null);
            if (countrieResponse.Status.Code == (int)eResponseStatus.OK)
            {
                response = countrieResponse.Countries.Select(c => c.Id).Where(c => !countries.Contains(c)).ToList();
                response.AddRange(countrieResponse.CountryLocales.Select(c => c.Id).Where(c => !countries.Contains(c)));
            }
            return response;
        }

        private static double CalcTotalOfssetForCountry(int groupId, AssetRuleAction action, int country)
        {
            TimeOffsetRuleAction offsetAction = (TimeOffsetRuleAction)action;
            double totalOffset = offsetAction.Offset;

            if (offsetAction.TimeZone)
            {
                totalOffset += GetTimeZoneOffsetForCountry(groupId, country);
            }
            return totalOffset;
        }

        private static double GetTimeZoneOffsetForCountry(int groupId, int countryId)
        {
            CountryLocaleResponse countriesResponse = Api.Module.GetCountryList(groupId, new List<int>() { countryId });
            if (countriesResponse.Status.Code != (int)eResponseStatus.OK || countriesResponse.Countries.Count == 0)
            {
                log.ErrorFormat("Failed to get countryId = {0}, groupId = {1}", countryId, groupId);
                return 0;
            }

            Country country = countriesResponse.Countries[0];

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

            return tzi.BaseUtcOffset.TotalSeconds;
        }

        private static Dictionary<int, List<AssetRule>> GetRules(int groupId, List<long> rulesIds)
        {
            Dictionary<int, List<AssetRule>> rules = new Dictionary<int, List<AssetRule>>();

            AssetRulesResponse ruleResponse = GetAssetRules();
            if (ruleResponse.Status.Code == (int)eResponseStatus.OK && ruleResponse.AssetRules != null)
            {
                if (groupId != 0)
                {
                    rules.Add(groupId, ruleResponse.AssetRules);
                }
                else
                {
                    foreach (var rule in ruleResponse.AssetRules)
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
                assetTypes = group.GetMediaTypes()

            };

            // Call catalog
            return unifiedSearchRequest.GetResponse(unifiedSearchRequest) as UnifiedSearchResponse;
        }

        /******************************/

        internal static AssetRulesResponse AddAssetRule(int groupId, AssetRule assetRule)
        {

            AssetRulesResponse response = new AssetRulesResponse() { Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };
            try
            {
                if (assetRule == null)
                {
                    log.ErrorFormat("Error while AddAssetRule. assetRule is empty.");
                    return response;
                }

                assetRule.GroupId = groupId;
                DataSet ds = ApiDAL.AddAssetRule(groupId, assetRule.Name, assetRule.Description);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    long assetRuleId = ODBCWrapper.Utils.GetLongSafeVal(ds.Tables[0].Rows[0], "ID");
                    assetRule.Id = assetRuleId;

                    // Save assetRulesActions
                    if (!ApiDAL.SaveAssetRule(groupId, assetRule))
                    {
                        log.ErrorFormat("Error while saving AssetRule. groupId: {0}, assetRuleId:{1}", groupId, assetRule.Id);
                    }
                    else
                    {
                        response.Status.Code = (int)eResponseStatus.OK;
                        response.Status.Message = eResponseStatus.OK.ToString();
                        response.AssetRules = new List<AssetRule>() { assetRule };

                        LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAllAssetRulesInvalidationKey());
                    }
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
                AssetRule assetRule = ApiDAL.GetAssetRule(assetRuleId);
                if (assetRule == null || assetRule.Id == 0 || groupId != assetRule.GroupId)
                {
                    response.Code = (int)eResponseStatus.AssetRuleNotExists;
                    response.Message = ASSET_RULE_NOT_EXIST;
                    return response;
                }

                ResetMediaCountries(groupId, assetRule.Id);

                if (!ApiDAL.DeleteAssetRule(groupId, assetRuleId))
                {
                    response.Code = (int)eResponseStatus.Error;
                    response.Message = ASSET_RULE_FAILED_DELETE;
                    return response;
                }

                // delete assetRulesActions from CB
                if (!ApiDAL.DeleteAssetRuleCb(groupId, assetRuleId))
                {
                    log.ErrorFormat("Error while delete AssetRules CB. groupId: {0}, assetRuleId:{1}", groupId, assetRuleId);
                }

                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAllAssetRulesInvalidationKey());
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAssetRuleInvalidationKey(assetRuleId));

                response.Code = (int)eResponseStatus.OK;
                response.Message = eResponseStatus.OK.ToString();
                return response;

            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteAssetRule failed ex={0}, groupId={1}, AssetRuleId={2}", ex, groupId, assetRuleId);
            }

            return response;
        }

        internal static AssetRulesResponse GetAssetRules(int groupId = 0)
        {
            AssetRulesResponse response = new AssetRulesResponse();
            try
            {
                DataTable assetRules = new DataTable();
                string key = LayeredCacheKeys.GetAllAssetRulesKey();
                bool cacheResult = LayeredCache.Instance.Get<DataTable>(key, ref assetRules, GetAllAssetRules, null, groupId,
                    LayeredCacheConfigNames.GET_ALL_ASSET_RULES, new List<string>() { LayeredCacheKeys.GetAllAssetRulesInvalidationKey() });

                if (!cacheResult)
                {
                    log.ErrorFormat("GetAssetRules - GetAllAssetRules - Failed get data from cache. groupId: {0}", groupId);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    return response;
                }

                if (assetRules == null || assetRules.Rows == null || assetRules.Rows.Count == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "No Asset Rules found");
                    return response;
                }

                DataRow[] filteredAssetRules = null;

                if (groupId > 0)
                {
                    filteredAssetRules = assetRules.Select("GROUP_ID = " + groupId);
                }
                else
                {
                    filteredAssetRules = assetRules.Select();
                }

                if (filteredAssetRules == null)
                {
                    log.ErrorFormat("GetAssetRules - filteredAssetRules is empty. groupId: {0}", groupId);
                    return null;
                }

                Dictionary<string, string> keysToOriginalValueMap = new Dictionary<string, string>();
                Dictionary<string, List<string>> invalidationKeysMap = new Dictionary<string, List<string>>();


                long ruleId = 0;
                foreach (DataRow row in filteredAssetRules)
                {
                    ruleId = ODBCWrapper.Utils.GetLongSafeVal(row, "ID");
                    keysToOriginalValueMap.Add(LayeredCacheKeys.GetAssetRuleKey(ruleId), ruleId.ToString());
                    invalidationKeysMap.Add(LayeredCacheKeys.GetAssetRuleKey(ruleId), new List<string>() { LayeredCacheKeys.GetAssetRuleInvalidationKey(ruleId) });
                }

                Dictionary<string, string> fullAssetRules = null;

                // try to get from cache            
                if (LayeredCache.Instance.GetValues<string>(keysToOriginalValueMap, ref fullAssetRules, GetAssetRules, new Dictionary<string, object>() {
                                                            { "ruleIds", keysToOriginalValueMap.Values.ToList() } }, groupId, LayeredCacheConfigNames.GET_ASSET_RULE, invalidationKeysMap))
                {
                    JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };

                    if (fullAssetRules != null && fullAssetRules.Count > 0)
                    {
                        response.AssetRules = fullAssetRules.Values.Select(x => JsonConvert.DeserializeObject<AssetRule>(x, jsonSerializerSettings)).ToList();
                    }
                }

                if (response.AssetRules == null || response.AssetRules.Count == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "No Asset Rules found");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed GetAssetRules groupID: {0}, ex: {1}", groupId, ex);
            }

            return response;
        }

        private static Tuple<DataTable, bool> GetAllAssetRules(Dictionary<string, object> funcParams)
        {
            DataTable dtAssetRules = DAL.ApiDAL.GetAssetRules();
            return new Tuple<DataTable, bool>(dtAssetRules, dtAssetRules != null);
        }

        private static Tuple<Dictionary<string, string>, bool> GetAssetRules(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, string> result = new Dictionary<string, string>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("ruleIds"))
                {
                    List<string> ids = null;
                    if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                    {
                        ids = funcParams[LayeredCache.MISSING_KEYS] as List<string>;
                    }
                    else
                    {
                        ids = funcParams["ruleIds"] != null ? funcParams["ruleIds"] as List<string> : null;
                    }

                    if (ids != null && ids.Count > 0)
                    {
                        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };

                        long ruleId = 0;
                        AssetRule assetRule = null;

                        foreach (string key in ids)
                        {
                            ruleId = long.Parse(key);
                            assetRule = ApiDAL.GetAssetRule(ruleId);
                            if (assetRule != null)
                            {
                                result.Add(LayeredCacheKeys.GetAssetRuleKey(ruleId), JsonConvert.SerializeObject(assetRule, jsonSerializerSettings));
                            }
                        }
                    }

                    res = result.Keys.Count() == ids.Count();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetAssetRules failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, string>, bool>(result, res);
        }

        internal static AssetRulesResponse UpdateAssetRule(int groupId, AssetRule assetRule)
        {
            AssetRulesResponse response = new AssetRulesResponse() { Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };
            try
            {
                //check AssetRule exists
                AssetRule oldAssetRule = ApiDAL.GetAssetRule(assetRule.Id);
                assetRule.GroupId = groupId;
                if (assetRule == null || assetRule.Id == 0 || groupId != oldAssetRule.GroupId)
                {
                    response.Status.Code = (int)eResponseStatus.AssetRuleNotExists;
                    response.Status.Message = ASSET_RULE_NOT_EXIST;
                    return response;
                }

                if (!ApiDAL.UpdateAssetRule(groupId, assetRule.Id, assetRule.Name, assetRule.Description))
                {
                    response.Status.Code = (int)eResponseStatus.Error;
                    response.Status.Message = ASSET_RULE_FAILED_UPDATE;
                    return response;
                }

                ResetMediaCountries(groupId, assetRule.Id);
                //Irena run rule after update

                // before saving AssetRule complete name,actions,conditions in case they are empty
                if (string.IsNullOrEmpty(assetRule.Name))
                {
                    assetRule.Name = oldAssetRule.Name;
                }

                if (assetRule.Actions == null || assetRule.Actions.Count == 0)
                {
                    assetRule.Actions = oldAssetRule.Actions;
                }

                if (assetRule.Conditions == null || assetRule.Conditions.Count == 0)
                {
                    assetRule.Conditions = oldAssetRule.Conditions;
                }

                // upsert dtUpdatedAssetRulesActions            
                if (!ApiDAL.SaveAssetRule(groupId, assetRule))
                {
                    log.ErrorFormat("Error while saving AssetRule. groupId: {0}, assetRuleId:{1}", groupId, assetRule.Id);
                }
                else
                {
                    response.AssetRules = new List<AssetRule>();
                    response.AssetRules.Add(assetRule);
                    response.Status.Code = (int)eResponseStatus.OK;
                    response.Status.Message = eResponseStatus.OK.ToString();

                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAllAssetRulesInvalidationKey());
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAssetRuleInvalidationKey(assetRule.Id));
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
                    mediaIds.Add(ODBCWrapper.Utils.GetIntSafeVal(dr, "MEDIA_ID"));
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
    }
}
