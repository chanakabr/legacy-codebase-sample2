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

namespace Catalog
{
    public class UnifiedSearchDefinitionsCache : BaseCacheHelper<UnifiedSearchDefinitions>
    {
        #region Ctor

        public UnifiedSearchDefinitionsCache()
            : base()
        {
            this.OnErrorOccurred += UnifiedSearchDefinitionsCache_OnErrorOccurred;
        }

        #endregion

        #region Override Methods
        
        protected override UnifiedSearchDefinitions BuildValue(params object[] parameters)
        {
            UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions();

            UnifiedSearchRequest request = parameters[0] as UnifiedSearchRequest;

            CatalogCache catalogCache = CatalogCache.Instance();
            int parentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);

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
                definitions.deviceRuleId = ProtocolsFuncs.GetDeviceAllowedRuleIDs(request.m_oFilter.m_sDeviceId, request.m_nGroupID).ToArray();
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

            #endregion

            #region Asset Types

            // Special case - if no type was specified or "All" is contained, search all types
            if (request.assetTypes == null || request.assetTypes.Count == 0)
            {
                definitions.shouldSearchEpg = true;
                definitions.shouldSearchMedia = true;
            }
            else
            {
                definitions.mediaTypes = new List<int>(request.assetTypes);
            }

            // 0 - hard coded for EPG
            if (definitions.mediaTypes.Remove(0))
            {
                definitions.shouldSearchEpg = true;
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

            #region Personal Filters

            //if (request.personalFilters != null)
            //{
            //    // Get geo block rules that the user is allowed to watch
            //    if (request.personalFilters.Contains(ePersonalFilter.GeoBlockRules))
            //    {
            //        if (geoBlockRules == null)
            //        {
            //            geoBlockRules = GetGeoBlockRules(request.m_nGroupID, request.m_sUserIP);
            //        }

            //        definitions.geoBlockRules = geoBlockRules;
            //    }

            //    // Get parental rules tags that user is NOT allowed to see
            //    if (request.personalFilters.Contains(ePersonalFilter.ParentalRules))
            //    {
            //        if (mediaParentalRulesTags == null || epgParentalRulesTags == null)
            //        {
            //            Catalog.GetParentalRulesTags(request.m_nGroupID, request.m_sSiteGuid,
            //                out mediaParentalRulesTags, out epgParentalRulesTags);
            //        }

            //        definitions.mediaParentalRulesTags = mediaParentalRulesTags;
            //        definitions.epgParentalRulesTags = epgParentalRulesTags;
            //    }
            //}

            #endregion

            #region Search by entitlement

            if (definitions.entitlementSearchDefinitions != null)
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
                        request.order, entitlementMediaTypes, definitions.deviceRuleId);
                }

                if (group.groupMediaFileTypeToFileType != null && entitlementSearchDefinitions.shouldGetFreeAssets)
                {
                    // Convert the file type that we received in request (taken from groups_media_type)
                    // into the file type that the media file knows (based on the table media_files)
                    entitlementSearchDefinitions.fileTypes = new List<int>();

                    foreach (var fileType in fileTypes)
                    {
                        entitlementSearchDefinitions.fileTypes.Add(group.groupMediaFileTypeToFileType[fileType]);
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

            return definitions;
        }

        #endregion

        #region Public Methods

        public UnifiedSearchDefinitions GetDefinitions(UnifiedSearchRequest request)
        {
            // if no request Id - simply build the value and return it
            if (string.IsNullOrEmpty(request.requestId))
            {
                return this.BuildValue(request);
            }
            else
            {
                // Make sure every time that cache time is 10 minutes
                this.cacheTime = 10;

                string mutexName = string.Concat("Search Definitions GID_", request.m_nGroupID);
                string cacheKey =
                    string.Format("{0}_{1}_{2}_{3}",
                    this.version, "Search_Definitions", request.m_sSiteGuid, request.requestId);

                return this.Get(cacheKey, mutexName, request);
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// Throw upwards the kaltura exception - to maintain the status code
        /// </summary>
        /// <param name="ex"></param>
        private void UnifiedSearchDefinitionsCache_OnErrorOccurred(Exception ex)
        {
            if (ex is KalturaException)
            {
                throw ex;
            }
        }

        #endregion
    }
}
