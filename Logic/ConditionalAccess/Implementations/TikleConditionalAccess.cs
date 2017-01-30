using KLogMonitor;
using Core.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiObjects.ConditionalAccess;

namespace Core.ConditionalAccess
{
    class TikleConditionalAccess : TvinciConditionalAccess
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public TikleConditionalAccess(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public TikleConditionalAccess(Int32 nGroupID, string connKey)
            : base(nGroupID, connKey)
        {
        }

        public override CampaignActionInfo ActivateCampaignWithInfo(int campaignID, CampaignActionInfo cai)
        {
            throw null;
        }

        public override bool ActivateCampaign(int campaignID, CampaignActionInfo cai)
        {
            return false;
        }

        protected string GetLicensedLink(string sBasicLink, string sUserIP, string sRefferer)
        {
            string sPath =
            "/cp94204.edgefcs.net/ondemand"; // --> valid for the cpcode
            // "videos/test"; // --> valid for the test flv
            // "videos/"; // --> valid for the videos path
            //"videos/test"; // --> valid for the test flv and blah virtual path, multiple values separated by semicolons

            Akamai.Authentication.SecureStreaming.TypeDToken token = new Akamai.Authentication.SecureStreaming.TypeDToken(
                sPath,
                sUserIP, // sIP
                "Token", // Profile
                "edenmaybar", // Password][
                Convert.ToInt64(0), // Time: defaults to now: time span start
                Convert.ToInt64(600), // Window (time span lenght): here set for 10 minutes
                Convert.ToInt64(0), // Duration: N/A in flash
                null);

            if (token != null)
            {
                string url = sPath.StartsWith("/") ? // matching against a domain/application name does not require an slist
                    string.Format("{0}?auth={1}&aifp=2000", sBasicLink, token.String) :
                    string.Format("{0}?auth={1}&aifp=2000&slist={2}", sBasicLink, token.String, sPath);
                return url;
            }
            return sBasicLink;
        }

        protected override string GetErrorLicensedLink(string sBasicLink)
        {
            return "";
        }

        protected void HandleSubscriptionUsesNotification(Int32 nMediaFileID, string sSubCode, string sSiteGUID)
        {
            Int32 nMediaID = Utils.GetMediaIDFromFileID(nMediaFileID, m_nGroupID);

            string sUrl = string.Format("https://www.sinema.com/wswebtv/FreeMediaStat.aspx?media_id={0}&package_id={1}&customer_id={2}", nMediaFileID, sSubCode, sSiteGUID);

            string res = TVinciShared.WS_Utils.SendXMLHttpReq(sUrl, string.Empty, string.Empty);

            string sLogMessage = string.Format("response:{0} media_id:{1} sebscription_code:{2} site_guid:{3}", res, nMediaID.ToString(), sSubCode, sSiteGUID);

            log.Debug(sLogMessage);
        }

        protected virtual void HandleCouponUses(Subscription relevantSub, string sPPVModuleCode,
            string sSiteGUID, double dPrice, string sCurrency,
            Int32 nMediaFileID, string sCouponCode, string sUserIP,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bFromPurchase, int nPrePaidCode, Int32 relevantCollection)
        {
            if (!string.IsNullOrEmpty(sCouponCode))
            {
                if (bFromPurchase)
                    return;

                Pricing.Module.SetCouponUsed(m_nGroupID, sCouponCode, sSiteGUID);

                double dPercent = Utils.GetCouponDiscountPercent(m_nGroupID, sCouponCode);
                log.Debug("Set Coupon Used called : " + sCouponCode + ", Coupon Discount Percent : " + dPercent.ToString());
                if (dPercent == 100)
                {
                    log.Debug("Coupon Code : " + sCouponCode + ", Coupon Discount Percent : " + dPercent.ToString());

                    Int32[] nMediaFiles = { nMediaFileID };
                    string sMediaFileForCache = Utils.ConvertArrayIntToStr(nMediaFiles);
                    PPVModule oPPVModule = null;
                    string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                    if (CachingManager.CachingManager.Exist("GetPPVModuleData" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                        oPPVModule = (PPVModule)(CachingManager.CachingManager.GetCachedData("GetPPVModuleData" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache));
                    else
                    {
                        oPPVModule = Pricing.Module.GetPPVModuleData(m_nGroupID, sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        CachingManager.CachingManager.SetCachedData("GetPPVModuleData" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache, oPPVModule, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    }

                    Int32 nMediaID = Utils.GetMediaIDFromFileID(nMediaFileID, m_nGroupID);

                    string sCustomData = GetCustomData(relevantSub, oPPVModule, null, sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID,
                        sPPVModuleCode, string.Empty, sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);


                    string sType = "pp";

                    string smedia_file = nMediaFileID.ToString();
                    string smedia_id = nMediaID.ToString();

                    string ssub = string.Empty;
                    //string srelevantsub = string.Empty;  
                    if (relevantSub != null)
                    {
                        ssub = relevantSub.m_sObjectCode;
                        //srelevantsub = relevantSub.m_sObjectCode;
                    }

                    string sppvmodule = sPPVModuleCode;
                    string smnou = string.Empty;
                    string smaxusagemodulelifecycle = string.Empty;
                    string sviewlifecyclesecs = string.Empty;
                    if (oPPVModule != null)
                    {
                        smnou = oPPVModule.m_oUsageModule.m_nMaxNumberOfViews.ToString();
                        smaxusagemodulelifecycle = oPPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle.ToString();
                        sviewlifecyclesecs = oPPVModule.m_oUsageModule.m_tsViewLifeCycle.ToString();
                    }

                    string sir = "false";
                    bool bIsRecurring = false;
                    if (sir == "true")
                        bIsRecurring = true;

                    string sTransactionData = "<TransactionData>";
                    sTransactionData += "<CustomerId>" + sSiteGUID + "</CustomerId>";
                    sTransactionData += "<ProductId>" + smedia_id + "</ProductId>";
                    sTransactionData += "<SubscriptionId>" + ssub + "</SubscriptionId>";

                    if (sType == "sp")
                    {
                        if (bIsRecurring == true)
                        {
                            if (smaxusagemodulelifecycle == "111111")
                                sTransactionData += "<SalesType>2</SalesType>";
                            else if (smaxusagemodulelifecycle == "10080")
                                sTransactionData += "<SalesType>1</SalesType>";
                            else
                                sTransactionData += "<SalesType>0</SalesType>";
                        }
                        else
                            sTransactionData += "<SalesType>0</SalesType>";
                    }

                    if (sType == "pp")
                        sTransactionData += "<SalesType>0</SalesType>";

                    sTransactionData += "<PaymentToken></PaymentToken>";
                    sTransactionData += "<Msisdn></Msisdn>";
                    sTransactionData += "<IsCargo>false</IsCargo>";
                    sTransactionData += "<Price>" + dPrice.ToString() + "</Price>";
                    sTransactionData += "<Currency>" + sCurrency + "</Currency>";
                    sTransactionData += "<PromotionCoupon>" + dPercent.ToString() + "</PromotionCoupon>";
                    sTransactionData += "<TransactionDate>" + DateTime.UtcNow.ToString("dd.MM.yyyy hh:mm:ss") + "</TransactionDate>";
                    sTransactionData += "<CustomData>" + sCustomData + "</CustomData>";
                    sTransactionData += "</TransactionData>";

                    //Tickle WS
                    APILogic.tikle.Service s = new APILogic.tikle.Service();
                    string sTikleWSURL = Utils.GetWSURL("tikle_ws");
                    s.Url = sTikleWSURL;

                    string sMD5Hash = Core.Billing.Utils.GetHash("0" + sTransactionData, "WS_SECRET");
                    try
                    {
                        s.Purchase("0", sTransactionData, sMD5Hash);
                        log.Debug(sTransactionData);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message);
                    }

                }
            }
        }

        protected internal override string GetCustomData(Subscription relevantSub, PPVModule thePPVModule, Campaign campaign,
            string sSiteGUID, double dPrice, string sCurrency,
            Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCampaignCode, string sCouponCode, string sUserIP,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            string sCustomData = "<customdata type=\"pp\">";
            if (String.IsNullOrEmpty(sCountryCd) == false)
                sCustomData += "<lcc>" + sCountryCd + "</lcc>";
            if (String.IsNullOrEmpty(sLANGUAGE_CODE) == false)
                sCustomData += "<llc>" + sLANGUAGE_CODE + "</llc>";
            if (String.IsNullOrEmpty(sDEVICE_NAME) == false)
                sCustomData += "<ldn>" + sDEVICE_NAME + "</ldn>";
            sCustomData += "<rs>";
            if (relevantSub != null)
                sCustomData += relevantSub.m_sObjectCode;
            sCustomData += "</rs>";
            sCustomData += "<mnou>";
            if (thePPVModule != null && thePPVModule.m_oUsageModule != null)
                sCustomData += thePPVModule.m_oUsageModule.m_nMaxNumberOfViews.ToString();
            sCustomData += "</mnou>";
            sCustomData += "<mumlc>";
            if (thePPVModule != null && thePPVModule.m_oUsageModule != null)
                sCustomData += thePPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle.ToString();
            sCustomData += "</mumlc>";
            sCustomData += "<u id=\"" + sSiteGUID + "\"/>";
            sCustomData += string.Format("<up>{0}</up>", sUserIP);
            sCustomData += "<mf>";
            sCustomData += nMediaFileID.ToString();
            sCustomData += "</mf>";
            sCustomData += "<m>";
            sCustomData += nMediaID.ToString();
            sCustomData += "</m>";
            sCustomData += "<ppvm>";
            sCustomData += sPPVModuleCode;
            sCustomData += "</ppvm>";
            sCustomData += "<cc>";
            sCustomData += Utils.GetCouponDiscountPercent(m_nGroupID, sCouponCode).ToString();
            sCustomData += "</cc>";
            sCustomData += "<p ir=\"false\" n=\"1\" o=\"1\"/>";

            sCustomData += "<pc>";
            if (thePPVModule != null && thePPVModule.m_oPriceCode != null)
                sCustomData += thePPVModule.m_oPriceCode.m_sCode;
            sCustomData += "</pc>";
            sCustomData += "<pri>";
            sCustomData += dPrice.ToString();
            sCustomData += "</pri>";
            sCustomData += "<cu>";
            sCustomData += sCurrency;
            sCustomData += "</cu>";

            sCustomData += "</customdata>";

            return sCustomData;
        }

        protected override string GetCustomDataForSubscription(Subscription theSub, Campaign campaign, string sSubscriptionCode, string sCampaignCode,
            string sSiteGUID, double dPrice, string sCurrency, string sCouponCode, string sUserIP,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {

            bool bIsRecurring = theSub.m_bIsRecurring;
            Int32 nRecPeriods = theSub.m_nNumberOfRecPeriods;

            string sCustomData = "<customdata type=\"sp\">";
            if (String.IsNullOrEmpty(sCountryCd) == false)
                sCustomData += "<lcc>" + sCountryCd + "</lcc>";
            if (String.IsNullOrEmpty(sLANGUAGE_CODE) == false)
                sCustomData += "<llc>" + sLANGUAGE_CODE + "</llc>";
            if (String.IsNullOrEmpty(sDEVICE_NAME) == false)
                sCustomData += "<ldn>" + sDEVICE_NAME + "</ldn>";
            sCustomData += "<mnou>";
            if (theSub != null && theSub.m_oUsageModule != null)
                sCustomData += theSub.m_oUsageModule.m_nMaxNumberOfViews.ToString();
            sCustomData += "</mnou>";
            sCustomData += "<u id=\"" + sSiteGUID + "\"/>";
            sCustomData += string.Format("<up>{0}</up>", sUserIP);
            sCustomData += "<s>" + sSubscriptionCode + "</s>";
            sCustomData += "<cc>";
            sCustomData += Utils.GetCouponDiscountPercent(m_nGroupID, sCouponCode).ToString();
            sCustomData += "</cc>";
            sCustomData += "<p ir=\"" + bIsRecurring.ToString().ToLower() + "\" n=\"1\" o=\"" + nRecPeriods.ToString() + "\"/>";
            sCustomData += "<vlcs>";
            if (theSub != null && theSub.m_oUsageModule != null)
                sCustomData += theSub.m_oUsageModule.m_tsViewLifeCycle.ToString();
            sCustomData += "</vlcs>";
            sCustomData += "<mumlc>";
            if (theSub != null && theSub.m_oSubscriptionUsageModule != null)
                sCustomData += theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle.ToString();
            sCustomData += "</mumlc>";
            sCustomData += "<ppvm>";
            sCustomData += "</ppvm>";
            sCustomData += "<pc>";
            if (theSub != null && theSub.m_oSubscriptionPriceCode != null)
                sCustomData += theSub.m_oSubscriptionPriceCode.m_sCode;
            sCustomData += "</pc>";
            sCustomData += "<pri>";
            sCustomData += dPrice.ToString();
            sCustomData += "</pri>";
            sCustomData += "<cu>";
            sCustomData += sCurrency;
            sCustomData += "</cu>";

            sCustomData += "</customdata>";

            return sCustomData;

        }

        
    }
}
