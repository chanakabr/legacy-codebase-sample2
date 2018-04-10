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
        public abstract CouponData GetCouponStatus(string sCouponCode, long domainId);
        public abstract CouponsStatus SetCouponUsed(string sCouponCode, string sSiteGUID, Int32 nMFID, Int32 nSubCode, Int32 nCollectionCode, int nPrePaidCode, long domainId);

        protected Int32 m_nGroupID;

        public abstract List<Coupon> GenerateCoupons(int numberOfCoupons, long couponGroupId, out ApiObjects.Response.Status status, bool useLetters = true, bool useNumbers = true, bool useSpecialCharacters = true);

        public abstract CouponDataResponse ValidateCouponForSubscription(int groupId, int subscriptionId, string couponCode, long domainId);

        public abstract List<Coupon> GeneratePublicCode(int groupId, long couponGroupId, string code, out ApiObjects.Response.Status status);
        public abstract CouponsGroupResponse GetCouponGroupData(long couponsGroupId);

        public abstract CouponsGroupsResponse GetCouponGroups();
        
    }
}
