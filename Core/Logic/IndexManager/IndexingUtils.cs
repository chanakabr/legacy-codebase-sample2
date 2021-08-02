
using ApiObjects;
using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;
using KLogMonitor;
using System.Reflection;
using GroupsCacheManager;
using ElasticSearch.Searcher;
using ApiObjects.Response;
using ElasticSearch.Common;
using Core.Catalog;
using Core.Catalog.Cache;
using Core.Catalog.CatalogManagement;
using ConfigurationManager;
using ApiObjects.Catalog;
using ApiLogic.Catalog.IndexManager.GroupBy;
using System.Net;
using System.Net.Sockets;
using ApiLogic.Catalog;
using Core.Catalog.Request;
using Core.Api.Managers;

namespace Core.Catalog
{
    public static class IndexingUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly int DEFAULT_CURRENT_REQUEST_DAYS_OFFSET = 7;
        public static readonly string META_DOUBLE_SUFFIX = "_DOUBLE";
        public static readonly string META_BOOL_SUFFIX = "_BOOL";
        public static readonly string META_DATE_PREFIX = "date";
        public static long UnixTimeStampNow()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds, CultureInfo.CurrentCulture);
        }

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

        public static string GetNewEpgIndexStr(int nGroupID)
        {
            return string.Format("{0}_epg_{1}", nGroupID, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        public static string GetNewRecordingIndexStr(int nGroupID)
        {
            return string.Format("{0}_recording_{1}", nGroupID, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        public static string GetNewMediaIndexStr(int nGroupID)
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
        
        public static string GetNewIPv6IndexString()
        {
            return string.Format("ipv6_{0}", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        internal static string GetMetadataGroupAliasStr(int groupId)
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

        public static string GetRecordingGroupAliasStr(int nGroupID)
        {
            return string.Format("{0}_recording", nGroupID);
        }

        public static string GetTanslationType(string sType, LanguageObj oLanguage)
        {
            if (oLanguage.IsDefault)
            {
                return sType;
            }
            else
            {
                return string.Concat(sType, "_", oLanguage.Code);
            }
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

        public static bool GroupBySearchIsSupportedForOrder(OrderBy orderBy) => GetStrategy(orderBy) != null;


        internal static IGroupBySearch GetStrategy(OrderBy orderBy)
        {
            switch (orderBy)
            {
                case OrderBy.NONE:
                case OrderBy.ID:
                case OrderBy.CREATE_DATE:
                case OrderBy.START_DATE: return new GroupByWithOrderByNumericField();
                case OrderBy.NAME:
                case OrderBy.META: return new GroupByWithOrderByNonNumericField();
                default: return null;
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


        public static string GetChannelQuery(ESMediaQueryBuilder mediaQueryParser, ESUnifiedQueryBuilder unifiedQueryBuilder,
            Channel currentChannel, IWatchRuleManager watchRuleManager, Group group, bool doesGroupUseTemplates)
        {
            string channelQuery = string.Empty;

            if (currentChannel == null)
            {
                log.ErrorFormat("BuildChannelQueries - All channels list has null or in-active channel, continuing");
                return channelQuery;
            }

            // if group uses templates - index inactive channel as well
            if (doesGroupUseTemplates && currentChannel.m_nIsActive != 1)
            {
                log.ErrorFormat("BuildChannelQueries - All channels list has null or in-active channel, continuing");
                return channelQuery;
            }

            try
            {
                log.DebugFormat("BuildChannelQueries - Current channel  = {0}", currentChannel.m_nChannelID);

                if ((currentChannel.m_nChannelTypeID == (int)ChannelType.KSQL) ||
                   (currentChannel.m_nChannelTypeID == (int)ChannelType.Manual && doesGroupUseTemplates && currentChannel.AssetUserRuleId > 0))
                {
                    try
                    {
                        if (currentChannel.m_nChannelTypeID == (int)ChannelType.Manual && currentChannel.AssetUserRuleId > 0)
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.Append("(or ");

                            foreach (var item in currentChannel.m_lChannelTags)
                            {
                                builder.AppendFormat("media_id='{0}' ", item.m_lValue);
                            }

                            builder.Append(")");

                            currentChannel.filterQuery = builder.ToString();
                        }

                        UnifiedSearchDefinitions definitions = BuildSearchDefinitions(group, currentChannel, true, watchRuleManager);

                        definitions.shouldSearchEpg = false;

                        unifiedQueryBuilder.SearchDefinitions = definitions;
                        channelQuery = unifiedQueryBuilder.BuildSearchQueryString(true);
                    }
                    catch (KalturaException ex)
                    {
                        log.ErrorFormat("Tried to index an invalid KSQL Channel. ID = {0}, message = {1}", currentChannel.m_nChannelID, ex.Message, ex);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    mediaQueryParser.m_nGroupID = currentChannel.m_nGroupID;
                    MediaSearchObj mediaSearchObject = BuildBaseChannelSearchObject(currentChannel, watchRuleManager);

                    mediaQueryParser.oSearchObject = mediaSearchObject;
                    channelQuery = mediaQueryParser.BuildSearchQueryString(true);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("BuildChannelQueries - building query for channel {0} has failed, ex = {1}", currentChannel.m_nChannelID, ex);
            }

            return channelQuery;
        }

        public static UnifiedSearchDefinitions BuildSearchDefinitions(Group group, Channel channel, bool useMediaTypes, IWatchRuleManager watchRuleManager)
        {
            UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions();

            definitions.groupId = channel.m_nGroupID;

            if (useMediaTypes)
            {
                definitions.mediaTypes = new List<int>(channel.m_nMediaType);
            }

            if (channel.m_nMediaType != null)
            {
                // Nothing = all
                if (channel.m_nMediaType.Count == 0)
                {
                    definitions.shouldSearchEpg = true;
                    definitions.shouldSearchMedia = true;
                }
                else
                {
                    if (channel.m_nMediaType.Contains(Channel.EPG_ASSET_TYPE))
                    {
                        definitions.shouldSearchEpg = true;
                    }

                    // If there's anything besides EPG
                    if (channel.m_nMediaType.Count(type => type != Channel.EPG_ASSET_TYPE) > 0)
                    {
                        definitions.shouldSearchMedia = true;
                    }
                }
            }

            definitions.permittedWatchRules = GetPermittedWatchRules(watchRuleManager, channel.m_nGroupID);
            definitions.order = new OrderObj();

            definitions.shouldUseStartDateForMedia = false;
            definitions.shouldUseFinalEndDate = false;

            BaseRequest dummyRequest = new BaseRequest()
            {
                domainId = 0,
                m_nGroupID = channel.m_nParentGroupID,
                m_nPageIndex = 0,
                m_nPageSize = 0,
                m_oFilter = new Filter(),
                m_sSiteGuid = string.Empty,
                m_sUserIP = string.Empty
            };

            if (channel.AssetUserRuleId.HasValue && channel.AssetUserRuleId.Value > 0)
            {
                var assetUserRule = AssetUserRuleManager.GetAssetUserRuleByRuleId(channel.m_nGroupID, channel.AssetUserRuleId.Value);

                if (assetUserRule != null && assetUserRule.Status != null && assetUserRule.Status.Code == (int)eResponseStatus.OK && assetUserRule.Object != null)
                {
                    BooleanPhraseNode phrase = null;

                    var rulesIds = new List<long>();
                    string queryString = string.Empty;

                    UnifiedSearchDefinitionsBuilder.GetQueryStringFromAssetUserRules(new List<ApiObjects.Rules.AssetUserRule>()
                        {
                            assetUserRule.Object
                        },
                        out rulesIds,
                        out queryString);

                    BooleanPhrase.ParseSearchExpression(queryString, ref phrase);

                    CatalogLogic.UpdateNodeTreeFields(dummyRequest, ref phrase, definitions, group, group.m_nParentGroupID);

                    definitions.assetUserRuleFilterPhrase = phrase;
                }
            }

            if (!string.IsNullOrEmpty(channel.filterQuery))
            {
                BooleanPhraseNode filterTree = null;
                Status parseStatus = BooleanPhraseNode.ParseSearchExpression(channel.filterQuery, ref filterTree);

                if (parseStatus.Code != (int)eResponseStatus.OK)
                {
                    throw new KalturaException(parseStatus.Message, parseStatus.Code);
                }
                else
                {
                    definitions.filterPhrase = filterTree;
                }

                CatalogLogic.UpdateNodeTreeFields(dummyRequest,
                    ref definitions.filterPhrase, definitions, group, channel.m_nParentGroupID);
            }

            return definitions;
        }

        public static MediaSearchObj BuildBaseChannelSearchObject(Channel channel, IWatchRuleManager watchRuleManager)
        {
            MediaSearchObj searchObject = new MediaSearchObj();
            searchObject.m_nGroupId = channel.m_nGroupID;
            searchObject.m_bExact = true;
            searchObject.m_eCutWith = channel.m_eCutWith;

            if (channel.m_nMediaType != null)
            {
                searchObject.m_sMediaTypes = string.Join(";", channel.m_nMediaType.Select(type => type.ToString()));
            }

            searchObject.m_sPermittedWatchRules = GetPermittedWatchRules(watchRuleManager, channel.m_nGroupID);
            searchObject.m_oOrder = new ApiObjects.SearchObjects.OrderObj();

            searchObject.m_bUseStartDate = false;
            searchObject.m_bUseFinalEndDate = false;

            CopySearchValuesToSearchObjects(ref searchObject, channel.m_eCutWith, channel.m_lChannelTags);

            // If it is a manual channel without media, make it an empty request
            if (channel.m_nChannelTypeID == (int)ChannelType.Manual &&
                (channel.m_lChannelTags == null || channel.m_lChannelTags.Count == 0))
            {
                searchObject.m_eCutWith = CutWith.AND;
                searchObject.m_eFilterTagsAndMetasCutWith = CutWith.AND;
                searchObject.m_lFilterTagsAndMetas = new List<SearchValue>()
                {
                    new SearchValue("media_id", "0")
                    {
                        m_eInnerCutWith = CutWith.AND,
                        m_lValue = new List<string>()
                        {
                            "0"
                        }
                    }
                };
            }

            return searchObject;
        }

        private static string GetPermittedWatchRules(IWatchRuleManager watchRuleManager, int nGroupId)
        {
            List<string> groupPermittedWatchRules = watchRuleManager.GetGroupPermittedWatchRules(nGroupId);
            string sRules = string.Empty;

            if (groupPermittedWatchRules != null && groupPermittedWatchRules.Count > 0)
            {
                sRules = string.Join(" ", groupPermittedWatchRules);
            }

            return sRules;
        }

        private static void CopySearchValuesToSearchObjects(ref MediaSearchObj searchObject, CutWith cutWith, List<SearchValue> channelSearchValues)
        {
            List<SearchValue> m_dAnd = new List<SearchValue>();
            List<SearchValue> m_dOr = new List<SearchValue>();

            SearchValue search = new SearchValue();
            if (channelSearchValues != null && channelSearchValues.Count > 0)
            {
                foreach (SearchValue searchValue in channelSearchValues)
                {
                    if (!string.IsNullOrEmpty(searchValue.m_sKey))
                    {
                        search = new SearchValue();
                        search.m_sKey = searchValue.m_sKey;
                        search.m_lValue = searchValue.m_lValue;
                        search.m_sKeyPrefix = searchValue.m_sKeyPrefix;
                        search.m_eInnerCutWith = searchValue.m_eInnerCutWith;

                        switch (cutWith)
                        {
                            case ApiObjects.SearchObjects.CutWith.OR:
                                {
                                    m_dOr.Add(search);
                                    break;
                                }
                            case ApiObjects.SearchObjects.CutWith.AND:
                                {
                                    m_dAnd.Add(search);
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
            }

            if (m_dOr.Count > 0)
            {
                searchObject.m_dOr = m_dOr;
            }

            if (m_dAnd.Count > 0)
            {
                searchObject.m_dAnd = m_dAnd;
            }
        }

        public enum eESFeederType
        {
            MEDIA,
            EPG
        }
    }
}
