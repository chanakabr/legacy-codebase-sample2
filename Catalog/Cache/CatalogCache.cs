using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CachingProvider;
using DAL;

namespace Catalog.Cache
{
    public class CatalogCache
    {
        #region Constants
        private static readonly double DEFAULT_TIME_IN_CACHE_MINUTES = 120d;
        private static readonly string DEFAULT_CACHE_NAME = "CatalogCache";
        #endregion

        #region Singleton properties
        private static object locker = new object();
        private static CatalogCache instance = null;
        #endregion
        private ICachingService cache = null;

        private string GetCacheName()
        {
            string res = Utils.GetWSURL("CATALOG_CACHE_NAME");
            if(res.Length > 0)
                return res;
            return DEFAULT_CACHE_NAME;
        }

        private double GetDefaultCacheTimeInMinutes()
        {
            double res = 0d;
            string timeStr = Utils.GetWSURL("CATALOG_CACHE_TIME_IN_MINUTES");
            if (timeStr.Length > 0 && Double.TryParse(timeStr, out res) && res > 0)
                return res;
            return DEFAULT_TIME_IN_CACHE_MINUTES;
        }

        private void InitializeCachingService(string cacheName, double cachingTimeMinutes)
        {
            this.cache = new SingleInMemoryCache(cacheName, cachingTimeMinutes);
        }

        private CatalogCache()
        {
            InitializeCachingService(GetCacheName(), GetDefaultCacheTimeInMinutes());
        }

        public static CatalogCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new CatalogCache();
                    }
                }
            }

            return instance;
        }


        public int GetParentGroup(int nGroupID)
        {
            int nParentGroup = 0;
            BaseModuleCache bModule;
            try
            {
                string sKey = "ParentGroupCache_" + nGroupID.ToString();
                bModule = instance.cache.Get(sKey);
                if (bModule != null && bModule.result != null)
                {
                    nParentGroup = int.Parse(bModule.result.ToString());
                }
                else
                {
                    //GetParentGroup
                    nParentGroup = UtilsDal.GetParentGroupID(nGroupID);
                    bModule = new BaseModuleCache(nParentGroup);
                    bool bSet = instance.cache.Set(sKey, bModule);
                }
                return nParentGroup;
            }
            catch (Exception ex)
            {
                return nGroupID;
            }
        }
    }
}
