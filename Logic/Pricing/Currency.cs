using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public class Currency
    {
        public Currency()
        {
            this.m_sCurrencyCD2 = string.Empty;
            this.m_sCurrencyCD3 = string.Empty;
            this.m_nCurrencyID = 0;
            this.m_sCurrencySign = string.Empty;
            this.m_sCurrencyName = string.Empty;
            this.m_bIsDefault = false;
        }

        public Currency(int id, string name, string code, string sign, bool isDefault)
        {
            this.m_nCurrencyID = id;
            this.m_sCurrencyName = name;
            this.m_sCurrencyCD2 = code;
            this.m_sCurrencySign = sign;
            this.m_sCurrencyCD3 = string.Empty;
            this.m_bIsDefault = isDefault;
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
