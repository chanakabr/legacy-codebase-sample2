using ApiObjects.Base;

namespace ApiObjects.Pricing
{
    public class CouponWalletFilter : ICrudFilter
    {
        public long BusinessModuleId { get; set; }
        public eTransactionType BusinessModuleType { get; set; }
    }
}