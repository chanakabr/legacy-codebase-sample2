using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using WebAPI.Exceptions;
using WebAPI.Models.LiveToVod;

namespace WebAPI.Validation
{
    public class LiveToVodLinearAssetConfigurationValidator : ILiveToVodLinearAssetConfigurationValidator
    {
        private const int MIN_RETENTION_PERIOD_DAYS = 1;
        private const int MAX_RETENTION_PERIOD_DAYS = 999999999;
        
        private static readonly Lazy<LiveToVodLinearAssetConfigurationValidator> Lazy = new Lazy<LiveToVodLinearAssetConfigurationValidator>(
            () => new LiveToVodLinearAssetConfigurationValidator(AssetManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static ILiveToVodLinearAssetConfigurationValidator Instance => Lazy.Value;

        private readonly IAssetManager _assetManager;

        public LiveToVodLinearAssetConfigurationValidator(IAssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        public void Validate(long partnerId, KalturaLiveToVodLinearAssetConfiguration configuration, string argumentName)
        {
            if (!configuration.LinearAssetId.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, $"{argumentName}.linearAssetId");
            }

            ValidateLinearAssetId(partnerId, configuration.LinearAssetId.Value, argumentName);

            if (!configuration.IsLiveToVodEnabled.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, $"{argumentName}.isL2vEnabled");
            }

            if (configuration.RetentionPeriodDays.HasValue)
            {
                if (configuration.RetentionPeriodDays.Value < MIN_RETENTION_PERIOD_DAYS)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, $"{argumentName}.retentionPeriodDays", MIN_RETENTION_PERIOD_DAYS);
                }

                if (configuration.RetentionPeriodDays.Value > MAX_RETENTION_PERIOD_DAYS)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_MAX_VALUE_CROSSED, $"{argumentName}.retentionPeriodDays", MAX_RETENTION_PERIOD_DAYS);
                }
            }
        }

        public void ValidateLinearAssetId(long partnerId, long linearAssetId, string argumentName)
        {            
            var assetResponse = _assetManager.GetAssets(partnerId, new[] { new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, linearAssetId) }, false);
            if (!assetResponse.Any(x => x is LiveAsset))
            {
                throw new ClientException(new Status(eResponseStatus.AssetDoesNotExist));
            }
        }
    }
}