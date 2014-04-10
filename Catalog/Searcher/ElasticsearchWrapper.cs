using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using ElasticSearch.Searcher;
using ElasticSearch.Common;
using ApiObjects;
using System.Collections;
using System.Collections.Concurrent;
using Logger;

namespace Catalog
{
    public class ElasticsearchWrapper : ISearcher
    {
        private static readonly string DATE_FORMAT = "yyyyMMddHHmmss";
        private static readonly string INDEX_DATE_FORMAT = "yyyyMMdd";

        public static readonly string ES_BASE_ADDRESS = Utils.GetWSURL("ES_URL");
        public const int STATUS_OK = 200;
        protected const string ES_MEDIA_TYPE = "media";
        protected const string ES_EPG_TYPE = "epg";
        protected ElasticSearchApi m_oESApi;

        public ElasticsearchWrapper()
        {
            m_oESApi = new ElasticSearchApi();
        }

        public SearchResultsObj SearchMedias(int nGroupID, MediaSearchObj oSearch, int nLangID, bool bUseStartDate)
        {
            SearchResultsObj oRes = new SearchResultsObj();

            Group oGroup = GroupsCache.Instance.GetGroup(nGroupID);

            if (oGroup == null)
                return oRes;

            int nParentGroupID = oGroup.m_nParentGroupID;

            ESMediaQueryBuilder queryParser = new ESMediaQueryBuilder(nGroupID, oSearch);


            int nPageIndex = 0;
            int nPageSize = 0;
            if ((oSearch.m_oOrder.m_eOrderBy <= ApiObjects.SearchObjects.OrderBy.VIEWS && oSearch.m_oOrder.m_eOrderBy >= ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER)
                          || oSearch.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.VOTES_COUNT))
            {
                nPageIndex = oSearch.m_nPageIndex;
                nPageSize = oSearch.m_nPageSize;
                queryParser.PageIndex = 0;
                queryParser.PageSize = 0;
            }
            else
            {
                queryParser.PageIndex = oSearch.m_nPageIndex;
                queryParser.PageSize = oSearch.m_nPageSize;
            }

            queryParser.QueryType = (oSearch.m_bExact) ? eQueryType.EXACT : eQueryType.BOOLEAN;

            string sQuery = queryParser.BuildSearchQueryString();

            if (!string.IsNullOrEmpty(sQuery))
            {
                int nStatus = 0;

                string sType = Utils.GetESTypeByLanguage(ES_MEDIA_TYPE, oSearch.m_oLangauge);
                string sUrl = string.Format("{0}/{1}/{2}/_search", ES_BASE_ADDRESS, nParentGroupID, sType);

                string retObj = m_oESApi.SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, sQuery);

                if (nStatus == STATUS_OK)
                {
                    int nTotalItems = 0;
                    List<ElasticSearchApi.ESAssetDocument> lMediaDocs = DecodeAssetSearchJsonObject(retObj, ref nTotalItems);
                    if (lMediaDocs != null && lMediaDocs.Count > 0)
                    {
                        oRes.m_resultIDs = new List<SearchResult>();
                        oRes.n_TotalItems = nTotalItems;

                        foreach (ElasticSearchApi.ESAssetDocument doc in lMediaDocs)
                        {
                            oRes.m_resultIDs.Add(new SearchResult() { assetID = doc.asset_id, UpdateDate = doc.cache_date });
                        }

                        if ((oSearch.m_oOrder.m_eOrderBy <= ApiObjects.SearchObjects.OrderBy.VIEWS && oSearch.m_oOrder.m_eOrderBy >= ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER)
                            || oSearch.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.VOTES_COUNT))
                        {
                            List<int> lMediaIds = oRes.m_resultIDs.Select(item => item.assetID).ToList();

                            Utils.OrderMediasByStats(lMediaIds, (int)oSearch.m_oOrder.m_eOrderBy, (int)oSearch.m_oOrder.m_eOrderDir);

                            Dictionary<int, SearchResult> dItems = oRes.m_resultIDs.ToDictionary(item => item.assetID);
                            oRes.m_resultIDs.Clear();

                            int nValidNumberOfMediasRange = nPageSize;
                            if (Utils.ValidatePageSizeAndPageIndexAgainstNumberOfMedias(lMediaIds.Count, nPageIndex, ref nValidNumberOfMediasRange))
                            {
                                if (nValidNumberOfMediasRange > 0)
                                {
                                    lMediaIds = lMediaIds.GetRange(nPageSize * nPageIndex, nValidNumberOfMediasRange);
                                }
                            }

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

            return oRes;
        }

        public List<string> GetAutoCompleteList(int nGroupID, MediaSearchObj oSearch, int nLangID, ref int nTotalItems)
        {
            List<string> lRes = new List<string>();

            Group oGroup = GroupsCache.Instance.GetGroup(nGroupID);

            if (oGroup == null || oSearch == null)
                return lRes;

            int nParentGroupID = oGroup.m_nParentGroupID;

            oSearch.m_dOr.Add(new SearchValue() { m_lValue = new List<string>() { "" }, m_sKey = "name^3" });

            ESMediaQueryBuilder queryParser = new ESMediaQueryBuilder(nGroupID, oSearch);
            queryParser.PageIndex = oSearch.m_nPageIndex;
            queryParser.PageSize = oSearch.m_nPageSize;

            queryParser.QueryType = eQueryType.PHRASE_PREFIX;

            string sQuery = queryParser.BuildMediaAutoCompleteQuery();

            if (!string.IsNullOrEmpty(sQuery))
            {
                string sType = Utils.GetESTypeByLanguage(ES_MEDIA_TYPE, oSearch.m_oLangauge);
                string retObj = m_oESApi.Search(nParentGroupID.ToString(), sType, ref sQuery);

                List<ElasticSearchApi.ESAssetDocument> lMediaDocs = DecodeAssetSearchJsonObject(retObj, ref nTotalItems);
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

        public SearchResultsObj GetChannelsMedias(int nGroupID, int[] channels, string mediaTypes, int nUserTypeID, OrderObj oOrder, int pageIndex, int pageSize)
        {

            return null;
        }

        public bool UpdateRecord(int nGroupID, int nMediaID)
        {

            return false;
        }

        public bool UpdateChannel(int nGroupID, int nChannelID)
        {
            return false;
        }

        public bool RemoveRecord(int nGroupID, int nMediaID)
        {
            return false;
        }

        public bool ClearAndBuildGroup(int nGroupID)
        {
            return false;
        }

        public SearchResultsObj SearchSubscriptionMedias(int nSubscriptionGroupId, List<MediaSearchObj> oSearch, int nLangID, bool bUseStartDate, string sMediaTypes, ApiObjects.SearchObjects.OrderObj oOrderObj, int nPageIndex, int nPageSize)
        {
            Logger.Logger.Log("Info", "Started SearchSubscriptionMedias", "Elasticsearch");
            DateTime dtStart = DateTime.Now;
            SearchResultsObj lSortedMedias = new SearchResultsObj();
            Group oGroup = GroupsCache.Instance.GetGroup(nSubscriptionGroupId);

            int nTotalItems = 0;

            if (oGroup == null)
                return lSortedMedias;

            int nParentGroupID = oGroup.m_nParentGroupID;

            if (oSearch != null && oSearch.Count > 0)
            {
                List<ElasticSearchApi.ESAssetDocument> lSearchResults = new List<ElasticSearchApi.ESAssetDocument>();

                List<string> searchQueries = new List<string>();

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

                tempQuery = new FilteredQuery() { PageIndex = nPageIndex, PageSize = nPageSize };
                tempQuery.ESSort.Add(new ESOrderObj() { m_eOrderDir = oOrderObj.m_eOrderDir, m_sOrderValue = FilteredQuery.GetESSortValue(oOrderObj) });
                tempQuery.Filter = new QueryFilter() { FilterSettings = groupedFilters };

                string sSearchQuery = tempQuery.ToString();



                string sRetVal = m_oESApi.Search(oGroup.m_nParentGroupID.ToString(), ES_MEDIA_TYPE, ref sSearchQuery);

                lSearchResults = DecodeAssetSearchJsonObject(sRetVal, ref nTotalItems);

                if (lSearchResults != null && lSearchResults.Count > 0)
                {
                    Logger.Logger.Log("Info", "SearchSubscriptionMedias returned search results", "Elasticsearch");
                    lSortedMedias.m_resultIDs = new List<SearchResult>();

                    lSortedMedias.n_TotalItems = nTotalItems;


                    if ((oOrderObj.m_eOrderBy <= ApiObjects.SearchObjects.OrderBy.VIEWS && oOrderObj.m_eOrderBy >= ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER) || oOrderObj.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.VOTES_COUNT))
                    {
                        List<int> lIds = lSearchResults.Select(item => item.asset_id).ToList();
                        Utils.OrderMediasByStats(lIds, (int)oOrderObj.m_eOrderBy, (int)oOrderObj.m_eOrderDir);

                        Dictionary<int, ElasticSearchApi.ESAssetDocument> dItems = lSearchResults.ToDictionary(item => item.asset_id);

                        ElasticSearchApi.ESAssetDocument oTemp;
                        foreach (int mediaID in lIds)
                        {
                            if (dItems.TryGetValue(mediaID, out oTemp))
                            {
                                lSortedMedias.m_resultIDs.Add(new SearchResult() { assetID = oTemp.asset_id, UpdateDate = oTemp.cache_date });
                            }
                        }
                    }
                    else
                    {
                        lSortedMedias.m_resultIDs = lSearchResults.Select(item => new SearchResult() { assetID = item.asset_id, UpdateDate = item.cache_date }).ToList();
                    }
                }
            }
            DateTime dtEnd = DateTime.Now;

            double totalMilli = (dtEnd - dtStart).TotalMilliseconds;
            Logger.Logger.Log("Info", "SearchSubscriptionMedias took " + totalMilli + " milliseconds", "Elasticsearch");
            return lSortedMedias;
        }

        public bool DoesMediaBelongToChannels(int nGroupID, List<int> lChannelIDs, int nMediaID)
        {
            bool bResult = false;

            if (lChannelIDs == null || lChannelIDs.Count < 1)
                return bResult;

            List<int> lChannelsFound = GetMediaChannels(nGroupID, nMediaID);

            if (lChannelsFound != null && lChannelsFound.Count > 0)
            {
                foreach (int channelId in lChannelsFound)
                {
                    if (lChannelIDs.Contains(channelId))
                    {
                        bResult = true;
                        break;
                    }
                }
            }

            return bResult;
        }


        public List<ChannelContainObj> GetSubscriptionContainingMedia(List<ChannelContainSearchObj> oSearch)
        {
            List<ChannelContainObj> oRes = null;

            ChannelContainSearchObj tempObj = oSearch.First();

            if (tempObj != null && tempObj.m_oSearchObj != null)
            {
                List<int> nChannels = GetMediaChannels(tempObj.m_oSearchObj.m_nGroupId, tempObj.m_oSearchObj.m_nMediaID);

                if (nChannels != null && nChannels.Count > 0)
                {
                    Dictionary<int, int> dChannels = nChannels.ToDictionary<int, int>(item => item);

                    oRes = new List<ChannelContainObj>();

                    ChannelContainObj tempRes;
                    foreach (ChannelContainSearchObj searchObj in oSearch)
                    {
                        tempRes = new ChannelContainObj() { m_nChannelID = searchObj.m_nChannelID };
                        tempRes.m_bContain = (dChannels.ContainsKey(searchObj.m_nChannelID)) ? true : false;
                    }
                }

            }
            return oRes;
        }


        public List<int> GetMediaChannels(int nGroupID, int nMediaID)
        {
            List<int> lResult = null;

            Group oGroup = GroupsCache.Instance.GetGroup(nGroupID);

            if (oGroup == null)
                return lResult;

            string sIndex = oGroup.m_nParentGroupID.ToString();

            string sMediaDoc = m_oESApi.GetDoc(sIndex, ES_MEDIA_TYPE, nMediaID.ToString());

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
                    List<string> lRetVal = m_oESApi.SearchPercolator(sIndex, ES_MEDIA_TYPE, ref sMediaDoc);

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
                    Logger.Logger.Log("Error", string.Format("GetMediaChannels - Could not parse response. Ex={0}", ex.Message), "ElasticSearch");
                }
            }

            return lResult;
        }

        public SearchResult GetDoc(int nParentGroupID, int nMediaID)
        {
            SearchResult oResult = new SearchResult();

            string sRetVal = m_oESApi.GetDoc(nParentGroupID.ToString(), ES_MEDIA_TYPE, nMediaID.ToString());

            if (!string.IsNullOrEmpty(sRetVal))
            {
                ElasticSearchApi.ESAssetDocument mediaDoc = DecodeSingleJsonObject(sRetVal);
                if (mediaDoc != null)
                {
                    oResult.assetID = mediaDoc.asset_id;
                    oResult.UpdateDate = mediaDoc.cache_date;
                }
            }
            return oResult;
        }

        public virtual SearchResultsObj SearchEpgs(EpgSearchObj epgSearch)
        {
            SearchResultsObj epgResponse = null;
            try
            {
                if (epgSearch == null || epgSearch.m_nGroupID == 0)
                {
                    Logger.Logger.Log("Info", "SearchEpgs return null due to epgSearch == null || epgSearch.m_nGroupID==0 ", "ElasticSearch");
                    return null;
                }
                Logger.Logger.Log("Info", string.Format("SearchEpgs GroupID={0}, between Dates {1}- {2}", epgSearch.m_nGroupID, epgSearch.m_dStartDate.ToShortDateString(), epgSearch.m_dEndDate.ToShortDateString()), "ElasticSearch");

                DateTime startDate = epgSearch.m_dStartDate;
                DateTime endDate = epgSearch.m_dEndDate;

                Group group = GroupsCache.Instance.GetGroup(epgSearch.m_nGroupID);

                if (group != null)
                {
                    ESEpgQueryBuilder epgQueryBuilder = new ESEpgQueryBuilder() { m_oEpgSearchObj = epgSearch, bAnalyzeWildcards = true };
                    string sQuery = epgQueryBuilder.BuildSearchQueryString();

                    DateTime dTempDate = epgSearch.m_dStartDate.AddDays(-1);
                    dTempDate = new DateTime(dTempDate.Year, dTempDate.Month, dTempDate.Day);

                    List<string> lRouting = new List<string>();

                    while (dTempDate <= epgSearch.m_dEndDate)
                    {
                        lRouting.Add(dTempDate.ToString("yyyyMMdd"));
                        dTempDate = dTempDate.AddDays(1);
                    }

                    string sGroupAlias = string.Format("{0}_epg", group.m_nParentGroupID);
                    string searchRes = m_oESApi.Search(sGroupAlias, ES_EPG_TYPE, ref sQuery, lRouting);

                    int nTotalRecords = 0;
                    List<ElasticSearchApi.ESAssetDocument> lDocs = DecodeEpgSearchJsonObject(searchRes, ref nTotalRecords);

                    if (lDocs != null)
                    {
                        epgResponse = new SearchResultsObj();
                        epgResponse.m_resultIDs = lDocs.Select(doc => new SearchResult { assetID = doc.asset_id, UpdateDate = doc.cache_date }).ToList();
                        epgResponse.n_TotalItems = nTotalRecords;
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("SearchEpgs ex={0}", ex.Message), "ElasticSearch");
            }

            return epgResponse;
        }

        private List<string> GetEpgExistingAliases(int nGroupID, DateTime dStartDate, DateTime dEndDate)
        {
            List<string> lAliasResult = new List<string>();


            List<string> lDateAliases = new List<string>();
            while (dStartDate <= dEndDate)
            {
                lDateAliases.Add(string.Format("{0}_epg_{1}", nGroupID, dStartDate.ToString(INDEX_DATE_FORMAT)));

                dStartDate = dStartDate.AddDays(1);
            }

            if (lDateAliases.Count > 0)
            {
                Task<string>[] tAliasRequests = new Task<string>[lDateAliases.Count];

                for (int i = 0; i < lDateAliases.Count; i++)
                {
                    tAliasRequests[i] = Task.Factory.StartNew<string>(
                         (index) =>
                         {
                             string sIndex = lDateAliases[(int)index];
                             return (m_oESApi.IndexExists(sIndex)) ? sIndex : string.Empty;
                         }, i);
                }
                Task.WaitAll(tAliasRequests);

                foreach (Task<string> task in tAliasRequests)
                {
                    if (!string.IsNullOrEmpty(task.Result))
                    {
                        lAliasResult.Add(task.Result);
                    }
                }
            }

            return lAliasResult;
        }

        public List<string> GetEpgAutoCompleteList(EpgSearchObj oSearch)
        {
            Logger.Logger.Log("Info", string.Format("{0}:{1}", "Auto Complete", oSearch.m_nGroupID.ToString()), "ElasticSearch");

            List<string> resultFinalList = null;

            Group group = GroupsCache.Instance.GetGroup(oSearch.m_nGroupID);

            if (group == null)
            {
                Logger.Logger.Log("Info", string.Format("Auto Complete :  Group is empty {0} , Between the dates{1}-{2}", oSearch.m_nGroupID, oSearch.m_dStartDate, oSearch.m_dEndDate), "ElasticSearch");
                return null;
            }

            List<string> lRouting = new List<string>();

            DateTime dTempDate = oSearch.m_dStartDate;
            while (dTempDate <= oSearch.m_dEndDate)
            {
                lRouting.Add(dTempDate.ToString("yyyyMMdd"));
                dTempDate = dTempDate.AddDays(1);
            }


            ESEpgQueryBuilder queryBuilder = new ESEpgQueryBuilder() { m_oEpgSearchObj = oSearch };
            string sQuery = queryBuilder.BuildEpgAutoCompleteQuery();


            string sGroupAlias = string.Format("{0}_epg", group.m_nParentGroupID);
            string searchRes = m_oESApi.Search(sGroupAlias, ES_EPG_TYPE, ref sQuery, lRouting);

            int nTotalRecords = 0;
            List<ElasticSearchApi.ESAssetDocument> lDocs = DecodeEpgSearchJsonObject(searchRes, ref nTotalRecords);

            if (lDocs != null)
            {
                resultFinalList = lDocs.Select(doc => doc.name).ToList();
                resultFinalList = resultFinalList.Distinct().OrderBy(q => q).ToList<string>();
            }


            return resultFinalList;
        }

        protected ElasticSearchApi.ESAssetDocument DecodeSingleJsonObject(string sObj)
        {
            ElasticSearchApi.ESAssetDocument doc = null;

            try
            {
                var jsonObj = JObject.Parse(sObj);

                if (jsonObj != null)
                {
                    doc = new ElasticSearchApi.ESAssetDocument();
                    JToken tempToken;

                    doc.id = ((tempToken = jsonObj.SelectToken("_id")) == null ? string.Empty : (string)tempToken);
                    doc.index = ((tempToken = jsonObj.SelectToken("_index")) == null ? string.Empty : (string)tempToken);
                    doc.type = ((tempToken = jsonObj.SelectToken("_type")) == null ? string.Empty : (string)tempToken);
                    doc.asset_id = ((tempToken = jsonObj.SelectToken("_source.media_id")) == null ? 0 : (int)tempToken);
                    doc.group_id = ((tempToken = jsonObj.SelectToken("_source.group_id")) == null ? 0 : (int)tempToken);
                    doc.name = ((tempToken = jsonObj.SelectToken("_source.name")) == null ? string.Empty : (string)tempToken);
                    doc.cache_date = ((tempToken = jsonObj.SelectToken("_source.cache_date")) == null ? new DateTime(1970, 1, 1, 0, 0, 0) :
                                    DateTime.ParseExact((string)tempToken, DATE_FORMAT, null));
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("Json Deserialization failed for ElasticSearch request. Execption={0}", ex.Message), "Catalog");
                doc = null;
            }

            return doc;
        }

        private List<ElasticSearchApi.ESAssetDocument> DecodeMediaMultiSearchJsonObject(string sObj, ref int totalItems)
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
                        tempDocs = DecodeAssetSearchJsonObject(item.ToString(), ref tempTotal);
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
                Logger.Logger.Log("Error", string.Format("Json Deserialization failed for ElasticSearch Media request. Execption={0}", ex.Message), "Catalog");
            }

            return documents;
        }

        private List<ElasticSearchApi.ESAssetDocument> DecodeAssetSearchJsonObject(string sObj, ref int totalItems)
        {
            List<ElasticSearchApi.ESAssetDocument> documents = null;
            try
            {
                var jsonObj = JObject.Parse(sObj);

                if (jsonObj != null)
                {
                    JToken tempToken;
                    totalItems = ((tempToken = jsonObj.SelectToken("hits.total")) == null ? 0 : (int)tempToken);
                    if (totalItems > 0)
                    {
                        documents = jsonObj.SelectToken("hits.hits").Select(item => new ElasticSearchApi.ESAssetDocument()
                        {
                            id = ((tempToken = item.SelectToken("_id")) == null ? string.Empty : (string)tempToken),
                            index = ((tempToken = item.SelectToken("_index")) == null ? string.Empty : (string)tempToken),
                            //score = ((tempToken = item.SelectToken("_score")) == null ? 0.0 : (double)tempToken),
                            type = ((tempToken = item.SelectToken("_type")) == null ? string.Empty : (string)tempToken),
                            asset_id = ((tempToken = item.SelectToken("fields.media_id")) == null ? 0 : (int)tempToken),
                            group_id = ((tempToken = item.SelectToken("fields.group_id")) == null ? 0 : (int)tempToken),
                            name = ((tempToken = item.SelectToken("fields.name")) == null ? string.Empty : (string)tempToken),
                            cache_date = ((tempToken = item.SelectToken("fields.cache_date")) == null ? new DateTime(1970, 1, 1, 0, 0, 0) :
                                            DateTime.ParseExact((string)tempToken, DATE_FORMAT, null))
                        }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("Json Deserialization failed for ElasticSearch Media request. Execption={0}", ex.Message), "Catalog");
            }

            return documents;
        }


        private List<ElasticSearchApi.ESAssetDocument> DecodeEpgSearchJsonObject(string sObj, ref int totalItems)
        {
            List<ElasticSearchApi.ESAssetDocument> documents = null;
            try
            {
                var jsonObj = JObject.Parse(sObj);

                if (jsonObj != null)
                {
                    JToken tempToken;
                    totalItems = ((tempToken = jsonObj.SelectToken("hits.total")) == null ? 0 : (int)tempToken);
                    if (totalItems > 0)
                    {
                        documents = jsonObj.SelectToken("hits.hits").Select(item => new ElasticSearchApi.ESAssetDocument()
                        {
                            id = ((tempToken = item.SelectToken("_id")) == null ? string.Empty : (string)tempToken),
                            index = ((tempToken = item.SelectToken("_index")) == null ? string.Empty : (string)tempToken),
                            //score = ((tempToken = item.SelectToken("_score")) == null ? 0.0 : (double)tempToken),
                            type = ((tempToken = item.SelectToken("_type")) == null ? string.Empty : (string)tempToken),
                            asset_id = ((tempToken = item.SelectToken("fields.epg_id")) == null ? 0 : (int)tempToken),
                            group_id = ((tempToken = item.SelectToken("fields.group_id")) == null ? 0 : (int)tempToken),
                            name = ((tempToken = item.SelectToken("fields.name")) == null ? string.Empty : (string)tempToken),
                            cache_date = ((tempToken = item.SelectToken("fields.cache_date")) == null ? new DateTime(1970, 1, 1, 0, 0, 0) :
                                            DateTime.ParseExact((string)tempToken, DATE_FORMAT, null)),
                            epg_channel_id = ((tempToken = item.SelectToken("fields.epg_channel_id")) == null ? 0 : (int)tempToken),
                            start_date = ((tempToken = item.SelectToken("fields.start_date")) == null ? new DateTime(1970, 1, 1, 0, 0, 0) :
                                            DateTime.ParseExact((string)tempToken, DATE_FORMAT, null)),
                        }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("Json Deserialization failed for ElasticSearch Epg request. Execption={0}", ex.Message), "Catalog");
            }

            return documents;
        }

        public List<List<string>> GetChannelsDefinitions(List<List<long>> listsOfChannelIDs, long groupID)
        {
            if (listsOfChannelIDs != null && listsOfChannelIDs.Count > 0)
            {
                int length = listsOfChannelIDs.Count;
                List<string> indicesBehindAlias = m_oESApi.GetAliases(groupID + "");
                if (indicesBehindAlias != null && indicesBehindAlias.Count > 0)
                {
                    List<List<string>> res = new List<List<string>>(length);
                    for (int i = 0; i < length; i++)
                    {
                        string sESAnswer = m_oESApi.MultiGetIDs("_percolator", indicesBehindAlias[0], listsOfChannelIDs[i], listsOfChannelIDs[i].Count);
                        if (!string.IsNullOrEmpty(sESAnswer))
                        {
                            List<string> definitions = ExtractChannelsDefinitionsOutOfResultJSON(sESAnswer);
                            res.Add(definitions);
                        }
                        else
                        {
                            res.Add(new List<string>(0));
                        }
                    }

                    return res;
                }
            }

            return new List<List<string>>(0);
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

        public Dictionary<long, bool> ValidateMediaIDsInChannels(int nGroupID, List<long> distinctMediaIDs,
                List<string> jsonizedChannelsDefinitionsMediasHaveToAppearInAtLeastOne,
                List<string> jsonizedChannelsDefinitionsMediasMustNotAppearInAll)
        {
            Dictionary<long, bool> res = null;
            if (distinctMediaIDs != null && distinctMediaIDs.Count > 0)
            {
                InitializeDictionary(distinctMediaIDs, ref res);
                MediaSearchObj searchObj = BuildSearchObjectForValidatingMediaIDsInChannels(distinctMediaIDs,
                    jsonizedChannelsDefinitionsMediasHaveToAppearInAtLeastOne,
                    jsonizedChannelsDefinitionsMediasMustNotAppearInAll);
                ESMediaQueryBuilder queryBuilder = new ESMediaQueryBuilder(nGroupID, searchObj);
                string sQuery = queryBuilder.GetDocumentsByIdsQuery(distinctMediaIDs, new OrderObj() { m_eOrderBy = ApiObjects.SearchObjects.OrderBy.ID });

                if (!string.IsNullOrEmpty(sQuery))
                {
                    string sESAnswer = m_oESApi.Search(nGroupID + "", ES_MEDIA_TYPE, ref sQuery);
                    if (sESAnswer.Length > 0)
                    {
                        int nTotalItems = 0;
                        List<ElasticSearchApi.ESAssetDocument> lMediaDocs = DecodeAssetSearchJsonObject(sESAnswer, ref nTotalItems);

                        UpdateDictionaryAccordingToESResults(lMediaDocs, ref res);

                        return res;
                    }
                }

            }

            return null;
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

    }

    class AssetDocCompare : IEqualityComparer<ElasticSearchApi.ESAssetDocument>
    {
        public bool Equals(ElasticSearchApi.ESAssetDocument x, ElasticSearchApi.ESAssetDocument y)
        {
            return x.asset_id == y.asset_id;
        }
        public int GetHashCode(ElasticSearchApi.ESAssetDocument codeh)
        {
            return codeh.asset_id.GetHashCode();
        }
    }
}
