using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using System.Data;
using DAL;
using ApiObjects.Pricing;

namespace Core.Pricing
{
    [Serializable]
    public class Subscription : PPVModule
    {
        #region Member
        //The codes which identify which medias are relevant to the subscription (int Tvinci it is the channels)
        public BundleCodeContainer[] m_sCodes;
        public DateTime m_dStartDate;
        public DateTime m_dEndDate;
        public Int32[] m_sFileTypes;
        public bool m_bIsRecurring;
        public Int32 m_nNumberOfRecPeriods;
        public PriceCode m_oSubscriptionPriceCode;
        public DiscountModule m_oExtDisountModule;
        public LanguageContainer[] m_sName;
        public UsageModule m_oSubscriptionUsageModule;
        public int m_fictivicMediaID;
        public long m_Priority;
        public string m_ProductCode = string.Empty;
        public string m_SubscriptionCode;
        public UsageModule[] m_MultiSubscriptionUsageModule;
        public int n_GeoCommerceID;
        public bool m_bIsInfiniteRecurring;
        public UserType[] m_UserTypes;
        public PreviewModule m_oPreviewModule;
        public int m_nDomainLimitationModule;
        public ServiceObject[] m_lServices;
        public int m_GracePeriodMinutes;

        public List<SubscriptionCouponGroup> CouponsGroups;
        public List<KeyValuePair<VerificationPaymentGateway, string>> ExternalProductCodes;

        #endregion

        #region Ctr
        public Subscription()
            : base()
        {
            m_sName = null;
            m_oSubscriptionUsageModule = null;
            m_MultiSubscriptionUsageModule = null;
            m_oPreviewModule = null;
            CouponsGroups = new List<SubscriptionCouponGroup>();
            ExternalProductCodes = new List<KeyValuePair<VerificationPaymentGateway, string>>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get Fictivic Media ID
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="subCode"></param>
        private void GetFictivicMediaID(int groupID, int subCode)
        {
            int fictivicGroupID = 0;
            int fictivicMediaID = 0;
            string paramName = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            ODBCWrapper.DataSetSelectQuery mediaSelectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select FICTIVIC_MEDIA_META_NAME, FICTIVIC_GROUP_ID from groups_parameters with (nolock) ";
                selectQuery += " where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        paramName = selectQuery.Table("query").DefaultView[0].Row["FICTIVIC_MEDIA_META_NAME"].ToString();
                        fictivicGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["FICTIVIC_GROUP_ID"].ToString());
                    }
                }

                if (fictivicGroupID > 0)
                {
                    mediaSelectQuery = new ODBCWrapper.DataSetSelectQuery();
                    mediaSelectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                    mediaSelectQuery += "select id from media with (nolock) ";
                    mediaSelectQuery += " where ";
                    mediaSelectQuery += ODBCWrapper.Parameter.NEW_PARAM(paramName, "=", subCode);
                    mediaSelectQuery += " and ";
                    mediaSelectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", fictivicGroupID);
                    if (mediaSelectQuery.Execute("query", true) != null)
                    {
                        int count = mediaSelectQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            fictivicMediaID = int.Parse(mediaSelectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                        }
                    }

                }
                m_fictivicMediaID = fictivicMediaID;
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (mediaSelectQuery != null)
                {
                    mediaSelectQuery.Finish();
                }
            }
        }

        public void Initialize(PriceCode oPriceCode, UsageModule oUsageModule,
            DiscountModule oDiscountModule, CouponsGroup oCouponsGroup, LanguageContainer[] sDescriptions,
            string sSubscriptionCode, BundleCodeContainer[] sCodes, DateTime dStart, DateTime dEnd,
            Int32[] sFileTypes, bool bIsRecurring, Int32 nNumOfRecPeriods, LanguageContainer[] sName, PriceCode subPriceCode, UsageModule oSubUsageModule, string sObjectVirtualName, 
            int nGeoCommerceID = 0, int dlmID = 0, AdsPolicy? adsPolicy = null, string adsParam = null)
        {
            Initialize(0, oPriceCode, oUsageModule, oDiscountModule, oCouponsGroup, sDescriptions, sSubscriptionCode, sCodes, dStart, dEnd,
                        sFileTypes, bIsRecurring, nNumOfRecPeriods, sName, subPriceCode, oSubUsageModule, sObjectVirtualName, 0, dlmID, adsPolicy, adsParam);
        }

        public void Initialize(PriceCode oPriceCode, UsageModule oUsageModule,
            DiscountModule oDiscountModule, DiscountModule extDisountModule, CouponsGroup oCouponsGroup, LanguageContainer[] sDescriptions,
            string sSubscriptionCode, BundleCodeContainer[] sCodes, DateTime dStart, DateTime dEnd,
            Int32[] sFileTypes, bool bIsRecurring, Int32 nNumOfRecPeriods, LanguageContainer[] sName, PriceCode subPriceCode, UsageModule oSubUsageModule, string sObjectVirtualName,
            int nGeoCommerceID = 0, int dlmID = 0, AdsPolicy? adsPolicy = null, string adsParam = null)
        {
            Initialize(0, oPriceCode, oUsageModule,
            oDiscountModule, oCouponsGroup, sDescriptions,
            sSubscriptionCode, sCodes, dStart, dEnd,
            sFileTypes, bIsRecurring, nNumOfRecPeriods, sName, subPriceCode, oSubUsageModule, sObjectVirtualName, nGeoCommerceID, dlmID, adsPolicy, adsParam);
            m_oExtDisountModule = extDisountModule;
        }

        public void Initialize(Int32 nGroupID, PriceCode oPriceCode, UsageModule oUsageModule,
            DiscountModule oDiscountModule, CouponsGroup oCouponsGroup, LanguageContainer[] sDescriptions,
            string sSubscriptionCode, BundleCodeContainer[] sCodes, DateTime dStart, DateTime dEnd,
            Int32[] sFileTypes, bool bIsRecurring, Int32 nNumOfRecPeriods, LanguageContainer[] sName, PriceCode subPriceCode, UsageModule oSubUsageModule, string sObjectVirtualName,
            int nGeoCommerceID = 0, int dlmID = 0, AdsPolicy? adsPolicy = null, string adsParam = null)
        {
            base.Initialize(oPriceCode, oUsageModule, oDiscountModule, oCouponsGroup, sDescriptions,
                sSubscriptionCode, false, sObjectVirtualName, null, false);

            m_SubscriptionCode = sSubscriptionCode;
            m_ProductCode = string.Empty;
            m_sCodes = sCodes;
            m_dStartDate = dStart;
            m_dEndDate = dEnd;
            m_sFileTypes = sFileTypes;
            m_bIsRecurring = bIsRecurring;
            m_nNumberOfRecPeriods = nNumOfRecPeriods;
            m_sName = sName;
            m_oSubscriptionPriceCode = subPriceCode;
            m_oSubscriptionUsageModule = oSubUsageModule;
            GetFictivicMediaID(nGroupID, int.Parse(sSubscriptionCode));
            n_GeoCommerceID = nGeoCommerceID;
            m_nDomainLimitationModule = dlmID;
            AdsPolicy = adsPolicy;
            AdsParam = adsParam;

        }

        public void Initialize(string sPriceCode, string sUsageModuleCode,
            string sDiscountModuleCode, string sCouponGroupCode, LanguageContainer[] sDescriptions, Int32 nGroupID,
            string sSubscriptionCode, BundleCodeContainer[] sCodes, DateTime dStart, DateTime dEnd,
            Int32[] sFileTypes, bool bIsRecurring, Int32 nNumOfRecPeriods, LanguageContainer[] sName, string subPriceCode,
            string sSubUsageModule, string sObjectVirtualName,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string priority, int nGeoCommerceID = 0, int dlmID = 0, AdsPolicy? adsPolicy = null, string adsParam = null)
        {
            base.Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode,
                sDescriptions, nGroupID, sSubscriptionCode, false, sObjectVirtualName,
                sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, null, false, string.Empty);

            if (sSubUsageModule.Length > 0)
            {
                BaseUsageModule um = null;
                Utils.GetBaseImpl(ref um, nGroupID);
                if (um != null)
                    m_oSubscriptionUsageModule = um.GetUsageModuleData(sSubUsageModule);
                else
                    m_oSubscriptionUsageModule = null;
            }
            else
                m_oSubscriptionUsageModule = null;

            m_SubscriptionCode = sSubscriptionCode;
            m_ProductCode = string.Empty;
            m_sCodes = sCodes;
            m_dStartDate = dStart;
            m_dEndDate = dEnd;
            m_sFileTypes = sFileTypes;
            m_bIsRecurring = bIsRecurring;
            m_nNumberOfRecPeriods = nNumOfRecPeriods;
            m_sName = sName;
            n_GeoCommerceID = nGeoCommerceID;
            m_nDomainLimitationModule = dlmID;
            AdsPolicy = adsPolicy;
            AdsParam = adsParam;

            if (!string.IsNullOrEmpty(priority))
            {
                m_Priority = long.Parse(priority);
            }
            GetFictivicMediaID(nGroupID, int.Parse(sSubscriptionCode));

            if (subPriceCode.Length > 0)
            {
                BasePricing p = null;
                Utils.GetBaseImpl(ref p, nGroupID);
                if (p != null)
                    m_oSubscriptionPriceCode = p.GetPriceCodeData(subPriceCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                else
                    m_oSubscriptionPriceCode = null;
            }
            else
                m_oSubscriptionPriceCode = null;

            InitializeMultiUsageModule(nGroupID, sSubscriptionCode);
        }

        public void Initialize(string sPriceCode, string sUsageModuleCode,
           string sDiscountModuleCode, string sCouponGroupCode, LanguageContainer[] sDescriptions, Int32 nGroupID,
           string sSubscriptionCode, BundleCodeContainer[] sCodes, DateTime dStart, DateTime dEnd,
           Int32[] sFileTypes, bool bIsRecurring, Int32 nNumOfRecPeriods, LanguageContainer[] sName, string subPriceCode,
           string sSubUsageModule, string sObjectVirtualName,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string priority, string sProductCode, string sExtDiscount, UserType[] userTypes, ServiceObject[] services,
            long lPreviewModuleID, int nGeoCommerceID = 0, int dlmID = 0, int gracePeriodMinutes = 0, AdsPolicy? adsPolicy = null, string adsParam = null,
             List<SubscriptionCouponGroup> couponsGroup = null, List<KeyValuePair<VerificationPaymentGateway, string>> externalProductCodes = null)
        {
            base.Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode,
                sDescriptions, nGroupID, sSubscriptionCode, false, sObjectVirtualName,
                sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, null, false, sProductCode);

            if (sSubUsageModule.Length > 0)
            {
                BaseUsageModule um = null;
                Utils.GetBaseImpl(ref um, nGroupID);
                if (um != null)
                    m_oSubscriptionUsageModule = um.GetUsageModuleData(sSubUsageModule);
                else
                    m_oSubscriptionUsageModule = null;
            }
            else
                m_oSubscriptionUsageModule = null;

            this.m_GracePeriodMinutes = gracePeriodMinutes;
            m_SubscriptionCode = sSubscriptionCode;
            m_ProductCode = sProductCode;
            m_sCodes = sCodes;
            m_dStartDate = dStart;
            m_dEndDate = dEnd;
            m_sFileTypes = sFileTypes;
            m_bIsRecurring = bIsRecurring;
            m_nNumberOfRecPeriods = nNumOfRecPeriods;
            m_sName = sName;
            m_nDomainLimitationModule = dlmID;
            if (!string.IsNullOrEmpty(priority))
            {
                m_Priority = long.Parse(priority);
            }
            GetFictivicMediaID(nGroupID, int.Parse(sSubscriptionCode));

            n_GeoCommerceID = nGeoCommerceID;

            if (subPriceCode.Length > 0)
            {
                BasePricing p = null;
                Utils.GetBaseImpl(ref p, nGroupID);
                if (p != null)
                    m_oSubscriptionPriceCode = p.GetPriceCodeData(subPriceCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                else
                    m_oSubscriptionPriceCode = null;
            }
            else
                m_oSubscriptionPriceCode = null;


            if (!string.IsNullOrEmpty(sExtDiscount))
            {
                BaseDiscount d = null;
                Utils.GetBaseImpl(ref d, nGroupID);
                if (d != null)
                    m_oExtDisountModule = d.GetDiscountCodeData(sExtDiscount);
                else
                    m_oExtDisountModule = null;
            }
            else
                m_oExtDisountModule = null;

            m_UserTypes = userTypes;
            InitializeMultiUsageModule(nGroupID, sSubscriptionCode);
            m_oPreviewModule = GetPreviewModule(nGroupID, lPreviewModuleID);
            m_lServices = services;
            AdsPolicy = adsPolicy;
            AdsParam = adsParam;
            this.CouponsGroups = couponsGroup;
            if (externalProductCodes != null)
            {
                this.ExternalProductCodes = externalProductCodes;
            }
        }

        private void InitializeMultiUsageModule(int nGroupID, string sSubscriptionCode)
        {

            BaseUsageModule um = null;
            Utils.GetBaseImpl(ref um, nGroupID);

            m_bIsInfiniteRecurring = m_bIsRecurring;

            if (um != null)
            {
                m_MultiSubscriptionUsageModule = um.GetSubscriptionUsageModuleList(sSubscriptionCode);
                int periodnum = 0;
                if (m_MultiSubscriptionUsageModule != null)
                {
                    foreach (UsageModule u in m_MultiSubscriptionUsageModule)
                    {
                        if (u == null)
                            continue;
                        if (u.m_is_renew == 1 && u.m_num_of_rec_periods == 0)
                        {
                            periodnum = 0;
                            m_bIsRecurring = true;
                            m_bIsInfiniteRecurring = true;
                            break;
                        }
                        else if (u.m_is_renew == 0)
                        {
                            periodnum++;
                        }
                        else if (u.m_is_renew == 1 && u.m_num_of_rec_periods > 0)
                        {
                            periodnum += u.m_num_of_rec_periods + 1;
                            m_bIsRecurring = true;
                        }
                    }

                    if (periodnum > 0 && m_MultiSubscriptionUsageModule.Length > 1)
                    {
                        m_bIsRecurring = true;
                    }
                    this.m_nNumberOfRecPeriods = periodnum;
                }
            }
            else
            {
                m_MultiSubscriptionUsageModule = null;
            }
        }

        private PreviewModule GetPreviewModule(int nGroupID, long nPreviewModuleID)
        {
            PreviewModule res = null;
            DataTable dt = PricingDAL.Get_PreviewModuleData(nGroupID, nPreviewModuleID);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                res = new PreviewModule();
                res.m_nID = nPreviewModuleID;
                if (dt.Rows[0]["NAME"] != DBNull.Value && dt.Rows[0]["NAME"] != null)
                    res.m_sName = dt.Rows[0]["Name"].ToString();
                if (dt.Rows[0]["FULL_LIFE_CYCLE_ID"] != DBNull.Value && dt.Rows[0]["FULL_LIFE_CYCLE_ID"] != null)
                    res.m_tsFullLifeCycle = Int32.Parse(dt.Rows[0]["FULL_LIFE_CYCLE_ID"].ToString());
                if (dt.Rows[0]["NON_RENEWING_PERIOD_ID"] != DBNull.Value && dt.Rows[0]["NON_RENEWING_PERIOD_ID"] != null)
                    res.m_tsNonRenewPeriod = Int32.Parse(dt.Rows[0]["NON_RENEWING_PERIOD_ID"].ToString());
            }

            return res;

        }
        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Subscription. ");
            sb.Append(String.Concat("PPV Obj Code: ", m_sObjectCode != null ? m_sObjectCode : "null"));
            sb.Append(String.Concat(" Sub Code: ", m_SubscriptionCode != null ? m_SubscriptionCode : "null"));
            return sb.ToString();
        }

    }
}
