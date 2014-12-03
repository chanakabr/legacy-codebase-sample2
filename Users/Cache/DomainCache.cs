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


        #region Public methods

        internal Domain GetDomain(int nDomainID, int nGroupID)
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

                                    Domain tempDomain = DomainFactory.GetDomain(nGroupID, nDomainID);

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

        internal bool InsertDomain(Domain domain)
        {
            BaseModuleCache bModule;
            bool bInsert = false;
            try
            {
                if (domain == null)
                    return false;

                string sKey = string.Format("{0}{1}", sKeyCache, domain.m_nDomainID);
                for (int i = 0; i < 3 && !bInsert; i++)
                {
                    bool createdNew = false;
                    var mutexSecurity = Utils.CreateMutex();
                    using (Mutex mutex = new Mutex(false, string.Concat("domainID_", domain.m_nDomainID), out createdNew, mutexSecurity))
                    {
                        try
                        {
                            mutex.WaitOne(-1);
                            //try insert to Cache                                     
                            bModule = new BaseModuleCache(domain);
                            bInsert = this.cache.Set(sKey, bModule, dCacheTT); // set this Domain object anyway - Shouldn't get here if domain already exsits 
                        }

                        catch (Exception ex)
                        {
                            Logger.Logger.Log("InsertNewDomain", string.Format("failed insert new domianobject with domainID = {0}, ex = {1}", domain != null ? domain.m_nDomainID : 0, ex.Message), DOMAIN_LOG_FILENAME);
                        }
                        finally
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }

                return bInsert;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("InsertDomain", string.Format("failed insert domain {0}, ex = {1}", domain != null ? domain.m_nDomainID : 0, ex.Message), DOMAIN_LOG_FILENAME);
                return false;
            }
        }

        internal bool UpdateDomain(Domain domain)
        {
            VersionModuleCache vModule;
            bool bUpdate = false;
            try
            {
                if (domain == null)
                    return false;

                string sKey = string.Format("{0}{1}", sKeyCache, domain.m_nDomainID);


                for (int i = 0; i < 3 && !bUpdate; i++)
                {
                    vModule = (VersionModuleCache)this.cache.GetWithVersion<Domain>(sKey);

                    if (vModule != null && vModule.result != null)
                    {
                        bool createdNew = false;
                        var mutexSecurity = Utils.CreateMutex();
                        using (Mutex mutex = new Mutex(false, string.Concat("domainID_", domain.m_nDomainID), out createdNew, mutexSecurity))
                        {
                            try
                            {
                                mutex.WaitOne(-1);
                                //try insert to Cache                                     
                                vModule.result = domain;
                                bUpdate = this.cache.SetWithVersion<Domain>(sKey, vModule, dCacheTT); // set this Domain object anyway - Shouldn't get here if domain already exsits 
                            }

                            catch (Exception ex)
                            {
                                Logger.Logger.Log("UpdateDomain", string.Format("failed Update domianobject with domainID = {0}, ex = {1}", domain != null ? domain.m_nDomainID : 0, ex.Message), DOMAIN_LOG_FILENAME);
                            }
                            finally
                            {
                                mutex.ReleaseMutex();
                            }
                        }
                    }
                }

                return bUpdate;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("UpdateDomain", string.Format("failed Update Domain  {0}, ex = {1}", domain != null ? domain.m_nDomainID : 0, ex.Message), DOMAIN_LOG_FILENAME);
                return false;
            }
        }

        internal bool RemoveDomain(int nDomainID)
        {
            bool bIsRemove = false;
            VersionModuleCache vModule = null;
            try
            {
                Domain oDomain = null;

                string sKey = string.Format("{0}{1}", sKeyCache, nDomainID);

                for (int i = 0; i < 3 && !bIsRemove; i++)
                {
                    vModule = (VersionModuleCache)cache.GetWithVersion<Domain>(sKey);
                    if (vModule != null && vModule.result != null)
                    {
                        oDomain = vModule.result as Domain;

                        bool createdNew = false;
                        var mutexSecurity = Utils.CreateMutex();

                        using (Mutex mutex = new Mutex(false, string.Concat("Cache DeleteDomainID_", nDomainID), out createdNew, mutexSecurity))
                        {
                            mutex.WaitOne(-1);
                            //try update to CB
                            BaseModuleCache bModule = cache.Remove(sKey);
                            if (bModule != null && bModule.result != null)
                            {
                                bIsRemove = true;
                            }
                            mutex.ReleaseMutex();
                        }
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

        // return users in status 1 and pending users (status 3)
        internal List<int> GetFullUserList(int nDomainID, int nGroupID, ref Domain oDomain)
        {
            try
            {
                List<int> users = null;
                bool bUsers = false;
                string sKey = string.Format("{0}{1}", sKeyCache, nDomainID);
                bUsers = UsersFullListFromDomain(nDomainID, oDomain, users);

                if (!bUsers) // continue to get the domain 
                {
                    // need to get Domain from CB                 
                    oDomain = GetDomain(nDomainID, nGroupID);
                    bUsers = UsersFullListFromDomain(nDomainID, oDomain, users);
                }
                return users;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("GetFullUserList", string.Format("Couldn't get full users list from domain {0}, ex = {1}", nDomainID, ex.Message), DOMAIN_LOG_FILENAME);
                return null; ;
            }
        }

        internal bool GetUserList(int nDomainID, int nGroupID, ref Domain oDomain, ref List<int> usersIDs, ref List<int> pendingUsersIDs, ref List<int> masterGUIDs, ref List<int> defaultUsersIDs)
        {
            try
            {
                bool bUsers = false;
                string sKey = string.Format("{0}{1}", sKeyCache, nDomainID);
                bUsers = UsersListFromDomain(nDomainID, oDomain, usersIDs, pendingUsersIDs, masterGUIDs, defaultUsersIDs);

                if (!bUsers) // continue to get the domain 
                {
                    // need to get Domain from CB                 
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

        internal bool RemoveUserFromList(int nUserGuid, int nDomainID)
        {
            bool bRemoveUser = false;
            try
            {
                Domain oDomain = null;
                VersionModuleCache vModule = null;

                string sKey = string.Format("{0}{1}", sKeyCache, nDomainID);
                for (int i = 0; i < 3 && !bRemoveUser; i++)
                {
                    vModule = (VersionModuleCache)cache.GetWithVersion<Domain>(sKey);

                    if (vModule != null && vModule.result != null)
                    {
                        oDomain = vModule.result as Domain;
                        if (oDomain != null && oDomain.m_UsersIDs != null && oDomain.m_UsersIDs.Contains(nUserGuid))
                        {
                            bool createdNew = false;
                            var mutexSecurity = Utils.CreateMutex();

                            using (Mutex mutex = new Mutex(false, string.Format("Cache user{0}fromDomain{1}", nUserGuid, nDomainID), out createdNew, mutexSecurity))
                            {
                                mutex.WaitOne(-1);
                                bRemoveUser = oDomain.m_UsersIDs.Remove(nUserGuid);
                                if (bRemoveUser)
                                {
                                    //try update to cache
                                    vModule.result = oDomain;
                                    bRemoveUser = cache.SetWithVersion<Domain>(sKey, vModule, dCacheTT);
                                }
                                mutex.ReleaseMutex();
                            }
                        }
                    }
                }

                return bRemoveUser;

            }
            catch (Exception ex)
            {
                Logger.Logger.Log("RemoveUserFromList", string.Format("Couldn't remove userID={0} from domainID {1}, ex = {2}", nUserGuid, nDomainID, ex.Message), DOMAIN_LOG_FILENAME);
                return false;
            }
        }

        internal bool AddUser(int nUserGuid, int nDomainID, int nMasterUserGuid, UserDomainType userType)
        {
            bool bAddUser = false;
            try
            {
                Domain oDomain = null;
                VersionModuleCache vModule = null;

                string sKey = string.Format("{0}{1}", sKeyCache, nDomainID);
                for (int i = 0; i < 3 && !bAddUser; i++)
                {
                    vModule = (VersionModuleCache)cache.GetWithVersion<Domain>(sKey);

                    if (vModule != null && vModule.result != null)
                    {
                        oDomain = vModule.result as Domain;
                        if (oDomain != null)
                        {
                            bool createdNew = false;
                            var mutexSecurity = Utils.CreateMutex();

                            using (Mutex mutex = new Mutex(false, string.Format("Cache user{0}fromDomain{1}", nUserGuid, nDomainID), out createdNew, mutexSecurity))
                            {
                                mutex.WaitOne(-1);

                                if (!oDomain.m_UsersIDs.Contains(nUserGuid))
                                {
                                    if (oDomain.m_UsersIDs == null)
                                    {
                                        oDomain.m_UsersIDs = new List<int>();
                                    }
                                    oDomain.m_UsersIDs.Add(nUserGuid);
                                }
                                if (!oDomain.m_masterGUIDs.Contains(nUserGuid) && nUserGuid == nMasterUserGuid)
                                {
                                    if (oDomain.m_masterGUIDs == null)
                                    {
                                        oDomain.m_masterGUIDs = new List<int>();
                                    }

                                    oDomain.m_masterGUIDs.Add(nUserGuid);
                                }
                                //try update to cache
                                vModule.result = oDomain;
                                bAddUser = cache.SetWithVersion<Domain>(sKey, vModule, dCacheTT);

                                mutex.ReleaseMutex();
                            }
                        }
                    }
                }

                return bAddUser;

            }
            catch (Exception ex)
            {
                Logger.Logger.Log("AddUser", string.Format("Couldn't add userID={0} to domainID {1}, ex = {2}", nUserGuid, nDomainID, ex.Message), DOMAIN_LOG_FILENAME);
                return false;
            }
        }

        internal bool ChangePendingUserToUser(int nUserGuid, int nDomainID, int nGroupID, int nUserDomainID)
        {
            bool bUpdateUser = false;
            try
            {
                Domain oDomain = null;
                VersionModuleCache vModule = null;

                string sKey = string.Format("{0}{1}", sKeyCache, nDomainID);
                for (int i = 0; i < 3 && !bUpdateUser; i++)
                {
                    vModule = (VersionModuleCache)cache.GetWithVersion<Domain>(sKey);

                    if (vModule != null && vModule.result != null)
                    {
                        oDomain = vModule.result as Domain;
                        if (oDomain != null)
                        {
                            bool createdNew = false;
                            var mutexSecurity = Utils.CreateMutex();

                            using (Mutex mutex = new Mutex(false, string.Format("Cache user{0}fromDomain{1}", nUserGuid, nDomainID), out createdNew, mutexSecurity))
                            {
                                mutex.WaitOne(-1);

                                if (oDomain.m_PendingUsersIDs.Contains(nUserGuid))
                                {
                                    if (oDomain.m_UsersIDs == null)
                                    {
                                        oDomain.m_UsersIDs = new List<int>();
                                    }
                                    oDomain.m_UsersIDs.Add(nUserGuid);
                                }
                                //try update to cache
                                vModule.result = oDomain;
                                bUpdateUser = cache.SetWithVersion<Domain>(sKey, vModule, dCacheTT);

                                mutex.ReleaseMutex();
                            }
                        }
                    }
                }

                return bUpdateUser;

            }
            catch (Exception ex)
            {
                Logger.Logger.Log("ChangePendingUserToUser", string.Format("Couldn't get domain {0}, ex = {1}", nDomainID, ex.Message), DOMAIN_LOG_FILENAME);
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

        private static bool UsersFullListFromDomain(int nDomainID, Domain oDomain, List<int> users)
        {
            try
            {
                if (oDomain != null && oDomain.m_nDomainID == nDomainID)
                {
                    foreach (int pendingUser in oDomain.m_PendingUsersIDs)
                    {
                        users.Add(pendingUser * (-1));
                    }
                    users.AddRange(oDomain.m_UsersIDs);

                    return true;
                }
                return false;

            }
            catch (Exception ex)
            {
                Logger.Logger.Log("UsersFullListFromDomain", string.Format("Couldn't get domain {0}, ex = {1}", nDomainID, ex.Message), DOMAIN_LOG_FILENAME);
                return false;
            }
        }

        #endregion


      
        #endregion
        
    }
}
