using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;

namespace TVPApiServices
{
    // NOTE: If you change the interface name "IService1" here, you must also update the reference to "IService1" in Web.config.
    [ServiceContract]
    public interface IMediaService
    {
        [OperationContract]
        Media GetMediaInfo(InitializationObject initObj, string ws_User, string ws_Pass, long MediaID, int mediaType, string picSize, bool withDynamic);

        [OperationContract]
        List<Media> GetChannelMediaList(InitializationObject initObj, string ws_User, string ws_Pass, long ChannelID, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        List<Media> GetChannelMediaListWithMediaCount(InitializationObject initObj, string ws_User, string ws_Pass, long ChannelID, string picSize, int pageSize, int pageIndex, ref long mediaCount);

        [OperationContract]
        bool IsMediaFavorite(InitializationObject initObj, string ws_User, string ws_Pass, int mediaID);

        [OperationContract]
        List<Media> GetRelatedMedias(InitializationObject initObj, string ws_User, string ws_Pass, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        List<Media> GetRelatedMediaWithMediaCount(InitializationObject initObj, string ws_User, string ws_Pass, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, ref long mediaCount);

        [OperationContract]
        List<Media> GetPeopleWhoWatched(InitializationObject initObj, string ws_User, string ws_Pass, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        List<Comment> GetMediaComments(InitializationObject initObj, string ws_User, string ws_Pass, int mediaID, int pageSize, int pageIndex);

        [OperationContract]
        List<Media> SearchMediaByTag(InitializationObject initObj, string ws_User, string ws_Pass, string tagName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy);

        [OperationContract]
        List<Media> SearchMediaByMeta(InitializationObject initObj, string ws_User, string ws_Pass, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy);

        [OperationContract]
        List<Media> SearchMediaByMetaWithMediaCount(InitializationObject initObj, string ws_User, string ws_Pass, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy, ref long mediaCount);

        [OperationContract]
        Category GetCategory(InitializationObject initObj, string ws_User, string ws_Pass, int categoryID);

        [OperationContract]
        List<Media> SearchMedia(InitializationObject initObj, string ws_User, string ws_Pass, string text, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy);

        [OperationContract]
        List<Media> SearchMediaWithMediaCount(InitializationObject initObj, string ws_User, string ws_Pass, string text, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy, ref long mediaCount);

        [OperationContract]
        List<Media> GetUserItems(InitializationObject initObj, string ws_User, string ws_Pass, UserItemType itemType, int mediaType, string picSize, int pageSize, int start_index);

        [OperationContract]
        List<string> GetNMostSearchedTexts(InitializationObject initObj, string ws_User, string ws_Pass, int N, int pageSize, int start_index);

        [OperationContract]
        string[] GetAutoCompleteSearchList(InitializationObject initObj, string ws_User, string ws_Pass, string prefixText);

        [OperationContract]
        bool ActionDone(InitializationObject initObj, string ws_User, string ws_Pass, TVPApi.ActionType action, int mediaID, int mediaType, int extraVal);

        [OperationContract]
        List<Media> GetMediasByMostAction(InitializationObject initObj, string ws_User, string ws_Pass, TVPApi.ActionType action, int mediaType);

        [OperationContract]
        List<Media> GetMediasByRating(InitializationObject initObj, string ws_User, string ws_Pass, int rating);

        // TODO: Add your service operations here
    }
}
