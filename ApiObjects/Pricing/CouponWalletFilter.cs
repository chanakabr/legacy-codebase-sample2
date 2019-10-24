using ApiObjects.Base;

namespace ApiObjects.Pricing
{
    public class CouponWalletFilter : ICrudFilter
    {
        public long BusinessModuleId { get; set; }
        public eTransactionType BusinessModuleType { get; set; }
        public string CouponCode { get; set; }
        public CouponsStatus? Status { get; set; }
    }
}