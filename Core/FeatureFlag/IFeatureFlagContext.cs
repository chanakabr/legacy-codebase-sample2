namespace FeatureFlag
{
    public interface IFeatureFlagContext
    {
        long? GetPartnerId();
        
        long? GetUserId();
    }
}
