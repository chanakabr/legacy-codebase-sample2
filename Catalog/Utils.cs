using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;
using Logger;
using System.Reflection;
using System.Configuration;
using System.Security.Cryptography;
using System.Data;
using System.Security.AccessControl;
using System.Security.Principal;
using ApiObjects.SearchObjects;
using Tvinci.Core.DAL;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DAL;
using ElasticSearch.Searcher;
using Catalog.Cache;
using GroupsCacheManager;
using ApiObjects;
using Catalog.Request;

namespace Catalog
{
    public class Utils
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public const int DEFAULT_CATALOG_LOG_THRESHOLD_MILLISEC = 500; // half a second

        public static string GetWSURL(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }


        public static string GetSignature(string sSigningString, Int32 nGroupID)
        {
            string retVal;

            string hmacSecret = GetWSURL("CatalogSignatureKey");

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            using (HMACSHA1 myhmacsha1 = new HMACSHA1(encoding.GetBytes(hmacSecret)))
            {
                retVal = System.Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(sSigningString)));
            }
            return retVal;
        }


        public static List<SearchResult> GetMediaForPaging(List<SearchResult> medias, BaseRequest request)
        {
            List<SearchResult> mediaList = new List<SearchResult>();
            try
            {
                if (medias.Count() == 0)
                    return mediaList;

                int startIndex = 0;
                int countItems = medias.Count();

                if (request.m_nPageIndex > 0)
                    startIndex = request.m_nPageIndex * request.m_nPageSize;//first page index = 0

                if (request.m_nPageSize > 0)
                    countItems = request.m_nPageSize;
                else
                    countItems = medias.Count() - startIndex;

                if (medias.Count() < startIndex)
                    return mediaList;

                if ((startIndex + countItems) > medias.Count())
                    countItems = medias.Count() - startIndex;

                mediaList = medias.ToList<SearchResult>().GetRange(startIndex, countItems);

                return mediaList;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return null;
            }
        }

        public static bool GetPagingValues(int nRowCount, int nPageIndex, int nPageSize, ref int startIndex, ref int count)
        {
            try
            {
                if (nRowCount == 0)
                {
                    return false;
                }

                startIndex = 0;
                count = nRowCount;

                if (nPageIndex > 0)
                    startIndex = nPageIndex * nPageSize;//first page index = 0

                if (nPageSize > 0)
                    count = nPageSize;
                else
                    count = nRowCount - startIndex;

                if (nRowCount < startIndex)
                {
                    startIndex = 0;
                    count = nRowCount;
                }


                if ((startIndex + count) > nRowCount)
                    count = nRowCount - startIndex;

                return true;

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return false;
            }
        }

        public static bool IsLangMain(int nGroupID, int nLanguage)
        {
            bool bIsMain = true;

            if (nLanguage == 0)
                return bIsMain;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select g.LANGUAGE_ID from groups g (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        Int32 nMainLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["LANGUAGE_ID"].ToString());
                        if (nLanguage == nMainLangID)
                            bIsMain = true;
                        else
                            bIsMain = false;
                    }
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return bIsMain;
        }

        public static string GetStrSafeVal(DataRow dr, string sField)
        {
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                    return dr[sField].ToString();
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static int GetIntSafeVal(DataRow dr, string sField)
        {
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                    return int.Parse(dr[sField].ToString());
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// This function gets the number of items in a collection, a page index and a range. It checks whether the range of items is valid among the
        /// the nNumOfMedias. 
        /// </summary>
        /// <param name="nNumOfMedias">Number of items in a collection</param>
        /// <param name="nPageIndex">The requested page index</param>
        /// <param name="nValidRange">The range of items request within a page index (updated if required)</param>
        /// <returns>True if valid, false if not valid</returns>
        internal static bool ValidatePageSizeAndPageIndexAgainstNumberOfMedias(int nNumOfMedias, int nPageIndex, ref int nValidRange)
        {
            bool bIsValidRange = false;
            if (nValidRange > 0 || nPageIndex > 0)
            {
                int nSizePageIndexMultiplicity = nPageIndex * nValidRange;
                if (nSizePageIndexMultiplicity < nNumOfMedias)
                {
                    if (nNumOfMedias - nSizePageIndexMultiplicity < nValidRange)
                    {
                        nValidRange = nNumOfMedias - nSizePageIndexMultiplicity;
                    }

                    bIsValidRange = true;
                }
            }
            else if (nValidRange == 0 && nPageIndex == 0)   // Returning all items in collection
            {
                bIsValidRange = true;
            }

            return bIsValidRange;
        }

        public static void OrderMediasByStats(List<int> medias, int nOrderType, int nOrderDirection)
        {
            if (medias.Count > 0)
            {
                DataTable dt = CatalogDAL.Get_OrderedMediaIdList(medias, nOrderType, nOrderDirection);

                if (dt != null && dt.Rows.Count > 0)
                {
                    medias.Clear();
                    medias.AddRange(dt.AsEnumerable().Select(dr => ODBCWrapper.Utils.GetIntSafeVal(dr["ID"])));
                }
            }
        }



        //This method is used specifically for Lucene cases when we get a search result which does not consist of an update date (Lucene does not hold update_date
        //within its documents and therefore we need to go to the DB and return the media update date
        public static List<SearchResult> GetMediaUpdateDate(List<ApiObjects.SearchObjects.SearchResult> lSearchResults)
        {
            ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();
            List<SearchResult> lMediaRes = new List<SearchResult>();
            if (lSearchResults != null && lSearchResults.Count > 0)
            {
                if (searcher.GetType().Equals(typeof(LuceneWrapper)))
                {
                    List<int> mediaIds = lSearchResults.Select(item => item.assetID).ToList();

                    DataTable dt = CatalogDAL.Get_MediaUpdateDate(mediaIds);

                    SearchResult oMediaRes = new SearchResult();
                    if (dt != null)
                    {
                        if (dt.Columns != null)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                oMediaRes.assetID = Utils.GetIntSafeVal(dt.Rows[i], "ID");
                                if (!string.IsNullOrEmpty(dt.Rows[i]["UPDATE_DATE"].ToString()))
                                {
                                    oMediaRes.UpdateDate = System.Convert.ToDateTime(dt.Rows[i]["UPDATE_DATE"].ToString());
                                }
                                lMediaRes.Add(oMediaRes);
                                oMediaRes = new SearchResult();
                            }
                        }
                    }
                }
                else
                {
                    lMediaRes = lSearchResults.Select(item => new SearchResult() { assetID = item.assetID, UpdateDate = item.UpdateDate }).ToList();
                }
            }

            return lMediaRes;
        }

        public static List<SearchResult> GetMediaUpdateDate(int nParentGroupID, List<int> lMediaIDs)
        {
            List<SearchResult> lMediaRes = new List<SearchResult>();

            if (lMediaIDs == null || lMediaIDs.Count == 0)
                return lMediaRes;

            ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

            if (searcher != null)
            {

                if (searcher.GetType().Equals(typeof(LuceneWrapper)))
                {
                    DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);

                    lMediaRes = GetMediaUpdateDate(lMediaIDs.Select(id => new SearchResult() { assetID = id, UpdateDate = dt }).ToList());
                }
                else
                {
                    ConcurrentBag<SearchResult> itemList = new ConcurrentBag<SearchResult>();
                    Dictionary<int, SearchResult> dictRes = new Dictionary<int, SearchResult>();
                    foreach (int mediaID in lMediaIDs)
                    {
                        dictRes.Add(mediaID, new SearchResult());
                    }
                    Parallel.ForEach<int>(lMediaIDs, mediaID =>
                    {

                        SearchResult res = searcher.GetDoc(nParentGroupID, mediaID);
                        if (res != null)
                        {
                            dictRes[mediaID] = new SearchResult() { assetID = res.assetID, UpdateDate = res.UpdateDate };
                        }
                    }
                        );

                    lMediaRes = dictRes.Values.ToList();

                }
            }


            return lMediaRes;

        }

        public static bool IsGroupIDContainedInConfig(long lGroupID, string sKey, char cSeperator)
        {
            bool res = false;
            string rawStrFromConfig = GetWSURL(sKey);
            if (rawStrFromConfig.Length > 0)
            {
                string[] strArrOfIDs = rawStrFromConfig.Split(cSeperator);
                if (strArrOfIDs != null && strArrOfIDs.Length > 0)
                {
                    List<long> listOfIDs = strArrOfIDs.Select(s =>
                    {
                        long l = 0;
                        if (Int64.TryParse(s, out l))
                            return l;
                        return 0;
                    }).ToList();

                    res = listOfIDs.Contains(lGroupID);
                }
            }

            return res;
        }

        public static int GetOperatorIDBySiteGuid(int nGroupID, long lSiteGuid)
        {
            int res = 0;
            bool bIsDomainMaster = false;
            DomainSuspentionStatus eSuspendStat = DomainSuspentionStatus.OK;
            DomainDal.GetDomainIDBySiteGuid(nGroupID, (int)lSiteGuid, ref res, ref bIsDomainMaster, ref eSuspendStat);

            return res;

        }

        public static string GetESTypeByLanguage(string sType, ApiObjects.LanguageObj oLanguage)
        {
            string sResult;

            if (oLanguage != null && !oLanguage.IsDefault)
            {
                sResult = string.Concat(sType, "_", oLanguage.Code);
            }
            else
            {
                sResult = sType;
            }

            return sResult;
        }

        internal static List<ChannelViewsResult> GetChannelViewsResult(int nGroupID, List<Int32> nMediaTypes)
        {
            List<ChannelViewsResult> channelViews = new List<ChannelViewsResult>();

            #region Define Facet Query
            ElasticSearch.Searcher.FilteredQuery filteredQuery = new ElasticSearch.Searcher.FilteredQuery() { PageIndex = 0, PageSize = 1 };
            filteredQuery.Filter = new ElasticSearch.Searcher.QueryFilter();

            BaseFilterCompositeType filter = new FilterCompositeType(CutWith.AND);
            filter.AddChild(new ESTerm(true) { Key = "group_id", Value = nGroupID.ToString() });

            #region define date filter
            ESRange dateRange = new ESRange(false) { Key = "action_date" };
            string sMax = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            double csts = GetDoubleValFromConfig("CrowdSourceTimeSpan");
            if (csts == 0)
                csts = 30.0;

            string sMin = DateTime.UtcNow.AddSeconds(-1 * csts).ToString("yyyyMMddHHmmss");
            dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
            dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sMax));
            filter.AddChild(dateRange);
            #endregion

            #region define action filter
            ESTerms esActionTerm = new ESTerms(false) { Key = "action" };
            esActionTerm.Value.Add("mediahit");
            filter.AddChild(esActionTerm);
            #endregion

            #region define media Type filter
            if (nMediaTypes != null && nMediaTypes.Count > 0)
            {
                ESTerms esMediaTypeTerms = new ESTerms(true) { Key = "media_type" };
                esMediaTypeTerms.Value.AddRange(nMediaTypes.Select(item => item.ToString()));
                filter.AddChild(esMediaTypeTerms);
            }
            #endregion

            filteredQuery.Filter.FilterSettings = filter;

            ESTermsFacet facet = new ESTermsFacet("channel_views", "media_id", 100000);
            facet.Query = filteredQuery;
            #endregion

            string sFacetQuery = facet.ToString();

            //Search
            string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(nGroupID);
            ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
            string retval = esApi.Search(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref sFacetQuery);

            if (!string.IsNullOrEmpty(retval))
            {
                //Get facet results
                Dictionary<string, Dictionary<string, int>> dFacets = ESTermsFacet.FacetResults(ref retval);

                if (dFacets != null && dFacets.Count > 0)
                {
                    Dictionary<string, int> dFacetResult;
                    //retrieve channel_views facet results
                    dFacets.TryGetValue("channel_views", out dFacetResult);

                    if (dFacetResult != null && dFacetResult.Count > 0)
                    {
                        foreach (string sFacetKey in dFacetResult.Keys)
                        {
                            int count = dFacetResult[sFacetKey];

                            int nChannelID;
                            if (int.TryParse(sFacetKey, out nChannelID))
                            {
                                channelViews.Add(new ChannelViewsResult(nChannelID, count));
                            }
                        }
                    }
                }
            }

            return channelViews;
        }

        public static List<int> SlidingWindowCountFacet(int nGroupId, List<int> lMediaIds, DateTime dtStartDate, string action)
        {
            List<int> result = new List<int>();

            #region Define Facet Query
            ElasticSearch.Searcher.FilteredQuery filteredQuery = new ElasticSearch.Searcher.FilteredQuery() { PageIndex = 0, PageSize = 1 };
            filteredQuery.Filter = new ElasticSearch.Searcher.QueryFilter();

            BaseFilterCompositeType filter = new FilterCompositeType(CutWith.AND);
            filter.AddChild(new ESTerm(true) { Key = "group_id", Value = nGroupId.ToString() });

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

            ESTermsFacet facet = new ESTermsFacet("sliding_window", "media_id", 100000);
            facet.Query = filteredQuery;
            #endregion

            string sFacetQuery = facet.ToString();


            //Search
            string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(nGroupId);
            ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
            string retval = esApi.Search(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref sFacetQuery);

            if (!string.IsNullOrEmpty(retval))
            {
                //Get facet results
                Dictionary<string, Dictionary<string, int>> dFacets = ESTermsFacet.FacetResults(ref retval);

                if (dFacets != null && dFacets.Count > 0)
                {
                    Dictionary<string, int> dFacetResult;
                    //retrieve channel_views facet results
                    dFacets.TryGetValue("sliding_window", out dFacetResult);

                    if (dFacetResult != null && dFacetResult.Count > 0)
                    {
                        foreach (string sFacetKey in dFacetResult.Keys)
                        {
                            int count = dFacetResult[sFacetKey];

                            int nMediaId;
                            if (int.TryParse(sFacetKey, out nMediaId))
                            {
                                result.Add(nMediaId);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static List<int> SlidingWindowStatisticsFacet(int nGroupId, List<int> lMediaIds, DateTime dtStartDate, string action, string valueField, ESTermsStatsFacet.FacetCompare.eCompareType compareType)
        {
            List<int> result = new List<int>();

            #region Define Facet Query
            ElasticSearch.Searcher.FilteredQuery filteredQuery = new ElasticSearch.Searcher.FilteredQuery() { PageIndex = 0, PageSize = 1 };
            filteredQuery.Filter = new ElasticSearch.Searcher.QueryFilter();

            BaseFilterCompositeType filter = new FilterCompositeType(CutWith.AND);
            filter.AddChild(new ESTerm(true) { Key = "group_id", Value = nGroupId.ToString() });

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

            ESTermsStatsFacet facet = new ESTermsStatsFacet("sliding_window", "media_id", valueField, 100000);
            facet.Query = filteredQuery;
            #endregion

            string sFacetQuery = facet.ToString();


            //Search
            string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(nGroupId);
            ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
            string retval = esApi.Search(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref sFacetQuery);

            if (!string.IsNullOrEmpty(retval))
            {
                //Get facet results
                Dictionary<string, List<ESTermsStatsFacet.StatisticFacetResult>> dFacets = ESTermsStatsFacet.FacetResults(ref retval);

                if (dFacets != null && dFacets.Count > 0)
                {
                    List<ESTermsStatsFacet.StatisticFacetResult> lFacetResult;
                    //retrieve channel_views facet results
                    dFacets.TryGetValue("sliding_window", out lFacetResult);

                    if (lFacetResult != null && lFacetResult.Count > 0)
                    {
                        int mediaId;

                        lFacetResult.Sort(new ESTermsStatsFacet.FacetCompare(compareType));

                        foreach (var stats in lFacetResult)
                        {
                            if (int.TryParse(stats.term, out mediaId))
                            {
                                result.Add(mediaId);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static bool KeyInGroupTags(int nGroupID, string sTagType)
        {
            bool bRes = false;

            GroupManager groupManager = new GroupManager();
            Group group = groupManager.GetGroup(nGroupID);

            if (group != null)
            {
                if (group.m_oGroupTags.ContainsValue(sTagType))
                {
                    bRes = true;
                }
            }

            return bRes;
        }

        internal static List<ApiObjects.EPGChannelProgrammeObject> CompleteFullEpgPicURL(List<ApiObjects.EPGChannelProgrammeObject> epgList)
        {
            try
            {
                string sBaseURL = string.Empty;
                string sWidth = string.Empty;
                string sHeight = string.Empty;
                if (epgList != null && epgList.Count > 0 && epgList[0] != null)
                {
                    int groupID = int.Parse(epgList[0].GROUP_ID);
                    DataTable dtPic = Tvinci.Core.DAL.CatalogDAL.GetPicEpgURL(groupID);
                    if (dtPic != null && dtPic.Rows != null && dtPic.Rows.Count > 0)
                    {
                        sBaseURL = ODBCWrapper.Utils.GetSafeStr(dtPic.Rows[0], "baseURL");
                        sWidth = ODBCWrapper.Utils.GetSafeStr(dtPic.Rows[0], "WIDTH");
                        sHeight = ODBCWrapper.Utils.GetSafeStr(dtPic.Rows[0], "HEIGHT");
                        if (sBaseURL.Substring(sBaseURL.Length - 1, 1) != "/")
                        {
                            sBaseURL = string.Format("{0}/", sBaseURL);
                        }
                    }

                    foreach (ApiObjects.EPGChannelProgrammeObject oProgram in epgList)
                    {
                        if (oProgram != null && !string.IsNullOrEmpty(sBaseURL) && !string.IsNullOrEmpty(oProgram.PIC_URL))
                        {
                            if (!string.IsNullOrEmpty(sWidth) && !string.IsNullOrEmpty(sHeight))
                            {
                                oProgram.PIC_URL = oProgram.PIC_URL.Replace(".", string.Format("_{0}X{1}.", sWidth, sHeight));
                            }
                            oProgram.PIC_URL = string.Format("{0}{1}", sBaseURL, oProgram.PIC_URL);
                        }
                    }
                }
                return epgList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static double GetDoubleValFromConfig(string sKey)
        {
            double nRes = 0;
            if (TVinciShared.WS_Utils.GetTcmConfigValue(sKey) != string.Empty)
            {
                double.TryParse(TVinciShared.WS_Utils.GetTcmConfigValue(sKey), out nRes);
            }
            return nRes;
        }

        public static List<T> ListPaging<T>(List<T> list, int nPageSize, int nPageIndex)
        {
            List<T> result = new List<T>();

            if (list != null && list.Count > 0)
            {
                int skip = nPageIndex * nPageSize;

                if (list.Count > skip)
                {
                    result = (list.Count) > (skip + nPageSize) ? list.Skip(skip).Take(nPageSize).ToList() : list.Skip(skip).ToList();
                }
            }

            return result;
        }

        internal static int GetUserType(string sSiteGuid, int nGroupID)
        {
            int nUserTypeID = 0;
            ws_users.UsersService u = null;
            try
            {
                u = new ws_users.UsersService();
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL.Length > 0)
                    u.Url = sWSURL;

                //get username + password from wsCache
                Credentials oCredentials = TvinciCache.WSCredentials.GetWSCredentials(ApiObjects.eWSModules.CATALOG, nGroupID, ApiObjects.eWSModules.USERS);
                if (oCredentials != null)
                {
                    nUserTypeID = u.GetUserType(oCredentials.m_sUsername, oCredentials.m_sPassword, sSiteGuid);
                }

                return nUserTypeID;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", string.Format("Failed to obtain user type. Site Guid: {0} , Ex Msg: {1} , Stack Trace: {2}", sSiteGuid, ex.Message, ex.StackTrace), "GetUserType");
                return 0;
            }
            finally
            {
                if (u != null)
                {
                    u.Dispose();
                }
            }
        }

        public static int GetCatalogLogThreshold()
        {
            int res = 0;
            string configOverride = GetWSURL("LOG_THRESHOLD");
            if (!string.IsNullOrEmpty(configOverride) && Int32.TryParse(configOverride, out res) && res > 0)
                return res;
            return DEFAULT_CATALOG_LOG_THRESHOLD_MILLISEC;
        }

    }
}
