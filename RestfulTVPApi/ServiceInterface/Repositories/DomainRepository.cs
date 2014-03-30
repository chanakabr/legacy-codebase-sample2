using System;
using System.Collections.Generic;
using TVPApiModule.Interfaces;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Objects;
using TVPApiModule.Helper;
using RestfulTVPApi.ServiceModel;

namespace RestfulTVPApi.ServiceInterface
{
    public class DomainRepository : IDomainRepository
    {
        public DomainResponseObject AddDeviceToDomain(AddDeviceToDomainRequest request)
        {
            DomainResponseObject resDomain = null;

            IImplementation impl = WSUtils.GetImplementation(request.GroupID, request.InitObj);
            resDomain = impl.AddDeviceToDomain(request.device_name, request.device_brand_id);
            
            return resDomain;
        }

        public DomainResponseObject AddDomain(AddDomainRequest request)
        {
            DomainResponseObject domainRes = null;

            domainRes = ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).AddDomain(request.device_name, request.domain_desc, request.master_guid_id);

            return domainRes;
        }

        public List<DeviceDomain> GetDeviceDomains(GetDeviceDomainsRequest request)
        {
            return ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).GetDeviceDomains(request.InitObj.UDID);
        }

        //public DomainResponseObject GetDomainByCoGuid(InitializationObject initObj, string coGuid)
        //{
        //    DomainResponseObject res = null;
        //    int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainByCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupID > 0)
        //    {
        //        res = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDomainByCoGuid(coGuid);
        //    }
        //    else
        //    {
        //        throw new UnknownGroupException();
        //    }            

        //    return res;
        //}

        public DomainResponseObject RemoveDeviceFromDomain(RemoveDeviceFromDomainRequest request)
        {
            return ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).RemoveDeviceToDomain(request.domain_id, request.InitObj.UDID);
        }

        public Domain GetDomainInfo(GetDomainInfoRequest request)
        {
            return ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).GetDomainInfo(request.domain_id);            
        }

        public DomainResponseObject ChangeDeviceDomainStatus(ChangeDeviceDomainStatusRequest request)
        {
            return ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).ChangeDeviceDomainStatus(request.InitObj.DomainID, request.InitObj.UDID, request.is_active);
        }

        public DomainResponseObject AddUserToDomain(AddUserToDomainRequest request)
        {
            return ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).AddUserToDomain(request.InitObj.DomainID, request.InitObj.SiteGuid, int.Parse(request.site_guid));
        }

        //public int[] GetDomainIDsByOperatorCoGuid(InitializationObject initObj, string operatorCoGuid)
        //{
        //    int[] resDomains = null;

        //    int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainIDsByOperatorCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupID > 0)
        //    {
        //        resDomains = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDomainIDsByOperatorCoGuid(operatorCoGuid);
        //    }
        //    else
        //    {
        //        throw new UnknownGroupException();
        //    }

        //    return resDomains;
        //}

        public string GetPINForDevice(GetPINForDeviceRequest request)
        {
            return ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).GetPINForDevice(request.InitObj.UDID, request.dev_brand_id);
        }

        public bool SetRuleState(SetRuleStateRequest request)
        {
            return ServicesManager.ApiApiService(request.GroupID, request.InitObj.Platform).SetRuleState(request.InitObj.SiteGuid, request.InitObj.DomainID, request.rule_id, request.is_active);
        }

        //public TVPApiModule.Services.ApiDomainsService.DeviceRegistration RegisterDeviceByPIN(InitializationObject initObj, string pin)
        //{
        //    TVPApiModule.Services.ApiDomainsService.DeviceRegistration deviceRes = new TVPApiModule.Services.ApiDomainsService.DeviceRegistration();
        //    int groupID = ConnectionHelper.GetGroupID("tvpapi", "RegisterDeviceByPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupID > 0)
        //    {
        //        TVPApiModule.Services.ApiDomainsService service = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform);
        //        DeviceResponseObject device = service.RegisterDeviceByPIN(initObj.UDID, initObj.DomainID, pin);

        //        if (device == null || device.m_oDeviceResponseStatus == DeviceResponseStatus.Error)
        //            deviceRes.RegStatus = TVPApiModule.Services.ApiDomainsService.eDeviceRegistrationStatus.Error;
        //        else if (device.m_oDeviceResponseStatus == DeviceResponseStatus.DuplicatePin || device.m_oDeviceResponseStatus == DeviceResponseStatus.DeviceNotExists)
        //            deviceRes.RegStatus = TVPApiModule.Services.ApiDomainsService.eDeviceRegistrationStatus.Invalid;
        //        else
        //        {
        //            deviceRes.RegStatus = TVPApiModule.Services.ApiDomainsService.eDeviceRegistrationStatus.Success;
        //            deviceRes.UDID = device.m_oDevice.m_deviceUDID;
        //        }
        //    }
        //    else
        //    {
        //        throw new UnknownGroupException();
        //    }

        //    return deviceRes;
        //}

        public DomainResponseStatus RemoveDomain(RemoveDomainRequest request)
        {
            return ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).RemoveDomain(request.InitObj.DomainID);                
        }

        public DomainResponseObject RemoveUserFromDomain(RemoveUserFromDomainRequest request)
        {
            return ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).RemoveUserFromDomain(request.domain_id, request.site_guid);
        }

        public bool SetDeviceInfo(SetDeviceInfoRequest request)
        {
            return ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).SetDeviceInfo(request.udid, request.device_name);
        }

        public DomainResponseObject SetDomainInfo(SetDomainInfoRequest request)
        {
            return ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).SetDomainInfo(request.domain_id, request.domain_name, request.domain_description);
        }

        public DomainResponseObject SubmitAddUserToDomainRequest(SubmitAddUserToDomainRequest request)
        {
            return ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).SubmitAddUserToDomainRequest(request.site_guid, request.master_user_name);
        }

        public List<TVPApiModule.Objects.Responses.GroupRule> GetDomainGroupRules(GetDomainGroupRulesRequest request)
        {
            return ServicesManager.ApiApiService(request.GroupID, request.InitObj.Platform).GetDomainGroupRules(request.domain_id);
        }

        public bool SetDomainGroupRule(SetDomainGroupRuleRequest request)
        {
            return ServicesManager.ApiApiService(request.GroupID, request.InitObj.Platform).SetDomainGroupRule(request.domain_id, request.rule_id, request.pin, request.is_active);                
        }

        public List<TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse> GetDomainsBillingHistory(GetDomainsBillingHistoryRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetDomainsBillingHistory(request.domain_ids, request.start_date, request.end_date);
        }

        public DomainResponseObject AddDomainWithCoGuid(AddDomainWithCoGuidRequest request)
        {
            return ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).AddDomainWithCoGuid(request.domain_name, request.domain_description, request.master_guid, request.co_guid);
        }

        //public int GetDomainIDByCoGuid(InitializationObject initObj, string coGuid)
        //{
        //    int res = 0;
        //    int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainIDByCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupID > 0)
        //    {
        //        res = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDomainIDByCoGuid(coGuid);
        //    }
        //    else
        //    {
        //        throw new UnknownGroupException();
        //    }

        //    return res;
        //}

        public List<PermittedMediaContainer> GetDomainPermittedItems(GetDomainPermittedItemsRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetDomainPermittedItems(request.domain_id);                
        }

        public List<PermittedSubscriptionContainer> GetDomainPermittedSubscriptions(GetDomainPermittedSubscriptionsRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetDomainPermittedSubscriptions(request.domain_id);                            
        }
    }
}
