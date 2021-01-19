using ApiObjects;
using CachingProvider.LayeredCache;
using Core.Users;
using DAL;
using System;
using System.Data;
using System.Threading;

namespace ApiLogic.Users.Security
{
    public class UserStorage : IUserStorage
    {
        private static readonly Lazy<IUserStorage> _instance = new Lazy<IUserStorage>(() =>
            new UserStorage(UserDataEncryptor.Instance()), LazyThreadSafetyMode.PublicationOnly);
        public static IUserStorage Instance() => _instance.Value;

        private readonly IUserDataEncryptor _encryptor;

        public UserStorage(IUserDataEncryptor encryptor)
        {
            _encryptor = encryptor;
        }

        public string GetActivationToken(int groupId, string username)
        {
            var encryptionType = GetEncryptionType(groupId);
            var encryptedUsername = _encryptor.EncryptUsername(groupId, encryptionType, username);
            var token = UsersDal.GetActivationToken(groupId, encryptedUsername);

            #region lazy migration
            if (string.IsNullOrEmpty(token) && encryptionType.HasValue)
            {
                var clearToken = UsersDal.GetActivationToken(groupId, username);
                if (!string.IsNullOrEmpty(clearToken))
                {
                    if (MigrateUsername(username, encryptedUsername, groupId)) token = clearToken;
                }
            }
            #endregion

            return token;
        }

        public UserActivationState GetUserActivationState(int groupId, int activationMustHours, ref string username, ref int userId, ref bool isGracePeriod)
        {
            // there is possible that username and userId are both not empty. userId has higher priority
            var usernameIsInputParameter = userId <= 0;
            if (usernameIsInputParameter)
            {
                var encryptionType = GetEncryptionType(groupId);
                var encryptedUsername = _encryptor.EncryptUsername(groupId, encryptionType, username);
                var result = (UserActivationState)UsersDal.GetUserActivationState(groupId, activationMustHours, ref encryptedUsername, ref userId, ref isGracePeriod);

                #region lazy migration
                if (result == UserActivationState.UserDoesNotExist && encryptionType.HasValue)
                {
                    var clearResult = (UserActivationState)UsersDal.GetUserActivationState(groupId, activationMustHours, ref username, ref userId, ref isGracePeriod);
                    if (clearResult != UserActivationState.UserDoesNotExist)
                    {
                        if (MigrateUsername(username, encryptedUsername, groupId)) result = clearResult;
                    }
                }
                #endregion
                return result;
            }
            else
            {
                username = null; // in this case we will receive username from DB by userId
                var result = (UserActivationState)UsersDal.GetUserActivationState(groupId, activationMustHours, ref username, ref userId, ref isGracePeriod);
                if (!string.IsNullOrEmpty(username)) // receive username from DB
                {
                    var encryptionType = GetEncryptionType(groupId);
                    var decryptedUsername = _encryptor.DecryptUsername(groupId, encryptionType, username);
                    #region lazy migration
                    if (decryptedUsername == username && encryptionType.HasValue) //encryption enabled but username is not encrypted
                    {
                        var encryptedUsername = _encryptor.EncryptUsername(groupId, encryptionType, username);
                        MigrateUsername(username, encryptedUsername, groupId);
                    }
                    #endregion
                    username = decryptedUsername;
                }
                return result;
            }
        }

        public int GetUserIDByUsername(string username, int groupId)
        {
            var encryptionType = GetEncryptionType(groupId);
            var encryptedUsername = _encryptor.EncryptUsername(groupId, encryptionType, username);
            var userId = UsersDal.GetUserIDByUsername(encryptedUsername, groupId);

            #region lazy migration
            if (userId == 0 && encryptionType.HasValue)
            {
                var clearUserId = UsersDal.GetUserIDByUsername(username, groupId);
                if (clearUserId != 0)
                {
                    if (MigrateUsername(username, encryptedUsername, groupId)) userId = clearUserId;
                }
            }
            #endregion

            return userId;
        }

        public int GetUserPasswordFailHistory(string username, int groupId, ref DateTime dNow, ref int failCount, ref DateTime lastFailDate, ref DateTime lastHitDate, ref DateTime passwordUpdateDate)
        {
            var encryptionType = GetEncryptionType(groupId);
            var encryptedUsername = _encryptor.EncryptUsername(groupId, encryptionType, username);
            var userId = UsersDal.GetUserPasswordFailHistory(encryptedUsername, groupId, ref dNow, ref failCount, ref lastFailDate, ref lastHitDate, ref passwordUpdateDate);

            #region lazy migration
            // because of ref parameters we can't generalize lazy-migration to one method, that's why we have code duplication
            if (userId == 0 && encryptionType.HasValue)
            {
                var clearUserId = UsersDal.GetUserPasswordFailHistory(username, groupId, ref dNow, ref failCount, ref lastFailDate, ref lastHitDate, ref passwordUpdateDate);
                if (clearUserId != 0)
                {
                    if (MigrateUsername(username, encryptedUsername, groupId)) userId = clearUserId;
                }
            }
            #endregion

            return userId;
        }

        public bool GenerateToken(int groupId, string username, int tokenValidityHours, out int userId, out string email, out string firstName, out string token)
        {
            userId = 0;
            email = firstName = token = string.Empty;

            var encryptionType = GetEncryptionType(groupId);
            var encryptedUsername = _encryptor.EncryptUsername(groupId, encryptionType, username);
            var dt = UsersDal.GenerateToken(encryptedUsername, groupId, tokenValidityHours);

            #region lazy migration
            if (dt != null && dt.Rows != null && dt.Rows.Count == 0 && encryptionType.HasValue)
            {
                var clearDt = UsersDal.GenerateToken(encryptedUsername, groupId, tokenValidityHours);
                if (clearDt?.Rows?.Count > 0)
                {
                    if (MigrateUsername(username, encryptedUsername, groupId)) dt = clearDt;
                }
            }
            #endregion

            if (dt?.Rows?.Count > 0)
            {
                DataRow dr = dt.Rows[0];

                email = ODBCWrapper.Utils.GetSafeStr(dr["EMAIL_ADD"]);
                userId = ODBCWrapper.Utils.GetIntSafeVal(dr["ID"]);
                firstName = ODBCWrapper.Utils.GetSafeStr(dr["FIRST_NAME"]);
                token = ODBCWrapper.Utils.GetSafeStr(dr["TOKEN"]);
            }

            return userId > 0;
        }

        private EncryptionType? GetEncryptionType(int groupId) => _encryptor.GetUsernameEncryptionType(groupId);

        private bool MigrateUsername(string clearUsername, string encryptedUsername, int groupId)
        {
            var userId = UsersDal.UpdateUsername(groupId, clearUsername, encryptedUsername);
            var success = userId.HasValue;
            if (success)
            {
                var invalidationKey = LayeredCacheKeys.GetUserInvalidationKey(groupId, userId.ToString());
                LayeredCache.Instance.SetInvalidationKey(invalidationKey);
            }
            return success;
        }
    }
}
