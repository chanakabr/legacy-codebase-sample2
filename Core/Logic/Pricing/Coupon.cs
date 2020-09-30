using ApiObjects.Pricing;
using System;
using System.Data;
using static ODBCWrapper.Utils;

namespace Core.Pricing
{
    [Serializable]
    public class Coupon
    {
        public int m_nCouponID;
        public CouponType m_couponType;
        public long m_campaignID;
        public long m_ownerGUID;
        public long m_ownerMedia;
        public string code;
        public long couponsGroupId;
        public int useCount;
        public bool isActive;
        public bool status;

        public Coupon()
        {
        }

        public Coupon(int couponID, string couponCode)
        {
            m_nCouponID = couponID;
            code = couponCode;
        }
    }

    // should be in data-access layer, but DAL hasn't access to Coupon class
    public static class DataTableCouponExtension
    {
        public static Coupon ToCoupon(this DataTable dt)
        {
            if (dt?.Rows == null || dt.Rows.Count == 0) return null;

            return new Coupon
            {
                m_nCouponID = GetIntSafeVal(dt.Rows[0], "id"),
                m_campaignID = GetLongSafeVal(dt.Rows[0], "voucher_campaign_id"),
                m_ownerGUID = GetLongSafeVal(dt.Rows[0], "owner_guid"),
                m_ownerMedia = GetLongSafeVal(dt.Rows[0], "rel_media"),
                couponsGroupId = GetLongSafeVal(dt.Rows[0], "COUPON_GROUP_ID"),
                useCount = GetIntSafeVal(dt.Rows[0], "USE_COUNT"),
                isActive = GetIntSafeVal(dt.Rows[0], "IS_ACTIVE") > 0,
                status = GetIntSafeVal(dt.Rows[0], "STATUS") > 0,
                m_couponType = GetIntSafeVal(dt.Rows[0], "voucher") > 0 ? CouponType.Voucher : CouponType.Coupon
            };
        }
    }
}
