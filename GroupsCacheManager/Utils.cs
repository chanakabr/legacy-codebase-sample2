using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using KLogMonitor;

namespace GroupsCacheManager
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal static MutexSecurity CreateMutex()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            MutexSecurity mutexSecurity = new MutexSecurity();
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.FullControl, AccessControlType.Allow));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.ChangePermissions, AccessControlType.Deny));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.Delete, AccessControlType.Deny));

            return mutexSecurity;
        }

        public static Group BuildGroup(int nGroupID, bool bUseRAM)
        {
            Group group = null;
            try
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();

                Logger.Logger.Log("BuildGroup", string.Format("Started for nGroupID={0}, from ST={1}", nGroupID, st.ToString()), "Catalog");

                DateTime dNow = DateTime.Now;

                group = ChannelRepository.BuildGroup(nGroupID);

            }
            catch (Exception ex)
            {
                log.Error("BuildGroup - " + string.Format("failed nGroupIDwith nGroupID={0}, ex={1}", nGroupID, ex.Message), ex);
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


        public static bool IsGroupIDContainedInConfig(long lGroupID, string sKey, char cSeperator)
        {
            bool res = false;
            string rawStrFromConfig = TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
            if (rawStrFromConfig.Length > 0)
            {
                string[] strArrOfIDs = rawStrFromConfig.Split(cSeperator);
                if (strArrOfIDs != null && strArrOfIDs.Length > 0)
                {
                    List<long> listOfIDs = strArrOfIDs.Select(s =>
                    {
                        long l = 0;
                        if (Int64.TryParse(s, out l))
                            return l;
                        return 0;
                    }).ToList();

                    res = listOfIDs.Contains(lGroupID);
                }
            }

            return res;
        }
    }
}

