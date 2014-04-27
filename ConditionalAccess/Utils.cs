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

        private const string SUB_USES_TABLE = "subscriptions_uses";
        private const string COL_USES_TABLE = "collections_uses";


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

        static protected Int32 GetSubUseCount(string sSiteGUID, string subCode)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select count(*) as co from subscriptions_uses where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("is_Credit_downloaded", "=", 1);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", subCode);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected Int32 GetPPVPurchaseCount(Int32 nMediaFileID, string sSiteGUID, string subCode)
        {
            Int32 nRet = 0;
            if (!string.IsNullOrEmpty(subCode))
            {

            }

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select count(*) as co from ppv_purchases where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            if (!string.IsNullOrEmpty(subCode))
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", subCode);
            }
            else
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
            }

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
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

        static public bool ValidateBaseLink(Int32 nGroupID, Int32 nMediaFileID, string sBaseLink)
        {
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";
            TvinciAPI.API m = new ConditionalAccess.TvinciAPI.API();
            if (GetWSURL("api_ws") != "")
                m.Url = GetWSURL("api_ws");
            bool bRet = false;
            if (CachingManager.CachingManager.Exist("ValidateBaseLink" + nMediaFileID.ToString() + "_" + sBaseLink + "_" + nGroupID.ToString()) == true)
                bRet = (bool)(CachingManager.CachingManager.GetCachedData("ValidateBaseLink" + nMediaFileID.ToString() + "_" + sBaseLink + "_" + nGroupID.ToString()));
            else
            {
                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "ValidateBaseLink", "api", sIP, ref sWSUserName, ref sWSPass);
                bRet = m.ValidateBaseLink(sWSUserName, sWSPass, nMediaFileID, sBaseLink);
                CachingManager.CachingManager.SetCachedData("ValidateBaseLink" + nMediaFileID.ToString() + "_" + sBaseLink + "_" + nGroupID.ToString(), bRet, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }
            return bRet;
        }

        static public string GetHash(string sToHash, string sHashParameterName)
        {
            string sSecret = ODBCWrapper.Utils.GetTableSingleVal("tikle_group_parameters", sHashParameterName, 1, "BILLING_CONNECTION_STRING").ToString();
            sToHash += sSecret;

            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            md5Provider = new MD5CryptoServiceProvider();
            byte[] originalBytes = UTF8Encoding.Default.GetBytes(sToHash);
            byte[] encodedBytes = md5Provider.ComputeHash(originalBytes);
            return BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
        }

        static public TvinciAPI.MeidaMaper[] GetMediaMapper(Int32 nGroupID, Int32[] nMediaFilesIDs)
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
                m = new ConditionalAccess.TvinciAPI.API();
                if (GetWSURL("api_ws").Length > 0)
                    m.Url = GetWSURL("api_ws");
                string nMediaFilesIDsToCache = ConvertArrayIntToStr(nMediaFilesIDs);
                if (CachingManager.CachingManager.Exist("MapMediaFiles" + nMediaFilesIDsToCache + "_" + nGroupID.ToString()) == true)
                    mapper = (TvinciAPI.MeidaMaper[])(CachingManager.CachingManager.GetCachedData("MapMediaFiles" + nMediaFilesIDsToCache + "_" + nGroupID.ToString()));
                else
                {
                    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "MapMediaFiles", "api", sIP, ref sWSUserName, ref sWSPass);
                    mapper = m.MapMediaFiles(sWSUserName, sWSPass, nMediaFilesIDs);
                    CachingManager.CachingManager.SetCachedData("MapMediaFiles" + nMediaFilesIDsToCache + "_" + nGroupID.ToString(), mapper, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region Disposing
                if (m != null)
                {
                    m.Dispose();
                    m = null;
                }
                #endregion



            }
            return mapper;
        }

        static protected Int32 GetMediaFileTypeID(Int32 nGroupID, Int32 nMediaFileID)
        {
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";
            Int32 nRet = 0;
            using (TvinciAPI.API m = new ConditionalAccess.TvinciAPI.API())
            {
                string apiUrl = GetWSURL("api_ws");
                if (apiUrl.Length > 0)
                    m.Url = apiUrl;
                if (CachingManager.CachingManager.Exist("GetMediaFileTypeID" + nMediaFileID.ToString() + "_" + nGroupID.ToString()) == true)
                    nRet = (Int32)(CachingManager.CachingManager.GetCachedData("GetMediaFileTypeID" + nMediaFileID.ToString() + "_" + nGroupID.ToString()));
                else
                {
                    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetMediaFileTypeID", "api", sIP, ref sWSUserName, ref sWSPass);
                    nRet = m.GetMediaFileTypeID(sWSUserName, sWSPass, nMediaFileID);
                    CachingManager.CachingManager.SetCachedData("GetMediaFileTypeID" + nMediaFileID.ToString() + "_" + nGroupID.ToString(), nRet, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                }
            }

            return nRet;
        }

        /// <summary>
        /// PPV Does Credit Need To Downloaded
        /// </summary>
        static public bool PPV_DoesCreditNeedToDownloadedUsingCollection(int groupID, Int32 nMediaFileID, List<int> lUsersIds, string sCollectionCode)
        {
            bool nIsCreditDownloaded    = true;
            string sIP                  = "1.1.1.1";
            string sWSUserName          = "";
            string sWSPass              = "";

            using (TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule())
            {
                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (sWSURL.Length > 0)
                    m.Url = sWSURL;

                TvinciPricing.UsageModule u     = null;
                TvinciPricing.Collection theCol = null;

                if (CachingManager.CachingManager.Exist("GetCollectionData" + sCollectionCode + "_" + groupID.ToString()) == true)
                    theCol = (TvinciPricing.Collection)(CachingManager.CachingManager.GetCachedData("GetCollectionData" + sCollectionCode + "_" + groupID.ToString()));
                else
                {
                    TVinciShared.WS_Utils.GetWSUNPass(groupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                    theCol = m.GetCollectionData(sWSUserName, sWSPass, sCollectionCode, String.Empty, String.Empty, String.Empty, false);
                    CachingManager.CachingManager.SetCachedData("GetCollectionData" + sCollectionCode + "_" + groupID.ToString(), theCol, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                }

                u = theCol.m_oCollectionUsageModule;

                Int32 nViewLifeCycle = u.m_tsViewLifeCycle;

                int nCollectionID = 0;
                Int32.TryParse(sCollectionCode, out nCollectionID);
                DataTable dtPPVUses = DAL.ConditionalAccessDAL.Get_allDomainsPPVUsesUsingCollection(lUsersIds, groupID, nMediaFileID, nCollectionID);

                if (dtPPVUses != null)
                {
                    Int32 nCount = dtPPVUses.Rows.Count;
                    if (nCount > 0)
                    {
                        DateTime dNow = ODBCWrapper.Utils.GetDateSafeVal(dtPPVUses.Rows[0]["dNow"]);
                        DateTime dUsed = ODBCWrapper.Utils.GetDateSafeVal(dtPPVUses.Rows[0]["CREATE_DATE"]);

                        DateTime dEndDate = Utils.GetEndDateTime(dUsed, nViewLifeCycle);

                        if (dNow < dEndDate)
                            nIsCreditDownloaded = false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return nIsCreditDownloaded;
        }

        static public bool Bundle_DoesCreditNeedToDownloaded(string sBundleCd, string sSiteGUID, int mediaFileID, int groupID, eBundleType bundleType)
        {

            bool nIsCreditDownloaded = true;
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";

            using (TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule())
            {
                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (sWSURL.Length > 0)
                    m.Url = sWSURL;

                TvinciPricing.PPVModule theBundle = null;
                TvinciPricing.UsageModule u       = null;

                string sTableName = string.Empty;

                switch (bundleType)
                {
                    case eBundleType.SUBSCRIPTION:
                    {
                        TvinciPricing.Subscription theSub = null;

                        if (CachingManager.CachingManager.Exist("GetSubscriptionData" + sBundleCd + "_" + groupID.ToString()) == true)
                            theSub = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData("GetSubscriptionData" + sBundleCd + "_" + groupID.ToString()));
                        else
                        {
                            TVinciShared.WS_Utils.GetWSUNPass(groupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                            theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sBundleCd, String.Empty, String.Empty, String.Empty, false);
                            CachingManager.CachingManager.SetCachedData("GetSubscriptionData" + sBundleCd + "_" + groupID.ToString(), theSub, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                        }

                        u          = theSub.m_oSubscriptionUsageModule;
                        theBundle  = theSub;
                        sTableName = SUB_USES_TABLE;

                        break;
                    }
                    case eBundleType.COLLECTION:
                    {
                        TvinciPricing.Collection theCol = null;

                        if (CachingManager.CachingManager.Exist("GetCollectionData" + sBundleCd + "_" + groupID.ToString()) == true)
                            theCol = (TvinciPricing.Collection)(CachingManager.CachingManager.GetCachedData("GetCollectionData" + sBundleCd + "_" + groupID.ToString()));
                        else
                        {
                            TVinciShared.WS_Utils.GetWSUNPass(groupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                            theCol = m.GetCollectionData(sWSUserName, sWSPass, sBundleCd, String.Empty, String.Empty, String.Empty, false);
                            CachingManager.CachingManager.SetCachedData("GetCollectionData" + sBundleCd + "_" + groupID.ToString(), theCol, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                        }

                        u           = theCol.m_oCollectionUsageModule;
                        theBundle   = theCol;
                        sTableName  = COL_USES_TABLE;

                        break;
                    }
                }

                Int32 nViewLifeCycle = u.m_tsViewLifeCycle;

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select CREATE_DATE,getdate() as dNow from " + sTableName + " where ";
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(groupID, "MAIN_CONNECTION_STRING");
                selectQuery += " and ";
                if(bundleType == eBundleType.SUBSCRIPTION)
                {
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sBundleCd);
                }
                else if (bundleType == eBundleType.COLLECTION)
                {
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COLLECTION_CODE", "=", sBundleCd);
                }
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_CREDIT_DOWNLOADED", "=", 1);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", mediaFileID);
                selectQuery += " order by id desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        DateTime dNow = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["dNow"]);
                        DateTime dUsed = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["CREATE_DATE"]);
                        if ((dNow - dUsed).TotalMinutes < nViewLifeCycle)
                            nIsCreditDownloaded = false;
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }

            return nIsCreditDownloaded;
        }

        static protected TvinciPricing.PPVModule[] GetUserValidBundlesFromList(string sSiteGUID, int mediaID, int mediaFileID, int groupID, int[] nFileTypes, List<int> lUsersIds, eBundleType bundleType)
        {
            if (string.IsNullOrEmpty(sSiteGUID) || sSiteGUID.Equals("0"))
                return null;

            TvinciPricing.PPVModule[] ret = null;
            TvinciPricing.PPVModule[] actualRet = null;
            int numOfActualBundles = 0;
            TvinciPricing.mdoule pricingModule = new TvinciPricing.mdoule();
            string pricingWSUser = string.Empty;
            string pricingWSPass = string.Empty;
            string pricingUrl = GetWSURL("pricing_ws");
            if (pricingUrl.Length > 0)
                pricingModule.Url = pricingUrl;

            DataTable dt = null;

            switch (bundleType)
            {
                case eBundleType.SUBSCRIPTION:
                {
                    dt = DAL.ConditionalAccessDAL.Get_AllSubscriptionInfoByUsersIDs(lUsersIds, nFileTypes.ToList<int>());
                    break;
                }
                case eBundleType.COLLECTION:
                {
                    dt = DAL.ConditionalAccessDAL.Get_AllCollectionInfoByUsersIDs(lUsersIds);
                    break;
                }
            }
            

            int k = 0;
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                Int32 nCount = dt.Rows.Count;
                ret = new ConditionalAccess.TvinciPricing.PPVModule[nCount];
                using (TvinciAPI.API api = new ConditionalAccess.TvinciAPI.API())
                {
                    string sWSURL = Utils.GetWSURL("api_ws");
                    if (sWSURL.Length > 0)
                        api.Url = sWSURL;
                    string sWSUser = string.Empty;
                    string sWSPass = string.Empty;
                    TVinciShared.WS_Utils.GetWSUNPass(groupID, "DoesMediaBelongToSubscription", "api", "1.1.1.1", ref sWSUser, ref sWSPass);

                    foreach (DataRow dr in dt.Rows)
                    {
                        bool bundleValid = false;
                        int id           = 0;
                        string sCode     = string.Empty;
                        int numOfUses    = 0;
                        int maxNumOfUses = 0;

                        id = ODBCWrapper.Utils.GetIntSafeVal(dr["ID"]);

                        if(bundleType == eBundleType.SUBSCRIPTION)
                        {
                            sCode = ODBCWrapper.Utils.GetSafeStr(dr["SUBSCRIPTION_CODE"]);
                        }
                        else if(bundleType == eBundleType.COLLECTION)
                        {
                            sCode = ODBCWrapper.Utils.GetSafeStr(dr["COLLECTION_CODE"]);
                        }

                        numOfUses        = ODBCWrapper.Utils.GetIntSafeVal(dr["NUM_OF_USES"]);
                        maxNumOfUses = ODBCWrapper.Utils.GetIntSafeVal(dr["MAX_NUM_OF_USES"]);


                        if (!(maxNumOfUses != 0 && numOfUses >= maxNumOfUses && Bundle_DoesCreditNeedToDownloaded(sCode, sSiteGUID, mediaFileID, groupID, bundleType)))
                        {
                            try
                            {
                                if (bundleType == eBundleType.SUBSCRIPTION)
                                {
                                    bundleValid = api.DoesMediaBelongToSubscription(sWSUser, sWSPass, int.Parse(sCode), nFileTypes, mediaID, "");
                                }
                                else if (bundleType == eBundleType.COLLECTION)
                                {
                                    bundleValid = api.DoesMediaBelongToCollection(sWSUser, sWSPass, int.Parse(sCode), nFileTypes, mediaID, "");
                                    if (bundleValid == true)
                                    {
                                        bundleValid = !PPV_DoesCreditNeedToDownloadedUsingCollection(groupID, mediaFileID, lUsersIds, sCode);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //Logger
                                bundleValid = false;
                            }
                        }

                        if (bundleValid)
                        {

                            if (string.IsNullOrEmpty(pricingWSUser))
                            {
                                TVinciShared.WS_Utils.GetWSUNPass(groupID, "GetUserValidSubscriptions", "pricing", "1.1.1.1", ref pricingWSUser, ref pricingWSPass);
                            }

                            if (bundleType == eBundleType.SUBSCRIPTION)
                            {
                                ret[k] = pricingModule.GetSubscriptionData(pricingWSUser, pricingWSPass, sCode, string.Empty, string.Empty, string.Empty, false);
                            }
                            else if (bundleType == eBundleType.COLLECTION)
                            {
                                ret[k] = pricingModule.GetCollectionData(pricingWSUser, pricingWSPass, sCode, string.Empty, string.Empty, string.Empty, false);
                            }

                            numOfActualBundles++;

                        }
                        ++k;
                    }
                }
            } // end if dt != null
            if (numOfActualBundles > 0)
            {
                actualRet = new ConditionalAccess.TvinciPricing.PPVModule[numOfActualBundles];
                int addedBundles = 0;
                for (int i = 0; i < ret.Length; i++)
                {
                    if (ret[i] != null)
                    {
                        actualRet[addedBundles] = ret[i];
                        addedBundles++;
                    }
                }
            }
            return actualRet;
        }

        static protected TvinciPricing.Subscription[] GetUserValidSubscriptionFromList(string sSiteGUID, TvinciPricing.Subscription[] subs, int mediaFileID, int groupID)
        {
            if (subs == null)
                return null;
            TvinciPricing.Subscription[] ret = null;
            TvinciPricing.Subscription[] actualRet = null;
            int numOfActualSubs = 0;
            Int32 nSubsCount = 0;
            if (subs != null)
                nSubsCount = subs.Length;
            string sSubscCodes = "";
            System.Collections.Hashtable h = new System.Collections.Hashtable();
            for (int i = 0; i < nSubsCount; i++)
            {
                TvinciPricing.Subscription s = subs[i];
                if (sSubscCodes != "")
                    sSubscCodes += ",";
                sSubscCodes += "'" + s.m_sObjectCode + "'";
                h[s.m_sObjectCode] = s;
            }
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ID,SUBSCRIPTION_CODE, NUM_OF_USES, MAX_NUM_OF_USES from subscriptions_purchases where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            selectQuery += " and (MAX_NUM_OF_USES>=NUM_OF_USES OR MAX_NUM_OF_USES=0) and START_DATE<getdate() and (end_date is null or end_date>getdate()) ";
            if (sSubscCodes != "")
                selectQuery += " and SUBSCRIPTION_CODE in (" + sSubscCodes + ")";

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    ret = new ConditionalAccess.TvinciPricing.Subscription[nCount];

                for (int i = 0; i < nCount; i++)
                {
                    int id = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                    int numOfUses = int.Parse(selectQuery.Table("query").DefaultView[i].Row["NUM_OF_USES"].ToString());
                    int maxNumOfUses = int.Parse(selectQuery.Table("query").DefaultView[i].Row["MAX_NUM_OF_USES"].ToString());
                    string sCode = selectQuery.Table("query").DefaultView[i].Row["SUBSCRIPTION_CODE"].ToString();
                    if (maxNumOfUses != 0 && numOfUses >= maxNumOfUses)
                    {
                        if (!Bundle_DoesCreditNeedToDownloaded(sCode, sSiteGUID, mediaFileID, groupID, eBundleType.SUBSCRIPTION))
                        {
                            ret[i] = (TvinciPricing.Subscription)(h[sCode]);
                            numOfActualSubs++;
                        }
                    }
                    else
                    {
                        ret[i] = (TvinciPricing.Subscription)(h[sCode]);
                        numOfActualSubs++;
                    }

                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (numOfActualSubs > 0)
            {
                actualRet = new ConditionalAccess.TvinciPricing.Subscription[numOfActualSubs];
                int addedSubs = 0;
                for (int i = 0; i < ret.Length; i++)
                {
                    if (ret[i] != null)
                    {
                        actualRet[addedSubs] = ret[i];
                        addedSubs++;
                    }
                }
            }
            return actualRet;
        }
        static protected TvinciPricing.Subscription[] GetUserValidSubscriptionFromList(string sSiteGUID, TvinciPricing.Subscription[] subs)
        {
            if (subs == null)
                return null;
            TvinciPricing.Subscription[] ret = null;
            Int32 nSubsCount = 0;
            if (subs != null)
                nSubsCount = subs.Length;
            string sSubscCodes = "";
            System.Collections.Hashtable h = new System.Collections.Hashtable();
            for (int i = 0; i < nSubsCount; i++)
            {
                TvinciPricing.Subscription s = subs[i];
                if (sSubscCodes != "")
                    sSubscCodes += ",";
                sSubscCodes += "'" + s.m_sObjectCode + "'";
                h[s.m_sObjectCode] = s;
            }
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select SUBSCRIPTION_CODE from subscriptions_purchases where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            selectQuery += " and (MAX_NUM_OF_USES>=NUM_OF_USES OR MAX_NUM_OF_USES=0) and START_DATE<getdate() and (end_date is null or end_date>getdate()) ";
            if (sSubscCodes != "")
                selectQuery += " and SUBSCRIPTION_CODE in (" + sSubscCodes + ")";

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    ret = new ConditionalAccess.TvinciPricing.Subscription[nCount];
                for (int i = 0; i < nCount; i++)
                {
                    string sCode = selectQuery.Table("query").DefaultView[i].Row["SUBSCRIPTION_CODE"].ToString();
                    ret[i] = (TvinciPricing.Subscription)(h[sCode]);


                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return ret;
        }



        static protected TvinciPricing.Price CopyPrice(TvinciPricing.Price toCopy)
        {
            TvinciPricing.Price ret = new ConditionalAccess.TvinciPricing.Price();
            ret.m_dPrice = toCopy.m_dPrice;
            ret.m_oCurrency = toCopy.m_oCurrency;
            return ret;
        }

        static protected TvinciPricing.Price CalculateCouponDiscount(ref TvinciPricing.Price pModule, TvinciPricing.CouponsGroup oCouponsGroup, string sCouponCode, int nGroupID)
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
            selectQuery += "select CREATE_DATE,getdate() as dNow from campaigns_uses where ";
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


        static protected TvinciPricing.Price CalculateMediaFileFinalPriceNoSubs(Int32 nMediaFileID, int mediaID, TvinciPricing.Price pModule, TvinciPricing.DiscountModule discModule, TvinciPricing.CouponsGroup oCouponsGroup, string sSiteGUID, string sCouponCode, Int32 nGroupID, string subCode)
        {
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";

            TvinciPricing.Price p = CopyPrice(pModule);
            if (discModule != null)
            {
                int nPPVPurchaseCount = 0;
                if (discModule.m_dPercent == 100 && !string.IsNullOrEmpty(subCode))
                {
                    nPPVPurchaseCount = GetSubUseCount(sSiteGUID, subCode);
                }
                else
                {
                    nPPVPurchaseCount = GetPPVPurchaseCount(nMediaFileID, sSiteGUID, subCode);
                }
                p = GetPriceAfterDiscount(p, discModule, nPPVPurchaseCount);
            }

            if (sCouponCode.Length > 0)
            {

                using (TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule())
                {
                    if (GetWSURL("pricing_ws").Length > 0)
                        m.Url = GetWSURL("pricing_ws");


                    TvinciPricing.CouponData theCouponData = null;

                    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetCouponStatus", "pricing", sIP, ref sWSUserName, ref sWSPass);
                    theCouponData = m.GetCouponStatus(sWSUserName, sWSPass, sCouponCode);
                    CachingManager.CachingManager.SetCachedData("GetCouponStatus" + sCouponCode + "_" + nGroupID.ToString(), theCouponData, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);


                    if (oCouponsGroup != null && theCouponData.m_CouponType == TvinciPricing.CouponType.Voucher && theCouponData.m_campID > 0 && mediaID == theCouponData.m_ownerMedia)
                    {
                        bool isCampaignValid = false;
                        TvinciPricing.Campaign camp = m.GetCampaignData(sWSUserName, sWSPass, theCouponData.m_campID);

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

        static protected TvinciPricing.Price GetMediaFileFinalPriceNoSubs(Int32 nMediaFileID, int mediaID, TvinciPricing.PPVModule ppvModule, string sSiteGUID, string sCouponCode, Int32 nGroupID, string subCode)
        {
            TvinciPricing.Price pModule = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(ppvModule.m_oPriceCode.m_oPrise));
            //TvinciPricing.Price p = null;
            TvinciPricing.DiscountModule discModule = TVinciShared.ObjectCopier.Clone<TvinciPricing.DiscountModule>((TvinciPricing.DiscountModule)(ppvModule.m_oDiscountModule));
            TvinciPricing.CouponsGroup couponGroups = TVinciShared.ObjectCopier.Clone<TvinciPricing.CouponsGroup>((TvinciPricing.CouponsGroup)(ppvModule.m_oCouponsGroup));
            return CalculateMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, pModule, discModule, couponGroups, sSiteGUID, sCouponCode, nGroupID, subCode);
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
        static public TvinciPricing.Price GetSubscriptionFinalPrice(Int32 nGroupID, string sSubCode, string sSiteGUID, string sCouponCode, ref PriceReason theReason, ref TvinciPricing.Subscription theSub,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string connStr)
        {
            TvinciPricing.Price p = null;
            string sIP = "1.1.1.1";
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;
            TvinciPricing.Subscription s = null;
            using (TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule())
            {
                if (GetWSURL("pricing_ws").Length > 0)
                    m.Url = GetWSURL("pricing_ws");
                string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                if (CachingManager.CachingManager.Exist("GetSubscriptionData" + sSubCode + "_" + nGroupID.ToString() + sLocaleForCache) == true)
                    s = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData("GetSubscriptionData" + sSubCode + "_" + nGroupID.ToString() + sLocaleForCache));
                else
                {
                    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetSubscriptionData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                    s = m.GetSubscriptionData(sWSUserName, sWSPass, sSubCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);
                    CachingManager.CachingManager.SetCachedData("GetSubscriptionData" + sSubCode + "_" + nGroupID.ToString() + sLocaleForCache, s, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
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

                List<int> lUsersIds = ConditionalAccess.Utils.GetAllUsersDomainBySiteGUID(sSiteGUID, nGroupID);

                DataTable dt = DAL.ConditionalAccessDAL.Get_SubscriptionBySubscriptionCodeAndUserIDs(lUsersIds, sSubCode);

                if (dt != null)
                {
                    Int32 nCount = dt.Rows.Count;
                    if (nCount > 0)
                    {
                        //p = s.m_oSubscriptionPriceCode.m_oPrise;
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
            } // end using
            return p;
        }

        static public TvinciPricing.Price GetCollectionFinalPrice(Int32 nGroupID, string sColCode, string sSiteGUID, string sCouponCode, ref PriceReason theReason, ref TvinciPricing.Collection theCol,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string connStr)
        {
            TvinciPricing.Price price   = null;
            string sIP                  = "1.1.1.1";
            string sWSUserName          = string.Empty;
            string sWSPass              = string.Empty;
            TvinciPricing.Collection collection = null;
            using (TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule())
            {
                if (GetWSURL("pricing_ws").Length > 0)
                    m.Url = GetWSURL("pricing_ws");
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

                DataTable dt = DAL.ConditionalAccessDAL.Get_CollectionByCollectionCodeAndUserIDs(lUsersIds, sColCode);

                if (dt != null)
                {
                    Int32 nCount = dt.Rows.Count;
                    if (nCount > 0)
                    {
                        price.m_dPrice = 0.0;
                        theReason = PriceReason.CollectionPurchased;
                    }
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
            string sWSUserName = "";
            string sWSPass = "";
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
            string sRet = "";
            for (int i = 0; i < theArray.Length; i++)
                sRet += theArray[i].ToString() + "-";
            return sRet;
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

            DataTable dt = DAL.ConditionalAccessDAL.Get_MediaFileByProductCode(nGroupID, sProductCode);

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
            string sRet = "";
            if (String.IsNullOrEmpty(sCountryCd) == false)
                sRet += "_" + sCountryCd;
            if (String.IsNullOrEmpty(sLANGUAGE_CODE) == false)
                sRet += "_" + sLANGUAGE_CODE;
            if (String.IsNullOrEmpty(sDEVICE_NAME) == false)
                sRet += "_" + sDEVICE_NAME;
            return sRet;
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

        static private List<int> GetRelatedPPVMediaFiles(int mediaID, TvinciPricing.PPVModule pricingModule)
        {
            List<int> retVal = null;
            return retVal;
        }

        static public TvinciPricing.Price GetMediaFileFinalPrice(Int32 nMediaFileID, TvinciPricing.PPVModule ppvModule, string sSiteGUID, string sCouponCode, Int32 nGroupID, ref PriceReason theReason, ref TvinciPricing.Subscription relevantSub, ref TvinciPricing.Collection relevantCol,
            ref TvinciPricing.PrePaidModule relevantPP,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            string sFirstDeviceNameFound = string.Empty;
            return GetMediaFileFinalPrice(nMediaFileID, ppvModule, sSiteGUID, sCouponCode, nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, ref sFirstDeviceNameFound, sCouponCode, sLANGUAGE_CODE, sDEVICE_NAME, "");
        }

        static public TvinciPricing.Price GetMediaFileFinalPrice(Int32 nMediaFileID, TvinciPricing.PPVModule ppvModule, string sSiteGUID, string sCouponCode, Int32 nGroupID, ref PriceReason theReason, ref TvinciPricing.Subscription relevantSub, ref TvinciPricing.Collection relevantCol,
             ref TvinciPricing.PrePaidModule relevantPP, ref string sFirstDeviceNameFound,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sClientIP = null)
        {
            theReason = PriceReason.UnKnown;
            TvinciPricing.Price p = null;
            if (ppvModule == null)
            {
                theReason = PriceReason.Free;
                return null;
            }
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

            if (mapper.Length == 0)
                return null;
            Int32 nMediaFileTypeID = GetMediaFileTypeID(nGroupID, nMediaFileID);
            int[] fileTypes = new int[] { nMediaFileTypeID };
            int mediaID = 0;
            foreach (TvinciAPI.MeidaMaper mediaMap in mapper)
            {
                if (mediaMap != null && mediaMap.m_nMediaFileID == nMediaFileID)
                {
                    mediaID = mediaMap.m_nMediaID;
                }
            }


            if (!string.IsNullOrEmpty(sSiteGUID))
            {
                TvinciAPI.API apiWS = null;
                TvinciPricing.mdoule m = null;
                ODBCWrapper.DataSetSelectQuery fileTypesSelectQuery = null;
                try
                {
                    string sIP = "1.1.1.1";
                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    m = new ConditionalAccess.TvinciPricing.mdoule();
                    string pricingUrl = GetWSURL("pricing_ws");
                    if (pricingUrl.Length > 0)
                        m.Url = pricingUrl;

                    string relFileTypesStr = string.Empty;
                    int[] ppvRelatedFileTypes = ppvModule.m_relatedFileTypes;
                    bool isMultiMediaTypes = false;
                    if (ppvRelatedFileTypes != null && ppvRelatedFileTypes.Length > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < ppvRelatedFileTypes.Length; i++)
                        {
                            if (i > 0)
                            {
                                sb.Append(",");
                            }
                            sb.Append(ppvRelatedFileTypes[i].ToString());
                            Logger.Logger.Log("Related FileTypes", sb.ToString(), "PPVRelatedFileTypes");
                        }

                        if (!string.IsNullOrEmpty(sb.ToString()))
                        {
                            fileTypesSelectQuery = new ODBCWrapper.DataSetSelectQuery();
                            fileTypesSelectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                            fileTypesSelectQuery += string.Format(" select media_type_id from groups_media_type with (nolock) where id in ({0})", sb.ToString());
                            if (fileTypesSelectQuery.Execute("query", true) != null)
                            {
                                int count = fileTypesSelectQuery.Table("query").DefaultView.Count;
                                if (count > 0)
                                {
                                    StringBuilder relSB = new StringBuilder();
                                    for (int j = 0; j < count; j++)
                                    {
                                        if (j > 0)
                                        {
                                            relSB.Append(",");
                                            isMultiMediaTypes = true;
                                        }
                                        relSB.Append(fileTypesSelectQuery.Table("query").DefaultView[j].Row["media_type_id"].ToString());
                                    }
                                    relFileTypesStr = relSB.ToString();
                                }
                            }
                        }
                    }
                    else
                    {
                        Logger.Logger.Log("Related FileTypes", "No Related File Types Found", "PPVRelatedFileTypes");
                    }

                    // AC == Get all domains users ID's current SiteGUID
                    List<int> lUsersIds = GetAllUsersDomainBySiteGUID(sSiteGUID, nGroupID);

                    List<string> mediaFilesListstr = relFileTypesStr.Split(new char[] { ',' }).ToList();
                    List<int> mediaFilesList = new List<int>();

                    int k = 0;
                    if (isMultiMediaTypes != false)
                    {
                        foreach (string str in mediaFilesListstr)
                        {
                            mediaFilesList.Add(int.Parse(mediaFilesListstr[k]));
                            ++k;
                        }
                    }

                    DataTable dtAllFiles = DAL.ConditionalAccessDAL.Get_MediaFileByID(mediaFilesList, nMediaFileID, isMultiMediaTypes);

                    List<int> FileIDs = new List<int>();

                    foreach (DataRow dr in dtAllFiles.Rows)
                    {
                        FileIDs.Add(ODBCWrapper.Utils.GetIntSafeVal(dr["ID"]));
                    }
                    p = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(ppvModule.m_oPriceCode.m_oPrise));
                    bool bEnd = false;

                    DataTable dt = DAL.ConditionalAccessDAL.Get_AllUsersPurchases(lUsersIds, FileIDs, nMediaFileID);

                    if (dt != null)
                    {
                        Int32 nCount = dt.Rows.Count;
                        if (nCount > 0)
                        {
                            int ppvID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["ID"]);

                            string sSubCode = string.Empty;
                            string sPPCode = string.Empty;

                            sSubCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["subscription_code"]);
                            sPPCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["rel_pp"]);

                            p.m_dPrice = 0;
                            if (sSubCode.Length == 0 && sPPCode.Length == 0)
                            {
                                theReason = PriceReason.PPVPurchased;
                                if (ppvModule.m_bFirstDeviceLimitation)
                                {
                                    if (!IsFirstDeviceEqualToCurrentDevice(nMediaFileID, ppvModule.m_sObjectCode, lUsersIds, sDEVICE_NAME, ref sFirstDeviceNameFound))
                                    {
                                        theReason = PriceReason.FirstDeviceLimitation;
                                    }
                                }
                            }
                            else if (sSubCode.Length > 0)
                            {
                                theReason = PriceReason.SubscriptionPurchased;
                                string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                                if (CachingManager.CachingManager.Exist("GetSubscriptionData" + sSubCode + "_" + nGroupID.ToString() + sLocaleForCache) == true)
                                    relevantSub = (ConditionalAccess.TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData("GetSubscriptionData" + sSubCode + "_" + nGroupID.ToString() + sLocaleForCache));
                                else
                                {
                                    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetSubscriptionData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                                    relevantSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);
                                    CachingManager.CachingManager.SetCachedData("GetSubscriptionData" + sSubCode + "_" + nGroupID.ToString() + sLocaleForCache, relevantSub, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                                }

                            }
                            else if (sPPCode.Length > 0)
                            {
                                theReason = PriceReason.PrePaidPurchased;
                                string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                                if (CachingManager.CachingManager.Exist("GetPrePaidModuleData" + sPPCode + "_" + nGroupID.ToString() + sLocaleForCache) == true)
                                    relevantPP = (ConditionalAccess.TvinciPricing.PrePaidModule)(CachingManager.CachingManager.GetCachedData("GetPrePaidModuleData" + sPPCode + "_" + nGroupID.ToString() + sLocaleForCache));
                                else
                                {
                                    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetPrePaidModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                                    relevantPP = m.GetPrePaidModuleData(sWSUserName, sWSPass, int.Parse(sPPCode), sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                                    CachingManager.CachingManager.SetCachedData("GetPrePaidModuleData" + sPPCode + "_" + nGroupID.ToString() + sLocaleForCache, relevantPP, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                                }
                            }
                            bEnd = true;
                        }
                        else if (ppvModule.m_bSubscriptionOnly)
                            theReason = PriceReason.ForPurchaseSubscriptionOnly;

                    }


                    if (bEnd)
                        return p;

                    //subscriptions check
                    TvinciPricing.Subscription[] relevantValidSubscriptions = GetUserValidBundlesFromList(sSiteGUID, mediaID, nMediaFileID, nGroupID, fileTypes, lUsersIds, eBundleType.SUBSCRIPTION) as TvinciPricing.Subscription[];

                    if (relevantValidSubscriptions != null)
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
                                TvinciPricing.Price subp = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(CalculateMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule.m_oPriceCode.m_oPrise, s.m_oDiscountModule, s.m_oCouponsGroup, sSiteGUID, sCouponCode, nGroupID, s.m_sObjectCode)));
                                if (subp != null)
                                {
                                    //****************************************************************
                                    // Geo Block  Check Subscription
                                    //****************************************************************
                                    //Start...
                                    apiWS = new TvinciAPI.API();
                                    string apiUrl = GetWSURL("api_ws");
                                    if (apiUrl.Length > 0)
                                        apiWS.Url = apiUrl;

                                    string apiWSUser = string.Empty;
                                    string apiWSPass = string.Empty;
                                    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "CheckGeoCommerceBlock", "api", "1.1.1.1", ref apiWSUser, ref apiWSPass);

                                    bool isGeoBlock = false;
                                    if (!string.IsNullOrEmpty(sClientIP))
                                    {
                                        isGeoBlock = apiWS.CheckGeoCommerceBlock(apiWSUser, apiWSPass, s.n_GeoCommerceID, sClientIP);
                                    }

                                    //End...
                                    //****************************************************************
                                    // Geo Block  Check Subscription
                                    //****************************************************************
                                    if (isGeoBlock)
                                    {
                                        p = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(subp));
                                        relevantSub = TVinciShared.ObjectCopier.Clone<TvinciPricing.Subscription>((TvinciPricing.Subscription)(s));
                                        theReason = PriceReason.GeoCommerceBlocked;
                                    }
                                    else
                                    {
                                        if (p == null)
                                        {
                                            p = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(subp));
                                            relevantSub = TVinciShared.ObjectCopier.Clone<TvinciPricing.Subscription>((TvinciPricing.Subscription)(s));
                                            theReason = PriceReason.SubscriptionPurchased;
                                        }
                                        else if (subp.m_oCurrency.m_sCurrencyCD3 == ppvModule.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3 && subp.m_dPrice <= p.m_dPrice)
                                        {
                                            p = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(subp));
                                            relevantSub = TVinciShared.ObjectCopier.Clone<TvinciPricing.Subscription>((TvinciPricing.Subscription)(s));
                                            theReason = PriceReason.SubscriptionPurchased;
                                        }
                                        else if (subp.m_oCurrency.m_sCurrencyCD3 != ppvModule.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3 && p.m_dPrice > 0)
                                        {
                                            p = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(subp));
                                            relevantSub = TVinciShared.ObjectCopier.Clone<TvinciPricing.Subscription>((TvinciPricing.Subscription)(s));
                                            theReason = PriceReason.SubscriptionPurchased;
                                        }

                                        bEnd = true;
                                    }
                                }
                            }
                        }
                    }

                    if (bEnd)
                        return p;

                    //collections check
                    TvinciPricing.Collection[] relevantValidCollections = new Collection[1];
                    relevantValidCollections[0] = (TvinciPricing.Collection)GetUserValidBundlesFromList(sSiteGUID, mediaID, nMediaFileID, nGroupID, fileTypes, lUsersIds, eBundleType.COLLECTION)[0];

                    if (relevantValidCollections != null)
                    {
                        List<TvinciPricing.Collection> priorityCollections = relevantValidCollections.ToList();
                        for (int i = 0; i < priorityCollections.Count; i++)
                        {
                            TvinciPricing.Collection collection   = priorityCollections[i];
                            TvinciPricing.DiscountModule discount = (TvinciPricing.DiscountModule)(collection.m_oDiscountModule);
                            TvinciPricing.Price collectionsPrice  = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(CalculateMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule.m_oPriceCode.m_oPrise, collection.m_oDiscountModule, collection.m_oCouponsGroup, sSiteGUID, sCouponCode, nGroupID, collection.m_sObjectCode)));
                            if (collectionsPrice != null)
                            {
                                if (p == null)
                                {
                                    p = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(collectionsPrice));
                                    relevantCol = TVinciShared.ObjectCopier.Clone<TvinciPricing.Collection>((TvinciPricing.Collection)(collection));
                                    theReason = PriceReason.CollectionPurchased;
                                }
                                else if (collectionsPrice.m_oCurrency.m_sCurrencyCD3 == ppvModule.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3 && collectionsPrice.m_dPrice <= p.m_dPrice)
                                {
                                    p = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(collectionsPrice));
                                    relevantCol = TVinciShared.ObjectCopier.Clone<TvinciPricing.Collection>((TvinciPricing.Collection)(collection));
                                    theReason = PriceReason.CollectionPurchased;
                                }
                                else if (collectionsPrice.m_oCurrency.m_sCurrencyCD3 != ppvModule.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3 && p.m_dPrice > 0)
                                {
                                    p = TVinciShared.ObjectCopier.Clone<TvinciPricing.Price>((TvinciPricing.Price)(collectionsPrice));
                                    relevantCol = TVinciShared.ObjectCopier.Clone<TvinciPricing.Collection>((TvinciPricing.Collection)(collection));
                                    theReason = PriceReason.CollectionPurchased;
                                }
                            }
                        }
                    }
                    //If was not purchase in any way
                    else
                    {
                        p = GetMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule, sSiteGUID, sCouponCode, nGroupID, string.Empty);
                        if (p != null && p.m_dPrice == 0 && theReason != PriceReason.ForPurchaseSubscriptionOnly)
                        {
                            theReason = PriceReason.Free;
                        }
                        else if (theReason != PriceReason.ForPurchaseSubscriptionOnly)
                        {
                            theReason = PriceReason.ForPurchase;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    #region Disposing
                    if (m != null)
                    {
                        m.Dispose();
                        m = null;
                    }
                    if (apiWS != null)
                    {
                        apiWS.Dispose();
                        apiWS = null;
                    }
                    if (fileTypesSelectQuery != null)
                    {
                        fileTypesSelectQuery.Finish();
                        fileTypesSelectQuery = null;
                    }
                    #endregion
                }
            } // end if site guid is not null or empty
            else
            {
                p = GetMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule, sSiteGUID, sCouponCode, nGroupID, string.Empty);
                if (ppvModule != null && ppvModule.m_bSubscriptionOnly)
                {
                    theReason = PriceReason.ForPurchaseSubscriptionOnly;
                }
                if (theReason != PriceReason.ForPurchaseSubscriptionOnly)
                {
                    theReason = PriceReason.ForPurchase;
                }
            }

            return p;
        }


        static public List<int> GetAllUsersDomainBySiteGUID(string sSiteGUID, Int32 nGroupID)
        {
            List<int> lDomainsUsers = new List<int>();

            if (string.IsNullOrEmpty(sSiteGUID) || sSiteGUID.Equals("0"))
            {
                return lDomainsUsers;
            }

            string sIP = "1.1.1.1";
            using (TvinciUsers.UsersService u = new TvinciUsers.UsersService())
            {
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetUserData", "Users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL.Length > 0)
                    u.Url = sWSURL;

                TvinciUsers.UserResponseObject userResponseObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);

                if (userResponseObj.m_RespStatus == TvinciUsers.ResponseStatus.OK && userResponseObj.m_user.m_domianID != 0)
                {
                    lDomainsUsers = GetDomainsUsers(userResponseObj.m_user.m_domianID, nGroupID);
                }
                else
                {
                    lDomainsUsers.Add(int.Parse(sSiteGUID));
                }
            }

            return lDomainsUsers;
        }

        static private List<int> GetDomainsUsers(int nDomainID, Int32 nGroupID)
        {
            string sIP = "1.1.1.1";
            List<int> intUsersList = new List<int>();
            using (TvinciDomains.module bm = new ConditionalAccess.TvinciDomains.module())
            {
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetDomainUserList", "Domains", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("domains_ws");
                if (sWSURL.Length > 0)
                    bm.Url = sWSURL;


                try
                {
                    string[] usersList = bm.GetDomainUserList(sWSUserName, sWSPass, nDomainID);
                    if (usersList != null && usersList.Length != 0)
                    {
                        foreach (string str in usersList)
                        {
                            intUsersList.Add(int.Parse(str));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log("Error", "msg:" + ex.Message, "GetDomainsUsers");
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
                    m.Url = Utils.GetWSURL("pricing_ws");

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

        static public Int32 GetMediaFileIDWithCoGuid(Int32 nGroupID, string sMediaFileCoGuid)
        {
            int nMediaFileID = 0;

            DataTable dt = DAL.ConditionalAccessDAL.Get_MediaFileFromCoGuid(nGroupID, sMediaFileCoGuid);

            if (dt != null && dt.DefaultView.Count > 0)
            {
                nMediaFileID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["ID"]);
            }

            return nMediaFileID;
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
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetSubscriptionByProductCode", "pricing", sIP, ref sWSUserName, ref sWSPass);

                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (sWSURL.Length > 0)
                    p.Url = sWSURL;

                return p.GetSubscriptionDataByProductCode(sWSUserName, sWSPass, sProductCode, sCountryCd2, sLanguageCode3, sDeviceName, bGetAlsoUnActive);
            }
        }

        static public string GetBasicLink(int nGroupID, int[] nMediaFileIDs, int nMediaFileID, string sBasicLink)
        {
            
            TvinciAPI.MeidaMaper[] mapper = null;
            mapper = Utils.GetMediaMapper(nGroupID, nMediaFileIDs);
            int mediaID = 0;

            foreach (TvinciAPI.MeidaMaper mediaMap in mapper)
            {
                if (mediaMap != null && mediaMap.m_nMediaFileID == nMediaFileID)
                {
                    mediaID = mediaMap.m_nMediaID;
                }
            }

            if (sBasicLink == string.Format("{0}||{1}", mediaID, nMediaFileID))
            {
                string sBaseURL = string.Empty;
                string sStreamID = string.Empty;

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += " select sc.VIDEO_BASE_URL, mf.STREAMING_CODE from streaming_companies sc , media_files mf where ";
                selectQuery += "mf.STREAMING_SUPLIER_ID = sc.ID";
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.ID", "=", nMediaFileID);
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sBaseURL = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "VIDEO_BASE_URL", 0);
                        sStreamID = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "STREAMING_CODE", 0);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                sBasicLink = string.Format("{0}{1}", sBaseURL, sStreamID);
                if (sStreamID != "")
                {
                    if (sBasicLink.IndexOf("!--COUNTRY_CD--") != -1)
                    {
                        object oGroupCD = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_COUNTRY_CODE", nGroupID, 86400, "MAIN_CONNECTION_STRING");
                        if (oGroupCD != null && oGroupCD != DBNull.Value)
                            sBasicLink = sBasicLink.Replace("!--COUNTRY_CD--", oGroupCD.ToString().Trim().ToLower());
                    }

                    if (sBasicLink.IndexOf("!--tick_time--") != -1)
                    {
                        long lT = DateTime.UtcNow.Ticks;
                        object oGroupSecret = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_SECRET_CODE", nGroupID, 86400, "MAIN_CONNECTION_STRING");
                        sBasicLink = sBasicLink.Replace("!--tick_time--", "tick=" + lT.ToString());
                        string sToHash = "";
                        string sHashed = "";
                        if (oGroupSecret != null && oGroupSecret != DBNull.Value)
                        {
                            sToHash = oGroupSecret.ToString() + lT.ToString();
                            sHashed = TVinciShared.ProtocolsFuncs.CalculateMD5Hash(sToHash);
                        }
                        sBasicLink = sBasicLink.Replace("!--hash--", "hash=" + sHashed);
                    }
                    if (sBasicLink.IndexOf("!--group--") != -1)
                    {
                        sBasicLink = sBasicLink.Replace("!--group--", "group=" + nGroupID.ToString());
                    }
                    if (sBasicLink.IndexOf("!--config_data--") != -1)
                    {
                        sBasicLink = sBasicLink.Replace("!--config_data--", "brt=" + "");
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
                    if (sWSURL != "")
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
            catch (Exception ex)
            {
                res = null;
            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                    u = null;
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
                    p = null;
                }
                #endregion
            }
            return result;
        }

        static public eBillingProvider GetBiilingProvider(int nBillingProvider)
        {
            eBillingProvider result = eBillingProvider.Unknown;
            try
            {
                if (Enum.IsDefined(typeof(eBillingProvider), nBillingProvider))
                {
                    result = (eBillingProvider)nBillingProvider;
                }
            }
            catch
            {
                result = eBillingProvider.Unknown;
            }
            return result;
        }


        public static bool IsFirstDeviceEqualToCurrentDevice(int nMediaFileID, string sPPVCode, List<int> lUsersIds, string sCurrentDeviceName, ref string sFirstDeviceName)
        {
            bool result = false;
            int numOfRowsReturned = 0;

            sFirstDeviceName = ConditionalAccessDAL.Get_FirstDeviceUsedByPPVModule(nMediaFileID, sPPVCode, lUsersIds, out numOfRowsReturned);
            if (numOfRowsReturned == 0)
            {
                return true;
            }

            result = (sCurrentDeviceName == sFirstDeviceName);

            return result;
        }

    }
}
