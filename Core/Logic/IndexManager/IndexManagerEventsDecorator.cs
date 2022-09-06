using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ApiObjects.Statistics;
using Catalog.Response;
using Core.Catalog;
using Core.Catalog.Response;
using ElasticSearch.Common;
using GroupsCacheManager;
using Polly.Retry;
using System.Reflection;
using ApiLogic.IndexManager.Helpers;
using Couchbase.Utils;
using EventBus.Abstraction;
using MoreLinq;
using OTT.Lib.Kafka;
using TVinciShared;
using Channel = GroupsCacheManager.Channel;

namespace ApiLogic.Catalog.IndexManager
{

    public class IndexManagerEventsDecorator : IIndexManager
    {
        private const int BatchChunkSize = 10;
        private readonly IIndexManager _indexManager;
        private readonly Func<IKafkaContextProvider, IEventBusPublisher> _publisherFunc;
        private readonly IndexManagerVersion _indexManagerVersion;
        private readonly int _partnerId;
        private readonly Type _indexManagerType;

        public IndexManagerEventsDecorator(IIndexManager indexManager,
            Func<IKafkaContextProvider, IEventBusPublisher> publisherFunc,
            IndexManagerVersion indexManagerVersion,
            int partnerId)
        {
            _indexManager = indexManager;
            _publisherFunc = publisherFunc;
            _indexManagerVersion = indexManagerVersion;
            _indexManagerType = _indexManager.GetType();
            _partnerId = partnerId;
        }

        #region Execute Decoration


        private T Execute<T>(MethodBase methodBase, string eventKey, params object[] methodParameters)
        {
            var methodBaseName = methodBase.Name;
            var methodInfo = _indexManagerType.GetMethod(methodBaseName);
            var result = (T)methodInfo.Invoke(_indexManager, methodParameters);
            CallMigrateEvent(methodBaseName, eventKey, methodParameters);
            return result;
        }

        private void Execute(MethodBase methodBase, string eventKey, IEnumerable<object[]> transformedParameters, params object[] methodParameters)
        {
            var methodBaseName = methodBase.Name;
            var methodInfo = _indexManagerType.GetMethod(methodBaseName);
            methodInfo.Invoke(_indexManager, methodParameters);
            foreach (var transformedMethodParameters in transformedParameters ?? new []{methodParameters})
            {
                CallMigrateEvent(methodBaseName, eventKey, transformedMethodParameters);
            }
        }

        private void CallMigrateEvent(string methodName, string eventKey, params object[] methodParameters)
        {
            var migrationEvent = new ApiObjects.DataMigrationEvents.ElasticSearchMigrationEvent(_indexManagerVersion.ToString(), _partnerId)
            {
                MethodName = methodName,
                Parameters = methodParameters,
                EventKey = eventKey
            };

            _publisherFunc(migrationEvent).Publish(migrationEvent);
        }

        #endregion

        #region CUD

        //CUD
        public bool UpsertMedia(long assetId)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.MEDIA, assetId);
        }

        //CUD
        public bool CompactEpgV2Indices(int futureIndexCompactionStart, int pastIndexCompactionStart)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.EPG, futureIndexCompactionStart, pastIndexCompactionStart);
        }

        //CUD
        public bool DeleteProgram(List<long> assetIds, bool isRecording = false)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.EPG, assetIds, isRecording);
        }

        //CUD
        public bool UpsertProgram(List<EpgCB> epgObjects, Dictionary<string, LinearChannelSettings> linearChannelSettings)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.EPG, epgObjects, linearChannelSettings);
        }

        //CUD
        public bool DeleteChannel(int channelId)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.CHANNEL, channelId);
        }

        //CUD
        public bool UpsertChannel(int channelId, Channel channel = null, long userId = 0)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.CHANNEL, channelId, channel, userId);
        }

        //CUD
        public bool DeleteMedia(long assetId)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.MEDIA, assetId);
        }

        public void DeleteMediaByTypeAndFinalEndDate(long mediaTypeId, DateTime finalEndDate)
        {
            Execute(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.MEDIA, null, mediaTypeId, finalEndDate);
        }

        //CUD
        public void UpsertPrograms(IList<EpgProgramBulkUploadObject> calculatedPrograms, string draftIndexName, LanguageObj defaultLanguage, IDictionary<string, LanguageObj> languages)
        {
            Execute(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.EPG, null, calculatedPrograms, draftIndexName, defaultLanguage, languages);
        }

        //CUD
        public void DeletePrograms(IList<EpgProgramBulkUploadObject> programsToDelete, string epgIndexName, IDictionary<string, LanguageObj> languages)
        {
            Execute(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.EPG,
                null, programsToDelete, epgIndexName,
                languages);
        }

        //CUD
        public bool DeleteChannelPercolator(List<int> channelIds)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.CHANNEL, channelIds);
        }

        //CUD
        public bool UpdateChannelPercolator(List<int> channelIds, Channel channel = null)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.CHANNEL, channelIds, channel);
        }

        //CUD
        public Status UpdateTag(TagValue tag)
        {
            return Execute<Status>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.TAG, tag);
        }

        //CUD
        public Status DeleteTag(long tagId)
        {
            return Execute<Status>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.TAG, tagId);
        }

        //CUD
        public Status DeleteTagsByTopic(long topicId)
        {
            return Execute<Status>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.TAG, topicId);
        }

        //CUD
        public bool DeleteSocialAction(StatisticsActionSearchObj socialSearch)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.STATS, socialSearch);
        }

        //CUD
        public string SetupIPToCountryIndex()
        {
            return Execute<string>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.IP_TO_COUNTRY);
        }

        //CUD
        public bool InsertDataToIPToCountryIndex(string newIndexName,
            List<IPV4> ipV4ToCountryMapping, List<IPV6> ipV6ToCountryMapping)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.IP_TO_COUNTRY, newIndexName, ipV4ToCountryMapping, ipV6ToCountryMapping);
        }

        //CUD
        public bool PublishIPToCountryIndex(string newIndexName)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.IP_TO_COUNTRY, newIndexName);
        }

        //CUD
        public bool SetupMediaIndex(DateTime indexDate)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.MEDIA, indexDate);
        }

        //CUD
        public bool SetupChannelPercolatorIndex(DateTime indexDate)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.MEDIA, indexDate);
        }

        //CUD
        public bool AddChannelsPercolatorsToIndex(HashSet<int> channelIds, DateTime? indexDate, bool shouldCleanupInvalidChannels = false)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.CHANNEL, channelIds, indexDate, shouldCleanupInvalidChannels);
        }

        //CUD
        public string SetupChannelMetadataIndex(DateTime indexDate)
        {
            return Execute<string>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.CHANNEL_METADATA, indexDate);
        }

        //CUD
        public void AddChannelsMetadataToIndex(string newIndexName, List<Channel> allChannels)
        {
            // Constant value for chunking is used to make is as simple as it could be.
            // Possible solutions:
            // 1. Move it to TCM configuration and empirically choose batch size.
            // 2. Calculate the size of the message to be published in memory. There are a lot of possible problems (LOH grows, memory consumption, etc.)
            // Taking into account that replication between ESV2 and ESV7 is a temporary solution - let's avoid unnecessary complexity.
            var transformedParameters = allChannels.Batch(BatchChunkSize)
                .Select(batch => new object[] {newIndexName, batch.ToList()})
                .ToArray();

            Execute(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.CHANNEL_METADATA,
                transformedParameters, newIndexName, allChannels);
        }

        //CUD
        public void PublishChannelsMetadataIndex(string newIndexName, bool shouldSwitchAlias, bool shouldDeleteOldIndices)
        {
            Execute(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.CHANNEL_METADATA,
                null, newIndexName, shouldSwitchAlias, shouldDeleteOldIndices);
        }

        //CUD
        public string SetupTagsIndex(DateTime indexDate)
            => Execute<string>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.TAG, indexDate);

        //CUD
        public void InsertTagsToIndex(string newIndexName, List<TagValue> allTagValues)
        {
            // Constant value for chunking is used to make is as simple as it could be.
            // Possible solutions:
            // 1. Move it to TCM configuration and empirically choose batch size.
            // 2. Calculate the size of the message to be published in memory. There are a lot of possible problems (LOH grows, memory consumption, etc.)
            // Taking into account that replication between ESV2 and ESV7 is a temporary solution - let's avoid unnecessary complexity.
            var transformedParameters = allTagValues.Batch(BatchChunkSize)
                .Select(batch => new object[] {newIndexName, batch.ToList()})
                .ToArray();

            Execute(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.TAG, transformedParameters, newIndexName, allTagValues);
        }

        //CUD
        public bool PublishTagsIndex(string newIndexName, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.TAG,
                newIndexName, shouldSwitchIndexAlias, shouldDeleteOldIndices);
        }

        //CUD
        public void InsertMedias(Dictionary<int, Dictionary<int, Media>> groupMedias, DateTime indexDate)
        {
            // Constant value for chunking is used to make is as simple as it could be.
            // Possible solutions:
            // 1. Move it to TCM configuration and empirically choose batch size.
            // 2. Calculate the size of the message to be published in memory. There are a lot of possible problems (LOH grows, memory consumption, etc.)
            // Taking into account that replication between ESV2 and ESV7 is a temporary solution - let's avoid unnecessary complexity.
            var transformedParameters = groupMedias.Batch(BatchChunkSize)
                .Select(batch => new object[] {batch.ToDictionary(), indexDate})
                .ToArray();

            Execute(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.MEDIA, transformedParameters, groupMedias, indexDate);
        }

        //CUD
        public void PublishMediaIndex(DateTime indexDate, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            Execute(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.MEDIA,
                null,
                indexDate, shouldSwitchIndexAlias, shouldDeleteOldIndices);
        }

        //CUD
        public string SetupEpgIndex(DateTime indexDate, bool isRecording)
        {
            var eventKey = isRecording ? IndexManagerMigrationEventKeys.RECORDING : IndexManagerMigrationEventKeys.EPG;
            return Execute<string>(MethodBase.GetCurrentMethod(), eventKey, indexDate, isRecording);
        }

        //CUD
        public void AddEPGsToIndex(string index, bool isRecording, Dictionary<ulong, Dictionary<string, EpgCB>> programs,
            Dictionary<long, List<int>> linearChannelsRegionsMapping,
            Dictionary<long, long> epgToRecordingMapping)
        {
            var eventKey = isRecording ? IndexManagerMigrationEventKeys.RECORDING : IndexManagerMigrationEventKeys.EPG;
            // Constant value for chunking is used to make is as simple as it could be.
            // Possible solutions:
            // 1. Move it to TCM configuration and empirically choose batch size.
            // 2. Calculate the size of the message to be published in memory. There are a lot of possible problems (LOH grows, memory consumption, etc.)
            // Taking into account that replication between ESV2 and ESV7 is a temporary solution - let's avoid unnecessary complexity.
            var transformedParameters = programs.Batch(BatchChunkSize)
                .Select(batch => new object[] {index, isRecording, batch.ToDictionary(), linearChannelsRegionsMapping, epgToRecordingMapping})
                .ToArray();
            Execute(MethodBase.GetCurrentMethod(), eventKey,
                transformedParameters,
                index,
                isRecording,
                programs,
                linearChannelsRegionsMapping,
                epgToRecordingMapping);
        }

        //CUD
        public bool PublishEpgIndex(string newIndexName, bool isRecording, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            var eventKey = isRecording ? IndexManagerMigrationEventKeys.RECORDING : IndexManagerMigrationEventKeys.EPG;
            return Execute<bool>(MethodBase.GetCurrentMethod(), eventKey,
                newIndexName, isRecording, shouldSwitchIndexAlias, shouldDeleteOldIndices);
        }

        //CUD
        public bool UpdateEpgs(List<EpgCB> epgObjects, bool isRecording,
            Dictionary<long, long> epgToRecordingMapping = null)
        {
            var eventKey = isRecording ? IndexManagerMigrationEventKeys.RECORDING : IndexManagerMigrationEventKeys.EPG;
            return Execute<bool>(MethodBase.GetCurrentMethod(), eventKey,
                epgObjects, isRecording, epgToRecordingMapping);
        }

        public bool UpdateEpgsPartial(EpgPartialUpdate[] epgs)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.EPG, epgs);
        }

        //CUD
        public string SetupEpgV2Index(string indexName)
        {
            return Execute<string>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.EPG, indexName);
        }

        //CUD
        public bool SetupSocialStatisticsDataIndex()
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.STATS);
        }

        public bool InsertSocialStatisticsData(SocialActionStatistics action)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.STATS, action);
        }


        //CUD
        public bool ForceRefreshEpgV2Index(string indexName)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.EPG, indexName);
        }

        //CUD
        public bool FinalizeEpgV2Indices(List<DateTime> date)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.EPG, date);
        }

        public void PublishChannelPercolatorIndex(DateTime indexDate, bool shouldSwitchIndexAlias,
            bool shouldDeleteOldIndices)
        {
            Execute(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.CHANNEL,
                null,
                indexDate, shouldSwitchIndexAlias, shouldDeleteOldIndices);
        }

        #endregion

        #region READ
        public SearchResultsObj SearchEpgs(EpgSearchObj epgSearch)
        {
            return _indexManager.SearchEpgs(epgSearch);
        }

        public List<string> GetAutoCompleteList(MediaSearchObj mediaSearch, int nLangID, ref int nTotalItems)
        {
            return _indexManager.GetAutoCompleteList(mediaSearch, nLangID, ref nTotalItems);
        }
                public Country GetCountryByCountryName(string countryName)
        {
            return _indexManager.GetCountryByCountryName(countryName);
        }

        public Country GetCountryByIp(string ip, out bool searchSuccess)
        {
            return _indexManager.GetCountryByIp(ip, out searchSuccess);
        }

        public List<string> GetChannelPrograms(int channelId, DateTime startDate, DateTime endDate, List<ESOrderObj> esOrderObjs)
        {
            return _indexManager.GetChannelPrograms(channelId, startDate, endDate, esOrderObjs);
        }

        public List<string> GetEpgCBDocumentIdsByEpgId(IEnumerable<long> epgIds, IEnumerable<LanguageObj> languages)
        {
            return _indexManager.GetEpgCBDocumentIdsByEpgId(epgIds, languages);
        }

        public List<UnifiedSearchResult> GetAssetsUpdateDates(List<UnifiedSearchResult> assets, ref int totalItems, int pageSize, int pageIndex,
            bool shouldIgnoreRecordings = false)
        {
            return _indexManager.GetAssetsUpdateDates(
                assets,
                ref totalItems,
                pageSize,
                pageIndex,
                shouldIgnoreRecordings);

        }

        public void GetAssetStats(List<int> assetIDs, DateTime startDate, DateTime endDate, StatsType type,
            ref Dictionary<int, AssetStatsResult> assetIDsToStatsMapping)
        {
            _indexManager.GetAssetStats(assetIDs, startDate, endDate, type, ref assetIDsToStatsMapping);
        }

        public List<int> OrderMediaBySlidingWindow(OrderBy orderBy, bool isDesc, int pageSize, int PageIndex, List<int> media,
            DateTime windowTime)
        {
            return _indexManager.OrderMediaBySlidingWindow(orderBy, isDesc, pageSize, PageIndex, media, windowTime);
        }

        public IList<EpgProgramBulkUploadObject> GetCurrentProgramsByDate(int channelId, DateTime fromDate,
            DateTime toDate)
        {
            return _indexManager.GetCurrentProgramsByDate(channelId, fromDate, toDate);
        }

        public IList<EpgProgramInfo> GetCurrentProgramInfosByDate(int channelId, DateTime fromDate, DateTime toDate)
        {
            return _indexManager.GetCurrentProgramInfosByDate(channelId, fromDate, toDate);
        }

        public List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearch, ref int totalItems)
        {
            return _indexManager.UnifiedSearch(unifiedSearch, ref totalItems);
        }

        public List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearch,
            ref int totalItems,
            out List<AggregationsResult> aggregationsResult)
        {
            return _indexManager.UnifiedSearch(unifiedSearch, ref totalItems, out aggregationsResult);
        }

        public AggregationsResult UnifiedSearchForGroupBy(UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            return _indexManager.UnifiedSearchForGroupBy(unifiedSearchDefinitions);
        }

        public List<UnifiedSearchResult> SearchSubscriptionAssets(List<BaseSearchObject> searchObjects, int languageId, bool useStartDate, string mediaTypes, OrderObj order,
            int pageIndex, int pageSize, ref int totalItems)
        {
            return _indexManager.SearchSubscriptionAssets(searchObjects, languageId, useStartDate, mediaTypes, order,
                pageIndex, pageSize, ref totalItems);
        }

        public List<int> GetEntitledEpgLinearChannels(UnifiedSearchDefinitions definitions)
        {
            return _indexManager.GetEntitledEpgLinearChannels(definitions);
        }

        public bool DoesMediaBelongToChannels(List<int> channelIDs, int mediaId)
        {
            return _indexManager.DoesMediaBelongToChannels(channelIDs, mediaId);
        }

        public List<int> GetMediaChannels(int mediaId)
        {
            return _indexManager.GetMediaChannels(mediaId);
        }

        public List<string> GetEpgAutoCompleteList(EpgSearchObj epgSearchObj)
        {
            return _indexManager.GetEpgAutoCompleteList(epgSearchObj);
        }

        public List<SearchResult> GetAssetsUpdateDate(eObjectType assetType, List<int> assetIds)
        {
            return _indexManager.GetAssetsUpdateDate(assetType, assetIds);
        }

        public List<int> SearchChannels(ChannelSearchDefinitions definitions, ref int totalItems)
        {

            return _indexManager.SearchChannels(definitions, ref totalItems);
        }

        public List<TagValue> SearchTags(TagSearchDefinitions definitions, out int totalItems)
        {
            return _indexManager.SearchTags(definitions, out totalItems);
        }

        public SearchResultsObj SearchMedias(MediaSearchObj search, int langId, bool useStartDate)
        {
            return _indexManager.SearchMedias(search, langId, useStartDate);
        }

        public SearchResultsObj SearchSubscriptionMedias(List<MediaSearchObj> oSearch, int nLangID, bool shouldUseStartDate, string sMediaTypes,
            OrderObj orderObj, int nPageIndex, int nPageSize)
        {
            return _indexManager.SearchSubscriptionMedias(oSearch, nLangID, shouldUseStartDate, sMediaTypes,
                orderObj, nPageIndex, nPageSize);
        }
        #endregion
    }
}
