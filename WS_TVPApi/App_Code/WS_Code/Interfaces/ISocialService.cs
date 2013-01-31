using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Social;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace TVPApiServices
{
    [ServiceContract]
    public interface ISocialService
    {
        [OperationContract]
        FriendWatchedObject[] GetAllFriendsWatched(InitializationObject initObj, int maxResult);

        [OperationContract]
        FriendWatchedObject[] GetFriendsWatchedByMedia(InitializationObject initObj, int mediaId);

        [OperationContract]
        string[] GetUsersLikedMedia(InitializationObject initObj, int mediaID, bool onlyFriends, int startIndex, int pageSize);

        [OperationContract]
        string[] GetUserFriends(InitializationObject initObj);
    }
}
