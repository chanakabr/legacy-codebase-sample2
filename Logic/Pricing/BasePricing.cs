using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Core.Pricing
{
    [Serializable]
    public abstract class BasePricing
    {
        protected static readonly string BASE_PRICING_LOG_FILE = "BasePricing";
        protected Int32 m_nGroupID;
        protected BasePricing() { }
        protected BasePricing(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        public int GroupID
        {
            get
            {
                return m_nGroupID;
            }
            protected set
            {
                m_nGroupID = value;
            }
        }

        public abstract PriceCode[] GetPriceCodeList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME);
        public abstract PriceCode GetPriceCodeData(string sPC, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME);
        public abstract PriceCode GetPriceCodeDataByCountyAndCurrency(int priceCodeId, string countryCode, string currencyCode);        


        public virtual ApiObjects.Response.Status InsertPriceCode(int groupID, string code, Price price)
        {
            throw new NotImplementedException("not implemented");
        }

        public virtual ApiObjects.Response.Status InsertDiscountCode(int groupID, DiscountModule discount)
        {
            throw new NotImplementedException();
        }

        public virtual ApiObjects.Response.Status InsertCouponGroup(int groupID, CouponsGroup coupon)
        {
            throw new NotImplementedException();
        }

        public virtual ApiObjects.Response.Status InsertUsageModule(int groupID, UsageModule usageModule)
        {
            throw new NotImplementedException();
        }
        

        public virtual ApiObjects.Response.Status UpdatetUsageModule(int groupID, UsageModule usageModule)
        {
            throw new NotImplementedException();
        }
        public virtual ApiObjects.Response.Status InsertPreviewModule(int groupID, PreviewModule previewModule)
        {
            throw new NotImplementedException();
        }

        public virtual BusinessModuleResponse InsertMPP(IngestMultiPricePlan mpp)
        {
            throw new NotImplementedException();
        }

        public virtual BusinessModuleResponse UpdateMPP(IngestMultiPricePlan multiPricePlan)
        {
            throw new NotImplementedException();
        }

        public virtual BusinessModuleResponse DeleteMPP(string multiPricePlan)
        {
            throw new NotImplementedException();
        }

        public virtual BusinessModuleResponse InsertPricePlan(IngestPricePlan pricePlan)
        {
            throw new NotImplementedException();
        }

        public virtual BusinessModuleResponse UpdatePricePlan(IngestPricePlan pricePlan)
        {
            throw new NotImplementedException();
        }

        public virtual BusinessModuleResponse DeletePricePlan(string pricePlan)
        {
            throw new NotImplementedException();
        }

        public virtual BusinessModuleResponse InsertPPV(IngestPPV ppv)
        {
            throw new NotImplementedException();
        }

        public virtual BusinessModuleResponse UpdatePPV(IngestPPV ppv)
        {
            throw new NotImplementedException();
        }

        public virtual BusinessModuleResponse DeletePPV(string ppv)
        {
            throw new NotImplementedException();
        }

    }
}
