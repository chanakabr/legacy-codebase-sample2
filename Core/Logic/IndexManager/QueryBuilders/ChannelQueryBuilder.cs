using ApiLogic.Catalog;
using ApiLogic.IndexManager.NestData;
using ApiLogic.IndexManager.QueryBuilders.NestQueryBuilders;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Api.Managers;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Request;
using ElasticSearch.Searcher;
using GroupsCacheManager;
using Phx.Lib.Log;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiLogic.IndexManager.Sorting;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ElasticSearch.Utils;

namespace ApiLogic.IndexManager.QueryBuilders
{
    public interface IChannelQueryBuilder
    {
        MediaSearchObj BuildBaseChannelSearchObject(Channel channel);
        UnifiedSearchDefinitions BuildSearchDefinitions(Channel channel);

        NestPercolatedQuery GetChannelQuery(Channel currentChannel);
        string GetChannelQueryString(ESMediaQueryBuilder mediaQueryParser, ESUnifiedQueryBuilder unifiedQueryBuilder, Channel currentChannel);
    }

    public class ChannelQueryBuilder : IChannelQueryBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<ChannelQueryBuilder> LazyV2 = new Lazy<ChannelQueryBuilder>(
            () => new ChannelQueryBuilder(ElasticsearchVersion.ES_2_3),
            LazyThreadSafetyMode.PublicationOnly);

        private static readonly Lazy<ChannelQueryBuilder> LazyV7 = new Lazy<ChannelQueryBuilder>(
            () => new ChannelQueryBuilder(ElasticsearchVersion.ES_7),
            LazyThreadSafetyMode.PublicationOnly);

        public static ChannelQueryBuilder Instance(ElasticsearchVersion version)
            => version == ElasticsearchVersion.ES_2_3 ? LazyV2.Value : LazyV7.Value;


        private IWatchRuleManager _watchRuleManager;
        private ICatalogManager _catalogManager;
        private IGroupManager _groupManager;
        private readonly IEsSortingService _esSortingService;
        private readonly ISortingAdapter _sortingAdapter;
        private readonly IUnifiedQueryBuilderInitializer _queryInitializer;
        private readonly IChannelSearchOptionsService _channelSearchOptionsService;
        public ChannelQueryBuilder(ElasticsearchVersion version)
        {
            _watchRuleManager = WatchRuleManager.Instance;
            _catalogManager = CatalogManager.Instance;
            _groupManager = new GroupManager();
            _esSortingService = EsSortingService.Instance(version);
            _sortingAdapter = SortingAdapter.Instance;
            _queryInitializer = UnifiedQueryBuilderInitializer.Instance(version);
            _channelSearchOptionsService = ChannelSearchOptionsService.Instance;
        }

        public ChannelQueryBuilder(
            IWatchRuleManager watchRuleManager,
            ICatalogManager catalogManager,
            IGroupManager groupManager,
            IEsSortingService esSortingService,
            ISortingAdapter sortingAdapter)
        {
            _watchRuleManager = watchRuleManager;
            _catalogManager = catalogManager;
            _groupManager = groupManager;
            _esSortingService = esSortingService;
            _sortingAdapter = sortingAdapter;
        }

        public string GetChannelQueryString(ESMediaQueryBuilder mediaQueryParser, ESUnifiedQueryBuilder unifiedQueryBuilder,
            Channel currentChannel)
        {
            if (unifiedQueryBuilder is null)
            {
                throw new ArgumentNullException(nameof(unifiedQueryBuilder));
            }

            string channelQuery = string.Empty;

            if (currentChannel == null)
            {
                log.ErrorFormat("BuildChannelQueries - All channels list has null or in-active channel, continuing");
                return channelQuery;
            }

            int groupId = currentChannel.m_nParentGroupID;
            bool doesGroupUseTemplates = _catalogManager.DoesGroupUsesTemplates(groupId);

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
                            var mediaIds = currentChannel.m_lChannelTags.SelectMany(x => x.m_lValue);
                            currentChannel.filterQuery = $"(or media_id:'{string.Join(",", mediaIds)}')";
                        }

                        UnifiedSearchDefinitions definitions = BuildSearchDefinitions(currentChannel);

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
                    MediaSearchObj mediaSearchObject = BuildBaseChannelSearchObject(currentChannel);

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

        public UnifiedSearchDefinitions BuildSearchDefinitions(Channel channel)
        {
            var definitions = new UnifiedSearchDefinitions
            {
                groupId = channel.m_nGroupID,
                mediaTypes = channel.m_nMediaType.ToList()
            };

            var group = _groupManager.GetGroup(definitions.groupId);

            var dummyRequest = new BaseRequest
            {
                domainId = 0,
                m_nGroupID = channel.m_nParentGroupID,
                m_nPageIndex = 0,
                m_nPageSize = 0,
                m_oFilter = new Core.Catalog.Filter(),
                m_sSiteGuid = string.Empty,
                m_sUserIP = string.Empty
            };

            if (!string.IsNullOrEmpty(channel.filterQuery))
            {
                BooleanPhraseNode filterTree = null;
                var parseStatus = BooleanPhraseNode.ParseSearchExpression(channel.filterQuery, ref filterTree);
                if (parseStatus.Code != (int)eResponseStatus.OK)
                {
                    throw new KalturaException(parseStatus.Message, parseStatus.Code);
                }

                definitions.filterPhrase = filterTree;
                CatalogLogic.UpdateNodeTreeFields(dummyRequest,
                    ref definitions.filterPhrase, definitions, group, channel.m_nParentGroupID, _catalogManager);
            }

            if (channel.m_nChannelTypeID == (int)ChannelType.KSQL)
            {
                var searchOptionsContext = new ChannelSearchOptionsContext
                {
                    CatalogGroupCache = GetCatalogGroupCache(definitions.groupId),
                    InitialTree = definitions.filterPhrase,
                    MediaTypes = definitions.mediaTypes
                };

                var searchOptionsResult = _channelSearchOptionsService.ResolveKsqlChannelSearchOptions(searchOptionsContext);
                definitions.shouldSearchEpg = searchOptionsResult.ShouldSearchEpg;
                definitions.shouldSearchMedia = searchOptionsResult.ShouldSearchMedia;
                definitions.mediaTypes = searchOptionsResult.MediaTypes.ToList();
            }
            else
            {
                definitions.shouldSearchMedia = true;
                definitions.shouldSearchEpg = channel.m_nChannelTypeID == (int) ChannelType.Manual
                    && channel.m_lChannelTags?.Any(x => x.m_sKey.Equals("epg_id")) == true;

                // if it contains ONLY 0 - it means search all
                if (definitions.mediaTypes.Count == 1 && definitions.mediaTypes.Contains(0))
                {
                    definitions.mediaTypes.Remove(0);
                }
            }

            definitions.permittedWatchRules = GetPermittedWatchRules(channel.m_nGroupID);
            definitions.order = new OrderObj();

            definitions.shouldUseStartDateForMedia = false;
            definitions.shouldUseFinalEndDate = false;

            if (channel.AssetUserRuleId.HasValue && channel.AssetUserRuleId.Value > 0)
            {
                var assetUserRule = AssetUserRuleManager.Instance.GetAssetUserRuleByRuleId(channel.m_nGroupID, channel.AssetUserRuleId.Value);

                if (assetUserRule != null && assetUserRule.Status != null && assetUserRule.Status.Code == (int)eResponseStatus.OK && assetUserRule.Object != null)
                {
                    BooleanPhraseNode phrase = null;

                    var rulesIds = new List<long>();
                    string queryString = string.Empty;

                    UnifiedSearchDefinitionsBuilder.GetQueryStringFromAssetUserRules(channel.m_nGroupID, new List<ApiObjects.Rules.AssetUserRule>()
                        {
                            assetUserRule.Object
                        },
                        out rulesIds,
                        out queryString);

                    BooleanPhrase.ParseSearchExpression(queryString, ref phrase);

                    CatalogLogic.UpdateNodeTreeFields(dummyRequest, ref phrase, definitions, group, group.m_nParentGroupID, _catalogManager);

                    definitions.assetUserRuleFilterPhrase = phrase;
                }
            }

            return definitions;
        }

        public MediaSearchObj BuildBaseChannelSearchObject(Channel channel)
        {
            MediaSearchObj searchObject = new MediaSearchObj();
            searchObject.m_oLangauge = GetDefaultLanguage(channel.m_nParentGroupID);
            searchObject.m_nGroupId = channel.m_nGroupID;
            searchObject.m_bExact = true;
            searchObject.m_eCutWith = channel.m_eCutWith;

            if (channel.m_nMediaType != null)
            {
                searchObject.m_sMediaTypes = string.Join(";", channel.m_nMediaType.Select(type => type.ToString()));
            }

            searchObject.m_sPermittedWatchRules = GetPermittedWatchRules(channel.m_nGroupID);
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

        private string GetPermittedWatchRules(int nGroupId)
        {
            List<string> groupPermittedWatchRules = _watchRuleManager.GetGroupPermittedWatchRules(nGroupId);
            string sRules = string.Empty;

            if (groupPermittedWatchRules != null && groupPermittedWatchRules.Count > 0)
            {
                sRules = string.Join(" ", groupPermittedWatchRules);
            }

            return sRules;
        }

        private void CopySearchValuesToSearchObjects(ref MediaSearchObj searchObject, CutWith cutWith, List<SearchValue> channelSearchValues)
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

        public NestPercolatedQuery GetChannelQuery(Channel channel)
        {
            QueryContainer query = null;

            if (channel == null)
            {
                log.ErrorFormat("BuildChannelQueries - All channels list has null or in-active channel, continuing");
                return null;
            }

            int groupId = channel.m_nParentGroupID;
            bool doesGroupUseTemplates = _catalogManager.DoesGroupUsesTemplates(groupId);

            // if group uses templates - index inactive channel as well
            if (!doesGroupUseTemplates && channel.m_nIsActive != 1)
            {
                log.ErrorFormat("GetChannelQuery - channel is inactive");
                return null;
            }

            try
            {
                log.DebugFormat("GetChannelQuery - Current channel  = {0}", channel.m_nChannelID);

                if ((channel.m_nChannelTypeID == (int)ChannelType.KSQL) ||
                   (channel.m_nChannelTypeID == (int)ChannelType.Manual && doesGroupUseTemplates && channel.AssetUserRuleId > 0))
                {
                    if (channel.m_nChannelTypeID == (int)ChannelType.Manual && channel.AssetUserRuleId > 0)
                    {
                        var mediaIds = channel.m_lChannelTags.SelectMany(x => x.m_lValue);
                        channel.filterQuery = $"(or media_id:'{string.Join(",", mediaIds)}')";
                    }

                    UnifiedSearchDefinitions definitions = BuildSearchDefinitions(channel);
                    definitions.shouldSearchEpg = false;
                    definitions.shouldIgnoreDeviceRuleID = true;
                    
                    // fill language with default language if not specified
                    if (definitions.langauge == null)
                    {
                        definitions.langauge = GetDefaultLanguage(groupId);
                    }

                    var nestBuilder = new UnifiedSearchNestBuilder(_esSortingService, _sortingAdapter, _queryInitializer)
                    {
                        SearchDefinitions = definitions
                    };

                    query = nestBuilder.GetQuery();
                }
                else
                {
                    MediaSearchObj mediaSearchObject = BuildBaseChannelSearchObject(channel);
                    mediaSearchObject.m_bIgnoreDeviceRuleId = true;

                    var nestBuilder = new UnifiedSearchNestMediaBuilder()
                    {
                        Definitions = mediaSearchObject,
                    };

                    query = nestBuilder.GetQuery();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetChannelQuery - building query for channel {0} has failed, ex = {1}", channel.m_nChannelID, ex);
            }

            if (query == null)
            {
                return null;
            }

            var result = new NestPercolatedQuery()
            {
                Query = query,
                ChannelId = channel.m_nChannelID
            };

            return result;
        }

        private LanguageObj GetDefaultLanguage(int partnerId)
        {
            return VerifyGroupUsesTemplates(partnerId) ? GetCatalogGroupCache(partnerId).GetDefaultLanguage() : GetGroupManager(partnerId).GetGroupDefaultLanguage();
        }

        private bool VerifyGroupUsesTemplates(int partnerId)
        {
            return _catalogManager.DoesGroupUsesTemplates(partnerId);
        }

        private CatalogGroupCache GetCatalogGroupCache(int partnerId)
        {
            CatalogGroupCache catalogGroupCache;
            _catalogManager.TryGetCatalogGroupCacheFromCache(partnerId, out catalogGroupCache);
            return catalogGroupCache;
        }

        private GroupsCacheManager.Group GetGroupManager(int partnerId)
        {
            return _groupManager.GetGroup(partnerId);
        }
    }
}
