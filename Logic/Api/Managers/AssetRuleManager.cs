using ApiObjects;
using ApiObjects.AssetLifeCycleRules;
using ApiObjects.Response;
using ApiObjects.Rules;
using ApiObjects.SearchObjects;
using ConfigurationManager;
using Core.Catalog.Request;
using Core.Catalog.Response;
using DAL;
using GroupsCacheManager;
using KLogMonitor;
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

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());                

        public static int DoActionRules()
        {
            int groupId = 0;
            List<long> rulesIds = null;
            List <int> result = new List<int>();

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

                        for (int ruleIndex = 0; ruleIndex < rules.Count; ruleIndex++)
                        {
                            tasks[ruleIndex] = new Task<List<int>>(
                                (obj) =>
                                {
                                    long ruleId = -1;

                                    try
                                    {
                                        var rule = rules[(int)obj];
                                        ruleId = rule.Id;

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
                                        List<int> countriesToAllow = new List<int>();

                                        if (countryConditions != null && countryConditions.Count > 0)
                                        {
                                            foreach (var countryCondition in countryConditions)
                                            {
                                                if (countryCondition.Not)
                                                {
                                                    countriesToAllow.AddRange(GetAllCountriesBut(groupId, countryCondition.Countries));
                                                }
                                                else
                                                {
                                                    countriesToAllow.AddRange(countryCondition.Countries);
                                                }
                                            }

                                            countriesToAllow = countriesToAllow.Distinct().ToList();
                                        }

                                        List<int> assetIds = new List<int>();

                                        foreach (int country in countriesToAllow)
                                        {
                                            foreach (var action in rule.Actions)
                                            {
                                                assetIds = new List<int>();
                                                string actionKsqlFilter;

                                                if (action.Type == RuleActionType.StartDateOffset)
                                                {
                                                    // append the country and offset conditions
                                                    double totalOffset = CalcTotalOfssetForCountry(groupId, action, country);
                                                    actionKsqlFilter = string.Format("(and {0} start_date <= '-{1}' allowed_countries != '{2}')", ksqlFilter, totalOffset, country);

                                                    UnifiedSearchResponse unifiedSearcjResponse = GetUnifiedSearchResponse(group, actionKsqlFilter);

                                                    if (unifiedSearcjResponse != null)
                                                    {
                                                        bool isSearchSuccessfull = unifiedSearcjResponse.status.Code == (int)eResponseStatus.OK;
                                                        if (isSearchSuccessfull && unifiedSearcjResponse.searchResults != null && unifiedSearcjResponse.searchResults.Count > 0)
                                                        {
                                                            assetIds = unifiedSearcjResponse.searchResults.Select(asset => Convert.ToInt32(asset.AssetId)).ToList();

                                                            // Apply rule on assets that returned from search
                                                            if (ApiDAL.InsertMediaCountry(groupId, assetIds, country, true))
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
                                                        actionKsqlFilter = string.Format("(and {0} end_date <= '-{1}' blocked_countries != '{2}')", ksqlFilter, totalOffset, country);
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
                                                            if (ApiDAL.InsertMediaCountry(groupId, assetIds, country, false))
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
                                            }
                                        }

                                        /*if (isSearchSuccessfull && (!isAppliedSuccessfully.HasValue || isAppliedSuccessfully.Value))
                                        {
                                            // init result for DoActionByRuleIds
                                            result = 0;
                                            if (!ApiDAL.UpdateAssetRuleLastRunDate(rule.Id))
                                            {
                                                log.WarnFormat("failed to update asset rule last run date for groupId: {0}, rule: {1}", groupId, rule);
                                            }
                                        }*/
                                        return modifiedAssetIds;

                                    }
                                    catch (Exception ex)
                                    {
                                        log.ErrorFormat("Failed doing actions of rule: groupId = {0}, ruleId = {1}, ex = {2}", groupId, ruleId, ex);
                                        return result;
                                    }
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
                    #endregion
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in DoActionRules", ex);
            }

            return result.Count;
        }

        private static IEnumerable<int> GetAllCountriesBut(int groupId, List<int> countries)
        {
            CountryLocaleResponse countrieResponse = Api.Module.GetCountryList(groupId, null);
            if (countrieResponse.Status.Code == (int)eResponseStatus.OK)
            {
                return countrieResponse.Countries.Select(c => c.Id).Where(c => !countries.Contains(c));
            }
            return null;
        }

        private static double CalcTotalOfssetForCountry(int groupId, AssetRuleAction action, int country)
        {
            StartDateOffsetRuleAction offsetAction = (StartDateOffsetRuleAction)action;
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
                log.DebugFormat("Failed to get time zone ID for country = {0}, groupId = {1}", countryId, groupId);
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
            
            // IRENA: make sure Anat supports 0;
            AssetRulesResponse ruleResponse = Api.api.GetAssetRules();
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
            int pageSize = ApplicationConfiguration.ElasticSearchConfiguration.MaxResults.IntValue;
            

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

    }
}
