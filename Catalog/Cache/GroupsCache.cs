using ApiObjects;
using DAL;
using Logger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Tvinci.Core.DAL;
using DalCB;


namespace Catalog.Cache
{
    public class GroupsCache
    {
        private const string GROUP_LOG_FILENAME = "Group";

        #region Members

        private ConcurrentDictionary<int, Group> m_GroupByParentGroupId;
        private static readonly string LOG_FILE = "GroupsCache";
        private static readonly string LOG_HEADER_STATUS = "Status";
        private static readonly string LOG_HEADER_EXCEPTION = "Exception";
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private ConcurrentDictionary<int, ReaderWriterLockSlim> m_oLockers; // readers-writers lockers for operator channel ids.

        #endregion

        #region CTOR

        private GroupsCache()
        {
            m_GroupByParentGroupId = new ConcurrentDictionary<int, Group>();
            this.m_oLockers = new ConcurrentDictionary<int, ReaderWriterLockSlim>();
        }

        #endregion

        #region Singleton

        public static GroupsCache Instance
        {
            get { return Nested.Instance; }
        }

        class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly GroupsCache Instance = new GroupsCache();
        }

        #endregion

        #region Public Methods

        public void GetGroupAndChannel(int nChannelId, int nParentGroupId, ref Group group, ref Channel channel)
        {
            group = this.GetGroup(nParentGroupId);

            if (group != null)
            {
                channel = this.GetChannel(nChannelId, ref group);
            }
        }

        public bool RemoveChannel(int nGroupId, int nChannelId)
        {
            bool isRemovingChannelSucceded = false;

            //// If you want to make it work against prod DB, you must change "groups1" to "groups" ////////////
            int nParentGroupID = ODBCWrapper.Utils.GetIntSafeVal(ODBCWrapper.Utils.GetTableSingleVal("groups", "PARENT_GROUP_ID", nGroupId));

            Group group = null;

            m_GroupByParentGroupId.TryGetValue(nParentGroupID, out group);

            if (group != null)
            {
                Channel removedChannel = RemoveChannelByChannelId(nChannelId, ref group);

                if (removedChannel != null)
                {
                    isRemovingChannelSucceded = true;
                }
            }

            return isRemovingChannelSucceded;
        }

        public bool RemoveGroup(int groupId)
        {
            bool bIsGroupRemoved = false;

            int nParentGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "parent_group_id", groupId, "MAIN_CONNECTION_STRING").ToString());
            if (nParentGroupID == 1)
            {
                nParentGroupID = groupId;
            }

            if (m_GroupByParentGroupId.ContainsKey(nParentGroupID))
            {
                Group group = null;
                bIsGroupRemoved = m_GroupByParentGroupId.TryRemove(nParentGroupID, out group);
                if (bIsGroupRemoved && group != null)
                    group.Dispose();
            }

            return bIsGroupRemoved;
        }

        public bool DeleteOperator(int nGroupID, int nOperatorID)
        {
            bool res = true;
            ReaderWriterLockSlim locker = null;
            GetLocker(nGroupID, nOperatorID, ref locker);
            if (m_GroupByParentGroupId != null && m_GroupByParentGroupId.ContainsKey(nGroupID))
            {
                Group group = m_GroupByParentGroupId[nGroupID];
                if (group != null)
                {
                    List<int> lOperator = group.GetAllOperators();

                    if (lOperator.Contains(nOperatorID))
                    {
                        if (locker == null)
                        {
                            Logger.Logger.Log("Delete", string.Format("Delete. Failed to obtain locker. Operator ID: {0}", nOperatorID), GROUP_LOG_FILENAME);
                            throw new Exception(string.Format("Delete. Cannot retrieve reader writer manager for operator id: {0} , group id: {1}", nOperatorID, nGroupID));
                        }
                        try
                        {
                            locker.EnterWriteLock();
                            if (lOperator.Contains(nOperatorID))
                            {
                                res = group.RemoveOperator(nOperatorID);
                                if (!res)
                                {
                                    // failed to remove from dictionary
                                    Logger.Logger.Log("Delete", string.Format("Failed to remove channel ids from dictionary. Operator ID: {0} , Group ID: {1}", nOperatorID, nGroupID), GROUP_LOG_FILENAME);
                                }
                            }
                        }
                        finally
                        {
                            locker.ExitWriteLock();
                        }
                    }
                }
            }
            return res;
        }

        public Channel GetChannel(int nChannelId, ref Group group)
        {
            Channel channel = null;
            if (!group.m_oGroupChannels.ContainsKey(nChannelId))
            {
                bool createdNew = false;
                var mutexSecurity = Utils.CreateMutex();
                string mutexName = string.Concat("Catalog ChannelID_", nChannelId);
                int threadID = System.Threading.Thread.CurrentThread.ManagedThreadId;

                using (Mutex mutex = new Mutex(false, mutexName, out createdNew, mutexSecurity))
                {
                    try
                    {
                        Logger.Logger.Log(LOG_HEADER_STATUS, String.Concat("GetChannel. Thread ID: ", threadID, " about to wait on mutex: ", mutexName), LOG_FILE);
                        mutex.WaitOne(-1);
                        Logger.Logger.Log(LOG_HEADER_STATUS, String.Concat("GetChannel. Thread ID: ", threadID, " locked mutex: ", mutexName), LOG_FILE);
                        if (!group.m_oGroupChannels.ContainsKey(nChannelId))
                        {
                            _logger.Info("Entered critical section for building channel");

                            Channel tempChannel = ChannelRepository.GetChannel(nChannelId, group);
                            if (tempChannel != null)
                            {
                                bool res = group.m_oGroupChannels.TryAdd(nChannelId, tempChannel);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Log(LOG_HEADER_EXCEPTION, string.Format("Exception at GetChannel. C ID: {0}  Msg: {1} , Type: {2} , ST: {3}", nChannelId, ex.Message, ex.GetType().Name, ex.StackTrace), LOG_FILE);
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                        Logger.Logger.Log(LOG_HEADER_STATUS, String.Concat("GetChannel. Thread ID: ", threadID, " locked mutex: ", mutexName), LOG_FILE);
                    }
                }
            }

            channel = null;
            group.m_oGroupChannels.TryGetValue(nChannelId, out channel);

            return channel;
        }

        public Group GetGroup(int nGroupID)
        {
            Group group = null;

            if (!m_GroupByParentGroupId.ContainsKey(nGroupID))
            {
                bool createdNew = false;
                var mutexSecurity = Utils.CreateMutex();
                string mutexName = string.Concat("Group GID_", nGroupID);
                int threadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
                using (Mutex mutex = new Mutex(false, mutexName, out createdNew, mutexSecurity))
                {
                    try
                    {
                        Logger.Logger.Log(LOG_HEADER_STATUS, String.Concat("GetGroup. Thread ID: ", threadID, " about to wait on mutex: ", mutexName), LOG_FILE);
                        mutex.WaitOne(-1);
                        Logger.Logger.Log(LOG_HEADER_STATUS, String.Concat("GetGroup. Thread ID: ", threadID, " locked mutex: ", mutexName), LOG_FILE);
                        if (!m_GroupByParentGroupId.ContainsKey(nGroupID))
                        {

                            int nParentGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "parent_group_id", nGroupID, "MAIN_CONNECTION_STRING").ToString());
                            if (nParentGroupID == 1)
                            {
                                nParentGroupID = nGroupID;
                            }

                            Group tempGroup = GroupCacheUtils.BuildGroup(nParentGroupID, true);

                            if (tempGroup != null)
                            {
                                List<int> lSubGroups = GroupCacheUtils.Get_SubGroupsTree(nParentGroupID);
                                bool res = true;
                                foreach (int groupId in lSubGroups)
                                {
                                    res &= m_GroupByParentGroupId.TryAdd(groupId, tempGroup);
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Logger.Logger.Log(LOG_HEADER_EXCEPTION, string.Format("Exception at GetGroup. G ID: {0} , Msg: {1} , Type: {2} , ST: {3}", nGroupID, ex.Message, ex.GetType().Name, ex.StackTrace), LOG_FILE);
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                        Logger.Logger.Log(LOG_HEADER_STATUS, String.Concat("GetGroup. Thread ID: ", threadID, " released mutex: ", mutexName), LOG_FILE);
                    }
                }
            }

            group = null;
            m_GroupByParentGroupId.TryGetValue(nGroupID, out group);

            return group;
        }

        public List<long> GetOperatorChannelIDs(int nGroupID, int nOperatorID)
        {
            if (Utils.IsGroupIDContainedInConfig(nGroupID, "GroupIDsWithIPNOFilteringSeperatedBySemiColon", ';'))
            {
                // group has ipnos
                Group group = GetGroup(nGroupID);
                if (group != null)
                {
                    return group.GetOperatorChannelIDs(nOperatorID);
                }
            }

            return new List<long>(0);
        }

        public List<long> GetDistinctAllOperatorsChannels(int nGroupID)
        {
            if (Utils.IsGroupIDContainedInConfig(nGroupID, "GroupIDsWithIPNOFilteringSeperatedBySemiColon", ';'))
            {
                // group has ipnos
                Group group = GetGroup(nGroupID);
                if (group != null)
                {
                    return group.GetAllOperatorsChannelIDs();
                }
            }

            return new List<long>(0);
        }

        public void GetLocker(int nGroupID, int nOperatorID, ref ReaderWriterLockSlim locker)
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

        #endregion

        #region Private Methods

        private Channel RemoveChannelByChannelId(int nChannelId, ref Group group)
        {
            Channel removedChannel = null;

            if (group.m_oGroupChannels.ContainsKey(nChannelId))
            {
                bool createdNew = false;
                var mutexSecurity = Utils.CreateMutex();
                string mutexName = string.Concat("Catalog ChannelID_", nChannelId);
                int threadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
                using (Mutex mutex = new Mutex(false, mutexName, out createdNew, mutexSecurity))
                {
                    try
                    {
                        Logger.Logger.Log(LOG_HEADER_STATUS, String.Concat("RemoveChannelByChannelId. Thread ID: ", threadID, " about to wait on mutex: ", mutexName), LOG_FILE);
                        mutex.WaitOne(-1);
                        Logger.Logger.Log(LOG_HEADER_STATUS, String.Concat("RemoveChannelByChannelId. Thread ID: ", threadID, " locked mutex: ", mutexName), LOG_FILE);
                        if (group.m_oGroupChannels.ContainsKey(nChannelId))
                        {
                            bool res = group.m_oGroupChannels.TryRemove(nChannelId, out removedChannel);
                        }
                    }
                    catch(Exception ex)
                    {
                        Logger.Logger.Log(LOG_HEADER_EXCEPTION, string.Format("Exception at RemoveChannelByChannelId. C ID: {0} , Msg: {1} , Type: {2} , ST: {3}", nChannelId, ex.Message, ex.GetType().Name, ex.StackTrace), LOG_FILE);
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                        Logger.Logger.Log(LOG_HEADER_STATUS, String.Concat("RemoveChannelByChannelId. Thread ID: ", threadID, " locked mutex: ", mutexName), LOG_FILE);
                    }
                }
            }

            return removedChannel;
        }

        private bool Add(int nGroupID, int nOperatorID, List<long> channelIDs)
        {
            bool retVal = true;
            ReaderWriterLockSlim locker = null;
            GetLocker(nGroupID, nOperatorID, ref locker);

            if (m_GroupByParentGroupId != null && m_GroupByParentGroupId.ContainsKey(nGroupID))
            {
                Group group = m_GroupByParentGroupId[nGroupID];
                if (group != null)
                {
                    List<int> lOperator = group.GetAllOperators();
                    if (lOperator.Contains(nOperatorID))
                    {
                        if (locker == null)
                        {
                            Logger.Logger.Log("Add", string.Format("Add. Failed to obtain locker. Operator ID: {0} , Channel IDs: {1}", nOperatorID, channelIDs.Aggregate<long, string>(string.Empty, (res, item) => String.Concat(res, ";", item))), GROUP_LOG_FILENAME);
                            throw new Exception(string.Format("Add. Cannot retrieve reader writer manager for operator id: {0} , group id: {1}", nOperatorID, nGroupID));
                        }
                        try
                        {
                            locker.EnterWriteLock();
                            if (lOperator.Contains(nOperatorID))
                            {
                                retVal = group.AddChannelsToOperatorCache(nOperatorID, channelIDs, false);
                            }
                        }
                        finally
                        {
                            locker.ExitWriteLock();
                        }
                    }
                    else
                    {
                        // no operator in cache. we wait for the next read command that will lazy evaluate initialize the cache.
                        retVal = false;
                    }

                }
            }
            return retVal;
        }
        #endregion


        #region internal

        internal bool UpdateoOperatorChannels(int nGroupID, int nOperatorID, List<long> channelIDs, bool p)
        {
            bool bRes = false;
            if (m_GroupByParentGroupId != null && m_GroupByParentGroupId.ContainsKey(nGroupID))
            {
                Group group = m_GroupByParentGroupId[nGroupID];
                if (group != null)
                {
                    bRes = group.UpdateChannelsToOperator(nOperatorID, channelIDs);
                }
            }
            return bRes;
        }

        internal bool InsertChannels(List<Channel> lChannels, int groupId)
        {
            bool bInsert = false;
            if (lChannels != null)
            {
                foreach (Channel channel in lChannels)
                {
                    if (!m_GroupByParentGroupId[groupId].m_oGroupChannels.ContainsKey(channel.m_nChannelID))
                    {
                        bool createdNew = false;
                        var mutexSecurity = Utils.CreateMutex();
                        string mutexName = string.Concat("Catalog ChannelID_", channel.m_nChannelID);
                        int threadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
                        using (Mutex mutex = new Mutex(false, mutexName, out createdNew, mutexSecurity))
                        {
                            try
                            {
                                Logger.Logger.Log(LOG_HEADER_STATUS, String.Concat("InsertChannels. Thread ID: ", threadID, " about to wait on mutex: ", mutexName), LOG_FILE);
                                mutex.WaitOne(-1);
                                Logger.Logger.Log(LOG_HEADER_STATUS, String.Concat("InsertChannels. Thread ID: ", threadID, " locked mutex: ", mutexName), LOG_FILE);
                                if (!m_GroupByParentGroupId[groupId].m_oGroupChannels.ContainsKey(channel.m_nChannelID))
                                {
                                    bInsert &= m_GroupByParentGroupId[groupId].m_oGroupChannels.TryAdd(channel.m_nChannelID, channel);
                                }
                            }
                            catch(Exception ex)
                            {
                                StringBuilder sb = new StringBuilder(String.Concat("Exception at InsertChannels. Msg: ", ex.Message));
                                sb.Append(String.Concat(" C ID: ", channel != null ? channel.m_nChannelID.ToString() : "null"));
                                sb.Append(String.Concat(" G ID: ", groupId));
                                sb.Append(String.Concat(" Type: ", ex.GetType().Name));
                                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                                Logger.Logger.Log(LOG_HEADER_EXCEPTION, sb.ToString(), LOG_FILE);
                                bInsert = false;
                            }
                            finally
                            {
                                mutex.ReleaseMutex();
                                Logger.Logger.Log(LOG_HEADER_STATUS, String.Concat("InsertChannels. Thread ID: ", threadID, " released mutex: ", mutexName), LOG_FILE);
                            }
                        }
                    }
                }
            }
            return bInsert;
        }

        internal bool AddOperatorChannels(int nGroupID, int nOperatorID, List<long> channelIDs)
        {
            return Add(nGroupID, nOperatorID, channelIDs);
        }

        internal bool AddChannelsToOperator(int nGroupID, int nOperatorID, List<long> channelIDs)
        {
            return Add(nGroupID, nOperatorID, channelIDs);
        }

        #endregion

    }
}
