using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.TvinciPlatform.Social;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Users;
using KLogMonitor;
using System.Reflection;

namespace TVPApiModule.Services
{
    public class ApiSocialService : ApiBase
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object instanceLock = new object();
        private int m_groupID;
        private PlatformType m_platform;
        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;
        private TVPPro.SiteManager.TvinciPlatform.Social.module m_Module;

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

        public TVPPro.SiteManager.TvinciPlatform.Social.DoSocialActionResponse DoSocialAction(int mediaID,
                                                                                                  string siteGuid,
                                                                                                  string udid,
                                                                                                  TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction,
                                                                                                  TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform,
                                                                                                  string actionParam)
        {
            TVPPro.SiteManager.TvinciPlatform.Social.DoSocialActionResponse eRes = null;

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

                BaseDoUserActionRequest actionRequest = new BaseDoUserActionRequest()
                {
                    m_eAction = userAction,
                    m_eAssetType = eAssetType.MEDIA,
                    m_eSocialPlatform = socialPlatform,
                    m_nAssetID = mediaID,
                    m_oKeyValue = extraPatams,
                    m_sDeviceUDID = udid,
                    m_sSiteGuid = siteGuid
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    DoSocialActionResponse response = m_Module.DoUserAction(m_wsUserName, m_wsPassword, actionRequest);
                    if (response != null)
                        eRes = response;
                }

            }
            catch (Exception ex)
            {
                logger.ErrorFormat(
                    "Error occurred in DoSocialAction, Error : {0} Parameters : mediaID {1}, action: {2}, platform: {3}, param: {4}",
                    ex.Message, mediaID,
                    userAction, socialPlatform, actionParam);
            }

            return eRes;
        }

        public TVPPro.SiteManager.TvinciPlatform.Social.SocialActivityDoc[] GetUserSocialActions(string siteGuid,
                                                                                                      TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction,
                                                                                                      TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform,
                                                                                                      bool onlyFriends,
                                                                                                      int startIndex,
                                                                                                      int numOfItems)
        {
            TVPPro.SiteManager.TvinciPlatform.Social.SocialActivityDoc[] res = null;

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

                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        var response = m_Module.GetFriendsActions(m_wsUserName, m_wsPassword, friendActionRequest);
                        if (response != null)
                            res = response.SocialActivity;
                    }
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

                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        res = m_Module.GetUserActions(m_wsUserName, m_wsPassword, userActionRequest);
                    }
                }
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

        public TVPPro.SiteManager.TvinciPlatform.Social.FriendWatchedObject[] GetAllFriendsWatched(int sGuid,
                                                                                                   int maxResult)
        {
            TVPPro.SiteManager.TvinciPlatform.Social.FriendWatchedObject[] res = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.GetAllFriendsWatched(m_wsUserName, m_wsPassword, sGuid, maxResult);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.GetFriendsWatchedByMedia(m_wsUserName, m_wsPassword, sGuid, mediaId);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetFriendsWatchedByMedia, Error : {0} Parameters : mediaId {1}",
                                   ex.Message, mediaId);
            }

            return res;
        }

        public string[] GetUsersLikedMedia(int iSiteGuid, int iMediaID, int iPlatform, bool bOnlyFriends, int iStartIndex, int iPageSize)
        {
            string[] res = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.GetUsersLikedMedia(m_wsUserName, m_wsPassword, iSiteGuid, iMediaID, iPlatform,
                                                      bOnlyFriends, iStartIndex, iPageSize);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.GetUserFriends(m_wsUserName, m_wsPassword, sGuid);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var response = m_Module.FBConfig(m_wsUserName, m_wsPassword);
                    if (response != null)
                    {
                        res = response.FacebookConfig;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetFBConfig, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public FacebookResponseObject GetFBUserData(string stoken)
        {
            FacebookResponse facebookResponse = null;
            FacebookResponseObject clientResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    facebookResponse = m_Module.FBUserData(m_wsUserName, m_wsPassword, stoken);
                    if (facebookResponse != null)
                        clientResponse = facebookResponse.ResponseData;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetFBConfig, Error : {0} Parameters", ex.Message);
            }

            return clientResponse;
        }

        public FacebookResponseObject FBUserRegister(string stoken, string sSTG, List<TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair> oExtra, string sIP)
        {
            FacebookResponse facebookResponse = null;
            FacebookResponseObject clientResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    facebookResponse = m_Module.FBUserRegister(m_wsUserName, m_wsPassword, stoken, oExtra.ToArray(), sIP);
                    if (facebookResponse != null)
                        clientResponse = facebookResponse.ResponseData;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in FBUserRegister, Error : {0} Parameters", ex.Message);
            }

            return clientResponse;
        }

        public FacebookResponseObject FBUserMerge(string stoken, string sFBID, string sUsername, string sPassword)
        {
            FacebookResponse facebookResponse = null;
            FacebookResponseObject clientResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    facebookResponse = m_Module.FBUserMerge(m_wsUserName, m_wsPassword, stoken, sFBID, sUsername, sPassword);
                    if (facebookResponse != null)
                        clientResponse = facebookResponse.ResponseData;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in FBUserMerge, Error : {0} Parameters", ex.Message);
            }

            return clientResponse;
        }

        public eSocialPrivacy GetUserSocialPrivacy(int sGuid, SocialPlatform socialPlatform, eUserAction userAction)
        {
            eSocialPrivacy res = eSocialPrivacy.UNKNOWN;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.GetUserSocialPrivacy(m_wsUserName, m_wsPassword, sGuid, socialPlatform, userAction);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetUserSocialPrivacy, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public eSocialPrivacy[] GetUserAllowedSocialPrivacyList(int sGuid)
        {
            eSocialPrivacy[] res = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.GetUserAllowedSocialPrivacyList(m_wsUserName, m_wsPassword, sGuid);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetUserAllowedSocialPrivacyList, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public bool SetUserSocialPrivacy(int sGuid, SocialPlatform socialPlatform, eUserAction userAction, eSocialPrivacy socialPrivacy)
        {
            bool res = false;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.SetUserSocialPrivacy(m_wsUserName, m_wsPassword, sGuid, socialPlatform, userAction, socialPrivacy);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in SetUserSocialPrivacy, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public SocialActivityDoc[] GetUserActions(string siteGuid, eUserAction userAction, eAssetType assetType, int assetID, int startIndex, int numOfRecords, SocialPlatform socialPlatform)
        {
            SocialActivityDoc[] res = null;

            try
            {
                // create request object
                UserSocialActionQueryRequest request = new UserSocialActionQueryRequest()
                {
                    m_eSocialPlatform = socialPlatform,
                    m_eUserActions = userAction,
                    m_lAssetIDs = new int[] { assetID },
                    m_eAssetType = assetType,
                    m_nNumOfRecords = numOfRecords,
                    m_nStartIndex = startIndex,
                    m_sSiteGuid = siteGuid
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.GetUserActions(m_wsUserName, m_wsPassword, request);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetUserActions, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public SocialActivityDoc[] GetFriendsActions(string siteGuid, string[] userActions, eAssetType assetType, int assetID, int startIndex, int numOfRecords, SocialPlatform socialPlatform)
        {
            SocialActivityDoc[] res = null;

            try
            {
                // create eUserAction OR list
                eUserAction userAction = eUserAction.UNKNOWN;
                foreach (var action in userActions)
                    userAction |= (eUserAction)Enum.Parse(typeof(eUserAction), action);

                // create request object
                GetFriendsActionsRequest request = new GetFriendsActionsRequest()
                {
                    m_eSocialPlatform = socialPlatform,
                    m_eUserActions = userAction,
                    m_lAssetIDs = new int[] { assetID },
                    m_eAssetType = assetType,
                    m_nNumOfRecords = numOfRecords,
                    m_nStartIndex = startIndex,
                    m_sSiteGuid = siteGuid,
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var response = m_Module.GetFriendsActions(m_wsUserName, m_wsPassword, request);
                    if (response != null)
                        res = response.SocialActivity;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetFriendsActions, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public DoSocialActionResponse DoUserAction(string siteGuid, string udid, eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[] extraParams, SocialPlatform socialPlatform, eAssetType assetType, int assetID)
        {
            DoSocialActionResponse res = null;

            try
            {
                // create request object
                BaseDoUserActionRequest request = new BaseDoUserActionRequest()
                {
                    m_eAction = userAction,
                    m_eSocialPlatform = socialPlatform,
                    m_oKeyValue = extraParams,
                    m_sSiteGuid = siteGuid,
                    m_eAssetType = assetType,
                    m_nAssetID = assetID,
                    m_sDeviceUDID = udid,

                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.DoUserAction(m_wsUserName, m_wsPassword, request);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in DoUserAction, Error : {0} Parameters: siteGuid: {1}, udid: {2},  assetType: {3}, assetID: {4}, userAction: {5}", ex.Message, siteGuid, udid, assetType, assetID, userAction.ToString());
            }

            return res;
        }

        public eSocialActionPrivacy GetUserExternalActionShare(string siteGuid, eUserAction userAction, SocialPlatform socialPlatform)
        {
            eSocialActionPrivacy res = eSocialActionPrivacy.UNKNOWN;

            try
            {
                int iSiteGuid = 0;
                if (int.TryParse(siteGuid, out iSiteGuid))
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        res = m_Module.GetUserExternalActionShare(m_wsUserName, m_wsPassword, iSiteGuid, socialPlatform, userAction);
                    }
                }
                else
                    throw new Exception("siteGuid not in the right format");
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetUserFBActionPrivacy, Error : {0} Parameters: siteGuid: {1}", ex.Message, siteGuid);
            }

            return res;
        }

        public eSocialActionPrivacy GetUserInternalActionPrivacy(string siteGuid, eUserAction userAction, SocialPlatform socialPlatform)
        {
            eSocialActionPrivacy res = eSocialActionPrivacy.UNKNOWN;

            try
            {
                int iSiteGuid = 0;
                if (int.TryParse(siteGuid, out iSiteGuid))
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        res = m_Module.GetUserInternalActionPrivacy(m_wsUserName, m_wsPassword, iSiteGuid, socialPlatform, userAction);
                    }
                }
                else
                    throw new Exception("siteGuid not in the right format");
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetUserInternalActionPrivacy, Error : {0} Parameters: siteGuid: {1}", ex.Message, siteGuid);
            }

            return res;
        }

        public bool SetUserExternalActionShare(string siteGuid, eUserAction userAction, SocialPlatform socialPlatform, eSocialActionPrivacy actionPrivacy)
        {
            bool res = false;

            try
            {
                int iSiteGuid = 0;
                if (int.TryParse(siteGuid, out iSiteGuid))
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        res = m_Module.SetUserExternalActionShare(m_wsUserName, m_wsPassword, iSiteGuid, socialPlatform, userAction, actionPrivacy);
                    }
                }
                else
                    throw new Exception("siteGuid not in the right format");
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in SetUserFBActionPrivacy, Error : {0} Parameters: siteGuid: {1}", ex.Message, siteGuid);
            }

            return res;
        }

        public bool SetUserInternalActionPrivacy(string siteGuid, eUserAction userAction, SocialPlatform socialPlatform, eSocialActionPrivacy actionPrivacy)
        {
            bool res = false;

            try
            {
                int iSiteGuid = 0;
                if (int.TryParse(siteGuid, out iSiteGuid))
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        res = m_Module.SetUserInternalActionPrivacy(m_wsUserName, m_wsPassword, iSiteGuid, socialPlatform, userAction, actionPrivacy);
                    }
                }
                else
                    throw new Exception("siteGuid not in the right format");
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in SetUserInternalActionPrivacy, Error : {0} Parameters: siteGuid: {1}", ex.Message, siteGuid);
            }

            return res;
        }

        public FacebookResponseObject FBUserUnmerge(string token, string username, string password)
        {
            FacebookResponse facebookResponse = null;
            FacebookResponseObject clientResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    facebookResponse = m_Module.FBUserUnmerge(m_wsUserName, m_wsPassword, token, username, password);
                    if (facebookResponse != null)
                    {
                        clientResponse = facebookResponse.ResponseData;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in FBUserUnmerge, Error : {0} Parameters: token: {1}, username: {2}, password: {3}", ex.Message, token, username, password);
            }

            return clientResponse;
        }


        public SocialActivityDoc[] GetUserActivityFeed(string siteGuid, int nPageSize, int nPageIndex, string sPicDimension)
        {
            SocialActivityDoc[] response = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.GetUserActivityFeed(m_wsUserName, m_wsPassword, siteGuid, nPageSize, nPageIndex, sPicDimension);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetUserActivityFeed, Error : {0} Parameters: siteGuid: {1}, nPageSize: {2}, nPageIndex: {3}, sPicDimension: {4}", ex.Message, siteGuid, nPageSize, nPageIndex, sPicDimension);
            }

            return response;
        }

        public FacebookTokenResponse FBTokenValidation(string sToken)
        {
            FacebookTokenResponse res = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.FBTokenValidation(m_wsUserName, m_wsPassword, sToken);
                }
            }
            catch (Exception e)
            {

                logger.ErrorFormat("Error occurred in SetUserExternalActionPrivacy, Error : {0}", e.Message);
            }
            return res;
        }

        public FBSignin FBUserSignin(string token, string deviceId, bool preventDoubleLogin)
        {
            FBSignin res = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.FBUserSignin(m_wsUserName, m_wsPassword, token, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP(), deviceId, preventDoubleLogin);
                }
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in FBUserSignin, Error : {0}", e.Message);
            }
            return res;
        }
    }
}
