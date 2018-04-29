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

        public static int DoActionRules(int groupId = 0, List<long> rulesIds = null)
        {
            int result = -1;

            try
            {
                // Get all rules of this group
                Dictionary<int, List<AssetRule>> allRules = GetRules(groupId, rulesIds);
                foreach (KeyValuePair<int, List<AssetRule>> pair in allRules)
                {
                    groupId = pair.Key;
                    List<AssetRule> rules = pair.Value;
                    Task<int>[] tasks = new Task<int>[rules.Count];

                    for (int ruleIndex = 0; ruleIndex < rules.Count; ruleIndex++)
                    {
                        tasks[ruleIndex] = new Task<int>(
                            (obj) =>
                            {
                                long ruleId = -1;

                                try
                                {
                                    var rule = rules[(int)obj];
                                    ruleId = rule.Id;

                                    // separate the country conditions and the ksql, 
                                    List<CountryCondition> countryConditions = rule.Conditions.Where(c => c.Type == AssetRuleConditionType.Country).Select(c => (CountryCondition)c).ToList();
                                    List<AssetCondition> assetConditions = rule.Conditions.Where(c => c.Type == AssetRuleConditionType.Asset).Select(c => (AssetCondition)c).ToList();

                                    string ksqlFilter = null;

                                    // concat the ksql with 'and'
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
                                                countriesToAllow.AddRange(GetAllCountriesBut(countryCondition.Countries));
                                            }
                                            else
                                            {
                                                countriesToAllow.AddRange(countryCondition.Countries);
                                            }
                                        }

                                        countriesToAllow = countriesToAllow.Distinct().ToList();
                                    }

                                    foreach (int country in countriesToAllow)
                                    {
                                        foreach (var action in rule.Actions)
                                        {
                                            List<int> assetIds = new List<int>();
                                            string actionKsqlFilter;

                                            if (action.Type == RuleActionType.StartDateOffset)
                                            {
                                                // append the country and offset conditions
                                                long totalOffset = CalcTotalOfssetForCountry(groupId, action, country);
                                                actionKsqlFilter = string.Format("(and {0} start_date <= '-{1}' allowed_countries != '{2}')", ksqlFilter, totalOffset, country); //IRENA: not in

                                                UnifiedSearchResponse unifiedSearcjResponse = GetUnifiedSearchResponse(groupId, actionKsqlFilter);

                                                if (unifiedSearcjResponse != null)
                                                {
                                                    bool isSearchSuccessfull = unifiedSearcjResponse.status.Code == (int)eResponseStatus.OK;
                                                    if (isSearchSuccessfull && unifiedSearcjResponse.searchResults != null && unifiedSearcjResponse.searchResults.Count > 0)
                                                    {
                                                        assetIds = unifiedSearcjResponse.searchResults.Select(asset => Convert.ToInt32(asset.AssetId)).ToList();

                                                        // Apply rule on assets that returned from search
                                                        if (AddCountryToAllowedToAssets(groupId, assetIds, country))
                                                        {
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
                                                    long totalOffset = CalcTotalOfssetForCountry(groupId, action, country);
                                                    actionKsqlFilter = string.Format("(and {0} end_date <= '-{1}' blocked_countries != '{2}')", ksqlFilter, totalOffset, country);
                                                }
                                                else // block
                                                {
                                                    actionKsqlFilter = string.Format("(and {0} blocked_countries != '{1}')", ksqlFilter, country);
                                                }
                                                UnifiedSearchResponse unifiedSearcjResponse = GetUnifiedSearchResponse(groupId, actionKsqlFilter);

                                                if (unifiedSearcjResponse != null)
                                                {
                                                    bool isSearchSuccessfull = unifiedSearcjResponse.status.Code == (int)eResponseStatus.OK;
                                                    if (isSearchSuccessfull && unifiedSearcjResponse.searchResults != null && unifiedSearcjResponse.searchResults.Count > 0)
                                                    {
                                                        assetIds = unifiedSearcjResponse.searchResults.Select(asset => Convert.ToInt32(asset.AssetId)).ToList();

                                                        // Apply rule on assets that returned from search
                                                        if (AddCountryToBlockedToAssets(groupId, assetIds, country))
                                                        {
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
                                    return result; // IRENA: Make sure its OK, had assetIds.Count before

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
                            result += task.Result;
                            task.Dispose();
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in DoActionRules", ex);
            }

            return result;
        }

        private static IEnumerable<int> GetAllCountriesBut(List<int> countries)
        {
            throw new NotImplementedException();
        }

        private static bool AddCountryToAllowedToAssets(int groupId, List<int> assetIds, int country)
        {
            throw new NotImplementedException();
        }

        private static bool AddCountryToBlockedToAssets(int groupId, List<int> assetIds, int country)
        {
            throw new NotImplementedException();
        }

        private static long CalcTotalOfssetForCountry(int groupId, AssetRuleAction action, int country)
        {
            throw new NotImplementedException();
        }

        private static Dictionary<int, List<AssetRule>> GetRules(int groupId, List<long> rulesIds)
        {
            throw new NotImplementedException();
        }

        private static UnifiedSearchResponse GetUnifiedSearchResponse(int groupId, string ksql)
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
                m_nGroupID = groupId,
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
                    m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC
                },
                filterQuery = ksql,
            };

            // Call catalog
            return unifiedSearchRequest.GetResponse(unifiedSearchRequest) as UnifiedSearchResponse;
        }

    }
}
