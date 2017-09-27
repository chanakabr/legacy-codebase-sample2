using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Billing
{
    public class TransactionUnifiedRenewalDetails
    {
        public string ExternalTransactionId { get; set; }
        public int GracePeriodMinutes { get; set; }

        public string siteGuid;
        public double price;
        public int productId;
        public eTransactionType productType;
        public int contentId;
        public string billingGuid;
        public string customData;
        public string productCode;
        public string adapterData;
    }
}