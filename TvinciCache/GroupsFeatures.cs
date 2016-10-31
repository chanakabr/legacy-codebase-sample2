using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;

namespace TvinciCache
{
    public class GroupsFeatures
    {

        private static object lck = new object();

        public static bool GetGroupFeatureStatus(int groupId, GroupFeature groupFeature)
        {
            bool res = false;
            string key = string.Format("GroupFeatures_{0}", groupId);
            Dictionary<GroupFeature, bool> groupFeatures = null;

            if (!WSCache.Instance.TryGet(key, out groupFeatures))
            {
                lock (lck)
                {
                    if (!WSCache.Instance.TryGet(key, out groupFeatures))
                    {
                        groupFeatures = UtilsDal.GetGroupFeatures(groupId);
                        if (groupFeatures != null)
                        {
                            TvinciCache.WSCache.Instance.Add(key, groupFeatures);
                        }
                    }
                }
            }

            if (groupFeatures != null && groupFeatures.ContainsKey(groupFeature))
            {
                res = groupFeatures[groupFeature];
            }

            return res;
        }

    }
}