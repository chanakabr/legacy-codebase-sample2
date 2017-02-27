using ApiObjects;
using ApiObjects.Billing;
using Core.Billing;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Core.Billing
{
    public class Module
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        
        public static string GetBillingCutomData(int nGroupID)
        {
            BaseBilling t = null;
            Utils.GetBaseBillingImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.BillingCustomData();
            }
            else
            {
                return null;
            }

        }

        
        public static string CC_GetUserCCDigits(int nGroupID, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseCreditCard t = null;
            Utils.GetBaseCreditCardImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetUserCreditCardDigits(sSiteGUID);
            }
            else
            {
                return null;
            }
        }

        
        public static void CC_DeleteUserCCDigits(int nGroupID, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseCreditCard t = null;
            Utils.GetBaseCreditCardImpl(ref t, nGroupID);
            if (t != null)
            {
                t.DeleteUserCreditCardDigits(sSiteGUID);
            }
        }

        
        public static string CC_GetClientCheckSum(int nGroupID, string sUserIP, string sRandom)
        {
            BaseCreditCard t = null;
            Utils.GetBaseCreditCardImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetClientCheckSum(sUserIP, sRandom);
            }
            else
            {
                return null;
            }
        }

        
        public static string GetClientMerchantSig(int nGroupID, string sParamaters)
        {
            BaseCreditCard t = null;
            Utils.GetBaseCreditCardImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetClientMerchantSig(sParamaters);
            }
            else
            {
                return null;
            }
        }

        
        public static string CC_GetPopupURL(int nGroupID, double dChargePrice, string sCurrencyCode,
            string sItemName, string sCustomData, string sPaymentMethod, string sExtraParameters)
        {
            BasePopup t = null;
            Utils.GetBasePopupImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetPopupMethodURL(dChargePrice, sCurrencyCode, sItemName, sCustomData, sPaymentMethod, sExtraParameters);
            }
            else
            {
                return null;
            }
        }

        
        public static AdyenBillingDetail GetLastBillingUserInfo(int nGroupID, string sSiteGUID, int nBillingMethod)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseDirectDebit t = null;
            Utils.GetBaseDirectDebitImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetLastBillingUserInfo(sSiteGUID, nBillingMethod);
            }
            else
            {
                return null;
            }
        }


        
        public static AdyenBillingDetail GetLastBillingTypeUserInfo(int nGroupID, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            return Utils.GetLastBillingTypeUserInfo(nGroupID, sSiteGUID);
        }


        
        public static BillingResponse CC_ChargeUser(int nGroupID, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string sExtraParameters, string sPaymentMethodID, string sEncryptedCVV)
        {
            BaseCreditCard oCreditCard = null;
            Utils.GetBaseCreditCardImpl(ref oCreditCard, nGroupID, sPaymentMethodID, sEncryptedCVV);
            if (oCreditCard != null)
            {
                log.Debug("group_id found - " + TVinciShared.PageUtils.GetCallerIP());
                return oCreditCard.ChargeUser(sSiteGUID, dChargePrice, sCurrencyCode, sUserIP, sCustomData, nPaymentNumber, nNumberOfPayments, sExtraParameters);
            }
            else
            {
                return null;
            }
        }

        
        public static bool UpdatePurchaseIDInBilling(int nGroupID, long purchaseID, long billingRefTransactionID)
        {
            BaseCreditCard oCreditCard = null;
            Utils.GetBaseCreditCardImpl(ref oCreditCard, nGroupID);
            if (oCreditCard != null)
            {
                log.Error("group_id found " + TVinciShared.PageUtils.GetCallerIP());
                return oCreditCard.UpdatePurchaseIDInBillingTable(purchaseID, billingRefTransactionID);
            }
            else
            {
                return false;
            }
        }

        
        public static BillingResponse DD_ChargeUser(int nGroupID, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string sExtraParameters, int nBillingMethod)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseDirectDebit t = null;
            Utils.GetBaseDirectDebitImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ChargeUser(sSiteGUID, dChargePrice, sCurrencyCode, sUserIP, sCustomData, nPaymentNumber, nNumberOfPayments, sExtraParameters, nBillingMethod);
            }
            else
            {
                return null;
            }
        }

        
        public static InAppBillingResponse InApp_ChargeUser(int nGroupID, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string ReceiptData)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseInAppPurchase t = null;
            Utils.GetBaseInAppPurchaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ChargeUser(sSiteGUID, dChargePrice, sCurrencyCode, sUserIP, sCustomData, nPaymentNumber, nNumberOfPayments, ReceiptData);
            }
            else
            {
                return null;
            }
        }
        
        public static InAppBillingResponse InApp_ReneweInAppPurchase(int nGroupID, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, int nInAppTransactionID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseInAppPurchase t = null;
            Utils.GetBaseInAppPurchaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ReneweInAppPurchase(sSiteGUID, dChargePrice, sCurrencyCode, sCustomData, nPaymentNumber, nNumberOfPayments, nInAppTransactionID);
            }
            else
            {
                return null;
            }
        }



        
        public static bool DD_RefundUser(int nGroupID, string sPSPReference, string sSiteGuid, double dChargePrice, string sCurrencyCode, long lPurchaseID, int nType, int nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            BaseDirectDebit t = null;
            Utils.GetBaseDirectDebitImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.RefundPayment(sPSPReference, sSiteGuid, nGroupID, dChargePrice, sCurrencyCode, lPurchaseID, nType, nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne);
            }
            else
            {
                return false;
            }
        }

        
        public static BillingResponse CC_DummyChargeUser(int nGroupID, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string sExtraParameters)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseCreditCard t = null;
            Utils.GetBaseCreditCardImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ChargeUser(sSiteGUID, dChargePrice, sCurrencyCode, sUserIP, sCustomData, nPaymentNumber, nNumberOfPayments, sExtraParameters);
            }
            else
            {
                return null;
            }
        }

        
        public static BillingResponse SMS_SendCode(int nGroupID, string sSiteGUID, string sCellPhone, string sReferenceCode, string sExtraParameters)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseSMS t = null;
            Utils.GetBaseSMSImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.SendCode(sSiteGUID, sCellPhone, sReferenceCode, sExtraParameters);
            }
            else
            {
                return null;
            }
        }

        
        public static BillingResponse SMS_CheckCode(int nGroupID, string sSiteGUID, string sCellPhone, string sSMSCode, string sReferenceCode)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseSMS t = null;
            Utils.GetBaseSMSImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.CheckCode(sSiteGUID, sCellPhone, sSMSCode, sReferenceCode);
            }
            else
            {
                return null;
            }
        }
        
        public static bool SendPurchaseMail(int nGroupID, string sSiteGUID, string sPaymentMethod, string sItemName, Int32 nBillingTransID, string stotalAmount, string scurrency, string sExternalNum)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseBilling t = null;
            Utils.GetBaseBillingImpl(ref t, nGroupID);
            if (t != null)
            {
                Utils.SendMail(sPaymentMethod, sItemName, sSiteGUID, nBillingTransID, stotalAmount, scurrency, sExternalNum, nGroupID, string.Empty, eMailTemplateType.Purchase);
                return true;
            }
            else
            {
                return false;
            }
        }

        
        public static BillingResponse Cellular_ChargeUser(int nGroupID, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string sExtraParameters)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseCellularCreditCard t = null;
            Utils.GetBaseCellularCreditCardImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ChargeUser(sSiteGUID, dChargePrice, sCurrencyCode, sUserIP, sCustomData, nPaymentNumber, nNumberOfPayments, sExtraParameters);
            }
            else
            {
                return null;
            }
        }

        
        public static PaymentGatewaySettingsResponse GetPaymentGatewateSettings(int nGroupID)
        {
            PaymentGatewaySettingsResponse response = new PaymentGatewaySettingsResponse();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.GetPaymentGatewateSettings();
            }
            else
            {
                response = new PaymentGatewaySettingsResponse();
                response.resp.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.resp.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static ApiObjects.Response.Status SetPaymentGateway(int nGroupID, int paymentGatewayId, PaymentGateway paymentGateway)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.SetPaymentGateway(paymentGatewayId, paymentGateway).Status;
            }
            else
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static PaymentGatewayItemResponse UpdatePaymentGateway(int nGroupID, int paymentGatewayId, PaymentGateway paymentGateway)
        {
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.SetPaymentGateway(paymentGatewayId, paymentGateway);
            }
            else
            {
                PaymentGatewayItemResponse response = new PaymentGatewayItemResponse();
                response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                return response;
            }
        }

        
        public static ApiObjects.Response.Status SetPaymentGWSettings(int nGroupID, int paymentGWID, List<PaymentGatewaySettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.SetPaymentGWSettings(paymentGWID, settings);
            }
            else
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static ApiObjects.Response.Status DeletePaymentGateway(int nGroupID, int paymentGwID)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.DeletePaymentGateway(paymentGwID);
            }
            else
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static ApiObjects.Response.Status DeletePaymentGWSettings(int nGroupID, int paymentGwID, List<PaymentGatewaySettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.DeletePaymentGatewaySettings(paymentGwID, settings);
            }
            else
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static ApiObjects.Response.Status InsertPaymentGateway(int nGroupID, PaymentGateway pgw)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.InsertPaymentGateway(pgw).Status;
            }
            else
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static PaymentGatewayItemResponse AddPaymentGateway(int nGroupID, PaymentGateway pgw)
        {
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.InsertPaymentGateway(pgw);
            }
            else
            {
                PaymentGatewayItemResponse response = new PaymentGatewayItemResponse();
                response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                return response;
            }
        }

        
        [Obsolete]
        public static ApiObjects.Response.Status InsertPaymentGatewaySettings(int nGroupID, int paymentGatewayId, List<PaymentGatewaySettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.InsertPaymentGatewaySettings(paymentGatewayId, settings).Status;
            }
            else
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static PaymentGatewayItemResponse AddPaymentGatewaySettings(int nGroupID, int paymentGatewayId, List<PaymentGatewaySettings> settings)
        {
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.InsertPaymentGatewaySettings(paymentGatewayId, settings);
            }
            else
            {
                PaymentGatewayItemResponse response = new PaymentGatewayItemResponse();
                response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                return response;
            }
        }

        
        public static PaymentGatewayResponse GetPaymentGateway(int nGroupID)
        {
            PaymentGatewayResponse response = new PaymentGatewayResponse();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.GetPaymentGateway();
            }
            else
            {
                response = new PaymentGatewayResponse();
                response.resp.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.resp.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static ApiObjects.Response.Status SetHouseholdPaymentGateway(int nGroupID, int paymentGwID, string siteGuid, int householdId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.SetHouseholdPaymentGateway(paymentGwID, siteGuid, householdId);
            }
            else
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static ApiObjects.Response.Status DeleteHouseholdPaymentGateway(int nGroupID, int paymentGatewayId, string siteGuid, int householdId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.DeleteHouseholdPaymentGateway(paymentGatewayId, siteGuid, householdId);
            }
            else
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static PaymentGatewayListResponse GetHouseholdPaymentGateways(int nGroupID, string siteGuid, int householdId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            PaymentGatewayListResponse response = new PaymentGatewayListResponse();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.GetHouseholdPaymentGateways(siteGuid, householdId);
            }
            else
            {
                response = new PaymentGatewayListResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static HouseholdPaymentGatewayResponse GetSelectedHouseholdPaymentGateway(int nGroupID, int householdId)
        {
            HouseholdPaymentGatewayResponse response = new HouseholdPaymentGatewayResponse();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.GetSelectedHouseholdPaymentGateway(householdId);
            }
            else
            {
                response = new HouseholdPaymentGatewayResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static ApiObjects.Response.Status SetHouseholdChargeID(int nGroupID, string externalIdentifier, int householdID, string chargeID)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.SetHouseholdChargeID(externalIdentifier, householdID, chargeID);
            }
            else
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static PaymentGatewayChargeIDResponse GetHouseholdChargeID(int nGroupID, string externalIdentifier, int householdID)
        {
            PaymentGatewayChargeIDResponse response = new PaymentGatewayChargeIDResponse();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.GetHouseholdChargeID(externalIdentifier, householdID);
            }
            else
            {
                response = new PaymentGatewayChargeIDResponse();
                response.ResponseStatus.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.ResponseStatus.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static TransactResult Transact(int nGroupID, string siteGUID, long householdID, double price, string currency, string userIP,
            string customData, int productID, eTransactionType productType, int contentID, string billingGuid, int paymentGWId, int paymentGatewayHHPaymentMethodId, string adapterData)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGUID != null ? siteGUID : "null";

            TransactResult response = null;

            try
            {
                BasePaymentGateway t = new BasePaymentGateway(nGroupID);
                if (t != null)
                {
                    return t.Transact(siteGUID, householdID, price, currency, userIP, customData, productID, productType, contentID, billingGuid, paymentGWId, paymentGatewayHHPaymentMethodId,
                        adapterData);
                }
                else
                {
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

        
        public static ApiObjects.Response.Status SetPaymentGatewayConfiguration(int nGroupID, int paymentGWId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                response = t.SetPaymentGatewayConfiguration(paymentGWId);
            }
            else
            {
                response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
            return response;
        }

        
        public static PaymentGatewayConfigurationResponse GetPaymentGatewayConfiguration(int nGroupID, string paymentGWExternalId, string intent, List<KeyValuePair> extraParams)
        {
            PaymentGatewayConfigurationResponse response = new PaymentGatewayConfigurationResponse();

            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                response = t.GetPaymentGatewayConfiguration(paymentGWExternalId, intent, extraParams);
            }
            else
            {
                response = new PaymentGatewayConfigurationResponse();
                response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
            return response;
        }

        
        public static UpdatePendingResponse UpdatePendingTransaction(int nGroupID, string paymentGatewayId, int adapterTransactionState, string externalTransactionId,
            string externalStatus, string externalMessage, int failReason, string signature)
        {
            UpdatePendingResponse response = new UpdatePendingResponse();

            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                response = t.UpdatePendingTransaction(paymentGatewayId, adapterTransactionState, externalTransactionId, externalStatus, externalMessage, failReason, signature);
            }
            else
            {
                response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
            return response;
        }

        
        public static TransactResult CheckPendingTransaction(int nGroupID,
            long paymentGatewayPendingId, int numberOfRetries, string billingGuid, long paymentGatewayTransactionId, string siteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            TransactResult response = new TransactResult();

            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                response = t.CheckPendingTransaction(paymentGatewayPendingId, numberOfRetries, billingGuid, paymentGatewayTransactionId, siteGuid);
            }
            else
            {
                response = new TransactResult();
                response.Status = new ApiObjects.Response.Status();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }

            return response;
        }

        
        public static TransactResult VerifyReceipt(int nGroupID, string siteGUID, long householdID, double price, string currency, string userIP,
            string customData, int productID, string productCode, eTransactionType productType, int contentID, string purchaseToken, string paymentGatewayType, string billingGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGUID != null ? siteGUID : "null";

            TransactResult response = null;

            try
            {
                BasePaymentGateway t = new BasePaymentGateway(nGroupID);
                if (t != null)
                {
                    response = t.VerifyReceipt(siteGUID, householdID, price, currency, userIP, customData, productID, productCode,
                                               productType, contentID, purchaseToken, paymentGatewayType, billingGuid);
                }
                else
                {
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

        
        public static TransactResult ProcessRenewal(int nGroupID, string siteGUID, long householdId, double price, string currency,
            string customData, int productId, string productCode, int paymentNumber, int numberOfPayments, string billingGuid, int gracePeriodMinutes)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGUID != null ? siteGUID : "null";

            TransactResult response = null;

            try
            {
                BasePaymentGateway t = new BasePaymentGateway(nGroupID);
                if (t != null)
                {
                    return t.ProcessRenewal(siteGUID, householdId, price, currency, customData, productId, productCode, paymentNumber, numberOfPayments, billingGuid, gracePeriodMinutes);
                }
                else
                {
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

        
        public static PaymentGateway GetPaymentGatewayByBillingGuid(int nGroupID, long householdId, string billingGuid)
        {
            PaymentGateway response = null;
            try
            {
                BasePaymentGateway t = new BasePaymentGateway(nGroupID);
                if (t != null)
                {
                    return t.GetPaymentGatewayByBillingGuid(householdId, billingGuid);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
            }
            return response;
        }

        
        public static PaymentGatewayItemResponse GeneratePaymentGatewaySharedSecret(int nGroupID, int paymentGatewayId)
        {
            PaymentGatewayItemResponse response = new PaymentGatewayItemResponse();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.GeneratePaymentGatewaySharedSecret(paymentGatewayId);
            }
            else
            {
                response = new PaymentGatewayItemResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static ApiObjects.Response.Status SetPartnerConfiguration(int nGroupID, PartnerConfiguration partnerConfig)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return Utils.SetPartnerConfiguration(nGroupID, partnerConfig);
            }
            else
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static TransactResult RecordTransaction(int nGroupID, string userId, int householdId, string externalTransactionId, string externalStatus,
            int productId, int productType, string billingGuid, int contentId, string message, int state, int paymentGatewayID, int failReason, string paymentMethod,
            string paymentDetails, string customData, string paymentMethodExternalId)
        {
            TransactResult response = new TransactResult()
            {
                Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
            };
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                response = t.RecordTransaction(userId, householdId, externalTransactionId, externalStatus, productId, productType, billingGuid, contentId, message, state,
                    paymentGatewayID, failReason, paymentMethod, paymentDetails, customData, paymentMethodExternalId);
            }

            return response;
        }

        
        public static ApiObjects.Response.Status SetPaymentGatewayHouseholdPaymentMethod(int nGroupID, string externalIdentifier, int householdID,
            string paymentMethodName, string paymentDetails, string paymentMethodExternalId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                int pghhpmId = 0;
                return t.SetPaymentGatewayHouseholdPaymentMethod(externalIdentifier, householdID, paymentMethodName, paymentDetails, paymentMethodExternalId, out pghhpmId);
            }
            else
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static PaymentMethodsResponse AddPaymentMethodToPaymentGateway(int nGroupID, int paymentGatewayId, string name, bool allowMultiInstance)
        {
            PaymentMethodsResponse response = new PaymentMethodsResponse()
            {
                Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
            };

            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                response = t.AddPaymentMethodToPaymentGateway(paymentGatewayId, name, allowMultiInstance);
            }

            return response;
        }

        
        public static ApiObjects.Response.Status UpdatePaymentGatewayPaymentMethod(int nGroupID, int paymentGatewayId, int paymentMethodId, string name, bool allowMultiInstance)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());

            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                response = t.UpdatePaymentGatewayPaymentMethod(paymentGatewayId, paymentMethodId, name, allowMultiInstance);
            }

            return response;
        }

        
        public static PaymentMethodResponse UpdatePaymentMethod(int nGroupID, int paymentMethodId, string name, bool allowMultiInstance)
        {
            PaymentMethodResponse response = new PaymentMethodResponse()
            {
                Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
            };

            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                response = t.UpdatePaymentMethod(paymentMethodId, name, allowMultiInstance);
            }

            return response;
        }

        
        public static ApiObjects.Response.Status DeletePaymentGatewayPaymentMethod(int nGroupID, int paymentGatewayId, int paymentMethodId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());

            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                response = t.DeletePaymentGatewayPaymentMethod(paymentGatewayId, paymentMethodId);
            }

            return response;
        }

        
        public static ApiObjects.Response.Status DeletePaymentMethod(int nGroupID, int paymentMethodId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());

            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                response = t.DeletePaymentMethod(paymentMethodId);
            }

            return response;
        }

        
        public static PaymentMethodsResponse GetPaymentGatewayPaymentMethods(int nGroupID, int paymentGatewayId)
        {
            PaymentMethodsResponse response = new PaymentMethodsResponse()
            {
                Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
            };

            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                response = t.GetPaymentGatewayPaymentMethods(paymentGatewayId);
            }

            return response;
        }

        
        public static ApiObjects.Response.Status SetPaymentMethodHouseholdPaymentGateway(int nGroupID, int paymentGatewayID, string siteGuid, int householdId, int paymentMethodId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.SetPaymentMethodHouseholdPaymentGateway(paymentGatewayID, siteGuid, householdId, paymentMethodId);
            }
            else
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static ApiObjects.Response.Status RemovePaymentMethodHouseholdPaymentGateway(int nGroupID, int paymentGatewayID, string siteGuid, int householdId, int paymentMethodId, bool force)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.RemovePaymentMethodHouseholdPaymentGateway(paymentGatewayID, siteGuid, householdId, paymentMethodId, force);
            }
            else
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static ApiObjects.Response.Status UpdateRecordedTransaction(int nGroupID, int householdId, string externalTransactionId, string paymentDetails, string paymentMethod, int paymentGatewayId, string paymentMethodExternalID)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                return t.UpdateRecordedTransaction(householdId, externalTransactionId, paymentDetails, paymentMethod, paymentGatewayId, paymentMethodExternalID);
            }
            else
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        
        public static int AddHouseholdNewPaymentMethod(int nGroupID, string adapterExternalIdentifier, int householdID,
            string paymentMethodName, string paymentDetails, string paymentMethodExternalId)
        {
            int response = 0;
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                ApiObjects.Response.Status status = t.SetPaymentGatewayHouseholdPaymentMethod(adapterExternalIdentifier, householdID, paymentMethodName, paymentDetails, paymentMethodExternalId, out response);
                if (status.Code != 0)
                {
                    log.DebugFormat("AddHouseholdNewPaymentMethod for adapterExternalIdentifier:{0}, householdID:{1}, paymentMethodName:{2}, paymentMethodExternalId:{3}. SetPaymentGatewayHouseholdPaymentMethod return statusCode: {4}",
                        adapterExternalIdentifier,
                        householdID,
                        paymentMethodName,
                        paymentMethodExternalId,
                        status.Code);
                }
                return response;
            }
            else
            {
                return response;
            }
        }

        
        public static PaymentGatewayConfigurationResponse PaymentGatewayInvoke(int nGroupID, int paymentGatewayId, string intent, List<KeyValuePair> extraParams)
        {
            PaymentGatewayConfigurationResponse response = new PaymentGatewayConfigurationResponse();

            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                response = t.PaymentGatewayInvoke(paymentGatewayId, intent, extraParams);
            }
            else
            {
                response = new PaymentGatewayConfigurationResponse();
                response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
            return response;
        }

        
        public static PaymentMethodIdResponse AddPaymentGatewayHouseholdPaymentMethod(int nGroupID, string externalIdentifier, int householdID,
            string paymentMethodName, string paymentDetails, string paymentMethodExternalId)
        {
            PaymentMethodIdResponse response = new PaymentMethodIdResponse();
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                int paymentMethodId = 0;
                response.Status = t.SetPaymentGatewayHouseholdPaymentMethod(externalIdentifier, householdID, paymentMethodName, paymentDetails, paymentMethodExternalId, out paymentMethodId);
                if (response.Status != null && response.Status.Code == (int)ApiObjects.Response.eResponseStatus.OK)
                {
                    response.PaymentMethodId = paymentMethodId;
                }
            }
            else
            {
                response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
            return response;
        }


        
        public static HouseholdPaymentMethodResponse AddPaymentGatewayPaymentMethodToHousehold(int nGroupID, HouseholdPaymentMethod householdPaymentMethod, int householdId)
        {
            HouseholdPaymentMethodResponse response = new HouseholdPaymentMethodResponse()
            {
                Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
            };
            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                response = t.AddPaymentGatewayPaymentMethodToHousehold(householdPaymentMethod, householdId);
            }
            return response;
        }

        public static ApiObjects.Response.Status RemoveAccount(int nGroupID, int householdId)
        {
            ApiObjects.Response.Status result = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());

            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                result = t.RemoveAccount(householdId);
            }

            return result;
        }
        
        public static ApiObjects.Response.Status ChangePaymentDetails(int nGroupID, string billingGuid, long householdId, int paymentGatewayId, int paymentMethodId)
        {
            ApiObjects.Response.Status result = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());

            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                result = t.ChangePaymentDetails(billingGuid, householdId, paymentGatewayId, paymentMethodId);
            }

            return result;
        }

        public static List<PaymentDetails> GetPaymentDetails(int nGroupID, List<string> billingGuids)
        {
            List<PaymentDetails> result = new List<PaymentDetails>();

            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                result = t.GetPaymentDetails(billingGuids);
            }

            return result;
        }

        public static ApiObjects.Response.Status GetPaymentGatewayVerificationStatus(int nGroupID, string billingGuid)
        {
            ApiObjects.Response.Status result = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());

            BasePaymentGateway t = new BasePaymentGateway(nGroupID);
            if (t != null)
            {
                result = t.GetPaymentGatewayVerificationStatus(billingGuid);
            }

            return result;
        }
    }
}
