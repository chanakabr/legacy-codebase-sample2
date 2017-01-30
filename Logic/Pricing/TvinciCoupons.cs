using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    public class TvinciCoupons : BaseCoupons
    {
        public TvinciCoupons(Int32 nGroupID): base(nGroupID)
        {
        }

        public override CouponsGroup[] GetCouponGroupListForAdmin(bool isVoucher)
        {
            CouponsGroup[] ret = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select id from coupons_groups with (nolock) where is_active=1 and status=1 and START_DATE<getdate() and (end_date is null OR end_date>getdate()) and ";
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        ret = new CouponsGroup[nCount];
                    for (int i = 0; i < nCount; i++)
                    {
                        ret[i] = GetCouponGroupData(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                    }
                }

            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return ret;
        }

        public override CouponsGroup[] GetCouponGroupListForAdmin()
        {
            return GetCouponGroupListForAdmin(false);

        }

        public override CouponsGroup GetCouponGroupData(string sCouponGroupID)
        {
            CouponsGroup tmp = null;
            Int32 nCouponGroupID = 0;
            try
            {
                nCouponGroupID = int.Parse(sCouponGroupID);
            }
            catch
            {
                return null;
            }
            tmp = new CouponsGroup();
            tmp.Initialize(nCouponGroupID, m_nGroupID);
            return tmp;
        }

        public override CouponData GetCouponStatus(string sCouponCode)
        {
            CouponData data = new CouponData();
            Coupon coupon = new Coupon();

            if (coupon.Initialize(sCouponCode, m_nGroupID))
            {
                CouponsGroup couponsGroup = coupon.GetCouponGroup(m_nGroupID);
                data.Initialize(sCouponCode, couponsGroup, coupon.GetCouponStatus(), coupon.m_couponType, coupon.m_campaignID, coupon.m_ownerGUID, coupon.m_ownerMedia);
            }
            else
            {
                data.m_CouponStatus = CouponsStatus.NotExists;
            }

            return data;
        }

        public override CouponsStatus SetCouponUsed(string sCouponCode, string sSiteGUID, Int32 nMFID, Int32 nSubCode, Int32 nCollectionCode, int nPrePaidCode)
        {
            return Coupon.SetCouponUsed(sCouponCode, m_nGroupID, sSiteGUID,nCollectionCode, nMFID, nSubCode, nPrePaidCode);
        }
    }
}
