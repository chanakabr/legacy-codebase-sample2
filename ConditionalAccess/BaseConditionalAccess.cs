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
using ConditionalAccess.TvinciPricing;
using System.Collections;


namespace ConditionalAccess
{
    public abstract class BaseConditionalAccess
    {
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


        protected abstract bool HandleChargeUserForSubscriptionBillingSuccess(string sSiteGUID, TvinciPricing.Subscription theSub,
            double dPrice, string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLanguageCode,
            string sDeviceName, TvinciBilling.BillingResponse br, bool bIsEntitledToPreviewModule, string sSubscriptionCode, string sCustomData,
            bool bIsRecurring, ref long lBillingTransactionID, ref long lPurchaseID);

        protected abstract bool HandleChargeUserForCollectionBillingSuccess(string sSiteGUID, TvinciPricing.Collection theCol,
            double dPrice, string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLanguageCode,
            string sDeviceName, TvinciBilling.BillingResponse br, string sCollectionCode,
            string sCustomData, ref long lBillingTransactionID, ref long lPurchaseID);

        protected abstract bool HandleChargeUserForMediaFileBillingSuccess(string sSiteGUID, TvinciPricing.Subscription relevantSub,
            double dPrice, string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLanguageCode,
            string sDeviceName, TvinciBilling.BillingResponse br, string sCustomData, TvinciPricing.PPVModule thePPVModule,
            long lMediaFileID, ref long lBillingTransactionID, ref long lPurchaseID);

        /*
         * This method was created in order to solve a bug in the flow of ChargeUserForMediaFile in Cinepolis.
         * 1. Cinepolis does not dummy charge their user. All transactions are recorded in their billing gateway,
         *    including transactions for the price of zero.
         * 2. However, since it is not a dummy charge (dummy charge is when you skip the step of contacting the billing gateway)
         *    the flow in the mentioned method did not reach the step where it contacts the billing gateway.
         * 3. This patch resolves this situation without changing any billing logic related to different customers.
         */
        protected abstract bool RecalculateDummyIndicatorForChargeMediaFile(bool bDummy, PriceReason reason, bool bIsCouponUsedAndValid);
        #endregion

        protected BaseConditionalAccess() { }

        protected BaseConditionalAccess(Int32 nGroupID)
            : this(nGroupID, string.Empty)
        {

        }

        protected BaseConditionalAccess(Int32 nGroupID, string connKey)
        {
            m_nGroupID = nGroupID;
            m_bIsInitialized = false;
            Initialize(connKey);
        }
        /// <summary>
        /// Initialize
        /// </summary>
        public void Initialize()
        {
            Initialize(string.Empty);
        }
        /// <summary>
        /// Initialize
        /// </summary>
        public void Initialize(string connectionKey)
        {
            //if (m_bIsInitialized == true)
            //return;
            if (m_sPurchaseMailTemplate == null)
                m_sPurchaseMailTemplate = "";
            lock (m_sPurchaseMailTemplate)
            {
                //if (m_bIsInitialized == true)
                //return;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                if (!string.IsNullOrEmpty(connectionKey))
                {
                    selectQuery.SetConnectionKey(connectionKey);
                }
                selectQuery.SetCachedSec(0);
                selectQuery += "select * from groups_parameters with (nolock) where status=1 and is_active=1 and ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                selectQuery += " order by id desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        object oPurchaseMail = selectQuery.Table("query").DefaultView[0].Row["PURCHASE_MAIL"];
                        object oMailFromName = selectQuery.Table("query").DefaultView[0].Row["MAIL_FROM_NAME"];
                        object oMailFromAdd = selectQuery.Table("query").DefaultView[0].Row["MAIL_FROM_ADD"];
                        object oMailServer = selectQuery.Table("query").DefaultView[0].Row["MAIL_SERVER"];
                        object oMailServerUN = selectQuery.Table("query").DefaultView[0].Row["MAIL_USER_NAME"];
                        object oMailServerPass = selectQuery.Table("query").DefaultView[0].Row["MAIL_PASSWORD"];
                        object oPurchaseMailSubject = selectQuery.Table("query").DefaultView[0].Row["PURCHASE_MAIL_SUBJECT"];
                        if (oPurchaseMail != null && oPurchaseMail != DBNull.Value)
                            m_sPurchaseMailTemplate = oPurchaseMail.ToString();
                        if (oPurchaseMailSubject != null && oPurchaseMailSubject != DBNull.Value)
                            m_sPurchaseMailSubject = oPurchaseMailSubject.ToString();
                        if (oMailFromName != null && oMailFromName != DBNull.Value)
                            m_sMailFromName = oMailFromName.ToString();
                        if (oMailFromAdd != null && oMailFromAdd != DBNull.Value)
                            m_sMailFromAdd = oMailFromAdd.ToString();
                        if (oMailServer != null && oMailServer != DBNull.Value)
                            m_sMailServer = oMailServer.ToString();
                        if (oMailServerUN != null && oMailServerUN != DBNull.Value)
                            m_sMailServerUN = oMailServerUN.ToString();
                        if (oMailServerPass != null && oMailServerPass != DBNull.Value)
                            m_sMailServerPass = oMailServerPass.ToString();
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
                m_bIsInitialized = true;
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
            TvinciAPI.PurchaseMailRequest retVal = new TvinciAPI.PurchaseMailRequest();
            string sFirstName = "";
            string sLastName = "";
            //TVinciShared.Mailer t = new TVinciShared.Mailer(0);
            //t.SetMailServer(m_sMailServer, m_sMailServerUN, m_sMailServerPass, m_sMailFromName, m_sMailFromAdd);
            TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";
            TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
            string sWSURL = Utils.GetWSURL("users_ws");
            if (sWSURL != "")
                u.Url = sWSURL;

            ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sUserGUID);
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
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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
            double taxDisc = 0;
            selectQuery.Finish();
            selectQuery = null;
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

            return retVal;
        }
        /// <summary>
        /// Send Mail
        /// </summary>
        protected void SendMail(string sText, string sEmail, Int32 nGroupID)
        {
            if (sText == "")
                return;
            string sMailData = sText;
            TVinciShared.Mailer t = new TVinciShared.Mailer(0);
            t.SetMailServer(m_sMailServer, m_sMailServerUN, m_sMailServerPass, m_sMailFromName, m_sMailFromAdd);
            t.SendMail(sEmail, "", sMailData, m_sPurchaseMailSubject);
        }
        /// <summary>
        /// Get Main Lanuage
        /// </summary>
        static protected Int32 GetMainLang(ref string sMainLang, ref string sMainLangCode, Int32 nGroupID)
        {
            Int32 nLangID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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
            selectQuery.Finish();
            selectQuery = null;
            return nLangID;
        }
        /// <summary>
        /// Get Current DB Time
        /// </summary>
        static public DateTime GetCurrentDBTime()
        {
            object t = DAL.ConditionalAccessDAL.GetCurrentDBTime();

            if (t != null && t != DBNull.Value)
            {
                return (DateTime)t;
            }

            return new DateTime();
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
                string sIP = "1.1.1.1";
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "WriteLog", "users", sIP, ref sWSUserName, ref sWSPass);
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
                Logger.Logger.Log("WriteToUserLog", string.Format("Failed to write to user log. Site Guid: {0} , Msg: {1} , Exception msg: {2} , Stack trace : {3}", sSiteGUID, sMessage, ex.Message, ex.StackTrace), GetLogFilename());
            }
            finally
            {
                if (u != null)
                {
                    u.Dispose();
                    u = null;
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
            ret.m_sRecieptCode = "";
            ret.m_sStatusDescription = "";
            if (string.IsNullOrEmpty(sSiteGUID))
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Cant charge an unknown user";
            }
            else
            {
                TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL != "")
                    u.Url = sWSURL;

                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                }
                else
                {

                    if (Utils.IsCouponValid(m_nGroupID, sCouponCode) == false)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                        ret.m_sRecieptCode = "";
                        ret.m_sStatusDescription = "Coupon not valid";
                        try { WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC):" + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription); }
                        catch { }
                        return ret;
                    }


                    sIP = "1.1.1.1";
                    sWSUserName = "";
                    sWSPass = "";

                    TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
                    sWSURL = Utils.GetWSURL("pricing_ws");
                    if (sWSURL != "")
                        m.Url = sWSURL;
                    TvinciPricing.PrePaidModule thePrePaidModule = null;

                    string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                    if (CachingManager.CachingManager.Exist("GetPrePaidModule " + sPrePaidModuleCode + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                        thePrePaidModule = (TvinciPricing.PrePaidModule)(CachingManager.CachingManager.GetCachedData("GetPrePaidModule " + sPrePaidModuleCode + "_" + m_nGroupID.ToString() + sLocaleForCache));
                    else
                    {
                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPrePaidModule", "pricing", sIP, ref sWSUserName, ref sWSPass);
                        thePrePaidModule = m.GetPrePaidModuleData(sWSUserName, sWSPass, int.Parse(sPrePaidModuleCode), sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        CachingManager.CachingManager.SetCachedData("GetPrePaidModule " + sPrePaidModuleCode + "_" + m_nGroupID.ToString() + sLocaleForCache, thePrePaidModule, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    }

                    if (thePrePaidModule == null)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownPPVModule;
                        ret.m_sRecieptCode = "";
                        ret.m_sStatusDescription = "This PrePaid Module does not exist ";
                        try { WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode.ToString() + " error returned: " + ret.m_sStatusDescription); }
                        catch { }
                    }
                    else
                    {
                        PriceReason theReason = PriceReason.UnKnown;

                        if (thePrePaidModule != null)
                        {
                            TvinciPricing.Price p = Utils.GetPrePaidFinalPrice(m_nGroupID, sPrePaidModuleCode, sSiteGUID, ref theReason, ref thePrePaidModule, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty, sCouponCode);
                            if (theReason == PriceReason.ForPurchase && p.m_dPrice > 0 || bDummy == true)
                            {
                                if (bDummy == true || (p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency))
                                {
                                    string sCustomData = "";
                                    if (p.m_dPrice != 0 || bDummy == true)
                                    {
                                        TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
                                        sWSUserName = "";
                                        sWSPass = "";
                                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "CC_ChargeUser", "billing", sIP, ref sWSUserName, ref sWSPass);
                                        sWSURL = Utils.GetWSURL("billing_ws");
                                        if (sWSURL != "")
                                            bm.Url = sWSURL;

                                        if (string.IsNullOrEmpty(sCountryCd) && !string.IsNullOrEmpty(sUserIP))
                                        {
                                            sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
                                        }

                                        //Create the Custom Data
                                        sCustomData = GetCustomDataForPrePaid(thePrePaidModule, null, sPrePaidModuleCode, string.Empty, sSiteGUID, dPrice, sCurrency, sCouponCode, sUserIP,
                                            sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                                        Logger.Logger.Log("CustomData", sCustomData, "CustomData");

                                        //customdata id
                                        if (bDummy == false)
                                            ret = bm.CC_ChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, 1, sExtraParameters, string.Empty, string.Empty);
                                        else
                                            ret = bm.CC_DummyChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, 1, sExtraParameters);
                                    }
                                    if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
                                    {
                                        HandleCouponUses(null, string.Empty, sSiteGUID, p.m_dPrice, sCurrency, 0, sCouponCode, sUserIP,
                                            sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, true, thePrePaidModule.m_ObjectCode, 0);

                                        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("pre_paid_purchases");
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
                                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", DateTime.Now);
                                        }

                                        insertQuery.Execute();
                                        insertQuery.Finish();
                                        insertQuery = null;
                                        try { WriteToUserLog(sSiteGUID, "Pre Paid Module ID: " + sPrePaidModuleCode + " Purchased(CC): " + dPrice.ToString() + sCurrency); }
                                        catch { }
                                        Int32 nPurchaseID = 0;
                                        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                        selectQuery += " select id from pre_paid_purchases where ";
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
                                        selectQuery.Finish();
                                        selectQuery = null;

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
                                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                                            updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                                            updateQuery += "where";
                                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                                            updateQuery.Execute();
                                            updateQuery.Finish();
                                            updateQuery = null;
                                            try
                                            {
                                                //send purchase mail
                                                string sEmail = "";
                                                string sPaymentMethod = "Credit Card";
                                                string sDateOfPurchase = GetDateSTRByGroup(DateTime.UtcNow, m_nGroupID);

                                                if (bDummy == false)
                                                {
                                                    TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
                                                    sWSUserName = "";
                                                    sWSPass = "";
                                                    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "CC_GetUserCCDigits", "billing", sIP, ref sWSUserName, ref sWSPass);
                                                    sWSURL = Utils.GetWSURL("billing_ws");
                                                    if (sWSURL != "")
                                                        bm.Url = sWSURL;
                                                    string sDigits = bm.CC_GetUserCCDigits(sWSUserName, sWSPass, sSiteGUID);
                                                    sPaymentMethod += " (************" + sDigits + ")";
                                                }
                                                else
                                                    sPaymentMethod = "Gift";
                                                TvinciAPI.PurchaseMailRequest sMailReq = GetPurchaseMailRequest(ref sEmail, sSiteGUID, thePrePaidModule.m_Title, sPaymentMethod, sDateOfPurchase, sReciept, dPrice, sCurrency, m_nGroupID);
                                                TvinciAPI.API apiWs = new TvinciAPI.API();
                                                string sAPIWSUserName = "";
                                                string sAPIWSPass = "";
                                                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetMail", "api", sIP, ref sAPIWSUserName, ref sAPIWSPass);
                                                string sAPIWSURL = Utils.GetWSURL("api_ws");
                                                if (sAPIWSURL != "")
                                                    apiWs.Url = sAPIWSURL;
                                                apiWs.SendMailTemplate(sAPIWSUserName, sWSPass, sMailReq);

                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Logger.Log("Send purchase mail", ex.Message + " | " + ex.StackTrace, "mailer");
                                            }
                                        }
                                        else
                                        {
                                            try { WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription); }
                                            catch { }
                                        }
                                    }
                                    else
                                    {
                                        try { WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription); }
                                        catch { }
                                    }
                                }
                                else
                                {
                                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                                    ret.m_sRecieptCode = "";
                                    ret.m_sStatusDescription = "The price of the request is not the actual price";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription); }
                                    catch { }
                                }
                            }
                            else
                            {
                                if (theReason == PriceReason.PPVPurchased)
                                {
                                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                    ret.m_sRecieptCode = "";
                                    ret.m_sStatusDescription = "The pre paid module is already purchased";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription); }
                                    catch { }
                                }
                                else if (theReason == PriceReason.Free)
                                {
                                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                    ret.m_sRecieptCode = "";
                                    ret.m_sStatusDescription = "The pre paid module is free";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription); }
                                    catch { }
                                }
                                else if (theReason == PriceReason.ForPurchaseSubscriptionOnly)
                                {
                                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                    ret.m_sRecieptCode = "";
                                    ret.m_sStatusDescription = "The pre paid module is for purchase with subscription only";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription); }
                                    catch { }
                                }
                                else if (theReason == PriceReason.SubscriptionPurchased)
                                {
                                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                    ret.m_sRecieptCode = "";
                                    ret.m_sStatusDescription = "The pre paid module is already purchased (subscription)";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription); }
                                    catch { }
                                }
                            }
                        }
                        else
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = "";
                            ret.m_sStatusDescription = "The ppv module is unknown";
                            try { WriteToUserLog(sSiteGUID, "While trying to purchase pre paid module(CC): " + sPrePaidModuleCode + " error returned: " + ret.m_sStatusDescription); }
                            catch { }
                        }
                    }
                }
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
            InAppRes.m_oBillingResponse.m_sRecieptCode = "";
            InAppRes.m_oBillingResponse.m_sStatusDescription = "";

            if (sSiteGUID == "")
            {
                InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                InAppRes.m_oBillingResponse.m_sStatusDescription = "Cant charge an unknown user";
            }
            else
            {
                #region Init Tvinci Users Webservice
                TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL != "")
                    u.Url = sWSURL;
                #endregion

                //get user data
                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    //return UnKnownUser 
                    InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                    InAppRes.m_oBillingResponse.m_sStatusDescription = "Cant charge an unknown user";
                }
                else
                {
                    sIP = "1.1.1.1";
                    sWSUserName = "";
                    sWSPass = "";

                    #region Init Tvinci Pricing Webservice
                    TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
                    sWSURL = Utils.GetWSURL("pricing_ws");
                    if (sWSURL != "")
                        m.Url = sWSURL;
                    #endregion

                    Int32[] nMediaFiles = { nMediaFileID };

                    string sMediaFileForCache = Utils.ConvertArrayIntToStr(nMediaFiles);

                    TvinciPricing.MediaFilePPVModule[] oModules = null;

                    //create local chache string
                    string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                    if (CachingManager.CachingManager.Exist("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                    {
                        //retrive MediaFilePPVModule from cahce
                        oModules = (TvinciPricing.MediaFilePPVModule[])(CachingManager.CachingManager.GetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache));
                    }
                    else
                    {
                        //set username and password credential for pricing webservice.
                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleListForMediaFiles", "pricing", sIP, ref sWSUserName, ref sWSPass);
                        //execute webservice 
                        oModules = m.GetPPVModuleListForMediaFiles(sWSUserName, sWSPass, nMediaFiles, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        //add resault to cache
                        CachingManager.CachingManager.SetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache, oModules, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    }

                    #region check PPVModuleCode belong to item
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
                        InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownPPVModule;
                        InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                        InAppRes.m_oBillingResponse.m_sStatusDescription = "This PPVModule does not belong to item";
                        try
                        {
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription);
                        }
                        catch
                        {
                        }
                    }
                    #endregion

                    if (bOK == true)
                    {
                        PriceReason theReason = PriceReason.UnKnown;

                        TvinciPricing.Subscription relevantSub = null;
                        TvinciPricing.Collection relevantCol = null;
                        TvinciPricing.PrePaidModule relevantPP = null;

                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                        TvinciPricing.PPVModule thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (thePPVModule != null)
                        {
                            TvinciPricing.Price p = Utils.GetMediaFileFinalPrice(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                            if (theReason == PriceReason.ForPurchase || (theReason == PriceReason.SubscriptionPurchased && p.m_dPrice > 0))
                            {
                                if (p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency)
                                {
                                    string sCustomData = "";
                                    if (p.m_dPrice != 0)
                                    {
                                        #region Init Tvinci Billing Webservice
                                        TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
                                        sWSUserName = "";
                                        sWSPass = "";
                                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "CC_ChargeUser", "billing", sIP, ref sWSUserName, ref sWSPass);
                                        sWSURL = Utils.GetWSURL("billing_ws");
                                        if (sWSURL != "")
                                            bm.Url = sWSURL;
                                        #endregion

                                        if (string.IsNullOrEmpty(sCountryCd) && !string.IsNullOrEmpty(sUserIP))
                                        {
                                            sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
                                        }

                                        //Create the Custom Data
                                        sCustomData = GetCustomData(relevantSub, thePPVModule, null, sSiteGUID, dPrice, sCurrency,
                                            nMediaFileID, nMediaID, sPPVModuleCode, string.Empty, sCouponCode, sUserIP,
                                            sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                                        Logger.Logger.Log("CustomData", sCustomData, "CustomData");

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
                                        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ppv_purchases");
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
                                        insertQuery.Finish();
                                        insertQuery = null;
                                        try
                                        {
                                            WriteToUserLog(sSiteGUID, "Media file id: " + nMediaFileID.ToString() + " Purchased(CC): " + dPrice.ToString() + sCurrency);
                                        }
                                        catch
                                        {
                                        }
                                        #endregion
                                        #region Select - ppv_purchases the current purchase
                                        Int32 nPurchaseID = 0;
                                        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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
                                        selectQuery += "order by id desc";
                                        if (selectQuery.Execute("query", true) != null)
                                        {
                                            Int32 nCount1 = selectQuery.Table("query").DefaultView.Count;
                                            if (nCount1 > 0)
                                                nPurchaseID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                                        }
                                        selectQuery.Finish();
                                        selectQuery = null;
                                        #endregion

                                        //Should update the PURCHASE_ID

                                        string sReciept = InAppRes.m_oBillingResponse.m_sRecieptCode;
                                        if (sReciept != "")
                                        {
                                            Int32 nID = int.Parse(sReciept);
                                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                                            updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                                            updateQuery += "where";
                                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                                            updateQuery.Execute();
                                            updateQuery.Finish();
                                            updateQuery = null;
                                        }
                                        else
                                        {
                                            try
                                            { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription); }
                                            catch { }
                                        }
                                    }
                                }
                                else
                                {
                                    InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                                    InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                    InAppRes.m_oBillingResponse.m_sStatusDescription = "The price of the request is not the actual price";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription); }
                                    catch { }
                                }
                            }
                            else
                            {
                                if (theReason == PriceReason.PPVPurchased)
                                {
                                    InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                    InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                    InAppRes.m_oBillingResponse.m_sStatusDescription = "The media file is already purchased";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(InApp): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription); }
                                    catch { }
                                }
                                else if (theReason == PriceReason.Free)
                                {
                                    InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                    InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                    InAppRes.m_oBillingResponse.m_sStatusDescription = "The media file is free";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(InApp): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription); }
                                    catch { }
                                }
                                else if (theReason == PriceReason.ForPurchaseSubscriptionOnly)
                                {
                                    InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                    InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                    InAppRes.m_oBillingResponse.m_sStatusDescription = "The media file is for purchase with subscription only";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(InApp): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription); }
                                    catch { }
                                }
                                else if (theReason == PriceReason.SubscriptionPurchased)
                                {
                                    InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                    InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                    InAppRes.m_oBillingResponse.m_sStatusDescription = "The media file is already purchased (subscription)";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(InApp): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription); }
                                    catch { }
                                }
                            }
                        }
                        else
                        {
                            InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                            InAppRes.m_oBillingResponse.m_sStatusDescription = "The ppv module is unknown";
                            try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(InApp): " + nMediaFileID.ToString() + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription); }
                            catch { }
                        }
                    }
                }
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
            //TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            TvinciBilling.InAppBillingResponse InAppRes = new TvinciBilling.InAppBillingResponse();
            InAppRes.m_oBillingResponse = new TvinciBilling.BillingResponse();

            if (sSiteGUID == "")
            {
                InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                InAppRes.m_oBillingResponse.m_sStatusDescription = "Cant charge an unknown user";
            }
            else
            {
                TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL != "")
                    u.Url = sWSURL;

                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    InAppRes.m_oBillingResponse.m_sRecieptCode = "";
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
                            string sCustomData = "";
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

                            Logger.Logger.Log("CustomData", sCustomData, "CustomDataForSubsrpition");

                            if (p.m_dPrice != 0)
                            {
                                TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
                                sWSUserName = "";
                                sWSPass = "";
                                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "CC_ChargeUser", "billing", sIP, ref sWSUserName, ref sWSPass);
                                sWSURL = Utils.GetWSURL("billing_ws");
                                if (sWSURL != "")
                                    bm.Url = sWSURL;

                                InAppRes = bm.InApp_ChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, nRecPeriods, ReceiptData);
                            }

                            if (InAppRes.m_oBillingResponse.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
                            {
                                Int32 nReciptCode = 0;
                                if (!string.IsNullOrEmpty(InAppRes.m_oBillingResponse.m_sRecieptCode))
                                {
                                    nReciptCode = int.Parse(InAppRes.m_oBillingResponse.m_sRecieptCode);
                                }
                                Int32 nRet = 0;
                                ODBCWrapper.DataSetSelectQuery selectExistQuery = new ODBCWrapper.DataSetSelectQuery();
                                selectExistQuery += " select id from subscriptions_purchases where ";
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

                                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                                    updateQuery += " where ";
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                                    updateQuery += " and ";
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                                    updateQuery += " and ";
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                    updateQuery.Execute();
                                    updateQuery.Finish();
                                    updateQuery = null;

                                    DateTime dt1970 = new DateTime(1970, 1, 1);

                                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscriptions_purchases");
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

                                    if (InAppRes.m_oInAppReceipt.latest_receipt_info != null && !string.IsNullOrEmpty(InAppRes.m_oInAppReceipt.latest_receipt_info.expires_date) && !string.IsNullOrEmpty(InAppRes.m_oInAppReceipt.latest_receipt_info.purchase_date_ms))
                                    {
                                        double dEnd = double.Parse(InAppRes.m_oInAppReceipt.latest_receipt_info.expires_date);
                                        double dStart = double.Parse(InAppRes.m_oInAppReceipt.latest_receipt_info.purchase_date_ms);

                                        DateTime dStartDate = dt1970.AddMilliseconds(dStart);
                                        DateTime dEndDate = dt1970.AddMilliseconds(dEnd);

                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dEndDate.AddHours(6));
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", dStartDate);
                                    }
                                    else
                                    {
                                        double dEnd = double.Parse(InAppRes.m_oInAppReceipt.receipt.expires_date);
                                        double dStart = double.Parse(InAppRes.m_oInAppReceipt.receipt.purchase_date_ms);

                                        DateTime dStartDate = dt1970.AddMilliseconds(dStart);
                                        DateTime dEndDate = dt1970.AddMilliseconds(dEnd);

                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dEndDate.AddHours(6));
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", dStartDate);

                                    }
                                    insertQuery.Execute();
                                    insertQuery.Finish();
                                    insertQuery = null;

                                    Int32 nPurchaseID = 0;
                                    try { WriteToUserLog(sSiteGUID, "Subscription purchase (CC): " + sSubscriptionCode); }
                                    catch { }
                                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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
                                    selectQuery += " order by id desc";
                                    if (selectQuery.Execute("query", true) != null)
                                    {
                                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                                        if (nCount > 0)
                                        {
                                            nPurchaseID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                                        }
                                    }
                                    selectQuery.Finish();
                                    selectQuery = null;

                                    if (nReciptCode > 0)
                                    {
                                        ODBCWrapper.UpdateQuery updateQuery1 = new ODBCWrapper.UpdateQuery("billing_transactions");
                                        updateQuery1.SetConnectionKey("MAIN_CONNECTION_STRING");
                                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                                        updateQuery1 += "where";
                                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nReciptCode);
                                        updateQuery1.Execute();
                                        updateQuery1.Finish();
                                        updateQuery1 = null;
                                    }
                                    #region Old Code Send Mail
                                    //    //Send purchase mail
                                    //    try
                                    //    {
                                    //        string sMainLang = "";
                                    //        string sMainLangCode = "";
                                    //        GetMainLang(ref sMainLang, ref sMainLangCode, m_nGroupID);
                                    //        //send purchase mail
                                    //        string sEmail = "";
                                    //        string sPaymentMethod = string.Empty;
                                    //        if (dPrice > 0)
                                    //        {
                                    //            sPaymentMethod = "Credit Card";
                                    //        }
                                    //        else if (!string.IsNullOrEmpty(sCouponCode))
                                    //        {
                                    //            sPaymentMethod = "Coupon";
                                    //        }
                                    //        string sDateOfPurchase = GetDateSTRByGroup(DateTime.UtcNow, m_nGroupID);
                                    //        string sItemName = "";
                                    //        if (theSub.m_sName != null)
                                    //        {
                                    //            Int32 nNameLangLength = theSub.m_sName.Length;
                                    //            for (int i = 0; i < nNameLangLength; i++)
                                    //            {
                                    //                string sLang = theSub.m_sName[i].m_sLanguageCode3;
                                    //                string sVal = theSub.m_sName[i].m_sValue;
                                    //                if (sLang == sMainLangCode)
                                    //                    sItemName = sVal;
                                    //            }
                                    //        }
                                    //        if (bDummy == false && dPrice > 0)
                                    //        {
                                    //            TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
                                    //            sWSUserName = "";
                                    //            sWSPass = "";
                                    //            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "CC_GetUserCCDigits", "billing", sIP, ref sWSUserName, ref sWSPass);
                                    //            sWSURL = Utils.GetWSURL("billing_ws");
                                    //            if (sWSURL != "")
                                    //                bm.Url = sWSURL;
                                    //            string sDigits = bm.CC_GetUserCCDigits(sWSUserName, sWSPass, sSiteGUID);
                                    //            sPaymentMethod += " (************" + sDigits + ")";
                                    //        }
                                    //        else if (bDummy)
                                    //            sPaymentMethod = "Gift";
                                    //        string sMailText = GetPurchaseMailText(ref sEmail, sSiteGUID, sItemName, sPaymentMethod, sDateOfPurchase, nReciptCode.ToString(), String.Format("{0:0.##}", dPrice) + " " + sCurrency, m_nGroupID);
                                    //        SendMail(sMailText, sEmail, m_nGroupID);
                                    //    }
                                    //    catch (Exception ex)
                                    //    {
                                    //        Logger.Logger.Log("Send purchase mail", ex.Message + " | " + ex.StackTrace, "mailer");
                                    //    }

                                    //}
                                    //else
                                    //{
                                    //    try { WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription); }
                                    //    catch { }
                                    //}
                                    #endregion
                                }
                            }
                            else
                            {
                                InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                                InAppRes.m_oBillingResponse.m_sStatusDescription = InAppRes.m_oBillingResponse.m_sStatusDescription;
                                try { WriteToUserLog(sSiteGUID, "while trying to purchase subscription(InApp): " + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription); }
                                catch { }
                            }
                        }
                        else
                        {
                            InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                            InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                            InAppRes.m_oBillingResponse.m_sStatusDescription = "The price of the request is not the actual price";
                            try { WriteToUserLog(sSiteGUID, "while trying to purchase subscription(InApp): " + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription); }
                            catch { }
                        }
                    }
                    else
                    {
                        if (theReason == PriceReason.Free)
                        {
                            InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                            InAppRes.m_oBillingResponse.m_sStatusDescription = "The subscription is free";
                            try { WriteToUserLog(sSiteGUID, "while trying to purchase subscription(InApp): " + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription); }
                            catch { }
                        }
                        if (theReason == PriceReason.SubscriptionPurchased)
                        {
                            InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                            InAppRes.m_oBillingResponse.m_sStatusDescription = "The subscription is already purchased";
                            try { WriteToUserLog(sSiteGUID, "while trying to purchase subscription(InApp): " + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription); }
                            catch { }
                        }
                        if (theReason == PriceReason.UnKnown)
                        {
                            InAppRes.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            InAppRes.m_oBillingResponse.m_sRecieptCode = "";
                            InAppRes.m_oBillingResponse.m_sStatusDescription = "Error Unkown";
                            try { WriteToUserLog(sSiteGUID, "while trying to purchase subscription(InApp): " + " error returned: " + InAppRes.m_oBillingResponse.m_sStatusDescription); }
                            catch { }
                        }
                    }
                }
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
            PriceReason theReason = PriceReason.UnKnown;
            TvinciPricing.Subscription theSub = null;
            TvinciPricing.Price p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubscriptionCode, sSiteGUID, string.Empty, ref theReason, ref theSub, "", "", "");
            bool bIsRecurring = false;
            if (theSub != null && theSub.m_oUsageModule != null)
                bIsRecurring = theSub.m_bIsRecurring;
            if (bIsRecurring == true)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from subscriptions_purchases where IS_ACTIVE=1 and STATUS=1 and RECURRING_RUNTIME_STATUS=0 and ";
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
                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 1);
                        updateQuery += " where ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                        updateQuery.Execute();
                        updateQuery.Finish();
                        updateQuery = null;
                        bRet = true;

                        //Insert renew subscription row
                        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscriptions_status_changes");
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
                        insertQuery.Finish();
                        insertQuery = null;

                        //Write to users log
                        try { WriteToUserLog(sSiteGUID, "Subscription: " + sSubscriptionCode.ToString() + " renew activated"); }
                        catch { }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            return bRet;
        }
        /// <summary>
        /// Cancel Subscription
        /// </summary>
        public virtual bool CancelSubscription(string sSiteGUID, string sSubscriptionCode, Int32 nSubscriptionPurchaseID)
        {
            bool bRet = true;
            TvinciPricing.Subscription theSub = null;
            PriceReason theReason = PriceReason.UnKnown;

            TvinciPricing.Price p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubscriptionCode, sSiteGUID, string.Empty, ref theReason, ref theSub, "", "", "");

            if (theSub != null && theSub.m_oUsageModule != null && theSub.m_bIsRecurring)
            {
                DataTable dt = DAL.ConditionalAccessDAL.GetSubscriptionPurchaseID(nSubscriptionPurchaseID);
                if (dt != null)
                {
                    Int32 nCount = dt.Rows.Count;
                    if (nCount > 0)
                    {
                        DataRow dr = dt.Rows[0];
                        Int32 nID = ODBCWrapper.Utils.GetIntSafeVal(dr["ID"]);

                        bRet = DAL.ConditionalAccessDAL.CancelSubscription(nID, m_nGroupID, sSiteGUID, sSubscriptionCode) != 0 ? false : bRet;

                        WriteToUserLog(sSiteGUID, "Subscription: " + sSubscriptionCode.ToString() + " renew cancelled");
                    }
                }
            }
            return bRet;
        }
        /// <summary>
        /// Update Subscription
        /// </summary>
        public virtual bool UpdateSubscriptionDate(string sSiteGUID, string sSubscriptionCode, Int32 nSubscriptionPurchaseID, Int32 dAdditionInDays, bool bRenewable)
        {
            bool bRet = false;
            PriceReason theReason = PriceReason.UnKnown;
            TvinciPricing.Subscription theSub = null;
            TvinciPricing.Price p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubscriptionCode, sSiteGUID, string.Empty, ref theReason, ref theSub, "", "", "");
            bool bIsRecurring = false;
            if (theSub != null && theSub.m_oUsageModule != null)
                bIsRecurring = theSub.m_bIsRecurring;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from subscriptions_purchases where IS_ACTIVE=1 and STATUS=1 and RECURRING_RUNTIME_STATUS=0 and ";
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
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                    if (bIsRecurring == true && bRenewable == true)
                        RenewCacledSubscription(sSiteGUID, sSubscriptionCode, nSubscriptionPurchaseID);
                    if (bIsRecurring == true && bRenewable == false)
                        CancelSubscription(sSiteGUID, sSubscriptionCode, nSubscriptionPurchaseID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dEndDate);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                    bRet = true;
                    try { WriteToUserLog(sSiteGUID, "Subscription: " + sSubscriptionCode.ToString() + "End date changed(" + dCurrentEndDate.ToString("MM/dd/yyyy HH:mm") + "-->" + dEndDate.ToString("MM/dd/yyyy HH:mm") + ")"); }
                    catch { }
                    //Write to users log
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return bRet;
        }
        /// <summary>
        /// Credit Card Renew Subscription
        /// </summary>
        public TvinciBilling.BillingResponse CC_BaseRenewSubscription(string sSiteGUID, double dPrice, string sCurrency,
            string sSubscriptionCode, string sUserIP, string sExtraParams, Int32 nPurchaseID, Int32 nPaymentNumber,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            string sCouponCode = "";
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            if (sSiteGUID == "")
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Cant charge an unknown user";
            }
            else
            {
                TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL != "")
                    u.Url = sWSURL;


                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                    try { WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " error returned: " + ret.m_sStatusDescription); }
                    catch { }
                }
                else
                {
                    TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule();
                    if (Utils.GetWSURL("pricing_ws") != "")
                        m.Url = Utils.GetWSURL("pricing_ws");

                    TvinciPricing.Subscription theSub = null;
                    string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                    if (CachingManager.CachingManager.Exist("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                        theSub = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache));
                    else
                    {
                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetSubscriptionData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                        theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubscriptionCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);
                        CachingManager.CachingManager.SetCachedData("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache, theSub, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    }
                    if (theSub != null && theSub.m_nNumberOfRecPeriods != 0 && theSub.m_nNumberOfRecPeriods <= nPaymentNumber)
                    {
                        ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                        directQuery.SetConnectionKey("CA_CONNECTION_STRING");
                        directQuery += "update subscriptions_purchases set ";
                        directQuery += "IS_RECURRING_STATUS = 0 ";
                        directQuery += " where ";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                        directQuery.Execute();
                        directQuery.Finish();
                        directQuery = null;

                    }
                    else if (theSub != null)
                    {
                        string sCustomData = "";
                        if (dPrice != 0)
                        {
                            TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
                            sWSUserName = "";
                            sWSPass = "";
                            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "CC_ChargeUser", "billing", sIP, ref sWSUserName, ref sWSPass);
                            sWSURL = Utils.GetWSURL("billing_ws");
                            if (sWSURL != "")
                                bm.Url = sWSURL;

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
                            //customdata id
                            //int scTransactionID = GetRenewalTransactionID(sSiteGUID, sSubscriptionCode, m_nGroupID);
                            //ret = bm.CC_RenewChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber, nRecPeriods, sExtraParams, scTransactionID);
                            ret = bm.CC_ChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber, nRecPeriods, sExtraParams, string.Empty, string.Empty);
                        }
                    }
                    if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
                    {
                        Int32 nMaxVLC = theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle;
                        try { WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " renewed" + dPrice.ToString() + sCurrency); }
                        catch { }
                        DateTime d = (DateTime)(ODBCWrapper.Utils.GetTableSingleVal("subscriptions_purchases", "end_date", nPurchaseID, "CA_CONNECTION_STRING"));
                        DateTime dNext = Utils.GetEndDateTime(d, nMaxVLC);
                        ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                        directQuery.SetConnectionKey("CA_CONNECTION_STRING");
                        directQuery += "update subscriptions_purchases set ";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", "=", dNext);
                        directQuery += ",";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("num_of_uses", "=", 0);
                        directQuery += " where ";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                        directQuery.Execute();
                        directQuery.Finish();
                        directQuery = null;

                        string sReciept = ret.m_sRecieptCode;
                        if (sReciept != "")
                        {
                            Int32 nID = int.Parse(sReciept);
                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                            updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                            updateQuery += "where";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;
                        }
                    }
                    else
                    {
                        try { WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " error returned: " + ret.m_sStatusDescription); }
                        catch { }

                        if (ret.m_oStatus != ConditionalAccess.TvinciBilling.BillingResponseStatus.ExternalError)
                        {
                            if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.ExpiredCard)
                            {
                                ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                                directQuery.SetConnectionKey("CA_CONNECTION_STRING");
                                directQuery += "update subscriptions_purchases set ";
                                directQuery += "FAIL_COUNT = 10 ";
                                directQuery += " where ";
                                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                                directQuery.Execute();
                                directQuery.Finish();
                                directQuery = null;
                            }
                            ODBCWrapper.DirectQuery directQuery2 = new ODBCWrapper.DirectQuery();
                            directQuery2.SetConnectionKey("CA_CONNECTION_STRING");
                            directQuery2 += "update subscriptions_purchases set ";
                            directQuery2 += "FAIL_COUNT = FAIL_COUNT + 1 ";
                            directQuery2 += " where ";
                            directQuery2 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                            directQuery2.Execute();
                            directQuery2.Finish();
                            directQuery2 = null;


                        }
                        #region Add notification request
                        // NotificationService.NotificationService client = new NotificationService.NotificationService();

                        //// NotificationService.NotificationServiceClient ns = new NotificationService.NotificationServiceClient("ElisaHttpBinding_INotificationService", Utils.GetWSURL("notification_wcf"));
                        // string nsWSUserName = "";
                        // string nsWSPass = "";
                        // TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "CC_BaseRenewSubscription", "notifications", sIP, ref nsWSUserName, ref nsWSPass);
                        // //client.AddNotificationRequest(AddNotificationRequest(nsWSUserName, nsWSPass, long.Parse(sSiteGUID), NotificationService.NotificationTriggerType.PaymentFailure);
                        #endregion
                    }
                }

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
            Logger.Logger.Log("Renew Fail", sSiteGUID + " " + sSubscriptionCode, "TempRenew");
            string sCouponCode = "";
            TvinciBilling.InAppBillingResponse ret = new TvinciBilling.InAppBillingResponse(); // new ConditionalAccess.TvinciBilling.InAppBillingResponse();
            if (sSiteGUID == "")
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
                TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL != "")
                    u.Url = sWSURL;

                #endregion

                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    #region terminate if ResponseStatus NOT Ok.
                    ret.m_oBillingResponse = new TvinciBilling.BillingResponse();
                    ret.m_oBillingResponse.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_oBillingResponse.m_sRecieptCode = "";
                    ret.m_oBillingResponse.m_sStatusDescription = "Cant charge an unknown user";
                    ret.m_oInAppReceipt = null;
                    try { WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " error returned: " + ret.m_oBillingResponse.m_sStatusDescription); }
                    catch { }
                    #endregion
                }
                else
                {
                    #region Init Tvinci Pricing web service
                    TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule();
                    if (Utils.GetWSURL("pricing_ws") != "")
                        m.Url = Utils.GetWSURL("pricing_ws");
                    #endregion

                    TvinciPricing.Subscription theSub = null;
                    //Get local string for cache
                    string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                    if (CachingManager.CachingManager.Exist("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                        //return the subscription from cache
                        theSub = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache));
                    else
                    {
                        //**********************
                        //
                        //**********************

                        //Get  pricing username and password
                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetSubscriptionData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                        //get subscription date
                        theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubscriptionCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);
                        //add subscription date to cache
                        CachingManager.CachingManager.SetCachedData("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache, theSub, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    }

                    if (theSub != null && theSub.m_nNumberOfRecPeriods != 0 && theSub.m_nNumberOfRecPeriods <= nPaymentNumber)
                    {
                        #region Update subscription purchasesto is recurring status = 0
                        ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                        directQuery.SetConnectionKey("CA_CONNECTION_STRING");
                        directQuery += "update subscriptions_purchases set ";
                        directQuery += "IS_RECURRING_STATUS = 0 ";
                        directQuery += " where ";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                        directQuery.Execute();
                        directQuery.Finish();
                        directQuery = null;
                        #endregion

                    }
                    else if (theSub != null && theSub.m_oSubscriptionUsageModule != null)
                    {
                        string sCustomData = "";
                        if (dPrice != 0)
                        {
                            #region Init Billing web service
                            TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
                            sWSUserName = "";
                            sWSPass = "";
                            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "InApp_ChargeUser", "billing", sIP, ref sWSUserName, ref sWSPass);
                            sWSURL = Utils.GetWSURL("billing_ws");
                            if (sWSURL != "")
                                bm.Url = sWSURL;
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
                        try { WriteToUserLog(sSiteGUID, "Subscription InApp renewal: " + sSubscriptionCode.ToString() + " renewed" + dPrice.ToString() + sCurrency); }
                        catch { }

                        DateTime dEndDate = new DateTime(1970, 1, 1);

                        if (ret.m_oInAppReceipt.latest_receipt_info != null && !string.IsNullOrEmpty(ret.m_oInAppReceipt.latest_receipt_info.expires_date))
                        {
                            double dEnd = double.Parse(ret.m_oInAppReceipt.latest_receipt_info.expires_date);

                            dEndDate = dEndDate.AddMilliseconds(dEnd);

                        }

                        #region update subscription purchases
                        ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                        directQuery.SetConnectionKey("CA_CONNECTION_STRING");
                        directQuery += "update subscriptions_purchases set ";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", "=", dEndDate.AddHours(6));
                        //directQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", "=", dEndDate.ToString());
                        directQuery += ",";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("num_of_uses", "=", 0);
                        directQuery += " where ";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                        directQuery.Execute();
                        directQuery.Finish();
                        directQuery = null;
                        #endregion

                        string sReciept = ret.m_oBillingResponse.m_sRecieptCode;

                        if (sReciept != "")
                        {
                            Int32 nID = int.Parse(sReciept);
                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                            updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                            updateQuery += "where";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;
                        }
                    }
                    else
                    {
                        Logger.Logger.Log("Fail", "Fail count for user " + sSiteGUID, "InAppPurchase");
                        try { WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " error returned: " + ret.m_oBillingResponse.m_sStatusDescription); }
                        catch { }

                        if (ret.m_oBillingResponse.m_oStatus != ConditionalAccess.TvinciBilling.BillingResponseStatus.ExternalError)
                        {
                            if (ret.m_oBillingResponse.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.ExpiredCard)
                            {
                                #region Update subscription purchases to fail count = 10
                                ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                                directQuery.SetConnectionKey("CA_CONNECTION_STRING");
                                directQuery += "update subscriptions_purchases set ";
                                directQuery += "FAIL_COUNT = 10 ";
                                directQuery += " where ";
                                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                                directQuery.Execute();
                                directQuery.Finish();
                                directQuery = null;
                                #endregion
                            }

                            #region Increase subscription purchase fail count
                            ODBCWrapper.DirectQuery directQuery2 = new ODBCWrapper.DirectQuery();
                            directQuery2.SetConnectionKey("CA_CONNECTION_STRING");
                            directQuery2 += "update subscriptions_purchases set ";
                            directQuery2 += "FAIL_COUNT = FAIL_COUNT + 1 ";
                            directQuery2 += " where ";
                            directQuery2 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                            directQuery2.Execute();
                            directQuery2.Finish();
                            directQuery2 = null;
                            #endregion
                        }
                    }
                }

            }
            return ret;
        }
        private UserResponseObject GetExistUser(string sSiteGUID)
        {
            #region Init Tvinci Users web service
            TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
            string sIP = "1.1.1.1";
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;

            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetExistUser", "users", sIP, ref sWSUserName, ref sWSPass);
            string sWSURL = Utils.GetWSURL("users_ws");
            if (sWSURL != "")
                u.Url = sWSURL;
            #endregion

            ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
            return uObj;
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
                    string sIP = "1.1.1.1";
                    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetSubscriptionData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                    m = new TvinciPricing.mdoule();
                    string sWSURL = Utils.GetWSURL("cloud_pricing_ws");
                    if (sWSURL.Length > 0)
                        m.Url = sWSURL;

                    TvinciPricing.Subscription theSub = null;
                    string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                    sLocaleForCache = string.Format("GetSubscriptionData{0}_{1}{2}", sSubscriptionCode, m_nGroupID.ToString(), sLocaleForCache);

                    if (CachingManager.CachingManager.Exist("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                    {
                        theSub = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData(sLocaleForCache));
                    }
                    else
                    {
                        theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubscriptionCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);
                        CachingManager.CachingManager.SetCachedData(sLocaleForCache, theSub, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    }

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

            }
            else
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Direct Deipt Renew Subscription
        /// </summary>
        public virtual TvinciBilling.BillingResponse DD_BaseRenewMultiUsageSubscription(string sSiteGUID, string sSubscriptionCode, string sUserIP, string sExtraParams, Int32 nPurchaseID, int nBillingMethod, Int32 nPaymentNumber,
           int nTotalPaymentsNumber, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int nNumOfPayments, bool bIsPurchasedWithPreviewModule, DateTime dtCurrentEndDate, ConditionalAccess.eBillingProvider bp)
        {
            TvinciBilling.BillingResponse res = new ConditionalAccess.TvinciBilling.BillingResponse();
            res.m_oStatus = TvinciBilling.BillingResponseStatus.UnKnown;

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
                    res = HandleBaseRenewMPPBillingCharge(sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber,
                        nRecPeriods, sExtraParams, nBillingMethod, nPurchaseID, bp);

                    if (res.m_oStatus == TvinciBilling.BillingResponseStatus.Success)
                    {
                        HandleMPPRenewalBillingSuccess(sSiteGUID, sSubscriptionCode, dtCurrentEndDate, bIsPurchasedWithPreviewModule,
                           nPurchaseID, sCurrency, dPrice, nPaymentNumber, res.m_sRecieptCode, nMaxVLCOfSelectedUsageModule,
                           bIsMPPRecurringInfinitely, nRecPeriods);

                    }
                    else
                    {
                        HandleMPPRenewalBillingFail(sSiteGUID, sSubscriptionCode, nPurchaseID, res, sCustomData);
                    }
                }
                else
                {
                    // user does not exist. update fail count to max so we won't try again to renew this mpp
                    // return unknown user
                    HandleMPPRenewalUserDoesNotExist(sSiteGUID, nPurchaseID, ref res);
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
                Logger.Logger.Log("DD_BaseRenewMultiUsageSubscription", sb.ToString(), "TvinciRenewer");
                WriteToUserLog(sSiteGUID, string.Format("MPP Renewal. Exception thrown. Msg: {0}", ex.Message));
                #endregion

                // increment fail count
                ConditionalAccessDAL.Update_MPPFailCountByPurchaseID(nPurchaseID, true, 0);

                // set billing response
                res.m_oStatus = TvinciBilling.BillingResponseStatus.Fail;
                res.m_sRecieptCode = string.Empty;
                res.m_sStatusDescription = ex.Message;
            }

            return res;
        }


        protected void HandleMPPRenewalBillingFail(string sSiteGUID, string sSubscriptionCode, long lPurchaseID,
         TvinciBilling.BillingResponse br, string sCustomData)
        {

            Logger.Logger.Log("Fail", string.Format("Fail count for user: {0} . Sub Code: {1} , Purchase ID: {2} , Response status: {3} , Response status desc: {4} , Custom Data: {5}", sSiteGUID, sSubscriptionCode, lPurchaseID, br.m_oStatus.ToString(), br.m_sStatusDescription, sCustomData), "TvinciRenewer");
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
            Logger.Logger.Log("Fail", string.Format("User ID: {0} does not exist. Purchase ID: {1}", sSiteGUID, lPurchaseID), "TvinciRenewer");

            // user does not exist. there is no point to continue trying renewing the mpp.
            // hence, we set the fail count to maximum
            ConditionalAccessDAL.Update_MPPFailCountByPurchaseID(lPurchaseID, false, Utils.GetGroupFAILCOUNT(m_nGroupID, "CA_CONNECTION_STRING"), "CA_CONNECTION_STRING");

            res.m_oStatus = TvinciBilling.BillingResponseStatus.UnKnownUser;
            res.m_sStatusDescription = "User does not exist";
            res.m_sRecieptCode = string.Empty;
        }


        /// <summary>
        /// Increase Subscription Purchase FAIL COUNT
        /// </summary>
        /// <param name="nPurchaseID">set Purchase ID</param>
        /// <param name="sFailCount">set Fail Count to update</param>
        private void IncreaseSubscriptionPurchaseFAILCOUNT(int nPurchaseID, string sFailCount)
        {
            ODBCWrapper.DirectQuery directQuery2 = new ODBCWrapper.DirectQuery();
            directQuery2.SetConnectionKey("CA_CONNECTION_STRING");
            directQuery2 += "update subscriptions_purchases set ";
            directQuery2 += string.Format("FAIL_COUNT = {0}", sFailCount);
            directQuery2 += " where ";
            directQuery2 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
            directQuery2.Execute();
            directQuery2.Finish();
            directQuery2 = null;
        }
        /// <summary>
        /// Direct Deipt Renew Subscription
        /// </summary>
        public TvinciBilling.BillingResponse DD_BaseRenewSubscription(string sSiteGUID, double dPrice, string sCurrency,
           string sSubscriptionCode, string sUserIP, string sExtraParams, Int32 nPurchaseID, int nBillingMethod, Int32 nPaymentNumber,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            string sCouponCode = "";
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            if (sSiteGUID == "")
            {
                #region terminate if site guid id empty
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Cant charge an unknown user";
                #endregion
            }
            else
            {
                #region Init useres web service
                TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL != "")
                    u.Url = sWSURL;
                #endregion

                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    #region terminate if ResponseStatus NOT Ok.
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                    try { WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " error returned: " + ret.m_sStatusDescription); }
                    catch { }

                    //Increase subscription purchase fail count
                    ODBCWrapper.DirectQuery directQuery2 = new ODBCWrapper.DirectQuery();
                    directQuery2.SetConnectionKey("CA_CONNECTION_STRING");
                    directQuery2 += "update subscriptions_purchases set ";
                    directQuery2 += "FAIL_COUNT = FAIL_COUNT + 1 ";
                    directQuery2 += " where ";
                    directQuery2 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                    directQuery2.Execute();
                    directQuery2.Finish();
                    directQuery2 = null;

                    #endregion
                }
                else
                {
                    #region Init Tvinci Pricing web service
                    TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule();
                    if (Utils.GetWSURL("pricing_ws") != "")
                        m.Url = Utils.GetWSURL("pricing_ws");
                    #endregion

                    TvinciPricing.Subscription theSub = null;
                    string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                    if (CachingManager.CachingManager.Exist("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                        theSub = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache));
                    else
                    {
                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetSubscriptionData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                        theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubscriptionCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);
                        CachingManager.CachingManager.SetCachedData("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache, theSub, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    }

                    if (theSub != null && theSub.m_nNumberOfRecPeriods != 0 && theSub.m_nNumberOfRecPeriods < nPaymentNumber)
                    {
                        #region Update subscription purchase to is recurring status = 0
                        ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                        directQuery.SetConnectionKey("CA_CONNECTION_STRING");
                        directQuery += "update subscriptions_purchases set ";
                        directQuery += "IS_RECURRING_STATUS = 0 ";
                        directQuery += " where ";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                        directQuery.Execute();
                        directQuery.Finish();
                        directQuery = null;
                        #endregion

                    }
                    else if (theSub != null && theSub.m_oSubscriptionUsageModule != null)
                    {
                        string sCustomData = "";
                        if (dPrice != 0)
                        {
                            TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
                            sWSUserName = "";
                            sWSPass = "";
                            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "CC_ChargeUser", "billing", sIP, ref sWSUserName, ref sWSPass);
                            sWSURL = Utils.GetWSURL("billing_ws");
                            if (sWSURL != "")
                                bm.Url = sWSURL;

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
                            //int scTransactionID = GetRenewalTransactionID(sSiteGUID, sSubscriptionCode, m_nGroupID);
                            //ret = bm.CC_RenewChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber, nRecPeriods, sExtraParams, scTransactionID);
                            ret = bm.DD_ChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber, nRecPeriods, nPurchaseID.ToString(), nBillingMethod);

                        }
                    }
                    if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success && theSub != null && theSub.m_oSubscriptionUsageModule != null)
                    {
                        Int32 nMaxVLC = theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle;
                        try { WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " renewed" + dPrice.ToString() + sCurrency); }
                        catch { }
                        DateTime d = (DateTime)(ODBCWrapper.Utils.GetTableSingleVal("subscriptions_purchases", "end_date", nPurchaseID, "CA_CONNECTION_STRING"));
                        DateTime dNext = Utils.GetEndDateTime(d, nMaxVLC);
                        #region update subscriptions_purchases end date
                        ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                        directQuery.SetConnectionKey("CA_CONNECTION_STRING");
                        directQuery += "update subscriptions_purchases set ";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", "=", dNext);
                        directQuery += ",";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("num_of_uses", "=", 0);
                        directQuery += " where ";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                        directQuery.Execute();
                        directQuery.Finish();
                        directQuery = null;
                        #endregion

                        if (!string.IsNullOrEmpty(ret.m_sRecieptCode))
                        {
                            Int32 nID = int.Parse(ret.m_sRecieptCode);

                            #region Update billing transactions with PurchaseID
                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                            updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                            updateQuery += "where";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;
                            #endregion
                        }
                    }
                    else
                    {
                        Logger.Logger.Log("Fail", "Fail count for user " + sSiteGUID, "CCRenewer");
                        try { WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " error returned: " + ret.m_sStatusDescription); }
                        catch { }

                        if (ret.m_oStatus != ConditionalAccess.TvinciBilling.BillingResponseStatus.ExternalError)
                        {
                            if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.ExpiredCard)
                            {
                                #region Update subscription purchases to fail count = 10
                                ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                                directQuery.SetConnectionKey("CA_CONNECTION_STRING");
                                directQuery += "update subscriptions_purchases set ";
                                directQuery += "FAIL_COUNT = 10 ";
                                directQuery += " where ";
                                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                                directQuery.Execute();
                                directQuery.Finish();
                                directQuery = null;
                                #endregion
                            }

                            #region Increase subscription purchase fail count
                            ODBCWrapper.DirectQuery directQuery2 = new ODBCWrapper.DirectQuery();
                            directQuery2.SetConnectionKey("CA_CONNECTION_STRING");
                            directQuery2 += "update subscriptions_purchases set ";
                            directQuery2 += "FAIL_COUNT = FAIL_COUNT + 1 ";
                            directQuery2 += " where ";
                            directQuery2 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                            directQuery2.Execute();
                            directQuery2.Finish();
                            directQuery2 = null;
                            #endregion
                        }
                        //#region Add notification request
                        //NotificationService.NotificationServiceClient ns = new NotificationService.NotificationServiceClient(string.Empty, Utils.GetWSURL("notification_ws"));
                        //string nsWSUserName = "";
                        //string nsWSPass = "";
                        //TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "CC_BaseRenewSubscription", "notifications", sIP, ref nsWSUserName, ref nsWSPass);
                        //ns.AddNotificationRequest(nsWSUserName, nsWSPass, long.Parse(sSiteGUID), NotificationService.NotificationTriggerType.PaymentFailure);
                        //#endregion
                    }
                }

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
                Logger.Logger.Log("Get Appropriate Multi Subscription Usage Module Fail", strLog, "TvinciRenewer");
            }
            return u;
        }


        public TvinciBilling.BillingResponse CC_BaseMultiRenewSubscription(string sSiteGUID, double dPrice, string sCurrency,
           string sSubscriptionCode, string sUserIP, string sExtraParams, Int32 nPurchaseID, int nBillingMethod, Int32 nPaymentNumber,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            //write loge
            Logger.Logger.Log("CC Base Multi usage modue renew subscritpion", sSiteGUID + " " + sSubscriptionCode, "RenewMultiUsageModule");
            //init coupon code
            string sCouponCode = "";
            //create billing response resault object 
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();

            #region Init useres web service
            TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";
            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
            string sWSURL = Utils.GetWSURL("users_ws");
            if (sWSURL != "")
                u.Url = sWSURL;
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


                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    #region terminate if ResponseStatus NOT Ok.
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                    try { WriteToUserLog(sSiteGUID, "Subscription auto renewal: " + sSubscriptionCode.ToString() + " error returned: " + ret.m_sStatusDescription); }
                    catch { }
                    #endregion
                }
            }
            #endregion

            #region Init Tvinci Pricing web service
            TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule();
            if (Utils.GetWSURL("pricing_ws") != "")
                m.Url = Utils.GetWSURL("pricing_ws");
            #endregion

            TvinciPricing.Subscription theSub = null;
            string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
            if (CachingManager.CachingManager.Exist("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                theSub = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache));
            else
            {
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetSubscriptionData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubscriptionCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);
                CachingManager.CachingManager.SetCachedData("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache, theSub, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }
            if (theSub != null && theSub.m_nNumberOfRecPeriods != 0 && theSub.m_nNumberOfRecPeriods <= nPaymentNumber)
            {


                #region Update subscription purchase to is recurring status = 0
                ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                directQuery.SetConnectionKey("CA_CONNECTION_STRING");
                directQuery += "update subscriptions_purchases set ";
                directQuery += "IS_RECURRING_STATUS = 0 ";
                directQuery += " where ";
                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                directQuery.Execute();
                directQuery.Finish();
                directQuery = null;
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
                        if (string.IsNullOrEmpty(sCouponCode) == false)
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
                Logger.Logger.Log("HandleRecurringCoupon error", ", PurchaseID: " + nPurchaseID.ToString() + ",Exception:" + ex.ToString(), "TvinciRenewer");
            }
        }
        /// <summary>
        /// Get Licensed Link
        /// </summary>
        protected abstract string GetLicensedLink(string sBasicLink, string sUserIP, string sRefferer);
        /// <summary>
        /// Get Error Licensed Link
        /// </summary>
        protected abstract string GetErrorLicensedLink(string sBasicLink);
        /// <summary>
        /// Activate Campaign
        /// </summary>
        public abstract bool ActivateCampaign(int campaignID, CampaignActionInfo cai);

        public abstract CampaignActionInfo ActivateCampaignWithInfo(int campaignID, CampaignActionInfo cai);

        protected bool isDevicePlayValid(string sSiteGUID, string sDEVICE_NAME)
        {
            bool isDeviceRecognized = false;
            TvinciUsers.UsersService u = null;
            TvinciDomains.module domainsWS = null;
            try
            {
                string sIP = "1.1.1.1";
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                u = new TvinciUsers.UsersService();
                string sWSURL = Utils.GetWSURL("users_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                    u.Url = sWSURL;
                TvinciUsers.UserResponseObject userRepObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                if (userRepObj != null && userRepObj.m_user != null && userRepObj.m_RespStatus == ResponseStatus.OK)
                {
                    int domainID = userRepObj.m_user.m_domianID;
                    if (domainID != 0)
                    {
                        domainsWS = new TvinciDomains.module();
                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetDomainData", "domains", sIP, ref sWSUserName, ref sWSPass);
                        sWSURL = Utils.GetWSURL("domains_ws");
                        if (!string.IsNullOrEmpty(sWSURL))
                            domainsWS.Url = sWSURL;
                        TvinciDomains.Domain userDomain = domainsWS.GetDomainInfo(sWSUserName, sWSPass, domainID);
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



        public virtual string GetEPGLink(int nProgramId, DateTime nStartTime, eEPGFormatType format, string sSiteGUID, Int32 nMediaFileID, string sBasicLink, string sUserIP, string sRefferer, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, string sCouponCode)
        {
            return string.Empty;
        }

        /// <summary>
        /// Get Licensed Link
        /// </summary>
        public virtual string GetLicensedLink(string sSiteGUID, Int32 nMediaFileID, string sBasicLink, string sUserIP, string sRefferer, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, string couponCode)
        {
            Int32[] nMediaFileIDs = { nMediaFileID };

            if (sBasicLink.Contains(string.Format("||{0}", nMediaFileID)))
            {
                sBasicLink = Utils.GetBasicLink(m_nGroupID, nMediaFileIDs, nMediaFileID, sBasicLink);
            }
            bool isDeviceRecognized = isDevicePlayValid(sSiteGUID, sDEVICE_NAME);

            if (!isDeviceRecognized)
            {
                Logger.Logger.Log("Device Not Recognized", string.Format("User:{0}, MediaFile:{1}, Device:{2}", sSiteGUID, nMediaFileID.ToString(), sDEVICE_NAME), "LicensedLink");
                return string.Empty;
            }

            MediaFileItemPricesContainer[] prices = GetItemsPrices(nMediaFileIDs, sSiteGUID, couponCode, true, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, sUserIP);
            if (prices.Length == 0)
            {
                return "";
            }
            if (prices[0].m_oItemPrices == null || prices[0].m_oItemPrices.Length == 0 || prices[0].m_oItemPrices[0].m_PriceReason == PriceReason.Free)
            {
                return GetLicensedLink(sBasicLink, sUserIP, sRefferer);
            }

            if (prices[0].m_oItemPrices[0].m_oPrice.m_dPrice == 0 && (prices[0].m_oItemPrices[0].m_PriceReason == PriceReason.PPVPurchased || prices[0].m_oItemPrices[0].m_PriceReason == PriceReason.SubscriptionPurchased || prices[0].m_oItemPrices[0].m_PriceReason == PriceReason.PrePaidPurchased || prices[0].m_oItemPrices[0].m_PriceReason == PriceReason.CollectionPurchased))
            {
                if (Utils.ValidateBaseLink(m_nGroupID, nMediaFileID, sBasicLink) == true)
                {
                    HandlePlayUses(prices[0], sSiteGUID, nMediaFileID, sUserIP, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, couponCode);
                    return GetLicensedLink(sBasicLink, sUserIP, sRefferer);
                }
            }

            return GetErrorLicensedLink(sBasicLink);
        }
        /// <summary>
        /// Get Licensed Link With Media File CoGuid
        /// </summary>
        public virtual string GetLicensedLinkWithMediaFileCoGuid(string sSiteGUID, string sMediaFileCoGuid, string sBasicLink, string sUserIP, string sRefferer, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, string couponCode)
        {

            Int32 nMediaFileID = int.Parse(sMediaFileCoGuid);
            Int32[] nMediaFileIDs = { nMediaFileID };
            bool isDeviceValid = isDevicePlayValid(sSiteGUID, sDEVICE_NAME);
            if (!isDeviceValid)
            {
                return string.Empty;
            }
            MediaFileItemPricesContainer[] prices = GetItemsPrices(nMediaFileIDs, sSiteGUID, couponCode, true, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);
            if (prices.Length == 0)
                return "";
            if (prices[0].m_oItemPrices[0].m_oPrice.m_dPrice == 0)
            {
                if (Utils.ValidateBaseLink(m_nGroupID, nMediaFileID, sBasicLink) == true)
                {
                    HandlePlayUses(prices[0], sSiteGUID, nMediaFileID, sUserIP, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, couponCode);
                    return GetLicensedLink(sBasicLink, sUserIP, sRefferer);
                }
            }
            return GetErrorLicensedLink(sBasicLink);
        }
        /// <summary>
        /// Handle Play Uses
        /// </summary>
        protected void HandlePlayUses(MediaFileItemPricesContainer price, string sSiteGUID, Int32 nMediaFileID, string sUserIP, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, string couponCode)
        {
            if (!string.IsNullOrEmpty(sUserIP) && string.IsNullOrEmpty(sCOUNTRY_CODE))
            {
                sCOUNTRY_CODE = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
            }

            int nReleventCollectionID = 0;

            if (price != null && price.m_oItemPrices != null && price.m_oItemPrices.Length > 0 && price.m_oItemPrices[0].m_relevantCol != null)
            {
                Int32.TryParse(price.m_oItemPrices[0].m_relevantCol.m_sObjectCode, out nReleventCollectionID);
            }

            HandleCouponUses(price.m_oItemPrices[0].m_relevantSub, price.m_oItemPrices[0].m_sPPVModuleCode, sSiteGUID,
            price.m_oItemPrices[0].m_oPrice.m_dPrice, price.m_oItemPrices[0].m_oPrice.m_oCurrency.m_sCurrencyCD3,
            nMediaFileID, couponCode, sUserIP,
            sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, false, 0, nReleventCollectionID);

            Int32 nRelPP = 0;
            //Get relevant PrePaid
            if (price.m_oItemPrices[0].m_relevantPP != null)
            {
                nRelPP = price.m_oItemPrices[0].m_relevantPP.m_ObjectCode;
            }

            List<int> lUsersIds = Utils.GetAllUsersDomainBySiteGUID(sSiteGUID, m_nGroupID);

            if (price.m_oItemPrices[0].m_relevantSub == null && price.m_oItemPrices[0].m_relevantCol == null)
            {
                string sPPVMCd = price.m_oItemPrices[0].m_sPPVModuleCode;
                Int32 nIsCreditDownloaded = PPV_DoesCreditNeedToDownloaded(sPPVMCd, sSiteGUID, nMediaFileID, null, null, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, lUsersIds);
                //ppv_uses
                UpdatePPVUses(nMediaFileID, price.m_oItemPrices[0].m_sPPVModuleCode, sSiteGUID, nIsCreditDownloaded, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, nRelPP, nReleventCollectionID);
                //ppv_purchases
                Int32 nPPVID = 0;
                string sRelSub = "";
                if (nIsCreditDownloaded == 1)
                {
                    //sRelSub - the subscription that caused the price to be lower
                    nPPVID = GetActivePPVPurchaseID(nMediaFileID, sSiteGUID, ref sRelSub, lUsersIds);

                    if (nPPVID == 0 && !string.IsNullOrEmpty(couponCode))
                    {
                        InsertPPVPurchases(sSiteGUID, nMediaFileID, price.m_oItemPrices[0].m_oPrice.m_dPrice,
                            price.m_oItemPrices[0].m_oPrice.m_oCurrency.m_sCurrencyCD3, sRelSub, 0, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, sPPVMCd,
                            couponCode, sUserIP);

                        nPPVID = GetActivePPVPurchaseID(nMediaFileID, sSiteGUID, ref sRelSub, lUsersIds);
                    }

                    UpdatePPVPurchases(nMediaFileID, sSiteGUID, nPPVID, price.m_oItemPrices[0].m_sPPVModuleCode, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);

                }
            }
            else if (price.m_oItemPrices[0].m_relevantCol == null)
            {
                //Send Subscription Uses Notification
                HandleSubscriptionUsesNotification(nMediaFileID, price.m_oItemPrices[0].m_relevantSub.m_sObjectCode, sSiteGUID);

                Int32 nIsCreditDownloaded = Utils.Bundle_DoesCreditNeedToDownloaded(price.m_oItemPrices[0].m_relevantSub.m_sObjectCode, sSiteGUID, nMediaFileID, m_nGroupID, eBundleType.SUBSCRIPTION) == true ? 1 : 0;
                //subscriptions_uses

                UpdateSubscriptionUses(nMediaFileID, price.m_oItemPrices[0].m_relevantSub.m_sObjectCode, sSiteGUID, nIsCreditDownloaded, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, nRelPP);
                //subscriptions_purchases
                if (nIsCreditDownloaded == 1)
                {
                    UpdateSubscriptionPurchases(price.m_oItemPrices[0].m_relevantSub.m_sObjectCode, sSiteGUID);
                }

                Int32 nIsCreditDownloaded1 = PPV_DoesCreditNeedToDownloaded("s: " + price.m_oItemPrices[0].m_relevantSub.m_sObjectCode, sSiteGUID, nMediaFileID, price.m_oItemPrices[0].m_relevantSub, null, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, lUsersIds);
                UpdatePPVUses(nMediaFileID, "s: " + price.m_oItemPrices[0].m_relevantSub.m_sObjectCode, sSiteGUID, nIsCreditDownloaded1, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, nRelPP, nReleventCollectionID);
                Int32 nPPVID = 0;
                if (nIsCreditDownloaded1 == 1)
                {
                    string sRelSub = "";
                    nPPVID = GetActivePPVPurchaseID(nMediaFileID, sSiteGUID, ref sRelSub, lUsersIds);

                    if (nPPVID == 0 && !string.IsNullOrEmpty(couponCode))
                    {
                        InsertPPVPurchases(sSiteGUID, nMediaFileID, price.m_oItemPrices[0].m_oPrice.m_dPrice,
                            price.m_oItemPrices[0].m_oPrice.m_oCurrency.m_sCurrencyCD3, sRelSub, 0, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME,
                            price.m_oItemPrices[0].m_sPPVModuleCode, couponCode, sUserIP);

                        nPPVID = GetActivePPVPurchaseID(nMediaFileID, sSiteGUID, ref sRelSub, lUsersIds);
                    }

                    UpdatePPVPurchases(nMediaFileID, sSiteGUID, nPPVID, price.m_oItemPrices[0].m_sPPVModuleCode, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);
                }
            }
            else
            {
                Int32 nIsCreditDownloaded = Utils.Bundle_DoesCreditNeedToDownloaded(price.m_oItemPrices[0].m_relevantCol.m_sObjectCode, sSiteGUID, nMediaFileID, m_nGroupID, eBundleType.COLLECTION) == true ? 1 : 0;

                //collections_uses
                UpdateCollectionUses(nMediaFileID, price.m_oItemPrices[0].m_relevantCol.m_sObjectCode, sSiteGUID, nIsCreditDownloaded, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, nRelPP);
                if (nIsCreditDownloaded == 1)
                {
                    UpdateCollectionPurchases(price.m_oItemPrices[0].m_relevantCol.m_sObjectCode, sSiteGUID);
                }

                Int32 nIsCreditDownloaded1 = PPV_DoesCreditNeedToDownloaded("b: " + price.m_oItemPrices[0].m_relevantCol.m_sObjectCode, sSiteGUID, nMediaFileID, null, price.m_oItemPrices[0].m_relevantCol, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, lUsersIds);
                UpdatePPVUses(nMediaFileID, "b: " + price.m_oItemPrices[0].m_relevantCol.m_sObjectCode, sSiteGUID, nIsCreditDownloaded1, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, nRelPP, nReleventCollectionID);
                Int32 nPPVID = 0;
                if (nIsCreditDownloaded1 == 1)
                {
                    string sRelCol = "";
                    nPPVID = GetActivePPVPurchaseID(nMediaFileID, sSiteGUID, ref sRelCol, lUsersIds);

                    if (nPPVID == 0 && !string.IsNullOrEmpty(couponCode))
                    {
                        InsertPPVPurchases(sSiteGUID, nMediaFileID, price.m_oItemPrices[0].m_oPrice.m_dPrice,
                            price.m_oItemPrices[0].m_oPrice.m_oCurrency.m_sCurrencyCD3, sRelCol, 0, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME,
                            price.m_oItemPrices[0].m_sPPVModuleCode, couponCode, sUserIP);

                        nPPVID = GetActivePPVPurchaseID(nMediaFileID, sSiteGUID, ref sRelCol, lUsersIds);
                    }

                    UpdatePPVPurchases(nMediaFileID, sSiteGUID, nPPVID, price.m_oItemPrices[0].m_sPPVModuleCode, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);
                }
            }

        }

        protected bool IsLastView(Int32 nMediaFileID, string sSiteGUID, Int32 nPPVPurchaseID, ref DateTime endDateTime)
        {
            int nMaxNumOfUses = 0;
            int nNumOfUses = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select NUM_OF_USES, MAX_NUM_OF_USES, END_DATE from ppv_purchases where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nPPVPurchaseID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nMaxNumOfUses = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "MAX_NUM_OF_USES", 0);
                    nNumOfUses = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "NUM_OF_USES", 0);
                    endDateTime = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "END_DATE", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return (nNumOfUses + 1 >= nMaxNumOfUses ? true : false);
        }

        protected TvinciPricing.PPVModule GetPPVModule(string sPPVModuleCode, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";

            using (TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule())
            {
                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (sWSURL.Length > 0)
                    m.Url = sWSURL;

                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                TvinciPricing.PPVModule thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVModuleCode, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);

                return thePPVModule;
            }
        }

        /// <summary>
        /// Update PPV Purchases
        /// </summary>
        protected void UpdatePPVPurchases(Int32 nMediaFileID, string sSiteGUID, Int32 nPPVPurchaseID, string sPPVModuleCode, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            DateTime endDateTime = DateTime.Now;
            DateTime d = DateTime.Now;

            // Check if this is the last watch credit, also return the full view end date
            bool bIsLastView = IsLastView(nMediaFileID, sSiteGUID, nPPVPurchaseID, ref endDateTime);

            TvinciPricing.PPVModule thePPVModule = null;
            if (bIsLastView == true)
            {
                thePPVModule = GetPPVModule(sPPVModuleCode, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);

                d = Utils.GetEndDateTime(DateTime.UtcNow, thePPVModule.m_oUsageModule.m_tsViewLifeCycle);
                // if view cycle is far then the full view date consider the full view date to be the end date
                if (endDateTime < d)
                {
                    bIsLastView = false;
                }
            }

            ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
            directQuery += "update ppv_purchases set NUM_OF_USES=NUM_OF_USES+1,LAST_VIEW_DATE=getdate() ";
            if (bIsLastView == true)
            {
                directQuery += ODBCWrapper.Parameter.NEW_PARAM(",end_date", "=", d);
            }
            directQuery += " where ";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPPVPurchaseID);
            directQuery.Execute();
            directQuery.Finish();
            directQuery = null;
        }
        /// <summary>
        /// Insert PPV Purchases
        /// </summary>
        protected void InsertPPVPurchases(string sSiteGUID, Int32 nMediaFileID, double dPrice, string sCurrency, string sSubCode,
            Int32 nRecieptCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sPPVModuleCode, string sCouponCode, string sUserIP)
        {

            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";

            TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
            string sWSURL = Utils.GetWSURL("pricing_ws");
            if (sWSURL != "")
                m.Url = sWSURL;

            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
            TvinciPricing.PPVModule thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetSubscriptionData", "pricing", sIP, ref sWSUserName, ref sWSPass);
            TvinciPricing.Subscription relevantSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);

            Int32 nMediaID = Utils.GetMediaIDFeomFileID(nMediaFileID, m_nGroupID);

            string sCustomData = GetCustomData(relevantSub, thePPVModule, null, sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode, string.Empty, sCouponCode,
                sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);


            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ppv_purchases");
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
            insertQuery.Finish();
            insertQuery = null;
        }
        /// <summary>
        /// Update Susbscription Purchase
        /// </summary>
        protected void UpdateSubscriptionPurchases(string sSubCd, string sSiteGUID)
        {
            ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
            directQuery += "update subscriptions_purchases set NUM_OF_USES=NUM_OF_USES+1,LAST_VIEW_DATE=getdate() where ";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", GetActiveSubscriptionPurchaseID(sSubCd, sSiteGUID));
            directQuery.Execute();
            directQuery.Finish();
            directQuery = null;
        }

        /// <summary>
        /// Update Susbscription Purchase
        /// </summary>
        protected void UpdateCollectionPurchases(string sColCd, string sSiteGUID)
        {
            ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
            directQuery += "update collections_purchases set NUM_OF_USES=NUM_OF_USES+1,LAST_VIEW_DATE=getdate() where ";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", GetActiveCollectionPurchaseID(sColCd, sSiteGUID));
            directQuery.Execute();
            directQuery.Finish();
            directQuery = null;
        }

        /// <summary>
        /// Get Active PPv Purchase ID 
        /// </summary>
        protected Int32 GetActivePPVPurchaseID(Int32 nMediaFileID, string sSiteGUID, ref string sRelSub, List<int> lUsersIds)
        {
            Int32 nRet = 0;
            DataTable dt = DAL.ConditionalAccessDAL.Get_AllPPVPurchasesByUserIDsAndMediaFileID(nMediaFileID, lUsersIds, m_nGroupID);

            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            if (dt != null)
            {
                Int32 nCount = dt.Rows.Count;
                if (nCount > 0)
                {
                    nRet = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["ID"]);
                    sRelSub = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["SUBSCRIPTION_CODE"]);
                }
            }
            return nRet;
        }
        /// <summary>
        /// Get Active Subscription Purchase ID
        /// </summary>
        protected Int32 GetActiveSubscriptionPurchaseID(string sSubCd, string sSiteGUID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ID from subscriptions_purchases where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            selectQuery += " and (MAX_NUM_OF_USES>=NUM_OF_USES OR MAX_NUM_OF_USES=0) and START_DATE<getdate() and (end_date is null or end_date>getdate()) and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubCd);
            selectQuery += " and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        /// <summary>
        /// Get Active Collection Purchase ID
        /// </summary>
        protected Int32 GetActiveCollectionPurchaseID(string sColCd, string sSiteGUID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ID from collections_purchases where is_active=1 and status=1 and ";
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
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        /// <summary>
        /// PPV Does Credit Need To Downloaded
        /// </summary>
        protected Int32 PPV_DoesCreditNeedToDownloaded(string sPPVMCd, string sSiteGUID, Int32 nMediaFileID, TvinciPricing.Subscription theSub, TvinciPricing.Collection theCol, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, List<int> lUsersIds)
        {
            Int32 nIsCreditDownloaded = 1;
            Int32 nViewLifeCycle = 0;
            int OfflineStatus = 0;

            #region Check if the file is offline type
            ODBCWrapper.DataSetSelectQuery selectOfflineQuery = new ODBCWrapper.DataSetSelectQuery();
            selectOfflineQuery += "select top 1 gmt.DESCRIPTION, gmt.GROUP_ID, OFFLINE_STATUS from TVinci.dbo.media_files mf inner join TVinci.dbo.groups_media_type gmt";
            selectOfflineQuery += "on mf.Media_Type_ID = gmt.MEDIA_TYPE_ID";
            selectOfflineQuery += "where mf.IS_ACTIVE=1 and mf.STATUS=1 and gmt.IS_ACTIVE=1 and gmt.STATUS=1 ";
            selectOfflineQuery += "and";
            selectOfflineQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", nMediaFileID);
            selectOfflineQuery += "and";
            selectOfflineQuery += " gmt.GROUP_ID " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
            if (selectOfflineQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectOfflineQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    OfflineStatus = int.Parse(selectOfflineQuery.Table("query").DefaultView[0].Row["OFFLINE_STATUS"].ToString());

                }
            }
            selectOfflineQuery.Finish();
            selectOfflineQuery = null;
            #endregion


            if (OfflineStatus == 1)
            {
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";

                TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (sWSURL != "")
                    m.Url = sWSURL;

                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                TvinciPricing.UsageModule OfflineUsageModule = m.GetOfflineUsageModule(sWSUserName, sWSPass, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);
                nViewLifeCycle = OfflineUsageModule.m_tsViewLifeCycle;
            }
            else if (theSub == null && theCol == null)
            {
                TvinciPricing.PPVModule ppvModule = null;
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";

                TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (sWSURL != "")
                    m.Url = sWSURL;

                if (CachingManager.CachingManager.Exist("GetPPVModuleData" + sPPVMCd.Replace("s: ", "") + "_" + m_nGroupID.ToString()) == true)
                    ppvModule = (TvinciPricing.PPVModule)(CachingManager.CachingManager.GetCachedData("GetPPVModuleData" + sPPVMCd.Replace("s: ", "") + "_" + m_nGroupID.ToString()));
                else
                {
                    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                    ppvModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVMCd.Replace("s: ", ""), String.Empty, String.Empty, String.Empty);
                    CachingManager.CachingManager.SetCachedData("GetPPVModuleData" + sPPVMCd.Replace("s: ", "") + "_" + m_nGroupID.ToString(), ppvModule, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
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

            DataTable dtPPVUses = DAL.ConditionalAccessDAL.Get_allDomainsPPVUses(lUsersIds, m_nGroupID, nMediaFileID);

            if (dtPPVUses != null)
            {
                Int32 nCount = dtPPVUses.Rows.Count;
                if (nCount > 0)
                {
                    DateTime dNow = ODBCWrapper.Utils.GetDateSafeVal(dtPPVUses.Rows[0]["dNow"]);
                    DateTime dUsed = ODBCWrapper.Utils.GetDateSafeVal(dtPPVUses.Rows[0]["CREATE_DATE"]);

                    DateTime dEndDate = Utils.GetEndDateTime(dUsed, nViewLifeCycle);

                    if (dNow < dEndDate)
                        nIsCreditDownloaded = 0;

                    //if ((dNow - dUsed).TotalMinutes < nViewLifeCycle)
                    //    nIsCreditDownloaded = 0;
                }
            }

            return nIsCreditDownloaded;
        }
        /// <summary>
        /// SUB Does Credit Need To Downloaded
        /// </summary>
        protected Int32 SUB_DoesCreditNeedToDownloaded(TvinciPricing.Subscription theSub, string sSiteGUID, int mediaFileID, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, List<int> lUsersIds)
        {

            Int32 nIsCreditDownloaded = 1;
            Int32 nViewLifeCycle = 0;
            int OfflineStatus = 0;
            #region Old Code
            /*
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";

            TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
            string sWSURL = Utils.GetWSURL("pricing_ws");
            if (sWSURL != "")
                m.Url = sWSURL;


            TvinciPricing.Subscription theSub = null;

            if (CachingManager.CachingManager.Exist("GetSubscriptionData" + sSubCd + "_" + m_nGroupID.ToString()) == true)
                theSub = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData("GetSubscriptionData" + sSubCd + "_" + m_nGroupID.ToString()));
            else
            {
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetSubscriptionData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubCd, String.Empty, String.Empty, String.Empty, false);
                CachingManager.CachingManager.SetCachedData("GetSubscriptionData" + sSubCd + "_" + m_nGroupID.ToString(), theSub, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }
            */
            #endregion


            #region Check if the file is offline type
            ODBCWrapper.DataSetSelectQuery selectOfflineQuery = new ODBCWrapper.DataSetSelectQuery();
            selectOfflineQuery += "select top 1 gmt.DESCRIPTION, gmt.GROUP_ID, OFFLINE_STATUS from TVinci.dbo.media_files mf inner join TVinci.dbo.groups_media_type gmt";
            selectOfflineQuery += "on mf.Media_Type_ID = gmt.MEDIA_TYPE_ID";
            selectOfflineQuery += "where mf.IS_ACTIVE=1 and mf.STATUS=1 and gmt.IS_ACTIVE=1 and gmt.STATUS=1 ";
            selectOfflineQuery += "and";
            selectOfflineQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", mediaFileID);
            selectOfflineQuery += "and";
            selectOfflineQuery += " gmt.GROUP_ID " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
            if (selectOfflineQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectOfflineQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    OfflineStatus = int.Parse(selectOfflineQuery.Table("query").DefaultView[0].Row["OFFLINE_STATUS"].ToString());

                }
            }
            selectOfflineQuery.Finish();
            selectOfflineQuery = null;
            #endregion


            if (OfflineStatus == 1)
            {
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";

                TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (sWSURL != "")
                    m.Url = sWSURL;

                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                TvinciPricing.UsageModule OfflineUsageModule = m.GetOfflineUsageModule(sWSUserName, sWSPass, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);
                nViewLifeCycle = OfflineUsageModule.m_tsViewLifeCycle;
            }
            else if (theSub != null && theSub.m_oSubscriptionUsageModule != null)
            {
                TvinciPricing.UsageModule u = theSub.m_oSubscriptionUsageModule;
                nViewLifeCycle = u.m_tsViewLifeCycle;
            }

            DataTable dt = DAL.ConditionalAccessDAL.Get_SubUsesByUserListFileIDAndSubCode(lUsersIds, theSub.m_sObjectCode, mediaFileID, m_nGroupID);
            if (dt != null)
            {
                Int32 nCount = dt.Rows.Count;
                if (nCount > 0)
                {
                    DateTime dNow = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0]["dNow"]);
                    DateTime dUsed = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0]["CREATE_DATE"]);
                    if ((dNow - dUsed).TotalMinutes < nViewLifeCycle)
                        nIsCreditDownloaded = 0;
                }
            }
            return nIsCreditDownloaded;
        }
        /// <summary>
        /// Update Subscription Uses
        /// </summary>
        protected void UpdateSubscriptionUses(Int32 nMediaFileID, string sSubscriptionCode, string sSiteGUID, Int32 nIsCreditDownloaded, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, Int32 nRelPP)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscriptions_uses");

            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_CREDIT_DOWNLOADED", "=", nIsCreditDownloaded);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCOUNTRY_CODE);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLANGUAGE_CODE);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDEVICE_NAME);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_pre_paid", "=", nRelPP);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
        }
        /// <summary>
        /// Update Subscription Uses
        /// </summary>
        protected void UpdateCollectionUses(Int32 nMediaFileID, string sCollectionCode, string sSiteGUID, Int32 nIsCreditDownloaded, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, Int32 nRelPP)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("collections_uses");

            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COLLECTION_CODE", "=", sCollectionCode);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_CREDIT_DOWNLOADED", "=", nIsCreditDownloaded);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCOUNTRY_CODE);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLANGUAGE_CODE);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDEVICE_NAME);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
        }
        /// <summary>
        /// Update PPV Uses
        /// </summary>
        protected void UpdatePPVUses(Int32 nMediaFileID, string sPPVModuleCode, string sSiteGUID, Int32 nIsCreditDownloaded, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, Int32 nRelPP, Int32 nRel_Box_Set)
        {

            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ppv_uses");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PPVMODULE_CODE", "=", sPPVModuleCode);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_CREDIT_DOWNLOADED", "=", nIsCreditDownloaded);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCOUNTRY_CODE);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLANGUAGE_CODE);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDEVICE_NAME);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_pp", "=", nRelPP);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_box_set", "=", nRel_Box_Set);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
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
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            try
            {
                if (sCellPhone.Trim() == "")
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "Problematic Cell Phone: " + sCellPhone;
                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                    catch { }
                }
                else if (sSiteGUID == "")
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                }
                else
                {
                    TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
                    string sIP = "1.1.1.1";
                    string sWSUserName = "";
                    string sWSPass = "";
                    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (sWSURL != "")
                        u.Url = sWSURL;

                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = "";
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                    }
                    else
                    {
                        sIP = "1.1.1.1";
                        sWSUserName = "";
                        sWSPass = "";

                        TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
                        sWSURL = Utils.GetWSURL("pricing_ws");
                        if (sWSURL != "")
                            m.Url = sWSURL;
                        Int32[] nMediaFiles = { nMediaFileID };
                        string sMediaFileForCache = Utils.ConvertArrayIntToStr(nMediaFiles);
                        TvinciPricing.MediaFilePPVModule[] oModules = null;
                        string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (CachingManager.CachingManager.Exist("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString()) == true)
                            oModules = (TvinciPricing.MediaFilePPVModule[])(CachingManager.CachingManager.GetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache));
                        else
                        {
                            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleListForMediaFiles", "pricing", sIP, ref sWSUserName, ref sWSPass);
                            oModules = m.GetPPVModuleListForMediaFiles(sWSUserName, sWSPass, nMediaFiles, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                            CachingManager.CachingManager.SetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache, oModules, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                        }

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
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownPPVModule;
                            ret.m_sRecieptCode = "";
                            ret.m_sStatusDescription = "This PPVModule does not belong to item";
                            try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                            catch { }
                        }
                        else
                        {
                            PriceReason theReason = PriceReason.UnKnown;
                            TvinciPricing.Subscription relevantSub = null;
                            TvinciPricing.Collection relevantCol = null;
                            TvinciPricing.PrePaidModule relevantPP = null;

                            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                            TvinciPricing.PPVModule thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                            TvinciPricing.Price p = Utils.GetMediaFileFinalPrice(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                            if (theReason == PriceReason.ForPurchase)
                            {
                                if (p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency)
                                {
                                    //if (p.m_dPrice != 0)
                                    //{
                                    TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
                                    sWSUserName = "";
                                    sWSPass = "";
                                    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "SMS_SendCode", "billing", sIP, ref sWSUserName, ref sWSPass);
                                    sWSURL = Utils.GetWSURL("billing_ws");
                                    if (sWSURL != "")
                                        bm.Url = sWSURL;
                                    string sPPVModule = "";
                                    if (thePPVModule != null)
                                        sPPVModule = thePPVModule.m_sObjectCode;
                                    string sRefference = "";

                                    //Create the Custom Data
                                    string sCustomData = GetCustomData(relevantSub, thePPVModule, null, sSiteGUID, dPrice, sCurrency,
                                        nMediaFileID, nMediaID, sPPVModuleCode, string.Empty, sCouponCode, string.Empty,
                                        sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                                    Logger.Logger.Log("SMS CustomData", sCustomData, "CustomData");

                                    //string sCustomData = "<customdata type=\"pp\">";
                                    //if (String.IsNullOrEmpty(sCountryCd) == false)
                                    //    sCustomData += "<lcc>" + sCountryCd + "</lcc>";
                                    //if (String.IsNullOrEmpty(sLANGUAGE_CODE) == false)
                                    //    sCustomData += "<llc>" + sLANGUAGE_CODE + "</llc>";
                                    //if (String.IsNullOrEmpty(sDEVICE_NAME) == false)
                                    //    sCustomData += "<ldn>" + sDEVICE_NAME + "</ldn>";
                                    //sCustomData += "<rs>";
                                    //if (relevantSub != null)
                                    //    sCustomData += relevantSub.m_sObjectCode;
                                    //sCustomData += "</rs>";
                                    //sCustomData += "<mnou>";
                                    //if (thePPVModule != null && thePPVModule.m_oUsageModule != null)
                                    //    sCustomData += thePPVModule.m_oUsageModule.m_nMaxNumberOfViews.ToString();
                                    //sCustomData += "</mnou>";
                                    //sCustomData += "<mumlc>";
                                    //if (thePPVModule != null && thePPVModule.m_oUsageModule != null)
                                    //    sCustomData += thePPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle.ToString();
                                    //sCustomData += "</mumlc>";
                                    //sCustomData += "<u id=\"" + sSiteGUID + "\"/>";
                                    //sCustomData += "<mf>";
                                    //sCustomData += nMediaFileID.ToString();
                                    //sCustomData += "</mf>";
                                    //sCustomData += "<m>";
                                    //sCustomData += nMediaID.ToString();
                                    //sCustomData += "</m>";
                                    //sCustomData += "<ppvm>";
                                    //sCustomData += sPPVModuleCode;
                                    //sCustomData += "</ppvm>";
                                    //sCustomData += "<cc>";
                                    //sCustomData += sCouponCode;
                                    //sCustomData += "</cc>";
                                    //sCustomData += "<p ir=\"false\" n=\"1\" o=\"1\"/>";

                                    //sCustomData += "<pc>";
                                    //if (thePPVModule != null && thePPVModule.m_oPriceCode != null)
                                    //    sCustomData += thePPVModule.m_oPriceCode.m_sCode;
                                    //sCustomData += "</pc>";
                                    //sCustomData += "<pri>";
                                    //sCustomData += dPrice.ToString();
                                    //sCustomData += "</pri>";
                                    //sCustomData += "<cu>";
                                    //sCustomData += sCurrency;
                                    //sCustomData += "</cu>";

                                    //sCustomData += "</customdata>";
                                    //customdata id
                                    if (relevantSub != null)
                                    {
                                        //sRefference = BuiltRefferenceString(nMediaFileID, relevantSub.m_sObjectCode, sPPVModule, relevantSub.m_oPriceCode.m_sCode, relevantSub.m_oPriceCode.m_oPrise.m_dPrice, relevantSub.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3);
                                        //ret = bm.SMS_SendCode(sWSUserName, sWSPass, sSiteGUID, sCellPhone, sRefference);
                                        ret = bm.SMS_SendCode(sWSUserName, sWSPass, sSiteGUID, sCellPhone, sCustomData, sExtraParameters);
                                        try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): SMS code sent to: " + sCellPhone); }
                                        catch { }
                                    }
                                    else
                                    {
                                        //sRefference = BuiltRefferenceString(nMediaFileID, "", thePPVModule.m_sObjectCode, thePPVModule.m_oPriceCode.m_sCode, thePPVModule.m_oPriceCode.m_oPrise.m_dPrice, thePPVModule.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3);
                                        //ret = bm.SMS_SendCode(sWSUserName, sWSPass, sSiteGUID, sCellPhone, sRefference);
                                        ret = bm.SMS_SendCode(sWSUserName, sWSPass, sSiteGUID, sCellPhone, sCustomData, sExtraParameters);
                                        try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): SMS code sent to: " + sCellPhone); }
                                        catch { }
                                    }
                                }
                                else
                                {
                                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                                    ret.m_sRecieptCode = "";
                                    ret.m_sStatusDescription = "Mismatch in price or currency";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                                    catch { }
                                }
                            }
                            else
                            {
                                if (theReason == PriceReason.PPVPurchased)
                                {
                                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                    ret.m_sRecieptCode = "";
                                    ret.m_sStatusDescription = "The media file is already purchased";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                                    catch { }
                                }
                                if (theReason == PriceReason.SubscriptionPurchased)
                                {
                                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                    ret.m_sRecieptCode = "";
                                    ret.m_sStatusDescription = "The media file is contained in a purchased subscription";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                                    catch { }
                                }
                                if (theReason == PriceReason.Free)
                                {
                                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                    ret.m_sRecieptCode = "";
                                    ret.m_sStatusDescription = "The media file is free";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                                    catch { }
                                }
                                if (theReason == PriceReason.ForPurchaseSubscriptionOnly)
                                {
                                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                    ret.m_sRecieptCode = "";
                                    ret.m_sStatusDescription = "The media file is for purchase with subscription only";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                                    catch { }
                                }
                            }
                        }
                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("exception", ex.Message + "||" + ex.StackTrace, "exc");
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = ex.Message + "||" + ex.StackTrace;
                return ret;
            }
        }
        /// <summary>
        /// SMS Charge User For Subscription
        /// </summary>
        public virtual TvinciBilling.BillingResponse SMS_ChargeUserForSubscription(string sSiteGUID, string sCellPhone, double dPrice, string sCurrency, string sSubscriptionCode, string sCouponCode, string sExtraParameters,
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
                TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL != "")
                    u.Url = sWSURL;

                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
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
                                TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
                                sWSUserName = "";
                                sWSPass = "";
                                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "SMS_SendCode", "billing", sIP, ref sWSUserName, ref sWSPass);
                                sWSURL = Utils.GetWSURL("billing_ws");
                                if (sWSURL != "")
                                    bm.Url = sWSURL;
                                if (theSub != null)
                                {
                                    bool bIsRecurring = theSub.m_bIsRecurring;
                                    Int32 nRecPeriods = theSub.m_nNumberOfRecPeriods;



                                    string sCustomData = GetCustomDataForSubscription(theSub, null, sSubscriptionCode, string.Empty, sSiteGUID, dPrice, sCurrency,
                                    sCouponCode, string.Empty, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                                    Logger.Logger.Log("SMS CustomData", sCustomData, "CustomDataForSubsrpition");
                                    //string sCustomData = "<customdata type=\"sp\">";
                                    //if (String.IsNullOrEmpty(sCountryCd) == false)
                                    //    sCustomData += "<lcc>" + sCountryCd + "</lcc>";
                                    //if (String.IsNullOrEmpty(sLANGUAGE_CODE) == false)
                                    //    sCustomData += "<llc>" + sLANGUAGE_CODE + "</llc>";
                                    //if (String.IsNullOrEmpty(sDEVICE_NAME) == false)
                                    //    sCustomData += "<ldn>" + sDEVICE_NAME + "</ldn>";
                                    //sCustomData += "<mnou>";
                                    //if (theSub != null && theSub.m_oUsageModule != null)
                                    //    sCustomData += theSub.m_oUsageModule.m_nMaxNumberOfViews.ToString();
                                    //sCustomData += "</mnou>";
                                    //sCustomData += "<u id=\"" + sSiteGUID + "\"/>";
                                    //sCustomData += "<s>" + sSubscriptionCode + "</s>";
                                    //sCustomData += "<cc>" + sCouponCode + "</cc>";
                                    //sCustomData += "<p ir=\"" + bIsRecurring.ToString().ToLower() + "\" n=\"1\" o=\"" + nRecPeriods.ToString() + "\"/>";
                                    //sCustomData += "<vlcs>";
                                    //if (theSub != null && theSub.m_oUsageModule != null)
                                    //    sCustomData += theSub.m_oUsageModule.m_tsViewLifeCycle.ToString();
                                    //sCustomData += "</vlcs>";
                                    //sCustomData += "<mumlc>";
                                    //if (theSub != null && theSub.m_oSubscriptionUsageModule != null)
                                    //    sCustomData += theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle.ToString();
                                    //sCustomData += "</mumlc>";
                                    //sCustomData += "<ppvm>";
                                    //sCustomData += "";
                                    //sCustomData += "</ppvm>";
                                    //sCustomData += "<pc>";
                                    //if (theSub != null && theSub.m_oSubscriptionPriceCode != null)
                                    //    sCustomData += theSub.m_oSubscriptionPriceCode.m_sCode;
                                    //sCustomData += "</pc>";
                                    //sCustomData += "<pri>";
                                    //sCustomData += dPrice.ToString();
                                    //sCustomData += "</pri>";
                                    //sCustomData += "<cu>";
                                    //sCustomData += sCurrency;
                                    //sCustomData += "</cu>";
                                    //sCustomData += "</customdata>";
                                    //customdata id
                                    ret = bm.SMS_SendCode(sWSUserName, sWSPass, sSiteGUID, sCellPhone, sCustomData, sExtraParameters);
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase subscription(SMS): " + sSubscriptionCode + " SMS code sent to: " + sCellPhone); }
                                    catch { }
                                }
                            }
                        }
                        else
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                            ret.m_sRecieptCode = "";
                            ret.m_sStatusDescription = "The price of the request is not the actual price";
                            try { WriteToUserLog(sSiteGUID, "While trying to purchase subscription(SMS): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription); }
                            catch { }
                        }
                    }
                    else
                    {
                        if (theReason == PriceReason.SubscriptionPurchased)
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = "";
                            ret.m_sStatusDescription = "The subscription is already purchased";
                            try { WriteToUserLog(sSiteGUID, "While trying to purchase subscription(SMS): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription); }
                            catch { }
                        }
                        if (theReason == PriceReason.Free)
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = "";
                            ret.m_sStatusDescription = "The subscription is free";
                            try { WriteToUserLog(sSiteGUID, "While trying to purchase subscription(SMS): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription); }
                            catch { }
                        }
                    }
                }

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

            UserCAStatus subStatus = UserCAStatus.NeverPurchased;
            bool subFound = GetUserCASubStatus(sSiteGUID, ref subStatus);
            if (subFound)
            {
                return subStatus;
            }
            //PermittedSubscriptionContainer[] subscriptionsItems = GetUserPermittedSubscriptions(sSiteGUID);
            //if (subscriptionsItems != null)
            //{
            //    Int32 nCurrentSubItems = subscriptionsItems.Length;
            //    if (nCurrentSubItems > 0)
            //        return UserCAStatus.CurrentSub;
            //}

            PermittedMediaContainer[] ppvItems = GetUserPermittedItems(sSiteGUID);
            if (ppvItems != null)
            {
                if (ppvItems.Length > 0)
                    return UserCAStatus.CurrentPPV;
            }

            Int32 nPastSub = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            selectQuery1 += "select count(*) as co from subscriptions_purchases where is_active=1 and status=1 and ";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            if (selectQuery1.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery1.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nPastSub = int.Parse(selectQuery1.Table("query").DefaultView[0].Row["co"].ToString());
                }
            }
            selectQuery1.Finish();
            selectQuery1 = null;
            if (nPastSub > 0)
                return UserCAStatus.ExSub;

            Int32 nPastPPV = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select count(*) as co from ppv_purchases where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nPastPPV = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nPastPPV > 0)
                return UserCAStatus.ExPPV;

            return UserCAStatus.NeverPurchased;
        }
        /// <summary>
        /// Get Billing Trans Method
        /// </summary>
        private PaymentMethod GetBillingTransMethod(int billingTransID)
        {
            PaymentMethod retVal = PaymentMethod.Unknown;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += " select BILLING_METHOD from billing_transactions where ";
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
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }
        /// <summary>
        /// Get Domains Users
        /// </summary>
        private List<int> GetDomainsUsers(int nDomainID)
        {
            string sIP = "1.1.1.1";
            TvinciDomains.module bm = new ConditionalAccess.TvinciDomains.module();
            string sWSUserName = "";
            string sWSPass = "";
            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetDomainUserList", "Domains", sIP, ref sWSUserName, ref sWSPass);
            string sWSURL = Utils.GetWSURL("domains_ws");
            if (sWSURL != "")
                bm.Url = sWSURL;

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

                Logger.Logger.Log("Error", sb.ToString(), "BaseConditionalAccess");

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
                    if (numOfItems != 0 && numOfItems < nCount)
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

                        DateTime dCurrent = DateTime.UtcNow;
                        if (dataRow["cDate"] != null && dataRow["cDate"] != DBNull.Value)
                            dCurrent = (DateTime)(dataRow["cDate"]);

                        DateTime dCreateDate = DateTime.UtcNow;
                        if (dataRow["CREATE_DATE"] != null && dataRow["CREATE_DATE"] != DBNull.Value)
                            dCreateDate = (DateTime)(dataRow["CREATE_DATE"]);

                        PaymentMethod payMet = GetBillingTransMethod(billingTransID);

                        string sDeviceUDID = ODBCWrapper.Utils.GetSafeStr(dataRow["device_name"]);

                        nMediaFilesIDs[i] = nMediaFileID;

                        #region Cancellation Window

                        string sPPVCode = ODBCWrapper.Utils.GetSafeStr(dataRow, "ppv");
                        
                        bool bCancellationWindow = false;                        
                        int nWaiver = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "WAIVER");

                        if (nWaiver == 0) // user didn't waiver yet
                        {
                            IsCancellationWindow(ref oUsageModule, sPPVCode, dCreateDate, ref bCancellationWindow, eTransactionType.PPV);
                        }

                        #endregion 

                        PermittedMediaContainer p = new PermittedMediaContainer();
                        p.Initialize(0, nMediaFileID, nMaxUses, nCurrentUses, dEnd, dCurrent, dCreateDate, payMet, sDeviceUDID, bCancellationWindow);
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

                Logger.Logger.Log("Exception", sb.ToString(), "BaseConditionalAccess");

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
            DataTable allCollectionsPurchases = DAL.ConditionalAccessDAL.Get_UsersPermittedCollections(lUsersIDs, isExpired);

            if (allCollectionsPurchases != null)
            {
                Int32 nCount = allCollectionsPurchases.Rows.Count;
                if (numOfItems != 0 && numOfItems < nCount)
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
                    #region Cancellation Window
                    int nWaiver = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "WAIVER");
                    if (nWaiver == 0) // user didn't waiver yet
                    {
                        IsCancellationWindow(ref oUsageModule, sCollectionCode, dCreateDate, ref bCancellationWindow, eTransactionType.Collection);
                    }
                    #endregion

                    PermittedCollectionContainer pcc = new PermittedCollectionContainer();
                    pcc.Initialize(sCollectionCode, dEnd, dCurrent, dLastViewDate, dCreateDate, nID, payMet, sDeviceUDID, bCancellationWindow);
                    ret[i] = pcc;
                    ++i;
                }
            }
            return ret;
        }

        private void IsCancellationWindow(ref TvinciPricing.UsageModule oUsageModule, string sAssetCode, DateTime dCreateDate, ref bool bCancellationWindow, eTransactionType transaction)
        {
            //get the right usage module for each ppv
            TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;
            string sIP = "1.1.1.1";
            string sWSURL = Utils.GetWSURL("pricing_ws");
            if (sWSURL != "")
                m.Url = sWSURL;

            string transactionName = Enum.GetName(typeof(eTransactionType), transaction);

            if (CachingManager.CachingManager.Exist("GetUsageModule" + transactionName + sAssetCode + "_" + m_nGroupID.ToString()) == true)
                oUsageModule = (TvinciPricing.UsageModule)(CachingManager.CachingManager.GetCachedData("GetUsageModule" + transactionName + sAssetCode + "_" + m_nGroupID.ToString()));
            else
            {
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUsageModule", "pricing", sIP, ref sWSUserName, ref sWSPass);
                oUsageModule = m.GetUsageModule(sWSUserName, sWSPass, sAssetCode, TvinciPricing.eTransactionType.Collection);
                CachingManager.CachingManager.SetCachedData("GetUsageModule" + transactionName + sAssetCode + "_" + m_nGroupID.ToString(), oUsageModule, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }

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
        /// <summary>
        /// Get User Permitted Subscriptions
        /// </summary>
        public virtual PermittedSubscriptionContainer[] GetUserPermittedSubscriptions(List<int> lUsersIDs, bool isExpired, int numOfItems)
        {
            PermittedSubscriptionContainer[] ret = null;
            DataTable allSubscriptionsPurchases = DAL.ConditionalAccessDAL.Get_UsersPermittedSubscriptions(lUsersIDs, isExpired);

            if (allSubscriptionsPurchases != null)
            {
                Int32 nCount = allSubscriptionsPurchases.Rows.Count;
                if (numOfItems != 0 && numOfItems < nCount)
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
                    #region Cancellation Window
                    int nWaiver = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "WAIVER");
                    if (nWaiver == 0) // user didn't waiver yet
                    {
                        IsCancellationWindow(ref oUsageModule, sSubscriptionCode, dCreateDate, ref bCancellationWindow, eTransactionType.Subscription);                       
                    }
                    #endregion

                    PermittedSubscriptionContainer p = new PermittedSubscriptionContainer();
                    p.Initialize(sSubscriptionCode, nMaxUses, nCurrentUses, dEnd, dCurrent, dLastViewDate, dCreateDate, dNextRenewalDate, bRecurringStatus, bIsSubRenewable, nID, payMet, sDeviceUDID, bCancellationWindow);
                    ret[i] = p;
                    ++i;
                }
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
                if (sHeader.StartsWith("mf:") == true)
                    nMediaFileID = int.Parse(sHeader.Substring(3));
                if (sHeader.StartsWith("sub:") == true)
                    sSubscriptionCode = sHeader.Substring(4);
                if (sHeader.StartsWith("ppvcode:") == true)
                    sPPVCode = sHeader.Substring(8);
            }
        }
        /// <summary>
        /// SMS Check Code For Media File
        /// </summary>
        public virtual TvinciBilling.BillingResponse SMS_CheckCodeForMediaFile(string sSiteGUID, string sCellPhone, string sSMSCode, Int32 nMediaFileID,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            string sIP = "1.1.1.1";
            TvinciBilling.BillingResponse ret = null;
            TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
            string sWSUserName = "";
            string sWSPass = "";
            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "SMS_CheckCode", "billing", sIP, ref sWSUserName, ref sWSPass);
            string sWSURL = Utils.GetWSURL("billing_ws");
            if (sWSURL != "")
                bm.Url = sWSURL;
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

                    TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
                    sWSURL = Utils.GetWSURL("pricing_ws");
                    if (sWSURL != "")
                        m.Url = sWSURL;

                    TvinciPricing.PPVModule thePPVModule = null;
                    string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                    if (CachingManager.CachingManager.Exist("GetPPVModuleData" + sPPVCode + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                    {
                        thePPVModule = TVinciShared.ObjectCopier.Clone<TvinciPricing.PPVModule>((TvinciPricing.PPVModule)(CachingManager.CachingManager.GetCachedData("GetPPVModuleData" + sPPVCode + "_" + m_nGroupID.ToString() + sLocaleForCache)));
                    }
                    else
                    {
                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                        thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        CachingManager.CachingManager.SetCachedData("GetPPVModuleData" + sPPVCode + "_" + m_nGroupID.ToString() + sLocaleForCache, thePPVModule, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    }

                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ppv_purchases");
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
                        //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", GetCurrentDBTime().AddSeconds(thePPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle));
                    }

                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;
                    try { WriteToUserLog(sSiteGUID, "Media file(SMS):" + nMediaFileID.ToString() + " purchased"); }
                    catch { }
                    Int32 nPurchaseID = 0;
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select id from ppv_purchases where ";
                    ///selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
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
                    selectQuery.Execute();
                    selectQuery.Finish();

                    //Should update the PURCHASE_ID

                    string sReciept = ret.m_sRecieptCode;
                    if (sReciept != "")
                    {
                        Int32 nID = int.Parse(sReciept);
                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                        updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                        updateQuery += "where";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                        updateQuery.Execute();
                        updateQuery.Finish();
                        updateQuery = null;
                    }

                }
                else
                {
                    if (ret.m_sStatusDescription != "SMS was not sent yet")
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Success;
                        ret.m_sStatusDescription = "Allready purchased";
                        try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(SMS): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                        catch { }
                    }
                }
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
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";

            TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
            string sWSURL = Utils.GetWSURL("pricing_ws");
            if (sWSURL != "")
                m.Url = sWSURL;

            TvinciPricing.Subscription theSub = null;
            string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
            if (CachingManager.CachingManager.Exist("GetSubscriptionData" + sSubscription + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                theSub = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData("GetSubscriptionData" + sSubscription + "_" + m_nGroupID.ToString() + sLocaleForCache));
            else
            {
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetSubscriptionData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubscription, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);
                CachingManager.CachingManager.SetCachedData("GetSubscriptionData" + sSubscription + "_" + m_nGroupID.ToString() + sLocaleForCache, theSub, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }

            if (theSub != null)
            {
                TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
                sWSUserName = "";
                sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "SMS_CheckCode", "billing", sIP, ref sWSUserName, ref sWSPass);
                sWSURL = Utils.GetWSURL("billing_ws");
                if (sWSURL != "")
                    bm.Url = sWSURL;

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
                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                        updateQuery += " where ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                        updateQuery += " and ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscription);
                        updateQuery += " and ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                        updateQuery.Execute();
                        updateQuery.Finish();
                        updateQuery = null;

                        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscriptions_purchases");
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscription);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", "");
                        //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                        //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrency);
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
                            //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", GetCurrentDBTime().AddSeconds(theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle));
                        }

                        insertQuery.Execute();
                        insertQuery.Finish();
                        insertQuery = null;
                        try { WriteToUserLog(sSiteGUID, "Subscription(SMS):" + sSubCode + " purchased"); }
                        catch { }
                    }
                    else
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Success;
                        ret.m_sStatusDescription = "Allready purchased";
                        try { WriteToUserLog(sSiteGUID, "While trying to purchase subscription(SMS): " + sSubscription + " error returned: " + ret.m_sStatusDescription); }
                        catch { }
                    }
                }
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
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnown;
            ret.m_sRecieptCode = string.Empty;
            ret.m_sStatusDescription = string.Empty;

            TvinciUsers.UsersService u = null;
            TvinciPricing.mdoule m = null;
            TvinciBilling.module bm = null;

            try
            {
                Logger.Logger.Log("CC_BaseChargeUserForMediaFile", string.Format("Entering CC_BaseChargeUserForMediaFile try block. Site Guid: {0} , Media File ID: {1} , Media ID: {2} , PPV Module Code: {3} , Coupon code: {4} , User IP: {5} , Payment Method: {6} , Dummy: {7}", sSiteGUID, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sPaymentMethodID, bDummy.ToString().ToLower()), GetLogFilename());
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
                    string sIP = "1.1.1.1";
                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (sWSURL.Length > 0)
                        u.Url = sWSURL;

                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                    }
                    else
                    {
                        bool bIsCouponValid = false;
                        bool bIsCouponUsedAndValid = false;
                        bIsCouponValid = Utils.IsCouponValid(m_nGroupID, sCouponCode);
                        if (!bIsCouponValid)
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = string.Empty;
                            ret.m_sStatusDescription = "Coupon not valid";
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }

                        bIsCouponUsedAndValid = bIsCouponValid && !string.IsNullOrEmpty(sCouponCode);

                        sIP = "1.1.1.1";
                        sWSUserName = string.Empty;
                        sWSPass = string.Empty;

                        m = new global::ConditionalAccess.TvinciPricing.mdoule();
                        sWSURL = Utils.GetWSURL("pricing_ws");
                        if (sWSURL.Length > 0)
                            m.Url = sWSURL;
                        Int32[] nMediaFiles = { nMediaFileID };
                        string sMediaFileForCache = Utils.ConvertArrayIntToStr(nMediaFiles);
                        TvinciPricing.MediaFilePPVModule[] oModules = null;
                        string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (CachingManager.CachingManager.Exist("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                            oModules = (TvinciPricing.MediaFilePPVModule[])(CachingManager.CachingManager.GetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache));
                        else
                        {
                            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleListForMediaFiles", "pricing", sIP, ref sWSUserName, ref sWSPass);
                            oModules = m.GetPPVModuleListForMediaFiles(sWSUserName, sWSPass, nMediaFiles, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                            CachingManager.CachingManager.SetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache, oModules, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                        }

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
                            if (!bDummy)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownPPVModule;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "This PPVModule does not belong to item";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                            else
                            {
                                bOK = true;
                                if (nCount > 0)
                                {
                                    sPPVModuleCode = oModules[0].m_oPPVModules[0].m_sObjectCode;
                                    dPrice = oModules[0].m_oPPVModules[0].m_oPriceCode.m_oPrise.m_dPrice;
                                    sCurrency = oModules[0].m_oPPVModules[0].m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3;
                                }
                            }
                        }
                        if (bOK)
                        {
                            PriceReason theReason = PriceReason.UnKnown;

                            TvinciPricing.Subscription relevantSub = null;
                            TvinciPricing.Collection relevantCol = null;
                            TvinciPricing.PrePaidModule relevantPP = null;

                            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                            TvinciPricing.PPVModule thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                            if (thePPVModule != null)
                            {
                                TvinciPricing.Price p = Utils.GetMediaFileFinalPrice(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                                bDummy = RecalculateDummyIndicatorForChargeMediaFile(bDummy, theReason, bIsCouponUsedAndValid);
                                if (theReason == PriceReason.ForPurchase || (theReason == PriceReason.SubscriptionPurchased && p.m_dPrice > 0) || bDummy)
                                {
                                    if (bDummy || (p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency))
                                    {
                                        string sCustomData = string.Empty;
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

                                        Logger.Logger.Log("CustomData", sCustomData, "CustomData");

                                        ret = HandleCCChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData,
                                            1, 1, sExtraParameters, sPaymentMethodID, sEncryptedCVV, bDummy, false, ref bm);
                                        if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
                                        {
                                            long lBillingTransactionID = 0;
                                            long lPurchaseID = 0;
                                            HandleChargeUserForMediaFileBillingSuccess(sSiteGUID, relevantSub, dPrice, sCurrency,
                                                sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, ret, sCustomData,
                                                thePPVModule, nMediaFileID, ref lBillingTransactionID, ref lPurchaseID);
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
                                        WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                                    }
                                    else if (theReason == PriceReason.Free)
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                        ret.m_sRecieptCode = string.Empty;
                                        ret.m_sStatusDescription = "The media file is free";
                                        WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                                    }
                                    else if (theReason == PriceReason.ForPurchaseSubscriptionOnly)
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                        ret.m_sRecieptCode = string.Empty;
                                        ret.m_sStatusDescription = "The media file is for purchase with subscription only";
                                        WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                                    }
                                    else if (theReason == PriceReason.SubscriptionPurchased)
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                        ret.m_sRecieptCode = string.Empty;
                                        ret.m_sStatusDescription = "The media file is already purchased (subscription)";
                                        WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                                    }
                                }
                            }
                            else
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "The ppv module is unknown";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                        }
                    }
                } // end else if siteguid == ""
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
                Logger.Logger.Log("CC_BaseChargeUserForMediaFile", sb.ToString(), GetLogFilename());
                WriteToUserLog(sSiteGUID, string.Format("Exception at CC_BaseChargeUserForMediaFile. Media File ID: {0} , Media ID: {1} , Coupon Code: {2}", nMediaFileID, nMediaID, sCouponCode));
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
                Logger.Logger.Log("GetPPVCustomDataID", GetGetCustomDataLogMsg("PPV", sSiteGUID, dPrice, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sPaymentMethod, sUserIP, string.Empty), GetLogFilename());
                u = new ConditionalAccess.TvinciUsers.UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL.Length > 0)
                    u.Url = sWSURL;

                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    retVal = 0;
                }
                else
                {
                    sIP = "1.1.1.1";
                    sWSUserName = string.Empty;
                    sWSPass = string.Empty;

                    m = new global::ConditionalAccess.TvinciPricing.mdoule();
                    sWSURL = Utils.GetWSURL("pricing_ws");
                    if (sWSURL.Length > 0)
                        m.Url = sWSURL;
                    Int32[] nMediaFiles = { nMediaFileID };
                    string sMediaFileForCache = Utils.ConvertArrayIntToStr(nMediaFiles);
                    TvinciPricing.MediaFilePPVModule[] oModules = null;
                    string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                    if (CachingManager.CachingManager.Exist("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                        oModules = (TvinciPricing.MediaFilePPVModule[])(CachingManager.CachingManager.GetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache));
                    else
                    {
                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleListForMediaFiles", "pricing", sIP, ref sWSUserName, ref sWSPass);
                        oModules = m.GetPPVModuleListForMediaFiles(sWSUserName, sWSPass, nMediaFiles, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        CachingManager.CachingManager.SetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache, oModules, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    }

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
                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                        TvinciPricing.PPVModule thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (!string.IsNullOrEmpty(sCampaignCode))
                        {
                            int nCampaignCode = int.Parse(sCampaignCode);
                            relevantCamp = m.GetCampaignData(sWSUserName, sWSPass, nCampaignCode);
                        }
                        if (thePPVModule != null)
                        {
                            TvinciPricing.Price p = Utils.GetMediaFileFinalPrice(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
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
                Logger.Logger.Log("GetPPVCustomDataID", sb.ToString(), GetLogFilename());
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
            if (sSiteGUID == "")
            {
                retVal = 0;
            }
            else
            {
                TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL != "")
                    u.Url = sWSURL;

                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
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
                    Logger.Logger.Log("GetBundleCustomDataID", GetGetCustomDataLogMsg("Bundle", sSiteGUID, dPrice, 0, 0, sBundleCode, sCouponCode, sPaymentMethod, sUserIP, sPreviewModuleID), GetLogFilename());
                    u = new ConditionalAccess.TvinciUsers.UsersService();
                    string sIP = "1.1.1.1";
                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (sWSURL.Length > 0)
                        u.Url = sWSURL;

                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
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
                                    sIP = "1.1.1.1";
                                    sWSUserName = string.Empty;
                                    sWSPass = string.Empty;

                                    m = new global::ConditionalAccess.TvinciPricing.mdoule();
                                    sWSURL = Utils.GetWSURL("pricing_ws");
                                    if (sWSURL.Length > 0)
                                        m.Url = sWSURL;

                                    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);

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
                                                sCustomData = GetCustomDataForSubscription(theBundle as TvinciPricing.Subscription, relevantCamp, sBundleCode, sCampaignCode, sSiteGUID, dPrice, sCurrency, sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, sOverrideEnddate, sPreviewModuleID);
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
                    Logger.Logger.Log("GetBundleCustomDataID", sb.ToString(), GetLogFilename());
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
            ret.m_sRecieptCode = "";
            ret.m_sStatusDescription = "";
            if (sSiteGUID == "")
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Cant charge an unknown user";
            }
            else
            {
                TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL != "")
                    u.Url = sWSURL;

                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                }
                else
                {
                    sIP = "1.1.1.1";
                    sWSUserName = "";
                    sWSPass = "";

                    TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
                    sWSURL = Utils.GetWSURL("pricing_ws");
                    if (sWSURL != "")
                        m.Url = sWSURL;
                    Int32[] nMediaFiles = { nMediaFileID };
                    string sMediaFileForCache = Utils.ConvertArrayIntToStr(nMediaFiles);
                    TvinciPricing.MediaFilePPVModule[] oModules = null;
                    string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                    if (CachingManager.CachingManager.Exist("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                        oModules = (TvinciPricing.MediaFilePPVModule[])(CachingManager.CachingManager.GetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache));
                    else
                    {
                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleListForMediaFiles", "pricing", sIP, ref sWSUserName, ref sWSPass);
                        oModules = m.GetPPVModuleListForMediaFiles(sWSUserName, sWSPass, nMediaFiles, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        CachingManager.CachingManager.SetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache, oModules, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    }

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
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownPPVModule;
                        ret.m_sRecieptCode = "";
                        ret.m_sStatusDescription = "This PPVModule does not belong to item";
                    }
                    else
                    {
                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.Subscription relevantSub = null;
                        TvinciPricing.Collection relevantCol = null;
                        TvinciPricing.PrePaidModule relevantPP = null;

                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                        TvinciPricing.PPVModule thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (thePPVModule != null)
                        {
                            TvinciPricing.Price p = Utils.GetMediaFileFinalPrice(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                            if (theReason == PriceReason.ForPurchase || (theReason == PriceReason.SubscriptionPurchased && p.m_dPrice > 0))
                            {
                                if (p.m_dPrice == dPrice && p.m_oCurrency.m_sCurrencyCD3 == sCurrency)
                                {
                                    if (p.m_dPrice != 0)
                                    {
                                        TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
                                        sWSUserName = "";
                                        sWSPass = "";
                                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "CC_GetPopupURL", "billing", sIP, ref sWSUserName, ref sWSPass);
                                        sWSURL = Utils.GetWSURL("billing_ws");
                                        if (sWSURL != "")
                                            bm.Url = sWSURL;
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
                                        if (sExtraParameters.StartsWith("&") == false)
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
                                    ret.m_sRecieptCode = "";
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
                }
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
                TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL != "")
                    u.Url = sWSURL;

                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
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
                                TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
                                sWSUserName = "";
                                sWSPass = "";
                                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "CC_ChargeUser", "billing", sIP, ref sWSUserName, ref sWSPass);
                                sWSURL = Utils.GetWSURL("billing_ws");
                                if (sWSURL != "")
                                    bm.Url = sWSURL;


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
                Logger.Logger.Log("CC_BaseChargeUserForBundle", string.Format("Entering CC_BaseChargeUserForBundle try block. Site Guid: {0} , Bundle Code: {1} , Coupon Code: {2} , User IP: {3} , Payment Method: {4} , Dummy: {5}", sSiteGUID, sBundleCode, sCouponCode, sUserIP, sPaymentMethodID, bDummy.ToString().ToLower()), GetLogFilename());
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
                    string sIP = "1.1.1.1";
                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (sWSURL.Length > 0)
                        u.Url = sWSURL;

                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cant charge an unknown user";
                    }
                    else
                    {
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
                        }

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

                                            ret = ExecuteCCSubscriprionPurchaseFlow(theBundle as TvinciPricing.Subscription, sBundleCode, sSiteGUID, dPrice, sCurrency, sCouponCode,
                                                                        sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, bIsEntitledToPreviewModule, bDummy, sExtraParams,
                                                                        sPaymentMethodID, sEncryptedCVV, p, ref bm, sWSUserName, sWSPass);
                                            break;
                                        }
                                    case eBundleType.COLLECTION:
                                        {
                                            ret = ExecuteCCCollectionPurchaseFlow(theBundle as TvinciPricing.Collection, sBundleCode, sSiteGUID, dPrice, sCurrency, sCouponCode,
                                                                        sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, bIsEntitledToPreviewModule, bDummy, sExtraParams,
                                                                        sPaymentMethodID, sEncryptedCVV, p, ref bm);

                                            break;
                                        }
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
                Logger.Logger.Log("CC_BaseChargeUserForSubscription", sb.ToString(), GetLogFilename());
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

        private TvinciBilling.BillingResponse ExecuteCCSubscriprionPurchaseFlow(TvinciPricing.Subscription theSub, string sBundleCode, string sSiteGUID, double dPrice,
                                    string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME,
                                    bool bIsEntitledToPreviewModule, bool bDummy, string sExtraParams, string sPaymentMethodID, string sEncryptedCVV, TvinciPricing.Price p,
                                    ref TvinciBilling.module bm, string sBillingUsername, string sBillingPassword)
        {
            string sCustomData = string.Empty;
            TvinciBilling.BillingResponse ret = null;

            //Create the Custom Data
            sCustomData = GetCustomDataForSubscription(theSub, null, sBundleCode, string.Empty, sSiteGUID, dPrice, sCurrency,
                sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty, bIsEntitledToPreviewModule ? theSub.m_oPreviewModule.m_nID + "" : string.Empty);

            Logger.Logger.Log("CustomData", string.Format("Subscription custom data created. Site Guid: {0} , User IP: {1} , Custom data: {2}", sSiteGUID, sUserIP, sCustomData), "CustomDataForSubsrpition");

            bool bIsRecurring = theSub.m_bIsRecurring;
            Int32 nRecPeriods = theSub.m_nNumberOfRecPeriods;

            if (p.m_dPrice != 0 || bDummy)
            {
                ret = HandleCCChargeUser(sBillingUsername, sBillingPassword, sSiteGUID, dPrice, sCurrency, sUserIP,
                    sCustomData, 1, nRecPeriods, sExtraParams, sPaymentMethodID, sEncryptedCVV,
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
                HandleChargeUserForSubscriptionBillingSuccess(sSiteGUID, theSub, dPrice, sCurrency, sCouponCode,
                    sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, ret, bIsEntitledToPreviewModule, sBundleCode, sCustomData,
                    bIsRecurring, ref lBillingTransactionID, ref lPurchaseID);
            }
            else
            {
                WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + sBundleCode + " error returned: " + ret.m_sStatusDescription);
            }

            return ret;
        }

        private TvinciBilling.BillingResponse ExecuteCCCollectionPurchaseFlow(TvinciPricing.Collection theCol, string sBundleCode, string sSiteGUID, double dPrice,
                                    string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME,
                                    bool bIsEntitledToPreviewModule, bool bDummy, string sExtraParams, string sPaymentMethodID, string sEncryptedCVV, TvinciPricing.Price p,
                                    ref TvinciBilling.module bm)
        {
            string sCustomData = string.Empty;
            TvinciBilling.BillingResponse ret = null;

            //Create the Custom Data
            sCustomData = GetCustomDataForCollection(theCol, sBundleCode, sSiteGUID, dPrice, sCurrency,
                sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty);

            Logger.Logger.Log("CustomData", string.Format("Collection custom data created. Site Guid: {0} , User IP: {1} , Custom data: {2}", sSiteGUID, sUserIP, sCustomData), "CustomDataForSubsrpition");

            if (p.m_dPrice != 0 || bDummy)
            {
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;

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
                HandleChargeUserForCollectionBillingSuccess(sSiteGUID, theCol, dPrice, sCurrency, sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE,
            sDEVICE_NAME, ret, sBundleCode, sCustomData, ref lBillingTransactionID, ref lPurchaseID);
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
            Int32 nCount = sSubscriptions.Length;
            if (nCount > 0)
                ret = new SubscriptionsPricesContainer[nCount];

            for (int i = 0; i < nCount; i++)
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
            return ret;
        }

        /// <summary>
        /// Get Collections Prices 
        /// </summary>
        public virtual CollectionsPricesContainer[] GetCollectionsPrices(string[] sCollections, string sUserGUID, string sCouponCode,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            CollectionsPricesContainer[] ret = null;
            Int32 nCount = sCollections.Length;
            if (nCount > 0)
                ret = new CollectionsPricesContainer[nCount];

            for (int i = 0; i < nCount; i++)
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
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual PrePaidPricesContainer[] GetPrePaidPrices(string[] sPrePaids, string sUserGUID, string sCouponCode,
         string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            PrePaidPricesContainer[] ret = null;
            Int32 nCount = sPrePaids.Length;
            if (nCount > 0)
                ret = new PrePaidPricesContainer[nCount];

            for (int i = 0; i < nCount; i++)
            {
                string sPrePaidCode = sPrePaids[i];
                PriceReason theReason = PriceReason.UnKnown;
                TvinciPricing.PrePaidModule prePaidMod = null;
                TvinciPricing.Price p = Utils.GetPrePaidFinalPrice(m_nGroupID, sPrePaidCode, sUserGUID, ref theReason, ref prePaidMod, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty, sCouponCode);
                PrePaidPricesContainer cont = new PrePaidPricesContainer();
                cont.Initialize(sPrePaidCode, p, theReason);
                ret[i] = cont;
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

            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";
            string sFirstDeviceNameFound = string.Empty;

            MediaFileItemPricesContainer[] ret = null;
            TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
            string sWSURL = Utils.GetWSURL("pricing_ws");
            if (sWSURL != "")
                m.Url = sWSURL;
            string nMediasForCache = Utils.ConvertArrayIntToStr(nMediaFiles);
            TvinciPricing.MediaFilePPVModule[] oModules = null;
            string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
            if (CachingManager.CachingManager.Exist("GetPPVModuleListForMediaFiles" + nMediasForCache + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                oModules = TVinciShared.ObjectCopier.Clone<TvinciPricing.MediaFilePPVModule[]>((TvinciPricing.MediaFilePPVModule[])(CachingManager.CachingManager.GetCachedData("GetPPVModuleListForMediaFiles" + nMediasForCache + "_" + m_nGroupID.ToString() + sLocaleForCache)));
            else
            {
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleListForMediaFiles", "pricing", sIP, ref sWSUserName, ref sWSPass);
                oModules = m.GetPPVModuleListForMediaFiles(sWSUserName, sWSPass, nMediaFiles, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                TvinciPricing.MediaFilePPVModule[] oModulesCopy = TVinciShared.ObjectCopier.Clone<TvinciPricing.MediaFilePPVModule[]>(oModules);
                CachingManager.CachingManager.SetCachedData("GetPPVModuleListForMediaFiles" + nMediasForCache + "_" + m_nGroupID.ToString() + sLocaleForCache, oModulesCopy, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }

            Int32 nCount = 0;
            if (oModules != null)
                nCount = oModules.Length;
            if (nCount > 0)
                ret = new MediaFileItemPricesContainer[nCount];

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

            for (int i = 0; i < nCount; i++)
            {

                Int32 nMediaFileID = oModules[i].m_nMediaFileID;
                TvinciPricing.PPVModule[] ppvModules = oModules[i].m_oPPVModules;
                MediaFileItemPricesContainer mf = new MediaFileItemPricesContainer();
                if (ppvModules != null)
                {
                    ItemPriceContainer[] itemPriceCont = null;
                    if (ppvModules.Length > 0)
                        itemPriceCont = new ItemPriceContainer[ppvModules.Length];
                    Int32 nLowestIndex = 0;
                    double dLowest = -1;
                    TvinciPricing.Price pLowest = null;
                    PriceReason theLowestReason = PriceReason.UnKnown;
                    TvinciPricing.Subscription relevantLowestSub = null;
                    TvinciPricing.Collection relevantLowestCol = null;
                    TvinciPricing.PrePaidModule relevantLowestPrePaid = null;
                    string sProductCode = string.Empty;
                    for (int j = 0; j < ppvModules.Length; j++)
                    {
                        string sPPVCode = ppvModules[j].m_sObjectCode;
                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.Subscription relevantSub = null;
                        TvinciPricing.Collection relevantCol = null;
                        TvinciPricing.PrePaidModule relevantPrePaid = null;

                        TvinciPricing.Price p = Utils.GetMediaFileFinalPrice(nMediaFileID, ppvModules[j], sUserGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPrePaid, ref sFirstDeviceNameFound, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, sClientIP);
                        sProductCode = oModules[i].m_sProductCode;
                        if (bOnlyLowest == false)
                        {
                            itemPriceCont[j] = new ItemPriceContainer();
                            itemPriceCont[j].Initialize(p, ppvModules[j].m_oPriceCode.m_oPrise, sPPVCode, ppvModules[j].m_sDescription, theReason, relevantSub, relevantCol, ppvModules[j].m_bSubscriptionOnly, relevantPrePaid, sFirstDeviceNameFound);
                        }
                        else
                        {
                            if (p.m_dPrice < dLowest || j == 0)
                            {
                                nLowestIndex = j;
                                dLowest = p.m_dPrice;
                                pLowest = p;
                                theLowestReason = theReason;
                                relevantLowestSub = relevantSub;
                                relevantLowestCol = relevantCol;
                                relevantLowestPrePaid = relevantPrePaid;
                            }
                        }
                    }
                    if (ppvModules.Length > 0 && bOnlyLowest == true)
                    {
                        itemPriceCont[0] = new ItemPriceContainer();
                        itemPriceCont[0].Initialize(pLowest, ppvModules[nLowestIndex].m_oPriceCode.m_oPrise, ppvModules[nLowestIndex].m_sObjectCode, ppvModules[nLowestIndex].m_sDescription, theLowestReason, relevantLowestSub, relevantLowestCol, ppvModules[nLowestIndex].m_bSubscriptionOnly, relevantLowestPrePaid, sFirstDeviceNameFound);
                    }
                    mf.Initialize(nMediaFileID, itemPriceCont, sProductCode);
                }
                else
                {
                    MediaFileItemPricesContainer mc = new MediaFileItemPricesContainer();
                    foreach (int mediaFileID in nMediaFiles)
                    {
                        ItemPriceContainer freeContainer = new ItemPriceContainer();
                        freeContainer.m_PriceReason = PriceReason.Free;
                        freeContainer.m_oPrice = new TvinciPricing.Price();
                        freeContainer.m_oPrice.m_dPrice = 0.0;
                        ItemPriceContainer[] priceContainer = new ItemPriceContainer[1];
                        priceContainer[0] = freeContainer;

                        mf.Initialize(mediaFileID, priceContainer);
                    }
                    ret[0] = mc;
                }
                ret[i] = mf;
            }
            return ret;
        }


        protected void InitializeBillingModule(ref TvinciBilling.module bm, ref string sWSUsername, ref string sWSPassword)
        {
            string sIP = "1.1.1.1";
            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "CC_ChargeUser", "billing", sIP, ref sWSUsername, ref sWSPassword);

            bm = new TvinciBilling.module();

            string sWSURL = Utils.GetWSURL("billing_ws");
            if (sWSURL.Length > 0)
                bm.Url = sWSURL;
        }

        /// <summary>
        /// Get Subscription Dates
        /// </summary>
        protected void GetSubscriptionDates(Int32 nPurchaseID, ref DateTime dStartDate, ref DateTime dEndDate)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select START_DATE, END_DATE from subscriptions_purchases where ";
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
            selectQuery.Finish();
            selectQuery = null;
        }
        /// <summary>
        /// Get Media Title
        /// </summary>
        protected string GetMediaTitle(Int32 nMediaID)
        {
            string sRet = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += "select name from media with (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sRet = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet;
        }
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

        /// <summary>
        /// Get Domains Billing History
        /// 
        /// (for Eutelsat Project)
        /// </summary>
        public DomainBillingTransactionsResponse[] GetDomainsBillingHistory(int[] domainIDs, DateTime dStartDate, DateTime dEndDate)
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

                    string[] sUserGuids = DAL.DomainDal.GetUsersInDomain(domainIDs[i], m_nGroupID, 1, 1).Select(ut => ut.Key.ToString()).ToArray();
                    //string[] sUserGuids = userIDs.Select(u => u.ToString()).ToArray();

                    domainBillingTransactions.m_BillingTransactionResponses = GetUsersBillingHistory(sUserGuids, dStartDate, dEndDate);

                    lDomainBillingTransactions.Add(domainBillingTransactions);
                }
                catch (Exception)
                {
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
                    userBillingTransactions.m_BillingTransactionResponse = GetUserBillingHistoryExt(arrUserGUIDs[i], dStartDate, dEndDate);
                    lUserBillingTransactions.Add(userBillingTransactions);
                }
                catch (Exception)
                {
                }
            }

            return lUserBillingTransactions.ToArray();
        }

        /// <summary>
        /// Get User Billing History
        /// </summary>
        protected virtual BillingTransactionsResponse GetUserBillingHistoryExt(string sUserGUID, DateTime dStartDate, DateTime dEndDate, int nStartIndex = 0, int nNumberOfItems = 0)
        {
            BillingTransactionsResponse theResp = new BillingTransactionsResponse();

            List<int> lGroupIDs = DAL.UtilsDal.GetAllRelatedGroups(m_nGroupID);
            string[] arrGroupIDs = lGroupIDs.Select(g => g.ToString()).ToArray();

            int nTopNum = nStartIndex + nNumberOfItems;
            DataView dvBillHistory = DAL.ConditionalAccessDAL.GetUserBillingHistory(arrGroupIDs, sUserGUID, nTopNum, dStartDate, dEndDate);


            if (dvBillHistory == null || dvBillHistory.Count == 0)
            {
                return theResp;
            }

            int nCount = dvBillHistory.Count;

            if (nTopNum > nCount || nTopNum == 0)
            {
                nTopNum = nCount;
            }

            theResp.m_nTransactionsCount = nCount;
            theResp.m_Transactions = new BillingTransactionContainer[nCount];


            for (int i = nStartIndex; i < nTopNum; i++)
            {
                theResp.m_Transactions[i] = new BillingTransactionContainer();

                string sCurrencyCode = dvBillHistory[i].Row["CURRENCY_CODE"].ToString();
                string sRemarks = "";

                if ((dvBillHistory[i].Row["REMARKS"] != null) && (dvBillHistory[i].Row["REMARKS"] != DBNull.Value))
                {
                    sRemarks = dvBillHistory[i].Row["REMARKS"].ToString();
                }

                double dPrice = GetSafeDouble(dvBillHistory[i].Row["TOTAL_PRICE"]);
                Int32 nPurchaseID = GetSafeInt(dvBillHistory[i].Row["PURCHASE_ID"]);
                DateTime dActionDate = (DateTime)(dvBillHistory[i].Row["CREATE_DATE"]);
                string sSubscriptionCode = dvBillHistory[i].Row["SUBSCRIPTION_CODE"].ToString();
                Int32 nMediaID = GetSafeInt(dvBillHistory[i].Row["MEDIA_ID"]);
                string sLAST_FOUR_DIGITS = dvBillHistory[i].Row["LAST_FOUR_DIGITS"].ToString();
                string sCellNum = dvBillHistory[i].Row["CELL_PHONE"].ToString();
                string sID = dvBillHistory[i].Row["ID"].ToString();
                int nPAYMENT_NUMBER = GetSafeInt(dvBillHistory[i].Row["PAYMENT_NUMBER"]);
                int nNUMBER_OF_PAYMENTS = GetSafeInt(dvBillHistory[i].Row["NUMBER_OF_PAYMENTS"]);
                int nBILLING_METHOD = GetSafeInt(dvBillHistory[i].Row["BILLING_METHOD"]);
                int nBILLING_PROCESSOR = GetSafeInt(dvBillHistory[i].Row["BILLING_PROCESSOR"]);
                int nNEW_RENEWABLE_STATUS = GetSafeInt(dvBillHistory[i].Row["NEW_RENEWABLE_STATUS"]);
                int nBILLING_PROVIDER = GetSafeInt(dvBillHistory[i].Row["BILLING_PROVIDER"]);
                int nBILLING_PROVIDER_REFFERENCE = GetSafeInt(dvBillHistory[i].Row["BILLING_PROVIDER_REFFERENCE"]);
                int nPURCHASE_ID = GetSafeInt(dvBillHistory[i].Row["PURCHASE_ID"]);

                string sPrePaidCode = Utils.GetStrSafeVal(dvBillHistory[i].Row["pre_paid_code"]);

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

                #region Handle Prepaid Data

                if (!string.IsNullOrEmpty(sPrePaidCode))
                {
                    try
                    {
                        theResp.m_Transactions[i].m_eItemType = BillingItemsType.PrePaid;

                        TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule();
                        if (Utils.GetWSURL("pricing_ws") != "")
                        {
                            m.Url = Utils.GetWSURL("pricing_ws");
                        }

                        TvinciPricing.PrePaidModule thePrePaid = null;
                        string sLocaleForCache = Utils.GetLocaleStringForCache("", "", "");
                        if (CachingManager.CachingManager.Exist("GetPrePaidModuleData" + sPrePaidCode + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                        {
                            thePrePaid = (TvinciPricing.PrePaidModule)(CachingManager.CachingManager.GetCachedData("GetPrePaidModuleData" + sPrePaidCode + "_" + m_nGroupID.ToString() + sLocaleForCache));
                        }
                        else
                        {
                            string sWSUserName = "";
                            string sWSPass = "";
                            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPrePaidModuleData", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
                            thePrePaid = m.GetPrePaidModuleData(sWSUserName, sWSPass, int.Parse(sPrePaidCode), string.Empty, string.Empty, string.Empty);
                            CachingManager.CachingManager.SetCachedData("GetPrePaidModuleData" + sPrePaidCode + "_" + m_nGroupID.ToString() + sLocaleForCache, thePrePaid, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                        }

                        theResp.m_Transactions[i].m_sPurchasedItemCode = sPrePaidCode;
                        theResp.m_Transactions[i].m_sPurchasedItemName = thePrePaid.m_Title;
                    }
                    catch (Exception)
                    { }
                }

                #endregion

                #region Handle Subscription Data

                if (!string.IsNullOrEmpty(sSubscriptionCode))
                {
                    theResp.m_Transactions[i].m_eItemType = BillingItemsType.Subscription;

                    try
                    {
                        TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule();
                        if (Utils.GetWSURL("pricing_ws") != "")
                        {
                            m.Url = Utils.GetWSURL("pricing_ws");
                        }

                        TvinciPricing.Subscription theSub = null;
                        string sLocaleForCache = Utils.GetLocaleStringForCache("", "", "");
                        if (CachingManager.CachingManager.Exist("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                        {
                            theSub = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache));
                        }
                        else
                        {
                            string sWSUserName = "";
                            string sWSPass = "";
                            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetSubscriptionData", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
                            theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubscriptionCode, "", "", "", true);
                            CachingManager.CachingManager.SetCachedData("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache, theSub, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                        }

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
                                {
                                    theResp.m_Transactions[i].m_sPurchasedItemName = sVal;
                                }
                            }
                        }

                        theResp.m_Transactions[i].m_sPurchasedItemCode = sSubscriptionCode;
                        //Int32 nIsRecurring = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("subscriptions_purchases" , "IS_RECURRING_STATUS" , nPurchaseID).ToString());
                        theResp.m_Transactions[i].m_bIsRecurring = theSub.m_bIsRecurring;
                    }
                    catch (Exception)
                    {
                    }
                }

                #endregion


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

                //actin date
                theResp.m_Transactions[i].m_dtActionDate = dActionDate;

                //Subscription dates
                if (nPurchaseID != 0)
                {
                    GetSubscriptionDates(nPurchaseID, ref theResp.m_Transactions[i].m_dtStartDate, ref theResp.m_Transactions[i].m_dtEndDate);
                }

                #region Handle Price Data

                if (!string.IsNullOrEmpty(sCurrencyCode))
                {
                    try
                    {
                        theResp.m_Transactions[i].m_Price = new ConditionalAccess.TvinciPricing.Price();
                        theResp.m_Transactions[i].m_Price.m_dPrice = dPrice;
                        TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
                        string sWSURL = Utils.GetWSURL("pricing_ws");
                        if (sWSURL != "")
                        {
                            m.Url = sWSURL;
                        }

                        if (CachingManager.CachingManager.Exist("GetCurrencyValues" + sCurrencyCode) == true)
                        {
                            theResp.m_Transactions[i].m_Price.m_oCurrency = (TvinciPricing.Currency)(CachingManager.CachingManager.GetCachedData("GetCurrencyValues" + sCurrencyCode));
                        }
                        else
                        {
                            string sWSUserName = "";
                            string sWSPass = "";
                            string sIP = "1.1.1.1";
                            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetCurrencyValues", "pricing", sIP, ref sWSUserName, ref sWSPass);
                            theResp.m_Transactions[i].m_Price.m_oCurrency = m.GetCurrencyValues(sWSUserName, sWSPass, sCurrencyCode);
                            CachingManager.CachingManager.SetCachedData("GetCurrencyValues" + sCurrencyCode, theResp.m_Transactions[i].m_Price.m_oCurrency, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                #endregion
            }

            return theResp;
        }


        /// <summary>
        /// Get User Billing History
        /// </summary>
        public virtual BillingTransactionsResponse GetUserBillingHistory(string sUserGUID, Int32 nStartIndex, Int32 nNumberOfItems)
        {
            DateTime minDate = new DateTime(2000, 1, 1);
            BillingTransactionsResponse res = GetUserBillingHistoryExt(sUserGUID, minDate, DateTime.MaxValue, nStartIndex, nNumberOfItems);
            return res;

            #region OLD
            //BillingTransactionsResponse theResp = new BillingTransactionsResponse();

            //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            //Int32 nTopNum = nStartIndex + nNumberOfItems;
            //selectQuery += "select top " + nTopNum.ToString() + " * from billing_transactions WITH (NOLOCK) where billing_status=0 and ";
            //selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
            //selectQuery += " and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sUserGUID);
            //selectQuery += " order by id desc ";
            //if (selectQuery.Execute("query", true) != null)
            //{
            //    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            //    theResp.m_nTransactionsCount = nCount;
            //    if (nTopNum > nCount)
            //        nTopNum = nCount;
            //    if (nCount > 0)
            //    {
            //        theResp.m_Transactions = new BillingTransactionContainer[nCount];
            //    }
            //    for (int i = nStartIndex; i < nTopNum; i++)
            //    {
            //        theResp.m_Transactions[i] = new BillingTransactionContainer();
            //        string sCurrencyCode = selectQuery.Table("query").DefaultView[i].Row["CURRENCY_CODE"].ToString();
            //        string sRemarks = "";
            //        if (selectQuery.Table("query").DefaultView[i].Row["REMARKS"] != null &&
            //            selectQuery.Table("query").DefaultView[i].Row["REMARKS"] != DBNull.Value)
            //            sRemarks = selectQuery.Table("query").DefaultView[i].Row["REMARKS"].ToString();
            //        double dPrice = GetSafeDouble(selectQuery.Table("query").DefaultView[i].Row["TOTAL_PRICE"]);
            //        Int32 nPurchaseID = GetSafeInt(selectQuery.Table("query").DefaultView[i].Row["PURCHASE_ID"]);
            //        DateTime dActionDate = (DateTime)(selectQuery.Table("query").DefaultView[i].Row["CREATE_DATE"]);
            //        string sSubscriptionCode = selectQuery.Table("query").DefaultView[i].Row["SUBSCRIPTION_CODE"].ToString();
            //        Int32 nMediaID = GetSafeInt(selectQuery.Table("query").DefaultView[i].Row["MEDIA_ID"]);
            //        string sLAST_FOUR_DIGITS = selectQuery.Table("query").DefaultView[i].Row["LAST_FOUR_DIGITS"].ToString();
            //        string sCellNum = selectQuery.Table("query").DefaultView[i].Row["CELL_PHONE"].ToString();
            //        string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
            //        Int32 nPAYMENT_NUMBER = GetSafeInt(selectQuery.Table("query").DefaultView[i].Row["PAYMENT_NUMBER"]);
            //        Int32 nNUMBER_OF_PAYMENTS = GetSafeInt(selectQuery.Table("query").DefaultView[i].Row["NUMBER_OF_PAYMENTS"]);
            //        Int32 nBILLING_METHOD = GetSafeInt(selectQuery.Table("query").DefaultView[i].Row["BILLING_METHOD"]);
            //        Int32 nBILLING_PROCESSOR = GetSafeInt(selectQuery.Table("query").DefaultView[i].Row["BILLING_PROCESSOR"]);
            //        Int32 nNEW_RENEWABLE_STATUS = GetSafeInt(selectQuery.Table("query").DefaultView[i].Row["NEW_RENEWABLE_STATUS"]);
            //        Int32 nBILLING_PROVIDER = GetSafeInt(selectQuery.Table("query").DefaultView[i].Row["BILLING_PROVIDER"]);
            //        Int32 nBILLING_PROVIDER_REFFERENCE = GetSafeInt(selectQuery.Table("query").DefaultView[i].Row["BILLING_PROVIDER_REFFERENCE"]);
            //        Int32 nPURCHASE_ID = GetSafeInt(selectQuery.Table("query").DefaultView[i].Row["PURCHASE_ID"]);

            //        string sPrePaidCode = Utils.GetStrSafeVal(ref selectQuery, "pre_paid_code", i);


            //        if (nBILLING_PROVIDER == -1)
            //        {
            //            if (nNEW_RENEWABLE_STATUS == 0)
            //                theResp.m_Transactions[i].m_eBillingAction = BillingAction.CancelSubscriptionOrder;
            //            if (nNEW_RENEWABLE_STATUS == 1)
            //                theResp.m_Transactions[i].m_eBillingAction = BillingAction.RenewCancledSubscription;
            //        }
            //        else if (nBILLING_PROVIDER == -2)
            //        {
            //            theResp.m_Transactions[i].m_eBillingAction = BillingAction.SubscriptionDateChanged;
            //        }
            //        else
            //        {
            //            if (nPAYMENT_NUMBER == 1)
            //                theResp.m_Transactions[i].m_eBillingAction = BillingAction.Purchase;
            //            if (nPAYMENT_NUMBER > 1)
            //                theResp.m_Transactions[i].m_eBillingAction = BillingAction.RenewPayment;
            //        }

            //        if (sPrePaidCode != "")
            //            theResp.m_Transactions[i].m_eItemType = BillingItemsType.PrePaid;
            //        if (sSubscriptionCode != "")
            //            theResp.m_Transactions[i].m_eItemType = BillingItemsType.Subscription;
            //        if (nMediaID != 0)
            //            theResp.m_Transactions[i].m_eItemType = BillingItemsType.PPV;

            //        //if (nBILLING_METHOD >= 1)
            //        PaymentMethod pm = (PaymentMethod)(nBILLING_METHOD);
            //        theResp.m_Transactions[i].m_ePaymentMethod = pm;



            //        if (pm == PaymentMethod.CreditCard || pm == PaymentMethod.Visa || pm == PaymentMethod.MasterCard)
            //            theResp.m_Transactions[i].m_sPaymentMethodExtraDetails = sLAST_FOUR_DIGITS;
            //        if (pm == PaymentMethod.SMS)
            //            theResp.m_Transactions[i].m_sPaymentMethodExtraDetails = sCellNum;
            //        theResp.m_Transactions[i].m_bIsRecurring = false;



            //        if (!string.IsNullOrEmpty(sPrePaidCode))
            //        {
            //            TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule();
            //            if (Utils.GetWSURL("pricing_ws") != "")
            //                m.Url = Utils.GetWSURL("pricing_ws");

            //            TvinciPricing.PrePaidModule thePrePaid = null;
            //            string sLocaleForCache = Utils.GetLocaleStringForCache("", "", "");
            //            if (CachingManager.CachingManager.Exist("GetPrePaidModuleData" + sPrePaidCode + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
            //                thePrePaid = (TvinciPricing.PrePaidModule)(CachingManager.CachingManager.GetCachedData("GetPrePaidModuleData" + sPrePaidCode + "_" + m_nGroupID.ToString() + sLocaleForCache));
            //            else
            //            {
            //                string sWSUserName = "";
            //                string sWSPass = "";
            //                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPrePaidModuleData", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
            //                thePrePaid = m.GetPrePaidModuleData(sWSUserName, sWSPass, int.Parse(sPrePaidCode), string.Empty, string.Empty, string.Empty);
            //                CachingManager.CachingManager.SetCachedData("GetPrePaidModuleData" + sPrePaidCode + "_" + m_nGroupID.ToString() + sLocaleForCache, thePrePaid, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            //            }

            //            theResp.m_Transactions[i].m_sPurchasedItemCode = sPrePaidCode;
            //            theResp.m_Transactions[i].m_sPurchasedItemName = thePrePaid.m_Title;
            //        }

            //        if (sSubscriptionCode != "")
            //        {
            //            TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule();
            //            if (Utils.GetWSURL("pricing_ws") != "")
            //                m.Url = Utils.GetWSURL("pricing_ws");

            //            TvinciPricing.Subscription theSub = null;
            //            string sLocaleForCache = Utils.GetLocaleStringForCache("", "", "");
            //            if (CachingManager.CachingManager.Exist("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
            //                theSub = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache));
            //            else
            //            {
            //                string sWSUserName = "";
            //                string sWSPass = "";
            //                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetSubscriptionData", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
            //                theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sSubscriptionCode, "", "", "", true);
            //                CachingManager.CachingManager.SetCachedData("GetSubscriptionData" + sSubscriptionCode + "_" + m_nGroupID.ToString() + sLocaleForCache, theSub, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            //            }
            //            string sMainLang = "";
            //            string sMainLangCode = "";
            //            GetMainLang(ref sMainLang, ref sMainLangCode, m_nGroupID);
            //            if (theSub.m_sName != null)
            //            {
            //                Int32 nNameLangLength = theSub.m_sName.Length;
            //                for (int j = 0; j < nNameLangLength; j++)
            //                {
            //                    string sLang = theSub.m_sName[j].m_sLanguageCode3;
            //                    string sVal = theSub.m_sName[j].m_sValue;
            //                    if (sLang == sMainLangCode)
            //                        theResp.m_Transactions[i].m_sPurchasedItemName = sVal;
            //                }
            //            }
            //            theResp.m_Transactions[i].m_sPurchasedItemCode = sSubscriptionCode;
            //            //Int32 nIsRecurring = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("subscriptions_purchases" , "IS_RECURRING_STATUS" , nPurchaseID).ToString());
            //            theResp.m_Transactions[i].m_bIsRecurring = theSub.m_bIsRecurring;
            //        }
            //        if (nMediaID != 0)
            //        {
            //            theResp.m_Transactions[i].m_sPurchasedItemName = GetMediaTitle(nMediaID);
            //            theResp.m_Transactions[i].m_sPurchasedItemCode = nMediaID.ToString();
            //        }
            //        theResp.m_Transactions[i].m_sRecieptCode = sID;
            //        theResp.m_Transactions[i].m_nBillingProviderRef = nBILLING_PROVIDER_REFFERENCE;
            //        theResp.m_Transactions[i].m_nPurchaseID = nPURCHASE_ID;
            //        theResp.m_Transactions[i].m_sRemarks = sRemarks;
            //        //actin date
            //        theResp.m_Transactions[i].m_dtActionDate = dActionDate;
            //        //Subscription dates
            //        if (nPurchaseID != 0)
            //            GetSubscriptionDates(nPurchaseID, ref theResp.m_Transactions[i].m_dtStartDate, ref theResp.m_Transactions[i].m_dtEndDate);
            //        //Price
            //        if (sCurrencyCode != "")
            //        {
            //            theResp.m_Transactions[i].m_Price = new ConditionalAccess.TvinciPricing.Price();
            //            theResp.m_Transactions[i].m_Price.m_dPrice = dPrice;
            //            TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
            //            string sWSURL = Utils.GetWSURL("pricing_ws");
            //            if (sWSURL != "")
            //                m.Url = sWSURL;

            //            if (CachingManager.CachingManager.Exist("GetCurrencyValues" + sCurrencyCode) == true)
            //                theResp.m_Transactions[i].m_Price.m_oCurrency = (TvinciPricing.Currency)(CachingManager.CachingManager.GetCachedData("GetCurrencyValues" + sCurrencyCode));
            //            else
            //            {
            //                string sWSUserName = "";
            //                string sWSPass = "";
            //                string sIP = "1.1.1.1";
            //                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetCurrencyValues", "pricing", sIP, ref sWSUserName, ref sWSPass);
            //                theResp.m_Transactions[i].m_Price.m_oCurrency = m.GetCurrencyValues(sWSUserName, sWSPass, sCurrencyCode);
            //                CachingManager.CachingManager.SetCachedData("GetCurrencyValues" + sCurrencyCode, theResp.m_Transactions[i].m_Price.m_oCurrency, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            //            }
            //        }

            //        //ret[i].Ini
            //    }

            //}
            //selectQuery.Finish();
            //selectQuery = null;
            //return theResp;

            #endregion
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
        /// Send Subscription Uses Notification  
        /// </summary>
        protected virtual void HandleSubscriptionUsesNotification(Int32 nMediaFileID, string sSubCode, string sSiteGUID)
        {
            return;
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

                string sIP = "1.1.1.1";
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "SetCouponUses", "pricing", sIP, ref sWSUserName, ref sWSPass);
                TvinciPricing.mdoule m = null;
                try
                {
                    m = new global::ConditionalAccess.TvinciPricing.mdoule();
                    string sWSURL = Utils.GetWSURL("pricing_ws");
                    if (sWSURL.Length > 0)
                        m.Url = sWSURL;
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
                    Logger.Logger.Log("HandleCouponUses", sb.ToString(), "BaseConditionalAccess");
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
            string sSiteGUID, double dPrice, string sCurrency,
            Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCampaignCode, string sCouponCode, string sUserIP,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return GetCustomData(relevantSub, thePPVModule, campaign,
             sSiteGUID, dPrice, sCurrency,
             nMediaFileID, nMediaID, sPPVModuleCode, sCampaignCode, sCouponCode, sUserIP,
             sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty);
        }
        /// <summary>
        /// Get Custom Data For Pre Paid
        /// </summary>
        protected virtual string GetCustomDataForPrePaid(TvinciPricing.PrePaidModule thePrePaidModule, TvinciPricing.Campaign campaign, string sPrePaidCode, string sCampaignCode,
        string sSiteGUID, double dPrice, string sCurrency, string sCouponCode, string sUserIP,
        string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sOverrideEndDate)
        {
            //Logger.Logger.Log("Custom Data User IP", sUserIP + " " + sCountryCd + TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP), "ADCustomData");
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
            //Logger.Logger.Log("Custom Data User IP", sUserIP + " " + sCountryCd + TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP), "ADCustomData");
            return GetCustomDataForPrePaid(thePrePaidModule, campaign, sPrePaidCode, sCampaignCode,
           sSiteGUID, dPrice, sCurrency, sCouponCode, sUserIP,
           sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty);
        }
        /// <summary>
        /// Get Custom Data For Subscription
        /// </summary>
        protected virtual string GetCustomDataForSubscription(TvinciPricing.Subscription theSub, TvinciPricing.Campaign campaign, string sSubscriptionCode, string sCampaignCode,
    string sSiteGUID, double dPrice, string sCurrency, string sCouponCode, string sUserIP,
    string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sOverrideEndDate, string sPreviewModuleID)
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
           sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty, string.Empty);

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

            if (sSiteGUID == "")
            {
                ret.m_oStatus = PrePaidResponseStatus.UnKnownUser;
                ret.m_sStatusDescription = "Cant charge an unknown user";
            }
            else
            {
                TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL != "")
                    u.Url = sWSURL;

                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    ret.m_oStatus = PrePaidResponseStatus.UnKnownUser;
                    ret.m_sStatusDescription = "Cant charge an unknown user";
                }
                else
                {
                    //Get User Valid PP
                    UserPrePaidContainer userPPs = new UserPrePaidContainer();
                    userPPs.Initialize(sSiteGUID, sCurrency);

                    //UserPrePaidObject relUppo = null; 

                    sIP = "1.1.1.1";
                    sWSUserName = "";
                    sWSPass = "";

                    TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
                    sWSURL = Utils.GetWSURL("pricing_ws");
                    if (sWSURL != "")
                        m.Url = sWSURL;
                    Int32[] nMediaFiles = { nMediaFileID };
                    string sMediaFileForCache = Utils.ConvertArrayIntToStr(nMediaFiles);
                    TvinciPricing.MediaFilePPVModule[] oModules = null;
                    string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                    if (CachingManager.CachingManager.Exist("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                        oModules = (TvinciPricing.MediaFilePPVModule[])(CachingManager.CachingManager.GetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache));
                    else
                    {
                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleListForMediaFiles", "pricing", sIP, ref sWSUserName, ref sWSPass);
                        oModules = m.GetPPVModuleListForMediaFiles(sWSUserName, sWSPass, nMediaFiles, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        CachingManager.CachingManager.SetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache, oModules, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    }

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
                        ret.m_oStatus = PrePaidResponseStatus.UnKnownPPVModule;
                        ret.m_sStatusDescription = "This PPVModule does not belong to item";
                        try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(PP): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                        catch { }

                    }
                    if (bOK == true)
                    {
                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.Subscription relevantSub = null;
                        TvinciPricing.Collection relevantCol = null;
                        TvinciPricing.PrePaidModule relevantPP = null;

                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                        TvinciPricing.PPVModule thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (thePPVModule != null)
                        {
                            TvinciPricing.Price p = Utils.GetMediaFileFinalPrice(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
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

                                            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ppv_purchases");
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
                                            insertQuery.Finish();
                                            insertQuery = null;

                                            Int32 nPurchaseID = 0;
                                            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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
                                            selectQuery.Finish();
                                            selectQuery = null;


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

                                                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("ppv_purchases");
                                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_pp", "=", nRelPrePaidID);
                                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
                                                    updateQuery += "where";
                                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nPurchaseID);
                                                    updateQuery.Execute();
                                                    updateQuery.Finish();
                                                    updateQuery = null;

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
                                        try { WriteToUserLog(sSiteGUID, "Media file id: " + nMediaFileID.ToString() + " Purchased(PP): " + dPrice.ToString() + sCurrency); }
                                        catch { }
                                        //send purchase mail
                                        string sEmail = "";
                                        string sPaymentMethod = "Pre Paid";
                                        string sDateOfPurchase = GetDateSTRByGroup(DateTime.UtcNow, m_nGroupID);
                                        string sItemName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", nMediaID, "MAIN_CONNECTION_STRING").ToString();

                                        TvinciAPI.PurchaseMailRequest sMailReq = GetPurchaseMailRequest(ref sEmail, sSiteGUID, sItemName, sPaymentMethod, sDateOfPurchase, string.Empty, dPrice, sCurrency, m_nGroupID);
                                        TvinciAPI.API apiWs = new TvinciAPI.API();
                                        string sAPIWSUserName = "";
                                        string sAPIWSPass = "";
                                        TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetMail", "api", sIP, ref sAPIWSUserName, ref sAPIWSPass);
                                        string sAPIWSURL = Utils.GetWSURL("api_ws");
                                        if (sAPIWSURL != "")
                                            apiWs.Url = sAPIWSURL;
                                        apiWs.SendMailTemplate(sAPIWSUserName, sWSPass, sMailReq);
                                    }
                                    else
                                    {
                                        ret.m_sStatusDescription = "No Credit";
                                        try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(PP): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                                        catch { }
                                    }
                                }
                                else
                                {
                                    ret.m_oStatus = PrePaidResponseStatus.PriceNotCorrect;
                                    ret.m_sStatusDescription = "The price of the request is not the actual price";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(PP): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                                    catch { }
                                }
                            }
                            else
                            {
                                if (theReason == PriceReason.PPVPurchased)
                                {
                                    ret.m_oStatus = PrePaidResponseStatus.Fail;
                                    ret.m_sStatusDescription = "The media file is already purchased";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(PP): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                                    catch { }
                                }
                                else if (theReason == PriceReason.Free)
                                {
                                    ret.m_oStatus = PrePaidResponseStatus.Fail;
                                    ret.m_sStatusDescription = "The media file is free";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(PP): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                                    catch { }
                                }
                                else if (theReason == PriceReason.ForPurchaseSubscriptionOnly)
                                {
                                    ret.m_oStatus = PrePaidResponseStatus.Fail;
                                    ret.m_sStatusDescription = "The media file is for purchase with subscription only";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(PP): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                                    catch { }
                                }
                                else if (theReason == PriceReason.SubscriptionPurchased)
                                {
                                    ret.m_oStatus = PrePaidResponseStatus.Fail;
                                    ret.m_sStatusDescription = "The media file is already purchased (subscription)";
                                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(PP): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                                    catch { }
                                }
                            }
                        }
                        else
                        {
                            ret.m_oStatus = PrePaidResponseStatus.Fail;
                            ret.m_sStatusDescription = "The ppv module is unknown";
                            try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                            catch { }
                        }
                    }
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
            if (sSiteGUID == "")
            {
                ret.m_oStatus = PrePaidResponseStatus.UnKnownUser;
                ret.m_sStatusDescription = "Cant charge an unknown user";
            }
            else
            {
                TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (sWSURL != "")
                    u.Url = sWSURL;

                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    ret.m_oStatus = PrePaidResponseStatus.UnKnownUser;
                    ret.m_sStatusDescription = "Cant charge an unknown user";
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

                                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                                    updateQuery += " where ";
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                                    updateQuery += " and ";
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                                    updateQuery += " and ";
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                    updateQuery.Execute();
                                    updateQuery.Finish();
                                    updateQuery = null;

                                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscriptions_purchases");
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
                                        //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", GetCurrentDBTime().AddSeconds(theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle));
                                    }
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_pp", "=", userPPs.m_oUserPPs[0].m_nPPModuleID);
                                    insertQuery.Execute();
                                    insertQuery.Finish();
                                    insertQuery = null;


                                    Int32 nPurchaseID = 0;

                                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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
                                    selectQuery.Finish();
                                    selectQuery = null;

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
                                            updateQuery.Finish();
                                            updateQuery = null;

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
                                try { WriteToUserLog(sSiteGUID, "Subscription purchase (PP): " + sSubscriptionCode); }
                                catch { }
                            }
                            else
                            {
                                ret.m_sStatusDescription = "No Credit";
                                try { WriteToUserLog(sSiteGUID, "while trying to purchase subscription(PP): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription); }
                                catch { }
                            }
                        }
                        else
                        {
                            ret.m_oStatus = PrePaidResponseStatus.PriceNotCorrect;
                            ret.m_sStatusDescription = "The price of the request is not the actual price";
                            try { WriteToUserLog(sSiteGUID, "while trying to purchase subscription(PP): " + " error returned: " + ret.m_sStatusDescription); }
                            catch { }
                        }
                    }
                    else
                    {
                        if (theReason == PriceReason.Free)
                        {
                            ret.m_oStatus = PrePaidResponseStatus.Fail;
                            ret.m_sStatusDescription = "The subscription is free";
                            try { WriteToUserLog(sSiteGUID, "while trying to purchase subscription(PP): " + " error returned: " + ret.m_sStatusDescription); }
                            catch { }
                        }
                        if (theReason == PriceReason.SubscriptionPurchased)
                        {
                            ret.m_oStatus = PrePaidResponseStatus.Fail;
                            ret.m_sStatusDescription = "The subscription is already purchased";
                            try { WriteToUserLog(sSiteGUID, "while trying to purchase subscription(PP): " + " error returned: " + ret.m_sStatusDescription); }
                            catch { }
                        }
                    }
                }
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
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("pre_paid_purchases");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("amount_used", "=", nUsed + nAmount);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
            updateQuery += "where";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nPPPurchaseID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }
        /// <summary>
        /// Insert PP Uses Record
        /// </summary>
        private void InsertPPUsesRecord(Int32 nPurchaseID, Int32 nItemID, BillingItemsType eItemType, string sSiteGUID, string sCurrency, Int32 nPPCD, Int32 nPPPurchaseID,
            double dPrice, double dCredit,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("pre_paid_uses");
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
            insertQuery.Finish();
            insertQuery = null;
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
            DateTime dLastDate = DateTime.Now;

            PrePaidHistoryResponse theResp = new PrePaidHistoryResponse();


            List<PrePaidHistoryContainer> items = new List<PrePaidHistoryContainer>();
            PrePaidHistoryContainer pphc = null;


            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select top " + nNumberOfItems + " item_id, item_type, currency_cd, SUM(price) as price, min(remains_credit) as remains_credit, MIN(create_date) as date, purchase_id from pre_paid_uses";
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
                    Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery, "item_id", i);
                    Int32 nItemType = Utils.GetIntSafeVal(ref selectQuery, "item_type", i);
                    string sCurrency = Utils.GetStrSafeVal(ref selectQuery, "currency_cd", i);
                    double dPrice = Utils.GetDoubleSafeVal(ref selectQuery, "price", i);
                    double dCredit = Utils.GetDoubleSafeVal(ref selectQuery, "remains_credit", i);
                    DateTime dDate = Utils.GetDateSafeVal(ref selectQuery, "date", i);

                    if (dLastCredit != dCredit)
                    {
                        ODBCWrapper.DataSetSelectQuery selectQueryE = new ODBCWrapper.DataSetSelectQuery();
                        selectQueryE += "select * from pre_paid_purchases where is_active=1 and status=1 and";
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

                                Int32 nPPID = Utils.GetIntSafeVal(ref selectQueryE, "pre_paid_module_id", j);
                                double dlostAmount = Utils.GetDoubleSafeVal(ref selectQueryE, "total_amount", j) - Utils.GetDoubleSafeVal(ref selectQueryE, "amount_used", j);
                                DateTime dExpired = Utils.GetDateSafeVal(ref selectQueryE, "end_date", j);

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
                                if (Utils.GetWSURL("pricing_ws") != "")
                                    m.Url = Utils.GetWSURL("pricing_ws");

                                TvinciPricing.PrePaidModule thePrePaid = null;
                                string sLocaleForCache = Utils.GetLocaleStringForCache("", "", "");
                                if (CachingManager.CachingManager.Exist("GetPrePaidModuleData" + nPPID + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                                    thePrePaid = (TvinciPricing.PrePaidModule)(CachingManager.CachingManager.GetCachedData("GetPrePaidModuleData" + nPPID + "_" + m_nGroupID.ToString() + sLocaleForCache));
                                else
                                {
                                    string sWSUserName = "";
                                    string sWSPass = "";
                                    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPrePaidModuleData", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
                                    thePrePaid = m.GetPrePaidModuleData(sWSUserName, sWSPass, nPPID, string.Empty, string.Empty, string.Empty);
                                    CachingManager.CachingManager.SetCachedData("GetPrePaidModuleData" + nPPID + "_" + m_nGroupID.ToString() + sLocaleForCache, thePrePaid, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                                }

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

                        Int32 nMediaID = Utils.GetMediaIDFeomFileID(nItemID, m_nGroupID);
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
                        if (Utils.GetWSURL("pricing_ws") != "")
                            m.Url = Utils.GetWSURL("pricing_ws");

                        TvinciPricing.Subscription theSub = null;
                        string sLocaleForCache = Utils.GetLocaleStringForCache("", "", "");
                        if (CachingManager.CachingManager.Exist("GetSubscriptionData" + nItemID + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                            theSub = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData("GetSubscriptionData" + nItemID + "_" + m_nGroupID.ToString() + sLocaleForCache));
                        else
                        {
                            string sWSUserName = "";
                            string sWSPass = "";
                            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetSubscriptionData", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
                            theSub = m.GetSubscriptionData(sWSUserName, sWSPass, nItemID.ToString(), "", "", "", true);
                            CachingManager.CachingManager.SetCachedData("GetSubscriptionData" + nItemID + "_" + m_nGroupID.ToString() + sLocaleForCache, theSub, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                        }
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
                        if (Utils.GetWSURL("pricing_ws") != "")
                            m.Url = Utils.GetWSURL("pricing_ws");

                        TvinciPricing.PrePaidModule thePrePaid = null;
                        string sLocaleForCache = Utils.GetLocaleStringForCache("", "", "");
                        if (CachingManager.CachingManager.Exist("GetPrePaidModuleData" + nItemID + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                            thePrePaid = (TvinciPricing.PrePaidModule)(CachingManager.CachingManager.GetCachedData("GetPrePaidModuleData" + nItemID + "_" + m_nGroupID.ToString() + sLocaleForCache));
                        else
                        {
                            string sWSUserName = "";
                            string sWSPass = "";
                            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPrePaidModuleData", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
                            thePrePaid = m.GetPrePaidModuleData(sWSUserName, sWSPass, nItemID, string.Empty, string.Empty, string.Empty);
                            CachingManager.CachingManager.SetCachedData("GetPrePaidModuleData" + nItemID + "_" + m_nGroupID.ToString() + sLocaleForCache, thePrePaid, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                        }

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
            Int32 nMediaFileID = 0;
            if (bIsCoGuid)
            {
                nMediaFileID = Utils.GetMediaFileIDWithCoGuid(m_nGroupID, sMediaFileID);
            }
            else
            {
                nMediaFileID = int.Parse(sMediaFileID);
            }

            if (nMediaFileID == 0)
                return TimeSpan.Zero.ToString();

            Int32[] nMediaFileIDs = { nMediaFileID };
            MediaFileItemPricesContainer[] prices = GetItemsPrices(nMediaFileIDs, sSiteGUID, string.Empty, true, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);

            if (prices != null && prices.Length > 0 && (prices[0].m_oItemPrices == null || prices[0].m_oItemPrices[0].m_PriceReason == PriceReason.Free))
            {
                TimeSpan ts = new TimeSpan(2, 0, 0, 0);

                string val = Utils.GetValueFromConfig(string.Format("free_left_view_{0}", m_nGroupID));

                if (!string.IsNullOrEmpty(val))
                {
                    DateTime dEndDate = Utils.GetEndDateTime(DateTime.UtcNow, int.Parse(val), true);
                    ts = dEndDate.Subtract(DateTime.UtcNow);
                }

                return ts.ToString();
            }

            Int32 nOffline_status = 0;
            string sPPVMCode = string.Empty;
            Int32 nUsageModuleID = 0;
            Int32 nViewLifeCycle = 0;
            DateTime dPurchaseDate = new DateTime();

            List<int> lUsersIds = ConditionalAccess.Utils.GetAllUsersDomainBySiteGUID(sSiteGUID, m_nGroupID);

            DataTable dt = DAL.ConditionalAccessDAL.Get_LatestFileUse(lUsersIds, nMediaFileID);

            DateTime dNow = new DateTime();
            if (dt != null)
            {
                Int32 nCount = dt.Rows.Count;
                if (nCount > 0)
                {
                    #region Get View Life Cycle
                    nOffline_status = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["Offline_status"]);
                    sPPVMCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["ppvmodule_code"]);
                    dPurchaseDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0]["create_date"]);
                    dNow = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0]["dNow"]);
                    if (nOffline_status == 1)
                    {
                        #region Get Offline view life cycle
                        //get the last file uses


                        ODBCWrapper.DataSetSelectQuery selectgrouppramater = new ODBCWrapper.DataSetSelectQuery();
                        selectgrouppramater += "select usage_module_code from Pricing.dbo.groups_parameters(nolock)";
                        selectgrouppramater += "where ";
                        selectgrouppramater += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                        if (selectgrouppramater.Execute("query", true) != null)
                        {
                            Int32 nCountparam = selectgrouppramater.Table("query").DefaultView.Count;
                            if (nCount > 0)
                            {
                                string nUsageModelCode = Utils.GetStrSafeVal(ref selectgrouppramater, "USAGE_MODULE_CODE", 0);

                                TvinciPricing.UsageModule tpmdoule = new TvinciPricing.UsageModule();

                                string sWSUserName = "";
                                string sWSPass = "";
                                TvinciPricing.mdoule m = new TvinciPricing.mdoule();
                                if (Utils.GetWSURL("pricing_ws") != "")
                                    m.Url = Utils.GetWSURL("pricing_ws");

                                if (CachingManager.CachingManager.Exist("GetOfflineUsageModuleData" + nUsageModelCode + "_" + m_nGroupID.ToString()) == true)
                                    tpmdoule = (TvinciPricing.UsageModule)(CachingManager.CachingManager.GetCachedData("GetOfflineUsageModuleData" + nUsageModelCode + "_" + m_nGroupID.ToString()));
                                else
                                {

                                    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetOfflineData", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
                                    tpmdoule = m.GetUsageModuleData(sWSUserName, sWSPass, nUsageModelCode, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);
                                    CachingManager.CachingManager.SetCachedData("GetOffLineUsageModuleData" + nUsageModelCode + "_" + m_nGroupID.ToString(), nUsageModelCode, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                                }

                                if (tpmdoule != null)
                                {
                                    nViewLifeCycle = tpmdoule.m_tsMaxUsageModuleLifeCycle;
                                }
                            }
                        }
                        selectgrouppramater.Finish();
                        selectgrouppramater = null;
                        #endregion
                    }
                    else
                    {
                        //Check if is off Line
                        if (sPPVMCode.Contains("s:"))
                        {
                            #region Get Subscription usage module view life cycle
                            Int32 nSubID = Convert.ToInt32(sPPVMCode.Split(' ')[1]);
                            nUsageModuleID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "usage_module_code", nSubID, "pricing_connection").ToString());

                            TvinciPricing.Subscription theSub = null;

                            string sWSUserName = "";
                            string sWSPass = "";
                            TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule();
                            if (Utils.GetWSURL("pricing_ws") != "")
                                m.Url = Utils.GetWSURL("pricing_ws");

                            if (CachingManager.CachingManager.Exist("GetSubscriptionData" + nSubID + "_" + m_nGroupID.ToString()) == true)
                                theSub = (TvinciPricing.Subscription)(CachingManager.CachingManager.GetCachedData("GetSubscriptionData" + nSubID + "_" + m_nGroupID.ToString()));
                            else
                            {
                                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetSubscriptionData", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
                                theSub = m.GetSubscriptionData(sWSUserName, sWSPass, nSubID.ToString(), sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, false);
                                CachingManager.CachingManager.SetCachedData("GetSubscriptionData" + nSubID + "_" + m_nGroupID.ToString(), theSub, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                            }

                            if (theSub != null && theSub.m_oSubscriptionUsageModule != null)
                            {
                                TvinciPricing.UsageModule u = theSub.m_oSubscriptionUsageModule;
                                nViewLifeCycle = u.m_tsViewLifeCycle;
                            }
                            #endregion
                        }
                        else if(sPPVMCode.Contains("c:"))
                        {
                            #region Get Collection usage module view life cycle
                            Int32 nColID = Convert.ToInt32(sPPVMCode.Split(' ')[1]);
                            nUsageModuleID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("collections", "usage_module_id", nColID, "pricing_connection").ToString());

                            TvinciPricing.Collection theCol = null;

                            string sWSUserName = "";
                            string sWSPass = "";
                            TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule();
                            if (Utils.GetWSURL("pricing_ws") != "")
                                m.Url = Utils.GetWSURL("pricing_ws");

                            if (CachingManager.CachingManager.Exist("GetCollectionData" + nColID + "_" + m_nGroupID.ToString()) == true)
                                theCol = (TvinciPricing.Collection)(CachingManager.CachingManager.GetCachedData("GetCollectionData" + nColID + "_" + m_nGroupID.ToString()));
                            else
                            {
                                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetCollectionData", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
                                theCol = m.GetCollectionData(sWSUserName, sWSPass, nColID.ToString(), sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, false);
                                CachingManager.CachingManager.SetCachedData("GetCollectionData" + nColID + "_" + m_nGroupID.ToString(), theCol, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                            }

                            if (theCol != null && theCol.m_oCollectionUsageModule != null)
                            {
                                TvinciPricing.UsageModule u = theCol.m_oCollectionUsageModule;
                                nViewLifeCycle = u.m_tsViewLifeCycle;
                            }
                            #endregion
                        }
                        else
                        {
                            #region PPVModule view life cycle
                            TvinciPricing.PPVModule thePPVModule = null;

                            string sWSUserName = "";
                            string sWSPass = "";
                            TvinciPricing.mdoule m = new ConditionalAccess.TvinciPricing.mdoule();
                            if (Utils.GetWSURL("pricing_ws") != "")
                                m.Url = Utils.GetWSURL("pricing_ws");

                            if (CachingManager.CachingManager.Exist("GetPPVModuleData" + sPPVMCode + "_" + m_nGroupID.ToString()) == true)
                                thePPVModule = (TvinciPricing.PPVModule)(CachingManager.CachingManager.GetCachedData("GetPPVModuleData" + sPPVMCode + "_" + m_nGroupID.ToString()));
                            else
                            {
                                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
                                thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVMCode, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);
                                CachingManager.CachingManager.SetCachedData("GetPPVModuleData" + sPPVMCode + "_" + m_nGroupID.ToString(), thePPVModule, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                            }

                            if (thePPVModule != null && thePPVModule.m_oUsageModule != null)
                                nViewLifeCycle = thePPVModule.m_oUsageModule.m_tsViewLifeCycle;
                            #endregion
                        }
                    }
                    #endregion
                }
            }

            if (nViewLifeCycle > 0)
            {
                DateTime dEndDate = Utils.GetEndDateTime(dPurchaseDate, nViewLifeCycle);

                TimeSpan ts = dEndDate.Subtract(dNow);
                return ts.ToString();
            }

            return TimeSpan.Zero.ToString();
        }

        public String SerializeToXML<T>(T objectToSerialize)
        {
            StringBuilder sb = new StringBuilder();

            XmlWriterSettings settings =
                new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };

            Type myType = this.GetType();
            using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
            {
                if (xmlWriter != null)
                {

                    Type myType2 = objectToSerialize.GetType();

                    // switch (((CachingDataResponse)objectToSerialize).OVal.GetType().FullName)
                    //switch (objectToSerialize.GetType().GenericParameterAttributes
                    //    .OVal.GetType().FullName)
                    //{
                    //    case "ConditionalAccess.TvinciPricing.MediaFilePPVModule[]":
                    //        break;
                    //    case "ConditionalAccess.TvinciAPI.MeidaMaper[]":
                    //        break;
                    //    default:
                    new XmlSerializer(typeof(T)).Serialize(xmlWriter, objectToSerialize);
                    //        break;
                    //}

                }
            }

            sb = sb.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
            return sb.ToString();
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

                Logger.Logger.Log("Cellular_BaseChargeUserForMediaFile", string.Format("Entering Cellular_BaseChargeUserForMediaFile try block. Site Guid: {0} , Media File ID: {1} , Media ID: {2} , PPV Module Code: {3} , Coupon code: {4} , User IP: {5} , Dummy: {6}", sSiteGUID, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, bDummy.ToString().ToLower()), GetLogFilename());

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
                    string sIP = "1.1.1.1";
                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (sWSURL.Length > 0)
                        u.Url = sWSURL;

                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cant charge an unknown user";
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

                        sIP = "1.1.1.1";
                        sWSUserName = string.Empty;
                        sWSPass = string.Empty;

                        m = new global::ConditionalAccess.TvinciPricing.mdoule();
                        sWSURL = Utils.GetWSURL("pricing_ws");
                        if (sWSURL.Length > 0)
                            m.Url = sWSURL;
                        Int32[] nMediaFiles = { nMediaFileID };
                        string sMediaFileForCache = Utils.ConvertArrayIntToStr(nMediaFiles);
                        TvinciPricing.MediaFilePPVModule[] oModules = null;
                        string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (CachingManager.CachingManager.Exist("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
                            oModules = (TvinciPricing.MediaFilePPVModule[])(CachingManager.CachingManager.GetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache));
                        else
                        {
                            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleListForMediaFiles", "pricing", sIP, ref sWSUserName, ref sWSPass);
                            oModules = m.GetPPVModuleListForMediaFiles(sWSUserName, sWSPass, nMediaFiles, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                            CachingManager.CachingManager.SetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache, oModules, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
                        }

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
                            if (!bDummy)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownPPVModule;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "This PPVModule does not belong to item";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                            else
                            {
                                bOK = true;
                                if (nCount > 0)
                                {
                                    sPPVModuleCode = oModules[0].m_oPPVModules[0].m_sObjectCode;
                                    dPrice = oModules[0].m_oPPVModules[0].m_oPriceCode.m_oPrise.m_dPrice;
                                    sCurrency = oModules[0].m_oPPVModules[0].m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3;
                                }
                            }
                        }
                        if (bOK)
                        {
                            PriceReason theReason = PriceReason.UnKnown;

                            TvinciPricing.Subscription relevantSub = null;
                            TvinciPricing.Collection relevantCol = null;
                            TvinciPricing.PrePaidModule relevantPP = null;

                            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                            TvinciPricing.PPVModule thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                            if (thePPVModule != null)
                            {
                                TvinciPricing.Price p = Utils.GetMediaFileFinalPrice(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                                if (theReason == PriceReason.ForPurchase || (theReason == PriceReason.SubscriptionPurchased && p.m_dPrice > 0) || bDummy)
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

                                            Logger.Logger.Log("CustomData", sCustomData, "CustomData");

                                            ret = HandleCellularChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, 1, sExtraParameters, bDummy, false, ref bm);
                                        }
                                        if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
                                        {
                                            long lBillingTransactionID = 0;
                                            long lPurchaseID = 0;
                                            HandleChargeUserForMediaFileBillingSuccess(sSiteGUID, relevantSub, dPrice, sCurrency,
                                                                                       sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, ret, sCustomData,
                                                                                       thePPVModule, nMediaFileID, ref lBillingTransactionID, ref lPurchaseID);
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
                                        WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                                    }
                                    else if (theReason == PriceReason.Free)
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                        ret.m_sRecieptCode = string.Empty;
                                        ret.m_sStatusDescription = "The media file is free";
                                        WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                                    }
                                    else if (theReason == PriceReason.ForPurchaseSubscriptionOnly)
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                        ret.m_sRecieptCode = string.Empty;
                                        ret.m_sStatusDescription = "The media file is for purchase with subscription only";
                                        WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                                    }
                                    else if (theReason == PriceReason.SubscriptionPurchased)
                                    {
                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                        ret.m_sRecieptCode = string.Empty;
                                        ret.m_sStatusDescription = "The media file is already purchased (subscription)";
                                        WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                                    }
                                }
                            }
                            else
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = string.Empty;
                                ret.m_sStatusDescription = "The ppv module is unknown";
                                WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                            }
                        }
                    }
                } // end else if siteguid == ""
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

                Logger.Logger.Log("Cellular_BaseChargeUserForMediaFile", sb.ToString(), GetLogFilename());

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
                Logger.Logger.Log("Cellular_BaseChargeUserForSubscription", string.Format("Entering Cellular_BaseChargeUserForSubscription try block. Site Guid: {0} , Sub Code: {1} , Coupon Code: {2} , User IP: {3} , Dummy: {4}", sSiteGUID, sSubscriptionCode, sCouponCode, sUserIP, bDummy.ToString().ToLower()), GetLogFilename());


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
                    string sIP = "1.1.1.1";
                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("users_ws");
                    if (sWSURL.Length > 0)
                        u.Url = sWSURL;

                    ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
                    if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "Cant charge an unknown user";
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
                                    sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, string.Empty, bIsEntitledToPreviewModule ? theSub.m_oPreviewModule.m_nID + "" : string.Empty);

                                Logger.Logger.Log("CustomData", string.Format("Subscription custom data created. Site Guid: {0} , User IP: {1} , Custom data: {2}", sSiteGUID, sUserIP, sCustomData), "CustomDataForSubsrpition");

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
                                    HandleChargeUserForSubscriptionBillingSuccess(sSiteGUID, theSub, dPrice, sCurrency, sCouponCode,
                                                                                  sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, ret, bIsEntitledToPreviewModule, sSubscriptionCode, sCustomData,
                                                                                  bIsRecurring, ref lBillingTransactionID, ref lPurchaseID);

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
                Logger.Logger.Log("Cellular_BaseChargeUserForSubscription", sb.ToString(), "BaseConditionalAccess");
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

        protected void UpdatePurchaseIDInExternalBillingTable(long lBillingTransactionID, long lPurchaseID)
        {
            int nExternalTransactionID = 0;
            int nBillingProvider = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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
            selectQuery.Finish();
            selectQuery = null;

            if (nExternalTransactionID > 0 && nBillingProvider > 0 && Enum.IsDefined(typeof(eBillingProvider), nBillingProvider))
            {
                eBillingProvider billingProvider = (eBillingProvider)nBillingProvider;
                string sTableName = string.Empty;

                switch (billingProvider)
                {
                    case eBillingProvider.Adyen:
                        sTableName = "adyen_transactions";
                        break;
                    case eBillingProvider.M1:
                        sTableName = "m1_transactions";
                        break;
                    case eBillingProvider.Cinepolis:
                        sTableName = "cinepolis_transactions";
                        break;
                    default:
                        Logger.Logger.Log("UpdatePurchaseIDInExternalBillingTable", string.Format("No table name assigned. Billing transaction ID: {0} , Purchase ID: {1} , BaseConditionalAccess is: {2} , Billing Provider: {3} , External transaction ID: {4}", lBillingTransactionID, lPurchaseID, this.GetType().Name, nBillingProvider, nExternalTransactionID), "BaseConditionalAccess");
                        break;
                }
                if (sTableName.Length > 0)
                {
                    ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                    directQuery.SetConnectionKey("BILLING_CONNECTION");
                    directQuery += "update  " + sTableName + " set  ";
                    directQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_id", "=", lPurchaseID);
                    directQuery += "where";
                    directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nExternalTransactionID);

                    directQuery.Execute();
                    directQuery.Finish();
                    directQuery = null;
                }
            }
            else
            {
                Logger.Logger.Log("UpdatePurchaseIDInExternalBillingTable", string.Format("Unexpected error. Billing transaction ID: {0} , Purchase ID: {1} , BaseConditionalAccess is: {2} , Billing Provider: {3} , External transaction ID: {4}", lBillingTransactionID, lPurchaseID, this.GetType().Name, nBillingProvider, nExternalTransactionID), "BaseConditionalAccess");
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
                UserResponseObject ExistUser = Utils.GetExistUser(sSiteGuid, m_nGroupID);

                if (ExistUser != null && ExistUser.m_RespStatus == ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    PermittedSubscriptionContainer[] userSubsArray = GetUserPermittedSubscriptions(sSiteGuid);//get all the valid subscriptions that this user has
                    Subscription userSubNew;
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
                                Logger.Logger.Log("ChangeSubscription", "Previous Subscription ID: " + nOldSub + " is not renewable. Subscription was not changed", "BaseConditionalAccess");
                                return ChangeSubscriptionStatus.OldSubNotRenewable;
                            }
                        }
                    }

                    //check if new subscsription already exists for this user
                    List<PermittedSubscriptionContainer> userNewSubList = userSubsArray.Where(x => x.m_sSubscriptionCode == nNewSub.ToString()).ToList();
                    if (userNewSubList != null && userNewSubList.Count > 0 && userNewSubList[0] != null)
                    {
                        Logger.Logger.Log("ChangeSubscription", "New Subscription ID: " + nNewSub + " is already attached to this user. Subscription was not changed", "BaseConditionalAccess");
                        return ChangeSubscriptionStatus.UserHadNewSub;
                    }

                    userSubNew = Utils.GetSubscriptionData(nNewSub.ToString(), m_nGroupID);
                                        
                    //set new subscprion
                    if (userSubNew != null && userSubNew.m_SubscriptionCode != null)
                    {
                        if (!userSubNew.m_bIsRecurring)
                        {
                            Logger.Logger.Log("ChangeSubscription", "New Subscription ID: " + nNewSub + " is not renewable. Subscription was not changed", "BaseConditionalAccess");
                            return ChangeSubscriptionStatus.NewSubNotRenewable;
                        }
                        
                        return setSubscriptionChange(sSiteGuid, userSubNew, userSubOld);
                    }
                    else
                    {
                        Logger.Logger.Log("ChangeSubscription", "New Subscription ID: " + nNewSub + " was not found. Subscription was not changed", "BaseConditionalAccess");
                        return ChangeSubscriptionStatus.NewSubNotExits;
                    }
                }
                else
                {
                    Logger.Logger.Log("ChangeSubscription", " User with siteGuid: " + sSiteGuid + " does not exist. Subscription was not changed", "BaseConditionalAccess");
                    return ChangeSubscriptionStatus.UserNotExists;
                }
            }
            catch (Exception exc)
            {
                Logger.Logger.Log("ChangeSubscription", "Exception: " + exc.Message + "In: " + exc.StackTrace, "BaseConditionalAccess");
                return ChangeSubscriptionStatus.Error;
            }
        }

        //the new subscription is dummy charged and its end date is set according the previous subscriptions end date
        //the previous  subscription is cancled and its end date is set to 'now'
        private ChangeSubscriptionStatus setSubscriptionChange(string sSiteGuid, Subscription subNew, PermittedSubscriptionContainer userSubOld)
        {
            ChangeSubscriptionStatus status = ChangeSubscriptionStatus.Error;
            try
            {               
                #region Initialize
                string sCurrency = "";
                double dPrice = 0;
                string sCouponCode = "";
                string sUserIP = "";
                string sCountry = "";
                string sLanguage = "";
                string sDeviceName = "";
                string sSubscriptionCode = subNew.m_SubscriptionCode;
                bool isDummyCharge = true;
                string extraParams = "";
                string sBillingMethod = "";//used only in real billing and not dummy
                string sEncryptedCVV = "";//used only in real billing and not dummy                  

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

                string sCouponCodeOld = "";
                #endregion

                //charge the user for the new subscription with dummy charge
                TvinciBilling.BillingResponse billResp = CC_BaseChargeUserForBundle(sSiteGuid, dPrice, sCurrency, sSubscriptionCode, sCouponCode, sUserIP, extraParams, sCountry, sLanguage, sDeviceName,
                    isDummyCharge, sBillingMethod, sEncryptedCVV, eBundleType.SUBSCRIPTION);

                //check if the charge was succesful: if so update relevant parameters and cancel the subsciption  
                if (billResp.m_oStatus == TvinciBilling.BillingResponseStatus.Success)
                {
                    //get the end_date of previous subsciption - with considuration of a free trial, if one was assigned to it 
                    PriceReason reason = new PriceReason();
                    Subscription subOld = new Subscription();
                    //the 'sCouponCodeOld' is empty, so the 'price' itself does not include a discount, if one was given
                    Price price = Utils.GetSubscriptionFinalPrice(m_nGroupID, userSubOld.m_sSubscriptionCode, sSiteGuid, sCouponCodeOld, ref reason, ref subOld, sCountry, sLanguage, userSubOld.m_sDeviceName);
                    bool bIsEntitledToPreviewModule = false;
                    if (reason == PriceReason.EntitledToPreviewModule)
                        bIsEntitledToPreviewModule = true;
                    DateTime dtSubEndDate = CalcSubscriptionEndDate(subOld, bIsEntitledToPreviewModule, DateTime.UtcNow);

                    int nBillingTransID = 0;
                    bool parseSucceeded = int.TryParse(billResp.m_sRecieptCode, out nBillingTransID);
                    if (parseSucceeded)
                    {
                        //update the new subscription End Date and Billing Method                                                                
                        bool updateEndDateNew = ConditionalAccessDAL.Update_SubscriptionPurchaseEndDate(null, sSiteGuid, nBillingTransID, dtSubEndDate);
                        int nBillingMethod = (int)PaymentMethod.ChangeSubscription;
                        bool updateBillingTrans = ConditionalAccessDAL.Update_BillingMethodInBillingTransactions(nBillingTransID, nBillingMethod);

                        //update the old subscription : is_recurring_status = 0, end_date = 'now'               
                        bool bCancel = DAL.ConditionalAccessDAL.CancelSubscription(userSubOld.m_nSubscriptionPurchaseID, m_nGroupID, sSiteGuid, userSubOld.m_sSubscriptionCode) != 0 ? false : true;
                        bool updateEndDateOld = ConditionalAccessDAL.Update_SubscriptionPurchaseEndDate(userSubOld.m_nSubscriptionPurchaseID, sSiteGuid, null, DateTime.UtcNow);

                        if (updateEndDateNew && updateBillingTrans && bCancel && updateEndDateOld)
                        {
                            status = ChangeSubscriptionStatus.OK;
                        }
                        else
                        {
                            Logger.Logger.Log("setSubscriptionChange", "Update of new subscription: " + sSubscriptionCode + "and previous subcsription:" + userSubOld.m_sSubscriptionCode + "for User: " + sSiteGuid + " failed.", "BaseConditionalAccess");
                        }
                    }
                }
                else
                {
                    Logger.Logger.Log("setSubscriptionChange", "User with siteGuid: " + sSiteGuid + " was not dummy charged for new Subscription: " + sSubscriptionCode + ". Subscription was not changed", "BaseConditionalAccess");
                }
                                
            }
            catch (Exception exc)
            {
                Logger.Logger.Log("setSubscriptionChange", "Exception: " + exc.Message + "In: " + exc.StackTrace, "BaseConditionalAccess");
                return ChangeSubscriptionStatus.Error;
            }
            return status;
        }


        /* This method shall set the cancellation Date column in the user entitlement table (subscriptions/ppv/collection_purchases) to the current date 
         * and set the is_active state to 0. 
         * The method shall perform a call to the client specific billing gateway to perform a cancellation action on the external billing gateway*/
        public virtual bool CancelTransaction(string sSiteGuid, int nAssetID, eTransactionType transactionType)
        {
            bool bRes = false;
            try
            {                
                switch (transactionType)
                {
                    case eTransactionType.PPV:
                        bRes = DAL.ConditionalAccessDAL.CancelPPVPurchaseTransaction(sSiteGuid, nAssetID);
                        break;
                    case eTransactionType.Subscription:
                        bRes = DAL.ConditionalAccessDAL.CancelSubscriptionPurchaseTransaction(sSiteGuid, nAssetID);
                        break;
                    case eTransactionType.Collection:
                        bRes = DAL.ConditionalAccessDAL.CancelCollectionPurchaseTransaction(sSiteGuid, nAssetID);
                        break;
                    default:
                        return false;                       
                }
                if (bRes)
                {
                    //call billing to the client specific billing gateway to perform a cancellation action on the external billing gateway
                    // call ? for REFUND?????


                }

                return bRes;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /*This method shall set the waiver flag on the user entitlement table (susbcriptions/ppv/collection_purchases) 
         * and the waiver_date field to the current date.*/
        public virtual bool WaiverTransaction(string sSiteGuid, int nAssetID,  eTransactionType transactionType)
        {
            bool bRes = false;
            try
            {
                switch (transactionType)
                {
                    case eTransactionType.PPV:
                        bRes = DAL.ConditionalAccessDAL.WaiverPPVPurchaseTransaction(sSiteGuid, nAssetID);
                        break;
                    case eTransactionType.Subscription:
                        DAL.ConditionalAccessDAL.WaiverSubscriptionPurchaseTransaction(sSiteGuid, nAssetID);
                        break;
                    case eTransactionType.Collection:
                        DAL.ConditionalAccessDAL.WaiverCollectionPurchaseTransaction(sSiteGuid, nAssetID);
                        break;
                    default:
                        return false;
                }
                return bRes;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

}
