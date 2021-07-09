namespace FeatureFlag
{
    public interface IFeatureFlag
    {
        bool IsEpgNotificationEnabled(int groupId);
    }
}
