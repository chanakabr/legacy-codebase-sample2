using ApiObjects.Pricing;
using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public class Coupon
    {
        public Coupon()
        {
        }

        public Coupon(Int32 couponID, string couponCode)
        {
            m_nCouponID = couponID;
            code = couponCode;
        }

        public void Initialize(Int32 nCouponID)
        {
            m_nCouponID = nCouponID;
        }

        public bool Initialize(string couponCode , int groupId)
        {
            DataTable dt = PricingDAL.GetCoupon(groupId, couponCode); 

            if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
            {
                return false;
            }

            m_nCouponID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "id");
            m_campaignID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "voucher_campaign_id");
            m_ownerGUID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "owner_guid");
            m_ownerMedia = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "rel_media");
            couponsGroupId = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "COUPON_GROUP_ID");
            useCount = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "USE_COUNT");
            isActive = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "IS_ACTIVE") > 0;
            status = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "STATUS") > 0;
            m_couponType = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "voucher") > 0 ? CouponType.Voucher : CouponType.Coupon;

            return true;
        }

        static public CouponsStatus SetCouponUsed(string sCouponCode, Int32 nGroupID, string sSiteGUID, Int32 nCollectionCode, Int32 nMediaFileID, Int32 nSubCode, int nPrePaidCode, long domainId)
        {
            Coupon c = new Coupon();
            ODBCWrapper.DirectQuery directQuery = null;
            ODBCWrapper.InsertQuery insertQuery = null;
            try
            {
                if (c.Initialize(sCouponCode, nGroupID))
                {
                    if (c.GetCouponStatus(nGroupID) == CouponsStatus.Valid)
                    {
                        directQuery = new ODBCWrapper.DirectQuery();
                        directQuery.SetConnectionKey("pricing_connection");
                        directQuery += "update coupons set USE_COUNT=USE_COUNT+1, LAST_USED_DATE=getdate() where ";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", c.m_nCouponID);
                        directQuery.Execute();


                        insertQuery = new ODBCWrapper.InsertQuery("coupon_uses");
                        insertQuery.SetConnectionKey("pricing_connection");
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUPON_ID", "=", c.m_nCouponID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", nSubCode);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRE_PAID_CODE", "=", nPrePaidCode);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COLLECTION_CODE", "=", nCollectionCode);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DOMAIN_ID", "=", domainId);
                        insertQuery.Execute();
                    }
                    return c.GetCouponStatus(nGroupID);
                }
                else
                    return CouponsStatus.NotExists;
            }
            finally
            {
                if (directQuery != null)
                {
                    directQuery.Finish();
                }
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
            }
        }

        public CouponsGroup GetCouponGroup(Int32 nGroupID)
        {
            CouponsGroup p = new CouponsGroup();
            if (couponsGroupId > 0)
            {
                p.Initialize((int)couponsGroupId, nGroupID);
                return p;
            }
            return null;
        }

        public CouponsStatus GetCouponStatus(int groupId, CouponsGroup couponsGroup = null)
        {
            CouponsStatus ret = CouponsStatus.Valid;

            if (!status || !isActive)
                ret = CouponsStatus.NotExists;
            else
            {
                if (couponsGroup == null)
                {
                    couponsGroup = new CouponsGroup();
                    couponsGroup.Initialize((int)couponsGroupId, groupId);
                }

                ret = couponsGroup.GetCouponStatusCode(useCount);
            }

            return ret;
        }

        public Int32 m_nCouponID;
        public CouponType m_couponType;
        public long m_campaignID;
        public long m_ownerGUID;
        public long m_ownerMedia;
        public string code;
        public long couponsGroupId;
        public int useCount;
        public bool isActive;
        public bool status;
    }
}
