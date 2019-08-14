
namespace ApiObjects.ConditionalAccess
{
    /// <summary>
    /// renew SubscriptionPurchase data
    /// </summary>
    public class RecurringRenewDetails
    {
        public string CouponCode { get; set; }        
        public double CouponRemainder { get; set; }
        public int LeftCouponRecurring { get; set; }
        public int TotalNumOfRenews { get; set; }
        // TODO SHIR - UPDATE Compensation WHEN IS NEEDED
        public Compensation Compensation { get; set; }
        public bool IsPurchasedWithPreviewModule { get; set; }
        public bool IsCouponGiftCard { get; set; }
        public bool IsCouponHasEndlessRecurring { get; set; }
    }
}
