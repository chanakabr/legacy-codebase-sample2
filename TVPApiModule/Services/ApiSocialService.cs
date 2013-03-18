using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.TvinciPlatform.Social;
using log4net;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace TVPApiModule.Services
{
    public class ApiSocialService
    {
        #region Fields

        private readonly ILog logger = LogManager.GetLogger(typeof (ApiSocialService));
        private static object instanceLock = new object();

        private int m_groupID;
        private PlatformType m_platform;

        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;

        private TVPPro.SiteManager.TvinciPlatform.Social.module m_Module;

        #endregion

        #region C'tor

        public ApiSocialService(int groupID, PlatformType platform)
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.Social.module();
            m_Module.Url =
                ConfigManager.GetInstance()
                             .GetConfig(groupID, platform)
                             .PlatformServicesConfiguration.Data.SocialService.URL;
            m_wsUserName =
                ConfigManager.GetInstance()
                             .GetConfig(groupID, platform)
                             .PlatformServicesConfiguration.Data.SocialService.DefaultUser;
            m_wsPassword =
                ConfigManager.GetInstance()
                             .GetConfig(groupID, platform)
                             .PlatformServicesConfiguration.Data.SocialService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }

        #endregion

        public TVPPro.SiteManager.TvinciPlatform.Social.SocialActionResponseStatus DoSocialAction(int mediaID,
                                                                                                  string siteGuid,
                                                                                                  TVPPro.SiteManager.
                                                                                                      TvinciPlatform.
                                                                                                      Social.
                                                                                                      SocialAction
                                                                                                      socialAction,
                                                                                                  TVPPro.SiteManager.
                                                                                                      TvinciPlatform.
                                                                                                      Social.
                                                                                                      SocialPlatform
                                                                                                      socialPlatform,
                                                                                                  string actionParam)
        {
            TVPPro.SiteManager.TvinciPlatform.Social.SocialActionResponseStatus eRes =
                TVPPro.SiteManager.TvinciPlatform.Social.SocialActionResponseStatus.UNKNOWN;

            try
            {
                eRes = m_Module.AddUserSocialAction(m_wsUserName, m_wsPassword, mediaID, siteGuid, socialAction,
                                                    socialPlatform, actionParam);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat(
                    "Error occured in DoSocialAction, Error : {0} Parameters : mediaID {1}, action: {2}, platform: {3}, param: {4}",
                    ex.Message, mediaID,
                    socialAction, socialPlatform, actionParam);
            }

            return eRes;
        }

        public TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionObject[] GetUserSocialActions(string siteGuid,
                                                                                                      TVPPro.SiteManager
                                                                                                          .
                                                                                                          TvinciPlatform
                                                                                                          .Social.
                                                                                                          SocialAction
                                                                                                          socialAction,
                                                                                                      TVPPro.SiteManager
                                                                                                          .
                                                                                                          TvinciPlatform
                                                                                                          .Social.
                                                                                                          SocialPlatform
                                                                                                          socialPlatform,
                                                                                                      bool onlyFriends,
                                                                                                      int startIndex,
                                                                                                      int numOfItems)
        {
            TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionObject[] res = null;

            try
            {
                res = m_Module.GetUserSocialActions(m_wsUserName, m_wsPassword, int.Parse(siteGuid), (int) socialAction,
                                                    (int) socialPlatform, onlyFriends, startIndex, numOfItems);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat(
                    "Error occurred in DoSocialAction, Error : {0} Parameters : siteGuid {1}, action: {2}, platform: {3}",
                    ex.Message, siteGuid,
                    socialAction, socialPlatform);
            }

            return res;
        }

        public TVPPro.SiteManager.TvinciPlatform.Social.FriendWatchedObject[] GetAllFriendsWatched(int sGuid,
                                                                                                   int maxResult)
        {
            TVPPro.SiteManager.TvinciPlatform.Social.FriendWatchedObject[] res = null;

            try
            {
                res = m_Module.GetAllFriendsWatched(m_wsUserName, m_wsPassword, sGuid, maxResult);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetAllFriendsWatched, Error : {0} Parameters : siteGuid {1}",
                                   ex.Message, sGuid);
            }

            return res;
        }

        public TVPPro.SiteManager.TvinciPlatform.Social.FriendWatchedObject[] GetFriendsWatchedByMedia(int sGuid,
                                                                                                       int mediaId)
        {
            TVPPro.SiteManager.TvinciPlatform.Social.FriendWatchedObject[] res = null;

            try
            {
                res = m_Module.GetFriendsWatchedByMedia(m_wsUserName, m_wsPassword, sGuid, mediaId);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetFriendsWatchedByMedia, Error : {0} Parameters : mediaId {1}",
                                   ex.Message, mediaId);
            }

            return res;
        }

        public string[] GetUsersLikedMedia(int iSiteGuid, int iMediaID, int iPlatform, bool bOnlyFriends,
                                           int iStartIndex, int iPageSize)
        {
            string[] res = null;

            try
            {
                res = m_Module.GetUsersLikedMedia(m_wsUserName, m_wsPassword, iSiteGuid, iMediaID, iPlatform,
                                                  bOnlyFriends, iStartIndex, iPageSize);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetUsersLikedMedia, Error : {0} Parameters : mediaId {1}",
                                   ex.Message, iMediaID);
            }

            return res;
        }

        public string[] GetUserFriends(int sGuid)
        {
            string[] res = null;

            try
            {
                res = m_Module.GetUserFriends(m_wsUserName, m_wsPassword, sGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetUserFriends, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public FacebookConfig GetFBConfig(string sStg)
        {
            FacebookConfig res = null;

            try
            {
                res = m_Module.FBConfig(m_wsUserName, m_wsPassword, sStg);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetFBConfig, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public FacebookResponseObject GetFBUserData(string stoken, string sSTG)
        {
            FacebookResponseObject res = null;

            try
            {
                res = m_Module.FBUserData(m_wsUserName, m_wsPassword, stoken, sSTG);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetFBConfig, Error : {0} Parameters", ex.Message);
            }

            return res;
        }


        public FacebookResponseObject FBUserRegister(string stoken, string sSTG, List<ExtraKeyValue> oExtra,string sIP)
        {
            FacebookResponseObject res = null;

            try
            {
                res = m_Module.FBUserRegister(m_wsUserName, m_wsPassword, stoken, sSTG, oExtra.ToArray(), sIP);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in FBUserRegister, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public FacebookResponseObject FBUserMerge(string stoken, string sFBID, string sUsername, string sPassword)
        {
            FacebookResponseObject res = null;

            try
            {
                res = m_Module.FBUserMerage(m_wsUserName, m_wsPassword, stoken, sFBID,sUsername,sPassword);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in FBUserMerge, Error : {0} Parameters", ex.Message);
            }

            return res;
        }
    }
}
