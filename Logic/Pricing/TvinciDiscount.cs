using ApiObjects.Pricing;
using CachingProvider.LayeredCache;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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

        public override DiscountModule GetDiscountCodeData(string sDC)
        {
            Int32 nDiscountCodeID = 0;
            bool isDiscountID = false;
            DiscountModule tmp = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            isDiscountID = int.TryParse(sDC,out nDiscountCodeID);

            try
            {
                if (isDiscountID && nDiscountCodeID > 0)
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("PRICING_CONNECTION");
                    selectQuery += "select * from discount_codes with (nolock) where is_active=1 and status=1 and ";
                    selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nDiscountCodeID);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            tmp = new DiscountModule();
                            string sCode = selectQuery.Table("query").DefaultView[0].Row["CODE"].ToString(); ;
                            Int32 nPriceCodeID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString()); ;
                            double dPrice = double.Parse(selectQuery.Table("query").DefaultView[0].Row["PRICE"].ToString());
                            double dDiscountPercent = double.Parse(selectQuery.Table("query").DefaultView[0].Row["DISCOUNT_PERCENT"].ToString());
                            RelationTypes theRelationType = ((RelationTypes)(int.Parse(selectQuery.Table("query").DefaultView[0].Row["RELATION_TYPE"].ToString())));
                            Int32 nCurrency = int.Parse(selectQuery.Table("query").DefaultView[0].Row["CURRENCY_CD"].ToString());
                            Price oPrise = new Price();
                            oPrise.InitializeByCodeID(nCurrency, dPrice);
                            DateTime dStart = new DateTime(2000, 1, 1);
                            DateTime dEnd = new DateTime(2099, 1, 1);
                            if (selectQuery.Table("query").DefaultView[0].Row["START_DATE"] != null &&
                                selectQuery.Table("query").DefaultView[0].Row["START_DATE"] != DBNull.Value)
                                dStart = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["START_DATE"]);

                            if (selectQuery.Table("query").DefaultView[0].Row["END_DATE"] != null &&
                                selectQuery.Table("query").DefaultView[0].Row["END_DATE"] != DBNull.Value)
                                dEnd = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["END_DATE"]);

                            WhenAlgoType oWhenAlgoType = (WhenAlgoType)(int.Parse(selectQuery.Table("query").DefaultView[0].Row["WHENALGO_TYPE"].ToString()));
                            Int32 nWhenAlgoTimes = int.Parse(selectQuery.Table("query").DefaultView[0].Row["WHENALGO_TIMES"].ToString());
                            WhenAlgo wa = new WhenAlgo();
                            wa.Initialize(oWhenAlgoType, nWhenAlgoTimes);

                            tmp.Initialize(sCode, oPrise, DiscountModule.GetDiscountCodeDescription(nPriceCodeID), nPriceCodeID, dDiscountPercent, theRelationType, dStart, dEnd, wa);
                        }
                    }

                    return tmp;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
        }

        public override DiscountModule GetDiscountCodeDataByCountryAndCurrency(int discountCodeId, string countryCode, string currencyCode)
        {
            DiscountModule discountModule = null;
            if (discountCodeId > 0)
            {
                string key = LayeredCacheKeys.GetDiscountModuleCodeByCountryAndCurrencyKey(m_nGroupID, discountCodeId, countryCode, currencyCode);
                if (!LayeredCache.Instance.Get<DiscountModule>(key, ref discountModule, Utils.GetDiscountModuleByCountryAndCurrency, new Dictionary<string, object>() { { "groupId", m_nGroupID },
                                                            { "discountCodeId", discountCodeId }, { "countryCode", countryCode }, { "currencyCode", currencyCode } },
                                                            m_nGroupID, LayeredCacheConfigNames.DISCOUNT_MODULE_LOCALE_LAYERED_CACHE_CONFIG_NAME))
                {
                    log.ErrorFormat("Failed getting discountModule by countryCode and currencyCode from LayeredCache, priceCodeId: {0}, countryCode: {1},currencyCode: {2}, key: {3}",
                                    discountCodeId, countryCode, currencyCode, key);
                }
            }

            return discountModule;
        }
    }
}
