using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public class BFTVConditionalAccess : BaseConditionalAccess
    {
        public BFTVConditionalAccess(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public BFTVConditionalAccess(Int32 nGroupID, string connKey)
            : base(nGroupID, connKey)
        {
        }

        protected override string GetLicensedLink(string sBasicLink, string sUserIP, string sRefferer)
        {
            throw new NotImplementedException();
        }

        public override CampaignActionInfo ActivateCampaignWithInfo(int campaignID, CampaignActionInfo cai)
        {
            throw new NotImplementedException();
        }

        protected override string GetErrorLicensedLink(string sBasicLink)
        {
            return "";
        }

        public override bool ActivateCampaign(int campaignID, CampaignActionInfo cai)
        {
            return false; 
        }

        public override ConditionalAccess.TvinciBilling.BillingResponse CC_ChargeUserForMediaFile(string sSiteGUID, double dPrice, string sCurrency, int nMediaFileID, int nMediaID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bDummy)
        {
            throw new NotImplementedException();
        }

        public override ConditionalAccess.TvinciBilling.BillingResponse CC_ChargeUserForSubscription(string sSiteGUID, double dPrice, string sCurrency, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParams, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bDummy)
        {
            throw new NotImplementedException();
        }
        public override SubscriptionsPricesContainer[] GetSubscriptionsPrices(string[] sSubscriptions, string sUserGUID, string sCouponCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sIP = null)
        {
            return base.GetSubscriptionsPrices(sSubscriptions, sUserGUID, sCouponCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, sIP);
        }
        //public override SubscriptionsPricesContainer[] GetSubscriptionsPrices(string[] sSubscriptions, string sUserGUID, string sCouponCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        //{
        //    throw new NotImplementedException();
        //}

        public override BillingTransactionsResponse GetUserBillingHistory(string sUserGUID, int nStartIndex, int nNumberOfItems)
        {
            throw new NotImplementedException();
        }

        public override UserCAStatus GetUserCAStatus(string sSiteGUID)
        {
            throw new NotImplementedException();
        }

        

        public override ConditionalAccess.TvinciBilling.BillingResponse PU_GetPPVPopupPaymentMethodURL(string sSiteGUID, double dPrice, string sCurrency, int nMediaFileID, int nMediaID, string sPPVModuleCode, string sCouponCode, string sPaymentMethod, string sExtraParameters, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            throw new NotImplementedException();
        }

        public override ConditionalAccess.TvinciBilling.BillingResponse PU_GetSubscriptionPopupPaymentMethodURL(string sSiteGUID, double dPrice, string sCurrency, string sSubscriptionCode, string sCouponCode, string sPaymentMethod, string sExtraParameters, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            throw new NotImplementedException();
        }

        public override ConditionalAccess.TvinciBilling.BillingResponse SMS_ChargeUserForMediaFile(string sSiteGUID, string sCellPhone, double dPrice, string sCurrency, int nMediaFileID, int nMediaID, string sPPVModuleCode, string sCouponCode, string sExtraParameters, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            throw new NotImplementedException();
        }

        public override ConditionalAccess.TvinciBilling.BillingResponse SMS_ChargeUserForSubscription(string sSiteGUID, string sCellPhone, double dPrice, string sCurrency, string sSubscriptionCode, string sCouponCode, string sExtraParameters, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            throw new NotImplementedException();
        }

        public override ConditionalAccess.TvinciBilling.BillingResponse SMS_CheckCodeForMediaFile(string sSiteGUID, string sCellPhone, string sSMSCode, int nMediaFileID, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            throw new NotImplementedException();
        }

        public override ConditionalAccess.TvinciBilling.BillingResponse SMS_CheckCodeForSubscription(string sSiteGUID, string sCellPhone, string sSMSCode, string sSubscription, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            throw new NotImplementedException();
        }

        public override string GetLicensedLink(string sSiteGUID, int nMediaFileID, string sBasicLink, string sUserIP, string sRefferer, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, string couponCode)
        {
            string sMediaGuid = "";
            //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery += "select m.CO_GUID from media m, media_files mf where mf.media_id=m.id and m.is_active=1 and m.status=1 and mf.is_active=1 and mf.status=1 and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", nMediaFileID);
            //if (selectQuery.Execute("query", true) != null)
            //{
            //    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            //    if (nCount > 0)
            //        sMediaGuid = selectQuery.Table("query").DefaultView[0].Row["CO_GUID"].ToString();
            //}
            //selectQuery.Finish();
            //selectQuery = null;
            //if (sMediaGuid == "")
            //    return "";
            //string sUN = System.Configuration.ConfigurationManager.AppSettings["BF_WS_UN"].ToString();
            //string sPass = System.Configuration.ConfigurationManager.AppSettings["BF_WS_PASS"].ToString();
            //string sSecret = System.Configuration.ConfigurationManager.AppSettings["BF_WS_SECRET"].ToString();
            //bool bRet = BFTVFeeder.feeder.IsUserPermitMedia(sSiteGUID, sMediaGuid , sUN , sPass);
            //if (bRet == true && Utils.ValidateBaseLink(m_nGroupID, nMediaFileID, sBasicLink) == true)
            //    return sBasicLink;
            return GetErrorLicensedLink(sBasicLink);
        }

        public override bool IsItemPermitted(string sSiteGUID, int mediaID)
        {
            string sMediaGuid = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += "select m.CO_GUID from media m where m.is_active=1 and m.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.id", "=", mediaID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sMediaGuid = selectQuery.Table("query").DefaultView[0].Row["CO_GUID"].ToString();
            }
            selectQuery.Finish();
            selectQuery = null;
            if (sMediaGuid == "")
                return false;
            string sUN = System.Configuration.ConfigurationManager.AppSettings["BF_WS_UN"].ToString();
            string sPass = System.Configuration.ConfigurationManager.AppSettings["BF_WS_PASS"].ToString();
            string sSecret = System.Configuration.ConfigurationManager.AppSettings["BF_WS_SECRET"].ToString();
            bool bRet = BFTVFeeder.feeder.IsUserPermitMedia(sSiteGUID, sMediaGuid, sUN, sPass);
            return bRet;
        }

        public override bool IsSubscriptionPurchased(string siteGuid, string subID, ref string reason)
        {
            string sMediaGuid = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += "select c.CO_GUID from channels c where c.is_active=1 and c.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("c.id", "=", subID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sMediaGuid = selectQuery.Table("query").DefaultView[0].Row["CO_GUID"].ToString();
            }
            selectQuery.Finish();
            selectQuery = null;
            if (sMediaGuid == "")
                return false;
            string sUN = System.Configuration.ConfigurationManager.AppSettings["BF_WS_UN"].ToString();
            string sPass = System.Configuration.ConfigurationManager.AppSettings["BF_WS_PASS"].ToString();
            string sSecret = System.Configuration.ConfigurationManager.AppSettings["BF_WS_SECRET"].ToString();
            bool bRet = BFTVFeeder.feeder.IsUserPermittedChannel(siteGuid, sMediaGuid, ref reason);
            return bRet;
        }

        public override MediaFileItemPricesContainer[] GetItemsPrices(int[] nMediaFiles, string sUserGUID, bool bOnlyLowest, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            MediaFileItemPricesContainer[] ret = null;
            string nMediasForCache = Utils.ConvertArrayIntToStr(nMediaFiles);
            PermittedMediaContainer[] oModules = null;
            string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
            if (CachingManager.CachingManager.Exist("GetUserPermittedItems" + nMediasForCache + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                oModules = TVinciShared.ObjectCopier.Clone<PermittedMediaContainer[]>((PermittedMediaContainer[])(CachingManager.CachingManager.GetCachedData("GetUserPermittedItems" + nMediasForCache + "_" + m_nGroupID.ToString() + sLocaleForCache)));
            else
            {
                oModules = GetUserPermittedItems(sUserGUID);
                PermittedMediaContainer[] oModulesCopy = TVinciShared.ObjectCopier.Clone<PermittedMediaContainer[]>(oModules);
                CachingManager.CachingManager.SetCachedData("GetUserPermittedItems" + nMediasForCache + "_" + m_nGroupID.ToString() + sLocaleForCache, oModulesCopy, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }
            Int32 nCount = 0;
            if (oModules != null)
                nCount = nMediaFiles.Length;
            if (nCount > 0)
                ret = new MediaFileItemPricesContainer[nCount];
            for (int i = 0; i < nMediaFiles.Length; i++)
            {
                MediaFileItemPricesContainer mf = new MediaFileItemPricesContainer();
                ItemPriceContainer[] itemPriceCont = new ItemPriceContainer[1];
                itemPriceCont[0] = new ItemPriceContainer();
                Int32 nMediaFileID = nMediaFiles[i];
                TvinciPricing.Subscription relevantSub = null;
                TvinciPricing.Price p = new ConditionalAccess.TvinciPricing.Price();
                p.m_dPrice = 1.0;
                p.m_oCurrency = new ConditionalAccess.TvinciPricing.Currency();
                p.m_oCurrency.m_sCurrencyCD3 = "USD";
                p.m_oCurrency.m_sCurrencySign = "&";
                p.m_oCurrency.m_nCurrencyID = 1;

                TvinciPricing.Price fp = new ConditionalAccess.TvinciPricing.Price();
                fp.m_dPrice = 1.0;
                fp.m_oCurrency = new ConditionalAccess.TvinciPricing.Currency();
                fp.m_oCurrency.m_sCurrencyCD3 = "USD";
                fp.m_oCurrency.m_sCurrencySign = "&";
                fp.m_oCurrency.m_nCurrencyID = 1;
                PriceReason theReason = PriceReason.ForPurchaseSubscriptionOnly;
                for (int x = 0; x < oModules.Length; x++)
                {
                    Int32 nRefMediaFileID = oModules[x].m_nMediaFileID;
                    if (nRefMediaFileID == nMediaFileID)
                    {
                        theReason = PriceReason.SubscriptionPurchased;
                        p.m_dPrice = 0.0;
                        break;
                    }
                }
                itemPriceCont[0].Initialize(p, fp, "", null, theReason, relevantSub, false);
                mf.Initialize(nMediaFileID, itemPriceCont);
                ret[i] = mf;
            }
            return ret;
        }

        public override MediaFileItemPricesContainer[] GetItemsPrices(int[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return GetItemsPrices(nMediaFiles, sUserGUID, bOnlyLowest, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
        }

        public override PermittedSubscriptionContainer[] GetUserPermittedSubscriptions(string sSiteGUID)
        {
            PermittedSubscriptionContainer[] ret = null;
            string sUN = System.Configuration.ConfigurationManager.AppSettings["BF_WS_UN"].ToString();
            string sPass = System.Configuration.ConfigurationManager.AppSettings["BF_WS_PASS"].ToString();
            List<string> channelsList = BFTVFeeder.feeder.GetUserPermittedChannels(sSiteGUID, sUN, sPass);
            int i = 0;
            if (channelsList != null && channelsList.Count > 0)
            {
                ret = new PermittedSubscriptionContainer[channelsList.Count];
                foreach (string channelID in channelsList)
                {
                    PermittedSubscriptionContainer sub = new PermittedSubscriptionContainer();
                    sub.Initialize(channelID, 0, 0, DateTime.MaxValue, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, DateTime.MaxValue, false, false, 0, PaymentMethod.Unknown, string.Empty);
                    ret[i] = sub;
                    i++;
                }
            }
            return ret;
        }

        public override PermittedMediaContainer[] GetUserPermittedItems(string sSiteGUID)
        {
            PermittedMediaContainer[] ret = null;
            string sUN = System.Configuration.ConfigurationManager.AppSettings["BF_WS_UN"].ToString();
            string sPass = System.Configuration.ConfigurationManager.AppSettings["BF_WS_PASS"].ToString();
            System.Collections.Hashtable nMediaFilesIDs = BFTVFeeder.feeder.GetUserPrmittedMediaFileIDs(sSiteGUID, sUN, sPass);

            if (nMediaFilesIDs.Count > 0)
                ret = new PermittedMediaContainer[nMediaFilesIDs.Count];
            System.Collections.IEnumerator d = nMediaFilesIDs.Keys.GetEnumerator();
            Int32 i = 0;
            while (d.MoveNext())
            {
                Int32 nMediaID = int.Parse(d.Current.ToString());
                Int32 nMediaFileID = (Int32)(nMediaFilesIDs[nMediaID]);
                Int32 nMaxUses = 0;
                Int32 nCurrentUses = 0;
                DateTime dEnd = new DateTime(2099, 1, 1);
                DateTime dCurrent = DateTime.UtcNow;
                DateTime dCreateDate = DateTime.UtcNow;

                PermittedMediaContainer p = new PermittedMediaContainer();
                p.Initialize(nMediaID, nMediaFileID, nMaxUses, nCurrentUses, dEnd, dCurrent, dCreateDate, PaymentMethod.Unknown, string.Empty);
                ret[i] = p;
                i++;
            }
            return ret;
        }
    }
}
