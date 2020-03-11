using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;
using System;

namespace ConfigurationManager
{
    public class GroupsManagerConfiguration : BaseConfig<GroupsManagerConfiguration>
    {
        public BaseValue<string> KeyFormat = new BaseValue<string>("key_format", "group_{0}");
        public BaseValue<double> CacheTTLSeconds = new BaseValue<double>("cache_ttl_seconds", 60);

        public override string TcmKey => TcmObjectKeys.GroupsManagerConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };


    }
}