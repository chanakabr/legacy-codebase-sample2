using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public class CouponData
    {
        public CouponsStatus m_CouponStatus;
        public CouponsGroup m_oCouponGroup;
        public CouponType m_CouponType;
        public long m_campID;
        public long m_ownerGUID;
        public long m_ownerMedia;
        public string id;

        public CouponData() 
        {
            m_CouponStatus = CouponsStatus.NotExists;
            m_oCouponGroup = null;
            m_CouponType = CouponType.Coupon;
            m_campID = 0;
            m_ownerGUID = 0;
            m_ownerMedia = 0;
        }

        public void Initialize(CouponsGroup theGroup, CouponsStatus theStatus, CouponType couponType, long campaignID, long ownerGUID, long mediaID)
        {
            m_CouponStatus = theStatus;
            m_oCouponGroup = theGroup;
            m_CouponType = couponType;
            m_campID = campaignID;
            m_ownerGUID = ownerGUID;
            m_ownerMedia = mediaID;
        }

        public void Initialize(string couponCode, CouponsGroup group, CouponsStatus status, CouponType couponType, long campaignID, long ownerGUID, long mediaID)
        {
            this.Initialize(group, status, couponType, campaignID, ownerGUID, mediaID);
            id = couponCode;
        }

        public void Initialize(CouponsGroup theGroup, CouponsStatus theStatus)
        {
            Initialize(theGroup, theStatus, CouponType.Coupon, 0, 0, 0);
        }
    }
}
