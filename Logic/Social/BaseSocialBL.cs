using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DAL;
using ApiObjects;
using ApiObjects.Social;

namespace Core.Social
{
    public abstract class BaseSocialBL
    {
        public static BaseSocialBL GetBaseSocialImpl(int nGroupID)
        {
            return new TvinciSocialBL(nGroupID);
        }

        protected int m_nGroupID;

        public BaseSocialBL(int nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        public abstract string GetGroupFBNamespace();
        public abstract bool InsertUserSocialAction(SocialActivityDoc oSocialDoc, out string sDBRecordID);
        public abstract bool DeleteUserSocialAction(string sSiteGuid, int nAssetID, eAssetType assetType, eUserAction userAction, SocialPlatform eSocialPlatform);
        public abstract bool GetUserSocialAction(string sSiteGuid, ApiObjects.SocialPlatform eSocialPlatform, eAssetType assetType, eUserAction eUserAction, int nAssetID, out SocialActivityDoc oSocialActionDoc);
        public abstract List<SocialActivityDoc> GetUserSocialAction(string sSiteGuid, ApiObjects.SocialPlatform eSocialPlatform, eAssetType assetType, List<int> lSocialActions, List<int> lAssetIDs);
        public abstract bool GetUserSocialAction(string sSiteGuid, int nNumOfRecords, int nSkip, out List<SocialActivityDoc> lActions);
        public abstract List<SocialActivityDoc> GetFriendsSocialActions(List<string> lFriendIds, SocialPlatform eSocialPlatform, eAssetType assetType, List<int> lSocialActions, List<int> lAssetIDs, string userId);
        public abstract bool DeleteActivityFromUserFeed(string sFeedDocID);
        public abstract List<FBUser> GetFBFriendsFromDB(List<long> lFBFriendList);
        public abstract eSocialPrivacy GetUserSocialPrivacy(int nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction);
        public abstract bool SetUserSocialPrivacy(int nSiteGuid, SocialPlatform eSocialPlatform, eUserAction eAction, eSocialPrivacy eNewPrivacy);
        public abstract void GetFBObjectID(int nAssetID, eAssetType assetType, ref string sObjectID);
        public abstract string GetMediaFBObjectID(int nMediaID);
        public abstract string GetProgramFBObjectID(int nProgID);
        public abstract string GetMediaLinkPostParameters(int nMediaID, ref Dictionary<string, string> dParams);
        public abstract bool IncrementAssetLikeCounter(int nAssetID, eAssetType assetType);
        public abstract bool DecrementAssetLikeCounter(int nAssetID, eAssetType assetType);
        public abstract bool SetAssetFBObjectID(int nAssetID, eAssetType assetType, string sAssetFBObjectID);
        public abstract bool RemoveUser(int nGroupID, string sSiteGuid);
        public abstract FacebookConfig GetFBConfig(string sConnStr = "");
        public abstract List<ApiObjects.KeyValuePair> GetFriendsWatchCount(List<int> lFriendsSiteGuid, int nLimit);
        public abstract List<string> GetFriendsWhoWatchedMedia(int nMediaID, List<int> lFriendsGuid, int nLimit);
        public abstract eSocialActionPrivacy GetUserExternalActionShare(string sSiteGuid, SocialPlatform eSocialPlatform, eUserAction eAction);
        public abstract eSocialActionPrivacy GetUserInternalActionShare(string sSiteGuid, SocialPlatform eSocialPlatform, eUserAction eAction);
        public abstract bool SetUserInternalActionShare(string sSiteGuid, SocialPlatform eSocialPlatform, eUserAction eAction, eSocialActionPrivacy ePrivacy);
        public abstract bool SetUserExternalActionShare(string sSiteGuid, SocialPlatform eSocialPlatform, eUserAction eAction, eSocialActionPrivacy ePrivacy);
        public abstract EPGChannelProgrammeObject GetProgramInfo(int nProgID);
        public abstract int GetProgramMediaID(int nProgID);
        public abstract List<FriendWatchedObject> GetAllFriendsWatchedMedia(int nMediaID, List<string> lFriendsSiteGuid);
        public abstract int GetAssetMediaID(int nAssetID, eAssetType assetType);
        public abstract bool RateAsset(string sSiteGuid, int nAssetID, eAssetType assetType, int nRateVal);
        public abstract int GetAssetLikeCounter(int nAssetID, eAssetType assetType);
        public abstract string GetMediaName(int nProgID);
        public abstract string[] GetUsersLikedMedia(int nUserGUID, int nMediaID, int nPlatform, int nStartIndex, int nNumberOfItems);
        public abstract bool UpdateUserActivityFeed(List<string> sOwnerSiteGuids, string sNewActivityID);
        public abstract bool GetUserActivityFeed(string sSiteGuid, int nNumOfRecords, int nStartIndex, string sPicDimensions, out List<SocialActivityDoc> lResult);
        public abstract bool GetUserActivityFeedIds(string sSiteGuid, int nNumOfRecords, int nStartIndex, out List<string> lResult);
        public abstract bool GetFeedIDsByActorID(string sActorSiteGuid, int nNumOfDocs, out List<string> docIDs);  //returns all activity docs that have actor ID
        public abstract bool InsertFriendsActivitiesToUserActivityFeed(string sSiteGuiid, List<SocialActivityDoc> lFriendsActivities);


        public abstract ApiObjects.Social.SocialPrivacySettingsResponse SetUserSocialPrivacySettings(string siteGUID, int groupID, SocialPrivacySettings settings);
        public abstract ApiObjects.Social.SocialPrivacySettingsResponse GetUserSocialPrivacySettings(string siteGUID, int groupID);
        //public abstract UserSocialActionResponse AddUserSocialAction(int groupID, UserSocialActionRequest actionRequest);
        public abstract ApiObjects.Response.Status InsertUserSocialAction(SocialActivityDoc oSocialDoc, SocialPrivacySettings privacy, out string sDBRecordID);
        public abstract bool GetUserSocialAction(string id, out SocialActivityDoc oSocialActionDoc);
       
    }
}
