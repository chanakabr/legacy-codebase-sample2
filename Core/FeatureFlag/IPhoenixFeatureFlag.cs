namespace FeatureFlag
{
    public interface IPhoenixFeatureFlag
    {
        bool IsEpgNotificationEnabled(int groupId);
        bool IsMediaMarksNewModel(int groupId);
        //bool IsUdidDynamicListAsExcelEnabled(int groupId);
        bool IsStrictUnlockDisabled();
        bool IsEfficientSerializationUsed();
        bool IsRenewUseKronos();
        bool IsUnifiedRenewUseKronos();
        bool IsRenewalReminderUseKronos();
        bool IsRenewSubscriptionEndsUseKronos();
    }
}
