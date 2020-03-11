using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using KLogMonitor;
using ConfigurationManager;

namespace GroupsCacheManager
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static Group BuildGroup(int groupId, bool bUseRAM)
        {
            Group group = null;
            try
            {
                group = ChannelRepository.BuildGroup(groupId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("BuildGroup - failed build group with group id={0}, ex={1}", groupId, ex);
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

    }
}

