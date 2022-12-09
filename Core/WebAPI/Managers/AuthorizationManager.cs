using ApiLogic.Users;
using ApiLogic.Users.Managers;
using ApiLogic.Users.Services;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.CanaryDeployment.Microservices;
using ApiObjects.DataMigrationEvents;
using ApiObjects.Response;
using ApiObjects.User;
using AuthenticationGrpcClientWrapper;
using CachingProvider.LayeredCache;
using CanaryDeploymentManager;
using Phx.Lib.Appconfig;
using Core.Users;
using EventBus.Kafka;
using Grpc.Core;
using Phx.Lib.Log;
using Newtonsoft.Json;
using SessionManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using ApiLogic.Api.Managers;
using TVinciShared;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;
using KalturaRequestContext;
using WebAPI.Controllers;
using AppToken = WebAPI.Managers.Models.AppToken;
using Status = ApiObjects.Response.Status;
using StatusCode = Grpc.Core.StatusCode;

namespace WebAPI.Managers
{
    public class AuthorizationManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string APP_TOKEN_PRIVILEGE_SESSION_ID = "sessionid";
        private const string APP_TOKEN_PRIVILEGE_APP_TOKEN = "apptoken";
        private const string CB_SECTION_NAME = "tokens";
        private const string REVOKED_SESSION_KEY_FORMAT = "r_session_{0}";
        private const string KS_VALIDATION_FALLBACK_EXPIRATION_KEY = "ks_validation_fallback_expiration_{0}";
        private const string IS_KS_VALID_KEY = "is_ks_validated";

        private static CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(CB_SECTION_NAME);
        private static readonly RequestContextUtils RequestContextUtils = new RequestContextUtils();

        public static KalturaLoginSession RefreshSession(string refreshToken, string udid = null)
        {
            KS ks = KS.GetFromRequest();
            int groupId = ks.GroupId;
            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.AuthenticationRefreshToken))
            {
                throw new Exception("This code should not be called, ownership flag of refresh token has been transfered to Authentication Service, Check TCM [MicroservicesClientConfiguration.Authentication.DataOwnershipConfiguration.RefreshToken]");
            }


            // validate request parameters
            if (string.IsNullOrEmpty(refreshToken))
            {
                log.ErrorFormat("RefreshSession: Bad request refresh token is empty");
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "refreshToken");
            }

            if (!IsKsActive(ks))
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
            if (!UpdateUsersSessionsRevocationTime(groupId, group, userId, udid, ksData.CreateDate, token.AccessTokenExpiration))
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

            DeviceRemovalPolicyHandler.Instance.SaveDomainDeviceUsageDate(udid, groupId);
            return new KalturaLoginSession()
            {
                KS = token.KS,
                RefreshToken = token.RefreshToken
            };
        }

        public static KalturaLoginSession GenerateSession(
            string userId,
            int groupId,
            bool isAdmin,
            bool isLoginWithPin,
            int domainId,
            string udid,
            List<long> userRoles,
            Dictionary<string, string> privileges = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                log.ErrorFormat("GenerateSession: userId is missing");
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "userId");
            }

            // get group configurations
            var group = GetGroupConfiguration(groupId);
            var sessionDuration = userId.IsAnonymous() ? group.AnonymousKSExpirationSeconds : group.KSExpirationSeconds; // TODO is it correct expiration?
            var payload = CreateKsPayload(groupId, userId, domainId, udid, userRoles, 0);
            var token = new ApiToken(userId, groupId, payload, isAdmin, group, isLoginWithPin, privileges);

            // generate access token and refresh token pair
            return GenerateSessionByApiToken(token, group);
        }

        private static KS.KSData CreateKsPayload(int groupId, string userId, int domainId, string udid, List<long> userRoles, int createDate)
        {
            var regionId = Core.Catalog.CatalogLogic.GetRegionIdOfUser(groupId, domainId, userId);
            var userSegments = Core.Api.Module.GetUserAndHouseholdSegmentIds(groupId, userId, domainId);

            // SessionCharacteristicKey is initialized only in Auth MS
            string sessionCharacteristicKey = null;

            var isBypassCacheEligible = RolesManager.IsPartner(groupId, userRoles);

            return new KS.KSData(
                udid,
                createDate,
                regionId,
                userSegments,
                userRoles,
                sessionCharacteristicKey,
                domainId,
                isBypassCacheEligible);
        }

        private static KalturaLoginSession GenerateSessionByApiToken(ApiToken token, Group group)
        {
            string tokenKey = string.Format(group.TokenKeyFormat, token.RefreshToken);

            // update the sessions data
            var ksData = KSUtils.ExtractKSPayload(token.KsObject);
            if (!UpdateUsersSessionsRevocationTime(token.GroupID, group, token.UserId, token.Udid, ksData.CreateDate, token.AccessTokenExpiration))
            {
                log.ErrorFormat("GenerateSession: Failed to store updated users sessions, userId = {0}", token.UserId);
                throw new InternalServerErrorException();
            }

            KalturaLoginSession session = new KalturaLoginSession();

            if (group.IsRefreshTokenEnabled)
            {
                // try store in CB, will return false if the same token already exists
                uint refreshTokenExpirationSeconds = (uint)(token.RefreshTokenExpiration - DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow));

                if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(token.GroupID, CanaryDeploymentDataOwnershipEnum.AuthenticationRefreshToken))
                {
                    var authClient = AuthenticationGrpcClientWrapper.AuthenticationClient.GetClientFromTCM();
                    var refreshTokenFromAuthMs = authClient.GenerateRefreshToken(token.GroupID, token.KS, refreshTokenExpirationSeconds);
                    if (refreshTokenFromAuthMs == null)
                    {
                        log.ErrorFormat("GenerateSession: Failed to generate refresh token using authentication microservice");
                        throw new InternalServerErrorException();
                    }

                    token.RefreshToken = refreshTokenFromAuthMs;
                }
                else
                {
                    if (!cbManager.Add(tokenKey, token, refreshTokenExpirationSeconds, true))
                    {
                        log.ErrorFormat("GenerateSession: Failed to store refreshed token");
                        throw new InternalServerErrorException();
                    }

                    SendRefreshTokenCanaryMigrationEvent(token, refreshTokenExpirationSeconds);
                }


                session.RefreshToken = token.RefreshToken;
            }

            session.KS = token.KS;
            session.Expiry = DateUtils.DateTimeToUtcUnixTimestampSeconds(token.KsObject.Expiration);
            DeviceRemovalPolicyHandler.Instance.SaveDomainDeviceUsageDate(token.Udid, token.GroupID);

            return session;
        }

        private static void SendRefreshTokenCanaryMigrationEvent(ApiToken token, uint refreshTokenExpirationSeconds)
        {
            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsEnabledMigrationEvent(token.GroupID, CanaryDeploymentMigrationEvent.RefreshSession))
            {
                var migrationEvent = new ApiObjects.DataMigrationEvents.RefreshToken()
                {
                    Operation = eMigrationOperation.Create,
                    PartnerId = token.GroupID,
                    Token = token.RefreshToken,
                    Udid = token.Udid,
                    UserId = long.Parse(token.UserId),
                    Ks = token.KS,
                    Ttl = refreshTokenExpirationSeconds,
                    ExpirationDate = DateUtils.GetUtcUnixTimestampNow()+refreshTokenExpirationSeconds
                };

                KafkaPublisher.GetFromTcmConfiguration(migrationEvent).Publish(migrationEvent);
            }
        }

        private static Group GetGroupConfiguration(int groupId)
        {
            // get group configurations
            Group groupConfig = GroupsManager.Instance.GetGroup(groupId);
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
            if (userId.IsAnonymous())
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

        internal static KalturaSessionInfo StartSessionWithAppToken(int groupId, 
                                                                    string id, 
                                                                    string tokenHash, 
                                                                    string userId, 
                                                                    string udid, 
                                                                    KalturaSessionType? type, 
                                                                    int? expiry, 
                                                                    int domainId)
        {
            KalturaSessionInfo response = null;

            Group group = GetGroupConfiguration(groupId);

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

            // 5. Get user role ids by userId, by the userId of the token itself
            UsersClient usersClient = ClientsManager.UsersClient();
            var userIdForRoles = appToken.SessionUserId;
            var userRoles = usersClient.GetUserRoleIds(groupId, userIdForRoles);

            // 6. If it is enabled, update AppToken's expiry and save it to CB
            if (group.AutoRefreshAppToken)
            {
                var utcNow = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
                var currentTokenExpiry = appToken.Expiry;
                var newTokenExpiry = (int)GetAppTokenExpiry(utcNow, appToken.Expiry, group.AppTokenMaxExpirySeconds, group.AutoRefreshAppToken, !IsEndUser(groupId, userRoles));
                appToken.Expiry = newTokenExpiry;
                SaveAppToken(utcNow, appTokenCbKey, appToken);
                SendAppTokenCanaryMigrationEvent(eMigrationOperation.Update, new KalturaAppToken(appToken), groupId);
                log.Info($"StartSessionWithAppToken: AppToken id = {id} for userId = {userIdForRoles} expiry updated from {currentTokenExpiry} to {newTokenExpiry}");
            }

            // 7. get token expiration: the session duration will be the minimum between the token session duration and the token left expiration time
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

            // 8. get session type from cb token - user if not defined - we currently support only user
            KalturaSessionType sessionType = KalturaSessionType.USER;

            // 9. get session user id from cb token - if not defined - use the supplied userId

            if (!string.IsNullOrEmpty(appToken.SessionUserId))
            {
                userId = appToken.SessionUserId;
                if (IsAnonymousLoginKs(domainId))//handle anonymous login ks
                {
                    var domainResponse = Core.Domains.Module.GetDomainByUser(groupId, userId);
                    if (domainResponse != null && domainResponse.Domain != null)
                    {
                        domainId = Convert.ToInt32(domainResponse.Domain.m_nDomainID);
                    }
                }
            }
            
            var userStatus = ValidateUser(groupId, userId, usersClient);
            
            if (!group.ApptokenUserValidationDisabled)
            {
                userStatus.ThrowOnError();

                if (group.ShouldCheckDeviceInDomain && IsEndUser(groupId, userRoles))
                {
                    DomainsClient domainsClient = ClientsManager.DomainsClient();
                    ValidateDevice(groupId, userId, udid, domainId, domainsClient);
                }
            }
            else if (userStatus == null || !userStatus.IsOkStatusCode())
            {
                log.Warn($"StartSessionWithAppToken InvalidUser groupId:{groupId}, userId:{userId}");
            }

            // 10. get the group secret by the session type
            string secret = sessionType == KalturaSessionType.ADMIN ? group.AdminSecret : group.UserSecret;

            // 11. privileges - we do not support it so copy from app token
            var privilegesList = new Dictionary<string, string>();

            if (!privilegesList.ContainsKey(APP_TOKEN_PRIVILEGE_APP_TOKEN))
            {
                privilegesList.Add(APP_TOKEN_PRIVILEGE_APP_TOKEN, appToken.Token);
            }
            if (!privilegesList.ContainsKey(APP_TOKEN_PRIVILEGE_SESSION_ID))
            {
                privilegesList.Add(APP_TOKEN_PRIVILEGE_SESSION_ID, appToken.Token);
            }

            if (!string.IsNullOrEmpty(appToken.SessionPrivileges))
            {
                var splitedPrivileges = appToken.SessionPrivileges.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitedPrivileges != null && splitedPrivileges.Length > 0)
                {
                    foreach (var privilige in splitedPrivileges)
                    {
                        var splitedPrivilege = privilige.Split(':');
                        if (splitedPrivilege != null && splitedPrivilege.Length > 0 && !privilegesList.ContainsKey(splitedPrivilege[0]))
                        {
                            if (splitedPrivilege.Length == 2)
                            {
                                privilegesList.Add(splitedPrivilege[0], splitedPrivilege[1]);
                            }
                            else
                            {
                                privilegesList.Add(splitedPrivilege[0], null);
                            }
                        }
                    }
                }
            }

            // set payload data
            var payload = CreateKsPayload(groupId, userId, domainId, udid, userRoles, (int)DateUtils.GetUtcUnixTimestampNow());
            log.Debug($"StartSessionWithAppToken - regionId: {payload.RegionId} for id: {id}");
            if (!UpdateUsersSessionsRevocationTime(groupId, group, userId, udid, payload.CreateDate, sessionDuration))
            {
                log.ErrorFormat("GenerateSession: Failed to store updated users sessions, userId = {0}", userId);
                throw new InternalServerErrorException();
            }

            // 12. build the ks:
            KS ks = new KS(secret, groupId.ToString(), userId, (int)sessionDuration, sessionType, payload, privilegesList, KS.KSVersion.V2);

            // 13. update last login date
            usersClient.UpdateLastLoginDate(groupId, userId);

            // 14. update udid last activity
            DeviceRemovalPolicyHandler.Instance.SaveDomainDeviceUsageDate(udid, groupId);

            // 15. build the response from the ks:
            response = new KalturaSessionInfo(ks);

            return response;
        }

        private static bool IsEndUser(int groupId, List<long> roleIds)
        {
            return !RolesManager.IsPartner(groupId, roleIds);
        }

        private static bool IsAnonymousLoginKs(int domainId)
        {
            return domainId == 0;
        }

        private static readonly HashSet<ResponseStatus> validUserStatus = new HashSet<ResponseStatus> { ResponseStatus.OK, ResponseStatus.UserWithNoDomain, ResponseStatus.UserNotIndDomain, ResponseStatus.UserNotMasterApproved };
        // TODO remove duplication with ValidateUser(int groupId, string userId)
        private static Status ValidateUser(int groupId, string userIdString, UsersClient usersClient)
        {
            var userId = userIdString.ParseUserId(invalidValue: -1);
            if (userId == -1) throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "userId");
            if (userId.IsAnonymous()) return Status.Ok;

            var userStatus = usersClient.GetUserActivationState(groupId, userId);

            if (validUserStatus.Contains(userStatus)) return Status.Ok;

            // ConvertResponseStatusToResponseObject use WrongPasswordOrUserName for ResponseStatus.UserDoesNotExist
            // but we want to return more meaningful status 
            var errorResponse = userStatus == ResponseStatus.UserDoesNotExist
                ? new Status(eResponseStatus.InvalidUser)
                : Core.Users.Utils.ConvertResponseStatusToResponseObject(userStatus);
            return errorResponse;
        }

        private static void ValidateDevice(int groupId, string userIdString, string udid, int domainId, DomainsClient domainsClient)
        {
            var skipValidation = userIdString.IsAnonymous();
            if (skipValidation) return;

            if (domainId <= 0) throw new ClientException(new Status(eResponseStatus.DeviceNotInDomain));
            if (string.IsNullOrEmpty(udid)) throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "udid");

            // throw ClientException if status != OK
            domainsClient.GetDevice(groupId, domainId, udid, userIdString);
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
            Group group = GroupsManager.Instance.GetGroup(groupId);

            if (string.IsNullOrEmpty(group.AppTokenKeyFormat) || group.AppTokenSessionMaxDurationSeconds == 0 || group.AppTokenMaxExpirySeconds == 0)
            {
                log.ErrorFormat("AddAppToken: missing configuration parameters for partner id = {0}", groupId);
                throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "Partner");
            }

            string appTokenCbKey = string.Format(group.AppTokenKeyFormat, appToken.Id);

            // 2. generate and set the token - guid 32
            appToken.Token = Utils.Utils.Generate32LengthGuid();

            // 3. set default values for empty properties
            var utcNow = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
            appToken.Expiry = (int)GetAppTokenExpiry(utcNow, appToken.getExpiry(), group.AppTokenMaxExpirySeconds, false, RequestContextUtils.IsPartnerRequest());
            appToken.CreateDate = utcNow;

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
            var cbAppToken = new AppToken(appToken);
            SaveAppToken(utcNow, appTokenCbKey, cbAppToken);
            SendAppTokenCanaryMigrationEvent(eMigrationOperation.Create, appToken, groupId);

            return appToken;
        }

        private static void SendAppTokenCanaryMigrationEvent(eMigrationOperation op, KalturaAppToken appToken, int groupId)
        {
            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsEnabledMigrationEvent(groupId, CanaryDeploymentMigrationEvent.AppToken))
            {
                var migrationEvent = new ApiObjects.DataMigrationEvents.AppToken()
                {
                    Operation = op,
                    PartnerId = groupId,
                    Id = appToken.Id,
                    Token = appToken.Token,
                    Expiry = appToken.getExpiry(),
                    HashType = appToken.HashType.ToString(),
                    CreateDate = appToken.CreateDate,
                    SessionDuration = appToken.getSessionDuration(),
                    SessionType = appToken.SessionType.ToString(),
                    SessionUserId = appToken.SessionUserId,
                    UpdateDate = appToken.UpdateDate,
                };

                KafkaPublisher.GetFromTcmConfiguration(migrationEvent).Publish(migrationEvent);
            }
        }

        internal static KalturaAppToken GetAppToken(string id, int groupId)
        {
            KalturaAppToken response = null;

            Group group = GroupsManager.Instance.GetGroup(groupId);

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
            Group group = GroupsManager.Instance.GetGroup(groupId);
            string appTokenCbKey = string.Format(group.AppTokenKeyFormat, id);

            var appToken = GetAppToken(id, groupId);
            if (appToken == null)
            {
                log.ErrorFormat("GetAppToken: failed to get AppToken from CB, key = {0}", appTokenCbKey);
                throw new NotFoundException(NotFoundException.OBJECT_ID_NOT_FOUND, "Application-token", id);
            }

            string userId = KS.GetFromRequest().UserId;
            if (appToken.SessionUserId.CompareTo(userId) != 0 && !RolesPermissionsManager.Instance.IsPermittedPermission(groupId, userId, RolePermissions.DELETE_ALL_APP_TOKENS))
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

            SendAppTokenCanaryMigrationEvent(eMigrationOperation.Delete, appToken, groupId);

            string revokedSessionKeyFormat = GetRevokedSessionKeyFormat(group);
            string revokedSessionCbKey = string.Format(revokedSessionKeyFormat, appToken.Token);
            long revokedSessionTime = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
            long revokedSessionExpiryInSeconds = group.RefreshExpirationForPinLoginSeconds;
            if (appToken.getSessionDuration() > 0 && appToken.getSessionDuration() < revokedSessionExpiryInSeconds)
                revokedSessionExpiryInSeconds = appToken.getSessionDuration();

            cbManager.Add(revokedSessionCbKey, revokedSessionTime, (uint)revokedSessionExpiryInSeconds, true);

            SendAppTokenRevocationMigrationEvent(groupId, appToken, revokedSessionTime, revokedSessionExpiryInSeconds);
            
            return response;
        }

        private static void SendAppTokenRevocationMigrationEvent(int groupId, KalturaAppToken appToken, long revokedSessionTime, long revokedSessionExpiryInSeconds)
        {
            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsEnabledMigrationEvent(groupId, CanaryDeploymentMigrationEvent.SessionRevocation))
            {
                var migrationEvent = new RevokeAppTokenSession
                {
                    Operation = eMigrationOperation.Create,
                    GroupId = groupId,
                    Token = appToken.Token,
                    KsExpiry = revokedSessionTime + revokedSessionExpiryInSeconds,
                    SessionRevocationTime = revokedSessionTime,
                };
                KafkaPublisher.GetFromTcmConfiguration(migrationEvent).Publish(migrationEvent);
            }
        }

        #endregion

        internal static bool LogOut(KS ks)
        {
            if (!string.IsNullOrEmpty(ks.UserId) && ks.UserId != "0")
            {
                Group group = GroupsManager.Instance.GetGroup(ks.GroupId);
                int revokedKsMaxTtlSeconds = GetRevokedKsMaxTtlSeconds(group);
                string revokedKsKeyFormat = GetRevokedKsKeyFormat(group);
                var payload = KSUtils.ExtractKSPayload();

                var revokedToken = new ApiToken(ks, payload);

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
                
                SendRevokeKsCanaryMigrationEvent(ks);
            }
            
            return true;
        }

        private static void SendRevokeKsCanaryMigrationEvent(KS ks)
        {
            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsEnabledMigrationEvent(ks.GroupId, CanaryDeploymentMigrationEvent.SessionRevocation))
            {
                var migrationEvent = new RevokeKs
                {
                    Operation = eMigrationOperation.Create,
                    GroupId = ks.GroupId,
                    Ks = ks.ToString(),
                    KsExpiry = TVinciShared.DateUtils.ToUtcUnixTimestampSeconds(ks.Expiration),
                };

                KafkaPublisher.GetFromTcmConfiguration(migrationEvent).Publish(migrationEvent);
            }
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
            Group group = GroupsManager.Instance.GetGroup(groupId);

            if (!UpdateUsersSessionsRevocationTime(groupId, group, userId, string.Empty, DateUtils.GetUtcUnixTimestampNow(), 0, true))
            {
                log.ErrorFormat("RevokeKs: Failed to store users sessions");
                throw new InternalServerErrorException();
            }

            return true;
        }

        internal static bool IsKsExpired(KS ks)
        {
            return ks.Expiration < DateTime.UtcNow;
        }

        private static bool IsKsActive(KS ks)
        {
            // Check if KS already validated by gateway
            string ksRandomHeader = HttpContext.Current.Request.Headers["X-Kaltura-KS-Random"];
            if (ks.IsKsFormat && ksRandomHeader == ks.Random)
            {
                return ValidateKsSignature(ks);
            }
            
            if (ks.IsKsFormat && CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(ks.GroupId, CanaryDeploymentDataOwnershipEnum.AuthenticationSessionRevocation))
            {
                //use cache if not found
                //call GRPC         
                var isKSValid = false;
                var hashedKS = new Core.Users.SHA384Encrypter().Encrypt(ks.ToString(), "");
                var key = LayeredCacheKeys.GetKsValidationResultKey(hashedKS);             
                //add hashed ks invalidation key
                var invalidationKeys = new List<string>() { LayeredCacheKeys.GetValidateKsInvalidationKeyByKs(ks.GroupId,hashedKS) };
                //if udid exists add also as invalidation key
                if (!string.IsNullOrEmpty(KSUtils.ExtractKSPayload().UDID))
                {
                    var invKey = LayeredCacheKeys.GetValidateKSInvalidationKeyByUdid(ks.GroupId,KSUtils.ExtractKSPayload().UDID);
                    invalidationKeys.Add(invKey);
                }

                //if userid exists add also as invalidation key
                if (!string.IsNullOrEmpty(ks.UserId)&& ks.UserId!="0")
                {
                    var invKey = LayeredCacheKeys.GetValidateKSInvalidationKeyByUserID(ks.GroupId, ks.UserId);
                    invalidationKeys.Add(invKey);
                }
                //if sessionid Privilege exists add also as invalidation key
                if (ks.Privileges != null && ks.Privileges.ContainsKey(APP_TOKEN_PRIVILEGE_APP_TOKEN))
                {
                    var invKey = LayeredCacheKeys.GetValidateKSInvalidationKeyByAppTokenToken(ks.GroupId, ks.Privileges[APP_TOKEN_PRIVILEGE_APP_TOKEN]);
                    invalidationKeys.Add(invKey);
                }

                var isSuccess = LayeredCache.Instance.Get<bool>(key, ref isKSValid, GetKsValidationResult, new Dictionary<string, object>() { { "ks", ks.ToString() }, { "ksPartnerId", (long)ks.GroupId } },
                                                        ks.GroupId, LayeredCacheConfigNames.GET_KS_VALIDATION, invalidationKeys);

                
                
                return isSuccess && isKSValid;
            }

            return ValidateKSLegacy(ks);
        }
        
        
        internal static bool IsAuthorized(KS ks, eKSValidation validationState = eKSValidation.All)
        {
            if (HttpContext.Current.Items.ContainsKey(IS_KS_VALID_KEY))
            {
                return (bool)HttpContext.Current.Items[IS_KS_VALID_KEY];
            }
            
            var isAuthotized =  (validationState == eKSValidation.None) ||
                                (validationState == eKSValidation.Expiration && !IsKsExpired(ks)) ||
                                (validationState == eKSValidation.All && ks.IsValid);
            
            if (!HttpContext.Current.Items.ContainsKey(IS_KS_VALID_KEY))
            {
                HttpContext.Current.Items.Add(IS_KS_VALID_KEY, isAuthotized);
            }

            return isAuthotized;
        }
        
        internal static bool IsKsValid(KS ks, bool validateExpiration = true)
        {
            if (HttpContext.Current.Items.ContainsKey(IS_KS_VALID_KEY))
            {
                return (bool)HttpContext.Current.Items[IS_KS_VALID_KEY];
            }

            var isValid = !(validateExpiration && IsKsExpired(ks));
            
            if(isValid) isValid = IsKsActive(ks);
            
            if (!HttpContext.Current.Items.ContainsKey(IS_KS_VALID_KEY))
            {
                HttpContext.Current.Items.Add(IS_KS_VALID_KEY, isValid);
            }

            return isValid;
        }
        
        private static bool ValidateKSLegacy(KS ks)
        {
            Group group = GroupsManager.Instance.GetGroup(ks.GroupId);

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

                if (usersSessions != null && ks.IsKsFormat)
                {
                    var ksData = KSUtils.ExtractKSPayload(ks);

                    // true
                    var sessionAlive = ksData.CreateDate >= usersSessions.UserRevocation;

                    // true
                    var hasSessionWithUdid = !string.IsNullOrEmpty(ksData.UDID) && usersSessions.UserWithUdidRevocations.ContainsKey(ksData.UDID);
                    
                    // false
                    var sessionWithUdidAlive = hasSessionWithUdid && ksData.CreateDate >= usersSessions.UserWithUdidRevocations[ksData.UDID];

                    //true
                    if (usersSessions.UserRevocation > 0)
                    {
                        return sessionAlive && (!hasSessionWithUdid || sessionWithUdidAlive);
                    }

                    if (hasSessionWithUdid)
                    {
                        return sessionWithUdidAlive;
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

        private static Tuple<bool, bool> GetKsValidationResult(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<bool, bool> result = new Dictionary<bool, bool>();
            var authClient = AuthenticationClient.GetClientFromTCM();
            var validationResult = false;

            try
            {
                if (funcParams != null && funcParams.ContainsKey("ks"))
                {
                    var ks = funcParams["ks"] != null ? funcParams["ks"] as string : string.Empty;
                    var ksPartnerId = funcParams["ksPartnerId"] != null ? (long) (funcParams["ksPartnerId"]) : 0L;

                    string ksValidationKey = LayeredCacheKeys.GetKsValidationResultKey(ks);
                    var isValid = authClient.ValidateKs(ks, ksPartnerId);
                    validationResult = isValid;
                    res = true;

                }
            }
            catch (RpcException rpcEx)
            {
                log.Error(string.Format("GetKsValidationResultKey failed params : {0}", string.Join(";", funcParams.Values)), rpcEx);
                if (rpcEx.StatusCode == StatusCode.Unavailable)
                {
                    // only if we could not reach the Auth MS service we will fall back with a valid ks so that this will not hinder 
                    // system usability
                    // also important! do not cache this result so that next time we validate KS we will retry.
                    HttpContext.Current.Items[LayeredCache.DATABASE_ERROR_DURING_SESSION] = true;
                    return Tuple.Create(true, true);
                }
                
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetKsValidationResultKey failed params : {0}", string.Join(";", funcParams.Values)), ex);
            }

            return new Tuple<bool, bool>(validationResult, res);


           
        }

        public static bool ValidateKsSignature(KS ks)
        {
            var group = GroupsManager.Instance.GetGroup(ks.GroupId);
            var signature = KSUtils.ExtractKSPayload(ks).Signature;
            var groupSecrets = ApplicationConfiguration.Current.RequestParserConfiguration.KsSecrets;

            if (!string.IsNullOrEmpty(signature) && group.EnforceGroupsSecret)
            {
                for (int i = groupSecrets.Count - 1; i >= 0; i--) //LIFO
                {
                    var concat = string.Format(EncryptionUtils.SignatureFormat, ks.Random, groupSecrets[i]);
                    var encryptedValue = Encoding.Default.GetString(EncryptionUtils.HashSHA1(concat));
                    if (encryptedValue == signature)
                    {
                        log.Debug($"Matching signature was received by {ks.UserId}, index: {i}");
                        return true;
                    }
                    log.Info($"Signature validation failed for user: {ks.UserId}, index: {i}");
                }

                return false;
            }
            return true;
        }

        private static string GetUserSessionsKeyFormat(Group group)
        {
            return SessionManager.SessionManager.GetUserSessionsKeyFormat(group.UserSessionsKeyFormat);
        }

        private static bool UpdateUsersSessionsRevocationTime(int groupId, Group group, string userId, string udid, long revocationTime, long expiration, bool revokeAll = false)
        {            
            return SessionManager.SessionManager.UpdateUsersSessionsRevocationTime(groupId, group.UserSessionsKeyFormat,
                 group.AppTokenSessionMaxDurationSeconds, group.KSExpirationSeconds, userId, udid, revocationTime,
                 expiration, revokeAll);
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

        public static void RevokeHouseholdSessions(int groupId, long domainId, string udid = null, List<string> householdUserIds = null)
        {
            try
            {
                if (householdUserIds == null)
                {
                    householdUserIds = HouseholdUtils.GetHouseholdUserIds(groupId, true, (int)domainId);

                    if (householdUserIds == null)
                    {
                        //BEO-9133: If Admin/Operator, should get list by udid
                        householdUserIds = Core.ConditionalAccess.Utils.GetDomainsUsers((int)domainId, groupId)?.Select(x => x.ToString()).ToList();
                    }
                }

                if (householdUserIds == null || householdUserIds.Count == 0)
                    return;

                Group group = GroupsManager.Instance.GetGroup(groupId);
                long utcNow = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
                long maxSessionDuration = utcNow + Math.Max(group.AppTokenSessionMaxDurationSeconds, group.KSExpirationSeconds);
                bool revokeAll = string.IsNullOrEmpty(udid) ? true : false;

                foreach (string userId in householdUserIds)
                {
                    if (!UpdateUsersSessionsRevocationTime(groupId, group, userId, udid, utcNow, (int)maxSessionDuration, revokeAll))
                    {
                        log.ErrorFormat("RevokeDeviceSessions: Failed to revoke session for userId = {0}, UDID = {1}", userId, revokeAll ? "All" : udid);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"RevokeHouseholdSessions error: {ex.Message}", ex);
            }
        }

        internal static KalturaLoginSession GenerateOvpSession(int groupId)
        {
            Group group = GroupsManager.Instance.GetGroup(groupId);

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
                KS.KSData.Empty,
                null,
                Models.KS.KSVersion.V2);

            session.KS = KsObject.ToString();
            session.Expiry = DateUtils.DateTimeToUtcUnixTimestampSeconds(KsObject.Expiration);

            return session;
        }

        private static long GetAppTokenExpiry(long utcNow, long appTokenExpiry, long maxExpirySeconds, bool autoRefreshExpiry, bool isPartner)
        {
            if (isPartner)
            {
                return appTokenExpiry;
            }
            
            if (appTokenExpiry == 0 || appTokenExpiry - utcNow > maxExpirySeconds || autoRefreshExpiry)
            {
                appTokenExpiry = utcNow + maxExpirySeconds;
            }

            return appTokenExpiry;
        }

        private static void SaveAppToken(long utcNow, string appTokenKey, AppToken appToken)
        {
            appToken.UpdateDate = utcNow;

            var appTokenExpiryInSeconds = appToken.Expiry > 0 ? appToken.Expiry - (int)utcNow : 0;
            if (!DAL.UtilsDal.SaveObjectInCB(CB_SECTION_NAME, appTokenKey, appToken, true, (uint)appTokenExpiryInSeconds))
            {
                log.Error($"{nameof(SaveAppToken)}: Failed to store refreshed token.");

                throw new InternalServerErrorException();
            }
        }
    }
}