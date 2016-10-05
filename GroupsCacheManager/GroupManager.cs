using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using DAL;
using Tvinci.Core.DAL;
using System.Threading;
using KLogMonitor;
using System.Reflection;

namespace GroupsCacheManager
{
    public class GroupManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private GroupsCache cache;
        public GroupManager()
        {
            cache = GroupsCache.Instance();
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
                group = cache.GetGroup(nGroupID);

                return group;
            }

            catch (Exception ex)
            {
                log.Error("GetGroup - " + string.Format("failed get group from IChach with nGroupID={0}, ex={1}", nGroupID, ex.Message), ex);
                return null;
            }
        }

        public bool AddChannelsToOperator(int nOperatorID, List<long> subscriptionChannels, Group group)
        {
            bool bAdd = false;
            try
            {
                bAdd = cache.AddChannelsToOperator(nOperatorID, subscriptionChannels, group);
                return bAdd;
            }
            catch (Exception ex)
            {
                log.Error("AddChannelsToOperator - " + string.Format("failed AddChannelsToOperator nOperatorID={0}, ex={1}", nOperatorID, ex.Message), ex);
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
                log.Error("GetSubGroup - " + string.Format("failed get subgroup List from IChach with nGroupID={0}, ex={1}", nGroupID, ex.Message), ex);
                return null;
            }
        }

        public bool RemoveChannel(int nGroupID, int nChannelId)
        {
            bool isRemovingChannelSucceded = false;

            try
            {
                isRemovingChannelSucceded = cache.RemoveChannel(nGroupID, nChannelId);

                return isRemovingChannelSucceded;
            }
            catch (Exception ex)
            {
                log.Error("GetGroup - " + string.Format("failed get group from IChach with nGroupID={0}, ex={1}", nGroupID, ex.Message), ex);
                return false;
            }
        }

        public bool RemoveGroup(int nGroupID)
        {
            bool bDelete = false;
            try
            {
                bDelete = cache.RemoveGroup(nGroupID);
                return bDelete;
            }
            catch (Exception ex)
            {
                log.Error("RemoveGroup - " + string.Format("failed to delete Group with nGroupID={0}, ex={1}", nGroupID, ex.Message), ex);
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


        public bool AddServices(int nGroupID, List<int> services)
        {
            bool bAdd = false;
            try
            {
                bAdd = cache.AddServices(nGroupID, services);
                return bAdd;
            }
            catch (Exception ex)
            {
                log.Error("AddServices - " + string.Format("failed AddServices nGroupID={0}, ex={1}", nGroupID, ex.Message), ex);
                return false;
            }
        }
        public bool DeleteServices(int nGroupID, List<int> services)
        {
            bool bDelete = false;
            try
            {
                bDelete = cache.DeleteServices(nGroupID, services);
                return bDelete;
            }
            catch (Exception ex)
            {
                log.Error("DeleteServices - " + string.Format("failed DeleteServices nGroupID={0}, ex={1}", nGroupID, ex.Message));
                return false;
            }
        }
        public bool UpdateServices(int nGroupID, List<int> services)
        {
            bool bUpdate = false;
            try
            {
                bUpdate = cache.UpdateServices(nGroupID, services);
                return bUpdate;
            }
            catch (Exception ex)
            {
                log.Error("UpdateServices - " + string.Format("failed UpdateServices nGroupID={0}, ex={1}", nGroupID, ex.Message));
                return false;
            }
        }

        public List<MediaType> GetMediaTypesOfGroup(int groupId)
        {
            List<MediaType> mediaTypes = new List<MediaType>();

            try
            {
                Group group = this.GetGroup(groupId);

                List<int> typeIds = group.GetMediaTypes();

                mediaTypes = cache.GetMediaTypes(typeIds, groupId);
            }
            catch (Exception ex)
            {
                log.Error(
                    "GetMediaTypesOfGroup - " +
                    string.Format("failed get media types of  groupID={0}, ex={1}, ST={2}", groupId, ex.Message, ex.StackTrace),
                    ex);
            }

            return mediaTypes;
        }

        #endregion

        #region Private
        private bool HandleChannelAddedToSubscription(int nGroupID, int nSubscriptionID, long lChannelID)
        {
            bool res = true;
            List<int> operators = CatalogDAL.Get_OperatorsOwningSubscription(nGroupID, nSubscriptionID);
            if (operators != null && operators.Count > 0)
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
                    res = AddChannelsToOperator(nGroupID, nOperatorID, subscriptionChannels);
                }
            }

            return res;
        }

        private bool HandleRemoval(int nGroupID, int nOperatorID)
        {
            return DeleteOperator(nGroupID, nOperatorID);
        }

        public Channel GetChannel(int channelId, ref Group group)
        {
            Channel result = null;

            try
            {
                result = cache.GetChannel(channelId, group);
            }
            catch (Exception ex)
            {
                log.Error("GetChannel - " +
                    string.Format("failed get channel with group ={0}, channel = {1}, ex={2}, ST={3}", group.m_nParentGroupID, channelId, ex.Message, ex.StackTrace),
                    ex);
            }

            return result;
        }


        public List<Channel> GetChannels(List<int> channelIds, int groupId)
        {
            List<Channel> channelsResults = new List<Channel>();

            if (channelIds != null && channelIds.Count > 0)
            {
                Group group = this.GetGroup(groupId);

                foreach (int id in channelIds)
                {
                    Channel currentChannel = cache.GetChannel(id, group);
                    channelsResults.Add(currentChannel);
                }
            }

            return channelsResults;
        }

        #endregion

        #region Internal
        internal bool UpdateoOperatorChannels(int nGroupID, int nOperatorID, List<long> channelIDs, bool bAddNewOperator)
        {
            bool bUpdate = false;
            try
            {
                {
                    bUpdate = cache.UpdateoOperatorChannels(nGroupID, nOperatorID, channelIDs, true);
                }
                return bUpdate;
            }
            catch (Exception ex)
            {
                log.Error("UpdateoOperatorChannels - " + string.Format("failed to update operatorChannels to IChach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), ex);
                return false;
            }
        }

        internal bool DeleteOperator(int nGroupID, int nOperatorID)
        {
            bool bDelete = false;
            try
            {
                bDelete = cache.DeleteOperator(nGroupID, nOperatorID);

                return bDelete;
            }
            catch (Exception ex)
            {
                log.Error("DeleteOperator - " + string.Format("failed to DeleteOperator nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), ex);
                return false;
            }
        }

        internal bool AddChannelsToOperator(int nGroupID, int nOperatorID, List<long> channelIDs, bool bAddNewOperator = false)
        {
            bool bAdd = false;
            try
            {
                bAdd = cache.AddOperatorChannels(nGroupID, nOperatorID, channelIDs, bAddNewOperator);
                return bAdd;
            }
            catch (Exception ex)
            {
                log.Error("UpdateoOperatorChannels - " + string.Format("failed to update operatorChannels to IChach with nGroupID={0}, operator={1}, ex={2}", nGroupID, nOperatorID, ex.Message), ex);
                return false;
            }
        }

        // XXX - Should it be used at all?
        //internal bool InsertChannels(List<Channel> newChannels, Group group)
        //{
        //    bool bInsert = false;
        //    try
        //    {
        //        bInsert = cache.InsertChannels(newChannels, group);

        //        return bInsert;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}
        #endregion

        public bool UpdateRegionalization(int groupID)
        {
            bool isUpdated = false;
            try
            {
                bool isRegionalizationEnabled;
                int defaultRegion;

                CatalogDAL.GetRegionalizationSettings(groupID, out isRegionalizationEnabled, out defaultRegion);

                isUpdated = cache.UpdateRegionalizationData(isRegionalizationEnabled, defaultRegion, groupID);

            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return false;
            }
            return isUpdated;
        }

        public bool UpdateGroup(int groupID)
        {
            bool isUpdated = false;

            try
            {
                Group group = ChannelRepository.BuildGroup(groupID);

                bool isRemoved = cache.RemoveGroup(groupID);

                if (isRemoved)
                {
                    Group newGroup = cache.GetGroup(groupID);

                    if (newGroup == null)
                    {
                        isUpdated = false;
                    }
                    else
                    {
                        isUpdated = true;
                    }
                }

                if (!isUpdated)
                {
                    log.ErrorFormat("Failed upating group {0} in cache", groupID);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed upating group {0} in cache", groupID, ex);
            }

            return isUpdated;
        }
    }
}
