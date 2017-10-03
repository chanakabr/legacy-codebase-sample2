using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APILogic.ConditionalAccess.Modules
{
    public class RenewSubscriptionDetails
    {
        public string ProductId { get; set; }

        public string UserId { get; set; }

        public string Currency { get; set; }

        public string CustomData { get; set; }

        public string CouponCode { get; set; }

        public string BillingGuid { get; set; }

         public string ExternalTransactionId {get;set;}

        public DateTime? EndDate { get; set; }

        public long PurchaseId { get; set; }

        public double Price { get; set; }
        
        public int PaymentMethodId { get; set; }
        
        public int PaymentNumber { get; set; }

        // get number of payments
        public int NumOfPayments { get; set; }

        // get total number of payments
        public int TotalNumOfPayments { get; set; }
        public int MaxVLCOfSelectedUsageModule { get; set; }
        public Compensation Compensation { get; set; }

        public bool IsPurchasedWithPreviewModule { get; set; }

        public int GracePeriodMinutes { get; set; }

        public string CountryName { get; set; }

        public string CountryCode { get; set; }

        public eTransactionType Type
        {
            get
            {
                return eTransactionType.Subscription;
            }
        }

        public long BillingTransactionId { get; set; }
    }
}
