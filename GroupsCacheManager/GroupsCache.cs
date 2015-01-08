using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CachingProvider;
using DAL;
using Enyim.Caching.Memcached;

namespace GroupsCacheManager
{
    public class GroupsCache
    {
        #region Constants
        private static readonly double DEFAULT_TIME_IN_CACHE_MINUTES = 1440d; // 24 hours
        private static readonly string DEFAULT_CACHE_NAME = "GroupsCache";
        protected const string GROUP_LOG_FILENAME = "Group";
        #endregion
              
        #region InnerCache properties
        private static object locker = new object();       
        #endregion

        #region OutOfProcessCache
        private static object syncRoot = new Object();        
        private ConcurrentDictionary<int, ReaderWriterLockSlim> m_oLockers; // readers-writers lockers for operator channel ids.
        #endregion

        private ICachingService CacheService = null;
        private readonly double dCacheTT;
        private string cacheGroupConfiguration = TVinciShared.WS_Utils.GetTcmConfigValue("GroupsCacheConfiguration");
        private string sKeyCache = string.Empty;

        private static GroupsCache instance = null;
        

        private string GetCacheName()
        {
            string res = TVinciShared.WS_Utils.GetTcmConfigValue("GROUPS_CACHE_NAME");
            if (res.Length > 0)
                return res;
            return DEFAULT_CACHE_NAME;
        }

        private double GetDefaultCacheTimeInMinutes()
        {
            double res = 0d;
            string timeStr = TVinciShared.WS_Utils.GetTcmConfigValue("GROUPS_CACHE_TIME_IN_MINUTES");
            if (timeStr.Length > 0 && Double.TryParse(timeStr, out res) && res > 0)
                return res;
            return DEFAULT_TIME_IN_CACHE_MINUTES;
        }

        private void InitializeCachingService(string cacheName, double cachingTimeMinutes)
        {
            this.CacheService = new SingleInMemoryCache(cacheName, cachingTimeMinutes);
        }

        private static double GetDocTTLSettings()
        {
            double nResult;
            if (!double.TryParse(TVinciShared.WS_Utils.GetTcmConfigValue("GroupsCacheDocTimeout"), out nResult))
            {
                nResult = 1440.0;
            }

            return nResult;
        }
        
        private GroupsCache()
        {
            switch (cacheGroupConfiguration)
            {
                case "CouchBase":
                    {
                        CacheService = CouchBaseCache<Group>.GetInstance("CACHE");
                        this.m_oLockers = new ConcurrentDictionary<int, ReaderWriterLockSlim>();
                        dCacheTT = GetDocTTLSettings();     //set ttl time for document 
                        break;
                    }
                case "InnerCache":
                    {   
                        dCacheTT = GetDefaultCacheTimeInMinutes();
                        InitializeCachingService(GetCacheName(), dCacheTT);
                        sKeyCache = "GroupCache_"; // the key for cache in the inner memory is an Integration between this string and groupID 
                        break;
                    }
            }
        }

        public static GroupsCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new GroupsCache();
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
        
        public Group GetGroup(int nGroupID)
        {
            Group group = null;
            BaseModuleCache baseModule;
            try
            {                
                string sKey = string.Format("{0}{1}", sKeyCache , nGroupID);
             
                baseModule = this.CacheService.Get(sKey);
                if (baseModule != null && baseModule.result != null)
                {
                    group = baseModule.result as Group;
                }
                else
                {
                    bool bInsert = false;
                    bool createdNew = false;
                    var mutexSecurity = Utils.CreateMutex();
                    using (Mutex mutex = new Mutex(false, string.Concat("Group GID_", nGroupID), out createdNew, mutexSecurity))
                    {
                        try
                        {
                            mutex.WaitOne(-1);
                            // try to get GRoup from CB 
                            VersionModuleCache versionModule;
                            versionModule = (VersionModuleCache)this.CacheService.GetWithVersion<Group>(sKey);

                            if (versionModule != null && versionModule.result != null)
                            {
                                group = baseModule.result as Group;
                            }

                            else
                            {
                                Group tempGroup = Utils.BuildGroup(nGroupID, true);
                                for (int i = 0; i < 3 && !bInsert; i++)
                                {
                                    //try insert to Cache                                     
                                    versionModule.result = tempGroup;
                                    bInsert = this.CacheService.SetWithVersion<Group>(sKey, versionModule, dCacheTT);
                                    if (bInsert)
                                    {
                                        group = tempGroup;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Logger.Log("GetGroup", string.Format("Couldn't get group {0}, ex = {1}", nGroupID, ex.Message), "GroupsCacheManager");
                        }
                        finally
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }
                
                return group;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("GetGroup", string.Format("Couldn't get group {0}, ex = {1}", nGroupID, ex.Message), "GroupsCacheManager");
                return null;
            }
        }
               
        internal bool AddChannelsToOperator(int nOperatorID, List<long> subscriptionChannels, Group group)
        {
            return Add(nOperatorID, subscriptionChannels, group);
        }

        private bool Add(int nOperatorID, List<long> channelIDs, Group group)
        {
            bool retVal = true;
            ReaderWriterLockSlim locker = null;
            GetLocker(group.m_nParentGroupID, nOperatorID, ref locker);
            List<int> operatorIDs = group.GetAllOperators();

            if (operatorIDs.Contains(nOperatorID))
            {
                if (locker == null)
                {
                    Logger.Logger.Log("Add", string.Format("Add. Failed to obtain locker. Operator ID: {0} , Channel IDs: {1}", nOperatorID, channelIDs.Aggregate<long, string>(string.Empty, (res, item) => String.Concat(res, ";", item))), GROUP_LOG_FILENAME);
                    throw new Exception(string.Format("Add. Cannot retrieve reader writer manager for operator id: {0} , group id: {1}", nOperatorID, group.m_nParentGroupID));
                }
                locker.EnterWriteLock();
                if (operatorIDs.Contains(nOperatorID))
                {
                    retVal = AddChannelsToOperator(group.m_nParentGroupID, nOperatorID, channelIDs);                   
                }
                else
                {
                    // no channel ids in cache. we wait for the next read command that will lazy evaluate initialize the cache.
                    retVal = false;
                }

                locker.ExitWriteLock();
            }
            return retVal;
        }

        private bool AddChannelsToOperator(int nGroupID, int nOperatorID, List<long> channelIDs, bool bAddNewOperator = false)
        {
            VersionModuleCache versionModule;
            bool bAdd = false;
            try
            {
                Group group = null;
                //get group by id from cache
                string sKey = string.Format("{0}{1}", sKeyCache, nGroupID);

                for (int i = 0; i < 3 && !bAdd; i++)
                {
                    versionModule = (VersionModuleCache)this.CacheService.GetWithVersion<Group>(sKey);
                    if (versionModule != null && versionModule.result != null)
                    {
                        group = versionModule.result as Group;
                        //try update to cache
                        if (group.AddChannelsToOperatorCache(nOperatorID, channelIDs, bAddNewOperator))
                        {
                            //try insert to CB                                   
                            versionModule.result = group;
                            bAdd = this.CacheService.SetWithVersion<Group>(nGroupID.ToString(), versionModule, dCacheTT);
                        }
                    }
                }
                return bAdd;
            }
            catch (Exception ex)
            {
                string sChannelsList = string.Join(",", channelIDs);
                Logger.Logger.Log("AddChannelsToOperator",
                    string.Format("fail to add channels to operator group {0}, OperatorID = {1}, sChannelsList = {2}, ex = {3}", nGroupID, nOperatorID, sChannelsList, ex.Message), 
                    "GroupsCacheManager");
                return false;
            }
        }
        
        /*Method that lock group when change the object*/
        private void GetLocker(int nGroupID, int nOperatorID, ref ReaderWriterLockSlim locker)
        {
            if (!m_oLockers.ContainsKey(nOperatorID))
            {
                lock (m_oLockers)
                {
                    Logger.Logger.Log("GetLocker", string.Format("Locked. Operator ID: {0} , Group ID: {1}", nOperatorID, nGroupID), GROUP_LOG_FILENAME);
                    if (!m_oLockers.ContainsKey(nOperatorID))
                    {
                        if (!m_oLockers.TryAdd(nOperatorID, new ReaderWriterLockSlim()))
                        {
                            Logger.Logger.Log("GetLocker", string.Format("Failed to create reader writer manager. operator id: {0} , group_id {1}", nOperatorID, nGroupID), GROUP_LOG_FILENAME);
                        }
                    }
                }
                Logger.Logger.Log("GetLocker", string.Format("Locker released. Operator ID: {0} , Group ID: {1}", nOperatorID, nGroupID), GROUP_LOG_FILENAME);
            }

            if (!m_oLockers.TryGetValue(nOperatorID, out locker))
            {
                Logger.Logger.Log("GetLocker", string.Format("Failed to read reader writer manager. operator id: {0} , group_id: {1}", nOperatorID, nGroupID), GROUP_LOG_FILENAME);
            }
        }

        internal bool UpdateoOperatorChannels(int nGroupID, int nOperatorID, List<long> channelIDs, bool bAddNewOperator = true)
        {
            bool bUpdate = false;
            Group group = null;
            try
            {
                VersionModuleCache versionModule = null;
                string sKey = string.Format("{0}{1}", sKeyCache, nGroupID);
                //get group by id from CB               
                for (int i = 0; i < 3 && !bUpdate; i++)
                {
                    versionModule = (VersionModuleCache)this.CacheService.GetWithVersion<Group>(sKey);
                    if (versionModule != null && versionModule.result != null)
                    {
                        group = versionModule.result as Group;
                        //try update to CB
                        if (group.AddChannelsToOperatorCache(nOperatorID, channelIDs, bAddNewOperator)) 
                        {
                            //try insert to CB                                   
                            versionModule.result = group;
                            bUpdate = this.CacheService.SetWithVersion<Group>(sKey, versionModule, dCacheTT);
                        }
                    }
                }
                return bUpdate;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("UpdateoOperatorChannels", string.Format("failed to update operatorChannels to Chach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), "GroupsCacheManager");
                return false;
            }
        }

        internal Channel GetChannel(int nChannelId, ref Group group)
        {
            bool bInsert = false;
            VersionModuleCache versionModule = null;
            try
            {
                Channel channel = null;
                string sKey = string.Format("{0}{1}", sKeyCache, group.m_nParentGroupID);
                //get group by id from cache

                for (int i = 0; i < 3 && !bInsert; i++)
                {
                    versionModule = (VersionModuleCache)this.CacheService.GetWithVersion<Group>(sKey);

                    if (versionModule != null && versionModule.result != null)
                    {
                        group.m_oGroupChannels.TryGetValue(nChannelId, out channel);
                        if (channel != null)
                        {
                            return channel;
                        }
                        else
                        {
                            //Build the new Channel
                            Group tempGroup = versionModule.result as Group;
                            Channel tempChannel = ChannelRepository.GetChannel(nChannelId, tempGroup);
                            if (tempChannel != null)
                            {
                                //try insert to CB
                                tempGroup.m_oGroupChannels.TryAdd(nChannelId, tempChannel);
                                versionModule.result = tempGroup;
                                bInsert = this.CacheService.SetWithVersion<Group>(sKey, versionModule, dCacheTT);
                                if (bInsert)
                                {
                                    group = tempGroup;
                                }
                            }
                        }
                    }
                }
                channel = null;
                group.m_oGroupChannels.TryGetValue(nChannelId, out channel);

                return channel;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("GetChannel", string.Format("failed GetChannel nChannelId={0}, ex={1}", nChannelId, ex.Message), "GroupsCacheManager");
                return null;
            }
        }

        internal bool RemoveChannel(int nGroupID, int nChannelId)
        {
            bool isRemovingChannelSucceded = false;
            try
            {
                Group group = null;
                Channel removedChannel = null;
                VersionModuleCache vModule  = null;

                string sKey = string.Format("{0}{1}", sKeyCache, nGroupID);
                for (int i = 0; i < 3 && !isRemovingChannelSucceded; i++)
                {
                    vModule = (VersionModuleCache)CacheService.GetWithVersion<Group>(sKey);

                    if (vModule != null && vModule.result != null )
                    {
                        group = vModule.result as Group;
                        if (group != null && group.m_oGroupChannels.ContainsKey(nChannelId))
                        {
                            bool createdNew = false;
                            var mutexSecurity = Utils.CreateMutex();

                            using (Mutex mutex = new Mutex(false, string.Concat("Cache ChannelID_", nChannelId), out createdNew, mutexSecurity))
                            {
                                mutex.WaitOne(-1);
                                removedChannel = Utils.RemoveChannelByChannelId(nChannelId, ref group);
                                if (removedChannel != null)
                                {
                                    //try update to cache
                                    vModule.result = group;
                                    isRemovingChannelSucceded = CacheService.SetWithVersion<Group>(sKey, vModule, dCacheTT);
                                }
                                mutex.ReleaseMutex();
                            }
                        }
                    }
                }
                
                return isRemovingChannelSucceded;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("RemoveChannel", string.Format("failed to Remove channel from group from cache GroupID={0}, ChannelID = {1}, ex={2}", nGroupID, nChannelId, ex.Message), "GroupsCacheManager");
                return false;
            }
        }

        internal bool RemoveGroup(int nGroupID)
        {
            bool isRemovingGroupSucceded = false;
            VersionModuleCache vModule = null;
            try
            {
                Group group = null;

                string sKey = string.Format("{0}{1}", sKeyCache, nGroupID);

                for (int i = 0; i < 3 && !isRemovingGroupSucceded; i++)
                {
                    vModule = (VersionModuleCache)CacheService.GetWithVersion<Group>(sKey);
                    if (vModule != null && vModule.result != null)
                    {
                        group = vModule.result as Group;

                        bool createdNew = false;
                        var mutexSecurity = Utils.CreateMutex();

                        using (Mutex mutex = new Mutex(false, string.Concat("Cache DeleteGroupID_", nGroupID), out createdNew, mutexSecurity))
                        {
                            mutex.WaitOne(-1);
                            //try update to CB
                            BaseModuleCache bModule = CacheService.Remove(sKey);
                            if (bModule != null && bModule.result != null)
                            {
                                isRemovingGroupSucceded = true;
                            }
                            mutex.ReleaseMutex();
                        }
                    }
                }
                return isRemovingGroupSucceded;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("RemoveGroup", string.Format("failed to Remove group from cache GroupID={0}, ex={1}", nGroupID, ex.Message), "GroupsCacheManager");
                return false;
            }
        }

        internal bool AddOperatorChannels(int nGroupID, int nOperatorID, List<long> channelIDs, bool bAddNewOperator)
        {
            try
            {
                bool bAdd = false;
                Group group = null;
                VersionModuleCache vModule = null;
                string sKey = string.Format("{0}{1}", sKeyCache, nGroupID);
                
                //get group by id from Cache
                for (int i = 0; i < 3 && !bAdd; i++)
                {
                    vModule = (VersionModuleCache)CacheService.GetWithVersion<Group>(sKey);

                    if (vModule != null && vModule.result != null)
                    {
                        group = vModule.result as Group;
                        //try update to CB
                        if (group.AddChannelsToOperatorCache(nOperatorID, channelIDs, bAddNewOperator))
                        {
                            vModule.result = group;
                            bAdd = CacheService.SetWithVersion<Group>(sKey, vModule, dCacheTT);
                        }
                    }
                }
                
                return bAdd;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("UpdateoOperatorChannels", string.Format("failed to update operatorChannels to IChach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), "GroupsCacheManager");
                return false;
            }
        }

        internal bool DeleteOperator(int nGroupID, int nOperatorID)
        {
            try
            {
                bool bDelete = false;
                Group group = null;
                VersionModuleCache vModule = null;
                string sKey = string.Format("{0}{1}", sKeyCache, nGroupID);
                
                //get group by id from cache                
                for (int i = 0; i < 3 && !bDelete; i++)
                {
                    vModule = (VersionModuleCache)CacheService.GetWithVersion<Group>(sKey);

                    if (vModule != null && vModule.result != null)
                    {
                        group = vModule.result as Group;
                        //try update to CB
                        if (group.DeleteOperatorCache(nOperatorID))
                        {
                            vModule.result = group;

                            bDelete = CacheService.SetWithVersion<Group>(sKey, vModule, dCacheTT);
                        }
                    }
                }
                
                return bDelete;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("UpdateoOperatorChannels", string.Format("failed to update operatorChannels to IChach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), "GroupsCacheManager");
                return false;
            }
        }

        internal bool InsertChannels(List<Channel> lNewCreatedChannels, int nGroupID)
        {
            bool bInsert = false;
            try
            {
                Group group = null;
                VersionModuleCache vModule = null;
                string sKey = string.Format("{0}{1}", sKeyCache, nGroupID);

                //get group by id from Cache

                for (int i = 0; i < 3 && !bInsert; i++)
                {
                   vModule = (VersionModuleCache)CacheService.GetWithVersion<Group>(sKey);

                   if (vModule != null && vModule.result != null)
                    {
                        group = vModule.result as Group;
                        //try update to CB
                        if (group.AddChannels(nGroupID, lNewCreatedChannels))
                        {
                            vModule.result = group;
                            bInsert = CacheService.SetWithVersion<Group>(sKey, vModule, dCacheTT);
                        }
                    }
                }
                return bInsert;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("InsertChannels", string.Format("failed to InsertChannels to Chach with nGroupID={0}, ex={1}", nGroupID, ex.Message), "GroupsCacheManager");
                return false;
            }
        }
    }
}
