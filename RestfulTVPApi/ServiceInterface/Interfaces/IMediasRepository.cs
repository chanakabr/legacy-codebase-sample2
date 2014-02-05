using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApi;
using TVPApiModule.Context;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IMediasRepository
    {
        List<Media> GetMediasInfo(InitializationObject initObj, List<int> MediaID, string picSize);

        List<Comment> GetMediaComments(InitializationObject initObj, int mediaID, int pageSize, int pageIndex);

        bool AddComment(InitializationObject initObj, int mediaID, int mediaType, string writer, string header, string subheader, string content, bool autoActive);

        MediaMarkObject GetMediaMark(InitializationObject initObj, int iMediaID);

        string MediaMark(InitializationObject initObj, action Action, int mediaType, int iMediaID, int iFileID, int iLocation);

        string MediaHit(InitializationObject initObj, int mediaType, int iMediaID, int iFileID, int iLocation);

        List<Media> GetRelatedMediasByTypes(InitializationObject initObj, int mediaID, string picSize, int pageSize, int pageIndex, List<int> reqMediaTypes);

        List<Media> GetPeopleWhoWatched(InitializationObject initObj, int mediaID, string picSize, int pageSize, int pageIndex);

        List<Media> SearchMediaByAndOrList(InitializationObject initObj, List<KeyValue> orList, List<KeyValue> andList, int mediaType, int pageSize, int pageIndex, string picSize, bool exact, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy orderBy, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderDir orderDir, string orderMetaName);

        bool SendToFriend(InitializationObject initObj, int mediaID, string senderName, string senderEmail, string toEmail);

        List<string> GetAutoCompleteSearchList(InitializationObject initObj, string prefixText, int[] iMediaTypes);

        List<int> GetSubscriptionIDsContainingMediaFile(InitializationObject initObj, int iMediaID, int iFileID);

        List<MediaFileItemPricesContainer> GetItemsPricesWithCoupons(InitializationObject initObj, string sSiteGUID, int[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName);

        bool IsItemPurchased(InitializationObject initObj, string sSiteGUID, int iFileID);

        bool IsUserSocialActionPerformed(InitializationObject initObj, string sSiteGUID, int nMediaID, int socialPlatform, int socialAction);

        string GetMediaLicenseLink(InitializationObject initObj, string sSiteGUID, int mediaFileID, string baseLink);

        PrePaidResponseStatus ChargeMediaWithPrepaid(InitializationObject initObj, string sSiteGUID, double price, string currency, int mediaFileID, string ppvModuleCode, string couponCode);

        bool ActionDone(InitializationObject initObj, string sSiteGUID, ActionType action, int mediaID, int mediaType, int extraVal);

        List<string> GetUsersLikedMedia(InitializationObject initObj, string siteGuid, int mediaID, bool onlyFriends, int startIndex, int pageSize);
    }
}