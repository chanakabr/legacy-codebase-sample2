using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    /*
    * 1. This class uses a decorator in order to wrap the BaseCollection class. Understand Decorator Design Pattern before you change anything.
    * 2. Its main functionality is to add caching mechanism to Pricing methods uses by the Conditional Access module.
    * 3. Methods not called by CAS do not cache their results right now (September 2014).
    * 
    */
    public class CollectionCacheWrapper : BaseCollectionDecorator
    {

        protected static readonly string COLL_DATA_CACHE_NAME = "coll_data";
        protected static readonly string COLL_CACHE_WRAPPER_LOG_FILE = "CollCacheWrapper";

        public CollectionCacheWrapper(BaseCollection originalBaseCollection)
            : base(originalBaseCollection)
        {
        }

        private string GetCollDataCacheKey(string collCode, bool isGetAlsoUnactive)
        {
            return String.Concat(this.originalBaseCollection.GroupID, "_", COLL_DATA_CACHE_NAME, "_cc_", collCode, isGetAlsoUnactive ? "au_t" : "au_f");
        }

        public override Collection GetCollectionData(string sCollectionCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bGetAlsoUnActive)
        {
            Collection res = null;
            if (!string.IsNullOrEmpty(sCollectionCode))
            {
                string cacheKey = GetCollDataCacheKey(sCollectionCode, bGetAlsoUnActive);
                Collection coll = null;
                if (PricingCache.TryGetCollection(cacheKey, out coll) && coll != null)
                    return coll;
                res = originalBaseCollection.GetCollectionData(sCollectionCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, bGetAlsoUnActive);
                if (res != null)
                {
                    if (!PricingCache.TryAddCollection(cacheKey, res))
                    {
                        PricingCache.LogCachingError("Failed to insert entry into cache. ", cacheKey, res, "GetCollectionData",
                            COLL_CACHE_WRAPPER_LOG_FILE);
                    }
                }
            }

            return res;
        }

        public override Collection[] GetCollectionsData(string[] oCollCodes, string sCountryCd, string sLanguageCode, string sDeviceName)
        {
            if (oCollCodes != null && oCollCodes.Length > 0)
            {
                List<string> uncachedColls = new List<string>();
                Dictionary<string, int> collsToIndexMapping = new Dictionary<string, int>();
                SortedSet<SortedCollection> set = new SortedSet<SortedCollection>();
                for (int i = 0; i < oCollCodes.Length; i++)
                {
                    if (string.IsNullOrEmpty(oCollCodes[i]) || collsToIndexMapping.ContainsKey(oCollCodes[i]))
                        continue;
                    string cacheKey = GetCollDataCacheKey(oCollCodes[i], false);
                    Collection coll = null;
                    if (PricingCache.TryGetCollection(cacheKey, out coll) && coll != null)
                    {
                        set.Add(new SortedCollection(coll, i));
                    }
                    else
                    {
                        collsToIndexMapping.Add(oCollCodes[i], i);
                        uncachedColls.Add(oCollCodes[i]);
                    }
                } // for

                if (uncachedColls.Count > 0)
                {
                    Collection[] colls = originalBaseCollection.GetCollectionsData(uncachedColls.ToArray(), sCountryCd, sLanguageCode, sDeviceName);
                    if (colls != null && colls.Length > 0)
                    {
                        for (int j = 0; j < colls.Length; j++)
                        {
                            if (colls[j] != null && !string.IsNullOrEmpty(colls[j].m_sObjectCode) &&
                                collsToIndexMapping.ContainsKey(colls[j].m_sObjectCode))
                            {
                                string cacheKey = GetCollDataCacheKey(colls[j].m_sObjectCode, false);
                                if (!PricingCache.TryAddCollection(cacheKey, colls[j]))
                                {
                                    PricingCache.LogCachingError("Failed to insert entry into cache. ", cacheKey, colls[j],
                                        "GetCollectionsData", COLL_CACHE_WRAPPER_LOG_FILE);
                                }
                                set.Add(new SortedCollection(colls[j], collsToIndexMapping[colls[j].m_sObjectCode]));
                            }
                        } // for
                    }
                }

                return set.Select((item) => item.GetCollection).ToArray<Collection>();
            }

            return null;
        }

        private class SortedCollection : IComparable<SortedCollection>
        {
            private Collection collection;
            private int index;

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

            public Collection GetCollection
            {
                get
                {
                    return collection;
                }
                private set
                {
                    collection = value;
                }
            }

            public SortedCollection(Collection c, int index)
            {
                this.collection = c;
                this.index = index;
            }

            public int CompareTo(SortedCollection other)
            {
                return this.Index.CompareTo(other.Index);
            }
        }
    }
}
