using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CachingProvider;
using DAL;
using ApiObjects;
using CouchbaseManager;
using KLogMonitor;
using System.Reflection;

namespace GroupsCacheManager
{
    public class GroupsCache
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Constants
        private static readonly uint DEFAULT_TIME_IN_CACHE_SECONDS = 86400; // 24 hours
        private static readonly string DEFAULT_CACHE_NAME = "GroupsCache";
        protected const string GROUP_LOG_FILENAME = "Group";
        #endregion

        #region Data members

        #region InnerCache properties
        private static object locker = new object();
        #endregion

        #region OutOfProcessCache
        private static object syncRoot = new Object();
        private ConcurrentDictionary<int, ReaderWriterLockSlim> m_oLockers; // readers-writers lockers for operator channel ids.
        #endregion


        private readonly uint dCacheTT;
        private string cacheGroupConfiguration;
        private string keyCachePrefix = string.Empty;
        private ICachingService groupCacheService = null;
        private ICachingService channelsCache = null;
        private static GroupsCache instance = null;

        private string version;

        #endregion

        #region Ctor

        private GroupsCache()
        {
            cacheGroupConfiguration = TVinciShared.WS_Utils.GetTcmConfigValue("GroupsCacheConfiguration");
            version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");

            switch (cacheGroupConfiguration)
            {
                case "CouchBase":
                    {
                        groupCacheService = CouchBaseCache<Group>.GetInstance("CACHE");
                        channelsCache = CouchBaseCache<Channel>.GetInstance("CACHE");
                        this.m_oLockers = new ConcurrentDictionary<int, ReaderWriterLockSlim>();
                        dCacheTT = GetDocTTLSettings();     //set ttl time for document 
                        break;
                    }
                case "InnerCache":
                    {
                        dCacheTT = GetDefaultCacheTimeInSeconds();
                        InitializeCachingService(GetCacheName(), dCacheTT);
                        keyCachePrefix = "GroupCache_"; // the key for cache in the inner memory is an Integration between this string and groupID 
                        break;
                    }
                case "Hybrid":
                    {
                        dCacheTT = GetDefaultCacheTimeInSeconds();
                        string cacheName = GetCacheName();
                        groupCacheService = HybridCache<Group>.GetInstance(eCouchbaseBucket.CACHE, cacheName);
                        channelsCache = HybridCache<Channel>.GetInstance(eCouchbaseBucket.CACHE, cacheName);

                        break;
                    }
            }
        }

        #endregion

        #region Singleton

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

        #endregion

        #region Initialization

        private string GetCacheName()
        {
            string res = TVinciShared.WS_Utils.GetTcmConfigValue("GROUPS_CACHE_NAME");
            if (res.Length > 0)
                return res;
            return DEFAULT_CACHE_NAME;
        }

        private uint GetDefaultCacheTimeInSeconds()
        {
            uint res = 0;
            string timeStr = TVinciShared.WS_Utils.GetTcmConfigValue("GROUPS_CACHE_TIME_IN_MINUTES");
            if (timeStr.Length > 0 && uint.TryParse(timeStr, out res) && res > 0)
            {
                res *= 60;
                return res;
            }
            return DEFAULT_TIME_IN_CACHE_SECONDS;
        }

        private void InitializeCachingService(string cacheName, uint expirationInSeconds)
        {
            this.groupCacheService = new SingleInMemoryCache(expirationInSeconds);
            this.channelsCache = new SingleInMemoryCache(expirationInSeconds);
        }

        #endregion

        #region Groups Cache

        private static uint GetDocTTLSettings()
        {
            uint nResult;
            if (!uint.TryParse(TVinciShared.WS_Utils.GetTcmConfigValue("GroupsCacheDocTimeout"), out nResult))
            {
                nResult = DEFAULT_TIME_IN_CACHE_SECONDS;
            }                
            else
            {
                // convert to seconds (TCM config is in minutes)
                nResult *= 60;
            }

            return nResult;
        }

        internal void LogCachingError(string msg, string key, object obj, string methodName, string logFile)
        {
            StringBuilder sb = new StringBuilder(msg);
            sb.Append(String.Concat(" Key: ", key));
            sb.Append(String.Concat(" Val: ", obj != null ? obj.ToString() : "null"));
            sb.Append(String.Concat(" Method Name: ", methodName));
            sb.Append(String.Concat(" Cache Data: ", ToString()));
            log.Error("CacheError - " + sb.ToString());
        }

        private string BuildGroupCacheKey(int nGroupID)
        {
            return string.Format("{0}{1}_{2}", keyCachePrefix, version, nGroupID);
        }

        public Group GetGroup(int nGroupID)
        {
            Group group = null;
            BaseModuleCache baseModule = null;
            try
            {
                string cacheKey = BuildGroupCacheKey(nGroupID);

                try
                {
                    baseModule = this.groupCacheService.Get(cacheKey);
                }
                catch (ArgumentException exception)
                {
                    log.Error("GetGroup - " +
                        string.Format("Group in cache was not in expected format. " +
                        "It will be rebuilt now. GroupId = {0}, Exception = {1}", nGroupID, exception.Message), exception);
                }

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

                            VersionModuleCache versionModule = null;

                            try
                            {
                                versionModule = (VersionModuleCache)this.groupCacheService.GetWithVersion<Group>(cacheKey);
                            }
                            catch (ArgumentException exception)
                            {
                                log.Error("GetGroup - " +
                                    string.Format("Group in cache was not in expected format. " +
                                    "It willbe rebuilt now. GroupId = {0}, Exception = {1}", nGroupID, exception.Message), exception);
                            }


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
                                    bInsert = this.groupCacheService.SetWithVersion<Group>(cacheKey, versionModule, dCacheTT);
                                    if (bInsert)
                                    {
                                        group = tempGroup;
                                    }
                                }

                                if (!bInsert)
                                {
                                    log.ErrorFormat("GroupsCache - could not insert group {0} after 3 retries", cacheKey);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("GetGroup - " + string.Format("Couldn't get group {0}, ex = {1}", nGroupID, ex.Message), ex);
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
                log.Error("GetGroup - " + string.Format("Couldn't get group {0}, ex = {1}", nGroupID, ex.Message), ex);
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
                    log.Debug("Add - " + string.Format("Add. Failed to obtain locker. Operator ID: {0} , Channel IDs: {1}", nOperatorID, channelIDs.Aggregate<long, string>(string.Empty, (res, item) => String.Concat(res, ";", item))));
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
                string sKey = BuildGroupCacheKey(nGroupID);

                for (int i = 0; i < 3 && !bAdd; i++)
                {
                    versionModule = (VersionModuleCache)this.groupCacheService.GetWithVersion<Group>(sKey);
                    if (versionModule != null && versionModule.result != null)
                    {
                        group = versionModule.result as Group;
                        //try update to cache
                        if (group.AddChannelsToOperatorCache(nOperatorID, channelIDs, bAddNewOperator))
                        {
                            //try insert to CB                                   
                            versionModule.result = group;
                            bAdd = this.groupCacheService.SetWithVersion<Group>(nGroupID.ToString(), versionModule, dCacheTT);
                        }
                    }
                }
                return bAdd;
            }
            catch (Exception ex)
            {
                string sChannelsList = string.Join(",", channelIDs);
                log.Error("AddChannelsToOperator - " +
                    string.Format("fail to add channels to operator group {0}, OperatorID = {1}, sChannelsList = {2}, ex = {3}", nGroupID, nOperatorID, sChannelsList, ex.Message),
                    ex);
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
                    log.Debug("GetLocker - " + string.Format("Locked. Operator ID: {0} , Group ID: {1}", nOperatorID, nGroupID));
                    if (!m_oLockers.ContainsKey(nOperatorID))
                    {
                        if (!m_oLockers.TryAdd(nOperatorID, new ReaderWriterLockSlim()))
                        {
                            log.Debug("GetLocker - " + string.Format("Failed to create reader writer manager. operator id: {0} , group_id {1}", nOperatorID, nGroupID));
                        }
                    }
                }
                log.Debug("GetLocker - " + string.Format("Locker released. Operator ID: {0} , Group ID: {1}", nOperatorID, nGroupID));
            }

            if (!m_oLockers.TryGetValue(nOperatorID, out locker))
            {
                log.Debug("GetLocker - " + string.Format("Failed to read reader writer manager. operator id: {0} , group_id: {1}", nOperatorID, nGroupID));
            }
        }

        internal bool UpdateoOperatorChannels(int nGroupID, int nOperatorID, List<long> channelIDs, bool bAddNewOperator = true)
        {
            bool bUpdate = false;
            Group group = null;
            try
            {
                VersionModuleCache versionModule = null;
                string sKey = BuildGroupCacheKey(nGroupID);
                //get group by id from CB               
                for (int i = 0; i < 3 && !bUpdate; i++)
                {
                    versionModule = (VersionModuleCache)this.groupCacheService.GetWithVersion<Group>(sKey);
                    if (versionModule != null && versionModule.result != null)
                    {
                        group = versionModule.result as Group;
                        //try update to CB
                        if (group.AddChannelsToOperatorCache(nOperatorID, channelIDs, bAddNewOperator))
                        {
                            //try insert to CB                                   
                            versionModule.result = group;
                            bUpdate = this.groupCacheService.SetWithVersion<Group>(sKey, versionModule, dCacheTT);
                        }
                    }
                }
                return bUpdate;
            }
            catch (Exception ex)
            {
                log.Error("UpdateoOperatorChannels - " + string.Format("failed to update operatorChannels to Chach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), ex);
                return false;
            }
        }

        internal Channel GetChannel(int channelId, Group group)
        {
            Channel resultChannel = null;

            try
            {
                string cacheKey = BuildChannelCacheKey(group.m_nParentGroupID, channelId);

                if (!this.channelsCache.GetJsonAsT<Channel>(cacheKey, out resultChannel))
                {
                    Channel temporaryCahnnel = ChannelRepository.GetChannel(channelId, group);

                    bool wasInserted = false;

                    if (temporaryCahnnel != null)
                    {
                        resultChannel = temporaryCahnnel;

                        for (int i = 0; i < 3 && !wasInserted; i++)
                        {
                            //try insert to cache
                            wasInserted = this.groupCacheService.SetJson<Channel>(cacheKey, temporaryCahnnel, dCacheTT);
                        }

                        if (!wasInserted)
                        {
                            log.DebugFormat("Couldn't set channel in CB channel id = {0}, group = {1}, key = {2}", channelId, group.m_nParentGroupID, cacheKey);
                        }
                    }
                    else
                    {
                        log.DebugFormat("channel is null (DB) channel id = {0}, group = {1}", channelId, group.m_nParentGroupID);
                    }
                }

                return resultChannel;
            }
            catch (Exception ex)
            {
                log.Error("GetChannel - " +
                    string.Format("Couldn't get channel id = {0}, group = {1}, ex = {2}, ST = {3}", channelId, group.m_nParentGroupID, ex.Message, ex.StackTrace),
                    ex);
                return null;
            }
        }

        internal bool RemoveChannel(int nGroupID, int nChannelId)
        {
            bool isRemovingChannelSucceded = false;
            try
            {
                string channelKey = BuildChannelCacheKey(nGroupID, nChannelId);

                var response = channelsCache.Remove(channelKey);

                if (response != null && response.result != null)
                {
                    isRemovingChannelSucceded = true;
                }

            }
            catch (Exception ex)
            {
                log.Error("RemoveChannel - " + string.Format("failed to Remove channel from group from cache GroupID={0}, ChannelID = {1}, ex={2}", nGroupID, nChannelId, ex.Message), ex);
            }

            return isRemovingChannelSucceded;
        }

        internal bool RemoveGroup(int nGroupID)
        {
            bool isRemovingGroupSucceded = false;
            VersionModuleCache vModule = null;
            try
            {
                Group group = null;

                string sKey = BuildGroupCacheKey(nGroupID);

                for (int i = 0; i < 3 && !isRemovingGroupSucceded; i++)
                {
                    vModule = (VersionModuleCache)groupCacheService.GetWithVersion<Group>(sKey);
                    if (vModule != null && vModule.result != null)
                    {
                        group = vModule.result as Group;

                        bool createdNew = false;
                        var mutexSecurity = Utils.CreateMutex();

                        using (Mutex mutex = new Mutex(false, string.Concat("Cache DeleteGroupID_", nGroupID), out createdNew, mutexSecurity))
                        {
                            mutex.WaitOne(-1);
                            //try update to CB
                            BaseModuleCache bModule = groupCacheService.Remove(sKey);
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
                log.Error("RemoveGroup - " + string.Format("failed to Remove group from cache GroupID={0}, ex={1}", nGroupID, ex.Message), ex);
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
                string sKey = BuildGroupCacheKey(nGroupID);

                //get group by id from Cache
                for (int i = 0; i < 3 && !bAdd; i++)
                {
                    vModule = (VersionModuleCache)groupCacheService.GetWithVersion<Group>(sKey);

                    if (vModule != null && vModule.result != null)
                    {
                        group = vModule.result as Group;
                        //try update to CB
                        if (group.AddChannelsToOperatorCache(nOperatorID, channelIDs, bAddNewOperator))
                        {
                            vModule.result = group;
                            bAdd = groupCacheService.SetWithVersion<Group>(sKey, vModule, dCacheTT);
                        }
                    }
                }

                return bAdd;
            }
            catch (Exception ex)
            {
                log.Error("UpdateoOperatorChannels - " + string.Format("failed to update operatorChannels to IChach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), ex);
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
                string sKey = BuildGroupCacheKey(nGroupID);

                //get group by id from cache                
                for (int i = 0; i < 3 && !bDelete; i++)
                {
                    vModule = (VersionModuleCache)groupCacheService.GetWithVersion<Group>(sKey);

                    if (vModule != null && vModule.result != null)
                    {
                        group = vModule.result as Group;
                        //try update to CB
                        if (group.DeleteOperatorCache(nOperatorID))
                        {
                            vModule.result = group;

                            bDelete = groupCacheService.SetWithVersion<Group>(sKey, vModule, dCacheTT);
                        }
                    }
                }

                return bDelete;
            }
            catch (Exception ex)
            {
                log.Error("UpdateoOperatorChannels - " + string.Format("failed to update operatorChannels to IChach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), ex);
                return false;
            }
        }

        // XXX - should it be used at all?
        //internal bool InsertChannels(List<Channel> channels, Group group)
        //{
        //    bool inserted = true;
        //    try
        //    {
        //        foreach (Channel channel in channels)
        //        {                    
        //            bool currentInserted = false;

        //            for (int i = 0; i < 3 && !currentInserted; i++)
        //            {
        //                string key = BuildChannelCacheKey(group.m_nParentGroupID, channel.m_nChannelID);

        //                BaseModuleCache casResult = primaryChannelsCache.GetWithVersion<Channel>(key);

        //                if (casResult != null && casResult.result != null)
        //                {
        //                    casResult.result = channel;

        //                    inserted = primaryChannelsCache.Set(key, casResult);
        //                }
        //                else
        //                {
        //                    inserted = primaryChannelsCache.Add(key, new BaseModuleCache(channel));
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        inserted = false;
        //    }

        //    return inserted;
        //}

        private string BuildChannelCacheKey(int groupId, int channelId)
        {
            return string.Format("{2}_group_{0}_channel_{1}", groupId, channelId, this.version);
        }

        private string BuildMediaTypeCacheKey(int groupId, int mediaType)
        {
            return string.Format("{2}_group_{0}_mediaType_{1}", groupId, mediaType, this.version);
        }

        internal bool AddServices(int nGroupID, List<int> services)
        {
            try
            {
                bool bAdd = false;
                Group group = null;
                VersionModuleCache vModule = null;
                string sKey = BuildGroupCacheKey(nGroupID);

                vModule = (VersionModuleCache)groupCacheService.GetWithVersion<Group>(sKey);
                if (vModule != null && vModule.result != null)
                {
                    group = vModule.result as Group;
                    //get group by id from Cache
                    for (int i = 0; i < 3 && !bAdd; i++)
                    {
                        //try update to CB
                        if (group.AddServices(services))
                        {
                            vModule.result = group;
                            bAdd = groupCacheService.SetWithVersion<Group>(sKey, vModule, dCacheTT);
                        }
                    }
                }

                return bAdd;
            }
            catch (Exception ex)
            {
                log.Error("AddServices - " + string.Format("failed to AddServices to IChach with nGroupID={0}, ex={1}", nGroupID, ex.Message), ex);
                return false;
            }
        }
        internal bool DeleteServices(int nGroupID, List<int> services)
        {
            try
            {
                bool bDelete = false;
                Group group = null;
                VersionModuleCache vModule = null;
                string sKey = BuildGroupCacheKey(nGroupID);
                vModule = (VersionModuleCache)groupCacheService.GetWithVersion<Group>(sKey);
                if (vModule != null && vModule.result != null)
                {
                    group = vModule.result as Group;
                    //get group by id from Cache
                    for (int i = 0; i < 3 && !bDelete; i++)
                    {
                        //try update to CB
                        if (group.RemoveServices(services))
                        {
                            vModule.result = group;
                            bDelete = groupCacheService.SetWithVersion<Group>(sKey, vModule, dCacheTT);
                        }
                    }
                }

                return bDelete;
            }
            catch (Exception ex)
            {
                log.Error("DeleteServices - " + string.Format("failed to DeleteServices to IChach with nGroupID={0}, ex={2}", nGroupID, ex.Message), ex);
                return false;
            }
        }
        internal bool UpdateServices(int nGroupID, List<int> services)
        {
            try
            {
                bool bUpdate = false;
                Group group = null;
                VersionModuleCache vModule = null;
                string sKey = BuildGroupCacheKey(nGroupID);
                vModule = (VersionModuleCache)groupCacheService.GetWithVersion<Group>(sKey);

                if (vModule != null && vModule.result != null)
                {
                    group = vModule.result as Group;
                    //get group by id from Cache
                    for (int i = 0; i < 3 && !bUpdate; i++)
                    {
                        //try update to CB
                        if (group.UpdateServices(services))
                        {
                            vModule.result = group;
                            bUpdate = groupCacheService.SetWithVersion<Group>(sKey, vModule, dCacheTT);
                        }
                    }
                }

                return bUpdate;
            }
            catch (Exception ex)
            {
                log.Error("UpdateServices - " + string.Format("failed to UpdateServices to IChach with nGroupID={0}, ex={2}", nGroupID, ex.Message), ex);
                return false;
            }
        }


        internal bool UpdateRegionalizationData(bool isRegionalizationEnabled, int defaultRegion, int groupID)
        {
            try
            {
                bool isUpdated = false;
                Group group = null;
                VersionModuleCache vModule = null;
                string sKey = BuildGroupCacheKey(groupID);
                vModule = (VersionModuleCache)this.groupCacheService.GetWithVersion<Group>(sKey);

                if (vModule != null && vModule.result != null)
                {
                    group = vModule.result as Group;
                    //get group by id from Cache
                    for (int i = 0; i < 3 && !isUpdated; i++)
                    {
                        group.isRegionalizationEnabled = isRegionalizationEnabled;
                        group.defaultRegion = defaultRegion;
                        vModule.result = group;
                        isUpdated = this.groupCacheService.SetWithVersion<Group>(sKey, vModule, dCacheTT);
                    }
                }

                return isUpdated;
            }
            catch (Exception ex)
            {
                log.Error("UpdateRegionalizationData - " + string.Format("failed to UpdateRegionalizationData to IChach with nGroupID={0}, ex={2}", groupID, ex.Message), ex);
                return false;
            }
        }

        internal List<MediaType> GetMediaTypes(List<int> typeIds, int groupId)
        {
            List<MediaType> mediaTypes = new List<MediaType>();

            HashSet<int> missingTypesList = new HashSet<int>();

            try
            {
                foreach (int typeId in typeIds)
                {
                    MediaType currentType = null;
                    string cacheKey = BuildMediaTypeCacheKey(groupId, typeId);

                    if (!this.groupCacheService.GetJsonAsT<MediaType>(cacheKey, out currentType))
                    {
                        missingTypesList.Add(typeId);
                    }
                    else if (currentType == null)
                    {
                        missingTypesList.Add(typeId);
                    }
                    else
                    {
                        mediaTypes.Add(currentType);
                    }
                }

                if (missingTypesList.Count > 0)
                {
                    List<MediaType> newMediaTypes = ChannelRepository.BuildMediaTypes(missingTypesList.ToList(), groupId);

                    foreach (MediaType newMediaType in newMediaTypes)
                    {
                        int typeId = newMediaType.id;

                        string cacheKey = BuildMediaTypeCacheKey(groupId, typeId);

                        bool wasInsert = false;

                        for (int i = 0; i < 3 && !wasInsert; i++)
                        {
                            wasInsert = this.groupCacheService.SetJson<MediaType>(cacheKey, newMediaType, dCacheTT);
                        }

                        mediaTypes.Add(newMediaType);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }

            return (mediaTypes);
        }

	    #endregion

    }
}
