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
    [RequiresInitializationObject]
    public class GeneralService : Service
    {
        public IApiRepository _repository { get; set; }  //Injected by IOC

        #region GET

        public object Get(GetCouponStatusRequest request)
        {
            return _repository.GetCouponStatus(request);
        }

        public object Get(GetPPVModuleDataRequest request)
        {
            return _repository.GetPPVModuleData(request);
        }

        public object Get(GetIPToCountryRequest request)
        {
            return _repository.GetIPToCountry(request);
        }

        public object Get(GetSecuredSiteGuidRequest request)
        {
            string privateKey = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}.{1}", "SiteGuidKv", "SecureSiteGuidKey"));
            string IV = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}.{1}", "SiteGuidKv", "SecureSiteGuidIV"));            

            return SecurityHelper.EncryptSiteGuid(privateKey, IV, request.site_guid);
        }

        public object Get(GetSiteGuidFromSecuredRequest request)
        {
            return _repository.GetSiteGuidFromSecured(request);
        }

        public object Get(GetUserDataByCoGuidRequest request)
        {
            return _repository.GetUserDataByCoGuid(request);
        }

        public object Get(GetCountriesListRequest request)
        {
            return _repository.GetCountriesList(request);
        }

        public object Get(GetGoogleSignatureRequest request)
        {
            return _repository.GetGoogleSignature(request);
        }

        public object Get(FBConfigRequest request)
        {
            return _repository.FBConfig(request);
        }        

        public object Get(GetDomainByCoGuidRequest request)
        {
            return _repository.GetDomainByCoGuid(request);
        }

        public object Get(GetDomainIDsByOperatorCoGuidRequest request)
        {
            return _repository.GetDomainIDsByOperatorCoGuid(request);
        }

        public object Get(GetDomainIDByCoGuidRequest request)
        {
            return _repository.GetDomainIDByCoGuid(request);
        }

        #endregion

        #region PUT

        public object Put(ActivateCampaignRequest request)
        {
            return _repository.ActivateCampaign(request);
        }

        public object Put(FBUserMergeRequest request)
        {
            return _repository.FBUserMerge(request);
        }

        public object Put(FBUserUnMergeRequest request)
        {
            return _repository.FBUserUnMerge(request);
        }

        #endregion

        #region POST

        public object Post(FBUserRegisterRequest request)
        {
            return _repository.FBUserRegister(request);
        }

        public object Post(RegisterDeviceByPINRequest request)
        {
            return _repository.RegisterDeviceByPIN(request);
        }

        public object Post(GetFBUserDataRequest request)
        {
            return _repository.GetFBUserData(request);
        }

        #endregion

        #region DELETE
        #endregion
        
    }
}
