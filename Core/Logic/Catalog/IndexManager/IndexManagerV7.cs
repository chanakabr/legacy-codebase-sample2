using System;
using System.Collections.Generic;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ApiObjects.Statistics;
using Catalog.Response;
using Core.Catalog.Response;
using GroupsCacheManager;
using Polly.Retry;

namespace Core.Catalog
{
    public class IndexManagerV7:IIndexManager
    {
        public bool UpsertMedia(long assetId)
        {
            throw new NotImplementedException();
        }

        public string SetupEpgV2Index(DateTime dateOfProgramsToIngest, IDictionary<string, LanguageObj> languages, LanguageObj _defaultLanguage,
            RetryPolicy retryPolicy)
        {
            throw new NotImplementedException();
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

        public List<int> GetMediaChannels(int nMediaID)
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
            throw new NotImplementedException();
        }

        public bool InsertSocialStatisticsData(SocialActionStatistics action)
        {
            throw new NotImplementedException();
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

        public string SetupEpgIndex(IEnumerable<LanguageObj> languages, LanguageObj defaultLanguage, bool isRecording)
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

        public bool UpdateEpgs(List<LanguageObj> languages, List<EpgCB> epgObjects, Dictionary<string, LinearChannelSettings> linearChannelSettings, bool isRecording,
            Dictionary<long, long> epgToRecordingMapping = null)
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
    }
}