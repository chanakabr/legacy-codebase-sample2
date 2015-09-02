using CachingProvider;
using CouchbaseManager;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace CachingHelpers
{
    public abstract class BaseCacheHelper<T>
    {
        #region Consts

        /// <summary>
        /// 24 hours
        /// </summary>
        protected static readonly double DEFAULT_TIME_IN_CACHE_MINUTES = 1440d;
        protected static readonly string DEFAULT_CACHE_NAME = "GroupsCache";

        #endregion

        #region Statis members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected static object locker = new object();

        #endregion

        #region Private Members

        protected ICachingService cacheService = null;
        protected readonly double cacheTime;
        protected string cacheGroupConfiguration;

        #endregion

        #region Ctor and initialization

        protected BaseCacheHelper()
        {
            cacheGroupConfiguration = TVinciShared.WS_Utils.GetTcmConfigValue("GroupsCacheConfiguration");

            switch (cacheGroupConfiguration)
            {
                case "CouchBase":
                {
                    cacheService = CouchBaseCache<T>.GetInstance("CACHE");

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
                    cacheService = HybridCache<T>.GetInstance(eCouchbaseBucket.CACHE, cacheName);

                    break;
                }
            }
        }

        private void InitializeCachingService(string cacheName, double cacheTime)
        {
            this.cacheService = new SingleInMemoryCache(cacheName, cacheTime);
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

        #region Abstract Methods

        protected abstract T BuildValue(params object[] parameters);

        #endregion

        #region Public Methods

        public T Get(string cacheKey, string mutexName, params object[] parameters)
        {
            T value = default(T);

            try
            {
                BaseModuleCache baseModule = this.cacheService.Get(cacheKey);

                if (baseModule != null && baseModule.result != null)
                {
                    value = (T)baseModule.result;
                }
                else
                {
                    bool inserted = false;
                    bool createdNew = false;
                    var mutexSecurity = Utils.CreateMutex();
                    using (Mutex mutex = new Mutex(false, mutexName, out createdNew, mutexSecurity))
                    {
                        try
                        {
                            mutex.WaitOne(-1);

                            VersionModuleCache versionModule = (VersionModuleCache)this.cacheService.GetWithVersion<T>(cacheKey);

                            if (versionModule != null && versionModule.result != null)
                            {
                                value = (T)baseModule.result;
                            }
                            else
                            {
                                T tempValue = BuildValue(parameters);

                                for (int i = 0; i < 3 && !inserted; i++)
                                {
                                    //try insert to Cache
                                    versionModule.result = value;
                                    inserted = this.cacheService.SetWithVersion<T>(cacheKey, versionModule, cacheTime);

                                    if (inserted)
                                    {
                                        value = tempValue;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("Get - " + string.Format("Couldn't get object in cache by key {0}. ex = {1}",
                                cacheKey, ex.Message), ex);
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
                log.ErrorFormat("Get - " + string.Format("Couldn't get object in cache by key {0}. ex = {1}",
                    cacheKey, ex.Message), ex);
            }

            return value;
        }

        public virtual bool Remove(string cacheKey, string mutexName)
        {
            bool isRemoveSucceeded = false;

            try
            {
                for (int i = 0; i < 3 && !isRemoveSucceeded; i++)
                {
                    VersionModuleCache versionModule = (VersionModuleCache)cacheService.GetWithVersion<T>(cacheKey);

                    if (versionModule != null && versionModule.result != null)
                    {
                        T cacheValue = (T)versionModule.result;

                        bool createdNew = false;
                        var mutexSecurity = Utils.CreateMutex();

                        using (Mutex mutex = new Mutex(false, mutexName, out createdNew, mutexSecurity))
                        {
                            mutex.WaitOne(-1);

                            //try update to CB
                            BaseModuleCache bModule = cacheService.Remove(cacheKey);

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
                log.ErrorFormat("Remove - " +
                    string.Format("failed to Remove object from cache key={0}, ex={1}", cacheKey, ex.Message), ex);
            }

            return isRemoveSucceeded;
        }

        #endregion
    }
}
