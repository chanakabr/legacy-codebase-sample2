using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using CachingProvider.LayeredCache;
using DAL;
using KLogMonitor;
using ODBCWrapper;

namespace Core.GroupManagers
{
    public class SubgroupManager
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<SubgroupManager> GroupsManagerInternal = new Lazy<SubgroupManager>(() => new SubgroupManager(), LazyThreadSafetyMode.PublicationOnly);

        public static SubgroupManager Instance => GroupsManagerInternal.Value;
        
        public GroupInfo[] GetSubGroups(int groupId)
        {
            var result = new GroupInfo[] { };
            var key = LayeredCacheKeys.GetSubGroupsKey(groupId);
            try
            {
                if (!LayeredCache.Instance.Get(key, ref result, GetSubGroups,
                    new Dictionary<string, object> {{"groupId", groupId}}, groupId,
                    LayeredCacheConfigNames.GET_SUB_GROUPS_CACHE_CONFIG))
                {
                    _logger.ErrorFormat("Failed getting GetSubGroups (key : {0}) from LayeredCache, groupId: {1}", key, groupId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Can't fetch subgroups (key: {key}) for groupId: {groupId}", ex);
            }

            return result;
        }
        
        private static Tuple<GroupInfo[], bool> GetSubGroups(IDictionary<string, object> funcParams)
        {
            var result = new GroupInfo[] { };
            try
            {
                object groupIdObj = null;
                if (!(funcParams?.TryGetValue("groupId", out groupIdObj)).GetValueOrDefault())
                {
                    return new Tuple<GroupInfo[], bool>(result, false);
                }

                var groupId = (groupIdObj as long?).GetValueOrDefault();
                return groupId == 0
                    ? new Tuple<GroupInfo[], bool>(result, true)
                    : new Tuple<GroupInfo[], bool>(GetSubGroupsInternal(groupId), true);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("GetAllSubGroups failed params : {0}", funcParams != null ? string.Join(";",
                    funcParams.Select(x => $"key:{x.Key}, value: {x.Value}").ToList()) : string.Empty), ex);
            }

            return new Tuple<GroupInfo[], bool>(result, false);
        }

        private static GroupInfo[] GetSubGroupsInternal(long groupId)
        {
            var dt = ApiDAL.Get_AllSubAccounts(groupId);
            if (dt.Rows.Count == 0)
            {
                return new GroupInfo[] { };
            }

            return dt.Rows.Cast<DataRow>()
                .Select(row => new GroupInfo
                {
                    Id = Utils.GetLongSafeVal(row, "ID"),
                    GroupName = Utils.GetSafeStr(row, "GROUP_NAME"),
                    ParentGroupId = Utils.GetLongSafeVal(row, "PARENT_GROUP_ID")
                }).ToArray();
        }
    }
}