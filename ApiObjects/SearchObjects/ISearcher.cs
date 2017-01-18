using System.Collections.Generic;
using System.ServiceModel;
﻿using System;
using System.Linq;
using System.Text;

namespace ApiObjects.SearchObjects
{
    //[ServiceContract]
    //public interface ISearcher
    //{    
    //    [OperationContract]
    //    SearchResultsObj SearchMedias(int nGroupID, MediaSearchObj oSearch, int nLangID, bool bUseStartDate, int nIndex);

    //    [OperationContract]
    //    List<string> GetAutoCompleteList(int nGroupID, MediaSearchObj oSearch, int nLangID, ref int nTotalItems);

    //    [OperationContract]
    //    SearchResultsObj GetChannelsMedias(int nGroupID, int[] channels, string mediaTypes, int nUserTypeID, OrderObj oOrder, int pageIndex, int pageSize);

    //    [OperationContract]
    //    bool UpdateRecord(int nGroupID, int nMediaID);

    //    [OperationContract]
    //    bool UpdateChannel(int nGroupID, int nChannelID);

    //    [OperationContract]
    //    bool RemoveRecord(int nGroupID, int nMediaID);

    //    [OperationContract]
    //    bool ClearAndBuildGroup(int nGroupID);

    //    [OperationContract]
    //    SearchResultsObj SearchSubscriptionMedias(int nSubscriptionGroupId, List<MediaSearchObj> oSearch, int nLangID, bool bUseStartDate, string sMediaTypes, OrderObj oOrderObj, int nPageIndex, int nPageSize);

    //    [OperationContract]
    //    bool DoesMediaBelongToChannels(int nGroupID, List<int> lChannelIDs, int nMediaID);

    //    [OperationContract]
    //    List<ChannelContainObj> GetSubscriptionContainingMedia(List<ChannelContainSearchObj> oSearch);

    //    [OperationContract]
    //    SearchResult GetDoc(int nGroupID, int nMediaID);

    //    [OperationContract]
    //    List<int> GetMediaChannels(int nGroupID, int nMediaID);

    //    [OperationContract]
    //    SearchResultsObj SearchEpgs(EpgSearchObj epgSearch);

    //    [OperationContract]
    //    List<string> GetEpgAutoCompleteList(EpgSearchObj oSearch);

    //    [OperationContract]
    //    List<List<string>> GetChannelsDefinitions(List<List<long>> listsOfChannelIDs, long groupID);

    //    [OperationContract]
    //    Dictionary<long, bool> ValidateMediaIDsInChannels(int nGroupID, List<long> distinctMediaIDs,
    //        List<string> jsonizedChannelsDefinitionsMediasHaveToAppearInAtLeastOne,
    //        List<string> jsonizedChannelsDefinitionsMediasMustNotAppearInAll);

    //    [OperationContract]
    //    SearchResultsObj UnifiedSearch(UnifiedSearchDefinitions unifiedSearch);

    //}
}