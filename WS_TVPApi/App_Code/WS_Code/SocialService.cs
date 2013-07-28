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
using TVPPro.SiteManager.TvinciPlatform.Social;
using System.Configuration;
using TVPApiModule.Services;
using System.Web;


namespace TVPApiServices
{
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class SocialService : System.Web.Services.WebService, ISocialService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(SocialService));

        [WebMethod(EnableSession = true, Description = "Get all medias that user's friends watched")]
        public FriendWatchedObject[] GetAllFriendsWatched(InitializationObject initObj, int maxResult)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetAllFriendsWatched", initObj.ApiUser, initObj.ApiPass,
                                                      SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    int siteGuid = 0;
                    if (Int32.TryParse(initObj.SiteGuid, out siteGuid))
                    {
                        TVPApiModule.Services.ApiSocialService service =
                            new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                        return service.GetAllFriendsWatched(siteGuid, maxResult);
                    }
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
        public FriendWatchedObject[] GetFriendsWatchedByMedia(InitializationObject initObj, int mediaId)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetFriendsWatchedByMedia", initObj.ApiUser,
                                                      initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    int siteGuid = 0;
                    if (Int32.TryParse(initObj.SiteGuid, out siteGuid))
                    {
                        TVPApiModule.Services.ApiSocialService service =
                            new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                        return service.GetFriendsWatchedByMedia(siteGuid, mediaId);
                    }
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
        public string[] GetUsersLikedMedia(InitializationObject initObj, int mediaID, bool onlyFriends, int startIndex,
                                           int pageSize)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUsersLikedMedia", initObj.ApiUser, initObj.ApiPass,
                                                      SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    int siteGuid = 0;
                    if (Int32.TryParse(initObj.SiteGuid, out siteGuid))
                    {
                        TVPApiModule.Services.ApiSocialService service =
                            new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                        return service.GetUsersLikedMedia(siteGuid, mediaID, (int)initObj.Platform, onlyFriends,
                                                          startIndex, pageSize);
                    }
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
        public string[] GetUserFriends(InitializationObject initObj)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserFriends", initObj.ApiUser, initObj.ApiPass,
                                                      SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    int siteGuid = 0;
                    if (Int32.TryParse(initObj.SiteGuid, out siteGuid))
                    {
                        TVPApiModule.Services.ApiSocialService service =
                            new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                        return service.GetUserFriends(siteGuid);
                    }
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
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBConfig", initObj.ApiUser, initObj.ApiPass,
                                                      SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {

                    TVPApiModule.Services.ApiSocialService service =
                        new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);

                    FacebookConfig config = service.GetFBConfig(sSTG);
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
        public FacebookResponseObject GetFBUserData(InitializationObject initObj, string sToken, string sSTG)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetFBUserData", initObj.ApiUser, initObj.ApiPass,
                                                      SiteHelper.GetClientIP());
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
        public FacebookResponseObject FBUserMerge(InitializationObject initObj, string sToken, string sFBID, string sUsername, string sPassword)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBUserMerge", initObj.ApiUser, initObj.ApiPass,
      SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service =
                        new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
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
        public FacebookResponseObject FBUserRegister(InitializationObject initObj, string sToken, bool bCreateNewDomain, bool bGetNewsletter, string sSTG)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBUserRegister", initObj.ApiUser, initObj.ApiPass,
                    SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {

                    ApiSocialService service = new ApiSocialService(groupId, initObj.Platform);
                    var oExtra = new List<ExtraKeyValue>() { new ExtraKeyValue() { key = "news", value = bGetNewsletter ? "1" : "0" }, new ExtraKeyValue() { key = "domain", value = bCreateNewDomain ? "1" : "0" } };
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
        public eSocialPrivacy GetUserSocialPrivacy(InitializationObject initObj)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserSocialPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    int siteGuid = 0;
                    if (Int32.TryParse(initObj.SiteGuid, out siteGuid))
                    {
                        TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                        return service.GetUserSocialPrivacy(siteGuid);
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return eSocialPrivacy.UNKNOWN;
        }

        [WebMethod(EnableSession = true, Description = "Gets user allowed social privacy list")]
        public eSocialPrivacy[] GetUserAllowedSocialPrivacyList(InitializationObject initObj)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserAllowedSocialPrivacyList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    int siteGuid = 0;
                    if (Int32.TryParse(initObj.SiteGuid, out siteGuid))
                    {
                        TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                        return service.GetUserAllowedSocialPrivacyList(siteGuid);
                    }
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
        public bool SetUserSocialPrivacy(InitializationObject initObj, eSocialPrivacy socialPrivacy)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetUserSocialPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    int siteGuid = 0;
                    if (Int32.TryParse(initObj.SiteGuid, out siteGuid))
                    {
                        TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                        return service.SetUserSocialPrivacy(siteGuid, socialPrivacy);
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
        public UserSocialActionObject[] GetUserActions(InitializationObject initObj, eUserAction userAction, int mediaId, int startIndex, int numOfRecords, SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserActions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetUserActions(initObj.SiteGuid, userAction, mediaId, startIndex, numOfRecords, socialPlatform);
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
        public UserSocialActionObject[] GetFriendsActions(InitializationObject initObj, eUserAction userAction, int mediaId, int startIndex, int numOfRecords, SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetFriendsActions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetFriendsActions(initObj.SiteGuid, userAction, mediaId, startIndex, numOfRecords, socialPlatform);
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
        public SocialActionResponseStatus DoUserAction(InitializationObject initObj, int mediaId, eUserAction userAction, ExtraKeyValue[] extraParams, SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "DoUserAction", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.DoUserAction(initObj.SiteGuid, mediaId, userAction, extraParams, socialPlatform);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return SocialActionResponseStatus.ERROR;
        }
    }
}
