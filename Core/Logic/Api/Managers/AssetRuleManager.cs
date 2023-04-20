using ApiLogic.Api.Managers;
using ApiLogic.Api.Validators;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Rules;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Request;
using Core.Catalog.Response;
using DAL;
using DAL.Api;
using GroupsCacheManager;
using Phx.Lib.Log;

using Newtonsoft.Json;
using QueueWrapper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TVinciShared;

namespace Core.Api.Managers
{
    public interface IAssetRuleManager
    {
        GenericListResponse<AssetRule> GetAssetRules(
            RuleConditionType assetRuleConditionType,
            int groupId = 0,
            SlimAsset slimAsset = null,
            RuleActionType? ruleActionType = null,
            string nameContains = null,
            AssetRuleOrderBy orderBy = AssetRuleOrderBy.None);
    }

    public class AssetRuleManager : IAssetRuleManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        internal static readonly HashSet<int> RuleActionsTypes = new HashSet<int>(Enum.GetValues(typeof(RuleActionType)).Cast<int>());
        internal static readonly HashSet<RuleConditionType> RuleConditionTypes = new HashSet<RuleConditionType>(Enum.GetValues(typeof(RuleConditionType)).Cast<RuleConditionType>());

        private const string ROUTING_KEY_GEO_RULE_UPDATE = "PROCESS_GEO_RULE_UPDATE\\{0}";
        private const string ASSET_RULE_NOT_EXIST = "Asset rule doesn't exist";
        private const string ASSET_RULE_FAILED_DELETE = "failed to delete Asset rule";
        private const string ASSET_RULE_FAILED_UPDATE = "failed to update Asset rule";
        private const int MAX_ASSETS_TO_UPDATE = 1000;

        private static readonly Lazy<AssetRuleManager> LazyInstance = new Lazy<AssetRuleManager>(() =>
            new AssetRuleManager(AssetRuleRepository.Instance,
                                 LayeredCache.Instance,
                                 AssetRuleActionValidator.Instance,
                                 IndexManagerFactory.Instance,
                                 ApplicationConfiguration.Current,
                                 CatalogManager.Instance,
                                 new GroupManager(),
                                 api.Instance,
                                 AssetManager.Instance,
                                 Catalog.Module.Instance), 
            LazyThreadSafetyMode.PublicationOnly);

        public static AssetRuleManager Instance => LazyInstance.Value;

        private readonly IAssetRuleRepository _repository;
        private readonly ILayeredCache _layeredCache;
        private readonly IAssetRuleActionValidator _assetRuleActionValidator;
        private readonly IIndexManagerFactory _indexManagerFactory;
        private readonly IApplicationConfiguration _applicationConfiguration;
        private readonly ICatalogManager _catalogManager;
        private readonly IGroupManager _groupManager;
        private readonly ICountryManager _countryManager;
        private readonly IAssetManager _assetManager;
        private readonly ICatalogIndexManager _catalogIndexManager;

        public AssetRuleManager(IAssetRuleRepository repository,
                                ILayeredCache layeredCache,
                                IAssetRuleActionValidator assetRuleActionValidator,
                                IIndexManagerFactory indexManagerFactory,
                                IApplicationConfiguration applicationConfiguration,
                                ICatalogManager catalogManager,
                                IGroupManager groupManager,
                                ICountryManager countryManager,
                                IAssetManager assetManager,
                                ICatalogIndexManager catalogIndexManager)
        {
            _repository = repository;
            _layeredCache = layeredCache;
            _assetRuleActionValidator = assetRuleActionValidator;
            _indexManagerFactory = indexManagerFactory;
            _applicationConfiguration = applicationConfiguration;
            _catalogManager = catalogManager;
            _groupManager = groupManager;
            _countryManager = countryManager;
            _assetManager = assetManager;
            _catalogIndexManager = catalogIndexManager;
        }

        #region Remote Task Methods

        public int DoActionRules()
        {
            int groupId = 0;
            int result = 0;

            try
            {
                // Get all rules of this group                
                Dictionary<int, List<AssetRule>> allRules = GetRules(groupId);
                foreach (KeyValuePair<int, List<AssetRule>> pair in allRules)
                {
                    groupId = pair.Key;
                    bool doesGroupUsesTemplates = _catalogManager.DoesGroupUsesTemplates(groupId);
                    Group group = null;
                    CatalogGroupCache catalogGroupCache = null;

                    if (doesGroupUsesTemplates)
                    {
                        if (!_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                        {
                            log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling DoActionRules", groupId);
                            return result;
                        }
                    }
                    else
                    {
                        group = _groupManager.GetGroup(groupId);
                    }

                    bool isGeoAvailabilityWindowingEnabled = doesGroupUsesTemplates ? catalogGroupCache.IsGeoAvailabilityWindowingEnabled : group.isGeoAvailabilityWindowingEnabled;

                    if (isGeoAvailabilityWindowingEnabled)
                    {
                        List<AssetRule> rules = pair.Value;
                        log.DebugFormat("Starting to do action on {0} asset rules for groupId = {1}", rules.Count, groupId);

                        int maxDegreeOfParallelism = _applicationConfiguration.RecordingsMaxDegreeOfParallelism.Value;
                        if (maxDegreeOfParallelism == 0)
                        {
                            maxDegreeOfParallelism = 5;
                        }

                        ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism };
                        LogContextData contextData = new LogContextData();
                        ConcurrentBag<int> assetIds = new ConcurrentBag<int>();
                        Parallel.ForEach(rules, options, (rule) =>
                        {
                            contextData.Load();
                            assetIds.AddRange(DoActionOnRule(rule, groupId));

                        });

                        if (assetIds?.Count > 0)
                        {
                            List<int> distinctAssetIds = assetIds.Distinct().ToList();
                            int numberOfDistinctAssetIds = distinctAssetIds.Count;
                            assetIds = null;
                            log.DebugFormat("going to update index for {0} assets", numberOfDistinctAssetIds);

                            int bulkSize = 10000;
                            for (int i = 0; i < numberOfDistinctAssetIds; i = i + bulkSize)
                            {
                                var pageMediaIds = distinctAssetIds.Skip(i).Take(bulkSize).ToList();
                                RebuildIndexForMedias(groupId, doesGroupUsesTemplates, pageMediaIds);
                            }

                            List<long> ranRules = allRules.Values.SelectMany(ar => ar).Select(ar => ar.Id).ToList();
                            if (!_repository.UpdateAssetRulesLastRunDate(groupId, ranRules))
                            {
                                log.ErrorFormat("Failed to update asset rule last run date, rule IDs = {0}", string.Join(", ", ranRules));
                            }

                            distinctAssetIds = null;
                            result += numberOfDistinctAssetIds;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in DoActionRules", ex);
            }

            return result;
        }

        public bool UpdateMedia(int groupId, long mediaId, bool checkGeoAvailabilityEnabled = false)
        {
            try
            {
                if (checkGeoAvailabilityEnabled && !IsGeoAssetRulesEnabled(groupId))
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
                        if (_repository.RemoveCountryRulesFromMedia(groupId, mediaId, assetRuleIdsToRemove))
                        {
                            string invalidationKey = LayeredCacheKeys.GetMediaCountriesInvalidationKey(groupId, mediaId);
                            if (!_layeredCache.SetInvalidationKey(invalidationKey))
                            {
                                log.ErrorFormat("Failed to set invalidation key on media countries key = {0}", invalidationKey);
                            }

                            _indexManagerFactory.GetIndexManager(groupId).UpsertMedia(mediaId);
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

        private bool AssetRuleAppliesOnMedia(long mediaId, AssetRule assetRule)
        {
            List<AssetCondition> assetConditions = assetRule.Conditions.Where(c => c.Type == RuleConditionType.Asset).Select(c => c as AssetCondition).ToList();
            string ksqlFilter = null;

            if (assetConditions != null && assetConditions.Count > 0)
            {
                StringBuilder ksql = new StringBuilder("(and ");
                ksql.AppendFormat("media_id = '{0}'", mediaId);
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

        private List<AssetRule> GetMediaRelatedGeoRules(int groupId, long mediaId)
        {
            List<AssetRule> assetRules = new List<AssetRule>();

            List<long> assetRuleIds = _repository.GetGeoAssetRulesAffectingMedia(groupId, mediaId);
            if (assetRuleIds.Count > 0)
            {
                assetRules = _repository.GetAssetRules(assetRuleIds);
            }

            return assetRules;

        }

        private List<int> DoActionOnRule(AssetRule rule, int groupId)
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
                            isAllowed = false;
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
                                    if (_repository.InsertMediaCountry(groupId, assetIds, country, isAllowed, rule.Id))
                                    {
                                        modifiedAssetIds.AddRange(assetIds);
                                        foreach (var assetId in assetIds)
                                        {
                                            string invalidationKey = LayeredCacheKeys.GetMediaCountriesInvalidationKey(groupId, assetId);
                                            if (!_layeredCache.SetInvalidationKey(invalidationKey))
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

        private string BuildKsqlFromConditions(List<AssetCondition> assetConditions)
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

        private IEnumerable<int> GetAllCountriesBut(int groupId, List<int> countryIds)
        {
            List<int> response = new List<int>();
            List<Country> countries = _countryManager.GetCountryListByIds(null, groupId);
            if (countries != null)
            {
                response.AddRange(countries.Select(c => c.Id).Where(c => !countryIds.Contains(c)));
            }
            return response;
        }

        private double CalcTotalOfssetForCountry(int groupId, AssetRuleAction action, int country)
        {
            TimeOffsetRuleAction offsetAction = (TimeOffsetRuleAction)action;
            double totalOffset = offsetAction.Offset;

            if (offsetAction.TimeZone)
            {
                totalOffset -= GetTimeZoneOffsetForCountry(groupId, country);
            }
            return totalOffset;
        }

        private double GetTimeZoneOffsetForCountry(int groupId, int countryId)
        {
            var countries = _countryManager.GetCountryMapById(groupId);
            if (countries == null || countries.Count == 0 || !countries.ContainsKey(countryId))
            {
                log.ErrorFormat("Failed to get countryId = {0}, groupId = {1}", countryId, groupId);
                return 0;
            }

            Country country = countries[countryId];
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

        private Dictionary<int, List<AssetRule>> GetRules(int groupId)
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

        private UnifiedSearchResponse GetMediaUnifiedSearchResponse(int groupId, string ksql)
        {
            // Initialize unified search request:
            // SignString/Signature (basic catalog parameters)
            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = _applicationConfiguration.CatalogSignatureKey.Value;
            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

            // page size should be max_results so it will return everything
            int pageSize = MAX_ASSETS_TO_UPDATE; //ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value;


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

        public int RebuildIndexForMedias(int groupId, bool doesGroupUsesTemplates, List<int> mediaToUpdate)
        {
            if (doesGroupUsesTemplates)
            {
                foreach (var assetId in mediaToUpdate)
                {
                    bool indexingResult = _indexManagerFactory.GetIndexManager(groupId).UpsertMedia(assetId);
                    if (!indexingResult)
                    {
                        log.ErrorFormat("Failed UpsertMedia index for assetId: {0}", assetId);
                    }
                    
                    _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetMediaInvalidationKey(groupId, assetId));

                    _assetManager.InvalidateAsset(eAssetTypes.MEDIA, groupId, assetId);
                }
                return mediaToUpdate.Count;
            }
            else
            {
                if (_catalogIndexManager.UpdateIndex(mediaToUpdate, groupId, eAction.Update))
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

        public GenericListResponse<AssetRule> GetAssetRules(
            RuleConditionType assetRuleConditionType,
            int groupId = 0,
            SlimAsset slimAsset = null,
            RuleActionType? ruleActionType = null,
            string nameContains = null,
            AssetRuleOrderBy orderBy = AssetRuleOrderBy.None)
        {
            GenericListResponse<AssetRule> response = new GenericListResponse<AssetRule>();

            try
            {
                List<AssetRule> allAssetRules = new List<AssetRule>();

                string allAssetRulesKey = LayeredCacheKeys.GetAllAssetRulesKey(
                    groupId,
                    (int)assetRuleConditionType,
                    ruleActionType.HasValue ? (int)ruleActionType.Value : (int?)null,
                    nameContains,
                    orderBy.ToString());
                string allAssetRulesInvalidationKey = groupId != 0 ?
                    LayeredCacheKeys.GetAllAssetRulesGroupInvalidationKey(groupId)
                        : LayeredCacheKeys.GetAllAssetRulesInvalidationKey();

                if (!_layeredCache.Get<List<AssetRule>>(allAssetRulesKey,
                                                        ref allAssetRules,
                                                        GetAllAssetRules,
                                                        new Dictionary<string, object>()
                                                        {
                                                            { "assetRuleConditionType", assetRuleConditionType },
                                                            { "groupId", groupId },
                                                            { "ruleActionType", ruleActionType },
                                                            { "nameContains", nameContains },
                                                            { "orderBy", orderBy }
                                                        },
                                                        groupId,
                                                        LayeredCacheConfigNames.GET_ALL_ASSET_RULES,
                                                        new List<string>() { allAssetRulesInvalidationKey }))
                {
                    log.ErrorFormat("GetAssetRules - GetAllAssetRules - Failed get data from cache. groupId: {0}", groupId);
                    return response;
                }

                response.Objects = allAssetRules;

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

                    if (!_layeredCache.Get<List<AssetRule>>(assetRulesByAssetKey,
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

        internal GenericResponse<AssetRule> AddAssetRule(int groupId, AssetRule assetRuleToAdd)
        {
            GenericResponse<AssetRule> response = new GenericResponse<AssetRule>();

            try
            {
                var validation = _assetRuleActionValidator.Validate(groupId, assetRuleToAdd.Actions);
                if (!validation.IsOkStatusCode())
                {
                    response.SetStatus(validation);
                    return response;
                }

                assetRuleToAdd.GroupId = groupId;
                long id = _repository.AddAssetRule(groupId, assetRuleToAdd, (int)AssetRuleType.AssetRule);
                if (id > 0)
                {
                    assetRuleToAdd.Id = id;
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

        internal Status DeleteAssetRule(int groupId, long assetRuleId)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                //check AssetRule exists
                AssetRule assetRule = _repository.GetAssetRule(assetRuleId);
                if (assetRule == null || assetRule.Id == 0 || groupId != assetRule.GroupId)
                {
                    response.Set((int)eResponseStatus.AssetRuleNotExists, ASSET_RULE_NOT_EXIST);
                    return response;
                }

                if (assetRule.Status == RuleStatus.InProgress)
                {
                    response.Set((int)eResponseStatus.AssetRuleStatusNotWritable, "Cannot update or delete asset rule when in progress");
                }

                if (assetRule.IsConditionExists(RuleConditionType.Country))
                {
                    ResetMediaCountries(groupId, assetRule.Id);
                }

                if (!_repository.DeleteAssetRule(groupId, assetRuleId))
                {
                    response.Set((int)eResponseStatus.Error, ASSET_RULE_FAILED_DELETE);
                    return response;
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

        internal GenericResponse<AssetRule> UpdateAssetRule(int groupId, AssetRule assetRule)
        {
            GenericResponse<AssetRule> response = new GenericResponse<AssetRule>();
            try
            {
                response.SetStatus(_assetRuleActionValidator.Validate(groupId, assetRule.Actions));
                if (!response.Status.IsOkStatusCode())
                {
                    return response;
                }

                assetRule.GroupId = groupId;
                if (assetRule.IsConditionExists(RuleConditionType.Country))
                {
                    HandleRuleUpdateStatus(groupId, ref assetRule);
                }

                if (!_repository.UpdateAssetRule(groupId, assetRule))
                {
                    response.SetStatus(eResponseStatus.Error, ASSET_RULE_FAILED_UPDATE);
                    return response;
                }

                SetInvalidationKeys(groupId, assetRule.Id);
                response.Object = assetRule;
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateAssetRule failed ex={0}, groupId={1}, AssetRuleId={2}", ex, groupId, assetRule.Id);
            }

            return response;
        }

        internal GenericResponse<AssetRule> GetAssetRule(int groupId, long assetRuleId)
        {
            GenericResponse<AssetRule> response = new GenericResponse<AssetRule>();
            try
            {
                AssetRule assetRule = _repository.GetAssetRule(assetRuleId);

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

        public static List<SlimAsset> GetAssetsForValidation(eAssetTypes assetType, int groupId, long assetId)
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

        public static bool HasAssetRules(int groupId, RuleActionType ruleActionType)
        {
            var networkAssetRules = Instance.GetAssetRules(RuleConditionType.Asset, groupId, null, ruleActionType);
            return networkAssetRules.HasObjects();
        }
        
        public static Status CheckNetworkRules(List<SlimAsset> assetsToCheck, int groupId, string ip, out AssetRule blockingRule)
        {
            blockingRule = null;

            if (assetsToCheck == null || assetsToCheck.Count == 0)
            {
                return new Status((int)eResponseStatus.OK);
            }

            APILogic.Utils.ConvertIpToNumber(ip, out var convertedIp, out var isV6);
            Dictionary<string, string> headers = ListUtils.ToDictionary(System.Web.HttpContext.Current.Request.GetHeaders());

            var scope = new NetworkConditionScope()
            {
                Headers = headers
            };

            if (isV6)
            {
                scope.IpV6 = ip;
            }
            else
            {
                scope.Ip = long.Parse(convertedIp);
            }

            log.DebugFormat("CheckNetworkRules - scope: {0}.", scope.ToString());

            foreach (var asset in assetsToCheck)
            {
                var networkAssetRules = Instance.GetAssetRules(RuleConditionType.Asset, groupId, asset, RuleActionType.Block);
                if (networkAssetRules.HasObjects())
                {
                    log.DebugFormat("CheckNetworkRules - there are {0} valid networkAssetRules for groupId {1} and for asset id {2} + type {3}.",
                                    networkAssetRules.Objects.Count, groupId, asset.Id, asset.Type);
                    foreach (var networkRule in networkAssetRules.Objects)
                    {
                        foreach (var condition in networkRule.Conditions.Where(x => x.Type == RuleConditionType.Header || x.Type == RuleConditionType.Or))
                        {
                            if (scope.Evaluate(condition))
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
            if (CatalogManager.Instance.DoesGroupUsesTemplates(groupId))
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    return false;
                }

                return catalogGroupCache.IsGeoAvailabilityWindowingEnabled;
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

                return group.isGeoAvailabilityWindowingEnabled;
            }
        }

        #endregion

        #region Private Methods

        private void SetInvalidationKeys(int groupId, long? assetRuleId = null)
        {
            _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetAllAssetRulesInvalidationKey());
            _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetAllAssetRulesGroupInvalidationKey(groupId));

            if (assetRuleId.HasValue)
            {
                _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetAssetRuleInvalidationKey(groupId, assetRuleId.Value));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcParams">assetRuleConditionType, groupId</param>
        /// <returns>true if assetRules is not null, otherwise false</returns>
        private Tuple<List<AssetRule>, bool> GetAllAssetRules(Dictionary<string, object> funcParams)
        {
            List<AssetRule> allAssetRules = new List<AssetRule>();

            try
            {
                if (funcParams != null)
                {
                    if (funcParams.ContainsKey("assetRuleConditionType")
                        && funcParams.ContainsKey("groupId")
                        && funcParams.ContainsKey("ruleActionType")
                        && funcParams.ContainsKey("nameContains")
                        && funcParams.ContainsKey("orderBy"))
                    {
                        RuleConditionType? assetRuleConditionType = funcParams["assetRuleConditionType"] as RuleConditionType?;
                        int? groupId = funcParams["groupId"] as int?;
                        RuleActionType? ruleActionType = funcParams["ruleActionType"] as RuleActionType?;
                        var nameContains = funcParams["nameContains"] as string;
                        var orderBy = funcParams["orderBy"] as AssetRuleOrderBy?;

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
                            else if (allAssetRulesDB.Any())
                            {
                                if (groupId > 0)
                                {
                                    allAssetRulesDB = allAssetRulesDB.Where(x => x.GroupId == groupId).ToList();
                                }

                                allAssetRules = _repository.GetAssetRules(allAssetRulesDB.Select(x => x.Id));
                                if (allAssetRules != null && allAssetRules.Any())
                                {
                                    var result = allAssetRules.AsEnumerable();
                                    if (assetRuleConditionType.HasValue)
                                    {
                                        result = result.Where(ar => ar.IsConditionExists(assetRuleConditionType.Value));
                                    }

                                    if (ruleActionType.HasValue)
                                    {
                                        result = result.Where(ar => ar.IsActionExists(ruleActionType.Value));
                                    }

                                    if (!string.IsNullOrEmpty(nameContains))
                                    {
                                        result = result
                                            .Where(x => x.Name.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) >= 0);
                                    }

                                    if (orderBy.Value != AssetRuleOrderBy.None)
                                    {
                                        result = orderBy.Value == AssetRuleOrderBy.NameAsc
                                            ? result.OrderBy(x => x.Name)
                                            : result.OrderByDescending(x => x.Name);
                                    }

                                    allAssetRules = result.ToList();
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
                log.Error(string.Format("GetAllAssetRules failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<AssetRule>, bool>(allAssetRules, allAssetRules != null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcParams">allAssetRules, slimAsset, groupId</param>
        /// <returns>true if assetRulesByAsset is not null, otherwise false</returns>
        private Tuple<List<AssetRule>, bool> GetAssetRulesByAsset(Dictionary<string, object> funcParams)
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

                            int maxDegreeOfParallelism = ApplicationConfiguration.Current.RecordingsMaxDegreeOfParallelism.Value;

                            if (maxDegreeOfParallelism == 0)
                            {
                                maxDegreeOfParallelism = 5;
                            }

                            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism };
                            LogContextData contextData = new LogContextData();
                            ConcurrentBag<AssetRule> assetRules = new ConcurrentBag<AssetRule>();

                            Parallel.ForEach(assetRulesWithKsql, options, (currAssetRuleWithKsql) =>
                            {
                                contextData.Load();
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
                                    assetRules.Add(currAssetRuleWithKsql);
                                }
                            });

                            if (assetRules?.Count > 0)
                            {
                                assetRulesByAsset = assetRules.ToList();
                                assetRules = null;
                            }

                            log.Debug("GetAssetRulesByAsset - success");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                assetRulesByAsset = null;
                log.Error(string.Format("GetAssetRulesByAsset failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<AssetRule>, bool>(assetRulesByAsset, assetRulesByAsset != null);
        }

        /// <summary>
        /// GetAllAssetRules from DB with no filter
        /// </summary>
        /// <param name="funcParams">is null</param>
        /// <returns>true if assetRules is not null, otherwise false</returns>
        private Tuple<List<AssetRule>, bool> GetAllAssetRulesDB(Dictionary<string, object> funcParams)
        {
            List<AssetRule> assetRules = null;

            try
            {
                assetRules = _repository.GetAllAssetRules();
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

                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetMediaCountriesInvalidationKey(groupId, mediaId));
                }

                if (Catalog.Module.Instance.UpdateIndex(mediaIds, groupId, eAction.Update))
                {
                    log.InfoFormat("Successfully updated index after asset rule update for assets: {0}", string.Join(",", mediaIds));
                }
                else
                {
                    log.InfoFormat("Failed to update index after asset rule update for assets", string.Join(",", mediaIds));
                }
            }
        }

        private HashSet<int> GetRuleCountriesList(int groupId, AssetRule rule)
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

                foreach (int item in countries)
                {
                    if (!result.Contains(item))
                        result.Add(item);
                }
            }

            return result;
        }

        public bool HandleRuleUpdate(int groupId, long assetRuleId, List<int> countriesToRemove, bool removeBlocked, bool removeAllowed, bool updateKsql)
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

            var indexManager = IndexManagerFactory.Instance.GetIndexManager(groupId);
            foreach (var mediaId in updatedMediaIds)
            {
                var success = indexManager.UpsertMedia(mediaId);
                _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetMediaInvalidationKey(groupId, mediaId));
            }

            assetRule.Status = RuleStatus.Ready;
            if (!_repository.UpdateAssetRule(groupId, assetRule))
            {
                log.ErrorFormat("Error while saving AssetRule. groupId: {0}, assetRuleId:{1}", groupId, assetRule.Id);
            }
            else
            {
                SetInvalidationKeys(groupId, assetRule.Id);
            }

            return true;
        }

        private void HandleRuleUpdateStatus(int groupId, ref AssetRule assetRule)
        {
            //get old rule
            var oldRuleResopnse = GetAssetRule(groupId, assetRule.Id);
            if (oldRuleResopnse == null || !oldRuleResopnse.HasObject())
            {
                return;
            }

            bool removeBlocked = false;
            bool removeAllowed = false;
            bool updateKsql = false;

            var oldRule = oldRuleResopnse.Object;

            //1. countries
            HashSet<int> oldCountries = GetRuleCountriesList(groupId, oldRule);
            HashSet<int> newCountries = GetRuleCountriesList(groupId, assetRule);

            List<int> countriesToRemove = oldCountries.Where(x => !newCountries.Contains(x)).ToList();

            //2. Block or EndtDate
            var oldAssetActionBlock = oldRule.Actions.FirstOrDefault(c => c.Type == RuleActionType.Block);
            var newAssetActionBlock = assetRule.Actions.FirstOrDefault(c => c.Type == RuleActionType.Block);
            TimeOffsetRuleAction oldAssetActionEndDate = oldRule.Actions.FirstOrDefault(c => c.Type == RuleActionType.EndDateOffset) as TimeOffsetRuleAction;
            TimeOffsetRuleAction newAssetActionEndDate = assetRule.Actions.FirstOrDefault(c => c.Type == RuleActionType.EndDateOffset) as TimeOffsetRuleAction;

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
                var queue = new GenericCeleryQueue();
                var data = new ApiObjects.QueueObjects.GeoRuleUpdateData(groupId, assetRule.Id, countriesToRemove, removeBlocked, removeAllowed, updateKsql)
                { ETA = DateTime.UtcNow };
                bool queueGeoRuleUpdateResult = queue.Enqueue(data, string.Format(ROUTING_KEY_GEO_RULE_UPDATE, groupId));
                if (!queueGeoRuleUpdateResult)
                {
                    log.ErrorFormat("Failed to queue GeoRuleUpdateData, assetRuleId: {0}, groupId: {1}", assetRule.Id, groupId);
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
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
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