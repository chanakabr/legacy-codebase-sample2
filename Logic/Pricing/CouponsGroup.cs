using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public class CouponsGroup
    {
        public CouponsGroup()
        {
            m_sDiscountCode = string.Empty;
            m_sDescription = null;
            m_dStartDate = new DateTime(2000, 1, 1);
            m_dEndDate = new DateTime(2020, 1, 1);
            m_nMaxUseCountForCoupon = 1;
            m_sGroupName = string.Empty;
            m_nFinancialEntityID = 0;
        }

        public CouponsStatus GetCouponStatusCode(Int32 nUseCount)
        {
            if (m_nMaxUseCountForCoupon <= nUseCount)
                return CouponsStatus.AllreadyUsed;
            if (m_dEndDate != null && m_dEndDate <= DateTime.UtcNow)
                return CouponsStatus.Expired;
            if (m_dStartDate >= DateTime.UtcNow)
                return CouponsStatus.NotActive;
            return CouponsStatus.Valid;
        }

        public bool Initialize(string sGroupName , string sGroupCode , DiscountModule oDiscountCode, LanguageContainer[] sDescription,
            DateTime dStartDate, DateTime dEndDate, Int32 nMaxUseCountForCoupon, Int32 nFinancilaEntityID, Int32 nMaxRecurringUsesCountForCoupon)
        {
            m_sGroupCode = sGroupCode;
            m_sDescription = sDescription;
            m_oDiscountCode = oDiscountCode;
            m_dStartDate = dStartDate;
            m_dEndDate = dEndDate;
            m_nMaxUseCountForCoupon = nMaxUseCountForCoupon;
            m_sGroupName = sGroupName;
            m_nFinancialEntityID = nFinancilaEntityID;
            m_nMaxRecurringUsesCountForCoupon = nMaxRecurringUsesCountForCoupon;
            return true;
        }

        static public LanguageContainer[] GetCouponGroupDescription(Int32 nCouponGroupID)
        {
            LanguageContainer[] theContainer = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select * from coupon_groups_descriptions with (nolock) where is_active=1 and status=1 and ";
                selectQuery += " coupon_group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(nCouponGroupID, "MAIN_CONNECTION_STRING");                
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        theContainer = new LanguageContainer[nCount];
                    }
                    Int32 nIndex = 0;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sLang = selectQuery.Table("query").DefaultView[i].Row["language_code3"].ToString();
                        string sVal = selectQuery.Table("query").DefaultView[i].Row["description"].ToString();
                        LanguageContainer t = new LanguageContainer();
                        t.Initialize(sLang, sVal);
                        theContainer[nIndex] = t;
                        nIndex++;
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
            return theContainer;
        }

        public bool Initialize(Int32 nCouponGroupID , Int32 nGroupID)
        {
            bool bOK = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select * from coupons_groups with (nolock) where is_active=1 and status=1 and ";
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(nGroupID, "MAIN_CONNECTION_STRING");
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nCouponGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        string sDiscountCode = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "DISCOUNT_CODE", 0);
                        BaseDiscount t = null;
                        Utils.GetBaseImpl(ref t, nGroupID);

                        DiscountModule d = t.GetDiscountCodeData(sDiscountCode);
                        DateTime dStart = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "START_DATE", 0);
                        DateTime dEnd = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "END_DATE", 0);
                        Int32 nMaxUseForCoupon = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "MAX_USE_TIME", 0);
                        string sGroupName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "CODE", 0);
                        Int32 nFinancilaEntityID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "FINANCIAL_ENTITY_ID", 0);
                        Int32 nMaxRecurringUsesCountForCoupon = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "MAX_RECURRING_USES", 0);
                        Initialize(sGroupName, nCouponGroupID.ToString(), d, GetCouponGroupDescription(nCouponGroupID), dStart, dEnd, nMaxUseForCoupon, nFinancilaEntityID, nMaxRecurringUsesCountForCoupon);
                        bOK = true;
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
            return bOK;
        }

        public DiscountModule m_oDiscountCode;
        public string m_sDiscountCode;
        public LanguageContainer[] m_sDescription;
        public DateTime m_dStartDate;
        public DateTime m_dEndDate;
        public Int32 m_nMaxUseCountForCoupon;
        public string m_sGroupCode;
        public string m_sGroupName;
        public Int32 m_nFinancialEntityID;
        public Int32 m_nMaxRecurringUsesCountForCoupon;
        public string alias;

    }
}
