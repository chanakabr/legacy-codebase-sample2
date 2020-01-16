using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;
using System;

namespace ConfigurationManager
{
    public class UsersCacheConfiguration : BaseConfig<UsersCacheConfiguration>
    {
        public BaseValue<bool> ShouldUseCache = new BaseValue<bool>("should_use_cache", true);
        public BaseValue<double> TTLSeconds = new BaseValue<double>("ttl_seconds", 1440.0);

        public override string TcmKey => TcmObjectKeys.UsersCacheConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

  
    }
}
