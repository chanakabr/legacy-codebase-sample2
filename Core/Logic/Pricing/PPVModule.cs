using ApiObjects;
using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using APILogic;

namespace Core.Pricing
{
    [Serializable]
    public class PPVModule
    {
        #region Members
        public PriceCode m_oPriceCode;
        public UsageModule m_oUsageModule;
        public DiscountModule m_oDiscountModule;
        public CouponsGroup m_oCouponsGroup;
        public LanguageContainer[] m_sDescription;
        public string m_sObjectCode;
        public string m_sObjectVirtualName;
        public bool m_bSubscriptionOnly;
        public List<int> m_relatedFileTypes;
        public string m_Product_Code;
        public bool m_bFirstDeviceLimitation;
        public string alias;
        public string ExternalId { get; set; }
        public AdsPolicy? AdsPolicy { get; set; }
        public string AdsParam { get; set; }
        public bool? IsActive;
        public DateTime? CreateDate;
        public DateTime? UpdateDate;
        public long? VirtualAssetId { get; set; }
        public long? AssetUserRuleId { get; set; }
        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("PPVModule. ");
            sb.Append(String.Concat("Code: ", m_sObjectCode));
            sb.Append(String.Concat(" Prd Cd: ", m_Product_Code));
            sb.Append(String.Concat(" Obj Virtual Name: ", m_sObjectVirtualName));

            return sb.ToString();
        }

        public PPVModule()
        {
            m_oPriceCode = new PriceCode();
            m_oUsageModule = new UsageModule();
            m_oDiscountModule = new DiscountModule();
            m_oCouponsGroup = new CouponsGroup();
            m_bSubscriptionOnly = false;

            m_sObjectVirtualName = string.Empty;
            m_Product_Code = string.Empty;
            m_bFirstDeviceLimitation = false;
            alias = string.Empty;

        }

        public PPVModule(PPVModule other) {
            m_oPriceCode = Extensions.Clone(other.m_oPriceCode);
            m_oUsageModule = Extensions.Clone(other.m_oUsageModule);
            m_oDiscountModule = Extensions.Clone(other.m_oDiscountModule);
            m_oCouponsGroup = Extensions.Clone(other.m_oCouponsGroup);
            m_sDescription = Extensions.Clone(other.m_sDescription);
            m_sObjectCode = other.m_sObjectCode;
            m_sObjectVirtualName = other.m_sObjectVirtualName;
            m_bSubscriptionOnly = other.m_bSubscriptionOnly;
            m_relatedFileTypes = other.m_relatedFileTypes?.ToList();
            m_Product_Code = other.m_Product_Code;
            m_bFirstDeviceLimitation = other.m_bFirstDeviceLimitation;
            alias = other.alias;
            ExternalId = other.ExternalId;
            AdsParam = other.AdsParam;
            IsActive = other.IsActive;
            CreateDate = other.CreateDate;
            UpdateDate = other.UpdateDate;
            VirtualAssetId = other.VirtualAssetId;
            AssetUserRuleId = other.AssetUserRuleId;
        }
        
        public void Initialize(PriceCode oPriceCode, UsageModule oUsageModule,
            DiscountModule oDiscountModule, CouponsGroup oCouponsGroup, LanguageContainer[] sDescriptions,
            string sPPVCode, bool bSubscriptionOnly, string sObjectVirtualName, List<int> fileTypes, bool bFirstDeviceLimitation, 
            int dlmID = 0, AdsPolicy? adsPolicy = null, string adsParam = null)
        {
            m_oCouponsGroup = oCouponsGroup;
            m_oDiscountModule = oDiscountModule;
            m_oPriceCode = oPriceCode;
            m_oUsageModule = oUsageModule;
            m_sDescription = sDescriptions;
            m_sObjectCode = sPPVCode;
            m_bSubscriptionOnly = bSubscriptionOnly;
            m_sObjectVirtualName = sObjectVirtualName;
            m_relatedFileTypes = fileTypes;
            m_bFirstDeviceLimitation = bFirstDeviceLimitation;
            AdsPolicy = adsPolicy;
            AdsParam = adsParam;

        }

        public void Initialize(string sPriceCode, string sUsageModuleCode,
            string sDiscountModuleCode, string sCouponGroupCode, LanguageContainer[] sDescriptions, Int32 nGroupID,
            string sPPVCode, bool bSubscriptionOnly, string sObjectVirtualName, string sCountryCd, string sLANGUAGE_CODE, 
            string sDEVICE_NAME, List<int> fileTypes, bool bFirstDeviceLimitation, string productCode, int dlmID = 0, AdsPolicy? adsPolicy = null, 
            string adsParam = null, DateTime? createDate = null, DateTime? updateDate = null, bool? isActive = null, long? virtualAssetId = null, long? assetUserRuleId = null)
        {
            m_bSubscriptionOnly = bSubscriptionOnly;
            m_sObjectVirtualName = sObjectVirtualName;
            m_relatedFileTypes = fileTypes;
            VirtualAssetId = virtualAssetId;
            AssetUserRuleId = assetUserRuleId;

            if (createDate.HasValue)
            {
                CreateDate = createDate.Value;
            }
            
            if (updateDate.HasValue)
            {
                UpdateDate = updateDate.Value;
            }
            
            if (isActive.HasValue)
            {
                IsActive = isActive.Value;
            }
           
            if (sPriceCode.Length > 0)
            {
                BasePricing p = null;
                Utils.GetBaseImpl(ref p, nGroupID);
                if (p != null)
                    m_oPriceCode = p.GetPriceCodeData(sPriceCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                else
                    m_oPriceCode = null;
            }
            else
                m_oPriceCode = null;

            if (sUsageModuleCode.Length > 0)
            {
                BaseUsageModule um = null;
                Utils.GetBaseImpl(ref um, nGroupID);
                if (um != null)
                    m_oUsageModule = um.GetUsageModuleData(sUsageModuleCode);
                else
                    m_oUsageModule = null;
            }
            else
                m_oUsageModule = null;

            if (sDiscountModuleCode.Length > 0)
            {
                BaseDiscount d = null;
                Utils.GetBaseImpl(ref d, nGroupID);
                if (d != null)
                {
                    var currency = m_oPriceCode?.m_oPrise?.m_oCurrency?.m_sCurrencyCD3 ?? "";
                    m_oDiscountModule = d.GetDiscountCodeData(sDiscountModuleCode, currency);
                }
                else
                    m_oDiscountModule = null;
            }
            else
                m_oDiscountModule = null;

            if (sCouponGroupCode.Length > 0)
            {
                BaseCoupons c = null;
                Utils.GetBaseImpl(ref c, nGroupID);
                if (c != null)
                    m_oCouponsGroup = c.GetCouponGroupData(sCouponGroupCode);
                else
                    m_oCouponsGroup = null;
            }
            else
                m_oCouponsGroup = null;

            m_sDescription = sDescriptions;
            m_sObjectCode = sPPVCode;
            m_bFirstDeviceLimitation = bFirstDeviceLimitation;
            m_Product_Code = productCode;
            AdsPolicy = adsPolicy;
            AdsParam = adsParam;
        }

        public void Initialize(PriceCode oPriceCode, UsageModule oUsageModule,
               DiscountModule oDiscountModule, CouponsGroup oCouponsGroup, LanguageContainer[] sDescriptions,
               string sPPVCode, bool bSubscriptionOnly, string sObjectVirtualName, List<int> fileTypes, bool bFirstDeviceLimitation, string alias ,int dlmID = 0, AdsPolicy? adsPolicy = null, string adsParam = null)
        {
            Initialize(oPriceCode, oUsageModule, oDiscountModule, oCouponsGroup, sDescriptions, sPPVCode, bSubscriptionOnly, sObjectVirtualName, fileTypes, bFirstDeviceLimitation, dlmID, adsPolicy, adsParam);
            this.alias = alias;
        }

        public void Initialize(string sPriceCode, string sUsageModuleCode,
            string sDiscountModuleCode, string sCouponGroupCode, LanguageContainer[] sDescriptions, Int32 nGroupID,
            string sPPVCode, bool bSubscriptionOnly, string sObjectVirtualName,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, List<int> fileTypes, bool bFirstDeviceLimitation, string productCode, string alias, int dlmID = 0, 
            AdsPolicy? adsPolicy = null, string adsParam = null)
        {
            Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode, sDescriptions, nGroupID, sPPVCode, bSubscriptionOnly, sObjectVirtualName, sCountryCd, sLANGUAGE_CODE,
                sDEVICE_NAME, fileTypes, bFirstDeviceLimitation, productCode, dlmID, adsPolicy, adsParam);
            this.alias = alias;
        }
    
    }

    public class PPVModuleContainer
    {
        public PPVModuleContainer()
        {
            m_oPPVModule = null;
            m_bIsBelong = false;
            m_dStartDate = null;
            m_dEndDate = null;
        }

        public void Initialize(PPVModule oPPVModule, bool bIsBelong, DateTime? dStartDate, DateTime? dEndDate)
        {
            m_oPPVModule = oPPVModule;
            m_bIsBelong = bIsBelong;
            m_dStartDate = dStartDate;
            m_dEndDate = dEndDate;
        }

        public PPVModule m_oPPVModule;
        public bool m_bIsBelong;
        public DateTime? m_dStartDate;
        public DateTime? m_dEndDate;
    }
}
