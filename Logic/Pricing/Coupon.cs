using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
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

        public void Initialize(Int32 nCouponID)
        {
            m_nCouponID = nCouponID;
        }

        public bool Initialize(string sCode , Int32 nGroupID)
        {
            bool bRet = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select id, voucher_campaign_id, owner_guid, rel_media from coupons with (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("code", "=", sCode);
                selectQuery += "and";
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(nGroupID, "MAIN_CONNECTION_STRING");
                selectQuery += " order by status desc,is_active desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        m_nCouponID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                        object oCID = selectQuery.Table("query").DefaultView[0].Row["voucher_campaign_id"];
                        object oUID = selectQuery.Table("query").DefaultView[0].Row["owner_guid"];
                        object oMID = selectQuery.Table("query").DefaultView[0].Row["rel_media"];
                        if (oCID != null && oCID != System.DBNull.Value)
                        {
                            m_campaignID = long.Parse(selectQuery.Table("query").DefaultView[0].Row["voucher_campaign_id"].ToString());
                        }
                        if (oUID != null && oUID != System.DBNull.Value)
                        {
                            m_ownerGUID = long.Parse(selectQuery.Table("query").DefaultView[0].Row["owner_guid"].ToString());
                        }
                        if (oMID != null && oMID != System.DBNull.Value)
                        {
                            m_ownerMedia = long.Parse(selectQuery.Table("query").DefaultView[0].Row["rel_media"].ToString());
                        }
                        bRet = true;
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
            return bRet;
        }

        static public CouponsStatus SetCouponUsed(string sCouponCode, Int32 nGroupID, string sSiteGUID, Int32 nCollectionCode, Int32 nMediaFileID, Int32 nSubCode, int nPrePaidCode)
        {
            Coupon c = new Coupon();
            ODBCWrapper.DirectQuery directQuery = null;
            ODBCWrapper.InsertQuery insertQuery = null;
            try
            {
                if (c.Initialize(sCouponCode, nGroupID))
                {
                    if (c.GetCouponStatus() == CouponsStatus.Valid)
                    {
                        directQuery = new ODBCWrapper.DirectQuery();
                        directQuery.SetConnectionKey("pricing_connection");
                        directQuery += "update coupons set USE_COUNT=USE_COUNT+1, LAST_USED_DATE=getdate() where ";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", c.m_nCouponID);
                        directQuery.Execute();


                        insertQuery = new ODBCWrapper.InsertQuery("coupon_uses");
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUPON_ID", "=", c.m_nCouponID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", nSubCode);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRE_PAID_CODE", "=", nPrePaidCode);
                        insertQuery.Execute();
                    }
                    return c.GetCouponStatus();
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
            object o = ODBCWrapper.Utils.GetTableSingleVal("coupons", "COUPON_GROUP_ID", m_nCouponID, "pricing_connection");
            if (o != null && o != DBNull.Value)
            {
                Int32 nCouponGroupID = int.Parse(o.ToString());
                p.Initialize(nCouponGroupID, nGroupID);
                return p;
            }
            return null;
            
        }

        public CouponsStatus GetCouponStatus()
        {
            CouponsStatus ret = CouponsStatus.Valid;
            Int32 nCouponGroupID = 0;
            Int32 nUseCount = 0;
            DateTime dLast = new DateTime(2000, 1, 1);
            Int32 nIsActive = 0;
            Int32 nStatus = 0;
            Int32 nGroupID = 0;
            object oCouponType = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select * from coupons with (nolock) where ";
                selectQuery.SetCachedSec(0);
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nCouponID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nCouponGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["COUPON_GROUP_ID"].ToString());
                        object oDate = selectQuery.Table("query").DefaultView[0].Row["LAST_USED_DATE"];
                        if (oDate != null && oDate != DBNull.Value)
                            dLast = (DateTime)(oDate);
                        nUseCount = int.Parse(selectQuery.Table("query").DefaultView[0].Row["USE_COUNT"].ToString());
                        nIsActive = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IS_ACTIVE"].ToString());
                        nStatus = int.Parse(selectQuery.Table("query").DefaultView[0].Row["STATUS"].ToString());
                        nGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                        oCouponType = selectQuery.Table("query").DefaultView[0].Row["voucher"];
                        if (oCouponType != null && oCouponType != System.DBNull.Value)
                        {
                            int couponType = int.Parse(oCouponType.ToString());
                            if (couponType > 0)
                            {
                                m_couponType = CouponType.Voucher;
                            }
                        }
                    }
                    else
                        ret = CouponsStatus.NotExists;
                }

                if (ret != CouponsStatus.NotExists)
                {
                    if (nStatus == 0)
                        ret = CouponsStatus.NotExists;
                    else if (nIsActive == 0)
                        ret = CouponsStatus.NotExists;
                    else
                    {
                        CouponsGroup t = new CouponsGroup();
                        t.Initialize(nCouponGroupID, nGroupID);
                        ret = t.GetCouponStatusCode(nUseCount);
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

        protected Int32 m_nCouponID;
        public CouponType m_couponType;
        public long m_campaignID;
        public long m_ownerGUID;
        public long m_ownerMedia;
    }
}
