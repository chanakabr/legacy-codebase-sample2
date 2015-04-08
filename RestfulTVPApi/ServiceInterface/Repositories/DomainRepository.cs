using System;
using System.Collections.Generic;
using TVPApiModule.Interfaces;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Helper;
using RestfulTVPApi.ServiceModel;
using RestfulTVPApi.Clients;
using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.Objects.Responses.Enums;
using RestfulTVPApi.Clients.Utils;

namespace RestfulTVPApi.ServiceInterface
{
    public class DomainRepository : IDomainRepository
    {
        public DomainResponseObject AddDeviceToDomain(AddDeviceToDomainRequest request)
        {
            DomainResponseObject resDomain = null;

            IImplementation impl = WSUtils.GetImplementation(request.GroupID, request.InitObj);
            //resDomain = impl.AddDeviceToDomain(request.device_name, request.device_brand_id);
            
            
            return resDomain;
        }

        public DomainResponseObject AddDomain(AddDomainRequest request)
        {
            return ClientsManager.DomainsClient().AddDomain(request.domain_name, request.domain_desc, request.master_guid_id);
        }

        public List<DeviceDomain> GetDeviceDomains(GetDeviceDomainsRequest request)
        {
            return ClientsManager.DomainsClient().GetDeviceDomains(request.InitObj.UDID);
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
            return ClientsManager.DomainsClient().RemoveDeviceToDomain(request.domain_id, request.udid);
        }

        public DomainResponseObject GetDomainInfo(GetDomainInfoRequest request)
        {
            return ClientsManager.DomainsClient().GetDomainInfo(request.domain_id);            
        }

        public DomainResponseObject ChangeDeviceDomainStatus(ChangeDeviceDomainStatusRequest request)
        {
            return ClientsManager.DomainsClient().ChangeDeviceDomainStatus(request.InitObj.DomainID, request.InitObj.UDID, request.is_active);
        }

        public DomainResponseObject AddUserToDomain(AddUserToDomainRequest request)
        {
            return ClientsManager.DomainsClient().AddUserToDomain(request.InitObj.DomainID, request.InitObj.SiteGuid, int.Parse(request.site_guid));
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
            return ClientsManager.DomainsClient().GetPINForDevice(request.InitObj.UDID, request.dev_brand_id);
        }

        public bool SetRuleState(SetRuleStateRequest request)
        {
            return ClientsManager.ApiClient().SetRuleState(request.InitObj.SiteGuid, request.InitObj.DomainID, request.rule_id, request.is_active);
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
            return ClientsManager.DomainsClient().RemoveDomain(request.InitObj.DomainID);                
        }

        public DomainResponseObject RemoveUserFromDomain(RemoveUserFromDomainRequest request)
        {
            return ClientsManager.DomainsClient().RemoveUserFromDomain(request.domain_id, request.site_guid);
        }

        public bool SetDeviceInfo(SetDeviceInfoRequest request)
        {
            return ClientsManager.DomainsClient().SetDeviceInfo(request.udid, request.device_name);
        }

        public DomainResponseObject SetDomainInfo(SetDomainInfoRequest request)
        {
            return ClientsManager.DomainsClient().SetDomainInfo(request.domain_id, request.domain_name, request.domain_description);
        }

        public DomainResponseObject SubmitAddUserToDomainRequest(SubmitAddUserToDomainRequest request)
        {
            return ClientsManager.DomainsClient().SubmitAddUserToDomainRequest(request.site_guid, request.master_user_name);
        }

        public List<GroupRule> GetDomainGroupRules(GetDomainGroupRulesRequest request)
        {
            return ClientsManager.ApiClient().GetDomainGroupRules(request.domain_id);
        }

        public bool SetDomainGroupRule(SetDomainGroupRuleRequest request)
        {
            return ClientsManager.ApiClient().SetDomainGroupRule(request.domain_id, request.rule_id, request.pin, request.is_active);  
        }

        public List<DomainBillingTransactionsResponse> GetDomainsBillingHistory(GetDomainsBillingHistoryRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetDomainsBillingHistory(request.domain_ids, request.start_date, request.end_date);
        }

        public DomainResponseObject AddDomainWithCoGuid(AddDomainWithCoGuidRequest request)
        {
            return ClientsManager.DomainsClient().AddDomainWithCoGuid(request.domain_name, request.domain_description, request.master_guid, request.co_guid);
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
            return ClientsManager.ConditionalAccessClient().GetDomainPermittedItems(request.domain_id);
        }

        public List<PermittedSubscriptionContainer> GetDomainPermittedSubscriptions(GetDomainPermittedSubscriptionsRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetDomainPermittedSubscriptions(request.domain_id);   
        }

        public bool SetDomainRestriction(SetDomainRestrictionRequest request)
        {
            return ClientsManager.DomainsClient().SetDomainRestriction(request.domain_id, request.restriction);
        }

        public DomainResponseObject SubmitAddDeviceToDomainRequest(SubmitAddDeviceToDomainRequest request)
        {
            return ClientsManager.DomainsClient().SubmitAddDeviceToDomainRequest(request.udid, request.domain_id, request.site_guid, request.device_name, request.brand_id);
        }

        public NetworkResponseObject AddHomeNetworkToDomain(AddHomeNetworkToDomainRequest request)
        {
            return ClientsManager.DomainsClient().AddHomeNetworkToDomain(request.domain_id, request.network_id, request.network_name, request.network_description);
        }

        public NetworkResponseObject UpdateDomainHomeNetwork(UpdateDomainHomeNetworkRequest request)
        {
            return ClientsManager.DomainsClient().UpdateDomainHomeNetwork(request.domain_id, request.network_id, request.network_name, request.network_description, request.is_active);         
        }

        public NetworkResponseObject RemoveDomainHomeNetwork(RemoveDomainHomeNetworkRequest request)
        {
            return ClientsManager.DomainsClient().RemoveDomainHomeNetwork(request.domain_id, request.network_id);         
        }
        
        public List<HomeNetwork> GetDomainHomeNetworks(GetDomainHomeNetworksRequest request)
        {
            return ClientsManager.DomainsClient().GetDomainHomeNetworks(request.domain_id); 
        }

        public DeviceResponseObject GetDeviceInfo(GetDeviceInfoRequest request)
        {
            return ClientsManager.DomainsClient().GetDeviceInfo(request.id, request.is_udid); 
        }

        public DomainResponseObject ChangeDomainMaster(ChangeDomainMasterRequest request)
        {
            return ClientsManager.DomainsClient().ChangeDomainMaster(request.domain_id, request.current_master_id, request.new_master_id); 
        }
        
        public DomainResponseObject ResetDomainFrequency(ResetDomainFrequencyRequest request)
        {
            return ClientsManager.DomainsClient().ResetDomainFrequency(request.domain_id, request.frequency_type); 
        }

        public List<PermittedCollectionContainer> GetDomainPermittedCollections(GetDomainPermittedCollectionsRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetDomainPermittedCollections(request.domain_id); 
        }


        public List<string> GetDomainUsersList(GetDomainUsersListRequest request)
        {
            return ClientsManager.DomainsClient().GetDomainUsersList(request.domain_id);
        }
    }
}
