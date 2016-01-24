using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses.Pricing
{
    public class Currency
    {

        [JsonProperty(PropertyName = "currencyCD3")]
        public string currencyCD3;

        [JsonProperty(PropertyName = "currencyCD2")]
        public string currencyCD2;

        [JsonProperty(PropertyName = "currencySign")]
        public string currencySign;

        [JsonProperty(PropertyName = "currencyID")]
        public Int32 currencyID;        

        public Currency()
        {
            currencyCD2 = string.Empty;
            currencyCD3 = string.Empty;
            currencyID = 0;
            currencySign = string.Empty;
        }

        public Currency(TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.Currency sourceCurrency)
        {
            currencyID = sourceCurrency.m_nCurrencyID;
            currencySign = sourceCurrency.m_sCurrencySign;
            currencyCD2 = sourceCurrency.m_sCurrencyCD2;
            currencyCD3 = sourceCurrency.m_sCurrencyCD3;            
        }

        public void InitializeById(Int32 nID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from lu_currency with (nolock) where";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        currencyCD3 = selectQuery.Table("query").DefaultView[0].Row["code3"].ToString();
                        currencyCD2 = selectQuery.Table("query").DefaultView[0].Row["code2"].ToString();
                        currencySign = selectQuery.Table("query").DefaultView[0].Row["CURRENCY_SIGN"].ToString();
                        currencyID = nID;
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
        }

        public void InitializeByCode3(string sCode3)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from lu_currency with (nolock) where";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("code3", "=", sCode3);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        currencyCD3 = sCode3;
                        currencyCD2 = selectQuery.Table("query").DefaultView[0].Row["code2"].ToString();
                        currencySign = selectQuery.Table("query").DefaultView[0].Row["CURRENCY_SIGN"].ToString();
                        currencyID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
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
        }

        public void InitializeByCode2(string sCode2)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from lu_currency with (nolock) where";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("code2", "=", sCode2);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        currencyCD2 = sCode2;
                        currencyCD3 = selectQuery.Table("query").DefaultView[0].Row["code3"].ToString();
                        currencySign = selectQuery.Table("query").DefaultView[0].Row["CURRENCY_SIGN"].ToString();
                        currencyID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
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
        }
    }
}
