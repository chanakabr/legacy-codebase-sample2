using Newtonsoft.Json;

namespace LiveToVod.BOL
{
    public class LiveToVodLinearAssetConfiguration
    {
        public long LinearAssetId { get; }

        public bool IsLiveToVodEnabled { get; }

        public int? RetentionPeriodDays { get; }

        [JsonConstructor]
        public LiveToVodLinearAssetConfiguration(long linearAssetId, bool isLiveToVodEnabled, int? retentionPeriodDays)
        {
            LinearAssetId = linearAssetId;
            IsLiveToVodEnabled = isLiveToVodEnabled;
            RetentionPeriodDays = retentionPeriodDays;
        }

        public LiveToVodLinearAssetConfiguration(long linearAssetId, bool isLiveToVodEnabled)
            : this(linearAssetId, isLiveToVodEnabled, null)
        {
        }
    }
}