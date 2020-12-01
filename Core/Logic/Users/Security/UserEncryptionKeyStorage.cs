using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects;
using Core.Users.Cache;

namespace ApiLogic.Users.Security
{
    public class UserEncryptionKeyStorage : IUserEncryptionKeyStorage
    {
        public static IUserEncryptionKeyStorage Instance() => _instance.Value;
        private static readonly Lazy<IUserEncryptionKeyStorage> _instance = new Lazy<IUserEncryptionKeyStorage>(() => 
            new UserEncryptionKeyStorage(UserEncryptionKeyCache.Instance(), new ThreadLocal<Random>(() => new Random())), LazyThreadSafetyMode.PublicationOnly);

        private readonly UserEncryptionKeyCache _cache;
        private readonly ThreadLocal<Random> _random;

        public UserEncryptionKeyStorage(UserEncryptionKeyCache cache, ThreadLocal<Random> random)
        {
            _cache = cache;
            _random = random;
        }

        public byte[] GenerateRandomEncryptionKey(EncryptionType encryptionType)
        {
            int keySize;
            switch (encryptionType)
            {
                case EncryptionType.aes256: keySize = 32; break;
                default: throw new NotSupportedException();
            }
            
            var key = new byte[keySize];
            _random.Value.NextBytes(key);
            return key;
        }

        public bool AddEncryptionKey(EncryptionKey newKey, long updaterId)
        {
            var newRecordId = DAL.UsersDal.InsertEncryptionKey(newKey, updaterId);
            var success = newRecordId > 0;
            if (success)
            {
                _cache.RemoveEncryptionKeys(newKey.GroupId);
            }
            return success;
        }

        public (bool, EncryptionKey) GetEncryptionKey(int groupId)
        {
            var key = GetEncryptionKeys(groupId).FirstOrDefault();
            return (key != null, key);
        }

        private IEnumerable<EncryptionKey> GetEncryptionKeys(int groupId)
        {
            var encryptionKeys = _cache.GetEncryptionKeys(groupId);
            if (encryptionKeys != null) return encryptionKeys;
                
            encryptionKeys = DAL.UsersDal.GetEncryptionKeys(groupId);
            if (encryptionKeys.Any())
            {
                _cache.SetEncryptionKeys(groupId, encryptionKeys);
            }

            return encryptionKeys;
        }
    }
}
