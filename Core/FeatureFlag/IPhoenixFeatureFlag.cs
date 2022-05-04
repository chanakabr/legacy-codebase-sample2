namespace FeatureFlag
{
    public interface IPhoenixFeatureFlag
    {
        bool IsEpgNotificationEnabled(int groupId);
        bool IsMediaMarksNewModel(int groupId);
        //bool IsUdidDynamicListAsExcelEnabled(int groupId);

        bool IsStrictUnlockDisabled();
    }
}
