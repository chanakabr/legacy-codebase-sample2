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
        public void GetGroupAndChannel(int nChannelId, int nGroupId, ref Group group, ref Channel channel)
        {
            group = this.GetGroup(nGroupId);

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
                    int nParentGroupId = GetParentGroup(nGroupID); // get parent group id first 

                    group = groupCache.GetGroup(nParentGroupId);                    
                }
                return group;
            }         

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
                    int nParentGroupId = GetParentGroup(nGroupID); // get parent group id first 
                    isRemovingChannelSucceded = groupCache.RemoveChannel(nParentGroupId, nChannelId);
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
                    int nParentGroupId = GetParentGroup(nGroupID); // get parent group id first 
                    bDelete = groupCache.RemoveGroup(nParentGroupId);
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
                int nParentGroupId = GetParentGroup(nGroupID); // get parent group id first 
                switch (oe)
                {
                    case eOperatorEvent.ChannelAddedToSubscription:
                        {
                            res = HandleChannelAddedToSubscription(nParentGroupId, nSubscriptionID, lChannelID);
                            break;
                        }
                    case eOperatorEvent.SubscriptionAddedToOperator:
                        {
                            res = HandleSubscriptionAddedToOperator(nParentGroupId, nOperatorID, nSubscriptionID);
                            break;
                        }
                    default:
                        {
                            // same logic in removal. since subscriptions are not disjoint, it is hard to calculate the channels
                            // after removal. so we'd better just remove the operator data and let it get initialized in the next call
                            res = HandleRemoval(nParentGroupId, nOperatorID);
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

        private int GetParentGroup(int nGroupID)
        {
            int nParentGroup = 0;
            try
            {
                if (CachingManager.CachingManager.Exist("ParentGroupCache_" + nGroupID.ToString()) == true)
                {
                    nParentGroup = int.Parse(CachingManager.CachingManager.GetCachedData("ParentGroupCache_" + nGroupID.ToString()).ToString());
                }
                else
                {
                    //GetParentGroup
                    nParentGroup = UtilsDal.GetParentGroupID(nGroupID);
                    CachingManager.CachingManager.SetCachedData("ParentGroupCache_" + nGroupID.ToString(), nParentGroup, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                }

                return nParentGroup;
            }
            catch (Exception ex)
            {
                return nGroupID;
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
                    int nParentGroupId = GetParentGroup(nGroupID); // get parent group id first 
                    bUpdate = groupCache.UpdateoOperatorChannels(nParentGroupId, nOperatorID, channelIDs, true);
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
                    int nParentGroupId = GetParentGroup(nGroupID); // get parent group id first 
                    bInsert = groupCache.InsertChannels(lNewCreatedChannels, nParentGroupId);
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
