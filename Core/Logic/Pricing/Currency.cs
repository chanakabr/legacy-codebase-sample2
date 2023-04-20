using System;

namespace Core.Pricing
{
    [Serializable]
    public class Currency 
    {
        public Currency()
        {
            m_sCurrencyCD2 = string.Empty;
            m_sCurrencyCD3 = string.Empty;
            m_nCurrencyID = 0;
            m_sCurrencySign = string.Empty;
            m_sCurrencyName = string.Empty;
            m_bIsDefault = false;
        } 

        public Currency(int id, string name, string code, string sign, bool isDefault)
        {
            m_nCurrencyID = id;
            m_sCurrencyName = name;
            m_sCurrencyCD2 = code;
            m_sCurrencySign = sign;
            m_sCurrencyCD3 = string.Empty;
            m_bIsDefault = isDefault;
        }

        public Currency(Currency currency)
        {
            m_nCurrencyID = currency.m_nCurrencyID;
            m_sCurrencyName = currency.m_sCurrencyName;
            m_sCurrencyCD2 = currency.m_sCurrencyCD2;
            m_sCurrencySign = currency.m_sCurrencySign;
            m_sCurrencyCD3 = currency.m_sCurrencyCD3;
            m_bIsDefault = currency.m_bIsDefault;
        }

        public void InitializeById(Int32 nID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("PRICING_CONNECTION");
                selectQuery += "select * from lu_currency with (nolock) where";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        m_sCurrencyCD3 = selectQuery.Table("query").DefaultView[0].Row["code3"].ToString();
                        m_sCurrencyCD2 = selectQuery.Table("query").DefaultView[0].Row["code2"].ToString();
                        m_sCurrencySign = selectQuery.Table("query").DefaultView[0].Row["CURRENCY_SIGN"].ToString();
                        m_nCurrencyID = nID;
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
                selectQuery.SetConnectionKey("PRICING_CONNECTION");
                selectQuery += "select * from lu_currency with (nolock) where";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("code3", "=", sCode3);
                selectQuery.SetCachedSec(604800);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        m_sCurrencyCD3 = sCode3;
                        m_sCurrencyCD2 = selectQuery.Table("query").DefaultView[0].Row["code2"].ToString();
                        m_sCurrencySign = selectQuery.Table("query").DefaultView[0].Row["CURRENCY_SIGN"].ToString();
                        m_nCurrencyID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
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
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select * from lu_currency with (nolock) where";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("code2", "=", sCode2);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        m_sCurrencyCD2 = sCode2;
                        m_sCurrencyCD3 = selectQuery.Table("query").DefaultView[0].Row["code3"].ToString();
                        m_sCurrencySign = selectQuery.Table("query").DefaultView[0].Row["CURRENCY_SIGN"].ToString();
                        m_nCurrencyID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
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

        public string m_sCurrencyCD3;
        public string m_sCurrencyCD2;
        public string m_sCurrencySign;
        public string m_sCurrencyName;
        public Int32 m_nCurrencyID;
        public bool m_bIsDefault;
    }
}
