using System;
using ApiObjects.Pricing;

namespace Core.Pricing
{
    [Serializable]
    public class CouponData
    {
        public CouponsStatus m_CouponStatus { get; private set; }
        public CouponsGroup m_oCouponGroup { get; }
        public CouponType m_CouponType { get; private set; }
        public long m_campID { get; }
        public long m_ownerGUID { get; }
        public long m_ownerMedia { get; }
        public string id { get; }
        public int? leftUses { get; }
        public int? totalUses { get; }

        public CouponData(string couponCode, CouponsGroup group, CouponsStatus status, CouponType couponType,
            long campaignId, long ownerGuid, long mediaId, int? leftUses, int? totalUses)
        {
            id = couponCode;
            m_CouponStatus = status;
            m_oCouponGroup = group;
            m_CouponType = couponType;
            m_campID = campaignId;
            m_ownerGUID = ownerGuid;
            m_ownerMedia = mediaId;
            this.leftUses = leftUses;
            this.totalUses = totalUses;
        }

        public static CouponData NotExist => new CouponData
            {m_CouponStatus = CouponsStatus.NotExists, m_CouponType = CouponType.Coupon};

        private CouponData() { }
    }
}
