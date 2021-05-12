using System;
using System.Threading;
using ConfigurationManager;
using CouchbaseManager;
using DAL.DTO;

namespace DAL
{
    public interface IGroupBaseConfigurationRepository
    {
        bool SaveConfig(int groupId, GroupDTO group);
        GroupDTO GetConfig(int groupId);
        bool DeleteConfig(int groupId);
        string GetGroupConfigKey(int groupId);
    }

    public class GroupBaseConfigurationRepository : IGroupBaseConfigurationRepository
    {
        private readonly IApplicationConfiguration _applicationConfiguration;

        private static readonly Lazy<GroupBaseConfigurationRepository> Lazy =
            new Lazy<GroupBaseConfigurationRepository>(() => new GroupBaseConfigurationRepository(ApplicationConfiguration.Current),
                LazyThreadSafetyMode.PublicationOnly);

        public static IGroupBaseConfigurationRepository Instance => Lazy.Value;

        public GroupBaseConfigurationRepository(IApplicationConfiguration applicationConfiguration)
        {
            _applicationConfiguration = applicationConfiguration;
        }

        public GroupDTO GetConfig(int groupId)
        {
            string key = GetGroupConfigKey(groupId);
            return UtilsDal.GetObjectFromCB<GroupDTO>(eCouchbaseBucket.GROUPS, key);
        }

        public bool SaveConfig(int groupId, GroupDTO group)
        {
            string key = GetGroupConfigKey(groupId);
            return UtilsDal.SaveObjectInCB(eCouchbaseBucket.GROUPS, key, group);
        }

        public bool DeleteConfig(int groupId)
        {
            string key = GetGroupConfigKey(groupId);
            return UtilsDal.DeleteObjectFromCB(eCouchbaseBucket.GROUPS, key);
        }
        
        public string GetGroupConfigKey(int groupId)
        {
            return string.Format(_applicationConfiguration.GroupsManagerConfiguration.KeyFormat.Value, groupId);
        }
    }
}