using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Catalog.Cache;
using Core.Catalog.Request;
using Core.Catalog.Response;
using GroupsCacheManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Net;
using System.Web;
using System.ServiceModel;

namespace Core.Catalog
{
    public class EntitledAssetsUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static List<BaseSearchObject> GetUserSubscriptionSearchObjects(BaseRequest request, int groupId, string siteGuid, int domainId, int[] fileTypeIds,
            OrderObj order, string[] mediaTypes = null, int[] deviceRuleIds = null)
        {
            List<BaseSearchObject> result = new List<BaseSearchObject>();

            if (fileTypeIds == null)
            {
                fileTypeIds = new int[0];
            }

            // Get group from cache
            GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
            CatalogCache catalogCache = CatalogCache.Instance();
            int parentGroupId = catalogCache.GetParentGroup(groupId);
            Group group = groupManager.GetGroup(parentGroupId);

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

            var channelIds = userBundles.channels;

            // Get channels from cache
            List<GroupsCacheManager.Channel> allChannels = groupManager.GetChannels(channelIds.ToList(), group.m_nParentGroupID);

            if (allChannels != null && allChannels.Count > 0)
            {
                // Build search object for each channel
                var searchObjects = BundleAssetsRequest.BuildBaseSearchObjects(request, group, allChannels, mediaTypes, deviceRuleIds, order);
                result.AddRange(searchObjects);
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

        internal static List<int> GetUserEntitledEpgChannelIds(int parentGroupID, string siteGuid,
            UnifiedSearchDefinitions originalDefinitions,
            List<int> linearChannelMediaTypes)
        {
            List<int> result = new List<int>();

            ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

            if (searcher != null)
            {
                GroupManager manager = new GroupManager();
                Group group = manager.GetGroup(parentGroupID);
                UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions();

                // Copy definitons from original object
                definitions.entitlementSearchDefinitions = originalDefinitions.entitlementSearchDefinitions;
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
                definitions.filterPhrase = new BooleanLeaf("entitled_assets", "true", typeof(string), ComparisonOperator.Contains);
                definitions.mediaTypes = linearChannelMediaTypes;

                // Also indicate that we are interested in this field
                definitions.extraReturnFields.Add("epg_identifier");

                result = searcher.GetEntitledEpgLinearChannels(group, definitions);
            }

            return result;
        }
    }
}
