using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web;
using ConditionalAccess.TvinciUsers;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Data;
using DAL;
using M1BL;
using System.Collections;
using Tvinci.Core.DAL;
using ApiObjects;
using QueueWrapper;
using Newtonsoft.Json;
using ApiObjects.MediaIndexingObjects;
using GroupsCacheManager;
using ConditionalAccess.TvinciPricing;
using ApiObjects.Response;
using ConditionalAccess.Response;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Billing;

namespace ConditionalAccess
{
    public abstract class BaseConditionalAccess
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected string m_sPurchaseMailTemplate;
        protected string m_sMailFromName;
        protected string m_sMailFromAdd;
        protected string m_sMailServer;
        protected string m_sMailServerUN;
        protected string m_sMailServerPass;
        protected string m_sPurchaseMailSubject;
        protected bool m_bIsInitialized;
        protected Int32 m_nGroupID;


        #region Abstract methods
        protected abstract TvinciBilling.BillingResponse HandleBaseRenewMPPBillingCharge(string sSiteGuid, double dPrice,
            string sCurrency, string sUserIP, string sCustomData, int nPaymentNumber, int nRecPeriods, string sExtraParams,
            int nBillingMethod, long lPurchaseID, ConditionalAccess.eBillingProvider bp);

        protected abstract bool HandleMPPRenewalBillingSuccess(string sSiteGUID, string sSubscriptionCode, DateTime dtCurrentEndDate,
            bool bIsPurchasedWithPreviewModule, long lPurchaseID, string sCurrency, double dPrice, int nPaymentNumber,
            string sBillingTransactionID, int nUsageModuleMaxVLC, bool bIsMPPRecurringInfinitely, int nNumOfRecPeriods);

        protected abstract TvinciBilling.BillingResponse HandleCCChargeUser(string sWSUsername, string sWSPassword, string sSiteGuid,
            double dPrice, string sCurrency, string sUserIP, string sCustomData, int nPaymentNumber, int nNumOfPayments,
            string sExtraParams, string sPaymentMethodID, string sEncryptedCVV, bool bIsDummy, bool bIsEntitledToPreviewModule,
            ref TvinciBilling.module bm);

        protected abstract bool UpdatePurchaseIDInBilling(string sWSUsername, string sWSPassword,
          long purchaseID, long billingRefTransactionID, ref TvinciBilling.module wsBillingService);

        protected abstract bool HandleChargeUserForSubscriptionBillingSuccess(string sWSUsername, string sWSPassword, string sSiteGUID, int domianID, TvinciPricing.Subscription theSub,
            double dPrice, string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLanguageCode,
            string sDeviceName, TvinciBilling.BillingResponse br, bool bIsEntitledToPreviewModule, string sSubscriptionCode, string sCustomData,
            bool bIsRecurring, ref long lBillingTransactionID, ref long lPurchaseID, bool isDummy, ref TvinciBilling.module wsBillingService);

        protected abstract bool HandleChargeUserForCollectionBillingSuccess(string sWSUsername, string sWSPassword, string sSiteGUID, int domianID, TvinciPricing.Collection theCol,
            double dPrice, string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLanguageCode,
            string sDeviceName, TvinciBilling.BillingResponse br, string sCollectionCode,
            string sCustomData, ref long lBillingTransactionID, ref long lPurchaseID, ref TvinciBilling.module wsBillingService);

        protected abstract bool HandleChargeUserForMediaFileBillingSuccess(string sWSUsername, string sWSPassword, string sSiteGUID, int domianID, TvinciPricing.Subscription relevantSub,
            double dPrice, string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLanguageCode,
            string sDeviceName, TvinciBilling.BillingResponse br, string sCustomData, TvinciPricing.PPVModule thePPVModule,
            long lMediaFileID, ref long lBillingTransactionID, ref long lPurchaseID, bool isDummy, ref TvinciBilling.module wsBillingService);

        protected abstract bool HandlePPVBillingSuccess(string siteGUID, long houseHoldID, Subscription relevantSub, double price, string currency,
                                                        string coupon, string userIP, string country, string deviceName, long billingTransactionId, string customData,
                                                        PPVModule thePPVModule, int productID, int contentID, string billingGuid, ref long purchaseID);

        protected abstract bool HandleSubscriptionBillingSuccess(string siteGUID, long houseHoldID, Subscription subscription, double price, string currency, string coupon,
                                                                 string userIP, string country, string deviceName, long billingTransactionId, string customData,
                                                                 int productID, string billingGuid, bool isEntitledToPreviewModule, bool isRecurring, ref long purchaseID);

        protected abstract bool HandleCollectionBillingSuccess(string siteGUID, long houseHoldID, Collection collection, double price, string currency, string coupon,
                                                              string userIP, string country, string deviceName, long billingTransactionId, string customData, int productID,
                                                              string billingGuid, bool isEntitledToPreviewModule, ref long purchaseID);


        /*
         * This method was created in order to solve a bug in the flow of ChargeUserForMediaFile in Cinepolis.
         * 1. Cinepolis does not dummy charge their user. All transactions are recorded in their billing gateway,
         *    including transactions for the price of zero.
         * 2. However, since it is not a dummy charge (dummy charge is when you skip the step of contacting the billing gateway)
         *    the flow in the mentioned method did not reach the step where it contacts the billing gateway.
         * 3. This patch resolves this situation without changing any billing logic related to different customers.
         */
        protected bool RecalculateDummyIndicatorForChargeMediaFile(bool bDummy, PriceReason reason, bool bIsCouponUsedAndValid)
        {
            return (bIsCouponUsedAndValid && reason == PriceReason.Free) || bDummy;
        }

        ///// <summary>
        ///// Get Licensed Link
        ///// </summary>
        //protected abstract string GetLicensedLink(string sBasicLink, string sUserIP, string sRefferer);
        /// <summary>
        /// Get Error Licensed Link
        /// </summary>
        protected abstract string GetErrorLicensedLink(string sBasicLink);
        /// <summary>
        /// Activate Campaign
        /// </summary>
        public abstract bool ActivateCampaign(int campaignID, CampaignActionInfo cai);

        public abstract CampaignActionInfo ActivateCampaignWithInfo(int campaignID, CampaignActionInfo cai);

        /// <summary>
        /// Get Licensed Link
        /// </summary>
        protected abstract string GetLicensedLink(int nStreamingCompany, Dictionary<string, string> dParams);
        #endregion

        protected virtual string GetPPVCodeForGetItemsPrices(string ppvObjectCode, string ppvObjectVirtualName)
        {
            return ppvObjectCode;
        }

        protected BaseConditionalAccess() { }

        protected BaseConditionalAccess(Int32 nGroupID)
            : this(nGroupID, string.Empty)
        {

        }

        protected BaseConditionalAccess(Int32 nGroupID, string connKey)
        {
            m_nGroupID = nGroupID;
            m_bIsInitialized = false;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        private void InitializePurchaseMailTemplate(string connectionKey)
        {
            if (m_sPurchaseMailTemplate == null)
                m_sPurchaseMailTemplate = string.Empty;

            string key = string.Format("{0}_InitializeBaseConditionalAccess_{1}", eWSModules.CONDITIONALACCESS.ToString(), m_nGroupID);
            BaseConditionalAccess bCas;
            bool bRes = ConditionalAccessCache.GetItem<BaseConditionalAccess>(key, out bCas);
            if (!bRes || bCas == null)
            {
                lock (m_sPurchaseMailTemplate)
                {
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    if (!string.IsNullOrEmpty(connectionKey))
                    {
                        selectQuery.SetConnectionKey(connectionKey);
                    }
                    selectQuery.SetCachedSec(0);
                    selectQuery += "select * from groups_parameters with (nolock) where status=1 and is_active=1 and ";
                    selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                    selectQuery += " order by id desc";
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            DataRow dr = selectQuery.Table("query").DefaultView[0].Row;
                            m_sPurchaseMailTemplate = ODBCWrapper.Utils.GetSafeStr(dr, "PURCHASE_MAIL");
                            m_sPurchaseMailSubject = ODBCWrapper.Utils.GetSafeStr(dr, "PURCHASE_MAIL_SUBJECT");
                            m_sMailFromName = ODBCWrapper.Utils.GetSafeStr(dr, "MAIL_FROM_NAME");
                            m_sMailFromAdd = ODBCWrapper.Utils.GetSafeStr(dr, "MAIL_FROM_ADD");
                            m_sMailServer = ODBCWrapper.Utils.GetSafeStr(dr, "MAIL_SERVER");
                            m_sMailServerUN = ODBCWrapper.Utils.GetSafeStr(dr, "MAIL_USER_NAME");
                            m_sMailServerPass = ODBCWrapper.Utils.GetSafeStr(dr, "MAIL_PASSWORD");
                            ConditionalAccessCache.AddItem(key, this);
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;
                    m_bIsInitialized = true;
                }
            }
            else
            {
                m_sPurchaseMailTemplate = bCas.m_sPurchaseMailTemplate;
                m_sPurchaseMailSubject = bCas.m_sPurchaseMailSubject;
                m_sMailFromName = bCas.m_sMailFromName;
                m_sMailFromAdd = bCas.m_sMailFromAdd;
                m_sMailServer = bCas.m_sMailServer;
                m_sMailServerUN = bCas.m_sMailServerUN;
                m_sMailServerPass = bCas.m_sMailServerPass;
            }
        }

        protected string GetLogFilename()
        {
            return String.Concat("BaseConditionalAccess_", m_nGroupID);
        }

        private double CalcPriceAfterTax(double catalogPrice, double tax, ref double taxDisc)
        {
            double retVal = 0;
            retVal = Math.Round(catalogPrice / ((100 + tax) / 100), 2);
            taxDisc = Math.Round(catalogPrice - retVal, 2);
            return retVal;
        }
        /// <summary>
        /// Get Purchase Mail Text
        /// </summary>
        protected TvinciAPI.PurchaseMailRequest GetPurchaseMailRequest(ref string sEmail, string sUserGUID, string sItemName,
            string sPaymentMethod, string sDateOfPurchase, string sRecNumner, double dPrice, string sCurrency, Int32 nGroupID)
        {
            InitializePurchaseMailTemplate(string.Empty);
            TvinciAPI.PurchaseMailRequest retVal = new TvinciAPI.PurchaseMailRequest();
            string sFirstName = string.Empty;
            string sLastName = string.Empty;
            using (TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService())
            {
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;

                Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                {
                    u.Url = sWSURL;
                }
                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sUserGUID, string.Empty);
                if (uObj.m_RespStatus == ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    if (uObj.m_user != null)
                    {
                        sEmail = uObj.m_user.m_oBasicData.m_sEmail;
                        if (nGroupID == 109 || nGroupID == 110 || nGroupID == 111 || nGroupID == 112 || nGroupID == 113 || nGroupID == 114)
                        {
                            if (uObj.m_user.m_oDynamicData != null && uObj.m_user.m_oDynamicData.m_sUserData != null)
                            {
                                foreach (UserDynamicDataContainer dynamicData in uObj.m_user.m_oDynamicData.m_sUserData)
                                {
                                    if (dynamicData != null && dynamicData.m_sDataType.Equals("NickName"))
                                    {
                                        sFirstName = dynamicData.m_sValue;
                                        break;
                                    }
                                }
                            }

                        }
                        else
                        {
                            sFirstName = uObj.m_user.m_oBasicData.m_sFirstName;
                        }
                        sLastName = uObj.m_user.m_oBasicData.m_sLastName;
                    }
                }
                double tax = 0;
                double taxDisc = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = null;
                try
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("billing_connection");
                    selectQuery += " select tax_value from groups_parameters with (nolock) where ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        int count = selectQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            object taxObj = selectQuery.Table("query").DefaultView[0].Row["tax_value"];
                            if (taxObj != System.DBNull.Value && taxObj != null)
                            {
                                tax = double.Parse(taxObj.ToString());
                            }
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
                double taxTotalDIsc = CalcPriceAfterTax(dPrice, tax, ref taxDisc);
                retVal.m_eMailType = TvinciAPI.eMailTemplateType.Purchase;
                retVal.m_sFirstName = sFirstName;
                retVal.m_sItemName = sItemName;
                retVal.m_sLastName = sLastName;
                retVal.m_sPaymentMethod = sPaymentMethod;
                retVal.m_sPrice = string.Format("{0:0.##}", dPrice) + " " + sCurrency;
                retVal.m_sPurchaseDate = sDateOfPurchase;
                retVal.m_sSenderFrom = m_sMailFromAdd;
                retVal.m_sSenderName = m_sMailFromName;
                retVal.m_sSubject = m_sPurchaseMailSubject;
                retVal.m_sTemplateName = m_sPurchaseMailTemplate;
                retVal.m_sSenderTo = sEmail;
                retVal.m_sTaxVal = tax.ToString();
                retVal.m_sTaxSubtotal = taxTotalDIsc.ToString();
                retVal.m_sTaxAmount = taxDisc.ToString();
            }

            return retVal;
        }

        /// <summary>
        /// Get Main Lanuage
        /// </summary>
        static protected Int32 GetMainLang(ref string sMainLang, ref string sMainLangCode, Int32 nGroupID)
        {
            Int32 nLangID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select l.NAME,l.CODE3,l.id from groups g with (nolock), lu_languages l with (nolock) where l.id=g.language_id and  ";
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sMainLang = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
                        sMainLangCode = selectQuery.Table("query").DefaultView[0].Row["CODE3"].ToString();
                        nLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
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
            return nLangID;
        }

        /// <summary>
        /// Write To User Log
        /// </summary>
        protected void WriteToUserLog(string sSiteGUID, string sMessage)
        {
            TvinciUsers.UsersService u = null;
            try
            {
                u = new ConditionalAccess.TvinciUsers.UsersService();
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");

                if (!string.IsNullOrEmpty(sWSURL))
                {
                    u.Url = sWSURL;
                }

                if (!string.IsNullOrEmpty(sWSUserName))
                {
                    u.WriteLog(sWSUserName, sWSPass, sSiteGUID, sMessage, "Conditional access module");
                }
            }
            catch (Exception ex)
            {
                log.Error("WriteToUserLog - " + string.Format("Failed to write to user log. Site Guid: {0} , Msg: {1} , Exception msg: {2} , Stack trace : {3}", sSiteGUID, sMessage, ex.Message, ex.StackTrace), ex);
            }
            finally
            {
                if (u != null)
                {
                    u.Dispose();
                }
            }

        }
        /// <summary>
        /// Get Date STR By Group
        /// When we finish the localization feature - remove this! 
        /// </summary>
        protected virtual string GetDateSTRByGroup(DateTime dt, int groupID)
        {
            string retVal = dt.ToString("MM/dd/yyyy HH:mm:ss");
            if (groupID == 109 || groupID == 110 || groupID == 111)
            {
                retVal = dt.ToString("dd/MM/yyyy HH:mm:ss");
            }
            if (groupID == 112 || groupID == 113 || groupID == 114)
            {
                retVal = String.Format("{0} GMT", dt.ToString("dd-MMM-yyyy HH:mm:ss"));
            }
            return retVal;

        }

        protected virtual TvinciBilling.BillingResponse HandleCellularChargeUser(string sWSUsername, string sWSPassword, string sSiteGuid, double dPrice, string sCurrency, string sUserIP, string sCustomData,
                                                                                 int nPaymentNumber, int nNumOfPayments, string sExtraParams, bool bIsDummy, bool bIsEntitledToPreviewModule,
                                                                                 ref TvinciBilling.module bm)
        {
            if (!bIsDummy && !bIsEntitledToPreviewModule)
            {

                //Cellular_ChargeUser
                return bm.Cellular_ChargeUser(sWSUsername, sWSPassword, sSiteGuid, dPrice, sCurrency, sUserIP, sCustomData, 1, 1, sExtraParams);

            }
            else
            {
                return bm.CC_DummyChargeUser(sWSUsername, sWSPassword, sSiteGuid, dPrice, sCurrency, sUserIP, sCustomData, 1, 1, sExtraParams);

            }
        }


        /// <summary>
        /// Credit Card Charge User For PrePaid
        /// </summary>
        public virtual TvinciBilling.BillingResponse CC_ChargeUserForPrePaid(string sSiteGUID, double dPrice, string sCurrency,
           string sPrePaidModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bDummy)
        {
            return CC_BaseChargeUserForPrePaid(sSiteGUID, dPrice, sCurrency, sPrePaidModuleCode,
                sCouponCode, sUserIP, sExtraParameters,
                sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, bDummy);


        }
        /// <summary>
        /// Credit Card Charge User For Pre Paid
        /// </summary>
        protected TvinciBilling.BillingResponse CC_BaseChargeUserForPrePaid(string sSiteGUID, double dPrice, string sCurrency,
            string sPrePaidModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bDummy)
        {
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnown;
            ret.m_sRecieptCode = string.Empty;
            ret.m_sStatusDescription = string.Empty;
            TvinciUsers.UsersService u = null;
            TvinciPricing.mdoule m = null;
            TvinciBilling.module bm = null;
            TvinciAPI.API apiWs = null;
            ODBCWrapper.InsertQuery insertQuery = null;
            ODBCWrapper.UpdateQuery updateQuery = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;

            try
            {
                if (string.IsNullOrEmpty(sSiteGUID))
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                }
                else
                {
                    u = new ConditionalAccess.TvinciUsers.UsersService();
                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }
                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                    }
                    else
                    {
                        if (uObj.m_user != null && uObj.m_user.m_eSuspendState == TvinciUsers.DomainSuspentionStatus.Suspended)
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UserSuspended;
                            ret.m_sRecieptCode = string.Empty;
                            ret.m_sStatusDescription = "User suspended";
                            WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC):" + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }

                        if (!Utils.IsCouponValid(m_nGroupID, sCouponCode))
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = string.Empty;
                            ret.m_sStatusDescription = "Coupon not valid";
                            WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC):" + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }

                        sWSUserName = string.Empty;
                        sWSPass = string.Empty;

                        m = new global::ConditionalAccess.TvinciPricing.mdoule();
                        sWSURL = Utils.GetWSURL("pricing_ws");
                        if (!string.IsNullOrEmpty(sWSURL))
                        {
                            m.Url = sWSURL;
                        }
                        TvinciPricing.PrePaidModule thePrePaidModule = null;

                        Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                        thePrePaidModule = m.GetPrePaidModuleData(sWSUserName, sWSPass, int.Parse(sPrePaidModuleCode), sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                        if (thePrePaidModule == null)
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownPPVModule;
                            ret.m_sRecieptCode = string.Empty;
                            ret.m_sStatusDescription = "This PrePaid Module does not exist ";
                            WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode.ToString() + " error returned: " + ret.m_sStatusDescription);
                        }
                        else
                        {
                            PriceReason theReason = PriceReason.UnKnown;

                            if (thePrePaidModule != null)
                            {
                                TvinciPricing.Price p = Utils.GetPrePaidFinalPrice(m_nGroupID, sPrePaidModuleCode, sSiteGUID, ref theReason, ref thePrePaidModule, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty, sCouponCode);
                                if (theReason == PriceReason.ForPurchase && p.m_dPrice > 0 || bDummy == true)
                                {
                                    if (bDummy || (p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency))
                                    {
                                        string sCustomData = string.Empty;
                                        if (p.m_dPrice != 0 || bDummy)
                                        {
                                            bm = new ConditionalAccess.TvinciBilling.module();
                                            sWSUserName = string.Empty;
                                            sWSPass = string.Empty;
                                            Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                                            sWSURL = Utils.GetWSURL("billing_ws");
                                            if (!string.IsNullOrEmpty(sWSURL))
                                            {
                                                bm.Url = sWSURL;
                                            }
                                            if (string.IsNullOrEmpty(sCountryCd) && !string.IsNullOrEmpty(sUserIP))
                                            {
                                                sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
                                            }

                                            //Create the Custom Data
                                            sCustomData = GetCustomDataForPrePaid(thePrePaidModule, null, sPrePaidModuleCode, string.Empty, sSiteGUID, dPrice, sCurrency, sCouponCode, sUserIP,
                                                sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                                            log.Debug("CustomData - " + sCustomData);

                                            //customdata id
                                            if (!bDummy)
                                                ret = bm.CC_ChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, 1, sExtraParameters, string.Empty, string.Empty);
                                            else
                                                ret = bm.CC_DummyChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, 1, sExtraParameters);
                                        }
                                        if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
                                        {
                                            HandleCouponUses(null, string.Empty, sSiteGUID, p.m_dPrice, sCurrency, 0, sCouponCode, sUserIP,
                                                sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, true, thePrePaidModule.m_ObjectCode, 0);

                                            insertQuery = new ODBCWrapper.InsertQuery("pre_paid_purchases");
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("pre_paid_module_id", "=", int.Parse(sPrePaidModuleCode));
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("total_amount", "=", thePrePaidModule.m_CreditValue.m_oPrise.m_dPrice);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("amount_used", "=", 0);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOM_DATA", "=", sCustomData);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", int.Parse(ret.m_sRecieptCode));
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLANGUAGE_CODE);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDEVICE_NAME);
                                            if (thePrePaidModule != null &&
                                                thePrePaidModule.m_UsageModule != null)
                                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", thePrePaidModule.m_UsageModule.m_nMaxNumberOfViews);
                                            else
                                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);

                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                                            if (thePrePaidModule != null &&
                                                thePrePaidModule.m_UsageModule != null)
                                            {
                                                DateTime d = Utils.GetEndDateTime(DateTime.UtcNow, thePrePaidModule.m_UsageModule.m_tsMaxUsageModuleLifeCycle);
                                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);
                                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", DateTime.UtcNow);
                                            }

                                            insertQuery.Execute();
                                            WriteToUserLog(sSiteGUID, "Pre Paid Module ID: " + sPrePaidModuleCode + " Purchased(CC): " + dPrice.ToString() + sCurrency);
                                            Int32 nPurchaseID = 0;
                                            selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                            selectQuery += " select id from pre_paid_purchases with (nolock) where ";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                                            selectQuery += "and";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pre_paid_module_id", "=", int.Parse(sPrePaidModuleCode));
                                            selectQuery += "and";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                            selectQuery += "and";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                                            selectQuery += "and";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
                                            selectQuery += "and";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("total_amount", "=", thePrePaidModule.m_CreditValue.m_oPrise.m_dPrice);
                                            selectQuery += "and";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("amount_used", "=", 0);
                                            selectQuery += "and";
                                            if (thePrePaidModule != null &&
                                                thePrePaidModule.m_UsageModule != null)
                                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", thePrePaidModule.m_UsageModule.m_nMaxNumberOfViews);
                                            else
                                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);
                                            selectQuery += "and";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                            selectQuery += "and";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                                            selectQuery += "order by id desc";
                                            if (selectQuery.Execute("query", true) != null)
                                            {
                                                Int32 nCount1 = selectQuery.Table("query").DefaultView.Count;
                                                if (nCount1 > 0)
                                                    nPurchaseID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                                            }

                                            //Update PrePaidUses
                                            UserPrePaidContainer uppc = GetUserPrePaidStatus(sSiteGUID, sCurrency);

                                            InsertPPUsesRecord(nPurchaseID, int.Parse(sPrePaidModuleCode), BillingItemsType.PrePaid, sSiteGUID, sCurrency, int.Parse(sPrePaidModuleCode),
                                                nPurchaseID, thePrePaidModule.m_CreditValue.m_oPrise.m_dPrice, (uppc.m_nTotalAmount - uppc.m_nAmountUsed),
                                                sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                                            //Should update the PURCHASE_ID
                                            string sReciept = ret.m_sRecieptCode;
                                            if (sReciept != "")
                                            {
                                                Int32 nID = int.Parse(sReciept);
                                                updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                                                updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                                                updateQuery += "where";
                                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                                                updateQuery.Execute();

                                                try
                                                {
                                                    //send purchase mail
                                                    string sEmail = string.Empty;
                                                    string sPaymentMethod = "Credit Card";
                                                    string sDateOfPurchase = GetDateSTRByGroup(DateTime.UtcNow, m_nGroupID);

                                                    if (!bDummy)
                                                    {
                                                        if (bm == null)
                                                        {
                                                            bm = new ConditionalAccess.TvinciBilling.module();
                                                        }
                                                        sWSUserName = string.Empty;
                                                        sWSPass = string.Empty;
                                                        Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                                                        sWSURL = Utils.GetWSURL("billing_ws");
                                                        if (!string.IsNullOrEmpty(sWSURL))
                                                        {
                                                            bm.Url = sWSURL;
                                                        }
                                                        string sDigits = bm.CC_GetUserCCDigits(sWSUserName, sWSPass, sSiteGUID);
                                                        sPaymentMethod += " (************" + sDigits + ")";
                                                    }
                                                    else
                                                        sPaymentMethod = "Gift";
                                                    TvinciAPI.PurchaseMailRequest sMailReq = GetPurchaseMailRequest(ref sEmail, sSiteGUID, thePrePaidModule.m_Title, sPaymentMethod, sDateOfPurchase, sReciept, dPrice, sCurrency, m_nGroupID);
                                                    apiWs = new TvinciAPI.API();
                                                    string sAPIWSUserName = string.Empty;
                                                    string sAPIWSPass = string.Empty;
                                                    Utils.GetWSCredentials(m_nGroupID, eWSModules.API, ref sWSUserName, ref sWSPass);
                                                    string sAPIWSURL = Utils.GetWSURL("api_ws");
                                                    if (!string.IsNullOrEmpty(sAPIWSURL))
                                                    {
                                                        apiWs.Url = sAPIWSURL;
                                                    }
                                                    apiWs.SendMailTemplate(sAPIWSUserName, sWSPass, sMailReq);

                                                }
                                                catch (Exception ex)
                                                {
                                                    #region Logging
                                                    StringBuilder pmErr = new StringBuilder("Failed to send purchase mail. At CC_ChargeUserForPrePaid. ");
                                                    pmErr.Append(String.Concat(" Site Guid: ", sSiteGUID));
                                                    pmErr.Append(String.Concat(" PP Module Code: ", sPrePaidModuleCode));
                                                    pmErr.Append(String.Concat(" Ex Msg: ", ex.Message));
                                                    pmErr.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                                                    pmErr.Append(String.Concat(" Stack Trace: ", ex.StackTrace));
                                                    log.Debug("Send purchase mail - " + pmErr.ToString(), ex);
                                                    #endregion
                                                }
                                            }
                                            else
                                            {
                                                WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription);
                                            }
                                        }
                                        else
                                        {
                                            WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription);
                                        }
                                    }
                                    else
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                                        ret.m_sRecieptCode = string.Empty;
                                        ret.m_sStatusDescription = "The price of the request is not the actual price";
                                        WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription);
                                    }
                                }
                                else
                                {
                                    if (theReason == PriceReason.PPVPurchased)
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                        ret.m_sRecieptCode = string.Empty;
                                        ret.m_sStatusDescription = "The pre paid module is already purchased";
                                        WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription);
                                    }
                                    else if (theReason == PriceReason.Free)
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                        ret.m_sRecieptCode = string.Empty;
                                        ret.m_sStatusDescription = "The pre paid module is free";
                                        WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription);
                                    }
                                    else if (theReason == PriceReason.ForPurchaseSubscriptionOnly)
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                        ret.m_sRecieptCode = string.Empty;
                                        ret.m_sStatusDescription = "The pre paid module is for purchase with subscription only";
                                        WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription);
                                    }
                                    else if (theReason == PriceReason.SubscriptionPurchased)
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                        ret.m_sRecieptCode = string.Empty;
                                        ret.m_sStatusDescription = "The pre paid module is already purchased (subscription)";
                                        WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription);
                                    }
                                }
                            }
                            else
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "The ppv module is unknown";
                                WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at CC_BaseChargeUserForPrePaid. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" Curr Cd: ", sCurrency));
                sb.Append(String.Concat(" PP Cd: ", sPrePaidModuleCode));
                sb.Append(String.Concat(" Coupon Cd: ", sCouponCode));
                sb.Append(String.Concat(" IP: ", sUserIP));
                sb.Append(String.Concat(" Device Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" Dummy: ", bDummy.ToString().ToLower()));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                }
                if (m != null)
                {
                    m.Dispose();
                }
                if (bm != null)
                {
                    bm.Dispose();
                }
                if (apiWs != null)
                {
                    apiWs.Dispose();
                }
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }

                #endregion
            }
            return ret;
        }

        /// <summary>
        /// InApp Charge User For Media File
        /// </summary>
        protected TvinciBilling.BillingResponse InApp_BaseChargeUserForMediaFile(string sSiteGUID, double dPrice, string sCurrency,
            Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sRecieptCode)
        {
            TvinciBilling.InAppBillingResponse InAppRes = new TvinciBilling.InAppBillingResponse();
            InAppRes.m_oBillingResponse = new TvinciBilling.BillingResponse();
            InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnown;
            InAppRes.m_oBillingResponse.m_sRecieptCode = string.Empty;
            InAppRes.m_oBillingResponse.m_sStatusDescription = string.Empty;
            TvinciUsers.UsersService u = null;
            TvinciPricing.mdoule m = null;
            TvinciBilling.module bm = null;
            ODBCWrapper.InsertQuery insertQuery = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {
                if (string.IsNullOrEmpty(sSiteGUID))
                {
                    InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    InAppRes.m_oBillingResponse.m_sRecieptCode = string.Empty;
                    InAppRes.m_oBillingResponse.m_sStatusDescription = "Cant charge an unknown user";
                    return InAppRes.m_oBillingResponse;
                }
                else
                {
                    u = new ConditionalAccess.TvinciUsers.UsersService();
                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }
                    //get user data
                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        //return UnKnownUser 
                        InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        InAppRes.m_oBillingResponse.m_sRecieptCode = string.Empty;
                        InAppRes.m_oBillingResponse.m_sStatusDescription = "Cant charge an unknown user";
                        return InAppRes.m_oBillingResponse;
                    }
                    else if (uObj != null && uObj.m_user != null && uObj.m_user.m_eSuspendState == TvinciUsers.DomainSuspentionStatus.Suspended)
                    {
                        InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UserSuspended;
                        InAppRes.m_oBillingResponse.m_sRecieptCode = string.Empty;
                        InAppRes.m_oBillingResponse.m_sStatusDescription = "Cannot charge a suspended user";
                        WriteToUserLog(sSiteGUID, "while trying to purchase media file id(InApp): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                        return InAppRes.m_oBillingResponse;
                    }
                    else
                    {
                        sWSUserName = string.Empty;
                        sWSPass = string.Empty;

                        m = new global::ConditionalAccess.TvinciPricing.mdoule();
                        sWSURL = Utils.GetWSURL("pricing_ws");
                        if (!string.IsNullOrEmpty(sWSURL))
                        {
                            m.Url = sWSURL;
                        }
                        if (string.IsNullOrEmpty(sPPVModuleCode))
                        {
                            InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            InAppRes.m_oBillingResponse.m_sRecieptCode = string.Empty;
                            InAppRes.m_oBillingResponse.m_sStatusDescription = "Charge must have ppv module code";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(InApp): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                            return InAppRes.m_oBillingResponse;
                        }

                        // check if ppvModule related to mediaFile 
                        long ppvModuleCode = 0;
                        long.TryParse(sPPVModuleCode, out ppvModuleCode);

                        TvinciPricing.PPVModule thePPVModule = m.ValidatePPVModuleForMediaFile(sWSUserName, sWSPass, nMediaFileID, ppvModuleCode);
                        if (thePPVModule == null)
                        {
                            InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            InAppRes.m_oBillingResponse.m_sRecieptCode = string.Empty;
                            InAppRes.m_oBillingResponse.m_sStatusDescription = "The ppv module is unknown";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(InApp): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                            return InAppRes.m_oBillingResponse;
                        }
                        else if (thePPVModule.m_sObjectCode != ppvModuleCode.ToString())
                        {
                            InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownPPVModule;
                            InAppRes.m_oBillingResponse.m_sRecieptCode = string.Empty;
                            InAppRes.m_oBillingResponse.m_sStatusDescription = "This PPVModule does not belong to item";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(InApp): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                            return InAppRes.m_oBillingResponse;
                        }
                        PriceReason theReason = PriceReason.UnKnown;

                        TvinciPricing.Subscription relevantSub = null;
                        TvinciPricing.Collection relevantCol = null;
                        TvinciPricing.PrePaidModule relevantPP = null;

                        TvinciPricing.Price p = Utils.GetMediaFileFinalPriceForNonGetItemsPrices(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (theReason == PriceReason.ForPurchase || (theReason == PriceReason.SubscriptionPurchased && p.m_dPrice > 0))
                        {
                            if (p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency)
                            {
                                string sCustomData = "";
                                if (p.m_dPrice != 0)
                                {
                                    #region Init Tvinci Billing Webservice
                                    bm = new ConditionalAccess.TvinciBilling.module();
                                    sWSUserName = string.Empty;
                                    sWSPass = string.Empty;
                                    Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                                    sWSURL = Utils.GetWSURL("billing_ws");
                                    if (!string.IsNullOrEmpty(sWSURL))
                                    {
                                        bm.Url = sWSURL;
                                    }
                                    #endregion

                                    if (string.IsNullOrEmpty(sCountryCd) && !string.IsNullOrEmpty(sUserIP))
                                    {
                                        sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
                                    }

                                    //Create the Custom Data
                                    sCustomData = GetCustomData(relevantSub, thePPVModule, null, sSiteGUID, dPrice, sCurrency,
                                        nMediaFileID, nMediaID, sPPVModuleCode, string.Empty, sCouponCode, sUserIP,
                                        sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                                    log.Debug("CustomData - " + sCustomData);

                                    //customdata id
                                    InAppRes = bm.InApp_ChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, 1, sRecieptCode);
                                }

                                if (InAppRes.m_oBillingResponse.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
                                {
                                    Int32 nReciptCode = 0;
                                    if (!string.IsNullOrEmpty(InAppRes.m_oBillingResponse.m_sRecieptCode))
                                    {
                                        nReciptCode = int.Parse(InAppRes.m_oBillingResponse.m_sRecieptCode);
                                    }

                                    HandleCouponUses(relevantSub, string.Empty, sSiteGUID, p.m_dPrice, sCurrency, nMediaFileID, sCouponCode, sUserIP,
                                        sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, true, 0, 0);

                                    #region Insert - ppv_purchases
                                    insertQuery = new ODBCWrapper.InsertQuery("ppv_purchases");
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                                    if (relevantSub != null)
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", relevantSub.m_sObjectCode);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrency);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", sCustomData);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", nReciptCode);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLANGUAGE_CODE);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDEVICE_NAME);
                                    if (thePPVModule != null &&
                                        thePPVModule.m_oUsageModule != null)
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", thePPVModule.m_oUsageModule.m_nMaxNumberOfViews);
                                    else
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                                    if (thePPVModule != null &&
                                        thePPVModule.m_oUsageModule != null)
                                    {
                                        DateTime d = Utils.GetEndDateTime(DateTime.UtcNow, thePPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);
                                    }

                                    insertQuery.Execute();
                                    WriteToUserLog(sSiteGUID, "Media file id: " + nMediaFileID.ToString() + " Purchased(CC): " + dPrice.ToString() + sCurrency);
                                    #endregion
                                    #region Select - ppv_purchases the current purchase
                                    Int32 nPurchaseID = 0;
                                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                    selectQuery += " select id from ppv_purchases with (nolock) where ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);

                                    if (relevantSub != null)
                                    {
                                        selectQuery += "and";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", relevantSub.m_sObjectCode);
                                    }
                                    selectQuery += "and";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                                    selectQuery += "and";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                    selectQuery += "and";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                                    selectQuery += "and";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrency);
                                    selectQuery += "and";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                    selectQuery += "and";
                                    if (thePPVModule != null &&
                                        thePPVModule.m_oUsageModule != null)
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", thePPVModule.m_oUsageModule.m_nMaxNumberOfViews);
                                    else
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);
                                    selectQuery += "and";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                    selectQuery += "and";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                                    selectQuery += "order by id desc";
                                    if (selectQuery.Execute("query", true) != null)
                                    {
                                        Int32 nCount1 = selectQuery.Table("query").DefaultView.Count;
                                        if (nCount1 > 0)
                                            nPurchaseID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                                    }
                                    #endregion

                                    //Should update the PURCHASE_ID

                                    string sReciept = InAppRes.m_oBillingResponse.m_sRecieptCode;
                                    if (sReciept != "")
                                    {
                                        Int32 nID = int.Parse(sReciept);
                                        updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                                        updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                                        updateQuery += "where";
                                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                                        updateQuery.Execute();
                                    }
                                    else
                                    {
                                        WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                                    }
                                }
                            }
                            else
                            {
                                InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                                InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                InAppRes.m_oBillingResponse.m_sStatusDescription = "The price of the request is not the actual price";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(InAPP): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                            }
                        }
                        else
                        {
                            if (theReason == PriceReason.PPVPurchased)
                            {
                                InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                InAppRes.m_oBillingResponse.m_sStatusDescription = "The media file is already purchased";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(InApp): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                            }
                            else if (theReason == PriceReason.Free)
                            {
                                InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                InAppRes.m_oBillingResponse.m_sStatusDescription = "The media file is free";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(InApp): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                            }
                            else if (theReason == PriceReason.ForPurchaseSubscriptionOnly)
                            {
                                InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                InAppRes.m_oBillingResponse.m_sStatusDescription = "The media file is for purchase with subscription only";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(InApp): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                            }
                            else if (theReason == PriceReason.SubscriptionPurchased)
                            {
                                InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                InAppRes.m_oBillingResponse.m_sStatusDescription = "The media file is already purchased (subscription)";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(InApp): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                            }
                            else if (theReason == PriceReason.NotForPurchase)
                            {
                                InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                InAppRes.m_oBillingResponse.m_sRecieptCode = string.Empty;
                                InAppRes.m_oBillingResponse.m_sStatusDescription = "The media file is not valid for purchased";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(InApp): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at InApp_BaseChargeUserForMediaFile. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" MF ID: ", nMediaFileID));
                sb.Append(String.Concat(" M ID: ", nMediaID));
                sb.Append(String.Concat(" PPV M C: ", sPPVModuleCode));
                sb.Append(String.Concat(" Coupon: ", sCouponCode));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                }
                if (m != null)
                {
                    m.Dispose();
                }
                if (bm != null)
                {
                    bm.Dispose();
                }
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
                #endregion
            }
            return InAppRes.m_oBillingResponse;
        }



        protected DateTime CalcSubscriptionEndDate(TvinciPricing.Subscription sub, bool bIsEntitledToPreviewModule, DateTime dtToInitializeWith)
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

        protected DateTime CalcCollectionEndDate(TvinciPricing.Collection col, DateTime dtToInitializeWith)
        {
            DateTime res = dtToInitializeWith;
            if (col != null)
            {
                if (col.m_oCollectionUsageModule != null)
                {
                    // calc end date as before.
                    res = Utils.GetEndDateTime(res, col.m_oCollectionUsageModule.m_tsMaxUsageModuleLifeCycle);
                }
            }
            return res;
        }

        /// <summary>
        /// In App Charge User For Subscription
        /// </summary>
        protected TvinciBilling.BillingResponse InApp_BaseChargeUserForSubscription(string sSiteGUID, double dPrice, string sCurrency, string sProductCode, string sUserIP, string sExtraParams,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string ReceiptData)
        {

            TvinciBilling.InAppBillingResponse InAppRes = new TvinciBilling.InAppBillingResponse();
            InAppRes.m_oBillingResponse = new TvinciBilling.BillingResponse();

            TvinciUsers.UsersService u = null;
            TvinciBilling.module bm = null;
            ODBCWrapper.DataSetSelectQuery selectExistQuery = null;
            ODBCWrapper.UpdateQuery updateQuery = null;
            ODBCWrapper.InsertQuery insertQuery = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            ODBCWrapper.UpdateQuery updateQuery1 = null;

            try
            {
                if (string.IsNullOrEmpty(sSiteGUID))
                {
                    InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    InAppRes.m_oBillingResponse.m_sRecieptCode = string.Empty;
                    InAppRes.m_oBillingResponse.m_sStatusDescription = "Cant charge an unknown user";
                }
                else
                {
                    u = new ConditionalAccess.TvinciUsers.UsersService();

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }
                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        InAppRes.m_oBillingResponse.m_sRecieptCode = string.Empty;
                        InAppRes.m_oBillingResponse.m_sStatusDescription = "Cant charge an unknown user";
                    }
                    else
                    {
                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.Subscription theSub = Utils.GetSubscriptionBytProductCode(m_nGroupID, sProductCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);
                        string sSubscriptionCode = theSub.m_SubscriptionCode;

                        TvinciPricing.Price p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubscriptionCode, sSiteGUID, string.Empty, ref theReason, ref theSub, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                        if (theReason == PriceReason.ForPurchase)
                        {
                            if (p != null && dPrice > 0)
                            {
                                string sCustomData = string.Empty;
                                dPrice = p.m_dPrice;
                                sCurrency = p.m_oCurrency.m_sCurrencyCD3;

                                bool bIsRecurring = theSub.m_bIsRecurring;
                                Int32 nRecPeriods = theSub.m_nNumberOfRecPeriods;

                                if (string.IsNullOrEmpty(sCountryCd) && !string.IsNullOrEmpty(sUserIP))
                                {
                                    sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
                                }
                                //Create the Custom Data
                                sCustomData = GetCustomDataForSubscription(theSub, null, sSubscriptionCode, string.Empty, sSiteGUID, dPrice, sCurrency,
                                    string.Empty, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                                log.Debug("CustomData - " + sCustomData);

                                if (p.m_dPrice != 0)
                                {
                                    bm = new ConditionalAccess.TvinciBilling.module();
                                    sWSUserName = string.Empty;
                                    sWSPass = string.Empty;
                                    Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                                    sWSURL = Utils.GetWSURL("billing_ws");
                                    if (!string.IsNullOrEmpty(sWSURL))
                                    {
                                        bm.Url = sWSURL;
                                    }
                                    InAppRes = bm.InApp_ChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, nRecPeriods, ReceiptData);
                                }

                                if (InAppRes.m_oBillingResponse.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
                                {
                                    try
                                    {
                                        Int32 nReciptCode = 0;
                                        if (!string.IsNullOrEmpty(InAppRes.m_oBillingResponse.m_sRecieptCode))
                                        {
                                            nReciptCode = int.Parse(InAppRes.m_oBillingResponse.m_sRecieptCode);
                                        }
                                        Int32 nRet = 0;
                                        selectExistQuery = new ODBCWrapper.DataSetSelectQuery();
                                        selectExistQuery += " select id from subscriptions_purchases with (nolock) where ";
                                        selectExistQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", nReciptCode);
                                        selectExistQuery += "AND";
                                        selectExistQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                                        selectExistQuery += "AND";
                                        selectExistQuery += ODBCWrapper.Parameter.NEW_PARAM("site_user_guid", "=", sSiteGUID);

                                        if (selectExistQuery.Execute("query", true) != null)
                                        {
                                            Int32 nCount = selectExistQuery.Table("query").DefaultView.Count;
                                            if (nCount > 0)
                                                nRet = int.Parse(selectExistQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                                        }

                                        if (nRet == 0)
                                        {
                                            HandleCouponUses(theSub, string.Empty, sSiteGUID, dPrice, sCurrency, 0, string.Empty, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, true, 0, 0);

                                            updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                                            updateQuery += " where ";
                                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                                            updateQuery += " and ";
                                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                                            updateQuery += " and ";
                                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                            updateQuery.Execute();

                                            DateTime dt1970 = new DateTime(1970, 1, 1);

                                            // by default add 6 hours to end date, so that renewer scheduler will work appropiately 
                                            bool shouldAddTimeToEndDate = true;

                                            insertQuery = new ODBCWrapper.InsertQuery("subscriptions_purchases");
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrency);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", sCustomData);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLANGUAGE_CODE);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDEVICE_NAME);
                                            if (theSub != null &&
                                                theSub.m_oUsageModule != null)
                                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", theSub.m_oUsageModule.m_nMaxNumberOfViews);
                                            else
                                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);
                                            if (theSub != null &&
                                                theSub.m_oUsageModule != null)
                                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", theSub.m_oUsageModule.m_tsViewLifeCycle);
                                            else
                                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", 0);
                                            if (bIsRecurring == true)
                                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 1);
                                            else
                                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", nReciptCode);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);

                                            DateTime startDate = DateTime.MinValue;
                                            DateTime endDate = DateTime.MinValue;

                                            // First try with iOs 6
                                            if (InAppRes.m_oInAppReceipt.iOSVersion == "6")
                                            {
                                                // If we have only one latest receipt info (in iOS 6, it should be)
                                                if (InAppRes.m_oInAppReceipt.latest_receipt_info != null &&
                                                    InAppRes.m_oInAppReceipt.latest_receipt_info.Length == 1)
                                                {
                                                    ConditionalAccess.TvinciBilling.iTunesReceipt latestReceiptInfo = InAppRes.m_oInAppReceipt.latest_receipt_info[0];

                                                    // Use expires_date (which is in MS in iOs 6) and purchase_date_ms to find start/end dates
                                                    if (!string.IsNullOrEmpty(latestReceiptInfo.expires_date) &&
                                                        !string.IsNullOrEmpty(latestReceiptInfo.purchase_date_ms))
                                                    {
                                                        double startMS = double.Parse(latestReceiptInfo.purchase_date_ms);
                                                        double endMS = double.Parse(latestReceiptInfo.expires_date);

                                                        startDate = dt1970.AddMilliseconds(startMS);
                                                        endDate = dt1970.AddMilliseconds(endMS);
                                                    }
                                                }

                                                // If we couldn't find the dates with latest receipt info,
                                                // Use receipt
                                                if (startDate == DateTime.MinValue || endDate == DateTime.MinValue)
                                                {
                                                    double startMS = double.Parse(InAppRes.m_oInAppReceipt.receipt.purchase_date_ms);
                                                    double endMS = double.Parse(InAppRes.m_oInAppReceipt.receipt.expires_date);

                                                    startDate = dt1970.AddMilliseconds(startMS);
                                                    endDate = dt1970.AddMilliseconds(endMS);
                                                }
                                            }
                                            // Then try with iOS 7
                                            else if (InAppRes.m_oInAppReceipt.iOSVersion == "7")
                                            {
                                                if (InAppRes.m_oInAppReceipt.latest_receipt_info != null && InAppRes.m_oInAppReceipt.latest_receipt_info.Length > 0)
                                                {
                                                    double startMS = 0;
                                                    double endMS = 0;

                                                    // Run on all latest receipts and find the one that matches the date and the product id
                                                    foreach (var lastReceipt in InAppRes.m_oInAppReceipt.latest_receipt_info)
                                                    {
                                                        // If the product code matches
                                                        if (lastReceipt.product_id == sProductCode)
                                                        {
                                                            double currentStartMS;
                                                            double currentEndMS;

                                                            // Find the maximum start date
                                                            double.TryParse(lastReceipt.purchase_date_ms, out currentStartMS);
                                                            double.TryParse(lastReceipt.expires_date_ms, out currentEndMS);

                                                            if (currentStartMS > startMS)
                                                            {
                                                                startMS = currentStartMS;
                                                                endMS = currentEndMS;
                                                            }
                                                        }
                                                    }

                                                    // If we found a receipt with the matching product code and good purchase date
                                                    if (startMS > 0)
                                                    {
                                                        startDate = dt1970.AddMilliseconds(startMS);

                                                        if (endMS > 0)
                                                        {
                                                            endDate = dt1970.AddMilliseconds(endMS);
                                                        }
                                                        else
                                                        {
                                                            if (theSub != null && theSub.m_oSubscriptionUsageModule != null &&
                                                                theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle > 0)
                                                            {
                                                                // Set end date according to subscription's usage module full life cycle
                                                                // This data is in MINUTES
                                                                endDate = startDate.AddMinutes(theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle);

                                                                shouldAddTimeToEndDate = false;
                                                            }
                                                        }
                                                    }
                                                }

                                                // If we don't have a start or end date, check receipt
                                                if (startDate == DateTime.MinValue &&
                                                    endDate == DateTime.MinValue)
                                                {
                                                    if (InAppRes.m_oInAppReceipt.receipt != null)
                                                    {
                                                        double startMS;
                                                        double endMS;

                                                        double.TryParse(InAppRes.m_oInAppReceipt.receipt.purchase_date_ms, out startMS);
                                                        double.TryParse(InAppRes.m_oInAppReceipt.receipt.expires_date_ms, out endMS);

                                                        // If we found a receipt with the matching product code and good purchase date
                                                        if (startMS > 0)
                                                        {
                                                            startDate = dt1970.AddMilliseconds(startMS);

                                                            if (endMS > 0)
                                                            {
                                                                endDate = dt1970.AddMilliseconds(endMS);
                                                            }
                                                            else
                                                            {
                                                                if (theSub != null && theSub.m_oSubscriptionUsageModule != null &&
                                                                    theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle > 0)
                                                                {
                                                                    // Set end date according to subscription's usage module full life cycle
                                                                    // This data is in MINUTES
                                                                    endDate = startDate.AddMinutes(theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle);

                                                                    shouldAddTimeToEndDate = false;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                // If we don't have a start or end date, check in_app
                                                if (startDate == DateTime.MinValue &&
                                                    endDate == DateTime.MinValue)
                                                {
                                                    if (InAppRes.m_oInAppReceipt.in_app != null && InAppRes.m_oInAppReceipt.in_app.Length > 0)
                                                    {
                                                        double startMS = 0;
                                                        double endMS = 0;

                                                        // Run on all latest receipts and find the one that matches the date and the product id
                                                        foreach (var lastReceipt in InAppRes.m_oInAppReceipt.in_app)
                                                        {
                                                            // If the product code matches
                                                            if (lastReceipt.product_id == sProductCode)
                                                            {
                                                                // Find the maximum start date
                                                                double currentStartMS = double.Parse(lastReceipt.purchase_date_ms);
                                                                double currentEndMS;

                                                                if (currentStartMS > startMS)
                                                                {
                                                                    startMS = currentStartMS;

                                                                    // End date is optional here, so try parse it
                                                                    if (!string.IsNullOrEmpty(lastReceipt.expires_date_ms) &&
                                                                        double.TryParse(lastReceipt.expires_date_ms, out currentEndMS))
                                                                    {
                                                                        endMS = currentEndMS;
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        // If we found a receipt with the matching product code and good purchase date
                                                        if (startMS > 0)
                                                        {
                                                            startDate = dt1970.AddMilliseconds(startMS);
                                                            endDate = dt1970.AddMilliseconds(endMS);
                                                        }

                                                        // If we don't have an end date
                                                        if (endDate == DateTime.MinValue || endDate == dt1970)
                                                        {
                                                            if (theSub != null && theSub.m_oSubscriptionUsageModule != null &&
                                                                theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle > 0)
                                                            {
                                                                // Set end date according to subscription's usage module full life cycle
                                                                // This data is in MINUTES
                                                                endDate = startDate.AddMinutes(theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle);

                                                                shouldAddTimeToEndDate = false;
                                                            }
                                                            else
                                                            {
                                                                endDate = dt1970;
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            // If we don't have a start or end date
                                            if (startDate == DateTime.MinValue ||
                                                endDate == DateTime.MinValue)
                                            {
                                                InAppRes.m_oBillingResponse.m_sStatusDescription = "Something went wrong with start and end date";

                                                startDate = dt1970;
                                                endDate = dt1970;
                                            }

                                            if (shouldAddTimeToEndDate)
                                            {
                                                endDate = endDate.AddHours(6);
                                            }

                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", endDate);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", startDate);
                                            insertQuery.Execute();

                                            Int32 nPurchaseID = 0;
                                            WriteToUserLog(sSiteGUID, "Subscription purchase (CC): " + sSubscriptionCode);
                                            selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                            selectQuery += " select id from subscriptions_purchases where ";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                                            selectQuery += " and ";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                                            selectQuery += " and ";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                            selectQuery += " and ";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                                            selectQuery += " and ";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrency);
                                            selectQuery += " and ";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                            selectQuery += " and ";
                                            if (theSub != null &&
                                                theSub.m_oUsageModule != null)
                                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", theSub.m_oUsageModule.m_nMaxNumberOfViews);
                                            else
                                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);
                                            selectQuery += " and ";
                                            if (theSub != null &&
                                                theSub.m_oUsageModule != null)
                                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", theSub.m_oUsageModule.m_tsViewLifeCycle);
                                            else
                                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", 0);

                                            selectQuery += " and ";
                                            if (bIsRecurring)
                                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 1);
                                            else
                                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                                            selectQuery += " and ";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                            selectQuery += " and ";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                                            selectQuery += " order by id desc";
                                            if (selectQuery.Execute("query", true) != null)
                                            {
                                                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                                                if (nCount > 0)
                                                {
                                                    nPurchaseID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                                                }
                                            }

                                            if (nReciptCode > 0)
                                            {
                                                updateQuery1 = new ODBCWrapper.UpdateQuery("billing_transactions");
                                                updateQuery1.SetConnectionKey("MAIN_CONNECTION_STRING");
                                                updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                                                updateQuery1 += "where";
                                                updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nReciptCode);
                                                updateQuery1.Execute();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        #region Logging
                                        StringBuilder sb = new StringBuilder("Exception at InApp_BaseChargeUserForSubscription. ");
                                        sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                                        sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                                        sb.Append(String.Concat(" Price: ", dPrice));
                                        sb.Append(String.Concat(" Product Cd: ", sProductCode));
                                        sb.Append(String.Concat(" Curr Cd: ", sCurrency));
                                        sb.Append(String.Concat(" IP: ", sUserIP));
                                        sb.Append(String.Concat(" Device Name: ", sDEVICE_NAME));
                                        sb.Append(String.Concat(" this is: ", this.GetType().Name));
                                        sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                                        sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));

                                        log.Error("Exception - " + sb.ToString(), ex);
                                        #endregion

                                        InAppRes.m_oBillingResponse.m_oStatus = TvinciBilling.BillingResponseStatus.Fail;
                                        InAppRes.m_oBillingResponse.m_sStatusDescription = "Failed saving subscription purchase";
                                    }
                                }
                                else
                                {
                                    InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                    InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                    InAppRes.m_oBillingResponse.m_sStatusDescription = InAppRes.m_oBillingResponse.m_sStatusDescription;
                                    WriteToUserLog(sSiteGUID, "while trying to purchase subscription(InApp): " + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                                }
                            }
                            else
                            {
                                InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                                InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                InAppRes.m_oBillingResponse.m_sStatusDescription = "The price of the request is not the actual price";
                                WriteToUserLog(sSiteGUID, "while trying to purchase subscription(InApp): " + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                            }
                        }
                        else
                        {
                            if (theReason == PriceReason.Free)
                            {
                                InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                InAppRes.m_oBillingResponse.m_sStatusDescription = "The subscription is free";
                                WriteToUserLog(sSiteGUID, "while trying to purchase subscription(InApp): " + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                            }
                            if (theReason == PriceReason.SubscriptionPurchased)
                            {
                                InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                InAppRes.m_oBillingResponse.m_sStatusDescription = "The subscription is already purchased";
                                WriteToUserLog(sSiteGUID, "while trying to purchase subscription(InApp): " + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                            }
                            if (theReason == PriceReason.UnKnown)
                            {
                                InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                InAppRes.m_oBillingResponse.m_sStatusDescription = "Error Unkown";
                                WriteToUserLog(sSiteGUID, "while trying to purchase subscription(InApp): " + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at InApp_BaseChargeUserForSubscription. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" Product Cd: ", sProductCode));
                sb.Append(String.Concat(" Curr Cd: ", sCurrency));
                sb.Append(String.Concat(" IP: ", sUserIP));
                sb.Append(String.Concat(" Device Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion

                InAppRes.m_oBillingResponse.m_oStatus = TvinciBilling.BillingResponseStatus.Fail;
                InAppRes.m_oBillingResponse.m_sStatusDescription = "Internal error";

            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                }
                if (bm != null)
                {
                    bm.Dispose();
                }
                if (selectExistQuery != null)
                {
                    selectExistQuery.Finish();
                }
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
                if (updateQuery1 != null)
                {
                    updateQuery1.Finish();
                }
                #endregion
            }
            return InAppRes.m_oBillingResponse;
        }

        /// <summary>
        /// In App Charge User For Subscription
        /// </summary>
        public virtual TvinciBilling.BillingResponse InApp_ChargeUserForSubscription(string sSiteGUID, double dPrice, string sCurrency, string sProductCode, string sUserIP, string sExtraParams,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string ReceiptData)
        {
            return InApp_BaseChargeUserForSubscription(sSiteGUID, dPrice, sCurrency, sProductCode, sUserIP, sExtraParams, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, ReceiptData);
        }
        /// <summary>
        /// Renew Cacled Subscription
        /// </summary>
        public bool RenewCacledSubscription(string sSiteGUID, string sSubscriptionCode, Int32 nSubscriptionPurchaseID)
        {
            bool bRet = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            ODBCWrapper.UpdateQuery updateQuery = null;
            ODBCWrapper.InsertQuery insertQuery = null;
            try
            {
                PriceReason theReason = PriceReason.UnKnown;
                TvinciPricing.Subscription theSub = null;
                TvinciPricing.Price p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubscriptionCode, sSiteGUID, string.Empty, ref theReason, ref theSub, string.Empty, string.Empty, string.Empty);
                bool bIsRecurring = false;
                if (theSub != null && theSub.m_oUsageModule != null)
                    bIsRecurring = theSub.m_bIsRecurring;
                if (bIsRecurring)
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select * from subscriptions_purchases with (nolock) where IS_ACTIVE=1 and STATUS=1 and RECURRING_RUNTIME_STATUS=0 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSubscriptionPurchaseID);
                    if (m_nGroupID != 0)
                    {
                        selectQuery += "and";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                    }
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            Int32 nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                            updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 1);
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                            updateQuery.Execute();
                            bRet = true;

                            //Insert renew subscription row
                            insertQuery = new ODBCWrapper.InsertQuery("subscriptions_status_changes");
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NEW_RENEWABLE_STATUS", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", "");
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", "");
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", "");
                            insertQuery.Execute();


                            //Write to users log
                            WriteToUserLog(sSiteGUID, "Subscription: " + sSubscriptionCode.ToString() + " renew activated");
                        }
                    }

                }
            }
            finally
            {
                #region Disposing
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
                #endregion
            }
            return bRet;
        }

        /// <summary>
        /// Cancel a household service subscription at the next renewal. The subscription stays valid till the next renewal.
        /// </summary>
        /// <param name="sSiteGUID"></param>
        /// <param name="sSubscriptionCode"></param>
        /// <param name="nSubscriptionPurchaseID"></param>
        /// <returns></returns>
        public virtual bool CancelSubscription(string sSiteGUID, string sSubscriptionCode, Int32 nSubscriptionPurchaseID)
        {
            bool bRet = false;
            TvinciPricing.Subscription theSub = null;
            PriceReason theReason = PriceReason.UnKnown;
            try
            {
                TvinciPricing.Price p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubscriptionCode, sSiteGUID, string.Empty, ref theReason, ref theSub, "", "", "");

                if (theSub != null && theSub.m_oUsageModule != null && theSub.m_bIsRecurring)
                {
                    DataTable dt = ConditionalAccessDAL.GetSubscriptionPurchaseID(nSubscriptionPurchaseID);
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        Int32 nID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["ID"]);

                        bRet = ConditionalAccessDAL.CancelSubscription(nID, m_nGroupID, sSiteGUID, sSubscriptionCode) > 0;
                        if (bRet)
                        {
                            WriteToUserLog(sSiteGUID,
                                String.Concat("Sub ID: ", sSubscriptionCode, " with Purchase ID: ", nSubscriptionPurchaseID, " has been canceled."));
                        }
                        else
                        {
                            #region Logging
                            StringBuilder sb = new StringBuilder("CancelSubscription. Probably failed to cancel subscription against DB. ");
                            sb.Append(String.Concat("Site Guid: ", sSiteGUID));
                            sb.Append(String.Concat(" Sub Code: ", sSubscriptionCode));
                            sb.Append(String.Concat(" Sub Purchase ID: ", nSubscriptionPurchaseID));

                            log.Error("Error - " + sb.ToString());
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at CancelSubscriptionRenewal. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Sub Code: ", sSubscriptionCode));
                sb.Append(String.Concat(" Sub Purchase ID: ", nSubscriptionPurchaseID));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            return bRet;
        }
        /// <summary>
        /// Cancel a household service subscription at the next renewal. The subscription stays valid till the next renewal.
        /// </summary>
        /// <param name="p_nDomainId"></param>
        /// <param name="p_sSubscriptionCode"></param>
        /// <returns></returns>
        public virtual ApiObjects.Response.Status CancelSubscriptionRenewal(int p_nDomainId, string p_sSubscriptionCode)
        {
            ApiObjects.Response.Status oResult = new ApiObjects.Response.Status();
            bool bResult = false;

            try
            {
                // Get domain info - both for validation and for getting users in domain
                TvinciDomains.Domain oDomain = Utils.GetDomainInfo(p_nDomainId, this.m_nGroupID);

                // Check if the domain is OK
                if (oDomain == null || oDomain.m_DomainStatus != TvinciDomains.DomainStatus.OK)
                {
                    if (oDomain.m_DomainStatus == TvinciDomains.DomainStatus.DomainSuspended)
                    {
                        oResult.Code = (int)eResponseStatus.DomainSuspended;
                        oResult.Message = "Domain suspended";
                    }
                    else
                    {
                        oResult.Code = (int)eResponseStatus.DomainNotExists;
                        oResult.Message = "Domain doesn't exist";
                    }
                }
                else
                {
                    int[] arrUsers = oDomain.m_UsersIDs;

                    DataRow drUserPurchase = GetSubscriptionPurchaseRow(p_sSubscriptionCode, arrUsers);

                    // If all of the users didn't purchase this subscription
                    if (drUserPurchase == null)
                    {
                        oResult.Code = (int)eResponseStatus.InvalidPurchase;
                        oResult.Message = "Subscription is not permitted for this domain";
                    }
                    else
                    {
                        int nPurchaseID = ODBCWrapper.Utils.ExtractInteger(drUserPurchase, "ID");
                        int nIsRecurringStatus = ODBCWrapper.Utils.ExtractInteger(drUserPurchase, "IS_RECURRING_STATUS");
                        string sPurchasingSiteGuid = ODBCWrapper.Utils.ExtractValue<string>(drUserPurchase, "SITE_USER_GUID");

                        // If the subscription is not recurring already
                        if (nIsRecurringStatus != 1)
                        {
                            oResult.Code = (int)eResponseStatus.SubscriptionNotRenewable;
                            oResult.Message = "Subscription already does not renew";
                        }
                        else
                        {
                            // Try to cancel subscription
                            bResult = ConditionalAccessDAL.CancelSubscription(nPurchaseID, m_nGroupID, sPurchasingSiteGuid, p_sSubscriptionCode) > 0;

                            if (bResult)
                            {
                                // site guid of purchasing user
                                WriteToUserLog(sPurchasingSiteGuid,
                                    String.Concat("Sub ID: ", p_sSubscriptionCode, " with Purchase ID: ",
                                    ODBCWrapper.Utils.ExtractInteger(drUserPurchase, "ID"), " has been canceled."));

                                oResult.Code = (int)eResponseStatus.OK;
                                oResult.Message = "Subscription renewal cancelled";

                                DateTime dtServiceEndDate = ODBCWrapper.Utils.ExtractDateTime(drUserPurchase, "END_DATE");

                                // Fire event that action occurred
                                Dictionary<string, object> dicData = new Dictionary<string, object>()
                                    {
                                        {"DomainId", p_nDomainId},
                                        {"ServiceID", p_sSubscriptionCode},
                                        {"ServiceEndDate", dtServiceEndDate}
                                    };

                                EnqueueEventRecord(NotifiedAction.CancelDomainSubscriptionRenewal, dicData);
                            }
                            else
                            {
                                #region Logging
                                StringBuilder sb = new StringBuilder("CancelSubscriptionRenewal. Probably failed to cancel subscription on DB. ");
                                sb.Append(String.Concat("Domain Id: ", p_nDomainId));
                                sb.Append(String.Concat(" Sub Code: ", p_sSubscriptionCode));

                                log.Error("Error - " + sb.ToString());
                                #endregion

                                oResult.Code = (int)eResponseStatus.Error;
                                oResult.Message = "Error while cancelling";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at CancelSubscriptionRenewal. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Domain Id: ", p_nDomainId));
                sb.Append(String.Concat(" Sub Code: ", p_sSubscriptionCode));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion

                oResult.Code = (int)eResponseStatus.Error;
                oResult.Message = "Unexpected error occurred";
            }

            return oResult;
        }

        /// <summary>
        /// Tells whether the users purchased this subscription or not.
        /// Also gets the user permitted subscriptions' purchases
        /// </summary>
        /// <param name="p_sSubscriptionCode"></param>
        /// <param name="p_arrUsers"></param>
        /// <returns></returns>
        private bool IsSubscriptionPermittedForUsers(string p_sSubscriptionCode, int[] p_arrUsers, out DataTable p_dtUserPurchases)
        {
            bool bResult = false;

            p_dtUserPurchases = ConditionalAccessDAL.Get_UsersPermittedSubscriptions(p_arrUsers.ToList(), false);

            // If there is at least one valid subscription
            if (p_dtUserPurchases != null && p_dtUserPurchases.Rows != null && p_dtUserPurchases.Rows.Count > 0)
            {
                // Run on all purchases until a match is found
                foreach (DataRow drUserPurchase in p_dtUserPurchases.Rows)
                {
                    // If this it the subscription we are looking for
                    if (p_sSubscriptionCode == ODBCWrapper.Utils.ExtractString(drUserPurchase, "SUBSCRIPTION_CODE"))
                    {
                        object oCancellationDate = drUserPurchase["CANCELLATION_DATE"];

                        // Check if the subscription is not cancelled
                        if ((oCancellationDate == null) || (oCancellationDate == DBNull.Value))
                        {
                            bResult = true;
                            break;
                        }
                    }
                }
            }

            return (bResult);
        }

        /// <summary>
        /// Gets the subscription purchase row of the given subscription by any of the given users
        /// </summary>
        /// <param name="p_sSubscriptionCode"></param>
        /// <param name="p_arrUsers"></param>
        /// <returns></returns>
        private DataRow GetSubscriptionPurchaseRow(string p_sSubscriptionCode, int[] p_arrUsers)
        {
            DataRow drUserPurchase = null;
            DataTable dtUsersPurchases = ConditionalAccessDAL.Get_UsersSubscriptionPurchases(p_arrUsers.ToList(), p_sSubscriptionCode);

            // If there is at least one valid purchase
            if (dtUsersPurchases != null && dtUsersPurchases.Rows != null && dtUsersPurchases.Rows.Count > 0)
            {
                drUserPurchase = dtUsersPurchases.Rows[0];
            }

            return (drUserPurchase);
        }

        /// <summary>
        /// Update Subscription
        /// </summary>
        public virtual bool UpdateSubscriptionDate(string sSiteGUID, string sSubscriptionCode, Int32 nSubscriptionPurchaseID, Int32 dAdditionInDays, bool bRenewable)
        {
            bool bRet = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {
                PriceReason theReason = PriceReason.UnKnown;
                TvinciPricing.Subscription theSub = null;
                TvinciPricing.Price p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubscriptionCode, sSiteGUID, string.Empty, ref theReason, ref theSub, "", "", "");
                bool bIsRecurring = false;
                if (theSub != null && theSub.m_oUsageModule != null)
                    bIsRecurring = theSub.m_bIsRecurring;

                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from subscriptions_purchases with (nolock) where IS_ACTIVE=1 and STATUS=1 and RECURRING_RUNTIME_STATUS=0 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                if (m_nGroupID != 0)
                {
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                }
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSubscriptionPurchaseID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        Int32 nID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                        Int32 nCurrentRecurring = int.Parse(selectQuery.Table("query").DefaultView[i].Row["IS_RECURRING_STATUS"].ToString());
                        DateTime dCurrentEndDate = (DateTime)(selectQuery.Table("query").DefaultView[i].Row["END_DATE"]);
                        DateTime dEndDate = dCurrentEndDate.AddDays(dAdditionInDays);
                        if (dAdditionInDays == -111111)
                            dEndDate = DateTime.UtcNow;
                        if (dAdditionInDays == 30)
                            dEndDate = dCurrentEndDate.AddMonths(1);
                        updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                        if (bIsRecurring == true && bRenewable == true)
                            RenewCacledSubscription(sSiteGUID, sSubscriptionCode, nSubscriptionPurchaseID);
                        if (bIsRecurring == true && bRenewable == false)
                            CancelSubscription(sSiteGUID, sSubscriptionCode, nSubscriptionPurchaseID);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dEndDate);
                        updateQuery += " where ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                        updateQuery.Execute();
                        bRet = true;
                        WriteToUserLog(sSiteGUID, "Subscription: " + sSubscriptionCode.ToString() + "End date changed(" + dCurrentEndDate.ToString("MM/dd/yyyy HH:mm") + "-->" + dEndDate.ToString("MM/dd/yyyy HH:mm") + ")");
                        //Write to users log
                    }
                }
            }
            finally
            {
                #region Disposing
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
                #endregion
            }
            return bRet;
        }
        /// <summary>
        /// Credit Card Renew Subscription
        /// </summary>
        public TvinciBilling.BillingResponse CC_BaseRenewSubscription(string sSiteGUID, double dPrice, string sCurrency,
            string sSubscriptionCode, string sUserIP, string sExtraParams, Int32 nPurchaseID, Int32 nPaymentNumber,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            string sCouponCode = string.Empty;
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            TvinciUsers.UsersService u = null;
            TvinciPricing.mdoule m = null;
            TvinciBilling.module bm = null;
            ODBCWrapper.DirectQuery directQuery = null;
            ODBCWrapper.DirectQuery directQuery1 = null;
            ODBCWrapper.DirectQuery directQuery2 = null;
            ODBCWrapper.DirectQuery directQuery3 = null;
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {
                if (string.IsNullOrEmpty(sSiteGUID))
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                }
                else
                {
                    u = new ConditionalAccess.TvinciUsers.UsersService();

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }

                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                        WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " error returned: " + ret.m_sStatusDescription);
                    }
                    else
                    {
                        m = new ConditionalAccess.TvinciPricing.mdoule();
                        string pricingUrl = Utils.GetWSURL("pricing_ws");
                        if (!string.IsNullOrEmpty(pricingUrl))
                        {
                            m.Url = pricingUrl;
                        }
                        TvinciPricing.Subscription theSub = null;

                        Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                        theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubscriptionCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);

                        if (theSub != null && theSub.m_nNumberOfRecPeriods != 0 && theSub.m_nNumberOfRecPeriods <= nPaymentNumber)
                        {
                            directQuery = new ODBCWrapper.DirectQuery();
                            directQuery.SetConnectionKey("CA_CONNECTION_STRING");
                            directQuery += "update subscriptions_purchases set ";
                            directQuery += "IS_RECURRING_STATUS = 0 ";
                            directQuery += " where ";
                            directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                            directQuery.Execute();

                        }
                        else if (theSub != null)
                        {
                            string sCustomData = string.Empty;
                            if (dPrice != 0)
                            {
                                bm = new ConditionalAccess.TvinciBilling.module();
                                sWSUserName = string.Empty;
                                sWSPass = string.Empty;
                                Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                                sWSURL = Utils.GetWSURL("billing_ws");
                                if (!string.IsNullOrEmpty(sWSURL))
                                {
                                    bm.Url = sWSURL;
                                }
                                bool bIsRecurring = theSub.m_bIsRecurring;

                                Int32 nRecPeriods = theSub.m_nNumberOfRecPeriods;

                                sCustomData = "<customdata type=\"sp\">";
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
                                sCustomData += "<s>" + sSubscriptionCode + "</s>";
                                sCustomData += "<cc>" + sCouponCode + "</cc>";
                                sCustomData += "<p ir=\"" + bIsRecurring.ToString().ToLower() + "\" n=\"" + nPaymentNumber.ToString() + "\" o=\"" + nRecPeriods.ToString() + "\"/>";
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
                                ret = bm.CC_ChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber, nRecPeriods, sExtraParams, string.Empty, string.Empty);
                            }
                        }
                        if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
                        {
                            Int32 nMaxVLC = theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle;
                            WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " renewed" + dPrice.ToString() + sCurrency);
                            DateTime d = (DateTime)(ODBCWrapper.Utils.GetTableSingleVal("subscriptions_purchases", "end_date", nPurchaseID, "CA_CONNECTION_STRING"));
                            DateTime dNext = Utils.GetEndDateTime(d, nMaxVLC);
                            directQuery1 = new ODBCWrapper.DirectQuery();
                            directQuery1.SetConnectionKey("CA_CONNECTION_STRING");
                            directQuery1 += "update subscriptions_purchases set ";
                            directQuery1 += ODBCWrapper.Parameter.NEW_PARAM("end_date", "=", dNext);
                            directQuery1 += ",";
                            directQuery1 += ODBCWrapper.Parameter.NEW_PARAM("num_of_uses", "=", 0);
                            directQuery1 += " where ";
                            directQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                            directQuery1.Execute();

                            string sReciept = ret.m_sRecieptCode;
                            if (sReciept.Length > 0)
                            {
                                Int32 nID = int.Parse(sReciept);
                                updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                                updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                                updateQuery += "where";
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                                updateQuery.Execute();
                            }
                        }
                        else
                        {
                            WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " error returned: " + ret.m_sStatusDescription);

                            if (ret.m_oStatus != ConditionalAccess.TvinciBilling.BillingResponseStatus.ExternalError)
                            {
                                if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.ExpiredCard)
                                {
                                    directQuery3 = new ODBCWrapper.DirectQuery();
                                    directQuery3.SetConnectionKey("CA_CONNECTION_STRING");
                                    directQuery3 += "update subscriptions_purchases set ";
                                    directQuery3 += "FAIL_COUNT = 10 ";
                                    directQuery3 += " where ";
                                    directQuery3 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                                    directQuery3.Execute();

                                }
                                directQuery2 = new ODBCWrapper.DirectQuery();
                                directQuery2.SetConnectionKey("CA_CONNECTION_STRING");
                                directQuery2 += "update subscriptions_purchases set ";
                                directQuery2 += "FAIL_COUNT = FAIL_COUNT + 1 ";
                                directQuery2 += " where ";
                                directQuery2 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                                directQuery2.Execute();



                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at CC_BaseRenewSubscription.");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" Currency: ", sCurrency));
                sb.Append(String.Concat(" Sub Code: ", sSubscriptionCode));
                sb.Append(String.Concat(" User IP: ", sUserIP));
                sb.Append(String.Concat(" Extra Params: ", sExtraParams));
                sb.Append(String.Concat(" Purchase ID: ", nPurchaseID));
                sb.Append(String.Concat(" Payment num: ", nPaymentNumber));
                sb.Append(String.Concat(" Country Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Code: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" Device Nm: ", sDEVICE_NAME));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                }
                if (m != null)
                {
                    m.Dispose();
                }
                if (bm != null)
                {
                    bm.Dispose();
                }
                if (directQuery != null)
                {
                    directQuery.Finish();
                }
                if (directQuery1 != null)
                {
                    directQuery1.Finish();
                }
                if (directQuery2 != null)
                {
                    directQuery2.Finish();
                }
                if (directQuery3 != null)
                {
                    directQuery3.Finish();
                }
                #endregion
            }
            return ret;
        }

        /// <summary>
        /// In App Renew Subscription
        /// </summary>
        public virtual TvinciBilling.InAppBillingResponse InApp_RenewSubscription(string sSiteGUID, double dPrice, string sCurrency,
           string sSubscriptionCode, Int32 nPurchaseID, int nBillingMethod, Int32 nPaymentNumber,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int nInAppTransactionID)
        {
            log.Debug("Renew Fail - " + sSiteGUID + " " + sSubscriptionCode);
            string sCouponCode = string.Empty;
            TvinciBilling.InAppBillingResponse ret = new TvinciBilling.InAppBillingResponse(); // new ConditionalAccess.TvinciBilling.InAppBillingResponse();
            TvinciUsers.UsersService u = null;
            TvinciPricing.mdoule m = null;
            ODBCWrapper.DirectQuery directQuery = null;
            TvinciBilling.module bm = null;
            ODBCWrapper.DirectQuery directQuery1 = null;
            ODBCWrapper.UpdateQuery updateQuery = null;
            ODBCWrapper.DirectQuery directQuery2 = null;
            ODBCWrapper.DirectQuery directQuery3 = null;
            try
            {
                if (string.IsNullOrEmpty(sSiteGUID))
                {
                    #region terminate if site guid id empty
                    ret.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_oBillingResponse.m_sRecieptCode = "";
                    ret.m_oBillingResponse.m_sStatusDescription = "Cant charge an unknown user";
                    ret.m_oInAppReceipt = null;
                    #endregion
                }
                else
                {
                    #region Init useres web service
                    u = new ConditionalAccess.TvinciUsers.UsersService();

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }
                    #endregion

                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        #region terminate if ResponseStatus NOT Ok.
                        ret.m_oBillingResponse = new TvinciBilling.BillingResponse();
                        ret.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_oBillingResponse.m_sRecieptCode = "";
                        ret.m_oBillingResponse.m_sStatusDescription = "Cant charge an unknown user";
                        ret.m_oInAppReceipt = null;
                        WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " error returned: " + ret.m_oBillingResponse.m_sStatusDescription);
                        #endregion
                    }
                    else
                    {
                        #region Init Tvinci Pricing web service
                        m = new ConditionalAccess.TvinciPricing.mdoule();
                        string pricingUrl = Utils.GetWSURL("pricing_ws");
                        if (!string.IsNullOrEmpty(pricingUrl))
                        {
                            m.Url = pricingUrl;
                        }
                        #endregion

                        TvinciPricing.Subscription theSub = null;

                        Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                        theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubscriptionCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);

                        if (theSub != null && theSub.m_nNumberOfRecPeriods != 0 && theSub.m_nNumberOfRecPeriods <= nPaymentNumber)
                        {
                            #region Update subscription purchasesto is recurring status = 0
                            directQuery = new ODBCWrapper.DirectQuery();
                            directQuery.SetConnectionKey("CA_CONNECTION_STRING");
                            directQuery += "update subscriptions_purchases set ";
                            directQuery += "IS_RECURRING_STATUS = 0 ";
                            directQuery += " where ";
                            directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                            directQuery.Execute();
                            #endregion

                        }
                        else if (theSub != null && theSub.m_oSubscriptionUsageModule != null)
                        {
                            string sCustomData = string.Empty;
                            if (dPrice != 0)
                            {
                                #region Init Billing web service
                                bm = new ConditionalAccess.TvinciBilling.module();
                                sWSUserName = string.Empty;
                                sWSPass = string.Empty;
                                Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                                sWSURL = Utils.GetWSURL("billing_ws");
                                if (!string.IsNullOrEmpty(sWSURL))
                                {
                                    bm.Url = sWSURL;
                                }
                                #endregion


                                bool bIsRecurring = theSub.m_bIsRecurring;

                                Int32 nRecPeriods = theSub.m_nNumberOfRecPeriods;

                                #region Create Custom Data
                                sCustomData = "<customdata type=\"sp\">";
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
                                sCustomData += "<s>" + sSubscriptionCode + "</s>";
                                sCustomData += "<cc>" + sCouponCode + "</cc>";
                                sCustomData += "<p ir=\"" + bIsRecurring.ToString().ToLower() + "\" n=\"" + nPaymentNumber.ToString() + "\" o=\"" + nRecPeriods.ToString() + "\"/>";
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
                                #endregion


                                ret = bm.InApp_ReneweInAppPurchase(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sCustomData, nPaymentNumber, nRecPeriods, nInAppTransactionID);


                            }
                        }
                        if (ret.m_oBillingResponse.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success && theSub != null && theSub.m_oSubscriptionUsageModule != null)
                        {
                            Int32 nMaxVLC = theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle;
                            WriteToUserLog(sSiteGUID, "Subscription InApp renewal: " + sSubscriptionCode.ToString() + " renewed" + dPrice.ToString() + sCurrency);

                            DateTime dt1970 = new DateTime(1970, 1, 1);
                            DateTime endDate = DateTime.MinValue;
                            ConditionalAccess.TvinciBilling.InAppReceipt receipt = ret.m_oInAppReceipt;

                            // First try with iOs 6
                            if (receipt.iOSVersion == "6")
                            {
                                // If we have only one latest receipt info (in iOS 6, it should be)
                                if (receipt.latest_receipt_info != null &&
                                    receipt.latest_receipt_info.Length == 1)
                                {
                                    ConditionalAccess.TvinciBilling.iTunesReceipt latestReceiptInfo = receipt.latest_receipt_info[0];

                                    // Use expires_date (which is in MS in iOs 6) and purchase_date_ms to find start/end dates
                                    if (!string.IsNullOrEmpty(latestReceiptInfo.expires_date) &&
                                        !string.IsNullOrEmpty(latestReceiptInfo.purchase_date_ms))
                                    {
                                        double endMS = double.Parse(latestReceiptInfo.expires_date);

                                        endDate = dt1970.AddMilliseconds(endMS);
                                    }
                                }

                                // If we couldn't find the dates with "latest receipt info",
                                // Use receipt
                                if (endDate == DateTime.MinValue)
                                {
                                    double endMS = double.Parse(receipt.receipt.expires_date);

                                    endDate = dt1970.AddMilliseconds(endMS);
                                }
                            }
                            // Then try with iOS 7
                            else if (receipt.iOSVersion == "7")
                            {
                                if (receipt.latest_receipt_info != null)
                                {
                                    double endMS = 0;

                                    // Run on all latest receipts and find the one that matches the date and the product id
                                    foreach (var lastReceipt in receipt.latest_receipt_info)
                                    {
                                        // If the product code matches
                                        if (lastReceipt.product_id == theSub.m_ProductCode)
                                        {
                                            // Find the maximum start date
                                            double currentEndMS = double.Parse(lastReceipt.expires_date_ms);

                                            if (currentEndMS > endMS)
                                            {
                                                endMS = currentEndMS;
                                            }
                                        }
                                    }

                                    // If we found a receipt with the matching product code and good end date
                                    if (endMS > 0)
                                    {
                                        endDate = dt1970.AddMilliseconds(endMS);
                                    }
                                }
                            }

                            // Check if there was a problem
                            if (endDate == DateTime.MinValue)
                            {
                                ret.m_oBillingResponse.m_sStatusDescription = "Something went wrong with the end date";
                            }

                            #region update subscription purchases
                            directQuery1 = new ODBCWrapper.DirectQuery();
                            directQuery1.SetConnectionKey("CA_CONNECTION_STRING");
                            directQuery1 += "update subscriptions_purchases set ";
                            directQuery1 += ODBCWrapper.Parameter.NEW_PARAM("end_date", "=", endDate.AddHours(6));
                            directQuery1 += ",";
                            directQuery1 += ODBCWrapper.Parameter.NEW_PARAM("num_of_uses", "=", 0);
                            directQuery1 += " where ";
                            directQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                            directQuery1.Execute();
                            #endregion

                            string sReciept = ret.m_oBillingResponse.m_sRecieptCode;

                            if (string.IsNullOrEmpty(sReciept))
                            {
                                Int32 nID = int.Parse(sReciept);
                                updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                                updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                                updateQuery += "where";
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                                updateQuery.Execute();
                            }
                        }
                        else
                        {
                            WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " error returned: " + ret.m_oBillingResponse.m_sStatusDescription);

                            if (ret.m_oBillingResponse.m_oStatus != ConditionalAccess.TvinciBilling.BillingResponseStatus.ExternalError)
                            {
                                if (ret.m_oBillingResponse.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.ExpiredCard)
                                {
                                    #region Update subscription purchases to fail count = 10
                                    directQuery2 = new ODBCWrapper.DirectQuery();
                                    directQuery2.SetConnectionKey("CA_CONNECTION_STRING");
                                    directQuery2 += "update subscriptions_purchases set ";
                                    directQuery2 += "FAIL_COUNT = 10 ";
                                    directQuery2 += " where ";
                                    directQuery2 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                                    directQuery2.Execute();
                                    #endregion
                                }

                                #region Increase subscription purchase fail count
                                directQuery3 = new ODBCWrapper.DirectQuery();
                                directQuery3.SetConnectionKey("CA_CONNECTION_STRING");
                                directQuery3 += "update subscriptions_purchases set ";
                                directQuery3 += "FAIL_COUNT = FAIL_COUNT + 1 ";
                                directQuery3 += " where ";
                                directQuery3 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                                directQuery3.Execute();
                                #endregion
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at InApp_RenewSubscription.");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" Currency: ", sCurrency));
                sb.Append(String.Concat(" Sub Code: ", sSubscriptionCode));
                sb.Append(String.Concat(" Purchase ID: ", nPurchaseID));
                sb.Append(String.Concat(" Billing Method: ", nBillingMethod));
                sb.Append(String.Concat(" Payment Num: ", nPaymentNumber));
                sb.Append(String.Concat(" Country Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" Device Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" InApp Trans ID: ", nInAppTransactionID));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);

                #endregion

                ret.m_oBillingResponse.m_oStatus = TvinciBilling.BillingResponseStatus.Fail;
                ret.m_oBillingResponse.m_sStatusDescription = "Internal error";
            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                }
                if (m != null)
                {
                    m.Dispose();
                }
                if (bm != null)
                {
                    bm.Dispose();
                }
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
                if (directQuery != null)
                {
                    directQuery.Finish();
                }
                if (directQuery1 != null)
                {
                    directQuery1.Finish();
                }
                if (directQuery2 != null)
                {
                    directQuery2.Finish();
                }
                if (directQuery3 != null)
                {
                    directQuery3.Finish();
                }
                #endregion
            }
            return ret;
        }

        /*
            * returns true if user exists. false otherwise. throws exception if cannot deliver all data required for renewal
            * 
        */
        protected bool GetBaseRenewMultiUsageSubscriptionData(string sSiteGUID, string sSubscriptionCode, string sUserIP,
           Int32 nPurchaseID, Int32 nPaymentNumber, int nTotalPaymentsNumber, string sCountryCd, string sLANGUAGE_CODE,
           string sDEVICE_NAME, int nNumOfPayments, bool bIsPurchasedWithPreviewModule, DateTime dtCurrentEndDate,
           ref double dPrice, ref string sCustomData, ref string sCurrency, ref int nRecPeriods,
           ref bool bIsMPPRecurringInfinitely, ref int nMaxVLCOfSelectedUsageModule)
        {
            string sCouponCode = string.Empty;

            UserResponseObject ExistUser = Utils.GetExistUser(sSiteGUID, m_nGroupID);

            if (ExistUser != null && ExistUser.m_RespStatus == ConditionalAccess.TvinciUsers.ResponseStatus.OK)
            {
                TvinciPricing.mdoule m = null;
                try
                {
                    DateTime dtCorruptedEndDate = new DateTime(2000, 1, 1);
                    if (dtCorruptedEndDate.Equals(dtCurrentEndDate))
                        throw new Exception("End date extracted from subscriptions purchases is corrupted");

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;

                    Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                    m = new TvinciPricing.mdoule();
                    string sWSURL = Utils.GetWSURL("cloud_pricing_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        m.Url = sWSURL;
                    }
                    TvinciPricing.Subscription theSub = null;

                    theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubscriptionCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);
                    if (theSub != null)
                    {
                        TvinciPricing.UsageModule AppUsageModule = GetAppropriateMultiSubscriptionUsageModule(theSub, nPaymentNumber, nPurchaseID, nTotalPaymentsNumber, nNumOfPayments, bIsPurchasedWithPreviewModule);

                        dPrice = 0;
                        TvinciPricing.Currency oCurrency = null;
                        sCurrency = "n/a";
                        bool bIsRecurring = theSub.m_bIsRecurring;

                        if (AppUsageModule != null)
                        {
                            TvinciPricing.PriceCode p = m.GetPriceCodeData(sWSUserName, sWSPass, AppUsageModule.m_pricing_id.ToString(), sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                            TvinciPricing.DiscountModule externalDisount = m.GetDiscountCodeData(sWSUserName, sWSPass, AppUsageModule.m_ext_discount_id.ToString());

                            if (externalDisount != null)
                            {
                                TvinciPricing.Price price = Utils.GetPriceAfterDiscount(p.m_oPrise, externalDisount, 1);
                                dPrice = price.m_dPrice;
                                oCurrency = price.m_oCurrency;
                                sCurrency = price.m_oCurrency.m_sCurrencyCD3;

                            }
                            else
                            {
                                dPrice = p.m_oPrise.m_dPrice;
                                oCurrency = p.m_oPrise.m_oCurrency;
                                sCurrency = p.m_oPrise.m_oCurrency.m_sCurrencyCD3;
                            }

                            HandleRecurringCoupon(nPurchaseID, theSub, nTotalPaymentsNumber, oCurrency, bIsPurchasedWithPreviewModule, ref dPrice, ref sCouponCode);

                            nRecPeriods = theSub.m_nNumberOfRecPeriods;

                            bIsMPPRecurringInfinitely = theSub.m_bIsInfiniteRecurring;

                            nMaxVLCOfSelectedUsageModule = AppUsageModule.m_tsMaxUsageModuleLifeCycle;

                            sCustomData = GetCustomDataForMPPRenewal(theSub, AppUsageModule, p, sSubscriptionCode,
                                sSiteGUID, dPrice, sCurrency, sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                        }
                        else
                        {
                            throw new Exception("Usage Module returned from GetAppropriateUsageModule is null");
                        }
                    }
                    else
                    {
                        throw new Exception("Subscription returned from pricing module is null");
                    }
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

            }
            else
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Direct debit Renew Subscription
        /// </summary>
        /// <param name="sSiteGUID"></param>
        /// <param name="sSubscriptionCode"></param>
        /// <param name="sUserIP"></param>
        /// <param name="sExtraParams"></param>
        /// <param name="nPurchaseID"></param>
        /// <param name="nBillingMethod"></param>
        /// <param name="nPaymentNumber"></param>
        /// <param name="nTotalPaymentsNumber"></param>
        /// <param name="sCountryCd"></param>
        /// <param name="sLANGUAGE_CODE"></param>
        /// <param name="sDEVICE_NAME"></param>
        /// <param name="nNumOfPayments"></param>
        /// <param name="bIsPurchasedWithPreviewModule"></param>
        /// <param name="dtCurrentEndDate"></param>
        /// <param name="eBillingProvider"></param>
        /// <returns></returns>
        public virtual TvinciBilling.BillingResponse DD_BaseRenewMultiUsageSubscription(string sSiteGUID, string sSubscriptionCode, string sUserIP, string sExtraParams,
            Int32 nPurchaseID, int nBillingMethod, Int32 nPaymentNumber, int nTotalPaymentsNumber, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME,
            int nNumOfPayments, bool bIsPurchasedWithPreviewModule, DateTime dtCurrentEndDate, ConditionalAccess.eBillingProvider eBillingProvider)
        {
            TvinciBilling.BillingResponse oBillingResponse = new ConditionalAccess.TvinciBilling.BillingResponse();
            oBillingResponse.m_oStatus = TvinciBilling.BillingResponseStatus.UnKnown;

            try
            {
                double dPrice = 0.0;
                string sCustomData = string.Empty;
                string sCurrency = string.Empty;
                int nRecPeriods = 0;
                bool bIsMPPRecurringInfinitely = false;
                int nMaxVLCOfSelectedUsageModule = 0;

                if (GetBaseRenewMultiUsageSubscriptionData(sSiteGUID, sSubscriptionCode, sUserIP, nPurchaseID, nPaymentNumber,
                    nTotalPaymentsNumber, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, nNumOfPayments, bIsPurchasedWithPreviewModule,
                    dtCurrentEndDate, ref dPrice, ref sCustomData, ref sCurrency, ref nRecPeriods, ref bIsMPPRecurringInfinitely,
                    ref nMaxVLCOfSelectedUsageModule))
                {
                    oBillingResponse = HandleBaseRenewMPPBillingCharge(sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber,
                        nRecPeriods, sExtraParams, nBillingMethod, nPurchaseID, eBillingProvider);

                    if (oBillingResponse.m_oStatus == TvinciBilling.BillingResponseStatus.Success)
                    {
                        // Enqueue notification for PS so they will know a sub was renewed

                        var dicData = new Dictionary<string, object>()
                        {
                            {"BillingTransactionID", oBillingResponse.m_sRecieptCode},
                            {"SiteGUID", sSiteGUID},
                            {"PaymentNumber", nPaymentNumber},
                            {"TotalPaymentsNumber", nTotalPaymentsNumber},
                            {"CustomData", sCustomData},
                            {"Price", dPrice},
                            {"PurchaseID", nPurchaseID},
                            {"SubscriptionCode", sSubscriptionCode}
                        };

                        this.EnqueueEventRecord(NotifiedAction.ChargedSubscriptionRenewal, dicData);

                        HandleMPPRenewalBillingSuccess(sSiteGUID, sSubscriptionCode, dtCurrentEndDate, bIsPurchasedWithPreviewModule,
                           nPurchaseID, sCurrency, dPrice, nPaymentNumber, oBillingResponse.m_sRecieptCode, nMaxVLCOfSelectedUsageModule,
                           bIsMPPRecurringInfinitely, nRecPeriods);

                    }
                    else
                    {
                        HandleMPPRenewalBillingFail(sSiteGUID, sSubscriptionCode, nPurchaseID, oBillingResponse, sCustomData);
                    }
                }
                else
                {
                    // user does not exist. update fail count to max so we won't try again to renew this mpp
                    // return unknown user
                    HandleMPPRenewalUserDoesNotExist(sSiteGUID, nPurchaseID, ref oBillingResponse);
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder(String.Concat("Exception. Exception msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Sub Code: ", sSubscriptionCode));
                sb.Append(String.Concat(" User IP: ", sUserIP));
                sb.Append(String.Concat(" Purchase ID: ", nPurchaseID));
                sb.Append(String.Concat(" Extra Params: ", sExtraParams));
                sb.Append(String.Concat(" Billing Method: ", nBillingMethod));
                sb.Append(String.Concat(" Payment Number: ", nPaymentNumber));
                sb.Append(String.Concat(" Total payments number: ", nTotalPaymentsNumber));
                sb.Append(String.Concat(" Country Code: ", sCountryCd));
                sb.Append(String.Concat(" Language Code: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" Device name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" Num of payments: ", nNumOfPayments));
                sb.Append(String.Concat(" Purchased with preview module: ", bIsPurchasedWithPreviewModule.ToString().ToLower()));
                sb.Append(String.Concat(" BaseConditionalAccess is: ", this.GetType().ToString()));
                sb.Append(String.Concat(" Stack trace: ", ex.StackTrace));
                log.Debug("DD_BaseRenewMultiUsageSubscription - " + sb.ToString(), ex);
                WriteToUserLog(sSiteGUID, string.Format("MPP Renewal. Exception thrown. Msg: {0}", ex.Message));
                #endregion

                // increment fail count
                ConditionalAccessDAL.Update_MPPFailCountByPurchaseID(nPurchaseID, true, 0, "CA_CONNECTION_STRING");

                // set billing response
                oBillingResponse.m_oStatus = TvinciBilling.BillingResponseStatus.Fail;
                oBillingResponse.m_sRecieptCode = string.Empty;
                oBillingResponse.m_sStatusDescription = ex.Message;
            }

            return oBillingResponse;
        }


        protected void HandleMPPRenewalBillingFail(string sSiteGUID, string sSubscriptionCode, long lPurchaseID,
         TvinciBilling.BillingResponse br, string sCustomData)
        {

            log.Debug("Fail - " + string.Format("Fail count for user: {0} . Sub Code: {1} , Purchase ID: {2} , Response status: {3} , Response status desc: {4} , Custom Data: {5}", sSiteGUID, sSubscriptionCode, lPurchaseID, br.m_oStatus.ToString(), br.m_sStatusDescription, sCustomData));
            WriteToUserLog(sSiteGUID, string.Format("MPP auto renewal: {0} , error returned: {1} , status returned: {2}", sSubscriptionCode.ToString(), br.m_sStatusDescription, br.m_oStatus.ToString()));

            if (br.m_oStatus != ConditionalAccess.TvinciBilling.BillingResponseStatus.ExternalError)
            {

                if (br.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.ExpiredCard || br.m_oStatus == TvinciBilling.BillingResponseStatus.CellularPermissionsError)
                {
                    // card is expired. there is no point to continue trying renewing the mpp for this user.
                    // hence, we set the fail count to maximum.
                    ConditionalAccessDAL.Update_MPPFailCountByPurchaseID(lPurchaseID, false, Utils.GetGroupFAILCOUNT(m_nGroupID, "CA_CONNECTION_STRING"), "CA_CONNECTION_STRING");
                }
                else
                {
                    // failed to renew. increase fail count by one.
                    ConditionalAccessDAL.Update_MPPFailCountByPurchaseID(lPurchaseID, true, 0, "CA_CONNECTION_STRING");
                }
            }
        }

        protected void HandleMPPRenewalUserDoesNotExist(string sSiteGUID, long lPurchaseID, ref TvinciBilling.BillingResponse res)
        {
            log.Debug("Fail - " + string.Format("User ID: {0} does not exist. Purchase ID: {1}", sSiteGUID, lPurchaseID));

            // user does not exist. there is no point to continue trying renewing the mpp.
            // hence, we set the fail count to maximum
            ConditionalAccessDAL.Update_MPPFailCountByPurchaseID(lPurchaseID, false, Utils.GetGroupFAILCOUNT(m_nGroupID, "CA_CONNECTION_STRING"), "CA_CONNECTION_STRING");

            res.m_oStatus = TvinciBilling.BillingResponseStatus.UnKnownUser;
            res.m_sStatusDescription = "User does not exist";
            res.m_sRecieptCode = string.Empty;
        }

        /// <summary>
        /// Direct Deipt Renew Subscription
        /// </summary>
        public TvinciBilling.BillingResponse DD_BaseRenewSubscription(string sSiteGUID, double dPrice, string sCurrency,
           string sSubscriptionCode, string sUserIP, string sExtraParams, Int32 nPurchaseID, int nBillingMethod, Int32 nPaymentNumber,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            string sCouponCode = string.Empty;
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            TvinciUsers.UsersService u = null;
            ODBCWrapper.DirectQuery directQuery1 = null;
            TvinciPricing.mdoule m = null;
            ODBCWrapper.DirectQuery directQuery2 = null;
            ODBCWrapper.DirectQuery directQuery3 = null;
            TvinciBilling.module bm = null;
            ODBCWrapper.UpdateQuery updateQuery = null;
            ODBCWrapper.DirectQuery directQuery4 = null;
            ODBCWrapper.DirectQuery directQuery5 = null;
            try
            {
                if (string.IsNullOrEmpty(sSiteGUID))
                {
                    #region terminate if site guid id empty
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                    #endregion
                }
                else
                {
                    #region Init useres web service
                    u = new ConditionalAccess.TvinciUsers.UsersService();

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }
                    #endregion

                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        #region terminate if ResponseStatus NOT Ok.
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                        WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " error returned: " + ret.m_sStatusDescription);

                        //Increase subscription purchase fail count
                        directQuery1 = new ODBCWrapper.DirectQuery();
                        directQuery1.SetConnectionKey("CA_CONNECTION_STRING");
                        directQuery1 += "update subscriptions_purchases set ";
                        directQuery1 += "FAIL_COUNT = FAIL_COUNT + 1 ";
                        directQuery1 += " where ";
                        directQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                        directQuery1.Execute();

                        #endregion
                    }
                    else
                    {
                        #region Init Tvinci Pricing web service
                        m = new ConditionalAccess.TvinciPricing.mdoule();
                        string pricingUrl = Utils.GetWSURL("pricing_ws");
                        if (!string.IsNullOrEmpty(pricingUrl))
                        {
                            m.Url = pricingUrl;
                        }
                        #endregion

                        TvinciPricing.Subscription theSub = null;

                        Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                        theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubscriptionCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);

                        if (theSub != null && theSub.m_nNumberOfRecPeriods != 0 && theSub.m_nNumberOfRecPeriods < nPaymentNumber)
                        {
                            #region Update subscription purchase to is recurring status = 0
                            directQuery2 = new ODBCWrapper.DirectQuery();
                            directQuery2.SetConnectionKey("CA_CONNECTION_STRING");
                            directQuery2 += "update subscriptions_purchases set ";
                            directQuery2 += "IS_RECURRING_STATUS = 0 ";
                            directQuery2 += " where ";
                            directQuery2 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                            directQuery2.Execute();
                            #endregion

                        }
                        else if (theSub != null && theSub.m_oSubscriptionUsageModule != null)
                        {
                            string sCustomData = string.Empty;
                            if (dPrice != 0)
                            {
                                bm = new ConditionalAccess.TvinciBilling.module();
                                sWSUserName = string.Empty;
                                sWSPass = string.Empty;
                                Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                                sWSURL = Utils.GetWSURL("billing_ws");
                                if (!string.IsNullOrEmpty(sWSURL))
                                {
                                    bm.Url = sWSURL;
                                }
                                bool bIsRecurring = theSub.m_bIsRecurring;
                                Int32 nRecPeriods = theSub.m_nNumberOfRecPeriods;

                                #region Create Custom Data
                                sCustomData = "<customdata type=\"sp\">";
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
                                sCustomData += "<s>" + sSubscriptionCode + "</s>";
                                sCustomData += "<cc>" + sCouponCode + "</cc>";
                                sCustomData += "<p ir=\"" + bIsRecurring.ToString().ToLower() + "\" n=\"" + nPaymentNumber.ToString() + "\" o=\"" + nRecPeriods.ToString() + "\"/>";
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
                                #endregion

                                //customdata id
                                ret = bm.DD_ChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber, nRecPeriods, nPurchaseID.ToString(), nBillingMethod);

                            }
                        }
                        if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success && theSub != null && theSub.m_oSubscriptionUsageModule != null)
                        {
                            Int32 nMaxVLC = theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle;
                            WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " renewed" + dPrice.ToString() + sCurrency);
                            DateTime d = (DateTime)(ODBCWrapper.Utils.GetTableSingleVal("subscriptions_purchases", "end_date", nPurchaseID, "CA_CONNECTION_STRING"));
                            DateTime dNext = Utils.GetEndDateTime(d, nMaxVLC);
                            #region update subscriptions_purchases end date
                            directQuery3 = new ODBCWrapper.DirectQuery();
                            directQuery3.SetConnectionKey("CA_CONNECTION_STRING");
                            directQuery3 += "update subscriptions_purchases set ";
                            directQuery3 += ODBCWrapper.Parameter.NEW_PARAM("end_date", "=", dNext);
                            directQuery3 += ",";
                            directQuery3 += ODBCWrapper.Parameter.NEW_PARAM("num_of_uses", "=", 0);
                            directQuery3 += " where ";
                            directQuery3 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                            directQuery3.Execute();
                            #endregion

                            if (!string.IsNullOrEmpty(ret.m_sRecieptCode))
                            {
                                Int32 nID = int.Parse(ret.m_sRecieptCode);

                                #region Update billing transactions with PurchaseID
                                updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                                updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                                updateQuery += "where";
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                                updateQuery.Execute();
                                #endregion
                            }
                        }
                        else
                        {

                            WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " error returned: " + ret.m_sStatusDescription);

                            if (ret.m_oStatus != ConditionalAccess.TvinciBilling.BillingResponseStatus.ExternalError)
                            {
                                if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.ExpiredCard)
                                {
                                    #region Update subscription purchases to fail count = 10
                                    directQuery4 = new ODBCWrapper.DirectQuery();
                                    directQuery4.SetConnectionKey("CA_CONNECTION_STRING");
                                    directQuery4 += "update subscriptions_purchases set ";
                                    directQuery4 += "FAIL_COUNT = 10 ";
                                    directQuery4 += " where ";
                                    directQuery4 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                                    directQuery4.Execute();
                                    #endregion
                                }

                                #region Increase subscription purchase fail count
                                directQuery5 = new ODBCWrapper.DirectQuery();
                                directQuery5.SetConnectionKey("CA_CONNECTION_STRING");
                                directQuery5 += "update subscriptions_purchases set ";
                                directQuery5 += "FAIL_COUNT = FAIL_COUNT + 1 ";
                                directQuery5 += " where ";
                                directQuery5 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                                directQuery5.Execute();
                                #endregion
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at DD_BaseRenewSubscription. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" Curr: ", sCurrency));
                sb.Append(String.Concat(" Sub Code: ", sSubscriptionCode));
                sb.Append(String.Concat(" User IP: ", sUserIP));
                sb.Append(String.Concat(" Xtra Prms: ", sExtraParams));
                sb.Append(String.Concat(" Prchs ID: ", nPurchaseID));
                sb.Append(String.Concat(" BM ID: ", nBillingMethod));
                sb.Append(String.Concat(" Payment num: ", nPaymentNumber));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" Device Nm: ", sDEVICE_NAME));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                }
                if (m != null)
                {
                    m.Dispose();
                }
                if (bm != null)
                {
                    bm.Dispose();
                }
                if (directQuery1 != null)
                {
                    directQuery1.Finish();
                }
                if (directQuery2 != null)
                {
                    directQuery2.Finish();
                }
                if (directQuery3 != null)
                {
                    directQuery3.Finish();
                }
                if (directQuery4 != null)
                {
                    directQuery4.Finish();
                }
                if (directQuery5 != null)
                {
                    directQuery5.Finish();
                }
                #endregion
            }
            return ret;
        }

        private TvinciPricing.UsageModule GetAppropriateMultiSubscriptionUsageModule(TvinciPricing.Subscription thesub, int nPaymentNumber, int nPurchaseID, int nTotalNumOfPayments, int nNumOfPayments, bool bIsPurchasedWithPreviewModule)
        {
            TvinciPricing.UsageModule u = null;
            object oSub_StartDate = ODBCWrapper.Utils.GetTableSingleVal("subscriptions_purchases", "START_DATE", nPurchaseID, "CA_CONNECTION_STRING");
            object oSub_EndDate = ODBCWrapper.Utils.GetTableSingleVal("subscriptions_purchases", "END_DATE", nPurchaseID, "CA_CONNECTION_STRING");

            DateTime dt_statdate = Convert.ToDateTime(oSub_StartDate.ToString());
            DateTime sub_enddate = Convert.ToDateTime(oSub_EndDate.ToString());
            DateTime tempsub_enddate = Convert.ToDateTime(oSub_EndDate.ToString());
            if (thesub.m_nNumberOfRecPeriods != 0)
            {
                int npm = nPaymentNumber % thesub.m_nNumberOfRecPeriods;
                if (npm != 0)
                {
                    nPaymentNumber = npm;
                }
                else
                {
                    nPaymentNumber = thesub.m_nNumberOfRecPeriods;
                }

            }
            if (thesub.m_MultiSubscriptionUsageModule != null)
            {
                TvinciPricing.UsageModule[] uList = thesub.m_MultiSubscriptionUsageModule;
                int totalperiod = 0;
                for (int i = 0; i < thesub.m_MultiSubscriptionUsageModule.Length; i++)
                {
                    int umtotal = uList[i].m_tsViewLifeCycle * (uList[i].m_num_of_rec_periods + 1);
                    tempsub_enddate = tempsub_enddate.AddMinutes(umtotal);

                    totalperiod += uList[i].m_num_of_rec_periods + 1;
                    if (/*i == 0 && uList[i].m_is_renew == 0*/ IsSkipOnFirstUsageModule(i, uList[i].m_is_renew == 1, nTotalNumOfPayments, nNumOfPayments, bIsPurchasedWithPreviewModule))
                    {
                        /*
                         * 1. The renewer runs only after the user purchases an mpp.
                         * 2. Hence, if the first usage module is not renewable and it has already been used because it was chosen when the user purchased the mpp
                         * 3. This resolves the following bug:
                         *      a. MPP contains two usage modules
                         *      b. First usage module is not renewable
                         *      c. Second usage modules is renewable
                         *      d. Before the fix, when the renewer first launched, it grabbed the first usage module instead of the second one.
                         * 
                         */
                        continue;
                    }
                    if (uList[i].m_is_renew == 1 && uList[i].m_num_of_rec_periods == 0)
                    {
                        u = thesub.m_MultiSubscriptionUsageModule[i];
                        break;
                    }
                    else if (nPaymentNumber <= totalperiod)
                    {
                        if (sub_enddate < tempsub_enddate)
                        {
                            u = thesub.m_MultiSubscriptionUsageModule[i];
                            break;
                        }
                    }

                }
                if (u == null)
                {
                    totalperiod = 0;
                    for (int i = 0; i < thesub.m_MultiSubscriptionUsageModule.Length; i++)
                    {
                        totalperiod += uList[i].m_num_of_rec_periods + 1;
                        if (uList[i].m_is_renew == 1 && uList[i].m_num_of_rec_periods == 0)
                        {
                            u = thesub.m_MultiSubscriptionUsageModule[i];
                            break;
                        }
                        else if (nPaymentNumber <= totalperiod)
                        {

                            u = thesub.m_MultiSubscriptionUsageModule[i];
                            break;
                        }


                    }
                }
            }
            if (u == null)
            {
                string strLog = string.Format("could not find Appropriate Multi usage module for Subscription ID : {0}", thesub.m_SubscriptionCode);
                log.Debug("Get Appropriate Multi Subscription Usage Module Fail - " + strLog);
            }
            return u;
        }


        public TvinciBilling.BillingResponse CC_BaseMultiRenewSubscription(string sSiteGUID, double dPrice, string sCurrency,
           string sSubscriptionCode, string sUserIP, string sExtraParams, Int32 nPurchaseID, int nBillingMethod, Int32 nPaymentNumber,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            //write loge
            log.Debug("CC Base Multi usage module renew subscription - " + sSiteGUID + " " + sSubscriptionCode);
            //create billing response resault object 
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            TvinciUsers.UsersService u = null;
            TvinciPricing.mdoule m = null;
            ODBCWrapper.DirectQuery directQuery = null;
            try
            {
                #region Init useres web service
                u = new ConditionalAccess.TvinciUsers.UsersService();

                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                {
                    u.Url = sWSURL;
                }
                #endregion

                #region Check Exist User Guid ,terminate if site guid id empty or response status dose not OK.
                if (string.IsNullOrEmpty(sSiteGUID))
                {

                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "Cant charge an unknown user";

                }
                else
                {


                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        #region terminate if ResponseStatus NOT Ok.
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = "";
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                        WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " error returned: " + ret.m_sStatusDescription);
                        #endregion
                    }
                }
                #endregion

                #region Init Tvinci Pricing web service
                m = new ConditionalAccess.TvinciPricing.mdoule();
                string pricingUrl = Utils.GetWSURL("pricing_ws");
                if (!string.IsNullOrEmpty(pricingUrl))
                {
                    m.Url = pricingUrl;
                }
                #endregion

                TvinciPricing.Subscription theSub = null;

                Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubscriptionCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);
                if (theSub != null && theSub.m_nNumberOfRecPeriods != 0 && theSub.m_nNumberOfRecPeriods <= nPaymentNumber)
                {
                    #region Update subscription purchase to is recurring status = 0
                    directQuery = new ODBCWrapper.DirectQuery();
                    directQuery.SetConnectionKey("CA_CONNECTION_STRING");
                    directQuery += "update subscriptions_purchases set ";
                    directQuery += "IS_RECURRING_STATUS = 0 ";
                    directQuery += " where ";
                    directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                    directQuery.Execute();
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at CC_BaseMultiRenewSubscription. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" Curr: ", sCurrency));
                sb.Append(String.Concat(" Sub Cd: ", sSubscriptionCode));
                sb.Append(String.Concat(" User IP: ", sUserIP));
                sb.Append(String.Concat(" Extra Params: ", sExtraParams));
                sb.Append(String.Concat(" Purchase ID: ", nPurchaseID));
                sb.Append(String.Concat(" BM: ", nBillingMethod));
                sb.Append(String.Concat(" Payment num: ", nPaymentNumber));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" Device Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));

                log.Error("Exception- " + sb.ToString(), ex);
                #endregion
            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                }
                if (m != null)
                {
                    m.Dispose();
                }
                if (directQuery != null)
                {
                    directQuery.Finish();
                }
                #endregion
            }
            return ret;
        }
        /// <summary>
        /// Checks if there is recurring coupon definition on the subscription and if yes 
        /// activate the discount of the coupon on the price and return the updated price and the coupon code. 
        /// </summary> 
        private void HandleRecurringCoupon(int nPurchaseID, TvinciPricing.Subscription theSub, int nTotalPaymentsNumber, TvinciPricing.Currency oCurrency, bool bIsPurchasedWithPreviewModule, ref double dPrice, ref string retCouponCode)
        {
            try
            {
                if (theSub.m_oCouponsGroup != null && theSub.m_oCouponsGroup.m_oDiscountCode != null)
                {
                    if (IsCouponStillRedeemable(bIsPurchasedWithPreviewModule, theSub.m_oCouponsGroup.m_nMaxRecurringUsesCountForCoupon, nTotalPaymentsNumber))
                    {
                        string sCouponCode = Utils.GetSubscriptiopnPurchaseCoupon(nPurchaseID);
                        if (!string.IsNullOrEmpty(sCouponCode))
                        {
                            TvinciPricing.Price priceBeforeCouponDiscount = new TvinciPricing.Price();
                            priceBeforeCouponDiscount.m_dPrice = dPrice;
                            priceBeforeCouponDiscount.m_oCurrency = oCurrency;
                            TvinciPricing.Price priceResult = Utils.GetPriceAfterDiscount(priceBeforeCouponDiscount, theSub.m_oCouponsGroup.m_oDiscountCode, 0);
                            dPrice = priceResult.m_dPrice;
                            retCouponCode = sCouponCode;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("HandleRecurringCoupon error - , PurchaseID: " + nPurchaseID.ToString() + ",Exception:" + ex.ToString(), ex);
            }
        }



        protected bool isDevicePlayValid(string sSiteGUID, string sDEVICE_NAME, ref TvinciDomains.Domain userDomain)
        {
            if (Utils.IsAnonymousUser(sSiteGUID))
                return true;

            TvinciUsers.UsersService u = null;
            TvinciDomains.module domainsWS = null;
            bool isDeviceRecognized = false;
            try
            {
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                u = new TvinciUsers.UsersService();
                string sWSURL = Utils.GetWSURL("users_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                {
                    u.Url = sWSURL;
                }
                TvinciUsers.UserResponseObject userRepObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                if (userRepObj != null && userRepObj.m_user != null && userRepObj.m_RespStatus == ResponseStatus.OK)
                {
                    int domainID = userRepObj.m_user.m_domianID;
                    if (domainID != 0)
                    {
                        domainsWS = new TvinciDomains.module();
                        Utils.GetWSCredentials(m_nGroupID, eWSModules.DOMAINS, ref sWSUserName, ref sWSPass);
                        sWSURL = Utils.GetWSURL("domains_ws");
                        if (!string.IsNullOrEmpty(sWSURL))
                        {
                            domainsWS.Url = sWSURL;
                        }
                        var res = domainsWS.GetDomainInfo(sWSUserName, sWSPass, domainID);
                        if (res != null)
                        {
                            userDomain = res.Domain;
                        }
                        if (userDomain != null)
                        {
                            TvinciDomains.DeviceContainer[] deviceContainers = userDomain.m_deviceFamilies;
                            if (deviceContainers != null && deviceContainers.Length > 0)
                            {
                                List<int> familyIDs = new List<int>();
                                for (int i = 0; i < deviceContainers.Length; i++)
                                {
                                    TvinciDomains.DeviceContainer container = deviceContainers[i];

                                    if (container != null)
                                    {
                                        if (!familyIDs.Contains(container.m_deviceFamilyID))
                                        {
                                            familyIDs.Add(container.m_deviceFamilyID);
                                        }

                                        if (container.DeviceInstances != null && container.DeviceInstances.Length > 0)
                                        {
                                            for (int j = 0; j < container.DeviceInstances.Length; j++)
                                            {
                                                TvinciDomains.Device device = container.DeviceInstances[j];
                                                if (string.Compare(device.m_deviceUDID.Trim(), sDEVICE_NAME.Trim()) == 0)
                                                {
                                                    isDeviceRecognized = true;
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            familyIDs.Add(container.m_deviceFamilyID);
                                        }

                                        if (container.DeviceInstances != null && container.DeviceInstances.Length > 0)
                                        {
                                            for (int j = 0; j < container.DeviceInstances.Length; j++)
                                            {
                                                TvinciDomains.Device device = container.DeviceInstances[j];
                                                if (string.Compare(device.m_deviceUDID.Trim(), sDEVICE_NAME.Trim()) == 0)
                                                {
                                                    isDeviceRecognized = true;
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //Patch!!
                                            if (container.m_deviceFamilyID == 5 && (string.IsNullOrEmpty(sDEVICE_NAME) || sDEVICE_NAME.ToLower().Equals("web site")))
                                            {
                                                isDeviceRecognized = true;
                                            }
                                        }
                                        if (isDeviceRecognized)
                                        {
                                            break;
                                        }

                                    }
                                }
                                if (!familyIDs.Contains(5) && string.IsNullOrEmpty(sDEVICE_NAME) || (familyIDs.Contains(5) && familyIDs.Count == 0) || (!familyIDs.Contains(5) && sDEVICE_NAME.ToLower().Equals("web site")))
                                {
                                    isDeviceRecognized = true;
                                }
                            }

                        }
                    }
                    else
                    {
                        // No Domain - No device check!!
                        isDeviceRecognized = true;
                    }
                }
            }
            finally
            {
                if (u != null)
                {
                    u.Dispose();
                }
                if (domainsWS != null)
                {
                    domainsWS.Dispose();
                }
            }
            return isDeviceRecognized;
        }



        public virtual LicensedLinkResponse GetEPGLink(string sProgramId, DateTime dStartTime, int format, string sSiteGUID, Int32 nMediaFileID, string sBasicLink, string sUserIP, string sRefferer, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, string sCouponCode)
        {
            return new LicensedLinkResponse();
        }

        /// <summary>
        /// Get Licensed Link
        /// </summary>
        public virtual string GetLicensedLink(string sSiteGUID, Int32 nMediaFileID, string sBasicLink, string sUserIP, string sRefferer, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, string couponCode)
        {
            LicensedLinkResponse llr = GetLicensedLinks(sSiteGUID, nMediaFileID, sBasicLink, sUserIP, sRefferer, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, couponCode);

            return llr.mainUrl;
        }

        /// <summary>
        /// Get Licensed Link With Media File CoGuid
        /// </summary>
        public virtual string GetLicensedLinkWithMediaFileCoGuid(string sSiteGUID, string sMediaFileCoGuid, string sBasicLink, string sUserIP, string sRefferer, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, string couponCode)
        {

            int mediaFileID = 0;
            if (Int32.TryParse(sMediaFileCoGuid, out mediaFileID) && mediaFileID > 0)
            {
                LicensedLinkResponse llr = GetLicensedLinks(sSiteGUID, mediaFileID, sBasicLink, sUserIP, sRefferer, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, couponCode);
                return llr.mainUrl;
            }

            return GetErrorLicensedLink(sBasicLink);
        }

        private bool IsPurchasedAsPartOfPrePaid(MediaFileItemPricesContainer price)
        {
            return price.m_oItemPrices[0].m_relevantPP != null;
        }

        private bool IsPurchasedAsPurePPV(MediaFileItemPricesContainer price)
        {
            return price.m_oItemPrices[0].m_relevantSub == null && price.m_oItemPrices[0].m_relevantCol == null;
        }

        private bool IsPurchasedAsPartOfSub(MediaFileItemPricesContainer price)
        {
            return price.m_oItemPrices[0].m_relevantCol == null;
        }

        private List<int> GetRelatedMediaFiles(MediaFileItemPricesContainer price, int mediaFileID)
        {
            List<int> lRelatedMediaFiles = new List<int>();

            if (price != null && price.m_oItemPrices != null && price.m_oItemPrices.Length > 0 &&
                price.m_oItemPrices[0].m_lRelatedMediaFileIDs != null && price.m_oItemPrices[0].m_lRelatedMediaFileIDs.Length > 0)
            {
                lRelatedMediaFiles.AddRange(price.m_oItemPrices[0].m_lRelatedMediaFileIDs.ToList());
            }
            if (!lRelatedMediaFiles.Contains(mediaFileID))
            {
                lRelatedMediaFiles.Add(mediaFileID);
            }
            return lRelatedMediaFiles;
        }

        private string GetCountryCodeForHandlePlayUses(string userIP, string countryCode)
        {
            string res = countryCode;
            if (!string.IsNullOrEmpty(userIP) && string.IsNullOrEmpty(countryCode))
                res = TVinciShared.WS_Utils.GetIP2CountryCode(userIP);

            return res;
        }

        private int ExtractRelevantCollectionID(MediaFileItemPricesContainer price)
        {
            int res = 0;
            if (price != null && price.m_oItemPrices != null && price.m_oItemPrices.Length > 0 && price.m_oItemPrices[0].m_relevantCol != null)
            {
                Int32.TryParse(price.m_oItemPrices[0].m_relevantCol.m_sObjectCode, out res);
            }

            return res;
        }

        private int ExtractRelevantPrePaidID(MediaFileItemPricesContainer price)
        {
            if (price != null && price.m_oItemPrices != null && price.m_oItemPrices.Length > 0 && price.m_oItemPrices[0].m_relevantSub == null
                && price.m_oItemPrices[0].m_relevantCol == null && price.m_oItemPrices[0].m_relevantPP != null)
            {
                return price.m_oItemPrices[0].m_relevantPP.m_ObjectCode;
            }

            return 0;
        }

        private string GetPurchasingSiteGuid(MediaFileItemPricesContainer price, string inputSiteGuid)
        {
            if (price != null && price.m_oItemPrices != null && price.m_oItemPrices.Length > 0 && price.m_oItemPrices[0] != null &&
                !string.IsNullOrEmpty(price.m_oItemPrices[0].m_sPurchasedBySiteGuid))
            {
                return price.m_oItemPrices[0].m_sPurchasedBySiteGuid;
            }

            return inputSiteGuid;
        }

        /// <summary>
        /// Handle Play Uses
        /// </summary>
        protected void HandlePlayUses(MediaFileItemPricesContainer price, string sSiteGUID, Int32 nMediaFileID, string sUserIP, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, string couponCode)
        {
            sCOUNTRY_CODE = GetCountryCodeForHandlePlayUses(sUserIP, sCOUNTRY_CODE);

            int nReleventCollectionID = ExtractRelevantCollectionID(price);

            HandleCouponUses(price.m_oItemPrices[0].m_relevantSub, price.m_oItemPrices[0].m_sPPVModuleCode, sSiteGUID,
            price.m_oItemPrices[0].m_oPrice.m_dPrice, price.m_oItemPrices[0].m_oPrice.m_oCurrency.m_sCurrencyCD3,
            nMediaFileID, couponCode, sUserIP,
            sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, false, 0, nReleventCollectionID);

            Int32 nRelPP = ExtractRelevantPrePaidID(price);

            List<int> lUsersIds = Utils.GetAllUsersDomainBySiteGUID(sSiteGUID, m_nGroupID);

            if (IsPurchasedAsPurePPV(price))
            {
                string sPPVMCd = price.m_oItemPrices[0].m_sPPVModuleCode;

                Int32 nIsCreditDownloaded = PPV_DoesCreditNeedToDownloaded(sPPVMCd, null, null, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, lUsersIds, GetRelatedMediaFiles(price, nMediaFileID));

                if (ConditionalAccessDAL.Insert_NewPPVUse(m_nGroupID, nMediaFileID, price.m_oItemPrices[0].m_sPPVModuleCode,
                    sSiteGUID, nIsCreditDownloaded > 0, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, nRelPP, nReleventCollectionID) < 1)
                {
                    // failed to insert ppv use.
                    throw new Exception(GetPPVUseInsertionFailureExMsg(nMediaFileID, price.m_oItemPrices[0].m_sPPVModuleCode, sSiteGUID,
                        nIsCreditDownloaded > 0, nRelPP, nReleventCollectionID));
                }


                Int32 nPPVID = 0;
                string sRelSub = string.Empty;
                if (nIsCreditDownloaded == 1)
                {
                    //sRelSub - the subscription that caused the price to be lower

                    nPPVID = GetActivePPVPurchaseID(price.m_oItemPrices[0].m_lPurchasedMediaFileID > 0 ? new List<int>(1) { price.m_oItemPrices[0].m_lPurchasedMediaFileID } : new List<int>(1) { nMediaFileID }, ref sRelSub, lUsersIds);
                    if (nPPVID == 0 && !string.IsNullOrEmpty(couponCode))
                    {
                        InsertPPVPurchases(sSiteGUID, nMediaFileID, price.m_oItemPrices[0].m_oPrice.m_dPrice,
                            price.m_oItemPrices[0].m_oPrice.m_oCurrency.m_sCurrencyCD3, sRelSub, 0, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, sPPVMCd,
                            couponCode, sUserIP);

                        nPPVID = GetActivePPVPurchaseID(new List<int>(1) { nMediaFileID }, ref sRelSub, lUsersIds);
                    }

                    UpdatePPVPurchases(nPPVID, price.m_oItemPrices[0].m_sPPVModuleCode, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);
                }
            }
            else
            {
                string purchasingSiteGuid = GetPurchasingSiteGuid(price, sSiteGUID);
                if (IsPurchasedAsPartOfSub(price))
                {
                    // PPV purchased as part of Subscription

                    Int32 nIsCreditDownloaded = Utils.Bundle_DoesCreditNeedToDownloaded(price.m_oItemPrices[0].m_relevantSub.m_sObjectCode, lUsersIds, GetRelatedMediaFiles(price, nMediaFileID), m_nGroupID, eBundleType.SUBSCRIPTION) ? 1 : 0;

                    if (ConditionalAccessDAL.Insert_NewSubscriptionUse(m_nGroupID, price.m_oItemPrices[0].m_relevantSub.m_sObjectCode, nMediaFileID,
                        sSiteGUID, nIsCreditDownloaded > 0, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, nRelPP) < 1)
                    {
                        // failed to insert subscription use
                        throw new Exception(GetSubUseInsertionFailureExMsg(price.m_oItemPrices[0].m_relevantSub.m_sObjectCode, nMediaFileID, sSiteGUID,
                            nIsCreditDownloaded > 0, nRelPP));
                    }

                    //subscriptions_purchases
                    if (nIsCreditDownloaded == 1)
                    {
                        if (!ConditionalAccessDAL.Update_SubPurchaseNumOfUses(m_nGroupID, purchasingSiteGuid,
                            price.m_oItemPrices[0].m_relevantSub.m_sObjectCode))
                        {
                            // failed to update num of uses in subscriptions_purchases.
                            #region Logging
                            StringBuilder sb = new StringBuilder("Failed to update num of uses in subscriptions_purchases table. ");
                            sb.Append(String.Concat("Sub Cd: ", price.m_oItemPrices[0].m_relevantSub.m_sObjectCode));
                            sb.Append(String.Concat(" Site Guid: ", purchasingSiteGuid));
                            sb.Append(String.Concat(" Group ID: ", m_nGroupID));
                            sb.Append(String.Concat(" MF ID: ", nMediaFileID));

                            log.Debug("CriticalError - " + sb.ToString());
                            #endregion

                        }
                    }

                    string modifiedPPVModuleCode = GetPPVModuleCodeForPPVUses(price.m_oItemPrices[0].m_relevantSub.m_sObjectCode, eTransactionType.Subscription);

                    Int32 nIsCreditDownloaded1 = PPV_DoesCreditNeedToDownloaded(modifiedPPVModuleCode, price.m_oItemPrices[0].m_relevantSub, null, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, lUsersIds, GetRelatedMediaFiles(price, nMediaFileID));
                    if (ConditionalAccessDAL.Insert_NewPPVUse(m_nGroupID, nMediaFileID, modifiedPPVModuleCode,
                        sSiteGUID, nIsCreditDownloaded1 > 0, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, nRelPP, nReleventCollectionID) < 1)
                    {
                        // failed to insert ppv use
                        throw new Exception(GetPPVUseInsertionFailureExMsg(nMediaFileID, modifiedPPVModuleCode, sSiteGUID,
                            nIsCreditDownloaded1 > 0, nRelPP, nReleventCollectionID));
                    }

                    Int32 nPPVID = 0;
                    if (nIsCreditDownloaded1 == 1)
                    {
                        string sRelSub = string.Empty;
                        nPPVID = GetActivePPVPurchaseID(price.m_oItemPrices[0].m_lPurchasedMediaFileID > 0 ? new List<int>(1) { price.m_oItemPrices[0].m_lPurchasedMediaFileID } : new List<int>(1) { nMediaFileID }, ref sRelSub, lUsersIds);

                        if (nPPVID == 0 && !string.IsNullOrEmpty(couponCode))
                        {
                            InsertPPVPurchases(sSiteGUID, nMediaFileID, price.m_oItemPrices[0].m_oPrice.m_dPrice,
                                price.m_oItemPrices[0].m_oPrice.m_oCurrency.m_sCurrencyCD3, sRelSub, 0, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME,
                                price.m_oItemPrices[0].m_sPPVModuleCode, couponCode, sUserIP);

                            nPPVID = GetActivePPVPurchaseID(new List<int>(1) { nMediaFileID }, ref sRelSub, lUsersIds);
                        }

                        UpdatePPVPurchases(nPPVID, price.m_oItemPrices[0].m_sPPVModuleCode, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);
                    }
                }
                else
                {
                    // PPV purchased as part of Collection

                    Int32 nIsCreditDownloaded = Utils.Bundle_DoesCreditNeedToDownloaded(price.m_oItemPrices[0].m_relevantCol.m_sObjectCode, lUsersIds, GetRelatedMediaFiles(price, nMediaFileID), m_nGroupID, eBundleType.COLLECTION) ? 1 : 0;

                    //collections_uses

                    if (ConditionalAccessDAL.Insert_NewCollectionUse(m_nGroupID, price.m_oItemPrices[0].m_relevantCol.m_sObjectCode, nMediaFileID,
                        sSiteGUID, nIsCreditDownloaded > 0, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME) < 1)
                    {
                        // failed to insert values in collections_uses
                        // throw here an exception
                        throw new Exception(GetColUseInsertionFailureMsg(price.m_oItemPrices[0].m_relevantCol.m_sObjectCode, nMediaFileID,
                            sSiteGUID, nIsCreditDownloaded > 0, nRelPP));

                    }
                    if (nIsCreditDownloaded == 1)
                    {
                        if (!ConditionalAccessDAL.Update_ColPurchaseNumOfUses(price.m_oItemPrices[0].m_relevantCol.m_sObjectCode, purchasingSiteGuid, m_nGroupID))
                        {
                            // failed to update num of uses in collections_purchases. logging
                            #region Logging
                            StringBuilder sb = new StringBuilder("Failed to increment num of uses in collections_purchases. ");
                            sb.Append(String.Concat(" Col Code: ", price.m_oItemPrices[0].m_relevantCol.m_sObjectCode));
                            sb.Append(String.Concat(" Group ID: ", m_nGroupID));
                            sb.Append(String.Concat(" Site Guid: ", purchasingSiteGuid));

                            log.Error("CriticalError - " + sb.ToString());
                            #endregion
                        }
                    }

                    string modifiedPPVModuleCode = GetPPVModuleCodeForPPVUses(price.m_oItemPrices[0].m_relevantCol.m_sObjectCode, eTransactionType.Collection);

                    Int32 nIsCreditDownloaded1 = PPV_DoesCreditNeedToDownloaded(modifiedPPVModuleCode, null, price.m_oItemPrices[0].m_relevantCol, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, lUsersIds, GetRelatedMediaFiles(price, nMediaFileID));

                    if (ConditionalAccessDAL.Insert_NewPPVUse(m_nGroupID, nMediaFileID, modifiedPPVModuleCode, sSiteGUID, nIsCreditDownloaded1 > 0,
                        sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, nRelPP, nReleventCollectionID) < 1)
                    {
                        // failed to insert ppv use
                        throw new Exception(GetPPVUseInsertionFailureExMsg(nMediaFileID, modifiedPPVModuleCode, sSiteGUID, nIsCreditDownloaded1 > 0,
                            nRelPP, nReleventCollectionID));
                    }

                    Int32 nPPVID = 0;
                    if (nIsCreditDownloaded1 == 1)
                    {
                        string sRelCol = string.Empty;
                        nPPVID = GetActivePPVPurchaseID(price.m_oItemPrices[0].m_lPurchasedMediaFileID > 0 ? new List<int>(1) { price.m_oItemPrices[0].m_lPurchasedMediaFileID } : new List<int>(1) { nMediaFileID }, ref sRelCol, lUsersIds);

                        if (nPPVID == 0 && !string.IsNullOrEmpty(couponCode))
                        {
                            InsertPPVPurchases(sSiteGUID, nMediaFileID, price.m_oItemPrices[0].m_oPrice.m_dPrice,
                                price.m_oItemPrices[0].m_oPrice.m_oCurrency.m_sCurrencyCD3, sRelCol, 0, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME,
                                price.m_oItemPrices[0].m_sPPVModuleCode, couponCode, sUserIP);

                            nPPVID = GetActivePPVPurchaseID(new List<int>(1) { nMediaFileID }, ref sRelCol, lUsersIds);
                        }

                        UpdatePPVPurchases(nPPVID, price.m_oItemPrices[0].m_sPPVModuleCode, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);
                    }
                }
            }
        }

        private string GetColUseInsertionFailureMsg(string colCode, long mediaFileID, string siteGuid, bool isCreditDownloaded, int relPP)
        {
            StringBuilder sb = new StringBuilder("Failed to insert value into collection_uses table. ");
            sb.Append(String.Concat("Col ID: ", colCode));
            sb.Append(String.Concat(" MF ID: ", mediaFileID));
            sb.Append(String.Concat(" Site Guid: ", siteGuid));
            sb.Append(String.Concat(" Is CD: ", isCreditDownloaded));
            sb.Append(String.Concat(" Rel PP: ", relPP));

            return sb.ToString();

        }

        private string GetSubUseInsertionFailureExMsg(string subCode, long mediaFileID, string siteGuid, bool isCreditDownloaded, int relPP)
        {
            StringBuilder sb = new StringBuilder("Failed to insert sub use. ");
            sb.Append(String.Concat("Sub Code: ", subCode));
            sb.Append(String.Concat(" MF ID: ", mediaFileID));
            sb.Append(String.Concat(" Site Guid: ", siteGuid));
            sb.Append(String.Concat(" Is CD: ", isCreditDownloaded));
            sb.Append(String.Concat(" Rel PP: ", relPP));

            return sb.ToString();
        }

        protected string GetPPVModuleCodeForPPVUses(string ppvModuleCode, eTransactionType purchasedAs)
        {
            string res = string.Empty;
            switch (purchasedAs)
            {
                case eTransactionType.Subscription:
                    res = String.Concat("s: ", ppvModuleCode);
                    break;
                case eTransactionType.Collection:
                    res = String.Concat("b: ", ppvModuleCode);
                    break;
                default:
                    // ppv
                    res = ppvModuleCode;
                    break;

            }

            return res;
        }

        private string GetPPVUseInsertionFailureExMsg(long mediaFileID, string ppvModuleCode, string siteGuid, bool isCreditDownloaded,
            int nRelPP, int nRelevantCol)
        {
            StringBuilder sb = new StringBuilder("Failed to insert new ppv use. ");
            sb.Append(String.Concat("MF ID: ", mediaFileID));
            sb.Append(String.Concat(" PPV MC: ", ppvModuleCode));
            sb.Append(String.Concat(" Site Guid: ", siteGuid));
            sb.Append(String.Concat(" Is CD: ", isCreditDownloaded));
            sb.Append(String.Concat(" Rel PP: ", nRelPP));
            sb.Append(String.Concat(" Rel Col: ", nRelevantCol));

            return sb.ToString();
        }

        protected bool IsLastView(Int32 nPPVPurchaseID, ref DateTime endDateTime)
        {
            int nMaxNumOfUses = 0;
            int nNumOfUses = 0;
            ConditionalAccessDAL.Get_IsLastViewData(nPPVPurchaseID, ref nNumOfUses, ref nMaxNumOfUses, ref endDateTime);

            return nNumOfUses + 1 >= nMaxNumOfUses;
        }

        protected TvinciPricing.PPVModule GetPPVModule(string sPPVModuleCode, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;

            using (TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule())
            {
                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                {
                    m.Url = sWSURL;
                }
                Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                TvinciPricing.PPVModule thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVModuleCode, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);

                return thePPVModule;
            }
        }

        /// <summary>
        /// Update PPV Purchases
        /// </summary>
        protected void UpdatePPVPurchases(Int32 nPPVPurchaseID, string sPPVModuleCode, string sCOUNTRY_CODE,
            string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            DateTime endDateTime = DateTime.UtcNow;
            DateTime d = DateTime.UtcNow;

            // Check if this is the last watch credit, also return the full view end date
            bool bIsLastView = IsLastView(nPPVPurchaseID, ref endDateTime);

            TvinciPricing.PPVModule thePPVModule = null;
            if (bIsLastView)
            {
                thePPVModule = GetPPVModule(sPPVModuleCode, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);

                d = Utils.GetEndDateTime(DateTime.UtcNow, thePPVModule.m_oUsageModule.m_tsViewLifeCycle);
                // if view cycle is far then the full view date consider the full view date to be the end date
                if (endDateTime < d)
                {
                    bIsLastView = false;
                }
            }

            if (!ConditionalAccessDAL.Update_PPVNumOfUses(nPPVPurchaseID, bIsLastView ? (DateTime?)d : null))
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Error at UpdatePPVPurchases. Probably failed to update num of uses value. ");
                sb.Append(String.Concat(" PPV Purchase ID: ", nPPVPurchaseID));
                sb.Append(String.Concat(" PPV M CD: ", sPPVModuleCode));
                log.Error("Error - " + sb.ToString());
                #endregion
            }
        }
        /// <summary>
        /// Insert PPV Purchases
        /// </summary>
        protected void InsertPPVPurchases(string sSiteGUID, Int32 nMediaFileID, double dPrice, string sCurrency, string sSubCode,
            Int32 nRecieptCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sPPVModuleCode, string sCouponCode, string sUserIP)
        {
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;
            TvinciPricing.mdoule m = null;
            ODBCWrapper.InsertQuery insertQuery = null;
            try
            {
                m = new global::ConditionalAccess.TvinciPricing.mdoule();
                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                {
                    m.Url = sWSURL;
                }
                Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                TvinciPricing.PPVModule thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                TvinciPricing.Subscription relevantSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);

                Int32 nMediaID = Utils.GetMediaIDFromFileID(nMediaFileID, m_nGroupID);

                string sCustomData = GetCustomData(relevantSub, thePPVModule, null, sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode, string.Empty, sCouponCode,
                    sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);


                insertQuery = new ODBCWrapper.InsertQuery("ppv_purchases");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                if (relevantSub != null)
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", relevantSub.m_sObjectCode);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrency);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", sCustomData);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", nRecieptCode);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLANGUAGE_CODE);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDEVICE_NAME);
                if (thePPVModule != null &&
                    thePPVModule.m_oUsageModule != null)
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", thePPVModule.m_oUsageModule.m_nMaxNumberOfViews);
                else
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);

                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                if (thePPVModule != null &&
                    thePPVModule.m_oUsageModule != null)
                {
                    DateTime d = Utils.GetEndDateTime(DateTime.UtcNow, thePPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);
                }

                insertQuery.Execute();
            }
            finally
            {
                #region Disposing
                if (m != null)
                {
                    m.Dispose();
                }
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
                #endregion
            }
        }

        /// <summary>
        /// Update Susbscription Purchase
        /// </summary>
        protected void UpdateCollectionPurchases(string sColCd, string sSiteGUID)
        {
            ODBCWrapper.DirectQuery directQuery = null;
            try
            {
                directQuery = new ODBCWrapper.DirectQuery();
                directQuery += "update collections_purchases set NUM_OF_USES=NUM_OF_USES+1,LAST_VIEW_DATE=getdate() where ";
                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", GetActiveCollectionPurchaseID(sColCd, sSiteGUID));
                directQuery.Execute();
            }
            finally
            {
                if (directQuery != null)
                {
                    directQuery.Finish();
                }
            }
        }

        private Int32 GetActivePPVPurchaseID(List<int> relatedMediaFileIDs, ref string sRelSub, List<int> lUsersIds)
        {
            Int32 nRet = 0;
            DataTable dt = ConditionalAccessDAL.Get_AllPPVPurchasesByUserIDsAndMediaFileIDs(m_nGroupID, relatedMediaFileIDs, lUsersIds);

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {

                nRet = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["ID"]);
                sRelSub = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["SUBSCRIPTION_CODE"]);
            }
            return nRet;
        }

        /// <summary>
        /// Get Active Subscription Purchase ID
        /// </summary>
        protected Int32 GetActiveSubscriptionPurchaseID(string sSubCd, string sSiteGUID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select ID from subscriptions_purchases with (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                selectQuery += " and (MAX_NUM_OF_USES>=NUM_OF_USES OR MAX_NUM_OF_USES=0) and START_DATE<getdate() and (end_date is null or end_date>getdate()) and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubCd);
                selectQuery += " and ";
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return nRet;
        }

        /// <summary>
        /// Get Active Collection Purchase ID
        /// </summary>
        protected Int32 GetActiveCollectionPurchaseID(string sColCd, string sSiteGUID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select ID from collections_purchases with (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                selectQuery += " and (MAX_NUM_OF_USES>=NUM_OF_USES OR MAX_NUM_OF_USES=0) and START_DATE<getdate() and (end_date is null or end_date>getdate()) and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COLLECTION_CODE", "=", sColCd);
                selectQuery += " and ";
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return nRet;
        }

        private PPVModule GetPPVModuleDataForDoesCreditNeedToDownload(string ppvModuleCode)
        {
            string actualPPVModuleCode = string.Empty;
            if (ppvModuleCode.Contains("s: "))
                actualPPVModuleCode = ppvModuleCode.Replace("s: ", string.Empty);
            else
            {
                if (ppvModuleCode.Contains("b: "))
                    actualPPVModuleCode = ppvModuleCode.Replace("b: ", string.Empty);
                else
                    actualPPVModuleCode = ppvModuleCode;
            }

            string wsUsername = string.Empty, wsPassword = string.Empty;
            Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref wsUsername, ref wsPassword);

            return Utils.GetPPVModuleDataWithCaching(actualPPVModuleCode, wsUsername, wsPassword, m_nGroupID, string.Empty, string.Empty, string.Empty);

        }
        protected int PPV_DoesCreditNeedToDownloaded(string sPPVMCd, TvinciPricing.Subscription theSub,
            TvinciPricing.Collection theCol, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME,
            List<int> lUsersIds, List<int> mediaFileIDs)
        {
            Int32 nIsCreditDownloaded = 1;
            Int32 nViewLifeCycle = 0;
            int OfflineStatus = 0;
            TvinciPricing.mdoule m = null;
            try
            {
                if (OfflineStatus == 1)
                {
                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;

                    m = new global::ConditionalAccess.TvinciPricing.mdoule();
                    string sWSURL = Utils.GetWSURL("pricing_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        m.Url = sWSURL;
                    }
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                    TvinciPricing.UsageModule OfflineUsageModule = m.GetOfflineUsageModule(sWSUserName, sWSPass, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);
                    nViewLifeCycle = OfflineUsageModule.m_tsViewLifeCycle;
                }
                else if (theSub == null && theCol == null)
                {
                    TvinciPricing.PPVModule ppvModule = GetPPVModuleDataForDoesCreditNeedToDownload(sPPVMCd);
                    if (ppvModule == null)
                    {
                        throw new Exception(String.Concat("PPV_DoesCreditNeedToDownloaded. PPV Module was returned null by WS_Pricing. PPV Code: ", sPPVMCd));
                    }

                    nViewLifeCycle = ppvModule.m_oUsageModule.m_tsViewLifeCycle;
                }
                else if (theSub != null && theSub.m_oSubscriptionUsageModule != null)
                {
                    nViewLifeCycle = theSub.m_oSubscriptionUsageModule.m_tsViewLifeCycle;
                }
                else if (theCol != null && theCol.m_oCollectionUsageModule != null)
                {
                    nViewLifeCycle = theCol.m_oCollectionUsageModule.m_tsViewLifeCycle;
                }

                DataTable dtPPVUses = ConditionalAccessDAL.Get_AllDomainPPVUsesByMediaFiles(m_nGroupID, lUsersIds, mediaFileIDs, sPPVMCd);

                if (dtPPVUses != null && dtPPVUses.Rows != null && dtPPVUses.Rows.Count > 0)
                {
                    DateTime dNow = ODBCWrapper.Utils.GetDateSafeVal(dtPPVUses.Rows[0]["dNow"]);
                    DateTime dUsed = ODBCWrapper.Utils.GetDateSafeVal(dtPPVUses.Rows[0]["CREATE_DATE"]);

                    DateTime dEndDate = Utils.GetEndDateTime(dUsed, nViewLifeCycle);

                    if (dNow < dEndDate)
                        nIsCreditDownloaded = 0;

                }
            }
            finally
            {
                if (m != null)
                {
                    m.Dispose();
                }
            }

            return nIsCreditDownloaded;
        }

        /// <summary>
        /// Built Refference String 
        /// </summary>
        protected string BuiltRefferenceString(Int32 nMediaFileID, string sSubscriptionCode, string sPPVCode,
            string sPriceCode, double dPrice, string sCurrency)
        {
            string sRet = "";
            if (nMediaFileID != 0)
                sRet += "mf:" + nMediaFileID.ToString() + " ";
            if (sSubscriptionCode != "")
                sRet += "sub:" + sSubscriptionCode + " ";
            if (sPPVCode != "")
                sRet += "ppvcode:" + sPPVCode + " ";
            if (sPriceCode != "")
                sRet += "pricecode:" + sPriceCode + " ";
            sRet += "price:" + dPrice.ToString() + " ";
            sRet += "currency:" + sCurrency + " ";
            return sRet;
        }
        /// <summary>
        /// SMS Charge User For Media File 
        /// </summary>
        public virtual TvinciBilling.BillingResponse SMS_ChargeUserForMediaFile(string sSiteGUID, string sCellPhone, double dPrice, string sCurrency, Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCouponCode, string sExtraParameters,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            TvinciBilling.module bm = null;
            TvinciUsers.UsersService u = null;
            TvinciPricing.mdoule m = null;

            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            try
            {
                if (sCellPhone.Trim() == "")
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "Problematic Cell Phone: " + sCellPhone;
                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                    return ret;
                }
                else if (sSiteGUID == "")
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                    return ret;
                }
                else
                {
                    u = new ConditionalAccess.TvinciUsers.UsersService();

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }

                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = "";
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                        return ret;
                    }
                    else if (uObj != null && uObj.m_user != null && uObj.m_user.m_eSuspendState == TvinciUsers.DomainSuspentionStatus.Suspended)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UserSuspended;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cannot charge a suspended user";
                        WriteToUserLog(sSiteGUID, "while trying to purchase  media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                        return ret;
                    }
                    else
                    {
                        sWSUserName = string.Empty;
                        sWSPass = string.Empty;

                        m = new global::ConditionalAccess.TvinciPricing.mdoule();
                        sWSURL = Utils.GetWSURL("pricing_ws");
                        if (!string.IsNullOrEmpty(sWSURL))
                        {
                            m.Url = sWSURL;
                        }
                        Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                        if (string.IsNullOrEmpty(sPPVModuleCode))
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = string.Empty;
                            ret.m_sStatusDescription = "Charge must have ppv module code";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }

                        // chack if ppvModule related to mediaFile 
                        long ppvModuleCode = 0;
                        long.TryParse(sPPVModuleCode, out ppvModuleCode);

                        TvinciPricing.PPVModule thePPVModule = m.ValidatePPVModuleForMediaFile(sWSUserName, sWSPass, nMediaFileID, ppvModuleCode);
                        if (thePPVModule == null)
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = string.Empty;
                            ret.m_sStatusDescription = "The ppv module is unknown";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }
                        else if (thePPVModule.m_sObjectCode != ppvModuleCode.ToString() && !string.IsNullOrEmpty(sPPVModuleCode))
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownPPVModule;
                            ret.m_sRecieptCode = string.Empty;
                            ret.m_sStatusDescription = "This PPVModule does not belong to item";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }

                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.Subscription relevantSub = null;
                        TvinciPricing.Collection relevantCol = null;
                        TvinciPricing.PrePaidModule relevantPP = null;


                        TvinciPricing.Price p = Utils.GetMediaFileFinalPriceForNonGetItemsPrices(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (theReason == PriceReason.ForPurchase)
                        {
                            if (p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency)
                            {
                                bm = new ConditionalAccess.TvinciBilling.module();
                                sWSUserName = string.Empty;
                                sWSPass = string.Empty;
                                Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                                sWSURL = Utils.GetWSURL("billing_ws");
                                if (!string.IsNullOrEmpty(sWSURL))
                                {
                                    bm.Url = sWSURL;
                                }
                                string sPPVModule = "";
                                if (thePPVModule != null)
                                    sPPVModule = thePPVModule.m_sObjectCode;

                                //Create the Custom Data
                                string sCustomData = GetCustomData(relevantSub, thePPVModule, null, sSiteGUID, dPrice, sCurrency,
                                    nMediaFileID, nMediaID, sPPVModuleCode, string.Empty, sCouponCode, string.Empty,
                                    sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                                log.Debug("SMS CustomData - " + sCustomData);

                                if (relevantSub != null)
                                {
                                    ret = bm.SMS_SendCode(sWSUserName, sWSPass, sSiteGUID, sCellPhone, sCustomData, sExtraParameters);
                                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): SMS code sent to: " + sCellPhone);
                                }
                                else
                                {
                                    ret = bm.SMS_SendCode(sWSUserName, sWSPass, sSiteGUID, sCellPhone, sCustomData, sExtraParameters);
                                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): SMS code sent to: " + sCellPhone);
                                }
                            }
                            else
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                                ret.m_sRecieptCode = "";
                                ret.m_sStatusDescription = "Mismatch in price or currency";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                        }
                        else
                        {
                            if (theReason == PriceReason.PPVPurchased)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = "";
                                ret.m_sStatusDescription = "The media file is already purchased";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                            if (theReason == PriceReason.SubscriptionPurchased)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = "";
                                ret.m_sStatusDescription = "The media file is contained in a purchased subscription";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                            if (theReason == PriceReason.Free)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = "";
                                ret.m_sStatusDescription = "The media file is free";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                            if (theReason == PriceReason.ForPurchaseSubscriptionOnly)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = "";
                                ret.m_sStatusDescription = "The media file is for purchase with subscription only";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                            else if (theReason == PriceReason.NotForPurchase)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "The media file is not valid for purchased";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                        }
                    }
                    return ret;
                }
            }
            catch (Exception ex)
            {
                log.Error("exception - " + ex.Message + "||" + ex.StackTrace, ex);
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = ex.Message + "||" + ex.StackTrace;
                return ret;
            }
            finally
            {
                if (u != null)
                {
                    u.Dispose();
                }
                if (m != null)
                {
                    m.Dispose();
                }
                if (bm != null)
                {
                    bm.Dispose();
                }
            }
        }

        /// <summary>
        /// SMS Charge User For Subscription
        /// </summary>
        public virtual TvinciBilling.BillingResponse SMS_ChargeUserForSubscription(string sSiteGUID, string sCellPhone, double dPrice, string sCurrency, string sSubscriptionCode, string sCouponCode, string sExtraParameters,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            TvinciUsers.UsersService u = null;
            TvinciBilling.module bm = null;
            try
            {
                if (string.IsNullOrEmpty(sSiteGUID))
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                }
                else
                {
                    u = new ConditionalAccess.TvinciUsers.UsersService();

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }
                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = "";
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                    }
                    else if (uObj != null && uObj.m_user != null && uObj.m_user.m_eSuspendState == TvinciUsers.DomainSuspentionStatus.Suspended)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UserSuspended;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cannot charge a suspended user";
                        WriteToUserLog(sSiteGUID, "while trying to purchase subscription(SMS): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription);
                    }
                    else
                    {
                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.Subscription theSub = null;
                        TvinciPricing.Price p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubscriptionCode, sSiteGUID, sCouponCode, ref theReason, ref theSub, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (theReason == PriceReason.ForPurchase)
                        {
                            if (p != null && p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency)
                            {
                                if (p.m_dPrice != 0)
                                {
                                    bm = new ConditionalAccess.TvinciBilling.module();
                                    sWSUserName = string.Empty;
                                    sWSPass = string.Empty;
                                    Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                                    sWSURL = Utils.GetWSURL("billing_ws");
                                    if (!string.IsNullOrEmpty(sWSURL))
                                    {
                                        bm.Url = sWSURL;
                                    }
                                    if (theSub != null)
                                    {
                                        bool bIsRecurring = theSub.m_bIsRecurring;
                                        Int32 nRecPeriods = theSub.m_nNumberOfRecPeriods;

                                        string sCustomData = GetCustomDataForSubscription(theSub, null, sSubscriptionCode, string.Empty, sSiteGUID, dPrice, sCurrency,
                                        sCouponCode, string.Empty, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                                        log.Debug("SMS CustomData - " + sCustomData);

                                        ret = bm.SMS_SendCode(sWSUserName, sWSPass, sSiteGUID, sCellPhone, sCustomData, sExtraParameters);
                                        WriteToUserLog(sSiteGUID, "While trying to purchase subscription(SMS): " + sSubscriptionCode + " SMS code sent to: " + sCellPhone);
                                    }
                                }
                            }
                            else
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                                ret.m_sRecieptCode = "";
                                ret.m_sStatusDescription = "The price of the request is not the actual price";
                                WriteToUserLog(sSiteGUID, "While trying to purchase subscription(SMS): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription);
                            }
                        }
                        else
                        {
                            if (theReason == PriceReason.SubscriptionPurchased)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = "";
                                ret.m_sStatusDescription = "The subscription is already purchased";
                                WriteToUserLog(sSiteGUID, "While trying to purchase subscription(SMS): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription);
                            }
                            if (theReason == PriceReason.Free)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = "";
                                ret.m_sStatusDescription = "The subscription is free";
                                WriteToUserLog(sSiteGUID, "While trying to purchase subscription(SMS): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at SMS_ChargeUserForSubscription. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Cell Phone: ", sCellPhone));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" Curr: ", sCurrency));
                sb.Append(String.Concat(" Sub Code: ", sSubscriptionCode));
                sb.Append(String.Concat(" Cpn Cd: ", sCouponCode));
                sb.Append(String.Concat(" Extra Params: ", sExtraParameters));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" Device Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                }
                if (bm != null)
                {
                    bm.Dispose();
                }
                #endregion
            }
            return ret;
        }
        /// <summary>
        /// Get User CA Sub Status
        /// </summary>
        protected virtual bool GetUserCASubStatus(string sSiteGUID, ref UserCAStatus oUserCAStatus)
        {
            return false;
        }
        /// <summary>
        /// Get User CA Status
        /// </summary>
        public virtual UserCAStatus GetUserCAStatus(string sSiteGUID)
        {
            if (sSiteGUID == "" || sSiteGUID == "0")
                return UserCAStatus.Annonymus;

            ODBCWrapper.DataSetSelectQuery selectQuery1 = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                UserCAStatus subStatus = UserCAStatus.NeverPurchased;
                bool subFound = GetUserCASubStatus(sSiteGUID, ref subStatus);
                if (subFound)
                {
                    return subStatus;
                }

                PermittedMediaContainer[] ppvItems = GetUserPermittedItems(sSiteGUID);
                if (ppvItems != null)
                {
                    if (ppvItems.Length > 0)
                        return UserCAStatus.CurrentPPV;
                }

                Int32 nPastSub = 0;
                selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery1 += "select count(*) as co from subscriptions_purchases with (nolock) where is_active=1 and status=1 and ";
                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                if (selectQuery1.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery1.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nPastSub = int.Parse(selectQuery1.Table("query").DefaultView[0].Row["co"].ToString());
                    }
                }

                if (nPastSub > 0)
                    return UserCAStatus.ExSub;

                Int32 nPastPPV = 0;
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select count(*) as co from ppv_purchases with (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nPastPPV = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                    }
                }

                if (nPastPPV > 0)
                    return UserCAStatus.ExPPV;
            }
            finally
            {
                if (selectQuery1 != null)
                {
                    selectQuery1.Finish();
                }
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return UserCAStatus.NeverPurchased;
        }
        /// <summary>
        /// Get Billing Trans Method
        /// </summary>
        private PaymentMethod GetBillingTransMethod(int billingTransID)
        {
            PaymentMethod retVal = PaymentMethod.Unknown;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += " select BILLING_METHOD from billing_transactions with (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", billingTransID);
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
                                retVal = (PaymentMethod)billingInt;
                            }
                        }
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
        /// <summary>
        /// Get Domains Users
        /// </summary>
        private List<int> GetDomainsUsers(int nDomainID)
        {
            using (TvinciDomains.module bm = new ConditionalAccess.TvinciDomains.module())
            {
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                Utils.GetWSCredentials(m_nGroupID, eWSModules.DOMAINS, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("domains_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                {
                    bm.Url = sWSURL;
                }
                string[] usersList = bm.GetDomainUserList(sWSUserName, sWSPass, nDomainID);
                List<int> intUsersList = new List<int>();

                if (usersList != null && usersList.Length != 0)
                {
                    foreach (string str in usersList)
                    {
                        intUsersList.Add(int.Parse(str));
                    }
                }

                return intUsersList;
            }
        }
        /// <summary>
        /// Get Domain Permitted Items
        /// </summary>
        public virtual PermittedMediaContainer[] GetDomainPermittedItems(int nDomainID)
        {
            List<int> intUsersList = GetDomainsUsers(nDomainID);

            return GetUserPermittedItems(intUsersList, false, 0);
        }
        /// <summary>
        /// Get Domain Permitted Subscriptions
        /// </summary>
        public virtual PermittedSubscriptionContainer[] GetDomainPermittedSubscriptions(int nDomainID)
        {
            List<int> intUsersList = GetDomainsUsers(nDomainID);

            return GetUserPermittedSubscriptions(intUsersList, false, 0);
        }
        /// <summary>
        /// Get Domain Permitted Collections
        /// </summary>
        public virtual PermittedCollectionContainer[] GetDomainPermittedCollections(int nDomainID)
        {
            List<int> intUsersList = GetDomainsUsers(nDomainID);

            return GetUserPermittedCollections(intUsersList, false, 0);
        }
        /// <summary>
        /// Get User Permitted Items
        /// </summary>
        public virtual PermittedMediaContainer[] GetUserPermittedItems(string sSiteGUID)
        {
            int nSiteGuid = 0;
            PermittedMediaContainer[] res = null;
            if (!string.IsNullOrEmpty(sSiteGUID) && Int32.TryParse(sSiteGUID, out nSiteGuid) && nSiteGuid > 0)
            {
                res = GetUserPermittedItems(new List<int>(1) { nSiteGuid }, false, 0);
            }
            else
            {
                StringBuilder sb = new StringBuilder("GetUserPermittedItems. SiteGUID is in incorrect format. ");
                sb.Append(String.Concat("Group ID: ", m_nGroupID));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));

                log.Error("Error - " + sb.ToString());

                res = new PermittedMediaContainer[0];
            }

            return res;
        }
        /// <summary>
        /// Get User Permitted Items
        /// </summary>
        public virtual PermittedMediaContainer[] GetUserPermittedItems(List<int> lUsersIDs, bool isExpired, int numOfItems)
        {
            //PermittedMediaContainer[] ret = null;
            PermittedMediaContainer[] ret = { };
            Int32[] nMediaFilesIDs = null;
            Hashtable h = new Hashtable();
            try
            {
                DataTable allPPVModules = ConditionalAccessDAL.Get_All_Users_PPV_modules(lUsersIDs, isExpired);

                if (allPPVModules != null)
                {
                    Int32 nCount = allPPVModules.Rows.Count;
                    if (numOfItems == 0)
                    {
                        numOfItems = nCount;
                    }
                    else if (numOfItems != 0 && numOfItems < nCount)
                    {
                        nCount = numOfItems;
                    }
                    if (nCount > 0)
                        ret = new PermittedMediaContainer[nCount];

                    nMediaFilesIDs = new int[nCount];
                    int i = 0;

                    TvinciPricing.UsageModule oUsageModule = null;
                    foreach (DataRow dataRow in allPPVModules.Rows)
                    {
                        Int32 nMediaFileID = ODBCWrapper.Utils.GetIntSafeVal(dataRow["MEDIA_FILE_ID"]);
                        Int32 nMaxUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow["MAX_NUM_OF_USES"]);
                        Int32 nCurrentUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow["NUM_OF_USES"]);

                        int billingTransID = ODBCWrapper.Utils.GetIntSafeVal(dataRow["billing_transaction_id"]);
                        DateTime dEnd = new DateTime(2099, 1, 1);
                        if (dataRow["END_DATE"] != null && dataRow["END_DATE"] != DBNull.Value)
                            dEnd = (DateTime)(dataRow["END_DATE"]);

                        // get last view date
                        DateTime dLastViewDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow["LAST_VIEW_DATE"]);

                        DateTime dCurrent = DateTime.UtcNow;
                        if (dataRow["cDate"] != null && dataRow["cDate"] != DBNull.Value)
                            dCurrent = (DateTime)(dataRow["cDate"]);

                        DateTime dCreateDate = DateTime.UtcNow;
                        if (dataRow["CREATE_DATE"] != null && dataRow["CREATE_DATE"] != DBNull.Value)
                            dCreateDate = (DateTime)(dataRow["CREATE_DATE"]);

                        PaymentMethod payMet = GetBillingTransMethod(billingTransID);

                        string sDeviceUDID = ODBCWrapper.Utils.GetSafeStr(dataRow["device_name"]);

                        nMediaFilesIDs[i] = nMediaFileID;


                        string sPPVCode = ODBCWrapper.Utils.GetSafeStr(dataRow, "ppv");

                        bool bCancellationWindow = false;
                        int nWaiver = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "WAIVER");

                        if (nWaiver == 0) // user didn't waiver yet
                        {
                            IsCancellationWindow(ref oUsageModule, sPPVCode, dCreateDate, ref bCancellationWindow, eTransactionType.PPV);
                        }


                        PermittedMediaContainer p = new PermittedMediaContainer();
                        p.Initialize(0, nMediaFileID, nMaxUses, nCurrentUses, dEnd, dCurrent, dLastViewDate, dCreateDate, payMet, sDeviceUDID, bCancellationWindow);
                        h[nMediaFileID] = p;
                        ++i;
                    }
                }

                TvinciAPI.MeidaMaper[] mapper = Utils.GetMediaMapper(m_nGroupID, nMediaFilesIDs);
                if (mapper == null)
                {
                    return ret;
                }
                Int32 nCo = mapper.Length;
                if (nCo > 0)
                    ret = new PermittedMediaContainer[nCo];
                for (int i = 0; i < nCo; i++)
                {
                    Int32 nMediaFileID = mapper[i].m_nMediaFileID;
                    if (h.Contains(nMediaFileID) == true)
                    {
                        ((PermittedMediaContainer)(h[nMediaFileID])).m_nMediaID = mapper[i].m_nMediaID;
                        ret[i] = (PermittedMediaContainer)(h[nMediaFileID]);
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at GetUserPermittedItems.");
                sb.Append(String.Concat(" Group ID: ", m_nGroupID));
                sb.Append(String.Concat(" User IDs: ", lUsersIDs == null ? "null" : lUsersIDs.Aggregate<int, string>(string.Empty, (res, next) => (String.Concat(res, ":", next)))));
                sb.Append(String.Concat(" isExpired: ", isExpired.ToString().ToLower()));
                sb.Append(String.Concat(" numOfItems: ", numOfItems));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Exception msg: ", ex.Message));
                sb.Append(String.Concat(" Stack trace: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
            }
            return ret;

        }
        /// <summary>
        /// Get User Permitted Subscriptions
        /// </summary>
        public virtual PermittedSubscriptionContainer[] GetUserPermittedSubscriptions(string sSiteGUID)
        {
            return GetUserPermittedSubscriptions(new List<int>() { int.Parse(sSiteGUID) }, false, 0);
        }
        /// <summary>
        /// Get User Permitted Collections
        /// </summary>
        public virtual PermittedCollectionContainer[] GetUserPermittedCollections(string sSiteGUID)
        {
            return GetUserPermittedCollections(new List<int>() { int.Parse(sSiteGUID) }, false, 0);
        }
        /// <summary>
        /// Get User Permitted Collections
        /// </summary>
        public virtual PermittedCollectionContainer[] GetUserPermittedCollections(List<int> lUsersIDs, bool isExpired, int numOfItems)
        {
            PermittedCollectionContainer[] ret = null;
            try
            {
                DataTable allCollectionsPurchases = ConditionalAccessDAL.Get_UsersPermittedCollections(lUsersIDs, isExpired);

                if (allCollectionsPurchases != null)
                {
                    Int32 nCount = allCollectionsPurchases.Rows.Count;
                    if (numOfItems == 0)
                    {
                        numOfItems = nCount;
                    }
                    else if (numOfItems != 0 && numOfItems < nCount)
                    {
                        nCount = numOfItems;
                    }
                    if (nCount > 0)
                    {
                        ret = new PermittedCollectionContainer[nCount];
                    }
                    int i = 0;
                    TvinciPricing.UsageModule oUsageModule = null;
                    foreach (DataRow dataRow in allCollectionsPurchases.Rows)
                    {
                        //take care of numOfItem< nCount 
                        if (numOfItems <= i)
                        {
                            break;
                        }
                        string sCollectionCode = ODBCWrapper.Utils.GetSafeStr(dataRow["COLLECTION_CODE"]);
                        Int32 nMaxUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow["MAX_NUM_OF_USES"]);
                        Int32 nCurrentUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow["NUM_OF_USES"]);
                        int billingTransID = ODBCWrapper.Utils.GetIntSafeVal(dataRow["billing_transaction_id"]);

                        DateTime dEnd = ODBCWrapper.Utils.GetDateSafeVal(dataRow["END_DATE"]);
                        DateTime dCurrent = ODBCWrapper.Utils.GetDateSafeVal(dataRow["cDate"]);
                        DateTime dCreateDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow["CREATE_DATE"]);
                        DateTime dLastViewDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow["LAST_VIEW_DATE"]);

                        if (isExpired && nMaxUses != 0 && nCurrentUses >= nMaxUses)
                        {
                            dEnd = dLastViewDate;
                        }

                        Int32 nID = ODBCWrapper.Utils.GetIntSafeVal(dataRow["ID"]);
                        PaymentMethod payMet = GetBillingTransMethod(billingTransID);
                        string sDeviceUDID = ODBCWrapper.Utils.GetSafeStr(dataRow["device_name"]);

                        bool bCancellationWindow = false;
                        int nWaiver = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "WAIVER");
                        if (nWaiver == 0) // user didn't waiver yet
                        {
                            IsCancellationWindow(ref oUsageModule, sCollectionCode, dCreateDate, ref bCancellationWindow, eTransactionType.Collection);
                        }

                        PermittedCollectionContainer pcc = new PermittedCollectionContainer();
                        pcc.Initialize(sCollectionCode, dEnd, dCurrent, dLastViewDate, dCreateDate, nID, payMet, sDeviceUDID, bCancellationWindow);
                        ret[i] = pcc;
                        ++i;
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetUserPermittedCollections. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                if (lUsersIDs != null && lUsersIDs.Count > 0)
                {
                    sb.Append(" User IDs: ");
                    for (int i = 0; i < lUsersIDs.Count; i++)
                    {
                        sb.Append(String.Concat(" ", lUsersIDs[i], " "));
                    }
                }
                else
                {
                    sb.Append(" No users. ");
                }
                sb.Append(String.Concat(" IsExpired: ", isExpired.ToString().ToLower()));
                sb.Append(String.Concat(" NumOfItems: ", numOfItems));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion

            }
            return ret;
        }

        private void IsCancellationWindow(ref TvinciPricing.UsageModule oUsageModule, string sAssetCode, DateTime dCreateDate, ref bool bCancellationWindow, eTransactionType transaction)
        {
            //get the right usage module for each ppv
            using (TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule())
            {
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;

                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                {
                    m.Url = sWSURL;
                }
                string transactionName = Enum.GetName(typeof(eTransactionType), transaction);

                Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                TvinciPricing.eTransactionType enumPricing = (TvinciPricing.eTransactionType)Enum.Parse(typeof(TvinciPricing.eTransactionType), transactionName);
                oUsageModule = m.GetUsageModule(sWSUserName, sWSPass, sAssetCode, enumPricing);

                if (oUsageModule != null)
                {
                    if (oUsageModule.m_bWaiver) // if this usage module need to be waiver - check the date
                    {
                        DateTime waiverDate = Utils.GetEndDateTime(dCreateDate, oUsageModule.m_nWaiverPeriod); // dCreateDate = ppv purchase date
                        if (DateTime.UtcNow <= waiverDate)
                        {
                            bCancellationWindow = true;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Get User Permitted Subscriptions
        /// </summary>
        public virtual PermittedSubscriptionContainer[] GetUserPermittedSubscriptions(List<int> lUsersIDs, bool isExpired, int numOfItems)
        {
            PermittedSubscriptionContainer[] ret = null;
            try
            {
                DataTable allSubscriptionsPurchases = ConditionalAccessDAL.Get_UsersPermittedSubscriptions(lUsersIDs, isExpired);

                if (allSubscriptionsPurchases != null)
                {
                    Int32 nCount = allSubscriptionsPurchases.Rows.Count;
                    if (numOfItems == 0)
                    {
                        numOfItems = nCount;
                    }
                    else if (numOfItems != 0 && numOfItems < nCount)
                    {
                        nCount = numOfItems;
                    }
                    if (nCount > 0)
                    {
                        ret = new PermittedSubscriptionContainer[nCount];
                    }
                    int i = 0;

                    TvinciPricing.UsageModule oUsageModule = null;

                    foreach (DataRow dataRow in allSubscriptionsPurchases.Rows)
                    {
                        //take care of numOfItem< nCount 
                        if (numOfItems <= i)
                        {
                            break;
                        }
                        DateTime dNextRenewalDate = DateTime.MaxValue;
                        bool bRecurringStatus = false;
                        bool bIsSubRenewable = false;
                        int nPurchaseID = ODBCWrapper.Utils.GetIntSafeVal(dataRow["ID"]);
                        string sSubscriptionCode = ODBCWrapper.Utils.GetSafeStr(dataRow["SUBSCRIPTION_CODE"]);

                        Int32 nMaxUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow["MAX_NUM_OF_USES"]);
                        Int32 nCurrentUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow["NUM_OF_USES"]);
                        int billingTransID = ODBCWrapper.Utils.GetIntSafeVal(dataRow["billing_transaction_id"]);

                        DateTime dEnd = ODBCWrapper.Utils.GetDateSafeVal(dataRow["END_DATE"]);
                        DateTime dCurrent = ODBCWrapper.Utils.GetDateSafeVal(dataRow["cDate"]);
                        DateTime dCreateDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow["CREATE_DATE"]);
                        DateTime dLastViewDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow["LAST_VIEW_DATE"]);

                        Int32 nIsRecurringStatus = ODBCWrapper.Utils.GetIntSafeVal(dataRow["IS_RECURRING_STATUS"]);
                        if (nIsRecurringStatus == 1)
                        {
                            bRecurringStatus = true;
                            dNextRenewalDate = dEnd;
                        }

                        if (isExpired && nMaxUses != 0 && nCurrentUses >= nMaxUses)
                        {
                            dEnd = dLastViewDate;
                        }

                        Int32 nIsSubRenewable = ODBCWrapper.Utils.GetIntSafeVal(dataRow["IS_RECURRING"]);
                        if (nIsSubRenewable == 1)
                            bIsSubRenewable = true;

                        Int32 nID = ODBCWrapper.Utils.GetIntSafeVal(dataRow["ID"]);
                        PaymentMethod payMet = GetBillingTransMethod(billingTransID);
                        string sDeviceUDID = ODBCWrapper.Utils.GetSafeStr(dataRow["device_name"]);


                        bool bCancellationWindow = false;
                        int nWaiver = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "WAIVER");
                        if (nWaiver == 0) // user didn't waiver yet
                        {
                            IsCancellationWindow(ref oUsageModule, sSubscriptionCode, dCreateDate, ref bCancellationWindow, eTransactionType.Subscription);
                        }


                        PermittedSubscriptionContainer p = new PermittedSubscriptionContainer();
                        p.Initialize(sSubscriptionCode, nMaxUses, nCurrentUses, dEnd, dCurrent, dLastViewDate, dCreateDate, dNextRenewalDate, bRecurringStatus, bIsSubRenewable, nID, payMet, sDeviceUDID, bCancellationWindow);
                        ret[i] = p;
                        ++i;
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetUserPermittedSubscriptions. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                if (lUsersIDs != null && lUsersIDs.Count > 0)
                {
                    sb.Append(" User IDs: ");
                    for (int i = 0; i < lUsersIDs.Count; i++)
                    {
                        sb.Append(String.Concat(" ", lUsersIDs[i], " "));
                    }
                }
                else
                {
                    sb.Append(String.Concat(" No users. "));
                }
                sb.Append(String.Concat(" IsExpired: ", isExpired));
                sb.Append(String.Concat(" NumOfItems: ", numOfItems));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            return ret;
        }

        /// <summary>
        /// Split Refference
        /// </summary>
        protected void SplitRefference(string sRefference, ref Int32 nMediaFileID, ref string sSubscriptionCode, ref string sPPVCode)
        {
            string[] spliter = { " " };
            string[] splited = sRefference.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splited.Length; i++)
            {
                string sHeader = splited[i];
                if (sHeader.StartsWith("mf:"))
                    nMediaFileID = int.Parse(sHeader.Substring(3));
                if (sHeader.StartsWith("sub:"))
                    sSubscriptionCode = sHeader.Substring(4);
                if (sHeader.StartsWith("ppvcode:"))
                    sPPVCode = sHeader.Substring(8);
            }
        }
        /// <summary>
        /// SMS Check Code For Media File
        /// </summary>
        public virtual TvinciBilling.BillingResponse SMS_CheckCodeForMediaFile(string sSiteGUID, string sCellPhone, string sSMSCode, Int32 nMediaFileID,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {

            TvinciBilling.BillingResponse ret = null;
            string sWSUserName = "";
            string sWSPass = "";
            TvinciBilling.module bm = null;
            TvinciPricing.mdoule m = null;
            ODBCWrapper.InsertQuery insertQuery = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {
                bm = new ConditionalAccess.TvinciBilling.module();

                Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("billing_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                {
                    bm.Url = sWSURL;
                }
                string sRefference = BuiltRefferenceString(nMediaFileID, "", "", "", 0, "");
                ret = bm.SMS_CheckCode(sWSUserName, sWSPass, sSiteGUID, sCellPhone, sSMSCode, sRefference);
                if (ret.m_sRecieptCode != "")
                {
                    if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
                    {
                        string sSubCode = "";
                        string sPPVCode = "";
                        SplitRefference(ret.m_sStatusDescription, ref nMediaFileID, ref sSubCode, ref sPPVCode);
                        sWSUserName = "";
                        sWSPass = "";

                        m = new global::ConditionalAccess.TvinciPricing.mdoule();
                        sWSURL = Utils.GetWSURL("pricing_ws");
                        if (!string.IsNullOrEmpty(sWSURL))
                        {
                            m.Url = sWSURL;
                        }
                        TvinciPricing.PPVModule thePPVModule = null;

                        Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                        thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                        insertQuery = new ODBCWrapper.InsertQuery("ppv_purchases");
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubCode);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", thePPVModule.m_oPriceCode.m_oPrise.m_dPrice);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", thePPVModule.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", "");
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLANGUAGE_CODE);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDEVICE_NAME);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", int.Parse(ret.m_sRecieptCode));
                        if (thePPVModule != null &&
                            thePPVModule.m_oUsageModule != null)
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", thePPVModule.m_oUsageModule.m_nMaxNumberOfViews);
                        else
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);

                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                        if (thePPVModule != null &&
                            thePPVModule.m_oUsageModule != null)
                        {
                            DateTime d = Utils.GetEndDateTime(DateTime.UtcNow, thePPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);

                        }

                        insertQuery.Execute();

                        WriteToUserLog(sSiteGUID, "Media file(SMS):" + nMediaFileID.ToString() + " purchased");
                        Int32 nPurchaseID = 0;
                        selectQuery = new ODBCWrapper.DataSetSelectQuery();
                        selectQuery += "select id from ppv_purchases where ";

                        selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                        selectQuery += "and";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubCode);
                        selectQuery += "and";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                        selectQuery += "and";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                        selectQuery += "and";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", thePPVModule.m_oPriceCode.m_oPrise.m_dPrice);
                        selectQuery += "and";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", thePPVModule.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3);
                        selectQuery += "and";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                        selectQuery += "and";
                        if (thePPVModule != null &&
                            thePPVModule.m_oUsageModule != null)
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", thePPVModule.m_oUsageModule.m_nMaxNumberOfViews);
                        else
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);
                        selectQuery += "and";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                        selectQuery += "and";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                        selectQuery += "order by id desc";
                        if (selectQuery.Execute("query", true) != null)
                        {
                            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                            if (nCount > 0)
                                nPurchaseID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                        }

                        //Should update the PURCHASE_ID

                        string sReciept = ret.m_sRecieptCode;
                        if (sReciept != "")
                        {
                            Int32 nID = int.Parse(sReciept);
                            updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                            updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                            updateQuery += "where";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                            updateQuery.Execute();

                        }

                    }
                    else
                    {
                        if (ret.m_sStatusDescription != "SMS was not sent yet")
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Success;
                            ret.m_sStatusDescription = "Allready purchased";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at SMS_CheckCodeForMediaFile. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" CP: ", sCellPhone));
                sb.Append(String.Concat(" SMS Code: ", sSMSCode));
                sb.Append(String.Concat(" MF ID: ", nMediaFileID));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            finally
            {
                #region Disposing
                if (bm != null)
                {
                    bm.Dispose();
                }
                if (m != null)
                {
                    m.Dispose();
                }
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
                #endregion
            }
            return ret;
        }
        /// <summary>
        /// SMS Check Code For Subscription
        /// </summary>
        public virtual TvinciBilling.BillingResponse SMS_CheckCodeForSubscription(string sSiteGUID, string sCellPhone, string sSMSCode, string sSubscription,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            TvinciBilling.BillingResponse ret = null;

            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;
            TvinciBilling.module bm = null;
            TvinciPricing.mdoule m = null;
            ODBCWrapper.UpdateQuery updateQuery = null;
            ODBCWrapper.InsertQuery insertQuery = null;
            try
            {
                m = new global::ConditionalAccess.TvinciPricing.mdoule();
                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                {
                    m.Url = sWSURL;
                }
                TvinciPricing.Subscription theSub = null;

                Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubscription, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);

                if (theSub != null)
                {
                    bm = new ConditionalAccess.TvinciBilling.module();
                    sWSUserName = string.Empty;
                    sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                    sWSURL = Utils.GetWSURL("billing_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        bm.Url = sWSURL;
                    }
                    string sRefference = BuiltRefferenceString(0, sSubscription, "", theSub.m_oPriceCode.m_sCode, theSub.m_oPriceCode.m_oPrise.m_dPrice, theSub.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3);
                    ret = bm.SMS_CheckCode(sWSUserName, sWSPass, sSiteGUID, sCellPhone, sSMSCode, sRefference);
                    if (ret.m_sRecieptCode != "")
                    {
                        if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
                        {
                            string sSubCode = "";
                            string sPPVCode = "";
                            Int32 nMediaFileID = 0;
                            SplitRefference(ret.m_sStatusDescription, ref nMediaFileID, ref sSubCode, ref sPPVCode);
                            updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                            updateQuery += " and ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscription);
                            updateQuery += " and ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                            updateQuery.Execute();


                            insertQuery = new ODBCWrapper.InsertQuery("subscriptions_purchases");
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscription);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", "");
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                            if (theSub != null &&
                                theSub.m_oUsageModule != null)
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", theSub.m_oUsageModule.m_nMaxNumberOfViews);
                            else
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);

                            if (theSub != null &&
                                theSub.m_oUsageModule != null)
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", theSub.m_oUsageModule.m_tsViewLifeCycle);
                            else
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", 0);

                            bool bIsRecurring = false;
                            if (theSub != null && theSub.m_oUsageModule != null)
                                bIsRecurring = theSub.m_bIsRecurring;

                            if (bIsRecurring == true)
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 1);
                            else
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                            if (ret.m_sRecieptCode != "")
                            {
                                try
                                {
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", int.Parse(ret.m_sRecieptCode));
                                }
                                catch { }
                            }
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLANGUAGE_CODE);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDEVICE_NAME);
                            if (theSub != null &&
                                theSub.m_oSubscriptionUsageModule != null)
                            {
                                DateTime d = Utils.GetEndDateTime(DateTime.UtcNow, theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);

                            }

                            insertQuery.Execute();

                            WriteToUserLog(sSiteGUID, "Subscription(SMS):" + sSubCode + " purchased");
                        }
                        else
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Success;
                            ret.m_sStatusDescription = "Allready purchased";
                            WriteToUserLog(sSiteGUID, "While trying to purchase subscription(SMS): " + sSubscription + " error returned: " + ret.m_sStatusDescription);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at SMS_CheckCodeForSubscription. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" CP: ", sCellPhone));
                sb.Append(String.Concat(" SMS: ", sSMSCode));
                sb.Append(String.Concat(" Sub: ", sSubscription));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Nm: ", sDEVICE_NAME));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            finally
            {
                #region Disposing
                if (m != null)
                {
                    m.Dispose();
                }
                if (bm != null)
                {
                    bm.Dispose();
                }
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
                #endregion
            }
            return ret;
        }


        /// <summary>
        /// Credit Card Charge User For Media File
        /// </summary>
        public virtual TvinciBilling.BillingResponse Cellular_ChargeUserForMediaFile(string sSiteGUID, double dPrice, string sCurrency, Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bDummy)
        {
            return Cellular_BaseChargeUserForMediaFile(sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, bDummy);
        }


        /// <summary>
        /// Credit Card Charge User For Media File
        /// </summary>
        public virtual TvinciBilling.BillingResponse CC_ChargeUserForMediaFile(string sSiteGUID, double dPrice, string sCurrency, Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode,
            string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bDummy, string sPaymentMethodID, string sEncryptedCVV)
        {
            return CC_BaseChargeUserForMediaFile(sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters,
                sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, bDummy, sPaymentMethodID, sEncryptedCVV);
        }
        protected TvinciBilling.BillingResponse CC_BaseChargeUserForMediaFile(string sSiteGUID, double dPrice, string sCurrency,
           Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bDummy, string sPaymentMethodID, string sEncryptedCVV)
        {
            TvinciBilling.BillingResponse oResponse = new ConditionalAccess.TvinciBilling.BillingResponse();
            oResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnown;
            oResponse.m_sRecieptCode = string.Empty;
            oResponse.m_sStatusDescription = string.Empty;

            TvinciUsers.UsersService wsUsersService = null;
            TvinciPricing.mdoule wsPricingService = null;
            TvinciBilling.module wsBillingService = null;

            try
            {
                log.Debug("CC_BaseChargeUserForMediaFile - " + string.Format("Entering CC_BaseChargeUserForMediaFile try block. Site Guid: {0} , Media File ID: {1} , Media ID: {2} , PPV Module Code: {3} , Coupon code: {4} , User IP: {5} , Payment Method: {6} , Dummy: {7}", sSiteGUID, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sPaymentMethodID, bDummy.ToString().ToLower()));
                if (!bDummy && string.IsNullOrEmpty(sPPVModuleCode))
                {
                    oResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                    oResponse.m_sRecieptCode = string.Empty;
                    oResponse.m_sStatusDescription = "Charge must have ppv module code";
                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + oResponse.m_sStatusDescription);
                    return oResponse;
                }

                long lSiteGuid = 0;
                if (sSiteGUID.Length == 0 || !Int64.TryParse(sSiteGUID, out lSiteGuid) || lSiteGuid == 0)
                {
                    oResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    oResponse.m_sRecieptCode = string.Empty;
                    oResponse.m_sStatusDescription = "Cant charge an unknown user";
                    return oResponse;
                }
                else
                {
                    wsUsersService = new ConditionalAccess.TvinciUsers.UsersService();

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        wsUsersService.Url = sWSURL;
                    }
                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = wsUsersService.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        oResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        oResponse.m_sRecieptCode = string.Empty;
                        oResponse.m_sStatusDescription = "Cant charge an unknown user";
                        return oResponse;
                    }
                    else if (uObj != null && uObj.m_user != null && uObj.m_user.m_eSuspendState == TvinciUsers.DomainSuspentionStatus.Suspended)
                    {
                        oResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UserSuspended;
                        oResponse.m_sRecieptCode = string.Empty;
                        oResponse.m_sStatusDescription = "Cannot charge a suspended user";
                        WriteToUserLog(sSiteGUID, "while trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + oResponse.m_sStatusDescription);
                        return oResponse;
                    }
                    else
                    {
                        bool bIsCouponValid = false;
                        bool bIsCouponUsedAndValid = false;
                        bIsCouponValid = Utils.IsCouponValid(m_nGroupID, sCouponCode);
                        if (!bIsCouponValid)
                        {
                            oResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            oResponse.m_sRecieptCode = string.Empty;
                            oResponse.m_sStatusDescription = "Coupon not valid";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + oResponse.m_sStatusDescription);
                            return oResponse;
                        }

                        bIsCouponUsedAndValid = bIsCouponValid && !string.IsNullOrEmpty(sCouponCode);


                        sWSUserName = string.Empty;
                        sWSPass = string.Empty;

                        wsPricingService = new global::ConditionalAccess.TvinciPricing.mdoule();
                        sWSURL = Utils.GetWSURL("pricing_ws");
                        if (!string.IsNullOrEmpty(sWSURL))
                        {
                            wsPricingService.Url = sWSURL;
                        }
                        Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                        if (!bDummy && string.IsNullOrEmpty(sPPVModuleCode))
                        {
                            oResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            oResponse.m_sRecieptCode = string.Empty;
                            oResponse.m_sStatusDescription = "Charge must have ppv module code";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + oResponse.m_sStatusDescription);
                            return oResponse;
                        }

                        // chack if ppvModule related to mediaFile 
                        long ppvModuleCode = 0;
                        long.TryParse(sPPVModuleCode, out ppvModuleCode);

                        TvinciPricing.PPVModule thePPVModule = wsPricingService.ValidatePPVModuleForMediaFile(sWSUserName, sWSPass, nMediaFileID, ppvModuleCode);

                        if (thePPVModule == null)
                        {
                            oResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            oResponse.m_sRecieptCode = string.Empty;
                            oResponse.m_sStatusDescription = "The ppv module is unknown";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + oResponse.m_sStatusDescription);
                            return oResponse;
                        }

                        if (!bDummy && !thePPVModule.m_sObjectCode.Equals(sPPVModuleCode))
                        {
                            oResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownPPVModule;
                            oResponse.m_sRecieptCode = string.Empty;
                            oResponse.m_sStatusDescription = "This PPVModule does not belong to item";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + oResponse.m_sStatusDescription);
                            return oResponse;
                        }

                        if (bDummy)
                        {
                            sPPVModuleCode = thePPVModule.m_sObjectCode;
                            dPrice = thePPVModule.m_oPriceCode.m_oPrise.m_dPrice;
                            sCurrency = thePPVModule.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3;
                            if (!IsTakePriceFromMediaFileFinalPrice(bDummy))
                            { // Cinepolis patch
                                dPrice = 0d;
                            }
                        }

                        PriceReason ePriceReason = PriceReason.UnKnown;

                        TvinciPricing.Subscription relevantSub = null;
                        TvinciPricing.Collection relevantCol = null;
                        TvinciPricing.PrePaidModule relevantPP = null;

                        TvinciPricing.Price oPrice = Utils.GetMediaFileFinalPriceForNonGetItemsPrices(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref ePriceReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        bDummy = RecalculateDummyIndicatorForChargeMediaFile(bDummy, ePriceReason, bIsCouponUsedAndValid);
                        if ((ePriceReason == PriceReason.ForPurchase || (ePriceReason == PriceReason.SubscriptionPurchased && oPrice.m_dPrice > 0) || bDummy) && ePriceReason != PriceReason.NotForPurchase)
                        {
                            if (bDummy || (oPrice.m_dPrice == dPrice && oPrice.m_oCurrency.m_sCurrencyCD3 == sCurrency))
                            {
                                string sCustomData = string.Empty;
                                sWSUserName = string.Empty;
                                sWSPass = string.Empty;

                                InitializeBillingModule(ref wsBillingService, ref sWSUserName, ref sWSPass);
                                if (string.IsNullOrEmpty(sCountryCd) && !string.IsNullOrEmpty(sUserIP))
                                {
                                    sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
                                }
                                //Create the Custom Data
                                sCustomData = GetCustomData(relevantSub, thePPVModule, null, sSiteGUID, dPrice, sCurrency,
                                    nMediaFileID, nMediaID, sPPVModuleCode, string.Empty, sCouponCode, sUserIP,
                                    sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                                log.Debug("CustomData - " + sCustomData);


                                oResponse = HandleCCChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData,
                                    1, 1, sExtraParameters, sPaymentMethodID, sEncryptedCVV, bDummy, false, ref wsBillingService);
                                if (oResponse.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
                                {
                                    long lBillingTransactionID = 0;
                                    long lPurchaseID = 0;
                                    HandleChargeUserForMediaFileBillingSuccess(sWSUserName, sWSPass, sSiteGUID, uObj.m_user.m_domianID, relevantSub, dPrice, sCurrency,
                                        sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, oResponse, sCustomData,
                                        thePPVModule, nMediaFileID, ref lBillingTransactionID, ref lPurchaseID, bDummy, ref wsBillingService);

                                    // Enqueue notification for PS so they know a media file was charged
                                    var dicData = new Dictionary<string, object>()
                                            {
                                                {"MediaFileID", nMediaFileID},
                                                {"BillingTransactionID", lBillingTransactionID},
                                                {"PPVModuleCode", sPPVModuleCode},
                                                {"SiteGUID", sSiteGUID},
                                                {"CouponCode", sCouponCode},
                                                {"CustomData", sCustomData},
                                                {"PurchaseID", lPurchaseID}
                                            };

                                    this.EnqueueEventRecord(NotifiedAction.ChargedMediaFile, dicData);
                                }
                                else
                                {
                                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + oResponse.m_sStatusDescription);
                                }
                            }
                            else
                            {
                                oResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                                oResponse.m_sRecieptCode = string.Empty;
                                oResponse.m_sStatusDescription = "The price of the request is not the actual price";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + oResponse.m_sStatusDescription);
                            }
                        }
                        else
                        {
                            if (ePriceReason == PriceReason.PPVPurchased)
                            {
                                oResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                oResponse.m_sRecieptCode = string.Empty;
                                oResponse.m_sStatusDescription = "The media file is already purchased";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + oResponse.m_sStatusDescription);
                            }
                            else if (ePriceReason == PriceReason.Free)
                            {
                                oResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                oResponse.m_sRecieptCode = string.Empty;
                                oResponse.m_sStatusDescription = "The media file is free";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + oResponse.m_sStatusDescription);
                            }
                            else if (ePriceReason == PriceReason.ForPurchaseSubscriptionOnly)
                            {
                                oResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                oResponse.m_sRecieptCode = string.Empty;
                                oResponse.m_sStatusDescription = "The media file is for purchase with subscription only";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + oResponse.m_sStatusDescription);
                            }
                            else if (ePriceReason == PriceReason.SubscriptionPurchased)
                            {
                                oResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                oResponse.m_sRecieptCode = string.Empty;
                                oResponse.m_sStatusDescription = "The media file is already purchased (subscription)";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + oResponse.m_sStatusDescription);
                            }
                            else if (ePriceReason == PriceReason.NotForPurchase)
                            {
                                oResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                oResponse.m_sRecieptCode = string.Empty;
                                oResponse.m_sStatusDescription = "The media file is not valid for purchased";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + oResponse.m_sStatusDescription);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at CC_BaseChargeUserForMediaFile. ");
                sb.Append(String.Concat("Exception msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Media File ID: ", nMediaFileID));
                sb.Append(String.Concat(" Media ID: ", nMediaID));
                sb.Append(String.Concat(" Coupon Code: ", sCouponCode));
                sb.Append(String.Concat(" User IP: ", sUserIP));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Stack trace: ", ex.StackTrace));
                log.Debug("CC_BaseChargeUserForMediaFile - " + sb.ToString(), ex);
                WriteToUserLog(sSiteGUID, string.Format("Exception at CC_BaseChargeUserForMediaFile. Media File ID: {0} , Media ID: {1} , Coupon Code: {2}", nMediaFileID, nMediaID, sCouponCode));
                #endregion
            }
            finally
            {
                #region Disposing
                if (wsUsersService != null)
                {
                    wsUsersService.Dispose();
                }
                if (wsPricingService != null)
                {
                    wsPricingService.Dispose();
                }
                if (wsBillingService != null)
                {
                    wsBillingService.Dispose();
                }
                #endregion
            }
            return oResponse;
        }


        /// <summary>
        /// InApp Charge User For Media File
        /// </summary>
        public virtual TvinciBilling.BillingResponse CC_ChargeUserForMediaFile(string sSiteGUID, double dPrice, string sCurrency, Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sRecieptCode)
        {
            return InApp_BaseChargeUserForMediaFile(sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, sRecieptCode);
        }
        /// <summary>
        /// Get PPV CustomData ID
        /// </summary>


        public virtual int GetPPVCustomDataID(string sSiteGUID, double dPrice, string sCurrency,
            Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCouponCode, string sCampaignCode, string sPaymentMethod, string sUserIP,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return GetPPVCustomDataID(sSiteGUID, dPrice, sCurrency,
            nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sCampaignCode, sPaymentMethod, sUserIP,
            sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty);
        }

        public virtual int GetPPVCustomDataID(string sSiteGUID, double dPrice, string sCurrency,
            Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCouponCode, string sCampaignCode, string sPaymentMethod, string sUserIP,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sOverrideEndDate)
        {
            int retVal = 0;
            TvinciUsers.UsersService u = null;
            TvinciPricing.mdoule m = null;

            try
            {
                log.Debug("GetPPVCustomDataID - " + GetGetCustomDataLogMsg("PPV", sSiteGUID, dPrice, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sPaymentMethod, sUserIP, string.Empty));
                u = new ConditionalAccess.TvinciUsers.UsersService();

                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                {
                    u.Url = sWSURL;
                }
                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    retVal = 0;
                }
                else
                {
                    sWSUserName = string.Empty;
                    sWSPass = string.Empty;

                    m = new global::ConditionalAccess.TvinciPricing.mdoule();
                    sWSURL = Utils.GetWSURL("pricing_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        m.Url = sWSURL;
                    }
                    Int32[] nMediaFiles = { nMediaFileID };
                    TvinciPricing.MediaFilePPVModule[] oModules = null;

                    Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                    oModules = m.GetPPVModuleListForMediaFiles(sWSUserName, sWSPass, nMediaFiles, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                    Int32 nCount = 0;
                    if (oModules[0].m_oPPVModules != null)
                        nCount = oModules[0].m_oPPVModules.Length;
                    bool bOK = false;
                    for (int i = 0; i < nCount; i++)
                    {
                        if (oModules[0].m_oPPVModules[i].m_sObjectCode == sPPVModuleCode)
                            bOK = true;
                    }
                    if (bOK == false)
                    {
                        retVal = 0;
                    }
                    else
                    {
                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.Subscription relevantSub = null;
                        TvinciPricing.Collection relevantCol = null;
                        TvinciPricing.PrePaidModule relevantPP = null;
                        TvinciPricing.Campaign relevantCamp = null;
                        Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                        TvinciPricing.PPVModule thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (!string.IsNullOrEmpty(sCampaignCode))
                        {
                            int nCampaignCode = int.Parse(sCampaignCode);
                            relevantCamp = m.GetCampaignData(sWSUserName, sWSPass, nCampaignCode);
                        }
                        if (thePPVModule != null)
                        {
                            TvinciPricing.Price p = Utils.GetMediaFileFinalPriceForNonGetItemsPrices(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                            if (theReason == PriceReason.ForPurchase || (theReason == PriceReason.SubscriptionPurchased && p.m_dPrice > 0))
                            {
                                if (p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency)
                                {
                                    if (p.m_dPrice != 0)
                                    {
                                        string sCustomData = GetCustomData(relevantSub, thePPVModule, relevantCamp, sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode, sCampaignCode, sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, sOverrideEndDate);

                                        retVal = Utils.AddCustomData(sCustomData);

                                        //customdata id

                                    }
                                }
                            }
                        }
                    }
                } // end big else
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder(String.Concat("Exception. Msg: ", ex.Message));
                sb.Append(String.Concat(" ,Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" ,Price: ", dPrice));
                sb.Append(String.Concat(" ,Currency: ", sCurrency));
                sb.Append(String.Concat(" ,Media file ID: ", nMediaFileID));
                sb.Append(String.Concat(" ,Media ID: ", nMediaID));
                sb.Append(String.Concat(" ,PPV Module code: ", sPPVModuleCode));
                sb.Append(String.Concat(" ,Coupon code: ", sCouponCode));
                sb.Append(String.Concat(" ,Campaign code: ", sCampaignCode));
                sb.Append(String.Concat(" ,Payment method: ", sPaymentMethod));
                sb.Append(String.Concat(" ,Country code: ", sCountryCd));
                sb.Append(String.Concat(" ,User IP: ", sUserIP));
                sb.Append(String.Concat(" ,Language code: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" ,Device name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" ,Override end date: ", sOverrideEndDate));
                sb.Append(String.Concat(" ,BaseConditionalAccess is: ", this.GetType().Name));
                sb.Append(String.Concat(" ,Stack trace: ", ex.StackTrace));
                log.Debug("GetPPVCustomDataID - " + sb.ToString());
                #endregion
            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                    u = null;
                }
                if (m != null)
                {
                    m.Dispose();
                    m = null;
                }
                #endregion
            }

            return retVal;
        }

        protected string GetGetCustomDataLogMsg(string sBusinessModuleName, string sSiteGUID, double dPrice, Int32 nMediaFileID,
            Int32 nMediaID, string sPPVModuleCode, string sCouponCode, string sPaymentMethod, string sUserIP, string sPreviewModuleID)
        {
            StringBuilder sb = new StringBuilder(string.Format("Entering GetCustomData{0} try block: ", sBusinessModuleName));
            sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
            sb.Append(String.Concat(" Price: ", dPrice));
            sb.Append(String.Concat(" Media File ID: ", nMediaFileID));
            sb.Append(String.Concat(" Media ID: ", nMediaID));
            sb.Append(String.Concat(" PPV Module Code: ", sPPVModuleCode));
            sb.Append(String.Concat(" Coupon Code: ", sCouponCode));
            sb.Append(String.Concat(" Payment Method: ", sPaymentMethod));
            sb.Append(String.Concat(" User IP: ", sUserIP));
            sb.Append(String.Concat(" Preview Module ID: ", sPreviewModuleID));
            sb.Append(String.Concat(" BaseConditionalAccess is:", this.GetType().Name));
            return sb.ToString();
        }
        /// <summary>
        /// Get PrePaid Custom DataID
        /// </summary>
        public virtual int GetPrePaidCustomDataID(string sSiteGUID, double dPrice,
            string sCurrency, string sPrePaidCode, string sCouponCode, string sPaymentMethod, string sUserIP,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return GetPrePaidCustomDataID(sSiteGUID, dPrice,
            sCurrency, sPrePaidCode, sCouponCode, sPaymentMethod, sUserIP,
            sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty);
        }

        public virtual int GetPrePaidCustomDataID(string sSiteGUID, double dPrice,
            string sCurrency, string sPrePaidCode, string sCouponCode, string sPaymentMethod, string sUserIP,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sOverrideEnddate)
        {
            int retVal = 0;
            TvinciUsers.UsersService u = null;
            try
            {
                if (string.IsNullOrEmpty(sSiteGUID))
                {
                    retVal = 0;
                }
                else
                {
                    u = new ConditionalAccess.TvinciUsers.UsersService();

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }
                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        retVal = 0;
                    }
                    else
                    {
                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.PrePaidModule thePrePaid = null;
                        TvinciPricing.Price p = Utils.GetPrePaidFinalPrice(m_nGroupID, sPrePaidCode, sSiteGUID, ref theReason, ref thePrePaid, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, "pricing_connection", sCouponCode);
                        if (theReason == PriceReason.ForPurchase)
                        {
                            if (p != null && p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency)
                            {
                                if (p.m_dPrice != 0)
                                {
                                    string sCustomData = GetCustomDataForPrePaid(thePrePaid, null, sPrePaidCode, string.Empty, sSiteGUID, dPrice, sCurrency, sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, sOverrideEnddate);


                                    retVal = Utils.AddCustomData(sCustomData);
                                }
                            }
                            else
                            {
                                retVal = 0;
                            }
                        }
                        else
                        {
                            if (theReason == PriceReason.Free)
                            {
                                retVal = 0;
                            }
                            if (theReason == PriceReason.SubscriptionPurchased)
                            {
                                retVal = 0;
                            }
                        }
                    }
                } // end else
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetPrePaidCustomDataID. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" Currency: ", sCurrency));
                sb.Append(String.Concat(" PP Cd: ", sPrePaidCode));
                sb.Append(String.Concat(" Cpn Cd: ", sCouponCode));
                sb.Append(String.Concat(" PM: ", sPaymentMethod));
                sb.Append(String.Concat(" User IP: ", sUserIP));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Nm: ", sDEVICE_NAME));
                sb.Append(String.Concat(" Override ED: ", sOverrideEnddate));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            finally
            {
                if (u != null)
                {
                    u.Dispose();
                }
            }
            return retVal;
        }
        /// <summary>
        /// Get Subscription Custom Data ID
        /// </summary>
        public virtual int GetSubscriptionCustomDataID(string sSiteGUID, double dPrice,
    string sCurrency, string sSubscriptionCode, string sCampaignCode, string sCouponCode, string sPaymentMethod, string sUserIP,
    string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return GetBundleCustomDataID(sSiteGUID, dPrice,
            sCurrency, sSubscriptionCode, sCampaignCode, sCouponCode, sPaymentMethod, sUserIP,
            sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty, string.Empty, eBundleType.SUBSCRIPTION);
        }

        /// <summary>
        /// Get Subscription Custom Data ID
        /// </summary>
        public virtual int GetCollectionCustomDataID(string sSiteGUID, double dPrice,
    string sCurrency, string sSubscriptionCode, string sCampaignCode, string sCouponCode, string sPaymentMethod, string sUserIP,
    string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return GetBundleCustomDataID(sSiteGUID, dPrice,
            sCurrency, sSubscriptionCode, sCampaignCode, sCouponCode, sPaymentMethod, sUserIP,
            sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty, string.Empty, eBundleType.COLLECTION);
        }

        public virtual int GetBundleCustomDataID(string sSiteGUID, double dPrice,
            string sCurrency, string sBundleCode, string sCampaignCode, string sCouponCode, string sPaymentMethod, string sUserIP,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sOverrideEnddate, string sPreviewModuleID, eBundleType bundleType)
        {
            int retVal = 0;
            long lSiteGuid = 0;
            if (sSiteGUID.Length == 0 || !Int64.TryParse(sSiteGUID, out lSiteGuid) || lSiteGuid == 0)
            {
                retVal = 0;
            }
            else
            {
                TvinciUsers.UsersService u = null;
                TvinciPricing.mdoule m = null;
                try
                {
                    log.Debug("GetBundleCustomDataID - " + GetGetCustomDataLogMsg("Bundle", sSiteGUID, dPrice, 0, 0, sBundleCode, sCouponCode, sPaymentMethod, sUserIP, sPreviewModuleID));
                    u = new ConditionalAccess.TvinciUsers.UsersService();

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }
                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        retVal = 0;
                    }
                    else
                    {

                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.PPVModule theBundle = null;
                        TvinciPricing.Campaign relevantCamp = null;
                        TvinciPricing.Price price = null;

                        switch (bundleType)
                        {
                            case eBundleType.SUBSCRIPTION:
                                {
                                    TvinciPricing.Subscription theSub = null;
                                    price = Utils.GetSubscriptionFinalPrice(m_nGroupID, sBundleCode, sSiteGUID, sCouponCode, ref theReason, ref theSub, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                                    theBundle = theSub;
                                    break;
                                }
                            case eBundleType.COLLECTION:
                                {
                                    TvinciPricing.Collection theCol = null;
                                    price = Utils.GetCollectionFinalPrice(m_nGroupID, sBundleCode, sSiteGUID, sCouponCode, ref theReason, ref theCol, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, "");
                                    theBundle = theCol;
                                    break;
                                }
                        }


                        if (theReason == PriceReason.ForPurchase || theReason == PriceReason.EntitledToPreviewModule)
                        {
                            if (price != null && price.m_dPrice == dPrice && price.m_oCurrency.m_sCurrencyCD3 == sCurrency)
                            {
                                if (price.m_dPrice != 0 || (theReason == PriceReason.EntitledToPreviewModule && IsPreviewModuleInGroupIDCostsZero()))
                                {

                                    sWSUserName = string.Empty;
                                    sWSPass = string.Empty;

                                    m = new global::ConditionalAccess.TvinciPricing.mdoule();
                                    sWSURL = Utils.GetWSURL("pricing_ws");
                                    if (!string.IsNullOrEmpty(sWSURL))
                                    {
                                        m.Url = sWSURL;
                                    }
                                    Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);

                                    if (!string.IsNullOrEmpty(sCampaignCode))
                                    {
                                        int nCampaignCode = int.Parse(sCampaignCode);
                                        relevantCamp = m.GetCampaignData(sWSUserName, sWSPass, nCampaignCode);
                                    }

                                    string sCustomData = string.Empty;

                                    switch (bundleType)
                                    {
                                        case eBundleType.SUBSCRIPTION:
                                            {
                                                sCustomData = GetCustomDataForSubscription(theBundle as TvinciPricing.Subscription, relevantCamp, sBundleCode, sCampaignCode, sSiteGUID, dPrice, sCurrency, sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, sOverrideEnddate, sPreviewModuleID, theReason == PriceReason.EntitledToPreviewModule);
                                                break;
                                            }
                                        case eBundleType.COLLECTION:
                                            {
                                                sCustomData = GetCustomDataForCollection(theBundle as TvinciPricing.Collection, sBundleCode, sSiteGUID, dPrice, sCurrency, sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, sOverrideEnddate);
                                                break;
                                            }
                                    }
                                    retVal = Utils.AddCustomData(sCustomData);
                                }
                            }
                            else
                            {
                                retVal = 0;
                            }
                        }
                        else
                        {
                            if (theReason == PriceReason.Free)
                            {
                                retVal = 0;
                            }
                            if (theReason == PriceReason.SubscriptionPurchased)
                            {
                                retVal = 0;
                            }
                            if (theReason == PriceReason.CollectionPurchased)
                            {
                                retVal = 0;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    #region Logging
                    StringBuilder sb = new StringBuilder(String.Concat("Exception msg: ", ex.Message));
                    sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                    sb.Append(String.Concat(" Price: ", dPrice));
                    sb.Append(String.Concat(" Currency: ", sCurrency));
                    sb.Append(String.Concat(" Bundle Code: ", sBundleCode));
                    sb.Append(String.Concat(" Campaign Code: ", sCampaignCode));
                    sb.Append(String.Concat(" Coupon Code: ", sCouponCode));
                    sb.Append(String.Concat(" Payment Method: ", sPaymentMethod));
                    sb.Append(String.Concat(" User IP: ", sUserIP));
                    sb.Append(String.Concat(" Country Code: ", sCountryCd));
                    sb.Append(String.Concat(" Language Code: ", sLANGUAGE_CODE));
                    sb.Append(String.Concat(" Device Name: ", sDEVICE_NAME));
                    sb.Append(String.Concat(" Override End Date: ", sOverrideEnddate));
                    sb.Append(String.Concat(" Preview Module ID: ", sPreviewModuleID));
                    sb.Append(String.Concat(" BaseConditionalAccess is: ", this.GetType().ToString()));
                    sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));
                    log.Debug("GetBundleCustomDataID - " + sb.ToString());
                    #endregion
                    retVal = 0;

                }
                finally
                {
                    #region Disposing
                    if (u != null)
                    {
                        u.Dispose();
                        u = null;
                    }
                    if (m != null)
                    {
                        m.Dispose();
                        m = null;
                    }
                    #endregion
                }
            }
            return retVal;
        }

        /// <summary>
        /// PU Get PPV Popup Payment Method URL
        /// </summary>
        public virtual TvinciBilling.BillingResponse PU_GetPPVPopupPaymentMethodURL(string sSiteGUID, double dPrice, string sCurrency,
            Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCouponCode, string sPaymentMethod, string sExtraParameters,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnown;
            ret.m_sRecieptCode = string.Empty;
            ret.m_sStatusDescription = string.Empty;
            TvinciUsers.UsersService u = null;
            TvinciPricing.mdoule m = null;
            TvinciBilling.module bm = null;
            try
            {
                if (string.IsNullOrEmpty(sSiteGUID))
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                }
                else
                {
                    u = new ConditionalAccess.TvinciUsers.UsersService();

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }

                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                    }
                    else
                    {

                        sWSUserName = string.Empty;
                        sWSPass = string.Empty;

                        m = new global::ConditionalAccess.TvinciPricing.mdoule();
                        sWSURL = Utils.GetWSURL("pricing_ws");
                        if (!string.IsNullOrEmpty(sWSURL))
                        {
                            m.Url = sWSURL;
                        }

                        Int32[] nMediaFiles = { nMediaFileID };
                        TvinciPricing.MediaFilePPVModule[] oModules = null;

                        Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                        oModules = m.GetPPVModuleListForMediaFiles(sWSUserName, sWSPass, nMediaFiles, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        Int32 nCount = 0;
                        if (oModules[0].m_oPPVModules != null)
                            nCount = oModules[0].m_oPPVModules.Length;
                        bool bOK = false;
                        for (int i = 0; i < nCount; i++)
                        {
                            if (oModules[0].m_oPPVModules[i].m_sObjectCode == sPPVModuleCode)
                                bOK = true;
                        }
                        if (!bOK)
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownPPVModule;
                            ret.m_sRecieptCode = string.Empty;
                            ret.m_sStatusDescription = "This PPVModule does not belong to item";
                        }
                        else
                        {
                            PriceReason theReason = PriceReason.UnKnown;
                            TvinciPricing.Subscription relevantSub = null;
                            TvinciPricing.Collection relevantCol = null;
                            TvinciPricing.PrePaidModule relevantPP = null;

                            Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                            TvinciPricing.PPVModule thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                            if (thePPVModule != null)
                            {
                                TvinciPricing.Price p = Utils.GetMediaFileFinalPriceForNonGetItemsPrices(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                                if (theReason == PriceReason.ForPurchase || (theReason == PriceReason.SubscriptionPurchased && p.m_dPrice > 0))
                                {
                                    if (p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency)
                                    {
                                        if (p.m_dPrice != 0)
                                        {
                                            bm = new ConditionalAccess.TvinciBilling.module();
                                            sWSUserName = string.Empty;
                                            sWSPass = string.Empty;
                                            Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                                            sWSURL = Utils.GetWSURL("billing_ws");
                                            if (!string.IsNullOrEmpty(sWSURL))
                                            {
                                                bm.Url = sWSURL;
                                            }
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
                                            sCustomData += sCouponCode;
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
                                            string sExtraParams = "email=";
                                            if (uObj.m_user.m_oBasicData.m_sEmail != "")
                                                sExtraParams += uObj.m_user.m_oBasicData.m_sEmail;
                                            else
                                                sExtraParams += "empty@empty.com";
                                            sExtraParams += "&address1=";
                                            if (uObj.m_user.m_oBasicData.m_sAddress != "")
                                                sExtraParams += uObj.m_user.m_oBasicData.m_sAddress;
                                            else
                                                sExtraParams += "Empty address";
                                            sExtraParams += "&city=";
                                            if (uObj.m_user.m_oBasicData.m_sCity != "")
                                                sExtraParams += uObj.m_user.m_oBasicData.m_sCity;
                                            else
                                                sExtraParams += "Empty city";
                                            sExtraParams += "&country=";
                                            if (uObj.m_user.m_oBasicData.m_Country != null && uObj.m_user.m_oBasicData.m_Country.m_sCountryName != "")
                                                //sExtraParams += uObj.m_user.m_oBasicData.m_Country.m_sCountryName;
                                                sExtraParams += uObj.m_user.m_oBasicData.m_Country.m_sCountryCode;
                                            else
                                                sExtraParams += "Empty country";
                                            sExtraParams += "&phone1=";
                                            if (uObj.m_user.m_oBasicData.m_sPhone != "")
                                                sExtraParams += uObj.m_user.m_oBasicData.m_sPhone;
                                            else
                                                sExtraParams += "0000000";
                                            if (!sExtraParameters.StartsWith("&"))
                                                sExtraParameters = "&" + sExtraParameters;
                                            sExtraParams += sExtraParameters;

                                            //customdata id
                                            ret.m_sRecieptCode = bm.CC_GetPopupURL(sWSUserName, sWSPass, dPrice, sCurrency, "PPV Item", sCustomData, sPaymentMethod, sExtraParams);
                                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Success;
                                            ret.m_sStatusDescription = "PopUp URL";
                                        }
                                    }
                                    else
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                                        ret.m_sRecieptCode = string.Empty;
                                        ret.m_sStatusDescription = "The price of the request is not the actual price";
                                    }
                                }
                                else
                                {
                                    if (theReason == PriceReason.PPVPurchased)
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                        ret.m_sRecieptCode = "";
                                        ret.m_sStatusDescription = "The media file is already purchased";
                                    }
                                    else if (theReason == PriceReason.Free)
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                        ret.m_sRecieptCode = "";
                                        ret.m_sStatusDescription = "The media file is free";
                                    }
                                    else if (theReason == PriceReason.ForPurchaseSubscriptionOnly)
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                        ret.m_sRecieptCode = "";
                                        ret.m_sStatusDescription = "The media file is for purchase with subscription only";
                                    }
                                    else if (theReason == PriceReason.SubscriptionPurchased)
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                        ret.m_sRecieptCode = "";
                                        ret.m_sStatusDescription = "The media file is already purchased (subscription)";
                                    }
                                }
                            }
                            else
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = "";
                                ret.m_sStatusDescription = "The ppv module is unknown";
                            }
                        }
                    } // end inner else
                } // end else
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at PU_GetPPVPopupPaymentMethodURL. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" MF ID: ", nMediaFileID));
                sb.Append(String.Concat(" M ID: ", nMediaID));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + ex.StackTrace, ex);
                #endregion
            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                }
                if (bm != null)
                {
                    bm.Dispose();
                }
                if (m != null)
                {
                    m.Dispose();
                }
                #endregion
            }
            return ret;
        }
        /// <summary>
        /// PU Get Subscription Popup Payment Method URL
        /// </summary>
        public virtual TvinciBilling.BillingResponse PU_GetSubscriptionPopupPaymentMethodURL(string sSiteGUID, double dPrice,
            string sCurrency, string sSubscriptionCode, string sCouponCode, string sPaymentMethod, string sExtraParameters,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            if (sSiteGUID == "")
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Cant charge an unknown user";
            }
            else
            {
                using (TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService())
                {

                    string sWSUserName = "";
                    string sWSPass = "";
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }
                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = "";
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                    }
                    else
                    {
                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.Subscription theSub = null;
                        TvinciPricing.Price p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubscriptionCode, sSiteGUID, sCouponCode, ref theReason, ref theSub, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (theReason == PriceReason.ForPurchase)
                        {
                            if (p != null && p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency)
                            {
                                if (p.m_dPrice != 0)
                                {
                                    using (TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module())
                                    {
                                        sWSUserName = "";
                                        sWSPass = "";
                                        Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                                        sWSURL = Utils.GetWSURL("billing_ws");
                                        if (!string.IsNullOrEmpty(sWSURL))
                                        {
                                            bm.Url = sWSURL;
                                        }

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
                                        sCustomData += "<s>" + sSubscriptionCode + "</s>";
                                        sCustomData += "<cc>" + sCouponCode + "</cc>";
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
                                        sCustomData += "";
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
                                        string sExtraParams = "email=";
                                        if (uObj.m_user.m_oBasicData.m_sEmail != "")
                                            sExtraParams += uObj.m_user.m_oBasicData.m_sEmail;
                                        else
                                            sExtraParams += "empty@empty.com";
                                        sExtraParams += "&address1=";
                                        if (uObj.m_user.m_oBasicData.m_sAddress != "")
                                            sExtraParams += uObj.m_user.m_oBasicData.m_sAddress;
                                        else
                                            sExtraParams += "Empty address";
                                        sExtraParams += "&city=";
                                        if (uObj.m_user.m_oBasicData.m_sCity != "")
                                            sExtraParams += uObj.m_user.m_oBasicData.m_sCity;
                                        else
                                            sExtraParams += "Empty city";
                                        sExtraParams += "&country=";
                                        if (uObj.m_user.m_oBasicData.m_Country != null && uObj.m_user.m_oBasicData.m_Country.m_sCountryName != "")
                                            //sExtraParams += uObj.m_user.m_oBasicData.m_Country.m_sCountryName;
                                            sExtraParams += uObj.m_user.m_oBasicData.m_Country.m_sCountryCode;
                                        else
                                            sExtraParams += "Empty country";
                                        sExtraParams += "&phone1=";
                                        if (uObj.m_user.m_oBasicData.m_sPhone != "")
                                            sExtraParams += uObj.m_user.m_oBasicData.m_sPhone;
                                        else
                                            sExtraParams += "0000000";
                                        if (sExtraParameters.StartsWith("&") == false)
                                            sExtraParameters = "&" + sExtraParameters;
                                        sExtraParams += sExtraParameters;
                                        ret.m_sRecieptCode = bm.CC_GetPopupURL(sWSUserName, sWSPass, dPrice, sCurrency, "Subscription", sCustomData, sPaymentMethod, sExtraParams);
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Success;
                                        ret.m_sStatusDescription = "PopUp URL";
                                    }
                                } // end if price is not zero
                            }
                            else
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                                ret.m_sRecieptCode = "";
                                ret.m_sStatusDescription = "The price of the request is not the actual price";
                            }
                        }
                        else
                        {
                            if (theReason == PriceReason.Free)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = "";
                                ret.m_sStatusDescription = "The subscription is free";
                            }
                            if (theReason == PriceReason.SubscriptionPurchased)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = "";
                                ret.m_sStatusDescription = "The subscription is already purchased";
                            }
                        }
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// Credit Card Charge User For Subscription
        /// </summary>
        public virtual TvinciBilling.BillingResponse Cellular_ChargeUserForSubscription(string sSiteGUID, double dPrice, string sCurrency, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParams,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bDummy)
        {
            return Cellular_BaseChargeUserForSubscription(sSiteGUID, dPrice, sCurrency, sSubscriptionCode, sCouponCode, sUserIP, sExtraParams, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, bDummy);
        }

        /// <summary>
        /// Credit Card Charge User For Bundle
        /// </summary>
        public virtual TvinciBilling.BillingResponse CC_ChargeUserForBundle(string sSiteGUID, double dPrice, string sCurrency, string sBundleCode, string sCouponCode, string sUserIP, string sExtraParams,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bDummy, string sPaymentMethodID, string sEncryptedCVV, eBundleType bundleType)
        {
            return CC_BaseChargeUserForBundle(sSiteGUID, dPrice, sCurrency, sBundleCode, sCouponCode, sUserIP, sExtraParams, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, bDummy, sPaymentMethodID, sEncryptedCVV, bundleType);
        }

        protected virtual double InitializePriceForBundlePurchase(double inputPrice, bool isDummy)
        {
            return inputPrice;
        }

        protected TvinciBilling.BillingResponse CC_BaseChargeUserForBundle
           (string sSiteGUID, double dPrice, string sCurrency, string sBundleCode, string sCouponCode, string sUserIP, string sExtraParams,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bDummy, string sPaymentMethodID, string sEncryptedCVV, eBundleType bundleType)
        {
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnown;
            ret.m_sRecieptCode = string.Empty;
            ret.m_sStatusDescription = string.Empty;

            TvinciBilling.module bm = null;
            TvinciUsers.UsersService u = null;

            try
            {
                log.Debug("CC_BaseChargeUserForBundle - " + string.Format("Entering CC_BaseChargeUserForBundle try block. Site Guid: {0} , Bundle Code: {1} , Coupon Code: {2} , User IP: {3} , Payment Method: {4} , Dummy: {5}", sSiteGUID, sBundleCode, sCouponCode, sUserIP, sPaymentMethodID, bDummy.ToString().ToLower()));
                long lSiteGuid = 0;
                if (sSiteGUID.Length == 0 || !Int64.TryParse(sSiteGUID, out lSiteGuid) || lSiteGuid == 0)
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                }
                else
                {
                    u = new ConditionalAccess.TvinciUsers.UsersService();

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);

                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }
                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                    }
                    else if (uObj != null && uObj.m_user != null && uObj.m_user.m_eSuspendState == TvinciUsers.DomainSuspentionStatus.Suspended)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UserSuspended;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cannot charge a suspended user";
                        WriteToUserLog(sSiteGUID, "while trying to purchase Bundle(CC): " + sBundleCode + " error returned: " + ret.m_sStatusDescription);
                    }
                    else
                    {
                        dPrice = InitializePriceForBundlePurchase(dPrice, bDummy);
                        if (!Utils.IsCouponValid(m_nGroupID, sCouponCode))
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = string.Empty;
                            ret.m_sStatusDescription = "Coupon not valid";
                            WriteToUserLog(sSiteGUID, "While trying to purchase Bundle(CC): " + sBundleCode + " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }

                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.Price p = null;
                        TvinciPricing.PPVModule theBundle = null;

                        switch (bundleType)
                        {
                            case eBundleType.SUBSCRIPTION:
                                {
                                    TvinciPricing.Subscription theSub = null;
                                    p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sBundleCode, sSiteGUID, sCouponCode, ref theReason, ref theSub, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                                    theBundle = theSub;
                                    break;
                                }
                            case eBundleType.COLLECTION:
                                {
                                    TvinciPricing.Collection theCol = null;
                                    p = Utils.GetCollectionFinalPrice(m_nGroupID, sBundleCode, sSiteGUID, sCouponCode, ref theReason, ref theCol, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty);
                                    theBundle = theCol;
                                    break;
                                }
                            default:
                                break;
                        }

                        if (IsTakePriceFromBundleFinalPrice(bDummy, p))
                        {
                            dPrice = p.m_dPrice;
                            sCurrency = p.m_oCurrency.m_sCurrencyCD3;
                        }
                        bool bIsEntitledToPreviewModule = theReason == PriceReason.EntitledToPreviewModule;

                        if (theReason == PriceReason.ForPurchase || bIsEntitledToPreviewModule)
                        {
                            if (bDummy || (p != null && p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency))
                            {
                                if (string.IsNullOrEmpty(sCountryCd) && !string.IsNullOrEmpty(sUserIP))
                                {
                                    sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
                                }

                                switch (bundleType)
                                {
                                    case eBundleType.SUBSCRIPTION:
                                        {
                                            sWSUserName = string.Empty;
                                            sWSPass = string.Empty;

                                            InitializeBillingModule(ref bm, ref sWSUserName, ref sWSPass);

                                            ret = ExecuteCCSubscriprionPurchaseFlow(theBundle as TvinciPricing.Subscription, sBundleCode, sSiteGUID, uObj.m_user.m_domianID, dPrice, sCurrency, sCouponCode,
                                                                        sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, bIsEntitledToPreviewModule, bDummy, sExtraParams,
                                                                        sPaymentMethodID, sEncryptedCVV, p, ref bm, sWSUserName, sWSPass);
                                            break;
                                        }
                                    case eBundleType.COLLECTION:
                                        {
                                            ret = ExecuteCCCollectionPurchaseFlow(theBundle as TvinciPricing.Collection, sBundleCode, sSiteGUID, uObj.m_user.m_domianID, dPrice, sCurrency, sCouponCode,
                                                                        sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, bIsEntitledToPreviewModule, bDummy, sExtraParams,
                                                                        sPaymentMethodID, sEncryptedCVV, p, ref bm);

                                            break;
                                        }
                                    default:
                                        break;
                                }

                            }
                            else
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "The price of the request is not the actual price";
                                WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + " error returned: " + ret.m_sStatusDescription);
                            }
                        }
                        else
                        {
                            // If we reached this else section, there can be only 3 correct price reasons - free or already purchased.
                            // Everything else should be noted as an error of GetSubscription/CollectionFinalPrice

                            string collectionOrSubscription = bundleType == eBundleType.SUBSCRIPTION ? "subscription" : "collection";
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = string.Empty;

                            switch (theReason)
                            {
                                case PriceReason.Free:
                                    {
                                        ret.m_sStatusDescription = string.Format("The {0} is free", collectionOrSubscription);
                                        WriteToUserLog(sSiteGUID, "while trying to purchase " + collectionOrSubscription + "(CC): " + " error returned: " + ret.m_sStatusDescription);

                                        break;
                                    }
                                case PriceReason.SubscriptionPurchased:
                                case PriceReason.CollectionPurchased:
                                    {
                                        ret.m_sStatusDescription = string.Format("The {0} is already purchased", collectionOrSubscription);
                                        WriteToUserLog(sSiteGUID, "while trying to purchase " + collectionOrSubscription + "(CC): " + " error returned: " + ret.m_sStatusDescription);
                                        break;
                                    }
                                default:
                                    {
                                        log.Debug("ChargeUserForBundle" +
                                            string.Format("Flow of CC_BaseChargeUserForBundle went wrong. Get{0}FinalPrice returned " +
                                            "price reason = {1} for site guid = {2} and bundle id = {3}",
                                            collectionOrSubscription, theReason, sSiteGUID, sBundleCode));
                                        break;
                                    }
                            }
                        }
                    }
                } // end else siteguid == ""
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder(String.Concat("Exception msg: ", ex.Message));
                sb.Append(String.Concat(", Stack trace: ", ex.StackTrace));
                sb.Append(String.Concat(", Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(", Price: ", dPrice));
                sb.Append(String.Concat(", Currency: ", sCurrency));
                sb.Append(String.Concat(", Subscription Code: ", sBundleCode));
                sb.Append(String.Concat(", Coupon Code: ", sCouponCode));
                sb.Append(String.Concat(", User IP: ", sUserIP));
                sb.Append(String.Concat(", Extra Params: ", sExtraParams));
                sb.Append(String.Concat(", Country Code: ", sCountryCd));
                sb.Append(String.Concat(", Language Code: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(", Device Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(", Dummy: ", bDummy.ToString().ToLower()));
                log.Error("CC_BaseChargeUserForSubscription - " + sb.ToString(), ex);
                WriteToUserLog(sSiteGUID, string.Format("While trying to purchase subscription id: {0} , Exception occurred.", sBundleCode));
                #endregion
                long lBillingID = 0;
                if (ret.m_oStatus != TvinciBilling.BillingResponseStatus.Success || ret.m_sRecieptCode.Length == 0 || !Int64.TryParse(ret.m_sRecieptCode, out lBillingID) || lBillingID == 0)
                {
                    ret.m_oStatus = TvinciBilling.BillingResponseStatus.UnKnown;
                    ret.m_sStatusDescription = "Undefined";
                    ret.m_sRecieptCode = string.Empty;
                }
            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                }
                if (bm != null)
                {
                    bm.Dispose();
                }
                #endregion
            }
            return ret;
        }

        protected virtual bool IsTakePriceFromBundleFinalPrice(bool isDummy, Price p)
        {
            return isDummy && p != null;
        }

        protected virtual bool IsTakePriceFromMediaFileFinalPrice(bool isDummy)
        {
            return true;
        }

        private TvinciBilling.BillingResponse ExecuteCCSubscriprionPurchaseFlow(TvinciPricing.Subscription theSub, string sBundleCode, string sSiteGUID, int domianID, double dPrice,
                                    string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME,
                                    bool bIsEntitledToPreviewModule, bool bDummy, string sExtraParams, string sPaymentMethodID, string sEncryptedCVV, TvinciPricing.Price p,
                                    ref TvinciBilling.module bm, string sBillingUsername, string sBillingPassword)
        {
            string sCustomData = string.Empty;
            TvinciBilling.BillingResponse ret = new TvinciBilling.BillingResponse()
            {
                m_oStatus = TvinciBilling.BillingResponseStatus.UnKnown
            };

            //Create the Custom Data
            sCustomData = GetCustomDataForSubscription(theSub, null, sBundleCode, string.Empty, sSiteGUID, dPrice, sCurrency,
                sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty, bIsEntitledToPreviewModule ? theSub.m_oPreviewModule.m_nID + "" : string.Empty, bIsEntitledToPreviewModule);

            log.Debug("CustomData - " + string.Format("Subscription custom data created. Site Guid: {0} , User IP: {1} , Custom data: {2}", sSiteGUID, sUserIP, sCustomData));

            bool bIsRecurring = theSub.m_bIsRecurring;
            Int32 nRecPeriods = theSub.m_nNumberOfRecPeriods;

            if (p.m_dPrice != 0 || bDummy || (p.m_dPrice == 0 && (bIsEntitledToPreviewModule || !string.IsNullOrEmpty(sCouponCode))))
            {
                ret = HandleCCChargeUser(sBillingUsername, sBillingPassword, sSiteGUID, dPrice, sCurrency, sUserIP,
                    sCustomData, 1, nRecPeriods, sExtraParams, sPaymentMethodID, sEncryptedCVV,
                    true, bIsEntitledToPreviewModule, ref bm);
            }

            if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
            {
                long lBillingTransactionID = 0;
                long lPurchaseID = 0;

                HandleChargeUserForSubscriptionBillingSuccess(sBillingUsername, sBillingPassword, sSiteGUID, domianID, theSub, dPrice, sCurrency, sCouponCode,
                    sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, ret, bIsEntitledToPreviewModule, sBundleCode, sCustomData,
                    bIsRecurring, ref lBillingTransactionID, ref lPurchaseID, bDummy, ref bm);

                // Update domain DLM with new DLM from subscription or if no DLM in new subscription, with last domain DLM
                if (theSub.m_nDomainLimitationModule != 0)
                {
                    UpdateDLM(domianID, theSub.m_nDomainLimitationModule);
                }

                // Enqueue notification for PS so they know a collection was charged
                var dicData = new Dictionary<string, object>()
                    {
                        {"SubscriptionCode", sBundleCode},
                        {"BillingTransactionID", lBillingTransactionID},
                        {"SiteGUID", sSiteGUID},
                        {"PurchaseID", lPurchaseID},
                        {"CouponCode", sCouponCode},
                        {"CustomData", sCustomData}
                    };

                var isEnqueSuccessful = this.EnqueueEventRecord(NotifiedAction.ChargedSubscription, dicData);
            }
            else
            {
                WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + sBundleCode + " error returned: " + ret.m_sStatusDescription);
            }

            return ret;
        }

        private TvinciBilling.BillingResponse ExecuteCCCollectionPurchaseFlow(TvinciPricing.Collection theCol, string sBundleCode, string sSiteGUID, int domainID, double dPrice,
                                    string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME,
                                    bool bIsEntitledToPreviewModule, bool bDummy, string sExtraParams, string sPaymentMethodID, string sEncryptedCVV, TvinciPricing.Price p,
                                    ref TvinciBilling.module bm)
        {
            string sCustomData = string.Empty;
            TvinciBilling.BillingResponse ret = null;

            //Create the Custom Data
            sCustomData = GetCustomDataForCollection(theCol, sBundleCode, sSiteGUID, dPrice, sCurrency,
                sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty);

            log.Debug("CustomData - " + string.Format("Collection custom data created. Site Guid: {0} , User IP: {1} , Custom data: {2}", sSiteGUID, sUserIP, sCustomData));

            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;

            if (p.m_dPrice != 0 || bDummy)
            {
                InitializeBillingModule(ref bm, ref sWSUserName, ref sWSPass);

                ret = HandleCCChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP,
                    sCustomData, 1, 1, sExtraParams, sPaymentMethodID, sEncryptedCVV,
                    bDummy, bIsEntitledToPreviewModule, ref bm);
            }
            if ((p.m_dPrice == 0 && !string.IsNullOrEmpty(sCouponCode)) || bIsEntitledToPreviewModule)
            {
                ret.m_oStatus = TvinciBilling.BillingResponseStatus.Success;
            }
            if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
            {
                long lBillingTransactionID = 0;
                long lPurchaseID = 0;

                HandleChargeUserForCollectionBillingSuccess(sWSUserName, sWSPass, sSiteGUID, domainID, theCol, dPrice, sCurrency, sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE,
                    sDEVICE_NAME, ret, sBundleCode, sCustomData, ref lBillingTransactionID, ref lPurchaseID, ref bm);

                // Enqueue notification for PS so they know a collection was charged
                var dicData = new Dictionary<string, object>()
                                            {
                                                {"CollectionCode", sBundleCode},
                                                {"BillingTransactionID", lBillingTransactionID},
                                                {"SiteGUID", sSiteGUID},
                                                {"PurchaseID", lPurchaseID},
                                                {"CouponCode", sCouponCode},
                                                {"CustomData", sCustomData}
                                            };

                this.EnqueueEventRecord(NotifiedAction.ChargedCollection, dicData);

            }
            else
            {
                WriteToUserLog(sSiteGUID, "while trying to purchase collection(CC): " + sBundleCode + " error returned: " + ret.m_sStatusDescription);
            }

            return ret;
        }

        /// <summary>
        /// Get Subscriptions Prices 
        /// </summary>
        public virtual SubscriptionsPricesContainer[] GetSubscriptionsPrices(string[] sSubscriptions, string sUserGUID, string sCouponCode,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sIP = null)
        {
            SubscriptionsPricesContainer[] ret = null;
            try
            {
                if (sSubscriptions != null && sSubscriptions.Length > 0)
                {
                    ret = new SubscriptionsPricesContainer[sSubscriptions.Length];

                    for (int i = 0; i < sSubscriptions.Length; i++)
                    {
                        string sSubCode = sSubscriptions[i];
                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.Subscription s = null;
                        TvinciPricing.Price p = null;
                        if (string.IsNullOrEmpty(sIP))
                        {
                            p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubCode, sUserGUID, sCouponCode, ref theReason, ref s, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        }
                        else
                        {
                            p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubCode, sUserGUID, sCouponCode, ref theReason, ref s, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty, sIP);
                        }
                        SubscriptionsPricesContainer cont = new SubscriptionsPricesContainer();
                        cont.Initialize(sSubCode, p, theReason);
                        ret[i] = cont;
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetSubscriptionsPrices. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sUserGUID));
                sb.Append(String.Concat(" Cpn Cd: ", sCouponCode));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Nm: ", sDEVICE_NAME));
                sb.Append(String.Concat(" sIP: ", sIP != null ? sIP : "null"));
                if (sSubscriptions != null && sSubscriptions.Length > 0)
                {
                    sb.Append("Subs: ");
                    for (int i = 0; i < sSubscriptions.Length; i++)
                    {
                        sb.Append(String.Concat(sSubscriptions[i], "; "));
                    }
                }
                else
                {
                    sb.Append("Subs are null or empty.");
                }
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            return ret;
        }

        /// <summary>
        /// Get Collections Prices 
        /// </summary>
        public virtual CollectionsPricesContainer[] GetCollectionsPrices(string[] sCollections, string sUserGUID, string sCouponCode,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            CollectionsPricesContainer[] ret = null;
            try
            {
                if (sCollections != null && sCollections.Length > 0)
                {
                    ret = new CollectionsPricesContainer[sCollections.Length];

                    for (int i = 0; i < sCollections.Length; i++)
                    {
                        string sColCode = sCollections[i];
                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.Collection collection = null;
                        TvinciPricing.Price price = null;

                        price = Utils.GetCollectionFinalPrice(m_nGroupID, sColCode, sUserGUID, sCouponCode, ref theReason, ref collection, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty);

                        CollectionsPricesContainer cont = new CollectionsPricesContainer();
                        cont.Initialize(sColCode, price, theReason);
                        ret[i] = cont;
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetCollectionsPrices. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sUserGUID));
                sb.Append(String.Concat(" Cpn Cd: ", sCouponCode));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Nm: ", sDEVICE_NAME));
                if (sCollections != null && sCollections.Length > 0)
                {
                    sb.Append("Colls: ");
                    for (int i = 0; i < sCollections.Length; i++)
                    {
                        sb.Append(String.Concat(sCollections[i], "; "));
                    }
                }
                else
                {
                    sb.Append("Colls are null or empty.");
                }
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual PrePaidPricesContainer[] GetPrePaidPrices(string[] sPrePaids, string sUserGUID, string sCouponCode,
         string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            PrePaidPricesContainer[] ret = null;
            try
            {
                if (sPrePaids != null && sPrePaids.Length > 0)
                {
                    ret = new PrePaidPricesContainer[sPrePaids.Length];

                    for (int i = 0; i < sPrePaids.Length; i++)
                    {
                        string sPrePaidCode = sPrePaids[i];
                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.PrePaidModule prePaidMod = null;
                        TvinciPricing.Price p = Utils.GetPrePaidFinalPrice(m_nGroupID, sPrePaidCode, sUserGUID, ref theReason, ref prePaidMod, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty, sCouponCode);
                        PrePaidPricesContainer cont = new PrePaidPricesContainer();
                        cont.Initialize(sPrePaidCode, p, theReason);
                        ret[i] = cont;
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetPrePaidPrices. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sUserGUID));
                sb.Append(String.Concat(" Cpn Cd: ", sCouponCode));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Nm: ", sDEVICE_NAME));
                if (sPrePaids != null && sPrePaids.Length > 0)
                {
                    sb.Append("Colls: ");
                    for (int i = 0; i < sPrePaids.Length; i++)
                    {
                        sb.Append(String.Concat(sPrePaids[i], "; "));
                    }
                }
                else
                {
                    sb.Append("Colls are null or empty.");
                }
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            return ret;
        }
        public virtual MediaFileItemPricesContainer[] GetItemsPrices(Int32[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return GetItemsPrices(nMediaFiles, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, "");
        }

        /// <summary>
        /// Get Items Prices
        /// </summary>
        public virtual MediaFileItemPricesContainer[] GetItemsPrices(Int32[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sClientIP)
        {
            string sFirstDeviceNameFound = string.Empty;
            TvinciPricing.mdoule objPricingModule = null;
            MediaFileItemPricesContainer[] ret = null;
            string sPricingUsername = string.Empty;
            string sPricingPassword = string.Empty;
            string sAPIUsername = string.Empty;
            string sAPIPassword = string.Empty;
            bool bCancellationWindow = false;
            try
            {
                TvinciPricing.MediaFilePPVContainer[] oModules = null;
                Utils.GetWSCredentials(m_nGroupID, eWSModules.API, ref sAPIUsername, ref sAPIPassword);
                Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sPricingUsername, ref sPricingPassword);

                // get details about files + media (validity about files)                
                Dictionary<int, MediaFileStatus> validMediaFiles = Utils.ValidateMediaFiles(nMediaFiles);

                //return - MediaAdObject is NotFiniteNumberException validMediaFiles for purchase                    
                List<MediaFileItemPricesContainer> tempRet = new List<MediaFileItemPricesContainer>();
                MediaFileItemPricesContainer tempItemPricesContainer = null;

                List<int> notForPurchaseFiles = validMediaFiles.Where(x => x.Value == MediaFileStatus.NotForPurchase).Select(x => x.Key).ToList();
                nMediaFiles = validMediaFiles.Where(x => x.Value != MediaFileStatus.NotForPurchase).Select(x => x.Key).ToArray();

                foreach (int mf in notForPurchaseFiles)
                {
                    tempItemPricesContainer = new MediaFileItemPricesContainer();
                    tempItemPricesContainer.m_nMediaFileID = mf;
                    tempItemPricesContainer.m_oItemPrices = new ItemPriceContainer[1];
                    tempItemPricesContainer.m_oItemPrices[0] = new ItemPriceContainer();
                    tempItemPricesContainer.m_oItemPrices[0].m_PriceReason = PriceReason.NotForPurchase;
                    tempItemPricesContainer.m_sProductCode = string.Empty;
                    tempRet.Add(tempItemPricesContainer);
                }
                if (nMediaFiles.Count() == 0) // all file not for purchase - return
                {
                    ret = tempRet.ToArray();
                    return ret;
                }

                InitializePricingModule(ref objPricingModule);
                oModules = objPricingModule.GetPPVModuleListForMediaFilesWithExpiry(sPricingUsername, sPricingPassword, nMediaFiles, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                if (oModules != null && oModules.Length > 0)
                {
                    ret = new MediaFileItemPricesContainer[oModules.Length];
                    Dictionary<int, int> mediaFileTypesMapping = null;
                    List<int> allUsersInDomain = null;
                    GetAllUsersInDomainAndMediaFileTypes(oModules, sUserGUID, out mediaFileTypesMapping, out allUsersInDomain);

                    for (int i = 0; i < oModules.Length; i++)
                    {
                        Int32 nMediaFileID = oModules[i].m_nMediaFileID;
                        TvinciPricing.PPVModuleWithExpiry[] ppvModules = oModules[i].m_oPPVModules;
                        MediaFileItemPricesContainer mf = new MediaFileItemPricesContainer();
                        int nMediaFileTypeID = Utils.GetMediaFileTypeID(m_nGroupID, nMediaFileID, sAPIUsername, sAPIPassword);

                        if (ppvModules != null && ppvModules.Length > 0)
                        {
                            List<ItemPriceContainer> itemPriceCont = new List<ItemPriceContainer>();

                            Int32 nLowestIndex = 0;
                            double dLowest = -1;
                            TvinciPricing.Price pLowest = null;
                            PriceReason theLowestReason = PriceReason.UnKnown;
                            TvinciPricing.Subscription relevantLowestSub = null;
                            TvinciPricing.Collection relevantLowestCol = null;
                            TvinciPricing.PrePaidModule relevantLowestPrePaid = null;
                            string sProductCode = string.Empty;
                            bool tempCancellationWindow = false;
                            string lowestPurchasedBySiteGuid = string.Empty;
                            int lowestPurchasedAsMediaFileID = 0;
                            List<int> lowestRelatedMediaFileIDs = new List<int>();
                            DateTime? dtLowestStartDate = null;
                            DateTime? dtLowestEndDate = null;
                            bool isUserSuspended = false;

                            for (int j = 0; j < ppvModules.Length; j++)
                            {
                                string sPPVCode = GetPPVCodeForGetItemsPrices(ppvModules[j].PPVModule.m_sObjectCode, ppvModules[j].PPVModule.m_sObjectVirtualName);

                                PriceReason theReason = PriceReason.UnKnown;
                                TvinciPricing.Subscription relevantSub = null;
                                TvinciPricing.Collection relevantCol = null;
                                TvinciPricing.PrePaidModule relevantPrePaid = null;
                                string purchasedBySiteGuid = string.Empty;
                                int purchasedAsMediaFileID = 0;
                                List<int> relatedMediaFileIDs = new List<int>();
                                DateTime? dtEntitlementStartDate = null;
                                DateTime? dtEntitlementEndDate = null;

                                TvinciPricing.Price p = Utils.GetMediaFileFinalPrice(nMediaFileID, validMediaFiles[nMediaFileID], ppvModules[j].PPVModule, sUserGUID, sCouponCode, m_nGroupID, ppvModules[j].IsValidForPurchase,
                                    ref theReason, ref relevantSub, ref relevantCol, ref relevantPrePaid, ref sFirstDeviceNameFound,
                                    sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, sClientIP, mediaFileTypesMapping,
                                    allUsersInDomain, nMediaFileTypeID, sAPIUsername, sAPIPassword, sPricingUsername, sPricingPassword,
                                    ref bCancellationWindow, ref purchasedBySiteGuid, ref purchasedAsMediaFileID, ref relatedMediaFileIDs, ref dtEntitlementStartDate, ref dtEntitlementEndDate);

                                sProductCode = oModules[i].m_sProductCode;

                                var tempItemPriceContainer = new ItemPriceContainer();
                                tempItemPriceContainer.Initialize(p, ppvModules[j].PPVModule.m_oPriceCode.m_oPrise, sPPVCode, ppvModules[j].PPVModule.m_sDescription,
                                    theReason, relevantSub, relevantCol, ppvModules[j].PPVModule.m_bSubscriptionOnly, relevantPrePaid,
                                    sFirstDeviceNameFound, bCancellationWindow, purchasedBySiteGuid, purchasedAsMediaFileID, relatedMediaFileIDs, dtEntitlementStartDate,
                                    dtEntitlementEndDate);

                                if (theReason == PriceReason.UserSuspended)
                                {
                                    isUserSuspended = true;

                                    if (!bOnlyLowest)
                                    {
                                        itemPriceCont.Add(tempItemPriceContainer);
                                    }
                                    else
                                    {
                                        if (j == 0)//insert only the first ppvModule (when the user is suspended we cannot compare prices)
                                        {
                                            itemPriceCont.Insert(0, tempItemPriceContainer);
                                        }
                                    }
                                }
                                else //user is not suspended
                                {
                                    bool isValidForPurchase = ppvModules[j].IsValidForPurchase;
                                    if (isValidForPurchase || (!isValidForPurchase && theReason == PriceReason.PPVPurchased))
                                    {
                                        if (!bOnlyLowest)
                                        {
                                            itemPriceCont.Add(tempItemPriceContainer);
                                        }
                                        else
                                        {
                                            if (p != null && (p.m_dPrice < dLowest || j == 0))
                                            {
                                                #region insert lowest price parameters
                                                nLowestIndex = j;
                                                dLowest = p.m_dPrice;
                                                pLowest = p;
                                                theLowestReason = theReason;
                                                relevantLowestSub = relevantSub;
                                                relevantLowestCol = relevantCol;
                                                relevantLowestPrePaid = relevantPrePaid;
                                                tempCancellationWindow = bCancellationWindow;
                                                lowestPurchasedBySiteGuid = purchasedBySiteGuid;
                                                lowestPurchasedAsMediaFileID = purchasedAsMediaFileID;
                                                lowestRelatedMediaFileIDs = relatedMediaFileIDs;
                                                dtLowestStartDate = dtEntitlementStartDate;
                                                dtLowestEndDate = dtEntitlementEndDate;
                                                #endregion
                                            }
                                        }
                                    }
                                }
                            } // end for

                            if (ppvModules.Length > 0 && itemPriceCont.Count == 0 && !isUserSuspended)
                            {
                                if (bOnlyLowest)
                                {
                                    var tempItemPriceContainer = new ItemPriceContainer();

                                    tempItemPriceContainer.Initialize(pLowest, ppvModules[nLowestIndex].PPVModule.m_oPriceCode.m_oPrise,
                                        ppvModules[nLowestIndex].PPVModule.m_sObjectCode, ppvModules[nLowestIndex].PPVModule.m_sDescription, theLowestReason,
                                        relevantLowestSub, relevantLowestCol, ppvModules[nLowestIndex].PPVModule.m_bSubscriptionOnly,
                                        relevantLowestPrePaid, sFirstDeviceNameFound, tempCancellationWindow,
                                        lowestPurchasedBySiteGuid, lowestPurchasedAsMediaFileID, lowestRelatedMediaFileIDs, dtLowestStartDate, dtLowestEndDate);
                                    itemPriceCont.Insert(0, tempItemPriceContainer);
                                }
                                else
                                {
                                    ItemPriceContainer[] priceContainer = new ItemPriceContainer[1];
                                    priceContainer[0] = GetFreeItemPriceContainer();

                                    itemPriceCont.Insert(0, priceContainer[0]);

                                }
                            }

                            mf.Initialize(nMediaFileID, itemPriceCont.ToArray(), sProductCode);
                        }
                        else
                        {
                            ItemPriceContainer[] priceContainer = new ItemPriceContainer[1];
                            priceContainer[0] = GetFreeItemPriceContainer();

                            mf.Initialize(nMediaFileID, priceContainer);
                        }

                        ret[i] = mf;
                    }
                }
                else
                {
                    ret = new MediaFileItemPricesContainer[1];
                    MediaFileItemPricesContainer mc = new MediaFileItemPricesContainer();
                    foreach (int mediaFileID in nMediaFiles)
                    {
                        ItemPriceContainer freeContainer = new ItemPriceContainer();
                        freeContainer.m_PriceReason = PriceReason.Free;
                        ItemPriceContainer[] priceContainer = new ItemPriceContainer[1];
                        priceContainer[0] = freeContainer;

                        mc.Initialize(mediaFileID, priceContainer);
                    }
                    ret[0] = mc;
                }

                // add all files that are not for purchased
                tempRet.AddRange(ret);
                ret = tempRet.ToArray();
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder(String.Concat("GetItemsPrices Exception. Msg: ", ex.Message));
                sb.Append(String.Concat(" SiteGuid: ", sUserGUID));
                sb.Append(String.Concat(" Coupon Code: ", sCouponCode));
                sb.Append(String.Concat(" OnlyLowest: ", bOnlyLowest.ToString().ToLower()));
                if (nMediaFiles != null && nMediaFiles.Length > 0)
                {
                    sb.Append(" Media Files: ");
                    for (int i = 0; i < nMediaFiles.Length; i++)
                    {
                        sb.Append(String.Concat(nMediaFiles[i], " "));
                    }
                }
                else
                {
                    sb.Append(" No Media Files ");
                }
                sb.Append(String.Concat(" BaseCAS is: ", this.GetType().Name));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);

                ret = null;
                #endregion
            }
            finally
            {
                #region Disposing
                if (objPricingModule != null)
                {
                    objPricingModule.Dispose();
                }

                #endregion
            }

            return ret;
        }

        /*
         * 1. This method is a helper function for GetItemsPrices.
         * 2. It is used to optimize DB access. In case the data is not needed in the function Utils.GetMediaFileFinalPrice it will not attempt
         * 3. to access the DB.
         */
        private void GetAllUsersInDomainAndMediaFileTypes(TvinciPricing.MediaFilePPVContainer[] oModules, string sSiteGuid,
            out Dictionary<int, int> mediaFileTypesMapping, out List<int> allUsersInDomain)
        {
            long lSiteGuid = 0;
            if (!string.IsNullOrEmpty(sSiteGuid) && Int64.TryParse(sSiteGuid, out lSiteGuid) && lSiteGuid > 0 && IsExistPPVModule(oModules))
            {
                mediaFileTypesMapping = ConditionalAccessDAL.Get_GroupMediaTypesIDs(m_nGroupID);
                allUsersInDomain = Utils.GetAllUsersDomainBySiteGUID(sSiteGuid, m_nGroupID);
            }
            else
            {
                mediaFileTypesMapping = new Dictionary<int, int>(0);
                allUsersInDomain = new List<int>(0);
            }
        }

        private bool IsExistPPVModule(TvinciPricing.MediaFilePPVContainer[] oModules)
        {
            if (oModules != null && oModules.Length > 0)
            {
                for (int i = 0; i < oModules.Length; i++)
                {
                    PPVModuleWithExpiry[] ppvModules = oModules[i].m_oPPVModules;
                    if (ppvModules != null && ppvModules.Length > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected ItemPriceContainer GetFreeItemPriceContainer()
        {
            ItemPriceContainer freeContainer = new ItemPriceContainer();
            freeContainer.m_PriceReason = PriceReason.Free;
            freeContainer.m_oPrice = new TvinciPricing.Price();
            freeContainer.m_oPrice.m_dPrice = 0.0;

            return freeContainer;
        }

        protected void InitializePricingModule(ref TvinciPricing.mdoule pm)
        {

            pm = new TvinciPricing.mdoule();

            string sWSURL = Utils.GetWSURL("pricing_ws");
            if (!string.IsNullOrEmpty(sWSURL))
            {
                pm.Url = sWSURL;
            }
        }


        protected void InitializeBillingModule(ref TvinciBilling.module bm, ref string sWSUsername, ref string sWSPassword)
        {
            Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUsername, ref sWSPassword);

            bm = new TvinciBilling.module();

            string sWSURL = Utils.GetWSURL("billing_ws");
            if (!string.IsNullOrEmpty(sWSURL))
            {
                bm.Url = sWSURL;
            }
        }

        /// <summary>
        /// Get Subscription Dates
        /// </summary>
        protected void GetPurchaseItemDates(Int32 nPurchaseID, ref DateTime dStartDate, ref DateTime dEndDate, BillingItemsType billingType)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = null;

            string tableName = string.Empty;
            switch (billingType)
            {

                case BillingItemsType.PPV:
                    tableName = "ppv_purchases";
                    break;

                case BillingItemsType.PrePaid:
                case BillingItemsType.PrePaidExpired:
                    tableName = "pre_paid_purchases";
                    break;

                case BillingItemsType.Collection:
                    tableName = "collections_purchases";
                    break;

                case BillingItemsType.Unknown:
                case BillingItemsType.Subscription:
                default:
                    tableName = "subscriptions_purchases";
                    break;
            }

            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += string.Format("select START_DATE, END_DATE from {0} with (nolock) where ", tableName);
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        dStartDate = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["START_DATE"]);
                        dEndDate = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["END_DATE"]);
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
        }
        /// <summary>
        /// Get Media Title
        /// </summary>
        protected string GetMediaTitle(Int32 nMediaID)
        {
            string sRet = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select name from media with (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        sRet = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
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


        /*
        /// <summary>
        /// Get Safe Double
        /// </summary>
        protected double GetSafeDouble(object o)
        {
            try
            {
                return double.Parse(o.ToString());
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// Get Safe Int
        /// </summary>
        protected Int32 GetSafeInt(object o)
        {
            try
            {
                return int.Parse(o.ToString());
            }
            catch
            {
                return 0;
            }
        }
        */

        /// <summary>
        /// Get Domains Billing History
        /// 
        /// (for Eutelsat Project)
        /// </summary>
        public virtual DomainBillingTransactionsResponse[] GetDomainsBillingHistory(int[] domainIDs, DateTime dStartDate, DateTime dEndDate)
        {
            List<DomainBillingTransactionsResponse> lDomainBillingTransactions = new List<DomainBillingTransactionsResponse>();

            if (domainIDs == null || domainIDs.Length == 0)
            {
                return lDomainBillingTransactions.ToArray();
            }

            for (int i = 0; i < domainIDs.Length; i++)
            {
                try
                {
                    DomainBillingTransactionsResponse domainBillingTransactions = new DomainBillingTransactionsResponse();
                    domainBillingTransactions.m_nDomainID = domainIDs[i];

                    string[] sUserGuids = DomainDal.GetUsersInDomain(domainIDs[i], m_nGroupID, 1, 1).Select(ut => ut.Key.ToString()).ToArray();
                    //string[] sUserGuids = userIDs.Select(u => u.ToString()).ToArray();

                    domainBillingTransactions.m_BillingTransactionResponses = GetUsersBillingHistory(sUserGuids, dStartDate, dEndDate);

                    lDomainBillingTransactions.Add(domainBillingTransactions);
                }
                catch (Exception ex)
                {
                    #region Logging
                    StringBuilder sb = new StringBuilder("Exception at GetDomainsBillingHistory. ");
                    sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                    sb.Append(String.Concat(" Start Date: ", dStartDate));
                    sb.Append(String.Concat(" End Date: ", dEndDate));
                    sb.Append(String.Concat(" Domain ID: ", domainIDs[i]));
                    sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                    sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));

                    log.Error("Exception - " + sb.ToString(), ex);
                    #endregion

                }
            }

            return lDomainBillingTransactions.ToArray();
        }

        /// <summary>
        /// Get Users Billing History
        /// 
        /// (for Eutelsat Project)
        /// </summary>
        public virtual UserBillingTransactionsResponse[] GetUsersBillingHistory(string[] arrUserGUIDs, DateTime dStartDate, DateTime dEndDate)
        {
            List<UserBillingTransactionsResponse> lUserBillingTransactions = new List<UserBillingTransactionsResponse>();

            if (arrUserGUIDs == null || arrUserGUIDs.Length == 0)
            {
                return lUserBillingTransactions.ToArray();
            }

            for (int i = 0; i < arrUserGUIDs.Length; i++)
            {
                try
                {
                    UserBillingTransactionsResponse userBillingTransactions = new UserBillingTransactionsResponse();
                    userBillingTransactions.m_sSiteGUID = arrUserGUIDs[i];
                    BillingTransactions billingTransactions = GetUserBillingHistoryExt(arrUserGUIDs[i], dStartDate, dEndDate);
                    if (billingTransactions != null)
                    {
                        userBillingTransactions.m_BillingTransactionResponse = billingTransactions.transactions;
                    }
                    lUserBillingTransactions.Add(userBillingTransactions);
                }
                catch (Exception ex)
                {
                    #region Logging
                    StringBuilder sb = new StringBuilder("Exception at GetUsersBillingHistory. ");
                    sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                    sb.Append(String.Concat(" Start Date: ", dStartDate));
                    sb.Append(String.Concat(" End Date: ", dEndDate));
                    sb.Append(String.Concat(" Site Guid: ", arrUserGUIDs[i]));
                    sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                    sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));

                    log.Error("Exception - " + sb.ToString(), ex);
                    #endregion
                }
            }

            return lUserBillingTransactions.ToArray();
        }

        /// <summary>
        /// Get User Billing History
        /// </summary>
        protected virtual BillingTransactions GetUserBillingHistoryExt(string sUserGUID, DateTime dStartDate, DateTime dEndDate, int nStartIndex = 0, int nNumberOfItems = 0)
        {

            BillingTransactionsResponse theResp = new BillingTransactionsResponse();
            BillingTransactions response = new BillingTransactions();
            TvinciPricing.mdoule m = null;

            try
            {
                List<int> lGroupIDs = UtilsDal.GetAllRelatedGroups(m_nGroupID);
                string[] arrGroupIDs = lGroupIDs.Select(g => g.ToString()).ToArray();

                int nTopNum = nStartIndex + nNumberOfItems;
                DataView dvBillHistory = ConditionalAccessDAL.GetUserBillingHistory(arrGroupIDs, sUserGUID, nTopNum, dStartDate, dEndDate);


                if (dvBillHistory == null || dvBillHistory.Count == 0)
                {
                    response.resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no history billing for user");
                    return response;
                }

                int nCount = dvBillHistory.Count;

                if (nTopNum > nCount || nTopNum == 0)
                {
                    nTopNum = nCount;
                }

                theResp.m_nTransactionsCount = nCount;
                theResp.m_Transactions = new BillingTransactionContainer[nCount];

                m = new ConditionalAccess.TvinciPricing.mdoule();
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                string pricingUrl = Utils.GetWSURL("pricing_ws");
                if (!string.IsNullOrEmpty(pricingUrl))
                {
                    m.Url = pricingUrl;
                }

                for (int i = nStartIndex; i < nTopNum; i++)
                {
                    theResp.m_Transactions[i] = new BillingTransactionContainer();

                    string sCurrencyCode = ODBCWrapper.Utils.GetSafeStr(dvBillHistory[i].Row["CURRENCY_CODE"]);
                    string sRemarks = ODBCWrapper.Utils.GetSafeStr(dvBillHistory[i].Row["REMARKS"]);

                    double dPrice = ODBCWrapper.Utils.GetDoubleSafeVal(dvBillHistory[i].Row["TOTAL_PRICE"]);
                    Int32 nPurchaseID = ODBCWrapper.Utils.GetIntSafeVal(dvBillHistory[i].Row["PURCHASE_ID"]);
                    DateTime dActionDate = (DateTime)(dvBillHistory[i].Row["CREATE_DATE"]);
                    string sSubscriptionCode = dvBillHistory[i].Row["SUBSCRIPTION_CODE"].ToString();
                    Int32 nMediaID = ODBCWrapper.Utils.GetIntSafeVal(dvBillHistory[i].Row["MEDIA_ID"]);
                    string sLAST_FOUR_DIGITS = dvBillHistory[i].Row["LAST_FOUR_DIGITS"].ToString();
                    string sCellNum = dvBillHistory[i].Row["CELL_PHONE"].ToString();
                    string sID = dvBillHistory[i].Row["ID"].ToString();
                    int nPAYMENT_NUMBER = ODBCWrapper.Utils.GetIntSafeVal(dvBillHistory[i].Row["PAYMENT_NUMBER"]);
                    int nNUMBER_OF_PAYMENTS = ODBCWrapper.Utils.GetIntSafeVal(dvBillHistory[i].Row["NUMBER_OF_PAYMENTS"]);
                    int nBILLING_METHOD = ODBCWrapper.Utils.GetIntSafeVal(dvBillHistory[i].Row["BILLING_METHOD"]);
                    int nBILLING_PROCESSOR = ODBCWrapper.Utils.GetIntSafeVal(dvBillHistory[i].Row["BILLING_PROCESSOR"]);
                    int nNEW_RENEWABLE_STATUS = ODBCWrapper.Utils.GetIntSafeVal(dvBillHistory[i].Row["NEW_RENEWABLE_STATUS"]);
                    int nBILLING_PROVIDER = ODBCWrapper.Utils.GetIntSafeVal(dvBillHistory[i].Row["BILLING_PROVIDER"]);
                    int nBILLING_PROVIDER_REFFERENCE = ODBCWrapper.Utils.GetIntSafeVal(dvBillHistory[i].Row["BILLING_PROVIDER_REFFERENCE"]);
                    int nPURCHASE_ID = ODBCWrapper.Utils.GetIntSafeVal(dvBillHistory[i].Row["PURCHASE_ID"]);
                    string sPrePaidCode = ODBCWrapper.Utils.GetSafeStr(dvBillHistory[i].Row["pre_paid_code"]);
                    string collectionCode = ODBCWrapper.Utils.GetSafeStr(dvBillHistory[i].Row["COLLECTION_CODE"]);


                    if (nBILLING_PROVIDER == -1)
                    {
                        if (nNEW_RENEWABLE_STATUS == 0)
                        {
                            theResp.m_Transactions[i].m_eBillingAction = BillingAction.CancelSubscriptionOrder;
                        }
                        if (nNEW_RENEWABLE_STATUS == 1)
                        {
                            theResp.m_Transactions[i].m_eBillingAction = BillingAction.RenewCancledSubscription;
                        }
                    }
                    else if (nBILLING_PROVIDER == -2)
                    {
                        theResp.m_Transactions[i].m_eBillingAction = BillingAction.SubscriptionDateChanged;
                    }
                    else
                    {
                        if (nPAYMENT_NUMBER == 1)
                        {
                            theResp.m_Transactions[i].m_eBillingAction = BillingAction.Purchase;
                        }
                        if (nPAYMENT_NUMBER > 1)
                        {
                            theResp.m_Transactions[i].m_eBillingAction = BillingAction.RenewPayment;
                        }
                    }



                    if (!string.IsNullOrEmpty(sPrePaidCode))
                    {

                        theResp.m_Transactions[i].m_eItemType = BillingItemsType.PrePaid;

                        TvinciPricing.PrePaidModule thePrePaid = null;

                        thePrePaid = m.GetPrePaidModuleData(sWSUserName, sWSPass, int.Parse(sPrePaidCode), string.Empty, string.Empty, string.Empty);

                        theResp.m_Transactions[i].m_sPurchasedItemCode = sPrePaidCode;
                        theResp.m_Transactions[i].m_sPurchasedItemName = thePrePaid.m_Title;

                    }

                    if (!string.IsNullOrEmpty(sSubscriptionCode))
                    {
                        theResp.m_Transactions[i].m_eItemType = BillingItemsType.Subscription;


                        TvinciPricing.Subscription theSub = null;
                        theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubscriptionCode, string.Empty, string.Empty, string.Empty, true);

                        string sMainLang = string.Empty;
                        string sMainLangCode = string.Empty;
                        GetMainLang(ref sMainLang, ref sMainLangCode, m_nGroupID);
                        if (theSub.m_sName != null)
                        {
                            Int32 nNameLangLength = theSub.m_sName.Length;
                            for (int j = 0; j < nNameLangLength; j++)
                            {
                                string sLang = theSub.m_sName[j].m_sLanguageCode3;
                                string sVal = theSub.m_sName[j].m_sValue;

                                if (sLang == sMainLangCode)
                                {
                                    theResp.m_Transactions[i].m_sPurchasedItemName = sVal;
                                }
                            }
                        }

                        theResp.m_Transactions[i].m_sPurchasedItemCode = sSubscriptionCode;

                        theResp.m_Transactions[i].m_bIsRecurring = theSub.m_bIsRecurring;
                    }

                    // check if transaction is a collection type
                    if (!string.IsNullOrEmpty(collectionCode))
                    {
                        // update type
                        theResp.m_Transactions[i].m_eItemType = BillingItemsType.Collection;


                        // get collection data
                        TvinciPricing.Collection collection = null;
                        collection = m.GetCollectionData(sWSUserName, sWSPass, collectionCode, string.Empty, string.Empty, string.Empty, true);

                        // get collection name
                        if (collection != null)
                            theResp.m_Transactions[i].m_sPurchasedItemName = ((PPVModule)collection).m_sObjectVirtualName;

                        theResp.m_Transactions[i].m_sPurchasedItemCode = collectionCode;
                    }

                    if (nMediaID != 0)
                    {
                        theResp.m_Transactions[i].m_eItemType = BillingItemsType.PPV;

                        theResp.m_Transactions[i].m_sPurchasedItemName = GetMediaTitle(nMediaID);
                        theResp.m_Transactions[i].m_sPurchasedItemCode = nMediaID.ToString();
                    }

                    //if (nBILLING_METHOD >= 1)
                    PaymentMethod pm = (PaymentMethod)(nBILLING_METHOD);
                    theResp.m_Transactions[i].m_ePaymentMethod = pm;


                    if (pm == PaymentMethod.CreditCard || pm == PaymentMethod.Visa || pm == PaymentMethod.MasterCard)
                    {
                        theResp.m_Transactions[i].m_sPaymentMethodExtraDetails = sLAST_FOUR_DIGITS;
                    }

                    if (pm == PaymentMethod.SMS || pm == PaymentMethod.M1)
                    {
                        theResp.m_Transactions[i].m_sPaymentMethodExtraDetails = sCellNum;
                    }

                    theResp.m_Transactions[i].m_bIsRecurring = false;

                    theResp.m_Transactions[i].m_sRecieptCode = sID;
                    theResp.m_Transactions[i].m_nBillingProviderRef = nBILLING_PROVIDER_REFFERENCE;
                    theResp.m_Transactions[i].m_nPurchaseID = nPURCHASE_ID;
                    theResp.m_Transactions[i].m_sRemarks = sRemarks;

                    //action date
                    theResp.m_Transactions[i].m_dtActionDate = dActionDate;

                    //Subscription dates
                    if (nPurchaseID != 0)
                    {
                        GetPurchaseItemDates(nPurchaseID, ref theResp.m_Transactions[i].m_dtStartDate, ref theResp.m_Transactions[i].m_dtEndDate, theResp.m_Transactions[i].m_eItemType);
                    }

                    if (!string.IsNullOrEmpty(sCurrencyCode))
                    {
                        theResp.m_Transactions[i].m_Price = new ConditionalAccess.TvinciPricing.Price();
                        theResp.m_Transactions[i].m_Price.m_dPrice = dPrice;
                        theResp.m_Transactions[i].m_Price.m_oCurrency = m.GetCurrencyValues(sWSUserName, sWSPass, sCurrencyCode);
                    }
                } // for

                response.resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                response.transactions = theResp;
            }
            catch (Exception ex)
            {
                log.Error("GetUserBillingHistoryExt - " + string.Format("UserGUID={0}, dStartDate={1}, dEndDate={2}, nStartIndex={3},nNumberOfItems={4}, ex={5} ", sUserGUID, dStartDate, dEndDate, nStartIndex, nNumberOfItems, ex.Message), ex);
                response.resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, ex.Message);
            }
            finally
            {
                if (m != null)
                {
                    m.Dispose();
                }
            }

            return response;
        }


        /// <summary>
        /// Get User Billing History
        /// </summary>
        public virtual BillingTransactions GetUserBillingHistory(string sUserGUID, Int32 nStartIndex, Int32 nNumberOfItems)
        {
            BillingTransactions res = null;
            try
            {
                DateTime minDate = new DateTime(2000, 1, 1);
                res = GetUserBillingHistoryExt(sUserGUID, minDate, DateTime.MaxValue, nStartIndex, nNumberOfItems);
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetUserBillingHistory. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sUserGUID));
                sb.Append(String.Concat(" Start Index: ", nStartIndex));
                sb.Append(String.Concat(" Num Of Items: ", nNumberOfItems));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            return res;

        }

        /// <summary>
        /// GetI tems Prices
        /// </summary>
        public virtual MediaFileItemPricesContainer[] GetItemsPrices(Int32[] nMediaFiles, string sUserGUID, bool bOnlyLowest,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return GetItemsPrices(nMediaFiles, sUserGUID, "", bOnlyLowest, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, "");
        }
        /// <summary>
        /// GetI tems Prices
        /// </summary>
        public virtual MediaFileItemPricesContainer[] GetItemsPrices(Int32[] nMediaFiles, string sUserGUID, bool bOnlyLowest,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sClientIP = null)
        {
            return GetItemsPrices(nMediaFiles, sUserGUID, "", bOnlyLowest, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, sClientIP);
        }
        /// <summary>
        /// Is Subscription Purchased
        /// </summary>
        public virtual bool IsSubscriptionPurchased(string siteGuid, string subID, ref string reason)
        {
            return true;
        }
        /// <summary>
        /// Is Item Permitted
        /// </summary>
        public virtual bool IsItemPermitted(string sSiteGUID, int mediaID)
        {
            return true;
        }

        /// <summary>
        /// Handle Coupon Uses
        /// </summary>
        protected virtual void HandleCouponUses(TvinciPricing.Subscription relevantSub, string sPPVModuleCode,
            string sSiteGUID, double dPrice, string sCurrency,
            Int32 nMediaFileID, string sCouponCode, string sUserIP,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bFromPurchase, int nPrePaidCode, Int32 relevantCollection)
        {
            if (!string.IsNullOrEmpty(sCouponCode))
            {

                if (bFromPurchase == false)
                {
                    double dPercent = Utils.GetCouponDiscountPercent(m_nGroupID, sCouponCode);

                    if (dPercent < 100)
                        return;
                }

                Int32 nSubCode = 0;

                if (relevantSub != null && relevantSub.m_sObjectCode != null && relevantSub.m_sObjectCode != string.Empty)
                {
                    nSubCode = int.Parse(relevantSub.m_sObjectCode);
                }

                //No media_file and no sub --> do nothing
                if (nMediaFileID == 0 && nSubCode == 0 && nPrePaidCode == 0)
                    return;

                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);

                TvinciPricing.mdoule m = null;
                try
                {
                    m = new global::ConditionalAccess.TvinciPricing.mdoule();
                    string sWSURL = Utils.GetWSURL("pricing_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        m.Url = sWSURL;
                    }
                    m.SetCouponUses(sWSUserName, sWSPass, sCouponCode, sSiteGUID, nMediaFileID, nSubCode, nPrePaidCode, relevantCollection);
                }
                catch (Exception ex)
                {
                    #region Logging
                    StringBuilder sb = new StringBuilder(String.Concat("Exception message: ", ex.Message));
                    sb.Append(String.Concat(" Relevant Sub ID: ", relevantSub != null ? relevantSub.m_fictivicMediaID.ToString() : "null"));
                    sb.Append(String.Concat(" PPV Module Code: ", sPPVModuleCode));
                    sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                    sb.Append(String.Concat(" Price: ", dPrice));
                    sb.Append(String.Concat(" Currency: ", sCurrency));
                    sb.Append(String.Concat(" Media File ID: ", nMediaFileID));
                    sb.Append(String.Concat(" Coupon Code: ", sCouponCode));
                    sb.Append(String.Concat(" User IP ", sUserIP));
                    sb.Append(String.Concat(" Country Code: ", sCountryCd));
                    sb.Append(String.Concat(" Language Code: ", sLANGUAGE_CODE));
                    sb.Append(String.Concat(" Device Name: ", sDEVICE_NAME));
                    sb.Append(String.Concat(" bFromPurchase: ", bFromPurchase.ToString().ToLower()));
                    sb.Append(String.Concat(" Pre Paid Code: ", nPrePaidCode));
                    sb.Append(String.Concat(" ST: ", ex.StackTrace));
                    log.Debug("HandleCouponUses - " + sb.ToString());
                    #endregion
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

            }
        }

        /// <summary>
        /// Get CustomData string  
        /// </summary>

        protected virtual string GetCustomData(TvinciPricing.Subscription relevantSub, TvinciPricing.PPVModule thePPVModule, TvinciPricing.Campaign campaign,
               string sSiteGUID, double dPrice, string sCurrency,
               Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCampaignCode, string sCouponCode, string sUserIP,
               string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sOverrideEndDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<customdata type=\"pp\">");
            if (String.IsNullOrEmpty(sCountryCd) == false)
                sb.Append("<lcc>" + sCountryCd + "</lcc>");
            else
            {
                sb.AppendFormat("<lcc>{0}</lcc>", TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP));
            }
            if (String.IsNullOrEmpty(sLANGUAGE_CODE) == false)
                sb.Append("<llc>" + sLANGUAGE_CODE + "</llc>");
            if (String.IsNullOrEmpty(sDEVICE_NAME) == false)
                sb.Append("<ldn>" + sDEVICE_NAME + "</ldn>");
            sb.Append("<rs>");
            if (relevantSub != null)
                sb.Append(relevantSub.m_sObjectCode);
            sb.Append("</rs>");
            sb.Append("<mnou>");
            if (thePPVModule != null && thePPVModule.m_oUsageModule != null)
                sb.Append(thePPVModule.m_oUsageModule.m_nMaxNumberOfViews.ToString());
            sb.Append("</mnou>");
            sb.Append("<mumlc>");
            if (thePPVModule != null && thePPVModule.m_oUsageModule != null)
                sb.Append(thePPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle.ToString());
            sb.Append("</mumlc>");
            sb.Append("<oed>");
            sb.Append(sOverrideEndDate);
            sb.Append("</oed>");
            sb.Append("<u id=\"" + sSiteGUID + "\"/>");
            sb.Append("<mf>");
            sb.Append(nMediaFileID.ToString());
            sb.Append("</mf>");
            sb.Append("<m>");
            sb.Append(nMediaID.ToString());
            sb.Append("</m>");
            sb.Append("<ppvm>");
            sb.Append(sPPVModuleCode);
            sb.Append("</ppvm>");
            sb.Append("<pm></pm>");
            sb.Append("<cc>");
            sb.Append(sCouponCode);
            sb.Append("</cc>");
            sb.Append("<campcode>");
            sb.Append(sCampaignCode);
            sb.Append("</campcode>");
            sb.Append("<cmnov>");
            if (campaign != null && campaign.m_usageModule != null)
            {
                sb.Append(campaign.m_usageModule.m_nMaxNumberOfViews.ToString());
            }
            sb.Append("</cmnov>");
            sb.Append("<cmumlc>");
            if (campaign != null && campaign.m_usageModule != null)
                sb.Append(campaign.m_usageModule.m_tsMaxUsageModuleLifeCycle.ToString());
            sb.Append("</cmumlc>");
            sb.Append("<p ir=\"false\" n=\"1\" o=\"1\"/>");

            sb.Append("<pc>");
            if (thePPVModule != null && thePPVModule.m_oPriceCode != null)
                sb.Append(thePPVModule.m_oPriceCode.m_sCode);
            sb.Append("</pc>");
            sb.Append("<pri>");
            sb.Append(dPrice.ToString());
            sb.Append("</pri>");
            sb.Append("<cu>");
            sb.Append(sCurrency);
            sb.Append("</cu>");
            sb.Append("</customdata>");

            return sb.ToString();
        }

        // Get CustomData string
        protected virtual string GetCustomData(TvinciPricing.Subscription relevantSub, TvinciPricing.PPVModule thePPVModule, TvinciPricing.Campaign campaign,
                                               string sSiteGUID, double dPrice, string sCurrency, Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode,
                                               string sCampaignCode, string sCouponCode, string sUserIP, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return GetCustomData(relevantSub, thePPVModule, campaign, sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode,
                                 sCampaignCode, sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty);
        }

        /// <summary>
        /// Get Custom Data For Pre Paid
        /// </summary>
        protected virtual string GetCustomDataForPrePaid(TvinciPricing.PrePaidModule thePrePaidModule, TvinciPricing.Campaign campaign, string sPrePaidCode, string sCampaignCode,
        string sSiteGUID, double dPrice, string sCurrency, string sCouponCode, string sUserIP,
        string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sOverrideEndDate)
        {
            bool bIsFixed = thePrePaidModule.m_isFixedCredit;
            StringBuilder sb = new StringBuilder();
            sb.Append("<customdata type=\"prepaid\">");
            if (String.IsNullOrEmpty(sCountryCd) == false)
            {
                sb.Append("<lcc>" + sCountryCd + "</lcc>");
            }
            else
            {
                sb.AppendFormat("<lcc>{0}</lcc>", TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP));
            }

            if (String.IsNullOrEmpty(sLANGUAGE_CODE) == false)
                sb.Append("<llc>" + sLANGUAGE_CODE + "</llc>");
            if (String.IsNullOrEmpty(sDEVICE_NAME) == false)
                sb.Append("<ldn>" + sDEVICE_NAME + "</ldn>");
            sb.Append("<mnou>");
            if (thePrePaidModule != null && thePrePaidModule.m_UsageModule != null)
                sb.Append(thePrePaidModule.m_UsageModule.m_nMaxNumberOfViews.ToString());
            sb.Append("</mnou>");
            sb.Append("<u id=\"" + sSiteGUID + "\"/>");
            sb.AppendFormat("<up>{0}</up>", sUserIP);
            sb.AppendFormat("<pp>{0}</pp>", sPrePaidCode);
            sb.AppendFormat("<cc>{0}</cc>", sCouponCode);
            sb.Append("<if if=\"" + bIsFixed.ToString().ToLower() + "\"/>");
            sb.Append("<vlcs>");
            if (thePrePaidModule != null && thePrePaidModule.m_UsageModule != null)
                sb.Append(thePrePaidModule.m_UsageModule.m_tsViewLifeCycle.ToString());
            sb.Append("</vlcs>");
            sb.Append("<mumlc>");
            if (thePrePaidModule != null && thePrePaidModule.m_UsageModule != null)
                sb.Append(thePrePaidModule.m_UsageModule.m_tsMaxUsageModuleLifeCycle.ToString());
            sb.Append("</mumlc>");
            sb.Append("<oed>");
            sb.Append(sOverrideEndDate);
            sb.Append("</oed>");
            sb.Append("<ppvm>");
            sb.Append("</ppvm>");
            sb.Append("<pm></pm>");
            sb.Append("<pc>");
            if (thePrePaidModule != null && thePrePaidModule.m_PriceCode != null)
                sb.Append(thePrePaidModule.m_PriceCode.m_sCode);
            sb.Append("</pc>");
            sb.Append("<cvpc>");
            if (thePrePaidModule != null && thePrePaidModule.m_CreditValue != null)
                sb.Append(thePrePaidModule.m_CreditValue.m_sCode);
            sb.Append("</cvpc>");
            sb.AppendFormat("<pri>{0}</pri>", dPrice.ToString());
            sb.Append("<cpri>");
            if (thePrePaidModule != null && thePrePaidModule.m_CreditValue != null)
                sb.Append(thePrePaidModule.m_CreditValue.m_oPrise.m_dPrice.ToString());

            sb.Append("</cpri>");
            sb.AppendFormat("<campcode>{0}</campcode>", sCampaignCode);
            sb.Append("<cmnov>");
            if (campaign != null && campaign.m_usageModule != null)
            {
                sb.Append(campaign.m_usageModule.m_tsMaxUsageModuleLifeCycle.ToString());
            }
            sb.Append("</cmnov>");
            sb.Append("<cmumlc>");
            if (campaign != null && campaign.m_usageModule != null)
                sb.Append(campaign.m_usageModule.m_tsMaxUsageModuleLifeCycle.ToString());
            sb.Append("</cmumlc>");
            sb.AppendFormat("<cu>{0}</cu>", sCurrency);
            sb.Append("</customdata>");

            return sb.ToString();

        }

        protected virtual string GetCustomDataForPrePaid(TvinciPricing.PrePaidModule thePrePaidModule, TvinciPricing.Campaign campaign, string sPrePaidCode, string sCampaignCode,
           string sSiteGUID, double dPrice, string sCurrency, string sCouponCode, string sUserIP,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return GetCustomDataForPrePaid(thePrePaidModule, campaign, sPrePaidCode, sCampaignCode,
           sSiteGUID, dPrice, sCurrency, sCouponCode, sUserIP,
           sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty);
        }
        /// <summary>
        /// Get Custom Data For Subscription
        /// </summary>
        protected virtual string GetCustomDataForSubscription(TvinciPricing.Subscription theSub, TvinciPricing.Campaign campaign, string sSubscriptionCode, string sCampaignCode,
    string sSiteGUID, double dPrice, string sCurrency, string sCouponCode, string sUserIP,
    string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sOverrideEndDate, string sPreviewModuleID, bool previewEntitled)
        {


            bool bIsRecurring = theSub.m_bIsRecurring;


            Int32 nRecPeriods = theSub.m_nNumberOfRecPeriods;
            StringBuilder sb = new StringBuilder();
            sb.Append("<customdata type=\"sp\">");
            if (String.IsNullOrEmpty(sCountryCd) == false)
            {
                sb.AppendFormat("<lcc>{0}</lcc>", sCountryCd);
            }
            else
            {
                sb.AppendFormat("<lcc>{0}</lcc>", TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP));
            }
            if (String.IsNullOrEmpty(sLANGUAGE_CODE) == false)
            {
                sb.AppendFormat("<llc>{0}</llc>", sLANGUAGE_CODE);
            }
            if (String.IsNullOrEmpty(sDEVICE_NAME) == false)
            {
                sb.AppendFormat("<ldn>{0}</ldn>", sDEVICE_NAME);
            }
            sb.Append("<mnou>");
            if (theSub != null && theSub.m_oUsageModule != null)
            {
                sb.Append(theSub.m_oUsageModule.m_nMaxNumberOfViews.ToString());
            }
            sb.Append("</mnou>");
            sb.AppendFormat("<u id=\"{0}\"/>", sSiteGUID);
            sb.AppendFormat("<s>{0}</s>", sSubscriptionCode);
            sb.AppendFormat("<cc>{0}</cc>", sCouponCode);
            sb.AppendFormat("<campcode>{0}</campcode>", sCampaignCode);
            sb.Append("<cmnov>");
            if (campaign != null && campaign.m_usageModule != null)
            {
                sb.Append(campaign.m_usageModule.m_tsMaxUsageModuleLifeCycle.ToString());
            }
            sb.Append("</cmnov>");
            sb.Append("<cmumlc>");
            if (campaign != null && campaign.m_usageModule != null)
            {
                sb.Append(campaign.m_usageModule.m_tsMaxUsageModuleLifeCycle.ToString());
            }
            sb.Append("</cmumlc>");
            sb.Append("<oed>");
            sb.Append(sOverrideEndDate);
            sb.Append("</oed>");
            sb.AppendFormat("<p ir=\"{0}\" n=\"1\" o=\"{1}\"/>", /*bIsRecurring.ToString().ToLower()*/ GetIsRecurringStrForSubscriptionCustomData(bIsRecurring, sPreviewModuleID), nRecPeriods.ToString());
            sb.Append("<vlcs>");
            if (theSub != null && theSub.m_oUsageModule != null)
            {
                sb.Append(theSub.m_oUsageModule.m_tsViewLifeCycle.ToString());
            }
            sb.Append("</vlcs>");
            sb.Append("<mumlc>");
            if (theSub != null && theSub.m_oSubscriptionUsageModule != null)
                sb.Append(theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle.ToString());
            sb.Append("</mumlc>");
            sb.Append("<ppvm>");
            sb.Append("</ppvm>");
            sb.Append(String.Concat("<pm>", sPreviewModuleID, "</pm>"));
            sb.Append("<pc>");
            if (theSub != null && theSub.m_oSubscriptionPriceCode != null)
                sb.Append(theSub.m_oSubscriptionPriceCode.m_sCode);
            sb.Append("</pc>");
            sb.Append("<pri>");
            sb.Append(dPrice.ToString());
            sb.Append("</pri>");
            sb.Append("<cu>");
            sb.Append(sCurrency);
            sb.Append("</cu>");
            if (theSub != null && theSub.m_oPreviewModule != null && theSub.m_oPreviewModule.m_tsFullLifeCycle != null && previewEntitled)
            {
                sb.Append("<prevlc>");
                sb.Append(theSub.m_oPreviewModule.m_tsFullLifeCycle);
                sb.Append("</prevlc>");
            }
            sb.Append("</customdata>");
            return sb.ToString();

        }

        protected virtual string GetCustomDataForCollection(TvinciPricing.Collection theCol, string sCollectionCode,
    string sSiteGUID, double dPrice, string sCurrency, string sCouponCode, string sUserIP,
    string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sOverrideEndDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<customdata type=\"col\">");
            if (String.IsNullOrEmpty(sCountryCd) == false)
            {
                sb.AppendFormat("<lcc>{0}</lcc>", sCountryCd);
            }
            else
            {
                sb.AppendFormat("<lcc>{0}</lcc>", TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP));
            }
            if (String.IsNullOrEmpty(sLANGUAGE_CODE) == false)
            {
                sb.AppendFormat("<llc>{0}</llc>", sLANGUAGE_CODE);
            }
            if (String.IsNullOrEmpty(sDEVICE_NAME) == false)
            {
                sb.AppendFormat("<ldn>{0}</ldn>", sDEVICE_NAME);
            }
            sb.Append("<mnou>");
            if (theCol != null && theCol.m_oUsageModule != null)
            {
                sb.Append(theCol.m_oUsageModule.m_nMaxNumberOfViews.ToString());
            }
            sb.Append("</mnou>");
            sb.AppendFormat("<u id=\"{0}\"/>", sSiteGUID);
            sb.AppendFormat("<cID>{0}</cID>", sCollectionCode);
            sb.AppendFormat("<cc>{0}</cc>", sCouponCode);
            sb.Append("<oed>");
            sb.Append(sOverrideEndDate);
            sb.Append("</oed>");
            sb.Append("<vlcs>");
            if (theCol != null && theCol.m_oUsageModule != null)
            {
                sb.Append(theCol.m_oUsageModule.m_tsViewLifeCycle.ToString());
            }
            sb.Append("</vlcs>");
            sb.Append("<mumlc>");
            if (theCol != null && theCol.m_oCollectionUsageModule != null)
                sb.Append(theCol.m_oCollectionUsageModule.m_tsMaxUsageModuleLifeCycle.ToString());
            sb.Append("</mumlc>");
            sb.Append("<ppvm>");
            sb.Append("</ppvm>");
            sb.Append("<pc>");
            if (theCol != null && theCol.m_oCollectionPriceCode != null)
                sb.Append(theCol.m_oCollectionPriceCode.m_sCode);
            sb.Append("</pc>");
            sb.Append("<pri>");
            sb.Append(dPrice.ToString());
            sb.Append("</pri>");
            sb.Append("<cu>");
            sb.Append(sCurrency);
            sb.Append("</cu>");
            sb.Append("</customdata>");
            return sb.ToString();

        }

        protected virtual string GetCustomDataForSubscription(TvinciPricing.Subscription theSub, TvinciPricing.Campaign campaign, string sSubscriptionCode, string sCampaignCode,
   string sSiteGUID, double dPrice, string sCurrency, string sCouponCode, string sUserIP,
   string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {

            return GetCustomDataForSubscription(theSub, campaign, sSubscriptionCode, sCampaignCode,
           sSiteGUID, dPrice, sCurrency, sCouponCode, sUserIP,
           sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty, string.Empty, false);

        }

        public virtual PrePaidResponse PP_ChargeUserForMediaFile(string sSiteGUID, double dPrice, string sCurrency, Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return PP_BaseChargeUserForMediaFile(sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
        }
        /// <summary>
        /// PP Base Charge User For Media File
        /// </summary>
        protected PrePaidResponse PP_BaseChargeUserForMediaFile(string sSiteGUID, double dPrice, string sCurrency,
            Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {

            PrePaidResponse ret = new PrePaidResponse();
            ret.m_oStatus = PrePaidResponseStatus.UnKnown;
            ret.m_sStatusDescription = "";
            TvinciAPI.API apiWs = null;
            TvinciUsers.UsersService u = null;
            TvinciPricing.mdoule m = null;
            ODBCWrapper.InsertQuery insertQuery = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {

                if (string.IsNullOrEmpty(sSiteGUID))
                {
                    ret.m_oStatus = PrePaidResponseStatus.UnKnownUser;
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                    return ret;
                }
                else
                {
                    u = new ConditionalAccess.TvinciUsers.UsersService();

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }
                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = PrePaidResponseStatus.UnKnownUser;
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                        return ret;
                    }
                    else if (uObj != null && uObj.m_user != null && uObj.m_user.m_eSuspendState == TvinciUsers.DomainSuspentionStatus.Suspended)
                    {
                        ret.m_oStatus = PrePaidResponseStatus.UserSuspended;
                        ret.m_sStatusDescription = "Cannot charge a suspended user";
                        WriteToUserLog(sSiteGUID, "while trying to purchase media file id(PP): " + nMediaFileID + " error returned: " + ret.m_sStatusDescription);
                        return ret;
                    }
                    else
                    {
                        //Get User Valid PP
                        UserPrePaidContainer userPPs = new UserPrePaidContainer();
                        userPPs.Initialize(sSiteGUID, sCurrency);

                        sWSUserName = "";
                        sWSPass = "";

                        m = new global::ConditionalAccess.TvinciPricing.mdoule();
                        sWSURL = Utils.GetWSURL("pricing_ws");
                        if (!string.IsNullOrEmpty(sWSURL))
                        {
                            m.Url = sWSURL;
                        }
                        Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                        if (string.IsNullOrEmpty(sPPVModuleCode))
                        {
                            ret.m_oStatus = PrePaidResponseStatus.Fail;
                            ret.m_sStatusDescription = "Charge must have ppv module code";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }
                        // chack if ppvModule related to mediaFile 
                        long ppvModuleCode = 0;
                        long.TryParse(sPPVModuleCode, out ppvModuleCode);

                        TvinciPricing.PPVModule thePPVModule = m.ValidatePPVModuleForMediaFile(sWSUserName, sWSPass, nMediaFileID, ppvModuleCode);
                        if (thePPVModule == null)
                        {
                            ret.m_oStatus = PrePaidResponseStatus.Fail;
                            ret.m_sStatusDescription = "The ppv module is unknown";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }
                        else if (thePPVModule.m_sObjectCode != ppvModuleCode.ToString() && !string.IsNullOrEmpty(sPPVModuleCode))
                        {
                            ret.m_oStatus = PrePaidResponseStatus.UnKnownPPVModule;
                            ret.m_sStatusDescription = "This PPVModule does not belong to item";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }


                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.Subscription relevantSub = null;
                        TvinciPricing.Collection relevantCol = null;
                        TvinciPricing.PrePaidModule relevantPP = null;


                        TvinciPricing.Price p = Utils.GetMediaFileFinalPriceForNonGetItemsPrices(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (theReason == PriceReason.ForPurchase || (theReason == PriceReason.SubscriptionPurchased && p.m_dPrice > 0))
                        {
                            if ((p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency))
                            {
                                if (p.m_dPrice != 0)
                                {
                                    //Check For Credit
                                    if (p.m_dPrice <= userPPs.m_nTotalAmount - userPPs.m_nAmountUsed)
                                    {

                                        if (string.IsNullOrEmpty(sCountryCd) && !string.IsNullOrEmpty(sUserIP))
                                        {
                                            sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
                                        }

                                        //Create the Custom Data
                                        string sCustomData = GetCustomData(relevantSub, thePPVModule, null, sSiteGUID, dPrice, sCurrency,
                                            nMediaFileID, nMediaID, sPPVModuleCode, string.Empty, sCouponCode, sUserIP,
                                            sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                                        HandleCouponUses(relevantSub, sPPVModuleCode, sSiteGUID, p.m_dPrice, sCurrency, nMediaFileID, sCouponCode,
                                            sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, true, 0, 0);

                                        insertQuery = new ODBCWrapper.InsertQuery("ppv_purchases");
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                                        if (relevantSub != null)
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", relevantSub.m_sObjectCode);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrency);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", sCustomData);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", 0);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLANGUAGE_CODE);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDEVICE_NAME);
                                        if (thePPVModule != null &&
                                            thePPVModule.m_oUsageModule != null)
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", thePPVModule.m_oUsageModule.m_nMaxNumberOfViews);
                                        else
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);

                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                                        if (thePPVModule != null &&
                                            thePPVModule.m_oUsageModule != null)
                                        {
                                            DateTime d = Utils.GetEndDateTime(DateTime.UtcNow, thePPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);
                                        }
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_pp", "=", userPPs.m_oUserPPs[0].m_nPPModuleID);

                                        insertQuery.Execute();

                                        Int32 nPurchaseID = 0;
                                        selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                        selectQuery += " select id from ppv_purchases where ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                                        if (relevantSub != null)
                                        {
                                            selectQuery += "and";
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", relevantSub.m_sObjectCode);
                                        }
                                        selectQuery += "and";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                                        selectQuery += "and";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                        selectQuery += "and";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                                        selectQuery += "and";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrency);
                                        selectQuery += "and";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                        selectQuery += "and";
                                        if (thePPVModule != null &&
                                            thePPVModule.m_oUsageModule != null)
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", thePPVModule.m_oUsageModule.m_nMaxNumberOfViews);
                                        else
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);
                                        selectQuery += "and";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                        selectQuery += "and";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                                        selectQuery += "and";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_pp", "=", userPPs.m_oUserPPs[0].m_nPPModuleID);
                                        selectQuery += "order by id desc";
                                        if (selectQuery.Execute("query", true) != null)
                                        {
                                            Int32 nCount1 = selectQuery.Table("query").DefaultView.Count;
                                            if (nCount1 > 0)
                                                nPurchaseID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                                        }


                                        double sum = 0;
                                        double credit = (userPPs.m_nTotalAmount - userPPs.m_nAmountUsed);

                                        double dMaxAmount = 0.0;
                                        Int32 nRelPrePaidID = 0;

                                        foreach (UserPrePaidObject uppo in userPPs.m_oUserPPs)
                                        {
                                            double diff = p.m_dPrice - sum;
                                            if ((uppo.m_nTotalAmount - uppo.m_nAmountUsed) < diff)
                                            {
                                                diff = (uppo.m_nTotalAmount - uppo.m_nAmountUsed);
                                            }

                                            if (diff > dMaxAmount)
                                            {
                                                dMaxAmount = diff;
                                                nRelPrePaidID = uppo.m_nPPModuleID;
                                            }

                                            credit -= diff;
                                            UpdatePPPurchase(uppo.m_nPPPurchaseID, diff, uppo.m_nAmountUsed);
                                            InsertPPUsesRecord(nPurchaseID, nMediaFileID, BillingItemsType.PPV, sSiteGUID, sCurrency, uppo.m_nPPModuleID, uppo.m_nPPPurchaseID, diff, credit, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                                            sum += diff;

                                            if (sum == p.m_dPrice)
                                            {
                                                ret.m_oStatus = PrePaidResponseStatus.Success;

                                                updateQuery = new ODBCWrapper.UpdateQuery("ppv_purchases");
                                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_pp", "=", nRelPrePaidID);
                                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
                                                updateQuery += "where";
                                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nPurchaseID);
                                                updateQuery.Execute();

                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ret.m_oStatus = PrePaidResponseStatus.NoCredit;
                                    }

                                }
                                if (ret.m_oStatus == PrePaidResponseStatus.Success)
                                {
                                    WriteToUserLog(sSiteGUID, "Media file id: " + nMediaFileID.ToString() + " Purchased(PP): " + dPrice.ToString() + sCurrency);
                                    //send purchase mail
                                    string sEmail = "";
                                    string sPaymentMethod = "Pre Paid";
                                    string sDateOfPurchase = GetDateSTRByGroup(DateTime.UtcNow, m_nGroupID);
                                    string sItemName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", nMediaID, "MAIN_CONNECTION_STRING").ToString();

                                    TvinciAPI.PurchaseMailRequest sMailReq = GetPurchaseMailRequest(ref sEmail, sSiteGUID, sItemName, sPaymentMethod, sDateOfPurchase, string.Empty, dPrice, sCurrency, m_nGroupID);
                                    apiWs = new TvinciAPI.API();
                                    string sAPIWSUserName = string.Empty;
                                    string sAPIWSPass = string.Empty;
                                    Utils.GetWSCredentials(m_nGroupID, eWSModules.API, ref sWSUserName, ref sWSPass);

                                    string sAPIWSURL = Utils.GetWSURL("api_ws");
                                    if (!string.IsNullOrEmpty(sAPIWSURL))
                                    {
                                        apiWs.Url = sAPIWSURL;
                                    }
                                    apiWs.SendMailTemplate(sAPIWSUserName, sWSPass, sMailReq);
                                }
                                else
                                {
                                    ret.m_sStatusDescription = "No Credit";
                                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(PP): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                                }
                            }
                            else
                            {
                                ret.m_oStatus = PrePaidResponseStatus.PriceNotCorrect;
                                ret.m_sStatusDescription = "The price of the request is not the actual price";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(PP): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                        }
                        else
                        {
                            if (theReason == PriceReason.PPVPurchased)
                            {
                                ret.m_oStatus = PrePaidResponseStatus.Fail;
                                ret.m_sStatusDescription = "The media file is already purchased";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(PP): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                            else if (theReason == PriceReason.Free)
                            {
                                ret.m_oStatus = PrePaidResponseStatus.Fail;
                                ret.m_sStatusDescription = "The media file is free";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(PP): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                            else if (theReason == PriceReason.ForPurchaseSubscriptionOnly)
                            {
                                ret.m_oStatus = PrePaidResponseStatus.Fail;
                                ret.m_sStatusDescription = "The media file is for purchase with subscription only";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(PP): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                            else if (theReason == PriceReason.SubscriptionPurchased)
                            {
                                ret.m_oStatus = PrePaidResponseStatus.Fail;
                                ret.m_sStatusDescription = "The media file is already purchased (subscription)";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(PP): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                            else if (theReason == PriceReason.NotForPurchase)
                            {
                                ret.m_oStatus = PrePaidResponseStatus.Fail;
                                ret.m_sStatusDescription = "The media file is not valid for purchased";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(PP): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at PP_BaseChargeUserForMediaFile. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" M ID: ", nMediaID));
                sb.Append(String.Concat(" MF ID: ", nMediaFileID));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            finally
            {
                if (u != null)
                {
                    u.Dispose();
                }
                if (apiWs != null)
                {
                    apiWs.Dispose();
                }
                if (m != null)
                {
                    m.Dispose();
                }
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
            }
            return ret;
        }
        /// <summary>
        /// PP Charge User For Subscription
        /// </summary>
        public virtual PrePaidResponse PP_ChargeUserForSubscription(string sSiteGUID, double dPrice, string sCurrency, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParams,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return PP_BaseChargeUserForSubscription(sSiteGUID, dPrice, sCurrency, sSubscriptionCode, sCouponCode, sUserIP, sExtraParams, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
        }
        /// <summary>
        /// PP Base Charge User For Subscription
        /// </summary>
        protected PrePaidResponse PP_BaseChargeUserForSubscription(string sSiteGUID, double dPrice, string sCurrency, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParams,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            //string sCouponCode = "";
            PrePaidResponse ret = new PrePaidResponse();
            TvinciUsers.UsersService u = null;
            ODBCWrapper.UpdateQuery updateQuery1 = null;
            ODBCWrapper.InsertQuery insertQuery = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {
                if (string.IsNullOrEmpty(sSiteGUID))
                {
                    ret.m_oStatus = PrePaidResponseStatus.UnKnownUser;
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                }
                else
                {
                    u = new ConditionalAccess.TvinciUsers.UsersService();

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }
                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = PrePaidResponseStatus.UnKnownUser;
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                    }
                    else if (uObj != null && uObj.m_user != null && uObj.m_user.m_eSuspendState == TvinciUsers.DomainSuspentionStatus.Suspended)
                    {
                        ret.m_oStatus = PrePaidResponseStatus.UserSuspended;
                        ret.m_sStatusDescription = "Cannot charge a suspended user";
                        WriteToUserLog(sSiteGUID, "while trying to purchase subscription(PP): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription);
                    }
                    else
                    {
                        //Get User Valid PP
                        UserPrePaidContainer userPPs = new UserPrePaidContainer();
                        userPPs.Initialize(sSiteGUID, sCurrency);

                        //UserPrePaidObject relUppo = null; 

                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.Subscription theSub = null;
                        TvinciPricing.Price p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubscriptionCode, sSiteGUID, sCouponCode, ref theReason, ref theSub, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (p != null)
                        {
                            dPrice = p.m_dPrice;
                            sCurrency = p.m_oCurrency.m_sCurrencyCD3;
                        }
                        if (theReason == PriceReason.ForPurchase)
                        {
                            if ((p != null && p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency))
                            {
                                if (p.m_dPrice != 0)
                                {
                                    bool bIsRecurring = theSub.m_bIsRecurring;
                                    Int32 nRecPeriods = theSub.m_nNumberOfRecPeriods;

                                    //Check For Credit
                                    if (p.m_dPrice <= userPPs.m_nTotalAmount - userPPs.m_nAmountUsed)
                                    {

                                        if (string.IsNullOrEmpty(sCountryCd) && !string.IsNullOrEmpty(sUserIP))
                                        {
                                            sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
                                        }

                                        HandleCouponUses(theSub, string.Empty, sSiteGUID, dPrice, sCurrency, 0, sCouponCode, sUserIP,
                                            sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, true, 0, 0);

                                        updateQuery1 = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                                        updateQuery1 += " where ";
                                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                                        updateQuery1 += " and ";
                                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                                        updateQuery1 += " and ";
                                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                        updateQuery1.Execute();

                                        insertQuery = new ODBCWrapper.InsertQuery("subscriptions_purchases");
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrency);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", string.Empty);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLANGUAGE_CODE);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDEVICE_NAME);
                                        if (theSub != null &&
                                            theSub.m_oUsageModule != null)
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", theSub.m_oUsageModule.m_nMaxNumberOfViews);
                                        else
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);

                                        if (theSub != null &&
                                            theSub.m_oUsageModule != null)
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", theSub.m_oUsageModule.m_tsViewLifeCycle);
                                        else
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", 0);

                                        if (bIsRecurring == true)
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 1);
                                        else
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", 0);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                                        if (theSub != null &&
                                            theSub.m_oSubscriptionUsageModule != null)
                                        {
                                            DateTime d = Utils.GetEndDateTime(DateTime.UtcNow, theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle);
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);

                                        }
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_pp", "=", userPPs.m_oUserPPs[0].m_nPPModuleID);
                                        insertQuery.Execute();


                                        Int32 nPurchaseID = 0;

                                        selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                        selectQuery += " select id from subscriptions_purchases where ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrency);
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                        selectQuery += " and ";
                                        if (theSub != null &&
                                            theSub.m_oUsageModule != null)
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", theSub.m_oUsageModule.m_nMaxNumberOfViews);
                                        else
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);
                                        selectQuery += " and ";
                                        if (theSub != null &&
                                            theSub.m_oUsageModule != null)
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", theSub.m_oUsageModule.m_tsViewLifeCycle);
                                        else
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", 0);

                                        selectQuery += " and ";
                                        if (bIsRecurring == true)
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 1);
                                        else
                                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_pp", "=", userPPs.m_oUserPPs[0].m_nPPModuleID);
                                        selectQuery += " order by id desc";
                                        if (selectQuery.Execute("query", true) != null)
                                        {
                                            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                                            if (nCount > 0)
                                            {
                                                nPurchaseID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                                            }
                                        }

                                        double sum = 0;
                                        double credit = (userPPs.m_nTotalAmount - userPPs.m_nAmountUsed);

                                        double dMaxAmount = 0.0;
                                        Int32 nRelPrePaidID = 0;

                                        foreach (UserPrePaidObject uppo in userPPs.m_oUserPPs)
                                        {

                                            double diff = p.m_dPrice - sum;
                                            if ((uppo.m_nTotalAmount - uppo.m_nAmountUsed) < diff)
                                            {
                                                diff = (uppo.m_nTotalAmount - uppo.m_nAmountUsed);
                                            }

                                            if (diff > dMaxAmount)
                                            {
                                                dMaxAmount = diff;
                                                nRelPrePaidID = uppo.m_nPPModuleID;
                                            }

                                            credit -= diff;
                                            UpdatePPPurchase(uppo.m_nPPPurchaseID, diff, uppo.m_nAmountUsed);
                                            InsertPPUsesRecord(nPurchaseID, int.Parse(sSubscriptionCode), BillingItemsType.Subscription, sSiteGUID, sCurrency, uppo.m_nPPModuleID, uppo.m_nPPPurchaseID, diff, credit, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                                            sum += diff;

                                            if (sum == p.m_dPrice)
                                            {
                                                ret.m_oStatus = PrePaidResponseStatus.Success;

                                                updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_pp", "=", nRelPrePaidID);
                                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
                                                updateQuery += "where";
                                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nPurchaseID);
                                                updateQuery.Execute();

                                                break;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        ret.m_oStatus = PrePaidResponseStatus.NoCredit;
                                    }
                                }
                                if (ret.m_oStatus == PrePaidResponseStatus.Success)
                                {
                                    WriteToUserLog(sSiteGUID, "Subscription purchase (PP): " + sSubscriptionCode);
                                }
                                else
                                {
                                    ret.m_sStatusDescription = "No Credit";
                                    WriteToUserLog(sSiteGUID, "while trying to purchase subscription(PP): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription);
                                }
                            }
                            else
                            {
                                ret.m_oStatus = PrePaidResponseStatus.PriceNotCorrect;
                                ret.m_sStatusDescription = "The price of the request is not the actual price";
                                WriteToUserLog(sSiteGUID, "while trying to purchase subscription(PP): " + " error returned: " + ret.m_sStatusDescription);
                            }
                        }
                        else
                        {
                            if (theReason == PriceReason.Free)
                            {
                                ret.m_oStatus = PrePaidResponseStatus.Fail;
                                ret.m_sStatusDescription = "The subscription is free";
                                WriteToUserLog(sSiteGUID, "while trying to purchase subscription(PP): " + " error returned: " + ret.m_sStatusDescription);
                            }
                            if (theReason == PriceReason.SubscriptionPurchased)
                            {
                                ret.m_oStatus = PrePaidResponseStatus.Fail;
                                ret.m_sStatusDescription = "The subscription is already purchased";
                                WriteToUserLog(sSiteGUID, "while trying to purchase subscription(PP): " + " error returned: " + ret.m_sStatusDescription);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at PP_BaseChargeUserForSubscription. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" Currency: ", sCurrency));
                sb.Append(String.Concat(" Sub Cd: ", sSubscriptionCode));
                sb.Append(String.Concat(" User IP: ", sUserIP));
                sb.Append(String.Concat(" Extra Params: ", sExtraParams));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                }
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
                if (updateQuery1 != null)
                {
                    updateQuery1.Finish();
                }
                #endregion
            }
            return ret;
        }
        /// <summary>
        /// Get User Pre Paid Status
        /// </summary>
        public virtual UserPrePaidContainer GetUserPrePaidStatus(string sSiteGUID, string sCurrency)
        {
            UserPrePaidContainer UserPPs = new UserPrePaidContainer();
            UserPPs.Initialize(sSiteGUID, sCurrency);
            return UserPPs;
        }
        /// <summary>
        /// Update PP Purchase
        /// </summary>
        private void UpdatePPPurchase(Int32 nPPPurchaseID, double nAmount, double nUsed)
        {
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {
                updateQuery = new ODBCWrapper.UpdateQuery("pre_paid_purchases");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("amount_used", "=", nUsed + nAmount);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nPPPurchaseID);
                updateQuery.Execute();
            }
            finally
            {
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
            }

        }
        /// <summary>
        /// Insert PP Uses Record
        /// </summary>
        private void InsertPPUsesRecord(Int32 nPurchaseID, Int32 nItemID, BillingItemsType eItemType, string sSiteGUID, string sCurrency, Int32 nPPCD, Int32 nPPPurchaseID,
            double dPrice, double dCredit,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            ODBCWrapper.InsertQuery insertQuery = null;
            try
            {
                insertQuery = new ODBCWrapper.InsertQuery("pre_paid_uses");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);

                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ITEM_ID", "=", nItemID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ITEM_TYPE", "=", (int)eItemType);

                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrency);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLANGUAGE_CODE);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", "=", sDEVICE_NAME);

                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PP_CD", "=", nPPCD);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PP_PURCHASE_ID", "=", nPPPurchaseID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("REMAINS_CREDIT", "=", dCredit);

                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);

                insertQuery.Execute();
            }
            finally
            {
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
            }

        }
        /// <summary>
        /// Get User PrePaidS History
        /// </summary>
        public virtual PrePaidHistoryResponse GetUserPrePaidSHistory(string sSiteGUID, Int32 nNumberOfItems)
        {
            //Get User Valid PP
            UserPrePaidContainer userPPs = new UserPrePaidContainer();
            userPPs.Initialize(sSiteGUID, string.Empty);

            double dLastCredit = (userPPs.m_nTotalAmount - userPPs.m_nAmountUsed);
            DateTime dLastDate = DateTime.UtcNow;

            PrePaidHistoryResponse theResp = new PrePaidHistoryResponse();


            List<PrePaidHistoryContainer> items = new List<PrePaidHistoryContainer>();
            PrePaidHistoryContainer pphc = null;


            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select top " + nNumberOfItems + " item_id, item_type, currency_cd, SUM(price) as price, min(remains_credit) as remains_credit, MIN(create_date) as date, purchase_id from pre_paid_uses with (nolock) ";
            selectQuery += "where is_active=1 and status=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            selectQuery += "group by item_id, item_type, currency_cd, purchase_id order by date desc";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;

                for (int i = 0; i < nCount; i++)
                {
                    Int32 nItemID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "item_id", i);
                    Int32 nItemType = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "item_type", i);
                    string sCurrency = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "currency_cd", i);
                    double dPrice = ODBCWrapper.Utils.GetDoubleSafeVal(selectQuery, "price", i);
                    double dCredit = ODBCWrapper.Utils.GetDoubleSafeVal(selectQuery, "remains_credit", i);
                    DateTime dDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "date", i);

                    if (dLastCredit != dCredit)
                    {
                        ODBCWrapper.DataSetSelectQuery selectQueryE = new ODBCWrapper.DataSetSelectQuery();
                        selectQueryE += "select * from pre_paid_purchases with (nolock) where is_active=1 and status=1 and";
                        selectQueryE += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                        selectQueryE += " and ";
                        selectQueryE += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                        selectQueryE += " and ";
                        selectQueryE += ODBCWrapper.Parameter.NEW_PARAM("end_date", "<", dLastDate);
                        selectQueryE += " and ";
                        selectQueryE += ODBCWrapper.Parameter.NEW_PARAM("end_date", ">", dDate);
                        selectQueryE += "and total_amount>amount_used order by end_date desc";
                        if (selectQueryE.Execute("query", true) != null)
                        {
                            Int32 nCount1 = selectQueryE.Table("query").DefaultView.Count;
                            for (int j = 0; j < nCount1; j++)
                            {
                                if (items.Count == nNumberOfItems)
                                    break;

                                Int32 nPPID = ODBCWrapper.Utils.GetIntSafeVal(selectQueryE, "pre_paid_module_id", j);
                                double dlostAmount = ODBCWrapper.Utils.GetDoubleSafeVal(selectQueryE, "total_amount", j) - ODBCWrapper.Utils.GetDoubleSafeVal(selectQueryE, "amount_used", j);
                                DateTime dExpired = ODBCWrapper.Utils.GetDateSafeVal(selectQueryE, "end_date", j);

                                pphc = new PrePaidHistoryContainer();

                                pphc.m_oPrice = new ConditionalAccess.TvinciPricing.Price();
                                pphc.m_oPrice.m_dPrice = dlostAmount;
                                pphc.m_oPrice.m_oCurrency = new ConditionalAccess.TvinciPricing.Currency();
                                pphc.m_oPrice.m_oCurrency.m_sCurrencyCD2 = sCurrency;

                                pphc.m_oCredit = new ConditionalAccess.TvinciPricing.Price();
                                pphc.m_oCredit.m_dPrice = dLastCredit;
                                pphc.m_oCredit.m_oCurrency = new ConditionalAccess.TvinciPricing.Currency();
                                pphc.m_oCredit.m_oCurrency.m_sCurrencyCD2 = sCurrency;

                                pphc.m_dtActionDate = dExpired;

                                pphc.m_eItemType = BillingItemsType.PrePaidExpired;

                                TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule();
                                string pricingURL = Utils.GetWSURL("pricing_ws");
                                if (!string.IsNullOrEmpty(pricingURL))
                                {
                                    m.Url = pricingURL;
                                }

                                TvinciPricing.PrePaidModule thePrePaid = null;

                                string sWSUserName = string.Empty;
                                string sWSPass = string.Empty;
                                Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                                thePrePaid = m.GetPrePaidModuleData(sWSUserName, sWSPass, nPPID, string.Empty, string.Empty, string.Empty);

                                pphc.m_sPurchasedItemCode = nPPID.ToString();
                                pphc.m_sPurchasedItemName = thePrePaid.m_Title;

                                items.Add(pphc);

                                dLastCredit += dlostAmount;

                            }
                        }
                        selectQueryE.Finish();
                        selectQueryE = null;

                    }

                    if (items.Count == nNumberOfItems)
                        break;

                    pphc = new PrePaidHistoryContainer();

                    pphc.m_oPrice = new ConditionalAccess.TvinciPricing.Price();
                    pphc.m_oPrice.m_dPrice = dPrice;
                    pphc.m_oPrice.m_oCurrency = new ConditionalAccess.TvinciPricing.Currency();
                    pphc.m_oPrice.m_oCurrency.m_sCurrencyCD2 = sCurrency;

                    pphc.m_oCredit = new ConditionalAccess.TvinciPricing.Price();
                    pphc.m_oCredit.m_dPrice = dCredit;
                    pphc.m_oCredit.m_oCurrency = new ConditionalAccess.TvinciPricing.Currency();
                    pphc.m_oCredit.m_oCurrency.m_sCurrencyCD2 = sCurrency;

                    pphc.m_dtActionDate = dDate;
                    dLastDate = dDate;


                    pphc.m_eItemType = (BillingItemsType)nItemType;

                    //Case PPV
                    if (pphc.m_eItemType == BillingItemsType.PPV)
                    {

                        Int32 nMediaID = Utils.GetMediaIDFromFileID(nItemID, m_nGroupID);
                        if (nMediaID != 0)
                        {
                            pphc.m_sPurchasedItemName = GetMediaTitle(nMediaID);
                            pphc.m_sPurchasedItemCode = nMediaID.ToString();
                        }
                    }
                    //Case Subscription
                    else if (pphc.m_eItemType == BillingItemsType.Subscription)
                    {
                        pphc.m_eItemType = BillingItemsType.Subscription;

                        TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule();
                        string pricingURL = Utils.GetWSURL("pricing_ws");
                        if (!string.IsNullOrEmpty(pricingURL))
                        {
                            m.Url = pricingURL;
                        }
                        TvinciPricing.Subscription theSub = null;

                        string sWSUserName = string.Empty;
                        string sWSPass = string.Empty;
                        Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                        theSub = m.GetSubscriptionData(sWSUserName, sWSPass, nItemID.ToString(), "", "", "", true);
                        string sMainLang = "";
                        string sMainLangCode = "";
                        GetMainLang(ref sMainLang, ref sMainLangCode, m_nGroupID);
                        if (theSub.m_sName != null)
                        {
                            Int32 nNameLangLength = theSub.m_sName.Length;
                            for (int j = 0; j < nNameLangLength; j++)
                            {
                                string sLang = theSub.m_sName[j].m_sLanguageCode3;
                                string sVal = theSub.m_sName[j].m_sValue;
                                if (sLang == sMainLangCode)
                                    pphc.m_sPurchasedItemName = sVal;
                            }
                        }
                        pphc.m_sPurchasedItemCode = nItemID.ToString();
                    }
                    //Case Pre Paid
                    else if (pphc.m_eItemType == BillingItemsType.PrePaid)
                    {
                        TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule();
                        string pricingURL = Utils.GetWSURL("pricing_ws");
                        if (!string.IsNullOrEmpty(pricingURL))
                        {
                            m.Url = pricingURL;
                        }
                        TvinciPricing.PrePaidModule thePrePaid = null;

                        string sWSUserName = string.Empty;
                        string sWSPass = string.Empty;
                        Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                        thePrePaid = m.GetPrePaidModuleData(sWSUserName, sWSPass, nItemID, string.Empty, string.Empty, string.Empty);
                        pphc.m_sPurchasedItemCode = nItemID.ToString();
                        pphc.m_sPurchasedItemName = thePrePaid.m_Title;

                        pphc.m_oPrice.m_dPrice = -dPrice;
                    }

                    dLastCredit += pphc.m_oPrice.m_dPrice;
                    items.Add(pphc);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            if (items.Count < nNumberOfItems)
            {
                nNumberOfItems = items.Count;
            }


            theResp.m_nTransactionsCount = nNumberOfItems;
            theResp.m_Transactions = new PrePaidHistoryContainer[nNumberOfItems];
            for (int i = 0; i < nNumberOfItems; i++)
            {
                theResp.m_Transactions[i] = new PrePaidHistoryContainer();
                theResp.m_Transactions[i] = items[i];
            }

            return theResp;
        }
        /// <summary>
        /// Get Item Left View Life Cycle
        /// </summary>
        public virtual string GetItemLeftViewLifeCycle(string sMediaFileID, string sSiteGUID, bool bIsCoGuid,
            string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            string strResponse = TimeSpan.Zero.ToString();

            EntitlementResponse objItemLeftLifeCycle = this.GetEntitlement(sMediaFileID, sSiteGUID, bIsCoGuid, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);

            if (objItemLeftLifeCycle != null)
            {
                strResponse = objItemLeftLifeCycle.ViewLifeCycle;
            }

            return (strResponse);
        }

        /// <summary>
        /// Gets the time-spans of what's left for this specific item's life cycle (both full and view)
        /// </summary>
        /// <param name="p_sMediaFileID"></param>
        /// <param name="p_sSiteGUID"></param>
        /// <param name="p_bIsCoGuid"></param>
        /// <param name="p_sCOUNTRY_CODE"></param>
        /// <param name="p_sLANGUAGE_CODE"></param>
        /// <param name="p_sDEVICE_NAME"></param>
        /// <returns></returns>
        public EntitlementResponse GetEntitlement(
            string p_sMediaFileID, string p_sSiteGUID, bool p_bIsCoGuid, string p_sCOUNTRY_CODE, string p_sLANGUAGE_CODE, string p_sDEVICE_NAME)
        {
            EntitlementResponse objResponse = new EntitlementResponse();

            int nMediaFileID = 0;
            string strViewLifeCycle = TimeSpan.Zero.ToString();
            string strFullLifeCycle = TimeSpan.Zero.ToString();
            bool bIsOfflinePlayback = false;

            try
            {
                if (p_bIsCoGuid)
                {
                    if (!Utils.GetMediaFileIDByCoGuid(p_sMediaFileID, m_nGroupID, p_sSiteGUID, ref nMediaFileID))
                    {
                        throw new Exception("Failed to retrieve Media File ID from WS Catalog.");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(p_sMediaFileID) || !Int32.TryParse(p_sMediaFileID, out nMediaFileID))
                    {
                        throw new ArgumentException(String.Concat("MediaFileID is in incorrect format: ", p_sMediaFileID));
                    }
                }

                if (nMediaFileID > 0)
                {
                    int[] arrMediaFileIDs = { nMediaFileID };
                    MediaFileItemPricesContainer[] arrPrices =
                        GetItemsPrices(arrMediaFileIDs, p_sSiteGUID, string.Empty, true, p_sCOUNTRY_CODE, p_sLANGUAGE_CODE, p_sDEVICE_NAME);

                    if (arrPrices != null && arrPrices.Length > 0)
                    {
                        MediaFileItemPricesContainer objPrice = arrPrices[0];

                        // If the item is free
                        if (IsFreeItem(objPrice))
                        {
                            GetFreeItemLeftLifeCycle(ref strViewLifeCycle, ref strFullLifeCycle);
                        }
                        else if (!IsUserSuspended(objPrice))
                        // Item is not free and also not user is not suspended
                        {
                            bool bIsOfflineStatus = false;
                            string sPPVMCode = string.Empty;
                            int nViewLifeCycle = 0;
                            int nFullLifeCycle = 0;
                            DateTime dtViewDate = new DateTime();
                            DateTime dtNow = DateTime.UtcNow;
                            List<int> lstUsersIds = Utils.GetAllUsersDomainBySiteGUID(p_sSiteGUID, m_nGroupID);
                            List<int> lstRelatedMediaFiles = GetRelatedMediaFiles(objPrice, nMediaFileID);
                            DateTime? dtEntitlementStartDate = GetStartDate(objPrice);
                            DateTime? dtEntitlementEndDate = GetEndDate(objPrice);

                            string sPricingUsername = string.Empty;
                            string sPricingPassword = string.Empty;

                            Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sPricingUsername, ref sPricingPassword);

                            // Get latest use (watch/download) of the media file. If there was one, continue.
                            if (ConditionalAccessDAL.Get_LatestMediaFilesUse(lstUsersIds, lstRelatedMediaFiles, ref sPPVMCode, ref bIsOfflineStatus, ref dtNow,
                                ref dtViewDate))
                            {
                                if (bIsOfflineStatus)
                                {
                                    string sGroupUsageModuleCode = string.Empty;

                                    if (PricingDAL.Get_GroupUsageModuleCode(m_nGroupID, "PRICING_CONNECTION", ref sGroupUsageModuleCode))
                                    {
                                        UsageModule objUsageModule = Utils.GetUsageModuleDataWithCaching(sGroupUsageModuleCode, sPricingUsername, sPricingPassword,
                                            p_sCOUNTRY_CODE, p_sLANGUAGE_CODE, p_sDEVICE_NAME, m_nGroupID, "GetOfflineUsageModuleData");

                                        if (objUsageModule != null)
                                        {
                                            nViewLifeCycle = objUsageModule.m_tsViewLifeCycle;
                                            nFullLifeCycle = objUsageModule.m_tsMaxUsageModuleLifeCycle;
                                            bIsOfflinePlayback = objUsageModule.m_bIsOfflinePlayBack;
                                        }
                                    }
                                }
                                else
                                {
                                    bool bIsSuccess = GetLifeCycleByPPVMCode(p_sCOUNTRY_CODE, p_sLANGUAGE_CODE, p_sDEVICE_NAME, ref bIsOfflinePlayback, sPPVMCode,
                                        ref nViewLifeCycle, ref nFullLifeCycle, sPricingUsername, sPricingPassword);

                                    // If getting didn't succeed for any reason, write to log
                                    if (!bIsSuccess)
                                    {
                                        log.Error("Error - " + GetPricingErrLogMsg(sPPVMCode, p_sSiteGUID, p_sMediaFileID, p_bIsCoGuid,
                                            p_sCOUNTRY_CODE, p_sLANGUAGE_CODE, p_sDEVICE_NAME, eTransactionType.PPV));
                                    }
                                }
                            }

                            // If we found the view cycle (and there was a view), calculate what's left of it
                            // Base date is the view date
                            if (nViewLifeCycle > 0)
                            {
                                DateTime dtViewEndDate = Utils.GetEndDateTime(dtViewDate, nViewLifeCycle);
                                TimeSpan tsViewLeftSpan = dtViewEndDate.Subtract(dtNow);
                                strViewLifeCycle = tsViewLeftSpan.ToString();
                            }

                            //// In case user purchased the item but didn't view it - we need to find what is the usage module's full life cycle
                            //if (nFullLifeCycle == 0 && dtStartDate.HasValue)
                            //{
                            //    string sPPVMCodeFromPrice = GetPPVModuleCode(objPrice);

                            //    bool bIsSuccess = GetLifeCycleByPPVMCode(p_sCOUNTRY_CODE, p_sLANGUAGE_CODE, p_sDEVICE_NAME, ref bIsOfflinePlayback, sPPVMCodeFromPrice,
                            //        ref nViewLifeCycle, ref nFullLifeCycle, sPricingUsername, sPricingPassword);

                            //    // If getting didn't succeed for any reason, write to log
                            //    if (!bIsSuccess)
                            //    {
                            //            p_sCOUNTRY_CODE, p_sLANGUAGE_CODE, p_sDEVICE_NAME, eTransactionType.PPV), GetLogFilename());
                            //    }
                            //}

                            eTransactionType eBusinessModuleType = GetBusinessModuleType(sPPVMCode);

                            // If it is a subscription, use the end date that is saved in the DB and that was gotten in GetItemPrice
                            if (eBusinessModuleType == eTransactionType.Subscription || eBusinessModuleType == eTransactionType.Collection)
                            {
                                if (dtEntitlementEndDate.HasValue)
                                {
                                    TimeSpan tsFullLeftSpan = dtEntitlementEndDate.Value.Subtract(dtNow);
                                    strFullLifeCycle = tsFullLeftSpan.ToString();
                                }
                            }
                            else if (eBusinessModuleType == eTransactionType.PPV)
                            {
                                // If we found the full cycle, meaning the user purchased the media file, calculate what's left of it
                                // Base date is purchase date
                                if (nFullLifeCycle > 0 && dtEntitlementStartDate.HasValue)
                                {
                                    DateTime dtSubscriptionEndDate = Utils.GetEndDateTime(dtEntitlementStartDate.Value, nFullLifeCycle);
                                    TimeSpan tsFullLeftSpan = dtSubscriptionEndDate.Subtract(dtNow);
                                    strFullLifeCycle = tsFullLeftSpan.ToString();
                                }
                            }
                        }
                    }
                } // end if nMediaFileID > 0
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetItemLeftLifeCycle. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" MF ID or CG: ", p_sMediaFileID));
                sb.Append(String.Concat(" Is CG: ", p_bIsCoGuid.ToString().ToLower()));
                sb.Append(String.Concat(" Site Guid: ", p_sSiteGUID));
                sb.Append(String.Concat(" Country Cd: ", p_sCOUNTRY_CODE));
                sb.Append(String.Concat(" Lng Cd: ", p_sLANGUAGE_CODE));
                sb.Append(String.Concat(" Device Name: ", p_sDEVICE_NAME));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }

            objResponse.ViewLifeCycle = strViewLifeCycle;
            objResponse.FullLifeCycle = strFullLifeCycle;
            objResponse.IsOfflinePlayBack = bIsOfflinePlayback;

            return (objResponse);
        }

        /// <summary>
        /// Returns the default timespans of free items
        /// </summary>
        /// <param name="p_strViewLifeCycle"></param>
        /// <param name="p_strFullLifeCycle"></param>
        private void GetFreeItemLeftLifeCycle(ref string p_strViewLifeCycle, ref string p_strFullLifeCycle)
        {
            // Default is 2 days
            TimeSpan ts = new TimeSpan(2, 0, 0, 0);

            // Get the group's configuration for free view life cycle
            string sFreeLeftView = Utils.GetValueFromConfig(string.Format("free_left_view_{0}", m_nGroupID));

            if (!string.IsNullOrEmpty(sFreeLeftView))
            {
                DateTime dEndDate = Utils.GetEndDateTime(DateTime.UtcNow, int.Parse(sFreeLeftView), true);
                ts = dEndDate.Subtract(DateTime.UtcNow);
            }

            p_strViewLifeCycle = ts.ToString();
            // TODO: Understand what to do with full life cycle of free item. Right now I write it the same as view
            p_strFullLifeCycle = ts.ToString();
        }

        /// <summary>
        /// For a given PPVMCode, returns the full life cycle, view life cycle and is offline playback
        /// </summary>
        /// <param name="p_sCOUNTRY_CODE"></param>
        /// <param name="p_sLANGUAGE_CODE"></param>
        /// <param name="p_sDEVICE_NAME"></param>
        /// <param name="p_bIsOfflinePlayback"></param>
        /// <param name="p_sPPVMCode"></param>
        /// <param name="p_nViewLifeCycle"></param>
        /// <param name="p_nFullLifeCycle"></param>
        /// <param name="p_sPricingUsername"></param>
        /// <param name="p_sPricingPassword"></param>
        /// <returns>If the get succeeded or not</returns>
        private bool GetLifeCycleByPPVMCode(string p_sCOUNTRY_CODE, string p_sLANGUAGE_CODE, string p_sDEVICE_NAME, ref bool p_bIsOfflinePlayback,
            string p_sPPVMCode, ref int p_nViewLifeCycle, ref int p_nFullLifeCycle, string p_sPricingUsername, string p_sPricingPassword)
        {
            bool bIsSuccess = true;

            eTransactionType eBusinessModuleType = GetBusinessModuleType(p_sPPVMCode);

            switch (eBusinessModuleType)
            {
                case eTransactionType.Subscription:
                    {
                        // Get the code itself, without the prefix
                        string sSubCode = p_sPPVMCode.Substring(3);

                        // Get the subscription item of this code
                        Subscription[] arrSubscriptions =
                            Utils.GetSubscriptionsDataWithCaching(new List<string>(1) { sSubCode }, p_sPricingUsername, p_sPricingPassword, m_nGroupID);

                        // If there is a valid subscription with a valid usage module
                        if (arrSubscriptions != null && arrSubscriptions.Length > 0 && arrSubscriptions[0] != null &&
                            arrSubscriptions[0].m_oSubscriptionUsageModule != null)
                        {
                            p_nViewLifeCycle = arrSubscriptions[0].m_oSubscriptionUsageModule.m_tsViewLifeCycle;
                            p_nFullLifeCycle = arrSubscriptions[0].m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle;
                            p_bIsOfflinePlayback = arrSubscriptions[0].m_oSubscriptionUsageModule.m_bIsOfflinePlayBack;
                        }
                        else
                        {
                            bIsSuccess = false;
                        }
                        break;
                    }
                case eTransactionType.Collection:
                    {
                        // Get the code itself, without the prefix
                        string sCollCode = p_sPPVMCode.Substring(3);

                        // Get the collection item of this code
                        Collection[] arrCollections =
                            Utils.GetCollectionsDataWithCaching(new List<string>(1) { sCollCode }, p_sPricingUsername, p_sPricingPassword, m_nGroupID);

                        // If there is a valid collection with a valid usage module
                        if (arrCollections != null && arrCollections.Length > 0 && arrCollections[0] != null &&
                            arrCollections[0].m_oCollectionUsageModule != null)
                        {
                            p_nViewLifeCycle = arrCollections[0].m_oCollectionUsageModule.m_tsViewLifeCycle;
                            p_nFullLifeCycle = arrCollections[0].m_oCollectionUsageModule.m_tsMaxUsageModuleLifeCycle;
                            p_bIsOfflinePlayback = arrCollections[0].m_oCollectionUsageModule.m_bIsOfflinePlayBack;
                        }
                        else
                        {
                            bIsSuccess = false;
                        }
                        break;
                    }
                case eTransactionType.PPV:
                    {
                        PPVModule objPPV = Utils.GetPPVModuleDataWithCaching(p_sPPVMCode, p_sPricingUsername, p_sPricingPassword, m_nGroupID,
                            p_sCOUNTRY_CODE, p_sLANGUAGE_CODE, p_sDEVICE_NAME);

                        if (objPPV != null && objPPV.m_oUsageModule != null)
                        {
                            p_nViewLifeCycle = objPPV.m_oUsageModule.m_tsViewLifeCycle;
                            p_nFullLifeCycle = objPPV.m_oUsageModule.m_tsMaxUsageModuleLifeCycle;
                            p_bIsOfflinePlayback = objPPV.m_oUsageModule.m_bIsOfflinePlayBack;
                        }
                        else
                        {
                            bIsSuccess = false;
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            return (bIsSuccess);
        }

        /// <summary>
        /// Returns the ppv module code of the price container, if it has one
        /// </summary>
        /// <param name="p_objPrice"></param>
        /// <returns></returns>
        private string GetPPVModuleCode(MediaFileItemPricesContainer p_objPrice)
        {
            string strCode = string.Empty;

            if (p_objPrice != null && p_objPrice.m_oItemPrices != null && p_objPrice.m_oItemPrices.Length > 0)
            {
                strCode = p_objPrice.m_oItemPrices[0].m_sPPVModuleCode;
            }

            return (strCode);
        }

        /// <summary>
        /// Returns the start date of the price container, if it has one
        /// </summary>
        /// <param name="p_objPrice"></param>
        /// <returns></returns>
        private DateTime? GetStartDate(MediaFileItemPricesContainer p_objPrice)
        {
            DateTime? dtStartDate = null;

            if (p_objPrice != null && p_objPrice.m_oItemPrices != null && p_objPrice.m_oItemPrices.Length > 0)
            {
                dtStartDate = p_objPrice.m_oItemPrices[0].m_dtStartDate;
            }

            return (dtStartDate);
        }

        /// <summary>
        /// Returns the end date of the price container, if it has one
        /// </summary>
        /// <param name="p_objPrice"></param>
        /// <returns></returns>
        private DateTime? GetEndDate(MediaFileItemPricesContainer p_objPrice)
        {
            DateTime? dtEndDate = null;

            if (p_objPrice != null && p_objPrice.m_oItemPrices != null && p_objPrice.m_oItemPrices.Length > 0)
            {
                dtEndDate = p_objPrice.m_oItemPrices[0].m_dtEndDate;
            }

            return (dtEndDate);
        }

        private string GetPricingErrLogMsg(string businessModuleCode, string siteGuid, string mediaFileIDStr,
            bool isCoGuid, string countryCd, string langCode, string deviceName, eTransactionType businessModuleType)
        {
            StringBuilder sb = new StringBuilder("Failed to retrieve business module code from WS Pricing at GetItemLeftViewLifeCycle. ");
            sb.Append(String.Concat(" BM Cd: ", businessModuleCode));
            sb.Append(String.Concat(" BM Type: ", businessModuleType.ToString().ToLower()));
            sb.Append(String.Concat(" SG: ", siteGuid));
            sb.Append(String.Concat(" MF: ", mediaFileIDStr));
            sb.Append(String.Concat(" Is CG: ", isCoGuid.ToString().ToLower()));
            sb.Append(String.Concat(" Country Cd: ", countryCd));
            sb.Append(String.Concat(" Lng Cd: ", langCode));
            sb.Append(String.Concat(" Device Name: ", deviceName));

            return sb.ToString();

        }

        private eTransactionType GetBusinessModuleType(string sPPVModuleCode)
        {
            if (!string.IsNullOrEmpty(sPPVModuleCode))
            {
                if (sPPVModuleCode.Contains("s:"))
                    return eTransactionType.Subscription;
                if (sPPVModuleCode.Contains("c:"))
                    return eTransactionType.Collection;
            }

            return eTransactionType.PPV;
        }

        private bool IsSkipOnFirstUsageModule(int nIndexOfUsageModule, bool bIsUsageModuleIsRenewable, int nTotalNumOfPayments, int nNumOfPayments, bool bIsPurchasedWithPreviewModule)
        {
            if (bIsPurchasedWithPreviewModule)
            {
                if (nNumOfPayments != 0)
                    return nIndexOfUsageModule == 0 && !bIsUsageModuleIsRenewable && nTotalNumOfPayments % nNumOfPayments != 1;
                else
                    return nIndexOfUsageModule == 0 && !bIsUsageModuleIsRenewable && nTotalNumOfPayments == 2;
            }
            return nIndexOfUsageModule == 0 && !bIsUsageModuleIsRenewable && (nNumOfPayments == 0 || nTotalNumOfPayments % nNumOfPayments != 0);
        }

        /*
       * 1. This method was added as a result of a bug discovered in the renewer process
       * 2. The bug occurred in the following case:
       *      a. MPP contains two usage modules
       *      b. The mpp is not renewable
       *      c. However both the usage modules it containes are renewable.
       *      d. The bug caused the renewer to set is_recurring_status = 0 in DB after the first time the second usage module was selected by the renewer.
       *      e. This change in the DB caused the renewer to stop renewing the second usage module.
       * 
       */
        protected bool IsLastPeriodOfLastUsageModule(bool bIsPurchasedWithPreviewModule, bool bIsRecurringInfinitely, int nNumOfRecPeriods, int nPaymentNumber)
        {
            if (nNumOfRecPeriods != 0 && !bIsRecurringInfinitely)
                return nNumOfRecPeriods <= nPaymentNumber;
            return false;
        }

        protected bool IsCouponStillRedeemable(bool bIsPurchasedWithPreviewModule, int nMaxRecurringUsesCountForCoupon, int nTotalPaymentsNumber)
        {
            if (bIsPurchasedWithPreviewModule)
                nTotalPaymentsNumber--;
            return nMaxRecurringUsesCountForCoupon > nTotalPaymentsNumber || nMaxRecurringUsesCountForCoupon == 0;
        }

        private string GetIsRecurringStrForSubscriptionCustomData(bool bIsRecurring, string sPreviewModuleID)
        {
            long lPreviewModuleID = 0;
            return bIsRecurring || (Int64.TryParse(sPreviewModuleID, out lPreviewModuleID) && lPreviewModuleID > 0) ? "true" : "false";
        }

        protected virtual string GetCustomDataForMPPRenewal(TvinciPricing.Subscription theSub, TvinciPricing.UsageModule theUsageModule,
           TvinciPricing.PriceCode p, string sSubscriptionCode, string sSiteGUID, double dPrice, string sCurrency,
           string sCouponCode, string sUserIP, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {

            bool bIsRecurring = theSub.m_bIsRecurring;
            Int32 nRecPeriods = theSub.m_nNumberOfRecPeriods;

            StringBuilder sb = new StringBuilder();
            sb.Append("<customdata type=\"sp\">");
            if (!String.IsNullOrEmpty(sCountryCd))
            {
                sb.AppendFormat("<lcc>{0}</lcc>", sCountryCd);
            }
            if (!String.IsNullOrEmpty(sLANGUAGE_CODE))
            {
                sb.AppendFormat("<llc>{0}</llc>", sLANGUAGE_CODE);
            }
            if (!String.IsNullOrEmpty(sDEVICE_NAME))
            {
                sb.AppendFormat("<ldn>{0}</ldn>", sDEVICE_NAME);
            }
            sb.Append("<mnou>");
            sb.Append(theUsageModule.m_nMaxNumberOfViews.ToString());
            sb.Append("</mnou>");
            sb.AppendFormat("<u id=\"{0}\"/>", sSiteGUID);
            sb.AppendFormat("<s>{0}</s>", sSubscriptionCode);
            sb.AppendFormat("<cc>{0}</cc>", sCouponCode);
            sb.AppendFormat("<p ir=\"{0}\" n=\"1\" o=\"{1}\"/>", bIsRecurring.ToString().ToLower(), nRecPeriods.ToString());
            sb.Append("<vlcs>");
            sb.Append(theUsageModule.m_tsViewLifeCycle.ToString());
            sb.Append("</vlcs>");
            sb.Append("<mumlc>");
            sb.Append(theUsageModule.m_tsMaxUsageModuleLifeCycle.ToString());
            sb.Append("</mumlc>");
            sb.Append("<ppvm></ppvm>");
            sb.Append("<pm></pm>");
            sb.Append("<pc>");
            sb.Append(p.m_sCode);
            sb.Append("</pc>");
            sb.Append("<pri>");
            sb.Append(dPrice.ToString());
            sb.Append("</pri>");
            sb.Append("<cu>");
            sb.Append(sCurrency);
            sb.Append("</cu>");
            sb.Append("</customdata>");

            return sb.ToString();

        }

        /// <summary>
        /// Credit Card Charge User For Media File
        /// </summary>
        protected TvinciBilling.BillingResponse Cellular_BaseChargeUserForMediaFile(string sSiteGUID, double dPrice, string sCurrency,
            Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bDummy)
        {
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnown;
            ret.m_sRecieptCode = string.Empty;
            ret.m_sStatusDescription = string.Empty;

            TvinciUsers.UsersService u = null;
            TvinciPricing.mdoule m = null;
            TvinciBilling.module bm = null;

            try
            {
                log.Debug("Cellular_BaseChargeUserForMediaFile - " + string.Format("Entering Cellular_BaseChargeUserForMediaFile try block. Site Guid: {0} , Media File ID: {1} , Media ID: {2} , PPV Module Code: {3} , Coupon code: {4} , User IP: {5} , Dummy: {6}", sSiteGUID, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, bDummy.ToString().ToLower()));

                long lSiteGuid = 0;
                if (sSiteGUID.Length == 0 || !Int64.TryParse(sSiteGUID, out lSiteGuid) || lSiteGuid == 0)
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                    return ret;
                }
                else
                {
                    u = new ConditionalAccess.TvinciUsers.UsersService();

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }
                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                        WriteToUserLog(sSiteGUID, "while trying to purchase media file id(Cellular): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                        return ret;
                    }
                    else if (uObj != null && uObj.m_user != null && uObj.m_user.m_eSuspendState == TvinciUsers.DomainSuspentionStatus.Suspended)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UserSuspended;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cannot charge a suspended user";
                        WriteToUserLog(sSiteGUID, "while trying to purchase media file id(Cellular): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                        return ret;
                    }
                    else
                    {
                        if (!Utils.IsCouponValid(m_nGroupID, sCouponCode))
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = string.Empty;
                            ret.m_sStatusDescription = "Coupon not valid";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }

                        sWSUserName = string.Empty;
                        sWSPass = string.Empty;

                        m = new global::ConditionalAccess.TvinciPricing.mdoule();
                        sWSURL = Utils.GetWSURL("pricing_ws");
                        if (!string.IsNullOrEmpty(sWSURL))
                        {
                            m.Url = sWSURL;
                        }

                        Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                        if (!bDummy && string.IsNullOrEmpty(sPPVModuleCode))
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = string.Empty;
                            ret.m_sStatusDescription = "Charge must have ppv module code";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(Cellular): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }
                        // check if ppvModule related to mediaFile 
                        long ppvModuleCode = 0;
                        long.TryParse(sPPVModuleCode, out ppvModuleCode);

                        TvinciPricing.PPVModule thePPVModule = m.ValidatePPVModuleForMediaFile(sWSUserName, sWSPass, nMediaFileID, ppvModuleCode);
                        if (thePPVModule == null)
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = string.Empty;
                            ret.m_sStatusDescription = "The ppv module is unknown";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(Cellular): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }

                        if (!bDummy && !thePPVModule.m_sObjectCode.Equals(sPPVModuleCode))
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownPPVModule;
                            ret.m_sRecieptCode = string.Empty;
                            ret.m_sStatusDescription = "This PPVModule does not belong to item";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(Cellular): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }

                        if (bDummy)
                        {
                            sPPVModuleCode = thePPVModule.m_sObjectCode;
                            dPrice = thePPVModule.m_oPriceCode.m_oPrise.m_dPrice;
                            sCurrency = thePPVModule.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3;
                            if (!IsTakePriceFromMediaFileFinalPrice(bDummy))
                            { // Cinepolis patch
                                dPrice = 0d;
                            }
                        }

                        PriceReason theReason = PriceReason.UnKnown;

                        TvinciPricing.Subscription relevantSub = null;
                        TvinciPricing.Collection relevantCol = null;
                        TvinciPricing.PrePaidModule relevantPP = null;

                        TvinciPricing.Price p = Utils.GetMediaFileFinalPriceForNonGetItemsPrices(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if ((theReason == PriceReason.ForPurchase || (theReason == PriceReason.SubscriptionPurchased && p.m_dPrice > 0) || bDummy) && theReason != PriceReason.NotForPurchase)
                        {
                            if (bDummy || (p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency))
                            {
                                string sCustomData = string.Empty;
                                if (p.m_dPrice != 0 || bDummy)
                                {
                                    sWSUserName = string.Empty;
                                    sWSPass = string.Empty;

                                    InitializeBillingModule(ref bm, ref sWSUserName, ref sWSPass);
                                    if (string.IsNullOrEmpty(sCountryCd) && !string.IsNullOrEmpty(sUserIP))
                                    {
                                        sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
                                    }
                                    //Create the Custom Data
                                    sCustomData = GetCustomData(relevantSub, thePPVModule, null, sSiteGUID, dPrice, sCurrency,
                                        nMediaFileID, nMediaID, sPPVModuleCode, string.Empty, sCouponCode, sUserIP,
                                        sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                                    log.Debug("CustomData - " + sCustomData);
                                    ret = HandleCellularChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, 1, sExtraParameters, bDummy, false, ref bm);
                                }
                                if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
                                {
                                    long lBillingTransactionID = 0;
                                    long lPurchaseID = 0;
                                    HandleChargeUserForMediaFileBillingSuccess(sWSUserName, sWSPass, sSiteGUID, uObj.m_user.m_domianID, relevantSub, dPrice, sCurrency,
                                                                               sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, ret, sCustomData,
                                                                               thePPVModule, nMediaFileID, ref lBillingTransactionID, ref lPurchaseID, bDummy, ref bm);
                                    // Enqueue notification for PS so they know a collection was charged
                                    var dicData = new Dictionary<string, object>()
                                            {
                                                {"MediaFileID", nMediaFileID},
                                                {"BillingTransactionID", lBillingTransactionID},
                                                {"SiteGUID", sSiteGUID},
                                                {"PurchaseID", lPurchaseID},
                                                {"CouponCode", sCouponCode},
                                                {"CustomData", sCustomData}
                                            };

                                    this.EnqueueEventRecord(NotifiedAction.ChargedMediaFile, dicData);
                                }
                                else
                                {
                                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                                }
                            }
                            else
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "The price of the request is not the actual price";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                        }
                        else
                        {
                            if (theReason == PriceReason.PPVPurchased)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "The media file is already purchased";
                            }
                            else if (theReason == PriceReason.Free)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "The media file is free";
                            }
                            else if (theReason == PriceReason.ForPurchaseSubscriptionOnly)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "The media file is for purchase with subscription only";
                            }
                            else if (theReason == PriceReason.SubscriptionPurchased)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "The media file is already purchased (subscription)";
                            }
                            else if (theReason == PriceReason.UserSuspended)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UserSuspended;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "The user is suspended";
                            }
                            else if (theReason == PriceReason.NotForPurchase)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "The media file is not valid for purchased";
                            }
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CellularC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                        }
                    }
                }// end else if siteguid == ""
            }

            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at Cellular_BaseChargeUserForMediaFile. ");
                sb.Append(String.Concat("Exception msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Media File ID: ", nMediaFileID));
                sb.Append(String.Concat(" Media ID: ", nMediaID));
                sb.Append(String.Concat(" Coupon Code: ", sCouponCode));
                sb.Append(String.Concat(" User IP: ", sUserIP));
                sb.Append(String.Concat(" Stack trace: ", ex.StackTrace));

                log.Debug("Cellular_BaseChargeUserForMediaFile - " + sb.ToString());

                WriteToUserLog(sSiteGUID, string.Format("Exception at Cellular_BaseChargeUserForMediaFile. Media File ID: {0} , Media ID: {1} , Coupon Code: {2}", nMediaFileID, nMediaID, sCouponCode));
                #endregion
            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                    u = null;
                }
                if (m != null)
                {
                    m.Dispose();
                    m = null;
                }
                if (bm != null)
                {
                    bm.Dispose();
                    bm = null;
                }
                #endregion
            }
            return ret;

        }

        /// <summary>
        /// Credit Card Charge User For Subscription
        /// </summary>
        protected TvinciBilling.BillingResponse Cellular_BaseChargeUserForSubscription
           (string sSiteGUID, double dPrice, string sCurrency, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParams,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bDummy)
        {
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnown;
            ret.m_sRecieptCode = string.Empty;
            ret.m_sStatusDescription = string.Empty;

            TvinciUsers.UsersService u = null;
            TvinciBilling.module bm = null;
            try
            {
                log.Debug("Cellular_BaseChargeUserForSubscription - " + string.Format("Entering Cellular_BaseChargeUserForSubscription try block. Site Guid: {0} , Sub Code: {1} , Coupon Code: {2} , User IP: {3} , Dummy: {4}", sSiteGUID, sSubscriptionCode, sCouponCode, sUserIP, bDummy.ToString().ToLower()));


                long lSiteGuid = 0;
                if (sSiteGUID.Length == 0 || !Int64.TryParse(sSiteGUID, out lSiteGuid) || lSiteGuid == 0)
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                }
                else
                {
                    u = new ConditionalAccess.TvinciUsers.UsersService();

                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        u.Url = sWSURL;
                    }
                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                    }
                    else if (uObj != null && uObj.m_user != null && uObj.m_user.m_eSuspendState == TvinciUsers.DomainSuspentionStatus.Suspended)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UserSuspended;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cannot charge a suspended user";
                        WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription);
                    }
                    else
                    {
                        if (!Utils.IsCouponValid(m_nGroupID, sCouponCode))
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = string.Empty;
                            ret.m_sStatusDescription = "Coupon not valid";
                            WriteToUserLog(sSiteGUID, "While trying to purchase subscription(CC): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }

                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.Subscription theSub = null;
                        TvinciPricing.Price p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubscriptionCode, sSiteGUID, sCouponCode, ref theReason, ref theSub, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (bDummy && p != null)
                        {
                            dPrice = p.m_dPrice;
                            sCurrency = p.m_oCurrency.m_sCurrencyCD3;
                        }
                        bool bIsEntitledToPreviewModule = theReason == PriceReason.EntitledToPreviewModule;
                        if (theReason == PriceReason.ForPurchase || bIsEntitledToPreviewModule)
                        {
                            if (bDummy || (p != null && p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency))
                            {
                                string sCustomData = string.Empty;

                                bool bIsRecurring = theSub.m_bIsRecurring;
                                Int32 nRecPeriods = theSub.m_nNumberOfRecPeriods;

                                if (string.IsNullOrEmpty(sCountryCd) && !string.IsNullOrEmpty(sUserIP))
                                {
                                    sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
                                }
                                //Create the Custom Data
                                sCustomData = GetCustomDataForSubscription(theSub, null, sSubscriptionCode, string.Empty, sSiteGUID, dPrice, sCurrency,
                                    sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty, bIsEntitledToPreviewModule ? theSub.m_oPreviewModule.m_nID + "" : string.Empty, bIsEntitledToPreviewModule);

                                log.Debug("CustomData - " + string.Format("Subscription custom data created. Site Guid: {0} , User IP: {1} , Custom data: {2}", sSiteGUID, sUserIP, sCustomData));

                                if (p.m_dPrice != 0 || bDummy)
                                {

                                    sWSUserName = string.Empty;
                                    sWSPass = string.Empty;

                                    InitializeBillingModule(ref bm, ref sWSUserName, ref sWSPass);

                                    ret = HandleCellularChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP,
                                                                   sCustomData, 1, nRecPeriods, sExtraParams, bDummy, bIsEntitledToPreviewModule, ref bm);

                                }
                                if ((p.m_dPrice == 0 && !string.IsNullOrEmpty(sCouponCode)) || bIsEntitledToPreviewModule)
                                {
                                    ret.m_oStatus = TvinciBilling.BillingResponseStatus.Success;
                                }
                                if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
                                {

                                    long lBillingTransactionID = 0;
                                    long lPurchaseID = 0;
                                    HandleChargeUserForSubscriptionBillingSuccess(sWSUserName, sWSPass, sSiteGUID, uObj.m_user.m_domianID, theSub, dPrice, sCurrency, sCouponCode,
                                                                                  sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, ret, bIsEntitledToPreviewModule, sSubscriptionCode, sCustomData,
                                                                                  bIsRecurring, ref lBillingTransactionID, ref lPurchaseID, bDummy, ref bm);

                                    // Enqueue notification for PS so they know a collection was charged
                                    var dicData = new Dictionary<string, object>()
                                            {
                                                {"SubscriptionCode", sSubscriptionCode},
                                                {"BillingTransactionID", lBillingTransactionID},
                                                {"SiteGUID", sSiteGUID},
                                                {"PurchaseID", lPurchaseID},
                                                {"CouponCode", sCouponCode},
                                                {"CustomData", sCustomData}
                                            };

                                    this.EnqueueEventRecord(NotifiedAction.ChargedSubscription, dicData);

                                }
                                else
                                {
                                    WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription);
                                }
                            }
                            else
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "The price of the request is not the actual price";
                                WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + " error returned: " + ret.m_sStatusDescription);
                            }
                        }
                        else
                        {
                            if (theReason == PriceReason.Free)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "The subscription is free";
                                WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + " error returned: " + ret.m_sStatusDescription);
                            }
                            if (theReason == PriceReason.SubscriptionPurchased)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "The subscription is already purchased";
                                WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + " error returned: " + ret.m_sStatusDescription);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder(String.Concat("Exception msg: ", ex.Message));
                sb.Append(String.Concat(", Stack trace: ", ex.StackTrace));
                sb.Append(String.Concat(", Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(", Price: ", dPrice));
                sb.Append(String.Concat(", Currency: ", sCurrency));
                sb.Append(String.Concat(", Subscription Code: ", sSubscriptionCode));
                sb.Append(String.Concat(", Coupon Code: ", sCouponCode));
                sb.Append(String.Concat(", User IP: ", sUserIP));
                sb.Append(String.Concat(", Extra Params: ", sExtraParams));
                sb.Append(String.Concat(", Country Code: ", sCountryCd));
                sb.Append(String.Concat(", Language Code: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(", Device Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(", Dummy: ", bDummy.ToString().ToLower()));
                log.Debug("Cellular_BaseChargeUserForSubscription - " + sb.ToString());
                WriteToUserLog(sSiteGUID, string.Format("While trying to purchase subscription id: {0} , Exception occurred.", sSubscriptionCode));
                #endregion
                long lBillingID = 0;
                if (ret.m_oStatus != TvinciBilling.BillingResponseStatus.Success || ret.m_sRecieptCode.Length == 0 || !Int64.TryParse(ret.m_sRecieptCode, out lBillingID) || lBillingID == 0)
                {
                    ret.m_oStatus = TvinciBilling.BillingResponseStatus.UnKnown;
                    ret.m_sStatusDescription = "Exception occurred.";
                    ret.m_sRecieptCode = string.Empty;
                }
            }
            finally
            {
                #region Disposing
                if (u != null)
                {
                    u.Dispose();
                    u = null;
                }
                if (bm != null)
                {
                    bm.Dispose();
                    bm = null;
                }
                #endregion
            }
            return ret;
        }

        protected void UpdatePurchaseIDInExternalBillingTable(string sWSUsername, string sWSPassword, long lBillingTransactionID, long lPurchaseID, ref TvinciBilling.module wsBillingService)
        {
            int nExternalTransactionID = 0;
            int nBillingProvider = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select billing_provider_refference,billing_provider from billing_transactions(nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", lBillingTransactionID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        nExternalTransactionID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").Rows[0]["billing_provider_refference"]);
                        nBillingProvider = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").Rows[0]["billing_provider"]);
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

            if (nExternalTransactionID > 0 && nBillingProvider > 0 && Enum.IsDefined(typeof(eBillingProvider), nBillingProvider))
            {
                UpdatePurchaseIDInBilling(sWSUsername, sWSPassword, lPurchaseID, lBillingTransactionID, ref wsBillingService);
            }
            else
            {
                log.Debug("UpdatePurchaseIDInExternalBillingTable - " + string.Format("Unexpected error. Billing transaction ID: {0} , Purchase ID: {1} , BaseConditionalAccess is: {2} , Billing Provider: {3} , External transaction ID: {4}", lBillingTransactionID, lPurchaseID, this.GetType().Name, nBillingProvider, nExternalTransactionID));
            }
        }

        protected bool IsPreviewModuleInGroupIDCostsZero()
        {
            string sKeyOfMinPrice = String.Concat("PreviewModuleMinPrice", m_nGroupID);
            double dMinPriceForPreviewModule = Utils.DEFAULT_MIN_PRICE_FOR_PREVIEW_MODULE;
            string sValInConfig = Utils.GetValueFromConfig(sKeyOfMinPrice);
            if (sValInConfig.Length > 0 && double.TryParse(sValInConfig, out dMinPriceForPreviewModule))
                return dMinPriceForPreviewModule == 0d;
            return false;

        }


        //Change subscription for a given user - the user will be able to watch the new subscription content, 
        //but the billing for the new subscription will happen only when the previous subcription ends 
        public ChangeSubscriptionStatus ChangeSubscription(string sSiteGuid, int nOldSub, int nNewSub)
        {
            try
            {
                //check if user exists
                TvinciUsers.DomainSuspentionStatus suspendStatus = TvinciUsers.DomainSuspentionStatus.OK;
                int domainID = 0;

                if (!Utils.IsUserValid(sSiteGuid, m_nGroupID, ref domainID, ref suspendStatus))
                {
                    log.Debug("ChangeSubscription - User with siteGuid: " + sSiteGuid + " does not exist. Subscription was not changed");
                    return ChangeSubscriptionStatus.UserNotExists;
                }

                if (suspendStatus == TvinciUsers.DomainSuspentionStatus.Suspended)
                {
                    log.Debug("ChangeSubscription - User with siteGuid: " + sSiteGuid + " Suspended. Subscription was not changed");
                    return ChangeSubscriptionStatus.UserSuspended;
                }

                PermittedSubscriptionContainer[] userSubsArray = GetUserPermittedSubscriptions(sSiteGuid);//get all the valid subscriptions that this user has
                Subscription userSubNew = null;
                PermittedSubscriptionContainer userSubOld = new PermittedSubscriptionContainer();
                List<PermittedSubscriptionContainer> userOldSubList = new List<PermittedSubscriptionContainer>();
                //check if old sub exists
                if (userSubsArray != null)
                {
                    userOldSubList = userSubsArray.Where(x => x.m_sSubscriptionCode == nOldSub.ToString()).ToList();
                }
                if (userOldSubList == null || userOldSubList.Count == 0)
                {
                    return ChangeSubscriptionStatus.OldSubNotExists;
                }
                else
                {
                    if (userOldSubList.Count > 0 && userOldSubList[0] != null)
                    {
                        userSubOld = userOldSubList[0];
                        //check if the Subscription has autorenewal  
                        if (!userSubOld.m_bRecurringStatus)
                        {
                            log.Debug("ChangeSubscription - Previous Subscription ID: " + nOldSub + " is not renewable. Subscription was not changed");
                            return ChangeSubscriptionStatus.OldSubNotRenewable;
                        }
                    }
                }

                //check if new subscsription already exists for this user
                List<PermittedSubscriptionContainer> userNewSubList = userSubsArray.Where(x => x.m_sSubscriptionCode == nNewSub.ToString()).ToList();
                if (userNewSubList != null && userNewSubList.Count > 0 && userNewSubList[0] != null)
                {
                    log.Debug("ChangeSubscription - New Subscription ID: " + nNewSub + " is already attached to this user. Subscription was not changed");
                    return ChangeSubscriptionStatus.UserHadNewSub;
                }
                string pricingUsername = string.Empty, pricingPassword = string.Empty;
                Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref pricingUsername, ref pricingPassword);

                Subscription[] subs = Utils.GetSubscriptionsDataWithCaching(new List<int>(1) { nNewSub }, pricingUsername, pricingPassword, m_nGroupID);
                if (subs != null && subs.Length > 0)
                {
                    userSubNew = subs[0];
                }
                //set new subscprion
                if (userSubNew != null && userSubNew.m_SubscriptionCode != null)
                {
                    if (!userSubNew.m_bIsRecurring)
                    {
                        log.Debug("ChangeSubscription - New Subscription ID: " + nNewSub + " is not renewable. Subscription was not changed");
                        return ChangeSubscriptionStatus.NewSubNotRenewable;
                    }

                    return SetSubscriptionChange(sSiteGuid, domainID, userSubNew, userSubOld);
                }
                else
                {
                    log.Debug("ChangeSubscription - New Subscription ID: " + nNewSub + " was not found. Subscription was not changed");
                    return ChangeSubscriptionStatus.NewSubNotExits;
                }


            }
            catch (Exception exc)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at ChangeSubscription. ");
                sb.Append(String.Concat(" Ex Msg: ", exc.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGuid));
                sb.Append(String.Concat(" New Sub: ", nNewSub));
                sb.Append(String.Concat(" Old Sub: ", nOldSub));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Ex Type: ", exc.GetType().Name));
                sb.Append(String.Concat(" ST: ", exc.StackTrace));
                log.Error("Exception - " + sb.ToString(), exc);
                #endregion

                return ChangeSubscriptionStatus.Error;
            }
        }

        //the new subscription is dummy charged and its end date is set according the previous subscriptions end date
        //the previous  subscription is cancled and its end date is set to 'now'
        private ChangeSubscriptionStatus SetSubscriptionChange(string sSiteGuid, int nDomainID, Subscription subNew, PermittedSubscriptionContainer userSubOld)
        {
            ChangeSubscriptionStatus status = ChangeSubscriptionStatus.Error;
            try
            {
                #region Initialize
                string sCurrency = string.Empty;
                double dPrice = 0;
                string sCouponCode = string.Empty;
                string sUserIP = string.Empty;
                string sCountry = string.Empty;
                string sLanguage = string.Empty;
                string sDeviceName = string.Empty;
                string sSubscriptionCode = subNew.m_SubscriptionCode;
                bool isDummyCharge = true;
                string extraParams = string.Empty;
                string sBillingMethod = string.Empty;//used only in real billing and not dummy
                string sEncryptedCVV = string.Empty;//used only in real billing and not dummy                  

                if (subNew.m_oPriceCode != null && subNew.m_oPriceCode.m_oPrise != null)
                {
                    dPrice = subNew.m_oPriceCode.m_oPrise.m_dPrice;
                    if (subNew.m_oPriceCode.m_oPrise.m_oCurrency != null)
                        sCurrency = subNew.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3;
                }

                if (subNew.m_oSubscriptionUsageModule != null && subNew.m_oSubscriptionUsageModule.m_coupon_id > 0)
                {
                    sCouponCode = subNew.m_oSubscriptionUsageModule.m_coupon_id.ToString();
                }

                string sCouponCodeOld = string.Empty;
                #endregion

                //charge the user for the new subscription with dummy charge
                dPrice = 0d; // Patch for Cinepolis. price == 0 && string.IsNullOrEmpty(encryptedCVV) && string.IsNullOrEmpty(paymentMethodID) will cause billing to dummy charge.
                TvinciBilling.BillingResponse billResp = CC_BaseChargeUserForBundle(sSiteGuid, dPrice, sCurrency, sSubscriptionCode, sCouponCode, sUserIP, extraParams, sCountry, sLanguage, sDeviceName,
                    isDummyCharge, sBillingMethod, sEncryptedCVV, eBundleType.SUBSCRIPTION);

                //check if the charge was succesful: if so update relevant parameters and cancel the subsciption  
                if (billResp.m_oStatus == TvinciBilling.BillingResponseStatus.Success)
                {
                    int nBillingTransID = 0;
                    if (Int32.TryParse(billResp.m_sRecieptCode, out nBillingTransID) && nBillingTransID > 0)
                    {
                        //update the new subscription End Date and Billing Method
                        bool updateEndDateNew = ConditionalAccessDAL.Update_SubscriptionPurchaseEndDate(null, sSiteGuid, nBillingTransID, userSubOld.m_dEndDate); // set new sub end date to be the end date of the old sub
                        int nBillingMethod = (int)PaymentMethod.ChangeSubscription;
                        bool updateBillingTrans = ConditionalAccessDAL.Update_BillingMethodInBillingTransactions(nBillingTransID, nBillingMethod);

                        //update the old subscription : is_recurring_status = 0, end_date = 'now'               
                        bool bCancel = ConditionalAccessDAL.CancelSubscription(userSubOld.m_nSubscriptionPurchaseID, m_nGroupID, sSiteGuid, userSubOld.m_sSubscriptionCode) > 0;
                        bool updateEndDateOld = ConditionalAccessDAL.Update_SubscriptionPurchaseEndDate(userSubOld.m_nSubscriptionPurchaseID, sSiteGuid, null, DateTime.UtcNow);

                        if (updateEndDateNew && updateBillingTrans && bCancel && updateEndDateOld)
                        {
                            // Update domain DLM with new DLM from subscription or if no DLM in new subscription, with last domain DLM
                            UpdateDLM(nDomainID, subNew.m_nDomainLimitationModule);
                            status = ChangeSubscriptionStatus.OK;
                            WriteSubChangeToUserLog(sSiteGuid, subNew, userSubOld);
                            // Enqueue notification for PS so they know a collection was charged
                            var dicData = new Dictionary<string, object>()
                            {
                                {"oldSubscription", userSubOld.m_sSubscriptionCode},
                                {"newSubscription", subNew},
                                {"SiteGUID", sSiteGuid},
                                {"domainID", nDomainID}
                            };

                            this.EnqueueEventRecord(NotifiedAction.ChangedSubscription, dicData);
                        }
                        else
                        {
                            status = ChangeSubscriptionStatus.Error;
                            #region Logging
                            StringBuilder sb = new StringBuilder(String.Concat("SetSubscriptionChange. Update of new subscription: ", sSubscriptionCode, " from prev sub: ", userSubOld.m_sSubscriptionCode, " for user: ", sSiteGuid, " failed."));
                            sb.Append(String.Concat(" updateEndDateNew: ", updateEndDateNew.ToString().ToLower()));
                            sb.Append(String.Concat(" updateBillingTrans: ", updateBillingTrans.ToString().ToLower()));
                            sb.Append(String.Concat(" bCancel: ", bCancel.ToString().ToLower()));
                            sb.Append(String.Concat(" updateEndDateOld: ", updateEndDateOld.ToString().ToLower()));

                            log.Error("CriticalError - " + sb.ToString());

                            #endregion
                        }
                    }
                    else
                    {
                        status = ChangeSubscriptionStatus.Error;
                        #region Logging
                        StringBuilder parseMsg = new StringBuilder(String.Concat("SetSubscriptionChange. Failed to parse billing trans id: ", billResp.m_sRecieptCode));
                        parseMsg.Append(String.Concat(" Site Guid: ", sSiteGuid));
                        parseMsg.Append(String.Concat(" New Sub: ", sSubscriptionCode));
                        parseMsg.Append(String.Concat(" Old Sub: ", userSubOld.m_sSubscriptionCode));
                        log.Error("Error - " + parseMsg.ToString());
                        #endregion
                    }
                }
                else
                {
                    status = ChangeSubscriptionStatus.Error;
                    #region Logging
                    StringBuilder billingErr = new StringBuilder(String.Concat("SiteGuid: ", sSiteGuid, " did not receive success from WS_Billing while trying to change subscription. "));
                    billingErr.Append(String.Concat("New Sub: ", sSubscriptionCode));
                    billingErr.Append(String.Concat(" Price: ", dPrice));
                    billingErr.Append(String.Concat(" Curr: ", sCurrency));
                    billingErr.Append(String.Concat(" Coupon Cd: ", sCouponCode));
                    billingErr.Append(String.Concat(" User IP: ", sUserIP));
                    billingErr.Append(String.Concat(" Extra Params: ", extraParams));
                    billingErr.Append(String.Concat(" Cntry: ", sCountry));
                    billingErr.Append(String.Concat(" Lng: ", sLanguage));
                    billingErr.Append(String.Concat(" Device Nm: ", sDeviceName));
                    billingErr.Append(String.Concat(" Is Dummy: ", isDummyCharge.ToString().ToLower()));
                    billingErr.Append(String.Concat(" Bill Resp Status: ", billResp.m_oStatus.ToString()));
                    log.Error("Error - " + billingErr.ToString());
                    #endregion
                }
            }
            catch (Exception exc)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception in SetSubscriptionChange. ");
                sb.Append(String.Concat("Ex Msg: ", exc.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGuid));
                sb.Append(String.Concat(" Stack Trace: ", exc.StackTrace));
                log.Error("SetSubscriptionChange - " + sb.ToString(), exc);
                #endregion
                status = ChangeSubscriptionStatus.Error;
            }
            return status;
        }

        private void WriteSubChangeToUserLog(string sSiteGuid, Subscription subNew, PermittedSubscriptionContainer userSubOld)
        {
            StringBuilder sb = new StringBuilder("Subscription Change Request.");
            sb.Append(String.Concat(" Site Guid: ", sSiteGuid));
            if (subNew != null)
            {
                sb.Append(String.Concat(" Sub New ID: ", subNew.m_SubscriptionCode));
            }
            else
            {
                sb.Append(" Sub New ID is null.");
            }
            if (userSubOld != null)
            {
                sb.Append(String.Concat(" Sub Old ID: ", userSubOld.m_sSubscriptionCode));
            }
            else
            {
                sb.Append(" Sub Old ID is null.");
            }

            WriteToUserLog(sSiteGuid, sb.ToString());
        }

        /// <summary>
        /// Immediately cancel a household service 
        /// Cancel immediately if within cancellation window and content not already consumed OR if force flag is provided
        /// </summary>
        /// <param name="p_nDomainID"></param>
        /// <param name="p_nAssetID"></param>
        /// <param name="p_enmTransactionType"></param>
        /// <param name="p_nGroupID"></param>
        /// <param name="p_bIsForce"></param>
        /// <returns></returns>
        public virtual ApiObjects.Response.Status CancelServiceNow(int p_nDomainID, int p_nAssetID,
            eTransactionType p_enmTransactionType, int p_nGroupID, bool p_bIsForce = false)
        {
            ApiObjects.Response.Status oResult = new ApiObjects.Response.Status();

            bool bResult = false;

            try
            {
                // Start with getting domain info - both for validation and to get domain's users
                TvinciDomains.Domain oDomain = Utils.GetDomainInfo(p_nDomainID, this.m_nGroupID);

                // Check if the domain is OK
                if (oDomain == null || oDomain.m_DomainStatus != TvinciDomains.DomainStatus.OK)
                {
                    if (oDomain.m_DomainStatus == TvinciDomains.DomainStatus.DomainSuspended)
                    {
                        oResult.Code = (int)eResponseStatus.DomainSuspended;
                        oResult.Message = "Domain suspended";
                    }
                    else
                    {
                        oResult.Code = (int)eResponseStatus.DomainNotExists;
                        oResult.Message = "Domain doesn't exist";
                    }
                }
                else
                {
                    int[] arrUserIDs = oDomain.m_UsersIDs;

                    DataTable dtUserPurchases = null;
                    DataRow drUserPurchase = null;
                    string sPurchasingSiteGuid = string.Empty;

                    // Check if within cancellation window
                    bool bCancellationWindow = GetCancellationWindow(arrUserIDs, p_nAssetID, p_enmTransactionType, this.m_nGroupID, ref dtUserPurchases);

                    // Check if the user purchased the asset at all
                    if (dtUserPurchases == null || dtUserPurchases.Rows == null || dtUserPurchases.Rows.Count == 0)
                    {
                        oResult.Code = (int)eResponseStatus.InvalidPurchase;
                        oResult.Message = "There is not a valid purchase for this user and asset ID";
                    }
                    // Cancel immediately if within cancellation window and content not already consumed OR if force flag is provided
                    else if (bCancellationWindow || p_bIsForce)
                    {
                        drUserPurchase = dtUserPurchases.Rows[0];
                        sPurchasingSiteGuid = ODBCWrapper.Utils.ExtractString(drUserPurchase, "SITE_USER_GUID");
                        int nNumOfUses = ODBCWrapper.Utils.ExtractInteger(drUserPurchase, "NUM_OF_USES");

                        // If user already consumed service - cannot be cancelled without force
                        if (nNumOfUses > 0 && !p_bIsForce)
                        {
                            oResult.Code = (int)eResponseStatus.ContentAlreadyConsumed;
                            oResult.Message = "Service could not be cancelled because content was already consumed";
                        }
                        else
                        {
                            // Cancel NOW - according to type
                            switch (p_enmTransactionType)
                            {
                                case eTransactionType.PPV:
                                    {
                                        bResult = DAL.ConditionalAccessDAL.CancelPPVPurchaseTransaction(sPurchasingSiteGuid, p_nAssetID);
                                        break;
                                    }
                                case eTransactionType.Subscription:
                                    {
                                        bResult = DAL.ConditionalAccessDAL.CancelSubscriptionPurchaseTransaction(sPurchasingSiteGuid, p_nAssetID);
                                        break;
                                    }
                                case eTransactionType.Collection:
                                    {
                                        bResult = DAL.ConditionalAccessDAL.CancelCollectionPurchaseTransaction(sPurchasingSiteGuid, p_nAssetID);
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }

                            if (bResult)
                            {
                                // Update domain with last domain DLM
                                UpdateDLM(p_nDomainID, 0);

                                // Report to user log
                                WriteToUserLog(sPurchasingSiteGuid,
                                    string.Format("user :{0} CancelServiceNow for {1} item :{2}", p_nDomainID, Enum.GetName(typeof(eTransactionType), p_enmTransactionType),
                                    p_nAssetID));
                                //call billing to the client specific billing gateway to perform a cancellation action on the external billing gateway                   

                                oResult.Code = (int)eResponseStatus.OK;
                                oResult.Message = "Service successfully cancelled";

                                if (drUserPurchase != null)
                                {
                                    DateTime dtEndDate = ODBCWrapper.Utils.ExtractDateTime(drUserPurchase, "END_DATE");

                                    EnqueueCancelServiceRecord(p_nDomainID, p_nAssetID, p_enmTransactionType, dtEndDate);
                                }
                            }
                            else
                            {
                                oResult.Code = (int)eResponseStatus.Error;
                                oResult.Message = "Cancellation failed";
                            }
                        }
                    }
                    else
                    {
                        oResult.Code = (int)eResponseStatus.CancelationWindowPeriodExpired;
                        oResult.Message = "Subscription could not be cancelled because it is not in cancellation window";
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                string sLoggingMessage =
                    string.Format("Exception at CancelServiceNow. Ex Msg: {0}, Domain Id: {1}, Asset ID: {2}. Trans Type: {6}. This is {3}, Ex type: {4}, ST: {5}",
                    ex.Message, p_nDomainID, p_nAssetID, this.GetType().Name, ex.GetType().Name, ex.StackTrace, p_enmTransactionType.ToString());
                StringBuilder sb = new StringBuilder("Exception at CancelServiceNow. ");

                log.Error("Exception - " + sLoggingMessage, ex);
                #endregion

                oResult.Code = (int)eResponseStatus.Error;
                oResult.Message = "Unexpected error occurred";
            }

            return oResult;
        }

        /// <summary>
        /// Fire event that cancellation occurred 
        /// </summary>
        /// <param name="p_nDomainId"></param>
        /// <param name="p_nAssetID"></param>
        /// <param name="p_enmTransactionType"></param>
        /// <param name="p_dtServiceEndDate"></param>
        private bool EnqueueCancelServiceRecord(int p_nDomainId, int p_nAssetID, eTransactionType p_enmTransactionType, DateTime p_dtServiceEndDate)
        {
            bool bResult = false;
            try
            {
                Dictionary<string, object> dicData = new Dictionary<string, object>();
                dicData.Add("DomainId", p_nDomainId);
                dicData.Add("ServiceID", p_nAssetID);
                dicData.Add("ServiceType", (int)p_enmTransactionType);
                dicData.Add("ServiceEndDate", p_dtServiceEndDate);

                bResult = EnqueueEventRecord(NotifiedAction.CancelDomainServiceNow, dicData);
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + string.Format("Error when trying to enqueue event record. Msg: {0}", ex.Message));
            }

            return (bResult);
        }

        /// <summary>
        /// Fire event to the queue
        /// </summary>
        /// <param name="p_dicData"></param>
        protected bool EnqueueEventRecord(NotifiedAction p_eAction, Dictionary<string, object> p_dicData)
        {
            string task = Utils.GetValueFromConfig("ProfessionalServices.task");

            PSNotificationData oNotification = new PSNotificationData(task, m_nGroupID, p_dicData, p_eAction);

            PSNotificationsQueue qNotificationQueue = new PSNotificationsQueue();

            string routingKey = Utils.GetValueFromConfig("ProfessionalServices.routingKey");

            if (string.IsNullOrEmpty(routingKey))
            {
                routingKey = m_nGroupID.ToString();
            }

            bool bResult = qNotificationQueue.Enqueue(oNotification, routingKey);

            return (bResult);
        }

        /// <summary>
        /// Immediately cancel a household service 
        /// Cancel immediately if within cancellation window and content not already consumed OR if force flag is provided
        /// </summary>
        /// <param name="p_sSiteGuid"></param>
        /// <param name="p_nAssetID"></param>
        /// <param name="p_enmTransactionType"></param>
        /// <param name="p_nGroupID"></param>
        /// <param name="p_bIsForce"></param>
        /// <returns></returns>
        public virtual bool CancelTransaction(string p_sSiteGuid, int p_nAssetID, eTransactionType p_enmTransactionType, int p_nGroupID, bool p_bIsForce = false)
        {
            bool bResult = false;

            try
            {
                System.Data.DataTable dtUserPurchases = null;

                // Check if within cancellation window
                bool bCancellationWindow = GetCancellationWindow(p_sSiteGuid, p_nAssetID, p_enmTransactionType, p_nGroupID, ref dtUserPurchases);

                // Cancel immediately if within cancellation window and content not already consumed OR if force flag is provided
                if (bCancellationWindow || p_bIsForce)
                {
                    // Cancel NOW - according to type

                    switch (p_enmTransactionType)
                    {
                        case eTransactionType.PPV:
                            {
                                bResult = DAL.ConditionalAccessDAL.CancelPPVPurchaseTransaction(p_sSiteGuid, p_nAssetID);
                                break;
                            }
                        case eTransactionType.Subscription:
                            {
                                bResult = DAL.ConditionalAccessDAL.CancelSubscriptionPurchaseTransaction(p_sSiteGuid, p_nAssetID);
                                break;
                            }
                        case eTransactionType.Collection:
                            {
                                bResult = DAL.ConditionalAccessDAL.CancelCollectionPurchaseTransaction(p_sSiteGuid, p_nAssetID);
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }

                if (bResult)
                {
                    // Report to user log
                    WriteToUserLog(p_sSiteGuid, string.Format("user :{0} CancelTransaction for {1} item :{2}", p_sSiteGuid, Enum.GetName(typeof(eTransactionType), p_enmTransactionType), p_nAssetID));
                    //call billing to the client specific billing gateway to perform a cancellation action on the external billing gateway                   
                }


            }
            catch (Exception ex)
            {
                #region Logging
                string sLoggingMessage = string.Format("Exception at CancelTransaction. Ex Msg: {0}, Site Guid: {1}, Asset ID: {2}. Trans Type: {6}. This is {3}, Ex type: {4}, ST: {5}",
                    ex.Message, p_sSiteGuid, p_nAssetID, this.GetType().Name, ex.GetType().Name, ex.StackTrace, p_enmTransactionType.ToString());
                StringBuilder sb = new StringBuilder("Exception at CancelTransaction. ");

                log.Error("Exception - " + sLoggingMessage, ex);
                #endregion
            }

            return bResult;
        }

        /// <summary>
        /// Tells whether the user can still cancel the given asset or not - if it is within the cancellation window
        /// </summary>
        /// <param name="sSiteGuid"></param>
        /// <param name="nAssetID"></param>
        /// <param name="transactionType"></param>
        /// <param name="nGroupID"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        private bool GetCancellationWindow(string sSiteGuid, int nAssetID, eTransactionType transactionType, int nGroupID, ref DataTable dt)
        {
            bool bResult = false;
            int nSiteGuid;

            if (int.TryParse(sSiteGuid, out nSiteGuid))
            {
                bResult = GetCancellationWindow(new int[] { nSiteGuid }, nAssetID, transactionType, nGroupID, ref dt);
            }

            return (bResult);
        }

        /// <summary>
        /// Tells whether the users can still cancel the given asset or not - if they are within the cancellation window
        /// </summary>
        /// <param name="p_arrUserIDs"></param>
        /// <param name="p_nAssetID"></param>
        /// <param name="p_enmServiceType"></param>
        /// <param name="p_nGroupID"></param>
        /// <param name="p_dtUserPurchases"></param>
        /// <returns></returns>
        private bool GetCancellationWindow(int[] p_arrUserIDs, int p_nAssetID, eTransactionType p_enmServiceType, int p_nGroupID, ref DataTable p_dtUserPurchases)
        {
            TvinciPricing.UsageModule oUsageModule = null;
            bool bCancellationWindow = false;
            DateTime dtCreateDate = DateTime.MinValue;
            string sAssetCode = string.Empty;

            // According to service type (sub, ppv or col), get all purchases of users
            switch (p_enmServiceType)
            {
                case eTransactionType.PPV:
                    {
                        p_dtUserPurchases = ConditionalAccessDAL.Get_AllPPVPurchasesByUserIDsAndMediaFileID(p_nAssetID, p_arrUserIDs.ToList(), p_nGroupID);
                        break;
                    }
                case eTransactionType.Subscription:
                    {
                        p_dtUserPurchases = ConditionalAccessDAL.Get_AllSubscriptionPurchasesByUserIDsAndSubscriptionCode(p_nAssetID, p_arrUserIDs.ToList(), p_nGroupID);
                        break;
                    }
                case eTransactionType.Collection:
                    {
                        p_dtUserPurchases = ConditionalAccessDAL.Get_AllCollectionPurchasesByUserIDsAndCollectionCode(p_nAssetID, p_arrUserIDs.ToList(), p_nGroupID);
                        break;
                    }
                default:
                    {
                        bCancellationWindow = false;
                        break;
                    }
            }

            // If any of the users purchased this asset and it is valid
            if (p_dtUserPurchases != null && p_dtUserPurchases.Rows != null && p_dtUserPurchases.Rows.Count > 0)
            {
                // First row is supposed to be the relevant purchase
                DataRow drUserPurchase = p_dtUserPurchases.Rows[0];

                dtCreateDate = ODBCWrapper.Utils.ExtractDateTime(drUserPurchase, "CREATE_DATE");
                sAssetCode = ODBCWrapper.Utils.ExtractString(drUserPurchase, "assetCode"); // ppvCode/SubscriptionCode/CollectionCode

                IsCancellationWindow(ref oUsageModule, sAssetCode, dtCreateDate, ref bCancellationWindow, p_enmServiceType);
            }

            return bCancellationWindow;
        }

        /*This method shall set the waiver flag on the user entitlement table (susbcriptions/ppv/collection_purchases) 
         * and the waiver_date field to the current date.*/
        public virtual bool WaiverTransaction(string sSiteGuid, int nAssetID, eTransactionType transactionType, int nGroupID)
        {
            bool bRes = false;
            System.Data.DataTable dt = null;

            try
            {
                bool bCancellationWindow = GetCancellationWindow(sSiteGuid, nAssetID, transactionType, nGroupID, ref dt);

                if (bCancellationWindow)
                {
                    // if it's relevant by dates cancel it
                    switch (transactionType)
                    {
                        case eTransactionType.PPV:
                            bRes = ConditionalAccessDAL.WaiverPPVPurchaseTransaction(sSiteGuid, nAssetID);
                            break;
                        case eTransactionType.Subscription:
                            bRes = ConditionalAccessDAL.WaiverSubscriptionPurchaseTransaction(sSiteGuid, nAssetID);
                            break;
                        case eTransactionType.Collection:
                            bRes = ConditionalAccessDAL.WaiverCollectionPurchaseTransaction(sSiteGuid, nAssetID);
                            break;
                        default:
                            return false;
                    }
                }


                if (bRes)
                {
                    WriteToUserLog(sSiteGuid, string.Format("user :{0} waiver cancellation for {1} item :{2}", sSiteGuid, Enum.GetName(typeof(eTransactionType), transactionType), nAssetID));
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at WaiverTransaction. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGuid));
                sb.Append(String.Concat(" Asset ID: ", nAssetID));
                sb.Append(String.Concat(" Trans Type: ", transactionType.ToString()));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }

            return bRes;
        }

        private bool TryGetFileUrlLinks(int mediaFileID, string userIP, string siteGuid, ref string mainUrl, ref string altUrl,
            ref int mainStreamingCoID, ref int altStreamingCoID, ref int nMediaID)
        {
            bool res = false;

            // True - use DAL, with "our" slim stored procedure; false - use Catalog, with its full stored procedure
            bool shouldUseDalOrCatalog = TVinciShared.WS_Utils.GetTcmBoolValue("ShouldGetMediaFileDetailsDirectly");

            if (shouldUseDalOrCatalog)
            {
                res = ConditionalAccessDAL.GetFileUrlLinks(mediaFileID, siteGuid, m_nGroupID, ref mainUrl, ref altUrl, ref mainStreamingCoID, ref altStreamingCoID);
            }
            else
            {
                WS_Catalog.MediaFilesRequest request = new WS_Catalog.MediaFilesRequest();
                request.m_lMediaFileIDs = new int[1] { mediaFileID };
                request.m_nGroupID = m_nGroupID;
                request.m_oFilter = new WS_Catalog.Filter();
                request.m_sSiteGuid = siteGuid;
                request.m_sUserIP = userIP;
                request.m_sSignString = Guid.NewGuid().ToString();
                request.m_sSignature = TVinciShared.WS_Utils.GetCatalogSignature(request.m_sSignString, Utils.GetWSURL("CatalogSignatureKey"));
                request.m_lCoGuids = new string[0];
                using (WS_Catalog.IserviceClient catalog = new WS_Catalog.IserviceClient())
                {
                    catalog.Endpoint.Address = new System.ServiceModel.EndpointAddress(Utils.GetWSURL("WS_Catalog"));
                    WS_Catalog.MediaFilesResponse response = catalog.GetResponse(request) as WS_Catalog.MediaFilesResponse;

                    if (response != null && response.m_lObj != null && response.m_lObj.Length > 0)
                    {
                        WS_Catalog.MediaFileObj mf = response.m_lObj[0] as WS_Catalog.MediaFileObj;
                        if (mf != null && mf.m_oFile != null)
                        {
                            res = true;
                            mainUrl = mf.m_oFile.m_sUrl;
                            altUrl = mf.m_oFile.m_sAltUrl;
                            mainStreamingCoID = mf.m_oFile.m_nCdnID;
                            altStreamingCoID = mf.m_oFile.m_nAltCdnID;
                        }

                    }

                }
            }

            return res;
        }


        public virtual LicensedLinkResponse GetLicensedLinks(string sSiteGuid, Int32 nMediaFileID, string sBasicLink, string sUserIP,
           string sRefferer, string sCountryCode, string sLanguageCode, string sDeviceName, string sCouponCode)
        {
            int fileMainStreamingCoID = 0;
            return GetLicensedLinks(sSiteGuid, nMediaFileID, sBasicLink, sUserIP, sRefferer, sCountryCode, sLanguageCode, sDeviceName, sCouponCode, eObjectType.Media, ref fileMainStreamingCoID);
        }

        public virtual LicensedLinkResponse GetLicensedLinks(string sSiteGuid, Int32 nMediaFileID, string sBasicLink, string sUserIP,
           string sRefferer, string sCountryCode, string sLanguageCode, string sDeviceName, string sCouponCode, ref int fileMainStreamingCoID)
        {
            return GetLicensedLinks(sSiteGuid, nMediaFileID, sBasicLink, sUserIP, sRefferer, sCountryCode, sLanguageCode, sDeviceName, sCouponCode, eObjectType.Media, ref fileMainStreamingCoID);
        }

        public virtual LicensedLinkResponse GetLicensedLinks(string sSiteGuid, Int32 nMediaFileID, string sBasicLink, string sUserIP,
            string sRefferer, string sCountryCode, string sLanguageCode, string sDeviceName, string sCouponCode, eObjectType eLinkType, ref int fileMainStreamingCoID)
        {
            LicensedLinkResponse res = new LicensedLinkResponse();

            try
            {
                int[] mediaFiles = new int[1] { nMediaFileID };
                int streamingCoID = 0;

                if ((!string.IsNullOrEmpty(sBasicLink) && nMediaFileID > 0))
                {
                    if (IsAlterBasicLink(sBasicLink, nMediaFileID))
                    {
                        sBasicLink = Utils.GetBasicLink(m_nGroupID, mediaFiles, nMediaFileID, sBasicLink, out streamingCoID);
                    }

                    MediaFileItemPricesContainer[] prices = GetItemsPrices(mediaFiles, sSiteGuid, sCouponCode, true, sCountryCode,
                        sLanguageCode, sDeviceName, sUserIP);

                    if (prices != null && prices.Length > 0)
                    {
                        int nMediaID = 0;
                        int nRuleID = 0;
                        List<int> lRuleIDS = new List<int>();
                        TvinciDomains.DomainResponseStatus mediaConcurrencyResponse;

                        string fileMainUrl = string.Empty;
                        string fileAltUrl = string.Empty;
                        //int fileMainStreamingCoID = 0;
                        int fileAltStreamingCoID = 0;

                        if (!IsUserSuspended(prices[0]))       //check that the user is not suspended
                        {
                            if (TryGetFileUrlLinks(nMediaFileID, sUserIP, sSiteGuid, ref fileMainUrl, ref fileAltUrl, ref fileMainStreamingCoID,
                                ref fileAltStreamingCoID, ref nMediaID))
                            {
                                Dictionary<string, string> licensedLinkParams = GetLicensedLinkParamsDict(sSiteGuid, nMediaFileID.ToString(),
                                    fileMainUrl, sUserIP, sCountryCode, sLanguageCode, sDeviceName, sCouponCode);

                                if (IsFreeItem(prices[0]) || IsItemPurchased(prices[0]))
                                {
                                    string CdnStrID = string.Empty;
                                    bool bIsDynamic = Utils.GetStreamingUrlType(fileMainStreamingCoID, ref CdnStrID);

                                    if (sBasicLink.ToLower().Trim().EndsWith(fileMainUrl.ToLower().Trim()) || bIsDynamic)
                                    {
                                        mediaConcurrencyResponse = CheckMediaConcurrency(sSiteGuid, nMediaFileID, sDeviceName, prices, nMediaID, sUserIP, ref lRuleIDS);
                                        if (mediaConcurrencyResponse == TvinciDomains.DomainResponseStatus.OK)
                                        {
                                            if (IsItemPurchased(prices[0]))
                                            {
                                                HandlePlayUses(prices[0], sSiteGuid, nMediaFileID, sUserIP, sCountryCode, sLanguageCode, sDeviceName, sCouponCode);
                                            }

                                            // TO DO if dynamic call to right provider to get the URL
                                            if (eLinkType == eObjectType.Media && bIsDynamic)
                                            {
                                                //call the right provider to get the link 
                                                StreamingProvider.ILSProvider provider = StreamingProvider.LSProviderFactory.GetLSProvidernstance(CdnStrID);
                                                if (provider != null)
                                                {
                                                    string vodUrl = provider.GenerateVODLink(sBasicLink);
                                                    if (!string.IsNullOrEmpty(vodUrl))
                                                    {
                                                        licensedLinkParams[CDNTokenizers.Constants.URL] = vodUrl;
                                                    }
                                                }
                                            }


                                            res.mainUrl = GetLicensedLink(fileMainStreamingCoID, licensedLinkParams);
                                            licensedLinkParams[CDNTokenizers.Constants.URL] = fileAltUrl;
                                            res.altUrl = GetLicensedLink(fileAltStreamingCoID, licensedLinkParams);
                                            res.status = mediaConcurrencyResponse.ToString();
                                            res.Status.Code = ConcurrencyResponseToResponseStatus(mediaConcurrencyResponse);

                                            // create PlayCycle
                                            CreatePlayCycle(sSiteGuid, nMediaFileID, sUserIP, sDeviceName, nMediaID, nRuleID, lRuleIDS);
                                        }
                                        else
                                        {
                                            res.altUrl = GetErrorLicensedLink(sBasicLink);
                                            res.mainUrl = GetErrorLicensedLink(sBasicLink);
                                            res.status = mediaConcurrencyResponse.ToString();
                                            res.Status.Code = ConcurrencyResponseToResponseStatus(mediaConcurrencyResponse);

                                            log.Debug("GetLicensedLinks - " + string.Format("{0}, user:{1}, MFID:{2}",
                                                mediaConcurrencyResponse.ToString(), sSiteGuid, nMediaFileID));
                                        }
                                    }
                                    else
                                    {
                                        res.altUrl = GetErrorLicensedLink(sBasicLink);
                                        res.mainUrl = GetErrorLicensedLink(sBasicLink);
                                        res.status = eLicensedLinkStatus.InvalidBaseLink.ToString();
                                        res.Status.Code = (int)eResponseStatus.InvalidBaseLink;

                                        log.Debug("GetLicensedLinks - " + string.Format("Error ValidateBaseLink, user:{0}, MFID:{1}, link:{2}",
                                            sSiteGuid, nMediaFileID, sBasicLink));
                                    }
                                }
                                else
                                {
                                    res.altUrl = GetErrorLicensedLink(sBasicLink);
                                    res.mainUrl = GetErrorLicensedLink(sBasicLink);
                                    res.status = eLicensedLinkStatus.InvalidPrice.ToString();
                                    res.Status.Code = (int)eResponseStatus.Error;

                                    log.Debug("GetLicensedLinks - " + string.Format("Price not valid, user:{0}, MFID:{1}, priceReason:{2}, price:{3}", sSiteGuid,
                                        nMediaFileID, prices[0].m_oItemPrices[0].m_PriceReason.ToString(), prices[0].m_oItemPrices[0].m_oPrice.m_dPrice));
                                }
                            }
                            else
                            {
                                res.altUrl = GetErrorLicensedLink(sBasicLink);
                                res.mainUrl = GetErrorLicensedLink(sBasicLink);
                                res.status = eLicensedLinkStatus.InvalidFileData.ToString();
                                res.Status.Code = (int)eResponseStatus.Error;

                                log.Debug("GetLicensedLinks - " + string.Format("Failed to retrieve data from Catalog, user:{0}, MFID:{1}, link:{2}",
                                    sSiteGuid, nMediaFileID, sBasicLink));
                            }
                        }
                        else //user is Suspended
                        {
                            //returns empty url
                            res.status = eLicensedLinkStatus.UserSuspended.ToString();
                            res.Status.Code = (int)eResponseStatus.UserSuspended;

                            log.Debug("GetLicensedLinks - " + string.Format("User is suspended. user:{0}, MFID:{1}", sSiteGuid, nMediaFileID));
                        }
                    }
                    else
                    {
                        res.altUrl = GetErrorLicensedLink(sBasicLink);
                        res.mainUrl = GetErrorLicensedLink(sBasicLink);
                        res.status = eLicensedLinkStatus.InvalidPrice.ToString();
                        res.Status.Code = (int)eResponseStatus.Error;

                        log.Debug("GetLicensedLinks - " + string.Format("Price is null. user:{0}, MFID:{1}", sSiteGuid, nMediaFileID));
                    }
                }
                else
                {
                    res.altUrl = GetErrorLicensedLink(sBasicLink);
                    res.mainUrl = GetErrorLicensedLink(sBasicLink);
                    res.status = eLicensedLinkStatus.InvalidInput.ToString();
                    res.Status.Code = (int)eResponseStatus.Error;

                    log.Debug("GetLicensedLinks - " + string.Format("input is invalid. user:{0}, MFID:{1}, device:{2}, link:{3}",
                        sSiteGuid, nMediaFileID, sDeviceName, sBasicLink));
                }
            }
            catch (Exception ex)
            {
                res.altUrl = GetErrorLicensedLink(sBasicLink);
                res.mainUrl = GetErrorLicensedLink(sBasicLink);
                res.status = eLicensedLinkStatus.Error.ToString();
                res.Status.Code = (int)eResponseStatus.Error;

                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetLicensedLinks. ");
                sb.Append(String.Concat("Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" SiteGuid: ", sSiteGuid));
                sb.Append(String.Concat(" MF ID: ", nMediaFileID));
                sb.Append(String.Concat(" Basic Link: ", sBasicLink));
                sb.Append(String.Concat(" User IP: ", sUserIP));
                sb.Append(String.Concat(" Referrer: ", sRefferer));
                sb.Append(String.Concat(" Country Cd: ", sCountryCode));
                sb.Append(String.Concat(" Lng Cd: ", sLanguageCode));
                sb.Append(String.Concat(" Device Name: ", sDeviceName));
                sb.Append(String.Concat(" Coupon Cd: ", sCouponCode));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);

                #endregion
            }

            return res;
        }

        private int ConcurrencyResponseToResponseStatus(TvinciDomains.DomainResponseStatus mediaConcurrencyResponse)
        {
            eResponseStatus res;

            switch (mediaConcurrencyResponse)
            {
                case ConditionalAccess.TvinciDomains.DomainResponseStatus.LimitationPeriod:
                    res = eResponseStatus.LimitationPeriod;
                    break;
                case ConditionalAccess.TvinciDomains.DomainResponseStatus.Error:
                    res = eResponseStatus.Error;
                    break;
                case ConditionalAccess.TvinciDomains.DomainResponseStatus.ExceededLimit:
                    res = eResponseStatus.ExceededLimit;
                    break;
                case ConditionalAccess.TvinciDomains.DomainResponseStatus.DeviceTypeNotAllowed:
                    res = eResponseStatus.DeviceTypeNotAllowed;
                    break;
                case ConditionalAccess.TvinciDomains.DomainResponseStatus.DeviceNotInDomain:
                    res = eResponseStatus.DeviceNotInDomain;
                    break;
                case ConditionalAccess.TvinciDomains.DomainResponseStatus.DeviceAlreadyExists:
                    res = eResponseStatus.DeviceAlreadyExists;
                    break;
                case ConditionalAccess.TvinciDomains.DomainResponseStatus.OK:
                    res = eResponseStatus.OK;
                    break;
                case ConditionalAccess.TvinciDomains.DomainResponseStatus.DeviceExistsInOtherDomains:
                    res = eResponseStatus.DeviceExistsInOtherDomains;
                    break;
                case ConditionalAccess.TvinciDomains.DomainResponseStatus.ConcurrencyLimitation:
                    res = eResponseStatus.ConcurrencyLimitation;
                    break;
                case ConditionalAccess.TvinciDomains.DomainResponseStatus.MediaConcurrencyLimitation:
                    res = eResponseStatus.MediaConcurrencyLimitation;
                    break;
                default:
                    res = eResponseStatus.Error;
                    break;
            }

            return (int)res;
        }

        /*******************************************************************************************
         * This method create the PLAY_CYCLE_KEY key in DB with the (media_concurrency) mc_rule_id 
         ***************************************************************************************** */
        private void CreatePlayCycle(string sSiteGuid, Int32 nMediaFileID, string sUserIP, string sDeviceName, int nMediaID, int nRuleID, List<int> lRuleIDS)
        {
            // create PlayCycle       
            if (lRuleIDS != null && lRuleIDS.Count > 0)
            {
                nRuleID = lRuleIDS[0]; // take the first rule (probably will be just one rule)
            }
            string sPlayCycleKey = Guid.NewGuid().ToString();
            int nCountryID = Utils.GetCountryIDByIP(sUserIP);
            Tvinci.Core.DAL.CatalogDAL.Insert_NewPlayCycleKey(this.m_nGroupID, nMediaID, nMediaFileID, sSiteGuid, 0, sDeviceName, nCountryID, sPlayCycleKey, nRuleID);
        }

        private TvinciDomains.DomainResponseStatus CheckMediaConcurrency(string sSiteGuid, Int32 nMediaFileID, string sDeviceName, MediaFileItemPricesContainer[] prices,
            int nMediaID, string sUserIP, ref List<int> lRuleIDS)
        {
            TvinciDomains.DomainResponseStatus response = TvinciDomains.DomainResponseStatus.OK;
            TvinciDomains.module domainsWS = null;
            TvinciAPI.API apiWs = null;

            if (Utils.IsAnonymousUser(sSiteGuid))
            {
                return response;
            }

            try
            {
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;

                /*Get Media Concurrency Rules*/
                apiWs = new TvinciAPI.API();
                Utils.GetWSCredentials(m_nGroupID, eWSModules.API, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("api_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                {
                    apiWs.Url = sWSURL;
                }
                int bmID = 0;
                bool bSuccess = false;

                TvinciAPI.eBusinessModule eBM = TvinciAPI.eBusinessModule.PPV;

                if (prices[0].m_oItemPrices != null && prices[0].m_oItemPrices[0].m_PriceReason == PriceReason.PPVPurchased)
                {
                    bSuccess = int.TryParse(prices[0].m_oItemPrices[0].m_sPPVModuleCode, out bmID);
                }
                else if (prices[0].m_oItemPrices != null && prices[0].m_oItemPrices[0].m_PriceReason == PriceReason.SubscriptionPurchased)
                {
                    bSuccess = int.TryParse(prices[0].m_oItemPrices[0].m_sPPVModuleCode, out bmID);
                    eBM = TvinciAPI.eBusinessModule.Subscription;
                }
                if (!bSuccess)
                {
                    return response;
                }

                TvinciAPI.MediaConcurrencyRule[] mcRules = apiWs.GetMediaConcurrencyRules(sWSUserName, sWSPass, nMediaID, sUserIP, bmID, eBM);
                TvinciDomains.ValidationResponseObject validationResponse = new TvinciDomains.ValidationResponseObject();
                /*MediaConurrency Check */
                domainsWS = new TvinciDomains.module();
                sWSUserName = string.Empty;
                sWSPass = string.Empty;

                Utils.GetWSCredentials(m_nGroupID, eWSModules.DOMAINS, ref sWSUserName, ref sWSPass);
                sWSURL = Utils.GetWSURL("domains_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                {
                    domainsWS.Url = sWSURL;
                }
                int nDeviceFamilyBrand = 0;
                long lSiteGuid = 0;
                long.TryParse(sSiteGuid, out lSiteGuid);

                if (mcRules != null && mcRules.Count() > 0)
                {
                    foreach (TvinciAPI.MediaConcurrencyRule mcRule in mcRules)
                    {
                        lRuleIDS.Add(mcRule.RuleID); // for future use

                        validationResponse = domainsWS.ValidateLimitationModule(sWSUserName, sWSPass, sDeviceName, nDeviceFamilyBrand, lSiteGuid, 0,
                            TvinciDomains.ValidationType.Concurrency, mcRule.RuleID, 0, nMediaID);
                        if (response == TvinciDomains.DomainResponseStatus.OK) // when there is more then one rule  - change response status only when status is still OK (that mean that this is the first time it's change)
                        {
                            response = validationResponse.m_eStatus;
                        }
                    }
                }
                else
                {
                    validationResponse = domainsWS.ValidateLimitationModule(sWSUserName, sWSPass, sDeviceName, nDeviceFamilyBrand, lSiteGuid, 0,
                           TvinciDomains.ValidationType.Concurrency, 0, 0, nMediaID);
                    response = validationResponse.m_eStatus;
                }
                return response;
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at CheckMediaConcurrency. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" SiteGuid: ", sSiteGuid));
                sb.Append(String.Concat(" MF ID: ", nMediaFileID));
                sb.Append(String.Concat(" Device: ", sDeviceName));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                response = TvinciDomains.DomainResponseStatus.Error;
            }
            finally
            {
                if (domainsWS != null)
                {
                    domainsWS.Dispose();
                }
                if (apiWs != null)
                {
                    apiWs.Dispose();
                }
            }

            return response;
        }

        private Dictionary<string, string> GetLicensedLinkParamsDict(string sSiteGuid, string mediaFileIDStr, string basicLink,
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

        internal bool IsItemPurchased(MediaFileItemPricesContainer price)
        {
            bool res = false;
            PriceReason reason = price.m_oItemPrices[0].m_PriceReason;
            switch (reason)
            {
                case PriceReason.SubscriptionPurchased:
                    goto case PriceReason.PPVPurchased;
                case PriceReason.PrePaidPurchased:
                    goto case PriceReason.PPVPurchased;
                case PriceReason.CollectionPurchased:
                    goto case PriceReason.PPVPurchased;
                case PriceReason.PPVPurchased:
                    res = price.m_oItemPrices[0].m_oPrice.m_dPrice == 0d;
                    break;
                default:
                    break;

            }

            return res;
        }

        private bool IsAlterBasicLink(string sBasicLink, int nMediaFileID)
        {
            return sBasicLink.Contains(string.Format("||{0}", nMediaFileID));
        }

        private bool IsGetLicensedLinksInputValid(string siteGuid, int mediaFileID, string basicLink)
        {
            int temp = 0;
            return basicLink != null && mediaFileID > 0 && !string.IsNullOrEmpty(siteGuid) && Int32.TryParse(siteGuid, out temp) && temp > 0;
        }

        internal bool IsFreeItem(MediaFileItemPricesContainer container)
        {
            return container.m_oItemPrices == null || container.m_oItemPrices.Length == 0 || container.m_oItemPrices[0].m_PriceReason == PriceReason.Free;
        }

        private bool IsUserSuspended(MediaFileItemPricesContainer container)
        {
            return (container.m_oItemPrices[0] != null && container.m_oItemPrices[0].m_PriceReason == PriceReason.UserSuspended);
        }

        public virtual RecordResponse RecordNPVR(string siteGuid, string assetID, bool isSeries)
        {
            throw new NotImplementedException("Not implemented yet.");
        }

        public virtual RecordResponse RecordSeriesByProgramID(string siteGuid, string epgProgramIdAssignedToSeries)
        {
            throw new NotImplementedException("Not implemented yet.");
        }

        public virtual RecordResponse RecordSeriesByName(string siteGuid, string seriesName)
        {
            throw new NotImplementedException("Not implemented yet.");
        }

        public virtual NPVRResponse CancelNPVR(string siteGuid, string assetID, bool isSeries)
        {
            throw new NotImplementedException("Not implemented yet.");
        }

        public virtual NPVRResponse DeleteNPVR(string siteGuid, string assetID, bool isSeries)
        {
            throw new NotImplementedException("Not implemented yet.");
        }

        public virtual QuotaResponse GetNPVRQuota(string siteGuid)
        {
            throw new NotImplementedException("Not implemented yet.");
        }

        public virtual NPVRResponse SetNPVRProtectionStatus(string siteGuid, string assetID, bool isSeries, bool isProtect)
        {
            throw new NotImplementedException("Not implemented yet.");
        }

        protected string GetNPVRLogMsg(string msg, string siteGuid, string assetID, bool isSeries, Exception ex)
        {
            StringBuilder sb = new StringBuilder(String.Concat(msg, "."));
            sb.Append(String.Concat(" Site Guid: ", siteGuid));
            sb.Append(String.Concat(" Asset ID: ", assetID));
            sb.Append(String.Concat(" Is Series: ", isSeries.ToString().ToLower()));
            sb.Append(String.Concat(" this is: ", this.GetType().Name));
            sb.Append(String.Concat(" Group ID: ", m_nGroupID));
            if (ex != null)
            {
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
            }

            return sb.ToString();
        }

        protected virtual string CalcNPVRLicensedLink(string sProgramId, DateTime dStartTime, int format, string sSiteGUID, Int32 nMediaFileID, string sBasicLink, string sUserIP,
            string sRefferer, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, string sCouponCode)
        {
            return string.Empty;
        }
        public ConditionalAccess.Response.DomainServicesResponse GetDomainServices(int groupID, int domainID)
        {
            DomainServicesResponse domainServicesResponse = new DomainServicesResponse((int)eResponseStatus.OK);
            PermittedSubscriptionContainer[] domainSubscriptions = GetDomainPermittedSubscriptions(domainID);

            if (domainSubscriptions != null)
            {
                List<long> subscriptionIDs = domainSubscriptions.Select(s => long.Parse(s.m_sSubscriptionCode)).ToList();
                DataTable subscriptionServices = PricingDAL.Get_SubscriptionsServices(groupID, subscriptionIDs);
                if (subscriptionServices != null && subscriptionServices.Rows != null && subscriptionServices.Rows.Count > 0)
                {
                    HashSet<int> uniqueIds = new HashSet<int>();

                    foreach (DataRow row in subscriptionServices.Rows)
                    {
                        int serviceId = ODBCWrapper.Utils.ExtractInteger(row, "service_id");

                        if (!uniqueIds.Contains(serviceId))
                        {
                            domainServicesResponse.Services.Add(new ServiceObject()
                            {
                                ID = serviceId,
                                Name = ODBCWrapper.Utils.GetSafeStr(row["description"])
                            });

                            uniqueIds.Add(serviceId);
                        }
                    }
                }
            }

            return domainServicesResponse;
        }

        protected void UpdateDLM(long domainID, int dlm)
        {
            if (dlm == 0)
            {
                long lastDomainDLM = ConditionalAccessDAL.Get_LastDomainDLM(m_nGroupID, domainID);
                ConditionalAccess.TvinciDomains.ChangeDLMObj changeDlmObj = Utils.ChangeDLM(m_nGroupID, domainID, (int)lastDomainDLM);
                if (changeDlmObj.resp != null && changeDlmObj.resp.Code == (int)eResponseStatus.OK)
                {
                    #region Logging
                    StringBuilder sb = new StringBuilder("Failed to change domain DLM to last DLM");
                    sb.Append(String.Concat(" with Status: ", changeDlmObj.resp));
                    sb.Append(String.Concat(" Domain ID: ", domainID));
                    sb.Append(String.Concat(" Last DLM ID: ", lastDomainDLM));
                    sb.Append(String.Concat(" BaseConditionalAccess is: ", this.GetType().Name));
                    log.Debug("ChangeDLM - " + sb.ToString());
                    #endregion
                }
            }
            else
            {
                ConditionalAccess.TvinciDomains.ChangeDLMObj changeDlmObj = Utils.ChangeDLM(m_nGroupID, domainID, dlm);
                if (changeDlmObj.resp != null && changeDlmObj.resp.Code == (int)eResponseStatus.OK)
                {
                    #region Logging
                    StringBuilder sb = new StringBuilder("Failed to change domain DLM to new DLM");
                    sb.Append(String.Concat(" with Status: ", changeDlmObj.resp));
                    sb.Append(String.Concat(" Domain ID: ", domainID));
                    sb.Append(String.Concat(" New DLM ID: ", dlm));
                    sb.Append(String.Concat(" BaseConditionalAccess is: ", this.GetType().Name));
                    log.Debug("ChangeDLM - " + sb.ToString());
                    #endregion
                }
            }
        }

        protected bool IsServiceAllowed(int groupID, int domainID, eService service)
        {
            GroupsCacheManager.Group group = GroupsCache.Instance().GetGroup(groupID);
            if (group != null)
            {
                List<int> enforcedGroupServices = group.GetServices();
                //check if service is part of the group enforced services
                if (enforcedGroupServices == null || enforcedGroupServices.Count == 0 || !enforcedGroupServices.Contains((int)service))
                {
                    return true;
                }

                // check if the service is allowed for the domain
                ConditionalAccess.Response.DomainServicesResponse allowedDomainServicesRes = GetDomainServices(groupID, domainID);
                if (allowedDomainServicesRes != null && allowedDomainServicesRes.Status.Code == 0 &&
                    allowedDomainServicesRes.Services != null && allowedDomainServicesRes.Services.Count > 0 && allowedDomainServicesRes.Services.Where(s => s.ID == (int)service).FirstOrDefault() != null)
                {
                    return true;
                }
            }
            return false;
        }

        protected eService GetServiceByEPGFormat(eEPGFormatType eformat)
        {
            eService eservice;

            switch (eformat)
            {
                case eEPGFormatType.Catchup:
                    eservice = eService.CatchUp;
                    break;
                case eEPGFormatType.StartOver:
                    eservice = eService.StartOver;
                    break;
                case eEPGFormatType.LivePause:
                    eservice = eService.Unknown;
                    break;
                case eEPGFormatType.NPVR:
                    eservice = eService.NPVR;
                    break;
                default:
                    eservice = eService.Unknown;
                    break;
            }
            return eservice;
        }

        /// <summary>
        /// Get User Subscriptions
        /// </summary>
        public virtual Entitlement GetUserSubscriptions(string sSiteGUID)
        {
            Entitlement response = new Entitlement();

            try
            {
                PermittedSubscriptionContainer[] psc = GetUserPermittedSubscriptions(new List<int>() { int.Parse(sSiteGUID) }, false, 0);
                if (psc != null && psc.Length > 0)
                {
                    // fill Entitlement object
                    response.resp = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.OK, "OK");
                    response.entitelments = new List<Entitlements>();
                    foreach (PermittedSubscriptionContainer item in psc)
                    {
                        Entitlements ent = new Entitlements(item);
                        response.entitelments.Add(ent);
                    }
                }
                else
                {
                    response = new Entitlement();
                    response.resp = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.OK, "no items return");
                }
            }
            catch (Exception ex)
            {
                log.Error("GetUserSubscriptions - " + string.Format("failed GetUserPermittedSubscriptions ex = {0}", ex.Message), ex);
                response = new Entitlement();
                response.resp = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
            return response;
        }


        /// <summary>
        /// Purchase
        /// </summary>
        public virtual PurchaseResponse Purchase(string siteguid, long household, double price, string currency, int contentId, int productId,
                                                 eTransactionType transactionType, string coupon, string userIp, string deviceName, int paymentGwId)
        {
            PurchaseResponse response = new PurchaseResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // log request
            string logString = string.Format("Purchase request: siteguid {0}, household {1}, price {2}, currency {3}, contentId {4}, productId {5}, productType {6}, coupon {7}, userIp {8}, deviceName {9}, paymentGwId {10}",
                !string.IsNullOrEmpty(siteguid) ? siteguid : string.Empty,     // {0}
                household,                                                     // {1}
                price,                                                         // {2}  
                !string.IsNullOrEmpty(currency) ? currency : string.Empty,     // {3}
                contentId,                                                     // {4}
                productId,                                                     // {5}   
                transactionType.ToString(),                                    // {6}
                !string.IsNullOrEmpty(coupon) ? coupon : string.Empty,         // {7}
                !string.IsNullOrEmpty(userIp) ? userIp : string.Empty,         // {8}
                !string.IsNullOrEmpty(deviceName) ? deviceName : string.Empty, // {9}
                paymentGwId);                                                  // {10}

            log.Debug(logString);

            // validate siteguid
            if (string.IsNullOrEmpty(siteguid))
            {
                response.Status.Message = "Illegal user ID";
                log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                return response;
            }

            // validate household
            if (household < 1)
            {
                response.Status.Message = "Illegal household";
                log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                return response;
            }

            // validate currency
            if (string.IsNullOrEmpty(currency))
            {
                response.Status.Message = "Illegal currency";
                log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                return response;
            }

            // validate productId
            if (productId < 1)
            {
                response.Status.Message = "Illegal product ID";
                log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                return response;
            }

            try
            {
                // validate user
                ResponseStatus userValidStatus = ResponseStatus.OK;
                userValidStatus = Utils.ValidateUser(m_nGroupID, siteguid, ref household);

                if (userValidStatus != ResponseStatus.OK)
                {
                    // user validation failed
                    switch (userValidStatus)
                    {
                        case ResponseStatus.UserDoesNotExist:
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.UserDoesNotExist, "User doesn't exists");
                            break;
                        case ResponseStatus.UserSuspended:
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.UserSuspended, "Suspended user");
                            break;
                        case ResponseStatus.UserNotIndDomain:
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.UserNotInDomain, "User doesn't exist in household");
                            break;
                        default:
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed to validate user");
                            break;
                    }

                    log.ErrorFormat("User validation failed: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                // coupon validation
                bool isCouponValid = Utils.IsCouponValid(m_nGroupID, coupon);
                if (!isCouponValid)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.CouponNotValid, "Coupon not valid");
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                switch (transactionType)
                {
                    case eTransactionType.PPV:
                        response = PurchasePPV(siteguid, household, price, currency, contentId, productId, coupon, userIp, deviceName, paymentGwId);
                        break;
                    case eTransactionType.Subscription:
                        response = PurchaseSubscription(siteguid, household, price, currency, productId, coupon, userIp, deviceName, paymentGwId);
                        break;
                    case eTransactionType.Collection:
                        response = PurchaseCollection(siteguid, household, price, currency, productId, coupon, userIp, deviceName, paymentGwId);
                        break;
                    default:
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Illegal product ID");
                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Purchase Error. data: {0}", logString, ex));
            }

            return response;
        }

        private PurchaseResponse PurchaseCollection(string siteguid, long houseHoldId, double price, string currency, int productId, string coupon, string userIp, string deviceName, int paymentGwId)
        {
            PurchaseResponse response = new PurchaseResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // log request
            string logString = string.Format("Purchase request: siteguid {0}, household {1}, price {2}, currency {3}, productId {4}, coupon {5}, userIp {6}, deviceName {7}, paymentGwId {8}",
                !string.IsNullOrEmpty(siteguid) ? siteguid : string.Empty,     // {0}
                houseHoldId,                                                   // {1}
                price,                                                         // {2}  
                !string.IsNullOrEmpty(currency) ? currency : string.Empty,     // {3}
                productId,                                                     // {4}   
                !string.IsNullOrEmpty(coupon) ? coupon : string.Empty,         // {5}
                !string.IsNullOrEmpty(userIp) ? userIp : string.Empty,         // {6}
                !string.IsNullOrEmpty(deviceName) ? deviceName : string.Empty, // {7}
                paymentGwId);                                                  // {8}

            try
            {
                string country = string.Empty;
                if (!string.IsNullOrEmpty(userIp))
                {
                    // get country by user IP
                    country = TVinciShared.WS_Utils.GetIP2CountryCode(userIp);
                }

                // validate price
                PriceReason priceReason = PriceReason.UnKnown;
                TvinciPricing.Price priceResponse = null;
                TvinciPricing.Collection collection = null;
                bool isEntitledToPreviewModule = priceReason == PriceReason.EntitledToPreviewModule;
                priceResponse = Utils.GetCollectionFinalPrice(m_nGroupID, productId.ToString(), siteguid, coupon, ref priceReason,
                                                              ref collection, country, string.Empty, deviceName, string.Empty);

                if (priceReason == PriceReason.ForPurchase ||
                    isEntitledToPreviewModule)
                {
                    // item is for purchase
                    if (priceResponse != null &&
                        priceResponse.m_dPrice == price &&
                        priceResponse.m_oCurrency.m_sCurrencyCD3 == currency)
                    {
                        // price validated, create the Custom Data
                        string customData = GetCustomDataForCollection(collection, productId.ToString(), siteguid, price, currency, coupon,
                                                                       userIp, country, string.Empty, deviceName, string.Empty);

                        // create new GUID for billing_transaction
                        string billingGuid = Guid.NewGuid().ToString();

                        // purchase
                        response = HandlePurchase(siteguid, houseHoldId, price, currency, userIp, customData, productId, TvinciBilling.eTransactionType.Collection, billingGuid, paymentGwId, 0);
                        if (response != null &&
                            response.Status != null &&
                            response.Status.Code == (int)eResponseStatus.OK)
                        {
                            // grant entitlement
                            long purchaseID = 0;
                            bool handleBillingPassed = HandleCollectionBillingSuccess(siteguid, houseHoldId, collection, price, currency, coupon, userIp,
                                                                                      country, deviceName, response.TransactionID, customData, productId,
                                                                                      billingGuid, isEntitledToPreviewModule, ref purchaseID);

                            if (handleBillingPassed)
                            {
                                // entitlement passed - build notification message
                                var dicData = new Dictionary<string, object>()
                                {
                                    {"CollectionCode", productId},
                                    {"BillingTransactionID", response.TransactionID},
                                    {"SiteGUID", siteguid},
                                    {"PurchaseID", purchaseID},
                                    {"CouponCode", coupon},
                                    {"CustomData", customData}
                                };

                                // notify purchase
                                if (!this.EnqueueEventRecord(NotifiedAction.ChargedCollection, dicData))
                                {
                                    log.DebugFormat("Error while enqueue purchase record: {0}, data: {1}", response.Status.Message, logString);
                                }
                            }
                            else
                            {
                                // purchase passed, entitlement failed
                                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase passed but entitlement failed");
                                log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                            }
                        }
                        else
                        {
                            // purchase failed
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase failed");
                            log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                        }
                    }
                    else
                    {
                        // incorrect price
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.IncorrectPrice, "The price of the request is not the actual price");
                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    }
                }
                else
                {
                    // not for purchase
                    switch (priceReason)
                    {
                        case PriceReason.Free:
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Free, "The collection is already purchased");
                            break;
                        case PriceReason.SubscriptionPurchased:
                        case PriceReason.CollectionPurchased:
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Free, "The collection is already purchased");
                            break;
                        default:
                            break;
                    }

                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception occurred. data: " + logString, ex);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error);
            }
            return response;
        }


        private PurchaseResponse PurchaseSubscription(string siteguid, long houseHoldId, double price, string currency, int productId,
                                                      string coupon, string userIp, string deviceName, int paymentGwId)
        {
            PurchaseResponse response = new PurchaseResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // log request
            string logString = string.Format("Purchase request: siteguid {0}, household {1}, price {2}, currency {3}, productId {4}, coupon {5}, userIp {6}, deviceName {7}, paymentGwId {8}",
                !string.IsNullOrEmpty(siteguid) ? siteguid : string.Empty,     // {0}
                houseHoldId,                                                   // {1}
                price,                                                         // {2}  
                !string.IsNullOrEmpty(currency) ? currency : string.Empty,     // {3}
                productId,                                                     // {4}   
                !string.IsNullOrEmpty(coupon) ? coupon : string.Empty,         // {5}
                !string.IsNullOrEmpty(userIp) ? userIp : string.Empty,         // {6}
                !string.IsNullOrEmpty(deviceName) ? deviceName : string.Empty, // {7}
                paymentGwId);

            try
            {
                string country = string.Empty;
                if (!string.IsNullOrEmpty(userIp))
                {
                    // get country by user IP
                    country = TVinciShared.WS_Utils.GetIP2CountryCode(userIp);
                }

                // validate price
                PriceReason priceReason = PriceReason.UnKnown;
                bool entitleToPreview = priceReason == PriceReason.EntitledToPreviewModule;
                TvinciPricing.Subscription subscription = null;
                TvinciPricing.Price priceResponse = Utils.GetSubscriptionFinalPrice(m_nGroupID, productId.ToString(), siteguid, coupon, ref priceReason, ref subscription, country, string.Empty, deviceName);
                if (priceReason == PriceReason.ForPurchase ||
                    entitleToPreview)
                {
                    // item is for purchase
                    if (priceResponse != null &&
                        priceResponse.m_dPrice == price &&
                        priceResponse.m_oCurrency.m_sCurrencyCD3 == currency)
                    {
                        // price is validated, create custom data
                        string customData = GetCustomDataForSubscription(subscription, null, productId.ToString(), string.Empty, siteguid, price, currency,
                                                                         coupon, userIp, country, string.Empty, deviceName, string.Empty,
                                                                         entitleToPreview ? subscription.m_oPreviewModule.m_nID + "" : string.Empty,
                                                                         entitleToPreview);

                        // create new GUID for billing transaction
                        string billingGuid = Guid.NewGuid().ToString();

                        // purchase
                        response = HandlePurchase(siteguid, houseHoldId, price, currency, userIp, customData, productId,
                                                  TvinciBilling.eTransactionType.Subscription, billingGuid, paymentGwId, 0);
                        if (response != null &&
                            response.Status != null &&
                            response.Status.Code == (int)eResponseStatus.OK)
                        {
                            long purchaseID = 0;

                            // grant entitlement
                            bool handleBillingPassed = HandleSubscriptionBillingSuccess(siteguid, houseHoldId, subscription, price, currency, coupon, userIp,
                                                                                  country, deviceName, response.TransactionID, customData, productId,
                                                                                  billingGuid.ToString(), entitleToPreview, false, ref purchaseID);

                            if (handleBillingPassed)
                            {
                                // entitlement passed, update domain DLM with new DLM from subscription or if no DLM in new subscription, with last domain DLM
                                if (subscription.m_nDomainLimitationModule != 0)
                                {
                                    UpdateDLM(houseHoldId, subscription.m_nDomainLimitationModule);
                                }

                                // build notification message
                                var dicData = new Dictionary<string, object>()
                                {
                                    {"SubscriptionCode", productId},
                                    {"BillingTransactionID", response.TransactionID},
                                    {"SiteGUID", siteguid},
                                    {"PurchaseID", purchaseID},
                                    {"CouponCode", coupon},
                                    {"CustomData", customData}
                                };

                                // notify purchase
                                if (!this.EnqueueEventRecord(NotifiedAction.ChargedSubscription, dicData))
                                {
                                    log.ErrorFormat("Error while enqueue purchase record: {0}, data: {1}", response.Status.Message, logString);
                                }
                            }
                            else
                            {
                                // purchase passed, entitlement failed
                                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase passed but entitlement failed");
                                log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                            }
                        }
                        else
                        {
                            // purchase failed
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase failed");
                            log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                        }
                    }
                    else
                    {
                        // incorrect price
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.IncorrectPrice, "The price of the request is not the actual price");
                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    }
                }
                else
                {
                    // item not for purchase
                    switch (priceReason)
                    {
                        case PriceReason.Free:
                            response.Status.Message = string.Format("The subscription = {0} is already purchased", productId);
                            break;

                        case PriceReason.SubscriptionPurchased:
                        case PriceReason.CollectionPurchased:
                            response.Status.Message = string.Format("The subscription = {0} is already purchased", productId);
                            break;

                        default:
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
                            break;
                    }

                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }


        private PurchaseResponse PurchasePPV(string siteguid, long houseHoldId, double price, string currency, int contentId, int productId, string coupon, string userIp, string deviceName, int paymentGwId)
        {
            PurchaseResponse response = new PurchaseResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // log request
            string logString = string.Format("Purchase request: siteguid {0}, household {1}, price {2}, currency {3}, contentId {4}, productId {5}, coupon {6}, userIp {7}, deviceName {8}, paymentGwId {9}",
                !string.IsNullOrEmpty(siteguid) ? siteguid : string.Empty,     // {0}
                houseHoldId,                                                   // {1}
                price,                                                         // {2}  
                !string.IsNullOrEmpty(currency) ? currency : string.Empty,     // {3}
                contentId,                                                     // {4}
                productId,                                                     // {5}   
                !string.IsNullOrEmpty(coupon) ? coupon : string.Empty,         // {6}
                !string.IsNullOrEmpty(userIp) ? userIp : string.Empty,         // {7}
                !string.IsNullOrEmpty(deviceName) ? deviceName : string.Empty, // {8}
                paymentGwId);                                                  // {9}

            try
            {
                // validate content ID
                if (contentId < 1)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Illegal content ID");
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                // validate content ID related media
                int mediaID = ConditionalAccess.Utils.GetMediaIDFromFileID(contentId, m_nGroupID);
                if (mediaID < 1)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Content ID with a related media");
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                // validate PPV 
                TvinciPricing.PPVModule thePPVModule = null;
                ApiObjects.Response.Status status = ValidatePPVModuleCode(productId, contentId, ref thePPVModule);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    response.Status = status;
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                // validate price
                PriceReason ePriceReason = PriceReason.UnKnown;
                TvinciPricing.Subscription relevantSub = null;
                TvinciPricing.Collection relevantCol = null;
                TvinciPricing.PrePaidModule relevantPP = null;
                TvinciPricing.Price oPrice = Utils.GetMediaFileFinalPriceForNonGetItemsPrices(contentId, thePPVModule, siteguid, coupon, m_nGroupID,
                                                                                              ref ePriceReason, ref relevantSub, ref relevantCol, ref relevantPP,
                                                                                              string.Empty, string.Empty, deviceName);

                if (ePriceReason == PriceReason.ForPurchase ||
                   (ePriceReason == PriceReason.SubscriptionPurchased && oPrice.m_dPrice > 0))
                {
                    // item is for purchase
                    if (oPrice.m_dPrice == price && oPrice.m_oCurrency.m_sCurrencyCD3 == currency)
                    {
                        string country = string.Empty;
                        if (!string.IsNullOrEmpty(userIp))
                        {
                            // get country by user IP
                            country = TVinciShared.WS_Utils.GetIP2CountryCode(userIp);
                        }

                        // create custom data
                        string customData = GetCustomData(relevantSub, thePPVModule, null, siteguid, price, currency,
                                                          contentId, mediaID, productId.ToString(), string.Empty, coupon,
                                                          userIp, country, string.Empty, deviceName);

                        // create new GUID for billing transaction
                        string billingGuid = Guid.NewGuid().ToString();

                        // purchase
                        response = HandlePurchase(siteguid, houseHoldId, price, currency, userIp, customData, productId, TvinciBilling.eTransactionType.PPV, billingGuid, paymentGwId, contentId);
                        if (response != null &&
                            response.Status != null &&
                            response.Status.Code == (int)eResponseStatus.OK)
                        {
                            long purchaseId = 0;

                            // grant entitlement
                            bool handleBillingPassed = HandlePPVBillingSuccess(siteguid, houseHoldId, relevantSub, price, currency, coupon, userIp,
                                                                               country, deviceName, response.TransactionID, customData, thePPVModule,
                                                                               productId, contentId, billingGuid, ref purchaseId);

                            if (handleBillingPassed)
                            {
                                // entitlement passed - build notification message
                                var dicData = new Dictionary<string, object>()
                                {
                                    {"MediaFileID", contentId},
                                    {"BillingTransactionID", response.TransactionID},
                                    {"PPVModuleCode", productId},
                                    {"SiteGUID", siteguid},
                                    {"CouponCode", coupon},
                                    {"CustomData", customData},
                                    {"PurchaseID", purchaseId}
                                };

                                // notify purchase
                                if (!this.EnqueueEventRecord(NotifiedAction.ChargedMediaFile, dicData))
                                {
                                    log.DebugFormat("Error while enqueue purchase record: {0}, data: {1}", response.Status.Message, logString);
                                }
                            }
                            else
                            {
                                // purchase passed, entitlement failed
                                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase passed, entitlement failed");
                                log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                            }
                        }
                        else
                        {
                            // purchase failed
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase failed");
                            log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                        }
                    }
                    else
                    {
                        // incorrect price
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.IncorrectPrice, "The request price is incorrect");
                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    }
                }
                else
                {
                    // not for purchase
                    switch (ePriceReason)
                    {
                        case PriceReason.PPVPurchased:
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PPVPurchased, "PPV Already purchased");
                            break;
                        case PriceReason.Free:
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Free, "Free media");
                            break;
                        case PriceReason.ForPurchaseSubscriptionOnly:
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ForPurchaseSubscriptionOnly, "Subscription only");
                            break;
                        case PriceReason.SubscriptionPurchased:
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.SubscriptionPurchased, "Already purchased (subscription)");
                            break;
                        case PriceReason.NotForPurchase:
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NotForPurchase, "Not valid for purchased");
                            break;
                        default:
                            break;
                    }

                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error);
                log.Error("Exception occurred. data: " + logString, ex);
            }
            return response;
        }

        protected PurchaseResponse HandlePurchase(string siteGUID, long houseHoldID, double price, string currency, string userIP, string customData,
                                                  int productID, TvinciBilling.eTransactionType transactionType, string billingGuid, int paymentGWId, int contentId)
        {
            PurchaseResponse response = new PurchaseResponse();

            string logString = string.Format("fail get response from billing service siteGUID={0}, houseHoldID={1}, price={2}, currency={3}, userIP={4}, customData={5}, productID={6}, (int)transactionType={7}, billingGuid={8}, paymentGWId={9}",
                                        siteGUID,                 // {0}
                                        houseHoldID,              // {1}
                                        price,                    // {2}
                                        currency,                 // {3}
                                        userIP,                   // {4}
                                        customData,               // {5}
                                        productID,                // {6}
                                        (int)transactionType,     // {7}
                                        billingGuid.ToString(),   // {8}
                                        paymentGWId);             // {9}

            try
            {
                string userName = string.Empty;
                string password = string.Empty;
                TvinciBilling.module wsBillingService = null;
                InitializeBillingModule(ref wsBillingService, ref userName, ref password);

                // call new billing method for charge adapter
                var transactionResponse = wsBillingService.Transact(userName, password, siteGUID, (int)houseHoldID, price, currency, userIP, customData, productID, transactionType, contentId, billingGuid, paymentGWId);

                if (transactionResponse != null)
                {
                    // convert response to purchase response
                    response.PGReferenceID = transactionResponse.PGReferenceID != null ? transactionResponse.PGReferenceID : string.Empty;
                    response.PGResponseID = transactionResponse.PGResponseID != null ? transactionResponse.PGResponseID : string.Empty;
                    response.TransactionID = transactionResponse.TransactionID;
                    if (transactionResponse.Status != null)
                    {
                        response.Status = new ApiObjects.Response.Status((int)transactionResponse.Status.Code, transactionResponse.Status.Message);
                    }
                    else
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "No status returned from billing service");
                        log.Error("Received error from billing service. " + logString);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(logString, ex);
                response = new PurchaseResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }

        private ApiObjects.Response.Status ValidatePPVModuleCode(int productID, int contentID, ref PPVModule thePPVModule)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            try
            {
                string userName = string.Empty;
                string password = string.Empty;
                TvinciPricing.mdoule wsPricingService = new TvinciPricing.mdoule();
                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                {
                    wsPricingService.Url = sWSURL;
                }
                Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref userName, ref password);
                long ppvModuleCode = 0;
                long.TryParse(productID.ToString(), out ppvModuleCode);

                thePPVModule = wsPricingService.ValidatePPVModuleForMediaFile(userName, password, contentID, ppvModuleCode);

                if (thePPVModule == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "The ppv module is unknown");
                    return response;
                }

                if (!thePPVModule.m_sObjectCode.Equals(productID.ToString()))
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
    }

}
