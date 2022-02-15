using ApiObjects;
using CachingProvider;
using Phx.Lib.Appconfig;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Core.Users.Cache
{
    public sealed class UserEncryptionKeyCache
    {
        public static UserEncryptionKeyCache Instance() => _instance.Value;
        private static readonly Lazy<UserEncryptionKeyCache> _instance = new Lazy<UserEncryptionKeyCache>(() =>
            new UserEncryptionKeyCache(GetExpiration()), LazyThreadSafetyMode.PublicationOnly);

        private readonly ICachingService inMemoryCache;
        private readonly uint expirationInSeconds;
        
        public UserEncryptionKeyCache(uint expirationInSeconds)
        {
            inMemoryCache = SingleInMemoryCache.GetInstance(InMemoryCacheType.General, expirationInSeconds);
            this.expirationInSeconds = expirationInSeconds;
        }

        public IEnumerable<EncryptionKey> GetEncryptionKeys(int groupId)
        {
            return inMemoryCache.Get<IEnumerable<EncryptionKey>>(Key(groupId));
        }

        public bool SetEncryptionKeys(int groupId, IEnumerable<EncryptionKey> encryptionKeys)
        {
            return inMemoryCache.Add(Key(groupId), encryptionKeys, expirationInSeconds);
        }

        public object RemoveEncryptionKeys(int groupId)
        {
            return inMemoryCache.Remove(Key(groupId));
        }

        private static string Key(int groupId)
        {
            return $"group_encryption_keys_{groupId}";
        }

        private static uint GetExpiration()
        {
            var ttl = ApplicationConfiguration.Current.UserEncryptionKeysCacheConfiguration.TTLSeconds;
            return (uint)((ttl.Value <= 0) ? ttl.GetDefaultValue() : ttl.Value);
        }
    }
}
