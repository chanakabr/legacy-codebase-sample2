namespace FeatureFlag
{
    public interface IPhoenixFeatureFlag
    {
        bool IsEpgNotificationEnabled(int groupId);
        bool IsUdidDynamicListAsExcelEnabled(int groupId);
    }
}