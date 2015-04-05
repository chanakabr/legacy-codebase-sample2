using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.ServiceModel;
using System.Collections.Generic;
using TVPApi;


namespace RestfulTVPApi.ServiceInterface
{
    public interface IApiRepository
    {        
        bool ActivateCampaign(ActivateCampaignRequest request);

        CouponData GetCouponStatus(GetCouponStatusRequest request);

        PPVModule GetPPVModuleData(GetPPVModuleDataRequest request);

        string GetIPToCountry(GetIPToCountryRequest request);

        string GetSiteGuidFromSecured(GetSiteGuidFromSecuredRequest request);

        UserResponseObject GetUserDataByCoGuid(GetUserDataByCoGuidRequest request);

        List<Country> GetCountriesList(GetCountriesListRequest request);

        string GetGoogleSignature(GetGoogleSignatureRequest request);

        FBConnectConfig FBConfig(FBConfigRequest request);

        FacebookResponseObject FBUserMerge(FBUserMergeRequest request);

        FacebookResponseObject FBUserUnMerge(FBUserUnMergeRequest request);

        FacebookResponseObject FBUserRegister(FBUserRegisterRequest request);

        FacebookResponseObject GetFBUserData(GetFBUserDataRequest request);

        DomainResponseObject GetDomainByCoGuid(GetDomainByCoGuidRequest requst);

        List<int> GetDomainIDsByOperatorCoGuid(GetDomainIDsByOperatorCoGuidRequest request);

        int GetDomainIDByCoGuid(GetDomainIDByCoGuidRequest request);

        DeviceRegistration RegisterDeviceByPIN(RegisterDeviceByPINRequest request);
    }
}