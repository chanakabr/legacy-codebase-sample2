using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.Helper;
using System.Web.Services;
using log4net;
using TVPPro.SiteManager.TvinciPlatform.Users;
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
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetAllFriendsWatched", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("GetAllFriendsWatched-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, initObj.SiteGuid);

            if (groupId > 0)
            {
                try
                {
                    int siteGuid = 0;
                    if (Int32.TryParse(initObj.SiteGuid, out siteGuid))
                    {
                        TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                        return service.GetAllFriendsWatched(siteGuid, maxResult);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("GetAllFriendsWatched->", ex);
                }
            }
            else
                logger.ErrorFormat("GetAllFriendsWatched-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);

            return null;
        }

        [WebMethod(EnableSession = true, Description = "Get all friends that watched the specified media")]
        public FriendWatchedObject[] GetFriendsWatchedByMedia(InitializationObject initObj, int mediaId)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetFriendsWatchedByMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("GetFriendsWatchedByMedia-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, initObj.SiteGuid);

            if (groupId > 0)
            {
                try
                {

                    int siteGuid = 0;
                    if (Int32.TryParse(initObj.SiteGuid, out siteGuid))
                    {
                        TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                        return service.GetFriendsWatchedByMedia(siteGuid, mediaId);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("GetFriendsWatchedByMedia->", ex);
                }
            }
            else
                logger.ErrorFormat("GetFriendsWatchedByMedia-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);

            return null;
        }

        [WebMethod(EnableSession = true, Description = "Get all friends that liked a media")]
        public string[] GetUsersLikedMedia(InitializationObject initObj, int mediaID, bool onlyFriends, int startIndex, int pageSize)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUsersLikedMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("GetUsersLikedMedia-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, initObj.SiteGuid);

            if (groupId > 0)
            {
                try
                {
                    int siteGuid = 0;
                    if (Int32.TryParse(initObj.SiteGuid, out siteGuid))
                    {
                        TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                        return service.GetUsersLikedMedia(siteGuid, mediaID, (int)initObj.Platform, onlyFriends, startIndex, pageSize);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("GetUsersLikedMedia->", ex);
                }
            }
            else
                logger.ErrorFormat("GetUsersLikedMedia-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);

            return null;
        }

        [WebMethod(EnableSession = true, Description = "Get all user's friends")]
        public string[] GetUserFriends(InitializationObject initObj)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserFriends", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("GetUserFriends-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, initObj.SiteGuid);

            if (groupId > 0)
            {
                try
                {
                    int siteGuid = 0;
                    if (Int32.TryParse(initObj.SiteGuid, out siteGuid))
                    {
                        TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                        return service.GetUserFriends(siteGuid);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("GetUserFriends->", ex);
                }
            }
            else
                logger.ErrorFormat("GetUserFriends-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);

            return null;
        }
    }
}
