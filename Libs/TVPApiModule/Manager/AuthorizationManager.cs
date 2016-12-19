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
using CouchbaseManager;

namespace TVPApiModule.Manager
{
    public class AuthorizationManager
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static long _groupConfigsTtlSeconds;
        private static CouchbaseManager.CouchbaseManager cbManager;
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

                cbManager = new CouchbaseManager.CouchbaseManager("authorization");
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
                groupConfig = cbManager.Get<GroupConfiguration>(groupKey, true);
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

        public APIToken GenerateAccessToken(string siteGuid, int groupId, bool isAdmin, bool isSTB, string udid, PlatformType platform)
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

            // check if session revocation is allowed and udid is not empty
            if (groupConfig.SessionRevocationEnabled && string.IsNullOrEmpty(udid))
            {
                logger.ErrorFormat("GenerateAccessToken: UDID cannot be empty when session revocation is enabled. siteGuid = {0}", siteGuid);
                returnError(403);
                return null;
            }

            // generate access token and refresh token pair
            APIToken apiToken = new APIToken(siteGuid, groupId, isAdmin, groupConfig, isSTB, udid, platform);
            RefreshToken refreshToken = new RefreshToken(apiToken);

            // try store access token doc in CB, will return false if the same token already exists
            if (!cbManager.Add(apiToken.Id, apiToken, (uint)(apiToken.AccessTokenExpiration - TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow)), true))
            {
                logger.ErrorFormat("GenerateAccessToken: access token was not saved in CB.");
                returnError(500);
                return null;
            }

            // try store refresh token doc in CB, will return false if the same token already exists
            if (!cbManager.Add(refreshToken.Id, refreshToken, (uint)(apiToken.RefreshTokenExpiration - TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow)), true))
            {
                logger.ErrorFormat("GenerateAccessToken: refresh token was not saved in CB.");
                returnError(500);
                return null;
            }

            // handle session revocation if turned on for the group
            if (groupConfig.SessionRevocationEnabled)
            {
                string sessionInfoString = string.Format("userId = {0}, udid = {1}, IP = {2}, groupId = {3}", siteGuid, udid, SiteHelper.GetClientIP(), groupId);

                // check if the user already logged in from the same device by getting the user device tokens view
                string viewId = UserDeviceTokensView.GetViewId(siteGuid, udid);
                UserDeviceTokensView view = cbManager.Get<UserDeviceTokensView>(viewId, true);
                if (view != null)
                {
                    // delete old access token
                    if (!cbManager.Remove(view.AccessTokenId))
                    {
                        logger.ErrorFormat("GenerateAccessToken: failed to delete old access token for {0}", sessionInfoString);
                    }
                    else
                    {
                        logger.DebugFormat("GenerateAccessToken: Removed access token with ID = {0} for {1}", view.AccessTokenId, sessionInfoString);
                    }

                    // delete old refresh token
                    if (!cbManager.Remove(view.RefreshTokenId))
                    {
                        logger.ErrorFormat("GenerateAccessToken: failed to delete old refresh token for {0}", sessionInfoString);
                    }
                    else
                    {
                        logger.DebugFormat("GenerateAccessToken: Removed refresh token with ID = {0} for {1}", view.RefreshTokenId, sessionInfoString);
                    }
                }
                else
                {
                    // create new view doc
                    view = new UserDeviceTokensView()
                    {
                        UDID = udid,
                        SiteGuid = siteGuid,
                        GroupID = groupId,
                    };
                }

                // set new data in view
                view.AccessTokenId = apiToken.Id;
                view.RefreshTokenId = refreshToken.Id;
                view.AccessTokenExpiration = apiToken.AccessTokenExpiration;
                view.RefreshTokenExpiration = apiToken.RefreshTokenExpiration;

                // save new view
                if (!cbManager.Set(view.Id, view, (uint)(apiToken.RefreshTokenExpiration - TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow)), true))
                {
                    logger.ErrorFormat("GenerateAccessToken: failed to to save user-device tokens view for {0}", sessionInfoString);
                }
            }
            return apiToken;
        }

        public void AddTokenToHeadersForValidNotAdminUser(TVPApiModule.Services.ApiUsersService.LogInResponseData signInResponse, int groupId, string udid, PlatformType platform)
        {
            if (HttpContext.Current.Items.Contains("tokenization") && signInResponse.UserData != null &&
                (signInResponse.LoginStatus == ResponseStatus.OK || signInResponse.LoginStatus == ResponseStatus.UserNotActivated || signInResponse.LoginStatus == ResponseStatus.DeviceNotRegistered ||
                signInResponse.LoginStatus == ResponseStatus.UserNotMasterApproved || signInResponse.LoginStatus == ResponseStatus.UserNotIndDomain || signInResponse.LoginStatus == ResponseStatus.UserWithNoDomain ||
                signInResponse.LoginStatus == ResponseStatus.UserSuspended))
            {
                var token = AuthorizationManager.Instance.GenerateAccessToken(signInResponse.SiteGuid, groupId, false, false, udid, platform);
                if (token != null)
                {
                    HttpContext.Current.Response.Headers.Add("access_token", string.Format("{0}|{1}", token.AccessToken, token.AccessTokenExpiration));
                    HttpContext.Current.Response.Headers.Add("refresh_token", string.Format("{0}|{1}", token.RefreshToken, token.RefreshTokenExpiration));
                }
            }
        }

        public object RefreshAccessToken(string refreshToken, string accessToken, int groupId, PlatformType platform, string udid)
        {
            // validate request parameters
            if (string.IsNullOrEmpty(refreshToken))
            {
                logger.ErrorFormat("RefreshAccessToken: Bad request refreshToken is empty");
                returnError(400);
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

            // check if session revocation is allowed and udid is not empty
            if (groupConfig.SessionRevocationEnabled && string.IsNullOrEmpty(udid))
            {
                logger.ErrorFormat("RefreshAccessToken: UDID cannot be empty when session revocation is enabled. refreshToken = {0}", refreshToken);
                returnError(403);
                return null;
            }

            RefreshToken refreshTokenDoc = null;
            APIToken accessTokenDoc = null;

            string apiTokenId = APIToken.GetAPITokenId(accessToken);
            string refreshTokenId = RefreshToken.GetRefreshTokenId(refreshToken);

            // try get refresh token from CB
            ulong refreshVersion;
            refreshTokenDoc = cbManager.GetWithVersion<RefreshToken>(refreshTokenId, out refreshVersion, true);
            if (refreshTokenDoc == null)
            {
                logger.DebugFormat("RefreshAccessToken: Refresh token not found - token doc not found in CB.");

                // try extract refresh token data using access token dox (if access token supplied).
                if (string.IsNullOrEmpty(accessToken))
                {
                    logger.ErrorFormat("RefreshAccessToken: Refresh token doc not found, access token not supplied - refresh cannot be done");
                }

                // try get access token doc from CB
                ulong accessVersion;
                accessTokenDoc = cbManager.GetWithVersion<APIToken>(apiTokenId, out accessVersion, true);
                if (accessTokenDoc == null)
                {
                    logger.ErrorFormat("RefreshAccessToken: Access token not found - no response from CB.");
                    returnError(401);
                    return null;
                }
                if (accessTokenDoc == null)
                {
                    logger.ErrorFormat("RefreshAccessToken: Access token not found (refreshToken expired).");
                    returnError(401);
                    return null;
                }

                // create refresh access doc
                refreshTokenDoc = new RefreshToken(accessTokenDoc);
            }

            // validate refresh token
            if (refreshTokenDoc.RefreshTokenValue != refreshToken)
            {
                logger.ErrorFormat("RefreshAccessToken: refreshToken not valid.");
                returnError(401);
                return null;
            }

            // if the provided udid does not match the udid in the token and session revocation and the token is not old (and missing udid) is enabled return 403
            if (groupConfig.SessionRevocationEnabled && !string.IsNullOrEmpty(refreshTokenDoc.UDID) && refreshTokenDoc.UDID != udid)
            {
                logger.ErrorFormat("RefreshAccessToken: provided udid does not match the udid in the token and session revocation enabled. siteGuid = {0}, supplied udid = {1}", refreshTokenDoc.SiteGuid, udid);
                returnError(403);
                return null;
            }
            string siteGuid = refreshTokenDoc.SiteGuid;

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

            // generate new access token with the old refresh token
            accessTokenDoc = new APIToken(refreshTokenDoc, groupConfig);

            // set refresh token new expiration as calculated in access token
            refreshTokenDoc.RefreshTokenExpiration = accessTokenDoc.RefreshTokenExpiration;

            // Store new access + refresh tokens pair
            if (cbManager.Set(accessTokenDoc.Id, accessTokenDoc, (uint)(accessTokenDoc.AccessTokenExpiration - TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow)), true))
            {
                cbManager.Add(refreshTokenDoc.Id, refreshTokenDoc, (uint)(refreshTokenDoc.RefreshTokenExpiration - TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow)), true);
            }
            else
            {
                logger.ErrorFormat("RefreshAccessToken: Failed to store new access token, returning 500");
                returnError(500);
                return null;
            }

            // handle session revocation if enabled
            if (groupConfig.SessionRevocationEnabled)
            {
                string sessionInfoString = string.Format("userId = {0}, udid = {1}, IP = {2}, groupId = {3}", siteGuid, udid, SiteHelper.GetClientIP(), groupId);

                // update view doc if revocation session is enabled
                string viewId = UserDeviceTokensView.GetViewId(siteGuid, udid);
                UserDeviceTokensView view = cbManager.Get<UserDeviceTokensView>(viewId, true);
                if (view != null)
                {
                    // delete old access token
                    if (!cbManager.Remove(view.AccessTokenId))
                    {
                        logger.ErrorFormat("RefreshAccessToken: failed to delete old access token for user = {0} udid = {1}.", siteGuid, udid);
                    }
                    else
                    {
                        logger.DebugFormat("RefreshAccessToken: Removed access token with ID = {0} for {1}", view.RefreshTokenId, sessionInfoString);

                    }
                }
                else
                {
                    // create new view doc if not exists
                    view = new UserDeviceTokensView()
                    {
                        UDID = udid,
                        SiteGuid = siteGuid,
                        GroupID = groupId,
                    };
                }

                // set new data in view
                view.AccessTokenId = accessTokenDoc.Id;
                view.RefreshTokenId = refreshTokenDoc.Id;
                view.AccessTokenExpiration = accessTokenDoc.AccessTokenExpiration;
                view.RefreshTokenExpiration = accessTokenDoc.RefreshTokenExpiration;

                // save new view
                if (!cbManager.Set(view.Id, view, (uint)(accessTokenDoc.RefreshTokenExpiration - TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow)), true))
                {
                    logger.ErrorFormat("RefreshAccessToken: failed to to save user-device tokens view for user = {0} udid = {1}.", siteGuid, udid);
                }
            }

            return GetTokenResponseObject(accessTokenDoc);
        }

        public bool IsAccessTokenValid(string accessToken, int? domainId, int groupId, PlatformType platform, string udid, out string siteGuid, out bool isAdmin)
        {
            siteGuid = string.Empty;
            isAdmin = false;

            bool isRefreshTokenInCb = false;

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

            APIToken apiToken = cbManager.Get<APIToken>(apiTokenId, true);
            if (apiToken == null)
            {
                logger.ErrorFormat("ValidateAccessToken: access token not found.");
                returnError(401);
                return false;
            }

            if (!string.IsNullOrEmpty(apiToken.UDID) && apiToken.UDID != udid)
            {
                logger.ErrorFormat("ValidateAccessToken: access token UDID does not match initObj.UDID.");
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

            // access token is valid - extend refreshToken if extendable, store refresh token doc if not found

            // try to get refresh token 
            string refreshTokenId = RefreshToken.GetRefreshTokenId(apiToken.RefreshToken);
            ulong refreshVersion;
            RefreshToken refreshToken = cbManager.GetWithVersion<RefreshToken>(refreshTokenId, out refreshVersion, true);
            if (refreshToken == null)
            {
                // refresh token doc not found - create a new one
                refreshToken = new RefreshToken(apiToken);
            }
            else
            {
                isRefreshTokenInCb = true;
            }

            if (groupConfig.IsRefreshTokenExtendable)
            {
                apiToken.RefreshTokenExpiration = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshTokenExpirationSeconds));

                // set refresh token expiration as calculated in access token
                refreshToken.RefreshTokenExpiration = apiToken.RefreshTokenExpiration;

                // store updated access token doc
                cbManager.Set(apiToken.Id, apiToken, (uint)(apiToken.AccessTokenExpiration - TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow)), true);

                // store refresh token doc
                cbManager.Set(refreshToken.Id, refreshToken, (uint)(refreshToken.RefreshTokenExpiration - TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow)), true);
            }
            else if (!isRefreshTokenInCb)
            {
                // save refresh token in CB if it was not there before (for backwards compatibility)
                cbManager.Set(refreshToken.Id, refreshToken, (uint)(refreshToken.RefreshTokenExpiration - TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow)), true);
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
                if (cbManager.Remove(apiTokenId))
                    logger.DebugFormat("DeleteAccessToken: removed access token {0} on logout", accessToken);

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
        public object UpdateUserInToken(string accessToken, string siteGuid, int groupId, string udid)
        {
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(siteGuid))
            {
                logger.ErrorFormat("UpdateUserInToken: accessToken or siteGuid empty");
                returnError(400);
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

            // check if session revocation is allowed and udid is not empty
            if (groupConfig.SessionRevocationEnabled && string.IsNullOrEmpty(udid))
            {
                logger.ErrorFormat("UpdateUserInToken: UDID cannot be empty when session revocation is enabled. accessToken = {1}, new siteGuid = {0}", siteGuid, accessToken);
                returnError(403);
                return null;
            }

            // get token
            string apiTokenId = APIToken.GetAPITokenId(accessToken);
            APIToken apiToken = cbManager.Get<APIToken>(apiTokenId, true);
            if (apiToken == null)
            {
                logger.ErrorFormat("UpdateUserInToken: access token not found.");
                returnError(401);
                return false;
            }

            string refreshTokenId = RefreshToken.GetRefreshTokenId(apiToken.RefreshToken);
            RefreshToken refreshToken = cbManager.Get<RefreshToken>(refreshTokenId, true);
            if (refreshTokenId == null)
            {
                logger.ErrorFormat("UpdateUserInToken: refresh token not found.");
                returnError(401);
                return false;
            }

            // if session revocation enabled - remove the old user-device tokens view for the old user
            if (groupConfig.SessionRevocationEnabled)
            {
                if (!string.IsNullOrEmpty(apiToken.UDID) && apiToken.UDID != udid)
                {
                    logger.ErrorFormat("UpdateUserInToken: request UDID and token UDID do not match ", groupId);
                    returnError(403);
                    return false;
                }

                string sessionInfoString = string.Format("userId = {0}, udid = {1}, IP = {2}, groupId = {3}", siteGuid, udid, SiteHelper.GetClientIP(), groupId);

                if (!cbManager.Remove(UserDeviceTokensView.GetViewId(apiToken.SiteGuid, udid)))
                {
                    logger.ErrorFormat("UpdateUserInToken: failed to remove user-device tokens view on changing user for {0}", sessionInfoString);
                }
                else
                {
                    logger.DebugFormat("UpdateUserInToken: removed user-device tokens view on changing user for {0}", sessionInfoString);
                }
            }

            // change user id in both access and refresh tokens
            apiToken.SiteGuid = siteGuid;
            refreshToken.SiteGuid = siteGuid;

            // access token is valid - extend refreshToken if extendable
            if (groupConfig.IsRefreshTokenExtendable)
            {
                apiToken.RefreshTokenExpiration = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshTokenExpirationSeconds));
                refreshToken.RefreshTokenExpiration = apiToken.RefreshTokenExpiration;
            }

            // store the updated token
            if (!cbManager.Set(apiToken.Id, apiToken, (uint)(apiToken.RefreshTokenExpiration - TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow)), true))
            {
                logger.ErrorFormat("UpdateUserInToken: access token was not saved in CB.");
                returnError(500);
                return null;
            }

            // store updated refresh token doc in CB
            if (!cbManager.Set(refreshToken.Id, refreshToken, (uint)(apiToken.RefreshTokenExpiration - TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow)), true))
            {
                logger.ErrorFormat("UpdateUserInToken: refresh token was not saved in CB.");
                returnError(500);
                return null;
            }

            // revoke the old token of the new user
            // save new user-device tokens view for the new user if session revocation is enabled for the group
            if (groupConfig.SessionRevocationEnabled)
            {
                string sessionInfoString = string.Format("userId = {0}, udid = {1}, IP = {2}, groupId = {3}", siteGuid, udid, SiteHelper.GetClientIP(), groupId);

                // check if the user already logged in from the same device by getting the user device tokens view
                string newUserViewId = UserDeviceTokensView.GetViewId(siteGuid, udid);
                UserDeviceTokensView newUserView = cbManager.Get<UserDeviceTokensView>(newUserViewId, true);
                if (newUserView != null)
                {
                    // delete old access token
                    if (!cbManager.Remove(newUserView.AccessTokenId))
                    {
                        logger.ErrorFormat("UpdateUserInToken: failed to delete old access token for {0}", sessionInfoString);
                    }
                    else
                    {
                        logger.DebugFormat("UpdateUserInToken: Removed access token with ID = {0} for {1}", newUserView.AccessTokenId, sessionInfoString);
                    }

                    // delete old refresh token
                    if (!cbManager.Remove(newUserView.RefreshTokenId))
                    {
                        logger.ErrorFormat("UpdateUserInToken: failed to delete old refresh token for {0}", sessionInfoString);
                    }
                    else
                    {
                        logger.DebugFormat("UpdateUserInToken: Removed refresh token with ID = {0} for {1}", newUserView.RefreshTokenId, sessionInfoString);
                    }

                    newUserView.AccessTokenExpiration = apiToken.AccessTokenExpiration;
                    newUserView.AccessTokenId = apiToken.Id;
                    newUserView.RefreshTokenExpiration = apiToken.RefreshTokenExpiration;
                    newUserView.RefreshTokenId = refreshToken.Id;
                }
                else
                {
                    // create new view for the new user + device
                    newUserView = new UserDeviceTokensView()
                    {
                        AccessTokenExpiration = apiToken.AccessTokenExpiration,
                        AccessTokenId = apiToken.Id,
                        GroupID = groupId,
                        RefreshTokenExpiration = apiToken.RefreshTokenExpiration,
                        RefreshTokenId = refreshToken.Id,
                        SiteGuid = siteGuid,
                        UDID = udid
                    };
                }
                // save new view
                if (!cbManager.Set(newUserView.Id, newUserView, (uint)(apiToken.RefreshTokenExpiration - TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow)), true))
                {
                    logger.ErrorFormat("GenerateAccessToken: failed to to save user-device tokens view for {0}", sessionInfoString);
                }
            }

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

        internal static long GetPlatformExpiration(GroupConfiguration group, PlatformType platform, DateTime? now = null)
        {
            long expiration;
            if (group.RefreshTokenExpirationForPlatforms.ContainsKey(platform.ToString()))
            {
                expiration = group.RefreshTokenExpirationForPlatforms[platform.ToString()];
            }
            else
            {
                expiration = group.RefreshTokenExpirationSeconds;
            }

            if (now.HasValue)
            {
                return (long)TimeHelper.ConvertToUnixTimestamp(now.Value.AddSeconds(expiration));
            }
            else
                return expiration;
        }
    }
}
