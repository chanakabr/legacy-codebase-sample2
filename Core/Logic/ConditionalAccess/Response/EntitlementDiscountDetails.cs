namespace Core.ConditionalAccess.Response
{
    public class EntitlementDiscountDetails
    {
        public double Amount { get; set; }
        public long? StartDate { get; set; }
        public long? EndDate { get; set; }
    }

    public class CouponEntitlementDiscountDetails : EntitlementDiscountDetails
    {
        public string CouponCode { get; set; }
        public bool EndlessCoupon { get; set; }
    }

    public class EntitlementDiscountDetailsIdentifier : EntitlementDiscountDetails
    {
        public long Id { get; set; }

    }

    public class CompensationEntitlementDiscountDetails : EntitlementDiscountDetailsIdentifier { }
    public class CampaignEntitlementDiscountDetails : EntitlementDiscountDetailsIdentifier { }
    public class DiscountEntitlementDiscountDetails : EntitlementDiscountDetailsIdentifier { }
    public class TrailEntitlementDiscountDetails : EntitlementDiscountDetailsIdentifier { }
}