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

        public HttpResult Get(GetCouponStatusRequest request)
        {
            var response = _repository.GetCouponStatus(request.InitObj, request.coupon_code);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetPPVModuleDataRequest request)
        {
            var response = _repository.GetPPVModuleData(request.InitObj, request.ppv_code);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetIPToCountryRequest request)
        {
            var response = _repository.GetIPToCountry(request.InitObj, request.ip);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetSecuredSiteGuidRequest request)
        {
            string response = string.Empty;

            string privateKey = ConfigurationManager.AppSettings["SecureSiteGuidKey"];
            string IV = ConfigurationManager.AppSettings["SecureSiteGuidIV"];

            response = SecurityHelper.EncryptSiteGuid(privateKey, IV, request.site_guid);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetSiteGuidFromSecuredRequest request)
        {
            var response = _repository.GetSiteGuidFromSecured(request.InitObj, request.encrypted_site_guid);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserDataByCoGuidRequest request)
        {
            var response = _repository.GetUserDataByCoGuid(request.InitObj, request.co_guid, request.operator_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetCountriesListRequest request)
        {
            var response = _repository.GetCountriesList(request.InitObj);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetGoogleSignatureRequest request)
        {
            var response = _repository.GetGoogleSignature(request.InitObj, request.customer_id);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetDomainByCoGuidRequest request)
        {
            var response = _repository.GetDomainByCoGuid(request.InitObj, request.co_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetDomainIDsByOperatorCoGuidRequest request)
        {
            var response = _repository.GetDomainIDsByOperatorCoGuid(request.InitObj, request.operator_co_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetDomainIDByCoGuidRequest request)
        {
            var response = _repository.GetDomainIDByCoGuid(request.InitObj, request.co_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        #endregion

        #region PUT

        public HttpResult Put(ActivateCampaignRequest request)
        {
            var response = _repository.ActivateCampaign(request.InitObj, request.site_guid, request.campaign_id, request.hash_code, request.media_id, request.media_link, request.sender_email, request.sender_name,
                                                        request.status, request.voucher_receipents);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        #endregion

        #region POST

        public HttpResult Post(RegisterDeviceByPINRequest request)
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
