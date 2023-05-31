using System;
using System.Collections.Generic;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using ApiObjects.SearchObjects;
using Catalog.Response;
using Core.Catalog.Response;
using GroupsCacheManager;
using Nest;
using Polly.Retry;
using ESUtils = ElasticSearch.Common.Utils;
using Phx.Lib.Appconfig;
using Status = ApiObjects.Response.Status;
using System.Linq;
using Phx.Lib.Log;
using System.Reflection;
using System.Threading.Tasks;
using Core.Catalog.CatalogManagement;
using ElasticSearch.Common;
using CachingProvider.LayeredCache;
using Core.Catalog.Cache;
using ApiLogic.Catalog;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.NestData;
using ElasticSearch.Searcher.Settings;
using ApiObjects.CanaryDeployment.Elasticsearch;
using Elasticsearch.Net;
using ElasticSearch.Utilities;
using Newtonsoft.Json;
using Polly;

using Policy = Polly.Policy;
using System.Net;
using System.Net.Sockets;
using ApiLogic.Api.Managers;
using Core.GroupManagers;
using NestMedia = ApiLogic.IndexManager.NestData.NestMedia;
using ApiLogic.IndexManager.QueryBuilders;
using ApiObjects.Response;
using ApiLogic.IndexManager.QueryBuilders.NestQueryBuilders;
using ApiLogic.IndexManager.QueryBuilders.NestQueryBuilders.Queries;
using TVinciShared;
using Channel = GroupsCacheManager.Channel;
using OrderDir = ApiObjects.SearchObjects.OrderDir;
using TvinciCache;
using MoreLinq;
using System.Globalization;
using Core.Api;
using ApiLogic.IndexManager;
using ApiLogic.IndexManager.Models;
using ApiLogic.IndexManager.Sorting;
using ElasticSearch.NEST;
using ElasticSearch.Searcher;
using ElasticSearch.Utils;
using Force.DeepCloner;
using BoolQuery = Nest.BoolQuery;
using System.Runtime.Caching;
using ApiLogic.IndexManager.Transaction;
using ApiObjects.Base;

namespace Core.Catalog
{
    public partial class IndexManagerV7 : IIndexManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Consts

        private const int REFRESH_INTERVAL_FOR_EMPTY_INDEX_SECONDS = 10;
        private const string INDEX_REFRESH_INTERVAL = "10s";
        private const int MAX_RESULTS_DEFAULT = 100000;

        protected const string DEFAULT_LOWERCASE_ANALYZER = "default_lowercase_analyzer";
        protected const string DEFAULT_INDEX_ANALYZER = "default_index_analyzer";
        protected const string DEFAULT_SEARCH_ANALYZER = "default_search_analyzer";
        protected const string DEFAULT_AUTOCOMPLETE_ANALYZER = "default_autocomplete_analyzer";
        protected const string DEFAULT_AUTOCOMPLETE_SEARCH_ANALYZER = "default_autocomplete_search_analyzer";
        protected const string DEFAULT_PHRASE_STARTS_WITH_ANALYZER = "default_phrase_starts_with_analyzer";
        protected const string DEFAULT_PHRASE_STARTS_WITH_SEARCH_ANALYZER = "default_phrase_starts_with_search_analyzer";
        protected const string DEFAULT_EDGENGRAM_FILTER = "default_edgengram_filter";
        protected const string EDGENGRAM_FILTER = "edgengram_filter";
        protected const string NGRAM_FILTER = "ngram_filter";

        public const string META_SUPPRESSED = "suppressed";

        #endregion

        #region Config Values

        private readonly int _numOfShards;
        private readonly int _numOfReplicas;
        private readonly int _maxResults;

        #endregion

        private Dictionary<string, Analyzer> _defaultAnalyzers;
        private Dictionary<string, ElasticSearch.Searcher.Settings.Filter> _defaultFilters;
        private Dictionary<string, Tokenizer> _defaultTokenizers;

        private readonly IApplicationConfiguration _applicationConfiguration;
        private readonly IElasticClient _elasticClient;
        private readonly ICatalogManager _catalogManager;
        private readonly IElasticSearchIndexDefinitionsNest _esIndexDefinitions;
        private readonly ILayeredCache _layeredCache;
        private readonly IChannelManager _channelManager;
        private readonly ICatalogCache _catalogCache;
        private readonly IWatchRuleManager _watchRuleManager;
        private readonly IChannelQueryBuilder _channelQueryBuilder;
        private readonly IGroupsFeatures _groupsFeatures;
        private readonly INamingHelper _namingHelper;
        private readonly IGroupSettingsManager _groupSettingsManager;
        private readonly ISortingService _sortingService;
        private readonly IStartDateAssociationTagsSortStrategy _startDateAssociationTagsSortStrategy;
        private readonly IStatisticsSortStrategy _statisticsSortStrategy;
        private readonly ISortingAdapter _sortingAdapter;
        private readonly IEsSortingService _esSortingService;
        private readonly IUnifiedQueryBuilderInitializer _queryInitializer;
        private readonly IRegionManager _regionManager;

        private readonly int _partnerId;
        private readonly IGroupManager _groupManager;
        private readonly int _sizeOfBulk;
        private readonly int _sizeOfBulkDefaultValue; 
        private readonly ITtlService _ttlService;

        /// <summary>
        /// Initialiezs an instance of Index Manager for work with ElasticSearch 7.14
        /// Please do not use this ctor, rather use IndexManagerFactory.
        /// </summary>
        public IndexManagerV7(int partnerId,
            IElasticClient elasticClient,
            IApplicationConfiguration applicationConfiguration,
            IGroupManager groupManager,
            ICatalogManager catalogManager,
            IElasticSearchIndexDefinitionsNest esIndexDefinitions,
            IChannelManager channelManager,
            ICatalogCache catalogCache,
            ITtlService ttlService,
            IWatchRuleManager watchRuleManager,
            IChannelQueryBuilder channelQueryBuilder,
            IGroupsFeatures groupsFeatures,
            ILayeredCache layeredCache,
            INamingHelper namingHelper,
            IGroupSettingsManager groupSettingsManager,
            ISortingService sortingService,
            IStartDateAssociationTagsSortStrategy startDateAssociationTagsSortStrategy,
            IStatisticsSortStrategy statisticsSortStrategy,
            ISortingAdapter sortingAdapter,
            IEsSortingService esSortingService,
            IUnifiedQueryBuilderInitializer queryInitializer,
            IRegionManager regionManager)
        {
            _elasticClient = elasticClient;
            _partnerId = partnerId;
            _applicationConfiguration = applicationConfiguration;
            _catalogManager = catalogManager;
            _esIndexDefinitions = esIndexDefinitions;
            _channelManager = channelManager;
            _catalogCache = catalogCache;
            _groupManager = groupManager;
            _ttlService = ttlService;
            _watchRuleManager = watchRuleManager;
            _channelQueryBuilder = channelQueryBuilder;
            _groupsFeatures = groupsFeatures;
            _layeredCache = layeredCache;
            _namingHelper = namingHelper;
            _groupSettingsManager = groupSettingsManager;
            _sortingService = sortingService;
            _startDateAssociationTagsSortStrategy = startDateAssociationTagsSortStrategy;
            _statisticsSortStrategy = statisticsSortStrategy;
            _sortingAdapter = sortingAdapter;
            _esSortingService = esSortingService;
            _queryInitializer = queryInitializer;
            _regionManager = regionManager;

            //init all ES const
            _numOfShards = _applicationConfiguration.ElasticSearchHandlerConfiguration.NumberOfShards.Value;
            _numOfReplicas = _applicationConfiguration.ElasticSearchHandlerConfiguration.NumberOfReplicas.Value;
            _maxResults = _applicationConfiguration.ElasticSearchConfiguration.MaxResults.Value;
            _sizeOfBulk = _applicationConfiguration.ElasticSearchHandlerConfiguration.BulkSize.Value;
            _sizeOfBulkDefaultValue = _applicationConfiguration.ElasticSearchHandlerConfiguration.BulkSize.GetDefaultValue();

            InitDefaultAnalyzersAndFilters();
    
        }
        
        #region OPC helpers

        private Group GetGroupManager()
        {
            return _groupManager.GetGroup(_partnerId);
        }

        private CatalogGroupCache GetCatalogGroupCache()
        {
            CatalogGroupCache catalogGroupCache;
            _catalogManager.TryGetCatalogGroupCacheFromCache(_partnerId, out catalogGroupCache);
            return catalogGroupCache;
        }

        private bool IsOpc()
        {
            return _catalogManager.DoesGroupUsesTemplates(_partnerId);
        }

        private IReadOnlyDictionary<long, List<int>> GetLinearChannelsMapping()
        {
            return _regionManager.GetLinearMediaRegions(_partnerId);
        }

        public HashSet<string> GetMetasToPad()
        {
            HashSet<string> metasToPad;
            GetMetasAndTagsForMapping(out _, out _, out metasToPad);
            return metasToPad;
        }
        
        #endregion

        public bool UpsertMedia(long assetId)
        {
            bool result = false;

            if (assetId <= 0)
            {
                log.WarnFormat("Received media request of invalid media id {0} when calling UpsertMedia", assetId);
                return result;
            }

            string index = NamingHelper.GetMediaIndexAlias(_partnerId);
            if (!_elasticClient.Indices.Exists(index).Exists)
            {
                log.Error($"Index of type media for group {_partnerId} does not exist");
                return false;
            }

            Dictionary<int, LanguageObj> languagesMap = null;

            var metasToPad = new HashSet<string>();
            if (IsOpc())
            {
                var catalogGroupCache = GetCatalogGroupCache();
                languagesMap = new Dictionary<int, LanguageObj>(catalogGroupCache.LanguageMapById);

                var metas = catalogGroupCache.TopicsMapById.Values.Where(x => x.Type == ApiObjects.MetaType.Number).Select(y => y.SystemName).ToList();
                if (metas?.Count > 0)
                {
                    metasToPad = new HashSet<string>(metas);
                }
            }
            else
            {
                metasToPad = GetMetasToPad();
                var groupManager = GetGroupManager();
                var languages = groupManager.GetLangauges();
                languagesMap = languages.ToDictionary(x => x.ID, x => x);
            }

            try
            {
                var mediaDictionary = _catalogManager.GetGroupMedia(_partnerId, assetId);

                if (mediaDictionary != null && mediaDictionary.Count > 0)
                {
                    var numOfBulkRequests = 0;
                    var bulkRequests = new Dictionary<int, List<NestEsBulkRequest<NestMedia>>>()
                    {
                        { numOfBulkRequests, new List<NestEsBulkRequest<NestMedia>>() }
                    };

                    GetMediaNestEsBulkRequest(index, GetBulkSize(), 0, bulkRequests, mediaDictionary);

                    result = true;

                    foreach (var item in bulkRequests.Values)
                    {
                        result &= ExecuteAndValidateBulkRequests(item);
                    }
                }
                else
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                result = false;
                log.Error($"Error on upsert media for asset {assetId}", ex);
            }

            log.Debug($"Upsert Media result {result}");

            return result;
        }

        public string SetupEpgV2Index(string indexNmae)
        {
            EnsureEpgIndexExist(indexNmae, EpgFeatureVersion.V2);
            SetNoRefresh(indexNmae);

            return indexNmae;
        }

        public void RollbackEpgV3ToV2WithoutReindexing(bool rollbackFromBackup, int batchSize)
        {
            throw new NotImplementedException();
        }

        public void RollbackEpgV3ToV1WithoutReindexing(bool rollbackFromBackup, int batchSize)
        {
            throw new NotImplementedException();
        }

        public bool ForceRefreshEpgIndex(string indexName)
        {
            var response = _elasticClient.Indices.Refresh(new RefreshRequest(indexName));
            return response.IsValid;
        }

        public bool FinalizeEpgV2Indices(List<DateTime> dates)
        {
            var indices = dates.Select(x => _namingHelper.GetDailyEpgIndexName(_partnerId, x));
            var existingIndices = indices.Select(x => x).Where(x => _elasticClient.Indices.Exists(x).IsValid).ToList();

            foreach (var index in existingIndices)
            {
                SetIndexRefreshToDefault(index);
            }

            return true;
        }

        public bool CompactEpgV2Indices(int futureIndexCompactionStart, int pastIndexCompactionStart)
        {
            var retryCount = 3;
            var globalEpgAlias = NamingHelper.GetEpgIndexAlias(_partnerId);
            var futureIndexName = NamingHelper.GetEpgFutureIndexName(_partnerId);
            var pastIndexName = NamingHelper.GetEpgPastIndexName(_partnerId);

            try
            {
                var numOfShards = _applicationConfiguration.EPGIngestV2Configuration.NumOfShardsForCompactedIndex.Value;
                EnsureEpgIndexExist(futureIndexName, EpgFeatureVersion.V2, numOfShards);
                SetNoRefresh(futureIndexName);
                EnsureEpgIndexExist(pastIndexName, EpgFeatureVersion.V2, numOfShards);
                SetNoRefresh(pastIndexName);

                var epgV2Indices = _elasticClient.GetIndicesPointingToAlias(globalEpgAlias);
                
                // Create a dictionary of index date parsed to the actual index name
                var epgV2IndicesDict = epgV2Indices
                    .Where(i => !i.Equals(futureIndexName, StringComparison.OrdinalIgnoreCase) && !i.Equals(pastIndexName, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(i => DateTime.ParseExact(i.Split('_').Last(), ElasticSearch.Common.Utils.ES_DATEONLY_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal));

                var pastCompactionStartDate = DateTime.UtcNow.Date.AddDays(-pastIndexCompactionStart);
                var futureCompactionStartDate = DateTime.UtcNow.Date.AddDays(futureIndexCompactionStart);
                var pastDatesToCompact = epgV2IndicesDict.Where(d => d.Key < pastCompactionStartDate).ToDictionary(k=>k.Key, v=>v.Value);
                var futureDatesToCompact = epgV2IndicesDict.Where(d => d.Key > futureCompactionStartDate).ToDictionary(k=>k.Key, v=>v.Value);
                // map to hold target index to list of sources
                var reindexMap = new Dictionary<string, Dictionary<DateTime, string>>()
                {
                    { pastIndexName, pastDatesToCompact },
                    { futureIndexName, futureDatesToCompact }
                };

                foreach (var sourceTargetPair in reindexMap)
                {
                    var target = sourceTargetPair.Key;
                    var sources = sourceTargetPair.Value;
                    foreach (var s in sources)
                    {
                        var sourceIndexName = s.Value;
                        if (!Reindex(sourceIndexName, target, retryCount)) { throw new Exception($"error reindexing source:[{sourceIndexName}], target:[{target}]"); }
                        if (!DeleteIndex(sourceIndexName)) { throw new Exception($"error deleting source after reindex: [{sourceIndexName}], target:[{target}]"); }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"error while trying to compact indices for partner:[{_partnerId}]", ex);
                return false;
            }
            finally
            {
                SetIndexRefreshToDefault(futureIndexName);
                SetIndexRefreshToDefault(pastIndexName);
            }

            return true;
        }

        public bool DeleteProgram(List<long> assetIds, bool isRecording = false)
        {
            if (assetIds == null || assetIds.Count == 0)
            {
                return false;
            }

            string index = isRecording ? NamingHelper.GetRecordingIndexAlias(_partnerId) : NamingHelper.GetEpgIndexAlias(_partnerId);
            
            var deleteResponse = _elasticClient.DeleteByQuery<NestEpg>(request => request
                .Conflicts(Conflicts.Proceed)
                .Index(index)
                .Query(query => query
                    .Terms(terms => terms.Field(NestEpg => NestEpg.EpgID).Terms<long>(assetIds))
                ));

            if (deleteResponse.VersionConflicts > 0)
            {
                log.DebugFormat($"Got {deleteResponse.VersionConflicts} version conflicts when deleting epgs {string.Join(",", assetIds.Take(20))}");
            }

            return deleteResponse.IsValid;
        }

        internal static RetryPolicy GetRetryPolicy<TException>(int retryCount = 3) where TException : Exception
        {
            return Policy.Handle<TException>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time, attempt, ctx) =>
                {
                    log.Warn($"ElasticSearch request attemp [{attempt}/{retryCount}] Failed, waiting for:[{time.TotalSeconds}] seconds.", ex);
                });
        }

        public bool UpsertProgram(List<EpgCB> epgObjects, Dictionary<string, LinearChannelSettings> linearChannelSettings)
        {
            if (!epgObjects.Any())
            {
                return true;
            }

            var result = true;

            try
            {
                var sizeOfBulk = GetBulkSize();
                var languages = GetLanguages();
                var epgChannelIds = epgObjects.Select(item => item.ChannelID.ToString()).Distinct().ToList();
                linearChannelSettings = linearChannelSettings ?? new Dictionary<string, LinearChannelSettings>();

                var bulkRequests = new List<NestEsBulkRequest<NestEpg>>();

                var linearChannelsRegionsMapping = _regionManager.GetLinearMediaRegions(_partnerId);

                var createdAliases = new HashSet<string>();
                _catalogManager.GetLinearChannelValues(epgObjects, _partnerId, epg => { Utils.ExtractSuppressedValue(GetCatalogGroupCache(), epg); });

                var alias = NamingHelper.GetEpgIndexAlias(_partnerId);
                var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(_partnerId);

                // Create dictionary by languages
                foreach (var language in languages)
                {
                    // Filter programs to current language
                    var currentLanguageEpgs = epgObjects.Where(epg =>
                        epg.Language.ToLower() == language.Code.ToLower() ||
                        language.IsDefault && string.IsNullOrEmpty(epg.Language)).ToList();

                    if (!currentLanguageEpgs.Any())
                        continue;

                    // Create bulk request object for each program
                    foreach (var epgCb in currentLanguageEpgs)
                    {
                        // Epg V2 has multiple indices connected to the global alias {groupID}_epg
                        // in that case we need to use the specific date alias for each epg item to update
                        if (epgFeatureVersion == EpgFeatureVersion.V2)
                        {
                            alias = EnsureEpgIndexExistsForEpg(epgCb, epgFeatureVersion, createdAliases);
                        }

                        UpdateEpgLinearMediaId(linearChannelSettings, epgCb);
                        UpdateEpgRegions(epgCb, linearChannelsRegionsMapping);

                        var expiry = GetEpgExpiry(epgCb);
                        var epg = NestDataCreator.GetEpg(epgCb, language.ID, true, IsOpc(), expiry);
                        var nestEsBulkRequest = GetEpgBulkRequest(alias, epg);
                        bulkRequests.Add(nestEsBulkRequest);

                        //prepare next bulk
                        if (bulkRequests.Count <= sizeOfBulk)
                        {
                            continue;
                        }

                        var isValid = ExecuteAndValidateBulkRequests(bulkRequests);
                        result &= isValid;
                    }
                }

                if (bulkRequests.Any())
                {
                    var isValid = ExecuteAndValidateBulkRequests(bulkRequests);
                    result &= isValid;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error: Update EPGs threw an exception. Exception={0}", ex);
                throw;
            }

            return result;
        }

        private bool Reindex(string sourceIndex, string targetIndex, int retryCount = 1)
        {
            var policy = IndexManagerCommonHelpers.GetRetryPolicy<Exception>(retryCount);
            var retryResult = policy.ExecuteAndCapture(()=>{
            var res = _elasticClient.ReindexOnServer(r => r
                .Source(s => s.Index(sourceIndex))
                .Destination(d => d.Index(targetIndex))
                .WaitForCompletion());
            
            if (!res.IsValid)
            {
                throw new Exception($"error while trying to compact indices for partner:[{_partnerId}], debug info:[{res.DebugInformation}]");
            }
            });
            return retryResult.Outcome == OutcomeType.Successful;
        }
        
        private bool DeleteIndex(string index, int retryCount = 1)
        {
            var policy = IndexManagerCommonHelpers.GetRetryPolicy<Exception>(retryCount);
            var retryResult = policy.ExecuteAndCapture(()=>
            {
                var res = _elasticClient.Indices.Delete(index);
                if (!res.IsValid)
                {
                    throw new Exception($"error while trying to delete index:[{index}] for partner:[{_partnerId}], debug info:[{res.DebugInformation}]");
                }
            });
            return retryResult.Outcome == OutcomeType.Successful;
        }

        private static void UpdateEpgRegions(EpgCB epgCb, Dictionary<long, List<int>> linearChannelsRegionsMapping)
        {
            if (epgCb.LinearMediaId > 0 &&
                linearChannelsRegionsMapping != null &&
                linearChannelsRegionsMapping.ContainsKey(epgCb.LinearMediaId))
            {
                epgCb.regions = linearChannelsRegionsMapping[epgCb.LinearMediaId];
            }
        }

        private void UpdateEpgLinearMediaId(Dictionary<string, LinearChannelSettings> linearChannelSettings, EpgCB epgCb)
        {
            if (linearChannelSettings.ContainsKey(epgCb.ChannelID.ToString()))
            {
                epgCb.LinearMediaId = linearChannelSettings[epgCb.ChannelID.ToString()].LinearMediaId;
            }
        }

        private string EnsureEpgIndexExistsForEpg(EpgCB epgCb, EpgFeatureVersion epgFeatureVersion, HashSet<string> createdAliases)
        {
            var alias = _namingHelper.GetDailyEpgIndexName(_partnerId, epgCb.StartDate.Date);

            // in case alias already created, no need to check in ES
            if (!createdAliases.Contains(alias))
            {
                EnsureEpgIndexExist(alias, epgFeatureVersion);
                createdAliases.Add(alias);
            }

            return alias;
        }

        public bool DeleteChannelPercolator(List<int> channelIds)
        {
            bool result = false;
            bool allValid = true;

            try
            {
                string alias = NamingHelper.GetChannelPercolatorIndexAlias(_partnerId);
                var indices = _elasticClient.GetIndicesPointingToAlias(alias);

                foreach (var index in indices)
                {
                    foreach (var channelId in channelIds)
                    {
                        var deleteResponse = _elasticClient.Delete<NestPercolatedQuery>(
                            GetChannelDocumentId(channelId),
                            request => request.Index(index));

                        if (!deleteResponse.IsValid && deleteResponse.Result != Result.NotFound)
                        {
                            log.Error($"Failed deleting channel percoaltor, id = {channelId}, index = {index}");
                            allValid = false;
                        }
                    }
                }

                result = allValid;
            }
            catch (Exception ex)
            {
                log.Error($"Error deleting channel percolator for partner {_partnerId}", ex);
            }

            return result;
        }

        public bool UpdateChannelPercolator(List<int> channelIds, Channel channel = null)
        {
            bool result = true;

            string index = NamingHelper.GetChannelPercolatorIndexAlias(_partnerId);
            var indices = _elasticClient.GetIndicesPointingToAlias(index);

            List<Channel> channels = new List<Channel>();

            if (channel != null)
            {
                channels.Add(channel);
            }
            else
            {
                var groupManager = GetGroupManager();
                if (groupManager == null || groupManager.channelIDs == null || groupManager.channelIDs.Count == 0)
                {
                    return result;
                }

                foreach (int channelId in channelIds)
                {
                    Channel channelToUpdate = ChannelRepository.GetChannel(channelId, groupManager);

                    if (channelToUpdate != null)
                    {
                        channels.Add(channelToUpdate);
                    }
                }
            }

            foreach (var currentChannel in channels)
            {
                result &= UpdateChannelPercolator(currentChannel, indices.ToList());
            }

            return result;
        }

        private bool UpdateChannelPercolator(Channel channel, List<string> aliases)
        {
            bool result = true;

            if (channel == null)
            {
                return false;
            }

            var query = _channelQueryBuilder.GetChannelQuery(channel);
            if (query == null)
            {
                return false;
            }

            query.Query = WrapQueryIfEpgV3Feature(query.Query);

            foreach (string alias in aliases)
            {
                try
                {
                    var indexResponse = _elasticClient.Index(query,
                        request => request
                            .Index(alias)
                            .Id(GetChannelDocumentId(channel))
                            );

                    if (indexResponse == null || !indexResponse.IsValid)
                    {
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    result = false;
                    log.Error($"Error indexing channel percolator for channel {channel.m_nChannelID}", ex);
                }
            }

            return result;
        }
        public bool DeleteChannel(int channelId)
        {
            bool result = false;

            if (channelId <= 0)
            {
                log.Warn($"Received channel request of invalid channel id {channelId} when calling DeleteChannel");
                return result;
            }

            try
            {
                string index = ESUtils.GetGroupChannelIndex(_partnerId);
                var policy = Policy.HandleResult<bool>(x => !x)
                    .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(1));
                result = policy.Execute(() =>
                {
                    var deleteResponse = _elasticClient.DeleteByQuery<NestChannelMetadata>(request => request
                        .Index(index)
                        .Query(query => query.Term(channel => channel.ChannelId, channelId)
                        ));
                    
                    return deleteResponse.IsValid && deleteResponse.Deleted > 0;
                });

                if (!result)
                {
                    log.Error($"Failed deleting channel metadata, id = {channelId}, index = {index}");
                }
                
                bool deletePercolatorResult = this.DeleteChannelPercolator(new List<int>() { channelId });
                if (!deletePercolatorResult)
                {
                    log.Error($"Error deleting channel percolator for channel {channelId}");
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error deleting channel with id {channelId}", ex);
            }

            return result;
        }

        public bool UpsertChannel(int channelId, Channel channel = null, long userId = 0)
        {
            var result = false;
            if (channelId <= 0)
            {
                log.WarnFormat("Received channel request of invalid channel id {0} when calling UpsertChannel", channelId);
                return false;
            }

            try
            {
                if (channel == null)
                {
                    var contextData = new ContextData(_partnerId) { UserId = userId };
                    var response = _channelManager.GetChannelById(contextData, channelId, true);
                    if (response != null && response.Status != null && response.Status.Code != (int)eResponseStatus.OK)
                    {
                        return false;
                    }

                    if (response != null)
                    {
                        channel = response.Object;
                    }

                    if (channel == null)
                    {
                        log.ErrorFormat(
                            "failed to get channel object for _partnerId: {0}, channelId: {1} when calling UpsertChannel",
                            _partnerId, channelId);
                        return false;
                    }
                }

                var index = ESUtils.GetGroupChannelIndex(_partnerId);

                if (!_elasticClient.Indices.Exists(index).Exists)
                {
                    log.Error($"channel metadata index doesn't exist for group {_partnerId}");
                    return false;
                }

                var channelMetadata = NestDataCreator.GetChannelMetadata(channel);

                var indexResponse = _elasticClient.Index(channelMetadata, x => x.Index(index));

                if (indexResponse.IsValid && (indexResponse.Result == Result.Created || indexResponse.Result == Result.Updated))
                {
                    result = true;
                    if ((channel.m_nChannelTypeID != (int)ChannelType.Manual ||
                         (channel.m_lChannelTags != null && channel.m_lChannelTags.Count > 0))
                        && !UpdateChannelPercolator(new List<int>() { channelId }, channel))
                    {
                        log.ErrorFormat("Update channel percolator failed for Upsert Channel with channelId: {0}",
                            channelId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(
                    "Error - " +
                    string.Format("Upsert Channel threw an exception. channelId: {0}, Exception={1};Stack={2}",
                        channelId, ex.Message, ex.StackTrace), ex);
            }

            if (!result)
            {
                log.ErrorFormat("Upsert channel with id {0} failed", channelId);
            }

            return result;

        }

        public bool DeleteMedia(long assetId)
        {
            var isSuccess = false;

            if (assetId <= 0)
            {
                log.WarnFormat("Received media request of invalid media id {0} when calling DeleteMedia", assetId);
                return isSuccess;
            }

            string index = NamingHelper.GetMediaIndexAlias(_partnerId);
            var deleteResponse = _elasticClient.DeleteByQuery<NestMedia>(request => request
                .Index(index)
                .Query(query => query.Term(media => media.MediaId, assetId)
                    ));

            isSuccess = deleteResponse.IsValid;

            if (!isSuccess)
            {
                log.ErrorFormat("Delete media with id {0} failed", assetId);
            }
            return isSuccess;
        }

        public void DeleteMediaByTypeAndFinalEndDate(long mediaTypeId, DateTime finalEndDate)
        {
            var index = NamingHelper.GetMediaIndexAlias(_partnerId);
            var response = _elasticClient.DeleteByQuery<NestMedia>(dbq => dbq
                .Index(index)
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            m => m.Terms(t => t.Field(field => field.MediaTypeId).Terms(mediaTypeId)),
                            m => m.DateRange(dr => dr.Field(f1 => f1.FinalEndDate).LessThan(finalEndDate))
                        )
                    )
                ));

            if (!response.IsValid)
            {
                log.ErrorFormat("Failed to delete media assets from ES. index: {0}, media_type_id: {1}, final_end_date: {2}", index, mediaTypeId, finalEndDate);
                return;
            }
                
            log.DebugFormat("Media assets were deleted from ES. index: {0}, media_type_id: {1}, final_end_date: {2}, deleted_count: {3}", index, mediaTypeId, finalEndDate, response.Deleted);
        }

        public void UpsertPrograms(IList<EpgProgramBulkUploadObject> calculatedPrograms, string draftIndexName, LanguageObj defaultLanguage, IDictionary<string, LanguageObj> languages)
        {
            var bulkSize = GetBulkSize();

            var retryCount = 5;
            var policy = IndexManagerCommonHelpers.GetRetryPolicy<Exception>(retryCount);
            var linearChannelsRegionsMapping = GetLinearChannelsMapping();

            policy.Execute(() =>
            {
                var bulkRequests = new List<NestEsBulkRequest<NestEpg>>();
                try
                {
                    var programTranslationsToIndex = calculatedPrograms.SelectMany(p => p.EpgCbObjects);
                    foreach (var program in programTranslationsToIndex)
                    {
                        var metasToPad = GetMetasToPad();
                        program.PadMetas(metasToPad);
                        var langId = GetLanguageIdByCode(program.Language);

                        var expiry = GetEpgExpiry(program);
                        // We don't store regions in CB that's why we need to calculate regions before insertion to ES on every program update during ingest.
                        if (program.LinearMediaId > 0 && linearChannelsRegionsMapping.TryGetValue(program.LinearMediaId, out var regions))
                        {
                            program.regions = regions;
                        }

                        var buildEpg = NestDataCreator.GetEpg(program, langId, isOpc: IsOpc(), expiryUnixTimeStamp: expiry);
                        var bulkRequest = GetEpgBulkRequest(draftIndexName, buildEpg);
                        bulkRequests.Add(bulkRequest);

                        // If we exceeded maximum size of bulk 
                        if (bulkRequests.Count >= bulkSize)
                        {
                            ExecuteAndValidateBulkRequests(bulkRequests);
                        }
                    }

                    // If we have anything left that is less than the size of the bulk
                    if (bulkRequests.Any())
                    {
                        ExecuteAndValidateBulkRequests(bulkRequests);
                    }
                }
                finally
                {
                    if (bulkRequests.Any())
                    {
                        log.Debug($"Clearing bulk requests");
                        bulkRequests.Clear();
                    }
                }
            });
        }

        private long GetEpgExpiry(EpgCB program)
        {
            var totalMinutes = _ttlService.GetEpgTtlMinutes(program);
            return ODBCWrapper.Utils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow.AddMinutes(totalMinutes));
        }

        private int GetBulkSize(int? defaultValueWhenZero = null)
        {
            var bulkSize = _sizeOfBulk;
            if (_sizeOfBulk == 0 || _sizeOfBulk > _sizeOfBulkDefaultValue)
            {
                bulkSize = _sizeOfBulkDefaultValue;
            }

            if (bulkSize == 0 && defaultValueWhenZero.HasValue)
            {
                return defaultValueWhenZero.Value;
            }

            return bulkSize;
        }

        private NestEsBulkRequest<NestEpg> GetEpgBulkRequest(string draftIndexName,
            NestEpg buildEpg,
            eOperation operation = eOperation.index)
        {
            var bulkRequest = new NestEsBulkRequest<NestEpg>()
            {
                DocID = buildEpg.DocumentId,
                Document = buildEpg,
                Index = draftIndexName,
                Operation = operation,
                Routing = buildEpg.StartDate.Date.ToString(ESUtils.ES_DATEONLY_FORMAT)
            };

            return bulkRequest;
        }

        private bool ExecuteAndValidateBulkRequests<K>(List<NestEsBulkRequest<K>> bulkRequests)
            where K : class
        {
            try
            {
                var bulkResponse = ExecuteBulkRequest(bulkRequests);

                //no errors clear and end
                if (!bulkResponse.ItemsWithErrors.Any())
                {
                    return true;
                }

                foreach (var item in bulkResponse.ItemsWithErrors)
                {
                    log.Error($"Could not add item to ES index. GroupID={_partnerId} Id={item.Id} Index ={item.Index} error={item.Error}");
                }

                log.Error($"Failed to upsert [{bulkResponse.ItemsWithErrors.Count()}] documents");
            }
            finally
            {
                bulkRequests.Clear();
            }

            return false;
        }

        private BulkResponse ExecuteBulkRequest<K>(List<NestEsBulkRequest<K>> bulkRequests) where K : class
        {
            var docToBulkReq = bulkRequests.ToDictionary(x => x.Document, x => x);

            var bulkResponse = _elasticClient.Bulk(b =>
            {
                // split to types
                var indexRequests = bulkRequests.Where(req => req.Operation == eOperation.index);
                var updateRequests = bulkRequests.Where(req => req.Operation == eOperation.update);
                var deleteRequests = bulkRequests.Where(req => req.Operation == eOperation.delete);

                if (indexRequests.Any())
                {
                    b.IndexMany(indexRequests.Select(x => x.Document), (descriptor, data) =>
                    {
                        var bulkRequest = docToBulkReq[data];
                        return descriptor
                                .Index(bulkRequest.Index)
                                .Document(data)
                                .Routing(bulkRequest.Routing)
                                .Id(bulkRequest.DocID)
                            ;
                    });
                }

                if (updateRequests.Any())
                {
                    b.UpdateMany(updateRequests.Select(x => x.Document), (descriptor, data) =>
                    {
                        var bulkRequest = docToBulkReq[data];
                        return descriptor
                                .Index(bulkRequest.Index)
                                .Doc(data)
                                .Routing(bulkRequest.Routing)
                                .Id(bulkRequest.DocID)
                            ;
                    });
                }

                if (deleteRequests.Any())
                {
                    b.DeleteMany(deleteRequests.Select(x => x.Document), (descriptor, data) =>
                    {
                        var bulkRequest = docToBulkReq[data];
                        return descriptor
                                .Index(bulkRequest.Index)
                                .Routing(bulkRequest.Routing)
                                .Id(bulkRequest.DocID)
                            ;
                    });
                }

                return b;
            });
            return bulkResponse;
        }


        public void DeletePrograms(IList<EpgProgramBulkUploadObject> programsToDelete,
            string epgIndexName,
            IDictionary<string, LanguageObj> languages)
        {
            if (!programsToDelete.Any())
                return;

            var programIds = programsToDelete.Select(program => program.EpgId).ToList();
            var channelIds = programsToDelete.Select(x => x.ChannelId).Distinct().ToList();
            var externalIds = programsToDelete.Select(program => program.EpgExternalId).Distinct().ToList();

            log.Debug($"Update elasticsearch index completed, deleting required documents. documents length:[{programsToDelete.Count}]");
            var retryCount = 5;
            var policy = IndexManagerCommonHelpers.GetRetryPolicy<Exception>(retryCount);

            policy.Execute(() =>
            {
                //get the the query new version
                var response = _elasticClient.DeleteByQuery<NestEpg>(dbq => dbq
                    .Index(epgIndexName)
                    .Query(q => q
                    .Bool(b => b
                        .Should( //equivalent to OR
                            should => should.Terms(t => t.Field(field => field.EpgID).Terms<ulong>(programIds)),
                            should => should.Bool(bs => bs
                                .Must( //equivalent to AND
                                    mu => mu.Terms(t => t.Field(field => field.EpgIdentifier).Terms<string>(externalIds)),
                                    mu => mu.Terms(t => t.Field(field => field.ChannelID).Terms<int>(channelIds))
                                )
                            )
                        )
                    )
                ));

                if (!response.IsValid)
                    throw new Exception($"could not delete programs from index:{epgIndexName}");
            });
        }

        public IList<EpgProgramBulkUploadObject> GetCurrentProgramsByDate(int channelId, DateTime fromDate,
            DateTime toDate)
        {
            log.Debug($"GetCurrentProgramsByDate > fromDate:[{fromDate}], toDate:[{toDate}]");
            var result = new List<EpgProgramBulkUploadObject>();
            var index = NamingHelper.GetEpgIndexAlias(_partnerId);

            // if index does not exist - then we have a fresh start, we have 0 programs currently
            if (!_elasticClient.Indices.Exists(index).Exists)
            {
                log.Debug(
                    $"GetCurrentProgramsByDate > index alias:[{index}] does not exits, assuming no current programs");
                return result;
            }

            log.Debug(
                $"GetCurrentProgramsByDate > index alias:[{index}] found, searching current programs, minStartDate:[{fromDate}], maxEndDate:[{toDate}]");


            var queryDescriptor = new QueryContainerDescriptor<NestEpg>();
            QueryContainer query = queryDescriptor.Bool(b=>b.Filter(f => f
                .Bool(b1 =>
                    b1.Must(
                        m => m.Terms(t => t.Field(f1 => f1.ChannelID).Terms(channelId)),
                        m => m.DateRange(dr => dr.Field(f1 => f1.StartDate).LessThanOrEquals(toDate)),
                        m => m.DateRange(dr => dr.Field(f1 => f1.EndDate).GreaterThanOrEquals(fromDate))
                    )
                )
            ));

            query = WrapQueryIfEpgV3Feature(query);

            var searchResponse = _elasticClient.Search<NestEpg>(s => s
                .Index(index)
                .Size(_maxResults)
                .Query(_=> query)
            );


            // get the programs - epg ids from elasticsearch, information from EPG DAL
            if (searchResponse.IsValid)
            {
                return searchResponse.Hits?.Select(x => x.Source.ToEpgProgramBulkUploadObject()).DistinctBy(x=>x.EpgId).ToList();
            }
            return result;
        }

        public List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearchDefinitions, ref int totalItems)
        {
            return UnifiedSearch(unifiedSearchDefinitions, ref totalItems, out _);
        }

        public List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions definitions,
            ref int totalItems,
            out List<AggregationsResult> aggregationsResults)
        {
            aggregationsResults = null;
            List<UnifiedSearchResult> searchResultsList = new List<UnifiedSearchResult>();
            totalItems = 0;

            // fill language with default language if not specified
            if (definitions.langauge == null)
            {
                definitions.langauge = GetDefaultLanguage();
            }

            var builder = new UnifiedSearchNestBuilder(_esSortingService, _sortingAdapter, _queryInitializer)
            {
                SearchDefinitions = definitions
            };
            builder.SetPagingForUnifiedSearch();
            builder.SetGroupByValuesForUnifiedSearch();

            ISearchResponse<NestBaseAsset> searchResponse = Search(builder);

            // by default - take the total items from the main response. it might change if it's an aggregative search...
            totalItems = (int)searchResponse.Total;

            var unifiedSearchResultToHit = new Dictionary<UnifiedSearchResult, IHit<NestBaseAsset>>();
            
            foreach (var doc in searchResponse.Hits)
            {
                var result = CreateUnifiedSearchResultFromESDocument(definitions, doc);
                searchResultsList.Add(result);
                unifiedSearchResultToHit.Add(result, doc);
            }

            searchResultsList = ProcessUnifiedSearchResponse(searchResultsList, searchResponse, definitions, unifiedSearchResultToHit);

            var topHitsMapping = MapTopHits(searchResponse.Aggregations, definitions);

            if (searchResponse.Aggregations != null && searchResponse.Aggregations.Any())
            {
                var aggregationsResult = ConvertAggregationsResponse(searchResponse.Aggregations, definitions.groupBy,
                    topHitsMapping, searchResultsList, definitions, unifiedSearchResultToHit);

                if (aggregationsResult != null)
                {
                    aggregationsResults = new List<AggregationsResult>();
                    aggregationsResults.Add(aggregationsResult);
                }
            }

            return searchResultsList;
        }

        
        public AggregationsResult UnifiedSearchForGroupBy(UnifiedSearchDefinitions definitions)
        {
            var singleGroupByWithDistinct = definitions.groupBy?.Count == 1 && definitions.groupBy.Single().Key == definitions.distinctGroup.Key;
            
            if (!singleGroupByWithDistinct)
            {
                throw new NotSupportedException($"Method should be used for single group by");
            }

            var orderByFields = _sortingAdapter.ResolveOrdering(definitions);
            if (!_sortingService.IsSortingCompatibleWithGroupBy(orderByFields))
            {
                throw new NotSupportedException($"Not supported group by with provided ordering.");
            }
            
            var searchResultsList = new List<UnifiedSearchResult>();

            // fill language with default language if not specified
            if (definitions.langauge == null)
            {
                definitions.langauge = GetDefaultLanguage();
            }

            var builder = new UnifiedSearchNestBuilder(_esSortingService, _sortingAdapter, _queryInitializer)
            {
                SearchDefinitions = definitions
            };
            builder.SetPagingForUnifiedSearch();
            builder.SetGroupByValuesForUnifiedSearch();

            ISearchResponse<NestBaseAsset> searchResponse = Search(builder);

            // by default - take the total items from the main response. it might change if it's an aggregative search...
            var totalItems = (int)searchResponse.Total;

            var unifiedSearchResultToHit = new Dictionary<UnifiedSearchResult, IHit<NestBaseAsset>>();

            foreach (var doc in searchResponse.Hits)
            {
                var result = CreateUnifiedSearchResultFromESDocument(definitions, doc);
                searchResultsList.Add(result);
                unifiedSearchResultToHit.Add(result, doc);
            }

            searchResultsList = ProcessUnifiedSearchResponse(searchResultsList, searchResponse, definitions, unifiedSearchResultToHit);

            var topHitsMapping = MapTopHits(searchResponse.Aggregations, definitions);

            var aggregationsResult = new AggregationsResult { totalItems = totalItems };
            var anyAggregations = searchResponse.Aggregations != null && searchResponse.Aggregations.Any();
            if (!anyAggregations)
            {
                return aggregationsResult;
            }

            aggregationsResult = ConvertAggregationsResponse(searchResponse.Aggregations, definitions.groupBy,
                    topHitsMapping, searchResultsList, definitions, unifiedSearchResultToHit);
            

            return aggregationsResult;
        }
        
        private ISearchResponse<NestBaseAsset> Search(UnifiedSearchNestBuilder builder)
        {
            var query = builder.GetQuery();
            query = WrapQueryIfEpgV3Feature(query);
            var searchResponse = _elasticClient.Search<NestBaseAsset>(searchRequest =>
            {
                searchRequest
                    .Index(Indices.Index(builder.GetIndices()))
                    .Query(queryGetter => query)
                    .Sort(sortGetter => builder.GetSort())
                    .TrackTotalHits();

                // aggs = optional
                var aggs = builder.GetAggs();
                if (aggs != null && aggs.Any())
                {
                    searchRequest = searchRequest.Aggregations(aggs);
                }

                searchRequest = builder.SetSizeAndFrom(searchRequest);
                searchRequest = builder.SetFields(searchRequest);

                return searchRequest;
            });
            return searchResponse;
        }

        private AggregationsResult ConvertAggregationsResponse(
            AggregateDictionary aggregationsResult,
            IEnumerable<GroupByDefinition> groupBys,
            Dictionary<string, Dictionary<string, UnifiedSearchResult>> topHitsMapping,
            List<UnifiedSearchResult> orderedResults,
            UnifiedSearchDefinitions definitions,
            Dictionary<UnifiedSearchResult, IHit<NestBaseAsset>> unifiedSearchResultToHit)
        {
            if (groupBys == null || !groupBys.Any())
            {
                return null;
            }

            var currentGroupBy = groupBys.First();

            AggregationsResult result = new AggregationsResult()
            {
                field = currentGroupBy.Key
            };

            string cardinalityKey = $"{currentGroupBy.Key}_count";
            var cardinalityAggregation = aggregationsResult.ValueCount(cardinalityKey);
            if (cardinalityAggregation != null)
            {
                result.totalItems = cardinalityAggregation.Value.HasValue ? Convert.ToInt32(cardinalityAggregation.Value.Value) : 0;
            }

            if (aggregationsResult.ContainsKey(currentGroupBy.Key))
            {
                var missedKeysBucket = aggregationsResult.Terms(currentGroupBy.Key).Buckets
                    .FirstOrDefault(x => x.Key == ESUnifiedQueryBuilder.MissedHitBucketKey.ToString());
                result.totalItems += Convert.ToInt32(missedKeysBucket?.DocCount);
            }

            var missingValueBucket = aggregationsResult.Terms(currentGroupBy.Key)
                .Buckets.FirstOrDefault(x => x.Key == UnifiedSearchNestBuilder.TERMS_AGGREGATION_MISSING_VALUE.ToString());
            if (missingValueBucket != null)
            {
                result.totalItems += Convert.ToInt32(missingValueBucket.DocCount.GetValueOrDefault());
            }

            // if there is only one group by and it is a distinct request, we need to reorder the buckets
            // so we will create the aggregation result with the correct order
            var orderByFields = _sortingAdapter.ResolveOrdering(definitions);
            if (_esSortingService.IsBucketsReorderingRequired(orderByFields, definitions.distinctGroup))
            {
                ConvertDistinctGroupByAggregationResponse(aggregationsResult, topHitsMapping, orderedResults, definitions, unifiedSearchResultToHit, result);
                return result;
            }

            if (aggregationsResult.ContainsKey(currentGroupBy.Key))
            {
                var currentAggregation = aggregationsResult.Terms(currentGroupBy.Key);
                AggregationResult missingKeysBucket = null;

                foreach (var bucket in currentAggregation.Buckets)
                {
                    var bucketResult = new AggregationResult
                    {
                        value = bucket.Key,
                        count = Convert.ToInt32(bucket.DocCount),
                    };

                    if (groupBys.Count() > 1)
                    {
                        // group bys list is SUPPOSED to be maximum 3 items (really, who wants that much grouping?!)
                        // no worries about memory here, me thinks...
                        var nextGroupBys = groupBys.Skip(1);
                        var sub = ConvertAggregationsResponse(bucket, nextGroupBys, topHitsMapping, orderedResults, definitions, unifiedSearchResultToHit);

                        if (sub != null)
                        {
                            bucketResult.subs.Add(sub);
                        }
                    }

                    AddTopHitsToBucketResult(topHitsMapping, bucket, bucketResult, definitions);

                    // when groupingOption is "Include" then "missed keys" bucket should be the last in result
                    if (definitions.GroupByOption == GroupingOption.Include
                        && bucketResult.value == ESUnifiedQueryBuilder.MissedHitBucketKey.ToString())
                    {
                        missingKeysBucket = bucketResult;
                        continue;
                    }

                    result.results.Add(bucketResult);
                }

                if (missingKeysBucket != null)
                {
                    result.results.Add(missingKeysBucket);
                }
            }

            return result;
        }

        private void ConvertDistinctGroupByAggregationResponse(
            AggregateDictionary aggregationsResult,
            Dictionary<string, Dictionary<string, UnifiedSearchResult>> topHitsMapping,
            List<UnifiedSearchResult> orderedResults,
            UnifiedSearchDefinitions definitions,
            Dictionary<UnifiedSearchResult, IHit<NestBaseAsset>> unifiedSearchResultToHit,
            AggregationsResult result)
        {
            var distinctGroup = definitions.distinctGroup;
            var bucketMapping = new Dictionary<string, KeyedBucket<string>>();
            var orderedBuckets = new List<AggregationResult>();
            var alreadyContainedBuckets = new HashSet<KeyedBucket<string>>();

            var distinctGroupTerms = aggregationsResult.Terms(distinctGroup.Key);
            // first map all buckets by their grouping value
            foreach (var bucket in distinctGroupTerms.Buckets)
            {
                bucketMapping.Add(bucket.Key, bucket);
            }

            // go over all the ordered IDs and reorder the buckets by the specific documents' order
            foreach (var searchResult in orderedResults)
            {
                // extract the grouping value from the fields / extra fields
                var doc = unifiedSearchResultToHit[searchResult];
                var distinctGroupField = doc.Fields.Value<object>(new Field(distinctGroup.Value))?.ToString();

                if (distinctGroupField != null)
                {
                    // Pay attention! We use "to lower" because the bucket value is lowercased because it uses the analyzer. 
                    var groupingValue = Convert.ToString(distinctGroupField);

                    if (bucketMapping.ContainsKey(groupingValue))
                    {
                        var bucket = bucketMapping[groupingValue];

                        if (!alreadyContainedBuckets.Contains(bucket))
                        {
                            alreadyContainedBuckets.Add(bucket);
                            var bucketResult = new AggregationResult
                            {
                                value = bucket.Key,
                                count = Convert.ToInt32(bucket.DocCount),
                                topHits = new List<UnifiedSearchResult> { searchResult }
                            };

                            orderedBuckets.Add(bucketResult);
                        }
                    }
                }
                else if (definitions.GroupByOption == GroupingOption.Group
                    && bucketMapping.TryGetValue(
                        UnifiedSearchNestBuilder.TERMS_AGGREGATION_MISSING_VALUE.ToString(),
                        out var missedKeyBucket)
                    && !alreadyContainedBuckets.Contains(missedKeyBucket))
                {
                    alreadyContainedBuckets.Add(missedKeyBucket);
                    var bucket = new AggregationResult
                    {
                        value = missedKeyBucket.Key,
                        count = Convert.ToInt32(missedKeyBucket.DocCount),
                        topHits = new List<UnifiedSearchResult> { searchResult }
                    };

                    orderedBuckets.Add(bucket);
                }
            }

            // Add the leftovers - the buckets that weren't included previously for some reason 
            // (shouldn't happen, will happen if something went wrong or if we have more than MAX_RESULTS)
            foreach (var bucket in distinctGroupTerms.Buckets)
            {
                if (!alreadyContainedBuckets.Contains(bucket))
                {
                    alreadyContainedBuckets.Add(bucket);
                    var bucketResult = new AggregationResult()
                    {
                        value = bucket.Key,
                        count = Convert.ToInt32(bucket.DocCount),
                    };
                    AddTopHitsToBucketResult(topHitsMapping, bucket, bucketResult, definitions);
                    orderedBuckets.Add(bucketResult);
                }
            }

            // replace the original list with the ordered list
            result.results = orderedBuckets.ToList();
        }

        private void AddTopHitsToBucketResult(
            Dictionary<string, Dictionary<string, UnifiedSearchResult>> topHitsMapping,
            KeyedBucket<string> bucket,
            AggregationResult bucketResult,
            UnifiedSearchDefinitions definitions)
        {
            var topHits = bucket.TopHits(UnifiedSearchNestBuilder.TOP_HITS_DEFAULT_NAME);

            if (topHits != null)
            {
                var hits = topHits.Hits<NestBaseAsset>();

                foreach (var hit in hits)
                {
                    var unifiedSearchResult =
                        topHitsMapping != null && topHitsMapping.ContainsKey(hit.Index) &&
                        topHitsMapping[hit.Index].ContainsKey(hit.Id)
                            ? topHitsMapping[hit.Index][hit.Id]
                            : CreateUnifiedSearchResultFromESDocument(definitions, hit);

                    bucketResult.topHits.Add(unifiedSearchResult);
                }
            }
        }

        // index -> [ doc_id -> result ]
        private Dictionary<string, Dictionary<string, UnifiedSearchResult>> MapTopHits(AggregateDictionary aggregationResult, UnifiedSearchDefinitions definitions)
        {
            var result = new Dictionary<string, Dictionary<string, UnifiedSearchResult>>();

            if (aggregationResult == null || !aggregationResult.Any())
            {
                return result;
            }

            var stack = new Stack<IAggregate>();

            foreach (var aggregation in aggregationResult)
            {
                stack.Push(aggregation.Value);
            }

            // Breadth-first search by tree... or something similar
            while (stack.Count > 0)
            {
                var aggregation = stack.Pop();
                var topHitAggregation = aggregation as TopHitsAggregate;

                if (topHitAggregation != null)
                {
                    var hits = topHitAggregation.Hits<NestBaseAsset>();

                    foreach (var hit in hits)
                    {
                        var unifiedSearchResult = CreateUnifiedSearchResultFromESDocument(definitions, hit);

                        result.TryAdd(hit.Index, new Dictionary<string, UnifiedSearchResult>());
                        result[hit.Index][hit.Id] = unifiedSearchResult;
                    }
                }
                else
                {
                    var bucketAggregation = aggregation as BucketAggregate;
                    if (bucketAggregation != null && bucketAggregation.Items != null && bucketAggregation.Items.Any())
                    {
                        foreach (var bucket in bucketAggregation.Items)
                        {
                            var keyedBucket = bucket as KeyedBucket<object>;

                            foreach (var value in keyedBucket.Values)
                            {
                                stack.Push(value);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private List<UnifiedSearchResult> ProcessUnifiedSearchResponse(
            List<UnifiedSearchResult> searchResultsList,
            ISearchResponse<NestBaseAsset> searchResponse,
            UnifiedSearchDefinitions definitions,
            Dictionary<UnifiedSearchResult, IHit<NestBaseAsset>> unifiedSearchResultToHit)
        {
            if (!searchResponse.IsValid)
            {
                // TODO: better error handling?
                log.Error($"UnifiedSearch request to ES has failed");
                return searchResultsList;
            }

            // make sure that when ordered by stats and we have aggregations, then we must have hits returned to reorder the buckets later on
            if (searchResponse.Hits?.Count == 0)
            {
                return searchResultsList;
            }

            if (!_sortingService.IsSortingCompleted(definitions))
            {
                var sortingDefinitions = GenerateSortingDefinitions(definitions);
                var extendedUnifiedSearchResults = unifiedSearchResultToHit.Select(x => new ExtendedUnifiedSearchResult(x.Key, x.Value)).ToArray();
                IEnumerable<UnifiedSearchResult> orderedResults;
                if (definitions.PriorityGroupsMappings == null || !definitions.PriorityGroupsMappings.Any())
                {
                    orderedResults = _sortingService.GetReorderedAssets(sortingDefinitions, extendedUnifiedSearchResults);
                }
                else
                {
                    var tempResults = new List<UnifiedSearchResult>();
                    var priorityGroupsResults = extendedUnifiedSearchResults.GroupBy(r => r.Result.Score);
                    foreach (var priorityGroupsResult in priorityGroupsResults)
                    {
                        var reorderedAssets = _sortingService.GetReorderedAssets(sortingDefinitions, priorityGroupsResult);
                        if (reorderedAssets == null)
                        {
                            log.Debug($"Chunk from priority group hasn't been processed. Asset Ids: [{string.Join(",", priorityGroupsResult.Select(x => x.AssetId))}]");
                            continue;
                        }

                        tempResults.AddRange(reorderedAssets);
                    }

                    orderedResults = tempResults.ToArray();
                }

                // should not apply paging if we need to reorder buckets according to assets order
                var esOrderByFields = _sortingAdapter.ResolveOrdering(definitions);
                searchResultsList =
                    _esSortingService.IsBucketsReorderingRequired(esOrderByFields, definitions.distinctGroup)
                        ? orderedResults.ToList()
                        : orderedResults.Page(definitions.pageSize, definitions.pageIndex, out _).ToList();
            }

            return searchResultsList;
        }

        private static UnifiedSearchDefinitions GenerateSortingDefinitions(UnifiedSearchDefinitions definitions)
        {
            var sortingDefinitions = definitions.DeepClone();
            sortingDefinitions.groupId = definitions.ExtractParentGroupId();
            return sortingDefinitions;
        }

        private UnifiedSearchResult CreateUnifiedSearchResultFromESDocument(
            UnifiedSearchDefinitions definitions, IHit<NestBaseAsset> doc)
        {
            UnifiedSearchResult result = null;
            string assetId = doc.Id;
            eAssetTypes assetType = eAssetTypes.UNKNOWN;
            double score = doc.Score.HasValue ? doc.Score.Value : 0;

            assetId = GetAssetIdAndType(doc, ref assetType);

            if (definitions.shouldReturnExtendedSearchResult)
            {
                result = new ExtendedSearchResult()
                {
                    AssetId = assetId,
                    m_dUpdateDate = doc.Source.UpdateDate,
                    AssetType = assetType,
                    EndDate = doc.Source.EndDate,
                    StartDate = doc.Source.StartDate,
                    Score = score
                };

                // TODO: check this
                if (definitions.extraReturnFields?.Count > 0)
                {
                    (result as ExtendedSearchResult).ExtraFields = new List<ApiObjects.KeyValuePair>();

                    foreach (var field in definitions.extraReturnFields)
                    {
                        string language = definitions.langauge != null ? definitions.langauge.Code : string.Empty;
                        var value = doc.Fields.Value<object>(UnifiedSearchNestBuilder.GetExtraFieldName(language, field));
                        if (value != null)
                        {
                            (result as ExtendedSearchResult).ExtraFields.Add(new ApiObjects.KeyValuePair()
                            {
                                key = field,
                                value = Convert.ToString(value)
                            });
                        }
                    }
                }
            }
            else
            {
                if (assetType == eAssetTypes.NPVR)
                {
                    // After we searched for recordings, we need to replace their ID (recording ID) with the personal ID (domain recording)
                    if (definitions != null && definitions.recordingIdToSearchableRecordingMapping != null && 
                        definitions.recordingIdToSearchableRecordingMapping.Count > 0)
                    {
                        result = new RecordingSearchResult
                        {
                            AssetType = eAssetTypes.NPVR,
                            Score = score,
                            RecordingId = assetId
                        };

                        if (definitions.recordingIdToSearchableRecordingMapping.ContainsKey(assetId))
                        {
                            // Replace ID
                            result.AssetId = definitions.recordingIdToSearchableRecordingMapping[assetId].DomainRecordingId.ToString();
                            (result as RecordingSearchResult).EpgId = definitions.recordingIdToSearchableRecordingMapping[assetId].EpgId.ToString();
                            (result as RecordingSearchResult).RecordingType = definitions.recordingIdToSearchableRecordingMapping[assetId].RecordingType;
                            (result as RecordingSearchResult).IsMulti = definitions.recordingIdToSearchableRecordingMapping[assetId].IsMulti;
                        }

                        // TODO: check this
                        if (string.IsNullOrEmpty((result as RecordingSearchResult).EpgId) || (result as RecordingSearchResult).EpgId == "0")
                        {
                            var docEpgId = doc.Fields["epg_id"];

                            if (docEpgId != null)
                            {
                                (result as RecordingSearchResult).EpgId = Convert.ToString(docEpgId.As<object>());
                            }
                        }
                    }
                    else
                    {
                        object epgId = doc.Fields.Value<object>("epg_id");
                        
                        if (epgId == null)
                        {
                            epgId = doc.Fields.Value<object>(NamingHelper.EPG_IDENTIFIER);
                        }

                        result = new RecordingSearchResult()
                        {
                            AssetId = assetId,
                            m_dUpdateDate = doc.Source.UpdateDate,
                            AssetType = assetType,
                            EpgId = Convert.ToString(epgId),
                            Score = score,
                            RecordingId = assetId
                        };
                    }
                }
                else if (assetType == eAssetTypes.EPG && definitions.EpgFeatureVersion != EpgFeatureVersion.V1)
                {
                    // TODO: make sure this is functioning
                    var epgCouchbaseKey = doc.Fields.Value<string>("cb_document_id");

                    result = new EpgSearchResult()
                    {
                        AssetId = assetId,
                        m_dUpdateDate = doc.Source.UpdateDate,
                        AssetType = assetType,
                        Score = score,
                        DocumentId = epgCouchbaseKey
                    };
                }
                else
                {
                    result = new UnifiedSearchResult()
                    {
                        AssetId = assetId,
                        m_dUpdateDate = doc.Source.UpdateDate,
                        AssetType = assetType,
                        Score = score
                    };
                }
            }

            return result;
        }

        private string GetAssetIdAndType(IHit<NestBaseAsset> doc, ref eAssetTypes assetType)
        {
            string assetId;
            long? assetIdNumeric = doc.Fields.Value<long?>("recording_id");

            if (assetIdNumeric.HasValue)
            {
                assetType = eAssetTypes.NPVR;
            }
            else
            {
                assetIdNumeric = doc.Fields.Value<long?>("epg_id");

                if (assetIdNumeric.HasValue)
                {
                    assetType = eAssetTypes.EPG;
                }
                else
                {
                    assetIdNumeric = doc.Fields.Value<long?>("media_id");

                    if (assetIdNumeric.HasValue)
                    {
                        assetType = eAssetTypes.MEDIA;
                    }
                }
            }

            assetId = $"{assetIdNumeric}";
            return assetId;
        }

        private ApiObjects.eAssetTypes GetHitAssetType(IHit<NestBaseAsset> doc)
        {
            eAssetTypes assetType = eAssetTypes.UNKNOWN;

            if (doc.Fields["recording_id"] != null)
            {
                assetType = eAssetTypes.NPVR;
            }
            else if (doc.Fields["epg_id"] != null)
            {
                assetType = eAssetTypes.EPG;
            }
            else if (doc.Fields["media_id"] != null)
            {
                assetType = eAssetTypes.MEDIA;
            }

            return assetType;
        }

        public List<UnifiedSearchResult> SearchSubscriptionAssets(List<BaseSearchObject> searchObjects,
            int languageId,
            bool useStartDate,
            string mediaTypes,
            OrderObj order,
            int pageIndex,
            int pageSize,
            ref int totalItems)
        {
            if (searchObjects == null && !searchObjects.Any())
                return new List<UnifiedSearchResult>();

            totalItems = 0;
            //create query builder
            var searchNestBuilder = new UnifiedSearchNestBuilder(_esSortingService, _sortingAdapter, _queryInitializer)
            {
                SearchDefinitions = new UnifiedSearchDefinitions
                {
                    langauge = GetDefaultLanguage(),
                    order = order
                },
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            //create query
            var multipleSearchQuery = searchNestBuilder.BuildMultipleSearchObjectsQuery(searchObjects);

            //get response 
            var searchResponse = _elasticClient.Search<object>(search =>
                {
                    search = searchNestBuilder.SetSizeAndFrom(search);
                    return search
                        .Fields(fields => fields.Fields("media_id", "epg_id", "recording_id", "update_date"))
                        .Index(Indices.Index(NamingHelper.GetMediaIndexAlias(_partnerId)))
                        .Query(q => multipleSearchQuery)
                        
                        .Sort(s => searchNestBuilder.GetSort());
                }
            );

            if (!searchResponse.Hits.Any() || !searchResponse.IsValid)
            {
                return new List<UnifiedSearchResult>();
            }

            totalItems = Convert.ToInt32(searchResponse.Total);
            log.Debug("Info - SearchSubscriptionAssets returned search results");

            var unifiedSearchResults = new List<UnifiedSearchResult>();
            
            foreach (var item in searchResponse.Fields)
            {
                var unifiedSearchResult = TryGetUnifiedSearchResultFromResultItem(item);
                if (unifiedSearchResult == null)
                {
                    log.Warn($"Could not get asset id from search request");
                    continue;
                }
                unifiedSearchResults.Add(unifiedSearchResult);
            }


            // Order by stats
            var orderResultsByStats = order.m_eOrderBy <= ApiObjects.SearchObjects.OrderBy.VIEWS &&
                                      order.m_eOrderBy >= ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER ||
                                      order.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.VOTES_COUNT);

            if (!orderResultsByStats)
            {
                return unifiedSearchResults;
            }

            var orderedIds = unifiedSearchResults.Select(item => int.Parse(item.AssetId)).ToList();
            Utils.OrderMediasByStats(orderedIds, (int)order.m_eOrderBy, (int)order.m_eOrderDir);
            Dictionary<int, UnifiedSearchResult> assetIdToSearchResultMap =
                unifiedSearchResults.ToDictionary(item => int.Parse(item.AssetId));

            var orderedResults = new List<UnifiedSearchResult>();

            foreach (var asset in orderedIds)
            {
                if (assetIdToSearchResultMap.TryGetValue(asset, out UnifiedSearchResult result))
                {
                    orderedResults.Add(result);
                }
            }
            
            return orderedResults;
        }

        private UnifiedSearchResult TryGetUnifiedSearchResultFromResultItem(FieldValues item)
        {
            var updateDate = item.Value<DateTime>("update_date");
            var assetId = item.Value<long?>("media_id");
            if (assetId.HasValue)
            {
                return new UnifiedSearchResult()
                {
                    AssetType = eAssetTypes.MEDIA,
                    AssetId = assetId.ToString(),
                    m_dUpdateDate  =updateDate
                };
            }
            
            assetId = item.Value<long?>("epg_id");
            if (assetId.HasValue)
            {
                return new UnifiedSearchResult()
                {
                    AssetType = eAssetTypes.EPG,
                    AssetId = assetId.ToString(),
                    m_dUpdateDate  =updateDate
                };
            }
            
            assetId = item.Value<long?>("recording_id");
            if (assetId.HasValue)
            {
                return new UnifiedSearchResult()
                {
                    AssetType = eAssetTypes.NPVR,
                    AssetId = assetId.ToString(),
                    m_dUpdateDate  =updateDate
                };
            }

            return null;
        }

        public List<int> GetEntitledEpgLinearChannels(UnifiedSearchDefinitions definitions)
        {
            List<int> result = new List<int>();

            // make sure we have epg identifier in the return fields
            if (!definitions.extraReturnFields.Contains(NamingHelper.EPG_IDENTIFIER))
            {
                definitions.extraReturnFields.Add(NamingHelper.EPG_IDENTIFIER);
            }

            // fill language with default language if not specified
            if (definitions.langauge == null)
            {
                definitions.langauge = GetDefaultLanguage();
            }

            var nestBuilder = new UnifiedSearchNestBuilder(_esSortingService, _sortingAdapter, _queryInitializer)
            {
                SearchDefinitions = definitions,
                PageIndex = 0,
                PageSize = 0,
                GetAllDocuments = true
            };

            var searchResponse = Search(nestBuilder);

            if (!searchResponse.IsValid)
            {
                log.Error("Failed getting entitled epg linear channels from ES");
                return result;
            }

            foreach (var item in searchResponse.Hits)
            {
                var epgIdentifier = item.Fields.Value<string>(NamingHelper.EPG_IDENTIFIER);
                if (!string.IsNullOrEmpty(epgIdentifier) && int.TryParse(epgIdentifier, out int epgIdentifierInt))
                {
                    result.Add(epgIdentifierInt);
                }
            }

            return result;
        }

        public bool DoesMediaBelongToChannels(List<int> channelIDs, int mediaId)
        {
            bool result = false;

            if (channelIDs == null || channelIDs.Count < 1)
                return result;

            var channels = GetMediaChannels(mediaId);
            if (channels.IsEmpty())
            {
                return result;
            }

            return channels.Any(x => channelIDs.Contains(x));
        }

        public List<int> GetMediaChannels(int mediaId)
        {
            var result = new List<int>();
            var index = NamingHelper.GetMediaIndexAlias(_partnerId);
            var percolatorIndex = NamingHelper.GetChannelPercolatorIndexAlias(_partnerId);
            var mediaDocId = $"{mediaId}_{GetDefaultLanguage().Code}";
            var response = _elasticClient.Get<NestMedia>(mediaDocId, x => x.Index(index));

            if (response.IsValid && response.Found)
            {
                try
                {
                    var searchResponse = _elasticClient.Search<NestPercolatedQuery>(
                        x => x
                            .Index(percolatorIndex)
                            .Query(q =>
                                q.Percolate(p => p.Documents(response.Source)
                                    .Field(f => f.Query)
                                )
                            )
                            .Size(_maxResults)
                            // only the id is interesting
                            .Source(source => source.Includes(fields => fields.Field(hit => hit.ChannelId)))
                    );

                    if (searchResponse.IsValid && searchResponse.Hits.Any())
                    {
                        return searchResponse.Hits?.Select(x => x.Source.ChannelId).ToList();
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Error - GetMediaChannels - Could not parse response. Ex={ex.Message}, ST: {ex.StackTrace}", ex);                            
                }
            }

            return result;
        }

        public List<string> GetEpgAutoCompleteList(EpgSearchObj epgSearchObj)
        {
            List<string> routing = new List<string>();
            var currentDate = epgSearchObj.m_dStartDate;
            while (currentDate <= epgSearchObj.m_dEndDate)
            {
                routing.Add(currentDate.ToString(ESUtils.ES_DATEONLY_FORMAT));
                currentDate = currentDate.AddDays(1);
            }

            var nestEpgQueries = new NestEpgQueries();
            var nestBaseQueries = new NestBaseQueries();
            
            var epgBuilder = new UnifiedSearchNestEpgBuilder() { Definitions = epgSearchObj };
            var searchResponse = _elasticClient.Search<NestEpg>(s =>
            {
                var must = new List<QueryContainer>();

                var searchPrefix = nestEpgQueries.GetSearchPrefix(epgSearchObj);
                if (searchPrefix != null)
                    must.Add(searchPrefix);

                var startMin = epgSearchObj.m_dStartDate.AddDays(-1);
                var startMax = epgSearchObj.m_dEndDate.AddDays(1);
                
                var epgStartDateRange = nestEpgQueries.GetEpgStartDateRange(startMin, startMax);
                if (epgStartDateRange!=null)
                {
                    must.Add(epgStartDateRange);
                }
                var endDateQueryDescriptor = new QueryContainerDescriptor<NestEpg>().Bool(b => b.Must(
                    m => m.DateRange(t => t.GreaterThanOrEquals(epgSearchObj.m_dStartDate).Field(f => f.EndDate))
                ));
                must.Add(endDateQueryDescriptor);
                must.Add(nestBaseQueries.GetIsActiveTerm());

                var queryDescriptor = new QueryContainerDescriptor<NestEpg>();
                var query = queryDescriptor.Bool(b => b.Must(must.ToArray()));
                query = WrapQueryIfEpgV3Feature(query);

                s = epgBuilder.SetSizeAndFrom<NestEpg>(s);
                s.Index(Indices.Index(epgBuilder.GetIndices()));
                s.Query(q => query);
                s.Source(source => source.Includes(include => include.Field(f1 => f1.Name)));
                if (routing.Any())
                {
                    s.Routing(string.Join(",", routing));
                }
                return s;
            });

            if (!searchResponse.Hits.Any())
            {
                return new List<string>();
            }
            
            return searchResponse.Hits.Select(x => x.Source.Name).Distinct().OrderBy(ob => ob).ToList();
        }

        public List<SearchResult> GetAssetsUpdateDate(eObjectType assetType, List<int> assetIds)
        {
            List<SearchResult> response = new List<SearchResult>();

            string index = string.Empty;
            string idField = string.Empty;

            switch (assetType)
            {
                case eObjectType.Media:
                    index = NamingHelper.GetMediaIndexAlias(_partnerId);
                    idField = "media_id";
                    break;
                case eObjectType.EPG:
                    index = NamingHelper.GetEpgIndexAlias(_partnerId);
                    idField = "epg_id";
                    break;
                case eObjectType.Recording:
                    index = NamingHelper.GetRecordingIndexAlias(_partnerId);
                    idField = "recording_id";
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(index))
            {
                log.Error($"Got invalid asset type when trying to get assets update date. type = {assetType}");
                return response;
            }

            int pageSize = 500;
            for (int from = 0; from < assetIds.Count; from += pageSize)
            {
                var searchResult = _elasticClient.Search<NestMedia>(searchDescriptor => searchDescriptor
                .Index(index)
                .Size(pageSize)
                .From(from)
                .Source(false)
                .Fields(fields => fields.Fields(idField, "update_date"))
                .Query(query => query
                    .Bool(boolQuery => boolQuery
                        .Filter(
                            filter => filter.Terms(terms => terms.Field(idField).Terms<int>(assetIds)),
                            filter => filter.Term("language_id", GetDefaultLanguage().ID)
                            )
                        )
                    )
                ); ;

                foreach (var item in searchResult.Fields)
                {
                    response.Add(new SearchResult()
                    {
                        assetID = item.Value<int>(idField),
                        UpdateDate = item.Value<DateTime>("update_date")
                    });
                }
            }

            return response;
        }
        public List<int> SearchChannels(ChannelSearchDefinitions definitions, ref int totalItems)
        {
            List<int> result = new List<int>();
            totalItems = 0;

            try
            {
                string index = NamingHelper.GetChannelMetadataIndexName(_partnerId);
                var searchResponse = _elasticClient.Search<NestChannelMetadata>(searchDescriptor => searchDescriptor
                    .Index(index)
                    .Size(definitions.PageSize)
                    .From(definitions.PageSize * definitions.PageIndex)
                    .Sort(sort =>
                    {
                        string orderValue = "_id";

                        switch (definitions.OrderBy)
                        {
                            case ChannelOrderBy.Name:
                                {
                                    orderValue = "name";
                                    break;
                                }
                            case ChannelOrderBy.CreateDate:
                                {
                                    orderValue = "create_date";
                                    break;
                                }
                            case ChannelOrderBy.UpdateDate:
                                {
                                    orderValue = "update_date";
                                    break;
                                }
                            case ChannelOrderBy.Id:
                                {
                                    orderValue = "_id";
                                    break;
                                }
                            default:
                                break;
                        }

                        if (definitions.OrderDirection == ApiObjects.SearchObjects.OrderDir.ASC)
                        {
                            sort.Ascending(orderValue);
                        }
                        else
                        {
                            sort.Descending(orderValue);
                        }

                        return sort;
                    })
                    .Query(query => query
                        .Bool(boolQuery =>
                        {
                            QueryContainerDescriptor<NestChannelMetadata> queryContainerDescriptor = new QueryContainerDescriptor<NestChannelMetadata>();
                            List<QueryContainer> mustQueryContainers = new List<QueryContainer>();

                            if (!string.IsNullOrEmpty(definitions.AutocompleteSearchValue))
                            {
                                string value = definitions.AutocompleteSearchValue.ToLower();
                                string field = "name.autocomplete";
                                mustQueryContainers.Add(queryContainerDescriptor.
                                    Match(match => match.Field(field).Query(value).Operator(Operator.And)));
                            }
                            else if (!string.IsNullOrEmpty(definitions.ExactSearchValue))
                            {
                                string value = definitions.ExactSearchValue;
                                string field = "name.lowercase";
                                mustQueryContainers.Add(queryContainerDescriptor.
                                    Match(match => match.Field(field).Query(value)));
                            }
                            else if (definitions.SpecificChannelIds?.Count > 0)
                            {
                                mustQueryContainers.Add(queryContainerDescriptor.
                                    Terms(terms =>
                                        terms
                                        .Field(c => c.ChannelId)
                                        .Terms<int>(definitions.SpecificChannelIds)
                                        )
                                );
                            }

                            if (!definitions.isAllowedToViewInactiveAssets)
                            {
                                var termContainer = queryContainerDescriptor.Term(term => term.
                                    Field(c => c.IsActive).
                                    Value(true)
                                );
                                mustQueryContainers.Add(termContainer);
                            }

                            if (definitions.AssetUserRuleIds != null && definitions.AssetUserRuleIds.Any())
                            {
                                var termContainer = queryContainerDescriptor.Terms(terms => terms.
                                    Field(c => c.AssetUserRuleId).
                                    Terms<long>(definitions.AssetUserRuleIds)
                                );
                                mustQueryContainers.Add(termContainer);
                            }

                            boolQuery.Must(mustQueryContainers.ToArray());
                            return boolQuery;
                        })
                    )
                );

                if (searchResponse.IsValid)
                {
                    totalItems = (int)searchResponse.Total;
                    result.AddRange(searchResponse.Hits.Select(channel => int.Parse(channel.Id)));
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error searching channels on partner {_partnerId}", ex);
            }

            return result;
        }

        public List<TagValue> SearchTags(TagSearchDefinitions definitions, out int totalItems)
        {
            List<TagValue> result = new List<TagValue>();
            totalItems = 0;

            try
            {
                string index = NamingHelper.GetMetadataIndexAlias(_partnerId);
                var searchResponse = _elasticClient.Search<NestTag>(searchDescriptor => searchDescriptor
                    .Index(index)
                    .Size(definitions.PageSize)
                    .From(definitions.PageSize * definitions.PageIndex)
                    //ah.....
                    .Sort(sort => sort.Ascending($"value.{GetDefaultLanguage().Code}"))
                    .Query(query => query
                        .Bool(boolQuery =>
                        {
                            List<QueryContainer> mustQueryContainers = new List<QueryContainer>();
                            var queryContainerDescriptor = new QueryContainerDescriptor<NestTag>();

                            if (definitions.TopicId != 0)
                            {
                                var queryContainer = queryContainerDescriptor.Term(tag => tag.topicId, definitions.TopicId);
                                mustQueryContainers.Add(queryContainer);
                            }

                            if (!string.IsNullOrEmpty(definitions.AutocompleteSearchValue) || !string.IsNullOrEmpty(definitions.ExactSearchValue))
                            {
                                // if we have a specific language - we will search it only
                                if (definitions.Language != null)
                                {
                                    var valueTerms = CreateTagValueBool(definitions, new List<LanguageObj>() { definitions.Language });
                                    mustQueryContainers.AddRange(valueTerms);
                                }
                                else
                                {
                                    
                                    var queryContainer = queryContainerDescriptor.Bool(innerBoolQuery =>
                                      {
                                          var languagesTerms = CreateTagValueBool(definitions, GetCatalogGroupCache().LanguageMapByCode.Values.ToList());

                                          return innerBoolQuery.Should(languagesTerms.ToArray());
                                      });

                                    mustQueryContainers.Add(queryContainer);
                                }
                            }

                            if (definitions.TagIds?.Count > 0)
                            {
                                mustQueryContainers.Add(
                                    queryContainerDescriptor.Terms(terms => terms.Field(tag => tag.tagId).Terms<long>(definitions.TagIds))
                                );
                            }

                            if (definitions.Language != null)
                            {
                                mustQueryContainers.Add(
                                    queryContainerDescriptor.Term(tag => tag.languageId, definitions.Language.ID)
                                );
                            }

                            boolQuery.Must(mustQueryContainers.ToArray());
                            return boolQuery;
                        })
                    )
                );

                Dictionary<long, TagValue> tagsDictionary = new Dictionary<long, TagValue>();

                foreach (var tagHit in searchResponse.Hits)
                {
                    var tag = tagHit.Source;
                    TagValue tagValue = null;

                    if (!tagsDictionary.TryGetValue(tag.tagId, out tagValue))
                    {
                        tagValue = new TagValue()
                        {
                            createDate = tag.createDate,
                            languageId = tag.languageId,
                            tagId = tag.tagId,
                            topicId = tag.topicId,
                            updateDate = tag.updateDate,
                        };

                        tagsDictionary[tag.tagId] = tagValue;
                    }

                    foreach (var value in tag.value)
                    {
                        if (GetCatalogGroupCache().LanguageMapByCode[value.Key].IsDefault)
                        {
                            tagValue.value = value.Value;
                        }
                        else
                        {
                            tagValue.TagsInOtherLanguages.Add(new LanguageContainer(value.Key, value.Value));
                        }
                    }
                }

                totalItems = (int)searchResponse.Total;
                result = tagsDictionary.Values.ToList();
            }
            catch (Exception ex)
            {
                log.Error($"Erorr searching for tags on partner {_partnerId}", ex);
            }
            return result;
        }

        private static List<QueryContainer> CreateTagValueBool(TagSearchDefinitions definitions, List<LanguageObj> languages)
        {
            List<QueryContainer> queryContainers = new List<QueryContainer>();

            string field = string.Empty;
            string value = string.Empty;

            if (!string.IsNullOrEmpty(definitions.AutocompleteSearchValue))
            {
                value = definitions.AutocompleteSearchValue.ToLower();
            }
            else
            {
                value = definitions.ExactSearchValue.ToLower();
            }

            foreach (var language in languages)
            {
                if (!string.IsNullOrEmpty(definitions.AutocompleteSearchValue))
                {
                    field = $"value.{definitions.Language.Code}.autocomplete";
                }
                else
                {
                    field = $"value.{language.Code}.lowercase";
                }

                queryContainers.Add(CreateValueMatchQueryContainer<NestTag>(field, value));
            }

            return queryContainers;
        }

        private static QueryContainer CreateValueMatchQueryContainer<K>(string field, string value)
            where K : class
        {
            return new QueryContainerDescriptor<K>().Match(match => match.Field(field).Query(value));
        }

        public Status UpdateTag(TagValue tagValue)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();
            string index = ESUtils.GetGroupMetadataIndex(_partnerId);

            if (!_elasticClient.Indices.Exists(index).Exists)
            {
                log.Error($"Error - Index of type metadata for group {_partnerId} does not exist");
                status.Message = "Index does not exist";

                return status;
            }

            var bulkRequests = new List<NestEsBulkRequest<NestTag>>();
            try
            {
                var languageId = tagValue.languageId > 0 ? tagValue.languageId : GetDefaultLanguage().ID;
                var catalogGroupCache = GetCatalogGroupCache();
                if (!catalogGroupCache.LanguageMapById.ContainsKey(languageId))
                {
                    log.WarnFormat("Found tag value with non existing language ID. tagId = {0}, tagText = {1}, languageId = {2}",
                        tagValue.tagId, tagValue.value, languageId);
                }
                else
                {
                    var languageCode = catalogGroupCache.LanguageMapById[languageId].Code;
                    var tag = new NestTag(tagValue, languageCode)
                    {
                        languageId = languageId
                    };
                    var bulkRequest = new NestEsBulkRequest<NestTag>()
                    {
                        DocID = $"{tag.tagId}_{languageCode}",
                        Document = tag,
                        Index = index,
                        Operation = eOperation.index
                    };
                    bulkRequests.Add(bulkRequest);
                }

                foreach (var languageContainer in tagValue.TagsInOtherLanguages)
                {
                    int currentLanguageId = 0;

                    if (catalogGroupCache.LanguageMapByCode.ContainsKey(languageContainer.m_sLanguageCode3))
                    {
                        currentLanguageId = catalogGroupCache.LanguageMapByCode[languageContainer.m_sLanguageCode3].ID;

                        if (currentLanguageId > 0)
                        {
                            var tag = new NestTag(tagValue.tagId, tagValue.topicId, currentLanguageId, languageContainer.m_sValue, languageContainer.m_sLanguageCode3, tagValue.createDate, tagValue.updateDate);
                            var bulkRequest = new NestEsBulkRequest<NestTag>()
                            {
                                DocID = $"{tag.tagId}_{languageContainer.m_sLanguageCode3}",
                                Document = tag,
                                Index = index,
                                Operation = eOperation.index
                            };
                            bulkRequests.Add(bulkRequest);
                        }
                    }
                }

                ExecuteAndValidateBulkRequests(bulkRequests);
            }
            catch (Exception ex)
            {
                log.Error($"Error updating tag", ex);
                status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                status.Message = "Failed performing insert query";
            }

            return status;
        }

        public Status DeleteTag(long tagId)
        {
            var status = new ApiObjects.Response.Status();
            string index = ESUtils.GetGroupMetadataIndex(_partnerId);

            var deleteResponse = _elasticClient.DeleteByQuery<NestTag>(request => request
                .Index(index)
                .Query(query => query.Term(tag => tag.tagId, tagId)
                    ));

            if (!deleteResponse.IsValid)
            {
                status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                status.Message = "Failed performing delete query";
            }

            return status;
        }

        public Status DeleteTagsByTopic(long topicId)
        {
            var status = new ApiObjects.Response.Status();
            string index = ESUtils.GetGroupMetadataIndex(_partnerId);

            var deleteResponse = _elasticClient.DeleteByQuery<NestTag>(request => request
                .Index(index)
                .Query(query => query.Term(tag => tag.topicId, topicId)
                    ));

            if (!deleteResponse.IsValid)
            {
                status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                status.Message = "Failed performing delete query";
            }

            return status;
        }

        public List<UnifiedSearchResult> GetAssetsUpdateDates(List<UnifiedSearchResult> assets, ref int totalItems, int pageSize, int pageIndex,
            bool shouldIgnoreRecordings = false)
        {
            List<UnifiedSearchResult> validAssets = new List<UnifiedSearchResult>();
            totalItems = 0;

            // Realize what asset types do we have
            var mediaIds = assets.Where(asset => asset.AssetType == eAssetTypes.MEDIA).Select(asset => long.Parse(asset.AssetId));
            bool shouldSearchMedia = mediaIds.Any();
            var epgIds = assets.Where(asset => asset.AssetType == eAssetTypes.EPG).Select(asset => long.Parse(asset.AssetId));
            bool shouldSearchEpg = epgIds.Any();

            List<string> indices = new List<string>();

            var mediaAlias = NamingHelper.GetMediaIndexAlias(_partnerId);
            var epgAlias = NamingHelper.GetEpgIndexAlias(_partnerId);

            if (shouldSearchMedia)
            {
                indices.Add(mediaAlias);
            }

            if (shouldSearchEpg)
            {
                indices.Add(epgAlias);
            }

            if (indices.Count == 0)
            {
                return validAssets;
            }

            var searchResult = _elasticClient.Search<object>(searchDescriptor => searchDescriptor
                .Index(Indices.Index(indices))
                .Source(false)
                .Fields(fields => fields.Fields("media_id", "epg_id", "update_date"))
                .From(0)
                .Size(_maxResults)
                .Query(query => query
                    .Bool(boolQuery => boolQuery
                        .Should(should =>
                        {
                            if (shouldSearchMedia)
                            {
                                should
                                  .Bool(b => b
                                      .Filter(filter => filter.Prefix(prefix => prefix.Field("_index").Value(mediaAlias)),
                                              filter => filter.Terms(terms => terms.Field("media_id").Terms<long>(mediaIds)))
                                  );
                            }

                            if (shouldSearchEpg)
                            {
                                should
                                  .Bool(b => b
                                      .Filter(filter => filter.Prefix(prefix => prefix.Field("_index").Value(epgAlias)),
                                              filter => filter.Terms(terms => terms.Field("epg_id").Terms<long>(epgIds)))
                                  );
                            }

                            return should;
                        })
                        .Filter(filter => filter.Term("language_id", GetDefaultLanguage().ID))
                    )
                )
            );

            if (searchResult.IsValid)
            {
                foreach (var item in searchResult.Fields)
                {
                    eAssetTypes type = eAssetTypes.MEDIA;
                    long? assetId = item.Value<long?>("media_id");

                    if (!assetId.HasValue)
                    {
                        assetId = item.Value<long?>("epg_id");
                        type = eAssetTypes.EPG;
                    }

                    if (!assetId.HasValue)
                    {
                        log.Warn($"Could not get asset id from search request");
                        continue;
                    }

                    var updateDate = item.Value<DateTime>("update_date");

                    // Find the asset in the list with this ID, set its update date
                    assets.First(result => result.AssetId == assetId.ToString() && result.AssetType == type).m_dUpdateDate = updateDate;
                }
            }

            foreach (UnifiedSearchResult asset in assets)
            {
                if (asset.m_dUpdateDate != DateTime.MinValue || (shouldIgnoreRecordings && asset.AssetType == eAssetTypes.NPVR))
                {
                    validAssets.Add(asset);
                }
                else
                {
                    log.WarnFormat("Received invalid asset from recommendation engine. ID = {0}, type = {1}", asset.AssetId, asset.AssetType.ToString());
                }
            }

            bool illegalRequest = false;
            var pagedList = validAssets.Page(pageSize, pageIndex, out illegalRequest);
            return validAssets;
        }

        public void GetAssetStats(List<int> assetIDs, DateTime startDate, DateTime endDate, StatsType type,
            ref Dictionary<int, AssetStatsResult> assetIDsToStatsMapping)
        {
            string index = NamingHelper.GetStatisticsIndexName(_partnerId);

            string firstPlayAggregationName = "first_play_aggregation";
            string firstPlayTermsAggregationName = "first_play_aggregation_terms";
            string likesAggregationName = "likes_aggregation";
            string likesTermsAggregationName = "likes_aggregation_terms";
            string ratingAggregationName = "ratings_aggregation";
            string ratingTermsAggregationName = "ratings_aggregation_terms";

            var descriptor = new QueryContainerDescriptor<ApiLogic.IndexManager.NestData.NestSocialActionStatistics>();
            var searchResponse = _elasticClient.Search<ApiLogic.IndexManager.NestData.NestSocialActionStatistics>(searchRequest => searchRequest
                .Index(index)
                .Size(0)
                .From(0)
                .Query(query => query
                    .Bool(boolQuery =>
                    {
                        List<QueryContainer> mustContainers = new List<QueryContainer>();

                        // group_id = 1234
                        mustContainers.Add(descriptor.Term(field => field.GroupID, _partnerId));

                        // action_date <= max and action_date >= min
                        if (!startDate.Equals(DateTime.MinValue) || !endDate.Equals(DateTime.MaxValue))
                        {
                            var dateRange = descriptor.DateRange(range =>
                            {
                                range = range.Field(field => field.Date);

                                if (!startDate.Equals(DateTime.MinValue))
                                {
                                    range = range.GreaterThanOrEquals(startDate);
                                }

                                if (!endDate.Equals(DateTime.MaxValue))
                                {
                                    range = range.LessThanOrEquals(endDate);
                                }

                                return range;
                            });

                            mustContainers.Add(dateRange);
                        }

                        // media_id in (1, 2, 3)
                        mustContainers.Add(descriptor.Terms(terms => terms.Field(field => field.MediaID).Terms<int>(assetIDs)));

                        boolQuery.Must(mustContainers.ToArray());
                        return boolQuery;
                    })
                )
                .Aggregations(rootAggs =>
                {
                    // first play and ratings are relevant only to medi
                    if (type == StatsType.MEDIA)
                    {
                        // first play aggregation
                        rootAggs.Filter(firstPlayAggregationName, firstPlayAggregation =>
                        {
                            // filter aggregation
                            firstPlayAggregation.Filter(filter => filter.Term(field => field.Action, NamingHelper.STAT_ACTION_FIRST_PLAY));

                            // sub aggregation - terms on media id
                            firstPlayAggregation.Aggregations(firstPlayAggs =>
                            {
                                firstPlayAggs.Terms(firstPlayTermsAggregationName, terms =>
                                {
                                    terms.Field(field => field.MediaID);
                                    // sub aggregation of terms - sum aggregation
                                    terms.Aggregations(termsAggs =>
                                    {
                                        termsAggs.Sum(NamingHelper.SUB_SUM_AGGREGATION_NAME, subSumAggregation =>
                                            subSumAggregation.Field(field => field.Count).Missing(1));
                                        return termsAggs;
                                    });

                                    return terms;
                                });

                                return firstPlayAggs;
                            });

                            return firstPlayAggregation;
                        });

                        // rates aggregation
                        rootAggs.Filter(ratingAggregationName, ratesAgg =>
                        {
                            // filter aggregation
                            ratesAgg.Filter(filter => filter.Term(field => field.Action, NamingHelper.STAT_ACTION_RATES));

                            // sub aggregation - terms on media id
                            ratesAgg.Aggregations(ratesAggs =>
                            {
                                ratesAggs.Terms(ratingTermsAggregationName, terms =>
                                {
                                    terms.Field(field => field.MediaID);

                                    // sub aggregation of terms = stats aggregation
                                    terms.Aggregations(termsAggs =>
                                    {
                                        termsAggs.Stats(NamingHelper.SUB_STATS_AGGREGATION_NAME, subStatsAggregation =>
                                            subStatsAggregation.Field(field => field.RateValue));
                                        return termsAggs;
                                    });

                                    return terms;
                                });

                                return ratesAggs;
                            });

                            return ratesAgg;
                        });
                    }

                    // liks aggregation
                    rootAggs.Filter(likesAggregationName, likesAgg =>
                    {
                        // filter aggregation
                        likesAgg.Filter(filter => filter.Term(field => field.Action, NamingHelper.STAT_ACTION_LIKE));

                        // sub aggregation - just terms on media id
                        likesAgg.Aggregations(likesAggs =>
                        {
                            likesAggs.Terms(likesTermsAggregationName, terms =>
                            {
                                terms.Field(field => field.MediaID);

                                return terms;
                            });

                            return likesAggs;
                        });

                        return likesAgg;
                    });

                    return rootAggs;
                })
            );

            if (searchResponse.IsValid)
            {
                // fill likes
                if (searchResponse.Aggregations.ContainsKey(likesAggregationName))
                {
                    var currentAgg = searchResponse.Aggregations[likesAggregationName] as SingleBucketAggregate;
                    var likesAgg = currentAgg[likesTermsAggregationName] as BucketAggregate;

                    foreach (var item in likesAgg.Items)
                    {
                        var bucket = item as KeyedBucket<object>;
                        var mediaId = Convert.ToInt32(bucket.Key);

                        if (assetIDsToStatsMapping.ContainsKey(mediaId))
                        {
                            assetIDsToStatsMapping[mediaId].m_nLikes = Convert.ToInt32(bucket.DocCount);
                        }
                    }
                }

                // fill views (if there are any)
                if (searchResponse.Aggregations.ContainsKey(firstPlayAggregationName))
                {
                    var currentAgg = searchResponse.Aggregations[firstPlayAggregationName] as SingleBucketAggregate;
                    var firstPlayAgg = currentAgg[firstPlayTermsAggregationName] as BucketAggregate;

                    foreach (var item in firstPlayAgg.Items)
                    {
                        var bucket = item as KeyedBucket<object>;
                        var mediaId = Convert.ToInt32(bucket.Key);

                        if (assetIDsToStatsMapping.ContainsKey(mediaId))
                        {
                            if (bucket.ContainsKey(NamingHelper.SUB_SUM_AGGREGATION_NAME))
                            {
                                var sumBucket = bucket[NamingHelper.SUB_SUM_AGGREGATION_NAME] as ValueAggregate;
                                assetIDsToStatsMapping[mediaId].m_nViews = Convert.ToInt32(sumBucket.Value);
                            }
                        }
                    }
                }

                // fill ratings
                if (searchResponse.Aggregations.ContainsKey(ratingAggregationName))
                {
                    var currentAgg = searchResponse.Aggregations[ratingAggregationName] as SingleBucketAggregate;
                    var ratesAgg = currentAgg[ratingTermsAggregationName] as BucketAggregate;

                    foreach (var item in ratesAgg.Items)
                    {
                        var bucket = item as KeyedBucket<object>;
                        var mediaId = Convert.ToInt32(bucket.Key);

                        if (assetIDsToStatsMapping.ContainsKey(mediaId))
                        {
                            if (bucket.ContainsKey(NamingHelper.SUB_STATS_AGGREGATION_NAME))
                            {
                                var statsBucket = bucket[NamingHelper.SUB_STATS_AGGREGATION_NAME] as StatsAggregate;
                                assetIDsToStatsMapping[mediaId].m_dRate = Convert.ToDouble(statsBucket.Average);
                                assetIDsToStatsMapping[mediaId].m_nVotes = Convert.ToInt32(statsBucket.Count);
                            }
                        }
                    }
                }
            }
        }

        public List<int> OrderMediaBySlidingWindow(OrderBy orderBy, bool isDesc, int pageSize, int PageIndex, List<int> media,
            DateTime windowTime)
        {
            List<int> result;
            DateTime now = DateTime.UtcNow;
            switch (orderBy)
            {
                case OrderBy.VIEWS:

                    result = SlidingWindowCountAggregations(media, windowTime, now, NamingHelper.STAT_ACTION_FIRST_PLAY);
                    break;
                case OrderBy.RATING:
                    result = SlidingWindowStatisticsAggregations(media,
                        windowTime,
                        now,
                        NamingHelper.STAT_ACTION_RATES,
                        NamingHelper.STAT_ACTION_RATE_VALUE_FIELD,
                        ElasticSearch.Searcher.AggregationsComparer.eCompareType.Average);
                    break;
                case OrderBy.VOTES_COUNT:
                    result = SlidingWindowCountAggregations(media, windowTime, now, NamingHelper.STAT_ACTION_RATES);
                    break;
                case OrderBy.LIKE_COUNTER:
                    result = SlidingWindowCountAggregations(media, windowTime, now, NamingHelper.STAT_ACTION_LIKE);
                    break;
                default:
                    result = media;
                    break;
            }

            if (result != null && result.Count > 0)
            {
                // all results are returned ordered by descending
                if (isDesc)
                {
                    result = Utils.ListPaging(result, pageSize, PageIndex);
                }
                else
                {
                    result.Reverse();
                    result = Utils.ListPaging(result, pageSize, PageIndex);
                }
            }

            return result;
            
        }

        private List<int> SlidingWindowStatisticsAggregations(List<int> mediaIds, DateTime dtStartDate,
            DateTime endDate, string action, string valueField, AggregationsComparer.eCompareType compareType)
        {
            List<int> result = new List<int>(mediaIds);

            var assetIds = mediaIds.Select(id => (long) id).ToList();
            OrderBy orderBy = OrderBy.ID;

            switch (action)
            {
                case NamingHelper.STAT_ACTION_FIRST_PLAY:
                {
                    orderBy = OrderBy.VIEWS;
                    break;
                }
                case NamingHelper.STAT_ACTION_LIKE:
                {
                    orderBy = OrderBy.LIKE_COUNTER;
                    break;
                }
                case NamingHelper.STAT_ACTION_RATES:
                {
                    orderBy = OrderBy.RATING;
                    break;
                }
                default:
                {
                    break;
                }
            }
            
            var orderedList = _statisticsSortStrategy.SortAssetsByStats(assetIds, orderBy, OrderDir.DESC, _partnerId, dtStartDate, endDate);

            result = orderedList.Select(id => (int) id).ToList();

            return result;
            
        }

        private List<int> SlidingWindowCountAggregations(List<int> mediaIds, DateTime startDate,
            DateTime endDate, string action)
        {
            List<int> result = new List<int>(mediaIds);

            var assetIds = mediaIds.Select(id => (long)id).ToList();
            OrderBy orderBy = OrderBy.ID;

            switch (action)
            {
                case NamingHelper.STAT_ACTION_FIRST_PLAY:
                {
                    orderBy = OrderBy.VIEWS;
                    break;
                }
                case NamingHelper.STAT_ACTION_LIKE:
                {
                    orderBy = OrderBy.LIKE_COUNTER;
                    break;
                }
                case NamingHelper.STAT_ACTION_RATES:
                {
                    orderBy = OrderBy.RATING;
                    break;
                }
                default:
                {
                    break;
                }
            }
            
            var orderedList = _statisticsSortStrategy.SortAssetsByStats(assetIds, orderBy, OrderDir.DESC, _partnerId, startDate, endDate);

            result = orderedList.Select(id => (int)id).ToList();

            return result;
        }

        public bool SetupSocialStatisticsDataIndex()
        {
            var statisticsIndex = NamingHelper.GetStatisticsIndexName(_partnerId);
            var createIndexResponse = _elasticClient.Indices.Create(statisticsIndex,
                c => c.Settings(settings => settings
                    .NumberOfShards(_numOfShards)
                    .NumberOfReplicas(_numOfReplicas)
                    )
                );
            bool result = createIndexResponse != null && createIndexResponse.Acknowledged && createIndexResponse.IsValid;

            return result;
        }

        public bool InsertSocialStatisticsData(ApiObjects.Statistics.SocialActionStatistics action)
        {
            var statisticsIndex = NamingHelper.GetStatisticsIndexName(_partnerId);

            try
            {
                var nestSocialActionStatistics = NestDataCreator.GetSocialActionStatistics(action);
                var response = _elasticClient.Index(nestSocialActionStatistics, i => i.Index(statisticsIndex));
                var result = response != null && response.IsValid && response.Result == Result.Created;

                if (result)
                    return true;

                var actionStatsJson = JsonConvert.SerializeObject(action);
                log.Debug("InsertStatisticsToES " + string.Format("Was unable to insert record to ES. index={0};doc={1}",
                    statisticsIndex, actionStatsJson));

            }
            catch (Exception ex)
            {
                log.Error($"InsertStatisticsToES - Failed ex={ex.Message}, group={_partnerId}", ex);
            }

            return false;
        }

        public bool DeleteSocialAction(StatisticsActionSearchObj socialSearch)
        {
            bool result = false;
            var index = NamingHelper.GetStatisticsIndexName(_partnerId);

            try
            {
                if (_elasticClient.Indices.Exists(index).Exists)
                {
                    var queryBuilder = new ESStatisticsQueryBuilder(_partnerId, socialSearch);
                    var query = queryBuilder.BuildQuery();

                    var deleteResponse = _elasticClient.DeleteByQuery<NestSocialActionStatistics>(request => request
                        .Index(index)
                        .Query(q =>
                            {
                                return query;
                            }
                        ));

                    result = deleteResponse.IsValid;

                    log.Debug($"DeleteSocialAction. Deleted = {deleteResponse.Deleted}, Failed = {deleteResponse.Failures.Count}");
                }
            }
            catch (Exception ex)
            {
                log.DebugFormat("DeleteActionFromES Failed ex={0}, index={1};type={2}", ex, index, ESUtils.ES_STATS_TYPE);
            }

            return result;
        }

        public string SetupIPToCountryIndex()
        {
            string newIndexName = NamingHelper.GetNewIpToCountryIndexName();

            var createResponse = _elasticClient.Indices.Create(newIndexName,
                 c => c.Settings(settings => settings
                     .NumberOfShards(_numOfShards)
                     .NumberOfReplicas(_numOfReplicas)
                     .Analysis(a => a
                         .Analyzers(an => an.Custom(DEFAULT_LOWERCASE_ANALYZER,
                        ca => ca
                        .CharFilters("html_strip")
                        .Tokenizer("keyword")
                        .Filters("lowercase", "asciifolding")
                     ))))
                 .Map<object>(map => map
                     .AutoMap<NestTag>()
                     .Properties(properties => properties
                        .Text(name => name.Name("name").Analyzer(DEFAULT_LOWERCASE_ANALYZER))
                     )
                 ));

            bool isIndexCreated = createResponse != null && createResponse.Acknowledged && createResponse.IsValid;
            if (!isIndexCreated)
            {
                log.Error(string.Format("Failed creating index for index:{0}", newIndexName));
                return string.Empty;
            }

            return newIndexName;
        }

        public bool InsertDataToIPToCountryIndex(string newIndexName, List<IPV4> ipV4ToCountryMapping, List<IPV6> ipV6ToCountryMapping)
        {
            bool result = false;
            int sizeOfBulk = 5000;

            var bulkRequestsIpv4 = new List<NestEsBulkRequest<NestIPv4>>();
            var bulkRequestsIpv6 = new List<NestEsBulkRequest<NestIPv6>>();
            try
            {
                if (ipV4ToCountryMapping != null)
                {
                    foreach (var ipv4 in ipV4ToCountryMapping)
                    {
                        var nestObject = new NestIPv4(ipv4);

                        var bulkRequest = new NestEsBulkRequest<NestIPv4>()
                        {
                            DocID = $"ipv4.{nestObject.id}",
                            Document = nestObject,
                            Index = newIndexName,
                            Operation = eOperation.index
                        };
                        bulkRequestsIpv4.Add(bulkRequest);

                        // If we exceeded maximum size of bulk 
                        if (bulkRequestsIpv4.Count >= sizeOfBulk)
                        {
                            ExecuteAndValidateBulkRequests(bulkRequestsIpv4);
                        }
                    }

                    // If we have anything left that is less than the size of the bulk
                    if (bulkRequestsIpv4.Any())
                    {
                        ExecuteAndValidateBulkRequests(bulkRequestsIpv4);
                    }
                }

                if (ipV6ToCountryMapping != null)
                {
                    foreach (var ipv6 in ipV6ToCountryMapping)
                    {
                        var nestObject = new NestIPv6(ipv6);

                        var bulkRequest = new NestEsBulkRequest<NestIPv6>()
                        {
                            DocID = $"ipv6.{nestObject.id}",
                            Document = nestObject,
                            Index = newIndexName,
                            Operation = eOperation.index
                        };
                        bulkRequestsIpv6.Add(bulkRequest);

                        // If we exceeded maximum size of bulk 
                        if (bulkRequestsIpv6.Count >= sizeOfBulk)
                        {
                            ExecuteAndValidateBulkRequests(bulkRequestsIpv6);
                        }
                    }

                    // If we have anything left that is less than the size of the bulk
                    if (bulkRequestsIpv6.Any())
                    {
                        ExecuteAndValidateBulkRequests(bulkRequestsIpv6);
                    }
                }

                result = true;
            }
            finally
            {
                if (bulkRequestsIpv4.Any())
                {
                    log.Debug($"Clearing bulk requests");
                    bulkRequestsIpv4.Clear();
                }

                if (bulkRequestsIpv6.Any())
                {
                    log.Debug($"Clearing bulk requests");
                    bulkRequestsIpv6.Clear();
                }
            }

            return result;
        }

        public bool PublishIPToCountryIndex(string newIndexName)
        {
            string alias = NamingHelper.GetIpToCountryIndexAlias();

            return this.SwitchIndexAlias(newIndexName, alias, true, true);
        }

        public ApiObjects.Country GetCountryByCountryName(string countryName)
        {
            ApiObjects.Country result = null;

            try
            {
                if (string.IsNullOrEmpty(countryName))
                {
                    return result;
                }

                string index = NamingHelper.GetIpToCountryIndexAlias();

                var searchResult = _elasticClient.Search<NestCountry>(search => search
                    .Index(index)
                    .Size(1)
                    .Fields(fields => fields
                        .Fields(f => f.country_id, f => f.name, f => f.name))
                    .Query(q => q
                        .Term(term => term.name, countryName)
                    )
                );

                //try get result
                var nestCountry = searchResult?.Hits?.FirstOrDefault()?.Source;

                if (nestCountry != null)
                {
                    result = nestCountry.ToApiObject();
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed GetCountryByCountryName for countryName: {countryName}", ex);
            }

            return result;
        }

        public ApiObjects.Country GetCountryByIp(string ip, out bool searchSuccess)
        {
            ApiObjects.Country result = null;
            searchSuccess = false;
            if (string.IsNullOrEmpty(ip)) { return null; }

            if (IPAddress.TryParse(ip, out IPAddress address))
            {
                if (IndexManagerCommonHelpers.CheckIpIsPrivate(address))
                {
                    searchSuccess = true;
                    return null;
                }

                IpToCountryHandler handler = null;
                if (address.AddressFamily == AddressFamily.InterNetworkV6 && !address.IsIPv4MappedToIPv6)
                {
                    handler = IpToCountryHandler.Handlers[AddressFamily.InterNetworkV6];
                }
                else
                {
                    handler = IpToCountryHandler.Handlers[AddressFamily.InterNetwork];
                }

                var ipValue = handler.ConvertIpToValidString(address);
                log.DebugFormat("GetCountryByIp: ip={0} was converted to ipValue={1}.", ip, ipValue);

                string index = NamingHelper.GetIpToCountryIndexAlias();

                // Perform search
                var searchResult = _elasticClient.Search<NestCountry>(search => search
                    .Index(index)
                    .Size(1)
                    .Fields(fields => fields
                        .Fields("country_id", "name", "code"))
                    .Query(q => handler.BuildNestQueryForIp(q, ipValue))
                    );

                searchSuccess = searchResult.IsValid;
                //try get result
                var nestCountry = searchResult?.Hits?.FirstOrDefault()?.Source;

                if (nestCountry != null)
                {
                    result = nestCountry.ToApiObject();
                }
            }

            return result;
        }

        public List<string> GetChannelPrograms(int channelId, DateTime startDate, DateTime endDate, List<ESOrderObj> esOrderObjs)
        {
            var index = NamingHelper.GetEpgIndexAlias(_partnerId);
            var searchResponse = _elasticClient.Search<NestEpg>(s =>
                GetChannelProgramsSearchDescriptor(channelId, startDate, endDate, esOrderObjs, index)
            );

            if (!searchResponse.IsValid)
            {
                log.Debug($"{searchResponse.DebugInformation}");
                return new List<string>();
            }

            if (searchResponse.IsValid && !searchResponse.Hits.Any())
            {
                return new List<string>();
            }

            // Checking is new Epg ingest here as well to avoid calling GetEpgCBKey if we already called elastic and have all required coument Ids
            var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(_partnerId);
            var isNewEpgIngest = epgFeatureVersion != EpgFeatureVersion.V1;

            if (isNewEpgIngest)
            {
                return searchResponse.Fields.Select(x => x.Value<string>("cb_document_id")).Distinct().ToList();
            }

            var resultEpgIds = searchResponse.Fields.Select(x => x.Value<long>("epg_id")).Distinct().ToList();
            return resultEpgIds.Select(epgId => GetEpgCbKey(epgId)).ToList();
        }

        private SearchDescriptor<NestEpg> GetChannelProgramsSearchDescriptor(int channelId, DateTime startDate, DateTime endDate, List<ESOrderObj> esOrderObjs, string index)
        {
            var queryDescriptor = new QueryContainerDescriptor<NestEpg>();
            var query = queryDescriptor.Bool(b => b.Filter(f => f
                    .Bool(b1 =>
                        b1.Must(
                            m => m.Terms(t => t.Field(f1 => f1.ChannelID).Terms(channelId)),
                            m => m.DateRange(dr => dr.Field(f1 => f1.StartDate).GreaterThanOrEquals(startDate)),
                            m => m.DateRange(dr => dr.Field(f1 => f1.EndDate).LessThanOrEquals(endDate))
                        )
                    )
                )
            );

            query = WrapQueryIfEpgV3Feature(query);

            return new SearchDescriptor<NestEpg>()
                            .Index(index)
                            .Size(_maxResults)
                            .Fields(sf => sf.Fields(fs => fs.CouchbaseDocumentId, fs => fs.EpgID))
                            .Source(false)
                            .Query(q => query)
                            .Sort(x =>
                            {
                                return BuildSortDescriptorFromOrderObj(esOrderObjs);
                            });
        }

        private static IPromise<IList<ISort>> BuildSortDescriptorFromOrderObj(List<ESOrderObj> esOrderObjs)
        {
            // TODO: gil make generic
            var descriptor = new SortDescriptor<NestEpg>();
            foreach (var order in esOrderObjs)
            {
                switch (order.m_eOrderDir)
                {
                    case OrderDir.ASC:
                        descriptor = descriptor.Ascending(new Field(order.m_sOrderValue));
                        break;
                    case OrderDir.DESC:
                        descriptor = descriptor.Descending(new Field(order.m_sOrderValue));
                        break;
                }
            }

            return descriptor;
        }


        private List<string> GetEpgsCbKeys(IEnumerable<long> epgIds, IEnumerable<LanguageObj> langCodes,
            bool isAddAction)
        {
            var result = new List<string>();
            var epgFeatureVersion = _groupSettingsManager.GetEpgFeatureVersion(_partnerId);
            if (epgFeatureVersion != EpgFeatureVersion.V1 && !isAddAction)
            {
                // elasticsearch holds the current document in CB so we go there to take it
                return GetEpgCBDocumentIdsByEpgId(epgIds, langCodes);
            }

            result.AddRange(epgIds.Select(x => x.ToString()));
            return result;
        }

        private string GetEpgCbKey(long epgId, string langCode = null, bool isAddAction = false)
        {
            var langs = string.IsNullOrEmpty(langCode) ? null : new[] { new LanguageObj { Code = langCode } };
            var keys = GetEpgsCbKeys(new[] { epgId }, langs, isAddAction);
            return keys.FirstOrDefault();
        }

        public List<string> GetEpgCBDocumentIdsByEpgId(IEnumerable<long> epgIds, IEnumerable<LanguageObj> languages)
        {
            languages = languages ?? Enumerable.Empty<LanguageObj>();
            var epgIdsList = epgIds.ToList();
            var alias = NamingHelper.GetEpgIndexAlias(_partnerId);
            var searchResult = _elasticClient.Search<NestEpg>(searchDescriptor => searchDescriptor
                .Index(alias)
                .Source(false)
                .Size(_maxResults)
                .Fields(sf => sf.Fields(fs => fs.CouchbaseDocumentId, fs => fs.EpgID))
                .Query(q => q.Bool(b =>
                        b.Filter(f =>
                            f.Bool(b2 =>
                                b2.Must(
                                    m => m.Terms(t => t.Field(f1 => f1.EpgID).Terms(epgIdsList)),
                                    m => m.Terms(t => t.Field(f1 => f1.LanguageId).Terms(languages.Select(x => x.ID)))
                                )
                            )
                        )
                    )
                )
            );


            if (!searchResult.IsValid)
            {
                throw new Exception($"GetEpgCBDocumentIdByEpgIdFromElasticsearch > " +
                    $"Got empty results from elasticsearch epgIds:[{string.Join(",", epgIdsList)}], " +
                    $"_partnerId:[{_partnerId}], langCodes:[{string.Join(",", languages)}]");
            }

            var resultEpgIds = searchResult.Fields.Select(x => x.Value<long>("epg_id")).Distinct().ToList();
            var resultEpgDocIds = searchResult.Fields.Select(x => x.Value<string>("cb_document_id")).Distinct().ToList();

            var except = epgIdsList.Except(resultEpgIds);
            if (except.Any())
            {
                resultEpgDocIds.AddRange(epgIdsList.Select(x=>x.ToString()));
            }

            return resultEpgDocIds.Distinct().ToList();
        }

        public bool SetupMediaIndex(DateTime indexDate)
        {
            string newIndexName = NamingHelper.GetMediaIndexName(_partnerId, indexDate);
            bool isIndexCreated = CreateMediaIndex(newIndexName);

            if (!isIndexCreated)
            {
                log.Error(string.Format("Failed creating index for index:{0}", newIndexName));
            }

            return isIndexCreated;
        }

        private bool CreateMediaIndex(string newIndexName, bool shouldAddPercolators = false)
        {
            // Default size of max results should be 100,000
            int maxResults = _maxResults;

            if (maxResults <= 0)
            {
                maxResults = MAX_RESULTS_DEFAULT;
            }

            var languages = GetLanguages();

            // get definitions of analyzers, filters and tokenizers
            GetAnalyzersWithLowercaseAndPhraseStartsWith(languages, out var analyzers, out var customProperties, out var filters, out var tokenizers, out var normalizers);
            AnalyzersDescriptor analyzersDescriptor = GetAnalyzersDesctiptor(analyzers);
            TokenFiltersDescriptor filtersDescriptor = GetTokenFiltersDescriptor(filters);
            TokenizersDescriptor tokenizersDescriptor = GetTokenizersDesctiptor(tokenizers);
            NormalizersDescriptor normalizersDescriptor = GetNormalizersDescriptor(normalizers);
            _groupManager.RemoveGroup(_partnerId); // remove from cache

            if (!GetMetasAndTagsForMapping(
                out var metas,
                out var tags,
                out var metasToPad))
            {
                throw new Exception($"failed to get metas and tags");
            }

            var propertiesDescriptor = GetMediaPropertiesDescriptor(languages, metas, tags, metasToPad, analyzers, customProperties);
            var createResponse = _elasticClient.Indices.Create(newIndexName,
                c => c.Settings(settings =>
                {
                    var maxClauseCount = _applicationConfiguration.ElasticSearchHandlerConfiguration.MaxClauseCount;
                    if (maxClauseCount.Value != maxClauseCount.GetDefaultValue())
                    {
                        settings = settings.Setting("indices.query.bool.max_clause_count", maxClauseCount.Value);
                    }

                    return settings
                      .NumberOfShards(_numOfShards)
                      .NumberOfReplicas(_numOfReplicas)
                      .Setting("index.max_result_window", _maxResults)
                      .Setting("index.max_ngram_diff", _applicationConfiguration.ElasticSearchHandlerConfiguration.MaxNgramDiff.Value)
                      .Setting("index.mapping.total_fields.limit", _applicationConfiguration.ElasticSearchHandlerConfiguration.TotalFieldsLimit.Value)
                      .Setting("index.max_inner_result_window", _applicationConfiguration.ElasticSearchConfiguration.MaxInnerResultWindow.Value)
                      .Analysis(a => a
                          .Analyzers(an => analyzersDescriptor)
                          .TokenFilters(tf => filtersDescriptor)
                          .Tokenizers(t => tokenizersDescriptor)
                          .Normalizers(n => normalizersDescriptor)
                      );
                })
                .Map(map =>
                {
                    if (shouldAddPercolators)
                    {
                        map = map.AutoMap<NestPercolatedQuery>().AutoMap<NestEpg>();
                    }

                    map = map.AutoMap<NestMedia>().AutoMap<NestBaseAsset>();

                    return map.Properties(props =>
                    {
                        if (shouldAddPercolators)
                        {
                            propertiesDescriptor = propertiesDescriptor.Percolator(x => x.Name("query"));
                        }

                        return propertiesDescriptor;
                    });
                }
                ));

            bool isIndexCreated = createResponse != null && createResponse.Acknowledged && createResponse.IsValid;
            return isIndexCreated;
        }

        public bool SetupChannelPercolatorIndex(DateTime indexDate)
        {
            string percolatorsIndexName = NamingHelper.GetChannelPercolatorIndex(_partnerId, indexDate);
            bool isIndexCreated = CreateMediaIndex(percolatorsIndexName, true);

            if (!isIndexCreated)
            {
                log.Error(string.Format("Failed creating index for index:{0}", percolatorsIndexName));
            }

            return isIndexCreated;
        }

        public void PublishChannelPercolatorIndex(DateTime indexDate, bool shouldSwitchIndexAlias,
            bool shouldDeleteOldIndices)
        {
            string alias = NamingHelper.GetChannelPercolatorIndexAlias(_partnerId);
            var indexName = NamingHelper.GetChannelPercolatorIndex(_partnerId, indexDate);
            this.SwitchIndexAlias(indexName, alias, shouldDeleteOldIndices, shouldSwitchIndexAlias);
        }

        public void InsertMedias(Dictionary<int, Dictionary<int, Media>> groupMedias, DateTime indexDate)
        {
            var indexName = NamingHelper.GetMediaIndexName(_partnerId, indexDate);
            var sizeOfBulk = GetBulkSize();

            log.DebugFormat("Start indexing medias. total medias={0}", groupMedias.Count);
            // save current value to restore at the end
            var currentDefaultConnectionLimit = ServicePointManager.DefaultConnectionLimit;
            try
            {
                var maxDegreeOfParallelism = GetMaxDegreeOfParallelism();
                var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism };
                var contextData = new LogContextData();
                ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount;

                HashSet<string> metasToPad = GetMetasToPad();

                // For each media
                var bulkRequests = GetMediaBulkRequests(groupMedias, indexName, sizeOfBulk, metasToPad);
                // Send request to elastic search in a different thread
                Parallel.ForEach(bulkRequests, options, (bulkRequest, state) =>
                {
                    contextData.Load();
                    ExecuteAndValidateBulkRequests(bulkRequest.Value);
                });
            }
            catch (Exception ex)
            {
                log.Error("Failed during InsertMedias", ex);
            }
            finally
            {
                ServicePointManager.DefaultConnectionLimit = currentDefaultConnectionLimit;
            }
        }

        private Dictionary<int, List<NestEsBulkRequest<NestMedia>>> GetMediaBulkRequests(Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> groupMedias,
            string newIndexName,
            int sizeOfBulk,
            HashSet<string> metasToPad = null)
        {
            var numOfBulkRequests = 0;
            var bulkRequests = new Dictionary<int, List<NestEsBulkRequest<NestMedia>>>() { { numOfBulkRequests, new List<NestEsBulkRequest<NestMedia>>() } };

            foreach (var groupMedia in groupMedias)
            {
                var groupMediaValue = groupMedia.Value;
                numOfBulkRequests = GetMediaNestEsBulkRequest(newIndexName, sizeOfBulk, numOfBulkRequests, bulkRequests, groupMediaValue, metasToPad);
            }

            return bulkRequests;
        }

        private int GetMediaNestEsBulkRequest(string newIndexName, int sizeOfBulk, int numOfBulkRequests, 
            Dictionary<int, List<NestEsBulkRequest<NestMedia>>> bulkRequests, 
            Dictionary<int, ApiObjects.SearchObjects.Media> groupMediaValue,
            HashSet<string> metasToPad = null)
        {
            // For each language
            foreach (var languageId in groupMediaValue.Keys.Distinct())
            {
                var language = GetLanguageById(languageId);
                var media = groupMediaValue[languageId];

                if (media == null)
                    continue;

                if (metasToPad == null)
                {
                    metasToPad = GetMetasToPad();
                }

                media.PadMetas(metasToPad);

                var bulkRequest =
                    GetMediaNestEsBulkRequest(newIndexName,
                        media,
                        language,
                        bulkRequests,
                        sizeOfBulk,
                        ref numOfBulkRequests);

                bulkRequests[numOfBulkRequests].Add(bulkRequest);
            }

            return numOfBulkRequests;
        }

        private NestEsBulkRequest<NestMedia> GetMediaNestEsBulkRequest(string indexName, ApiObjects.SearchObjects.Media media,
            LanguageObj language, Dictionary<int, List<NestEsBulkRequest<NestMedia>>> bulkRequests, int sizeOfBulk, ref int numOfBulkRequests)
        {
            var nestMedia = NestDataCreator.GetMedia(media, language);

            // If we exceeded the size of a single bulk request then create another list
            if (bulkRequests[numOfBulkRequests].Count >= sizeOfBulk)
            {
                numOfBulkRequests++;
                bulkRequests.Add(numOfBulkRequests, new List<NestEsBulkRequest<NestMedia>>());
            }
            var docId = nestMedia.DocumentId;
            return new NestEsBulkRequest<NestMedia>(docId, indexName, nestMedia);
        }

        private static int GetMaxDegreeOfParallelism()
        {
            var maxDegreeOfParallelism = ApplicationConfiguration.Current.RecordingsMaxDegreeOfParallelism.Value;
            if (maxDegreeOfParallelism == 0)
                maxDegreeOfParallelism = 5;
            return maxDegreeOfParallelism;
        }

        public void PublishMediaIndex(DateTime indexDate, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            string alias = NamingHelper.GetMediaIndexAlias(_partnerId);
            var indexName = NamingHelper.GetMediaIndexName(_partnerId, indexDate);
            SwitchIndexAlias(indexName, alias, shouldDeleteOldIndices, shouldSwitchIndexAlias);
        }

        public bool AddChannelsPercolatorsToIndex(HashSet<int> channelIds, DateTime? indexDate, bool shouldCleanupInvalidChannels = false)
        {
            try
            {
                var channelPercolatorIndexAlias = NamingHelper.GetChannelPercolatorIndexAlias(_partnerId);
                if (!indexDate.HasValue)
                {
                    var aliasResponse = _elasticClient.Indices.GetAlias(channelPercolatorIndexAlias);
                    if (!aliasResponse.IsValid)
                    {
                        log.ErrorFormat("Channel percolator index alias '{0}' does not exist. Run rebuild media index first.", channelPercolatorIndexAlias);

                        return false;
                    }
                }

                var indexName = indexDate.HasValue
                    ? NamingHelper.GetChannelPercolatorIndex(_partnerId, indexDate.Value)
                    : channelPercolatorIndexAlias;
                var groupChannels = IndexManagerCommonHelpers.GetGroupChannels(_partnerId, _channelManager, IsOpc(), ref channelIds);

                var bulkRequests = new List<NestEsBulkRequest<NestPercolatedQuery>>();
                int sizeOfBulk = 50;

                foreach (var channel in groupChannels)
                {
                    var query = _channelQueryBuilder.GetChannelQuery(channel);

                    if (query == null)
                    {
                        continue;
                    }

                    bulkRequests.Add(new NestEsBulkRequest<NestPercolatedQuery>()
                    {
                        DocID = GetChannelDocumentId(channel),
                        Document = query,
                        Index = indexName,
                        Operation = eOperation.index
                    });

                    // If we exceeded maximum size of bulk 
                    if (bulkRequests.Count >= sizeOfBulk)
                    {
                        ExecuteAndValidateBulkRequests(bulkRequests);
                    }
                }

                // If we have anything left that is less than the size of the bulk
                if (bulkRequests.Any())
                {
                    ExecuteAndValidateBulkRequests(bulkRequests);
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error when indexing percolators on partner {_partnerId}", ex);

                return false;
            }
        }


        public void AddChannelsMetadataToIndex(string newIndexName, List<Channel> allChannels)
        {
            var bulkList = new List<NestEsBulkRequest<NestChannelMetadata>>();
            var sizeOfBulk = GetBulkSize(50);
            var cd = new LogContextData();
            var channelMetadatas = allChannels.Select(x => NestDataCreator.GetChannelMetadata(x));
            foreach (var channelMetadata in channelMetadatas)
            {
                var nestEsBulkRequest =
                    new NestEsBulkRequest<NestChannelMetadata>(channelMetadata.ChannelId, newIndexName, channelMetadata);
                bulkList.Add(nestEsBulkRequest);
                // If we exceeded the size of a single bulk request
                if (bulkList.Count >= sizeOfBulk)
                {
                    // Send request to elastic search in a different thread
                    var t = Task.Run(() =>
                    {
                        cd.Load();
                        ExecuteAndValidateBulkRequests(bulkList);
                    });

                    t.Wait();
                    bulkList.Clear();
                }
            }

            // If we have a final bulk pending
            if (bulkList.Count > 0)
            {
                // Send request to elastic search in a different thread
                var t = Task.Run(() =>
                {
                    cd.Load();
                    ExecuteAndValidateBulkRequests(bulkList);
                });
                t.Wait();
            }
        }

        public void PublishChannelsMetadataIndex(string newIndexName, bool shouldSwitchAlias, bool shouldDeleteOldIndices)
        {
            var alias = NamingHelper.GetChannelMetadataIndexName(_partnerId);
            SwitchIndexAlias(newIndexName, alias, shouldDeleteOldIndices, shouldSwitchAlias);
        }

        private string SetupIndex<T>(string newIndexName, List<string> multilingualFields, List<string> simpleFields)
        where T : class
        {
            var languages = GetLanguages();
            GetAnalyzersWithLowercase(languages.ToList(), out var analyzers, out _, out var filters, out var tokenizers, out var normalizers);
            var analyzersDescriptor = GetAnalyzersDesctiptor(analyzers);
            var filtersDescriptor = GetTokenFiltersDescriptor(filters);
            var tokenizersDescriptor = GetTokenizersDesctiptor(tokenizers);
            var normalizersDescriptor = GetNormalizersDescriptor(normalizers);

            var createResponse = _elasticClient.Indices.Create(newIndexName,
                c => c.Settings(settings => settings
                        .NumberOfShards(_numOfShards)
                        .NumberOfReplicas(_numOfReplicas)
                        .Setting("index.max_result_window", _maxResults)
                        .Setting("index.max_ngram_diff", _applicationConfiguration.ElasticSearchHandlerConfiguration.MaxNgramDiff.Value)
                        .Setting("index.max_inner_result_window", _applicationConfiguration.ElasticSearchConfiguration.MaxInnerResultWindow.Value)
                        .Analysis(a => a
                            .Analyzers(an => analyzersDescriptor)
                            .TokenFilters(tf => filtersDescriptor)
                            .Tokenizers(t => tokenizersDescriptor)
                            .Normalizers(n => normalizersDescriptor)
                        ))
                    .Map<T>(map => map
                        .AutoMap<T>()
                        .Properties(properties =>
                            GetPropertiesDescriptor(properties, languages.ToList(), analyzers, multilingualFields, simpleFields)
                        )
                    ));

            var isIndexCreated = createResponse != null && createResponse.Acknowledged && createResponse.IsValid;
            if (!isIndexCreated)
            {
                log.Error($"Failed creating index for partner {_partnerId}, response = {createResponse}");
                return string.Empty;
            }

            return newIndexName;
        }


        public string SetupChannelMetadataIndex(DateTime indexDate)
        {
            return SetupIndex<NestChannelMetadata>(NamingHelper.GetNewChannelMetadataIndexName(_partnerId, indexDate), null, new List<string>() { "name" });
        }

        public string SetupTagsIndex(DateTime indexDate)
        {
            return SetupIndex<NestTag>(NamingHelper.GetNewMetadataIndexName(_partnerId, indexDate), new List<string>() { "value" }, null);
        }

        public void InsertTagsToIndex(string newIndexName, List<TagValue> allTagValues)
        {
            var sizeOfBulk = GetBulkSize(50);
            var bulkRequests = new List<NestEsBulkRequest<NestTag>>();
            try
            {
                foreach (var tagValue in allTagValues)
                {
                    var catalogGroupCache = GetCatalogGroupCache();
                    if (!catalogGroupCache.LanguageMapById.ContainsKey(tagValue.languageId))
                    {
                        log.WarnFormat("Found tag value with non existing language ID. tagId = {0}, tagText = {1}, languageId = {2}",
                            tagValue.tagId, tagValue.value, tagValue.languageId);
                        continue;
                    }

                    var languageCode = catalogGroupCache.LanguageMapById[tagValue.languageId].Code;

                    // Serialize EPG object to string
                    var tag = new NestTag(tagValue, languageCode);
                    var bulkRequest = new NestEsBulkRequest<NestTag>()
                    {
                        DocID = $"{tag.tagId}_{languageCode}",
                        Document = tag,
                        Index = newIndexName,
                        Operation = eOperation.index
                    };

                    bulkRequests.Add(bulkRequest);

                    // If we exceeded maximum size of bulk 
                    if (bulkRequests.Count >= sizeOfBulk)
                    {
                        ExecuteAndValidateBulkRequests(bulkRequests);
                    }
                }

                // If we have anything left that is less than the size of the bulk
                if (bulkRequests.Any())
                {
                    ExecuteAndValidateBulkRequests(bulkRequests);
                }
            }
            finally
            {
                if (bulkRequests.Any())
                {
                    log.Debug($"Clearing bulk requests");
                    bulkRequests.Clear();
                }
            }
        }

        public bool PublishTagsIndex(string newIndexName, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            string alias = NamingHelper.GetMetadataIndexAlias(_partnerId);

            return SwitchIndexAlias(newIndexName, alias, shouldDeleteOldIndices, shouldSwitchIndexAlias);
        }

        public string SetupEpgIndex(DateTime indexDate, bool isRecording)
        {
            var indexName = NamingHelper.GetNewEpgIndexName(_partnerId, indexDate);

            if (isRecording)
            {
                indexName = NamingHelper.GetNewRecordingIndexName(_partnerId, indexDate);
            }

            CreateNewEpgV1Index(indexName, isRecording);
            return indexName;
        }

        public void AddEPGsToIndex(string index,
            bool isRecording,
            Dictionary<ulong, Dictionary<string, EpgCB>> programs,
            Dictionary<long, List<int>> linearChannelsRegionsMapping,
            Dictionary<long, long> epgToRecordingMapping)
        {
            // Basic validation
            if (programs == null || programs.Count == 0)
            {
                log.ErrorFormat($"AddEPGsToIndex {index} for group {_partnerId}: programs is null or empty!");
                return;
            }

            // save current value to restore at the end
            var currentDefaultConnectionLimit = ServicePointManager.DefaultConnectionLimit;
            try
            {
                int numOfBulkRequests = 0;
                var bulkRequests =
                    new Dictionary<int, List<NestEsBulkRequest<NestEpg>>>() { { numOfBulkRequests,
                        new List<NestEsBulkRequest<NestEpg>>() } };

                // GetLinear Channel Values 
                var programsList = programs.SelectMany(x => x.Value.Values).ToList();

                _catalogManager.GetLinearChannelValues(programsList, _partnerId, _ => { });

                // used only to support linear media id search on elastic search
                List<string> epgChannelIds = programsList.Select(item => item.ChannelID.ToString()).ToList<string>();

                var linearChannelSettings = _catalogCache.GetLinearChannelSettings(_partnerId, epgChannelIds);

                // prevent from size of bulk to be more than the default value of 500 (currently as of 23.06.20)
                int sizeOfBulk = GetBulkSize();

                // Run on all programs
                foreach (ulong epgId in programs.Keys)
                {
                    foreach (string languageCode in programs[epgId].Keys)
                    {
                        string suffix = null;

                        LanguageObj language = null;
                        language = GetLanguageByCode(languageCode);
                        // Validate language
                        if (language == null)
                        {
                            log.ErrorFormat("AddEPGsToIndex: Epg {0} has invalid language code {1}", epgId,
                                languageCode);
                            continue;
                        }

                        var epgCb = programs[epgId][languageCode];

                        if (epgCb == null)
                        {
                            continue;
                        }

                        epgCb.PadMetas(GetMetasToPad());

                        // used only to currently support linear media id search on elastic search
                        if (linearChannelSettings.ContainsKey(epgCb.ChannelID.ToString()))
                        {
                            epgCb.LinearMediaId = linearChannelSettings[epgCb.ChannelID.ToString()].LinearMediaId;
                        }

                        if (epgCb.LinearMediaId > 0 && linearChannelsRegionsMapping != null &&
                            linearChannelsRegionsMapping.ContainsKey(epgCb.LinearMediaId))
                        {
                            epgCb.regions = linearChannelsRegionsMapping[epgCb.LinearMediaId];
                        }

                        long? recordingId = null;
                        if (isRecording)
                        {
                            recordingId = epgToRecordingMapping[(int)epgCb.EpgID];
                        }

                        var expiry = GetEpgExpiry(epgCb);
                        var epg = NestDataCreator.GetEpg(epgCb,
                            language.ID,
                            true,
                            IsOpc(),
                            recordingId: recordingId,
                            expiryUnixTimeStamp: expiry);

                        // If we exceeded the size of a single bulk reuquest then create another list
                        if (bulkRequests[numOfBulkRequests].Count >= sizeOfBulk)
                        {
                            numOfBulkRequests++;
                            bulkRequests.Add(numOfBulkRequests, new List<NestEsBulkRequest<NestEpg>>());
                        }
                        var bulkRequest = GetEpgBulkRequest(index, epg);

                        bulkRequests[numOfBulkRequests].Add(bulkRequest);
                    }
                }

                var maxDegreeOfParallelism = GetMaxDegreeOfParallelism();
                var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism };
                var contextData = new LogContextData();
                ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount;
                // Send request to elastic search in a different thread
                Parallel.ForEach(bulkRequests, options, (bulkRequest, state) =>
                {
                    contextData.Load();
                    ExecuteAndValidateBulkRequests(bulkRequest.Value);
                });
            }
            catch (Exception ex)
            {
                log.Error("Failed during AddEPGsToIndex", ex);
            }
            finally
            {
                ServicePointManager.DefaultConnectionLimit = currentDefaultConnectionLimit;
            }

        }

        public bool PublishEpgIndex(string newIndexName, bool isRecording, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            string alias = NamingHelper.GetEpgIndexAlias(_partnerId);

            if (isRecording)
            {
                alias = NamingHelper.GetRecordingIndexAlias(_partnerId);
            }

            return SwitchIndexAlias(newIndexName, alias, shouldDeleteOldIndices, shouldSwitchIndexAlias);
        }

        public bool UpdateEpgs(List<EpgCB> epgObjects, bool isRecording, Dictionary<long, long> epgToRecordingMapping = null)
        {
            var result = false;
            var bulkRequests = new List<NestEsBulkRequest<NestEpg>>();

            var sizeOfBulk = GetBulkSize(500);
            var languages = GetLanguages();

            var linearChannelsRegionsMapping = _regionManager.GetLinearMediaRegions(_partnerId);

            var alias = NamingHelper.GetEpgIndexAlias(_partnerId);

            if (isRecording)
            {
                alias = NamingHelper.GetRecordingIndexAlias(_partnerId);
            }

            if (!_elasticClient.Indices.Exists(alias).Exists)
            {
                log.Error($"Error - Index {alias} for group {_partnerId} does not exist");
                return false;
            }

            var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(_partnerId);
            _catalogManager.GetLinearChannelValues(epgObjects, _partnerId, epg => { Utils.ExtractSuppressedValue(GetCatalogGroupCache(), epg); });

            List<string> epgChannelIds = epgObjects.Select(item => item.ChannelID.ToString()).ToList();
            Dictionary<string, LinearChannelSettings> linearChannelSettings = _catalogCache.GetLinearChannelSettings(_partnerId, epgChannelIds);

            // Create dictionary by languages
            foreach (LanguageObj language in languages)
            {
                // Filter programs to current language
                var currentLanguageEpgs = epgObjects.Where(epg =>
                    epg.Language.ToLower() == language.Code.ToLower() || (language.IsDefault && string.IsNullOrEmpty(epg.Language))).ToList();

                if (currentLanguageEpgs.Any())
                {
                    // Create bulk request object for each program
                    foreach (EpgCB epgCb in currentLanguageEpgs)
                    {
                        // Epg V2 has multiple indices connected to the gloabl alias {groupID}_epg
                        // in that case we need to use the specific date alias for each epg item to update
                        if (!isRecording && epgFeatureVersion == EpgFeatureVersion.V2)
                        {
                            alias = _namingHelper.GetDailyEpgIndexName(_partnerId, epgCb.StartDate.Date);
                        }

                        epgCb.PadMetas(GetMetasToPad());
                        UpdateEpgLinearMediaId(linearChannelSettings, epgCb);
                        UpdateEpgRegions(epgCb, linearChannelsRegionsMapping);

                        long? recodingId = null;
                        if (isRecording && epgToRecordingMapping != null)
                        {
                            recodingId = epgToRecordingMapping[(int)epgCb.EpgID];
                        }

                        var shouldSetTtl = !isRecording;
                        long? expiry = null;
                        if (shouldSetTtl)
                        {
                            expiry = GetEpgExpiry(epgCb);
                        }

                        var epg = NestDataCreator.GetEpg(epgCb,
                            language,
                            true,
                            IsOpc(),
                            expiry,
                            recodingId);

                        var bulkRequest = GetEpgBulkRequest(alias, epg);
                        bulkRequests.Add(bulkRequest);

                        if (bulkRequests.Count > sizeOfBulk)
                        {
                            var isValid = ExecuteAndValidateBulkRequests(bulkRequests);
                            result &= isValid;
                            bulkRequests.Clear();
                        }
                    }
                }
            }

            if (bulkRequests.Count > 0)
            {
                var isValid = ExecuteAndValidateBulkRequests(bulkRequests);
                result &= isValid;
                bulkRequests.Clear();
            }

            return result;
        }

        public SearchResultsObj SearchMedias(MediaSearchObj search, int langId, bool useStartDate)
        {
            search.m_nGroupId = search.m_nGroupId == 0 ? _partnerId : search.m_nGroupId;
            search.m_oLangauge = search.m_oLangauge ?? GetDefaultLanguage();
            var mediaBuilder = new UnifiedSearchNestMediaBuilder()
            {
                Definitions = search,
                QueryType = search.m_bExact ? eQueryType.EXACT : eQueryType.BOOLEAN,
                IncludeRegionTerms = true,
                UseMustWhenBooleanQuery = true
            };

            //call the search
            var searchResponse = _elasticClient.Search<NestMedia>(searchDescriptor =>
                {
                    searchDescriptor.Query(q => mediaBuilder.GetQuery());
                    searchDescriptor.Index(Indices.Index(mediaBuilder.GetIndices()));
                    searchDescriptor.Sort(q => mediaBuilder.GetSort());
                    searchDescriptor.TrackTotalHits();
                    searchDescriptor = mediaBuilder.SetSizeAndFrom(searchDescriptor);

                    searchDescriptor.Source(
                        s => s.Includes(f =>
                            f.Fields(
                                f1 => f1.MediaId,
                                f1 => f1.UpdateDate,
                                f1 => f1.NamesDictionary,
                                f1 => f1.MediaTypeId,
                                f1 => f1.DocumentId
                            )
                        )
                    );

                    return searchDescriptor;
                }
            );

            //build data result
            var result = new SearchResultsObj();
            if (!searchResponse.IsValid || !searchResponse.Hits.Any())
            {
                return result;
            }

            var assetDocuments = searchResponse.Hits.Select(responseHit =>
                    new SearchResult()
                {
                    assetID = responseHit.Source.MediaId,
                    UpdateDate = responseHit.Source.UpdateDate
                }
            ).ToList();

            result.n_TotalItems = (int)searchResponse.Total;
            result.m_resultIDs = assetDocuments;
            
            if (!ShouldOrderMediaSearchData(search))
            {
                return result;
            }

            return OrderMediaResults(search, result, searchResponse);
        }

        private bool ShouldOrderMediaSearchData(MediaSearchObj search)
        {
            var sortByStartDateOfAssociationTagsAndParentMedia =
                search.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.START_DATE) &&
                search.associationTags?.Count > 0 &&
                search.parentMediaTypes?.Count > 0;

            var orderByViews = search.m_oOrder.m_eOrderBy <= ApiObjects.SearchObjects.OrderBy.VIEWS;
            var orderByLikes = search.m_oOrder.m_eOrderBy >= ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER;
            var orderByVotes = search.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.VOTES_COUNT);
            var isOrderData = orderByViews && orderByLikes || orderByVotes ||
                              sortByStartDateOfAssociationTagsAndParentMedia;
            return isOrderData;
        }

        private SearchResultsObj OrderMediaResults(MediaSearchObj definitions,
            SearchResultsObj result,
            ISearchResponse<NestMedia> searchResponse)
        {
            var mediaIds = result.m_resultIDs.Select(item => item.assetID).ToList();
            if (definitions.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.START_DATE))
            {
                var extendedUnifiedSearchResults = searchResponse.Hits.Select(x =>
                {
                    var unifiedSearchResult = new UnifiedSearchResult
                    {
                        AssetId = x.Source.MediaId.ToString()
                    };
                    return new ExtendedUnifiedSearchResult(unifiedSearchResult, x);
                });
                mediaIds = _startDateAssociationTagsSortStrategy.SortAssetsByStartDate(
                        extendedUnifiedSearchResults,
                        definitions.m_oLangauge,
                        definitions.m_oOrder.m_eOrderDir,
                        definitions.associationTags,
                        definitions.parentMediaTypes,
                        _partnerId)
                    .Select(x => (int)x.id)
                    .ToList();
            }
            else
            {
                Utils.OrderMediasByStats(mediaIds, (int)definitions.m_oOrder.m_eOrderBy,
                    (int)definitions.m_oOrder.m_eOrderDir);
            }

            var dItems = result.m_resultIDs.ToDictionary(item => item.assetID);
            result.m_resultIDs.Clear();

            // check which results should be returned
            if (definitions.m_nPageSize <= 0 || definitions.m_nPageIndex < 0)
            {
                return result;
            }

            if (definitions.m_nPageSize > 0 || definitions.m_nPageIndex >= 0)
            {
                // apply paging on results 
                mediaIds = mediaIds.Skip(definitions.m_nPageSize * definitions.m_nPageIndex).Take(definitions.m_nPageSize).ToList();
            }

            SearchResult searchResult;
            foreach (var mediaId in mediaIds)
            {
                var tryGetValue = dItems.TryGetValue(mediaId, out searchResult);
                if (tryGetValue)
                {
                    result.m_resultIDs.Add(searchResult);
                }
            }

            return result;
        }

        public SearchResultsObj SearchSubscriptionMedias(List<MediaSearchObj> oSearch,
            int nLangID,
            bool bUseStartDate,
            string sMediaTypes,
            OrderObj orderObj,
            int nPageIndex,
            int nPageSize)
        {
            var mediasResult = new SearchResultsObj();
            if (oSearch == null || !oSearch.Any())
            {
                return mediasResult;
            }

            int nTotalItems = 0;
            var unifiedSearchNestMediaBuilder = new UnifiedSearchNestMediaBuilder();
            var shouldContainer = new List<QueryContainer>();

            foreach (var searchObj in oSearch)
            {
                if (searchObj == null)
                    continue;
                searchObj.m_nPageSize = 0;
                searchObj.m_oLangauge = searchObj.m_oLangauge ?? GetDefaultLanguage();
                unifiedSearchNestMediaBuilder.Definitions = searchObj;
                unifiedSearchNestMediaBuilder.QueryType = searchObj.m_bExact ? eQueryType.EXACT : eQueryType.BOOLEAN;
                shouldContainer.Add(unifiedSearchNestMediaBuilder.GetQuery());
            }

            unifiedSearchNestMediaBuilder.Definitions.m_oOrder = orderObj;
            unifiedSearchNestMediaBuilder.Definitions.m_nPageIndex = nPageIndex;
            unifiedSearchNestMediaBuilder.Definitions.m_nPageSize = nPageSize;
            var sortDescriptor = unifiedSearchNestMediaBuilder.GetSort();
            var searchResponse = _elasticClient.Search<NestMedia>(s =>
            {
                s.Sort(sort => unifiedSearchNestMediaBuilder.GetSort());
                s.Query(q => q.Bool(b => b.Should(shouldContainer.ToArray())));
                s = unifiedSearchNestMediaBuilder.SetSizeAndFrom<NestMedia>(s);
                s.Index(NamingHelper.GetMediaIndexAlias(_partnerId));
                s.TrackTotalHits();
                return s;
            });
            
            //build data result

            if (!searchResponse.Hits.Any())
            {
                return mediasResult;
            }

            mediasResult.m_resultIDs = searchResponse.Hits.Select(responseHit =>
            {
                var nestMedia = responseHit.Source;
                return new SearchResult()
                {
                    assetID = nestMedia.MediaId,
                    UpdateDate = nestMedia.UpdateDate
                };
            }).ToList();

            mediasResult.n_TotalItems = searchResponse.Hits.Count;

            var shouldSort = orderObj.m_eOrderBy <= OrderBy.VIEWS &&
                              orderObj.m_eOrderBy >= ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER ||
                              orderObj.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.VOTES_COUNT);

            if (!shouldSort)
            {
                return mediasResult;
            }

            var mediaIds = mediasResult.m_resultIDs.Select(item => item.assetID).ToList();
            Utils.OrderMediasByStats(mediaIds, (int)orderObj.m_eOrderBy, (int)orderObj.m_eOrderDir);

            var mediaItemsIds = mediasResult.m_resultIDs.ToDictionary(item => item.assetID);
            var sortedMedias = new SearchResultsObj();
            sortedMedias.m_resultIDs = new List<SearchResult>();

            foreach (var mediaId in mediaIds)
            {
                if (mediaItemsIds.TryGetValue(mediaId, out SearchResult searchResult))
                {
                    sortedMedias.m_resultIDs.Add(searchResult);
                }
            }

            sortedMedias.n_TotalItems = searchResponse.Hits.Count;
            return sortedMedias;
        }

        public SearchResultsObj SearchEpgs(EpgSearchObj epgSearch)
        {
            if (epgSearch == null || epgSearch.m_nGroupID == 0)
            {
                log.Debug("Info - SearchEpgs return null due to epgSearch == null || epgSearch.m_nGroupID==0 ");
                return null;
            }

            try
            {
                DateTime startDate = epgSearch.m_dStartDate;
                DateTime endDate = epgSearch.m_dEndDate;

                epgSearch.m_bSearchEndDate = ConditionalAccess.Utils.GetIsTimeShiftedTvPartnerSettingsExists(_partnerId);
                var unifiedSearchNestEpgBuilder = new UnifiedSearchNestEpgBuilder() { Definitions = epgSearch };

                var routing = new List<string>();
                var currentDate = epgSearch.m_dStartDate.AddDays(-1).Date;
                while (currentDate <= epgSearch.m_dEndDate)
                {
                    routing.Add(currentDate.ToString(ESUtils.ES_DATEONLY_FORMAT));
                    currentDate = currentDate.AddDays(1);
                }

                var searchResponse = _elasticClient.Search<NestEpg>(s =>
                    {
                        s.Index(Indices.Index(NamingHelper.GetEpgIndexAlias(epgSearch.m_nGroupID)));
                        s.Routing(string.Join(",", routing));
                        s.Query(q => unifiedSearchNestEpgBuilder.GetQuery());
                        s.TrackTotalHits();
                        return s;
                    }
                );

                if (!searchResponse.IsValid || searchResponse.Total == 0)
                {
                    return null;
                }

                var searchResults = searchResponse.Hits.Select(x => new SearchResult
                {
                    assetID = (int)x.Source.EpgID,
                    UpdateDate = x.Source.UpdateDate
                }).ToList();

                return new SearchResultsObj()
                {
                    m_resultIDs = searchResults,
                    n_TotalItems = (int)searchResponse.Total
                };
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("SearchEpgs ex={0} st: {1}", ex.Message, ex.StackTrace), ex);
            }

            return null;
        }

        public List<string> GetAutoCompleteList(MediaSearchObj mediaSearch, int nLangID, ref int nTotalItems)
        {
            mediaSearch.m_dOr.Add(new SearchValue()
            {
                m_lValue = new List<string>() {""},
                m_sKey = "name^3"
            });

            mediaSearch.m_nGroupId = _partnerId;
            mediaSearch.m_oLangauge = mediaSearch.m_oLangauge ?? GetDefaultLanguage();
            
            var nestMediaBuilder = new UnifiedSearchNestMediaBuilder()
            {
                Definitions = mediaSearch,
                QueryType = eQueryType.PHRASE_PREFIX
            };
            
            var searchResponse = _elasticClient.Search<NestMedia>(x =>
                {
                    var searchDescriptor = x
                        .Index(Indices.Index(nestMediaBuilder.GetIndices()))
                        .Fields(f => f.Field(f1 => f1.Name))
                        .Source(s => s.Includes(i => i.Field(f2 => f2.Name)))
                        .Sort(s => nestMediaBuilder.GetSort())
                        .Query(q =>
                        {
                            var nestMediaQueries = new NestMediaQueries();
                            var nestBaseQueries = new NestBaseQueries();
                            var qc = new List<QueryContainer>();

                            var dateRangesTerms = nestMediaQueries.GetMediaDateRangesTerms(mediaSearch);
                            if (dateRangesTerms != null)
                            {
                                qc.Add(dateRangesTerms);
                            }

                            var mediaTypeTerms = nestMediaQueries.GetMediaTypeTerms(mediaSearch);
                            if (mediaTypeTerms != null)
                            {
                                qc.Add(mediaTypeTerms);
                            }

                            var mediaRegionTerms = nestMediaQueries.GetMediaRegionTerms(mediaSearch);
                            if (mediaRegionTerms)
                            {
                                qc.Add(mediaRegionTerms);
                            }

                            var multiMatch = nestMediaQueries.GetSearchValueOrMultiMatch(mediaSearch);
                            if (multiMatch != null)
                            {
                                qc.Add(multiMatch);
                            }

                            var isActiveTerm = nestBaseQueries.GetIsActiveTerm();
                            qc.Add(isActiveTerm);
                            return q.Bool(b => b.Must(qc.ToArray()));
                        });

                    searchDescriptor = nestMediaBuilder.SetSizeAndFrom(searchDescriptor);
                    return searchDescriptor;
                }
            );

            var searchResponseHits = searchResponse.Hits;
            if (!searchResponseHits.Any())
                return new List<string>();
            
            return searchResponseHits.Select(x => x.Source.Name).ToList();
        }


        /// <summary>
        /// Update EPGs partially.
        /// PadMetas aren't allowed during partial updates - be aware!
        /// Fields responsible for TTL Calculation aren't allowed as well!
        /// </summary>
        /// <param name="languages"></param>
        /// <param name="epgIds"></param>
        /// <param name="fieldMappings"></param>
        /// <returns></returns>
        public bool UpdateEpgsPartial(EpgPartialUpdate[] epgs)
        {
            bool result = false;

            var bulkRequests = new List<NestEsBulkRequest<NestEpgPartial>>();
            int sizeOfBulk = GetBulkSize();

            // Temporarily - assume success
            bool temporaryResult = true;
            List<LanguageObj> languages = IsOpc() ? GetCatalogGroupCache().LanguageMapById.Values.ToList() : GetGroupManager().GetLangauges();

            // Epg V2 has multiple indices connected to the gloabl alias {groupID}_epg
            // in that case we need to use the specific date alias for each epg item to update
            var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(_partnerId);

            var alias = NamingHelper.GetEpgIndexAlias(_partnerId);
            if (!_elasticClient.Indices.Exists(alias).Exists)
            {
                log.Error($"Error - EPG index for group {_partnerId} does not exist");
                return result;
            }

            // Create dictionary by languages
            foreach (LanguageObj language in languages)
            {
                // Filter programs to current language
                EpgPartialUpdate[] currentLanguageEpgs = epgs.Where(epg =>
                    epg.Language.ToLower() == language.Code.ToLower() || (language.IsDefault && string.IsNullOrEmpty(epg.Language))).ToArray();

                if (currentLanguageEpgs != null && currentLanguageEpgs.Length > 0)
                {
                    // Create bulk request object for each program
                    foreach (EpgPartialUpdate epg in currentLanguageEpgs)
                    {
                        // Epg V2 has multiple indices connected to the gloabl alias {groupID}_epg
                        // in that case we need to use the specific date alias for each epg item to update
                        if (epgFeatureVersion == EpgFeatureVersion.V2)
                        {
                            alias = _namingHelper.GetDailyEpgIndexName(_partnerId, epg.StartDate.Date);
                        }

                        string suffix = language.Code;

                        NestEpgPartial nestEpg = new NestEpgPartial()
                        {
                            Regions = epg.EpgPartial.Regions?.ToList()
                        };

                        var documentId = $"{epg.EpgId}_{suffix}";
                        bulkRequests.Add(new NestEsBulkRequest<NestEpgPartial>(documentId, alias, nestEpg)
                        {
                            Operation = eOperation.update,
                            Routing = epg.StartDate.ToString(ESUtils.ES_DATEONLY_FORMAT)
                        });
                            
                        if (bulkRequests.Count > sizeOfBulk)
                        {
                            var isValid = ExecuteAndValidateBulkRequests(bulkRequests);

                            if (!isValid)
                            {
                                result = false;
                                temporaryResult = false;
                            }
                            else
                            {
                                temporaryResult &= true;
                            }
                        }
                    }
                }
            }

            if (bulkRequests.Count > 0)
            {
                var isValid = ExecuteAndValidateBulkRequests(bulkRequests);

                if (!isValid)
                {
                    result = false;
                    temporaryResult = false;
                }
                else
                {
                    temporaryResult &= true;
                }

                result = temporaryResult;
            }

            return result;
        }



        public IList<EpgProgramInfo> GetCurrentProgramInfosByDate(int channelId, DateTime fromDate, DateTime toDate)
        {
            log.Debug($"GetCurrentProgramsByDate > fromDate:[{fromDate}], toDate:[{toDate}]");
            var result = new List<EpgProgramInfo>();
            var index = NamingHelper.GetEpgIndexAlias(_partnerId);

            log.Debug($"GetCurrentProgramsByDate > index alias:[{index}] found, searching current programs, minStartDate:[{fromDate}], maxEndDate:[{toDate}]");
            var boolMusts = new List<QueryContainer>();
            var descriptor = new QueryContainerDescriptor<NestEpg>();
            // Program end date > minimum start date
            // program start date < maximum end date
            var minimumRange = descriptor.DateRange(selector => selector.Field(field => field.EndDate).GreaterThanOrEquals(fromDate));
            var maximumRange = descriptor.DateRange(selector => selector.Field(field => field.StartDate).LessThanOrEquals(toDate));
            var channelFilter = descriptor.Term(field => field.ChannelID, channelId);

            boolMusts.Add(minimumRange);
            boolMusts.Add(maximumRange);
            boolMusts.Add(channelFilter);

            var boolQuery = new BoolQuery()
            {
                Must = boolMusts
            };

            // get the epg document ids from elasticsearch
            var searchResult = _elasticClient.Search<NestEpg>(selector => selector
                .Index(index)
                .Query(q => boolQuery)
                .Source(source => source.Includes(fields => fields.Fields(f => f.EpgIdentifier, f => f.CouchbaseDocumentId, f => f.IsAutoFill, f => f.Language)))
            );

            // get the programs - epg ids from elasticsearch, information from EPG DAL
            if (searchResult.IsValid)
            {
                foreach (var hit in searchResult.Hits)
                {
                    var epgItem = new EpgProgramInfo();
                    epgItem.LanguageCode = hit.Source.Language;
                    epgItem.EpgExternalId = hit.Source.EpgIdentifier;
                    epgItem.DocumentId = hit.Source.CouchbaseDocumentId;
                    epgItem.IsAutofill = hit.Source.IsAutoFill;
                    epgItem.GroupId = _partnerId;
                    result.Add(epgItem);
                }
            }

            return result;
        }

        #region Private Methods

        private List<LanguageObj> GetLanguages()
        {
            return IsOpc() ? GetCatalogGroupCache().LanguageMapById.Values.ToList(): GetGroupManager().GetLangauges();
        }

        private LanguageObj GetDefaultLanguage()
        {
            return IsOpc() ? GetCatalogGroupCache().GetDefaultLanguage() : GetGroupManager().GetGroupDefaultLanguage();
        }

        private LanguageObj GetLanguageByCode(string languageCode)
        {
            return IsOpc()
                ? GetCatalogGroupCache().LanguageMapByCode[languageCode]
                : GetGroupManager().GetLanguage(languageCode);
        }

        private int GetLanguageIdByCode(string languageCode)
        {
            return GetLanguageByCode(languageCode).ID;
        }

        private LanguageObj GetLanguageById(int id)
        {
            return GetLanguages().FirstOrDefault(x => x.ID == id);
        }

        private void EnsureEpgIndexExist(string dailyEpgIndexName, EpgFeatureVersion epgFeatureVersion, int? numberOfShards = null)
        {
            // TODO it's possible to create new index with mappings and alias in one request,
            // https://www.elastic.co/guide/en/elasticsearch/reference/2.3/indices-create-index.html#mappings
            // but we have huge mappings and don't know the impact on timeouts during index creation - should be tested on real environment.

            // Limitation: it's not a transaction, we don't remove index when add-mapping failed =>
            // EPGs could be added to the index without mapping (e.g. from asset.add)
            try
            {
                AddEmptyIndex(dailyEpgIndexName, epgFeatureVersion, numberOfShards);
                AddEpgIndexAlias(dailyEpgIndexName);
            }
            catch (Exception e)
            {
                log.Error($"index creation failed [{dailyEpgIndexName}]", e);
                throw new Exception($"index creation failed");
            }
        }

        private void SetNoRefresh(string dailyEpgIndexName)
        {
            // shut down refresh of index while bulk uploading
            IndexManagerCommonHelpers.GetRetryPolicy<Exception>().Execute(() =>
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

        private void SetIndexRefreshToDefault(string index)
        {
            var response = _elasticClient.Indices.UpdateSettings(Indices.Index(index),
                x => x.IndexSettings(y => y.RefreshInterval(INDEX_REFRESH_INTERVAL)));

            if (!response.IsValid)
            {
                IndexManagerCommonHelpers.GetRetryPolicy<Exception>().Execute(() =>
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

        private void AddEmptyIndex(string indexName, EpgFeatureVersion epgFeatureVersion, int? numOfShards = null)
        {
            IndexManagerCommonHelpers.GetRetryPolicy<Exception>().Execute(() =>
            {
                var isIndexExist = _elasticClient.Indices.Exists(indexName);

                if (isIndexExist != null && isIndexExist.Exists)
                {
                    return;
                }

                log.Info($"creating new index [{indexName}]");
                CreateEmptyEpgIndex(indexName, epgFeatureVersion, true, true, REFRESH_INTERVAL_FOR_EMPTY_INDEX_SECONDS, numOfShards: numOfShards);
            });
        }

        private void CreateEmptyEpgIndex(string newIndexName, EpgFeatureVersion epgFeatureVersion, bool shouldBuildWithReplicas = true,
            bool shouldUseNumOfConfiguredShards = true, int? refreshIntervalSeconds = null, int? numOfShards = null)
        {
            List<LanguageObj> languages = GetLanguages();
            GetAnalyzersWithLowercaseAndPhraseStartsWith(languages, out var analyzers, out var customProperties, out var filters, out var tokenizers, out var normalizers);
            int replicas = shouldBuildWithReplicas ? _numOfReplicas : 0;
            
            // use TCM global sharding configuration or hardcoded default
            int shards = shouldUseNumOfConfiguredShards ? _numOfShards : 1;
            
            // use sent num of shards or the already computed value
            shards = numOfShards ?? shards;
            var isIndexCreated = false;
            AnalyzersDescriptor analyzersDescriptor = GetAnalyzersDesctiptor(analyzers);
            TokenFiltersDescriptor filtersDesctiptor = GetTokenFiltersDescriptor(filters);
            TokenizersDescriptor tokenizersDescriptor = GetTokenizersDesctiptor(tokenizers);
            NormalizersDescriptor normalizersDescriptor = GetNormalizersDescriptor(normalizers);

            _groupManager.RemoveGroup(_partnerId); // remove from cache
            
            if (!GetMetasAndTagsForMapping(
                out var metas,
                out var tags,
                out var metasToPad,
                true))
            {
                throw new Exception($"failed to get metas and tags");
            }

            var propertiesDescriptor = GetEpgPropertiesDescriptor(languages, metas, tags, metasToPad, analyzers, epgFeatureVersion, customProperties);
            var createResponse = _elasticClient.Indices.Create(newIndexName,
                c => c
                    .Settings(settings =>
                    {
                        settings
                        .NumberOfShards(shards)
                        .NumberOfReplicas(replicas)
                        .Setting("index.max_result_window", _maxResults)
                        .Setting("index.max_ngram_diff", _applicationConfiguration.ElasticSearchHandlerConfiguration.MaxNgramDiff.Value)
                        .Setting("index.mapping.total_fields.limit", _applicationConfiguration.ElasticSearchHandlerConfiguration.TotalFieldsLimit.Value)
                        .Setting("index.max_inner_result_window", _applicationConfiguration.ElasticSearchConfiguration.MaxInnerResultWindow.Value)
                        .Analysis(a => a
                            .Analyzers(an => analyzersDescriptor)
                            .TokenFilters(tf => filtersDesctiptor)
                            .Tokenizers(t => tokenizersDescriptor)
                            .Normalizers(n => normalizersDescriptor)
                        );

                        if (refreshIntervalSeconds.HasValue)
                        {
                            settings.RefreshInterval(new Time(refreshIntervalSeconds.Value, TimeUnit.Second));
                        }

                        return settings;
                    })
                    .Map<NestEpg>(x => x.AutoMap<NestEpg>().AutoMap<NestBaseAsset>())
                    .Map(map => map.RoutingField(rf => new RoutingField() { Required = false }).Properties(props => propertiesDescriptor)
                )); ;

            isIndexCreated = createResponse != null && createResponse.Acknowledged && createResponse.IsValid;
            if (!isIndexCreated)
            {
                log.Error($"Failed creating index, debug information: {createResponse.DebugInformation}");
                throw new Exception(string.Format("Failed creating index for index:{0}", newIndexName));
            }

            //TODO: if epgFeatureVersion == v3 add required mapping..

        }

        private PropertiesDescriptor<NestEpg> GetEpgPropertiesDescriptor(
            List<LanguageObj> languages,
            Dictionary<string, KeyValuePair<eESFieldType, string>> metas,
            List<string> tags,
            HashSet<string> metasToPad,
            Dictionary<string, Analyzer> analyzers,
            EpgFeatureVersion epgFeatureVersion,
            Dictionary<string, CustomProperty> customProperties)
        {
            var propertiesDescriptor = new PropertiesDescriptor<NestEpg>();

            propertiesDescriptor
                .Keyword(x => x.Name(e => e.DocumentId))
                .Number(x => x.Name("epg_id").Type(NumberType.Long))
                .Number(x => x.Name("group_id").Type(NumberType.Integer))
                .Number(x => x.Name("epg_channel_id").Type(NumberType.Integer))
                .Number(x => x.Name("linear_media_id").Type(NumberType.Long))
                .Number(x => x.Name("wp_type_id").Type(NumberType.Integer).NullValue(0))
                .Boolean(x => x.Name("is_active"))
                .Number(x => x.Name("user_types").Type(NumberType.Integer))
                .Text(x => x.Name("date_routing"))
                .Number(x => x.Name("media_type_id").Type(NumberType.Integer).NullValue(0))
                .Number(x => x.Name("language_id").Type(NumberType.Long))
                .Keyword(x => InitializeDefaultTextPropertyDescriptor<string>(NamingHelper.EPG_IDENTIFIER))
                .Date(x => x.Name("start_date"))
                .Date(x => x.Name("end_date"))
                .Date(x => x.Name("cache_date"))
                .Date(x => x.Name("create_date"))
                .Keyword(x => x.Name(n => n.DocumentTransactionalStatus))
                .Keyword(x => InitializeDefaultTextPropertyDescriptor<string>("crid"))
                .Keyword(x => InitializeDefaultTextPropertyDescriptor<string>("external_id"))
                .Keyword(x => x.Name(e => e.CouchbaseDocumentId));

            AddLanguageSpecificMappingToPropertyDescriptor(languages, metas, tags, metasToPad, analyzers, customProperties, propertiesDescriptor);

            // for epg v3 we need to set up the require parent child relation for transactions
            if (epgFeatureVersion == EpgFeatureVersion.V3)
            {
                propertiesDescriptor.Join(j => j
                    .Name(p => p.Transaction)
                    .Relations(r => r.Join<NESTEpgTransaction, NestEpg>())
                );
            }

            return propertiesDescriptor;
        }

        private static void InitializeNumericMetaField<K>(
            PropertiesDescriptor<K> propertiesDescriptor,
            string metaName,
            bool shouldAddPaddedField,
            string searchAnalyzer,
            string indexAnalyzer,
            string lowercaseAnalyzer,
            string phraseStartsWithAnalyzer,
            string phraseStartsWithSearchAnalyzer,
            string autocompleteAnalyzer,
            string autocompleteSearchAnalyzer) where K : class
        {
            var lowercaseSubField = new TextPropertyDescriptor<object>()
                .Name("lowercase")
                .Analyzer(lowercaseAnalyzer)
                .SearchAnalyzer(lowercaseAnalyzer);

            var analyzedSubField = new TextPropertyDescriptor<object>()
                .Name("analyzed")
                .Analyzer(indexAnalyzer)
                .SearchAnalyzer(searchAnalyzer);

            var phraseAutocompleteSubField = new TextPropertyDescriptor<object>()
                .Name("phrase_autocomplete")
                .Analyzer(phraseStartsWithAnalyzer)
                .SearchAnalyzer(phraseStartsWithSearchAnalyzer);
            
            var autocompleteSubField = new TextPropertyDescriptor<object>()
                .Name("autocomplete")
                .Analyzer(autocompleteAnalyzer)
                .SearchAnalyzer(autocompleteSearchAnalyzer);

            PropertiesDescriptor<object> fieldsPropertiesDesctiptor = new PropertiesDescriptor<object>()
                .Number(y => y.Name(metaName).Type(NumberType.Double))
                        .Text(y => lowercaseSubField)
                        .Text(y => phraseAutocompleteSubField)
                        .Text(y => analyzedSubField)
                        .Text(y => autocompleteSubField)
                ;

            if (shouldAddPaddedField)
            {
                var padded = new TextPropertyDescriptor<object>()
                    .Name($"padded_{metaName}")
                    .Analyzer(lowercaseAnalyzer)
                    .SearchAnalyzer(lowercaseAnalyzer);
                fieldsPropertiesDesctiptor.Text(y => padded);
            }

            NumberPropertyDescriptor<object> numberPropertyDescriptor = new NumberPropertyDescriptor<object>()
                .Name(metaName)
                .Type(NumberType.Double)
                .Fields(fields => fieldsPropertiesDesctiptor)
                ;
            propertiesDescriptor.Number(x => numberPropertyDescriptor);
        }

        private KeywordPropertyDescriptor<K> InitializeTextField<K>(
            string nameFieldName,
            PropertiesDescriptor<K> propertiesDescriptor,
            string indexAnalyzer,
            string searchAnalyzer,
            string autocompleteAnalyzer,
            string autocompleteSearchAnalyzer,
            string lowercaseAnalyzer,
            string phraseStartsWithAnalyzer,
            string phraseStartsWithSearchAnalyzer,
            string phoneticIndexAnalyzer,
            string phoneticSearchAnalyzer,
            SortProperty sortProperty,
            bool shouldAddPaddedField = false) where K : class
        {
            var lowercaseSubField = new TextPropertyDescriptor<object>()
                .Name("lowercase")
                .Analyzer(lowercaseAnalyzer)
                .SearchAnalyzer(lowercaseAnalyzer)
                ;

            var autocompleteSubField = new TextPropertyDescriptor<object>()
                .Name("autocomplete")
                .Analyzer(autocompleteAnalyzer)
                .SearchAnalyzer(autocompleteSearchAnalyzer);

            var analyzedField = new TextPropertyDescriptor<object>()
                .Name("analyzed")
                .Analyzer(indexAnalyzer)
                .SearchAnalyzer(searchAnalyzer);

            // this will be something like this: {name}.{name} - keyword
            // meaning, no analyzing at all
            // good for sorts and all that
            PropertiesDescriptor<object> fieldsPropertiesDesctiptor = new PropertiesDescriptor<object>()
                .Keyword(y => y
                    .Name(nameFieldName)
                    .Normalizer(DEFAULT_LOWERCASE_ANALYZER))
                .Text(y => lowercaseSubField)
                .Text(y => autocompleteSubField)
                .Text(y => analyzedField);

            if (!string.IsNullOrEmpty(phraseStartsWithAnalyzer) && !string.IsNullOrEmpty(phraseStartsWithSearchAnalyzer))
            {
                var phraseAutocompleteSubField = new TextPropertyDescriptor<object>()
                    .Name("phrase_autocomplete")
                    .Analyzer(phraseStartsWithAnalyzer)
                    .SearchAnalyzer(phraseStartsWithSearchAnalyzer);

                fieldsPropertiesDesctiptor
                    .Text(y => phraseAutocompleteSubField);
            }

            if (!string.IsNullOrEmpty(phoneticIndexAnalyzer) && !string.IsNullOrEmpty(phoneticSearchAnalyzer))
            {
                var phoneticField = new TextPropertyDescriptor<object>()
                    .Name("phonetic")
                    .Analyzer(phoneticIndexAnalyzer)
                    .SearchAnalyzer(phoneticSearchAnalyzer);
                fieldsPropertiesDesctiptor.Text(y => phoneticField);
            }

            if (sortProperty != null)
            {
                var icuCollationKeywordProperty = new IcuCollationKeywordProperty("sort")
                {
                    Index = sortProperty.Index,
                    Language = sortProperty.Language,
                    Country = sortProperty.Country,
                    Variant = sortProperty.Variant,
                    Type = sortProperty.Type
                };
                fieldsPropertiesDesctiptor.Custom(icuCollationKeywordProperty);
            }

            if (shouldAddPaddedField)
            {
                var padded = new TextPropertyDescriptor<object>()
                    .Name($"padded_{nameFieldName}")
                    .Analyzer(lowercaseAnalyzer)
                    .SearchAnalyzer(lowercaseAnalyzer);
                fieldsPropertiesDesctiptor.Text(y => padded);
            }
            var keywordPropertyDescriptor = new KeywordPropertyDescriptor<K>()
                .Name(nameFieldName)
                .Normalizer(DEFAULT_LOWERCASE_ANALYZER)
                .Fields(fields => fieldsPropertiesDesctiptor);
            propertiesDescriptor.Keyword(x => keywordPropertyDescriptor);

            return keywordPropertyDescriptor;
        }

        private KeywordPropertyDescriptor<K> InitializeDefaultTextPropertyDescriptor<K>(string fieldName)
            where K : class
        {
            var lowercaseSubField = new TextPropertyDescriptor<object>()
                .Name("lowercase")
                .Analyzer(DEFAULT_LOWERCASE_ANALYZER)
                .SearchAnalyzer(DEFAULT_LOWERCASE_ANALYZER);

            var phraseAutocompleteSubField = new TextPropertyDescriptor<object>()
                .Name("phrase_autocomplete")
                .Analyzer(DEFAULT_PHRASE_STARTS_WITH_ANALYZER)
                .SearchAnalyzer(DEFAULT_PHRASE_STARTS_WITH_SEARCH_ANALYZER);

            var autocompleteSubField = new TextPropertyDescriptor<object>()
                .Name("autocomplete")
                .Analyzer(DEFAULT_AUTOCOMPLETE_ANALYZER)
                .SearchAnalyzer(DEFAULT_AUTOCOMPLETE_SEARCH_ANALYZER);

            var analyzedField = new TextPropertyDescriptor<object>()
                .Name("analyzed")
                .Analyzer(DEFAULT_INDEX_ANALYZER)
                .SearchAnalyzer(DEFAULT_SEARCH_ANALYZER);

            return new KeywordPropertyDescriptor<K>()
                .Name(fieldName)
                .Fields(fields => fields
                    .Text(y => y
                        .Name(fieldName)
                        .SearchAnalyzer(DEFAULT_LOWERCASE_ANALYZER)
                        .Analyzer(DEFAULT_LOWERCASE_ANALYZER)
                        .Fielddata(true)
                    )
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
                        SetNgramTokenFiltersDescriptor(filter.Key, filter.Value as NgramFilter, filtersDesctiptor);
                        break;
                    case "edgeNGram":
                        SetEdgeNgramTokenFiltersDescriptor(filter.Key, filter.Value as NgramFilter, filtersDesctiptor);
                        break;
                    case "stemmer":
                        SetStemmerTokenFiltersDescriptor(filter.Key, filter.Value as StemmerFilter, filtersDesctiptor);
                        break;
                    case "phonetic":
                        SetPhoneticTokenFiltersDescriptor(filter.Key, filter.Value as PhoneticFilter, filtersDesctiptor);
                        break;
                    case "elision":
                        SetElisionTokenFiltersDescriptor(filter.Key, filter.Value as ElisionFilter, filtersDesctiptor);
                        break;
                }
            }

            return filtersDesctiptor;
        }

        private void SetNgramTokenFiltersDescriptor(string name, NgramFilter filter, TokenFiltersDescriptor descriptor)
        {
            descriptor.NGram(name, f => f
                .MinGram(filter.min_gram)
                .MaxGram(filter.max_gram));
        }

        private void SetEdgeNgramTokenFiltersDescriptor(string name, NgramFilter filter, TokenFiltersDescriptor descriptor)
        {
            descriptor.EdgeNGram(name, f => f
                .MinGram(filter.min_gram)
                .MaxGram(filter.max_gram));
        }

        private void SetStemmerTokenFiltersDescriptor(string name, StemmerFilter filter, TokenFiltersDescriptor descriptor)
        {
            descriptor.Stemmer(name, f => f
                .Language(filter.language));
        }

        private void SetPhoneticTokenFiltersDescriptor(string name, PhoneticFilter filter, TokenFiltersDescriptor descriptor)
        {
            var encoder = JsonConvert.DeserializeObject<PhoneticEncoder>($"\"{filter.encoder}\"");
            var languageSet = filter.languageset?
                .Select(x => JsonConvert.DeserializeObject<PhoneticLanguage>($"\"{x}\""));

            descriptor.Phonetic(name, f => f
                .Encoder(encoder)
                .Replace(filter.replace)
                .LanguageSet(languageSet));
        }

        private void SetElisionTokenFiltersDescriptor(string name, ElisionFilter filter, TokenFiltersDescriptor descriptor)
        {
            descriptor.Elision(name, f => f
                .ArticlesCase(filter.articles_case)
                .Articles(filter.articles));
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

        private NormalizersDescriptor GetNormalizersDescriptor(Dictionary<string, Analyzer> normalizers)
        {
            var normalizersDescriptor = new NormalizersDescriptor();

            foreach (var normalizer in normalizers)
            {
                normalizersDescriptor = normalizersDescriptor.Custom(normalizer.Key,
                    cn => cn
                        .CharFilters(normalizer.Value.char_filter)
                        .Filters(normalizer.Value.filter));
            }

            return normalizersDescriptor;
        }

        private TokenizersDescriptor GetTokenizersDesctiptor(Dictionary<string, Tokenizer> tokenizers)
        {
            TokenizersDescriptor tokenizersDescriptor = new TokenizersDescriptor();

            foreach (var t in tokenizers)
            {
                switch (t.Value.type)
                {
                    case "kuromoji_tokenizer":
                        {
                            var casted = t.Value as ElasticSearch.Searcher.Settings.KuromojiTokenizer;
                            KuromojiTokenizationMode mode = KuromojiTokenizationMode.Normal;

                            if (casted.mode == KuromojiTokenizationMode.Extended.GetStringValue().ToLower())
                            {
                                mode = KuromojiTokenizationMode.Extended;
                            }
                            else if (casted.mode == KuromojiTokenizationMode.Search.GetStringValue().ToLower())
                            {
                                mode = KuromojiTokenizationMode.Search;
                            }

                            tokenizersDescriptor =
                                tokenizersDescriptor.Kuromoji(t.Key,
                                    kt =>
                                    {
                                        kt = kt.Mode(mode).DiscardPunctuation(casted.discard_punctuation);
                                        if (casted.user_dictionary?.Count > 0)
                                        {
                                            kt = kt.UserDictionary(string.Join(",", casted.user_dictionary));
                                        }

                                        return kt;
                                    }
                                );
                            break;
                        }
                    default:
                        break;
                }
            }

            return tokenizersDescriptor;
        }

        private void GetAnalyzersWithLowercaseAndPhraseStartsWith(
            IEnumerable<LanguageObj> languages,
            out Dictionary<string, Analyzer> analyzers,
            out Dictionary<string, CustomProperty> customProperties,
            out Dictionary<string, ElasticSearch.Searcher.Settings.Filter> filters,
            out Dictionary<string, Tokenizer> tokenizers,
            out Dictionary<string, Analyzer> normalizers)
        {
            analyzers = new Dictionary<string, Analyzer>();
            customProperties = new Dictionary<string, CustomProperty>();
            filters = new Dictionary<string, ElasticSearch.Searcher.Settings.Filter>();
            tokenizers = new Dictionary<string, Tokenizer>();
            normalizers = new Dictionary<string, Analyzer>();

            if (languages != null)
            {
                GetAnalyzersWithLowercase(languages, out analyzers, out customProperties, out filters, out tokenizers, out normalizers);

                var defaultCharFilter = new List<string>() { "html_strip" };

                analyzers.Add(DEFAULT_PHRASE_STARTS_WITH_ANALYZER, new Analyzer()
                {
                    tokenizer = "keyword",
                    filter = new List<string>()
                    {
                        "lowercase",
                        DEFAULT_EDGENGRAM_FILTER,
                        "icu_folding",
                        "icu_normalizer"
                    },
                    char_filter = defaultCharFilter
                });
                analyzers.Add(DEFAULT_PHRASE_STARTS_WITH_SEARCH_ANALYZER, new Analyzer()
                {
                    tokenizer = "keyword",
                    filter = new List<string>()
                    {
                        "lowercase",
                        "icu_folding",
                        "icu_normalizer"
                    },
                    char_filter = defaultCharFilter
                });
            }
        }

        private void GetAnalyzersWithLowercase(
            IEnumerable<LanguageObj> languages,
            out Dictionary<string, Analyzer> analyzers,
            out Dictionary<string, CustomProperty> customProperties,
            out Dictionary<string, ElasticSearch.Searcher.Settings.Filter> filters,
            out Dictionary<string, Tokenizer> tokenizers,
            out Dictionary<string, Analyzer> normalizers)
        {
            normalizers = new Dictionary<string, Analyzer>();

            GetAnalyzersAndFiltersFromConfiguration(languages, out analyzers, out customProperties, out filters, out tokenizers);

            // we always want a lowercase analyzer
            // we always want "autocomplete" ability
            analyzers.Add(DEFAULT_LOWERCASE_ANALYZER,
                new Analyzer
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
                });
            // Normalizers are almost the same as analyzer, so I'd say there is no need to change naming.
            normalizers.Add(DEFAULT_LOWERCASE_ANALYZER,
                new Analyzer
                {
                    tokenizer = "keyword",
                    filter = new List<string>()
                    {
                        "lowercase",
                        "asciifolding"
                    }
                });
        }

        private void GetAnalyzersAndFiltersFromConfiguration(
            IEnumerable<LanguageObj> languages,
            out Dictionary<string, Analyzer> analyzers,
            out Dictionary<string, CustomProperty> customProperties,
            out Dictionary<string, ElasticSearch.Searcher.Settings.Filter> filters,
            out Dictionary<string, Tokenizer> tokenizers)
        {
            analyzers = new Dictionary<string, Analyzer>();
            customProperties = new Dictionary<string, CustomProperty>();
            filters = new Dictionary<string, ElasticSearch.Searcher.Settings.Filter>();
            tokenizers = new Dictionary<string, Tokenizer>();
            // by ref... will be updated there
            SetDefaultAnalyzersAndFilters(analyzers, filters, tokenizers, languages);

            foreach (LanguageObj language in languages)
            {
                var currentAnalyzers = _esIndexDefinitions.GetAnalyzers(ElasticsearchVersion.ES_7, language.Code);
                var currentCustomProperties = _esIndexDefinitions.GetCustomProperties(ElasticsearchVersion.ES_7, language.Code);
                var currentFilters = _esIndexDefinitions.GetFilters(ElasticsearchVersion.ES_7, language.Code);
                var currentTokenizers = _esIndexDefinitions.GetTokenizers(ElasticsearchVersion.ES_7, language.Code);

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

                foreach (var customProperty in currentCustomProperties)
                {
                    customProperties[customProperty.Key] = customProperty.Value;
                }

                if (currentFilters != null)
                {
                    foreach (var filter in currentFilters)
                    {
                        filters[filter.Key] = filter.Value;
                    }
                }

                if (currentTokenizers != null)
                {
                    foreach (var tokenizer in currentTokenizers)
                    {
                        tokenizers[tokenizer.Key] = tokenizer.Value;
                    }
                }
            }
        }

        private void InitDefaultAnalyzersAndFilters()
        {
            _defaultAnalyzers = new Dictionary<string, Analyzer>();
            _defaultFilters = new Dictionary<string, ElasticSearch.Searcher.Settings.Filter>();
            _defaultTokenizers = new Dictionary<string, Tokenizer>();

            var defaultCharFilter = new List<string>() { "html_strip" };
            var defaultTokenChars = new List<string>() { "letter", "digit", "punctuation", "symbol" };

            var defaultIndexAnalyzer = new Analyzer()
            {
                char_filter = defaultCharFilter,
                filter = new List<string>()
                {
                    "asciifolding",
                    "lowercase",
                    "default_ngram_filter"
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
            _defaultAnalyzers.Add(DEFAULT_INDEX_ANALYZER, defaultIndexAnalyzer);
            _defaultAnalyzers.Add(DEFAULT_SEARCH_ANALYZER, defaultSearchAnalyzer);
            _defaultAnalyzers.Add(DEFAULT_AUTOCOMPLETE_ANALYZER, new Analyzer()
            {
                char_filter = defaultCharFilter,
                filter = new List<string>()
                {
                    "asciifolding",
                    "lowercase",
                    DEFAULT_EDGENGRAM_FILTER
                },
                tokenizer = "whitespace"
            });
            _defaultAnalyzers.Add(DEFAULT_AUTOCOMPLETE_SEARCH_ANALYZER, new Analyzer()
            {
                char_filter = defaultCharFilter,
                filter = new List<string>()
                {
                    "asciifolding",
                    "lowercase",
                },
                tokenizer = "whitespace"
            });
            _defaultFilters.Add($"default_ngram_filter", new NgramFilter()
            {
                type = "nGram",
                min_gram = 2,
                max_gram = 20,
                token_chars = defaultTokenChars
            });
            var defaultEdgeNgramFilter = new NgramFilter()
            {
                type = "edgeNGram",
                min_gram = 1,
                max_gram = 20,
                token_chars = defaultTokenChars
            };
            _defaultFilters.Add(DEFAULT_EDGENGRAM_FILTER, defaultEdgeNgramFilter);

            _defaultTokenizers.Add("jap_tokenizer", new ElasticSearch.Searcher.Settings.KuromojiTokenizer()
            {
                discard_punctuation = true,
                mode = KuromojiTokenizationMode.Extended.ToString(),
            });
            _defaultAnalyzers.Add("jap_autocomplete_analyzer", new Analyzer()
            {
                filter = new List<string>()
                {
                    "asciifolding",
                    "lowercase",
                    "jap_edgengram_filter"
                },
                tokenizer = "jap_tokenizer",
                char_filter = defaultCharFilter
            });
            _defaultAnalyzers.Add("jap_autocomplete_search_analyzer", new Analyzer()
            {
                filter = new List<string>()
                {
                    "asciifolding",
                    "lowercase"
                },
                tokenizer = "jap_tokenizer",
                char_filter = defaultCharFilter
            });
            _defaultFilters.Add("jap_edgengram_filter", defaultEdgeNgramFilter);
        }

        private void SetDefaultAnalyzersAndFilters(
            Dictionary<string, Analyzer> analyzers,
            Dictionary<string, ElasticSearch.Searcher.Settings.Filter> filters,
            Dictionary<string, Tokenizer> tokenizers,
            IEnumerable<LanguageObj> languages)
        {
            // take all the analyzers/filters/tokenizers starting with "default", and add them to dictionaries
            _defaultAnalyzers.Where(analyzer => analyzer.Key.StartsWith("default")).ForEach(analyzer => analyzers[analyzer.Key] = analyzer.Value);
            _defaultFilters.Where(filter => filter.Key.StartsWith("default")).ForEach(filter => filters[filter.Key] = filter.Value);
            _defaultTokenizers.Where(tokenizer => tokenizer.Key.StartsWith("default")).ForEach(tokenizer => tokenizers[tokenizer.Key] = tokenizer.Value);

            // take all the analyzers/filters/tokenizers starting with *this partner's languages*, and add them to dictionaries
            foreach (var language in languages)
            {
                _defaultAnalyzers.Where(analyzer => analyzer.Key.StartsWith(language.Code)).ForEach(analyzer => analyzers[analyzer.Key] = analyzer.Value);
                _defaultFilters.Where(filter => filter.Key.StartsWith(language.Code)).ForEach(filter => filters[filter.Key] = filter.Value);
                _defaultTokenizers.Where(tokenizer => tokenizer.Key.StartsWith(language.Code)).ForEach(tokenizer => tokenizers[tokenizer.Key] = tokenizer.Value);

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

            var catalogGroupCache = GetCatalogGroupCache();
            if (IsOpc() && catalogGroupCache != null)
            {
                try
                {
                    HashSet<string> topicsToIgnore = Core.Catalog.CatalogLogic.GetTopicsToIgnoreOnBuildIndex();
                    tags = catalogGroupCache.TopicsMapBySystemNameAndByType.Where(x => x.Value.ContainsKey(ApiObjects.MetaType.Tag.ToString()) && !topicsToIgnore.Contains(x.Key)).Select(x => x.Key.ToLower()).ToList();

                    foreach (KeyValuePair<string, Dictionary<string, Topic>> topics in catalogGroupCache.TopicsMapBySystemNameAndByType)
                    {
                        //TODO anat ask Ira
                        if (topics.Value.Keys.Any(x => x != ApiObjects.MetaType.Tag.ToString() && x != ApiObjects.MetaType.ReleatedEntity.ToString()))
                        {
                            string nullValue = string.Empty;

                            var topicMetaType = CatalogManager.GetTopicMetaType(topics.Value);
                            IndexManagerCommonHelpers.GetMetaType(topicMetaType, out var metaType, out nullValue);

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
            else
            {
                var groupManager = GetGroupManager();
                if (groupManager != null)
                {
                    try
                    {
                        if (groupManager.m_oEpgGroupSettings != null && groupManager.m_oEpgGroupSettings.m_lTagsName != null)
                        {
                            foreach (var item in groupManager.m_oEpgGroupSettings.m_lTagsName)
                            {
                                if (!tags.Contains(item.ToLower()))
                                {
                                    tags.Add(item.ToLower());
                                }
                            }
                        }

                        if (groupManager.m_oGroupTags != null)
                        {
                            foreach (var item in groupManager.m_oGroupTags.Values)
                            {
                                if (!tags.Contains(item.ToLower()))
                                {
                                    tags.Add(item.ToLower());
                                }
                            }
                        }

                        var realMetasType = new Dictionary<string, eESFieldType>();
                        if (groupManager.m_oMetasValuesByGroupId != null)
                        {
                            foreach (Dictionary<string, string> metaMap in groupManager.m_oMetasValuesByGroupId.Values)
                            {
                                foreach (KeyValuePair<string, string> meta in metaMap)
                                {
                                    string nullValue = string.Empty;
                                    eESFieldType metaType;
                                    IndexManagerCommonHelpers.GetMetaType(meta.Key, out metaType, out nullValue);

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

                        if (groupManager.m_oEpgGroupSettings != null && groupManager.m_oEpgGroupSettings.m_lMetasName != null)
                        {
                            foreach (string epgMeta in groupManager.m_oEpgGroupSettings.m_lMetasName)
                            {
                                string nullValue = string.Empty;
                                eESFieldType metaType;
                                IndexManagerCommonHelpers.GetMetaType(epgMeta, out metaType, out nullValue);

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
            }

            return result;
        }

        private void AddEpgIndexAlias(string indexName, bool isWriteIndex = false)
        {
            // create alias is idempotent request
            var epgIndexAlias = NamingHelper.GetEpgIndexAlias(_partnerId);
            log.Info($"creating alias. index [{indexName}], alias [{epgIndexAlias}]");
            IndexManagerCommonHelpers.GetRetryPolicy<Exception>().Execute(() =>
            {
                var putAliasResponse = _elasticClient.Indices.PutAlias(indexName, epgIndexAlias, i=>i.IsWriteIndex(isWriteIndex));
                bool isAliasAdded = putAliasResponse != null && putAliasResponse.IsValid;
                if (!isAliasAdded) throw new Exception($"index set alias failed [{indexName}], alias [{epgIndexAlias}]");
            });
        }

        private PropertiesDescriptor<NestMedia> GetMediaPropertiesDescriptor(
            List<LanguageObj> languages,
            Dictionary<string, KeyValuePair<eESFieldType, string>> metas,
            List<string> tags,
            HashSet<string> metasToPad,
            Dictionary<string, Analyzer> analyzers,
            Dictionary<string, CustomProperty> customProperties)
        {
            PropertiesDescriptor<NestMedia> propertiesDescriptor = new PropertiesDescriptor<NestMedia>();
            var liveToVodPropertiesDescriptor = new PropertiesDescriptor<NestLiveToVodProperties>()
                .Date(x => x.Name(NamingHelper.ORIGINAL_START_DATE))
                .Date(x => x.Name(NamingHelper.ORIGINAL_END_DATE))
                .Keyword(x => InitializeDefaultTextPropertyDescriptor<string>(NamingHelper.CRID))
                .Keyword(x => InitializeDefaultTextPropertyDescriptor<string>(NamingHelper.EPG_ID))
                .Number(x => x.Name(NamingHelper.LINEAR_ASSET_ID).Type(NumberType.Long))
                .Number(x => x.Name(NamingHelper.EPG_CHANNEL_ID).Type(NumberType.Long));

            propertiesDescriptor
                .Keyword(x => x.Name(m => m.DocumentId))
                .Number(x => x.Name("media_id").Type(NumberType.Long).NullValue(0))
                .Number(x => x.Name("group_id").Type(NumberType.Integer).NullValue(0))
                .Number(x => x.Name("media_type_id").Type(NumberType.Integer).NullValue(0))
                .Number(x => x.Name("epg_channel_id").Type(NumberType.Integer).NullValue(0))
                .Number(x => x.Name("wp_type_id").Type(NumberType.Integer).NullValue(0))
                .Number(x => x.Name("device_rule_id").Type(NumberType.Integer).NullValue(0))
                .Boolean(x => x.Name("is_active"))
                .Number(x => x.Name("like_counter").Type(NumberType.Integer).NullValue(0))
                .Number(x => x.Name("user_types").Type(NumberType.Integer))
                .Number(x => x.Name("language_id").Type(NumberType.Long))
                .Number(x => x.Name("allowed_countries").Type(NumberType.Integer))
                .Number(x => x.Name("blocked_countries").Type(NumberType.Integer))
                .Number(x => x.Name("inheritance_policy").Type(NumberType.Integer))
                .Date(x => x.Name("start_date"))
                .Date(x => x.Name("end_date"))
                .Date(x => x.Name("cache_date"))
                .Date(x => x.Name("create_date"))
                .Date(x => x.Name("update_date"))
                .Date(x => x.Name("catalog_start_date"))
                .Date(x => x.Name("final_date"))

                .Object<NestLiveToVodProperties>(x => x.Name(NamingHelper.LIVE_TO_VOD_PREFIX).Properties(properties => liveToVodPropertiesDescriptor));

            InitializeTextField(
                "external_id",
                propertiesDescriptor,
                DEFAULT_INDEX_ANALYZER,
                DEFAULT_SEARCH_ANALYZER,
                DEFAULT_AUTOCOMPLETE_ANALYZER,
                DEFAULT_AUTOCOMPLETE_SEARCH_ANALYZER,
                DEFAULT_LOWERCASE_ANALYZER,
                DEFAULT_PHRASE_STARTS_WITH_ANALYZER,
                DEFAULT_PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                null,
                null,
                null);
            InitializeTextField(
                "entry_id",
                propertiesDescriptor,
                DEFAULT_INDEX_ANALYZER,
                DEFAULT_SEARCH_ANALYZER,
                DEFAULT_AUTOCOMPLETE_ANALYZER,
                DEFAULT_AUTOCOMPLETE_SEARCH_ANALYZER,
                DEFAULT_LOWERCASE_ANALYZER,
                DEFAULT_PHRASE_STARTS_WITH_ANALYZER,
                DEFAULT_PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                null,
                null,
                null);

            if (!metas.ContainsKey(META_SUPPRESSED))
            {
                metas.Add(META_SUPPRESSED, new KeyValuePair<eESFieldType, string>(eESFieldType.STRING, string.Empty)); //new meta for suppressed value
            }

            AddLanguageSpecificMappingToPropertyDescriptor(languages, metas, tags, metasToPad, analyzers, customProperties, propertiesDescriptor);

            return propertiesDescriptor;
        }

        private PropertiesDescriptor<T> GetPropertiesDescriptor<T>(
            PropertiesDescriptor<T> propertiesDescriptor,
            List<LanguageObj> languages,
            Dictionary<string, Analyzer> analyzers,
            List<string> multilingualFields,
            List<string> simpleFields)
        where T : class
        {
            if (multilingualFields != null)
            {
                foreach (var field in multilingualFields)
                {
                    var fieldPropertiesDescriptor = new PropertiesDescriptor<object>();

                    foreach (var language in languages)
                    {
                        GetCurrentLanguageAnalyzers(
                            analyzers,
                            language,
                            out string indexAnalyzer,
                            out string searchAnalyzer,
                            out string autocompleteAnalyzer,
                            out string autocompleteSearchAnalyzer,
                            out _,
                            out _,
                            out string lowercaseAnalyzer,
                            out _,
                            out _);

                        InitializeTextField(
                            $"{language.Code}",
                            fieldPropertiesDescriptor,
                            indexAnalyzer,
                            searchAnalyzer,
                            autocompleteAnalyzer,
                            autocompleteSearchAnalyzer,
                            lowercaseAnalyzer,
                            null,
                            null,
                            null,
                            null,
                            null);
                    }

                    propertiesDescriptor.Object<object>(x => x
                        .Name(field)
                        .Properties(properties => fieldPropertiesDescriptor))
                    ;
                }
            }

            if (simpleFields != null)
            {
                foreach (var field in simpleFields)
                {
                    propertiesDescriptor.Keyword(t =>
                        InitializeTextField(
                            field,
                            propertiesDescriptor,
                            DEFAULT_INDEX_ANALYZER,
                            DEFAULT_SEARCH_ANALYZER,
                            DEFAULT_AUTOCOMPLETE_ANALYZER,
                            DEFAULT_AUTOCOMPLETE_SEARCH_ANALYZER,
                            DEFAULT_LOWERCASE_ANALYZER,
                            null,
                            null,
                            null,
                            null,
                            null));
                }
            }

            return propertiesDescriptor;
        }

        private void AddLanguageSpecificMappingToPropertyDescriptor<T>(
            List<LanguageObj> languages,
            Dictionary<string, KeyValuePair<eESFieldType, string>> metas,
            List<string> tags,
            HashSet<string> metasToPad,
            Dictionary<string, Analyzer> analyzers,
            Dictionary<string, CustomProperty> customProperties,
            PropertiesDescriptor<T> propertiesDescriptor)
            where T : class
        {
            PropertiesDescriptor<object> namePropertiesDescriptor = new PropertiesDescriptor<object>();
            PropertiesDescriptor<object> descriptionPropertiesDescriptor = new PropertiesDescriptor<object>();
            PropertiesDescriptor<object> tagsPropertiesDescriptor = new PropertiesDescriptor<object>();
            PropertiesDescriptor<object> metasPropertiesDescriptor = new PropertiesDescriptor<object>();
            var dictionaryTagPropertiesDescriptor = languages.ToDictionary(k => k.Code, k => new PropertiesDescriptor<NestBaseAsset>());
            var dictionaryMetaPropertiesDescriptor = languages.ToDictionary(k => k.Code, k => new PropertiesDescriptor<NestBaseAsset>());

            foreach (var language in languages)
            {
                GetCurrentLanguageAnalyzers(
                    analyzers,
                    language,
                    out var indexAnalyzer,
                    out var searchAnalyzer,
                    out var autocompleteAnalyzer,
                    out var autocompleteSearchAnalyzer,
                    out var phoneticIndexAnalyzer,
                    out var phoneticSearchAnalyzer,
                    out var lowercaseAnalyzer,
                    out var phraseStartsWithAnalyzer,
                    out var phraseStartsWithSearchAnalyzer);
                var sortProperty = GetCurrentLanguageSortProperty(customProperties, language);

                InitializeTextField(
                    $"{language.Code}",
                    namePropertiesDescriptor,
                    indexAnalyzer,
                    searchAnalyzer,
                    autocompleteAnalyzer,
                    autocompleteSearchAnalyzer,
                    lowercaseAnalyzer,
                    phraseStartsWithAnalyzer,
                    phraseStartsWithSearchAnalyzer,
                    phoneticIndexAnalyzer,
                    phoneticSearchAnalyzer,
                    sortProperty
                );
                InitializeTextField(
                    $"{language.Code}",
                    descriptionPropertiesDescriptor,
                    indexAnalyzer,
                    searchAnalyzer,
                    autocompleteAnalyzer,
                    autocompleteSearchAnalyzer,
                    lowercaseAnalyzer,
                    phraseStartsWithAnalyzer,
                    phraseStartsWithSearchAnalyzer,
                    phoneticIndexAnalyzer,
                    phoneticSearchAnalyzer,
                    null
                );

                foreach (var tag in tags)
                {
                    InitializeTextField(tag,
                        dictionaryTagPropertiesDescriptor[language.Code],
                        indexAnalyzer,
                        searchAnalyzer,
                        autocompleteAnalyzer,
                        autocompleteSearchAnalyzer,
                        lowercaseAnalyzer,
                        phraseStartsWithAnalyzer,
                        phraseStartsWithSearchAnalyzer,
                        phoneticIndexAnalyzer,
                        phoneticSearchAnalyzer,
                        null
                    );
                }

                tagsPropertiesDescriptor.Object<object>(selector => selector
                    .Name($"{language.Code}")
                    .Properties(p => dictionaryTagPropertiesDescriptor[language.Code]));

                foreach (var meta in metas)
                {
                    string metaName = meta.Key.ToLower();
                    bool shouldAddPadded = metasToPad.Contains(metaName);

                    var metaType = meta.Value.Key;

                    if (metaType != eESFieldType.DATE)
                    {
                        if (metaType == eESFieldType.STRING)
                        {
                            InitializeTextField(metaName,
                                dictionaryMetaPropertiesDescriptor[language.Code],
                                indexAnalyzer,
                                searchAnalyzer,
                                autocompleteAnalyzer,
                                autocompleteSearchAnalyzer,
                                lowercaseAnalyzer,
                                phraseStartsWithAnalyzer,
                                phraseStartsWithSearchAnalyzer,
                                phoneticIndexAnalyzer,
                                phoneticSearchAnalyzer,
                                sortProperty,
                                shouldAddPadded);
                        }
                        else
                        {
                            InitializeNumericMetaField(
                                dictionaryMetaPropertiesDescriptor[language.Code],
                                metaName, shouldAddPadded,
                                searchAnalyzer,
                                indexAnalyzer,
                                lowercaseAnalyzer,
                                phraseStartsWithAnalyzer,
                                phraseStartsWithSearchAnalyzer,
                                autocompleteAnalyzer,
                                autocompleteSearchAnalyzer);
                        }
                    }
                    else
                    {
                        dictionaryMetaPropertiesDescriptor[language.Code].Date(x => x.Name(metaName).Format(ESUtils.ES_DATE_FORMAT));
                    }
                }

                metasPropertiesDescriptor.Object<object>(selector => selector
                    .Name($"{language.Code}")
                    .Properties(p => dictionaryMetaPropertiesDescriptor[language.Code]));
            }

            propertiesDescriptor.Object<object>(x => x
                .Name($"name")
                .Properties(properties => namePropertiesDescriptor))
            ;
            propertiesDescriptor.Object<object>(x => x
                .Name($"description")
                .Properties(properties => descriptionPropertiesDescriptor))
            ;

            foreach (var meta in dictionaryMetaPropertiesDescriptor)
            {
                metasPropertiesDescriptor.Object<object>(x =>
                    x.Name(meta.Key.ToLower())
                    .Properties(p => meta.Value));
            }

            propertiesDescriptor.Object<object>(x => x
                .Name($"metas")
                .Properties(properties => metasPropertiesDescriptor))
            ;

            foreach (var tag in dictionaryTagPropertiesDescriptor)
            {
                tagsPropertiesDescriptor.Object<object>(x =>
                    x.Name(tag.Key)
                    .Properties(p => tag.Value));
            }

            propertiesDescriptor.Object<object>(x => x
                .Name($"tags")
                .Properties(properties => tagsPropertiesDescriptor))
            ;
        }

        private static void GetCurrentLanguageAnalyzers(
            Dictionary<string, Analyzer> analyzers,
            LanguageObj language,
            out string indexAnalyzer,
            out string searchAnalyzer,
            out string autocompleteAnalyzer,
            out string autocompleteSearchAnalyzer,
            out string phoneticIndexAnalyzer,
            out string phoneticSearchAnalyzer,
            out string lowercaseAnalyzer,
            out string phraseStartsWithAnalyzer,
            out string phraseStartsWithSearchAnalyzer)
        {
            indexAnalyzer = $"{language.Code}_index_analyzer";
            searchAnalyzer = $"{language.Code}_search_analyzer";
            autocompleteAnalyzer = $"{language.Code}_autocomplete_analyzer";
            autocompleteSearchAnalyzer = $"{language.Code}_autocomplete_search_analyzer";
            lowercaseAnalyzer = $"{language.Code}_lowercase_analyzer";
            phraseStartsWithAnalyzer = $"{language.Code}_phrase_starts_with_analyzer";
            phraseStartsWithSearchAnalyzer = $"{language.Code}_phrase_starts_with_search_analyzer";
            var dblMetaphoneIndexAnalyzerCandidate = $"{language.Code}_index_dbl_metaphone";
            var dblMetaphoneSearchAnalyzerCandidate = $"{language.Code}_search_dbl_metaphone";
            var phoneticIndexAnalyzerCandidate = $"{language.Code}_index_phonetic";
            var phoneticSearchAnalyzerCandidate = $"{language.Code}_search_phonetic";
            
            if (!analyzers.ContainsKey(indexAnalyzer))
            {
                indexAnalyzer = DEFAULT_INDEX_ANALYZER;
            }

            if (!analyzers.ContainsKey(searchAnalyzer))
            {
                searchAnalyzer = DEFAULT_SEARCH_ANALYZER;
            }

            if (!analyzers.ContainsKey(autocompleteAnalyzer))
            {
                autocompleteAnalyzer = DEFAULT_AUTOCOMPLETE_ANALYZER;
            }

            if (!analyzers.ContainsKey(autocompleteSearchAnalyzer))
            {
                autocompleteSearchAnalyzer = DEFAULT_AUTOCOMPLETE_SEARCH_ANALYZER;
            }

            if (!analyzers.ContainsKey(lowercaseAnalyzer))
            {
                lowercaseAnalyzer = DEFAULT_LOWERCASE_ANALYZER;
            }

            if (!analyzers.ContainsKey(phraseStartsWithAnalyzer))
            {
                phraseStartsWithAnalyzer = DEFAULT_PHRASE_STARTS_WITH_ANALYZER;
            }

            if (!analyzers.ContainsKey(phraseStartsWithSearchAnalyzer))
            {
                phraseStartsWithSearchAnalyzer = DEFAULT_PHRASE_STARTS_WITH_SEARCH_ANALYZER;
            }

            phoneticIndexAnalyzer = null;
            if (analyzers.ContainsKey(dblMetaphoneIndexAnalyzerCandidate))
            {
                phoneticIndexAnalyzer = dblMetaphoneIndexAnalyzerCandidate;
            }
            else if (analyzers.ContainsKey(phoneticIndexAnalyzerCandidate))
            {
                phoneticIndexAnalyzer = phoneticIndexAnalyzerCandidate;
            }

            phoneticSearchAnalyzer = null;
            if (analyzers.ContainsKey(dblMetaphoneSearchAnalyzerCandidate))
            {
                phoneticSearchAnalyzer = dblMetaphoneSearchAnalyzerCandidate;
            }
            else if (analyzers.ContainsKey(phoneticSearchAnalyzerCandidate))
            {
                phoneticSearchAnalyzer = phoneticSearchAnalyzerCandidate;
            }
        }

	   private SortProperty GetCurrentLanguageSortProperty(Dictionary<string, CustomProperty> customProperties, LanguageObj language)
        {
            if (customProperties.TryGetValue($"{language.Code}_sort", out var customProperty)
                && customProperty is SortProperty sortProperty)
            {
                return sortProperty;
            }

            return null;
        }

        private void CreateNewEpgV1Index(string newIndexName, bool isRecording = false, bool shouldBuildWithReplicas = true, bool shouldUseNumOfConfiguredShards = true,
        	int? refreshInterval = null)
        {
            CreateEmptyEpgIndex(newIndexName, EpgFeatureVersion.V1, shouldBuildWithReplicas, shouldUseNumOfConfiguredShards, refreshInterval);
        }

        private bool SwitchIndexAlias(string newIndexName, string alias, bool shouldDeleteOldIndices, bool shouldSwitchIndex)
        {
            bool result = false;

            try
            {
                var currentIndices = _elasticClient.GetIndicesPointingToAlias(alias);

                if (shouldSwitchIndex || currentIndices?.Count == 0)
                {
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
                            var deleteResult = _elasticClient.Indices.Delete(Indices.Index(currentIndices));

                            if (deleteResult != null && deleteResult.IsValid)
                            {
                                log.Debug($"Deleted indices {string.Join(",", currentIndices)}");
                            }
                        }
                    }
                }

                result = true;
            }
            catch (Exception ex)
            {
                log.Error($"Failed switching index alias for {newIndexName} and {alias}", ex);
            }

            return result;
        }

        private static string GetChannelDocumentId(Channel channel)
        {
            return GetChannelDocumentId(channel.m_nChannelID);
        }

        private static string GetChannelDocumentId(long channelId)
        {
            return $"{channelId}";
        }

        #endregion
    }
}
