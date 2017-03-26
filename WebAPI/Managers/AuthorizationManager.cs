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

namespace WebAPI.Managers
{
    public class AuthorizationManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string APP_TOKEN_PRIVILEGE_SESSION_ID = "sessionid";
        private const string APP_TOKEN_PRIVILEGE_APP_TOKEN = "apptoken";
        private const string CB_SECTION_NAME = "tokens";
        private const string REVOKED_KS_MAX_TTL_SECONDS_TCM_KEY = "revoked_ks_max_ttl_seconds";
        private const string USERS_SESSIONS_KEY_FORMAT_TCM_KEY = "users_sessions_key_format";
        private const string REVOKED_KS_KEY_FORMAT_TCM_KEY = "revoked_ks_key_format";
        private const string USERS_SESSIONS_KEY_FORMAT = "sessions_{0}";
        private const string REVOKED_KS_KEY_FORMAT = "r_ks_{0}";

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
            if (!cbManager.SetWithVersion(tokenKey, token, version, (uint)(token.RefreshTokenExpiration - Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow)), true))
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

        public static KalturaLoginSession GenerateSession(string userId, int groupId, bool isAdmin, bool isLoginWithPin, string udid = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                log.ErrorFormat("GenerateSession: userId is missing");
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "userId");
            }

            // get group configurations
            Group group = GetGroupConfiguration(groupId);

            // generate access token and refresh token pair
            ApiToken token = new ApiToken(userId, groupId, udid, isAdmin, group, isLoginWithPin);
            string tokenKey = string.Format(group.TokenKeyFormat, token.RefreshToken);

            // update the sessions data
            var ksData = KSUtils.ExtractKSPayload(token.KsObject);
            if (!UpdateUsersSessionsRevocationTime(group, userId, udid, ksData.CreateDate, (int)token.AccessTokenExpiration))
            {
                log.ErrorFormat("GenerateSession: Failed to store updated users sessions, userId = {0}", userId);
                throw new InternalServerErrorException();
            }

            // try store in CB, will return false if the same token already exists
            if (!cbManager.Add(tokenKey, token, (uint)(token.RefreshTokenExpiration - Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow)), true))
            {
                log.ErrorFormat("GenerateSession: Failed to store refreshed token");
                throw new InternalServerErrorException();
            }

            return new KalturaLoginSession()
            {
                KS = token.KS,
                RefreshToken = token.RefreshToken
            };
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
                ((household.Users != null && household.Users.Where(u => u.Id == userId).FirstOrDefault() != null) ||
                (household.DefaultUsers != null && household.DefaultUsers.Where(u => u.Id == userId).FirstOrDefault() != null) ||
                (household.MasterUsers != null && household.MasterUsers.Where(u => u.Id == userId).FirstOrDefault() != null) ||
                (household.PendingUsers != null && household.PendingUsers.Where(u => u.Id == userId).FirstOrDefault() != null)))
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

        internal static KalturaSessionInfo StartSessionWithAppToken(int groupId, string id, string tokenHash, string userId, string udid, KalturaSessionType? type, int? expiry)
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

            // 5. get token expiration:

            // the session duration will be the minimum between the token session duration and the token left expiration time
            long sessionDuration = 0;
            if (appToken.Expiry > 0)
            {
                sessionDuration = Math.Min(appToken.SessionDuration, appToken.Expiry - SerializationUtils.GetCurrentUtcTimeInUnixTimestamp());
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

            // set udid in payload
            WebAPI.Managers.Models.KS.KSData ksData = new WebAPI.Managers.Models.KS.KSData()
            {
                UDID = udid, 
                CreateDate = (int)SerializationUtils.GetCurrentUtcTimeInUnixTimestamp()
            };
            string payload = KSUtils.PrepareKSPayload(ksData);

            // 6. get session type from cb token - user if not defined - we currently support only user
            KalturaSessionType sessionType = KalturaSessionType.USER;

            // 7. get session user id from cb token - if not defined - use the supplied userId
            if (!string.IsNullOrEmpty(appToken.SessionUserId))
            {
                userId = appToken.SessionUserId;
            }

            // 8. get the group secret by the session type
            string secret = sessionType == KalturaSessionType.ADMIN ? group.AdminSecret : group.UserSecret;

            // 9. privileges - we do not support it so copy from app token
            var privilagesList = new List<KalturaKeyValue>();

            privilagesList.Add(new KalturaKeyValue() { key = APP_TOKEN_PRIVILEGE_APP_TOKEN, value = appToken.Token });
            privilagesList.Add(new KalturaKeyValue() { key = APP_TOKEN_PRIVILEGE_SESSION_ID, value = appToken.Token });

            if (!string.IsNullOrEmpty(appToken.SessionPrivileges))
            {
                var splitedPrivileges = appToken.SessionPrivileges.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitedPrivileges != null && splitedPrivileges.Length > 0)
                {
                    foreach (var privilige in splitedPrivileges)
                    {
                        var splitedPrivilege = privilige.Split(':');
                        if (splitedPrivilege != null && splitedPrivilege.Length > 0)
                        {
                            if (splitedPrivilege.Length == 2)
                            {
                                privilagesList.Add(new KalturaKeyValue() { key = splitedPrivilege[0], value = splitedPrivilege[1] });
                            }
                            else
                            {
                                privilagesList.Add(new KalturaKeyValue() { key = splitedPrivilege[0], value = null });
                            }
                        }
                    }
                }
            }

            if (!UpdateUsersSessionsRevocationTime(group, userId, udid, ksData.CreateDate, (int)sessionDuration))
            {
                log.ErrorFormat("GenerateSession: Failed to store updated users sessions, userId = {0}", userId);
                throw new InternalServerErrorException();
            }

            // 10. build the ks:
            KS ks = new KS(
                secret,
                groupId.ToString(),
                userId,
                (int)sessionDuration,
                sessionType,
                payload,
                privilagesList,
                Models.KS.KSVersion.V2);

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
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaAppToken.SessionUserId");
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
            // session duration
            if (appToken.SessionDuration == null || appToken.SessionDuration <= 0 || appToken.SessionDuration > group.AppTokenSessionMaxDurationSeconds)
            {
                appToken.SessionDuration = group.AppTokenSessionMaxDurationSeconds;
            }

            // session type - currently can be only USER
            appToken.SessionType = KalturaSessionType.USER;

            // status - status deleted id not supported (when a token is deleted its deleted from CB) and is default value if status is omitted in request while default value should be active
            if (appToken.Status == KalturaAppTokenStatus.DELETED)
            {
                appToken.Status = KalturaAppTokenStatus.ACTIVE;
            }

            // 4. save in CB
            AppToken cbAppToken = new AppToken(appToken);
            int appTokenExpiryInSeconds = appToken.getExpiry() > 0 ? appToken.getExpiry() - (int)Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow) : 0;
            if (!cbManager.Add(appTokenCbKey, cbAppToken, (uint)appTokenExpiryInSeconds, true))
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
            if (cbAppToken == null)
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
            var appToken = cbManager.Get<KalturaAppToken>(appTokenCbKey, true);
            if (appToken == null)
            {
                log.ErrorFormat("GetAppToken: failed to get AppToken from CB, key = {0}", appTokenCbKey);
                throw new NotFoundException(NotFoundException.OBJECT_ID_NOT_FOUND, "Application-token", id);
            }

            response = cbManager.Remove(appTokenCbKey);
            if (!response)
            {
                log.ErrorFormat("DeleteAppToken: failed to get AppToken from CB, key = {0}", appTokenCbKey);
                throw new InternalServerErrorException();
            }

            return response;
        }

        internal static bool LogOut(KS ks)
        {
            if (!string.IsNullOrEmpty(ks.UserId) && ks.UserId != "0")
            {
                Group group = GroupsManager.GetGroup(ks.GroupId);
                int revokedKsMaxTtlSeconds = GetRevokedKsMaxTtlSeconds(group);
                string revokedKsKeyFormat = GetRevokedKsKeyFormat(group);


                ApiToken revokedToken = new ApiToken()
                {
                    GroupID = ks.GroupId,
                    AccessTokenExpiration = SerializationUtils.ConvertToUnixTimestamp(ks.Expiration),
                    KS = ks.ToString(),
                    Udid = KSUtils.ExtractKSPayload().UDID,
                    UserId = ks.UserId
                };

                string revokedKsCbKey = string.Format(revokedKsKeyFormat, EncryptionUtils.HashMD5(ks.ToString()));

                uint expiration = (uint)(revokedToken.RefreshTokenExpiration - SerializationUtils.GetCurrentUtcTimeInUnixTimestamp());
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
                revokedKsKeyFormat = TCMClient.Settings.Instance.GetValue<string>(REVOKED_KS_KEY_FORMAT_TCM_KEY);

                if (string.IsNullOrEmpty(revokedKsKeyFormat))
                {
                    revokedKsKeyFormat = REVOKED_KS_KEY_FORMAT;
                }
            }

            return revokedKsKeyFormat;
        }

        private static int GetRevokedKsMaxTtlSeconds(Group group)
        {
            return group.RevokedKsMaxTtlSeconds == 0 ? TCMClient.Settings.Instance.GetValue<int>(REVOKED_KS_MAX_TTL_SECONDS_TCM_KEY) : group.RevokedKsMaxTtlSeconds;
        }

        internal static bool RevokeSessions(int groupId, string userId)
        {
            Group group = GroupsManager.GetGroup(groupId);

            if (!UpdateUsersSessionsRevocationTime(group, userId, string.Empty, (int)SerializationUtils.GetCurrentUtcTimeInUnixTimestamp(), 0, true))
            {
                log.ErrorFormat("RevokeKs: Failed to store users sessions");
                throw new InternalServerErrorException();
            }

            return true;
        }

        internal static bool IsKsValid(KS ks, bool validateExpiration = true)
        {
            if (validateExpiration && ks.Expiration < DateTime.UtcNow)
            {
                return false;
            }

            Group group = GroupsManager.GetGroup(ks.GroupId);
            string revokedKsKeyFormat = GetRevokedKsKeyFormat(group);
            string userSessionsKeyFormat = GetUserSessionsKeyFormat(group);

            string revokedKsCbKey = string.Format(revokedKsKeyFormat, EncryptionUtils.HashMD5(ks.ToString()));

            ApiToken revokedToken = cbManager.Get<ApiToken>(revokedKsCbKey, true);
            if (revokedToken != null)
            {
                return false;
            }

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

            return true;
        }

        private static string GetUserSessionsKeyFormat(Group group)
        {
            string userSessionsKeyFormat = group.UserSessionsKeyFormat;
            if (string.IsNullOrEmpty(userSessionsKeyFormat))
            {
                userSessionsKeyFormat = TCMClient.Settings.Instance.GetValue<string>(USERS_SESSIONS_KEY_FORMAT_TCM_KEY);

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

                    long now = SerializationUtils.GetCurrentUtcTimeInUnixTimestamp();
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
                if (!cbManager.SetWithVersion<UserSessions>(userSessionsCbKey, usersSessions, version, (uint)(usersSessions.expiration - SerializationUtils.GetCurrentUtcTimeInUnixTimestamp()), true))
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
    }
}