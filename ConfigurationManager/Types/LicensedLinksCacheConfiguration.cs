using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class LicensedLinksCacheConfiguration : BaseConfig<LicensedLinksCacheConfiguration>
    {
        public override string TcmKey => "licensed_links_cache_configuration";

        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<bool> ShouldUseCache = new BaseValue<bool>("should_use_cache", true, true, "description");
        public BaseValue<int> CacheTimeInSeconds = new BaseValue<int>("cache_time_in_seconds", 300, true, "LicenseLinkCacheInSec");
    }

}