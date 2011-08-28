using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPPro.SiteManager.TvinciPlatform.Pricing;
using TVPApiModule.Objects;

namespace TVPApiServices
{
    // NOTE: If you change the interface name "IService1" here, you must also update the reference to "IService1" in Web.config.
    [ServiceContract]
    public interface IMediaService
    {
        [OperationContract]
        Media GetMediaInfo(InitializationObject initObj, long MediaID, int mediaType, string picSize, bool withDynamic);

        [OperationContract]
        List<Media> GetChannelMediaList(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        List<Media> GetChannelMediaListWithMediaCount(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, ref long mediaCount);

        [OperationContract]
        bool IsMediaFavorite(InitializationObject initObj, int mediaID);

        [OperationContract]
        List<Media> GetRelatedMedias(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        List<Media> GetRelatedMediaWithMediaCount(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, ref long mediaCount);

        [OperationContract]
        List<Media> GetPeopleWhoWatched(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        List<Media> GetPeopleWhoLiked(InitializationObject initObj, TVPPro.SiteManager.TvinciPlatform.api.SocialPlatform socialPlatform, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        List<Comment> GetMediaComments(InitializationObject initObj, int mediaID, int pageSize, int pageIndex);

        [OperationContract]
        List<Media> SearchMediaByTag(InitializationObject initObj, string tagName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy);

        [OperationContract]
        List<Media> SearchMediaByMeta(InitializationObject initObj, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy);

        [OperationContract]
        List<Media> SearchMediaByMetaWithMediaCount(InitializationObject initObj, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy, ref long mediaCount);

        [OperationContract]
        Category GetCategory(InitializationObject initObj,int categoryID);

        [OperationContract]
        List<Media> SearchMedia(InitializationObject initObj, string text, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy);

        [OperationContract]
        List<Media> SearchMediaWithMediaCount(InitializationObject initObj, string text, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy, ref long mediaCount);

        [OperationContract]
        List<Media> GetUserItems(InitializationObject initObj, UserItemType itemType, int mediaType, string picSize, int pageSize, int start_index);

        [OperationContract]
        List<string> GetNMostSearchedTexts(InitializationObject initObj, int N, int pageSize, int start_index);

        [OperationContract]
        string[] GetAutoCompleteSearchList(InitializationObject initObj, string prefixText);

        [OperationContract]
        bool ActionDone(InitializationObject initObj, TVPApi.ActionType action, int mediaID, int mediaType, int extraVal);

        [OperationContract]
        List<Media> GetMediasByMostAction(InitializationObject initObj,TVPApi.ActionType action, int mediaType);

        [OperationContract]
        List<Media> GetMediasByRating(InitializationObject initObj, int rating);

        [OperationContract]
        string MediaMark(InitializationObject initObj, action Action, int mediaType, long iMediaID, long iFileID, int iLocation);

        [OperationContract]
        string MediaHit(InitializationObject initObj, int mediaType, long iMediaID, long iFileID, int iLocation);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.MediaMarkObject GetMediaMark(InitializationObject initObj, int iMediaID);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.Subscription[] GetSubscriptionsContainingMediaFile(InitializationObject initObj, int iMediaID, int iFileID);

        [OperationContract]
        bool AddUserSocialAction(InitializationObject initObj, int iMediaID, TVPPro.SiteManager.TvinciPlatform.api.SocialAction action, TVPPro.SiteManager.TvinciPlatform.api.SocialPlatform socialPlatform);
    }
}
