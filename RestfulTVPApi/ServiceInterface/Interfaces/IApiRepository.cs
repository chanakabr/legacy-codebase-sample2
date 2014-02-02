
using System.Collections.Generic;
using TVPApi;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IApiRepository
    {
        bool ActivateCampaign(InitializationObject initObj, string siteGuid, int campaignID, string hashCode, int mediaID, string mediaLink, string senderEmail, string senderName,
                                                   TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionResult status, TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.VoucherReceipentInfo[] voucherReceipents);

        CouponData GetCouponStatus(InitializationObject initObj, string sCouponCode);

        PPVModule GetPPVModuleData(InitializationObject initObj, int ppvCode);

        string GetIPToCountry(InitializationObject initObj, string IP);

        string GetSiteGuidFromSecured(InitializationObject initObj, string encSiteGuid);

        UserResponseObject GetUserDataByCoGuid(InitializationObject initObj, string coGuid, int operatorID);

        List<Country> GetCountriesList(InitializationObject initObj);

        string GetGoogleSignature(InitializationObject initObj, int customerId);

        FBConnectConfig FBConfig(InitializationObject initObj);

        FacebookResponseObject FBUserMerge(InitializationObject initObj, string sToken, string sFBID, string sUsername, string sPassword);

        FacebookResponseObject FBUserRegister(InitializationObject initObj, string sToken, bool bCreateNewDomain, bool bGetNewsletter);

        FacebookResponseObject GetFBUserData(InitializationObject initObj, string sToken);

        TVPApiModule.Objects.Responses.DomainResponseObject GetDomainByCoGuid(InitializationObject initObj, string coGuid);

        List<int> GetDomainIDsByOperatorCoGuid(InitializationObject initObj, string operatorCoGuid);

        int GetDomainIDByCoGuid(InitializationObject initObj, string coGuid);

        DeviceRegistration RegisterDeviceByPIN(InitializationObject initObj, string pin);
    }
}