using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.ClientManagers.Client;
using WebAPI.Models.Users;
using WebAPI.Utils;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using APILogic.Api.Managers;
using ApiObjects;
using ConfigurationManager;
using TVinciShared;
using Core.Api;

namespace WebAPI.Managers
{
    public class AuthorizationManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string APP_TOKEN_PRIVILEGE_SESSION_ID = "sessionid";
        private const string APP_TOKEN_PRIVILEGE_APP_TOKEN = "apptoken";
        private const string CB_SECTION_NAME = "tokens";

        private const string USERS_SESSIONS_KEY_FORMAT = "sessions_{0}";
        private const string REVOKED_KS_KEY_FORMAT = "r_ks_{0}";
        private const string REVOKED_SESSION_KEY_FORMAT = "r_session_{0}";


        private static CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(CB_SECTION_NAME);

        public static KalturaLoginSession RefreshSession(string refreshToken, string udid = null)
        {
            KS ks = KS.GetFromRequest();
            int groupId = ks.GroupId;

            // validate request parameters
            if (string.IsNullOrEmpty(refreshToken))
            {
                log.ErrorFormat("RefreshSession: Bad request refresh token is empty");
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "refreshToken");
            }

            if (!IsKsValid(ks, false))
            {
                log.ErrorFormat("RefreshSession: KS already revoked or overwritten");
                throw new UnauthorizedException(UnauthorizedException.KS_EXPIRED);
            }

            // get group configurations
            Group group = GetGroupConfiguration(groupId);

            // get token from CB
            string tokenKey = string.Format(group.TokenKeyFormat, refreshToken);
            ulong version;
            ApiToken token = cbManager.GetWithVersion<ApiToken>(tokenKey, out version, true);
            if (token == null)
            {
                log.ErrorFormat("RefreshSession: refreshToken expired");
                throw new UnauthorizedException(UnauthorizedException.INVALID_REFRESH_TOKEN);
            }

            // validate expired ks
            if (ks.ToString() != token.KS)
            {
                log.ErrorFormat("RefreshSession: invalid ks");
                throw new UnauthorizedException(UnauthorizedException.KS_EXPIRED);
            }

            if (udid != token.Udid)
            {
                log.ErrorFormat("RefreshSession: UDID does not match the KS's UDID, UDID = {0}, KS.UDID = {1}", udid, token.Udid);
                throw new UnauthorizedException(UnauthorizedException.INVALID_UDID, udid);
            }

            string userId = token.UserId;

            // get user
            ValidateUser(groupId, userId);

            // generate new access token with the old refresh token
            token = new ApiToken(token, group, udid);

            // update the sessions data
            var ksData = KSUtils.ExtractKSPayload(token.KsObject);
            if (!UpdateUsersSessionsRevocationTime(group, userId, udid, ksData.CreateDate, (int)token.AccessTokenExpiration))
            {
                log.ErrorFormat("RefreshSession: Failed to store updated users sessions, userId = {0}", userId);
                throw new UnauthorizedException(UnauthorizedException.REFRESH_TOKEN_FAILED);
            }

            // Store new access + refresh tokens pair
            if (!cbManager.SetWithVersion(tokenKey, token, version, (uint)(token.RefreshTokenExpiration - DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow)), true))
            {
                log.ErrorFormat("RefreshSession: Failed to store refreshed token");
                throw new UnauthorizedException(UnauthorizedException.REFRESH_TOKEN_FAILED);
            }

            return new KalturaLoginSession()
            {
                KS = token.KS,
                RefreshToken = token.RefreshToken
            };
        }

        public static KalturaLoginSession GenerateSession(string userId, int groupId, bool isAdmin, bool isLoginWithPin, int domainId, string udid, List<long> userRoles, Dictionary<string, string> privileges = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                log.ErrorFormat("GenerateSession: userId is missing");
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "userId");
            }

            // generate access token and refresh token pair
            var regionId = Core.Catalog.CatalogLogic.GetRegionIdOfDomain(groupId, domainId, userId);

            // get group configurations
            var group = GetGroupConfiguration(groupId);
            var userSegments = new List<long>();
            var userSegmentsResponse = Core.Api.Module.GetUserSegments(groupId, userId, 0, 0);
            if (userSegmentsResponse.HasObjects())
            {
                userSegments.AddRange(userSegmentsResponse.Objects.Select(x => x.SegmentId));
            }

            var payload = new KS.KSData(udid, 0, regionId, userSegments, userRoles);
            var token = new ApiToken(userId, groupId, payload, isAdmin, group, isLoginWithPin, privileges);
            return GenerateSessionByApiToken(token, group);
        }

        private static KalturaLoginSession GenerateSessionByApiToken(ApiToken token, Group group)
        {
            string tokenKey = string.Format(group.TokenKeyFormat, token.RefreshToken);

            // update the sessions data
            var ksData = KSUtils.ExtractKSPayload(token.KsObject);
            if (!UpdateUsersSessionsRevocationTime(group, token.UserId, token.Udid, ksData.CreateDate, (int)token.AccessTokenExpiration))
            {
                log.ErrorFormat("GenerateSession: Failed to store updated users sessions, userId = {0}", token.UserId);
                throw new InternalServerErrorException();
            }

            KalturaLoginSession session = new KalturaLoginSession();

            if (group.IsRefreshTokenEnabled)
            {
                // try store in CB, will return false if the same token already exists
                if (!cbManager.Add(tokenKey, token, (uint)(token.RefreshTokenExpiration - DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow)), true))
                {
                    log.ErrorFormat("GenerateSession: Failed to store refreshed token");
                    throw new InternalServerErrorException();
                }

                session.RefreshToken = token.RefreshToken;
            }

            session.KS = token.KS;
            session.Expiry = DateUtils.DateTimeToUtcUnixTimestampSeconds(token.KsObject.Expiration);

            return session;
        }

        private static Group GetGroupConfiguration(int groupId)
        {
            // get group configurations
            Group groupConfig = GroupsManager.GetGroup(groupId);
            if (groupConfig == null)
            {
                log.ErrorFormat("GetGroupConfiguration: group configuration was not found for groupId = {0}", groupId);
                throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "Partner");
            }

            return groupConfig;
        }

        private static void ValidateUser(int groupId, string userId)
        {
            // if anonymous user
            if (userId == "0")
                return;

            List<KalturaOTTUser> usersResponse = null;
            try
            {
                usersResponse = ClientsManager.UsersClient().GetUsersData(groupId, new List<string> { userId });
            }
            catch (ClientException ex)
            {
                log.ErrorFormat("RefreshAccessToken: error while getting user. userId = {0}, exception = {1}", userId, ex);
                ErrorUtils.HandleClientException(ex);
            }

            if (usersResponse == null || usersResponse.Count == 0)
            {
                log.ErrorFormat("RefreshAccessToken: user not found. siteGuid = {0}", userId);
                throw new NotFoundException(NotFoundException.OBJECT_ID_NOT_FOUND, "User", userId);
            }

            // validate user
            KalturaOTTUser user = usersResponse.Where(u => u.Id.ToString() == userId).FirstOrDefault();
            if (user == null || (user.UserState != KalturaUserState.ok && user.UserState != KalturaUserState.user_with_no_household))
            {
                log.ErrorFormat("RefreshAccessToken: user not valid. userId= {0}", userId);
                throw new UnauthorizedException(UnauthorizedException.INVALID_USER_ID, userId);
            }
        }

        internal static bool IsUserInHousehold(string userId, int groupId)
        {
            KalturaHousehold household = null;
            try
            {
                household = ClientsManager.DomainsClient().GetDomainByUser(groupId, KS.GetFromRequest().UserId);
            }
            catch (ClientException ex)
            {
                log.Error("IsUserInHousehold: got ClientException for GetDomainByUser", ex);
                household = null;
            }

            if (household != null &&
                ((household.Users != null && household.Users.FirstOrDefault(u => u.Id == userId) != null) ||
                (household.DefaultUsers != null && household.DefaultUsers.FirstOrDefault(u => u.Id == userId) != null) ||
                (household.MasterUsers != null && household.MasterUsers.FirstOrDefault(u => u.Id == userId) != null) ||
                (household.PendingUsers != null && household.PendingUsers.FirstOrDefault(u => u.Id == userId) != null)))
            {
                return true;
            }

            return false;
        }

        internal static bool IsUserInGroup(string userId, int groupId)
        {
            KalturaOTTUser user = null;
            try
            {
                var users = ClientsManager.UsersClient().GetUsersData(groupId, new List<string>() { userId });
                if (users != null && users.Count > 0)
                {
                    user = users[0];
                }
            }
            catch (ClientException ex)
            {
            }

            if (user == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #region AppToken

        internal static KalturaSessionInfo StartSessionWithAppToken(int groupId, string id, string tokenHash, string userId, string udid, KalturaSessionType? type, int? expiry, int domainId)
        {
            KalturaSessionInfo response = null;

            Group group = GroupsManager.GetGroup(groupId);

            // 1. get token from cb by id
            string appTokenCbKey = string.Format(group.AppTokenKeyFormat, id);
            AppToken appToken = cbManager.Get<AppToken>(appTokenCbKey, true);
            if (appToken == null)
            {
                log.ErrorFormat("StartSessionWithAppToken: failed to get AppToken from CB, key = {0}", appTokenCbKey);
                throw new NotFoundException(NotFoundException.OBJECT_ID_NOT_FOUND, "application-token", id);
            }

            // 2. check token status
            if (appToken.Status != KalturaAppTokenStatus.ACTIVE)
            {
                log.ErrorFormat("StartSessionWithAppToken: AppToken is not active, id = {0}", id);
                throw new ForbiddenException(ForbiddenException.NOT_ACTIVE_APP_TOKEN, id);
            }

            // 3. calc token hash - (ks + token) hash using the hash type 
            string cbTokenHash = appToken.CalcHash();

            // 4. compare the token hashes
            if (cbTokenHash != tokenHash)
            {
                log.ErrorFormat("StartSessionWithAppToken: token hash is not valid, id = {0}", id);
                throw new ForbiddenException(ForbiddenException.INVALID_APP_TOKEN_HASH);
            }

            // 5. get token expiration: the session duration will be the minimum between the token session duration and the token left expiration time
            long sessionDuration = 0;
            if (appToken.Expiry > 0)
            {
                sessionDuration = Math.Min(appToken.SessionDuration, appToken.Expiry - DateUtils.GetUtcUnixTimestampNow());
            }
            else
            {
                sessionDuration = appToken.SessionDuration;
            }

            // if the minimum is < 0 - token is expired (not possible in our case - will be deleted from CB before)
            if (sessionDuration < 0)
            {
                log.ErrorFormat("StartSessionWithAppToken: AppToken expired, id = {0}", id);
                throw new ForbiddenException(ForbiddenException.APP_TOKEN_EXPIRED);
            }

            // if expiry was supplied - take the minimum
            if (expiry != null && expiry.Value > 0)
            {
                sessionDuration = Math.Min(sessionDuration, expiry.Value);
            }

            // 6. get session type from cb token - user if not defined - we currently support only user
            KalturaSessionType sessionType = KalturaSessionType.USER;

            // 7. get session user id from cb token - if not defined - use the supplied userId
            if (!string.IsNullOrEmpty(appToken.SessionUserId))
            {
                userId = appToken.SessionUserId;
                if (domainId == 0)//handle anonymous login ks
                {
                    var domainResponse = Core.Domains.Module.GetDomainByUser(groupId, userId);
                    if (domainResponse != null && domainResponse.Domain != null)
                    {
                        domainId = Convert.ToInt32(domainResponse.Domain.m_nDomainID);
                    }
                }
            }

            // 8. get the group secret by the session type
            string secret = sessionType == KalturaSessionType.ADMIN ? group.AdminSecret : group.UserSecret;

            // 9. privileges - we do not support it so copy from app token
            var privilagesList = new Dictionary<string, string>();

            if (!privilagesList.ContainsKey(APP_TOKEN_PRIVILEGE_APP_TOKEN))
            {
                privilagesList.Add(APP_TOKEN_PRIVILEGE_APP_TOKEN, appToken.Token);
            }
            if (!privilagesList.ContainsKey(APP_TOKEN_PRIVILEGE_SESSION_ID))
            {
                privilagesList.Add(APP_TOKEN_PRIVILEGE_SESSION_ID, appToken.Token);
            }

            if (!string.IsNullOrEmpty(appToken.SessionPrivileges))
            {
                var splitedPrivileges = appToken.SessionPrivileges.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitedPrivileges != null && splitedPrivileges.Length > 0)
                {
                    foreach (var privilige in splitedPrivileges)
                    {
                        var splitedPrivilege = privilige.Split(':');
                        if (splitedPrivilege != null && splitedPrivilege.Length > 0 && !privilagesList.ContainsKey(splitedPrivilege[0]))
                        {
                            if (splitedPrivilege.Length == 2)
                            {
                                privilagesList.Add(splitedPrivilege[0], splitedPrivilege[1]);
                            }
                            else
                            {
                                privilagesList.Add(splitedPrivilege[0], null);
                            }
                        }
                    }
                }
            }

            // set payload data
            var regionId = Core.Catalog.CatalogLogic.GetRegionIdOfDomain(groupId, domainId, userId);
            var userRoles = ClientsManager.UsersClient().GetUserRoleIds(groupId, userId);
            var userSegments = new List<long>();
            var userSegmentsResponse = Core.Api.Module.GetUserSegments(groupId, userId, 0, 0);
            if (userSegmentsResponse.HasObjects())
            {
                userSegments.AddRange(userSegmentsResponse.Objects.Select(x => x.SegmentId));
            }
            
            log.Debug($"StartSessionWithAppToken - regionId: {regionId} for id: {id}");
            var ksData = new KS.KSData(udid, (int)DateUtils.GetUtcUnixTimestampNow(), regionId, userSegments, userRoles);
            if (!UpdateUsersSessionsRevocationTime(group, userId, udid, ksData.CreateDate, (int)sessionDuration))
            {
                log.ErrorFormat("GenerateSession: Failed to store updated users sessions, userId = {0}", userId);
                throw new InternalServerErrorException();
            }

            // 10. build the ks:
            var payload = KSUtils.PrepareKSPayload(ksData);
            KS ks = new KS(secret, groupId.ToString(), userId, (int)sessionDuration, sessionType, payload, privilagesList, KS.KSVersion.V2);

            // 11. build the response from the ks:
            response = new KalturaSessionInfo(ks);

            return response;
        }

        internal static KalturaAppToken AddAppToken(KalturaAppToken appToken, int groupId)
        {
            // validate partner id
            appToken.PartnerId = groupId;

            // we currently not support app token without user
            if (string.IsNullOrEmpty(appToken.SessionUserId))
            {
                appToken.SessionUserId = KS.GetFromRequest().UserId;
            }

            // 1. generate id for the appToken
            appToken.Id = Utils.Utils.Generate32LengthGuid();
            Group group = GroupsManager.GetGroup(groupId);

            if (string.IsNullOrEmpty(group.AppTokenKeyFormat) || group.AppTokenSessionMaxDurationSeconds == 0 || group.AppTokenMaxExpirySeconds == 0)
            {
                log.ErrorFormat("AddAppToken: missing configuration parameters for partner id = {0}", groupId);
                throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "Partner");
            }

            string appTokenCbKey = string.Format(group.AppTokenKeyFormat, appToken.Id);

            // 2. generate and set the token - guid 32
            appToken.Token = Utils.Utils.Generate32LengthGuid();

            // 3. set default values for empty properties
            List<long> userRoles = RolesManager.GetRoleIds(KS.GetFromRequest(), false);

            int utcNow = (int)DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            if (appToken.getExpiry() == 0 && userRoles.Count(ur => ur > RolesManager.MASTER_ROLE_ID) == 0)
            {
                appToken.Expiry = utcNow + group.AppTokenMaxExpirySeconds;
            }

            appToken.CreateDate = utcNow;
            appToken.UpdateDate = utcNow;

            // session duration
            if (appToken.SessionDuration == null || appToken.SessionDuration <= 0 || appToken.SessionDuration > group.AppTokenSessionMaxDurationSeconds)
            {
                appToken.SessionDuration = group.AppTokenSessionMaxDurationSeconds;
            }

            // session type - currently can be only USER
            appToken.SessionType = KalturaSessionType.USER;

            // default hash type is SHA-256
            if (!appToken.HashType.HasValue)
            {
                appToken.HashType = KalturaAppTokenHashType.SHA256;
            }

            // status - status deleted id not supported (when a token is deleted its deleted from CB) and is default value if status is omitted in request while default value should be active
            if (appToken.Status == KalturaAppTokenStatus.DELETED)
            {
                appToken.Status = KalturaAppTokenStatus.ACTIVE;
            }

            // 4. save in CB
            AppToken cbAppToken = new AppToken(appToken);
            int appTokenExpiryInSeconds = appToken.getExpiry() > 0 ? appToken.getExpiry() - (int)utcNow : 0;

            if (!DAL.UtilsDal.SaveObjectInCB(CB_SECTION_NAME, appTokenCbKey, cbAppToken, true, (uint)appTokenExpiryInSeconds))
            {
                log.ErrorFormat("GenerateSession: Failed to store refreshed token");
                throw new InternalServerErrorException();
            }

            return appToken;
        }

        internal static KalturaAppToken GetAppToken(string id, int groupId)
        {
            KalturaAppToken response = null;

            Group group = GroupsManager.GetGroup(groupId);

            string appTokenCbKey = string.Format(group.AppTokenKeyFormat, id);
            var cbAppToken = cbManager.Get<AppToken>(appTokenCbKey, true);
            if (cbAppToken == null || !cbAppToken.PartnerId.Equals(groupId))
            {
                log.ErrorFormat("GetAppToken: failed to get AppToken from CB, key = {0}", appTokenCbKey);
                throw new NotFoundException(NotFoundException.OBJECT_ID_NOT_FOUND, "Application-token", id);
            }

            response = new KalturaAppToken(cbAppToken);

            return response;
        }

        internal static bool DeleteAppToken(string id, int groupId)
        {
            bool response = false;
            Group group = GroupsManager.GetGroup(groupId);
            string appTokenCbKey = string.Format(group.AppTokenKeyFormat, id);

            var appToken = GetAppToken(id, groupId);
            if (appToken == null)
            {
                log.ErrorFormat("GetAppToken: failed to get AppToken from CB, key = {0}", appTokenCbKey);
                throw new NotFoundException(NotFoundException.OBJECT_ID_NOT_FOUND, "Application-token", id);
            }

            string userId = KS.GetFromRequest().UserId;
            if (appToken.SessionUserId.CompareTo(userId) != 0 && !RolesPermissionsManager.IsPermittedPermission(groupId, userId, RolePermissions.DELETE_ALL_APP_TOKENS))
            {
                // Because the user is not allowed to get or delete app-tokens that owned and created by other users, we throw object not found on purpose.
                throw new NotFoundException(NotFoundException.OBJECT_ID_NOT_FOUND, "Application-token", id);
            }

            response = cbManager.Remove(appTokenCbKey);
            if (!response)
            {
                log.ErrorFormat("DeleteAppToken: failed to get AppToken from CB, key = {0}", appTokenCbKey);
                throw new InternalServerErrorException();
            }

            string revokedSessionKeyFormat = GetRevokedSessionKeyFormat(group);
            string revokedSessionCbKey = string.Format(revokedSessionKeyFormat, appToken.Token);
            long revokedSessionTime = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
            long revokedSessionExpiryInSeconds = group.RefreshExpirationForPinLoginSeconds;
            if (appToken.getSessionDuration() > 0 && appToken.getSessionDuration() < revokedSessionExpiryInSeconds)
                revokedSessionExpiryInSeconds = appToken.getSessionDuration();

            cbManager.Add(revokedSessionCbKey, revokedSessionTime, (uint)revokedSessionExpiryInSeconds, true);

            return response;
        }

        #endregion

        internal static bool LogOut(KS ks)
        {
            if (!string.IsNullOrEmpty(ks.UserId) && ks.UserId != "0")
            {
                Group group = GroupsManager.GetGroup(ks.GroupId);
                int revokedKsMaxTtlSeconds = GetRevokedKsMaxTtlSeconds(group);
                string revokedKsKeyFormat = GetRevokedKsKeyFormat(group);
                var payload = KSUtils.ExtractKSPayload();

                var revokedToken = new ApiToken()
                {
                    GroupID = ks.GroupId,
                    AccessTokenExpiration = DateUtils.DateTimeToUtcUnixTimestampSeconds(ks.Expiration),
                    KS = ks.ToString(),
                    Udid = payload.UDID,
                    UserId = ks.UserId,
                    RegionId = payload.RegionId,
                    UserSegments = payload.UserSegments,
                    UserRoles = payload.UserRoles
                };

                string revokedKsCbKey = string.Format(revokedKsKeyFormat, EncryptionUtils.HashMD5(ks.ToString()));

                uint expiration = (uint)(revokedToken.RefreshTokenExpiration - DateUtils.GetUtcUnixTimestampNow());
                if (revokedKsMaxTtlSeconds > 0 && revokedKsMaxTtlSeconds < expiration)
                {
                    expiration = (uint)revokedKsMaxTtlSeconds;
                }

                if (!cbManager.Add(revokedKsCbKey, revokedToken, expiration, true))
                {
                    log.ErrorFormat("LogOut: Failed to store revoked KS");
                    throw new InternalServerErrorException();
                }

            }
            return true;
        }

        private static string GetRevokedKsKeyFormat(Group group)
        {
            string revokedKsKeyFormat = group.RevokedKsKeyFormat;
            if (string.IsNullOrEmpty(revokedKsKeyFormat))
            {
                revokedKsKeyFormat = ApplicationConfiguration.Current.AuthorizationManagerConfiguration.RevokedKSKeyFormat.Value;
            }

            return revokedKsKeyFormat;
        }

        private static string GetRevokedSessionKeyFormat(Group group)
        {
            string revokedSessionKeyFormat = group.RevokedSessionKeyFormat;
            if (string.IsNullOrEmpty(revokedSessionKeyFormat))
            {
                revokedSessionKeyFormat = ApplicationConfiguration.Current.AuthorizationManagerConfiguration.RevokedSessionKeyFormat.Value;

                if (string.IsNullOrEmpty(revokedSessionKeyFormat))
                {
                    revokedSessionKeyFormat = REVOKED_SESSION_KEY_FORMAT;
                }
            }

            return revokedSessionKeyFormat;
        }

        private static int GetRevokedKsMaxTtlSeconds(Group group)
        {
            return group.RevokedKsMaxTtlSeconds == 0 ? ApplicationConfiguration.Current.AuthorizationManagerConfiguration.RevokedKSMaxTTLSeconds.Value : group.RevokedKsMaxTtlSeconds;
        }

        internal static bool RevokeSessions(int groupId, string userId)
        {
            Group group = GroupsManager.GetGroup(groupId);

            if (!UpdateUsersSessionsRevocationTime(group, userId, string.Empty, (int)DateUtils.GetUtcUnixTimestampNow(), 0, true))
            {
                log.ErrorFormat("RevokeKs: Failed to store users sessions");
                throw new InternalServerErrorException();
            }

            return true;
        }

        internal static bool IsKsValid(KS ks, bool validateExpiration = true)
        {
            // Check if KS already validated by gateway
            string ksRandomHeader = HttpContext.Current.Request.Headers["X-Kaltura-KS-Random"];
            if (ksRandomHeader == ks.Random)
            {
                return true;
            }

            if (validateExpiration && ks.Expiration < DateTime.UtcNow)
            {
                return false;
            }

            Group group = GroupsManager.GetGroup(ks.GroupId);

            if (!string.IsNullOrEmpty(ks.UserId) && ks.UserId != "0")
            {
                string revokedKsKeyFormat = GetRevokedKsKeyFormat(group);

                string revokedKsCbKey = string.Format(revokedKsKeyFormat, EncryptionUtils.HashMD5(ks.ToString()));

                ApiToken revokedToken = cbManager.Get<ApiToken>(revokedKsCbKey, true);
                if (revokedToken != null)
                {
                    return false;
                }

                string userSessionsKeyFormat = GetUserSessionsKeyFormat(group);
                string userSessionsCbKey = string.Format(userSessionsKeyFormat, ks.UserId);
                UserSessions usersSessions = cbManager.Get<UserSessions>(userSessionsCbKey, true);

                if (usersSessions != null)
                {
                    var ksData = KSUtils.ExtractKSPayload(ks);

                    if (usersSessions.UserRevocation > 0)
                    {
                        return ksData.CreateDate >= usersSessions.UserRevocation;
                    }

                    if (!string.IsNullOrEmpty(ksData.UDID) && usersSessions.UserWithUdidRevocations.ContainsKey(ksData.UDID))
                    {
                        return ksData.CreateDate >= usersSessions.UserWithUdidRevocations[ksData.UDID];
                    }
                }
            }
            if (ks.Privileges != null && ks.Privileges.ContainsKey(APP_TOKEN_PRIVILEGE_SESSION_ID))
            {
                string sessionId = ks.Privileges[APP_TOKEN_PRIVILEGE_SESSION_ID];
                string revokedSessionKeyFormat = GetRevokedSessionKeyFormat(group);
                string revokedSessionCbKey = string.Format(revokedSessionKeyFormat, sessionId);

                long revokedSessionTime = cbManager.Get<long>(revokedSessionCbKey, true);
                if (revokedSessionTime > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static string GetUserSessionsKeyFormat(Group group)
        {
            string userSessionsKeyFormat = group.UserSessionsKeyFormat;
            if (string.IsNullOrEmpty(userSessionsKeyFormat))
            {
                userSessionsKeyFormat = ApplicationConfiguration.Current.AuthorizationManagerConfiguration.UsersSessionsKeyFormat.Value;

                if (string.IsNullOrEmpty(userSessionsKeyFormat))
                {
                    userSessionsKeyFormat = USERS_SESSIONS_KEY_FORMAT;
                }
            }

            return userSessionsKeyFormat;
        }

        private static bool UpdateUsersSessionsRevocationTime(Group group, string userId, string udid, int revocationTime, int expiration, bool revokeAll = false)
        {
            if (!string.IsNullOrEmpty(userId) && userId != "0")
            {
                string userSessionsKeyFormat = GetUserSessionsKeyFormat(group);

                // get user sessions from CB
                string userSessionsCbKey = string.Format(userSessionsKeyFormat, userId);

                ulong version;
                UserSessions usersSessions = cbManager.GetWithVersion<UserSessions>(userSessionsCbKey, out version, true);

                // if not found create one
                if (usersSessions == null)
                {
                    usersSessions = new UserSessions()
                    {
                        UserId = userId,
                    };
                }

                // calculate new expiration
                usersSessions.expiration = Math.Max(usersSessions.expiration, expiration);

                if (revokeAll)
                {
                    usersSessions.UserRevocation = revocationTime;

                    long now = DateUtils.GetUtcUnixTimestampNow();
                    usersSessions.expiration = Math.Max(Math.Max(usersSessions.expiration, (int)(now + group.KSExpirationSeconds)), (int)now + group.AppTokenSessionMaxDurationSeconds);
                }
                else
                {
                    if (!string.IsNullOrEmpty(udid))
                    {
                        if (usersSessions.UserWithUdidRevocations.ContainsKey(udid))
                        {
                            usersSessions.UserWithUdidRevocations[udid] = revocationTime;
                        }
                        else
                        {
                            usersSessions.UserWithUdidRevocations.Add(udid, revocationTime);
                        }
                    }
                }

                // store
                if (!cbManager.SetWithVersion<UserSessions>(userSessionsCbKey, usersSessions, version, (uint)(usersSessions.expiration - DateUtils.GetUtcUnixTimestampNow()), true))
                {
                    log.ErrorFormat("LogOut: failed to set UserSessions in CB, key = {0}", userSessionsCbKey);
                    return false;
                }
            }
            return true;
        }

        internal static void RemoveUserSessions(Group group, string userId)
        {
            string userSessionsKeyFormat = GetUserSessionsKeyFormat(group);
            string userSessionsCbKey = string.Format(userSessionsKeyFormat, userId);

            if (!cbManager.Remove(userSessionsCbKey))
            {
                log.ErrorFormat("RemoveUserSessions: failed to remove UserSessions from CB, key = {0}, uderId = {1}", userSessionsCbKey, userId);
            }
        }

        public static KalturaLoginSession SwitchUser(string userId, int groupId, KS.KSData payload, Dictionary<string, string> privileges, Group group)
        {
            KalturaLoginSession loginSession = null;

            // validate user is active
            try
            {
                bool isUserActivated = ClientsManager.UsersClient().IsUserActivated(groupId, userId);
            }
            catch (ClientException ex)
            {
                log.ErrorFormat("SwitchUser: error while getting user. userId = {0}, exception = {1}", userId, ex);
                ErrorUtils.HandleClientException(ex);
            }

            var apiToken = new ApiToken(userId, groupId, payload, false, group, false, privileges);
            loginSession = AuthorizationManager.GenerateSessionByApiToken(apiToken, group);

            return loginSession;
        }

        public static void RevokeDeviceSessions(int groupId, long householdId, string udid)
        {
            List<string> householdUserIds = HouseholdUtils.GetHouseholdUserIds(groupId, true);

            if (householdUserIds == null || householdUserIds.Count == 0)
                return;

            Group group = GroupsManager.GetGroup(groupId);
            long utcNow = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
            long maxSessionDuration = utcNow + Math.Max(group.AppTokenSessionMaxDurationSeconds, group.KSExpirationSeconds);

            foreach (string userId in householdUserIds)
            {
                if (!UpdateUsersSessionsRevocationTime(group, userId, udid, (int)utcNow, (int)maxSessionDuration))
                {
                    log.ErrorFormat("RevokeDeviceSessions: Failed to revoke session for UDID = {0}, userId = {1}", udid, userId);
                }
            }
        }

        private static void ValidateUdid(int groupId, string udid)
        {
            if (string.IsNullOrEmpty(udid))
                return;

            List<string> udids = HouseholdUtils.GetHouseholdUdids(groupId);

            if (udids == null || udids.Contains(udid))
            {
                log.ErrorFormat("ValidateUdid: UDID not found in household. UDID = {0}", udid);
                throw new UnauthorizedException(UnauthorizedException.INVALID_UDID, udid);
            }
        }

        internal static KalturaLoginSession GenerateOvpSession(int groupId)
        {
            Group group = GroupsManager.GetGroup(groupId);

            if (string.IsNullOrEmpty(group.MediaPrepAccountSecret) || group.MediaPrepAccountId == 0)
            {
                throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "Partner");
            }

            return GenerateExternalKs(group.MediaPrepAccountId, group.MediaPrepAccountSecret, group.KSExpirationSeconds);

        }

        public static KalturaLoginSession GenerateExternalKs(int partnerId, string secret, long expiration)
        {
            KalturaLoginSession session = new KalturaLoginSession();

            KS KsObject = new KS(secret,
                partnerId.ToString(),
                string.Empty,
                (int)(DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow.AddSeconds(expiration)) - DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow)),
                KalturaSessionType.ADMIN,
                string.Empty,
                null,
                Models.KS.KSVersion.V2);

            session.KS = KsObject.ToString();
            session.Expiry = DateUtils.DateTimeToUtcUnixTimestampSeconds(KsObject.Expiration);

            return session;
        }
    }
}