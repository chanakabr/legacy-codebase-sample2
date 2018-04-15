using ApiObjects;
using ApiObjects.Pricing;
using CachingProvider.LayeredCache;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Core.Pricing
{
    [Serializable]
    public class CouponsGroup
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public CouponsGroup()
        {
            m_sDiscountCode = string.Empty;
            m_sDescription = null;
            m_dStartDate = new DateTime(2000, 1, 1);
            m_dEndDate = new DateTime(2020, 1, 1);
            m_nMaxUseCountForCoupon = 1;
            m_sGroupName = string.Empty;
            m_nFinancialEntityID = 0;
            maxDomainUses = 0;
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
            DateTime dStartDate, DateTime dEndDate, Int32 nMaxUseCountForCoupon, Int32 nFinancilaEntityID, Int32 nMaxRecurringUsesCountForCoupon, int maxDomainUses,
             string discountCode, CouponGroupType couponType = CouponGroupType.Coupon)
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
            this.couponGroupType = couponType;
            this.maxDomainUses = maxDomainUses;
            this.m_sDiscountCode = discountCode;
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

        public bool Initialize(int couponGroupId , int groupId)
        {
            CouponsGroup couponsGroup = GetCouponsGroup(couponGroupId, groupId);
            if (couponsGroup == null)
                return false;

            return Initialize(couponsGroup);
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
        public CouponGroupType couponGroupType;
        public int maxDomainUses;

        internal bool Initialize(CouponsGroup couponGroup)
        {
            return this.Initialize(couponGroup.m_sGroupName, couponGroup.m_sGroupCode, couponGroup.m_oDiscountCode, couponGroup.m_sDescription,
            couponGroup.m_dStartDate, couponGroup.m_dEndDate, couponGroup.m_nMaxUseCountForCoupon, couponGroup.m_nFinancialEntityID, couponGroup.m_nMaxRecurringUsesCountForCoupon, couponGroup.maxDomainUses,
            couponGroup.m_sDiscountCode, couponGroup.couponGroupType);            
        }

        public static CouponsGroup GetCouponsGroup(long couponsGroupId, int groupId)
        {
            CouponsGroup couponsGroup = null;

            string key = LayeredCacheKeys.GetCouponsGroupKey(couponsGroupId, groupId);
            if (!LayeredCache.Instance.Get<CouponsGroup>(key, ref couponsGroup, GetCouponsGroup, new Dictionary<string, object>() { { "groupId", groupId }, { "couponsGroupId", couponsGroupId } },
                groupId, LayeredCacheConfigNames.GET_COUPONS_GROUP, new List<string>() { LayeredCacheKeys.GetCouponsGroupInvalidationKey(groupId, couponsGroupId) }))
            {
                log.ErrorFormat("Failed coupons group from LayeredCache, key: {0}", key);
            }

            return couponsGroup;
        }


        private static Tuple<CouponsGroup, bool> GetCouponsGroup(Dictionary<string, object> funcParams)
        {
            bool result = false;
            CouponsGroup couponsGroup = null;

            try
            {
                if (funcParams != null && funcParams.Count == 2)
                {
                    if (funcParams.ContainsKey("groupId") && funcParams.ContainsKey("couponsGroupId"))
                    {
                        int? groupId = funcParams["groupId"] as int?;
                        long? couponsGroupId = funcParams["couponsGroupId"] as long?;

                        if (groupId.HasValue && couponsGroupId.HasValue)
                        {
                            DataTable dt = PricingDAL.GetCouponsGroup(groupId.Value, couponsGroupId.Value);

                            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                            {

                                string sDiscountCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "DISCOUNT_CODE");
                                BaseDiscount t = null;
                                Utils.GetBaseImpl(ref t, groupId.Value);

                                couponsGroup = new CouponsGroup()
                                {
                                    m_sGroupCode = couponsGroupId.Value.ToString(),
                                    m_oDiscountCode = t.GetDiscountCodeData(sDiscountCode),
                                    m_dStartDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0], "START_DATE"),
                                    m_dEndDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0], "END_DATE"),
                                    m_nMaxUseCountForCoupon = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "MAX_USE_TIME"),
                                    m_sGroupName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "CODE"),
                                    m_nFinancialEntityID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "FINANCIAL_ENTITY_ID"),
                                    m_nMaxRecurringUsesCountForCoupon = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "MAX_RECURRING_USES"),
                                    couponGroupType = (CouponGroupType)ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "COUPON_GROUP_TYPE"),
                                    maxDomainUses = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "DOMAIN_MAX_USES"),
                                    m_sDiscountCode = sDiscountCode
                                };

                                result = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetCouponsGroup failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<CouponsGroup, bool>(couponsGroup, result);
        }

    }    
}
