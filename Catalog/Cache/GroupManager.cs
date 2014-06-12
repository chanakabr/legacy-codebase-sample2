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
        private int GROUP_CACHE_EXPIRY = ODBCWrapper.Utils.GetIntSafeVal(Utils.GetWSURL("cache_doc_expiry"));
        public GroupManager()
        {
        }
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
            try
            {
                Group group = null;
              
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
            catch (Exception ex)
            {
                Logger.Logger.Log("GetGroup", string.Format("failed get group from IChach with nGroupID={0}, ex={1}", nGroupID, ex.Message), "Catalog");
                return null;
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

                //get group by id from CB

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

        public bool RemoveChannel(int nGroupId, int nChannelId)
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
                                    removedChannel = RemoveChannelByChannelId(nChannelId, ref group);
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

        private bool HandleChannelAddedToSubscription(int nGroupID, int nSubscriptionID, long lChannelID)
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
                    res = group.AddChannelsToOperator(nOperatorID, subscriptionChannels);
                }
            }

            return res;
        }

        private bool HandleRemoval(int nGroupID, int nOperatorID)
        {
            Group group = GetGroup(nGroupID);

            if (group != null)
            {
                return group.DeleteOperatorChannels(nOperatorID);
            }
            return false;
        }


        private Channel RemoveChannelByChannelId(int nChannelId, ref Group group)
        {
            Channel removedChannel = null;
            bool isRemovingChannelSucceded = false;

            try
            {
                if (group.m_oGroupChannels.ContainsKey(nChannelId))
                {
                    isRemovingChannelSucceded = group.m_oGroupChannels.TryRemove(nChannelId, out removedChannel);
                }
            }
            catch
            {
                isRemovingChannelSucceded = false;
            }

            return removedChannel;
        }
        
        private List<int> Get_SubGroupsTree(int nGroupID)
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
                    {
                        lGroups.Add(groupId);
                    }
                }                
            }

            return lGroups;
        }

        private Group BuildGroup(int nGroupID, bool bUseRAM)
        {
            Group group = null;
            try
            {
                DateTime dNow = DateTime.Now;

                group = ChannelRepository.BuildGroup(nGroupID);                
            }
            catch (Exception ex)
            {               
                Logger.Logger.Log("BuildGroup", string.Format("failed nGroupIDwith nGroupID={0}, ex={1}", nGroupID, ex.Message), "Catalog");
            }

            return group;
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
                                        bInsert = cache.Insert(group.m_nParentGroupID.ToString(), tempGroup, DateTime.UtcNow.AddDays(GROUP_CACHE_EXPIRY), casResult.Cas);
                                        if (bInsert)
                                        {
                                            group = tempGroup;
                                        }
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




        internal bool UpdateoOperatorChannels(int nGroupID, int nOperatorID, List<long> channelIDs, bool bAddNewOperator) 
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
                Logger.Logger.Log("UpdateoOperatorChannels", string.Format("failed to update operatorChannels to IChach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID,ex.Message), "Catalog");
                return false;
            }
        }

        internal bool DeleteOperator(int nGroupID, int nOperatorID)
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

        internal bool AddOperatorChannels(int nGroupID, int nOperatorID, List<long> channelIDs, bool bAddNewOperator = false)
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
                            if (group.AddChannelsToOperatorCache(nOperatorID, channelIDs,bAddNewOperator))
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

        internal bool InsertChannels(List<Channel> lNewCreatedChannels, int nGroupID)
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
    }
}
