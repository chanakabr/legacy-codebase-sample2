using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class CouponData
    {
        public CouponsStatus m_CouponStatus { get; set; }

        public CouponsGroup m_oCouponGroup { get; set; }

        public CouponType m_CouponType { get; set; }

        public long m_campID { get; set; }

        public long m_ownerGUID { get; set; }

        public long m_ownerMedia { get; set; }

    }

    public enum CouponType
    {

        /// <remarks/>
        Unknown,

        /// <remarks/>
        Coupon,

        /// <remarks/>
        Voucher,
    }
}
