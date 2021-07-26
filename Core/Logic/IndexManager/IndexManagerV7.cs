using System;
using System.Collections.Generic;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using ApiObjects.SearchObjects;
using ApiObjects.Statistics;
using Catalog.Response;
using Core.Catalog.Response;
using GroupsCacheManager;
using Nest;
using Newtonsoft.Json.Linq;
using Polly.Retry;
using ESUtils = ElasticSearch.Common.Utils;
using ConfigurationManager;
using Status = ApiObjects.Response.Status;
using System.Linq;
using KLogMonitor;
using System.Reflection;
using Core.Catalog.CatalogManagement;
using ElasticSearch.Common;
using CachingProvider.LayeredCache;
using Core.Catalog.Cache;
using ApiLogic.Catalog;
using ElasticSearch.Searcher.Settings;
using ApiObjects.CanaryDeployment.Elasticsearch;

namespace Core.Catalog
{
    public class IndexManagerV7 : IIndexManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Consts

        private const int REFRESH_INTERVAL_FOR_EMPTY_INDEX_SECONDS = 10;

        #endregion

        #region Config Values

        private readonly int _numOfShards;
        private readonly int _numOfReplicas;
        private readonly int _maxResults;

        #endregion

        private readonly IApplicationConfiguration _applicationConfiguration;
        private readonly IElasticClient _elasticClient;
        private readonly ICatalogManager _catalogManager;
        private readonly IElasticSearchIndexDefinitions _esIndexDefinitions;
        private readonly ILayeredCache _layeredCache;
        private readonly IChannelManager _channelManager;
        private readonly ICatalogCache _catalogCache;
        private readonly IWatchRuleManager _watchRuleManager;

        private readonly int _partnerId;
        private bool _doesGroupUsesTemplates;
        private readonly IGroupManager _groupManager;
        private Group _group;
        private CatalogGroupCache _catalogGroupCache;

        public IndexManagerV7(int partnerId, 
            IElasticClient elasticClient, 
            IApplicationConfiguration applicationConfiguration,
            IGroupManager groupManager,
            ICatalogManager catalogManager,
            IElasticSearchIndexDefinitions esIndexDefinitions,
            IChannelManager channelManager,
            ICatalogCache catalogCache
            )
        {
            _elasticClient = elasticClient;
            _partnerId = partnerId;            
            _applicationConfiguration = applicationConfiguration;

            _catalogManager = catalogManager;
            _esIndexDefinitions = esIndexDefinitions;
            _channelManager = channelManager;
            _catalogCache = catalogCache;
            _partnerId = partnerId;
            _groupManager = groupManager;

            _numOfShards = _applicationConfiguration.ElasticSearchHandlerConfiguration.NumberOfShards.Value;
            _numOfReplicas = _applicationConfiguration.ElasticSearchHandlerConfiguration.NumberOfReplicas.Value;
            _maxResults = _applicationConfiguration.ElasticSearchConfiguration.MaxResults.Value;

            InitializePartnerData(_partnerId);
        }

        private void InitializePartnerData(int partnerId)
        {
            if (partnerId <= 0)
            {
                return;
            }

            _doesGroupUsesTemplates = _catalogManager.DoesGroupUsesTemplates(partnerId);

            if (_doesGroupUsesTemplates)
            {
                _catalogManager.TryGetCatalogGroupCacheFromCache(partnerId, out _catalogGroupCache);
            }
            else
            {
                _group = _groupManager.GetGroup(partnerId);
            }

            if (_catalogGroupCache == null && _group == null)
            {
                log.Error($"Could not load group configuration for {partnerId}");
            }
        }

        public bool UpsertMedia(long assetId)
        {
            var doc = new JObject();

            var indexResponse = _elasticClient.Index(doc, d => d.Index("index_name").Id("test_id"));

            return true;
        }

        public string SetupEpgV2Index(DateTime dateOfProgramsToIngest, RetryPolicy retryPolicy)
        {
            string dailyEpgIndexName = IndexingUtils.GetDailyEpgIndexName(_partnerId, dateOfProgramsToIngest);
            EnsureEpgIndexExist(dailyEpgIndexName, retryPolicy);
            SetNoRefresh(dailyEpgIndexName, retryPolicy);

            return dailyEpgIndexName;
        }

        public bool FinalizeEpgV2Index(DateTime date)
        {
            throw new NotImplementedException();
        }

        public bool FinalizeEpgV2Indices(List<DateTime> date, RetryPolicy retryPolicy)
        {
            throw new NotImplementedException();
        }

        public bool DeleteProgram(List<long> epgIds, IEnumerable<string> epgChannelIds)
        {
            throw new NotImplementedException();
        }

        public bool UpsertProgram(List<EpgCB> epgObjects, Dictionary<string, LinearChannelSettings> linearChannelSettings)
        {
            throw new NotImplementedException();
        }

        public bool DeleteChannelPercolator(List<int> channelIds)
        {
            throw new NotImplementedException();
        }

        public bool UpdateChannelPercolator(List<int> channelIds, Channel channel = null)
        {
            throw new NotImplementedException();
        }

        public bool DeleteChannel(int channelId)
        {
            throw new NotImplementedException();
        }

        public bool UpsertChannel(int channelId, Channel channel = null, long userId = 0)
        {
            throw new NotImplementedException();
        }

        public bool DeleteMedia(long assetId)
        {
            throw new NotImplementedException();
        }

        public void UpsertProgramsToDraftIndex(IList<EpgProgramBulkUploadObject> calculatedPrograms, string draftIndexName, DateTime dateOfProgramsToIngest,
            LanguageObj defaultLanguage, IDictionary<string, LanguageObj> languages)
        {
            throw new NotImplementedException();
        }

        public void DeleteProgramsFromIndex(IList<EpgProgramBulkUploadObject> programsToDelete, string epgIndexName, IDictionary<string, LanguageObj> languages)
        {
            throw new NotImplementedException();
        }

        public IList<EpgProgramBulkUploadObject> GetCurrentProgramsByDate(int channelId, DateTime fromDate, DateTime toDate)
        {
            throw new NotImplementedException();
        }

        public List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearch, ref int totalItems, ref int to)
        {
            throw new NotImplementedException();
        }

        public List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearch, ref int totalItems, ref int to, out List<AggregationsResult> aggregationsResult)
        {
            throw new NotImplementedException();
        }

        public AggregationsResult UnifiedSearchForGroupBy(UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            throw new NotImplementedException();
        }

        public List<UnifiedSearchResult> SearchSubscriptionAssets(List<BaseSearchObject> searchObjects, int languageId, bool useStartDate, string mediaTypes, OrderObj order,
            int pageIndex, int pageSize, ref int totalItems)
        {
            throw new NotImplementedException();
        }

        public List<int> GetEntitledEpgLinearChannels(UnifiedSearchDefinitions definitions)
        {
            throw new NotImplementedException();
        }

        public bool DoesMediaBelongToChannels(List<int> lChannelIDs, int nMediaID)
        {
            throw new NotImplementedException();
        }

        public List<int> GetMediaChannels(int mediaId)
        {
            throw new NotImplementedException();
        }

        public List<string> GetEpgAutoCompleteList(EpgSearchObj oSearch)
        {
            throw new NotImplementedException();
        }

        public List<List<string>> GetChannelsDefinitions(List<List<long>> listsOfChannelIDs)
        {
            throw new NotImplementedException();
        }

        public List<SearchResult> GetAssetsUpdateDate(eObjectType assetType, List<int> assetIds)
        {
            throw new NotImplementedException();
        }

        public List<int> SearchChannels(ChannelSearchDefinitions definitions, ref int totalItems)
        {
            throw new NotImplementedException();
        }

        public List<TagValue> SearchTags(TagSearchDefinitions definitions, out int totalItems)
        {
            throw new NotImplementedException();
        }

        public Status UpdateTag(TagValue tag)
        {
            throw new NotImplementedException();
        }

        public Status DeleteTag(long tagId)
        {
            throw new NotImplementedException();
        }

        public Status DeleteTagsByTopic(long topicId)
        {
            throw new NotImplementedException();
        }

        //DO NOT IMPLEMENT THIS METHOD
        public Status DeleteStatistics(DateTime until)
        {
            throw new NotImplementedException();
        }

        public List<UnifiedSearchResult> GetAssetsUpdateDates(List<UnifiedSearchResult> assets, ref int totalItems, int pageSize, int pageIndex,
            bool shouldIgnoreRecordings = false)
        {
            throw new NotImplementedException();
        }

        public void GetAssetStats(List<int> assetIDs, DateTime startDate, DateTime endDate, StatsType type,
            ref Dictionary<int, AssetStatsResult> assetIDsToStatsMapping)
        {
            throw new NotImplementedException();
        }

        public List<int> OrderMediaBySlidingWindow(OrderBy orderBy, bool isDesc, int pageSize, int PageIndex, List<int> media,
            DateTime windowTime)
        {
            throw new NotImplementedException();
        }

        public bool SetupSocialStatisticsDataIndex()
        {
            var statisticsIndex = ESUtils.GetGroupStatisticsIndex(_partnerId);
            var createIndexResponse = _elasticClient.Indices.Create(statisticsIndex,
                c => c.Settings(settings => 
                    settings.
                    NumberOfShards(_numOfShards).
                    NumberOfReplicas(_numOfReplicas)

                    //.Analysis(analysis => analysis.Analyzers(b => b.Custom("harta", new CustomAnalyzer())

                    ));
            bool result = createIndexResponse != null && createIndexResponse.Acknowledged && createIndexResponse.IsValid;
            
            return result;
        }

        public bool InsertSocialStatisticsData(SocialActionStatistics action)
        {
            var result = false;
            var guid = Guid.NewGuid();
            var statisticsIndex = ESUtils.GetGroupStatisticsIndex(_partnerId);

            try
            {
                var response = _elasticClient.Index(action, i => i.Index(statisticsIndex));
                result = response != null && response.IsValid && response.Result == Result.Created;
                
                if (!result)
                {
                    var actionStatsJson = Newtonsoft.Json.JsonConvert.SerializeObject(action);
                    log.Debug("InsertStatisticsToES " + string.Format("Was unable to insert record to ES. index={0};doc={1}",
                        statisticsIndex, actionStatsJson));
                }
            }
            catch (Exception ex)
            {
                log.Error($"InsertStatisticsToES - Failed ex={ex.Message}, group={_partnerId}", ex);
                result = false;
            }

            return result;
        }

        public bool DeleteSocialAction(StatisticsActionSearchObj socialSearch)
        {
            throw new NotImplementedException();
        }

        public string SetupIPToCountryIndex()
        {
            throw new NotImplementedException();
        }

        public bool InsertDataToIPToCountryIndex(string newIndexName, List<IPV4> ipV4ToCountryMapping, List<IPV6> ipV6ToCountryMapping)
        {
            throw new NotImplementedException();
        }

        public bool PublishIPToCountryIndex(string newIndexName)
        {
            throw new NotImplementedException();
        }

        public Country GetCountryByCountryName(string countryName)
        {
            throw new NotImplementedException();
        }

        public Country GetCountryByIp(string ip, out bool searchSuccess)
        {
            throw new NotImplementedException();
        }

        public List<string> GetChannelPrograms(int channelId, DateTime startDate, DateTime endDate, List<ESOrderObj> esOrderObjs)
        {
            throw new NotImplementedException();
        }

        public List<string> GetEpgCBDocumentIdsByEpgId(IEnumerable<long> epgIds, IEnumerable<LanguageObj> langCodes)
        {
            throw new NotImplementedException();
        }

        public string SetupMediaIndex(List<LanguageObj> languages, LanguageObj defaultLanguage)
        {
            throw new NotImplementedException();
        }

        public void InsertMedias(Dictionary<int, Dictionary<int, Media>> groupMedias, string newIndexName)
        {
            throw new NotImplementedException();
        }

        public void PublishMediaIndex(string newIndexName, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            throw new NotImplementedException();
        }

        public bool AddChannelsPercolatorsToIndex(HashSet<int> channelIds, string newIndexName, bool shouldCleanupInvalidChannels = false)
        {
            throw new NotImplementedException();
        }

        public string SetupChannelMetadataIndex()
        {
            throw new NotImplementedException();
        }

        public void AddChannelsMetadataToIndex(string newIndexName, List<Channel> allChannels)
        {
            throw new NotImplementedException();
        }

        public void PublishChannelsMetadataIndex(string newIndexName, bool shouldSwitchAlias, bool shouldDeleteOldIndices)
        {
            throw new NotImplementedException();
        }

        public string SetupTagsIndex()
        {
            throw new NotImplementedException();
        }

        public void AddTagsToIndex(string newIndexName, List<TagValue> allTagValues)
        {
            throw new NotImplementedException();
        }

        public bool PublishTagsIndex(string newIndexName, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            throw new NotImplementedException();
        }

        public string SetupEpgIndex(bool isRecording)
        {
            throw new NotImplementedException();
        }

        public void AddEPGsToIndex(string index, bool isRecording, Dictionary<ulong, Dictionary<string, EpgCB>> programs, Dictionary<long, List<int>> linearChannelsRegionsMapping,
            Dictionary<long, long> epgToRecordingMapping)
        {
            throw new NotImplementedException();
        }

        public bool FinishUpEpgIndex(string newIndexName, bool isRecording, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            throw new NotImplementedException();
        }

        public bool UpdateEpgs(List<EpgCB> epgObjects, bool isRecording, Dictionary<long, long> epgToRecordingMapping = null)
        {
            throw new NotImplementedException();
        }

        public SearchResultsObj SearchMedias(MediaSearchObj oSearch, int nLangID, bool bUseStartDate)
        {
            throw new NotImplementedException();
        }

        public SearchResultsObj SearchSubscriptionMedias(List<MediaSearchObj> oSearch, int nLangID, bool bUseStartDate, string sMediaTypes,
            OrderObj oOrderObj, int nPageIndex, int nPageSize)
        {
            throw new NotImplementedException();
        }

        public SearchResultsObj SearchEpgs(EpgSearchObj epgSearch)
        {
            throw new NotImplementedException();
        }

        public List<string> GetAutoCompleteList(MediaSearchObj oSearch, int nLangID, ref int nTotalItems)
        {
            throw new NotImplementedException();
        }

        public Dictionary<long, bool> ValidateMediaIDsInChannels(List<long> distinctMediaIDs,
            List<string> jsonizedChannelsDefinitionsMediasHaveToAppearInAtLeastOne,
            List<string> jsonizedChannelsDefinitionsMediasMustNotAppearInAll)
        {
            throw new NotImplementedException();
        }

        #region Private Methods

        private List<LanguageObj> GetLanguages()
        {
            return _doesGroupUsesTemplates ? _catalogGroupCache.LanguageMapById.Values.ToList() : _group.GetLangauges();
        }

        private LanguageObj GetDefaultLanguage()
        {
            return _doesGroupUsesTemplates ? _catalogGroupCache.GetDefaultLanguage() : _group.GetGroupDefaultLanguage();
        }

        private void EnsureEpgIndexExist(string dailyEpgIndexName, RetryPolicy retryPolicy)
        {
            // TODO it's possible to create new index with mappings and alias in one request,
            // https://www.elastic.co/guide/en/elasticsearch/reference/2.3/indices-create-index.html#mappings
            // but we have huge mappings and don't know the impact on timeouts during index creation - should be tested on real environment.

            // Limitation: it's not a transaction, we don't remove index when add-mapping failed =>
            // EPGs could be added to the index without mapping (e.g. from asset.add)
            try
            {
                AddEmptyIndex(dailyEpgIndexName, retryPolicy);
                AddEpgMappings(dailyEpgIndexName, retryPolicy);
                //AddAlias(dailyEpgIndexName, retryPolicy);
            }
            catch (Exception e)
            {
                log.Error($"index creation failed [{dailyEpgIndexName}]", e);
                throw new Exception($"index creation failed");
            }
        }

        private void SetNoRefresh(string dailyEpgIndexName, RetryPolicy retryPolicy)
        {
            // shut down refresh of index while bulk uploading
            retryPolicy.Execute(() =>
            {
                var updateDisableIndexRefresh = new UpdateIndexSettingsRequest(dailyEpgIndexName);
                updateDisableIndexRefresh.IndexSettings = new DynamicIndexSettings();
                updateDisableIndexRefresh.IndexSettings.RefreshInterval = Time.MinusOne;
                var updateSettingsResult = _elasticClient.Indices.UpdateSettings(updateDisableIndexRefresh);

                var isSetRefreshSuccess = updateSettingsResult != null && updateSettingsResult.Acknowledged && updateSettingsResult.IsValid;
                if (!isSetRefreshSuccess)
                {
                    log.Error($"index set refresh to -1 failed [false], dailyEpgIndexName [{dailyEpgIndexName}]");
                    throw new Exception("Could not set index refresh interval");
                }
            });
        }

        private void AddEmptyIndex(string dailyEpgIndexName, RetryPolicy retryPolicy)
        {
            retryPolicy.Execute(() =>
            {
                var isIndexExist = _elasticClient.Indices.Exists(dailyEpgIndexName);

                if (isIndexExist != null && isIndexExist.Exists)
                {
                    return;
                }

                log.Info($"creating new index [{dailyEpgIndexName}]");
                CreateEmptyEpgIndex(dailyEpgIndexName, true,
                    true, REFRESH_INTERVAL_FOR_EMPTY_INDEX_SECONDS);
            });
        }

        private void CreateEmptyEpgIndex(string newIndexName, bool shouldBuildWithReplicas = true,
            bool shouldUseNumOfConfiguredShards = true, int refreshIntervalSeconds = 0)
        {
            List<LanguageObj> languages = GetLanguages();
            GetAnalyzersWithLowercaseAndPhraseStartsWith(languages, out var analyzers, out var filters);
            int replicas = shouldBuildWithReplicas ? _numOfReplicas : 0;
            int shards = shouldUseNumOfConfiguredShards ? _numOfShards : 1;
            var isIndexCreated = false;
            AnalyzersDescriptor analyzersDescriptor = GetAnalyzersDesctiptor(analyzers);
            TokenFiltersDescriptor filtersDesctiptor = GetTokenFiltersDescriptor(filters);

            var createResponse = _elasticClient.Indices.Create(newIndexName,
                c => c.Settings(settings => settings
                    .NumberOfShards(shards)
                    .NumberOfReplicas(replicas)
                    .RefreshInterval(new Time(refreshIntervalSeconds, TimeUnit.Second))
                    .Setting("index.max_result_window", _maxResults)
                    .Setting("index.max_ngram_diff", 20)
                    // TODO: convert to tcm...
                    .Setting("index.mapping.total_fields.limit", 2600)
                    .Analysis(a => a
                        .Analyzers(an => analyzersDescriptor)
                        .TokenFilters(tf => filtersDesctiptor)
                    )
                    ));

            isIndexCreated = createResponse != null && createResponse.Acknowledged && createResponse.IsValid;
            if (!isIndexCreated) { throw new Exception(string.Format("Failed creating index for index:{0}", newIndexName)); }
        }

        private TokenFiltersDescriptor GetTokenFiltersDescriptor(Dictionary<string, ElasticSearch.Searcher.Settings.Filter> filters)
        {
            TokenFiltersDescriptor filtersDesctiptor = new TokenFiltersDescriptor();

            foreach (var filter in filters)
            {
                switch (filter.Value.type)
                {
                    case "nGram":
                        {
                            var castedFilter = filter.Value as NgramFilter;
                            filtersDesctiptor =
                                filtersDesctiptor.NGram(filter.Key,
                                    f => f
                                    .MinGram(castedFilter.min_gram)
                                    .MaxGram(castedFilter.max_gram)
                                );
                            break;
                        }
                    case "edgeNGram":
                        {
                            var castedFilter = filter.Value as NgramFilter;
                            filtersDesctiptor =
                                filtersDesctiptor.EdgeNGram(filter.Key,
                                    f => f
                                    .MinGram(castedFilter.min_gram)
                                    .MaxGram(castedFilter.max_gram)
                                );
                            break;
                        }
                    default:
                        break;
                }
            }

            return filtersDesctiptor;
        }

        private AnalyzersDescriptor GetAnalyzersDesctiptor(Dictionary<string, Analyzer> analyzers)
        {
            AnalyzersDescriptor analyzersDescriptor = new AnalyzersDescriptor();

            foreach (var analyzer in analyzers)
            {
                analyzersDescriptor = analyzersDescriptor.Custom(analyzer.Key,
                    ca => ca
                    .CharFilters(analyzer.Value.char_filter)
                    .Tokenizer(analyzer.Value.tokenizer)
                    .Filters(analyzer.Value.filter)
                );
            }

            return analyzersDescriptor;
        }

        private void GetAnalyzersWithLowercaseAndPhraseStartsWith(IEnumerable<LanguageObj> languages, 
            out Dictionary<string, Analyzer> analyzers, 
            out Dictionary<string, ElasticSearch.Searcher.Settings.Filter> filters)
        {
            analyzers = new Dictionary<string, Analyzer>();
            filters = new Dictionary<string, ElasticSearch.Searcher.Settings.Filter>();

            if (languages != null)
            {
                GetAnalyzersWithLowercase(languages, analyzers, filters);

                analyzers.Add("phrase_starts_with_analyzer", new Analyzer()
                {
                    tokenizer = "keyword",
                    filter = new List<string>()
                    {
                        "lowercase",
                        "edgengram_filter",
                        "icu_folding",
                        "icu_normalizer",
                        "asciifolding"
                    },
                    char_filter = new List<string>()
                    {
                        "html_strip"
                    }
                });
                analyzers.Add("phrase_starts_with_search_analyzer", new Analyzer()
                {
                    tokenizer = "keyword",
                    filter = new List<string>()
                    {
                        "lowercase",
                        "icu_folding",
                        "icu_normalizer",
                        "asciifolding"
                    },
                    char_filter = new List<string>()
                    {
                        "html_strip"
                    }
                });

                filters.Add("edgengram_filter", new ElasticSearch.Searcher.Settings.NgramFilter()
                {
                    type = "edgeNGram",
                    min_gram = 1,
                    max_gram = 20,
                    token_chars = new List<string>()
                    {
                        "letter",
                        "digit",
                        "punctuation",
                        "symbol"
                    }
                });
            }
        }

        private void GetAnalyzersWithLowercase(IEnumerable<LanguageObj> languages, Dictionary<string, Analyzer> analyzers, Dictionary<string, ElasticSearch.Searcher.Settings.Filter> filters)
        {
            GetAnalyzersAndFiltersFromConfiguration(languages, analyzers, filters);

            // we always want a lowercase analyzer
            // we always want "autocomplete" ability
            analyzers.Add("lowercase_analyzer",
                new Analyzer()
                {
                    tokenizer = "keyword",
                    char_filter = new List<string>()
                    {
                            "html_strip"
                    },
                    filter = new List<string>()
                    {
                            "lowercase",
                            "asciifolding"
                    }
                }
            );
        }

        private void GetAnalyzersAndFiltersFromConfiguration(IEnumerable<LanguageObj> languages, Dictionary<string, Analyzer> analyzers, Dictionary<string, ElasticSearch.Searcher.Settings.Filter> filters)
        {
            foreach (LanguageObj language in languages)
            {
                var currentAnalyzers = _esIndexDefinitions.GetAnalyzers(ElasticsearchVersion.ES_7_13, language.Code);
                var currentFilters = _esIndexDefinitions.GetFilters(ElasticsearchVersion.ES_7_13, language.Code);

                if (currentAnalyzers == null)
                {
                    log.Error(string.Format("analyzer for language {0} doesn't exist", language.Code));
                }
                else
                {
                    foreach (var analyzer in currentAnalyzers)
                    {
                        analyzers.Add(analyzer.Key, analyzer.Value);
                    }
                }

                if (currentFilters != null)
                {
                    foreach (var filter in currentFilters)
                    {
                        filters.Add(filter.Key, filter.Value);
                    }
                }
            }
        }

        private void AddEpgMappings(string dailyEpgIndexName, RetryPolicy retryPolicy)
        {
            var languages = GetLanguages();

            log.Info($"creating mappings. index [{dailyEpgIndexName}], languages [{languages.Select(_ => _.Name)}]");

            _groupManager.RemoveGroup(_partnerId); // remove from cache
            _group = _groupManager.GetGroup(_partnerId);
            var doesGroupUsesTemplates = _doesGroupUsesTemplates;

            if (!this.GetMetasAndTagsForMapping(
                out var metas,
                out var tags,
                out var metasToPad,
                true))
            {
                throw new Exception($"failed to get metas and tags");
            }

            var defaultLanguage = GetDefaultLanguage();
            foreach (var language in languages)
            {
                //retryPolicy.Execute(() =>
                //    this.AddLanguageMapping(dailyEpgIndexName, language, defaultLanguage, metas, tags, metasToPad));
            }
        }

        private bool GetMetasAndTagsForMapping(
            out Dictionary<string, KeyValuePair<eESFieldType, string>> metas,
            out List<string> tags,
            out HashSet<string> metasToPad, bool isEpg = false)
        {

            bool result = true;
            tags = new List<string>();
            metas = new Dictionary<string, KeyValuePair<eESFieldType, string>>();

            // Padded with zero prefix metas to sort numbers by text without issues in elastic (Brilliant!)
            metasToPad = new HashSet<string>();

            if (_doesGroupUsesTemplates && _catalogGroupCache != null)
            {
                try
                {
                    HashSet<string> topicsToIgnore = Core.Catalog.CatalogLogic.GetTopicsToIgnoreOnBuildIndex();
                    tags = _catalogGroupCache.TopicsMapBySystemNameAndByType.Where(x => x.Value.ContainsKey(ApiObjects.MetaType.Tag.ToString()) && !topicsToIgnore.Contains(x.Key)).Select(x => x.Key.ToLower()).ToList();

                    foreach (KeyValuePair<string, Dictionary<string, Topic>> topics in _catalogGroupCache.TopicsMapBySystemNameAndByType)
                    {
                        //TODO anat ask Ira
                        if (topics.Value.Keys.Any(x => x != ApiObjects.MetaType.Tag.ToString() && x != ApiObjects.MetaType.ReleatedEntity.ToString()))
                        {
                            string nullValue = string.Empty;
                            eESFieldType metaType;
                            ApiObjects.MetaType topicMetaType = CatalogManager.GetTopicMetaType(topics.Value);
                            IndexingUtils.GetMetaType(topicMetaType, out metaType, out nullValue);

                            if (topicMetaType == ApiObjects.MetaType.Number && !metasToPad.Contains(topics.Key.ToLower()))
                            {
                                metasToPad.Add(topics.Key.ToLower());
                            }

                            if (!metas.ContainsKey(topics.Key.ToLower()))
                            {
                                metas.Add(topics.Key.ToLower(), new KeyValuePair<eESFieldType, string>(isEpg ? eESFieldType.STRING : metaType, nullValue));
                            }
                            else
                            {
                                log.ErrorFormat("Duplicate topic found for group {0} name {1}", _partnerId, topics.Key.ToLower());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Failed BuildIndex for _partnerId: {0} because CatalogGroupCache", _partnerId), ex);
                    return false;
                }
            }
            else if (_group != null)
            {
                try
                {
                    if (_group.m_oEpgGroupSettings != null && _group.m_oEpgGroupSettings.m_lTagsName != null)
                    {
                        foreach (var item in _group.m_oEpgGroupSettings.m_lTagsName)
                        {
                            if (!tags.Contains(item.ToLower()))
                            {
                                tags.Add(item.ToLower());
                            }
                        }
                    }

                    if (_group.m_oGroupTags != null)
                    {
                        foreach (var item in _group.m_oGroupTags.Values)
                        {
                            if (!tags.Contains(item.ToLower()))
                            {
                                tags.Add(item.ToLower());
                            }
                        }
                    }

                    var realMetasType = new Dictionary<string, eESFieldType>();
                    if (_group.m_oMetasValuesByGroupId != null)
                    {
                        foreach (Dictionary<string, string> metaMap in _group.m_oMetasValuesByGroupId.Values)
                        {
                            foreach (KeyValuePair<string, string> meta in metaMap)
                            {
                                string nullValue = string.Empty;
                                eESFieldType metaType;
                                IndexingUtils.GetMetaType(meta.Key, out metaType, out nullValue);

                                var metaName = meta.Value.ToLower();
                                if (!metas.ContainsKey(metaName))
                                {
                                    realMetasType.Add(metaName, metaType);
                                    metas.Add(metaName, new KeyValuePair<eESFieldType, string>(isEpg ? eESFieldType.STRING : metaType, nullValue));
                                }
                                else
                                {
                                    log.WarnFormat("Duplicate media meta found for group {0} name {1}", _partnerId, meta.Value);
                                }
                            }
                        }
                    }

                    if (_group.m_oEpgGroupSettings != null && _group.m_oEpgGroupSettings.m_lMetasName != null)
                    {
                        foreach (string epgMeta in _group.m_oEpgGroupSettings.m_lMetasName)
                        {
                            string nullValue = string.Empty;
                            eESFieldType metaType;
                            IndexingUtils.GetMetaType(epgMeta, out metaType, out nullValue);

                            var epgMetaName = epgMeta.ToLower();
                            if (!metas.ContainsKey(epgMetaName))
                            {
                                realMetasType.Add(epgMetaName, metaType);
                                metas.Add(epgMetaName, new KeyValuePair<eESFieldType, string>(isEpg ? eESFieldType.STRING : metaType, nullValue));
                            }
                            else
                            {
                                var mediaMetaType = realMetasType[epgMetaName];

                                // If the metas is numeric for media and it exists also for epg, we will have problems with sorting 
                                // (since epg metas are string and there will be a type mismatch)
                                // the solution is to add another field of a padded string to the indices and sort by it
                                if (mediaMetaType == eESFieldType.INTEGER ||
                                    mediaMetaType == eESFieldType.DOUBLE ||
                                    mediaMetaType == eESFieldType.LONG)
                                {
                                    metasToPad.Add(epgMetaName);
                                }
                                else
                                {
                                    log.WarnFormat("Duplicate epg meta found for group {0} name {1}", _partnerId, epgMeta);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed get metas and tags for mapping for group {0} ex = {1}", _partnerId, ex);
                }
            }

            return result;
        }

        #endregion
    }
}