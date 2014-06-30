using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using TVPApiModule.Extentions;
using TVPApiModule.Context;
using TVPPro.SiteManager.TvinciPlatform.Social;

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

        #region Properties

        protected TVPPro.SiteManager.TvinciPlatform.Social.module Social
        {
            get
            {
                return (m_Module as TVPPro.SiteManager.TvinciPlatform.Social.module);
            }
        }

        #endregion

        public TVPApiModule.Objects.Responses.DoSocialActionResponse DoSocialAction(int mediaID,
                                                                                                  string siteGuid,
                                                                                                  string udid,
                                                                                                  TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction,
                                                                                                  TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform,
                                                                                                  string actionParam)
        {
            TVPApiModule.Objects.Responses.DoSocialActionResponse eRes = null;

            eRes = Execute(() =>
                {
                    TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[] extraParams;
                    if (string.IsNullOrEmpty(actionParam))
                        extraParams = new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[0];
                    else
                    {
                        extraParams = new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[1];
                        extraParams[0] = new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair() { key = "link", value = actionParam };
                    }

                    BaseDoUserActionRequest actionRequest = new BaseDoUserActionRequest()
                    {
                        m_eAction = userAction,
                        m_eAssetType = eAssetType.MEDIA,
                        m_eSocialPlatform = socialPlatform,
                        m_nAssetID = mediaID,
                        m_oKeyValue = extraParams,
                        m_sDeviceUDID = udid,
                        m_sSiteGuid = siteGuid
                    };
                    DoSocialActionResponse response = Social.DoUserAction(m_wsUserName, m_wsPassword, actionRequest);
                    if (response != null)
                        eRes = response.ToApiObject();

                    return eRes;
                }) as TVPApiModule.Objects.Responses.DoSocialActionResponse;

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

            res = Execute(() =>
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
                        response = Social.GetFriendsActions(m_wsUserName, m_wsPassword, friendActionRequest);
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
                        response = Social.GetUserActions(m_wsUserName, m_wsPassword, userActionRequest);
                    }

                    if (response != null)
                        res = response.Where(usa => usa != null).Select(u => u.ToApiObject()).ToList();


                    return res;
                }) as List<TVPApiModule.Objects.Responses.UserSocialActionObject>;

            return res;

            //try
            //{
            //    TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionObject[] response;

            //    if (onlyFriends)
            //    {
            //        TVPPro.SiteManager.TvinciPlatform.Social.GetFriendsActionsRequest friendActionRequest = new TVPPro.SiteManager.TvinciPlatform.Social.GetFriendsActionsRequest()
            //        {
            //            m_eSocialPlatform = socialPlatform,
            //            m_eUserActions = userAction,
            //            m_nNumOfRecords = numOfItems,
            //            m_nStartIndex = startIndex,
            //            m_sSiteGuid = siteGuid
            //        };
            //        response = Social.GetFriendsActions(m_wsUserName, m_wsPassword, friendActionRequest);
            //    }
            //    else
            //    {
            //        TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionQueryRequest userActionRequest = new TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionQueryRequest()
            //        {
            //            m_eSocialPlatform = socialPlatform,
            //            m_eUserActions = userAction,
            //            m_nNumOfRecords = numOfItems,
            //            m_nStartIndex = startIndex,
            //            m_sSiteGuid = siteGuid
            //        };
            //        response = Social.GetUserActions(m_wsUserName, m_wsPassword, userActionRequest);
            //    }

            //    if (response != null)
            //        res = response.Where(usa => usa != null).Select(u => u.ToApiObject()).ToList();
            //}
            //catch (Exception ex)
            //{
            //    logger.ErrorFormat(
            //        "Error occurred in GetUserSocialActions, Error : {0} Parameters : siteGuid {1}, action: {2}, platform: {3}",
            //        ex.Message, siteGuid,
            //        userAction, socialPlatform);
            //}

            //return res;
        }

        public List<TVPApiModule.Objects.Responses.FriendWatchedObject> GetAllFriendsWatched(string sGuid, int maxResult)
        {
            List<TVPApiModule.Objects.Responses.FriendWatchedObject> res = null;

            res = Execute(() =>
                {
                    var response = Social.GetAllFriendsWatched(m_wsUserName, m_wsPassword, int.Parse(sGuid), maxResult);

                    if (response != null)
                        res = response.Where(fw => fw != null).Select(fw => fw.ToApiObject()).ToList();

                    return res;
                }) as List<TVPApiModule.Objects.Responses.FriendWatchedObject>;

            return res;
        }

        public List<TVPApiModule.Objects.Responses.FriendWatchedObject> GetFriendsWatchedByMedia(string sGuid, int mediaId)
        {
            List<TVPApiModule.Objects.Responses.FriendWatchedObject> res = null;

            res = Execute(() =>
                {
                    var response = Social.GetFriendsWatchedByMedia(m_wsUserName, m_wsPassword, int.Parse(sGuid), mediaId);
                    if (response != null)
                        res = response.Where(fw => fw != null).Select(fw => fw.ToApiObject()).ToList();

                    return res;
                }) as List<TVPApiModule.Objects.Responses.FriendWatchedObject>;

            return res;
        }

        public List<string> GetUsersLikedMedia(string sSiteGuid, int iMediaID, int iPlatform, bool bOnlyFriends, int iStartIndex, int iPageSize)
        {
            List<string> retVal = null;

            retVal = Execute(() =>
                {
                    var res = Social.GetUsersLikedMedia(m_wsUserName, m_wsPassword, int.Parse(sSiteGuid), iMediaID, iPlatform,
                                                          bOnlyFriends, iStartIndex, iPageSize);
                    if (res != null)
                        retVal = res.ToList();

                    return retVal;
                }) as List<string>;

            return retVal;
        }

        public List<string> GetUserFriends(string sGuid)
        {
            List<string> retVal = null;

            retVal = Execute(() =>
                {
                    var res = Social.GetUserFriends(m_wsUserName, m_wsPassword, int.Parse(sGuid));

                    if (res != null)
                        retVal = res.ToList();

                    return retVal;
                }) as List<string>;

            return retVal;
        }

        public TVPApiModule.Objects.Responses.FacebookConfig GetFBConfig(string sStg)
        {
            TVPApiModule.Objects.Responses.FacebookConfig res = null;

            res = Execute(() =>
                {
                    var response = Social.FBConfig(m_wsUserName, m_wsPassword, sStg);
                    if (response != null)
                        res = response.ToApiObject();

                    return res;
                }) as TVPApiModule.Objects.Responses.FacebookConfig;

            return res;
        }

        public TVPApiModule.Objects.Responses.FacebookResponseObject GetFBUserData(string stoken, string sSTG)
        {
            TVPApiModule.Objects.Responses.FacebookResponseObject res = null;

            res = Execute(() =>
                {
                    var response = Social.FBUserData(m_wsUserName, m_wsPassword, stoken, sSTG);
                    if (response != null)
                        res = response.ToApiObject();

                    return res;
                }) as TVPApiModule.Objects.Responses.FacebookResponseObject;

            return res;
        }

        public TVPApiModule.Objects.Responses.FacebookResponseObject FBUserRegister(string stoken, string sSTG, List<TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair> oExtra, string sIP)
        {
            TVPApiModule.Objects.Responses.FacebookResponseObject res = null;

            res = Execute(() =>
                {
                    var response = Social.FBUserRegister(m_wsUserName, m_wsPassword, stoken, sSTG, oExtra.ToArray(), sIP);
                    if (response != null)
                        res = response.ToApiObject();

                    return res;
                }) as TVPApiModule.Objects.Responses.FacebookResponseObject;

            return res;
        }

        public TVPApiModule.Objects.Responses.FacebookResponseObject FBUserMerge(string stoken, string sFBID, string sUsername, string sPassword)
        {
            TVPApiModule.Objects.Responses.FacebookResponseObject res = null;

            res = Execute(() =>
                {
                    var response = Social.FBUserMerage(m_wsUserName, m_wsPassword, stoken, sFBID, sUsername, sPassword);
                    if (response != null)
                        res = response.ToApiObject();

                    return res;
                }) as TVPApiModule.Objects.Responses.FacebookResponseObject;

            return res;
        }

        public TVPApiModule.Objects.Responses.eSocialPrivacy GetUserSocialPrivacy(string sGuid, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction)
        {
            TVPApiModule.Objects.Responses.eSocialPrivacy res = TVPApiModule.Objects.Responses.eSocialPrivacy.UNKNOWN;

            res = (Objects.Responses.eSocialPrivacy)Enum.Parse(typeof(TVPApiModule.Objects.Responses.eSocialPrivacy), Execute(() =>
                {
                    res = (TVPApiModule.Objects.Responses.eSocialPrivacy)Social.GetUserSocialPrivacy(m_wsUserName, m_wsPassword, int.Parse(sGuid), socialPlatform, userAction);
                    return res;
                }).ToString());

            return res;
        }

        public List<TVPApiModule.Objects.Responses.eSocialPrivacy> GetUserAllowedSocialPrivacyList(string sGuid)
        {
            List<TVPApiModule.Objects.Responses.eSocialPrivacy> res = null;

            res = Execute(() =>
                {
                    var response = Social.GetUserAllowedSocialPrivacyList(m_wsUserName, m_wsPassword, int.Parse(sGuid));
                    if (response != null)
                        res = response.Select(sp => (TVPApiModule.Objects.Responses.eSocialPrivacy)sp).ToList();

                    return res;
                }) as List<TVPApiModule.Objects.Responses.eSocialPrivacy>;

            return res;
        }

        public bool SetUserSocialPrivacy(int sGuid, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.eSocialPrivacy socialPrivacy)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    res = Social.SetUserSocialPrivacy(m_wsUserName, m_wsPassword, sGuid, socialPlatform, userAction, socialPrivacy);
                    return res;
                }));

            return res;
        }

        public List<TVPApiModule.Objects.Responses.UserSocialActionObject> GetUserActions(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            List<TVPApiModule.Objects.Responses.UserSocialActionObject> res = null;

            res = Execute(() =>
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

                    var response = Social.GetUserActions(m_wsUserName, m_wsPassword, request);

                    if (response != null)
                        res = response.Where(usa => usa != null).Select(usa => usa.ToApiObject()).ToList();

                    return res;
                }) as List<TVPApiModule.Objects.Responses.UserSocialActionObject>;

            return res;
        }

        public List<TVPApiModule.Objects.Responses.UserSocialActionObject> GetFriendsActions(string siteGuid, string[] userActions, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            List<TVPApiModule.Objects.Responses.UserSocialActionObject> res = null;

            res = Execute(() =>
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

                    var response = Social.GetFriendsActions(m_wsUserName, m_wsPassword, request);
                    if (response != null)
                        res = response.Where(usa => usa != null).Select(usa => usa.ToApiObject()).ToList();

                    return res;
                }) as List<TVPApiModule.Objects.Responses.UserSocialActionObject>;

            return res;
        }

        //public TVPApiModule.Objects.Responses.SocialActionResponseStatus DoUserAction(string siteGuid, string udid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[] extraParams, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID)
        //{
        //    TVPApiModule.Objects.Responses.SocialActionResponseStatus res = TVPApiModule.Objects.Responses.SocialActionResponseStatus.UNKNOWN;

        //    res = (TVPApiModule.Objects.Responses.SocialActionResponseStatus)Enum.Parse(typeof(TVPApiModule.Objects.Responses.SocialActionResponseStatus), Execute(() =>
        //        {
        //            // create request object
        //            TVPPro.SiteManager.TvinciPlatform.Social.BaseDoUserActionRequest request = new TVPPro.SiteManager.TvinciPlatform.Social.BaseDoUserActionRequest()
        //            {
        //                m_eAction = userAction,
        //                m_eSocialPlatform = socialPlatform,
        //                m_oKeyValue = extraParams,
        //                m_sSiteGuid = siteGuid,
        //                m_eAssetType = assetType,
        //                m_nAssetID = assetID,
        //                m_sDeviceUDID = udid,

        //            };
        //            var response = Social.DoUserAction(m_wsUserName, m_wsPassword, request);
        //            if (response != null)
        //                res = (TVPApiModule.Objects.Responses.SocialActionResponseStatus)response.m_eActionResponseStatusIntern;
        //            //res = Social.DoUserAction(m_wsUserName, m_wsPassword, request);

        //            return res;
        //        }).ToString());

        //    return res;            
        //}

        public TVPApiModule.Objects.Responses.DoSocialActionResponse DoUserAction(string siteGuid, string udid, eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[] extraParams, SocialPlatform socialPlatform, eAssetType assetType, int assetID)
        {
            TVPApiModule.Objects.Responses.DoSocialActionResponse response = null;

            response = Execute(() =>
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

                    var res = Social.DoUserAction(m_wsUserName, m_wsPassword, request);
                    if (res != null)
                    {
                        response = res.ToApiObject();
                    }

                    return response;

                }) as TVPApiModule.Objects.Responses.DoSocialActionResponse;

            return response;
        }

        public TVPApiModule.Objects.Responses.eSocialActionPrivacy GetUserExternalActionShare(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            TVPApiModule.Objects.Responses.eSocialActionPrivacy res = TVPApiModule.Objects.Responses.eSocialActionPrivacy.UNKNOWN;

            res = (TVPApiModule.Objects.Responses.eSocialActionPrivacy)Enum.Parse(typeof(TVPApiModule.Objects.Responses.eSocialActionPrivacy), Execute(() =>
                {
                    res = (TVPApiModule.Objects.Responses.eSocialActionPrivacy)Social.GetUserExternalActionShare(m_wsUserName, m_wsPassword, int.Parse(siteGuid), socialPlatform, userAction);
                    return res;
                }).ToString());

            return res;
        }

        public TVPApiModule.Objects.Responses.eSocialActionPrivacy GetUserInternalActionPrivacy(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            TVPApiModule.Objects.Responses.eSocialActionPrivacy res = TVPApiModule.Objects.Responses.eSocialActionPrivacy.UNKNOWN;

            res = (TVPApiModule.Objects.Responses.eSocialActionPrivacy)Enum.Parse(typeof(TVPApiModule.Objects.Responses.eSocialActionPrivacy), Execute(() =>
                {
                    res = (TVPApiModule.Objects.Responses.eSocialActionPrivacy)Social.GetUserInternalActionPrivacy(m_wsUserName, m_wsPassword, int.Parse(siteGuid), socialPlatform, userAction);
                    return res;
                }).ToString());

            return res;
        }

        public bool SetUserExternalActionShare(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eSocialActionPrivacy actionPrivacy)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    int iSiteGuid = 0;
                    if (int.TryParse(siteGuid, out iSiteGuid))
                        res = Social.SetUserExternalActionShare(m_wsUserName, m_wsPassword, iSiteGuid, socialPlatform, userAction, actionPrivacy);
                    else
                        throw new Exception("siteGuid not in the right format");

                    return res;
                }));

            return res;
        }

        public bool SetUserInternalActionPrivacy(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eSocialActionPrivacy actionPrivacy)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    int iSiteGuid = 0;
                    if (int.TryParse(siteGuid, out iSiteGuid))
                        res = Social.SetUserInternalActionPrivacy(m_wsUserName, m_wsPassword, iSiteGuid, socialPlatform, userAction, actionPrivacy);
                    else
                        throw new Exception("siteGuid not in the right format");

                    return res;
                }));

            return res;
        }
    }
}
