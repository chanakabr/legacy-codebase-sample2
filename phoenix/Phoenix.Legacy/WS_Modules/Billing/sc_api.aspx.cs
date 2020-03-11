using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using System.Text;
using KLogMonitor;
using System.Reflection;
using ApiObjects;
using WS_Users;
using Core.Billing;

namespace WS_Billing
{
    public partial class sc_api : System.Web.UI.Page
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected string GetSafeValue(string sQueryKey , ref System.Xml.XmlNode theRoot)
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

        protected string GetSafeParValue(string sQueryKey, string sParName , ref System.Xml.XmlNode theRoot)
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

        protected Int32 GetGroupID(string sCheckSum , string sUserName)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            selectQuery += "select * from sc_group_parameters where is_active=1 and status=1";
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUserName);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["GROUP_ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        protected Int32 GetGroupIDPU(string sMerchantID, string sMerchantSiteID , ref string sSecret)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            selectQuery += "select * from sc_group_parameters where is_active=1 and status=1";
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MERCHANT_ID", "=", sMerchantID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MERCHANT_SITE_ID", "=", sMerchantSiteID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["GROUP_ID"].ToString());
                    sSecret = selectQuery.Table("query").DefaultView[0].Row["POPUP_SECRET"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected Int32 GetMainLang(ref string sMainLang, Int32 nGroupID)
        {
            Int32 nLangID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select l.NAME,l.id from groups g,lu_languages l where l.id=g.language_id and  ";
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sMainLang = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
                    nLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nLangID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            log.Debug("Return from G2S - "+ Request.Url.ToString());
         
            Response.Expires = -1;
            string sResponse = GetSafeValue("response"); 
            if (sResponse != "")
            {
                try
                {
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    doc.LoadXml(sResponse);
                    System.Xml.XmlNode theRequest = doc.SelectSingleNode("/Response");

                    string sCheckSum = GetSafeValue("CheckSum", ref theRequest);
                    string sStatus = GetSafeValue("Status", ref theRequest);
                    string sCustomData = HttpUtility.UrlDecode(GetSafeValue("CustomData", ref theRequest));
                    string[] toSep = { "|" };
                    string[] sCustomSep = sCustomData.Split(toSep, StringSplitOptions.RemoveEmptyEntries);

                    if (sCustomSep.Length == 2)
                    {
                        string sSiteGUID = sCustomSep[1];
                        string sToSave = sCustomSep[0];
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
                            string sExpMonth = GetSafeValue("sg_ExpMonth", ref theRequest);
                            string sExpYear = GetSafeValue("sg_ExpYear", ref theRequest);
                            string sUserName = GetSafeValue("ClientLoginID", ref theRequest);
                            string sToken = GetSafeValue("Token", ref theRequest);
                            string sTransactionID = GetSafeValue("TransactionID", ref theRequest);
                            string sCardNumber = GetSafeValue("sg_CardNumber", ref theRequest);
                            sCardNumber = sCardNumber.Substring(sCardNumber.Length - 4);

                            string sDCIssue = GetSafeValue("sg_DC_Issue", ref theRequest);
                            string sDCStartM = GetSafeValue("sg_DC_StartMon", ref theRequest);
                            string sDCStartY = GetSafeValue("sg_DC_StartYear", ref theRequest);
                            string sIssuingBank = GetSafeValue("sg_IssuingBankName", ref theRequest);

                            if (sStatus.Trim().ToLower().StartsWith("approved") == true ||
                                sStatus.Trim().ToLower().StartsWith("success") == true)
                            {
                                Int32 nGroupID = GetGroupID(sCheckSum, sUserName);
                                if (nGroupID != 0)
                                {
                                    //Update token
                                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_tokens");
                                    insertQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TOKEN", "=", sToken);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_FOUR_DIGITS", "=", sCardNumber);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EXP_MONTH", "=", int.Parse(sExpMonth));
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EXP_YEAR", "=", int.Parse(sExpYear));

                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DC_ISSUE", "=", sDCIssue);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DC_START_MONTH", "=", sDCStartM);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DC_START_YEAR", "=", sDCStartY);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DC_ISSUING_BANK", "=", sIssuingBank);
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TRANSACTION_ID", "=", int.Parse(sTransactionID));
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
                                    log.Debug("Success: "+ sUserName + " || " + sResponse);
                                    Response.Write("OK");
                                }
                                else
                                {
                                    log.Debug("Error: - group id not recognized for: " + sUserName + " || " + sResponse);
                                    Response.Write("Fail");
                                }
                            }
                            else
                            {
                                log.Debug("Error: status: " + sStatus);
                                Response.Write("Fail");
                            }
                        }
                        else
                        {
                            log.Debug("Error: site guid is 0: " + sCustomData);
                            Response.Write("Fail");
                        }
                    }
                    else
                    {
                        log.Debug("Error: CustomData with wrong structure: " + sCustomData);
                        Response.Write("Fail");
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Exception on: " + sResponse + " || " + ex.Message + " || " + ex.StackTrace, ex);
                    Response.Write("Fail");
                }
            }
            else
            {
                Int32 nGroupID = 0;
                try
                {
                    bool transExists = false;
                    string sStatus = GetSafeValue("Status");
                    string stotalAmount = GetSafeValue("totalAmount");
                    string sTransactionID = GetSafeValue("TransactionID");
                    string sClientUniqueID = GetSafeValue("ClientUniqueID");
                    string sErrCode = GetSafeValue("ErrCode");
                    string sExErrCode = GetSafeValue("ExErrCode");
                    string sAuthCode = GetSafeValue("AuthCode");
                    string sReason = GetSafeValue("Reason");
                    string sToken = GetSafeValue("Token");
                    string sReasonCode = GetSafeValue("ReasonCode");
                    string sadvanceResponseChecksum = GetSafeValue("advanceResponseChecksum");
                    string snameOnCard = GetSafeValue("nameOnCard");
                    string scurrency = GetSafeValue("currency");
                    string scustomField1 = GetSafeValue("customField1");
                    string scustomField2 = GetSafeValue("customField2");
                    string scustomDataID = GetSafeValue("customData");
                    string sCustomData = Utils.GetCustomData(int.Parse(scustomDataID));
                    //customdata id from db
                    string smerchant_site_id = GetSafeValue("merchant_site_id");
                    string smerchant_id = GetSafeValue("merchant_id");
                    string smessage = GetSafeValue("message");
                    string sError = GetSafeValue("Error") + " - " + sReason;
                    string sPPP_TransactionID = GetSafeValue("PPP_TransactionID");
                    string sppp_status = GetSafeValue("ppp_status");
                    string sResponseTimeStamp = GetSafeValue("responseTimeStamp");
                    string sProductID = GetSafeValue("productId");
                    string sitem_name_1 = GetSafeValue("item_name_1");
                    string sitem_quantity_1 = GetSafeValue("item_quantity_1");
                    if (sProductID == "")
                        sProductID = sitem_name_1;
                    string sSecret = "";
                    nGroupID = GetGroupIDPU(smerchant_id, smerchant_site_id, ref sSecret);
                    string sToHash = sSecret + stotalAmount + scurrency + sResponseTimeStamp + sPPP_TransactionID + sStatus + sProductID;
                    string sHased = "";
                    MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
                    md5Provider = new MD5CryptoServiceProvider();
                    byte[] originalBytes = UTF8Encoding.Default.GetBytes(sToHash);
                    byte[] encodedBytes = md5Provider.ComputeHash(originalBytes);
                    sHased = BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
                    //if (sHased == sadvanceResponseChecksum)
                    //{
                    string sCustomHashed = "";
                    if (scustomDataID != "" && scustomField1 != "")
                    {
                        originalBytes = UTF8Encoding.Default.GetBytes(scustomDataID);
                        encodedBytes = md5Provider.ComputeHash(originalBytes);
                        sCustomHashed = BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
                    }
                    string sType = "";
                    string sSiteGUID = "";
                    string smedia_file = "";
                    string sSubscriptionID = "";
                    if (sCustomHashed == "" || sCustomHashed == scustomField1)
                    {
                        string scouponcode = "";
                        string sPayNum = "";
                        string sPayOutOf = "";
                        string sppvmodule = "";
                        string srelevantsub = "";
                        string smnou = "";
                        string smaxusagemodulelifecycle = "";
                        string sviewlifecyclesecs = "";
                        string sDigits = "";
                        string sCountryCode = "";
                        string sLangCode = "";
                        string sDevice = "";
                        long lBillingTransactionID = 0;
                        if (sCustomData != "")
                        {
                            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                            doc.LoadXml(sCustomData);
                            System.Xml.XmlNode theRequest = doc.FirstChild;

                            sType = GetSafeParValue(".", "type", ref theRequest);
                            sSiteGUID = GetSafeParValue("//u", "id", ref theRequest);
                            sSubscriptionID = GetSafeValue("s", ref theRequest);
                            scouponcode = GetSafeValue("cc", ref theRequest);
                            sPayNum = GetSafeParValue("//p", "n", ref theRequest);
                            sPayOutOf = GetSafeParValue("//p", "o", ref theRequest);
                            smedia_file = GetSafeValue("mf", ref theRequest);
                            sppvmodule = GetSafeValue("ppvm", ref theRequest);
                            srelevantsub = GetSafeValue("rs", ref theRequest);
                            smnou = GetSafeValue("mnou", ref theRequest);
                            sCountryCode = GetSafeValue("lcc", ref theRequest);
                            sLangCode = GetSafeValue("llc", ref theRequest);
                            sDevice = GetSafeValue("ldn", ref theRequest);
                            smaxusagemodulelifecycle = GetSafeValue("mumlc", ref theRequest);
                            sviewlifecyclesecs = GetSafeValue("vlcs", ref theRequest);
                            sDigits = GetSafeValue("cc_card_number");
                            if (stotalAmount == "")
                                stotalAmount = "0.0";
                            Int32 nPaymentNum = 0;
                            Int32 nNumberOfPayments = 0;
                            if (sPayNum != "")
                                nPaymentNum = int.Parse(sPayNum);
                            if (sPayOutOf != "")
                                nNumberOfPayments = int.Parse(sPayOutOf);
                            Int32 nBillingProvider = 1;
                            Int32 nBillingMethod = 1;
                            if (scustomField2.ToLower().Trim() == "paypal")
                            {
                                nBillingProvider = 4;
                                nBillingMethod = 3;
                            }
                            if (scustomField2.ToLower().Trim() == "cc_card")
                            {
                                nBillingProvider = 1;
                                nBillingMethod = 1;
                            }
                            if (scustomField2.ToLower().Trim() == "dc_card")
                            {
                                nBillingProvider = 7;
                                nBillingMethod = 4;
                            }
                            if (scustomField2.ToLower().Trim() == "ideal")
                            {
                                nBillingProvider = 9;
                                nBillingMethod = 5;
                            }
                            if (scustomField2.ToLower().Trim() == "directdebit_nl")
                            {
                                nBillingProvider = 10;
                                nBillingMethod = 6;
                            }
                            if (!IsTransactionExists(nGroupID, sSiteGUID, sDigits, double.Parse(stotalAmount), scurrency,
                                sCustomData, sPPP_TransactionID + "|" + sTransactionID, sStatus, sType, "", "",
                                sReason, sErrCode, sExErrCode, nPaymentNum, nNumberOfPayments, 1, nBillingMethod, nBillingProvider))
                            {

                                lBillingTransactionID = InsertNewSCTransaction(nGroupID, sSiteGUID, sDigits, double.Parse(stotalAmount), scurrency,
                                    sCustomData, sPPP_TransactionID + "|" + sTransactionID, sStatus, sType, "", "",
                                    sReason, sErrCode, sExErrCode, nPaymentNum, nNumberOfPayments, 1, nBillingMethod, nBillingProvider);
                            }
                            else
                            {
                                transExists = true;
                                sError = "Duplicate transaction";
                            }
                        }
                        if ((sStatus.Trim().ToLower().StartsWith("approved") == true ||
                                sStatus.Trim().ToLower().StartsWith("success") == true) && !transExists)
                        {
                            if (sType == "pp")
                            {
                                try { WriteToUserLog(nGroupID, sSiteGUID, "Media file id: " + smedia_file + " Purchased(: " + scustomField2.ToString() + ")" + stotalAmount + scurrency); }
                                catch { }
                                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ppv_purchases");
                                insertQuery.SetConnectionKey("CA_CONNECTION_STRING");
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                                if (srelevantsub != "")
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", srelevantsub);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", int.Parse(smedia_file));
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", double.Parse(stotalAmount));
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", scurrency);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", sCustomData);
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
                                if (smaxusagemodulelifecycle != "")
                                {
                                    DateTime d = Utils.GetEndDateTime(DateTime.UtcNow, int.Parse(smaxusagemodulelifecycle));
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);
                                    //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", GetCurrentDBTime().AddSeconds());
                                }

                                insertQuery.Execute();
                                insertQuery.Finish();
                                insertQuery = null;

                                Int32 nPurchaseID = 0;

                                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                selectQuery += "select id from ppv_purchases where ";
                                selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                                if (srelevantsub != "")
                                {
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", srelevantsub);
                                }
                                selectQuery += " and ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", int.Parse(smedia_file));
                                selectQuery += " and ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                selectQuery += " and ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", double.Parse(stotalAmount));
                                selectQuery += " and ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", scurrency);
                                selectQuery += " and ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                if (smnou != "")
                                {
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", int.Parse(smnou));
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
                                    selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", int.Parse(smedia_file));
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
                                    string sPaymentMethod = scustomField2.ToLower();
                                    //sPaymentMethod += " (************" + sDigits + ")";
                                    Utils.SendMail(sPaymentMethod, sItemName , sSiteGUID , lBillingTransactionID , 
                                        stotalAmount , scurrency , string.Empty, nGroupID, string.Empty, eMailTemplateType.Purchase);
                                }

                                string sBaseRedirect = ODBCWrapper.Utils.GetTableSingleVal("sc_group_parameters", "BASE_REDIRECT_URL", "group_id", "=", nGroupID, 3600).ToString();
                                Response.Redirect(sBaseRedirect + "?status=OK&desc=OK", false);
                            }
                            else if (sType == "sp")
                            {
                                try { WriteToUserLog(nGroupID, sSiteGUID, "Subscription purchase (CC): " + sSubscriptionID); }
                                catch { }

                                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                                updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                                updateQuery += " where ";
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                                updateQuery += " and ";
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionID);
                                updateQuery += " and ";
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                updateQuery.Execute();
                                updateQuery.Finish();
                                updateQuery = null;

                                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscriptions_purchases");
                                insertQuery.SetConnectionKey("CA_CONNECTION_STRING");
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionID);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", double.Parse(stotalAmount));
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", scurrency);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", 0);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", sCustomData);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCode);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLangCode);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDevice);
                                if (smnou != "")
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", int.Parse(smnou));
                                else
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", 0);

                                if (sviewlifecyclesecs != "")
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", int.Parse(sviewlifecyclesecs));
                                else
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", 0);

                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", lBillingTransactionID);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                                if (smaxusagemodulelifecycle != "")
                                {
                                    DateTime d = Utils.GetEndDateTime(DateTime.UtcNow, int.Parse(smaxusagemodulelifecycle));
                                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);
                                    //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", GetCurrentDBTime().AddSeconds(int.Parse(smaxusagemodulelifecycle)));
                                }
                                insertQuery.Execute();
                                insertQuery.Finish();
                                insertQuery = null;

                                Int32 nPurchaseID = 0;

                                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                selectQuery += " select id from subscriptions_purchases where ";
                                selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                                selectQuery += " and ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionID);
                                selectQuery += " and ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                                selectQuery += " and ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", double.Parse(stotalAmount));
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
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                                selectQuery += " and ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                                if (smaxusagemodulelifecycle != "")
                                {
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", GetCurrentDBTime().AddSeconds(int.Parse(smaxusagemodulelifecycle)));
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
                                    string sPaymentMethod = scustomField2;
                                    if (scustomField2.ToLower().Trim() == "cc_card")
                                    {
                                        sPaymentMethod = "Credit Card";
                                        sPaymentMethod += " (************" + sDigits + ")";
                                    }
                                    string subName = GetSubName(sSubscriptionID);
                                    Utils.SendMail(sPaymentMethod, subName, sSiteGUID, lBillingTransactionID, stotalAmount, scurrency, string.Empty, nGroupID, string.Empty, eMailTemplateType.Purchase);
                                }

                                string sBaseRedirect = ODBCWrapper.Utils.GetTableSingleVal("sc_group_parameters", "BASE_REDIRECT_URL", "group_id", "=", nGroupID, 3600).ToString();
                                Response.Redirect(sBaseRedirect + "?status=OK&desc=OK", false);
                            }
                        }
                        else
                        {
                            string sBaseRedirect = ODBCWrapper.Utils.GetTableSingleVal("sc_group_parameters", "BASE_REDIRECT_URL", "group_id", "=", nGroupID, 3600).ToString();
                            Response.Redirect(sBaseRedirect + "?status=Error&desc=" + Server.UrlEncode(sError), false);
                            if (sType == "pp")
                            {
                                try { WriteToUserLog(nGroupID, sSiteGUID, "while trying to purchase media file(CC): " + smedia_file + " error returned: " + sError); }
                                catch { }
                            }
                            else if (sType == "sp")
                            {
                                try { WriteToUserLog(nGroupID, sSiteGUID, "while trying to purchase subscription(CC): " + sSubscriptionID + " error returned: " + sError); }
                                catch { }
                            }

                        }
                    }
                    else
                    {
                        string sBaseRedirect = ODBCWrapper.Utils.GetTableSingleVal("sc_group_parameters", "BASE_REDIRECT_URL", "group_id", "=", nGroupID, 3600).ToString();
                        Response.Redirect(sBaseRedirect + "?status=Error&desc=" + Server.UrlEncode("custom hash not verfied") , false);
                        if (sType == "pp")
                        {
                            try { WriteToUserLog(nGroupID, sSiteGUID, "while trying to purchase media file(CC): " + smedia_file + " error returned: " + sError); }
                            catch { }
                        }
                        else if (sType == "sp")
                        {
                            try { WriteToUserLog(nGroupID, sSiteGUID, "while trying to purchase subscription(CC): " + sSubscriptionID + " error returned: " + sError); }
                            catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (nGroupID != 0)
                    {
                        string sBaseRedirect = ODBCWrapper.Utils.GetTableSingleVal("sc_group_parameters", "BASE_REDIRECT_URL", "group_id", "=", nGroupID, 3600).ToString();
                        Response.Redirect(sBaseRedirect + "?status=Error&desc=" + Server.UrlEncode(ex.Message + "-" + ex.StackTrace), false);
                    }
                    log.Error("sc_api - "+ ex.Message + " - " + ex.StackTrace, ex);
                }
            }
        }

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

        protected bool IsTransactionExists(Int32 nGroupID, string sSiteGUID, string sDigits, double dPrice,
            string sCurrency, string sCustomData, string sTransactionID, string sStatus, string sAuthCode,
            string sAVSCode, string sCVV2Reply, string sReason, string sErrCode, string sExErrCode,
            Int32 nPaymentNum, Int32 nNumberOfPayments, Int32 nBILLING_PROCESSOR, Int32 nBILLING_METHOD,
            Int32 nBillingProvider)
        {
            bool retVal = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            selectQuery += "select * from sc_transactions where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_FOUR_DIGITS", "=", sDigits);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_CUSTOMDATA", "=", sCustomData);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_TRANSACTIONID", "=", sTransactionID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_STATUS", "=", sStatus);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_AUTHCODE", "=", sAuthCode);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_AVSCODE", "=", sAVSCode);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_CVV2REPLY", "=", sCVV2Reply);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_REASON", "=", sReason);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    retVal = true;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        protected long InsertNewSCTransaction(Int32 nGroupID , string sSiteGUID, string sDigits, double dPrice,
            string sCurrency, string sCustomData, string sTransactionID , string sStatus , string sAuthCode , 
            string sAVSCode , string sCVV2Reply , string sReason , string sErrCode , string sExErrCode ,
            Int32 nPaymentNum, Int32 nNumberOfPayments, Int32 nBILLING_PROCESSOR, Int32 nBILLING_METHOD , 
            Int32 nBillingProvider)
        {
            //Handle the custom data
            Int32 nRet = 0;
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("sc_transactions");
            insertQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_FOUR_DIGITS", "=", sDigits);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_CUSTOMDATA", "=", sCustomData);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_TRANSACTIONID", "=", sTransactionID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_STATUS", "=", sStatus);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_AUTHCODE", "=", sAuthCode);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_AVSCODE", "=", sAVSCode);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_CVV2REPLY", "=", sCVV2Reply);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_REASON", "=", sReason);
            if (sErrCode != "")
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_ERRORCODE", "=", int.Parse(sErrCode));
            if (sExErrCode != "")
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_EXTERRORCODE", "=", int.Parse(sExErrCode));
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 0);

            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            selectQuery += "select id from sc_transactions where is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_FOUR_DIGITS", "=", sDigits);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 0);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_TRANSACTIONID", "=", sTransactionID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_STATUS", "=", sStatus);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_AUTHCODE", "=", sAuthCode);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_AVSCODE", "=", sAVSCode);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_CVV2REPLY", "=", sCVV2Reply);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_REASON", "=", sReason);
            if (sErrCode != "")
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_ERRORCODE", "=", int.Parse(sErrCode));
            }
            if (sExErrCode != "")
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_EXTERRORCODE", "=", int.Parse(sExErrCode));
            }
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());

            }
            selectQuery.Finish();
            selectQuery = null;

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
            string sPPCode = string.Empty;
            if (sStatus.Trim().ToLower().StartsWith("approved") == true || sStatus.Trim().ToLower().StartsWith("success") == true)
                nStatus = 0;
            else
                nStatus = 1;
            Utils.SplitRefference(sCustomData, ref nMediaFileID, ref nMediaID, ref sSubscriptionCode, ref sPPVCode, ref sPPCode, ref sPriceCode,
                    ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPayments, ref sUserGUID,
                                ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs, ref sPurchaseType , 
                                ref sCountryCd , ref sLanguageCode , ref sDeviceName);

            long lBillingTransactionID = Utils.InsertBillingTransaction(sSiteGUID, sDigits, dChargePrice, sPriceCode,
                    sCurrencyCode, sCustomData, nStatus, sErrCode + "|" + sExErrCode + "|" + sReason, bIsRecurring, nMediaFileID, nMediaID, sPPVModuleCode,
                    sSubscriptionCode, "", nGroupID, nBillingProvider, nRet, 0.0, dChargePrice, nPaymentNum, nNumberOfPayments, "",
                    sCountryCd, sLanguageCode, sDeviceName, nBILLING_PROCESSOR, nBILLING_METHOD, sPPCode);

            return lBillingTransactionID;
        }

        protected void WriteToUserLog(Int32 nGroupID , string sSiteGUID, string sMessage)
        {
            UsersService u = new UsersService();
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";
            TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "WriteLog", "users", sIP, ref sWSUserName, ref sWSPass);
            if (sWSUserName != "")
                u.WriteLog(sWSUserName, sWSPass, sSiteGUID, sMessage, "Conditional access module");
        }

        static public DateTime GetCurrentDBTime()
        {
            object t = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            selectQuery += "select getdate() as t from groups_modules_ips";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    t = selectQuery.Table("query").DefaultView[0].Row["t"];
            }
            selectQuery.Finish();
            selectQuery = null;
            if (t != null && t != DBNull.Value)
                return (DateTime)t;
            return new DateTime();
        }
    }
}
