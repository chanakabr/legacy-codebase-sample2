using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Catalog.Cache
{
    public class GroupCacheInternal : BaseGroupCache
    {
       

        public GroupCacheInternal()
        {          
        }

        public override Group GetGroup(int nGroupID)
        {
            return GroupsCache.Instance.GetGroup(nGroupID);
        }

        public override bool AddChannelsToOperator(int nOperatorID, List<long> subscriptionChannels, Group group)
        {
            return GroupsCache.Instance.AddChannelsToOperator(group.m_nParentGroupID, nOperatorID, subscriptionChannels);
        }

        public override bool RemoveChannel(int nGroupID, int nChannelId)
        {
            return GroupsCache.Instance.RemoveChannel(nGroupID, nChannelId);
        }

        public override Channel GetChannel(int nChannelId, ref Group group)
        {
            return GroupsCache.Instance.GetChannel(nChannelId, ref group);
        }

        public override bool UpdateoOperatorChannels(int nGroupID, int nOperatorID, List<long> channelIDs, bool p)
        {
            return GroupsCache.Instance.UpdateoOperatorChannels(nGroupID, nOperatorID, channelIDs, false);
        }

        public override bool DeleteOperator(int nGroupID, int nOperatorID)
        {
            return GroupsCache.Instance.DeleteOperator(nGroupID, nOperatorID);
        }

        public override bool AddOperatorChannels(int nGroupID, int nOperatorID, List<long> channelIDs, bool bAddNewOperator/*no need - it's only Because the "wrapper" base class */)
        {
            return GroupsCache.Instance.AddOperatorChannels(nGroupID, nOperatorID, channelIDs);
        }

        public override bool InsertChannels(List<Channel> lNewCreatedChannels, int nGroupID)
        {
            return  GroupsCache.Instance.InsertChannels(lNewCreatedChannels, nGroupID);
        }
    }
}
