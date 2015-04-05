using System;
using System.Collections.Generic;
//using TVPApiModule.Objects.Responses;
using TVPApiModule.Objects;
using RestfulTVPApi.ServiceModel;
using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.Objects.Responses.Enums;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IDomainRepository
    {
        // Gilad: sDomainId - shouldn't it be outside of the init obj? 
        // it's a main parameter in this function
        DomainResponseObject AddDeviceToDomain(AddDeviceToDomainRequest request);

        DomainResponseObject AddDomain(AddDomainRequest request);

        DomainResponseObject AddUserToDomain(AddUserToDomainRequest request);

        List<DeviceDomain> GetDeviceDomains(GetDeviceDomainsRequest request);

        //TVPApiModule.Objects.Responses.DomainResponseObject GetDomainByCoGuid(InitializationObject initObj, string coGuid);

        DomainResponseObject RemoveDeviceFromDomain(RemoveDeviceFromDomainRequest request);

        DomainResponseObject GetDomainInfo(GetDomainInfoRequest reuqest);

        DomainResponseObject ChangeDeviceDomainStatus(ChangeDeviceDomainStatusRequest request);

        //int[] GetDomainIDsByOperatorCoGuid(InitializationObject initObj, string operatorCoGuid);

        string GetPINForDevice(GetPINForDeviceRequest request);

        //TVPApiModule.Services.ApiDomainsService.DeviceRegistration RegisterDeviceByPIN(InitializationObject initObj, string pin);

        DomainResponseStatus RemoveDomain(RemoveDomainRequest request);

        DomainResponseObject RemoveUserFromDomain(RemoveUserFromDomainRequest request);

        bool SetDeviceInfo(SetDeviceInfoRequest request);

        DomainResponseObject SetDomainInfo(SetDomainInfoRequest request);

        DomainResponseObject SubmitAddUserToDomainRequest(SubmitAddUserToDomainRequest request);

        List<GroupRule> GetDomainGroupRules(GetDomainGroupRulesRequest request);

        bool SetDomainGroupRule(SetDomainGroupRuleRequest request);

        List<DomainBillingTransactionsResponse> GetDomainsBillingHistory(GetDomainsBillingHistoryRequest request);

        DomainResponseObject AddDomainWithCoGuid(AddDomainWithCoGuidRequest request);

        //int GetDomainIDByCoGuid(InitializationObject initObj, string coGuid);

        List<PermittedMediaContainer> GetDomainPermittedItems(GetDomainPermittedItemsRequest request);

        List<PermittedSubscriptionContainer> GetDomainPermittedSubscriptions(GetDomainPermittedSubscriptionsRequest request);

        bool SetRuleState(SetRuleStateRequest request);

        bool SetDomainRestriction(SetDomainRestrictionRequest request);

        DomainResponseObject SubmitAddDeviceToDomainRequest(SubmitAddDeviceToDomainRequest request);

        NetworkResponseObject AddHomeNetworkToDomain(AddHomeNetworkToDomainRequest request);

        NetworkResponseObject UpdateDomainHomeNetwork(UpdateDomainHomeNetworkRequest request);

        NetworkResponseObject RemoveDomainHomeNetwork(RemoveDomainHomeNetworkRequest request);

        List<HomeNetwork> GetDomainHomeNetworks(GetDomainHomeNetworksRequest request);

        DeviceResponseObject GetDeviceInfo(GetDeviceInfoRequest request);

        DomainResponseObject ChangeDomainMaster(ChangeDomainMasterRequest request);

        DomainResponseObject ResetDomainFrequency(ResetDomainFrequencyRequest request);

        List<PermittedCollectionContainer> GetDomainPermittedCollections(GetDomainPermittedCollectionsRequest request);

        List<string> GetDomainUsersList(GetDomainUsersListRequest request);
    }
}