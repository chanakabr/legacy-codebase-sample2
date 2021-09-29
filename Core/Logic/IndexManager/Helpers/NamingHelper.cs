using System;

namespace ApiLogic.IndexManager.Helpers
{
    public static class NamingHelper
    {
        internal const string SUB_SUM_AGGREGATION_NAME = "sub_sum";
        internal const string SUB_STATS_AGGREGATION_NAME = "sub_stats";
        internal const string STAT_ACTION_RATE_VALUE_FIELD = "rate_value";
        internal const string STAT_SLIDING_WINDOW_AGGREGATION_NAME = "sliding_window";

        internal const string STAT_ACTION_MEDIA_HIT = "mediahit";
        internal const string STAT_ACTION_FIRST_PLAY = "firstplay";
        internal const string STAT_ACTION_LIKE = "like";
        internal const string STAT_ACTION_RATES = "rates";
        internal const string STAT_ACTION_COUNT_VALUE_FIELD = "count";

        public const string ENTITLED_ASSETS_FIELD = "entitled_assets";
        public const string GEO_BLOCK_FIELD = "geo_block";
        public const string PARENTAL_RULES_FIELD = "parental_rules";
        public const string USER_INTERESTS_FIELD = "user_interests";
        public const string ASSET_TYPE = "asset_type";
        public const string RECORDING_ID = "recording_id";
        public const string AUTO_FILL_FIELD = "auto_fill";
        public const string ENABLE_CDVR = "enable_cdvr";
        public const string ENABLE_CATCHUP = "enable_catchup";

        public const string EPG_IDENTIFIER = "epg_identifier";

        public static string GetEpgIndexAlias(int groupId)
        {
            return $"{groupId}_epg";
        }

        public static string GetDailyEpgIndexName(int groupId, DateTime indexDate)
        {
            string dateString = indexDate.Date.ToString(ElasticSearch.Common.Utils.ES_DATEONLY_FORMAT);
            return $"{groupId}_epg_v2_{dateString}";
        }

        public static string GetMediaIndexAlias(int nGroupID)
        {
            return nGroupID.ToString();
        }

        public static string GetNewEpgIndexName(int nGroupID)
        {
            return string.Format("{0}_epg_{1}", nGroupID, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        public static string GetNewRecordingIndexName(int nGroupID)
        {
            return string.Format("{0}_recording_{1}", nGroupID, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        public static string GetNewMediaIndexName(int nGroupID)
        {
            return string.Format("{0}_{1}", nGroupID, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        public static string GetNewUtilsIndexString()
        {
            return string.Format("utils_{0}", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));        
        }

        public static string GetUtilsIndexName()
        {
            return "utils";
        }
        
        public static string GetNewIPv6IndexName()
        {
            return string.Format("ipv6_{0}", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        internal static string GetMetadataIndexAlias(int groupId)
        {
            return string.Format("{0}_metadata", groupId);
        }

        internal static string GetNewMetadataIndexName(int groupId)
        {
            return string.Format("{0}_metadata_{1}", groupId, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        internal static string GetNewChannelMetadataIndexName(int groupId)
        {
            return string.Format("{0}_channel_{1}", groupId, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        internal static string GetChannelMetadataIndexName(int groupId)
        {
            return string.Format("{0}_channel", groupId);
        }

        public static string GetRecordingIndexAlias(int nGroupID)
        {
            return string.Format("{0}_recording", nGroupID);
        }

        public static string GetIpToCountryIndexAlias()
        {
            return "ip_to_country";
        }

        public static string GetNewIpToCountryIndexName()
        {
            return $"ip_to_country_{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
        }

        public static string GetNewChannelPercolatorIndex(int partnerId)
        {
            return $"{partnerId}_channel_percolator_{DateTime.UtcNow.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT)}";
        }

        public static string GetChannelPercolatorIndexAlias(int partnerId)
        {
            return $"{partnerId}_channel_percolator";
        }

        public static string GetStatisticsIndexName(int groupId)
        {
            return string.Concat(groupId, "_statistics");
        }
    }
}
