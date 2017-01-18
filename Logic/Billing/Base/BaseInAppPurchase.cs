using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using KLogMonitor;
using System.Reflection;
using Core.Pricing;
using Core.Users;
using ApiObjects;
using ApiObjects.Billing;

namespace Core.Billing
{
    public abstract class BaseInAppPurchase
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected int m_nGroupID;

        public BaseInAppPurchase() { }

        public BaseInAppPurchase(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        #region Methods

        public virtual InAppBillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, int nPaymentNumber, int nNumberOfPayments, string ReceiptData)
        {
            InAppBillingResponse ret = new InAppBillingResponse();
            try
            {
                string sReceiptData = ReceiptData; // save the receipt data 

                UserResponseObject uObj = Core.Users.Module.GetUserData(m_nGroupID, sSiteGUID, string.Empty);
                if (uObj.m_RespStatus != ResponseStatus.OK)
                {
                    ret = new InAppBillingResponse();
                    ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.UnKnownUser;
                    ret.m_oBillingResponse.m_sRecieptCode = "";
                    ret.m_oBillingResponse.m_sStatusDescription = "Unknown or active user";
                    ret.m_oInAppReceipt = null;
                    return ret;
                }

                bool IsPPV = true;

                if (sCustomData.Contains("customdata type=\"sp\"")) //if it's subscription then get the latest receipt
                {
                    IsPPV = false;
                }

                InAppReceipt receipt = ValidateReceipt(ReceiptData, IsPPV);

                //if it's subscription then get the latest receipt
                if (!IsPPV && receipt != null && receipt.latest_receipt != null)
                {
                    sReceiptData = receipt.latest_receipt.ToString();
                }

                ret.m_oInAppReceipt = receipt;
                InAppTransactionStatus status = (InAppTransactionStatus)Enum.Parse(typeof(InAppTransactionStatus), receipt.Status);

                #region Get BillingResponse Status
                switch (status)
                {
                    case InAppTransactionStatus.Success:
                        #region Update Transaction
                        ret.m_oBillingResponse.m_sRecieptCode = Utils.InsertInAppTransaction(m_nGroupID, sSiteGUID, dChargePrice, sCurrencyCode, sReceiptData, nPaymentNumber, nNumberOfPayments, sCustomData).ToString();
                        #endregion
                        if (ret.m_oBillingResponse.m_sRecieptCode != "-1")
                        {

                            ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Success;
                            ret.m_oBillingResponse.m_sStatusDescription = "OK";
                        }
                        else
                        {
                            ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                            ret.m_oBillingResponse.m_sStatusDescription = "Insert InApp transaction fail.";
                        }
                        break;
                    case InAppTransactionStatus.InvalidJSON:
                        ret.m_oBillingResponse.m_sStatusDescription = "The App Store could not read the JSON object you provided.";
                        ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                        break;
                    case InAppTransactionStatus.ReceiptDataMalformed:
                        ret.m_oBillingResponse.m_sStatusDescription = "The data in the receipt-data property was malformed.";
                        ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                        break;
                    case InAppTransactionStatus.AuthenticatedFailed:
                        ret.m_oBillingResponse.m_sStatusDescription = "The receipt could not be authenticated.";
                        ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                        break;
                    case InAppTransactionStatus.SharedSecretError:
                        ret.m_oBillingResponse.m_sStatusDescription = "The shared secret you provided does not match the shared secret on file for your account.";
                        ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                        break;
                    case InAppTransactionStatus.ServerUnavailable:
                        ret.m_oBillingResponse.m_sStatusDescription = "The Receipt server is not currently availble.";
                        ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                        break;
                    case InAppTransactionStatus.SubscriptionExpired:
                        ret.m_oBillingResponse.m_sRecieptCode = Utils.InsertInAppTransaction(m_nGroupID, sSiteGUID, dChargePrice, sCurrencyCode, ReceiptData, nPaymentNumber, nNumberOfPayments, sCustomData).ToString();
                        ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Success;
                        if (ret.m_oBillingResponse.m_sRecieptCode != "-1")
                        {
                            ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Success;
                            ret.m_oBillingResponse.m_sStatusDescription = "OK";
                        }
                        else
                        {
                            ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                            ret.m_oBillingResponse.m_sStatusDescription = "Insert InApp transaction fail.";
                        }
                        break;
                    case InAppTransactionStatus.SandboxVerficationError:
                        ret.m_oBillingResponse.m_sStatusDescription = "This receipt is a sandbox receipt, but it was sent to the production service for verfication.";
                        ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                        break;
                    case InAppTransactionStatus.ProductionVerficationError:
                        ret.m_oBillingResponse.m_sStatusDescription = "This receipt is a production receipt, but it was sent to the sandbox service fpr verfication.";
                        ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                        break;
                }
                #endregion
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at BaseInAppPurchase.ChargeUser. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Charge Price: ", dChargePrice));
                sb.Append(String.Concat(" Currency Cd: ", sCurrencyCode));
                sb.Append(String.Concat(" User IP: ", sUserIP));
                sb.Append(String.Concat(" CD: ", sCustomData));
                sb.Append(String.Concat(" Payment num: ", nPaymentNumber));
                sb.Append(String.Concat(" Num Of Payments: ", nNumberOfPayments));
                sb.Append(String.Concat(" Rcpt: ", ReceiptData));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;
            }
            return ret;
        }
        public virtual InAppBillingResponse ReneweInAppPurchase(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sCustomData, int nPaymentNumber, int nNumberOfPayments, int nInAppTransactionID)
        {
            InAppBillingResponse ret = new InAppBillingResponse();
            Stream dataStream = null;
            WebResponse response = null;
            StreamReader reader = null;
            try
            {
                string ReceiptData = GetReceiptData(nInAppTransactionID);
                string InAppToken = string.Empty;
                string InAppSharedSecret = string.Empty;
                string AppleValidationReceitsURL = string.Empty;
                Utils.GetAppleValidationReceitsURL(ref AppleValidationReceitsURL, ref InAppToken, ref InAppSharedSecret, m_nGroupID);

                // Create a request using a URL that can receive a post.             
                WebRequest request = WebRequest.Create(AppleValidationReceitsURL);
                // Set the Method property of the request to POST.            
                request.Method = "POST";

                // Create POST data and convert it to a byte array.       
                //Renwable subscription must use   "SharedSecret" key   
                string JSONObject = string.Format("{{\"receipt-data\" : \"{0}\", \"password\" : \"{1}\"}}", ReceiptData, InAppSharedSecret);
                byte[] byteArray = Encoding.UTF8.GetBytes(JSONObject);

                // Set the ContentType property of the WebRequest.            
                request.ContentType = "application/json";
                // Set the ContentLength property of the WebRequest.            
                request.ContentLength = byteArray.Length;
                // Get the request stream.            
                dataStream = request.GetRequestStream();
                // Write the data to the request stream.            
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Get the response.            
                response = request.GetResponse();
                // Display the status           
                // Get the stream containing content returned by the server.            
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.            
                reader = new StreamReader(dataStream);
                // Read the content.            
                string responseFromServer = reader.ReadToEnd();

                //JavaScriptSerializer serializer = new JavaScriptSerializer();
                //InAppReceipt receipt = serializer.Deserialize<InAppReceipt>(responseFromServer);

                InAppReceipt receipt = CreateInAppReceiptObject(responseFromServer);

                ret.m_oInAppReceipt = receipt;

                InAppTransactionStatus status =
                    (InAppTransactionStatus)Enum.Parse(typeof(InAppTransactionStatus), receipt.Status);

                string latestReceipt = receipt.latest_receipt;
                bool terminate = false;

                if (status == InAppTransactionStatus.Success && receipt.iOSVersion == "7" && receipt.latest_receipt_info != null)
                {
                    string sProductCode = string.Empty;

                    try
                    {
                        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                        doc.LoadXml(sCustomData);
                        System.Xml.XmlNode theRequest = doc.FirstChild;

                        string sSubscriptionID = Utils.GetSafeValue("s", ref theRequest);

                        if (!string.IsNullOrEmpty(sSubscriptionID))
                        {
                            Subscription subscription = Utils.GetSubscriptionData(m_nGroupID, sSubscriptionID);
                            if (subscription != null)
                            {
                                sProductCode = subscription.m_ProductCode;

                                double endMS = 0;

                                // Run on all latest receipts and find the one that matches the date and the product id
                                foreach (var lastReceipt in receipt.latest_receipt_info)
                                {
                                    // If the product code matches
                                    if (lastReceipt.product_id == sProductCode)
                                    {
                                        // Find the maximum start date
                                        double currentEndMS = double.Parse(lastReceipt.expires_date_ms);

                                        if (currentEndMS > endMS)
                                        {
                                            endMS = currentEndMS;
                                        }
                                    }
                                }

                                long epochMS = TVinciShared.DateUtils.DateTimeToUnixTimestampMilliseconds(DateTime.UtcNow);
                                if (endMS < epochMS)
                                {
                                    status = InAppTransactionStatus.SubscriptionExpired;
                                    log.Debug("ReneweInAppPurchase - " + string.Format("SubscriptionExpired, nGroupID : {0} , inapp : {1}, subID : {2}, expires_date_ms : {3} , epochMS : {4}", m_nGroupID, nInAppTransactionID, sSubscriptionID, endMS, epochMS));
                                }
                            }
                            else
                            {
                                log.Debug("ReneweInAppPurchase - " + string.Format("Get sub return null, nGroupID : {0} , inapp : {1}, subID : {2}", m_nGroupID, nInAppTransactionID, sSubscriptionID));
                                ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.UnKnownPPVModule;
                                ret.m_oBillingResponse.m_sStatusDescription = "Insert InApp transaction fail.";
                                terminate = true;
                            }

                        }
                        else
                        {
                            log.Debug("ReneweInAppPurchase - " + string.Format("Get sub return empty, nGroupID : {1} , inapp : {1}", m_nGroupID, nInAppTransactionID));
                            ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.UnKnownPPVModule;
                            ret.m_oBillingResponse.m_sStatusDescription = "Insert InApp transaction fail.";
                            terminate = true;
                        }

                    }
                    catch (Exception ex)
                    {
                        log.Error("ReneweInAppPurchase - " + string.Format("Get sub failed, Exception: {0} , nGroupID : {1} , inapp : {2}", ex.ToString(), m_nGroupID, nInAppTransactionID), ex);
                        ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                        ret.m_oBillingResponse.m_sStatusDescription = "Insert InApp transaction fail.";
                        terminate = true;
                    }

                }

                if (!terminate)
                {
                    switch (status)
                    {
                        case InAppTransactionStatus.Success:
                            {
                                #region Update Transaction
                                ret.m_oBillingResponse.m_sRecieptCode =
                                    Utils.InsertInAppTransaction(m_nGroupID, sSiteGUID, dChargePrice, sCurrencyCode, latestReceipt, nPaymentNumber, nNumberOfPayments, sCustomData).ToString();
                                #endregion

                                if (ret.m_oBillingResponse.m_sRecieptCode != "-1")
                                {
                                    ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Success;
                                    ret.m_oBillingResponse.m_sStatusDescription = "OK";
                                }
                                else
                                {
                                    ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                                    ret.m_oBillingResponse.m_sStatusDescription = "Insert InApp transaction fail.";
                                }

                                break;
                            }
                        case InAppTransactionStatus.InvalidJSON:
                            ret.m_oBillingResponse.m_sStatusDescription = "The App Store could not read the JSON object you provided.";
                            ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                            break;
                        case InAppTransactionStatus.ReceiptDataMalformed:
                            ret.m_oBillingResponse.m_sStatusDescription = "The data in the receipt-data property was malformed.";
                            ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                            break;
                        case InAppTransactionStatus.AuthenticatedFailed:
                            ret.m_oBillingResponse.m_sStatusDescription = "The receipt could not be authenticated.";
                            ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                            break;
                        case InAppTransactionStatus.SharedSecretError:
                            ret.m_oBillingResponse.m_sStatusDescription = "The shared secret you provided does not match the shared secret on file for your account.";
                            ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                            break;
                        case InAppTransactionStatus.ServerUnavailable:
                            ret.m_oBillingResponse.m_sStatusDescription = "The Receipt server is not currently availble.";
                            ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                            break;
                        case InAppTransactionStatus.SubscriptionExpired:
                            ret.m_oBillingResponse.m_sStatusDescription = "This receipt is valid but the subscription has expired.";
                            ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                            break;
                        case InAppTransactionStatus.SandboxVerficationError:
                            ret.m_oBillingResponse.m_sStatusDescription = "This receipt is a sandbox receipt, but it was sent to the production service for verfication.";
                            ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                            break;
                        case InAppTransactionStatus.ProductionVerficationError:
                            ret.m_oBillingResponse.m_sStatusDescription = "This receipt is a production receipt, but it was sent to the sandbox service fpr verfication.";
                            ret.m_oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at ReneweInAppPurchase. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" CP: ", dChargePrice));
                sb.Append(String.Concat(" Currency Cd: ", sCurrencyCode));
                sb.Append(String.Concat(" CD: ", sCustomData));
                sb.Append(String.Concat(" Payment Num: ", nPaymentNumber));
                sb.Append(String.Concat(" Num Of Payments: ", nNumberOfPayments));
                sb.Append(String.Concat(" InApp Trans ID: ", nInAppTransactionID));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - "+ sb.ToString());
                #endregion
                throw;

            }
            finally
            {
                if (dataStream != null)
                {
                    dataStream.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
                if (reader != null)
                {
                    reader.Dispose();
                }
            }
            return ret;
        }

        private InAppReceipt ValidateReceipt(string sReceiptData, bool IsPPV)
        {
            InAppReceipt receipt = new InAppReceipt();

            string InAppToken = string.Empty;
            string InAppSharedSecret = string.Empty;
            string AppleValidationReceitsURL = string.Empty;
            Stream dataStream = null;
            WebResponse response = null;
            StreamReader reader = null;
            try
            {
                Utils.GetAppleValidationReceitsURL(ref AppleValidationReceitsURL, ref InAppToken, ref InAppSharedSecret, m_nGroupID);
                string PasswordToSendBox = string.Empty;

                if (IsPPV)
                {
                    PasswordToSendBox = InAppToken;
                }
                else
                {
                    PasswordToSendBox = InAppSharedSecret;
                }

                // Create a request using a URL that can receive a post.             
                WebRequest request = WebRequest.Create(AppleValidationReceitsURL);
                // Set the Method property of the request to POST.            
                request.Method = "POST";
                // Create POST data and convert it to a byte array.
                string JSONObject = string.Format("{{\"receipt-data\" : \"{0}\", \"password\" : \"{1}\"}}", sReceiptData, PasswordToSendBox);
                byte[] byteArray = Encoding.UTF8.GetBytes(JSONObject);
                // Set the ContentType property of the WebRequest.            
                request.ContentType = "application/json";
                // Set the ContentLength property of the WebRequest.            
                request.ContentLength = byteArray.Length;
                // Get the request stream.            
                dataStream = request.GetRequestStream();
                // Write the data to the request stream.            
                dataStream.Write(byteArray, 0, byteArray.Length);

                // Get the response.            
                response = request.GetResponse();
                // Display the status.
                // Get the stream containing content returned by the server.            
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.            
                reader = new StreamReader(dataStream);
                // Read the content.            
                string responseFromServer = reader.ReadToEnd();

                //JavaScriptSerializer serializer = new JavaScriptSerializer();
                receipt = CreateInAppReceiptObject(responseFromServer);

                //receipt = serializer.Deserialize<InAppReceipt>(responseFromServer);
            }
            finally
            {
                if (dataStream != null)
                {
                    dataStream.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
                if (reader != null)
                {
                    reader.Dispose();
                }
            }
            return receipt;
        }

        /// <summary>
        /// Parses a json string to an InApp Receipt object - but only the needed fields
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        private static InAppReceipt CreateInAppReceiptObject(string jsonString)
        {
            InAppReceipt receipt = new InAppReceipt();
            receipt.latest_receipt_info = new List<iTunesReceipt>();

            // Version will be 6 until evidence shows otherwise
            receipt.iOSVersion = "6";

            JObject json = JObject.Parse(jsonString);

            receipt.Status = json["status"].ToString();

            // Only if succesful
            if (receipt.Status == "0")
            {
                var jsonLatestReceipt = json["latest_receipt"];

                if (jsonLatestReceipt != null)
                {
                    receipt.latest_receipt = jsonLatestReceipt.ToString();
                }

                JToken jsonLatestReceiptInfo = json["latest_receipt_info"];

                if (jsonLatestReceiptInfo != null)
                {
                    // Differences between iOS 6 and 7 - in one there is an array, the other has just an object. 
                    // That's why we check the type.
                    // The last in the array is the truely latest receipt
                    if (jsonLatestReceiptInfo.Type == JTokenType.Array)
                    {
                        receipt.iOSVersion = "7";

                        foreach (JToken jsonReceiptInfo in jsonLatestReceiptInfo)
                        {
                            receipt.latest_receipt_info.Add(
                                new iTunesReceipt()
                                {
                                    product_id = GetJsonString(jsonReceiptInfo, "product_id"),
                                    expires_date = GetJsonString(jsonReceiptInfo, "expires_date"),
                                    expires_date_ms = GetJsonString(jsonReceiptInfo, "expires_date_ms"),
                                    purchase_date_ms = GetJsonString(jsonReceiptInfo, "purchase_date_ms"),
                                }
                            );
                        }
                    }
                    else if (jsonLatestReceiptInfo.Type == JTokenType.Object)
                    {
                        receipt.latest_receipt_info = new List<iTunesReceipt>()
                        {
                            new iTunesReceipt()
                            {
                                product_id = GetJsonString(jsonLatestReceiptInfo, "product_id"),
                                expires_date = GetJsonString(jsonLatestReceiptInfo, "expires_date"),
                                expires_date_ms = GetJsonString(jsonLatestReceiptInfo, "expires_date_ms"),
                                purchase_date_ms = GetJsonString(jsonLatestReceiptInfo, "purchase_date_ms"),
                            }
                        };
                    }
                }

                JToken jsonReceipt = json["receipt"];
                JToken jsonInAppReceipt = jsonReceipt["in_app"];

                // Differences between iOS 6 and 7 - in one there is "in_app" and the other doesn't have it
                // So we only do a null check - if it exists or not
                if (jsonInAppReceipt == null)
                {
                    if (jsonReceipt != null)
                    {
                        receipt.receipt = new iTunesReceipt()
                        {
                            expires_date = GetJsonString(jsonReceipt, "expires_date"),
                            purchase_date_ms = GetJsonString(jsonReceipt, "purchase_date_ms")
                        };
                    }
                }
                else
                {
                    receipt.iOSVersion = "7";
                    receipt.in_app = new List<iTunesReceipt>();

                    // Usually there will be an array. We choose the last (most recent) in app receipt
                    if (jsonInAppReceipt.Type == JTokenType.Array)
                    {
                        foreach (JToken jsonReceiptInfo in jsonInAppReceipt)
                        {
                            receipt.in_app.Add(
                                new iTunesReceipt()
                                {
                                    product_id = GetJsonString(jsonReceiptInfo, "product_id"),
                                    expires_date = GetJsonString(jsonReceiptInfo, "expires_date"),
                                    expires_date_ms = GetJsonString(jsonReceiptInfo, "expires_date_ms"),
                                    purchase_date_ms = GetJsonString(jsonReceiptInfo, "purchase_date_ms"),
                                }
                            );
                        }
                    }
                }
            }

            return receipt;
        }

        /// <summary>
        /// Extracts a field from a token, if it exists (fail-safe)
        /// </summary>
        /// <param name="token"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        private static string GetJsonString(JToken token, string field)
        {
            string str = string.Empty;

            var jsonField = token[field];

            if (jsonField != null)
            {
                str = jsonField.ToString();
            }

            return str;
        }

        private string GetiTunesValidation()
        {

            return string.Empty;
        }

        private string GetReceiptData(int nInAppTransactionID)
        {
            string res = string.Empty;
            object oRecipt = ODBCWrapper.Utils.GetTableSingleVal("inapp_transactions", "receipt_data", nInAppTransactionID);
            if (oRecipt != null && oRecipt != DBNull.Value)
            {
                res = oRecipt.ToString();
            }

            return res;
        }

        #endregion
    }
}
