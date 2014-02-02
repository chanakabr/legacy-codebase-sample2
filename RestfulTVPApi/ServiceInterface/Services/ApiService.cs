using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System.Net;
using ServiceStack.Api.Swagger;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using RestfulTVPApi.ServiceModel;
using TVPPro.SiteManager.TvinciPlatform.Pricing;
using System.Configuration;
using TVPPro.SiteManager.Helper;
using System;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class GeneralService : Service
    {
        public IApiRepository _repository { get; set; }  //Injected by IOC

        #region GET

        public object Get(GetCouponStatusRequest request)
        {
            return _repository.GetCouponStatus(request.InitObj, request.coupon_code);
        }

        public object Get(GetPPVModuleDataRequest request)
        {
            return _repository.GetPPVModuleData(request.InitObj, request.ppv_code);
        }

        public object Get(GetIPToCountryRequest request)
        {
            return _repository.GetIPToCountry(request.InitObj, request.ip);
        }

        public object Get(GetSecuredSiteGuidRequest request)
        {
            string privateKey = ConfigurationManager.AppSettings["SecureSiteGuidKey"];
            string IV = ConfigurationManager.AppSettings["SecureSiteGuidIV"];

            return SecurityHelper.EncryptSiteGuid(privateKey, IV, request.site_guid);
        }

        public object Get(GetSiteGuidFromSecuredRequest request)
        {
            return _repository.GetSiteGuidFromSecured(request.InitObj, request.encrypted_site_guid);
        }

        public object Get(GetUserDataByCoGuidRequest request)
        {
            return _repository.GetUserDataByCoGuid(request.InitObj, request.co_guid, request.operator_id);
        }

        public object Get(GetCountriesListRequest request)
        {
            return _repository.GetCountriesList(request.InitObj);
        }

        public object Get(GetGoogleSignatureRequest request)
        {
            return _repository.GetGoogleSignature(request.InitObj, request.customer_id);
        }

        public object Get(FBConfigRequest request)
        {
            return _repository.FBConfig(request.InitObj);
        }

        public object Get(GetFBUserDataRequest request)
        {
            return _repository.GetFBUserData(request.InitObj, request.token);
        }

        public object Get(GetDomainByCoGuidRequest request)
        {
            return _repository.GetDomainByCoGuid(request.InitObj, request.co_guid);
        }

        public object Get(GetDomainIDsByOperatorCoGuidRequest request)
        {
            return _repository.GetDomainIDsByOperatorCoGuid(request.InitObj, request.operator_co_guid);
        }

        public object Get(GetDomainIDByCoGuidRequest request)
        {
            return _repository.GetDomainIDByCoGuid(request.InitObj, request.co_guid);
        }

        #endregion

        #region PUT

        public object Put(ActivateCampaignRequest request)
        {
            return _repository.ActivateCampaign(request.InitObj, request.site_guid, request.campaign_id, request.hash_code, request.media_id, request.media_link, request.sender_email, request.sender_name,
                                                        request.status, request.voucher_receipents);
        }

        public object Put(FBUserMergeRequest request)
        {
            return _repository.FBUserMerge(request.InitObj, request.token, request.facebook_id, request.user_name, request.password);
        }

        #endregion

        #region POST

        public object Post(FBUserRegisterRequest request)
        {
            return _repository.FBUserRegister(request.InitObj, request.token, request.create_new_domain, request.get_newsletter);
        }

        public object Post(RegisterDeviceByPINRequest request)
        {
            return _repository.RegisterDeviceByPIN(request.InitObj, request.pin);
        }

        #endregion

        #region DELETE
        #endregion
        
    }
}
