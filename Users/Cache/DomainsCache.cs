using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CachingProvider;

namespace Users.Cache
{
    public class DomainsCache
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
        private string sDomainKeyCache = "domain_";
        private string sDLMCache = "DLM_";
        #endregion

        #region Constants
        protected const string DOMAIN_LOG_FILENAME = "DomainCache";
        #endregion

        private static DomainsCache instance = null;

        private static double GetDocTTLSettings()
        {
            double nResult;
            if (!double.TryParse(TVinciShared.WS_Utils.GetTcmConfigValue("DomainCacheDocTimeout"), out nResult))
            {
                nResult = 1440.0;
            }

            return nResult;
        }

        private DomainsCache()
        {
            // create to instanse of cache to domain  external (by CouchBase)
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

        public static DomainsCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new DomainsCache();
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


        #region Public methods

        // try to get domain from cache , if domain don't exsits - build it , thrn insert it to cache only if bInsertToCache == true
        internal Domain GetDomain(int nDomainID, int nGroupID, bool bInsertToCache = true)
        {
            Domain oDomain = null;
            try
            {
                string sKey = string.Format("{0}{1}", sDomainKeyCache, nDomainID);
                // try to get the domain id from cache
                bool bSuccess = this.cache.GetJsonAsT<Domain>(sKey, out oDomain);


                if (!bSuccess || oDomain == null)
                {
                    bool bInsert = false;
                    oDomain = DomainFactory.GetDomain(nGroupID, nDomainID);

                    // if bInsertToCache = true = need to insert the domain to cache 
                    if (bInsertToCache)
                    {
                        for (int i = 0; i < 3 && !bInsert; i++)
                        {
                            //try insert to Cache                                              
                            bInsert = this.cache.SetJson<Domain>(sKey, oDomain, dCacheTT); // set this Domain object anyway - Shouldn't get here if domain already exsits 
                        }
                    }
                }
                else
                {
                    DomainFactory.InitializeDLM(oDomain);
                }

                return oDomain;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("GetDomain", string.Format("Couldn't get domain {0}, ex = {1}", nDomainID, ex.Message), DOMAIN_LOG_FILENAME);
                return null;
            }
        }


        internal bool InsertDomain(Domain domain)
        {
            bool bInsert = false;
            try
            {
                if (domain == null)
                {
                    return false;
                }

                string sKey = string.Format("{0}{1}", sDomainKeyCache, domain.m_nDomainID);

                //try insert to Cache
                for (int i = 0; i < 3 && !bInsert; i++)
                {
                    bInsert = this.cache.SetJson<Domain>(sKey, domain, dCacheTT); // set this Domain object anyway - Shouldn't get here if domain already exsits 
                }
                return bInsert;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("InsertDomain", string.Format("failed insert domain {0}, ex = {1}", domain != null ? domain.m_nDomainID : 0, ex.Message), DOMAIN_LOG_FILENAME);
                return false;
            }
        }

        internal bool RemoveDomain(int nDomainID)
        {
            bool bIsRemove = false;
            try
            {
                string sKey = string.Format("{0}{1}", sDomainKeyCache, nDomainID);

                //try remove domain from cache 
                for (int i = 0; i < 3 && !bIsRemove; i++)
                {
                    BaseModuleCache bModule = cache.Remove(sKey);
                    if (bModule != null && bModule.result != null)
                    {
                        bIsRemove = (bool)bModule.result;
                    }
                }
                return bIsRemove;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("RemoveDomain", string.Format("failed to Remove domain from cache DomainID={0}, ex={1}", nDomainID, ex.Message), DOMAIN_LOG_FILENAME);
                return false;
            }
        }

        #region Users

        internal bool GetUserList(int nDomainID, int nGroupID, Domain oDomain, ref List<int> usersIDs, ref List<int> pendingUsersIDs, ref List<int> masterGUIDs, ref List<int> defaultUsersIDs)
        {
            try
            {
                bool bUsers = false;
                string sKey = string.Format("{0}{1}", sDomainKeyCache, nDomainID);
                bUsers = UsersListFromDomain(nDomainID, oDomain, usersIDs, pendingUsersIDs, masterGUIDs, defaultUsersIDs);

                if (!bUsers) // continue to get the domain 
                {
                    // need to get Domain from cache                 
                    oDomain = GetDomain(nDomainID, nGroupID);
                    bUsers = UsersListFromDomain(nDomainID, oDomain, usersIDs, pendingUsersIDs, masterGUIDs, defaultUsersIDs);
                }
                return bUsers;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("GetFullUserList", string.Format("Couldn't get full users list from domain {0}, ex = {1}", nDomainID, ex.Message), DOMAIN_LOG_FILENAME);
                return false;
            }
        }

        private bool UsersListFromDomain(int nDomainID, Domain oDomain, List<int> usersIDs, List<int> pendingUsersIDs, List<int> masterGUIDs, List<int> defaultUsersIDs)
        {
            try
            {
                if (oDomain != null && oDomain.m_nDomainID == nDomainID)
                {
                    pendingUsersIDs.AddRange(oDomain.m_PendingUsersIDs);
                    usersIDs.AddRange(oDomain.m_UsersIDs);
                    masterGUIDs.AddRange(oDomain.m_DefaultUsersIDs);
                    defaultUsersIDs.AddRange(oDomain.m_DefaultUsersIDs);

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("UsersListFromDomain", string.Format("Couldn't get domain {0}, ex = {1}", nDomainID, ex.Message), DOMAIN_LOG_FILENAME);
                return false;
            }
        }

        #endregion

        #endregion

        /***** don't use - throwing exception due to changes in cache.GetValues *****/
        //internal Dictionary<int, Domain> GetDomains(List<int> dbDomains)
        //{
        //    try
        //    {
        //        IDictionary<string, object> dTempRes = null;
        //        List<string> sDomains = new List<string>();
        //        string sKey = string.Empty;
        //        foreach (int nDomainID in dbDomains)
        //        {
        //            sKey = string.Format("{0}{1}", sDomainKeyCache, nDomainID);
        //            sDomains.Add(sKey);
        //        }

        //        dTempRes = this.cache.GetValues(sDomains, true);
        //        if (dTempRes == null)
        //            return null;
        //        Dictionary<int, Domain> dRes = new Dictionary<int, Domain>();
        //        foreach (KeyValuePair<string, object> obj in dTempRes)
        //        {
        //            string domainKey = obj.Key.Replace(sDomainKeyCache, "");
        //            int domainID = int.Parse(domainKey);
        //            Domain oDomain = (Domain)obj.Value;
        //            dRes.Add(domainID, oDomain);
        //        }
        //        return dRes;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Logger.Log("GetDomains", string.Format("Couldn't get domain {0}, ex = {1}", string.Join(";", dbDomains), ex.Message), DOMAIN_LOG_FILENAME);
        //        return null;
        //    }
        //}

        private static T JsonToObject<T>(string json)
        {
            if (!string.IsNullOrEmpty(json))
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            else
                return default(T);
        }


        internal bool GetDLM(int nDomainLimitID, int nGroupID, out LimitationsManager oLimitationsManager)
        {
            string sKey = string.Empty;
            sKey = string.Format("{0}{1}", sDLMCache, nDomainLimitID);
            oLimitationsManager = null;

            // try to get the DLM id from cache
            bool bSuccess = this.cache.GetJsonAsT<LimitationsManager>(sKey, out oLimitationsManager);

            if (!bSuccess || oLimitationsManager == null)
            {
                bool bInsert = false;
                oLimitationsManager = DomainFactory.GetDLM(nGroupID, nDomainLimitID);

                for (int i = 0; i < 3 && !bInsert; i++)
                {
                    //try insert to Cache                                              
                    bInsert = this.cache.SetJson<LimitationsManager>(sKey, oLimitationsManager, dCacheTT); // set this DLM object anyway
                }
            }
            if (oLimitationsManager != null)
                return true;
            else
                return false;
        }

        internal bool RemoveDLM(int nDlmID)
        {
            bool bIsRemove = false;
            try
            {
                string sKey = string.Format("{0}{1}", sDLMCache, nDlmID);

                //try remove domain from cache 
                for (int i = 0; i < 3 && !bIsRemove; i++)
                {
                    BaseModuleCache bModule = cache.Remove(sKey);
                    if (bModule != null && bModule.result != null)
                    {
                        bIsRemove = (bool)bModule.result;
                    }
                }
                return bIsRemove;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("RemoveDLM", string.Format("failed to Remove domain from cache DomainID={0}, ex={1}", nDlmID, ex.Message), DOMAIN_LOG_FILENAME);
                return false;
            }
        }
    }
}
