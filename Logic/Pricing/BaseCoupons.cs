using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public abstract class BaseCoupons
    {
        protected BaseCoupons() { }
        protected BaseCoupons(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        public abstract CouponsGroup GetCouponGroupData(string sCouponGroupID);
        public abstract CouponsGroup[] GetCouponGroupListForAdmin();
        public abstract CouponsGroup[] GetCouponGroupListForAdmin(bool isVoucher);
        public abstract CouponData GetCouponStatus(string sCouponCode);
        public abstract CouponsStatus SetCouponUsed(string sCouponCode, string sSiteGUID, Int32 nMFID, Int32 nSubCode, Int32 nCollectionCode, int nPrePaidCode);

        protected Int32 m_nGroupID;

        public abstract List<Coupon> GenerateCoupons(int numberOfCoupons, long couponGroupId);
    }
}
