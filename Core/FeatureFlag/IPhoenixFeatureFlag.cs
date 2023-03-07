namespace FeatureFlag
{
    public interface IPhoenixFeatureFlag
    {
        bool IsMediaMarksNewModel(int groupId);
        //bool IsUdidDynamicListAsExcelEnabled(int groupId);
        bool IsStrictUnlockDisabled();
        bool IsEfficientSerializationUsed();
        bool IsRenewUseKronos();
        bool IsRenewUseKronosPog();
        bool IsUnifiedRenewUseKronos();
        bool IsRenewalReminderUseKronos();
        bool IsRenewSubscriptionEndsUseKronos();
        bool IsCloudfrontInvalidationEnabled();
        bool IsImprovedUpdateMediaAssetStoredProcedureShouldBeUsed();
    }
}
