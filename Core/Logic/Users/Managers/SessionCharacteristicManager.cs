using System;
using System.Threading;
using AuthenticationGrpcClientWrapper;
using CachingProvider.LayeredCache;
using OTT.Service.Authentication;
using Phx.Lib.Log;
using TVinciShared;

namespace ApiLogic.Users.Managers
{
    public interface ISessionCharacteristicManager
    {
        GetSessionCharacteristicsResponse GetFromCache(int groupId, string sessionCharacteristicKey);
    }

    public class SessionCharacteristicManager : ISessionCharacteristicManager
    {
        private static readonly KLogger Log = new KLogger(nameof(SessionCharacteristicManager));

        private readonly ILayeredCache _layeredCache;
        private readonly Lazy<IAuthenticationClient> _authenticationClient;

        private static readonly Lazy<SessionCharacteristicManager> LazyInstance =
            new Lazy<SessionCharacteristicManager>(() =>
                    new SessionCharacteristicManager(
                        LayeredCache.Instance,
                        new Lazy<IAuthenticationClient>(AuthenticationClient.GetClientFromTCM, LazyThreadSafetyMode.PublicationOnly)),
                LazyThreadSafetyMode.PublicationOnly);

        public static ISessionCharacteristicManager Instance => LazyInstance.Value;

        public SessionCharacteristicManager(ILayeredCache layeredCache, Lazy<IAuthenticationClient> authenticationClient)
        {
            _layeredCache = layeredCache;
            _authenticationClient = authenticationClient;
        }

        public GetSessionCharacteristicsResponse GetFromCache(int groupId, string sessionCharacteristicKey)
        {
            if (sessionCharacteristicKey.IsNullOrEmpty()) return null;

            GetSessionCharacteristicsResponse response = null;
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

        private GetSessionCharacteristicsResponse Get(int groupId, string sessionCharacteristicId)
            => _authenticationClient.Value.GetSessionCharacteristics(groupId, sessionCharacteristicId);
    }
}