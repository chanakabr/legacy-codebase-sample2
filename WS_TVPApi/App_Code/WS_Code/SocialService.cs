using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Web.Script.Serialization;
using Tvinci.Data.TVMDataLoader.Protocols.ChannelsMedia;
using TVPApi;
using TVPApiModule.Objects;
using TVPPro.SiteManager.DataLoaders;
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
        public string[] GetUsersLikedMedia(InitializationObject initObj, int mediaID, bool onlyFriends, int startIndex, int pageSize)
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
                        return service.GetUsersLikedMedia(siteGuid, mediaID, (int)SocialPlatform.FACEBOOK, onlyFriends,
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

                    FacebookConfig config = service.GetFBConfig("0");
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
                    return service.GetFBUserData(sToken, "0");
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
                    var oExtra = new List<KeyValuePair>() { new KeyValuePair() { key = "news", value = bGetNewsletter ? "1" : "0" }, new KeyValuePair() { key = "domain", value = bCreateNewDomain ? "1" : "0" } };
                    return service.FBUserRegister(sToken, "0", oExtra, Context.Request.UserHostAddress);

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
        public string GetUserSocialPrivacy(InitializationObject initObj, SocialPlatform socialPlatform, eUserAction userAction)
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
                        return service.GetUserSocialPrivacy(siteGuid, socialPlatform, userAction).ToString();
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return eSocialPrivacy.UNKNOWN.ToString();
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
        public bool SetUserSocialPrivacy(InitializationObject initObj, SocialPlatform socialPlatform, eUserAction userAction, eSocialPrivacy socialPrivacy)
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
                        return service.SetUserSocialPrivacy(siteGuid, socialPlatform, userAction, socialPrivacy);
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
        public SocialActivityDoc[] GetUserActions(InitializationObject initObj, eUserAction userAction, eAssetType assetType, int assetID, int startIndex, int numOfRecords, SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserActions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetUserActions(initObj.SiteGuid, userAction, assetType, assetID, startIndex, numOfRecords, socialPlatform);
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
        public SocialActivityDoc[] GetFriendsActions(InitializationObject initObj, string[] userActions, eAssetType assetType, int assetID, int startIndex, int numOfRecords, SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetFriendsActions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetFriendsActions(initObj.SiteGuid, userActions, assetType, assetID, startIndex, numOfRecords, socialPlatform);
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
        public DoSocialActionResponse DoUserAction(InitializationObject initObj, eUserAction userAction, KeyValuePair[] extraParams, SocialPlatform socialPlatform, eAssetType assetType, int assetID)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "DoUserAction", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.DoUserAction(initObj.SiteGuid, initObj.UDID, userAction, extraParams, socialPlatform, assetType, assetID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return new DoSocialActionResponse() { m_eActionResponseStatusExtern = SocialActionResponseStatus.ERROR, m_eActionResponseStatusIntern = SocialActionResponseStatus.ERROR };
        }

        [WebMethod(EnableSession = true, Description = "Gets User Facebook Action Privacy")]
        public string GetUserExternalActionShare(InitializationObject initObj, eUserAction userAction, SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserExternalActionShare", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetUserExternalActionShare(initObj.SiteGuid, userAction, socialPlatform).ToString();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return eSocialActionPrivacy.UNKNOWN.ToString();
        }

        [WebMethod(EnableSession = true, Description = "Gets User Internal Action Privacy")]
        public string GetUserInternalActionPrivacy(InitializationObject initObj, eUserAction userAction, SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserInternalActionPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.GetUserInternalActionPrivacy(initObj.SiteGuid, userAction, socialPlatform).ToString();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            HttpContext.Current.Items.Add("Error", "Unknown group");
            return eSocialActionPrivacy.UNKNOWN.ToString();
        }

        [WebMethod(EnableSession = true, Description = "Sets User Facebook Action Privacy")]
        public bool SetUserExternalActionShare(InitializationObject initObj, eUserAction userAction, SocialPlatform socialPlatform, eSocialActionPrivacy actionPrivacy)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetUserFBActionPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.SetUserExternalActionShare(initObj.SiteGuid, userAction, socialPlatform, actionPrivacy);
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
        public bool SetUserInternalActionPrivacy(InitializationObject initObj, eUserAction userAction, SocialPlatform socialPlatform, eSocialActionPrivacy actionPrivacy)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetUserInternalActionPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupId, initObj.Platform);
                    return service.SetUserInternalActionPrivacy(initObj.SiteGuid, userAction, socialPlatform, actionPrivacy);
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
        public SocialFeed GetSocialFeed(InitializationObject initObj, int mediaId, eSocialPlatform socialPlatform, int numOfItems, long epochStartTime)
        {
            SocialFeed resSocialFeed = new SocialFeed();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetUserInternalActionPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    int siteGuid;
                    if (mediaId == 0)
                    {
                        resSocialFeed.Error = "No mediaId";
                        return resSocialFeed;
                    }
                    if (!int.TryParse(initObj.SiteGuid, out siteGuid))
                    {
                        resSocialFeed.Error = "No siteGuid";
                        return resSocialFeed;
                    }
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.QueryString.Add("mediaId", mediaId.ToString());
                        webClient.QueryString.Add("groupId", groupId.ToString());
                        webClient.QueryString.Add("siteGuid", initObj.SiteGuid);
                        webClient.QueryString.Add("platform", socialPlatform.ToString());
                        using (StreamReader streamReader = new StreamReader(webClient.OpenRead(ConfigurationManager.AppSettings["SocialFeedProxy"])))
                        {
                            string socialFeedStr = streamReader.ReadToEnd();
                            try
                            {
                                Dictionary<string, List<SocialFeedItem>> respFeed = new JavaScriptSerializer().Deserialize<SerializableDictionary<string, List<SocialFeedItem>>>(socialFeedStr).ToDictionary(item => item.Key, item => item.Value);

                                foreach (KeyValuePair<string, List<SocialFeedItem>> item in respFeed)
                                {
                                    var y = item.Value.Where(post => epochStartTime != 0 ? post.CreateDate < epochStartTime : true).Take(numOfItems == 0 ? int.MaxValue : numOfItems);
                                    resSocialFeed.Feed.Add(item.Key, y.ToList());
                                }

                            }
                            catch (Exception)
                            {
                                resSocialFeed.Error = socialFeedStr;
                            }

                        }
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
                resSocialFeed.Error = "Unknown group";
            }

            return resSocialFeed;
        }

        [WebMethod(EnableSession = true, Description = "Removes data stored in Kaltura's DB which makes Facebook actions (login, share, like, etc) on the customer site feasible. The user will still be able to see the actions he performed as these are logged as 'Tvinci actions'. However, all his friends won't be able to view his actions as they'll be deleted from social feed")]
        public FacebookResponseObject FBUserUnmerge(InitializationObject initObj, string token, string username, string password)
        {
            FacebookResponseObject response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBUserUnmerge", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    response = new ApiSocialService(groupId, initObj.Platform).FBUserUnmerge(token, username, password);
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

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Removes data stored in Kaltura's DB which makes Facebook actions (login, share, like, etc) on the customer site feasible. The user will still be able to see the actions he performed as these are logged as 'Tvinci actions'. However, all his friends won't be able to view his actions as they'll be deleted from social feed")]
        public SocialActivityDoc[] GetUserActivityFeed(InitializationObject initObj, string siteGuid, int nPageSize, int nPageIndex, string sPicDimension)
        {
            SocialActivityDoc[] response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserActivityFeed", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    response = new ApiSocialService(groupId, initObj.Platform).GetUserActivityFeed(siteGuid, nPageSize, nPageIndex, sPicDimension);
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

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Validate Facebook user by token")]
        public FacebookTokenResponse FBTokenValidation(InitializationObject initObj, string token)
        {
            FacebookTokenResponse response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBTokenValidation", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    response = new ApiSocialService(groupId, initObj.Platform).FBTokenValidation(token);
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

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Sign-In using Facebook token")]
        public FBSignin FBUserSignin(InitializationObject initObj, string token)
        {
            FBSignin responseData = new FBSignin();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBUserSignin", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    bool isSingleLogin = TVPApi.ConfigManager.GetInstance().GetConfig(groupId, initObj.Platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                    responseData = new ApiSocialService(groupId, initObj.Platform).FBUserSignin(token, initObj.UDID, isSingleLogin);
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

            return responseData;
        }
    }
}
