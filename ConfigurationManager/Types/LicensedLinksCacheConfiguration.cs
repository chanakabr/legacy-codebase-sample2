using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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