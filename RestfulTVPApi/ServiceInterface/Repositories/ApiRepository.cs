using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Helper;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;

namespace RestfulTVPApi.ServiceInterface
{
    public class ApiRepository : IApiRepository
    {
        public bool ActivateCampaign(InitializationObject initObj, string siteGuid, int campaignID, string hashCode, int mediaID, string mediaLink, string senderEmail, string senderName,
                                                   CampaignActionResult status, VoucherReceipentInfo[] voucherReceipents)
        {
            bool res = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ActivateCampaign", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                CampaignActionInfo actionInfo = new CampaignActionInfo()
                {
                    m_siteGuid = int.Parse(initObj.SiteGuid),
                    m_socialInviteInfo = !string.IsNullOrEmpty(hashCode) ? new SocialInviteInfo() { m_hashCode = hashCode } : null,
                    m_mediaID = mediaID,
                    m_mediaLink = mediaLink,
                    m_senderEmail = senderEmail,
                    m_senderName = senderName,
                    m_status = status,
                    m_voucherReceipents = voucherReceipents
                };

                //Ofir - talk To irena
                //res = new ApiConditionalAccessService(groupId, initObj.Platform).ActivateCampaign(siteGuid, campaignID, actionInfo);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

        public CouponData GetCouponStatus(InitializationObject initObj, string sCouponCode)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCouponStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiPricingService _service = new ApiPricingService(groupId, initObj.Platform);

                return _service.GetCouponStatus(sCouponCode);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public PPVModule GetPPVModuleData(InitializationObject initObj, int ppvCode)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetPPVModuleData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiPricingService _service = new ApiPricingService(groupId, initObj.Platform);

                return _service.GetPPVModuleData(ppvCode, string.Empty, string.Empty, initObj.UDID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public string GetIPToCountry(InitializationObject initObj, string IP)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetIPToCountry", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupId, initObj.Platform);

                return _service.IpToCountry(IP);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public string GetSiteGuidFromSecured(InitializationObject initObj, string encSiteGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSiteGuidFromSecured", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                string privateKey = ConfigurationManager.AppSettings["SecureSiteGuidKey"];
                string IV = ConfigurationManager.AppSettings["SecureSiteGuidIV"];

                return SecurityHelper.DecryptSiteGuid(privateKey, IV, encSiteGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public UserResponseObject GetUserDataByCoGuid(InitializationObject initObj, string coGuid, int operatorID)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserDataByCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupId, initObj.Platform);

                return _service.GetUserDataByCoGuid(coGuid, operatorID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public Country[] GetCountriesList(InitializationObject initObj)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetCountriesList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupID, initObj.Platform);

                return _service.GetCountriesList();
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public string GetGoogleSignature(InitializationObject initObj, int customerId)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGoogleSignature", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupID, initObj.Platform);

                return _service.GetGoogleSignature(initObj.SiteGuid, customerId);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public DomainResponseObject GetDomainByCoGuid(InitializationObject initObj, string coGuid)
        {
            DomainResponseObject res = null;
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainByCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                res = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDomainByCoGuid(coGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

        public int[] GetDomainIDsByOperatorCoGuid(InitializationObject initObj, string operatorCoGuid)
        {
            int[] resDomains = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainIDsByOperatorCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                resDomains = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDomainIDsByOperatorCoGuid(operatorCoGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return resDomains;
        }

        public int GetDomainIDByCoGuid(InitializationObject initObj, string coGuid)
        {
            int res = 0;
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainIDByCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                res = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDomainIDByCoGuid(coGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

        public TVPApiModule.Services.ApiDomainsService.DeviceRegistration RegisterDeviceByPIN(InitializationObject initObj, string pin)
        {
            TVPApiModule.Services.ApiDomainsService.DeviceRegistration deviceRes = new TVPApiModule.Services.ApiDomainsService.DeviceRegistration();
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RegisterDeviceByPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                TVPApiModule.Services.ApiDomainsService service = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform);
                DeviceResponseObject device = service.RegisterDeviceByPIN(initObj.UDID, initObj.DomainID, pin);

                if (device == null || device.m_oDeviceResponseStatus == DeviceResponseStatus.Error)
                    deviceRes.RegStatus = TVPApiModule.Services.ApiDomainsService.eDeviceRegistrationStatus.Error;
                else if (device.m_oDeviceResponseStatus == DeviceResponseStatus.DuplicatePin || device.m_oDeviceResponseStatus == DeviceResponseStatus.DeviceNotExists)
                    deviceRes.RegStatus = TVPApiModule.Services.ApiDomainsService.eDeviceRegistrationStatus.Invalid;
                else
                {
                    deviceRes.RegStatus = TVPApiModule.Services.ApiDomainsService.eDeviceRegistrationStatus.Success;
                    deviceRes.UDID = device.m_oDevice.m_deviceUDID;
                }
            }
            else
            {
                throw new UnknownGroupException();
            }

            return deviceRes;
        }

    }
}