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
using Catalog.Cache;
using GroupsCacheManager;
using Catalog.Response;
using KLogMonitor;
using System.Reflection;
using KlogMonitorHelper;

namespace Catalog
{
    public class ElasticsearchWrapper : ISearcher
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly string DATE_FORMAT = "yyyyMMddHHmmss";
        private static readonly string INDEX_DATE_FORMAT = "yyyyMMdd";

        public static readonly string ES_BASE_ADDRESS = Utils.GetWSURL("ES_URL");
        public const int STATUS_OK = 200;
        public const int STATUS_NOT_FOUND = 404;
        public const int STATUS_INTERNAL_ERROR = 500;

        protected const string ES_MEDIA_TYPE = "media";
        protected const string ES_EPG_TYPE = "epg";
        protected ElasticSearchApi m_oESApi;

        public ElasticsearchWrapper()
        {
            m_oESApi = new ElasticSearchApi();
        }

        public SearchResultsObj SearchMedias(int nGroupID, MediaSearchObj oSearch, int nLangID, bool bUseStartDate, int nIndex)
        {
            SearchResultsObj oRes = new SearchResultsObj();

            ESMediaQueryBuilder queryParser = new ESMediaQueryBuilder(nGroupID, oSearch);

            int nPageIndex = 0;
            int nPageSize = 0;
            if ((oSearch.m_oOrder.m_eOrderBy <= ApiObjects.SearchObjects.OrderBy.VIEWS && oSearch.m_oOrder.m_eOrderBy >= ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER)
                || oSearch.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.VOTES_COUNT)
                || oSearch.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.START_DATE))
            {
                nPageIndex = oSearch.m_nPageIndex;
                nPageSize = oSearch.m_nPageSize;
                queryParser.PageIndex = 0;
                queryParser.PageSize = 0;

                if (oSearch.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.START_DATE))
                {
                    queryParser.ReturnFields.Add("\"start_date\"");
                    queryParser.ReturnFields.Add("\"media_type_id\"");
                }
            }
            else
            {
                queryParser.PageIndex = oSearch.m_nPageIndex;
                queryParser.PageSize = oSearch.m_nPageSize;
            }

            queryParser.QueryType = (oSearch.m_bExact) ? eQueryType.EXACT : eQueryType.BOOLEAN;

            string sQuery = queryParser.BuildSearchQueryString(oSearch.m_bIgnoreDeviceRuleId, oSearch.m_bUseActive);

            if (!string.IsNullOrEmpty(sQuery))
            {
                int nStatus = 0;

                string sType = Utils.GetESTypeByLanguage(ES_MEDIA_TYPE, oSearch.m_oLangauge);
                string sUrl = string.Format("{0}/{1}/{2}/_search", ES_BASE_ADDRESS, nIndex, sType);

                string retObj = m_oESApi.SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, sQuery, true);

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
                            oRes.m_resultIDs.Add(new SearchResult()
                            {
                                assetID = doc.asset_id,
                                UpdateDate = doc.update_date
                            });
                        }

                        if ((oSearch.m_oOrder.m_eOrderBy <= ApiObjects.SearchObjects.OrderBy.VIEWS &&
                            oSearch.m_oOrder.m_eOrderBy >= ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER)
                            || oSearch.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.VOTES_COUNT)
                            || oSearch.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.START_DATE))
                        {
                            List<int> lMediaIds = oRes.m_resultIDs.Select(item => item.assetID).ToList();

                            if (oSearch.m_oOrder.m_eOrderBy.Equals(ApiObjects.SearchObjects.OrderBy.START_DATE))
                            {
                                lMediaIds =
                                    SortAssetsByStartDate(lMediaDocs, nIndex, oSearch.m_oOrder.m_eOrderDir,
                                        oSearch.associationTags, oSearch.parentMediaTypes).Select(x => Convert.ToInt32(x)).ToList();
                            }
                            else
                            {
                                Utils.OrderMediasByStats(lMediaIds, (int)oSearch.m_oOrder.m_eOrderBy, (int)oSearch.m_oOrder.m_eOrderDir);
                            }

                            Dictionary<int, SearchResult> dItems = oRes.m_resultIDs.ToDictionary(item => item.assetID);
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

        public List<string> GetAutoCompleteList(int nGroupID, MediaSearchObj oSearch, int nLangID, ref int nTotalItems)
        {
            List<string> lRes = new List<string>();

            oSearch.m_dOr.Add(new SearchValue()
            {
                m_lValue = new List<string>() { "" },
                m_sKey = "name^3"
            });

            ESMediaQueryBuilder queryParser = new ESMediaQueryBuilder(nGroupID, oSearch);
            queryParser.PageIndex = oSearch.m_nPageIndex;
            queryParser.PageSize = oSearch.m_nPageSize;

            queryParser.QueryType = eQueryType.PHRASE_PREFIX;

            string sQuery = queryParser.BuildMediaAutoCompleteQuery();

            if (!string.IsNullOrEmpty(sQuery))
            {
                string sType = Utils.GetESTypeByLanguage(ES_MEDIA_TYPE, oSearch.m_oLangauge);
                string retObj = m_oESApi.Search(nGroupID.ToString(), sType, ref sQuery);

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

        public SearchResultsObj SearchSubscriptionMedias(int nSubscriptionGroupId, List<MediaSearchObj> oSearch, int nLangID, bool bUseStartDate,
            string sMediaTypes, ApiObjects.SearchObjects.OrderObj oOrderObj, int nPageIndex, int nPageSize)
        {
            SearchResultsObj lSortedMedias = new SearchResultsObj();

            GroupManager groupManager = new GroupManager();

            CatalogCache catalogCache = CatalogCache.Instance();
            int nSubscriptionParentGroupID = catalogCache.GetParentGroup(nSubscriptionGroupId);

            Group oGroup = groupManager.GetGroup(nSubscriptionParentGroupID);

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

                string sOrderValue = FilteredQuery.GetESSortValue(oOrderObj);


                tempQuery = new FilteredQuery()
                {
                    PageIndex = nPageIndex,
                    PageSize = nPageSize
                };
                tempQuery.ESSort.Add(new ESOrderObj()
                {
                    m_eOrderDir = oOrderObj.m_eOrderDir,
                    m_sOrderValue = sOrderValue
                });
                tempQuery.Filter = new QueryFilter()
                {
                    FilterSettings = groupedFilters
                };

                string sSearchQuery = tempQuery.ToString();



                string sRetVal = m_oESApi.Search(oGroup.m_nParentGroupID.ToString(), ES_MEDIA_TYPE, ref sSearchQuery);

                lSearchResults = DecodeAssetSearchJsonObject(sRetVal, ref nTotalItems);

                if (lSearchResults != null && lSearchResults.Count > 0)
                {
                    log.Debug("Info - SearchSubscriptionMedias returned search results");
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
        public List<UnifiedSearchResult> SearchSubscriptionAssets(int subscriptionGroupId, List<BaseSearchObject> searchObjects, int languageId, bool useStartDate,
            string mediaTypes, ApiObjects.SearchObjects.OrderObj order, int pageIndex, int pageSize, ref int totalItems)
        {
            List<UnifiedSearchResult> finalSearchResults = new List<UnifiedSearchResult>();
            totalItems = 0;

            GroupManager groupManager = new GroupManager();

            CatalogCache catalogCache = CatalogCache.Instance();
            int parentGroupId = catalogCache.GetParentGroup(subscriptionGroupId);

            Group group = groupManager.GetGroup(parentGroupId);

            if (group == null)
                return finalSearchResults;

            int parentGroupID = group.m_nParentGroupID;

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

                string searchResultString = m_oESApi.Search(group.m_nParentGroupID.ToString(), ES_MEDIA_TYPE, ref searchQuery);

                int temporaryTotalItems = 0;
                searchResults = DecodeAssetSearchJsonObject(searchResultString, ref temporaryTotalItems);

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
                                    AssetType = UnifiedSearchResult.ParseType(temporaryDocument.type),
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
                            AssetType = UnifiedSearchResult.ParseType(item.type),
                            m_dUpdateDate = item.update_date
                        }).ToList();
                    }
                }

                #endregion
            }

            return finalSearchResults;
        }

        private static BoolQuery BuildMultipleSearchQuery(List<BaseSearchObject> searchObjects, int parentGroupId)
        {

            ESMediaQueryBuilder mediaQueryBuilder = new ESMediaQueryBuilder();
            ESUnifiedQueryBuilder unifiedQueryBuilder = new ESUnifiedQueryBuilder(null, parentGroupId);

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
                    FilteredQuery tempQuery = mediaQueryBuilder.BuildChannelFilteredQuery();

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

                    unifiedQueryBuilder.BuildInnerFilterAndQuery(out currentFilter, out currentQuery);

                    ESFilteredQuery currentFilteredQuery = new ESFilteredQuery()
                    {
                        Filter = new QueryFilter()
                        {
                            FilterSettings = currentFilter
                        },
                        Query = currentQuery
                    };

                    boolQuery.AddChild(currentQuery, CutWith.OR);
                }
            }

            return boolQuery;
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
                        tempRes = new ChannelContainObj()
                        {
                            m_nChannelID = searchObj.m_nChannelID
                        };
                        tempRes.m_bContain = (dChannels.ContainsKey(searchObj.m_nChannelID)) ? true : false;
                    }
                }

            }
            return oRes;
        }


        public List<int> GetMediaChannels(int nGroupID, int nMediaID)
        {
            List<int> lResult = new List<int>();
            string sIndex = nGroupID.ToString();

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
                    log.Error("Error - " + string.Format("GetMediaChannels - Could not parse response. Ex={0}, ST: {1}", ex.Message, ex.StackTrace), ex);
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
                    oResult.UpdateDate = mediaDoc.update_date;
                }
            }
            return oResult;
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
                DateTime searchEndDate = epgSearch.m_dSearchEndDate;

                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(epgSearch.m_nGroupID);


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

                string sGroupAlias = string.Format("{0}_epg", nParentGroupID);
                string searchRes = string.Empty;
                int nTotalRecords = 0;
                List<ElasticSearchApi.ESAssetDocument> lDocs = null;
                if (queries.Count == 1)
                {
                    string sQuery = queries[0];
                    searchRes = m_oESApi.Search(sGroupAlias, ES_EPG_TYPE, ref sQuery, lRouting);
                    lDocs = DecodeEpgSearchJsonObject(searchRes, ref nTotalRecords);
                }
                else
                {
                    searchRes = m_oESApi.MultiSearch(sGroupAlias, ES_EPG_TYPE, queries, lRouting);
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
                // save monitor and logs context data
                ContextData contextData = new ContextData();

                Task<string>[] tAliasRequests = new Task<string>[lDateAliases.Count];

                for (int i = 0; i < lDateAliases.Count; i++)
                {
                    tAliasRequests[i] = Task.Factory.StartNew<string>(
                         (index) =>
                         {
                             // load monitor and logs context data
                             contextData.Load();

                             string sIndex = lDateAliases[(int)index];
                             return (m_oESApi.IndexExists(sIndex)) ? sIndex : string.Empty;
                         }, i);
                }
                Task.WaitAll(tAliasRequests);

                foreach (Task<string> task in tAliasRequests)
                {
                    if (task != null)
                    {
                        if (!string.IsNullOrEmpty(task.Result))
                        {
                            lAliasResult.Add(task.Result);
                        }
                        task.Dispose();
                    }
                }
            }

            return lAliasResult;
        }

        public List<string> GetEpgAutoCompleteList(EpgSearchObj oSearch)
        {

            List<string> resultFinalList = null;

            List<string> lRouting = new List<string>();

            DateTime dTempDate = oSearch.m_dStartDate;
            while (dTempDate <= oSearch.m_dEndDate)
            {
                lRouting.Add(dTempDate.ToString("yyyyMMdd"));
                dTempDate = dTempDate.AddDays(1);
            }


            ESEpgQueryBuilder queryBuilder = new ESEpgQueryBuilder()
            {
                m_oEpgSearchObj = oSearch
            };
            string sQuery = queryBuilder.BuildEpgAutoCompleteQuery();


            string sGroupAlias = string.Format("{0}_epg", oSearch.m_nGroupID);
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
                    doc = DecodeSingleAssetJsonObject(jsonObj, "_source");
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Json Deserialization failed for ElasticSearch request. Exception={0}", ex.Message), ex);
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
                log.Error("Error - " + string.Format("Json Deserialization failed for ElasticSearch Media request. Exception={0}", ex.Message), ex);
            }

            return documents;
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
                        tempDocs = DecodeEpgSearchJsonObject(item.ToString(), ref tempTotal);
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
                        documents = new List<ElasticSearchApi.ESAssetDocument>();

                        string prefix = "fields";
                        foreach (var item in jsonObj.SelectToken("hits.hits"))
                        {
                            var newDocument = DecodeSingleAssetJsonObject(item, prefix);

                            documents.Add(newDocument);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Json Deserialization failed for ElasticSearch search request. Execption={0}", ex.Message), ex);
            }

            return documents;
        }

        private static ElasticSearchApi.ESAssetDocument DecodeSingleAssetJsonObject(JToken item, string fieldNamePrefix)
        {
            JToken tempToken = null;
            string typeString = ((tempToken = item.SelectToken("_type")) == null ? string.Empty : (string)tempToken);
            eAssetTypes assetType = UnifiedSearchResult.ParseType(typeString);

            string assetIdField = string.Empty;

            switch (assetType)
            {
                case eAssetTypes.MEDIA:
                {
                    assetIdField = AddPrefixToFieldName("media_id", fieldNamePrefix);
                    break;
                }
                case eAssetTypes.EPG:
                {
                    assetIdField = AddPrefixToFieldName("epg_id", fieldNamePrefix);
                    break;
                }
                case eAssetTypes.NPVR:
                {
                    assetIdField = AddPrefixToFieldName("recording_id", fieldNamePrefix);
                    break;
                }
                default:
                {
                    break;
                }
            }

            string id = ((tempToken = item.SelectToken("_id")) == null ? string.Empty : (string)tempToken);
            string index = ((tempToken = item.SelectToken("_index")) == null ? string.Empty : (string)tempToken);
            int assetId = 0;
            int groupId = 0;
            string name = string.Empty;
            DateTime cacheDate = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime updateDate = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime startDate = new DateTime(1970, 1, 1, 0, 0, 0);
            int mediaTypeId = 0;
            string epgIdentifier = string.Empty;

            JArray tempArray = null;

            tempToken = item.SelectToken(assetIdField);

            if (tempToken != null)
            {
                tempArray = tempToken as JArray;

                if (tempArray != null && tempArray.Count > 0)
                {
                    assetId = (int)tempArray[0];
                }
                else
                {
                    assetId = (int)tempToken;
                }
            }

            tempToken = item.SelectToken(AddPrefixToFieldName("group_id", fieldNamePrefix));

            if (tempToken != null)
            {
                tempArray = tempToken as JArray;

                if (tempArray != null && tempArray.Count > 0)
                {
                    groupId = (int)tempArray[0];
                }
                else
                {
                    groupId = (int)tempToken;
                }
            }

            tempToken = item.SelectToken(AddPrefixToFieldName("name", fieldNamePrefix));

            if (tempToken != null)
            {
                tempArray = tempToken as JArray;

                if (tempArray != null && tempArray.Count > 0)
                {
                    name = (string)tempArray[0];
                }
                else
                {
                    name = (string)tempToken;
                }
            }

            tempToken = item.SelectToken(AddPrefixToFieldName("cache_date", fieldNamePrefix));

            if (tempToken != null)
            {
                tempArray = tempToken as JArray;

                if (tempArray != null && tempArray.Count > 0)
                {
                    cacheDate = DateTime.ParseExact((string)tempArray[0], DATE_FORMAT, null);
                }
                else
                {
                    cacheDate = DateTime.ParseExact((string)tempToken, DATE_FORMAT, null);
                }
            }

            tempToken = item.SelectToken(AddPrefixToFieldName("update_date", fieldNamePrefix));

            if (tempToken != null)
            {
                tempArray = tempToken as JArray;

                if (tempArray != null && tempArray.Count > 0)
                {
                    updateDate = DateTime.ParseExact((string)tempArray[0], DATE_FORMAT, null);
                }
                else
                {
                    updateDate = DateTime.ParseExact((string)tempToken, DATE_FORMAT, null);
                }
            }

            tempToken = item.SelectToken(AddPrefixToFieldName("start_date", fieldNamePrefix));

            if (tempToken != null)
            {
                tempArray = tempToken as JArray;

                if (tempArray != null && tempArray.Count > 0)
                {
                    startDate = DateTime.ParseExact((string)tempArray[0], DATE_FORMAT, null);
                }
                else
                {
                    startDate = DateTime.ParseExact((string)tempToken, DATE_FORMAT, null);
                }
            }

            tempToken = item.SelectToken(AddPrefixToFieldName("media_type_id", fieldNamePrefix));

            if (tempToken != null)
            {
                tempArray = tempToken as JArray;

                if (tempArray != null && tempArray.Count > 0)
                {
                    mediaTypeId = (int)tempArray[0];
                }
                else
                {
                    mediaTypeId = (int)tempToken;
                }
            }

            tempToken = item.SelectToken(AddPrefixToFieldName("epg_identifier", fieldNamePrefix));

            if (tempToken != null)
            {
                tempArray = tempToken as JArray;

                if (tempArray != null && tempArray.Count > 0)
                {
                    epgIdentifier = (string)tempArray[0];
                }
                else
                {
                    epgIdentifier = (string)tempToken;
                }
            }

            var newDocument = new ElasticSearchApi.ESAssetDocument()
            {
                id = id,
                index = index,
                type = typeString,
                asset_id = assetId,
                group_id = groupId,
                name = name,
                cache_date = cacheDate,
                update_date = updateDate,
                start_date = startDate,
                media_type_id = mediaTypeId,
                epg_identifier = epgIdentifier,
            };
            return newDocument;
        }

        private static string AddPrefixToFieldName(string fieldName, string prefix)
        {
            string result = fieldName;

            if (!string.IsNullOrEmpty(prefix))
            {
                result = string.Format("{0}.{1}", prefix, fieldName);
            }

            return result;
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
                            update_date = ((tempToken = item.SelectToken("fields.update_date")) == null ? new DateTime(1970, 1, 1, 0, 0, 0) :
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
                log.Error("Error - " + string.Format("Json Deserialization failed for ElasticSearch Epg request. Ex Msg: {0}, JSON Obj: {1} ST: {2}", ex.Message, sObj, ex.StackTrace), ex);
            }

            return documents;
        }

        public List<List<string>> GetChannelsDefinitions(List<List<long>> listsOfChannelIDs, long groupID)
        {
            if (listsOfChannelIDs != null && listsOfChannelIDs.Count > 0)
            {
                List<string> indicesBehindAlias = m_oESApi.GetAliases(groupID + "");
                if (indicesBehindAlias != null && indicesBehindAlias.Count > 0)
                {
                    List<List<string>> res = new List<List<string>>(listsOfChannelIDs.Count);
                    for (int i = 0; i < listsOfChannelIDs.Count; i++)
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
                string sQuery = queryBuilder.GetDocumentsByIdsQuery(distinctMediaIDs, new OrderObj()
                {
                    m_eOrderBy = ApiObjects.SearchObjects.OrderBy.ID
                });

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

        #region Unified Search

        /// <summary>
        /// Performs a search on several types of assets in a single call
        /// </summary>
        /// <param name="unifiedSearchDefinitions"></param>
        /// <returns></returns>
        public List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearchDefinitions, ref int totalItems, ref int to)
        {
            List<UnifiedSearchResult> searchResultsList = new List<UnifiedSearchResult>();
            totalItems = 0;

            OrderObj order = unifiedSearchDefinitions.order;
            ApiObjects.SearchObjects.OrderBy orderBy = order.m_eOrderBy;
            bool isOrderedByStat = false;

            ESUnifiedQueryBuilder queryParser = new ESUnifiedQueryBuilder(unifiedSearchDefinitions);

            int pageIndex = 0;
            int pageSize = 0;

            // If this is orderd by a social-stat - first we will get all asset Ids and only then we will sort and page
            if ((orderBy <= ApiObjects.SearchObjects.OrderBy.VIEWS &&
                orderBy >= ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER) ||
                orderBy.Equals(ApiObjects.SearchObjects.OrderBy.VOTES_COUNT) ||
                // Recommendations is also non-sortable
                orderBy.Equals(ApiObjects.SearchObjects.OrderBy.RECOMMENDATION) ||
                // If there are virtual assets (series/episode) and the sort is by start date - this is another case of unique sort
                (orderBy.Equals(ApiObjects.SearchObjects.OrderBy.START_DATE) &&
                unifiedSearchDefinitions.parentMediaTypes.Count > 0 &&
                unifiedSearchDefinitions.shouldSearchMedia
                ))
            {
                pageIndex = unifiedSearchDefinitions.pageIndex;
                pageSize = unifiedSearchDefinitions.pageSize;
                queryParser.PageIndex = 0;
                queryParser.PageSize = 0;

                if (orderBy.Equals(ApiObjects.SearchObjects.OrderBy.START_DATE))
                {
                    unifiedSearchDefinitions.extraReturnFields.Add("start_date");
                    unifiedSearchDefinitions.extraReturnFields.Add("media_type_id");
                }
                else
                {
                    // Initial sort will be by ID
                    unifiedSearchDefinitions.order.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.ID;
                }

                isOrderedByStat = true;
            }
            else
            {
                queryParser.PageIndex = unifiedSearchDefinitions.pageIndex;
                queryParser.PageSize = unifiedSearchDefinitions.pageSize;
                queryParser.From = unifiedSearchDefinitions.from;
            }

            // ES index is on parent group id
            CatalogCache catalogCache = CatalogCache.Instance();
            int parentGroupId = catalogCache.GetParentGroup(unifiedSearchDefinitions.groupId);

            // In case something failed here, use the group that was sent
            if (parentGroupId == 0)
            {
                parentGroupId = unifiedSearchDefinitions.groupId;
            }

            if (unifiedSearchDefinitions.entitlementSearchDefinitions != null &&
                unifiedSearchDefinitions.entitlementSearchDefinitions.subscriptionSearchObjects != null)
            {
                // If we need to search by entitlements, we have A LOT of work to do now
                BoolQuery boolQuery = BuildMultipleSearchQuery(unifiedSearchDefinitions.entitlementSearchDefinitions.subscriptionSearchObjects, parentGroupId);
                queryParser.SubscriptionsQuery = boolQuery;
            }

            string requestBody = queryParser.BuildSearchQueryString(unifiedSearchDefinitions.shouldIgnoreDeviceRuleID, unifiedSearchDefinitions.shouldAddActive);

            if (!string.IsNullOrEmpty(requestBody))
            {
                int httpStatus = 0;

                string indexes = ESUnifiedQueryBuilder.GetIndexes(unifiedSearchDefinitions, parentGroupId);
                string types = ESUnifiedQueryBuilder.GetTypes(unifiedSearchDefinitions);
                string url = string.Format("{0}/{1}/{2}/_search", ES_BASE_ADDRESS, indexes, types);

                string queryResultString = m_oESApi.SendPostHttpReq(url, ref httpStatus, string.Empty, string.Empty, requestBody, true);

                log.DebugFormat("ES request: URL = {0}, body = {1}, result = {2}", url, requestBody, queryResultString);

                if (httpStatus == STATUS_OK)
                {
                    #region Process ElasticSearch result

                    List<ElasticSearchApi.ESAssetDocument> assetsDocumentsDecoded = DecodeAssetSearchJsonObject(queryResultString, ref totalItems);

                    if (assetsDocumentsDecoded != null && assetsDocumentsDecoded.Count > 0)
                    {
                        searchResultsList = new List<UnifiedSearchResult>();

                        foreach (ElasticSearchApi.ESAssetDocument doc in assetsDocumentsDecoded)
                        {
                            searchResultsList.Add(new UnifiedSearchResult()
                            {
                                AssetId = doc.asset_id.ToString(),
                                m_dUpdateDate = doc.update_date,
                                AssetType = UnifiedSearchResult.ParseType(doc.type)
                            });
                        }

                        // If this is orderd by a social-stat - first we will get all asset Ids and only then we will sort and page
                        if (isOrderedByStat)
                        {
                            #region Ordered by stat
                            List<long> assetIds = searchResultsList.Select(item => long.Parse(item.AssetId)).ToList();

                            List<long> orderedIds = null;

                            // Do special sort only when searching by media
                            if (orderBy == ApiObjects.SearchObjects.OrderBy.START_DATE && unifiedSearchDefinitions.shouldSearchMedia)
                            {
                                orderedIds = SortAssetsByStartDate(assetsDocumentsDecoded, parentGroupId, order.m_eOrderDir,
                                    unifiedSearchDefinitions.associationTags,
                                    unifiedSearchDefinitions.parentMediaTypes);
                            }
                            // Recommendation - the order is predefined already. We will use the order that is given to us
                            else if (orderBy == ApiObjects.SearchObjects.OrderBy.RECOMMENDATION)
                            {
                                orderedIds = new List<long>();
                                HashSet<long> idsHashset = new HashSet<long>(assetIds);

                                // Add all ordered ids from definitions first
                                foreach (var id in unifiedSearchDefinitions.specificOrder)
                                {
                                    // If the id exists in search results
                                    if (idsHashset.Remove(id))
                                    {
                                        // add to ordered list
                                        orderedIds.Add(id);
                                    }
                                }

                                // Add all ids that are left
                                foreach (long id in idsHashset)
                                {
                                    orderedIds.Add(id);
                                }
                            }
                            else
                            {
                                orderedIds = SortAssetsByStats(assetIds, parentGroupId, orderBy, order.m_eOrderDir);
                            }

                            Dictionary<int, UnifiedSearchResult> idToResultDictionary = new Dictionary<int, UnifiedSearchResult>();

                            // Map all results in dictionary
                            searchResultsList.ForEach(item =>
                                {
                                    int assetId = int.Parse(item.AssetId);

                                    if (!idToResultDictionary.ContainsKey(assetId))
                                    {
                                        idToResultDictionary.Add(assetId, item);
                                    }
                                });

                            searchResultsList.Clear();

                            // check which results should be returned
                            bool illegalRequest = false;
                            assetIds = TVinciShared.ListUtils.Page<long>(orderedIds, pageSize, pageIndex, out illegalRequest).ToList();

                            if (!illegalRequest)
                            {
                                UnifiedSearchResult temporaryResult;

                                foreach (int id in assetIds)
                                {
                                    if (idToResultDictionary.TryGetValue(id, out temporaryResult))
                                    {
                                        searchResultsList.Add(temporaryResult);
                                    }
                                }
                            }
                            #endregion
                        }
                    }


                    #endregion
                }
                else if (httpStatus == STATUS_NOT_FOUND || httpStatus >= STATUS_INTERNAL_ERROR)
                {
                    throw new System.Web.HttpException(httpStatus, queryResultString);
                }
            }

            return (searchResultsList);
        }

        private List<long> SortAssetsByStartDate(List<ElasticSearchApi.ESAssetDocument> assets,
            int groupId, OrderDir orderDirection,
            Dictionary<int, string> associationTags, Dictionary<int, int> mediaTypeParent)
        {
            if (assets == null || assets.Count == 0)
            {
                return new List<long>();
            }

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
                    ESTerms tagsTerms = new ESTerms(false)
                    {
                        Key = string.Format("tags.{0}", item.Value.ToLower())
                    };

                    tagsTerms.Value.AddRange(typeToNames[mediaTypeParent[item.Key]]);

                    tagsFilter.AddChild(tagsTerms);
                }
            }

            ESTerm isActiveTerm = new ESTerm(true)
            {
                Key = "is_active",
                Value = "1"
            };

            string nowSearchString = DateTime.UtcNow.ToString(DATE_FORMAT);

            ESRange startDateRange = new ESRange(false)
            {
                Key = "start_date"
            };

            startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, nowSearchString));

            ESRange endDateRange = new ESRange(false)
            {
                Key = "end_date"
            };

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
            string index = groupId.ToString();

            string searchResults = m_oESApi.Search(index, "media", ref searchRequestBody);

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

                                            idToStartDate[assetId] = DateTime.ParseExact(maximumStartDate, DATE_FORMAT, null);
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
            var sortedDictionary = idToStartDate.OrderBy(pair => pair.Value);

            #region Create final, sorted, list

            List<long> sortedList = new List<long>();
            HashSet<int> alreadyContainedIds = new HashSet<int>();

            foreach (var currentId in sortedDictionary)
            {
                int id = int.Parse(currentId.Key);

                // Depending on direction - if it is ascending, insert Id at start. Otherwise at end
                if (orderDirection == OrderDir.DESC)
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
                    if (orderDirection == OrderDir.ASC)
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
        /// <param name="groupId"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <returns></returns>
        private List<long> SortAssetsByStats(List<long> assetIds, int groupId, ApiObjects.SearchObjects.OrderBy orderBy, OrderDir orderDirection)
        {
            List<long> sortedList = null;
            HashSet<long> alreadyContainedIds = null;

            ConcurrentDictionary<string, List<StatisticsAggregationResult>> ratingsAggregationsDictionary =
                new ConcurrentDictionary<string, List<StatisticsAggregationResult>>();
            ConcurrentDictionary<string, ConcurrentDictionary<string, int>> countsAggregationsDictionary =
                new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();

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
                Value = groupId.ToString()
            });

            #region define action filter

            string actionName = string.Empty;

            switch (orderBy)
            {
                case ApiObjects.SearchObjects.OrderBy.VIEWS:
                {
                    actionName = Catalog.STAT_ACTION_FIRST_PLAY;
                    break;
                }
                case ApiObjects.SearchObjects.OrderBy.RATING:
                {
                    actionName = Catalog.STAT_ACTION_RATES;
                    break;
                }
                case ApiObjects.SearchObjects.OrderBy.VOTES_COUNT:
                {
                    actionName = Catalog.STAT_ACTION_RATES;
                    break;
                }
                case ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER:
                {
                    actionName = Catalog.STAT_ACTION_LIKE;
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
                    Type = eElasticAggregationType.terms,
                };

                aggregations.SubAggrgations.Add(new ESBaseAggsItem()
                {
                    Name = "sub_stats",
                    Type = eElasticAggregationType.stats,
                    Field = Catalog.STAT_ACTION_RATE_VALUE_FIELD
                });
            }
            else
            {
                aggregations = new ESBaseAggsItem()
                {
                    Name = "stats",
                    Field = "media_id",
                    Type = eElasticAggregationType.terms,
                };
            }

            filteredQuery.Aggregations.Add(aggregations);

            #endregion

            #region Split call of aggregations query to pieces

            int aggregationssSize = 5000;

            //Start MultiThread Call
            List<Task> tasks = new List<Task>();

            // Split the request to small pieces, to avoid timeout exceptions
            for (int assetIndex = 0; assetIndex < assetIds.Count; assetIndex += aggregationssSize)
            {
                idsTerm.Value.Clear();

                // Convert partial Ids to strings
                idsTerm.Value.AddRange(assetIds.Skip(assetIndex).Take(aggregationssSize).Select(id => id.ToString()));

                string aggregationsRequestBody = filteredQuery.ToString();

                string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(groupId);

                try
                {
                    ContextData contextData = new ContextData();
                    // Create a task for the search and merge of partial aggregations
                    Task task = Task.Factory.StartNew((obj) =>
                        {
                            contextData.Load();
                            // Get aggregations results
                            string aggregationsResults = m_oESApi.Search(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref aggregationsRequestBody);

                            if (orderBy == ApiObjects.SearchObjects.OrderBy.RATING)
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
                            else
                            {
                                // Parse string into dictionary
                                var partialDictionary = ESAggregationsResult.DeserializeAggrgations<string>(aggregationsResults);

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
                        },
                        new Object());

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
                    if (orderDirection == OrderDir.ASC)
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

        /// <summary>
        /// After receiving a result from ES server, process it to create a list of Ids with the given order
        /// </summary>
        /// <param name="statsDictionary"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="alreadyContainedIds"></param>
        /// <returns></returns>
        private static void ProcessCountDictionaryResults(ConcurrentDictionary<string, ConcurrentDictionary<string, int>> statsDictionary,
            OrderDir orderDirection, HashSet<long> alreadyContainedIds, List<long> sortedList)
        {
            if (statsDictionary != null && statsDictionary.Count > 0)
            {
                ConcurrentDictionary<string, int> statResult;

                //retrieve specific stats result
                statsDictionary.TryGetValue("stats", out statResult);

                if (statResult != null && statResult.Count > 0)
                {
                    // We base this section on the assumption that aggregations request is sorted, descending
                    foreach (string currentKey in statResult.Keys)
                    {
                        int count = statResult[currentKey];

                        int currentId;

                        if (int.TryParse(currentKey, out currentId))
                        {
                            // Depending on direction - if it is ascending, insert Id at start. Otherwise at end
                            if (orderDirection == OrderDir.ASC)
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
            OrderDir orderDirection, HashSet<long> alreadyContainedIds, List<long> sortedList)
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
                            if (orderDirection == OrderDir.ASC)
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

        #region Multiple Unified Search

        public List<UnifiedSearchResult> MultipleUnifiedSearch(int groupId, List<UnifiedSearchDefinitions> unifiedSearchDefinitions, ref int totalItems)
        {
            List<UnifiedSearchResult> searchResultsList = new List<UnifiedSearchResult>();
            totalItems = 0;

            string requestBody = new ESUnifiedQueryBuilder(null, groupId).BuildMultiSearchQueryString(unifiedSearchDefinitions);

            if (!string.IsNullOrEmpty(requestBody))
            {
                int httpStatus = 0;

                string indexes = ESUnifiedQueryBuilder.GetIndexes(unifiedSearchDefinitions, groupId);
                string types = ESUnifiedQueryBuilder.GetTypes(unifiedSearchDefinitions);
                string url = string.Format("{0}/{1}/{2}/_search", ES_BASE_ADDRESS, indexes, types);

                string queryResultString = m_oESApi.SendPostHttpReq(url, ref httpStatus, string.Empty, string.Empty, requestBody, true);

                log.DebugFormat("ES request: URL = {0}, body = {1}, result = {2}", url, requestBody, queryResultString);

                if (httpStatus == STATUS_OK)
                {
                    #region Process ElasticSearch result

                    List<ElasticSearchApi.ESAssetDocument> assetsDocumentsDecoded = DecodeAssetSearchJsonObject(queryResultString, ref totalItems);

                    if (assetsDocumentsDecoded != null && assetsDocumentsDecoded.Count > 0)
                    {
                        searchResultsList = new List<UnifiedSearchResult>();

                        foreach (ElasticSearchApi.ESAssetDocument doc in assetsDocumentsDecoded)
                        {
                            searchResultsList.Add(new UnifiedSearchResult()
                            {
                                AssetId = doc.asset_id.ToString(),
                                m_dUpdateDate = doc.update_date,
                                AssetType = UnifiedSearchResult.ParseType(doc.type)
                            });
                        }
                    }

                    #endregion
                }
                else if (httpStatus == STATUS_NOT_FOUND || httpStatus >= STATUS_INTERNAL_ERROR)
                {
                    throw new System.Web.HttpException(httpStatus, queryResultString);
                }
            }

            return searchResultsList;
        }

        #endregion

        public List<UnifiedSearchResult> FillUpdateDates(int groupId, List<UnifiedSearchResult> assets, ref int totalItems, int pageSize, int pageIndex)
        {
            List<UnifiedSearchResult> finalList = new List<UnifiedSearchResult>();
            totalItems = 0;

            bool shouldSearchEpg = false;
            bool shouldSearchMedia = false;
            string media = "media";
            string epg = "epg";

            // Realize what asset types do we have
            shouldSearchMedia = assets.Exists(asset => asset.AssetType == eAssetTypes.MEDIA);
            shouldSearchEpg = assets.Exists(asset => asset.AssetType == eAssetTypes.EPG);

            // Build indexes and types string - for URL
            string indexes = string.Empty;
            string types = string.Empty;

            if (shouldSearchEpg)
            {
                if (shouldSearchMedia)
                {
                    indexes = string.Format("{0},{0}_epg", groupId);
                    types = string.Format("{0},{1}", media, epg);
                }
                else
                {
                    indexes = string.Format("{0}_epg", groupId);
                    types = epg;
                }
            }
            else
            {
                indexes = groupId.ToString();
                types = media;
            }

            // Build complete URL
            string url = string.Format("{0}/{1}/{2}/_search", ES_BASE_ADDRESS, indexes, types);

            // Build request body with the assistance of unified query builder
            List<KeyValuePair<eAssetTypes, string>> assetsPairs = assets.Select(asset =>
                new KeyValuePair<eAssetTypes, string>(asset.AssetType, asset.AssetId)).ToList();

            string requestBody = ESUnifiedQueryBuilder.BuildGetUpdateDatesString(assetsPairs);

            int httpStatus = 0;

            // Perform search
            string queryResultString = m_oESApi.SendPostHttpReq(url, ref httpStatus, string.Empty, string.Empty, requestBody, true);

            log.DebugFormat("ES request: URL = {0}, body = {1}, result = {2}", url, requestBody, queryResultString);

            if (httpStatus == STATUS_OK)
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
                            eAssetTypes assetType = UnifiedSearchResult.ParseType(typeString);

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
                            DateTime update_date = ((tempToken = item.SelectToken("fields.update_date")) == null ? new DateTime(1970, 1, 1, 0, 0, 0) :
                                        DateTime.ParseExact((string)tempToken, DATE_FORMAT, null));

                            // Find the asset in the list with this ID, set its update date
                            assets.First(result => result.AssetId == id).m_dUpdateDate = update_date;
                        }
                    }
                }

                var validAssets = assets.Where(asset =>
                    {
                        bool valid = asset.m_dUpdateDate != DateTime.MinValue;

                        if (!valid)
                        {
                            log.WarnFormat(
                                "Received invalid asset from recommendation engine. ID = {0}, type = {1}", asset.AssetId, asset.AssetType.ToString());
                        }

                        return valid;
                    });

                finalList = validAssets.ToList();

                bool illegalRequest = false;
                var pagedList = TVinciShared.ListUtils.Page(validAssets, pageSize, pageIndex, out illegalRequest);

                //if (!illegalRequest)
                //{
                //    finalList = pagedList.ToList();
                //}
                //else
                //{
                //    finalList = null;
                //}

                #endregion
            }

            return finalList;
        }

        public List<int> GetEntitledEpgLinearChannels(Group group, UnifiedSearchDefinitions definitions)
        {
            List<int> result = new List<int>();
            ESUnifiedQueryBuilder queryParser = new ESUnifiedQueryBuilder(definitions);
            queryParser.PageIndex = 0;
            queryParser.PageSize = 0;

            if (definitions.entitlementSearchDefinitions != null &&
                definitions.entitlementSearchDefinitions.subscriptionSearchObjects != null)
            {
                // If we need to search by entitlements, we have A LOT of work to do now
                BoolQuery boolQuery = BuildMultipleSearchQuery(definitions.entitlementSearchDefinitions.subscriptionSearchObjects, group.m_nParentGroupID);
                queryParser.SubscriptionsQuery = boolQuery;
            }

            string requestBody = queryParser.BuildSearchQueryString(definitions.shouldIgnoreDeviceRuleID, definitions.shouldAddActive);

            if (!string.IsNullOrEmpty(requestBody))
            {
                int httpStatus = 0;

                string indexes = ESUnifiedQueryBuilder.GetIndexes(definitions, group.m_nParentGroupID);
                string types = ESUnifiedQueryBuilder.GetTypes(definitions);
                string url = string.Format("{0}/{1}/{2}/_search", ES_BASE_ADDRESS, indexes, types);

                string queryResultString = m_oESApi.SendPostHttpReq(url, ref httpStatus, string.Empty, string.Empty, requestBody, true);

                log.DebugFormat("ES request: URL = {0}, body = {1}, result = {2}", url, requestBody, queryResultString);

                if (httpStatus == STATUS_OK)
                {
                    #region Process ElasticSearch result

                    int totalItems = 0;
                    List<ElasticSearchApi.ESAssetDocument> assetsDocumentsDecoded = DecodeAssetSearchJsonObject(queryResultString, ref totalItems);

                    if (assetsDocumentsDecoded != null && assetsDocumentsDecoded.Count > 0)
                    {
                        foreach (var asset in assetsDocumentsDecoded)
                        {
                            string epgIdentifier = asset.epg_identifier;
                            int epgIdentifierInt;

                            if (int.TryParse(epgIdentifier, out epgIdentifierInt))
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

        public ApiObjects.Response.Status DeleteStatistics(int groupId, DateTime until)
        {
            ApiObjects.Response.Status status = null;

            try
            {
                var api = new ElasticSearch.Common.ElasticSearchApi();

                string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(groupId);
                string type = ElasticSearch.Common.Utils.ES_STATS_TYPE;

                #region Build Query

                string date = until.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT);

                ESRange dateRange = new ESRange(false)
                {
                    Key = "action_date"
                };

                dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LT, date));

                ESTerm typeTerm = new ESTerm(false)
                {
                    Key = "action",
                    Value = "mediahit"
                };

                BaseFilterCompositeType filter = new FilterCompositeType(CutWith.AND);
                filter.AddChild(dateRange);
                filter.AddChild(typeTerm);

                QueryFilter queryFilter = new QueryFilter()
                {
                    FilterSettings = filter
                };

                BoolQuery boolQuery = new BoolQuery();
                boolQuery.AddChild(typeTerm, CutWith.AND);
                boolQuery.AddChild(dateRange, CutWith.AND);

                FilteredQuery filteredQuery = new FilteredQuery(true)
                {
                    PageIndex = 0,
                    PageSize = 0,
                    Query = boolQuery
                };

                filteredQuery.ReturnFields.Clear();
                filteredQuery.ReturnFields.Add("\"_id\"");

                #endregion

                string query = filteredQuery.ToString();

                string searchResults = api.Search(index, type, ref query);

                List<string> documents = ElasticSearch.Common.Utils.GetDocumentIds(searchResults);

                List<ESBulkRequestObj<string>> lBulkObj = new List<ESBulkRequestObj<string>>();           
                int sizeOfBulk = 500;

                foreach (var document in documents)
                {
                        lBulkObj.Add(new ESBulkRequestObj<string>()
                        {
                            docID = document,
                            index = index,
                            type = type,
                            Operation = eOperation.delete
                        });

                        if (lBulkObj.Count >= sizeOfBulk)
                        {
                            Task<object> t = Task<object>.Factory.StartNew(() => api.CreateBulkRequest(lBulkObj));
                            t.Wait();
                            
                            lBulkObj = new List<ESBulkRequestObj<string>>();
                        }
                }

                if (lBulkObj.Count > 0)
                {
                    Task<object> t = Task<object>.Factory.StartNew(() => api.CreateBulkRequest(lBulkObj));
                    t.Wait();
                }

                status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ex.Message);
            }

            return status;
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
