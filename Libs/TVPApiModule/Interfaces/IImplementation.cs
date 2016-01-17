using System.Collections.Specialized;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApi;
using TVPApiModule.Objects;
using TVPApiModule.Services;
using TVPApiModule.yes.tvinci.ITProxy;
using TVPPro.Configuration.OrcaRecommendations;
using TVPPro.SiteManager.TvinciPlatform.Domains;

namespace TVPApiModule.Interfaces
{
    public interface IImplementation
    {
        ApiUsersService.LogInResponseData SignIn(string sUsername, string sPassword, NameValueCollection nameValueCollection = null);

        DomainResponseObject AddDeviceToDomain(string sDeviceName, int nDeviceBrandID);

        string MediaHit(int nMediaID, int nFileID, string sNPVRID, int nLocationID);

        string ChargeUserForSubscription(double dPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sIP, string sExtraParams, string sPaymentMethodID, string sEncryptedCVV);

        string MediaMark(action eAction, int nMediaType, int nMediaID, int nFileID, string sNPVRID, int nLocationID);

        bool IsItemPurchased(int iFileID, string sUserGuid);

        string GetMediaLicenseData(int iMediaFileID, int iMediaID);

        TVPApiModule.Helper.OrcaResponse GetRecommendedMediasByGallery(InitializationObject initObj, int groupID, int mediaID, string picSize, int maxParentalLevel, eGalleryType galleryType, string coGuid);

        string GetMediaLicenseLink(InitializationObject initObj, int groupId, int mediaFileID, string baseLink, string clientIP);

        RecordAllResult RecordAll(string accountNumber, string channelCode, string recordDate, string recordTime, string versionId, string serialNumber);

        TVPApiModule.yes.tvinci.ITProxy.STBData[] GetMemirDetails(string accountNumber, string serviceAddressId);

        UserResponse SetUserDynamicData(InitializationObject initObj, int groupID, string key, string value);
    }
}
