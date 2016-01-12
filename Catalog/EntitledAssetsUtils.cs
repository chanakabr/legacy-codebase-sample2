using ApiObjects;
using ApiObjects.SearchObjects;
using Catalog.Cache;
using Catalog.Request;
using GroupsCacheManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Catalog
{
    public class EntitledAssetsUtils
    {
        public static List<BaseSearchObject> GetUserSubscriptionSearchObjects(BaseRequest request, int groupId, string siteGuid, OrderObj order,
            string[] mediaTypes = null, int[] deviceRuleIds = null)
        {
            List<BaseSearchObject> result = new List<BaseSearchObject>();

            // Get group from cache
            GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
            CatalogCache catalogCache = CatalogCache.Instance();
            int parentGroupId = catalogCache.GetParentGroup(groupId);
            Group group = groupManager.GetGroup(parentGroupId);

            List<int> subscriptionIds = new List<int>();
            List<int> collectionIds = new List<int>();
            
            // TODO: get subscriptions and collections

            foreach (var subscription in subscriptionIds)
            {
                eBundleType bundleType = eBundleType.SUBSCRIPTION;

                // Get channel IDs of current bundle
                List<int> channelIds = Catalog.GetBundleChannelIds(group.m_nParentGroupID, subscription, bundleType);

                // Get channels from cache
                List<GroupsCacheManager.Channel> allChannels = groupManager.GetChannels(channelIds, group.m_nParentGroupID);

                // Build search object for each channel
                var subscriptionsSearchObjects = BundleAssetsRequest.BuildBaseSearchObjects(request, group, allChannels, mediaTypes, deviceRuleIds, order);
                result.AddRange(subscriptionsSearchObjects);
            }

            foreach (var collection in collectionIds)
            {
                eBundleType bundleType = eBundleType.COLLECTION;

                // Get channel IDs of current bundle
                List<int> channelIds = Catalog.GetBundleChannelIds(group.m_nParentGroupID, collection, bundleType);
                
                // Get channels from cache
                List<GroupsCacheManager.Channel> allChannels = groupManager.GetChannels(channelIds, group.m_nParentGroupID);

                // Build search object for each channel
                var collectionsSearchObjects = BundleAssetsRequest.BuildBaseSearchObjects(request, group, allChannels, mediaTypes, deviceRuleIds, order);
                result.AddRange(collectionsSearchObjects);
            }

            return result;
        }

        internal static Dictionary<eAssetTypes, List<string>> GetFreeAssets(int groupId, string siteGuid)
        {
            Dictionary<eAssetTypes, List<string>> result = new Dictionary<eAssetTypes, List<string>>();

            return result;
        }

        internal static Dictionary<eAssetTypes, List<string>> GetUserPPVAssets(int groupId, string siteGuid)
        {
            Dictionary<eAssetTypes, List<string>> result = new Dictionary<eAssetTypes, List<string>>();

            return result;
        }
    }
}
