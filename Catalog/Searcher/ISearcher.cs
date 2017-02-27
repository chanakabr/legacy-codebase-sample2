using System.Collections.Generic;
using System.ServiceModel;
﻿using System;
using System.Linq;
using System.Text;
using ApiObjects.SearchObjects;
using Catalog.Response;
using ApiObjects;

namespace Catalog
{
    [ServiceContract]
    public interface ISearcher
    {    
        [OperationContract]
        SearchResultsObj SearchMedias(int nGroupID, MediaSearchObj oSearch, int nLangID, bool bUseStartDate, int nIndex);

        [OperationContract]
        List<string> GetAutoCompleteList(int nGroupID, MediaSearchObj oSearch, int nLangID, ref int nTotalItems);

        [OperationContract]
        SearchResultsObj GetChannelsMedias(int nGroupID, int[] channels, string mediaTypes, int nUserTypeID, OrderObj oOrder, int pageIndex, int pageSize);

        [OperationContract]
        bool UpdateRecord(int nGroupID, int nMediaID);

        [OperationContract]
        bool UpdateChannel(int nGroupID, int nChannelID);

        [OperationContract]
        bool RemoveRecord(int nGroupID, int nMediaID);

        [OperationContract]
        bool ClearAndBuildGroup(int nGroupID);

        [OperationContract]
        SearchResultsObj SearchSubscriptionMedias(int nSubscriptionGroupId, List<MediaSearchObj> oSearch, int nLangID, bool bUseStartDate, string sMediaTypes, OrderObj oOrderObj, int nPageIndex, int nPageSize);

        [OperationContract]
        bool DoesMediaBelongToChannels(int nGroupID, List<int> lChannelIDs, int nMediaID);

        [OperationContract]
        List<ChannelContainObj> GetSubscriptionContainingMedia(List<ChannelContainSearchObj> oSearch);

        [OperationContract]
        SearchResult GetDoc(int nGroupID, int nMediaID);

        [OperationContract]
        List<int> GetMediaChannels(int nGroupID, int nMediaID);

        [OperationContract]
        SearchResultsObj SearchEpgs(EpgSearchObj epgSearch);

        [OperationContract]
        List<string> GetEpgAutoCompleteList(EpgSearchObj oSearch);

        [OperationContract]
        List<List<string>> GetChannelsDefinitions(List<List<long>> listsOfChannelIDs, long groupID);

        [OperationContract]
        Dictionary<long, bool> ValidateMediaIDsInChannels(int nGroupID, List<long> distinctMediaIDs,
            List<string> jsonizedChannelsDefinitionsMediasHaveToAppearInAtLeastOne,
            List<string> jsonizedChannelsDefinitionsMediasMustNotAppearInAll);

        [OperationContract]
        List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearch, ref int totalItems, ref int to);

        [OperationContract]
        List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearch, ref int totalItems, ref int to, out ElasticSearch.Searcher.ESAggregationsResult aggregationResult);

        [OperationContract]
        List<UnifiedSearchResult> FillUpdateDates(int groupId, List<UnifiedSearchResult> assets, ref int totalItems, int pageSize, int pageIndex);

        [OperationContract]
        List<UnifiedSearchResult> MultipleUnifiedSearch(int groupId, List<UnifiedSearchDefinitions> unifiedSearchDefinitions, ref int totalItems);

        [OperationContract]
        List<UnifiedSearchResult> SearchSubscriptionAssets(int subscriptionGroupId, List<BaseSearchObject> searchObjects, int languageId, bool useStartDate,
            string mediaTypes, ApiObjects.SearchObjects.OrderObj order, int pageIndex, int pageSize, ref int totalItems);

        [OperationContract]
        List<int> GetEntitledEpgLinearChannels(GroupsCacheManager.Group group, UnifiedSearchDefinitions definitions);
    }
}