using ApiObjects.Pricing;
using CachingProvider.LayeredCache;
using Core.GroupManagers;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Core.Pricing
{
    public class TvinciDiscount: BaseDiscount
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public TvinciDiscount(Int32 nGroupID) : base(nGroupID)
        {
        }

        public override DiscountModule[] GetDiscountsModuleListForAdmin()
        {
            DiscountModule[] ret = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select id from discount_codes with (nolock) where is_active=1 and status=1 and START_DATE<getdate() and (end_date is null OR end_date>getdate()) and ";                
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        ret = new DiscountModule[nCount];
                    for (int i = 0; i < nCount; i++)
                    {
                        ret[i] = GetDiscountCodeData(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
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

        public override DiscountModule GetDiscountCodeData(string sDC, string currency = "")
        {
            DiscountModule discountModule = null;

            if (int.TryParse(sDC, out int discountId) && discountId > 0)
            {
                discountModule = GetDiscountByCountryAndCurrency(discountId, "--", currency, withDefaultFallback: true);
            }

            return discountModule;
        }

        public override DiscountModule GetDiscountCodeDataByCountryAndCurrency(int discountCodeId, string countryCode, string currencyCode)
        {
            DiscountModule discountModule = GetDiscountByCountryAndCurrency(discountCodeId, countryCode, currencyCode, withDefaultFallback: false);
            
            return discountModule;
        }

        private DiscountModule GetDiscountByCountryAndCurrency(int discountCodeId, string countryCode, string currencyCode, bool withDefaultFallback)
        {
            DiscountModule discountModule = null;

            if (discountCodeId > 0)
            {
                discountModule = Utils.GetDiscountModuleByCountryAndCurrency(m_nGroupID, discountCodeId, countryCode, currencyCode, withDefaultFallback);
            }

            return discountModule;
        }
    }
}
