using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class RoleIdsConfiguration : BaseConfig<RoleIdsConfiguration>
    {
        public BaseValue<long> MasterRoleId = new BaseValue<long>("master_role_id", 2);
        public BaseValue<long> UserRoleId = new BaseValue<long>("user_role_id", 1);

        public override string TcmKey => TcmObjectKeys.RoleIdsConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };
    }
}