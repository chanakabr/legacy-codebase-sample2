using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using CachingProvider.LayeredCache;
using KLogMonitor;
using System.Reflection;

namespace TvinciCache
{
    public class GroupsFeatures
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static bool GetGroupFeatureStatus(int groupId, GroupFeature groupFeature)
        {
            bool res = false;
            Dictionary<GroupFeature, bool> groupFeatures = null;
            try
            {
                string key = LayeredCacheKeys.GroupFeaturesKey(groupId);
                if (!LayeredCache.Instance.Get<Dictionary<GroupFeature, bool>>(key, ref groupFeatures, GetGroupFeatures, new Dictionary<string, object>() { { "groupId", groupId } }, groupId,
                                                    LayeredCacheConfigNames.GROUP_FEATURES_LAYERED_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetGroupFeatureInvalidationKey(groupId) }))
                {
                    log.DebugFormat("GetGroupFeatureStatus - Couldn't get groupId {0} features", groupId);
                }
                else if (groupFeatures != null && groupFeatures.ContainsKey(groupFeature))
                {
                    res = groupFeatures[groupFeature];
                }


            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGroupFeatureStatus for groupId: {0}", groupId), ex);
            }

            return res;
        }

        private static Tuple<Dictionary<GroupFeature, bool>, bool> GetGroupFeatures(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<GroupFeature, bool> groupFeatures = null;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    groupFeatures = UtilsDal.GetGroupFeatures(groupId.Value);
                    res = groupFeatures != null;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetUser failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<GroupFeature, bool>, bool>(groupFeatures, res);
        }

    }
}