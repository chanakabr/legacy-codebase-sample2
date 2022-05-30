using LiveToVod.BOL;

namespace LiveToVod.DAL
{
    internal static class Mapper
    {
        public static LiveToVodPartnerConfigurationData Map(LiveToVodPartnerConfiguration source, long updaterId)
        {
            if (source == null)
            {
                return null;
            }

            return new LiveToVodPartnerConfigurationData
            {
                Id = LiveToVodPartnerConfigurationData.PARTNER_CONFIG_DOCUMENT_ID,
                IsLiveToVodEnabled = source.IsLiveToVodEnabled,
                RetentionPeriodDays = source.RetentionPeriodDays,
                MetadataClassifier = source.MetadataClassifier,
                LastUpdaterId = updaterId
            };
        }

        public static LiveToVodLinearAssetConfigurationData Map(long partnerId, LiveToVodLinearAssetConfiguration source, long updaterId)
        {
            if (source == null)
            {
                return null;
            }

            return new LiveToVodLinearAssetConfigurationData
            {
                LinearAssetId = source.LinearAssetId,
                IsLiveToVodEnabled = source.IsLiveToVodEnabled,
                RetentionPeriodDays = source.RetentionPeriodDays,
                LastUpdaterId = updaterId
            };
        }

        public static LiveToVodPartnerConfiguration Map(LiveToVodPartnerConfigurationData source)
        {
            if (source == null)
            {
                return null;
            }

            return new LiveToVodPartnerConfiguration(source.IsLiveToVodEnabled, source.RetentionPeriodDays, source.MetadataClassifier);
        }

        public static LiveToVodLinearAssetConfiguration Map(LiveToVodLinearAssetConfigurationData source)
        {
            if (source == null)
            {
                return null;
            }

            return new LiveToVodLinearAssetConfiguration(source.LinearAssetId, source.IsLiveToVodEnabled, source.RetentionPeriodDays);
        }
    }
}