using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using TVPPro.Configuration.PlatformServices;
using TVPPro.SiteManager.TvinciPlatform.Social;

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

        private TvinciPlatform.Social.module m_Module;

        #endregion

        #region C'tor
        public SocialService()
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.Social.module();
            m_Module.Url = PlatformServicesConfiguration.Instance.Data.SocialService.URL;

            wsUserName = PlatformServicesConfiguration.Instance.Data.SocialService.DefaultUser;
            wsPassword = PlatformServicesConfiguration.Instance.Data.SocialService.DefaultPassword;
        }
        #endregion


        // we can use this for DoUserAction
        public string DoSocialAction(int mediaID, string siteGuid, TvinciPlatform.Social.eUserAction userAction, TvinciPlatform.Social.SocialPlatform socialPlatform, string actionParam)
        {
            string sRes = TvinciPlatform.Social.SocialActionResponseStatus.UNKNOWN.ToString();

            try
            {
                TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[] extraPatams;
                if (string.IsNullOrEmpty(actionParam))
                    extraPatams = new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[0];
                else
                {
                    extraPatams = new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[1];
                    extraPatams[0] = new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair() { key = "link", value = actionParam };
                }
                TVPPro.SiteManager.TvinciPlatform.Social.BaseDoUserActionRequest actionRequest = new TVPPro.SiteManager.TvinciPlatform.Social.BaseDoUserActionRequest()
                {
                    m_eAction = userAction,
                    m_eAssetType = TVPPro.SiteManager.TvinciPlatform.Social.eAssetType.MEDIA,
                    m_eSocialPlatform = socialPlatform,
                    m_nAssetID = mediaID,
                    m_oKeyValue = extraPatams,
                    m_sSiteGuid = siteGuid
                };
                TvinciPlatform.Social.DoSocialActionResponse res = m_Module.DoUserAction(wsUserName, wsPassword, actionRequest);

                if (res != null && (res.m_eActionResponseStatusIntern == TvinciPlatform.Social.SocialActionResponseStatus.OK || res.m_eActionResponseStatusIntern == TvinciPlatform.Social.SocialActionResponseStatus.INVALID_ACCESS_TOKEN))
                    sRes = res.ToString();
                else
                    sRes = TvinciPlatform.Social.SocialActionResponseStatus.ERROR.ToString();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in DoSocialAction, Error : {0} Parameters : mediaID {1}, action: {2}, platform: {3}, param: {4}", ex.Message, mediaID,
                    userAction, socialPlatform, actionParam);
            }

            return sRes;
        }

        public TVPPro.SiteManager.TvinciPlatform.Social.SocialActivityDoc[] GetUserSocialActions(string siteGuid, TvinciPlatform.Social.eUserAction userAction, TvinciPlatform.Social.SocialPlatform socialPlatform, bool onlyFriends,
            int startIndex, int numOfItems)
        {
            TVPPro.SiteManager.TvinciPlatform.Social.SocialActivityDoc[] res = null;

            try
            {
                if (onlyFriends)
                {
                    TVPPro.SiteManager.TvinciPlatform.Social.GetFriendsActionsRequest friendActionRequest = new TVPPro.SiteManager.TvinciPlatform.Social.GetFriendsActionsRequest()
                    {
                        m_eSocialPlatform = socialPlatform,
                        m_eUserActions = userAction,
                        m_nNumOfRecords = numOfItems,
                        m_nStartIndex = startIndex,
                        m_sSiteGuid = siteGuid
                    };
                    var response = m_Module.GetFriendsActions(wsUserName, wsPassword, friendActionRequest);
                    if (response != null)
                        res = response.SocialActivity;
                }
                else
                {
                    TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionQueryRequest userActionRequest = new TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionQueryRequest()
                    {
                        m_eSocialPlatform = socialPlatform,
                        m_eUserActions = userAction,
                        m_nNumOfRecords = numOfItems,
                        m_nStartIndex = startIndex,
                        m_sSiteGuid = siteGuid
                    };
                    res = m_Module.GetUserActions(wsUserName, wsPassword, userActionRequest);
                }

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in DoSocialAction, Error : {0} Parameters : siteGuid {1}, action: {2}, platform: {3}", ex.Message, siteGuid,
                    userAction, socialPlatform);
            }

            return res;
        }

        public TvinciPlatform.Social.FriendWatchedObject[] GetAllFriendsWatched(int sGuid, int maxResult)
        {
            TvinciPlatform.Social.FriendWatchedObject[] res = null;
            try
            {
                res = m_Module.GetAllFriendsWatched(wsUserName, wsPassword, sGuid, maxResult);
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
                res = m_Module.GetUserFriends(wsUserName, wsPassword, sGuid);
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in GetUserFriends, Error : {0} Parameters : siteGuid {1}", e.Message, sGuid);
            }

            return res;
        }

        public TvinciPlatform.Social.FriendWatchedObject[] GetFriendsWatchedByMedia(int sGuid, int mediaID)
        {
            TvinciPlatform.Social.FriendWatchedObject[] res = null;
            try
            {
                res = m_Module.GetFriendsWatchedByMedia(wsUserName, wsPassword, sGuid, mediaID);
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in GetFriendsWatchedByMedia, Error : {0} Parameters : siteGuid {1}, mediaID: {2}", e.Message, sGuid, mediaID);
            }

            return res;
        }

        public TvinciPlatform.Social.FacebookConfig getFBConfig()
        {
            TvinciPlatform.Social.FacebookConfig res = null;
            try
            {
                var response = m_Module.FBConfig(wsUserName, wsPassword);
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

        public TvinciPlatform.Social.FacebookResponseObject getFBUserData(string token)
        {
            FacebookResponse facebookResponse = null;
            FacebookResponseObject clientResponse = null;

            try
            {
                facebookResponse = m_Module.FBUserData(wsUserName, wsPassword, token);
                if (facebookResponse != null)
                    clientResponse = facebookResponse.ResponseData;
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in getFBUserData, Error : {0}", e.Message);
            }

            return clientResponse;
        }

        public TvinciPlatform.Social.FacebookResponseObject FBRegister(string token, TvinciPlatform.Social.KeyValuePair[] extra, string sUserIP)
        {
            FacebookResponse facebookResponse = null;
            FacebookResponseObject clientResponse = null;

            try
            {
                facebookResponse = m_Module.FBUserRegister(wsUserName, wsPassword, token, extra, sUserIP);
                if (facebookResponse != null)
                    clientResponse = facebookResponse.ResponseData;
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in FBRegister, Error : {0}", e.Message);
            }

            return clientResponse;
        }

        public TvinciPlatform.Social.FacebookResponseObject FBUserMerge(string token, string fbid, string FBUserName, string FBPassword)
        {
            FacebookResponse facebookResponse = null;
            FacebookResponseObject clientResponse = null;

            try
            {
                facebookResponse = m_Module.FBUserMerge(wsUserName, wsPassword, token, fbid, FBUserName, FBPassword);
                if (facebookResponse != null)
                    clientResponse = facebookResponse.ResponseData;
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in FBUserMerge, Error : {0}", e.Message);
            }

            return clientResponse;
        }

        public bool SetUserSocialPrivacy(int siteGuid, TvinciPlatform.Social.SocialPlatform socialPlatform, TvinciPlatform.Social.eUserAction userAction, TvinciPlatform.Social.eSocialPrivacy privacy)
        {
            bool res = false;

            try
            {
                res = m_Module.SetUserSocialPrivacy(wsUserName, wsPassword, siteGuid, socialPlatform, userAction, privacy);
            }
            catch (Exception e)
            {

                logger.ErrorFormat("Error occurred in SetUserSocialPrivacy, Error : {0}", e.Message);
            }

            return res;
        }

        public bool SetUserInternalActionPrivacy(int siteGuid, TvinciPlatform.Social.SocialPlatform socialPlatform, TvinciPlatform.Social.eUserAction userAction, TvinciPlatform.Social.eSocialActionPrivacy actionPrivacy)
        {
            bool res = false;

            try
            {
                res = m_Module.SetUserInternalActionPrivacy(wsUserName, wsPassword, siteGuid, socialPlatform, userAction, actionPrivacy);
            }
            catch (Exception e)
            {

                logger.ErrorFormat("Error occurred in SetUserInternalActionPrivacy, Error : {0}", e.Message);
            }

            return res;
        }

        public bool SetUserExternalActionShare(int siteGuid, TvinciPlatform.Social.SocialPlatform socialPlatform, TvinciPlatform.Social.eUserAction userAction, TvinciPlatform.Social.eSocialActionPrivacy actionPrivacy)
        {
            bool res = false;

            try
            {
                res = m_Module.SetUserExternalActionShare(wsUserName, wsPassword, siteGuid, socialPlatform, userAction, actionPrivacy);
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
                res = m_Module.FBTokenValidation(wsUserName, wsPassword, sToken);
            }
            catch (Exception e)
            {

                logger.ErrorFormat("Error occurred in SetUserExternalActionPrivacy, Error : {0}", e.Message);
            }
            return res;
        }
    }
}
