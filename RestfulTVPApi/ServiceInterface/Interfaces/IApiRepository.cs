
using TVPApi;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IApiRepository
    {
        bool ActivateCampaign(InitializationObject initObj, string siteGuid, int campaignID, string hashCode, int mediaID, string mediaLink, string senderEmail, string senderName,
                                                   CampaignActionResult status, VoucherReceipentInfo[] voucherReceipents);

        CouponData GetCouponStatus(InitializationObject initObj, string sCouponCode);

        PPVModule GetPPVModuleData(InitializationObject initObj, int ppvCode);

        string GetIPToCountry(InitializationObject initObj, string IP);

        string GetSiteGuidFromSecured(InitializationObject initObj, string encSiteGuid);

        UserResponseObject GetUserDataByCoGuid(InitializationObject initObj, string coGuid, int operatorID);

        Country[] GetCountriesList(InitializationObject initObj);

        string GetGoogleSignature(InitializationObject initObj, int customerId);

        TVPApiModule.Objects.Responses.DomainResponseObject GetDomainByCoGuid(InitializationObject initObj, string coGuid);

        int[] GetDomainIDsByOperatorCoGuid(InitializationObject initObj, string operatorCoGuid);

        int GetDomainIDByCoGuid(InitializationObject initObj, string coGuid);

        TVPApiModule.Services.ApiDomainsService.DeviceRegistration RegisterDeviceByPIN(InitializationObject initObj, string pin);
    }
}