namespace Core.Pricing
{
    public class UsageModuleForUpdate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? MaxNumberOfViews { get; set; }
        public int? TsViewLifeCycle { get; set; }
        public int? TsMaxUsageModuleLifeCycle { get; set; }
        public bool? Waiver { get; set; }
        public int? WaiverPeriod { get; set; }
        public bool? IsOfflinePlayBack { get; set; }

        public bool ShouldUpdate(UsageModule oldUsageModule)
        {
            bool shouldUpdate = false;

            if (!string.IsNullOrEmpty(Name) && !string.Equals(Name, oldUsageModule.m_sVirtualName))
            {
                shouldUpdate = true;
            }
            else
            {
                Name = oldUsageModule.m_sVirtualName;
            }

            if (MaxNumberOfViews.HasValue && MaxNumberOfViews.Value != oldUsageModule.m_nMaxNumberOfViews)
            {
                shouldUpdate = true;
            }
            else
            {
                MaxNumberOfViews = oldUsageModule.m_nMaxNumberOfViews;
            }

            if (TsViewLifeCycle.HasValue && TsViewLifeCycle.Value != oldUsageModule.m_tsViewLifeCycle)
            {
                shouldUpdate = true;
            }
            else
            {
                TsViewLifeCycle = oldUsageModule.m_tsViewLifeCycle;
            }

            if (TsMaxUsageModuleLifeCycle.HasValue && TsMaxUsageModuleLifeCycle.Value != oldUsageModule.m_tsMaxUsageModuleLifeCycle)
            {
                shouldUpdate = true;
            }
            else
            {
                TsMaxUsageModuleLifeCycle = oldUsageModule.m_tsMaxUsageModuleLifeCycle;
            }

            if (Waiver.HasValue && Waiver.Value != oldUsageModule.m_bWaiver)
            {
                shouldUpdate = true;
            }
            else
            {
                Waiver = oldUsageModule.m_bWaiver;
            }
            
            if (IsOfflinePlayBack.HasValue && IsOfflinePlayBack.Value != oldUsageModule.m_bIsOfflinePlayBack)
            {
                shouldUpdate = true;
            }
            else
            {
                IsOfflinePlayBack = oldUsageModule.m_bIsOfflinePlayBack;
            }

            if (WaiverPeriod.HasValue && WaiverPeriod.Value != oldUsageModule.m_nWaiverPeriod)
            {
                shouldUpdate = true;
            }
            else
            {
                WaiverPeriod = oldUsageModule.m_nWaiverPeriod;
            }

            return shouldUpdate;
        }
    }
}