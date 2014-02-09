using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApi;

namespace TVPWebApi.Models
{
    public interface IMediasService
    {
        Media GetMediaInfo(InitializationObject initObj, long MediaID, int mediaType, string picSize, bool withDynamic);

        List<Comment> GetMediaComments(InitializationObject initObj, int mediaID, int pageSize, int pageIndex);

        bool AddComment(InitializationObject initObj, int mediaID, int mediaType, string writer, string header, string subheader, string content, bool autoActive);

        TVPPro.SiteManager.TvinciPlatform.api.RateMediaObject RateMedia(InitializationObject initObj, int mediaID, int mediaType, int extraVal);

        TVPPro.SiteManager.TvinciPlatform.api.MediaMarkObject GetMediaMark(InitializationObject initObj, int iMediaID);

        string MediaMark(InitializationObject initObj, action Action, int mediaType, long iMediaID, long iFileID, int iLocation);

        string MediaHit(InitializationObject initObj, int mediaType, long iMediaID, long iFileID, int iLocation);

        List<Media> GetRelatedMedias(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex);

        List<Media> GetPeopleWhoWatched(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex);

        string CheckGeoBlockForMedia(InitializationObject initObj, int iMediaID);

        string GetMediaLicenseLink(InitializationObject initObj, int mediaFileID, string baseLink);

        bool IsItemPurchased(InitializationObject initObj, int iFileID, string sUserGuid);
    }
}