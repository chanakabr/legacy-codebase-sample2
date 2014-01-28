using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Interfaces;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Objects.Responses;

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

        public IEnumerable<TVPApiModule.Services.ApiDomainsService.DeviceDomain> GetDeviceDomains(InitializationObject initObj, string udId)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDeviceDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                IEnumerable<TVPApiModule.Services.ApiDomainsService.DeviceDomain> devDomains = null;

                IEnumerable<Domain> domains = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDeviceDomains(initObj.UDID);

                if (domains != null && domains.Any())
                {
                    devDomains = new TVPApiModule.Services.ApiDomainsService.DeviceDomain[domains.Count()];

                    //for (int i = 0; i < domains.Count(); i++)
                    //{
                    //    devDomains[i] = new TVPApiModule.Services.ApiDomainsService.DeviceDomain() { DomainID = domains[i].domain_id, DomainName = domains[i].name, SiteGuid = domains[i].master_guids[0].ToString() };
                    //}
                }     
                
                return devDomains;
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
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveDeviceFromDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).RemoveDeviceToDomain(domainID, initObj.UDID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return resDomain;
        }

        public Domain GetDomainInfo(InitializationObject initObj, int domainId)
        {
            Domain domain = null;


            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                domain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDomainInfo(initObj.DomainID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return domain;
        }

        public DomainResponseObject ChangeDeviceDomainStatus(InitializationObject initObj, bool bActive)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ChangeDeviceDomainStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).ChangeDeviceDomainStatus(initObj.DomainID, initObj.UDID, bActive);
            }
            else
            {
                throw new UnknownGroupException();
            }
            
            return resDomain;
        }

        public DomainResponseObject AddUserToDomain(InitializationObject initObj, string addedUserGuid, int domainId)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddUserToDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                //Ofir - need to make sure irena changes site_guid to string
                resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).AddUserToDomain(initObj.DomainID, Convert.ToInt32(initObj.SiteGuid), int.Parse(addedUserGuid));
            }
            else
            {
                throw new UnknownGroupException();
            }

            return resDomain;
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
            string pin = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPINForDevice", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                pin = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetPINForDevice(initObj.UDID, devBrandID);
            }
            else
            {
                throw new UnknownGroupException();
            }
            
            return pin;
        }

        public bool SetRuleState(InitializationObject initObj, int ruleID, int isActive)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetRuleState", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetRuleState(initObj.SiteGuid, initObj.DomainID, ruleID, isActive);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
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
                return new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).RemoveDomain(initObj.DomainID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public DomainResponseObject RemoveUserFromDomain(InitializationObject initObj, string userGuidToRemove, int domainId)
        {
            DomainResponseObject domain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveUserFromDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {

                domain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).RemoveUserFromDomain(initObj.DomainID, userGuidToRemove);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return domain;
        }

        public bool SetDeviceInfo(InitializationObject initObj, string udid, string deviceName)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetDeviceInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new ApiDomainsService(groupID, initObj.Platform).SetDeviceInfo(initObj.UDID, deviceName);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public DomainResponseObject SetDomainInfo(InitializationObject initObj, int domainId, string sDomainName, string sDomainDescription)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetDomainInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).SetDomainInfo(initObj.DomainID, sDomainName, sDomainDescription);
            }
            else
            {
                throw new UnknownGroupException();
            }


            return resDomain;
        }

        public DomainResponseObject SubmitAddUserToDomainRequest(InitializationObject initObj, string siteGuid, string masterUsername)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SubmitAddUserToDomainRequest", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).SubmitAddUserToDomainRequest(int.Parse(siteGuid), masterUsername);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return resDomain;
        }

        public IEnumerable<TVPApiModule.Objects.Responses.GroupRule> GetDomainGroupRules(InitializationObject initObj, int domainId)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetDomainGroupRules(initObj.DomainID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool SetDomainGroupRule(InitializationObject initObj, int domainId, int ruleID, string PIN, int isActive)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetDomainGroupRule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetDomainGroupRule(initObj.DomainID, ruleID, PIN, isActive);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public IEnumerable<TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse> GetDomainsBillingHistory(InitializationObject initObj, int[] domainIDs, DateTime startDate, DateTime endDate)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainsBillingHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return new ApiConditionalAccessService(groupId, initObj.Platform).GetDomainsBillingHistory(domainIDs, startDate, endDate);
            }
            else
            {
                throw new UnknownGroupException();
            } 
        }

        public DomainResponseObject AddDomainWithCoGuid(InitializationObject initObj, string domainName, string domainDesc, int masterGuid, string coGuid)
        {
            DomainResponseObject domainRes = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddDomainWithCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                domainRes = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).AddDomainWithCoGuid(domainName, domainDesc, masterGuid, coGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return domainRes;
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

        public IEnumerable<PermittedMediaContainer> GetDomainPermittedItems(InitializationObject initObj, int domainId)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainPermittedItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return new ApiConditionalAccessService(groupId, initObj.Platform).GetDomainPermittedItems(initObj.DomainID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public IEnumerable<PermittedSubscriptionContainer> GetDomainPermittedSubscriptions(InitializationObject initObj, int domainId)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainPermittedSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                return new ApiConditionalAccessService(groupId, initObj.Platform).GetDomainPermittedSubscriptions(initObj.DomainID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }
    }
}
