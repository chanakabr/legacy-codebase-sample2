using ApiObjects;
using ApiObjects.Billing;
using Core.Billing;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Services;

namespace WS_Billing
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://billing.tvinci.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class module : System.Web.Services.WebService
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [WebMethod]
        public string GetBillingCutomData(string sWSUserName, string sWSPassword)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "GetBillingCutomData");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.GetBillingCutomData(nGroupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }

        }

        [WebMethod]
        public string CC_GetUserCCDigits(string sWSUserName, string sWSPassword, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "CC_GetUserCCDigits");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.CC_GetUserCCDigits(nGroupID, sSiteGUID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public void CC_DeleteUserCCDigits(string sWSUserName, string sWSPassword, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "CC_DeleteUserCCDigits");
            if (nGroupID != 0)
            {
                Core.Billing.Module.CC_DeleteUserCCDigits(nGroupID, sSiteGUID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
        }

        [WebMethod]
        public string CC_GetClientCheckSum(string sWSUserName, string sWSPassword, string sUserIP, string sRandom)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "CC_GetClientCheckSum");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.CC_GetClientCheckSum(nGroupID, sUserIP, sRandom);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public string GetClientMerchantSig(string sWSUserName, string sWSPassword, string sParamaters)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "GetClientMerchantSig");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.GetClientMerchantSig(nGroupID, sParamaters);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public string CC_GetPopupURL(string sWSUserName, string sWSPassword, double dChargePrice, string sCurrencyCode,
            string sItemName, string sCustomData, string sPaymentMethod, string sExtraParameters)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "CC_GetPopupURL");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.CC_GetPopupURL(nGroupID, dChargePrice, sCurrencyCode, sItemName, sCustomData, sPaymentMethod, sExtraParameters);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public AdyenBillingDetail GetLastBillingUserInfo(string sWSUserName, string sWSPassword, string sSiteGUID, int nBillingMethod)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "GetLastBillingUserInfo");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.GetLastBillingUserInfo(nGroupID, sSiteGUID, nBillingMethod);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        public AdyenBillingDetail GetLastBillingTypeUserInfo(string sWSUserName, string sWSPassword, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "GetLastBillingTypeUserInfo");
            if (nGroupID != 0)
            {
                return Utils.GetLastBillingTypeUserInfo(nGroupID, sSiteGUID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(CCDetails))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse CC_ChargeUser(string sWSUserName, string sWSPassword, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string sExtraParameters, string sPaymentMethodID, string sEncryptedCVV)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "CC_ChargeUser");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.CC_ChargeUser(nGroupID, sSiteGUID, dChargePrice, sCurrencyCode, sUserIP, sCustomData, nPaymentNumber, nNumberOfPayments, sExtraParameters, sPaymentMethodID, sEncryptedCVV);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public bool UpdatePurchaseIDInBilling(string sWSUserName, string sWSPassword, long purchaseID, long billingRefTransactionID)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "CC_UpdatePurchaseIDInBilling");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.UpdatePurchaseIDInBilling(nGroupID, purchaseID, billingRefTransactionID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(CCDetails))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse DD_ChargeUser(string sWSUserName, string sWSPassword, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string sExtraParameters, int nBillingMethod)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "CC_ChargeUser");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.DD_ChargeUser(nGroupID, sSiteGUID, dChargePrice, sCurrencyCode, sUserIP, sCustomData, nPaymentNumber, nNumberOfPayments, sExtraParameters, nBillingMethod);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(CCDetails))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public InAppBillingResponse InApp_ChargeUser(string sWSUserName, string sWSPassword, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string ReceiptData)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "InApp_ChargeUser");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.InApp_ChargeUser(nGroupID, sSiteGUID, dChargePrice, sCurrencyCode, sUserIP, sCustomData, nPaymentNumber, nNumberOfPayments, ReceiptData);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(CCDetails))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public InAppBillingResponse InApp_ReneweInAppPurchase(string sWSUserName, string sWSPassword, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, int nInAppTransactionID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "InApp_ChargeUser");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.InApp_ReneweInAppPurchase(nGroupID, sSiteGUID, dChargePrice, sCurrencyCode, sCustomData, nPaymentNumber, nNumberOfPayments, nInAppTransactionID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }



        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(CCDetails))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public bool DD_RefundUser(string sWSUserName, string sWSPassword, string sPSPReference, string sSiteGuid, double dChargePrice, string sCurrencyCode, long lPurchaseID, int nType, int nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "CC_ChargeUser");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.DD_RefundUser(nGroupID, sPSPReference, sSiteGuid, dChargePrice, sCurrencyCode, lPurchaseID, nType, nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(CCDetails))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse CC_DummyChargeUser(string sWSUserName, string sWSPassword, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string sExtraParameters)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "CC_DummyChargeUser");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.CC_DummyChargeUser(nGroupID, sSiteGUID, dChargePrice, sCurrencyCode, sUserIP, sCustomData, nPaymentNumber, nNumberOfPayments, sExtraParameters);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(CCDetails))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse SMS_SendCode(string sWSUserName, string sWSPassword, string sSiteGUID, string sCellPhone, string sReferenceCode, string sExtraParameters)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SMS_SendCode");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.SMS_SendCode(nGroupID, sSiteGUID, sCellPhone, sReferenceCode, sExtraParameters);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(CCDetails))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse SMS_CheckCode(string sWSUserName, string sWSPassword, string sSiteGUID, string sCellPhone, string sSMSCode, string sReferenceCode)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SMS_CheckCode");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.SMS_CheckCode(nGroupID, sSiteGUID, sCellPhone, sSMSCode, sReferenceCode);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(CCDetails))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public bool SendPurchaseMail(string sWSUserName, string sWSPassword, string sSiteGUID, string sPaymentMethod, string sItemName, Int32 nBillingTransID, string stotalAmount, string scurrency, string sExternalNum)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SendPurchaseMail");
            if (nGroupID != 0)
            {
                Utils.SendMail(sPaymentMethod, sItemName, sSiteGUID, nBillingTransID, stotalAmount, scurrency, sExternalNum, nGroupID, string.Empty, eMailTemplateType.Purchase);
                return true;
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(CCDetails))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse Cellular_ChargeUser(string sWSUserName, string sWSPassword, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string sExtraParameters)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "Cellular_ChargeUser");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.Cellular_ChargeUser(nGroupID, sSiteGUID, dChargePrice, sCurrencyCode, sUserIP, sCustomData, nPaymentNumber, nNumberOfPayments, sExtraParameters);
            }
            else
            {
                log.Debug("group_id not found - " + sWSUserName + " " + sWSPassword);
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PaymentGatewaySettingsResponse))]
        public PaymentGatewaySettingsResponse GetPaymentGatewateSettings(string sWSUserName, string sWSPassword)
        {
            PaymentGatewaySettingsResponse response = new PaymentGatewaySettingsResponse();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "GetPaymentGWConfiguration");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.GetPaymentGatewateSettings(nGroupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new PaymentGatewaySettingsResponse();
                response.resp.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.resp.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status SetPaymentGateway(string sWSUserName, string sWSPassword, int paymentGatewayId, PaymentGateway paymentGateway)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SetPaymentGW");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.SetPaymentGateway(nGroupID, paymentGatewayId, paymentGateway);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public PaymentGatewayItemResponse UpdatePaymentGateway(string sWSUserName, string sWSPassword, int paymentGatewayId, PaymentGateway paymentGateway)
        {

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SetPaymentGW");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.UpdatePaymentGateway(nGroupID, paymentGatewayId, paymentGateway);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;

                PaymentGatewayItemResponse response = new PaymentGatewayItemResponse();
                response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status SetPaymentGWSettings(string sWSUserName, string sWSPassword, int paymentGWID, List<PaymentGatewaySettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SetPaymentGWConfiguration");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.SetPaymentGWSettings(nGroupID, paymentGWID, settings);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status DeletePaymentGateway(string sWSUserName, string sWSPassword, int paymentGwID)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "DeletePaymentGW");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.DeletePaymentGateway(nGroupID, paymentGwID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status DeletePaymentGWSettings(string sWSUserName, string sWSPassword, int paymentGwID, List<PaymentGatewaySettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "DeletePaymentGWParams");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.DeletePaymentGWSettings(nGroupID, paymentGwID, settings);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status InsertPaymentGateway(string sWSUserName, string sWSPassword, PaymentGateway pgw)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "InsertPaymentGateway");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.InsertPaymentGateway(nGroupID, pgw);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public PaymentGatewayItemResponse AddPaymentGateway(string sWSUserName, string sWSPassword, PaymentGateway pgw)
        {

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "InsertPaymentGateway");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.AddPaymentGateway(nGroupID, pgw);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                
                PaymentGatewayItemResponse response = new PaymentGatewayItemResponse();
                response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                return response;
            }
        }

        [WebMethod]
        [Obsolete]
        public ApiObjects.Response.Status InsertPaymentGatewaySettings(string sWSUserName, string sWSPassword, int paymentGatewayId, List<PaymentGatewaySettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "InsertPaymentGatewaySettings");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.InsertPaymentGatewaySettings(nGroupID, paymentGatewayId, settings);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public PaymentGatewayItemResponse AddPaymentGatewaySettings(string sWSUserName, string sWSPassword, int paymentGatewayId, List<PaymentGatewaySettings> settings)
        {

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "InsertPaymentGatewaySettings");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.AddPaymentGatewaySettings(nGroupID, paymentGatewayId, settings);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;

                PaymentGatewayItemResponse response = new PaymentGatewayItemResponse();
                response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PaymentGatewayResponse))]
        public PaymentGatewayResponse GetPaymentGateway(string sWSUserName, string sWSPassword)
        {
            PaymentGatewayResponse response = new PaymentGatewayResponse();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "GetPaymentGateway");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.GetPaymentGateway(nGroupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new PaymentGatewayResponse();
                response.resp.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.resp.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status SetHouseholdPaymentGateway(string sWSUserName, string sWSPassword, int paymentGwID, string siteGuid, int householdId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SetHouseholdPaymentGateway");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.SetHouseholdPaymentGateway(nGroupID, paymentGwID, siteGuid, householdId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status DeleteHouseholdPaymentGateway(string sWSUserName, string sWSPassword, int paymentGatewayId, string siteGuid, int householdId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "DeleteHouseholdPaymentGateway");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.DeleteHouseholdPaymentGateway(nGroupID, paymentGatewayId, siteGuid, householdId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public PaymentGatewayListResponse GetHouseholdPaymentGateways(string sWSUserName, string sWSPassword, string siteGuid, int householdId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            PaymentGatewayListResponse response = new PaymentGatewayListResponse();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "GetHouseholdPaymentGateways");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.GetHouseholdPaymentGateways(nGroupID, siteGuid, householdId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new PaymentGatewayListResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public HouseholdPaymentGatewayResponse GetSelectedHouseholdPaymentGateway(string sWSUserName, string sWSPassword, int householdId)
        {
            HouseholdPaymentGatewayResponse response = new HouseholdPaymentGatewayResponse();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "GetSelectedHouseholdPaymentGateway");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.GetSelectedHouseholdPaymentGateway(nGroupID, householdId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new HouseholdPaymentGatewayResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status SetHouseholdChargeID(string sWSUserName, string sWSPassword, string externalIdentifier, int householdID, string chargeID)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SetHouseholdChargeID");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.SetHouseholdChargeID(nGroupID, externalIdentifier, householdID, chargeID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PaymentGatewayChargeIDResponse))]
        public PaymentGatewayChargeIDResponse GetHouseholdChargeID(string sWSUserName, string sWSPassword, string externalIdentifier, int householdID)
        {
            PaymentGatewayChargeIDResponse response = new PaymentGatewayChargeIDResponse();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "GetHouseholdChargeID");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.GetHouseholdChargeID(nGroupID, externalIdentifier, householdID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new PaymentGatewayChargeIDResponse();
                response.ResponseStatus.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.ResponseStatus.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(TransactResult))]
        public TransactResult Transact(string sWSUserName, string sWSPassword, string siteGUID, long householdID, double price, string currency, string userIP,
            string customData, int productID, eTransactionType productType, int contentID, string billingGuid, int paymentGWId, int paymentGatewayHHPaymentMethodId, string adapterData)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGUID != null ? siteGUID : "null";

            TransactResult response = null;

            try
            {

                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "Transact");
                if (nGroupID != 0)
                {
                    return Core.Billing.Module.Transact(nGroupID, siteGUID, householdID, price, currency, userIP, customData, productID, productType, contentID, billingGuid, paymentGWId, paymentGatewayHHPaymentMethodId, 
                        adapterData);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    response = new TransactResult();
                    response.Status = new ApiObjects.Response.Status();
                    response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                    response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                response = new TransactResult();
                response.Status = new ApiObjects.Response.Status();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(TransactResult))]
        public ApiObjects.Response.Status SetPaymentGatewayConfiguration(string sWSUserName, string sWSPassword, int paymentGWId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();


            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SetPaymentGatewayConfiguration");
            if (nGroupID != 0)
            {
                response = Core.Billing.Module.SetPaymentGatewayConfiguration(nGroupID, paymentGWId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PaymentGatewayConfigurationResponse))]
        public PaymentGatewayConfigurationResponse GetPaymentGatewayConfiguration(string sWSUserName, string sWSPassword, string paymentGWExternalId, string intent, List<KeyValuePair> extraParams)
        {
            PaymentGatewayConfigurationResponse response = new PaymentGatewayConfigurationResponse();


            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "GetPaymentGatewayConfiguration");
            if (nGroupID != 0)
            {
                response = Core.Billing.Module.GetPaymentGatewayConfiguration(nGroupID, paymentGWExternalId, intent, extraParams);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new PaymentGatewayConfigurationResponse();
                response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(TransactResult))]
        public UpdatePendingResponse UpdatePendingTransaction(string sWSUserName, string sWSPassword, string paymentGatewayId, int adapterTransactionState, string externalTransactionId,
            string externalStatus, string externalMessage, int failReason, string signature)
        {
            UpdatePendingResponse response = new UpdatePendingResponse();


            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "UpdatePendingTransaction");
            if (nGroupID != 0)
            {
                response = Core.Billing.Module.UpdatePendingTransaction(nGroupID, paymentGatewayId, adapterTransactionState, externalTransactionId, externalStatus, externalMessage, failReason, signature);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(TransactResult))]
        public TransactResult CheckPendingTransaction(string sWSUserName, string sWSPassword,
            long paymentGatewayPendingId, int numberOfRetries, string billingGuid, long paymentGatewayTransactionId, string siteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            TransactResult response = new TransactResult();

            
            int groupId = Utils.GetGroupID(sWSUserName, sWSPassword, "CheckPendingTransaction");
            if (groupId != 0)
            {
                response = Core.Billing.Module.CheckPendingTransaction(groupId, paymentGatewayPendingId, numberOfRetries, billingGuid, paymentGatewayTransactionId, siteGuid);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new TransactResult();
                response.Status = new ApiObjects.Response.Status();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }

            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(TransactResult))]
        public TransactResult VerifyReceipt(string sWSUserName, string sWSPassword, string siteGUID, long householdID, double price, string currency, string userIP,
            string customData, int productID, string productCode, eTransactionType productType, int contentID, string purchaseToken, string paymentGatewayType, string billingGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGUID != null ? siteGUID : "null";

            TransactResult response = null;

            try
            {
                
                int groupId = Utils.GetGroupID(sWSUserName, sWSPassword, "VerifyReceipt");
                if (groupId != 0)
                {
                    response = Core.Billing.Module.VerifyReceipt(groupId, siteGUID, householdID, price, currency, userIP, customData, productID, productCode,
                                               productType, contentID, purchaseToken, paymentGatewayType, billingGuid);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    response = new TransactResult();
                    response.Status = new ApiObjects.Response.Status();
                    response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                    response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                }
            }
            catch (Exception ex)
            {
                response = new TransactResult();
                response.Status = new ApiObjects.Response.Status();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();

                log.Error(string.Empty, ex);
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(TransactResult))]
        public TransactResult ProcessRenewal(string sWSUserName, string sWSPassword, string siteGUID, long householdId, double price, string currency,
            string customData, int productId, string productCode, int paymentNumber, int numberOfPayments, string billingGuid, int gracePeriodMinutes)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGUID != null ? siteGUID : "null";

            TransactResult response = null;

            try
            {

                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "ProcessRenewal");
                if (nGroupID != 0)
                {
                    return Core.Billing.Module.ProcessRenewal(nGroupID, siteGUID, householdId, price, currency, customData, productId, productCode, paymentNumber, numberOfPayments, billingGuid, gracePeriodMinutes);
                }
                else
                {
                    log.ErrorFormat("Error while trying to get group ID. sWSUserName: {0}, sWSPassword: {1}", sWSUserName, sWSPassword);
                    HttpContext.Current.Response.StatusCode = 404;
                    response = new TransactResult();
                    response.Status = new ApiObjects.Response.Status();
                    response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                    response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                }
            }
            catch (Exception ex)
            {
                response = new TransactResult();
                response.Status = new ApiObjects.Response.Status();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                log.Error(string.Empty, ex);
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PaymentGateway))]
        public PaymentGateway GetPaymentGatewayByBillingGuid(string sWSUserName, string sWSPassword, long householdId, string billingGuid)
        {
            PaymentGateway response = null;
            try
            {

                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "GetPaymentGatewayByBillingGuid");
                if (nGroupID != 0)
                {
                    return Core.Billing.Module.GetPaymentGatewayByBillingGuid(nGroupID, householdId, billingGuid);
                }
                else
                {
                    log.ErrorFormat("Error while trying to get group ID or GetPaymentGatewayByBillingGuid implementation. sWSUserName: {0}, sWSPassword: {1}", sWSUserName, sWSPassword);
                    HttpContext.Current.Response.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
            }
            return response;
        }

        [WebMethod]
        public PaymentGatewayItemResponse GeneratePaymentGatewaySharedSecret(string sWSUserName, string sWSPassword, int paymentGatewayId)
        {
            PaymentGatewayItemResponse response = new PaymentGatewayItemResponse();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "GeneratePaymentGatewaySharedSecret");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.GeneratePaymentGatewaySharedSecret(nGroupID, paymentGatewayId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new PaymentGatewayItemResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status SetPartnerConfiguration(string sWSUserName, string sWSPassword, PartnerConfiguration partnerConfig)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SetPartnerConfiguration");
            if (nGroupID != 0)
            {
                return Utils.SetPartnerConfiguration(nGroupID, partnerConfig);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public TransactResult RecordTransaction(string sWSUserName, string sWSPassword, string userId, int householdId, string externalTransactionId, string externalStatus,
            int productId, int productType, string billingGuid, int contentId, string message, int state, int paymentGatewayID, int failReason, string paymentMethod,
            string paymentDetails, string customData, string paymentMethodExternalId)
        {
            TransactResult response = new TransactResult()
            {
                Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
            };

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SetPartnerConfiguration");
            if (nGroupID != 0)
            {
                response = Core.Billing.Module.RecordTransaction(nGroupID, userId, householdId, externalTransactionId, externalStatus, productId, productType, billingGuid, contentId, message, state,
                    paymentGatewayID, failReason, paymentMethod, paymentDetails, customData, paymentMethodExternalId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public ApiObjects.Response.Status SetPaymentGatewayHouseholdPaymentMethod(string sWSUserName, string sWSPassword, string externalIdentifier, int householdID,
            string paymentMethodName, string paymentDetails, string paymentMethodExternalId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SetPaymentGatewayHouseholdPaymentMethod");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.SetPaymentGatewayHouseholdPaymentMethod(nGroupID, externalIdentifier, householdID, paymentMethodName, paymentDetails, paymentMethodExternalId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public PaymentMethodsResponse AddPaymentMethodToPaymentGateway(string sWSUserName, string sWSPassword, int paymentGatewayId, string name, bool allowMultiInstance)
        {
            PaymentMethodsResponse response = new PaymentMethodsResponse()
            {
                Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
            };


            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "AddPaymentMethodToPaymentGateway");
            if (nGroupID != 0)
            {
                response = Core.Billing.Module.AddPaymentMethodToPaymentGateway(nGroupID, paymentGatewayId, name, allowMultiInstance);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public ApiObjects.Response.Status UpdatePaymentGatewayPaymentMethod(string sWSUserName, string sWSPassword, int paymentGatewayId, int paymentMethodId, string name, bool allowMultiInstance)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());


            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "UpdatePaymentGatewayPaymentMethod");
            if (nGroupID != 0)
            {
                response = Core.Billing.Module.UpdatePaymentGatewayPaymentMethod(nGroupID, paymentGatewayId, paymentMethodId, name, allowMultiInstance);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public PaymentMethodResponse UpdatePaymentMethod(string sWSUserName, string sWSPassword, int paymentMethodId, string name, bool allowMultiInstance)
        {
            PaymentMethodResponse response = new PaymentMethodResponse()
            {
                Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
            };


            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "UpdatePaymentMethod");
            if (nGroupID != 0)
            {
                response = Core.Billing.Module.UpdatePaymentMethod(nGroupID, paymentMethodId, name, allowMultiInstance);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public ApiObjects.Response.Status DeletePaymentGatewayPaymentMethod(string sWSUserName, string sWSPassword, int paymentGatewayId, int paymentMethodId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());


            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "DeletePaymentGatewayPaymentMethod");
            if (nGroupID != 0)
            {
                response = Core.Billing.Module.DeletePaymentGatewayPaymentMethod(nGroupID, paymentGatewayId, paymentMethodId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public ApiObjects.Response.Status DeletePaymentMethod(string sWSUserName, string sWSPassword, int paymentMethodId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());


            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "DeletePaymentGatewayPaymentMethod");
            if (nGroupID != 0)
            {
                response = Core.Billing.Module.DeletePaymentMethod(nGroupID, paymentMethodId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public PaymentMethodsResponse GetPaymentGatewayPaymentMethods(string sWSUserName, string sWSPassword, int paymentGatewayId)
        {
            PaymentMethodsResponse response = new PaymentMethodsResponse()
            {
                Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
            };


            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "GetPaymentGatewayPaymentMethods");
            if (nGroupID != 0)
            {
                response = Core.Billing.Module.GetPaymentGatewayPaymentMethods(nGroupID, paymentGatewayId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public ApiObjects.Response.Status SetPaymentMethodHouseholdPaymentGateway(string sWSUserName, string sWSPassword, int paymentGatewayID, string siteGuid, int householdId, int paymentMethodId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SetHouseholdPaymentGateway");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.SetPaymentMethodHouseholdPaymentGateway(nGroupID, paymentGatewayID, siteGuid, householdId, paymentMethodId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status RemovePaymentMethodHouseholdPaymentGateway(string sWSUserName, string sWSPassword, int paymentGatewayID, string siteGuid, int householdId, int paymentMethodId, bool force)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "RemoveHouseholdPaymentGateway");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.RemovePaymentMethodHouseholdPaymentGateway(nGroupID, paymentGatewayID, siteGuid, householdId, paymentMethodId, force);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status UpdateRecordedTransaction(string sWSUserName, string sWSPassword, int householdId, string externalTransactionId, string paymentDetails, string paymentMethod, int paymentGatewayId, string paymentMethodExternalID)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SetPaymentGatewayHouseholdPaymentMethod");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.UpdateRecordedTransaction(nGroupID, householdId, externalTransactionId, paymentDetails, paymentMethod, paymentGatewayId, paymentMethodExternalID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public int AddHouseholdNewPaymentMethod(string sWSUserName, string sWSPassword, string adapterExternalIdentifier, int householdID,
            string paymentMethodName, string paymentDetails, string paymentMethodExternalId)
        {
            int response = 0;

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "AddHouseholdNewPaymentMethod");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.AddHouseholdNewPaymentMethod(nGroupID, adapterExternalIdentifier, householdID, paymentMethodName, paymentDetails, paymentMethodExternalId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PaymentGatewayConfigurationResponse))]
        public PaymentGatewayConfigurationResponse PaymentGatewayInvoke(string sWSUserName, string sWSPassword, int paymentGatewayId, string intent, List<KeyValuePair> extraParams)
        {
            PaymentGatewayConfigurationResponse response = new PaymentGatewayConfigurationResponse();


            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "PaymentGatewayInvoke");
            if (nGroupID != 0)
            {
                response = Core.Billing.Module.PaymentGatewayInvoke(nGroupID, paymentGatewayId, intent, extraParams);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new PaymentGatewayConfigurationResponse();
                response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
            return response;
        }

        [WebMethod]
        public PaymentMethodIdResponse AddPaymentGatewayHouseholdPaymentMethod(string sWSUserName, string sWSPassword, string externalIdentifier, int householdID,
            string paymentMethodName, string paymentDetails, string paymentMethodExternalId)
        {
            PaymentMethodIdResponse response = new PaymentMethodIdResponse();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "AddPaymentGatewayHouseholdPaymentMethod");
            if (nGroupID != 0)
            {
                return Core.Billing.Module.AddPaymentGatewayHouseholdPaymentMethod(nGroupID, externalIdentifier, householdID, paymentMethodName, paymentDetails, paymentMethodExternalId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
            return response;

        }


        [WebMethod]
        public HouseholdPaymentMethodResponse AddPaymentGatewayPaymentMethodToHousehold(string sWSUserName, string sWSPassword, HouseholdPaymentMethod householdPaymentMethod, int householdId)
        {
            HouseholdPaymentMethodResponse response = new HouseholdPaymentMethodResponse()
            {
                Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
            };

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "AddPaymentGatewayPaymentMethodToHousehold");
            if (nGroupID != 0)
            {
                response = Core.Billing.Module.AddPaymentGatewayPaymentMethodToHousehold(nGroupID, householdPaymentMethod, householdId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;

        }

        public ApiObjects.Response.Status RemoveAccount(string sWSUserName, string sWSPassword, int householdId)
        {
            ApiObjects.Response.Status result = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());


            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "RemoveHouseholdPaymentMethods");
            if (nGroupID != 0)
            {
                result = Core.Billing.Module.RemoveAccount(nGroupID, householdId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return result;
        }
        [WebMethod]
        public ApiObjects.Response.Status ChangePaymentDetails(string sWSUserName, string sWSPassword, string billingGuid, long householdId, int paymentGatewayId, int paymentMethodId)            
        {
             ApiObjects.Response.Status result = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());


             Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "ChangePaymentDetails");
             if (nGroupID != 0)
            {
                result = Core.Billing.Module.ChangePaymentDetails(nGroupID, billingGuid, householdId, paymentGatewayId, paymentMethodId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return result;
        }
    }
}
