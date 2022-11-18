using ApiObjects;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Catalog.Response;
using Phx.Lib.Appconfig;
using Core.Catalog.Cache;
using Core.Catalog.Response;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using GroupsCacheManager;
using Phx.Lib.Log;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ApiLogic.Catalog.IndexManager.GroupBy;
using TVinciShared;
using ApiObjects.Statistics;
using System.Net;
using System.Net.Sockets;
using ApiLogic.Api.Managers;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Api.Managers;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Request;
using Core.GroupManagers;
using ElasticSearch.Common.DeleteResults;
using ElasticSearch.Utilities;
using Tvinci.Core.DAL;
using Channel = GroupsCacheManager.Channel;
using Polly.Retry;
using ApiObjects.BulkUpload;
using Polly;
using ESUtils = ElasticSearch.Common.Utils;
using ApiLogic.Catalog;
using ApiLogic.IndexManager;
using ElasticSearch.Common.Mappers;
using EpgBL;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.QueryBuilders;
using ApiLogic.IndexManager.Mappings;
using ApiLogic.IndexManager.Models;
using ApiLogic.IndexManager.Sorting;
using ApiLogic.IndexManager.Sorting.Stages;
using ApiLogic.IndexManager.Transaction;
using Core.Api;
using Core.Catalog.Searchers;
using ElasticSearch.Utils;
using MongoDB.Driver.Core.Operations;
using System.Diagnostics;
using ApiLogic.EPG;
using ApiObjects.Epg;

namespace Core.Catalog
{
    public partial class IndexManagerV2 : IIndexManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string MEDIA = "media";
        private const string CHANNEL = "channel";
        private const string PERCOLATOR = ".percolator";

        protected const string CHANNEL_SEARCH_IS_ACTIVE = "is_active";

        protected const string CHANNEL_SEARCH_IS_ACTIVE_VALUE = "1";
        protected const string ES_MEDIA_TYPE = "media";
        protected const string ES_EPG_TYPE = "epg";
        protected const string CHANNEL_ASSET_USER_RULE_ID = "asset_user_rule_id";

        // Basic TCM configurations for indexing - number of shards/replicas, max results
        private static readonly int NUM_OF_SHARDS = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.NumberOfShards.Value;
        private static readonly int NUM_OF_REPLICAS = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.NumberOfReplicas.Value;
        private static readonly int MAX_RESULTS = ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value;
        private static readonly int SIZE_OF_BULK_DEFAULT_VALUE = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.BulkSize.GetDefaultValue();
        private static int SIZE_OF_BULK = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.BulkSize.Value;
        private static readonly ITtlService _ttlService = new TtlService();

        protected const string ES_VERSION = "2";

        public const string EPG_INDEX_TYPE = "epg";
        public const string RECORDING_INDEX_TYPE = "recording";
        public const string TAG_INDEX_TYPE = "tag";
        public const string LOWERCASE_ANALYZER = "\"lowercase_analyzer\": {\"type\": \"custom\",\"tokenizer\": \"keyword\",\"filter\": [\"lowercase\",\"asciifolding\"],\"char_filter\": [\"html_strip\"]}";

        public const string PHRASE_STARTS_WITH_FILTER = "\"edgengram_filter\": {\"type\":\"edgeNGram\",\"min_gram\":1,\"max_gram\":20,\"token_chars\":[\"letter\",\"digit\",\"punctuation\",\"symbol\"]}";
        public const string PHRASE_STARTS_WITH_ANALYZER = "\"phrase_starts_with_analyzer\": {\"type\":\"custom\",\"tokenizer\":\"keyword\",\"filter\":[\"lowercase\",\"edgengram_filter\", \"icu_folding\",\"icu_normalizer\",\"asciifolding\"],\"char_filter\":[\"html_strip\"]}";
        public const string PHRASE_STARTS_WITH_SEARCH_ANALYZER = "\"phrase_starts_with_search_analyzer\": {\"type\":\"custom\",\"tokenizer\":\"keyword\",\"filter\":[\"lowercase\", \"icu_folding\",\"icu_normalizer\",\"asciifolding\"],\"char_filter\":[\"html_strip\"]}";

        public const string EPG_GREEN_SUFFIX = "green";
        public const string EPG_BLUE_SUFFIX = "blue";

        private const string REFRESH_INTERVAL_FOR_EMPTY_INDEX = "10s";
        private const string INDEX_REFRESH_INTERVAL = "10s";

        protected const string VERSION = "2";

        private readonly IElasticSearchApi _elasticSearchApi;
        private readonly IGroupManager _groupManager;
        private readonly ESSerializerV2 _serializer;
        private readonly ICatalogManager _catalogManager;
        private readonly IElasticSearchIndexDefinitions _esIndexDefinitions;
        private readonly ILayeredCache _layeredCache;
        private readonly IChannelManager _channelManager;
        private readonly ICatalogCache _catalogCache;
        private readonly IWatchRuleManager _watchRuleManager;
        private readonly IMappingTypeResolver _mappingTypeResolver;
        private readonly ISortingService _sortingService;
        private readonly IStartDateAssociationTagsSortStrategy _startDateAssociationTagsSortStrategy;
        private readonly IStatisticsSortStrategy _statisticsSortStrategy;
        private readonly ISortingAdapter _sortingAdapter;
        private readonly IEsSortingService _esSortingService;
        private readonly int _partnerId;
        private readonly IChannelQueryBuilder _channelQueryBuilder;
        private readonly INamingHelper _namingHelper;
        private readonly IGroupSettingsManager _groupSettingsManager;
        private readonly IUnifiedQueryBuilderInitializer _queryInitializer;
        private readonly IRegionManager _regionManager;

        /// <summary>
        /// Initialiezs an instance of Index Manager for work with ElasticSearch 2.3. 
        /// Please do not use this ctor, rather use IndexManagerFactory.
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="elasticSearchClient"></param>
        /// <param name="groupManager"></param>
        /// <param name="eSSerializerV2"></param>
        /// <param name="catalogManager"></param>
        /// <param name="esIndexDefinitions"></param>
        /// <param name="layeredCache"></param>
        /// <param name="channelManager"></param>
        /// <param name="catalogCache"></param>
        /// <param name="watchRuleManager"></param>
        /// <param name="channelQueryBuilder"></param>
        /// <param name="mappingTypeResolver"></param>
        /// <param name="namingHelper"></param>
        /// <param name="groupSettingsManager"></param>
        /// <param name="sortingService"></param>
        /// <param name="startDateAssociationTagsSortStrategy"></param>
        /// <param name="statisticsSortStrategy"></param>
        /// <param name="sortingAdapter"></param>
        /// <param name="esSortingService"></param>
        /// <param name="queryInitializer"></param>
        /// <param name="regionManager"></param>
        public IndexManagerV2(int partnerId,
            IElasticSearchApi elasticSearchClient,
            IGroupManager groupManager,
            ESSerializerV2 eSSerializerV2,
            ICatalogManager catalogManager,
            IElasticSearchIndexDefinitions esIndexDefinitions,
            ILayeredCache layeredCache,
            IChannelManager channelManager,
            ICatalogCache catalogCache,
            IWatchRuleManager watchRuleManager,
            IChannelQueryBuilder channelQueryBuilder,
            IMappingTypeResolver mappingTypeResolver,
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
            _elasticSearchApi = elasticSearchClient;
            _groupManager = groupManager;
            _serializer = eSSerializerV2;
            _catalogManager = catalogManager;
            _esIndexDefinitions = esIndexDefinitions;
            _layeredCache = layeredCache;
            _channelManager = channelManager;
            _catalogCache = catalogCache;
            _partnerId = partnerId;
            _watchRuleManager = watchRuleManager;
            _channelQueryBuilder = channelQueryBuilder;
            _mappingTypeResolver = mappingTypeResolver;
            _sortingService = sortingService;
            _namingHelper = namingHelper;
            _groupSettingsManager = groupSettingsManager;
            _startDateAssociationTagsSortStrategy = startDateAssociationTagsSortStrategy;
            _statisticsSortStrategy = statisticsSortStrategy;
            _sortingAdapter = sortingAdapter;
            _esSortingService = esSortingService;
            _queryInitializer = queryInitializer;
            _regionManager = regionManager;
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

        private bool isOpc()
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

        #region Methods from Static IndexManager

        public bool UpsertMedia(long assetId)
        {
            bool result = false;

            if (assetId <= 0)
            {
                log.WarnFormat("Received media request of invalid media id {0} when calling UpsertMedia", assetId);
                return result;
            }

            if (!_elasticSearchApi.IndexExists($"{_partnerId}"))
            {
                log.Error($"Index of type media for group {_partnerId} does not exist");
                return false;
            }

            Dictionary<int, LanguageObj> languagesMap = null;

            var metasToPad = new HashSet<string>();
            if (isOpc())
            {
                var catalogGroupCache = GetCatalogGroupCache();
                languagesMap = new Dictionary<int, LanguageObj>(catalogGroupCache.LanguageMapById);

                metasToPad = catalogGroupCache.TopicsMapById.Values
                    .Where(x => x.Type == ApiObjects.MetaType.Number)
                    .Select(y => y.SystemName.ToLower())
                    .ToHashSet();
            }
            else
            {
                metasToPad = GetMetasToPad();
                var languages = GetGroupManager().GetLangauges();
                languagesMap = languages.ToDictionary(x => x.ID, x => x);
            }

            try
            {
                //Create Media Object
                var mediaDictionary = _catalogManager.GetGroupMedia(_partnerId, assetId);
                if (mediaDictionary != null && mediaDictionary.Count > 0)
                {
                    foreach (int languageId in mediaDictionary.Keys)
                    {
                        LanguageObj language = languagesMap.ContainsKey(languageId) ? languagesMap[languageId] : null;
                        if (language != null)
                        {
                            string suffix = null;
                            if (!language.IsDefault)
                            {
                                suffix = language.Code;
                            }

                            Media media = mediaDictionary[languageId];
                            if (media != null)
                            {
                                media.PadMetas(metasToPad);

                                string serializedMedia = _serializer.SerializeMediaObject(media, suffix);
                                string type = GetTranslationType(MEDIA, language);
                                if (!string.IsNullOrEmpty(serializedMedia))
                                {
                                    result = _elasticSearchApi.InsertRecord(_partnerId.ToString(), type, media.m_nMediaID.ToString(), serializedMedia);
                                    if (!result)
                                    {
                                        log.Error("Error - " + string.Format("Could not update media in ES. GroupID={0};Type={1};MediaID={2};serializedObj={3};",
                                            _partnerId, type, media.m_nMediaID, serializedMedia));
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Upsert Media threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            }

            log.Debug($"Upsert Media result {result}");


            return result;
        }

        public bool DeleteMedia(long assetId)
        {
            bool result = false;
            string index = NamingHelper.GetMediaIndexAlias(_partnerId);

            if (assetId <= 0)
            {
                log.WarnFormat("Received media request of invalid media id {0} when calling DeleteMedia", assetId);
                return result;
            }

            List<LanguageObj> languages = null;
            if (isOpc())
            {
                languages = GetCatalogGroupCache().LanguageMapById.Values.ToList();
            }
            else
            {
                languages = GetGroupManager().GetLangauges();
            }

            try
            {
                if (languages != null && languages.Count > 0)
                {
                    result = true;

                    foreach (LanguageObj lang in languages)
                    {
                        string type = GetTranslationType(MEDIA, lang);
                        ESDeleteResult deleteResult = _elasticSearchApi.DeleteDoc(index, type, assetId.ToString());

                        if (!deleteResult.Found)
                        {
                            log.WarnFormat("IndexManager - DeleteMedia Delete request: delete media with ID {0} and language {1} not found", assetId, lang.Code);
                        }
                        else
                        {
                            if (!deleteResult.Ok)
                            {
                                log.ErrorFormat("IndexManager - DeleteMedia error: Could not delete media from ES. Media id={0} language={1}", assetId, lang.Code);
                            }

                            result = deleteResult.Ok && result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Could not delete media from ES. Media id={0}, ex={1}", assetId, ex);
            }

            if (!result)
            {
                log.ErrorFormat("Delete media with id {0} failed", assetId);
            }

            return result;
        }

        public void DeleteMediaByTypeAndFinalEndDate(long mediaTypeId, DateTime finalEndDate)
        {
            var index = NamingHelper.GetMediaIndexAlias(_partnerId);
            var languages = GetCatalogGroupCache().LanguageMapByCode.Values;
            var matchQuery = new BoolQuery();
            var typeTerm = new ESTerm(true) { Key = "media_type_id", Value = mediaTypeId.ToString() };
            matchQuery.AddChild(typeTerm, CutWith.AND);
            var catalogEndDateRange = new ESRange(false) { Key = "final_date" };
            catalogEndDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LT, finalEndDate.ToESDateFormat()));
            matchQuery.AddChild(catalogEndDateRange, CutWith.AND);
            var queryString = new ESQuery(matchQuery).ToString();
            foreach (var language in languages)
            {
                var type = GetTranslationType(MEDIA, language);
                if (!_elasticSearchApi.DeleteDocsByQuery(index, type, ref queryString, out var countDeleted))
                {
                    log.ErrorFormat("Failed to delete media assets. type: {1}, media_type_id: {2}, final_end_date: {3}", index, type, mediaTypeId, finalEndDate);
                    continue;
                }
                
                log.DebugFormat("Media assets were deleted from ES. index: {0}, type: {1}, media_type_id: {2}, final_end_date: {3}, deleted_count: {4}", index, type, mediaTypeId, finalEndDate, countDeleted);
            }
        }

        public bool UpsertChannel(int channelId, Channel channel = null, long userId = 0)
        {
            var result = false;
            if (channelId <= 0)
            {
                log.WarnFormat("Received channel request of invalid channel id {0} when calling UpsertChannel", channelId);
                return result;
            }

            try
            {
                if (channel == null)
                {
                    // isAllowedToViewInactiveAssets = true because only operator can cause upsert of channel
                    //todo tests move to ctor
                    GenericResponse<Channel> response = _channelManager.GetChannelById(_partnerId, channelId, true, userId);
                    if (response != null && response.Status != null && response.Status.Code != (int)eResponseStatus.OK)
                    {
                        return result;
                    }

                    channel = response.Object;
                    if (channel == null)
                    {
                        log.ErrorFormat("failed to get channel object for _partnerId: {0}, channelId: {1} when calling UpsertChannel", _partnerId, channelId);
                        return result;
                    }
                }

                string index = ESUtils.GetGroupChannelIndex(_partnerId);

                if (!_elasticSearchApi.IndexExists(index))
                {
                    log.Error($"channel metadata index doesn't exist for group {_partnerId}");
                    return false;
                }

                string type = "channel";
                string serializedChannel = _serializer.SerializeChannelObject(channel);
                if (_elasticSearchApi.InsertRecord(index, type, channelId.ToString(), serializedChannel))
                {
                    result = true;
                    if ((channel.m_nChannelTypeID != (int)ChannelType.Manual || (channel.m_lChannelTags != null && channel.m_lChannelTags.Count > 0))
                        && !UpdateChannelPercolator(new List<int>() { channelId }, channel))
                    {
                        log.ErrorFormat("Update channel percolator failed for Upsert Channel with channelId: {0}", channelId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Upsert Channel threw an exception. channelId: {0}, Exception={1};Stack={2}", channelId, ex.Message, ex.StackTrace), ex);
            }

            if (!result)
            {
                log.ErrorFormat("Upsert channel with id {0} failed", channelId);
            }

            return result;
        }

        public bool DeleteChannel(int channelId)
        {
            bool result = false;

            if (channelId <= 0)
            {
                log.WarnFormat("Received channel request of invalid channel id {0} when calling DeleteChannel", channelId);
                return result;
            }

            try
            {
                string index = ESUtils.GetGroupChannelIndex(_partnerId);
                ESDeleteResult deleteResult = _elasticSearchApi.DeleteDoc(index, CHANNEL, channelId.ToString());
                if (deleteResult.Ok)
                {
                    result = true;
                    if (DeleteChannelPercolator(new List<int>() { channelId }))
                    {
                        log.ErrorFormat("Delete channel percolator failed for Delete Channel");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Delete Channel threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            }

            if (!result)
            {
                log.ErrorFormat("Delete channel with id {0} failed", channelId);
            }

            return result;
        }

        public bool UpdateChannelPercolator(List<int> channelIds, Channel channel = null)
        {
            bool result = false;

            List<string> mediaAliases = _elasticSearchApi.GetAliases(NamingHelper.GetMediaIndexAlias(_partnerId));
            List<string> epgAliases = _elasticSearchApi.GetAliases(NamingHelper.GetEpgIndexAlias(_partnerId));

            try
            {
                if (mediaAliases != null && mediaAliases.Count > 0)
                {
                    if (channel != null)
                    {
                        result = UpdateChannelPercolator(channel, mediaAliases, epgAliases);
                    }
                    else
                    {
                        var groupManager = GetGroupManager();
                        if (groupManager == null || groupManager.channelIDs == null || groupManager.channelIDs.Count == 0)
                        {
                            return result;
                        }

                        result = true;
                        foreach (int channelId in channelIds)
                        {
                            var channelToUpdate = ChannelRepository.GetChannel(channelId, groupManager);

                            if (channelToUpdate != null)
                            {
                                result = result && UpdateChannelPercolator(channelToUpdate, mediaAliases, epgAliases);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Update Channel Percolator threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            }

            if (!result)
            {
                log.ErrorFormat("Update Channel Percolator with ids {0} failed", channelIds != null && channelIds.Count > 0 ? string.Join(",", channelIds) : string.Empty);
            }

            return result;
        }

        public bool DeleteChannelPercolator(List<int> channelIds)
        {
            bool result = false;
            string mediaIndex = NamingHelper.GetMediaIndexAlias(_partnerId);
            string epgIndex = NamingHelper.GetEpgIndexAlias(_partnerId);
            ESDeleteResult deleteResult;

            try
            {
                bool epgExists = _elasticSearchApi.IndexExists(epgIndex);
                List<string> mediaAliases = _elasticSearchApi.GetAliases(mediaIndex);
                List<string> epgAliases = null;

                if (epgExists)
                {
                    epgAliases = _elasticSearchApi.GetAliases(epgIndex);
                }

                // If we found aliases to both, or if we don't have EPG at all
                if (mediaAliases != null && epgAliases != null &&
                    (!epgExists || (mediaAliases.Count > 0 && epgAliases.Count > 0)))
                {
                    result = true;
                }

                if (mediaAliases != null && mediaAliases.Count > 0)
                {
                    foreach (int channelID in channelIds)
                    {
                        foreach (string index in mediaAliases)
                        {
                            deleteResult = _elasticSearchApi.DeleteDoc(index, PERCOLATOR, channelID.ToString());
                            result &= deleteResult.Ok;

                            if (!deleteResult.Ok)
                            {
                                log.Error("Error - " + string.Concat("Could not delete channel from elasticsearch. ID=", channelID));
                            }
                        }
                    }
                }
                else
                {
                    log.Error("Error - " + string.Concat("Could not find indices for alias ", mediaIndex));
                }

                if (epgAliases != null && epgAliases.Count > 0)
                {
                    foreach (int channelId in channelIds)
                    {
                        foreach (string index in epgAliases)
                        {
                            deleteResult = _elasticSearchApi.DeleteDoc(index, PERCOLATOR, channelId.ToString());
                            result &= deleteResult.Ok;

                            if (!deleteResult.Ok)
                            {
                                log.Error("Error - " + string.Concat("Could not delete channel from elasticsearch. ID=", channelId));
                            }
                        }
                    }
                }
                else
                {
                    log.Error("Error - " + string.Concat("Could not find indices for alias ", epgIndex));
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Delete Channel Percolator threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            }

            if (!result)
            {
                log.ErrorFormat("Delete Channel Percolator with ids {0} failed", channelIds != null && channelIds.Count > 0 ? string.Join(",", channelIds) : string.Empty);
            }

            return result;
        }

        public bool UpsertProgram(List<EpgCB> epgObjects, Dictionary<string, LinearChannelSettings> linearChannelSettings)
        {
            bool result = true;
            if (epgObjects == null || epgObjects.Count == 0) { return result; }
            try
            {
                _catalogManager.GetLinearChannelValues(epgObjects, _partnerId, _ => { });
                var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(_partnerId);
                if (epgFeatureVersion == EpgFeatureVersion.V3)
                {
                    UpsertProgramsEpgV3(epgObjects);
                    return result;
                }

                // This is an upsert program for epg version 1 and 2...
                result = UpsertProgramLegacy(epgObjects, linearChannelSettings, epgFeatureVersion, result);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error: Update EPGs threw an exception. Exception={0}", ex);
                throw ex;
            }

            return result;
        }

        private void UpsertProgramsEpgV3(List<EpgCB> epgObjects)
        {
            var epgsToUpsertByChannel = epgObjects.GroupBy(e => e.ChannelID);
            foreach (var epgOfChannel in epgsToUpsertByChannel)
            {
                var channelId = epgOfChannel.Key;
                var programsToIndex = epgOfChannel.ToList();
                // manual OPC operations in epgV3 will always use 0 as transactio id and will make sure that the relevant parent doc exists before
                // operation is done
                var transactionId = NamingHelper.GetEpgV3TransactionId(channelId, 0);
                CommitEpgCrudTransaction(transactionId, channelId);
                ApplyEpgCrudOperationWithTransaction(transactionId, programsToIndex, new List<EpgCB>());
            }
        }

        private bool UpsertProgramLegacy(List<EpgCB> epgObjects, Dictionary<string, LinearChannelSettings> linearChannelSettings, EpgFeatureVersion epgFeatureVersion, bool result)
        {
            var alias = NamingHelper.GetEpgIndexAlias(_partnerId);
            // prevent from size of bulk to be more than the default value of 500 (currently as of 23.06.20)
            var sizeOfBulk = SIZE_OF_BULK == 0 ? SIZE_OF_BULK_DEFAULT_VALUE : SIZE_OF_BULK > SIZE_OF_BULK_DEFAULT_VALUE ? SIZE_OF_BULK_DEFAULT_VALUE : SIZE_OF_BULK;

            // dictionary contains all language ids and its  code (string)
            var groupUsesTemplates = isOpc();
            CatalogGroupCache catalogGroupCache = null;
            Group groupManager = null;
            List<LanguageObj> languages;
            HashSet<string> metasToPad;

            if (groupUsesTemplates)
            {
                catalogGroupCache = GetCatalogGroupCache();
                languages = catalogGroupCache.LanguageMapById.Values.ToList();

                metasToPad = catalogGroupCache.TopicsMapById.Values
                    .Where(x => x.Type == ApiObjects.MetaType.Number)
                    .Select(y => y.SystemName.ToLower())
                    .ToHashSet();
            }
            else
            {
                groupManager = GetGroupManager();
                languages = groupManager.GetLangauges();

                metasToPad = GetMetasToPad();
            }

            if (languages == null)
            {
                // return false; // perhaps?
                log.Debug("Warning - " + string.Format("Group {0} has no languages defined.", _partnerId));
            }

            // TODO - Lior, remove these 5 lines below - used only to currently support linear media id search on elastic search
            var epgChannelIds = epgObjects.Select(item => item.ChannelID.ToString()).ToList<string>();

            if (linearChannelSettings == null)
            {
                linearChannelSettings = new Dictionary<string, LinearChannelSettings>();
            }

            #region Get Linear Channels Regions

            var linearChannelsRegionsMapping = RegionManager.Instance.GetLinearMediaRegions(_partnerId);

           #endregion

            // Temporarily - assume success
            var temporaryResult = true;
            var createdAliases = new HashSet<string>();

            // Create dictionary by languages
            var bulkRequests = new List<ESBulkRequestObj<ulong>>();
            List<KeyValuePair<string, string>> invalidResults = null;
            foreach (var language in languages)
            {
                // Filter programs to current language
                var currentLanguageEpgs = epgObjects.Where(epg =>
                    epg.Language.ToLower() == language.Code.ToLower() || (language.IsDefault && string.IsNullOrEmpty(epg.Language))).ToList();

                if (currentLanguageEpgs != null && currentLanguageEpgs.Count > 0)
                {
                    // Create bulk request object for each program
                    foreach (var epg in currentLanguageEpgs)
                    {
                        epg.PadMetas(metasToPad);

                        // Epg V2 has multiple indices connected to the gloabl alias {groupID}_epg
                        // in that case we need to use the specific date alias for each epg item to update
                        if (epgFeatureVersion == EpgFeatureVersion.V2)
                        {
                            alias = _namingHelper.GetDailyEpgIndexName(_partnerId, epg.StartDate.Date);
                            //in case alias already created ,no need to check in ES
                            if (!createdAliases.Contains(alias))
                            {
                                var aliases = new string[] { NamingHelper.GetEpgIndexAlias(_partnerId) };
                                CreateIndex(alias, aliases);
                                createdAliases.Add(alias);
                            }
                        }

                        string suffix = null;

                        if (!language.IsDefault)
                        {
                            suffix = language.Code;
                        }

                        // TODO - Lior, remove all this if - used only to currently support linear media id search on elastic search
                        if (linearChannelSettings.ContainsKey(epg.ChannelID.ToString()))
                        {
                            epg.LinearMediaId = linearChannelSettings[epg.ChannelID.ToString()].LinearMediaId;
                        }

                        if (epg.LinearMediaId > 0 && linearChannelsRegionsMapping != null && linearChannelsRegionsMapping.ContainsKey(epg.LinearMediaId))
                        {
                            epg.regions = linearChannelsRegionsMapping[epg.LinearMediaId];
                        }

                        string serializedEpg = _serializer.SerializeEpgObject(epg, suffix);
                        var totalMinutes = _ttlService.GetEpgTtlMinutes(epg);

                        bulkRequests.Add(new ESBulkRequestObj<ulong>()
                        {
                            docID = epg.EpgID,
                            index = alias,
                            type = GetTranslationType(EPG_INDEX_TYPE, language),
                            Operation = eOperation.index,
                            document = serializedEpg,
                            routing = epg.StartDate.ToUniversalTime().ToString("yyyyMMdd"),
                            ttl = $"{totalMinutes}m"
                        });

                        if (bulkRequests.Count > sizeOfBulk)
                        {
                            // send request to ES API
                            invalidResults = _elasticSearchApi.CreateBulkRequest(bulkRequests);

                            if (invalidResults != null && invalidResults.Count > 0)
                            {
                                foreach (var invalidResult in invalidResults)
                                {
                                    log.Error("Error - " + string.Format("Could not update EPG in ES. GroupID={0};Type={1};EPG_ID={2};error={3};",
                                        _partnerId, EPG_INDEX_TYPE, invalidResult.Key, invalidResult.Value));
                                }

                                result = false;
                                temporaryResult = false;
                            }
                            else
                            {
                                temporaryResult &= true;
                            }

                            bulkRequests.Clear();
                        }
                    }
                }
            }

            if (bulkRequests.Count > 0)
            {
                // send request to ES API
                invalidResults = _elasticSearchApi.CreateBulkRequest(bulkRequests);

                if (invalidResults != null && invalidResults.Count > 0)
                {
                    foreach (var invalidResult in invalidResults)
                    {
                        log.Error("Error - " + string.Format(
                            "Could not update EPG in ES. GroupID={0};Type={1};EPG_ID={2};error={3};",
                            _partnerId, EPG_INDEX_TYPE, invalidResult.Key, invalidResult.Value));
                    }

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

        public bool CompactEpgV2Indices(int futureIndexCompactionStart, int pastIndexCompactionStart)
        {
            var retryCount = 3;
            var globalEpgAlias = NamingHelper.GetEpgIndexAlias(_partnerId);
            var futureIndexName = NamingHelper.GetEpgFutureIndexName(_partnerId);
            var pastIndexName = NamingHelper.GetEpgPastIndexName(_partnerId);

            try
            {
                var numOfShards = ApplicationConfiguration.Current.EPGIngestV2Configuration.NumOfShardsForCompactedIndex.Value;
                EnsureEpgIndexExist(futureIndexName, EpgFeatureVersion.V2, numOfShards);
                SetNoRefresh(futureIndexName);
                EnsureEpgIndexExist(pastIndexName, EpgFeatureVersion.V2, numOfShards);
                SetNoRefresh(pastIndexName);

                var epgV2Indices = _elasticSearchApi.ListIndicesByAlias(globalEpgAlias)?.Select(i => i.Name)?.ToList();

                // Create a dictionary of index date parsed to the actual index name
                var epgV2IndicesDict = epgV2Indices
                    .Where(i => !i.Equals(futureIndexName, StringComparison.OrdinalIgnoreCase) && !i.Equals(pastIndexName, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(i => DateTime.ParseExact(i.Split('_').Last(), ElasticSearch.Common.Utils.ES_DATEONLY_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal));

                var pastCompactionStartDate = DateTime.UtcNow.Date.AddDays(-pastIndexCompactionStart);
                var futureCompactionStartDate = DateTime.UtcNow.Date.AddDays(futureIndexCompactionStart);
                var pastDatesToCompact = epgV2IndicesDict.Where(d => d.Key < pastCompactionStartDate).ToDictionary(k => k.Key, v => v.Value);
                var futureDatesToCompact = epgV2IndicesDict.Where(d => d.Key > futureCompactionStartDate).ToDictionary(k => k.Key, v => v.Value);
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
            bool result = false;
            var groupUsesTemplates = isOpc();
            if (assetIds != null & assetIds.Count > 0)
            {
                // dictionary contains all language ids and its  code (string)
                var languages = groupUsesTemplates
                    ? GetCatalogGroupCache().LanguageMapById.Values.ToList()
                    : GetGroupManager().GetLangauges();

                string type = isRecording ? RECORDING_INDEX_TYPE : ES_EPG_TYPE;
                string alias = isRecording
                    ? NamingHelper.GetRecordingIndexAlias(_partnerId)
                    : NamingHelper.GetEpgIndexAlias(_partnerId);

                ESTerms terms = new ESTerms(true)
                {
                    Key = "epg_id"
                };

                terms.Value.AddRange(assetIds.Select(id => id.ToString()));

                ESQuery query = new ESQuery(terms);
                string queryString = query.ToString();


                foreach (var lang in languages)
                {
                    string typeWithLanguage = GetTranslationType(type, lang);
                    _elasticSearchApi.DeleteDocsByQuery(alias, typeWithLanguage, ref queryString);
                }

                result = true;
            }

            return result;
        }

        private void CreateNewEpgIndex(string newIndexName, bool isRecording = false, bool shouldBuildWithReplicas = true, bool shouldUseNumOfConfiguredShards = true,
            string refreshInterval = null)
        {
            CreateEmptyEpgIndex(newIndexName, shouldBuildWithReplicas, shouldUseNumOfConfiguredShards, refreshInterval);
            AddMappingsToEpgIndex(newIndexName, isRecording);
        }

        private void CreateEmptyEpgIndex(string newIndexName, bool shouldBuildWithReplicas = true,
            bool shouldUseNumOfConfiguredShards = true, string refreshInterval = null, int? numOfShards = null)
        {
            List<LanguageObj> languages = GetLanguages();
            GetEpgAnalyzers(languages, out var analyzers, out var filters, out var tokenizers);
            int replicas = shouldBuildWithReplicas ? NUM_OF_REPLICAS : 0;
            int shards = shouldUseNumOfConfiguredShards ? NUM_OF_SHARDS : 1;

            // use sent num of shards or the already computed value
            shards = numOfShards ?? shards;
            var isIndexCreated = _elasticSearchApi.BuildIndex(newIndexName, shards, replicas, analyzers, filters, tokenizers, MAX_RESULTS, refreshInterval);
            if (!isIndexCreated) { throw new Exception(string.Format("Failed creating index for index:{0}", newIndexName)); }
        }

        private List<LanguageObj> GetLanguages()
        {
            return isOpc() ? GetCatalogGroupCache().LanguageMapById.Values.ToList() : GetGroupManager().GetLangauges();
        }

        private LanguageObj GetDefaultLanguage()
        {
            return isOpc() ? GetCatalogGroupCache().GetDefaultLanguage() : GetGroupManager().GetGroupDefaultLanguage();
        }

        #endregion

        #region methods required by epg v2

        public string SetupEpgV2Index(string indexNmae)
        {
            EnsureEpgIndexExist(indexNmae, EpgFeatureVersion.V2);
            SetNoRefresh(indexNmae);

            return indexNmae;
        }


        private void EnsureEpgIndexExist(string dailyEpgIndexName, EpgFeatureVersion epgVersion, int? numOfShards = null)
        {
            // TODO it's possible to create new index with mappings and alias in one request,
            // https://www.elastic.co/guide/en/elasticsearch/reference/2.3/indices-create-index.html#mappings
            // but we have huge mappings and don't know the impact on timeouts during index creation - should be tested on real environment.

            // Limitation: it's not a transaction, we don't remove index when add-mapping failed =>
            // EPGs could be added to the index without mapping (e.g. from asset.add)
            try
            {
                AddEmptyIndex(dailyEpgIndexName, numOfShards);
                AddEpgMappings(dailyEpgIndexName, epgVersion);
                AddEpgAlias(dailyEpgIndexName);
            }
            catch (Exception e)
            {
                log.Error($"index creation failed [{dailyEpgIndexName}]", e);
                throw new Exception($"index creation failed");
            }
        }

        private void AddEmptyIndex(string indexName, int? numOfShards = 0)
        {
            IndexManagerCommonHelpers.GetRetryPolicy<Exception>().Execute(() =>
            {
                var isIndexExist = _elasticSearchApi.IndexExists(indexName);
                if (isIndexExist) return;
                log.Info($"creating new index [{indexName}]");
                this.CreateEmptyEpgIndex(indexName, true, true, REFRESH_INTERVAL_FOR_EMPTY_INDEX, numOfShards: numOfShards);
            });
        }

        private void AddEpgMappings(string indexName, EpgFeatureVersion epgVersion)
        {
            var languages = GetLanguages();
            var existingMappings = _elasticSearchApi.GetMappingsNames(indexName).ToHashSet();
            var languagesToCreate = languages.Where(language =>
            {
                var mappingName = GetIndexType(false, language);
                return !existingMappings.Contains(mappingName);
            }).ToList();
            if (languagesToCreate.Count == 0) return;

            log.Info($"creating mappings. index [{indexName}], languages [{languagesToCreate.Select(_ => _.Name)}]");

            _groupManager.RemoveGroup(_partnerId); // remove from cache           
            if (!GetMetasAndTagsForMapping(
                out var metas,
                out var tags,
                out var metasToPad,
                true))
            {
                throw new Exception($"failed to get metas and tags");
            }

            var defaultLanguage = GetDefaultLanguage();

            foreach (var language in languagesToCreate)
            {
                IndexManagerCommonHelpers.GetRetryPolicy<Exception>().Execute(() =>
                    this.AddLanguageMapping(indexName, language, defaultLanguage, metas, tags, metasToPad, epgVersion));
            }

            // in epg v3 we use special transaction type for parent - child relation
            // NOTE: it is important that the transaction document type will be created after there are other types pointing to it as parent otherwise it cannot be a parent type
            if (epgVersion == EpgFeatureVersion.V3)
            {
                var success = _elasticSearchApi.InsertMapping(indexName, NamingHelper.EPG_V3_TRANSACTION_DOCUMENT_TYPE_NAME, "{\"properties\": {}}");
            }
        }

        private void AddEpgAlias(string indexName)
        {
            // create alias is idempotent request
            var epgIndexAlias = NamingHelper.GetEpgIndexAlias(_partnerId);
            log.Info($"creating alias. index [{indexName}], alias [{epgIndexAlias}]");
            IndexManagerCommonHelpers.GetRetryPolicy<Exception>().Execute(() =>
            {
                var isAliasAdded = _elasticSearchApi.AddAlias(indexName, epgIndexAlias);
                if (!isAliasAdded) throw new Exception($"index set alias failed [{indexName}], alias [{epgIndexAlias}]");
            });
        }

        private void RemoveEpgAlias(string indexName)
        {
            // create alias is idempotent request
            var epgIndexAlias = NamingHelper.GetEpgIndexAlias(_partnerId);
            log.Info($"creating alias. index [{indexName}], alias [{epgIndexAlias}]");
            IndexManagerCommonHelpers.GetRetryPolicy<Exception>().Execute(() =>
            {
                var isAliasAdded = _elasticSearchApi.RemoveAlias(indexName, epgIndexAlias);
                if (!isAliasAdded) throw new Exception($"index set alias failed [{indexName}], alias [{epgIndexAlias}]");
            });
        }

        private void SetNoRefresh(string dailyEpgIndexName)
        {
            // shut down refresh of index while bulk uploading
            IndexManagerCommonHelpers.GetRetryPolicy<Exception>().Execute(() =>
            {
                var isSetRefreshSuccess = _elasticSearchApi.UpdateIndexRefreshInterval(dailyEpgIndexName, "-1");
                if (!isSetRefreshSuccess)
                {
                    log.Error($"index set refresh to -1 failed [false], dailyEpgIndexName [{dailyEpgIndexName}]");
                    throw new Exception("Could not set index refresh interval");
                }
            });
        }

        private void SetIndexRefreshToDefault(string indexName)
        {
            IndexManagerCommonHelpers.GetRetryPolicy<Exception>().Execute(() =>
            {
                var isSetRefreshSuccess = _elasticSearchApi.UpdateIndexRefreshInterval(indexName, INDEX_REFRESH_INTERVAL);
                if (!isSetRefreshSuccess)
                {
                    log.Error($"index {indexName} set refresh to -1 failed [{isSetRefreshSuccess}]]");
                    throw new Exception("Could not set index refresh interval");
                }
            });
        }

        public bool ForceRefreshEpgIndex(string indexName)
        {
            return _elasticSearchApi.ForceRefresh(indexName);
        }

        public bool FinalizeEpgV2Indices(List<DateTime> dates)
        {
            var indexes = dates.Select(x => _namingHelper.GetDailyEpgIndexName(_partnerId, x));

            foreach (var indexName in indexes)
            {
                if (!_elasticSearchApi.IndexExists(indexName)) { continue; }
                SetIndexRefreshToDefault(indexName);
            }

            return true;
        }

        public IList<EpgProgramBulkUploadObject> GetCurrentProgramsByDate(int channelId, DateTime fromDate, DateTime toDate)
        {
            log.Debug($"GetCurrentProgramsByDate > fromDate:[{fromDate}], toDate:[{toDate}]");
            var result = new List<EpgProgramBulkUploadObject>();
            var epgIndexAlias = NamingHelper.GetEpgIndexAlias(_partnerId);

            // if index does not exist - then we have a fresh start, we have 0 programs currently
            if (!_elasticSearchApi.IndexExists(epgIndexAlias))
            {
                log.Debug($"GetCurrentProgramsByDate > index alias:[{epgIndexAlias}] does not exits, assuming no current programs");
                return result;
            }

            log.Debug($"GetCurrentProgramsByDate > index alias:[{epgIndexAlias}] found, searching current programs, minStartDate:[{fromDate}], maxEndDate:[{toDate}]");
            var query = new FilteredQuery();

            // Program end date > minimum start date
            // program start date < maximum end date
            var minimumRange = new ESRange(false, "end_date", eRangeComp.GTE, fromDate.ToString(ESUtils.ES_DATE_FORMAT));
            var maximumRange = new ESRange(false, "start_date", eRangeComp.LTE, toDate.ToString(ESUtils.ES_DATE_FORMAT));
            var channelFilter = ESTerms.GetSimpleNumericTerm("epg_channel_id", new[] { channelId });


            var filterCompositeType = new FilterCompositeType(CutWith.AND);
            filterCompositeType.AddChild(minimumRange);
            filterCompositeType.AddChild(maximumRange);
            filterCompositeType.AddChild(channelFilter);


            var queryFilter = new QueryFilter { FilterSettings = filterCompositeType };
            var epgFeatureVersion = _groupSettingsManager.GetEpgFeatureVersion(_partnerId);
            if (epgFeatureVersion == EpgFeatureVersion.V3)
            {
                Helper.WrapFilterWithCommittedOnlyTransactionsForEpgV3(_partnerId, queryFilter, _elasticSearchApi);
            }

            query.Filter = queryFilter;

            query.ReturnFields.Clear();
            query.AddReturnField("_index");
            query.AddReturnField("epg_id");
            query.AddReturnField("start_date");
            query.AddReturnField("end_date");
            query.AddReturnField("epg_identifier");
            query.AddReturnField("is_auto_fill");
            query.AddReturnField("linear_media_id");
            query.AddReturnField("group_id");

            // get the epg document ids from elasticsearch
            var searchQuery = query.ToString();
            var searchResult = _elasticSearchApi.Search(epgIndexAlias, EPG_INDEX_TYPE, ref searchQuery);

            // get the programs - epg ids from elasticsearch, information from EPG DAL
            if (!string.IsNullOrEmpty(searchResult))
            {
                var json = JObject.Parse(searchResult);

                var hits = (json["hits"]["hits"] as JArray);

                foreach (var hit in hits)
                {
                    var hitFields = hit["fields"];
                    var epgItem = new EpgProgramBulkUploadObject();
                    epgItem.EpgExternalId = ESUtils.ExtractValueFromToken<string>(hitFields, "epg_identifier");
                    epgItem.StartDate = ESUtils.ExtractDateFromToken(hitFields, "start_date");
                    epgItem.EndDate = ESUtils.ExtractDateFromToken(hitFields, "end_date");
                    epgItem.EpgId = ESUtils.ExtractValueFromToken<ulong>(hitFields, "epg_id");
                    epgItem.IsAutoFill = ESUtils.ExtractValueFromToken<bool>(hitFields, "is_auto_fill");
                    epgItem.ChannelId = channelId;
                    epgItem.LinearMediaId = ESUtils.ExtractValueFromToken<int>(hitFields, "linear_media_id");
                    epgItem.ParentGroupId = ESUtils.ExtractValueFromToken<int>(hitFields, "group_id"); ;
                    epgItem.GroupId = _partnerId;

                    result.Add(epgItem);
                }
            }

            return result;
        }

        public IList<EpgProgramInfo> GetCurrentProgramInfosByDate(int channelId, DateTime fromDate, DateTime toDate)
        {
            log.Debug($"GetCurrentProgramsByDate > fromDate:[{fromDate}], toDate:[{toDate}]");
            var result = new List<EpgProgramInfo>();
            var index = NamingHelper.GetEpgIndexAlias(_partnerId);

            // if index does not exist - then we have a fresh start, we have 0 programs currently
            if (!_elasticSearchApi.IndexExists(index))
            {
                log.Debug($"GetCurrentProgramsByDate > index alias:[{index}] does not exits, assuming no current programs");
                return result;
            }

            log.Debug($"GetCurrentProgramsByDate > index alias:[{index}] found, searching current programs, minStartDate:[{fromDate}], maxEndDate:[{toDate}]");
            var query = new FilteredQuery();

            // Program end date > minimum start date
            // program start date < maximum end date
            var minimumRange = new ESRange(false, "end_date", eRangeComp.GTE, fromDate.ToString(ESUtils.ES_DATE_FORMAT));
            var maximumRange = new ESRange(false, "start_date", eRangeComp.LTE, toDate.ToString(ESUtils.ES_DATE_FORMAT));
            var channelFilter = ESTerms.GetSimpleNumericTerm("epg_channel_id", new[] { channelId });

            var filterCompositeType = new FilterCompositeType(CutWith.AND);
            filterCompositeType.AddChild(minimumRange);
            filterCompositeType.AddChild(maximumRange);
            filterCompositeType.AddChild(channelFilter);


            var queryFilter = new QueryFilter() { FilterSettings = filterCompositeType };
            var epgFeatureVersion = _groupSettingsManager.GetEpgFeatureVersion(_partnerId);
            if (epgFeatureVersion == EpgFeatureVersion.V3)
            {
                Helper.WrapFilterWithCommittedOnlyTransactionsForEpgV3(_partnerId, queryFilter, _elasticSearchApi);
            }
            query.Filter = queryFilter;

            query.ReturnFields.Clear();
            query.AddReturnField("_index");
            query.AddReturnField("epg_identifier");
            query.AddReturnField("document_id");
            query.AddReturnField("is_auto_fill");


            // get the epg document ids from elasticsearch
            var searchQuery = query.ToString();
            var searchResult = _elasticSearchApi.Search(index, string.Empty, ref searchQuery);

            // get the programs - epg ids from elasticsearch, information from EPG DAL
            if (!string.IsNullOrEmpty(searchResult))
            {
                var json = JObject.Parse(searchResult);

                var hits = (json["hits"]["hits"] as JArray);

                foreach (var hit in hits)
                {
                    var epgItem = new EpgProgramInfo();
                    var hitFields = hit["fields"];
                    var type = hit["_type"].ToString();
                    var language = _mappingTypeResolver.ExtractLanguageCodeFromMappingType(type, out bool isDefault);

                    if (isDefault)
                    {
                        language = GetDefaultLanguage().Code;
                    }

                    epgItem.LanguageCode = language;
                    epgItem.EpgExternalId = ESUtils.ExtractValueFromToken<string>(hitFields, "epg_identifier");
                    epgItem.DocumentId = ESUtils.ExtractValueFromToken<string>(hitFields, "document_id");
                    epgItem.IsAutofill = ESUtils.ExtractValueFromToken<bool>(hitFields, "is_auto_fill");
                    epgItem.GroupId = _partnerId;
                    result.Add(epgItem);
                }
            }

            return result;
        }

        #endregion

        public SearchResultsObj SearchMedias(MediaSearchObj search, int langId, bool useStartDate)
        {
            SearchResultsObj oRes = new SearchResultsObj();
            int partnerId = search.m_nGroupId > 0 ? search.m_nGroupId : _partnerId;
            ESMediaQueryBuilder queryParser = new ESMediaQueryBuilder(partnerId, search);

            bool shouldSortByStartDateOfAssociationTagsAndParentMedia =
                search.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.START_DATE) &&
                search.associationTags?.Count > 0 &&
                search.parentMediaTypes?.Count > 0;
            int nPageIndex = 0;
            int nPageSize = 0;
            if ((search.m_oOrder.m_eOrderBy <= ApiObjects.SearchObjects.OrderBy.VIEWS && search.m_oOrder.m_eOrderBy >= ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER)
                || search.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.VOTES_COUNT)
                || shouldSortByStartDateOfAssociationTagsAndParentMedia)
            {
                nPageIndex = search.m_nPageIndex;
                nPageSize = search.m_nPageSize;
                queryParser.PageIndex = 0;
                queryParser.PageSize = 0;

                if (search.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.START_DATE))
                {
                    queryParser.ReturnFields.Add("\"start_date\"");
                    queryParser.ReturnFields.Add("\"media_type_id\"");
                }
            }
            else
            {
                queryParser.PageIndex = search.m_nPageIndex;
                queryParser.PageSize = search.m_nPageSize;
            }

            queryParser.QueryType = (search.m_bExact) ? eQueryType.EXACT : eQueryType.BOOLEAN;

            string sQuery = queryParser.BuildSearchQueryString(search.m_bIgnoreDeviceRuleId, search.m_bUseActive);

            if (!string.IsNullOrEmpty(sQuery))
            {
                int nStatus = 0;

                string sType = Utils.GetESTypeByLanguage(ES_MEDIA_TYPE, search.m_oLangauge);
                string sUrl = string.Format("{0}/{1}/{2}/_search", _elasticSearchApi.baseUrl, NamingHelper.GetMediaIndexAlias(_partnerId), sType);

                string retObj = _elasticSearchApi.SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, sQuery, true);

                if (nStatus == ElasticSearchApi.STATUS_OK)
                {
                    int nTotalItems = 0;
                    List<ElasticSearchApi.ESAssetDocument> lMediaDocs = ESUtils.DecodeAssetSearchJsonObject(retObj, ref nTotalItems);
                    if (lMediaDocs != null && lMediaDocs.Count > 0)
                    {
                        var extendedUnifiedSearchResults = new List<ExtendedUnifiedSearchResult>();

                        oRes.m_resultIDs = new List<SearchResult>();
                        oRes.n_TotalItems = nTotalItems;

                        foreach (ElasticSearchApi.ESAssetDocument doc in lMediaDocs)
                        {
                            var searchResult = new SearchResult()
                            {
                                assetID = doc.asset_id,
                                UpdateDate = doc.update_date
                            };
                            oRes.m_resultIDs.Add(searchResult);

                            // new UnifiedSearchResult is used there just for the sake of compatibility.
                            extendedUnifiedSearchResults.Add(new ExtendedUnifiedSearchResult(new UnifiedSearchResult(), doc));
                        }

                        if ((search.m_oOrder.m_eOrderBy <= ApiObjects.SearchObjects.OrderBy.VIEWS &&
                             search.m_oOrder.m_eOrderBy >= ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER)
                            || search.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.VOTES_COUNT)
                            || shouldSortByStartDateOfAssociationTagsAndParentMedia)
                        {
                            List<int> lMediaIds = oRes.m_resultIDs.Select(item => item.assetID).ToList();

                            if (search.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.START_DATE))
                            {
                                lMediaIds =
                                    _startDateAssociationTagsSortStrategy.SortAssetsByStartDate(
                                            extendedUnifiedSearchResults.ToArray(),
                                            search.m_oLangauge,
                                            search.m_oOrder.m_eOrderDir,
                                            search.associationTags,
                                            search.parentMediaTypes,
                                            _partnerId)
                                        .Select(x => Convert.ToInt32(x))
                                        .ToList();
                            }
                            else
                            {
                                Utils.OrderMediasByStats(lMediaIds, (int)search.m_oOrder.m_eOrderBy, (int)search.m_oOrder.m_eOrderDir);
                            }

                            var dItems = oRes.m_resultIDs.ToDictionary(item => item.assetID);
                            oRes.m_resultIDs.Clear();

                            // check which results should be returned
                            bool illegalRequest = false;
                            if (nPageSize < 0 || nPageIndex < 0)
                            {
                                // illegal parameters
                                illegalRequest = true;
                            }
                            else
                            {
                                if (nPageSize == 0 && nPageIndex == 0)
                                {
                                    // return all results
                                }
                                else
                                {
                                    // apply paging on results 
                                    lMediaIds = lMediaIds.Skip(nPageSize * nPageIndex).Take(nPageSize).ToList();
                                }
                            }

                            if (!illegalRequest)
                            {
                                SearchResult oTemp;
                                foreach (int mediaID in lMediaIds)
                                {
                                    if (dItems.TryGetValue(mediaID, out oTemp))
                                    {
                                        oRes.m_resultIDs.Add(oTemp);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return oRes;
        }

        public List<string> GetAutoCompleteList(MediaSearchObj mediaSearch, int nLangID, ref int nTotalItems)
        {
            List<string> lRes = new List<string>();

            mediaSearch.m_dOr.Add(new SearchValue()
            {
                m_lValue = new List<string>() { "" },
                m_sKey = "name^3"
            });

            ESMediaQueryBuilder queryParser = new ESMediaQueryBuilder(_partnerId, mediaSearch);
            queryParser.PageIndex = mediaSearch.m_nPageIndex;
            queryParser.PageSize = mediaSearch.m_nPageSize;

            queryParser.QueryType = eQueryType.PHRASE_PREFIX;

            string sQuery = queryParser.BuildMediaAutoCompleteQuery();

            if (!string.IsNullOrEmpty(sQuery))
            {
                string sType = Utils.GetESTypeByLanguage(ES_MEDIA_TYPE, mediaSearch.m_oLangauge);
                string retObj = _elasticSearchApi.Search(_partnerId.ToString(), sType, ref sQuery);

                List<ElasticSearchApi.ESAssetDocument> lMediaDocs = ESUtils.DecodeAssetSearchJsonObject(retObj, ref nTotalItems);
                if (lMediaDocs != null && lMediaDocs.Count > 0)
                {
                    foreach (ElasticSearchApi.ESAssetDocument doc in lMediaDocs)
                    {
                        lRes.Add(doc.name);
                    }
                }
            }

            return lRes;
        }

        public List<string> GetEpgAutoCompleteList(EpgSearchObj epgSearchObj)
        {
            List<string> resultFinalList = null;

            List<string> lRouting = new List<string>();

            DateTime dTempDate = epgSearchObj.m_dStartDate;
            while (dTempDate <= epgSearchObj.m_dEndDate)
            {
                lRouting.Add(dTempDate.ToString("yyyyMMdd"));
                dTempDate = dTempDate.AddDays(1);
            }


            ESEpgQueryBuilder queryBuilder = new ESEpgQueryBuilder()
            {
                m_oEpgSearchObj = epgSearchObj
            };

            var filteredQuery = queryBuilder.BuildEpgAutoCompleteQuery();
            var epgFeatureVersion = _groupSettingsManager.GetEpgFeatureVersion(_partnerId);
            if (epgFeatureVersion == EpgFeatureVersion.V3)
            {
                var queryFilter = new QueryFilter();
                filteredQuery.Filter = queryFilter;
                Helper.WrapFilterWithCommittedOnlyTransactionsForEpgV3(_partnerId, queryFilter, _elasticSearchApi);
            }
            var sQuery = filteredQuery.ToString();


            string sGroupAlias = string.Format("{0}_epg", epgSearchObj.m_nGroupID);
            string searchRes = _elasticSearchApi.Search(sGroupAlias, ES_EPG_TYPE, ref sQuery, lRouting);

            int nTotalRecords = 0;
            List<ElasticSearchApi.ESAssetDocument> lDocs = ESUtils.DecodeAssetSearchJsonObject(searchRes, ref nTotalRecords);

            if (lDocs != null)
            {
                resultFinalList = lDocs.Select(doc => doc.name).ToList();
                resultFinalList = resultFinalList.Distinct().OrderBy(q => q).ToList<string>();
            }


            return resultFinalList;
        }

        // Testable
        public SearchResultsObj SearchSubscriptionMedias(List<MediaSearchObj> oSearch, int nLangID, bool shouldUseStartDate,
            string sMediaTypes, OrderObj orderObj, int nPageIndex, int nPageSize)
        {
            SearchResultsObj lSortedMedias = new SearchResultsObj();

            int nTotalItems = 0;

            var groupManager = GetGroupManager();
            if (groupManager == null)
                return lSortedMedias;

            if (oSearch != null && oSearch.Count > 0)
            {
                var lSearchResults = new List<ElasticSearchApi.ESAssetDocument>();


                ESMediaQueryBuilder queryBuilder = new ESMediaQueryBuilder();

                FilteredQuery tempQuery;

                FilterCompositeType groupedFilters = new FilterCompositeType(CutWith.OR);
                /*
                 * Foreach media search object, create filtered query.
                 * Add the query's filter to the grouped filter so that we can then create a single request
                 * containing all the channels that we want.
                 */
                foreach (MediaSearchObj searchObj in oSearch)
                {
                    if (searchObj == null)
                        continue;

                    queryBuilder.m_nGroupID = searchObj.m_nGroupId;
                    searchObj.m_nPageSize = 0;
                    queryBuilder.oSearchObject = searchObj;
                    queryBuilder.QueryType = (searchObj.m_bExact) ? eQueryType.EXACT : eQueryType.BOOLEAN;
                    tempQuery = queryBuilder.BuildChannelFilteredQuery();

                    if (tempQuery != null && tempQuery.Filter != null)
                    {
                        groupedFilters.AddChild(tempQuery.Filter.FilterSettings);
                    }
                }

                string sOrderValue = FilteredQuery.GetESSortValue(orderObj);


                tempQuery = new FilteredQuery()
                {
                    PageIndex = nPageIndex,
                    PageSize = nPageSize
                };
                tempQuery.ESSort.Add(new ESOrderObj()
                {
                    m_eOrderDir = orderObj.m_eOrderDir,
                    m_sOrderValue = sOrderValue
                });
                tempQuery.Filter = new QueryFilter()
                {
                    FilterSettings = groupedFilters
                };

                string sSearchQuery = tempQuery.ToString();


                string sRetVal = _elasticSearchApi.Search(groupManager.m_nParentGroupID.ToString(), ES_MEDIA_TYPE, ref sSearchQuery);

                lSearchResults = ESUtils.DecodeAssetSearchJsonObject(sRetVal, ref nTotalItems);

                if (lSearchResults != null && lSearchResults.Count > 0)
                {
                    log.Debug("Info - SearchSubscriptionMedias returned search results");
                    lSortedMedias.m_resultIDs = new List<SearchResult>();

                    lSortedMedias.n_TotalItems = nTotalItems;


                    if ((orderObj.m_eOrderBy <= OrderBy.VIEWS &&
                         orderObj.m_eOrderBy >= ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER) ||
                        orderObj.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.VOTES_COUNT))
                    {
                        List<int> lIds = lSearchResults.Select(item => item.asset_id).ToList();

                        Utils.OrderMediasByStats(lIds, (int)orderObj.m_eOrderBy, (int)orderObj.m_eOrderDir);

                        var dItems = lSearchResults.ToDictionary(item => item.asset_id);

                        ElasticSearchApi.ESAssetDocument oTemp;
                        foreach (int mediaID in lIds)
                        {
                            if (dItems.TryGetValue(mediaID, out oTemp))
                            {
                                lSortedMedias.m_resultIDs.Add(new SearchResult()
                                {
                                    assetID = oTemp.asset_id,
                                    UpdateDate = oTemp.update_date
                                });
                            }
                        }
                    }
                    else
                    {
                        lSortedMedias.m_resultIDs = lSearchResults.Select(item => new SearchResult()
                        {
                            assetID = item.asset_id,
                            UpdateDate = item.update_date
                        }).ToList();
                    }
                }
            }

            return lSortedMedias;
        }

        /// <summary>
        /// Takes several search objects, joins them together and searches the assets in ES indexes.
        /// </summary>
        /// <param name="subscriptionGroupId"></param>
        /// <param name="searchObjects"></param>
        /// <param name="languageId"></param>
        /// <param name="useStartDate"></param>
        /// <param name="mediaTypes"></param>
        /// <param name="order"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalItems"></param>
        /// <returns></returns>
        public List<UnifiedSearchResult> SearchSubscriptionAssets(List<BaseSearchObject> searchObjects, int languageId, bool useStartDate,
            string mediaTypes, OrderObj order, int pageIndex, int pageSize, ref int totalItems)
        {
            List<UnifiedSearchResult> finalSearchResults = new List<UnifiedSearchResult>();
            totalItems = 0;

            var groupManager = GetGroupManager();
            if (groupManager == null)
                return finalSearchResults;

            var parentGroupId = _partnerId;


            parentGroupId = groupManager.m_nParentGroupID;


            if (searchObjects != null && searchObjects.Count > 0)
            {
                List<ElasticSearchApi.ESAssetDocument> searchResults = new List<ElasticSearchApi.ESAssetDocument>();

                #region Build Search Query

                BoolQuery boolQuery = BuildMultipleSearchQuery(searchObjects, parentGroupId);

                string orderValue = FilteredQuery.GetESSortValue(order);

                FilteredQuery filteredQuery = new FilteredQuery()
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
                filteredQuery.ESSort.Add(new ESOrderObj()
                {
                    m_eOrderDir = order.m_eOrderDir,
                    m_sOrderValue = orderValue
                });

                //// Set filter to be grouped filters we created earlier
                //tempQuery.Filter = new QueryFilter()
                //{
                //    FilterSettings = groupedFilters
                //};

                filteredQuery.Query = boolQuery;

                string searchQuery = filteredQuery.ToString();

                #endregion

                string searchResultString = _elasticSearchApi.Search(NamingHelper.GetMediaIndexAlias(parentGroupId), ES_MEDIA_TYPE, ref searchQuery);

                int temporaryTotalItems = 0;
                searchResults = ESUtils.DecodeAssetSearchJsonObject(searchResultString, ref temporaryTotalItems);

                #region Process results

                if (searchResults != null && searchResults.Count > 0)
                {
                    log.Debug("Info - SearchSubscriptionAssets returned search results");

                    totalItems = temporaryTotalItems;

                    // Order by stats
                    if ((order.m_eOrderBy <= ApiObjects.SearchObjects.OrderBy.VIEWS && order.m_eOrderBy >= ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER) ||
                        order.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.VOTES_COUNT))
                    {
                        List<int> ids = searchResults.Select(item => item.asset_id).ToList();

                        Utils.OrderMediasByStats(ids, (int)order.m_eOrderBy, (int)order.m_eOrderDir);

                        Dictionary<int, ElasticSearchApi.ESAssetDocument> itemsDictionary = searchResults.ToDictionary(item => item.asset_id);

                        ElasticSearchApi.ESAssetDocument temporaryDocument;

                        foreach (int asset in ids)
                        {
                            if (itemsDictionary.TryGetValue(asset, out temporaryDocument))
                            {
                                finalSearchResults.Add(new UnifiedSearchResult()
                                {
                                    AssetId = temporaryDocument.asset_id.ToString(),
                                    AssetType = ESUtils.ParseAssetType(temporaryDocument.type),
                                    m_dUpdateDate = temporaryDocument.update_date
                                });
                            }
                        }
                    }
                    else
                    {
                        finalSearchResults = searchResults.Select(item => new UnifiedSearchResult()
                        {
                            AssetId = item.asset_id.ToString(),
                            AssetType = ESUtils.ParseAssetType(item.type),
                            m_dUpdateDate = item.update_date
                        }).ToList();
                    }
                }

                #endregion
            }

            return finalSearchResults;
        }

        public bool DoesMediaBelongToChannels(List<int> channelIDs, int mediaId)
        {
            bool bResult = false;

            if (channelIDs == null || channelIDs.Count < 1)
                return bResult;

            List<int> lChannelsFound = GetMediaChannels(mediaId);

            if (lChannelsFound != null && lChannelsFound.Count > 0)
            {
                foreach (int channelId in lChannelsFound)
                {
                    if (channelIDs.Contains(channelId))
                    {
                        bResult = true;
                        break;
                    }
                }
            }

            return bResult;
        }

        public List<int> GetMediaChannels(int mediaId)
        {
            List<int> lResult = new List<int>();
            string sIndex = NamingHelper.GetMediaIndexAlias(_partnerId);

            string sMediaDoc = _elasticSearchApi.GetDoc(sIndex, ES_MEDIA_TYPE, mediaId.ToString());

            if (!string.IsNullOrEmpty(sMediaDoc))
            {
                try
                {
                    var jsonObj = JObject.Parse(sMediaDoc);
                    sMediaDoc = jsonObj.SelectToken("_source").ToString();

                    StringBuilder sbMediaDoc = new StringBuilder();
                    sbMediaDoc.Append("{\"doc\":");
                    sbMediaDoc.Append(sMediaDoc);
                    sbMediaDoc.Append("}");

                    sMediaDoc = sbMediaDoc.ToString();
                    List<string> lRetVal = _elasticSearchApi.SearchPercolator(sIndex, ES_MEDIA_TYPE, ref sMediaDoc);

                    if (lRetVal != null && lRetVal.Count > 0)
                    {
                        int nID;
                        foreach (string match in lRetVal)
                        {
                            if (int.TryParse(match, out nID))
                            {
                                lResult.Add(nID);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + string.Format("GetMediaChannels - Could not parse response. Ex={0}, ST: {1}", ex.Message, ex.StackTrace), ex);
                }
            }

            return lResult;
        }

        public List<SearchResult> GetAssetsUpdateDate(eObjectType assetType, List<int> assetIds)
        {
            List<SearchResult> response = new List<SearchResult>();

            try
            {
                string index = string.Empty;
                string type = string.Empty;
                string idField = string.Empty;

                switch (assetType)
                {
                    case eObjectType.Media:
                        index = $"{_partnerId}";
                        type = "media";
                        idField = "media_id";
                        break;
                    case eObjectType.EPG:
                        index = $"epg_{_partnerId}";
                        type = "epg";
                        idField = "epg_id";
                        break;
                    case eObjectType.Recording:
                        index = $"recording_{_partnerId}";
                        type = "recording";
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

                /*
                {
                    "size": 500,
                    "from": 0,
                    "fields": [
                        "media_id",
                        "update_date"
                    ],
                    "query": {
                        "terms": {
                            "media_id": [
                                762870,
                                762874
                            ]
                        }
                    }
                }
                */

                for (int from = 0; from < assetIds.Count; from += 500)
                {
                    JObject searchJsonObject = new JObject();
                    searchJsonObject["size"] = 500;
                    searchJsonObject["from"] = 0;
                    searchJsonObject["fields"] = new JArray(new List<string>() { idField, "update_date" });
                    JObject queryPart = new JObject();
                    JObject termsPart = new JObject();
                    // every time take another 500 assets
                    termsPart[idField] = new JArray(assetIds.Skip(from).Take(500));
                    queryPart["terms"] = termsPart;
                    searchJsonObject["query"] = queryPart;

                    string searchQuery = searchJsonObject.ToString();

                    string searchResultString = _elasticSearchApi.Search(index, type, ref searchQuery);

                    if (string.IsNullOrEmpty(searchResultString))
                    {
                        log.Error($"Got empty search result when trying to get assets update date in group {_partnerId} type {assetType}");
                        return response;
                    }

                    var searchResultObject = JObject.Parse(searchResultString);

                    var hitsArray = (searchResultObject["hits"]["hits"] as JArray);
                    foreach (var hit in hitsArray)
                    {
                        var fields = hit["fields"];
                        if (fields[idField] != null && fields["update_date"] != null)
                        {
                            var assetId = Convert.ToInt32((fields[idField].FirstOrDefault() as JValue));
                            var dateString = Convert.ToString((fields["update_date"].FirstOrDefault() as JValue));
                            var updateDate = DateTime.ParseExact(dateString, ESUtils.ES_DATE_FORMAT, null);

                            response.Add(new SearchResult()
                            {
                                assetID = assetId,
                                UpdateDate = updateDate
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error when getting assets update date. group = {_partnerId}, ex = {ex}");
            }

            return response;
        }

        public virtual SearchResultsObj SearchEpgs(EpgSearchObj epgSearch)
        {
            SearchResultsObj epgResponse = null;

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

                DateTime searchEndDate = epgSearch.m_dSearchEndDate;

                ESEpgQueryBuilder epgQueryBuilder = new ESEpgQueryBuilder()
                {
                    m_oEpgSearchObj = epgSearch,
                    bAnalyzeWildcards = true
                };

                //string sQuery = epgQueryBuilder.BuildSearchQueryString();
                List<string> queries = epgQueryBuilder.BuildSearchQueryStrings();
                DateTime dTempDate = epgSearch.m_dStartDate.AddDays(-1);
                dTempDate = new DateTime(dTempDate.Year, dTempDate.Month, dTempDate.Day);

                List<string> lRouting = new List<string>();

                while (dTempDate <= epgSearch.m_dEndDate)
                {
                    lRouting.Add(dTempDate.ToString("yyyyMMdd"));
                    dTempDate = dTempDate.AddDays(1);
                }

                string sGroupAlias = string.Format("{0}_epg", _partnerId);
                string searchRes = string.Empty;
                int nTotalRecords = 0;
                List<ElasticSearchApi.ESAssetDocument> lDocs = null;
                if (queries.Count == 1)
                {
                    string sQuery = queries[0];
                    searchRes = _elasticSearchApi.Search(sGroupAlias, ES_EPG_TYPE, ref sQuery, lRouting);
                    lDocs = ESUtils.DecodeAssetSearchJsonObject(searchRes, ref nTotalRecords);
                    //DecodeEpgSearchJsonObject(searchRes, ref nTotalRecords);
                }
                else
                {
                    searchRes = _elasticSearchApi.MultiSearch(sGroupAlias, ES_EPG_TYPE, queries, lRouting);
                    lDocs = DecodeEpgMultiSearchJsonObject(searchRes, ref nTotalRecords);
                }

                if (lDocs != null)
                {
                    epgResponse = new SearchResultsObj();
                    epgResponse.m_resultIDs = lDocs.Select(doc => new SearchResult
                    {
                        assetID = doc.asset_id,
                        UpdateDate = doc.update_date
                    }).ToList();
                    epgResponse.n_TotalItems = nTotalRecords;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("SearchEpgs ex={0} st: {1}", ex.Message, ex.StackTrace), ex);
            }

            return epgResponse;
        }

        private void InitializeDictionary(List<long> distinctMediaIDs, ref Dictionary<long, bool> dict)
        {
            int length = distinctMediaIDs.Count;
            dict = new Dictionary<long, bool>(length);
            for (int i = 0; i < length; i++)
                dict.Add(distinctMediaIDs[i], false);
        }

        private MediaSearchObj BuildSearchObjectForValidatingMediaIDsInChannels(List<long> mediaIDs,
            List<string> jsonizedChannelsDefinitionsToSearchIn, List<string> jsonizedChannelsDefinitionsMediaIDsShouldNotAppearIn)
        {
            MediaSearchObj searchObj = new MediaSearchObj();
            searchObj.m_nPageSize = mediaIDs.Count;
            searchObj.m_lChannelsDefinitionsMediaNeedsToBeInAtLeastOneOfIt = jsonizedChannelsDefinitionsToSearchIn;
            searchObj.m_lOrMediaNotInAnyOfTheseChannelsDefinitions = jsonizedChannelsDefinitionsMediaIDsShouldNotAppearIn;

            return searchObj;
        }

        private void UpdateDictionaryAccordingToESResults(List<ElasticSearchApi.ESAssetDocument> lMediaDocs, ref Dictionary<long, bool> dict)
        {
            if (lMediaDocs != null && lMediaDocs.Count > 0)
            {
                int length = lMediaDocs.Count;
                for (int i = 0; i < length; i++)
                {
                    if (dict.ContainsKey(lMediaDocs[i].asset_id))
                        dict[lMediaDocs[i].asset_id] = true;
                }
            }
        }

        #region Unified Search

        /// <summary>
        /// Performs a search on several types of assets in a single call
        /// </summary>
        /// <param name="unifiedSearchDefinitions"></param>
        /// <returns></returns>
        public List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearchDefinitions, ref int totalItems)
            => UnifiedSearch(unifiedSearchDefinitions, ref totalItems, out _);


        /// <summary>
        /// Performs a search on several types of assets in a single call
        /// </summary>
        /// <param name="unifiedSearchDefinitions"></param>
        /// <returns></returns>
        public List<UnifiedSearchResult> UnifiedSearch(
            UnifiedSearchDefinitions unifiedSearchDefinitions,
            ref int totalItems,
            out List<AggregationsResult> aggregationsResults)
        {
            aggregationsResults = null;
            var searchResultsList = new List<UnifiedSearchResult>();
            var extendedUnifiedSearchResults = new List<ExtendedUnifiedSearchResult>();
            totalItems = 0;

            var esQueryBuilder = new ESUnifiedQueryBuilder(_esSortingService, _sortingAdapter, _queryInitializer, unifiedSearchDefinitions);
            esQueryBuilder.SetPagingForUnifiedSearch();

            // ES index is on parent group id
            int parentGroupId = _partnerId;

            // In case something failed here, use the group that was sent
            if (parentGroupId == 0)
            {
                parentGroupId = unifiedSearchDefinitions.groupId;
            }

            if (unifiedSearchDefinitions.entitlementSearchDefinitions != null &&
                unifiedSearchDefinitions.entitlementSearchDefinitions.subscriptionSearchObjects != null)
            {
                // If we need to search by entitlements, we have A LOT of work to do now
                esQueryBuilder.SubscriptionsQuery = BuildMultipleSearchQuery(unifiedSearchDefinitions.entitlementSearchDefinitions.subscriptionSearchObjects, parentGroupId, true); ;
            }

            esQueryBuilder.SetGroupByValuesForUnifiedSearch();

            // WARNING has side effect - updates queryParser.Aggregations
            var requestBody = esQueryBuilder.BuildSearchQueryString(
                unifiedSearchDefinitions.shouldIgnoreDeviceRuleID,
                unifiedSearchDefinitions.shouldAddIsActiveTerm,
                unifiedSearchDefinitions.isGroupingOptionInclude);

            if (!string.IsNullOrEmpty(requestBody))
            {
                int httpStatus = 0;

                string indexes = ESUnifiedQueryBuilder.GetIndexes(unifiedSearchDefinitions, parentGroupId);

                if (!string.IsNullOrEmpty(indexes))
                {
                    string types = ESUnifiedQueryBuilder.GetTypes(unifiedSearchDefinitions);
                    string url = string.Format("{0}/{1}/{2}/_search", _elasticSearchApi.baseUrl, indexes, types);

                    if (!string.IsNullOrEmpty(unifiedSearchDefinitions.preference))
                    {
                        url = string.Format("{0}?preference={1}", url, unifiedSearchDefinitions.preference);
                    }

                    string queryResultString = _elasticSearchApi.SendPostHttpReq(url, ref httpStatus, string.Empty, string.Empty, requestBody, true);

                    if (httpStatus == ElasticSearchApi.STATUS_OK)
                    {
                        #region Process ElasticSearch result

                        // Parse results
                        ESAggregationsResult esAggregationResult = null;
                        Dictionary<ElasticSearchApi.ESAssetDocument, UnifiedSearchResult> topHitsMapping = null;

                        if (esQueryBuilder.Aggregations != null)
                        {
                            esAggregationResult = ESAggregationsResult.FullParse(queryResultString, esQueryBuilder.Aggregations);
                            topHitsMapping = MapTopHits(esAggregationResult, unifiedSearchDefinitions);
                        }

                        var assetsDocumentsDecoded = ESUtils.DecodeAssetSearchJsonObject(
                            queryResultString,
                            ref totalItems,
                            unifiedSearchDefinitions.extraReturnFields?.ToList());

                        if (assetsDocumentsDecoded?.Count > 0)
                        {
                            searchResultsList = new List<UnifiedSearchResult>();
                            extendedUnifiedSearchResults = new List<ExtendedUnifiedSearchResult>();
                            var idToDocument = new Dictionary<string, ElasticSearchApi.ESAssetDocument>();
                            foreach (var doc in assetsDocumentsDecoded)
                            {
                                var result = CreateUnifiedSearchResultFromESDocument(unifiedSearchDefinitions, doc);
                                searchResultsList.Add(result);
                                idToDocument.Add(result.AssetId, doc);

                                extendedUnifiedSearchResults.Add(new ExtendedUnifiedSearchResult(result, doc));
                            }

                            if (!_sortingService.IsSortingCompleted(unifiedSearchDefinitions))
                            {
                                IReadOnlyCollection<long> reorderedAssetIds = null;
                                if (unifiedSearchDefinitions.PriorityGroupsMappings == null || !unifiedSearchDefinitions.PriorityGroupsMappings.Any())
                                {
                                    reorderedAssetIds = _sortingService.GetReorderedAssetIds(unifiedSearchDefinitions, extendedUnifiedSearchResults);
                                }
                                else
                                {
                                    var priorityGroupsResults = extendedUnifiedSearchResults.GroupBy(r => r.Result.Score);
                                    var orderedIds = new List<long>();
                                    foreach (var priorityGroupsResult in priorityGroupsResults)
                                    {
                                        var reorderedIdsChunk = _sortingService.GetReorderedAssetIds(unifiedSearchDefinitions, priorityGroupsResult);
                                        if (reorderedIdsChunk == null)
                                        {
                                            log.Debug($"Chunk from priority group hasn't been processed. Asset Ids: [{string.Join(",", priorityGroupsResult.Select(x => x.AssetId))}]");
                                            continue;
                                        }

                                        orderedIds.AddRange(reorderedIdsChunk);
                                    }

                                    reorderedAssetIds = orderedIds;
                                }

                                // need to reorder items
                                if (reorderedAssetIds?.Count > 0)
                                {
                                    // Page results: check which results should be returned
                                    Dictionary<int, UnifiedSearchResult> idToResultDictionary = new Dictionary<int, UnifiedSearchResult>();
                                    // Map all results in dictionary
                                    searchResultsList.ForEach(item =>
                                    {
                                        int assetId = int.Parse(item.AssetId);
                                        if (item.AssetType == eAssetTypes.NPVR)
                                        {
                                            assetId = int.Parse(((RecordingSearchResult)item).RecordingId);
                                        }

                                        if (!idToResultDictionary.ContainsKey(assetId))
                                        {
                                            idToResultDictionary.Add(assetId, item);
                                        }
                                    });

                                    searchResultsList.Clear();
                                    var assetIds = reorderedAssetIds.Page(
                                        unifiedSearchDefinitions.pageSize,
                                        unifiedSearchDefinitions.pageIndex,
                                        out var illegalRequest)
                                        .ToArray();
                                    if (!illegalRequest)
                                    {
                                        foreach (int id in assetIds)
                                        {
                                            if (idToResultDictionary.TryGetValue(id, out var temporaryResult))
                                            {
                                                searchResultsList.Add(temporaryResult);
                                            }
                                        }
                                    }
                                }

                                var orderByFields = _sortingAdapter.ResolveOrdering(unifiedSearchDefinitions);
                                if (esAggregationResult != null
                                    && _esSortingService.IsBucketsReorderingRequired(orderByFields, unifiedSearchDefinitions.distinctGroup))
                                {
                                    var assetIds = searchResultsList.Select(item => long.Parse(item.AssetId)).ToList();
                                    ReorderBuckets(
                                        esAggregationResult,
                                        unifiedSearchDefinitions.pageIndex,
                                        unifiedSearchDefinitions.pageSize,
                                        unifiedSearchDefinitions.distinctGroup,
                                        idToDocument,
                                        assetIds);
                                }
                            }
                        }

                        if (esAggregationResult != null)
                        {
                            aggregationsResults = new List<AggregationsResult>
                            {
                                ConvertAggregationsResponse(
                                    esAggregationResult,
                                    unifiedSearchDefinitions.groupBy.Select(g => g.Key).ToList(),
                                    topHitsMapping)
                            }; // TODO if we add one element always, why it's an array?
                        }

                        #endregion
                    }
                    else if (httpStatus == ElasticSearchApi.STATUS_NOT_FOUND || httpStatus >= ElasticSearchApi.STATUS_INTERNAL_ERROR)
                    {
                        throw new System.Web.HttpException(httpStatus, queryResultString);
                    }
                }
            }

            return (searchResultsList);
        }

        public AggregationsResult UnifiedSearchForGroupBy(UnifiedSearchDefinitions search)
        {
            var orderByFields = _sortingAdapter.ResolveOrdering(search);
            var singleGroupByWithDistinct = search.groupBy?.Count == 1
                && search.groupBy.Single().Key == search.distinctGroup.Key;
            if (!singleGroupByWithDistinct)
            {
                throw new NotSupportedException($"Method should be used for single group.");
            }

            var groupBySearch = _sortingService.GetGroupBySortingStrategy(orderByFields)
                ?? throw new NotSupportedException($"Not supported group by with provided ordering.");

            // save original page and size, will be mutated later :(
            var pageSize = search.pageSize;
            var fromIndex = search.from > 0 ? search.from : search.pageIndex * pageSize;

            // prepare body and url
            var queryBuilder = new ESUnifiedQueryBuilder(_esSortingService, _sortingAdapter, _queryInitializer, search);
            groupBySearch.SetQueryPaging(search, queryBuilder);
            if (search.entitlementSearchDefinitions?.subscriptionSearchObjects != null)
            {
                queryBuilder.SubscriptionsQuery = BuildMultipleSearchQuery(search.entitlementSearchDefinitions.subscriptionSearchObjects, _partnerId, true);
            }

            queryBuilder.SetGroupByValuesForUnifiedSearch();
            var requestBody = queryBuilder.BuildSearchQueryString(search.shouldIgnoreDeviceRuleID, search.shouldAddIsActiveTerm, search.isGroupingOptionInclude); // WARNING has side effect - updates queryBuilder.Aggregations, used later
            var url = GetUrl(search, _partnerId);

            // send request
            int httpStatus = 0;
            var responseBody = _elasticSearchApi.SendPostHttpReq(url, ref httpStatus, string.Empty, string.Empty, requestBody, true);
            if (httpStatus == ElasticSearchApi.STATUS_NOT_FOUND || httpStatus >= ElasticSearchApi.STATUS_INTERNAL_ERROR) throw new HttpException(httpStatus, responseBody);
            if (httpStatus != ElasticSearchApi.STATUS_OK) throw new Exception("Elasticsearch responded with not OK status");

            // handle response
            var elasticAggregation = groupBySearch.HandleQueryResponse(search, pageSize, fromIndex, queryBuilder, responseBody);
            var topHitsMapping = MapTopHits(elasticAggregation, search);
            var aggregationsResult = ConvertAggregationsResponse(elasticAggregation, new List<string> { search.groupBy.Single().Key }, topHitsMapping);

            return aggregationsResult;
        }

        private string GetUrl(UnifiedSearchDefinitions unifiedSearchDefinitions, int parentGroupId)
        {
            string indexes = ESUnifiedQueryBuilder.GetIndexes(unifiedSearchDefinitions, parentGroupId); // ES index is on parent group id
            if (indexes.IsNullOrEmptyOrWhiteSpace()) throw new Exception("Empty Elasticsearch index");

            string types = ESUnifiedQueryBuilder.GetTypes(unifiedSearchDefinitions);

            string url = $"{_elasticSearchApi.baseUrl}/{indexes}/{types}/_search";
            if (!unifiedSearchDefinitions.preference.IsNullOrEmptyOrWhiteSpace())
            {
                url = $"{url}?preference={unifiedSearchDefinitions.preference}";
            }

            return url;
        }

        private Dictionary<ElasticSearchApi.ESAssetDocument, UnifiedSearchResult> MapTopHits(
            ESAggregationsResult aggregationResult, UnifiedSearchDefinitions definitions)
        {
            var result = new Dictionary<ElasticSearchApi.ESAssetDocument, UnifiedSearchResult>();

            Stack<ESAggregationResult> stack = new Stack<ESAggregationResult>();

            foreach (var aggregation in aggregationResult.Aggregations.Values)
            {
                stack.Push(aggregation);
            }

            // Breadth-first search by tree... or something similar
            while (stack.Count > 0)
            {
                var aggregation = stack.Pop();

                if (aggregation.hits != null && aggregation.hits.hits != null)
                {
                    foreach (var hit in aggregation.hits.hits)
                    {
                        hit.extraReturnFields = hit.extraReturnFields ?? new Dictionary<string, string>();
                        var unifiedSearchResult = CreateUnifiedSearchResultFromESDocument(definitions, hit);

                        result[hit] = unifiedSearchResult;
                    }
                }

                foreach (var sub in aggregation.Aggregations.Values)
                {
                    stack.Push(sub);
                }

                if (aggregation.buckets != null && aggregation.buckets.Count > 0)
                {
                    foreach (var bucket in aggregation.buckets)
                    {
                        foreach (var sub in bucket.Aggregations.Values)
                        {
                            stack.Push(sub);
                        }
                    }
                }
            }

            return result;
        }

        public static UnifiedSearchResult CreateUnifiedSearchResultFromESDocument(
            UnifiedSearchDefinitions definitions, ElasticSearchApi.ESAssetDocument doc)
        {
            UnifiedSearchResult result = null;
            string assetId = doc.asset_id.ToString();
            var assetType = ESUtils.ParseAssetType(doc.type);

            if (definitions.shouldReturnExtendedSearchResult)
            {
                result = new ExtendedSearchResult()
                {
                    AssetId = assetId,
                    m_dUpdateDate = doc.update_date,
                    AssetType = assetType,
                    EndDate = doc.end_date,
                    StartDate = doc.start_date,
                    Score = doc.score
                };

                if (doc.extraReturnFields != null)
                {
                    (result as ExtendedSearchResult).ExtraFields = new List<ApiObjects.KeyValuePair>();

                    foreach (var field in doc.extraReturnFields)
                    {
                        (result as ExtendedSearchResult).ExtraFields.Add(new ApiObjects.KeyValuePair()
                        {
                            key = field.Key,
                            value = field.Value
                        });
                    }
                }
            }
            else
            {
                if (assetType == eAssetTypes.NPVR)
                {
                    // After we searched for recordings, we need to replace their ID (recording ID) with the personal ID (domain recording)
                    if (definitions != null && definitions.recordingIdToSearchableRecordingMapping != null && definitions.recordingIdToSearchableRecordingMapping.Count > 0)
                    {
                        result = new RecordingSearchResult
                        {
                            AssetType = eAssetTypes.NPVR,
                            Score = doc.score,
                            RecordingId = assetId
                        };
                        if (definitions.recordingIdToSearchableRecordingMapping.ContainsKey(assetId))
                        {
                            // Replace ID
                            result.AssetId = definitions.recordingIdToSearchableRecordingMapping[assetId].DomainRecordingId.ToString();
                            (result as RecordingSearchResult).EpgId = definitions.recordingIdToSearchableRecordingMapping[assetId].EpgId.ToString();
                            (result as RecordingSearchResult).RecordingType = definitions.recordingIdToSearchableRecordingMapping[assetId].RecordingType;
                        }

                        if (doc.extraReturnFields.ContainsKey("epg_id") && (string.IsNullOrEmpty((result as RecordingSearchResult).EpgId) || (result as RecordingSearchResult).EpgId == "0"))
                        {
                            (result as RecordingSearchResult).EpgId = doc.extraReturnFields["epg_id"];
                        }
                    }
                    else
                    {
                        string epgId = string.Empty;

                        if (doc.extraReturnFields.ContainsKey("epg_id"))
                        {
                            epgId = doc.extraReturnFields["epg_id"];
                        }
                        else if (!string.IsNullOrEmpty(doc.epg_identifier))
                        {
                            epgId = doc.epg_identifier;
                        }

                        result = new RecordingSearchResult()
                        {
                            AssetId = assetId,
                            m_dUpdateDate = doc.update_date,
                            AssetType = assetType,
                            EpgId = epgId,
                            Score = doc.score,
                            RecordingId = assetId
                        };
                    }
                }
                else if (assetType == eAssetTypes.EPG && definitions.EpgFeatureVersion != EpgFeatureVersion.V1)
                {
                    result = new EpgSearchResult()
                    {
                        AssetId = assetId,
                        m_dUpdateDate = doc.update_date,
                        AssetType = assetType,
                        Score = doc.score,
                        DocumentId = doc.epg_couchbase_key
                    };
                }
                else
                {
                    result = new UnifiedSearchResult()
                    {
                        AssetId = assetId,
                        m_dUpdateDate = doc.update_date,
                        AssetType = assetType,
                        Score = doc.score
                    };
                }
            }

            return result;
        }

        private static void ReorderBuckets(
            ESAggregationsResult aggregationResult, int pageIndex, int pageSize,
            GroupByDefinition distinctGroup, Dictionary<string, ElasticSearchApi.ESAssetDocument> idToDocument, List<long> orderedIds)
        {
            var bucketMapping = new Dictionary<string, ESAggregationBucket>();
            var orderedBuckets = new List<ESAggregationBucket>();
            var alreadyContainedBuckets = new HashSet<ESAggregationBucket>();

            // first map all buckets by their grouping value
            foreach (var bucket in aggregationResult.Aggregations[distinctGroup.Key].buckets)
            {
                bucketMapping.Add(bucket.key, bucket);
            }

            // go over all the ordered IDs and reorder the buckets by the specific documents' order
            foreach (var id in orderedIds)
            {
                var doc = idToDocument[id.ToString()];

                if (doc.extraReturnFields.ContainsKey(distinctGroup.Value))
                {
                    // Pay attention! We use "to lower" because the bucket value is lowercased because it uses the analyzer. 
                    var groupingValue = doc.extraReturnFields[distinctGroup.Value].ToLower();

                    if (bucketMapping.ContainsKey(groupingValue))
                    {
                        var bucket = bucketMapping[groupingValue];

                        if (!alreadyContainedBuckets.Contains(bucket))
                        {
                            alreadyContainedBuckets.Add(bucket);
                            orderedBuckets.Add(bucket);

                            if (!bucket.Aggregations.ContainsKey(ESTopHitsAggregation.DEFAULT_NAME))
                            {
                                // Fake the top hit to be the first asset after sorting
                                bucket.Aggregations[ESTopHitsAggregation.DEFAULT_NAME] = new ESAggregationResult()
                                {
                                    hits = new ESHits()
                                    {
                                        hits = new List<ElasticSearchApi.ESAssetDocument>()
                                        {
                                            doc
                                        },
                                        total = bucket.doc_count
                                    }
                                };
                            }
                            else
                            {
                                var hits = bucket.Aggregations[ESTopHitsAggregation.DEFAULT_NAME].hits;

                                // bother doing this only if document isn't contained already
                                if (!hits.hits.Contains(doc))
                                {
                                    hits.hits.Clear();
                                    hits.hits.Add(doc);
                                }
                            }
                        }
                    }
                }
            }

            // Add the leftovers - the buckets that weren't included previously for some reason 
            // (shouldn't happen, will happen if something went wrong or if we have more than MAX_RESULTS)
            foreach (var bucket in aggregationResult.Aggregations[distinctGroup.Key].buckets)
            {
                if (!alreadyContainedBuckets.Contains(bucket))
                {
                    alreadyContainedBuckets.Add(bucket);
                    orderedBuckets.Add(bucket);
                }
            }

            bool illegalRequest = false;
            var pagedBuckets = orderedBuckets.Page(pageSize, pageIndex, out illegalRequest);

            // replace the original list with the ordered list
            aggregationResult.Aggregations[distinctGroup.Key].buckets = pagedBuckets.ToList();
        }

        private List<long> SortAssetsByStartDate(List<ElasticSearchApi.ESAssetDocument> assets, ApiObjects.SearchObjects.OrderDir orderDirection, Dictionary<int, string> associationTags,
            Dictionary<int, int> mediaTypeParent)
        {
            if (assets == null || assets.Count == 0)
            {
                return new List<long>();
            }

            bool search = false;
            Dictionary<string, DateTime> idToStartDate = new Dictionary<string, DateTime>();
            Dictionary<string, Dictionary<int, List<string>>> nameToTypeToId = new Dictionary<string, Dictionary<int, List<string>>>();
            Dictionary<int, List<string>> typeToNames = new Dictionary<int, List<string>>();

            #region Map documents name and initial start dates

            // Create mappings for later on
            foreach (var document in assets)
            {
                idToStartDate.Add(document.id, document.start_date);

                if (document.media_type_id > 0)
                {
                    if (!nameToTypeToId.ContainsKey(document.name))
                    {
                        nameToTypeToId[document.name] = new Dictionary<int, List<string>>();
                    }

                    if (!nameToTypeToId[document.name].ContainsKey(document.media_type_id))
                    {
                        nameToTypeToId[document.name][document.media_type_id] = new List<string>();
                    }

                    nameToTypeToId[document.name][document.media_type_id].Add(document.id);

                    if (!typeToNames.ContainsKey(document.media_type_id))
                    {
                        typeToNames[document.media_type_id] = new List<string>();
                    }

                    typeToNames[document.media_type_id].Add(document.name);
                }
            }

            #endregion

            #region Define Aggregations Search Query

            FilteredQuery filteredQuery = new FilteredQuery()
            {
                PageIndex = 0,
                PageSize = 1
            };

            filteredQuery.Filter = new QueryFilter();

            FilterCompositeType filterSettings = new FilterCompositeType(CutWith.AND);

            FilterCompositeType tagsFilter = new FilterCompositeType(CutWith.OR);

            // Filter data only to contain documents that have the specifiic tag
            foreach (var item in associationTags)
            {
                if (mediaTypeParent.ContainsKey(item.Key) &&
                    typeToNames.ContainsKey(mediaTypeParent[item.Key]))
                {
                    search = true;
                    ESTerms tagsTerms = new ESTerms(false)
                    {
                        Key = string.Format("tags.{0}", item.Value.ToLower())
                    };

                    tagsTerms.Value.AddRange(typeToNames[mediaTypeParent[item.Key]]);

                    tagsFilter.AddChild(tagsTerms);
                }
            }

            if (search)
            {
                ESTerm isActiveTerm = new ESTerm(true)
                {
                    Key = "is_active",
                    Value = "1"
                };

                string nowSearchString = DateTime.UtcNow.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT);

                ESRange startDateRange = new ESRange(false)
                {
                    Key = "start_date"
                };

                startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, nowSearchString));

                ESRange endDateRange = new ESRange(false)
                {
                    Key = "end_date"
                };

                // If we don't have any tag, use a "0=1" filter so query return 0 results instead of ALL results
                if (tagsFilter.IsEmpty())
                {
                    tagsFilter.AddChild(new ESTerm(true)
                    {
                        Key = "_id",
                        Value = "-1"
                    });
                }

                // Filter associated media by:
                // is_active = 1
                // start_date < NOW
                // end_date > NOW
                // tag is actually the current series
                filterSettings.AddChild(isActiveTerm);
                filterSettings.AddChild(startDateRange);
                filterSettings.AddChild(endDateRange);
                filterSettings.AddChild(tagsFilter);
                filteredQuery.Filter.FilterSettings = filterSettings;

                // Create an aggregation search object for each association tag we have
                foreach (var associationTag in associationTags)
                {
                    ESTerm filter = new ESTerm(true)
                    {
                        Key = "media_type_id",
                        // key of association tag is the child media type
                        Value = associationTag.Key.ToString()
                    };

                    ESFilterAggregation currentAggregation = new ESFilterAggregation(filter)
                    {
                        Name = associationTag.Value
                    };

                    ESBaseAggsItem subAggregation1 = new ESBaseAggsItem()
                    {
                        Name = associationTag.Value + "_sub1",
                        Field = string.Format("tags.{0}", associationTag.Value).ToLower(),
                        Type = eElasticAggregationType.terms
                    };

                    ESBaseAggsItem subAggregation2 = new ESBaseAggsItem()
                    {
                        Name = associationTag.Value + "_sub2",
                        Field = "start_date",
                        Type = eElasticAggregationType.stats
                    };

                    subAggregation1.SubAggrgations.Add(subAggregation2);
                    currentAggregation.SubAggrgations.Add(subAggregation1);

                    filteredQuery.Aggregations.Add(currentAggregation);
                }

                #endregion

                #region Get Aggregations Results

                string searchRequestBody = filteredQuery.ToString();
                string index = NamingHelper.GetMediaIndexAlias(_partnerId);

                string searchResults = _elasticSearchApi.Search(index, "media", ref searchRequestBody);

                ESAggregationsResult aggregationsResult =
                    ESAggregationsResult.FullParse(searchResults, filteredQuery.Aggregations);

                #endregion

                #region Process Aggregations Results

                if (aggregationsResult != null && aggregationsResult.Aggregations != null && aggregationsResult.Aggregations.Count > 0)
                {
                    foreach (var associationTag in associationTags)
                    {
                        int parentMediaType = mediaTypeParent[associationTag.Key];

                        if (aggregationsResult.Aggregations.ContainsKey(associationTag.Value))
                        {
                            ESAggregationResult currentResult = aggregationsResult.Aggregations[associationTag.Value];

                            ESAggregationResult firstSub;

                            if (currentResult.Aggregations.TryGetValue(associationTag.Value + "_sub1", out firstSub))
                            {
                                foreach (var bucket in firstSub.buckets)
                                {
                                    ESAggregationResult subBucket;

                                    if (bucket.Aggregations.TryGetValue(associationTag.Value + "_sub2", out subBucket))
                                    {
                                        // "series name" is the bucket's key
                                        string tagValue = bucket.key;

                                        if (nameToTypeToId.ContainsKey(tagValue) && nameToTypeToId[tagValue].ContainsKey(parentMediaType))
                                        {
                                            foreach (var assetId in nameToTypeToId[tagValue][parentMediaType])
                                            {
                                                string maximumStartDate = subBucket.max_as_string.ToString();

                                                idToStartDate[assetId] =
                                                    DateTime.ParseExact(maximumStartDate,
                                                    ElasticSearch.Common.Utils.ES_DATE_FORMAT, null);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            // Sort the list of key value pairs by the value (the start date)
            var sortedDictionary = idToStartDate.OrderBy(pair => pair.Value).ThenBy(pair => pair.Key);

            #region Create final, sorted, list

            List<long> sortedList = new List<long>();
            HashSet<int> alreadyContainedIds = new HashSet<int>();

            foreach (var currentId in sortedDictionary)
            {
                int id = int.Parse(currentId.Key);

                // Depending on direction - if it is ascending, insert Id at start. Otherwise at end
                if (orderDirection == ApiObjects.SearchObjects.OrderDir.DESC)
                {
                    sortedList.Insert(0, id);
                }
                else
                {
                    sortedList.Add(id);
                }

                alreadyContainedIds.Add(id);
            }

            // Add all ids that don't have stats
            foreach (var asset in assets)
            {
                int currentId = int.Parse(asset.id);

                if (!alreadyContainedIds.Contains(currentId))
                {
                    // Depending on direction - if it is ascending, insert Id at start. Otherwise at end
                    if (orderDirection == ApiObjects.SearchObjects.OrderDir.ASC)
                    {
                        sortedList.Insert(0, currentId);
                    }
                    else
                    {
                        sortedList.Add(currentId);
                    }
                }
            }

            #endregion

            return sortedList;
        }

        /// <summary>
        /// For a given list of asset Ids, returns a list of the same IDs, after sorting them by a specific statistics
        /// </summary>
        /// <param name="assetIds"></param>
        /// <param name="_partnerId"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <returns></returns>
        private List<long> SortAssetsByStats(List<long> assetIds,
            OrderBy orderBy, ApiObjects.SearchObjects.OrderDir orderDirection,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            assetIds = assetIds.Distinct().ToList();

            List<long> sortedList = null;
            HashSet<long> alreadyContainedIds = null;
            ConcurrentDictionary<string, List<StatisticsAggregationResult>> ratingsAggregationsDictionary =
                new ConcurrentDictionary<string, List<StatisticsAggregationResult>>();
            ConcurrentDictionary<string, ConcurrentDictionary<string, int>> countsAggregationsDictionary =
                new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();

            // we will use layered cache for asset stats for non-rating values and only if we don't have dates in filter
            if (startDate == null && endDate == null && orderBy != OrderBy.RATING)
            {
                Dictionary<string, int> sortValues = SortAssetsByStatsWithLayeredCache(assetIds, orderBy, orderDirection);
                ConcurrentDictionary<string, int> innerDictionary = new ConcurrentDictionary<string, int>();
                foreach (var sortValue in sortValues)
                {
                    // we don't check if the key exists since the SortAssetsByStatsWithLayeredCache function returns a value for all passed assetIds
                    innerDictionary[sortValue.Key] = sortValue.Value;
                }

                countsAggregationsDictionary["stats"] = innerDictionary;
            }
            else
            {
                GetAssetStatsValuesFromElasticSearch(assetIds, orderBy, startDate, endDate, ratingsAggregationsDictionary, countsAggregationsDictionary);
            }

            #region Process Aggregations

            // get a sorted list of the asset Ids that have statistical data in the aggregations dictionary
            sortedList = new List<long>();
            alreadyContainedIds = new HashSet<long>();

            // Ratings is a special case, because it is not based on count, but on average instead
            if (orderBy == ApiObjects.SearchObjects.OrderBy.RATING)
            {
                ProcessRatingsAggregationsResult(ratingsAggregationsDictionary, orderDirection, alreadyContainedIds, sortedList);
            }
            // If it is not ratings - just use count
            else
            {
                ProcessCountDictionaryResults(countsAggregationsDictionary, orderDirection, alreadyContainedIds, sortedList);
            }

            #endregion

            if (sortedList == null)
            {
                sortedList = new List<long>();
            }

            // Add all ids that don't have stats
            foreach (var currentId in assetIds)
            {
                if (alreadyContainedIds == null || !alreadyContainedIds.Contains(currentId))
                {
                    // Depending on direction - if it is ascending, insert Id at start. Otherwise at end
                    if (orderDirection == ApiObjects.SearchObjects.OrderDir.ASC)
                    {
                        sortedList.Insert(0, currentId);
                    }
                    else
                    {
                        sortedList.Add(currentId);
                    }
                }
            }

            return sortedList;
        }

        private void GetAssetStatsValuesFromElasticSearch(List<long> assetIds, OrderBy orderBy,
            DateTime? startDate, DateTime? endDate,
            ConcurrentDictionary<string, List<StatisticsAggregationResult>> ratingsAggregationsDictionary,
            ConcurrentDictionary<string, ConcurrentDictionary<string, int>> countsAggregationsDictionary)
        {
            #region Define Aggregation Query

            FilteredQuery filteredQuery = new FilteredQuery()
            {
                PageIndex = 0,
                PageSize = 1
            };

            filteredQuery.Filter = new QueryFilter();

            BaseFilterCompositeType filter = new FilterCompositeType(CutWith.AND);

            filter.AddChild(new ESTerm(true)
            {
                Key = "group_id",
                Value = _partnerId.ToString()
            });

            #region define date filter

            if ((startDate != null && startDate.HasValue && !startDate.Equals(DateTime.MinValue)) ||
                (endDate != null && endDate.HasValue && !endDate.Equals(DateTime.MaxValue)))
            {
                ESRange dateRange = new ESRange(false)
                {
                    Key = "action_date"
                };

                if (startDate != null && startDate.HasValue && !startDate.Equals(DateTime.MinValue))
                {
                    string sMin = startDate.Value.ToString(ESUtils.ES_DATE_FORMAT);
                    dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
                }

                if (endDate != null && endDate.HasValue && !endDate.Equals(DateTime.MaxValue))
                {
                    string sMax = endDate.Value.ToString(ESUtils.ES_DATE_FORMAT);
                    dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sMax));
                }

                filter.AddChild(dateRange);
            }

            #endregion

            #region define action filter

            string actionName = string.Empty;

            switch (orderBy)
            {
                case ApiObjects.SearchObjects.OrderBy.VIEWS:
                    {
                        actionName = NamingHelper.STAT_ACTION_FIRST_PLAY;
                        break;
                    }
                case ApiObjects.SearchObjects.OrderBy.RATING:
                    {
                        actionName = NamingHelper.STAT_ACTION_RATES;
                        break;
                    }
                case ApiObjects.SearchObjects.OrderBy.VOTES_COUNT:
                    {
                        actionName = NamingHelper.STAT_ACTION_RATES;
                        break;
                    }
                case ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER:
                    {
                        actionName = NamingHelper.STAT_ACTION_LIKE;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            ESTerm actionTerm = new ESTerm(false)
            {
                Key = "action",
                Value = actionName
            };

            filter.AddChild(actionTerm);

            #endregion

            #region Define IDs term

            ESTerms idsTerm = new ESTerms(true)
            {
                Key = "media_id"
            };

            idsTerm.Value.Add("0");

            filter.AddChild(idsTerm);

            #endregion

            filteredQuery.Filter.FilterSettings = filter;

            ESBaseAggsItem aggregations = null;

            // Ratings is a special case, because it is not based on count, but on average instead
            if (orderBy == ApiObjects.SearchObjects.OrderBy.RATING)
            {
                aggregations = new ESBaseAggsItem()
                {
                    Name = "stats",
                    Field = "media_id",
                    Type = eElasticAggregationType.terms
                };

                aggregations.SubAggrgations.Add(new ESBaseAggsItem()
                {
                    Name = "sub_stats",
                    Type = eElasticAggregationType.stats,
                    Field = NamingHelper.STAT_ACTION_RATE_VALUE_FIELD
                });
            }
            else
            {
                aggregations = new ESBaseAggsItem()
                {
                    Name = "stats",
                    Field = "media_id",
                    Type = eElasticAggregationType.terms
                };

                aggregations.SubAggrgations.Add(new ESBaseAggsItem()
                {
                    Name = NamingHelper.SUB_SUM_AGGREGATION_NAME,
                    Type = eElasticAggregationType.sum,
                    Field = NamingHelper.STAT_ACTION_COUNT_VALUE_FIELD,
                    Missing = 1
                });
            }

            filteredQuery.Aggregations.Add(aggregations);

            #endregion

            #region Split call of aggregations query to pieces

            int aggregationsSize = ApplicationConfiguration.Current.ElasticSearchConfiguration.StatSortBulkSize.Value;

            //Start MultiThread Call
            List<Task> tasks = new List<Task>();

            // Split the request to small pieces, to avoid timeout exceptions
            for (int assetIndex = 0; assetIndex < assetIds.Count; assetIndex += aggregationsSize)
            {
                idsTerm.Value.Clear();

                // Convert partial Ids to strings
                idsTerm.Value.AddRange(assetIds.Skip(assetIndex).Take(aggregationsSize).Select(id => id.ToString()));

                string aggregationsRequestBody = filteredQuery.ToString();

                string index = NamingHelper.GetStatisticsIndexName(_partnerId);

                try
                {
                    LogContextData contextData = new LogContextData();
                    // Create a task for the search and merge of partial aggregations
                    Task task = Task.Run(() =>
                    {
                        contextData.Load();
                        // Get aggregations results
                        string aggregationsResults = _elasticSearchApi.Search(index, ESUtils.ES_STATS_TYPE, ref aggregationsRequestBody);

                        if (orderBy == ApiObjects.SearchObjects.OrderBy.RATING)
                        {
                            if (ratingsAggregationsDictionary != null)
                            {
                                // Parse string into dictionary
                                var partialDictionary = ESAggregationsResult.DeserializeStatisticsAggregations(aggregationsResults, "sub_stats");

                                // Run on partial dictionary and merge into main dictionary
                                foreach (var mainPart in partialDictionary)
                                {
                                    if (!ratingsAggregationsDictionary.ContainsKey(mainPart.Key))
                                    {
                                        ratingsAggregationsDictionary[mainPart.Key] = new List<StatisticsAggregationResult>();
                                    }

                                    foreach (var singleResult in mainPart.Value)
                                    {
                                        ratingsAggregationsDictionary[mainPart.Key].Add(singleResult);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (countsAggregationsDictionary != null)
                            {
                                // Parse string into dictionary
                                var partialDictionary = ESAggregationsResult.DeserializeAggrgations<string>(aggregationsResults, NamingHelper.SUB_SUM_AGGREGATION_NAME);

                                // Run on partial dictionary and merge into main dictionary
                                foreach (var mainPart in partialDictionary)
                                {
                                    if (!countsAggregationsDictionary.ContainsKey(mainPart.Key))
                                    {
                                        countsAggregationsDictionary[mainPart.Key] = new ConcurrentDictionary<string, int>();
                                    }

                                    foreach (var singleResult in mainPart.Value)
                                    {
                                        countsAggregationsDictionary[mainPart.Key][singleResult.Key] = singleResult.Value;
                                    }
                                }
                            }
                        }
                    });

                    tasks.Add(task);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error in SortAssetsByStats, Exception: {0}", ex);
                }
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in SortAssetsByStats (WAIT ALL), Exception: {0}", ex);
            }

            #endregion
        }

        private Tuple<Dictionary<string, int>, bool> SortAssetsByStatsDelegate(Dictionary<string, object> funcParams)
        {
            var countsAggregationsDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();
            var result = new Dictionary<string, int>();
            bool success = true;
            List<long> assetIds = new List<long>();
            OrderBy orderBy = OrderBy.NONE;

            try
            {
                // extract from funcParams
                if (funcParams.ContainsKey("orderBy"))
                {
                    orderBy = (OrderBy)funcParams["orderBy"];
                }

                // if we don't have missing keys - all ids should be sent
                if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                {
                    assetIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => long.Parse(x)).ToList();
                }
                else if (funcParams.ContainsKey("assetIds"))
                {
                    assetIds = (List<long>)funcParams["assetIds"];
                }

                GetAssetStatsValuesFromElasticSearch(assetIds, orderBy, null, null, null, countsAggregationsDictionary);

                var statsDictionary = countsAggregationsDictionary["stats"];

                // fill dictionary of asset-id..stats-value (if it doesn't exist in ES, fill it with a 0)
                foreach (var assetId in assetIds)
                {
                    string dictionaryKey =
                        LayeredCacheKeys.GetAssetStatsSortKey(assetId.ToString(), orderBy.ToString());

                    if (!statsDictionary.ContainsKey(assetId.ToString()))
                    {
                        result[dictionaryKey] = 0;
                    }
                    else
                    {
                        result[dictionaryKey] = statsDictionary[assetId.ToString()];
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                log.ErrorFormat("Error when trying to sort assets by stats. group Id = {0}, ex = {1}", _partnerId, ex);
            }

            return new Tuple<Dictionary<string, int>, bool>(result, success);
        }

        private Dictionary<string, int> SortAssetsByStatsWithLayeredCache(List<long> assetIds, OrderBy orderBy, ApiObjects.SearchObjects.OrderDir orderDirection)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            Dictionary<string, int> layeredCacheResult = new Dictionary<string, int>();
            if (assetIds != null && assetIds.Count > 0)
            {
                try
                {
                    Dictionary<string, string> keyToOriginalValueMap = assetIds.Select(x => x.ToString()).ToDictionary(x => LayeredCacheKeys.GetAssetStatsSortKey(x, orderBy.ToString()));
                    Dictionary<string, List<string>> invalidationKeys =
                        keyToOriginalValueMap.Keys.ToDictionary(x => x, x => new List<string>() { LayeredCacheKeys.GetAssetStatsInvalidationKey(_partnerId) });

                    Dictionary<string, object> funcParams = new Dictionary<string, object>();
                    funcParams.Add("orderBy", orderBy);
                    funcParams.Add("assetIds", assetIds);

                    if (!_layeredCache.GetValues<int>(keyToOriginalValueMap, ref layeredCacheResult, SortAssetsByStatsDelegate, funcParams,
                        _partnerId, LayeredCacheConfigNames.ASSET_STATS_SORT_CONFIG_NAME, invalidationKeys))
                    {
                        log.ErrorFormat("Failed getting asset stats from cache, ids: {0}:", assetIds.Count < 100 ? string.Join(",", assetIds) : string.Join(",", assetIds.Take(100)));
                    }
                    else
                    {
                        foreach (var item in layeredCacheResult)
                        {
                            string key = keyToOriginalValueMap[item.Key];
                            result.Add(key, item.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Failed SortAssetsByStatsLayeredCache", ex);
                }
            }

            return result;
        }

        /// <summary>
        /// After receiving a result from ES server, process it to create a list of Ids with the given order
        /// </summary>
        /// <param name="statsDictionary"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="alreadyContainedIds"></param>
        /// <returns></returns>
        private static void ProcessCountDictionaryResults(ConcurrentDictionary<string, ConcurrentDictionary<string, int>> statsDictionary,
            ApiObjects.SearchObjects.OrderDir orderDirection, HashSet<long> alreadyContainedIds, List<long> sortedList)
        {
            if (statsDictionary != null && statsDictionary.Count > 0)
            {
                ConcurrentDictionary<string, int> statResult;

                //retrieve specific stats result
                statsDictionary.TryGetValue("stats", out statResult);

                if (statResult != null && statResult.Count > 0)
                {
                    var sortedStatsDictionary = statResult.OrderBy(o => o.Value).ThenBy(o => o.Key).Reverse();

                    // We base this section on the assumption that aggregations request is sorted, descending
                    foreach (var currentValue in sortedStatsDictionary)
                    {
                        int count = statResult[currentValue.Key];

                        int currentId;

                        if (int.TryParse(currentValue.Key, out currentId))
                        {
                            // Depending on direction - if it is ascending, insert Id at start. Otherwise at end
                            if (orderDirection == ApiObjects.SearchObjects.OrderDir.ASC)
                            {
                                sortedList.Insert(0, currentId);
                            }
                            else
                            {
                                sortedList.Add(currentId);
                            }

                            alreadyContainedIds.Add(currentId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// After receiving a result from ES server, process it to create a list of Ids with the given order
        /// </summary>
        /// <param name=")"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="alreadyContainedIds"></param>
        /// <returns></returns>
        private static void ProcessRatingsAggregationsResult(ConcurrentDictionary<string, List<StatisticsAggregationResult>> statisticsDictionary,
            ApiObjects.SearchObjects.OrderDir orderDirection, HashSet<long> alreadyContainedIds, List<long> sortedList)
        {
            if (statisticsDictionary != null && statisticsDictionary.Count > 0)
            {
                List<StatisticsAggregationResult> statResult;

                //retrieve specific aggregation result
                statisticsDictionary.TryGetValue("stats", out statResult);

                if (statResult != null && statResult.Count > 0)
                {
                    // sort ASCENDING - different than normal execution!
                    statResult.Sort(new AggregationsComparer(AggregationsComparer.eCompareType.Average));

                    foreach (var result in statResult)
                    {
                        int currentId;

                        // Depending on direction - if it is ascending, insert Id at end. Otherwise at start
                        if (int.TryParse(result.key, out currentId))
                        {
                            if (orderDirection == ApiObjects.SearchObjects.OrderDir.ASC)
                            {
                                sortedList.Insert(0, currentId);
                            }
                            else
                            {
                                sortedList.Add(currentId);
                            }

                            alreadyContainedIds.Add(currentId);
                        }
                    }
                }
            }
        }

        #endregion

        public List<UnifiedSearchResult> GetAssetsUpdateDates(List<UnifiedSearchResult> assets, ref int totalItems, int pageSize, int pageIndex, bool shouldIgnoreRecordings = false)
        {
            List<UnifiedSearchResult> validAssets = new List<UnifiedSearchResult>();
            totalItems = 0;

            bool shouldSearchEpg = false;
            bool shouldSearchMedia = false;
            string media = "media";
            string epg = "epg";

            // Realize what asset types do we have
            shouldSearchMedia = assets.Exists(asset => asset.AssetType == eAssetTypes.MEDIA);
            shouldSearchEpg = assets.Exists(asset => asset.AssetType == eAssetTypes.EPG);

            bool shouldSearch = shouldSearchEpg || shouldSearchMedia;

            if (shouldSearch)
            {
                // Build indexes and types string - for URL
                string indexes = string.Empty;
                string types = string.Empty;

                if (shouldSearchEpg && shouldSearchMedia)
                {
                    indexes = string.Format("{0},{0}_epg", _partnerId);
                    types = string.Format("{0},{1}", media, epg);
                }
                else if (shouldSearchMedia)
                {
                    indexes = _partnerId.ToString();
                    types = media;
                }
                else if (shouldSearchEpg)
                {
                    indexes = string.Format("{0}_epg", _partnerId);
                    types = epg;
                }

                // Build complete URL
                string url = string.Format("{0}/{1}/{2}/_search", _elasticSearchApi.baseUrl, indexes, types);

                // Build request body with the assistance of unified query builder
                List<KeyValuePair<eAssetTypes, string>> assetsPairs = assets.Select(asset =>
                    new KeyValuePair<eAssetTypes, string>(asset.AssetType, asset.AssetId)).ToList();

                string requestBody = ESUnifiedQueryBuilder.BuildGetUpdateDatesString(assetsPairs, shouldIgnoreRecordings);

                int httpStatus = 0;

                // Perform search
                string queryResultString = _elasticSearchApi.SendPostHttpReq(url, ref httpStatus, string.Empty, string.Empty, requestBody, true);

                if (httpStatus == ElasticSearchApi.STATUS_OK)
                {
                    #region Process ElasticSearch result

                    var jsonObj = JObject.Parse(queryResultString);

                    if (jsonObj != null)
                    {
                        JToken tempToken;
                        totalItems = ((tempToken = jsonObj.SelectToken("hits.total")) == null ? 0 : (int)tempToken);

                        if (totalItems > 0)
                        {
                            foreach (var item in jsonObj.SelectToken("hits.hits"))
                            {
                                string typeString = ((tempToken = item.SelectToken("_type")) == null ? string.Empty : (string)tempToken);
                                eAssetTypes assetType = ESUtils.ParseAssetType(typeString);

                                string assetIdField = string.Empty;

                                switch (assetType)
                                {
                                    case eAssetTypes.MEDIA:
                                        {
                                            assetIdField = "fields.media_id";
                                            break;
                                        }
                                    case eAssetTypes.EPG:
                                        {
                                            assetIdField = "fields.epg_id";
                                            break;
                                        }
                                    default:
                                        {
                                            break;
                                        }
                                }

                                string id = ((tempToken = item.SelectToken("_id")) == null ? string.Empty : (string)tempToken);
                                DateTime update_date = ESUtils.ExtractDateFromToken(item, "fields.update_date");

                                // Find the asset in the list with this ID, set its update date
                                assets.First(result => result.AssetId == id).m_dUpdateDate = update_date;
                            }
                        }
                    }

                    #endregion
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

            //if (!illegalRequest)
            //{
            //    finalList = pagedList.ToList();
            //}
            //else
            //{
            //    finalList = null;
            //}


            return validAssets;
        }

        public List<int> GetEntitledEpgLinearChannels(UnifiedSearchDefinitions definitions)
        {
            List<int> result = new List<int>();
            var queryParser = new ESUnifiedQueryBuilder(_esSortingService, _sortingAdapter, _queryInitializer, definitions)
            {
                PageIndex = 0,
                PageSize = 0,
                GetAllDocuments = true
            };

            if (definitions.entitlementSearchDefinitions != null &&
                definitions.entitlementSearchDefinitions.subscriptionSearchObjects != null)
            {
                // If we need to search by entitlements, we have A LOT of work to do now
                BoolQuery boolQuery = BuildMultipleSearchQuery(definitions.entitlementSearchDefinitions.subscriptionSearchObjects, _partnerId);
                queryParser.SubscriptionsQuery = boolQuery;
            }

            string requestBody = queryParser.BuildSearchQueryString(definitions.shouldIgnoreDeviceRuleID, definitions.shouldAddIsActiveTerm, definitions.isGroupingOptionInclude);

            if (!string.IsNullOrEmpty(requestBody))
            {
                int httpStatus = 0;

                string indexes = ESUnifiedQueryBuilder.GetIndexes(definitions, _partnerId);
                string types = ESUnifiedQueryBuilder.GetTypes(definitions);
                string url = string.Format("{0}/{1}/{2}/_search", _elasticSearchApi.baseUrl, indexes, types);

                string queryResultString = _elasticSearchApi.SendPostHttpReq(url, ref httpStatus, string.Empty, string.Empty, requestBody, true);

                if (httpStatus == ElasticSearchApi.STATUS_OK)
                {
                    #region Process ElasticSearch result

                    int totalItems = 0;
                    List<ElasticSearchApi.ESAssetDocument> assetsDocumentsDecoded = ESUtils.DecodeAssetSearchJsonObject(queryResultString, ref totalItems);

                    if (assetsDocumentsDecoded != null && assetsDocumentsDecoded.Count > 0)
                    {
                        foreach (var asset in assetsDocumentsDecoded)
                        {
                            string epgIdentifier = asset.epg_identifier;
                            int epgIdentifierInt;

                            // check if ID is a valid number
                            if (int.TryParse(epgIdentifier, out epgIdentifierInt) && epgIdentifierInt > 0)
                            {
                                result.Add(epgIdentifierInt);
                            }
                        }
                    }

                    #endregion
                }
            }

            return result;
        }

        #region Tags

        public List<TagValue> SearchTags(TagSearchDefinitions definitions, out int totalItems)
        {
            List<TagValue> result = new List<TagValue>();
            totalItems = 0;

            #region Build filtered query

            BoolQuery query = new BoolQuery();

            if (definitions.TopicId != 0)
            {
                // Create the bool query of the topic Id and the value (autocomplete)
                ESTerm topicTerm = new ESTerm(true)
                {
                    Key = "topic_id",
                    Value = definitions.TopicId.ToString()
                };

                query.AddChild(topicTerm, CutWith.AND);
            }

            IESTerm valueTerm = null;

            CatalogGroupCache catalogGroupCache = null;

            if (!string.IsNullOrEmpty(definitions.AutocompleteSearchValue) || !string.IsNullOrEmpty(definitions.ExactSearchValue))
            {
                // if we have a specific language - we will search it only
                if (definitions.Language != null)
                {
                    valueTerm = CreateTagValueTerm(definitions.Language, definitions.AutocompleteSearchValue, definitions.ExactSearchValue);
                }
                else
                {
                    // if we don't have a specific language - we will search all languageas using OR between them
                    BoolQuery boolQuery = new BoolQuery();
                    catalogGroupCache = catalogGroupCache ?? GetCatalogGroupCache();

                    var languageObjs = catalogGroupCache.LanguageMapByCode.Values;
                    foreach (var language in languageObjs)
                    {
                        var currentTerm = CreateTagValueTerm(language, definitions.AutocompleteSearchValue, definitions.ExactSearchValue);

                        boolQuery.AddChild(currentTerm, CutWith.OR);
                    }

                    valueTerm = boolQuery;
                }
            }

            if (valueTerm != null)
            {
                query.AddChild(valueTerm, CutWith.AND);
            }

            if (definitions.TagIds != null && definitions.TagIds.Count > 0)
            {
                ESTerms idsTerm = new ESTerms(true)
                {
                    Key = "tag_id"
                };

                idsTerm.Value.AddRange(definitions.TagIds.Select(id => id.ToString()));

                query.AddChild(idsTerm, CutWith.AND);
            }

            FilteredQuery filteredQuery = new FilteredQuery()
            {
                PageIndex = definitions.PageIndex,
                PageSize = definitions.PageSize,
                Query = query
            };

            filteredQuery.ESSort.Add(new ESOrderObj()
            {
                m_sOrderValue = "value",
                m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC
            });

            filteredQuery.ReturnFields.Clear();

            #endregion

            #region Perform search

            string searchQueryString = filteredQuery.ToString();

            string type = string.Empty;

            // if we have a specific language
            if (definitions.Language != null)
            {
                if (!definitions.Language.IsDefault)
                {
                    type = string.Format("tag_{0}", definitions.Language.Code);
                }
                else
                {
                    type = "tag";
                }
            }
            // no language = all languages
            else
            {
                StringBuilder typeBuilder = new StringBuilder();

                // combine all language codes together
                catalogGroupCache = catalogGroupCache ?? GetCatalogGroupCache();
                var languages = catalogGroupCache.LanguageMapByCode.Values;
                foreach (var language in languages)
                {
                    string currentType = string.Empty;

                    if (!language.IsDefault)
                    {
                        currentType = string.Format("tag_{0}", language.Code);
                    }
                    else
                    {
                        currentType = "tag";
                    }

                    typeBuilder.Append(currentType);
                    typeBuilder.Append(",");
                }

                if (typeBuilder.Length > 0)
                {
                    typeBuilder.Remove(typeBuilder.Length - 1, 1);
                }

                type = typeBuilder.ToString();
            }

            string index = ESUtils.GetGroupMetadataIndex(definitions.GroupId);

            string searchResultString = _elasticSearchApi.Search(index, type, ref searchQueryString);

            #endregion

            #region Parse result

            if (string.IsNullOrEmpty(searchResultString))
            {
                return result;
            }

            var jsonObj = JObject.Parse(searchResultString);

            if (jsonObj != null)
            {
                JToken tempToken;
                totalItems = ((tempToken = jsonObj.SelectToken("hits.total")) == null ? 0 : (int)tempToken);

                if (totalItems > 0)
                {
                    foreach (var item in jsonObj.SelectToken("hits.hits"))
                    {
                        var itemSource = item["_source"];
                        var tagValue = itemSource.ToObject<TagValue>();

                        if (string.IsNullOrEmpty(tagValue.value))
                        {
                            foreach (var subSubItem in itemSource)
                            {
                                JProperty property = subSubItem as JProperty;

                                if (property != null && property.Name.Contains("value"))
                                {
                                    tagValue.value = (itemSource[property.Name] as JValue).Value.ToString();

                                    break;
                                }
                            }
                        }

                        result.Add(tagValue);
                    }
                }
            }

            #endregion

            return result;
        }

        private static IESTerm CreateTagValueTerm(LanguageObj language, string autocompleteValue, string exactValue)
        {
            IESTerm term = null;
            string key = "value";

            // not default language - add suffix
            if (!language.IsDefault)
            {
                key = string.Format("value_{0}", language.Code);
            }

            string value = string.Empty;

            // whether it is autocomplete or not
            if (!string.IsNullOrEmpty(autocompleteValue))
            {
                key = string.Format("{0}.autocomplete", key);
                value = autocompleteValue.ToLower();
                term = new ESMatchQuery(null)
                {
                    Field = key,
                    eOperator = CutWith.AND,
                    Query = value
                };
            }
            else if (!string.IsNullOrEmpty(exactValue))
            {
                value = exactValue.ToLower();

                //BEO-10960
                if (IsDiacriticsAccentsTag(value))
                {
                    term = new ESMatchQuery()
                    {
                        eOperator = CutWith.AND,
                        Field = key,
                        Query = value
                    };
                }
                else
                {
                    term = new ESTerm(false)
                    {
                        Key = key,
                        Value = value
                    };
                }
            }


            return term;
        }

        private static bool IsDiacriticsAccentsTag(string accentedStr)
        {
            return accentedStr.Any(c => IsLetterWithDiacritics(c));
        }

        static bool IsLetterWithDiacritics(char c)
        {
            var s = c.ToString().Normalize(NormalizationForm.FormD);
            return (s.Length > 1) &&
                   char.IsLetter(s[0]) &&
                   s.Skip(1).All(c2 => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c2) == System.Globalization.UnicodeCategory.NonSpacingMark);
        }

        public ApiObjects.Response.Status DeleteTag(long tagId)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();
            string index = ESUtils.GetGroupMetadataIndex(_partnerId);

            // dictionary contains all language ids and its  code (string)
            var languages = GetCatalogGroupCache().LanguageMapByCode.Values;

            ESTerm term = new ESTerm(true)
            {
                Key = "tag_id",
                Value = tagId.ToString()
            };

            ESQuery query = new ESQuery(term);
            string queryString = query.ToString();

            foreach (var language in languages)
            {
                string type = "tag";

                if (!language.IsDefault)
                {
                    type = string.Format("tag_{0}", language.Code);
                }

                bool deleteResult = _elasticSearchApi.DeleteDocsByQuery(index, type, ref queryString);

                if (!deleteResult)
                {
                    status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                    status.Message = "Failed performing delete query";
                }
            }

            return status;
        }

        public ApiObjects.Response.Status DeleteTagsByTopic(long topicId)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();
            string index = ESUtils.GetGroupMetadataIndex(_partnerId);

            // dictionary contains all language ids and its  code (string)
            var languages = GetCatalogGroupCache().LanguageMapByCode.Values;

            ESTerm term = new ESTerm(true)
            {
                Key = "topic_id",
                Value = topicId.ToString()
            };

            ESQuery query = new ESQuery(term);
            string queryString = query.ToString();

            foreach (var language in languages)
            {
                string type = "tag";

                if (!language.IsDefault)
                {
                    type = string.Format("tag_{0}", language.Code);
                }

                bool deleteResult = _elasticSearchApi.DeleteDocsByQuery(index, type, ref queryString);

                if (!deleteResult)
                {
                    status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                    status.Message = "Failed performing delete query";
                }
            }

            return status;
        }

        public ApiObjects.Response.Status UpdateTag(TagValue tag)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();
            string index = ESUtils.GetGroupMetadataIndex(_partnerId);

            if (!_elasticSearchApi.IndexExists(index))
            {
                log.Error($"Error - Index of type metadata for group {_partnerId} does not exist");
                status.Message = "Index does not exist";

                return status;
            }

            List<TagValue> tagsToInsert = new List<TagValue>();

            int defaultLanguageId = tag.languageId;

            if (defaultLanguageId == 0)
            {
                defaultLanguageId = GetCatalogGroupCache().GetDefaultLanguage().ID;
            }

            tagsToInsert.Add(new TagValue()
            {
                createDate = tag.createDate,
                languageId = defaultLanguageId,
                tagId = tag.tagId,
                topicId = tag.topicId,
                updateDate = tag.updateDate,
                value = tag.value
            });

            var catalogGroupCache = GetCatalogGroupCache();
            foreach (var languageContainer in tag.TagsInOtherLanguages)
            {
                int languageId = 0;

                if (catalogGroupCache.LanguageMapByCode.ContainsKey(languageContainer.m_sLanguageCode3))
                {
                    languageId = catalogGroupCache.LanguageMapByCode[languageContainer.m_sLanguageCode3].ID;

                    if (languageId > 0)
                    {
                        tagsToInsert.Add(new TagValue()
                        {
                            createDate = tag.createDate,
                            languageId = languageId,
                            tagId = tag.tagId,
                            topicId = tag.topicId,
                            updateDate = tag.updateDate,
                            value = languageContainer.m_sValue
                        });
                    }
                }
            }

            foreach (var tagToInsert in tagsToInsert)
            {
                // insert only tags with valid language id
                if (tagToInsert.languageId == 0)
                {
                    continue;
                }

                var language = catalogGroupCache.LanguageMapById[tagToInsert.languageId];
                string suffix = null;

                if (!language.IsDefault)
                {
                    suffix = language.Code;
                }

                string type = "tag";

                if (!language.IsDefault)
                {
                    type = string.Format("tag_{0}", language.Code);
                }

                // Serialize tag and create a bulk request for it
                string serializedTag = _serializer.SerializeTagValueObject(tagToInsert, language);

                string id = string.Format("{0}_{1}", tagToInsert.tagId, tagToInsert.languageId);
                bool insertResult = _elasticSearchApi.InsertRecord(index, type, id, serializedTag);

                if (!insertResult)
                {
                    status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                    status.Message = "Failed performing insert query";
                }
            }

            return status;
        }

        #endregion

        #region Channels

        public List<int> SearchChannels(ChannelSearchDefinitions definitions, ref int totalItems)
        {
            List<int> result = new List<int>();

            #region Build filtered query

            BoolQuery query = new BoolQuery();

            ESTerms idsTerm = null;
            ESMatchQuery matchQuery = null;

            if (!string.IsNullOrEmpty(definitions.AutocompleteSearchValue))
            {
                matchQuery = new ESMatchQuery
                {
                    Field = "name.autocomplete",
                    eOperator = CutWith.AND,
                    Query = definitions.AutocompleteSearchValue.ToLower()
                };
            }
            else if (!string.IsNullOrEmpty(definitions.ExactSearchValue))
            {
                matchQuery = new ESMatchQuery
                {
                    Field = "name",
                    Query = definitions.ExactSearchValue
                };
            }
            else if (definitions.SpecificChannelIds != null && definitions.SpecificChannelIds.Count > 0)
            {
                idsTerm = new ESTerms(true)
                {
                    Key = "_id"
                };

                idsTerm.Value.AddRange(definitions.SpecificChannelIds.Select(x => x.ToString()));
            }

            if (matchQuery != null)
            {
                query.AddChild(matchQuery, CutWith.AND);
            }
            else if (idsTerm != null)
            {
                query.AddChild(idsTerm, CutWith.AND);
            }

            QueryFilter queryFilter = null;
            BaseFilterCompositeType filterSettings = null;

            if (!definitions.isAllowedToViewInactiveAssets)
            {
                filterSettings = new FilterCompositeType(CutWith.AND);
                filterSettings.AddChild(new ESTerm(true)
                {
                    Key = CHANNEL_SEARCH_IS_ACTIVE,
                    Value = CHANNEL_SEARCH_IS_ACTIVE_VALUE
                });

                queryFilter = new QueryFilter() { FilterSettings = filterSettings };
            }

            if (definitions.AssetUserRuleId > 0)
            {
                if (queryFilter == null)
                {
                    filterSettings = new FilterCompositeType(CutWith.AND);
                    filterSettings.AddChild(new ESTerm(true)
                    {
                        Key = CHANNEL_ASSET_USER_RULE_ID,
                        Value = definitions.AssetUserRuleId.ToString()
                    });

                    queryFilter = new QueryFilter() { FilterSettings = filterSettings };
                }
                else
                {
                    queryFilter.FilterSettings.AddChild(new ESTerm(true)
                    {
                        Key = CHANNEL_ASSET_USER_RULE_ID,
                        Value = definitions.AssetUserRuleId.ToString()
                    });
                }
            }

            FilteredQuery filteredQuery = new FilteredQuery()
            {
                PageIndex = definitions.PageIndex,
                PageSize = definitions.PageSize,
                Query = query,
                Filter = queryFilter
            };

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

            filteredQuery.ESSort.Add(new ESOrderObj()
            {
                m_sOrderValue = orderValue,
                m_eOrderDir = definitions.OrderDirection
            });

            filteredQuery.ReturnFields.Clear();

            #endregion

            #region Perform search

            string searchQueryString = filteredQuery.ToString();

            string type = "channel";

            string index = ESUtils.GetGroupChannelIndex(definitions.GroupId);

            string searchResultString = _elasticSearchApi.Search(index, type, ref searchQueryString);

            #endregion

            #region Parse result

            var jsonObj = JObject.Parse(searchResultString);

            if (jsonObj != null)
            {
                JToken tempToken;
                totalItems = ((tempToken = jsonObj.SelectToken("hits.total")) == null ? 0 : (int)tempToken);

                if (totalItems > 0)
                {
                    foreach (var item in jsonObj.SelectToken("hits.hits"))
                    {
                        var source = item["_source"];


                        string id = ((tempToken = source.SelectToken("channel_id")) == null ? string.Empty : (string)tempToken);
                        int channelId = 0;
                        if (int.TryParse(id, out channelId) && channelId > 0)
                        {
                            result.Add(channelId);
                        }
                    }
                }
            }

            #endregion

            return result;
        }

        #endregion

        #region Asset Stats and company

        public void GetAssetStats(List<int> assetIDs, DateTime startDate,
            DateTime endDate, StatsType type, ref Dictionary<int, AssetStatsResult> assetIDsToStatsMapping)
        {
            string index = NamingHelper.GetStatisticsIndexName(_partnerId);

            switch (type)
            {
                case StatsType.MEDIA:
                    {
                        List<string> aggregations = new List<string>(3);
                        aggregations.Add(BuildSlidingWindowCountAggregationRequest(_partnerId, assetIDs, startDate, endDate, NamingHelper.STAT_ACTION_FIRST_PLAY, true)); // views count
                        aggregations.Add(BuildSlidingWindowCountAggregationRequest(_partnerId, assetIDs, startDate, endDate, NamingHelper.STAT_ACTION_LIKE));
                        aggregations.Add(BuildSlidingWindowStatisticsAggregationRequest(_partnerId, assetIDs, startDate, endDate, NamingHelper.STAT_ACTION_RATES, NamingHelper.STAT_ACTION_RATE_VALUE_FIELD));

                        string esResp = _elasticSearchApi.MultiSearch(index, ESUtils.ES_STATS_TYPE, aggregations, null);

                        List<string> responses = ParseResponsesFromMultiAggregations(esResp);
                        string currResp = responses[0];
                        Dictionary<string, Dictionary<int, int>> viewsRaw = ESAggregationsResult.DeserializeAggrgations<int>(currResp, NamingHelper.SUB_SUM_AGGREGATION_NAME);
                        currResp = responses[1];
                        Dictionary<string, Dictionary<int, int>> likesRaw = ESAggregationsResult.DeserializeAggrgations<int>(currResp);
                        currResp = responses[2];
                        Dictionary<string, List<StatisticsAggregationResult>> ratesRaw = ESAggregationsResult.DeserializeStatisticsAggregations(currResp, "sub_stats");

                        Dictionary<int, int> views, likes;
                        List<StatisticsAggregationResult> rates = null;
                        viewsRaw.TryGetValue(NamingHelper.STAT_SLIDING_WINDOW_AGGREGATION_NAME, out views);
                        likesRaw.TryGetValue(NamingHelper.STAT_SLIDING_WINDOW_AGGREGATION_NAME, out likes);
                        ratesRaw.TryGetValue(NamingHelper.STAT_SLIDING_WINDOW_AGGREGATION_NAME, out rates);
                        InjectResultsIntoAssetStatsResponse(assetIDsToStatsMapping, views, likes, rates);
                        break;
                    }
                case StatsType.EPG:
                    {
                        // in epg we bring just likes
                        string likesAggregations = BuildSlidingWindowCountAggregationRequest(_partnerId, assetIDs, startDate, endDate, NamingHelper.STAT_ACTION_LIKE);
                        string searchResponse = _elasticSearchApi.Search(index, ESUtils.ES_STATS_TYPE, ref likesAggregations);

                        if (!string.IsNullOrEmpty(searchResponse))
                        {
                            Dictionary<string, Dictionary<int, int>> likesRaw = ESAggregationsResult.DeserializeAggrgations<int>(searchResponse);
                            Dictionary<int, int> likes = null;
                            likesRaw.TryGetValue(NamingHelper.STAT_SLIDING_WINDOW_AGGREGATION_NAME, out likes);

                            if (likes != null && likes.Count > 0)
                            {
                                InjectResultsIntoAssetStatsResponse(assetIDsToStatsMapping, new Dictionary<int, int>(0), likes,
                                    new List<StatisticsAggregationResult>(0));
                            }
                        }

                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private static string BuildSlidingWindowCountAggregationRequest(int groupID, List<int> mediaIDs, DateTime startDate, DateTime endDate,
            string action, bool setSubSum = false)
        {
            #region Define Aggregation Query

            ElasticSearch.Searcher.FilteredQuery filteredQuery = new ElasticSearch.Searcher.FilteredQuery()
            {
                PageIndex = 0,
                PageSize = 0,
                ZeroSize = true
            };

            filteredQuery.ReturnFields.Clear();

            filteredQuery.Filter = new ElasticSearch.Searcher.QueryFilter();

            BaseFilterCompositeType filter = new FilterCompositeType(CutWith.AND);
            filter.AddChild(new ESTerm(true)
            {
                Key = "group_id",
                Value = groupID.ToString()
            });

            #region define date filter

            if (!startDate.Equals(DateTime.MinValue) || !endDate.Equals(DateTime.MaxValue))
            {
                ESRange dateRange = new ESRange(false)
                {
                    Key = "action_date"
                };
                string sMax = endDate.ToString(ESUtils.ES_DATE_FORMAT);
                string sMin = startDate.ToString(ESUtils.ES_DATE_FORMAT);

                if (!startDate.Equals(DateTime.MinValue))
                {
                    dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
                }

                if (!endDate.Equals(DateTime.MaxValue))
                {
                    dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sMax));
                }

                filter.AddChild(dateRange);
            }

            #endregion

            #region define action filter

            ESTerm esActionTerm = new ESTerm(false)
            {
                Key = "action",
                Value = action
            };
            filter.AddChild(esActionTerm);

            #endregion

            #region define media id filter

            ESTerms esMediaIdTerms = new ESTerms(true)
            {
                Key = "media_id"
            };
            esMediaIdTerms.Value.AddRange(mediaIDs.Select(item => item.ToString()));
            filter.AddChild(esMediaIdTerms);

            #endregion

            #region define order filter

            // if no ordering is specified the default is order by count descending

            #endregion

            filteredQuery.Filter.FilterSettings = filter;

            ESBaseAggsItem aggregation = new ESBaseAggsItem()
            {
                Field = "media_id",
                Name = NamingHelper.STAT_SLIDING_WINDOW_AGGREGATION_NAME,
                Type = eElasticAggregationType.terms,
                ShardSize = 0
            };

            if (setSubSum)
            {
                aggregation.SubAggrgations.Add(new ESBaseAggsItem()
                {
                    Name = NamingHelper.SUB_SUM_AGGREGATION_NAME,
                    Type = eElasticAggregationType.sum,
                    Field = NamingHelper.STAT_ACTION_COUNT_VALUE_FIELD,
                    Missing = 1
                });
            }

            filteredQuery.Aggregations.Add(aggregation);

            #endregion

            return filteredQuery.ToString();
        }

        private List<int> SlidingWindowCountAggregations(List<int> lMediaIds, DateTime dtStartDate,
            DateTime dtEndDate, string action)
        {
            List<int> result = new List<int>(lMediaIds);

            var assetIds = lMediaIds.Select(id => (long)id).ToList();
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

            var orderedList = _statisticsSortStrategy.SortAssetsByStats(assetIds, orderBy, ApiObjects.SearchObjects.OrderDir.DESC, _partnerId, dtStartDate, dtEndDate);

            result = orderedList.Select(id => (int)id).ToList();

            return result;
        }

        internal Dictionary<int, int> SlidingWindowCountAggregationsMappings(List<int> lMediaIds, DateTime dtStartDate,
            DateTime dtEndDate, string action)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();

            string aggregationsQuery = BuildSlidingWindowCountAggregationRequest(_partnerId, lMediaIds, dtStartDate, dtEndDate, action);

            //Search
            string index = NamingHelper.GetStatisticsIndexName(_partnerId);

            string retval = _elasticSearchApi.Search(index, ESUtils.ES_STATS_TYPE, ref aggregationsQuery);

            if (!string.IsNullOrEmpty(retval))
            {
                //Get aggregations results
                Dictionary<string, Dictionary<string, int>> aggregationResults = ESAggregationsResult.DeserializeAggrgations<string>(retval, NamingHelper.SUB_SUM_AGGREGATION_NAME);

                if (aggregationResults != null && aggregationResults.Count > 0)
                {
                    Dictionary<string, int> aggregationResult;
                    //retrieve channel_views aggregations results
                    aggregationResults.TryGetValue(NamingHelper.STAT_SLIDING_WINDOW_AGGREGATION_NAME, out aggregationResult);

                    if (aggregationResult != null && aggregationResult.Count > 0)
                    {
                        foreach (string key in aggregationResult.Keys)
                        {
                            int count = aggregationResult[key];

                            int nMediaId;
                            if (int.TryParse(key, out nMediaId) && !result.ContainsKey(nMediaId))
                            {
                                result.Add(nMediaId, count);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static string BuildSlidingWindowStatisticsAggregationRequest(int groupID, List<int> mediaIDs, DateTime startDate,
            DateTime endDate, string action, string valueField)
        {
            #region Define Aggregation Query

            ElasticSearch.Searcher.FilteredQuery filteredQuery = new ElasticSearch.Searcher.FilteredQuery()
            {
                PageIndex = 0,
                PageSize = 0,
                ZeroSize = true
            };
            filteredQuery.Filter = new ElasticSearch.Searcher.QueryFilter();

            BaseFilterCompositeType filter = new FilterCompositeType(CutWith.AND);
            filter.AddChild(new ESTerm(true)
            {
                Key = "group_id",
                Value = groupID.ToString()
            });

            #region define date filter

            if (!startDate.Equals(DateTime.MinValue) || !endDate.Equals(DateTime.MaxValue))
            {
                ESRange dateRange = new ESRange(false)
                {
                    Key = "action_date"
                };
                string sMax = endDate.ToString(ESUtils.ES_DATE_FORMAT);
                string sMin = startDate.ToString(ESUtils.ES_DATE_FORMAT);

                if (!startDate.Equals(DateTime.MinValue))
                {
                    dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
                }

                if (!endDate.Equals(DateTime.MaxValue))
                {
                    dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sMax));
                }

                filter.AddChild(dateRange);
            }

            #endregion

            #region define action filter

            ESTerm esActionTerm = new ESTerm(false)
            {
                Key = "action",
                Value = action
            };
            filter.AddChild(esActionTerm);

            #endregion

            #region define media id filter

            ESTerms esMediaIdTerms = new ESTerms(true)
            {
                Key = "media_id"
            };
            esMediaIdTerms.Value.AddRange(mediaIDs.Select(item => item.ToString()));
            filter.AddChild(esMediaIdTerms);

            #endregion

            filteredQuery.Filter.FilterSettings = filter;

            #endregion

            var aggregation = new ESBaseAggsItem()
            {
                Name = NamingHelper.STAT_SLIDING_WINDOW_AGGREGATION_NAME,
                Field = "media_id",
                Type = eElasticAggregationType.terms,
                ShardSize = 0
            };

            aggregation.SubAggrgations.Add(new ESBaseAggsItem()
            {
                Name = "sub_stats",
                Type = eElasticAggregationType.stats,
                Field = valueField
            });

            filteredQuery.Aggregations.Add(aggregation);

            return filteredQuery.ToString();
        }

        private List<int> SlidingWindowStatisticsAggregations(List<int> lMediaIds, DateTime dtStartDate,
            DateTime dtEndDate, string action, string valueField, AggregationsComparer.eCompareType compareType)
        {
            List<int> result = new List<int>(lMediaIds);

            var assetIds = lMediaIds.Select(id => (long)id).ToList();
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

            var orderedList = _statisticsSortStrategy.SortAssetsByStats(assetIds, orderBy, ApiObjects.SearchObjects.OrderDir.DESC, _partnerId, dtStartDate, dtEndDate);

            result = orderedList.Select(id => (int)id).ToList();

            return result;
        }

        private static List<string> ParseResponsesFromMultiAggregations(string esResp)
        {
            List<string> res = new List<string>();

            if (!string.IsNullOrEmpty(esResp))
            {
                JObject jObj = JObject.Parse(esResp);
                JToken responses = jObj["responses"];
                if (responses != null && responses.Count() > 0)
                {
                    foreach (var response in responses)
                    {
                        res.Add(response.ToString());
                    }
                }
            }

            return res;
        }

        private static void InjectResultsIntoAssetStatsResponse(Dictionary<int, AssetStatsResult> assetIDsToStatsMapping,
            Dictionary<int, int> views, Dictionary<int, int> likes, List<StatisticsAggregationResult> rates)
        {
            if (assetIDsToStatsMapping != null)
            {
                // views and likes
                foreach (KeyValuePair<int, AssetStatsResult> kvp in assetIDsToStatsMapping)
                {
                    if (views != null && views.ContainsKey(kvp.Key))
                    {
                        kvp.Value.m_nViews = views[kvp.Key];
                    }

                    if (likes != null && likes.ContainsKey(kvp.Key))
                    {
                        kvp.Value.m_nLikes = likes[kvp.Key];
                    }
                }

                if (rates != null)
                {
                    // rates
                    for (int i = 0; i < rates.Count; i++)
                    {
                        int assetId = 0;

                        if (Int32.TryParse(rates[i].key, out assetId) && assetId > 0 && assetIDsToStatsMapping.ContainsKey(assetId))
                        {
                            assetIDsToStatsMapping[assetId].m_nVotes = rates[i].count;
                            assetIDsToStatsMapping[assetId].m_dRate = rates[i].avg;
                        }
                    }
                }
            }
        }

        // Testable
        // mid priority
        public List<int> OrderMediaBySlidingWindow(ApiObjects.SearchObjects.OrderBy orderBy, bool isDesc, int pageSize, int PageIndex, List<int> media, DateTime windowTime)
        {
            List<int> result;
            DateTime now = DateTime.UtcNow;
            switch (orderBy)
            {
                case OrderBy.VIEWS:

                    result = SlidingWindowCountAggregations(media, windowTime, now, NamingHelper.STAT_ACTION_FIRST_PLAY);
                    break;
                case OrderBy.RATING:
                    result = SlidingWindowStatisticsAggregations(media, windowTime, now, NamingHelper.STAT_ACTION_RATES, NamingHelper.STAT_ACTION_RATE_VALUE_FIELD,
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

        public List<int> UtilsSlidingWindowStatisticsAggregations(List<int> lMediaIds,
            DateTime dtStartDate, string action, string valueField, AggregationsComparer.eCompareType compareType)
        {
            List<int> result = new List<int>();

            #region Define Aggregations Query

            ElasticSearch.Searcher.FilteredQuery filteredQuery = new ElasticSearch.Searcher.FilteredQuery() { PageIndex = 0, PageSize = 1 };
            filteredQuery.Filter = new ElasticSearch.Searcher.QueryFilter();

            BaseFilterCompositeType filter = new FilterCompositeType(CutWith.AND);
            filter.AddChild(new ESTerm(true) { Key = "group_id", Value = _partnerId.ToString() });

            #region define date filter

            ESRange dateRange = new ESRange(false) { Key = "action_date" };
            string sMax = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string sMin = dtStartDate.ToString("yyyyMMddHHmmss");
            dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
            dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sMax));
            filter.AddChild(dateRange);

            #endregion

            #region define action filter

            ESTerm esActionTerm = new ESTerm(false) { Key = "action", Value = action };
            filter.AddChild(esActionTerm);

            #endregion

            #region define media id filter

            ESTerms esMediaIdTerms = new ESTerms(true) { Key = "media_id" };
            esMediaIdTerms.Value.AddRange(lMediaIds.Select(item => item.ToString()));
            filter.AddChild(esMediaIdTerms);

            #endregion

            filteredQuery.Filter.FilterSettings = filter;

            var aggregations = new ESBaseAggsItem()
            {
                Name = "sliding_window",
                Field = "media_id",
                Type = eElasticAggregationType.terms,
            };

            aggregations.SubAggrgations.Add(new ESBaseAggsItem()
            {
                Name = "sub_stats",
                Type = eElasticAggregationType.stats,
                Field = NamingHelper.STAT_ACTION_RATE_VALUE_FIELD
            });

            filteredQuery.Aggregations.Add(aggregations);

            #endregion

            string aggregationsQuery = filteredQuery.ToString();


            //Search
            string index = NamingHelper.GetStatisticsIndexName(_partnerId);

            string retval = _elasticSearchApi.Search(index, ESUtils.ES_STATS_TYPE, ref aggregationsQuery);

            if (!string.IsNullOrEmpty(retval))
            {
                //Get aggregations results
                Dictionary<string, List<StatisticsAggregationResult>> statisticsResults =
                    ESAggregationsResult.DeserializeStatisticsAggregations(retval, "sub_stats");

                if (statisticsResults != null && statisticsResults.Count > 0)
                {
                    List<StatisticsAggregationResult> aggregationResults;
                    //retrieve channel_views aggregations results
                    statisticsResults.TryGetValue("sliding_window", out aggregationResults);

                    if (aggregationResults != null && aggregationResults.Count > 0)
                    {
                        int mediaId;

                        aggregationResults.Sort(new AggregationsComparer(compareType));

                        foreach (var stats in aggregationResults)
                        {
                            if (int.TryParse(stats.key, out mediaId))
                            {
                                result.Add(mediaId);
                            }
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Statistics

        public bool SetupSocialStatisticsDataIndex()
        {
            var statisticsIndex = NamingHelper.GetStatisticsIndexName(_partnerId);

            var analyzers = new List<string>();
            var filters = new List<string>();
            return _elasticSearchApi.BuildIndex(statisticsIndex, NUM_OF_SHARDS, NUM_OF_REPLICAS, analyzers, filters);
        }

        public bool InsertSocialStatisticsData(SocialActionStatistics action)
        {
            bool result = false;

            Guid guid = Guid.NewGuid();
            string statisticsIndex = NamingHelper.GetStatisticsIndexName(_partnerId);

            try
            {
                string actionStatsJson = Newtonsoft.Json.JsonConvert.SerializeObject(action);

                result = _elasticSearchApi.InsertRecord(statisticsIndex, ESUtils.ES_STATS_TYPE, guid.ToString(), actionStatsJson);

                if (!result)
                {
                    log.Debug("InsertStatisticsToES " + string.Format("Was unable to insert record to ES. index={0};type={1};doc={2}",
                        statisticsIndex, ESUtils.ES_STATS_TYPE, actionStatsJson));
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
            string index = NamingHelper.GetStatisticsIndexName(_partnerId);

            try
            {
                if (_elasticSearchApi.IndexExists(index))
                {
                    var queryBuilder = new ESStatisticsQueryBuilder(_partnerId, socialSearch);
                    var queryString = queryBuilder.BuildQueryString();
                    return _elasticSearchApi.DeleteDocsByQuery(index, ESUtils.ES_STATS_TYPE, ref queryString);
                }
            }
            catch (Exception ex)
            {
                log.DebugFormat("DeleteActionFromES Failed ex={0}, index={1};type={2}", ex, index, ESUtils.ES_STATS_TYPE);
            }

            return false;
        }

        private List<StatisticsView> DecodeStatisticsSearchJsonObject(string sObj, ref int totalItems)
        {
            List<StatisticsView> documents = null;
            try
            {
                var jsonObj = JObject.Parse(sObj);

                if (jsonObj != null)
                {
                    JToken tempToken;
                    totalItems = ((tempToken = jsonObj.SelectToken("hits.total")) == null ? 0 : (int)tempToken);

                    if (totalItems > 0)
                    {
                        documents = jsonObj.SelectToken("hits.hits").Select(item => new StatisticsView()
                        {
                            ID = ESUtils.ExtractValueFromToken<string>(item, "_id"),
                            GroupID = ESUtils.ExtractValueFromToken<int>(item, "group_id"),
                            MediaID = ESUtils.ExtractValueFromToken<int>(item, "media_id")
                        }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                documents = null;
                log.Error("Error - " + string.Format("Json Deserialization failed for ElasticSearch Media request. Execption={0}", ex.Message), ex);
            }

            return documents;
        }

        #endregion

        #region Ip to Country

        public string SetupIPToCountryIndex()
        {
            string newIndexName = NamingHelper.GetNewUtilsIndexString();
            string ipToCountryType = "iptocountry";
            string ipV6ToCountryType = "ipv6tocountry";
            int numOfShards = NUM_OF_SHARDS;
            int numOfReplicas = NUM_OF_REPLICAS;

            bool indexExists = _elasticSearchApi.IndexExists(newIndexName);

            if (!indexExists)
            {
                List<string> analyzers = new List<string>()
                    {
                        LOWERCASE_ANALYZER
                    };

                indexExists = _elasticSearchApi.BuildIndex(newIndexName, numOfShards, numOfReplicas, analyzers, new List<string>());
            }

            #region Ip

            // Insert mapping for name field - default mapping isn't good for us
            ESMappingObj ipIndexMapping = new ESMappingObj(ipToCountryType);

            ipIndexMapping.AddProperty(new BasicMappingPropertyV2()
            {
                name = "name",
                type = eESFieldType.STRING,
                index = eMappingIndex.analyzed,
                analyzer = "lowercase_analyzer"
            });

            _elasticSearchApi.InsertMapping(newIndexName, ipToCountryType, ipIndexMapping.ToString());

            #endregion

            #region IpV6

            // Insert mapping for name field - default mapping isn't good for us
            ESMappingObj ipV6IndexMapping = new ESMappingObj(ipV6ToCountryType);

            ipV6IndexMapping.AddProperty(new BasicMappingPropertyV2()
            {
                name = "name",
                type = eESFieldType.STRING,
                index = eMappingIndex.analyzed,
                analyzer = "lowercase_analyzer"
            });

            #endregion

            _elasticSearchApi.InsertMapping(newIndexName, ipV6ToCountryType, ipV6IndexMapping.ToString());

            return newIndexName;
        }

        public bool InsertDataToIPToCountryIndex(string newIndexName,
            List<IPV4> ipV4ToCountryMapping, List<IPV6> ipV6ToCountryMapping)
        {
            string ipV6ToCountryType = "ipv6tocountry";
            string ipV4ToCountryType = "iptocountry";

            var bulkObjects = new List<ESBulkRequestObj<string>>();

            if (ipV6ToCountryMapping != null)
            {
                foreach (IPV6 row in ipV6ToCountryMapping)
                {
                    string serializedMapping = SerializeIPV6Mapping(row);

                    bulkObjects.Add(new ESBulkRequestObj<string>()
                    {
                        docID = row.id,
                        index = newIndexName,
                        type = ipV6ToCountryType,
                        document = serializedMapping
                    });

                    if (bulkObjects.Count >= 5000)
                    {
                        Task<object> t = Task<object>.Factory.StartNew(() => _elasticSearchApi.CreateBulkRequest(bulkObjects));
                        t.Wait();
                        bulkObjects = new List<ESBulkRequestObj<string>>();
                    }
                }

                if (bulkObjects.Count > 0)
                {
                    Task<object> t = Task<object>.Factory.StartNew(() => _elasticSearchApi.CreateBulkRequest(bulkObjects));
                    t.Wait();
                }
            }
            if (ipV4ToCountryMapping != null)
            {
                var ipV4bulkObjects = new List<ESBulkRequestObj<string>>();

                foreach (IPV4 row in ipV4ToCountryMapping)
                {
                    string serializedMapping = SerialiezIPV4Mapping(row);

                    bulkObjects.Add(new ESBulkRequestObj<string>()
                    {
                        docID = row.id,
                        index = newIndexName,
                        type = ipV4ToCountryType,
                        document = serializedMapping
                    });

                    if (bulkObjects.Count >= 5000)
                    {
                        Task<object> t = Task<object>.Factory.StartNew(() => _elasticSearchApi.CreateBulkRequest(bulkObjects));
                        t.Wait();
                        bulkObjects = new List<ESBulkRequestObj<string>>();
                    }
                }

                if (bulkObjects.Count > 0)
                {
                    Task<object> t = Task<object>.Factory.StartNew(() => _elasticSearchApi.CreateBulkRequest(bulkObjects));
                    t.Wait();
                }
            }

            return true;
        }

        public bool PublishIPToCountryIndex(string newIndexName)
        {
            bool result = true;
            // Switch index alias + Delete old indices handling
            string alias = "utils";
            bool currentIndexExists = _elasticSearchApi.IndexExists(alias);

            List<string> oldIndices = null;

            try
            {
                oldIndices = _elasticSearchApi.GetAliases(alias);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error when getting aliases of {0}, ex={1}", alias, ex);
            }

            bool switchIndexResult = _elasticSearchApi.SwitchIndex(newIndexName, alias, oldIndices);

            if (!switchIndexResult)
            {
                log.ErrorFormat("Failed switching index for new index name = {0}, index alias = {1}", newIndexName, alias);
                result = false;
            }

            if (switchIndexResult && oldIndices != null && oldIndices.Count > 0)
            {
                _elasticSearchApi.DeleteIndices(oldIndices);
            }

            return result;
        }

        private string SerialiezIPV4Mapping(IPV4 ipv4)
        {
            string result = string.Empty;

            long ipFrom = ipv4.ip_from;
            long ipTo = ipv4.ip_to;
            int countryId = ipv4.country_id;
            string code = ipv4.code;
            string name = ipv4.name;
            name = ElasticSearch.Common.Utils.ReplaceDocumentReservedCharacters(name, false);

            result = string.Concat("{",
                string.Format("\"ip_from\": {0}, \"ip_to\": {1}, \"country_id\": {2}, \"code\": \"{3}\", \"name\": \"{4}\" ",
                                ipFrom, ipTo, countryId, code, name),
                "}");

            return result;
        }

        private string SerializeIPV6Mapping(IPV6 ipv6)
        {
            ipv6.name = ElasticSearch.Common.Utils.ReplaceDocumentReservedCharacters(ipv6.name, false);
            return Newtonsoft.Json.JsonConvert.SerializeObject(ipv6);
        }

        public Country GetCountryByCountryName(string countryName)
        {
            Country country = null;
            try
            {
                if (string.IsNullOrEmpty(countryName))
                {
                    return country;
                }

                // Build query for getting country
                QueryFilter filter = new QueryFilter();
                FilterCompositeType composite = new FilterCompositeType(CutWith.AND);
                ESTerm term = new ESTerm(false)
                {
                    Key = "name",
                    Value = countryName.ToLower()
                };
                composite.AddChild(term);

                filter.FilterSettings = composite;

                var query = new FilteredQuery(true)
                {
                    PageIndex = 0,
                    PageSize = 1,
                    Filter = filter
                };

                query.ReturnFields.Clear();
                query.ReturnFields.AddRange(new List<string>()
                {
                    {string.Format("\"{0}\"", "country_id")},
                    {string.Format("\"{0}\"", "name")},
                    {string.Format("\"{0}\"", "code")},
                    {string.Format("\"{0}\"", "_id")}
                });

                string searchQuery = query.ToString();

                // Perform search
                string searchResult = _elasticSearchApi.Search("utils", "iptocountry", ref searchQuery);

                // parse search reult to json object
                country = ParseSearchResultToCountry(searchResult);
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetCountryByCountryName for countryName: {0}", countryName), ex);
            }

            return country;
        }

        public Country GetCountryByIp(string ip, out bool searchSuccess)
        {
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
                var query = handler.BuildFilteredQueryForIp(ipValue);
                var searchQuery = query.ToString();

                // Perform search
                var searchResult = _elasticSearchApi.SearchWithStatus("utils", handler.IndexType, ref searchQuery);

                if (searchResult.Item2 != 200)
                {
                    log.Error($"Error - Search query failed. query={searchQuery}; " +
                              $"explanation={searchResult.Item1}; statusCode: {searchResult.Item2}");
                    return null;
                }

                searchSuccess = true;
                // parse search reult to json object
                var country = ParseSearchResultToCountry(searchResult.Item1);
                return country;
            }

            return null;
        }


        private static Country ParseSearchResultToCountry(string searchResult)
        {
            var jsonObj = JObject.Parse(searchResult);
            if (jsonObj == null) { return null; }

            var tempToken = jsonObj.SelectToken("hits.total");

            // check total items
            int totalItems = tempToken == null ? 0 : (int)tempToken;
            if (totalItems <= 0) { return null; }

            // get country from first (and hopefully only) result 
            if (jsonObj.SelectToken("hits.hits").First().SelectToken("fields") is JObject jObj && jObj.HasValues)
            {
                string country_id = string.Empty, code = string.Empty, name = string.Empty, id = string.Empty;
                foreach (JProperty jProp in jObj.Properties())
                {
                    if (jProp != null && jProp.HasValues)
                    {
                        string key = jProp.Name;
                        if (jProp.Value is JArray jArray && jArray.Count > 0)
                        {
                            string value = jArray[0].ToString();
                            if (!string.IsNullOrEmpty(key))
                            {
                                switch (key)
                                {
                                    case "name":
                                        name = value;
                                        break;
                                    case "country_id":
                                        country_id = value;
                                        break;
                                    case "countryId":
                                        country_id = value;
                                        break;
                                    case "code":
                                        code = value;
                                        break;
                                    case "_id":
                                        id = value;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(code) && int.TryParse(country_id, out int countryId))
                {
                    log.DebugFormat("ParseSearchResultToCountry - the result (network) ID is:{0}.", id);
                    var country = new Country() { Id = countryId, Code = code, Name = name };
                    return country;
                }
            }

            return null;
        }

        #endregion

        #region Programs

        public List<string> GetChannelPrograms(int channelId, DateTime startDate, DateTime endDate, List<ESOrderObj> esOrderObjs)
        {
            string index = GetProgramIndexAlias(_partnerId);
            string type = "epg";

            var query = new FilteredQuery(true);

            var channelTerm = new ESTerm(true) { Key = "epg_channel_id", Value = channelId.ToString() };

            var endDateRange = new ESRange(false, "end_date", eRangeComp.LTE, endDate.ToString(ESUtils.ES_DATE_FORMAT));
            var startDateRange = new ESRange(false, "start_date", eRangeComp.GTE, startDate.ToString(ESUtils.ES_DATE_FORMAT));

            var filterCompositeType = new FilterCompositeType(CutWith.AND);
            filterCompositeType.AddChild(endDateRange);
            filterCompositeType.AddChild(startDateRange);
            filterCompositeType.AddChild(channelTerm);


            var queryFilter= new QueryFilter()
            {
                FilterSettings = filterCompositeType
            };

            query.Filter = queryFilter;
            query.ReturnFields.Clear();
            query.AddReturnField("document_id");
            query.AddReturnField("epg_id");

            if (esOrderObjs != null)
            {
                foreach (ESOrderObj item in esOrderObjs)
                {
                    query.ESSort.Add(item);
                }
            }

            var epgFeatureVersion = _groupSettingsManager.GetEpgFeatureVersion(_partnerId);
            if (epgFeatureVersion == EpgFeatureVersion.V3)
            {
                Helper.WrapFilterWithCommittedOnlyTransactionsForEpgV3(_partnerId, queryFilter, _elasticSearchApi);
            }

            // get the epg document ids from elasticsearch
            var searchQuery = query.ToString();



            var searchResult = _elasticSearchApi.Search(index, type, ref searchQuery);

            JObject json = JObject.Parse(searchResult);
            var hits = (json["hits"]["hits"] as JArray);

            List<string> documentIds = null;

            // Checking is new Epg ingest here as well to avoid calling GetEpgCBKey if we already called elastic and have all required coument Ids
            var isNewEpgIngest = epgFeatureVersion != EpgFeatureVersion.V1;
            if (isNewEpgIngest)
            {
                documentIds = hits.Select(hit => ESUtils.ExtractValueFromToken<string>(hit["fields"], "document_id")).ToList();
            }
            else
            {
                var epgIds = hits.Select(hit => ESUtils.ExtractValueFromToken<long>(hit["fields"], "epg_id")).ToList();
                documentIds = epgIds.Select(epgId => GetEpgCBKey(epgId)).ToList();
            }

            return documentIds;
        }

        private string GetProgramIndexAlias(int groupId)
        {
            return $"{groupId}_epg";
        }

        public List<string> GetEpgCBDocumentIdsByEpgId(IEnumerable<long> epgIds, IEnumerable<LanguageObj> languages)
        {
            languages = languages ?? Enumerable.Empty<LanguageObj>();

            // Build query for getting programs
            var query = new FilteredQuery(true);
            var filter = new QueryFilter();

            // basic initialization
            query.PageIndex = 0;
            query.PageSize = 0;
            query.ReturnFields.Clear();
            query.AddReturnField("epg_id");
            query.AddReturnField("document_id");

            var composite = new FilterCompositeType(CutWith.AND);
            var terms = new ESTerms("epg_id", epgIds.ToArray());
            composite.AddChild(terms);

            filter.FilterSettings = composite;
            var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(_partnerId);
            if (epgFeatureVersion == EpgFeatureVersion.V3)
            {
                Helper.WrapFilterWithCommittedOnlyTransactionsForEpgV3(_partnerId, filter, _elasticSearchApi);
            }
            query.Filter = filter;

            string searchQuery = query.ToString();

            var alias = GetProgramIndexAlias(_partnerId);
            var languageIndexes = languages.Select(langCode => langCode.IsDefault ? "epg" : $"epg_{langCode.Code}");
            var indexType = languages.Any() ? string.Join(",", languageIndexes) : "epg";

            var searchResult = _elasticSearchApi.Search(alias, indexType, ref searchQuery);

            if (string.IsNullOrEmpty(searchResult))
            {
                throw new Exception($"GetEpgCBDocumentIdByEpgIdFromElasticsearch > " +
                    $"Got empty results from elasticsearch epgIds:[{string.Join(",", epgIds)}], " +
                    $"_partnerId:[{_partnerId}], langCodes:[{string.Join(",", languages)}]");
            }

            var json = JObject.Parse(searchResult);

            var hits = (json["hits"]["hits"] as JArray);
            var results = hits.Select(hit => ESUtils.ExtractValueFromToken<string>(hit["fields"], "document_id")).ToList();

            //support for recordings
            var resultsEpgIds = hits.Select(hit => long.Parse(ESUtils.ExtractValueFromToken<string>(hit["fields"], "epg_id"))).ToList();

            var except = epgIds.ToList().Except(resultsEpgIds).ToList();
            if (except?.Count > 0)
            {
                results.AddRange(epgIds.Select(x => x.ToString()));
            }

            return results.Distinct().ToList();
        }

        private List<string> GetEpgsCBKeys(IEnumerable<long> epgIds)
        {
            var result = new List<string>();
            var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(_partnerId);
            var isNewEpgIngestEnabled = epgFeatureVersion != EpgFeatureVersion.V1;

            if (isNewEpgIngestEnabled)
            {
                // using the new EPG ingest the document id has a suffix cintaining the bulk upload that inserted it
                // so there is no way for us to now what is the document id.
                // elastisearch holds the current document in CB so we go there to take it
                result = GetEpgCBDocumentIdsByEpgId(epgIds, Enumerable.Empty<LanguageObj>());
            }
            else
            {
                result.AddRange(epgIds.Select(x => x.ToString()));
            }

            return result.Distinct().ToList();
        }



        private string GetEpgCBKey(long epgId)
        {
            var keys = GetEpgsCBKeys(new[] { epgId });
            return keys.FirstOrDefault();
        }

        #endregion

        #region Rebuilding

        public bool SetupMediaIndex(DateTime indexDate)
        {
            string newIndexName = NamingHelper.GetMediaIndexName(_partnerId, indexDate);

            #region Build new index and specify number of nodes/shards

            // prevent from size of bulk to be more than the default value of 500 (currently as of 23.06.20)
            // Default size of epg cb bulk size

            int maxResults = 100000;
            // Default size of max results should be 100,000
            if (MAX_RESULTS > 0)
            {
                maxResults = MAX_RESULTS;
            }

            var languages = GetLanguages();
            var defaultLanguage = GetDefaultLanguage();
            List<string> analyzers;
            List<string> filters;
            List<string> tokenizers;

            // get definitions of analyzers, filters and tokenizers
            GetAnalyzers(languages, out analyzers, out filters, out tokenizers);

            bool actionResult = this._elasticSearchApi.BuildIndex(newIndexName, NUM_OF_SHARDS, NUM_OF_REPLICAS, analyzers, filters, tokenizers, maxResults);

            if (!actionResult)
            {
                log.Error(string.Format("Failed creating index for index:{0}", newIndexName));
                return false;
            }

            #endregion

            #region create mapping            


            MappingAnalyzers defaultMappingAnalyzers = GetMappingAnalyzers(defaultLanguage, VERSION);

            if (!GetMetasAndTagsForMapping(
                out Dictionary<string, KeyValuePair<eESFieldType, string>> metas, out List<string> tags,
                out _))
            {
                log.Error("Failed GetMetasAndTagsForMapping as part of BuildIndex");
            }

            // Mapping for each language
            foreach (ApiObjects.LanguageObj language in languages)
            {
                string type = MEDIA;

                if (!language.IsDefault)
                {
                    type = string.Concat(MEDIA, "_", language.Code);
                }

                MappingAnalyzers specificMappingAnalyzers = GetMappingAnalyzers(language, VERSION);

                // Ask serializer to create the mapping definitions string
                string mapping = _serializer.CreateMediaMapping(metas, tags, GetMetasToPad(), specificMappingAnalyzers, defaultMappingAnalyzers);
                bool mappingResult = _elasticSearchApi.InsertMapping(newIndexName, type, mapping.ToString());

                // Most important is the mapping for the default language, we can live without the others...
                if (language.IsDefault && !mappingResult)
                {
                    actionResult = false;
                }

                if (!mappingResult)
                {
                    log.Error(string.Concat("Could not create mapping of type media for language ", language.Name));
                }

                // insert a dummy transaction child typ to allow unified search to use "commited only" query addition
                var dummyTransactionChildMapping = "{\"_parent\":{\"type\":\""+NamingHelper.EPG_V3_TRANSACTION_DOCUMENT_TYPE_NAME+"\"}}";
                mappingResult = _elasticSearchApi.InsertMapping(newIndexName, NamingHelper.EPG_V3_DUMMY_TRANSACTION_CHILD_DOCUMENT_TYPE_NAME, dummyTransactionChildMapping);
                if (!mappingResult)
                {
                    log.Error("Could not create media index mapping of type dummy transaction child doc");
                    actionResult = false;
                }
                // insert transaction type as dummy to allow unified search to use "commited only" query addition
                mappingResult = _elasticSearchApi.InsertMapping(newIndexName, NamingHelper.EPG_V3_TRANSACTION_DOCUMENT_TYPE_NAME, "{\"properties\": {}}");
                if (!mappingResult)
                {
                    log.Error("Could not create media index  mapping of type transaction doc");
                    actionResult = false;
                }
            }

            return actionResult;

            #endregion
        }

        public void InsertMedias(Dictionary<int, Dictionary<int, Media>> groupMedias, DateTime indexDate)
        {
            var indexName = NamingHelper.GetMediaIndexName(_partnerId, indexDate);
            if (groupMedias != null)
            {
                var sizeOfBulk = SIZE_OF_BULK == 0 ? SIZE_OF_BULK_DEFAULT_VALUE : SIZE_OF_BULK > SIZE_OF_BULK_DEFAULT_VALUE ? SIZE_OF_BULK_DEFAULT_VALUE : SIZE_OF_BULK;

                log.DebugFormat("Start indexing medias. total medias={0}", groupMedias.Count);
                // save current value to restore at the end
                int currentDefaultConnectionLimit = System.Net.ServicePointManager.DefaultConnectionLimit;
                try
                {
                    int numOfBulkRequests = 0;
                    var bulkRequests = new Dictionary<int, List<ESBulkRequestObj<int>>>() { { numOfBulkRequests, new List<ESBulkRequestObj<int>>() } };
                    var metasToPad = GetMetasToPad();

                    // For each media
                    foreach (var groupMedia in groupMedias)
                    {
                        var mediaId = groupMedia.Key;

                        // For each language
                        foreach (int languageId in groupMedia.Value.Keys)
                        {
                            ApiObjects.LanguageObj language = isOpc()
                                ? GetCatalogGroupCache().LanguageMapById[languageId]
                                : GetGroupManager().GetLanguage(languageId);

                            string suffix = null;

                            if (!language.IsDefault)
                            {
                                suffix = language.Code;
                            }

                            Media media = groupMedia.Value[languageId];

                            if (media != null)
                            {
                                media.PadMetas(metasToPad);
                                
                                // Serialize media and create a bulk request for it
                                string serializedMedia = _serializer.SerializeMediaObject(media, suffix);
                                string documentType = GetTranslationType(MEDIA, language);

                                // If we exceeded the size of a single bulk reuquest then create another list
                                if (bulkRequests[numOfBulkRequests].Count >= sizeOfBulk)
                                {
                                    numOfBulkRequests++;
                                    bulkRequests.Add(numOfBulkRequests, new List<ESBulkRequestObj<int>>());
                                }

                                var bulkRequest = new ESBulkRequestObj<int>(media.m_nMediaID, indexName, documentType, serializedMedia);
                                bulkRequests[numOfBulkRequests].Add(bulkRequest);
                            }
                        }
                    }

                    var maxDegreeOfParallelism = ApplicationConfiguration.Current.RecordingsMaxDegreeOfParallelism.Value;
                    if (maxDegreeOfParallelism == 0)
                    {
                        maxDegreeOfParallelism = 5;
                    }

                    var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism };
                    var LogContextData = new LogContextData();
                    ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount;

                    var failedBulkRequests = new System.Collections.Concurrent.ConcurrentBag<List<ESBulkRequestObj<int>>>();
                    // Send request to elastic search in a different thread
                    Parallel.ForEach(bulkRequests, options, (bulkRequest, state) =>
                    {
                        LogContextData.Load();
                        List<ESBulkRequestObj<int>> invalidResults;
                        bool bulkResult = _elasticSearchApi.CreateBulkRequests(bulkRequest.Value, out invalidResults);

                        // Log invalid results
                        if (!bulkResult && invalidResults != null && invalidResults.Count > 0)
                        {
                            log.Warn($"Bulk request when indexing media for partner {_partnerId} has invalid results. Will retry soon.");
                            // add entire failed retry requests to failedBulkRequests, will try again not in parallel (maybe ES is loaded)
                            failedBulkRequests.Add(invalidResults);
                        }
                    });

                    // retry on all failed bulk requests (this time not in parallel)
                    if (failedBulkRequests.Count > 0)
                    {
                        foreach (List<ESBulkRequestObj<int>> bulkRequest in failedBulkRequests)
                        {
                            List<ESBulkRequestObj<int>> invalidResults;
                            bool bulkResult = _elasticSearchApi.CreateBulkRequests(bulkRequest, out invalidResults);

                            // Log invalid results
                            if (!bulkResult && invalidResults != null && invalidResults.Count > 0)
                            {
                                foreach (var item in invalidResults)
                                {
                                    log.ErrorFormat(
                                        "Error - Could not add Media to ES index, additional retry will not be attempted. GroupID={0};Type={1};ID={2};error={3};",
                                        _partnerId, MEDIA, item.docID, item.error);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Failed during InsertMedias", ex);
                }
                finally
                {
                    System.Net.ServicePointManager.DefaultConnectionLimit = currentDefaultConnectionLimit;
                }
            }
        }

        public void PublishMediaIndex(DateTime indexDate, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            string alias = NamingHelper.GetMediaIndexAlias(_partnerId);
            bool indexExists = _elasticSearchApi.IndexExists(alias);
            var indexName = NamingHelper.GetMediaIndexName(_partnerId, indexDate);
            if (shouldSwitchIndexAlias || !indexExists)
            {
                List<string> oldIndices = _elasticSearchApi.GetAliases(alias);

                bool switchIndexResult = _elasticSearchApi.SwitchIndex(indexName, alias, oldIndices);

                if (!switchIndexResult)
                {
                    log.ErrorFormat("Failed switching index for new index name = {0}, group alias = {1}", indexName, alias);
                }

                if (shouldDeleteOldIndices && switchIndexResult && oldIndices.Count > 0)
                {
                    _elasticSearchApi.DeleteIndices(oldIndices);
                }
            }
        }

        private void GetAnalyzers(List<ApiObjects.LanguageObj> languages, out List<string> analyzers, out List<string> filters, out List<string> tokenizers)
        {
            analyzers = new List<string>();
            filters = new List<string>();
            tokenizers = new List<string>();

            if (languages != null)
            {
                foreach (ApiObjects.LanguageObj language in languages)
                {
                    string analyzer = _esIndexDefinitions.GetAnalyzerDefinition(ESUtils.GetLangCodeAnalyzerKey(language.Code, VERSION));
                    string filter = _esIndexDefinitions.GetFilterDefinition(ESUtils.GetLangCodeFilterKey(language.Code, VERSION));
                    string tokenizer = _esIndexDefinitions.GetTokenizerDefinition(ESUtils.GetLangCodeTokenizerKey(language.Code, VERSION));

                    if (string.IsNullOrEmpty(analyzer))
                    {
                        log.Error(string.Format("analyzer for language {0} doesn't exist", language.Code));
                    }
                    else
                    {
                        analyzers.Add(analyzer);
                    }

                    if (!string.IsNullOrEmpty(filter))
                    {
                        filters.Add(filter);
                    }

                    if (!string.IsNullOrEmpty(tokenizer))
                    {
                        tokenizers.Add(tokenizer);
                    }
                }

                // we always want a lowercase analyzer
                analyzers.Add(LOWERCASE_ANALYZER);

                // we always want "autocomplete" ability
                filters.Add(PHRASE_STARTS_WITH_FILTER);
                analyzers.Add(PHRASE_STARTS_WITH_ANALYZER);
                analyzers.Add(PHRASE_STARTS_WITH_SEARCH_ANALYZER);
            }
        }

        public bool AddChannelsPercolatorsToIndex(HashSet<int> channelIds, DateTime? indexDate, bool shouldCleanupInvalidChannels = false)
        {
            var channelsToRemove = new HashSet<string>();
            List<Channel> groupChannels = null;
            List<string> previousChannelIds = null;
            var indexName = indexDate.HasValue
                ? NamingHelper.GetMediaIndexName(_partnerId, indexDate.Value)
                : NamingHelper.GetMediaIndexAlias(_partnerId);
            var groupUsesTemplates = isOpc();
            if (groupUsesTemplates || channelIds != null)
            {
                log.Info(string.Format("Start indexing channel percolators. total channels={0}, doesGroupUsesTemplates={1}", channelIds.Count, groupUsesTemplates));
                List<KeyValuePair<int, string>> channelRequests = new List<KeyValuePair<int, string>>();

                try
                {
                    List<int> subGroups = new List<int>();

                    groupChannels = IndexManagerCommonHelpers.GetGroupChannels(_partnerId, _channelManager, groupUsesTemplates, ref channelIds);

                    ESMediaQueryBuilder mediaQueryParser = new ESMediaQueryBuilder()
                    {
                        QueryType = eQueryType.EXACT
                    };
                    var unifiedQueryBuilder = new ESUnifiedQueryBuilder(_esSortingService, _sortingAdapter, _queryInitializer, null, _partnerId);

                    if (shouldCleanupInvalidChannels)
                    {
                        previousChannelIds = this.GetAllChannels();
                    }

                    //allChannels = groupChannels.Select(channel => channel.m_nChannelID.ToString()).ToList();

                    foreach (Channel currentChannel in groupChannels)
                    {
                        if (currentChannel == null)
                        {
                            log.ErrorFormat("BuildChannelQueries - All channels list has null or in-active channel, continuing");
                            continue;
                        }

                        // if group uses templates - index inactive channel as well
                        if (!groupUsesTemplates && currentChannel.m_nIsActive != 1)
                        {
                            log.ErrorFormat("BuildChannelQueries - All channels list has null or in-active channel, continuing");
                            continue;
                        }

                        string channelQuery = string.Empty;

                        try
                        {
                            log.DebugFormat("BuildChannelQueries - Current channel  = {0}", currentChannel.m_nChannelID);

                            if ((currentChannel.m_nChannelTypeID == (int)ChannelType.KSQL) ||
                               (currentChannel.m_nChannelTypeID == (int)ChannelType.Manual && groupUsesTemplates && currentChannel.AssetUserRuleId > 0))
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

                        if (!string.IsNullOrEmpty(channelQuery))
                        {
                            log.DebugFormat("Adding channel to percolator - channelId = {0}", currentChannel.m_nChannelID);

                            channelRequests.Add(new KeyValuePair<int, string>(currentChannel.m_nChannelID, channelQuery));

                            if (channelRequests.Count > 50)
                            {
                                _elasticSearchApi.CreateBulkIndexRequest(indexName, ESUtils.ES_PERCOLATOR_TYPE, channelRequests);
                                channelRequests.Clear();
                            }
                        }
                        else
                        {
                            log.DebugFormat("channel with empty query will be removed from percolator - channelId = {0}", currentChannel.m_nChannelID);
                            channelsToRemove.Add(currentChannel.m_nChannelID.ToString());
                        }
                    }

                    if (channelRequests.Count > 0)
                    {
                        _elasticSearchApi.CreateBulkIndexRequest(indexName, ESUtils.ES_PERCOLATOR_TYPE, channelRequests);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Caught exception while indexing channels. Ex={0};Stack={1}", ex.Message, ex.StackTrace));
                    return false;
                }
            }

            if (shouldCleanupInvalidChannels)
            {
                if (!groupUsesTemplates && channelIds.Count == 0)
                {
                    channelIds = GetGroupManager().channelIDs;
                }

                CleanupChannelsPercolators(previousChannelIds, channelsToRemove, channelIds);
            }

            return true;
        }

        private void CleanupChannelsPercolators(List<string> previousChannelIds, HashSet<string> channelsToRemove, HashSet<int> channelIds)
        {
            LogContextData cd = new LogContextData();
            string indexName = $"{_partnerId}";

            // remove old deleted channels
            List<ESBulkRequestObj<string>> bulkList = new List<ESBulkRequestObj<string>>();
            int sizeOfBulk = 500;

            int id = 0;
            foreach (var channelId in previousChannelIds)
            {
                // channel is not in groups channel anymore / channel with empty query / channel id is not int - must be garbage
                if ((int.TryParse(channelId, out id) && !channelIds.Contains(id)) || id == 0 || channelsToRemove.Contains(channelId))
                {
                    log.DebugFormat("Removing channel from percolator - channelId = {0}", channelId);

                    bulkList.Add(new ESBulkRequestObj<string>()
                    {
                        docID = channelId,
                        index = indexName,
                        type = ESUtils.ES_PERCOLATOR_TYPE,
                        Operation = eOperation.delete
                    });

                    if (bulkList.Count >= sizeOfBulk)
                    {
                        Task t = Task.Run(() =>
                        {
                            cd.Load();
                            var invalidResults = _elasticSearchApi.CreateBulkRequest(bulkList);

                            if (invalidResults != null && invalidResults.Count > 0)
                            {
                                foreach (var item in invalidResults)
                                {
                                    log.ErrorFormat("Error - Could not delete channel from ES index. GroupID={0};ID={1};error={2};",
                                        _partnerId, item.Key, item.Value);
                                }
                            }
                        });
                        t.Wait();
                        bulkList = new List<ESBulkRequestObj<string>>();
                    }
                }
            }

            if (bulkList.Count > 0)
            {
                Task t = Task.Run(() =>
                {
                    cd.Load();
                    var invalidResults = _elasticSearchApi.CreateBulkRequest(bulkList);

                    if (invalidResults != null && invalidResults.Count > 0)
                    {
                        foreach (var item in invalidResults)
                        {
                            log.ErrorFormat("Error - Could not add channel to ES index. GroupID={0};ID={1};error={2};",
                                _partnerId, item.Key, item.Value);
                        }
                    }
                });
                t.Wait();
            }
        }

        private List<string> GetAllChannels()
        {
            List<string> currentChannelIds = new List<string>();
            // get current indexed channels
            ESMatchAllQuery matchAllQuery = new ESMatchAllQuery();
            FilteredQuery filteredQuery = new FilteredQuery()
            {
                Query = matchAllQuery
            };

            string indexName = $"{_partnerId}";

            string query = filteredQuery.ToString();
            string searchResults = _elasticSearchApi.Search(indexName, ESUtils.ES_PERCOLATOR_TYPE, ref query);
            currentChannelIds = ESUtils.GetDocumentIds(searchResults);
            return currentChannelIds;
        }

        public void AddChannelsMetadataToIndex(string newIndexName, List<Channel> allChannels)
        {
            List<ESBulkRequestObj<string>> bulkList = new List<ESBulkRequestObj<string>>();
            int sizeOfBulk = SIZE_OF_BULK;
            LogContextData cd = new LogContextData();

            // Default for size of bulk should be 50, if not stated otherwise in TCM
            if (sizeOfBulk == 0)
            {
                sizeOfBulk = 50;
            }

            // For each channel value
            foreach (var channel in allChannels)
            {
                string documentType = CHANNEL;

                // Serialize channel and create a bulk request for it
                string serializedChannel = _serializer.SerializeChannelObject(channel);

                bulkList.Add(new ESBulkRequestObj<string>()
                {
                    docID = channel.m_nChannelID.ToString(),
                    index = newIndexName,
                    type = documentType,
                    document = serializedChannel
                });

                // If we exceeded the size of a single bulk reuquest
                if (bulkList.Count >= sizeOfBulk)
                {
                    // Send request to elastic search in a different thread
                    Task t = Task.Run(() =>
                    {
                        cd.Load();

                        var invalidResults = _elasticSearchApi.CreateBulkRequest(bulkList);

                        // Log invalid results
                        if (invalidResults != null && invalidResults.Count > 0)
                        {
                            foreach (var item in invalidResults)
                            {
                                log.ErrorFormat("Error - Could not add channel to ES index. GroupID={0};Type={1};ID={2};error={3};",
                                    _partnerId, CHANNEL, item.Key, item.Value);
                            }
                        }
                    });

                    t.Wait();
                    bulkList.Clear();
                }
            }

            // If we have a final bulk pending
            if (bulkList.Count > 0)
            {
                // Send request to elastic search in a different thread
                Task t = Task.Run(() =>
                {
                    cd.Load();
                    var invalidResults = _elasticSearchApi.CreateBulkRequest(bulkList);

                    if (invalidResults != null && invalidResults.Count > 0)
                    {
                        foreach (var item in invalidResults)
                        {
                            log.ErrorFormat("Error - Could not add channel to ES index. GroupID={0};Type={1};ID={2};error={3};",
                                _partnerId, CHANNEL, item.Key, item.Value);
                        }
                    }
                });
                t.Wait();
            }
        }

        public string SetupChannelMetadataIndex(DateTime indexDate)
        {
            // Basic TCM configurations for indexing - number of shards/replicas, size of bulks 
            int numOfShards = NUM_OF_SHARDS;
            int numOfReplicas = NUM_OF_REPLICAS;
            int maxResults = MAX_RESULTS;

            // Default size of max results should be 100,000
            if (maxResults == 0)
            {
                maxResults = 100000;
            }

            #region Build Index

            List<string> analyzers;
            List<string> filters;
            List<string> tokenizers;

            var languages = new List<ApiObjects.LanguageObj>()
                {
                    GetCatalogGroupCache().GetDefaultLanguage()
                };

            GetTagsAndChannelsAnalyzers(languages, out analyzers, out filters, out tokenizers);

            string newIndexName = NamingHelper.GetNewChannelMetadataIndexName(_partnerId, indexDate);
            bool actionResult = _elasticSearchApi.BuildIndex(newIndexName, numOfShards, numOfReplicas,
                analyzers, filters, tokenizers, maxResults);
            if (!actionResult)
            {
                log.Error(string.Format("Failed creating index for index:{0}", newIndexName));
                return string.Empty;
            }

            #endregion

            #region Mapping
            // Mapping for each language
            foreach (ApiObjects.LanguageObj language in languages)
            {
                string indexAnalyzer, searchAnalyzer;
                string autocompleteIndexAnalyzer = null;
                string autocompleteSearchAnalyzer = null;
                string lowercaseAnalyzer = null;

                // create names for analyzers to be used in the mapping later on
                string analyzerDefinitionName = ESUtils.GetLangCodeAnalyzerKey(language.Code, VERSION);

                if (_esIndexDefinitions.AnalyzerExists(analyzerDefinitionName))
                {
                    indexAnalyzer = $"{language.Code}_index_analyzer";
                    searchAnalyzer = $"{language.Code}_search_analyzer";

                    string analyzerDefinition = _esIndexDefinitions.GetAnalyzerDefinition(analyzerDefinitionName);

                    var autocompleteIndexAnalyzerCandidate = $"{language.Code}_autocomplete_analyzer";
                    if (analyzerDefinition.Contains(autocompleteIndexAnalyzerCandidate))
                    {
                        autocompleteIndexAnalyzer = autocompleteIndexAnalyzerCandidate;
                    }

                    var autocompleteSearchAnalyzerCandidate = $"{language.Code}_autocomplete_search_analyzer";
                    if (analyzerDefinition.Contains(autocompleteSearchAnalyzerCandidate))
                    {
                        autocompleteSearchAnalyzer = autocompleteSearchAnalyzerCandidate;
                    }

                    var lowercaseAnalyzerCandidate = $"{language.Code}_lowercase_analyzer";
                    if (analyzerDefinition.Contains(lowercaseAnalyzerCandidate))
                    {
                        lowercaseAnalyzer = lowercaseAnalyzerCandidate;
                    }
                }
                else
                {
                    indexAnalyzer = "whitespace";
                    searchAnalyzer = "whitespace";
                    log.Error(string.Format("could not find analyzer for language ({0}) for mapping. whitespace analyzer will be used instead", language.Code));
                }

                string type = CHANNEL;
                string suffix = null;

                // Ask serializer to create the mapping definitions string
                string mapping = _serializer.CreateChannelMapping(indexAnalyzer, searchAnalyzer, autocompleteIndexAnalyzer, autocompleteSearchAnalyzer, lowercaseAnalyzer, suffix);

                bool mappingResult = _elasticSearchApi.InsertMapping(newIndexName, type, mapping.ToString());

                // Most important is the mapping for the default language, we can live without the others...
                if (language.IsDefault && !mappingResult)
                {
                    actionResult = false;
                }

                if (!mappingResult)
                {
                    log.Error(string.Concat("Could not create mapping of type channel for language ", language.Name));
                }
            }

            #endregion

            return newIndexName;
        }

        public void PublishChannelsMetadataIndex(string newIndexName, bool shouldSwitchAlias, bool shouldDeleteOldIndices)
        {
            string alias = NamingHelper.GetChannelMetadataIndexName(_partnerId);
            bool indexExists = _elasticSearchApi.IndexExists(alias);

            if (shouldSwitchAlias || !indexExists)
            {
                List<string> oldIndices = _elasticSearchApi.GetAliases(alias);

                var switchIndexResult = _elasticSearchApi.SwitchIndex(newIndexName, alias, oldIndices);

                if (!switchIndexResult)
                {
                    log.ErrorFormat("Failed switching index for new index name = {0}, group alias = {1}", newIndexName, alias);
                }

                if (shouldDeleteOldIndices && switchIndexResult && oldIndices.Count > 0)
                {
                    _elasticSearchApi.DeleteIndices(oldIndices);
                }
            }
        }

        public string SetupTagsIndex(DateTime indexDate)
        {
            #region Build Index

            List<string> analyzers;
            List<string> filters;
            List<string> tokenizers;

            var catalogGroupCache = GetCatalogGroupCache();
            GetTagsAndChannelsAnalyzers(catalogGroupCache.LanguageMapById.Values.ToList(), out analyzers, out filters, out tokenizers);

            string newIndexName = NamingHelper.GetNewMetadataIndexName(_partnerId, indexDate);

            // Basic TCM configurations for indexing - number of shards/replicas, size of bulks 
            int numOfShards = NUM_OF_SHARDS;
            int numOfReplicas = NUM_OF_REPLICAS;
            int maxResults = MAX_RESULTS;

            // Default size of max results should be 100,000
            if (maxResults == 0)
            {
                maxResults = 100000;
            }

            bool actionResult = _elasticSearchApi.BuildIndex(newIndexName, numOfShards, numOfReplicas,
                analyzers, filters, tokenizers, maxResults);

            if (!actionResult)
            {
                log.Error(string.Format("Failed creating index for index:{0}", newIndexName));
                return string.Empty;
            }

            #endregion

            var languages = catalogGroupCache.LanguageMapById.Values;

            #region Mapping
            // Mapping for each language
            foreach (ApiObjects.LanguageObj language in languages)
            {
                string indexAnalyzer, searchAnalyzer;
                string autocompleteIndexAnalyzer = null;
                string autocompleteSearchAnalyzer = null;
                string lowercaseAnalyzer = null;

                // create names for analyzers to be used in the mapping later on
                string analyzerDefinitionName = ESUtils.GetLangCodeAnalyzerKey(language.Code, VERSION);

                if (_esIndexDefinitions.AnalyzerExists(analyzerDefinitionName))
                {
                    indexAnalyzer = $"{language.Code}_index_analyzer";
                    searchAnalyzer =$"{language.Code}_search_analyzer";

                    string analyzerDefinition = _esIndexDefinitions.GetAnalyzerDefinition(analyzerDefinitionName);

                    var autocompleteIndexAnalyzerCandidate = $"{language.Code}_autocomplete_analyzer";
                    if (analyzerDefinition.Contains(autocompleteIndexAnalyzerCandidate))
                    {
                        autocompleteIndexAnalyzer = autocompleteIndexAnalyzerCandidate;
                    }

                    var autocompleteSearchAnalyzerCandidate = $"{language.Code}_autocomplete_search_analyzer";
                    if (analyzerDefinition.Contains(autocompleteSearchAnalyzerCandidate))
                    {
                        autocompleteSearchAnalyzer = autocompleteSearchAnalyzerCandidate;
                    }

                    var lowercaseAnalyzerCandidate = $"{language.Code}_lowercase_analyzer";
                    if (analyzerDefinition.Contains(lowercaseAnalyzerCandidate))
                    {
                        lowercaseAnalyzer = lowercaseAnalyzerCandidate;
                    }
                }
                else
                {
                    indexAnalyzer = "whitespace";
                    searchAnalyzer = "whitespace";
                    log.Error(string.Format("could not find analyzer for language ({0}) for mapping. whitespace analyzer will be used instead", language.Code));
                }

                string type = TAG_INDEX_TYPE;
                string suffix = null;

                if (!language.IsDefault)
                {
                    type = string.Concat(TAG_INDEX_TYPE, "_", language.Code);
                    suffix = language.Code;
                }

                // Ask serializer to create the mapping definitions string
                string mapping = _serializer.CreateMetadataMapping(indexAnalyzer, searchAnalyzer, autocompleteIndexAnalyzer, autocompleteSearchAnalyzer, lowercaseAnalyzer, suffix);

                bool mappingResult = _elasticSearchApi.InsertMapping(newIndexName, type, mapping.ToString());

                // Most important is the mapping for the default language, we can live without the others...
                if (language.IsDefault && !mappingResult)
                {
                    actionResult = false;
                }

                if (!mappingResult)
                {
                    log.Error(string.Concat("Could not create mapping of type tag for language ", language.Name));
                }
            }

            return newIndexName;

            #endregion
        }

        public void InsertTagsToIndex(string newIndexName, List<ApiObjects.SearchObjects.TagValue> allTagValues)
        {
            LogContextData cd = new LogContextData();
            int sizeOfBulk = TVinciShared.WS_Utils.GetTcmIntValue("ES_BULK_SIZE");

            // Default for size of bulk should be 50, if not stated otherwise in TCM
            if (sizeOfBulk == 0)
            {
                sizeOfBulk = 50;
            }

            List<ESBulkRequestObj<string>> bulkList = new List<ESBulkRequestObj<string>>();

            // For each tag value
            var catalogGroupCache = GetCatalogGroupCache();

            foreach (var tagValue in allTagValues)
            {
                if (!catalogGroupCache.LanguageMapById.ContainsKey(tagValue.languageId))
                {
                    log.WarnFormat("Found tag value with non existing language ID. tagId = {0}, tagText = {1}, languageId = {2}",
                        tagValue.tagId, tagValue.value, tagValue.languageId);

                    continue;
                }

                var language = catalogGroupCache.LanguageMapById[tagValue.languageId];
                string suffix = null;

                if (!language.IsDefault)
                {
                    suffix = language.Code;
                }

                string documentType = GetTranslationType(TAG_INDEX_TYPE, language);

                // Serialize tag and create a bulk request for it
                string serializedTag = _serializer.SerializeTagValueObject(tagValue, language);

                bulkList.Add(new ESBulkRequestObj<string>()
                {
                    docID = string.Format("{0}_{1}", tagValue.tagId, tagValue.languageId),
                    index = newIndexName,
                    type = documentType,
                    document = serializedTag
                });

                // If we exceeded the size of a single bulk reuquest
                if (bulkList.Count >= sizeOfBulk)
                {
                    // Send request to elastic search in a different thread
                    Task t = Task.Run(() =>
                    {
                        cd.Load();

                        var invalidResults = _elasticSearchApi.CreateBulkRequest(bulkList);

                        // Log invalid results
                        if (invalidResults != null && invalidResults.Count > 0)
                        {
                            foreach (var item in invalidResults)
                            {
                                log.ErrorFormat("Error - Could not add tag to ES index. GroupID={0};Type={1};ID={2};error={3};",
                                    _partnerId, TAG_INDEX_TYPE, item.Key, item.Value);
                            }
                        }
                    });

                    t.Wait();
                    bulkList.Clear();
                }
            }

            // If we have a final bulk pending
            if (bulkList.Count > 0)
            {
                // Send request to elastic search in a different thread
                Task t = Task.Run(() =>
                {
                    cd.Load();
                    var invalidResults = _elasticSearchApi.CreateBulkRequest(bulkList);

                    if (invalidResults != null && invalidResults.Count > 0)
                    {
                        foreach (var item in invalidResults)
                        {
                            log.ErrorFormat("Error - Could not add tag to ES index. GroupID={0};Type={1};ID={2};error={3};",
                                _partnerId, TAG_INDEX_TYPE, item.Key, item.Value);
                        }
                    }
                });
                t.Wait();
            }
        }

        public bool PublishTagsIndex(string newIndexName, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            bool result = true;

            #region Switch index alias + Delete old indices handling

            string alias = NamingHelper.GetMetadataIndexAlias(_partnerId);
            bool indexExists = _elasticSearchApi.IndexExists(alias);

            if (shouldSwitchIndexAlias || !indexExists)
            {
                List<string> oldIndices = _elasticSearchApi.GetAliases(alias);

                var switchIndexResult = _elasticSearchApi.SwitchIndex(newIndexName, alias, oldIndices);
                if (!switchIndexResult)
                {
                    log.ErrorFormat("Failed switching index for new index name = {0}, group alias = {1}", newIndexName, alias);
                    result = false;
                }

                if (shouldDeleteOldIndices && switchIndexResult && oldIndices.Count > 0)
                {
                    _elasticSearchApi.DeleteIndices(oldIndices);
                }
            }

            #endregion

            return result;
        }

        public string SetupEpgIndex(DateTime indexDate, bool isRecording)
        {
            var indexName = NamingHelper.GetNewEpgIndexName(_partnerId, indexDate);

            if (isRecording)
            {
                indexName = NamingHelper.GetNewRecordingIndexName(_partnerId, indexDate);
            }

            CreateNewEpgIndex(indexName, isRecording);
            return indexName;
        }

        public void AddEPGsToIndex(string index, bool isRecording,
            Dictionary<ulong, Dictionary<string, EpgCB>> programs,
            Dictionary<long, List<int>> linearChannelsRegionsMapping,
            Dictionary<long, long> epgToRecordingMapping)
        {
            string type = ES_EPG_TYPE;

            if (isRecording)
            {
                type = RECORDING_INDEX_TYPE;
            }

            // Basic validation
            if (programs == null || programs.Count == 0)
            {
                log.ErrorFormat("AddEPGsToIndex {0}/{1} for group {2}: programs is null or empty!", index, type, _partnerId);
                return;
            }

            // save current value to restore at the end
            int currentDefaultConnectionLimit = System.Net.ServicePointManager.DefaultConnectionLimit;
            try
            {
                int numOfBulkRequests = 0;
                Dictionary<int, List<ESBulkRequestObj<ulong>>> bulkRequests =
                    new Dictionary<int, List<ESBulkRequestObj<ulong>>>() { { numOfBulkRequests, new List<ESBulkRequestObj<ulong>>() } };

                // GetLinear Channel Values 
                var programsList = new List<EpgCB>();

                foreach (Dictionary<string, EpgCB> programsValues in programs.Values)
                {
                    programsList.AddRange(programsValues.Values);
                }

                _catalogManager.GetLinearChannelValues(programsList, _partnerId, _ => { });

                // used only to support linear media id search on elastic search
                List<string> epgChannelIds = programsList.Select(item => item.ChannelID.ToString()).ToList<string>();

                var linearChannelSettings = _catalogCache.GetLinearChannelSettings(_partnerId, epgChannelIds);

                // prevent from size of bulk to be more than the default value of 500 (currently as of 23.06.20)
                int sizeOfBulk = SIZE_OF_BULK == 0 ? SIZE_OF_BULK_DEFAULT_VALUE : SIZE_OF_BULK > SIZE_OF_BULK_DEFAULT_VALUE ? SIZE_OF_BULK_DEFAULT_VALUE : SIZE_OF_BULK;

                // Run on all programs
                CatalogGroupCache catalogGroupCache = null;
                Group groupManager = null;
                var groupUsesTemplates = isOpc();
                if (groupUsesTemplates)
                {
                    catalogGroupCache = GetCatalogGroupCache();
                }
                else
                {
                    groupManager = GetGroupManager();
                }
                var metasToPad = GetMetasToPad();

                foreach (ulong epgID in programs.Keys)
                {
                    foreach (string languageCode in programs[epgID].Keys)
                    {
                        string suffix = null;

                        LanguageObj language = null;

                        if (!string.IsNullOrEmpty(languageCode))
                        {

                            if (groupUsesTemplates)
                            {

                                language = catalogGroupCache.LanguageMapByCode.ContainsKey(languageCode) ? catalogGroupCache.LanguageMapByCode[languageCode] : null;
                            }
                            else
                            {
                                language = groupManager.GetLanguage(languageCode);
                            }

                            // Validate language
                            if (language == null)
                            {
                                log.ErrorFormat("AddEPGsToIndex: Epg {0} has invalid language code {1}", epgID, languageCode);
                                continue;
                            }

                            if (!language.IsDefault)
                            {
                                suffix = language.Code;
                            }
                        }
                        else
                        {
                            language = groupUsesTemplates ? catalogGroupCache.GetDefaultLanguage() : groupManager.GetGroupDefaultLanguage();
                        }

                        EpgCB epg = programs[epgID][languageCode];

                        if (epg != null)
                        {
                            epg.PadMetas(metasToPad);

                            // used only to currently support linear media id search on elastic search
                            if (linearChannelSettings.ContainsKey(epg.ChannelID.ToString()))
                            {
                                epg.LinearMediaId = linearChannelSettings[epg.ChannelID.ToString()].LinearMediaId;
                            }

                            if (epg.LinearMediaId > 0 && linearChannelsRegionsMapping != null && linearChannelsRegionsMapping.ContainsKey(epg.LinearMediaId))
                            {
                                epg.regions = linearChannelsRegionsMapping[epg.LinearMediaId];
                            }

                            // Serialize EPG object to string
                            string serializedEpg = SerializeEPGObject(epg, isRecording, epgToRecordingMapping, suffix, groupUsesTemplates);
                            string epgType = GetTranslationType(type, language);
                            ulong documentId = GetDocumentId(epg, isRecording, epgToRecordingMapping);


                            var ttl = string.Empty;
                            var shouldSetTTL = !isRecording;
                            if (shouldSetTTL)
                            {
                                var totalMinutes = _ttlService.GetEpgTtlMinutes(epg);
                                ttl = $"{totalMinutes}m";
                            }

                            // If we exceeded the size of a single bulk reuquest then create another list
                            if (bulkRequests[numOfBulkRequests].Count >= sizeOfBulk)
                            {
                                numOfBulkRequests++;
                                bulkRequests.Add(numOfBulkRequests, new List<ESBulkRequestObj<ulong>>());
                            }

                            ESBulkRequestObj<ulong> bulkRequest =
                                new ESBulkRequestObj<ulong>(documentId, index, epgType, serializedEpg, eOperation.index, epg.StartDate.ToUniversalTime().ToString("yyyyMMdd"), ttl);
                            bulkRequests[numOfBulkRequests].Add(bulkRequest);
                        }
                    }
                }

                int maxDegreeOfParallelism = ApplicationConfiguration.Current.RecordingsMaxDegreeOfParallelism.Value;
                if (maxDegreeOfParallelism == 0)
                {
                    maxDegreeOfParallelism = 5;
                }

                ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism };
                LogContextData contextData = new LogContextData();
                System.Net.ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount;
                System.Collections.Concurrent.ConcurrentBag<List<ESBulkRequestObj<ulong>>> failedBulkRequests = new System.Collections.Concurrent.ConcurrentBag<List<ESBulkRequestObj<ulong>>>();
                // Send request to elastic search in a different thread
                Parallel.ForEach(bulkRequests, options, (bulkRequest, state) =>
                {
                    contextData.Load();
                    List<ESBulkRequestObj<ulong>> invalidResults;
                    bool bulkResult = _elasticSearchApi.CreateBulkRequests(bulkRequest.Value, out invalidResults);

                    // Log invalid results
                    if (!bulkResult && invalidResults != null && invalidResults.Count > 0)
                    {
                        log.Warn($"Bulk request when indexing epg for partner {_partnerId} has invalid results. Will retry soon.");

                        // add entire failed retry requests to failedBulkRequests, will try again not in parallel (maybe ES is loaded)
                        failedBulkRequests.Add(invalidResults);
                    }
                });

                // retry on all failed bulk requests (this time not in parallel)
                if (failedBulkRequests.Count > 0)
                {
                    foreach (List<ESBulkRequestObj<ulong>> bulkRequest in failedBulkRequests)
                    {
                        List<ESBulkRequestObj<ulong>> invalidResults;
                        bool bulkResult = _elasticSearchApi.CreateBulkRequests(bulkRequest, out invalidResults);

                        // Log invalid results
                        if (!bulkResult && invalidResults != null && invalidResults.Count > 0)
                        {
                            foreach (var item in invalidResults)
                            {
                                log.ErrorFormat("Error - Could not add EPG to ES index, additional retry will not be attempted. GroupID={0};Type={1};EPG_ID={2};error={3};",
                                    _partnerId, IndexManagerV2.EPG_INDEX_TYPE, item.docID, item.error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed during AddEPGsToIndex", ex);
            }
            finally
            {
                System.Net.ServicePointManager.DefaultConnectionLimit = currentDefaultConnectionLimit;
            }
        }


        public bool PublishEpgIndex(string newIndexName, bool isRecording, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            string alias = NamingHelper.GetEpgIndexAlias(_partnerId);

            if (isRecording)
            {
                alias = NamingHelper.GetRecordingIndexAlias(_partnerId);
            }

            bool indexExists = _elasticSearchApi.IndexExists(alias);

            if (shouldSwitchIndexAlias || !indexExists)
            {
                List<string> oldIndices = _elasticSearchApi.GetAliases(alias);

                bool switchSuccess = _elasticSearchApi.SwitchIndex(newIndexName, alias, oldIndices, null);

                if (!switchSuccess)
                {
                    log.ErrorFormat("Failed switching index for new index name = {0}, group alias = {1}", newIndexName, alias);
                    return false;
                }

                if (shouldDeleteOldIndices && oldIndices.Count > 0)
                {
                    _elasticSearchApi.DeleteIndices(oldIndices);
                }
            }

            return true;
        }

        protected virtual ulong GetDocumentId(ulong epgId, bool isRecording, Dictionary<long, long> epgToRecordingMapping)
        {
            if (!isRecording)
            {
                return epgId;
            }
            return (ulong)(epgToRecordingMapping[(int)epgId]);
        }

        protected virtual ulong GetDocumentId(EpgCB epg, bool isRecording, Dictionary<long, long> epgToRecordingMapping)
        {
            if (isRecording)
                return (ulong)epgToRecordingMapping[(long)epg.EpgID];
            return epg.EpgID;
        }

        protected virtual string SerializeEPGObject(EpgCB epg, bool isRecording, Dictionary<long, long> epgToRecordingMapping, string suffix = null, bool doesGroupUsesTemplates = false)
        {
            if (!isRecording)
            {
                return _serializer.SerializeEpgObject(epg, suffix, doesGroupUsesTemplates);
            }
            else
            {
                long recordingId = (long)(epgToRecordingMapping[(int)epg.EpgID]);

                return _serializer.SerializeRecordingObject(epg, recordingId, suffix, doesGroupUsesTemplates);
            }
        }

        protected virtual string SerializeEPGObject(EpgEs epg, string suffix = null)
        {
            return _serializer.SerializeEpgObject(epg, suffix);
        }

        #endregion

        #region 

        public bool UpdateEpgs(List<EpgCB> epgObjects,
            bool isRecording,
            Dictionary<long, long> epgToRecordingMapping = null)
        {
            bool result = false;

            List<ESBulkRequestObj<ulong>> bulkRequests = new List<ESBulkRequestObj<ulong>>();
            List<KeyValuePair<string, string>> invalidResults = null;

            string type = ES_EPG_TYPE;

            if (isRecording)
            {
                type = RECORDING_INDEX_TYPE;
            }


            int sizeOfBulk = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.BulkSize.Value;

            if (sizeOfBulk == 0)
            {
                sizeOfBulk = 500;
            }

            // Temporarily - assume success
            bool temporaryResult = true;

            CatalogGroupCache catalogGroupCache = null;
            Group groupManager = null;
            List<LanguageObj> languages = null;
            var groupUsesTemplates = isOpc();
            if (groupUsesTemplates)
            {
                catalogGroupCache = GetCatalogGroupCache();
                languages = catalogGroupCache.LanguageMapById.Values.ToList();
            }
            else
            {
                groupManager = GetGroupManager();
                languages = groupManager.GetLangauges();
            }

            var linearChannelsRegionsMapping = _regionManager.GetLinearMediaRegions(_partnerId);

            var alias = NamingHelper.GetEpgIndexAlias(_partnerId);

            if (isRecording)
            {
                alias = NamingHelper.GetRecordingIndexAlias(_partnerId);
            }

            if (!_elasticSearchApi.IndexExists(alias))
            {
                log.Error($"Error - Index of type {type} for group {_partnerId} does not exist");
                return result;
            }


            _catalogManager.GetLinearChannelValues(epgObjects, _partnerId, _ => { });
            var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(_partnerId);
            if (epgFeatureVersion == EpgFeatureVersion.V3 && !isRecording)
            {
                UpsertProgramsEpgV3(epgObjects);
                return result;
            }

            List<string> epgChannelIds = epgObjects.Select(item => item.ChannelID.ToString()).ToList();
            Dictionary<string, LinearChannelSettings> linearChannelSettings = _catalogCache.GetLinearChannelSettings(_partnerId, epgChannelIds);

            // Create dictionary by languages
            var metasToPad = GetMetasToPad();
            var suppressesIndexes = Api.api.GetMediaSuppressedIndexes(_partnerId)?.Object;

            foreach (LanguageObj language in languages)
            {
                // Filter programs to current language
                List<EpgCB> currentLanguageEpgs = epgObjects.Where(epg =>
                    epg.Language.ToLower() == language.Code.ToLower() || (language.IsDefault && string.IsNullOrEmpty(epg.Language))).ToList();

                if (currentLanguageEpgs != null && currentLanguageEpgs.Count > 0)
                {
                    // Create bulk request object for each program
                    foreach (EpgCB epg in currentLanguageEpgs)
                    {
                        // Epg V2 has multiple indices connected to the gloabl alias {groupID}_epg
                        // in that case we need to use the specific date alias for each epg item to update
                        if (!isRecording && epgFeatureVersion == EpgFeatureVersion.V2)
                        {
                            alias = _namingHelper.GetDailyEpgIndexName(_partnerId, epg.StartDate.Date);
                        }

                        epg.PadMetas(metasToPad);

                        string suffix = null;

                        if (!language.IsDefault)
                        {
                            suffix = language.Code;
                        }

                        // used only to support linear media id search on elastic search
                        if (linearChannelSettings.ContainsKey(epg.ChannelID.ToString()))
                        {
                            epg.LinearMediaId = linearChannelSettings[epg.ChannelID.ToString()].LinearMediaId;
                        }

                        if (epg.LinearMediaId > 0 && linearChannelsRegionsMapping != null && linearChannelsRegionsMapping.ContainsKey(epg.LinearMediaId))
                        {
                            epg.regions = linearChannelsRegionsMapping[epg.LinearMediaId];
                        }

                        string serializedEpg = SerializeEPGObject(epg, isRecording, epgToRecordingMapping, suffix,
                            groupUsesTemplates);

                        var ttl = string.Empty;
                        var shouldSetTTL = !isRecording;

                        if (shouldSetTTL)
                        {
                            var totalMinutes = _ttlService.GetEpgTtlMinutes(epg);
                            ttl = $"{totalMinutes}m";
                        }

                        bulkRequests.Add(new ESBulkRequestObj<ulong>()
                        {
                            docID = GetDocumentId(epg, isRecording, epgToRecordingMapping),
                            index = alias,
                            type = GetTranslationType(type, language),
                            Operation = eOperation.index,
                            document = serializedEpg,
                            routing = epg.StartDate.ToUniversalTime().ToString("yyyyMMdd"),
                            ttl = ttl
                        });

                        if (bulkRequests.Count > sizeOfBulk)
                        {
                            // send request to ES API
                            invalidResults = _elasticSearchApi.CreateBulkRequest(bulkRequests);

                            if (invalidResults != null && invalidResults.Count > 0)
                            {
                                foreach (var invalidResult in invalidResults)
                                {
                                    log.Error("Error - " + string.Format(
                                        "Could not update EPG in ES. GroupID={0};Type={1};EPG_ID={2};error={3};",
                                        _partnerId, ES_EPG_TYPE, invalidResult.Key, invalidResult.Value));
                                }

                                result = false;
                                temporaryResult = false;
                            }
                            else
                            {
                                temporaryResult &= true;
                            }

                            bulkRequests.Clear();
                        }
                    }
                }
            }

            if (bulkRequests.Count > 0)
            {
                // send request to ES API
                invalidResults = _elasticSearchApi.CreateBulkRequest(bulkRequests);

                if (invalidResults != null && invalidResults.Count > 0)
                {
                    foreach (var invalidResult in invalidResults)
                    {
                        log.Error("Error - " + string.Format(
                            "Could not update EPG in ES. GroupID={0};Type={1};EPG_ID={2};error={3};",
                            _partnerId, ES_EPG_TYPE, invalidResult.Key, invalidResult.Value));
                    }

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

            var bulkRequests = new List<ESBulkRequestObj<string>>();
            List<KeyValuePair<string, string>> invalidResults = null;

            string type = ES_EPG_TYPE;

            int sizeOfBulk = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.BulkSize.Value;
            if (sizeOfBulk == 0)
            {
                sizeOfBulk = 500;
            }

            // Temporarily - assume success
            bool temporaryResult = true;
            List<LanguageObj> languages = isOpc() ? GetCatalogGroupCache().LanguageMapById.Values.ToList() : GetGroupManager().GetLangauges();

            // Epg V2 has multiple indices connected to the gloabl alias {groupID}_epg
            // in that case we need to use the specific date alias for each epg item to update
            var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(_partnerId);

            var alias = NamingHelper.GetEpgIndexAlias(_partnerId);
            if (!_elasticSearchApi.IndexExists(alias))
            {
                log.Error($"Error - Index of type {type} for group {_partnerId} does not exist");
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

                        string suffix = null;
                        if (!language.IsDefault)
                        {
                            suffix = language.Code;
                        }

                        string serializedEpg = SerializeEPGObject(new EpgEs(epg.EpgPartial), suffix);

                        bulkRequests.Add(new ESBulkRequestObj<string>()
                        {
                            docID = epgFeatureVersion == EpgFeatureVersion.V3 ? epg.DocumentId : epg.EpgId.ToString(),
                            index = alias,
                            type = GetTranslationType(type, language),
                            Operation = eOperation.update,
                            document = serializedEpg,
                            routing = epg.StartDate.ToUniversalTime().ToString("yyyyMMdd")
                        });

                        if (bulkRequests.Count > sizeOfBulk)
                        {
                            // send request to ES API
                            invalidResults = _elasticSearchApi.CreateBulkRequest(bulkRequests);

                            if (invalidResults != null && invalidResults.Count > 0)
                            {
                                foreach (var invalidResult in invalidResults)
                                {
                                    log.Error("Error - " + string.Format(
                                        "Could not update EPG in ES. GroupID={0};Type={1};EPG_ID={2};error={3};",
                                        _partnerId, ES_EPG_TYPE, invalidResult.Key, invalidResult.Value));
                                }

                                result = false;
                                temporaryResult = false;
                            }
                            else
                            {
                                temporaryResult &= true;
                            }

                            bulkRequests.Clear();
                        }
                    }
                }
            }

            if (bulkRequests.Count > 0)
            {
                // send request to ES API
                invalidResults = _elasticSearchApi.CreateBulkRequest(bulkRequests);

                if (invalidResults != null && invalidResults.Count > 0)
                {
                    foreach (var invalidResult in invalidResults)
                    {
                        log.Error("Error - " + string.Format(
                            "Could not update EPG in ES. GroupID={0};Type={1};EPG_ID={2};error={3};",
                            _partnerId, ES_EPG_TYPE, invalidResult.Key, invalidResult.Value));
                    }

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

        public void UpsertPrograms(IList<EpgProgramBulkUploadObject> calculatedPrograms, string draftIndexName, LanguageObj defaultLanguage, IDictionary<string, LanguageObj> languages)
        {
            var bulkSize = GetBulkSizeForUpsertPrograms();
            var policy = GetRetryPolicyForUpsertPrograms();
            var metasToPad = GetMetasToPad();
            var linearChannelsRegionsMapping = GetLinearChannelsMapping();

            policy.Execute(() =>
            {
                var bulkRequests = new List<ESBulkRequestObj<string>>();
                try
                {
                    var programTranslationsToIndex = calculatedPrograms.SelectMany(p => p.EpgCbObjects);
                    foreach (var program in programTranslationsToIndex)
                    {
                        var bulkRequest = MapEpgCBToEsBulkRequest(program, draftIndexName, languages, defaultLanguage, metasToPad, linearChannelsRegionsMapping);
                        bulkRequests.Add(bulkRequest);

                        // If we exceeded maximum size of bulk
                        if (bulkRequests.Count >= bulkSize)
                        {
                            ExecuteAndValidateBulkRequests(bulkRequests);
                            bulkRequests.Clear();
                        }
                    }

                    // If we have anything left that is less than the size of the bulk
                    if (bulkRequests.Count > 0)
                    {
                        ExecuteAndValidateBulkRequests(bulkRequests);
                        bulkRequests.Clear();
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

        private ESBulkRequestObj<string> MapEpgCBToEsBulkRequest(EpgCB program, string indexName, IDictionary<string, LanguageObj> languages, LanguageObj defaultLanguage, HashSet<string> metasToPad, IReadOnlyDictionary<long, List<int>> linearChannelsRegionsMapping)
        {
            program.PadMetas(metasToPad);
            var suffix = program.Language == defaultLanguage.Code ? "" : program.Language;
            var language = languages[program.Language];

            // We don't store regions in CB that's why we need to calculate regions before insertion to ES on every program update during ingest.
            if (program.LinearMediaId > 0 && linearChannelsRegionsMapping.TryGetValue(program.LinearMediaId, out var regions))
            {
                program.regions = regions;
            }

            // Serialize EPG object to string
            string serializedEpg = TryGetSerializedEpg(isOpc(), program, suffix);
            var epgType = GetTranslationType(IndexManagerV2.EPG_INDEX_TYPE, language);

            var totalMinutes = _ttlService.GetEpgTtlMinutes(program);
            totalMinutes = totalMinutes < 0 ? 10 : totalMinutes;

            var bulkRequest = new ESBulkRequestObj<string>()
            {
                docID = program.EpgID.ToString(),
                document = serializedEpg,
                index = indexName,
                Operation = eOperation.index,
                routing = program.StartDate.Date.ToString("yyyyMMdd") /*program.StartDate.ToUniversalTime().ToString("yyyyMMdd")*/,
                type = epgType,
                ttl = $"{totalMinutes}m"
            };
            return bulkRequest;
        }

        private static int GetBulkSizeForUpsertPrograms()
        {
            int bulkSize = SIZE_OF_BULK;
            bulkSize = bulkSize == 0 ? SIZE_OF_BULK_DEFAULT_VALUE :
                bulkSize > SIZE_OF_BULK_DEFAULT_VALUE ? SIZE_OF_BULK_DEFAULT_VALUE : bulkSize;
            return bulkSize;
        }

        private static RetryPolicy GetRetryPolicyForUpsertPrograms()
        {
            var retryCount = 5;
            var policy = RetryPolicy.Handle<Exception>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time, attempt, ctx) =>
                {
                    log.Warn($"upsert attemp [{attempt}/{retryCount}] Failed, waiting for:[{time.TotalSeconds}] seconds.", ex);
                });
            return policy;
        }

        private string TryGetSerializedEpg(bool isOpc, EpgCB program, string suffix, eTransactionOperation? transactionOperation = null)
        {
            try
            {
                var documentTransactionalStatus = transactionOperation.HasValue ? transactionOperation.ToString() : null;
                return _serializer.SerializeEpgObject(program, suffix, isOpc, documentTransactionalStatus);
            }
            catch (Exception e)
            {
                string msg = "";
                if (program != null && program.Crid != null)
                {
                    msg += $" program_crid: {program.Crid} ";
                    msg += $" program_epg_id: {program.EpgID} ";
                }

                if (e.InnerException != null && e.InnerException.Message != null)
                {
                    msg += $" InnerException: {e.InnerException.Message} ";
                }

                log.Debug($"Error while calling SerializeEpgObject {msg} {e.Message}", e);
                throw;
            }
        }

        public void DeletePrograms(IList<EpgProgramBulkUploadObject> programsToDelete, string epgIndexName,
            IDictionary<string, LanguageObj> languages)
        {
            if (!programsToDelete.Any())
                return;

            var programIds = programsToDelete.Select(program => program.EpgId);
            var channelIds = programsToDelete.Select(x => x.ChannelId).Distinct().ToList();
            var externalIds = programsToDelete.Select(program => program.EpgExternalId).Distinct().ToList();

            // We will retry deletion until the sum of all deleted programs is equal to the total docs deleted, this is becasue
            // there is an issue in elastic 2.3 where we cannot be sure it will find the item to delete
            // right after re-index.
            var totalDocumentsDeleted = 0;
            log.Debug($"Update elasticsearch index completed, deleting required documents. documents.length:[{programsToDelete.Count}]");
            if (programIds.Any())
            {
                var retryCount = 5;
                var policy = RetryPolicy.Handle<Exception>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time, attempt, ctx) =>
                {
                    log.Warn($"delete attemp [{attempt}/{retryCount}] Failed, waiting for:[{time.TotalSeconds}] seconds.", ex);
                });

                policy.Execute(() =>
                {
                    var deleteQuery = GetElasticsearchQueryForEpgIDs(programIds, externalIds ?? new List<string>(), channelIds);
                    _elasticSearchApi.DeleteDocsByQuery(epgIndexName, string.Empty, ref deleteQuery, out var deletedDocsCount);
                    totalDocumentsDeleted += deletedDocsCount;
                });
            }
        }

        #endregion

        #region Private Methods

        private bool Reindex(string sourceIndex, string targetIndex, int retryCount = 1)
        {
            var policy = IndexManagerCommonHelpers.GetRetryPolicy<Exception>(retryCount);
            var retryResult = policy.ExecuteAndCapture(() =>
            {
                var res = _elasticSearchApi.Reindex(sourceIndex, targetIndex);
                if (!res)
                {
                    throw new Exception($"error while trying to compact indices for partner:[{_partnerId}]");
                }
            });
            return retryResult.Outcome == OutcomeType.Successful;
        }

        private bool DeleteIndex(string index, int retryCount = 1)
        {
            var policy = IndexManagerCommonHelpers.GetRetryPolicy<Exception>(retryCount);
            var retryResult = policy.ExecuteAndCapture(() =>
            {
                var res = _elasticSearchApi.DeleteIndices(new List<string> { index });
                if (!res)
                {
                    throw new Exception($"error while trying to delete index:[{index}] for partner:[{_partnerId}]");
                }
            });
            return retryResult.Outcome == OutcomeType.Successful;
        }

        private bool GetMetasAndTagsForMapping(
            out Dictionary<string, KeyValuePair<eESFieldType, string>> metas,
            out List<string> tags,
            out HashSet<string> metasToPad,
            bool isEpg = false)
        {
            var serializer = _serializer;

            bool result = true;
            tags = new List<string>();
            metas = new Dictionary<string, KeyValuePair<eESFieldType, string>>();

            // Padded with zero prefix metas to sort numbers by text without issues in elastic (Brilliant!)
            metasToPad = new HashSet<string>();

            if (isOpc())
            {
                var catalogGroupCache = GetCatalogGroupCache();
                if (catalogGroupCache != null)
                {
                    try
                    {
                        var topicsToIgnore = Core.Catalog.CatalogLogic.GetTopicsToIgnoreOnBuildIndex();
                        tags = catalogGroupCache.TopicsMapBySystemNameAndByType
                            .Where(x => x.Value.ContainsKey(ApiObjects.MetaType.Tag.ToString()) &&
                                        !topicsToIgnore.Contains(x.Key)).Select(x => x.Key.ToLower()).ToList();

                        foreach (var topics in catalogGroupCache.TopicsMapBySystemNameAndByType)
                        {
                            if (topics.Value.Keys.Any(x =>
                                x != ApiObjects.MetaType.Tag.ToString() &&
                                x != ApiObjects.MetaType.ReleatedEntity.ToString()))
                            {
                                var nullValue = string.Empty;
                                eESFieldType metaType;
                                var topicMetaType = CatalogManager.GetTopicMetaType(topics.Value);
                                serializer.GetMetaType(topicMetaType, out metaType, out nullValue);

                                if (topicMetaType == ApiObjects.MetaType.Number &&
                                    !metasToPad.Contains(topics.Key.ToLower()))
                                {
                                    metasToPad.Add(topics.Key.ToLower());
                                }

                                if (!metas.ContainsKey(topics.Key.ToLower()))
                                {
                                    metas.Add(topics.Key.ToLower(),
                                        new KeyValuePair<eESFieldType, string>(isEpg ? eESFieldType.STRING : metaType,
                                        nullValue));
                                }
                                else
                                {
                                    log.ErrorFormat("Duplicate topic found for group {0} name {1}", _partnerId,
                                        topics.Key.ToLower());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(
                            string.Format("Failed BuildIndex for _partnerId: {0} because CatalogGroupCache",
                                _partnerId), ex);
                        return false;
                    }
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
                                    serializer.GetMetaType(meta.Key, out metaType, out nullValue);

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
                                serializer.GetMetaType(epgMeta, out metaType, out nullValue);

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

        public string GetElasticsearchQueryForEpgIDs(IEnumerable<ulong> programIds, IEnumerable<string> externalIds, List<int> channelIds)
        {
            // Build query for getting programs
            var query = new FilteredQuery();
            var filter = new QueryFilter();

            // basic initialization
            query.PageIndex = 0;
            query.PageSize = 1;
            query.ReturnFields.Clear();

            var composite = new FilterCompositeType(CutWith.OR);

            // build terms query: epg_id IN (1, 2, 3 ... bulkSize)

            var epgIdTerms = ESTerms.GetSimpleNumericTerm("epg_id", programIds);

            composite.AddChild(epgIdTerms);

            //external id must be in one of the given channels
            var externalIdsComposite = new FilterCompositeType(CutWith.AND);
            var epgExternalIdTerms = ESTerms.GetSimpleStringTerm("epg_identifier", externalIds);
            var epgChannelIdTerms = ESTerms.GetSimpleStringTerm("epg_channel_id", channelIds);
            externalIdsComposite.AddChild(epgExternalIdTerms);
            externalIdsComposite.AddChild(epgChannelIdTerms);

            composite.AddChild(externalIdsComposite);
            filter.FilterSettings = composite;
            query.Filter = filter;

            var searchQuery = query.ToString();
            return searchQuery;
        }



        private void ExecuteAndValidateBulkRequests(List<ESBulkRequestObj<string>> bulkRequests)
        {
            // create bulk request now and clear list
            var invalidResults = _elasticSearchApi.CreateBulkRequest(bulkRequests);

            if (invalidResults != null && invalidResults.Count > 0)
            {
                foreach (var item in invalidResults)
                {
                    log.Error($"Could not add EPG to ES index. GroupID={_partnerId} epgId={item.Key} error={item.Value}");
                }
            }

            if (invalidResults.Any())
            {
                throw new Exception($"Failed to upsert [{invalidResults.Count}] documents");
            }
        }

        private static AggregationsResult ConvertAggregationsResponse(
            ESAggregationsResult aggregationsResult,
            List<string> groupBys,
            Dictionary<ElasticSearchApi.ESAssetDocument, UnifiedSearchResult> topHitsMapping)
        {
            string currentGroupBy = groupBys[0];

            var esAggregation = aggregationsResult.Aggregations[currentGroupBy];
            int totalItems = 0;

            string cardinalityKey = string.Format("{0}_count", currentGroupBy);

            if (aggregationsResult.Aggregations.ContainsKey(cardinalityKey))
            {
                totalItems = Convert.ToInt32(aggregationsResult.Aggregations[cardinalityKey].value);
            }

            //BEO-9740
            if (aggregationsResult.Aggregations[currentGroupBy].buckets.Any(x => x.key == ESUnifiedQueryBuilder.MissedHitBucketKey.ToString()))
            {
                totalItems += aggregationsResult.Aggregations[currentGroupBy].buckets
                    .Where(x => x.key == ESUnifiedQueryBuilder.MissedHitBucketKey.ToString()).First().doc_count;
            }

            var result = new AggregationsResult()
            {
                field = currentGroupBy,
                results = new List<AggregationResult>(),
                totalItems = totalItems
            };

            foreach (var bucket in esAggregation.buckets)
            {
                var bucketResult = new AggregationResult()
                {
                    value = bucket.key,
                    count = bucket.doc_count,
                };

                // go for sub aggregations, if there are
                if (groupBys.Count > 1)
                {
                    bucketResult.subs = new List<AggregationsResult>();

                    string nextGroupBy = groupBys[1];

                    var sub = ConvertAggregationsResponse(bucket.Aggregations[nextGroupBy], 1, groupBys, topHitsMapping);

                    if (sub != null)
                    {
                        bucketResult.subs.Add(sub);
                    }
                }

                if (bucket.Aggregations != null && bucket.Aggregations.ContainsKey(ESTopHitsAggregation.DEFAULT_NAME))
                {
                    var topHitsAggregation = bucket.Aggregations[ESTopHitsAggregation.DEFAULT_NAME];

                    if (topHitsAggregation.hits != null && topHitsAggregation.hits.hits != null)
                    {
                        bucketResult.topHits = new List<UnifiedSearchResult>();

                        foreach (var doc in topHitsAggregation.hits.hits)
                        {
                            UnifiedSearchResult unifiedSearchResult = null;

                            if (topHitsMapping != null && topHitsMapping.ContainsKey(doc))
                            {
                                unifiedSearchResult = topHitsMapping[doc];
                            }
                            else
                            {
                                unifiedSearchResult = new UnifiedSearchResult()
                                {
                                    AssetId = doc.asset_id.ToString(),
                                    AssetType = ESUtils.ParseAssetType(doc.type),
                                    m_dUpdateDate = doc.update_date
                                };
                            }

                            bucketResult.topHits.Add(unifiedSearchResult);
                        }
                    }
                }

                result.results.Add(bucketResult);
            }

            return result;
        }

        /// <summary>
        /// Converts the inner ESAggregationResult to the formal AggregationsResult
        /// </summary>
        /// <param name="aggregationsResult"></param>
        /// <param name="groupByIndex"></param>
        /// <returns></returns>
        private static AggregationsResult ConvertAggregationsResponse(
            ESAggregationResult aggregationsResult,
            int groupByIndex,
            List<string> groupBys,
            Dictionary<ElasticSearchApi.ESAssetDocument, UnifiedSearchResult> topHitsMapping)
        {
            // validate parameter
            if (groupByIndex > groupBys.Count)
            {
                return null;
            }

            string currentGroupBy = groupBys[groupByIndex];

            int totalItems = 0;
            string cardinalityKey = string.Format("{0}_count", currentGroupBy);

            if (aggregationsResult.Aggregations.ContainsKey(cardinalityKey))
            {
                totalItems = Convert.ToInt32(aggregationsResult.Aggregations[cardinalityKey].value);
            }

            var result = new AggregationsResult()
            {
                field = groupBys[groupByIndex],
                totalItems = totalItems
            };

            foreach (var bucket in aggregationsResult.buckets)
            {
                var bucketResult = new AggregationResult()
                {
                    value = bucket.key,
                    count = bucket.doc_count
                };

                // go for sub aggregations
                if (groupBys.Count > groupByIndex + 1)
                {
                    string nextGroupBy = groupBys[groupByIndex + 1];

                    var sub = ConvertAggregationsResponse(bucket.Aggregations[nextGroupBy], 1, groupBys, topHitsMapping);

                    if (sub != null)
                    {
                        bucketResult.subs.Add(sub);
                    }
                }

                if (bucket.Aggregations != null && bucket.Aggregations.ContainsKey(ESTopHitsAggregation.DEFAULT_NAME))
                {
                    var topHitsAggregation = bucket.Aggregations[ESTopHitsAggregation.DEFAULT_NAME];

                    if (topHitsAggregation.hits != null && topHitsAggregation.hits.hits != null)
                    {
                        bucketResult.topHits = new List<UnifiedSearchResult>();

                        foreach (var doc in topHitsAggregation.hits.hits)
                        {
                            UnifiedSearchResult unifiedSearchResult = null;

                            if (topHitsMapping != null && topHitsMapping.ContainsKey(doc))
                            {
                                unifiedSearchResult = topHitsMapping[doc];
                            }
                            else
                            {
                                unifiedSearchResult = new UnifiedSearchResult()
                                {
                                    AssetId = doc.asset_id.ToString(),
                                    AssetType = ESUtils.ParseAssetType(doc.type),
                                    m_dUpdateDate = doc.update_date
                                };
                            }

                            bucketResult.topHits.Add(unifiedSearchResult);
                        }
                    }
                }

                result.results.Add(bucketResult);
            }

            return result;
        }

        private void GetEpgAnalyzers(IEnumerable<LanguageObj> languages, out List<string> analyzers, out List<string> filters, out List<string> tokenizers)
        {
            analyzers = new List<string>();
            filters = new List<string>();
            tokenizers = new List<string>();

            if (languages != null)
            {
                foreach (LanguageObj language in languages)
                {
                    string analyzer = _esIndexDefinitions.GetAnalyzerDefinition(ESUtils.GetLangCodeAnalyzerKey(language.Code, ES_VERSION));
                    string filter = _esIndexDefinitions.GetFilterDefinition(ESUtils.GetLangCodeFilterKey(language.Code, ES_VERSION));
                    string tokenizer = _esIndexDefinitions.GetTokenizerDefinition(ESUtils.GetLangCodeTokenizerKey(language.Code, ES_VERSION));

                    if (string.IsNullOrEmpty(analyzer))
                    {
                        log.Error(string.Format("analyzer for language {0} doesn't exist", language.Code));
                    }
                    else
                    {
                        analyzers.Add(analyzer);
                    }

                    if (!string.IsNullOrEmpty(filter))
                    {
                        filters.Add(filter);
                    }

                    if (!string.IsNullOrEmpty(tokenizer))
                    {
                        tokenizers.Add(tokenizer);
                    }
                }

                // we always want a lowercase analyzer
                analyzers.Add(LOWERCASE_ANALYZER);

                // we always want "autocomplete" ability
                filters.Add(PHRASE_STARTS_WITH_FILTER);
                analyzers.Add(PHRASE_STARTS_WITH_ANALYZER);
                analyzers.Add(PHRASE_STARTS_WITH_SEARCH_ANALYZER);
            }
        }

        private void AddLanguageMapping(string indexName,
            LanguageObj language,
            LanguageObj defaultLanguage,
            Dictionary<string, KeyValuePair<eESFieldType, string>> metas,
            List<string> tags,
            HashSet<string> metasToPad, EpgFeatureVersion epgFeatureVersion)
        {
            var defaultMappingAnalyzers = GetMappingAnalyzers(defaultLanguage, ES_VERSION);
            var specificMappingAnalyzers = GetMappingAnalyzers(language, ES_VERSION);
            var mappingName = GetIndexType(false, language);

            var transactionParentDocumentType = epgFeatureVersion == EpgFeatureVersion.V3 ? NamingHelper.EPG_V3_TRANSACTION_DOCUMENT_TYPE_NAME : null;
            var mapping = _serializer.CreateEpgMapping(metas, tags, metasToPad, specificMappingAnalyzers,
                defaultMappingAnalyzers, mappingName, true, transactionParentDocumentType);

            var success = _elasticSearchApi.InsertMapping(indexName, mappingName, mapping);
            if (!success) throw new Exception($"Failed to add mapping. index: [{indexName}]. mapping [{mappingName}]");
        }

        private MappingAnalyzers GetMappingAnalyzers(LanguageObj language, string version)
        {
            MappingAnalyzers specificMappingAnlyzers = new MappingAnalyzers();

            // create names for analyzers to be used in the mapping later on
            string analyzerDefinitionName = ESUtils.GetLangCodeAnalyzerKey(language.Code, version);

            if (_esIndexDefinitions.AnalyzerExists(analyzerDefinitionName))
            {
                specificMappingAnlyzers.normalIndexAnalyzer = $"{language.Code}_index_analyzer";
                specificMappingAnlyzers.normalSearchAnalyzer = $"{language.Code}_search_analyzer";

                var analyzerDefinition = _esIndexDefinitions.GetAnalyzerDefinition(analyzerDefinitionName);

                var autocompleteIndexAnalyzerCandidate = $"{language.Code}_autocomplete_analyzer";
                if (analyzerDefinition.Contains(autocompleteIndexAnalyzerCandidate))
                {
                    specificMappingAnlyzers.autocompleteIndexAnalyzer = autocompleteIndexAnalyzerCandidate;
                }

                var autocompleteSearchAnalyzerCandidate = $"{language.Code}_autocomplete_search_analyzer";
                if (analyzerDefinition.Contains(autocompleteSearchAnalyzerCandidate))
                {
                    specificMappingAnlyzers.autocompleteSearchAnalyzer = autocompleteSearchAnalyzerCandidate;
                }

                var dblMetaphoneIndexAnalyzerCandidate = $"{language.Code}_index_dbl_metaphone";
                var phoneticIndexAnalyzerCandidate = $"{language.Code}_index_phonetic";
                if (analyzerDefinition.Contains(dblMetaphoneIndexAnalyzerCandidate))
                {
                    specificMappingAnlyzers.phoneticIndexAnalyzer = dblMetaphoneIndexAnalyzerCandidate;
                }
                else if (analyzerDefinition.Contains(phoneticIndexAnalyzerCandidate))
                {
                    specificMappingAnlyzers.phoneticIndexAnalyzer = phoneticIndexAnalyzerCandidate;
                }

                var dblMetaphoneSearchAnalyzerCandidate = $"{language.Code}_search_dbl_metaphone";
                var phoneticSearchAnalyzerCandidate = $"{language.Code}_search_phonetic";
                if (analyzerDefinition.Contains(dblMetaphoneSearchAnalyzerCandidate))
                {
                    specificMappingAnlyzers.phoneticSearchAnalyzer = dblMetaphoneSearchAnalyzerCandidate;
                }
                else if (analyzerDefinition.Contains(phoneticIndexAnalyzerCandidate))
                {
                    specificMappingAnlyzers.phoneticSearchAnalyzer = phoneticSearchAnalyzerCandidate;
                }

                var lowercaseAnalyzerCandidate = $"{language.Code}_lowercase_analyzer";
                if (analyzerDefinition.Contains(lowercaseAnalyzerCandidate))
                {
                    specificMappingAnlyzers.lowercaseAnalyzer = lowercaseAnalyzerCandidate;
                }

                var phraseStartsWithAnalyzerCandidate = $"{language.Code}_phrase_starts_with_analyzer";
                if (analyzerDefinition.Contains(phraseStartsWithAnalyzerCandidate))
                {
                    specificMappingAnlyzers.phraseStartsWithAnalyzer = phraseStartsWithAnalyzerCandidate;
                }

                var phraseStartsWithSearchAnalyzerCandidate = $"{language.Code}_phrase_starts_with_search_analyzer";
                if (analyzerDefinition.Contains(phraseStartsWithSearchAnalyzerCandidate))
                {
                    specificMappingAnlyzers.phraseStartsWithSearchAnalyzer = phraseStartsWithSearchAnalyzerCandidate;
                }

                var sortingAnalyzerCandidate = $"{language.Code}_sorting_analyzer";
                if (analyzerDefinition.Contains(sortingAnalyzerCandidate))
                {
                    specificMappingAnlyzers.sortingAnalyzer = sortingAnalyzerCandidate;
                }
            }
            else
            {
                specificMappingAnlyzers.normalIndexAnalyzer = "whitespace";
                specificMappingAnlyzers.normalSearchAnalyzer = "whitespace";
                log.Error(string.Format("could not find analyzer for language ({0}) for mapping. whitespace analyzer will be used instead", language.Code));
            }

            specificMappingAnlyzers.suffix = null;

            if (!language.IsDefault)
            {
                specificMappingAnlyzers.suffix = language.Code;
            }

            return specificMappingAnlyzers;
        }

        private string GetIndexType(bool isRecording, ApiObjects.LanguageObj language = null)
        {
            return _mappingTypeResolver.GetMappingType(isRecording, language);
        }

        /// <summary>
        /// will create a new index in case not exists
        /// </summary>
        private void CreateIndex(string index, string[] aliases)
        {
            if (!_elasticSearchApi.IndexExists(index))
            {
                CreateNewEpgIndex(index);
                foreach (var indexAlias in aliases)
                {
                    _elasticSearchApi.AddAlias(index, indexAlias);
                }
            }
        }


        private bool UpdateChannelPercolator(Channel channel, List<string> mediaAliases, List<string> epgAliases)
        {
            var esApi = _elasticSearchApi;
            bool result = false;
            if (channel != null)
            {
                bool isMedia = false;
                bool isEpg = false;

                string channelQueryForMedia = string.Empty;
                string channelQueryForEpg = string.Empty;

                bool groupUsesTemplates = isOpc();

                if ((channel.m_nChannelTypeID == (int)ChannelType.KSQL) ||
                    (channel.m_nChannelTypeID == (int)ChannelType.Manual && groupUsesTemplates && channel.AssetUserRuleId > 0))
                {
                    if (channel.m_nChannelTypeID == (int)ChannelType.Manual && channel.AssetUserRuleId > 0)
                    {
                        var mediaIds = channel.m_lChannelTags.SelectMany(x => x.m_lValue);
                        channel.filterQuery = $"(or media_id:'{string.Join(",", mediaIds)}')";
                    }

                    UnifiedSearchDefinitions definitions = _channelQueryBuilder.BuildSearchDefinitions(channel);

                    isMedia = definitions.shouldSearchMedia;
                    isEpg = definitions.shouldSearchEpg;

                    if (isMedia)
                    {
                        definitions.shouldSearchEpg = false;

                        ESUnifiedQueryBuilder unifiedQueryBuilder = new ESUnifiedQueryBuilder(_esSortingService, _sortingAdapter, _queryInitializer, definitions);
                        channelQueryForMedia = unifiedQueryBuilder.BuildSearchQueryString(true);
                    }

                    if (isEpg)
                    {
                        definitions.shouldSearchEpg = true;
                        definitions.shouldSearchMedia = false;

                        ESUnifiedQueryBuilder unifiedQueryBuilder = new ESUnifiedQueryBuilder(_esSortingService, _sortingAdapter, _queryInitializer, definitions);
                        channelQueryForEpg = unifiedQueryBuilder.BuildSearchQueryString(true);
                    }
                }
                else
                {
                    isMedia = true;
                    ESMediaQueryBuilder mediaQueryParser = new ESMediaQueryBuilder()
                    {
                        QueryType = eQueryType.EXACT
                    };

                    mediaQueryParser.m_nGroupID = channel.m_nGroupID;
                    MediaSearchObj mediaSearchObject = _channelQueryBuilder.BuildBaseChannelSearchObject(channel);

                    mediaQueryParser.oSearchObject = mediaSearchObject;
                    channelQueryForMedia = mediaQueryParser.BuildSearchQueryString(true);
                }

                if (isMedia && !string.IsNullOrEmpty(channelQueryForMedia))
                {
                    log.DebugFormat("Update channel for media with query: {0}", channelQueryForMedia);

                    foreach (string alias in mediaAliases)
                    {
                        result = esApi.AddQueryToPercolatorV2(alias, channel.m_nChannelID.ToString(), ref channelQueryForMedia);
                    }
                }

                if (isEpg && !string.IsNullOrEmpty(channelQueryForEpg))
                {
                    log.DebugFormat("Update channel for epg with query: {0}", channelQueryForEpg);

                    foreach (string alias in epgAliases)
                    {
                        result = esApi.AddQueryToPercolatorV2(alias, channel.m_nChannelID.ToString(), ref channelQueryForEpg);
                    }
                }
            }

            return result;
        }

        private void AddMappingsToEpgIndex(string indexName, bool isRecording)
        {
            var languages = GetLanguages();
            var defaultLanguage = GetDefaultLanguage();
            var defaultMappingAnalyzers = GetMappingAnalyzers(defaultLanguage, ES_VERSION);

            if (!GetMetasAndTagsForMapping(out Dictionary<string, KeyValuePair<eESFieldType, string>> metas,
                out List<string> tags, out var metasToPad, true))
            {
                throw new Exception("Failed GetMetasAndTagsForMapping as part of BuildIndex");
            }

            foreach (ApiObjects.LanguageObj language in languages)
            {
                // TODO could use AddLanguageMapping here
                MappingAnalyzers specificMappingAnalyzers = GetMappingAnalyzers(language, ES_VERSION);
                string specificType = GetIndexType(isRecording, language);

                var shouldAddRouting = true;
                var epgFeatureVersion = _groupSettingsManager.GetEpgFeatureVersion(_partnerId);
                var transactionParentDocumentType = epgFeatureVersion == EpgFeatureVersion.V3 ? NamingHelper.EPG_V3_TRANSACTION_DOCUMENT_TYPE_NAME : null;
                string mappingString = _serializer.CreateEpgMapping(metas, tags, metasToPad, specificMappingAnalyzers, defaultMappingAnalyzers, specificType, shouldAddRouting, transactionParentDocumentType);
                bool isMappingInsertSuccess = _elasticSearchApi.InsertMapping(indexName, specificType, mappingString.ToString());

                if (!isMappingInsertSuccess)
                {
                    if (language.IsDefault)
                    {
                        throw new Exception($"Could not insert mappings to default lanague");
                    }
                    else
                    {
                        log.Error(string.Concat("Could not create mapping of type epg for language ", language.Name));
                    }
                }
            }
        }


        private UnifiedSearchDefinitions BuildSearchDefinitions(Channel channel)
        {
            UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions();

            definitions.groupId = channel.m_nGroupID;

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

            definitions.permittedWatchRules = GetPermittedWatchRules(channel.m_nGroupID);
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

            var groupManager = GetGroupManager();

            if (channel.AssetUserRuleId.HasValue && channel.AssetUserRuleId.Value > 0)
            {
                var assetUserRule = AssetUserRuleManager.GetAssetUserRuleByRuleId(channel.m_nGroupID, channel.AssetUserRuleId.Value);

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

                    CatalogLogic.UpdateNodeTreeFields(dummyRequest, ref phrase, definitions, groupManager, groupManager.m_nParentGroupID);

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
                    ref definitions.filterPhrase, definitions, groupManager, channel.m_nParentGroupID);
            }

            return definitions;
        }

        private MediaSearchObj BuildBaseChannelSearchObject(Channel channel)
        {
            MediaSearchObj searchObject = new MediaSearchObj();
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

        private void GetTagsAndChannelsAnalyzers(List<ApiObjects.LanguageObj> languages, out List<string> analyzers, out List<string> filters, out List<string> tokenizers)
        {
            analyzers = new List<string>();
            filters = new List<string>();
            tokenizers = new List<string>();

            if (languages != null)
            {
                foreach (ApiObjects.LanguageObj language in languages)
                {
                    string analyzer = _esIndexDefinitions.GetAnalyzerDefinition(ESUtils.GetLangCodeAnalyzerKey(language.Code, VERSION));
                    string filter = _esIndexDefinitions.GetFilterDefinition(ESUtils.GetLangCodeFilterKey(language.Code, VERSION));
                    string tokenizer = _esIndexDefinitions.GetTokenizerDefinition(ESUtils.GetLangCodeTokenizerKey(language.Code, VERSION));

                    if (string.IsNullOrEmpty(analyzer))
                    {
                        log.Error(string.Format("analyzer for language {0} doesn't exist", language.Code));
                    }
                    else
                    {
                        analyzers.Add(analyzer);
                    }

                    if (!string.IsNullOrEmpty(filter))
                    {
                        filters.Add(filter);
                    }

                    if (!string.IsNullOrEmpty(tokenizer))
                    {
                        tokenizers.Add(tokenizer);
                    }
                }

                // we always want a lowercase analyzer
                analyzers.Add(LOWERCASE_ANALYZER);
            }
        }

        private List<string> ExtractChannelsDefinitionsOutOfResultJSON(string sESPercolatorResultJSON)
        {
            List<string> res = null;
            JObject json = JObject.Parse(sESPercolatorResultJSON);
            if (json != null)
            {
                var docsArray = json.SelectToken("docs");
                if (docsArray is JArray)
                {
                    JArray docs = (JArray)docsArray;
                    int length = docs.Count;
                    res = new List<string>(length);
                    string[] orderedPathDownTheJSONTree = new string[4] { "_source", "query", "filtered", "filter" };
                    string definition = string.Empty;
                    for (int i = 0; i < length; i++)
                    {
                        if (TVinciShared.JSONUtils.TryGetJSONToken(docs[i], orderedPathDownTheJSONTree, ref definition) && definition.Length > 0)
                            res.Add(definition);
                        definition = string.Empty;
                    }
                }
            }

            if (res == null)
                return new List<string>(0);
            return res;
        }

        private BoolQuery BuildMultipleSearchQuery(List<BaseSearchObject> searchObjects, int parentGroupId, bool shouldMinimizeQuery = false)
        {
            ESMediaQueryBuilder mediaQueryBuilder = new ESMediaQueryBuilder();
            ESUnifiedQueryBuilder unifiedQueryBuilder = new ESUnifiedQueryBuilder(_esSortingService, _sortingAdapter, _queryInitializer, null, parentGroupId);

            BoolQuery boolQuery = new BoolQuery();

            /*
             * Foreach media/unified search object, create filtered query.
             * Add the query's filter to the grouped filter so that we can then create a single request
             * containing all the channels that we want.
             */
            foreach (BaseSearchObject searchObject in searchObjects)
            {
                if (searchObject == null)
                    continue;

                if (searchObject is MediaSearchObj)
                {
                    MediaSearchObj mediaSearchObject = searchObject as MediaSearchObj;
                    mediaQueryBuilder.m_nGroupID = mediaSearchObject.m_nGroupId;
                    mediaSearchObject.m_nPageSize = 0;
                    mediaQueryBuilder.oSearchObject = mediaSearchObject;
                    mediaQueryBuilder.QueryType = (mediaSearchObject.m_bExact) ? eQueryType.EXACT : eQueryType.BOOLEAN;
                    FilteredQuery tempQuery = mediaQueryBuilder.BuildChannelFilteredQuery(true, shouldMinimizeQuery);

                    if (tempQuery != null && tempQuery.Filter != null)
                    {
                        ESFilteredQuery currentFilteredQuery = new ESFilteredQuery()
                        {
                            Filter = tempQuery.Filter
                        };

                        boolQuery.AddChild(currentFilteredQuery, CutWith.OR);
                    }
                }
                else if (searchObject is UnifiedSearchDefinitions)
                {
                    UnifiedSearchDefinitions definitions = searchObject as UnifiedSearchDefinitions;
                    unifiedQueryBuilder.SearchDefinitions = definitions;

                    BaseFilterCompositeType currentFilter;
                    IESTerm currentQuery;

                    unifiedQueryBuilder.BuildInnerFilterAndQuery(out currentFilter, out currentQuery, definitions.shouldIgnoreDeviceRuleID, true, shouldMinimizeQuery);

                    ESFilteredQuery currentFilteredQuery = new ESFilteredQuery()
                    {
                        Filter = new QueryFilter()
                        {
                            FilterSettings = currentFilter
                        },
                        Query = currentQuery
                    };

                    boolQuery.AddChild(currentFilteredQuery, CutWith.OR);
                }
            }

            return boolQuery;
        }

        private List<ElasticSearchApi.ESAssetDocument> DecodeEpgMultiSearchJsonObject(string sObj, ref int totalItems)
        {
            List<ElasticSearchApi.ESAssetDocument> documents = new List<ElasticSearchApi.ESAssetDocument>();
            try
            {
                var jsonObj = JObject.Parse(sObj);

                if (jsonObj != null)
                {
                    int nTotalItems = 0;
                    int tempTotal = 0;
                    List<ElasticSearchApi.ESAssetDocument> tempDocs;
                    List<List<ElasticSearchApi.ESAssetDocument>> l = jsonObj.SelectToken("responses").Select(item =>
                    {
                        tempDocs = ESUtils.DecodeAssetSearchJsonObject(item.ToString(), ref tempTotal);
                        nTotalItems += tempTotal;
                        if (tempDocs != null && tempDocs.Count > 0)
                            documents.AddRange(tempDocs);

                        return tempDocs;
                    }).ToList();
                    totalItems = nTotalItems;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Json Deserialization failed for ElasticSearch Media request. Ex Msg: {0} , ", ex.Message), ex);
            }

            return documents;
        }

        public bool SetupChannelPercolatorIndex(DateTime indexDate)
        {
            return false;
        }

        public void PublishChannelPercolatorIndex(DateTime indexDate, bool shouldSwitchIndexAlias,
            bool shouldDeleteOldIndices)
        {

        }

        private static string GetTranslationType(string sType, LanguageObj oLanguage)
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

        #endregion
    }
}
