using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class LicensedLinksCacheConfiguration : ConfigurationValue
    {
        public BooleanConfigurationValue ShouldUseCache;
        public NumericConfigurationValue CacheTimeInSeconds;
        
        public LicensedLinksCacheConfiguration(string key) : base(key)
        {
            ShouldUseCache = new BooleanConfigurationValue("should_use_cache", this)
            {
                DefaultValue = true,
                OriginalKey = "ShouldUseLicenseLinkCache"
            };
            CacheTimeInSeconds = new NumericConfigurationValue("cache_time_in_seconds", this)
            {
                DefaultValue = 300,
                OriginalKey = "LicenseLinkCacheInSec"
            };
        }
    }
}