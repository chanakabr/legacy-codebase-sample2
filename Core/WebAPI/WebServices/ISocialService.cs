using System.Collections.Generic;
using System.ServiceModel;
using ApiObjects;
using ApiObjects.Social;
using Core.Social;
using Core.Social.Requests;
using Core.Social.Responses;
using Core.Social.SocialFeed;

namespace WebAPI.WebServices
{
    /// <summary>
    /// Pun intended
    /// </summary>
    [ServiceContract(Namespace="http://social.tvinci.com/")]
    public interface ISocialService
    {
        [OperationContract]
        UserSocialActionResponse AddUserSocialAction(string sWSUserName, string sWSPassword, UserSocialActionRequest actionRequest);
        [OperationContract]
        void DeleteFriendsFeed(string sWSUserName, string sWSPassword, int siteGuid);
        [OperationContract]
        void DeleteUserFeed(string sWSUserName, string sWSPassword, int siteGuid);
        [OperationContract]
        UserSocialActionResponse DeleteUserSocialAction(string sWSUserName, string sWSPassword, string siteGuid, string id);
        [OperationContract]
        DoSocialActionResponse DoUserAction(string sWSUserName, string sWSPassword, BaseDoUserActionRequest oActionRequest);
        [OperationContract]
        FacebookConfigResponse FBConfig(string sWSUserName, string sWSPassword);
        [OperationContract]
        SocialObjectReponse FBObjectRequest(string sWSUserName, string sWSPassword, FacebookObjectRequest objRequest);
        [OperationContract]
        FacebookTokenResponse FBTokenValidation(string sWSUserName, string sWSPassword, string sToken);
        [OperationContract]
        FacebookResponse FBUserData(string sWSUserName, string sWSPassword, string token);
        [OperationContract]
        FacebookResponse FBUserDataByUserId(string sWSUserName, string sWSPassword, string userId);
        [OperationContract]
        FacebookResponse FBUserMerge(string sWSUserName, string sWSPassword, string token, string fbid, string sUserName, string sPass);
        [OperationContract]
        FacebookResponse FBUserMergeByUserId(string sWSUserName, string sWSPassword, string userId, string token);
        [OperationContract]
        FacebookResponse FBUserRegister(string sWSUserName, string sWSPassword, string token, List<KeyValuePair> extra, string sUserIP);
        [OperationContract]
        FBSignin FBUserSignin(string sWSUserName, string sWSPassword, string token, string sIP, string deviceID, bool bPreventDoubleLogins);
        [OperationContract]
        FacebookResponse FBUserUnmerge(string sWSUserName, string sWSPassword, string sToken, string sUsername, string sPassword);
        [OperationContract]
        FacebookResponse FBUserUnmergeByUserId(string sWSUserName, string sWSPassword, string sUserId);
        [OperationContract]
        List<FriendWatchedObject> GetAllFriendsWatched(string sWSUserName, string sWSPassword, int nSiteGUID, int nMaxResults = 0);
        [OperationContract]
        int GetAssetLikeCounter(string sWSUserName, string sWSPassword, int nAssetID, eAssetType assetType);
        [OperationContract]
        SocialActivityResponse GetFriendsActions(string sWUserName, string sWSPassword, GetFriendsActionsRequest oFriendActionRequest);
        List<FriendWatchedObject> GetFriendsWatchedByMedia(string sWSUserName, string sWSPassword, int nSiteGUID, int nMediaID);
        [OperationContract]
        SocialFeedResponse GetSocialFeed(string sWSUserName, string sWSPassword, string userId, int assetId, eAssetType assetType, eSocialPlatform socialPlatform, int pageSize, int pageIndex, long epochStartTime, SocialFeedOrderBy orderBy);
        [OperationContract]
        SocialActivityResponse GetUserActions(string sWUserName, string sWSPassword, UserSocialActionQueryRequest oUserActionRequest);
        [OperationContract]
        List<SocialActivityDoc> GetUserActivityFeed(string sWSUserName, string sWSPassword, string sSiteGuid, int nPageSize, int nPageIndex, string sPicDimension);
        [OperationContract]
        eSocialPrivacy[] GetUserAllowedSocialPrivacyList(string sWSUserName, string sWSPassword, int nSiteGUID);
        [OperationContract]
        eSocialActionPrivacy GetUserExternalActionShare(string sWSUserName, string sWSPassword, int nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction);
        [OperationContract]
        string[] GetUserFriends(string sWSUserName, string sWSPassword, int nSiteGUID);
        [OperationContract]
        eSocialActionPrivacy GetUserInternalActionPrivacy(string sWSUserName, string sWSPassword, int nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction);
        [OperationContract]
        string[] GetUsersLikedMedia(string sWSUserName, string sWSPassword, int nSiteGUID, int nMediaID, int nPlatform, bool bOnlyFriends, int nStartIndex, int nNumberOfItems);
        [OperationContract]
        eSocialPrivacy GetUserSocialPrivacy(string sWSUserName, string sWSPassword, int nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction);
        [OperationContract]
        SocialPrivacySettingsResponse GetUserSocialPrivacySettings(string sWSUserName, string sWSPassword, string siteGUID);
        [OperationContract]
        void MergeFriendsActivityFeed(string sWSUserName, string sWSPassword, int siteGuid);
        [OperationContract]
        bool SetUserExternalActionShare(string sWSUserName, string sWSPassword, int nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction, eSocialActionPrivacy eActionPrivacy);
        [OperationContract]
        bool SetUserInternalActionPrivacy(string sWSUserName, string sWSPassword, int nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction, eSocialActionPrivacy eActionPrivacy);
        [OperationContract]
        bool SetUserSocialPrivacy(string sWSUserName, string sWSPassword, int nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction, eSocialPrivacy ePrivacy);
        [OperationContract]
        SocialPrivacySettingsResponse SetUserSocialPrivacySettings(string sWSUserName, string sWSPassword, string siteGUID, SocialPrivacySettings settings);
        [OperationContract]
        bool Share(string sWSUserName, string sWSPassword, string sSiteGuid, string sDeviceUdid, string sFBActionID, int nMediaID, int nAssetID, eAssetType eAssetType);
        [OperationContract]
        bool UpdateFriendsActivityFeed(string sWSUserName, string sWSPassword, int siteGuid, string dbActionId);
    }
}