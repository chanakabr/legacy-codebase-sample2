namespace LiveToVod.BOL
{
    public class LiveToVodPartnerConfiguration
    {
        public bool IsLiveToVodEnabled { get; }

        public int RetentionPeriodDays { get; }

        public string MetadataClassifier { get; }

        public LiveToVodPartnerConfiguration()
        {
        }

        public LiveToVodPartnerConfiguration(bool isLiveToVodEnabled, int retentionPeriodDays, string metadataClassifier)
        {
            IsLiveToVodEnabled = isLiveToVodEnabled;
            RetentionPeriodDays = retentionPeriodDays;
            MetadataClassifier = metadataClassifier;
        }
    }
}