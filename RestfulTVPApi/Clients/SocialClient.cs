using RestfulTVPApi.Clients.ClientsCache;
using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.Social;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestfulTVPApi.Objects.Extentions;

namespace RestfulTVPApi.Clients
{
    public class SocialClient : BaseClient
    {
        #region Fields

        private readonly ILog logger = LogManager.GetLogger(typeof(SocialClient));
        private static object instanceLock = new object();

        
        #endregion

        #region C'tor

        public SocialClient(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
          
        }

        public SocialClient()
        {
            // TODO: Complete member initialization
        }

        #endregion

        #region Properties

        protected RestfulTVPApi.Social.module Social
        {
            get
            {
                return (Module as RestfulTVPApi.Social.module);
            }
        }

        #endregion

        public RestfulTVPApi.Objects.Responses.DoSocialActionResponse DoSocialAction(int mediaID,
                                                                                                  string siteGuid,
                                                                                                  string udid,
                                                                                                  RestfulTVPApi.Social.eUserAction userAction,
                                                                                                  RestfulTVPApi.Social.SocialPlatform socialPlatform,
                                                                                                  string actionParam)
        {
            RestfulTVPApi.Objects.Responses.DoSocialActionResponse eRes = null;

            eRes = Execute(() =>
                {
                    RestfulTVPApi.Social.KeyValuePair[] extraParams;
                    if (string.IsNullOrEmpty(actionParam))
                        extraParams = new RestfulTVPApi.Social.KeyValuePair[0];
                    else
                    {
                        extraParams = new RestfulTVPApi.Social.KeyValuePair[1];
                        extraParams[0] = new RestfulTVPApi.Social.KeyValuePair() { key = "link", value = actionParam };
                    }

                    BaseDoUserActionRequest actionRequest = new BaseDoUserActionRequest()
                    {
                        m_eAction = userAction,
                        m_eAssetType = RestfulTVPApi.Social.eAssetType.MEDIA,
                        m_eSocialPlatform = socialPlatform,
                        m_nAssetID = mediaID,
                        m_oKeyValue = extraParams,
                        m_sDeviceUDID = udid,
                        m_sSiteGuid = siteGuid
                    };
                    RestfulTVPApi.Social.DoSocialActionResponse response = Social.DoUserAction(WSUserName, WSPassword, actionRequest);
                    if (response != null)
                        eRes = response.ToApiObject();

                    return eRes;
                }) as RestfulTVPApi.Objects.Responses.DoSocialActionResponse;

            return eRes;
        }

        public List<RestfulTVPApi.Objects.Responses.SocialActivityDoc> GetUserSocialActions(string siteGuid,
                                                                                                      RestfulTVPApi.Social.eUserAction userAction,
                                                                                                      RestfulTVPApi.Social.SocialPlatform socialPlatform,
                                                                                                      bool onlyFriends,
                                                                                                      int startIndex,
                                                                                                      int numOfItems)
        {
            List<RestfulTVPApi.Objects.Responses.SocialActivityDoc> res = null;

            res = Execute(() =>
                {

                    RestfulTVPApi.Social.SocialActivityDoc[] response;

                    if (onlyFriends)
                    {
                        RestfulTVPApi.Social.GetFriendsActionsRequest friendActionRequest = new RestfulTVPApi.Social.GetFriendsActionsRequest()
                        {
                            m_eSocialPlatform = socialPlatform,
                            m_eUserActions = userAction,
                            m_nNumOfRecords = numOfItems,
                            m_nStartIndex = startIndex,
                            m_sSiteGuid = siteGuid
                        };
                        response = Social.GetFriendsActions(WSUserName, WSPassword, friendActionRequest);
                    }
                    else
                    {
                        RestfulTVPApi.Social.UserSocialActionQueryRequest userActionRequest = new RestfulTVPApi.Social.UserSocialActionQueryRequest()
                        {
                            m_eSocialPlatform = socialPlatform,
                            m_eUserActions = userAction,
                            m_nNumOfRecords = numOfItems,
                            m_nStartIndex = startIndex,
                            m_sSiteGuid = siteGuid
                        };
                        response = Social.GetUserActions(WSUserName, WSPassword, userActionRequest);
                    }

                    if (response != null)
                        res = new List<RestfulTVPApi.Objects.Responses.SocialActivityDoc>();//res = response.Where(usa => usa != null).Select(u => u.ToApiObject()).ToList();


                    return res;
                }) as List<RestfulTVPApi.Objects.Responses.SocialActivityDoc>;

            return res;

        }

        public List<RestfulTVPApi.Objects.Responses.FriendWatchedObject> GetAllFriendsWatched(string sGuid, int maxResult)
        {
            List<RestfulTVPApi.Objects.Responses.FriendWatchedObject> res = null;

            res = Execute(() =>
                {
                    var response = Social.GetAllFriendsWatched(WSUserName, WSPassword, int.Parse(sGuid), maxResult);

                    if (response != null)
                        res = response.Where(fw => fw != null).Select(fw => fw.ToApiObject()).ToList();

                    return res;
                }) as List<RestfulTVPApi.Objects.Responses.FriendWatchedObject>;

            return res;
        }

        public List<RestfulTVPApi.Objects.Responses.FriendWatchedObject> GetFriendsWatchedByMedia(string sGuid, int mediaId)
        {
            List<RestfulTVPApi.Objects.Responses.FriendWatchedObject> res = null;

            res = Execute(() =>
                {
                    var response = Social.GetFriendsWatchedByMedia(WSUserName, WSPassword, int.Parse(sGuid), mediaId);
                    if (response != null)
                        res = response.Where(fw => fw != null).Select(fw => fw.ToApiObject()).ToList();

                    return res;
                }) as List<RestfulTVPApi.Objects.Responses.FriendWatchedObject>;

            return res;
        }

        public List<string> GetUsersLikedMedia(string sSiteGuid, int iMediaID, int iPlatform, bool bOnlyFriends, int iStartIndex, int iPageSize)
        {
            List<string> retVal = null;

            retVal = Execute(() =>
                {
                    var res = Social.GetUsersLikedMedia(WSUserName, WSPassword, int.Parse(sSiteGuid), iMediaID, iPlatform,
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
                    var res = Social.GetUserFriends(WSUserName, WSPassword, int.Parse(sGuid));

                    if (res != null)
                        retVal = res.ToList();

                    return retVal;
                }) as List<string>;

            return retVal;
        }

        public RestfulTVPApi.Objects.Responses.FacebookConfig GetFBConfig(string sStg)
        {
            RestfulTVPApi.Objects.Responses.FacebookConfig res = null;

            res = Execute(() =>
                {
                    var response = Social.FBConfig(WSUserName, WSPassword, sStg);
                    if (response != null)
                        res = response.ToApiObject();

                    return res;
                }) as RestfulTVPApi.Objects.Responses.FacebookConfig;

            return res;
        }

        public RestfulTVPApi.Objects.Responses.FacebookResponseObject GetFBUserData(string stoken, string sSTG)
        {
            RestfulTVPApi.Objects.Responses.FacebookResponseObject res = null;

            res = Execute(() =>
                {
                    var response = Social.FBUserData(WSUserName, WSPassword, stoken, sSTG);
                    if (response != null)
                        res = response.ToApiObject();

                    return res;
                }) as RestfulTVPApi.Objects.Responses.FacebookResponseObject;

            return res;
        }

        public RestfulTVPApi.Objects.Responses.FacebookResponseObject FBUserRegister(string stoken, string sSTG, List<RestfulTVPApi.Social.KeyValuePair> oExtra, string sIP)
        {
            RestfulTVPApi.Objects.Responses.FacebookResponseObject res = null;

            res = Execute(() =>
                {
                    var response = Social.FBUserRegister(WSUserName, WSPassword, stoken, sSTG, oExtra.ToArray(), sIP);
                    if (response != null)
                        res = response.ToApiObject();

                    return res;
                }) as RestfulTVPApi.Objects.Responses.FacebookResponseObject;

            return res;
        }

        public RestfulTVPApi.Objects.Responses.FacebookResponseObject FBUserMerge(string stoken, string sFBID, string sUsername, string sPassword)
        {
            RestfulTVPApi.Objects.Responses.FacebookResponseObject res = null;

            res = Execute(() =>
                {
                    var response = Social.FBUserMerage(WSUserName, WSPassword, stoken, sFBID, sUsername, sPassword);
                    if (response != null)
                        res = response.ToApiObject();

                    return res;
                }) as RestfulTVPApi.Objects.Responses.FacebookResponseObject;

            return res;
        }

        public RestfulTVPApi.Objects.Responses.FacebookResponseObject FBUserUnMerge(string stoken, string sUsername, string sPassword)
        {
            RestfulTVPApi.Objects.Responses.FacebookResponseObject res = null;

            res = Execute(() =>
            {
                var response = Social.FBUserUnmerge(WSUserName, WSPassword, stoken, sUsername, sPassword);
                if (response != null)
                    res = response.ToApiObject();

                return res;
            }) as RestfulTVPApi.Objects.Responses.FacebookResponseObject;

            return res;
        }

        public RestfulTVPApi.Objects.Responses.Enums.eSocialPrivacy GetUserSocialPrivacy(string sGuid, RestfulTVPApi.Social.SocialPlatform socialPlatform, RestfulTVPApi.Social.eUserAction userAction)
        {
            RestfulTVPApi.Objects.Responses.Enums.eSocialPrivacy res = RestfulTVPApi.Objects.Responses.Enums.eSocialPrivacy.UNKNOWN;

            res = (Objects.Responses.Enums.eSocialPrivacy)Enum.Parse(typeof(RestfulTVPApi.Objects.Responses.Enums.eSocialPrivacy), Execute(() =>
                {
                    res = (RestfulTVPApi.Objects.Responses.Enums.eSocialPrivacy)Social.GetUserSocialPrivacy(WSUserName, WSPassword, int.Parse(sGuid), socialPlatform, userAction);
                    return res;
                }).ToString());

            return res;
        }

        public List<RestfulTVPApi.Objects.Responses.Enums.eSocialPrivacy> GetUserAllowedSocialPrivacyList(string sGuid)
        {
            List<RestfulTVPApi.Objects.Responses.Enums.eSocialPrivacy> res = null;

            res = Execute(() =>
                {
                    var response = Social.GetUserAllowedSocialPrivacyList(WSUserName, WSPassword, int.Parse(sGuid));
                    if (response != null)
                        res = response.Select(sp => (RestfulTVPApi.Objects.Responses.Enums.eSocialPrivacy)sp).ToList();

                    return res;
                }) as List<RestfulTVPApi.Objects.Responses.Enums.eSocialPrivacy>;

            return res;
        }

        public bool SetUserSocialPrivacy(int sGuid, RestfulTVPApi.Social.SocialPlatform socialPlatform, RestfulTVPApi.Social.eUserAction userAction, RestfulTVPApi.Social.eSocialPrivacy socialPrivacy)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    res = Social.SetUserSocialPrivacy(WSUserName, WSPassword, sGuid, socialPlatform, userAction, socialPrivacy);
                    return res;
                }));

            return res;
        }

        public List<RestfulTVPApi.Objects.Responses.SocialActivityDoc> GetUserActions(string siteGuid, RestfulTVPApi.Social.eUserAction userAction, RestfulTVPApi.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, RestfulTVPApi.Social.SocialPlatform socialPlatform)
        {
            List<RestfulTVPApi.Objects.Responses.SocialActivityDoc> res = null;

            res = Execute(() =>
                {
                    // create request object
                    RestfulTVPApi.Social.UserSocialActionQueryRequest request = new RestfulTVPApi.Social.UserSocialActionQueryRequest()
                    {
                        m_eSocialPlatform = socialPlatform,
                        m_eUserActions = userAction,
                        //m_nAssetID = assetID,
                        m_eAssetType = assetType,
                        m_nNumOfRecords = numOfRecords,
                        m_nStartIndex = startIndex,
                        m_sSiteGuid = siteGuid,
                        
                    };

                    var response = Social.GetUserActions(WSUserName, WSPassword, request);

                    if (response != null)
                        res = new List<RestfulTVPApi.Objects.Responses.SocialActivityDoc>();//res = response.Where(usa => usa != null).Select(usa => usa.ToApiObject()).ToList();

                    return res;
                }) as List<RestfulTVPApi.Objects.Responses.SocialActivityDoc>;

            return res;
        }

        public List<RestfulTVPApi.Objects.Responses.UserSocialActionObject> GetFriendsActions(string siteGuid, string[] userActions, RestfulTVPApi.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, RestfulTVPApi.Social.SocialPlatform socialPlatform)
        {
            List<RestfulTVPApi.Objects.Responses.UserSocialActionObject> res = null;

            res = Execute(() =>
                {
                    // create eUserAction OR list
                    RestfulTVPApi.Social.eUserAction userAction = RestfulTVPApi.Social.eUserAction.UNKNOWN;
                    foreach (var action in userActions)
                        userAction |= (RestfulTVPApi.Social.eUserAction)Enum.Parse(typeof(RestfulTVPApi.Social.eUserAction), action);

                    // create request object
                    RestfulTVPApi.Social.GetFriendsActionsRequest request = new RestfulTVPApi.Social.GetFriendsActionsRequest()
                    {
                        m_eSocialPlatform = socialPlatform,
                        m_eUserActions = userAction,
                        //m_nAssetID = assetID,
                        m_eAssetType = assetType,
                        m_nNumOfRecords = numOfRecords,
                        m_nStartIndex = startIndex,
                        m_sSiteGuid = siteGuid,
                    };

                    var response = Social.GetFriendsActions(WSUserName, WSPassword, request);
                    if (response != null)
                        res = new List<RestfulTVPApi.Objects.Responses.UserSocialActionObject>(); //res = response.Where(usa => usa != null).Select(usa => usa.ToApiObject()).ToList();

                    return res;
                }) as List<RestfulTVPApi.Objects.Responses.UserSocialActionObject>;

            return res;
        }

        //public RestfulTVPApi.Objects.Responses.SocialActionResponseStatus DoUserAction(string siteGuid, string udid, RestfulTVPApi.Social.eUserAction userAction, RestfulTVPApi.Social.KeyValuePair[] extraParams, RestfulTVPApi.Social.SocialPlatform socialPlatform, RestfulTVPApi.Social.eAssetType assetType, int assetID)
        //{
        //    RestfulTVPApi.Objects.Responses.SocialActionResponseStatus res = RestfulTVPApi.Objects.Responses.SocialActionResponseStatus.UNKNOWN;

        //    res = (RestfulTVPApi.Objects.Responses.SocialActionResponseStatus)Enum.Parse(typeof(RestfulTVPApi.Objects.Responses.SocialActionResponseStatus), Execute(() =>
        //        {
        //            // create request object
        //            RestfulTVPApi.Social.BaseDoUserActionRequest request = new RestfulTVPApi.Social.BaseDoUserActionRequest()
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
        //                res = (RestfulTVPApi.Objects.Responses.SocialActionResponseStatus)response.m_eActionResponseStatusIntern;
        //            //res = Social.DoUserAction(m_wsUserName, m_wsPassword, request);

        //            return res;
        //        }).ToString());

        //    return res;            
        //}

        public RestfulTVPApi.Objects.Responses.DoSocialActionResponse DoUserAction(string siteGuid, string udid, eUserAction userAction, RestfulTVPApi.Social.KeyValuePair[] extraParams, RestfulTVPApi.Social.SocialPlatform socialPlatform, RestfulTVPApi.Social.eAssetType assetType, int assetID)
        {
            RestfulTVPApi.Objects.Responses.DoSocialActionResponse response = null;

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

                    var res = Social.DoUserAction(WSUserName, WSPassword, request);
                    if (res != null)
                    {
                        response = res.ToApiObject();
                    }

                    return response;

                }) as RestfulTVPApi.Objects.Responses.DoSocialActionResponse;

            return response;
        }

        public RestfulTVPApi.Objects.Responses.Enums.eSocialActionPrivacy GetUserExternalActionShare(string siteGuid, RestfulTVPApi.Social.eUserAction userAction, RestfulTVPApi.Social.SocialPlatform socialPlatform)
        {
            RestfulTVPApi.Objects.Responses.Enums.eSocialActionPrivacy res = RestfulTVPApi.Objects.Responses.Enums.eSocialActionPrivacy.UNKNOWN;

            res = (RestfulTVPApi.Objects.Responses.Enums.eSocialActionPrivacy)Enum.Parse(typeof(RestfulTVPApi.Objects.Responses.Enums.eSocialActionPrivacy), Execute(() =>
                {
                    res = (RestfulTVPApi.Objects.Responses.Enums.eSocialActionPrivacy)Social.GetUserExternalActionShare(WSUserName, WSPassword, int.Parse(siteGuid), socialPlatform, userAction);
                    return res;
                }).ToString());

            return res;
        }

        public RestfulTVPApi.Objects.Responses.Enums.eSocialActionPrivacy GetUserInternalActionPrivacy(string siteGuid, RestfulTVPApi.Social.eUserAction userAction, RestfulTVPApi.Social.SocialPlatform socialPlatform)
        {
            RestfulTVPApi.Objects.Responses.Enums.eSocialActionPrivacy res = RestfulTVPApi.Objects.Responses.Enums.eSocialActionPrivacy.UNKNOWN;

            res = (RestfulTVPApi.Objects.Responses.Enums.eSocialActionPrivacy)Enum.Parse(typeof(RestfulTVPApi.Objects.Responses.Enums.eSocialActionPrivacy), Execute(() =>
                {
                    res = (RestfulTVPApi.Objects.Responses.Enums.eSocialActionPrivacy)Social.GetUserInternalActionPrivacy(WSUserName, WSPassword, int.Parse(siteGuid), socialPlatform, userAction);
                    return res;
                }).ToString());

            return res;
        }

        public bool SetUserExternalActionShare(string siteGuid, RestfulTVPApi.Social.eUserAction userAction, RestfulTVPApi.Social.SocialPlatform socialPlatform, RestfulTVPApi.Social.eSocialActionPrivacy actionPrivacy)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    int iSiteGuid = 0;
                    if (int.TryParse(siteGuid, out iSiteGuid))
                        res = Social.SetUserExternalActionShare(WSUserName, WSPassword, iSiteGuid, socialPlatform, userAction, actionPrivacy);
                    else
                        throw new Exception("siteGuid not in the right format");

                    return res;
                }));

            return res;
        }

        public bool SetUserInternalActionPrivacy(string siteGuid, RestfulTVPApi.Social.eUserAction userAction, RestfulTVPApi.Social.SocialPlatform socialPlatform, RestfulTVPApi.Social.eSocialActionPrivacy actionPrivacy)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    int iSiteGuid = 0;
                    if (int.TryParse(siteGuid, out iSiteGuid))
                        res = Social.SetUserInternalActionPrivacy(WSUserName, WSPassword, iSiteGuid, socialPlatform, userAction, actionPrivacy);
                    else
                        throw new Exception("siteGuid not in the right format");

                    return res;
                }));

            return res;
        }

        public FBSignIn FBUserSignin(string token, string ip, string deviceId, bool preventDoubleLogins)
        {
            FBSignIn signIn = new FBSignIn();

            signIn = Execute(() =>
            {

                var res = Social.FBUserSignin(WSUserName, WSPassword, token, ip, deviceId, preventDoubleLogins);
                if (res != null)
                {
                    signIn = res.ToApiObject();
                }

                return signIn;

            }) as FBSignIn;

            return signIn;

        }
    }
}