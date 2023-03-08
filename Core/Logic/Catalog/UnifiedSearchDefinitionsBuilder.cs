using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CachingHelpers;
using Core.Catalog.Request;
using Core.Catalog.Cache;
using GroupsCacheManager;
using TVinciShared;
using ApiObjects.Response;
using Phx.Lib.Log;
using System.Reflection;
using ApiLogic.Api.Managers.Rule;
using ApiLogic.Catalog;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiLogic.Catalog.Tree;
using ApiLogic.IndexManager.QueryBuilders.SearchPriority;
using ApiLogic.IndexManager.Sorting;
using ApiObjects;
using ApiObjects.Rules;
using Phx.Lib.Appconfig;
using Core.Api.Managers;
using Core.Catalog.CatalogManagement;
using ElasticSearch.Utils;
using OrderDir = ApiObjects.SearchObjects.OrderDir;
using Core.GroupManagers;

namespace Core.Catalog
{
    public class UnifiedSearchDefinitionsBuilder
    {
        private readonly IFilterTreeValidator _filterTreeValidator;
        private readonly IAssetOrderingService _assetOrderingService;

        private static readonly KLogger log = new KLogger(nameof(UnifiedSearchDefinitionsBuilder));
        private bool shouldUseCache = false;

        #region Ctor

        public UnifiedSearchDefinitionsBuilder(IFilterTreeValidator filterTreeValidator, IAssetOrderingService assetOrderingService)
        {
            _filterTreeValidator = filterTreeValidator;
            _assetOrderingService = assetOrderingService ?? throw new ArgumentNullException(nameof(assetOrderingService));
            shouldUseCache = ApplicationConfiguration.Current.CatalogLogicConfiguration.ShouldUseSearchCache.Value;
        }

        #endregion

        #region Public Methods

        public UnifiedSearchDefinitions GetDefinitions(UnifiedSearchRequest request)
        {
            UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions();

            try
            {
                SetLanguageDefinition(request.m_nGroupID, request.m_oFilter, definitions);

                int parentGroupID = request.m_nGroupID;
                GroupManager groupManager = null;
                Group group = null;
                CatalogGroupCache catalogGroupCache = null;
                bool doesGroupUsesTemplates = CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(parentGroupID);
                if (doesGroupUsesTemplates && !CatalogManagement.CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(parentGroupID, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetDefinitions", parentGroupID);
                    return definitions;
                }

                definitions.shouldIgnoreDeviceRuleID = request.shouldIgnoreDeviceRuleID; // BEO-6731

                parentGroupID = CatalogCache.Instance().GetParentGroup(request.m_nGroupID);
                groupManager = new GroupManager();
                group = groupManager.GetGroup(parentGroupID);
                if (!doesGroupUsesTemplates && request.isAllowedToViewInactiveAssets && !request.isInternalSearch)
                {
                    definitions.shouldIgnoreDeviceRuleID = true;
                    request.isAllowedToViewInactiveAssets = false;
                }

                definitions.shouldUseSearchEndDate = request.GetShouldUseSearchEndDate() && !request.isAllowedToViewInactiveAssets;
                definitions.shouldDateSearchesApplyToAllTypes = request.shouldDateSearchesApplyToAllTypes || request.isAllowedToViewInactiveAssets;
                definitions.isInternalSearch = request.isInternalSearch || request.isAllowedToViewInactiveAssets;

                definitions.isAllowedToViewInactiveAssets = request.isAllowedToViewInactiveAssets;
                if (definitions.isAllowedToViewInactiveAssets)
                {
                    definitions.shouldAddIsActiveTerm = false;
                    definitions.shouldIgnoreDeviceRuleID = true;
                }

                if (request.filterTree != null)
                {
                    CatalogLogic.UpdateNodeTreeFields(request, ref request.filterTree, definitions, group, parentGroupID);
                }
                
                #region Priority Groups

                definitions.PriorityGroupsMappings = PriorityGroupsPreprocessor.Instance.Preprocess(request.PriorityGroupsMappings, request, definitions, group, parentGroupID);

                #endregion

                // Get days offset for EPG search from TCM
                definitions.epgDaysOffest = ApplicationConfiguration.Current.CatalogLogicConfiguration.CurrentRequestDaysOffset.Value;

                #region Filter & Order

                if (request.m_oFilter != null)
                {
                    definitions.shouldUseStartDateForMedia = request.m_oFilter.m_bUseStartDate;
                    definitions.shouldUseFinalEndDate = request.m_oFilter.m_bUseFinalDate;
                    definitions.userTypeID = request.m_oFilter.m_nUserTypeID;
                    if (!definitions.shouldIgnoreDeviceRuleID)
                    {
                        definitions.deviceRuleId = Api.api.GetDeviceAllowedRuleIDs(request.m_nGroupID, request.m_oFilter.m_sDeviceId, request.domainId).ToArray();
                    }
                }

                // in case operator is searching we override the existing value
                definitions.shouldUseStartDateForMedia = !request.isAllowedToViewInactiveAssets;
                definitions.shouldUseCatalogStartDateForMedia = doesGroupUsesTemplates;
                definitions.shouldIgnoreEndDate = request.isAllowedToViewInactiveAssets || request.shouldIgnoreEndDate;

                definitions.groupId = request.m_nGroupID;
                definitions.permittedWatchRules = CatalogLogic.GetPermittedWatchRules(request.m_nGroupID);
                definitions.filterPhrase = request.filterTree;
                definitions.exactGroupId = request.exactGroupId;

                #endregion

                #region Asset Types

                // Special case - if no type was specified or "All" is contained, search all types
                if (request.assetTypes == null || request.assetTypes.Count == 0)
                {
                    definitions.shouldSearchEpg = true;
                    definitions.shouldSearchMedia = true;
                    definitions.shouldSearchRecordings = false;
                }
                else
                {
                    definitions.mediaTypes = new List<int>(request.assetTypes);
                }

                if (definitions.ksqlAssetTypes?.Count > 0)
                {
                    definitions.shouldSearchEpg = false;
                }

                // 0 or 'epg' - hard coded for EPG
                if (definitions.mediaTypes.Remove(UnifiedSearchDefinitions.EPG_ASSET_TYPE) || 
                    definitions.ksqlAssetTypes.Contains("epg") || 
                    definitions.ksqlAssetTypes.Contains(UnifiedSearchDefinitions.EPG_ASSET_TYPE.ToString()))
                {
                    definitions.shouldSearchEpg = true;
                }

                if (definitions.ksqlAssetTypes.Contains("recording"))
                {
                    definitions.shouldSearchRecordings = true;
                }

                // 1 - hard coded for recording
                if (definitions.mediaTypes.Remove(UnifiedSearchDefinitions.RECORDING_ASSET_TYPE))
                {
                    definitions.shouldSearchRecordings = true;
                }

                // If there are items left in media types after removing 0, we are searching for media
                if (definitions.mediaTypes.Count > 0)
                {
                    definitions.shouldSearchMedia = true;
                }
                
                if (definitions.hasMediaIdTerm && !definitions.hasOrNode)
                {
                    definitions.shouldSearchEpg = false;
                }

                var result = _filterTreeValidator.ValidateTree(request.filterTree, request.assetTypes);
                if (result != null)
                {
                    definitions.shouldSearchEpg = result.ShouldSearchEpg;
                    definitions.shouldSearchMedia = result.ShouldSearchMedia;
                }

                HashSet<int> mediaTypes = null;
                if (doesGroupUsesTemplates)
                {
                    definitions.ObjectVirtualAssetIds = catalogGroupCache.GetObjectVirtualAssetIds();
                    
                    mediaTypes = new HashSet<int>(catalogGroupCache.AssetStructsMapById.Keys.Select(x => (int)x));
                }
                else
                {
                    mediaTypes = new HashSet<int>(group.GetMediaTypes());
                }

                if (mediaTypes != null)
                {
                    // Validate that the media types in the "assetTypes" list exist in the group's list of media types
                    foreach (var mediaType in definitions.mediaTypes)
                    {
                        // If one of them doesn't exist, throw an exception that says the request is bad
                        if (!mediaTypes.Contains(mediaType))
                        {
                            throw new KalturaException(string.Format("Invalid media type was sent: {0}", mediaType), (int)eResponseStatus.BadSearchRequest);
                        }
                    }
                }

                #endregion

                #region Regions

                if (!definitions.isAllowedToViewInactiveAssets)
                {
                    List<int> regionIds;
                    List<string> linearMediaTypes;

                    CatalogLogic.SetSearchRegions(request.m_nGroupID, doesGroupUsesTemplates, request.domainId, request.m_sSiteGuid, out regionIds, out linearMediaTypes);

                    definitions.regionIds = regionIds;
                    definitions.linearChannelMediaTypes = linearMediaTypes;
                }

                #endregion

                CatalogLogic.GetParentMediaTypesAssociations(request.m_nGroupID,
                    out definitions.parentMediaTypes, out definitions.associationTags,
                    definitions.mediaTypes, definitions.mediaTypes.Count == 0, groupManager);

                #region Search by entitlement

                if (definitions.entitlementSearchDefinitions != null)
                {
                    eEntitlementSearchType type = eEntitlementSearchType.None;

                    if (definitions.entitlementSearchDefinitions.shouldGetFreeAssets)
                    {
                        if (definitions.entitlementSearchDefinitions.shouldGetPurchasedAssets)
                        {
                            type = eEntitlementSearchType.Both;
                        }
                        else
                        {
                            type = eEntitlementSearchType.Free;
                        }
                    }
                    else if (definitions.entitlementSearchDefinitions.shouldGetPurchasedAssets)
                    {
                        type = definitions.entitlementSearchDefinitions.shouldGetOnlySubscriptionAssets 
                            ? eEntitlementSearchType.Subscriptions 
                            : eEntitlementSearchType.Entitled;
                    }

                    if (type != eEntitlementSearchType.None)
                    {
                        if (shouldUseCache)
                        {
                            definitions.entitlementSearchDefinitions =
                                EntitlementDefinitionsCache.Instance().GetEntitlementSearchDefinitions(definitions, request, parentGroupID, group, type);
                        }
                        else
                        {
                            BuildEntitlementSearchDefinitions(definitions, request, parentGroupID, group);
                        }
                    }
                }

                #endregion

                #region Specific Assets

                if (request.specificAssets != null)
                {
                    // If we don't have dictionary yet - create it
                    if (definitions.specificAssets == null)
                    {
                        definitions.specificAssets = new Dictionary<ApiObjects.eAssetTypes, List<string>>();
                    }

                    foreach (var specificAsset in request.specificAssets)
                    {
                        // If we don't have the list of the current asset type - create it
                        if (!definitions.specificAssets.ContainsKey(specificAsset.Key))
                        {
                            definitions.specificAssets[specificAsset.Key] = new List<string>();
                        }

                        // Add asset from request to definitions
                        definitions.specificAssets[specificAsset.Key].Add(specificAsset.Value.ToString());
                    }
                }

                definitions.GroupByOption = request.GroupByOption
                    ?? (request.searchGroupBy?.isGroupingOptionInclude == true || request.isGroupingOptionInclude
                        ? GroupingOption.Include
                        : GroupingOption.Omit);
                #endregion

                #region Get Recordings

                // we should go to CAS if we DON'T have predefined recordings
                definitions.shouldGetDomainsRecordings = !request.hasPredefinedRecordings;

                if (definitions.shouldSearchRecordings)
                {
                    definitions.extraReturnFields.Add("epg_id");

                    if (definitions.shouldGetDomainsRecordings && request.domainId > 0)
                    {
                        if (definitions.specificAssets == null)
                        {
                            definitions.specificAssets = new Dictionary<eAssetTypes, List<string>>();
                        }

                        HashSet<long> specificRecordingIds = new HashSet<long>();
                        List<string> recordingIds = new List<string>();
                        /* If there are previous specific assets - we need to map the list of domain recording ids
                         * to recording ids  and we narrow down the list to contain only the user's recordings */
                        if (request.domainId > 0 && definitions.specificAssets.ContainsKey(eAssetTypes.NPVR))
                        {
                            specificRecordingIds = new HashSet<long>(definitions.specificAssets[eAssetTypes.NPVR].Select(x => long.Parse(x)));
                        }

                        recordingIds = GetDomainRecordings(definitions, request.m_nGroupID, (long)request.domainId, specificRecordingIds);
                        
                        // If domain has at least one recording
                        if (recordingIds != null && recordingIds.Count > 0)
                        {
                            definitions.specificAssets[eAssetTypes.NPVR] = recordingIds;
                        }
                        // if not, create a new list which symbols no assets at all.
                        else
                        {                           
                            definitions.specificAssets[eAssetTypes.NPVR] = new List<string>() { "0" };
                        }
                    }
                }
                #endregion

                #region Excluded CRIDs

                if (request.excludedCrids != null && request.excludedCrids.Count > 0)
                {
                    definitions.excludedCrids = request.excludedCrids;
                }

                #endregion

                #region Extended search request

                List<string> extraReturnFields = request.GetExtraReturnFields();

                if (extraReturnFields != null && extraReturnFields.Count > 0)
                {
                    foreach (var field in extraReturnFields)
                    {
                        definitions.extraReturnFields.Add(field.ToLower());
                    }

                    definitions.shouldReturnExtendedSearchResult = true;
                }

                definitions.EpgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(request.m_nGroupID);

                #endregion

                #region Group By

                Utils.BuildSearchGroupBy(request.searchGroupBy, group, definitions, request.m_nGroupID);

                #endregion

                #region User Preferences

                if (definitions.shouldGetUserPreferences)
                {
                    definitions.userPreferences = CatalogLogic.GetUserPreferences(request.m_nGroupID, Convert.ToInt32(request.m_sSiteGuid));
                }

                #endregion

                #region Geo Availability

                if (!definitions.isAllowedToViewInactiveAssets && !definitions.isInternalSearch && 
                    (doesGroupUsesTemplates ? catalogGroupCache.IsGeoAvailabilityWindowingEnabled : group.isGeoAvailabilityWindowingEnabled))
                {
                    definitions.countryId = Utils.GetIP2CountryId(request.m_nGroupID, request.m_sUserIP);
                }

                #endregion

                #region Asset User Rule

                var shopUserId = request.GetCallerUserId();
                if (shopUserId > 0)
                {
                    GetUserAssetRulesPhrase(request, group, ref definitions, request.m_nGroupID, shopUserId);
                }

                #endregion

                #region Preference

                if (!string.IsNullOrEmpty(request.m_sSiteGuid))
                {
                    definitions.preference = request.m_sSiteGuid;
                }
                else if (!string.IsNullOrEmpty(request.m_sUserIP))
                {
                    definitions.preference = request.m_sUserIP.Replace(".", string.Empty);
                }
                else
                {
                    definitions.preference = "BeInternal";
                }

                #endregion

                #region Order
                // Ordering should go at the end as it depends on other definitions properties.
                var model = new AssetListEsOrderingCommonInput
                {
                    GroupId = definitions.groupId,
                    ShouldSearchEpg = definitions.shouldSearchEpg,
                    ShouldSearchMedia = definitions.shouldSearchRecordings,
                    ShouldSearchRecordings = definitions.shouldSearchRecordings,
                    AssociationTags = definitions.associationTags,
                    ParentMediaTypes = definitions.parentMediaTypes,
                    Language = definitions.langauge
                };

                var orderingResult = _assetOrderingService.MapToEsOrderByFields(
                    request.order,
                    request.orderingParameters,
                    model);

                definitions.orderByFields = orderingResult.EsOrderByFields;

                // Still need these assignments as IndexManagerV7 doesn't support secondary sorting.
                definitions.order = orderingResult.Order;

                #endregion
            }
            catch(KalturaException ex)
            {
                log.WarnFormat("Failed building unified search definitions. ex = {0}, st = {1}", ex, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("failed building unified search definitions. ex = {0}, st = {1}", ex, ex.StackTrace);
                throw ex;
            }

            return definitions;
        }

        internal static void GetUserAssetRulesPhrase(BaseRequest request, Group group, ref UnifiedSearchDefinitions definitions, int groupId, long userId)
        {
            definitions.assetUserBlockRulePhrase = GetUserAssetRulesPhrase(request, group, ref definitions, groupId, RuleActionType.UserBlock, userId);

            BooleanPhraseNode userPhraseNode = GetUserAssetRulesPhrase(request, group, ref definitions, groupId, RuleActionType.UserFilter, userId);
            definitions.assetUserRuleFilterPhrase = UnionBooleanPhraseNode(definitions.assetUserRuleFilterPhrase, userPhraseNode);          
        }       

        private static BooleanPhraseNode UnionBooleanPhraseNode(BooleanPhraseNode channelPhraseNode, BooleanPhraseNode userPhraseNode)
        {
            BooleanPhraseNode newPhrase = null;

            if (channelPhraseNode != null)
            {
                if (userPhraseNode != null)
                {
                    List<BooleanPhraseNode> nodes = new List<BooleanPhraseNode>()
                    {
                        userPhraseNode,
                        channelPhraseNode
                    };
                    newPhrase = new BooleanPhrase(nodes, eCutType.And);
                }
                else
                {
                    newPhrase = channelPhraseNode;
                }
            }
            else if (userPhraseNode != null)
            {
                newPhrase = userPhraseNode;
            }

            return newPhrase;
        }

        internal static void GetChannelUserAssetRulesPhrase(BaseRequest request, Group group, ref UnifiedSearchDefinitions definitions, int groupId, long ruleId)
        {
            BooleanPhraseNode phrase = null;
            var assetUserRulesResponse = AssetUserRuleManager.GetAssetUserRuleByRuleId(request.m_nGroupID, ruleId);
            if (assetUserRulesResponse.Status.Code == (int)eResponseStatus.OK)
            {
                if (assetUserRulesResponse.Object != null)
                {
                    // check if rule applay on channel 
                    ApiObjects.Rules.AssetUserRuleFilterAction assetUserRuleFilterAction = assetUserRulesResponse.Object.Actions[0] as ApiObjects.Rules.AssetUserRuleFilterAction;
                    if (assetUserRuleFilterAction != null && assetUserRuleFilterAction.ApplyOnChannel)
                    {
                        StringBuilder query = new StringBuilder();
                        query.Append("(or ");
                        definitions.assetUserRuleIds.Add(assetUserRulesResponse.Object.Id);
                        foreach (var assetCondition in assetUserRulesResponse.Object.Conditions)
                        {
                            var ksql = AssetConditionKsqlFactory.Instance.GetKsql(groupId, assetCondition);
                            query.AppendFormat(" {0}", ksql);
                        }

                        query.Append(")");

                        string queryString = query.ToString();

                        BooleanPhrase.ParseSearchExpression(queryString, ref phrase);

                        CatalogLogic.UpdateNodeTreeFields(request, ref phrase, definitions, group, groupId);

                        definitions.assetUserRuleFilterPhrase = phrase;
                    }
                }
            }
            else
            {
                log.ErrorFormat("Failed to get asset user rule {0}, code = {1}", ruleId, assetUserRulesResponse.Status.Code);
            }            
        }

        private static BooleanPhraseNode GetUserAssetRulesPhrase(BaseRequest request, Group group, ref UnifiedSearchDefinitions definitions, int groupId, 
            RuleActionType ruleActionType, long userId)
        {
            BooleanPhraseNode phrase = null;
            var assetUserRulesResponse = AssetUserRuleManager.GetAssetUserRuleList(request.m_nGroupID, userId, true, ruleActionType);
            if (assetUserRulesResponse.IsOkStatusCode())
            {
                if (assetUserRulesResponse.HasObjects())
                {
                    var rulesList = assetUserRulesResponse.Objects;
                    List<long> rulesIds;
                    string queryString;
                    GetQueryStringFromAssetUserRules(groupId, rulesList, out rulesIds, out queryString);

                    definitions.assetUserRuleIds.AddRange(rulesIds);
                    BooleanPhrase.ParseSearchExpression(queryString, ref phrase);
                    CatalogLogic.UpdateNodeTreeFields(request, ref phrase, definitions, group, groupId);

                    return phrase;
                }
            }
            else
            {
                log.ErrorFormat("Failed to get asset user rules for userId = {0}, ruleAction:{1}, code = {2}", userId, ruleActionType.ToString(), assetUserRulesResponse.Status.Code);
            }

            return phrase;
        }

        public static void GetQueryStringFromAssetUserRules(long groupId, List<ApiObjects.Rules.AssetUserRule> rulesList, out List<long> rulesIds, out string queryString)
        {
            rulesIds = new List<long>();
            StringBuilder query = new StringBuilder();
            query.Append("(or ");
            foreach (var rule in rulesList)
            {
                rulesIds.Add(rule.Id);
                foreach (var assetCondition in rule.Conditions)
                {
                    var ksql = AssetConditionKsqlFactory.Instance.GetKsql(groupId, assetCondition);
                    query.AppendFormat(" {0}", ksql);
                }
            }

            query.Append(")");

            queryString = query.ToString();
        }

        private List<string> GetDomainRecordings(UnifiedSearchDefinitions definitions, int groupId, long domainId, HashSet<long> specificRecordingIds)
        {
            List<string> result = new List<string>();
            definitions.domainRecordingIdToRecordingIdMapping = new Dictionary<string, string>();
            ApiObjects.TimeShiftedTv.SearchableRecording[] domainSearchableRecordings = ConditionalAccess.Module.GetDomainSearchableRecordings(groupId, domainId);

            if (domainSearchableRecordings == null)
            {
                log.ErrorFormat("GetDomainSearchableRecordings returned invalid response, domainId : {0}, groupId: {1}", domainId, groupId);
                return result;
            }

            foreach (ApiObjects.TimeShiftedTv.SearchableRecording recording in domainSearchableRecordings)
            {
                string key = recording.RecordingId.ToString();

                if (!definitions.recordingIdToSearchableRecordingMapping.ContainsKey(key)
                    && (specificRecordingIds.Count == 0 || specificRecordingIds.Contains(recording.RecordingId)))
                {
                    definitions.recordingIdToSearchableRecordingMapping.Add(key, recording);
                    result.Add(key);
                }
            }

            if (definitions.recordingIdToSearchableRecordingMapping?.Count > 0)
            {
                definitions.domainRecordingIdToRecordingIdMapping = definitions.recordingIdToSearchableRecordingMapping.ToDictionary(x => x.Value.DomainRecordingId, x => x.Key);
            }

            return result;
        }

        public static void BuildEntitlementSearchDefinitions(
            UnifiedSearchDefinitions definitions,
            BaseRequest request,
            int parentGroupID,
            Group group)
        {
            int[] fileTypes = null;
            CatalogGroupCache catalogGroupCache = null;
            bool doesGroupUsesTemplates = CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(parentGroupID);
            List<int> linearChannelMediaTypes = new List<int>();
            if (doesGroupUsesTemplates)
            {
                if (!CatalogManagement.CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(parentGroupID, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling BuildEntitlementSearchDefinitions", parentGroupID);
                    return;
                }
                else if (catalogGroupCache.AssetStructsMapBySystemName.ContainsKey(CatalogManagement.CatalogManager.LINEAR_ASSET_STRUCT_SYSTEM_NAME))
                {
                    linearChannelMediaTypes.Add((int)catalogGroupCache.AssetStructsMapBySystemName[CatalogManagement.CatalogManager.LINEAR_ASSET_STRUCT_SYSTEM_NAME].Id);
                }
            }
            else if (group != null)
            {
                linearChannelMediaTypes.AddRange(group.linearChannelMediaTypes);
            }

            if (request.m_oFilter != null)
            {
                fileTypes = request.m_oFilter.fileTypes;
            }

            var entitlementSearchDefinitions = definitions.entitlementSearchDefinitions;

            List<int> epgChannelIds = new List<int>();
            List<int> freeEpgChannelIds = null;
            List<int> purchasedEpgChannelIds = null;

            if (entitlementSearchDefinitions.shouldGetFreeAssets)
            {
                entitlementSearchDefinitions.freeAssets =
                    EntitledAssetsUtils.GetFreeAssets(parentGroupID, request.m_sSiteGuid, out freeEpgChannelIds);
            }

            if (entitlementSearchDefinitions.shouldGetPurchasedAssets && !entitlementSearchDefinitions.shouldGetOnlySubscriptionAssets)
            {
                entitlementSearchDefinitions.entitledPaidForAssets =
                   EntitledAssetsUtils.GetUserPPVAssets(parentGroupID, request.m_sSiteGuid, request.domainId, fileTypes, out purchasedEpgChannelIds);
            }

            if (freeEpgChannelIds != null)
            {
                epgChannelIds.AddRange(freeEpgChannelIds);
            }

            if (purchasedEpgChannelIds != null)
            {
                epgChannelIds.AddRange(purchasedEpgChannelIds);
            }

            string[] entitlementMediaTypes = null;

            // If there are no specific media types, the utility method still needs a "0" to indicate "all"
            if (definitions.shouldSearchMedia && definitions.mediaTypes.Count == 0)
            {
                entitlementMediaTypes = new string[] { "0" };
            }
            else
            {
                // If there are specific media types, use them
                var selectedMediaTypes = definitions.mediaTypes.Select(t => t.ToString());

                // Also add linear channel media type so that we get them in the next search
                if (definitions.shouldSearchEpg)
                {
                    selectedMediaTypes = selectedMediaTypes.Union(linearChannelMediaTypes.Select(t => t.ToString()));
                }

                entitlementMediaTypes = selectedMediaTypes.ToArray();
            }

            if (entitlementSearchDefinitions.shouldGetPurchasedAssets)
            {
                entitlementSearchDefinitions.subscriptionSearchObjects =
                    EntitledAssetsUtils.GetUserBundlePurchasedSearchObjects(request, parentGroupID, request.m_sSiteGuid, request.domainId, fileTypes, entitlementMediaTypes, definitions.deviceRuleId, null, entitlementSearchDefinitions.shouldGetOnlySubscriptionAssets);
            }

            if (entitlementSearchDefinitions.shouldGetFreeAssets)
            {
                entitlementSearchDefinitions.fileTypes = new List<int>();
                if (doesGroupUsesTemplates)
                {
                    GenericListResponse<MediaFileType> mediaFileTypesResponse = CatalogManagement.FileManager.Instance.GetMediaFileTypes(parentGroupID);
                    if (mediaFileTypesResponse != null && mediaFileTypesResponse.Status != null && mediaFileTypesResponse.Status.Code == (int)eResponseStatus.OK
                        && mediaFileTypesResponse.Objects != null && mediaFileTypesResponse.Objects.Count > 0)
                    {
                        entitlementSearchDefinitions.fileTypes.AddRange(mediaFileTypesResponse.Objects.Select(x => (int)x.Id));
                    }
                }
                else if (group.groupMediaFileTypeToFileType != null && entitlementSearchDefinitions.shouldGetFreeAssets)
                {
                    // Convert the file type that we received in request (taken from groups_media_type)
                    // into the file type that the media file knows (based on the table media_files)                    

                    if (fileTypes != null)
                    {
                        foreach (var fileType in fileTypes)
                        {
                            entitlementSearchDefinitions.fileTypes.Add(group.groupMediaFileTypeToFileType[fileType]);
                        }
                    }
                }
            }

            // TODO: Maybe this will be the method that gets the FREE epg channel IDs
            var entitledChannelIds =
                EntitledAssetsUtils.GetUserEntitledEpgChannelIds(
                    parentGroupID,
                    request.m_sSiteGuid,
                    definitions,
                    linearChannelMediaTypes,
                    doesGroupUsesTemplates);

            epgChannelIds.AddRange(entitledChannelIds);

            entitlementSearchDefinitions.epgChannelIds = epgChannelIds;

            /* Not sure if we need this if I add is_free member to ES index
            // edge case - user is not entitled to anything!
            if ((definitions.freeAssets == null || definitions.freeAssets.Count == 0) &&
                (definitions.entitledPaidForAssets == null || definitions.entitledPaidForAssets.Count == 0) &&
                definitions.subscriptionSearchObjects.Count == 0)
            {
                // Make sure that all lists in dictionaries are empty
                bool entitledToAnything = 
                    definitions.freeAssets.Values.Any(item => item.Count > 0) || definitions.entitledPaidForAssets.Values.Any(item => item.Count > 0);

                if (!entitledToAnything)
                {
                    // If user is not entitled to anything, add a dummy media, so that it will filter everything out
                    definitions.freeAssets.Add(eAssetTypes.MEDIA, new List<string>()
                    {
                        int.MinValue.ToString()
                    });
                }
            }
             */
        }

        #endregion

        /// <summary>
        /// Sets the language parameter of a search request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="searchDefinitions"></param>
        private static void SetLanguageDefinition(int groupId, Filter filter, UnifiedSearchDefinitions searchDefinitions)
        {
            LanguageObj objLang = null;

            if (filter == null)
            {
                objLang = GetLanguage(groupId, -1);
            }
            else
            {
                objLang = GetLanguage(groupId, filter.m_nLanguage);
            }

            searchDefinitions.langauge = objLang;
        }

        /// <summary>
        /// Creates a language object for a given group
        /// </summary>
        /// <param name="request"></param>
        /// <param name="searchDefinitions"></param>
        private static LanguageObj GetLanguage(int groupId, int languageId)
        {
            LanguageObj language = null;

            GroupManager groupManager = new GroupManager();
            CatalogCache catalogCache = CatalogCache.Instance();
            int parentGroupId = catalogCache.GetParentGroup(groupId);
            Group groupInCache = groupManager.GetGroup(parentGroupId);

            if (groupInCache != null)
            {
                if (languageId <= 0)
                {
                    language = groupInCache.GetGroupDefaultLanguage();
                }
                else
                {
                    language = groupInCache.GetLanguage(languageId);
                }
            }

            return language;
        }
    }
}
