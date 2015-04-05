using RestfulTVPApi.Clients.ClientsCache;
using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.Objects.Responses.Enums;
using RestfulTVPApi.ServiceModel;
using System.Collections.Generic;
using System.Configuration;
using TVPApiModule.Helper;
using TVPPro.SiteManager.Helper;

namespace RestfulTVPApi.ServiceInterface
{
    public class ApiRepository : IApiRepository
    {
        public bool ActivateCampaign(ActivateCampaignRequest request)
        {
            return ClientsManager.ConditionalAccessService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).ActivateCampaign(request.site_guid, request.campaign_id, request.hash_code, request.media_id, request.media_link, request.sender_email, request.sender_name, request.status, request.voucher_receipents);            
        }

        public CouponData GetCouponStatus(GetCouponStatusRequest request)
        {
            return ClientsManager.PricingService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).GetCouponStatus(request.coupon_code);
        }

        public PPVModule GetPPVModuleData(GetPPVModuleDataRequest request)
        {
            return ClientsManager.PricingService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).GetPPVModuleData(request.ppv_code, string.Empty, string.Empty, request.InitObj.UDID);            
        }

        public string GetIPToCountry(GetIPToCountryRequest request)
        {
            return ClientsManager.UsersService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).IpToCountry(request.ip);            
        }

        public string GetSiteGuidFromSecured(GetSiteGuidFromSecuredRequest request)
        {
            // Shouldn't we wrap it here with try/catch in case TCMClient fails to get values?

            string privateKey = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}.{1}", "SiteGuidKv", "SecureSiteGuidKey"));
            string IV = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}.{1}", "SiteGuidKv", "SecureSiteGuidIV"));

            return SecurityHelper.DecryptSiteGuid(privateKey, IV, request.encrypted_site_guid);
        }

        public UserResponseObject GetUserDataByCoGuid(GetUserDataByCoGuidRequest request)
        {
            return ClientsManager.UsersService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).GetUserDataByCoGuid(request.co_guid, request.operator_id);            
        }

        public List<Country> GetCountriesList(GetCountriesListRequest request)
        {
            return ClientsManager.UsersService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).GetCountriesList();
        }

        public string GetGoogleSignature(GetGoogleSignatureRequest request)
        {
            return ClientsManager.ConditionalAccessService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).GetGoogleSignature(request.InitObj.SiteGuid, request.customer_id);
        }

        public FBConnectConfig FBConfig(FBConfigRequest request)
        {
            FacebookConfig fbConfig = ClientsManager.SocialService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).GetFBConfig("0");

                FBConnectConfig retVal = new FBConnectConfig
                {
                    app_id = fbConfig.fb_key,
                    scope = fbConfig.fb_permissions,
                    api_user = request.InitObj.ApiUser,
                    api_pass = request.InitObj.ApiPass
                };

                return retVal;            
        }

        public FacebookResponseObject FBUserMerge(FBUserMergeRequest request)
        {
            return ClientsManager.SocialService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).FBUserMerge(request.token, request.facebook_id, request.user_name, request.password);
        }

        public FacebookResponseObject FBUserUnMerge(FBUserUnMergeRequest request)
        {
            return ClientsManager.SocialService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).FBUserUnMerge(request.token, request.user_name, request.password);
        }

        public FacebookResponseObject FBUserRegister(FBUserRegisterRequest request)
        {
            var oExtra = new List<RestfulTVPApi.Social.KeyValuePair>() { new RestfulTVPApi.Social.KeyValuePair() { key = "news", value = request.get_newsletter ? "1" : "0" }, new RestfulTVPApi.Social.KeyValuePair() { key = "domain", value = request.create_new_domain ? "1" : "0" } };

            //Ofir - why its was UserHostAddress in ip param?
            return ClientsManager.SocialService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).FBUserRegister(request.token, "0", oExtra, SiteHelper.GetClientIP());
        }

        public FacebookResponseObject GetFBUserData(GetFBUserDataRequest request)
        {
            return ClientsManager.SocialService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).GetFBUserData(request.token, "0");            
        }

        public DomainResponseObject GetDomainByCoGuid(GetDomainByCoGuidRequest request)
        {
            return ClientsManager.DomainsService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).GetDomainByCoGuid(request.co_guid);
        }

        public List<int> GetDomainIDsByOperatorCoGuid(GetDomainIDsByOperatorCoGuidRequest request)
        {
            return ClientsManager.DomainsService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).GetDomainIDsByOperatorCoGuid(request.operator_co_guid);
        }

        public int GetDomainIDByCoGuid(GetDomainIDByCoGuidRequest request)
        {
            return ClientsManager.DomainsService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).GetDomainIDByCoGuid(request.co_guid);
        }

        public DeviceRegistration RegisterDeviceByPIN(RegisterDeviceByPINRequest request)
        {
            DeviceRegistration deviceRegistration = new DeviceRegistration();
            request.InitObj.DomainID = request.domain_id;
            DeviceResponseObject response = ClientsManager.DomainsService(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform).RegisterDeviceByPIN(request.device_name, request.InitObj.DomainID, request.pin);

            if (response != null)
            {
                deviceRegistration.reg_status = (eDeviceRegistrationStatus)response.device_response_status;
                deviceRegistration.udid = response.device.udid;                
            }

            return deviceRegistration;
        }
    }
}