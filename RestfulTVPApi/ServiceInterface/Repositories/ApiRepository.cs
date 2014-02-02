using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Helper;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;

namespace RestfulTVPApi.ServiceInterface
{
    public class ApiRepository : IApiRepository
    {
        public bool ActivateCampaign(InitializationObject initObj, string siteGuid, int campaignID, string hashCode, int mediaID, string mediaLink, string senderEmail, string senderName,
                                                   TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionResult status, TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.VoucherReceipentInfo[] voucherReceipents)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ActivateCampaign", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupId, initObj.Platform);

                return _service.ActivateCampaign(siteGuid, campaignID, hashCode, mediaID, mediaLink, senderEmail, senderName, status, voucherReceipents);
            }
            else
            {
                throw new UnknownGroupException();
            }
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

        public List<Country> GetCountriesList(InitializationObject initObj)
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

        public FBConnectConfig FBConfig(InitializationObject initObj)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBConfig", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                FacebookConfig fbConfig = _service.GetFBConfig("0");

                FBConnectConfig retVal = new FBConnectConfig
                {
                    app_id = fbConfig.fb_key,
                    scope = fbConfig.fb_permissions,
                    api_user = initObj.ApiUser,
                    api_pass = initObj.ApiPass
                };

                return retVal;
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public FacebookResponseObject FBUserMerge(InitializationObject initObj, string sToken, string sFBID, string sUsername, string sPassword)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBUserMerge", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.FBUserMerge(sToken, sFBID, sUsername, sPassword);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public FacebookResponseObject FBUserRegister(InitializationObject initObj, string sToken, bool bCreateNewDomain, bool bGetNewsletter)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBUserRegister", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                var oExtra = new List<TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair>() { new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair() { key = "news", value = bGetNewsletter ? "1" : "0" }, new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair() { key = "domain", value = bCreateNewDomain ? "1" : "0" } };

                //Ofir - why its was UserHostAddress in ip param?
                return _service.FBUserRegister(sToken, "0", oExtra, SiteHelper.GetClientIP());

            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public FacebookResponseObject GetFBUserData(InitializationObject initObj, string sToken)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetFBUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.GetFBUserData(sToken, "0");
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public DomainResponseObject GetDomainByCoGuid(InitializationObject initObj, string coGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainByCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);

                return _service.GetDomainByCoGuid(coGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<int> GetDomainIDsByOperatorCoGuid(InitializationObject initObj, string operatorCoGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainIDsByOperatorCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);

                return _service.GetDomainIDsByOperatorCoGuid(operatorCoGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public int GetDomainIDByCoGuid(InitializationObject initObj, string coGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainIDByCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);

                return _service.GetDomainIDByCoGuid(coGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public DeviceRegistration RegisterDeviceByPIN(InitializationObject initObj, string pin)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RegisterDeviceByPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                DeviceRegistration deviceRegistration = null;

                ApiDomainsService _service = new ApiDomainsService(groupID, initObj.Platform);

                DeviceResponseObject device = _service.RegisterDeviceByPIN(initObj.UDID, initObj.DomainID, pin);

                if (device != null)
                {
                    if (device.device_response_status == DeviceResponseStatus.Error)
                        deviceRegistration.reg_status = eDeviceRegistrationStatus.Error;
                    else if (device.device_response_status == DeviceResponseStatus.DuplicatePin || device.device_response_status == DeviceResponseStatus.DeviceNotExists)
                        deviceRegistration.reg_status = eDeviceRegistrationStatus.Invalid;
                    else
                    {
                        deviceRegistration.reg_status = eDeviceRegistrationStatus.Success;
                        deviceRegistration.udid = device.device.device_udid;
                    }
                }
                
                return deviceRegistration;
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

    }
}