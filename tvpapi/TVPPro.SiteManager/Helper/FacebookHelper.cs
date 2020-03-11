using ApiObjects;
using Core.Social;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using TVPPro.SiteManager.Services;

namespace TVPPro.SiteManager.Helper
{
    public class FacebookHelper
    {
        class FacebookUserCachedData
        {
            public string userID;
            public int domainID;
            public string deviceDNA;
            public string _sToken;

            public Dictionary<string, object> config;
            public Dictionary<string, object> userData;
            public Dictionary<USER_ACTIONS, object> userActions = new Dictionary<USER_ACTIONS, object>();
        }

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

        public static bool IsFBConfigured
        {
            get
            {
                return SessionHelper.IsValueInSession(SessionHelper.SessionKeys.FacebookUserConfiguration);
            }
        }

        public static Dictionary<USER_ACTIONS, object> userActions
        {
            get
            {
                if (IsFBConfigured)
                {
                    var fbConfig = getFacebookUserConfigurations();
                    return fbConfig.userActions;
                }
                else
                {
                    return new Dictionary<USER_ACTIONS, object>();
                }
            }
        }

        public static bool useTVPAPI = true;

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
                TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.SetUserSocialPrivacy, JsonConvert.SerializeObject(postData));
            }
        }

        public static string SetUserExternalActionShare(USER_ACTIONS UserAction, ACTION_PRIVACY ActionPrivacy)
        {
            string response = string.Empty;

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
                response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.SetUserExternalActionShare, JsonConvert.SerializeObject(postData));
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
            string response = string.Empty;

            var postData = new
            {
                userAction = UserAction.ToString(),
                initObj = getInitObj(),
                socialPlatform = SOCIAL_PLATFORMS.FACEBOOK
            };

            // make the request
            if (useTVPAPI)
            {
                response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.GetUserExternalActionShare, JsonConvert.SerializeObject(postData));
            }

            return response;
        }

        public static string DoUserAction(string action, int assetID = 0, string extraParams = null)
        {
            return DoUserAction(action, assetID, extraParams, null);
        }

        public static string DoUserAction(string action, int assetID = 0, string extraParams = null, string assetType = null)
        {
            string response = string.Empty;

            USER_ACTIONS _action = (USER_ACTIONS)Enum.Parse(typeof(USER_ACTIONS), action, true);
            USER_ACTIONS actionEnum = (USER_ACTIONS)Enum.Parse(typeof(USER_ACTIONS), action, true);
            string socialPlatform = SOCIAL_PLATFORMS.FACEBOOK.ToString();
            string assetTypeName = (string.IsNullOrEmpty(assetType)) ? ASSET_TYPES.MEDIA.ToString() : ((ASSET_TYPES)Enum.Parse(typeof(ASSET_TYPES), assetType, true)).ToString();
            if (IsFBConfigured)
            {
                var fbConfig = getFacebookUserConfigurations();
                if (useTVPAPI)
                {
                    // prepare the postData
                    var postData = new
                    {
                        userAction = _action.ToString(),
                        initObj = getInitObj(),
                        extraParams = JsonConvert.DeserializeObject(extraParams),
                        socialPlatform = SOCIAL_PLATFORMS.FACEBOOK.ToString(),
                        assetType = assetTypeName,
                        assetID = assetID
                    };
                    // make the request
                    response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.DoUserAction, JsonConvert.SerializeObject(postData));
                }
                else
                {
                    eUserAction userAction = (eUserAction)Enum.Parse(typeof(eUserAction), _action.ToString());
                    response = SocialService.Instance.DoSocialAction(assetID, fbConfig.userID, userAction, SocialPlatform.FACEBOOK, string.Empty);
                }
            }
            // return the response
            return response;
        }

        public static void SetFBConfig(string UserID, int DomainID, string DeviceDNA, bool UseTVPAPI)
        {
            FacebookUserCachedData currentUserFBConfig = null;
            useTVPAPI = UseTVPAPI;
            // save props
            if (!SessionHelper.IsValueInSession(SessionHelper.SessionKeys.FacebookUserConfiguration))
            {
                if (!string.IsNullOrEmpty(UserID) && !string.IsNullOrEmpty(DeviceDNA))
                {
                    currentUserFBConfig = new FacebookUserCachedData()
                    {
                        userID = UserID,
                        domainID = DomainID,
                        deviceDNA = DeviceDNA,

                    };
                    SessionHelper.SetValueInSession(SessionHelper.SessionKeys.FacebookUserConfiguration, currentUserFBConfig);
                }
            }
        }

        public static void SetFBConfig(string UserID, int DomainID, string DeviceDNA)
        {
            SetFBConfig(UserID, DomainID, DeviceDNA, false);
        }

        public static string GetFBConfig()
        {
            string response = string.Empty;

            if (IsFBConfigured)
            {
                var fbConfig = getFacebookUserConfigurations();
                if (useTVPAPI)
                {
                    // prepare the postData
                    var postData = new
                    {
                        sSTG = 1,
                        initObj = getInitObj(),
                    };

                    // make the request
                    response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.FBConfig, JsonConvert.SerializeObject(postData));

                    //deserialize the response
                    var deserializedResponse = JsonConvert.DeserializeObject(response);
                    fbConfig.config = deserializedResponse as Dictionary<string, object>;
                }
                else
                {
                    // make the request
                    FacebookConfig fbConfigResponse = SocialService.Instance.getFBConfig();

                    if (fbConfig.config == null)
                    {
                        fbConfig.config = new Dictionary<string, object>();
                        fbConfig.config.Add("appId", fbConfigResponse.sFBKey);
                        fbConfig.config.Add("scope", fbConfigResponse.sFBPermissions);
                    }

                    // return the response
                    response = JsonConvert.SerializeObject(fbConfig.config);
                }
            }
            // return the response
            return response;
        }

        public static string getFBUserData(string sToken)
        {
            string response = string.Empty;

            //SetUserSocialPrivacy();
            //SetUserExternalActionShare();
            if (IsFBConfigured)
            {
                var fbConfig = getFacebookUserConfigurations();
                fbConfig._sToken = sToken;
                if (useTVPAPI)
                {
                    // prepare the postData
                    var postData = new
                    {
                        sSTG = 1,
                        sToken = fbConfig._sToken,
                        initObj = getInitObj()
                    };

                    // make the request
                    response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.GetFBUserData, JsonConvert.SerializeObject(postData));

                    //deserialize the response
                    var deserializedResponse = JsonConvert.DeserializeObject(response);
                    fbConfig.userData = deserializedResponse as Dictionary<string, object>;
                }
                else
                {
                    // make the request
                    FacebookResponseObject FbResponse = SocialService.Instance.getFBUserData(fbConfig._sToken);

                    fbConfig.userData = new Dictionary<string, object>();
                    fbConfig.userData.Add("data", FbResponse.data);
                    fbConfig.userData.Add("facebookName", FbResponse.facebookName);
                    fbConfig.userData.Add("fbUser", FbResponse.fbUser);
                    fbConfig.userData.Add("minFriends", FbResponse.minFriends);
                    fbConfig.userData.Add("pic", FbResponse.pic);
                    fbConfig.userData.Add("siteGuid", FbResponse.siteGuid);
                    fbConfig.userData.Add("status", FbResponse.status);
                    fbConfig.userData.Add("token", fbConfig._sToken);
                    fbConfig.userData.Add("tvinciName", FbResponse.tvinciName);

                    // return the response
                    response = JsonConvert.SerializeObject(fbConfig.userData);
                }
            }
            // return the response
            return response;
        }

        public static string GetFBUserAction(string action, int assetId, string assetTypeName)
        {
            USER_ACTIONS actionValue = (USER_ACTIONS)Enum.Parse(typeof(USER_ACTIONS), action, true);
            ASSET_TYPES assetType = (string.IsNullOrEmpty(assetTypeName)) ? ASSET_TYPES.MEDIA : ((ASSET_TYPES)Enum.Parse(typeof(ASSET_TYPES), assetTypeName, true));

            return GetFBUserAction(actionValue, assetId, assetType);
        }


        public static string GetFBUserAction(USER_ACTIONS action, int assetID, ASSET_TYPES assetType = ASSET_TYPES.UNKNOWN)
        {
            string response = string.Empty;
            // prepare the postData
            var postData = new
            {
                userAction = action.ToString(),
                initObj = getInitObj(),
                socialPlatform = SOCIAL_PLATFORMS.FACEBOOK.ToString(),
                assetType = assetType.ToString(),
                assetID = assetID,
                numOfRecords = 0,
                startIndex = 0
            };

            if (!userActions.ContainsKey(action))
            {
                // make the request
                response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.GetUserActions, JsonConvert.SerializeObject(postData));

                //deserialize the response
                var deserializedResponse = JsonConvert.DeserializeObject(response);

                userActions.Add(action, deserializedResponse);
            }
            else
            {
                // make the request
                response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.GetUserActions, JsonConvert.SerializeObject(postData));

                //deserialize the response
                var deserializedResponse = JsonConvert.DeserializeObject(response);
                userActions[action] = deserializedResponse;
            }

            // return the response
            return response;
        }

        public static string FBTokenValidation(string sToken)
        {
            string response = string.Empty;
            var postData = new
            {
                initObj = getInitObj(),
                token = sToken
            };
            // make the request
            response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.FBTokenValidation, JsonConvert.SerializeObject(postData));

            return response;
        }

        private static object getInitObj()
        {
            if (IsFBConfigured)
            {
                var fbConfig = getFacebookUserConfigurations();
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
                            SiteGuid = fbConfig.userID,
                            DomainID = fbConfig.domainID,
                            UDID = fbConfig.deviceDNA,
                            ApiUser = "tvpapi_153",
                            ApiPass = "11111"
                        };
            }
            return null;
        }

        public static string FBUserMerge(string username, string password)
        {
            string response = string.Empty;

            if (IsFBConfigured)
            {
                var fbconfig = getFacebookUserConfigurations();
                string FBID = (string)((Dictionary<string, object>)fbconfig.userData["fbUser"])["uid"];
                if (useTVPAPI)
                {
                    // prepare the postData
                    var postData = new
                    {
                        sFBID = FBID,
                        sUsername = username,
                        sPassword = password,
                        sToken = fbconfig._sToken,
                        initObj = getInitObj()
                    };

                    // make the request
                    response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.FBUserMerge, JsonConvert.SerializeObject(postData));
                }
                else
                {
                    FacebookResponseObject fbResponse = SocialService.Instance.FBUserMerge
                        (fbconfig._sToken, FBID, username, password);

                    // return the response
                    response = JsonConvert.SerializeObject(fbResponse);
                }

                if (!string.IsNullOrEmpty(response))
                {
                    //deserialize the response
                    var deserializedResponse = JsonConvert.DeserializeObject(response);
                    var userData = deserializedResponse as Dictionary<string, object>;
                    if (userData.ContainsKey("status") && userData["status"].ToString() == "MERGEOK")
                    {
                        fbconfig.userData = userData;
                    }
                }
            }
            return response;
        }

        private static FacebookUserCachedData getFacebookUserConfigurations()
        {
            FacebookUserCachedData fbConfig = null;
            SessionHelper.GetValueFromSession<FacebookUserCachedData>(SessionHelper.SessionKeys.FacebookUserConfiguration, out fbConfig);

            return fbConfig;
        }
    }


}