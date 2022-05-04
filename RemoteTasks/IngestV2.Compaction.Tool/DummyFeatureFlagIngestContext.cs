using FeatureFlag;

namespace IngestV2.Compaction.Tool
{
    public class DummyFeatureFlagIngestContext : IFeatureFlagContext
    {
        public long? GetPartnerId()
        {
            return 0;
        }

        public long? GetUserId()
        {
            return 0;
        }
    }
}
