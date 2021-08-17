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
using Newtonsoft.Json.Linq;
using Polly.Retry;
using ESUtils = ElasticSearch.Common.Utils;
using ConfigurationManager;
using Status = ApiObjects.Response.Status;
using System.Linq;
using KLogMonitor;
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
using KlogMonitorHelper;
using Policy = Polly.Policy;
using System.Net;
using System.Net.Sockets;
using ApiLogic.Api.Managers;
using Core.GroupManagers;
using NestMedia = ApiLogic.IndexManager.NestData.NestMedia;
using SocialActionStatistics = ApiObjects.Statistics.SocialActionStatistics;
using ApiLogic.IndexManager.QueryBuilders;
using ApiObjects.Response;
using System.Text;
using Index = Nest.Index;
using TVinciShared;
using Channel = GroupsCacheManager.Channel;
using OrderDir = ApiObjects.SearchObjects.OrderDir;
using TvinciCache;

namespace Core.Catalog
{
    public class IndexManagerV7 : IIndexManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Consts

        private const int REFRESH_INTERVAL_FOR_EMPTY_INDEX_SECONDS = 10;
        private const string INDEX_REFRESH_INTERVAL = "10s";
        private const int MAX_RESULTS_DEFAULT = 100000;

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
        private readonly IChannelQueryBuilder _channelQueryBuilder;
        private readonly IGroupsFeatures _groupsFeatures;

        private readonly int _partnerId;
        private bool _groupUsesTemplates;
        private readonly IGroupManager _groupManager;
        private readonly int _sizeOfBulk;
        private readonly int _sizeOfBulkDefaultValue;
        private readonly ITtlService _ttlService;

        private HashSet<string> _metasToPad;
        private Group _group;
        private CatalogGroupCache _catalogGroupCache;
        private Dictionary<string, LanguageObj> _partnerLanguageCodes;

        /// <summary>
        /// Initialiezs an instance of Index Manager for work with ElasticSearch 7.14
        /// Please do not use this ctor, rather use IndexManagerFactory.
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="elasticClient"></param>
        /// <param name="applicationConfiguration"></param>
        /// <param name="groupManager"></param>
        /// <param name="catalogManager"></param>
        /// <param name="esIndexDefinitions"></param>
        /// <param name="channelManager"></param>
        /// <param name="catalogCache"></param>
        /// <param name="ttlService"></param>
        /// <param name="watchRuleManager"></param>
        /// <param name="channelQueryBuilder"></param>
        public IndexManagerV7(int partnerId,
            IElasticClient elasticClient,
            IApplicationConfiguration applicationConfiguration,
            IGroupManager groupManager,
            ICatalogManager catalogManager,
            IElasticSearchIndexDefinitions esIndexDefinitions,
            IChannelManager channelManager,
            ICatalogCache catalogCache,
            ITtlService ttlService,
            IWatchRuleManager watchRuleManager,
            IChannelQueryBuilder channelQueryBuilder,
            IGroupsFeatures groupsFeatures
        )
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

            //init all ES const
            _numOfShards = _applicationConfiguration.ElasticSearchHandlerConfiguration.NumberOfShards.Value;
            _numOfReplicas = _applicationConfiguration.ElasticSearchHandlerConfiguration.NumberOfReplicas.Value;
            _maxResults = _applicationConfiguration.ElasticSearchConfiguration.MaxResults.Value;
            _sizeOfBulk = _applicationConfiguration.ElasticSearchHandlerConfiguration.BulkSize.Value;
            _sizeOfBulkDefaultValue = _applicationConfiguration.ElasticSearchHandlerConfiguration.BulkSize.GetDefaultValue();

            InitializePartnerData(_partnerId);
            GetMetasAndTagsForMapping(out _, out _, out _metasToPad);
        }

        private void InitializePartnerData(int partnerId)
        {
            _partnerLanguageCodes = new Dictionary<string, LanguageObj>();

            if (partnerId <= 0)
            {
                return;
            }

            _groupUsesTemplates = _catalogManager.DoesGroupUsesTemplates(partnerId);

            if (_groupUsesTemplates)
            {
                _catalogManager.TryGetCatalogGroupCacheFromCache(partnerId, out _catalogGroupCache);
                _partnerLanguageCodes = _catalogGroupCache.LanguageMapByCode;
            }
            else
            {
                _group = _groupManager.GetGroup(partnerId);
                _partnerLanguageCodes = _group.GetLangauges().ToDictionary(x => x.Code, x => x);
            }

            if (_catalogGroupCache == null && _group == null)
            {
                log.Error($"Could not load group configuration for {partnerId}");
            }
        }

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

            var metasToPad = _metasToPad;
            if (_groupUsesTemplates)
            {
                languagesMap = new Dictionary<int, LanguageObj>(_catalogGroupCache.LanguageMapById);

                var metas = _catalogGroupCache.TopicsMapById.Values.Where(x => x.Type == ApiObjects.MetaType.Number).Select(y => y.SystemName).ToList();
                if (metas?.Count > 0)
                {
                    metasToPad = new HashSet<string>(metas);
                }
            }
            else
            {
                List<LanguageObj> languages = _group.GetLangauges();
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
                log.Error($"Error on upsert media for asste {assetId}", ex);
            }

            log.Debug($"Upsert Media result {result}");

            return result;
        }

        public string SetupEpgV2Index(DateTime dateOfProgramsToIngest)
        {
            string dailyEpgIndexName = NamingHelper.GetDailyEpgIndexName(_partnerId, dateOfProgramsToIngest);
            EnsureEpgIndexExist(dailyEpgIndexName);

            SetNoRefresh(dailyEpgIndexName);

            return dailyEpgIndexName;
        }

        public bool ForceRefreshEpgV2Index(DateTime date)
        {
            var dailyEpgIndexName = NamingHelper.GetDailyEpgIndexName(_partnerId, date);
            var response = _elasticClient.Indices.Refresh(new RefreshRequest(dailyEpgIndexName));
            return response.IsValid;
        }

        public bool FinalizeEpgV2Indices(List<DateTime> dates)
        {
            var indices = dates.Select(x => NamingHelper.GetDailyEpgIndexName(_partnerId, x));
            var existingIndices = indices.Select(x => x).Where(x => _elasticClient.Indices.Exists(x).IsValid).ToList();

            foreach (var index in existingIndices)
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

            return true;
        }

        public bool DeleteProgram(List<long> epgIds, IEnumerable<string> epgChannelIds)
        {
            bool isSuccess = false;

            if (epgIds == null || epgIds.Count == 0)
            {
                return isSuccess;
            }

            string index = NamingHelper.GetEpgIndexAlias(_partnerId);
            var deleteResponse = _elasticClient.DeleteByQuery<NestEpg>(request => request
                .Index(index)
                .Query(query => query
                    .Terms(terms => terms.Field(epg => epg.EpgID).Terms<long>(epgIds))
                    ));

            isSuccess = deleteResponse.IsValid;

            try
            {
                // support for old invalidation keys
                if (isSuccess)
                {
                    // invalidate epg's for OPC and NON-OPC accounts
                    EpgAssetManager.InvalidateEpgs(_partnerId, epgIds, _groupUsesTemplates, epgChannelIds, true);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed invalidating Epgs on partner {_partnerId}", ex);
            }

            return isSuccess;
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
                var epgChannelIds = epgObjects.Select(item => item.ChannelID.ToString()).ToList();
                linearChannelSettings = linearChannelSettings ?? new Dictionary<string, LinearChannelSettings>();

                var bulkRequests = new List<NestEsBulkRequest<NestEpg>>();

                var isRegionalizationEnabled = _groupUsesTemplates
                    ? _catalogGroupCache.IsRegionalizationEnabled
                    : _group.isRegionalizationEnabled;

                var linearChannelsRegionsMapping = isRegionalizationEnabled
                    ? RegionManager.GetLinearMediaRegions(_partnerId)
                    : new Dictionary<long, List<int>>();

                var createdAliases = new HashSet<string>();
                _catalogManager.GetLinearChannelValues(epgObjects, _partnerId, _ => { });

                var alias = NamingHelper.GetEpgIndexAlias(_partnerId);
                var isIngestV2 = GroupSettingsManager.DoesGroupUseNewEpgIngest(_partnerId);

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
                        if (isIngestV2)
                        {
                            alias = EnsureEpgIndexExistsForEpg(epgCb, createdAliases);
                        }

                        UpdateEpgLinearMediaId(linearChannelSettings, epgCb);
                        UpdateEpgRegions(epgCb, linearChannelsRegionsMapping);

                        var expiry = GetEpgExpiry(epgCb);
                        var epg = NestDataCreator.GetEpg(epgCb, language.ID, true, _groupUsesTemplates,expiry);
                        var nestEsBulkRequest = GetEpgBulkRequest(alias, epgCb.StartDate.ToUniversalTime(), epg);
                        bulkRequests.Add(nestEsBulkRequest);

                        //prepare next bulk
                        if (bulkRequests.Count <= sizeOfBulk)
                        {
                            continue;
                        }

                        var isValid = ExecuteAndValidateBulkRequests(bulkRequests);
                        if (isValid)
                        {
                            EpgAssetManager.InvalidateEpgs(_partnerId,
                                bulkRequests.Select(x => (long)x.Document.EpgID), _groupUsesTemplates, epgChannelIds,
                                false);
                        }

                        result &= isValid;
                    }
                }

                if (bulkRequests.Any())
                {
                    var isValid = ExecuteAndValidateBulkRequests(bulkRequests);
                    if (isValid)
                    {
                        EpgAssetManager.InvalidateEpgs(_partnerId,
                            bulkRequests.Select(x => (long)x.Document.EpgID), _groupUsesTemplates,
                            epgChannelIds, false);
                    }
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

        private string EnsureEpgIndexExistsForEpg(EpgCB epgCb, HashSet<string> createdAliases)
        {
            var alias = NamingHelper.GetDailyEpgIndexName(_partnerId, epgCb.StartDate.Date);

            // in case alias already created, no need to check in ES
            if (!createdAliases.Contains(alias))
            {
                EnsureEpgIndexExist(alias);
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

            string mediaIndex = NamingHelper.GetMediaIndexAlias(_partnerId);
            var indices = _elasticClient.GetIndicesPointingToAlias(mediaIndex);

            List<Channel> channels = new List<Channel>();

            if (channel != null)
            {
                channels.Add(channel);
            }
            else
            {
                if (_group == null || _group.channelIDs == null || _group.channelIDs.Count == 0)
                {
                    return result;
                }

                foreach (int channelId in channelIds)
                {
                    //todo gil tests how can we remove it???
                    Channel channelToUpdate = ChannelRepository.GetChannel(channelId, _group);

                    if (channelToUpdate != null)
                    {
                        channels.Add(channelToUpdate);
                    }
                }
            }

            foreach (var currentChannel in channels)
            {
                result &= UpdateChannelPercolator(channel, indices.ToList());
            }

            return result;
        }

        private bool UpdateChannelPercolator(Channel channel, List<string> aliases)
        {
            bool result = true;

            var query = _channelQueryBuilder.GetChannelQuery(channel);

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
                var deleteResponse = _elasticClient.DeleteByQuery<NestChannelMetadata>(request => request
                    .Index(index)
                    .Query(query => query.Term(channel => channel.ChannelId, channelId)
                        ));

                result = deleteResponse.IsValid;

                if (!deleteResponse.IsValid)
                {
                    log.Error($"Failed deleting channel metadata, id = {channelId}, index = {index}");
                }
                else
                {
                    result = true;

                    bool deletePercolatorResult = this.DeleteChannelPercolator(new List<int>() { channelId });

                    if (!deletePercolatorResult)
                    {
                        log.Error($"Error deleting channel percolator for channel {channelId}");
                    }
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
                log.WarnFormat("Received channel request of invalid channel id {0} when calling UpsertChannel",channelId);
                return false;
            }

            try
            {
                if (channel == null)
                {
                    var response = _channelManager.GetChannelById(_partnerId, channelId, true, userId);
                    if (response != null && response.Status != null && response.Status.Code != (int) eResponseStatus.OK)
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

                if (indexResponse.IsValid && indexResponse.Result == Result.Created)
                {
                    result = true;
                    if ((channel.m_nChannelTypeID != (int) ChannelType.Manual ||
                         (channel.m_lChannelTags != null && channel.m_lChannelTags.Count > 0))
                        && !UpdateChannelPercolator(new List<int>() {channelId}, channel))
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
            else
            {
                try
                {
                    // support for old invalidation keys
                        // invalidate epg's for OPC and NON-OPC accounts
                        _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetMediaInvalidationKey(_partnerId, assetId));
                }
                catch (Exception ex)
                {
                    log.Error($"Failed invalidating media {assetId} on partner {_partnerId}", ex);
                }
            }

            return isSuccess;
        }

        public void UpsertPrograms(IList<EpgProgramBulkUploadObject> calculatedPrograms,
            string draftIndexName,
            DateTime dateOfProgramsToIngest,
            LanguageObj defaultLanguage,
            IDictionary<string, LanguageObj> languages)
        {
            var bulkSize = GetBulkSize();

            var retryCount = 5;
            var policy = IndexManagerCommonHelpers.GetRetryPolicy<Exception>(retryCount);
                
            policy.Execute(() =>
            {
                var bulkRequests = new List<NestEsBulkRequest<NestEpg>>();
                try
                {
                    var programTranslationsToIndex = calculatedPrograms.SelectMany(p => p.EpgCbObjects);
                    foreach (var program in programTranslationsToIndex)
                    {
                        program.PadMetas(_metasToPad);
                        var langId = GetLanguageIdByCode(program.Language);
                        
                        var expiry = GetEpgExpiry(program);
                        var buildEpg = NestDataCreator.GetEpg(program, langId, isOpc: _groupUsesTemplates,expiryUnixTimeStamp:expiry);
                        var bulkRequest = GetEpgBulkRequest(draftIndexName, dateOfProgramsToIngest, buildEpg);
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
            var expiry = ODBCWrapper.Utils.DateTimeToUtcUnixTimestampSeconds(program.EndDate.AddMinutes(totalMinutes));
            return expiry;
        }

        private int GetBulkSize(int? defaultValueWhenZero=null)
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
            DateTime routingDate,
            NestEpg buildEpg,
            string documentId="")
        {
            //override the doc id    
            var docId = string.IsNullOrEmpty(documentId) ? buildEpg.EpgIdentifier:documentId;
            
            var bulkRequest = new NestEsBulkRequest<NestEpg>()
            {
                DocID = $"{docId}_{buildEpg.Language}",
                Document = buildEpg,
                Index = draftIndexName,
                Operation = eOperation.index,
                Routing = routingDate.Date.ToString("yyyyMMdd")
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
            
            var searchResponse = _elasticClient.Search<NestEpg>(s => s
                .Index(index)
                .Size(_maxResults)
                .Query(q =>q
                    .Bool(b => b.Filter(f =>f
                            .Bool(b1 => 
                                b1.Must(
                                    m => m.Terms(t=>t.Field(f1=>f1.ChannelID).Terms(channelId)),
                                    m=>m.DateRange(dr=>dr.Field(f1=>f1.StartDate).LessThanOrEquals(toDate)),
                                    m=>m.DateRange(dr=>dr.Field(f1=>f1.EndDate).GreaterThanOrEquals(fromDate))
                                )
                            )
                        )
                    )
                )
            );
            
            
            // get the programs - epg ids from elasticsearch, information from EPG DAL
            if (searchResponse.IsValid)
            {
                return searchResponse.Hits?.Select(x=> GetEpgProgramBulkUploadObject( x.Source)).ToList();
            }
            return result;
        }

        private EpgProgramBulkUploadObject GetEpgProgramBulkUploadObject(NestEpg epg)
        {
            var epgItem = new EpgProgramBulkUploadObject();
            epgItem.EpgExternalId = epg.EpgIdentifier;
            epgItem.StartDate = epg.StartDate;
            epgItem.EndDate = epg.EndDate;
            epgItem.EpgId = epg.EpgID;
            epgItem.IsAutoFill = epg.IsAutoFill;
            epgItem.ChannelId = epg.ChannelID;
            epgItem.LinearMediaId = epg.LinearMediaId.HasValue ?epg.LinearMediaId.Value: 0;
            epgItem.ParentGroupId = epg.GroupID;
            epgItem.GroupId = _partnerId;
            return epgItem;
        }

        public List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearchDefinitions, ref int totalItems)
        {
            return UnifiedSearch(unifiedSearchDefinitions, ref totalItems, out _);
        }

        public List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions definitions, ref int totalItems, out List<AggregationsResult> aggregationsResults)
        {
            aggregationsResults = null;
            List<UnifiedSearchResult> searchResultsList = new List<UnifiedSearchResult>();
            totalItems = 0;
            
            // build request
            UnifiedSearchNestBuilder builder = new UnifiedSearchNestBuilder()
            {
                Definitions = definitions
            };

            var query = builder.GetQuery();
            var size = builder.GetSize();
            var from = builder.GetFrom();
            var aggs = builder.GetAggs();
            var indices = builder.GetIndices(); // new[] { "", "" };

            // send request
            var searchResponse = _elasticClient.Search<object>(searchRequest => searchRequest
                .Index(indices)
                .Size(size)
                .From(from)
                .Query(queryGetter => query)
                .Aggregations(aggs)
            );

            // process response

            return searchResultsList;
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
                        x => x.Index(percolatorIndex)
                            .Query(q =>
                                q.Percolate(p => p.Documents(response.Source)
                                    .Field(f => f.Query)
                                )
                            )
                    );

                    if (searchResponse.IsValid && searchResponse.Hits.Any())
                    {
                        return searchResponse.Hits?.Select(x => x.Source.ChannelId).ToList();
                    }
                }
                catch (Exception ex)
                {
                    log.Error(
                        "Error - " + string.Format("GetMediaChannels - Could not parse response. Ex={0}, ST: {1}",
                            ex.Message, ex.StackTrace), ex);
                }
            }

            return result;
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
                var searchResult = _elasticClient.Search<object>(searchDescriptor => searchDescriptor
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
                );

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
                                    Match(match => match.Field(field).Query(value)));
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

                            if (definitions.AssetUserRuleId > 0)
                            {
                                var termContainer = queryContainerDescriptor.Term(term => term.
                                    Field(c => c.AssetUserRuleId).
                                    Value(definitions.AssetUserRuleId)
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
                                          var languagesTerms = CreateTagValueBool(definitions, _catalogGroupCache.LanguageMapByCode.Values.ToList());

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
                        if (_catalogGroupCache.LanguageMapByCode[value.Key].IsDefault)
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
                    field = $"value.{language.Code}";
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
                if (!_catalogGroupCache.LanguageMapById.ContainsKey(tagValue.languageId))
                {
                    log.WarnFormat("Found tag value with non existing language ID. tagId = {0}, tagText = {1}, languageId = {2}",
                        tagValue.tagId, tagValue.value, tagValue.languageId);
                }
                else
                {
                    var languageCode = _catalogGroupCache.LanguageMapById[tagValue.languageId].Code;
                    var tag = new NestTag(tagValue, languageCode);
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
                    int languageId = 0;

                    if (_catalogGroupCache.LanguageMapByCode.ContainsKey(languageContainer.m_sLanguageCode3))
                    {
                        languageId = _catalogGroupCache.LanguageMapByCode[languageContainer.m_sLanguageCode3].ID;

                        if (languageId > 0)
                        {
                            var tag = new NestTag(tagValue.tagId, tagValue.topicId, languageId, languageContainer.m_sValue, languageContainer.m_sLanguageCode3, tagValue.createDate, tagValue.updateDate);
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
            throw new NotImplementedException();
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
                         .Analyzers(an => an.Custom(LOWERCASE_ANALYZER,
                        ca => ca
                        .CharFilters("html_strip")
                        .Tokenizer("keyword")
                        .Filters("lowercase", "asciifolding")
                     ))))
                 .Map<object>(map => map
                     .AutoMap<NestTag>()
                     .Properties(properties => properties
                        .Text(name => name.Name("name").Analyzer(LOWERCASE_ANALYZER))
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
                            DocID = nestObject.id,
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
                            DocID = nestObject.id,
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
                        .Fields("country_id", "name", "code"))
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
            var isNewEpgIngest = _groupsFeatures.GetGroupFeatureStatus(_partnerId, GroupFeature.EPG_INGEST_V2);
            if (isNewEpgIngest)
            {
                return searchResponse.Fields.Select(x => x.Value<string>("document_id")).Distinct().ToList();
            }

            var resultEpgIds = searchResponse.Fields.Select(x => x.Value<long>("epg_id")).Distinct().ToList();
            return resultEpgIds.Select(epgId => GetEpgCbKey(epgId)).ToList();
        }

        private SearchDescriptor<NestEpg> GetChannelProgramsSearchDescriptor(int channelId, DateTime startDate, DateTime endDate, List<ESOrderObj> esOrderObjs, string index)
        {
            return new SearchDescriptor<NestEpg>()
                            .Index(index)
                            .Size(_maxResults)
                            .Fields(sf => sf.Fields(fs => fs.DocumentId, fs => fs.EpgID))
                            .Source(false)
                            .Query(q => q
                                .Bool(b => b.Filter(f => f
                                        .Bool(b1 =>
                                            b1.Must(
                                                m => m.Terms(t => t.Field(f1 => f1.ChannelID).Terms(channelId)),
                                                m => m.DateRange(dr => dr.Field(f1 => f1.StartDate).GreaterThanOrEquals(startDate)),
                                                m => m.DateRange(dr => dr.Field(f1 => f1.EndDate).LessThanOrEquals(endDate))
                                            )
                                        )
                                    )
                                )
                            )
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
            var isNewEpgIngestEnabled =
                _groupsFeatures.GetGroupFeatureStatus(_partnerId, GroupFeature.EPG_INGEST_V2);

            if (isNewEpgIngestEnabled && !isAddAction)
            {
                // elasticsearch holds the current document in CB so we go there to take it
                return  GetEpgCBDocumentIdsByEpgId(epgIds, langCodes);
            }

            result.AddRange(IndexManagerCommonHelpers.GetEpgsCBKeysV1(epgIds, langCodes));

            return result;
        }



        private string GetEpgCbKey(long epgId, string langCode = null, bool isAddAction = false)
        {
            var langs = string.IsNullOrEmpty(langCode) ? null : new[] {new LanguageObj {Code = langCode}};
            var keys = GetEpgsCbKeys(new[] {epgId}, langs, isAddAction);
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
                .Fields(sf => sf.Fields(fs => fs.DocumentId, fs => fs.EpgID))
                .Query(q => q.Bool(b =>
                        b.Filter(f =>
                            f.Bool(b2 =>
                                b2.Must(
                                    m =>m.Terms(t => t.Field(f1 => f1.EpgID).Terms(epgIdsList)),
                                    m=>m.Terms(t=>t.Field(f1=>f1.LanguageId).Terms(languages.Select(x=>x.ID)))
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
            var resultEpgDocIds = searchResult.Fields.Select(x => x.Value<string>("document_id")).Distinct().ToList();
            
            var except = epgIdsList.Except(resultEpgIds);
            if (except.Any())
            {
                resultEpgDocIds.AddRange(IndexManagerCommonHelpers.GetEpgsCBKeysV1(epgIdsList, languages));
            }

            return  resultEpgDocIds.Distinct().ToList();
        }

        public string SetupMediaIndex()
        {
            string newIndexName = NamingHelper.GetNewMediaIndexName(_partnerId);
            bool isIndexCreated = CreateMediaIndex(newIndexName);
            
            if (!isIndexCreated)
            {
                log.Error(string.Format("Failed creating index for index:{0}", newIndexName));
                return string.Empty;
            }

            return newIndexName;
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
            var defaultLanguage = GetDefaultLanguage();

            // get definitions of analyzers, filters and tokenizers
            GetAnalyzersWithLowercaseAndPhraseStartsWith(languages, out var analyzers, out var filters, out var tokenizers);
            AnalyzersDescriptor analyzersDescriptor = GetAnalyzersDesctiptor(analyzers);
            TokenFiltersDescriptor filtersDescriptor = GetTokenFiltersDescriptor(filters);
            TokenizersDescriptor tokenizersDescriptor = GetTokenizersDesctiptor(tokenizers);
            _groupManager.RemoveGroup(_partnerId); // remove from cache
            _group = _groupManager.GetGroup(_partnerId);

            if (!GetMetasAndTagsForMapping(
                out var metas,
                out var tags,
                out var metasToPad))
            {
                throw new Exception($"failed to get metas and tags");
            }

            PropertiesDescriptor<object> propertiesDescriptor =
                GetMediaPropertiesDescriptor(languages, metas, tags, metasToPad, analyzers);
            var createResponse = _elasticClient.Indices.Create(newIndexName,
                c => c.Settings(settings => settings
                    .NumberOfShards(_numOfShards)
                    .NumberOfReplicas(_numOfReplicas)
                    .Setting("index.max_result_window", _maxResults)
                    .Setting("index.max_ngram_diff", _applicationConfiguration.ElasticSearchHandlerConfiguration.MaxNgramDiff.Value)
                    .Setting("index.mapping.total_fields.limit", _applicationConfiguration.ElasticSearchHandlerConfiguration.TotalFieldsLimit.Value)
                    .Analysis(a => a
                        .Analyzers(an => analyzersDescriptor)
                        .TokenFilters(tf => filtersDescriptor)
                        .Tokenizers(t => tokenizersDescriptor)
                    ))
                .Map(map =>
                {
                    if (shouldAddPercolators)
                    {
                        map = map.AutoMap<NestPercolatedQuery>();
                    }

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

        public string SetupChannelPercolatorIndex()
        {
            string percolatorsIndexName = NamingHelper.GetNewChannelPercolatorIndex(_partnerId);
            bool isIndexCreated = CreateMediaIndex(percolatorsIndexName, true);

            if (!isIndexCreated)
            {
                log.Error(string.Format("Failed creating index for index:{0}", percolatorsIndexName));
                return string.Empty;
            }

            return percolatorsIndexName;
        }

        public void PublishChannelPercolatorIndex(string newIndexName, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            string alias = NamingHelper.GetChannelPercolatorIndexAlias(_partnerId);
            this.SwitchIndexAlias(newIndexName, alias, shouldSwitchIndexAlias, shouldDeleteOldIndices);
        }

        public void InsertMedias(Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> groupMedias, string newIndexName)
        {
            var sizeOfBulk = GetBulkSize();

            log.DebugFormat("Start indexing medias. total medias={0}", groupMedias.Count);
            // save current value to restore at the end
            var currentDefaultConnectionLimit = ServicePointManager.DefaultConnectionLimit;
            try
            {
                var maxDegreeOfParallelism = GetMaxDegreeOfParallelism();
                var options = new ParallelOptions() {MaxDegreeOfParallelism = maxDegreeOfParallelism};
                var contextData = new ContextData();
                ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount;
                
                // For each media
                var bulkRequests = GetMediaBulkRequests(groupMedias, newIndexName, sizeOfBulk);
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
            int sizeOfBulk)
        {
            var numOfBulkRequests = 0;
            var bulkRequests = new Dictionary<int, List<NestEsBulkRequest<NestMedia>>>(){{numOfBulkRequests, new List<NestEsBulkRequest<NestMedia>>()}};
            
            foreach (var groupMedia in groupMedias)
            {
                var groupMediaValue = groupMedia.Value;
                numOfBulkRequests = GetMediaNestEsBulkRequest(newIndexName, sizeOfBulk, numOfBulkRequests, bulkRequests, groupMediaValue);
            }

            return bulkRequests;
        }

        private int GetMediaNestEsBulkRequest(string newIndexName, int sizeOfBulk, int numOfBulkRequests, Dictionary<int, List<NestEsBulkRequest<NestMedia>>> bulkRequests, Dictionary<int, ApiObjects.SearchObjects.Media> groupMediaValue)
        {
            // For each language
            foreach (var languageId in groupMediaValue.Keys.Distinct())
            {
                var language = GetLanguageById(languageId);
                var media = groupMediaValue[languageId];

                if (media == null)
                    continue;

                media.PadMetas(_metasToPad);

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
            var docId = $"{nestMedia.MediaId}_{language.Code}";
            return new NestEsBulkRequest<NestMedia>(docId, indexName, nestMedia);
        }

        private static int GetMaxDegreeOfParallelism()
        {
            var maxDegreeOfParallelism = ApplicationConfiguration.Current.RecordingsMaxDegreeOfParallelism.Value;
            if (maxDegreeOfParallelism == 0)
                maxDegreeOfParallelism = 5;
            return maxDegreeOfParallelism;
        }

        public void PublishMediaIndex(string newIndexName, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            string alias = NamingHelper.GetMediaIndexAlias(_partnerId);

            SwitchIndexAlias(newIndexName, alias, shouldDeleteOldIndices, shouldSwitchIndexAlias);
        }

        public bool AddChannelsPercolatorsToIndex(HashSet<int> channelIds, string newIndexName, bool shouldCleanupInvalidChannels = false)
        {
            bool result = false;

            if (string.IsNullOrEmpty(newIndexName))
            {
                newIndexName = NamingHelper.GetMediaIndexAlias(_partnerId);
            }

            try
            {
                List<Channel> groupChannels = IndexManagerCommonHelpers.GetGroupChannels(_partnerId, _channelManager, _groupUsesTemplates, ref channelIds);

                var mediaQueryParser = new ESMediaQueryBuilder() { QueryType = eQueryType.EXACT };
                var unifiedQueryBuilder = new ESUnifiedQueryBuilder(null, _partnerId);

                var bulkRequests = new List<NestEsBulkRequest<NestPercolatedQuery>>();
                int sizeOfBulk = 50;

                foreach (var channel in groupChannels)
                {
                    var query = _channelQueryBuilder.GetChannelQuery(mediaQueryParser, unifiedQueryBuilder, channel);

                    bulkRequests.Add(new NestEsBulkRequest<NestPercolatedQuery>()
                    {
                        DocID = GetChannelDocumentId(channel),
                        Document = query,
                        Index = newIndexName,
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

                result = true;
            }
            catch (Exception ex)
            {
                log.Error($"Error when indexing percolators on partner {_partnerId}", ex);
            }

            return result;
        }


        public void AddChannelsMetadataToIndex(string newIndexName, List<Channel> allChannels)
        {
            var bulkList = new List<NestEsBulkRequest<NestChannelMetadata>>();
            var sizeOfBulk = GetBulkSize(50);
            var cd = new ContextData();
            var channelMetadatas = allChannels.Select(x=>NestDataCreator.GetChannelMetadata(x));
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

        public void PublishChannelsMetadataIndex(string newIndexName, bool shouldSwitchAlias,bool shouldDeleteOldIndices)
        {
            var alias = NamingHelper.GetChannelMetadataIndexName(_partnerId);
            SwitchIndexAlias(newIndexName, alias, shouldDeleteOldIndices, shouldSwitchAlias);
        }

        private string SetupIndex<T>(string newIndexName, List<string> multilingualFields, List<string> simpleFields)
        where T : class 
        {
            Dictionary<string, Analyzer> analyzers;
            Dictionary<string, Tokenizer> tokenizers;
            Dictionary<string, ElasticSearch.Searcher.Settings.Filter> filters;
            var languages = GetLanguages();
            GetAnalyzersWithLowercase(languages.ToList(), out analyzers, out filters, out tokenizers);
            var analyzersDescriptor = GetAnalyzersDesctiptor(analyzers);
            var filtersDescriptor = GetTokenFiltersDescriptor(filters);
            var tokenizersDescriptor = GetTokenizersDesctiptor(tokenizers);

            var createResponse = _elasticClient.Indices.Create(newIndexName,
                c => c.Settings(settings => settings
                        .NumberOfShards(_numOfShards)
                        .NumberOfReplicas(_numOfReplicas)
                        .Setting("index.max_result_window", _maxResults)
                        .Setting("index.max_ngram_diff", _applicationConfiguration.ElasticSearchHandlerConfiguration.MaxNgramDiff.Value)
                        .Analysis(a => a
                            .Analyzers(an => analyzersDescriptor)
                            .TokenFilters(tf => filtersDescriptor)
                            .Tokenizers(t => tokenizersDescriptor)
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


        public string SetupChannelMetadataIndex() 
        {
            return SetupIndex<NestChannelMetadata>(NamingHelper.GetNewChannelMetadataIndexName(_partnerId), null, new List<string>() { "name" });
        }

        public string SetupTagsIndex()
        {
            return SetupIndex<NestTag>(NamingHelper.GetNewMetadataIndexName(_partnerId), new List<string>() { "value" }, null);
        }

        public void InsertTagsToIndex(string newIndexName, List<TagValue> allTagValues)
        {
            var sizeOfBulk = GetBulkSize(50);
            var bulkRequests = new List<NestEsBulkRequest<NestTag>>();
            try
            {
                foreach (var tagValue in allTagValues)
                {
                    if (!_catalogGroupCache.LanguageMapById.ContainsKey(tagValue.languageId))
                    {
                        log.WarnFormat("Found tag value with non existing language ID. tagId = {0}, tagText = {1}, languageId = {2}",
                            tagValue.tagId, tagValue.value, tagValue.languageId);
                        continue;
                    }

                    var languageCode = _catalogGroupCache.LanguageMapById[tagValue.languageId].Code;

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

        public string SetupEpgIndex(bool isRecording)
        {
            var indexName = NamingHelper.GetNewEpgIndexName(_partnerId);

            if (isRecording)
            {
                indexName = NamingHelper.GetNewRecordingIndexName(_partnerId);
            }

            CreateNewEpgIndex(indexName, isRecording);
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

                        epgCb.PadMetas(_metasToPad);

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

                        // Serialize EPG object to string
                        long? recodingId=null;
                        if (isRecording)
                        {
                            recodingId = epgToRecordingMapping[(int) epgCb.EpgID];
                        }

                        var expiry = GetEpgExpiry(epgCb);
                        var epg = NestDataCreator.GetEpg(epgCb,
                            language.ID,
                            true,
                            _groupUsesTemplates,
                            recordingId: recodingId,
                            expiryUnixTimeStamp: expiry);
                        
                        var documentId = GetEpgDocumentId(epgCb, isRecording, epgToRecordingMapping);
                        
                        // If we exceeded the size of a single bulk reuquest then create another list
                        if (bulkRequests[numOfBulkRequests].Count >= sizeOfBulk)
                        {
                            numOfBulkRequests++;
                            bulkRequests.Add(numOfBulkRequests, new List<NestEsBulkRequest<NestEpg>>());
                        }
                        var bulkRequest = GetEpgBulkRequest(index, epgCb.StartDate, epg,documentId);
                         
                        bulkRequests[numOfBulkRequests].Add(bulkRequest);
                    }
                }

                var maxDegreeOfParallelism = GetMaxDegreeOfParallelism();
                var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism };
                var contextData = new ContextData();
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
        private string GetEpgDocumentId(EpgCB epg, bool isRecording, Dictionary<long, long> epgToRecordingMapping)
        {
            if (isRecording)
                return epgToRecordingMapping[(long)epg.EpgID].ToString();
            return epg.EpgID.ToString();
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

            Dictionary<long, List<int>> linearChannelsRegionsMapping = null;

            if (_groupUsesTemplates ? _catalogGroupCache.IsRegionalizationEnabled : _group.isRegionalizationEnabled)
            {
                linearChannelsRegionsMapping = RegionManager.GetLinearMediaRegions(_partnerId);
            }

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

            var isIngestV2 = GroupSettingsManager.DoesGroupUseNewEpgIngest(_partnerId);

            _catalogManager.GetLinearChannelValues(epgObjects, _partnerId, _ => { });

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
                        if (!isRecording && isIngestV2)
                        {
                            alias = NamingHelper.GetDailyEpgIndexName(_partnerId, epgCb.StartDate.Date);
                        }

                        epgCb.PadMetas(_metasToPad);
                        UpdateEpgLinearMediaId(linearChannelSettings, epgCb);
                        UpdateEpgRegions(epgCb, linearChannelsRegionsMapping);

                        long? recodingId=null;
                        if (isRecording && epgToRecordingMapping!=null )
                        {
                            recodingId = epgToRecordingMapping[(int) epgCb.EpgID];
                        }

                        var shouldSetTtl = !isRecording;
                        long? expiry = null;
                        if (shouldSetTtl)
                        {
                            expiry = GetEpgExpiry(epgCb);
                        }

                        var epg = NestDataCreator.GetEpg(epgCb,
                            language.ID,
                            true,
                            _groupUsesTemplates,
                            expiry,
                            recodingId);

                        var documentId = GetEpgDocumentId(epgCb, isRecording, epgToRecordingMapping);
                        var bulkRequest = GetEpgBulkRequest(alias, epgCb.StartDate.ToUniversalTime(), epg,documentId);
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
            return _partnerLanguageCodes.Values.ToList();
        }

        private LanguageObj GetDefaultLanguage()
        {
            return _groupUsesTemplates ? _catalogGroupCache.GetDefaultLanguage() : _group.GetGroupDefaultLanguage();
        }
        
        private LanguageObj GetLanguageByCode(string languageCode)
        {
            return _partnerLanguageCodes[languageCode];
        }
        
        private int GetLanguageIdByCode(string languageCode)
        {
            return GetLanguageByCode(languageCode).ID;
        }

        private LanguageObj GetLanguageById(int id)
        {
            return GetLanguages().FirstOrDefault(x => x.ID == id);
        }

        private void EnsureEpgIndexExist(string dailyEpgIndexName)
        {
            // TODO it's possible to create new index with mappings and alias in one request,
            // https://www.elastic.co/guide/en/elasticsearch/reference/2.3/indices-create-index.html#mappings
            // but we have huge mappings and don't know the impact on timeouts during index creation - should be tested on real environment.

            // Limitation: it's not a transaction, we don't remove index when add-mapping failed =>
            // EPGs could be added to the index without mapping (e.g. from asset.add)
            try
            {
                AddEmptyIndex(dailyEpgIndexName);
                AddAlias(dailyEpgIndexName);
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

        private void AddEmptyIndex(string dailyEpgIndexName)
        {
            IndexManagerCommonHelpers.GetRetryPolicy<Exception>().Execute(() =>
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
            bool shouldUseNumOfConfiguredShards = true, int? refreshIntervalSeconds = null)
        {
            List<LanguageObj> languages = GetLanguages();
            GetAnalyzersWithLowercaseAndPhraseStartsWith(languages, out var analyzers, out var filters, out var tokenizers);
            int replicas = shouldBuildWithReplicas ? _numOfReplicas : 0;
            int shards = shouldUseNumOfConfiguredShards ? _numOfShards : 1;
            var isIndexCreated = false;
            AnalyzersDescriptor analyzersDescriptor = GetAnalyzersDesctiptor(analyzers);
            TokenFiltersDescriptor filtersDesctiptor = GetTokenFiltersDescriptor(filters);
            TokenizersDescriptor tokenizersDescriptor = GetTokenizersDesctiptor(tokenizers);

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

            PropertiesDescriptor<object> propertiesDescriptor = GetEpgPropertiesDescriptor(languages, metas, tags, metasToPad, analyzers);
            var createResponse = _elasticClient.Indices.Create(newIndexName,
                c => c
                    .Settings(settings => {
                        settings
                        .NumberOfShards(shards)
                        .NumberOfReplicas(replicas)
                        .Setting("index.max_result_window", _maxResults)
                        .Setting("index.max_ngram_diff", _applicationConfiguration.ElasticSearchHandlerConfiguration.MaxNgramDiff.Value)
                        .Setting("index.mapping.total_fields.limit", _applicationConfiguration.ElasticSearchHandlerConfiguration.TotalFieldsLimit.Value)
                        .Analysis(a => a
                            .Analyzers(an => analyzersDescriptor)
                            .TokenFilters(tf => filtersDesctiptor)
                            .Tokenizers(t => tokenizersDescriptor)
                        );

                        if (refreshIntervalSeconds.HasValue)
                        {
                            settings.RefreshInterval(new Time(refreshIntervalSeconds.Value, TimeUnit.Second));
                        }

                        return settings;
                    })
                    .Map(x => x.AutoMap())
                .Map(map => map.RoutingField(rf => new RoutingField() { Required = false }).Properties(props => propertiesDescriptor)
                ));

            isIndexCreated = createResponse != null && createResponse.Acknowledged && createResponse.IsValid;
            if (!isIndexCreated) { throw new Exception(string.Format("Failed creating index for index:{0}", newIndexName)); }
        }

        private PropertiesDescriptor<object> GetEpgPropertiesDescriptor(List<LanguageObj> languages,
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
                .Text(x => x.Name("date_routing"))
                .Number(x => x.Name("media_type_id").Type(NumberType.Integer).NullValue(0))
                .Number(x => x.Name("language_id").Type(NumberType.Long))
                .Keyword(x => InitializeDefaultTextPropertyDescriptor<string>("epg_identifier"))
                .Date(x => x.Name("start_date"))
                .Date(x => x.Name("end_date"))
                .Date(x => x.Name("cache_date"))
                .Date(x => x.Name("create_date"))
                .Keyword(x => InitializeDefaultTextPropertyDescriptor<string>("crid"))
                .Keyword(x => InitializeDefaultTextPropertyDescriptor<string>("external_id"))
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

        private static void InitializeNumericMetaField<K>(PropertiesDescriptor<K> propertiesDescriptor,
            string metaName,
            bool shouldAddPaddedField)
            where K : class
        {
            var lowercaseSubField = new TextPropertyDescriptor<object>()
                .Name("lowercase")
                .Analyzer(LOWERCASE_ANALYZER)
                .SearchAnalyzer(LOWERCASE_ANALYZER);

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

        private KeywordPropertyDescriptor<K> InitializeTextField<K>(
            string nameFieldName,
            PropertiesDescriptor<K> propertiesDescriptor,
            string indexAnalyzer,
            string searchAnalyzer,
            string autocompleteAnalyzer,
            string autocompleteSearchAnalyzer,
            bool shouldAddPhraseAutocompleteField,
            string phoneticIndexAnalyzer = null,
            string phoneticSearchAnalyzer = null,
            bool shouldAddPhoneticField = false,
            bool shouldAddPaddedField = false)
            where K : class
        {
            var lowercaseSubField = new TextPropertyDescriptor<object>()
                .Name("lowercase")
                .Analyzer(LOWERCASE_ANALYZER)
                .SearchAnalyzer(LOWERCASE_ANALYZER)
                ;

            var autocompleteSubField = new TextPropertyDescriptor<object>()
                .Name("autocomplete")
                .Analyzer(autocompleteAnalyzer)
                .SearchAnalyzer(autocompleteSearchAnalyzer);

            var analyzedField = new TextPropertyDescriptor<object>()
                .Name("analyzed")
                .Analyzer(indexAnalyzer)
                .SearchAnalyzer(searchAnalyzer);

            PropertiesDescriptor<object> fieldsPropertiesDesctiptor = new PropertiesDescriptor<object>()
                .Keyword(y => y.Name(nameFieldName))
                .Text(y => lowercaseSubField)
                .Text(y => autocompleteSubField)
                .Text(y => analyzedField)
                ;

            if (shouldAddPhraseAutocompleteField)
            {
                var phraseAutocompleteSubField = new TextPropertyDescriptor<object>()
                    .Name("phrase_autocomplete")
                    .Analyzer(PHRASE_STARTS_WITH_ANALYZER)
                    .SearchAnalyzer(PHRASE_STARTS_WITH_SEARCH_ANALYZER);

                fieldsPropertiesDesctiptor
                    .Text(y => phraseAutocompleteSubField);
            }

            if (shouldAddPhoneticField && !string.IsNullOrEmpty(phoneticIndexAnalyzer) && !string.IsNullOrEmpty(phoneticSearchAnalyzer))
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
            var keywordPropertyDescriptor = new KeywordPropertyDescriptor<K>()
                .Name(nameFieldName)
                .Fields(fields => fieldsPropertiesDesctiptor)
                ;
            propertiesDescriptor.Keyword(x => keywordPropertyDescriptor);

            return keywordPropertyDescriptor;
        }

        private KeywordPropertyDescriptor<K> InitializeDefaultTextPropertyDescriptor<K>(string fieldName) 
            where K : class
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

            return new KeywordPropertyDescriptor<K>()
                .Name(fieldName)
                .Fields(fields => fields
                    .Text(y => y
                        .Name(fieldName)
                        .SearchAnalyzer(LOWERCASE_ANALYZER)
                        .Analyzer(LOWERCASE_ANALYZER)
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

        private void GetAnalyzersWithLowercaseAndPhraseStartsWith(IEnumerable<LanguageObj> languages,
            out Dictionary<string, Analyzer> analyzers,
            out Dictionary<string, ElasticSearch.Searcher.Settings.Filter> filters,
            out Dictionary<string, Tokenizer> tokeniezrs)
        {
            analyzers = new Dictionary<string, Analyzer>();
            filters = new Dictionary<string, ElasticSearch.Searcher.Settings.Filter>();
            tokeniezrs = new Dictionary<string, Tokenizer>();

            if (languages != null)
            {
                GetAnalyzersWithLowercase(languages, out analyzers, out filters, out tokeniezrs);

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
            out Dictionary<string, ElasticSearch.Searcher.Settings.Filter> filters,
            out Dictionary<string, Tokenizer> tokenizers)
        {
            GetAnalyzersAndFiltersFromConfiguration(languages, out analyzers, out filters, out tokenizers);

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
            out Dictionary<string, ElasticSearch.Searcher.Settings.Filter> filters,
            out Dictionary<string, Tokenizer> tokenizers)
        {
            analyzers = new Dictionary<string, Analyzer>();
            filters = new Dictionary<string, ElasticSearch.Searcher.Settings.Filter>();
            tokenizers = new Dictionary<string, Tokenizer>();
            SetDefaultAnalyzersAndFilters(analyzers, filters);

            foreach (LanguageObj language in languages)
            {
                var currentAnalyzers = _esIndexDefinitions.GetAnalyzers(ElasticsearchVersion.ES_7_13, language.Code);
                var currentFilters = _esIndexDefinitions.GetFilters(ElasticsearchVersion.ES_7_13, language.Code);
                var currentTokenizers = _esIndexDefinitions.GetTokenizers(ElasticsearchVersion.ES_7_13, language.Code);

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

                if (currentTokenizers != null)
                {
                    foreach (var tokenizer in currentTokenizers)
                    {
                        tokenizers[tokenizer.Key] = tokenizer.Value;
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
                    $"{defaultLanguage.Code}_edgengram_filter"
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
                },
                tokenizer = "keyword"
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

            if (_groupUsesTemplates && _catalogGroupCache != null)
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

                    if (_group.m_oEpgGroupSettings != null && _group.m_oEpgGroupSettings.m_lMetasName != null)
                    {
                        foreach (string epgMeta in _group.m_oEpgGroupSettings.m_lMetasName)
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

            return result;
        }

        private void AddAlias(string dailyEpgIndexName)
        {
            // create alias is idempotent request
            var epgIndexAlias = NamingHelper.GetEpgIndexAlias(_partnerId);
            log.Info($"creating alias. index [{dailyEpgIndexName}], alias [{epgIndexAlias}]");
            IndexManagerCommonHelpers.GetRetryPolicy<Exception>().Execute(() =>
            {
                var putAliasResponse = _elasticClient.Indices.PutAlias(dailyEpgIndexName, epgIndexAlias);
                bool isAliasAdded = putAliasResponse != null && putAliasResponse.IsValid;
                if (!isAliasAdded) throw new Exception($"index set alias failed [{dailyEpgIndexName}], alias [{epgIndexAlias}]");
            });
        }


        private PropertiesDescriptor<object> GetMediaPropertiesDescriptor(List<LanguageObj> languages,
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
                ;
            
            InitializeTextField("external_id", propertiesDescriptor, 
                defaultIndexAnalyzer, defaultSearchAnalyzer, defaultAutocompleteAnalyzer, defaultAutocompleteSearchAnalyzer, true);
            InitializeTextField("entry_id", propertiesDescriptor, 
                defaultIndexAnalyzer, defaultSearchAnalyzer, defaultAutocompleteAnalyzer, defaultAutocompleteSearchAnalyzer, true);

            if (!metas.ContainsKey(META_SUPPRESSED))
            {
                metas.Add(META_SUPPRESSED, new KeyValuePair<eESFieldType, string>(eESFieldType.STRING, string.Empty));//new meta for suppressed value
            }

            AddLanguageSpecificMappingToPropertyDescriptor(languages, metas, tags, metasToPad, analyzers, propertiesDescriptor,
                defaultIndexAnalyzer, defaultSearchAnalyzer, defaultAutocompleteAnalyzer, defaultAutocompleteSearchAnalyzer);

            return propertiesDescriptor;
        }

        private PropertiesDescriptor<T> GetPropertiesDescriptor<T>(PropertiesDescriptor<T> propertiesDescriptor,
            List<LanguageObj> languages, 
            Dictionary<string, Analyzer> analyzers, List<string> multilingualFields, List<string> simpleFields)
        where T :class
        {
            if (multilingualFields != null)
            {
                foreach (var field in multilingualFields)
                {
                    var fieldPropertiesDescriptor = new PropertiesDescriptor<object>();

                    var defaultLanguage = GetDefaultLanguage();
                    string defaultIndexAnalyzer = $"{defaultLanguage.Code}_index_analyzer";
                    string defaultSearchAnalyzer = $"{defaultLanguage.Code}_search_analyzer";
                    string defaultAutocompleteAnalyzer = $"{defaultLanguage.Code}_autocomplete_analyzer";
                    string defaultAutocompleteSearchAnalyzer = $"{defaultLanguage.Code}_autocomplete_search_analyzer";

                    foreach (var language in languages)
                    {
                        GetCurrentLanguageAnalyzers(analyzers,
                            defaultIndexAnalyzer, defaultSearchAnalyzer, defaultAutocompleteAnalyzer, defaultAutocompleteSearchAnalyzer,
                            language,
                            out string indexAnalyzer, out string searchAnalyzer,
                            out string autocompleteAnalyzer, out string autocompleteSearchAnalyzer,
                            out _, out _);

                        InitializeTextField($"{language.Code}",
                            fieldPropertiesDescriptor,
                            indexAnalyzer, searchAnalyzer, autocompleteAnalyzer, autocompleteSearchAnalyzer, false);
                    }

                    propertiesDescriptor.Object<object>(x => x
                        .Name(field)
                        .Properties(properties => fieldPropertiesDescriptor))
                    ;
                }
            }

            if (simpleFields != null)
            {
                var defaultLanguage = GetDefaultLanguage();
                var defaultLanguageCode = defaultLanguage.Code;

                foreach (var field in simpleFields)
                {
                    propertiesDescriptor.Keyword(t => 
                        InitializeTextField<T>(field, propertiesDescriptor, 
                            $"{defaultLanguageCode}_{DEFAULT_INDEX_ANALYZER}", $"{defaultLanguageCode}_{DEFAULT_SEARCH_ANALYZER}", 
                            $"{defaultLanguageCode}_{AUTOCOMPLETE_ANALYZER}", $"{defaultLanguageCode}_{AUTOCOMPLETE_SEARCH_ANALYZER}", false));
                }
            }

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
                string indexAnalyzer, searchAnalyzer, autocompleteAnalyzer, autocompleteSearchAnalyzer, 
                    phoneticIndexAnalyzer, phoneticSearchAnalyzer;
                GetCurrentLanguageAnalyzers(analyzers, 
                    defaultIndexAnalyzer, defaultSearchAnalyzer, defaultAutocompleteAnalyzer, defaultAutocompleteSearchAnalyzer, 
                    language, 
                    out indexAnalyzer, out searchAnalyzer, 
                    out autocompleteAnalyzer, out autocompleteSearchAnalyzer, 
                    out phoneticIndexAnalyzer, out phoneticSearchAnalyzer);

                bool shouldAddPhoneticField = analyzers.ContainsKey(phoneticIndexAnalyzer) && analyzers.ContainsKey(phoneticSearchAnalyzer);

                InitializeTextField(
                    $"{language.Code}",
                    namePropertiesDescriptor,
                    indexAnalyzer,
                    searchAnalyzer,
                    autocompleteAnalyzer,
                    autocompleteSearchAnalyzer,
                    true,
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
                    true,
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
                        true,
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
                                true,
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

        private static void GetCurrentLanguageAnalyzers(Dictionary<string, Analyzer> analyzers, 
            string defaultIndexAnalyzer, 
            string defaultSearchAnalyzer, 
            string defaultAutocompleteAnalyzer, 
            string defaultAutocompleteSearchAnalyzer, 
            LanguageObj language, 
            out string indexAnalyzer, out string searchAnalyzer, 
            out string autocompleteAnalyzer, out string autocompleteSearchAnalyzer, 
            out string phoneticIndexAnalyzer, out string phoneticSearchAnalyzer)
        {
            indexAnalyzer = $"{language.Code}_index_analyzer";
            searchAnalyzer = $"{language.Code}_search_analyzer";
            autocompleteAnalyzer = $"{language.Code}_autocomplete_analyzer";
            autocompleteSearchAnalyzer = $"{language.Code}_autocomplete_search_analyzer";
            phoneticIndexAnalyzer = $"{language.Code}_index_dbl_metaphone";
            phoneticSearchAnalyzer = $"{language.Code}_search_dbl_metaphone";
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
        }

        private void CreateNewEpgIndex(string newIndexName, bool isRecording = false, bool shouldBuildWithReplicas = true, bool shouldUseNumOfConfiguredShards = true,
            int? refreshInterval = null)
        {
            CreateEmptyEpgIndex(newIndexName, shouldBuildWithReplicas, shouldUseNumOfConfiguredShards, refreshInterval);
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