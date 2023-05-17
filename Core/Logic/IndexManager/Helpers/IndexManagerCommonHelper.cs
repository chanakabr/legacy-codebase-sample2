using ApiObjects;
using Phx.Lib.Log;
using Polly;
using Polly.Retry;
using System;
using System.Reflection;
using ApiObjects.SearchObjects;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Tvinci.Core.DAL;
using GroupsCacheManager;
using Core.Catalog.CatalogManagement;
using ApiLogic.Catalog.IndexManager.GroupBy;
using System.Net;
using System.Net.Sockets;

namespace ApiLogic.IndexManager.Helpers
{
    /// <summary>
    /// keeps all the common static method that can be used between the different index managers
    /// </summary>
    public static class IndexManagerCommonHelpers
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly int DEFAULT_CURRENT_REQUEST_DAYS_OFFSET = 7;
        public static readonly string META_DOUBLE_SUFFIX = "_DOUBLE";
        public static readonly string META_BOOL_SUFFIX = "_BOOL";
        public static readonly string META_DATE_PREFIX = "date";

        private static readonly Dictionary<string, string> NONE_PHONETIC_LANGUAGES
            = new Dictionary<string, string> { { "heb", @"[\u0590-\u05FF]+" } };

        public static string GetTranslationType(string type, LanguageObj language)
        {
            if (language.IsDefault)
            {
                return type;
            }

            return string.Concat(type, "_", language.Code);
        }

        internal static RetryPolicy GetRetryPolicy<TException>(int retryCount = 3) where TException : Exception
        {
            return Policy.Handle<TException>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time, attempt, ctx) =>
                {
                    log.Warn($"ElasticSearch request attemp [{attempt}/{retryCount}] Failed, waiting for:[{time.TotalSeconds}] seconds.", ex);
                });
        }

        public static EpgCB GetEpgProgram(int nGroupID, int nEpgID)
        {
            EpgCB epg = null;

            EpgBL.BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(nGroupID);
            try
            {
                ulong uEpgID = ulong.Parse(nEpgID.ToString());
                epg = oEpgBL.GetEpgCB(uEpgID);
                return epg;
            }
            catch (Exception ex)
            {
                log.Error("Error (GetEpgProgram) " + string.Format("epg:{0}, msg:{1}, st:{2}", nEpgID, ex.Message, ex.StackTrace), ex);
                return null;
            }
        }

        public static List<EpgCB> GetEpgProgram(int nGroupID, int nEpgID, List<string> languages)
        {
            List<EpgCB> epgs = null;

            EpgBL.BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(nGroupID);
            try
            {
                ulong uEpgID = ulong.Parse(nEpgID.ToString());
                epgs = oEpgBL.GetEpgCB(uEpgID, languages);
                return epgs;
            }
            catch (Exception ex)
            {
                log.Error("Error (GetEpgProgram) - " + string.Format("epg:{0}, msg:{1}, st:{2}", nEpgID, ex.Message, ex.StackTrace));
                return new List<EpgCB>();
            }
        }

        public static string GetPermittedWatchRules(int nGroupId)
        {
            DataTable permittedWathRulesDt = Tvinci.Core.DAL.CatalogDAL.GetPermittedWatchRulesByGroupId(nGroupId, null);
            List<string> lWatchRulesIds = null;
            if (permittedWathRulesDt != null && permittedWathRulesDt.Rows.Count > 0)
            {
                lWatchRulesIds = new List<string>();
                foreach (DataRow permittedWatchRuleRow in permittedWathRulesDt.Rows)
                {
                    lWatchRulesIds.Add(ODBCWrapper.Utils.GetSafeStr(permittedWatchRuleRow["RuleID"]));
                }
            }

            string sRules = string.Empty;

            if (lWatchRulesIds != null && lWatchRulesIds.Count > 0)
            {
                sRules = string.Join(" ", lWatchRulesIds);
            }

            return sRules;
        }

        public static List<LanguageObj> GetLanguages(int nGroupID)
        {
            List<LanguageObj> lLang = new List<LanguageObj>();
            try
            {
                lLang = CatalogDAL.GetGroupLanguages(nGroupID);
                return lLang;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error GettingLanguages of group {0}. Exception: {1}", nGroupID, ex);

                return new List<LanguageObj>();
            }
        }

        public static bool IsSlidingWindowSupported(OrderBy orderBy)
            => typeof(OrderBy)
                .GetMember(orderBy.ToString())[0]
                .GetCustomAttributes(typeof(SlidingWindowSupportedAttribute), false).Length > 0;

        public static bool IsChannelOrderCompatibleWithGroupBy(
            SearchAggregationGroupBy searchGroupBy,
            IReadOnlyCollection<AssetOrder> orderingParameters,
            OrderObj order)
        {
            if (searchGroupBy?.groupBy == null || !searchGroupBy.groupBy.Any())
            {
                return true;
            }

            if (orderingParameters?.Count > 0)
            {
                return searchGroupBy?.groupBy?.Count == 1
                    && orderingParameters.Count == 1
                    && GroupBySearchIsSupportedForOrder(orderingParameters.Single().Field);
            }
            if (order != null)
            {
                return searchGroupBy?.groupBy?.Count == 1 && GroupBySearchIsSupportedForOrder(order.m_eOrderBy);
            }

            return false;
        }

        public static bool GroupBySearchIsSupportedForOrder(OrderBy orderBy) => GetStrategy(orderBy) != null;

        internal static IGroupBySearch GetStrategy(OrderBy orderBy)
        {
            switch (orderBy)
            {
                case OrderBy.NONE:
                case OrderBy.ID:
                case OrderBy.CREATE_DATE:
                case OrderBy.START_DATE:
                    return GroupByWithOrderByNumericField.Instance;
                case OrderBy.NAME:
                case OrderBy.META:
                    return GroupByWithOrderByNonNumericField.Instance;
                case OrderBy.RECOMMENDATION:
                    return new GroupByWithSpecificOrder();
                default:
                    return null;
            }
        }

        public static void GetMetaType(string sMeta, out eESFieldType sMetaType, out string sNullValue)
        {
            sMetaType = eESFieldType.STRING;
            sNullValue = string.Empty;

            if (sMeta.Contains(META_BOOL_SUFFIX))
            {
                sMetaType = eESFieldType.INTEGER;
                sNullValue = "0";
            }
            else if (sMeta.Contains(META_DOUBLE_SUFFIX))
            {
                sMetaType = eESFieldType.DOUBLE;
                sNullValue = "0.0";
            }
            else if (sMeta.StartsWith(META_DATE_PREFIX))
            {
                sMetaType = eESFieldType.DATE;
            }
        }

        public static void GetMetaType(ApiObjects.MetaType metaType, out eESFieldType esFieldType, out string sNullValue)
        {
            esFieldType = eESFieldType.STRING;
            sNullValue = string.Empty;
            switch (metaType)
            {
                case ApiObjects.MetaType.MultilingualString:
                case ApiObjects.MetaType.String:
                    esFieldType = eESFieldType.STRING;
                    break;
                case ApiObjects.MetaType.Number:
                    esFieldType = eESFieldType.DOUBLE;
                    sNullValue = "0.0";
                    break;
                case ApiObjects.MetaType.Bool:
                    esFieldType = eESFieldType.INTEGER;
                    sNullValue = "0";
                    break;
                case ApiObjects.MetaType.DateTime:
                    esFieldType = eESFieldType.DATE;
                    break;
                case ApiObjects.MetaType.All:
                case ApiObjects.MetaType.Tag:
                default:
                    break;
            }
        }

        public static bool CheckIpIsPrivate(IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                //https://stackoverflow.com/questions/8113546/how-to-determine-whether-an-ip-address-in-private
                byte[] bytes = address.GetAddressBytes();
                switch (bytes[0])
                {
                    case 10:
                        return true;
                    case 172:
                        return bytes[1] < 32 && bytes[1] >= 16;
                    case 192:
                        return bytes[1] == 168;
                    default:
                        return false;
                }
            }

            return false;
        }

        public static List<Channel> GetGroupChannels(int partnerId, IChannelManager channelManager, bool doesGroupUseTemplates, ref HashSet<int> channelIds)
        {
            List<Channel> groupChannels = null;

            if (doesGroupUseTemplates)
            {
                groupChannels = channelManager.GetGroupChannels(partnerId);
                channelIds = new HashSet<int>(groupChannels.Select(x => x.m_nChannelID));
            }
            // means that channelIds != null
            else
            {
                GroupManager groupManager = new GroupManager();
                groupManager.RemoveGroup(partnerId);
                groupChannels = groupManager.GetChannels(channelIds.ToList(), partnerId);
            }

            return groupChannels;
        }

        public static bool OrderByString(OrderBy? orderBy)
        {
            if (!orderBy.HasValue) return false;

            return orderBy == OrderBy.META || orderBy == OrderBy.NAME;
        }

        //Select fuzzy instead of phonetic if phrase is in non supported language
        public static bool IsLanguagePhoneticSupported(string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
                return true;

            var anyMatch = NONE_PHONETIC_LANGUAGES.Any(x => System.Text.RegularExpressions.Regex.IsMatch(phrase, x.Value));
            return !anyMatch;
        }

        public static string GetSuppressedIndex(Media media, HashSet<string> suppressedIndexes)
        {
            if (suppressedIndexes == null || !suppressedIndexes.Any())
                return string.Empty;

            var result = GetSuppressedMeta(media, suppressedIndexes);
            if (string.IsNullOrEmpty(result))
                return GetSuppressedTag(media, suppressedIndexes);

            return string.Empty;
        }

        private static string GetSuppressedMeta(Media media, HashSet<string> suppressedIndexes)
        {
            //Suppressed Metas
            if (media.m_dMeatsValues.Any())
            {
                var suppressedIndex = media.m_dMeatsValues.FirstOrDefault(x => suppressedIndexes.Contains(x.Key));

                if (suppressedIndex.Equals(default(KeyValuePair<string, string>)))
                    return string.Empty;

                if (!string.IsNullOrEmpty(suppressedIndex.Key))
                {
                    var _suppressed = media.m_dMeatsValues[suppressedIndex.Key];
                    log.Debug($"Set suppressed value: {_suppressed} from index (Meta): {suppressedIndex.Key}");
                    return _suppressed;
                }
            }

            return string.Empty;
        }

        private static string GetSuppressedTag(Media media, HashSet<string> suppressedIndexes)
        {
            //Suppressed Tag
            if (media.m_dTagValues.Any())
            {
                var suppressedIndex = media.m_dTagValues.FirstOrDefault(x => suppressedIndexes.Contains(x.Key));

                if (suppressedIndex.Equals(default(KeyValuePair<string, string>)))
                    return string.Empty;

                if (!string.IsNullOrEmpty(suppressedIndex.Key))
                {
                    var _suppressed = string.Join("", media.m_dTagValues[suppressedIndex.Key]);
                    log.Debug($"Set suppressed value: {_suppressed} from index (Tag): {suppressedIndex.Key}");
                    return _suppressed;
                }
            }

            return string.Empty;
        }

        public static string GetSuppressedIndex(EpgCB epg, HashSet<string> suppressedIndexes)
        {
            if (suppressedIndexes == null || !suppressedIndexes.Any())
                return string.Empty;

            var result = GetSuppressedMeta(epg, suppressedIndexes);
            if (string.IsNullOrEmpty(result))
                return GetSuppressedTag(epg, suppressedIndexes);

            return string.Empty;
        }

        private static string GetSuppressedMeta(EpgCB epg, HashSet<string> suppressedIndexes)
        {
            //Suppressed Metas
            if (epg.Metas.Any())
            {
                var suppressedIndex = epg.Metas.FirstOrDefault(x => suppressedIndexes.Contains(x.Key));

                if (suppressedIndex.Equals(default(KeyValuePair<string, string>)))
                    return string.Empty;

                if (!string.IsNullOrEmpty(suppressedIndex.Key))
                {
                    var _suppressed = epg.Metas[suppressedIndex.Key];
                    log.Debug($"Set suppressed value: {_suppressed} from index (Meta): {suppressedIndex.Key}");
                    return _suppressed.FirstOrDefault();
                }
            }

            return string.Empty;
        }

        private static string GetSuppressedTag(EpgCB epg, HashSet<string> suppressedIndexes)
        {
            //Suppressed Tag
            if (epg.Tags.Any())
            {
                var suppressedIndex = epg.Tags.FirstOrDefault(x => suppressedIndexes.Contains(x.Key));

                if (suppressedIndex.Equals(default(KeyValuePair<string, string>)))
                    return string.Empty;

                if (!string.IsNullOrEmpty(suppressedIndex.Key))
                {
                    var _suppressed = string.Join("", epg.Tags[suppressedIndex.Key]);
                    log.Debug($"Set suppressed value: {_suppressed} from index (Tag): {suppressedIndex.Key}");
                    return _suppressed;
                }
            }

            return string.Empty;
        }
    }
}