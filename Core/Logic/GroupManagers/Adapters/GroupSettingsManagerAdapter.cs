using System;
using System.Threading;

namespace Core.GroupManagers.Adapters
{
    public class GroupSettingsManagerAdapter : IGroupSettingsManager
    {
        private static readonly Lazy<IGroupSettingsManager> LazyInternalInstance = new Lazy<IGroupSettingsManager>(() => new GroupSettingsManagerAdapter(), LazyThreadSafetyMode.PublicationOnly);

        public static IGroupSettingsManager Instance => LazyInternalInstance.Value;

        public bool IsOpc(int groupId)
        {
            return GroupSettingsManager.IsOpc(groupId);
        }

        public bool DoesGroupUsesTemplates(int groupId)
        {
            return GroupSettingsManager.DoesGroupUsesTemplates(groupId);
        }

        public bool DoesGroupUseNewEpgIngest(int groupId)
        {
            return GroupSettingsManager.DoesGroupUseNewEpgIngest(groupId);
        }
    }
}