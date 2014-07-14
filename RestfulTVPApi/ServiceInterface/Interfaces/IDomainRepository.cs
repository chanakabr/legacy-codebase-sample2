using System;
using System.Collections.Generic;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Objects;
using RestfulTVPApi.ServiceModel;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IDomainRepository
    {
        // Gilad: sDomainId - shouldn't it be outside of the init obj? 
        // it's a main parameter in this function
        TVPApiModule.Objects.Responses.DomainResponseObject AddDeviceToDomain(AddDeviceToDomainRequest request);

        TVPApiModule.Objects.Responses.DomainResponseObject AddDomain(AddDomainRequest request);

        TVPApiModule.Objects.Responses.DomainResponseObject AddUserToDomain(AddUserToDomainRequest request);

        List<DeviceDomain> GetDeviceDomains(GetDeviceDomainsRequest request);

        //TVPApiModule.Objects.Responses.DomainResponseObject GetDomainByCoGuid(InitializationObject initObj, string coGuid);

        TVPApiModule.Objects.Responses.DomainResponseObject RemoveDeviceFromDomain(RemoveDeviceFromDomainRequest request);

        TVPApiModule.Objects.Responses.Domain GetDomainInfo(GetDomainInfoRequest reuqest);

        TVPApiModule.Objects.Responses.DomainResponseObject ChangeDeviceDomainStatus(ChangeDeviceDomainStatusRequest request);

        //int[] GetDomainIDsByOperatorCoGuid(InitializationObject initObj, string operatorCoGuid);

        string GetPINForDevice(GetPINForDeviceRequest request);

        //TVPApiModule.Services.ApiDomainsService.DeviceRegistration RegisterDeviceByPIN(InitializationObject initObj, string pin);

        DomainResponseStatus RemoveDomain(RemoveDomainRequest request);

        TVPApiModule.Objects.Responses.DomainResponseObject RemoveUserFromDomain(RemoveUserFromDomainRequest request);

        bool SetDeviceInfo(SetDeviceInfoRequest request);

        TVPApiModule.Objects.Responses.DomainResponseObject SetDomainInfo(SetDomainInfoRequest request);

        TVPApiModule.Objects.Responses.DomainResponseObject SubmitAddUserToDomainRequest(SubmitAddUserToDomainRequest request);

        List<TVPApiModule.Objects.Responses.GroupRule> GetDomainGroupRules(GetDomainGroupRulesRequest request);

        bool SetDomainGroupRule(SetDomainGroupRuleRequest request);

        List<TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse> GetDomainsBillingHistory(GetDomainsBillingHistoryRequest request);

        TVPApiModule.Objects.Responses.DomainResponseObject AddDomainWithCoGuid(AddDomainWithCoGuidRequest request);

        //int GetDomainIDByCoGuid(InitializationObject initObj, string coGuid);

        List<TVPApiModule.Objects.Responses.PermittedMediaContainer> GetDomainPermittedItems(GetDomainPermittedItemsRequest request);

        List<TVPApiModule.Objects.Responses.PermittedSubscriptionContainer> GetDomainPermittedSubscriptions(GetDomainPermittedSubscriptionsRequest request);

        bool SetRuleState(SetRuleStateRequest request);
    }
}