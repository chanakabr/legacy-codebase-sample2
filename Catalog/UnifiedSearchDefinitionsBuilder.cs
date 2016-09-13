using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CachingHelpers;
using Catalog.Request;
using Catalog.Cache;
using GroupsCacheManager;
using TVinciShared;
using ApiObjects.Response;
using KLogMonitor;
using System.Reflection;
using ApiObjects;

namespace Catalog
{
    public class UnifiedSearchDefinitionsBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private bool shouldUseCache = false;

        #region Ctor

        public UnifiedSearchDefinitionsBuilder()
        {
            shouldUseCache = WS_Utils.GetTcmBoolValue("Use_Search_Cache");
        }

        #endregion

        #region Public Methods

        public UnifiedSearchDefinitions GetDefinitions(UnifiedSearchRequest request)
        {
            UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions();

            try
            {
                SetLanguageDefinition(request.m_nGroupID, request.m_oFilter, definitions);

                CatalogCache catalogCache = CatalogCache.Instance();
                int parentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);

                definitions.shouldUseSearchEndDate = request.GetShouldUseSearchEndDate();
                definitions.shouldDateSearchesApplyToAllTypes = request.shouldDateSearchesApplyToAllTypes;

                definitions.shouldIgnoreDeviceRuleID = request.shouldIgnoreDeviceRuleID;

                GroupManager groupManager = new GroupManager();
                Group group = groupManager.GetGroup(parentGroupID);

                if (request.filterTree != null)
                {
                    Catalog.UpdateNodeTreeFields(request, ref request.filterTree, definitions, group);
                }

                // Get days offset for EPG search from TCM
                definitions.epgDaysOffest = Catalog.GetCurrentRequestDaysOffset();

                #region Filter & Order

                if (request.m_oFilter != null)
                {
                    definitions.shouldUseStartDate = request.m_oFilter.m_bUseStartDate;
                    definitions.shouldUseFinalEndDate = request.m_oFilter.m_bUseFinalDate;
                    definitions.userTypeID = request.m_oFilter.m_nUserTypeID;
                    if (!definitions.shouldIgnoreDeviceRuleID)
                    {
                        definitions.deviceRuleId = ProtocolsFuncs.GetDeviceAllowedRuleIDs(request.m_oFilter.m_sDeviceId, request.m_nGroupID).ToArray();
                    }
                }

                OrderObj order = new OrderObj();
                order.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.NONE;
                order.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;

                Catalog.GetOrderValues(ref order, request.order);

                if (order.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.META && string.IsNullOrEmpty(order.m_sOrderValue))
                {
                    order.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.CREATE_DATE;
                    order.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                }

                definitions.order = new OrderObj();
                definitions.order.m_eOrderDir = order.m_eOrderDir;
                definitions.order.m_eOrderBy = order.m_eOrderBy;
                definitions.order.m_sOrderValue = order.m_sOrderValue;
                definitions.groupId = request.m_nGroupID;
                definitions.permittedWatchRules = Catalog.GetPermittedWatchRules(request.m_nGroupID);
                definitions.filterPhrase = request.filterTree;
                definitions.exactGroupId = request.exactGroupId;

                #endregion

                #region Asset Types

                // Special case - if no type was specified or "All" is contained, search all types
                if (request.assetTypes == null || request.assetTypes.Count == 0)
                {
                    definitions.shouldSearchEpg = true;
                    definitions.shouldSearchMedia = true;

                    // except recodrings. FOR NOW
                    // TEMPORARY UNTIL REST IS FIXED
                    // FOR NOW ONLY
                    definitions.shouldSearchRecordings = false;
                }
                else
                {
                    definitions.mediaTypes = new List<int>(request.assetTypes);
                }

                // 0 - hard coded for EPG
                if (definitions.mediaTypes.Remove(UnifiedSearchDefinitions.EPG_ASSET_TYPE))
                {
                    definitions.shouldSearchEpg = true;
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

                HashSet<int> mediaTypes = new HashSet<int>(group.GetMediaTypes());

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

                List<int> regionIds;
                List<string> linearMediaTypes;

                Catalog.SetSearchRegions(request.m_nGroupID, request.domainId, request.m_sSiteGuid, out regionIds, out linearMediaTypes);

                definitions.regionIds = regionIds;
                definitions.linearChannelMediaTypes = linearMediaTypes;

                #endregion

                Catalog.GetParentMediaTypesAssociations(request.m_nGroupID,
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
                        type = eEntitlementSearchType.Entitled;
                    }

                    if (type != eEntitlementSearchType.None)
                    {
                        if (shouldUseCache)
                        {
                            definitions.entitlementSearchDefinitions =
                                EntitlementDefinitionsCache.Instance().GetEntitlementSearchDefinitions(definitions, request, request.order, parentGroupID, group, type);
                        }
                        else
                        {
                            BuildEntitlementSearchDefinitions(definitions, request, request.order, parentGroupID, group);
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

                #endregion

                #region Get Recordings

                if (definitions.shouldSearchRecordings)
                {
                    List<string> recordings = GetUserRecordings(definitions, request.m_sSiteGuid, request.m_nGroupID, (long)request.domainId);

                    if (definitions.specificAssets == null)
                    {
                        definitions.specificAssets = new Dictionary<eAssetTypes, List<string>>();
                    }

                    // If there are previous specific assets - we narrow down the list to contain only the user's recordings
                    if (definitions.specificAssets.ContainsKey(eAssetTypes.NPVR))
                    {
                        var currentRecordings = definitions.specificAssets[eAssetTypes.NPVR];

                        var newRecordings = currentRecordings.Intersect(recordings).ToList();

                        definitions.specificAssets[eAssetTypes.NPVR] = newRecordings;
                    }
                    // Otherwise we are happy with the list we got from conditional access
                    else
                    {
                        definitions.specificAssets.Add(eAssetTypes.NPVR, recordings);
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
                        if (!definitions.extraReturnFields.Contains(field.ToLower()))
                        {
                            definitions.extraReturnFields.Add(field.ToLower());
                        }
                    }
                    definitions.shouldReturnExtendedSearchResult = true;
                }

                #endregion

            }
            catch (Exception ex)
            {
                log.ErrorFormat("failed building unified search definitions. ex = {0}, st = {1}", ex, ex.StackTrace);
                throw ex;
            }

            return definitions;
        }

        private List<string> GetUserRecordings(UnifiedSearchDefinitions definitions, string siteGuid, int groupId, long domainId)
        {
            List<string> result = new List<string>();

            string userName = string.Empty;
            string password = string.Empty;

            //get username + password from wsCache
            Credentials credentials =
                TvinciCache.WSCredentials.GetWSCredentials(ApiObjects.eWSModules.CATALOG, groupId, ApiObjects.eWSModules.CONDITIONALACCESS);

            if (credentials != null)
            {
                userName = credentials.m_sUsername;
                password = credentials.m_sPassword;
            }

            // validate user name and password length
            if (userName.Length == 0 || password.Length == 0)
            {
                throw new Exception(string.Format(
                    "No WS_CAS login parameters were extracted from DB. userId={0}, groupid={1}",
                    siteGuid, groupId));
            }

            // Initialize web service
            using (ws_cas.module cas = new ws_cas.module())
            {
                string url = Utils.GetWSURL("ws_cas");
                cas.Url = url;

                var casResponse = cas.GetDomainRecordingsMapping(userName, password, domainId);

                if (casResponse == null)
                {
                    throw new Exception("WS_CAS GetDomainRecordingsMapping returned invalid response");
                }

                Dictionary<string, string> recordingsToDomainRecordingsMapping = new Dictionary<string,string>();

                foreach (var recording in casResponse)
                {
                    recordingsToDomainRecordingsMapping.Add(recording.value, recording.key);
                    result.Add(recording.value);
                }

                definitions.recordingsToDomainRecordingsMapping = recordingsToDomainRecordingsMapping;

            }

            return result;
        }

        public static void BuildEntitlementSearchDefinitions(UnifiedSearchDefinitions definitions,
            BaseRequest request,
            OrderObj order,
            int parentGroupID, Group group)
        {
            int[] fileTypes = null;

            if (request.m_oFilter != null)
            {
                fileTypes = request.m_oFilter.fileTypes;
            }

            var entitlementSearchDefinitions = definitions.entitlementSearchDefinitions;

            List<int> epgChannelIds = new List<int>();
            List<int> freeEpgChannelIds = null;
            List<int> purchasedEpgChannelIds = null;

            // TODO: Maybe we won't use this (getting free assets) method eventually!
            if (entitlementSearchDefinitions.shouldGetFreeAssets)
            {
                entitlementSearchDefinitions.freeAssets =
                    EntitledAssetsUtils.GetFreeAssets(parentGroupID, request.m_sSiteGuid, out freeEpgChannelIds);
            }

            if (entitlementSearchDefinitions.shouldGetPurchasedAssets)
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
                    selectedMediaTypes = selectedMediaTypes.Union(group.linearChannelMediaTypes.Select(t => t.ToString()));
                }

                entitlementMediaTypes = selectedMediaTypes.ToArray();
            }

            if (entitlementSearchDefinitions.shouldGetPurchasedAssets)
            {
                entitlementSearchDefinitions.subscriptionSearchObjects =
                    EntitledAssetsUtils.GetUserSubscriptionSearchObjects(request, parentGroupID, request.m_sSiteGuid, request.domainId, fileTypes,
                    order, entitlementMediaTypes, definitions.deviceRuleId);
            }

            if (group.groupMediaFileTypeToFileType != null && entitlementSearchDefinitions.shouldGetFreeAssets)
            {
                // Convert the file type that we received in request (taken from groups_media_type)
                // into the file type that the media file knows (based on the table media_files)
                entitlementSearchDefinitions.fileTypes = new List<int>();

                if (fileTypes != null)
                {
                    foreach (var fileType in fileTypes)
                    {
                        entitlementSearchDefinitions.fileTypes.Add(group.groupMediaFileTypeToFileType[fileType]);
                    }
                }
            }

            // TODO: Maybe this will be the method that gets the FREE epg channel IDs
            var entitledChannelIds =
                EntitledAssetsUtils.GetUserEntitledEpgChannelIds(parentGroupID, request.m_sSiteGuid, definitions, group.linearChannelMediaTypes);

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
