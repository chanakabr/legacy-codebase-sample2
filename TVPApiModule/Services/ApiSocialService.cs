using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using TVPApi;
using TVPApiModule.Extentions;
using TVPApiModule.Context;
using TVPApiModule.Manager;
using TVPApiModule.Objects;

namespace TVPApiModule.Services
{
    public class ApiSocialService : BaseService
    {
        #region Fields

        private readonly ILog logger = LogManager.GetLogger(typeof(ApiSocialService));
        private static object instanceLock = new object();

        //private int m_groupID;
        //private PlatformType m_platform;

        //private string m_wsUserName = string.Empty;
        //private string m_wsPassword = string.Empty;

        //private TVPPro.SiteManager.TvinciPlatform.Social.module m_Module;

        #endregion

        #region C'tor

        public ApiSocialService(int groupID, PlatformType platform)
        {
            //m_Module = new TVPPro.SiteManager.TvinciPlatform.Social.module();
            //m_Module.Url =
            //    ConfigManager.GetInstance()
            //                 .GetConfig(groupID, platform)
            //                 .PlatformServicesConfiguration.Data.SocialService.URL;
            //m_wsUserName =
            //    ConfigManager.GetInstance()
            //                 .GetConfig(groupID, platform)
            //                 .PlatformServicesConfiguration.Data.SocialService.DefaultUser;
            //m_wsPassword =
            //    ConfigManager.GetInstance()
            //                 .GetConfig(groupID, platform)
            //                 .PlatformServicesConfiguration.Data.SocialService.DefaultPassword;

            //m_groupID = groupID;
            //m_platform = platform;
        }

        public ApiSocialService()
        {
            // TODO: Complete member initialization
        }

        #endregion

        #region Public Static Functions

        public static ApiSocialService Instance(int groupId, PlatformType platform)
        {
            return BaseService.Instance(groupId, platform, eService.SocialService) as ApiSocialService;
        }

        #endregion

        public TVPApiModule.Objects.Responses.SocialActionResponseStatus DoSocialAction(int mediaID,
                                                                                                  string siteGuid,
                                                                                                  string udid,
                                                                                                  TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction,
                                                                                                  TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform,
                                                                                                  string actionParam)
        {
            TVPApiModule.Objects.Responses.SocialActionResponseStatus eRes = TVPApiModule.Objects.Responses.SocialActionResponseStatus.UNKNOWN;

            try
            {
                TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[] extraPatams;
                if (string.IsNullOrEmpty(actionParam))
                    extraPatams = new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[0];
                else
                {
                    extraPatams = new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[1];
                    extraPatams[0] = new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair() { key = "link", value = actionParam};
                }

                TVPPro.SiteManager.TvinciPlatform.Social.BaseDoUserActionRequest actionRequest = new TVPPro.SiteManager.TvinciPlatform.Social.BaseDoUserActionRequest()
                {
                    m_eAction = userAction,
                    m_eAssetType = TVPPro.SiteManager.TvinciPlatform.Social.eAssetType.MEDIA,
                    m_eSocialPlatform = socialPlatform,
                    m_nAssetID = mediaID,
                    m_oKeyValue =  extraPatams,
                    m_sDeviceUDID = udid,
                    m_sSiteGuid = siteGuid
                };
                var res = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).DoUserAction(m_wsUserName, m_wsPassword, actionRequest);
                eRes = res != null ? (TVPApiModule.Objects.Responses.SocialActionResponseStatus)res.m_eActionResponseStatusIntern : TVPApiModule.Objects.Responses.SocialActionResponseStatus.UNKNOWN;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat(
                    "Error occured in DoSocialAction, Error : {0} Parameters : mediaID {1}, action: {2}, platform: {3}, param: {4}",
                    ex.Message, mediaID,
                    userAction, socialPlatform, actionParam);
            }

            return eRes;
        }

        public List<TVPApiModule.Objects.Responses.UserSocialActionObject> GetUserSocialActions(string siteGuid,
                                                                                                      TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction,
                                                                                                      TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform,
                                                                                                      bool onlyFriends,
                                                                                                      int startIndex,
                                                                                                      int numOfItems)
        {
            List<TVPApiModule.Objects.Responses.UserSocialActionObject> res = null;

            try
            {
                TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionObject[] response;

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
                    response = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).GetFriendsActions(m_wsUserName, m_wsPassword, friendActionRequest);
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
                    response = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).GetUserActions(m_wsUserName, m_wsPassword, userActionRequest);
                }

                if (response != null)
                    res = response.Where(usa => usa != null).Select(u => u.ToApiObject()).ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat(
                    "Error occurred in GetUserSocialActions, Error : {0} Parameters : siteGuid {1}, action: {2}, platform: {3}",
                    ex.Message, siteGuid,
                    userAction, socialPlatform);
            }

            return res;
        }

        public List<TVPApiModule.Objects.Responses.FriendWatchedObject> GetAllFriendsWatched(string sGuid, int maxResult)
        {
            List<TVPApiModule.Objects.Responses.FriendWatchedObject> res = null;

            try
            {
                var response = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).GetAllFriendsWatched(m_wsUserName, m_wsPassword, int.Parse(sGuid), maxResult);

                if (response != null)
                    res = response.Where(fw => fw != null).Select(fw => fw.ToApiObject()).ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetAllFriendsWatched, Error : {0} Parameters : siteGuid {1}", ex.Message, sGuid);
            }

            return res;
        }

        public List<TVPApiModule.Objects.Responses.FriendWatchedObject> GetFriendsWatchedByMedia(string sGuid, int mediaId)
        {
            List<TVPApiModule.Objects.Responses.FriendWatchedObject> res = null;

            try
            {
                var response = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).GetFriendsWatchedByMedia(m_wsUserName, m_wsPassword, int.Parse(sGuid), mediaId);
                if (response != null)
                    res = response.Where(fw => fw != null).Select(fw => fw.ToApiObject()).ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetFriendsWatchedByMedia, Error : {0} Parameters : mediaId {1}",
                                   ex.Message, mediaId);
            }

            return res;
        }

        public List<string> GetUsersLikedMedia(string sSiteGuid, int iMediaID, int iPlatform, bool bOnlyFriends, int iStartIndex, int iPageSize)
        {
            List<string> retVal = null;

            try
            {
                var res = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).GetUsersLikedMedia(m_wsUserName, m_wsPassword, int.Parse(sSiteGuid), iMediaID, iPlatform,
                                                  bOnlyFriends, iStartIndex, iPageSize);

                if (res != null)
                    retVal = res.ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetUsersLikedMedia, Error : {0} Parameters : mediaId {1}",
                                   ex.Message, iMediaID);
            }

            return retVal;
        }

        public List<string> GetUserFriends(string sGuid)
        {
            List<string> retVal = null;

            try
            {
                var res = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).GetUserFriends(m_wsUserName, m_wsPassword, int.Parse(sGuid));

                if (res != null)
                    retVal = res.ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetUserFriends, Error : {0} Parameters", ex.Message);
            }

            return retVal;
        }

        public TVPApiModule.Objects.Responses.FacebookConfig GetFBConfig(string sStg)
        {
            TVPApiModule.Objects.Responses.FacebookConfig res = null;

            try
            {
                var response = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).FBConfig(m_wsUserName, m_wsPassword, sStg);
                if (response != null)
                    res = response.ToApiObject();

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetFBConfig, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public TVPApiModule.Objects.Responses.FacebookResponseObject GetFBUserData(string stoken, string sSTG)
        {
            TVPApiModule.Objects.Responses.FacebookResponseObject res = null;

            try
            {
                var response = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).FBUserData(m_wsUserName, m_wsPassword, stoken, sSTG);
                if (response != null)
                    res = response.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetFBConfig, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public TVPApiModule.Objects.Responses.FacebookResponseObject FBUserRegister(string stoken, string sSTG, List<TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair> oExtra, string sIP)
        {
            TVPApiModule.Objects.Responses.FacebookResponseObject res = null;

            try
            {
                var response = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).FBUserRegister(m_wsUserName, m_wsPassword, stoken, sSTG, oExtra.ToArray(), sIP);
                if (response != null)
                    res = response.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in FBUserRegister, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public TVPApiModule.Objects.Responses.FacebookResponseObject FBUserMerge(string stoken, string sFBID, string sUsername, string sPassword)
        {
            TVPApiModule.Objects.Responses.FacebookResponseObject res = null;

            try
            {
                var response = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).FBUserMerage(m_wsUserName, m_wsPassword, stoken, sFBID, sUsername, sPassword);
                if (response != null)
                    res = response.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in FBUserMerge, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public TVPApiModule.Objects.Responses.eSocialPrivacy GetUserSocialPrivacy(string sGuid, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction)
        {
            TVPApiModule.Objects.Responses.eSocialPrivacy res = TVPApiModule.Objects.Responses.eSocialPrivacy.UNKNOWN;

            try
            {
                res = (TVPApiModule.Objects.Responses.eSocialPrivacy)(m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).GetUserSocialPrivacy(m_wsUserName, m_wsPassword, int.Parse(sGuid), socialPlatform, userAction);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetUserSocialPrivacy, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public List<TVPApiModule.Objects.Responses.eSocialPrivacy> GetUserAllowedSocialPrivacyList(string sGuid)
        {
            List<TVPApiModule.Objects.Responses.eSocialPrivacy> res = null;

            try
            {
                var response = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).GetUserAllowedSocialPrivacyList(m_wsUserName, m_wsPassword, int.Parse(sGuid));
                if (response != null)
                    res = response.Select(sp => (TVPApiModule.Objects.Responses.eSocialPrivacy)sp).ToList();

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetUserAllowedSocialPrivacyList, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public bool SetUserSocialPrivacy(int sGuid, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.eSocialPrivacy socialPrivacy)
        {
            bool res = false;

            try
            {
                res = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).SetUserSocialPrivacy(m_wsUserName, m_wsPassword, sGuid, socialPlatform, userAction, socialPrivacy);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in SetUserSocialPrivacy, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public List<TVPApiModule.Objects.Responses.UserSocialActionObject> GetUserActions(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            List<TVPApiModule.Objects.Responses.UserSocialActionObject> res = null;

            try
            {
                // create request object
                TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionQueryRequest request = new TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionQueryRequest()
                {
                    m_eSocialPlatform = socialPlatform,
                    m_eUserActions = userAction,
                    m_nAssetID = assetID,
                    m_eAssetType = assetType,
                    m_nNumOfRecords = numOfRecords,
                    m_nStartIndex = startIndex,
                    m_sSiteGuid = siteGuid
                };

                var response = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).GetUserActions(m_wsUserName, m_wsPassword, request);

                if (response != null)
                    res = response.Where(usa => usa != null).Select(usa => usa.ToApiObject()).ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetUserActions, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public List<TVPApiModule.Objects.Responses.UserSocialActionObject> GetFriendsActions(string siteGuid, string[] userActions, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            List<TVPApiModule.Objects.Responses.UserSocialActionObject> res = null;

            try
            {
                // create eUserAction OR list
                TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction = TVPPro.SiteManager.TvinciPlatform.Social.eUserAction.UNKNOWN;
                foreach (var action in userActions)
                    userAction |= (TVPPro.SiteManager.TvinciPlatform.Social.eUserAction)Enum.Parse(typeof(TVPPro.SiteManager.TvinciPlatform.Social.eUserAction), action);

                // create request object
                TVPPro.SiteManager.TvinciPlatform.Social.GetFriendsActionsRequest request = new TVPPro.SiteManager.TvinciPlatform.Social.GetFriendsActionsRequest()
                {
                    m_eSocialPlatform = socialPlatform,
                    m_eUserActions = userAction,
                    m_nAssetID = assetID,
                    m_eAssetType = assetType,
                    m_nNumOfRecords = numOfRecords,
                    m_nStartIndex = startIndex,
                    m_sSiteGuid = siteGuid,
                };

                var response = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).GetFriendsActions(m_wsUserName, m_wsPassword, request);
                if (response != null)
                    res = response.Where(usa => usa != null).Select(usa => usa.ToApiObject()).ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetFriendsActions, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public TVPApiModule.Objects.Responses.SocialActionResponseStatus DoUserAction(string siteGuid, string udid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[] extraParams, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID)
        {
            TVPApiModule.Objects.Responses.SocialActionResponseStatus res = TVPApiModule.Objects.Responses.SocialActionResponseStatus.UNKNOWN;

            try
            {
                // create request object
                TVPPro.SiteManager.TvinciPlatform.Social.BaseDoUserActionRequest request = new TVPPro.SiteManager.TvinciPlatform.Social.BaseDoUserActionRequest()
                {
                    m_eAction = userAction,
                    m_eSocialPlatform = socialPlatform,
                    m_oKeyValue = extraParams,
                    m_sSiteGuid = siteGuid,
                    m_eAssetType = assetType,
                    m_nAssetID = assetID,
                    m_sDeviceUDID = udid,

                };
                var response = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).DoUserAction(m_wsUserName, m_wsPassword, request);
                if (response != null)
                    res = (TVPApiModule.Objects.Responses.SocialActionResponseStatus)response.m_eActionResponseStatusIntern;
                //res = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).DoUserAction(m_wsUserName, m_wsPassword, request);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in DoUserAction, Error : {0} Parameters: siteGuid: {1}, udid: {2},  assetType: {3}, assetID: {4}, userAction: {5}", ex.Message, siteGuid, udid, assetType, assetID, userAction.ToString());
            }

            return res;
        }

        public TVPApiModule.Objects.Responses.eSocialActionPrivacy GetUserExternalActionShare(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            TVPApiModule.Objects.Responses.eSocialActionPrivacy res = TVPApiModule.Objects.Responses.eSocialActionPrivacy.UNKNOWN;

            try
            {
                res = (TVPApiModule.Objects.Responses.eSocialActionPrivacy)(m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).GetUserExternalActionShare(m_wsUserName, m_wsPassword, int.Parse(siteGuid), socialPlatform, userAction);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetUserFBActionPrivacy, Error : {0} Parameters: siteGuid: {1}", ex.Message, siteGuid);
            }

            return res;
        }

        public TVPApiModule.Objects.Responses.eSocialActionPrivacy GetUserInternalActionPrivacy(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            TVPApiModule.Objects.Responses.eSocialActionPrivacy res = TVPApiModule.Objects.Responses.eSocialActionPrivacy.UNKNOWN;

            try
            {
                res = (TVPApiModule.Objects.Responses.eSocialActionPrivacy)(m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).GetUserInternalActionPrivacy(m_wsUserName, m_wsPassword, int.Parse(siteGuid), socialPlatform, userAction);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetUserInternalActionPrivacy, Error : {0} Parameters: siteGuid: {1}", ex.Message, siteGuid);
            }

            return res;
        }

        public bool SetUserExternalActionShare(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eSocialActionPrivacy actionPrivacy)
        {
            bool res = false;

            try
            {
                int iSiteGuid = 0;
                if (int.TryParse(siteGuid, out iSiteGuid))
                    res = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).SetUserExternalActionShare(m_wsUserName, m_wsPassword, iSiteGuid, socialPlatform, userAction, actionPrivacy);
                else 
                    throw new Exception("siteGuid not in the right format");
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in SetUserFBActionPrivacy, Error : {0} Parameters: siteGuid: {1}", ex.Message, siteGuid);
            }

            return res;
        }

        public bool SetUserInternalActionPrivacy(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eSocialActionPrivacy actionPrivacy)
        {
            bool res = false;

            try
            {
                int iSiteGuid = 0;
                if (int.TryParse(siteGuid, out iSiteGuid))
                    res = (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module).SetUserInternalActionPrivacy(m_wsUserName, m_wsPassword, iSiteGuid, socialPlatform, userAction, actionPrivacy);
                else 
                    throw new Exception("siteGuid not in the right format");
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in SetUserInternalActionPrivacy, Error : {0} Parameters: siteGuid: {1}", ex.Message, siteGuid);
            }

            return res;
        }
    }
}
