using CachingProvider.LayeredCache;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ApiLogic.Catalog
{
    public interface IWatchRuleManager
    {
        List<string> GetGroupPermittedWatchRules(int groupId);
    }

    public class WatchRuleManager : IWatchRuleManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<WatchRuleManager> lazy = new Lazy<WatchRuleManager>(() => new WatchRuleManager(), LazyThreadSafetyMode.PublicationOnly);

        public static WatchRuleManager Instance { get { return lazy.Value; } }

        public List<string> GetGroupPermittedWatchRules(int groupId)
        {
            List<string> result = null;
            try
            {
                string key = LayeredCacheKeys.GetGroupWatchPermissionRulesKey(groupId);
                string invalidationKey = LayeredCacheKeys.GetGroupWatchPermissionRulesInvalidationKey(groupId);
                if (!LayeredCache.Instance.Get<List<string>>(key, ref result, GetGroupPermittedWatchRules, new Dictionary<string, object>() { { "groupId", groupId } }, groupId,
                                                                LayeredCacheConfigNames.GROUP_WATCH_PERMISSION_RULES_LAYERED_CACHE_CONFIG_NAME, new List<string>() { invalidationKey }))
                {
                    log.ErrorFormat("GetGroupPermittedWatchRules - Couldn't get groupId {0} watch permission rules", groupId);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGroupPermittedWatchRules, groupId: {0}", groupId), ex);
            }

            return result;
        }

        private Tuple<List<string>, bool> GetGroupPermittedWatchRules(Dictionary<string, object> funcParams)
        {
            List<string> watchPermissionRules = null;
            bool res = false;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
                    List<int> lSubGroup = groupManager.GetSubGroup(groupId.Value);
                    watchPermissionRules = new List<string>();
                    DataTable dt = Tvinci.Core.DAL.CatalogDAL.GetPermittedWatchRulesByGroupId(groupId.Value, lSubGroup);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            watchPermissionRules.Add(Core.Catalog.Utils.GetStrSafeVal(dr, "RuleID"));
                        }
                    }
                }

                res = watchPermissionRules != null;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetGroupPermittedWatchRules failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<string>, bool>(watchPermissionRules, res);
        }
    }
}
