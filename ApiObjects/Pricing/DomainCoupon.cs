using ApiObjects.Base;

namespace ApiObjects.Pricing
{
    // TODO ANAT(BEO-6931) - ADD ALL relevant DomainCoupon data members
    public class DomainCoupon : ICrudHandeledObject
    {
        public int DomainId { get; set; }
        public int CouponId { get; set; }
    }
}
