using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApiModule.Services;
using TVPPro.SiteManager.TvinciPlatform.Domains;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApi;
using TVPPro.SiteManager.DataEntities;
using TVPPro.Configuration.OrcaRecommendations;
using TVPApiModule.yes.tvinci.ITProxy;

namespace TVPApiModule.Interfaces
{
    public interface IImplementation
    {
        ApiUsersService.LogInResponseData SignIn(string sUsername, string sPassword);
        DomainResponseObject AddDeviceToDomain(string sDeviceName, int nDeviceBrandID);
        string MediaHit(int nMediaID, int nFileID, int nLocationID);
        string ChargeUserForSubscription(double dPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sIP, string sExtraParams, string sPaymentMethodID, string sEncryptedCVV);

        string MediaMark(action eAction, int nMediaType, int nMediaID, int nFileID, int nLocationID);

        bool IsItemPurchased(int iFileID, string sUserGuid);

        string GetMediaLicenseData(int iMediaFileID, int iMediaID);

        TVPApiModule.Helper.OrcaResponse GetRecommendedMediasByGallery(InitializationObject initObj, int groupID, int mediaID, string picSize, int maxParentalLevel, eGalleryType galleryType);

        string GetMediaLicenseLink(InitializationObject initObj, int groupId, int mediaFileID, string baseLink, string clientIP);

        RecordAllResult RecordAll(string accountNumber, string channelCode, string recordDate, string recordTime, string versionId);
    }
}
