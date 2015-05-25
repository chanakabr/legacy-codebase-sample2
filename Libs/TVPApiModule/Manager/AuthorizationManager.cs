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
using TVPApi;
using TVPApiModule.Helper;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Authorization;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.TvinciPlatform.Domains;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace TVPApiModule.Manager
{
    public class AuthorizationManager
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(AuthorizationManager));

        //private static long deviceTokenExpirationSeconds;
        //private static long accessTokenExpirationSeconds;
        //private static long refreshTokenExpirationSeconds;

        private static long _groupConfigsTtlSeconds;
        
        //private static string _key; 
        //private static string _iv; 

        private static GenericCouchbaseClient _client;

        private static ReaderWriterLockSlim _lock;
        //private Dictionary<string, AppCredentials> _appsCredentials;
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
                //string deviceTokenExpiration = ConfigurationManager.AppSettings["Authorization.DeviceTokenExpirationSeconds"];
                //string accessTokenExpiration = ConfigurationManager.AppSettings["Authorization.AccessTokenExpirationSeconds"];
                //string refreshTokenExpiration = ConfigurationManager.AppSettings["Authorization.RefreshTokenExpirationSeconds"];
                string groupConfigsTtlSeconds = ConfigurationManager.AppSettings["Authorization.GroupConfigsTtlSeconds"];

                //_key = ConfigurationManager.AppSettings["Authorization.key"];
                //_iv = ConfigurationManager.AppSettings["Authorization.iv"];

                _client = CouchbaseWrapper.CouchbaseManager.GetInstance("authorization");

                _lock = new ReaderWriterLockSlim();
                //_appsCredentials = new Dictionary<string, AppCredentials>();

                //if (!long.TryParse(deviceTokenExpiration, out deviceTokenExpirationSeconds) ||
                //    !long.TryParse(accessTokenExpiration, out accessTokenExpirationSeconds) ||
                //    !long.TryParse(refreshTokenExpiration, out refreshTokenExpirationSeconds) ||
                //    !long.TryParse(groupConfigsTtlSeconds, out _groupConfigsTtlSeconds) ||
                //    string.IsNullOrEmpty(_key) || string.IsNullOrEmpty(_iv))
                if (!long.TryParse(groupConfigsTtlSeconds, out _groupConfigsTtlSeconds))
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

        public GroupConfiguration GetGroupConfigurations(int groupId)
        {
            GroupConfiguration groupConfig = null;
            string groupKey = GroupConfiguration.GetGroupConfigId(groupId);

            // try get app credentials from dictionary
            if (_lock.TryEnterReadLock(1000))
            {
                try
                {
                    var group = HttpContext.Current.Cache.Get(groupKey);
                    if (group != null && group is GroupConfiguration)
                    {
                        groupConfig = group as GroupConfiguration;
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("GetGroupConfigurations: on extracting from cache with groupId = {0}, Exception = {1}", groupId, ex);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }

            // if not exists in dictionary, try get from CB 
            if (groupConfig == null)
            {
                groupConfig = _client.Get<GroupConfiguration>(groupKey);
                if (groupConfig != null)
                {
                    // add app credentials to dictionary if not exists
                    if (_lock.TryEnterWriteLock(1000))
                    {
                        try
                        {
                            HttpContext.Current.Cache.Insert(groupKey, groupConfig, null, DateTime.UtcNow.AddSeconds(_groupConfigsTtlSeconds), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
                        }
                        catch (Exception ex)
                        {
                            logger.ErrorFormat("GetGroupConfigurations: on adding to cache with groupId = {0}, Exception = {1}", groupId, ex);
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
                    logger.ErrorFormat("GetGroupConfigurations: group configuration not exist for groupId = {0}", groupId);
                }
            }

            return groupConfig;
        }

        //public AppCredentials GetAppCredentials(string appId)
        //{
        //    AppCredentials appCredentials = null;

        //    // try get app credentials from dictionary
        //    if (_lock.TryEnterReadLock(1000))
        //    {
        //        try
        //        {
        //            _appsCredentials.TryGetValue(appId, out appCredentials);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.ErrorFormat("GetAppCredentials: on extracting from dictionary with appId = {0}, Exception = {1}", appId, ex);
        //        }
        //        finally
        //        {
        //            _lock.ExitReadLock();
        //        }
        //    }

        //    // if not exists in dictionary, try get from CB 
        //    if (appCredentials == null)
        //    {
        //        string appCredentialsId = AppCredentials.GetAppCredentialsId(EncryptData(appId));
        //        appCredentials = _client.Get<AppCredentials>(appCredentialsId);
        //        if (appCredentials != null)
        //        {
        //            // add app credentials to dictionary if not exists
        //            if (_lock.TryEnterWriteLock(1000))
        //            {
        //                try
        //                {
        //                    if (!_appsCredentials.Keys.Contains(appId))
        //                    {
        //                        _appsCredentials.Add(appId, appCredentials);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    logger.ErrorFormat("GetAppCredentials: on adding to dictionary with appId = {0}, Exception = {1}", appId, ex);
        //                }
        //                finally
        //                {
        //                    _lock.ExitWriteLock();
        //                }
        //            }
        //        }
        //        // no app credentials in CB
        //        else
        //        {
        //            logger.ErrorFormat("GetAppCredentials: app credentials not exist for appId = {0}", appId);
        //        }
        //    }

        //    return appCredentials;
        //}

        //Maybe should be deleted later
        //public AppCredentials GenerateAppCredentials(int groupId)
        //{
        //    new AuthorizationManager();
        //    AppCredentials appCredentials = null;
        //    appCredentials = new AppCredentials(groupId);
        //    _client.Store<AppCredentials>(appCredentials);
        //    appCredentials.EncryptedAppId = DecryptData(appCredentials.EncryptedAppId);
        //    appCredentials.EncryptedAppSecret = DecryptData(appCredentials.EncryptedAppSecret);
        //    return appCredentials;
        //}



        //public string GenerateDeviceToken(string udid, string appId)
        //{
        //    // validate request parameters
        //    if (string.IsNullOrEmpty(udid) || string.IsNullOrEmpty(appId))
        //    {
        //        logger.ErrorFormat("GenerateDeviceToken: bad request. app_id = {0}, udid = {2}", appId, udid);
        //        returnError(403);
        //        return null;
        //    }

        //    // validate app credentials
        //    AppCredentials appCredentials = Instance.GetAppCredentials(appId);
        //    if (appCredentials == null)
        //    {
        //        logger.ErrorFormat("GenerateDeviceToken: appId not found = {0}", appId);
        //        returnError(403);
        //        return null;
        //    }

        //    // generate device token
        //    DeviceToken deviceToken = new DeviceToken(appCredentials.EncryptedAppId, udid);
        //    _client.Store<DeviceToken>(deviceToken, DateTime.UtcNow.AddSeconds(deviceTokenExpirationSeconds));
        //    return deviceToken.Token;
        //}

        //public object ExchangeDeviceToken(string udid, string appId, string appSecret, string deviceToken)
        //{
        //    // validate request parameters
        //    if (string.IsNullOrEmpty(udid) || string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret) || string.IsNullOrEmpty(deviceToken))
        //    {
        //        logger.ErrorFormat("ExchangeDeviceToken: Bad request udid = {0}, appId = {1}, appSecret = {2}, deviceToken = {3}", udid, appId, appSecret, deviceToken);
        //        returnError(403);
        //        return null;
        //    }

        //    // validate app credentials
        //    AppCredentials appCredentials = Instance.GetAppCredentials(appId);
        //    if (appCredentials == null || DecryptData(appCredentials.EncryptedAppSecret) != appSecret)
        //    {
        //        logger.ErrorFormat("ExchangeDeviceToken: app credentials not found or do not match for appId = {0}", appId);
        //        returnError(403);
        //        return null;
        //    }

        //    // validate device token
        //    string deviceTokenId = DeviceToken.GetDeviceTokenId(appCredentials.EncryptedAppId, deviceToken);
        //    DeviceToken deviceTokenObj = _client.Get<DeviceToken>(deviceTokenId);
        //    if (deviceTokenObj == null || deviceTokenObj.UDID != udid)
        //    {
        //        logger.ErrorFormat("ExchangeDeviceToken: device token not valid or expired. deviceToken = {0}, udid = {1}, appId = {2}", deviceToken, udid, appId);
        //        returnError(403);
        //        return null;
        //    }

        //    // generate access token and refresh token pair
        //    APIToken apiToken = new APIToken(appCredentials.EncryptedAppId, appCredentials.GroupId, udid);
        //    _client.Store<APIToken>(apiToken, DateTime.UtcNow.AddSeconds(refreshTokenExpirationSeconds));
        //    _client.Remove(deviceTokenId);

        //    return GetTokenResponseObject(apiToken);
        //}

        public APIToken GenerateAccessToken(string siteGuid, int groupId, bool isAdmin, bool isSTB)
        {
            if (string.IsNullOrEmpty(siteGuid))
            {
                logger.ErrorFormat("GenerateAccessToken: siteGuid is missing");
                returnError(403);
                return null;
            }

            // get group configurations
            GroupConfiguration groupConfig = Instance.GetGroupConfigurations(groupId);
            if (groupConfig == null)
            {
                logger.ErrorFormat("GenerateAccessToken: group configuration was not found for groupId = {0}", groupId);
                returnError(403);
                return null;
            }

            // generate access token and refresh token pair
            APIToken apiToken = new APIToken(siteGuid, groupId, isAdmin, groupConfig, isSTB);
            _client.Store<APIToken>(apiToken, DateTime.UtcNow.AddSeconds(groupConfig.RefreshTokenExpirationSeconds));
            
            return apiToken;
        }

        public void AddTokenToHeadersForValidNotAdminUser(TVPApiModule.Services.ApiUsersService.LogInResponseData signInResponse, int groupId)
        {
            if (HttpContext.Current.Items.Contains("tokenization") &&
                        signInResponse.UserData != null && signInResponse.LoginStatus == TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK)
            {
                var token = AuthorizationManager.Instance.GenerateAccessToken(signInResponse.SiteGuid, groupId, false, false);

                HttpContext.Current.Response.Headers.Add("access_token", string.Format("{0}|{1}", token.AccessToken, token.AccessTokenExpiration));
                HttpContext.Current.Response.Headers.Add("refresh_token", string.Format("{0}|{1}", token.RefreshToken, token.RefreshTokenExpiration));
            }
        }


        public object RefreshAccessToken(string refreshToken, string accessToken, int groupId, PlatformType platform)
        {
            // validate request parameters
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                logger.ErrorFormat("RefreshAccessToken: Bad request refreshToken = {0}, accessToken = {1}", refreshToken, accessToken);
                returnError(403);
                return null;
            }

            // try get api token from CB
            string apiTokenId = APIToken.GetAPITokenId(accessToken);
            CasGetResult<APIToken> casRes = _client.GetWithCas<APIToken>(apiTokenId);
            if (casRes == null || casRes.OperationResult != eOperationResult.NoError || casRes.Value == null)
            {
                logger.ErrorFormat("RefreshAccessToken: refreshToken expired. refreshToken = {0}, accessToken = {1}",  refreshToken, accessToken);
                returnError(403);
                return null;
            }

            APIToken apiToken = casRes.Value;

            // validate refresh token
            if (apiToken.RefreshToken != refreshToken)
            {
                logger.ErrorFormat("RefreshAccessToken: refreshToken not valid. refreshToken = {0}, accessToken = {1}", refreshToken, accessToken);
                returnError(403);
                return null;
            }

            string siteGuid = apiToken.SiteGuid;

            // validate siteGuid
            if (apiToken.SiteGuid != siteGuid)
            {
                logger.ErrorFormat("RefreshAccessToken: siteGuid not valid. siteGuid = {0}, refreshToken = {1}, accessToken = {2}", siteGuid, refreshToken, accessToken);
                returnError(403);
                return null;
            }

            // validate user
            UserResponseObject user = null;
            try
            {
                user = new ApiUsersService(groupId, platform).GetUserData(siteGuid);
            }
            catch (Exception)
            {
                logger.ErrorFormat("RefreshAccessToken: error while getting user. siteGuid = {0}, refreshToken = {1}, accessToken = {2}", siteGuid, refreshToken, accessToken);
                returnError(403);
                return null;
            }
            if (user == null || user.m_RespStatus != ResponseStatus.OK || user.m_user.m_eSuspendState == DomainSuspentionStatus.Suspended)
            {
                logger.ErrorFormat("RefreshAccessToken: siteGuid not valid. siteGuid = {0}, refreshToken = {1}, accessToken = {2}", siteGuid, refreshToken, accessToken);
                returnError(403);
                return null;
            }

            // get group configurations
            GroupConfiguration groupConfig = Instance.GetGroupConfigurations(groupId);
            if (groupConfig == null)
            {
                logger.ErrorFormat("RefreshAccessToken: group configuration was not found for groupId = {0}", groupId);
                returnError(403);
                return null;
            }

            // generate new access token with the old refresh token
            apiToken = new APIToken(apiToken, groupConfig.AccessTokenExpirationSeconds);

            // Store new access + refresh tokens pair
            if (_client.Store<APIToken>(apiToken, DateTime.UtcNow.AddSeconds(groupConfig.RefreshTokenExpirationSeconds)))
            {
                // delete the old one
                _client.Remove(apiTokenId);
            }
            else
            {
                logger.ErrorFormat("RefreshAccessToken: Failed to store new token, returning 403");
                returnError(403);
                return null;
            }

            return GetTokenResponseObject(apiToken, groupConfig);
        }

        public bool IsAccessTokenValid(string accessToken, int? domainId, int groupId, PlatformType platform, out string siteGuid, out bool isAdmin)
        {
            siteGuid = string.Empty;
            isAdmin = false;

            // if no access token - validation will be performed later if needed
            if (string.IsNullOrEmpty(accessToken))
            {
                return true;
            }

            string apiTokenId = APIToken.GetAPITokenId(accessToken);

            if (string.IsNullOrEmpty(accessToken))
            {
                logger.ErrorFormat("ValidateAccessToken: empty accessToken or siteGuid. access_token = {0}", accessToken);
                returnError(403);
                return false;
            }

            APIToken apiToken = _client.Get<APIToken>(apiTokenId);
            if (apiToken == null)
            {
                logger.ErrorFormat("ValidateAccessToken: access token not found. access_token = {0}", accessToken);
                returnError(403);
                return false;
            }

            siteGuid = apiToken.SiteGuid;
            isAdmin = apiToken.IsAdmin;

            // get group configurations
            GroupConfiguration groupConfig = Instance.GetGroupConfigurations(groupId);
            if (groupConfig == null)
            {
                logger.ErrorFormat("ValidateAccessToken: group configuration was not found for groupId = {0}", groupId);
                returnError(403);
                return false;
            }

            // check if access token expired 
            if (TimeHelper.ConvertFromUnixTimestamp(apiToken.AccessTokenExpiration) < DateTime.UtcNow)
            {
                logger.ErrorFormat("ValidateAccessToken: access token expired. access_token = {0}", accessToken);
                returnError(403);
                return false;
            }

            // access token is valid - extend refreshToken if extendable
            if (groupConfig.IsRefreshTokenExtendable)
            {
                apiToken.RefreshTokenExpiration = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshTokenExpirationSeconds));
                _client.Store<APIToken>(apiToken, DateTime.UtcNow.AddSeconds(groupConfig.RefreshTokenExpirationSeconds));
            }

            return true;
        }

        public bool ValidateMultipleSiteGuids(string initSiteGuid, string[] siteGuids, int groupId, PlatformType platform)
        {
            if (string.IsNullOrEmpty(initSiteGuid) || siteGuids == null || siteGuids.Length == 0)
            {
                logger.ErrorFormat("validateMultipleSiteGuids: initSiteGuid or siteGuids are empty. initSiteGuid {0}", initSiteGuid);
                returnError(403);
                return false;
            }

            // get domain
            Domain domain = new ApiDomainsService(groupId, platform).GetDomainByUser(initSiteGuid);
            if (domain == null)
            {
                logger.ErrorFormat("validateMultipleSiteGuids: domain not found for initSiteGuid = {0}", initSiteGuid);
                returnError(403);
                return false;
            }

            int userId = 0;
            foreach (var siteGuid in siteGuids)
            {
                userId = int.Parse(siteGuid);
                if (!domain.m_DefaultUsersIDs.Contains(userId) && !domain.m_masterGUIDs.Contains(userId) && !domain.m_UsersIDs.Contains(userId) && !domain.m_PendingUsersIDs.Contains(userId))
                {
                    logger.ErrorFormat("ValidateRequestParameters: initSiteGuid and one of the siteGuids are not in the same domain. initSiteGuid = {0}, userId = {1}", initSiteGuid, userId);
                    returnError(403);
                    return false;
                }
            }

            return true;
        }

        public bool ValidateRequestParameters(string initSiteGuid, string siteGuid, int domainId, string udid, int groupId, PlatformType platform)
        {
            if (string.IsNullOrEmpty(initSiteGuid))
            {
                logger.ErrorFormat("ValidateRequestParameters: initSiteGuid or siteGuid are empty. initSiteGuid {0}, siteGuid = {1}", initSiteGuid, siteGuid);
                returnError(403);
                return false;
            }

            if ((!string.IsNullOrEmpty(siteGuid) && initSiteGuid != siteGuid) || domainId != 0 || !string.IsNullOrEmpty(udid))
            {
               Domain domain = new ApiDomainsService(groupId, platform).GetDomainByUser(initSiteGuid);
               if (domain == null)
                {
                    logger.ErrorFormat("ValidateRequestParameters: domain not found for initSiteGuid = {0}", initSiteGuid);
                    returnError(403);
                    return false;
               }

                // if siteGuids are not the same, check if siteGuids are in the same domain
               int userId;
                if (!string.IsNullOrEmpty(siteGuid) && initSiteGuid != siteGuid)
                {
                    userId = int.Parse(siteGuid);
                    if (!domain.m_DefaultUsersIDs.Contains(userId) && !domain.m_masterGUIDs.Contains(userId) && !domain.m_UsersIDs.Contains(userId) && !domain.m_PendingUsersIDs.Contains(userId))
                    {
                        logger.ErrorFormat("ValidateRequestParameters: initSiteGuid and siteGuid are not in the same domain. initSiteGuid = {0}, siteGuid = {1}", initSiteGuid, siteGuid);
                        returnError(403);
                        return false;
                    }
                }

                // if the domain is not the users domain
                if (domainId != 0)
                {
                    if (domain.m_nDomainID != domainId)
                    {
                        logger.ErrorFormat("ValidateRequestParameters: siteGuid is not in the same domain. siteGuid = {0}, domainId = {2}", initSiteGuid, domainId);
                        returnError(403);
                        return false;
                    }
                }

                // if udid is not in domain
                if (!string.IsNullOrEmpty(udid))
                {
                    if (domain.m_deviceFamilies == null || domain.m_deviceFamilies.Length == 0)
                    {
                        logger.ErrorFormat("ValidateRequestParameters: udid is not in the domain. udid = {0}, domainId = {1}", udid, domain.m_nDomainID);
                        returnError(403);
                        return false;
                    }
                    foreach (var family in domain.m_deviceFamilies)
                    {
                        if (family.DeviceInstances.Where(d => d.m_deviceUDID == udid).FirstOrDefault() != null)
                        {
                            return true;
                        }
                    }
                    logger.ErrorFormat("ValidateRequestParameters: udid is not in the domain. udid = {0}, domainId = {1}", udid, domain.m_nDomainID);
                    returnError(403);
                    return false;
                }
            }
            return true;
        }

        public void returnError(int statusCode, string description = null)
        {
            HttpContext.Current.Items["StatusCode"] = statusCode;
            if (!string.IsNullOrEmpty(description))
            {
                HttpContext.Current.Items["StatusDescription"] = description;
            }
        }

        private object GetTokenResponseObject(APIToken apiToken, GroupConfiguration groupConfig)
        {
            if (apiToken == null)
                return null;

            return new
            {
                access_token = apiToken.AccessToken,
                refresh_token = apiToken.RefreshToken,
                expiration_time = apiToken.AccessTokenExpiration,
                refresh_expiration_time = apiToken.RefreshTokenExpiration
            };
        }


        //public string EncryptData(string data)
        //{
        //    if (data == null)
        //        return null;
        //    return SecurityHelper.EncryptData(_key, _iv, data);
        //}

        //public string DecryptData(string data)
        //{
        //    if (data == null)
        //        return null;
        //    return SecurityHelper.DecryptData(_key, _iv, data);
        //}

        public void DeleteAccessToken(string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                string apiTokenId = APIToken.GetAPITokenId(accessToken);
                // delete token
                _client.Remove(apiTokenId);
            }
        }
    }
}
