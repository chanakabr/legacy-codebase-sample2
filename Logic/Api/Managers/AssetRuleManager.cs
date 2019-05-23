using APILogic.Api.Managers;
using APILogic.ConditionalAccess;
using ApiObjects;
using ApiObjects.AssetLifeCycleRules;
using ApiObjects.Response;
using ApiObjects.Rules;
using ApiObjects.SearchObjects;
using ApiObjects.TimeShiftedTv;
using CachingProvider.LayeredCache;
using ConfigurationManager;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
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
        private const string ROUTING_KEY_GEO_RULE_UPDATE = "PROCESS_GEO_RULE_UPDATE\\{0}";
        private const string ASSET_RULE_NOT_EXIST = "Asset rule doesn't exist";
        private const string ASSET_RULE_FAILED_DELETE = "failed to delete Asset rule";
        private const string ASSET_RULE_FAILED_UPDATE = "failed to update Asset rule";
        
        private const int MAX_ASSETS_TO_UPDATE = 1000;

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Remote Task Methods

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
                    bool doesGroupUsesTemplates = Catalog.CatalogManagement.CatalogManager.DoesGroupUsesTemplates(groupId);
                    Group group = null;
                    CatalogGroupCache catalogGroupCache = null;

                    if (doesGroupUsesTemplates)
                    {
                        if (!Catalog.CatalogManagement.CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                        {
                            log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling DoActionRules", groupId);
                            return result.Count;
                        }
                    }
                    else
                    {
                        group = new GroupsCacheManager.GroupManager().GetGroup(groupId);
                    }

                    bool isGeoAvailabilityWindowingEnabled = doesGroupUsesTemplates ? catalogGroupCache.IsGeoAvailabilityWindowingEnabled : group.isGeoAvailabilityWindowingEnabled;

                    if (isGeoAvailabilityWindowingEnabled)
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
                                return DoActionOnRule(rules[(int)obj], groupId);
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

                            int bulkSize = 10000;
                            for (int i = 0; i < result.Count; i = i + bulkSize)
                            {
                                var pageMediaIds = result.Skip(i).Take(bulkSize).ToList();
                                RebuildIndexForMedias(groupId, doesGroupUsesTemplates, pageMediaIds);
                            }

                            List<long> ranRules = allRules.Values.SelectMany(ar => ar).Select(ar => ar.Id).ToList();
                            if (!ApiDAL.UpdateAssetRulesLastRunDate(groupId, ranRules))
                            {
                                log.ErrorFormat("Failed to update asset rule last run date, rule IDs = {0}", string.Join(", ", ranRules));
                            }
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

        public static bool UpdateMedia(int groupId, long mediaId, bool checkGeoAvailabilityEnabled = false)
        {
            try
            {
                if (checkGeoAvailabilityEnabled && 
                    (!IsGeoAssetRulesEnabled(groupId) || UtilsDal.GetObjectFromCB<string>(eCouchbaseBucket.OTT_APPS, GetGeoUpdateMediaCbKey(groupId, mediaId)) == null))
                {
                    return true;
                }

                List<AssetRule> assetRules = GetMediaRelatedGeoRules(groupId, mediaId);
                if (assetRules != null && assetRules.Count > 0)
                {
                    List<long> assetRuleIdsToRemove = new List<long>();
                    foreach (AssetRule assetRule in assetRules)
                    {
                        if (!AssetRuleAppliesOnMedia(mediaId, assetRule))
                        {
                            assetRuleIdsToRemove.Add(assetRule.Id);
                        }
                    }

                    if (assetRuleIdsToRemove.Count > 0)
                    {
                        if (ApiDAL.RemoveCountryRulesFromMedia(groupId, mediaId, assetRuleIdsToRemove))
                        {
                            string invalidationKey = LayeredCacheKeys.GetMediaCountriesInvalidationKey(mediaId);
                            if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                            {
                                log.ErrorFormat("Failed to set invalidation key on media countries key = {0}", invalidationKey);
                            }

                            IndexManager.UpsertMedia(groupId, mediaId);
                        }
                        else
                        {
                            log.ErrorFormat("Failed to remove asset rules for media: {0}, assetRuleIdsToRemove: {2}", mediaId, string.Join(", ", assetRuleIdsToRemove));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in UpdateMedia", ex);
                return false;
            }
            return true;
        }

        private static string GetGeoUpdateMediaCbKey(int groupId, long mediaId)
        {
            return string.Format("GeoUpdate_GroupId_{0}_MediaId_{1}", groupId, mediaId);
        }

        private static bool AssetRuleAppliesOnMedia(long mediaId, AssetRule assetRule)
        {
            List<AssetCondition> assetConditions = assetRule.Conditions.Where(c => c.Type == RuleConditionType.Asset).Select(c => c as AssetCondition).ToList();
            string ksqlFilter = null;

            if (assetConditions != null && assetConditions.Count > 0)
            {
                StringBuilder ksql = new StringBuilder("(and ");
                ksql.AppendFormat("media_id = {0}", mediaId);
                foreach (var assetCondition in assetConditions)
                {
                    ksql.Append(" " + assetCondition.Ksql);
                }
                ksql.AppendFormat(")");

                ksqlFilter = ksql.ToString();
            }
            else
            {
                return true; 
            }

            if (!string.IsNullOrEmpty(ksqlFilter))
            {
                UnifiedSearchResponse unifiedSearcjResponse = GetMediaUnifiedSearchResponse(assetRule.GroupId, ksqlFilter);

                if (unifiedSearcjResponse != null)
                {
                    bool isSearchSuccessfull = unifiedSearcjResponse.status.Code == (int)eResponseStatus.OK;
                    if (isSearchSuccessfull && unifiedSearcjResponse.searchResults != null && unifiedSearcjResponse.searchResults.Count > 0)
                    {
                        var assetIds = unifiedSearcjResponse.searchResults.Select(asset => Convert.ToInt32(asset.AssetId)).ToList();
                        if (assetIds != null && assetIds.Count > 0 && assetIds.Contains((int)mediaId))
                            return true;
                    }
                }
            }

            return false;
        }

        private static List<AssetRule> GetMediaRelatedGeoRules(int groupId, long mediaId)
        {
            List<AssetRule> assetRules = new List<AssetRule>();

            DataTable dtAssetRules = ApiDAL.GetGeoAssetRulesAffectingMedia(groupId, mediaId);

            List<long> assetRuleIds = new List<long>();
            if (dtAssetRules != null && dtAssetRules.Rows != null && dtAssetRules.Rows.Count > 0)
            {
                foreach (DataRow assetRuleRow in dtAssetRules.Rows)
                {
                    assetRuleIds.Add(ODBCWrapper.Utils.GetLongSafeVal(assetRuleRow, "RULE_ID"));
                }
            }

            if (assetRuleIds.Count > 0)
            {
                assetRules = ApiDAL.GetAssetRulesCB(assetRuleIds);
            }

            return assetRules;

        }

        private static List<int> DoActionOnRule(AssetRule rule, int groupId)
        {
            List<int> result = new List<int>();

            if (rule.Status == RuleStatus.InProgress)
            {
                log.DebugFormat("Rule In Progress, ruleId = {0} ", rule.Id);
                return result;
            }

            log.DebugFormat("Starting to do action on ruleId = {0} ", rule.Id);

            try
            {
                List<int> modifiedAssetIds = new List<int>();

                // separate the country conditions and the ksql, 
                List<CountryCondition> countryConditions = rule.Conditions.Where(c => c.Type == RuleConditionType.Country).Select(c => c as CountryCondition).ToList();
                List<AssetCondition> assetConditions = rule.Conditions.Where(c => c.Type == RuleConditionType.Asset).Select(c => c as AssetCondition).ToList();

                string ksqlFilter = BuildKsqlFromConditions(assetConditions);

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
                        string actionKsqlFilter = null;
                        bool isAllowed = false;

                        if (action.Type == RuleActionType.StartDateOffset)
                        {
                            // append the country and offset conditions
                            double totalOffset = CalcTotalOfssetForCountry(groupId, action, country);
                            actionKsqlFilter = string.Format("(and {0} start_date <= '{1}' allowed_countries != '{2}')", ksqlFilter, -1 * totalOffset, country);
                            isAllowed = true;
                        }
                        else if (action.Type == RuleActionType.EndDateOffset)
                        {
                            double totalOffset = CalcTotalOfssetForCountry(groupId, action, country);
                            actionKsqlFilter = string.Format("(and {0} end_date <= '{1}' allowed_countries != '{2}')", ksqlFilter, -1 * totalOffset, country);
                            isAllowed = true;
                        }
                        else if (action.Type == RuleActionType.Block)
                        {
                            actionKsqlFilter = string.Format("(and {0} blocked_countries != '{1}')", ksqlFilter, country);
                        }

                        if (!string.IsNullOrEmpty(actionKsqlFilter))
                        {
                            UnifiedSearchResponse unifiedSearcjResponse = GetMediaUnifiedSearchResponse(groupId, actionKsqlFilter);

                            if (unifiedSearcjResponse != null)
                            {
                                bool isSearchSuccessfull = unifiedSearcjResponse.status.Code == (int)eResponseStatus.OK;
                                if (isSearchSuccessfull && unifiedSearcjResponse.searchResults != null && unifiedSearcjResponse.searchResults.Count > 0)
                                {
                                    assetIds = unifiedSearcjResponse.searchResults.Select(asset => Convert.ToInt32(asset.AssetId)).ToList();

                                    // Apply rule on assets that returned from search
                                    if (ApiDAL.InsertMediaCountry(groupId, assetIds, country, isAllowed, rule.Id))
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
                                        log.InfoFormat("Successfully added country: {0} to allowed countries for assetRule: {1} on assets: {2}", country, rule.ToString(), string.Join(",", assetIds));
                                    }
                                    else
                                    {
                                        log.InfoFormat("Failed to add country: {0} to allowed countries for assetRule: {1} on assets: {2}", country, rule.ToString(), string.Join(",", assetIds));
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

        private static string BuildKsqlFromConditions(List<AssetCondition> assetConditions)
        {
            string ksqlFilter = string.Empty;

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

            return ksqlFilter;
        }

        private static IEnumerable<int> GetAllCountriesBut(int groupId, List<int> countryIds)
        {
            List<int> response = new List<int>();
            List<Country> countries = Core.Api.api.GetCountryListByIds(null, groupId);
            if (countries != null)
            {
                response.AddRange(countries.Select(c => c.Id).Where(c => !countryIds.Contains(c)));
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

            GenericListResponse<AssetRule> ruleResponse = GetAssetRules(RuleConditionType.Country);
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

        private static UnifiedSearchResponse GetMediaUnifiedSearchResponse(int groupId, string ksql)
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
                    m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC
                },
                filterQuery = string.Format("(and asset_type = 'media' {0})", ksql),
                isInternalSearch = true,
                shouldIgnoreEndDate = shouldIgnoreEndDate,
                isAllowedToViewInactiveAssets = true
            };

            // Call catalog
            return unifiedSearchRequest.GetResponse(unifiedSearchRequest) as UnifiedSearchResponse;
        }

        public static int RebuildIndexForMedias(int groupId, bool doesGroupUsesTemplates, List<int> mediaToUpdate)
        {
            if (doesGroupUsesTemplates)
            {
                foreach (var assetId in mediaToUpdate)
                {
                    bool indexingResult = Catalog.CatalogManagement.IndexManager.UpsertMedia(groupId, assetId);
                    if (!indexingResult)
                    {
                        log.ErrorFormat("Failed UpsertMedia index for assetId: {0}", assetId);
                    }

                    Catalog.CatalogManagement.AssetManager.InvalidateAsset(eAssetTypes.MEDIA, assetId);

                }
                return mediaToUpdate.Count;
            }
            else
            {
                if (Catalog.Module.UpdateIndex(mediaToUpdate, groupId, eAction.Update))
                {
                    log.InfoFormat("Successfully updated index after asset rule for assets: {0}", string.Join(",", mediaToUpdate));
                    return mediaToUpdate.Count;
                }
                else
                {
                    log.InfoFormat("Failed to update index after asset rule for assets", string.Join(",", mediaToUpdate));
                    return 0;
                }
            }
        }

        #endregion

        #region Public Methods

        internal static GenericListResponse<AssetRule> GetAssetRules(RuleConditionType assetRuleConditionType, int groupId = 0, SlimAsset slimAsset = null, RuleActionType? ruleActionType = null)
        {
            GenericListResponse<AssetRule> response = new GenericListResponse<AssetRule>();

            try
            {
                List<AssetRule> allAssetRules = new List<AssetRule>();
                string allAssetRulesKey = LayeredCacheKeys.GetAllAssetRulesKey(groupId, (int)assetRuleConditionType);
                string allAssetRulesInvalidationKey = groupId != 0 ? 
                    LayeredCacheKeys.GetAllAssetRulesGroupInvalidationKey(groupId)
                        : LayeredCacheKeys.GetAllAssetRulesInvalidationKey();

                if (!LayeredCache.Instance.Get<List<AssetRule>>(allAssetRulesKey,
                                                                ref allAssetRules,
                                                                GetAllAssetRules,
                                                                new Dictionary<string, object>()
                                                                {
                                                                    { "assetRuleConditionType", assetRuleConditionType },
                                                                    { "groupId", groupId }
                                                                },
                                                                groupId,
                                                                LayeredCacheConfigNames.GET_ALL_ASSET_RULES,
                                                                new List<string>() { allAssetRulesInvalidationKey }))
                {
                    log.ErrorFormat("GetAssetRules - GetAllAssetRules - Failed get data from cache. groupId: {0}", groupId);
                    return response;
                }
                
                response.Objects = allAssetRules;

                if (ruleActionType.HasValue)
                {
                    allAssetRules = allAssetRules.Where(x => x.Actions.Any(a => a.Type == ruleActionType.Value)).ToList();
                    response.Objects = allAssetRules;
                }

                if (slimAsset != null)
                {
                    List<AssetRule> assetRulesByAsset = new List<AssetRule>();

                    string assetRulesByAssetKey;
                    if (ruleActionType.HasValue)
                    {
                        assetRulesByAssetKey = LayeredCacheKeys.GetAssetRulesByAssetKey(slimAsset.Id, (int)slimAsset.Type, (int)assetRuleConditionType, (int)ruleActionType.Value);
                    }
                    else
                    {
                        assetRulesByAssetKey = LayeredCacheKeys.GetAssetRulesByAssetKey(slimAsset.Id, (int)slimAsset.Type, (int)assetRuleConditionType);
                    }

                    long assetId = long.Parse(slimAsset.Id);
                    string assetTypeInvalidationKey = slimAsset.Type == eAssetTypes.MEDIA ?
                        LayeredCacheKeys.GetMediaInvalidationKey(groupId, assetId)
                        : LayeredCacheKeys.GetEpgInvalidationKey(groupId, assetId);

                    if (!LayeredCache.Instance.Get<List<AssetRule>>(assetRulesByAssetKey,
                                                                    ref assetRulesByAsset,
                                                                    GetAssetRulesByAsset,
                                                                    new Dictionary<string, object>()
                                                                    {
                                                                        { "allAssetRules", allAssetRules },
                                                                        { "slimAsset", slimAsset },
                                                                        { "groupId", groupId }
                                                                    },
                                                                    groupId,
                                                                    LayeredCacheConfigNames.GET_ASSET_RULES_BY_ASSET,
                                                                    new List<string>()
                                                                    {
                                                                        allAssetRulesInvalidationKey,
                                                                        assetTypeInvalidationKey
                                                                    }))
                    {
                        log.ErrorFormat("GetAssetRules - GetAssetRulesByAsset - Failed get data from cache. groupId: {0}", groupId);
                        return response;
                    }

                    response.Objects = assetRulesByAsset;
                }
                
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                response.SetStatus(eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed GetAssetRules groupID: {0}, ex: {1}", groupId, ex);
            }

            return response;
        }
        
        internal static GenericResponse<AssetRule> AddAssetRule(int groupId, AssetRule assetRuleToAdd)
        {
            GenericResponse<AssetRule> response = new GenericResponse<AssetRule>();

            try
            {
                var validation = ValidateAssetLifeCycleActions(groupId, assetRuleToAdd.Actions.OfType<AssetLifeCycleTransitionAction>());
                if (!validation.IsOkStatusCode())
                {
                    response.SetStatus(validation);
                    return response;
                }

                assetRuleToAdd.GroupId = groupId;
                DataTable dt = ApiDAL.AddAssetRule(groupId, assetRuleToAdd.Name, assetRuleToAdd.Description, (int)AssetRuleType.AssetRule);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    assetRuleToAdd.Id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID");

                    // Save assetRulesActions
                    if (!ApiDAL.SaveAssetRuleCB(groupId, assetRuleToAdd))
                    {
                        log.ErrorFormat("Error while saving AssetRule. groupId: {0}, assetRuleId:{1}", groupId, assetRuleToAdd.Id);
                        return response;
                    }

                    SetInvalidationKeys(groupId);
                    response.Object = assetRuleToAdd;
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while adding new assetRule . groupId: {0}, assetRule: {1}, ex: {2}", groupId, JsonConvert.SerializeObject(assetRuleToAdd), ex);
            }

            return response;
        }

        private static Status ValidateAssetLifeCycleActions(int groupId, IEnumerable<AssetLifeCycleTransitionAction> assetLifeCycleTransitionActions)
        {
            Status res = Status.Ok;

            res = ValidateTagTransitionAction(groupId, assetLifeCycleTransitionActions.OfType<AssetLifeCycleTagTransitionAction>());
            if (!res.IsOkStatusCode())
                return res;

            return ValidateBuisnessModuleTransitionAction(groupId, assetLifeCycleTransitionActions.OfType<AssetLifeCycleBuisnessModuleTransitionAction>());
        }

        private static Status ValidateTagTransitionAction(int groupId, IEnumerable<AssetLifeCycleTagTransitionAction> assetLifeCycleTags)
        {
            Status res = Status.Ok;

            if (assetLifeCycleTags == null || !assetLifeCycleTags.Any())
            {
                return res;
            }

            // validate all tags exists
            var ids = assetLifeCycleTags.Select(s => s.TagIds).SelectMany(s => s).Select(s => (long)s).Distinct().ToList();

            CatalogManager.GetTagValues(groupId, ids, 0, 1000, out int totalItemsCount);

            if (totalItemsCount != ids.Count)
            {
                res.Set((int)eResponseStatus.Error, "tag doesn't exists");
            }

            return res;
        }

        private static Status ValidateBuisnessModuleTransitionAction(int groupId, IEnumerable<AssetLifeCycleBuisnessModuleTransitionAction> assetLifeCycleBuisnessModule)
        {
            Status res = Status.Ok;

            if (assetLifeCycleBuisnessModule == null || !assetLifeCycleBuisnessModule.Any())
            {
                return res;
            }

            // validate all file types exists
            var fileTypeIds = assetLifeCycleBuisnessModule.Select(s => s.Transitions.FileTypeIds).SelectMany(s => s).Select(s => (long)s).Distinct();
            if (fileTypeIds.Any())
            {
                var mediaFileTypesResponse = FileManager.GetMediaFileTypes(groupId);
                if (mediaFileTypesResponse.HasObjects())
                {
                    var count = mediaFileTypesResponse.Objects.Count(x => fileTypeIds.Contains(x.Id));
                    if (count != fileTypeIds.Count())
                    {
                        res.Set((int)eResponseStatus.Error, "file type doesn't exists");
                        return res;
                    }
                }
            }

            // validate all ppv exists
            var ppvIds = assetLifeCycleBuisnessModule.Select(s => s.Transitions.PpvIds).SelectMany(s => s).Distinct().Select(x => x.ToString());
            if (ppvIds.Any())
            {
                var ppvModuleList = Core.Pricing.Module.GetPPVModuleList(groupId);
                if (ppvModuleList.HasObjects())
                {
                    var count = ppvModuleList.Objects.Count(x => ppvIds.Contains(x.m_sObjectCode));
                    if (count != fileTypeIds.Count())
                    {
                        res.Set((int)eResponseStatus.Error, "ppv doesn't exists");
                        return res;
                    }
                }
            }

            return res;
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

                if (assetRule.Status == RuleStatus.InProgress)
                {
                    response.Set((int)eResponseStatus.AssetRuleStatusNotWritable, "Cannot update or delete asset rule when in progress");
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

                SetInvalidationKeys(groupId, assetRuleId);
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
                response.SetStatus(ValidateAssetLifeCycleActions(groupId, assetRule.Actions.OfType<AssetLifeCycleTransitionAction>()));
                if (!response.Status.IsOkStatusCode())
                {
                    return response;
                }


                assetRule.GroupId = groupId;

                if (!ApiDAL.UpdateAssetRule(groupId, assetRule.Id, assetRule.Name, assetRule.Description))
                {
                    response.SetStatus(eResponseStatus.Error, ASSET_RULE_FAILED_UPDATE);
                    return response;
                }

                if (assetRule.HasCountryConditions())
                {
                    HandleRuleUpdate(groupId, ref assetRule);
                }

                // upsert dtUpdatedAssetRulesActions            
                if (!ApiDAL.SaveAssetRuleCB(groupId, assetRule))
                {
                    log.ErrorFormat("Error while saving AssetRule. groupId: {0}, assetRuleId:{1}", groupId, assetRule.Id);
                }
                else
                {
                    SetInvalidationKeys(groupId, assetRule.Id);
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

        internal static GenericResponse<AssetRule> GetAssetRule(int groupId, long assetRuleId)
        {
            GenericResponse<AssetRule> response = new GenericResponse<AssetRule>();
            try
            {
                AssetRule assetRule = ApiDAL.GetAssetRuleCB(assetRuleId);

                if (assetRule == null || assetRule.Id == 0 || groupId != assetRule.GroupId)
                {
                    response.SetStatus(eResponseStatus.AssetRuleNotExists, ASSET_RULE_NOT_EXIST);
                    return response;
                }

                response.Object = assetRule;
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetAssetRule failed ex={0}, groupId={1}, AssetRuleId={2}", ex, groupId, assetRuleId);
            }

            return response;
        }

        internal static List<SlimAsset> GetAssetsForValidation(eAssetTypes assetType, int groupId, long assetId)
        {
            if (assetId <= 0 && (assetType != eAssetTypes.MEDIA && assetType != eAssetTypes.EPG))
            {
                return null;
            }

            List<SlimAsset> assetsToCheck = new List<SlimAsset>()
            {
                new SlimAsset(assetId, assetType)
            };

            // check the program of the linear asset
            if (assetType == eAssetTypes.MEDIA)
            {
                long programId = ConditionalAccess.Utils.GetCurrentProgramByMediaId(groupId, (int)assetId);
                if (programId != 0)
                {
                    assetsToCheck.Add(new SlimAsset(programId, eAssetTypes.EPG));
                }
            }
            else if (assetType == eAssetTypes.EPG)
            {
                long linearMediaId = ApiDAL.GetLinearMediaIdByEpgId(assetId);
                if (linearMediaId > 0)
                {
                    assetsToCheck.Add(new SlimAsset(linearMediaId, eAssetTypes.MEDIA));
                }
            }

            return assetsToCheck;
        }

        internal static List<SlimAsset> GetNpvrAssetsForValidation(int groupId, long epgId, long epgChannelId = 0, int mediaFileId = 0, int linearMediaId = 0)
        {
            if (epgId == 0)
            {
                return null;
            }

            List<SlimAsset> assetsToCheck = new List<SlimAsset>()
            {
                new SlimAsset(epgId, eAssetTypes.NPVR)
            };
            
            if (epgChannelId != 0 && linearMediaId == 0)
            {
                var linearChannelSettings = EpgManager.GetLinearChannelSettings(groupId, epgChannelId);
                if (linearChannelSettings != null)
                {
                    linearMediaId = (int)linearChannelSettings.LinearMediaId;
                }
            }
            else if (mediaFileId != 0 && linearMediaId == 0)
            {
                string mainUrl = string.Empty;
                string altUrl = string.Empty;
                int mainStreamingCoID = 0;
                int altStreamingCoID = 0;
                string fileCoGuid = string.Empty;

                ConditionalAccess.Utils.TryGetFileUrlLinks(groupId, mediaFileId, ref mainUrl, ref altUrl, ref mainStreamingCoID, ref altStreamingCoID, 
                                                           ref linearMediaId, ref fileCoGuid);
            }
            
            if (linearMediaId != 0)
            {
                assetsToCheck.Add(new SlimAsset(linearMediaId, eAssetTypes.MEDIA));
            }

            return assetsToCheck;
        }

        internal static Status CheckNetworkRules(List<SlimAsset> assetsToCheck, int groupId, string ip, out AssetRule blockingRule)
        {
            blockingRule = null;

            if (assetsToCheck == null || assetsToCheck.Count == 0)
            {
                return new Status((int)eResponseStatus.OK);
            }

            long convertedIp;
            APILogic.Utils.ConvertIpToNumber(ip, out convertedIp);
            Dictionary<string, string> headers = ListUtils.ToDictionary(System.Web.HttpContext.Current.Request.Headers);

            ConditionScope conditionScope = new ConditionScope()
            {
                Headers = headers,
                Ip = convertedIp
            };
            log.DebugFormat("CheckNetworkRules - ConditionScope: {0}.",conditionScope.ToString());

            foreach (var asset in assetsToCheck)
            {
                var networkAssetRules = GetAssetRules(RuleConditionType.Asset, groupId, asset, RuleActionType.Block);
                if (networkAssetRules.HasObjects())
                {
                    log.DebugFormat("CheckNetworkRules - there are {0} valid networkAssetRules for groupId {1} and for asset id {2} + type {3}.",
                                    networkAssetRules.Objects.Count, groupId, asset.Id, asset.Type);
                    foreach (var networkRule in networkAssetRules.Objects)
                    {
                        foreach (var condition in networkRule.Conditions)
                        {
                            if ((condition.Type == RuleConditionType.Header || condition.Type == RuleConditionType.Or) && condition.Evaluate(conditionScope))
                            {
                                blockingRule = networkRule;
                                log.DebugFormat("CheckNetworkRules the asset: {0} block because of NetworkRule: {1}.", asset.Id, networkRule.Id);
                                return new Status((int)eResponseStatus.NetworkRuleBlock, "Network rule block");
                            }
                        }
                    }
                }
            }
            
            return new Status((int)eResponseStatus.OK);
        }

        public static bool IsGeoAssetRulesEnabled(int groupId)
        {
            if (CatalogManager.DoesGroupUsesTemplates(groupId))
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    return false;
                }

                return catalogGroupCache.IsAssetUserRuleEnabled;
            }
            else
            {
                GroupManager groupManager = new GroupManager();
                Group group = groupManager.GetGroup(groupId);
                if (group == null)
                {
                    return false;
                }

                List<LanguageObj> languages = group.GetLangauges();

                return group.isAssetUserRuleEnabled;
            }
        }
        
        #endregion

        #region Private Methods

        private static void SetInvalidationKeys(int groupId, long? assetRuleId = null)
        {
            LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAllAssetRulesInvalidationKey());
            LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAllAssetRulesGroupInvalidationKey(groupId));

            if (assetRuleId.HasValue)
            {
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAssetRuleInvalidationKey(assetRuleId.Value));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcParams">assetRuleConditionType, groupId</param>
        /// <returns>true if assetRules is not null, otherwise false</returns>
        private static Tuple<List<AssetRule>, bool> GetAllAssetRules(Dictionary<string, object> funcParams)
        {
            List<AssetRule> allAssetRules = new List<AssetRule>();

            try
            {
                if (funcParams != null && funcParams.Count == 2)
                {
                    if (funcParams.ContainsKey("assetRuleConditionType") && funcParams.ContainsKey("groupId"))
                    {
                        RuleConditionType? assetRuleConditionType = funcParams["assetRuleConditionType"] as RuleConditionType?;
                        int? groupId = funcParams["groupId"] as int?;
                        
                        if (assetRuleConditionType.HasValue && groupId.HasValue)
                        {
                            List<AssetRule> allAssetRulesDB = new List<AssetRule>();
                            string allAssetRulesFromDBKey = LayeredCacheKeys.GetAllAssetRulesFromDBKey();

                            if (!LayeredCache.Instance.Get<List<AssetRule>>(allAssetRulesFromDBKey,
                                                                            ref allAssetRulesDB,
                                                                            GetAllAssetRulesDB,
                                                                            null,
                                                                            groupId.Value,
                                                                            LayeredCacheConfigNames.GET_ALL_ASSET_RULES_FROM_DB,
                                                                            new List<string>() { LayeredCacheKeys.GetAllAssetRulesInvalidationKey() }))
                            {
                                allAssetRules = null;
                                log.ErrorFormat("GetAllAssetRules - GetAllAssetRulesDB - Failed get data from cache. groupId: {0}", groupId);
                            }
                            else if (allAssetRulesDB.Count > 0)
                            {
                                IEnumerable<AssetRule> filteredAssetRules = allAssetRulesDB;

                                if (groupId > 0)
                                {
                                    filteredAssetRules = allAssetRulesDB.Where(x => x.GroupId == groupId);
                                }

                                List<long> ruleIds = null;
                                var rulesTypes = ApiDAL.GetAssetRuleTypeCB(filteredAssetRules.Select(x => x.Id));
                                if (rulesTypes != null && rulesTypes.Count > 0)
                                {
                                    ruleIds = new List<long>(rulesTypes.Where(x => x.TypeIdIn.Contains((int)assetRuleConditionType)).Select(x => x.Id));
                                }

                                if (ruleIds != null)
                                {
                                    var assetRulesCB = ApiDAL.GetAssetRulesCB(ruleIds);

                                    if (assetRulesCB != null && assetRulesCB.Count > 0)
                                    {
                                        allAssetRules = assetRulesCB;
                                    }
                                }
                            }

                            log.Debug("GetAllAssetRules - success");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                allAssetRules = null;
                log.Error(string.Format("GetAllAssetRules faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<AssetRule>, bool>(allAssetRules, allAssetRules != null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcParams">allAssetRules, slimAsset, groupId</param>
        /// <returns>true if assetRulesByAsset is not null, otherwise false</returns>
        private static Tuple<List<AssetRule>, bool> GetAssetRulesByAsset(Dictionary<string, object> funcParams)
        {
            List<AssetRule> assetRulesByAsset = new List<AssetRule>();

            try
            {
                if (funcParams != null && funcParams.Count == 3)
                {
                    if (funcParams.ContainsKey("allAssetRules") && funcParams.ContainsKey("slimAsset") && funcParams.ContainsKey("groupId"))
                    {
                        int? groupId = funcParams["groupId"] as int?;
                        SlimAsset slimAsset = funcParams["slimAsset"] as SlimAsset;
                        List<AssetRule> allAssetRules = funcParams["allAssetRules"] as List<AssetRule>;

                        if (slimAsset != null && allAssetRules != null && allAssetRules.Count > 0 && groupId.HasValue)
                        {
                            // Determine which AssetRules contains Conditions that they are typeof AssetCondition (have Ksql).
                            var assetRulesWithKsql = allAssetRules.Where(x => x.Conditions.Any(y => y is AssetCondition));

                            Parallel.ForEach(assetRulesWithKsql, (currAssetRuleWithKsql) =>
                            {
                                StringBuilder ksqlFilter = new StringBuilder();
                                
                                if (eAssetTypes.EPG == slimAsset.Type)
                                {
                                    ksqlFilter.AppendFormat(string.Format("(and asset_type='epg' epg_id = '{0}' (or", slimAsset.Id));
                                }
                                else if (eAssetTypes.MEDIA == slimAsset.Type)
                                {
                                    ksqlFilter.AppendFormat(string.Format("(and asset_type='media' media_id = '{0}' (or", slimAsset.Id));
                                }
                                else if (eAssetTypes.NPVR == slimAsset.Type)
                                {
                                    ksqlFilter.AppendFormat(string.Format("(and asset_type='recording' epg_id = '{0}' (or", slimAsset.Id));
                                }

                                foreach (var condition in currAssetRuleWithKsql.Conditions)
                                {
                                    if (condition is AssetCondition)
                                    {
                                        AssetCondition assetCondition = condition as AssetCondition;
                                        ksqlFilter.AppendFormat(" {0}", assetCondition.Ksql);
                                    }
                                }

                                ksqlFilter.Append("))");

                                var assets = api.SearchAssets(groupId.Value, ksqlFilter.ToString(), 0, 0, true, 0, true, string.Empty, string.Empty, string.Empty, 0, 0, true, true);

                                // If there is a match, add rule to list
                                if (assets != null && assets.Count() > 0)
                                {
                                    assetRulesByAsset.Add(currAssetRuleWithKsql);
                                }
                            });

                            log.Debug("GetAssetRulesByAsset - success");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                assetRulesByAsset = null;
                log.Error(string.Format("GetAssetRulesByAsset faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<AssetRule>, bool>(assetRulesByAsset, assetRulesByAsset != null);
        }
        
        /// <summary>
        /// GetAllAssetRules from DB with no filter
        /// </summary>
        /// <param name="funcParams">is null</param>
        /// <returns>true if assetRules is not null, otherwise false</returns>
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
                        //int assetRuleType = ODBCWrapper.Utils.GetIntSafeVal(assetRuleRow, "ASSET_RULE_TYPE");

                        AssetRule assetRule = new AssetRule()
                        {
                            Id = assetRuleId,
                            GroupId = groupId
                        };

                        assetRules.Add(assetRule);
                    }
                }
                log.Debug("GetAllAssetRulesDB - success");
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetAllAssetRulesDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<AssetRule>, bool>(assetRules, assetRules != null);
        }
        
        private static void ResetMediaCountries(int groupId, long ruleId)
        {
            DataTable mediaTable = ApiDAL.UpdateMediaCountries(groupId, ruleId);
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

        private static HashSet<int> GetRuleCountriesList(int groupId, AssetRule rule)
        {
            List<CountryCondition> countryConditions = rule.Conditions.Where(c => c.Type == RuleConditionType.Country).Select(c => c as CountryCondition).ToList();

            HashSet<int> result = new HashSet<int>();
            if (countryConditions != null && countryConditions.Count > 0)
            {
                List<int> countries = new List<int>();
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

                foreach(int item in countries)
                {
                    if (!result.Contains(item))
                        result.Add(item);
                }
            }

            return result;
        }

        public static bool HandleRuleUpdate(int groupId, long assetRuleId, List<int> countriesToRemove, bool removeBlocked, bool removeAllowed, bool updateKsql)
        {
            List<long> updatedMediaIds = new List<long>();
            DataTable mediaTable;
            var assetRuleResponse = GetAssetRule(groupId, assetRuleId);
            if (!assetRuleResponse.HasObject())
            {
                log.ErrorFormat("Failed to get asset rule by id = {0}", assetRuleId);
                return true;
            }

            AssetRule assetRule = assetRuleResponse.Object;

            //1. countries
            if (countriesToRemove?.Count > 0)
            {
                //remove from table
                while (true)
                {
                    mediaTable = ApiDAL.UpdateMediaCountriesByCountryIds(groupId, assetRule.Id, countriesToRemove);
                    if (mediaTable != null && mediaTable.Rows != null && mediaTable.Rows.Count > 0)
                    {
                        foreach (DataRow dr in mediaTable.Rows)
                        {
                            long mediaId = ODBCWrapper.Utils.GetLongSafeVal(dr, "MEDIA_ID");
                            updatedMediaIds.Add(mediaId);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //2. Block or EndtDate
            if (removeBlocked)
            {
                //remove block from table
                while (true)
                {
                    mediaTable = ApiDAL.UpdateMediaCountriesByIsAllowed(groupId, assetRule.Id, 0);
                    if (mediaTable != null && mediaTable.Rows != null && mediaTable.Rows.Count > 0)
                    {
                        foreach (DataRow dr in mediaTable.Rows)
                        {
                            int mediaId = ODBCWrapper.Utils.GetIntSafeVal(dr, "MEDIA_ID");
                            updatedMediaIds.Add(mediaId);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //3. StartDate
            if (removeAllowed)
            {
                //remove allowd from table
                while (true)
                {
                    mediaTable = ApiDAL.UpdateMediaCountriesByIsAllowed(groupId, assetRule.Id, 1);
                    if (mediaTable != null && mediaTable.Rows != null && mediaTable.Rows.Count > 0)
                    {
                        foreach (DataRow dr in mediaTable.Rows)
                        {
                            int mediaId = ODBCWrapper.Utils.GetIntSafeVal(dr, "MEDIA_ID");
                            updatedMediaIds.Add(mediaId);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

            }

            //4. Ksql
            if (updateKsql)
            {
                List<AssetCondition> newKsqlConditions = assetRule.Conditions.Where(c => c.Type == RuleConditionType.Asset).Select(c => c as AssetCondition).ToList();
                string newKsql = BuildKsqlFromConditions(newKsqlConditions);

                //1. get all assets from DB
                List<long> mediaIdsToRemove = new List<long>();
                List<long> mediaIds = GetMediaIdsByRule(groupId, assetRule.Id);
                int listLength = mediaIds.Count;

                //2. filter old media
                int bulkSize = 500;
                for (int i = 0; i < listLength; i = i + bulkSize)
                {
                    var pageMediaIds = mediaIds.Skip(i).Take(bulkSize);

                    string ksql = string.Format("(and {0} media_id: '{1}')", newKsql, string.Join(",", pageMediaIds));

                    var unifiedSearchResponse = GetMediaUnifiedSearchResponse(groupId, ksql);
                    if (unifiedSearchResponse != null)
                    {
                        if (unifiedSearchResponse.status.Code == (int)eResponseStatus.OK)
                        {
                            if (unifiedSearchResponse.searchResults != null && unifiedSearchResponse.searchResults.Count > 0)
                            {
                                var assetIds = unifiedSearchResponse.searchResults.Select(asset => long.Parse(asset.AssetId)).ToList();
                                mediaIdsToRemove.AddRange(mediaIds.Where(x => !assetIds.Contains(x)).ToList());
                            }
                            else
                            {
                                mediaIdsToRemove.AddRange(mediaIds);
                            }
                        }
                    }
                }

                //3. remove old from DB
                if (mediaIdsToRemove.Count > 0)
                {
                    while (ApiDAL.UpdateMediaCountriesByMediaIds(groupId, assetRule.Id, mediaIdsToRemove) > 0) ;
                    updatedMediaIds.AddRange(mediaIdsToRemove);
                }
            }

            updatedMediaIds = updatedMediaIds.Distinct().ToList();

            foreach (var mediaId in updatedMediaIds)
            {
                IndexManager.UpsertMedia(groupId, mediaId);
            }

            assetRule.Status = RuleStatus.Ready;
            if (!ApiDAL.SaveAssetRuleCB(groupId, assetRule))
            {
                log.ErrorFormat("Error while saving AssetRule. groupId: {0}, assetRuleId:{1}", groupId, assetRule.Id);
            }
            else
            {
                SetInvalidationKeys(groupId, assetRule.Id);
            }

            return true;
        }

        private static void HandleRuleUpdate(int groupId, ref AssetRule assetRule)
        {
            //get old rule
            var oldRuleResopnse = GetAssetRule(groupId, assetRule.Id);
            if (oldRuleResopnse != null && oldRuleResopnse.HasObject())
            {
                bool removeBlocked = false;
                bool removeAllowed = false;
                bool updateKsql = false;

                var oldRule = oldRuleResopnse.Object;
                
                //1. countries
                HashSet<int> oldCountries = GetRuleCountriesList(groupId, oldRule);
                HashSet<int> newCountries = GetRuleCountriesList(groupId, assetRule);

                List<int> countriesToRemove = oldCountries.Where(x => !newCountries.Contains(x)).ToList();

                //2. Block or EndtDate
                var oldAssetActionBlock = oldRule.Actions.FirstOrDefault<AssetRuleAction>(c => c.Type == RuleActionType.Block);
                var newAssetActionBlock = assetRule.Actions.FirstOrDefault<AssetRuleAction>(c => c.Type == RuleActionType.Block);
                TimeOffsetRuleAction oldAssetActionEndDate = oldRule.Actions.FirstOrDefault<AssetRuleAction>(c => c.Type == RuleActionType.EndDateOffset) as TimeOffsetRuleAction;
                TimeOffsetRuleAction newAssetActionEndDate = assetRule.Actions.FirstOrDefault<AssetRuleAction>(c => c.Type == RuleActionType.EndDateOffset) as TimeOffsetRuleAction;

                if ((oldAssetActionBlock != null && newAssetActionBlock == null) || (oldAssetActionEndDate != null && newAssetActionEndDate == null))
                {
                    removeBlocked = true;
                }

                //3. StartDate
                TimeOffsetRuleAction oldAssetActionStartDate = oldRule.Actions.FirstOrDefault<AssetRuleAction>(c => c.Type == RuleActionType.StartDateOffset) as TimeOffsetRuleAction;
                TimeOffsetRuleAction newAssetActionStartDate = assetRule.Actions.FirstOrDefault<AssetRuleAction>(c => c.Type == RuleActionType.StartDateOffset) as TimeOffsetRuleAction;
                if (oldAssetActionStartDate != null && newAssetActionStartDate == null)
                {
                    removeAllowed = true;
                }

                //4. Ksql
                List<AssetCondition> oldKsqlConditions = oldRule.Conditions.Where(c => c.Type == RuleConditionType.Asset).Select(c => c as AssetCondition).ToList();
                List<AssetCondition> newKsqlConditions = assetRule.Conditions.Where(c => c.Type == RuleConditionType.Asset).Select(c => c as AssetCondition).ToList();
                string oldKsql = BuildKsqlFromConditions(oldKsqlConditions);
                string newKsql = BuildKsqlFromConditions(newKsqlConditions);

                if (oldKsql != newKsql)
                {
                    updateKsql = true;
                }

                if (updateKsql || removeAllowed || removeBlocked || countriesToRemove?.Count > 0)
                {
                    assetRule.Status = RuleStatus.InProgress;

                    GenericCeleryQueue queue = new GenericCeleryQueue();
                    ApiObjects.QueueObjects.GeoRuleUpdateData data = new ApiObjects.QueueObjects.GeoRuleUpdateData(groupId, assetRule.Id,
                        countriesToRemove, removeBlocked, removeAllowed, updateKsql)
                    { ETA = DateTime.UtcNow };
                    bool queueGeoRuleUpdateResult = queue.Enqueue(data, string.Format(ROUTING_KEY_GEO_RULE_UPDATE, groupId));
                    if (!queueGeoRuleUpdateResult)
                    {
                        log.ErrorFormat("Failed to queue GeoRuleUpdateData, assetRuleId: {0}, groupId: {1}", assetRule.Id, groupId);
                    }
                }                
            }
        }

        private static List<long> GetMediaIdsByRule(int groupId, long ruleId)
        {
            List<long> mediaIds = new List<long>();
            long offset = 0;
            while (true)
            {
                DataTable dt = ApiDAL.GetMediaCountriesMediaIdsByRuleId(ruleId, offset);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        offset = ODBCWrapper.Utils.GetLongSafeVal(row, "MEDIA_ID");
                        mediaIds.Add(offset);
                    }
                }
                else
                {
                    break;
                }
            }

            return mediaIds;
        }

        #endregion
    }
}
