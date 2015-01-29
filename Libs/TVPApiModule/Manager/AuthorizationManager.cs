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
using TVPPro.SiteManager.Helper;

namespace TVPApiModule.Manager
{
    public class AuthorizationManager
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(AuthorizationManager));
        
        private static long deviceTokenExpirationSeconds;
        private static long accessTokenExpirationSeconds;
        private static long refreshTokenExpirationSeconds;

        private static string _key { get; set; }
        private static string _iv { get; set; }
         
        private static GenericCouchbaseClient _client;

        private static ReaderWriterLockSlim _lock;
        private Dictionary<string, AppCredentials> _appsCredentials;
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
            try
            {
                string deviceTokenExpiration = ConfigurationManager.AppSettings["Authorization.DeviceTokenExpirationSeconds"];
                string accessTokenExpiration = ConfigurationManager.AppSettings["Authorization.AccessTokenExpirationSeconds"];
                string refreshTokenExpiration = ConfigurationManager.AppSettings["Authorization.RefreshTokenExpirationSeconds"];

                _key = ConfigurationManager.AppSettings["Authorization.key"];
                _iv = ConfigurationManager.AppSettings["Authorization.iv"];

                _client = CouchbaseWrapper.CouchbaseManager.GetInstance("authorization");

                _lock = new ReaderWriterLockSlim();
                _appsCredentials = new Dictionary<string, AppCredentials>();

                if (!long.TryParse(deviceTokenExpiration, out deviceTokenExpirationSeconds) ||
                    !long.TryParse(accessTokenExpiration, out accessTokenExpirationSeconds) ||
                    !long.TryParse(refreshTokenExpiration, out refreshTokenExpirationSeconds) ||
                    string.IsNullOrEmpty(_key) || string.IsNullOrEmpty(_iv))
                {
                    logger.ErrorFormat("AuthorizationManager: Configuration for authorization is missing!");
                    throw new Exception("Configuration for authorization is missing!");
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("AuthorizationManager: Configuration for authorization is missing! Error: {0}", ex);
                throw ex;
            }
        }

        public AppCredentials GetAppCredentials(string appId)
        {
            AppCredentials appCredentials = null;

            // try get app credentials from dictionary
            if (_lock.TryEnterReadLock(1000))
            {
                try
                {
                    _appsCredentials.TryGetValue(appId, out appCredentials);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("GetAppCredentials: on extracting from dictionary with appId = {0}, Exception = {1}", appId, ex);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }

            // if not exists in dictionary, try get from CB 
            if (appCredentials == null)
            {
                string appCredentialsId = AppCredentials.GetAppCredentialsId(EncryptData(appId));
                appCredentials = _client.Get<AppCredentials>(appCredentialsId);
                if (appCredentials != null)
                {
                    // add app credentials to dictionary if not exists
                    if (_lock.TryEnterWriteLock(1000))
                    {
                        try
                        {
                            if (!_appsCredentials.Keys.Contains(appId))
                            {
                                _appsCredentials.Add(appId, appCredentials);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.ErrorFormat("GetAppCredentials: on adding to dictionary with appId = {0}, Exception = {1}", appId, ex);
                        }
                        finally
                        {
                            _lock.ExitWriteLock();
                        }
                    }
                }
                // no app credentials in CB
                else
                {
                    logger.ErrorFormat("GetAppCredentials: app credentials not exist for appId = {0}", appId);
                }
            }

            return appCredentials;
        }

        //Maybe should be deleted later
        public AppCredentials GenerateAppCredentials(int groupId)
        {
            new AuthorizationManager();
            AppCredentials appCredentials = null;
            appCredentials = new AppCredentials(groupId);
            _client.Store<AppCredentials>(appCredentials);
            appCredentials.EncryptedAppId = DecryptData(appCredentials.EncryptedAppId);
            appCredentials.EncryptedAppSecret = DecryptData(appCredentials.EncryptedAppSecret);
            return appCredentials;
        }

        

        public string GenerateDeviceToken(string udid, string appId)
        {
            // validate request parameters
            if (string.IsNullOrEmpty(udid) || string.IsNullOrEmpty(appId))
            {
                logger.ErrorFormat("GenerateDeviceToken: bad request. app_id = {0}, udid = {2}", appId, udid);
                returnError(403);
                return null;
            }

            // validate app credentials
            AppCredentials appCredentials = Instance.GetAppCredentials(appId);
            if (appCredentials == null)
            {
                logger.ErrorFormat("GenerateDeviceToken: appId not found = {0}", appId);
                returnError(403);
                return null;
            }

            // generate device token
            DeviceToken deviceToken = new DeviceToken(appCredentials.EncryptedAppId, udid);
            _client.Store<DeviceToken>(deviceToken, DateTime.UtcNow.AddSeconds(deviceTokenExpirationSeconds));
            return deviceToken.Token;        
        }

        public object ExchangeDeviceToken(string udid, string appId, string appSecret, string deviceToken)
        {
            // validate request parameters
            if (string.IsNullOrEmpty(udid) || string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret) || string.IsNullOrEmpty(deviceToken))
            {
                logger.ErrorFormat("ExchangeDeviceToken: Bad request udid = {0}, appId = {1}, appSecret = {2}, deviceToken = {3}", udid, appId, appSecret, deviceToken);
                returnError(403);
                return null;
            }

             // validate app credentials
            AppCredentials appCredentials = Instance.GetAppCredentials(appId);
            if (appCredentials == null || DecryptData(appCredentials.EncryptedAppSecret) != appSecret)
            {
                logger.ErrorFormat("ExchangeDeviceToken: app credentials not found or do not match for appId = {0}", appId);
                returnError(403);
                return null;
            }

            // validate device token
            string deviceTokenId = DeviceToken.GetDeviceTokenId(appCredentials.EncryptedAppId, deviceToken);
            DeviceToken deviceTokenObj = _client.Get<DeviceToken>(deviceTokenId);
            if (deviceTokenObj == null || deviceTokenObj.UDID != udid)
            {
                logger.ErrorFormat("ExchangeDeviceToken: device token not valid or expired. deviceToken = {0}, udid = {1}, appId = {2}", deviceToken, udid, appId);
                returnError(403);
                return null;
            }

            // generate access token and refresh token pair
            APIToken apiToken = new APIToken(appCredentials.EncryptedAppId, appCredentials.GroupId, udid);
            _client.Store<APIToken>(apiToken, DateTime.UtcNow.AddSeconds(refreshTokenExpirationSeconds));
            _client.Remove(deviceTokenId);

            return GetTokenResponseObject(apiToken);
        }

        public object RefreshAccessToken(string appId, string appSecret, string refreshToken, string accessToken)
        {
            // validate request parameters
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret) || string.IsNullOrEmpty(refreshToken))
            {
                logger.ErrorFormat("RefreshAccessToken: Bad request appId = {0}, appSecret = {1}, refreshToken = {2}", appId, appSecret, refreshToken);
                returnError(403);
                return null;
            }

            // validate app credentials
            AppCredentials appCredentials = Instance.GetAppCredentials(appId);
            if (appCredentials == null || DecryptData(appCredentials.EncryptedAppSecret) != appSecret)
            {
                logger.ErrorFormat("RefreshAccessToken: app credentials not found or do not match for appId = {0}", appId);
                returnError(403);
                return null;
            }

            // try get api token from CB
            string apiTokenId = APIToken.GetAPITokenId(accessToken);
            CasGetResult<APIToken> casRes = _client.GetWithCas<APIToken>(apiTokenId);
            if (casRes == null || casRes.OperationResult != eOperationResult.NoError || casRes.Value == null)
            {
                logger.ErrorFormat("RefreshAccessToken: refreshToken expired appId = {0}, accessToken = {1}, refreshToken = {2}", appId, accessToken, refreshToken);
                returnError(403);
                return null;
            }

            APIToken apiToken = casRes.Value;

            // validate refresh token
            if (apiToken.RefreshToken != refreshToken)
            {
                logger.ErrorFormat("RefreshAccessToken: refreshToken not valid appId = {0}, accessToken = {1}, refreshToken = {2}", appId, accessToken, refreshToken);
                returnError(403);
                return null;
            }

            // generate new access token and refresh token pair
            apiToken = new APIToken(appCredentials.EncryptedAppId, appCredentials.GroupId, apiToken.UDID);

            if (!_client.Cas<APIToken>(apiToken, DateTime.UtcNow.AddSeconds(refreshTokenExpirationSeconds), casRes.DocVersion))
            {
                // if already refreshed, return it
                apiToken = _client.Get<APIToken>(apiTokenId);
            }

            return GetTokenResponseObject(apiToken);
        }

        public bool IsAccessTokenValid(string accessToken)
        {
            string apiTokenId = APIToken.GetAPITokenId(accessToken);

            APIToken apiToken = _client.Get<APIToken>(apiTokenId);
            if (apiToken == null)
            {
                logger.ErrorFormat("ValidateAccessToken: access token not found. access_token = {0}", accessToken);
                returnError(403);
                return false;
            }
            
            // access token expired 
            if (TimeHelper.ConvertFromUnixTimestamp(apiToken.CreateDate).AddSeconds(accessTokenExpirationSeconds) < DateTime.UtcNow)
            {
                logger.ErrorFormat("ValidateAccessToken: access token expired. access_token = {0}", accessToken);
                returnError(401);
                return false;
            }

            return true;
        }

        private void returnError(int statusCode, string description = null)
        {
            HttpContext.Current.Items["StatusCode"] = statusCode;
            if (!string.IsNullOrEmpty(description))
            {
                HttpContext.Current.Items["StatusDescription"] = description;
            }
        }

        private object GetTokenResponseObject(APIToken apiToken)
        {
            var expirationInSeconds = accessTokenExpirationSeconds;
            if (apiToken == null)
                return null;

            return new 
            { 
                access_token = apiToken.AccessToken, 
                refresh_token = apiToken.RefreshToken,
                expiration_time = apiToken.CreateDate + expirationInSeconds
            };
        }

        public string EncryptData(string data)
        {
            if (data == null)
                return null;
            return SecurityHelper.EncryptData(_key, _iv, data);
        }

        public string DecryptData(string data)
        {
            if (data == null)
                return null;
            return SecurityHelper.DecryptData(_key, _iv, data);
        }


        
    }
}
