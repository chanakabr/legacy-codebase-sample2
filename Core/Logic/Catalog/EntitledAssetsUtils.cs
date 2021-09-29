using ApiLogic.IndexManager.Helpers;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Catalog.Cache;
using Core.Catalog.Request;
using GroupsCacheManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Core.Catalog
{
    public class EntitledAssetsUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static List<BaseSearchObject> GetUserSubscriptionSearchObjects(BaseRequest request, int groupId, string siteGuid, int domainId, int[] fileTypeIds,
            OrderObj order, string[] mediaTypes = null, int[] deviceRuleIds = null, CatalogGroupCache catalogGroupCache = null)
        {
            List<BaseSearchObject> result = new List<BaseSearchObject>();

            if (fileTypeIds == null)
            {
                fileTypeIds = new int[0];
            }

            int parentGroupId = groupId;
            GroupsCacheManager.GroupManager groupManager = null;
            Group group = null;
            bool doesGroupUsesTemplates = CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
            if (doesGroupUsesTemplates)
            {
                if (catalogGroupCache == null)
                {
                    if (!CatalogManagement.CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetUserSubscriptionSearchObjects", groupId);
                        return result;
                    }
                }
            }
            else
            {
                // Get group from cache
                groupManager = new GroupsCacheManager.GroupManager();
                CatalogCache catalogCache = CatalogCache.Instance();
                parentGroupId = catalogCache.GetParentGroup(groupId);
                group = groupManager.GetGroup(parentGroupId);
            }

            List<int> subscriptionIds = new List<int>();
            List<int> collectionIds = new List<int>();

            // Initialize web service
            var userBundles = ConditionalAccess.Module.GetUserBundles(groupId, domainId, fileTypeIds);

            if (userBundles == null || userBundles.status == null)
            {
                throw new KalturaException("Couldn't get user bundles", (int)eResponseStatus.Error);
            }

            if (userBundles.status.Code != (int)eResponseStatus.OK)
            {
                throw new KalturaException(userBundles.status.Message, userBundles.status.Code);
            }

            List<GroupsCacheManager.Channel> allChannels = new List<Channel>();
            if (userBundles.channels != null && userBundles.channels.Count > 0)
            {
                if (doesGroupUsesTemplates)
                {
                    long userId = 0;
                    long.TryParse(request.m_sSiteGuid, out userId);

                    GenericListResponse<Channel> channelRes = CatalogManagement.ChannelManager.Instance.SearchChannels(groupId, true, string.Empty, 
                        userBundles.channels, 0, userBundles.channels.Count, ChannelOrderBy.Id, OrderDir.ASC, false, userId);
                    if (channelRes.HasObjects())
                    {
                        allChannels.AddRange(channelRes.Objects);
                    }
                }
                else
                {
                    // Get channels from cache
                    allChannels.AddRange(groupManager.GetChannels(userBundles.channels, group.m_nParentGroupID));
                }

                if (allChannels != null && allChannels.Count > 0)
                {
                    // Build search object for each channel
                    var searchObjects = BundleAssetsRequest.BuildBaseSearchObjects(request, group, allChannels, mediaTypes, order, groupId, doesGroupUsesTemplates, false);
                    result.AddRange(searchObjects);
                }
            }

            return result;
        }

        internal static Dictionary<eAssetTypes, List<string>> GetFreeAssets(int groupId, string siteGuid, out List<int> epgChannelIds)
        {
            Dictionary<eAssetTypes, List<string>> result = new Dictionary<eAssetTypes, List<string>>();
            epgChannelIds = new List<int>();

            return result;
        }

        internal static Dictionary<eAssetTypes, List<string>> GetUserPPVAssets(int groupId, string siteGuid, int domainId, int[] fileTypes, out List<int> epgChannelIds)
        {
            Dictionary<eAssetTypes, List<string>> result = new Dictionary<eAssetTypes, List<string>>();
            epgChannelIds = new List<int>();

            if (fileTypes == null)
            {
                fileTypes = new int[0];
            }

            log.DebugFormat("domain {0} filetypes {1}",
                domainId, string.Join(",", fileTypes));

            var purchasedAssets = ConditionalAccess.Module.GetUserPurchasedAssets(groupId, domainId, fileTypes);

            if (purchasedAssets == null || purchasedAssets.status == null)
            {
                throw new KalturaException("Couldn't get user purchased assets", (int)eResponseStatus.Error);
            }

            if (purchasedAssets.status.Code != (int)eResponseStatus.OK)
            {
                throw new KalturaException(purchasedAssets.status.Message, purchasedAssets.status.Code);
            }

            // Process result from CAS - create dictionary based on key/value pairs
            foreach (var currentAsset in purchasedAssets.assets)
            {
                eAssetTypes currentType = eAssetTypes.UNKNOWN;

                if (Enum.TryParse<eAssetTypes>(currentAsset.key, out currentType))
                {
                    if (!result.ContainsKey(currentType))
                    {
                        result.Add(currentType, new List<string>());
                    }

                    result[currentType].Add(currentAsset.value);
                }
            }

            return result;
        }

        internal static List<int> GetUserEntitledEpgChannelIds(int parentGroupID, string siteGuid, UnifiedSearchDefinitions originalDefinitions, List<int> linearChannelMediaTypes, bool doesGroupUsesTemplates)
        {
            //IRA: check if can skip ES

            List<int> result = new List<int>();

            IIndexManager indexManager = IndexManagerFactory.Instance.GetIndexManager(parentGroupID);

            Group group = null;
            if (!doesGroupUsesTemplates)
            {
                GroupManager manager = new GroupManager();
                group = manager.GetGroup(parentGroupID);
            }

            UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions();
            bool shouldSearchNotEntitled = originalDefinitions.entitlementSearchDefinitions.shouldSearchNotEntitled;

            // Copy definitions from original object
            definitions.entitlementSearchDefinitions = originalDefinitions.entitlementSearchDefinitions;
            definitions.entitlementSearchDefinitions.shouldSearchNotEntitled = false;
            definitions.deviceRuleId = originalDefinitions.deviceRuleId;
            definitions.groupId = parentGroupID;
            definitions.indexGroupId = parentGroupID;
            definitions.geoBlockRules = originalDefinitions.geoBlockRules;
            definitions.permittedWatchRules = originalDefinitions.permittedWatchRules;
            definitions.shouldSearchMedia = true;
            definitions.shouldSearchEpg = false;
            definitions.shouldSearchRecordings = false;
            definitions.userTypeID = originalDefinitions.userTypeID;

            // Most important part - tell the definitions to search only entitled assets and only of linear channels
            definitions.filterPhrase = new BooleanLeaf(NamingHelper.ENTITLED_ASSETS_FIELD, "true", typeof(string), ComparisonOperator.Contains);
            definitions.mediaTypes = linearChannelMediaTypes;

            // Also indicate that we are interested in this field
            definitions.extraReturnFields.Add("epg_identifier");

            result = indexManager.GetEntitledEpgLinearChannels(definitions);

            originalDefinitions.entitlementSearchDefinitions.shouldSearchNotEntitled = shouldSearchNotEntitled;

            return result;
        }
    }
}
