using System.ServiceModel;
using TVPApi;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.TvinciPlatform.Billing;

namespace TVPApiServices
{
    [ServiceContract]
    public interface IBillingService
    {
        [OperationContract]
        AdyenBillingDetail GetLastBillingUserInfo(InitializationObject initObj, int billingMethod);

        [OperationContract]
        string GetClientMerchantSig(InitializationObject initObj, string sParamaters);

        [OperationContract]
        AdyenBillingDetail GetLastBillingTypeUserInfo(InitializationObject initObj, string sSiteGuid);

        [OperationContract]
        TVPApiModule.Objects.Responses.Billing.PaymentGatewayChargeIdResponse GetChargeID(InitializationObject initObj, string externalIdentifier, int domainId);

        [OperationContract]
        ClientResponseStatus SetChargeID(InitializationObject initObj, string externalIdentifier, int domainId, string chargeId);       
    }
}
