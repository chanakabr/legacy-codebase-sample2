using ApiObjects.Pricing.Dto;
using CachingProvider.LayeredCache;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Core.Pricing
{
    public interface IPricingCache
    {
        bool TryGetGroupPricePlans(string key, out List<UsageModule> pricePlans);
        List<SubscriptionItemDTO> GetGroupSubscriptionsItems(int groupId, bool getAlsoInActive, string nameContains);
        List<Subscription> GetSubscriptions(int groupId, List<long> subscriptionIds);
        bool InvalidateSubscription(int groupId, int subId = 0);
        bool InvalidateSubscriptions(int groupId, List<int> subIds = null);
    }

    public class PricingCache : IPricingCache
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<PricingCache> lazy = new Lazy<PricingCache>(() => new PricingCache(), LazyThreadSafetyMode.PublicationOnly);

        public static PricingCache Instance => lazy.Value;

        private const string PRICING_CACHE_WRAPPER_LOG_FILE = "PricingCacheWrapper";

        private PricingCache() { }

        static internal Dictionary<string, Subscription> TryGetSubscriptions(List<string> keys)
        {
            Dictionary<string, Subscription> subscriptions = null;
            try
            {
                Dictionary<string, object> values = GetValues(keys);
                if (values != null && values.Count > 0)
                {
                    subscriptions = new Dictionary<string, Subscription>();
                    foreach (KeyValuePair<string, object> pair in values)
                    {
                        if (!string.IsNullOrEmpty(pair.Key) && pair.Value != null)
                        {
                            Subscription sub = (Subscription)pair.Value;
                            if (sub != null && !string.IsNullOrEmpty(sub.m_sObjectCode) && !subscriptions.ContainsKey(sub.m_sObjectCode))
                            {
                                subscriptions.Add(sub.m_sObjectCode, sub);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                if (keys != null && keys.Count > 0)
                {
                    foreach (string key in keys)
                    {
                        sb.Append(key + ", ");
                    }
                }
                log.ErrorFormat("Error getting subscriptions from cache, keys: {0}. Exception: {1}", sb.ToString(), ex.Message);
            }

            return subscriptions;
        }

        static internal Dictionary<string, PPVModule> TryGetPPVmodules(List<string> keys)
        {
            Dictionary<string, PPVModule> ppvModules = null;
            try
            {
                Dictionary<string, object> values = GetValues(keys);
                if (values != null && values.Count > 0)
                {
                    ppvModules = new Dictionary<string, PPVModule>();
                    foreach (KeyValuePair<string, object> pair in values)
                    {
                        if (!string.IsNullOrEmpty(pair.Key) && pair.Value != null)
                        {
                            PPVModule ppvModule = (PPVModule)pair.Value;
                            if (ppvModule != null && !string.IsNullOrEmpty(ppvModule.m_sObjectCode) && !ppvModules.ContainsKey(ppvModule.m_sObjectCode))
                            {
                                ppvModules.Add(ppvModule.m_sObjectCode, ppvModule);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                if (keys != null && keys.Count > 0)
                {
                    foreach (string key in keys)
                    {
                        sb.Append(key + ", ");
                    }
                }
                log.ErrorFormat("Error getting PPVmodules from cache, keys: {0}. Exception: {1}", sb.ToString(), ex.Message);
            }

            return ppvModules;
        }

        static internal bool TryGetSubscription(string key, out Subscription sub)
        {
            bool res = false;
            Subscription temp = Get<Subscription>(key);
            if (temp != null)
            {
                sub = temp;
                res = true;
            }
            else
            {
                sub = null;
                res = false;
            }

            return res;
        }

        static internal bool TryAddSubscription(string key, Subscription sub)
        {
            return sub != null && Add(key, sub);
        }

        static internal bool TryGetCollection(string key, out Collection coll)
        {
            bool res = false;
            Collection temp = Get<Collection>(key);
            if (temp != null)
            {
                coll = temp;
                res = true;
            }
            else
            {
                coll = null;
                res = false;
            }

            return res;
        }

        static internal bool TryAddCollection(string key, Collection coll)
        {
            return coll != null && Add(key, coll);
        }

        static internal bool TryGetPPVModule(string key, out PPVModule ppv)
        {
            bool res = false;
            PPVModule temp = Get<PPVModule>(key);
            if (temp != null)
            {
                ppv = temp;
                res = true;
            }
            else
            {
                ppv = null;
                res = false;
            }

            return res;
        }

        static internal bool TryAddPPVModule(string key, PPVModule ppv)
        {
            return ppv != null && Add(key, ppv);
        }

        static internal bool TryGetMediaFilePPVModuleObj(string key, out MediaFilePPVModule mfpm)
        {
            bool res = false;
            MediaFilePPVModule temp = Get<MediaFilePPVModule>(key);
            if (temp != null)
            {
                mfpm = temp;
                res = true;
            }
            else
            {
                mfpm = null;
                res = false;
            }

            return res;
        }

        static internal bool TryGetUsageModule(string key, out UsageModule um)
        {
            bool res = false;
            UsageModule temp = Get<UsageModule>(key);
            if (temp != null)
            {
                um = temp;
                res = true;
            }
            else
            {
                um = null;
                res = false;
            }

            return res;
        }

        static internal bool TryAddUsageModule(string key, UsageModule um)
        {
            return um != null && Add(key, um);
        }

        static internal bool TryAddMediaFilePPVModuleObj(string key, MediaFilePPVModule mfpm)
        {
            return mfpm != null && Add(key, mfpm);
        }

        static internal bool TryGetPrePaidModule(string key, out PrePaidModule ppm)
        {
            bool res = false;
            PrePaidModule temp = Get<PrePaidModule>(key);
            if (temp != null)
            {
                res = true;
                ppm = temp;
            }
            else
            {
                res = false;
                ppm = null;
            }

            return res;
        }

        static internal bool TryAddPrePaidModule(string key, PrePaidModule ppm)
        {
            return ppm != null && Add(key, ppm);
        }

        static internal bool TryGetPriceCode(string key, out PriceCode pc)
        {
            bool res = false;
            PriceCode temp = Get<PriceCode>(key);
            if (temp != null)
            {
                pc = temp;
                res = true;
            }
            else
            {
                pc = null;
                res = false;
            }

            return res;
        }

        static internal bool TryAddPriceCode(string key, PriceCode pc)
        {
            var result = pc != null && Add(key, pc);
            if (!result)
            {
                LogCachingError("Failed to insert entry into cache. ", key, pc, "GetPriceCodeData",
                    PRICING_CACHE_WRAPPER_LOG_FILE);
            }

            return result;
        }

        static internal bool TryGetMediaFilePPVContainer(string key, out MediaFilePPVContainer mfpc)
        {
            bool res = false;
            MediaFilePPVContainer temp = Get<MediaFilePPVContainer>(key);
            if (temp != null)
            {
                mfpc = temp;
                res = true;
            }
            else
            {
                mfpc = null;
                res = false;
            }

            return res;
        }

        static internal bool TryAddMediaFilePPVContainer(string key, MediaFilePPVContainer mfpc)
        {
            return mfpc != null && Add(key, mfpc);
        }


        static internal void LogCachingError(string msg, string key, object obj, string methodName, string logFile)
        {
            StringBuilder sb = new StringBuilder(msg);
            sb.Append(String.Concat(" Key: ", key));
            sb.Append(String.Concat(" Val: ", obj != null ? obj.ToString() : "null"));
            sb.Append(String.Concat(" Method Name: ", methodName));
            //sb.Append(String.Concat(" Cache Data: ", ToString()));
            log.Error("CacheError - " + sb.ToString());
        }

        /*
        public override string ToString()
        {
            return cache.ToString();
        }
        */

        private static bool Add(string key, object obj)
        {
            return TvinciCache.WSCache.Instance.Add(key, obj);
        }

        private static T Get<T>(string key)
        {
            return TvinciCache.WSCache.Instance.Get<T>(key);
        }        

        public static void Remove(string key)
        {
            TvinciCache.WSCache.Instance.Remove(key);
        }

        private static Dictionary<string, object> GetValues(List<string> keys)
        {
            Dictionary<string, object> values = null;
            if (keys != null && keys.Count > 0)
            {
                values = TvinciCache.WSCache.Instance.GetValues(keys) as Dictionary<string, object>;
            }

            return values;
        }

        public bool TryGetGroupPricePlans(string key, out List<UsageModule> pricePlans)
        {
            bool res = false;
            List<UsageModule> temp = Get<List<UsageModule>>(key);
            if (temp != null)
            {
                pricePlans = temp;
                res = true;
            }
            else
            {
                pricePlans = null;
                res = false;
            }

            return res;
        }

        static internal bool TryAddGroupPricePlans(string key, List<UsageModule> pricePlans)
        {
            var result = pricePlans != null && Add(key, pricePlans);
            if (!result)
            {
                LogCachingError("Failed to insert entry into cache. ", key, pricePlans, "GetPricePlans",
                    PRICING_CACHE_WRAPPER_LOG_FILE);
            }

            return result;
        }

        public static List<long> GetCollectionsIds(int groupId, bool inactiveAssets, HashSet<long> assetUserRuleIds, string nameContains)
        {
            var response = new List<long>();
            
            try
            {
                var result = GetGroupCollectionsItems(groupId);

                if (result?.Count > 0)
                {
                    foreach (var item in result)
                    {
                        if (!inactiveAssets && !item.IsActive)
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(nameContains) && !string.IsNullOrEmpty(item.Name) && item.Name.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) == -1)
                        {
                            continue;
                        }

                        if (assetUserRuleIds?.Count > 0 && (!item.AssetUserRuleId.HasValue || !assetUserRuleIds.Contains(item.AssetUserRuleId.Value)))
                        {
                            continue;
                        }

                        response.Add(item.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed GetGroupCollectionIds for groupId: {0}, ex: {1}", groupId, ex);
            }

            return response;
        }

        public static List<long> FilterCollectionsByAssetUserRuleId(int groupId, List<long> collectionIds, HashSet<long> assetUserRuleIds)
        {
            var response = new List<long>();

            try
            {
                var result = GetGroupCollectionsItems(groupId);

                if (result?.Count > 0)
                {
                    response = result.Where(x => x.AssetUserRuleId.HasValue && assetUserRuleIds.Contains(x.AssetUserRuleId.Value) && collectionIds.Contains(x.Id)).Select(x => x.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed FilterCollectionsByAssetUserRuleId for groupId: {0}, ex: {1}", groupId, ex);
            }

            return response;
        }

        private static List<CollectionItemDTO> GetGroupCollectionsItems(int groupId)
        {
            var result = new List<CollectionItemDTO>();

            try
            {
                var key = GetGroupCollectionsItemsCacheKey(groupId);
                if (!LayeredCache.Instance.Get(key, ref result,
                    GetGroupCollectionsItems, new Dictionary<string, object>() { { "groupId", groupId } },
                groupId, LayeredCacheConfigNames.GET_GROUP_COLLECTIONS, new List<string>()
                { LayeredCacheKeys.GetCollectionsIdsInvalidationKey(groupId) }))
                {
                    log.ErrorFormat($"GetGroupCollectionsItems - Failed get data from cache. groupId: {groupId}");
                    return result;
                }

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed GetGroupCollectionsItems for groupId: {0}, ex: {1}", groupId, ex);
            }

            return result;
        }

        public List<SubscriptionItemDTO> GetGroupSubscriptionsItems(int groupId, bool getAlsoInActive, string nameContains)
        {
            var response = new List<SubscriptionItemDTO>();
            try
            {
                var key = LayeredCacheKeys.GetGroupSubscriptionItemsKey(groupId);
                var ikKey = LayeredCacheKeys.GetGroupSubscriptionItemsInvalidationKey(groupId);

                if (!LayeredCache.Instance.Get(key, ref response,
                    GetGroupSubscriptionsItems, new Dictionary<string, object>() { { "groupId", groupId } },
                    groupId, LayeredCacheConfigNames.GET_GROUP_SUBSCRIPTION_ITEMS, new List<string>() { ikKey }))
                {
                    log.ErrorFormat($"GetGroupSubscriptionsItems - Failed get data from cache. groupId: {groupId}");
                    return response;
                }

                if (response != null)
                {
                    var tempResult = response.AsEnumerable();
                    if (!getAlsoInActive && tempResult.Any())
                    {
                        tempResult = tempResult.Where(x => x.IsActive);
                    }

                    if (!string.IsNullOrEmpty(nameContains) && tempResult.Any())
                    {
                        tempResult = tempResult.Where(x => x.Name.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) > -1);
                    }

                    response = tempResult.ToList();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed GetGroupSubscriptions for groupId: {0}, ex: {1}", groupId, ex);
            }

            return response;
        }

        public static string GetCollectionsIdsCacheKey(int groupId)
        {
            return $"CollectionsIds_V2_{groupId}";
        }

        public static string GetGroupCollectionsItemsCacheKey(int groupId)
        {
            return $"GroupCollectionsItems_V2_{groupId}";
        }

        public static Tuple<Dictionary<long, bool>, bool> GetGroupCollectionIds(Dictionary<string, object> funcParams)
        {
            int? groupId = 0;
            if (funcParams != null && funcParams.Count == 1)
            {
                if (funcParams.ContainsKey("groupId"))
                {
                    groupId = funcParams["groupId"] as int?;
                    if (groupId == null)
                    {
                        return Tuple.Create(new Dictionary<long, bool>(), false);
                    }
                }
            }

            Dictionary<long, bool> res = DAL.PricingDAL.GetAllCollectionIds(groupId.Value);
            return Tuple.Create(res, res?.Count > 0);
        }

        public static Tuple<List<CollectionItemDTO>, bool> GetGroupCollectionsItems(Dictionary<string, object> funcParams)
        {
            int? groupId = 0;
            if (funcParams != null && funcParams.Count == 1)
            {
                if (funcParams.ContainsKey("groupId"))
                {
                    groupId = funcParams["groupId"] as int?;
                    if (groupId == null)
                    {
                        return Tuple.Create(new List<CollectionItemDTO>(), false);
                    }
                }
            }

            List<CollectionItemDTO> res = DAL.PricingDAL.GetGroupCollectionsItems(groupId.Value);
            return Tuple.Create(res, res?.Count > 0);
        }

        public static Tuple<List<SubscriptionItemDTO>, bool> GetGroupSubscriptionsItems(Dictionary<string, object> funcParams)
        {
            List<SubscriptionItemDTO> res = new List<SubscriptionItemDTO>();

            if (funcParams != null && funcParams.ContainsKey("groupId"))
            {
                int? groupId = funcParams["groupId"] as int?;
                res = DAL.PricingDAL.GetGroupSubscriptionsItems(groupId.Value);
            }

            return Tuple.Create(res, true);
        }

        public List<Subscription> GetSubscriptions(int groupId, List<long> subscriptionIds)
        {
            List<Subscription> subscriptions = new List<Subscription>();

            if (subscriptionIds == null || subscriptionIds.Count == 0)
            {
                return subscriptions;
            }

            Dictionary<string, string> keysToOriginalValueMap = new Dictionary<string, string>();
            Dictionary<string, List<string>> invalidationKeysMap = new Dictionary<string, List<string>>();

            foreach (long id in subscriptionIds)
            {
                string key = LayeredCacheKeys.GetSubscriptionKey(groupId, id);
                keysToOriginalValueMap.Add(key, id.ToString());
                invalidationKeysMap.Add(key, new List<string>() { LayeredCacheKeys.GetSubscriptionInvalidationKey(groupId, id) });
            }

            Dictionary<string, Subscription> subscriptionsMap = null;

            if (!LayeredCache.Instance.GetValues(keysToOriginalValueMap,
                                                ref subscriptionsMap,
                                                GetSubscriptions,
                                                new Dictionary<string, object>() {
                                                        { "groupId", groupId },
                                                        { "subscriptionIds", keysToOriginalValueMap.Values.ToList() }
                                                   },
                                                groupId,
                                                LayeredCacheConfigNames.GET_SUBSCRIPTIONS,
                                                invalidationKeysMap))
            {
                log.Warn($"Failed getting Subscriptions from LayeredCache, groupId: {groupId}, subIds: {string.Join(",", subscriptionIds)}");
                return subscriptions;
            }

            return subscriptionsMap == null ? new List<Subscription>() : subscriptionsMap.Values.ToList();
        }
        public static Tuple<Dictionary<string, Subscription>, bool> GetSubscriptions(Dictionary<string, object> funcParams)
        {
            Dictionary<string, Subscription> subscriptions = new Dictionary<string, Subscription>();
            List<string> subscriptionIds = null;
            int? groupId = funcParams["groupId"] as int?;
            if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
            {
                subscriptionIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]);
            }
            else if (funcParams["subscriptionIds"] != null)
            {
                subscriptionIds = (List<string>)funcParams["subscriptionIds"];
            }

            if (subscriptionIds?.Count > 0)
            {
                BaseSubscription t = null;
                Utils.GetBaseImpl(ref t, groupId.Value);
                if (t != null)
                {
                    var subs = t.GetSubscriptionsData(subscriptionIds.ToArray(), string.Empty, string.Empty, string.Empty, ApiObjects.Pricing.SubscriptionOrderBy.StartDateAsc);
                    if (subs != null && subs.Length > 0)
                    {
                        subscriptions = subs.ToDictionary(x => LayeredCacheKeys.GetSubscriptionKey(groupId.Value, long.Parse(x.m_sObjectCode)), y => y);
                    }
                }
            }

            return Tuple.Create(subscriptions, true);
        }

        public bool InvalidateSubscription(int groupId, int subId = 0)
        {
            List<string> keys = new List<string>() { LayeredCacheKeys.GetGroupSubscriptionItemsInvalidationKey(groupId) };
            if (subId > 0)
            {
                keys.Add(LayeredCacheKeys.GetSubscriptionInvalidationKey(groupId, subId));
            }

            return LayeredCache.Instance.InvalidateKeys(keys);
        }

        public bool InvalidateSubscriptions(int groupId, List<int> subIds = null)
        {
            List<string> keys = new List<string>() { LayeredCacheKeys.GetGroupSubscriptionItemsInvalidationKey(groupId) };
            
            if (subIds?.Count > 0)
            {
                keys.AddRange(subIds.Select(x => LayeredCacheKeys.GetSubscriptionInvalidationKey(groupId, x)));  
            }

            return LayeredCache.Instance.InvalidateKeys(keys);
        }

        public List<Collection> GetCollections(int groupId, List<long> collectionIds)
        {
            List<Collection> collections = new List<Collection>();

            if (collectionIds == null || collectionIds.Count == 0)
            {
                return collections;
            }

            Dictionary<string, string> keysToOriginalValueMap = new Dictionary<string, string>();
            Dictionary<string, List<string>> invalidationKeysMap = new Dictionary<string, List<string>>();

            foreach (long id in collectionIds)
            {
                string key = LayeredCacheKeys.GetCollectionKey(groupId, id);
                keysToOriginalValueMap.Add(key, id.ToString());
                invalidationKeysMap.Add(key, new List<string>() { LayeredCacheKeys.GetCollectionInvalidationKey(groupId, id) });
            }

            Dictionary<string, Collection> collectionsMap = null;

            if (!LayeredCache.Instance.GetValues(keysToOriginalValueMap,
                                                ref collectionsMap,
                                                GetCollections,
                                                new Dictionary<string, object>() {
                                                        { "groupId", groupId },
                                                        { "collectionIds", keysToOriginalValueMap.Values.ToList() }
                                                   },
                                                groupId,
                                                LayeredCacheConfigNames.GET_SUBSCRIPTIONS,
                                                invalidationKeysMap))
            {
                log.Warn($"Failed getting Collections from LayeredCache, groupId: {groupId}, subIds: {string.Join(",", collectionIds)}");
                return collections;
            }

            return collectionsMap == null ? new List<Collection>() : collectionsMap.Values.ToList();
        }

        public static Tuple<Dictionary<string, Collection>, bool> GetCollections(Dictionary<string, object> funcParams)
        {
            Dictionary<string, Collection> collections = new Dictionary<string, Collection>();
            List<string> collectionIds = null;
            int? groupId = funcParams["groupId"] as int?;
            if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
            {
                collectionIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]);
            }
            else if (funcParams["collectionIds"] != null)
            {
                collectionIds = (List<string>)funcParams["collectionIds"];
            }

            if (collectionIds?.Count > 0)
            {
                BaseCollection t = null;
                Utils.GetBaseImpl(ref t, groupId.Value);
                if (t != null)
                {
                    var response = t.GetCollectionsData(collectionIds.ToArray(), string.Empty, string.Empty, string.Empty, null);
                    if (response != null && response.Collections != null && response.Collections.Length > 0)
                    {
                        collections = response.Collections.ToDictionary(x => LayeredCacheKeys.GetCollectionKey(groupId.Value, long.Parse(x.m_sObjectCode)), y => y);
                    }
                }
            }

            return Tuple.Create(collections, true);
        }

        public bool InvalidateCollection(int groupId, long collectionId = 0)
        {
            List<string> keys = new List<string>() { LayeredCacheKeys.GetCollectionsIdsInvalidationKey(groupId) };
            if (collectionId > 0)
            {
                keys.Add(LayeredCacheKeys.GetCollectionInvalidationKey(groupId, collectionId));
            }

            return LayeredCache.Instance.InvalidateKeys(keys);
        }

        public bool InvalidateCollections(int groupId, List<long> collectionIds = null)
        {
            List<string> keys = new List<string>() { LayeredCacheKeys.GetCollectionsIdsInvalidationKey(groupId) };

            if (collectionIds?.Count > 0)
            {
                keys.AddRange(collectionIds.Select(x => LayeredCacheKeys.GetCollectionInvalidationKey(groupId, x)));
            }

            return LayeredCache.Instance.InvalidateKeys(keys);
        }

        public bool InvalidatePago(long groupId, long pagoId = 0)
        {
            List<string> keys = new List<string>() { LayeredCacheKeys.GetPagoIdsInvalidationKey(groupId) };
            if (pagoId > 0)
            {
                keys.Add(LayeredCacheKeys.GetPagoInvalidationKey(groupId, pagoId));
            }

            return LayeredCache.Instance.InvalidateKeys(keys);
        }
    }
}