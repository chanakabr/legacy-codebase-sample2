using RestfulTVPApi.Objects.Responses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class CouponData
    {
        public CouponsStatus coupon_status { get; set; }

        public CouponsGroup coupon_group { get; set; }

        public CouponType coupon_type { get; set; }

        public long camp_id { get; set; }

        public long owner_guid { get; set; }

        public long owner_media { get; set; }
    }
}
