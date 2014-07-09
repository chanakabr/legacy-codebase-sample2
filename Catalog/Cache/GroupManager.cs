using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using ApiObjects;
using ApiObjects.Cache;
using DAL;
using Enyim.Caching.Memcached;
using Tvinci.Core.DAL;

namespace Catalog.Cache
{
    public class GroupManager
    {
        private Type cacheGroupType = Type.GetType(Utils.GetWSURL("cache_group_type"));
       
        public GroupManager()
        {         

        }

        #region Public
        public void GetGroupAndChannel(int nChannelId, int nParentGroupId, ref Group group, ref Channel channel)
        {
            group = this.GetGroup(nParentGroupId);

            if (group != null)
            {
                channel = this.GetChannel(nChannelId, ref group);
            }
        }

        public Group GetGroup(int nGroupID)
        {
            Group group = null;
            try
            {
                BaseGroupCache groupCache = GroupCacheUtils.GetGroupCacheInstance(cacheGroupType);
                if (groupCache != null)
                {
                    group = groupCache.GetGroup(nGroupID);                    
                }
                return group;
            }

            #region OLD CODE
            /*
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

                                        Group tempGroup = BuildGroup(nGroupID, true);
                                        if (tempGroup != null)
                                        {
                                            List<int> lSubGroups = Get_SubGroupsTree(nGroupID);
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
             * */
            #endregion

            catch (Exception ex)
            {
                Logger.Logger.Log("GetGroup", string.Format("failed get group from IChach with nGroupID={0}, ex={1}", nGroupID, ex.Message), "Catalog");
                return null;
            }
        }
               
        public bool AddChannelsToOperator(int nOperatorID, List<long> subscriptionChannels, Group group)
        {
            bool bAdd = false;
            try
            {
                BaseGroupCache groupCache = GroupCacheUtils.GetGroupCacheInstance(cacheGroupType);
                if (groupCache != null)
                {
                    bAdd = groupCache.AddChannelsToOperator(nOperatorID, subscriptionChannels, group);
                }
                return bAdd;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("AddChannelsToOperator", string.Format("failed AddChannelsToOperator nOperatorID={0}, ex={1}", nOperatorID, ex.Message), "Catalog");
                return false;
            }

        }

        public List<long> GetOperatorChannelIDs(int nGroupID, int nOperatorID)
        {

            if (Utils.IsGroupIDContainedInConfig(nGroupID, "GroupIDsWithIPNOFilteringSeperatedBySemiColon", ';'))
            {
                // group has ipnos
                Group group = GetGroup(nGroupID);
                if (group != null)
                {
                    List<long> operatorChannelIDs = group.GetOperatorChannelIDs(nOperatorID);

                    return operatorChannelIDs;
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

        public List<int> GetSubGroup(int nGroupID)
        {
            try
            {
                Group group = null;

                //get group by id 

                group = this.GetGroup(nGroupID);

                if (group != null)
                {
                    return group.m_nSubGroup;
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("GetSubGroup", string.Format("failed get subgroup List from IChach with nGroupID={0}, ex={1}", nGroupID, ex.Message), "Catalog");
                return null;
            }
        }

        public bool RemoveChannel(int nGroupID, int nChannelId)
        {
            bool isRemovingChannelSucceded = false;

            try
            {
                BaseGroupCache groupCache = GroupCacheUtils.GetGroupCacheInstance(cacheGroupType);
                if (groupCache != null)
                {
                    isRemovingChannelSucceded = groupCache.RemoveChannel(nGroupID, nChannelId);
                }
                return isRemovingChannelSucceded;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("GetGroup", string.Format("failed get group from IChach with nGroupID={0}, ex={1}", nGroupID, ex.Message), "Catalog");
                return false;
            }
        }

        public bool RemoveGroup(int nGroupID)
        {
            bool bDelete = false;
            try
            {
                BaseGroupCache groupCache = GroupCacheUtils.GetGroupCacheInstance(cacheGroupType);
                if (groupCache != null)
                {
                    bDelete = groupCache.RemoveGroup(nGroupID);
                }
                return bDelete;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("RemoveGroup", string.Format("failed to delete Group with nGroupID={0}, ex={1}", nGroupID, ex.Message), "Catalog");
                return false;
            }
        }

        public bool HandleOperatorEvent(int nGroupID, int nOperatorID, int nSubscriptionID, long lChannelID, eOperatorEvent oe)
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

        #endregion

        #region Private
        private bool HandleChannelAddedToSubscription(int nGroupID, int nSubscriptionID, long lChannelID)
        {
            bool res = true;
            List<int> operators = CatalogDAL.Get_OperatorsOwningSubscription(nGroupID, nSubscriptionID);
            if (operators!= null && operators.Count > 0)
            {
                for (int i = 0; i < operators.Count; i++)
                {                  
                    res &= AddChannelsToOperator(nGroupID, operators[i], new List<long>(1) { lChannelID });
                }             
            }

            return res;
        }

        private bool HandleSubscriptionAddedToOperator(int nGroupID, int nOperatorID, int nSubscriptionID)
        {
            bool res = true;
            List<long> subscriptionChannels = PricingDAL.Get_SubscriptionChannelIDs(nGroupID, nSubscriptionID, "pricing_connection");
            if (subscriptionChannels != null && subscriptionChannels.Count > 0)
            {
                Group group = GetGroup(nGroupID);

                if (group != null)
                {                   
                    res = AddChannelsToOperator(nGroupID, nOperatorID, subscriptionChannels); ;
                }
            }

            return res;
        }

        private bool HandleRemoval(int nGroupID, int nOperatorID)
        {
            return DeleteOperator(nGroupID, nOperatorID);         
        }
        
        private Channel GetChannel(int nChannelId, ref Group group)
        {
            try
            {
                Channel channel = null;
                if (group.m_oGroupChannels.ContainsKey(nChannelId))
                {
                    group.m_oGroupChannels.TryGetValue(nChannelId, out channel);
                    return channel;
                }
                else  //Build the Channel and update the group
                {
                    BaseGroupCache groupCache = GroupCacheUtils.GetGroupCacheInstance(cacheGroupType);
                    if (groupCache != null)
                    {
                        channel = groupCache.GetChannel(nChannelId, ref group);
                    }
                    return channel;                  
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("GetChannel", string.Format("failed GetChannel nChannelId={0}, ex={1}", nChannelId, ex.Message), "Catalog");
                throw;
            }
        }

        #endregion

        #region Internal
        internal bool UpdateoOperatorChannels(int nGroupID, int nOperatorID, List<long> channelIDs, bool bAddNewOperator)
        {
            bool bUpdate = false;
            try
            {
                BaseGroupCache groupCache = GroupCacheUtils.GetGroupCacheInstance(cacheGroupType);
                if (groupCache != null)
                {
                    bUpdate = groupCache.UpdateoOperatorChannels(nGroupID, nOperatorID, channelIDs, true);
                }
                return bUpdate;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("UpdateoOperatorChannels", string.Format("failed to update operatorChannels to IChach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), "Catalog");
                return false;
            }  
        }

        internal bool DeleteOperator(int nGroupID, int nOperatorID)
        {
            bool bDelete = false;
            try
            {
                BaseGroupCache groupCache = GroupCacheUtils.GetGroupCacheInstance(cacheGroupType);
                if (groupCache != null)
                {
                    bDelete = groupCache.DeleteOperator(nGroupID, nOperatorID);
                }
                return bDelete;               
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        internal bool AddChannelsToOperator(int nGroupID, int nOperatorID, List<long> channelIDs, bool bAddNewOperator = false)
        {
            bool bAdd = false;
            try
            {
                BaseGroupCache groupCache = GroupCacheUtils.GetGroupCacheInstance(cacheGroupType);
                if (groupCache != null)
                {
                    bAdd = groupCache.AddOperatorChannels(nGroupID, nOperatorID, channelIDs, bAddNewOperator);
                }
                return bAdd;
            }          
            catch (Exception ex)
            {
                Logger.Logger.Log("UpdateoOperatorChannels", string.Format("failed to update operatorChannels to IChach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), "Catalog");
                return false;
            }
        }

        internal bool InsertChannels(List<Channel> lNewCreatedChannels, int nGroupID)
        {
            bool bInsert = false;
            try
            {
                BaseGroupCache groupCache = GroupCacheUtils.GetGroupCacheInstance(cacheGroupType);
                if (groupCache != null)
                {
                    bInsert = groupCache.InsertChannels(lNewCreatedChannels, nGroupID);
                }
                return bInsert;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion
    }
}
