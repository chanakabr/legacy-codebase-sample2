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
            logger.InfoFormat("GetAllFriendsWatched-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform,
                              initObj.SiteGuid);

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
                    logger.Error("GetAllFriendsWatched->", ex);
                }
            }
            else
                logger.ErrorFormat("GetAllFriendsWatched-> 'Unknown group' Username: {0}, Password: {1}",
                                   initObj.ApiUser, initObj.ApiPass);

            return null;
        }

        [WebMethod(EnableSession = true, Description = "Get all friends that watched the specified media")]
        public FriendWatchedObject[] GetFriendsWatchedByMedia(InitializationObject initObj, int mediaId)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetFriendsWatchedByMedia", initObj.ApiUser,
                                                      initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("GetFriendsWatchedByMedia-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform,
                              initObj.SiteGuid);

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
                    logger.Error("GetFriendsWatchedByMedia->", ex);
                }
            }
            else
                logger.ErrorFormat("GetFriendsWatchedByMedia-> 'Unknown group' Username: {0}, Password: {1}",
                                   initObj.ApiUser, initObj.ApiPass);

            return null;
        }

        [WebMethod(EnableSession = true, Description = "Get all friends that liked a media")]
        public string[] GetUsersLikedMedia(InitializationObject initObj, int mediaID, bool onlyFriends, int startIndex,
                                           int pageSize)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUsersLikedMedia", initObj.ApiUser, initObj.ApiPass,
                                                      SiteHelper.GetClientIP());
            logger.InfoFormat("GetUsersLikedMedia-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform,
                              initObj.SiteGuid);

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
                    logger.Error("GetUsersLikedMedia->", ex);
                }
            }
            else
                logger.ErrorFormat("GetUsersLikedMedia-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser,
                                   initObj.ApiPass);

            return null;
        }

        [WebMethod(EnableSession = true, Description = "Get all user's friends")]
        public string[] GetUserFriends(InitializationObject initObj)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserFriends", initObj.ApiUser, initObj.ApiPass,
                                                      SiteHelper.GetClientIP());
            logger.InfoFormat("GetUserFriends-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform,
                              initObj.SiteGuid);

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
                    logger.Error("GetUserFriends->", ex);
                }
            }
            else
                logger.ErrorFormat("GetUserFriends-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser,
                                   initObj.ApiPass);

            return null;
        }


        [WebMethod(EnableSession = true, Description = "Get all user's friends")]
        public FBConnectConfig FBConfig(InitializationObject initObj, string sSTG)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBConfig", initObj.ApiUser, initObj.ApiPass,
                                                      SiteHelper.GetClientIP());
            logger.InfoFormat("GetFBConfig-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, initObj.SiteGuid);

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
                    logger.Error("GetFBConfig->", ex);
                }
            }
            else
                logger.ErrorFormat("GetFBConfig-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser,
                                   initObj.ApiPass);

            return null;
        }


        public FacebookResponseObject GetFBUserData(InitializationObject initObj, string sToken, string sSTG)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetFBUserData", initObj.ApiUser, initObj.ApiPass,
                                                      SiteHelper.GetClientIP());
            logger.InfoFormat("GetFBUserData-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, initObj.SiteGuid);

            if (groupId > 0)
            {
                try
                {
                    int siteGuid = 0;
                    TVPApiModule.Services.ApiSocialService service =
                        new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetFBUserData(sToken, sSTG);
                }
                catch (Exception ex)
                {
                    logger.Error("GetFBUserData->", ex);
                }
            }
            else
                logger.ErrorFormat("GetFBUserData-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser,
                                   initObj.ApiPass);

            return null;
        }


        public FacebookResponseObject FBUserMerge(InitializationObject initObj, string sToken, string sFBID, string sUsername, string sPassword)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBUserMerge", initObj.ApiUser, initObj.ApiPass,
      SiteHelper.GetClientIP());
            logger.InfoFormat("FBUserMerge-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, initObj.SiteGuid);

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
                    logger.Error("FBUserMerge->", ex);
                }
            }
            else
                logger.ErrorFormat("FBUserMerge-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser,
                                   initObj.ApiPass);

            return null;
        }


        public FacebookResponseObject FBUserRegister(InitializationObject initObj, string sToken, bool bCreateNewDomain, bool bGetNewsletter, string sSTG)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBUserRegister", initObj.ApiUser, initObj.ApiPass,
                    SiteHelper.GetClientIP());
            logger.InfoFormat("FBUserRegister-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, initObj.SiteGuid);

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
                    logger.Error("FBUserRegister->", ex);
                }

            }
            logger.ErrorFormat("FBUserRegister-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser,
                               initObj.ApiPass);

            return null;
        }
    }
}
