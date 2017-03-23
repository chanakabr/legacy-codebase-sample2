using ApiObjects;
using ApiObjects.Response;
using ApiObjects.TimeShiftedTv;
using DAL;
using KLogMonitor;
using Core.Recordings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using Tvinic.GoogleAPI;
using System.Net;
using System.ServiceModel;
using Core.Pricing;
using ApiObjects.CDNAdapter;
using Core;
using Core.Users;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Core.Catalog;
using ApiObjects.Catalog;
using ApiObjects.ConditionalAccess;
using ApiObjects.Pricing;
using NPVR;
using CachingProvider.LayeredCache;
using TVinciShared;
using KlogMonitorHelper;
using ApiObjects.Billing;

namespace Core.ConditionalAccess
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly KLogger offlinePpvLogger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "OfflinePpvLogger");
        private static readonly KLogger offlineSubscriptionLogger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "OfflineSubscriptionLogger");
        private static object lck = new object();

        public const string SERIES_ID = "seriesId";
        public const string SEASON_NUMBER = "seasonNumber";
        private const string EPISODE_NUMBER = "episodeNumber";
        private const string SERIES_ALIAS = "series_id";
        private const string SEASON_ALIAS = "season_number";
        private const string EPISODE_ALIAS = "episode_number";

        private const string MEDIA_FILES_CACHE_KEY_FORMAT = "MediaFiles_{0}";
        internal const double DEFAULT_MIN_PRICE_FOR_PREVIEW_MODULE = 0.2;
        public const int DEFAULT_MPP_RENEW_FAIL_COUNT = 10; // to be group specific override this value in the 
        // table groups_parameters, column FAIL_COUNT under ConditionalAccess DB.

        internal const string EPG_DATETIME_FORMAT = "dd/MM/yyyy HH:mm:ss";

        private static readonly string BASIC_LINK_TICK_TIME = "!--tick_time--";
        private static readonly string BASIC_LINK_COUNTRY_CODE = "!--COUNTRY_CD--";
        private static readonly string BASIC_LINK_HASH = "!--hash--";
        private static readonly string BASIC_LINK_GROUP = "!--group--";
        private static readonly string BASIC_LINK_CONFIG_DATA = "!--config_data--";

        static public void GetBaseConditionalAccessImpl(ref BaseConditionalAccess t, Int32 nGroupID)
        {
            GetBaseConditionalAccessImpl(ref t, nGroupID, "CA_CONNECTION_STRING");
        }

        static public void GetBaseConditionalAccessImpl(ref BaseConditionalAccess oConditionalAccess, Int32 nGroupID, string sConnKey)
        {
            int nImplID = TvinciCache.ModulesImplementation.GetModuleID(eWSModules.CONDITIONALACCESS, nGroupID, 1, sConnKey);

            switch (nImplID)
            {
                case (1):
                    {
                        oConditionalAccess = new TvinciConditionalAccess(nGroupID, sConnKey);
                        break;
                    }
                case (4):
                    {
                        oConditionalAccess = new FilmoConditionalAccess(nGroupID, sConnKey);
                        break;
                    }

                case (6):
                    {
                        oConditionalAccess = new ElisaConditionalAccess(nGroupID, sConnKey);
                        break;
                    }
                case (7):
                    {
                        oConditionalAccess = new EutelsatConditionalAccess(nGroupID, sConnKey);
                        break;
                    }
                case (9):
                    {
                        oConditionalAccess = new CinepolisConditionalAccess(nGroupID, sConnKey);
                        break;
                    }
                case (10):
                    {
                        oConditionalAccess = new VodafoneConditionalAccess(nGroupID);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public static void GetWSCredentials(int nGroupID, eWSModules eWSModule, ref string sUN, ref string sPass)
        {
            Credentials uc = TvinciCache.WSCredentials.GetWSCredentials(eWSModules.CONDITIONALACCESS, nGroupID, eWSModule);
            sUN = uc.m_sUsername;
            sPass = uc.m_sPassword;
        }

        static public BaseCampaignActionImpl GetCampaignActionByType(CampaignResult result)
        {
            BaseCampaignActionImpl retVal = null;
            switch (result)
            {
                case CampaignResult.Voucher:
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

        static public BaseCampaignActionImpl GetCampaignActionByTriggerType(CampaignTrigger trigger)
        {
            BaseCampaignActionImpl retVal = null;
            switch (trigger)
            {
                case CampaignTrigger.Purchase:
                    {
                        retVal = new VoucherCampaignImpl();
                        break;
                    }
                case CampaignTrigger.SocialInvite:
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

        internal static Price GetPriceAfterDiscount(Price price, DiscountModule disc, Int32 nUseTime)
        {
            Price discRetPrice = CopyPrice(price);

            if (disc.m_dEndDate < DateTime.UtcNow || disc.m_dStartDate > DateTime.UtcNow)
                return price;

            WhenAlgo whenAlgo = disc.m_oWhenAlgo;
            if (whenAlgo.m_eAlgoType == WhenAlgoType.N_FIRST_TIMES && whenAlgo.m_nNTimes != 0 && nUseTime >= whenAlgo.m_nNTimes)
                return price;

            if (whenAlgo.m_eAlgoType == WhenAlgoType.EVERY_N_TIMES && whenAlgo.m_nNTimes != 0 &&
                (double)(((double)nUseTime) / ((double)(whenAlgo.m_nNTimes))) - (Int32)((double)(((double)nUseTime) / ((double)(whenAlgo.m_nNTimes)))) != 0)
                return price;

            double dPer = disc.m_dPercent;
            Price discPrice = CopyPrice(disc.m_oPrise);

            if (disc.m_eTheRelationType == RelationTypes.And ||
                disc.m_eTheRelationType == RelationTypes.Or)
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

        internal static string GetCustomData(Int32 nCustomDataID)
        {
            string sRet = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                selectQuery += "select CUSTOMDATA from customdata_indexer where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nCustomDataID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        sRet = selectQuery.Table("query").DefaultView[0].Row["CUSTOMDATA"].ToString();
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return sRet;
        }

        internal static string GetSubscriptiopnPurchaseCoupon(Int32 nPurchaseID)
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

        internal static Int32 AddCustomData(string sCustomData)
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
            bool bRet = false;
            bRet = Api.Module.ValidateBaseLink(nGroupID, nMediaFileID, sBaseLink);
            return bRet;
        }

        internal static MeidaMaper[] GetMediaMapper(Int32 nGroupID, Int32[] nMediaFilesIDs)
        {
            if (nMediaFilesIDs == null)
                return null;

            string nMediaFilesIDsToCache = ConvertArrayIntToStr(nMediaFilesIDs);
            MeidaMaper[] mapper = Api.Module.MapMediaFiles(nGroupID, nMediaFilesIDs);

            return mapper;
        }

        internal static int GetMediaFileTypeID(int nGroupID, int nMediaFileID)
        {
            return Api.Module.GetMediaFileTypeID(nGroupID, nMediaFileID);
        }

        internal static Dictionary<int, bool> PPVBulkDoCreditNeedToDownloadedUsingCollections(int nGroupID, int nMediaFileID,
            List<int> lstAllUsersInDomain, List<int> lstCollectionCodes)
        {
            Dictionary<int, bool> res = new Dictionary<int, bool>();
            Dictionary<int, DateTime> collToCreateDateMapping = new Dictionary<int, DateTime>();
            DateTime dbTimeNow = ODBCWrapper.Utils.FICTIVE_DATE;
            if (lstCollectionCodes != null && lstCollectionCodes.Count > 0)
            {
                Collection[] colls = GetCollectionsDataWithCaching(lstCollectionCodes, nGroupID);
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

        /*
         * 1. Pass string or int or long as T
         * 2. Caching of pricing modules in CAS side is deprecated. Caching is now done on Pricing side.
         */
        internal static Collection[] GetCollectionsDataWithCaching<T>(List<T> lstCollsCodes, int nGroupID) where T : IComparable, IComparable<T>, IEquatable<T>, IConvertible
        {
            string[] colls = lstCollsCodes.Select((item) => item.ToString()).Distinct().ToArray();
            return Pricing.Module.GetCollectionsData(nGroupID, colls, string.Empty, string.Empty, string.Empty);
        }

        /*
         * 1. Pass string or int or long as T
         * 2. Caching of pricing modules in CAS side is deprecated. Caching is now done on Pricing side.
         */
        internal static Subscription[] GetSubscriptionsDataWithCaching<T>(List<T> lstSubsCodes, int nGroupID) where T : IComparable, IComparable<T>, IEquatable<T>, IConvertible
        {
            string[] subs = lstSubsCodes.Select((item) => item.ToString()).Distinct().ToArray();

            var res = Pricing.Module.GetSubscriptionsData(nGroupID, subs, string.Empty, string.Empty, string.Empty);
            if (res != null)
                return res.Subscriptions;
            return null;
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
            int nMediaFileID, int nGroupID, List<int> allUsersInDomain, List<int> relatedMediaFiles, 
            ref Dictionary<string, bool> subsRes, ref Dictionary<string, bool> collsRes)
        {
            Subscription[] subs = null;
            Collection[] colls = null;

            subsRes = InitializeCreditDownloadedDict(lstSubCodes);
            collsRes = InitializeCreditDownloadedDict(lstColCodes);

            if (lstSubCodes != null && lstSubCodes.Count > 0)
            {
                subs = GetSubscriptionsDataWithCaching(lstSubCodes, nGroupID);
            }
            if (lstColCodes != null && lstColCodes.Count > 0)
            {
                colls = GetCollectionsDataWithCaching(lstColCodes, nGroupID);
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

        internal static bool Bundle_DoesCreditNeedToDownloaded(string productCode, string userId, List<int> relatedMediaFiles, int groupID, eBundleType bundleType, int numOfUses)
        {
            bool bIsSub = true;
            bool isCreditDownloaded = false;

            PPVModule theBundle = null;
            UsageModule u = null;

            string sTableName = string.Empty;

            switch (bundleType)
            {
                case eBundleType.SUBSCRIPTION:
                    {
                        Subscription theSub = null;
                        theSub = Core.Pricing.Module.GetSubscriptionData(groupID, productCode, String.Empty, String.Empty, String.Empty, false);
                        u = theSub.m_oSubscriptionUsageModule;
                        theBundle = theSub;
                        bIsSub = true;

                        break;
                    }
                case eBundleType.COLLECTION:
                    {
                        Collection theCol = null;
                        theCol = Core.Pricing.Module.GetCollectionData(groupID, productCode, String.Empty, String.Empty, String.Empty, false);
                        u = theCol.m_oCollectionUsageModule;
                        theBundle = theCol;
                        bIsSub = false;

                        break;
                    }
            }

            Int32 nViewLifeCycle = u.m_tsViewLifeCycle;
            DateTime dtCreateDateOfLatestBundleUse = ODBCWrapper.Utils.FICTIVE_DATE;
            DateTime dtNow = ODBCWrapper.Utils.FICTIVE_DATE;

            if (numOfUses == 0)
            {
                isCreditDownloaded = true;
            }
            else if (u.m_nMaxNumberOfViews > 0)
            {
                int domainId = 0;
                List<int> allUsersInDomain = Utils.GetAllUsersInDomainBySiteGUIDIncludeDeleted(userId, groupID, ref domainId);
                if (ConditionalAccessDAL.Get_LatestCreateDateOfBundleUses(productCode, groupID, allUsersInDomain, relatedMediaFiles, bIsSub,
                    ref dtCreateDateOfLatestBundleUse, ref dtNow)
                    && !dtCreateDateOfLatestBundleUse.Equals(ODBCWrapper.Utils.FICTIVE_DATE)
                    && !dtNow.Equals(ODBCWrapper.Utils.FICTIVE_DATE)
                    && ((dtNow - dtCreateDateOfLatestBundleUse).TotalMinutes < nViewLifeCycle))
                {
                    isCreditDownloaded = false;
                }
                else
                {
                    isCreditDownloaded = true;
                }
            }

            return isCreditDownloaded;
        }

        internal static void FillCatalogSignature(BaseRequest request)
        {
            request.m_sSignString = Guid.NewGuid().ToString();
            request.m_sSignature = TVinciShared.WS_Utils.GetCatalogSignature(request.m_sSignString, GetWSURL("CatalogSignatureKey"));
        }

        private static BundlesContainingMediaRequest InitializeCatalogRequest(int nGroupID, int nMediaID,
            List<int> lstDistinctSubs, List<int> lstDistinctColls)
        {
            BundlesContainingMediaRequest request = new BundlesContainingMediaRequest();
            request.m_nGroupID = nGroupID;
            request.m_nMediaID = nMediaID;
            request.m_oFilter = new Filter();
            FillCatalogSignature(request);
            int sizeOfArr = lstDistinctSubs.Count + lstDistinctColls.Count, i = 0;
            request.m_oBundles = new BundleKeyValue[sizeOfArr];
            for (i = 0; i < lstDistinctSubs.Count; i++)
            {
                request.m_oBundles[i] = new BundleKeyValue() { m_nBundleCode = lstDistinctSubs[i], m_eBundleType = eBundleType.SUBSCRIPTION };
            }
            for (int j = 0; j < lstDistinctColls.Count; j++)
            {
                request.m_oBundles[j + i] = new BundleKeyValue() { m_nBundleCode = lstDistinctColls[j], m_eBundleType = eBundleType.COLLECTION };
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


        private static void GetUserValidBundlesFromListOptimized(string sSiteGuid, int nMediaID, int nMediaFileID, MediaFileStatus eMediaFileStatus, int nGroupID,
            int[] nFileTypes, List<int> lstUserIDs, List<int> relatedMediaFiles,
            ref Subscription[] subsRes, ref Collection[] collsRes,
            ref  Dictionary<string, UserBundlePurchase> subsPurchase, ref Dictionary<string, UserBundlePurchase> collPurchase, int domainID)
        {
            DataSet dataSet = ConditionalAccessDAL.Get_AllBundlesInfoByUserIDs(lstUserIDs, nFileTypes != null && nFileTypes.Length > 0 ? nFileTypes.ToList<int>() : new List<int>(0), nGroupID, domainID);
            if (IsBundlesDataSetValid(dataSet))
            {
                // the subscriptions and collections we add to those list will be sent to the Catalog in order to determine whether the media
                // given as input belongs to it.
                List<int> subsToSendToCatalog = new List<int>();
                List<int> collsToSendToCatalog = new List<int>();

                List<string> subsToBundleCreditDownloadedQuery = new List<string>();
                List<string> colsToBundleCreditDownloadedQuery = new List<string>();

                // iterate over subscriptions
                DataTable subs = dataSet.Tables[0];
                int waiver = 0;
                DateTime purchaseDate = DateTime.MinValue;
                DateTime endDate = DateTime.MinValue;

                if (subs != null && subs.Rows != null && subs.Rows.Count > 0)
                {
                    for (int i = 0; i < subs.Rows.Count; i++)
                    {
                        int numOfUses = 0;
                        int maxNumOfUses = 0;
                        string bundleCode = string.Empty;
                        waiver = 0;
                        purchaseDate = DateTime.MinValue;
                        endDate = DateTime.MinValue;
                        int gracePeriodMinutes = 0;

                        GetSubscriptionBundlePurchaseData(subs.Rows[i], "SUBSCRIPTION_CODE", ref numOfUses, ref maxNumOfUses, ref bundleCode, ref waiver, ref purchaseDate, ref endDate, ref gracePeriodMinutes);

                        // decide which is the correct end period
                        if (endDate < DateTime.UtcNow)
                            endDate = endDate.AddMinutes(gracePeriodMinutes);

                        // add to bulk query of Bundle_DoesCreditNeedToDownloaded to DB
                        //afterwards, the subs who pass the Bundle_DoesCreditNeedToDownloaded to DB test add to Catalog request.
                        if (eMediaFileStatus == MediaFileStatus.ValidOnlyIfPurchase || !IsUserCanStillUseSub(numOfUses, maxNumOfUses))
                        {
                            subsToBundleCreditDownloadedQuery.Add(bundleCode);
                        }
                        else
                        {
                            // add to Catalog's BundlesContainingMediaRequest
                            int subCode = 0;
                            if (Int32.TryParse(bundleCode, out subCode) && subCode > 0)
                            {
                                subsToSendToCatalog.Add(subCode);
                                if (!subsPurchase.ContainsKey(bundleCode))
                                {
                                    subsPurchase.Add(bundleCode, new UserBundlePurchase()
                                    {
                                        sBundleCode = bundleCode,
                                        nWaiver = waiver,
                                        dtPurchaseDate = purchaseDate,
                                        dtEndDate = endDate
                                    });
                                }
                            }
                            else
                            {
                                // log
                            }
                        }
                    }
                }

                //iterate over collections
                DataTable colls = dataSet.Tables[1];
                if (colls != null && colls.Rows != null && colls.Rows.Count > 0)
                {
                    for (int i = 0; i < colls.Rows.Count; i++)
                    {
                        int numOfUses = 0;
                        int maxNumOfUses = 0;
                        string bundleCode = string.Empty;
                        waiver = 0;
                        purchaseDate = DateTime.MinValue;
                        endDate = DateTime.MinValue;

                        GetCollectionBundlePurchaseData(colls.Rows[i], "COLLECTION_CODE", ref numOfUses, ref maxNumOfUses, ref bundleCode, ref waiver, ref purchaseDate, ref endDate);
                        // add to bulk query of Bundle_DoesCreditNeedToDownload to DB
                        //afterwards, the colls which pass the Bundle_DoesCreditNeedToDownloaded to DB test add to Catalog request.
                        // finally, the colls which pass the catalog need to be validated against PPV_DoesCreditNeedToDownloadedUsingCollection
                        if (eMediaFileStatus == MediaFileStatus.ValidOnlyIfPurchase || !IsUserCanStillUseCol(numOfUses, maxNumOfUses))
                        {
                            colsToBundleCreditDownloadedQuery.Add(bundleCode);
                        }
                        else
                        {
                            // add to Catalog's BundlesContainingMediaRequest
                            int collCode = 0;
                            if (Int32.TryParse(bundleCode, out collCode) && collCode > 0)
                            {
                                collsToSendToCatalog.Add(collCode);
                                if (!collPurchase.ContainsKey(bundleCode))
                                {
                                    collPurchase.Add(bundleCode, new UserBundlePurchase()
                                    {
                                        sBundleCode = bundleCode,
                                        nWaiver = waiver,
                                        dtPurchaseDate = purchaseDate,
                                        dtEndDate = endDate
                                    });
                                }
                            }
                            else
                            {
                                //log
                            }
                        }
                    }
                }

                HandleBundleCreditNeedToDownloadedQuery(subsToBundleCreditDownloadedQuery, colsToBundleCreditDownloadedQuery,
                    nMediaFileID, nGroupID, lstUserIDs, relatedMediaFiles, ref subsToSendToCatalog,
                    ref collsToSendToCatalog);
                // the subs / colls already purchased (no need to download creadit ) - can return it as OK 
                if (eMediaFileStatus == MediaFileStatus.ValidOnlyIfPurchase)
                {
                    if (subsToSendToCatalog != null && subsToSendToCatalog.Count > 0)
                    {
                        // check if credit need to be downloaded for specific mediafile 
                        subsRes = GetSubscriptionsDataWithCaching(subsToSendToCatalog, nGroupID);
                    }
                    if (collsToSendToCatalog != null && collsToSendToCatalog.Count > 0)
                    {
                        collsRes = GetCollectionsDataWithCaching(collsToSendToCatalog, nGroupID);
                    }
                }
                else // only if in the gap between end date to final end date - continue the check
                {
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
                        subsRes = GetSubscriptionsDataWithCaching(validatedSubs, nGroupID);
                    }

                    // now validate bulk collections - PPV_CreditNeedToDownloadedUsingCollection

                    if (validatedColls != null && validatedColls.Count > 0)
                    {
                        Dictionary<int, bool> collsAfterPPVCreditValidation = PPVBulkDoCreditNeedToDownloadedUsingCollections(nGroupID,
                            nMediaFileID, lstUserIDs, validatedColls);
                        List<int> finalCollCodes = GetFinalCollectionCodes(collsAfterPPVCreditValidation);
                        if (finalCollCodes != null && finalCollCodes.Count > 0)
                        {
                            collsRes = GetCollectionsDataWithCaching(finalCollCodes, nGroupID);
                        }
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

                log.Error("Error - " + sb.ToString());
                #endregion

                throw new Exception("Error occurred in GetUserValidBundlesFromListOptimized. Refer to CAS.Utils log file");

            }
        }

        /// <summary>
        /// Partially defines a user's purchase of a bundle, so data is easily transferred between methods
        /// </summary>
        [Serializable]
        public class UserBundlePurchase
        {
            public string sBundleCode;
            public int nWaiver;
            public DateTime dtPurchaseDate;
            public DateTime dtEndDate;
            public int nNumOfUses;
            public int nMaxNumOfUses;

            public UserBundlePurchase() { }
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
            ref List<int> subsToSendToCatalog, ref List<int> collsToSendToCatalog)
        {
            if (subsToBundleCreditDownloadedQuery.Count > 0 || colsToBundleCreditDownloadedQuery.Count > 0)
            {
                Dictionary<string, bool> subsRes = null;
                Dictionary<string, bool> colsRes = null;
                DoBundlesCreditNeedToBeDownloaded(subsToBundleCreditDownloadedQuery, colsToBundleCreditDownloadedQuery, nMediaFileID,
                    nGroupID, lstUserIDs, relatedMediaFileIDs, ref subsRes, ref colsRes);
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
            BundlesContainingMediaRequest request = InitializeCatalogRequest(nGroupID, nMediaID, distinctSubs, distinctColls);

            subsRes = new List<int>();
            collsRes = new List<int>();

            BundlesContainingMediaResponse response = request.GetResponse(request) as BundlesContainingMediaResponse;
            if (response != null && response.m_oBundles != null && response.m_oBundles.Count > 0)
            {
                for (int i = 0; i < response.m_oBundles.Count; i++)
                {
                    BundleTriple bt = response.m_oBundles[i];
                    if (bt.m_bIsContained)
                    {
                        switch (bt.m_eBundleType)
                        {
                            case eBundleType.SUBSCRIPTION:
                                subsRes.Add(bt.m_nBundleCode);
                                break;
                            case eBundleType.COLLECTION:
                                collsRes.Add(bt.m_nBundleCode);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private static void GetSubscriptionBundlePurchaseData(DataRow dataRow, string codeColumnName, ref int numOfUses, ref int maxNumOfUses,
            ref string bundleCode, ref int waiver, ref DateTime purchaseDate, ref DateTime endDate, ref int gracePeriodMin)
        {
            numOfUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow["NUM_OF_USES"]);
            maxNumOfUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow["MAX_NUM_OF_USES"]);
            bundleCode = ODBCWrapper.Utils.GetSafeStr(dataRow[codeColumnName]);
            waiver = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "WAIVER");
            purchaseDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "CREATE_DATE");
            endDate = ODBCWrapper.Utils.ExtractDateTime(dataRow, "END_DATE");
            gracePeriodMin = ODBCWrapper.Utils.GetIntSafeVal(dataRow["GRACE_PERIOD_MINUTES"]);
        }

        private static void GetCollectionBundlePurchaseData(DataRow dataRow, string codeColumnName, ref int numOfUses, ref int maxNumOfUses,
            ref string bundleCode, ref int waiver, ref DateTime purchaseDate, ref DateTime endDate)
        {
            numOfUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow["NUM_OF_USES"]);
            maxNumOfUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow["MAX_NUM_OF_USES"]);
            bundleCode = ODBCWrapper.Utils.GetSafeStr(dataRow[codeColumnName]);
            waiver = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "WAIVER");
            purchaseDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "CREATE_DATE");
            endDate = ODBCWrapper.Utils.ExtractDateTime(dataRow, "END_DATE");
        }

        private static bool IsBundlesDataSetValid(DataSet ds)
        {
            return ds != null && ds.Tables != null && ds.Tables.Count == 2;
        }

        internal static Price CopyPrice(Price toCopy)
        {
            Price ret = new Price();
            ret.m_dPrice = toCopy.m_dPrice;
            ret.m_oCurrency = toCopy.m_oCurrency;
            return ret;
        }

        internal static Price CalculateCouponDiscount(ref Price pModule, CouponsGroup oCouponsGroup, string sCouponCode, int nGroupID)
        {
            Price price = CopyPrice(pModule);
            if (!string.IsNullOrEmpty(sCouponCode) && sCouponCode.Length > 0)
            {
                CouponDataResponse theCouponData = null;

                theCouponData = Core.Pricing.Module.GetCouponStatus(nGroupID, sCouponCode);

                if (oCouponsGroup != null &&
                    theCouponData != null &&
                    theCouponData.Status != null &&
                    theCouponData.Status.Code == (int)eResponseStatus.OK &&
                    theCouponData.Coupon != null &&
                    theCouponData.Coupon.m_CouponStatus == CouponsStatus.Valid &&
                    theCouponData.Coupon.m_oCouponGroup.m_sGroupCode == oCouponsGroup.m_sGroupCode)
                {
                    // if it is a valid gift card, set price to be 0
                    if (theCouponData.Coupon.m_oCouponGroup.couponGroupType == CouponGroupType.GiftCard)
                    {
                        price.m_dPrice = 0.0;
                    }
                    else
                    {
                        //Coupon discount should take place
                        DiscountModule dCouponDiscount = oCouponsGroup.m_oDiscountCode;
                        price = GetPriceAfterDiscount(price, dCouponDiscount, 0);
                    }
                }
            }
            return price;
        }

        private static bool IsVoucherValid(int nLifeCycle, long nOwnerGuid, long campaignID)
        {
            bool retVal = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
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
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return retVal;
        }


        internal static Price CalculateMediaFileFinalPriceNoSubs(Int32 nMediaFileID, int mediaID, Price pModule,
            DiscountModule discModule, CouponsGroup oCouponsGroup, string sSiteGUID,
            string sCouponCode, Int32 nGroupID, string subCode, out DateTime? dtDiscountEnd)
        {
            dtDiscountEnd = null;
            Price p = CopyPrice(pModule);
            if (discModule != null)
            {
                int nPPVPurchaseCount = 0;
                if (discModule.m_oWhenAlgo.m_nNTimes > 0)
                {
                    if (discModule.m_dPercent == 100 && !string.IsNullOrEmpty(subCode))
                    {
                        nPPVPurchaseCount = ConditionalAccessDAL.Get_SubscriptionUseCount(sSiteGUID, subCode, nGroupID);
                    }
                    else
                    {
                        nPPVPurchaseCount = ConditionalAccessDAL.Get_PPVPurchaseCount(nGroupID, sSiteGUID, subCode, nMediaFileID);
                    }
                }
                
                p = GetPriceAfterDiscount(p, discModule, nPPVPurchaseCount);

                dtDiscountEnd = discModule.m_dEndDate;
            }

            if (sCouponCode.Length > 0)
            {
                CouponDataResponse theCouponData = Core.Pricing.Module.GetCouponStatus(nGroupID, sCouponCode);

                if (oCouponsGroup == null ||
                    theCouponData == null ||
                    theCouponData.Status == null ||
                    theCouponData.Status.Code != (int)eResponseStatus.OK ||
                    theCouponData.Coupon == null)
                {

                }
                else if (theCouponData.Coupon.m_CouponType == CouponType.Voucher &&
                        theCouponData.Coupon.m_campID > 0 &&
                        theCouponData.Coupon.m_ownerMedia == mediaID)
                {
                    bool isCampaignValid = false;
                    Campaign camp = Core.Pricing.Module.GetCampaignData(nGroupID, theCouponData.Coupon.m_campID);

                    if (camp != null && camp.m_ID == theCouponData.Coupon.m_campID)
                    {
                        int nViewLS = camp.m_usageModule.m_tsViewLifeCycle;
                        long ownerGuid = theCouponData.Coupon.m_ownerGUID;
                        isCampaignValid = IsVoucherValid(nViewLS, ownerGuid, theCouponData.Coupon.m_campID);
                    }

                    if (isCampaignValid)
                    {
                        DiscountModule voucherDiscount = theCouponData.Coupon.m_oCouponGroup.m_oDiscountCode;
                        p = GetPriceAfterDiscount(p, voucherDiscount, 1);
                    }
                }
                // If it is a gift card - it should be free
                else if (theCouponData.Coupon.m_CouponStatus == CouponsStatus.Valid &&
                    theCouponData.Coupon.m_oCouponGroup.couponGroupType == CouponGroupType.GiftCard)
                {
                    p.m_dPrice = 0;
                }
                else if (theCouponData.Coupon.m_CouponStatus == CouponsStatus.Valid &&
                        theCouponData.Coupon.m_oCouponGroup.m_sGroupCode == oCouponsGroup.m_sGroupCode)
                {
                    //Coupon discount should take place
                    DiscountModule dCouponDiscount = oCouponsGroup.m_oDiscountCode;
                    p = GetPriceAfterDiscount(p, dCouponDiscount, 0);
                }
            } // end if coupon code is not empty
            return p;
        }

        private static Price GetMediaFileFinalPriceNoSubs(Int32 nMediaFileID, int mediaID, PPVModule ppvModule,
            string sSiteGUID, string sCouponCode, Int32 nGroupID, string subCode, out DateTime? dtDiscountEnd)
        {
            Price pModule = TVinciShared.ObjectCopier.Clone<Price>((Price)(ppvModule.m_oPriceCode.m_oPrise));
            DiscountModule discModule = TVinciShared.ObjectCopier.Clone<DiscountModule>((DiscountModule)(ppvModule.m_oDiscountModule));
            CouponsGroup couponGroups = TVinciShared.ObjectCopier.Clone<CouponsGroup>((CouponsGroup)(ppvModule.m_oCouponsGroup));

            return CalculateMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, pModule, discModule, couponGroups, sSiteGUID,
                sCouponCode, nGroupID, subCode, out dtDiscountEnd);
        }

        internal static Price GetSubscriptionFinalPrice(int groupId, string subCode, string userId, string couponCode, ref PriceReason theReason, ref Subscription theSub,
                                                        string countryCode, string languageCode, string udid)
        {
            return GetSubscriptionFinalPrice(groupId, subCode, userId, couponCode, ref theReason, ref theSub, countryCode, languageCode, udid, string.Empty);
        }

        internal static Price GetSubscriptionFinalPrice(int groupId, string subCode, string userId, string couponCode, ref PriceReason theReason, ref Subscription theSub,
                                                        string countryCode, string languageCode, string udid, string ip, string currencyCode = null)
        {
            Price price = null;
            Subscription subscription = null;
            //create web service pricing insatance
            try
            {
                subscription = Core.Pricing.Module.GetSubscriptionData(groupId, subCode, countryCode, languageCode, udid, false);
                if (subscription == null)
                {
                    theReason = PriceReason.UnKnown;
                    return null;
                }

                bool isGeoCommerceBlock = false;
                if (!string.IsNullOrEmpty(ip))
                {
                    isGeoCommerceBlock = IsGeoBlock(groupId, subscription.n_GeoCommerceID, ip);
                }

                if (!isGeoCommerceBlock)
                {
                    theSub = TVinciShared.ObjectCopier.Clone<Subscription>((Subscription)(subscription));
                    DiscountModule externalDisount = theSub.m_oExtDisountModule != null ? TVinciShared.ObjectCopier.Clone<DiscountModule>((DiscountModule)(theSub.m_oExtDisountModule)) : null;                  
                    if (subscription.m_oSubscriptionPriceCode != null)
                    {
                        bool isValidCurrencyCode = false;
                        // Validate currencyCode if it was passed in the request
                        if (!string.IsNullOrEmpty(currencyCode))
                        {
                            if (!Utils.IsValidCurrencyCode(groupId, currencyCode))
                            {
                                theReason = PriceReason.InvalidCurrency;
                                return new Price();
                            }
                            else
                            {
                                isValidCurrencyCode = true;
                            }
                        }
                        
                        // Get subscription price code according to country and currency (if exists on the request)
                        if (!string.IsNullOrEmpty(ip) && (isValidCurrencyCode || Utils.GetGroupDefaultCurrency(groupId, ref currencyCode)))
                        {
                            countryCode = Utils.GetIP2CountryCode(groupId, ip);
                            PriceCode priceCodeWithCurrency = Core.Pricing.Module.GetPriceCodeDataByCountyAndCurrency(groupId, theSub.m_oSubscriptionPriceCode.m_nObjectID, countryCode, currencyCode);
                            if (priceCodeWithCurrency != null)
                            {
                                theSub.m_oSubscriptionPriceCode = TVinciShared.ObjectCopier.Clone<PriceCode>(priceCodeWithCurrency);
                            }
                            else
                            {
                                theReason = PriceReason.CurrencyNotDefinedOnPriceCode;
                                return new Price();
                            }

                            if (externalDisount != null)
                            {
                                DiscountModule externalDisountWithCurrency = Core.Pricing.Module.GetDiscountCodeDataByCountryAndCurrency(groupId, externalDisount.m_nObjectID, countryCode, currencyCode);
                                externalDisount = externalDisountWithCurrency != null ? TVinciShared.ObjectCopier.Clone<DiscountModule>(externalDisountWithCurrency) : externalDisount;
                            }
                        }

                        price = TVinciShared.ObjectCopier.Clone<Price>((Price)(theSub.m_oSubscriptionPriceCode.m_oPrise));
                    }

                    theReason = PriceReason.ForPurchase;
                    int domainID = 0;
                    List<int> lUsersIds = ConditionalAccess.Utils.GetAllUsersDomainBySiteGUID(userId, groupId, ref domainID);
                    DataTable dt = DAL.ConditionalAccessDAL.Get_SubscriptionBySubscriptionCodeAndUserIDs(lUsersIds, subCode, domainID);
                    if (dt != null)
                    {
                        int nCount = dt.Rows.Count;
                        if (nCount > 0)
                        {
                            price.m_dPrice = 0.0;
                            theReason = PriceReason.SubscriptionPurchased;
                        }
                    }

                    if (theReason != PriceReason.SubscriptionPurchased)
                    {
                        if (subscription.m_oPreviewModule != null && IsEntitledToPreviewModule(userId, groupId, subCode, subscription, ref price, ref theReason, domainID))
                        {
                            return price;
                        }

                        CouponsGroup couponGroups = TVinciShared.ObjectCopier.Clone<CouponsGroup>((CouponsGroup)(theSub.m_oCouponsGroup));
                        if (externalDisount != null)
                        {
                            price = GetPriceAfterDiscount(price, externalDisount, 1);
                        }

                        price = CalculateCouponDiscount(ref price, couponGroups, couponCode, groupId);
                    }
                }
                else
                {
                    theReason = PriceReason.GeoCommerceBlocked;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetSubscriptionFinalPrice failed, groupId: {0}, subCode: {1}, userId: {2}, couponCode: {3}, countryCode: {4}, languageCode: {5}, udid: {6}, ip: {7}, currency: {8}",
                    groupId, subCode, userId, couponCode, countryCode, languageCode, udid, !string.IsNullOrEmpty(ip) ? ip : string.Empty, !string.IsNullOrEmpty(currencyCode) ? currencyCode : string.Empty), ex);
            }

            return price;
        }

        internal static Price GetCollectionFinalPrice(Int32 nGroupID, string sColCode, string sSiteGUID, string sCouponCode, ref PriceReason theReason, ref Collection theCol,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string connStr)
        {
            Price price = null;
            Collection collection = null;
            collection = Pricing.Module.GetCollectionData(nGroupID, sColCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);
            theCol = TVinciShared.ObjectCopier.Clone<Collection>((Collection)(collection));
            if (collection == null)
            {
                theReason = PriceReason.UnKnown;
                return null;
            }

            if (collection.m_oCollectionPriceCode != null)
                price = TVinciShared.ObjectCopier.Clone<Price>((Price)(collection.m_oCollectionPriceCode.m_oPrise));
            theReason = PriceReason.ForPurchase;

            int domainID = 0;
            List<int> lUsersIds = Utils.GetAllUsersDomainBySiteGUID(sSiteGUID, nGroupID, ref domainID);

            DataTable dt = ConditionalAccessDAL.Get_CollectionByCollectionCodeAndUserIDs(lUsersIds, sColCode, domainID);

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                price.m_dPrice = 0.0;
                theReason = PriceReason.CollectionPurchased;

            }
            if (theReason != PriceReason.CollectionPurchased)
            {
                CouponsGroup couponGroups = TVinciShared.ObjectCopier.Clone<CouponsGroup>((CouponsGroup)(theCol.m_oCouponsGroup));
                if (theCol.m_oExtDisountModule != null)
                {
                    DiscountModule externalDisount = TVinciShared.ObjectCopier.Clone<DiscountModule>((DiscountModule)(theCol.m_oExtDisountModule));
                    price = GetPriceAfterDiscount(price, externalDisount, 1);
                }
                price = CalculateCouponDiscount(ref price, couponGroups, sCouponCode, nGroupID);
            }
            return price;
        }

        internal static Price GetPrePaidFinalPrice(Int32 nGroupID, string sPrePaidCode, string sSiteGUID, ref PriceReason theReason, ref PrePaidModule thePrePaid,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string connStr)
        {
            return GetPrePaidFinalPrice(nGroupID, sPrePaidCode, sSiteGUID, ref theReason, ref thePrePaid,
            sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, connStr, string.Empty);
        }

        internal static Price GetPrePaidFinalPrice(Int32 nGroupID, string sPrePaidCode, string sSiteGUID, ref PriceReason theReason, ref PrePaidModule thePrePaid,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string connStr, string sCouponCode)
        {
            Price p = null;

            if (thePrePaid == null)
            {
                PrePaidModule ppModule = null;

                ppModule = Pricing.Module.GetPrePaidModuleData(nGroupID, int.Parse(sPrePaidCode), sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                thePrePaid = TVinciShared.ObjectCopier.Clone<PrePaidModule>((PrePaidModule)(ppModule));
                if (thePrePaid == null)
                {
                    theReason = PriceReason.UnKnown;
                    return null;
                }
            } // end if thePrePaid==null

            if (thePrePaid.m_PriceCode != null)
            {
                p = TVinciShared.ObjectCopier.Clone<Price>((Price)(thePrePaid.m_PriceCode.m_oPrise));

                if (!string.IsNullOrEmpty(sCouponCode))
                {
                    CouponsGroup couponGroups = TVinciShared.ObjectCopier.Clone<CouponsGroup>((CouponsGroup)(thePrePaid.m_CouponsGroup));
                    p = CalculateCouponDiscount(ref p, couponGroups, sCouponCode, nGroupID);
                }
            }
            theReason = PriceReason.ForPurchase;

            return p;
        }

        internal static string ConvertArrayIntToStr(int[] theArray)
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

        //public static Int32 GetMediaIDFeomFileID(Int32 nMediaFileID, Int32 nGroupID)
        //{
        //    Int32[] nMediaFilesIDs = { nMediaFileID };
        //    MeidaMaper[] mapper = null;
        //    string nMediaFilesIDsForCache = ConvertArrayIntToStr(nMediaFilesIDs);

        //    mapper = GetMediaMapper(nGroupID, nMediaFilesIDs);

        //    if (mapper == null || mapper.Length == 0)
        //        return 0;

        //    if (mapper[0].m_nMediaFileID == nMediaFileID)
        //        return mapper[0].m_nMediaID;

        //    return 0;
        //}

        public static Int32 GetMediaIDFromFileID(Int32 nMediaFileID, Int32 nGroupID)
        {
            Int32[] nMediaFilesIDs = { nMediaFileID };
            MeidaMaper[] mapper = null;
            string nMediaFilesIDsForCache = ConvertArrayIntToStr(nMediaFilesIDs);

            mapper = GetMediaMapper(nGroupID, nMediaFilesIDs);

            if (mapper == null || mapper.Length == 0)
                return 0;

            if (mapper[0].m_nMediaFileID == nMediaFileID)
                return mapper[0].m_nMediaID;

            return 0;
        }

        //Get ProductCode and get it MediaFileID - then continue as it was mediaFileID
        static public Int32 GetMediaIDFromFileID(string sProductCode, Int32 nGroupID, ref int nMediaFileID)
        {

            DataTable dt = ConditionalAccessDAL.Get_MediaFileByProductCode(nGroupID, sProductCode);

            if (dt != null && dt.DefaultView.Count > 0)
            {
                nMediaFileID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["ID"]);
            }

            return GetMediaIDFromFileID(nMediaFileID, nGroupID);
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

        internal static Price GetMediaFileFinalPriceForNonGetItemsPrices(Int32 nMediaFileID, PPVModule ppvModule, string sSiteGUID, string sCouponCode, Int32 nGroupID,
                                                                                       ref PriceReason theReason, ref Subscription relevantSub, ref Collection relevantCol,
                                                                                       ref PrePaidModule relevantPP, string countryCode, string sLANGUAGE_CODE, string sDEVICE_NAME,
                                                                                       bool shouldIgnoreBundlePurchases = false, string ip = null, string currencyCode = null)
        {
            Dictionary<int, int> mediaFileTypesMapping = null;
            List<int> allUsersInDomain = null;
            string sFirstDeviceNameFound = string.Empty;
            int nMediaFileTypeID = 0;
            int domainID = 0;
            //Utils.GetApiAndPricingCredentials(nGroupID, ref sPricingUsername, ref sPricingPassword, ref sAPIUsername, ref sAPIPassword);

            // check if file is avilable             
            Dictionary<int, string> mediaFilesProductCode = new Dictionary<int, string>();
            Dictionary<int, MediaFileStatus> validMediaFiles = Utils.ValidateMediaFiles(new int[1] { nMediaFileID }, ref mediaFilesProductCode, nGroupID);
            if (validMediaFiles[nMediaFileID] == MediaFileStatus.NotForPurchase)
            {
                theReason = PriceReason.NotForPurchase;
                return null;
            }

            bool isValidCurrencyCode = false;
            // Validate currencyCode if it was passed in the request
            if (!string.IsNullOrEmpty(currencyCode))
            {
                if (!Utils.IsValidCurrencyCode(nGroupID, currencyCode))
                {
                    theReason = PriceReason.InvalidCurrency;
                    return new Price();
                }
                else
                {
                    isValidCurrencyCode = true;
                }
            }

            if (nMediaFileID > 0)
            {
                nMediaFileTypeID = GetMediaFileTypeID(nGroupID, nMediaFileID);
            }
            if (!string.IsNullOrEmpty(sSiteGUID))
            {
                allUsersInDomain = GetAllUsersInDomainBySiteGUIDIncludeDeleted(sSiteGUID, nGroupID, ref domainID);

                if (ppvModule != null && ppvModule.m_relatedFileTypes != null && ppvModule.m_relatedFileTypes.Count > 0)
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

            // purchasedBySiteGuid, purchasedAsMediaFileID, EndDate and StartDate are only needed in GetItemsPrices.
            string purchasedBySiteGuid = string.Empty;
            int purchasedAsMediaFileID = 0;
            DateTime? dtStartDate = null;
            DateTime? dtEndDate = null;
            DateTime? dtDiscountEndDate = null;

            if (!string.IsNullOrEmpty(ip) && (isValidCurrencyCode || Utils.GetGroupDefaultCurrency(nGroupID, ref currencyCode)))
            {
                countryCode = GetIP2CountryCode(nGroupID, ip);
                PriceCode priceCodeWithCurrency = Core.Pricing.Module.GetPriceCodeDataByCountyAndCurrency(nGroupID, ppvModule.m_oPriceCode.m_nObjectID, countryCode, currencyCode);
                bool shouldUpdateDiscountModule = false;
                DiscountModule discountModuleWithCurrency = null;
                if (ppvModule.m_oDiscountModule != null)
                {
                    discountModuleWithCurrency = Core.Pricing.Module.GetDiscountCodeDataByCountryAndCurrency(nGroupID, ppvModule.m_oDiscountModule.m_nObjectID, countryCode, currencyCode);
                    shouldUpdateDiscountModule = discountModuleWithCurrency != null;
                }

                if (priceCodeWithCurrency == null || (shouldUpdateDiscountModule && discountModuleWithCurrency == null))
                {
                    theReason = PriceReason.CurrencyNotDefinedOnPriceCode;
                    return new Price();
                }
                else
                {
                    ppvModule.m_oPriceCode = TVinciShared.ObjectCopier.Clone<PriceCode>(priceCodeWithCurrency);
                    if (shouldUpdateDiscountModule)
                    {
                        ppvModule.m_oDiscountModule = TVinciShared.ObjectCopier.Clone<DiscountModule>(discountModuleWithCurrency);
                    }
                }
            }

            // relatedMediaFileIDs is needed only GetLicensedLinks (which calls GetItemsPrices in order to get to GetMediaFileFinalPrice)
            List<int> relatedMediaFileIDs = new List<int>();
            return GetMediaFileFinalPrice(nMediaFileID, validMediaFiles[nMediaFileID], ppvModule, sSiteGUID, sCouponCode, nGroupID, true, ref theReason, ref relevantSub,
                ref relevantCol, ref relevantPP, ref sFirstDeviceNameFound, sCouponCode, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty,
                mediaFileTypesMapping, allUsersInDomain, nMediaFileTypeID, 
                ref bCancellationWindow, ref purchasedBySiteGuid, ref purchasedAsMediaFileID, ref relatedMediaFileIDs, ref dtStartDate, ref dtEndDate, ref dtDiscountEndDate, domainID,
                null, 0, DAL.DomainSuspentionStatus.Suspended, true, shouldIgnoreBundlePurchases);
        }

        internal static void GetApiAndPricingCredentials(int nGroupID, ref string sPricingUsername, ref string sPricingPassword,
            ref string sAPIUsername, ref string sAPIPassword)
        {
            Dictionary<string, string[]> dict = Get_MultipleWSCredentials(nGroupID, new List<string>(2) { "api", "pricing" });
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

        internal static int ExtractMediaIDOutOfMediaMapper(MeidaMaper[] mapper, int nMediaFileID)
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
            List<int> lFiles = new List<int>();
            if ((mediaFilesList != null && mediaFilesList.Count > 0) || !isMultiMediaTypes)
            {
                lFiles = ConditionalAccessDAL.Get_MediaFileByID(mediaFilesList, nMediaFileID, isMultiMediaTypes, nMediaID);
                if (!lFiles.Contains(nMediaFileID))
                {
                    lFiles.Add(nMediaFileID);
                }
                return lFiles;
            }

            return new List<int>(0);
        }

        private static bool IsPurchasedAsPurePPV(string sSubCode, string sPrePaidCode)
        {
            return sSubCode.Length == 0 && sPrePaidCode.Length == 0;
        }

        internal static bool IsAnonymousUser(string siteGuid)
        {
            int userID = 0;
            return (!int.TryParse(siteGuid, out userID) || userID <= 0);
        }

        internal static Price GetMediaFileFinalPrice(Int32 nMediaFileID, MediaFileStatus eMediaFileStatus, PPVModule ppvModule, string sSiteGUID,
            string sCouponCode, Int32 nGroupID, bool bIsValidForPurchase, ref PriceReason theReason, ref Subscription relevantSub,
            ref Collection relevantCol, ref PrePaidModule relevantPP, ref string sFirstDeviceNameFound,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sClientIP, Dictionary<int, int> mediaFileTypesMapping,
            List<int> allUserIDsInDomain, int nMediaFileTypeID, ref bool bCancellationWindow, ref string purchasedBySiteGuid, ref int purchasedAsMediaFileID,
            ref List<int> relatedMediaFileIDs, ref DateTime? p_dtStartDate, ref DateTime? p_dtEndDate, ref DateTime? dtDiscountEndDate, int domainID, DomainEntitlements domainEntitlements = null,
            int mediaID = 0, DAL.DomainSuspentionStatus userSuspendStatus = DAL.DomainSuspentionStatus.Suspended, bool shouldCheckUserStatus = true, bool shouldIgnoreBundlePurchases = false)
        {
            if (ppvModule == null)
            {
                theReason = PriceReason.Free;
                return null;
            }

            bool isUserValidRes = true;
            // get user status and validity if needed
            if (shouldCheckUserStatus)
            {
                int nDomainID = 0;
                isUserValidRes = IsUserValid(sSiteGUID, nGroupID, ref nDomainID, ref userSuspendStatus);
            }

            // check user status and validity
            if (isUserValidRes && userSuspendStatus == DAL.DomainSuspentionStatus.Suspended)
            {
                theReason = PriceReason.UserSuspended;
                return null;
            }

            theReason = PriceReason.UnKnown;
            Price price = null;
            int[] fileTypes = new int[1] { nMediaFileTypeID };

            // get mediaID
            if (mediaID == 0)
            {
                Int32[] nMediaFilesIDs = { nMediaFileID };
                MeidaMaper[] mapper = GetMediaMapper(nGroupID, nMediaFilesIDs);
                if (mapper == null || mapper.Length == 0)
                    return null;

                mediaID = ExtractMediaIDOutOfMediaMapper(mapper, nMediaFileID);
            }

            if (!IsAnonymousUser(sSiteGUID))
            {
                int[] ppvGroupFileTypes = ppvModule.m_relatedFileTypes != null ? ppvModule.m_relatedFileTypes.ToArray() : null;
                List<int> lstFileIDs;
                // get list of mediaFileIDs
                    if (domainEntitlements != null && domainEntitlements.DomainPpvEntitlements.MediaIdGroupFileTypeMapper != null)
                {
                        lstFileIDs = GetRelatedFileIDs(mediaID, ppvGroupFileTypes, domainEntitlements.DomainPpvEntitlements.MediaIdGroupFileTypeMapper);
                }
                else
                {
                    bool isMultiMediaTypes = false;
                    List<int> mediaFilesList = GetMediaTypesOfPPVRelatedFileTypes(nGroupID, ppvGroupFileTypes, mediaFileTypesMapping, ref isMultiMediaTypes);
                    lstFileIDs = GetFileIDs(mediaFilesList, nMediaFileID, isMultiMediaTypes, mediaID);
                }

                relatedMediaFileIDs.AddRange(lstFileIDs);
                relatedMediaFileIDs = relatedMediaFileIDs.Distinct().ToList();
                price = TVinciShared.ObjectCopier.Clone<Price>((Price)(ppvModule.m_oPriceCode.m_oPrise));

                bool bEnd = false;

                int ppvID = 0;
                string sSubCode = string.Empty;
                string sPPCode = string.Empty;
                int nWaiver = 0;
                DateTime dPurchaseDate = DateTime.MinValue;
                bool isEntitled = false;
                if (lstFileIDs.Count > 0)
                {
                        if (domainEntitlements != null && domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary != null)
                    {
                        isEntitled = IsUserEntitled(lstFileIDs, ppvModule.m_sObjectCode, ref ppvID, ref sSubCode, ref sPPCode, ref nWaiver,
                                                        ref dPurchaseDate, ref purchasedBySiteGuid, ref purchasedAsMediaFileID, ref p_dtStartDate, ref p_dtEndDate, domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary);
                    }
                    else
                    {
                        isEntitled = ConditionalAccessDAL.Get_AllUsersPurchases(allUserIDsInDomain, lstFileIDs, nMediaFileID, ppvModule.m_sObjectCode, ref ppvID, ref sSubCode,
                                                                            ref sPPCode, ref nWaiver, ref dPurchaseDate, ref purchasedBySiteGuid, ref purchasedAsMediaFileID, ref p_dtStartDate, ref p_dtEndDate, domainID);
                    }
                }

                // user or domain users have entitlements \ purchases
                if (isEntitled)
                {
                    price.m_dPrice = 0;
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
                    else if (!shouldIgnoreBundlePurchases)
                    {
                        if (sSubCode.Length > 0)
                        {
                            // purchased as part of subscription
                            theReason = PriceReason.SubscriptionPurchased;
                            Subscription[] sub = GetSubscriptionsDataWithCaching(new List<string>(1) { sSubCode }, nGroupID);
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
                                relevantPP = Pricing.Module.GetPrePaidModuleData(nGroupID, int.Parse(sPPCode), sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                            }
                        }
                    }
                    bEnd = true;
                }
                else if (lstFileIDs.Count > 0 && eMediaFileStatus == MediaFileStatus.ValidOnlyIfPurchase) // user didn't purchase and mediaFileREson is ValidOnlyIfPurchase
                {
                    theReason = PriceReason.NotForPurchase;
                }
                else
                {
                    if (IsPPVModuleToBePurchasedAsSubOnly(ppvModule))
                    {
                        theReason = PriceReason.ForPurchaseSubscriptionOnly;
                    }
                }


                if (bEnd || !bIsValidForPurchase)
                {
                    return price;
                }

                //check here if it is part of a purchased subscription or part of purchased collections

                Subscription[] relevantValidSubscriptions = null;
                Collection[] relevantValidCollections = null;

                // dictionary(subscriptionCode, [nWaiver, dPurchaseDate, dEndDate])
                Dictionary<string, UserBundlePurchase> subsPurchase = new Dictionary<string, UserBundlePurchase>();
                Dictionary<string, UserBundlePurchase> collPurchase = new Dictionary<string, UserBundlePurchase>();

                    if (domainEntitlements != null && domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions != null && domainEntitlements.DomainBundleEntitlements.EntitledCollections != null)
                {
                        subsPurchase = domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions;
                        collPurchase = domainEntitlements.DomainBundleEntitlements.EntitledCollections;
                        GetUserValidBundles(mediaID, nMediaFileID, eMediaFileStatus, nGroupID, fileTypes, allUserIDsInDomain, relatedMediaFileIDs, subsPurchase,
                                            collPurchase, domainEntitlements.DomainBundleEntitlements.FileTypeIdToSubscriptionMappings, domainEntitlements.DomainBundleEntitlements.SubscriptionsData,
                                            domainEntitlements.DomainBundleEntitlements.CollectionsData, domainEntitlements.DomainBundleEntitlements.ChannelsToSubscriptionMappings,
                                            domainEntitlements.DomainBundleEntitlements.ChannelsToCollectionsMappings, ref relevantValidSubscriptions, ref relevantValidCollections);
                }
                else
                {
                    GetUserValidBundlesFromListOptimized(sSiteGUID, mediaID, nMediaFileID, eMediaFileStatus, nGroupID, fileTypes, allUserIDsInDomain, 
                                                        relatedMediaFileIDs, ref relevantValidSubscriptions, ref relevantValidCollections, ref subsPurchase, ref collPurchase, domainID);
                }

                if (relevantValidSubscriptions != null && relevantValidSubscriptions.Length > 0)
                {
                    Dictionary<long, List<Subscription>> groupedSubs = (from s in relevantValidSubscriptions
                                                                                        group s by s.m_Priority).OrderByDescending(gr => gr.Key).ToDictionary(gr => gr.Key, gr => gr.ToList());

                    if (groupedSubs != null)
                    {
                        List<Subscription> prioritySubs = groupedSubs.Values.LastOrDefault();
                        for (int i = 0; i < prioritySubs.Count; i++)
                        {
                            Subscription s = prioritySubs[i];
                            DiscountModule d = (DiscountModule)(s.m_oDiscountModule);
                            Price subp = TVinciShared.ObjectCopier.Clone<Price>((Price)(CalculateMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule.m_oPriceCode.m_oPrise,
                                s.m_oDiscountModule, s.m_oCouponsGroup, sSiteGUID, sCouponCode, nGroupID, s.m_sObjectCode, out dtDiscountEndDate)));
                            if (subp != null)
                            {
                                if (IsGeoBlock(nGroupID, s.n_GeoCommerceID, sClientIP))
                                {
                                    price = TVinciShared.ObjectCopier.Clone<Price>((Price)(subp));
                                    relevantSub = TVinciShared.ObjectCopier.Clone<Subscription>((Subscription)(s));
                                    theReason = PriceReason.GeoCommerceBlocked;
                                }
                                else if (IsItemPurchased(price, subp, ppvModule) && !shouldIgnoreBundlePurchases)
                                {
                                    price = TVinciShared.ObjectCopier.Clone<Price>((Price)(subp));
                                    relevantSub = TVinciShared.ObjectCopier.Clone<Subscription>((Subscription)(s));
                                    theReason = PriceReason.SubscriptionPurchased;

                                    bEnd = true;
                                    break;
                                }
                            }
                        }

                        //cancellationWindow by relevantSub
                        if (relevantSub != null && relevantSub.m_MultiSubscriptionUsageModule != null && relevantSub.m_MultiSubscriptionUsageModule.Count() > 0)
                        {
                            if (subsPurchase.ContainsKey(relevantSub.m_SubscriptionCode))
                            {
                                nWaiver = subsPurchase[relevantSub.m_SubscriptionCode].nWaiver;
                                dPurchaseDate = subsPurchase[relevantSub.m_SubscriptionCode].dtPurchaseDate;
                                p_dtStartDate = dPurchaseDate;
                                p_dtEndDate = subsPurchase[relevantSub.m_SubscriptionCode].dtEndDate;
                                bCancellationWindow = IsCancellationWindowPerPurchase(relevantSub.m_MultiSubscriptionUsageModule[0], bCancellationWindow, nWaiver, dPurchaseDate);
                            }
                        }
                    }
                }

                if (bEnd)
                {
                    return price;
                }

                // check here if its part of a purchased collection                    

                    if (relevantValidCollections != null && relevantValidCollections.Length > 0)
                {
                    for (int i = 0; i < relevantValidCollections.Length; i++)
                    {
                        Collection collection = (Collection)relevantValidCollections[i];
                        DiscountModule discount = (DiscountModule)(collection.m_oDiscountModule);
                        Price collectionsPrice = TVinciShared.ObjectCopier.Clone<Price>((Price)(CalculateMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule.m_oPriceCode.m_oPrise, collection.m_oDiscountModule, collection.m_oCouponsGroup, sSiteGUID, sCouponCode, nGroupID, collection.m_sObjectCode, out dtDiscountEndDate)));
                        if (collectionsPrice != null)
                        {
                            if (IsItemPurchased(price, collectionsPrice, ppvModule))
                            {
                                price = TVinciShared.ObjectCopier.Clone<Price>((Price)(collectionsPrice));
                                relevantCol = TVinciShared.ObjectCopier.Clone<Collection>((Collection)(collection));
                                theReason = PriceReason.CollectionPurchased;
                                break;
                            }
                        }
                    }

                    //cancellationWindow by relevantSub
                        if (relevantCol != null && relevantCol.m_oCollectionUsageModule != null)
                    {
                        if (subsPurchase.ContainsKey(relevantCol.m_CollectionCode))
                        {
                            nWaiver = subsPurchase[relevantCol.m_CollectionCode].nWaiver;
                            dPurchaseDate = subsPurchase[relevantCol.m_CollectionCode].dtPurchaseDate;
                            p_dtStartDate = dPurchaseDate;
                            p_dtEndDate = subsPurchase[relevantCol.m_CollectionCode].dtEndDate;

                            bCancellationWindow = IsCancellationWindowPerPurchase(relevantCol.m_oCollectionUsageModule, bCancellationWindow, nWaiver, dPurchaseDate);
                        }
                    }
                }
                else
                {
                    // the media file was not purchased in any way. calculate its price as a single media file and its price reason
                    price = GetMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule, sSiteGUID, sCouponCode, nGroupID, string.Empty, out dtDiscountEndDate);
                    if (IsFreeMediaFile(theReason, price))
                    {
                        theReason = PriceReason.Free;
                    }
                    else if (theReason != PriceReason.ForPurchaseSubscriptionOnly && theReason != PriceReason.NotForPurchase)
                    {
                        theReason = PriceReason.ForPurchase;
                    }
                }
            } // end if site guid is not null or empty            
            else
            {
                price = GetMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule, sSiteGUID, sCouponCode, nGroupID, string.Empty, out dtDiscountEndDate);

                if (IsPPVModuleToBePurchasedAsSubOnly(ppvModule))
                {
                    theReason = PriceReason.ForPurchaseSubscriptionOnly;
                }
                else
                {
                    theReason = PriceReason.ForPurchase;
                }
            }

            return price;
        }

        private static bool IsPPVModuleToBePurchasedAsSubOnly(PPVModule ppvModule)
        {
            return ppvModule != null && ppvModule.m_bSubscriptionOnly;
        }

        private static bool IsCancellationWindowPerPurchase(UsageModule oUsageModule, bool bCancellationWindow, int nWaiver, DateTime dCreateDate)
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
                log.Error("", ex);
                return false;
            }
        }

        private static bool IsFreeMediaFile(PriceReason reason, Price p)
        {
            return p != null && p.m_dPrice == 0 && reason != PriceReason.ForPurchaseSubscriptionOnly;
        }

        private static bool IsItemPurchased(Price initialPrice, Price businessModulePrice, PPVModule ppvModule)
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
            if (nSubGeoCommerceID <= 0)
            {
                return res;
            }

            if (!string.IsNullOrEmpty(sClientIP))
            {
                res = Api.Module.CheckGeoCommerceBlock(nGroupID, nSubGeoCommerceID, sClientIP);
            }

            return res;
        }

        private static void GetUsersAndDomainsCredentials(int nGroupID, ref string sUsersUsername, ref string sUsersPassword,
            ref string sDomainsUsername, ref string sDomainsPassword)
        {
            Dictionary<string, string[]> dict = Utils.Get_MultipleWSCredentials(nGroupID, new List<string>(2) { "users", "domains" });
            string[] usersCreds = dict["users"];
            sUsersUsername = usersCreds[0];
            sUsersPassword = usersCreds[1];
            string[] domainsCreds = dict["domains"];
            sDomainsUsername = domainsCreds[0];
            sDomainsPassword = domainsCreds[1];
        }

        private static Dictionary<string, string[]> Get_MultipleWSCredentials(int groupId, List<string> serviceList)
        {
            Dictionary<string, string[]> result = new Dictionary<string, string[]>();
            List<string> missingWsCredentials = new List<string>();
            foreach (string serviceName in serviceList)
            {
                eWSModules wsModule;
                if (Enum.TryParse(serviceName, true, out wsModule))
                {
                    string userName = string.Empty;
                    string password = string.Empty;
                    GetWSCredentials(groupId, wsModule, ref userName, ref password);
                    if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                    {
                        missingWsCredentials.Add(serviceName);
                    }
                    else
                    {
                        result.Add(serviceName, new string[2] { userName, password });
                    }
                }
            }

            if (missingWsCredentials != null && missingWsCredentials.Count > 0)
            {
                result.Union(ConditionalAccessDAL.Get_MultipleWSCredentials(groupId, missingWsCredentials));                
            }

            return result;
        }

        internal static List<int> GetAllUsersDomainBySiteGUID(string sSiteGUID, Int32 nGroupID, ref int domainID)
        {
            List<int> lDomainsUsers = new List<int>();

            if (string.IsNullOrEmpty(sSiteGUID) || sSiteGUID.Equals("0"))
            {
                return lDomainsUsers;
            }

            UserResponseObject userResponseObj = Core.Users.Module.GetUserData(nGroupID, sSiteGUID, string.Empty);

            if (userResponseObj.m_RespStatus == ResponseStatus.OK && userResponseObj.m_user.m_domianID != 0)
            {
                domainID = userResponseObj.m_user.m_domianID;
                lDomainsUsers = GetDomainsUsers(userResponseObj.m_user.m_domianID, nGroupID, true);
            }
            else
            {
                lDomainsUsers.Add(int.Parse(sSiteGUID));
            }

            //change the user pending to users without (-1)
            lDomainsUsers = lDomainsUsers.ConvertAll(x => Math.Abs(x));

            return lDomainsUsers;
        }

        internal static List<int> GetAllUsersInDomain(int groupID, int domainId)
        {
            List<int> lDomainsUsers = GetDomainsUsers(domainId, groupID);
            //change the user pending to users without (-1)
            lDomainsUsers = lDomainsUsers.ConvertAll(x => Math.Abs(x));

            return lDomainsUsers;
        }

        internal static List<int> GetAllUsersInDomainBySiteGUIDIncludeDeleted(string sSiteGUID, Int32 nGroupID, ref int domainID)
        {
            List<int> lDomainsUsers = new List<int>();

            if (string.IsNullOrEmpty(sSiteGUID) || sSiteGUID.Equals("0"))
            {
                return lDomainsUsers;
            }

            UserResponseObject userResponseObj = Core.Users.Module.GetUserData(nGroupID, sSiteGUID, string.Empty);

            if (userResponseObj.m_RespStatus == ResponseStatus.OK && userResponseObj.m_user.m_domianID != 0)
            {
                domainID = userResponseObj.m_user.m_domianID;
                Dictionary<int, int> allUsersFromDB = DomainDal.GetUsersInDomainIncludeDeleted(domainID, nGroupID);
                if (allUsersFromDB != null)
                {
                    lDomainsUsers = allUsersFromDB.Keys.ToList();
                }
                else
                {
                    lDomainsUsers.Add(int.Parse(sSiteGUID));
                }
            }
            else
            {
                lDomainsUsers.Add(int.Parse(sSiteGUID));
            }

            //change the user pending to users without (-1)
            lDomainsUsers = lDomainsUsers.ConvertAll(x => Math.Abs(x));

            return lDomainsUsers;
        }

        private static List<int> GetDomainsUsers(int nDomainID, Int32 nGroupID, bool bGetAlsoPendingUsers = true)
        {

            List<int> intUsersList = new List<int>();
            List<string> usersList = Core.Domains.Module.GetDomainUserList(nGroupID, nDomainID);

            if (usersList != null && usersList.Count > 0)
            {
                for (int i = 0; i < usersList.Count; i++)
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

            return intUsersList;
        }

        static public Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseConditionalAccess t)
        {
            Credentials wsc = new Credentials(sWSUserName, sWSPassword);
            int nGroupID = TvinciCache.WSCredentials.GetGroupID(eWSModules.CONDITIONALACCESS, wsc);

            if (nGroupID > 0)
            {
                Utils.GetBaseConditionalAccessImpl(ref t, nGroupID);
            }
            else
            {
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}", sWSPassword, sWSPassword));
            }

            return nGroupID;
        }

        static public Int32 GetGroupID(string sWSUserName, string sWSPassword)
        {
            Credentials wsc = new Credentials(sWSUserName, sWSPassword);
            int nGroupID = TvinciCache.WSCredentials.GetGroupID(eWSModules.CONDITIONALACCESS, wsc);

            if (nGroupID == 0)
            {
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}", sWSPassword, sWSPassword));
            }

            return nGroupID;
        }

        static public double GetCouponDiscountPercent(Int32 nGroupID, string sCouponCode)
        {
            double dCouponDiscountPercent = 0;
            CouponDataResponse theCouponData = null;

            theCouponData = Pricing.Module.GetCouponStatus(nGroupID, sCouponCode);

            if (theCouponData != null &&
                theCouponData.Status != null &&
                theCouponData.Status.Code == (int)eResponseStatus.OK &&
                theCouponData.Coupon != null &&
                theCouponData.Coupon.m_oCouponGroup != null &&
                theCouponData.Coupon.m_CouponStatus == CouponsStatus.Valid)
            {

                DiscountModule dCouponDiscount = theCouponData.Coupon.m_oCouponGroup.m_oDiscountCode;
                dCouponDiscountPercent = dCouponDiscount.m_dPercent;
            }

            return dCouponDiscountPercent;
        }

        static public string GetMediaFileCoGuid(int nGroupID, int nMediaFileID)
        {
            string sMediaFileCoGuid =
                DAL.ConditionalAccessDAL.GetMediaFileCoGuid(nGroupID, nMediaFileID);

            return sMediaFileCoGuid;
        }

        static public Subscription GetSubscriptionBytProductCode(Int32 nGroupID, string sProductCode, string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive)
        {
            return Pricing.Module.GetSubscriptionDataByProductCode(nGroupID, sProductCode, sCountryCd2, sLanguageCode3, sDeviceName, bGetAlsoUnActive);
        }

        internal static string GetBasicLink(int nGroupID, int[] nMediaFileIDs, int nMediaFileID, string sBasicLink, out int nStreamingCompanyID, out string fileType)
        {
            MeidaMaper[] mapper = GetMediaMapper(nGroupID, nMediaFileIDs);
            nStreamingCompanyID = 0;
            fileType = string.Empty;
            int mediaID = 0;
            if (mapper != null && mapper.Length > 0)
            {
                mediaID = ExtractMediaIDOutOfMediaMapper(mapper, nMediaFileID);
            }

            if (sBasicLink.Equals(string.Format("{0}||{1}", mediaID, nMediaFileID)))
            {
                string sBaseURL = string.Empty;
                string sStreamID = string.Empty;

                ConditionalAccessDAL.Get_BasicLinkData(nMediaFileID, ref sBaseURL, ref sStreamID, ref nStreamingCompanyID, ref fileType);

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
        internal static void SplitRefference(string sRefference, ref Int32 nMediaFileID, ref Int32 nMediaID, ref string sSubscriptionCode, ref string sPPVCode, ref string sPrePaidCode,
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

                switch (sType)
                {
                    case "pp":
                        #region Handle PPV Transaction

                        HeaderObj = new JWTHeaderObject(JWTHeaderObject.JWTHash.HS256, "1", "JWT");
                        PPVModule pp = Pricing.Module.GetPPVModuleData(nGroupID, sPPVModuleCode, sCountryCd, sLanguageCode, sDeviceName);


                        var pp_description = (from descValue in pp.m_sDescription
                                                where descValue.m_sLanguageCode3 == sLanguageCode
                                                select descValue.m_sValue.ToString()).FirstOrDefault();

                        ClaimObj = new InAppItemObject(pp.m_sObjectVirtualName, pp_description.ToString(), dChargePrice.ToString(), sCurrencyCode, nCustomDataID.ToString(), MY_SELLER_ID, 60, "Google", "google/payments/inapp/item/v1", 0);

                        #endregion
                        break;
                    case "sp":
                        #region Subscription Purchase

                        HeaderObj = new JWTHeaderObject(JWTHeaderObject.JWTHash.HS256, "1", "JWT");

                        Subscription sp = Pricing.Module.GetSubscriptionData(nGroupID, sSubscriptionCode, sCountryCd, sLanguageCode, sDeviceName, false);

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

                        ClaimObj = new InAppItemObject(sp.m_sObjectVirtualName, sp_description.ToString(), dChargePrice.ToString(), scurrency, "prorated", nCustomDataID.ToString(), MY_SELLER_ID, 60, dChargePrice.ToString(), scurrency, "", fequencey, sNumberOfRecPeriods, "Google", "google/payments/inapp/subscription/v1", 0);



                        #endregion
                        break;
                    case "prepaid":
                        #region Handle PrePaid Transaction

                        #endregion
                        break;
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

        private static bool IsEntitledToPreviewModule(string sSiteGUID, Int32 nGroupID, string sSubCode, Subscription s, ref Price p, ref PriceReason theReason, int domainID)
        {
            bool res = true;
            if (s.m_oPreviewModule == null || s.m_oPreviewModule.m_nID == 0)
                return false;
            Dictionary<DateTime, List<int>> dict = GetPreviewModuleDataRelatedToUserFromDB(sSiteGUID, nGroupID, sSubCode, domainID);
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

        private static Dictionary<DateTime, List<int>> GetPreviewModuleDataRelatedToUserFromDB(string sSiteGuid, int nGroupID, string sSubCode, int domainID)
        {
            Dictionary<DateTime, List<int>> res = null;
            DataTable dt = ConditionalAccessDAL.Get_PreviewModuleDataForEntitlementCalc(nGroupID, sSiteGuid, sSubCode, domainID);
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
            return Core.Users.Module.GetUserData(nGroupID, sSiteGUID, string.Empty);
        }

        static public bool IsCouponValid(int nGroupID, string sCouponCode)
        {
            bool result = false;
            try
            {
                if (!string.IsNullOrEmpty(sCouponCode))
                {
                    CouponDataResponse couponData = Pricing.Module.GetCouponStatus(nGroupID, sCouponCode);

                    if (couponData != null &&
                        couponData.Status != null &&
                        couponData.Status.Code == (int)eResponseStatus.OK &&
                        couponData.Coupon != null &&
                        couponData.Coupon.m_CouponStatus == CouponsStatus.Valid)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                log.Error("IsCouponValid - " + string.Format("Error on IsCouponValid(), group id:{0}, coupon code:{1}, errorMessage:{2}", nGroupID, sCouponCode, ex.ToString()), ex);
            }
            return result;
        }

        static public CouponData GetCouponData(int groupID, string couponCode)
        {
            CouponData result = null;
            try
            {
                if (!string.IsNullOrEmpty(couponCode))
                {
                    CouponDataResponse couponResponse = Core.Pricing.Module.GetCouponStatus(groupID, couponCode);

                    if (couponResponse != null &&
                        couponResponse.Status != null &&
                        couponResponse.Status.Code == (int)eResponseStatus.OK &&
                        couponResponse.Coupon != null &&
                        couponResponse.Coupon.m_CouponStatus == CouponsStatus.Valid)
                    {
                        result = couponResponse.Coupon;
                    }
                }
            }
            catch (Exception ex)
            {
                result = null;
                log.Error("GetCouponData - " + string.Format("Error on GetCouponData(), group id:{0}, coupon code:{1}, errorMessage:{2}", groupID, couponCode, ex.ToString()), ex);
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

        /*
         * 1. Caching of pricing items in CAS is deprecated. Now all caching is done on Pricing side.
         * 
         */
        internal static PPVModule GetPPVModuleDataWithCaching<T>(T ppvCode, string wsUsername, string wsPassword,
            int groupID, string countryCd, string langCode, string deviceName)
        {
            return Pricing.Module.GetPPVModuleData(groupID, ppvCode.ToString(), countryCd, langCode, deviceName);
        }

        /*
         * 1. Caching of pricing items in CAS is deprecated. Now all caching is done on Pricing side.
         */
        internal static UsageModule GetUsageModuleDataWithCaching<T>(T usageModuleCode, string wsUsername, string wsPassword,
            string countryCode, string langCode, string deviceName, int groupID, string methodName)
        {
            return Pricing.Module.GetUsageModuleData(groupID, usageModuleCode.ToString(), countryCode, langCode, deviceName);
        }

        internal static bool GetMediaFileIDByCoGuid(string coGuid, int groupID, string siteGuid, ref int mediaFileID)
        {
            string key = LayeredCacheKeys.GetFileCoGuidKey(coGuid);
            bool cacheResult = LayeredCache.Instance.Get<int>(key, ref mediaFileID, Get_MediaFileIDByCoGuid, new Dictionary<string, object>() { { "groupID", groupID }, { "coGuid", coGuid } },
                                                              groupID, LayeredCacheConfigNames.MEDIA_FILE_ID_BY_CO_GUID_LAYERED_CACHE_CONFIG_NAME);
            if (!cacheResult)
            {
                log.ErrorFormat("fails Get Media FileID By CoGuid groupID:{0}, siteGuid:{1} ", groupID, siteGuid);
            }
            return cacheResult;  
        }

        internal static bool GetStreamingUrlType(int fileMainStreamingCoID, ref string CdnStrID)
        {
            bool isDynamic = false;

            string keyUrlType = string.Format("{0}_GetStreamingUrlType_{1}", ApiObjects.eWSModules.CONDITIONALACCESS, fileMainStreamingCoID);
            string keyCDN = string.Format("{0}_GetStreamingCDN_{1}", ApiObjects.eWSModules.CONDITIONALACCESS, fileMainStreamingCoID);

            bool resURL = ConditionalAccessCache.GetItem<bool>(keyUrlType, out isDynamic);
            bool resCDN = ConditionalAccessCache.GetItem<string>(keyCDN, out CdnStrID);

            if (!resURL || !resCDN)
            {

                int nUrlType = DAL.ConditionalAccessDAL.GetStreamingUrlType(fileMainStreamingCoID, ref CdnStrID);
                switch (nUrlType)
                {
                    case (int)eUrlType.Dynamic:
                        isDynamic = true;
                        break;
                    case (int)eUrlType.Static:
                        break;
                    default:
                        break;
                }
                ConditionalAccessCache.AddItem(keyUrlType, isDynamic);
                ConditionalAccessCache.AddItem(keyCDN, CdnStrID);

            }

            return isDynamic;
        }

        internal static string GetDateFormat(DateTime dateTime, string formatDate)
        {
            if (dateTime != null)
            {
                return dateTime.ToString(formatDate);
            }
            return string.Empty;
        }

        internal static bool IsUserValid(string siteGuid, int groupID, ref int domainID, ref DAL.DomainSuspentionStatus eSuspnedStatus)
        {
            bool res = false;

            long temp = 0;
            if (!Int64.TryParse(siteGuid, out temp) || temp < 1)
                return false;

            UserResponseObject resp = Core.Users.Module.GetUserData(groupID, siteGuid, string.Empty);
            if (resp != null && resp.m_RespStatus == ResponseStatus.OK && resp.m_user != null && resp.m_user.m_domianID > 0)
            {
                domainID = resp.m_user.m_domianID;
                eSuspnedStatus = resp.m_user.m_eSuspendState;
                res = true;
            }
            else
            {
                res = false;
            }

            return res;
        }

        /// <summary>
        /// Returns a full domain object for a given ID
        /// </summary>
        /// <param name="p_nDomainId"></param>
        /// <param name="p_nGroupId"></param>
        /// <returns></returns>
        public static Domain GetDomainInfo(int p_nDomainId, int p_nGroupId)
        {
            Domain oDomain = null;

            try
            {
                var res = Core.Domains.Module.GetDomainInfo(p_nGroupId, p_nDomainId);
                if (res != null && res.Status != null && res.Status.Code == (int)eResponseStatus.OK)
                {
                    oDomain = res.Domain;
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " +
                    string.Format("Failed getting domain info from WS. Domain Id = {0}, Group Id = {1}, Msg = {2}", p_nDomainId, p_nGroupId, ex.Message), ex);
            }
            return (oDomain);
        }

        public static ChangeDLMObj ChangeDLM(int groupID, long domainId, int dlmID)
        {
            ChangeDLMObj changeDLMObj = null;

            try
            {
                 changeDLMObj = Core.Domains.Module.ChangeDLM(groupID, (int)domainId, dlmID);
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + string.Format("Failed changing DLM using Domains WS. domainId = {0}, dlmID = {1}, groupID = {2}, Msg = {3}", domainId, dlmID, groupID, ex.Message), ex);
            }
            return changeDLMObj;
        }

       
        internal static Tuple<Dictionary<string, DataTable>, bool> Get_FileAndMediaBasicDetails(Dictionary<string, object> funcParams)
        {
            bool res = false;           
            Dictionary<string, DataTable> result = new Dictionary<string, DataTable>();            
            try
            {
                 if (funcParams != null && funcParams.Count == 2 && funcParams.ContainsKey("fileIDs") && funcParams.ContainsKey("groupId"))
                {
                    string key = string.Empty;
                     int[] fileIDs;

                    int? groupId = funcParams["groupId"] as int?;                    
                    fileIDs = funcParams["fileIDs"] != null ? funcParams["fileIDs"] as int[] : null;

                    if (fileIDs != null && groupId.HasValue)
                    {
                        DataTable dt = Tvinci.Core.DAL.CatalogDAL.Get_ValidateMediaFiles(fileIDs, groupId.Value);
                        if (dt != null && dt.Rows != null)
                        {
                            DataTable tempDt;
                            foreach (DataRow dr in dt.Rows)
                            {
                                tempDt = dt.Clone();
                                tempDt.ImportRow(dr);
                                result.Add(ODBCWrapper.Utils.GetSafeStr(dr, "media_file_id"), tempDt);
                            }
                        }

                        List<int> missingKeys = fileIDs.Where(x => !result.ContainsKey(x.ToString())).ToList();
                        if (missingKeys != null)
                        {
                            DataTable tempDt = dt != null ? dt.Clone() : new DataTable();
                            foreach (int missingKey in missingKeys)
                            {
                                result.Add(LayeredCacheKeys.GetFileAndMediaBasicDetailsKey(missingKey), tempDt);
                            }
                        }
                    } 
                    res = result.Keys.Count() == fileIDs.Count();

                    result = result.ToDictionary(x => LayeredCacheKeys.GetFileAndMediaBasicDetailsKey(int.Parse(x.Key)), x => x.Value);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Get_FileAndMediaBasicDetails failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<Dictionary<string, DataTable>, bool>(result, res);
        }

        // build dictionary - for each media file get one priceResonStatus mediaFilesStatus NotForPurchase, if UnKnown need to continue check that mediafile
        internal static Dictionary<int, MediaFileStatus> ValidateMediaFiles(int[] nMediaFiles, ref Dictionary<int, string> mediaFilesProductCode, int groupId)
        {
            Dictionary<int, MediaFileStatus> mediaFilesStatus = new Dictionary<int, MediaFileStatus>();
            mediaFilesProductCode = new Dictionary<int, string>();
            try
            {
                string productCode = string.Empty;
                MediaFileStatus eMediaFileStatus = MediaFileStatus.OK;

                //initialize all status as OK 
                foreach (int mf in nMediaFiles)
                {
                    if (!mediaFilesStatus.ContainsKey(mf))
                    {
                        mediaFilesStatus.Add(mf, eMediaFileStatus);
                        mediaFilesProductCode.Add(mf, productCode);
                    }
                }

                // get basic file details from cach / DB                 
                Dictionary<string, DataTable> fileDatatables = null;
                List<string> keys = nMediaFiles.Select(x => LayeredCacheKeys.GetFileAndMediaBasicDetailsKey(x)).ToList();

                // try to get from cache            
                bool cacheResult = LayeredCache.Instance.GetValues<DataTable>(keys, ref fileDatatables, Get_FileAndMediaBasicDetails, new Dictionary<string, object>() { { "fileIDs", nMediaFiles }, { "groupId", groupId } },
                                                                                groupId, LayeredCacheConfigNames.VALIDATE_MEDIA_FILES_LAYERED_CACHE_CONFIG_NAME);

                int mediaFileID;
                int mediaIsActive = 0, mediaFileIsActive = 0;
                int mediaStatus = 0, mediaFileStatus = 0;
                DateTime mediaStartDate, mediaFileStartDate;
                DateTime? mediaEndDate, mediaFileEndDate, mediaFinalEndDate;
                DateTime currentDate;

                if (cacheResult && fileDatatables != null)
                {

                    // get the media_file_id from key
                    // find keys not exsits in result
                    List<int> missingsKeys = mediaFilesStatus.Where(x => !fileDatatables.ContainsKey(LayeredCacheKeys.GetFileAndMediaBasicDetailsKey(x.Key))).Select(x => x.Key).ToList();
                    if (missingsKeys != null && missingsKeys.Count > 0)
                    {
                        foreach (int mf in missingsKeys)
                        {
                            if (mediaFilesStatus.ContainsKey(mf))
                            {
                                mediaFilesStatus[mf] = MediaFileStatus.NotForPurchase;
                            }
                            else
                            {
                                mediaFilesStatus.Add(mf, MediaFileStatus.NotForPurchase);
                            }
                        }
                    }


                    foreach (DataTable dt in fileDatatables.Values)
                    {
                        if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
                        {
                            DataRow dr = dt.Rows[0];
                            currentDate = DateTime.UtcNow;
                            //media
                            mediaIsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_is_active");
                            mediaStatus = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_status");
                            mediaStartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "media_start_date");
                            mediaEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "media_end_date");
                            mediaFinalEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "media_final_end_date");

                            //mediaFiles
                            mediaFileID = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_file_id");
                            mediaFileIsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "file_is_active");
                            mediaFileStatus = ODBCWrapper.Utils.GetIntSafeVal(dr, "file_status");
                            mediaFileStartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "file_start_date");
                            mediaFileEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "file_end_date");
                            productCode = ODBCWrapper.Utils.GetSafeStr(dr, "Product_Code");

                            if (!mediaFilesProductCode.ContainsKey(mediaFileID))
                            {
                                mediaFilesProductCode.Add(mediaFileID, productCode);
                            }
                            else
                            {
                                mediaFilesProductCode[mediaFileID] = productCode;
                            }

                            if (mediaIsActive != 1 || mediaStatus != 1 || mediaFileIsActive != 1 || mediaFileStatus != 1)
                            {
                                eMediaFileStatus = MediaFileStatus.NotForPurchase;
                            }
                            else if (mediaStartDate > currentDate || mediaFileStartDate > currentDate)
                            {
                                eMediaFileStatus = MediaFileStatus.NotForPurchase;
                            }
                            else if ((mediaFinalEndDate.HasValue && mediaFinalEndDate.Value < currentDate) || (mediaFileEndDate.HasValue && mediaFileEndDate.Value < currentDate))
                            {
                                eMediaFileStatus = MediaFileStatus.NotForPurchase;
                            }
                            else if ((mediaEndDate.HasValue && mediaEndDate.Value < currentDate) &&
                                (!mediaFinalEndDate.HasValue || (mediaFinalEndDate.HasValue && mediaFinalEndDate.Value > currentDate))) // cun see only if purchased
                            {
                                eMediaFileStatus = MediaFileStatus.ValidOnlyIfPurchase;
                            }

                            if (eMediaFileStatus != MediaFileStatus.OK)
                            {
                                if (mediaFilesStatus.ContainsKey(mediaFileID))
                                {
                                    mediaFilesStatus[mediaFileID] = eMediaFileStatus;
                                }
                                else
                                {
                                    mediaFilesStatus.Add(mediaFileID, eMediaFileStatus);
                                }
                            }
                        }
                    }
                }
                else
                {
                    log.Error(string.Format("ValidateMediaFiles -  LayeredCache return false for keys :{0}", string.Join(",", keys)));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
            }
            return mediaFilesStatus;
        }

        /// <summary>
        /// Validates that a user exists and belongs to a given domain
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="siteGuid"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public static ResponseStatus ValidateUser(int groupId, string siteGuid, ref long houseHoldID)
        {
            Users.User user;

            return ValidateUser(groupId, siteGuid, ref houseHoldID, out user);
        }

        /// <summary>
        /// Validates that a user exists and belongs to a given domain
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="siteGuid"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public static ResponseStatus ValidateUser(int groupId, string siteGuid, ref long houseHoldID, out Users.User user)
        {
            user = null;
            ResponseStatus status = ResponseStatus.InternalError;
            long lSiteGuid = 0;
            if (siteGuid.Length == 0 || !Int64.TryParse(siteGuid, out lSiteGuid) || lSiteGuid == 0)
            {
                status = ResponseStatus.UserDoesNotExist;
                return status;
            }

            try
            {
                UserResponseObject response = Core.Users.Module.GetUserData(groupId, siteGuid, string.Empty);

                // Make sure response is OK
                if (response != null)
                {
                    status = response.m_RespStatus;

                    if (status == ResponseStatus.OK)
                    {
                        //check Domain and suspend
                        if (response.m_user != null)
                        {
                            user = response.m_user;

                            if (houseHoldID != 0 && houseHoldID != response.m_user.m_domianID)
                            {
                                status = ResponseStatus.UserNotIndDomain;
                            }
                            else // no domain id was sent
                            {
                                houseHoldID = response.m_user.m_domianID;

                                if (houseHoldID == 0)
                                {
                                    status = ResponseStatus.UserNotIndDomain;
                                }
                            }

                            if (response.m_user.m_eSuspendState == DAL.DomainSuspentionStatus.Suspended)
                            {
                                status = ResponseStatus.UserSuspended;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("ValidateUser - " + string.Format("Error when validating user {0} in group {1}. ex = {2}, ST = {3}", siteGuid, groupId, ex.Message, ex.StackTrace), ex);
            }

            return status;
        }

        /// <summary>
        /// Validates that a domain exists
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public static ApiObjects.Response.Status ValidateDomain(int groupId, int domainId, out Domain domain)
        {
            domain = null;

            ApiObjects.Response.Status status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "Error validating domain" };

            try
            {
                DomainResponse response = Core.Domains.Module.GetDomainInfo(groupId, domainId);
                status = new ApiObjects.Response.Status(response.Status.Code, response.Status.Message);

                domain = response.Domain;
            }
            catch (Exception ex)
            {
                log.Error("ValidateDomain - " +
                    string.Format("Error when validating domain {0} in group {1}. ex = {2}, ST = {3}", domainId, groupId, ex.Message, ex.StackTrace),
                    ex);
            }
            return status;
        }

        public static int CalcPaymentNumber(int nNumOfPayments, int nPaymentNumber, bool bIsPurchasedWithPreviewModule)
        {
            int res = nPaymentNumber;
            if (nPaymentNumber == 0 && bIsPurchasedWithPreviewModule)
                res = 0;
            else
            {
                if (nNumOfPayments != 0)
                {
                    res = nPaymentNumber % nNumOfPayments;
                    if (res == 0)
                        res = nNumOfPayments;
                }
            }
            return res;
        }

        private static bool IsUserEntitled(List<int> p_lstFileIds, string p_sPPVCode, ref int p_nPPVID, ref string p_sSubCode,
            ref string p_sPPCode, ref int p_nWaiver, ref DateTime p_dCreateDate, ref string p_sPurchasedBySiteGuid, ref int p_nPurchasedAsMediaFileID, ref DateTime? p_dtStartDate, ref DateTime? p_dtEndDate,
            Dictionary<string, EntitlementObject> entitlements)
        {
            bool res = false;
            if (entitlements.Count > 0)
            {
                foreach (int mediaFileID in p_lstFileIds)
                {
                    string entitlementKey = mediaFileID + "_" + p_sPPVCode;
                    if (entitlements.ContainsKey(entitlementKey))
                    {
                        EntitlementObject entitlement = entitlements[entitlementKey];
                        p_nPPVID = entitlement.ID;
                        p_sSubCode = entitlement.subscriptionCode;
                        p_sPPCode = entitlement.relPP.ToString();
                        p_nWaiver = entitlement.waiver;
                        p_dtStartDate = entitlement.startDate;
                        p_dtEndDate = entitlement.endDate;
                        p_dCreateDate = entitlement.createDate;
                        p_sPurchasedBySiteGuid = entitlement.purchasedBySiteGuid;
                        p_nPurchasedAsMediaFileID = entitlement.purchasedAsMediaFileID;
                        res = true;
                        break;
                    }
                }
            }

            return res;
        }

        internal static DomainEntitlements.PPVEntitlements InitializeDomainPpvs(int groupId, int domainId, List<int> allUsersInDomain, MeidaMaper[] mapper)
        {
            DomainEntitlements.PPVEntitlements domainPpvEntitlements = new DomainEntitlements.PPVEntitlements();
            try
            {
                // Get all user entitlements
                domainPpvEntitlements.EntitlementsDictionary = ConditionalAccessDAL.Get_AllUsersEntitlements(domainId, allUsersInDomain);
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed InitializeDomainPpvs, groupId: {0}, domainId: {1}, allUsersInDomain: {2}", groupId, domainId, allUsersInDomain != null ? string.Join(",", allUsersInDomain) : ""), ex);
            }

            return domainPpvEntitlements;
        }

        private static List<int> GetRelatedFileIDs(int mediaID, int[] ppvGroupFileTypes, Dictionary<string, int> mediaIdGroupFileTypeMappings)
        {
            List<int> relatedFileTypes = new List<int>();
            if (ppvGroupFileTypes != null && ppvGroupFileTypes.Length > 0 && mediaIdGroupFileTypeMappings.Count > 0)
            {
                foreach (int groupFileTypeID in ppvGroupFileTypes)
                {
                    string mapKey = mediaID + "_" + groupFileTypeID;
                    if (mediaIdGroupFileTypeMappings.ContainsKey(mapKey))
                    {
                        relatedFileTypes.Add(mediaIdGroupFileTypeMappings[mapKey]);
                    }
                }
            }
            else
            {
                foreach (int mediaFileID in mediaIdGroupFileTypeMappings.Where(dic => dic.Key.StartsWith(mediaID.ToString())).Select(dic => dic.Value).ToList<int>())
                {
                    relatedFileTypes.Add(mediaFileID);
                }
                relatedFileTypes = relatedFileTypes.Distinct().ToList();
            }
            return relatedFileTypes;
        }

        internal static void GetAllUserBundles(int nGroupID, int domainID, List<int> lstUserIDs, DomainEntitlements.BundleEntitlements userBundleEntitlements)
        {
            DataSet dataSet = ConditionalAccessDAL.Get_AllBundlesInfoByUserIDsOrDomainID(domainID, lstUserIDs, nGroupID);
            if (dataSet != null && IsBundlesDataSetValid(dataSet))
            {
                userBundleEntitlements.EntitledSubscriptions = new Dictionary<string, UserBundlePurchase>();
                userBundleEntitlements.EntitledCollections = new Dictionary<string, UserBundlePurchase>();
                // iterate over subscriptions
                DataTable subs = dataSet.Tables[0];
                int waiver = 0;
                DateTime purchaseDate = DateTime.MinValue;
                DateTime endDate = DateTime.MinValue;

                if (subs != null && subs.Rows != null && subs.Rows.Count > 0)
                {
                    for (int i = 0; i < subs.Rows.Count; i++)
                    {
                        int numOfUses = 0;
                        int maxNumOfUses = 0;
                        string bundleCode = string.Empty;
                        int gracePeriodMinutes = 0;

                        GetSubscriptionBundlePurchaseData(subs.Rows[i], "SUBSCRIPTION_CODE", ref numOfUses, ref maxNumOfUses, ref bundleCode, ref waiver, ref purchaseDate, ref endDate, ref gracePeriodMinutes);

                        // decide which is the correct end period
                        if (endDate < DateTime.UtcNow)
                            endDate = endDate.AddMinutes(gracePeriodMinutes);

                        int subCode = 0;
                        if (Int32.TryParse(bundleCode, out subCode) && subCode > 0)
                        {
                            if (!userBundleEntitlements.EntitledSubscriptions.ContainsKey(bundleCode))
                            {
                                userBundleEntitlements.EntitledSubscriptions.Add(bundleCode, new UserBundlePurchase()
                                {
                                    sBundleCode = bundleCode,
                                    nWaiver = waiver,
                                    dtPurchaseDate = purchaseDate,
                                    dtEndDate = endDate,
                                    nNumOfUses = numOfUses,
                                    nMaxNumOfUses = maxNumOfUses
                                });
                            }
                        }
                    }
                }

                //iterate over collections
                DataTable colls = dataSet.Tables[1];
                if (colls != null && colls.Rows != null && colls.Rows.Count > 0)
                {
                    for (int i = 0; i < colls.Rows.Count; i++)
                    {
                        int numOfUses = 0;
                        int maxNumOfUses = 0;
                        string bundleCode = string.Empty;
                        waiver = 0;
                        purchaseDate = DateTime.MinValue;
                        endDate = DateTime.MinValue;

                        GetCollectionBundlePurchaseData(colls.Rows[i], "COLLECTION_CODE", ref numOfUses, ref maxNumOfUses, ref bundleCode, ref waiver, ref purchaseDate, ref endDate);

                        int collCode = 0;
                        if (Int32.TryParse(bundleCode, out collCode) && collCode > 0)
                        {
                            if (!userBundleEntitlements.EntitledCollections.ContainsKey(bundleCode))
                            {
                                userBundleEntitlements.EntitledCollections.Add(bundleCode, new UserBundlePurchase()
                                {
                                    sBundleCode = bundleCode,
                                    nWaiver = waiver,
                                    dtPurchaseDate = purchaseDate,
                                    dtEndDate = endDate
                                });
                            }
                        }
                        else
                        {
                            //log
                        }
                    }
                }
            }
            else
            {
                #region Logging
                StringBuilder sb = new StringBuilder("SP: ConditionalAccessDAL.Get_AllBundlesInfoByUserIDsOrDomainID returned corrupted data. ");
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
                sb.Append(string.Format(" domainID: {0}, group_id: {1}", domainID, nGroupID));
                log.Error("Error - " + sb.ToString());
                #endregion
            }
        }

        internal static DomainEntitlements.BundleEntitlements InitializeDomainBundles(int domainId, int groupId, List<int> allUsersInDomain, bool shouldPopulateBundles)
        {
            DomainEntitlements.BundleEntitlements domainBundleEntitlements = new DomainEntitlements.BundleEntitlements();
            try
            {
                GetAllUserBundles(groupId, domainId, allUsersInDomain, domainBundleEntitlements);
                if (shouldPopulateBundles)
                {
                    PopulateDomainBundles(domainId, groupId, domainBundleEntitlements);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed domainBundleEntitlements, groupId: {0}, domainId: {1}, allUsersInDomain: {2}", groupId, domainId, allUsersInDomain != null ? string.Join(",", allUsersInDomain) : ""), ex);
            }

            return domainBundleEntitlements;
        }

        internal static void PopulateDomainBundles(int domainId, int groupId, DomainEntitlements.BundleEntitlements domainBundleEntitlements)
        {
            domainBundleEntitlements.ChannelsToCollectionsMappings = new Dictionary<int, List<Collection>>();
            domainBundleEntitlements.ChannelsToSubscriptionMappings = new Dictionary<int, List<Subscription>>();
            domainBundleEntitlements.CollectionsData = new Dictionary<int, Collection>();
            domainBundleEntitlements.SubscriptionsData = new Dictionary<int, Subscription>();
            domainBundleEntitlements.FileTypeIdToSubscriptionMappings = new Dictionary<int, List<Subscription>>();

            try
            {
                if (domainBundleEntitlements.EntitledSubscriptions != null && domainBundleEntitlements.EntitledSubscriptions.Count > 0)
                {
                    SubscriptionsResponse subscriptionsResponse = Core.Pricing.Module.GetSubscriptionsData(groupId, domainBundleEntitlements.EntitledSubscriptions.Keys.ToArray(), String.Empty, String.Empty, String.Empty);
                    if (subscriptionsResponse != null && subscriptionsResponse.Status.Code == (int)eResponseStatus.OK && subscriptionsResponse.Subscriptions.Count() > 0)
                    {
                        foreach (Subscription subscription in subscriptionsResponse.Subscriptions)
                        {
                            // Insert to subscriptionData if subscriptionCode isn't already contained
                            int subscriptionCode;
                            if (int.TryParse(subscription.m_sObjectCode, out subscriptionCode) && !domainBundleEntitlements.SubscriptionsData.ContainsKey(subscriptionCode))
                            {
                                domainBundleEntitlements.SubscriptionsData.Add(subscriptionCode, subscription);
                            }

                            // Insert to channelsToSubscriptionMappings
                            if (subscription.m_sCodes != null)
                            {
                                foreach (BundleCodeContainer bundleCode in subscription.m_sCodes)
                                {
                                    int channelID;
                                    if (int.TryParse(bundleCode.m_sCode, out channelID) && domainBundleEntitlements.ChannelsToSubscriptionMappings.ContainsKey(channelID))
                                    {
                                        domainBundleEntitlements.ChannelsToSubscriptionMappings[channelID].Add(subscription);
                                    }
                                    else if (channelID > 0)
                                    {
                                        domainBundleEntitlements.ChannelsToSubscriptionMappings.Add(channelID, new List<Subscription>() { subscription });
                                    }
                                }
                            }

                            // Insert to fileTypeIdToSubscriptionMappings
                            if (subscription.m_sFileTypes != null && subscription.m_sFileTypes.Count() > 0)
                            {
                                foreach (int fileTypeID in subscription.m_sFileTypes)
                                {
                                    if (domainBundleEntitlements.FileTypeIdToSubscriptionMappings.ContainsKey(fileTypeID))
                                    {
                                        domainBundleEntitlements.FileTypeIdToSubscriptionMappings[fileTypeID].Add(subscription);
                                    }
                                    else
                                    {
                                        domainBundleEntitlements.FileTypeIdToSubscriptionMappings.Add(fileTypeID, new List<Subscription>() { subscription });
                                    }
                                }
                            }
                            else
                            {
                                if (domainBundleEntitlements.FileTypeIdToSubscriptionMappings.ContainsKey(0))
                                {
                                    domainBundleEntitlements.FileTypeIdToSubscriptionMappings[0].Add(subscription);
                                }
                                else
                                {
                                    domainBundleEntitlements.FileTypeIdToSubscriptionMappings.Add(0, new List<Subscription>() { subscription });
                                }
                            }
                        }
                    }
                }

                if (domainBundleEntitlements.EntitledCollections != null && domainBundleEntitlements.EntitledCollections.Count > 0)
                {
                    Collection[] collectionsArray = Core.Pricing.Module.GetCollectionsData(groupId, domainBundleEntitlements.EntitledCollections.Keys.ToArray(), String.Empty, String.Empty, String.Empty);
                    if (collectionsArray != null && collectionsArray.Length > 0)
                    {
                        foreach (Collection collection in collectionsArray)
                        {
                            int collectionCode;
                            if (int.TryParse(collection.m_sObjectCode, out collectionCode) && !domainBundleEntitlements.CollectionsData.ContainsKey(collectionCode))
                            {
                                domainBundleEntitlements.CollectionsData.Add(collectionCode, collection);

                                // Insert to channelsToSubscriptionMappings
                                if (collection.m_sCodes != null)
                                {
                                    foreach (BundleCodeContainer bundleCode in collection.m_sCodes)
                                    {
                                        int channelID;
                                        if (int.TryParse(bundleCode.m_sCode, out channelID) && domainBundleEntitlements.ChannelsToCollectionsMappings.ContainsKey(channelID))
                                        {
                                            domainBundleEntitlements.ChannelsToCollectionsMappings[channelID].Add(collection);
                                        }
                                        else if (channelID > 0)
                                        {
                                            domainBundleEntitlements.ChannelsToCollectionsMappings.Add(channelID, new List<Collection>() { collection });
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed PopulateDomainBundles, groupId: {0}, domainId: {1}", groupId, domainId), ex);
            }
        }

        private static void GetUserValidBundles(int mediaID, int nMediaFileID, MediaFileStatus eMediaFileStatus, int nGroupID, int[] fileTypes, List<int> allUserIDsInDomain, 
                                                List<int> relatedMediaFileIDs, Dictionary<string, UserBundlePurchase> subsPurchase, Dictionary<string, UserBundlePurchase> collPurchases,
                                                Dictionary<int, List<Subscription>> fileTypeIdToSubscriptionMappings, Dictionary<int, Subscription> subscriptionsData, Dictionary<int, Collection> collectionsData,
                                                Dictionary<int, List<Subscription>> channelsToSubscriptionMappings, Dictionary<int, List<Collection>> channelsToCollectionsMappings,
                                                ref Subscription[] relevantValidSubscriptions, ref Collection[] relevantValidCollections)
        {
            List<string> subsToBundleCreditDownloadedQuery = new List<string>();
            List<string> colsToBundleCreditDownloadedQuery = new List<string>();
            List<int> subsToGetFromSubsDictionary = new List<int>();
            List<int> collsToGetFromDictionary = new List<int>();

            if (fileTypeIdToSubscriptionMappings.Count > 0)
            {
                int allFileTypeIDs_key = 0;
                List<UserBundlePurchase> subscriptionsToCheck = new List<UserBundlePurchase>();
                foreach (int filetypeID in fileTypes)
                {
                    // the subscriptions and collections we add to those list will be sent to the Catalog in order to determine whether the media
                    // given as input belongs to it.                

                    // subscriptions with all fileTypes
                    if (fileTypeIdToSubscriptionMappings.ContainsKey(allFileTypeIDs_key))
                    {
                        foreach (Subscription subscription in fileTypeIdToSubscriptionMappings[allFileTypeIDs_key])
                        {
                            subscriptionsToCheck.Add(subsPurchase[subscription.m_SubscriptionCode]);
                        }
                    }
                    // subscriptions with the current fileTypeID
                    if (fileTypeIdToSubscriptionMappings.ContainsKey(filetypeID))
                    {
                        foreach (Subscription subscription in fileTypeIdToSubscriptionMappings[filetypeID])
                        {
                            subscriptionsToCheck.Add(subsPurchase[subscription.m_SubscriptionCode]);
                        }
                    }
                }

                foreach (UserBundlePurchase bundle in subscriptionsToCheck)
                {
                    // add to bulk query of Bundle_DoesCreditNeedToDownloaded to DB
                    //afterwards, the subs who pass the Bundle_DoesCreditNeedToDownloaded to DB test add to Catalog request.
                    if (eMediaFileStatus == MediaFileStatus.ValidOnlyIfPurchase || !IsUserCanStillUseSub(bundle.nNumOfUses, bundle.nMaxNumOfUses))
                    {
                        subsToBundleCreditDownloadedQuery.Add(bundle.sBundleCode);
                    }
                    else
                    {
                        // add to Catalog's BundlesContainingMediaRequest
                        int subCode = 0;
                        if (Int32.TryParse(bundle.sBundleCode, out subCode) && subCode > 0)
                        {
                            subsToGetFromSubsDictionary.Add(subCode);
                        }
                    }
                }
            }

            foreach (UserBundlePurchase bundle in collPurchases.Values)
            {
                // add to bulk query of Bundle_DoesCreditNeedToDownload to DB
                //afterwards, the colls which pass the Bundle_DoesCreditNeedToDownloaded to DB test add to Catalog request.
                // finally, the colls which pass the catalog need to be validated against PPV_DoesCreditNeedToDownloadedUsingCollection
                if (eMediaFileStatus == MediaFileStatus.ValidOnlyIfPurchase || !IsUserCanStillUseSub(bundle.nNumOfUses, bundle.nMaxNumOfUses))
                {
                    colsToBundleCreditDownloadedQuery.Add(bundle.sBundleCode);
                }
                else
                {
                    // add to Catalog's BundlesContainingMediaRequest
                    int collCode = 0;
                    if (Int32.TryParse(bundle.sBundleCode, out collCode) && collCode > 0)
                    {
                        collsToGetFromDictionary.Add(collCode);
                    }
                }
            }

            // check if credit need to be downloaded for specific mediaFileID 
            HandleBundleCreditNeedToDownloadedQuery(subsToBundleCreditDownloadedQuery, colsToBundleCreditDownloadedQuery, nMediaFileID, nGroupID, allUserIDsInDomain,
                                                    relatedMediaFileIDs, ref subsToGetFromSubsDictionary, ref collsToGetFromDictionary);

            // the subs / collections already purchased (no need to download credit)
            if (eMediaFileStatus == MediaFileStatus.ValidOnlyIfPurchase)
            {
                List<Subscription> relevantValidSubscriptionsList = new List<Subscription>();
                List<Collection> relevantValidCollectionsList = new List<Collection>();
                //get subscriptions data from dictionary instead of going to catalog
                foreach (int subscriptionCode in subsToGetFromSubsDictionary)
                {
                    //get subscription from all subscriptions data dictionary
                    relevantValidSubscriptionsList.Add(subscriptionsData[subscriptionCode]);
                }

                relevantValidSubscriptions = relevantValidSubscriptionsList.ToArray();

                //get collections data from dictionary instead of going to catalog
                foreach (int collectionCode in subsToGetFromSubsDictionary)
                {
                    //get subscription from all subscriptions data dictionary
                    relevantValidCollectionsList.Add(collectionsData[collectionCode]);
                }

                relevantValidCollections = relevantValidCollectionsList.ToArray();
            }
            else // only if in the gap between end date to final end date - continue the check
            {
                // get distinct subs from subs list, same for collection

                List<int> validatedColls = new List<int>();
                List<int> channelsToCheck = new List<int>();
                int[] validatedChannels = null;

                foreach (int subsCode in subsToGetFromSubsDictionary.Distinct().ToList())
                {
                    if (subscriptionsData[subsCode].m_sCodes != null)
                    {
                        foreach (BundleCodeContainer bundleCode in subscriptionsData[subsCode].m_sCodes)
                        {
                            int channelID;
                            if (int.TryParse(bundleCode.m_sCode, out channelID))
                                channelsToCheck.Add(channelID);
                        }
                    }
                }

                foreach (int collCode in collsToGetFromDictionary.Distinct().ToList())
                {
                    if (collectionsData[collCode].m_sCodes != null)
                    {
                        foreach (BundleCodeContainer bundleCode in collectionsData[collCode].m_sCodes)
                        {
                            int channelID;
                            if (int.TryParse(bundleCode.m_sCode, out channelID))
                                channelsToCheck.Add(channelID);
                        }
                    }
                }

                if (channelsToCheck.Count > 0)
                {
                    ValidateMediaContainedInChannels(mediaID, nGroupID, channelsToCheck.Distinct().ToList(), ref validatedChannels);
                }

                if (validatedChannels != null)
                {
                    List<Subscription> relevantValidSubscriptionsList = new List<Subscription>();
                    foreach (int channelID in validatedChannels)
                    {
                        //get subscriptions data from dictionary instead of going to catalog
                        if (channelsToSubscriptionMappings.ContainsKey(channelID))
                        {
                            relevantValidSubscriptionsList.AddRange(channelsToSubscriptionMappings[channelID]);
                        }

                        // save validated collections, used for checking PPV_CreditNeedToDownloadedUsingCollection
                        if (channelsToCollectionsMappings.ContainsKey(channelID))
                        {
                            foreach (Collection collection in channelsToCollectionsMappings[channelID])
                            {
                                int collectionCode;
                                if (int.TryParse(collection.m_sObjectCode, out collectionCode))
                                {
                                    validatedColls.Add(collectionCode);
                                }
                            }
                        }
                    }
                    relevantValidSubscriptions = relevantValidSubscriptionsList.ToArray();
                }

                // now validate bulk collections - PPV_CreditNeedToDownloadedUsingCollection

                if (validatedColls != null && validatedColls.Count > 0)
                {
                    Dictionary<int, bool> collsAfterPPVCreditValidation = PPVBulkDoCreditNeedToDownloadedUsingCollections(nGroupID,
                        nMediaFileID, allUserIDsInDomain, validatedColls);
                    List<int> finalCollCodes = GetFinalCollectionCodes(collsAfterPPVCreditValidation);
                    if (finalCollCodes != null && finalCollCodes.Count > 0)
                    {
                        List<Collection> relevantValidCollectionsList = new List<Collection>();
                        //get collections data from dictionary instead of going to catalog
                        foreach (int collectionCode in finalCollCodes)
                        {
                            //get subscription from all subscriptions data dictionary
                            relevantValidCollectionsList.Add(collectionsData[collectionCode]);
                        }

                        relevantValidCollections = relevantValidCollectionsList.ToArray();
                    }
                }
            }
        }

        private static ChannelsContainingMediaRequest InitializeCatalogChannelsRequest(int nGroupID, int nMediaID, List<int> channelsToCheck)
        {
            ChannelsContainingMediaRequest request = new ChannelsContainingMediaRequest();
            request.m_nGroupID = nGroupID;
            request.m_nMediaID = nMediaID;
            request.m_oFilter = new Filter();
            FillCatalogSignature(request);
            request.m_lChannles = new List<int>();
            for (int i = 0; i < channelsToCheck.Count; i++)
            {
                request.m_lChannles.Add(channelsToCheck[i]);
            }

            return request;
        }

        private static void ValidateMediaContainedInChannels(int mediaID, int nGroupID, List<int> channelsToCheck, ref int[] validChannels)
        {
            ChannelsContainingMediaRequest request = InitializeCatalogChannelsRequest(nGroupID, mediaID, channelsToCheck);

            try
            {
                ChannelsContainingMediaResponse response = request.GetResponse(request) as ChannelsContainingMediaResponse;
                if (response != null && response.m_lChannellList != null && response.m_lChannellList.Count > 0)
                {
                    validChannels = response.m_lChannellList.ToArray();
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed ValidateMediaContainedInChannels Request To Catalog", ex);
            }
        }

        internal static bool ValidateFileTypesConatainedInGroup(int m_nGroupID, int[] fileTypeIDs)
        {
            bool isContained = true;
            // isContained set to true incase we want to ignore filetypeIDs (sent as null or empty)
            if (fileTypeIDs != null && fileTypeIDs.Length > 0)
            {
                // Get all the group file types
                Dictionary<int, int> groupFileTypes = ConditionalAccessDAL.Get_GroupMediaTypesIDs(m_nGroupID);
                if (groupFileTypes != null && groupFileTypes.Count > 0)
                {
                    // Validate that all the fileTypeIDs in the request are contained in the groupFileTypes
                    if (groupFileTypes.Keys.Intersect(fileTypeIDs).Count() != fileTypeIDs.Length)
                    {
                        isContained = false;
                    }
                }
            }

            return isContained;
        }

        internal static ApiObjects.Response.Status ValidateUserAndDomain(int groupId, string siteGuid, ref long householdId, out Domain domain)
        {
            Users.User user;
            return ValidateUserAndDomain(groupId, siteGuid, ref householdId, out domain, out user);
        }

        internal static ApiObjects.Response.Status ValidateUserAndDomain(int groupId, string siteGuid, ref long householdId, out Domain domain, out Users.User user)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();
            status.Code = -1;
            domain = null;
            user = null;

            // If no user - go immediately to domain validation
            if (string.IsNullOrEmpty(siteGuid))
            {
                status.Code = (int)eResponseStatus.OK;
            }
            else
            {
                // Get response from users WS
                ResponseStatus userStatus = ValidateUser(groupId, siteGuid, ref householdId, out user);
                if (householdId == 0)
                {
                    status.Code = (int)eResponseStatus.UserWithNoDomain;
                    status.Message = eResponseStatus.UserWithNoDomain.ToString();
                }
                else
                {
                    // Most of the cases are not interesting - focus only on those that matter
                    switch (userStatus)
                    {
                        case ResponseStatus.OK:
                            {
                                status.Code = (int)eResponseStatus.OK;
                                break;
                            }
                        case ResponseStatus.UserDoesNotExist:
                            {
                                status.Code = (int)eResponseStatus.UserDoesNotExist;
                                status.Message = eResponseStatus.UserDoesNotExist.ToString();
                                break;
                            }
                        case ResponseStatus.UserNotIndDomain:
                            {
                                status.Code = (int)eResponseStatus.UserNotInDomain;
                                status.Message = "User Not In Domain";
                                break;
                            }
                        case ResponseStatus.UserWithNoDomain:
                            {
                                status.Code = (int)eResponseStatus.UserWithNoDomain;
                                status.Message = eResponseStatus.UserWithNoDomain.ToString();
                                break;
                            }
                        case ResponseStatus.UserSuspended:
                            {
                                status.Code = (int)eResponseStatus.UserSuspended;
                                status.Message = eResponseStatus.UserSuspended.ToString();
                                break;
                            }
                        // Most cases will return general error
                        default:
                            {
                                status.Code = (int)eResponseStatus.Error;
                                status.Message = "Error validating user";
                                break;
                            }
                    }
                }
            }

            // If user is valid (or we don't have one)
            if (status.Code == (int)eResponseStatus.OK && householdId != 0)
            {
                //Get response from domains WS                
                status = ValidateDomain(groupId, (int)householdId, out domain);
            }
            return status;
        }

        internal static ApiObjects.Response.Status ValidateUserAndDomain(int groupId, string siteGuid, ref long householdId)
        {
            Domain domain;
            return ValidateUserAndDomain(groupId, siteGuid, ref householdId, out domain);
        }

        internal static List<int> GetChannelsListFromSubscriptions(List<Subscription> subscriptions)
        {
            List<int> channelsList = new List<int>();
            if (subscriptions != null && subscriptions.Count > 0)
            {
                foreach (Subscription subscription in subscriptions)
                {
                    if (subscription.m_sCodes != null)
                    {
                        // Get channels from subscriptions
                        foreach (BundleCodeContainer bundleCode in subscription.m_sCodes)
                        {
                            int channelID;
                            if (int.TryParse(bundleCode.m_sCode, out channelID) && !channelsList.Contains(channelID))
                            {
                                channelsList.Add(channelID);
                            }
                        }
                    }
                }
            }

            return channelsList;
        }

        internal static TimeShiftedTvPartnerSettings GetTimeShiftedTvPartnerSettings(int groupID)
        {
            string key = string.Format("TstvAccountSettings_{0}", groupID);
            TimeShiftedTvPartnerSettings settings = null;
            try
            {
                if (!TvinciCache.WSCache.Instance.TryGet(key, out settings))
                {
                    lock (lck)
                    {
                        if (!TvinciCache.WSCache.Instance.TryGet(key, out settings))
                        {
                            log.Debug("Getting TSTV Settings from DB");
                            DataRow dr = DAL.ApiDAL.GetTimeShiftedTvPartnerSettings(groupID);
                            if (dr != null)
                            {
                                int catchup = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_catch_up", 0);
                                int cdvr = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_cdvr", 0);
                                int startOver = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_start_over", 0);
                                int trickPlay = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_trick_play", 0);
                                long catchUpBuffer = ODBCWrapper.Utils.GetLongSafeVal(dr, "catch_up_buffer", 7);
                                long trickPlayBuffer = ODBCWrapper.Utils.GetLongSafeVal(dr, "trick_play_buffer", 1);
                                long recordingScheduleWindowBuffer = ODBCWrapper.Utils.GetLongSafeVal(dr, "recording_schedule_window_buffer", 0);
                                int recordingScheduleWindow = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_recording_schedule_window", -1);
                                long paddingAfterProgramEnds = ODBCWrapper.Utils.GetLongSafeVal(dr, "padding_after_program_ends", 0);
                                long paddingBeforeProgramStarts = ODBCWrapper.Utils.GetLongSafeVal(dr, "padding_before_program_starts", 0);
                                int protection = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_protection", 0);
                                int protectionPeriod = ODBCWrapper.Utils.GetIntSafeVal(dr, "protection_period", 90);
                                int protectionQuotaPercentage = ODBCWrapper.Utils.GetIntSafeVal(dr, "protection_quota_percentage", 25);
                                int recordingLifetimePeriod = ODBCWrapper.Utils.GetIntSafeVal(dr, "recording_lifetime_period", 182);
                                int cleanupNoticePeriod = ODBCWrapper.Utils.GetIntSafeVal(dr, "cleanup_notice_period", 7);
                                int enableSeriesRecording = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_series_recording", 1); //Default = enabled
                                int recordingPlaybackNonEntitledChannel = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_recording_playback_non_entitled", 0); // Default = disabled
                                int recordingPlaybackNonExistingChannel = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_recording_playback_non_existing", 0); // Default = disabled
                                int quotaOveragePolicy = ODBCWrapper.Utils.GetIntSafeVal(dr, "quota_overage_policy", 0);
                                int protectionPolicy = ODBCWrapper.Utils.GetIntSafeVal(dr, "protection_policy", 0); 
 
                                if (recordingScheduleWindow > -1)
                                {
                                    settings = new TimeShiftedTvPartnerSettings(catchup == 1, cdvr == 1, startOver == 1, trickPlay == 1, recordingScheduleWindow == 1, catchUpBuffer,
                                                                                trickPlayBuffer, recordingScheduleWindowBuffer, paddingAfterProgramEnds, paddingBeforeProgramStarts,
                                                                                protection == 1, protectionPeriod, protectionQuotaPercentage, recordingLifetimePeriod, cleanupNoticePeriod, enableSeriesRecording == 1,
                                                                                recordingPlaybackNonEntitledChannel == 1, recordingPlaybackNonExistingChannel == 1, quotaOveragePolicy, protectionPolicy);
                                    TvinciCache.WSCache.Instance.Add(key, settings);
                                }
                            }
                        }
                    }
                }
                else if (settings == null)
                {
                    log.Error("TSTV settings is null");
                }

                log.DebugFormat("current TSTV settings values are: {0}", settings != null ? settings.ToString() : "null");
            }

            catch (Exception ex)
            {
                log.Error("GetTimeShiftedTvPartnerSettings - " + string.Format("Error in GetTimeShiftedTvPartnerSettings: groupID = {0} ex = {1}", groupID, ex.Message, ex.StackTrace), ex);
            }

            return settings;
        }

        internal static List<EPGChannelProgrammeObject> GetEpgsByIds(int nGroupID, List<long> epgIds)
        {
            List<EPGChannelProgrammeObject> epgs = null;

            try
            {
                EpgProgramDetailsRequest request = new EpgProgramDetailsRequest();
                request.m_nGroupID = nGroupID;
                //don't get the same EPG from catalog
                request.m_lProgramsIds = epgIds.ConvertAll<int>(x => (int)x).Distinct().ToList();
                request.m_oFilter = new Filter();
                FillCatalogSignature(request);

                EpgProgramResponse response = request.GetProgramsByIDs(request) as EpgProgramResponse;
                if (response != null && response.m_nTotalItems > 0 && response.m_lObj != null && response.m_lObj.Count > 0)
                {
                    epgs = new List<EPGChannelProgrammeObject>();
                    foreach (ProgramObj program in response.m_lObj)
                    {
                        // no need to check epg status since catalog returns only active epg's
                        if (program.AssetType == eAssetTypes.EPG && program.m_oProgram != null && program.m_oProgram.EPG_ID > 0)
                        {
                            epgs.Add(program.m_oProgram);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error("Failed GetEpgsByIds Request To Catalog", ex);
            }

            return epgs;
        }

        internal static bool IsValidRecordingStatus(TstvRecordingStatus recordingStatus, bool isOkStatusValid = false)
        {
            bool res = false;
            switch (recordingStatus)
            {
                case TstvRecordingStatus.OK:
                    res = isOkStatusValid;
                    break;

                case TstvRecordingStatus.Recording:
                case TstvRecordingStatus.Recorded:
                case TstvRecordingStatus.Scheduled:
                    res = true;
                    break;

                case TstvRecordingStatus.Deleted:
                case TstvRecordingStatus.Failed:
                case TstvRecordingStatus.Canceled:
                case TstvRecordingStatus.LifeTimePeriodExpired:
                default:
                    res = false;
                    break;
            }

            return res;
        }

        internal static List<int> GetGroupEnforcedServices(int groupID)
        {
            List<int> services;
            string key = string.Format("GroupEnforcedServices_{0}", groupID);
            if (!ConditionalAccessCache.GetItem<List<int>>(key, out services))
            {
                log.DebugFormat("Failed getting GroupEnforcedServices from cache, key: {0}", key);
                services = Tvinci.Core.DAL.CatalogDAL.GetGroupServices(groupID);
                if (services == null)
                {
                    log.ErrorFormat("Failed CatalogDAL.GetGroupServices for groupID: {0}", groupID);
                }
                else if (!ConditionalAccessCache.AddItem(key, services))
                {
                    log.ErrorFormat("Failed inserting GroupEnforcedServices to cache, key: {0}", key);
                }
            }

            return services;
        }

        internal static List<Recording> SearchDomainRecordingIDsByFilter(int groupID, string userID, long domainID, Dictionary<long, Recording> recordingIdToDomainRecording,
                                                                        string filter, int pageIndex, int pageSize, ApiObjects.SearchObjects.OrderObj orderBy, ref int totalResults)
        {
            List<Recording> recordings = null;

            try
            {
                UnifiedSearchRequest request = new UnifiedSearchRequest();                
                request.m_nGroupID = groupID;
                request.m_dServerTime = DateTime.UtcNow;
                request.m_sSiteGuid = userID;
                request.domainId = (int)domainID;
                request.m_nPageIndex = pageIndex;
                request.m_nPageSize = pageSize;
                request.assetTypes = new List<int> { 1 };
                request.filterQuery = filter;
                request.order = orderBy;
                request.hasPredefinedRecordings = true;

                KeyValuePair<eAssetTypes, long>[] recordingAssets = new KeyValuePair<eAssetTypes, long>[recordingIdToDomainRecording.Count];
                for (int i = 0; i < recordingIdToDomainRecording.Count; i++)
                {
                    recordingAssets[i] = new KeyValuePair<eAssetTypes, long>(eAssetTypes.NPVR, recordingIdToDomainRecording.ElementAt(i).Key);
                }
                request.specificAssets = recordingAssets.ToList();
                request.m_oFilter = new Filter()
                {
                    m_bOnlyActiveMedia = true
                };
                FillCatalogSignature(request);

                UnifiedSearchResponse response = request.GetResponse(request) as UnifiedSearchResponse;
                if (response != null && response.status.Code == (int)eResponseStatus.OK && response.m_nTotalItems > 0 && response.searchResults != null)
                {
                    recordings = new List<Recording>();
                    totalResults = response.m_nTotalItems;
                    foreach (UnifiedSearchResult unifiedSearchResult in response.searchResults)
                    {
                        // no need to check epg status since catalog returns only active epg's
                        long searchRecordingID;
                        if (unifiedSearchResult.AssetType == eAssetTypes.NPVR && long.TryParse(unifiedSearchResult.AssetId, out searchRecordingID) && searchRecordingID > 0)
                        {
                            if (recordingIdToDomainRecording.ContainsKey(searchRecordingID))
                            {
                                Recording recording = recordingIdToDomainRecording[searchRecordingID];
                                recordings.Add(recording);
                            }
                        }
                    }
                }

            }

            catch (Exception ex)
            {
                log.Error("Failed UnifiedSearchRequest Request To Catalog", ex);
            }

            return recordings;
        }

        internal static List<TstvRecordingStatus> ConvertToTstvRecordingStatus(List<DomainRecordingStatus> domainRecordingStatuses)
        {
            List<TstvRecordingStatus> result = new List<TstvRecordingStatus>();
            foreach (DomainRecordingStatus status in domainRecordingStatuses.Distinct())
            {
                switch (status)
                {
                    case DomainRecordingStatus.OK:
                        result.Add(TstvRecordingStatus.OK);
                        break;
                    case DomainRecordingStatus.Canceled:
                        result.Add(TstvRecordingStatus.Canceled);
                        break;
                    case DomainRecordingStatus.Deleted:
                        result.Add(TstvRecordingStatus.Deleted);
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        internal static TstvRecordingStatus? ConvertToTstvRecordingStatus(RecordingInternalStatus recordingInternalStatus, DateTime epgStartDate, DateTime epgEndDate)
        {
            TstvRecordingStatus? recordingStatus = null;
            switch (recordingInternalStatus)
            {
                case RecordingInternalStatus.Canceled:
                    recordingStatus = TstvRecordingStatus.Canceled;
                    break;
                case RecordingInternalStatus.Deleted:
                    recordingStatus = TstvRecordingStatus.Deleted;
                    break;
                case RecordingInternalStatus.Failed:
                    recordingStatus = TstvRecordingStatus.Failed;
                    break;
                case RecordingInternalStatus.Waiting:
                    /* Unlike RecordingInternalStatus.OK we don't check the epg end date because
                     * we won't return recorded since the adapter call is async, so if the program
                     * already started it doesn't matter if it finished or not we will return recording */                      
                    if (epgStartDate < DateTime.UtcNow)
                    {
                        recordingStatus = TstvRecordingStatus.Recording;
                    }
                    else
                    {
                        recordingStatus = TstvRecordingStatus.Scheduled;
                    }
                    break;
                case RecordingInternalStatus.OK:
                    // If program already finished, we say it is recorded
                    if (epgEndDate < DateTime.UtcNow)
                    {
                        recordingStatus = TstvRecordingStatus.Recorded;
                    }
                    // If program already started but didn't finish, we say it is recording
                    else if (epgStartDate < DateTime.UtcNow)
                    {
                        recordingStatus = TstvRecordingStatus.Recording;
                    }
                    else
                    {
                        recordingStatus = TstvRecordingStatus.Scheduled;
                    }
                    break;
                default:
                    break;
            }

            return recordingStatus;
        }

        internal static TstvRecordingStatus? ConvertToTstvRecordingStatus(DomainRecordingStatus domainRecordingStatus)
        {
            TstvRecordingStatus? recordingStatus = null;
            switch (domainRecordingStatus)
            {
                case DomainRecordingStatus.Canceled:
                    recordingStatus = TstvRecordingStatus.Canceled;
                    break;
                case DomainRecordingStatus.Deleted:
                    recordingStatus = TstvRecordingStatus.Deleted;
                    break;
                case DomainRecordingStatus.DeletedBySystem:
                    /***** Currently the LifeTimePeriodExpired status is only for backend inner needs and we are not exposing it on the REST to the client *****/
                    //recordingStatus = TstvRecordingStatus.LifeTimePeriodExpired;
                    recordingStatus = TstvRecordingStatus.Deleted;
                    break;
                case DomainRecordingStatus.OK:
                    recordingStatus = TstvRecordingStatus.OK;
                    break;
                case DomainRecordingStatus.SeriesDelete:
                    recordingStatus = TstvRecordingStatus.SeriesDelete;
                    break;
                case DomainRecordingStatus.SeriesCancel:
                    recordingStatus = TstvRecordingStatus.SeriesCancel;
                    break;
                default:
                    break;
            }

            return recordingStatus;
        }

        internal static TstvRecordingStatus? ConvertToSeriesStatus(TstvRecordingStatus status)
        {
            TstvRecordingStatus? recordingStatus = null;
            switch (status)
            {
                case TstvRecordingStatus.Canceled:
                    recordingStatus = TstvRecordingStatus.SeriesCancel;
                    break;
                case TstvRecordingStatus.Deleted:
                    recordingStatus = TstvRecordingStatus.SeriesDelete;
                    break;
                default:                   
                    break;
            }

            return recordingStatus;
        }

        internal static List<DomainRecordingStatus> ConvertToDomainRecordingStatus(List<TstvRecordingStatus> recordingStatus)
        {
            List<DomainRecordingStatus> result = new List<DomainRecordingStatus>();
            foreach (TstvRecordingStatus status in recordingStatus)
            {
                switch (status)
                {
                    case TstvRecordingStatus.Failed:
                    case TstvRecordingStatus.Scheduled:
                    case TstvRecordingStatus.Recording:
                    case TstvRecordingStatus.Recorded:
                        if (!result.Contains(DomainRecordingStatus.OK))
                        {
                            result.Add(DomainRecordingStatus.OK);
                        }
                        break;
                    case TstvRecordingStatus.Canceled:
                        if (!result.Contains(DomainRecordingStatus.Canceled))
                        {
                            result.Add(DomainRecordingStatus.Canceled);
                        }
                        break;
                    case TstvRecordingStatus.SeriesCancel:
                        if (!result.Contains(DomainRecordingStatus.SeriesCancel))
                        {
                            result.Add(DomainRecordingStatus.SeriesCancel);
                        }
                        break;
                    /***** Currently the LifeTimePeriodExpired status is only for backend inner needs and we are not exposing it on the REST to the client *****/
                    /*
                    // add both DomainRecordingStatus.OK and DomainRecordingStatus.DeletedByCleanup because we don't know if the recording has already been deleted
                    case TstvRecordingStatus.LifeTimePeriodExpired:
                        if (!result.Contains(DomainRecordingStatus.OK))
                        {
                            result.Add(DomainRecordingStatus.OK);
                        }

                        if (!result.Contains(DomainRecordingStatus.DeletedBySystem))
                        {
                            result.Add(DomainRecordingStatus.DeletedBySystem);
                        }
                        break;
                     */
                    case TstvRecordingStatus.Deleted:
                    case TstvRecordingStatus.SeriesDelete:
                    default:
                        break;
                }
            }

            return result;
        }

        internal static DomainRecordingStatus? ConvertToDomainRecordingStatus(TstvRecordingStatus recordingStatus)
        {
            DomainRecordingStatus? result = null;
            switch (recordingStatus)
            {
                case TstvRecordingStatus.Failed:
                case TstvRecordingStatus.Scheduled:
                case TstvRecordingStatus.Recording:
                case TstvRecordingStatus.Recorded:
                    result = DomainRecordingStatus.OK;
                    break;
                case TstvRecordingStatus.Canceled:
                    result = DomainRecordingStatus.Canceled;
                    break;
                case TstvRecordingStatus.Deleted:
                    result = DomainRecordingStatus.Deleted;
                    break;
                case TstvRecordingStatus.SeriesCancel:
                    result = DomainRecordingStatus.SeriesCancel;
                    break;
                case TstvRecordingStatus.SeriesDelete:
                    result = DomainRecordingStatus.SeriesDelete;
                    break;
                default:
                    break;
            }

            return result;
        }

        internal static Recording ValidateEpgForRecord(TimeShiftedTvPartnerSettings accountSettings, EPGChannelProgrammeObject epg, bool shouldCheckCatchUp)
        {
            Recording response = new Recording() { EpgId = epg.EPG_ID, Crid = epg.CRID, Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()) };
            try
            {
                if (epg.ENABLE_CDVR != 1)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ProgramCdvrNotEnabled, eResponseStatus.ProgramCdvrNotEnabled.ToString());
                    return response;
                }

                if (shouldCheckCatchUp)
                {
                    DateTime epgStartDate;
                    if (!DateTime.TryParseExact(epg.START_DATE, EPG_DATETIME_FORMAT, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out epgStartDate))
                    {
                        log.ErrorFormat("Failed parsing EPG start date, epgID: {0}, startDate: {1}", epg.EPG_ID, epg.START_DATE);
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                        return response;
                    }

                    // validate recording schedule window according to the paddedStartDate
                    DateTime paddedStartDate = epgStartDate.AddSeconds(accountSettings.PaddingBeforeProgramStarts.HasValue ? (-1) * accountSettings.PaddingBeforeProgramStarts.Value : 0);
                    if (accountSettings.IsRecordingScheduleWindowEnabled.HasValue && accountSettings.IsRecordingScheduleWindowEnabled.Value &&
                        accountSettings.RecordingScheduleWindow.HasValue && paddedStartDate.AddMinutes(accountSettings.RecordingScheduleWindow.Value) < DateTime.UtcNow)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ProgramNotInRecordingScheduleWindow, eResponseStatus.ProgramNotInRecordingScheduleWindow.ToString());
                        return response;
                    }

                    if (epgStartDate < DateTime.UtcNow)
                    {
                        if (!accountSettings.IsCatchUpEnabled.HasValue || !accountSettings.IsCatchUpEnabled.Value)
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AccountCatchUpNotEnabled, eResponseStatus.AccountCatchUpNotEnabled.ToString());
                            return response;
                        }
                        if (epg.ENABLE_CATCH_UP != 1)
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ProgramCatchUpNotEnabled, eResponseStatus.ProgramCatchUpNotEnabled.ToString());
                            return response;
                        }
                        if (epg.CHANNEL_CATCH_UP_BUFFER == 0 || epgStartDate.AddMinutes(epg.CHANNEL_CATCH_UP_BUFFER) < DateTime.UtcNow)
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.CatchUpBufferLimitation, eResponseStatus.CatchUpBufferLimitation.ToString());
                            return response;
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at ValidateEpgForRecord. ");
                sb.Append(String.Concat("epgID: ", epg.EPG_ID));
                sb.Append(String.Concat("Ex Msg: ", ex.Message));
                sb.Append(String.Concat(", Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(", Stack Trace: ", ex.StackTrace));
                log.Error(sb.ToString(), ex);

                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }

        internal static List<Recording> CheckDomainExistingRecordingsByEpgs(int groupId, long domainID, Dictionary<long, EPGChannelProgrammeObject> validEpgObjectForRecordingMap)
        {
            Dictionary<long, Recording> responseDictionary = new Dictionary<long, Recording>();
            TimeShiftedTvPartnerSettings accountSettings = Utils.GetTimeShiftedTvPartnerSettings(groupId);
            foreach (long epgId in validEpgObjectForRecordingMap.Keys)
            {
                Recording recording = new Recording() { EpgId = epgId };
                EPGChannelProgrammeObject epg = validEpgObjectForRecordingMap[epgId];
                DateTime epgStartDate;
                DateTime epgEndDate;
                long epgChannelId;
                if (DateTime.TryParseExact(epg.START_DATE, EPG_DATETIME_FORMAT, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out epgStartDate)
                    && DateTime.TryParseExact(epg.END_DATE, EPG_DATETIME_FORMAT, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out epgEndDate)
                    && long.TryParse(epg.EPG_CHANNEL_ID, out epgChannelId))
                {
                    recording = new Recording()
                    {
                        Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()),
                        EpgId = epg.EPG_ID,
                        ChannelId = epgChannelId,
                        Id = 0,
                        EpgStartDate = epgStartDate,
                        EpgEndDate = epgEndDate,
                        Crid = epg.CRID
                    };

                    if (accountSettings != null && accountSettings.PaddingBeforeProgramStarts.HasValue && accountSettings.PaddingAfterProgramEnds.HasValue)
                    {
                        recording.EpgStartDate = recording.EpgStartDate.AddSeconds((-1) * accountSettings.PaddingBeforeProgramStarts.Value);
                        recording.EpgEndDate = recording.EpgEndDate.AddSeconds(accountSettings.PaddingAfterProgramEnds.Value);
                    }
                    else
                    {
                        log.ErrorFormat("Failed getting account padding, epgId: {0}, groupID: {1}", groupId, recording.EpgId);
                        recording.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed getting account padding settings");
                    }
                }
                else
                {
                    log.ErrorFormat("Failed parsing EPG start / end date / epgChannelId, epgID: {0}, domainID: {1}, startDate: {2}, endDate: {3}, epgChannelId: {4}", epg.EPG_ID, domainID, epg.START_DATE, epg.END_DATE, epg.EPG_CHANNEL_ID);
                    recording = new Recording() { EpgId = epg.EPG_ID };
                }

                responseDictionary.Add(epgId, recording);
            }

            try
            {
                Dictionary<long, Recording> domainIdToDomainRecordingMap = Utils.GetDomainRecordingIdToRecordingMapByEpgIds(groupId, domainID, validEpgObjectForRecordingMap.Keys.ToList());
                if (domainIdToDomainRecordingMap != null)
                {
                    foreach (KeyValuePair<long, Recording> pair in domainIdToDomainRecordingMap)
                    {
                        Recording domainRecoridng = pair.Value;
                        // add domain recording if it doesn't already exist in dictionary and wasn't canceled or deleted
                        if (domainRecoridng != null && domainRecoridng.Status != null && domainRecoridng.Status.Code == (int)eResponseStatus.OK
                            && domainRecoridng.RecordingStatus != TstvRecordingStatus.Canceled && domainRecoridng.RecordingStatus != TstvRecordingStatus.Deleted)
                        {
                            domainRecoridng.Id = pair.Key;
                            responseDictionary[domainRecoridng.EpgId] = domainRecoridng;
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at CheckDomainExistingRecording. ");
                sb.Append(String.Concat("domainID: ", domainID));
                sb.Append(String.Concat("Ex Msg: ", ex.Message));
                sb.Append(String.Concat(", Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(", Stack Trace: ", ex.StackTrace));

                log.Error(sb.ToString(), ex);
            }

            return responseDictionary.Values.ToList();
        }

        internal static int MapActionTypeForAdapter(eEPGFormatType eformat)
        {
            int response = -1;

            switch (eformat)
            {
                case eEPGFormatType.Catchup:
                    response = (int)CdnAdapterActionType.Catchup;
                    break;
                case eEPGFormatType.StartOver:
                    response = (int)CdnAdapterActionType.StartOver;
                    break;
                case eEPGFormatType.LivePause:
                    response = (int)CdnAdapterActionType.LivePause;
                    break;
                case eEPGFormatType.NPVR:
                    break;
                default:
                    break;
            }

            return response;
        }

        internal static CDNAdapterResponse GetRelevantCDN(int groupId, int fileStreamingCompanyId, ApiObjects.eAssetTypes assetType, ref bool isDefaultAdapter)
        {
            CDNAdapterResponse adapterResponse = null;

            try
            {
                // if nStreamingCompany is 0 - call api service for getting the default adapter / streaming company
                if (fileStreamingCompanyId == 0)
                {
                    isDefaultAdapter = true;
                    adapterResponse = Api.Module.GetGroupDefaultCDNAdapter(groupId, assetType);
                }
                // else - call api service for getting the adapter / streaming company with the nStreamingCompany ID                
                else
                {
                    adapterResponse = Api.Module.GetCDNAdapter(groupId, fileStreamingCompanyId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetLicensedLink: failed calling WS API. groupId = {0}", groupId, ex);
                adapterResponse.Status.Code = (int)eResponseStatus.Error;
                adapterResponse.Status.Message = "Error";
                return adapterResponse;
            }

            if (adapterResponse == null || adapterResponse.Status == null)
            {
                log.ErrorFormat("GetLicensedLink: failed to get adapter response from WS API. groupId = {0}, adapterId = {1}", fileStreamingCompanyId, groupId);
                adapterResponse.Status.Code = (int)eResponseStatus.Error;
                adapterResponse.Status.Message = "Error";
                return adapterResponse;
            }
            if (adapterResponse.Status.Code != (int)eResponseStatus.OK)
            {
                log.DebugFormat("GetLicensedLink: got error adapter response from WS API. groupId = {0}, adapterId = {1}, status.code = {2}, status.message = {3}",
                    fileStreamingCompanyId, groupId, adapterResponse.Status.Code, adapterResponse.Status.Message);
            }

            return adapterResponse;
        }

        internal static bool IsValidRecordingStatus(TstvRecordingStatus recordStatus, List<TstvRecordingStatus> RecordingStatus)
        {
            if (RecordingStatus.Contains(recordStatus))
            {
                return true;
            }
            return false;
        }

        internal static Dictionary<long, Recording> GetDomainProtectedRecordings(int groupID, long domainID)
        {
            Dictionary<long, Recording> domainProtectedRecordings = null;
            DataTable dt = RecordingsDAL.GetDomainProtectedRecordings(groupID, domainID, TVinciShared.DateUtils.UnixTimeStampNow());
            if (dt != null && dt.Rows != null)
            {
                domainProtectedRecordings = new Dictionary<long, Recording>();
                foreach (DataRow dr in dt.Rows)
                {
                    long recordingID = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                    if (recordingID > 0)
                    {
                        Recording protectedRecording = new Recording()
                        {
                            Id = recordingID,
                            EpgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_ID"),
                            EpgStartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "START_DATE"),
                            EpgEndDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "END_DATE"),
                            Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString())
                        };
                        if (!domainProtectedRecordings.ContainsKey(recordingID))
                        {
                            domainProtectedRecordings.Add(recordingID, protectedRecording);
                        }
                    }
                }
            }

            return domainProtectedRecordings;
        }

        internal static Dictionary<long, Recording> GetDomainRecordingIdsToRecordingsMap(int groupID, long domainID, List<long> domainRecordingIds)
        {
            Dictionary<long, Recording> DomainRecordingIdToRecordingMap = null;
            DataTable dt = RecordingsDAL.GetDomainRecordingsByIds(groupID, domainID, domainRecordingIds);
            if (dt != null && dt.Rows != null)
            {
                DomainRecordingIdToRecordingMap = new Dictionary<long, Recording>();
                foreach (DataRow dr in dt.Rows)
                {
                    long domainRecordingID = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                    if (domainRecordingID > 0)
                    {
                        Recording domainRecording = BuildDomainRecordingFromDataRow(dr);
                        // add domain recording if its valid and doesn't already exist in dictionary
                        if (domainRecording != null && domainRecording.Status != null && domainRecording.Status.Code == (int)eResponseStatus.OK
                            && !DomainRecordingIdToRecordingMap.ContainsKey(domainRecordingID))
                        {
                            DomainRecordingIdToRecordingMap.Add(domainRecordingID, domainRecording);
                        }
                    }
                }
            }

            return DomainRecordingIdToRecordingMap;
        }

        internal static Dictionary<long, Recording> GetDomainRecordingsByTstvRecordingStatuses(int groupID, long domainID, List<ApiObjects.TstvRecordingStatus> recordingStatuses, bool withUser = false)
        {
            Dictionary<long, Recording> DomainRecordingIdToRecordingMap = null;
            List<DomainRecordingStatus> domainRecordingStatuses = ConvertToDomainRecordingStatus(recordingStatuses);
            DataTable dt = RecordingsDAL.GetDomainRecordingsByRecordingStatuses(groupID, domainID, domainRecordingStatuses.Select(x => (int)x).ToList());
            if (dt != null && dt.Rows != null)
            {
                DomainRecordingIdToRecordingMap = new Dictionary<long, Recording>();
                foreach (DataRow dr in dt.Rows)
                {
                    long domainRecordingID = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                    if (domainRecordingID > 0)
                    {
                        Recording domainRecording = BuildDomainRecordingFromDataRow(dr);
                        // add domain recording if its valid and doesn't already exist in dictionary
                        if (domainRecording != null && domainRecording.Status != null && domainRecording.Status.Code == (int)eResponseStatus.OK
                            && !DomainRecordingIdToRecordingMap.ContainsKey(domainRecordingID) && recordingStatuses.Contains(domainRecording.RecordingStatus))
                        {
                            if (withUser)
                            {
                                domainRecording = new UserRecording(domainRecording, ODBCWrapper.Utils.GetSafeStr(dr, "USER_ID"));
                            }
                            DomainRecordingIdToRecordingMap.Add(domainRecordingID, domainRecording);
                        }
                    }
                }
            }

            return DomainRecordingIdToRecordingMap;
        }

        internal static Recording ValidateRecordID(int groupID, long domainID, long domainRecordingID)
        {
            Recording recording = new Recording()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.RecordingNotFound, eResponseStatus.RecordingNotFound.ToString())
            };

            try
            {
                Dictionary<long, Recording> DomainRecordingIdToRecordingMap = Utils.GetDomainRecordingIdsToRecordingsMap(groupID, domainID, new List<long>() { domainRecordingID });
                if (DomainRecordingIdToRecordingMap == null || DomainRecordingIdToRecordingMap.Count == 0 ||
                    !DomainRecordingIdToRecordingMap.ContainsKey(domainRecordingID) || DomainRecordingIdToRecordingMap[domainRecordingID].RecordingStatus == TstvRecordingStatus.Deleted)
                {
                    log.DebugFormat("No valid recording was returned from Utils.GetDomainRecordingIdsToRecordingsMap");
                    recording.Status = new ApiObjects.Response.Status((int)eResponseStatus.RecordingNotFound, eResponseStatus.RecordingNotFound.ToString());
                    recording.Id = domainRecordingID;
                    return recording;
                }

                recording = DomainRecordingIdToRecordingMap[domainRecordingID];
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at ValidateRecordID. ");
                sb.Append(String.Concat("domainID: ", domainID));
                sb.Append(String.Concat(", domainRecordingID: ", domainRecordingID));
                sb.Append(String.Concat("Ex Msg: ", ex.Message));
                sb.Append(String.Concat(", Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(", Stack Trace: ", ex.StackTrace));

                log.Error(sb.ToString(), ex);

                recording.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "failed ValidateRecordID");
            }

            return recording;
        }

        internal static SeriesRecording ValidateSeriesRecordID(int groupId, long domainId, long domainSeriesRecordingId)
        {
            SeriesRecording seriesRecording = new SeriesRecording()
               {
                   Status = new ApiObjects.Response.Status((int)eResponseStatus.SeriesRecordingNotFound, eResponseStatus.SeriesRecordingNotFound.ToString())
               };

            try
            {
                DataSet ds = RecordingsDAL.GetDomainSeriesRecordingsById(groupId, domainId, domainSeriesRecordingId);
                var seriesRecordings = BuildSeriesRecordingDetails(ds);
                if (seriesRecordings != null && seriesRecordings.Count > 0)
                {
                    seriesRecording = seriesRecordings[0];
                }
                if (seriesRecording == null)
                {
                    log.DebugFormat("No valid series recording was returned from Utils.GetDomainSeriesRecording");
                    seriesRecording = new SeriesRecording()
                    {
                        Status = new ApiObjects.Response.Status((int)eResponseStatus.SeriesRecordingNotFound, eResponseStatus.SeriesRecordingNotFound.ToString()),
                        Id = domainSeriesRecordingId
                    };
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at ValidateSeriesRecordID. ");
                sb.Append(String.Concat("domainID: ", domainId));
                sb.Append(String.Concat(", domainSeriesRecordingId: ", domainSeriesRecordingId));
                sb.Append(String.Concat("Ex Msg: ", ex.Message));
                sb.Append(String.Concat(", Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(", Stack Trace: ", ex.StackTrace));

                log.Error(sb.ToString(), ex);
                seriesRecording = new SeriesRecording()
                {
                    Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "failed Validate series recordID"),
                    Id = domainSeriesRecordingId
                };
            }
            return seriesRecording;
        }

        public static SeriesRecording BuildSeriesRecordingDetails(DataRow dr)
        {
            long epgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_ID");
            long epgChannelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_CHANNEL_ID");
            long domainSeriesRecordingId = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
            int seasonNumber = ODBCWrapper.Utils.GetIntSafeVal(dr, "SEASON_NUMBER");
            string seriesId = ODBCWrapper.Utils.GetSafeStr(dr, "SERIES_ID");
            DateTime createDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE");
            DateTime updateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "UPDATE_DATE");

            return new SeriesRecording()
            {
                EpgChannelId = epgChannelId,
                EpgId = epgId,
                Id = domainSeriesRecordingId,
                SeasonNumber = seasonNumber,
                SeriesId = seriesId,
                Type = seasonNumber > 0 ? RecordingType.Season : RecordingType.Series,
                CreateDate = createDate,
                UpdateDate = updateDate,
                Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString())
            };

        }

        public static List<SeriesRecording> BuildSeriesRecordingDetails(DataSet ds)
        {
            Dictionary<long, SeriesRecording> result = new Dictionary<long, SeriesRecording>(); 
            SeriesRecording seriesRecording = null;

            if (ds != null && ds.Tables != null && ds.Tables.Count >= 2)
            {
                if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        seriesRecording = new SeriesRecording()
                        {
                            EpgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_ID"),
                            EpgChannelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_CHANNEL_ID"),
                            Id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID"),
                            SeasonNumber = ODBCWrapper.Utils.GetIntSafeVal(dr, "SEASON_NUMBER"),
                            SeriesId = ODBCWrapper.Utils.GetSafeStr(dr, "SERIES_ID"),
                            CreateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE"),
                            UpdateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "UPDATE_DATE"),
                            ExcludedSeasons = new List<int>(),
                            Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString())
                        };
                        seriesRecording.Type = seriesRecording.SeasonNumber > 0 ? RecordingType.Season : RecordingType.Series;
                        result.Add(seriesRecording.Id, seriesRecording);
                    }
                }
                if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                {
                    long seriesRecordingId = 0;
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        seriesRecordingId = ODBCWrapper.Utils.GetIntSafeVal(dr, "DOMAIN_SERIES_ID");
                        if (result.ContainsKey(seriesRecordingId))
                        {
                            result[seriesRecordingId].ExcludedSeasons.Add(ODBCWrapper.Utils.GetIntSafeVal(dr, "SEASON_NUMBER"));
                        }
                    }
                }
            }

            return result.Values.ToList();
        }

        internal static Recording BuildDomainRecordingFromDataRow(DataRow dr)
        {
            Recording recording = new Recording();
            long recordingID = ODBCWrapper.Utils.GetLongSafeVal(dr, "RECORDING_ID");
            if (recordingID > 0)
            {
                long epgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_ID");
                long epgChannelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_CHANNEL_ID");
                DateTime createDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE");
                DateTime updateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "UPDATE_DATE");
                long viewableUntilEpoch = ODBCWrapper.Utils.GetLongSafeVal(dr, "VIEWABLE_UNTIL_EPOCH", 0);
                long protectedUntilDate = ODBCWrapper.Utils.GetLongSafeVal(dr, "PROTECTED_UNTIL_EPOCH", 0);
                RecordingInternalStatus recordingInternalStatus = (RecordingInternalStatus)ODBCWrapper.Utils.GetIntSafeVal(dr, "RECORDING_STATUS");
                DomainRecordingStatus domainRecordingStatus = (DomainRecordingStatus)ODBCWrapper.Utils.GetIntSafeVal(dr, "DOMAIN_RECORDING_STATUS");
                TstvRecordingStatus? recordingStatus = ConvertToTstvRecordingStatus(domainRecordingStatus);
                DateTime epgStartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "START_DATE");
                DateTime epgEndDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "END_DATE");
                string externalRecordingId = ODBCWrapper.Utils.GetSafeStr(dr, "EXTERNAL_RECORDING_ID");
                string crid = ODBCWrapper.Utils.GetSafeStr(dr, "CRID");
                RecordingType recordingType = (RecordingType)ODBCWrapper.Utils.GetIntSafeVal(dr, "RECORDING_TYPE");
                string userId = ODBCWrapper.Utils.GetSafeStr(dr, "USER_ID");

                if (!recordingStatus.HasValue)
                {
                    log.ErrorFormat("Failed Convert DomainRecordingStatus: {0} to TstvRecordingStatus for recordingID: {1}, epgID: {2}",
                                     domainRecordingStatus, recordingID, epgId);
                    return recording;
                }
                // if the domain recording status was 1 now recordingStatus is OK and we need to get recordingStatus from recordings and not domains table
                else if (recordingStatus.Value == TstvRecordingStatus.OK)
                {
                    recordingStatus = ConvertToTstvRecordingStatus(recordingInternalStatus, epgStartDate, epgEndDate);
                    if (!recordingStatus.HasValue)
                    {
                        log.ErrorFormat("Failed Convert RecordingInternalStatus: {0} to TstvRecordingStatus for recordingID: {1}, epgID: {2}",
                                         recordingInternalStatus, recordingID, epgId);
                        return recording;
                    }

                    // if internal recording status was 0 now recordingStatus is OK and we need to set recording status according to RecordingsManager
                    if (recordingStatus.Value == TstvRecordingStatus.OK)
                    {
                        recordingStatus = RecordingsManager.GetTstvRecordingStatus(epgStartDate, epgEndDate, TstvRecordingStatus.Scheduled);
                    }
                }

                // create recording object
                recording = new Recording()
                {
                    Id = recordingID,
                    EpgId = epgId,
                    ChannelId = epgChannelId,
                    EpgStartDate = epgStartDate,
                    EpgEndDate = epgEndDate,
                    CreateDate = createDate,
                    UpdateDate = updateDate,
                    RecordingStatus = recordingStatus.Value,
                    ExternalRecordingId = externalRecordingId,
                    Crid = crid,
                    Type = recordingType,

                };

                // if recording status is Recorded then set ViewableUntilDate
                if (recording.RecordingStatus == TstvRecordingStatus.Recorded)
                {
                    recording.ViewableUntilDate = viewableUntilEpoch;

                    // if recording is/was protected then set ProtectedUntilDate
                    if (protectedUntilDate > 0)
                    {
                        recording.ProtectedUntilDate = protectedUntilDate;
                        // update viewableUntilDate incase protectedUntilDate is bigger than viewableUntilDate
                        if (recording.ProtectedUntilDate.Value > recording.ViewableUntilDate.Value)
                        {
                            recording.ViewableUntilDate = recording.ProtectedUntilDate;
                        }
                    }

                    long currentUtcEpoch = TVinciShared.DateUtils.UnixTimeStampNow();
                    // modify recordings status to Deleted if it's currently not viewable
                    if (recording.ViewableUntilDate.Value < currentUtcEpoch)
                    {
                        /***** Currently the LifeTimePeriodExpired status is only for backend inner needs and we are not exposing it on the REST to the client *****
                        recording.RecordingStatus = TstvRecordingStatus.LifeTimePeriodExpired;*/
                        recording.RecordingStatus = TstvRecordingStatus.Deleted;
                    }
                }

                // if we got until here then recording.Status is OK
                recording.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return recording;
        }

        internal static Dictionary<long, HandleDomainQuataByRecordingTask> UpdateAndGetExpiredRecordingsTasks(long unixTimeStampNow)
        {
            Dictionary<long, HandleDomainQuataByRecordingTask> expiredRecordings = new Dictionary<long, HandleDomainQuataByRecordingTask>();
            DataTable dt = RecordingsDAL.UpdateAndGetExpiredRecordingsTasks(unixTimeStampNow);
            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                    if (id > 0 && !expiredRecordings.ContainsKey(id))
                    {
                        long recordingId = ODBCWrapper.Utils.GetLongSafeVal(dr, "RECORDING_ID", 0);
                        int groupId = ODBCWrapper.Utils.GetIntSafeVal(dr, "GROUP_ID", 0);                        
                        long scheduledExpirationEpoch = ODBCWrapper.Utils.GetLongSafeVal(dr, "scheduled_expiration_epoch", 0);                        
                        HandleDomainQuataByRecordingTask expiredRecording = new HandleDomainQuataByRecordingTask()
                        {
                            Id = id,
                            RecordingId = recordingId,
                            ScheduledExpirationEpoch = scheduledExpirationEpoch,
                            GroupId = groupId                            
                        };
                        expiredRecordings.Add(id, expiredRecording);
                    }
                }
            }

            return expiredRecordings;
        }

        internal static int GetDomainDefaultQuota(int groupId, long domainId)
        {
            int domainDefaultQuota = 0;
            try
            {
                string key = UtilsDal.GetDefaultQuotaInSeconds(groupId, domainId);
                bool res = ConditionalAccessCache.GetItem<int>(key, out domainDefaultQuota);
                if (!res || domainDefaultQuota == 0)
                {
                    domainDefaultQuota = ConditionalAccessDAL.GetDefaultQuotaInSeconds(groupId);
                    res = ConditionalAccessCache.AddItem(key, domainDefaultQuota);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("failed to get default quota in seconds for domainID = {0}, groupID = {1} , ex = {2}", domainId, groupId, ex.Message);
                domainDefaultQuota = 0;
            }

            return domainDefaultQuota;
        }

        internal static List<ExtendedSearchResult> GetFirstFollowerEpgIdsToRecord(int groupId, string epgChannelId, string seriesId, int seasonNumber, DateTime? windowStartDate)
        {
            List<ExtendedSearchResult> programs = null;

            try
            {
                StringBuilder ksql = new StringBuilder();
                ksql.AppendFormat("(and series_id = '{0}' ", seriesId);

                if (seasonNumber > 0)
                    ksql.AppendFormat("season_number = '{0}'", seasonNumber);

                if (windowStartDate.HasValue)
                {
                    ksql.AppendFormat("start_date > '{0}'", TVinciShared.DateUtils.DateTimeToUnixTimestamp(windowStartDate.Value));
                }

                ksql.AppendFormat("epg_channel_id = '{0}')", epgChannelId);

                ExtendedSearchRequest request = new ExtendedSearchRequest()
                {
                    m_nGroupID = groupId,
                    m_dServerTime = DateTime.UtcNow,
                    m_nPageIndex = 0,
                    m_nPageSize = 0,
                    assetTypes = new List<int> { 0 },
                    filterQuery = ksql.ToString(),
                    order = new ApiObjects.SearchObjects.OrderObj()
                    {
                        m_eOrderBy = ApiObjects.SearchObjects.OrderBy.START_DATE,
                        m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC
                    },
                    m_oFilter = new Filter()
                    {
                        m_bOnlyActiveMedia = true
                    },
                    ExtraReturnFields = new List<string> { "epg_channel_id", "crid" },
                };
                FillCatalogSignature(request);

                UnifiedSearchResponse response = request.GetResponse(request) as UnifiedSearchResponse;

                if (response == null || response.status == null)
                {
                    log.ErrorFormat("Got empty response from Catalog 'GetResponse' for 'ExtendedSearchRequest'");
                    return programs;
                }
                if (response.status.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Got error response from catalog 'GetResponse' for 'ExtendedSearchRequest'. response: code = {0}, message = {1}", response.status.Code, response.status.Message);
                    return programs;
                }

                programs = response.searchResults.Select(sr => (ExtendedSearchResult)sr).ToList();
            }

            catch 
            {
                log.ErrorFormat("Failed GetFirstFollowerEpgIdsToRecord, channelId: {0}, seriesId: {1}, seassonNumber: {2}, windowStartDate: {3}", epgChannelId, seriesId, seasonNumber, windowStartDate);
            }

            return programs;
        }

        internal static Dictionary<long, Recording> GetEpgToRecordingsMapByCridAndChannel(int groupId, string crid, long channelId, long epgId)
        {
            Dictionary<long, Recording> epgToRecordingMap = new Dictionary<long, Recording>();
            DataTable dt = RecordingsDAL.GetEpgToRecordingsMapByCridChannelAndEpgId(groupId, crid, channelId, epgId);
            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    Recording recording = BuildRecordingFromDataRow(dr);
                    // add recording if its valid and the epg doesn't already exist in dictionary
                    if (recording != null && recording.Status != null && recording.Status.Code == (int)eResponseStatus.OK && !epgToRecordingMap.ContainsKey(recording.EpgId))
                    {
                        epgToRecordingMap.Add(recording.EpgId, recording);
                    }
                }
            }

            return epgToRecordingMap;
        }

        internal static Recording BuildRecordingFromDataRow(DataRow dr)
        {
            Recording recording = new Recording();
            long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
            if (id > 0)
            {
                long epgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_PROGRAM_ID");
                long epgChannelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_CHANNEL_ID");
                RecordingInternalStatus recordingInternalStatus = (RecordingInternalStatus)ODBCWrapper.Utils.GetIntSafeVal(dr, "RECORDING_STATUS", 0);
                DateTime epgStartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "START_DATE");
                DateTime epgEndDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "END_DATE");
                DateTime createDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE");
                DateTime updateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "UPDATE_DATE");
                long? viewableUntilDate = ODBCWrapper.Utils.GetLongSafeVal(dr, "VIEWABLE_UNTIL_EPOCH", 0);
                string externalRecordingId = ODBCWrapper.Utils.GetSafeStr(dr, "EXTERNAL_RECORDING_ID");
                string crid = ODBCWrapper.Utils.GetSafeStr(dr, "CRID");
                int getStatusRetries = ODBCWrapper.Utils.GetIntSafeVal(dr, "GET_STATUS_RETRIES", 0);

                if (recordingInternalStatus < 0)
                {
                    log.ErrorFormat("Failed getting recordingInternalStatus for recording with id: {0}", id);
                    return recording;
                }

                TstvRecordingStatus? recordingStatus = ConvertToTstvRecordingStatus(recordingInternalStatus, epgStartDate, epgEndDate);
                if (!recordingStatus.HasValue)
                {
                    log.ErrorFormat("Failed Convert RecordingInternalStatus: {0} to TstvRecordingStatus for recordingID: {1}", recordingInternalStatus, id);
                    return recording;
                }

                // create recording object
                recording = new Recording()
                {
                    Id = id,
                    EpgId = epgId,
                    ChannelId = epgChannelId,
                    EpgStartDate = epgStartDate,
                    EpgEndDate = epgEndDate,
                    CreateDate = createDate,
                    UpdateDate = updateDate,
                    RecordingStatus = recordingStatus.Value,
                    ExternalRecordingId = externalRecordingId,
                    Crid = crid,
                    GetStatusRetries = getStatusRetries
                };

                // if recording status is Recorded then set ViewableUntilDate
                if (recording.RecordingStatus == TstvRecordingStatus.Recorded)
                {
                    recording.ViewableUntilDate = viewableUntilDate > 0 ? viewableUntilDate : null;
                }

                // if we got until here then recording.Status is OK
                recording.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return recording;
        }

        internal static SeriesRecording FollowSeasonOrSeries(int groupId, string userId, long domainID, long epgId, RecordingType recordingType, ref bool isSeriesFollowed, ref List<long> futureSeriesRecordingIds, EPGChannelProgrammeObject epg = null)
        {
            SeriesRecording seriesRecording = new SeriesRecording();
            if (epg == null)
            {
                List<EPGChannelProgrammeObject> epgs = Utils.GetEpgsByIds(groupId, new List<long>() { epgId });
                if (epgs == null || epgs.Count != 1)
                {
                    log.DebugFormat("Failed Getting EPG from Catalog, DomainID: {0}, UserID: {1}, EpgId: {2}", domainID, userId, epgId);
                    return seriesRecording;
                }
                else
                {
                    epg = epgs[0];
                }
            }
            
            Dictionary<string, string> epgFieldMappings = GetEpgFieldTypeEntitys(groupId, epg, recordingType);
            if (epgFieldMappings == null || epgFieldMappings.Count == 0)
            {
                log.ErrorFormat("failed GetEpgFieldTypeEntitys, groupId: {0}, epgId: {1}, recordingType: {2}", groupId, epg.EPG_ID, recordingType.ToString());
                return seriesRecording;
            }

            string seriesId = epgFieldMappings[SERIES_ID];
            int seasonNumber = 0, episodeNumber = 0;

            if (recordingType == RecordingType.Season && (!epgFieldMappings.ContainsKey(SEASON_NUMBER) || !int.TryParse(epgFieldMappings[SEASON_NUMBER], out seasonNumber)))
            {
                log.ErrorFormat("failed parsing SEASON_NUMBER, groupId: {0}, epgId: {1}, recordingType: {2}", groupId, epg.EPG_ID, recordingType.ToString());
                return seriesRecording;
            }

            // currently we don't care about episode number so no log + error if we can't parse it
            if (epgFieldMappings.ContainsKey(EPISODE_NUMBER))
            {
                int.TryParse(epgFieldMappings[EPISODE_NUMBER], out episodeNumber);
            }

            long channelId;
            if (!long.TryParse(epg.EPG_CHANNEL_ID, out channelId))
            {
                log.ErrorFormat("Error on FollowSeasonOrSeries while trying to parse epgChannelId: {0}", epg.EPG_CHANNEL_ID);
                return seriesRecording;
            }

            isSeriesFollowed = RecordingsDAL.IsSeriesFollowed(groupId, seriesId, seasonNumber, channelId);
            // insert or update domain_series table
            DataTable dt = RecordingsDAL.FollowSeries(groupId, userId, domainID, epgId, channelId, seriesId, seasonNumber, episodeNumber);
            if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
            {
                seriesRecording = BuildSeriesRecordingDetails(dt.Rows[0]);
            }

            // check if the user has future single episodes of the series/season and return them so we will cancel them and they will be recorded as part of series/season
            DomainSeriesRecording domainSeriesRecording = (DomainSeriesRecording)seriesRecording;
            List<ExtendedSearchResult> futureRecordingsOfSeasonOrSeries = Utils.SearchSeriesRecordings(groupId, new List<string>(), new List<DomainSeriesRecording>() { domainSeriesRecording }, SearchSeriesRecordingsTimeOptions.future);
            if (futureRecordingsOfSeasonOrSeries != null)
            {
                foreach (ExtendedSearchResult futureRecordingSearchResult in futureRecordingsOfSeasonOrSeries)
                {
                    long recordingId;
                    if (long.TryParse(futureRecordingSearchResult.AssetId, out recordingId))
                    {
                        futureSeriesRecordingIds.Add(recordingId);
                    }
                }
            }

            return seriesRecording;
        }

        internal static Dictionary<string, string> GetEpgFieldTypeEntitys(int groupId, EPGChannelProgrammeObject epg, RecordingType recordingType)
        {
            Dictionary<string, string> epgFieldMappings = new Dictionary<string, string>();
            try
            {
                List<ApiObjects.Epg.FieldTypeEntity> metaTagsMappings = Tvinci.Core.DAL.CatalogDAL.GetAliasMappingFields(groupId);
                if (metaTagsMappings == null || metaTagsMappings.Count == 0)
                {
                    log.DebugFormat("No alias mapping returned from 'GetAliasMappingFields'. groupId = {0} ", groupId);
                    return epgFieldMappings;
                }

                ApiObjects.Epg.FieldTypeEntity field = metaTagsMappings.Where(m => m.Alias.ToLower() == SERIES_ALIAS).FirstOrDefault();
                if (field == null)
                {
                    log.DebugFormat("alias for series_id was not found. group_id = {0}", groupId);
                    return epgFieldMappings;
                }
                else if (field.FieldType == FieldTypes.Meta)
                {
                    if (epg.EPG_Meta != null && epg.EPG_Meta.Count > 0)
                    {
                        epgFieldMappings.Add(SERIES_ID, epg.EPG_Meta.Where(x => x.Key == field.Name).First().Value);
                    }
                    else
                    {
                        log.DebugFormat("alias for series_id was not found - no metas on epg. group_id = {0}", groupId);
                        return epgFieldMappings;
                    }
                }
                else if (field.FieldType == FieldTypes.Tag)
                {
                    if (epg.EPG_TAGS != null && epg.EPG_TAGS.Count > 0)
                    {
                        epgFieldMappings.Add(SERIES_ID, epg.EPG_TAGS.Where(x => x.Key == field.Name).First().Value);
                    }
                    else
                    {
                        log.DebugFormat("alias for series_id was not found - no tags on epg. group_id = {0}", groupId);
                        return epgFieldMappings;
                    }
                }

                field = metaTagsMappings.Where(m => m.Alias.ToLower() == SEASON_ALIAS).FirstOrDefault();
                if (recordingType == RecordingType.Season && field == null)
                {
                    log.DebugFormat("alias for season_number was not found. group_id = {0}", groupId);
                    return epgFieldMappings;
                }
                else if (field != null)
                {
                    if (field.FieldType == FieldTypes.Meta && epg.EPG_Meta != null && epg.EPG_Meta.Count > 0)
                    {
                        epgFieldMappings.Add(SEASON_NUMBER, epg.EPG_Meta.Where(x => x.Key == field.Name).First().Value);
                    }
                    else if (field.FieldType == FieldTypes.Tag && epg.EPG_TAGS != null && epg.EPG_TAGS.Count > 0)
                    {
                        epgFieldMappings.Add(SEASON_NUMBER, epg.EPG_TAGS.Where(x => x.Key == field.Name).First().Value);
                    }
                }

                field = metaTagsMappings.Where(m => m.Alias.ToLower() == EPISODE_ALIAS).FirstOrDefault();
                if (field != null)
                {
                    if (field.FieldType == FieldTypes.Meta && epg.EPG_Meta != null && epg.EPG_Meta.Count > 0)
                    {
                        epgFieldMappings.Add(EPISODE_NUMBER, epg.EPG_Meta.Where(x => x.Key == field.Name).First().Value);
                    }
                    else if (field.FieldType == FieldTypes.Tag && epg.EPG_TAGS != null && epg.EPG_TAGS.Count > 0)
                    {
                        epgFieldMappings.Add(EPISODE_NUMBER, epg.EPG_TAGS.Where(x => x.Key == field.Name).First().Value);
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error("Failed GetEpgFieldTypeEntitys", ex);
            }

            return epgFieldMappings;
        }

        internal static List<ExtendedSearchResult> SearchSeriesRecordings(int groupID, List<string> excludedCrids, List<DomainSeriesRecording> series, SearchSeriesRecordingsTimeOptions SearchSeriesRecordingsTimeOption)
        {
            List<ExtendedSearchResult> recordings = null;

            // build the KSQL for the series
            string seriesId;
            string seasonNumber;
            string episodeNumber;

            if (!GetSeriesMetaTagsFieldsNamesForSearch(groupID, out seriesId, out seasonNumber, out episodeNumber))
            {
                log.ErrorFormat("failed to 'GetSeriesMetaTagsNamesForGroup' for groupId = {0} ", groupID);
                return recordings;
            }

            // build the filter query for the search
            StringBuilder ksql = new StringBuilder("(and (or ");
            StringBuilder seasonsToExclude = null;
            string season = null;
            foreach (var serie in series)
            {
                season = (serie.SeasonNumber > 0 && !string.IsNullOrEmpty(seasonNumber)) ? string.Format("{0} = '{1}' ", seasonNumber, serie.SeasonNumber) : string.Empty;
                seasonsToExclude = new StringBuilder();
                if (serie.ExcludedSeasons != null && serie.ExcludedSeasons.Count > 0)
                {                    
                    foreach (int seasonNumberToExclude in serie.ExcludedSeasons)
                    {
                        seasonsToExclude.AppendFormat("{0} != '{1}' ", seasonNumber, seasonNumberToExclude); 
                    }
                }

                ksql.AppendFormat("(and {0} = '{1}' epg_channel_id = '{2}' {3} {4})", seriesId, serie.SeriesId, serie.EpgChannelId, season, seasonsToExclude.ToString());

            }

            switch (SearchSeriesRecordingsTimeOption)
            {
                case SearchSeriesRecordingsTimeOptions.past:
                    ksql.AppendFormat(") start_date < '0')");
                    break;
                case SearchSeriesRecordingsTimeOptions.future:
                    ksql.AppendFormat(") start_date > '0')");
                    break;
                case SearchSeriesRecordingsTimeOptions.all:                  
                default:
                    ksql.AppendFormat("))");
                    break;
            }            


            // get program ids
            try
            {
                ExtendedSearchRequest request = new ExtendedSearchRequest()
                {
                    m_nGroupID = groupID,
                    m_dServerTime = DateTime.UtcNow,
                    m_nPageIndex = 0,
                    m_nPageSize = 0,
                    assetTypes = new List<int> { 1 },
                    filterQuery = ksql.ToString(),
                    order = new ApiObjects.SearchObjects.OrderObj()
                    {
                        m_eOrderBy = ApiObjects.SearchObjects.OrderBy.START_DATE,
                        m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC
                    },
                    m_oFilter = new Filter()
                    {
                        m_bOnlyActiveMedia = true
                    },
                    excludedCrids = excludedCrids != null ? excludedCrids : null,
                    ExtraReturnFields = new List<string> { "epg_id", "crid", "epg_channel_id", seriesId, seasonNumber },
                    ShouldUseSearchEndDate = true
                };
                FillCatalogSignature(request);
                string catalogUrl = GetWSURL("WS_Catalog");
                if (string.IsNullOrEmpty(catalogUrl))
                {
                    log.Error("Catalog Url is null or empty");
                    return recordings;
                }

                UnifiedSearchResponse response = request.GetResponse(request) as UnifiedSearchResponse;

                if (response == null || response.status == null)
                {
                    log.ErrorFormat("Got empty response from Catalog 'GetResponse' for 'ExtendedSearchRequest'");
                    return recordings;
                }
                if (response.status.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Got error response from catalog 'GetResponse' for 'ExtendedSearchRequest'. response: code = {0}, message = {1}", response.status.Code, response.status.Message);
                    return recordings;
                }

                recordings = response.searchResults.Select(sr => (ExtendedSearchResult)sr).ToList();
            }

            catch (Exception ex)
            {
                log.Error("SearchSeriesRecordings - Failed UnifiedSearchRequest Request To Catalog", ex);
            }

            return recordings;
        }        

        internal static bool GetSeriesMetaTagsFieldsNamesForSearch(int groupId, out string seriesIdName, out string seasonNumberName, out string episodeNumberName)
        {
            seriesIdName = seasonNumberName = episodeNumberName = string.Empty;

            // TODO: Add this alias stuff to cache!!!!
            var metaTagsMappings = Tvinci.Core.DAL.CatalogDAL.GetAliasMappingFields(groupId);
            if (metaTagsMappings == null || metaTagsMappings.Count == 0)
            {
                log.ErrorFormat("failed to 'GetAliasMappingFields' for seriesId. groupId = {0} ", groupId);
                return false;
            }

            var feild = metaTagsMappings.Where(m => m.Alias.ToLower() == "series_id").FirstOrDefault();
            if (feild == null)
            {
                log.ErrorFormat("alias for series_id was not found. group_id = {0}", groupId);
                return false;
            }

            seriesIdName = string.Format("{0}.{1}", feild.FieldType == FieldTypes.Meta ? "metas" : "tags", feild.Name);

            feild = metaTagsMappings.Where(m => m.Alias.ToLower() == "season_number").FirstOrDefault();
            if (feild != null)
            {
                seasonNumberName = string.Format("{0}.{1}", feild.FieldType == FieldTypes.Meta ? "metas" : "tags", feild.Name);
            }

            feild = metaTagsMappings.Where(m => m.Alias.ToLower() == "episode_number").FirstOrDefault();
            if (feild != null)
            {
                episodeNumberName = string.Format("{0}.{1}", feild.FieldType == FieldTypes.Meta ? "metas" : "tags", feild.Name);
            }

            return true;
        }

        internal static List<EpgCB> GetEpgRelatedToSeriesRecording(int groupId, SeriesRecording seriesRecording, List<EpgCB> epgs, long seasonNumber = 0)
        {
            List<EpgCB> epgMatch = new List<EpgCB>();
            try
            {   
                List<ApiObjects.Epg.FieldTypeEntity> metaTagsMappings = Tvinci.Core.DAL.CatalogDAL.GetAliasMappingFields(groupId);
                if (metaTagsMappings == null || metaTagsMappings.Count == 0)
                {
                    log.ErrorFormat("failed to 'GetAliasMappingFields' for seriesId. groupId = {0} ", groupId);
                    return new List<EpgCB>();
                }

                ApiObjects.Epg.FieldTypeEntity series_alias = metaTagsMappings.Where(m => m.Alias.ToLower() == SERIES_ALIAS).FirstOrDefault();
                if (series_alias == null)
                {
                    log.ErrorFormat("alias for series_id was not found. group_id = {0}", groupId);
                    return new List<EpgCB>();
                }

                if (series_alias.FieldType == FieldTypes.Meta)
                {
                    epgMatch = epgs.Where(x => x.Metas.Any(y => y.Key == series_alias.Name && y.Value.Contains(seriesRecording.SeriesId))).ToList();
                }
                else if (series_alias.FieldType == FieldTypes.Tag)
                {
                    epgMatch = epgs.Where(x => x.Tags.Any(y => y.Key == series_alias.Name && y.Value.Contains(seriesRecording.SeriesId))).ToList();
                }


                if (seriesRecording.SeasonNumber > 0 || seasonNumber > 0)
                {
                    long seasonNumberEqual = seriesRecording.SeasonNumber > 0 ? seriesRecording.SeasonNumber : seasonNumber;
                    ApiObjects.Epg.FieldTypeEntity season_alias = metaTagsMappings.Where(m => m.Alias.ToLower() == SEASON_ALIAS).FirstOrDefault();
                    if (season_alias == null)
                    {
                        log.ErrorFormat("alias for season_number was not found. group_id = {0}", groupId);
                        return new List<EpgCB>();
                    }

                    if (season_alias.FieldType == FieldTypes.Meta)
                    {
                        epgMatch = epgMatch.Where(x => x.Metas.Any(y => y.Key == season_alias.Name && y.Value.Contains(seasonNumberEqual.ToString()))).ToList();
                    }
                    else if (season_alias.FieldType == FieldTypes.Tag)
                    {
                        epgMatch = epgMatch.Where(x => x.Tags.Any(y => y.Key == season_alias.Name && y.Value.Contains(seasonNumberEqual.ToString()))).ToList();
                    }
                }
            }
            catch 
            {
                log.ErrorFormat("failed to 'GetEpgRelatedToSeriesRecording groupId = {0}, seriesRecordingID = {1}", groupId, seriesRecording.Id);
            }
            return epgMatch;            
        }

        internal static string GetFollowingUserIdForSerie(int groupId, List<DomainSeriesRecording> series, ExtendedSearchResult potentialRecording,
                                                            out RecordingType recordingType, out long domainSeriesRecordingId)
        {
            string userId = null;
            domainSeriesRecordingId = 0;
            recordingType = RecordingType.Series;

            string seriesIdName;
            string seasonNumberName;
            string episodeNumberName;

            GetSeriesMetaTagsFieldsNamesForSearch(groupId, out seriesIdName, out seasonNumberName, out episodeNumberName);

            if (potentialRecording != null && potentialRecording.ExtraFields != null)
            {
                string seriesId = null;
                int seasonNumber = 0;
                foreach (var field in potentialRecording.ExtraFields)
                {
                    if (field.key.ToLower() == seriesIdName.ToLower())
                    {
                        seriesId = field.value;
                    }

                    if (field.key.ToLower() == seasonNumberName.ToLower())
                    {
                        int.TryParse(field.value, out seasonNumber);
                    }
                }

                foreach (var serie in series)
                {
                    if (serie.SeriesId == seriesId && (serie.SeasonNumber == 0 || serie.SeasonNumber == seasonNumber))
                    {
                        userId = serie.UserId;
                        domainSeriesRecordingId = serie.Id;
                        if (serie.SeasonNumber == 0)
                            recordingType = RecordingType.Series;
                        else
                            recordingType = RecordingType.Season;

                        break;
                    }
                }
            }

            return userId;
        }

        internal static long GetLongParamFromExtendedSearchResult(ExtendedSearchResult extendedResult, string paramName)
        {
            long result = 0;

            if (extendedResult != null && extendedResult.ExtraFields != null)
            {
                var field = extendedResult.ExtraFields.Where(ef => ef.key == paramName).FirstOrDefault();
                if (field != null)
                {
                    long.TryParse(field.value, out result);
                }
            }
            return result;
        }

        internal static string GetStringParamFromExtendedSearchResult(ExtendedSearchResult extendedResult, string paramName)
        {
            string result = string.Empty;

            if (extendedResult != null && extendedResult.ExtraFields != null)
            {
                var field = extendedResult.ExtraFields.Where(ef => ef.key == paramName).FirstOrDefault();
                if (field != null)
                {
                    result = field.value;
                }
            }
            return result;
        }

        internal static bool UpdateRecording(Recording recording, int groupId, int rowStatus, int isActive, RecordingInternalStatus? status)
        {
            TimeShiftedTvPartnerSettings accountSettings = GetTimeShiftedTvPartnerSettings(groupId);
            int? recordingLifetime = accountSettings.RecordingLifetimePeriod;
            DateTime? viewableUntilDate = null;
            if (recordingLifetime.HasValue)
            {
                viewableUntilDate = recording.EpgEndDate.AddDays(recordingLifetime.Value);
                recording.ViewableUntilDate = TVinciShared.DateUtils.DateTimeToUnixTimestamp(viewableUntilDate.Value);
            }
            return RecordingsDAL.UpdateRecording(recording, groupId, rowStatus, isActive, status, viewableUntilDate);
        }

        internal static List<Recording> GetRecordings(int groupId, List<long> recordingIds)
        {
            List<Recording> recordings = new List<Recording>();
            DataSet dataSet = RecordingsDAL.GetRecordings(groupId, recordingIds);
            if (dataSet != null && dataSet.Tables != null)
            {
                recordings = BuildRecordingsFromDataSet(dataSet);
            }

            return recordings;
        }

        internal static List<Recording> BuildRecordingsFromDataSet(DataSet dataSet)
        {
            List<Recording> recordings = new List<Recording>();

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0 &&
                dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    Recording recording = BuildRecordingFromDataRow(row);

                    recordings.Add(recording);
                }
            }

            return recordings;
        }

        internal static Recording GetRecordingByEpgId(int groupId, long epgId)
        {
            Recording recording = null;
            DataTable dt = RecordingsDAL.GetRecordingByEpgId(groupId, epgId);
            if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
            {
                recording = BuildRecordingFromDataRow(dt.Rows[0]);
            }

            return recording;
        }

        internal static Recording InsertRecording(Recording recording, int groupId, RecordingInternalStatus? status)
        {
            Recording insertedRecording = null;
            TimeShiftedTvPartnerSettings accountSettings = GetTimeShiftedTvPartnerSettings(groupId);
            int? recordingLifetime = accountSettings.RecordingLifetimePeriod;
            DateTime? viewableUntilDate = null;
            if (recordingLifetime.HasValue)
            {
                viewableUntilDate = recording.EpgEndDate.AddDays(recordingLifetime.Value);
                recording.ViewableUntilDate = TVinciShared.DateUtils.DateTimeToUnixTimestamp(viewableUntilDate.Value);
            }

            DataTable dt = RecordingsDAL.InsertRecording(recording, groupId, status, viewableUntilDate);
            if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
            {
                insertedRecording = BuildRecordingFromDataRow(dt.Rows[0]);
            }

            return insertedRecording;
        }

        internal static Recording GetRecordingById(long id, bool takeOnlyValidRecording = true)
        {
            Recording recording = null;
            DataTable dt = RecordingsDAL.GetRecordingById(id, takeOnlyValidRecording);
            if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
            {
                recording = BuildRecordingFromDataRow(dt.Rows[0]);
            }

            return recording;

        }

        internal static List<Recording> GetAllRecordingsByStatuses(int groupId, List<int> statuses)
        {
            List<Recording> recordings = new List<Recording>();
            DataSet dataSet = RecordingsDAL.GetAllRecordingsByStatuses(groupId, statuses);
            if (dataSet != null && dataSet.Tables != null)
            {
                recordings = BuildRecordingsFromDataSet(dataSet);
            }

            return recordings;
        }

        internal static List<DomainSeriesRecording> GetDomainSeriesRecordingFromDataSet(DataSet serieDataSet)
        {
            Dictionary<long, DomainSeriesRecording> result = new Dictionary<long, DomainSeriesRecording>();

            if (serieDataSet != null && serieDataSet.Tables != null && serieDataSet.Tables.Count >= 2)
            {
                if (serieDataSet.Tables[0] != null && serieDataSet.Tables[0].Rows != null && serieDataSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in serieDataSet.Tables[0].Rows)
                    {

                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                        result.Add(id, new DomainSeriesRecording()
                        {
                            Id = id,
                            EpgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_ID", 0),
                            SeasonNumber = ODBCWrapper.Utils.GetIntSafeVal(dr, "SEASON_NUMBER", 0),
                            SeriesId = ODBCWrapper.Utils.GetSafeStr(dr, "SERIES_ID"),
                            UserId = ODBCWrapper.Utils.GetSafeStr(dr, "USER_ID"),
                            EpgChannelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_CHANNEL_ID", 0),
                            ExcludedSeasons = new List<int>()
                        });
                    }
                }
                if (serieDataSet.Tables[1] != null && serieDataSet.Tables[1].Rows != null && serieDataSet.Tables[1].Rows.Count > 0)
                {
                    long domainSeriesId = 0;
                    foreach (DataRow dr in serieDataSet.Tables[1].Rows)
                    {
                        domainSeriesId = ODBCWrapper.Utils.GetIntSafeVal(dr, "DOMAIN_SERIES_ID", 0);
                        if (result.ContainsKey(domainSeriesId))
                        {
                            result[domainSeriesId].ExcludedSeasons.Add(ODBCWrapper.Utils.GetIntSafeVal(dr, "SEASON_NUMBER", 0));
                        }
                    }
                }
            }

            return result.Values.ToList();
        }

        internal static MediaObj GetMediaById(int groupID, int mediaId)
        {
            MediaObj media = null;

            try
            {
                MediasProtocolRequest request = new MediasProtocolRequest();
                request.m_nGroupID = groupID;
                request.m_nPageIndex = 0;
                request.m_nPageSize = 0;
                request.m_lMediasIds = new List<int> { mediaId };
                request.m_oFilter = new Filter()
                {
                    m_bOnlyActiveMedia = true
                };
                FillCatalogSignature(request);

                MediaResponse response = request.GetMediasByIDs(request) as MediaResponse;
                if (response != null && response.m_lObj != null && response.m_lObj.Count > 0)
                {
                    media = response.m_lObj[0] as MediaObj;
                }
            }

            catch (Exception ex)
            {
                log.Error("Failed GetMediasByIDs to Catalog", ex);
            }

            return media;
        }

        internal static bool GetRecordingPlaybackSettingsByLinearMediaIdFromCache(int groupId, int mediaId)
        {            
            string key = LayeredCacheKeys.GetRecordingPlaybackSettingsKey(groupId, mediaId);
            bool? enableRecordingPlaybackNonEntitledChannel = null;
            bool res = false;
            try
            {
                bool cacheResult = LayeredCache.Instance.Get<bool?>(key, ref enableRecordingPlaybackNonEntitledChannel, GetRecordingPlaybackSettingsByLinearMediaId, new Dictionary<string, object>()
                { { "groupId", groupId }, { "mediaId", mediaId } }, groupId, LayeredCacheConfigNames.GET_RECORDING_PLAYBACK_SETTINGS_LAYERED_CACHE_CONFIG_NAME);

                if (cacheResult && enableRecordingPlaybackNonEntitledChannel.HasValue)
                {
                    res = enableRecordingPlaybackNonEntitledChannel.Value;
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("failed GetRecordingPlaybackSettingsByLinearMediaIdFromCache, groupId: {0}, mediaId: {1}", groupId, mediaId), ex);
            }

            return res;
        }

        internal static Tuple<bool?, bool> GetRecordingPlaybackSettingsByLinearMediaId(Dictionary<string, object> funcParams)
        {
            bool? enableRecordingPlaybackNonEntitledChannel = null;
            bool res = false;

            try
            {
                if (funcParams != null && funcParams.Count == 2 && funcParams.ContainsKey("mediaId") && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    int? mediaId = funcParams["mediaId"] as int?;
                    if (groupId.HasValue && mediaId.HasValue)
                    {
                        MediaObj media = GetMediaById(groupId.Value, mediaId.Value);
                        if (media != null && !string.IsNullOrEmpty(media.AssetId))
                        {
                            enableRecordingPlaybackNonEntitledChannel = media.EnableRecordingPlaybackNonEntitledChannel;
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetRecordingPlaybackSettingsByLinearMediaId failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<bool?, bool>(enableRecordingPlaybackNonEntitledChannel, res);
        }

        internal static bool IsDeviceInDomain(Domain domain, string udid)
        {
            if (domain != null && domain.m_deviceFamilies != null && domain.m_deviceFamilies.Count > 0)
            {
                foreach (var deviceFamily in domain.m_deviceFamilies)
                {
                    if (deviceFamily.DeviceInstances != null && deviceFamily.DeviceInstances.Count > 0 && deviceFamily.DeviceInstances.Where(d => d.m_deviceUDID == udid).FirstOrDefault() != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }        

        internal static bool IsFollowingSeries(int groupId, long domainID, string seriesId, int seasonNumber, long channelId)
        {
            long domainSeriesId = RecordingsDAL.GetDomainSeriesId(groupId, domainID, seriesId, seasonNumber, channelId);
            return domainSeriesId > 0;
        }

        internal static Dictionary<long, Recording> GetFutureDomainRecordingsByRecordingIDs(int groupID, long domainID, List<long> recordingIds, RecordingType recordingType)
        {
            Dictionary<long, Recording> DomainRecordingIdToRecordingMap = null;
            DataTable dt = RecordingsDAL.GetFutureDomainRecordingsByRecordingIDs(groupID, domainID, recordingIds, recordingType);
            if (dt != null && dt.Rows != null)
            {
                DomainRecordingIdToRecordingMap = new Dictionary<long, Recording>();
                foreach (DataRow dr in dt.Rows)
                {
                    long domainRecordingID = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                    if (domainRecordingID > 0)
                    {
                        Recording domainRecording = BuildDomainRecordingFromDataRow(dr);
                        // add domain recording if its valid and doesn't already exist in dictionary
                        if (domainRecording != null && domainRecording.Status != null && domainRecording.Status.Code == (int)eResponseStatus.OK && !DomainRecordingIdToRecordingMap.ContainsKey(domainRecordingID))
                        {
                            DomainRecordingIdToRecordingMap.Add(domainRecordingID, domainRecording);
                        }
                    }
                }
            }

            return DomainRecordingIdToRecordingMap;
        }

        internal static ApiObjects.Response.Status IsFollowingEpgAsSeriesOrSeason(int groupId, EPGChannelProgrammeObject epg, long domainId, RecordingType recordingType)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Dictionary<string, string> epgFieldMappings = Utils.GetEpgFieldTypeEntitys(groupId, epg, RecordingType.Single);
            if (epgFieldMappings == null || epgFieldMappings.Count == 0)
            {
                log.DebugFormat("no epgFieldMappings found, groupId: {0}, epgId: {1}", groupId, epg.EPG_ID);
                // if no mapping found we assume the user is not following the epg's season/series
                response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                return response;
            }
            else
            {
                string seriesId = epgFieldMappings[Utils.SERIES_ID];
                int seasonNumber = 0;
                long channelId = 0;
                if ((epgFieldMappings.ContainsKey(Utils.SEASON_NUMBER) && !int.TryParse(epgFieldMappings[Utils.SEASON_NUMBER], out seasonNumber)) || !long.TryParse(epg.EPG_CHANNEL_ID, out channelId))
                {
                    log.ErrorFormat("failed parsing SEASON_NUMBER or EPG_CHANNEL_ID, groupId: {0}, epgId: {1}", groupId, epg.EPG_ID);
                    return response;
                }

                if (IsFollowingSeries(groupId, domainId, seriesId, recordingType == RecordingType.Series ? 0 : seasonNumber, channelId))
                {
                    log.DebugFormat("domain already follows the series, can't record as single, DomainID: {0}, seriesID: {1}", domainId, seriesId);
                    response = new ApiObjects.Response.Status((int)eResponseStatus.AlreadyRecordedAsSeriesOrSeason, eResponseStatus.AlreadyRecordedAsSeriesOrSeason.ToString());
                    return response;
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    return response;
                }
            }
        }

        internal static bool ShouldOrderByWithoutCatalg(ApiObjects.SearchObjects.OrderObj orderBy)
        {
            if (orderBy.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.CREATE_DATE
                || orderBy.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.ID
                || orderBy.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.START_DATE)
            {
                return true;
            }
            else
            {
                return false;
            }


        }

        internal static List<Recording> OrderRecordingWithoutCatalog(List<Recording> recordings, ApiObjects.SearchObjects.OrderObj orderBy, int pageIndex, int pageSize,
                                                                     ref int totalResults, bool shouldIgnorePaging = false)
        {
            List<Recording> orderedRecordings = new List<Recording>();
            switch (orderBy.m_eOrderBy)
	        {
		        case ApiObjects.SearchObjects.OrderBy.ID:
                    if (orderBy.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.DESC)
                    {
                        orderedRecordings = recordings.OrderByDescending(x => x.Id).ToList();
                    }
                    else
                    {
                        orderedRecordings = recordings.OrderBy(x => x.Id).ToList();
                    }
                    break;
                case ApiObjects.SearchObjects.OrderBy.START_DATE:
                    if (orderBy.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.DESC)
                    {
                        orderedRecordings = recordings.OrderByDescending(x => x.EpgStartDate).ToList();
                    }
                    else
                    {
                        orderedRecordings = recordings.OrderBy(x => x.EpgStartDate).ToList();
                    }
                    break;
                case ApiObjects.SearchObjects.OrderBy.CREATE_DATE:
                    if (orderBy.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.DESC)
                    {
                        orderedRecordings = recordings.OrderByDescending(x => x.CreateDate).ToList();
                    }
                    else
                    {
                        orderedRecordings = recordings.OrderBy(x => x.CreateDate).ToList();
                    }
                    break;
                default:
                    log.DebugFormat("Invalid orderBy type: {0} on OrderRecordingWithoutCatalog", orderBy.m_eOrderBy.ToString()); 
                    break;
	        }
            
            totalResults = orderedRecordings.Count;
            if (!shouldIgnorePaging)
            {
                int startIndexOnList = pageIndex * pageSize;
                int rangeToGetFromList = (startIndexOnList + pageSize) > totalResults ? (totalResults - startIndexOnList) > 0 ? (totalResults - startIndexOnList) : 0 : pageSize;
                if (rangeToGetFromList > 0)
                {
                    orderedRecordings = orderedRecordings.GetRange(startIndexOnList, rangeToGetFromList);
                }
                else
                {
                    orderedRecordings.Clear();
                }
            }

            return orderedRecordings;
        }

        internal static Dictionary<long, Recording> GetDomainRecordingIdToRecordingMapByEpgIds(int groupId, long domainId, List<long> epgIds)
        {
            Dictionary<long, Recording> domainIdToRecordingMap = new Dictionary<long, Recording>();
            DataTable dt = RecordingsDAL.GetDomainExistingRecordingsByEpgIds(groupId, domainId, epgIds);
            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    long domainRecordingID = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                    if (domainRecordingID > 0 && !domainIdToRecordingMap.ContainsKey(domainRecordingID))
                    {
                        Recording domainRecording = BuildDomainRecordingFromDataRow(dr);                            
                        domainIdToRecordingMap.Add(domainRecordingID, domainRecording);                                                        
                    }                    
                }
            }

            return domainIdToRecordingMap;
        }

        internal static List<Recording> GetRecordingsByExternalRecordingId(int groupId, string externalRecordingId)
        {
            List<Recording> recordings = new List<Recording>();
            DataTable dt = RecordingsDAL.GetRecordingsByExternalRecordingId(groupId, externalRecordingId);
            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    Recording recording = BuildRecordingFromDataRow(dr);
                    // add recording if its valid
                    if (recording != null && recording.Status != null && recording.Status.Code == (int)eResponseStatus.OK)                        
                    {
                        recordings.Add(recording);
                    }
                }
            }

            return recordings;
        }

        internal static Dictionary<long, Recording> GetDomainRecordingsByDomainSeriesId(int groupID, long domainID, long domainSeriesRecordingId)
        {
            Dictionary<long, Recording> DomainRecordingIdToRecordingMap = null;
            DataTable dt = RecordingsDAL.GetDomainRecordingsByDomainSeriesId(groupID, domainID, domainSeriesRecordingId);
            if (dt != null && dt.Rows != null)
            {
                DomainRecordingIdToRecordingMap = new Dictionary<long, Recording>();
                foreach (DataRow dr in dt.Rows)
                {
                    long domainRecordingID = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                    if (domainRecordingID > 0)
                    {
                        Recording domainRecording = BuildDomainRecordingFromDataRow(dr);
                        // add domain recording if its valid and doesn't already exist in dictionary
                        if (domainRecording != null && domainRecording.Status != null && domainRecording.Status.Code == (int)eResponseStatus.OK && !DomainRecordingIdToRecordingMap.ContainsKey(domainRecordingID))
                        {
                            DomainRecordingIdToRecordingMap.Add(domainRecordingID, domainRecording);
                        }
                    }
                }
            }

            return DomainRecordingIdToRecordingMap;
        }

        internal static Dictionary<int, List<long>> GetFileIdsToEpgIdsMap(int groupId, Dictionary<long,string> epgToChannelMap)
        {
            Dictionary<int, List<long>> fileIdsToEpgMap = new Dictionary<int,List<long>>();
            HashSet<long> epgIdsToGetFromDb = new HashSet<long>();
            try
            {
                foreach (KeyValuePair<long, string> epgAndChannel in epgToChannelMap)
                {
                    List<int> channelFileIds = null;
                    string key = string.Format("Channel_{0}_FileIds", epgAndChannel.Value);
                    if (!TvinciCache.WSCache.Instance.TryGet(key, out channelFileIds))
                    {
                        lock (lck)
                        {
                            if (!TvinciCache.WSCache.Instance.TryGet(key, out channelFileIds))
                            {
                                log.DebugFormat("Getting Epg {0} file ids from DB", epgAndChannel.Key);
                                if (!epgIdsToGetFromDb.Contains(epgAndChannel.Key))
                                {
                                    epgIdsToGetFromDb.Add(epgAndChannel.Key);
                                }                                
                            }
                        }
                    }
                    else if (channelFileIds == null)
                    {
                        log.ErrorFormat("Channel {0} FileIds list is null", epgAndChannel.Value);
                    }
                    else
                    {
                        foreach (int fileId in channelFileIds)
                        {
                            if (fileIdsToEpgMap.ContainsKey(fileId))
                            {
                                fileIdsToEpgMap[fileId].Add(epgAndChannel.Key);
                            }
                            else
                            {
                                fileIdsToEpgMap.Add(fileId, new List<long>() { epgAndChannel.Key });
                            }
                        }
                    }
                }

                if (epgIdsToGetFromDb.Count > 0)
                {
                    Dictionary<long, List<int>> epgsToFileIdsMap = ConditionalAccessDAL.GetEpgsToFileIdsMap(groupId, epgIdsToGetFromDb.ToList());
                    if (epgsToFileIdsMap != null)
                    {
                        foreach (KeyValuePair<long, List<int>> epgFileIdDetails in epgsToFileIdsMap)
                        {
                            long epgId = epgFileIdDetails.Key;
                            List<int> epgFileIds = epgFileIdDetails.Value;
                            foreach (int fileId in epgFileIds)
                            {
                                if (fileIdsToEpgMap.ContainsKey(fileId))
                                {
                                    fileIdsToEpgMap[fileId].Add(epgId);
                                }
                                else
                                {
                                    fileIdsToEpgMap.Add(fileId, new List<long>() { epgId });
                                }
                            }

                            string key = string.Format("Channel_{0}_FileIds", epgToChannelMap[epgId]);
                            List<int> channelFileIds = null;
                            if (!TvinciCache.WSCache.Instance.TryGet(key, out channelFileIds))
                            {                                
                                TvinciCache.WSCache.Instance.Add(key, epgFileIds, 10);
                            }
                        }
                    }
                }

                log.DebugFormat("current fileIds returned from GetFileIdsToEpgIdsMap are: {0}", string.Join(",", fileIdsToEpgMap.Keys));
            }

            catch (Exception ex)
            {
                log.Error("GetFileIdsToEpgIdsMap - " + string.Format("Error in GetFileIdsToEpgIdsMap: groupID = {0}, epgIds: {1], ex.Message: {2}, ex.StackTrace: {3}", groupId, string.Join(",", epgToChannelMap.Keys), ex.Message, ex.StackTrace), ex);
            }

            return fileIdsToEpgMap;
        }

        public static bool UpdateDomainSeriesRecordingsUserToMaster(int groupId, int domainId, string userId, string masterUserId)
        {
            bool result = false;
            // update domain series recordings to master user
            var domainSeriesDs = RecordingsDAL.GetDomainSeriesRecordings(groupId, domainId);
            var domainSeries = GetDomainSeriesRecordingFromDataSet(domainSeriesDs);

            if (domainSeries != null && domainSeries.Count > 0)
            {
                List<long> domainSeriesIdsToUpdate = domainSeries.Where(s => s.UserId == userId).Select(s => s.Id).ToList();
                if (domainSeriesIdsToUpdate != null)
                {
                    result = RecordingsDAL.UpdateDomainSeriesRecordingsUserId(groupId, domainSeriesIdsToUpdate, masterUserId);
                    if (!result)
                    {
                        log.ErrorFormat("Failed to update DomainSeriesRecordings to master user after deleting user = {0}, domainId = {1}", userId, domainId);
                    }
                    else
                    {
                        log.DebugFormat("Successfully updated DomainSeriesRecordings to master user after deleting user = {0}, domainId = {1}, masterUserId = {2}", userId, domainId, masterUserId);
                    }
                }
            }
            else
            {
                return true;
            }

            return result;
        }

        public static bool UpdateScheduledRecordingsUserToMaster(int groupId, int domainId, string userId, string masterUserId)
        {
            bool result = false;
            var domainScheduledRecordings = GetDomainRecordingsByTstvRecordingStatuses(groupId, domainId, new List<TstvRecordingStatus> { TstvRecordingStatus.Scheduled }, true);

            if (domainScheduledRecordings != null && domainScheduledRecordings.Count > 0)
            {
                List<long> domainSceduledIdsToUpdate = domainScheduledRecordings.Where(r => ((UserRecording)r.Value).UserId == userId).Select(r => r.Key).ToList();
                if (domainSceduledIdsToUpdate != null)
                {
                    result = RecordingsDAL.UpdateDomainScheduledRecordingsUserId(groupId, domainSceduledIdsToUpdate, masterUserId);
                    if (!result)
                    {
                        log.ErrorFormat("Failed to update domain scheduled recordings to master user after deleting user = {0}, domainId = {1}", userId, domainId);
                    }
                    else
                    {
                        log.DebugFormat("Successfully updated domain scheduled recordings to master user after deleting user = {0}, domainId = {1}, masterUserId = {2}", userId, domainId, masterUserId);
                    }
                }
            }
            else
            {
                return true;
            }

            return result;
        }

        internal static bool GetLinearMediaInfoByEpgChannelIdAndFileType(int groupId, string epgChannelId, string fileType, ref int linearMediaId, ref int mediaFileId)
        {
            bool res = false;
            DataTable dt = ApiDAL.GetLinearMediaInfoByEpgChannelIdAndFileType(groupId, epgChannelId, fileType);
            if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
            {
                DataRow dr = dt.Rows[0];
                linearMediaId = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_id", 0);
                mediaFileId = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_file_id", 0);
                res = true;
            }

            return res;
        }

        internal static bool InsertOrSetCachedEntitlementResults(long domainId, int mediaFileId, CachedEntitlementResults cachedEntitlementResults)
        {
            return ConditionalAccessDAL.InsertOrSetCachedEntitlementResults(TVinciShared.WS_Utils.GetTcmConfigValue("Version"), domainId, mediaFileId, cachedEntitlementResults);
        }

        internal static CachedEntitlementResults GetCachedEntitlementResults(long domainId, int mediaFileId)
        {
            return ConditionalAccessDAL.GetCachedEntitlementResults(TVinciShared.WS_Utils.GetTcmConfigValue("Version"), domainId, mediaFileId);
        }

        internal static ApiObjects.Response.Status SetResponseStatus(PriceReason priceReason)
        {
            ApiObjects.Response.Status status = null;
            switch (priceReason)
            {
                case PriceReason.PPVPurchased:
                status = new ApiObjects.Response.Status((int)eResponseStatus.UnableToPurchasePPVPurchased, "PPV already purchased");
                break;
                case PriceReason.Free:
                status = new ApiObjects.Response.Status((int)eResponseStatus.UnableToPurchaseFree, "Free");
                break;
                case PriceReason.ForPurchaseSubscriptionOnly:
                status = new ApiObjects.Response.Status((int)eResponseStatus.UnableToPurchaseForPurchaseSubscriptionOnly, "Subscription only");
                break;
                case PriceReason.SubscriptionPurchased:
                status = new ApiObjects.Response.Status((int)eResponseStatus.UnableToPurchaseSubscriptionPurchased, "Already purchased (subscription)");
                break;
                case PriceReason.NotForPurchase:
                status = new ApiObjects.Response.Status((int)eResponseStatus.NotForPurchase, "Not valid for purchase");
                break;
                case PriceReason.CollectionPurchased:
                status = new ApiObjects.Response.Status((int)eResponseStatus.UnableToPurchaseCollectionPurchased, "Collection already purchased");
                break;
                default:
                status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                break;
            }

            return status;
        }

        internal static ApiObjects.Response.Status SetResponseStatus(ResponseStatus userValidStatus)
        {
            ApiObjects.Response.Status status = null;
            // user validation failed
            switch (userValidStatus)
            {
                case ResponseStatus.UserDoesNotExist:
                status = new ApiObjects.Response.Status((int)eResponseStatus.UserDoesNotExist, "User doesn't exists");
                break;
                case ResponseStatus.UserSuspended:
                status = new ApiObjects.Response.Status((int)eResponseStatus.UserSuspended, "Suspended user");
                break;
                case ResponseStatus.UserNotIndDomain:
                status = new ApiObjects.Response.Status((int)eResponseStatus.UserNotInDomain, "User doesn't exist in household");
                break;
                default:
                status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed to validate user");
                break;
            }

            return status;
        }

        internal static List<MediaFile> FilterMediaFilesForAsset(int groupId, string assetId, eAssetTypes assetType, long mediaId, StreamerType? streamerType, string mediaProtocol, 
            PlayContextType context, List<long> fileIds, bool filterOnlyByIds = false)
        {
            List<MediaFile> files = null;

            List<MediaFile> allMediafiles = null;
            string key = LayeredCacheKeys.GetMediaFilesKey(mediaId, assetType.ToString());
            bool cacheResult = LayeredCache.Instance.Get<List<MediaFile>>(key, ref allMediafiles, GetMediaFiles, new Dictionary<string, object>() { { "mediaId", mediaId }, { "groupId", groupId },
                                                                        { "assetType", assetType } }, groupId, LayeredCacheConfigNames.MEDIA_FILES_LAYERED_CACHE_CONFIG_NAME);

            // filter
            if (allMediafiles != null && allMediafiles.Count > 0)
            {
                if (filterOnlyByIds)
                {
                    files = allMediafiles.Where(f => fileIds != null && fileIds.Contains(f.Id)).ToList();
                }
                else
                {
                    files = allMediafiles.Where(f => (!streamerType.HasValue || streamerType.Value == f.StreamerType) &&
                        ((context == PlayContextType.Trailer && f.IsTrailer) ||
                        ((context == PlayContextType.Playback || context == PlayContextType.CatchUp || context == PlayContextType.StartOver) && !f.IsTrailer)) &&
                        (string.IsNullOrEmpty(mediaProtocol) || string.IsNullOrEmpty(f.Url) || f.Url.ToLower().StartsWith(string.Format("{0}:", mediaProtocol.ToLower()))) &&
                        (fileIds == null || fileIds.Count == 0 || fileIds.Contains(f.Id))).ToList();
                }
            }

            return files;
        }

        internal static ApiObjects.Response.Status GetMediaIdForAsset(int groupId, string assetId, eAssetTypes assetType, string userId, Domain domain ,string udid, 
            out long mediaId, out Recording recording, out EPGChannelProgrammeObject program)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            mediaId = 0;
            recording = null;
            program = null;
            Tuple<long, Recording, EPGChannelProgrammeObject, ApiObjects.Response.Status> tupleResult = null;            
            string key = LayeredCacheKeys.GetMediaIdForAssetKey(assetId, assetType.ToString());

            try
            {
                bool cacheResult = LayeredCache.Instance.Get<Tuple<long, Recording, EPGChannelProgrammeObject, ApiObjects.Response.Status>>(key, ref tupleResult, GetMediaIdForAssetFromCache,
                new Dictionary<string, object>() { { "assetId", assetId }, { "groupId", groupId }, { "assetType", assetType }, { "userId", userId }, { "domain", domain }, { "udid", udid } },
                groupId, LayeredCacheConfigNames.MEDIA_IF_FOR_ASSET_LAYERED_CACHE_CONFIG_NAME);

                if (cacheResult && tupleResult != null)
                {
                    mediaId = tupleResult.Item1;
                    recording = tupleResult.Item2;
                    program = tupleResult.Item3;
                    status = tupleResult.Item4;
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("failed GetMediaIdForAsset, groupId: {0}, assetId: {1}, assetType: {2}", groupId, assetId, assetType.ToString()), ex);
            }

            return status;
        }

        private static Tuple<Tuple<long, Recording, EPGChannelProgrammeObject, ApiObjects.Response.Status>, bool> GetMediaIdForAssetFromCache(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Tuple<long, Recording, EPGChannelProgrammeObject, ApiObjects.Response.Status> tupleResults = null;
            long mediaId = 0;
            Recording recording = null;
            EPGChannelProgrammeObject program = null;
            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (funcParams != null && funcParams.Count == 6)
                {
                    if (funcParams.ContainsKey("assetId") && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("assetType") && funcParams.ContainsKey("userId"))
                    {
                        long id;
                        string assetId = funcParams["assetId"] as string;
                        int? groupId = funcParams["groupId"] as int?;
                        eAssetTypes? assetType = funcParams["assetType"] as eAssetTypes?;
                        string userId = funcParams["userId"] as string;
                        if (!string.IsNullOrEmpty(assetId) && long.TryParse(assetId, out id) && groupId.HasValue && assetType.HasValue && !string.IsNullOrEmpty(userId))
                        {
                            switch (assetType)
                            {
                                case eAssetTypes.NPVR:
                                    {
                                        Domain domain = funcParams.ContainsKey("domain") ? funcParams["domain"] as Domain : null;
                                        string udid = funcParams.ContainsKey("groupId") ? funcParams["udid"] as string : string.Empty;
                                        // check recording valid
                                        var recordingStatus = ValidateRecording(groupId.Value, domain, udid, userId, id, ref recording);

                                        if (recordingStatus.Code != (int)eResponseStatus.OK)
                                        {
                                            log.ErrorFormat("recording is not valid - recordingId = {0}", assetId);
                                            status = new ApiObjects.Response.Status(recordingStatus.Code, recordingStatus.Message);
                                        }

                                        List<EPGChannelProgrammeObject> epgs = Utils.GetEpgsByIds(groupId.Value, new List<long> { recording.EpgId });
                                        if (epgs != null && epgs.Count > 0)
                                        {
                                            program = epgs[0];
                                            mediaId = program.LINEAR_MEDIA_ID;
                                        }
                                        else
                                        {
                                            status = new ApiObjects.Response.Status((int)eResponseStatus.ProgramDoesntExist, "Program not found");
                                        }
                                    }
                                    break;
                                case eAssetTypes.EPG:
                                    {
                                        List<EPGChannelProgrammeObject> epgs = Utils.GetEpgsByIds(groupId.Value, new List<long> { id });
                                        if (epgs != null && epgs.Count > 0)
                                        {
                                            program = epgs[0];
                                            mediaId = program.LINEAR_MEDIA_ID;
                                        }
                                        else
                                        {
                                            status = new ApiObjects.Response.Status((int)eResponseStatus.ProgramDoesntExist, "Program not found");
                                        }
                                    }
                                    break;
                                case eAssetTypes.MEDIA:
                                    mediaId = id;
                                    break;
                                default:
                                    break;
                            }

                            tupleResults = new Tuple<long, Recording, EPGChannelProgrammeObject, ApiObjects.Response.Status>(mediaId, recording, program, status);
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetMediaIdForAssetFromCache failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Tuple<long, Recording, EPGChannelProgrammeObject, ApiObjects.Response.Status>, bool>(tupleResults, res);
        }

        internal static eService GetServiceByPlayContextType(PlayContextType contextType)
        {
            eService service;
            switch (contextType)
            {
                case PlayContextType.CatchUp:
                    service = eService.CatchUp;
                    break;
                case PlayContextType.StartOver:
                    service = eService.StartOver;
                    break;
                case PlayContextType.Trailer:
                case PlayContextType.Playback:
                default:
                    service = eService.Unknown;
                    break;
            }

            return service;
        }

        internal static eEPGFormatType GetEpgFormatTypeByPlayContextType(PlayContextType contextType)
        {
            eEPGFormatType type;

            switch (contextType)
            {
                case PlayContextType.CatchUp:
                    type = eEPGFormatType.Catchup;
                    break;
                case PlayContextType.StartOver:
                    type = eEPGFormatType.StartOver;
                    break;
                default:
                    throw new Exception("not supported context for EPG");
            }

            return type;
        }

        internal static ApiObjects.Response.Status ValidateRecording(int groupId, Domain domain, string udid, string userId, long domainRecordingId, ref Recording recording)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            // get device brand ID - and make sure the device is in the domain
            if (!Utils.IsDeviceInDomain(domain, udid))
            {
                log.ErrorFormat("Device not in the user's domain. groupId = {0}, userId = {1}, domainId = {2}, domainRecordingId = {3}, udid = {4}",
                    groupId, userId, domain.m_nDomainID, domainRecordingId, udid);
                response = new ApiObjects.Response.Status((int)eResponseStatus.DeviceNotInDomain, "Device not in the user's domain");
                return response;
            }

            // validate recording
            var domainRecordings = Utils.GetDomainRecordingIdsToRecordingsMap(groupId, domain.m_nDomainID, new List<long>() { domainRecordingId });
            if (domainRecordings == null || domainRecordings.Count == 0 || (recording = domainRecordings[domainRecordingId]) == null)
            {
                log.ErrorFormat("Recording does not exist. groupId = {0}, userId = {1}, domainId = {2}, domainRecordingId = {3}", groupId, userId, domain.m_nDomainID, domainRecordingId);
                response = new ApiObjects.Response.Status((int)eResponseStatus.RecordingNotFound, "Recording was not found");
                return response;
            }
            if (recording.RecordingStatus != TstvRecordingStatus.Recorded)
            {
                log.ErrorFormat("Recording status is not valid for playback. groupId = {0}, userId = {1}, domainId = {2}, domainRecordingId = {3}, recording = {4}, recordingStatus = {5}",
                    groupId, userId, domain.m_nDomainID, domainRecordingId, recording.Id, recording.RecordingStatus);
                response = new ApiObjects.Response.Status((int)eResponseStatus.RecordingStatusNotValid, "Recording status is not valid");
                return response;
            }

            return response;
        }

        public static string GetAssetUrl(int groupId, eAssetTypes assetType, string url, int cdnId)
        {
            // get adapter
            bool isDefaultAdapter = false;
            var adapterResponse = GetRelevantCDN(groupId, cdnId, assetType, ref isDefaultAdapter);

            url = string.Format("{0}{1}", adapterResponse.Adapter.BaseUrl, url);

            return url;
        }

        public static Dictionary<string, string> GetLicensedLinkParamsDict(string sSiteGuid, string mediaFileIDStr, string basicLink,
            string userIP, string countryCode, string langCode,
            string deviceName, string couponCode)
        {
            Dictionary<string, string> res = new Dictionary<string, string>(8);

            res.Add(CDNTokenizers.Constants.SITE_GUID, sSiteGuid);
            res.Add(CDNTokenizers.Constants.MEDIA_FILE_ID, mediaFileIDStr);
            res.Add(CDNTokenizers.Constants.URL, basicLink);
            res.Add(CDNTokenizers.Constants.IP, userIP);
            res.Add(CDNTokenizers.Constants.COUNTRY_CODE, countryCode);
            res.Add(CDNTokenizers.Constants.LANGUAGE_CODE, langCode);
            res.Add(CDNTokenizers.Constants.DEVICE_NAME, deviceName);
            res.Add(CDNTokenizers.Constants.COUPON_CODE, couponCode);

            return res;
        }

        public static bool IsItemPurchased(MediaFileItemPricesContainer price)
        {
            bool res = false;
            if (price == null || price.m_oItemPrices == null || price.m_oItemPrices.Length == 0)
            {
                return res;
            }
            PriceReason reason = price.m_oItemPrices[0].m_PriceReason;
            switch (reason)
            {
                case PriceReason.SubscriptionPurchased:
                case PriceReason.PrePaidPurchased:
                case PriceReason.CollectionPurchased:
                case PriceReason.PPVPurchased:
                    res = price.m_oItemPrices[0].m_oPrice.m_dPrice == 0d;
                    break;
                default:
                    break;

            }

            return res;
        }

        public static void GetDataFromCustomData(int customDataId, string customData, ref double customDataPrice, ref string customDataCurrency, ref string userIP, ref string coupon, ref string udid)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(customData);
                XmlNode theRequest = doc.FirstChild;

                customDataCurrency = XmlUtils.GetSafeValue(BaseConditionalAccess.CURRENCY, ref theRequest);
                userIP = XmlUtils.GetSafeValue(BaseConditionalAccess.USER_IP, ref theRequest);
                coupon = XmlUtils.GetSafeValue(BaseConditionalAccess.COUPON_CODE, ref theRequest);
                udid = XmlUtils.GetSafeValue(BaseConditionalAccess.DEVICE_NAME, ref theRequest);
                if (!Double.TryParse(XmlUtils.GetSafeValue(BaseConditionalAccess.PRICE, ref theRequest), out customDataPrice))
                {
                    customDataPrice = 0.0;
                }

            }
            catch (Exception exc)
            {
                log.ErrorFormat("SetEntitlement - error load custom data xml {0} Exception:{1}", customDataId, exc);
                throw exc;
            }
        }


        public static NPVRUserActionResponse HandleNPVRQuota(int groupId, Subscription subscription, long householdId, bool isCreate)
        {
            NPVRUserActionResponse userActionResponse = new NPVRUserActionResponse();
            NpvrServiceObject npvrObject = (NpvrServiceObject)subscription.m_lServices.Where(x => x.ID == (int)eService.NPVR).FirstOrDefault();
            log.DebugFormat("Subscription with NPVR service, Quota: {0}", npvrObject.Quota);

            INPVRProvider npvr;
            if (NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(groupId, out npvr))
            {
                try
                {
                    if (isCreate)
                    {
                        userActionResponse = npvr.CreateAccount(new NPVRParamsObj() { EntityID = householdId.ToString(), Quota = npvrObject.Quota });
                    }
                    else
                    {                       
                        // get current user quota                         
                        DomainQuotaResponse hhQuota = QuotaManager.Instance.GetDomainQuotaResponse(groupId, householdId);
                        if (hhQuota != null && hhQuota.Status.Code == (int)eResponseStatus.OK)
                        {
                            int usedQuota = hhQuota.TotalQuota - hhQuota.AvailableQuota; // get used quota
                            if (usedQuota > npvrObject.Quota)
                            {
                                // call the handel to delete all recordings
                                QuotaManager.Instance.HandleDominQuotaOvarge(groupId, householdId, (int)(usedQuota - npvrObject.Quota), DomainRecordingStatus.DeletePending);
                            }
                        }

                        if (!QuotaManager.Instance.SetDomainTotalQuota(groupId, householdId, npvrObject.Quota))
                        {
                            // what do do if it's fail ? ???? 
                        }

                        userActionResponse = npvr.UpdateAccount(new NPVRParamsObj() { EntityID = householdId.ToString(), Quota = npvrObject.Quota });
                    }
                }
                catch
                {

                }

            }

            return userActionResponse;
        }

        internal static ApiObjects.Country GetCountryByIp(int groupId, string ip)
        {
            ApiObjects.Country res = null;
            try
            {
                res = Core.Api.Module.GetCountryByIp(groupId, ip);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Utils.GetCountryByIp with groupId: {0}, ip: {1}", groupId, ip), ex);
            }

            return res;
        }

        internal static ApiObjects.Country GetCountryByCountryName(int groupId, string countryName)
        {
            ApiObjects.Country res = null;
            try
            {
                res = Core.Api.Module.GetCountryByCountryName(groupId, countryName);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Utils.GetCountryByCountryName with groupId: {0}, countryName: {1}", groupId, countryName), ex);
            }

            return res;
        }

        internal static string GetIP2CountryName(int groupId, string ip)
        {
            string res = string.Empty;
            try
            {
                ApiObjects.Country country = GetCountryByIp(groupId, ip);
                res = country != null ? country.Name : res;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Utils.GetIP2CountryName with groupId: {0}, ip: {1}", groupId, ip), ex);
            }

            return res;
        }

        internal static string GetIP2CountryCode(int groupId, string ip)
        {
            string res = string.Empty;
            try
            {
                ApiObjects.Country country = GetCountryByIp(groupId, ip);
                res = country != null ? country.Code : res;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Utils.GetIP2CountryCode with groupId: {0}, ip: {1}", groupId, ip), ex);
            }

            return res;
        }

        internal static int GetIP2CountryId(int groupId, string ip)
        {
            int res = 0;
            try
            {
                ApiObjects.Country country = GetCountryByIp(groupId, ip);
                res = country != null ? country.Id : res;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Utils.GetIP2CountryId with groupId: {0}, ip: {1}", groupId, ip), ex);
            }

            return res;
        }

        internal static string GetCountryCodeByCountryName(int groupId, string countryName)
        {
            string res = string.Empty;
            try
            {
                ApiObjects.Country country = GetCountryByCountryName(groupId, countryName);
                res = country != null ? country.Code : res;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Utils.GetCountryCodeByCountryName with groupId: {0}, countryName: {1}", groupId, countryName), ex);
            }

            return res;
        }

        internal static bool TryGetDomainEntitlementsFromCache(int groupId, int domainId, MeidaMaper[] mapper, ref DomainEntitlements domainEntitlements)
        {
            bool res = false;
            try
            {
                string key = LayeredCacheKeys.GetDomainEntitlementsKey(groupId, domainId);
                // if mapper is null init it to empty for passing validation in InitializeDomainEntitlements
                if (mapper == null)
                {
                    mapper = new MeidaMaper[0];
                }

                Dictionary<string, object> funcParams = new Dictionary<string, object>() { { "groupId", groupId }, { "domainId", domainId }, { "mapper", mapper } };
                res = LayeredCache.Instance.Get<DomainEntitlements>(key, ref domainEntitlements, InitializeDomainEntitlements, funcParams,
                                                                    groupId, LayeredCacheConfigNames.GET_DOMAIN_ENTITLEMENTS_LAYERED_CACHE_CONFIG_NAME, GetDomainEntitlementInvalidationKeys(domainId));

                if (res && domainEntitlements != null)
                {
                    // remove expired PPV's
                    if (domainEntitlements.DomainPpvEntitlements != null && domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary != null)
                    {
                        List<string> keysToRemove = new List<string>();
                        foreach (KeyValuePair<string, EntitlementObject> pair in domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary)
                        {
                            if (pair.Value.endDate.HasValue && pair.Value.endDate.Value <= DateTime.UtcNow)
                            {
                                keysToRemove.Add(pair.Key);
                            }
                        }

                        foreach (string keyToRemove in keysToRemove)
                        {
                            domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary.Remove(keyToRemove);
                        }
                    }

                    // Get mappings of mediaFileIDs - MediaIDs
                    if (mapper != null && mapper.Length > 0)
                    {
                        HashSet<int> mediaIdsToMap = new HashSet<int>();
                        Dictionary<string, List<string>> invalidationKeysMap = new Dictionary<string, List<string>>();
                        List<string> keys = new List<string>();
                        foreach (MeidaMaper mediaMapper in mapper)
                        {
                            if (!mediaIdsToMap.Contains(mediaMapper.m_nMediaID))
                            {
                                mediaIdsToMap.Add(mediaMapper.m_nMediaID);
                                invalidationKeysMap.Add(mediaMapper.m_nMediaID.ToString(), new List<string>() { LayeredCacheKeys.GetMediaInvalidationKey(groupId, mediaMapper.m_nMediaID) });
                                keys.Add(DAL.UtilsDal.MediaIdGroupFileTypesKey(mediaMapper.m_nMediaID));
                            }
                        }

                        Dictionary<string, Dictionary<string, int>> mediaIdGroupFileTypeMapper = null;                        
                        bool cacheResult = LayeredCache.Instance.GetValues<Dictionary<string, int>>(keys, ref mediaIdGroupFileTypeMapper, Get_AllMediaIdGroupFileTypesMappings,
                                                                                                    new Dictionary<string, object>() { { "mediaIds", mediaIdsToMap } },
                                                                                                    groupId, LayeredCacheConfigNames.GET_MEDIA_ID_GROUP_FILE_MAPPER_LAYERED_CACHE_CONFIG_NAME,
                                                                                                    invalidationKeysMap);
                        if (!cacheResult)
                        {
                            log.Error(string.Format("InitializeUsersEntitlements fail get mediaId group file types mappings from cache keys: {0}", string.Join(",", keys)));
                        }

                        Dictionary<string, int> mapping = new Dictionary<string, int>();

                        // combine all the results (all dictionaries that return to ONE dictionary)
                        foreach (Dictionary<string, int> val in mediaIdGroupFileTypeMapper.Values)
                        {
                            foreach (KeyValuePair<string, int> item in val)
                            {
                                if (!mapping.ContainsKey(item.Key))
                                {
                                    mapping.Add(item.Key, item.Value);
                                }
                            }
                        }

                        domainEntitlements.DomainPpvEntitlements.MediaIdGroupFileTypeMapper = mapping;
                    }

                    // remove expired Bundles
                    if (domainEntitlements.DomainBundleEntitlements != null)
                    {
                        // remove expired subscriptions
                        if (domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions != null)
                        {
                            List<string> keysToRemove = new List<string>();
                            foreach (KeyValuePair<string, UserBundlePurchase> pair in domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions)
                            {
                                if (pair.Value.dtEndDate != null && pair.Value.dtEndDate <= DateTime.UtcNow)
                                {
                                    keysToRemove.Add(pair.Key);
                                }
                            }

                            foreach (string keyToRemove in keysToRemove)
                            {
                                domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions.Remove(keyToRemove);
                            }
                        }

                        // remove expired collections
                        if (domainEntitlements.DomainBundleEntitlements.EntitledCollections != null)
                        {
                            List<string> keysToRemove = new List<string>();
                            foreach (KeyValuePair<string, UserBundlePurchase> pair in domainEntitlements.DomainBundleEntitlements.EntitledCollections)
                            {
                                if (pair.Value.dtEndDate != null && pair.Value.dtEndDate <= DateTime.UtcNow)
                                {
                                    keysToRemove.Add(pair.Key);
                                }
                            }

                            foreach (string keyToRemove in keysToRemove)
                            {
                                domainEntitlements.DomainBundleEntitlements.EntitledCollections.Remove(keyToRemove);
                            }
                        }

                        PopulateDomainBundles(domainId, groupId, domainEntitlements.DomainBundleEntitlements);
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetDomainEntitlementsFromCache, groupId: {0}, domainId: {1}", groupId, domainId), ex);
            }

            return res && domainEntitlements != null;           
        }

        private static List<string> GetDomainEntitlementInvalidationKeys(int domainId)
        {
            return new List<string>()
            {
                LayeredCacheKeys.GetCancelSubscriptionInvalidationKey(domainId),
                LayeredCacheKeys.GetCancelTransactionInvalidationKey(domainId),
                LayeredCacheKeys.GetPurchaseInvalidationKey(domainId),
                LayeredCacheKeys.GetGrantEntitlementInvalidationKey(domainId),
                LayeredCacheKeys.GetCancelServiceNowInvalidationKey(domainId),
                LayeredCacheKeys.GetRenewInvalidationKey(domainId)
            };
        }

        private static Tuple<DomainEntitlements, bool> InitializeDomainEntitlements(Dictionary<string, object> funcParams)
        {            
            DomainEntitlements domainEntitlements = null;
            try
            {
                if (funcParams != null && funcParams.Count == 3 && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("domainId") && funcParams.ContainsKey("mapper"))
                {
                    int? groupId = funcParams["groupId"] as int?, domainId = funcParams["domainId"] as int?;                    
                    MeidaMaper[] mapper = funcParams["mapper"] as MeidaMaper[];                    
                    if (groupId.HasValue && domainId.HasValue && mapper != null)
                    {
                        List<int> usersInDomain = Utils.GetAllUsersInDomain(groupId.Value, domainId.Value);
                        domainEntitlements = new DomainEntitlements();
                        //Get domain PPV entitlements
                        domainEntitlements.DomainPpvEntitlements = InitializeDomainPpvs(groupId.Value, domainId.Value, usersInDomain, mapper);
                        //Get domain bundle entitlements
                        domainEntitlements.DomainBundleEntitlements = InitializeDomainBundles(domainId.Value, groupId.Value, usersInDomain, false);
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("InitializeDomainEntitlements failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            bool res = domainEntitlements != null;

            return new Tuple<DomainEntitlements, bool>(domainEntitlements, res);
        }

        internal static Tuple<DataTable, bool> GetFileUrlLinks(Dictionary<string, object> funcParams)
        {
            bool res = false;
            DataTable dt = null;

            try
            {
                if (funcParams != null && funcParams.Count == 1)
                {
                    if (funcParams.ContainsKey("mediaFileId"))
                    {
                        int? mediaFileId;
                        mediaFileId = funcParams["mediaFileId"] as int?;

                        if (mediaFileId.HasValue)
                        {
                            dt = ConditionalAccessDAL.GetFileCdnData(mediaFileId.Value);
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetFileUrlLinks failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<DataTable, bool>(dt, res);
        }

        internal static Tuple<List<MediaFile>, bool> GetMediaFiles(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<MediaFile> mediaFiles = null;

            try
            {
                if (funcParams != null && funcParams.Count == 3)
                {
                    if (funcParams.ContainsKey("mediaId") && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("assetType"))
                    {
                        long? mediaId = funcParams["mediaId"] as long?;
                        int? groupId = funcParams["groupId"] as int?;
                        eAssetTypes? assetType = funcParams["assetType"] as eAssetTypes?;

                        if (mediaId.HasValue && groupId.HasValue && assetType.HasValue)
                        {
                            mediaFiles = ApiDAL.GetMediaFiles(mediaId.Value);
                            if (mediaFiles != null)
                            {
                                foreach (MediaFile mediaFile in mediaFiles)
                                {
                                    mediaFile.Url = GetAssetUrl(groupId.Value, assetType.Value, mediaFile.Url, mediaFile.CdnId);
                                }
                            }
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetMediaFiles failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<List<MediaFile>, bool>(mediaFiles, res);
        }

        internal static List<int> GetRelatedMediaFiles(ItemPriceContainer price, int mediaFileID)
        {
            List<int> lRelatedMediaFiles = new List<int>();

            if (price != null && price.m_lRelatedMediaFileIDs != null && price.m_lRelatedMediaFileIDs.Length > 0)
            {
                lRelatedMediaFiles.AddRange(price.m_lRelatedMediaFileIDs.ToList());
            }
            if (!lRelatedMediaFiles.Contains(mediaFileID))
            {
                lRelatedMediaFiles.Add(mediaFileID);
            }
            return lRelatedMediaFiles;
        }
        
        internal static DateTime? GetStartDate(ItemPriceContainer price)
        {
            DateTime? dtStartDate = null;

            if (price != null)
            {
                dtStartDate = price.m_dtStartDate;
            }

            return (dtStartDate);
        }

        internal static DateTime? GetEndDate(ItemPriceContainer price)
        {
            DateTime? dtEndDate = null;

            if (price != null)
            {
                dtEndDate = price.m_dtEndDate;
            }

            return (dtEndDate);
        }

        internal static void InsertOfflinePpvUse(int groupId, int mediaFileId, string productCode, string userId, string countryCode, string languageCode, string udid, int nRelPP, int releventCollectionID, ContextData context)
        {
            try
            {
                context.Load();
                // We write an empty string as the first parameter to split the start of the log from the offlinePpvUsesLog row data
                string infoToLog = string.Join(",", new object[] { " ", groupId, mediaFileId, productCode, userId, countryCode, languageCode, udid, nRelPP, releventCollectionID });
                offlinePpvLogger.Info(infoToLog);
            }
            catch (Exception ex)
            {
                log.Error(string.Format(@"Error in InsertOfflinePpvUse, groupId: {0}, mediaFileId: {1}, productCode: {2}, userId: {3}, countryCode: {4}, languageCode: {5}, udid: {6}, nRelPP: {7},
                                            releventCollectionID: {8}", groupId, mediaFileId, productCode, userId, countryCode, languageCode, udid, nRelPP, releventCollectionID), ex);
            }
        }

        internal static void InsertOfflineSubscriptionUse(int groupId, int mediaFileId, string productCode, string userId, string countryCode, string languageCode, string udid, int nRelPP, ContextData context)
        {
            try
            {
                context.Load();
                // We write an empty string as the first parameter to split the start of the log from the offlineSubscriptionUsesLog row data
                string infoToLog = string.Join(",", new object[] { " ", groupId, mediaFileId, productCode, userId, countryCode, languageCode, udid, nRelPP });
                offlineSubscriptionLogger.Info(infoToLog);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in InsertOfflinePpvUse, groupId: {0}, mediaFileId: {1}, productCode: {2}, userId: {3}, countryCode: {4}, languageCode: {5}, udid: {6}, nRelPP: {7}",
                                        groupId, mediaFileId, productCode, userId, countryCode, languageCode, udid, nRelPP), ex);
            }
        }

        private static Tuple<int, bool> Get_MediaFileIDByCoGuid(Dictionary<string, object> funcParams)
        {
            bool res = false;
            int mediaFileID = 0;
            Dictionary<string, int> result = new Dictionary<string, int>();
            try
            {
                int? groupID = 0;
                string coGuid = string.Empty;
                if (funcParams.ContainsKey("groupID"))
                {
                    groupID = funcParams["groupID"] as int?;
                }
                if (funcParams.ContainsKey("coGuid"))
                {
                    coGuid = funcParams["coGuid"] as string;
                }
                if (groupID > 0 && !string.IsNullOrEmpty(coGuid))
                {
                    res = ConditionalAccessDAL.Get_MediaFileIDByCoGuid(coGuid, groupID.Value, ref mediaFileID);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Get_MediaFileIDByCoGuid faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<int, bool>(mediaFileID, res);
        }

        private static Tuple<Dictionary<string, Dictionary<string, int>>, bool> Get_AllMediaIdGroupFileTypesMappings(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, Dictionary<string, int>> result = new Dictionary<string, Dictionary<string, int>>();
            try
            {
                if (funcParams.ContainsKey("mediaIds"))
                {
                    HashSet<int> mediaIds;
                    mediaIds = funcParams["mediaIds"] != null ? funcParams["mediaIds"] as HashSet<int> : null;
                    if (mediaIds != null)
                    {
                        result = ConditionalAccessDAL.Get_AllMediaIdGroupFileTypesMappings(mediaIds);
                        res = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Get_FileAndMediaBasicDetails faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, Dictionary<string, int>>, bool>(result, res);
        }

        internal static ApiObjects.Response.Status ValidatePPVModuleCode(int groupId, int productId, int contentId, ref PPVModule thePPVModule)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            try
            {
                long ppvModuleCode = 0;
                long.TryParse(productId.ToString(), out ppvModuleCode);

                thePPVModule = Core.Pricing.Module.ValidatePPVModuleForMediaFile(groupId, contentId, ppvModuleCode);

                if (thePPVModule == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.UnKnownPPVModule, "The ppv module is unknown");
                    return response;
                }

                if (!thePPVModule.m_sObjectCode.Equals(productId.ToString()))
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.UnKnownPPVModule, "This PPVModule does not belong to item");
                    return response;
                }

                response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                return response;
            }
            catch (Exception ex)
            {
                log.Error("ValidateModuleCode  ", ex);
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "error ValidateModuleCode");
                return response;
            }
        }

        internal static DateTime CalcSubscriptionEndDate(Subscription sub, bool bIsEntitledToPreviewModule, DateTime dtToInitializeWith)
        {
            DateTime res = dtToInitializeWith;
            if (sub != null)
            {
                if (bIsEntitledToPreviewModule && sub.m_oPreviewModule != null && sub.m_oPreviewModule.m_tsFullLifeCycle > 0)
                {
                    // calc end date according to preview module life cycle
                    res = Utils.GetEndDateTime(res, sub.m_oPreviewModule.m_tsFullLifeCycle);
                }
                else
                {
                    if (sub.m_oSubscriptionUsageModule != null)
                    {
                        // calc end date as before.
                        res = Utils.GetEndDateTime(res, sub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle);
                    }
                }
            }

            return res;
        }
        
        internal static void GetFreeItemLeftLifeCycle(int groupId, ref string p_strViewLifeCycle, ref string p_strFullLifeCycle)
        {
            // Default is 2 days
            TimeSpan ts = new TimeSpan(2, 0, 0, 0);

            // Get the group's configuration for free view life cycle
            string sFreeLeftView = Utils.GetValueFromConfig(string.Format("free_left_view_{0}", groupId));

            if (!string.IsNullOrEmpty(sFreeLeftView))
            {
                DateTime dEndDate = Utils.GetEndDateTime(DateTime.UtcNow, int.Parse(sFreeLeftView), true);
                ts = dEndDate.Subtract(DateTime.UtcNow);
            }

            p_strViewLifeCycle = ts.ToString();
            // TODO: Understand what to do with full life cycle of free item. Right now I write it the same as view
            p_strFullLifeCycle = ts.ToString();
        }

        public static bool IsFreeItem(MediaFileItemPricesContainer container)
        {
            return container != null && (container.m_oItemPrices == null || container.m_oItemPrices.Length == 0 || container.m_oItemPrices[0].m_PriceReason == PriceReason.Free);
        }
        
        internal static eTransactionType GetBusinessModuleType(string moduleCode)
        {
            if (!string.IsNullOrEmpty(moduleCode))
            {
                if (moduleCode.Contains("s:"))
                    return eTransactionType.Subscription;
                if (moduleCode.Contains("c:"))
                    return eTransactionType.Collection;
            }
            return eTransactionType.PPV;
        }

        internal static string GetDeviceName(string deviceUDID)
        {
            return DAL.ConditionalAccessDAL.GetDeviceName(deviceUDID);            
        }

        /// <summary>
        /// Get Billing Trans Method
        /// </summary>
        internal static ePaymentMethod GetBillingTransMethod(int billingTransID, string billingGuid)
        {
            ePaymentMethod retVal = ePaymentMethod.Unknown;

            if (billingTransID <= 0 && string.IsNullOrEmpty(billingGuid))
            {
                return retVal;
            }

            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += " select BILLING_METHOD from billing_transactions with (nolock) where status=1 and";
                if (billingTransID > 0)
                {
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", billingTransID);
                }
                else
                {
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_guid", "=", billingGuid);
                }
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        if (selectQuery.Table("query").DefaultView[0].Row["BILLING_METHOD"] != System.DBNull.Value && selectQuery.Table("query").DefaultView[0].Row["BILLING_METHOD"] != null)
                        {
                            int billingInt = int.Parse(selectQuery.Table("query").DefaultView[0].Row["BILLING_METHOD"].ToString());
                            if (billingInt > 0)
                            {
                                if (Enum.IsDefined(typeof(ePaymentMethod), ((ePaymentMethod)billingInt).ToString()))
                                    retVal = (ePaymentMethod)billingInt;
                            }
                        }
                    }
                    else if (count == 0)
                    {
                        retVal = ePaymentMethod.Gift;
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
            return retVal;
        }

        internal static bool IsGroupIDContainedInConfig(long lGroupID, string sKey, char cSeperator)
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

        internal static ApiObjects.Response.Status ConcurrencyResponseToResponseStatus(DomainResponseStatus mediaConcurrencyResponse)
        {
            ApiObjects.Response.Status res;

            switch (mediaConcurrencyResponse)
            {
                case DomainResponseStatus.LimitationPeriod:
                    res = new ApiObjects.Response.Status((int)eResponseStatus.LimitationPeriod, "Limitation period");
                    break;
                case DomainResponseStatus.Error:
                    res = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    break;
                case DomainResponseStatus.ExceededLimit:
                    res = new ApiObjects.Response.Status((int)eResponseStatus.ExceededLimit, "Exceeded limit");
                    break;
                case DomainResponseStatus.DeviceTypeNotAllowed:
                    res = new ApiObjects.Response.Status((int)eResponseStatus.DeviceTypeNotAllowed, "Device type not allowed");
                    break;
                case DomainResponseStatus.DeviceNotInDomain:
                    res = new ApiObjects.Response.Status((int)eResponseStatus.DeviceNotInDomain, "Device not in household");
                    break;
                case DomainResponseStatus.DeviceAlreadyExists:
                    res = new ApiObjects.Response.Status((int)eResponseStatus.DeviceAlreadyExists, "Device already exists");
                    break;
                case DomainResponseStatus.OK:
                    res = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    break;
                case DomainResponseStatus.DeviceExistsInOtherDomains:
                    res = new ApiObjects.Response.Status((int)eResponseStatus.DeviceExistsInOtherDomains, "Device exists in other household");
                    break;
                case DomainResponseStatus.ConcurrencyLimitation:
                    res = new ApiObjects.Response.Status((int)eResponseStatus.ConcurrencyLimitation, "Concurrency limitation");
                    break;
                case DomainResponseStatus.MediaConcurrencyLimitation:
                    res = new ApiObjects.Response.Status((int)eResponseStatus.MediaConcurrencyLimitation, "Media concurrency limitation");
                    break;
                default:
                    res = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    break;
            }

            return res;
        }

        public static Subscription GetSubscription(int groupId, int subscriptionId)
        {
            Subscription subscription = null;

            try
            {
                subscription = Core.Pricing.Module.GetSubscriptionData(groupId, subscriptionId.ToString(), string.Empty, string.Empty, string.Empty, false);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while trying to fetch subscription data. subscriptionId = {0}", subscriptionId), ex);
            }

            return subscription;
        }

        internal static bool GetGroupDefaultCurrency(int groupId, ref string currencyCode)
        {
            bool res = false;
            try
            {
                int defaultGroupCurrencyId = 0;
                if (LayeredCache.Instance.Get<int>(LayeredCacheKeys.GetGroupDefaultCurrencyKey(groupId), ref defaultGroupCurrencyId, GetGroupDefaultCurrency, new Dictionary<string, object>()
                                                    { { "groupId", groupId } }, groupId, LayeredCacheConfigNames.GET_DEFAULT_GROUP_CURRENCY_LAYERED_CACHE_CONFIG_NAME) && defaultGroupCurrencyId > 0)
                {
                    DataTable dt = null;
                    if (LayeredCache.Instance.Get<DataTable>(LayeredCacheKeys.GET_CURRENCIES_KEY, ref dt, GetAllCurrencies, new Dictionary<string, object>(), groupId,
                                                            LayeredCacheConfigNames.GET_CURRENCIES_LAYERED_CACHE_CONFIG_NAME) && dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        currencyCode = (from row in dt.AsEnumerable()
                                        where (Int64)row["ID"] == defaultGroupCurrencyId
                                        select row.Field<string>("CODE3")).FirstOrDefault();
                        res = !string.IsNullOrEmpty(currencyCode);
                    }
                }                

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGroupDefaultCurrency, groupId: {0}", groupId), ex);
            }

            return res;

        }

        internal static bool IsValidCurrencyCode(int groupId, string currencyCode3)
        {
            bool res = false;
            if (string.IsNullOrEmpty(currencyCode3))
            {
                return res;
            }

            try
            {
                DataTable dt = null;
                if (LayeredCache.Instance.Get<DataTable>(LayeredCacheKeys.GET_CURRENCIES_KEY, ref dt, GetAllCurrencies, new Dictionary<string, object>(), groupId,
                                                        LayeredCacheConfigNames.GET_CURRENCIES_LAYERED_CACHE_CONFIG_NAME) && dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = (from row in dt.AsEnumerable()
                           where ((string)row["CODE3"]).ToUpper() == currencyCode3.ToUpper()
                           select row).Count() > 0;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed IsValidCurrencyCode, groupId: {0}, currencyCode: {1}", groupId, currencyCode3), ex);
            }

            return res;
        }

        private static Tuple<DataTable, bool> GetAllCurrencies(Dictionary<string, object> funcParams)
        {
            bool res = false;
            DataTable dt = null;
            try
            {
                dt = ConditionalAccessDAL.GetAllCurrencies();
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetAllCurrencies failed, function parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            res = dt != null;
            return new Tuple<DataTable, bool>(dt, res);
        }

        private static Tuple<int, bool> GetGroupDefaultCurrency(Dictionary<string, object> funcParams)
        {
            bool res = false;
            int groupDefaultCurrencyId = 0;
            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue && groupId.Value > 0)
                    {
                        groupDefaultCurrencyId = ConditionalAccessDAL.GetGroupDefaultCurrency(groupId.Value);
                        res = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetGroupDefaultCurrency failed, function parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<int, bool>(groupDefaultCurrencyId, res);
        }
              
    }
}
