using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DAL;
using WS_Users;
using Core.Billing;
using ApiObjects.Billing;

namespace WS_Billing
{
    public partial class plimus_api : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            #region intial callback param
            string referenceNumber = GetSafeValue("referenceNumber");
            string accountId = GetSafeValue("accountId");
            string firstName = GetSafeValue("firstName");
            string lastName = GetSafeValue("lastName");
            string custom1 = GetSafeValue("custom1");
            string InvoiceStatus = GetSafeValue("InvoiceStatus");
            string PlimusSubscriptionID = GetSafeValue("SubscriptionID");
            string paymentMethod = "";
            #endregion


            int nBillingProvider = (int)eBillingProvider.Adyen;
            int nBillingMethod = 1;// GetBillingMethods(paymentMethod);
            int nproccessor = 2;
            bool purchaseSuccess = false;

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
            long nBillingTransactionID = 0;
            #endregion

            if (InvoiceStatus == "Approved")
            {

                if (!string.IsNullOrEmpty(custom1))
                {
                    //The custom data is created by calling the AD_GetCustomDataID function in the CA/ 
                    string sCustomData = Utils.GetCustomData(int.Parse(custom1));
                    if (sCustomData != "")
                    {
                        #region Parse custom data xml
                        //Parse the custom data xml
                        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                        doc.LoadXml(sCustomData);
                        System.Xml.XmlNode theRequest = doc.FirstChild;

                        sType = GetSafeParValue(".", "type", ref theRequest);
                        sSiteGUID = GetSafeParValue("//u", "id", ref theRequest);
                        string email = string.Empty;
                        int domainId = 0;
                        int groupID = GetGroupID(sSiteGUID);
                        if (!Utils.IsUserExist(sSiteGUID, groupID, ref email, ref domainId))
                        {
                            RedirectPage(accountId, "Error", "Item purchase error", false, true, "user does not exist");
                        }
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



                        //TODO : is Fraud Check

                        //Get Group ID by skin code                        
                        int plimusID = int.Parse(referenceNumber);

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

                        //if (transactionType.Trim().ToLower().StartsWith("pending") == true || transactionType.Trim().ToLower().StartsWith("authorised") == true || transactionType.Trim().ToLower().StartsWith("authorized") == true || transactionType.Trim().ToLower().StartsWith("success") == true || transactionType.Trim().ToLower().StartsWith("renewal"))
                        //    nStatus = 0;
                        //else
                        //    nStatus = 1;
                        Utils.SplitRefference(sCustomData, ref nMediaFileID, ref nMediaID, ref sSubscriptionCode, ref sPPVCode, ref sPrePaidCode, ref sPriceCode,
                                ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPayments, ref sUserGUID,
                                            ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs, ref sPurchaseType,
                                            ref sCountryCd, ref sLanguageCode, ref sDeviceName);




                        nBillingTransactionID = Utils.InsertBillingTransaction(sSiteGUID, sDigits, dChargePrice, sPriceCode,
                                sCurrencyCode, sCustomData, nStatus, "", bIsRecurring, nMediaFileID, nMediaID, sPPVModuleCode,
                                sSubscriptionCode, "", groupID, nBillingProvider, int.Parse(referenceNumber), 0.0, dChargePrice, nPaymentNum, nNumberOfPayments, "",
                                sCountryCd, sLanguageCode, sDeviceName, nproccessor, nBillingMethod, sPrePaidCode);

                        //nBillingTransactionID = Billing.Utils.InsertNewPlimusTransaction(groupID, sSiteGUID, sDigits, double.Parse(price), currencyCode, custom1, sCustomData, productId, productName, contractId, contractName, referenceNumber, transactionType, transactionDate, paymentMethod, paymentType, creditCardType, remoteAddress, contractOwner, creditCardLastFourDigits, creditCardExpDate, accountId, firstName, lastName, sCustomData, ref plimusID, true, string.Empty, string.Empty, 1, 1, nproccessor, nBillingMethod, nBillingProvider, 1);
                        if (nBillingTransactionID > 0)
                        {
                            if (!string.IsNullOrEmpty(sCampCode))
                            {
                                int nCampCode = int.Parse(sCampCode);
                                if (nCampCode > 0)
                                {
                                    HandleCampaignUse(nCampCode, sSiteGUID, int.Parse(sCampMNOU), sCampLS);
                                }
                            }

                            switch (sType)
                            {
                                case "pp":
                                    #region Handle PPV Transaction
                                    purchaseSuccess = HandlePPVTransaction(groupID, srelevantsub, smedia_file, sSiteGUID, paymentMethod, price, scurrency, sCustomData, sCountryCode, sLangCode, sDevice,
                                                                            smnou, nBillingTransactionID, smaxusagemodulelifecycle, plimusID, domainId);
                                    if (!string.IsNullOrEmpty(scouponcode))
                                    {
                                        HandleCouponUse(scouponcode, sSiteGUID, int.Parse(smedia_file), srelevantsub, groupID);
                                    }
                                    #endregion
                                    break;
                                case "sp":
                                    #region Subscription Purchase
                                    purchaseSuccess = HandleSubscrptionTransaction(groupID, sSubscriptionID, sSiteGUID, paymentMethod, price, scurrency, sCustomData, sCountryCode, sLangCode, sDevice,
                                                                                    smnou, sviewlifecyclesecs, isRecurringStr, smaxusagemodulelifecycle, nBillingTransactionID, plimusID, domainId);
                                    if (!string.IsNullOrEmpty(scouponcode))
                                    {
                                        HandleCouponUse(scouponcode, sSiteGUID, 0, sSubscriptionID, groupID);
                                    }
                                    #endregion
                                    break;
                                case "prepaid":
                                    #region Handle PrePaid Transaction
                                    purchaseSuccess = HandlePrePaidTransaction(groupID, sPrePaidID, sSiteGUID, paymentMethod, price, sPPCreditValue, scurrency, sCustomData, sCountryCode, sLangCode, sDevice, smnou, smaxusagemodulelifecycle, nBillingTransactionID, plimusID);
                                    #endregion
                                    break;
                            }
                            if (purchaseSuccess)
                            {
                                RedirectPage(accountId, "OK", "OK", false);
                            }
                            else
                            {
                                RedirectPage(accountId, "Error", "Item purchase error", false, true, "Item already purchased");
                            }
                        }
                        else
                        {
                            RedirectPage(accountId, "Error", "Item already purchased", false, true, "Item already purchased");
                        }
                    }
                    else
                    {
                        RedirectPage(accountId, "Error", "Invalid error", false, true, "Custom data is null");


                    }
                }

            }
            else
            {
                RedirectPage(accountId, "Error", "Invalid Invoice Status", false, true, string.Format("invice status '{0}' error", InvoiceStatus));
            }



        }
        protected Int32 GetGroupID(string sSiteGuid)
        {
          //TO DO 
            //-------------------------------
            
            // Crearte table and get the group ID by contract plimus ID
            //-------------------------------
            return 144;
            
        }
        /// <summary>
        /// Get Safe Value
        /// </summary>
        /// <param name="sQueryKey"></param>
        /// <returns></returns>
        protected string GetSafeValue(string sQueryKey)
        {
            string[] keyValue = this.Page.Request.Params.GetValues(sQueryKey);
            if (keyValue != null && keyValue.Length > 0)
                return keyValue[0];
            else
                return string.Empty;

        }
        /// <summary>
        /// Get Billing Methods Number
        /// </summary>
        /// <param name="sPaymentMethod"></param>
        /// <returns></returns>
        protected int GetBillingMethods(string sPaymentMethod)
        {
            int res = 6;
            switch (sPaymentMethod)
            {
                case "CC":
                    res = 1;
                    break;


            }

            return res;


        }
        /// <summary>
        /// GetSafeValue
        /// </summary>
        protected string GetSafeValue(string sQueryKey, ref System.Xml.XmlNode theRoot)
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
        /// <summary>
        /// Get Safe Par Value
        /// </summary>
        protected string GetSafeParValue(string sQueryKey, string sParName, ref System.Xml.XmlNode theRoot)
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
        /// <summary>
        /// Handle Coupon Use
        /// </summary>
        protected void HandleCouponUse(string sCouponCode, string sSiteGUID, int nMediaFileID, string sSubCode, int nGroupID)
        {
            int couponID = 0;
            ODBCWrapper.DataSetSelectQuery couponSelectQuery = new ODBCWrapper.DataSetSelectQuery();
            couponSelectQuery.SetConnectionKey("pricing_connection");
            couponSelectQuery += "select id from coupons where ";
            couponSelectQuery += ODBCWrapper.Parameter.NEW_PARAM("code", "=", sCouponCode);
            couponSelectQuery += "and";
            couponSelectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(nGroupID, "MAIN_CONNECTION_STRING");
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            couponSelectQuery += " order by status desc,is_active desc";
            if (couponSelectQuery.Execute("query", true) != null)
            {
                Int32 nCount = couponSelectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    couponID = int.Parse(couponSelectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
            }
            couponSelectQuery.Finish();
            couponSelectQuery = null;

            if (couponID > 0)
            {
                int nSubCode = 0;
                if (!string.IsNullOrEmpty(sSubCode))
                {
                    nSubCode = int.Parse(sSubCode);
                }
                ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                directQuery.SetConnectionKey("pricing_connection");
                directQuery += "update coupons set USE_COUNT=USE_COUNT+1, LAST_USED_DATE=getdate() where ";
                directQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", couponID);
                directQuery.Execute();
                directQuery.Finish();
                directQuery = null;

                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("coupon_uses");
                insertQuery.SetConnectionKey("pricing_connection");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUPON_ID", "=", couponID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", nSubCode);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
        }
        /// <summary>
        /// Handle Campaign Use
        /// </summary>
        protected void HandleCampaignUse(int campaignID, string siteGuid, int maxNumOfUses, string maxLifeCycle)
        {
            ODBCWrapper.DataSetInsertQuery insertQuery = new ODBCWrapper.DataSetInsertQuery("campaigns_uses");
            insertQuery.SetConnectionKey("ca_connection_string");
            DateTime d = Utils.GetEndDateTime(DateTime.UtcNow, int.Parse(maxLifeCycle));
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("campaign_id", "=", campaignID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("site_guid", "=", int.Parse(siteGuid));
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("num_of_uses", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("max_num_of_uses", "=", maxNumOfUses);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", "=", d);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
        }
        /// <summary>
        /// Handle PPV Transaction
        /// </summary>
        protected bool HandlePPVTransaction(int groupID, string srelevantsub, string smedia_file, string sSiteGUID, string paymentMethod, string price, string scurrency, string sCustomData, string sCountryCode,
                string sLangCode, string sDevice, string smnou, long lBillingTransactionID, string smaxusagemodulelifecycle, int plimusID, int domainId)
        {
            bool retVal = false;
            DateTime endDate = DateTime.UtcNow;
            int maxUsageModuleLifecycle = 0;
            if (!string.IsNullOrEmpty(smaxusagemodulelifecycle) && int.TryParse(smaxusagemodulelifecycle, out maxUsageModuleLifecycle))
            {                
                endDate = Utils.GetEndDateTime(DateTime.UtcNow, maxUsageModuleLifecycle);
            }

            long purchaseId = ConditionalAccessDAL.Insert_NewPPVPurchase(groupID, long.Parse(smedia_file), sSiteGUID, double.Parse(price), scurrency, !string.IsNullOrEmpty(smnou) ? long.Parse(smnou) : 0,
                                                                         sCustomData, !string.IsNullOrEmpty(srelevantsub) ? srelevantsub : string.Empty, lBillingTransactionID, DateTime.UtcNow, endDate,
                                                                         DateTime.UtcNow, sCountryCode, sLangCode, sDevice, domainId, null);
           
            //Should update the PURCHASE_ID

            if (lBillingTransactionID != 0 && purchaseId != 0)
            {
                UpdateBillingTransactionsPurchaseID(plimusID, (int)purchaseId);
                UpdatePlimusTransactionsPurchaseID(plimusID, (int)purchaseId);
                retVal = true;

            }

            //string sItemName = "";
            //ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery1.SetConnectionKey("MAIN_CONNECTION_STRING");
            //selectQuery1 += "select name from media m, media_files mf where mf.media_id=m.id and ";
            //selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", int.Parse(smedia_file));
            //if (selectQuery1.Execute("query", true) != null)
            //{
            //    Int32 nCount = selectQuery1.Table("query").DefaultView.Count;
            //    if (nCount > 0)
            //    {
            //        sItemName = selectQuery1.Table("query").DefaultView[0].Row["NAME"].ToString();
            //    }
            //}
            //selectQuery1.Finish();
            //selectQuery1 = null;
            //Billing.Utils.SendPurchaseMail(paymentMethod, sItemName, sSiteGUID, nBillingTransactionID,
            //   price, scurrency, groupID);

            return retVal;
        }
        /// <summary>
        /// Handle Subscrption Transaction
        /// </summary>
        protected bool HandleSubscrptionTransaction(int groupID, string sSubscriptionID, string sSiteGUID, string paymentMethod, string price, string scurrency, string sCustomData, string sCountryCode, string sLangCode, string sDevice,
            string smnou, string sviewlifecyclesecs, string isRecurringStr, string smaxusagemodulelifecycle, long lBillingTransactionID, int plimusID, int domainId)
        {

            try
            {
                WriteToUserLog(groupID, sSiteGUID, "Subscription purchase (CC): " + sSubscriptionID);
            }
            catch { }

            bool retVal = false;
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
            updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            updateQuery += " and ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionID);
            updateQuery += " and ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

            DateTime endDate = DateTime.UtcNow;
            int maxUsageModuleLifecycle = 0;
            if (!string.IsNullOrEmpty(smaxusagemodulelifecycle) && int.TryParse(smaxusagemodulelifecycle, out maxUsageModuleLifecycle))
            {                
                endDate = Utils.GetEndDateTime(DateTime.UtcNow, maxUsageModuleLifecycle);
            }
            
            long purchaseId = ConditionalAccessDAL.Insert_NewMPPPurchase(groupID, sSubscriptionID, sSiteGUID, double.Parse(price), scurrency, sCustomData, sCountryCode, sLangCode, sDevice,
                                                                            !string.IsNullOrEmpty(smnou) ? int.Parse(smnou) : 0, !string.IsNullOrEmpty(sviewlifecyclesecs) ? int.Parse(sviewlifecyclesecs) : 0,
                                                                            !string.IsNullOrEmpty(isRecurringStr) ? bool.Parse(isRecurringStr) : false, lBillingTransactionID, 0, DateTime.UtcNow,
                                                                            endDate, DateTime.UtcNow, "CA_CONNECTION_STRING", domainId);

            //Should update the PURCHASE_ID

            if (lBillingTransactionID != 0 && purchaseId != 0)
            {
                UpdateBillingTransactionsPurchaseID(plimusID, (int)purchaseId);
                UpdatePlimusTransactionsPurchaseID(plimusID, (int)purchaseId);

                retVal = true;
            }
            return retVal;
        }

        /// <summary>
        /// Handle PrePaid Transaction
        /// </summary>
        protected bool HandlePrePaidTransaction(int groupID, string sPrePaidID, string sSiteGUID, string paymentMethod, string price, string sPPCreditValue, string scurrency, string sCustomData, string sCountryCode, string sLangCode, string sDevice,
            string smnou, string smaxusagemodulelifecycle, long lBillingTransactionID, int plimusID)
        {

            bool retVal = false;
            double userPPVal = GetUserPrePaidAmount(sSiteGUID, scurrency);


            //Subscription Purchase
            try { WriteToUserLog(groupID, sSiteGUID, "Pre Paid purchase (CC): " + sPrePaidID); }
            catch { }


            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("pre_paid_purchases");
            insertQuery.SetConnectionKey("CA_CONNECTION_STRING");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);

            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("pre_paid_module_id", "=", int.Parse(sPrePaidID));
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", double.Parse(price));
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", scurrency);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("total_amount", "=", double.Parse(sPPCreditValue));
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("amount_used", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOM_DATA", "=", sCustomData);
            //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", int.Parse(ret.m_sRecieptCode));
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCode);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLangCode);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDevice);
            if (smnou != "")
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", int.Parse(smnou));
            else
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);

            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", lBillingTransactionID);
            DateTime d = DateTime.MaxValue;
            if (!string.IsNullOrEmpty(smaxusagemodulelifecycle))
            {
                d = Utils.GetEndDateTime(DateTime.UtcNow, int.Parse(smaxusagemodulelifecycle));
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);
                //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", GetCurrentDBTime().AddSeconds(thePPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle));
            }

            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
            //try { WriteToUserLog(sSiteGUID, "Pre Paid Module ID: " + sPrePaidModuleCode + " Purchased(CC): " + dPrice.ToString() + sCurrency); }
            //catch { }
            Int32 nPurchaseID = 0;


            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select id from pre_paid_purchases where ";
            selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pre_paid_module_id", "=", int.Parse(sPrePaidID));
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", double.Parse(price));
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", scurrency);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("total_amount", "=", double.Parse(sPPCreditValue));
            selectQuery += " and ";
            if (smnou != "")
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", int.Parse(smnou));
            else
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);

            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);

            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            if (smaxusagemodulelifecycle != "")
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);
            }
            selectQuery += "order by id desc";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nPurchaseID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
            }

            selectQuery.Finish();
            selectQuery = null;



            ODBCWrapper.InsertQuery ppInsertQuery = new ODBCWrapper.InsertQuery("pre_paid_uses");
            ppInsertQuery.SetConnectionKey("CA_CONNECTION_STRING");
            ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);

            ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("ITEM_ID", "=", int.Parse(sPrePaidID));
            ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("ITEM_TYPE", "=", 3);

            ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", double.Parse(sPPCreditValue));
            ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", scurrency);
            ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCode);
            ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLangCode);
            ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", "=", sDevice);

            ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
            ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("PP_CD", "=", int.Parse(sPrePaidID));
            ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("PP_PURCHASE_ID", "=", nPurchaseID);
            ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("REMAINS_CREDIT", "=", userPPVal + double.Parse(sPPCreditValue));

            ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
            ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);

            ppInsertQuery.Execute();
            ppInsertQuery.Finish();
            ppInsertQuery = null;
            //Should update the PURCHASE_ID

            if (lBillingTransactionID != 0 && nPurchaseID != 0)
            {
                UpdatePlimusTransactionsPurchaseID(plimusID, nPurchaseID);
                UpdateBillingTransactionsPurchaseID(plimusID, nPurchaseID);
                retVal = true;
            }
            return retVal;
        }
     
        /// <summary>
        /// Get SubName
        /// </summary>
        private string GetSubName(string subCode)
        {
            string retVal = string.Empty;
            if (!string.IsNullOrEmpty(subCode))
            {
                int subID = int.Parse(subCode);
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select NAME from subscriptions where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", subID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        retVal = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }

            return retVal;
        }
        /// <summary>
        /// Get User PrePaid Amount
        /// </summary>
        protected double GetUserPrePaidAmount(string sSiteGUID, string sCurrencyCode)
        {
            double retVal = 0.0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
            selectQuery += "select * from pre_paid_purchases where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("site_user_guid", "=", sSiteGUID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("currency_code", "=", sCurrencyCode);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", ">=", DateTime.Now);
            selectQuery += " and total_amount>amount_used order by end_date";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;

                for (int i = 0; i < nCount; i++)
                {

                    if (selectQuery.Table("query").DefaultView[i].Row["total_amount"] != DBNull.Value && selectQuery.Table("query").DefaultView[i].Row["total_amount"] != null && !string.IsNullOrEmpty(selectQuery.Table("query").DefaultView[i].Row["total_amount"].ToString()))
                        retVal += (double.Parse(selectQuery.Table("query").DefaultView[i].Row["total_amount"].ToString()) - double.Parse(selectQuery.Table("query").DefaultView[i].Row["amount_used"].ToString()));

                }
            }

            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }
         /// <summary>
        /// Update Plimus Purchase ID
        /// </summary>
        protected void UpdatePlimusTransactionsPurchaseID(int plimusID, int purchaseID)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("plimus_transactions");
            updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_id", "=", purchaseID);
            updateQuery += "where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("REFERENCE_NUMBER", "=", plimusID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

        }
        protected void UpdateBillingTransactionsPurchaseID(int plimusID, int purchaseID)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
            updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", purchaseID);
            updateQuery += "where";
            //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nBillingTransactionID);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROVIDER_REFFERENCE", "=", plimusID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }
        /// <summary>
        /// Write To User Log
        /// </summary>
        protected void WriteToUserLog(Int32 nGroupID, string sSiteGUID, string sMessage)
        {
            UsersService u = new UsersService();
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";
            TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "WriteLog", "users", sIP, ref sWSUserName, ref sWSPass);
            if (sWSUserName != "")
                u.WriteLog(sWSUserName, sWSPass, sSiteGUID, sMessage, "Conditional access module");
        }
        protected void RedirectPage(string accountId, string status, string description, bool endResponse)
        {
            RedirectPage(accountId, status, description, endResponse, false, "");
        }
        protected void RedirectPage(string accountId, string status, string description, bool endResponse, bool enableLog)
        {
            RedirectPage(accountId, status, description, endResponse, enableLog, "");
        }
        protected void RedirectPage(string accountId, string status, string description, bool endResponse, bool enableLog, string logMessage)
        {
            Response.Write(string.Format("status : {0} <br>", status));
            Response.Write(string.Format("Description {0}", description));

            //string sBaseRedirect = ODBCWrapper.Utils.GetTableSingleVal("Plimus_group_parameters", "BASE_REDIRECT_URL", "accountId", "=", accountId, 3600).ToString();
            //if (enableLog)
            //{

            //}

            //Response.Redirect(string.Format("{0}?=status={1}&desc={2}", sBaseRedirect, Server.UrlEncode(status), Server.UrlEncode(description)), endResponse);
            string redirectUrl = TVinciShared.WS_Utils.GetTcmConfigValue("REDIRECT_PILMUS_URL"); 
            Response.Redirect(string.Format("{0}?status={1}&desc={2}", redirectUrl, Server.UrlEncode(status), Server.UrlEncode(description)), endResponse);

        }
    }
}