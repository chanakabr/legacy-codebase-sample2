using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPApiModule.Objects;
using TVPPro.SiteManager.Helper;
using System.Web.Services;
using log4net;
//using TVPPro.SiteManager.TvinciPlatform.Social;
using System.Configuration;
using TVPApiModule.Services;
using System.Web;


namespace TVPApiServices
{
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class SocialService : System.Web.Services.WebService//, ISocialService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(SocialService));

        [WebMethod(EnableSession = true, Description = "Get all medias that user's friends watched")]
        public TVPApiModule.Objects.Responses.FriendWatchedObject[] GetAllFriendsWatched(InitializationObject initObj, string siteGuid, int maxResult)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetAllFriendsWatched", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetAllFriendsWatched(siteGuid, maxResult);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return null;
        }

        [WebMethod(EnableSession = true, Description = "Get all friends that watched the specified media")]
        public TVPApiModule.Objects.Responses.FriendWatchedObject[] GetFriendsWatchedByMedia(InitializationObject initObj, string siteGuid, int mediaId)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetFriendsWatchedByMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetFriendsWatchedByMedia(siteGuid, mediaId);
                    
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return null;
        }

        [WebMethod(EnableSession = true, Description = "Get all friends that liked a media")]
        public string[] GetUsersLikedMedia(InitializationObject initObj, int mediaID, bool onlyFriends, int startIndex, int pageSize)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUsersLikedMedia", initObj.ApiUser, initObj.ApiPass,
                                                      SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetUsersLikedMedia(initObj.SiteGuid, mediaID, (int)TVPApiModule.Objects.Responses.SocialPlatform.FACEBOOK, onlyFriends,
                                                          startIndex, pageSize);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return null;
        }

        [WebMethod(EnableSession = true, Description = "Get all user's friends")]
        public string[] GetUserFriends(InitializationObject initObj, string siteGuid)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserFriends", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                   
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetUserFriends(siteGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return null;
        }

        [WebMethod(EnableSession = true, Description = "Get all user's friends")]
        public FBConnectConfig FBConfig(InitializationObject initObj, string sSTG)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBConfig", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {

                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);

                    TVPApiModule.Objects.Responses.FacebookConfig config = service.GetFBConfig(sSTG);
                    FBConnectConfig retVal = new FBConnectConfig
                        {
                            appId = config.sFBKey,
                            scope = config.sFBPermissions,
                            apiUser = initObj.ApiUser,
                            apiPass = initObj.ApiPass
                        };
                    return retVal;

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }
            return null;
        }

        [WebMethod(EnableSession = true, Description = "Gets FB user data")]
        public TVPApiModule.Objects.Responses.FacebookResponseObject GetFBUserData(InitializationObject initObj, string sToken, string sSTG)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetFBUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetFBUserData(sToken, sSTG);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return null;
        }

        [WebMethod(EnableSession = true, Description = "Merges FB user")]
        public TVPApiModule.Objects.Responses.FacebookResponseObject FBUserMerge(InitializationObject initObj, string sToken, string sFBID, string sUsername, string sPassword)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBUserMerge", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.FBUserMerge(sToken, sFBID, sUsername, sPassword);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return null;
        }

        [WebMethod(EnableSession = true, Description = "Registers FB user")]
        public TVPApiModule.Objects.Responses.FacebookResponseObject FBUserRegister(InitializationObject initObj, string sToken, bool bCreateNewDomain, bool bGetNewsletter, string sSTG)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBUserRegister", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {

                    ApiSocialService service = new ApiSocialService(groupId, initObj.Platform);
                    var oExtra = new List<TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair>() { new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair() { key = "news", value = bGetNewsletter ? "1" : "0" }, new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair() { key = "domain", value = bCreateNewDomain ? "1" : "0" } };
                    return service.FBUserRegister(sToken, sSTG, oExtra, Context.Request.UserHostAddress);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }

            }
            HttpContext.Current.Items.Add("Error", "Unknown group");

            return null;
        }

        [WebMethod(EnableSession = true, Description = "Gets user social privacy")]
        public string GetUserSocialPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserSocialPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetUserSocialPrivacy(siteGuid, socialPlatform, userAction).ToString();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return TVPApiModule.Objects.Responses.eSocialPrivacy.UNKNOWN.ToString();
        }

        [WebMethod(EnableSession = true, Description = "Gets user allowed social privacy list")]
        public TVPApiModule.Objects.Responses.eSocialPrivacy[] GetUserAllowedSocialPrivacyList(InitializationObject initObj, string siteGuid)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserAllowedSocialPrivacyList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetUserAllowedSocialPrivacyList(siteGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return null;
        }

        [WebMethod(EnableSession = true, Description = "Sets user social privacy")]
        public bool SetUserSocialPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.eSocialPrivacy socialPrivacy)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetUserSocialPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    int iSiteGuid = 0;
                    if (Int32.TryParse(siteGuid, out iSiteGuid))
                    {
                        TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                        return service.SetUserSocialPrivacy(iSiteGuid, socialPlatform, userAction, socialPrivacy);
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return false;
        }

        [WebMethod(EnableSession = true, Description = "Gets user actions")]
        public TVPApiModule.Objects.Responses.UserSocialActionObject[] GetUserActions(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserActions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetUserActions(siteGuid, userAction, assetType, assetID, startIndex, numOfRecords, socialPlatform);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return null;
        }

        [WebMethod(EnableSession = true, Description = "Gets friends actions")]
        public TVPApiModule.Objects.Responses.UserSocialActionObject[] GetFriendsActions(InitializationObject initObj, string siteGuid, string[] userActions, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetFriendsActions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetFriendsActions(siteGuid, userActions, assetType, assetID, startIndex, numOfRecords, socialPlatform);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return null;
        }

        [WebMethod(EnableSession = true, Description = "Does a user requested action")]
        public string DoUserAction(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[] extraParams, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "DoUserAction", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.DoUserAction(siteGuid, initObj.UDID, userAction, extraParams, socialPlatform, assetType, assetID).ToString();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return TVPApiModule.Objects.Responses.SocialActionResponseStatus.ERROR.ToString();
        }

        [WebMethod(EnableSession = true, Description = "Gets User Facebook Action Privacy")]
        public string GetUserExternalActionShare(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserFBActionPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetUserExternalActionShare(siteGuid, userAction, socialPlatform).ToString();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return TVPApiModule.Objects.Responses.eSocialActionPrivacy.UNKNOWN.ToString();
        }

        [WebMethod(EnableSession = true, Description = "Gets User Internal Action Privacy")]
        public string GetUserInternalActionPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserInternalActionPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetUserInternalActionPrivacy(siteGuid, userAction, socialPlatform).ToString();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return TVPApiModule.Objects.Responses.eSocialActionPrivacy.UNKNOWN.ToString();
        }

        [WebMethod(EnableSession = true, Description = "Sets User Facebook Action Privacy")]
        public bool SetUserExternalActionShare(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eSocialActionPrivacy actionPrivacy)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetUserFBActionPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.SetUserExternalActionShare(siteGuid, userAction, socialPlatform, actionPrivacy);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return false;
        }

        [WebMethod(EnableSession = true, Description = "Sets User Internal Action Privacy")]
        public bool SetUserInternalActionPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eSocialActionPrivacy actionPrivacy)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetUserInternalActionPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.SetUserInternalActionPrivacy(siteGuid, userAction, socialPlatform, actionPrivacy);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return false;
        }
    }
}
