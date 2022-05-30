namespace LiveToVod.BOL
{
    public class LiveToVodLinearAssetConfiguration
    {
        public long LinearAssetId { get; }

        public bool IsLiveToVodEnabled { get; }

        public int? RetentionPeriodDays { get; }

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