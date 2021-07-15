namespace FeatureFlag
{
    public class KalturaFeatureFlagUserBuilder : IKalturaFeatureFlagUserBuilder, IKalturaFeatureFlagOptionalBuilder
    {
        private long? _userId;
        private int _groupId;

        public KalturaFeatureFlagUserBuilder()
        {
        }
        
        public IKalturaFeatureFlagOptionalBuilder WithUserId(long? userId)
        {
            _userId = userId;
            return this;
        }

        public IKalturaFeatureFlagOptionalBuilder WithGroupId(int groupId)
        {
            _groupId = groupId;
            return this;
        }

        public KalturaFeatureFlagUser Build()
        {
            return new KalturaFeatureFlagUser {UserId = _userId, GroupId = _groupId};
        }
    }

    public interface IKalturaFeatureFlagUserBuilder
    {
        IKalturaFeatureFlagOptionalBuilder WithUserId(long? userId);
    }

    public interface IKalturaFeatureFlagOptionalBuilder
    {
        IKalturaFeatureFlagOptionalBuilder WithGroupId(int groupId);

        KalturaFeatureFlagUser Build();
    }
}