using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Catalog.Cache
{
    public class GroupCacheUtils
    {
        public static BaseGroupCache GetGroupCacheInstance(Type baseType)
        {
            switch (baseType.Name)
            {
                case "GroupCacheExternal":
                    {
                        return GroupCacheExternal.Instance;
                    }
                case "GroupCacheInternal":
                    {
                        return new GroupCacheInternal(); ;
                    }
                default:
                    {
                        return GroupCacheExternal.Instance;
                    }
            }
        }

        public static Group BuildGroup(int nGroupID, bool bUseRAM)
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

        public static List<int> Get_SubGroupsTree(int nGroupID)
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
        
        public static Channel RemoveChannelByChannelId(int nChannelId, ref Group group)
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
    }
}
