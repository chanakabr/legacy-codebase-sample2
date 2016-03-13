using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

<<<<<<< HEAD
namespace ApiObjects.IngestBusinessModules
{
    public class IngestPPV : IngestModule
    {
        public string PriceCode { get; set; }

        public string PricePlan { get; set; }

        public string Discount { get; set; }

        public string GroupCoupon { get; set; }

        public bool SubscriptionOnly { get; set; }

        public string Title { get; set; }

        public bool FirstDeviceLimitation { get; set; }

        public string ProductCode { get; set; }

=======
namespace ApiObjects
{
    public class IngestPPV : IngestModule
    {
        public List<KeyValuePair> Titles { get; set; } // lang, title

        public string Discount { get; set; }

        public string CouponGroup { get; set; }

        public string ProductCode { get; set; }

        public bool IsRenewable { get; set; }

        public bool SubscriptionOnly { get; set; }

        public bool FirstDeviceLimitation { get; set; }

        public string UsageModule { get; set; }

        public string PriceCode { get; set; }

        public bool IsActive { get; set; }
>>>>>>> 4643f134c29a4feb7ff971e93c489f5e35a02e27
    }
}
