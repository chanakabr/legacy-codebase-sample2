namespace FeatureFlag
{
    public class KalturaFeatureFlagUserBuilder
    {
        private long? _userId;
        private int _groupId;

        public static KalturaFeatureFlagUserBuilder Get()
        {
            return new KalturaFeatureFlagUserBuilder();
        }

        public KalturaFeatureFlagUserBuilder WithUserId(long? userId)
        {
            _userId = userId;
            return this;
        }

        public KalturaFeatureFlagUserBuilder WithGroupId(int groupId)
        {
            _groupId = groupId;
            return this;
        }

        public KalturaFeatureFlagUser Build()
        {
            return new KalturaFeatureFlagUser(_userId, _groupId);
        }
    }
}