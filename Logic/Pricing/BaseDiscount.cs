using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public abstract class BaseDiscount
    {
        protected BaseDiscount() { }
        protected BaseDiscount(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        public abstract DiscountModule GetDiscountCodeData(string sDC);
        public abstract DiscountModule[] GetDiscountsModuleListForAdmin();
        public abstract DiscountModule GetDiscountCodeDataByCountryAndCurrency(int discountCodeId, string countryCode, string currencyCode);
        protected Int32 m_nGroupID;
    }
}
