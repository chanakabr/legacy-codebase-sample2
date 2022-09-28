using ApiObjects.SearchObjects;
using System;
using System.Threading;
using ApiLogic.EPG;

namespace ApiLogic.IndexManager.Helpers
{
    public interface INamingHelper
    {
        string GetDailyEpgIndexName(int groupId, DateTime indexDate);
        string GetEpgIndexDateSuffix(int groupId, DateTime d);
    }

    public class NamingHelper : INamingHelper
    {
        private readonly IEpgPartnerConfigurationManager _epgV2PartnerConfigurationManager;
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
        public const string IS_AUTO_FILL_FIELD = "is_auto_fill";
        public const string ENABLE_CDVR = "enable_cdvr";
        public const string ENABLE_CATCHUP = "enable_catchup";
        public const string EPG_V3_TRANSACTION_DOCUMENT_TYPE_NAME = "transaction";
        public const string EPG_V3_DUMMY_TRANSACTION_CHILD_DOCUMENT_TYPE_NAME = "dummy_transaction_child";
        public const string IS_ACTIVE = "is_active";

        public const string EPG_IDENTIFIER = "epg_identifier";

        // live to vod
        public const string LIVE_TO_VOD_PREFIX = "live_to_vod";
        public const string LINEAR_ASSET_ID = "linear_asset_id";
        public const string EPG_CHANNEL_ID = "epg_channel_id";
        public const string CRID = "crid";
        public const string ORIGINAL_START_DATE = "orig_start_date";
        public const string ORIGINAL_END_DATE = "orig_end_date";
        public const string EPG_ID = "epg_id";

        private const string EPG_V2_PAST_INDEX_NAME_SUFFIX = "past";
        private const string EPG_V2_FUTURE_INDEX_NAME_SUFFIX = "future";
        


        private static readonly Lazy<INamingHelper> _lazy =
            new Lazy<INamingHelper>(GetNamingHelperInstance, LazyThreadSafetyMode.PublicationOnly);

        public NamingHelper(IEpgPartnerConfigurationManager epgV2PartnerConfigurationManager)
        {
            _epgV2PartnerConfigurationManager = epgV2PartnerConfigurationManager;
        }

        private static INamingHelper GetNamingHelperInstance()
        {
            return new NamingHelper(EpgPartnerConfigurationManager.Instance);
        }

        public static INamingHelper Instance => _lazy.Value;

        public static string GetEpgFutureIndexName(int groupId) => $"{groupId}_epg_v2_{EPG_V2_FUTURE_INDEX_NAME_SUFFIX}";
        public static string GetEpgPastIndexName(int groupId) => $"{groupId}_epg_v2_{EPG_V2_PAST_INDEX_NAME_SUFFIX}";
        public static string GetEpgIndexAlias(int groupId)
        {
            return $"{groupId}_epg";
        }

        public static string GetEpgV3TransactionId(long linearChannelId, long bulkUploadId)
        {
            return $"{bulkUploadId}_{linearChannelId}";
        }

        public string GetDailyEpgIndexName(int groupId, DateTime indexDate)
        {
            var dateString = GetEpgIndexDateSuffix(groupId, indexDate);
            return $"{groupId}_epg_v2_{dateString}";
        }

        public string GetEpgIndexDateSuffix(int groupId, DateTime d)
        {
            var currDate = DateTime.UtcNow.Date;
            var epgV2Conf = _epgV2PartnerConfigurationManager.GetEpgV2Configuration(groupId);
            if (epgV2Conf.IsIndexCompactionEnabled)
            {
                if (d > currDate && d.Subtract(currDate).TotalDays > epgV2Conf.FutureIndexCompactionStart)
                {
                    return EPG_V2_FUTURE_INDEX_NAME_SUFFIX;
                }

                if (d < currDate && currDate.Subtract(d).TotalDays > epgV2Conf.PastIndexCompactionStart)
                {
                    return EPG_V2_PAST_INDEX_NAME_SUFFIX;
                }
            }

            return d.ToString(ElasticSearch.Common.Utils.ES_DATEONLY_FORMAT);
        } 

        public static string GetMediaIndexAlias(int nGroupID)
        {
            return nGroupID.ToString();
        }

        public static string GetNewEpgIndexName(int nGroupID, DateTime indexDate)
            => $"{nGroupID}_epg_{indexDate.ToString("yyyyMMddHHmmss")}";

        public static string GetNewRecordingIndexName(int nGroupID, DateTime indexDate)
            => $"{nGroupID}_recording_{indexDate.ToString("yyyyMMddHHmmss")}";

        public static string GetMediaIndexName(int nGroupID, DateTime indexDate)
            => $"{nGroupID}_{indexDate.ToString("yyyyMMddHHmmss")}";

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

        internal static string GetNewMetadataIndexName(int groupId, DateTime indexDate)
            => $"{groupId}_metadata_{indexDate.ToString("yyyyMMddHHmmss")}";

        internal static string GetNewChannelMetadataIndexName(int groupId, DateTime indexDate)
            => $"{groupId}_channel_{indexDate.ToString("yyyyMMddHHmmss")}";

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

        public static string GetChannelPercolatorIndex(int partnerId, DateTime indexDate)
        => $"{partnerId}_channel_percolator_{indexDate.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT)}";

        public static string GetChannelPercolatorIndexAlias(int partnerId) => $"{partnerId}_channel_percolator";

        public static string GetStatisticsIndexName(int groupId)
        {
            return string.Concat(groupId, "_statistics");
        }

        internal static string GetExtraFieldName(string key, eFieldType type)
        {
            var loweredKey = key.ToLower();
            if (type == eFieldType.NonStringMeta || type == eFieldType.StringMeta)
            {
                return $"metas.{loweredKey}";
            }
            else if (type == eFieldType.Tag)
            {
                return $"tags.{loweredKey}";
            }
            else
            {
                return loweredKey;
            }
        }
    }
}
