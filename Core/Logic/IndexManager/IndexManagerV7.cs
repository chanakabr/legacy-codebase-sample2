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
using ApiLogic.IndexManager.Helpers;
using ElasticSearch.Searcher.Settings;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ApiObjects.Nest;
using ElasticSearch.NEST;
using Elasticsearch.Net;
using ElasticSearch.Searcher;
using ElasticSearch.Utilities;
using Newtonsoft.Json;
using Polly;

namespace Core.Catalog
{
    public class IndexManagerV7 : IIndexManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Consts

        private const int REFRESH_INTERVAL_FOR_EMPTY_INDEX_SECONDS = 10;
        private const string INDEX_REFRESH_INTERVAL = "10s";

        protected const string LOWERCASE_ANALYZER = "lowercase_analyzer";
        protected const string DEFAULT_INDEX_ANALYZER = "index_analyzer";
        protected const string DEFAULT_SEARCH_ANALYZER = "search_analyzer";
        protected const string AUTOCOMPLETE_ANALYZER = "autocomplete_analyzer";
        protected const string AUTOCOMPLETE_SEARCH_ANALYZER = "autocomplete_search_analyzer";
        protected const string PHRASE_STARTS_WITH_ANALYZER = "phrase_starts_with_analyzer";
        protected const string PHRASE_STARTS_WITH_SEARCH_ANALYZER = "phrase_starts_with_search_analyzer";
        protected const string EDGENGRAM_FILTER = "edgengram_filter";
        protected const string NGRAM_FILTER = "ngram_filter";
        
        public const string META_SUPPRESSED = "suppressed";

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
        private readonly int _sizeOfBulk;
        private readonly int _sizeOfBulkDefaultValue;
        private readonly ITtlService _ttlService;
        
        private HashSet<string> _metasToPad;
        private Group _group;
        private CatalogGroupCache _catalogGroupCache;

        public IndexManagerV7(int partnerId,
            IElasticClient elasticClient,
            IApplicationConfiguration applicationConfiguration,
            IGroupManager groupManager,
            ICatalogManager catalogManager,
            IElasticSearchIndexDefinitions esIndexDefinitions,
            IChannelManager channelManager,
            ICatalogCache catalogCache,
            ITtlService ttlService
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
            _ttlService = ttlService;

            //init all ES const
            _numOfShards = _applicationConfiguration.ElasticSearchHandlerConfiguration.NumberOfShards.Value;
            _numOfReplicas = _applicationConfiguration.ElasticSearchHandlerConfiguration.NumberOfReplicas.Value;
            _maxResults = _applicationConfiguration.ElasticSearchConfiguration.MaxResults.Value;
            _sizeOfBulk = _applicationConfiguration.ElasticSearchHandlerConfiguration.BulkSize.Value;
            _sizeOfBulkDefaultValue =_applicationConfiguration.ElasticSearchHandlerConfiguration.BulkSize.GetDefaultValue();

            InitializePartnerData(_partnerId);
            GetMetasAndTagsForMapping(out _, out _, out _metasToPad);
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
            var dailyEpgIndexName = IndexingUtils.GetDailyEpgIndexName(_partnerId, date);
            var response = _elasticClient.Indices.Refresh(new RefreshRequest(dailyEpgIndexName));
            return response.IsValid;
        }

        public bool FinalizeEpgV2Indices(List<DateTime> dates, RetryPolicy retryPolicy)
        {
            var indices = dates.Select(x => IndexingUtils.GetDailyEpgIndexName(_partnerId, x));
            var existingIndices = indices.Select(x => x).Where(x => _elasticClient.Indices.Exists(x).IsValid).ToList();

            foreach (var index in existingIndices)
            {
                var response = _elasticClient.Indices.UpdateSettings(Indices.Index(index),
                    x => x.IndexSettings(y => y.RefreshInterval(INDEX_REFRESH_INTERVAL)));
                
                if (!response.IsValid)
                {
                    retryPolicy.Execute(() =>
                    {
                        var isSetRefreshSuccess = _elasticClient.Indices.UpdateSettings(Indices.Index(index),
                            x => x.IndexSettings(y => y.RefreshInterval(INDEX_REFRESH_INTERVAL))).IsValid;
                        
                        if (!isSetRefreshSuccess)
                        {
                            log.Error($"index {index} set refresh to -1 failed [{isSetRefreshSuccess}]]");
                            throw new Exception("Could not set index refresh interval");
                        }
                    });
                }
            }
            
            return true;
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

        public void UpsertProgramsToDraftIndex(IList<EpgProgramBulkUploadObject> calculatedPrograms, 
            string draftIndexName,
            DateTime dateOfProgramsToIngest,
            LanguageObj defaultLanguage,
            IDictionary<string, LanguageObj> languages)
        {
            var bulkSize = _sizeOfBulk;
            if (_sizeOfBulk == 0 || _sizeOfBulk > _sizeOfBulkDefaultValue)
            {
                bulkSize = _sizeOfBulkDefaultValue;
            }

            var retryCount = 5;
            var policy = RetryPolicy.Handle<Exception>()
            .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time, attempt, ctx) =>
            {
                log.Warn($"upsert attemp [{attempt}/{retryCount}] Failed, waiting for:[{time.TotalSeconds}] seconds.", ex);
            });

            policy.Execute(() =>
            {
                var bulkRequests = new List<NestEsBulkRequest<string,NestEpg>>();
                try
                {
                    var programTranslationsToIndex = calculatedPrograms.SelectMany(p => p.EpgCbObjects);
                    foreach (var program in programTranslationsToIndex)
                    {
                        program.PadMetas(_metasToPad);
                        var suffix = program.Language == defaultLanguage.Code ? "" : program.Language;
                        var language = languages[program.Language];

                        // Serialize EPG object to string
                        var buildEpg = new ElasticSearchNestDataBuilder().BuildEpg(program, suffix, isOpc: _doesGroupUsesTemplates);
                        var epgType = IndexManagerCommonHelpers.GetTranslationType(IndexManagerV2.EPG_INDEX_TYPE, language);
                        var bulkRequest = GetNestEsBulkRequest(draftIndexName, dateOfProgramsToIngest, program, buildEpg, epgType);
                        bulkRequests.Add(bulkRequest);

                        // If we exceeded maximum size of bulk 
                        if (bulkRequests.Count >= bulkSize)
                        {
                            ExecuteAndValidateBulkRequests(bulkRequests);
                        }
                    }

                    // If we have anything left that is less than the size of the bulk
                    if (bulkRequests.Count > 0)
                    {
                        ExecuteAndValidateBulkRequests(bulkRequests);
                    }
                }
                finally
                {
                    if (bulkRequests != null && bulkRequests.Any())
                    {
                        log.Debug($"Clearing bulk requests");
                        bulkRequests.Clear();
                    }
                }
            });
        }

        private NestEsBulkRequest<string, NestEpg> GetNestEsBulkRequest(string draftIndexName, DateTime dateOfProgramsToIngest, EpgCB program,
            NestEpg buildEpg, string epgType)
        {
            var totalMinutes = _ttlService.GetEpgTtlMinutes(program);
            totalMinutes = totalMinutes < 0 ? 10 : totalMinutes;

            var bulkRequest = new NestEsBulkRequest<string, NestEpg>()
            {
                DocID = program.EpgID.ToString(),
                Document = buildEpg,
                Index = draftIndexName,
                Operation = eOperation.index,
                Routing = dateOfProgramsToIngest.Date.ToString("yyyyMMdd"),
                Type = epgType,
                TTL = $"{totalMinutes}m"
            };
            return bulkRequest;
        }

        private void ExecuteAndValidateBulkRequests<K>(List<NestEsBulkRequest<string, K>> bulkRequests)
            where K : class
        {
            var bulkResponse = ExecuteBulkRequest(bulkRequests);

            //no errors clear and end
            if (!bulkResponse.ItemsWithErrors.Any())
            {
                bulkRequests.Clear();
                return;
            }
            
            foreach (var item in bulkResponse.ItemsWithErrors)
            {
                log.Error($"Could not add item to ES index. GroupID={_partnerId} Id={item.Id} Index ={item.Index} error={item.Error}");
            }

            throw new Exception($"Failed to upsert [{bulkResponse.ItemsWithErrors.Count()}] documents");
        }

        private BulkResponse ExecuteBulkRequest<K>(List<NestEsBulkRequest<string, K>> bulkRequests) where K : class
        {
            var docToBulkReq = bulkRequests.ToDictionary(x => x.Document, x => x);
            var docs = bulkRequests.Select(x => x.Document).ToList();

            var bulkResponse = _elasticClient.Bulk(b =>
            {
                return b.IndexMany(docs.ToArray(), (descriptor, data) =>
                {
                    var bulkRequest = docToBulkReq[data];
                    return descriptor
                            .Index(bulkRequest.Index)
                            .Document(data)
                            .Routing(bulkRequest.Routing)
                            .Id(bulkRequest.DocID)
                        ;
                });
            });
            return bulkResponse;
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
            var index = ESUtils.GetGroupStatisticsIndex(_partnerId);

            try
            {
                if (_elasticClient.Indices.Exists(index).Exists)
                {
                    var queryBuilder = new ESStatisticsQueryBuilder(_partnerId, socialSearch);
                    var queryString = queryBuilder.BuildQuery();
                    //TODO IMPLEMENT THIS METHOD!
                    throw new NotImplementedException();
                    //return _elasticSearchApi.DeleteDocsByQuery(index, ESUtils.ES_STATS_TYPE, ref queryString);
                }
            }
            catch (Exception ex)
            {
                log.DebugFormat("DeleteActionFromES Failed ex={0}, index={1};type={2}", ex, index, ESUtils.ES_STATS_TYPE);
            }

            return false;
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

        public string SetupMediaIndex()
        {
            string newIndexName = IndexingUtils.GetNewMediaIndexStr(_partnerId);

            int maxResults = 100000;
            // Default size of max results should be 100,000
            if (_maxResults > 0)
            {
                maxResults = _maxResults;
            }

            var languages = GetLanguages();
            var defaultLanguage = GetDefaultLanguage();

            // get definitions of analyzers, filters and tokenizers
            GetAnalyzersWithLowercaseAndPhraseStartsWith(languages, out var analyzers, out var filters);
            AnalyzersDescriptor analyzersDescriptor = GetAnalyzersDesctiptor(analyzers);
            TokenFiltersDescriptor filtersDesctiptor = GetTokenFiltersDescriptor(filters);

            _groupManager.RemoveGroup(_partnerId); // remove from cache
            _group = _groupManager.GetGroup(_partnerId);

            if (!this.GetMetasAndTagsForMapping(
                out var metas,
                out var tags,
                out var metasToPad))
            {
                throw new Exception($"failed to get metas and tags");
            }

            PropertiesDescriptor<object> propertiesDescriptor = 
                GetMediaPropertiesDesctiptor(languages, metas, tags, metasToPad, analyzers);
            var createResponse = _elasticClient.Indices.Create(newIndexName,
                c => c.Settings(settings => settings
                    .NumberOfShards(_numOfShards)
                    .NumberOfReplicas(_numOfReplicas)
                    .Setting("index.max_result_window", _maxResults)
                    .Setting("index.max_ngram_diff", 20)
                    // TODO: convert to tcm...
                    .Setting("index.mapping.total_fields.limit", 2600)
                    .Analysis(a => a
                        .Analyzers(an => analyzersDescriptor)
                        .TokenFilters(tf => filtersDesctiptor)
                    ))
                .Map(map => map.Properties(props => propertiesDescriptor)
                ));

            bool isIndexCreated = createResponse != null && createResponse.Acknowledged && createResponse.IsValid;
            if (!isIndexCreated)
            {
                log.Error(string.Format("Failed creating index for index:{0}", newIndexName));
                return string.Empty;
            }

            return newIndexName;
        }

        public void InsertMedias(Dictionary<int, Dictionary<int, Media>> groupMedias, string newIndexName)
        {
            throw new NotImplementedException();
        }

        public void PublishMediaIndex(string newIndexName, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            string alias = IndexingUtils.GetMediaIndexAlias(_partnerId);

            if (shouldSwitchIndexAlias)
            {
                var currentIndices = _elasticClient.GetIndicesPointingToAlias(alias);

                var aliasResult = _elasticClient.Indices.BulkAlias(aliases => 
                {
                    if (currentIndices?.Count > 0)
                    {
                        aliases.Remove(a => a.Alias(alias).Index("*"));
                    }
                    aliases.Add(a => a.Alias(alias).Index(newIndexName));
                    return aliases;
                });

                if (aliasResult != null && aliasResult.IsValid)
                {
                    log.Debug($"Set new alias {alias} for index {newIndexName}");

                    if (shouldDeleteOldIndices && currentIndices?.Count > 0)
                    {
                        var deleteResult = _elasticClient.Indices.Delete(Nest.Indices.Index(currentIndices));

                        if (deleteResult != null && deleteResult.IsValid)
                        {
                            log.Debug($"Deleted indices {string.Join(",", currentIndices)}");
                        }
                    }
                }
            }
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
                AddAlias(dailyEpgIndexName, retryPolicy);
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

            _groupManager.RemoveGroup(_partnerId); // remove from cache
            _group = _groupManager.GetGroup(_partnerId);

            if (!this.GetMetasAndTagsForMapping(
                out var metas,
                out var tags,
                out var metasToPad,
                true))
            {
                throw new Exception($"failed to get metas and tags");
            }

            PropertiesDescriptor<object> propertiesDescriptor = GetEpgPropertiesDesctiptor(languages, metas, tags, metasToPad, analyzers);
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
                    ))
                .Map(map => map.RoutingField(rf => new RoutingField() { Required = false }).Properties(props => propertiesDescriptor)
                ));

            isIndexCreated = createResponse != null && createResponse.Acknowledged && createResponse.IsValid;
            if (!isIndexCreated) { throw new Exception(string.Format("Failed creating index for index:{0}", newIndexName)); }
        }

        private PropertiesDescriptor<object> GetEpgPropertiesDesctiptor(List<LanguageObj> languages, 
            Dictionary<string, KeyValuePair<eESFieldType, string>> metas,
            List<string> tags,
            HashSet<string> metasToPad,
            Dictionary<string, Analyzer> analyzers)
        {
            PropertiesDescriptor<object> propertiesDescriptor = new PropertiesDescriptor<object>();

            propertiesDescriptor
                .Number(x => x.Name("epg_id").Type(NumberType.Long))
                .Number(x => x.Name("group_id").Type(NumberType.Integer))
                .Number(x => x.Name("epg_channel_id").Type(NumberType.Integer))
                .Number(x => x.Name("linear_media_id").Type(NumberType.Long))
                .Number(x => x.Name("wp_type_id").Type(NumberType.Integer).NullValue(0))
                .Boolean(x => x.Name("is_active"))
                .Number(x => x.Name("user_types").Type(NumberType.Integer))
                .Date(x => x.Name("start_date").Format(ESUtils.ES_DATE_FORMAT))
                .Date(x => x.Name("end_date").Format(ESUtils.ES_DATE_FORMAT))
                .Text(x => x.Name("date_routing"))
                .Number(x => x.Name("media_type_id").Type(NumberType.Integer).NullValue(0))
                .Number(x => x.Name("language_id").Type(NumberType.Long))
                .Text(x => InitializeDefaultTextPropertyDescriptor("epg_identifier"))
                .Date(x => x.Name("cache_date").Format(ESUtils.ES_DATE_FORMAT))
                .Date(x => x.Name("create_date").Format(ESUtils.ES_DATE_FORMAT))
                .Text(x => InitializeDefaultTextPropertyDescriptor("crid"))
                .Text(x => InitializeDefaultTextPropertyDescriptor("external_id"))
                ;

            var defaultLanguage = GetDefaultLanguage();
            string defaultIndexAnalyzer = $"{defaultLanguage.Code}_index_analyzer";
            string defaultSearchAnalyzer = $"{defaultLanguage.Code}_search_analyzer";
            string defaultAutocompleteAnalyzer = $"{defaultLanguage.Code}_autocomplete_analyzer";
            string defaultAutocompleteSearchAnalyzer = $"{defaultLanguage.Code}_autocomplete_search_analyzer";

            AddLanguageSpecificMappingToPropertyDescriptor(languages, metas, tags, metasToPad, analyzers, propertiesDescriptor,
                defaultIndexAnalyzer, defaultSearchAnalyzer, defaultAutocompleteAnalyzer, defaultAutocompleteSearchAnalyzer);

            return propertiesDescriptor;
        }

        private static void InitializeNumericMetaField(PropertiesDescriptor<object> propertiesDescriptor, 
            string metaName, 
            bool shouldAddPaddedField)
        {
            var lowercaseSubField = new TextPropertyDescriptor<object>()
                .Name("lowercase")
                .Analyzer(PHRASE_STARTS_WITH_ANALYZER)
                .SearchAnalyzer(PHRASE_STARTS_WITH_SEARCH_ANALYZER);

            var phraseAutocompleteSubField = new TextPropertyDescriptor<object>()
                .Name("phrase_autocomplete")
                .Analyzer(PHRASE_STARTS_WITH_ANALYZER)
                .SearchAnalyzer(PHRASE_STARTS_WITH_SEARCH_ANALYZER);

            PropertiesDescriptor<object> fieldsPropertiesDesctiptor = new PropertiesDescriptor<object>()
                .Number(y => y.Name(metaName).Type(NumberType.Double))
                        .Text(y => lowercaseSubField)
                        .Text(y => phraseAutocompleteSubField)
                ;

            if (shouldAddPaddedField)
            {
                var padded = new TextPropertyDescriptor<object>()
                    .Name($"padded_{metaName}")
                    .Analyzer(LOWERCASE_ANALYZER)
                    .SearchAnalyzer(LOWERCASE_ANALYZER);
                fieldsPropertiesDesctiptor.Text(y => padded);
            }

            NumberPropertyDescriptor<object> numberPropertyDescriptor = new NumberPropertyDescriptor<object>()
                .Name(metaName)
                .Type(NumberType.Double)
                .Fields(fields => fieldsPropertiesDesctiptor)
                ;
            propertiesDescriptor.Number(x => numberPropertyDescriptor);
        }

        private TextPropertyDescriptor<object> InitializeTextField(
            string nameFieldName,
            PropertiesDescriptor<object> propertiesDescriptor,
            string indexAnalyzer, 
            string searchAnalyzer,
            string autocompleteAnalyzer, 
            string autocompleteSearchAnalyzer,
            string phoneticIndexAnalyzer,
            string phoneticSearchAnalyzer,
            bool shouldAddPhoneticField,
            bool shouldAddPaddedField)
        {
            var lowercaseSubField = new TextPropertyDescriptor<object>()
                .Name("lowercase")
                .Analyzer(PHRASE_STARTS_WITH_ANALYZER)
                .SearchAnalyzer(PHRASE_STARTS_WITH_SEARCH_ANALYZER);

            var phraseAutocompleteSubField = new TextPropertyDescriptor<object>()
                .Name("phrase_autocomplete")
                .Analyzer(PHRASE_STARTS_WITH_ANALYZER)
                .SearchAnalyzer(PHRASE_STARTS_WITH_SEARCH_ANALYZER);

            var autocompleteSubField = new TextPropertyDescriptor<object>()
                .Name("autocomplete")
                .Analyzer(autocompleteAnalyzer)
                .SearchAnalyzer(autocompleteSearchAnalyzer);

            var analyzedField = new TextPropertyDescriptor<object>()
                .Name("analyzed")
                .Analyzer(indexAnalyzer)
                .SearchAnalyzer(searchAnalyzer);

            PropertiesDescriptor<object> fieldsPropertiesDesctiptor = new PropertiesDescriptor<object>()
                .Text(y => y.Name(nameFieldName).SearchAnalyzer(LOWERCASE_ANALYZER).Analyzer(LOWERCASE_ANALYZER))
                        .Text(y => lowercaseSubField)
                        .Text(y => phraseAutocompleteSubField)
                        .Text(y => autocompleteSubField)
                        .Text(y => analyzedField)
                ;

            if (shouldAddPhoneticField)
            {
                var phoneticField = new TextPropertyDescriptor<object>()
                    .Name("phonetic")
                    .Analyzer(phoneticIndexAnalyzer)
                    .SearchAnalyzer(phoneticSearchAnalyzer);
                fieldsPropertiesDesctiptor.Text(y => phoneticField);
            }

            if (shouldAddPaddedField)
            {
                var padded = new TextPropertyDescriptor<object>()
                    .Name($"padded_{nameFieldName}")
                    .Analyzer(LOWERCASE_ANALYZER)
                    .SearchAnalyzer(LOWERCASE_ANALYZER);
                fieldsPropertiesDesctiptor.Text(y => padded);
            }
            TextPropertyDescriptor<object> textPropertyDescriptor = new TextPropertyDescriptor<object>()
                .Name(nameFieldName).SearchAnalyzer(LOWERCASE_ANALYZER).Analyzer(LOWERCASE_ANALYZER)
                    .Fields(fields => fieldsPropertiesDesctiptor)
                ;
            propertiesDescriptor.Text(x => textPropertyDescriptor);

            return textPropertyDescriptor;
        }

        private TextPropertyDescriptor<object> InitializeDefaultTextPropertyDescriptor(string fieldName)
        {
            var lowercaseSubField = new TextPropertyDescriptor<object>()
                .Name("lowercase")
                .Analyzer(LOWERCASE_ANALYZER)
                .SearchAnalyzer(LOWERCASE_ANALYZER);

            var phraseAutocompleteSubField = new TextPropertyDescriptor<object>()
                .Name("phrase_autocomplete")
                .Analyzer(PHRASE_STARTS_WITH_ANALYZER)
                .SearchAnalyzer(PHRASE_STARTS_WITH_SEARCH_ANALYZER);

            var autocompleteSubField = new TextPropertyDescriptor<object>()
                .Name("autocomplete")
                .Analyzer(AUTOCOMPLETE_ANALYZER)
                .SearchAnalyzer(AUTOCOMPLETE_SEARCH_ANALYZER);

            var analyzedField = new TextPropertyDescriptor<object>()
                .Name("analyzed")
                .Analyzer(DEFAULT_INDEX_ANALYZER)
                .SearchAnalyzer(DEFAULT_SEARCH_ANALYZER);

            return new TextPropertyDescriptor<object>().Name(fieldName).SearchAnalyzer(LOWERCASE_ANALYZER).Analyzer(LOWERCASE_ANALYZER)
                .Fields(fields => fields
                    .Text(y => y.Name(fieldName).SearchAnalyzer(LOWERCASE_ANALYZER).Analyzer(LOWERCASE_ANALYZER))
                    .Text(y => lowercaseSubField)
                    .Text(y => phraseAutocompleteSubField)
                    .Text(y => autocompleteSubField)
                    .Text(y => analyzedField)
                    );
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
                GetAnalyzersWithLowercase(languages, out analyzers, out filters);

                var defaultCharFilter = new List<string>() { "html_strip" };

                analyzers.Add(PHRASE_STARTS_WITH_ANALYZER, new Analyzer()
                {
                    tokenizer = "keyword",
                    filter = new List<string>()
                    {
                        "lowercase",
                        EDGENGRAM_FILTER,
                        "icu_folding",
                        "icu_normalizer",
                        "asciifolding"
                    },
                    char_filter = defaultCharFilter
                });
                analyzers.Add(PHRASE_STARTS_WITH_SEARCH_ANALYZER, new Analyzer()
                {
                    tokenizer = "keyword",
                    filter = new List<string>()
                    {
                        "lowercase",
                        "icu_folding",
                        "icu_normalizer",
                        "asciifolding"
                    },
                    char_filter = defaultCharFilter
                });
                analyzers.Add(AUTOCOMPLETE_ANALYZER, new Analyzer()
                {
                    tokenizer = "whitespace",
                    filter = new List<string>()
                    {
                        "lowercase",
                        EDGENGRAM_FILTER,
                        "icu_folding",
                        "icu_normalizer",
                        "asciifolding"
                    },
                    char_filter = defaultCharFilter
                });
                analyzers.Add(AUTOCOMPLETE_SEARCH_ANALYZER, new Analyzer()
                {
                    tokenizer = "whitespace",
                    filter = new List<string>()
                    {
                        "lowercase",
                        "icu_folding",
                        "icu_normalizer",
                        "asciifolding"
                    },
                    char_filter = defaultCharFilter
                });
                filters.Add(EDGENGRAM_FILTER, new ElasticSearch.Searcher.Settings.NgramFilter()
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

        private void GetAnalyzersWithLowercase(IEnumerable<LanguageObj> languages, 
            out Dictionary<string, Analyzer> analyzers, 
            out Dictionary<string, ElasticSearch.Searcher.Settings.Filter> filters)
        {
            GetAnalyzersAndFiltersFromConfiguration(languages, out analyzers, out filters);

            // we always want a lowercase analyzer
            // we always want "autocomplete" ability
            analyzers.Add(LOWERCASE_ANALYZER,
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

        private void GetAnalyzersAndFiltersFromConfiguration(IEnumerable<LanguageObj> languages, 
            out Dictionary<string, Analyzer> analyzers, 
            out Dictionary<string, ElasticSearch.Searcher.Settings.Filter> filters)
        {
            analyzers = new Dictionary<string, Analyzer>();
            filters = new Dictionary<string, ElasticSearch.Searcher.Settings.Filter>();
            SetDefaultAnalyzersAndFilters(analyzers, filters);

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
                        analyzers[analyzer.Key] = analyzer.Value;
                    }
                }

                if (currentFilters != null)
                {
                    foreach (var filter in currentFilters)
                    {
                        filters[filter.Key] = filter.Value;
                    }
                }
            }
        }

        private void SetDefaultAnalyzersAndFilters(Dictionary<string, Analyzer> analyzers, Dictionary<string, ElasticSearch.Searcher.Settings.Filter> filters)
        {
            var defaultLanguage = GetDefaultLanguage();
            var defaultCharFilter = new List<string>() { "html_strip" };
            var defaultTokenChars = new List<string>() { "letter", "digit", "punctuation", "symbol" };

            var defaultIndexAnalyzer = new Analyzer()
            {
                char_filter = defaultCharFilter,
                filter = new List<string>()
                {
                    "asciifolding",
                    "lowercase",
                    $"{defaultLanguage.Code}_ngram_filter"
                },
                tokenizer = "keyword"
            };
            var defaultSearchAnalyzer = new Analyzer()
            {
                char_filter = defaultCharFilter,
                filter = new List<string>()
                {
                    "asciifolding",
                    "lowercase"
                },
                tokenizer = "keyword"
            };
            analyzers.Add($"{defaultLanguage.Code}_index_analyzer", defaultIndexAnalyzer);
            analyzers.Add(DEFAULT_INDEX_ANALYZER, defaultIndexAnalyzer);
            analyzers.Add($"{defaultLanguage.Code}_search_analyzer", defaultSearchAnalyzer);
            analyzers.Add(DEFAULT_SEARCH_ANALYZER, defaultSearchAnalyzer);
            analyzers.Add($"{defaultLanguage.Code}_autocomplete_analyzer", new Analyzer()
            {
                char_filter = defaultCharFilter,
                filter = new List<string>()
                {
                    "asciifolding",
                    "lowercase",
                },
                tokenizer = "whitespace"
            });
            analyzers.Add($"{defaultLanguage.Code}_autocomplete_search_analyzer", new Analyzer()
            {
                char_filter = defaultCharFilter,
                filter = new List<string>()
                {
                    "asciifolding",
                    "lowercase",
                    $"{defaultLanguage.Code}_edgengram_filter"
                },
                tokenizer = "whitespace"
            });
            filters.Add($"{defaultLanguage.Code}_ngram_filter", new NgramFilter()
            {
                type = "nGram",
                min_gram = 2,
                max_gram = 20,
                token_chars = defaultTokenChars
            });
            filters.Add($"{defaultLanguage.Code}_edgengram_filter", new NgramFilter()
            {
                type = "edgeNGram",
                min_gram = 2,
                max_gram = 20,
                token_chars = defaultTokenChars
            });
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
                            
                            var topicMetaType = CatalogManager.GetTopicMetaType(topics.Value);
                            IndexingUtils.GetMetaType(topicMetaType, out var metaType, out nullValue);

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

        private void AddAlias(string dailyEpgIndexName, RetryPolicy retryPolicy)
        {
            // create alias is idempotent request
            var epgIndexAlias = IndexingUtils.GetEpgIndexAlias(_partnerId);
            log.Info($"creating alias. index [{dailyEpgIndexName}], alias [{epgIndexAlias}]");
            retryPolicy.Execute(() =>
            {
                var putAliasResponse = _elasticClient.Indices.PutAlias(dailyEpgIndexName, epgIndexAlias);
                bool isAliasAdded = putAliasResponse != null && putAliasResponse.IsValid;
                if (!isAliasAdded) throw new Exception($"index set alias failed [{dailyEpgIndexName}], alias [{epgIndexAlias}]");
            });
        }


        private PropertiesDescriptor<object> GetMediaPropertiesDesctiptor(List<LanguageObj> languages,
            Dictionary<string, KeyValuePair<eESFieldType, string>> metas,
            List<string> tags,
            HashSet<string> metasToPad,
            Dictionary<string, Analyzer> analyzers)
        {
            PropertiesDescriptor<object> propertiesDescriptor = new PropertiesDescriptor<object>();

            var defaultLanguage = GetDefaultLanguage();
            string defaultIndexAnalyzer = $"{defaultLanguage.Code}_index_analyzer";
            string defaultSearchAnalyzer = $"{defaultLanguage.Code}_search_analyzer";
            string defaultAutocompleteAnalyzer = $"{defaultLanguage.Code}_autocomplete_analyzer";
            string defaultAutocompleteSearchAnalyzer = $"{defaultLanguage.Code}_autocomplete_search_analyzer";

            propertiesDescriptor
                .Number(x => x.Name("media_id").Type(NumberType.Long).NullValue(0))
                .Number(x => x.Name("group_id").Type(NumberType.Integer).NullValue(0))
                .Number(x => x.Name("media_type_id").Type(NumberType.Integer).NullValue(0))
                .Number(x => x.Name("epg_channel_id").Type(NumberType.Integer).NullValue(0))
                .Number(x => x.Name("wp_type_id").Type(NumberType.Integer).NullValue(0))
                .Number(x => x.Name("device_rule_id").Type(NumberType.Integer).NullValue(0))
                .Boolean(x => x.Name("is_active"))
                .Number(x => x.Name("like_counter").Type(NumberType.Integer).NullValue(0))
                .Number(x => x.Name("user_types").Type(NumberType.Integer))
                .Date(x => x.Name("start_date").Format(ESUtils.ES_DATE_FORMAT))
                .Date(x => x.Name("catalog_start_date").Format(ESUtils.ES_DATE_FORMAT))
                .Date(x => x.Name("end_date").Format(ESUtils.ES_DATE_FORMAT))
                .Date(x => x.Name("final_date").Format(ESUtils.ES_DATE_FORMAT))
                .Number(x => x.Name("language_id").Type(NumberType.Long))
                .Number(x => x.Name("allowed_countries").Type(NumberType.Integer))
                .Number(x => x.Name("blocked_countries").Type(NumberType.Integer))
                .Number(x => x.Name("inheritence_policy").Type(NumberType.Integer))
                .Date(x => x.Name("cache_date").Format(ESUtils.ES_DATE_FORMAT))
                .Date(x => x.Name("create_date").Format(ESUtils.ES_DATE_FORMAT))
                .Date(x => x.Name("update_date").Format(ESUtils.ES_DATE_FORMAT))
                .Percolator(x => x.Name("query"))
                ;

            InitializeTextField("external_id", propertiesDescriptor, defaultIndexAnalyzer, defaultSearchAnalyzer, defaultAutocompleteAnalyzer, defaultAutocompleteSearchAnalyzer,
                string.Empty, string.Empty, false, false);
            InitializeTextField("entry_id", propertiesDescriptor, defaultIndexAnalyzer, defaultSearchAnalyzer, defaultAutocompleteAnalyzer, defaultAutocompleteSearchAnalyzer,
                string.Empty, string.Empty, false, false);

            if (!metas.ContainsKey(META_SUPPRESSED))
            {
                metas.Add(META_SUPPRESSED, new KeyValuePair<eESFieldType, string>(eESFieldType.STRING, string.Empty));//new meta for suppressed value
            }

            AddLanguageSpecificMappingToPropertyDescriptor(languages, metas, tags, metasToPad, analyzers, propertiesDescriptor, 
                defaultIndexAnalyzer, defaultSearchAnalyzer, defaultAutocompleteAnalyzer, defaultAutocompleteSearchAnalyzer);

            return propertiesDescriptor;
        }

        private void AddLanguageSpecificMappingToPropertyDescriptor(List<LanguageObj> languages,
            Dictionary<string, KeyValuePair<eESFieldType, string>> metas, List<string> tags, HashSet<string> metasToPad, 
            Dictionary<string, Analyzer> analyzers, 
            PropertiesDescriptor<object> propertiesDescriptor, 
            string defaultIndexAnalyzer, string defaultSearchAnalyzer, string defaultAutocompleteAnalyzer, string defaultAutocompleteSearchAnalyzer)
        {
            PropertiesDescriptor<object> namePropertiesDescriptor = new PropertiesDescriptor<object>();
            PropertiesDescriptor<object> descriptionPropertiesDescriptor = new PropertiesDescriptor<object>();
            PropertiesDescriptor<object> tagsPropertiesDescriptor = new PropertiesDescriptor<object>();
            PropertiesDescriptor<object> metasPropertiesDescriptor = new PropertiesDescriptor<object>();

            foreach (var language in languages)
            {
                string indexAnalyzer = $"{language.Code}_index_analyzer";
                string searchAnalyzer = $"{language.Code}_search_analyzer";
                string autocompleteAnalyzer = $"{language.Code}_autocomplete_analyzer";
                string autocompleteSearchAnalyzer = $"{language.Code}_autocomplete_search_analyzer";
                string phoneticIndexAnalyzer = $"{language.Code}_index_dbl_metaphone";
                string phoneticSearchAnalyzer = $"{language.Code}_search_dbl_metaphone";

                if (!analyzers.ContainsKey(indexAnalyzer))
                {
                    indexAnalyzer = defaultIndexAnalyzer;
                }

                if (!analyzers.ContainsKey(searchAnalyzer))
                {
                    searchAnalyzer = defaultSearchAnalyzer;
                }

                if (!analyzers.ContainsKey(autocompleteAnalyzer))
                {
                    autocompleteAnalyzer = defaultAutocompleteAnalyzer;
                }

                if (!analyzers.ContainsKey(autocompleteSearchAnalyzer))
                {
                    autocompleteSearchAnalyzer = defaultAutocompleteSearchAnalyzer;
                }

                bool shouldAddPhoneticField = analyzers.ContainsKey(phoneticIndexAnalyzer) && analyzers.ContainsKey(phoneticSearchAnalyzer);

                InitializeTextField(
                    $"{language.Code}",
                    namePropertiesDescriptor,
                    indexAnalyzer,
                    searchAnalyzer,
                    autocompleteAnalyzer,
                    autocompleteSearchAnalyzer,
                    phoneticIndexAnalyzer,
                    phoneticSearchAnalyzer,
                    shouldAddPhoneticField,
                    false
                    );
                InitializeTextField(
                    $"{language.Code}",
                    descriptionPropertiesDescriptor,
                    indexAnalyzer,
                    searchAnalyzer,
                    autocompleteAnalyzer,
                    autocompleteSearchAnalyzer,
                    phoneticIndexAnalyzer,
                    phoneticSearchAnalyzer,
                    shouldAddPhoneticField,
                    false
                    );

                foreach (var tag in tags)
                {
                    InitializeTextField($"{tag}_{language.Code}",
                        tagsPropertiesDescriptor,
                        indexAnalyzer,
                        searchAnalyzer,
                        autocompleteAnalyzer,
                        autocompleteSearchAnalyzer,
                        phoneticIndexAnalyzer,
                        phoneticSearchAnalyzer,
                        shouldAddPhoneticField,
                        false
                        );
                }

                foreach (var meta in metas)
                {
                    string metaName = meta.Key.ToLower();
                    bool shouldAddPadded = metasToPad.Contains(metaName);

                    var metaType = meta.Value.Key;

                    if (metaType != eESFieldType.DATE)
                    {
                        if (metaType == eESFieldType.STRING)
                        {
                            var descriptor = InitializeTextField($"{metaName}_{language.Code}",
                                metasPropertiesDescriptor,
                                indexAnalyzer,
                                searchAnalyzer,
                                autocompleteAnalyzer,
                                autocompleteSearchAnalyzer,
                                phoneticIndexAnalyzer,
                                phoneticSearchAnalyzer,
                                shouldAddPhoneticField,
                                shouldAddPadded);
                        }
                        else
                        {
                            InitializeNumericMetaField(metasPropertiesDescriptor, metaName, shouldAddPadded);
                        }
                    }
                    else
                    {
                        metasPropertiesDescriptor.Date(x => x.Name(metaName).Format(ESUtils.ES_DATE_FORMAT));
                    }
                }

            }

            propertiesDescriptor.Object<object>(x => x
                .Name($"name")
                .Properties(properties => namePropertiesDescriptor))
            ;
            propertiesDescriptor.Object<object>(x => x
                .Name($"description")
                .Properties(properties => descriptionPropertiesDescriptor))
            ;
            propertiesDescriptor.Object<object>(x => x
                .Name($"metas")
                .Properties(properties => metasPropertiesDescriptor))
            ;
            propertiesDescriptor.Object<object>(x => x
                .Name($"tag")
                .Properties(properties => tagsPropertiesDescriptor))
            ;
        }

        #endregion
    }
}