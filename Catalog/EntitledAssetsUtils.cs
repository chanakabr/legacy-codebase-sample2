using ApiObjects;
using ApiObjects.Response;
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
                var userBundles = cas.GetUserBundles(string.Empty, string.Empty, siteGuid);

                if (userBundles == null || userBundles.status == null)
                {
                    throw new KalturaException("Couldn't get user bundles", (int)eResponseStatus.Error);
                }

                if (userBundles.status.Code != (int)eResponseStatus.OK)
                {
                    throw new KalturaException(userBundles.status.Message, userBundles.status.Code);
                }

                foreach (var subscription in userBundles.subscriptions)
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

                foreach (var collection in userBundles.collections)
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
            }

            return result;
        }

        internal static Dictionary<eAssetTypes, List<string>> GetFreeAssets(int groupId, string siteGuid, out List<int> epgChannelIds)
        {
            Dictionary<eAssetTypes, List<string>> result = new Dictionary<eAssetTypes, List<string>>();
            epgChannelIds = new List<int>();

            return result;
        }

        internal static Dictionary<eAssetTypes, List<string>> GetUserPPVAssets(int groupId, string siteGuid, out List<int> epgChannelIds)
        {
            Dictionary<eAssetTypes, List<string>> result = new Dictionary<eAssetTypes, List<string>>();
            epgChannelIds = new List<int>();

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
                var purchasedAssets = cas.GetUserPurchasedAssets(userName, password, siteGuid);

                if (purchasedAssets == null || purchasedAssets.status == null)
                {
                    throw new KalturaException("Couldn't get user purchased assets", (int)eResponseStatus.Error);
                }

                if (purchasedAssets.status.Code != (int)eResponseStatus.OK)
                {
                    throw new KalturaException(purchasedAssets.status.Message, purchasedAssets.status.Code);
                }

            }
            return result;
        }

        internal static List<int> GetUserEntitledEpgChannelIds(int parentGroupID, string siteGuid)
        {
            List<int> result = new List<int>();

            return result;
        }
    }
}
