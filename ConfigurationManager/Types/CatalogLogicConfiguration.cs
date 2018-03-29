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
        public NumericConfigurationValue CurrentRequestDaysOffset;
        public NumericConfigurationValue UpdateEPGPackage;
        public NumericConfigurationValue PersonalRecommendedMaxResultsSize;
        public StringConfigurationValue GroupsUsingDBForAssetsStats;
        public StringConfigurationValue GroupsWithIUserTypeSeperatedBySemiColon;
        public StringConfigurationValue GroupsWithIPNOFilteringShowAllCatalogAnonymousUser;
        public StringConfigurationValue GroupIDsWithIFPNPC;

        public CatalogLogicConfiguration(string key) : base(key)
        {
            WatchHistoryStaleMode = new StringConfigurationValue("watch_history_stale_mode", this)
            {
                DefaultValue = "False",
                Description = "ViewStaleState enum. Possible values are: None, False, Ok, UpdateAfter",
                OriginalKey = "WatchHistory_StaleMode"
            };
            HitCacheTimeInMinutes = new NumericConfigurationValue("hit_cache_time_in_minutes", this)
            {
                DefaultValue = 120,
                OriginalKey = "CATALOG_HIT_CACHE_TIME_IN_MINUTES",
            };
            ShouldUseHitCache = new BooleanConfigurationValue("should_use_hit_cache", this)
            {
                DefaultValue = true,
                OriginalKey = "CATALOG_HIT_CACHE"
            };
            ShouldUseSearchCache = new BooleanConfigurationValue("should_use_search_cache", this)
            {
                DefaultValue = false,
                ShouldAllowEmpty = true,
                OriginalKey = "Use_Search_Cache"
            };
            CurrentRequestDaysOffset = new NumericConfigurationValue("CURRENT_REQUEST_DAYS_OFFSET", this)
            {
                DefaultValue = 7,
                OriginalKey = "CURRENT_REQUEST_DAYS_OFFSET"
            };
            UpdateEPGPackage = new ConfigurationManager.NumericConfigurationValue("update_epg_package", this)
            {
                DefaultValue = 200,
                OriginalKey = "update_epg_package"
            };
            PersonalRecommendedMaxResultsSize = new NumericConfigurationValue("personal_recommended_max_results_size", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = 20,
                OriginalKey = "PWLALP_MAX_RESULTS_SIZE"
            };
            GroupsUsingDBForAssetsStats = new StringConfigurationValue("groups_using_db_for_assets_stats", this)
            {
                ShouldAllowEmpty = true,
                OriginalKey = "GROUPS_USING_DB_FOR_ASSETS_STATS"
            };
            GroupsWithIUserTypeSeperatedBySemiColon = new StringConfigurationValue("GroupIDsWithIUserTypeSeperatedBySemiColon", this)
            {
                ShouldAllowEmpty = true,
                OriginalKey = "GroupIDsWithIPNOFilteringSeperatedBySemiColon"
            };
            GroupsWithIPNOFilteringShowAllCatalogAnonymousUser = new StringConfigurationValue("GroupIDsWithIPNOFilteringShowAllCatalogAnonymousUser", this)
            {
                ShouldAllowEmpty = true,
                OriginalKey = "GroupIDsWithIPNOFilteringShowAllCatalogAnonymousUser"
            };
            GroupIDsWithIFPNPC = new StringConfigurationValue("GroupIDsWithIFPNPC", this)
            {
                ShouldAllowEmpty = true,
                OriginalKey = "GroupIDsWithIFPNPC"
            };
        }
    }
}