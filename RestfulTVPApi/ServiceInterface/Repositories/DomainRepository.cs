using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Interfaces;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Objects;
using TVPApiModule.Helper;

namespace RestfulTVPApi.ServiceInterface
{
    public class DomainRepository : IDomainRepository
    {
        public DomainResponseObject AddDeviceToDomain(InitializationObject initObj, string sDeviceName, int iDeviceBrandID)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddDeviceToDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                IImplementation impl = WSUtils.GetImplementation(groupID, initObj);
                resDomain = impl.AddDeviceToDomain(sDeviceName, iDeviceBrandID);
            }
            else
            {
                throw new UnknownGroupException();
            }            

            return resDomain;
        }

        public DomainResponseObject AddDomain(InitializationObject initObj, string domainName, string domainDesc, int masterGuid)
        {
            DomainResponseObject domainRes = null;
           
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                domainRes = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).AddDomain(domainName, domainDesc, masterGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }            

            return domainRes;
        }

        public List<DeviceDomain> GetDeviceDomains(InitializationObject initObj, string udId)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDeviceDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);

                return _service.GetDeviceDomains(initObj.UDID);
            }
            else
            {
                throw new UnknownGroupException();
            }
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

        public DomainResponseObject RemoveDeviceFromDomain(InitializationObject initObj, int domainID, string udId, string sDeviceName, int iDeviceBrandID)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveDeviceFromDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);

                return _service.RemoveDeviceToDomain(domainID, initObj.UDID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public Domain GetDomainInfo(InitializationObject initObj, int domainId)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                //ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);
                //ApiDomainsService _service = ApiDomainsService.Instance(groupID, initObj.Platform);
                try
                {
                    ApiDomainsService _service = ServicesManager.Instance.GetService(groupID, initObj.Platform, eService.DomainsService) as ApiDomainsService;

                    return ((ApiDomainsService)_service).GetDomainInfo(initObj.DomainID);
                }
                catch (Exception ex)
                {
                    // Implement FailOver Mechanism
                    GetDomainInfo(initObj, domainId);
                    throw ex;
                }
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public DomainResponseObject ChangeDeviceDomainStatus(InitializationObject initObj, bool bActive)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ChangeDeviceDomainStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);

                return _service.ChangeDeviceDomainStatus(initObj.DomainID, initObj.UDID, bActive);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public DomainResponseObject AddUserToDomain(InitializationObject initObj, string addedUserGuid, int domainId)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddUserToDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);

                return _service.AddUserToDomain(initObj.DomainID, initObj.SiteGuid, int.Parse(addedUserGuid));
            }
            else
            {
                throw new UnknownGroupException();
            }
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

        public string GetPINForDevice(InitializationObject initObj, int devBrandID)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPINForDevice", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);

                return _service.GetPINForDevice(initObj.UDID, devBrandID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool SetRuleState(InitializationObject initObj, int ruleID, int isActive)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetRuleState", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiApiService _service = new ApiApiService(groupID, initObj.Platform);

                return _service.SetRuleState(initObj.SiteGuid, initObj.DomainID, ruleID, isActive);
            }
            else
            {
                throw new UnknownGroupException();
            }
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

        public DomainResponseStatus RemoveDomain(InitializationObject initObj, int domainId)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);

                return _service.RemoveDomain(initObj.DomainID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public DomainResponseObject RemoveUserFromDomain(InitializationObject initObj, string userGuidToRemove, int domainId)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveUserFromDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);

                return _service.RemoveUserFromDomain(initObj.DomainID, userGuidToRemove);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool SetDeviceInfo(InitializationObject initObj, string udid, string deviceName)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetDeviceInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);

                return _service.SetDeviceInfo(initObj.UDID, deviceName);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public DomainResponseObject SetDomainInfo(InitializationObject initObj, int domainId, string sDomainName, string sDomainDescription)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetDomainInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);

                return _service.SetDomainInfo(initObj.DomainID, sDomainName, sDomainDescription);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public DomainResponseObject SubmitAddUserToDomainRequest(InitializationObject initObj, string siteGuid, string masterUsername)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SubmitAddUserToDomainRequest", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);

                return _service.SubmitAddUserToDomainRequest(siteGuid, masterUsername);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<TVPApiModule.Objects.Responses.GroupRule> GetDomainGroupRules(InitializationObject initObj, int domainId)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiApiService _service = new ApiApiService(groupID, initObj.Platform);

                return _service.GetDomainGroupRules(initObj.DomainID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool SetDomainGroupRule(InitializationObject initObj, int domainId, int ruleID, string PIN, int isActive)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetDomainGroupRule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiApiService _service = new ApiApiService(groupID, initObj.Platform);

                return _service.SetDomainGroupRule(initObj.DomainID, ruleID, PIN, isActive);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse> GetDomainsBillingHistory(InitializationObject initObj, int[] domainIDs, DateTime startDate, DateTime endDate)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainsBillingHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupID, initObj.Platform);

                return _service.GetDomainsBillingHistory(domainIDs, startDate, endDate);
            }
            else
            {
                throw new UnknownGroupException();
            } 
        }

        public DomainResponseObject AddDomainWithCoGuid(InitializationObject initObj, string domainName, string domainDesc, int masterGuid, string coGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddDomainWithCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);

                return _service.AddDomainWithCoGuid(domainName, domainDesc, masterGuid, coGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }
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

        public List<PermittedMediaContainer> GetDomainPermittedItems(InitializationObject initObj, int domainId)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainPermittedItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupID, initObj.Platform);

                return _service.GetDomainPermittedItems(initObj.DomainID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<PermittedSubscriptionContainer> GetDomainPermittedSubscriptions(InitializationObject initObj, int domainId)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainPermittedSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupID, initObj.Platform);

                return _service.GetDomainPermittedSubscriptions(initObj.DomainID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }
    }
}
