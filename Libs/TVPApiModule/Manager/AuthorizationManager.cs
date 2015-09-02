using CouchbaseWrapper;
using CouchbaseWrapper.DalEntities;
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
using KLogMonitor;
using System.Reflection;

namespace TVPApiModule.Manager
{
    public class AuthorizationManager
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static long _groupConfigsTtlSeconds;
        private static GenericCouchbaseClient _client;
        private static ReaderWriterLockSlim _lock;
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
                string groupConfigsTtlSeconds = ConfigurationManager.AppSettings["Authorization.GroupConfigsTtlSeconds"];

                _client = CouchbaseWrapper.CouchbaseManager.GetInstance("authorization");
                _lock = new ReaderWriterLockSlim();

                if (!long.TryParse(groupConfigsTtlSeconds, out _groupConfigsTtlSeconds))
                {
                    logger.ErrorFormat("AuthorizationManager: Configuration Authorization.GroupConfigsTtlSeconds is missing!");
                    throw new Exception("Configuration Authorization.GroupConfigsTtlSeconds is missing!");
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
                            HttpContext.Current.Cache.Insert(groupKey, groupConfig, null, _groupConfigsTtlSeconds == 0 ? DateTime.MaxValue : DateTime.UtcNow.AddSeconds(_groupConfigsTtlSeconds), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
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

        public APIToken GenerateAccessToken(string siteGuid, int groupId, bool isAdmin, bool isSTB)
        {
            if (string.IsNullOrEmpty(siteGuid))
            {
                logger.ErrorFormat("GenerateAccessToken: siteGuid is missing");
                returnError(400);
                return null;
            }

            // get group configurations
            GroupConfiguration groupConfig = Instance.GetGroupConfigurations(groupId);
            if (groupConfig == null)
            {
                logger.ErrorFormat("GenerateAccessToken: group configuration was not found for groupId = {0}", groupId);
                returnError(500);
                return null;
            }

            // generate access token and refresh token pair
            APIToken apiToken = new APIToken(siteGuid, groupId, isAdmin, groupConfig, isSTB);

            // try store in CB, will return false if the same token already exists
            if (!_client.Add<APIToken>(apiToken, TimeHelper.ConvertFromUnixTimestamp(apiToken.RefreshTokenExpiration)))
            {
                logger.ErrorFormat("GenerateAccessToken: access token was not saved in CB.");
                returnError(500);
                return null;
            }

            return apiToken;
        }

        public void AddTokenToHeadersForValidNotAdminUser(TVPApiModule.Services.ApiUsersService.LogInResponseData signInResponse, int groupId)
        {
            if (HttpContext.Current.Items.Contains("tokenization") && signInResponse.UserData != null &&
                (signInResponse.LoginStatus == ResponseStatus.OK || signInResponse.LoginStatus == ResponseStatus.UserNotActivated || signInResponse.LoginStatus == ResponseStatus.DeviceNotRegistered ||
                signInResponse.LoginStatus == ResponseStatus.UserNotMasterApproved || signInResponse.LoginStatus == ResponseStatus.UserNotIndDomain || signInResponse.LoginStatus == ResponseStatus.UserWithNoDomain ||
                signInResponse.LoginStatus == ResponseStatus.UserSuspended))
            {
                var token = AuthorizationManager.Instance.GenerateAccessToken(signInResponse.SiteGuid, groupId, false, false);
                if (token != null)
                {
                    HttpContext.Current.Response.Headers.Add("access_token", string.Format("{0}|{1}", token.AccessToken, token.AccessTokenExpiration));
                    HttpContext.Current.Response.Headers.Add("refresh_token", string.Format("{0}|{1}", token.RefreshToken, token.RefreshTokenExpiration));
                }
            }
        }


        public object RefreshAccessToken(string refreshToken, string accessToken, int groupId, PlatformType platform)
        {
            // validate request parameters
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                logger.ErrorFormat("RefreshAccessToken: Bad request accessToken or refreshToken are empty");
                returnError(400);
                return null;
            }

            // try get api token from CB
            string apiTokenId = APIToken.GetAPITokenId(accessToken);
            CasGetResult<APIToken> casRes = _client.GetWithCas<APIToken>(apiTokenId);
            if (casRes == null)
            {
                logger.ErrorFormat("RefreshAccessToken: No response from CB.");
                returnError(401);
                return null;
            }
            if (casRes.OperationResult != eOperationResult.NoError)
            {
                logger.ErrorFormat("RefreshAccessToken: Error response from CB: OperationResult = {0}", casRes.OperationResult);
                returnError(401);
                return null;
            }
            if (casRes.Value == null)
            {
                logger.ErrorFormat("RefreshAccessToken: Token doc not found in CB - refreshToken expired.");
                returnError(401);
                return null;
            }

            APIToken apiToken = casRes.Value;

            // validate refresh token
            if (apiToken.RefreshToken != refreshToken)
            {
                logger.ErrorFormat("RefreshAccessToken: refreshToken not valid.");
                returnError(401);
                return null;
            }

            string siteGuid = apiToken.SiteGuid;

            // validate user
            UserResponseObject user = null;
            try
            {
                user = new ApiUsersService(groupId, platform).GetUserData(siteGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("RefreshAccessToken: error while getting user. siteGuid = {0}, exception = {1}", siteGuid, ex);
                returnError(500);
                return null;
            }
            if (user == null || (user.m_RespStatus != ResponseStatus.OK && user.m_RespStatus != ResponseStatus.UserNotActivated && user.m_RespStatus != ResponseStatus.DeviceNotRegistered &&
                user.m_RespStatus != ResponseStatus.UserNotMasterApproved && user.m_RespStatus != ResponseStatus.UserNotIndDomain && user.m_RespStatus != ResponseStatus.UserWithNoDomain &&
                user.m_RespStatus != ResponseStatus.UserSuspended))
            {
                logger.ErrorFormat("RefreshAccessToken: siteGuid not valid. siteGuid = {0}", siteGuid);
                returnError(401);
                return null;
            }

            // get group configurations
            GroupConfiguration groupConfig = Instance.GetGroupConfigurations(groupId);
            if (groupConfig == null)
            {
                logger.ErrorFormat("RefreshAccessToken: group configuration was not found for groupId = {0}", groupId);
                returnError(500);
                return null;
            }

            // generate new access token with the old refresh token
            apiToken = new APIToken(apiToken, groupConfig);

            // Store new access + refresh tokens pair
            if (_client.Add<APIToken>(apiToken, TimeHelper.ConvertFromUnixTimestamp(apiToken.RefreshTokenExpiration)))
            {
                // delete the old one
                _client.Remove(apiTokenId);
            }
            else
            {
                logger.ErrorFormat("RefreshAccessToken: Failed to store new token, returning 500");
                returnError(500);
                return null;
            }

            return GetTokenResponseObject(apiToken);
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

            if (string.IsNullOrEmpty(accessToken))
            {
                logger.ErrorFormat("ValidateAccessToken: empty accessToken or siteGuid.");
                returnError(401);
                return false;
            }

            string apiTokenId = APIToken.GetAPITokenId(accessToken);

            APIToken apiToken = _client.Get<APIToken>(apiTokenId);
            if (apiToken == null)
            {
                logger.ErrorFormat("ValidateAccessToken: access token not found.");
                returnError(401);
                return false;
            }

            siteGuid = apiToken.SiteGuid;
            isAdmin = apiToken.IsAdmin;

            // get group configurations
            GroupConfiguration groupConfig = Instance.GetGroupConfigurations(groupId);
            if (groupConfig == null)
            {
                logger.ErrorFormat("ValidateAccessToken: group configuration was not found for groupId = {0}", groupId);
                returnError(500);
                return false;
            }

            // check if access token expired 
            if (TimeHelper.ConvertFromUnixTimestamp(apiToken.AccessTokenExpiration) < DateTime.UtcNow)
            {
                logger.ErrorFormat("ValidateAccessToken: access token expired.");
                returnError(401);
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
                returnError(400);
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

        public void returnError(int statusCode)
        {
            returnError(statusCode, null);
        }

        public void returnError(int statusCode, string description = null)
        {
            HttpContext.Current.Items["StatusCode"] = statusCode;
            if (!string.IsNullOrEmpty(description))
            {
                HttpContext.Current.Items["StatusDescription"] = description;
            }
        }

        public object GetTokenResponseObject(APIToken apiToken)
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

        public void DeleteAccessToken(string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                string apiTokenId = APIToken.GetAPITokenId(accessToken);
                // delete token
                _client.Remove(apiTokenId);
            }
        }

        public static bool IsTokenizationEnabled()
        {
            return HttpContext.Current.Items.Contains("tokenization");
        }

        public static bool IsSwitchingUsersAllowed(int groupId)
        {
            GroupConfiguration groupConfig = Instance.GetGroupConfigurations(groupId);

            return groupConfig.IsSwitchingUsersAllowed;
        }


        // Updates the siteguid of the token 
        public object UpdateUserInToken(string accessToken, string siteGuid, int groupId)
        {
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(siteGuid))
            {
                logger.ErrorFormat("UpdateUserInToken: accessToken or siteGuid empty");
                returnError(400);
                return false;
            }

            // get token
            string apiTokenId = APIToken.GetAPITokenId(accessToken);
            APIToken apiToken = _client.Get<APIToken>(apiTokenId);
            if (apiToken == null)
            {
                logger.ErrorFormat("UpdateUserInToken: access token not found.");
                returnError(401);
                return false;
            }

            // get group configurations
            GroupConfiguration groupConfig = Instance.GetGroupConfigurations(groupId);
            if (groupConfig == null)
            {
                logger.ErrorFormat("UpdateUserInToken: group configuration was not found for groupId = {0}", groupId);
                returnError(500);
                return false;
            }

            apiToken.SiteGuid = siteGuid;

            // access token is valid - extend refreshToken if extendable
            if (groupConfig.IsRefreshTokenExtendable)
            {
                apiToken.RefreshTokenExpiration = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshTokenExpirationSeconds));
            }

            // store the updated token
            _client.Store<APIToken>(apiToken, TimeHelper.ConvertFromUnixTimestamp(apiToken.RefreshTokenExpiration));


            // return token response
            return Instance.GetTokenResponseObject(apiToken);
        }

        public static Domain GetSiteGuidsDomain(string siteGuid, Domain[] domains)
        {
            Domain siteGuidsDomain = null;

            int userId;
            foreach (var domain in domains)
            {
                userId = int.Parse(siteGuid);
                if (domain.m_DefaultUsersIDs.Contains(userId) || domain.m_masterGUIDs.Contains(userId) || domain.m_UsersIDs.Contains(userId) || domain.m_PendingUsersIDs.Contains(userId))
                {
                    siteGuidsDomain = domain;
                }
            }
            return siteGuidsDomain;
        }
    }
}
