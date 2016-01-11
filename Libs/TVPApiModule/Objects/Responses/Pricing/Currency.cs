using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses.Pricing
{
    public class Currency
    {

        [JsonProperty(PropertyName = "CurrencyCD3")]
        public string CurrencyCD3;

        [JsonProperty(PropertyName = "CurrencyCD2")]
        public string CurrencyCD2;

        [JsonProperty(PropertyName = "CurrencySign")]
        public string CurrencySign;

        [JsonProperty(PropertyName = "CurrencyID")]
        public Int32 CurrencyID;        

        public Currency()
        {
            CurrencyCD2 = string.Empty;
            CurrencyCD3 = string.Empty;
            CurrencyID = 0;
            CurrencySign = string.Empty;
        }

        public Currency(TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.Currency sourceCurrency)
        {
            CurrencyID = sourceCurrency.m_nCurrencyID;
            CurrencySign = sourceCurrency.m_sCurrencySign;
            CurrencyCD2 = sourceCurrency.m_sCurrencyCD2;
            CurrencyCD3 = sourceCurrency.m_sCurrencyCD3;            
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
                        CurrencyCD3 = selectQuery.Table("query").DefaultView[0].Row["code3"].ToString();
                        CurrencyCD2 = selectQuery.Table("query").DefaultView[0].Row["code2"].ToString();
                        CurrencySign = selectQuery.Table("query").DefaultView[0].Row["CURRENCY_SIGN"].ToString();
                        CurrencyID = nID;
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
                        CurrencyCD3 = sCode3;
                        CurrencyCD2 = selectQuery.Table("query").DefaultView[0].Row["code2"].ToString();
                        CurrencySign = selectQuery.Table("query").DefaultView[0].Row["CURRENCY_SIGN"].ToString();
                        CurrencyID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
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
                        CurrencyCD2 = sCode2;
                        CurrencyCD3 = selectQuery.Table("query").DefaultView[0].Row["code3"].ToString();
                        CurrencySign = selectQuery.Table("query").DefaultView[0].Row["CURRENCY_SIGN"].ToString();
                        CurrencyID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
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
