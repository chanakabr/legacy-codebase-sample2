using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public void Initialize(PriceCode oPriceCode, UsageModule oUsageModule,
            DiscountModule oDiscountModule, CouponsGroup oCouponsGroup, LanguageContainer[] sDescriptions,
            string sPPVCode, bool bSubscriptionOnly, string sObjectVirtualName, List<int> fileTypes, bool bFirstDeviceLimitation, int dlmID = 0)
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

        }

        public void Initialize(string sPriceCode, string sUsageModuleCode,
            string sDiscountModuleCode, string sCouponGroupCode, LanguageContainer[] sDescriptions, Int32 nGroupID,
            string sPPVCode, bool bSubscriptionOnly, string sObjectVirtualName,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, List<int> fileTypes, bool bFirstDeviceLimitation, string productCode, int dlmID = 0)
        {
            m_bSubscriptionOnly = bSubscriptionOnly;
            m_sObjectVirtualName = sObjectVirtualName;
            m_relatedFileTypes = fileTypes;
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
                    m_oDiscountModule = d.GetDiscountCodeData(sDiscountModuleCode);
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
        }

        public void Initialize(PriceCode oPriceCode, UsageModule oUsageModule,
               DiscountModule oDiscountModule, CouponsGroup oCouponsGroup, LanguageContainer[] sDescriptions,
               string sPPVCode, bool bSubscriptionOnly, string sObjectVirtualName, List<int> fileTypes, bool bFirstDeviceLimitation, string alias ,int dlmID = 0)
        {
            Initialize(oPriceCode, oUsageModule, oDiscountModule, oCouponsGroup, sDescriptions, sPPVCode, bSubscriptionOnly, sObjectVirtualName, fileTypes, bFirstDeviceLimitation, dlmID);
            this.alias = alias;
        }

        public void Initialize(string sPriceCode, string sUsageModuleCode,
            string sDiscountModuleCode, string sCouponGroupCode, LanguageContainer[] sDescriptions, Int32 nGroupID,
            string sPPVCode, bool bSubscriptionOnly, string sObjectVirtualName,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, List<int> fileTypes, bool bFirstDeviceLimitation, string productCode, string alias, int dlmID = 0)
        {
            Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode, sDescriptions, nGroupID, sPPVCode, bSubscriptionOnly, sObjectVirtualName, sCountryCd, sLANGUAGE_CODE, 
                sDEVICE_NAME, fileTypes, bFirstDeviceLimitation, productCode, dlmID);
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
