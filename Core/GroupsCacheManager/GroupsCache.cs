using CachingProvider;
using CachingProvider.LayeredCache;
using ConfigurationManager;
using CouchbaseManager;
using KLogMonitor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

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
            cacheGroupConfiguration = ApplicationConfiguration.Current.GroupsCacheConfiguration.Type.Value;
            version = ApplicationConfiguration.Current.Version.Value;

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
            string res = ApplicationConfiguration.Current.GroupsCacheConfiguration.Name.Value;

            if (res.Length > 0)
                return res;
            return DEFAULT_CACHE_NAME;
        }

        private uint GetDefaultCacheTimeInSeconds()
        {
            uint result = (uint)ApplicationConfiguration.Current.GroupsCacheConfiguration.TTLSeconds.Value; ;

            if (result <= 0)
            {
                result = DEFAULT_TIME_IN_CACHE_SECONDS;
            }

            return result;
        }

        private void InitializeCachingService(string cacheName, uint expirationInSeconds)
        {
            this.groupCacheService = SingleInMemoryCache.GetInstance(InMemoryCacheType.General, expirationInSeconds);
            this.channelsCache = SingleInMemoryCache.GetInstance(InMemoryCacheType.General, expirationInSeconds);
        }

        #endregion

        #region Groups Cache

        private static uint GetDocTTLSettings()
        {
            uint result = (uint)ApplicationConfiguration.Current.GroupsCacheConfiguration.TTLSeconds.Value; ;

            if (result <= 0)
            {
                result = DEFAULT_TIME_IN_CACHE_SECONDS;
            }

            return result;
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

        public Group GetGroup(int groupId)
        {
            Group group = null;

            string cacheKey = BuildGroupCacheKey(groupId);
            Dictionary<string, object> funcParams = new Dictionary<string, object>() { { "groupId", groupId } };
            bool result = LayeredCache.Instance.Get<Group>(cacheKey, ref group, BuildGroup, funcParams, groupId,
                LayeredCacheConfigNames.GROUP_MANAGER_GET_GROUP_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GroupManagerGetGroupInvalidationKey(groupId) });

            return group;
        }

        private static Tuple<Group, bool> BuildGroup(Dictionary<string, object> funcParams)
        {
            bool success = false;
            Group group = null;

            if (funcParams != null && funcParams.ContainsKey("groupId"))
            {
                int groupId = Convert.ToInt32(funcParams["groupId"]);
                group = Utils.BuildGroup(groupId, true);

                if (group != null)
                {
                    success = true;
                }
            }

            return new Tuple<Group, bool>(group, success);
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

                bUpdate &= LayeredCache.Instance.InvalidateKeys(new List<string>() { LayeredCacheKeys.GroupManagerGetGroupInvalidationKey(nGroupID) });
                return bUpdate;
            }
            catch (Exception ex)
            {
                log.Error("UpdateoOperatorChannels - " + string.Format("failed to update operatorChannels to Chach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), ex);
                return false;
            }
        }

        internal Channel GetChannel(int channelId, Group group, bool isAlsoInActive = false)
        {
            Channel resultChannel = null;

            try
            {
                List<Channel> channels = GetChannels(new List<int>() { channelId }, group, isAlsoInActive);
                if (channels != null && channels.Count == 1)
                {
                    resultChannel = channels.First();
                }
            }
            catch (Exception ex)
            {
                log.Error("GetChannel - " +
                    string.Format("Couldn't get channel id = {0}, group = {1}, ex = {2}, ST = {3}", channelId, group.m_nParentGroupID, ex.Message, ex.StackTrace),
                    ex);
            }

            return resultChannel;
        }

        internal List<Channel> GetChannels(List<int> channelIds, Group group, bool isAlsoInActive = false)
        {
            {
                List<Channel> channels = new List<Channel>();

                try
                {
                    if (channelIds == null || channelIds.Count == 0)
                    {
                        return channels;
                    }


                    Dictionary<string, Channel> channelMap = null;
                    Dictionary<string, string> keyToOriginalValueMap = LayeredCacheKeys.GetChannelsKeysMap(group.m_nParentGroupID, channelIds);
                    Dictionary<string, List<string>> invalidationKeysMap = LayeredCacheKeys.GetChannelsInvalidationKeysMap(group.m_nParentGroupID, channelIds);
                    Dictionary<string, object> funcParams = new Dictionary<string, object>() { { "group", group }, { "channelIds", channelIds }, { "isAlsoInActive", isAlsoInActive } };

                    if (!LayeredCache.Instance.GetValues<Channel>(keyToOriginalValueMap, ref channelMap, BuildChannels, funcParams, group.m_nParentGroupID,
                                                                    LayeredCacheConfigNames.GET_CHANNELS_CACHE_CONFIG_NAME, invalidationKeysMap))
                    {
                        log.ErrorFormat("Failed getting Channels from LayeredCache, groupId: {0}, channelIds: {1}", group.m_nParentGroupID, string.Join(",", channelIds));
                    }
                    else if (channelMap != null)
                    {
                        channels = channelMap.Values.ToList();
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Failed GetChannels for groupId: {0}, channelIds: {1}", group.m_nParentGroupID, string.Join(",", channelIds)), ex);
                }

                return channels;
            }
        }

        private static Tuple<Dictionary<string, Channel>, bool> BuildChannels(Dictionary<string, object> funcParams)
        {
            bool success = false;
            Dictionary<string, Channel> result = new Dictionary<string, Channel>();

            if (funcParams != null && funcParams.ContainsKey("channelIds") && funcParams.ContainsKey("isAlsoInActive") && funcParams.ContainsKey("group"))
            {
                string key = string.Empty;
                List<int> channelIds;
                Group group = funcParams["group"] as Group;
                bool? isAlsoInActive = funcParams["isAlsoInActive"] as bool?;
                if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                {
                    channelIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => int.Parse(x)).ToList();
                }
                else
                {
                    channelIds = funcParams["channelIds"] != null ? funcParams["channelIds"] as List<int> : null;
                }

                List<Channel> channels = new List<Channel>();
                if (group != null && channelIds != null && isAlsoInActive.HasValue)
                {
                    channels = ChannelRepository.GetChannels(channelIds, group, isAlsoInActive.Value);
                    if (channels != null)
                    {
                        success = true;
                        result = channels.ToDictionary(x => LayeredCacheKeys.GetChannelKey(group.m_nParentGroupID, x.m_nChannelID), x => x);
                    }
                }
            }

            return new Tuple<Dictionary<string, Channel>, bool>(result, success);
        }

        internal bool RemoveChannel(int nGroupID, int nChannelId)
        {
            bool isRemovingChannelSucceded = false;

            try
            {
                string channelKey = BuildChannelCacheKey(nGroupID, nChannelId);

                isRemovingChannelSucceded = LayeredCache.Instance.InvalidateKeys(new List<string>() 
                    { LayeredCacheKeys.GroupManagerGetGroupInvalidationKey(nGroupID), LayeredCacheKeys.GetChannelInvalidationKey(nGroupID, nChannelId) });
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
                        //try update to CB
                        BaseModuleCache bModule = groupCacheService.Remove(sKey);
                        if (bModule != null && bModule.result != null)
                        {
                            isRemovingGroupSucceded = true;
                        }
                    }
                }

                isRemovingGroupSucceded &= LayeredCache.Instance.InvalidateKeys(new List<string>() { LayeredCacheKeys.GroupManagerGetGroupInvalidationKey(nGroupID) });

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

                bAdd &= LayeredCache.Instance.InvalidateKeys(new List<string>() { LayeredCacheKeys.GroupManagerGetGroupInvalidationKey(nGroupID) });

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

                bDelete &= LayeredCache.Instance.InvalidateKeys(new List<string>() { LayeredCacheKeys.GroupManagerGetGroupInvalidationKey(nGroupID) });

                return bDelete;
            }
            catch (Exception ex)
            {
                log.Error("UpdateoOperatorChannels - " + string.Format("failed to update operatorChannels to IChach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), ex);
                return false;
            }
        }

        private string BuildChannelCacheKey(int groupId, int channelId)
        {
            return LayeredCacheKeys.GetChannelKey(groupId, channelId);
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

                bAdd &= LayeredCache.Instance.InvalidateKeys(new List<string>() { LayeredCacheKeys.GroupManagerGetGroupInvalidationKey(nGroupID) });

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

                bDelete &= LayeredCache.Instance.InvalidateKeys(new List<string>() { LayeredCacheKeys.GroupManagerGetGroupInvalidationKey(nGroupID) });

                return bDelete;
            }
            catch (Exception ex)
            {
                log.Error($"DeleteServices - failed to DeleteServices to IChach with nGroupID={nGroupID}, ex={ex.Message}", ex);
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
                log.Error($"UpdateServices - failed to UpdateServices to IChach with nGroupID={nGroupID}, ex={ex.Message}", ex);
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

                isUpdated &= LayeredCache.Instance.InvalidateKeys(new List<string>() { LayeredCacheKeys.GroupManagerGetGroupInvalidationKey(groupID) });

                return isUpdated;
            }
            catch (Exception ex)
            {
                log.Error($"UpdateRegionalizationData - failed to UpdateRegionalizationData to IChach with nGroupID={groupID}, ex={ex.Message}", ex);
                return false;
            }
        }

        internal List<MediaType> GetMediaTypes(List<int> typeIds, int groupId)
        {
            List<MediaType> mediaTypes = new List<MediaType>();

            try
            {
                if (typeIds != null)
                {
                    Group group = this.GetGroup(groupId);

                    if (group != null && group.mediaTypes != null)
                    {
                        mediaTypes = group.mediaTypes.Where(mediaType => typeIds.Contains(mediaType.id)).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetMediaTypes, group = {0}, ex = {1}", groupId, ex);
            }

            return (mediaTypes);
        }

        internal int GetLinearMediaTypeId(int groupId)
        {
            int result = 0;

            try
            {
                Group group = this.GetGroup(groupId);
                if (group != null && group.mediaTypes != null && group.mediaTypes.Any(x => x.isLinear))
                {
                    result = group.mediaTypes.First(x => x.isLinear).id;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in GetLinearMediaType, groupId = {0}", groupId), ex);
            }

            return result;
        }

        #endregion

    }
}
