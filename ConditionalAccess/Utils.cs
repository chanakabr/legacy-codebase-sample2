using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Security.Cryptography;
using ConditionalAccess.TvinciPricing;
using System.Web;
using Tvinic.GoogleAPI;
using System.Data;
using System.Xml;
using DAL;
using ConditionalAccess.TvinciUsers;

namespace ConditionalAccess
{
    public class Utils
    {


        internal const double DEFAULT_MIN_PRICE_FOR_PREVIEW_MODULE = 0.2;
        public const int DEFAULT_MPP_RENEW_FAIL_COUNT = 10; // to be group specific override this value in the 
        // table groups_parameters, column FAIL_COUNT under ConditionalAccess DB.

        private static readonly string BASIC_LINK_TICK_TIME = "!--tick_time--";
        private static readonly string BASIC_LINK_COUNTRY_CODE = "!--COUNTRY_CD--";
        private static readonly string BASIC_LINK_HASH = "!--hash--";
        private static readonly string BASIC_LINK_GROUP = "!--group--";
        private static readonly string BASIC_LINK_CONFIG_DATA = "!--config_data--";


        static public void GetBaseConditionalAccessImpl(ref ConditionalAccess.BaseConditionalAccess t, Int32 nGroupID)
        {
            GetBaseConditionalAccessImpl(ref t, nGroupID, "");
        }

        static public void GetBaseConditionalAccessImpl(ref ConditionalAccess.BaseConditionalAccess t, Int32 nGroupID, string sConnKey)
        {
            Int32 nImplID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                if (sConnKey.Length > 0)
                    selectQuery.SetConnectionKey(sConnKey);
                selectQuery += "select implementation_id from groups_modules_implementations where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 1);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
                    }
                }

                if (nImplID == 1)
                    t = new ConditionalAccess.TvinciConditionalAccess(nGroupID, sConnKey);
                if (nImplID == 4)
                    t = new ConditionalAccess.FilmoConditionalAccess(nGroupID, sConnKey);
                if (nImplID == 6)
                    t = new ConditionalAccess.ElisaConditionalAccess(nGroupID, sConnKey);
                if (nImplID == 7)
                    t = new ConditionalAccess.EutelsatConditionalAccess(nGroupID, sConnKey);
                if (nImplID == 9)
                    t = new ConditionalAccess.CinepolisConditionalAccess(nGroupID, sConnKey);
            }
            catch (Exception ex)
            {



            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                    selectQuery = null;
                }
            }
        }

        static public BaseCampaignActionImpl GetCampaignActionByType(TvinciPricing.CampaignResult result)
        {
            BaseCampaignActionImpl retVal = null;
            switch (result)
            {
                case TvinciPricing.CampaignResult.Voucher:
                    {
                        retVal = new VoucherCampaignImpl();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return retVal;
        }


        static public BaseCampaignActionImpl GetCampaignActionByTriggerType(TvinciPricing.CampaignTrigger trigger)
        {
            BaseCampaignActionImpl retVal = null;
            switch (trigger)
            {
                case TvinciPricing.CampaignTrigger.Purchase:
                    {
                        retVal = new VoucherCampaignImpl();
                        break;
                    }
                case TvinciPricing.CampaignTrigger.SocialInvite:
                    {
                        retVal = new SocialInviteCampaignImpl();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return retVal;
        }

        static public TvinciPricing.Price GetPriceAfterDiscount(TvinciPricing.Price price, TvinciPricing.DiscountModule disc, Int32 nUseTime)
        {
            TvinciPricing.Price discRetPrice = new ConditionalAccess.TvinciPricing.Price();
            discRetPrice = price;
            if (disc.m_dEndDate < DateTime.UtcNow ||
                disc.m_dStartDate > DateTime.UtcNow)
                return price;

            TvinciPricing.WhenAlgo whenAlgo = disc.m_oWhenAlgo;
            if (whenAlgo.m_eAlgoType == TvinciPricing.WhenAlgoType.N_FIRST_TIMES && whenAlgo.m_nNTimes != 0 &&
                nUseTime >= whenAlgo.m_nNTimes)
                return price;

            if (whenAlgo.m_eAlgoType == TvinciPricing.WhenAlgoType.EVERY_N_TIMES && whenAlgo.m_nNTimes != 0 &&
                (double)(((double)nUseTime) / ((double)(whenAlgo.m_nNTimes))) - (Int32)((double)(((double)nUseTime) / ((double)(whenAlgo.m_nNTimes)))) != 0)
                return price;

            double dPer = disc.m_dPercent;
            TvinciPricing.Price discPrice = CopyPrice(disc.m_oPrise);

            if (disc.m_eTheRelationType == TvinciPricing.RelationTypes.And ||
                disc.m_eTheRelationType == TvinciPricing.RelationTypes.Or)
            {
                if (discPrice != null && discPrice.m_dPrice != 0 && discPrice.m_oCurrency.m_sCurrencyCD3 == discRetPrice.m_oCurrency.m_sCurrencyCD3)
                {
                    discRetPrice.m_dPrice -= discPrice.m_dPrice;
                    if (discRetPrice.m_dPrice < 0)
                        discRetPrice.m_dPrice = 0;
                }

                if (dPer > 0.0)
                {
                    discRetPrice.m_dPrice = (double)((Int32)((discRetPrice.m_dPrice * (100 - dPer)))) / 100;
                }
                else
                {
                    discRetPrice.m_dPrice = Math.Round((discRetPrice.m_dPrice * 100.0), MidpointRounding.AwayFromZero) / 100.0;
                }
            }
            return discRetPrice;
        }

        static public string GetWSURL(string sKey)
        {
            return GetValueFromConfig(sKey);
        }

        static public string GetValueFromConfig(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        static public Int32 GetCustomData(string sCustomData)
        {
            return (int)BillingDAL.Get_LatestCustomDataID(sCustomData, "BILLING_CONNECTION_STRING");
        }

        static public string GetCustomData(Int32 nCustomDataID)
        {
            string sRet = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            selectQuery += "select CUSTOMDATA from customdata_indexer where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nCustomDataID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sRet = selectQuery.Table("query").DefaultView[0].Row["CUSTOMDATA"].ToString();
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet;
        }

        static public string GetSubscriptiopnPurchaseCoupon(Int32 nPurchaseID)
        {
            string sRet = string.Empty;
            object oExistingCustomData = ODBCWrapper.Utils.GetTableSingleVal("subscriptions_purchases", "customdata", nPurchaseID, 0, "CA_CONNECTION_STRING");

            if (oExistingCustomData != null)
            {
                string sExistingCustomData = oExistingCustomData.ToString();
                XmlDocument docCustomData = new XmlDocument();
                docCustomData.LoadXml(sExistingCustomData);
                if (docCustomData.DocumentElement != null)
                {
                    XmlNode rootNode = docCustomData.DocumentElement;
                    sRet = Utils.GetSafeValue("cc", ref rootNode);
                }
            }
            return sRet;
        }

        static public Int32 AddCustomData(string sCustomData)
        {
            Int32 nRet = GetCustomData(sCustomData);
            if (nRet == 0)
            {
                return (int)BillingDAL.Insert_NewCustomData(sCustomData, "BILLING_CONNECTION_STRING");
            }
            return nRet;
        }

        internal static bool ValidateBaseLink(Int32 nGroupID, Int32 nMediaFileID, string sBaseLink)
        {
            string sIP = "1.1.1.1";
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;
            using (TvinciAPI.API m = new ConditionalAccess.TvinciAPI.API())
            {
                string apiUrl = GetWSURL("api_ws");
                if (apiUrl.Length > 0)
                    m.Url = apiUrl;
                bool bRet = false;
                string sCacheKey = GetCachingManagerKey("ValidateBaseLink", String.Concat(nMediaFileID.ToString(), "_", sBaseLink), nGroupID);
                if (CachingManager.CachingManager.Exist(sCacheKey))
                    bRet = (bool)(CachingManager.CachingManager.GetCachedData(sCacheKey));
                else
                {
                    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "ValidateBaseLink", "api", sIP, ref sWSUserName, ref sWSPass);
                    bRet = m.ValidateBaseLink(sWSUserName, sWSPass, nMediaFileID, sBaseLink);
                    CachingManager.CachingManager.SetCachedData(sCacheKey, bRet, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                }
                return bRet;
            }
        }

        internal static TvinciAPI.MeidaMaper[] GetMediaMapper(Int32 nGroupID, Int32[] nMediaFilesIDs)
        {
            return GetMediaMapper(nGroupID, nMediaFilesIDs, string.Empty, string.Empty);
        }

        internal static TvinciAPI.MeidaMaper[] GetMediaMapper(Int32 nGroupID, Int32[] nMediaFilesIDs, string sAPIUsername, string sAPIPassword)
        {
            if (nMediaFilesIDs == null)
                return null;
            string sIP = "1.1.1.1";
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;

            TvinciAPI.API m = null;

            TvinciAPI.MeidaMaper[] mapper = null;
            try
            {
                string nMediaFilesIDsToCache = ConvertArrayIntToStr(nMediaFilesIDs);
                string sCacheKey = Utils.GetCachingManagerKey("MapMediaFiles", nMediaFilesIDsToCache, nGroupID);
                if (CachingManager.CachingManager.Exist(sCacheKey))
                    mapper = (TvinciAPI.MeidaMaper[])(CachingManager.CachingManager.GetCachedData(sCacheKey));
                else
                {
                    m = new ConditionalAccess.TvinciAPI.API();
                    string sWSUrl = GetWSURL("api_ws");
                    if (sWSUrl.Length > 0)
                        m.Url = sWSUrl;

                    if (string.IsNullOrEmpty(sAPIUsername) || string.IsNullOrEmpty(sAPIPassword))
                    {
                        TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "MapMediaFiles", "api", sIP, ref sWSUserName, ref sWSPass);
                        mapper = m.MapMediaFiles(sWSUserName, sWSPass, nMediaFilesIDs);
                    }
                    else
                    {
                        mapper = m.MapMediaFiles(sAPIUsername, sAPIPassword, nMediaFilesIDs);
                    }
                    CachingManager.CachingManager.SetCachedData(sCacheKey, mapper, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                }
            }
            finally
            {
                #region Disposing
                if (m != null)
                {
                    m.Dispose();
                }
                #endregion

            }
            return mapper;
        }

        internal static int GetMediaFileTypeID(int nGroupID, int nMediaFileID)
        {
            return GetMediaFileTypeID(nGroupID, nMediaFileID, string.Empty, string.Empty);
        }

        internal static int GetMediaFileTypeID(int nGroupID, int nMediaFileID, string sAPIUsername, string sAPIPassword)
        {
            string sIP = "1.1.1.1";
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;
            Int32 nRet = 0;

            string sCacheKey = GetCachingManagerKey("GetMediaFileTypeID", nMediaFileID + "", nGroupID, string.Empty, string.Empty, string.Empty);
            if (CachingManager.CachingManager.Exist(sCacheKey))
                nRet = (Int32)(CachingManager.CachingManager.GetCachedData(sCacheKey));
            else
            {
                using (TvinciAPI.API m = new ConditionalAccess.TvinciAPI.API())
                {
                    string apiUrl = GetWSURL("api_ws");
                    if (apiUrl.Length > 0)
                        m.Url = apiUrl;
                    if (string.IsNullOrEmpty(sAPIUsername) || string.IsNullOrEmpty(sAPIPassword))
                    {
                        TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetMediaFileTypeID", "api", sIP, ref sWSUserName, ref sWSPass);
                        nRet = m.GetMediaFileTypeID(sWSUserName, sWSPass, nMediaFileID);
                    }
                    else
                    {
                        nRet = m.GetMediaFileTypeID(sAPIUsername, sAPIPassword, nMediaFileID);
                    }
                    CachingManager.CachingManager.SetCachedData(sCacheKey, nRet, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                }
            }

            return nRet;
        }

        internal static Dictionary<int, bool> PPVBulkDoCreditNeedToDownloadedUsingCollections(int nGroupID, int nMediaFileID,
            List<int> lstAllUsersInDomain, List<int> lstCollectionCodes, string sPricingUsername,
            string sPricingPassword)
        {
            Dictionary<int, bool> res = new Dictionary<int, bool>();
            Dictionary<int, DateTime> collToCreateDateMapping = new Dictionary<int, DateTime>();
            DateTime dbTimeNow = ODBCWrapper.Utils.FICTIVE_DATE;
            if (lstCollectionCodes != null && lstCollectionCodes.Count > 0)
            {
                Collection[] colls = GetCollectionsDataWithCaching(lstCollectionCodes, sPricingUsername, sPricingPassword, nGroupID);
                InitializePPVBulkDoCreditNeedDownloadedDictionary(ref res, lstCollectionCodes);

                if (ConditionalAccessDAL.Get_AllDomainsPPVUsesUsingCollections(lstAllUsersInDomain, nGroupID, nMediaFileID, lstCollectionCodes,
                    ref dbTimeNow, ref collToCreateDateMapping) && collToCreateDateMapping.Count > 0)
                {
                    for (int i = 0; i < colls.Length; i++)
                    {
                        int collCode = 0;
                        if (colls[i] != null && Int32.TryParse(colls[i].m_CollectionCode, out collCode) &&
                            collCode > 0 && res.ContainsKey(collCode) && collToCreateDateMapping.ContainsKey(collCode)
                            && colls[i].m_oCollectionUsageModule != null)
                        {
                            int nViewLifeCycle = colls[i].m_oCollectionUsageModule.m_tsViewLifeCycle;
                            DateTime lastCreateDate = collToCreateDateMapping[collCode];
                            DateTime endDate = Utils.GetEndDateTime(lastCreateDate, nViewLifeCycle);
                            res[collCode] = dbTimeNow >= endDate;
                        }
                    }
                }

            }

            return res;
        }

        private static void InitializePPVBulkDoCreditNeedDownloadedDictionary(ref Dictionary<int, bool> dict, List<int> lstCollectionCodes)
        {
            for (int i = 0; i < lstCollectionCodes.Count; i++)
            {
                if (!dict.ContainsKey(lstCollectionCodes[i]))
                {
                    dict.Add(lstCollectionCodes[i], false);
                }
            }
        }

        // pass string or numeric as T
        internal static TvinciPricing.Collection[] GetCollectionsDataWithCaching<T>(List<T> lstCollsCodes, string sWSUsername, string sWSPassword, int nGroupID) where T : IComparable, IComparable<T>, IEquatable<T>, IConvertible
        {
            Collection[] res = null;
            if (lstCollsCodes != null && lstCollsCodes.Count > 0)
            {
                List<T> distinctLstCollsCodes = lstCollsCodes.Distinct().ToList<T>();

                res = new Collection[distinctLstCollsCodes.Count];
                List<string> collsCodesToQueryPricing = new List<string>();
                Dictionary<string, int> colToIndexMapping = new Dictionary<string, int>();
                Dictionary<string, string> colToCacheKeyMapping = new Dictionary<string, string>();

                for (int i = 0; i < distinctLstCollsCodes.Count; i++)
                {
                    Collection col = null;
                    string sColCode = distinctLstCollsCodes[i].ToString();
                    string sCacheKey = GetCachingManagerKey("GetCollectionData", sColCode, nGroupID);
                    if (CachingManager.CachingManager.Exist(sCacheKey))
                    {
                        col = (TvinciPricing.Collection)(CachingManager.CachingManager.GetCachedData(sCacheKey));
                        res[i] = col;
                    }
                    else
                    {
                        colToIndexMapping.Add(sColCode, i);
                        colToCacheKeyMapping.Add(sColCode, sCacheKey);
                        collsCodesToQueryPricing.Add(sColCode);
                    }
                } // end for

                // if we encountered un-cached collections query pricing and then cache the results
                if (collsCodesToQueryPricing.Count > 0)
                {
                    using (TvinciPricing.mdoule m = new TvinciPricing.mdoule())
                    {
                        string pricingUrl = GetWSURL("pricing_ws");
                        if (pricingUrl.Length > 0)
                            m.Url = pricingUrl;
                        Collection[] pricingAnswer = m.GetCollectionsData(sWSUsername, sWSPassword, collsCodesToQueryPricing.ToArray(),
                            string.Empty, string.Empty, string.Empty);
                        if (pricingAnswer != null && pricingAnswer.Length > 0)
                        {
                            for (int i = 0; i < pricingAnswer.Length; i++)
                            {
                                Collection c = pricingAnswer[i];
                                if (c != null && colToIndexMapping.ContainsKey(c.m_CollectionCode))
                                {
                                    res[colToIndexMapping[c.m_CollectionCode]] = c;
                                    CachingManager.CachingManager.SetCachedData(colToCacheKeyMapping[c.m_CollectionCode], c, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                                }
                            }
                        }
                    }
                }

            }

            return res;
        }

        // pass either list of string or list of numerics.
        internal static TvinciPricing.Subscription[] GetSubscriptionsDataWithCaching<T>(List<T> lstSubsCodes, string sWSUsername, string sWSPassword, int nGroupID) where T : IComparable, IComparable<T>, IEquatable<T>, IConvertible
        {
            Subscription[] res = null;
            if (lstSubsCodes != null && lstSubsCodes.Count > 0)
            {
                List<T> distinctLstSubCodes = lstSubsCodes.Distinct().ToList<T>();

                res = new Subscription[distinctLstSubCodes.Count];
                List<string> subCodesToQueryPricing = new List<string>();
                Dictionary<string, int> subToIndexMapping = new Dictionary<string, int>();
                Dictionary<string, string> subToCacheKeyMapping = new Dictionary<string, string>();

                // try fetch subscriptions data from cache
                for (int i = 0; i < distinctLstSubCodes.Count; i++)
                {
                    Subscription sub = null;
                    string subCode = distinctLstSubCodes[i].ToString();
                    string sCacheKey = GetCachingManagerKey("GetSubscriptionData", subCode, nGroupID);
                    if (CachingManager.CachingManager.Exist(sCacheKey))
                    {
                        sub = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData(sCacheKey));
                        res[i] = sub;
                    }
                    else
                    {
                        subToIndexMapping.Add(subCode, i);
                        subToCacheKeyMapping.Add(subCode, sCacheKey);
                        subCodesToQueryPricing.Add(subCode);
                    }

                } // end for

                // if we encountered un-cached subscriptions query pricing
                if (subCodesToQueryPricing.Count > 0)
                {
                    using (TvinciPricing.mdoule m = new TvinciPricing.mdoule())
                    {
                        string pricingUrl = GetWSURL("pricing_ws");
                        if (pricingUrl.Length > 0)
                            m.Url = pricingUrl;
                        Subscription[] pricingAnswer = m.GetSubscriptionsData(sWSUsername, sWSPassword, subCodesToQueryPricing.ToArray(), string.Empty, string.Empty, string.Empty);
                        if (pricingAnswer != null && pricingAnswer.Length > 0)
                        {
                            // fill result array and cache pricing answer
                            for (int i = 0; i < pricingAnswer.Length; i++)
                            {
                                Subscription s = pricingAnswer[i];
                                if (s != null && subToIndexMapping.ContainsKey(s.m_SubscriptionCode))
                                {
                                    res[subToIndexMapping[s.m_SubscriptionCode]] = s;
                                    CachingManager.CachingManager.SetCachedData(subToCacheKeyMapping[s.m_SubscriptionCode], s, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                                }
                            }
                        }
                    }
                }


            }

            return res;
        }

        private static List<string> GetSubCodesForDBQuery(Subscription[] subs)
        {
            List<string> res = new List<string>();
            if (subs != null && subs.Length > 0)
            {
                for (int i = 0; i < subs.Length; i++)
                {
                    if (subs[i] != null)
                    {
                        res.Add(subs[i].m_SubscriptionCode);
                    }
                }
            }

            return res;
        }

        private static List<string> GetColCodesForDBQuery(Collection[] colls)
        {
            List<string> res = new List<string>();
            if (colls != null && colls.Length > 0)
            {
                for (int i = 0; i < colls.Length; i++)
                {
                    if (colls[i] != null)
                    {
                        res.Add(colls[i].m_CollectionCode);
                    }
                }
            }

            return res;
        }

        private static Dictionary<string, bool> InitializeCreditDownloadedDict(List<string> lst)
        {
            Dictionary<string, bool> res = new Dictionary<string, bool>();
            if (lst != null && lst.Count > 0)
            {
                for (int i = 0; i < lst.Count; i++)
                {
                    if (!res.ContainsKey(lst[i]))
                    {
                        res.Add(lst[i], true);
                    }
                }
            }

            return res;
        }

        // bulk version of Bundle_DoesCreditNeedToDownloaded
        internal static void DoBundlesCreditNeedToBeDownloaded(List<string> lstSubCodes, List<string> lstColCodes,
            int nMediaFileID, int nGroupID, List<int> allUsersInDomain, List<int> relatedMediaFiles, string sPricingUsername, string sPricingPassword,
            ref Dictionary<string, bool> subsRes, ref Dictionary<string, bool> collsRes)
        {
            Subscription[] subs = null;
            Collection[] colls = null;

            subsRes = InitializeCreditDownloadedDict(lstSubCodes);
            collsRes = InitializeCreditDownloadedDict(lstColCodes);

            if (lstSubCodes != null && lstSubCodes.Count > 0)
            {
                subs = GetSubscriptionsDataWithCaching(lstSubCodes, sPricingUsername, sPricingPassword, nGroupID);
            }
            if (lstColCodes != null && lstColCodes.Count > 0)
            {
                colls = GetCollectionsDataWithCaching(lstColCodes, sPricingUsername, sPricingPassword, nGroupID);
            }

            Dictionary<string, DateTime> subsToCreateDateMapping = null;
            Dictionary<string, DateTime> colsToCreateDateMapping = null;
            DateTime dbTimeNow = ODBCWrapper.Utils.FICTIVE_DATE;
            List<string> subsLst = GetSubCodesForDBQuery(subs);
            List<string> colsLst = GetColCodesForDBQuery(colls);
            List<string> domainUsers = allUsersInDomain.Select(item => item.ToString()).ToList<string>();
            if (ConditionalAccessDAL.Get_LatestCreateDateOfBundlesUses(subsLst, colsLst, domainUsers, relatedMediaFiles, nGroupID,
                ref subsToCreateDateMapping, ref colsToCreateDateMapping, ref dbTimeNow))
            {
                if (subs != null && subs.Length > 0)
                {
                    for (int i = 0; i < subs.Length; i++)
                    {
                        if (subs[i] != null && subsToCreateDateMapping.ContainsKey(subs[i].m_SubscriptionCode))
                        {
                            subsRes[subs[i].m_SubscriptionCode] = CalcIsCreditNeedToBeDownloadedForSub(dbTimeNow, subsToCreateDateMapping[subs[i].m_SubscriptionCode], subs[i]);
                        }
                    }
                }

                if (colls != null && colls.Length > 0)
                {
                    for (int i = 0; i < colls.Length; i++)
                    {
                        if (colls[i] != null && colsToCreateDateMapping.ContainsKey(colls[i].m_CollectionCode))
                        {
                            collsRes[colls[i].m_CollectionCode] = CalcIsCreditNeedToBeDownloadedForCol(dbTimeNow, colsToCreateDateMapping[colls[i].m_CollectionCode], colls[i]);
                        }
                    }
                }
            }

        }

        private static bool CalcIsCreditNeedToBeDownloadedForSub(DateTime dbTimeNow, DateTime lastCreateDate, Subscription s)
        {
            bool res = true;
            if (s.m_oSubscriptionUsageModule != null && !lastCreateDate.Equals(ODBCWrapper.Utils.FICTIVE_DATE)
                && !dbTimeNow.Equals(ODBCWrapper.Utils.FICTIVE_DATE))
            {
                if (GetEndDateTime(lastCreateDate, s.m_oSubscriptionUsageModule.m_tsViewLifeCycle) > dbTimeNow)
                {
                    res = false;
                }
            }

            return res;
        }

        private static bool CalcIsCreditNeedToBeDownloadedForCol(DateTime dbTimeNow, DateTime lastCreateDate, Collection c)
        {
            bool res = true;
            if (c.m_oCollectionUsageModule != null && !lastCreateDate.Equals(ODBCWrapper.Utils.FICTIVE_DATE) &&
                !dbTimeNow.Equals(ODBCWrapper.Utils.FICTIVE_DATE))
            {
                if (GetEndDateTime(lastCreateDate, c.m_oCollectionUsageModule.m_tsViewLifeCycle) > dbTimeNow)
                {
                    res = false;
                }
            }

            return res;
        }

        internal static bool Bundle_DoesCreditNeedToDownloaded(string sBundleCd, List<int> usersInDomain, List<int> relatedMediaFiles, int groupID, eBundleType bundleType)
        {
            bool bIsSub = true;
            bool nIsCreditDownloaded = true;
            string sIP = "1.1.1.1";
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;

            using (TvinciPricing.mdoule m = new TvinciPricing.mdoule())
            {
                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (sWSURL.Length > 0)
                    m.Url = sWSURL;

                TvinciPricing.PPVModule theBundle = null;
                TvinciPricing.UsageModule u = null;

                string sTableName = string.Empty;

                switch (bundleType)
                {
                    case eBundleType.SUBSCRIPTION:
                        {
                            TvinciPricing.Subscription theSub = null;
                            string sCacheKey = GetCachingManagerKey("GetSubscriptionData", sBundleCd, groupID, string.Empty, string.Empty, string.Empty);
                            if (CachingManager.CachingManager.Exist(sCacheKey))
                                theSub = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData(sCacheKey));
                            else
                            {
                                TVinciShared.WS_Utils.GetWSUNPass(groupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                                theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sBundleCd, String.Empty, String.Empty, String.Empty, false);
                                CachingManager.CachingManager.SetCachedData(sCacheKey, theSub, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                            }

                            u = theSub.m_oSubscriptionUsageModule;
                            theBundle = theSub;
                            bIsSub = true;

                            break;
                        }
                    case eBundleType.COLLECTION:
                        {
                            TvinciPricing.Collection theCol = null;
                            string sCacheKey = GetCachingManagerKey("GetCollectionData", sBundleCd, groupID, string.Empty, string.Empty, string.Empty);
                            if (CachingManager.CachingManager.Exist(sCacheKey))
                                theCol = (TvinciPricing.Collection)(CachingManager.CachingManager.GetCachedData(sCacheKey));
                            else
                            {
                                TVinciShared.WS_Utils.GetWSUNPass(groupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                                theCol = m.GetCollectionData(sWSUserName, sWSPass, sBundleCd, String.Empty, String.Empty, String.Empty, false);
                                CachingManager.CachingManager.SetCachedData(sCacheKey, theCol, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                            }

                            u = theCol.m_oCollectionUsageModule;
                            theBundle = theCol;
                            bIsSub = false;

                            break;
                        }
                }

                Int32 nViewLifeCycle = u.m_tsViewLifeCycle;
                DateTime dtCreateDateOfLatestBundleUse = ODBCWrapper.Utils.FICTIVE_DATE;
                DateTime dtNow = ODBCWrapper.Utils.FICTIVE_DATE;

                if (ConditionalAccessDAL.Get_LatestCreateDateOfBundleUses(sBundleCd, groupID, usersInDomain, relatedMediaFiles, bIsSub,
                    ref dtCreateDateOfLatestBundleUse, ref dtNow)
                    && !dtCreateDateOfLatestBundleUse.Equals(ODBCWrapper.Utils.FICTIVE_DATE)
                    && !dtNow.Equals(ODBCWrapper.Utils.FICTIVE_DATE)
                    && ((dtNow - dtCreateDateOfLatestBundleUse).TotalMinutes < nViewLifeCycle))
                {
                    nIsCreditDownloaded = false;
                }
            }

            return nIsCreditDownloaded;
        }

        private static void FillCatalogSignature(WS_Catalog.BaseRequest request)
        {
            request.m_sSignString = Guid.NewGuid().ToString();
            request.m_sSignature = TVinciShared.WS_Utils.GetCatalogSignature(request.m_sSignString, GetWSURL("CatalogSignatureKey"));
        }

        private static WS_Catalog.BundlesContainingMediaRequest InitializeCatalogRequest(int nGroupID, int nMediaID,
            List<int> lstDistinctSubs, List<int> lstDistinctColls)
        {
            WS_Catalog.BundlesContainingMediaRequest request = new WS_Catalog.BundlesContainingMediaRequest();
            request.m_nGroupID = nGroupID;
            request.m_nMediaID = nMediaID;
            request.m_oFilter = new WS_Catalog.Filter();
            FillCatalogSignature(request);
            int sizeOfArr = lstDistinctSubs.Count + lstDistinctColls.Count, i = 0;
            request.m_oBundles = new WS_Catalog.BundleKeyValue[sizeOfArr];
            for (i = 0; i < lstDistinctSubs.Count; i++)
            {
                request.m_oBundles[i] = new WS_Catalog.BundleKeyValue() { m_nBundleCode = lstDistinctSubs[i], m_eBundleType = WS_Catalog.eBundleType.SUBSCRIPTION };
            }
            for (int j = 0; j < lstDistinctColls.Count; j++)
            {
                request.m_oBundles[j + i] = new WS_Catalog.BundleKeyValue() { m_nBundleCode = lstDistinctColls[j], m_eBundleType = WS_Catalog.eBundleType.COLLECTION };
            }

            return request;
        }

        private static bool IsUserCanStillUseSub(int numOfUses, int maxNumOfUses)
        {
            // maxNumOfUses==0 means unlimited uses.
            return maxNumOfUses == 0 || numOfUses < maxNumOfUses;
        }

        private static bool IsUserCanStillUseCol(int numOfUses, int maxNumOfUses)
        {
            return maxNumOfUses == 0 || numOfUses < maxNumOfUses;
        }


        private static void GetUserValidBundlesFromListOptimized(string sSiteGuid, int nMediaID, int nMediaFileID, int nGroupID,
            int[] nFileTypes, List<int> lstUserIDs, string sPricingUsername, string sPricingPassword, List<int> relatedMediaFiles,
            ref Subscription[] subsRes, ref Collection[] collsRes,
            ref  Dictionary<string, KeyValuePair<int, DateTime>> subsPurchase, ref Dictionary<string, KeyValuePair<int, DateTime>> collPurchase)
        {
            DataSet ds = ConditionalAccessDAL.Get_AllBundlesInfoByUserIDs(lstUserIDs, nFileTypes != null && nFileTypes.Length > 0 ? nFileTypes.ToList<int>() : new List<int>(0));
            if (IsBundlesDataSetValid(ds))
            {
                // the subscriptions and collections we add to those list will be sent to the Catalog in order to determine whether the media
                // given as input belongs to it.
                List<int> subsToSendToCatalog = new List<int>();
                List<int> collsToSendToCatalog = new List<int>();

                List<string> subsToBundleCreditDownloadedQuery = new List<string>();
                List<string> colsToBundleCreditDownloadedQuery = new List<string>();

                // iterate over subscriptions
                DataTable subs = ds.Tables[0];
                int nWaiver = 0;
                DateTime dPurchaseDate = DateTime.MinValue;

                if (subs != null && subs.Rows != null && subs.Rows.Count > 0)
                {
                    for (int i = 0; i < subs.Rows.Count; i++)
                    {
                        int numOfUses = 0;
                        int maxNumOfUses = 0;
                        string bundleCode = string.Empty;
                        nWaiver = 0;
                        dPurchaseDate = DateTime.MinValue;

                        GetBundlePurchaseData(subs.Rows[i], "SUBSCRIPTION_CODE", ref numOfUses, ref maxNumOfUses, ref bundleCode, ref nWaiver, ref dPurchaseDate);
                        if (IsUserCanStillUseSub(numOfUses, maxNumOfUses))
                        {
                            // add to Catalog's BundlesContainingMediaRequest
                            int subCode = 0;
                            if (Int32.TryParse(bundleCode, out subCode) && subCode > 0)
                            {
                                subsToSendToCatalog.Add(subCode);
                                if (!subsPurchase.ContainsKey(bundleCode))
                                {
                                    subsPurchase.Add(bundleCode, new KeyValuePair<int, DateTime>(nWaiver, dPurchaseDate));
                                }
                            }
                            else
                            {
                                // log
                            }
                        }
                        else
                        {
                            // add to bulk query of Bundle_DoesCreditNeedToDownloaded to DB
                            //afterwards, the subs who pass the Bundle_DoesCreditNeedToDownloaded to DB test add to Catalog request.
                            subsToBundleCreditDownloadedQuery.Add(bundleCode);
                        }
                    }
                }

                //iterate over collections
                DataTable colls = ds.Tables[1];
                if (colls != null && colls.Rows != null && colls.Rows.Count > 0)
                {
                    for (int i = 0; i < colls.Rows.Count; i++)
                    {
                        int numOfUses = 0;
                        int maxNumOfUses = 0;
                        string bundleCode = string.Empty;
                        nWaiver = 0;
                        dPurchaseDate = DateTime.MinValue;

                        GetBundlePurchaseData(colls.Rows[i], "COLLECTION_CODE", ref numOfUses, ref maxNumOfUses, ref bundleCode, ref nWaiver, ref dPurchaseDate);
                        if (IsUserCanStillUseCol(numOfUses, maxNumOfUses))
                        {
                            // add to Catalog's BundlesContainingMediaRequest
                            int collCode = 0;
                            if (Int32.TryParse(bundleCode, out collCode) && collCode > 0)
                            {
                                collsToSendToCatalog.Add(collCode);
                                if (!collPurchase.ContainsKey(bundleCode))
                                {
                                    collPurchase.Add(bundleCode, new KeyValuePair<int, DateTime>(nWaiver, dPurchaseDate));
                                }
                            }
                            else
                            {
                                //log
                            }
                        }
                        else
                        {
                            colsToBundleCreditDownloadedQuery.Add(bundleCode);
                            // add to bulk query of Bundle_DoesCreditNeedToDownload to DB
                            //afterwards, the colls which pass the Bundle_DoesCreditNeedToDownloaded to DB test add to Catalog request.
                            // finally, the colls which pass the catalog need to be validated against PPV_DoesCreditNeedToDownloadedUsingCollection
                        }
                    }
                }

                HandleBundleCreditNeedToDownloadedQuery(subsToBundleCreditDownloadedQuery, colsToBundleCreditDownloadedQuery,
                    nMediaFileID, nGroupID, lstUserIDs, relatedMediaFiles, sPricingUsername, sPricingPassword, ref subsToSendToCatalog,
                    ref collsToSendToCatalog);

                // get distinct subs from subs list, same for collection
                List<int> distinctSubs = subsToSendToCatalog.Distinct().ToList<int>();
                List<int> distinctColls = collsToSendToCatalog.Distinct().ToList<int>();

                List<int> validatedSubs = null;
                List<int> validatedColls = null;

                if (distinctSubs.Count > 0 || distinctColls.Count > 0)
                {
                    ValidateMediaContainedInBundles(nMediaID, nGroupID, distinctSubs, distinctColls, ref validatedSubs, ref validatedColls);
                }

                if (validatedSubs != null && validatedSubs.Count > 0)
                {
                    subsRes = GetSubscriptionsDataWithCaching(validatedSubs, sPricingUsername, sPricingPassword, nGroupID);
                }

                // now validate bulk collections - PPV_CreditNeedToDownloadedUsingCollection

                if (validatedColls != null && validatedColls.Count > 0)
                {
                    Dictionary<int, bool> collsAfterPPVCreditValidation = PPVBulkDoCreditNeedToDownloadedUsingCollections(nGroupID,
                        nMediaFileID, lstUserIDs, validatedColls, sPricingUsername, sPricingPassword);
                    List<int> finalCollCodes = GetFinalCollectionCodes(collsAfterPPVCreditValidation);
                    if (finalCollCodes != null && finalCollCodes.Count > 0)
                    {
                        collsRes = GetCollectionsDataWithCaching(finalCollCodes, sPricingUsername, sPricingPassword, nGroupID);
                    }

                }
            }
            else
            {
                #region Logging
                StringBuilder sb = new StringBuilder("SP: ConditionalAccessDAL.Get_AllBundlesInfoByUserIDs returned corrupted data. ");
                sb.Append(String.Concat(" Site Guid: ", sSiteGuid));
                sb.Append(String.Concat(" Media ID: ", nMediaID));
                sb.Append(String.Concat(" Media File ID: ", nMediaFileID));
                if (lstUserIDs != null && lstUserIDs.Count > 0)
                {
                    sb.Append(" User IDs: ");
                    for (int i = 0; i < lstUserIDs.Count; i++)
                    {
                        sb.Append(String.Concat(lstUserIDs[i], ", "));
                    }
                }
                else
                {
                    sb.Append(" User IDs is null or empty. ");
                }
                if (nFileTypes != null && nFileTypes.Length > 0)
                {
                    sb.Append(" File Types: ");
                    for (int i = 0; i < nFileTypes.Length; i++)
                    {
                        sb.Append(String.Concat(nFileTypes[i], ", "));
                    }
                }
                else
                {
                    sb.Append("File Types is null of empty");
                }

                Logger.Logger.Log("Error", sb.ToString(), "CAS.Utils");
                #endregion

                throw new Exception("Error occurred in GetUserValidBundlesFromListOptimized. Refer to CAS.Utils log file");

            }
        }

        private static List<int> GetFinalCollectionCodes(Dictionary<int, bool> collsAfterPPVCreditValidation)
        {
            List<int> res = new List<int>();
            foreach (KeyValuePair<int, bool> kvp in collsAfterPPVCreditValidation)
            {
                if (!kvp.Value)
                {
                    res.Add(kvp.Key);
                }
            }

            return res;
        }

        private static void HandleBundleCreditNeedToDownloadedQuery(List<string> subsToBundleCreditDownloadedQuery,
            List<string> colsToBundleCreditDownloadedQuery, int nMediaFileID, int nGroupID, List<int> lstUserIDs, List<int> relatedMediaFileIDs,
            string sPricingUsername, string sPricingPassword, ref List<int> subsToSendToCatalog,
            ref List<int> collsToSendToCatalog)
        {
            if (subsToBundleCreditDownloadedQuery.Count > 0 || colsToBundleCreditDownloadedQuery.Count > 0)
            {
                Dictionary<string, bool> subsRes = null;
                Dictionary<string, bool> colsRes = null;
                DoBundlesCreditNeedToBeDownloaded(subsToBundleCreditDownloadedQuery, colsToBundleCreditDownloadedQuery, nMediaFileID,
                    nGroupID, lstUserIDs, relatedMediaFileIDs, sPricingUsername, sPricingPassword, ref subsRes, ref colsRes);
                if (subsRes.Count > 0)
                {
                    foreach (KeyValuePair<string, bool> kvp in subsRes)
                    {
                        int temp = 0;
                        if (!kvp.Value && Int32.TryParse(kvp.Key, out temp) && temp > 0)
                        {

                            subsToSendToCatalog.Add(temp);
                        }
                    }
                }
                if (colsRes.Count > 0)
                {
                    foreach (KeyValuePair<string, bool> kvp in colsRes)
                    {
                        int temp = 0;
                        if (!kvp.Value && Int32.TryParse(kvp.Key, out temp) && temp > 0)
                        {
                            collsToSendToCatalog.Add(temp);
                        }
                    }
                }
            }
        }

        private static void ValidateMediaContainedInBundles(int nMediaID, int nGroupID, List<int> distinctSubs, List<int> distinctColls,
            ref List<int> subsRes, ref List<int> collsRes)
        {
            WS_Catalog.BundlesContainingMediaRequest request = InitializeCatalogRequest(nGroupID, nMediaID, distinctSubs, distinctColls);
            WS_Catalog.IserviceClient client = null;

            subsRes = new List<int>();
            collsRes = new List<int>();
            try
            {
                client = new WS_Catalog.IserviceClient();
                string sCatalogUrl = GetWSURL("WS_Catalog");
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(sCatalogUrl);
                WS_Catalog.BundlesContainingMediaResponse response = client.GetResponse(request) as WS_Catalog.BundlesContainingMediaResponse;
                if (response != null && response.m_oBundles != null && response.m_oBundles.Length > 0)
                {
                    for (int i = 0; i < response.m_oBundles.Length; i++)
                    {
                        WS_Catalog.BundleTriple bt = response.m_oBundles[i];
                        if (bt.m_bIsContained)
                        {
                            switch (bt.m_eBundleType)
                            {
                                case WS_Catalog.eBundleType.SUBSCRIPTION:
                                    subsRes.Add(bt.m_nBundleCode);
                                    break;
                                case WS_Catalog.eBundleType.COLLECTION:
                                    collsRes.Add(bt.m_nBundleCode);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
            }
        }

        private static void GetBundlePurchaseData(DataRow dr, string codeColumnName, ref int numOfUses, ref int maxNumOfUses,
            ref string bundleCode, ref int nWaiver, ref DateTime dPurchaseDate)
        {
            numOfUses = ODBCWrapper.Utils.GetIntSafeVal(dr["NUM_OF_USES"]);
            maxNumOfUses = ODBCWrapper.Utils.GetIntSafeVal(dr["MAX_NUM_OF_USES"]);
            bundleCode = ODBCWrapper.Utils.GetSafeStr(dr[codeColumnName]);

            nWaiver = ODBCWrapper.Utils.GetIntSafeVal(dr, "WAIVER");
            dPurchaseDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE");
        }

        private static bool IsBundlesDataSetValid(DataSet ds)
        {
            return ds != null && ds.Tables != null && ds.Tables.Count == 2;
        }

        internal static TvinciPricing.Price CopyPrice(TvinciPricing.Price toCopy)
        {
            TvinciPricing.Price ret = new ConditionalAccess.TvinciPricing.Price();
            ret.m_dPrice = toCopy.m_dPrice;
            ret.m_oCurrency = toCopy.m_oCurrency;
            return ret;
        }

        internal static TvinciPricing.Price CalculateCouponDiscount(ref TvinciPricing.Price pModule, TvinciPricing.CouponsGroup oCouponsGroup, string sCouponCode, int nGroupID)
        {
            TvinciPricing.Price p = CopyPrice(pModule);
            if (sCouponCode.Length > 0)
            {
                string sIP = "1.1.1.1";
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;

                using (TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule())
                {
                    if (GetWSURL("pricing_ws").Length > 0)
                        m.Url = GetWSURL("pricing_ws");


                    TvinciPricing.CouponData theCouponData = null;

                    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetCouponStatus", "pricing", sIP, ref sWSUserName, ref sWSPass);
                    theCouponData = m.GetCouponStatus(sWSUserName, sWSPass, sCouponCode);
                    CachingManager.CachingManager.SetCachedData("GetCouponStatus" + sCouponCode + "_" + nGroupID.ToString(), theCouponData, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    // }


                    if (oCouponsGroup != null &&
                        theCouponData.m_CouponStatus == ConditionalAccess.TvinciPricing.CouponsStatus.Valid &&
                        theCouponData.m_oCouponGroup.m_sGroupCode == oCouponsGroup.m_sGroupCode)
                    {
                        //Coupon discount should take place
                        TvinciPricing.DiscountModule dCouponDiscount = oCouponsGroup.m_oDiscountCode;
                        p = GetPriceAfterDiscount(p, dCouponDiscount, 0);
                    }
                }
            }
            return p;
        }

        private static bool IsVoucherValid(int nLifeCycle, long nOwnerGuid, long campaignID)
        {
            bool retVal = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select CREATE_DATE,getdate() as dNow from campaigns_uses with (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("campaign_id", "=", campaignID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", nOwnerGuid);
            selectQuery += " order by id desc";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    DateTime dNow = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["dNow"]);
                    DateTime dUsed = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["CREATE_DATE"]);
                    if ((dNow - dUsed).TotalMinutes < nLifeCycle)
                        retVal = true;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }


        internal static TvinciPricing.Price CalculateMediaFileFinalPriceNoSubs(Int32 nMediaFileID, int mediaID, TvinciPricing.Price pModule,
            TvinciPricing.DiscountModule discModule, TvinciPricing.CouponsGroup oCouponsGroup, string sSiteGUID,
            string sCouponCode, Int32 nGroupID, string subCode, string sPricingUsername, string sPricingPassword)
        {

            TvinciPricing.Price p = CopyPrice(pModule);
            if (discModule != null)
            {
                int nPPVPurchaseCount = 0;
                if (discModule.m_dPercent == 100 && !string.IsNullOrEmpty(subCode))
                {
                    nPPVPurchaseCount = ConditionalAccessDAL.Get_SubscriptionUseCount(sSiteGUID, subCode, nGroupID);
                }
                else
                {
                    nPPVPurchaseCount = ConditionalAccessDAL.Get_PPVPurchaseCount(nGroupID, sSiteGUID, subCode, nMediaFileID);
                }
                p = GetPriceAfterDiscount(p, discModule, nPPVPurchaseCount);
            }

            if (sCouponCode.Length > 0)
            {

                using (TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule())
                {
                    string sPricingURL = GetWSURL("pricing_ws");
                    if (sPricingURL.Length > 0)
                        m.Url = sPricingURL;


                    TvinciPricing.CouponData theCouponData = null;

                    //TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetCouponStatus", "pricing", sIP, ref sWSUserName, ref sWSPass);
                    string sCacheKey = Utils.GetCachingManagerKey("GetCouponStatus", sCouponCode, nGroupID);
                    theCouponData = m.GetCouponStatus(sPricingUsername, sPricingPassword, sCouponCode);
                    CachingManager.CachingManager.SetCachedData(sCacheKey, theCouponData, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);


                    if (oCouponsGroup != null && theCouponData.m_CouponType == TvinciPricing.CouponType.Voucher && theCouponData.m_campID > 0 && mediaID == theCouponData.m_ownerMedia)
                    {
                        bool isCampaignValid = false;
                        TvinciPricing.Campaign camp = m.GetCampaignData(sPricingUsername, sPricingPassword, theCouponData.m_campID);

                        if (camp != null && camp.m_ID == theCouponData.m_campID)
                        {
                            int nViewLS = camp.m_usageModule.m_tsViewLifeCycle;
                            long ownerGuid = theCouponData.m_ownerGUID;
                            isCampaignValid = IsVoucherValid(nViewLS, ownerGuid, theCouponData.m_campID);

                        }

                        if (isCampaignValid)
                        {
                            TvinciPricing.DiscountModule voucherDiscount = theCouponData.m_oCouponGroup.m_oDiscountCode;
                            p = GetPriceAfterDiscount(p, voucherDiscount, 1);
                        }
                    }


                    else
                    {
                        if (oCouponsGroup != null &&
                            theCouponData.m_CouponStatus == ConditionalAccess.TvinciPricing.CouponsStatus.Valid &&
                            theCouponData.m_oCouponGroup.m_sGroupCode == oCouponsGroup.m_sGroupCode)
                        {
                            //Coupon discount should take place
                            TvinciPricing.DiscountModule dCouponDiscount = oCouponsGroup.m_oDiscountCode;
                            p = GetPriceAfterDiscount(p, dCouponDiscount, 0);
                        }
                    }
                }
            } // end if coupon code is not empty
            return p;
        }

        private static TvinciPricing.Price GetMediaFileFinalPriceNoSubs(Int32 nMediaFileID, int mediaID, TvinciPricing.PPVModule ppvModule,
            string sSiteGUID, string sCouponCode, Int32 nGroupID, string subCode, string sPricingUsername, string sPricingPassword)
        {
            TvinciPricing.Price pModule = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(ppvModule.m_oPriceCode.m_oPrise));
            TvinciPricing.DiscountModule discModule = TVinciShared.ObjectCopier.Clone<TvinciPricing.DiscountModule>((TvinciPricing.DiscountModule)(ppvModule.m_oDiscountModule));
            TvinciPricing.CouponsGroup couponGroups = TVinciShared.ObjectCopier.Clone<TvinciPricing.CouponsGroup>((TvinciPricing.CouponsGroup)(ppvModule.m_oCouponsGroup));

            return CalculateMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, pModule, discModule, couponGroups, sSiteGUID,
                sCouponCode, nGroupID, subCode, sPricingUsername, sPricingPassword);
        }

        static public TvinciPricing.Price GetSubscriptionFinalPrice(Int32 nGroupID, string sSubCode, string sSiteGUID, string sCouponCode, ref PriceReason theReason, ref TvinciPricing.Subscription theSub,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return GetSubscriptionFinalPrice(nGroupID, sSubCode, sSiteGUID, sCouponCode, ref theReason, ref theSub,
           sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty);
        }

        //***********************************************
        static public TvinciPricing.Price GetSubscriptionFinalPrice(Int32 nGroupID, string sSubCode, string sSiteGUID, string sCouponCode, ref PriceReason theReason, ref TvinciPricing.Subscription theSub,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string connStr, string sClientIP)
        {
            TvinciPricing.Price p = null;
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";
            TvinciPricing.Subscription s = null;
            bool isGeoCommerceBlock = false;

            #region Get Init Subscription Object

            //create web service pricing insatance
            TvinciPricing.mdoule m = null;
            try
            {
                m = new ConditionalAccess.TvinciPricing.mdoule();

                //set web service pricing url
                if (GetWSURL("pricing_ws").Length > 0)
                    m.Url = GetWSURL("pricing_ws");

                //create Cahe object Name
                string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                if (CachingManager.CachingManager.Exist("GetSubscriptionData" + sSubCode + "_" + nGroupID.ToString() + sLocaleForCache) == true)
                    //get subscription object from chace
                    s = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData("GetSubscriptionData" + sSubCode + "_" + nGroupID.ToString() + sLocaleForCache));
                else
                {
                    //init user name and password to use pricing webservice
                    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetSubscriptionData", "pricing", sIP, ref sWSUserName, ref sWSPass);

                    //get subscription data object
                    s = m.GetSubscriptionData(sWSUserName, sWSPass, sSubCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);

                    //add the subscription object to cache
                    //CachingManager.CachingManager.SetCachedData("GetSubscriptionData" + sSubCode + "_" + nGroupID.ToString() + sLocaleForCache,s,86400,System.Web.Caching.CacheItemPriority.Default,0,false);
                }

            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder(String.Concat("Exception. Exception msg: ", ex.Message));
                sb.Append(String.Concat(" Group ID: ", nGroupID));
                sb.Append(String.Concat(" Sub Code: ", sSubCode));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Coupon Code: ", sCouponCode));
                sb.Append(String.Concat(" Country Code: ", sCountryCd));
                sb.Append(String.Concat(" Language Code: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" Device Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" Connection String: ", connStr));
                sb.Append(String.Concat(" Client IP: ", sClientIP));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));
                Logger.Logger.Log("GetSubscriptionFinalPrice", sb.ToString(), "ConditionalAccessUtils");
                #endregion
                #region Disposing
                if (m != null)
                {
                    m.Dispose();
                    m = null;
                }
                #endregion
                theReason = PriceReason.UnKnown;
                return null;

            }
            if (m != null)
            {
                m.Dispose();
                m = null;
            }

            #endregion

            #region Check subscription Geo Commerce Block

            TvinciAPI.API api = null;
            try
            {
                api = new TvinciAPI.API();
                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetSubscriptionData", "api", sIP, ref sWSUserName, ref sWSPass);
                if (GetWSURL("api_ws").Length > 0)
                    api.Url = GetWSURL("api_ws");

                isGeoCommerceBlock = api.CheckGeoCommerceBlock(sWSUserName, sWSPass, s.n_GeoCommerceID, sClientIP);
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder(String.Concat("Exception. Exception msg: ", ex.Message));
                sb.Append(String.Concat(" Group ID: ", nGroupID));
                sb.Append(String.Concat(" Sub Code: ", sSubCode));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Coupon Code: ", sCouponCode));
                sb.Append(String.Concat(" Country Code: ", sCountryCd));
                sb.Append(String.Concat(" Language Code: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" Device Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" Connection String: ", connStr));
                sb.Append(String.Concat(" Client IP: ", sClientIP));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));
                Logger.Logger.Log("GetSubscriptionFinalPrice", sb.ToString(), "ConditionalAccessUtils");
                #endregion
                #region Disposing
                if (api != null)
                {
                    api.Dispose();
                    api = null;
                }
                #endregion
                theReason = PriceReason.UnKnown;
                return null;
            }
            if (api != null)
            {
                api.Dispose();
                api = null;
            }

            #endregion

            if (!isGeoCommerceBlock)
            {
                theSub = TVinciShared.ObjectCopier.Clone<TvinciPricing.Subscription>((TvinciPricing.Subscription)(s));
                if (s == null)
                {
                    theReason = PriceReason.UnKnown;
                    return null;
                }

                if (s.m_oSubscriptionPriceCode != null)
                    p = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(s.m_oSubscriptionPriceCode.m_oPrise));
                theReason = PriceReason.ForPurchase;

                List<int> lUsersIds = ConditionalAccess.Utils.GetAllUsersDomainBySiteGUID(sSiteGUID, nGroupID);

                DataTable dt = DAL.ConditionalAccessDAL.Get_SubscriptionBySubscriptionCodeAndUserIDs(lUsersIds, sSubCode);

                if (dt != null)
                {
                    Int32 nCount = dt.Rows.Count;
                    if (nCount > 0)
                    {
                        p.m_dPrice = 0.0;
                        theReason = PriceReason.SubscriptionPurchased;
                    }
                }
                if (theReason != PriceReason.SubscriptionPurchased)
                {
                    if (s.m_oPreviewModule != null)
                        if (IsEntitledToPreviewModule(sSiteGUID, nGroupID, sSubCode, s, ref p, ref theReason))
                            return p;
                    TvinciPricing.CouponsGroup couponGroups = TVinciShared.ObjectCopier.Clone<TvinciPricing.CouponsGroup>((TvinciPricing.CouponsGroup)(theSub.m_oCouponsGroup));
                    if (theSub.m_oExtDisountModule != null)
                    {
                        TvinciPricing.DiscountModule externalDisount = TVinciShared.ObjectCopier.Clone<TvinciPricing.DiscountModule>((TvinciPricing.DiscountModule)(theSub.m_oExtDisountModule));
                        p = GetPriceAfterDiscount(p, externalDisount, 1);
                    }
                    p = CalculateCouponDiscount(ref p, couponGroups, sCouponCode, nGroupID);
                }
                return p;
            }
            else
            {
                theReason = PriceReason.GeoCommerceBlocked;
                return null;
            }
        }

        //***********************************************
        internal static TvinciPricing.Price GetSubscriptionFinalPrice(Int32 nGroupID, string sSubCode, string sSiteGUID, string sCouponCode, ref PriceReason theReason, ref TvinciPricing.Subscription theSub,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string connStr)
        {
            TvinciPricing.Price p = null;
            string sIP = "1.1.1.1";
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;
            TvinciPricing.Subscription s = null;
            using (TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule())
            {
                string sPricingURL = GetWSURL("pricing_ws");
                if (sPricingURL.Length > 0)
                    m.Url = sPricingURL;
                string sCacheKey = GetCachingManagerKey("GetSubscriptionData", sSubCode, nGroupID, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                if (CachingManager.CachingManager.Exist(sCacheKey))
                    s = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData(sCacheKey));
                else
                {
                    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetSubscriptionData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                    s = m.GetSubscriptionData(sWSUserName, sWSPass, sSubCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);
                    CachingManager.CachingManager.SetCachedData(sCacheKey, s, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                }
                theSub = TVinciShared.ObjectCopier.Clone<TvinciPricing.Subscription>((TvinciPricing.Subscription)(s));
                if (s == null)
                {
                    theReason = PriceReason.UnKnown;
                    return null;
                }

                if (s.m_oSubscriptionPriceCode != null)
                    p = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(s.m_oSubscriptionPriceCode.m_oPrise));
                theReason = PriceReason.ForPurchase;

                List<int> lUsersIds = GetAllUsersDomainBySiteGUID(sSiteGUID, nGroupID);

                DataTable dt = ConditionalAccessDAL.Get_SubscriptionBySubscriptionCodeAndUserIDs(lUsersIds, sSubCode);

                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    p.m_dPrice = 0.0;
                    theReason = PriceReason.SubscriptionPurchased;
                }
                if (theReason != PriceReason.SubscriptionPurchased)
                {
                    if (s.m_oPreviewModule != null)
                        if (IsEntitledToPreviewModule(sSiteGUID, nGroupID, sSubCode, s, ref p, ref theReason))
                            return p;
                    TvinciPricing.CouponsGroup couponGroups = TVinciShared.ObjectCopier.Clone<TvinciPricing.CouponsGroup>((TvinciPricing.CouponsGroup)(theSub.m_oCouponsGroup));
                    if (theSub.m_oExtDisountModule != null)
                    {
                        TvinciPricing.DiscountModule externalDisount = TVinciShared.ObjectCopier.Clone<TvinciPricing.DiscountModule>((TvinciPricing.DiscountModule)(theSub.m_oExtDisountModule));
                        p = GetPriceAfterDiscount(p, externalDisount, 1);
                    }
                    p = CalculateCouponDiscount(ref p, couponGroups, sCouponCode, nGroupID);
                }
            } // end using
            return p;
        }

        internal static TvinciPricing.Price GetCollectionFinalPrice(Int32 nGroupID, string sColCode, string sSiteGUID, string sCouponCode, ref PriceReason theReason, ref TvinciPricing.Collection theCol,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string connStr)
        {
            TvinciPricing.Price price = null;
            string sIP = "1.1.1.1";
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;
            TvinciPricing.Collection collection = null;
            using (TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule())
            {
                string pricingUrl = GetWSURL("pricing_ws");
                if (pricingUrl.Length > 0)
                    m.Url = pricingUrl;
                string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                if (CachingManager.CachingManager.Exist("GetCollectionData" + sColCode + "_" + nGroupID.ToString() + sLocaleForCache) == true)
                    collection = (TvinciPricing.Collection)(CachingManager.CachingManager.GetCachedData("GetCollectionData" + sColCode + "_" + nGroupID.ToString() + sLocaleForCache));
                else
                {
                    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetCollectionData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                    collection = m.GetCollectionData(sWSUserName, sWSPass, sColCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);
                    CachingManager.CachingManager.SetCachedData("GetSubscriptionData" + sColCode + "_" + nGroupID.ToString() + sLocaleForCache, collection, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                }
                theCol = TVinciShared.ObjectCopier.Clone<TvinciPricing.Collection>((TvinciPricing.Collection)(collection));
                if (collection == null)
                {
                    theReason = PriceReason.UnKnown;
                    return null;
                }

                if (collection.m_oCollectionPriceCode != null)
                    price = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(collection.m_oCollectionPriceCode.m_oPrise));
                theReason = PriceReason.ForPurchase;

                List<int> lUsersIds = ConditionalAccess.Utils.GetAllUsersDomainBySiteGUID(sSiteGUID, nGroupID);

                DataTable dt = ConditionalAccessDAL.Get_CollectionByCollectionCodeAndUserIDs(lUsersIds, sColCode);

                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    price.m_dPrice = 0.0;
                    theReason = PriceReason.CollectionPurchased;

                }
                if (theReason != PriceReason.CollectionPurchased)
                {
                    TvinciPricing.CouponsGroup couponGroups = TVinciShared.ObjectCopier.Clone<TvinciPricing.CouponsGroup>((TvinciPricing.CouponsGroup)(theCol.m_oCouponsGroup));
                    if (theCol.m_oExtDisountModule != null)
                    {
                        TvinciPricing.DiscountModule externalDisount = TVinciShared.ObjectCopier.Clone<TvinciPricing.DiscountModule>((TvinciPricing.DiscountModule)(theCol.m_oExtDisountModule));
                        price = GetPriceAfterDiscount(price, externalDisount, 1);
                    }
                    price = CalculateCouponDiscount(ref price, couponGroups, sCouponCode, nGroupID);
                }
            } // end using
            return price;
        }

        static public TvinciPricing.Price GetPrePaidFinalPrice(Int32 nGroupID, string sPrePaidCode, string sSiteGUID, ref PriceReason theReason, ref TvinciPricing.PrePaidModule thePrePaid,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string connStr)
        {
            return GetPrePaidFinalPrice(nGroupID, sPrePaidCode, sSiteGUID, ref theReason, ref thePrePaid,
            sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, connStr, string.Empty);
        }

        static public TvinciPricing.Price GetPrePaidFinalPrice(Int32 nGroupID, string sPrePaidCode, string sSiteGUID, ref PriceReason theReason, ref TvinciPricing.PrePaidModule thePrePaid,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string connStr, string sCouponCode)
        {
            TvinciPricing.Price p = null;
            string sIP = "1.1.1.1";
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;
            if (thePrePaid == null)
            {
                using (TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule())
                {
                    string pricingUrl = GetWSURL("pricing_ws");
                    if (pricingUrl.Length > 0)
                        m.Url = pricingUrl;
                    TvinciPricing.PrePaidModule ppModule = null;
                    string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                    if (CachingManager.CachingManager.Exist("GetPrePaidData" + sPrePaidCode + "_" + nGroupID.ToString() + sLocaleForCache) == true)
                        ppModule = (TvinciPricing.PrePaidModule)(CachingManager.CachingManager.GetCachedData("GetPrePaidData" + sPrePaidCode + "_" + nGroupID.ToString() + sLocaleForCache));
                    else
                    {
                        TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetPrePaidData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                        ppModule = m.GetPrePaidModuleData(sWSUserName, sWSPass, int.Parse(sPrePaidCode), sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        CachingManager.CachingManager.SetCachedData("GetPrePaidData" + sPrePaidCode + "_" + nGroupID.ToString() + sLocaleForCache, ppModule, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    }

                    thePrePaid = TVinciShared.ObjectCopier.Clone<TvinciPricing.PrePaidModule>((TvinciPricing.PrePaidModule)(ppModule));
                    if (thePrePaid == null)
                    {
                        theReason = PriceReason.UnKnown;
                        return null;
                    }
                }
            } // end if thePrePaid==null

            if (thePrePaid.m_PriceCode != null)
            {
                p = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(thePrePaid.m_PriceCode.m_oPrise));

                if (!string.IsNullOrEmpty(sCouponCode))
                {
                    TvinciPricing.CouponsGroup couponGroups = TVinciShared.ObjectCopier.Clone<TvinciPricing.CouponsGroup>((TvinciPricing.CouponsGroup)(thePrePaid.m_CouponsGroup));
                    p = CalculateCouponDiscount(ref p, couponGroups, sCouponCode, nGroupID);
                }
            }
            theReason = PriceReason.ForPurchase;

            return p;
        }

        static public string ConvertArrayIntToStr(int[] theArray)
        {

            StringBuilder sb = new StringBuilder();
            if (theArray != null && theArray.Length > 0)
            {
                for (int i = 0; i < theArray.Length; i++)
                {
                    sb.Append(String.Concat(theArray[i], "-"));
                }
            }
            return sb.ToString();
        }

        static public Int32 GetMediaIDFeomFileID(Int32 nMediaFileID, Int32 nGroupID)
        {
            Int32[] nMediaFilesIDs = { nMediaFileID };
            TvinciAPI.MeidaMaper[] mapper = null;
            string nMediaFilesIDsForCache = ConvertArrayIntToStr(nMediaFilesIDs);
            if (CachingManager.CachingManager.Exist("GetMediaMapper" + nMediaFilesIDsForCache + "_" + nGroupID.ToString()) == true)
                mapper = (TvinciAPI.MeidaMaper[])(CachingManager.CachingManager.GetCachedData("GetMediaMapper" + nMediaFilesIDsForCache + "_" + nGroupID.ToString()));
            else
            {
                mapper = GetMediaMapper(nGroupID, nMediaFilesIDs);
                CachingManager.CachingManager.SetCachedData("GetMediaMapper" + nMediaFilesIDsForCache + "_" + nGroupID.ToString(), mapper, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }
            if (mapper == null || mapper.Length == 0)
                return 0;
            if (mapper[0].m_nMediaFileID == nMediaFileID)
                return mapper[0].m_nMediaID;
            return 0;
        }

        //Get ProductCode and get it MediaFileID - then continue as it was mediaFileID
        static public Int32 GetMediaIDFeomFileID(string sProductCode, Int32 nGroupID, ref int nMediaFileID)
        {

            DataTable dt = ConditionalAccessDAL.Get_MediaFileByProductCode(nGroupID, sProductCode);

            if (dt != null && dt.DefaultView.Count > 0)
            {
                nMediaFileID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["ID"]);
            }

            return GetMediaIDFeomFileID(nMediaFileID, nGroupID);
        }

        static public DateTime GetEndDateTime(DateTime dBase, Int32 nVal, bool bIsAddLifeCycle)
        {
            int mulFactor = bIsAddLifeCycle ? 1 : -1;
            DateTime dRet = dBase;
            if (nVal == 1111111)
                dRet = dRet.AddMonths(mulFactor * 1);
            else if (nVal == 2222222)
                dRet = dRet.AddMonths(mulFactor * 2);
            else if (nVal == 3333333)
                dRet = dRet.AddMonths(mulFactor * 3);
            else if (nVal == 4444444)
                dRet = dRet.AddMonths(mulFactor * 4);
            else if (nVal == 5555555)
                dRet = dRet.AddMonths(mulFactor * 5);
            else if (nVal == 6666666)
                dRet = dRet.AddMonths(mulFactor * 6);
            else if (nVal == 9999999)
                dRet = dRet.AddMonths(mulFactor * 9);
            else if (nVal == 11111111)
                dRet = dRet.AddYears(mulFactor * 1);
            else if (nVal == 22222222)
                dRet = dRet.AddYears(mulFactor * 2);
            else if (nVal == 33333333)
                dRet = dRet.AddYears(mulFactor * 3);
            else if (nVal == 44444444)
                dRet = dRet.AddYears(mulFactor * 4);
            else if (nVal == 55555555)
                dRet = dRet.AddYears(mulFactor * 5);
            else if (nVal == 100000000)
                dRet = dRet.AddYears(mulFactor * 10);
            else
                dRet = dRet.AddMinutes(mulFactor * nVal);
            return dRet;
        }

        public static DateTime GetEndDateTime(DateTime dBase, Int32 nVal)
        {
            return GetEndDateTime(dBase, nVal, true);
        }

        static public string GetLocaleStringForCache(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {

            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(sCountryCd))
                sb.Append(String.Concat("_", sCountryCd));
            if (!string.IsNullOrEmpty(sLANGUAGE_CODE))
                sb.Append(String.Concat("_", sLANGUAGE_CODE));
            if (!string.IsNullOrEmpty(sDEVICE_NAME))
                sb.Append(String.Concat("_", sDEVICE_NAME));

            return sb.ToString();
        }

        static public double GetDoubleSafeVal(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
        {
            try
            {
                if (selectQuery.Table("query").DefaultView[nIndex].Row[sField] != null &&
                    selectQuery.Table("query").DefaultView[nIndex].Row[sField] != DBNull.Value)
                    return double.Parse(selectQuery.Table("query").DefaultView[nIndex].Row[sField].ToString());
                return 0.0;
            }
            catch
            {
                return 0.0;
            }
        }

        static public string GetStrSafeVal(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
        {
            try
            {
                if (selectQuery.Table("query").DefaultView[nIndex].Row[sField] != null &&
                    selectQuery.Table("query").DefaultView[nIndex].Row[sField] != DBNull.Value)
                    return selectQuery.Table("query").DefaultView[nIndex].Row[sField].ToString();
                return "";
            }
            catch
            {
                return "";
            }
        }

        static public string GetStrSafeVal(object val)
        {
            try
            {
                if (val != null && val != DBNull.Value)
                {
                    return val.ToString();
                }

                return string.Empty;

            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        static public Int32 GetIntSafeVal(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
        {
            try
            {
                if (selectQuery.Table("query").DefaultView[nIndex].Row[sField] != null &&
                    selectQuery.Table("query").DefaultView[nIndex].Row[sField] != DBNull.Value)
                    return int.Parse(selectQuery.Table("query").DefaultView[nIndex].Row[sField].ToString());
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        static public DateTime GetDateSafeVal(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
        {
            try
            {
                if (selectQuery.Table("query").DefaultView[nIndex].Row[sField] != null &&
                    selectQuery.Table("query").DefaultView[nIndex].Row[sField] != DBNull.Value)
                    return (DateTime)(selectQuery.Table("query").DefaultView[nIndex].Row[sField]);
                return new DateTime(2000, 1, 1);
            }
            catch
            {
                return new DateTime(2000, 1, 1);
            }
        }

        internal static TvinciPricing.Price GetMediaFileFinalPriceForNonGetItemsPrices(Int32 nMediaFileID, TvinciPricing.PPVModule ppvModule, string sSiteGUID, string sCouponCode, Int32 nGroupID, ref PriceReason theReason, ref TvinciPricing.Subscription relevantSub, ref TvinciPricing.Collection relevantCol,
            ref TvinciPricing.PrePaidModule relevantPP,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            Dictionary<int, int> mediaFileTypesMapping = null;
            List<int> allUsersInDomain = null;
            string sFirstDeviceNameFound = string.Empty;
            int nMediaFileTypeID = 0;
            string sAPIUsername = string.Empty;
            string sAPIPassword = string.Empty;
            string sPricingUsername = string.Empty;
            string sPricingPassword = string.Empty;
            Utils.GetApiAndPricingCredentials(nGroupID, ref sPricingUsername, ref sPricingPassword, ref sAPIUsername, ref sAPIPassword);

            if (nMediaFileID > 0)
            {
                nMediaFileTypeID = GetMediaFileTypeID(nGroupID, nMediaFileID, sAPIUsername, sAPIPassword);
            }
            if (!string.IsNullOrEmpty(sSiteGUID))
            {
                allUsersInDomain = GetAllUsersDomainBySiteGUID(sSiteGUID, nGroupID);

                if (ppvModule != null && ppvModule.m_relatedFileTypes != null && ppvModule.m_relatedFileTypes.Length > 0)
                {
                    /*
                     * 1. In this case GetMediaFileFinalPrice needs the mapping of ppv related file types to media file types from DB.
                     * 2. Otherwise, GetItemsPrices does not need it, and we just send an empty dictionary in order to avoid a DB connection.
                     * 3. GetItemsPrices iterate over this function, hence, if possible, invoke Get_GroupMediaTypesIDs SP just once from GetItemsPrices.
                     */
                    mediaFileTypesMapping = ConditionalAccessDAL.Get_GroupMediaTypesIDs(nGroupID);
                }
                else
                {
                    mediaFileTypesMapping = new Dictionary<int, int>(0);
                }
            }
            else
            {
                allUsersInDomain = new List<int>(0);
                mediaFileTypesMapping = new Dictionary<int, int>(0);
            }
            bool bCancellationWindow = false;
            
            // purchasedBySiteGuid and purchasedAsMediaFileID are only needed in GetItemsPrices.
            string purchasedBySiteGuid = string.Empty;
            int purchasedAsMediaFileID = 0;
            // relatedMediaFileIDs is needed only GetLicensedLinks (which calls GetItemsPrices in order to get to GetMediaFileFinalPrice)
            List<int> relatedMediaFileIDs = new List<int>();

            return GetMediaFileFinalPrice(nMediaFileID, ppvModule, sSiteGUID, sCouponCode, nGroupID, true, ref theReason, ref relevantSub,
                ref relevantCol, ref relevantPP, ref sFirstDeviceNameFound, sCouponCode, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty,
                mediaFileTypesMapping, allUsersInDomain, nMediaFileTypeID, sAPIUsername, sAPIPassword, sPricingUsername, sPricingPassword, 
                ref bCancellationWindow, ref purchasedBySiteGuid, ref purchasedAsMediaFileID, ref relatedMediaFileIDs);
        }

        internal static void GetApiAndPricingCredentials(int nGroupID, ref string sPricingUsername, ref string sPricingPassword,
            ref string sAPIUsername, ref string sAPIPassword)
        {
            Dictionary<string, string[]> dict = ConditionalAccessDAL.Get_MultipleWSCredentials(nGroupID, new List<string>(2) { "api", "pricing" });
            string[] apiDetails = dict["api"];
            sAPIUsername = apiDetails[0];
            sAPIPassword = apiDetails[1];
            string[] pricingDetails = dict["pricing"];
            sPricingUsername = pricingDetails[0];
            sPricingPassword = pricingDetails[1];
        }

        private static List<int> GetMediaTypesOfPPVRelatedFileTypes(int nGroupID, int[] ppvRelatedFileTypes, Dictionary<int, int> mediaFileTypesMapping, ref bool bIsMultiMediaTypes)
        {
            List<int> res = null;
            if (ppvRelatedFileTypes != null && ppvRelatedFileTypes.Length > 0)
            {
                res = new List<int>(ppvRelatedFileTypes.Length);
                if (mediaFileTypesMapping != null && mediaFileTypesMapping.Count > 0)
                {
                    for (int i = 0; i < ppvRelatedFileTypes.Length; i++)
                    {
                        int mediaTypeID = 0;
                        if (mediaFileTypesMapping.TryGetValue(ppvRelatedFileTypes[i], out mediaTypeID) && mediaTypeID > 0)
                        {
                            res.Add(mediaTypeID);
                        }
                    }
                }
            }
            else
            {
                res = new List<int>(0);
            }

            if (res.Count > 1)
            {
                bIsMultiMediaTypes = true;
            }

            return res;
        }

        private static int ExtractMediaIDOutOfMediaMapper(TvinciAPI.MeidaMaper[] mapper, int nMediaFileID)
        {
            for (int i = 0; i < mapper.Length; i++)
            {
                if (mapper[i] != null && mapper[i].m_nMediaFileID == nMediaFileID)
                {
                    return mapper[i].m_nMediaID;
                }
            }

            return 0;
        }

        internal static string GetCachingManagerKey(string sMethodName, string sBusinessModuleCode, int nGroupID, string sCountryCd, string sLanguageCode, string sDeviceName)
        {
            return String.Concat(sMethodName, sBusinessModuleCode, "_", nGroupID, GetLocaleStringForCache(sCountryCd, sLanguageCode, sDeviceName));
        }

        internal static string GetCachingManagerKey(string sMethodName, string sBusinessModuleCode, int nGroupID)
        {
            return GetCachingManagerKey(sMethodName, sBusinessModuleCode, nGroupID, string.Empty, string.Empty, string.Empty);
        }

        private static List<int> GetFileIDs(List<int> mediaFilesList, int nMediaFileID, bool isMultiMediaTypes, int nMediaID)
        {
            if (mediaFilesList != null && mediaFilesList.Count > 0)
                return ConditionalAccessDAL.Get_MediaFileByID(mediaFilesList, nMediaFileID, isMultiMediaTypes, nMediaID);
            return new List<int>(0);
        }

        private static bool IsPurchasedAsPurePPV(string sSubCode, string sPrePaidCode)
        {
            return sSubCode.Length == 0 && sPrePaidCode.Length == 0;
        }

        public static bool IsAnonymousUser(string siteGuid)
        {
            return string.IsNullOrEmpty(siteGuid) || siteGuid.Trim().Equals("0");
        }

        internal static TvinciPricing.Price GetMediaFileFinalPrice(Int32 nMediaFileID, TvinciPricing.PPVModule ppvModule, string sSiteGUID,
            string sCouponCode, Int32 nGroupID, bool bIsValidForPurchase, ref PriceReason theReason, ref TvinciPricing.Subscription relevantSub,
            ref TvinciPricing.Collection relevantCol, ref TvinciPricing.PrePaidModule relevantPP, ref string sFirstDeviceNameFound,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sClientIP, Dictionary<int, int> mediaFileTypesMapping,
            List<int> allUserIDsInDomain, int nMediaFileTypeID, string sAPIUsername, string sAPIPassword, string sPricingUsername,
            string sPricingPassword, ref bool bCancellationWindow, ref string purchasedBySiteGuid, ref int purchasedAsMediaFileID,
            ref List<int> relatedMediaFileIDs)
        {
            if (ppvModule == null)
            {
                theReason = PriceReason.Free;
                return null;
            }

            theReason = PriceReason.UnKnown;
            TvinciPricing.Price p = null;
            Int32[] nMediaFilesIDs = { nMediaFileID };
            TvinciAPI.MeidaMaper[] mapper = GetMediaMapper(nGroupID, nMediaFilesIDs, sAPIUsername, sAPIPassword);

            if (mapper == null || mapper.Length == 0)
                return null;

            int[] fileTypes = new int[1] { nMediaFileTypeID };
            int mediaID = ExtractMediaIDOutOfMediaMapper(mapper, nMediaFileID);

            if (!IsAnonymousUser(sSiteGUID))
            {
                TvinciPricing.mdoule m = null;
                try
                {
                    int[] ppvRelatedFileTypes = ppvModule.m_relatedFileTypes;
                    bool isMultiMediaTypes = false;
                    List<int> mediaFilesList = GetMediaTypesOfPPVRelatedFileTypes(nGroupID, ppvRelatedFileTypes, mediaFileTypesMapping, ref isMultiMediaTypes);

                    List<int> FileIDs = GetFileIDs(mediaFilesList, nMediaFileID, isMultiMediaTypes, mediaID);
                    relatedMediaFileIDs.AddRange(FileIDs);
                    relatedMediaFileIDs = relatedMediaFileIDs.Distinct().ToList();
                    p = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(ppvModule.m_oPriceCode.m_oPrise));

                    bool bEnd = false;

                    int ppvID = 0;
                    string sSubCode = string.Empty;
                    string sPPCode = string.Empty;
                    int nWaiver = 0;
                    DateTime dPurchaseDate = DateTime.MinValue;

                    if (FileIDs.Count > 0 && ConditionalAccessDAL.Get_AllUsersPurchases(allUserIDsInDomain, FileIDs, nMediaFileID, ppvModule.m_sObjectCode, ref ppvID, 
                        ref sSubCode, ref sPPCode, ref nWaiver, ref dPurchaseDate, ref purchasedBySiteGuid, ref purchasedAsMediaFileID))
                    {
                        p.m_dPrice = 0;
                        // Cancellation Window check by ppvUsageModule + purchase date
                        bCancellationWindow = IsCancellationWindowPerPurchase(ppvModule.m_oUsageModule, bCancellationWindow, nWaiver, dPurchaseDate);

                        if (IsPurchasedAsPurePPV(sSubCode, sPPCode))
                        {
                            if (ppvModule.m_bFirstDeviceLimitation && !IsFirstDeviceEqualToCurrentDevice(nMediaFileID, ppvModule.m_sObjectCode, allUserIDsInDomain, sDEVICE_NAME, ref sFirstDeviceNameFound))
                            {
                                theReason = PriceReason.FirstDeviceLimitation;
                            }
                            else
                            {
                                theReason = PriceReason.PPVPurchased;
                            }
                        }
                        else
                        {
                            if (sSubCode.Length > 0)
                            {
                                // purchased as part of subscription
                                theReason = PriceReason.SubscriptionPurchased;
                                Subscription[] sub = GetSubscriptionsDataWithCaching(new List<string>(1) { sSubCode }, sPricingUsername, sPricingPassword, nGroupID);
                                if (sub != null && sub.Length > 0)
                                {
                                    relevantSub = sub[0];
                                }
                                else
                                {
                                    relevantSub = null;
                                }

                            }
                            else
                            {
                                if (sPPCode.Length > 0)
                                {
                                    // purchased as part of pre paid
                                    theReason = PriceReason.PrePaidPurchased;
                                    string sCacheKey = GetCachingManagerKey("GetPrePaidModuleData", sPPCode, nGroupID, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                                    if (CachingManager.CachingManager.Exist(sCacheKey))
                                        relevantPP = (ConditionalAccess.TvinciPricing.PrePaidModule)(CachingManager.CachingManager.GetCachedData(sCacheKey));
                                    else
                                    {
                                        m = new ConditionalAccess.TvinciPricing.mdoule();
                                        string pricingUrl = GetWSURL("pricing_ws");
                                        if (pricingUrl.Length > 0)
                                            m.Url = pricingUrl;
                                        relevantPP = m.GetPrePaidModuleData(sPricingUsername, sPricingPassword, int.Parse(sPPCode), sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                                        CachingManager.CachingManager.SetCachedData(sCacheKey, relevantPP, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                                    }
                                }
                            }
                        }
                        bEnd = true;
                    }
                    else
                    {
                        if (IsPPVModuleToBePurchasedAsSubOnly(ppvModule))
                        {
                            theReason = PriceReason.ForPurchaseSubscriptionOnly;
                        }
                    }


                    if (bEnd || bIsValidForPurchase)
                    {
                        return p;
                    }

                    //check here if it is part of a purchased subscription or part of purchased collections
                    Subscription[] relevantValidSubscriptions = null;
                    Collection[] relevantValidCollections = null;
                    Dictionary<string, KeyValuePair<int, DateTime>> subsPurchase = new Dictionary<string, KeyValuePair<int, DateTime>>();/*dictionary(subscriptionCode, KeyValuePair<nWaiver, dPurchaseDate>)*/
                    Dictionary<string, KeyValuePair<int, DateTime>> collPurchase = new Dictionary<string, KeyValuePair<int, DateTime>>();

                    GetUserValidBundlesFromListOptimized(sSiteGUID, mediaID, nMediaFileID, nGroupID, fileTypes, allUserIDsInDomain, sPricingUsername, sPricingPassword, relatedMediaFileIDs,
                        ref relevantValidSubscriptions, ref relevantValidCollections, ref subsPurchase, ref collPurchase);

                    if (relevantValidSubscriptions != null && relevantValidSubscriptions.Length > 0)
                    {
                        Dictionary<long, List<TvinciPricing.Subscription>> groupedSubs = (from s in relevantValidSubscriptions
                                                                                          group s by s.m_Priority).OrderByDescending(gr => gr.Key).ToDictionary(gr => gr.Key, gr => gr.ToList());

                        if (groupedSubs != null)
                        {
                            List<TvinciPricing.Subscription> prioritySubs = groupedSubs.Values.LastOrDefault();
                            for (int i = 0; i < prioritySubs.Count; i++)
                            {
                                TvinciPricing.Subscription s = prioritySubs[i];
                                TvinciPricing.DiscountModule d = (TvinciPricing.DiscountModule)(s.m_oDiscountModule);
                                TvinciPricing.Price subp = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(CalculateMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule.m_oPriceCode.m_oPrise,
                                    s.m_oDiscountModule, s.m_oCouponsGroup, sSiteGUID, sCouponCode, nGroupID, s.m_sObjectCode, sPricingUsername, sPricingPassword)));
                                if (subp != null)
                                {
                                    if (IsGeoBlock(nGroupID, s.n_GeoCommerceID, sClientIP, sAPIUsername, sAPIPassword))
                                    {
                                        p = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(subp));
                                        relevantSub = TVinciShared.ObjectCopier.Clone<TvinciPricing.Subscription>((TvinciPricing.Subscription)(s));
                                        theReason = PriceReason.GeoCommerceBlocked;
                                    }
                                    else
                                    {
                                        if (IsItemPurchased(p, subp, ppvModule))
                                        {
                                            p = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(subp));
                                            relevantSub = TVinciShared.ObjectCopier.Clone<TvinciPricing.Subscription>((TvinciPricing.Subscription)(s));
                                            theReason = PriceReason.SubscriptionPurchased;
                                        }

                                        bEnd = true;
                                        break;
                                    }
                                }
                            }

                            //cancellationWindow by relevantSub
                            if (relevantSub.m_MultiSubscriptionUsageModule != null && relevantSub.m_MultiSubscriptionUsageModule.Count() > 0)
                            {
                                if (subsPurchase.ContainsKey(relevantSub.m_SubscriptionCode))
                                {
                                    nWaiver = subsPurchase[relevantSub.m_SubscriptionCode].Key;
                                    dPurchaseDate = subsPurchase[relevantSub.m_SubscriptionCode].Value;
                                    bCancellationWindow = IsCancellationWindowPerPurchase(relevantSub.m_MultiSubscriptionUsageModule[0], bCancellationWindow, nWaiver, dPurchaseDate);
                                }
                            }
                        }
                    }

                    if (bEnd)
                    {
                        return p;
                    }

                    // check here if its part of a purchased collection                    

                    if (relevantValidCollections != null)
                    {
                        for (int i = 0; i < relevantValidCollections.Length; i++)
                        {
                            TvinciPricing.Collection collection = (TvinciPricing.Collection)relevantValidCollections[i];
                            TvinciPricing.DiscountModule discount = (TvinciPricing.DiscountModule)(collection.m_oDiscountModule);
                            TvinciPricing.Price collectionsPrice = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(CalculateMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule.m_oPriceCode.m_oPrise, collection.m_oDiscountModule, collection.m_oCouponsGroup, sSiteGUID, sCouponCode, nGroupID, collection.m_sObjectCode, sPricingUsername, sPricingPassword)));
                            if (collectionsPrice != null)
                            {
                                if (IsItemPurchased(p, collectionsPrice, ppvModule))
                                {
                                    p = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(collectionsPrice));
                                    relevantCol = TVinciShared.ObjectCopier.Clone<TvinciPricing.Collection>((TvinciPricing.Collection)(collection));
                                    theReason = PriceReason.CollectionPurchased;
                                    break;
                                }
                            }
                        }

                        //cancellationWindow by relevantSub
                        if (relevantCol.m_oCollectionUsageModule != null)
                        {
                            if (subsPurchase.ContainsKey(relevantCol.m_CollectionCode))
                            {
                                nWaiver = subsPurchase[relevantCol.m_CollectionCode].Key;
                                dPurchaseDate = subsPurchase[relevantCol.m_CollectionCode].Value;
                                bCancellationWindow = IsCancellationWindowPerPurchase(relevantCol.m_oCollectionUsageModule, bCancellationWindow, nWaiver, dPurchaseDate);
                            }
                        }
                    }
                    else
                    {
                        // the media file was not purchased in any way. calculate its price as a single media file and its price reason
                        p = GetMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule, sSiteGUID, sCouponCode, nGroupID, string.Empty,
                            sPricingUsername, sPricingPassword);
                        if (IsFreeMediaFile(theReason, p))
                        {
                            theReason = PriceReason.Free;
                        }
                        else if (theReason != PriceReason.ForPurchaseSubscriptionOnly)
                        {
                            theReason = PriceReason.ForPurchase;
                        }
                    }
                }
                finally
                {
                    #region Disposing
                    if (m != null)
                    {
                        m.Dispose();
                    }
                    #endregion
                }
            } // end if site guid is not null or empty
            else
            {
                p = GetMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule, sSiteGUID, sCouponCode, nGroupID, string.Empty,
                    sPricingUsername, sPricingPassword);

                if (IsPPVModuleToBePurchasedAsSubOnly(ppvModule))
                {
                    theReason = PriceReason.ForPurchaseSubscriptionOnly;
                }
                else
                {
                    theReason = PriceReason.ForPurchase;
                }
            }

            return p;
        }

        private static bool IsPPVModuleToBePurchasedAsSubOnly(PPVModule ppvModule)
        {
            return ppvModule != null && ppvModule.m_bSubscriptionOnly;
        }

        private static bool IsCancellationWindowPerPurchase(TvinciPricing.UsageModule oUsageModule, bool bCancellationWindow, int nWaiver, DateTime dCreateDate)
        {
            try
            {
                if (nWaiver == 0) // user never waiver the cancel transaction option  - so bCancellationWindow = true
                {
                    // check date 
                    if (oUsageModule != null && oUsageModule.m_bWaiver)
                    {
                        DateTime waiverDate = Utils.GetEndDateTime(dCreateDate, oUsageModule.m_nWaiverPeriod); // dCreateDate = ppv purchase date
                        if (DateTime.UtcNow <= waiverDate)
                        {
                            bCancellationWindow = true;
                        }
                    }
                }
                return bCancellationWindow;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool IsFreeMediaFile(PriceReason reason, Price p)
        {
            return p != null && p.m_dPrice == 0 && reason != PriceReason.ForPurchaseSubscriptionOnly;
        }

        private static bool IsItemPurchased(TvinciPricing.Price initialPrice, TvinciPricing.Price businessModulePrice, PPVModule ppvModule)
        {
            return initialPrice == null ||
                (businessModulePrice.m_oCurrency.m_sCurrencyCD3 == ppvModule.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3 && businessModulePrice.m_dPrice <= initialPrice.m_dPrice) ||
                (businessModulePrice.m_oCurrency.m_sCurrencyCD3 != ppvModule.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3 && initialPrice.m_dPrice > 0);
        }

        private static bool IsGeoBlock(int nGroupID, int nSubGeoCommerceID, string sClientIP)
        {
            return IsGeoBlock(nGroupID, nSubGeoCommerceID, sClientIP, string.Empty, string.Empty);
        }

        private static bool IsGeoBlock(int nGroupID, int nSubGeoCommerceID, string sClientIP, string sAPIUsername,
            string sAPIPassword)
        {
            bool res = false;
            if (!string.IsNullOrEmpty(sClientIP))
            {
                using (TvinciAPI.API apiWS = new TvinciAPI.API())
                {
                    string apiUrl = GetWSURL("api_ws");
                    if (apiUrl.Length > 0)
                        apiWS.Url = apiUrl;

                    string apiWSUser = string.Empty;
                    string apiWSPass = string.Empty;
                    if (string.IsNullOrEmpty(sAPIUsername) || string.IsNullOrEmpty(sAPIPassword))
                    {
                        TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "CheckGeoCommerceBlock", "api", "1.1.1.1", ref apiWSUser, ref apiWSPass);
                        res = apiWS.CheckGeoCommerceBlock(apiWSUser, apiWSPass, nSubGeoCommerceID, sClientIP);
                    }
                    else
                    {
                        res = apiWS.CheckGeoCommerceBlock(sAPIUsername, sAPIPassword, nSubGeoCommerceID, sClientIP);
                    }

                }
            }

            return res;
        }

        private static void GetUsersAndDomainsCredentials(int nGroupID, ref string sUsersUsername, ref string sUsersPassword,
            ref string sDomainsUsername, ref string sDomainsPassword)
        {
            Dictionary<string, string[]> dict = ConditionalAccessDAL.Get_MultipleWSCredentials(nGroupID, new List<string>(2) { "users", "domains" });
            string[] usersCreds = dict["users"];
            sUsersUsername = usersCreds[0];
            sUsersPassword = usersCreds[1];
            string[] domainsCreds = dict["domains"];
            sDomainsUsername = domainsCreds[0];
            sDomainsPassword = domainsCreds[1];
        }

        internal static List<int> GetAllUsersDomainBySiteGUID(string sSiteGUID, Int32 nGroupID)
        {
            List<int> lDomainsUsers = new List<int>();

            if (string.IsNullOrEmpty(sSiteGUID) || sSiteGUID.Equals("0"))
            {
                return lDomainsUsers;
            }

            using (TvinciUsers.UsersService u = new TvinciUsers.UsersService())
            {
                string sUsersUsername = string.Empty;
                string sUsersPassword = string.Empty;
                string sDomainsUsername = string.Empty;
                string sDomainsPassword = string.Empty;
                GetUsersAndDomainsCredentials(nGroupID, ref sUsersUsername, ref sUsersPassword, ref sDomainsUsername, ref sDomainsPassword);
                //TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetUserData", "Users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL.Length > 0)
                    u.Url = sWSURL;

                TvinciUsers.UserResponseObject userResponseObj = u.GetUserData(sUsersUsername, sUsersPassword, sSiteGUID);

                if (userResponseObj.m_RespStatus == TvinciUsers.ResponseStatus.OK && userResponseObj.m_user.m_domianID != 0)
                {
                    lDomainsUsers = GetDomainsUsers(userResponseObj.m_user.m_domianID, nGroupID, sDomainsUsername, sDomainsPassword, true);
                }
                else
                {
                    lDomainsUsers.Add(int.Parse(sSiteGUID));
                }
            }

            return lDomainsUsers;
        }
        private static List<int> GetDomainsUsers(int nDomainID, Int32 nGroupID)
        {
            return GetDomainsUsers(nDomainID, nGroupID, string.Empty, string.Empty, true);
        }

        private static List<int> GetDomainsUsers(int nDomainID, Int32 nGroupID, string sDomainsUsername, string sDomainsPassword,
            bool bGetAlsoPendingUsers)
        {
            string sIP = "1.1.1.1";
            List<int> intUsersList = new List<int>();
            using (TvinciDomains.module bm = new ConditionalAccess.TvinciDomains.module())
            {
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                string[] usersList = null;
                string sWSURL = Utils.GetWSURL("domains_ws");
                if (sWSURL.Length > 0)
                    bm.Url = sWSURL;
                if (string.IsNullOrEmpty(sDomainsUsername) || string.IsNullOrEmpty(sDomainsPassword))
                {
                    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetDomainUserList", "Domains", sIP, ref sWSUserName, ref sWSPass);
                    usersList = bm.GetDomainUserList(sWSUserName, sWSPass, nDomainID);
                }
                else
                {
                    usersList = bm.GetDomainUserList(sDomainsUsername, sDomainsPassword, nDomainID);
                }
                if (usersList != null && usersList.Length > 0)
                {
                    for (int i = 0; i < usersList.Length; i++)
                    {
                        int temp = 0;
                        // pending users are returned with domains with a minus before their site guid.
                        // for example: site 123456 which is pending in the domain will be returned as -123456
                        if (Int32.TryParse(usersList[i], out temp) && (bGetAlsoPendingUsers || temp > 0))
                        {
                            intUsersList.Add(temp);
                        }
                    }
                }
            }

            return intUsersList;
        }

        static public Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseConditionalAccess t)
        {
            string sIP = TVinciShared.PageUtils.GetCallerIP();
            Int32 nGroupID = TVinciShared.WS_Utils.GetGroupID("conditionalaccess", sFunctionName, sWSUserName, sWSPassword, sIP);
            if (nGroupID != 0)
            {
                Utils.GetBaseConditionalAccessImpl(ref t, nGroupID);
            }
            else
            {
                Logger.Logger.Log("WS ignored", "IP: " + sIP + ",Function: " + sFunctionName + " UN: " + sWSUserName + " Pass: " + sWSPassword, "pricing");
            }

            return nGroupID;
        }

        static public double GetCouponDiscountPercent(Int32 nGroupID, string sCouponCode)
        {
            double dCouponDiscountPercent = 0;

            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";

            using (TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule())
            {
                string pricingUrl = Utils.GetWSURL("pricing_ws");
                if (pricingUrl.Length > 0)
                    m.Url = pricingUrl;

                TvinciPricing.CouponData theCouponData = null;

                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetCouponStatus", "pricing", sIP, ref sWSUserName, ref sWSPass);
                theCouponData = m.GetCouponStatus(sWSUserName, sWSPass, sCouponCode);

                if (theCouponData.m_oCouponGroup != null &&
                    theCouponData.m_CouponStatus == ConditionalAccess.TvinciPricing.CouponsStatus.Valid &&
                    theCouponData.m_oCouponGroup.m_sGroupCode == theCouponData.m_oCouponGroup.m_sGroupCode)
                {

                    TvinciPricing.DiscountModule dCouponDiscount = theCouponData.m_oCouponGroup.m_oDiscountCode;
                    dCouponDiscountPercent = dCouponDiscount.m_dPercent;
                }
            }

            return dCouponDiscountPercent;
        }

        static public string GetMediaFileCoGuid(int nGroupID, int nMediaFileID)
        {
            string sMediaFileCoGuid =
                DAL.ConditionalAccessDAL.GetMediaFileCoGuid(nGroupID, nMediaFileID);

            return sMediaFileCoGuid;
        }

        static public TvinciPricing.Subscription GetSubscriptionBytProductCode(Int32 nGroupID, string sProductCode, string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive)
        {

            using (TvinciPricing.mdoule p = new TvinciPricing.mdoule())
            {
                string sIP = "1.1.1.1";
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetSubscriptionByProductCode", "pricing", sIP, ref sWSUserName, ref sWSPass);

                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (sWSURL.Length > 0)
                    p.Url = sWSURL;

                return p.GetSubscriptionDataByProductCode(sWSUserName, sWSPass, sProductCode, sCountryCd2, sLanguageCode3, sDeviceName, bGetAlsoUnActive);
            }
        }

        internal static string GetBasicLink(int nGroupID, int[] nMediaFileIDs, int nMediaFileID, string sBasicLink, out int nStreamingCompanyID)
        {

            TvinciAPI.MeidaMaper[] mapper = GetMediaMapper(nGroupID, nMediaFileIDs);
            nStreamingCompanyID = 0;
            int mediaID = 0;
            if (mapper != null && mapper.Length > 0)
            {
                mediaID = ExtractMediaIDOutOfMediaMapper(mapper, nMediaFileID);
            }

            if (sBasicLink.Equals(string.Format("{0}||{1}", mediaID, nMediaFileID)))
            {
                string sBaseURL = string.Empty;
                string sStreamID = string.Empty;

                ConditionalAccessDAL.Get_BasicLinkData(nMediaFileID, ref sBaseURL, ref sStreamID, ref nStreamingCompanyID);

                sBasicLink = string.Format("{0}{1}", sBaseURL, sStreamID);
                if (sStreamID.Length > 0)
                {
                    string groupCountryCode = string.Empty;
                    string groupSecretCode = string.Empty;
                    ConditionalAccessDAL.Get_GroupSecretAndCountryCode(nGroupID, ref groupSecretCode, ref groupCountryCode);
                    if (sBasicLink.Contains(BASIC_LINK_COUNTRY_CODE))
                    {
                        sBasicLink = sBasicLink.Replace(BASIC_LINK_COUNTRY_CODE, groupCountryCode.Trim().ToLower());
                    }

                    if (sBasicLink.Contains(BASIC_LINK_TICK_TIME))
                    {
                        long lT = DateTime.UtcNow.Ticks;
                        sBasicLink = sBasicLink.Replace(BASIC_LINK_TICK_TIME, String.Concat("tick=", lT.ToString()));
                        string sToHash = string.Empty;
                        string sHashed = string.Empty;
                        sToHash = String.Concat(groupSecretCode, lT);
                        sHashed = TVinciShared.ProtocolsFuncs.CalculateMD5Hash(sToHash);
                        sBasicLink = sBasicLink.Replace(BASIC_LINK_HASH, String.Concat("hash=", sHashed));
                    }
                    if (sBasicLink.Contains(BASIC_LINK_GROUP))
                    {
                        sBasicLink = sBasicLink.Replace(BASIC_LINK_GROUP, String.Concat("group=", nGroupID.ToString()));
                    }
                    if (sBasicLink.Contains(BASIC_LINK_CONFIG_DATA))
                    {
                        sBasicLink = sBasicLink.Replace(BASIC_LINK_CONFIG_DATA, "brt=");
                    }
                }
                sBasicLink = HttpContext.Current.Server.HtmlDecode(sBasicLink).Replace("''", "\"");
            }
            return sBasicLink;
        }

        public static int GetGroupFAILCOUNT(int nGroupID, string sConnKey)
        {
            int res = ConditionalAccessDAL.Get_GroupFailCount(nGroupID, sConnKey);
            return res > 0 ? res : DEFAULT_MPP_RENEW_FAIL_COUNT;
        }

        public static int GetGroupFAILCOUNT(int nGroupID)
        {
            return GetGroupFAILCOUNT(nGroupID, string.Empty);
        }

        static public string GetSafeParValue(string sQueryKey, string sParName, ref System.Xml.XmlNode theRoot)
        {
            try
            {
                return theRoot.SelectSingleNode(sQueryKey).Attributes[sParName].Value;
            }
            catch
            {
                return "";
            }
        }

        static public string GetSafeValue(string sQueryKey, ref System.Xml.XmlNode theRoot)
        {
            try
            {
                return theRoot.SelectSingleNode(sQueryKey).FirstChild.Value;
            }
            catch
            {
                return "";
            }
        }
        static public void SplitRefference(string sRefference, ref Int32 nMediaFileID, ref Int32 nMediaID, ref string sSubscriptionCode, ref string sPPVCode, ref string sPrePaidCode,
           ref string sPriceCode, ref double dPrice, ref string sCurrencyCd, ref bool bIsRecurring, ref string sPPVModuleCode,
           ref Int32 nNumberOfPayments, ref string sSiteGUID, ref string sRelevantSub, ref Int32 nMaxNumberOfUses,
           ref Int32 nMaxUsageModuleLifeCycle, ref Int32 nViewLifeCycleSecs, ref string sPurchaseType,
           ref string sCountryCd, ref string sLanguageCd, ref string sDeviceName)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(sRefference);
            System.Xml.XmlNode theRequest = doc.FirstChild;

            bIsRecurring = false;
            string sType = Utils.GetSafeParValue(".", "type", ref theRequest);
            string sSubscriptionID = Utils.GetSafeValue("s", ref theRequest);
            string scouponcode = Utils.GetSafeValue("cc", ref theRequest);
            string sPayNum = Utils.GetSafeParValue("//p", "n", ref theRequest);
            string sPayOutOf = Utils.GetSafeParValue("//p", "o", ref theRequest);
            string suid = Utils.GetSafeParValue("//u", "id", ref theRequest);
            string smedia_file = GetSafeValue("mf", ref theRequest);
            string smedia_id = GetSafeValue("m", ref theRequest);
            string ssub = Utils.GetSafeValue("s", ref theRequest);
            string sPP = Utils.GetSafeValue("pp", ref theRequest);
            string sppvmodule = Utils.GetSafeValue("ppvm", ref theRequest);
            string srelevantsub = Utils.GetSafeValue("rs", ref theRequest);
            string smnou = Utils.GetSafeValue("mnou", ref theRequest);
            string smaxusagemodulelifecycle = Utils.GetSafeValue("mumlc", ref theRequest);
            string sviewlifecyclesecs = Utils.GetSafeValue("vlcs", ref theRequest);
            string sppvcode = Utils.GetSafeValue("ppvm", ref theRequest);
            string spc = Utils.GetSafeValue("pc", ref theRequest);
            string spri = Utils.GetSafeValue("pri", ref theRequest);
            string scur = Utils.GetSafeValue("cu", ref theRequest);
            string sir = Utils.GetSafeParValue("//p", "ir", ref theRequest);
            string srs = Utils.GetSafeValue("rs", ref theRequest);

            string slcc = Utils.GetSafeValue("lcc", ref theRequest);
            if (!string.IsNullOrEmpty(slcc))
            {
                sCountryCd = slcc;
            }
            string sllc = Utils.GetSafeValue("llc", ref theRequest);
            sLanguageCd = sllc;
            string sldn = Utils.GetSafeValue("ldn", ref theRequest);
            sDeviceName = sldn;

            if (smedia_file != "")
                nMediaFileID = int.Parse(smedia_file);
            if (smedia_id != "")
                nMediaID = int.Parse(smedia_id);
            sSubscriptionCode = sSubscriptionID;
            sPPVCode = sppvcode;
            sPrePaidCode = sPP;
            sPriceCode = spc;
            if (spri != "")
                dPrice = double.Parse(spri);
            sCurrencyCd = scur;
            if (sir == "true")
                bIsRecurring = true;
            sPPVModuleCode = sppvmodule;
            if (sPayOutOf != "")
                nNumberOfPayments = int.Parse(sPayOutOf);
            sSiteGUID = suid;
            sRelevantSub = srs;
            if (smnou != "")
                nMaxNumberOfUses = int.Parse(smnou);
            if (smaxusagemodulelifecycle != "")
                nMaxUsageModuleLifeCycle = int.Parse(smaxusagemodulelifecycle);
            if (sviewlifecyclesecs != "")
                nViewLifeCycleSecs = int.Parse(sviewlifecyclesecs);
            sPurchaseType = sType;
        }

        static public string GetGoogleSignature(int nGroupID, int nCustomDataID)
        {
            string MY_SELLER_ID = "06511210546291891713"; //"YOUR SELLER ID";
            string MY_SELLER_SECRET = "hRVpATY0ZIsANB0gv756OQ"; //"YOUR SELLER SECRET";

            JWTHeaderObject HeaderObj = null;
            InAppItemObject ClaimObj = null;

            #region Reset callback custom data varibles
            string price = string.Empty;
            string currencyCode = string.Empty;
            string sSiteGUID = string.Empty;
            string assetID = string.Empty;
            string ppvOrSub = string.Empty;
            string sPrePaidID = string.Empty;
            string smedia_file = string.Empty;
            string sSubscriptionID = string.Empty;
            string sType = string.Empty;
            string scouponcode = string.Empty;
            string sPayNum = string.Empty;
            string sPayOutOf = string.Empty;
            string sppvmodule = string.Empty;
            string srelevantsub = string.Empty;
            string smnou = string.Empty;
            string smaxusagemodulelifecycle = string.Empty;
            string sviewlifecyclesecs = string.Empty;
            string sDigits = string.Empty;
            string sCountryCode = string.Empty;
            string sLangCode = string.Empty;
            string sDevice = string.Empty;
            string scurrency = string.Empty;
            string isRecurringStr = string.Empty;
            string sPPCreditValue = string.Empty;
            string sUserIP = string.Empty;
            string sCampCode = string.Empty;
            string sCampMNOU = string.Empty;
            string sCampLS = string.Empty;
            int nBillingTransactionID = 0;
            #endregion

            //The custom data is created by calling the AD_GetCustomDataID function in the CA/ 
            string sCustomData = GetCustomData(nCustomDataID);
            if (sCustomData != "")
            {
                #region Parse custom data xml

                //Parse the custom data xml
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.LoadXml(sCustomData);
                System.Xml.XmlNode theRequest = doc.FirstChild;

                sType = GetSafeParValue(".", "type", ref theRequest);
                sSiteGUID = GetSafeParValue("//u", "id", ref theRequest);
                sSubscriptionID = GetSafeValue("s", ref theRequest);
                sPrePaidID = GetSafeValue("pp", ref theRequest);
                sPPCreditValue = GetSafeValue("cpri", ref theRequest);
                scouponcode = GetSafeValue("cc", ref theRequest);
                sPayNum = GetSafeParValue("//p", "n", ref theRequest);
                sPayOutOf = GetSafeParValue("//p", "o", ref theRequest);
                isRecurringStr = GetSafeParValue("//p", "ir", ref theRequest);
                smedia_file = GetSafeValue("mf", ref theRequest);
                sppvmodule = GetSafeValue("ppvm", ref theRequest);
                srelevantsub = GetSafeValue("rs", ref theRequest);
                smnou = GetSafeValue("mnou", ref theRequest);
                sCountryCode = GetSafeValue("lcc", ref theRequest);
                sLangCode = GetSafeValue("llc", ref theRequest);
                sDevice = GetSafeValue("ldn", ref theRequest);
                smaxusagemodulelifecycle = GetSafeValue("mumlc", ref theRequest);
                sviewlifecyclesecs = GetSafeValue("vlcs", ref theRequest);
                sDigits = GetSafeValue("cc_card_number", ref theRequest);
                price = GetSafeValue("pri", ref theRequest);
                scurrency = GetSafeValue("cu", ref theRequest);
                sUserIP = GetSafeValue("up", ref theRequest);
                sCampCode = GetSafeValue("campcode", ref theRequest);
                sCampMNOU = GetSafeValue("cmnov", ref theRequest);
                sCampLS = GetSafeValue("cmumlc", ref theRequest);
                if (price == "")
                    price = "0.0";
                Int32 nPaymentNum = 0;
                Int32 nNumberOfPayments = 0;
                if (sPayNum != "")
                    nPaymentNum = int.Parse(sPayNum);
                if (sPayOutOf != "")
                    nNumberOfPayments = int.Parse(sPayOutOf);

                int nType = 1;
                if (sType == "sp")
                {
                    nType = 2;
                }
                else if (sType == "prepaid")
                {
                    nType = 3;
                }

                #endregion

                Int32 nMediaFileID = 0;
                Int32 nMediaID = 0;
                string sSubscriptionCode = "";
                string sPPVCode = "";
                string sPriceCode = "";
                string sPPVModuleCode = "";
                bool bIsRecurring = false;
                string sCurrencyCode = "";
                double dChargePrice = 0.0;
                Int32 nStatus = 0;
                string sRelevantSub = "";
                string sUserGUID = "";
                Int32 nMaxNumberOfUses = 0;
                Int32 nMaxUsageModuleLifeCycle = 0;
                Int32 nViewLifeCycleSecs = 0;
                string sPurchaseType = "";

                string sCountryCd = "";
                string sLanguageCode = "";
                string sDeviceName = "";
                string sPrePaidCode = string.Empty;


                SplitRefference(sCustomData, ref nMediaFileID, ref nMediaID, ref sSubscriptionCode, ref sPPVCode, ref sPrePaidCode, ref sPriceCode,
                        ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPayments, ref sUserGUID,
                                    ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs, ref sPurchaseType,
                                    ref sCountryCd, ref sLanguageCode, ref sDeviceName);

                if (!string.IsNullOrEmpty(sCampCode))
                {
                    int nCampCode = int.Parse(sCampCode);
                    if (nCampCode > 0)
                    {
                        //HandleCampaignUse(nCampCode, sSiteGUID, int.Parse(sCampMNOU), sCampLS);
                    }
                }
                using (TvinciPricing.mdoule p = new TvinciPricing.mdoule())
                {
                    string sIP = "1.1.1.1";
                    string sWSUserName = "";
                    string sWSPass = "";
                    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetGoogleSignature", "pricing", sIP, ref sWSUserName, ref sWSPass);

                    string sWSURL = Utils.GetWSURL("pricing_ws");
                    if (sWSURL.Length > 0)
                        p.Url = sWSURL;



                    switch (sType)
                    {
                        case "pp":
                            #region Handle PPV Transaction

                            HeaderObj = new JWTHeaderObject(JWTHeaderObject.JWTHash.HS256, "1", "JWT");
                            PPVModule pp = p.GetPPVModuleData(sWSUserName, sWSPass, sPPVModuleCode, sCountryCd, sLanguageCode, sDeviceName);


                            var pp_description = (from descValue in pp.m_sDescription
                                                  where descValue.m_sLanguageCode3 == sLanguageCode
                                                  select descValue.m_sValue.ToString()).FirstOrDefault();

                            ClaimObj = new InAppItemObject(pp.m_sObjectVirtualName, pp_description.ToString(), dChargePrice.ToString(), sCurrencyCode, nCustomDataID.ToString(), MY_SELLER_ID, 60, "Google", "google/payments/inapp/item/v1", 0);

                            //purchaseSuccess = HandlePPVTransaction(groupID, srelevantsub, smedia_file, sSiteGUID, paymentMethod, price, scurrency, sCustomData, sCountryCode, sLangCode, sDevice, smnou, nBillingTransactionID, smaxusagemodulelifecycle, plimusID);
                            //if (!string.IsNullOrEmpty(scouponcode))
                            //{
                            //    HandleCouponUse(scouponcode, sSiteGUID, int.Parse(smedia_file), srelevantsub, groupID);
                            //}
                            #endregion
                            break;
                        case "sp":
                            #region Subscription Purchase

                            HeaderObj = new JWTHeaderObject(JWTHeaderObject.JWTHash.HS256, "1", "JWT");

                            Subscription sp = p.GetSubscriptionData(sWSUserName, sWSPass, sSubscriptionCode, sCountryCd, sLanguageCode, sDeviceName, false);

                            DateTime nextdate = GetEndDateTime(DateTime.Now, sp.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle);
                            string fequencey = "";
                            if (nextdate.Month > DateTime.Now.Month)
                            {
                                fequencey = "monthly";
                            }
                            else if (nextdate.Year > DateTime.Now.Year)
                            {
                                fequencey = "yearly";
                            }


                            var sp_description = (from descValue in sp.m_sDescription
                                                  where descValue.m_sLanguageCode3 == sLanguageCode
                                                  select descValue.m_sValue.ToString()).FirstOrDefault();


                            string sNumberOfRecPeriods = sp.m_nNumberOfRecPeriods == 0 ? null : sp.m_nNumberOfRecPeriods.ToString();
                            //ClaimObj = new InAppItemObject("Piece of Cake", "A delicious piece of virtual cake", "10.50", "USD", "prorated", "Your Data Here", MY_SELLER_ID, 60, "4.99", "USD", "1360171852", "monthly", "12", "Google", "google/payments/inapp/subscription/v1", 0);
                            ClaimObj = new InAppItemObject(sp.m_sObjectVirtualName, sp_description.ToString(), dChargePrice.ToString(), scurrency, "prorated", nCustomDataID.ToString(), MY_SELLER_ID, 60, dChargePrice.ToString(), scurrency, "", fequencey, sNumberOfRecPeriods, "Google", "google/payments/inapp/subscription/v1", 0);

                            //purchaseSuccess = HandleSubscrptionTransaction(groupID, sSubscriptionID, sSiteGUID, paymentMethod, price, scurrency, sCustomData, sCountryCode, sLangCode, sDevice, smnou, sviewlifecyclesecs, isRecurringStr, smaxusagemodulelifecycle, nBillingTransactionID, plimusID);
                            //if (!string.IsNullOrEmpty(scouponcode))
                            //{
                            //    HandleCouponUse(scouponcode, sSiteGUID, 0, sSubscriptionID, groupID);
                            //}

                            #endregion
                            break;
                        case "prepaid":
                            #region Handle PrePaid Transaction

                            //purchaseSuccess = HandlePrePaidTransaction(groupID, sPrePaidID, sSiteGUID, paymentMethod, price, sPPCreditValue, scurrency, sCustomData, sCountryCode, sLangCode, sDevice, smnou, smaxusagemodulelifecycle, nBillingTransactionID, plimusID);

                            #endregion
                            break;
                    }
                }


            }
            return JWTHelpers.buildJWT(HeaderObj, ClaimObj, MY_SELLER_SECRET);


        }

        internal static bool CheckStartDateBeforeEndDate(DateTime startDate, DateTime endDate)
        {
            return (startDate.CompareTo(endDate) < 0); // If true, then startDate is earlier than endDate
        }

        internal static long ConvertDateToEpochTimeInMilliseconds(DateTime dateTime)
        {
            return long.Parse((Math.Floor(dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds).ToString()));
        }

        internal static void ReplaceSubStr(ref string url, Dictionary<string, object> oValuesToReplace)
        {
            if (oValuesToReplace.Count > 0)
            {
                foreach (KeyValuePair<string, object> pair in oValuesToReplace)
                {
                    string sKeyToSearch = string.Format("{0}{1}{2}", "{", pair.Key, "}");
                    if (url.Contains(sKeyToSearch))
                    {
                        url = url.Replace(sKeyToSearch, pair.Value.ToString());
                    }
                }
            }
        }

        internal static eStreamType GetStreamType(string sBaseLink)
        {
            eStreamType streamType = eStreamType.HLS;

            if ((sBaseLink.ToLower().Contains("ism")) && (sBaseLink.ToLower().Contains("manifest")))
            {
                streamType = eStreamType.SS;
            }
            else if (sBaseLink.Contains(".m3u8"))
            {
                streamType = eStreamType.HLS;
            }
            else if (sBaseLink.Contains(".mpd"))
            {
                streamType = eStreamType.DASH;
            }

            return streamType;
        }

        internal static string GetStreamTypeAndFormatLink(eStreamType streamType, eEPGFormatType format)
        {
            string url = string.Empty;
            string urlConfig = string.Empty;
            switch (format)
            {
                case eEPGFormatType.Catchup:
                    {
                        switch (streamType)
                        {
                            case eStreamType.HLS:
                                urlConfig = "hls_catchup";
                                break;
                            case eStreamType.SS:
                                urlConfig = "smooth_catchup";
                                break;
                            case eStreamType.DASH:
                                urlConfig = "dash_catchup";
                                break;
                            default:
                                break;
                        }
                    }
                    break;

                case eEPGFormatType.StartOver:
                    {
                        switch (streamType)
                        {
                            case eStreamType.HLS:
                                urlConfig = "hls_start_over";
                                break;
                            case eStreamType.SS:
                                urlConfig = "smooth_start_over";
                                break;
                            case eStreamType.DASH:
                                urlConfig = "dash_start_over";
                                break;
                            default:
                                break;
                        }
                    }
                    break;

                case eEPGFormatType.LivePause:
                    {
                        switch (streamType)
                        {
                            case eStreamType.HLS:
                                urlConfig = "hls_start_over";
                                break;
                            case eStreamType.SS:
                                urlConfig = "smooth_start_over";
                                break;
                            case eStreamType.DASH:
                                urlConfig = "dash_start_over";
                                break;
                            default:
                                break;
                        }
                    }
                    break;

                default:
                    break;
            }

            if (!string.IsNullOrEmpty(urlConfig))
                url = Utils.GetValueFromConfig(urlConfig);

            return url;
        }


        /*
         * 1. The user is entitled to preview module iff 
         *    The MPP has a valid preview module object AND
         *        (a. The user did not purchase preview module before OR
         *         b. User purchased a preview module before and satsifies the following property:
         *            (billing transactions row's create date of this MPP with preview module + full life cycle of the 
         *            preview module + non renewing period of preview module < datetime.utcnow)
         *         )
         *  2. According to MCORP-1723 specification document, free trial non renewing period is counted from the day
         *     the preview module full life cycle expires.
         *  3. The procedure of purchasing an MPP with preview module within customers who use Adyen is the following:
         *      a. If we hold the user's CC details, we just dummy charge him
         *      b. Otherwise, we charge him with a minimum amount, and automatically issue to Adyen a
         *         cancelOrRefund request. Hence, in this method, we extract from configuration a minimum amount to
         *         charge the user.
         */

        private static bool IsEntitledToPreviewModule(string sSiteGUID, Int32 nGroupID, string sSubCode, TvinciPricing.Subscription s, ref TvinciPricing.Price p, ref PriceReason theReason)
        {
            bool res = true;
            if (s.m_oPreviewModule == null || s.m_oPreviewModule.m_nID == 0)
                return false;
            Dictionary<DateTime, List<int>> dict = GetPreviewModuleDataRelatedToUserFromDB(sSiteGUID, nGroupID, sSubCode);
            if (dict != null)
            {
                DateTime dtUtcNow = DateTime.UtcNow;
                foreach (KeyValuePair<DateTime, List<int>> kvp in dict)
                {
                    DateTime dtPreviousPreviewModuleStartDate = kvp.Key;
                    DateTime dtEndDateOfPreviousPreviewModule = GetEndDateTime(dtPreviousPreviewModuleStartDate, kvp.Value[1]);
                    DateTime dtEndDateOfNonRenewingPeriod = GetEndDateTime(dtEndDateOfPreviousPreviewModule, kvp.Value[2]);
                    if (dtUtcNow <= dtEndDateOfNonRenewingPeriod)
                    {
                        res = false;
                        break;
                    }
                }
            }
            if (res)
            {
                string sKeyOfMinPrice = String.Concat("PreviewModuleMinPrice", nGroupID);
                double dMinPriceForPreviewModule = DEFAULT_MIN_PRICE_FOR_PREVIEW_MODULE;
                if (GetValueFromConfig(sKeyOfMinPrice) != string.Empty)
                    double.TryParse(GetValueFromConfig(sKeyOfMinPrice), out dMinPriceForPreviewModule);
                p.m_dPrice = dMinPriceForPreviewModule;
                theReason = PriceReason.EntitledToPreviewModule;
            }
            return res;
        }

        private static Dictionary<DateTime, List<int>> GetPreviewModuleDataRelatedToUserFromDB(string sSiteGuid, int nGroupID, string sSubCode)
        {
            Dictionary<DateTime, List<int>> res = null;
            DataTable dt = ConditionalAccessDAL.Get_PreviewModuleDataForEntitlementCalc(nGroupID, sSiteGuid, sSubCode);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    bool bIsParsingSuccessful = true;
                    DateTime dtStartDateOfMPP = DateTime.MaxValue;
                    int nPreviewModuleID = 0;
                    int nFullLifeCycleOfPreviewModule = 0;
                    int nNonRenewingPeriod = 0;
                    if (dt.Rows[i]["create_date"] != DBNull.Value && dt.Rows[i]["create_date"] != null)
                        bIsParsingSuccessful = DateTime.TryParse(dt.Rows[i]["create_date"].ToString(), out dtStartDateOfMPP);
                    if (dt.Rows[i]["preview_module_id"] != DBNull.Value && dt.Rows[i]["preview_module_id"] != null)
                        bIsParsingSuccessful = Int32.TryParse(dt.Rows[i]["preview_module_id"].ToString(), out nPreviewModuleID);
                    if (dt.Rows[i]["full_life_cycle_id"] != DBNull.Value && dt.Rows[i]["full_life_cycle_id"] != null)
                        bIsParsingSuccessful = Int32.TryParse(dt.Rows[i]["full_life_cycle_id"].ToString(), out nFullLifeCycleOfPreviewModule);
                    if (dt.Rows[i]["non_renewing_period_id"] != DBNull.Value && dt.Rows[i]["non_renewing_period_id"] != null)
                        bIsParsingSuccessful = Int32.TryParse(dt.Rows[i]["non_renewing_period_id"].ToString(), out nNonRenewingPeriod);
                    if (bIsParsingSuccessful)
                    {
                        if (res == null)
                            res = new Dictionary<DateTime, List<int>>();
                        List<int> lst = new List<int>(3);
                        lst.Add(nPreviewModuleID);
                        lst.Add(nFullLifeCycleOfPreviewModule);
                        lst.Add(nNonRenewingPeriod);
                        res.Add(dtStartDateOfMPP, lst);
                    }
                }
            }

            return res;
        }

        public static long ParseLongIfNotEmpty(string sStrToParse)
        {
            if (sStrToParse.Length > 0)
                return Int64.Parse(sStrToParse);
            return 0;
        }

        public static UserResponseObject GetExistUser(string sSiteGUID, int nGroupID)
        {
            ConditionalAccess.TvinciUsers.UserResponseObject res = null;
            TvinciUsers.UsersService u = null;
            try
            {
                u = new ConditionalAccess.TvinciUsers.UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;

                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetExistUser", "users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL.Length > 0)
                    u.Url = sWSURL;

                res = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                }
                #endregion
            }
            return res;
        }

        static public bool IsCouponValid(int nGroupID, string sCouponCode)
        {
            bool result = true;
            TvinciPricing.mdoule p = null;
            try
            {
                if (!string.IsNullOrEmpty(sCouponCode))
                {
                    p = new TvinciPricing.mdoule();
                    string sIP = "1.1.1.1";
                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetCouponStatus", "pricing", sIP, ref sWSUserName, ref sWSPass);

                    string sWSURL = Utils.GetWSURL("pricing_ws");
                    if (sWSURL.Length > 0)
                        p.Url = sWSURL;
                    TvinciPricing.CouponData couponData = p.GetCouponStatus(sWSUserName, sWSPass, sCouponCode);
                    if (couponData != null && couponData.m_CouponStatus != TvinciPricing.CouponsStatus.Valid)
                    {
                        result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                Logger.Logger.Log("IsCouponValid", string.Format("Error on IsCouponValid(), group id:{0}, coupon code:{1}, errorMessage:{2}", nGroupID, sCouponCode, ex.ToString()), "ConditionalAccessUtils");
            }
            finally
            {
                #region Disposing
                if (p != null)
                {
                    p.Dispose();
                }
                #endregion
            }
            return result;
        }


        internal static bool IsFirstDeviceEqualToCurrentDevice(int nMediaFileID, string sPPVCode, List<int> lUsersIds, string sCurrentDeviceName, ref string sFirstDeviceName)
        {
            int numOfRowsReturned = 0;

            sFirstDeviceName = ConditionalAccessDAL.Get_FirstDeviceUsedByPPVModule(nMediaFileID, sPPVCode, lUsersIds, out numOfRowsReturned);
            if (numOfRowsReturned == 0)
                return true;
            return sCurrentDeviceName.Equals(sFirstDeviceName);
        }

        internal static TvinciPricing.PPVModule GetPPVModuleDataWithCaching<T>(T ppvCode, string wsUsername, string wsPassword,
            int groupID, string countryCd, string langCode, string deviceName)
        {
            PPVModule res = null;
            string ppvCodeStr = ppvCode.ToString();
            string cacheKey = GetCachingManagerKey("GetPPVModuleData", ppvCodeStr, groupID);
            if (CachingManager.CachingManager.Exist(cacheKey))
                res = (TvinciPricing.PPVModule)(CachingManager.CachingManager.GetCachedData(cacheKey));
            else
            {
                using (TvinciPricing.mdoule m = new TvinciPricing.mdoule())
                {
                    string pricingUrl = GetWSURL("pricing_ws");
                    if (pricingUrl.Length > 0)
                        m.Url = pricingUrl;
                    res = m.GetPPVModuleData(wsUsername, wsPassword, ppvCodeStr, countryCd, langCode, deviceName);
                    if (res != null)
                    {
                        CachingManager.CachingManager.SetCachedData(cacheKey, res, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    }
                }
            }

            return res;
        }

        internal static TvinciPricing.UsageModule GetUsageModuleDataWithCaching<T>(T usageModuleCode, string wsUsername, string wsPassword,
            string countryCode, string langCode, string deviceName, int groupID, string methodName)
        {
            UsageModule res = null;
            string usageModuleCodeStr = usageModuleCode.ToString();
            string cacheKey = GetCachingManagerKey(string.IsNullOrEmpty(methodName) ? "GetUsageModuleData" : methodName, usageModuleCodeStr, groupID);

            if (CachingManager.CachingManager.Exist(cacheKey))
                res = (TvinciPricing.UsageModule)(CachingManager.CachingManager.GetCachedData(cacheKey));
            else
            {
                using (TvinciPricing.mdoule m = new TvinciPricing.mdoule())
                {
                    res = m.GetUsageModuleData(wsUsername, wsPassword, usageModuleCodeStr, countryCode, langCode, deviceName);
                    if (res != null)
                    {
                        CachingManager.CachingManager.SetCachedData(cacheKey, res, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    }
                }
            }

            return res;
        }

        internal static bool GetMediaFileIDByCoGuid(string coGuid, int groupID, string siteGuid, ref int mediaFileID)
        {
            bool res = false;
            WS_Catalog.MediaFilesRequest request = new WS_Catalog.MediaFilesRequest();
            request.m_lMediaFileIDs = new int[0];
            request.m_nGroupID = groupID;
            request.m_oFilter = new WS_Catalog.Filter();
            request.m_sSiteGuid = siteGuid;
            request.m_sUserIP = string.Empty;
            request.m_sSignString = Guid.NewGuid().ToString();
            request.m_sSignature = TVinciShared.WS_Utils.GetCatalogSignature(request.m_sSignString, Utils.GetWSURL("CatalogSignatureKey"));
            request.m_lCoGuids = new string[1] { coGuid };
            using (WS_Catalog.IserviceClient catalog = new WS_Catalog.IserviceClient())
            {
                catalog.Endpoint.Address = new System.ServiceModel.EndpointAddress(GetWSURL("WS_Catalog"));
                WS_Catalog.MediaFilesResponse response = catalog.GetResponse(request) as WS_Catalog.MediaFilesResponse;
                if (response != null && response.m_lObj != null && response.m_lObj.Length > 0)
                {
                    WS_Catalog.MediaFileObj mf = response.m_lObj[0] as WS_Catalog.MediaFileObj;
                    if (mf != null && mf.m_oFile != null)
                    {
                        res = true;
                        mediaFileID = mf.m_oFile.m_nFileId;
                    }
                }
            }

            return res;
        }

        internal static int GetCountryIDByIP(string sIP)
        {         
            int retCountryID = 0;

            if (!string.IsNullOrEmpty(sIP))
            {
                long nIPVal = 0;
                string[] splited = sIP.Split('.');

                if (splited != null && splited.Length >= 3)
                {
                    nIPVal = long.Parse(splited[3]) + Int64.Parse(splited[2]) * 256 + Int64.Parse(splited[1]) * 256 * 256 + Int64.Parse(splited[0]) * 256 * 256 * 256;
                }

                DataTable dtCountry = ApiDAL.Get_IPCountryCode(nIPVal);
                if (dtCountry != null && dtCountry.Rows.Count > 0)
                {
                    retCountryID = ODBCWrapper.Utils.GetIntSafeVal(dtCountry.Rows[0], "Country_ID");
                }
            }
            return retCountryID;
        }
    }
}
