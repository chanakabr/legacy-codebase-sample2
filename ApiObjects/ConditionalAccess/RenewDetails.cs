using System;

namespace ApiObjects.ConditionalAccess
{
    public class RenewDetails
    {
        /// <summary>
        /// SubscriptionCode
        /// </summary>
        public int ProductId { get; set; }
        public long PurchaseId { get; set; }
        public string BillingGuid { get; set; }
        public long BillingTransactionId { get; set; }
        public string ExternalTransactionId { get; set; }
        public string UserId { get; set; }
        public long DomainId { get; set; }
        public bool ShouldSwitchToMasterUser { get; set; }
        public int GroupId { get; set; }
        public string Currency { get; set; }
        public double Price { get; set; }
        public int PaymentMethodId { get; set; }
        public int PaymentNumber { get; set; }
        public int NumOfPayments { get; set; }
        public string CustomData { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? StartDate { get; set; }
        public int MaxVLCOfSelectedUsageModule { get; set; }
        public int GracePeriodMinutes { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string PreviousPurchaseCurrencyCode { get; set; }
        public SubscriptionPurchaseStatus SubscriptionStatus { get; set; }
        public bool IsUseCouponRemainder { get; set; }
        public RecurringRenewDetails RecurringData { get; set; }

        public eTransactionType Type { get { return eTransactionType.Subscription; } }
    }
}
