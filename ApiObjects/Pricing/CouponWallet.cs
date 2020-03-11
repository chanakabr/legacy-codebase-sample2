using ApiObjects.Base;
using System;

namespace ApiObjects.Pricing
{
    public class CouponWallet : ICrudHandeledObject
    {
        public int CouponId { get; set; }

        public string CouponCode { get; set; }

        public DateTime CreateDate { get; set; }

        public string CouponGroupId { get; set; }

        public long? LastUsageDate { get; set; }
    }
}
