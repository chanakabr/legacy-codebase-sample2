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
                        locker.ExitWriteLock();
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

                using (Mutex mutex = new Mutex(false, string.Concat("Catalog ChannelID_", nChannelId), out createdNew, mutexSecurity))
                {
                    try
                    {
                        _logger.Info(string.Format("{0} : {1}", "Lock", string.Concat("Catalog ChannelID_", nChannelId)));
                        mutex.WaitOne(-1);
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
                        _logger.Error(string.Format("Couldn't get channel {0}, msg:{1}", nChannelId, ex.Message));
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                        _logger.Info(string.Format("{0} : {1}", "Release", string.Concat("Catalog ChannelID_", nChannelId)));
                    }
                }
            }

            channel = null;
            group.m_oGroupChannels.TryGetValue(nChannelId, out channel);
            _logger.Info("Current Thread finished getting channel");

            return channel;
        }

        public Group GetGroup(int nGroupID)
        {
            Group group = null;
            
            if (!m_GroupByParentGroupId.ContainsKey(nGroupID))
            {
                bool createdNew = false;
                var mutexSecurity = Utils.CreateMutex();

                    using (Mutex mutex = new Mutex(false, string.Concat("Group GID_", nGroupID), out createdNew, mutexSecurity))
                {
                    try
                    {
                        _logger.Info(string.Format("{0} : {1}", "Lock", string.Concat("Group GID_", nGroupID)));
                        mutex.WaitOne(-1);
                        if (!m_GroupByParentGroupId.ContainsKey(nGroupID))
                        {
                            _logger.Info("Entered critical section for building group");

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
                    catch
                    {
                        _logger.Error(string.Format("Couldn't get group {0}", nGroupID));
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                        _logger.Info(string.Format("{0} : {1}", "Release", string.Concat("Group GID_", nGroupID)));
                    }
                }
            }

            group = null;
            m_GroupByParentGroupId.TryGetValue(nGroupID, out group);
            _logger.Info("Current Thread finished getting group");

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

       /* public bool HandleOperatorEvent(int nGroupID, int nOperatorID, int nSubscriptionID, long lChannelID, eOperatorEvent oe)
        {
            bool res = false;
            if (Utils.IsGroupIDContainedInConfig(nGroupID, "GroupIDsWithIPNOFilteringSeperatedBySemiColon", ';'))
            {
                switch (oe)
                {
                    case eOperatorEvent.ChannelAddedToSubscription:
                        {
                            res = HandleChannelAddedToSubscription(nGroupID, nSubscriptionID, lChannelID);
                            break;
                        }
                    case eOperatorEvent.SubscriptionAddedToOperator:
                        {
                            res = HandleSubscriptionAddedToOperator(nGroupID, nOperatorID, nSubscriptionID);
                            break;
                        }
                    default:
                        {
                            // same logic in removal. since subscriptions are not disjoint, it is hard to calculate the channels
                            // after removal. so we'd better just remove the operator data and let it get initialized in the next call
                            res = HandleRemoval(nGroupID, nOperatorID);
                            break;
                        }
                }
            }

            return res;
        }
        */
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

     /*   private bool HandleChannelAddedToSubscription(int nGroupID, int nSubscriptionID, long lChannelID)
        {
            bool res = true;
            List<int> operators = CatalogDAL.Get_OperatorsOwningSubscription(nGroupID, nSubscriptionID);
            if (operators.Count > 0)
            {
                Group group = GetGroup(nGroupID);
                if (group != null)
                {
                    for (int i = 0; i < operators.Count; i++)
                    {
                        res &= group.AddChannelsToOperator(operators[i], new List<long>(1) { lChannelID });
                    }                  
                }
                else
                {
                    res = false;
                }
            }

            return res;
        }*/

     /*   private bool HandleSubscriptionAddedToOperator(int nGroupID, int nOperatorID, int nSubscriptionID)
        {
            bool res = true;
            List<long> subscriptionChannels = PricingDAL.Get_SubscriptionChannelIDs(nGroupID, nSubscriptionID, "pricing_connection");
            if (subscriptionChannels != null && subscriptionChannels.Count > 0)
            {
                Group group = GetGroup(nGroupID);
                if (group != null)
                    res = group.AddChannelsToOperator(nOperatorID, subscriptionChannels);
            }

            return res;
        }
        */
     /*   private bool HandleRemoval(int nGroupID, int nOperatorID)
        {
            Group group = GetGroup(nGroupID);
            if (group != null)
            {
                return group.DeleteOperatorChannels(nOperatorID);
            }
            return false;
        }
        */
      
        private Channel RemoveChannelByChannelId(int nChannelId, ref Group group)
        {
            Channel removedChannel = null;

            if (group.m_oGroupChannels.ContainsKey(nChannelId))
            {
                bool createdNew = false;
                var mutexSecurity = Utils.CreateMutex();

                using (Mutex mutex = new Mutex(false, string.Concat("Catalog ChannelID_", nChannelId), out createdNew, mutexSecurity))
                {
                    try
                    {
                        _logger.Info(string.Format("{0} : {1}", "Lock", string.Concat("Catalog ChannelID_", nChannelId)));
                        mutex.WaitOne(-1);
                        if (group.m_oGroupChannels.ContainsKey(nChannelId))
                        {
                            bool res = group.m_oGroupChannels.TryRemove(nChannelId, out removedChannel);
                        }
                    }
                    catch
                    {
                        _logger.Error(string.Format("Couldn't remove channel {0}", nChannelId));
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                        _logger.Info(string.Format("{0} : {1}", "Release", string.Concat("Catalog ChannelID_", nChannelId)));
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
                        locker.EnterWriteLock();
                        if (lOperator.Contains(nOperatorID))
                        {
                            retVal = group.AddChannelsToOperatorCache(nOperatorID, channelIDs, false);
                        }
                    }
                    else
                    {
                        // no operator in cache. we wait for the next read command that will lazy evaluate initialize the cache.
                        retVal = false;
                    }

                    locker.ExitWriteLock();
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

                        using (Mutex mutex = new Mutex(false, string.Concat("Catalog ChannelID_", channel.m_nChannelID), out createdNew, mutexSecurity))
                        {
                            try
                            {
                                _logger.Info(string.Format("{0} : {1}", "Lock", string.Concat("Catalog ChannelID_", channel.m_nChannelID)));
                                mutex.WaitOne(-1);
                                if (!m_GroupByParentGroupId[groupId].m_oGroupChannels.ContainsKey(channel.m_nChannelID))
                                {
                                    _logger.Info("Entered critical section for inserting channel");
                                    bInsert &= m_GroupByParentGroupId[groupId].m_oGroupChannels.TryAdd(channel.m_nChannelID, channel);                                    
                                }
                            }
                            catch
                            {
                                _logger.Error(string.Format("Couldn't get channel {0}", channel.m_nChannelID));
                                bInsert = false;
                            }
                            finally
                            {
                                mutex.ReleaseMutex();
                                _logger.Info(string.Format("{0} : {1}", "Release", string.Concat("Catalog ChannelID_", channel.m_nChannelID)));
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

        #region OLD CODE
        /* private List<int> Get_SubGroupsTree(int nGroupID)
     {
         List<int> lGroups = new List<int>();

         DataTable dt = DAL.UtilsDal.GetGroupsTree(nGroupID);
         if (dt != null && dt.DefaultView.Count > 0)
         {
             int groupId;
             for (int i = 0; i < dt.DefaultView.Count; i++)
             {
                 groupId = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i], "id");
                 if (groupId != 0)
                     lGroups.Add(groupId);
             }
         }
         return lGroups;
     }*/

        /*private Group BuildGroup(int nGroupID, bool bUseRAM)
      {
          Group group = null;
          try
          {
              DateTime dNow = DateTime.Now;
              _logger.Info(string.Format("{0}:{1} , {2}", "Start Build Group", nGroupID, dNow.ToString()));

              group = ChannelRepository.BuildGroup(nGroupID);

              _logger.Info(string.Format("{0}:{1}", "Finish Build Group ", nGroupID));
          }
          catch (Exception ex)
          {

              string msg = string.Format("{0}:{1}, {2}", "BuildGroup", nGroupID, ex.Message);

              //_logger.Error(msg, ex);
          }

          return group;
      }*/
        #endregion

    }
}
