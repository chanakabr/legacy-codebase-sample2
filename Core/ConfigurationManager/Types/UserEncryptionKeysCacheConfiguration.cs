using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class UserEncryptionKeysCacheConfiguration : BaseConfig<UserEncryptionKeysCacheConfiguration>
    {
        public override string TcmKey => "user_encryption_keys_cache_configuration";

        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<int> TTLSeconds = new BaseValue<int>("ttl_seconds", 86400);
    }
}
