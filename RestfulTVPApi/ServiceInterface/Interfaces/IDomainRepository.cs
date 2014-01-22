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
        DomainResponseObject AddDeviceToDomain(InitializationObject initObj, string sDeviceName, int iDeviceBrandID);

        DomainResponseObject AddDomain(InitializationObject initObj, string domainName, string domainDesc, int masterGuid);

        DomainResponseObject AddUserToDomain(InitializationObject initObj, int addedUserGuid, int domainId);

        ApiDomainsService.DeviceDomain[] GetDeviceDomains(InitializationObject initObj, string udId);

        DomainResponseObject GetDomainByCoGuid(InitializationObject initObj, string coGuid);

        DomainResponseObject RemoveDeviceFromDomain(InitializationObject initObj, int domainID, string udId, string sDeviceName, int iDeviceBrandID);

        Domain GetDomainInfo(InitializationObject initObj, int domainId);

        DomainResponseObject ChangeDeviceDomainStatus(InitializationObject initObj, bool bActive);

        int[] GetDomainIDsByOperatorCoGuid(InitializationObject initObj, string operatorCoGuid);

        string GetPINForDevice(InitializationObject initObj, int devBrandID);

        TVPApiModule.Services.ApiDomainsService.DeviceRegistration RegisterDeviceByPIN(InitializationObject initObj, string pin);

        string RemoveDomain(InitializationObject initObj, int domainId);

        DomainResponseObject RemoveUserFromDomain(InitializationObject initObj, string userGuidToRemove, int domainId);

        bool SetDeviceInfo(InitializationObject initObj, string udid, string deviceName);

        DomainResponseObject SetDomainInfo(InitializationObject initObj, int domainId, string sDomainName, string sDomainDescription);

        DomainResponseObject SubmitAddUserToDomainRequest(InitializationObject initObj, string masterUsername);

        TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetDomainGroupRules(InitializationObject initObj, int domainId);

        bool SetDomainGroupRule(InitializationObject initObj, int domainId, int ruleID, string PIN, int isActive);

        TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.DomainBillingTransactionsResponse[] GetDomainsBillingHistory(InitializationObject initObj, int[] domainIDs, DateTime startDate, DateTime endDate);

        DomainResponseObject AddDomainWithCoGuid(InitializationObject initObj, string domainName, string domainDesc, int masterGuid, string coGuid);

        int GetDomainIDByCoGuid(InitializationObject initObj, string coGuid);

        PermittedMediaContainer[] GetDomainPermittedItems(InitializationObject initObj, int domainId);

        PermittedSubscriptionContainer[] GetDomainPermittedSubscriptions(InitializationObject initObj, int domainId);
    }
}