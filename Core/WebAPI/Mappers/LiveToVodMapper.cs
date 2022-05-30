using System.Linq;
using LiveToVod.BOL;
using WebAPI.Models.LiveToVod;

namespace WebAPI.Mappers
{
    public static class LiveToVodMapper
    {
        public static LiveToVodPartnerConfiguration Map(KalturaLiveToVodPartnerConfiguration source)
        {
            if (source == null)
            {
                return null;
            }

            return new LiveToVodPartnerConfiguration(source.IsLiveToVodEnabled.Value, source.RetentionPeriodDays.Value, source.MetadataClassifier);
        }

        public static LiveToVodLinearAssetConfiguration Map(KalturaLiveToVodLinearAssetConfiguration source)
        {
            if (source == null)
            {
                return null;
            }

            return new LiveToVodLinearAssetConfiguration(source.LinearAssetId.Value, source.IsLiveToVodEnabled.Value, source.RetentionPeriodDays);
        }

        public static KalturaLiveToVodFullConfiguration Map(LiveToVodFullConfiguration source)
        {
            if (source == null)
            {
                return null;
            }

            return new KalturaLiveToVodFullConfiguration
            {
                IsLiveToVodEnabled = source.IsLiveToVodEnabled,
                RetentionPeriodDays = source.RetentionPeriodDays,
                MetadataClassifier = source.MetadataClassifier,
                LinearAssets = source.LinearAssets?.Select(Map).ToList()
            };
        }

        public static KalturaLiveToVodPartnerConfiguration Map(LiveToVodPartnerConfiguration source)
        {
            if (source == null)
            {
                return null;
            }

            return new KalturaLiveToVodPartnerConfiguration
            {
                IsLiveToVodEnabled = source.IsLiveToVodEnabled,
                RetentionPeriodDays = source.RetentionPeriodDays,
                MetadataClassifier = source.MetadataClassifier
            };
        }

        public static KalturaLiveToVodLinearAssetConfiguration Map(LiveToVodLinearAssetConfiguration source)
        {
            if (source == null)
            {
                return null;
            }

            return new KalturaLiveToVodLinearAssetConfiguration
            {
                LinearAssetId = source.LinearAssetId,
                RetentionPeriodDays = source.RetentionPeriodDays,
                IsLiveToVodEnabled = source.IsLiveToVodEnabled
            };
        }
    }
}