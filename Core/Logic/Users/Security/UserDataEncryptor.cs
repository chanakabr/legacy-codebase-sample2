using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using TVinciShared;

namespace ApiLogic.Users.Security
{
    public class UserDataEncryptor : IUserDataEncryptor
    {
        private readonly IUserEncryptionKeyStorage _keyStorage;
        private readonly Func<int, GenericResponse<SecurityPartnerConfig>> _securityConfigGetter;

        private static readonly Lazy<IUserDataEncryptor> _instance = new Lazy<IUserDataEncryptor>(() => 
            new UserDataEncryptor(UserEncryptionKeyStorage.Instance(), PartnerConfigurationManager.GetSecurityPartnerConfigFromCache), LazyThreadSafetyMode.PublicationOnly);
        public static IUserDataEncryptor Instance() => _instance.Value;

        public UserDataEncryptor(IUserEncryptionKeyStorage keyStorage, Func<int, GenericResponse<SecurityPartnerConfig>> securityConfigGetter)
        {
            _keyStorage = keyStorage;
            _securityConfigGetter = securityConfigGetter;
        }

        public EncryptionType? GetUsernameEncryptionType(int groupId)
        {
            var securityConfigResponse = _securityConfigGetter(groupId);
            if (!securityConfigResponse.IsOkStatusCode()) throw new Exception("Can't get security configuration for the group");

            var usernameEncryption = securityConfigResponse.Object?.Encryption?.Username;
            if (usernameEncryption == null || !usernameEncryption.IsApplicable()) return null;
            
            return usernameEncryption.EncryptionType;
        }

        public string DecryptUsername(int groupId, EncryptionType? encryptionType, string encryptedUsername)
        {
            if (string.IsNullOrEmpty(encryptedUsername) || encryptionType == null) return encryptedUsername;

            var key = GetKey(groupId);
            return Decrypt(key, encryptedUsername, encryptionType.Value);
        }

        public string DecryptUsername(int groupId, string encryptedUsername)
        {
            if (string.IsNullOrEmpty(encryptedUsername)) return encryptedUsername;

            return DecryptUsername(groupId, GetUsernameEncryptionType(groupId), encryptedUsername);
        }

        public string EncryptUsername(int groupId, EncryptionType? encryptionType, string clearUsername)
        {
            if (string.IsNullOrEmpty(clearUsername) || encryptionType == null) return clearUsername;

            var key = GetKey(groupId);
            return Encrypt(key, clearUsername, encryptionType.Value);
        }

        private byte[] GetKey(int groupId)
        {
            (var success, var key) = _keyStorage.GetEncryptionKey(groupId);
            if (!success) throw new Exception("Can't find encryption key for the group"); // when encryption enabled, key should present always

            return key.Value;
        }

        private static string Encrypt(byte[] key, string username, EncryptionType encryptionType)
        {
            switch (encryptionType)
            {
                case EncryptionType.aes256: return AesEncrypt(username.ToLower(), key);
                default: throw new NotSupportedException("Unknown encryption type");
            }
        }

        private static string Decrypt(byte[] key, string username, EncryptionType encryptionType)
        {
            switch (encryptionType)
            {
                case EncryptionType.aes256: return AesDecrypt(username, key);
                default: throw new NotSupportedException("Unknown encryption type");
            }
        }

        private static string AesEncrypt(string clearUsername, byte[] key)
        {
            var encryptedBytes = EncryptUtils.AesEncrypt(Encoding.UTF8.GetBytes(clearUsername), key);
            return Convert.ToBase64String(encryptedBytes);
        }

        private static string AesDecrypt(string encryptedUsername, byte[] key)
        {
            try
            {
                var decryptedBytes = EncryptUtils.AesDecrypt(Convert.FromBase64String(encryptedUsername), key);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            // means, that it's clear username
            catch (FormatException)
            {
                return encryptedUsername;
            }
            catch (CryptographicException)
            {
                return encryptedUsername;
            }
        }
    }
}
