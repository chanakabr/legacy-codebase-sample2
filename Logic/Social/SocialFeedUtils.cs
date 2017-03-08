using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using ODBCWrapper;
using Core.Social.SocialFeed;
using Core.Social.SocialFeed.SocialFeedJsonTemplates;
using LinqToTwitter;
using KLogMonitor;
using System.Reflection;
using DAL;
using TVinciShared;
using ApiObjects;
using Core.Users;
using ApiObjects.Response;
using Core.Catalog.Request;
using Core.Catalog;
using ApiObjects.Social;
using Core.Catalog.Response;

namespace Core.Social
{
    public static class SocialFeedUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string SOCIAL_FEED_TAGS_TTL_KEY = "SocialFeed_Tags_TTL";
        private const string SOCIAL_FEED_PLATFORM_TTL_KEY_FORMAT = "SocialFeed_{0}_TTL";
        private const string SOCIAL_MEDIA_TAGS_CACHE_KEY_FORMAT = "social_tags_media_{0}";
        private const string SOCIAL_FEED_PLATFORM_CACHE_KEY_FORMAT = "social_feed_platform={0}_q={1}";
        private const string QUERY_PARAM_FORMAT = "{0}&siteGuid={1}&groupId={2}";

        public static class Facebook
        {
            private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

            static readonly string FB_GRAPH_SOCIALFEED_FIELDS = "posts?fields=from,message,link,picture,created_time,likes.limit(1).summary(true),comments.limit(25).fields(message,from,like_count,created_time)";
            static readonly string FB_GRAPH_URI_PREFIX = Utils.GetValFromConfig("FB_GRAPH_URI");
            static Dictionary<int, string> _accessTokenDict = new Dictionary<int, string>();
            static object _locker = new object();

            static public List<SocialFeedItem> GetFacebookSocialFeed(string pageName, int numOfPosts, string accessToken)
            {
                int nStatus = 0;

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}/{1}/{2}&access_token={3}", FB_GRAPH_URI_PREFIX, pageName, FB_GRAPH_SOCIALFEED_FIELDS, accessToken);

                string fbRespStr = Utils.SendGetHttpReq(sb.ToString(), ref nStatus, string.Empty, string.Empty);
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                FacebookResp parsedResp = serializer.Deserialize<FacebookResp>(fbRespStr);

                while (parsedResp.data.Count < numOfPosts)
                {
                    FacebookResp tmpResp = GetNextPage(parsedResp.paging.next);
                    if (tmpResp.data.Count > 0)
                    {
                        parsedResp.data.AddRange(tmpResp.data);
                    }
                    else
                        break;
                }
                return ParsePlatformResp(parsedResp, eSocialPlatform.Facebook);
            }

            static private FacebookResp GetNextPage(string nextPageUrl)
            {
                int nStatus = 0;
                string fbRespStr = Utils.SendGetHttpReq(nextPageUrl, ref nStatus, string.Empty, string.Empty);
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                return serializer.Deserialize<FacebookResp>(fbRespStr);
            }

            static public string GetFacebookAppAccessToken(int groupId)
            {
                if (_accessTokenDict.ContainsKey(groupId))
                {
                    return _accessTokenDict[groupId];
                }
                lock (_locker)
                {
                    if (!_accessTokenDict.ContainsKey(groupId))
                    {
                        FacebookWrapper facebookWrapper = new FacebookWrapper(groupId);
                        string clientId = facebookWrapper.FBConfig.sFBKey;
                        string clientSecret = facebookWrapper.FBConfig.sFBSecret;
                        int nStatus = 0;
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("{0}/oauth/access_token?client_id={1}&client_secret={2}&grant_type=client_credentials", FB_GRAPH_URI_PREFIX, clientId, clientSecret);

                        string retVal = Utils.SendGetHttpReq(sb.ToString(), ref nStatus, string.Empty, string.Empty);

                        _accessTokenDict.Add(groupId, retVal.Replace("access_token=", ""));

                    }
                }
                return _accessTokenDict[groupId];
            }
        }

        public static class Twitter
        {
            static readonly string TWITTER_CONSUMER_KEY = TVinciShared.WS_Utils.GetTcmConfigValue("TWITTER_CONSUMER_KEY");
            static readonly string TWITTER_CONSUMER_SECRET = TVinciShared.WS_Utils.GetTcmConfigValue("TWITTER_CONSUMER_SECRET");
            static Dictionary<int, ApplicationOnlyAuthorizer> _accessTokenDictionary = new Dictionary<int, ApplicationOnlyAuthorizer>();
            static object _locker = new object();

            static public List<SocialFeedItem> GetTwitterAppSocialFeed(string hashTagVal, int numOfPosts, int groupId)
            {
                TwitterContext context = new TwitterContext(getAuthorizer(groupId));

                return context.Search.Where(s => s.Query == "#" + hashTagVal && s.Type == SearchType.Search && s.Count == numOfPosts).Select(x => x.Statuses.Select(s => new SocialFeedItem()
                {
                    Body = s.Text,
                    CreatorName = s.User.Name,
                    CreateDate = TVinciShared.DateUtils.DateTimeToUnixTimestamp(s.CreatedAt),
                    CreatorImageUrl = s.User.ProfileImageUrl,
                    PopularityCounter = s.FavoriteCount.HasValue ? s.FavoriteCount.Value : 0,
                }).ToList()).SingleOrDefault();
            }

            static public List<SocialFeedItem> GetTwitterUserSocialFeed(string hashTagVal, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, int numOfPosts)
            {

                TwitterContext context = new TwitterContext(new SingleUserAuthorizer()
                {
                    Credentials = new SingleUserInMemoryCredentials()
                    {
                        ConsumerKey = consumerKey,
                        ConsumerSecret = consumerSecret,
                        TwitterAccessToken = accessToken,
                        TwitterAccessTokenSecret = accessTokenSecret
                    }
                });
                context.AuthorizedClient.Authorize();

                return context.Search.Where(s => s.Query == "#" + hashTagVal && s.Type == SearchType.Search && s.Count == numOfPosts).Select(x => x.Statuses.Select(s => new SocialFeedItem()
                {
                    Body = s.Text,
                    CreatorName = s.User.Name,
                    CreateDate = TVinciShared.DateUtils.DateTimeToUnixTimestamp(s.CreatedAt),
                    CreatorImageUrl = s.User.ProfileImageUrl,
                    PopularityCounter = s.FavoriteCount.HasValue ? s.FavoriteCount.Value : 0,


                }).ToList()).SingleOrDefault();

            }

            private static ApplicationOnlyAuthorizer getAuthorizer(int groupId)
            {
                if (_accessTokenDictionary.ContainsKey(groupId))
                {
                    return _accessTokenDictionary[groupId];
                }
                lock (_locker)
                {
                    if (!_accessTokenDictionary.ContainsKey(groupId))
                    {
                        ApplicationOnlyAuthorizer authorizer = new ApplicationOnlyAuthorizer()
                        {
                            Credentials = new InMemoryCredentials()
                            {
                                ConsumerKey = TWITTER_CONSUMER_KEY,
                                ConsumerSecret = TWITTER_CONSUMER_SECRET
                            }
                        };
                        authorizer.Authorize();
                        _accessTokenDictionary.Add(groupId, authorizer);
                    }
                }
                return _accessTokenDictionary[groupId];
            }
        }

        public static class InApp
        {
            static public List<SocialFeedItem> GetInAppSocialFeed(int assetId, ApiObjects.eAssetType assetType, int groupId, int numOfPosts)
            {   
                string signatureString = Guid.NewGuid().ToString();
                AssetCommentsRequest request = new AssetCommentsRequest()
                {
                    m_sSignString = signatureString,
                    m_sSignature = GetCatalogSignature(signatureString),
                    m_oFilter = new Filter(),
                    m_nGroupID = groupId,
                    m_nPageSize = numOfPosts,
                    assetId = assetId,
                    assetType = (eAssetType)assetType,
                };

                AssetCommentsListResponse resp = null;

                try
                {
                    resp = (AssetCommentsListResponse)request.GetResponse(request);
                }
                catch (Exception ex)
                {
                    log.Error("GetInAppSocialFeed - " + string.Format("ERROR: failed getting response from catalog. msg = {0}", ex.Message), ex);
                }

                List<SocialFeedItem> lstSocialFeed = null;

                if (resp != null)
                {
                    lstSocialFeed = ParsePlatformResp(resp, eSocialPlatform.InApp);
                }
                else
                {
                    lstSocialFeed = new List<SocialFeedItem>();
                }

                return (lstSocialFeed);
            }

            static public List<SocialFeedItem> GetInAppSocialFeed(int mediaId, int groupId, int numOfPosts)
            {
                string signatureString = Guid.NewGuid().ToString();
                CommentsListRequest request = new CommentsListRequest()
                {
                    m_nGroupID = groupId,
                    m_nMediaID = mediaId,
                    m_nPageSize = numOfPosts,
                    m_nPageIndex = 0,
                    m_sSignString = signatureString,
                    m_sSignature = GetCatalogSignature(signatureString),
                    m_oFilter = new Filter(),

                };

                CommentsListResponse resp = null;

                try
                {
                    resp = (CommentsListResponse)request.GetResponse(request);
                }
                catch (Exception ex)
                {
                    log.Error("GetInAppSocialFeed - " + string.Format("ERROR: failed getting response from catalog. msg = {0}", ex.Message), ex);
                }

                List<SocialFeedItem> lstSocialFeed = null;

                if (resp != null)
                {
                    lstSocialFeed = ParsePlatformResponse(resp, eSocialPlatform.InApp);
                }
                else
                {
                    lstSocialFeed = new List<SocialFeedItem>();
                }

                return (lstSocialFeed);
            }

            private static string GetCatalogSignature(string signString)
            {
                string retVal;
                //Get key from DB
                string hmacSecret = "liat regev";
                // The HMAC secret as configured in the skin
                // Values are always transferred using UTF-8 encoding
                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

                // Calculate the HMAC
                // signingString is the SignString from the request
                HMACSHA1 myhmacsha1 = new HMACSHA1(encoding.GetBytes(hmacSecret));
                retVal = System.Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(signString)));
                myhmacsha1.Clear();
                return retVal;
            }

        }

        private static List<SocialFeedItem> ParsePlatformResponse(object socialPlatformResp, eSocialPlatform platform)
        {
            ISocialFeed feed;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            switch (platform)
            {
                case eSocialPlatform.Facebook:
                    feed = (FacebookResp)socialPlatformResp;
                    break;
                case eSocialPlatform.InApp:
                    feed = (AssetCommentsListResponse)socialPlatformResp;
                    break;

                case eSocialPlatform.Twitter:
                    feed = serializer.Deserialize<TweeterResp>((string)socialPlatformResp);
                    break;
                default:
                    return null;
            }
            return feed.ToBaseSocialFeedObj();

        }

        private static List<SocialFeedItem> ParsePlatformResp(object socialPlatformResp, eSocialPlatform platform)
        {
            ISocialFeed feed;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            switch (platform)
            {
                case eSocialPlatform.Facebook:
                    feed = (FacebookResp)socialPlatformResp;
                    break;
                case eSocialPlatform.InApp:
                    feed = (CommentsListResponse)socialPlatformResp;
                    break;

                case eSocialPlatform.Twitter:
                    feed = serializer.Deserialize<TweeterResp>((string)socialPlatformResp);
                    break;
                default:
                    return null;
            }
            return feed.ToBaseSocialFeedObj();

        }

        public static List<SocialFeedMediaTag> GetSocialFeedMediaTags(int groupId, int mediaID)
        {
            List<SocialFeedMediaTag> response = null;

            string cacheKey = string.Format(SOCIAL_MEDIA_TAGS_CACHE_KEY_FORMAT, mediaID);
            CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.CACHE);
            response = cbManager.Get<List<SocialFeedMediaTag>>(cacheKey);

            if (response != null)
            {
                return response;
            }

            SocialDAL socialDal = new SocialDAL(groupId);

            DataTable dt = socialDal.GetSocialFeedMediaTags(mediaID);

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                response = new List<SocialFeedMediaTag>();
                SocialFeedMediaTag tag;
                string socialPlatformStr;
                eSocialPlatform socialPlatform;
                foreach (DataRow row in dt.Rows)
                {
                    socialPlatformStr = ODBCWrapper.Utils.GetSafeStr(row, "DESCRIPTION");
                    if (!Enum.TryParse(socialPlatformStr, true, out socialPlatform))
                    {
                        socialPlatform = eSocialPlatform.Unknown;
                    }
                    tag = new SocialFeedMediaTag()
                    {
                        SocialPlatform = socialPlatform,
                        Value = ODBCWrapper.Utils.GetSafeStr(row, "VALUE"),
                    };

                    response.Add(tag);
                }
            }

            cbManager.Set(cacheKey, response, (uint)WS_Utils.GetTcmIntValue(SOCIAL_FEED_TAGS_TTL_KEY));

            return response;
        }

        public static List<KeyValuePair<string, string>> GetSocialFeedTags(int mediaID)
        {
            List<KeyValuePair<string, string>> retVal = new List<KeyValuePair<string, string>>();
            ODBCWrapper.DataSetSelectQuery query = new DataSetSelectQuery();
            query += "SELECT mtt.DESCRIPTION, t.VALUE FROM media_tags_types mtt, tags t, media_tags mt WHERE  mtt.STATUS=1 and t.STATUS=1 and mt.TAG_ID=t.ID and mtt.ID=t.Tag_TYPE_ID and mtt.TagFamilyID = 1 and mt.STATUS=1 and ";
            query += ODBCWrapper.Parameter.NEW_PARAM("mt.Media_ID", "=", mediaID);
            DataTable dt = query.Execute("query", true);
            if (dt.DefaultView.Count > 0)
            {
                retVal.AddRange(from DataRow row in dt.Rows select new KeyValuePair<string, string>(row["DESCRIPTION"].ToString(), row["VALUE"].ToString()));
            }

            return retVal;
        }

        public static SocialFeedResponse GetSocialFeed(int groupId, string userId, int assetId, ApiObjects.eAssetType assetType, eSocialPlatform socialPlatform, int pageSize, int pageIndex, 
            long createDateSince, SocialFeedOrderBy orderBy)
        {
            Core.Social.SocialFeed.SocialFeedResponse response = new Core.Social.SocialFeed.SocialFeedResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()),
                SocialFeeds = new List<SocialFeedItem>()
            };

            Core.Social.SocialFeed.SocialFeedMediaTag mediaTag = null;
            
            try
            {
                if (socialPlatform == eSocialPlatform.InApp)
                {
                    mediaTag = new SocialFeedMediaTag() { SocialPlatform = eSocialPlatform.InApp, Value = eSocialPlatform.InApp.ToString() };
                }
                else if (assetType == ApiObjects.eAssetType.MEDIA)
                {
                    var mediaTags = SocialFeedUtils.GetSocialFeedMediaTags(groupId, assetId);
                    if (mediaTags != null && mediaTags.Count > 0)
                    {
                        mediaTag = mediaTags.Where(mt => mt.SocialPlatform == socialPlatform).FirstOrDefault();
                    }
                }

                if (mediaTag == null)
                {
                    response.SocialFeeds = new List<SocialFeedItem>();
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    return response;
                }

                List<Core.Social.SocialFeed.SocialFeedItem> socialPlatformFeed = null;
                UserResponseObject userData = Core.Social.Utils.GetUserDataByID(userId, groupId);
                string cacheKey;
                if (mediaTag.Value == eSocialPlatform.InApp.ToString())
                {
                    cacheKey = string.Format("social_feed_platform={0}_assetId={1}_assetType={2}_userId={3}_groupId={4}", socialPlatform, assetId, assetType, userId, groupId);
                }
                else
                {
                    cacheKey = string.Format("social_feed_platform={0}_tagValue={1}_userId={2}_groupId={3}", socialPlatform, mediaTag.Value, userId, groupId);
                   
                }

                CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.CACHE);
                socialPlatformFeed = cbManager.Get<List<Core.Social.SocialFeed.SocialFeedItem>>(cacheKey);

                if (socialPlatformFeed == null)
                {
                    switch (socialPlatform)
                    {
                        case eSocialPlatform.Facebook:

                            if (userData.m_RespStatus == ResponseStatus.OK && !string.IsNullOrEmpty(userData.m_user.m_oBasicData.m_sFacebookToken))
                            {
                                try
                                {
                                    socialPlatformFeed = SocialFeedUtils.Facebook.GetFacebookSocialFeed(mediaTag.Value, TVinciShared.WS_Utils.GetTcmIntValue("SocialFeed_FB_item_count"),
                                        Core.Social.Utils.Decrypt(userData.m_user.m_oBasicData.m_sFacebookToken, Core.Social.Utils.GetValFromConfig("FB_TOKEN_KEY")));
                                }
                                catch (Exception)
                                {
                                    //Fallback to app token in case user token is invalid 
                                    socialPlatformFeed = SocialFeedUtils.Facebook.GetFacebookSocialFeed(mediaTag.Value, TVinciShared.WS_Utils.GetTcmIntValue("SocialFeed_FB_item_count"),
                                        SocialFeedUtils.Facebook.GetFacebookAppAccessToken(groupId));
                                }
                            }
                            // User wasn't found or has no facebook access_token
                            else
                            {
                                socialPlatformFeed = SocialFeedUtils.Facebook.GetFacebookSocialFeed(mediaTag.Value, TVinciShared.WS_Utils.GetTcmIntValue("SocialFeed_FB_item_count"),
                                    SocialFeedUtils.Facebook.GetFacebookAppAccessToken(groupId));
                            }
                            break;

                        case eSocialPlatform.InApp:
                            socialPlatformFeed = SocialFeedUtils.InApp.GetInAppSocialFeed(assetId, assetType, groupId, TVinciShared.WS_Utils.GetTcmIntValue("SocialFeed_InApp_item_count"));
                            break;

                        case eSocialPlatform.Twitter:
                            if (userData.m_RespStatus == ResponseStatus.OK && !string.IsNullOrEmpty(userData.m_user.m_oBasicData.m_sTwitterToken) && !string.IsNullOrEmpty(userData.m_user.m_oBasicData.m_sTwitterTokenSecret))
                            {
                                try
                                {
                                    socialPlatformFeed = SocialFeedUtils.Twitter.GetTwitterUserSocialFeed(mediaTag.Value, TVinciShared.WS_Utils.GetTcmConfigValue("TWITTER_CONSUMER_KEY"), TVinciShared.WS_Utils.GetTcmConfigValue("TWITTER_CONSUMER_SECRET"), userData.m_user.m_oBasicData.m_sTwitterToken, userData.m_user.m_oBasicData.m_sTwitterTokenSecret, TVinciShared.WS_Utils.GetTcmIntValue("SocialFeed_Twitter_item_count"));
                                }
                                catch (Exception)
                                {
                                    socialPlatformFeed = SocialFeedUtils.Twitter.GetTwitterAppSocialFeed(mediaTag.Value, TVinciShared.WS_Utils.GetTcmIntValue("SocialFeed_Twitter_item_count"), groupId);
                                }
                            }
                            else
                                // User wasn't found or has no twitter access_token
                                socialPlatformFeed = SocialFeedUtils.Twitter.GetTwitterAppSocialFeed(mediaTag.Value, TVinciShared.WS_Utils.GetTcmIntValue("SocialFeed_Twitter_item_count"), groupId);
                            break;
                    }

                    // add to cache

                    cbManager.Set(cacheKey, socialPlatformFeed, (uint)WS_Utils.GetTcmIntValue(string.Format(SOCIAL_FEED_PLATFORM_TTL_KEY_FORMAT, socialPlatform)));
                }

                if (socialPlatformFeed != null)
                {
                    // filter create date
                    socialPlatformFeed = socialPlatformFeed.Where(x => x.CreateDate > createDateSince).ToList();

                    // paging/indexing
                    response.TotalCount = socialPlatformFeed.Count;
                    response.SocialFeeds = GetPagedSocialFeed(socialPlatformFeed, pageSize, pageIndex, orderBy, socialPlatform);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in GetSocialFeed - Platform: {0} , q = {1} , group = {2} , Msg: {3} , Stacktrace: {4}",
                    socialPlatform, mediaTag != null ? mediaTag.Value : string.Empty, groupId, ex.Message, ex.StackTrace), ex);
            }

            return response;
        }

        private static List<SocialFeedItem> GetPagedSocialFeed(List<SocialFeedItem> socialPlatformFeed, int pageSize, int pageIndex, SocialFeedOrderBy orderBy, eSocialPlatform socialPlatform)
        {
            int totalItems = socialPlatformFeed.Count;
            int startIndexOnList = pageIndex * pageSize;
            int rangeToGetFromList = (startIndexOnList + pageSize) > totalItems ? (totalItems - startIndexOnList) > 0 ? (totalItems - startIndexOnList) : 0 : pageSize;
            if (rangeToGetFromList > 0)
            {
                switch (orderBy)
                {
                    case SocialFeedOrderBy.CreateDateAsc:
                        socialPlatformFeed.OrderBy(sf => sf.CreateDate);
                        break;
                    case SocialFeedOrderBy.CreateDateDesc:
                        socialPlatformFeed.OrderByDescending(sf => sf.CreateDate);
                        break;
                }

                socialPlatformFeed = socialPlatformFeed.GetRange(startIndexOnList, rangeToGetFromList);
                socialPlatformFeed.ForEach(sf => sf.SocialPlatform = socialPlatform);
            }
            else
            {
                socialPlatformFeed.Clear();
            }

            return socialPlatformFeed;
        }

    }
}