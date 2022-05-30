using System;
using System.Linq;
using System.Threading;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiObjects.Response;
using LiveToVod.BOL;
using LiveToVod.DAL;
using Microsoft.Extensions.Logging;
using Phx.Lib.Log;

namespace LiveToVod
{
    public class LiveToVodManager : ILiveToVodManager
    {
        private static readonly Lazy<LiveToVodManager> Lazy = new Lazy<LiveToVodManager>(
            () => new LiveToVodManager(Repository.Instance, LiveToVodService.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IRepository _repository;
        private readonly ILiveToVodService _liveToVodService;
        private readonly ILogger _logger;

        public static ILiveToVodManager Instance => Lazy.Value;

        public LiveToVodManager(IRepository repository, ILiveToVodService liveToVodService)
            : this(repository, liveToVodService, new KLogger(nameof(LiveToVodManager)))
        {
        }
        
        public LiveToVodManager(IRepository repository, ILiveToVodService liveToVodService, ILogger logger)
        {
            _repository = repository;
            _liveToVodService = liveToVodService;
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
                linearAssetConfiguration = new LiveToVodLinearAssetConfiguration(linearAssetId, partnerConfiguration.IsLiveToVodEnabled, null);
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
            }
            else
            {
                _logger.LogWarning($"Update of {nameof(LiveToVodLinearAssetConfiguration)} was skipped because LiveToVod is disabled on partner's level. {nameof(partnerId)}={partnerId}.");
            }

            var linearAssetConfig = GetLinearAssetConfiguration(partnerId, config.LinearAssetId);

            return linearAssetConfig;
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