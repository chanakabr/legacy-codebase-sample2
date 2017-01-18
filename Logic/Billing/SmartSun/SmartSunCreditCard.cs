using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using DAL;
using KLogMonitor;
using M1BL;
using TVinciShared;
using ApiObjects;
using ApiObjects.Billing;

namespace Core.Billing
{
    public class SmartSunCreditCard : AdyenCreditCard
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected string SLogFile
        {
            get { return string.Concat("SmartSunCreditCard_", m_nGroupID); }
        }

        //protected string m_sPaymentMethodID;
        protected string msisdn;

        public SmartSunCreditCard(int nGroupID, string sPaymentMethodID, string sEncryptedCvv)
            : base(nGroupID)
        {
            msisdn = sEncryptedCvv;
            m_sPaymentMethodID = sPaymentMethodID;
        }


        public override BillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode,
            string sUserIP, string sCustomData, int nPaymentNumber, int nNumberOfPayments, string sExtraParameters)
        {
            BillingResponse res = new BillingResponse();

            // if extraParams are empty charge via adyen
            if (string.IsNullOrEmpty(sExtraParameters.Trim()))
            {
                return
                    base.ChargeUser(sSiteGUID, dChargePrice, sCurrencyCode, sUserIP, sCustomData, nPaymentNumber, nNumberOfPayments, sExtraParameters);
            }


            string sAssetID = String.Empty;

            try
            {
                // bool isDummy = (Math.Abs(dChargePrice) < 0.001);
                log.Debug("ChargeUser - " +
                    string.Format("Entering SmartSunCreditCard ChargeUser try block. Site Guid: {0} , User IP: {1} , Custom data: {2}", sSiteGUID, sUserIP, sCustomData));

                if (!Utils.IsUserExist(sSiteGUID, m_nGroupID))
                {
                    res.m_oStatus = BillingResponseStatus.UnKnownUser;
                    res.m_sStatusDescription = "Unknown or inactive user";
                    res.m_sRecieptCode = string.Empty;
                    return res;
                }

                // Retrieve asset ID and Name (Media/Sub) 
                string assetName = String.Empty;
                Dictionary<string, string> oCustomDataDict = Utils.GetCustomDataDictionary(sCustomData);
                ItemType nItemType = ItemType.Unknown;
                sAssetID = GetAssetID(oCustomDataDict, ref assetName, ref nItemType);

                if (string.IsNullOrEmpty(sAssetID))
                    throw new Exception("AssetID is empty");

                int nCustomDataID = Utils.AddCustomData(sCustomData);
                int nTransactionStatus = (int)TransactionStatus.Pending;

                // Create reference transaction ID for SmartSun
                int nReferenceTransactionID = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

                // Insert pending transaction
                int nSmartSunTransactionID =
                    BillingDAL.Insert_SmartSunTransaction(m_nGroupID, sSiteGUID, (int)nItemType, msisdn, dChargePrice, nCustomDataID, nReferenceTransactionID, nTransactionStatus);

                // Limit to 10 digits
                string sReferenceTransactionID = nReferenceTransactionID.ToString("D10");
                if (sReferenceTransactionID.Length > 10)
                {
                    sReferenceTransactionID = sReferenceTransactionID.Substring(0, 10);
                }

                // Try to charge SmartSun
                SmartSunPaymentResponse charged = new SmartSunPaymentResponse { amountTransaction = new Amounttransaction() };
                string resJson = ChargeSmartSun(sReferenceTransactionID, msisdn, dChargePrice, sCurrencyCode, assetName, sExtraParameters);

                if (!string.IsNullOrEmpty(resJson))
                {
                    try
                    {
                        charged = (SmartSunPaymentResponse)Newtonsoft.Json.JsonConvert.DeserializeObject(resJson, typeof(SmartSunPaymentResponse));
                    }
                    catch
                    {
                    }
                }


                // Check and handle SmartSun charge failure
                if ((charged == null) || (charged.amountTransaction == null) ||
                    (string.IsNullOrEmpty(charged.amountTransaction.transactionOperationStatus)) ||
                    (!charged.amountTransaction.transactionOperationStatus.ToLower().Equals("charged")))
                {
                    nTransactionStatus = (int)TransactionStatus.Fail;
                    BillingDAL.UpdateSmartSunTransactionStatus(m_nGroupID, nSmartSunTransactionID, nTransactionStatus);

                    // failed to establish connection or parse cinepolis response
                    res.m_oStatus = BillingResponseStatus.Fail;
                    res.m_sStatusDescription = string.Format("Failed to charge user via SmartSun. Response: {0}", resJson);
                    res.m_sRecieptCode = string.Empty;

                    try
                    {
                        Utils.SendMail("SMART-SUN", assetName, sSiteGUID, nReferenceTransactionID, dChargePrice.ToString(), sCurrencyCode, msisdn, m_nGroupID, string.Empty, eMailTemplateType.PaymentFail);
                    }
                    catch (Exception ex)
                    {
                        log.Error("ChargeUser - " + string.Format("Failed to send mail. Site Guid: {0} , Asset: {1} , Exception: {2}", sSiteGUID, assetName, ex), ex);
                    }

                    return res;
                }

                // Charge succeeded
                nTransactionStatus = (int)TransactionStatus.Success;
                BillingDAL.UpdateSmartSunTransactionStatus(m_nGroupID, nSmartSunTransactionID, nTransactionStatus);

                bool bIsRecurring = Utils.CalcIsRecurringBool(oCustomDataDict);
                int nMediaFileID = Utils.ParseIntIfNotEmpty(oCustomDataDict[Constants.MEDIA_FILE]);
                int nMediaID = Utils.ParseIntIfNotEmpty(oCustomDataDict[Constants.MEDIA_ID]);
                int nBillingStatus = 0; //inserted new value to tvinci.dbo.billing_transactions should be 0   

                long nBillingTransactionID =
                    Utils.InsertBillingTransaction(sSiteGUID,
                                                    String.Empty,
                                                    dChargePrice,
                                                    oCustomDataDict[Constants.PRICE_CODE],
                                                    sCurrencyCode,
                                                    sCustomData,
                                                    nBillingStatus,
                                                    String.Empty,
                                                    bIsRecurring,
                                                    nMediaFileID,
                                                    nMediaID,
                                                    oCustomDataDict[Constants.PPV_MODULE],
                                                    oCustomDataDict[Constants.SUBSCRIPTION_ID],
                                                    msisdn,
                                                    m_nGroupID,
                                                    (int)eBillingProvider.SmartSun,
                                                    nSmartSunTransactionID,
                                                    0.0,
                                                    dChargePrice,
                                                    nPaymentNumber,
                                                    nNumberOfPayments,
                                                    String.Empty,
                                                    oCustomDataDict[Constants.COUNTRY_CODE], 
                                                    oCustomDataDict[Constants.LANGUAGE_CODE], 
                                                    oCustomDataDict[Constants.DEVICE_NAME], 
                                                    (int)eBillingProvider.SmartSun, 
                                                    (int)ePaymentMethod.SmartSun, 
                                                    String.Empty, 
                                                    oCustomDataDict[Constants.PREVIEW_MODULE], 
                                                    String.Empty);


                res.m_oStatus = BillingResponseStatus.Success;
                res.m_sRecieptCode = nBillingTransactionID.ToString();

                if (charged != null && charged.amountTransaction != null)
                {
                    if (!string.IsNullOrEmpty(charged.amountTransaction.serverReferenceCode))
                    {
                        res.m_sExternalReceiptCode = charged.amountTransaction.serverReferenceCode;
                    }

                    if (charged.amountTransaction.paymentAmount != null &&
                        charged.amountTransaction.paymentAmount.chargingInformation != null &&
                        !string.IsNullOrEmpty(charged.amountTransaction.paymentAmount.chargingInformation.description))
                    {
                        res.m_sStatusDescription = charged.amountTransaction.paymentAmount.chargingInformation.description;
                    }
                }
                else
                {
                    res.m_sStatusDescription = resJson;
                }

                try
                {
                    Utils.SendMail("SMART-SUN", assetName, sSiteGUID, nReferenceTransactionID, dChargePrice.ToString(CultureInfo.InvariantCulture), sCurrencyCode, 
                        msisdn, m_nGroupID, string.Empty, eMailTemplateType.Purchase);
                }
                catch (Exception ex)
                {
                    log.Error("ChargeUser - " + string.Format("Failed to send mail. Site Guid: {0} , Asset: {1} , Exception: {2}", sSiteGUID, assetName, ex), ex);
                }
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

        protected string ChargeSmartSun(string sTransactionID, string msisdn, double dPrice, string sCurrency, string description, string token)
        {
            string res = String.Empty;
            //SmartSunPaymentResponse res = new SmartSunPaymentResponse();

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

                Uri requestUri = null;
                bool isGoodUri = Uri.TryCreate(sWSURL, UriKind.Absolute, out requestUri) &&
                    (requestUri.Scheme == Uri.UriSchemeHttp || requestUri.Scheme == Uri.UriSchemeHttps);

                if (isGoodUri)
                {
                    string clientID = GetSmartSunClientID(m_nGroupID);
                    string jsonRes = MakeJsonRequest(requestUri, token, clientID, postMessage);

                    if (string.IsNullOrEmpty(jsonRes))
                    {
                        return res;
                        //res = new SmartSunPaymentResponse { amountTransaction = new Amounttransaction() };
                    }

                    res = jsonRes;
                    //SmartSunPaymentResponse objResponse = (SmartSunPaymentResponse)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonRes, typeof(SmartSunPaymentResponse));
                    //res = objResponse;
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

                log.Debug("Exception - " + ex.ToString(), ex);

                #endregion

                int errorCode = System.Runtime.InteropServices.Marshal.GetExceptionCode();
                //res = new SmartSunPaymentResponse { amountTransaction = new Amounttransaction() };
            }

            return res;
        }

        protected static string GetSmartSunClientID(int groupID)
        {
            string sRet = "";

            string strClientID = BillingDAL.GetClientIDFromGroupParams(groupID);

            if (!string.IsNullOrEmpty(strClientID) && strClientID.ToLower().Contains("client_id"))
            {
                //https://stg.authenticate.smart.com.ph/sps/SmartDevOAuth2/oauth20/authorize?response_type=code&client_id=BLINK01&scope=O_VL_chargeAmountAirtime&redirect_uri=https://www.blink-now.com/firstleglanding.aspx

                string temp = strClientID.Substring(strClientID.IndexOf("client_id=") + 10);
                sRet = temp.Substring(0, temp.IndexOf("&"));
            }

            return sRet;
        }

        protected string MakeJsonRequest(Uri requestUri, string bearerToken, string clientID, string jsonContent = "")
        {
            try
            {
                Dictionary<string, string> dHeaders = new Dictionary<string, string>() 
                {
                    {"Authorization", "Bearer " + bearerToken },
                    {"Client_id", clientID }
                };

                DateTime dNow = DateTime.UtcNow;

                // handle trusting the ssl certificate
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, policyErrors) => true;

                string profJson = WS_Utils.SendXMLHttpReqWithHeaders(requestUri.OriginalString, jsonContent, dHeaders, "application/x-www-form-urlencoded");    //, "application/json", "", "", "", "", "post");
                double dTime = DateTime.UtcNow.Subtract(dNow).TotalMilliseconds;

                log.Debug("MakeJsonRequest - " + string.Format("Response: {0}", profJson));

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

                log.Error("MakeJsonRequest - " + sb.ToString());
                #endregion
            }

            return string.Empty;
        }

        protected static string GetAssetID(Dictionary<string, string> oCustomDataDict, ref string assetName, ref ItemType type)
        {
            string assetID = String.Empty;

            if (oCustomDataDict[Constants.BUSINESS_MODULE_TYPE].Trim().ToLower().Equals("pp"))
            {
                assetID = oCustomDataDict[Constants.PPV_MODULE];
                type = ItemType.PPV;

                string sMediaID = oCustomDataDict[Constants.MEDIA_ID].Trim();
                if (!string.IsNullOrEmpty(sMediaID))
                {
                    int nMediaID = int.Parse(sMediaID);
                    assetName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", nMediaID, "MAIN_CONNECTION_STRING").ToString();
                }
            }
            if (oCustomDataDict[Constants.BUSINESS_MODULE_TYPE].Trim().ToLower().Equals("sp"))
            {
                assetID = oCustomDataDict[Constants.SUBSCRIPTION_ID];
                type = ItemType.Subscription;

                if (!string.IsNullOrEmpty(assetID))
                {
                    int nSubID = int.Parse(assetID);
                    assetName = ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "name", nSubID, "PRICING_CONNECTION").ToString();
                }
            }

            return assetID;
        }

    }

    public enum TransactionStatus
    {
        Pending = 0,
        Success = 1,
        Fail = 2
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
