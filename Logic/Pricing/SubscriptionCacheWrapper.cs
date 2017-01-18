using ApiObjects.Pricing;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    /*
    * 1. This class uses a decorator in order to wrap the BaseSubscription class. Understand Decorator Design Pattern before you change anything.
    * 2. Its main functionality is to add caching mechanism to Pricing methods uses by the Conditional Access module.
    * 3. Methods not called by CAS do not cache their results right now (September 2014).
    * 
    */
    public class SubscriptionCacheWrapper : BaseSubscriptionDecorator
    {
        protected static readonly string SUB_DATA_CACHE_NAME = "sub_data";
        protected static readonly string SUB_CACHE_WRAPPER_LOG_FILE = "SubCacheWrapper";

        public SubscriptionCacheWrapper(BaseSubscription originalBaseSubscription)
            : base(originalBaseSubscription)
        {

        }

        #region Methods with caching

        private string GetSubDataCacheKey(string subCode, bool isGetAlsoUnactive, bool isProductCode)
        {
            StringBuilder sb = new StringBuilder(String.Concat(originalBaseSubscription.GroupID, "_", SUB_DATA_CACHE_NAME, "_"));
            sb.Append(String.Concat(isProductCode ? "pc_" : "sc_", subCode, "_"));
            sb.Append(String.Concat(isGetAlsoUnactive ? "au_t" : "au_f"));
            return sb.ToString();
        }

        private List<string> GetSubscriptionsCacheKey(List<string> subscriptionCodes, bool isGetAlsoUnactive, bool isProductCode)
        {
            List<string> cachedKeys = null;            
            if (subscriptionCodes != null && subscriptionCodes.Count > 0)
            {
                cachedKeys = new List<string>();
                foreach (string subscriptionCode in subscriptionCodes)
                {
                    cachedKeys.Add(GetSubDataCacheKey(subscriptionCode, isGetAlsoUnactive, isProductCode));
                }
            }

            return cachedKeys;
        }

        private Dictionary<string, Subscription> GetSubscriptionsCacheKeyMappings(List<Subscription> subscriptions, bool isGetAlsoUnactive, bool isProductCode)
        {
            Dictionary<string, Subscription> cachedKeyMappings = null;
            if (subscriptions != null && subscriptions.Count > 0)
            {
                cachedKeyMappings = new Dictionary<string, Subscription>();
                foreach (Subscription subscription in subscriptions)
                {
                    if (!string.IsNullOrEmpty(subscription.m_sObjectCode))
                    {
                        string key = GetSubDataCacheKey(subscription.m_sObjectCode, isGetAlsoUnactive, isProductCode);
                        if (!cachedKeyMappings.ContainsKey(key))
                        {
                            cachedKeyMappings.Add(key, subscription);
                        }
                    }
                }
            }

            return cachedKeyMappings;
        }

        public override Subscription GetSubscriptionData(string sSubscriptionCode, string sCountryCd, string sLANGUAGE_CODE, 
            string sDEVICE_NAME, bool bGetAlsoUnActive)
        {
            Subscription res = null;
            if (!string.IsNullOrEmpty(sSubscriptionCode))
            {
                string cacheKey = GetSubDataCacheKey(sSubscriptionCode, bGetAlsoUnActive, false);
                Subscription sub = null;
                if (PricingCache.TryGetSubscription(cacheKey, out sub) && sub != null)
                    return sub;
                res = originalBaseSubscription.GetSubscriptionData(sSubscriptionCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, bGetAlsoUnActive);
                if (res != null)
                {
                    if (!PricingCache.TryAddSubscription(cacheKey, res))
                    {
                        PricingCache.LogCachingError("Failed to insert entry into cache. ", cacheKey, res, "GetSubscriptionData",
                            SUB_CACHE_WRAPPER_LOG_FILE);
                    }
                }
            }

            return res;
        }

        public override Subscription GetSubscriptionDataByProductCode(string sProductCode, string sCountryCd, string sLANGUAGE_CODE, 
            string sDEVICE_NAME, bool bGetAlsoUnActive)
        {
            string cacheKey = GetSubDataCacheKey(sProductCode, bGetAlsoUnActive, true);
            Subscription sub = null;
            if (PricingCache.TryGetSubscription(cacheKey, out sub) && sub != null)
                return sub;
            Subscription res = originalBaseSubscription.GetSubscriptionDataByProductCode(sProductCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, bGetAlsoUnActive);
            if (res != null)
            {
                if (!PricingCache.TryAddSubscription(cacheKey, res))
                {
                    PricingCache.LogCachingError("Failed to insert entry into cache. ", cacheKey, res, "GetSubscriptionDataByProductCode",
                        SUB_CACHE_WRAPPER_LOG_FILE);
                }
            }

            return res;
        }

        public override Subscription[] GetSubscriptionsDataByProductCodes(List<string> productCodes, bool getAlsoUnactive)
        {
            Subscription[] subscriptions = null;
            if (productCodes != null && productCodes.Count > 0)
            {
                Dictionary<string, Subscription> subscriptionsMapping = new Dictionary<string,Subscription>();
                List<string> unfoundSubscriptions = DAL.PricingDAL.Get_SubscriptionsFromProductCodes(productCodes.Distinct().ToList(), originalBaseSubscription.GroupID).Keys.ToList();
                if (unfoundSubscriptions != null && unfoundSubscriptions.Count > 0)
                {
                    List<string> cachedKeys = GetSubscriptionsCacheKey(unfoundSubscriptions, false, false);
                    if (cachedKeys != null && cachedKeys.Count > 0)
                    {
                        subscriptionsMapping = PricingCache.TryGetSubscriptions(cachedKeys);
                        if (subscriptionsMapping != null && subscriptionsMapping.Count != cachedKeys.Count)
                        {
                            foreach (string subscriptionKey in subscriptionsMapping.Keys)
                            {
                                unfoundSubscriptions.Remove(subscriptionKey);
                            }
                        }
                        else
                        {
                            subscriptionsMapping = new Dictionary<string, Subscription>();
                        }
                    }

                    if (unfoundSubscriptions.Count > 0)
                    {
                        Subscription[] subscriptionsFromDB = originalBaseSubscription.GetSubscriptionsData(unfoundSubscriptions.ToArray(), string.Empty, string.Empty, string.Empty, SubscriptionOrderBy.StartDateAsc);
                        if (subscriptionsFromDB != null && subscriptionsFromDB.Length > 0)
                        {
                            // add here unfound subs to both cache and result returned

                            Dictionary<string, Subscription> subscriptionCacheKeyMappings = GetSubscriptionsCacheKeyMappings(subscriptionsFromDB.ToList(), false, false);

                            if (subscriptionCacheKeyMappings != null && subscriptionCacheKeyMappings.Count > 0)
                            {
                                foreach (KeyValuePair<string, Subscription> pair in subscriptionCacheKeyMappings)
                                {
                                    if (!PricingCache.TryAddSubscription(pair.Key, pair.Value))
                                    {
                                        PricingCache.LogCachingError("Failed to insert entry into cache. ", pair.Key, pair.Value, "GetSubscriptionsDataByProductCodes", SUB_CACHE_WRAPPER_LOG_FILE);
                                    }

                                    if (subscriptionsMapping != null && !subscriptionsMapping.ContainsKey(pair.Key))
                                    {
                                        subscriptionsMapping.Add(pair.Key, pair.Value);
                                    }
                                }
                            }
                        }
                    }
                }

                subscriptions = subscriptionsMapping.Values.ToArray();
            }

            return subscriptions;
        }

        public override Subscription[] GetSubscriptionsData(string[] oSubCodes, string sCountryCd, string sLanguageCode, string sDeviceName, SubscriptionOrderBy orderBy)
        {
            if (oSubCodes != null && oSubCodes.Length > 0)
            {
                List<string> uncachedSubs = new List<string>();
                Dictionary<string, int> subsToIndexMapping = new Dictionary<string, int>();
                SortedSet<SortedSubscription> set = new SortedSet<SortedSubscription>();
                for (int i = 0; i < oSubCodes.Length; i++)
                {
                    if (string.IsNullOrEmpty(oSubCodes[i]) || subsToIndexMapping.ContainsKey(oSubCodes[i]))
                        continue;
                    string cacheKey = GetSubDataCacheKey(oSubCodes[i], false, false);
                    Subscription sub = null;
                    if (PricingCache.TryGetSubscription(cacheKey, out sub) && sub != null)
                    {
                        set.Add(new SortedSubscription(sub, i));
                    }
                    else
                    {
                        uncachedSubs.Add(oSubCodes[i]);
                        subsToIndexMapping.Add(oSubCodes[i], i);
                    }
                } // for

                if (uncachedSubs.Count > 0)
                {
                    // bring uncached subs from DB.
                    Subscription[] reducedSubs = originalBaseSubscription.GetSubscriptionsData(uncachedSubs.ToArray(), sCountryCd, sLanguageCode, sDeviceName, SubscriptionOrderBy.StartDateAsc);
                    if (reducedSubs != null && reducedSubs.Length > 0)
                    {
                        // add here uncached subs to both cache and result returned
                        for (int j = 0; j < reducedSubs.Length; j++)
                        {
                            if (reducedSubs[j] != null && !string.IsNullOrEmpty(reducedSubs[j].m_sObjectCode) &&
                                subsToIndexMapping.ContainsKey(reducedSubs[j].m_sObjectCode))
                            {
                                string cacheKey = GetSubDataCacheKey(reducedSubs[j].m_sObjectCode, false, false);
                                if (!PricingCache.TryAddSubscription(cacheKey, reducedSubs[j]))
                                {
                                    PricingCache.LogCachingError("Failed to insert entry into cache. ", cacheKey, reducedSubs[j],
                                        "GetSubscriptionsData", SUB_CACHE_WRAPPER_LOG_FILE);
                                }
                                set.Add(new SortedSubscription(reducedSubs[j], subsToIndexMapping[reducedSubs[j].m_sObjectCode]));
                            }
                        }
                    }
                }
                
                var ret = set.Select((item) => item.GetSubscription);
                if (orderBy == SubscriptionOrderBy.StartDateAsc)
                {
                    ret = ret.OrderBy(item => item.m_dStartDate);
                }
                else if (orderBy == SubscriptionOrderBy.StartDateDesc)
                {
                    ret = ret.OrderByDescending(item => item.m_dStartDate);
                }

                return ret.ToArray<Subscription>();
            }

            return null;
        }

        #endregion

        #region Methods without caching

        public override Subscription[] GetSubscriptionsShrinkList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return this.originalBaseSubscription.GetSubscriptionsShrinkList(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
        }

        public override Subscription[] GetSubscriptionsList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME) // consider adding caching
        {
            return this.originalBaseSubscription.GetSubscriptionsList(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
        }

        public override Subscription[] GetSubscriptionsList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int count) // consider adding caching
        {
            return this.originalBaseSubscription.GetSubscriptionsList(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, count);
        }

        public override Subscription[] GetSubscriptionsList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int nIsActive, int[] userTypesIDs) // consider adding caching
        {
            return this.originalBaseSubscription.GetSubscriptionsList(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, nIsActive, userTypesIDs);
        }

        public override Subscription[] GetRelevantSubscriptionsList(bool bShrink, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int count, int mediaID, int nFileTypeID)
        {
            return this.originalBaseSubscription.GetRelevantSubscriptionsList(bShrink, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, count, mediaID, nFileTypeID);
        }

        public override string[] GetRelevantSubscriptionsListSTR(bool bShrink, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int count, int mediaID, int nFileTypeID)
        {
            return this.originalBaseSubscription.GetRelevantSubscriptionsListSTR(bShrink, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, count, mediaID, nFileTypeID);
        }

        public override int[] GetMediaList(string sSubscriptionCode, int nFileTypeID, string sDevice)
        {
            return this.originalBaseSubscription.GetMediaList(sSubscriptionCode, nFileTypeID, sDevice);
        }

        public override bool DoesMediasExists(string sSubscriptionCode, int nMediaID)
        {
            return this.originalBaseSubscription.DoesMediasExists(sSubscriptionCode, nMediaID);
        }

        public override string DoesMediasExistsInSubs(string sSubscriptionCodes, int nMediaID)
        {
            return this.originalBaseSubscription.DoesMediasExistsInSubs(sSubscriptionCodes, nMediaID);
        }

        public override Subscription[] GetSubscriptionsContainingMedia(int nMediaID, int nFileTypeID)
        {
            return this.originalBaseSubscription.GetSubscriptionsContainingMedia(nMediaID, nFileTypeID);
        }

        public override Subscription[] GetSubscriptionsContainingMedia(int nMediaID, int nFileTypeID, bool isShrinked)
        {
            return this.originalBaseSubscription.GetSubscriptionsContainingMedia(nMediaID, nFileTypeID, isShrinked);
        }

        public override string GetSubscriptionsContainingMediaSTR(int nMediaID, int nFileTypeID, bool isShrinked)
        {
            return this.originalBaseSubscription.GetSubscriptionsContainingMediaSTR(nMediaID, nFileTypeID, isShrinked);
        }

        public override Subscription[] GetSubscriptionsContainingMedia(int nMediaID, int nFileTypeID, bool isShrinked, int index)
        {
            return this.originalBaseSubscription.GetSubscriptionsContainingMedia(nMediaID, nFileTypeID, isShrinked, index);
        }

        public override IdsResponse GetSubscriptionIDsContainingMediaFile(int nMediaID, int nMediaFileID)
        {
            return this.originalBaseSubscription.GetSubscriptionIDsContainingMediaFile(nMediaID, nMediaFileID);
        }

        public override Subscription[] GetSubscriptionsContainingMediaFile(int nMediaID, int nMediaFileID)
        {
            return this.originalBaseSubscription.GetSubscriptionsContainingMediaFile(nMediaID, nMediaFileID);
        }
        #endregion

        private class SortedSubscription : IComparable<SortedSubscription>
        {
            private int index;
            private Subscription sub;

            public SortedSubscription(Subscription s, int index)
            {
                this.index = index;
                this.sub = s;
            }

            public int Index
            {
                get
                {
                    return index;
                }
                private set
                {
                    index = value;
                }
            }

            public Subscription GetSubscription
            {
                get
                {
                    return sub;
                }
                private set
                {
                    this.sub = value;
                }
            }


            public int CompareTo(SortedSubscription other)
            {
                return Index.CompareTo(other.Index);
            }
        }


    }
}
