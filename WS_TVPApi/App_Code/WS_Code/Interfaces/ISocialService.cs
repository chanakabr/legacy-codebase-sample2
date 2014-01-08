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
        FriendWatchedObject[] GetAllFriendsWatched(InitializationObject initObj, string siteGuid, int maxResult);

        [OperationContract]
        FriendWatchedObject[] GetFriendsWatchedByMedia(InitializationObject initObj, string siteGuid, int mediaId);

        [OperationContract]
        string[] GetUsersLikedMedia(InitializationObject initObj, int mediaID, bool onlyFriends, int startIndex, int pageSize);

        [OperationContract]
        string[] GetUserFriends(InitializationObject initObj, string siteGuid);

        [OperationContract]
        FBConnectConfig FBConfig(InitializationObject initObj, string sSTG);

        [OperationContract]
        FacebookResponseObject GetFBUserData(InitializationObject initObj, string sToken, string sSTG);

        [OperationContract]
        FacebookResponseObject FBUserRegister(InitializationObject initObj, string sToken, bool bCreateNewDomain, bool bGetNewsletter, string sSTG);

        [OperationContract]
        FacebookResponseObject FBUserMerge(InitializationObject initObj, string sToken, string sFBID, string sUsername, string sPassword);

        [OperationContract]
        string GetUserSocialPrivacy(InitializationObject initObj, string siteGuid, SocialPlatform socialPlatform, eUserAction userAction);

        [OperationContract]
        eSocialPrivacy[] GetUserAllowedSocialPrivacyList(InitializationObject initObj, string siteGuid);

        [OperationContract]
        bool SetUserSocialPrivacy(InitializationObject initObj, string siteGuid, SocialPlatform socialPlatform, eUserAction userAction, eSocialPrivacy socialPrivacy);

        [OperationContract]
        UserSocialActionObject[] GetUserActions(InitializationObject initObj, string siteGuid, eUserAction userAction, eAssetType assetType, int assetID, int startIndex, int numOfRecords, SocialPlatform socialPlatform);

        [OperationContract]
        UserSocialActionObject[] GetFriendsActions(InitializationObject initObj, string siteGuid, string[] userActions, eAssetType assetType, int assetID, int startIndex, int numOfRecords, SocialPlatform socialPlatform);

        [OperationContract]
        string DoUserAction(InitializationObject initObj, string siteGuid, eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[] extraParams, SocialPlatform socialPlatform, eAssetType assetType, int assetID);

        [OperationContract]
        string GetUserExternalActionShare(InitializationObject initObj, string siteGuid, eUserAction userAction, SocialPlatform socialPlatform);

        [OperationContract]
        string GetUserInternalActionPrivacy(InitializationObject initObj, string siteGuid, eUserAction userAction, SocialPlatform socialPlatform);

        [OperationContract]
        bool SetUserExternalActionShare(InitializationObject initObj, string siteGuid, eUserAction userAction, SocialPlatform socialPlatform, eSocialActionPrivacy actionPrivacy);

        [OperationContract]
        bool SetUserInternalActionPrivacy(InitializationObject initObj, string siteGuid, eUserAction userAction, SocialPlatform socialPlatform, eSocialActionPrivacy actionPrivacy);
    }
}
