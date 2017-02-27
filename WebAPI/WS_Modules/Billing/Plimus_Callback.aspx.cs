using ApiObjects.Billing;
using Core.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WS_Users;

namespace WS_Billing
{
    public partial class Plimus_Callback : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            #region intial callback param
            string productId = GetSafeValue("productId");
            string productName = GetSafeValue("productName");
            string contractId = GetSafeValue("contractId");
            string contractName = GetSafeValue("contractName");
            string referenceNumber = GetSafeValue("referenceNumber");
            string transactionType = GetSafeValue("transactionType");

            string transactionDate = GetSafeValue("transactionDate");
            string paymentMethod = GetSafeValue("paymentMethod");
            string paymentType = GetSafeValue("paymentType");
            string creditCardType = GetSafeValue("creditCardType");
            string remoteAddress = GetSafeValue("remoteAddress");
            string contractOwner = GetSafeValue("contractOwner");
            string creditCardLastFourDigits = GetSafeValue("creditCardLastFourDigits");
            string creditCardExpDate = GetSafeValue("creditCardExpDate");
            string accountId = GetSafeValue("accountId");
            string firstName = GetSafeValue("firstName");
            string lastName = GetSafeValue("lastName");
            string custom1 = GetSafeValue("custom1");
            string InvoiceStatus = GetSafeValue("InvoiceStatus");
            string PlimusSubscriptionID = GetSafeValue("SubscriptionID");
            #endregion

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
            long lBillingTransactionID = 0;
            int nPlimusBillingTransactionID = 0;
            #endregion

            int nBillingProvider = (int)eBillingProvider.Adyen;
            int nBillingMethod = GetBillingMethods(paymentMethod);
            int nproccessor = 2;
            bool purchaseSuccess = false;

            switch (transactionType)
            {

                //orders that were authorized for a future charge.  
                case "AUTH_ONLY ":
                    //TODO: 
                    break;


                //orders that were cancelled (for unapproved orders and cancelled subscriptions). 
                case "CANCELLATION":
                    //TODO: 
                    break;


                //orders that were refunded and cancelled (for cancelled subscriptions). 
                case "CANCELLATION_REFUND":
                    //TODO: 
                    break;


                //orders that were successfully charged. 
                case "CHARGE":



                  

              
                  

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
                                int groupID = GetGroupID(sSiteGUID);
                                int plimusID = 0;

                                lBillingTransactionID = Utils.InsertNewPlimusTransaction(groupID, sSiteGUID, sDigits, double.Parse(price), currencyCode, custom1, sCustomData, productId, productName, contractId, contractName, referenceNumber, transactionType, transactionDate, paymentMethod, paymentType, creditCardType, remoteAddress, contractOwner, creditCardLastFourDigits, creditCardExpDate, accountId, firstName, lastName, sCustomData, ref plimusID, true, string.Empty, string.Empty, 1, 1, nproccessor, nBillingMethod, nBillingProvider, 1);
                                UpdatePlimustransactionsPurchaseID(groupID, referenceNumber);
                            }

                        
                    }
                   
                    break;
                
                case "RECURRING" :

                     

                

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
                                int groupID = GetGroupID(sSiteGUID);
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
                                Int32 nStatus = 1;
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

                                nPlimusBillingTransactionID = Utils.InsertNewPlimusTransaction(groupID, sSiteGUID, sDigits, double.Parse(price), currencyCode, custom1, sCustomData, productId, productName, contractId, contractName, referenceNumber, transactionType, transactionDate, paymentMethod, paymentType, creditCardType, remoteAddress, contractOwner, creditCardLastFourDigits, creditCardExpDate, accountId, firstName, lastName, sCustomData, ref plimusID, true, string.Empty, string.Empty, 1, 1, nproccessor, nBillingMethod, nBillingProvider, 1);




                                if (nPlimusBillingTransactionID > 0)
                                {



                                    Utils.SplitRefference(sCustomData, ref nMediaFileID, ref nMediaID, ref sSubscriptionCode, ref sPPVCode, ref sPrePaidCode, ref sPriceCode,
                                            ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPayments, ref sUserGUID,
                                                        ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs, ref sPurchaseType,
                                                        ref sCountryCd, ref sLanguageCode, ref sDeviceName);




                                    lBillingTransactionID = Utils.InsertBillingTransaction(sSiteGUID, sDigits, dChargePrice, sPriceCode,
                                            sCurrencyCode, sCustomData, nStatus, "", bIsRecurring, nMediaFileID, nMediaID, sPPVModuleCode,
                                            sSubscriptionCode, "", groupID, nBillingProvider, int.Parse(referenceNumber), 0.0, dChargePrice, nPaymentNum, nNumberOfPayments, "",
                                            sCountryCd, sLanguageCode, sDeviceName, nproccessor, nBillingMethod, sPrePaidCode);

                                    purchaseSuccess = RenewSubscrptionTransaction(groupID, sSubscriptionID, sSiteGUID, paymentMethod, price, scurrency, sCustomData, 
                                        sCountryCode, sLangCode, sDevice, smnou, sviewlifecyclesecs, isRecurringStr, smaxusagemodulelifecycle, lBillingTransactionID, plimusID);
                                }
                                //UpdatePlimustransactionsPurchaseID(groupID, referenceNumber);
                            }
                    }
                    break;


                //orders that were refunded and cancelled (for cancelled subscriptions). 
                case "CHARGEBACK":
                    //TODO
                    break;


                //Recurring & Contract Change : contracts that were switched with another contract.
                case "CONTRACT_CHANGE":
                    //TODO
                    break;


                //orders that were refunded.
                case "REFUND":
                    //TODO:
                    break;


                //when user password has changed on vendor's site. 
                case "PASSWORD_CHANGE":
                    //TODO
                    break;

            }
            }

        /// <summary>
        /// Handle Subscrption Transaction
        /// </summary>
        protected bool RenewSubscrptionTransaction(int groupID, string sSubscriptionID, string sSiteGUID, 
            string paymentMethod, string price, string scurrency, string sCustomData, string sCountryCode, string sLangCode, string sDevice,
            string smnou, string sviewlifecyclesecs, string isRecurringStr, string smaxusagemodulelifecycle, long lBillingTransactionID, int plimusID)
        {


            try
            {
                WriteToUserLog(groupID, sSiteGUID, "Renew Plimus Subscription purchase (CC): " + sSubscriptionID);
            }
            catch { }

            bool retVal = false;
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
            updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
            
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
            DateTime d = Utils.GetEndDateTime(DateTime.UtcNow, int.Parse(smaxusagemodulelifecycle));
            if (smaxusagemodulelifecycle != "")
            {
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);
            }
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", lBillingTransactionID);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            updateQuery += " and ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionID);
            updateQuery += " and ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;


            int recurringInt = 0;

            if (!string.IsNullOrEmpty(isRecurringStr) && isRecurringStr.ToLower().Equals("true"))
            {
                recurringInt = 1;
            }
            Int32 nPurchaseID = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select id from subscriptions_purchases where ";
            selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", double.Parse(price));
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", scurrency);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
            selectQuery += " and ";
            if (smnou != "")
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", int.Parse(smnou));
            else
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);
            selectQuery += " and ";
            if (sviewlifecyclesecs != "")
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", int.Parse(sviewlifecyclesecs));
            else
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", 0);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", recurringInt);
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


            //Should update the PURCHASE_ID

            if (lBillingTransactionID != 0 && nPurchaseID != 0)
            {
                UpdateBillingTransactionsPurchaseID(plimusID, nPurchaseID);
                UpdatePlimusTransactionsPurchaseID(plimusID, nPurchaseID);

                retVal = true;
            }
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
        /// Update Plimus PPV Purchase ID
        /// </summary>
        /// <param name="plimusID"></param>
        /// <param name="ppvID"></param>
        protected void UpdatePlimustransactionsPurchaseID(int nGroupID, string nReferenceNumber)
        {
            int purchase_id = GetPurchaseID(nGroupID, nReferenceNumber);
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("plimus_transactions");
            updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_id", "=", purchase_id);
            updateQuery += "where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("REFERENCE_NUMBER", "=", nReferenceNumber);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

        }
        protected int GetPurchaseID(int nGroupID, string nBillingTransactionID)
        {
            int nPurchaseID = -1;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select PURCHASE_ID from billing_transactions where ";
            selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROVIDER_REFFERENCE", "=", nBillingTransactionID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nPurchaseID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
            }

            selectQuery.Finish();
            selectQuery = null;
            return nPurchaseID;
        }
        protected Int32 GetGroupID(string sSiteGuid)
        {
            //string sIP = "1.1.1.1";
            //string sWSUserName = "";
            //string sWSPass = "";
            //int res = TVinciShared.WS_Utils.GetGroupID("billing", "plimus_api", sWSUserName, sWSPass, sIP);
            //return res;
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
        /// <param name="sQueryKey"></param>
        /// <param name="theRoot"></param>
        /// <returns></returns>
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
        /// <param name="sQueryKey"></param>
        /// <param name="sParName"></param>
        /// <param name="theRoot"></param>
        /// <returns></returns>
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
        /// Write To User Log
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="sSiteGUID"></param>
        /// <param name="sMessage"></param>
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
      
    }
}