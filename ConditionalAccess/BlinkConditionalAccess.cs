using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using ConditionalAccess.TvinciAPI;
using ConditionalAccess.TvinciBilling;
using DAL;
using TVinciShared;

namespace ConditionalAccess
{
    class BlinkConditionalAccess : TvinciConditionalAccess
    {
        private static readonly string UNREACHABLE_ERROR = "Unable to connect to the billing server";

        public BlinkConditionalAccess(int nGroupID)
            : base(nGroupID)
        {
        }

        public BlinkConditionalAccess(int nGroupID, string connKey)
            : base(nGroupID, connKey)
        {
        }

        /// <summary>
        /// Credit Card Charge User For Media File
        /// </summary>
        public override TvinciBilling.BillingResponse CC_ChargeUserForMediaFile(string sSiteGUID, double dPrice,
            string sCurrency, Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCouponCode,
            string sUserIP, string sExtraParameters,
            string sCountryCd, string sLANGUAGE_CODE, string sDeviceUDID, bool bDummy, string sPaymentMethodID,
            string sEncryptedCVV)
        {
            TvinciBilling.BillingResponse ret = new TvinciBilling.BillingResponse();

            try
            {
                if (string.IsNullOrEmpty(sExtraParameters))
                {
                    return 
                        base.CC_ChargeUserForMediaFile(sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters,
                                                    sCountryCd, sLANGUAGE_CODE, sDeviceUDID, bDummy, sPaymentMethodID, sEncryptedCVV);
                }

                //Logger.Logger.Log("CC_ChargeUserForMediaFile",
                //    string.Format(
                //        "Entering CC_ChargeUserForMediaFile try block. Site Guid: {0} , Media File ID: {1} , Media ID: {2} , Price: {3} , PPV Module Code: {4} , Coupon: {5} , User IP: {6} , UDID: {7}",
                //        sSiteGUID, nMediaFileID, nMediaID, dPrice, sPPVModuleCode, sCouponCode, sUserIP, sDeviceUDID),
                //    GetLogFilename());

                // Let's make it dummy for now...
                bool dummy = true;
                ret = base.CC_ChargeUserForMediaFile(sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters,
                    sCountryCd, sLANGUAGE_CODE, sDeviceUDID, dummy, sPaymentMethodID, sEncryptedCVV);

                if (ret.m_oStatus != TvinciBilling.BillingResponseStatus.Success)
                {
                    return ret;
                }

                string msisdn = sEncryptedCVV; // Account ID
                string token = sExtraParameters;
                string mediaName = GetMediaNameByFileID(nMediaFileID);

                if (string.IsNullOrEmpty(ret.m_sRecieptCode))
                {
                    ret.m_sRecieptCode = "0000000000";
                }

                SmartSunPaymentResponse res = ChargeSmartSun(ret.m_sRecieptCode.PadLeft(10, '0'), msisdn, dPrice, sCurrency, mediaName, token);

                if ((res == null) || (string.IsNullOrEmpty(res.amountTransaction.transactionOperationStatus)) ||
                    (!res.amountTransaction.transactionOperationStatus.ToLower().Equals("charged")))
                {
                    bool cancelledTransaction = ConditionalAccessDAL.CancelPPVPurchaseTransaction(sSiteGUID, nMediaFileID);
                    //bool cancelledTransaction = CancelTransaction(sSiteGUID, nMediaFileID, eTransactionType.PPV, m_nGroupID);

                    bool cancelled = cancelledTransaction && DAL.ConditionalAccessDAL.CancelTransaction(int.Parse(ret.m_sRecieptCode));
                    //bool canceled = canceledTransaction && DAL.ConditionalAccessDAL.CancelPpvPurchase(m_nGroupID, nPurchaseID, sSiteGUID, nMediaFileID, 0, 2);

                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                    ret.m_sStatusDescription = "Error: Failed to charge user via SmartSun";

                    if (res != null)
                    {
                        ret.m_sStatusDescription += string.Concat(" | Response: ", res.ToJSON());
                    }

                    //ret.m_sStatusDescription = "Error " + transNotificationRes.ErrorCode + ">" + transNotificationRes.ErrorMessage; //"User is not entitled for VOD purchasing";

                    WriteToUserLog(sSiteGUID,
                        "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " +
                        ret.m_sStatusDescription);
                }
                else
                {
                    ret.m_oStatus = TvinciBilling.BillingResponseStatus.Success;
                    ret.m_sExternalReceiptCode = res.amountTransaction.serverReferenceCode;
                    ret.m_sStatusDescription += string.Concat(" | Response: ", res.ToJSON());

                    try
                    {
                        SendPurchaseMail(sSiteGUID, mediaName, dPrice, sCurrency, int.Parse(ret.m_sRecieptCode));
                    }
                    catch (Exception ex)
                    {
                        WriteToUserLog(sSiteGUID,
                            "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " failed to send purchase mail: " + ex.ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                #region Logging

                StringBuilder sb = new StringBuilder(String.Concat("Exception at Eutelsat CC_ChargeUserForMediaFile. Msg: ", ex.Message));
                sb.Append(String.Concat("Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Media File ID: ", nMediaFileID));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" UDID: ", sDeviceUDID));
                sb.Append(String.Concat(" Currency: ", sCurrency));
                sb.Append(String.Concat(" Cntry CD: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" Trace: ", ex.StackTrace));

                Logger.Logger.Log("Exception", sb.ToString(), GetLogFilename());

                #endregion

                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                ret.m_sRecieptCode = string.Empty;
                ret.m_sStatusDescription = "Failed to purchase media";

                return ret;
            }

            return ret;
        }

        private void SendPurchaseMail(string sSiteGUID, string mediaName, double dPrice, string sCurrency, int billingTransactionID)
        {
            TvinciBilling.module bm = new TvinciBilling.module();
            string sIP = "1.1.1.1";
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;
            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "CC_ChargeUser", "billing", sIP, ref sWSUserName, ref sWSPass);
            string sWSURL = Utils.GetWSURL("billing_ws");
            if (sWSURL.Length > 0)
                bm.Url = sWSURL;

            bm.SendPurchaseMail(sWSUserName, sWSPass, sSiteGUID, "SmartSun", mediaName, billingTransactionID, dPrice.ToString(), sCurrency, "");
        }


        /// <summary>
        /// Credit Card Charge User For Bundle
        /// </summary>
        public override TvinciBilling.BillingResponse CC_ChargeUserForBundle(string sSiteGUID, double dPrice, string sCurrency, string sBundleCode, string sCouponCode, string sUserIP, string sExtraParams,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bDummy, string sPaymentMethodID, string sEncryptedCVV, eBundleType bundleType)
        {

            TvinciBilling.BillingResponse ret = new TvinciBilling.BillingResponse();

            if (string.IsNullOrEmpty(sExtraParams))
            {
                return
                    base.CC_BaseChargeUserForBundle(sSiteGUID, dPrice, sCurrency, sBundleCode, sCouponCode, sUserIP,
                    sExtraParams, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, bDummy, sPaymentMethodID, sEncryptedCVV, bundleType);
            }

            try
            {
                // Let's make it dummy for now...
                bool dummy = true;
                ret = base.CC_BaseChargeUserForBundle(sSiteGUID, dPrice, sCurrency, sBundleCode, sCouponCode, sUserIP,
                    sExtraParams, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, dummy, sPaymentMethodID, sEncryptedCVV, bundleType);

                if (ret.m_oStatus != TvinciBilling.BillingResponseStatus.Success)
                {
                    return ret;
                }

                int subID = int.Parse(sBundleCode);
                string msisdn = sEncryptedCVV; // Account ID
                string token = sExtraParams;
                string subName = GetSubscriptionName(subID);

                if (string.IsNullOrEmpty(ret.m_sRecieptCode))
                {
                    ret.m_sRecieptCode = "0000000000";
                }

                SmartSunPaymentResponse res = ChargeSmartSun(ret.m_sRecieptCode.PadLeft(10, '0'), msisdn, dPrice, sCurrency, subName, token);

                if ((res == null) || (string.IsNullOrEmpty(res.amountTransaction.transactionOperationStatus)) ||
                    (!res.amountTransaction.transactionOperationStatus.ToLower().Equals("charged")))
                {
                    bool cancelledTransaction = ConditionalAccessDAL.CancelSubscriptionPurchaseTransaction(sSiteGUID, subID);
                    //bool cancelledTransaction = CancelTransaction(sSiteGUID, subID, eTransactionType.Subscription, m_nGroupID);

                    bool cancelled = cancelledTransaction && DAL.ConditionalAccessDAL.CancelTransaction(int.Parse(ret.m_sRecieptCode));
                    //bool canceled = canceledTransaction && DAL.ConditionalAccessDAL.CancelPpvPurchase(m_nGroupID, nPurchaseID, sSiteGUID, nMediaFileID, 0, 2);

                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                    //ret.m_sRecieptCode = sReceipt;
                    ret.m_sStatusDescription = "Error: Failed to charge user via SmartSun";

                    if (res != null)
                    {
                        ret.m_sStatusDescription += string.Concat(" | Response: ", res.ToJSON());
                    }


                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + sBundleCode + " error returned: " + ret.m_sStatusDescription);
                }
                else
                {
                    ret.m_oStatus = TvinciBilling.BillingResponseStatus.Success;
                    ret.m_sExternalReceiptCode = res.amountTransaction.serverReferenceCode;
                    ret.m_sStatusDescription += string.Concat(" | Response: ", res.ToJSON());

                    try
                    {
                        SendPurchaseMail(sSiteGUID, subName, dPrice, sCurrency, int.Parse(ret.m_sRecieptCode));
                    }
                    catch (Exception ex)
                    {
                        WriteToUserLog(sSiteGUID,
                            "While trying to purchase subscription (CC): " + sBundleCode + " failed to send purchase mail: " + ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging

                StringBuilder sb =
                    new StringBuilder(String.Concat("Exception at Blink CC_ChargeUserForBundle. Msg: ", ex.Message));
                sb.Append(String.Concat("Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Sub ID: ", sBundleCode));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" UDID: ", sDEVICE_NAME));
                sb.Append(String.Concat(" Currency: ", sCurrency));
                sb.Append(String.Concat(" Cntry CD: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" Trace: ", ex.StackTrace));

                Logger.Logger.Log("Exception", sb.ToString(), GetLogFilename());

                #endregion

                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                ret.m_sRecieptCode = string.Empty;
                ret.m_sStatusDescription = "Failed to purchase subscription";

                return ret;
            }

            return ret;
        }


        protected SmartSunPaymentResponse ChargeSmartSun(string sTransactionID, string msisdn, double dPrice, string sCurrency, string description, string token)
        {
            SmartSunPaymentResponse res = new SmartSunPaymentResponse();

            try
            {
                //BlinkSmartSunPaymentURL: https://stg.apis.smart.com.ph:7443/OneAPI-Payment-REST-1.0/payment/tel%3A%2B{0}/transactions/amount 
                //BlinkSmartSunMerchantID: 01140
                //BlinkSmartSunServiceID: TSTOB

                string sWSURL = string.Format(Utils.GetWSURL("BlinkSmartSunPaymentURL"), msisdn);

                string strServiceMerchantID = TVinciShared.WS_Utils.GetTcmConfigValue("BlinkSmartSunServiceMerchantID");

                if (string.IsNullOrEmpty(strServiceMerchantID.Trim()))
                {
                    return null;
                }

                string[] serviceMerchantID = TVinciShared.WS_Utils.GetTcmConfigValue("BlinkSmartSunServiceMerchantID").Split(',');

                string serviceID = serviceMerchantID[0].Trim();
                string merchantID = serviceMerchantID[1].Trim();

                //string merchantID = TVinciShared.WS_Utils.GetTcmConfigValue("BlinkSmartSunMerchantID");
                //string serviceID = TVinciShared.WS_Utils.GetTcmConfigValue("BlinkSmartSunServiceID");

                if (string.IsNullOrEmpty(sWSURL) || string.IsNullOrEmpty(merchantID) || string.IsNullOrEmpty(serviceID))
                {
                    return null;
                }

                //endUserId=tel%3A%2B639989049181&transactionOperationStatus=Charged&description=Nihontosekaijuu&currency=PHP&amount=1.00
                //&clientCorrelator=6cb1a85ba797e742e7a0&onBehalfOf=OpBillingTest&purchaseCategoryCode=test:Scenarios
                //&channel=WAP&referenceCode=01017-1234567890&serviceID=MSC01

                StringBuilder sb = new StringBuilder();
                sb.Append(HttpUtility.UrlEncode("endUserId")).Append("=").Append(HttpUtility.UrlEncode("tel:+")).Append(HttpUtility.UrlEncode(msisdn)).Append("&")
                    .Append(HttpUtility.UrlEncode("transactionOperationStatus")).Append("=").Append(HttpUtility.UrlEncode("Charged")).Append("&")
                    .Append(HttpUtility.UrlEncode("description")).Append("=").Append(HttpUtility.UrlEncode(description)).Append("&")
                    .Append(HttpUtility.UrlEncode("currency")).Append("=").Append(HttpUtility.UrlEncode(sCurrency)).Append("&")
                    .Append(HttpUtility.UrlEncode("amount")).Append("=").Append(HttpUtility.UrlEncode(dPrice.ToString())).Append("&")
                    .Append(HttpUtility.UrlEncode("referenceCode")).Append("=").Append(HttpUtility.UrlEncode(merchantID)).Append("-").Append(HttpUtility.UrlEncode(sTransactionID)).Append("&")
                    .Append(HttpUtility.UrlEncode("serviceID")).Append("=").Append(HttpUtility.UrlEncode(serviceID));

                string postMessage = sb.ToString();

                //sb.Append("endUserId=tel%3A%2B").Append(msisdn)
                //.Append("&transactionOperationStatus=Charged")
                //.Append("&description=").Append(description)
                //.Append("&currency=").Append(sCurrency)
                //.Append("&amount=").Append(dPrice)
                //.Append("&referenceCode=").Append(merchantID).Append("-").Append(sTransactionID)
                //.Append("&serviceID=").Append(serviceID);


                //SmartSunChargeRequest request = new SmartSunChargeRequest();

                //request.request.transactionId = sTransactionID;
                //request.request.merchantId = merchantID;
                //request.request.accountDetails = new Accountdetails();
                //request.request.accountDetails.accountId = msisdn;
                //request.request.accountDetails.accountType = "MSISDN";
                //request.request.accountDetails.fundSource = "Mob";
                //request.request.amountDetails = new Amountdetails();
                //request.request.amountDetails.amount = dPrice.ToString();
                //request.request.amountDetails.currency = sCurrency;
                //request.request.serviceDetails = new Servicedetails();
                //request.request.serviceDetails.serviceID = "G000";

                //var jsonTransactionContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);

                //"transactionId": "1234567890", 
                //"merchantId": "MW00001", 
                //"accountDetails": { 
                //"accountId": "639183371234", 
                //"accountType": "MSISDN",
                //"fundSource": "Mob" 
                //}, 
                //"amountDetails": { 
                //"amount": "10.00", 
                //"currency": "PHP" 
                //}, 
                //"serviceDetails": { 
                //"serviceID" : "G0000", 

                Uri requestUri = null;
                bool isGoodUri = Uri.TryCreate(sWSURL, UriKind.Absolute, out requestUri) &&
                    (requestUri.Scheme == Uri.UriSchemeHttp || requestUri.Scheme == Uri.UriSchemeHttps);

                if (isGoodUri)
                {
                    string clientID = GetClientID(m_nGroupID);
                    string jsonRes = MakeJsonRequest(requestUri, token, clientID, postMessage);
                    //res = MakeJsonRequest(requestUri, token, jsonTransactionContent) as SmartSunChargeResponse;

                    if (string.IsNullOrEmpty(jsonRes))
                    {
                        res = new SmartSunPaymentResponse { amountTransaction = new Amounttransaction() };
                    }

                    SmartSunPaymentResponse objResponse = (SmartSunPaymentResponse)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonRes, typeof(SmartSunPaymentResponse));

                    res = objResponse;
                }

            }

            catch (Exception ex)
            {
                #region Logging

                //StringBuilder sb = new StringBuilder("Exception at ChargeSmartSun - ");
                //sb.Append(String.Concat("Household: ", sHouseholdUID));
                //sb.Append(String.Concat(" Asset ID: ", nAssetID));
                //sb.Append(String.Concat(" External Asset ID: ", sExternalAssetID));
                //sb.Append(String.Concat(" PPV Module: ", sPpvModuleCode));
                //sb.Append(String.Concat(" Coupon: ", sCouponCode));
                //sb.Append(String.Concat(" UDID: ", sDeviceUDID));
                //sb.Append(String.Concat(" Trans ID: ", nTransactionID));
                //sb.Append(String.Concat(" Trace: ", ex.StackTrace));

                Logger.Logger.Log("Exception", ex.ToString(), GetLogFilename());

                #endregion

                int errorCode = System.Runtime.InteropServices.Marshal.GetExceptionCode();
                res = new SmartSunPaymentResponse { amountTransaction = new Amounttransaction() };

                //res = new SmartSunChargeResponse()
                //{
                //    response = new Response()
                //    {
                //        transactionId = sTransactionID,

                //    }
                //};

            }

            return res;
        }

        protected string MakeJsonRequest(Uri requestUri, string bearerToken, string clientID, string jsonContent = "")
        {
            try
            {
                Dictionary<string, string> dHeaders = new Dictionary<string, string>() 
                {
                    //{ "Host", "beta-login.mediacorp.sg" },
                    {"Authorization", "Bearer " + bearerToken },
                    {"Client_id", clientID }
                };

                DateTime dNow = DateTime.UtcNow;
                ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                string profJson = TVinciShared.WS_Utils.SendXMLHttpReqWithHeaders(requestUri.OriginalString, jsonContent, dHeaders, "application/x-www-form-urlencoded");    //, "application/json", "", "", "", "", "post");
                double dTime = DateTime.UtcNow.Subtract(dNow).TotalMilliseconds;

                Logger.Logger.Log("MakeJsonRequest", string.Format("Response: {0}", profJson), GetLogFilename());

                //object objResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(profJson, typeof(SmartSunChargeResponse));

                return profJson;
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at MakeJsonRequest. ");
                sb.Append(String.Concat(" Msg: ", ex.Message));
                sb.Append(String.Concat(" Req URI: ", requestUri != null ? requestUri.OriginalString : "null"));
                sb.Append(String.Concat(" JSON: ", jsonContent));
                sb.Append(String.Concat(" Trace: ", ex.StackTrace));

                Logger.Logger.Log("MakeJsonRequest", sb.ToString(), GetLogFilename());
                #endregion
            }

            return string.Empty;
        }

        /// <summary>
        /// Get Client ID
        /// </summary>
        protected string GetClientID(int groupID)
        {
            string sRet = "";

            ODBCWrapper.DataSetSelectQuery selectQuery = null;

            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "SELECT ID, CLIENT_ID FROM GROUPS_OPERATORS WITH (NOLOCK) WHERE IS_ACTIVE=1 AND STATUS=1 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sRet = selectQuery.Table("query").DefaultView[0].Row["CLIENT_ID"].ToString();
                    }

                    if (!string.IsNullOrEmpty(sRet) && sRet.ToLower().Contains("client_id"))
                    {
                        //https://stg.authenticate.smart.com.ph/sps/SmartDevOAuth2/oauth20/authorize?response_type=code&client_id=BLINK01&scope=O_VL_chargeAmountAirtime&redirect_uri=https://www.blink-now.com/firstleglanding.aspx

                        string temp = sRet.Substring(sRet.IndexOf("client_id=") + 10);
                        sRet = temp.Substring(0, temp.IndexOf("&"));
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
            return sRet;
        }

        /// <summary>
        /// Get Client ID
        /// </summary>
        protected string GetMediaNameByFileID(int fileID)
        {
            string sRet = "";

            ODBCWrapper.DataSetSelectQuery selectQuery = null;

            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select m.NAME from media m with (nolock) inner join media_files mf with (nolock) on mf.MEDIA_ID = m.id where m.STATUS=1 and m.IS_ACTIVE=1 and mf.IS_ACTIVE=1 and mf.STATUS=1 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.ID", "=", fileID);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sRet = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
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

            return sRet;
        }

        private string GetSubscriptionName(int subID)
        {
            string sRet = "";

            ODBCWrapper.DataSetSelectQuery selectQuery = null;

            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("PRICING_CONNECTION");
                selectQuery += "select ID, NAME, from subscriptions with (nolock) where STATUS=1 and IS_ACTIVE=1 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", subID);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sRet = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
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
            return sRet;
        }

        // this function do the trust for the certificate
        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }
    }


    #region SmartSunPaymentResponse

    public class SmartSunPaymentResponse
    {
        public Amounttransaction amountTransaction { get; set; }
    }

    public class Amounttransaction
    {
        public string serverReferenceCode { get; set; }
        public string transactionOperationStatus { get; set; }
        public string endUserId { get; set; }
        public string referenceCode { get; set; }
        public string serviceID { get; set; }
        public string clientCorrelator { get; set; }
        public Paymentamount paymentAmount { get; set; }
    }

    public class Paymentamount
    {
        public string totalAmountCharged { get; set; }
        public Charginginformation chargingInformation { get; set; }
    }

    public class Charginginformation
    {
        public string description { get; set; }
        public string currency { get; set; }
        public string amount { get; set; }
    }

    #endregion

    #region SmartSunRequest

    public class SmartSunChargeRequest
    {
        public Request request { get; set; }
    }

    public class Request
    {
        public string transactionId { get; set; }
        public string merchantId { get; set; }
        public Accountdetails accountDetails { get; set; }
        public Amountdetails amountDetails { get; set; }
        public Servicedetails serviceDetails { get; set; }
    }

    public class Accountdetails
    {
        public string accountId { get; set; }
        public string accountType { get; set; }
        public string fundSource { get; set; }
    }

    public class Amountdetails
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }

    public class Servicedetails
    {
        public string serviceID { get; set; }
    }

    #endregion

    #region SmartSunChargeResponse

    public class SmartSunChargeResponse
    {
        public Response response { get; set; }
    }

    public class Response
    {
        public string transactionId { get; set; }
        public string serverReferenceId { get; set; }
        public string responseCode { get; set; }
        public string responseDesc { get; set; }
        public Fundsourceresponsedetails fundSourceResponseDetails { get; set; }
        public Amountdetails amountDetails { get; set; }
    }

    public class Fundsourceresponsedetails
    {
        public string fundSource { get; set; }
        public string fundSourceResponseCode { get; set; }
    }

    #endregion

}