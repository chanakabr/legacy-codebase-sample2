using CachingProvider;
using CouchbaseManager;
using DAL;
using KLogMonitor;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace CachingHelpers
{
    public class GeoBlockRules
    {
        #region Consts

        /// <summary>
        /// 24 hours
        /// </summary>
        private static readonly double DEFAULT_TIME_IN_CACHE_MINUTES = 1440d;
        private static readonly string DEFAULT_CACHE_NAME = "GroupsCache";

        #endregion

        #region Statis members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object locker = new object();

        #endregion

        #region Private Members

        private ICachingService rulesCache = null;
        private readonly double cacheTime;
        private string cacheGroupConfiguration;

        #endregion

        #region Singleton

        private static GeoBlockRules instance;

        public static GeoBlockRules Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new GeoBlockRules();
                    }
                }
            }

            return instance;
        }

        #endregion

        #region Ctor and initialization

        private GeoBlockRules()
        {
            cacheGroupConfiguration = TVinciShared.WS_Utils.GetTcmConfigValue("GroupsCacheConfiguration");

            switch (cacheGroupConfiguration)
            {
                case "CouchBase":
                {
                    rulesCache = CouchBaseCache<List<int>>.GetInstance("CACHE");

                    //set ttl time for document 
                    cacheTime = GetDocTTLSettings();     
                    break;
                }
                case "InnerCache":
                {
                    cacheTime = GetDefaultCacheTimeInMinutes();
                    InitializeCachingService(GetCacheName(), cacheTime);
                    break;
                }
                case "Hybrid":
                {
                    cacheTime = GetDefaultCacheTimeInMinutes();
                    string cacheName = GetCacheName();
                    rulesCache = HybridCache<List<int>>.GetInstance(eCouchbaseBucket.CACHE, cacheName);

                    break;
                }
            }
        }

        private void InitializeCachingService(string cacheName, double cacheTime)
        {
            this.rulesCache = new SingleInMemoryCache(cacheName, cacheTime);
        }

        private string GetCacheName()
        {
            string result = DEFAULT_CACHE_NAME;

            string tcm = TVinciShared.WS_Utils.GetTcmConfigValue("GROUPS_CACHE_NAME");
            
            if (tcm.Length > 0)
            {
                result = tcm;
            }

            return result;
        }

        private double GetDefaultCacheTimeInMinutes()
        {
            double result = DEFAULT_TIME_IN_CACHE_MINUTES;
            double tcm = 0d;

            string timeString = TVinciShared.WS_Utils.GetTcmConfigValue("GROUPS_CACHE_TIME_IN_MINUTES");

            if (timeString.Length > 0 && Double.TryParse(timeString, out tcm) && tcm > 0)
            {
                result = tcm;
            }

            return result;
        }

        private double GetDocTTLSettings()
        {
            double result;

            if (!double.TryParse(TVinciShared.WS_Utils.GetTcmConfigValue("GroupsCacheDocTimeout"), out result))
            {
                result = 1440.0;
            }

            return result;
        }

        #endregion

        #region Country To Rule cache

        public List<int> GetGeoBlockRulesByCountry(int groupId, int countryId)
        {
            List<int> rules = new List<int>();

            BaseModuleCache baseModule = null;
            try
            {
                string cacheKey = string.Format("country_to_rules_{0}_{1}", groupId, countryId);

                baseModule = this.rulesCache.Get(cacheKey);

                if (baseModule != null && baseModule.result != null)
                {
                    rules = baseModule.result as List<int>;
                }
                else
                {
                    bool inserted = false;
                    bool createdNew = false;
                    var mutexSecurity = Utils.CreateMutex();
                    using (Mutex mutex = new Mutex(false, string.Concat("Group GeoBlockRules GID_", groupId), out createdNew, mutexSecurity))
                    {
                        try
                        {
                            mutex.WaitOne(-1);

                            VersionModuleCache versionModule = (VersionModuleCache)this.rulesCache.GetWithVersion<List<int>>(cacheKey);

                            if (versionModule != null && versionModule.result != null)
                            {
                                rules = baseModule.result as List<int>;
                            }

                            else
                            {
                                List<int> tempRules = ApiDAL.GetPermittedGeoBlockRulesByCountry(groupId, countryId);

                                for (int i = 0; i < 3 && !inserted; i++)
                                {
                                    //try insert to Cache
                                    versionModule.result = tempRules;
                                    inserted = this.rulesCache.SetWithVersion<List<int>>(cacheKey, versionModule, cacheTime);

                                    if (inserted)
                                    {
                                        rules = tempRules;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("GetGeoBlockRulesByCountry - " + string.Format("Couldn't get geo block rules for group {0} and country {1}, ex = {2}",
                                groupId, countryId, ex.Message), ex);
                        }
                        finally
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetGeoBlockRulesByCountry - " + string.Format("Couldn't get geo block rules for group {0} and country {1}, ex = {2}",
                    groupId, countryId, ex.Message), ex);
            }

            return rules;
        }

        public bool RemoveGeoBlockRulesOfcountry(int groupId, int countryId)
        {
            bool isRemoveSucceeded = false;
            VersionModuleCache versionModule = null;

            try
            {
                string cacheKey = string.Format("country_to_rules_{0}_{1}", groupId, countryId);

                for (int i = 0; i < 3 && !isRemoveSucceeded; i++)
                {
                    versionModule = (VersionModuleCache)rulesCache.GetWithVersion<List<int>>(cacheKey);

                    if (versionModule != null && versionModule.result != null)
                    {
                        List<int> rules = versionModule.result as List<int>;

                        bool createdNew = false;
                        var mutexSecurity = Utils.CreateMutex();

                        using (Mutex mutex = new Mutex(false, string.Concat("Cache Delete GeoBlockRules_", groupId), out createdNew, mutexSecurity))
                        {
                            mutex.WaitOne(-1);

                            //try update to CB
                            BaseModuleCache bModule = rulesCache.Remove(cacheKey);

                            if (bModule != null && bModule.result != null)
                            {
                                isRemoveSucceeded = true;
                            }

                            mutex.ReleaseMutex();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("RemoveGeoBlockRulesOfcountry - " +
                    string.Format("failed to Remove geo block rules from cache GroupID={0}, country= {1}, ex={2}", groupId, countryId, ex.Message), ex);
            }

            return isRemoveSucceeded;
        }

        #endregion
    }
}
