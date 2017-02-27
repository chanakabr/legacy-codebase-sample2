using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DAL;
using System.Data;
using System.Threading;
using KLogMonitor;
using System.Reflection;
using ApiObjects;
using WS_Users;
using TVinciShared;
using M1BL;
using Core.Billing;
using ApiObjects.Billing;

namespace WS_Billing
{
    public partial class M1_Callback : System.Web.UI.Page
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        //private const string M1_CALLBACK_LOG_FILE = "M1Callbacks";
        
        private const string M1_CALLBACK_LOG_HEADER = "M1 Callback";
        private const string M1_PSP_REFERENCE = "M1";
        private const string M1_INVALID_SERVICE_NUMBER_ERROR = "Invalid M1 service no";

        private class MailObj
        {
            public int m_nGroupID;
            public string m_sPaymentMethod;
            public string m_sItemName;
            public string m_sSiteGUID;
            public int m_nBillingTransactionID;
            public string m_sPrice;
            public string m_sCurrency;
            public string m_sMsisdn;
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            string sBaseRedirectUrl = string.Empty;
            try
            {
                string reqForm = Request.Form.ToJSON();
                log.Debug(M1_CALLBACK_LOG_HEADER + string.Format("Entering M1 Callback try block. parameters: URL '{0}', Request [{1}]", Request.Url, reqForm));

                string sessionId;
                string sMsisdn;     //selService;
                string userId;
                string userType;

                //string sM1CallBackLoginCode = string.Empty;
                //string sM1SessionToken = string.Empty;
                //string sMsisdn = string.Empty;
                //string sMerchantReturnData = string.Empty;

                string sAppID;
                string sFixedMailAddress;
                string sResultMessage;
                string sCustomerServiceID;
                int nGroupID = 0;
                int nCustomDataID = 0;
                long nBillingTransactionID = 0;

                bool continuePurchase = ParseCallBackData(out sMsisdn, out sessionId, out userId, out userType, out nCustomDataID, out sAppID, out sResultMessage);
                log.DebugFormat("M1 Callback start - Msisdn: {0}, sessionId: {1}, userId: {2}, userType: {3}, customDataId: {4}, appId: {5}, resMsg: {6}", sMsisdn, sessionId, userId, userType, nCustomDataID, sAppID, sResultMessage);

                if (!continuePurchase)
                {
                    log.Debug(M1_CALLBACK_LOG_HEADER + string.Format("Error on parsing callback params:{0},{1}", Request.Url, sResultMessage));
                    RedirectPage(sBaseRedirectUrl, "Error", M1_INVALID_SERVICE_NUMBER_ERROR, false);
                    return;
                }

                //M1Response m1Response = CheckCallBackLoginCode(sM1CallBackLoginCode);
                //continuePurchase = m1Response.is_succeeded;
                //if (!continuePurchase)
                //{
                //    log.Debug(M1_CALLBACK_LOG_HEADER + string.Format("Error on validate callback login code against M1 api :{0},{1}", Request.Url, m1Response.reason + "," + m1Response.description));
                //    RedirectPage(sBaseRedirectUrl, "Error", M1_INVALID_SERVICE_NUMBER_ERROR, false);
                //    return;
                //}

                M1Response m1Response = CheckFirstPurchasePermissions(sAppID, sessionId, userId, userType, sMsisdn, out nGroupID, out sCustomerServiceID, out sBaseRedirectUrl, out sFixedMailAddress);
                log.DebugFormat("CheckFirstPurchasePermissions for userId {0}, Msisdn: {1}, CustomerServiceID: {2} - Result: {3}", userId, sMsisdn, sCustomerServiceID, m1Response.is_succeeded);

                continuePurchase = m1Response.is_succeeded;
                if (!continuePurchase)
                {
                    log.Debug(M1_CALLBACK_LOG_HEADER + string.Format(" Error on validate user permissions against M1 api: {0}, [{1}]", Request.Url, m1Response.reason + "," + m1Response.description));
                    RedirectPage(sBaseRedirectUrl, "Error", M1_INVALID_SERVICE_NUMBER_ERROR, false);
                    return;
                }

                continuePurchase = ProcessPurchase(nGroupID, nCustomDataID, sMsisdn, sCustomerServiceID, sFixedMailAddress, out nBillingTransactionID, out sResultMessage);
                log.DebugFormat("ProcessPurchase for userId {0}, Msisdn: {1}, CustomerServiceID: {2} - Result: [{3}|{4}], Transaction ID: {5}, ", userId, sMsisdn, sCustomerServiceID, continuePurchase, sResultMessage, nBillingTransactionID);

                if (!continuePurchase)
                {
                    log.Debug(M1_CALLBACK_LOG_HEADER + string.Format(" Error on purchase processing: [{0}], {1}", Request.Url, sResultMessage));
                    RedirectPage(sBaseRedirectUrl, "Error", sResultMessage, false);
                    return;
                }

                int domainId = 0;
                string userEmail = null;
                Utils.IsUserExist(userId, nGroupID, ref userEmail, ref domainId);
                string invalidationKey = CachingProvider.LayeredCache.LayeredCacheKeys.GetPurchaseInvalidationKey(domainId);
                if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key on M1_Callback.Page_Load key = {0}", invalidationKey);
                }

                RedirectPage(sBaseRedirectUrl, "OK", sResultMessage, false, nBillingTransactionID.ToString(), M1_PSP_REFERENCE);
            }
            catch (Exception ex)
            {
                log.Error(M1_CALLBACK_LOG_HEADER + string.Format(" Exception at M1 Callback. Exception Message: {0} , GET call parameters: {1} , Stack trace: {2}", ex.Message, Request.Url.ToString(), ex.StackTrace), ex);
                RedirectPage(sBaseRedirectUrl, "Error", "Invalid error", false);
            }
        }


        private string GetFormSafeValue(string sFormKey)
        {
            if (string.IsNullOrEmpty(Request.Form[sFormKey]))
                return string.Empty;

            return Request.Form[sFormKey];
        }

        private string GetQueryStringSafeValue(string sQueryKey)
        {
            if (string.IsNullOrEmpty(Request.QueryString[sQueryKey]))
                return string.Empty;

            return Request.QueryString[sQueryKey];
        }

        //protected string GetHeaderSafeValue(string sHeaderKey)
        //{
        //    if (string.IsNullOrEmpty(Request.Headers[sHeaderKey]))
        //        return string.Empty;

        //    return Request.Headers[sHeaderKey];
        //}

        private bool ParseCallBackData(out string sMsisdn, out string sessionId, out string userId, out string userType, out int nCustomDataID, out string sAppID, out string sResultMessage)
        {

            sMsisdn = GetFormSafeValue("selService");
            sessionId = GetFormSafeValue("sessionId");
            userId = GetFormSafeValue("userId");
            userType = GetFormSafeValue("userType");

            //sM1SessionToken = GetFormSafeValue("sessionToken");
            //sM1CallBackCode = GetFormSafeValue("code");
            //sMsisdn = GetFormSafeValue("msisdn");
            
            nCustomDataID = 0;
            string sCallBackData = GetQueryStringSafeValue("callBackData");
            sAppID = string.Empty;

            try
            {

                if (string.IsNullOrEmpty(sessionId))
                {
                    sResultMessage = "Invalid M1 sessionId - value is empty";
                    return false;
                }

                if (string.IsNullOrEmpty(userId))
                {
                    sResultMessage = "Invalid M1 userType - value is empty";
                    return false;
                }

                string[] arrParsedData = sCallBackData.Split('-');
                bool bParseMerchantData = int.TryParse(arrParsedData[0], out nCustomDataID);
                if (!bParseMerchantData)
                {
                    sResultMessage = "Cannot parse custom data id";
                    return false;
                }

                sAppID = arrParsedData[1];

                if (string.IsNullOrEmpty(sAppID))
                {
                    sResultMessage = "Cannot parse app id";
                    return false;
                }

            }
            catch
            {
                sResultMessage = "Error parse query string";
                return false;
            }
            sResultMessage = "Ok";
            return true; ;
        }

        private static string GetCustomDataSafeValue(string sQueryKey, System.Xml.XmlNode theRoot)
        {
            try
            {
                if (theRoot.SelectSingleNode(sQueryKey) != null && theRoot.SelectSingleNode(sQueryKey).FirstChild != null)
                {
                    return theRoot.SelectSingleNode(sQueryKey).FirstChild.Value;
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty; ;
            }
        }

        private static string GetCustomDataParamSafeValue(string sQueryKey, string sParName, System.Xml.XmlNode theRoot)
        {
            try
            {
                if (theRoot.SelectSingleNode(sQueryKey) != null && theRoot.SelectSingleNode(sQueryKey).Attributes[sParName] != null)
                {
                    return theRoot.SelectSingleNode(sQueryKey).Attributes[sParName].Value;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Handle Campaign Use
        /// </summary>
        /// <param name="campaignID"></param>
        /// <param name="siteGuid"></param>
        /// <param name="maxNumOfUses"></param>
        /// <param name="maxLifeCycle"></param>
        private static void HandleCampaignUse(int campaignID, string siteGuid, int maxNumOfUses, string maxLifeCycle)
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

        private static bool HandlePPVTransaction(int groupID, string srelevantsub, string smedia_file, string sSiteGUID,
            string price, string scurrency, string sCustomData, string sCountryCode, string sLangCode, string sDevice, string smnou,
            long lBillingTransactionID, string smaxusagemodulelifecycle, int m1ID, string sOverrideEndDate, int domainId)
        {
            bool retVal = false;
            DateTime endDate = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(sOverrideEndDate))
            {
                log.Debug("Override End Date - " + sOverrideEndDate);
                try
                {
                    endDate = DateTime.ParseExact(sOverrideEndDate, "dd/MM/yyyy", null);
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("failed parsing sOverrideEndDate: {0}", sOverrideEndDate), ex);
                }
            }
            else
            {
                int maxUsageModuleLifecycle = 0;
                if (!string.IsNullOrEmpty(smaxusagemodulelifecycle) && int.TryParse(smaxusagemodulelifecycle, out maxUsageModuleLifecycle))
                {
                    log.Debug("Max Usage - " + smaxusagemodulelifecycle);
                    endDate = Utils.GetEndDateTime(DateTime.UtcNow, maxUsageModuleLifecycle);
                }
                else
                {
                    log.ErrorFormat("failed parsing smaxusagemodulelifecycle: {0}", smaxusagemodulelifecycle);
                }
            }

            long purchaseId = ConditionalAccessDAL.Insert_NewPPVPurchase(groupID, long.Parse(smedia_file), sSiteGUID, double.Parse(price), scurrency, !string.IsNullOrEmpty(smnou) ? long.Parse(smnou) : 0,
                                                                         sCustomData, !string.IsNullOrEmpty(srelevantsub) ? srelevantsub : string.Empty, lBillingTransactionID, DateTime.UtcNow, endDate,
                                                                         DateTime.UtcNow, sCountryCode, sLangCode, sDevice, domainId, null);
            if (m1ID != 0)
            {
                UpdateM1PurchaseID(m1ID, (int)purchaseId);
            }
            //Should update the PURCHASE_ID

            if (lBillingTransactionID != 0 && purchaseId != 0)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", purchaseId);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", lBillingTransactionID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
                retVal = true;

            }

            string sItemName = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += "select name from media m, media_files mf where mf.media_id=m.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", int.Parse(smedia_file));
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sItemName = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return retVal;
        }


        private static bool HandleSubscrptionTransaction(int groupID, string sSubscriptionID, string sSiteGUID, string price, string scurrency, string sCustomData, string sCountryCode, string sLangCode, string sDevice,
                                                    string smnou, string sviewlifecyclesecs, string isRecurringStr, string smaxusagemodulelifecycle, long nBillingTransactionID, int m1ID, string sOverrideEndDate,
                                                    string sPreviewModuleID, ref long lPurchaseID, int domainId)
        {
            bool retVal = false;
            long lPreviewModuleID = 0;
            if (!string.IsNullOrEmpty(sPreviewModuleID))
                Int64.TryParse(sPreviewModuleID, out lPreviewModuleID);
            double dPriceToWriteToDatabase = lPreviewModuleID > 0 ? 0.0 : double.Parse(price);
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
            updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            updateQuery += " and ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionID);
            updateQuery += " and ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

            DateTime dtCalculatedEndDate = CalcSubscriptionEndDate(groupID, sOverrideEndDate, smaxusagemodulelifecycle, sPreviewModuleID);
            long purchaseId = ConditionalAccessDAL.Insert_NewMPPPurchase(groupID, sSubscriptionID, sSiteGUID, dPriceToWriteToDatabase, scurrency, sCustomData, sCountryCode, sLangCode, sDevice,
                                                                        !string.IsNullOrEmpty(smnou) ? int.Parse(smnou) : 0, !string.IsNullOrEmpty(sviewlifecyclesecs) ? int.Parse(sviewlifecyclesecs) : 0,
                                                                        !string.IsNullOrEmpty(isRecurringStr) ? bool.Parse(isRecurringStr) : false, nBillingTransactionID, lPreviewModuleID, DateTime.UtcNow,
                                                                        dtCalculatedEndDate, DateTime.UtcNow, "CA_CONNECTION_STRING", domainId);            

            if (m1ID != 0)
            {
                UpdateM1PurchaseID(m1ID, (int)purchaseId);
            }

            //Should update the PURCHASE_ID

            if (nBillingTransactionID != 0 && purchaseId != 0)
            {
                ODBCWrapper.UpdateQuery updateQuery1 = new ODBCWrapper.UpdateQuery("billing_transactions");
                updateQuery1.SetConnectionKey("MAIN_CONNECTION_STRING");
                updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", purchaseId);
                updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
                updateQuery1 += "where";
                updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nBillingTransactionID);
                updateQuery1.Execute();
                updateQuery1.Finish();
                updateQuery1 = null;

                lPurchaseID = purchaseId;

                retVal = true;
            }

            return retVal;
        }


        //private bool GetCallBackGroupParameters(string sAppID, out int nGroupID, out string sAppPassword, out string sWsAdsUrl, out string sWsServiceFacadeUrl, out string sWsServiceInterfaceUrl,
        //                                        out string sSessionValidationUrl, out string sBaseRedirectUrl, out string sFixedMailAddress, out string sResultMessage)
        //{
        //    bool result = true;
        //    nGroupID = 0;
        //    sAppPassword = string.Empty;
        //    sWsAdsUrl = string.Empty;
        //    sWsServiceFacadeUrl = string.Empty;
        //    sWsServiceInterfaceUrl = string.Empty;
        //    sSessionValidationUrl = string.Empty;
        //    sBaseRedirectUrl = string.Empty;
        //    sFixedMailAddress = string.Empty;
        //    sResultMessage = string.Empty;

        //    try
        //    {
        //        DataSet dsGroupParams = BillingDAL.Get_M1GroupParameters(null, sAppID);
        //        if (dsGroupParams != null && dsGroupParams.Tables.Count > 0)
        //        {
        //            DataTable dtGroupParams = dsGroupParams.Tables[0];
        //            if (dtGroupParams != null && dtGroupParams.Rows.Count > 0)
        //            {
        //                DataRow groupParameterRow = dtGroupParams.Rows[0];
        //                nGroupID = ODBCWrapper.Utils.GetIntSafeVal(groupParameterRow["group_id"]);
        //                sAppPassword = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["app_password"]);
        //                sWsAdsUrl = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ws_ads_url"]);
        //                sWsServiceFacadeUrl = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ws_service_facade_url"]);
        //                sWsServiceInterfaceUrl = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ws_service_interface_url"]);
        //                sSessionValidationUrl = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["sessionValidation_url"]);
        //                sBaseRedirectUrl = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["base_redirect_url"]);
        //                sFixedMailAddress = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["invoice_mail_address"]);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(M1_CALLBACK_LOG_HEADER + " AppID=" + sAppID + ",GroupID=" + nGroupID.ToString() + ",exception:" + ex.Message + " || " + ex.StackTrace, ex);
        //        result = false;
        //    }
        //    return result;
        //}

        //private M1Response CheckCallBackLoginCode(string sM1CallBackLoginCode)
        //{
        //    M1Response response = M1Logic.CheckCallBackLoginCode(sM1CallBackLoginCode);
        //    return response;
        //}

        private static M1Response CheckFirstPurchasePermissions(string sAppID, string sSessionId, string sUserId, string sUserType, string sMsisdn, out int nGroupId, out string sCustomerServiceID, out string sBaseRedirectUrl, out string sFixedMailAddress)
        {
            //sCustomerServiceID = string.Empty;
            //sBaseRedirectUrl = string.Empty;
            //sFixedMailAddress = string.Empty;

            M1Response response = M1Logic.CheckFirstPurchasePermissions(sAppID, sSessionId, sUserId, sUserType, sMsisdn, out nGroupId, out sCustomerServiceID, out sBaseRedirectUrl, out sFixedMailAddress);

            return response;
        }
        
        private static DateTime CalcSubscriptionEndDate(int nGroupID, string sOverrideEndDate, string sMaxUsageModuleLifeCycle, string sPreviewModuleID)
        {
            DateTime res = DateTime.MaxValue;
            if (!string.IsNullOrEmpty(sOverrideEndDate))
            {
                try
                {
                    res = DateTime.ParseExact(sOverrideEndDate, "dd/MM/yyyy", null);
                    return res;
                }
                catch
                {
                }

            }
            long lPreviewModuleID = 0;
            if (!string.IsNullOrEmpty(sPreviewModuleID) && Int64.TryParse(sPreviewModuleID, out lPreviewModuleID) && lPreviewModuleID > 0)
            {
                Core.Pricing.PreviewModule pm = Utils.GetPreviewModuleByID(nGroupID, lPreviewModuleID);
                if (pm != null && pm.m_tsFullLifeCycle > 0)
                {
                    res = Utils.GetEndDateTime(DateTime.UtcNow, pm.m_tsFullLifeCycle);
                    return res;
                }

            }
            int nMaxUsageModuleLifeCycle = 0;
            if (!string.IsNullOrEmpty(sMaxUsageModuleLifeCycle) && Int32.TryParse(sMaxUsageModuleLifeCycle, out nMaxUsageModuleLifeCycle) && nMaxUsageModuleLifeCycle > 0)
                res = Utils.GetEndDateTime(DateTime.UtcNow, nMaxUsageModuleLifeCycle);

            return res;

        }
        
        /// <summary>
        /// Update M1 Purchase ID
        /// </summary>
        /// <param name="adyenID"></param>
        /// <param name="ppvID"></param>
        private static void UpdateM1PurchaseID(int m1ID, int purchaseID)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("m1_transactions");
            updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_id", "=", purchaseID);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
            updateQuery += "where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m1ID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

        }

        /// <summary>
        /// Write To User Log
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="sSiteGUID"></param>
        /// <param name="sMessage"></param>
        private static void WriteToUserLog(Int32 nGroupID, string sSiteGUID, string sMessage)
        {
            UsersService u = null;
            try
            {
                u = new UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "WriteLog", "users", sIP, ref sWSUserName, ref sWSPass);
                if (sWSUserName != "")
                    u.WriteLog(sWSUserName, sWSPass, sSiteGUID, sMessage, "Conditional access module");
            }
            catch { }
            finally
            {
                if (u != null)
                    u.Dispose();
            }
        }

        private void RedirectPage(string sBaseRedirect, string sStatus, string sDescription, bool endResponse, string sBillingID = "0", string sM1PspReference = "0")
        {
            Response.Redirect(string.Format("{0}?status={1}&desc={2}&bid={3}&psp={4}", sBaseRedirect, Server.UrlEncode(sStatus), Server.UrlEncode(sDescription), sBillingID, sM1PspReference), endResponse);
        }

        /// <summary>
        /// Handle Coupon Use
        /// </summary>
        /// <param name="sCouponCode"></param>
        /// <param name="sSiteGUID"></param>
        /// <param name="nMediaFileID"></param>
        /// <param name="sSubCode"></param>
        /// <param name="nPrePaidCode"></param>
        /// <param name="nGroupID"></param>
        private static void HandleCouponUse(string sCouponCode, string sSiteGUID, int nMediaFileID, string sSubCode, int nPrePaidCode, int nGroupID)
        {
            int couponID = 0;
            ODBCWrapper.DataSetSelectQuery couponSelectQuery = new ODBCWrapper.DataSetSelectQuery();
            couponSelectQuery.SetConnectionKey("pricing_connection");
            couponSelectQuery += "select id from coupons where ";
            couponSelectQuery += ODBCWrapper.Parameter.NEW_PARAM("code", "=", sCouponCode);
            couponSelectQuery += "and";
            couponSelectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(nGroupID, "MAIN_CONNECTION_STRING");
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

            if (couponID <= 0) { return; }

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

            PricingDAL.Insert_NewCouponUse(sSiteGUID, couponID, nGroupID, nMediaFileID, nSubCode, nPrePaidCode, 0);
        }

        private static bool ProcessPurchase(int nGroupID, int nCustomDataID, string sMsisdn, string sCustomerServiceID, string sFixedMailAddress, out long lBillingTransactionID, out string sResultMessage)
        {
            bool purchaseSuccess = false;
            bool bIsPurchaseWithPreviewModule = false;
            string sCustomData = Utils.GetCustomData(nCustomDataID);

            lBillingTransactionID = 0;
            sResultMessage = string.Empty;


            if (string.IsNullOrEmpty(sCustomData))
            {
                sResultMessage = "Custom data is null";
                return false;
            }

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(sCustomData);
            System.Xml.XmlNode theRequest = doc.FirstChild;


            string ppvOrSub = string.Empty;
            lBillingTransactionID = 0;
            int nM1TransactionID = 0;
            string sReasonToWriteToDB = string.Empty;
            int nBillingProvider = (int)eBillingProvider.M1;
            int nBillingMethod = 60;
            int nBillingProcessor = 30;
            int nBillingStatus = 0; //inserted new value to tvinci.dbo.billing_transactions should be 0   

            string sType = GetCustomDataParamSafeValue(".", "type", theRequest);
            string sSiteGUID = GetCustomDataParamSafeValue("//u", "id", theRequest);
            string email = string.Empty;
            int domainId = 0;
            if (!Utils.IsUserExist(sSiteGUID, nGroupID, ref email, ref domainId))
            {
                log.ErrorFormat("User does not exist, userId: {0}", sSiteGUID);
                return false;
            }
            string sSubscriptionID = GetCustomDataSafeValue("s", theRequest);
            string sPrePaidID = GetCustomDataSafeValue("pp", theRequest);
            string sPPCreditValue = GetCustomDataSafeValue("cpri", theRequest);
            string scouponcode = GetCustomDataSafeValue("cc", theRequest);
            string sPayNum = GetCustomDataParamSafeValue("//p", "n", theRequest);
            string sPayOutOf = GetCustomDataParamSafeValue("//p", "o", theRequest);
            string isRecurringStr = GetCustomDataParamSafeValue("//p", "ir", theRequest);
            string smedia_file = GetCustomDataSafeValue("mf", theRequest);
            string sppvmodule = GetCustomDataSafeValue("ppvm", theRequest);
            string srelevantsub = GetCustomDataSafeValue("rs", theRequest);
            string smnou = GetCustomDataSafeValue("mnou", theRequest);
            string sCountryCode = GetCustomDataSafeValue("lcc", theRequest);
            string sLangCode = GetCustomDataSafeValue("llc", theRequest);
            string sDevice = GetCustomDataSafeValue("ldn", theRequest);
            string smaxusagemodulelifecycle = GetCustomDataSafeValue("mumlc", theRequest);
            string sviewlifecyclesecs = GetCustomDataSafeValue("vlcs", theRequest);
            string price = GetCustomDataSafeValue("pri", theRequest);
            string sCurrency = GetCustomDataSafeValue("cu", theRequest);
            string sUserIP = GetCustomDataSafeValue("up", theRequest);
            string sCampCode = GetCustomDataSafeValue("campcode", theRequest);
            string sCampMNOU = GetCustomDataSafeValue("cmnov", theRequest);
            string sCampLS = GetCustomDataSafeValue("cmumlc", theRequest);
            string sOED = GetCustomDataSafeValue("oed", theRequest);
            string sPreviewModuleID = GetCustomDataSafeValue("pm", theRequest);

            if (price == "")
                price = "0.0";
            int nPaymentNum = 0;
            int nNumberOfPayments = 0;
            if (sPayNum != "")
                nPaymentNum = int.Parse(sPayNum);
            if (sPayOutOf != "")
                nNumberOfPayments = int.Parse(sPayOutOf);

            int nType = 1;
            if (sType == "sp")
            {
                nType = 2;
            }
            double dPrice = double.Parse(price);

            bIsPurchaseWithPreviewModule = !string.IsNullOrEmpty(sPreviewModuleID);
            if (bIsPurchaseWithPreviewModule)
            {
                dPrice = 0.0;
            }


            log.DebugFormat("HandleRemoveDummyVas - User ID: {0}, MSISDN: {1}", sSiteGUID, sMsisdn);
            M1Response m1Response = HandleRemoveDummyVas(nGroupID, sSiteGUID, sMsisdn);
            if (!m1Response.is_succeeded)
            {
                sResultMessage = "Dummy vas removal failed";
                return false;
            }


            lBillingTransactionID = Utils.InsertNewM1Transaction(nGroupID, sSiteGUID, nType, sMsisdn, sCustomerServiceID, dPrice, nCustomDataID, (int)M1TransactionStatus.Pending, string.Empty,
                                                                         sCurrency, sCustomData, nBillingStatus, string.Empty, string.Empty, string.Empty, string.Empty, 1, 1, nBillingProcessor,
                                                                         nBillingMethod, nBillingProvider, ref nM1TransactionID);


            if (lBillingTransactionID <= 0)
            {
                sResultMessage = "Transaction id is invalid";
                return false;
            }

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
                    WriteToUserLog(nGroupID, sSiteGUID, "PPV purchase (CC): " + smedia_file);

                    purchaseSuccess = HandlePPVTransaction(nGroupID, srelevantsub, smedia_file, sSiteGUID, price, sCurrency, sCustomData,
                        sCountryCode, sLangCode, sDevice, smnou, lBillingTransactionID, smaxusagemodulelifecycle, nM1TransactionID, sOED, domainId);

                    if (!string.IsNullOrEmpty(scouponcode))
                    {
                        HandleCouponUse(scouponcode, sSiteGUID, int.Parse(smedia_file), srelevantsub, 0, nGroupID);
                    }
                    #endregion
                    break;

                case "sp":
                    #region Subscription Purchase
                    WriteToUserLog(nGroupID, sSiteGUID, "Subscription purchase (CC): " + sSubscriptionID);
                    long lPurchaseID = 0;
                    purchaseSuccess = HandleSubscrptionTransaction(nGroupID, sSubscriptionID, sSiteGUID, price, sCurrency, sCustomData,
                        sCountryCode, sLangCode, sDevice, smnou, sviewlifecyclesecs,
                        isRecurringStr, smaxusagemodulelifecycle, lBillingTransactionID, nM1TransactionID, sOED, sPreviewModuleID, ref lPurchaseID, domainId);
                    if (!string.IsNullOrEmpty(scouponcode))
                        HandleCouponUse(scouponcode, sSiteGUID, 0, sSubscriptionID, 0, nGroupID);
                    #endregion
                    break;
            }

            SendMailWrapper(nM1TransactionID, sMsisdn, purchaseSuccess, sFixedMailAddress);

            return purchaseSuccess;
        }

        private static void SendPurchaseMail(object mailObj, string sFixedMailAddress)
        {
            MailObj obj = (MailObj)mailObj;
            Utils.SendMailToFixedAddress(obj.m_sPaymentMethod, obj.m_sItemName, obj.m_sSiteGUID, obj.m_nBillingTransactionID, obj.m_sPrice, obj.m_sCurrency, obj.m_sMsisdn, obj.m_nGroupID, sFixedMailAddress, eMailTemplateType.Purchase);
        }

        private static void SendPurchaseFailMail(object mailObj, string sFixedMailAddress)
        {
            MailObj obj = (MailObj)mailObj;
            Utils.SendMailToFixedAddress(obj.m_sPaymentMethod, obj.m_sItemName, obj.m_sSiteGUID, obj.m_nBillingTransactionID, obj.m_sPrice, obj.m_sCurrency, obj.m_sMsisdn, obj.m_nGroupID, sFixedMailAddress, eMailTemplateType.PaymentFail);
        }

        private static bool SendMailWrapper(int nM1TransactionID, string sMsisdn, bool bIsPaymentSuccessful, string sFixedMailAddress)
        {
            DataTable dt = BillingDAL.Get_M1_PurchaseMailData(nM1TransactionID);

            if (dt == null || dt.Rows == null || dt.Rows.Count <= 0) { return false; }
            
            long lBillingID = 0;
            int nBillingMethod = 0;
            long lItemCode = 0;
            int nGroupID = 0;
            string sCurrency = string.Empty;
            string sSiteGuid = string.Empty;
            double dTotalPrice = 0.0;
            string sItemName = string.Empty;
            ItemType it = ItemType.Unknown;

            if (dt.Rows[0]["billing_id"] != DBNull.Value && dt.Rows[0]["billing_id"] != null)
            {
                Int64.TryParse(dt.Rows[0]["billing_id"].ToString(), out lBillingID);
            }

            if (dt.Rows[0]["group_id"] != DBNull.Value && dt.Rows[0]["group_id"] != null)
            {
                Int32.TryParse(dt.Rows[0]["group_id"].ToString(), out nGroupID);
            }

            if (dt.Rows[0]["currency_code"] != DBNull.Value && dt.Rows[0]["currency_code"] != null)
            {
                sCurrency = dt.Rows[0]["currency_code"].ToString();
            }

            if (dt.Rows[0]["site_guid"] != DBNull.Value && dt.Rows[0]["site_guid"] != null)
            {
                sSiteGuid = dt.Rows[0]["site_guid"].ToString();
            }

            if (dt.Rows[0]["total_price"] != DBNull.Value && dt.Rows[0]["total_price"] != null)
            {
                Double.TryParse(dt.Rows[0]["total_price"].ToString(), out dTotalPrice);
            }

            if (dt.Rows[0]["billing_method"] != DBNull.Value && dt.Rows[0]["billing_method"] != null)
            {
                Int32.TryParse(dt.Rows[0]["billing_method"].ToString(), out nBillingMethod);
            }

            if (dt.Rows[0]["SUBSCRIPTION_CODE"] != DBNull.Value && dt.Rows[0]["SUBSCRIPTION_CODE"] != null &&
                dt.Rows[0]["SUBSCRIPTION_CODE"].ToString().Length > 0 &&
                Int64.TryParse(dt.Rows[0]["SUBSCRIPTION_CODE"].ToString(), out lItemCode) && lItemCode > 0)
            {
                it = ItemType.Subscription;
            }
            else
            {
                if (dt.Rows[0]["PPVMODULE_CODE"] != DBNull.Value && dt.Rows[0]["PPVMODULE_CODE"] != null &&
                    dt.Rows[0]["PPVMODULE_CODE"].ToString().Length > 0 &&
                    Int64.TryParse(dt.Rows[0]["PPVMODULE_CODE"].ToString(), out lItemCode) && lItemCode > 0)
                {
                    it = ItemType.PPV;
                }
            }

            sItemName = GetItemName(it, lItemCode, nM1TransactionID);

            MailObj obj = new MailObj
            {
                m_nBillingTransactionID = (int) lBillingID,
                m_nGroupID = nGroupID,
                m_sCurrency = sCurrency,
                m_sItemName = sItemName,
                m_sPaymentMethod = Utils.GetPaymentMethod(nBillingMethod),
                m_sPrice = dTotalPrice + "",
                m_sMsisdn = sMsisdn,
                m_sSiteGUID = sSiteGuid
            };

            Thread t = bIsPaymentSuccessful ? 
                new Thread(delegate() { SendPurchaseMail(obj, sFixedMailAddress); }) : 
                new Thread(delegate() { SendPurchaseFailMail(obj, sFixedMailAddress); });

            t.Start();

            return true;
        }

        private static M1Response HandleRemoveDummyVas(int nGroupID, string sSiteGUID, string sMsisdn)
        {
            M1Response m1Response = new M1Response
            {
                is_succeeded = true,
                reason = string.Empty
            };
            
            string sLastCellPhone = string.Empty;
            string sCustomerServiceID = string.Empty;
            bool isLastM1CellPhoneExists = Utils.GetLastM1BillingUserInfo(nGroupID, sSiteGUID, out sLastCellPhone, out sCustomerServiceID);
            if (!isLastM1CellPhoneExists || (sLastCellPhone.Equals(sMsisdn)))
            {
                return m1Response;
            }

            m1Response = M1Logic.RemoveDummyVas(nGroupID, sLastCellPhone, sCustomerServiceID);
            
            if (!m1Response.is_succeeded)
            {
                log.Debug(M1_CALLBACK_LOG_HEADER + string.Format("Error on remove dummy vas,GroupID:{0},SiteGuid{1},Msisdn{2},Desc{3}", nGroupID.ToString(), sSiteGUID, sMsisdn, m1Response.description));
            }

            return m1Response;
        }

        private static string GetItemName(ItemType it, long lItemCode, int nM1TransactionID)
        {
            string sTableName = string.Empty;
            string res = string.Empty;

            switch (it)
            {
                case (ItemType.Subscription):
                    sTableName = "subscriptions";
                    break;
                case (ItemType.PPV):
                    res = ApiDAL.Get_M1_PPVNameForPurchaseMail(nM1TransactionID);
                    break;
                default:
                    break;

            }
            if (sTableName.Length > 0)
                res = PricingDAL.Get_ItemName(sTableName, lItemCode);

            return res;
        }

    }
}