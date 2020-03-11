using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{

    [Serializable]
    public class PrePaidModule
    {

        //The price code for the pre paid module
        public PriceCode m_PriceCode;
        //The credit bought with the pre paid
        public PriceCode m_CreditValue;
        public UsageModule m_UsageModule;
        public DiscountModule m_DiscountModule;
        public CouponsGroup m_CouponsGroup;
        public LanguageContainer[] m_Description;
        public int m_ObjectCode;
        public string m_Title;
        //Is the pre paid with fixed credit amount or chargeable
        public bool m_isFixedCredit;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("PrePaidModule. ");
            sb.Append(String.Concat(" Obj Cd: ", m_ObjectCode));
            sb.Append(String.Concat(" Title: ", m_Title));

            return sb.ToString();
        }

        public PrePaidModule()
        {
            m_PriceCode = new PriceCode();
            m_UsageModule = new UsageModule();
            m_DiscountModule = new DiscountModule();
            m_CouponsGroup = new CouponsGroup();
            m_isFixedCredit = false;
            m_Title = string.Empty;
            m_CreditValue = new PriceCode();
        }

        public void Initialize(string sPriceCode, string sCreditCode, string sUsageModuleCode,
            string sDiscountModuleCode, string sCouponGroupCode, Int32 nGroupID,
            int nPrePaidCode, bool bIsFixedPrice, string sTitle,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            m_isFixedCredit = bIsFixedPrice;
            m_Title = sTitle;
            BasePricing p = null;
            Utils.GetBaseImpl(ref p, nGroupID);
            //Get the price object
            if (!string.IsNullOrEmpty(sPriceCode))
            {

                if (p != null)
                {
                    m_PriceCode = p.GetPriceCodeData(sPriceCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                }
                else
                {
                    m_PriceCode = null;
                }
            }
            else
            {
                m_PriceCode = null;
            }

            //Get the credit price object
            if (!string.IsNullOrEmpty(sCreditCode))
            {
                if (p != null)
                {
                    m_CreditValue = p.GetPriceCodeData(sCreditCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                }
                else
                {
                    m_CreditValue = null;
                }
            }
            else
            {
                m_CreditValue = null;
            }

            if (!string.IsNullOrEmpty(sUsageModuleCode))
            {
                BaseUsageModule um = null;
                Utils.GetBaseImpl(ref um, nGroupID);
                if (um != null)
                    m_UsageModule = um.GetUsageModuleData(sUsageModuleCode);
                else
                    m_UsageModule = null;
            }
            else
                m_UsageModule = null;

            if (!string.IsNullOrEmpty(sDiscountModuleCode))
            {
                BaseDiscount d = null;
                Utils.GetBaseImpl(ref d, nGroupID);
                if (d != null)
                    m_DiscountModule = d.GetDiscountCodeData(sDiscountModuleCode);
                else
                    m_DiscountModule = null;
            }
            else
                m_DiscountModule = null;

            if (!string.IsNullOrEmpty(sCouponGroupCode))
            {
                BaseCoupons c = null;
                Utils.GetBaseImpl(ref c, nGroupID);
                if (c != null)
                    m_CouponsGroup = c.GetCouponGroupData(sCouponGroupCode);
                else
                    m_CouponsGroup = null;
            }
            else
                m_CouponsGroup = null;

            m_ObjectCode = nPrePaidCode;

            m_Description = GetDescription();
        }

        private LanguageContainer[] GetDescription()
        {
            LanguageContainer[] theContainer = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select * from pre_paid_descriptions with (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pre_paid_module_id", "=", m_ObjectCode);
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

    }
}
