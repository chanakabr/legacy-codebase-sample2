using ApiObjects.Base;
using System;

namespace ApiObjects.Pricing
{
    public class CouponWalt : ICrudHandeledObject
    {
        public long DomainId { get; set; }

        public int CouponId { get; set; }

        public string CouponCode { get; set; }

        public DateTime CreateDate { get; set; }

        public string CouponGroupId { get; set; }
    }
}
