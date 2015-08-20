using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using Couchbase.Extensions;
using Enyim.Caching.Memcached.Results;
using WebAPI.ClientManagers.Client;
using WebAPI.Models.Users;
using WebAPI.Utils;
using Couchbase;

namespace WebAPI.Managers
{
    public class AuthorizationManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static CouchbaseClient couchbaseClient = CouchbaseManager.GetInstance(CouchbaseBucket.Tokens);

        public static KalturaLoginSession RefreshSession(string refreshToken, int groupId, string udid = null)
        {
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
            IGetOperationResult<ApiToken> tokenRes = couchbaseClient.ExecuteGetJson<ApiToken>(tokenKey);
            if (tokenRes == null || tokenRes.Success != true || tokenRes.HasValue != true || tokenRes.Value == null)
            {
                log.ErrorFormat("RefreshSession: refreshToken expired.");
                throw new UnauthorizedException((int)WebAPI.Managers.Models.StatusCode.ExpiredRefreshToken, "not recognized refresh toke"); 
            }

            ApiToken token = tokenRes.Value;
            string userId = token.UserId;

            // get user
            ValidateUser(groupId, userId);
             

            // generate new access token with the old refresh token
             token = new ApiToken(token, groupConfig, udid);

            // Store new access + refresh tokens pair
            if (!couchbaseClient.CasJson(Enyim.Caching.Memcached.StoreMode.Set, tokenKey, token, tokenRes.Cas, new TimeSpan(0, 0, (int)(token.RefreshTokenExpiration - Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow)))))
            {
                log.ErrorFormat("RefreshSession: Failed to store refreshed token");
                throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.Error, "failed to refresh token"); 
            }

            return new KalturaLoginSession() { KS = token.KS, RefreshToken = token.RefreshToken };
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
            if (!couchbaseClient.StoreJson(Enyim.Caching.Memcached.StoreMode.Add, tokenKey, token, 
                new TimeSpan(0, 0, (int)(token.RefreshTokenExpiration - Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow)))))
            {
                log.ErrorFormat("GenerateSession: Failed to store refreshed token");
                throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.Error, "failed to save session");
            }

            return new KalturaLoginSession() { KS = token.KS, RefreshToken = token.RefreshToken };
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
            List<KalturaOTTUser> usersResponse = null;
            try
            {
                usersResponse = ClientsManager.UsersClient().GetUsersData(groupId, new List<int> { int.Parse(userId) });
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
    }
}