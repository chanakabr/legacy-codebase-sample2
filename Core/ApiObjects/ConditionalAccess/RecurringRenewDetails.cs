
using System.Collections.Generic;

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
        public Compensation Compensation { get; set; }
        public bool IsPurchasedWithPreviewModule { get; set; }
        public bool IsCouponGiftCard { get; set; }
        public bool IsCouponHasEndlessRecurring { get; set; }
        public RecurringCampaignDetails CampaignDetails { get; set; }
}

    public class RecurringCampaignDetails
    {
        public long Id { get; set; }
        public double Remainder { get; set; }
        public int LeftRecurring { get; set; }
        public bool IsUseRemainder { get; set; }
        public string Udid { get; set; }
        public long CampaignEndDate { get; set; }
    }
}
