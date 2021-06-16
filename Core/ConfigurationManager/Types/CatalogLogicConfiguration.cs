using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class CatalogLogicConfiguration : BaseConfig<CatalogLogicConfiguration>
    {
        public BaseValue<bool> ShouldUseHitCache = new BaseValue<bool>("should_use_hit_cache",true);
        public BaseValue<bool> ShouldAddUserIPToStats = new BaseValue<bool>("should_add_user_ip_to_stats", false);
        public BaseValue<bool> ShouldUseSearchCache = new BaseValue<bool>("should_use_search_cache", false);
        public BaseValue<double> HitCacheTimeInMinutes = new BaseValue<double>("hit_cache_time_in_minutes", 120);
        public BaseValue<int> CurrentRequestDaysOffset = new BaseValue<int>("CURRENT_REQUEST_DAYS_OFFSET", 7);
        public BaseValue<int> UpdateEPGPackage = new BaseValue<int>("update_epg_package", 200);
        public BaseValue<int> PersonalRecommendedMaxResultsSize = new BaseValue<int>("personal_recommended_max_results_size", 20);
        public BaseValue<string> WatchHistoryStaleMode = new BaseValue<string>("watch_history_stale_mode", "false", false, "ViewStaleState enum. Possible values are: None, False, Ok, UpdateAfter");
        public BaseValue<string> GroupsWithIUserTypeSeperatedBySemiColon = new BaseValue<string>("GroupIDsWithIUserTypeSeperatedBySemiColon", "0");
        public BaseValue<bool> ShouldUseFirstPlayRateManager = new BaseValue<bool>("use_first_play_rate_manager", false);
        public BaseValue<int> SearchPastSeriesRecordingsPageSize = new BaseValue<int>("search_past_series_recordings_page_size", 200);

        public override string TcmKey => TcmObjectKeys.CatalogLogicConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };
    }
}