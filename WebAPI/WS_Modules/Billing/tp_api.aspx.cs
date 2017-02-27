using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Text;
using KLogMonitor;
using System.Reflection;
using ApiObjects;
using Core.Billing;


namespace WS_Billing
{
    public partial class tp_api : System.Web.UI.Page
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected Int32 GetGroupIDPU(ref string sSecret)
        {
            Int32 nRet = 95;
            sSecret = ODBCWrapper.Utils.GetTableSingleVal("tikle_group_parameters", "CLIENT_SECRET", 1).ToString();
            return nRet;
        }

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

        protected string GetSafeValue(string sQueryKey)
        {
            if (String.IsNullOrEmpty(Request.QueryString[sQueryKey]))
                return "";
            return Request.QueryString[sQueryKey].ToString();
        }

        protected Int32 GetSMSTokenEntry(string sCellNum, Int32 nMEDIA_FILE_ID, string sPPVMODULE_CODE,
            string sSUBSCRIPTION_CODE, string sSITE_USER_GUID, string sPRICE_CODE, double dPRICE,
            string sCURRENCY_CODE, string sCUSTOM_DATA, Int32 nSMS_BILLING_PROVIDER,
            Int32 nGroupID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            selectQuery += "select * from sms_codes where is_active=0 and status=1 ";
            if (sCellNum != "")
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CELL_PHONE", "=", sCellNum);
            }
            if (nMEDIA_FILE_ID != 0)
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMEDIA_FILE_ID);
            }
            if (sPPVMODULE_CODE != "")
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PPVMODULE_CODE", "=", sPPVMODULE_CODE);
            }
            if (sSUBSCRIPTION_CODE != "")
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSUBSCRIPTION_CODE);
            }
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSITE_USER_GUID);
            if (sPRICE_CODE != "")
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE_CODE", "=", sPRICE_CODE);
            }
            if (nSMS_BILLING_PROVIDER != 0)
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SMS_BILLING_PROVIDER", "=", nSMS_BILLING_PROVIDER);
            }
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPRICE);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCURRENCY_CODE);
            selectQuery += "and";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOM_DATA", "=", sCUSTOM_DATA);
            //selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    dPRICE = double.Parse(selectQuery.Table("query").DefaultView[0].Row["PRICE"].ToString());
                    sCURRENCY_CODE = selectQuery.Table("query").DefaultView[0].Row["CURRENCY_CODE"].ToString();
                    //                    sCellPhone = selectQuery.Table("query").DefaultView[0].Row["CELL_PHONE"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        protected bool UpdateSMSCodeStatus(Int32 nID, Int32 nStatus)
        {
            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("sms_codes");
                updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nStatus);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Tikle request - UpdateSMSCodeStatus - failed updating sms code status. Error:" + ex.Message, ex);
            }

            return false;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string sQuery = Request.Url.Query;
            log.Debug("tp_api - " + sQuery);
            Response.Expires = -1;
            Int32 nGroupID = 0;
            try
            {
                string sToken = GetSafeValue("token");
                string sCustomData = GetSafeValue("CustomData");
                //customdata id from db
                string sCardNumber = GetSafeValue("CardNumber");
                string sMSISDN = GetSafeValue("msisdn");
                string sExpMonth = GetSafeValue("ExpMonth");
                string sExpYear = GetSafeValue("ExpYear");
                string smd5hash = GetSafeValue("md5hash");
                string ssender = GetSafeValue("sender");
                string spayment_num = GetSafeValue("payment_num");
                string snumber_of_payments = GetSafeValue("number_of_payments");
                Int32 nPaymentNumber = 0;
                if (spayment_num != "")
                    nPaymentNumber = int.Parse(spayment_num);
                Int32 nNumberOfPayments = 0;
                if (snumber_of_payments != "")
                    nNumberOfPayments = int.Parse(snumber_of_payments);

                string sSecret = "";
                nGroupID = GetGroupIDPU(ref sSecret);
                string sToHash = sCustomData + sSecret;
                string sHased = "";
                MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
                md5Provider = new MD5CryptoServiceProvider();
                byte[] originalBytes = UTF8Encoding.Default.GetBytes(sToHash);
                byte[] encodedBytes = md5Provider.ComputeHash(originalBytes);
                sHased = BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
                if (sHased == smd5hash)
                {
                    if (ssender == "cc_register")
                    {
                        string sSiteGUID = sCustomData;
                        string sToSave = "true";
                        Int32 nSiteGUID = 0;
                        try
                        {
                            nSiteGUID = int.Parse(sSiteGUID);
                        }
                        catch
                        {
                            nSiteGUID = 0;
                        }
                        if (nSiteGUID != 0)
                        {
                            if (nGroupID != 0)
                            {
                                //Update token
                                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_tokens");
                                insertQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TOKEN", "=", sToken);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_FOUR_DIGITS", "=", sCardNumber);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EXP_MONTH", "=", int.Parse(sExpMonth));
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EXP_YEAR", "=", int.Parse(sExpYear));

                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                                if (sToSave.Trim().ToLower() == "true")
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("cc_saved", "=", 1);
                                else
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("cc_saved", "=", 0);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", int.Parse(sSiteGUID));
                                insertQuery.Execute();
                                insertQuery.Finish();
                                insertQuery = null;
                                log.Debug("Success: " + sQuery);
                                Response.Write("OK");
                            }
                            else
                            {
                                log.Debug("Error: group id not recognized for: " + sQuery);
                                Response.Write("Fail");
                            }
                        }
                        else
                        {
                            log.Debug("Error: site guid is 0: " + sCustomData);
                            Response.Write("Fail");
                        }
                    }
                    else if (ssender.ToLower().StartsWith("sms") == true)
                    {

                        string sSubscriptionCode = "";
                        string sPPVCode = "";
                        Int32 nMediaFileID = 0;
                        Int32 nMediaID = 0;
                        string stotalAmount = "";
                        string sPriceCode = "";
                        double dChargePrice = 0.0;
                        string sCurrencyCode = "";
                        bool bIsRecurring = false;
                        string sPPVModuleCode = "";
                        Int32 nNumberOfPaymentsCD = 0;
                        string sRelevantSub = "";
                        string sUserGUID = "";
                        Int32 nMaxNumberOfUses = 0;
                        Int32 nMaxUsageModuleLifeCycle = 0;
                        Int32 nViewLifeCycleSecs = 0;
                        string sPurchaseType = "";

                        string sCountryCd = "";
                        string sLanguageCode = "";
                        string sDeviceName = "";
                        string sPPCode = string.Empty;
                        //Handle the custom data
                        if (sCustomData != "")
                        {
                            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                            doc.LoadXml(sCustomData);
                            System.Xml.XmlNode theRequest = doc.FirstChild;

                            Utils.SplitRefference(sCustomData, ref nMediaFileID, ref nMediaID, ref sSubscriptionCode, ref sPPVCode, ref sPPCode, ref sPriceCode,
                                ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPaymentsCD, ref sUserGUID,
                                ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs, ref sPurchaseType,
                                ref sCountryCd, ref sLanguageCode, ref sDeviceName);

                            stotalAmount = GetSafeValue("totalAmount");
                            if (stotalAmount == "")
                                stotalAmount = "0.0";
                            Int32 nEntry = GetSMSTokenEntry(sMSISDN, nMediaFileID, sPPVModuleCode, sSubscriptionCode, sUserGUID, sPriceCode, dChargePrice,
                                sCurrencyCode, sCustomData, 6, nGroupID);

                            long lBillingTransactionID = Utils.InsertBillingTransaction(sUserGUID, "", dChargePrice, sPriceCode,
                                sCurrencyCode, sCustomData, 0, "Code OK", bIsRecurring, nMediaFileID, nMediaID, sPPVModuleCode,
                                sSubscriptionCode, sMSISDN, nGroupID, 6, nEntry, 0.0, dChargePrice,
                                nPaymentNumber, nNumberOfPayments, "", sCountryCd, sLanguageCode, sDeviceName, 2, 2, string.Empty);

                            UpdateSMSCodeStatus(nEntry, 1);

                            if (ssender.ToUpper() == "SMS")
                            {
                                if (sPurchaseType == "pp")
                                {
                                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ppv_purchases");
                                    insertQuery.SetConnectionKey("CA_CONNECTION_STRING");
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                                    if (sRelevantSub != "")
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sRelevantSub);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sUserGUID);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dChargePrice);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrencyCode);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", nMaxNumberOfUses);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", sCustomData);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", lBillingTransactionID);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLanguageCode);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDeviceName);
                                    if (nMaxUsageModuleLifeCycle != 0)
                                    {
                                        DateTime d = Utils.GetEndDateTime(DateTime.UtcNow, nMaxUsageModuleLifeCycle);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);
                                    }

                                    insertQuery.Execute();
                                    insertQuery.Finish();
                                    insertQuery = null;

                                    Int32 nPurchaseID = 0;

                                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                    selectQuery += "select id from ppv_purchases where ";
                                    selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                                    if (sRelevantSub != "")
                                    {
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sRelevantSub);
                                    }
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sUserGUID);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dChargePrice);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrencyCode);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                    if (nMaxNumberOfUses != 0)
                                    {
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", nMaxNumberOfUses);
                                    }
                                    else
                                    {
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);
                                    }
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
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

                                    if (lBillingTransactionID != 0)
                                    {
                                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                                        updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                                        updateQuery += "where";
                                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", lBillingTransactionID);
                                        updateQuery.Execute();
                                        updateQuery.Finish();
                                        updateQuery = null;
                                        //Send purchase mail
                                        string sItemName = "";
                                        ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                                        selectQuery1.SetConnectionKey("MAIN_CONNECTION_STRING");
                                        selectQuery1 += "select name from media m, media_files mf where mf.media_id=m.id and ";
                                        selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", nMediaFileID);
                                        if (selectQuery1.Execute("query", true) != null)
                                        {
                                            Int32 nCount = selectQuery1.Table("query").DefaultView.Count;
                                            if (nCount > 0)
                                            {
                                                sItemName = selectQuery1.Table("query").DefaultView[0].Row["NAME"].ToString();
                                            }
                                        }
                                        selectQuery1.Finish();
                                        selectQuery1 = null;
                                        string sPaymentMethod = "SMS (" + sMSISDN + ")";
                                        Utils.SendMail(sPaymentMethod, sItemName, sUserGUID, lBillingTransactionID,
                                            stotalAmount, sCurrencyCode, string.Empty, nGroupID, string.Empty, eMailTemplateType.Purchase);
                                    }

                                    Response.Clear();
                                    Response.Write("OK");
                                }
                                else if (sPurchaseType == "sp")
                                {
                                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                                    updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                                    updateQuery += " where ";
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                                    updateQuery += " and ";
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                                    updateQuery += " and ";
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sUserGUID);
                                    updateQuery.Execute();
                                    updateQuery.Finish();
                                    updateQuery = null;

                                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscriptions_purchases");
                                    insertQuery.SetConnectionKey("CA_CONNECTION_STRING");
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sUserGUID);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dChargePrice);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrencyCode);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", nMaxNumberOfUses);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", nViewLifeCycleSecs);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", sCustomData);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLanguageCode);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDeviceName);
                                    if (nMaxUsageModuleLifeCycle != 0)
                                    {
                                        DateTime d = Utils.GetEndDateTime(DateTime.UtcNow, nMaxUsageModuleLifeCycle);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);
                                        //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", DateTime.UtcNow.AddSeconds(nMaxUsageModuleLifeCycle));
                                    }
                                    if (bIsRecurring == true)
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 1);
                                    else
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", lBillingTransactionID);
                                    insertQuery.Execute();
                                    insertQuery.Finish();
                                    insertQuery = null;

                                    Int32 nPurchaseID = 0;

                                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                    selectQuery += " select id from subscriptions_purchases where ";
                                    selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sUserGUID);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dChargePrice);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrencyCode);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", nMaxNumberOfUses);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", nMaxUsageModuleLifeCycle);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                    selectQuery += " and ";
                                    if (bIsRecurring == true)
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 1);
                                    else
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
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

                                    if (lBillingTransactionID != 0)
                                    {
                                        ODBCWrapper.UpdateQuery updateQuery1 = new ODBCWrapper.UpdateQuery("billing_transactions");
                                        updateQuery1.SetConnectionKey("MAIN_CONNECTION_STRING");
                                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                                        updateQuery1 += "where";
                                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", lBillingTransactionID);
                                        updateQuery1.Execute();
                                        updateQuery1.Finish();
                                        updateQuery1 = null;
                                        //Send purchase mail
                                        string sPaymentMethod = "SMS";
                                        sPaymentMethod += " (" + sMSISDN + ")";
                                        Utils.SendMail(sPaymentMethod, "Package: " + sSubscriptionCode, sUserGUID, lBillingTransactionID,
                                            stotalAmount, sCurrencyCode, string.Empty, nGroupID, string.Empty, eMailTemplateType.Purchase);
                                    }

                                    Response.Clear();
                                    Response.Write("OK");
                                }
                            }
                            else if (ssender.ToUpper().StartsWith("SMS_") == true)
                            {
                                if (sPurchaseType == "sp")
                                {
                                    Int32 nPurchaseID = 0;
                                    DateTime DDate = DateTime.UtcNow;
                                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                    selectQuery += "select id,end_date from subscriptions_purchases where ";
                                    selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sUserGUID);
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dChargePrice);
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrencyCode);
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", nMaxNumberOfUses);
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", nViewLifeCycleSecs);
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 1);
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                                    if (selectQuery.Execute("query", true) != null)
                                    {
                                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                                        if (nCount > 0)
                                        {
                                            DDate = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["end_date"]);
                                            nPurchaseID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                                        }
                                    }
                                    selectQuery.Finish();
                                    selectQuery = null;
                                    DateTime d = Utils.GetEndDateTime(DDate, nMaxUsageModuleLifeCycle);
                                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                                    updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);
                                    updateQuery += " where ";
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPurchaseID);
                                    updateQuery.Execute();
                                    updateQuery.Finish();
                                    updateQuery = null;
                                    //insert billing transaction and send purchase mail
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Failure - hash not verified
                }
            }
            catch (Exception ex)
            {
                if (nGroupID != 0)
                {

                }
                log.Error("tp_api - " + ex.Message + " - " + ex.StackTrace, ex);
            }
        }
    }
}
