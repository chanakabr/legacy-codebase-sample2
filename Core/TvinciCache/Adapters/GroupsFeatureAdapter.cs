using System;
using System.Threading;
using ApiObjects;

namespace TvinciCache.Adapters
{
    public class GroupsFeatureAdapter : IGroupsFeatures
    {
        private static readonly Lazy<IGroupsFeatures> LazyInstance = new Lazy<IGroupsFeatures>(() => new GroupsFeatureAdapter(), LazyThreadSafetyMode.PublicationOnly);

        public static IGroupsFeatures Instance => LazyInstance.Value;

        public bool GetGroupFeatureStatus(int groupId, GroupFeature groupFeature)
        {
            return GroupsFeatures.GetGroupFeatureStatus(groupId, groupFeature);
        }
    }
}