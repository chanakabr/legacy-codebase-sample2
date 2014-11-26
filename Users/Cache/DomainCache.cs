using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CachingProvider;

namespace Users.Cache
{
    public class DomainCache
    {
        private static object locker = new object();

        #region inner cache - call WSCache
        private static bool Add(string key, object obj)
        {
            return TvinciCache.WSCache.Instance.Add(key, obj);
        }

        private static T Get<T>(string key)
        {
            return TvinciCache.WSCache.Instance.Get<T>(key);
        }

        internal static bool AddItem(string key, object obj)
        {
            return (!string.IsNullOrEmpty(key)) && Add(key, obj);
        }

        internal static bool GetItem<T>(string key, out T oValue)
        {
            bool res = false;
            T temp = Get<T>(key);
            if (temp != null)
            {
                res = true;
                oValue = temp;
            }
            else
            {
                res = false;
                oValue = default(T);
            }

            return res;
        }
        #endregion

        #region ExternalCache

        #region OutOfProcessCache
        private static object syncRoot = new Object();
        private ConcurrentDictionary<int, ReaderWriterLockSlim> m_oLockers; // readers-writers lockers for operator channel ids.        
        private ICachingService cache = null;
        private readonly double dCacheTT;
        private string sKeyCache = "d";
        #endregion

        #region Constants        
        protected const string DOMAIN_LOG_FILENAME = "DomainCache";
        #endregion

        private static DomainCache instance = null;

        private static double GetDocTTLSettings()
        {
            double nResult;
            if (!double.TryParse(TVinciShared.WS_Utils.GetTcmConfigValue("DomainCacheDocTimeout"), out nResult))
            {
                nResult = 1440.0;
            }

            return nResult;
        }

        private DomainCache()
        {
            // create to instanse of cache to domain  external (by CouchBase) abd internal (TvinciCache)
            cache = CouchBaseCache<Domain>.GetInstance("CACHE");
            this.m_oLockers = new ConcurrentDictionary<int, ReaderWriterLockSlim>();
            dCacheTT = GetDocTTLSettings();     //set ttl time for document            
        }

        /*Method that lock domain when change the object*/
        private void GetLocker(int ndomainID, ref ReaderWriterLockSlim locker)
        {
            if (!m_oLockers.ContainsKey(ndomainID))
            {
                lock (m_oLockers)
                {
                    Logger.Logger.Log("GetLocker", string.Format("Locked. Domain ID: {0}", ndomainID), DOMAIN_LOG_FILENAME);
                    if (!m_oLockers.ContainsKey(ndomainID))
                    {
                        if (!m_oLockers.TryAdd(ndomainID, new ReaderWriterLockSlim()))
                        {
                            Logger.Logger.Log("GetLocker", string.Format("Failed to create reader writer manager. DomainID {0}", ndomainID), DOMAIN_LOG_FILENAME);
                        }
                    }
                }
                Logger.Logger.Log("GetLocker", string.Format("Locker released. DomainID: {0}", ndomainID), DOMAIN_LOG_FILENAME);
            }

            if (!m_oLockers.TryGetValue(ndomainID, out locker))
            {
                Logger.Logger.Log("GetLocker", string.Format("Failed to read reader writer manager. DomainID: {0}", ndomainID), DOMAIN_LOG_FILENAME);
            }
        }

        #endregion

        public static DomainCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new DomainCache();
                    }
                }
            }

            return instance;
        }

        internal void LogCachingError(string msg, string key, object obj, string methodName, string logFile)
        {
            StringBuilder sb = new StringBuilder(msg);
            sb.Append(String.Concat(" Key: ", key));
            sb.Append(String.Concat(" Val: ", obj != null ? obj.ToString() : "null"));
            sb.Append(String.Concat(" Method Name: ", methodName));
            sb.Append(String.Concat(" Cache Data: ", ToString()));
            Logger.Logger.Log("CacheError", sb.ToString(), logFile);
        }


        #region Pubkic methods

        public Domain GetDomain(int nDomainID)
        {
            Domain oDomain = null;
            BaseModuleCache baseModule;
            try
            {
                string sKey = string.Format("{0}{1}", sKeyCache, nDomainID);

                baseModule = this.cache.Get(sKey);
                if (baseModule != null && baseModule.result != null)
                {
                    oDomain = baseModule.result as Domain;
                }
                else
                {
                    bool bInsert = false;
                    VersionModuleCache versionModule;
                    for (int i = 0; i < 3 && !bInsert; i++)
                    {
                        versionModule = (VersionModuleCache)this.cache.GetWithVersion<Domain>(sKey);

                        if (versionModule != null && versionModule.result != null)
                        {
                            oDomain = baseModule.result as Domain;
                        }
                        else
                        {
                            bool createdNew = false;
                            var mutexSecurity = Utils.CreateMutex();
                            using (Mutex mutex = new Mutex(false, string.Concat("Domain ID", nDomainID), out createdNew, mutexSecurity))
                            {
                                try
                                {
                                    mutex.WaitOne(-1);

                                    Domain tempDomain = Utils.BuildDomain(nDomainID, true);

                                    //try insert to Cache                                     
                                    versionModule.result = tempDomain;
                                    bInsert = this.cache.SetWithVersion<Domain>(sKey, versionModule, dCacheTT);
                                    if (bInsert)
                                    {
                                        oDomain = tempDomain;
                                    }
                                }

                                catch (Exception ex)
                                {
                                    Logger.Logger.Log("GetDomain", string.Format("Couldn't get domain {0}, ex = {1}", nDomainID, ex.Message), DOMAIN_LOG_FILENAME);
                                }
                                finally
                                {
                                    mutex.ReleaseMutex();
                                }
                            }
                        }
                    }
                }
                return oDomain;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("GetDomain", string.Format("Couldn't get domain {0}, ex = {1}", nDomainID, ex.Message), DOMAIN_LOG_FILENAME);
                return null;
            }
        }
        #endregion
    }
}
