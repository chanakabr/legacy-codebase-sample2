using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using LinqToTwitter;
using LiveToVod.BOL;
using LiveToVod.DAL;
using Microsoft.Extensions.Logging;
using Phx.Lib.Log;

namespace LiveToVod
{
    public class LiveToVodManager : ILiveToVodManager
    {
        private static readonly Lazy<LiveToVodManager> Lazy = new Lazy<LiveToVodManager>(
            () => new LiveToVodManager(Repository.Instance, LiveToVodService.Instance, LayeredCache.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IRepository _repository;
        private readonly ILiveToVodService _liveToVodService;
        private readonly ILayeredCache _layeredCache;
        private readonly ILogger _logger;

        public static ILiveToVodManager Instance => Lazy.Value;

        public LiveToVodManager(IRepository repository, ILiveToVodService liveToVodService, ILayeredCache layeredCache)
            : this(repository, liveToVodService, layeredCache, new KLogger(nameof(LiveToVodManager)))
        {
        }
        
        public LiveToVodManager(
            IRepository repository,
            ILiveToVodService liveToVodService,
            ILayeredCache layeredCache,
            ILogger logger)
        {
            _repository = repository;
            _liveToVodService = liveToVodService;
            _layeredCache = layeredCache;
            _logger = logger;
        }

        public LiveToVodFullConfiguration GetFullConfiguration(long partnerId)
        {
            var partnerConfiguration = GetPartnerConfiguration(partnerId);
            var linearAssetConfigurations = _repository.GetLinearAssetConfigurations(partnerId);

            var result = new LiveToVodFullConfiguration
            {
                IsLiveToVodEnabled = partnerConfiguration.IsLiveToVodEnabled,
                RetentionPeriodDays = partnerConfiguration.RetentionPeriodDays,
                MetadataClassifier = partnerConfiguration.MetadataClassifier,
                LinearAssets = linearAssetConfigurations?.Select(x => CreateOverridenLinearAssetConfiguration(partnerConfiguration, x)).ToList()
            };

            return result;
        }

        public LiveToVodPartnerConfiguration GetPartnerConfiguration(long partnerId)
        {
            var result = _repository.GetPartnerConfiguration(partnerId);
            if (result == null)
            {
                _logger.LogWarning($"{nameof(LiveToVodPartnerConfiguration)} has not been found. The default configuration will be returned. {nameof(partnerId)}={partnerId}.");
                result = new LiveToVodPartnerConfiguration();
            }

            return result;
        }

        public LiveToVodLinearAssetConfiguration GetLinearAssetConfiguration(long partnerId, long linearAssetId)
        {
            var partnerConfiguration = GetPartnerConfiguration(partnerId);
            var linearAssetConfiguration = _repository.GetLinearAssetConfiguration(partnerId, linearAssetId);
            if (linearAssetConfiguration == null)
            {
                _logger.LogWarning($"{nameof(LiveToVodLinearAssetConfiguration)} has not been found. {nameof(partnerId)}={partnerId}, {nameof(linearAssetId)}={linearAssetId}.");
                linearAssetConfiguration = new LiveToVodLinearAssetConfiguration(linearAssetId, false, null);
            }

            var result = CreateOverridenLinearAssetConfiguration(partnerConfiguration, linearAssetConfiguration);

            return result;
        }

        public LiveToVodPartnerConfiguration UpdatePartnerConfiguration(long partnerId, LiveToVodPartnerConfiguration config, long updaterId)
        {
            if (TryUpdatePartnerConfiguration(partnerId, config, updaterId))
            {
                UpsertPartnerConfiguration(partnerId, config, updaterId);
            }

            var partnerConfig = GetPartnerConfiguration(partnerId);

            return partnerConfig;
        }

        public LiveToVodLinearAssetConfiguration UpdateLinearAssetConfiguration(long partnerId, LiveToVodLinearAssetConfiguration config, long updaterId)
        {
            var partnerConfiguration = GetPartnerConfiguration(partnerId);
            if (partnerConfiguration.IsLiveToVodEnabled)
            {
                var isUpdated = _repository.UpsertLinearAssetConfiguration(partnerId, config, updaterId);
                if (!isUpdated)
                {
                    _logger.LogError($"{nameof(UpdateLinearAssetConfiguration)} failed. {nameof(partnerId)}={partnerId}, {nameof(LiveToVodLinearAssetConfiguration.LinearAssetId)}={config.LinearAssetId}, {nameof(updaterId)}={updaterId}.");
                }
                else
                {
                    _layeredCache.SetInvalidationKey(
                        LayeredCacheKeys.GetLiveToVodFullConfigurationInvalidationKey(partnerId));
                }
            }
            else
            {
                _logger.LogWarning($"Update of {nameof(LiveToVodLinearAssetConfiguration)} was skipped because LiveToVod is disabled on partner's level. {nameof(partnerId)}={partnerId}.");
            }

            var linearAssetConfig = GetLinearAssetConfiguration(partnerId, config.LinearAssetId);

            return linearAssetConfig;
        }

        public LiveToVodFullConfiguration GetCachedFullConfiguration(long partnerId)
        {
            try
            {
                LiveToVodFullConfiguration configuration = null;
                var key = LayeredCacheKeys.GetLiveToVodFullConfigurationKey(partnerId);
                var invalidationKeys = new List<string>
                {
                    LayeredCacheKeys.GetLiveToVodFullConfigurationInvalidationKey(partnerId)
                };
                var cacheResult = _layeredCache.Get(
                    key,
                    ref configuration,
                    GetFullConfiguration,
                    new Dictionary<string, object> { { "partnerId", partnerId } },
                    (int)partnerId,
                    LayeredCacheConfigNames.GET_LIVE_TO_VOD_FULL_CONFIGURATION_CACHE_CONFIG_NAME,
                    invalidationKeys);

                if (cacheResult)
                {
                    return configuration;
                }
                
                _logger.LogError($"{nameof(List)} - Failed to get live to vod full configuration: {nameof(partnerId)}={partnerId}.");

                return null;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(List)}: {e.Message}.");

                return null;
            }
        }
        
        private Tuple<LiveToVodFullConfiguration, bool> GetFullConfiguration(Dictionary<string, object> funcParams)
        {
            try
            {
                var partnerId = (long)funcParams["partnerId"];
                var configuration = GetFullConfiguration(partnerId);

                return new Tuple<LiveToVodFullConfiguration, bool>(configuration, true);
            }
            catch (Exception e)
            {
                var parameters = funcParams != null
                    ? string.Join(";", funcParams.Select(x => $"{{key: {x.Key}, value:{x.Value}}}"))
                    : string.Empty;
                _logger.LogError(e, $"Error while executing {nameof(GetFullConfiguration)}({parameters}): {e.Message}.");
            }

            return new Tuple<LiveToVodFullConfiguration, bool>(null, false);
        }


        private bool TryUpdatePartnerConfiguration(long partnerId, LiveToVodPartnerConfiguration config, long updaterId)
        {
            bool result;

            var currentConfig = GetPartnerConfiguration(partnerId);
            if (!currentConfig.IsLiveToVodEnabled && config.IsLiveToVodEnabled)
            {
                result = DoesLiveToVodAssetStructExist(partnerId, updaterId);
            }
            else
            {
                result = true;
            }

            return result;
        }

        private bool DoesLiveToVodAssetStructExist(long partnerId, long updaterId)
        {
            bool result = false;
            var getAssetStructResponse = _liveToVodService.GetLiveToVodAssetStruct((int)partnerId);
            if (getAssetStructResponse.IsOkStatusCode())
            {
                result = true;
            }
            else if (getAssetStructResponse.Status.Code == (int)eResponseStatus.AssetStructDoesNotExist)
            {
                var addAssetStructResponse = _liveToVodService.AddLiveToVodAssetStruct((int)partnerId, updaterId);
                if (!addAssetStructResponse.IsOkStatusCode())
                {
                    throw new Exception($"{addAssetStructResponse.Status.Code} - {addAssetStructResponse.Status.Message}.");
                }

                result = true;
            }

            return result;
        }

        private void UpsertPartnerConfiguration(long partnerId, LiveToVodPartnerConfiguration config, long updaterId)
        {
            var isUpdated = _repository.UpsertPartnerConfiguration(partnerId, config, updaterId);
            if (!isUpdated)
            {
                _logger.LogError($"{nameof(UpdatePartnerConfiguration)} failed. {nameof(partnerId)}={partnerId}, {nameof(updaterId)}={updaterId}.");
            }
            else
            {
                _layeredCache.SetInvalidationKey(
                    LayeredCacheKeys.GetLiveToVodFullConfigurationInvalidationKey(partnerId));
            }
        }

        private static LiveToVodLinearAssetConfiguration CreateOverridenLinearAssetConfiguration(LiveToVodPartnerConfiguration partnerConfig, LiveToVodLinearAssetConfiguration linearAssetConfig)
        {
            return new LiveToVodLinearAssetConfiguration(
                linearAssetConfig.LinearAssetId,
                linearAssetConfig.IsLiveToVodEnabled && partnerConfig.IsLiveToVodEnabled,
                linearAssetConfig.RetentionPeriodDays ?? partnerConfig.RetentionPeriodDays);
        }
    }
}