using System;
using System.Reflection;
using System.Text;
using System.Threading;
using ApiObjects.Response;
using ApiObjects.User.SessionProfile;
using CachingProvider.LayeredCache;
using CouchbaseManager;
using Phx.Lib.Log;
using TVinciShared;

namespace ApiLogic.Users.Managers
{
    public interface ISessionCharacteristicManager
    {
        string GetOrAdd(int groupId, SessionCharacteristic sessionCharacteristic, uint expirationInSeconds);
        SessionCharacteristic GetFromCache(int groupId, string sessionCharacteristicKey);
    }

    public class SessionCharacteristicManager : ISessionCharacteristicManager
    {
        private static readonly KLogger Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        private readonly ILayeredCache _layeredCache;
        private readonly ICouchbaseManager _couchbaseManager;

        private static readonly Lazy<SessionCharacteristicManager> LazyInstance =
            new Lazy<SessionCharacteristicManager>(() =>
                    new SessionCharacteristicManager(LayeredCache.Instance,
                        new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS)),
                LazyThreadSafetyMode.PublicationOnly);

        public static ISessionCharacteristicManager Instance => LazyInstance.Value;

        public SessionCharacteristicManager(ILayeredCache layeredCache, ICouchbaseManager couchbaseManager)
        {
            _layeredCache = layeredCache;
            _couchbaseManager = couchbaseManager;
        }

        public string GetOrAdd(int groupId, SessionCharacteristic sessionCharacteristic, uint expirationInSeconds)
        {
            var sessionCharacteristicKey = GenerateSessionCharacteristicKey(sessionCharacteristic);
            if (!_couchbaseManager.Set(CouchbaseKey(groupId, sessionCharacteristicKey), sessionCharacteristic,
                expirationInSeconds))
            {
                throw new KalturaException($"unable to save sessionCharacteristic to CB. groupId:[{groupId}]", 1);
            }

            return sessionCharacteristicKey;
        }

        public SessionCharacteristic GetFromCache(int groupId, string sessionCharacteristicKey)
        {
            if (sessionCharacteristicKey.IsNullOrEmpty()) return null;
            
            SessionCharacteristic response = null;
            var cacheResult = _layeredCache.Get( // effective when in-memory only
                LayeredCacheKeys.GetSessionCharacteristic(groupId, sessionCharacteristicKey),
                ref response,
                arg =>
                {
                    var sessionCharacteristic = Get(groupId, sessionCharacteristicKey);
                    return Tuple.Create(sessionCharacteristic, sessionCharacteristic != null);
                },
                null,
                groupId,
                LayeredCacheConfigNames.GET_SESSION_CHARACTERISTIC,
                null,
                true);

            if (!cacheResult)
            {
                Log.Info($"unable to get sessionCharacteristic from cache. groupId:[{groupId}], sessionCharacteristicKey:[{sessionCharacteristicKey}]");
            }

            return response;
        }

        private SessionCharacteristic Get(int groupId, string sessionCharacteristicKey)
        {
            return _couchbaseManager.Get<SessionCharacteristic>(CouchbaseKey(groupId, sessionCharacteristicKey));
        }

        private static string GenerateSessionCharacteristicKey(SessionCharacteristic sessionCharacteristic)
        {
            var sb = new StringBuilder();
            sb
                .Append(sessionCharacteristic.RegionId).Append('_')
                .AppendJoin(";", sessionCharacteristic.UserSegments).Append('_')
                .AppendJoin(";", sessionCharacteristic.UserRoles).Append('_')
                .AppendJoin(";", sessionCharacteristic.UserSessionProfileIds).Append('_');
            return sb.ToString().GetHashCode().ToString("x");
        }

        private static string CouchbaseKey(int groupId, string sessionCharacteristicKey) => $"session_characteristic_{groupId}_{sessionCharacteristicKey}";
    }
}