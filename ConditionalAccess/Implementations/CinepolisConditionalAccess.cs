using DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using TVinciShared;
using KLogMonitor;
using System.Reflection;
using Pricing;

namespace ConditionalAccess
{
    public class CinepolisConditionalAccess : TvinciConditionalAccess
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        internal const string CINEPOLIS_CA_LOG_FILE_NAME = "CinepolisConditionalAccess";

        public CinepolisConditionalAccess(Int32 nGroupID)
            : base(nGroupID)
        {

        }

        public CinepolisConditionalAccess(Int32 nGroupID, string connKey)
            : base(nGroupID, connKey)
        {
        }

        public override int GetPPVCustomDataID(string sSiteGUID, double dPrice, string sCurrency,
            Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCouponCode, string sCampaignCode, string sPaymentMethod, string sUserIP,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sOverrideEndDate)
        {
            int nSistrategiaToken = 0;
            // write custom data to db
            int nCustomDataID = base.GetPPVCustomDataID(sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sCampaignCode, sPaymentMethod, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, sOverrideEndDate);
            // request token from cinepolis
            if (nCustomDataID == 0 || !TryGetCinepolisToken(sSiteGUID, sUserIP, nCustomDataID, dPrice, ref nSistrategiaToken))
                return 0;
            return nSistrategiaToken;
        }



        public override int GetBundleCustomDataID(string sSiteGUID, double dPrice,
            string sCurrency, string sSubscriptionCode, string sCampaignCode, string sCouponCode, string sPaymentMethod, string sUserIP,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sOverrideEnddate, string sPreviewModuleID, eBundleType bundleType)
        {
            int nSistrategiaToken = 0;
            // write custom data to db

            int nCustomDataID = base.GetBundleCustomDataID(sSiteGUID, dPrice, sCurrency, sSubscriptionCode, sCampaignCode, sCouponCode, sPaymentMethod, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, sOverrideEnddate, sPreviewModuleID, eBundleType.SUBSCRIPTION);
            // request token from cinepolis
            if (nCustomDataID == 0 || !TryGetCinepolisToken(sSiteGUID, sUserIP, nCustomDataID, dPrice, ref nSistrategiaToken))
                return 0;
            return nSistrategiaToken;
        }

        private bool TryGetCinepolisToken(string sSiteGuid, string sUserIP, int nCustomDataID, double dPrice, ref int nToken)
        {
            bool res = false;
            long lSiteGuid = 0;
            int nTempToken = 0;
            if (!Int64.TryParse(sSiteGuid, out lSiteGuid) || lSiteGuid == 0)
            {
                #region Logging
                log.Debug("TryGetCinepolisToken - " + GetTryGetCinepolisTokenStdErrMsg("Incorrect format of site guid. ", sSiteGuid, sUserIP, nCustomDataID, dPrice, null));
                #endregion
                return false;
            }
            string sUserEmail = string.Empty;
            if (!UsersDal.Get_UserEmailBySiteGuid(lSiteGuid, "users_connection", ref sUserEmail) || !TVinciShared.Mailer.IsEmailAddressValid(sUserEmail))
            {
                #region Logging
                log.Debug("TryGetCinepolisToken - " + GetTryGetCinepolisTokenStdErrMsg("Failed to retrieve user email. ", sSiteGuid, sUserIP, nCustomDataID, dPrice, null));
                #endregion
                return false;
            }

            List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>(6);
            lst.Add(new KeyValuePair<string, string>("user_id", sSiteGuid));
            lst.Add(new KeyValuePair<string, string>("user_email", sUserEmail));
            lst.Add(new KeyValuePair<string, string>("user_ipaddress", sUserIP));

            string securityHash = CalcSecurityHash();
            if (securityHash.Length == 0)
            {
                // Failed to extracted security hash from DB. Cinepolis will not process our request.
                #region Logging
                log.Debug("TryGetCinepolisToken - " + string.Format("Failed to extract security hash from DB. Site Guid: {0} , Custom Data ID: {1}", sSiteGuid, nCustomDataID));
                #endregion
            }
            lst.Add(new KeyValuePair<string, string>("sh", securityHash));
            lst.Add(new KeyValuePair<string, string>("custom_data_id", nCustomDataID + ""));
            lst.Add(new KeyValuePair<string, string>("ammt", dPrice + ""));

            string sRequestBody = TVinciShared.WS_Utils.BuildDelimiterSeperatedString(lst, "&", false, false);
            string sResultJSON = string.Empty;
            string sErrorMsg = string.Empty;
            string sAddress = Utils.GetValueFromConfig("CinepolisCustomDataTokenAddress");
            string sContentType = Utils.GetValueFromConfig("CinepolisPostRequestContentType");
            if (sAddress.Length == 0 || sContentType.Length == 0)
            {
                #region Logging
                log.Debug("TryGetCinepolisToken - " + GetTryGetCinepolisTokenStdErrMsg("No address or content type at CA config.", sSiteGuid, sUserIP, nCustomDataID, dPrice, null));
                #endregion
                return false;
            }
            if (TVinciShared.WS_Utils.TrySendHttpPostRequest(sAddress, sRequestBody, sContentType, Encoding.UTF8, ref sResultJSON, ref sErrorMsg))
            {
                // parse result json
                bool bIsParsingSuccessful = false;
                Dictionary<string, string> dict = TVinciShared.WS_Utils.TryParseJSONToDictionary(sResultJSON, (new string[4] { "status", "internal_code", "message", "transaccion_token" }).ToList(), ref bIsParsingSuccessful, ref sErrorMsg);

                if (dict.ContainsKey("status") && dict.ContainsKey("transaccion_token"))
                {
                    string sTransactionToken = dict["transaccion_token"] != null ? dict["transaccion_token"].Trim().ToLower() : string.Empty;
                    if (!string.IsNullOrEmpty(dict["status"]) && dict["status"].Trim().ToLower() == "ok" && sTransactionToken.Length > 0 && Int32.TryParse(sTransactionToken, out nTempToken) && nTempToken > 0)
                    {
                        nToken = nTempToken;
                        res = true;
                    }
                    else
                    {
                        // log we didn't have ok or transaction token is invalid.
                        #region Logging
                        log.Debug("TryGetCinepolisToken - " + GetTryGetCinepolisTokenStdErrMsg(string.Format("ok or transaccion_token in JSON corrupted. ok: {0} , transaccion_token: {1} , error msg: {2} , json: {3}", dict["status"] == null ? "null" : dict["status"], sTransactionToken, sErrorMsg, sResultJSON), sSiteGuid, sUserIP, nCustomDataID, dPrice, dict));
                        #endregion
                    }
                }
                else
                {
                    // log. json wasn't parsed properly
                    #region Logging
                    log.Debug("TryGetCinepolisToken - " + GetTryGetCinepolisTokenStdErrMsg(string.Format("JSON wasn't parsed properly. Error msg: {0} , json: {1}", sErrorMsg, sResultJSON), sSiteGuid, sUserIP, nCustomDataID, dPrice, dict));
                    #endregion
                }
            }
            else
            {
                // log failed to send request to cinepolis
                #region Logging
                log.Debug("TryGetCinepolisToken - " + GetTryGetCinepolisTokenStdErrMsg(string.Format("Failed to send request to Cinepolis. Address: {0} , error msg: {1} , request body: {2}", sAddress, sErrorMsg, sRequestBody), sSiteGuid, sUserIP, nCustomDataID, dPrice, null));
                #endregion
            }

            return res;
        }

        private string GetTryGetCinepolisTokenStdErrMsg(string sMsg, string sSiteGuid, string sUserIP, int nCustomDataID, double dPrice, Dictionary<string, string> jsonDict)
        {
            StringBuilder sb = new StringBuilder(String.Concat(sMsg, " . "));
            sb.Append(String.Concat("Site Guid: ", sSiteGuid));
            sb.Append(String.Concat(" User IP: ", sUserIP));
            sb.Append(String.Concat(" Custom Data ID: ", nCustomDataID));
            sb.Append(String.Concat(" Price: ", dPrice));
            if (jsonDict != null && jsonDict.Count > 0)
            {
                sb.Append(" JSON Dictionary: ");
                foreach (KeyValuePair<string, string> kvp in jsonDict)
                {
                    sb.Append(String.Concat(kvp.Key, ":", kvp.Value, " "));
                }
            }
            return sb.ToString();
        }

        private void HandleSendOperationConfirmToCinepolis(long lPurchaseID, long lBillingTransactionID, BillingItemsType bit,
            string sSiteGUID)
        {
            bool bIsSuccessRcvd = false;
            string sErrorMsg = string.Empty;
            int nInternalCode = 0;

            if (TrySendOperationConfirm(lPurchaseID, lBillingTransactionID, bit, ref bIsSuccessRcvd,
                    ref sErrorMsg, ref nInternalCode))
            {
                BillingDAL.Update_CinepolisConfirmationDataByBillingID(lBillingTransactionID,
                    (byte)(bIsSuccessRcvd ? CinepolisConfirmationStatus.Received : CinepolisConfirmationStatus.Failed),
                    nInternalCode, sErrorMsg, "BILLING_CONNECTION_STRING");
            }
            else
            {
                BillingDAL.Update_CinepolisConfirmationDataByBillingID(lBillingTransactionID,
                    (byte)CinepolisConfirmationStatus.Failed, nInternalCode,
                    sErrorMsg, "BILLING_CONNECTION_STRING");
                WriteToUserLog(sSiteGUID, string.Format("Operation confirm call to Cinepolis corrupted or was not sent. Purchase ID: {0} , Billing Transaction ID: {1}", lPurchaseID, lBillingTransactionID));
            }
        }

        private bool TrySendOperationConfirm(long lPurchaseID, long lBillingTransactionID, BillingItemsType bit,
            ref bool bIsSuccess, ref string sMessage, ref int nInternalCode)
        {
            bool res = false;

            string sAddress = Utils.GetValueFromConfig("CinepolisOperationConfirmAddress");
            string sContentType = Utils.GetValueFromConfig("CinepolisPostRequestContentType");
            if (sAddress.Length == 0 || sContentType.Length == 0)
            {
                // either address or content type retrieved from config is empty
                #region Logging
                log.Debug("TrySendOperationConfirm - " + GetTrySendOperationConfirmStdErrMsg(string.Format("address or content type is empty. Address: {0} , Content type: {1}", sAddress, sContentType), lPurchaseID, lBillingTransactionID, bit, null));
                #endregion
                return false;
            }

            List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>(3);
            lst.Add(new KeyValuePair<string, string>("tvinci_transaction_id", lBillingTransactionID + ""));
            lst.Add(new KeyValuePair<string, string>("tvinci_confirmation_id", String.Concat((int)bit, "_", lPurchaseID)));
            string securityHash = CalcSecurityHash();
            if (securityHash.Length == 0)
            {
                // Failed to extract security hash. Cinepolis will not process our request.
                #region Logging
                log.Debug("TrySendOperationConfirm - " + string.Format("Failed to extract security hash from DB. Purchase ID: {0} , Billing transaction ID: {1}", lPurchaseID, lBillingTransactionID));
                #endregion
            }
            lst.Add(new KeyValuePair<string, string>("sh", securityHash));

            string sRequestData = TVinciShared.WS_Utils.BuildDelimiterSeperatedString(lst, "&", false, false);
            string sResponseJSON = string.Empty;
            string sErrorMsg = string.Empty;

            if (TVinciShared.WS_Utils.TrySendHttpPostRequest(sAddress, sRequestData, sContentType, Encoding.UTF8, ref sResponseJSON,
                ref sErrorMsg))
            {
                bool bIsParsingSuccessful = false;
                Dictionary<string, string> dict = TVinciShared.WS_Utils.TryParseJSONToDictionary(sResponseJSON, (new string[3] { "status", "internal_code", "message" }).ToList(), ref bIsParsingSuccessful, ref sErrorMsg);
                if (dict.ContainsKey("status"))
                {
                    if (dict["status"] != null && dict["status"].Trim().ToLower() == "ok")
                    {
                        bIsSuccess = true;
                    }
                    else
                    {
                        bIsSuccess = false;
                    }
                    if (dict.ContainsKey("internal_code") && dict["internal_code"] != null)
                        Int32.TryParse(dict["internal_code"], out nInternalCode);
                    if (dict.ContainsKey("message") && dict["message"] != null)
                        sMessage = dict["message"];
                    res = true;
                }
                else
                {
                    // no status in json. 
                    res = false;
                    #region Logging
                    log.Debug("TrySendOperationConfirm - " + GetTrySendOperationConfirmStdErrMsg(string.Format("No status key in JSON. JSON: {0}", sResponseJSON), lPurchaseID, lBillingTransactionID, bit, dict));
                    #endregion
                }
            }
            else
            {
                // Failed to send request to cinepolis
                res = false;
                #region Logging
                log.Debug("TrySendOperationConfirm - " + GetTrySendOperationConfirmStdErrMsg(string.Format("Post request to Cinepolis failed. Address: {0} , Content type: {1} , Request data: {2}", sAddress, sContentType, sRequestData), lPurchaseID, lBillingTransactionID, bit, null));
                #endregion
            }

            return res;
        }

        private string GetTrySendOperationConfirmStdErrMsg(string sDescription, long lPurchaseID, long lBillingTransactionID,
            BillingItemsType bit, Dictionary<string, string> dict)
        {
            StringBuilder sb = new StringBuilder(String.Concat(sDescription, " , "));
            sb.Append(String.Concat("Purchase ID: ", lPurchaseID));
            sb.Append(String.Concat(" Billing Transaction ID: ", lBillingTransactionID));
            sb.Append(String.Concat(" Billing Item Type: ", bit.ToString()));

            if (dict != null && dict.Count > 0)
            {
                sb.Append(" JSON Dictionary: ");
                foreach (KeyValuePair<string, string> kvp in dict)
                {
                    sb.Append(String.Concat(kvp.Key, ":", kvp.Value, " "));
                }
            }

            return sb.ToString();
        }

        private string CalcSecurityHash()
        {
            string secret = string.Empty;
            if (!BillingDAL.Get_CinepolisSecret("BILLING_CONNECTION_STRING", ref secret))
                return string.Empty; // MD5 output is of length 16bytes (32 hexa chars), hence we can recognize failure by empty string
            return TVinciShared.HashUtils.GetMD5HashUTF8EncodingInHexaString(String.Concat(DateTime.UtcNow.ToString("yyyy-MM-dd"), secret));
        }

        protected override TvinciBilling.BillingResponse HandleBaseRenewMPPBillingCharge(string sSiteGuid, double dPrice, string sCurrency, string sUserIP, string sCustomData, int nPaymentNumber, int nRecPeriods, string sExtraParams, int nBillingMethod, long lPurchaseID, ConditionalAccess.eBillingProvider bp)
        {
            TvinciBilling.module bm = null;
            TvinciBilling.BillingResponse res = null;
            try
            {
                string sWSUsername = string.Empty;
                string sWSPass = string.Empty;

                InitializeBillingModule(ref bm, ref sWSUsername, ref sWSPass);

                res = bm.CC_DummyChargeUser(sWSUsername, sWSPass, sSiteGuid, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber, nRecPeriods, sExtraParams);
            }
            finally
            {
                #region Disposing
                if (bm != null)
                {
                    bm.Dispose();
                }
                #endregion
            }

            return res;
        }

        protected override bool HandleMPPRenewalBillingSuccess(string sSiteGUID, string sSubscriptionCode, DateTime dtCurrentEndDate, bool bIsPurchasedWithPreviewModule, long lPurchaseID, string sCurrency, double dPrice, int nPaymentNumber, string sBillingTransactionID, int nUsageModuleMaxVLC, bool bIsMPPRecurringInfinitely, int nNumOfRecPeriods)
        {
            bool bIsSuccess = false;
            int nInternalCode = 0;
            string sMessage = string.Empty;
            long lBillingTransactionID = 0;
            if (base.HandleMPPRenewalBillingSuccess(sSiteGUID, sSubscriptionCode, dtCurrentEndDate, bIsPurchasedWithPreviewModule, lPurchaseID, sCurrency, dPrice, nPaymentNumber, sBillingTransactionID, nUsageModuleMaxVLC, bIsMPPRecurringInfinitely, nNumOfRecPeriods))
            {
                Int64.TryParse(sBillingTransactionID, out lBillingTransactionID);
            }
            else
            {
                // billing transaction id is corrupted
                #region Logging
                WriteToUserLog(sSiteGUID, string.Format("Billing transaction ID is corrupted. Purchase ID: {0} , Billing transaction ID: {1} , MPP Code: {2} , Price: {3}", lPurchaseID, sBillingTransactionID, sSubscriptionCode, dPrice));
                log.Debug("HandleMPPRenewalBillingSuccess - " + string.Format("Corrupted billing transaction ID. Site Guid: {0} , Purchase ID: {1} , Billing transaction ID: {2} , MPP Code: {3}", sSiteGUID, lPurchaseID, sBillingTransactionID, sSubscriptionCode));
                #endregion
            }

            if (TrySendRenewalDoneToCinepolis(sSiteGUID, lBillingTransactionID, dPrice, ref bIsSuccess, ref nInternalCode, ref sMessage))
            {
                BillingDAL.Update_CinepolisConfirmationDataByBillingID(lBillingTransactionID, (byte)(bIsSuccess ? CinepolisConfirmationStatus.Received : CinepolisConfirmationStatus.Failed), nInternalCode, sMessage, "BILLING_CONNECTION_STRING");
            }
            else
            {
                BillingDAL.Update_CinepolisConfirmationDataByBillingID(lBillingTransactionID, (byte)CinepolisConfirmationStatus.Failed, nInternalCode, sMessage, "BILLING_CONNECTION_STRING");
            }

            return true;
        }

        private bool TrySendRenewalDoneToCinepolis(string sSiteGuid, long lBillingTransactionID, double dPrice,
            ref bool bIsSuccess, ref int nInternalCode, ref string sMessage)
        {
            bool res = true;

            string sAddress = Utils.GetValueFromConfig("CinepolisRenewalDoneAddress");
            string sContentType = Utils.GetValueFromConfig("CinepolisPostRequestContentType");

            if (sAddress.Length == 0 || sContentType.Length == 0)
            {
                #region Logging
                log.Debug("TrySendRenewalDoneToCinepolis - " + GetTrySendRenewalDoneStdErrMsg(string.Format("Address or content type is empty. Address: {0} , Content type: {1}", sAddress, sContentType), sSiteGuid, lBillingTransactionID, dPrice, null));
                #endregion
                return false;
            }
            List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>(4);
            lst.Add(new KeyValuePair<string, string>("user_id", sSiteGuid));
            string securityHash = CalcSecurityHash();
            if (securityHash.Length == 0)
            {
                // Failed to extract security hash from DB. Cinepolis will not process out request.
                #region Logging
                log.Debug("TrySendRenewalDoneToCinepolis - " + string.Format("Failed to extract security hash from DB. Site Guid: {0} , Billing transaction ID: {1}", sSiteGuid, lBillingTransactionID));
                #endregion
            }
            lst.Add(new KeyValuePair<string, string>("sh", securityHash));
            lst.Add(new KeyValuePair<string, string>("tvinci_transaction_id", lBillingTransactionID + ""));
            lst.Add(new KeyValuePair<string, string>("ammt", dPrice + ""));

            string sRequestData = TVinciShared.WS_Utils.BuildDelimiterSeperatedString(lst, "&", false, false);
            string sResponseJSON = string.Empty;
            string sErrorMsg = string.Empty;
            if (TVinciShared.WS_Utils.TrySendHttpPostRequest(sAddress, sRequestData, sContentType, Encoding.UTF8, ref sResponseJSON,
                ref sErrorMsg))
            {
                bool bIsParsingSuccessful = false;
                Dictionary<string, string> dict = TVinciShared.WS_Utils.TryParseJSONToDictionary(sResponseJSON, (new string[3] { "status", "internal_code", "message" }).ToList(), ref bIsParsingSuccessful, ref sErrorMsg);

                if (dict.ContainsKey("status") && dict["status"] != null)
                {
                    bIsSuccess = dict["status"].Trim().ToLower() == "ok";

                    if (dict.ContainsKey("internal_code") && dict["internal_code"] != null)
                        Int32.TryParse(dict["internal_code"], out nInternalCode);
                    if (dict.ContainsKey("message") && dict["message"] != null)
                        sMessage = dict["message"];
                }
                else
                {
                    // no status in json. log and return false.
                    res = false;
                    #region Logging
                    log.Debug("TrySendRenewalDoneToCinepolis - " + GetTrySendRenewalDoneStdErrMsg(string.Format("No status in JSON. JSON: {0}, Error extracted: {1}", sResponseJSON, sErrorMsg), sSiteGuid, lBillingTransactionID, dPrice, dict));
                    #endregion
                }

            }
            else
            {
                // failed to send request to cinepolis
                res = false;
                #region Logging
                log.Debug("TrySendRenewalDoneToCinepolis - " + GetTrySendRenewalDoneStdErrMsg(string.Format("Failed to send http post request to Cinepolis. Error msg: {0} , Address: {1} , Content type: {2} , Request data: {3}", sErrorMsg, sAddress, sContentType, sRequestData), sSiteGuid, lBillingTransactionID, dPrice, null));
                #endregion
            }
            return res;
        }

        private string GetTrySendRenewalDoneStdErrMsg(string sDesc, string sSiteGuid, long lBillingTransactionID, double dPrice,
            Dictionary<string, string> dict)
        {
            StringBuilder sb = new StringBuilder(String.Concat(sDesc, " . "));
            sb.Append(String.Concat(" Site Guid: ", sSiteGuid));
            sb.Append(String.Concat(" Billing transaction ID: ", lBillingTransactionID));
            sb.Append(String.Concat(" Price: ", dPrice));

            if (dict != null && dict.Count > 0)
            {
                sb.Append(" JSON Dictionary: ");
                foreach (KeyValuePair<string, string> kvp in dict)
                {
                    sb.Append(String.Concat(kvp.Key, ":", kvp.Value, " "));
                }
            }
            return sb.ToString();
        }

        private void HandleSendRenewlDoneToCinepolis(string sSiteGuid, long lBillingTransactionID, double dPrice)
        {
            bool bIsSuccessRcvd = false;
            string sMsg = string.Empty;
            int nInternalCode = 0;

            if (TrySendRenewalDoneToCinepolis(sSiteGuid, lBillingTransactionID, dPrice, ref bIsSuccessRcvd, ref nInternalCode,
                    ref sMsg))
            {
                BillingDAL.Update_CinepolisConfirmationDataByBillingID(lBillingTransactionID,
                    (byte)(bIsSuccessRcvd ? CinepolisConfirmationStatus.Received : CinepolisConfirmationStatus.Failed),
                    nInternalCode, sMsg, "BILLING_CONNECTION_STRING");
            }
            else
            {
                BillingDAL.Update_CinepolisConfirmationDataByBillingID(lBillingTransactionID,
                    (byte)CinepolisConfirmationStatus.Failed, nInternalCode,
                    sMsg, "BILLING_CONNECTION_STRING");
            }
        }

        protected override TvinciBilling.BillingResponse HandleCCChargeUser(string sWSUsername, string sWSPassword, string sSiteGuid, double dPrice, string sCurrency, string sUserIP, string sCustomData, int nPaymentNumber, int nNumOfPayments, string sExtraParams, string sPaymentMethodID, string sEncryptedCVV, bool bIsDummy, bool bIsEntitledToPreviewModule, ref TvinciBilling.module bm)
        {
            return bm.CC_ChargeUser(sWSUsername, sWSPassword, sSiteGuid, dPrice, sCurrency, sUserIP, sCustomData, 1, nNumOfPayments, sExtraParams, sPaymentMethodID, sEncryptedCVV);
        }

        protected override bool HandleChargeUserForSubscriptionBillingSuccess(string sWSUsername, string sWSPassword, string sSiteGUID, int domainID, Subscription theSub, 
            double dPrice, string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLanguageCode, 
            string sDeviceName, TvinciBilling.BillingResponse br, bool bIsEntitledToPreviewModule, string sSubscriptionCode, 
            string sCustomData, bool bIsRecurring, ref long lBillingTransactionID, ref long lPurchaseID, bool isDummy, ref TvinciBilling.module wsBillingService)
        {
            bool res = base.HandleChargeUserForSubscriptionBillingSuccess(sWSUsername, sWSPassword,sSiteGUID, domainID, theSub, dPrice, sCurrency,
                sCouponCode, sUserIP, sCountryCd, sLanguageCode, sDeviceName, br, bIsEntitledToPreviewModule, sSubscriptionCode,
                sCustomData, bIsRecurring, ref lBillingTransactionID, ref lPurchaseID, isDummy, ref wsBillingService);

            if (res && lPurchaseID > 0 && lBillingTransactionID > 0)
            {
                if (IsSendOperationConfirmToCinepolis(isDummy, dPrice))
                {
                    HandleSendOperationConfirmToCinepolis(lPurchaseID, lBillingTransactionID, BillingItemsType.Subscription, sSiteGUID);
                }
            }
            else
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Purchase ID or billing transaction id is zero. ");
                sb.Append(String.Concat("Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Billing transaction ID: ", lBillingTransactionID));
                sb.Append(String.Concat(" Purchase ID: ", lPurchaseID));
                sb.Append(String.Concat(" Subscription Code: ", sSubscriptionCode));
                sb.Append(String.Concat(" Coupon Code: ", sCouponCode));
                sb.Append(String.Concat(" Custom data: ", sCustomData));
                log.Debug("HandleChargeUserForSubscriptionBillingSuccess - " + sb.ToString());
                WriteToUserLog(sSiteGUID, string.Format("No billing transaction id or purchase id. Billing transaction ID: {0} , Purchase ID: {1}", lBillingTransactionID, lPurchaseID));
                #endregion
            }

            return res;
        }

        private bool IsSendOperationConfirmToCinepolis(bool isDummy, double price)
        {
            return !isDummy || price != 0d;
        }

         protected override bool HandleChargeUserForMediaFileBillingSuccess(string sWSUsername, string sWSPassword, string sSiteGUID,int domainID, Subscription relevantSub, 
            double dPrice, string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLanguageCode, string sDeviceName, 
            TvinciBilling.BillingResponse br, string sCustomData, PPVModule thePPVModule, long lMediaFileID,
            ref long lBillingTransactionID, ref long lPurchaseID, bool isDummy, ref TvinciBilling.module wsBillingService, string billingGuid = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            bool res = base.HandleChargeUserForMediaFileBillingSuccess(sWSUsername,sWSPassword, sSiteGUID, domainID, relevantSub, dPrice, sCurrency,
                sCouponCode, sUserIP, sCountryCd, sLanguageCode, sDeviceName, br, sCustomData, thePPVModule, lMediaFileID,
                ref lBillingTransactionID, ref lPurchaseID, isDummy, ref wsBillingService, billingGuid);

            if (res && lPurchaseID > 0 && lBillingTransactionID > 0)
            {
                if (IsSendOperationConfirmToCinepolis(isDummy, dPrice))
                {
                    HandleSendOperationConfirmToCinepolis(lPurchaseID, lBillingTransactionID, BillingItemsType.PPV, sSiteGUID);
                }
            }
            else
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Purchase ID or billing transaction id is zero. ");
                sb.Append(String.Concat("Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Billing transaction ID: ", lBillingTransactionID));
                sb.Append(String.Concat(" Purchase ID: ", lPurchaseID));
                sb.Append(String.Concat(" Media File ID: ", lMediaFileID));
                sb.Append(String.Concat(" Coupon Code: ", sCouponCode));
                sb.Append(String.Concat(" Custom data: ", sCustomData));
                log.Debug("HandleChargeUserForMediaFileBillingSuccess - " + sb.ToString());
                WriteToUserLog(sSiteGUID, string.Format("No billing transaction id or purchase id. Billing transaction ID: {0} , Purchase ID: {1}", lBillingTransactionID, lPurchaseID));
                #endregion
            }

            return res;
        }

        protected override bool IsTakePriceFromBundleFinalPrice(bool isDummy, Price p)
        {
            return false;
        }

        protected override double InitializePriceForBundlePurchase(double inputPrice, bool isDummy)
        {
            return isDummy ? 0d : inputPrice;
        }

        protected override bool IsTakePriceFromMediaFileFinalPrice(bool isDummy)
        {
            return !isDummy;
        }

    }
}
