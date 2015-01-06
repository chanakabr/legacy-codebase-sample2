using RestfulTVPApi.ServiceModel;
using System.Collections.Generic;
using System.Configuration;
using TVPApiModule.Helper;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;

namespace RestfulTVPApi.ServiceInterface
{
    public class ApiRepository : IApiRepository
    {
        public bool ActivateCampaign(ActivateCampaignRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).ActivateCampaign(request.site_guid, request.campaign_id, request.hash_code, request.media_id, request.media_link, request.sender_email, request.sender_name, request.status, request.voucher_receipents);            
        }

        public CouponData GetCouponStatus(GetCouponStatusRequest request)
        {
            return ServicesManager.PricingService(request.GroupID, request.InitObj.Platform).GetCouponStatus(request.coupon_code);
        }

        public PPVModule GetPPVModuleData(GetPPVModuleDataRequest request)
        {
            return ServicesManager.PricingService(request.GroupID, request.InitObj.Platform).GetPPVModuleData(request.ppv_code, string.Empty, string.Empty, request.InitObj.UDID);            
        }

        public string GetIPToCountry(GetIPToCountryRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).IpToCountry(request.ip);            
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
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).GetUserDataByCoGuid(request.co_guid, request.operator_id);            
        }

        public List<Country> GetCountriesList(GetCountriesListRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).GetCountriesList();
        }

        public string GetGoogleSignature(GetGoogleSignatureRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetGoogleSignature(request.InitObj.SiteGuid, request.customer_id);
        }

        public FBConnectConfig FBConfig(FBConfigRequest request)
        {
            FacebookConfig fbConfig = ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).GetFBConfig("0");

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
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).FBUserMerge(request.token, request.facebook_id, request.user_name, request.password);
        }

        public FacebookResponseObject FBUserUnMerge(FBUserUnMergeRequest request)
        {
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).FBUserUnMerge(request.token, request.user_name, request.password);
        }

        public FacebookResponseObject FBUserRegister(FBUserRegisterRequest request)
        {
            var oExtra = new List<TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair>() { new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair() { key = "news", value = request.get_newsletter ? "1" : "0" }, new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair() { key = "domain", value = request.create_new_domain ? "1" : "0" } };

            //Ofir - why its was UserHostAddress in ip param?
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).FBUserRegister(request.token, "0", oExtra, SiteHelper.GetClientIP());
        }

        public FacebookResponseObject GetFBUserData(GetFBUserDataRequest request)
        {
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).GetFBUserData(request.token, "0");            
        }

        public DomainResponseObject GetDomainByCoGuid(GetDomainByCoGuidRequest request)
        {
            return ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).GetDomainByCoGuid(request.co_guid);
        }

        public List<int> GetDomainIDsByOperatorCoGuid(GetDomainIDsByOperatorCoGuidRequest request)
        {
            return ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).GetDomainIDsByOperatorCoGuid(request.operator_co_guid);
        }

        public int GetDomainIDByCoGuid(GetDomainIDByCoGuidRequest request)
        {
            return ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).GetDomainIDByCoGuid(request.co_guid);
        }

        public DeviceRegistration RegisterDeviceByPIN(RegisterDeviceByPINRequest request)
        {
            DeviceRegistration deviceRegistration = new DeviceRegistration();
            request.InitObj.DomainID = request.domain_id;
            DeviceResponseObject response = ServicesManager.DomainsService(request.GroupID, request.InitObj.Platform).RegisterDeviceByPIN(request.device_name, request.InitObj.DomainID, request.pin);

            if (response != null)
            {
                deviceRegistration.reg_status = (eDeviceRegistrationStatus)response.device_response_status;
                deviceRegistration.udid = response.device.udid;                
            }

            return deviceRegistration;
        }

    }
}