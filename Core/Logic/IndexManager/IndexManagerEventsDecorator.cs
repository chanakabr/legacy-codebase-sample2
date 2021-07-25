using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
using Couchbase.Utils;
using EventBus.Abstraction;
using TVinciShared;
using Channel = GroupsCacheManager.Channel;

namespace ApiLogic.Catalog.IndexManager
{
    
    public class IndexManagerEventsDecorator : IIndexManager
    {
        private readonly IIndexManager _indexManager;
        private readonly IEventBusPublisher _eventBusPublisher;
        private readonly IndexManagerVersion _indexManagerVersion;
        private readonly int _partnerId;
        private readonly Type _indexManagerType;

        public IndexManagerEventsDecorator(IIndexManager indexManager,
            IEventBusPublisher eventBusPublisher,
            IndexManagerVersion indexManagerVersion,
            int partnerId)
        {
            _indexManager = indexManager;
            _eventBusPublisher = eventBusPublisher;
            _indexManagerVersion = indexManagerVersion;
            _indexManagerType = _indexManager.GetType();
            _partnerId = partnerId;
        }

        #region Execute Decoration

        
        private T Execute<T>(MethodBase methodBase,string eventKey, params object[] methodParameters)
        {
            var methodBaseName = methodBase.Name;
            var methodInfo = _indexManagerType.GetMethod(methodBaseName);
            var result = (T)methodInfo.Invoke(_indexManager, methodParameters);
            CallMigrateEvent(methodBaseName,eventKey,methodParameters);
            return result;
        } 
        
        private void Execute(MethodBase methodBase,string eventKey,params object[] methodParameters)
        {
            var methodBaseName = methodBase.Name;
            var methodInfo = _indexManagerType.GetMethod(methodBaseName);
            methodInfo.Invoke(_indexManager, methodParameters);
            CallMigrateEvent(methodBaseName,eventKey,methodParameters);
        } 
        
        private void CallMigrateEvent(string methodName,string eventKey, params object[] methodParameters)
        {
            var migrationEvent = new ApiObjects.DataMigrationEvents.ElasticSearchMigrationEvent(_indexManagerVersion.ToString(),_partnerId)
            {
                MethodName = methodName,
                Parameters = methodParameters,
                EventKey = eventKey
            };
            
            _eventBusPublisher.Publish(migrationEvent);
        }

        #endregion

        #region CUD
        
        //CUD
        public bool UpsertMedia(long assetId)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),IndexManagerMigrationEventKeys.MEDIA,assetId);
        }
        
        //CUD
        public bool DeleteProgram(List<long> epgIds, IEnumerable<string> epgChannelIds)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),IndexManagerMigrationEventKeys.EPG, epgIds,  epgChannelIds);
        }

        //CUD
        public bool UpsertProgram(List<EpgCB> epgObjects, Dictionary<string, LinearChannelSettings> linearChannelSettings)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),IndexManagerMigrationEventKeys.EPG, epgObjects, linearChannelSettings);
        }

        //CUD
        public bool DeleteChannel(int channelId)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),IndexManagerMigrationEventKeys.CHANNEL, channelId);
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
            return Execute<bool>(MethodBase.GetCurrentMethod(),IndexManagerMigrationEventKeys.MEDIA, assetId); 
        }
        
        //CUD
        public void UpsertProgramsToDraftIndex(IList<EpgProgramBulkUploadObject> calculatedPrograms, string draftIndexName, DateTime dateOfProgramsToIngest,
            LanguageObj defaultLanguage, IDictionary<string, LanguageObj> languages)
        {
            Execute(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.EPG,
                calculatedPrograms, draftIndexName, dateOfProgramsToIngest, defaultLanguage, languages);
        }

        //CUD
        public void DeleteProgramsFromIndex(IList<EpgProgramBulkUploadObject> programsToDelete, string epgIndexName, IDictionary<string, LanguageObj> languages)
        {
            Execute(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.EPG, programsToDelete, epgIndexName,
                languages);
        }
        
        //CUD
        public bool DeleteChannelPercolator( List<int> channelIds)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),IndexManagerMigrationEventKeys.CHANNEL,  channelIds);
        }

        //CUD
        public bool UpdateChannelPercolator(  List<int> channelIds, Channel channel = null)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.CHANNEL, channelIds, channel);
        } 

        //CUD
        public Status UpdateTag(TagValue tag)
        {
            return Execute<Status>(MethodBase.GetCurrentMethod(),IndexManagerMigrationEventKeys.TAG, tag);
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
        public Status DeleteStatistics(DateTime until)
        {
            return Execute<Status>(MethodBase.GetCurrentMethod(),IndexManagerMigrationEventKeys.STATS, until);
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
            return Execute<string>(MethodBase.GetCurrentMethod(),IndexManagerMigrationEventKeys.IP_TO_COUNTRY);
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
        public string SetupMediaIndex(List<LanguageObj> languages, LanguageObj defaultLanguage)
        {
            return Execute<string>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.MEDIA, languages,
                defaultLanguage);
        }
        
        //CUD
        public bool AddChannelsPercolatorsToIndex(HashSet<int> channelIds, string newIndexName, bool shouldCleanupInvalidChannels = false)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.CHANNEL, channelIds, newIndexName, shouldCleanupInvalidChannels);
        }

        //CUD
        public string SetupChannelMetadataIndex()
        {
            return Execute<string>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.CHANNEL_METADATA);
        }

        //CUD
        public void AddChannelsMetadataToIndex(string newIndexName, List<Channel> allChannels)
        {
            Execute(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.CHANNEL_METADATA,newIndexName, allChannels);
        }

        //CUD
        public void PublishChannelsMetadataIndex(string newIndexName, bool shouldSwitchAlias, bool shouldDeleteOldIndices)
        {
            Execute(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.CHANNEL_METADATA,newIndexName, shouldSwitchAlias, shouldDeleteOldIndices);
        }

        //CUD
        public string SetupTagsIndex()
        {
            return Execute<string>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.TAG);
        }

        //CUD
        public void AddTagsToIndex(string newIndexName, List<TagValue> allTagValues)
        {
            Execute(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.TAG,
                newIndexName, allTagValues);
        }

        //CUD
        public bool PublishTagsIndex(string newIndexName, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.TAG,
                newIndexName, shouldSwitchIndexAlias, shouldDeleteOldIndices);
        }

        //CUD
        public void InsertMedias(Dictionary<int, Dictionary<int, Media>> groupMedias, string newIndexName)
        {
            Execute(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.MEDIA,
                groupMedias, newIndexName);
        }

        //CUD
        public void PublishMediaIndex(string newIndexName, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
        {
            Execute(MethodBase.GetCurrentMethod(), IndexManagerMigrationEventKeys.MEDIA,
                newIndexName, shouldSwitchIndexAlias, shouldDeleteOldIndices);
        }
        
        //CUD
        public string SetupEpgIndex(  IEnumerable<LanguageObj> languages,LanguageObj defaultLanguage, bool isRecording )
        {
            var eventKey = isRecording ? IndexManagerMigrationEventKeys.RECORDING : IndexManagerMigrationEventKeys.EPG;
            return Execute<string>(MethodBase.GetCurrentMethod(), eventKey, languages,
                defaultLanguage, isRecording);
        }

        //CUD
        public void AddEPGsToIndex(string index, bool isRecording, Dictionary<ulong, Dictionary<string, EpgCB>> programs,
            Dictionary<long, List<int>> linearChannelsRegionsMapping,
            Dictionary<long, long> epgToRecordingMapping)
        {
            var eventKey = isRecording ? IndexManagerMigrationEventKeys.RECORDING : IndexManagerMigrationEventKeys.EPG;
            Execute(MethodBase.GetCurrentMethod(),eventKey,
                index,
                isRecording,
                programs,
                linearChannelsRegionsMapping,
                epgToRecordingMapping);
        }

        //CUD
        public bool FinishUpEpgIndex(string newIndexName, bool isRecording, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices)
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
        
        //CUD
        public string SetupEpgV2Index(DateTime dateOfProgramsToIngest, IDictionary<string, LanguageObj> languages,
            LanguageObj defaultLanguage,
            RetryPolicy retryPolicy)
        {
            return Execute<string>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.EPG, dateOfProgramsToIngest, languages, defaultLanguage, retryPolicy);
        }

        //CUD
        public bool SetupSocialStatisticsDataIndex()
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),IndexManagerMigrationEventKeys.STATS);
        }
        
        public bool InsertSocialStatisticsData(SocialActionStatistics action)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),IndexManagerMigrationEventKeys.STATS,action);
        }
        
        
        //CUD
        public bool FinalizeEpgV2Index(DateTime date)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),IndexManagerMigrationEventKeys.EPG,date);
        }

        //CUD
        public bool FinalizeEpgV2Indices(List<DateTime> date, RetryPolicy retryPolicy)
        {
            return Execute<bool>(MethodBase.GetCurrentMethod(),
                IndexManagerMigrationEventKeys.EPG, date, retryPolicy);
        }

        #endregion
        
        #region READ
        public SearchResultsObj SearchEpgs(EpgSearchObj epgSearch)
        {
            return _indexManager.SearchEpgs(epgSearch);
        }

        public List<string> GetAutoCompleteList(MediaSearchObj oSearch, int nLangID, ref int nTotalItems)
        {
            return _indexManager.GetAutoCompleteList(oSearch, nLangID, ref nTotalItems);
        }

        public Dictionary<long, bool> ValidateMediaIDsInChannels(List<long> distinctMediaIDs,
            List<string> jsonizedChannelsDefinitionsMediasHaveToAppearInAtLeastOne,
            List<string> jsonizedChannelsDefinitionsMediasMustNotAppearInAll)
        {
            return _indexManager.ValidateMediaIDsInChannels(distinctMediaIDs,
                jsonizedChannelsDefinitionsMediasHaveToAppearInAtLeastOne,
                jsonizedChannelsDefinitionsMediasMustNotAppearInAll);
        }
        
        public Country GetCountryByCountryName(string countryName)
        {
            return _indexManager.GetCountryByCountryName(countryName);
        }

        public Country GetCountryByIp(string ip, out bool searchSuccess)
        {
            return _indexManager.GetCountryByIp(ip,out searchSuccess);
        }

        public List<string> GetChannelPrograms(int channelId, DateTime startDate, DateTime endDate, List<ESOrderObj> esOrderObjs)
        {
            return _indexManager.GetChannelPrograms(channelId, startDate, endDate, esOrderObjs);
        }
        
        public List<string> GetEpgCBDocumentIdsByEpgId(IEnumerable<long> epgIds, IEnumerable<LanguageObj> langCodes)
        {
            return _indexManager.GetEpgCBDocumentIdsByEpgId( epgIds, langCodes);
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
            _indexManager.GetAssetStats(assetIDs, startDate, endDate, type,ref assetIDsToStatsMapping);
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
        
        public List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearch, ref int totalItems, ref int to)
        {
            return  _indexManager.UnifiedSearch(unifiedSearch, ref totalItems, ref to);
        }

        public List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearch,
            ref int totalItems,
            ref int to,
            out List<AggregationsResult> aggregationsResult)
        {
            return _indexManager.UnifiedSearch(unifiedSearch, ref totalItems, ref to, out aggregationsResult);
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

        public bool DoesMediaBelongToChannels( List<int> lChannelIDs, int nMediaID)
        {
            return _indexManager.DoesMediaBelongToChannels( lChannelIDs, nMediaID);
        }

        public List<int> GetMediaChannels(  int mediaId)
        {
            return _indexManager.GetMediaChannels( mediaId);
        }

        public List<string> GetEpgAutoCompleteList(EpgSearchObj oSearch)
        {
            return _indexManager.GetEpgAutoCompleteList(oSearch);
        }

        public List<List<string>> GetChannelsDefinitions(List<List<long>> listsOfChannelIDs)
        {
            return _indexManager.GetChannelsDefinitions(listsOfChannelIDs);
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
        
        public SearchResultsObj SearchMedias(MediaSearchObj oSearch, int nLangID, bool bUseStartDate)
        {
            return _indexManager.SearchMedias(oSearch, nLangID, bUseStartDate);
        }
        
        public SearchResultsObj SearchSubscriptionMedias(List<MediaSearchObj> oSearch, int nLangID, bool shouldUseStartDate, string sMediaTypes,
            OrderObj oOrderObj, int nPageIndex, int nPageSize)
        {
            return _indexManager.SearchSubscriptionMedias(oSearch, nLangID, shouldUseStartDate, sMediaTypes,
                oOrderObj, nPageIndex, nPageSize);
        }
        #endregion
    }
}