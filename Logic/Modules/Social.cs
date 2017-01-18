using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Social;
using Core.Social.Requests;
using Core.Social.Responses;
using Core.Users;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Core.Social
{
    public class Module
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        
        public static string[] GetUsersLikedMedia(int nGroupID, Int32 nSiteGUID, Int32 nMediaID, Int32 nPlatform, bool bOnlyFriends, Int32 nStartIndex, Int32 nNumberOfItems)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            //Social.BaseSocial t = null;
            string[] ret = null;
            if (bOnlyFriends)
            {
                GetFriendsActionsRequest req = new GetFriendsActionsRequest()
                {
                    m_eAssetType = eAssetType.MEDIA,
                    m_eSocialPlatform = (SocialPlatform)nPlatform,
                    m_sSiteGuid = nSiteGUID.ToString(),
                    m_eUserActions = eUserAction.LIKE,
                    m_nStartIndex = nStartIndex,
                    m_nNumOfRecords = nNumberOfItems
                };
                req.m_lAssetIDs.Add(nMediaID);

                SocialActionQueryResponse response = req.GetResponse(nGroupID) as SocialActionQueryResponse;
                if (response != null && response.m_lUserActionObj != null)
                {
                    ret = new string[response.m_lUserActionObj.Count];
                    int i = 0;
                    foreach (var action in response.m_lUserActionObj)
                    {
                        ret[i] = action.ActivitySubject.ActorSiteGuid;
                        i++;
                    }
                }
            }
            else
            {
                BaseSocialBL oBL = BaseSocialBL.GetBaseSocialImpl(nGroupID) as BaseSocialBL;
                ret = oBL.GetUsersLikedMedia(nSiteGUID, nMediaID, nPlatform, nStartIndex, nNumberOfItems);
            }

            return ret;
        }

        
        public static List<FriendWatchedObject> GetAllFriendsWatched(int nGroupID, Int32 nSiteGUID, int nMaxResults = 0)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            FacebookWrapper fbWrapper = new FacebookWrapper(nGroupID);
            List<FriendWatchedObject> lRes = fbWrapper.GetAllFriendsWatched(nSiteGUID);

            if (lRes != null && nMaxResults > 0 && lRes.Count > nMaxResults)
            {
                lRes = lRes.GetRange(0, nMaxResults);
            }

            return lRes;
        }

        
        public static List<FriendWatchedObject> GetFriendsWatchedByMedia(int nGroupID, Int32 nSiteGUID, int nMediaID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            FacebookWrapper fbWrapper = new FacebookWrapper(nGroupID);
            return fbWrapper.GetAllFriendsWatched(nSiteGUID, nMediaID);
        }


        
        public static eSocialPrivacy GetUserSocialPrivacy(int nGroupID, Int32 nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            BaseSocialBL oSocialBL = BaseSocialBL.GetBaseSocialImpl(nGroupID) as BaseSocialBL;
            return oSocialBL.GetUserSocialPrivacy(nSiteGUID, eSocialPlatform, eAction);
        }

        
        public static eSocialPrivacy[] GetUserAllowedSocialPrivacyList(int nGroupID, Int32 nSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            string sFbListName = TVinciShared.WS_Utils.GetTcmConfigValue("FB_LIST_NAME");
            FacebookWrapper oFBWrapper = new FacebookWrapper(nGroupID);
            return oFBWrapper.GetFBAvailablePrivacyGroups(nSiteGUID.ToString(), nGroupID, sFbListName);
        }

        
        public static bool SetUserSocialPrivacy(int nGroupID, Int32 nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction, eSocialPrivacy ePrivacy)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            BaseSocialBL oSocialBL = BaseSocialBL.GetBaseSocialImpl(nGroupID) as BaseSocialBL;
            return oSocialBL.SetUserSocialPrivacy(nSiteGUID, eSocialPlatform, eAction, ePrivacy);
        }

        
        public static string[] GetUserFriends(int nGroupID, Int32 nSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            FacebookWrapper oFBWrapper = new FacebookWrapper(nGroupID);
            return oFBWrapper.GetUserFriends(nSiteGUID);
        }

        
        [System.Xml.Serialization.XmlInclude(typeof(Social.Requests.SocialBaseRequestWrapper))]
        [System.Xml.Serialization.XmlInclude(typeof(Social.Requests.UserSocialActionQueryRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(ApiObjects.SocialActivityDoc))]
        [System.Xml.Serialization.XmlInclude(typeof(ApiObjects.eUserAction))]
        public static SocialActivityResponse GetUserActions(int nGroupID, UserSocialActionQueryRequest oUserActionRequest)
        {
            SocialActivityResponse response = new SocialActivityResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };
            if (oUserActionRequest == null)
            {
                return null;
            }

            SocialActionQueryResponse socialResponse = null;
            socialResponse = oUserActionRequest.GetResponse(nGroupID) as SocialActionQueryResponse;
            if (socialResponse != null)
            {
                response.SocialActivity = socialResponse.m_lUserActionObj;
                response.TotalCount = socialResponse.TotalCount;
                if (socialResponse.m_nStatus != 200)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }

            return response;
        }


        
        [System.Xml.Serialization.XmlInclude(typeof(Social.Requests.SocialBaseRequestWrapper))]
        [System.Xml.Serialization.XmlInclude(typeof(Social.Requests.GetFriendsActionsRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(ApiObjects.SocialActivityDoc))]
        [System.Xml.Serialization.XmlInclude(typeof(ApiObjects.eUserAction))]
        public static SocialActivityResponse GetFriendsActions(int nGroupID, GetFriendsActionsRequest oFriendActionRequest)
        {
            SocialActivityResponse response = new SocialActivityResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            if (oFriendActionRequest == null)
            {
                return response;
            }

            SocialActionQueryResponse socialResponse = null;
            socialResponse = oFriendActionRequest.GetResponse(nGroupID) as SocialActionQueryResponse;
            if (socialResponse != null)
            {
                response.SocialActivity = socialResponse.m_lUserActionObj;
                response.Status = socialResponse.m_nStatus == SocialBaseRequestWrapper.STATUS_OK ?
                    new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()) :
                    response.Status;
                response.TotalCount = socialResponse.TotalCount;
            }
            return response;
        }


        
        [System.Xml.Serialization.XmlInclude(typeof(Social.Requests.SocialBaseRequestWrapper))]
        [System.Xml.Serialization.XmlInclude(typeof(Social.Requests.BaseDoUserActionRequest))]
        public static DoSocialActionResponse DoUserAction(int nGroupID, BaseDoUserActionRequest oActionRequest)
        {
            DoSocialActionResponse response = new DoSocialActionResponse(SocialActionResponseStatus.ERROR, SocialActionResponseStatus.ERROR);
            if (oActionRequest == null)
            {
                return response;
            }


            response = oActionRequest.GetResponse(nGroupID) as DoSocialActionResponse;
            if (response != null)
            {
                return response;
            }
            else
            {
                response = new DoSocialActionResponse(SocialActionResponseStatus.ERROR, SocialActionResponseStatus.ERROR);
                return response;
            }
        }

        
        [System.Xml.Serialization.XmlInclude(typeof(ApiObjects.FacebookConfig))]
        public static FacebookConfigResponse FBConfig(int nGroupID)
        {
            FacebookConfigResponse response = new FacebookConfigResponse();

            string str = TVinciShared.WS_Utils.GetTcmConfigValue("CONNECTION_STRING");
            FacebookWrapper oFBWRapper = new FacebookWrapper(nGroupID);
            response.FacebookConfig = oFBWRapper.FBConfig;
            if (response.FacebookConfig != null)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            return response;
        }

        
        public static FacebookResponse FBUserData(int nGroupID, string token)
        {
            FacebookWrapper oFBWRapper = new FacebookWrapper(nGroupID);
            return oFBWRapper.FBUserData(token);
        }

        
        public static FacebookResponse FBUserDataByUserId(int nGroupID, string userId)
        {
            FacebookWrapper oFBWRapper = new FacebookWrapper(nGroupID);
            return oFBWRapper.FBUserDataByUserId(userId);
        }

        
        public static FacebookResponse FBUserRegister(int nGroupID, string token, List<ApiObjects.KeyValuePair> extra, string sUserIP)
        {
            FacebookWrapper oFBWRapper = new FacebookWrapper(nGroupID);
            return oFBWRapper.FBUserRegister(token, extra, sUserIP);
        }

        
        [Obsolete]
        public static FacebookResponse FBUserMerge(int nGroupID, string token, string fbid, string sUserName, string sPass)
        {
            FacebookWrapper oFBWRapper = new FacebookWrapper(nGroupID);
            return oFBWRapper.FBUserMerge(token, fbid, sUserName, sPass);
        }

        
        public static FacebookResponse FBUserMergeByUserId(int nGroupID, string userId, string token)
        {
            FacebookWrapper oFBWRapper = new FacebookWrapper(nGroupID);
            return oFBWRapper.FBUserMergeByUserId(userId, token);
        }

        
        [System.Xml.Serialization.XmlInclude(typeof(Social.Requests.SocialBaseRequestWrapper))]
        [System.Xml.Serialization.XmlInclude(typeof(Social.Requests.FacebookObjectRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(Social.Responses.SocialObjectReponse))]
        [System.Xml.Serialization.XmlInclude(typeof(Social.Responses.BaseSocialResponse))]
        public static SocialObjectReponse FBObjectRequest(int nGroupID, FacebookObjectRequest objRequest)
        {
            SocialObjectReponse oRes = new SocialObjectReponse(404) { sID = string.Empty };
            if (objRequest == null)
            {
                return oRes;
            }

            return objRequest.GetResponse(nGroupID) as SocialObjectReponse;
        }

        
        public static bool SetUserExternalActionShare(int nGroupID, Int32 nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction, eSocialActionPrivacy eActionPrivacy)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            bool bRes = false;
            BaseSocialBL oBL = (BaseSocialBL)BaseSocialBL.GetBaseSocialImpl(nGroupID);
            bRes = oBL.SetUserExternalActionShare(nSiteGUID.ToString(), eSocialPlatform, eAction, eActionPrivacy);
            return bRes;
        }

        
        public static bool SetUserInternalActionPrivacy(int nGroupID, Int32 nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction, eSocialActionPrivacy eActionPrivacy)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            bool bRes = false;
            BaseSocialBL oBL = (BaseSocialBL)BaseSocialBL.GetBaseSocialImpl(nGroupID);
            bRes = oBL.SetUserInternalActionShare(nSiteGUID.ToString(), eSocialPlatform, eAction, eActionPrivacy);

            return bRes;
        }

        
        public static eSocialActionPrivacy GetUserExternalActionShare(int nGroupID, Int32 nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            eSocialActionPrivacy ePrivacy = eSocialActionPrivacy.UNKNOWN;

            BaseSocialBL oBL = (BaseSocialBL)BaseSocialBL.GetBaseSocialImpl(nGroupID);
            ePrivacy = oBL.GetUserExternalActionShare(nSiteGUID.ToString(), eSocialPlatform, eAction);

            return ePrivacy;
        }

        
        public static eSocialActionPrivacy GetUserInternalActionPrivacy(int nGroupID, Int32 nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            eSocialActionPrivacy ePrivacy = eSocialActionPrivacy.UNKNOWN;

            BaseSocialBL oBL = (BaseSocialBL)BaseSocialBL.GetBaseSocialImpl(nGroupID);
            ePrivacy = oBL.GetUserInternalActionShare(nSiteGUID.ToString(), eSocialPlatform, eAction);

            return ePrivacy;
        }

        
        public static int GetAssetLikeCounter(int nGroupID, int nAssetID, eAssetType assetType)
        {
            int nRes = 0;

            BaseSocialBL oBL = (BaseSocialBL)BaseSocialBL.GetBaseSocialImpl(nGroupID);
            nRes = oBL.GetAssetLikeCounter(nAssetID, assetType);

            return nRes;
        }

        
        public static bool Share(int nGroupID, string sSiteGuid, string sDeviceUdid, string sFBActionID, int nMediaID, int nAssetID, eAssetType eAssetType)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            bool bResult = false;

            if (!string.IsNullOrEmpty(sFBActionID))
            {
                UserResponseObject uObj = Social.Utils.GetUserDataByID(sSiteGuid, nGroupID);

                if (uObj != null && uObj.m_RespStatus == ResponseStatus.OK && uObj.m_user != null && uObj.m_user.m_oBasicData != null)
                {
                    BaseSocialBL oBL = (BaseSocialBL)BaseSocialBL.GetBaseSocialImpl(nGroupID);
                    eSocialActionPrivacy eInternalPrivacy = oBL.GetUserInternalActionShare(sSiteGuid, SocialPlatform.FACEBOOK, eUserAction.SHARE);

                    #region create social activity doc
                    SocialActivityDoc doc = new SocialActivityDoc()
                    {
                        SocialPlatform = (int)SocialPlatform.FACEBOOK,
                        DocOwnerSiteGuid = uObj.m_user.m_sSiteGUID,
                        DocType = "user_action",
                        IsActive = true,
                        ActivityObject = new SocialActivityObject()
                        {
                            AssetType = eAssetType,
                            AssetID = nMediaID
                        },
                        ActivitySubject = new SocialActivitySubject()
                        {
                            ActorSiteGuid = uObj.m_user.m_sSiteGUID,
                            ActorTvinciUsername = string.Format("{0} {1}", uObj.m_user.m_oBasicData.m_sFirstName, uObj.m_user.m_oBasicData.m_sLastName),
                            GroupID = nGroupID,
                            ActorPicUrl = uObj.m_user.m_oBasicData.m_sFacebookImage,
                            DeviceUdid = sDeviceUdid
                        },
                        ActivityVerb = new SocialActivityVerb()
                        {
                            SocialActionID = sFBActionID,
                            ActionName = Enum.GetName(typeof(eUserAction), eUserAction.SHARE),
                            ActionType = (int)eUserAction.SHARE
                        }

                    };
                    #endregion

                    string dbRecordID;
                    bResult = oBL.InsertUserSocialAction(doc, out dbRecordID);
                }
            }

            return bResult;
        }

        
        [System.Xml.Serialization.XmlInclude(typeof(SocialActivityDoc))]
        public static List<SocialActivityDoc> GetUserActivityFeed(int nGroupID, string sSiteGuid, int nPageSize, int nPageIndex, string sPicDimension)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            List<SocialActivityDoc> response = null;
            BaseSocialBL oBL = BaseSocialBL.GetBaseSocialImpl(nGroupID);
            bool bResult = oBL.GetUserActivityFeed(sSiteGuid, nPageSize, nPageIndex, sPicDimension, out response);

            return response;
        }

        
        [Obsolete]
        public static FacebookResponse FBUserUnmerge(int nGroupID, string sToken, string sUsername, string sPassword)
        {
            FacebookWrapper oFBWRapper = new FacebookWrapper(nGroupID);
            return oFBWRapper.FBUserUnmerge(sToken, sUsername, sPassword);
        }

        
        public static FacebookResponse FBUserUnmergeByUserId(int nGroupID, string sUserId)
        {
            FacebookWrapper oFBWRapper = new FacebookWrapper(nGroupID);
            return oFBWRapper.FBUserUnmergeByUserId(sUserId);
        }

        
        public static FacebookTokenResponse FBTokenValidation(int nGroupID, string sToken)
        {
            FacebookWrapper oFBWRapper = new FacebookWrapper(nGroupID);
            return oFBWRapper.FBTokenValidation(sToken);
        }

        
        public static FBSignin FBUserSignin(int nGroupID, string token, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            FacebookWrapper oFBWRapper = new FacebookWrapper(nGroupID);
            return oFBWRapper.FBUserSignin(token, sIP, deviceID, bPreventDoubleLogins);
        }

        
        public static bool UpdateFriendsActivityFeed(int nGroupID, int siteGuid, string dbActionId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid;

            bool result = false;
            FacebookWrapper fbWrapper = new FacebookWrapper(nGroupID);
            BaseSocialBL socialBl = BaseSocialBL.GetBaseSocialImpl(nGroupID) as BaseSocialBL;
            List<string> friendsGuid = null;
            if (fbWrapper.GetUserFriendsGuid(siteGuid, out friendsGuid))
            {
                if (friendsGuid != null && friendsGuid.Count > 0)
                    result = socialBl.UpdateUserActivityFeed(friendsGuid, dbActionId);
            }
            else
            {
                log.Debug("Info - " + string.Format("caught error when getting user friends guid. siteguid={0}", siteGuid));
            }

            return result;
        }

        
        public static void DeleteFriendsFeed(int nGroupID, int siteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid;

            BaseSocialBL socialBl = BaseSocialBL.GetBaseSocialImpl(nGroupID) as BaseSocialBL;
            List<string> lDocIDs;
            bool bHasNoErrors = true;
            int nNumOfDocs = 50;

            do
            {
                bHasNoErrors = socialBl.GetFeedIDsByActorID(siteGuid.ToString(), nNumOfDocs, out lDocIDs);
                if (bHasNoErrors && lDocIDs != null && lDocIDs.Count > 0)
                {
                    foreach (string docID in lDocIDs)
                        bHasNoErrors &= socialBl.DeleteActivityFromUserFeed(docID);
                }
            }
            while (bHasNoErrors == true && lDocIDs != null && lDocIDs.Count > 0);

            if (!bHasNoErrors)
                log.Debug("Info - Error occurred during deletion of friends feed");
        }

        
        public static void DeleteUserFeed(int nGroupID, int siteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid;

            BaseSocialBL socialBl = BaseSocialBL.GetBaseSocialImpl(nGroupID) as BaseSocialBL;
            List<string> lDocIDs;
            bool bHasNoErrors = true;
            int nNumOfDocs = 50;

            do
            {
                bHasNoErrors = socialBl.GetUserActivityFeedIds(siteGuid.ToString(), nNumOfDocs, 0, out lDocIDs);
                if (bHasNoErrors && lDocIDs != null && lDocIDs.Count > 0)
                {
                    foreach (string docID in lDocIDs)
                        bHasNoErrors &= socialBl.DeleteActivityFromUserFeed(docID);
                }
            }
            while (bHasNoErrors == true && lDocIDs != null && lDocIDs.Count > 0);

            if (!bHasNoErrors)
                log.Debug("Info - Error occurred during deletion user feed");
        }

        
        public static void MergeFriendsActivityFeed(int nGroupID, int siteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid;

            BaseSocialBL socialBl = BaseSocialBL.GetBaseSocialImpl(nGroupID) as BaseSocialBL;
            FacebookWrapper fbWrapper = new FacebookWrapper(nGroupID);
            List<string> friendsGuid = null;

            if (fbWrapper.GetUserFriendsGuid(siteGuid, out friendsGuid))
            {
                List<SocialActivityDoc> friendsActions = new List<SocialActivityDoc>();
                bool bSuccess = true;
                foreach (string friendID in friendsGuid)
                {
                    List<SocialActivityDoc> lActions;
                    if (socialBl.GetUserSocialAction(friendID, 20, 0, out lActions))
                        friendsActions.AddRange(lActions);
                    else
                    {
                        bSuccess = false;
                        break;
                    }
                }

                if (bSuccess)
                    socialBl.InsertFriendsActivitiesToUserActivityFeed(siteGuid.ToString(), friendsActions);
                else
                    log.Debug("Info - " + string.Format("Error while inserting friends activities into user feed. id={0}", siteGuid));
            }
        }


        
        public static SocialPrivacySettingsResponse SetUserSocialPrivacySettings(int nGroupID, string siteGUID, SocialPrivacySettings settings)
        {
            SocialPrivacySettingsResponse response = new SocialPrivacySettingsResponse();
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGUID;
            BaseSocialBL oBL = (BaseSocialBL)BaseSocialBL.GetBaseSocialImpl(nGroupID);
            response = oBL.SetUserSocialPrivacySettings(siteGUID, nGroupID, settings);

            return response;
        }

        
        public static SocialPrivacySettingsResponse GetUserSocialPrivacySettings(int nGroupID, string siteGUID)
        {
            SocialPrivacySettingsResponse response = new SocialPrivacySettingsResponse();
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGUID;
            BaseSocialBL oBL = (BaseSocialBL)BaseSocialBL.GetBaseSocialImpl(nGroupID);
            response = oBL.GetUserSocialPrivacySettings(siteGUID, nGroupID);

            return response;
        }

        
        public static UserSocialActionResponse AddUserSocialAction(int nGroupID, UserSocialActionRequest actionRequest)
        {
            UserSocialActionResponse response = new UserSocialActionResponse();
            if (actionRequest == null)
            {
                return response;
            }

            UserSocialAction usa = new UserSocialAction(nGroupID);
            response = usa.AddUserSocialAction(nGroupID, actionRequest);
            return response;
        }

        
        public static Social.SocialFeed.SocialFeedResponse GetSocialFeed(int nGroupID, string userId, int assetId, eAssetType assetType, eSocialPlatform socialPlatform,
            int pageSize, int pageIndex, long epochStartTime, Social.SocialFeed.SocialFeedOrderBy orderBy)
        {
            Social.SocialFeed.SocialFeedResponse response = new Social.SocialFeed.SocialFeedResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()),
            };

            response = SocialFeedUtils.GetSocialFeed(nGroupID, userId, assetId, assetType, socialPlatform, pageSize, pageIndex, epochStartTime, orderBy);

            return response;
        }

        
        public static UserSocialActionResponse DeleteUserSocialAction(int nGroupID, string siteGuid, string id)
        {
            UserSocialActionResponse response = new UserSocialActionResponse();
            if (string.IsNullOrEmpty(id))
            {
                return response;
            }

            UserSocialAction usa = new UserSocialAction(nGroupID);
            response = usa.DeleteUserSocialAction(nGroupID, siteGuid, id);
            return response;

        }
    }
}
