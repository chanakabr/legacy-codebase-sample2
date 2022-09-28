using System.Collections.Generic;
using System;
using ApiObjects.SearchObjects;
using Core.Catalog.Response;
using ApiObjects;
using ApiObjects.Catalog;
using Catalog.Response;
using ApiObjects.Statistics;
using GroupsCacheManager;
using ApiObjects.BulkUpload;
using Core.Api;

namespace Core.Catalog
{
    //TODO: check if we can spend some time to remove ref and out vars in current methods 
    public interface IIndexManager
    {
        #region added from IndexManager
        bool UpsertMedia(long assetId);
        string SetupEpgV2Index(string indexNmae);
        void SetupEpgV3Index();
        void MigrateEpgToV3(int batchSize, EpgFeatureVersion originalEpgVersion);
        void RollbackEpgV3ToV2(int batchSize);
        void RollbackEpgV3ToV1(int batchSize);
        bool ForceRefreshEpgIndex(string indexName);
        bool FinalizeEpgV2Indices(List<DateTime> date);

        bool CompactEpgV2Indices(int futureIndexCompactionStart, int pastIndexCompactionStart);

        bool DeleteProgram(List<long> assetIds, bool isRecording = false);
        bool UpsertProgram(List<EpgCB> epgObjects, Dictionary<string, LinearChannelSettings> linearChannelSettings);
        bool DeleteChannelPercolator(List<int> channelIds);
        bool UpdateChannelPercolator(List<int> channelIds, Channel channel = null);
        bool DeleteChannel(int channelId);
        bool UpsertChannel(int channelId, Channel channel = null, long userId = 0);
        bool DeleteMedia(long assetId);
        void DeleteMediaByTypeAndFinalEndDate(long mediaTypeId, DateTime finalEndDate);
        
        #endregion

        #region added from the epg ingest v2

        void UpsertPrograms(IList<EpgProgramBulkUploadObject> calculatedPrograms, string draftIndexName, LanguageObj defaultLanguage, IDictionary<string, LanguageObj> languages);
        void DeletePrograms(IList<EpgProgramBulkUploadObject> programsToDelete, string epgIndexName,
            IDictionary<string, LanguageObj> languages);

        /// <summary>
        /// applies CRUD operations of epg to elasticsearch with transaction using parent child relation
        /// NOTE! it is expected that the items to update will contain the original in items to delete
        /// </summary>
        void ApplyEpgCrudOperationWithTransaction(string transactionId, List<EpgCB> programsToIndex, List<EpgCB> programsToDelete);

        void CommitEpgCrudTransaction(string transactionId, long linearChannelId);
        // ................................................................................................

        IList<EpgProgramBulkUploadObject> GetCurrentProgramsByDate(int channelId, DateTime fromDate, DateTime toDate);

        IList<EpgProgramInfo> GetCurrentProgramInfosByDate(int channelId, DateTime fromDate, DateTime toDate);

        #endregion
        
        // Unified Search
        // TODO: remove "ref int to" - not urgent...
        List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearch, ref int totalItems);
        List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearch, ref int totalItems, out List<AggregationsResult> aggregationsResult);
        AggregationsResult UnifiedSearchForGroupBy(UnifiedSearchDefinitions unifiedSearchDefinitions);
        List<UnifiedSearchResult> SearchSubscriptionAssets(List<BaseSearchObject> searchObjects, int languageId, bool useStartDate, string mediaTypes, ApiObjects.SearchObjects.OrderObj order, int pageIndex, int pageSize, ref int totalItems);
        List<int> GetEntitledEpgLinearChannels(UnifiedSearchDefinitions definitions);
        bool DoesMediaBelongToChannels(List<int> channelIDs, int mediaId);
        List<int> GetMediaChannels(int mediaId);

        List<string> GetEpgAutoCompleteList(EpgSearchObj epgSearchObj);

        // Method that were previous implemented and called directly from elasticWrapper instead of the interface

        List<SearchResult> GetAssetsUpdateDate(eObjectType assetType, List<int> assetIds);
        List<int> SearchChannels(ChannelSearchDefinitions definitions, ref int totalItems);

        // Tags CRUD
        List<TagValue> SearchTags(TagSearchDefinitions definitions, out int totalItems);
        ApiObjects.Response.Status UpdateTag(TagValue tag);
        ApiObjects.Response.Status DeleteTag(long tagId);
        ApiObjects.Response.Status DeleteTagsByTopic(long topicId);

        // external search (in use in phoenix, for example)
        List<UnifiedSearchResult> GetAssetsUpdateDates(List<UnifiedSearchResult> assets, ref int totalItems, int pageSize, int pageIndex, bool shouldIgnoreRecordings = false);

        // added from other places - Catalog logic for example

        void GetAssetStats(List<int> assetIDs, DateTime startDate,
            DateTime endDate, StatsType type, ref Dictionary<int, AssetStatsResult> assetIDsToStatsMapping);
        List<int> OrderMediaBySlidingWindow(ApiObjects.SearchObjects.OrderBy orderBy, bool isDesc, int pageSize, int PageIndex, List<int> media, DateTime windowTime);

        // added from ESStatisticsUtilities
        bool SetupSocialStatisticsDataIndex();

        bool InsertSocialStatisticsData(SocialActionStatistics action);

        bool DeleteSocialAction(StatisticsActionSearchObj socialSearch);

        // ip to country
        string SetupIPToCountryIndex();
        bool InsertDataToIPToCountryIndex(string newIndexName, 
            List<IPV4> ipV4ToCountryMapping, List<IPV6> ipV6ToCountryMapping);
        bool PublishIPToCountryIndex(string newIndexName);
        
        Country GetCountryByCountryName(string countryName);
        Country GetCountryByIp(string ip, out bool searchSuccess);

        // programs

        List<string> GetChannelPrograms(int channelId, DateTime startDate, DateTime endDate, List<ESOrderObj> esOrderObjs);
        List<string> GetEpgCBDocumentIdsByEpgId(IEnumerable<long> epgIds, IEnumerable<LanguageObj> languages);

        // rebuilders
        bool SetupMediaIndex(DateTime indexDate);
        bool SetupChannelPercolatorIndex(DateTime indexDate);

        void InsertMedias(Dictionary<int, Dictionary<int, Media>> groupMedias, DateTime indexDate);
        void PublishMediaIndex(DateTime indexDate, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices);
        void PublishChannelPercolatorIndex(DateTime indexDate, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices);

        bool AddChannelsPercolatorsToIndex(HashSet<int> channelIds, DateTime? indexDate, bool shouldCleanupInvalidChannels = false);

        string SetupChannelMetadataIndex(DateTime indexDate);
        void AddChannelsMetadataToIndex(string newIndexName, List<Channel> allChannels);
        void PublishChannelsMetadataIndex(string newIndexName, bool shouldSwitchAlias, bool shouldDeleteOldIndices);

        string SetupTagsIndex(DateTime indexDate);
        void InsertTagsToIndex(string newIndexName, List<ApiObjects.SearchObjects.TagValue> allTagValues);
        bool PublishTagsIndex(string newIndexName, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices);

        string SetupEpgIndex(DateTime indexDate, bool isRecording);

        void AddEPGsToIndex(string index, bool isRecording,
            Dictionary<ulong, Dictionary<string, EpgCB>> programs,
            Dictionary<long, List<int>> linearChannelsRegionsMapping,
            Dictionary<long, long> epgToRecordingMapping);
        bool PublishEpgIndex(string newIndexName, bool isRecording, bool shouldSwitchIndexAlias, bool shouldDeleteOldIndices);

        // updaters 
        bool UpdateEpgs(List<EpgCB> epgObjects, 
            bool isRecording, Dictionary<long, long> epgToRecordingMapping = null);

        bool UpdateEpgsPartial(EpgPartialUpdate[] epgs);

        // remark: Verify these methods should really be here ..

        SearchResultsObj SearchMedias(MediaSearchObj search, int langId, bool useStartDate);
        SearchResultsObj SearchSubscriptionMedias(List<MediaSearchObj> oSearch, int nLangID, bool shouldUseStartDate, string sMediaTypes, OrderObj orderObj, int nPageIndex, int nPageSize);
        SearchResultsObj SearchEpgs(EpgSearchObj epgSearch);
        List<string> GetAutoCompleteList(MediaSearchObj mediaSearch, int nLangID, ref int nTotalItems);
    }
}