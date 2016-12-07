using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;
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
using KLogMonitor;
using KlogMonitorHelper;
using WS_Users;

namespace Catalog
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
                log.Error(ex.Message, ex);
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
                log.Error(ex.Message, ex);
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

        static public Int64 GetLongSafeVal(DataRow dr, string sField)
        {
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                {
                    return Convert.ToInt64(dr[sField]);
                }
                return 0;
            }
            catch
            {
                return 0;
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
                        if (!dictRes.ContainsKey(mediaID))
                            dictRes.Add(mediaID, new SearchResult());
                    }

                    try
                    {
                        ContextData contextData = new ContextData();
                        Parallel.ForEach<int>(lMediaIDs, mediaID =>
                        {
                            contextData.Load();
                            SearchResult res = new SearchResult()
                            {
                                assetID = mediaID,
                                UpdateDate = DateTime.MinValue
                            };
                            try
                            {
                                res = searcher.GetDoc(nParentGroupID, mediaID);
                                if (res != null)
                                {
                                    dictRes[mediaID] = new SearchResult()
                                    {
                                        assetID = res.assetID,
                                        UpdateDate = res.UpdateDate
                                    };
                                }
                            }
                            catch (Exception ex)
                            {
                                log.ErrorFormat("Failed getting document of media {0}. ex = {1}", mediaID, ex);
                            }
                        }
                        );
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Failed performing parallel GetMediaUpdateDate for group {0}. ex = {1}", nParentGroupID, ex);
                    }

                    //lMediaRes = dictRes.Values.ToList();

                    foreach (var item in lMediaIDs)
                    {
                        lMediaRes.Add(dictRes[item]);
                    }
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

            #region Define Aggregations Query
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

            #endregion

            var aggregatios = new ESBaseAggsItem()
            {
                Name = "channel_views",
                Field = "media_id",
                Type = eElasticAggregationType.terms,
            };

            filteredQuery.Aggregations.Add(aggregatios);

            string aggregationsQuery = filteredQuery.ToString();

            //Search
            string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(nGroupID);
            ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
            string retval = esApi.Search(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref aggregationsQuery);

            if (!string.IsNullOrEmpty(retval))
            {
                //Get aggregation results
                Dictionary<string, Dictionary<string, int>> aggregationsResults = ESAggregationsResult.DeserializeAggrgations<string>(retval);

                if (aggregationsResults != null && aggregationsResults.Count > 0)
                {
                    Dictionary<string, int> aggregationsResult;
                    //retrieve channel_views aggregation results
                    aggregationsResults.TryGetValue("channel_views", out aggregationsResult);

                    if (aggregationsResult != null && aggregationsResult.Count > 0)
                    {
                        foreach (string key in aggregationsResult.Keys)
                        {
                            int count = aggregationsResult[key];

                            int nChannelID;
                            if (int.TryParse(key, out nChannelID))
                            {
                                channelViews.Add(new ChannelViewsResult(nChannelID, count));
                            }
                        }
                    }
                }
            }

            return channelViews;
        }

        public static List<int> SlidingWindowCountAggregations(int nGroupId, List<int> lMediaIds, DateTime dtStartDate, string action)
        {
            List<int> result = new List<int>();

            #region Define Aggregations Query
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

            var aggregations = new ESBaseAggsItem()
            {
                Name = "sliding_window",
                Field = "media_id",
                Type = eElasticAggregationType.terms,
            };

            filteredQuery.Aggregations.Add(aggregations);

            #endregion

            string aggregationsQuery = filteredQuery.ToString();

            //Search
            string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(nGroupId);
            ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
            string retval = esApi.Search(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref aggregationsQuery);

            if (!string.IsNullOrEmpty(retval))
            {
                //Get aggregations results
                Dictionary<string, Dictionary<string, int>> aggregationsResults = ESAggregationsResult.DeserializeAggrgations<string>(retval);

                if (aggregationsResults != null && aggregationsResults.Count > 0)
                {
                    Dictionary<string, int> aggregationResult;
                    //retrieve channel_views aggregations results
                    aggregationsResults.TryGetValue("sliding_window", out aggregationResult);

                    if (aggregationResult != null && aggregationResult.Count > 0)
                    {
                        foreach (string key in aggregationResult.Keys)
                        {
                            int count = aggregationResult[key];

                            int nMediaId;
                            if (int.TryParse(key, out nMediaId))
                            {
                                result.Add(nMediaId);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static List<int> SlidingWindowStatisticsAggregations(int nGroupId, List<int> lMediaIds,
            DateTime dtStartDate, string action, string valueField, AggregationsComparer.eCompareType compareType)
        {
            List<int> result = new List<int>();

            #region Define Aggregations Query
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

            var aggregations = new ESBaseAggsItem()
            {
                Name = "sliding_window",
                Field = "media_id",
                Type = eElasticAggregationType.terms,
            };

            aggregations.SubAggrgations.Add(new ESBaseAggsItem()
            {
                Name = "sub_stats",
                Type = eElasticAggregationType.stats,
                Field = Catalog.STAT_ACTION_RATE_VALUE_FIELD
            });

            filteredQuery.Aggregations.Add(aggregations);

            #endregion

            string aggregationsQuery = filteredQuery.ToString();


            //Search
            string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(nGroupId);
            ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
            string retval = esApi.Search(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref aggregationsQuery);

            if (!string.IsNullOrEmpty(retval))
            {
                //Get aggregations results
                Dictionary<string, List<StatisticsAggregationResult>> statisticsResults = 
                    ESAggregationsResult.DeserializeStatisticsAggregations(retval, "sub_stats");

                if (statisticsResults != null && statisticsResults.Count > 0)
                {
                    List<StatisticsAggregationResult> aggregationResults;
                    //retrieve channel_views aggregations results
                    statisticsResults.TryGetValue("sliding_window", out aggregationResults);

                    if (aggregationResults != null && aggregationResults.Count > 0)
                    {
                        int mediaId;

                        aggregationResults.Sort(new AggregationsComparer(compareType));

                        foreach (var stats in aggregationResults)
                        {
                            if (int.TryParse(stats.key, out mediaId))
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
                log.Error("", ex);
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
            UsersService u = null;
            try
            {
                u = new UsersService();
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
                log.Error("Exception - " + string.Format("Failed to obtain user type. Site Guid: {0} , Ex Msg: {1} , Stack Trace: {2}", sSiteGuid, ex.Message, ex.StackTrace), ex);
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

        public static void BuildMediaFromDataSet(ref Dictionary<int, Dictionary<int, Media>> mediaTranslations,
            ref Dictionary<int, Media> medias, Group group, DataSet dataSet)
        {
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        Media media = new Media();
                        if (dataSet.Tables[0].Columns != null && dataSet.Tables[0].Rows != null)
                        {
                            #region media info
                            media.m_nMediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                            media.m_nWPTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "watch_permission_type_id");
                            media.m_nMediaTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_type_id");
                            media.m_nGroupID = ODBCWrapper.Utils.GetIntSafeVal(row, "group_id");
                            media.m_nIsActive = ODBCWrapper.Utils.GetIntSafeVal(row, "is_active");
                            media.m_nDeviceRuleId = ODBCWrapper.Utils.GetIntSafeVal(row, "device_rule_id");
                            media.m_nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(row, "like_counter");
                            media.m_nViews = ODBCWrapper.Utils.GetIntSafeVal(row, "views");
                            media.m_sUserTypes = ODBCWrapper.Utils.GetSafeStr(row["user_types"]);

                            double dSum = ODBCWrapper.Utils.GetDoubleSafeVal(row, "votes_sum");
                            double dCount = ODBCWrapper.Utils.GetDoubleSafeVal(row, "votes_count");

                            if (dCount > 0)
                            {
                                media.m_nVotes = (int)dCount;
                                media.m_dRating = dSum / dCount;
                            }

                            media.m_sName = ODBCWrapper.Utils.GetSafeStr(row, "name");
                            media.m_sDescription = ODBCWrapper.Utils.GetSafeStr(row, "description");

                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "create_date")))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "create_date");
                                media.m_sCreateDate = dt.ToString("yyyyMMddHHmmss");
                            }
                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "update_date")))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "update_date");
                                media.m_sUpdateDate = dt.ToString("yyyyMMddHHmmss");
                            }
                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "start_date")))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "start_date");
                                media.m_sStartDate = dt.ToString("yyyyMMddHHmmss");
                            }

                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "end_date")))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "end_date");
                                media.m_sEndDate = dt.ToString("yyyyMMddHHmmss");
                            }

                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "final_end_date")))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "final_end_date");
                                media.m_sFinalEndDate = dt.ToString("yyyyMMddHHmmss");
                            }

                            media.geoBlockRule = ODBCWrapper.Utils.ExtractInteger(row, "geo_block_rule_id");

                            #endregion

                            #region - get all metas by groupId
                            Dictionary<string, string> dMetas;
                            //Get Meta - MetaNames (e.g. will contain key/value <META1_STR, show>)
                            if (group.m_oMetasValuesByGroupId.TryGetValue(media.m_nGroupID, out dMetas))
                            {
                                foreach (string sMeta in dMetas.Keys)
                                {
                                    //Retreive meta name and check that it is not null or empty so that it will not form an invalid field later on
                                    string sMetaName;
                                    dMetas.TryGetValue(sMeta, out sMetaName);

                                    if (!string.IsNullOrEmpty(sMetaName))
                                    {
                                        string sMetaValue = ODBCWrapper.Utils.GetSafeStr(row[sMeta]);
                                        media.m_dMeatsValues.Add(sMetaName, sMetaValue);
                                    }
                                }
                            }
                        }
                        medias.Add(media.m_nMediaID, media);
                            #endregion
                    }

                    #region - get all the media files types for each mediaId that have been selected.
                    if (dataSet.Tables[1].Columns != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count > 0)
                    {
                        foreach (DataRow row in dataSet.Tables[1].Rows)
                        {
                            int mediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                            string sMFT = ODBCWrapper.Utils.GetSafeStr(row, "media_type_id");
                            bool isFree = ODBCWrapper.Utils.ExtractBoolean(row, "is_free");

                            Media theMedia = medias[mediaID];

                            theMedia.m_sMFTypes += string.Format("{0};", sMFT);

                            int mediaTypeId;

                            if (isFree)
                            {
                                // if at least one of the media types is free - this media is free
                                theMedia.isFree = true;

                                if (int.TryParse(sMFT, out mediaTypeId))
                                {
                                    theMedia.freeFileTypes.Add(mediaTypeId);
                                }
                            }
                        }
                    }
                    #endregion

                    #region - get regions of media

                    // Regions table should be 6h on stored procedure
                    if (dataSet.Tables.Count > 5 && dataSet.Tables[5].Columns != null && dataSet.Tables[5].Rows != null)
                    {
                        foreach (DataRow mediaRegionRow in dataSet.Tables[5].Rows)
                        {
                            int mediaId = ODBCWrapper.Utils.ExtractInteger(mediaRegionRow, "MEDIA_ID");
                            int regionId = ODBCWrapper.Utils.ExtractInteger(mediaRegionRow, "REGION_ID");

                            // Accumulate region ids in list
                            medias[mediaId].regions.Add(regionId);
                        }
                    }

                    // If no regions were found for this media - use 0, that indicates that the media is region-less
                    foreach (Media media in medias.Values)
                    {
                        if (media.regions.Count == 0)
                        {
                            media.regions.Add(0);
                        }
                    }


                    #endregion

                    #region - get all media tags
                    if (dataSet.Tables[2].Columns != null && dataSet.Tables[2].Rows != null && dataSet.Tables[2].Rows.Count > 0)
                    {
                        foreach (DataRow row in dataSet.Tables[2].Rows)
                        {
                            int nTagMediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                            int mttn = ODBCWrapper.Utils.GetIntSafeVal(row, "tag_type_id");
                            string val = ODBCWrapper.Utils.GetSafeStr(row, "value");
                            long tagID = ODBCWrapper.Utils.GetLongSafeVal(row, "tag_id");

                            try
                            {
                                if (group.m_oGroupTags.ContainsKey(mttn))
                                {
                                    string sTagName = group.m_oGroupTags[mttn];

                                    if (!string.IsNullOrEmpty(sTagName))
                                    {
                                        if (!medias[nTagMediaID].m_dTagValues.ContainsKey(sTagName))
                                        {
                                            medias[nTagMediaID].m_dTagValues.Add(sTagName, new Dictionary<long, string>());
                                        }

                                        if (!medias[nTagMediaID].m_dTagValues[sTagName].ContainsKey(tagID))
                                        {
                                            medias[nTagMediaID].m_dTagValues[sTagName].Add(tagID, val);
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                log.ErrorFormat("Error - Caught exception when trying to add media to group tags. TagMediaId={0}; TagTypeID={1}; TagID={2}; TagValue={3}", nTagMediaID, mttn, tagID, val);
                            }
                        }
                    }
                    #endregion

                    #region Clone medias to all translated languages
                    foreach (int mediaID in medias.Keys)
                    {
                        Media media = medias[mediaID];

                        Dictionary<int, Media> tempMediaTrans = new Dictionary<int, Media>();
                        foreach (ApiObjects.LanguageObj oLanguage in group.GetLangauges())
                        {
                            tempMediaTrans.Add(oLanguage.ID, media.Clone());
                        }

                        mediaTranslations.Add(mediaID, tempMediaTrans);

                    }
                    #endregion

                    #region get all translated metas and media info

                    if (dataSet.Tables[3].Columns != null && dataSet.Tables[3].Rows != null && dataSet.Tables[3].Rows.Count > 0)
                    {
                        Dictionary<string, string> dMetas;

                        foreach (DataRow row in dataSet.Tables[3].Rows)
                        {
                            int mediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "MEDIA_ID");
                            int nLanguageID = ODBCWrapper.Utils.GetIntSafeVal(row, "LANGUAGE_ID");

                            if (mediaTranslations.ContainsKey(mediaID) && mediaTranslations[mediaID].ContainsKey(nLanguageID))
                            {
                                Media oMedia = mediaTranslations[mediaID][nLanguageID];

                                if (group.m_oMetasValuesByGroupId.TryGetValue(oMedia.m_nGroupID, out dMetas))
                                {
                                    #region get media translated name
                                    string sTransName = ODBCWrapper.Utils.GetSafeStr(row, "NAME");

                                    if (!string.IsNullOrEmpty(sTransName))
                                        oMedia.m_sName = sTransName;
                                    #endregion

                                    #region get media translated description
                                    string sTransDesc = ODBCWrapper.Utils.GetSafeStr(row, "DESCRIPTION");

                                    if (!string.IsNullOrEmpty(sTransDesc))
                                        oMedia.m_sDescription = sTransDesc;
                                    #endregion

                                    #region get media translated metas
                                    foreach (string sMeta in dMetas.Keys)
                                    {
                                        //if meta is a string, then get translated value from DB, for all other metas, we keep the same values as there's no translation
                                        if (sMeta.EndsWith("_STR"))
                                        {
                                            string sMetaName;
                                            dMetas.TryGetValue(sMeta, out sMetaName);

                                            if (!string.IsNullOrEmpty(sMetaName))
                                            {
                                                string sMetaValue = ODBCWrapper.Utils.GetSafeStr(row, sMeta);

                                                if (!string.IsNullOrEmpty(sMetaValue))
                                                {
                                                    oMedia.m_dMeatsValues[sMetaName] = sMetaValue;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                    #endregion

                    #region - get all translated media tags
                    if (dataSet.Tables[4].Columns != null && dataSet.Tables[4].Rows != null && dataSet.Tables[4].Rows.Count > 0)
                    {
                        foreach (DataRow row in dataSet.Tables[4].Rows)
                        {
                            int nTagMediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                            int mttn = ODBCWrapper.Utils.GetIntSafeVal(row, "tag_type_id");
                            string val = ODBCWrapper.Utils.GetSafeStr(row, "translated_value");
                            int nLangID = ODBCWrapper.Utils.GetIntSafeVal(row, "language_id");
                            long tagID = ODBCWrapper.Utils.GetLongSafeVal(row, "tag_id");

                            if (group.m_oGroupTags.ContainsKey(mttn) && !string.IsNullOrEmpty(val))
                            {
                                Media oMedia;

                                if (mediaTranslations.ContainsKey(nTagMediaID) && mediaTranslations[nTagMediaID].ContainsKey(nLangID))
                                {
                                    oMedia = mediaTranslations[nTagMediaID][nLangID];
                                    string sTagTypeName = group.m_oGroupTags[mttn];

                                    if (oMedia.m_dTagValues.ContainsKey(sTagTypeName))
                                    {
                                        oMedia.m_dTagValues[sTagTypeName][tagID] = val;
                                    }
                                    else
                                    {
                                        Dictionary<long, string> dTemp = new Dictionary<long, string>();
                                        dTemp[tagID] = val;
                                        oMedia.m_dTagValues[sTagTypeName] = dTemp;
                                    }
                                }
                            }
                        }
                    }

                    #endregion
                }

            }
        }

        /// <summary>
        /// Finds the country name of a given ip, using special elastic search index
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string GetCountryNameByIp(string ip)
        {
            string result = string.Empty;

            int countryId = ElasticSearch.Utilities.IpToCountry.GetCountryByIp(ip);

            object value = ODBCWrapper.Utils.GetTableSingleVal("countries", "country_name", countryId);

            if (value != DBNull.Value)
            {
                result = Convert.ToString(value);
            }

            return result;
        }

        internal static Dictionary<string, long> GetEpgChannelIdToLinearMediaIdMap(int groupId, List<string> epgChannelIds)
        {
            Dictionary<string, long> epgChannelIdToLinearMediaIdMap = new Dictionary<string, long>();
            DataTable dt = CatalogDAL.GetEpgChannelIdToLinearMediaIdMap(groupId, epgChannelIds);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    long channelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "id", 0);
                    long linearMediaId = ODBCWrapper.Utils.GetLongSafeVal(dr, "media_id", 0);
                    if (channelId > 0 && linearMediaId > 0 && !epgChannelIdToLinearMediaIdMap.ContainsKey(channelId.ToString()))
                    {
                        epgChannelIdToLinearMediaIdMap.Add(channelId.ToString(), linearMediaId);
                    }
                }
            }

            return epgChannelIdToLinearMediaIdMap;
        }

    }
}
