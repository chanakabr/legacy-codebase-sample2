using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using TVPPro.SiteManager.Services;

namespace TVPPro.SiteManager.Helper
{
    public class FacebookHelper
    {


        public static bool IsFBConfigured = false;

        static bool useTVPAPI = true;
        static string userID;
        static int domainID;
        static string deviceDNA;
        static string _sToken;

        static Dictionary<string, object> config;
        static Dictionary<string, object> userData;
        public static Dictionary<USER_ACTIONS, object> userActions = new Dictionary<USER_ACTIONS, object>();
        public enum SOCIAL_PLATFORMS
        {
            UNKNOWN,
            FACEBOOK,
            GOOGLE
        };

        public enum ASSET_TYPES
        {
            UNKNOWN = 0,
            MEDIA = 1,
            PROGRAM = 2
        };

        public enum USER_ACTIONS
        {
            UNKNOWN,
            LIKE,
            UNLIKE,
            SHARE,
            POST,
            WATCHES,
            WANT_TO_WATCH,
            RATES
        };

        public enum SOCIAL_PRIVACY
        {
            UNKNOWN,
            EVERYONE,
            ALL_FRIENDS,
            FRIENDS_OF_FRIENDS,
            SELF,
            CUSTOM
        }

        public enum ACTION_PRIVACY
        {
            UNKNOWN,
            ALLOW,
            DONT_ALLOW
        }



        private static void SetUserSocialPrivacy()
        {
            // prepare the postData
            var postData = new
            {
                userAction = USER_ACTIONS.LIKE,
                initObj = getInitObj(),
                socialPlatform = SOCIAL_PLATFORMS.FACEBOOK,
                socialPrivacy = SOCIAL_PRIVACY.EVERYONE
            };

            // make the request
            if (useTVPAPI)
            {
                var response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.SetUserSocialPrivacy, new JavaScriptSerializer().Serialize(postData));
            }
            else
            {

            }
        }

        public static string SetUserExternalActionShare(USER_ACTIONS UserAction, ACTION_PRIVACY ActionPrivacy)
        {
            var response = "";
            // prepare the postData
            userID = UsersService.Instance.GetUserID();
            domainID = UsersService.Instance.GetDomainID();
            deviceDNA = DeviceDNAHelper.GetDeviceDNA();

            var postData = new
            {
                userAction = UserAction.ToString(),
                initObj = getInitObj(),
                socialPlatform = SOCIAL_PLATFORMS.FACEBOOK,
                actionPrivacy = ActionPrivacy
            };

            // make the request
            if (useTVPAPI)
            {
                response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.SetUserExternalActionShare, new JavaScriptSerializer().Serialize(postData));
            }
            return response;
        }

        // allow for "like"
        private static void SetUserExternalActionShare()
        {
            SetUserExternalActionShare(USER_ACTIONS.LIKE, ACTION_PRIVACY.ALLOW);
        }

        public static string GetUserExternalActionShare(USER_ACTIONS UserAction)
        {
            var response = "";
            // prepare the postData
            userID = UsersService.Instance.GetUserID();
            domainID = UsersService.Instance.GetDomainID();
            deviceDNA = DeviceDNAHelper.GetDeviceDNA();

            var postData = new
            {
                userAction = UserAction.ToString(),
                initObj = getInitObj(),
                socialPlatform = SOCIAL_PLATFORMS.FACEBOOK
            };

            // make the request
            if (useTVPAPI)
            {
                response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.GetUserExternalActionShare, new JavaScriptSerializer().Serialize(postData));
            }
            return response;
        }


        public static string DoUserAction(string action, int assetID = 0, string extraParams = null)
        {
            USER_ACTIONS _action = (USER_ACTIONS)Enum.Parse(typeof(USER_ACTIONS), action, true);

            var response = string.Empty;

            if (useTVPAPI)
            {
                // prepare the postData
                var postData = new
                {
                    userAction = _action.ToString(),
                    initObj = getInitObj(),
                    extraParams = new JavaScriptSerializer().DeserializeObject(extraParams),
                    socialPlatform = SOCIAL_PLATFORMS.FACEBOOK.ToString(),
                    assetType = ASSET_TYPES.MEDIA.ToString(),
                    assetID = assetID
                };
                // make the request
                response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.DoUserAction, new JavaScriptSerializer().Serialize(postData));
            }
            else
            {
                TvinciPlatform.Social.eUserAction userAction = (TvinciPlatform.Social.eUserAction)Enum.Parse(typeof(TvinciPlatform.Social.eUserAction), _action.ToString());
                response = SocialService.Instance.DoSocialAction(assetID, userID, userAction, TvinciPlatform.Social.SocialPlatform.FACEBOOK, string.Empty);
            }


            // return the response
            return response;
        }

        public static void SetFBConfig(string UserID, int DomainID, string DeviceDNA, bool UseTVPAPI)
        {
            // save props
            userID = UserID;
            domainID = DomainID;
            deviceDNA = DeviceDNA;
            useTVPAPI = UseTVPAPI;

            if (!string.IsNullOrEmpty(userID) && domainID != null && !string.IsNullOrEmpty(deviceDNA))
            {
                IsFBConfigured = true;
            }
        }

        public static void SetFBConfig(string UserID, int DomainID, string DeviceDNA)
        {
            SetFBConfig(UserID, DomainID, DeviceDNA, false);
        }

        public static string GetFBConfig()
        {
            if (useTVPAPI)
            {
                // prepare the postData
                var postData = new
                {
                    sSTG = 1,
                    initObj = getInitObj(),
                };

                // make the request
                var response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.FBConfig, new JavaScriptSerializer().Serialize(postData));

                //deserialize the response
                var deserializedResponse = new JavaScriptSerializer().DeserializeObject(response);
                config = deserializedResponse as Dictionary<string, object>;

                // return the response
                return response;
            }
            else
            {
                // make the request
                TvinciPlatform.Social.FacebookConfig response = SocialService.Instance.getFBConfig();

                if (config == null)
                {
                    config = new Dictionary<string, object>();
                    config.Add("appId", response.sFBKey);
                    config.Add("scope", response.sFBPermissions);
                }

                // return the response
                return new JavaScriptSerializer().Serialize(config);
            }
        }

        public static string getFBUserData(string sToken)
        {
            SetUserSocialPrivacy();
            SetUserExternalActionShare();

            _sToken = sToken;
            if (useTVPAPI)
            {
                // prepare the postData
                var postData = new
                {
                    sSTG = 1,
                    sToken = _sToken,
                    initObj = getInitObj()
                };

                // make the request
                var response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.GetFBUserData, new JavaScriptSerializer().Serialize(postData));

                //deserialize the response
                var deserializedResponse = new JavaScriptSerializer().DeserializeObject(response);
                userData = deserializedResponse as Dictionary<string, object>;

                // return the response
                return response;

            }
            else
            {
                // make the request
                TvinciPlatform.Social.FacebookResponseObject response = SocialService.Instance.getFBUserData(_sToken);

                userData = new Dictionary<string, object>();
                userData.Add("data", response.data);
                userData.Add("facebookName", response.facebookName);
                userData.Add("fbUser", response.fbUser);
                userData.Add("minFriends", response.minFriends);
                userData.Add("pic", response.pic);
                userData.Add("siteGuid", response.siteGuid);
                userData.Add("status", response.status);
                userData.Add("token", _sToken);
                userData.Add("tvinciName", response.tvinciName);

                // return the response
                return new JavaScriptSerializer().Serialize(userData);
            }
        }

        public static string getFBUserAction(USER_ACTIONS action, int assetID, ASSET_TYPES assetType = ASSET_TYPES.UNKNOWN)
        {
            var response = "";
            // prepare the postData
            var postData = new
            {
                userAction = action.ToString(),
                initObj = getInitObj(),
                socialPlatform = SOCIAL_PLATFORMS.FACEBOOK,
                assetType = assetType,
                assetID = 0,
                numOfRecords = 0,
                startIndex = 0
            };

            if (!userActions.ContainsKey(USER_ACTIONS.LIKE))
            {
                // make the request
                response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.GetUserActions, new JavaScriptSerializer().Serialize(postData));

                //deserialize the response
                var deserializedResponse = new JavaScriptSerializer().DeserializeObject(response);

                userActions.Add(action, deserializedResponse);
            }
            else
            {

                // make the request
                response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.GetUserActions, new JavaScriptSerializer().Serialize(postData));

                //deserialize the response
                var deserializedResponse = new JavaScriptSerializer().DeserializeObject(response);
                userActions[action] = deserializedResponse;
            }


            // return the response
            return response;
        }

        private static object getInitObj()
        {
            return new
                    {
                        Locale = new
                        {
                            LocaleLanguage = "",
                            LocaleCountry = "",
                            LocaleDevice = "",
                            LocaleUserState = "Unknown"
                        },
                        Platform = "iPad",
                        SiteGuid = userID,
                        DomainID = domainID,
                        UDID = deviceDNA,
                        ApiUser = "tvpapi_153",
                        ApiPass = "11111"
                    };
        }

        public static string FBUserMerge(string username, string password)
        {
            string FBID = (string)((Dictionary<string, object>)userData["fbUser"])["uid"];
            if (useTVPAPI)
            {
                // prepare the postData
                var postData = new
                {
                    sFBID = FBID,
                    sUsername = username,
                    sPassword = password,
                    sToken = _sToken,
                    initObj = getInitObj()
                };

                // make the request
                var response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.FBUserMerge, new JavaScriptSerializer().Serialize(postData));

                // return the response
                return response;
            }
            else
            {
                TvinciPlatform.Social.FacebookResponseObject response = SocialService.Instance.FBUserMerge
                    (_sToken, FBID, username, password);

                // return the response
                return new JavaScriptSerializer().Serialize(response);
            }
        }
    }
}