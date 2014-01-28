using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Services;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IDomainRepository
    {
        // Gilad: sDomainId - shouldn't it be outside of the init obj? 
        // it's a main parameter in this function
        TVPApiModule.Objects.Responses.DomainResponseObject AddDeviceToDomain(InitializationObject initObj, string sDeviceName, int iDeviceBrandID);

        TVPApiModule.Objects.Responses.DomainResponseObject AddDomain(InitializationObject initObj, string domainName, string domainDesc, int masterGuid);

        TVPApiModule.Objects.Responses.DomainResponseObject AddUserToDomain(InitializationObject initObj, string addedUserGuid, int domainId);

        IEnumerable<ApiDomainsService.DeviceDomain> GetDeviceDomains(InitializationObject initObj, string udId);

        //TVPApiModule.Objects.Responses.DomainResponseObject GetDomainByCoGuid(InitializationObject initObj, string coGuid);

        TVPApiModule.Objects.Responses.DomainResponseObject RemoveDeviceFromDomain(InitializationObject initObj, int domainID, string udId, string sDeviceName, int iDeviceBrandID);

        TVPApiModule.Objects.Responses.Domain GetDomainInfo(InitializationObject initObj, int domainId);

        TVPApiModule.Objects.Responses.DomainResponseObject ChangeDeviceDomainStatus(InitializationObject initObj, bool bActive);

        //int[] GetDomainIDsByOperatorCoGuid(InitializationObject initObj, string operatorCoGuid);

        string GetPINForDevice(InitializationObject initObj, int devBrandID);

        //TVPApiModule.Services.ApiDomainsService.DeviceRegistration RegisterDeviceByPIN(InitializationObject initObj, string pin);

        DomainResponseStatus RemoveDomain(InitializationObject initObj, int domainId);

        TVPApiModule.Objects.Responses.DomainResponseObject RemoveUserFromDomain(InitializationObject initObj, string userGuidToRemove, int domainId);

        bool SetDeviceInfo(InitializationObject initObj, string udid, string deviceName);

        TVPApiModule.Objects.Responses.DomainResponseObject SetDomainInfo(InitializationObject initObj, int domainId, string sDomainName, string sDomainDescription);

        TVPApiModule.Objects.Responses.DomainResponseObject SubmitAddUserToDomainRequest(InitializationObject initObj, string siteGuid, string masterUsername);

        IEnumerable<TVPApiModule.Objects.Responses.GroupRule> GetDomainGroupRules(InitializationObject initObj, int domainId);

        bool SetDomainGroupRule(InitializationObject initObj, int domainId, int ruleID, string PIN, int isActive);

        IEnumerable<TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse> GetDomainsBillingHistory(InitializationObject initObj, int[] domainIDs, DateTime startDate, DateTime endDate);

        TVPApiModule.Objects.Responses.DomainResponseObject AddDomainWithCoGuid(InitializationObject initObj, string domainName, string domainDesc, int masterGuid, string coGuid);

        //int GetDomainIDByCoGuid(InitializationObject initObj, string coGuid);

        IEnumerable<TVPApiModule.Objects.Responses.PermittedMediaContainer> GetDomainPermittedItems(InitializationObject initObj, int domainId);

        IEnumerable<TVPApiModule.Objects.Responses.PermittedSubscriptionContainer> GetDomainPermittedSubscriptions(InitializationObject initObj, int domainId);

        bool SetRuleState(InitializationObject initObj, int ruleID, int isActive);
    }
}