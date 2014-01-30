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
            var response = _repository.GetCouponStatus(request.InitObj, request.coupon_code);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Get(GetPPVModuleDataRequest request)
        {
            var response = _repository.GetPPVModuleData(request.InitObj, request.ppv_code);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Get(GetIPToCountryRequest request)
        {
            var response = _repository.GetIPToCountry(request.InitObj, request.ip);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public object Get(GetSecuredSiteGuidRequest request)
        {
            string response = string.Empty;

            string privateKey = ConfigurationManager.AppSettings["SecureSiteGuidKey"];
            string IV = ConfigurationManager.AppSettings["SecureSiteGuidIV"];

            response = SecurityHelper.EncryptSiteGuid(privateKey, IV, request.site_guid);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public object Get(GetSiteGuidFromSecuredRequest request)
        {
            var response = _repository.GetSiteGuidFromSecured(request.InitObj, request.encrypted_site_guid);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public object Get(GetUserDataByCoGuidRequest request)
        {
            var response = _repository.GetUserDataByCoGuid(request.InitObj, request.co_guid, request.operator_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public object Get(GetCountriesListRequest request)
        {
            var response = _repository.GetCountriesList(request.InitObj);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Get(GetGoogleSignatureRequest request)
        {
            var response = _repository.GetGoogleSignature(request.InitObj, request.customer_id);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public object Get(FBConfigRequest request)
        {
            var response = _repository.FBConfig(request.InitObj);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Get(GetFBUserDataRequest request)
        {
            var response = _repository.GetFBUserData(request.InitObj, request.token);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }
        public object Get(GetDomainByCoGuidRequest request)
        {
            var response = _repository.GetDomainByCoGuid(request.InitObj, request.co_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Get(GetDomainIDsByOperatorCoGuidRequest request)
        {
            var response = _repository.GetDomainIDsByOperatorCoGuid(request.InitObj, request.operator_co_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public object Get(GetDomainIDByCoGuidRequest request)
        {
            var response = _repository.GetDomainIDByCoGuid(request.InitObj, request.co_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }

        #endregion

        #region PUT

        public object Put(ActivateCampaignRequest request)
        {
            var response = _repository.ActivateCampaign(request.InitObj, request.site_guid, request.campaign_id, request.hash_code, request.media_id, request.media_link, request.sender_email, request.sender_name,
                                                        request.status, request.voucher_receipents);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public object Put(FBUserMergeRequest request)
        {
            var response = _repository.FBUserMerge(request.InitObj, request.token, request.facebook_id, request.user_name, request.password);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        #endregion

        #region POST

        public object Post(FBUserRegisterRequest request)
        {
            var response = _repository.FBUserRegister(request.InitObj, request.token, request.create_new_domain, request.get_newsletter);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Post(RegisterDeviceByPINRequest request)
        {
            var response = _repository.RegisterDeviceByPIN(request.InitObj, request.pin);

            if ((Nullable<TVPApiModule.Services.ApiDomainsService.DeviceRegistration>)response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        #endregion

        #region DELETE
        #endregion
        
    }
}
