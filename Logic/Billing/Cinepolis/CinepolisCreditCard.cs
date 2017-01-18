using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAL;
using System.Net;
using System.IO;
using TVinciShared;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Billing;

namespace Core.Billing
{
    public class CinepolisCreditCard : BaseCreditCard
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public CinepolisCreditCard(int nGroupID, string sPaymentMethodID, string sEncryptedCVV)
            : base(nGroupID, sPaymentMethodID, sEncryptedCVV)
        {

        }
        private const string CINEPOLIS_CREDIT_CARD_LOG_FILENAME = "CinepolisCreditCard";

        private bool IsDummyTransaction(double chargePrice)
        {
            return string.IsNullOrEmpty(m_sPaymentMethodID) && string.IsNullOrEmpty(m_sEncryptedCVV) && chargePrice == 0d;
        }

        public override BillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, int nPaymentNumber, int nNumberOfPayments, string sExtraParameters)
        {
            BillingResponse res = new BillingResponse();
            string sUserEmail = string.Empty;
            long lSiteGuid = 0, lCinepolisTransactionID = 0;
            string sSecurityHash = string.Empty;
            string sAssetID = string.Empty;
            Dictionary<string, string> oCustomDataDict = null;
            try
            {
                bool isDummy = IsDummyTransaction(dChargePrice);
                log.Debug("ChargeUser - " + string.Format("Entering CinepolisCreditCard ChargeUser try block. Site Guid: {0} , User IP: {1} , Custom data: {2} , IsDummy: {3}", sSiteGUID, sUserIP, sCustomData, isDummy));
                if (isDummy)
                {
                    BaseCreditCard cc = new CinepolisDummyCreditCard(m_nGroupID);
                    res = cc.ChargeUser(sSiteGUID, dChargePrice, sCurrencyCode, sUserIP, sCustomData, nPaymentNumber, nNumberOfPayments, CinepolisUtils.CINEPOLIS_DUMMY);
                }
                else
                {
                    if (!Utils.IsUserExist(sSiteGUID, m_nGroupID))
                    {
                        res.m_oStatus = BillingResponseStatus.UnKnownUser;
                        res.m_sStatusDescription = "Unknown or inactive user";
                        res.m_sRecieptCode = string.Empty;
                    }
                    else
                    {
                        lSiteGuid = Int64.Parse(sSiteGUID);
                        if (!UsersDal.Get_UserEmailBySiteGuid(lSiteGuid, "US_CONNECTION_STRING", ref sUserEmail) || !TVinciShared.Mailer.IsEmailAddressValid(sUserEmail))
                        {
                            res.m_oStatus = BillingResponseStatus.Fail;
                            res.m_sStatusDescription = "No user email";
                            res.m_sRecieptCode = string.Empty;
                            return res;
                        }

                        oCustomDataDict = Utils.GetCustomDataDictionary(sCustomData);
                        sAssetID = GetAssetID(oCustomDataDict);
                        if (sAssetID.Length == 0)
                            throw new Exception("AssetID is empty");


                        bool bIsSuccessRcvd = false;
                        int nInternalCode = 0;
                        string sKlicOperationID = string.Empty;
                        string sMessage = string.Empty;

                        if (TryChargeCinepolis(sSiteGUID, sUserEmail, sUserIP, sAssetID, dChargePrice + "", ref bIsSuccessRcvd,
                            ref sMessage, ref nInternalCode, ref sKlicOperationID))
                        {
                            // connection with cinepolis successful
                            long lCustomDataID = Utils.AddCustomData(sCustomData);
                            ItemType it = Utils.CinepolisConvertToItemType(oCustomDataDict[Constants.BUSINESS_MODULE_TYPE]);
                            int nType = (int)it;
                            long lBillingTransactionID = Utils.InsertNewCinepolisTransaction(m_nGroupID, lSiteGuid, dChargePrice,
                                sCurrencyCode, lCustomDataID, sCustomData, oCustomDataDict, string.Empty,
                                (byte)(bIsSuccessRcvd ? CinepolisTransactionStatus.Authorised : CinepolisTransactionStatus.Refused),
                                nPaymentNumber, nNumberOfPayments, (int)eBillingProvider.Cinepolis, 1, (int)eBillingProvider.Cinepolis, nType,
                                (byte)CinepolisConfirmationStatus.NotSentYet, sMessage, ref lCinepolisTransactionID, false);

                            res.m_oStatus = bIsSuccessRcvd ? BillingResponseStatus.Success : BillingResponseStatus.Fail;
                            res.m_sRecieptCode = lBillingTransactionID + "";
                            res.m_sStatusDescription = sMessage;

                            if (bIsSuccessRcvd)
                            {
                                CinepolisUtils.SendMail(it, oCustomDataDict, dChargePrice, m_nGroupID, CinepolisMailType.Purchase, lBillingTransactionID);
                            }
                        }
                        else
                        {
                            // failed to establish connection or parse cinepolis response
                            res.m_oStatus = BillingResponseStatus.Fail;
                            res.m_sStatusDescription = "Connection error";
                            res.m_sRecieptCode = string.Empty;
                        }



                    }
                } // end big else
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder(String.Concat("Exception occurred. ex msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Custom Data: ", sCustomData));
                sb.Append(String.Concat(" User IP: ", sUserIP));
                sb.Append(String.Concat(" Asset ID: ", sAssetID));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));
                log.Error("ChargeUser - " + sb.ToString(), ex);
                #endregion
                res.m_oStatus = BillingResponseStatus.Fail;
                res.m_sStatusDescription = "Exception occurred";
                res.m_sRecieptCode = string.Empty;
            }

            return res;
        }

        public override string GetClientCheckSum(string sUserIP, string sRandom)
        {
            return string.Empty;
        }

        public override string GetClientMerchantSig(string sParams)
        {
            return string.Empty;
        }


        private bool TryChargeCinepolis(string sSiteGuid, string sUserEmail, string sUserIP,
            string sAssetID, string sPrice, ref bool bIsSuccessRcvd, ref string sMessage,
            ref int nInternalCode, ref string sKlicOperationID)
        {
            bool res = false;
            string sAddress = Utils.GetWSURL("CinepolisChargeUserAddress");
            string sContentType = Utils.GetWSURL("CinepolisPostRequestContentType");
            if (sAddress.Length == 0 || sContentType.Length == 0)
            {
                // no valid data in config
                #region Logging
                log.Debug("TryChargeCinepolis - " + GetTryChargeCinepolisStdErrMsg(string.Format("No valid data in config. Address: {0} , Content type: {1}", sAddress, sContentType), sSiteGuid, sUserEmail, sUserIP, sAssetID, sPrice, null));
                #endregion
                return false;
            }

            List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>(8);
            lst.Add(new KeyValuePair<string, string>("user_id", sSiteGuid));
            lst.Add(new KeyValuePair<string, string>("payment_method_id", m_sPaymentMethodID));
            lst.Add(new KeyValuePair<string, string>("user_email", sUserEmail));
            lst.Add(new KeyValuePair<string, string>("user_ipaddress", sUserIP));
            lst.Add(new KeyValuePair<string, string>("sh", CinepolisUtils.CalcSecurityHash()));
            lst.Add(new KeyValuePair<string, string>("asset_id", sAssetID));
            lst.Add(new KeyValuePair<string, string>("ammt", sPrice));
            lst.Add(new KeyValuePair<string, string>("encrypted_cvv", m_sEncryptedCVV));

            string sResult = string.Empty;
            string sErrorMsg = string.Empty;
            string sRequestData = TVinciShared.WS_Utils.BuildDelimiterSeperatedString(lst, "&", false, false);
            if (TVinciShared.WS_Utils.TrySendHttpPostRequest(sAddress, sRequestData, sContentType, Encoding.UTF8,
                ref sResult, ref sErrorMsg))
            {
                bool bIsParsingSuccessful = false;
                Dictionary<string, string> dict = TVinciShared.WS_Utils.TryParseJSONToDictionary(sResult, (new string[4] { "status", "internal_code", "message", "klic_operation_id" }).ToList(), ref bIsParsingSuccessful, ref sErrorMsg);
                if (dict.ContainsKey("status") && dict["status"] != null)
                {
                    bIsSuccessRcvd = dict["status"].Trim().ToLower() == "ok";
                    if (dict.ContainsKey("internal_code") && dict["internal_code"] != null)
                        Int32.TryParse(dict["internal_code"].Trim(), out nInternalCode);
                    if (dict.ContainsKey("message") && dict["message"] != null)
                        sMessage = dict["message"];
                    if (dict.ContainsKey("klic_operation_id") && dict["klic_operation_id"] != null)
                        sKlicOperationID = dict["klic_operation_id"];

                    res = true;
                }
                else
                {
                    // log no success in json.
                    #region Logging
                    log.Debug("TryChargeCinepolis - "+ GetTryChargeCinepolisStdErrMsg(string.Format("No success key in JSON. JSON: {0} , Error msg: {1} ", sResult, sErrorMsg), sSiteGuid, sUserEmail, sUserIP, sAssetID, sPrice, dict));
                    #endregion
                    res = false;
                }
            }
            else
            {
                // log failed to contact cinepolis
                #region Logging
                log.Debug("TryChargeCinepolis - "+ GetTryChargeCinepolisStdErrMsg(string.Format("Failed to send reuqest to cinepolis. Error msg: {0} , value returned: {1}", sErrorMsg, sResult), sSiteGuid, sUserEmail, sUserIP, sAssetID, sPrice, null));
                #endregion
                res = false;
            }

            return res;

        }

        private string GetTryChargeCinepolisStdErrMsg(string sDesc, string sSiteGuid, string sUserEmail, string sUserIP,
            string sAssetID, string sPrice, Dictionary<string, string> dict)
        {
            StringBuilder sb = new StringBuilder(String.Concat(sDesc, " . "));
            sb.Append(String.Concat(" Site Guid: ", sSiteGuid));
            sb.Append(String.Concat(" User Email: ", sUserEmail));
            sb.Append(String.Concat(" User IP: ", sUserIP));
            sb.Append(String.Concat(" Asset ID: ", sAssetID));
            sb.Append(String.Concat(" Price: ", sPrice));

            if (dict != null && dict.Count > 0)
            {
                sb.Append(" JSON Dictionary ");
                foreach (KeyValuePair<string, string> kvp in dict)
                {
                    sb.Append(String.Concat(kvp.Key, ":", kvp.Value, " "));
                }
            }
            return sb.ToString();

        }

        private string GetAssetID(Dictionary<string, string> oCustomDataDict)
        {
            if (oCustomDataDict[Constants.BUSINESS_MODULE_TYPE].Trim().ToLower().Equals("pp"))
                return oCustomDataDict[Constants.PPV_MODULE];
            if (oCustomDataDict[Constants.BUSINESS_MODULE_TYPE].Trim().ToLower().Equals("sp"))
                return oCustomDataDict[Constants.SUBSCRIPTION_ID];
            return string.Empty;
        }

        public override bool UpdatePurchaseIDInBillingTable(long purchaseID, long billingRefTransactionID)
        {
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {
                updateQuery = new ODBCWrapper.UpdateQuery("cinepolis_transactions");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_id", "=", purchaseID);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", billingRefTransactionID);
                updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                updateQuery.Execute();
            }
            finally
            {
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
            }


            return true;

        }
    }
}
