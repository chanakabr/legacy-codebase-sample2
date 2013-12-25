using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IMediasRepository
    {
        List<Media> GetMediasInfo(InitializationObject initObj, List<int> MediaID, string picSize);

        List<Comment> GetMediaComments(InitializationObject initObj, int mediaID, int pageSize, int pageIndex);

        bool AddComment(InitializationObject initObj, int mediaID, int mediaType, string writer, string header, string subheader, string content, bool autoActive);

        TVPPro.SiteManager.TvinciPlatform.api.MediaMarkObject GetMediaMark(InitializationObject initObj, int iMediaID);

        string MediaMark(InitializationObject initObj, action Action, int mediaType, int iMediaID, int iFileID, int iLocation);

        string MediaHit(InitializationObject initObj, int mediaType, int iMediaID, int iFileID, int iLocation);

        List<Media> GetRelatedMediasByTypes(InitializationObject initObj, int mediaID, string picSize, int pageSize, int pageIndex, List<int> reqMediaTypes);

        List<Media> GetPeopleWhoWatched(InitializationObject initObj, int mediaID, string picSize, int pageSize, int pageIndex);

        List<KeyValuePair<int, bool>> AreMediasFavorite(InitializationObject initObj, List<int> mediaIds);

        string GetMediaLicenseLink(InitializationObject initObj, int mediaFileID, string baseLink);

        bool IsItemPurchased(InitializationObject initObj, int iFileID, string sUserGuid);

        PrePaidResponseStatus ChargeMediaWithPrepaid(InitializationObject initObj, double price, string currency, int mediaFileID, string ppvModuleCode, string couponCode);

        string DummyChargeUserForMediaFile(InitializationObject initObj, double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sCoupon);

        List<Media> GetRecommendedMediasByTypes(InitializationObject initObj, string picSize, int pageSize, int pageIndex, int[] reqMediaTypes);

        bool ActionDone(InitializationObject initObj, TVPApi.ActionType action, int mediaID, int mediaType, int extraVal);
    }
}