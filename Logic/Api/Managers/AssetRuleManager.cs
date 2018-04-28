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

                                    //IRENA: handle BLOCK action

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
                                    List<int> allowedCountries = new List<int>();
                                    List<int> blockedCountries = new List<int>();

                                    if (countryConditions != null && countryConditions.Count > 0)
                                    {
                                        foreach (var countryCondition in countryConditions)
                                        {
                                            if (countryCondition.Not)
                                            {
                                                blockedCountries.AddRange(countryCondition.Countries);
                                            }
                                            else
                                            {
                                                allowedCountries.AddRange(countryCondition.Countries);
                                            }
                                        }

                                        blockedCountries = blockedCountries.Distinct().ToList();
                                        allowedCountries = allowedCountries.Distinct().ToList();
                                    }

                                    foreach (int country in allowedCountries)
                                    {
                                        // append the country condition
                                        string countryKsqlFilter = string.Format("(and {0} : allowed_countries {1})", country, ksqlFilter);

                                        foreach (var action in rule.Actions)
                                        {
                                            if (action.Type == RuleActionType.StartDateOffset)
                                            {
                                                long totalOffset = CalcTotalConditionOfssetForCountry(groupId, action, country);
                                                string actionKsqlFilter = string.Format("(and {0} start_date <= -{1})", countryKsqlFilter, totalOffset);

                                                UnifiedSearchResponse unifiedSearcjResponse = GetUnifiedSearchResponse(groupId, actionKsqlFilter);

                                                List<int> assetIds = new List<int>();
                                                if (unifiedSearcjResponse != null)
                                                {
                                                    bool isSearchSuccessfull = unifiedSearcjResponse.status.Code == (int)eResponseStatus.OK;
                                                    bool? isAppliedSuccessfully = null;
                                                    if (isSearchSuccessfull && unifiedSearcjResponse.searchResults != null && unifiedSearcjResponse.searchResults.Count > 0)
                                                    {
                                                        // Remember count, first and last result - to verify that action was successful
                                                        int count = unifiedSearcjResponse.m_nTotalItems;
                                                        string firstAssetId = unifiedSearcjResponse.searchResults.FirstOrDefault().AssetId;
                                                        string lastAssetId = unifiedSearcjResponse.searchResults.LastOrDefault().AssetId;

                                                        assetIds = unifiedSearcjResponse.searchResults.Select(asset => Convert.ToInt32(asset.AssetId)).ToList();

                                                        // Apply rule on assets that returned from search
                                                        isAppliedSuccessfully = AddCountryToAllowedToAssets(groupId, assetIds, country);
                                                        if (isAppliedSuccessfully.Value)
                                                        {
                                                            log.InfoFormat("Successfully applied asset rule: {0} on assets: {1}", rule.ToString(), string.Join(",", assetIds));
                                                        }
                                                        else
                                                        {

                                                            log.InfoFormat("Failed to apply asset rule: {0} on assets: {1}", rule.ToString(), string.Join(",", assetIds));
                                                        }

                                                        // IRENA: Ask Lior WHAT???

                                                        //
                                                        //
                                                        // TODO: Only save the results in CB, and on next run perform verification search
                                                        //
                                                        //
                                                        //
                                                        //
                                                        //
                                                        //

                                                        /*
                                                        var verificationResponse = unifiedSearchRequest.GetResponse(unifiedSearchRequest) as UnifiedSearchResponse;

                                                        if (verificationResponse != null && verificationResponse.status.Code == (int)eResponseStatus.OK &&
                                                            verificationResponse.searchResults != null && verificationResponse.searchResults.Count > 0)
                                                        {
                                                            int verificationCount = verificationResponse.m_nTotalItems;
                                                            string verificationFirstAssetId = unifiedSearcjResponse.searchResults.FirstOrDefault().AssetId;
                                                            string verificationLastAssetId = unifiedSearcjResponse.searchResults.LastOrDefault().AssetId;

                                                            // If search result is identical, it means that action is invalid - either the KSQL is not good or the action itself
                                                            if (count == verificationCount && firstAssetId == verificationFirstAssetId && lastAssetId == verificationLastAssetId)
                                                            {
                                                                //this.DisableRule(groupId, rule);
                                                            }
                                                        }
                                                        */
                                                    }

                                                    if (isSearchSuccessfull && (!isAppliedSuccessfully.HasValue || isAppliedSuccessfully.Value))
                                                    {
                                                        // init result for DoActionByRuleIds
                                                        result = 0;
                                                        if (!ApiDAL.UpdateAssetRuleLastRunDate(rule.Id))
                                                        {
                                                            log.WarnFormat("failed to update asset rule last run date for groupId: {0}, rule: {1}", groupId, rule);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
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

        private static bool? AddCountryToAllowedToAssets(int groupId, List<int> assetIds, int country)
        {
            throw new NotImplementedException();
        }

        private static long CalcTotalConditionOfssetForCountry(int groupId, AssetRuleAction action, int country)
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
                    m_bOnlyActiveMedia = true
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
                filterQuery = ksql
            };

            // Call catalog
            return unifiedSearchRequest.GetResponse(unifiedSearchRequest) as UnifiedSearchResponse;
        }

    }
}
