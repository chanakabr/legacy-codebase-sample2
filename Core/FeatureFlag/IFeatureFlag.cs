
namespace FeatureFlag
{
    public interface IFeatureFlag
    {
        bool Enabled(string key, KalturaFeatureFlagUser user);
    }
}
