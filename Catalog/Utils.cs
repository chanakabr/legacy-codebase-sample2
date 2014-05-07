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

namespace Catalog
{
    public class Utils
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /*Build some of the "where" query part : the range date by the params*/
        static public string GetDateRangeQuery(string sEndDateField, bool bUseStartDate)
        {
            string sQuery = string.Empty;

            if (bUseStartDate)
            {
                sQuery += " start_date <= getdate() and (" + sEndDateField + " >= getdate() or " + sEndDateField + " is null  )";
            }
            else
            {
                sQuery += "(" + sEndDateField + " >= getdate() or " + sEndDateField + " is null  )";
            }

            return sQuery;

        }

        public static string GetWSURL(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        public static string GetLuceneUrl()
        {
            string sLuceneURL = string.Empty;

            try
            {
                sLuceneURL = GetWSURL("LUCENE_WCF");
            }
            catch (Exception ex)
            {
                _logger.Info("Failed getting lucene client url from config", ex);
            }

            return sLuceneURL;
        }

        public static string GetLuceneUrl(int nGroupID)
        {
            return GetLuceneUrl();
        }

        public static string GetSignature(string sSigningString, Int32 nGroupID)
        {
            string retVal;
            //Get key from DB
            // string hmacSecret = ODBCWrapper.Utils.GetTableSingleVal("groups", "Signature" ,"group_id", "=",nGroupID).ToString();
            //string hmacSecret = Utils.GetWSURL("hmacSecret");
            string hmacSecret = GetWSURL("CatalogSignatureKey");
            // The HMAC secret as configured in the skin

            // Values are always transferred using UTF-8 encoding
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            // Calculate the HMAC
            // signingString is the SignString from the request
            HMACSHA1 myhmacsha1 = new HMACSHA1(encoding.GetBytes(hmacSecret));
            retVal = System.Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(sSigningString)));
            myhmacsha1.Clear();
            return retVal;
        }

        /* get only the relevant medias by paging */

        public static List<int> GetMediaForPaging(int[] medias, BaseRequest request)
        {
            List<int> mediaList = new List<int>();
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

                mediaList = medias.ToList<int>().GetRange(startIndex, countItems);

                return mediaList;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return null;
            }
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

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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
            selectQuery.Finish();
            selectQuery = null;
            return bIsMain;
        }

        public static double GetSafeDouble(DataRow dr, string sField)
        {
            double retVal = 0.0;
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                    retVal = double.Parse(dr[sField].ToString());
                return retVal;
            }
            catch
            {
                return 0.0;
            }
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


        internal static MutexSecurity CreateMutex()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            MutexSecurity mutexSecurity = new MutexSecurity();
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.FullControl, AccessControlType.Allow));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.ChangePermissions, AccessControlType.Deny));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.Delete, AccessControlType.Deny));

            return mutexSecurity;

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

        internal static DateTime GetSlidingWindowStart(int minPeriodId)
        {
            switch (minPeriodId)
            {
                case 1:
                    return DateTime.UtcNow.AddMinutes(-1);
                case 5:
                    return DateTime.UtcNow.AddMinutes(-5);
                case 10:
                    return DateTime.UtcNow.AddMinutes(-10);
                case 30:
                    return DateTime.UtcNow.AddMinutes(-30);
                case 60:
                    return DateTime.UtcNow.AddHours(-1);
                case 120:
                    return DateTime.UtcNow.AddHours(-2);
                case 180:
                    return DateTime.UtcNow.AddHours(-3);
                case 360:
                    return DateTime.UtcNow.AddHours(-6);
                case 540:
                    return DateTime.UtcNow.AddHours(-9);
                case 720:
                    return DateTime.UtcNow.AddHours(-12);
                case 1080:
                    return DateTime.UtcNow.AddHours(-18);
                case 1440:
                    return DateTime.UtcNow.AddDays(-1);
                case 2880:
                    return DateTime.UtcNow.AddDays(-2);
                case 4320:
                    return DateTime.UtcNow.AddDays(-3);
                case 7200:
                    return DateTime.UtcNow.AddDays(-5);
                case 10080:
                    return DateTime.UtcNow.AddDays(-7);
                case 20160:
                    return DateTime.UtcNow.AddDays(-14);
                case 30240:
                    return DateTime.UtcNow.AddDays(-21);
                case 40320:
                    return DateTime.UtcNow.AddDays(-28);
                case 40321:
                    return DateTime.UtcNow.AddDays(-28);
                case 43200:
                    return DateTime.UtcNow.AddDays(-30);
                case 44600:
                    return DateTime.UtcNow.AddDays(-31);
                case 1111111:
                    return DateTime.UtcNow.AddMonths(-1);
                case 2222222:
                    return DateTime.UtcNow.AddMonths(-2);
                case 3333333:
                    return DateTime.UtcNow.AddMonths(-3);
                case 4444444:
                    return DateTime.UtcNow.AddMonths(-4);
                case 5555555:
                    return DateTime.UtcNow.AddMonths(-5);
                case 6666666:
                    return DateTime.UtcNow.AddMonths(-6);
                case 9999999:
                    return DateTime.UtcNow.AddMonths(-7);
                case 11111111:
                    return DateTime.UtcNow.AddYears(-1);
                case 22222222:
                    return DateTime.UtcNow.AddYears(-2);
                case 33333333:
                    return DateTime.UtcNow.AddYears(-3);
                case 44444444:
                    return DateTime.UtcNow.AddYears(-4);
                case 55555555:
                    return DateTime.UtcNow.AddYears(-5);
                case 66666666:
                    return DateTime.UtcNow.AddYears(-6);
                case 77777777:
                    return DateTime.UtcNow.AddYears(-7);
                case 88888888:
                    return DateTime.UtcNow.AddYears(-8);
                case 99999999:
                    return DateTime.UtcNow.AddYears(-9);
                case 100000000:
                    return DateTime.UtcNow.AddYears(-10);

                default:
                    return DateTime.MinValue;
            }
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
                    Parallel.ForEach<int>(lMediaIDs, mediaID =>
                    {

                        SearchResult res = searcher.GetDoc(nParentGroupID, mediaID);
                        if (res != null)
                        {
                            itemList.Add(new SearchResult() { assetID = res.assetID, UpdateDate = res.UpdateDate });
                        }
                    }
                        );
                    // tasks.Wait();
                    lMediaRes = itemList.ToList();
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
            DomainDal.GetDomainIDBySiteGuid(nGroupID, (int)lSiteGuid, ref res, ref bIsDomainMaster);

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

        public static bool KeyInGroupTags(int nGroupID, string sTagType)
        {
            bool bRes = false;

            Group group = GroupsCache.Instance.GetGroup(nGroupID);

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
                if (epgList != null && epgList.Count > 0 && epgList[0] != null)
                {
                    int groupID = int.Parse(epgList[0].GROUP_ID);
                    DataTable dtPic = Tvinci.Core.DAL.CatalogDAL.GetPicEpgURL(groupID);
                    if (dtPic != null && dtPic.Rows != null && dtPic.Rows.Count > 0)
                    {
                        sBaseURL = ODBCWrapper.Utils.GetSafeStr(dtPic.Rows[0], "baseURL");
                        if (sBaseURL.Substring(sBaseURL.Length - 1, 1) != "/")
                        {
                            sBaseURL = string.Format("{0}/", sBaseURL);
                        }
                    }

                    foreach (ApiObjects.EPGChannelProgrammeObject oProgram in epgList)
                    {
                        if (!string.IsNullOrEmpty(sBaseURL))
                        {
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
    }
}
