namespace ApiObjects.Pricing
{
    // TODO move to DAL/DTO
    public class UsageModuleDTO
    {
        public int Id { get; set; }
        public string VirtualName { get; set; }
        public int MaxNumberOfViews { get; set; }
        public int TsViewLifeCycle { get; set; }
        public int TsMaxUsageModuleLifeCycle { get; set; }
        public int ExtDiscountId { get; set; }        
        public int PricingId { get; set; }
        public int CouponId { get; set; }
        public int IsRenew { get; set; }
        public int NumOfRecPeriods { get; set; }        
        public bool Waiver { get; set; }
        public int WaiverPeriod { get; set; }
        public bool IsOfflinePlayBack { get; set; }
    }
}