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
        public delegate void ExceptionDelegate(Exception ex);

        public event ExceptionDelegate OnErrorOccurred;

        #region Consts

        /// <summary>
        /// 24 hours
        /// </summary>
        protected static readonly uint DEFAULT_TIME_IN_CACHE_SECONDS = 86400;
        protected static readonly string DEFAULT_CACHE_NAME = "GroupsCache";

        #endregion

        #region Statis members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected static object locker = new object();

        #endregion

        #region Private Members

        protected ICachingService cacheService = null;
        /// <summary>
        /// How long will an object stay in cache IN MINUTES
        /// </summary>
        protected uint cacheTime;
        protected string version;

        #endregion

        #region Ctor and initialization

        protected BaseCacheHelper(string cacheType = "")
        {
            if (string.IsNullOrEmpty(cacheType))
            {
                cacheType = TVinciShared.WS_Utils.GetTcmConfigValue("GroupsCacheConfiguration");
            }

            switch (cacheType.ToLower())
            {
                case "couchbase":
                {
                    cacheService = CouchBaseCache<T>.GetInstance("CACHE");
                    version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");

                    //set ttl time for document 
                    cacheTime = GetDocTTLSettings();
                    break;
                }
                case "innercache":
                {
                    cacheTime = GetDefaultCacheTimeInSeconds();
                    InitializeCachingService(GetCacheName(), cacheTime);
                    break;
                }
                case "hybrid":
                {
                    cacheTime = GetDefaultCacheTimeInSeconds();
                    string cacheName = GetCacheName();
                    cacheService = HybridCache<T>.GetInstance(eCouchbaseBucket.CACHE, cacheName);
                    version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");

                    break;
                }
            }
        }

        private void InitializeCachingService(string cacheName, uint expirationInSeconds)
        {
            this.cacheService = SingleInMemoryCacheManager.Instance(cacheName, expirationInSeconds);
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

        private uint GetDefaultCacheTimeInSeconds()
        {
            uint result = DEFAULT_TIME_IN_CACHE_SECONDS;
            uint tcm = 0;

            string timeString = TVinciShared.WS_Utils.GetTcmConfigValue("GROUPS_CACHE_TIME_IN_MINUTES");

            if (timeString.Length > 0 && uint.TryParse(timeString, out tcm) && tcm > 0)
            {
                result = tcm * 60;
            }

            return result;
        }

        private uint GetDocTTLSettings()
        {
            uint result;

            if (!uint.TryParse(TVinciShared.WS_Utils.GetTcmConfigValue("GroupsCacheDocTimeout"), out result))
            {
                // 24 hours
                result = 86400;
            }
            else
            {
                // convert to seconds (TCM config is in minutes)
                result *= 60;
            }

            return result;
        }

        #endregion

        #region Abstract and virtual Methods

        protected virtual T BuildValue(params object[] parameters)
        {
            return default(T);
        }

        protected virtual List<T> MultiBuildValue(List<long> fullIds, List<int> indexes, params object[] parameters)
        {
            return null;
        }

        #endregion

        #region Protected Methods

        protected List<T2> BuildPartialList<T2>(List<T2> fullList, List<int> indexes)
        {
            List<T2> partial = new List<T2>();

            for (int i = 0; i < indexes.Count; i++)
            {
                partial.Add(fullList[indexes[i]]);
            }

            return partial;
        }

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
                    log.DebugFormat("Couldn't get cache key {0}. Trying with version.", cacheKey);

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
                                value = (T)versionModule.result;
                            }
                            else
                            {
                                log.DebugFormat("Couldn't get cache key {0} with version. Building value.", cacheKey);

                                T tempValue = BuildValue(parameters);

                                for (int i = 0; i < 3 && !inserted; i++)
                                {
                                    if (versionModule == null)
                                    {
                                        versionModule = new VersionModuleCache();
                                    }

                                    //try insert to Cache
                                    versionModule.result = tempValue;

                                    inserted = this.cacheService.SetWithVersion<T>(cacheKey, versionModule, cacheTime);

                                    if (inserted)
                                    {
                                        log.DebugFormat("Inserted value to key {0}.", cacheKey);

                                        value = tempValue;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("Get - " + string.Format("Couldn't get object in cache by key {0}. ex = {1}",
                                cacheKey, ex.Message), ex);

                            if (this.OnErrorOccurred != null)
                            {
                                this.OnErrorOccurred(ex);
                            }
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
                log.ErrorFormat("Get: Couldn't get object in cache by key {0}. ex = {1}, ST = {2}",
                    cacheKey, ex, ex.StackTrace);

                if (this.OnErrorOccurred != null)
                {
                    this.OnErrorOccurred(ex);
                }
            }

            return value;
        }

        public List<T> MultiGet(List<long> ids, List<string> cacheKeys, string mutexName, params object[] parameters)
        {
            T[] values = new T[cacheKeys.Count];

            try
            {
                List<int> uncachedIndexes = new List<int>();

                for (int i = 0; i < cacheKeys.Count; i++)
                {
                    string cacheKey = cacheKeys[i];

                    T value = default(T);

                    BaseModuleCache baseModule = this.cacheService.Get(cacheKey);

                    // If we found - put in values array
                    if (baseModule != null && baseModule.result != null)
                    {
                        value = (T)baseModule.result;
                        values[i] = value;
                    }
                    else
                    {
                        bool createdNew = false;
                        var mutexSecurity = Utils.CreateMutex();
                        using (Mutex mutex = new Mutex(false, mutexName, out createdNew, mutexSecurity))
                        {
                            try
                            {
                                mutex.WaitOne(-1);

                                VersionModuleCache versionModule = (VersionModuleCache)this.cacheService.GetWithVersion<T>(cacheKey);

                                // If we found - put in values array
                                if (versionModule != null && versionModule.result != null)
                                {
                                    value = (T)baseModule.result;
                                    values[i] = value;
                                }
                                else
                                {
                                    // If we DIDN't find - remember the INDEX of the key
                                    uncachedIndexes.Add(i);
                                }
                            }
                            catch (Exception ex)
                            {
                                log.ErrorFormat("Get - " + string.Format("Couldn't get object in cache by key {0}. ex = {1}",
                                    cacheKey, ex.Message), ex);

                                if (this.OnErrorOccurred != null)
                                {
                                    this.OnErrorOccurred(ex);
                                }
                            }
                            finally
                            {
                                mutex.ReleaseMutex();
                            }
                        }
                    }
                }

                // If some of the keys didn't return in cache - build them and save in cache
                if (uncachedIndexes.Count > 0)
                {
                    List<long> partialIds = BuildPartialList(ids, uncachedIndexes);

                    // Ask the inhertied class to build the values to put in cache
                    List<T> newValues = this.MultiBuildValue(partialIds, uncachedIndexes, parameters);

                    for (int i = 0; i < uncachedIndexes.Count; i++)
                    {
                        // Order should be identical!
                        T tempValue = newValues[i];
                        int originalIndex = uncachedIndexes[i];

                        string cachedKey = cacheKeys[originalIndex];

                        VersionModuleCache versionModule = (VersionModuleCache)this.cacheService.GetWithVersion<T>(cachedKey);
                        bool inserted = false;
                        
                        //try insert to Cache
                        for (int tryNumber = 0; tryNumber < 3 && !inserted; tryNumber++)
                        {
                            versionModule.result = tempValue;
                            inserted = this.cacheService.SetWithVersion<T>(cachedKey, versionModule, cacheTime);

                            if (inserted)
                            {
                                values[originalIndex] = tempValue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Get - " + string.Format("Couldn't multi get. ex = {0}", ex.Message), ex);

                if (this.OnErrorOccurred != null)
                {
                    this.OnErrorOccurred(ex);
                }
            }

            return values.ToList();
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

                if (this.OnErrorOccurred != null)
                {
                    this.OnErrorOccurred(ex);
                }
            }

            return isRemoveSucceeded;
        }

        #endregion
    }
}