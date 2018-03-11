using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class CatalogLogicConfiguration : ConfigurationValue
    {
        public StringConfigurationValue WatchHistoryStaleMode;
        public NumericConfigurationValue HitCacheTimeInMinutes;
        public BooleanConfigurationValue ShouldUseHitCache;
        public BooleanConfigurationValue ShouldUseSearchCache;

        public CatalogLogicConfiguration(string key) : base(key)
        {
            WatchHistoryStaleMode = new StringConfigurationValue("watch_history_stale_mode", this)
            {
                DefaultValue = "False",
                Description = "ViewStaleState enum. Possible values are: None, False, Ok, UpdateAfter"
            };
            HitCacheTimeInMinutes = new NumericConfigurationValue("hit_cache_time_in_minutes", this)
            {
                DefaultValue = 120
            };
            ShouldUseHitCache = new BooleanConfigurationValue("should_use_hit_cache", this)
            {
                DefaultValue = true
            };
            ShouldUseSearchCache = new BooleanConfigurationValue("should_use_search_cache", this)
            {
                DefaultValue = false,
                ShouldAllowEmpty = true
            };
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= WatchHistoryStaleMode.Validate();
            result &= HitCacheTimeInMinutes.Validate();
            result &= ShouldUseHitCache.Validate();
            result &= ShouldUseSearchCache.Validate();

            return result;
        }
    }
}