using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using TVPApi;
using TVPApiModule.Objects;
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

        [OperationContract]
        FBConnectConfig FBConfig(InitializationObject initObj, string sSTG);

        [OperationContract]
        FacebookResponseObject GetFBUserData(InitializationObject initObj, string sToken, string sSTG);

        [OperationContract]
        FacebookResponseObject FBUserRegister(InitializationObject initObj, string sToken, bool bCreateNewDomain, bool bGetNewsletter, string sSTG);

        [OperationContract]
        FacebookResponseObject FBUserMerge(InitializationObject initObj, string sToken, string sFBID, string sUsername, string sPassword);
    }
}
