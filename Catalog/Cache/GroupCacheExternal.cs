using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ApiObjects.Cache;
using Enyim.Caching.Memcached;

namespace Catalog.Cache
{
    public class GroupCacheExternal : BaseGroupCache
    {
        private int GROUP_CACHE_EXPIRY = ODBCWrapper.Utils.GetIntSafeVal(Utils.GetWSURL("cache_doc_expiry"));        
        private ConcurrentDictionary<int, ReaderWriterLockSlim> m_oLockers; // readers-writers lockers for operator channel ids.
        private static volatile GroupCacheExternal instance = null;
        private static object syncRoot = new Object(); // lock for create the instance
         #region CTOR


        public static GroupCacheExternal Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new GroupCacheExternal();
                    }
                }

                return instance;
            }
        }


        private GroupCacheExternal()
        { 
            this.m_oLockers = new ConcurrentDictionary<int, ReaderWriterLockSlim>();
        }

        #endregion

        #region Public Methods
        public override Group GetGroup(int nGroupID)
        {
            Group group = null;
           
            try
            {
                //get group by id from CB
                ICache<Group> cache = Bootstrapper.GetInstance<ICache<Group>>();
                cache.Init();
                group = cache.Get(nGroupID.ToString());
                if (group != null)
                {
                    return group;
                }

                else //Group dosn't exsits ==> Build it 
                {
                    bool bInsert = false;

                    if (cache.GetType() == typeof(CouchBaseCacheWrapper<Group>))
                    {
                        for (int i = 0; i < 3 && !bInsert; i++)
                        {
                            CouchBaseCacheWrapper<Group> cbCache = cache as CouchBaseCacheWrapper<Group>;
                            CasResult<Group> casResult = cbCache.GetWithCas(nGroupID.ToString());

                            if (casResult.StatusCode == 0 && casResult.Result != null)
                            {
                                group = casResult.Result;
                            }
                            else
                            {
                                bool createdNew = false;
                                var mutexSecurity = Utils.CreateMutex();
                                using (Mutex mutex = new Mutex(false, string.Concat("Group GID_", nGroupID), out createdNew, mutexSecurity))
                                {
                                    try
                                    {
                                        mutex.WaitOne(-1);

                                        Group tempGroup = GroupCacheUtils.BuildGroup(nGroupID, true);
                                        if (tempGroup != null)
                                        {
                                            List<int> lSubGroups = GroupCacheUtils.Get_SubGroupsTree(nGroupID);
                                            tempGroup.m_nSubGroup = lSubGroups;
                                        }
                                        //try insert to CB
                                        bInsert = cache.Insert(nGroupID.ToString(), tempGroup, DateTime.UtcNow.AddDays(GROUP_CACHE_EXPIRY), casResult.Cas);
                                        if (bInsert)
                                        {
                                            group = tempGroup;
                                        }
                                    }

                                    catch (Exception ex)
                                    {
                                        Logger.Logger.Log("GetGroup", string.Format("Couldn't get group {0}, ex = {1}", nGroupID, ex.Message), "Catalog");
                                    }
                                    finally
                                    {
                                        mutex.ReleaseMutex();
                                    }
                                }
                            }
                        }
                    }
                }

                return group;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("GetGroup", string.Format("failed get group from IChach with nGroupID={0}, ex={1}", nGroupID, ex.Message), "Catalog");
                return null;
            }
        }

        public override Channel GetChannel(int nChannelId, ref Group group)
        {
            try
            {
                Channel channel = null;

                bool bInsert = false;
                //get group by id from CB
                ICache<Group> cache = Bootstrapper.GetInstance<ICache<Group>>();
                cache.Init();
                if (cache.GetType() == typeof(CouchBaseCacheWrapper<Group>))
                {
                    for (int i = 0; i < 3 && !bInsert; i++)
                    {
                        CouchBaseCacheWrapper<Group> cbCache = cache as CouchBaseCacheWrapper<Group>;
                        CasResult<Group> casResult = cbCache.GetWithCas(group.m_nParentGroupID.ToString());

                        if (casResult.StatusCode == 0)
                        {
                            group.m_oGroupChannels.TryGetValue(nChannelId, out channel);
                            if (channel != null)
                            {
                                return channel;
                            }
                            else
                            {
                                //Build the new Channel
                                Group tempGroup = casResult.Result;
                                Channel tempChannel = ChannelRepository.GetChannel(nChannelId, tempGroup);
                                if (tempChannel != null)
                                {
                                    //try insert to CB
                                    tempGroup.m_oGroupChannels.TryAdd(nChannelId, tempChannel);
                                    bInsert = cache.Update(group.m_nParentGroupID.ToString(), tempGroup, DateTime.UtcNow.AddDays(GROUP_CACHE_EXPIRY), casResult.Cas);
                                    if (bInsert)
                                    {
                                        group = tempGroup;
                                    }
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
                Logger.Logger.Log("GetChannel", string.Format("failed GetChannel nChannelId={0}, ex={1}", nChannelId, ex.Message), "Catalog");
                throw;
            }
        }

        public override bool DeleteOperator(int nGroupID, int nOperatorID)
        {
            try
            {
                bool bDelete = false;
                Group group = null;
                //get group by id from CB
                ICache<Group> cache = Bootstrapper.GetInstance<ICache<Group>>();
                cache.Init();
                if (cache.GetType() == typeof(CouchBaseCacheWrapper<Group>))
                {
                    for (int i = 0; i < 3 && !bDelete; i++)
                    {
                        CouchBaseCacheWrapper<Group> cbCache = cache as CouchBaseCacheWrapper<Group>;
                        CasResult<Group> casResult = cbCache.GetWithCas(nGroupID.ToString());

                        if (casResult.StatusCode == 0 && casResult.Result != null)
                        {
                            group = casResult.Result;
                            //try update to CB
                            if (group.DeleteOperatorCache(nOperatorID))
                            {
                                bDelete = cache.Update(nGroupID.ToString(), group, DateTime.UtcNow.AddDays(GROUP_CACHE_EXPIRY), casResult.Cas);
                            }
                        }
                    }
                }
                return bDelete;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("UpdateoOperatorChannels", string.Format("failed to update operatorChannels to IChach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), "Catalog");
                return false;
            }
        }

        public override bool RemoveChannel(int nGroupId, int nChannelId)
        {
            bool isRemovingChannelSucceded = false;
            try
            {
                Group group = null;
                Channel removedChannel = null;
                ICache<Group> cache = Bootstrapper.GetInstance<ICache<Group>>();
                cache.Init();

                if (cache.GetType() == typeof(CouchBaseCacheWrapper<Group>))
                {
                    for (int i = 0; i < 3 && !isRemovingChannelSucceded; i++)
                    {
                        CouchBaseCacheWrapper<Group> cbCache = cache as CouchBaseCacheWrapper<Group>;
                        CasResult<Group> casResult = cbCache.GetWithCas(nGroupId.ToString());

                        if (casResult.StatusCode == 0 && casResult.Result != null)
                        {
                            group = casResult.Result;
                            if (group != null && group.m_oGroupChannels.ContainsKey(nChannelId))
                            {
                                bool createdNew = false;
                                var mutexSecurity = Utils.CreateMutex();

                                using (Mutex mutex = new Mutex(false, string.Concat("Cache ChannelID_", nChannelId), out createdNew, mutexSecurity))
                                {
                                    mutex.WaitOne(-1);
                                    removedChannel = GroupCacheUtils.RemoveChannelByChannelId(nChannelId, ref group);
                                    if (removedChannel != null)
                                    {
                                        //try update to CB
                                        isRemovingChannelSucceded = cache.Update(nGroupId.ToString(), group, DateTime.UtcNow.AddDays(GROUP_CACHE_EXPIRY), casResult.Cas);
                                    }
                                    mutex.ReleaseMutex();
                                }
                            }
                        }
                    }
                }
                return isRemovingChannelSucceded;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public override bool RemoveGroup(int nGroupID)
        {
            bool isRemovingGroupSucceded = false;
            try
            {
                Group group = null;                
                ICache<Group> cache = Bootstrapper.GetInstance<ICache<Group>>();
                cache.Init();

                for (int i = 0; i < 3 && !isRemovingGroupSucceded; i++)
                {
                    group = cache.Get(nGroupID.ToString());
                    if (group != null)
                    {
                        bool createdNew = false;
                        var mutexSecurity = Utils.CreateMutex();

                        using (Mutex mutex = new Mutex(false, string.Concat("Cache DeleteGroupID_", nGroupID), out createdNew, mutexSecurity))
                        {
                            mutex.WaitOne(-1);
                            //try update to CB
                            isRemovingGroupSucceded = cache.Delete(nGroupID.ToString());
                            mutex.ReleaseMutex();
                        }
                    }
                }
                return isRemovingGroupSucceded;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public override bool AddOperatorChannels(int nGroupID, int nOperatorID, List<long> channelIDs, bool bAddNewOperator = false)
        {
            try
            {
                bool bAdd = false;
                Group group = null;
                //get group by id from CB
                ICache<Group> cache = Bootstrapper.GetInstance<ICache<Group>>();
                cache.Init();
                if (cache.GetType() == typeof(CouchBaseCacheWrapper<Group>))
                {
                    for (int i = 0; i < 3 && !bAdd; i++)
                    {
                        CouchBaseCacheWrapper<Group> cbCache = cache as CouchBaseCacheWrapper<Group>;
                        CasResult<Group> casResult = cbCache.GetWithCas(nGroupID.ToString());

                        if (casResult.StatusCode == 0 && casResult.Result != null)
                        {
                            group = casResult.Result;
                            //try update to CB
                            if (group.AddChannelsToOperatorCache(nOperatorID, channelIDs, bAddNewOperator))
                            {
                                bAdd = cache.Update(nGroupID.ToString(), group, DateTime.UtcNow.AddDays(GROUP_CACHE_EXPIRY), casResult.Cas);
                            }
                        }
                    }
                }
                return bAdd;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("UpdateoOperatorChannels", string.Format("failed to update operatorChannels to IChach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), "Catalog");
                return false;
            }
        }

        public override bool AddChannelsToOperator(int nOperatorID, List<long> channelIDs, Group group)
        {            
            return Add(nOperatorID, channelIDs, group);
        }
        
        public override bool UpdateoOperatorChannels(int nGroupID, int nOperatorID, List<long> channelIDs, bool bAddNewOperator)
        {
            try
            {
                bool bUpdate = false;
                Group group = null;
                //get group by id from CB
                ICache<Group> cache = Bootstrapper.GetInstance<ICache<Group>>();
                cache.Init();
                if (cache.GetType() == typeof(CouchBaseCacheWrapper<Group>))
                {
                    for (int i = 0; i < 3 && !bUpdate; i++)
                    {
                        CouchBaseCacheWrapper<Group> cbCache = cache as CouchBaseCacheWrapper<Group>;
                        CasResult<Group> casResult = cbCache.GetWithCas(nGroupID.ToString());

                        if (casResult.StatusCode == 0 && casResult.Result != null)
                        {
                            group = casResult.Result;
                            //try update to CB
                            if (group.AddChannelsToOperatorCache(nOperatorID, channelIDs, bAddNewOperator))
                            {
                                bUpdate = cache.Update(nGroupID.ToString(), group, DateTime.UtcNow.AddDays(GROUP_CACHE_EXPIRY), casResult.Cas);
                            }
                        }
                    }
                }
                return bUpdate;

            }
            catch (Exception ex)
            {
                Logger.Logger.Log("UpdateoOperatorChannels", string.Format("failed to update operatorChannels to IChach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), "Catalog");
                return false;
            }
        }

        public override bool InsertChannels(List<Channel> lNewCreatedChannels, int nGroupID)
        {
            bool bInsert = false;
            try
            {
                Group group = null;
                //get group by id from CB
                ICache<Group> cache = Bootstrapper.GetInstance<ICache<Group>>();
                cache.Init();
                if (cache.GetType() == typeof(CouchBaseCacheWrapper<Group>))
                {
                    for (int i = 0; i < 3 && !bInsert; i++)
                    {
                        CouchBaseCacheWrapper<Group> cbCache = cache as CouchBaseCacheWrapper<Group>;
                        CasResult<Group> casResult = cbCache.GetWithCas(nGroupID.ToString());

                        if (casResult.StatusCode == 0 && casResult.Result != null)
                        {
                            group = casResult.Result;
                            //try update to CB
                            if (group.AddChannels(nGroupID, lNewCreatedChannels))
                            {
                                bInsert = cache.Update(nGroupID.ToString(), group, DateTime.UtcNow.AddDays(GROUP_CACHE_EXPIRY), casResult.Cas);
                            }
                        }
                    }
                }
                return bInsert;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Private Methods

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
                    GroupManager groupManager = new GroupManager();
                    retVal = groupManager.AddChannelsToOperator(group.m_nParentGroupID, nOperatorID, channelIDs);
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

        #endregion
    }
}
