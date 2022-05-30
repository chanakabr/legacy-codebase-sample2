using System;
using System.Threading;
using WebAPI.Exceptions;
using WebAPI.Models.LiveToVod;

namespace WebAPI.Validation
{
    public class LiveToVodPartnerConfigurationValidator : ILiveToVodPartnerConfigurationValidator
    {
        private const int MIN_RETENTION_PERIOD_DAYS = 1;
        private const int MAX_RETENTION_PERIOD_DAYS = 999999999;
        private const int METADATA_CLASSIFIER_MAX_LENGTH = 50;

        private static readonly Lazy<LiveToVodPartnerConfigurationValidator> Lazy = new Lazy<LiveToVodPartnerConfigurationValidator>(() => new LiveToVodPartnerConfigurationValidator(), LazyThreadSafetyMode.PublicationOnly);

        public static ILiveToVodPartnerConfigurationValidator Instance => Lazy.Value;

        public void Validate(KalturaLiveToVodPartnerConfiguration configuration, string argumentName)
        {
            if (!configuration.IsLiveToVodEnabled.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, $"{argumentName}.isL2vEnabled");
            }

            if (!configuration.RetentionPeriodDays.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, $"{argumentName}.retentionPeriodDays");
            }

            if (configuration.RetentionPeriodDays.Value < MIN_RETENTION_PERIOD_DAYS)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, $"{argumentName}.retentionPeriodDays", MIN_RETENTION_PERIOD_DAYS);
            }

            if (configuration.RetentionPeriodDays.Value > MAX_RETENTION_PERIOD_DAYS)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_VALUE_CROSSED, $"{argumentName}.retentionPeriodDays", MAX_RETENTION_PERIOD_DAYS);
            }

            if (string.IsNullOrEmpty(configuration.MetadataClassifier))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, $"{argumentName}.metadataClassifier");
            }

            if (configuration.MetadataClassifier.Length > METADATA_CLASSIFIER_MAX_LENGTH)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED, $"{argumentName}.metadataClassifier", METADATA_CLASSIFIER_MAX_LENGTH);
            }
        }
    }
}