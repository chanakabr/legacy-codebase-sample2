namespace ApiLogic.FeatureToggle
{
    public interface IPhoenixFeatureFlag
    {
        bool IsEpgNotificationEnabled(int groupId);
    }
}