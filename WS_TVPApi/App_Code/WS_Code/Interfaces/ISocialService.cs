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
        FBConnectConfig FBConfig(InitializationObject initObj);

        [OperationContract]
        FacebookResponseObject GetFBUserData(InitializationObject initObj, string sToken);

        [OperationContract]
        FacebookResponseObject FBUserRegister(InitializationObject initObj, string sToken, bool bCreateNewDomain, bool bGetNewsletter);

        [OperationContract]
        FacebookResponseObject FBUserMerge(InitializationObject initObj, string sToken, string sFBID, string sUsername, string sPassword);

        [OperationContract]
        string GetUserSocialPrivacy(InitializationObject initObj, SocialPlatform socialPlatform, eUserAction userAction);

        [OperationContract]
        eSocialPrivacy[] GetUserAllowedSocialPrivacyList(InitializationObject initObj);

        [OperationContract]
        bool SetUserSocialPrivacy(InitializationObject initObj, SocialPlatform socialPlatform, eUserAction userAction, eSocialPrivacy socialPrivacy);

        [OperationContract]
        SocialActivityDoc[] GetUserActions(InitializationObject initObj, eUserAction userAction, eAssetType assetType, int assetID, int startIndex, int numOfRecords, SocialPlatform socialPlatform);

        [OperationContract]
        SocialActivityDoc[] GetFriendsActions(InitializationObject initObj, string[] userActions, eAssetType assetType, int assetID, int startIndex, int numOfRecords, SocialPlatform socialPlatform);

        [OperationContract]
        DoSocialActionResponse DoUserAction(InitializationObject initObj, eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[] extraParams, SocialPlatform socialPlatform, eAssetType assetType, int assetID);

        [OperationContract]
        string GetUserExternalActionShare(InitializationObject initObj, eUserAction userAction, SocialPlatform socialPlatform);

        [OperationContract]
        string GetUserInternalActionPrivacy(InitializationObject initObj, eUserAction userAction, SocialPlatform socialPlatform);

        [OperationContract]
        bool SetUserExternalActionShare(InitializationObject initObj, eUserAction userAction, SocialPlatform socialPlatform, eSocialActionPrivacy actionPrivacy);

        [OperationContract]
        bool SetUserInternalActionPrivacy(InitializationObject initObj, eUserAction userAction, SocialPlatform socialPlatform, eSocialActionPrivacy actionPrivacy);

        [OperationContract]
        SocialFeed GetSocialFeed(InitializationObject initObj, int mediaId, TVPPro.SiteManager.TvinciPlatform.Social.eSocialPlatform socialPlatform, int numOfItems, long epochStartTime);

        [OperationContract]
        FacebookResponseObject FBUserUnmerge(InitializationObject initObj, string token, string username, string password);

        [OperationContract]
        SocialActivityDoc[] GetUserActivityFeed(InitializationObject initObj, string siteGuid, int nPageSize, int nPageIndex, string sPicDimension);

        [OperationContract]
        FBSignin FBUserSignin(InitializationObject initObj, string token);
    }
}
