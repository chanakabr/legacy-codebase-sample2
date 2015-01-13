using CouchbaseWrapper;
using CouchbaseWrapper.DalEntities;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using TVPApiModule.Helper;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Authorization;

namespace TVPApiModule.Manager
{
    public class AuthorizationManager
    {
        private static int DEVICE_TOKEN_EXPIRATION_MINUTES = string.IsNullOrEmpty(ConfigurationManager.AppSettings["Authorization.DeviceTokenExpirationMinutes"]) ? 0 :int.Parse(ConfigurationManager.AppSettings["Authorization.DeviceTokenExpirationMinutes"]);
        private static int ACCESS_TOKEN_EXPIRATION_MINUTES = string.IsNullOrEmpty(ConfigurationManager.AppSettings["Authorization.AccessTokenExpirationMinutes"]) ? 0 : int.Parse(ConfigurationManager.AppSettings["Authorization.AccessTokenExpirationMinutes"]);
        private static int REFRESH_TOKEN_EXPIRATION_MINUTES = string.IsNullOrEmpty(ConfigurationManager.AppSettings["Authorization.RefreshTokenExpirationMinutes"]) ? 0 : int.Parse(ConfigurationManager.AppSettings["Authorization.RefreshTokenExpirationMinutes"]);

        private static ILog logger = log4net.LogManager.GetLogger(typeof(AuthorizationManager));

        private static GenericCouchbaseClient _client = CouchbaseWrapper.CouchbaseManager.GetInstance("authorization");
     
        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private Dictionary<int, AppCredentials> _appsCredentials = new Dictionary<int, AppCredentials>();
        private static AuthorizationManager _instance = null;

        public static AuthorizationManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AuthorizationManager();

                return _instance;
            }
        }

        private AuthorizationManager()
        {
        }

        public AppCredentials GetAppCredentials(int groupId)
        {
            AppCredentials appCredentials = null;

            // try get app credentials from dictionary
            if (_lock.TryEnterReadLock(1000))
            {
                try
                {
                    _appsCredentials.TryGetValue(groupId, out appCredentials);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("GetAppCredentials: on extracting from dictionary with groupId = {0}, Exception = {1}", groupId, ex);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }

            // if not exists in dictionary, try get from CB 
            if (appCredentials == null)
            {
                string appCredentialsId = GetAppCredentialsId(groupId);
                if (_client.Exists(appCredentialsId))
                {
                    appCredentials = _client.Get<AppCredentials>(appCredentialsId);

                    if (appCredentials != null)
                    {
                        // add app credentials to dictionary if not exists
                        if (_lock.TryEnterWriteLock(1000))
                        {
                            try
                            {
                                if (!_appsCredentials.Keys.Contains(groupId))
                                {
                                    _appsCredentials.Add(groupId, appCredentials);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.ErrorFormat("GetAppCredentials: on adding to dictionary with groupId = {0}, Exception = {1}", groupId, ex);
                            }
                            finally
                            {
                                _lock.ExitWriteLock();
                            }
                        }
                    }
                }
                // no app credentials in CB
                else
                {
                    logger.ErrorFormat("GetAppCredentials: app credentials not exist for groupId = {0}", groupId);
                }
            }

            return appCredentials;
        }

        //Maybe should be deleted later
        public static AppCredentials GenerateAppCredentials(int groupId)
        {
            AppCredentials appCredentials = null;
            appCredentials = _client.Get<AppCredentials>(GetAppCredentialsId(groupId));
            if (appCredentials != null)
            {
                appCredentials = new AppCredentials(groupId);
                _client.Store<AppCredentials>(appCredentials);
            }
            return appCredentials;
        }

        

        public static string GenerateDeviceToken(string udid, int groupId, string appId)
        {
            string token = null;
            if (!string.IsNullOrEmpty(udid))
            {
                // validate app credentials
                AppCredentials appCredentials = Instance.GetAppCredentials(groupId);
                if (appCredentials != null && appCredentials.AppID == appId)
                {
                    DeviceToken deviceToken = new DeviceToken(groupId, udid);
                    _client.Store<DeviceToken>(deviceToken, DateTime.UtcNow.AddMinutes(DEVICE_TOKEN_EXPIRATION_MINUTES));
                    token = deviceToken.Token;
                }
                else // app credentials not found or appId doesn't match
                {
                    logger.ErrorFormat("GenerateDeviceToken: appId doesn't match for groupId = {0}", groupId);
                    HttpContext.Current.Items.Add("Error", "invalid appId");
                    returnError(403);
                }
            }
            else
            {
                logger.ErrorFormat("GenerateDeviceToken: No UDID was supplied for groupId = {0}", groupId);
                HttpContext.Current.Items.Add("Error", "No UDID was supplied");
                returnError(403);
            }
            return token;
        }

        public static object ExchangeDeviceToken(string udid, int groupId, string appId, string appSecret, string deviceToken)
        {
            object token = null;
            if (!string.IsNullOrEmpty(deviceToken) || !string.IsNullOrEmpty(udid))
            {
                // validate app credentials
                AppCredentials appCredentials = Instance.GetAppCredentials(groupId);
                if (appCredentials != null && appCredentials.AppID == appId && appCredentials.AppSecret == appSecret)
                {
                    // validate device token
                    string deviceTokenId = GetDeviceTokenId(groupId, udid);
                    CasGetResult<DeviceToken> deviceTokenCasRes = _client.GetWithLock<DeviceToken>(deviceTokenId, TimeSpan.FromSeconds(5));
                    if (deviceTokenCasRes != null && deviceTokenCasRes.OperationResult == eOperationResult.NoError && deviceTokenCasRes.Value != null)
                    {
                        if (deviceTokenCasRes.Value.Token == deviceToken)
                        {
                            // generate access token and refresh token pair
                            APIToken apiToken = new APIToken(groupId, udid);
                            _client.Store<APIToken>(apiToken, DateTime.UtcNow.AddMinutes(REFRESH_TOKEN_EXPIRATION_MINUTES));
                            _client.Unlock(deviceTokenId, deviceTokenCasRes.DocVersion);
                            _client.Remove(deviceTokenId);
                            token = GetTokenResponseObject(apiToken);
                        }
                        else // device token doesn't match
                        {
                            logger.ErrorFormat("ExchangeDeviceToken: device token not valid for udid = {0}", udid);
                            returnError(403);
                        }
                    }
                    else // device token not found in CB
                    {
                        logger.ErrorFormat("ExchangeDeviceToken: device token not valid or expired for udid = {0}, groupId = {1}", udid, groupId);
                        returnError(403);
                    }
                }
                else // app credentials not found or don't match
                {
                    logger.ErrorFormat("ExchangeDeviceToken: app credentials do not exist or do not match for groupId = {0}", groupId);
                    returnError(403);
                }
            }
            else
            {
                logger.ErrorFormat("ExchangeDeviceToken: No UDID or device token was supplied for groupId = {0}", groupId);
                HttpContext.Current.Items.Add("Error", "No UDID or device token was supplied");
                returnError(403);
            }
            return token;
        }

        public static object RefreshAccessToken(string udid, int groupId, string appId, string appSecret, string refreshToken)
        {
            object token = null;
            if (!string.IsNullOrEmpty(refreshToken) || !string.IsNullOrEmpty(udid))
            {
                // validate device token
                AppCredentials appCredentials = Instance.GetAppCredentials(groupId);
                if (appCredentials != null && appCredentials.AppID == appId && appCredentials.AppSecret == appSecret)
                {
                    // get access token and refresh token pair
                    string apiTokenId = GetAPITokenId(groupId, udid);
                    CasGetResult<APIToken> casRes = _client.GetWithCas<APIToken>(apiTokenId);
                    if (casRes != null && casRes.OperationResult == eOperationResult.NoError && casRes.Value != null)
                    {
                        APIToken apiToken = casRes.Value;
                        // validate refresh token
                        if (apiToken != null && apiToken.RefreshToken == refreshToken)
                        {
                            // generate new access token and refresh token pair
                            apiToken = new APIToken(groupId, udid);

                            if (_client.Cas<APIToken>(apiToken, DateTime.UtcNow.AddMinutes(REFRESH_TOKEN_EXPIRATION_MINUTES), casRes.DocVersion))
                            {
                                token = GetTokenResponseObject(apiToken); 
                            }
                            else
                            {
                                apiToken = _client.Get<APIToken>(apiTokenId);
                                if (apiToken != null)
                                    token = GetTokenResponseObject(apiToken);
                            }
                        }
                        else // refresh token doesn't match
                        {
                            logger.ErrorFormat("RefreshAccessToken: refresh token is not valid for UDID = {0}", udid);
                            returnError(403);
                        }
                    }
                    else // refresh token not found in CB
                    {
                        logger.ErrorFormat("RefreshAccessToken: refresh token is expired for UDID = {0}", udid);
                        returnError(403);
                    }
                }
                else // app credentials not found or don't match
                {
                    logger.ErrorFormat("RefreshAccessToken: app credentials do not exist or do not match for groupId = {0}", groupId);
                    returnError(403);
                }
            }
            else
            {
                logger.ErrorFormat("RefreshAccessToken: No UDID or refresh token was supplied for groupId = {0}", groupId);
                HttpContext.Current.Items.Add("Error", "No UDID or refresh token was supplied");
                returnError(403);
            }

            return token;
        }



        public static bool ValidateAccessToken(int groupId, string udid, string accessToken)
        {
            bool valid = false;

            string apiTokenId = GetAPITokenId(groupId, udid);
            if (_client.Exists(apiTokenId))
            {
                APIToken apiToken = _client.Get<APIToken>(apiTokenId);
                if (apiToken != null)
                {
                    // valid access token - tokens match and not expired
                    if (apiToken.AccessToken == accessToken && TimeHelper.ConvertFromUnixTimestamp(apiToken.CreateDate).AddMinutes(ACCESS_TOKEN_EXPIRATION_MINUTES) >= DateTime.UtcNow)
                    {
                        valid = true;
                    }
                    // access token expired 
                    else if (apiToken.AccessToken == accessToken)
                    {
                        valid = false;
                        returnError(401);
                    }
                    else
                    {
                        valid = false;
                        returnError(403);
                    }
                }
            }
            else // access token was not found in CB
            {
                returnError(403);
            }
            return valid;
        }

        private static string GetAPITokenId(int groupId, string udid)
        {
            return string.Format("access_{0}_{1}", groupId, udid);
        }

        private static string GetAppCredentialsId(int groupId)
        {
            return string.Format("app_{0}", groupId);
        }

        private static string GetDeviceTokenId(int groupId, string udid)
        {
            return string.Format("device_{0}_{1}", groupId, udid);
        }

        private static void returnError(int statusCode, string description = null)
        {
            HttpContext.Current.Items["StatusCode"] = statusCode;
            if (!string.IsNullOrEmpty(description))
            {
                HttpContext.Current.Items["StatusDescription"] = description;
            }
        }

        private static object GetTokenResponseObject(APIToken apiToken)
        {
            var expirationInSeconds = ACCESS_TOKEN_EXPIRATION_MINUTES * 60;

            return new 
            { 
                access_token = apiToken.AccessToken, 
                refresh_token = apiToken.RefreshToken,
                expiration_time = apiToken.CreateDate + expirationInSeconds
            };
        }
    }
}
