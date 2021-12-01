namespace FeatureFlag
{
    public interface IPhoenixFeatureFlag
    {
        bool IsEpgNotificationEnabled(int groupId);
    }
}