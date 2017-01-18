using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ODBCWrapper;
using TVinciShared;
using System.Runtime.Serialization;
using System.Reflection;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using Tvinci.Core.DAL;
using ApiObjects.SearchObjects;

using Core.Catalog.Cache;
using GroupsCacheManager;
using Core.Catalog.Response;
using KLogMonitor;

namespace Core.Catalog.Request
{
    /**************************************************************************
   * Get Channel + It's Medias List
   * return :
   * Channel with it's media List
   * ************************************************************************/
    [Serializable]
    [DataContract]
    public class ChannelRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public Int32 m_nChannelID;
        [DataMember]
        public bool m_bIgnoreDeviceRuleID;
        [DataMember]
        public ApiObjects.SearchObjects.OrderObj m_oOrderObj;

        public ChannelRequest()
            : base()
        {
            m_bIgnoreDeviceRuleID = false;
        }

        public ChannelRequest(Int32 nChannelID, Int32 nGroupID, Int32 nPageSize, Int32 nPageIndex, string sUserIP, Filter oFilter, string sSignature, string sSignString, ApiObjects.SearchObjects.OrderObj oOrderObj)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {
            m_nChannelID = nChannelID;
            m_oOrderObj = oOrderObj;
            m_bIgnoreDeviceRuleID = false;
        }

        public ChannelRequest(ChannelRequest c)
            : base(c.m_nPageSize, c.m_nPageIndex, c.m_sUserIP, c.m_nGroupID, c.m_oFilter, c.m_sSignature, c.m_sSignString)
        {
            m_nChannelID = c.m_nChannelID;
            m_oOrderObj = c.m_oOrderObj;
            m_bIgnoreDeviceRuleID = c.m_bIgnoreDeviceRuleID;
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                ChannelRequest request = oBaseRequest as ChannelRequest;

                if (request == null || request.m_nChannelID == 0)
                    throw new ArgumentException("request object is null or Required variables is null");

                CheckSignature(request);

                ChannelResponse response = new ChannelResponse();
                Group group = null;
                GroupsCacheManager.Channel channel = null;

                ApiObjects.SearchObjects.MediaSearchObj channelSearchObject = null;

                try
                {
                    GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
                    CatalogCache catalogCache = CatalogCache.Instance();
                    int nParentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);
                    groupManager.GetGroupAndChannel(request.m_nChannelID, nParentGroupID, ref group, ref channel);

                }
                catch (Exception ex)
                {
                    log.Error("ChannelRequest - " + string.Format("failed to get GetGroupAndChannel channelID={0}, ex={1} , st: {2}", request.m_nChannelID, ex.Message, ex.StackTrace), ex);
                    group = null;
                    channel = null;
                }


                if (group != null && channel != null)
                {
                    // If this is a KSQL channel
                    if (channel.m_nChannelTypeID == 4)
                    {
                        response.m_nTotalItems = 0;
                        response.m_nMedias = null;
                        return response;
                    }

                    channelSearchObject = GetSearchObject(channel, request, group.m_nParentGroupID, group.GetGroupDefaultLanguage(), group.m_sPermittedWatchRules);

                    channelSearchObject.m_bIgnoreDeviceRuleId = request.m_bIgnoreDeviceRuleID;

                    List<int> medias = new List<int>();
                    int nPageIndex = 0;
                    int nPageSize = 0;

                    if (channel.m_nChannelTypeID == 2 || IsSlidingWindow(channel))
                    {
                        nPageIndex = channelSearchObject.m_nPageIndex;
                        nPageSize = channelSearchObject.m_nPageSize;
                        channelSearchObject.m_nPageSize = 0;
                        channelSearchObject.m_nPageIndex = 0;
                    }

                    ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();
                    if (searcher == null)
                    {
                        return response;
                    }
                    int nTotalItems = 0;

                    SearchResultsObj oSearchResults = searcher.SearchMedias(channel.m_nGroupID, channelSearchObject, request.m_oFilter.m_nLanguage, request.m_oFilter.m_bUseStartDate, request.m_nGroupID);
                    List<SearchResult> lMediaRes = null;
                    if (oSearchResults != null && oSearchResults.m_resultIDs != null && oSearchResults.m_resultIDs.Count > 0)
                    {
                        medias = oSearchResults.m_resultIDs.Select(item => item.assetID).ToList();
                        nTotalItems = oSearchResults.n_TotalItems;
                        DateTime nMinDateTime = new DateTime(1970, 1, 1, 0, 0, 0);

                        if (searcher.GetType().Equals(typeof(ElasticsearchWrapper))) // != typeof(LuceneWrapper))
                        {
                            lMediaRes = oSearchResults.m_resultIDs.Select(item => new SearchResult() { assetID = item.assetID, UpdateDate = item.UpdateDate }).ToList();
                        }
                    }
                    else
                    {
                        return response;
                    }

                    if (IsSlidingWindow(channel))
                    {
                        medias = OrderMediaBySlidingWindow(group.m_nParentGroupID, channel.m_OrderObject.m_eOrderBy, channel.m_OrderObject.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.DESC, nPageSize, nPageIndex, medias, channel.m_OrderObject.m_dSlidingWindowStartTimeField);

                        nTotalItems = 0;

                        if (medias != null && medias.Count > 0)
                        {
                            nTotalItems = medias.Count;
                            Dictionary<int, SearchResult> dMediaRes = lMediaRes.ToDictionary(item => item.assetID);
                            lMediaRes.Clear();
                            foreach (int item in medias)
                            {
                                if (dMediaRes.ContainsKey(item))
                                    lMediaRes.Add(dMediaRes[item]);
                            }
                        }
                        else
                        {
                            lMediaRes.Clear();
                        }
                    }

                    if (channel.m_nChannelTypeID == 2)
                    {
                        OrderMediasByOrderNum(ref medias, channel, channelSearchObject.m_oOrder);

                        int nValidNumberOfMediasRange = nPageSize;
                        if (Utils.ValidatePageSizeAndPageIndexAgainstNumberOfMedias(medias.Count, nPageIndex, ref nValidNumberOfMediasRange))
                        {
                            if (nValidNumberOfMediasRange > 0)
                            {
                                medias = medias.GetRange(nPageSize * nPageIndex, nValidNumberOfMediasRange);
                            }
                        }
                        else
                        {
                            medias.Clear();
                        }

                        if (searcher.GetType().Equals(typeof(ElasticsearchWrapper)) && medias != null && medias.Count > 0)
                        {
                            Dictionary<int, SearchResult> dMediaRes = lMediaRes.ToDictionary(item => item.assetID);
                            lMediaRes = new List<SearchResult>();
                            foreach (int item in medias)
                            {
                                if (dMediaRes.ContainsKey(item))
                                {
                                    lMediaRes.Add(dMediaRes[item]);
                                }
                            }
                        }
                    }

                    if (medias == null || medias.Count() == 0)
                    {
                        response.m_nMedias = null;
                        response.m_nTotalItems = 0;
                        return response;
                    }


                    response.m_nTotalItems = nTotalItems;

                    if (searcher.GetType().Equals(typeof(LuceneWrapper)))   //if (lMediaRes == null) //LUCENE
                    {
                        lMediaRes = GetMediaUpdateDate(medias);
                    }

                    if (lMediaRes.Count > 0)
                    {
                        response.m_nMedias = new List<SearchResult>(lMediaRes);
                    }
                    else
                    {
                        response.m_nMedias = null;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                log.Error("ES Error - " + String.Concat("Exception. Req: ", ToString(), " Msg: ", ex.Message, " Type: ", ex.GetType().Name, " ST: ", ex.StackTrace), ex);

                throw ex;
            }
        }

        public static bool IsSlidingWindow(GroupsCacheManager.Channel channel)
        {
            bool bResult = false;

            if (channel.m_OrderObject.m_bIsSlidingWindowField)
            {
                bResult = typeof(OrderBy).GetMember(channel.m_OrderObject.m_eOrderBy.ToString())[0].GetCustomAttributes(typeof(SlidingWindowSupportedAttribute), false).Length > 0;
            }

            return bResult;
        }

        public static List<int> OrderMediaBySlidingWindow(int nGroupId, ApiObjects.SearchObjects.OrderBy orderBy, bool isDesc, int pageSize, int PageIndex, List<int> media, DateTime windowTime)
        {
            List<int> result;
            DateTime now = DateTime.UtcNow;
            switch (orderBy)
            {
                case OrderBy.VIEWS:
                    if (Utils.IsGroupIDContainedInConfig(nGroupId, "GROUPS_USING_DB_FOR_ASSETS_STATS", ';'))
                    {
                        result = new List<int>();
                        Dictionary<int, int[]> dict = CatalogDAL.Get_MediaStatistics(windowTime, DateTime.UtcNow, nGroupId, media);
                        if (dict != null && dict.Count > 0)
                        {
                            result = (from pair in dict
                                        orderby pair.Value[0] descending
                                        select pair.Key).ToList();
                        }
                    }
                    /************* For versions after Joker that don't want to use DB for getting view stats (first_play), we fetch the data from ES statistics index **********/
                    else
                    {
                        result = CatalogLogic.SlidingWindowCountAggregations(nGroupId, media, windowTime, now, CatalogLogic.STAT_ACTION_FIRST_PLAY);
                    }
                    break;
                case OrderBy.RATING:
                    result = CatalogLogic.SlidingWindowStatisticsAggregations(nGroupId, media, windowTime, now, CatalogLogic.STAT_ACTION_RATES, CatalogLogic.STAT_ACTION_RATE_VALUE_FIELD, 
                        ElasticSearch.Searcher.AggregationsComparer.eCompareType.Average);
                    break;
                case OrderBy.VOTES_COUNT:
                    result = CatalogLogic.SlidingWindowCountAggregations(nGroupId, media, windowTime, now, CatalogLogic.STAT_ACTION_RATES);
                    break;
                case OrderBy.LIKE_COUNTER:
                    result = CatalogLogic.SlidingWindowCountAggregations(nGroupId, media, windowTime, now, CatalogLogic.STAT_ACTION_LIKE);
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

        public static void OrderMediasByOrderNum(ref List<int> medias, GroupsCacheManager.Channel channel, ApiObjects.SearchObjects.OrderObj oOrderObj)
        {
            if (oOrderObj.m_eOrderBy.Equals(OrderBy.ID))
            {
                if (channel.m_lManualMedias == null)
                {
                    channel.m_lManualMedias = new List<GroupsCacheManager.ManualMedia>();
                }

                IEnumerable<int> ids;
                if (oOrderObj.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.DESC)
                {
                    ids = from m in medias
                          join manual in channel.m_lManualMedias
                          on m.ToString() equals manual.m_sMediaId
                          orderby manual.m_nOrderNum descending
                          select int.Parse(manual.m_sMediaId);
                }
                else
                {
                    ids = from m in medias
                          join manual in channel.m_lManualMedias
                          on m.ToString() equals manual.m_sMediaId
                          orderby manual.m_nOrderNum
                          select int.Parse(manual.m_sMediaId);
                }

                medias = ids.ToList();
            }
        }

        protected virtual ApiObjects.SearchObjects.MediaSearchObj GetSearchObject(GroupsCacheManager.Channel channel, ChannelRequest request, int nParentGroupID, ApiObjects.LanguageObj oLanguage, List<string> lPermittedWatchRules)
        {
            int[] nDeviceRuleId = null;
            if (request.m_oFilter != null)
                nDeviceRuleId = ProtocolsFuncs.GetDeviceAllowedRuleIDs(request.m_oFilter.m_sDeviceId, request.m_nGroupID).ToArray();

            return CatalogLogic.BuildBaseChannelSearchObject(channel, request, request.m_oOrderObj, nParentGroupID, channel.m_nGroupID == channel.m_nParentGroupID ? lPermittedWatchRules : null, nDeviceRuleId, oLanguage);
        }
    }
}