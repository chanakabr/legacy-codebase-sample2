using System;
using System.Collections.Generic;
using Core.Social;
using Core.Social.SocialFeed;
using System.Web.Script.Serialization;
using System.Linq;
using TVinciShared;
using KLogMonitor;
using System.Reflection;
using Core.Users;
using ApiObjects;
using ApiObjects.Social;


namespace WS_Social.SocialFeed
{
    public partial class SocialFeed : System.Web.UI.Page
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected void Page_Load(object sender, EventArgs e)
        {
            eSocialPlatform platform = GetSocialPlatformType();
            string queryParam = Request.QueryString["q"];
            if (!string.IsNullOrEmpty(queryParam))
            {
                string siteGuid = Request.QueryString["siteGuid"];
                string sGroupId = Request.QueryString["groupId"];
                int groupId;
                if (int.TryParse(sGroupId, out groupId))
                {
                    try
                    {
                        List<SocialFeedItem> socialPlatformResp = new List<SocialFeedItem>();
                        UserResponseObject userData = Core.Social.Utils.GetUserDataByID(siteGuid, groupId);

                        switch (platform)
                        {
                            case eSocialPlatform.Facebook:

                                if (userData.m_RespStatus == ResponseStatus.OK && !string.IsNullOrEmpty(userData.m_user.m_oBasicData.m_sFacebookToken))
                                {
                                    try
                                    {
                                        socialPlatformResp = SocialFeedUtils.Facebook.GetFacebookSocialFeed(queryParam, TVinciShared.WS_Utils.GetTcmIntValue("SocialFeed_FB_item_count"), Core.Social.Utils.Decrypt(userData.m_user.m_oBasicData.m_sFacebookToken, Core.Social.Utils.GetValFromConfig("FB_TOKEN_KEY")));
                                    }
                                    catch (Exception)
                                    {
                                        //Fallback to app token in case user token is invalid 
                                        FacebookWrapper facebookWrapper = new FacebookWrapper(groupId);
                                        socialPlatformResp = SocialFeedUtils.Facebook.GetFacebookSocialFeed(queryParam, TVinciShared.WS_Utils.GetTcmIntValue("SocialFeed_FB_item_count"), SocialFeedUtils.Facebook.GetFacebookAppAccessToken(groupId));
                                    }
                                }
                                // User wasn't found or has no facebook access_token
                                else
                                {
                                    socialPlatformResp = SocialFeedUtils.Facebook.GetFacebookSocialFeed(queryParam, TVinciShared.WS_Utils.GetTcmIntValue("SocialFeed_FB_item_count"), SocialFeedUtils.Facebook.GetFacebookAppAccessToken(groupId));
                                }
                                break;

                            case eSocialPlatform.InApp:
                                int mediaId;
                                if (int.TryParse(queryParam, out mediaId))
                                    socialPlatformResp = SocialFeedUtils.InApp.GetInAppSocialFeed(mediaId, groupId, TVinciShared.WS_Utils.GetTcmIntValue("SocialFeed_InApp_item_count"));
                                break;

                            case eSocialPlatform.Twitter:
                                if (userData.m_RespStatus == ResponseStatus.OK && !string.IsNullOrEmpty(userData.m_user.m_oBasicData.m_sTwitterToken) && !string.IsNullOrEmpty(userData.m_user.m_oBasicData.m_sTwitterTokenSecret))
                                {
                                    try
                                    {
                                        socialPlatformResp = SocialFeedUtils.Twitter.GetTwitterUserSocialFeed(queryParam, TVinciShared.WS_Utils.GetTcmConfigValue("TWITTER_CONSUMER_KEY"), TVinciShared.WS_Utils.GetTcmConfigValue("TWITTER_CONSUMER_SECRET"), userData.m_user.m_oBasicData.m_sTwitterToken, userData.m_user.m_oBasicData.m_sTwitterTokenSecret, TVinciShared.WS_Utils.GetTcmIntValue("SocialFeed_Twitter_item_count"));
                                    }
                                    catch (Exception)
                                    {
                                        socialPlatformResp = SocialFeedUtils.Twitter.GetTwitterAppSocialFeed(queryParam, TVinciShared.WS_Utils.GetTcmIntValue("SocialFeed_Twitter_item_count"), groupId);
                                    }
                                }
                                else
                                    // User wasn't found or has no twitter access_token
                                    socialPlatformResp = SocialFeedUtils.Twitter.GetTwitterAppSocialFeed(queryParam, TVinciShared.WS_Utils.GetTcmIntValue("SocialFeed_Twitter_item_count"), groupId);
                                break;
                        }
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        Response.Write(serializer.Serialize(socialPlatformResp));
                        Response.Cache.SetMaxAge(TimeSpan.FromMinutes(WS_Utils.GetTcmIntValue(string.Format("SocialFeed_{0}_TTL", platform))));
                    }
                    catch (Exception ex)
                    {
                        log.Error("ERROR - " +
                            string.Format("Platform: {0} , q = {1} , group = {2} , Msg: {3} , Stacktrace: {4}",
                            platform, queryParam, groupId, ex.Message, ex.StackTrace), ex);
                        ReturnError("Error getting data from " + platform);
                    }
                }
                else
                {
                    ReturnError("GroupId Missing or invalid");
                }
            }
            else
            {
                ReturnError("No page specified");
            }


        }

        private eSocialPlatform GetSocialPlatformType()
        {
            try
            {
                return (eSocialPlatform)Enum.Parse(typeof(eSocialPlatform), Request.QueryString["socialPlatform"], true);
            }
            catch (Exception)
            {
                ReturnError("Unknown platform");
                return eSocialPlatform.Unknown;
            }
        }

        private void ReturnError(string message)
        {
            Response.Write(message);
            Response.CacheControl = "no-cache";
            Response.StatusCode = 500;
            Response.End();
        }
    }
}
//https://graph.facebook.com/oauth/access_token?client_id=526958710657762&client_secret=0dea404cc220356bfdcedab2c372fbf5&grant_type=client_credentials
//dexter/?fields=posts.fields(message,link,picture,created_time,likes.limit(1),comments.limit(25).fields(message,from,like_count,created_time))
//526958710657762|9sLtGBJGdslviqzZsgE3TYKoudo
//Pic by id = http://graph.facebook.com/67563683055/picture?type=square
