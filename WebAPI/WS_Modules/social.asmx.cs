using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;
using ApiObjects.Social;
using Core.Social.Requests;
using Core.Social.Responses;
using Core.Social;

namespace WS_Social
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://social.tvinci.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class module : System.Web.Services.WebService
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [WebMethod]
        public virtual string[] GetUsersLikedMedia(string sWSUserName, string sWSPassword, Int32 nSiteGUID, Int32 nMediaID, Int32 nPlatform, bool bOnlyFriends, Int32 nStartIndex, Int32 nNumberOfItems)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            //Social.BaseSocial t = null;
            int nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            string[] ret = null;
            if (nGroupID != 0)
            {
                return Core.Social.Module.GetUsersLikedMedia(nGroupID, nSiteGUID, nMediaID, nPlatform, bOnlyFriends, nStartIndex, nNumberOfItems);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return ret;
        }

        [WebMethod]
        public virtual List<FriendWatchedObject> GetAllFriendsWatched(string sWSUserName, string sWSPassword, Int32 nSiteGUID, int nMaxResults = 0)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                return Core.Social.Module.GetAllFriendsWatched(nGroupID, nSiteGUID, nMaxResults);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual List<FriendWatchedObject> GetFriendsWatchedByMedia(string sWSUserName, string sWSPassword, Int32 nSiteGUID, int nMediaID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                return Core.Social.Module.GetFriendsWatchedByMedia(nGroupID, nSiteGUID, nMediaID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        public virtual eSocialPrivacy GetUserSocialPrivacy(string sWSUserName, string sWSPassword, Int32 nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Social.Module.GetUserSocialPrivacy(nGroupID, nSiteGUID, eSocialPlatform, eAction);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return eSocialPrivacy.UNKNOWN;
            }
        }

        [WebMethod]
        public virtual eSocialPrivacy[] GetUserAllowedSocialPrivacyList(string sWSUserName, string sWSPassword, Int32 nSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Social.Module.GetUserAllowedSocialPrivacyList(nGroupID, nSiteGUID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual bool SetUserSocialPrivacy(string sWSUserName, string sWSPassword, Int32 nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction, eSocialPrivacy ePrivacy)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Social.Module.SetUserSocialPrivacy(nGroupID, nSiteGUID, eSocialPlatform, eAction, ePrivacy);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        public virtual string[] GetUserFriends(string sWSUserName, string sWSPassword, Int32 nSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Social.Module.GetUserFriends(nGroupID, nSiteGUID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Social.Requests.SocialBaseRequestWrapper))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Social.Requests.UserSocialActionQueryRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(ApiObjects.SocialActivityDoc))]
        [System.Xml.Serialization.XmlInclude(typeof(ApiObjects.eUserAction))]
        public virtual SocialActivityResponse GetUserActions(string sWUserName, string sWSPassword, UserSocialActionQueryRequest oUserActionRequest)
        {
            SocialActivityResponse response = new SocialActivityResponse()
                {
                    Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
                };
            if (oUserActionRequest == null)
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }

            Int32 nGroupID = Utils.GetGroupID(sWUserName, sWSPassword);
            SocialActionQueryResponse socialResponse = null;
            if (nGroupID != 0)
            {
                return Core.Social.Module.GetUserActions(nGroupID, oUserActionRequest);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;                
            }

            return response;
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Social.Requests.SocialBaseRequestWrapper))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Social.Requests.GetFriendsActionsRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(ApiObjects.SocialActivityDoc))]
        [System.Xml.Serialization.XmlInclude(typeof(ApiObjects.eUserAction))]
        public virtual SocialActivityResponse GetFriendsActions(string sWUserName, string sWSPassword, GetFriendsActionsRequest oFriendActionRequest)
        {
            SocialActivityResponse response = new SocialActivityResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            if (oFriendActionRequest == null)
            {
                HttpContext.Current.Response.StatusCode = 404;
                return response;
            }

            Int32 nGroupID = Utils.GetGroupID(sWUserName, sWSPassword);
            SocialActionQueryResponse socialResponse = null;
            if (nGroupID != 0)
            {
                return Core.Social.Module.GetUserActions(nGroupID, oFriendActionRequest);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Social.Requests.SocialBaseRequestWrapper))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Social.Requests.BaseDoUserActionRequest))]
        public virtual DoSocialActionResponse DoUserAction(string sWSUserName, string sWSPassword, BaseDoUserActionRequest oActionRequest)
        {
            DoSocialActionResponse response = new DoSocialActionResponse(SocialActionResponseStatus.ERROR, SocialActionResponseStatus.ERROR);
            if (oActionRequest == null)
            {
                HttpContext.Current.Response.StatusCode = 404;
                return response;
            }

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                response = oActionRequest.GetResponse(nGroupID) as DoSocialActionResponse;
            }
            if (response != null)
            {
                return response;
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new DoSocialActionResponse(SocialActionResponseStatus.ERROR, SocialActionResponseStatus.ERROR);
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(ApiObjects.FacebookConfig))]
        public virtual FacebookConfigResponse FBConfig(string sWSUserName, string sWSPassword)
        {
            FacebookConfigResponse response = new FacebookConfigResponse();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Social.Module.FBConfig(nGroupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }

        [WebMethod]
        public virtual FacebookResponse FBUserData(string sWSUserName, string sWSPassword, string token)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Social.Module.FBUserData(nGroupID, token);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual FacebookResponse FBUserDataByUserId(string sWSUserName, string sWSPassword, string userId)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Social.Module.FBUserDataByUserId(nGroupID, userId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual FacebookResponse FBUserRegister(string sWSUserName, string sWSPassword, string token, List<ApiObjects.KeyValuePair> extra, string sUserIP)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                return Core.Social.Module.FBUserRegister(nGroupID, token, extra, sUserIP);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [Obsolete]
        public virtual FacebookResponse FBUserMerge(string sWSUserName, string sWSPassword, string token, string fbid, string sUserName, string sPass)
        {

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                return Core.Social.Module.FBUserMerge(nGroupID, token, fbid, sUserName, sPass);
            }
            else
            {

                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual FacebookResponse FBUserMergeByUserId(string sWSUserName, string sWSPassword, string userId, string token)
        {

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                return Core.Social.Module.FBUserMergeByUserId(nGroupID, userId, token);
            }
            else
            {

                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Social.Requests.SocialBaseRequestWrapper))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Social.Requests.FacebookObjectRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Social.Responses.SocialObjectReponse))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Social.Responses.BaseSocialResponse))]
        public virtual SocialObjectReponse FBObjectRequest(string sWSUserName, string sWSPassword, FacebookObjectRequest objRequest)
        {
            SocialObjectReponse oRes = new SocialObjectReponse(404) { sID = string.Empty };
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (objRequest == null || nGroupID == 0)
            {
                HttpContext.Current.Response.StatusCode = 404;
                return oRes;
            }

            return objRequest.GetResponse(nGroupID) as SocialObjectReponse;
        }

        [WebMethod]
        public virtual bool SetUserExternalActionShare(string sWSUserName, string sWSPassword, Int32 nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction, eSocialActionPrivacy eActionPrivacy)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            bool bRes = false;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                bRes = Core.Social.Module.SetUserExternalActionShare(nGroupID, nSiteGUID, eSocialPlatform, eAction, eActionPrivacy);
            }

            return bRes;
        }

        [WebMethod]
        public virtual bool SetUserInternalActionPrivacy(string sWSUserName, string sWSPassword, Int32 nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction, eSocialActionPrivacy eActionPrivacy)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            bool bRes = false;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {

                bRes = Core.Social.Module.SetUserInternalActionPrivacy(nGroupID, nSiteGUID, eSocialPlatform, eAction, eActionPrivacy);
            }

            return bRes;
        }

        [WebMethod]
        public virtual eSocialActionPrivacy GetUserExternalActionShare(string sWSUserName, string sWSPassword, Int32 nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            eSocialActionPrivacy ePrivacy = eSocialActionPrivacy.UNKNOWN;

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {

                ePrivacy = Core.Social.Module.GetUserExternalActionShare(nGroupID, nSiteGUID, eSocialPlatform, eAction);
            }

            return ePrivacy;
        }

        [WebMethod]
        public virtual eSocialActionPrivacy GetUserInternalActionPrivacy(string sWSUserName, string sWSPassword, Int32 nSiteGUID, SocialPlatform eSocialPlatform, eUserAction eAction)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nSiteGUID;

            eSocialActionPrivacy ePrivacy = eSocialActionPrivacy.UNKNOWN;

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {

                ePrivacy = Core.Social.Module.GetUserInternalActionPrivacy(nGroupID, nSiteGUID, eSocialPlatform, eAction);
            }

            return ePrivacy;
        }

        [WebMethod]
        public virtual int GetAssetLikeCounter(string sWSUserName, string sWSPassword, int nAssetID, eAssetType assetType)
        {
            int nRes = 0;

            int nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                nRes = Core.Social.Module.GetAssetLikeCounter(nGroupID, nAssetID, assetType);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return nRes;
        }

        [WebMethod]
        public virtual bool Share(string sWSUserName, string sWSPassword, string sSiteGuid, string sDeviceUdid, string sFBActionID, int nMediaID, int nAssetID, eAssetType eAssetType)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            bool bResult = false;

            int nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0 && !string.IsNullOrEmpty(sFBActionID))
            {
                bResult = Core.Social.Module.Share(nGroupID, sSiteGuid, sDeviceUdid, sFBActionID, nMediaID, nAssetID, eAssetType);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return bResult;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(SocialActivityDoc))]
        public virtual List<SocialActivityDoc> GetUserActivityFeed(string sWSUserName, string sWSPassword, string sSiteGuid, int nPageSize, int nPageIndex, string sPicDimension)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            int nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            List<SocialActivityDoc> response = null;
            if (nGroupID != 0)
            {
                BaseSocialBL oBL = BaseSocialBL.GetBaseSocialImpl(nGroupID);
                bool bResult = oBL.GetUserActivityFeed(sSiteGuid, nPageSize, nPageIndex, sPicDimension, out response);
                if (!bResult)
                {
                    HttpContext.Current.Response.StatusCode = 500;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        [Obsolete]
        public virtual FacebookResponse FBUserUnmerge(string sWSUserName, string sWSPassword, string sToken, string sUsername, string sPassword)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                FacebookWrapper oFBWRapper = new FacebookWrapper(nGroupID);
                return oFBWRapper.FBUserUnmerge(sToken, sUsername, sPassword);
            }
            else
            {

                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual FacebookResponse FBUserUnmergeByUserId(string sWSUserName, string sWSPassword, string sUserId)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                FacebookWrapper oFBWRapper = new FacebookWrapper(nGroupID);
                return oFBWRapper.FBUserUnmergeByUserId(sUserId);
            }
            else
            {

                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual FacebookTokenResponse FBTokenValidation(string sWSUserName, string sWSPassword, string sToken)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                FacebookWrapper oFBWRapper = new FacebookWrapper(nGroupID);
                return oFBWRapper.FBTokenValidation(sToken);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual FBSignin FBUserSignin(string sWSUserName, string sWSPassword, string token, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                FacebookWrapper oFBWRapper = new FacebookWrapper(nGroupID);
                return oFBWRapper.FBUserSignin(token, sIP, deviceID, bPreventDoubleLogins);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual bool UpdateFriendsActivityFeed(string sWSUserName, string sWSPassword, int siteGuid, string dbActionId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid;

            bool result = false;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                return Core.Social.Module.UpdateFriendsActivityFeed(nGroupID, siteGuid, dbActionId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return result;
        }

        [WebMethod]
        public virtual void DeleteFriendsFeed(string sWSUserName, string sWSPassword, int siteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid;

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                Core.Social.Module.DeleteFriendsFeed(nGroupID, siteGuid);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
        }

        [WebMethod]
        public virtual void DeleteUserFeed(string sWSUserName, string sWSPassword, int siteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid;

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                Core.Social.Module.DeleteUserFeed(nGroupID, siteGuid);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
        }

        [WebMethod]
        public virtual void MergeFriendsActivityFeed(string sWSUserName, string sWSPassword, int siteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid;

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                Core.Social.Module.MergeFriendsActivityFeed(nGroupID, siteGuid);
            }
            else
                HttpContext.Current.Response.StatusCode = 404;
        }
               
              
        [WebMethod]
        public virtual SocialPrivacySettingsResponse SetUserSocialPrivacySettings(string sWSUserName, string sWSPassword, string siteGUID, SocialPrivacySettings settings)
        {
            SocialPrivacySettingsResponse response = new SocialPrivacySettingsResponse();
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGUID;
            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (groupID != 0)
            {
                response = Core.Social.Module.SetUserSocialPrivacySettings(groupID, siteGUID, settings);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }

        [WebMethod]
        public virtual SocialPrivacySettingsResponse GetUserSocialPrivacySettings(string sWSUserName, string sWSPassword, string siteGUID)
        {
            SocialPrivacySettingsResponse response = new SocialPrivacySettingsResponse();
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGUID;
            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (groupID != 0)
            {
                response = Core.Social.Module.GetUserSocialPrivacySettings(groupID, siteGUID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }
        
        [WebMethod]
        public virtual UserSocialActionResponse AddUserSocialAction(string sWSUserName, string sWSPassword, UserSocialActionRequest actionRequest)
        {
            UserSocialActionResponse response = new UserSocialActionResponse();
            if (actionRequest == null)
            {
                HttpContext.Current.Response.StatusCode = 404;
                return response;
            }

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                UserSocialAction usa = new UserSocialAction(groupID);
                response = usa.AddUserSocialAction(groupID, actionRequest);
            }           
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }       

        [WebMethod]
        public virtual Core.Social.SocialFeed.SocialFeedResponse GetSocialFeed(string sWSUserName, string sWSPassword, string userId, int assetId, eAssetType assetType, eSocialPlatform socialPlatform,
            int pageSize, int pageIndex, long epochStartTime, Core.Social.SocialFeed.SocialFeedOrderBy orderBy)
        {
            Core.Social.SocialFeed.SocialFeedResponse response = new Core.Social.SocialFeed.SocialFeedResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()),
            };

            int groupId = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                response = SocialFeedUtils.GetSocialFeed(groupId, userId, assetId, assetType, socialPlatform, pageSize, pageIndex, epochStartTime, orderBy);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

         [WebMethod]
        public virtual UserSocialActionResponse DeleteUserSocialAction(string sWSUserName, string sWSPassword, string siteGuid, string id)
         {
             UserSocialActionResponse response = new UserSocialActionResponse();
             if (string.IsNullOrEmpty(id))
             {
                 HttpContext.Current.Response.StatusCode = 404;
                 return response;
             }

             Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

             if (groupID != 0)
             {
                 UserSocialAction usa = new UserSocialAction(groupID);
                 response = usa.DeleteUserSocialAction(groupID, siteGuid, id);
             }
             else
             {
                 HttpContext.Current.Response.StatusCode = 404;
             }
             return response;

         }
    }
}

    
