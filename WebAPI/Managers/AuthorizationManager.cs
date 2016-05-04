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

        private static CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(CB_SECTION_NAME, true);


        public static KalturaLoginSession RefreshSession(string refreshToken, string udid = null)
        {
            KS ks = KS.GetFromRequest();
            int groupId = ks.GroupId;

            // validate request parameters
            if (string.IsNullOrEmpty(refreshToken))
            {
                log.ErrorFormat("RefreshSession: Bad request refresh token is empty");
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "refresh token cannot be empty");
            }

            // get group configurations
            Group groupConfig = GetGroupConfiguration(groupId);

            // get token from CB
            string tokenKey = string.Format(groupConfig.TokenKeyFormat, refreshToken);
            ulong version;
            ApiToken token = cbManager.GetWithVersion<ApiToken>(tokenKey, out version, true);
            if (token == null)
            {
                log.ErrorFormat("RefreshSession: refreshToken expired");
                throw new UnauthorizedException((int)WebAPI.Managers.Models.StatusCode.InvalidRefreshToken, "invalid refresh token");
            }

            // validate expired ks
            if (ks.ToString() != token.KS)
            {
                log.ErrorFormat("RefreshSession: invalid ks");
                throw new UnauthorizedException((int)WebAPI.Managers.Models.StatusCode.InvalidKS, "invalid ks");
            }

            string userId = token.UserId;


            // get user
            ValidateUser(groupId, userId);


            // generate new access token with the old refresh token
            token = new ApiToken(token, groupConfig, udid);

            // Store new access + refresh tokens pair
            if (!cbManager.SetWithVersion(tokenKey, token, version, (uint)(token.RefreshTokenExpiration - Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow)), true))
            {
                log.ErrorFormat("RefreshSession: Failed to store refreshed token");
                throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.Error, "failed to refresh token");
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
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "refresh token cannot be empty");
            }

            // get group configurations
            Group groupConfig = GetGroupConfiguration(groupId);

            // generate access token and refresh token pair
            ApiToken token = new ApiToken(userId, groupId, udid, isAdmin, groupConfig, isLoginWithPin);

            string tokenKey = string.Format(groupConfig.TokenKeyFormat, token.RefreshToken);

            // try store in CB, will return false if the same token already exists
            if (!cbManager.Add(tokenKey, token, (uint)(token.RefreshTokenExpiration - Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow)), true))
            {
                log.ErrorFormat("GenerateSession: Failed to store refreshed token");
                throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.Error, "failed to save session");
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
                throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.MissingConfiguration, "missing configuration for partner");
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
                throw new UnauthorizedException((int)WebAPI.Managers.Models.StatusCode.Unauthorized, "user not found");
            }

            // validate user
            KalturaOTTUser user = usersResponse.Where(u => u.Id.ToString() == userId).FirstOrDefault();
            if (user == null || (user.UserState != KalturaUserState.ok && user.UserState != KalturaUserState.user_with_no_household))
            {
                log.ErrorFormat("RefreshAccessToken: user not valid. userId= {0}", userId);
                throw new UnauthorizedException((int)WebAPI.Managers.Models.StatusCode.Unauthorized, "user not valid");
            }
        }

        internal static void CheckAdditionalUserId(string householdUserId, int groupId)
        {
            if (!IsUserInHousehold(householdUserId, groupId))
            {
                throw new ForbiddenException((int)WebAPI.Managers.Models.StatusCode.ServiceForbidden, "additional user is not in household");
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
                ErrorUtils.HandleClientException(ex);
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
                throw new InternalServerErrorException((int)StatusCode.InvalidAppToken, "Invalid application token");
            }

            // 2. check token status
            if (appToken.Status != KalturaAppTokenStatus.ACTIVE)
            {
                log.ErrorFormat("StartSessionWithAppToken: AppToken is not active, id = {0}", id);
                throw new InternalServerErrorException((int)StatusCode.NotActiveAppToken, "application token is not active");
            }

            // 3. calc token hash - (ks + token) hash using the hash type 
            string cbTokenHash = appToken.CalcHash();

            // 4. compare the token hashes
            if (cbTokenHash != tokenHash)
            {
                log.ErrorFormat("StartSessionWithAppToken: token hash is not valid, id = {0}", id);
                throw new InternalServerErrorException((int)StatusCode.InvalidAppTokenHash, "application token hash is not valid");
            }

            // 5. get token expiration:

            // the session duration will be the minimum between the token session duration and the token left expiration time
            long sessionDuration = Math.Min(appToken.SessionDuration, appToken.Expiry - SerializationUtils.GetCurrentUtcTimeInUnixTimestamp());

            // if the minimum is < 0 - token is expired (not possible in our case - will be deleted from CB before)
            if (sessionDuration < 0)
            {
                log.ErrorFormat("StartSessionWithAppToken: AppToken expired, id = {0}", id);
                throw new InternalServerErrorException((int)StatusCode.ExpiredAppToken, "application token is expired");
            }

            // if expiry was supplied - take the minimum
            if (expiry != null && expiry.Value > 0)
            {
                sessionDuration = Math.Min(sessionDuration, expiry.Value);
            }

            // set udid in payload
            string payload = KSUtils.PrepareKSPayload(new WebAPI.Managers.Models.KS.KSData()
            {
                UDID = udid
            });

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
            string privileges = appToken.SessionPrivileges;

            // 10. build the ks:
            KS ks = new KS(
                secret,
                groupId.ToString(),
                userId,
                (int)sessionDuration,
                sessionType,
                payload,
                privileges,
                Models.KS.KSVersion.V2);

            // 11. build the response from the ks:
            response = new KalturaSessionInfo(ks);

            return response;
        }

        internal static KalturaAppToken AddAppToken(KalturaAppToken appToken, int groupId)
        {
            // validate partner id
            if (appToken.PartnerId == 0)
            {
                throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.PartnerInvalid, "partner identifier cannot be 0");
            }

            // we currently not support app token without user
            if (string.IsNullOrEmpty(appToken.SessionUserId))
            {
                throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.UserIDInvalid, "user identifier cannot be empty");
            }

            // 1. generate id for the appToken
            appToken.Id = Utils.Utils.Generate32LengthGuid();
            Group group = GroupsManager.GetGroup(groupId);

            if (string.IsNullOrEmpty(group.AppTokenKeyFormat) || group.AppTokenSessionMaxDurationSeconds == 0 || group.AppTokenMaxExpirySeconds == 0)
            {
                log.ErrorFormat("AddAppToken: missing configuration parameters for partner id = {0}", groupId);
                throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.MissingConfiguration, "missing configuration parameters for partner");
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

            // expiry
            long maxExpiry = Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(group.AppTokenMaxExpirySeconds));
            if (appToken.Expiry == null || appToken.Expiry <= 0 || appToken.Expiry > maxExpiry)
            {
                appToken.Expiry = (int)maxExpiry;
            }

            // session type - currently can be only USER
            appToken.SessionType = KalturaSessionType.USER;

            // privileges - we currently not support privileges - but for future OVP aligning...
            appToken.SessionPrivileges = string.Format("{0}:{1},{2}:{3}", APP_TOKEN_PRIVILEGE_APP_TOKEN, appToken.Token, APP_TOKEN_PRIVILEGE_SESSION_ID, appToken.Token);

            // status - status deleted id not supported (when a token is deleted its deleted from CB) and is default value if status is omitted in request while default value should be active
            if (appToken.Status == KalturaAppTokenStatus.DELETED)
            {
                appToken.Status = KalturaAppTokenStatus.ACTIVE;
            }

            // 4. save in CB
            AppToken cbAppToken = new AppToken(appToken);

            int appTokenExpiryInSeconds = appToken.getExpiry() - (int)Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow);
            if (!cbManager.Add(appTokenCbKey, cbAppToken, (uint)appTokenExpiryInSeconds, true))
            {
                log.ErrorFormat("GenerateSession: Failed to store refreshed token");
                throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.Error, "failed to save application token");
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
                throw new InternalServerErrorException((int)StatusCode.InvalidAppToken, "Invalid application token");
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
                throw new InternalServerErrorException((int)StatusCode.InvalidAppToken, "Invalid application token");
            }

            response = cbManager.Remove(appTokenCbKey);
            if (!response)
            {
                log.ErrorFormat("DeleteAppToken: failed to get AppToken from CB, key = {0}", appTokenCbKey);
                throw new InternalServerErrorException((int)StatusCode.Error, "failed to delete application token");
            }

            return response;
        }
    }
}