namespace FeatureFlag
{
    internal class LaunchDarklyDummyFeatureFlag : IFeatureFlag
    {
        public bool Enabled(string key, KalturaFeatureFlagUser user)
        {
            return true;
        }
    }
}