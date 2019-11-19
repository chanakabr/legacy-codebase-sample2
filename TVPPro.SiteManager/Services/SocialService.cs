using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiObjects;
using ApiObjects.Social;
using Core.Social;
using Core.Social.Requests;
using Core.Social.Responses;
using KLogMonitor;
using TVPPro.Configuration.PlatformServices;
using KeyValuePair = ApiObjects.KeyValuePair;

namespace TVPPro.SiteManager.Services
{
    public class SocialService
    {
        #region Fields
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        static object instanceLock = new object();

        private static SocialService m_Instance;
        public static SocialService Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (instanceLock)
                    {
                        m_Instance = new SocialService();
                    }
                }

                return m_Instance;
            }
        }


        private string wsUserName;
        private string wsPassword;
        private int groupId;

        #endregion

        #region C'tor
        public SocialService()
        {
            wsUserName = PlatformServicesConfiguration.Instance.Data.SocialService.DefaultUser;
            wsPassword = PlatformServicesConfiguration.Instance.Data.SocialService.DefaultPassword;
            groupId = Core.Social.Utils.GetGroupID(wsUserName, wsPassword);
        }
        #endregion


        // we can use this for DoUserAction
        public string DoSocialAction(int mediaID, string siteGuid, eUserAction userAction, SocialPlatform socialPlatform, string actionParam)
        {
            string sRes = SocialActionResponseStatus.UNKNOWN.ToString();

            try
            {
                List<KeyValuePair> extraPatams;
                if (string.IsNullOrEmpty(actionParam))
                    extraPatams = new List<KeyValuePair>();
                else
                {
                    extraPatams = new List<KeyValuePair>() {
                        new KeyValuePair(){ key = "link", value = actionParam } };
                }
                BaseDoUserActionRequest actionRequest = new BaseDoUserActionRequest()
                {
                    m_eAction = userAction,
                    m_eAssetType = eAssetType.MEDIA,
                    m_eSocialPlatform = socialPlatform,
                    m_nAssetID = mediaID,
                    m_oKeyValue = extraPatams,
                    m_sSiteGuid = siteGuid
                };
                DoSocialActionResponse res = Core.Social.Module.DoUserAction(groupId, actionRequest);

                if (res != null && (res.m_eActionResponseStatusIntern == SocialActionResponseStatus.OK || res.m_eActionResponseStatusIntern == SocialActionResponseStatus.INVALID_ACCESS_TOKEN))
                    sRes = res.ToString();
                else
                    sRes = SocialActionResponseStatus.ERROR.ToString();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in DoSocialAction, Error : {0} Parameters : mediaID {1}, action: {2}, platform: {3}, param: {4}", ex.Message, mediaID,
                    userAction, socialPlatform, actionParam);
            }

            return sRes;
        }

        public SocialActivityDoc[] GetUserSocialActions(string siteGuid, eUserAction userAction, SocialPlatform socialPlatform, bool onlyFriends,
            int startIndex, int numOfItems)
        {
            SocialActivityDoc[] res = null;

            try
            {
                if (onlyFriends)
                {
                    GetFriendsActionsRequest friendActionRequest = new GetFriendsActionsRequest()
                    {
                        m_eSocialPlatform = socialPlatform,
                        m_eUserActions = userAction,
                        m_nNumOfRecords = numOfItems,
                        m_nStartIndex = startIndex,
                        m_sSiteGuid = siteGuid
                    };
                    var response = Core.Social.Module.GetFriendsActions(groupId, friendActionRequest);
                    if (response != null && response.SocialActivity != null)
                        res = response.SocialActivity.ToArray();
                }
                else
                {
                    UserSocialActionQueryRequest userActionRequest = new UserSocialActionQueryRequest()
                    {
                        m_eSocialPlatform = socialPlatform,
                        m_eUserActions = userAction,
                        m_nNumOfRecords = numOfItems,
                        m_nStartIndex = startIndex,
                        m_sSiteGuid = siteGuid
                    };
                    var response = Core.Social.Module.GetUserActions(groupId, userActionRequest);
                    if (response != null && response.SocialActivity != null)
                        res = response.SocialActivity.ToArray();
                }

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in DoSocialAction, Error : {0} Parameters : siteGuid {1}, action: {2}, platform: {3}", ex.Message, siteGuid,
                    userAction, socialPlatform);
            }

            return res;
        }

        public FriendWatchedObject[] GetAllFriendsWatched(int sGuid, int maxResult)
        {
            FriendWatchedObject[] res = null;
            try
            {
                var temp = Core.Social.Module.GetAllFriendsWatched(groupId, sGuid, maxResult);
                res = temp == null ? null : temp.ToArray();
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in GetAllFriendsWatched, Error : {0} Parameters : siteGuid {1}, maxResult: {2}", e.Message, sGuid, maxResult);
            }

            return res;
        }

        public string[] GetUserFriends(int sGuid)
        {
            string[] res = null;
            try
            {
                res = Core.Social.Module.GetUserFriends(groupId, sGuid);
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in GetUserFriends, Error : {0} Parameters : siteGuid {1}", e.Message, sGuid);
            }

            return res;
        }

        public FriendWatchedObject[] GetFriendsWatchedByMedia(int sGuid, int mediaID)
        {
            FriendWatchedObject[] res = null;
            try
            {
                var temp = Core.Social.Module.GetFriendsWatchedByMedia(groupId, sGuid, mediaID);
                res = temp == null ? null : temp.ToArray();
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in GetFriendsWatchedByMedia, Error : {0} Parameters : siteGuid {1}, mediaID: {2}", e.Message, sGuid, mediaID);
            }

            return res;
        }

        public FacebookConfig getFBConfig()
        {
            FacebookConfig res = null;
            try
            {
                var response = Core.Social.Module.FBConfig(groupId);
                if (response != null)
                {
                    res = response.FacebookConfig;
                }
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in getFBConfig, Error : {0}", e.Message);
            }

            return res;
        }

        public FacebookResponseObject getFBUserData(string token)
        {
            FacebookResponse facebookResponse = null;
            FacebookResponseObject clientResponse = null;

            try
            {
                facebookResponse = Core.Social.Module.FBUserData(groupId, token);
                if (facebookResponse != null)
                    clientResponse = facebookResponse.ResponseData;
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in getFBUserData, Error : {0}", e.Message);
            }

            return clientResponse;
        }

        public FacebookResponseObject FBRegister(string token, KeyValuePair[] extra, string sUserIP)
        {
            FacebookResponse facebookResponse = null;
            FacebookResponseObject clientResponse = null;

            try
            {
                facebookResponse = Core.Social.Module.FBUserRegister(groupId, token, 
                    extra == null ? null : extra.ToList(), 
                    sUserIP);
                if (facebookResponse != null)
                    clientResponse = facebookResponse.ResponseData;
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in FBRegister, Error : {0}", e.Message);
            }

            return clientResponse;
        }

        public FacebookResponseObject FBUserMerge(string token, string fbid, string FBUserName, string FBPassword)
        {
            FacebookResponse facebookResponse = null;
            FacebookResponseObject clientResponse = null;

            try
            {
                facebookResponse = Core.Social.Module.FBUserMerge(groupId, token, fbid, FBUserName, FBPassword);
                if (facebookResponse != null)
                    clientResponse = facebookResponse.ResponseData;
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in FBUserMerge, Error : {0}", e.Message);
            }

            return clientResponse;
        }

        public bool SetUserSocialPrivacy(int siteGuid, SocialPlatform socialPlatform, eUserAction userAction, eSocialPrivacy privacy)
        {
            bool res = false;

            try
            {
                res = Core.Social.Module.SetUserSocialPrivacy(groupId, siteGuid, socialPlatform, userAction, privacy);
            }
            catch (Exception e)
            {

                logger.ErrorFormat("Error occurred in SetUserSocialPrivacy, Error : {0}", e.Message);
            }

            return res;
        }

        public bool SetUserInternalActionPrivacy(int siteGuid, SocialPlatform socialPlatform, eUserAction userAction, eSocialActionPrivacy actionPrivacy)
        {
            bool res = false;

            try
            {
                res = Core.Social.Module.SetUserInternalActionPrivacy(groupId, siteGuid, socialPlatform, userAction, actionPrivacy);
            }
            catch (Exception e)
            {

                logger.ErrorFormat("Error occurred in SetUserInternalActionPrivacy, Error : {0}", e.Message);
            }

            return res;
        }

        public bool SetUserExternalActionShare(int siteGuid, SocialPlatform socialPlatform, eUserAction userAction, eSocialActionPrivacy actionPrivacy)
        {
            bool res = false;

            try
            {
                res = Core.Social.Module.SetUserExternalActionShare(groupId, siteGuid, socialPlatform, userAction, actionPrivacy);
            }
            catch (Exception e)
            {

                logger.ErrorFormat("Error occurred in SetUserExternalActionPrivacy, Error : {0}", e.Message);
            }
            return res;
        }

        public FacebookTokenResponse FBTokenValidation(string sToken)
        {
            FacebookTokenResponse res = null;

            try
            {
                res = Core.Social.Module.FBTokenValidation(groupId, sToken);
            }
            catch (Exception e)
            {

                logger.ErrorFormat("Error occurred in SetUserExternalActionPrivacy, Error : {0}", e.Message);
            }
            return res;
        }
    }
}
