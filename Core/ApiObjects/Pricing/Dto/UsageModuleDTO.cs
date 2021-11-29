namespace ApiObjects.Pricing
{
    public class UsageModuleDTO
    {
        public int Id { get; set; }
        public string VirtualName { get; set; }
        public int MaxNumberOfViews { get; set; }
        public int TsViewLifeCycle { get; set; }
        public int TsMaxUsageModuleLifeCycle { get; set; }
        public bool Waiver { get; set; }
        public int WaiverPeriod { get; set; }
        public bool IsOfflinePlayBack { get; set; }
        public int Type { get; set; }
    }
}