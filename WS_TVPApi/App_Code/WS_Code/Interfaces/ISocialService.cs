using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using TVPApi;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Social;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPApiModule.Objects.Responses;

namespace TVPApiServices
{
    [ServiceContract]
    public interface ISocialService
    {
        [OperationContract]
        TVPApiModule.Objects.Responses.FriendWatchedObject[] GetAllFriendsWatched(InitializationObject initObj, string siteGuid, int maxResult);

        [OperationContract]
        TVPApiModule.Objects.Responses.FriendWatchedObject[] GetFriendsWatchedByMedia(InitializationObject initObj, string siteGuid, int mediaId);

        [OperationContract]
        string[] GetUsersLikedMedia(InitializationObject initObj, int mediaID, bool onlyFriends, int startIndex, int pageSize);

        [OperationContract]
        string[] GetUserFriends(InitializationObject initObj, string siteGuid);

        [OperationContract]
        FBConnectConfig FBConfig(InitializationObject initObj, string sSTG);

        [OperationContract]
        TVPApiModule.Objects.Responses.FacebookResponseObject GetFBUserData(InitializationObject initObj, string sToken, string sSTG);

        [OperationContract]
        TVPApiModule.Objects.Responses.FacebookResponseObject FBUserRegister(InitializationObject initObj, string sToken, bool bCreateNewDomain, bool bGetNewsletter, string sSTG);

        [OperationContract]
        TVPApiModule.Objects.Responses.FacebookResponseObject FBUserMerge(InitializationObject initObj, string sToken, string sFBID, string sUsername, string sPassword);

        [OperationContract]
        string GetUserSocialPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction);

        [OperationContract]
        TVPApiModule.Objects.Responses.eSocialPrivacy[] GetUserAllowedSocialPrivacyList(InitializationObject initObj, string siteGuid);

        [OperationContract]
        bool SetUserSocialPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.eSocialPrivacy socialPrivacy);

        [OperationContract]
        TVPApiModule.Objects.Responses.UserSocialActionObject[] GetUserActions(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform);

        [OperationContract]
        TVPApiModule.Objects.Responses.UserSocialActionObject[] GetFriendsActions(InitializationObject initObj, string siteGuid, string[] userActions, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform);

        [OperationContract]
        string DoUserAction(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[] extraParams, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID);

        [OperationContract]
        string GetUserExternalActionShare(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform);

        [OperationContract]
        string GetUserInternalActionPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform);

        [OperationContract]
        bool SetUserExternalActionShare(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eSocialActionPrivacy actionPrivacy);

        [OperationContract]
        bool SetUserInternalActionPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eSocialActionPrivacy actionPrivacy);
    }
}
