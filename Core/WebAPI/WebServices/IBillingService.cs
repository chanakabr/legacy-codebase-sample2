using System.Collections.Generic;
using System.ServiceModel;
using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.Response;
using Core.Billing;
using KeyValuePair = ApiObjects.KeyValuePair;

namespace WebAPI.WebServices
{
    [ServiceContract(Namespace= "http://billing.tvinci.com/")]
    public interface IBillingService
    {
        [OperationContract]
        int AddHouseholdNewPaymentMethod(string sWSUserName, string sWSPassword, string adapterExternalIdentifier, int householdID, string paymentMethodName, string paymentDetails, string paymentMethodExternalId);
        [OperationContract]
        PaymentGatewayItemResponse AddPaymentGateway(string sWSUserName, string sWSPassword, PaymentGateway pgw);
        [OperationContract]
        PaymentMethodIdResponse AddPaymentGatewayHouseholdPaymentMethod(string sWSUserName, string sWSPassword, string externalIdentifier, int householdID, string paymentMethodName, string paymentDetails, string paymentMethodExternalId);
        [OperationContract]
        HouseholdPaymentMethodResponse AddPaymentGatewayPaymentMethodToHousehold(string sWSUserName, string sWSPassword, HouseholdPaymentMethod householdPaymentMethod, int householdId);
        [OperationContract]
        PaymentGatewayItemResponse AddPaymentGatewaySettings(string sWSUserName, string sWSPassword, int paymentGatewayId, List<PaymentGatewaySettings> settings);
        [OperationContract]
        PaymentMethodsResponse AddPaymentMethodToPaymentGateway(string sWSUserName, string sWSPassword, int paymentGatewayId, string name, bool allowMultiInstance);
        [OperationContract]
        BillingResponse CC_ChargeUser(string sWSUserName, string sWSPassword, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, int nPaymentNumber, int nNumberOfPayments, string sExtraParameters, string sPaymentMethodID, string sEncryptedCVV);
        [OperationContract]
        void CC_DeleteUserCCDigits(string sWSUserName, string sWSPassword, string sSiteGUID);
        [OperationContract]
        BillingResponse CC_DummyChargeUser(string sWSUserName, string sWSPassword, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, int nPaymentNumber, int nNumberOfPayments, string sExtraParameters);
        [OperationContract]
        string CC_GetClientCheckSum(string sWSUserName, string sWSPassword, string sUserIP, string sRandom);
        [OperationContract]
        string CC_GetPopupURL(string sWSUserName, string sWSPassword, double dChargePrice, string sCurrencyCode, string sItemName, string sCustomData, string sPaymentMethod, string sExtraParameters);
        [OperationContract]
        string CC_GetUserCCDigits(string sWSUserName, string sWSPassword, string sSiteGUID);
        [OperationContract]
        BillingResponse Cellular_ChargeUser(string sWSUserName, string sWSPassword, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, int nPaymentNumber, int nNumberOfPayments, string sExtraParameters);
        [OperationContract]
        Status ChangePaymentDetails(string sWSUserName, string sWSPassword, string billingGuid, long householdId, int paymentGatewayId, int paymentMethodId);
        [OperationContract]
        TransactResult CheckPendingTransaction(string sWSUserName, string sWSPassword, long paymentGatewayPendingId, int numberOfRetries, string billingGuid, long paymentGatewayTransactionId, string siteGuid);
        [OperationContract]
        BillingResponse DD_ChargeUser(string sWSUserName, string sWSPassword, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, int nPaymentNumber, int nNumberOfPayments, string sExtraParameters, int nBillingMethod);
        [OperationContract]
        bool DD_RefundUser(string sWSUserName, string sWSPassword, string sPSPReference, string sSiteGuid, double dChargePrice, string sCurrencyCode, long lPurchaseID, int nType, int nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne);
        [OperationContract]
        Status DeleteHouseholdPaymentGateway(string sWSUserName, string sWSPassword, int paymentGatewayId, string siteGuid, int householdId);
        [OperationContract]
        Status DeletePaymentGateway(string sWSUserName, string sWSPassword, int paymentGwID);
        [OperationContract]
        Status DeletePaymentGatewayPaymentMethod(string sWSUserName, string sWSPassword, int paymentGatewayId, int paymentMethodId);
        [OperationContract]
        Status DeletePaymentGWSettings(string sWSUserName, string sWSPassword, int paymentGwID, List<PaymentGatewaySettings> settings);
        [OperationContract]
        Status DeletePaymentMethod(string sWSUserName, string sWSPassword, int paymentMethodId);
        [OperationContract]
        PaymentGatewayItemResponse GeneratePaymentGatewaySharedSecret(string sWSUserName, string sWSPassword, int paymentGatewayId);
        [OperationContract]
        string GetBillingCutomData(string sWSUserName, string sWSPassword);
        [OperationContract]
        string GetClientMerchantSig(string sWSUserName, string sWSPassword, string sParamaters);
        [OperationContract]
        PaymentGatewayChargeIDResponse GetHouseholdChargeID(string sWSUserName, string sWSPassword, string externalIdentifier, int householdID);
        [OperationContract]
        PaymentGatewayListResponse GetHouseholdPaymentGateways(string sWSUserName, string sWSPassword, string siteGuid, int householdId);
        [OperationContract]
        AdyenBillingDetail GetLastBillingTypeUserInfo(string sWSUserName, string sWSPassword, string sSiteGUID);
        [OperationContract]
        AdyenBillingDetail GetLastBillingUserInfo(string sWSUserName, string sWSPassword, string sSiteGUID, int nBillingMethod);
        [OperationContract]
        PaymentGatewaySettingsResponse GetPaymentGatewateSettings(string sWSUserName, string sWSPassword);
        [OperationContract]
        PaymentGatewayResponse GetPaymentGateway(string sWSUserName, string sWSPassword);
        [OperationContract]
        PaymentGateway GetPaymentGatewayByBillingGuid(string sWSUserName, string sWSPassword, long householdId, string billingGuid);
        [OperationContract]
        PaymentGatewayConfigurationResponse GetPaymentGatewayConfiguration(string sWSUserName, string sWSPassword, string paymentGWExternalId, string intent, List<KeyValuePair> extraParams);
        [OperationContract]
        PaymentMethodsResponse GetPaymentGatewayPaymentMethods(string sWSUserName, string sWSPassword, int paymentGatewayId);
        [OperationContract]
        HouseholdPaymentGatewayResponse GetSelectedHouseholdPaymentGateway(string sWSUserName, string sWSPassword, int householdId);
        [OperationContract]
        InAppBillingResponse InApp_ChargeUser(string sWSUserName, string sWSPassword, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, int nPaymentNumber, int nNumberOfPayments, string ReceiptData);
        [OperationContract]
        InAppBillingResponse InApp_ReneweInAppPurchase(string sWSUserName, string sWSPassword, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sCustomData, int nPaymentNumber, int nNumberOfPayments, int nInAppTransactionID);
        [OperationContract]
        Status InsertPaymentGateway(string sWSUserName, string sWSPassword, PaymentGateway pgw);
        [OperationContract]
        Status InsertPaymentGatewaySettings(string sWSUserName, string sWSPassword, int paymentGatewayId, List<PaymentGatewaySettings> settings);
        [OperationContract]
        PaymentGatewayConfigurationResponse PaymentGatewayInvoke(string sWSUserName, string sWSPassword, int paymentGatewayId, string intent, List<KeyValuePair> extraParams);
        [OperationContract]
        TransactResult ProcessRenewal(string sWSUserName, string sWSPassword, string siteGUID, long householdId, double price, string currency, string customData, int productId, string productCode, int paymentNumber, int numberOfPayments, string billingGuid, int gracePeriodMinutes);
        [OperationContract]
        TransactResult RecordTransaction(string sWSUserName, string sWSPassword, string userId, int householdId, string externalTransactionId, string externalStatus, int productId, int productType, string billingGuid, int contentId, string message, int state, int paymentGatewayID, int failReason, string paymentMethod, string paymentDetails, string customData, string paymentMethodExternalId);
        [OperationContract]
        Status RemoveAccount(string sWSUserName, string sWSPassword, int householdId);
        [OperationContract]
        Status RemovePaymentMethodHouseholdPaymentGateway(string sWSUserName, string sWSPassword, int paymentGatewayID, string siteGuid, int householdId, int paymentMethodId, bool force);
        [OperationContract]
        bool SendPurchaseMail(string sWSUserName, string sWSPassword, string sSiteGUID, string sPaymentMethod, string sItemName, int nBillingTransID, string stotalAmount, string scurrency, string sExternalNum);
        [OperationContract]
        Status SetHouseholdChargeID(string sWSUserName, string sWSPassword, string externalIdentifier, int householdID, string chargeID);
        [OperationContract]
        Status SetHouseholdPaymentGateway(string sWSUserName, string sWSPassword, int paymentGwID, string siteGuid, int householdId);
        [OperationContract]
        Status SetPartnerConfiguration(string sWSUserName, string sWSPassword, PartnerConfiguration partnerConfig);
        [OperationContract]
        Status SetPaymentGateway(string sWSUserName, string sWSPassword, int paymentGatewayId, PaymentGateway paymentGateway);
        [OperationContract]
        Status SetPaymentGatewayConfiguration(string sWSUserName, string sWSPassword, int paymentGWId);
        [OperationContract]
        Status SetPaymentGatewayHouseholdPaymentMethod(string sWSUserName, string sWSPassword, string externalIdentifier, int householdID, string paymentMethodName, string paymentDetails, string paymentMethodExternalId);
        [OperationContract]
        Status SetPaymentGWSettings(string sWSUserName, string sWSPassword, int paymentGWID, List<PaymentGatewaySettings> settings);
        [OperationContract]
        Status SetPaymentMethodHouseholdPaymentGateway(string sWSUserName, string sWSPassword, int paymentGatewayID, string siteGuid, int householdId, int paymentMethodId);
        [OperationContract]
        BillingResponse SMS_CheckCode(string sWSUserName, string sWSPassword, string sSiteGUID, string sCellPhone, string sSMSCode, string sReferenceCode);
        [OperationContract]
        BillingResponse SMS_SendCode(string sWSUserName, string sWSPassword, string sSiteGUID, string sCellPhone, string sReferenceCode, string sExtraParameters);
        [OperationContract]
        TransactResult Transact(string sWSUserName, string sWSPassword, string siteGUID, long householdID, double price, string currency, string userIP, string customData, int productID, eTransactionType productType, int contentID, string billingGuid, int paymentGWId, int paymentGatewayHHPaymentMethodId, string adapterData);
        [OperationContract]
        PaymentGatewayItemResponse UpdatePaymentGateway(string sWSUserName, string sWSPassword, int paymentGatewayId, PaymentGateway paymentGateway);
        [OperationContract]
        Status UpdatePaymentGatewayPaymentMethod(string sWSUserName, string sWSPassword, int paymentGatewayId, int paymentMethodId, string name, bool allowMultiInstance);
        [OperationContract]
        PaymentMethodResponse UpdatePaymentMethod(string sWSUserName, string sWSPassword, int paymentMethodId, string name, bool allowMultiInstance);
        [OperationContract]
        UpdatePendingResponse UpdatePendingTransaction(string sWSUserName, string sWSPassword, string paymentGatewayId, int adapterTransactionState, string externalTransactionId, string externalStatus, string externalMessage, int failReason, string signature);
        [OperationContract]
        bool UpdatePurchaseIDInBilling(string sWSUserName, string sWSPassword, long purchaseID, long billingRefTransactionID);
        [OperationContract]
        Status UpdateRecordedTransaction(string sWSUserName, string sWSPassword, int householdId, string externalTransactionId, string paymentDetails, string paymentMethod, int paymentGatewayId, string paymentMethodExternalID);
        [OperationContract]
        TransactResult VerifyReceipt(string sWSUserName, string sWSPassword, string siteGUID, long householdID, double price, string currency, string userIP, string customData, int productID, string productCode, eTransactionType productType, int contentID, string purchaseToken, string paymentGatewayType, string billingGuid);
    }
}