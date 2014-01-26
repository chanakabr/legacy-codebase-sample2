using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class CouponData
    {
        public CouponsStatus couponStatus { get; set; }

        public CouponsGroup couponGroup { get; set; }

        public CouponType couponType { get; set; }

        public long campID { get; set; }

        public long ownerGUID { get; set; }

        public long ownerMedia { get; set; }
    }
}
