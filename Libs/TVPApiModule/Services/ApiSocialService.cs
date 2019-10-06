using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using KLogMonitor;
using System.Reflection;
using Core.Social.Requests;
using ApiObjects;
using Core.Social.Responses;
using Core.Social;
using TVPApiModule.Manager;

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

        public ApiSocialService(int groupID, PlatformType platform)
        {
            m_wsUserName = GroupsManager.GetGroup(groupID).SocialCredentials.Username;
            m_wsPassword = GroupsManager.GetGroup(groupID).SocialCredentials.Password;

            m_groupID = groupID;
            m_platform = platform;
        }

        public DoSocialActionResponse DoSocialAction(int mediaID,
                                                                                                  string siteGuid,
                                                                                                  string udid,
                                                                                                  eUserAction userAction,
                                                                                                  SocialPlatform socialPlatform,
                                                                                                  string actionParam)
        {
            DoSocialActionResponse eRes = null;

            try
            {
                KeyValuePair[] extraPatams;
                if (string.IsNullOrEmpty(actionParam))
                    extraPatams = new KeyValuePair[0];
                else
                {
                    extraPatams = new KeyValuePair[1];
                    extraPatams[0] = new KeyValuePair() { key = "link", value = actionParam };
                }

                BaseDoUserActionRequest actionRequest = new BaseDoUserActionRequest()
                {
                    m_eAction = userAction,
                    m_eAssetType = eAssetType.MEDIA,
                    m_eSocialPlatform = socialPlatform,
                    m_nAssetID = mediaID,
                    m_oKeyValue = extraPatams.ToList(),
                    m_sDeviceUDID = udid,
                    m_sSiteGuid = siteGuid
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    DoSocialActionResponse response = 
                        Core.Social.Module.DoUserAction(m_groupID, actionRequest);
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

        public SocialActivityDoc[] GetUserSocialActions(string siteGuid,
                                                                                                      eUserAction userAction,
                                                                                                      SocialPlatform socialPlatform,
                                                                                                      bool onlyFriends,
                                                                                                      int startIndex,
                                                                                                      int numOfItems)
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

                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        var response = Core.Social.Module.GetFriendsActions(m_groupID, friendActionRequest);
                        if (response != null && response.SocialActivity != null)
                            res = response.SocialActivity.ToArray();
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
                        var response = Core.Social.Module.GetUserActions(m_groupID, userActionRequest);
                        if (response != null && response.SocialActivity != null)
                            res = response.SocialActivity.ToArray();
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

        public FriendWatchedObject[] GetAllFriendsWatched(int sGuid,
                                                                                                   int maxResult)
        {
            FriendWatchedObject[] res = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var innerResult = Core.Social.Module.GetAllFriendsWatched(m_groupID, sGuid, maxResult);

                    if (innerResult != null)
                    {
                        res = innerResult.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetAllFriendsWatched, Error : {0} Parameters : siteGuid {1}",
                                   ex.Message, sGuid);
            }

            return res;
        }

        public FriendWatchedObject[] GetFriendsWatchedByMedia(int sGuid, int mediaId)
        {
            FriendWatchedObject[] res = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var innerResult = Core.Social.Module.GetFriendsWatchedByMedia(m_groupID, sGuid, mediaId);

                    if (innerResult != null)
                    {
                        res = innerResult.ToArray();
                    }
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
                    res = Core.Social.Module.GetUsersLikedMedia(m_groupID, iSiteGuid, iMediaID, iPlatform,
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
                    res = Core.Social.Module.GetUserFriends(m_groupID, sGuid);
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
                    var response = Core.Social.Module.FBConfig(m_groupID);
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
                    facebookResponse = Core.Social.Module.FBUserData(m_groupID, stoken);
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

        public FacebookResponseObject FBUserRegister(string stoken, string sSTG, List<KeyValuePair> oExtra, string sIP)
        {
            FacebookResponse facebookResponse = null;
            FacebookResponseObject clientResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    facebookResponse = Core.Social.Module.FBUserRegister(m_groupID, stoken, oExtra, sIP);
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
                    facebookResponse = Core.Social.Module.FBUserMerge(m_groupID, stoken, sFBID, sUsername, sPassword);
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
                    res = Core.Social.Module.GetUserSocialPrivacy(m_groupID, sGuid, socialPlatform, userAction);
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
                    res = Core.Social.Module.GetUserAllowedSocialPrivacyList(m_groupID, sGuid);
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
                    res = Core.Social.Module.SetUserSocialPrivacy(m_groupID, sGuid, socialPlatform, userAction, socialPrivacy);
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
                    m_lAssetIDs = new List<int>() { assetID },
                    m_eAssetType = assetType,
                    m_nNumOfRecords = numOfRecords,
                    m_nStartIndex = startIndex,
                    m_sSiteGuid = siteGuid
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var response = Core.Social.Module.GetUserActions(m_groupID, request);
                    if (response != null && response.SocialActivity != null)
                        res = response.SocialActivity.ToArray();
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
                    m_lAssetIDs = new List<int>() { assetID },
                    m_eAssetType = assetType,
                    m_nNumOfRecords = numOfRecords,
                    m_nStartIndex = startIndex,
                    m_sSiteGuid = siteGuid,
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var response = Core.Social.Module.GetFriendsActions(m_groupID, request);
                    if (response != null && response.SocialActivity != null)
                        res = response.SocialActivity.ToArray();
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in GetFriendsActions, Error : {0} Parameters", ex.Message);
            }

            return res;
        }

        public DoSocialActionResponse DoUserAction(string siteGuid, string udid, eUserAction userAction, KeyValuePair[] extraParams, SocialPlatform socialPlatform, eAssetType assetType, int assetID)
        {
            DoSocialActionResponse res = null;

            try
            {
                // create request object
                BaseDoUserActionRequest request = new BaseDoUserActionRequest()
                {
                    m_eAction = userAction,
                    m_eSocialPlatform = socialPlatform,
                    m_oKeyValue = extraParams == null ? null : extraParams.ToList(),
                    m_sSiteGuid = siteGuid,
                    m_eAssetType = assetType,
                    m_nAssetID = assetID,
                    m_sDeviceUDID = udid,

                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.Social.Module.DoUserAction(m_groupID, request);
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
                        res = Core.Social.Module.GetUserExternalActionShare(m_groupID, iSiteGuid, socialPlatform, userAction);
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
                        res = Core.Social.Module.GetUserInternalActionPrivacy(m_groupID, iSiteGuid, socialPlatform, userAction);
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
                        res = Core.Social.Module.SetUserExternalActionShare(m_groupID, iSiteGuid, socialPlatform, userAction, actionPrivacy);
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
                        res = Core.Social.Module.SetUserInternalActionPrivacy(m_groupID, iSiteGuid, socialPlatform, userAction, actionPrivacy);
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
                    facebookResponse = Core.Social.Module.FBUserUnmerge(m_groupID, token, username, password);
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
                    var res = Core.Social.Module.GetUserActivityFeed(m_groupID, siteGuid, nPageSize, nPageIndex, sPicDimension);

                    if (res != null)
                    {
                        response = res.ToArray();
                    }
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
                    res = Core.Social.Module.FBTokenValidation(m_groupID, sToken);
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
                    res = Core.Social.Module.FBUserSignin(m_groupID, token, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP(), deviceId, preventDoubleLogin);
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
