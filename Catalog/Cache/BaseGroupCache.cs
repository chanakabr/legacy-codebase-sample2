using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Catalog.Cache
{

    public abstract class BaseGroupCache
    {        
        protected const string GROUP_LOG_FILENAME = "Group";

        abstract public Group GetGroup(int nGroupID);
        abstract public bool RemoveChannel(int nGroupId, int nChannelId);
        abstract public Channel GetChannel(int nChannelId, ref Group group);
        abstract public bool UpdateoOperatorChannels(int nGroupID, int nOperatorID, List<long> channelIDs, bool bAddNewOperator);
        abstract public bool DeleteOperator(int nGroupID, int nOperatorID);
        abstract public bool AddOperatorChannels(int nGroupID, int nOperatorID, List<long> channelIDs, bool bAddNewOperator = false);
        abstract public bool InsertChannels(List<Channel> lNewCreatedChannels, int nGroupID);
        abstract public bool AddChannelsToOperator(int nOperatorID, List<long> channelIDs, Group group);
    }
}
