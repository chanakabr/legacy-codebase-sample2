namespace FeatureFlag
{
    public readonly struct KalturaFeatureFlagUser
    {
        public KalturaFeatureFlagUser(long? userId, int? groupId = null)
        {
            UserId = userId;
            GroupId = groupId;
        }

        public long? UserId { get; }

        public int? GroupId { get; }

        public bool IsAnonymous => UserId == null || UserId == 0;
    }
}