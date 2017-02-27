using System;
using DAL;
using System.Text;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;
using System.Diagnostics;
using ApiObjects.Billing;
using Core.Billing;
using CachingProvider.LayeredCache;

namespace WS_Billing
{
    public partial class adyen_api : System.Web.UI.Page
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public class MailObj
        {
            public int m_nGroupID;
            public string m_sPaymentMethod;
            public string m_sItemName;
            public string m_sSiteGUID;
            public int m_nBillingTransactionID;
            public string m_sPrice;
            public string m_sCurrency;
            public string m_sPSPReference;
            public string m_sLast4Digits;
            public string m_PreviewModulePeriod;
        }

        protected enum AdyenStatus
        {
            Cancelled = 0,
            Pending = 1,
            Authorized = 2
        }

        #region gad's changes
        private const string ADYEN_NOTIFICATION_LOG_HEADER = "Adyen Notification";
        private const string ADYEN_NOTIFICATION_LOG_FILE = "AdyenCallbacks";
        private const string ADYEN_CALLBACK_LOG_HEADER = "Adyen Callback";
        private const string ADYEN_CALLBACK_LOG_FILE = "AdyenCallbacks";
        private const string ADYEN_PAYMENT_ACCEPTED = "AUTHORISED";
        private const string ADYEN_PAYMENT_REFUSED = "REFUSED";

        private enum AdyenEventCode
        {
            Authorisation = 0,
            Refund = 1,
            Cancel = 2,
            CancelOrRefund = 3,
            Other = 4
        }

        private void GetAdyenNotificationData(ref string sPSPReference, ref string sEventCode, ref string sSuccess, ref string sLast4Digits, ref string sReason)
        {
            if (!string.IsNullOrEmpty(Request.Form["pspReference"]))
                sPSPReference = Request.Form["pspReference"];
            if (!string.IsNullOrEmpty(Request.Form["eventCode"]))
                sEventCode = Request.Form["eventCode"].ToLower();
            if (!string.IsNullOrEmpty(Request.Form["success"]))
            {
                string sTempSuccess = Request.Form["success"].Trim().ToLower();
                if (sTempSuccess == "true")
                    sSuccess = sTempSuccess;
                else
                {
                    if (sTempSuccess == "false")
                        sSuccess = sTempSuccess;
                }
            }
            string sTempReason = Request.Form["reason"];
            if (string.IsNullOrEmpty(sTempReason))
            {
                sLast4Digits = string.Empty;
                sReason = string.Empty;
            }
            else
            {
                sReason = sTempReason;
                string[] oReasonArr = sReason.Split(':');
                if (oReasonArr != null && oReasonArr.Length > 1)
                    sLast4Digits = oReasonArr[1];

            }

        }

        private AdyenEventCode ConvertEventCode(string sEventCodeInLowerCase)
        {
            if (sEventCodeInLowerCase == "authorisation")
                return AdyenEventCode.Authorisation;
            if (sEventCodeInLowerCase == "cancel")
                return AdyenEventCode.Cancel;
            if (sEventCodeInLowerCase == "refund")
                return AdyenEventCode.Refund;
            if (sEventCodeInLowerCase == "cancel_or_refund")
                return AdyenEventCode.CancelOrRefund;
            return AdyenEventCode.Other;
        }

        private void UpdateDataFromAdyenNotificationInDB(string sPSPReference, string sStatus, string sReason, int nPurchaseType, long lIDInAdyenTransactions, string sLast4Digits, long lIDInBillingTransactions)
        {
            string sTrimmedLast4Digits = sLast4Digits.Trim();

            BillingDAL.Update_AdyenTransactionStatusReasonLast4Digits(lIDInAdyenTransactions, sStatus, sReason, sTrimmedLast4Digits.Length == 4 ? sTrimmedLast4Digits : string.Empty);
            if (sStatus.ToLower() == ADYEN_PAYMENT_ACCEPTED.ToLower())
            {
                // update last four digits in billing_transactions. critical for purchase mail send.

                if (sTrimmedLast4Digits.Length != 4)
                {
                    // incorrect format of last 4 digits.
                    #region Logging
                    log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + string.Format("UpdateDataFromAdyenNotificationInDB. Incorrect format of last 4 digits. PSP Ref: {0} , Status: {1} , Reason: {2} , Last 4 digits: {3}", sPSPReference, sStatus, sReason, sLast4Digits));
                    #endregion
                }
                else
                {
                    ApiDAL.Update_Last4Digits(lIDInBillingTransactions, sTrimmedLast4Digits);
                }

            }
            if (sStatus.ToLower() == ADYEN_PAYMENT_REFUSED.ToLower())
            {
                if (!ConditionalAccessDAL.Update_BusinessModulePurchaseIsActive(lIDInAdyenTransactions, false, nPurchaseType, "CA_CONNECTION_STRING"))
                {
                    // failed to terminate entitlement of business module.
                    #region Logging
                    log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + string.Format("Failed to terminate entitlement in relevant ca table. Billing transaction ID: {0} , Purchase type: {1} , Reason: {2} , Status: {3}", lIDInBillingTransactions, nPurchaseType, sReason, sStatus));
                    #endregion
                }
                if (!ApiDAL.Update_BillingStatusAndReason(lIDInBillingTransactions, false, sReason))
                {
                    // failed to update billing status and reason at billing_transactions.
                    #region Logging
                    log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + string.Format("Failed to update billing status to 1 and billing reason to: {0} . Billing Transaction ID: {1} , PSP Ref: {2}", sReason, lIDInBillingTransactions, sPSPReference));
                    #endregion
                }
                #region Logging
                log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + string.Format("UpdateDataFromAdyenNotificationInDB. Payment refused. Set MPP is_active to 0. PSP Ref: {0} , ID in adyen_transactions {1} , ID in billing_transactions {2}", sPSPReference, lIDInAdyenTransactions, lIDInBillingTransactions));
                #endregion


                int domainId = 0;
                int groupId = 0;
                string userEmail = null;

                string userId = ApiDAL.GetUserIdByBillingTransactionId(lIDInBillingTransactions, out groupId);
                Core.Billing.Utils.IsUserExist(userId, groupId, ref userEmail, ref domainId);
                string invalidationKey = CachingProvider.LayeredCache.LayeredCacheKeys.GetCancelTransactionInvalidationKey(domainId);
                if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key on UpdateDataFromAdyenNotificationInDB key = {0}", invalidationKey);
                }
            }

        }

        private void HandleAdyenAuthorisationEvent(string sPSPReference, bool? bIsSuccess, string sLast4Digits, string sReason)
        {
            long lIDInAdyenTransactions = 0;
            long lIDInBillingTransactions = 0;
            long lIDInRelevantCATable = 0; // id in subscription_purchases or ppv_purchases or pre_paid_purchases
            int nPurchaseType = 0;
            bool bIsNotificationResultOfCallback = false;
            bool bIsPaymentSuccessful = true;
            bool bIsPurchasedWithPreviewModule = false;
            bool shouldSendMail = false;

            if (BillingDAL.Get_DataForAdyenNotification_And_HandleMail(sPSPReference, ref lIDInAdyenTransactions, 
                ref lIDInBillingTransactions,  ref bIsNotificationResultOfCallback, ref nPurchaseType, ref lIDInRelevantCATable, 
                ref bIsPurchasedWithPreviewModule, ref shouldSendMail) && 
                lIDInAdyenTransactions > 0 && lIDInBillingTransactions > 0 && 
                lIDInRelevantCATable > 0)
            {
                if (!bIsSuccess.HasValue)
                {
                    log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + string.Format("No success value at HandleAdyenAuthorisationEvent. PSP Reference: {0}", sPSPReference));
                }
                else
                {
                    bIsPaymentSuccessful = bIsSuccess.Value;
                    if (bIsPaymentSuccessful)
                    {
                        UpdateDataFromAdyenNotificationInDB(sPSPReference, ADYEN_PAYMENT_ACCEPTED, string.Empty, nPurchaseType, lIDInAdyenTransactions, sLast4Digits, lIDInBillingTransactions);
                    }
                    else
                    {
                        // payment failed.
                        UpdateDataFromAdyenNotificationInDB(sPSPReference, ADYEN_PAYMENT_REFUSED, sReason, nPurchaseType, lIDInAdyenTransactions, sLast4Digits, lIDInBillingTransactions);

                    }
                    if (shouldSendMail)
                    {
                        /*
                         * 1. Purchase mail should be sent after we know the last 4 digits.
                         * 2. Notifications caused by callback payments carry the last 4 digits we need for purchase mail
                         * 3. When we pay using recurring payments functionality we already know the last 4 digits.
                         * 4. Hence, Only callback notifications should send purchase mail.
                         */
                        if (bIsPaymentSuccessful)
                        {
                            SendMailToEndUserAsync(sPSPReference, string.Empty, bIsPurchasedWithPreviewModule ? AdyenMailType.PurchaseWithPreviewModuleSuccess : AdyenMailType.PurchaseSuccess);
                        }
                        else
                        {
                            // payment failed.
                            SendMailToEndUserAsync(sPSPReference, string.Empty, AdyenMailType.PurchaseFail);
                        }

                    }
                    BillingDAL.Update_AdyenNotification(sPSPReference, true);
                }
            }
            else
            {
                // no data in adyen_transactions or billing_transactions or in relevant ca table
                // probably the callback hasn't finished yet.
                #region Logging
                log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + String.Concat("HandleAdyenAuthorisationEvent no record in adyen_transactions or billing_transactions. PSP Reference: ", sPSPReference));
                #endregion
            }

        }

        private void HandleAdyenCancelOrRefundEvent(string sPSPReference, bool? bIsSuccess)
        {
            bool bIsCancelOrRefundResultOfPreviewModule = false;
            int nNumOfCancelOrRefundAttempts = 0;
            string sOriginalPSPReference = string.Empty;
            if (BillingDAL.Get_DataForAdyenCancelOrRefund(sPSPReference, ref sOriginalPSPReference, ref nNumOfCancelOrRefundAttempts, ref bIsCancelOrRefundResultOfPreviewModule))
            {
                if (!bIsSuccess.HasValue)
                {
                    #region Logging
                    log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + String.Concat("No success value at HandleAdyenCancelOrRefundEvent ", "PSP Reference: ", sPSPReference));
                    #endregion
                }
                else
                {
                    bool bIsRefundSuccessful = bIsSuccess.Value;

                    if (bIsRefundSuccessful)
                    {
                        HandleCancelOrRefundSuccess(sPSPReference, bIsCancelOrRefundResultOfPreviewModule, sOriginalPSPReference);
                    }
                    else
                    {
                        // refund failed
                        HandleCancelOrRefundFail(sPSPReference, sOriginalPSPReference, bIsCancelOrRefundResultOfPreviewModule, nNumOfCancelOrRefundAttempts);
                    }
                }
                BillingDAL.Update_AdyenNotification(sPSPReference, true);
            }
            else
            {
                // no data in adyen_cancels_or_refunds table relevant for this psp reference
                #region Logging
                log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + string.Format("HandleAdyenCancelOrRefundEvent. No data found in adyen_cancels_or_refunds table. PSP Reference: {0}", sPSPReference));
                #endregion
            }
        }

        private void HandleCancelOrRefundSuccess(string sCancelOrRefundPSPReference, bool bIsCancelOrRefundReqResultOfPreviewModule, string sOriginalPSPReference)
        {
            BillingDAL.Update_AdyenCancelOrRefundRequestStatus(sCancelOrRefundPSPReference, (int)CancelOrRefundRequestStatus.Authorised);
            if (bIsCancelOrRefundReqResultOfPreviewModule)
            {
                SendMailToEndUserAsync(sOriginalPSPReference, sCancelOrRefundPSPReference, AdyenMailType.PreviewModuleCORSuccess);
            }
        }

        private void HandleCancelOrRefundFail(string sCancelOrRefundPSPReference, string sOriginalPSPReference, bool bIsCancelOrRefundResultOfPreviewModule, int nNumOfCancelOrRefundAttempts)
        {
            BillingDAL.Update_AdyenCancelOrRefundRequestStatus(sCancelOrRefundPSPReference, (int)CancelOrRefundRequestStatus.Refused);

            if (bIsCancelOrRefundResultOfPreviewModule)
            {
                int nBoundOfNumOrCancelOrRefundAttempts = Core.Billing.Utils.GetPreviewModuleNumOfCancelOrRefundAttempts();
                if (nNumOfCancelOrRefundAttempts < nBoundOfNumOrCancelOrRefundAttempts)
                {
                    // resend cancelorrefund request
                    string sSiteGuid = string.Empty;
                    string sCurrencyCode = string.Empty;
                    int nGroupID = 0;
                    long lPurchaseID = 0;
                    double dChargePrice = 0.0;
                    int nType = 0;
                    if (!BillingDAL.Get_DataForResendingAdyenCancelOrRefund(sCancelOrRefundPSPReference, ref sSiteGuid, ref nGroupID, ref lPurchaseID, ref nType, ref dChargePrice, ref sCurrencyCode))
                    {
                        // failed to retrieve data from database. just log. no need to avoid calling CancelOrRefundPayment since sending a request of type
                        // cancelOrRefund for Adyen requires only psp reference and merchant account.
                        #region Logging
                        log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + string.Format("HandleCancelOrRefundFail. Failed to retrieve data from adyen_cancels_or_refunds. COR PSP Ref: {0} , Original PSP Ref: {1}", sCancelOrRefundPSPReference, sOriginalPSPReference));
                        #endregion
                    }
                    if (!CancelOrRefundPayment(sOriginalPSPReference, dChargePrice, sCurrencyCode, nGroupID, sSiteGuid, lPurchaseID, nType, true, nNumOfCancelOrRefundAttempts))
                    {
                        // attempt to send cancel or refund failed.
                        #region Logging
                        log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + string.Format("HandleCancelOrRefundFail. Failed to send cancel or refund. Original PSP Ref: {0} , COR PSP Ref: {1} , Num of COR attempts: {2}", sOriginalPSPReference, sCancelOrRefundPSPReference, nNumOfCancelOrRefundAttempts));
                        #endregion
                    }
                }
                else
                {
                    // log fail
                    #region Logging
                    log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + string.Format("HandleCancelOrRefundFail. Exceeded higher bound of COR attempts. COR PSP Ref: {0} , Original PSP Ref: {1} , Is COR result of PM: {2} , Num of COR attempts: {3}", sCancelOrRefundPSPReference, sOriginalPSPReference, bIsCancelOrRefundResultOfPreviewModule.ToString().ToLower(), nNumOfCancelOrRefundAttempts));
                    #endregion
                }
            }
        }

        private bool CancelOrRefundPayment(string sOriginalPSPRef, double dChargePrice, string sCurrencyCode, int nGroupID, string sSiteGuid, long lPurchaseID, int nType, bool bIsCancelOrRefundResultOfPreviewModule, int nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne)
        {
            bool result = false;
            try
            {
                BaseDirectDebit t = null;
                Utils.GetBaseDirectDebitImpl(ref  t, nGroupID);
                result = t.CancelOrRefundPayment(sOriginalPSPRef, sSiteGuid, dChargePrice, sCurrencyCode, lPurchaseID, nType, bIsCancelOrRefundResultOfPreviewModule, nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne);
            }
            catch (Exception ex)
            {
                #region Logging
                log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + string.Format("Exception at CancelOrRefundPayment. Exception msg: {0} , PSP Ref: {1} , Site Guid: {2} , Group ID: {3} Purchase ID: {4} , ST: {5}", ex.Message, sOriginalPSPRef, sSiteGuid, nGroupID, lPurchaseID, ex.StackTrace));
                #endregion
            }

            return result;
        }

        private string GetStdLogMsg(string sPSPReference, string sEventCode, string sSuccess, string sLast4Digits, string sReason)
        {
            StringBuilder sb = new StringBuilder(String.Concat("PSP Reference: ", sPSPReference));
            sb.Append(String.Concat("Event Code: ", sEventCode));
            sb.Append(String.Concat("Success: ", sSuccess));
            sb.Append(String.Concat("Last 4 Digits: ", sLast4Digits));
            sb.Append(String.Concat("Reason: ", sReason));

            return sb.ToString();

        }

        private bool IsNotification()
        {
            return Request.Form != null && Request.Form.AllKeys != null && Request.Form.AllKeys.Length > 0;
        }
        #endregion

        #region Events
        /// <summary>
        /// Page loaf
        /// </summary>
        /// <param name="sender">onject sender</param>
        /// <param name="e">Event Args</param>
        protected void Page_Load(object sender, EventArgs e)
        {

            if (IsNotification())
            {
                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();

                string sPSPReference = string.Empty;
                string sEventCode = string.Empty;
                string sSuccess = string.Empty;
                string sLast4Digits = string.Empty;
                string sReason = string.Empty;

                GetAdyenNotificationData(ref sPSPReference, ref sEventCode, ref sSuccess, ref sLast4Digits, ref sReason);

                try
                {
                    log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + String.Concat("Entering Notification Try Block: ", GetStdLogMsg(sPSPReference, sEventCode, sSuccess, sLast4Digits, sReason)));

                    if (AdyenUtils.IsValidPSPReference(sPSPReference))
                    {

                        BillingDAL.Insert_AdyenNotification(sPSPReference, sEventCode, sSuccess, sLast4Digits, sReason);

                        AdyenEventCode aec = ConvertEventCode(sEventCode);
                        bool? bIsSuccess = AdyenUtils.ConvertAdyenSuccess(sSuccess);
                        switch (aec)
                        {
                            case AdyenEventCode.Authorisation:
                                {
                                    HandleAdyenAuthorisationEvent(sPSPReference, bIsSuccess, sLast4Digits, sReason);
                                    break;
                                }
                            case AdyenEventCode.Cancel:
                                goto case AdyenEventCode.CancelOrRefund;
                            case AdyenEventCode.Refund:
                                goto case AdyenEventCode.CancelOrRefund;
                            case AdyenEventCode.CancelOrRefund:
                                {
                                    HandleAdyenCancelOrRefundEvent(sPSPReference, bIsSuccess);
                                    break;
                                }
                            default:
                                {
                                    #region Logging
                                    log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + String.Concat("No relevant Adyen Event Code ", GetStdLogMsg(sPSPReference, sEventCode, sSuccess, sLast4Digits, sReason)));
                                    #endregion
                                    break;
                                }
                        }
                    }
                    else
                    {
                        // no valid psp reference
                        #region Logging
                        log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + string.Format("Invalid format of PSP Reference rcvd at Adyen Notification. Event Code: {0} , Success: {1} , Last 4 digits: {2} , Reason {3} , PSP Reference: {4}", sEventCode, sSuccess, sLast4Digits, sReason, sPSPReference));
                        #endregion
                    }


                }
                catch (Exception ex)
                {
                    // log here exception message + parameters in form
                    #region Logging
                    log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + String.Concat("Notification Catch Block Exception Msg: ", ex.Message, " ", GetStdLogMsg(sPSPReference, sEventCode, sSuccess, sLast4Digits, sReason), " ST: ", ex.StackTrace));
                    #endregion
                }
                finally
                {
                    /*
                     * If we don't send "[accepted]" back to Adyen, Adyen will:
                     * 1. Slow down the rate of sending us notifications AND
                     * 2. They won't send any notification other than this until we confirm this one.
                     */
                    Response.Write("[accepted]");
                }

                stopwatch.Stop();
                long elapsed = stopwatch.ElapsedMilliseconds;

                log.Debug(ADYEN_NOTIFICATION_LOG_HEADER + string.Format("Total time:{0}, PSPRef:{1}", elapsed, sPSPReference));
            }
            else
            {
                #region In case of a payment callback (initial callback)
                string skinCode = GetSafeValue("skinCode");
                try
                {
                    //In case of a payment callback (initial callback)
                    log.Debug(ADYEN_CALLBACK_LOG_HEADER + string.Format("Entering Adyen Callback try block. GET call parameters: {0}", Request.Url.ToString()));

                    #region initial callback Param
                    string merchantReference = GetSafeValue("merchantReference");
                    string shopperLocale = GetSafeValue("shopperLocale");
                    string paymentMethod = GetSafeValue("paymentMethod");
                    string authResult = GetSafeValue("authResult");

                    string pspReference = GetSafeValue("pspReference");
                    string merchantReturnData = GetSafeValue("merchantReturnData");
                    string merchantSig = GetSafeValue("merchantSig");
                    string baseRedirectUrl = string.Empty;
                    string sCustomData = string.Empty;
                    int groupID = 0;
                    int nBillingProvider = (int)eBillingProvider.Adyen;
                    int nBillingMethod = 6;
                    GetInitialCallbackData(skinCode, merchantReturnData, pspReference, ref groupID, ref baseRedirectUrl, ref sCustomData);
                    string loweredPaymentMethod = paymentMethod.ToLower();
                    if (loweredPaymentMethod.Equals("ideal"))
                    {
                        nBillingMethod = 5;
                    }
                    else if (loweredPaymentMethod.Equals("visa") || loweredPaymentMethod.Equals("mc"))
                    {
                        nBillingMethod = 20;
                        if (paymentMethod.Equals("mc"))
                        {
                            nBillingMethod = 21;
                            paymentMethod = "Master Card";
                        }
                    }
                    else if (loweredPaymentMethod.Equals("paypal"))
                    {
                        nBillingMethod = 3;
                    }
                    if (paymentMethod.Equals("directdebit_NL"))
                    {
                        paymentMethod = "InCasso";
                    }
                    bool purchaseSuccess = false;
                    #endregion

                    if (groupID > 0)
                    {
                        string trimmedAuthResult = authResult.Trim().ToLower();
                        if (trimmedAuthResult.Equals("cancelled"))
                        {
                            RedirectPage(baseRedirectUrl, skinCode, "Error", AdyenRedirectMessages.USER_CANCELLED, false, true, "User cancelled");
                        }
                        #region Reset callback parm
                        string price = string.Empty;
                        string currencyCode = string.Empty;
                        string sSiteGUID = string.Empty;
                        string assetID = string.Empty;
                        string ppvOrSub = string.Empty;
                        string sPrePaidID = string.Empty;
                        string smedia_file = string.Empty;
                        string sSubscriptionID = string.Empty;
                        string sCollectionID = string.Empty;
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
                        string sOED = string.Empty;
                        string sPreviewModuleID = string.Empty;
                        long lBillingTransactionID = 0;
                        #endregion
                        if ((trimmedAuthResult.StartsWith("pending") || trimmedAuthResult.StartsWith("authorised")))
                        {
                            if (!string.IsNullOrEmpty(sCustomData))
                            {
                                //Parse the custom data xml
                                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                                doc.LoadXml(sCustomData);
                                System.Xml.XmlNode theRequest = doc.FirstChild;

                                sType = GetSafeParValue(".", "type", ref theRequest);
                                sSiteGUID = GetSafeParValue("//u", "id", ref theRequest);
                                string email = string.Empty;
                                int domainId = 0;
                                if (!Utils.IsUserExist(sSiteGUID, groupID, ref email, ref domainId))
                                {
                                    log.ErrorFormat("User does not exist, userId: {0}", sSiteGUID);
                                    RedirectPage(baseRedirectUrl, skinCode, "Error", AdyenRedirectMessages.INVALID_ERROR, false, true, "user does not exist");
                                }
                                sSubscriptionID = GetSafeValue("s", ref theRequest);
                                sCollectionID = GetSafeValue("cID", ref theRequest);
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
                                sOED = GetSafeValue("oed", ref theRequest);
                                sPreviewModuleID = GetSafeValue("pm", ref theRequest);
                                if (string.IsNullOrEmpty(price))
                                    price = "0.0";
                                Int32 nPaymentNum = 0;
                                Int32 nNumberOfPayments = 0;
                                if (!string.IsNullOrEmpty(sPayNum))
                                    nPaymentNum = int.Parse(sPayNum);
                                if (!string.IsNullOrEmpty(sPayOutOf))
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
                                else if (sType == "col")
                                {
                                    nType = 4;
                                }

                                int adyenID = 0;

                                double dPrice = double.Parse(price);

                                string sNotificationEventCode = string.Empty;
                                string sNotificationAdyenSuccess = string.Empty;
                                string sNotificationLast4Digits = string.Empty;
                                string sNotificationAdyenReason = string.Empty;
                                bool bIsDeactivateBusinessModule = false;
                                string sReasonToWriteToDB = string.Empty;
                                bool bIsCallbackToSendMail = false;
                                bool bIsPurchaseWithPreviewModule = !string.IsNullOrEmpty(sPreviewModuleID);

                                lBillingTransactionID = Utils.InsertNewAdyenTransaction(groupID, sSiteGUID, sDigits, dPrice, scurrency, merchantReturnData, sCustomData, pspReference, authResult, string.Empty, string.Empty, sReasonToWriteToDB, string.Empty, 1, 1, 3, nBillingMethod, nBillingProvider, nType, ref adyenID, true, true);

                                if (lBillingTransactionID > 0)
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
                                            Utils.WriteUserLogAsync(groupID, sSiteGUID, "PPV purchase (CC): " + smedia_file);
                                            purchaseSuccess = HandlePPVTransaction(groupID, srelevantsub, smedia_file, sSiteGUID, paymentMethod, price, scurrency, sCustomData, sCountryCode, sLangCode,
                                                                                    sDevice, smnou, lBillingTransactionID, smaxusagemodulelifecycle, adyenID, sOED, pspReference, domainId);

                                            if (!string.IsNullOrEmpty(scouponcode))
                                            {
                                                HandleCouponUse(scouponcode, sSiteGUID, int.Parse(smedia_file), srelevantsub, 0, 0, groupID);
                                            }
                                            #endregion
                                            break;
                                        case "sp":
                                            #region Subscription Purchase
                                            Utils.WriteUserLogAsync(groupID, sSiteGUID, "Subscription purchase (CC): " + sSubscriptionID);
                                            long lPurchaseID = 0;
                                            purchaseSuccess = HandleSubscrptionTransaction(groupID, sSubscriptionID, sSiteGUID, paymentMethod, price, scurrency, sCustomData,
                                                sCountryCode, sLangCode, sDevice, smnou, sviewlifecyclesecs, isRecurringStr, smaxusagemodulelifecycle,
                                                lBillingTransactionID, adyenID, sOED, pspReference, sPreviewModuleID, ref lPurchaseID, domainId);
                                            if (bIsPurchaseWithPreviewModule)
                                                HandlePreviewModule(groupID, pspReference, dPrice, scurrency, sSiteGUID, lPurchaseID);
                                            if (!string.IsNullOrEmpty(scouponcode))
                                                HandleCouponUse(scouponcode, sSiteGUID, 0, sSubscriptionID, 0, 0, groupID);
                                            #endregion
                                            break;
                                        case "prepaid":
                                            #region Handle PrePaid Transaction
                                            Utils.WriteUserLogAsync(groupID, sSiteGUID, "Pre Paid purchase (CC): " + sPrePaidID);
                                            purchaseSuccess = HandlePrePaidTransaction(groupID, sPrePaidID, sSiteGUID, paymentMethod, price, sPPCreditValue, scurrency,
                                                sCustomData, sCountryCode, sLangCode, sDevice, smnou, smaxusagemodulelifecycle, lBillingTransactionID, adyenID, sOED, pspReference);

                                            if (!string.IsNullOrEmpty(scouponcode))
                                            {
                                                HandleCouponUse(scouponcode, sSiteGUID, 0, srelevantsub, int.Parse(sPrePaidID), 0, groupID);
                                            }
                                            #endregion
                                            break;
                                        case "col":
                                            #region Collection Purchase
                                            Utils.WriteUserLogAsync(groupID, sSiteGUID, "Collection purchase (CC): " + sCollectionID);
                                            long lColPurchaseID = 0;
                                            int nCollectionCode;
                                            int.TryParse(sCollectionID, out nCollectionCode);
                                            purchaseSuccess = HandleCollectionTransaction(groupID, sCollectionID, sSiteGUID, paymentMethod, price, scurrency, sCustomData, sCountryCode, sLangCode, sDevice,
                                                smnou, sviewlifecyclesecs, smaxusagemodulelifecycle, lBillingTransactionID, adyenID, sOED, pspReference, ref lColPurchaseID);
                                            if (!string.IsNullOrEmpty(scouponcode))
                                                HandleCouponUse(scouponcode, sSiteGUID, 0, string.Empty, 0, nCollectionCode, groupID);
                                            #endregion
                                            break;
                                        default:
                                            {
                                                #region Logging
                                                StringBuilder sb = new StringBuilder("Error. No valid business module found. ");
                                                sb.Append(String.Concat(" PSP Ref: ", pspReference));
                                                sb.Append(String.Concat(" Type: ", sType));
                                                sb.Append(String.Concat(" CD: ", sCustomData));
                                                log.Debug(ADYEN_CALLBACK_LOG_HEADER + sb.ToString());
                                                #endregion
                                                break;
                                            }
                                    }
                                    if (BillingDAL.Get_DataOfAdyenNotificationForAdyenCallback(pspReference, ref sNotificationEventCode, ref sNotificationAdyenSuccess, ref sNotificationLast4Digits, ref sNotificationAdyenReason))
                                    {
                                        HandleNotificationRcvdJustAfterCallbackWritten(pspReference, adyenID, lBillingTransactionID, 0, nType, sNotificationAdyenSuccess, sNotificationAdyenReason, sNotificationLast4Digits, ref bIsDeactivateBusinessModule, ref bIsCallbackToSendMail);
                                    }
                                    if (bIsCallbackToSendMail)
                                    {
                                        log.Debug(ADYEN_CALLBACK_LOG_HEADER + string.Format("Mail sent by Adyen Callback. PSP Ref: {0}", pspReference));
                                        if (bIsPurchaseWithPreviewModule)
                                        {
                                            SendMailToEndUserAsync(pspReference, string.Empty, AdyenMailType.PurchaseWithPreviewModuleSuccess);
                                        }
                                        else
                                        {
                                            SendMailToEndUserAsync(pspReference, string.Empty, !bIsDeactivateBusinessModule ? AdyenMailType.PurchaseSuccess : AdyenMailType.PurchaseFail);
                                        }
                                    }

                                    if (purchaseSuccess)
                                    {
                                        RedirectPage(baseRedirectUrl, skinCode, "OK", AdyenRedirectMessages.OK, false, false, string.Empty, pspReference, lBillingTransactionID.ToString());
                                    }
                                    else
                                    {
                                        RedirectPage(baseRedirectUrl, skinCode, "Error", AdyenRedirectMessages.ITEM_PURCHASED_ERROR, false, true, "Item already purchased");
                                    }
                                }
                                else
                                {
                                    RedirectPage(baseRedirectUrl, skinCode, "Error", AdyenRedirectMessages.ITEM_ALREADY_PURCHASED, false, true, "Item already purchased");
                                }
                            }
                            else
                            {
                                RedirectPage(baseRedirectUrl, skinCode, "Error", AdyenRedirectMessages.INVALID_ERROR, false, true, "Custom data is null");
                            }

                        }
                        else
                        {
                            if (authResult.ToLower().Equals("refused"))
                            {
                                RedirectPage(baseRedirectUrl, skinCode, "Error", AdyenRedirectMessages.REFUSED_TRANSACTION, false, true, "error");
                            }
                        }
                    }
                    else
                    {
                        RedirectPage(baseRedirectUrl, skinCode, "Error", AdyenRedirectMessages.INVALID_ACCOUNT, false, true, "Group id not found");
                    }

                }
                catch (Exception ex)
                {
                    #region Logging
                    log.Error(ADYEN_CALLBACK_LOG_HEADER + string.Format("Exception at Adyen Callback. Exception Message: {0} , GET call parameters: {1} , Stack trace: {2}", ex.Message, Request.Url.ToString(), ex.StackTrace), ex);
                    #endregion
                    RedirectPage(string.Empty, skinCode, "Error", AdyenRedirectMessages.INVALID_ERROR, false, true, "Exception");
                }
                #endregion
            } // end callback section
        }
        #endregion

        #region Methods

        private void GetInitialCallbackData(string skinCode, string merchantReturnData, string pspReference,
            ref int groupID, ref string baseRedirectUrl, ref string customDataXml)
        {
            long customDataID = 0;
            if (!Int64.TryParse(merchantReturnData, out customDataID) || customDataID < 1)
            {
                log.Debug(ADYEN_CALLBACK_LOG_HEADER + String.Concat("GetInitialCallbackData failed to extract CD ID. PSP Ref: ", pspReference, " MRD: ", merchantReturnData));
            }
            if (!BillingDAL.Get_InitialAdyenCallbackData(skinCode, customDataID, ref groupID, ref baseRedirectUrl, ref customDataXml) ||
                groupID < 1 || string.IsNullOrEmpty(baseRedirectUrl) || string.IsNullOrEmpty(customDataXml))
            {
                log.Debug(ADYEN_CALLBACK_LOG_HEADER + String.Concat("GetInitialCallbackData failed to grab data from DB. PSP Ref: ", pspReference, " MRD: ", merchantReturnData, "SC: ", skinCode, "G ID: ", groupID, " BRU: ", baseRedirectUrl, " CD XML: ", customDataXml));
            }

        }

        private string GetAdyenAuthResult(string sCallbackAuthResult, string sNotificationEventCode, string sNotificationAdyenSuccess)
        {
            string res = sCallbackAuthResult;
            if (string.IsNullOrEmpty(sNotificationEventCode) || string.IsNullOrEmpty(sNotificationAdyenSuccess))
                return sCallbackAuthResult;
            if (sNotificationEventCode.Trim().ToLower() == "authorisation")
            {
                string sNotificationAdyenSuccessTrimmedLowerCase = sNotificationAdyenSuccess.Trim().ToLower();
                if (sNotificationAdyenSuccessTrimmedLowerCase == "true")
                    res = ADYEN_PAYMENT_ACCEPTED;
                else
                {
                    if (sNotificationAdyenSuccessTrimmedLowerCase == "false")
                        res = ADYEN_PAYMENT_REFUSED;
                    else
                        res = sCallbackAuthResult;
                }
            }

            return res;
        }

        private void SendMailToEndUserAsync(string pspReference, string corPspReference, AdyenMailType mailType)
        {
            Task.Factory.StartNew(() =>
            {
                (new AdyenMailer(pspReference, corPspReference, mailType)).SendMail();
            });
        }

        private void HandleNotificationRcvdJustAfterCallbackWritten(string sPSPReference, long lIDInAdyenTransactions, long lIDInBillingTransactions, long lPurchaseID, int nPurchaseType, string sNotificationAdyenSuccess, string sNotificationAdyenReason, string sNotificationLast4Digits, ref bool bIsDeactivateBusinessModule, ref bool bIsCallbackToSendMail)
        {
            bool? bIsSuccess = AdyenUtils.ConvertAdyenSuccess(sNotificationAdyenSuccess);
            if (!bIsSuccess.HasValue)
            {
                #region Logging
                log.Debug(ADYEN_CALLBACK_LOG_HEADER + string.Format("Callback, HandleNotificationEcvdJustAfterCallbackWritten: No success result at sNotificationAdyenSuccess variable. sNotificationAdyenSuccess: {0}", sNotificationAdyenSuccess));
                #endregion
            }
            else
            {
                bIsCallbackToSendMail = true;
                bool bIsPaymentSuccessful = bIsSuccess.Value;
                if (bIsPaymentSuccessful)
                {
                    UpdateDataFromAdyenNotificationInDB(sPSPReference, ADYEN_PAYMENT_ACCEPTED, sNotificationAdyenReason, nPurchaseType, lIDInAdyenTransactions, sNotificationLast4Digits, lIDInBillingTransactions);
                    bIsDeactivateBusinessModule = false;
                }
                else
                {
                    UpdateDataFromAdyenNotificationInDB(sPSPReference, ADYEN_PAYMENT_REFUSED, sNotificationAdyenReason, nPurchaseType, lIDInAdyenTransactions, sNotificationLast4Digits, lIDInBillingTransactions);
                    bIsDeactivateBusinessModule = true;
                }
            }

            BillingDAL.Update_AdyenNotification(sPSPReference, true);

        }


        /// <summary>
        ///  Redirect a client current a new page. Specifies SkinCode to excecute the redirect page.
        /// </summary>
        /// <param name="skinCode">Skin Code</param>
        /// <param name="status">Indicates wether redirect paramter status value.</param>
        /// <param name="description">Indicates wether redirect paramter description value</param>
        /// <param name="endResponse">Indicates whether execution of the current page should terminate.</param>
        /// <param name="enableLog">Optional,Enable write log, by default false.</param>
        /// <param name="logMessage">Optional,description write log, by default string empty.</param>
        protected void RedirectPage(string baseRedirectUrl, string skinCode, string status, string description, bool endResponse, bool enableLog = false, string logMessage = "", string pspRef = "0", string billingID = "0")
        {
            if (string.IsNullOrEmpty(baseRedirectUrl))
            {
                baseRedirectUrl = ODBCWrapper.Utils.GetTableSingleVal("adyen_group_parameters", "BASE_REDIRECT_URL", "skin_code", "=", skinCode, 3600).ToString();
            }
            if (enableLog)
            {

                log.Debug("Adyen Error " + string.Format("{0} : {1}", logMessage, Request.Url.ToString()));
            }

            Response.Redirect(string.Format("{0}?status={1}&desc={2}&bid={3}&psp={4}", baseRedirectUrl, Server.UrlEncode(status), Server.UrlEncode(description), billingID, pspRef), endResponse);
        }

        /// <summary>
        /// Refund payment using the suitable BaseDirectDebit
        /// according to Utils.GetBaseDirectDebitImpl()
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="sPSPReference"></param>
        /// <param name="dChargePrice"></param>
        /// <param name="sCurrencyCode"></param>
        /// <returns></returns>
        private bool RefundPayment(int nGroupID, string sPSPReference, double dChargePrice, string sCurrencyCode, string sSiteGuid, long lPurchaseID, int nType, int nHowManynHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne)
        {
            bool result = false;
            try
            {
                BaseDirectDebit t = null;
                Utils.GetBaseDirectDebitImpl(ref  t, nGroupID);
                result = t.RefundPayment(sPSPReference, sSiteGuid, nGroupID, dChargePrice, sCurrencyCode, lPurchaseID, nType, nHowManynHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne);
            }
            catch (Exception ex)
            {
                log.Error("AdyenCallbacks RefundPayment() Error " + string.Format("Error on RefundPayment(), group id:{0}, pspReference:{1}, chargePrice:{2}, currencyCode:{3}, errorMessage:{4}", nGroupID, sPSPReference, dChargePrice, sCurrencyCode, ex.ToString()), ex);
            }

            return result;
        }

        /// <summary>
        /// Get User PrePaid Amount
        /// </summary>
        /// <param name="sSiteGUID"></param>
        /// <param name="sCurrencyCode"></param>
        /// <returns></returns>
        protected double GetUserPrePaidAmount(string sSiteGUID, string sCurrencyCode)
        {
            double retVal = 0.0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
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
        /// Get Safe Value
        /// </summary>
        /// <param name="sQueryKey"></param>
        /// <returns></returns>
        protected string GetSafeValue(string sQueryKey)
        {
            if (String.IsNullOrEmpty(Request.QueryString[sQueryKey]))
                return "";
            return Request.QueryString[sQueryKey].ToString();
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
        /// Handle Coupon Use
        /// </summary>
        /// <param name="sCouponCode"></param>
        /// <param name="sSiteGUID"></param>
        /// <param name="nMediaFileID"></param>
        /// <param name="sSubCode"></param>
        /// <param name="nGroupID"></param>
        protected void HandleCouponUse(string sCouponCode, string sSiteGUID, int nMediaFileID, string sSubCode, int nPrePaidCode, int nCollectionCode, int nGroupID)
        {
            int couponID = 0;
            ODBCWrapper.DataSetSelectQuery couponSelectQuery = null;
            ODBCWrapper.DirectQuery directQuery = null;
            try
            {
                couponSelectQuery = new ODBCWrapper.DataSetSelectQuery();
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

                if (couponID > 0)
                {
                    int nSubCode = 0;
                    if (!string.IsNullOrEmpty(sSubCode))
                    {
                        nSubCode = int.Parse(sSubCode);
                    }
                    directQuery = new ODBCWrapper.DirectQuery();
                    directQuery.SetConnectionKey("pricing_connection");
                    directQuery += "update coupons set USE_COUNT=USE_COUNT+1, LAST_USED_DATE=getdate() where ";
                    directQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", couponID);
                    directQuery.Execute();

                    PricingDAL.Insert_NewCouponUse(sSiteGUID, couponID, nGroupID, nMediaFileID, nSubCode, nCollectionCode, nPrePaidCode);
                }
            }
            finally
            {
                if (couponSelectQuery != null)
                {
                    couponSelectQuery.Finish();
                }
                if (directQuery != null)
                {
                    directQuery.Finish();
                }
            }
        }

        /// <summary>
        /// Update Adyen PPV Purchase ID
        /// </summary>
        /// <param name="adyenID"></param>
        /// <param name="ppvID"></param>
        protected void UpdateAdyenPPVPurchaseID(int adyenID, int ppvID)
        {
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {
                updateQuery = new ODBCWrapper.UpdateQuery("adyen_transactions");
                updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_id", "=", ppvID);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
                updateQuery += "where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", adyenID);
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
        /// Handle PrePaid Transaction
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="sPrePaidID"></param>
        /// <param name="sSiteGUID"></param>
        /// <param name="paymentMethod"></param>
        /// <param name="price"></param>
        /// <param name="sPPCreditValue"></param>
        /// <param name="scurrency"></param>
        /// <param name="sCustomData"></param>
        /// <param name="sCountryCode"></param>
        /// <param name="sLangCode"></param>
        /// <param name="sDevice"></param>
        /// <param name="smnou"></param>
        /// <param name="smaxusagemodulelifecycle"></param>
        /// <param name="lBillingTransactionID"></param>
        /// <param name="adyenID"></param>
        /// <returns></returns>
        protected bool HandlePrePaidTransaction(int groupID, string sPrePaidID, string sSiteGUID, string paymentMethod,
            string price, string sPPCreditValue, string scurrency, string sCustomData, string sCountryCode, string sLangCode, string sDevice,
            string smnou, string smaxusagemodulelifecycle, long lBillingTransactionID, int adyenID, string sOverrideEndDate, string sPSPReference)
        {

            bool retVal = false;
            ODBCWrapper.InsertQuery insertQuery = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            ODBCWrapper.InsertQuery ppInsertQuery = null;
            ODBCWrapper.UpdateQuery updateQuery1 = null;
            try
            {
                double userPPVal = GetUserPrePaidAmount(sSiteGUID, scurrency);



                insertQuery = new ODBCWrapper.InsertQuery("pre_paid_purchases");
                insertQuery.SetConnectionKey("CA_CONNECTION_STRING");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);

                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("pre_paid_module_id", "=", int.Parse(sPrePaidID));
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", double.Parse(price));
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", scurrency);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("total_amount", "=", double.Parse(sPPCreditValue));
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("amount_used", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOM_DATA", "=", sCustomData);
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
                DateTime dtToWriteToDB = DateTime.UtcNow;
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "=", dtToWriteToDB);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", dtToWriteToDB);
                DateTime d = DateTime.MaxValue;
                if (!string.IsNullOrEmpty(sOverrideEndDate))
                {
                    try
                    {
                        d = DateTime.ParseExact(sOverrideEndDate, "dd/MM/yyyy", null);
                    }
                    catch
                    {
                    }
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);

                }
                else
                {
                    if (smaxusagemodulelifecycle != "")
                    {
                        d = Utils.GetEndDateTime(DateTime.UtcNow, int.Parse(smaxusagemodulelifecycle));
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", d);
                    }
                }

                insertQuery.Execute();

                Int32 nPurchaseID = 0;

                selectQuery = new ODBCWrapper.DataSetSelectQuery();
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

                if (adyenID != 0)
                {
                    UpdateAdyenPPVPurchaseID(adyenID, nPurchaseID);
                }

                ppInsertQuery = new ODBCWrapper.InsertQuery("pre_paid_uses");
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
                dtToWriteToDB = DateTime.UtcNow;
                ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "=", dtToWriteToDB);
                ppInsertQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", dtToWriteToDB);

                ppInsertQuery.Execute();
                //Should update the PURCHASE_ID

                if (lBillingTransactionID != 0 && nPurchaseID != 0)
                {
                    updateQuery1 = new ODBCWrapper.UpdateQuery("billing_transactions");
                    updateQuery1.SetConnectionKey("MAIN_CONNECTION_STRING");
                    updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                    updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
                    updateQuery1 += "where";
                    updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", lBillingTransactionID);
                    updateQuery1.Execute();

                    retVal = true;
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
                if (ppInsertQuery != null)
                {
                    ppInsertQuery.Finish();
                }
                if (updateQuery1 != null)
                {
                    updateQuery1.Finish();
                }
            }

            return retVal;
        }
        /// <summary>
        /// Handle Campaign Use
        /// </summary>
        /// <param name="campaignID"></param>
        /// <param name="siteGuid"></param>
        /// <param name="maxNumOfUses"></param>
        /// <param name="maxLifeCycle"></param>
        protected void HandleCampaignUse(int campaignID, string siteGuid, int maxNumOfUses, string maxLifeCycle)
        {
            ODBCWrapper.DataSetInsertQuery insertQuery = null;
            try
            {
                insertQuery = new ODBCWrapper.DataSetInsertQuery("campaigns_uses");
                insertQuery.SetConnectionKey("ca_connection_string");
                DateTime d = Utils.GetEndDateTime(DateTime.UtcNow, int.Parse(maxLifeCycle));
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("campaign_id", "=", campaignID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("site_guid", "=", int.Parse(siteGuid));
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("num_of_uses", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("max_num_of_uses", "=", maxNumOfUses);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", "=", d);
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
        /// Handle Collection Transaction
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="sCollectionID"></param>
        /// <param name="sSiteGUID"></param>
        /// <param name="paymentMethod"></param>
        /// <param name="price"></param>
        /// <param name="scurrency"></param>
        /// <param name="sCustomData"></param>
        /// <param name="sCountryCode"></param>
        /// <param name="sLangCode"></param>
        /// <param name="sDevice"></param>
        /// <param name="smnou"></param>
        /// <param name="smaxusagemodulelifecycle"></param>
        /// <param name="lBillingTransactionID"></param>
        /// <param name="adyenID"></param>
        /// <returns></returns>
        protected bool HandleCollectionTransaction(int groupID, string sCollectionID, string sSiteGUID, string paymentMethod,
            string price, string scurrency, string sCustomData, string sCountryCode, string sLangCode, string sDevice,
            string smnou, string sviewlifecyclesecs, string smaxusagemodulelifecycle, long lBillingTransactionID, int adyenID, string sOverrideEndDate,
            string sPSPReference, ref long lPurchaseID)
        {
            bool retVal = false;
            ODBCWrapper.UpdateQuery updateQuery = null;
            ODBCWrapper.InsertQuery insertQuery = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            ODBCWrapper.UpdateQuery updateQuery1 = null;
            try
            {
                double dPriceToWriteToDatabase = double.Parse(price);
                updateQuery = new ODBCWrapper.UpdateQuery("collections_purchases");
                updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", 0);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("COLLECCTION_CODE", "=", sCollectionID);
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                updateQuery.Execute();


                insertQuery = new ODBCWrapper.InsertQuery("collections_purchases");
                insertQuery.SetConnectionKey("CA_CONNECTION_STRING");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COLLECTION_CODE", "=", sCollectionID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPriceToWriteToDatabase);
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
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", lBillingTransactionID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                DateTime dtToWriteToDB = DateTime.UtcNow;
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "=", dtToWriteToDB);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", dtToWriteToDB);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("start_date", "=", dtToWriteToDB);
                DateTime dtCalculatedEndDate = CalcSubscriptionEndDate(groupID, sOverrideEndDate, smaxusagemodulelifecycle, string.Empty);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dtCalculatedEndDate);
                insertQuery.Execute();

                Int32 nPurchaseID = 0;

                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += " select id from collections_purchases where ";
                selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COLLECTION_CODE", "=", sCollectionID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPriceToWriteToDatabase);
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
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dtCalculatedEndDate);
                selectQuery += "order by id desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nPurchaseID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }

                if (adyenID != 0)
                {
                    UpdateAdyenPPVPurchaseID(adyenID, nPurchaseID);
                }

                //Should update the PURCHASE_ID

                if (lBillingTransactionID != 0 && nPurchaseID != 0)
                {
                    updateQuery1 = new ODBCWrapper.UpdateQuery("billing_transactions");
                    updateQuery1.SetConnectionKey("MAIN_CONNECTION_STRING");
                    updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                    updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
                    updateQuery1 += "where";
                    updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", lBillingTransactionID);
                    updateQuery1.Execute();

                    lPurchaseID = nPurchaseID;

                    retVal = true;
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
                if (updateQuery1 != null)
                {
                    updateQuery1.Finish();
                }
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
            }

            return retVal;
        }
        /// <summary>
        /// Handle Subscrption Transaction
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="sSubscriptionID"></param>
        /// <param name="sSiteGUID"></param>
        /// <param name="paymentMethod"></param>
        /// <param name="price"></param>
        /// <param name="scurrency"></param>
        /// <param name="sCustomData"></param>
        /// <param name="sCountryCode"></param>
        /// <param name="sLangCode"></param>
        /// <param name="sDevice"></param>
        /// <param name="smnou"></param>
        /// <param name="sviewlifecyclesecs"></param>
        /// <param name="isRecurringStr"></param>
        /// <param name="smaxusagemodulelifecycle"></param>
        /// <param name="lBillingTransactionID"></param>
        /// <param name="adyenID"></param>
        /// <returns></returns>
        protected bool HandleSubscrptionTransaction(int groupID, string sSubscriptionID, string sSiteGUID, string paymentMethod, string price, string scurrency,
            string sCustomData, string sCountryCode, string sLangCode, string sDevice, string smnou, string sviewlifecyclesecs, string isRecurringStr, string smaxusagemodulelifecycle,
            long lBillingTransactionID, int adyenID, string sOverrideEndDate, string sPSPReference, string sPreviewModuleID, ref long lPurchaseID, int domainId)
        {
            bool retVal = false;
            ODBCWrapper.UpdateQuery updateQuery1 = null;
            ODBCWrapper.UpdateQuery updateQuery = null;
            ODBCWrapper.InsertQuery insertQuery = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                long lPreviewModuleID = 0;
                if (!string.IsNullOrEmpty(sPreviewModuleID))
                    Int64.TryParse(sPreviewModuleID, out lPreviewModuleID);
                double dPriceToWriteToDatabase = lPreviewModuleID > 0 ? 0.0 : double.Parse(price);
                updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
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

                DateTime dtCalculatedEndDate = CalcSubscriptionEndDate(groupID, sOverrideEndDate, smaxusagemodulelifecycle, sPreviewModuleID);
                long purchaseId = ConditionalAccessDAL.Insert_NewMPPPurchase(groupID, sSubscriptionID, sSiteGUID, dPriceToWriteToDatabase, scurrency, sCustomData, sCountryCode, sLangCode, sDevice,
                                                                            !string.IsNullOrEmpty(smnou) ? int.Parse(smnou) : 0, !string.IsNullOrEmpty(sviewlifecyclesecs) ? int.Parse(sviewlifecyclesecs) : 0,
                                                                            !string.IsNullOrEmpty(isRecurringStr) ? bool.Parse(isRecurringStr) : false, lBillingTransactionID, lPreviewModuleID, DateTime.UtcNow,
                                                                            dtCalculatedEndDate, DateTime.UtcNow, "CA_CONNECTION_STRING", domainId);

                if (adyenID != 0)
                {
                    UpdateAdyenPPVPurchaseID(adyenID, (int)purchaseId);
                }

                string invalidationKey = LayeredCacheKeys.GetPurchaseInvalidationKey(domainId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key on HandleSubscrptionTransaction key = {0}", invalidationKey);
                }

                //Should update the PURCHASE_ID

                if (lBillingTransactionID != 0 && purchaseId != 0)
                {
                    updateQuery1 = new ODBCWrapper.UpdateQuery("billing_transactions");
                    updateQuery1.SetConnectionKey("MAIN_CONNECTION_STRING");
                    updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", purchaseId);
                    updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
                    updateQuery1 += "where";
                    updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", lBillingTransactionID);
                    updateQuery1.Execute();

                    lPurchaseID = purchaseId;



                    retVal = true;
                }
            }
            finally
            {
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
                if (updateQuery1 != null)
                {
                    updateQuery1.Finish();
                }
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
            }

            return retVal;
        }


        private DateTime CalcSubscriptionEndDate(int nGroupID, string sOverrideEndDate, string sMaxUsageModuleLifeCycle, string sPreviewModuleID)
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
        /// Handle PPV Transaction
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="srelevantsub"></param>
        /// <param name="smedia_file"></param>
        /// <param name="sSiteGUID"></param>
        /// <param name="paymentMethod"></param>
        /// <param name="price"></param>
        /// <param name="scurrency"></param>
        /// <param name="sCustomData"></param>
        /// <param name="sCountryCode"></param>
        /// <param name="sLangCode"></param>
        /// <param name="sDevice"></param>
        /// <param name="smnou"></param>
        /// <param name="lBillingTransactionID"></param>
        /// <param name="smaxusagemodulelifecycle"></param>
        /// <param name="adyenID"></param>
        /// <returns></returns>
        protected bool HandlePPVTransaction(int groupID, string srelevantsub, string smedia_file, string sSiteGUID, string paymentMethod, string price, string scurrency, string sCustomData, string sCountryCode,
            string sLangCode, string sDevice, string smnou, long lBillingTransactionID, string smaxusagemodulelifecycle, int adyenID, string sOverrideEndDate, string sPSPReference, int domainId)
        {
            bool retVal = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            ODBCWrapper.UpdateQuery updateQuery = null;            
            try
            {
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

                string invalidationKey = LayeredCacheKeys.GetPurchaseInvalidationKey(domainId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key on HandlePPVTransaction key = {0}", invalidationKey);
                }

                if (adyenID != 0)
                {
                    UpdateAdyenPPVPurchaseID(adyenID, (int)purchaseId);
                }
                //Should update the PURCHASE_ID

                if (lBillingTransactionID != 0 && purchaseId != 0)
                {
                    updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                    updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", purchaseId);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
                    updateQuery += "where";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", lBillingTransactionID);
                    updateQuery.Execute();

                    retVal = true;

                }

                string sItemName = "";
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
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

            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
            }
            return retVal;
        }

        private void HandlePreviewModule(int nGroupID, string sPSPReference, double dPrice, string sCurrencyCode, string sSiteGuid, long lPurchaseID)
        {
            log.Debug(ADYEN_CALLBACK_LOG_HEADER + string.Format("Sending Preview Module Refund Request: GroupID: {0}, PSP Ref: {1}, Price: {2}, Currency Code: {3}", nGroupID, sPSPReference, dPrice, sCurrencyCode));
            if (CancelOrRefundPayment(sPSPReference, dPrice, sCurrencyCode, nGroupID, sSiteGuid, lPurchaseID, 2, true, 0))
                log.Debug(ADYEN_CALLBACK_LOG_HEADER + string.Format("Preview Module Refund Request Success: GroupID: {0}, PSP Ref: {1}, Price: {2}, Currency Code: {3}", nGroupID, sPSPReference, dPrice, sCurrencyCode));
            else
                log.Debug(ADYEN_CALLBACK_LOG_HEADER + string.Format("Preview Module Refund Request Failure: GroupID: {0}, PSP Ref: {1}, Price: {2}, Currency Code: {3}", nGroupID, sPSPReference, dPrice, sCurrencyCode));
        }

        #endregion

    }
}