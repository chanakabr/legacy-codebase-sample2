using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Core.Users
{
    public class Country
    {
        public Country()
        {
            m_nObjecrtID = 0;
            m_sCountryName = "";
            m_sCountryCode = "";
        }

        public bool Initialize(Int32 nCountryID)
        {
            bool res = false;

            string key = string.Format("users_CountryInitialize_{0}", nCountryID);
            Country oCountry;
            res = UsersCache.GetItem<Country>(key, out oCountry);

            if (!res || oCountry == null)
            {
                try
                {
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "SELECT COUNTRY_NAME, COUNTRY_CD2 FROM COUNTRIES WITH (NOLOCK) WHERE ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nCountryID);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            string sCountryName = selectQuery.Table("query").DefaultView[0].Row["COUNTRY_NAME"].ToString();
                            string sCountryCode = selectQuery.Table("query").DefaultView[0].Row["COUNTRY_CD2"].ToString();
                            Initialize(nCountryID, sCountryName, sCountryCode);
                            res = true;
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;
                }
                catch
                {
                    res = false;
                }
                if (res)
                {
                    UsersCache.AddItem(key, this);
                }
            }
            else
            {
                res = Initialize(oCountry);
            }

            return res;
        }

        private bool Initialize(Country oCountry)
        {
            try
            {
                m_sCountryCode = oCountry.m_sCountryCode;
                m_sCountryName = oCountry.m_sCountryName;
                m_nObjecrtID = oCountry.m_nObjecrtID;
                return true;
            }
            catch
            {
                return false;
            }

        }

        public void Initialize(Int32 nCountryID, string sCountryName, string sCountryCode)
        {
            m_sCountryCode = sCountryCode;
            m_sCountryName = sCountryName;
            m_nObjecrtID = nCountryID;
        }

        public bool InitializeByCode(string sCode)
        {
            bool bOK = false;
            string key = string.Format("users_CountryInitializeByCode_{0}", sCode);
            Country oCountry;
            bOK = UsersCache.GetItem<Country>(key, out oCountry);

            if (!bOK || oCountry == null)
            {
                try
                {
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "SELECT ID, COUNTRY_NAME FROM COUNTRIES WITH (NOLOCK) WHERE ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CD2", "=", sCode);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            Int32 nCountryID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                            string sCountryName = selectQuery.Table("query").DefaultView[0].Row["COUNTRY_NAME"].ToString();
                            Initialize(nCountryID, sCountryName, sCode);
                            bOK = true;
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;
                }
                catch
                {
                    bOK = false;
                }
                if (bOK)
                {
                    UsersCache.AddItem(key, this);
                }
            }
            else
            {
                bOK = Initialize(oCountry);
            }
            return bOK;
        }

        public bool InitializeByName(string sName)
        {
            bool bOK = false;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "SELECT ID, COUNTRY_CD2 FROM COUNTRIES WITH (NOLOCK) WHERE ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_NAME", "=", sName);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        Int32 nCountryID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                        string sCountryCode = selectQuery.Table("query").DefaultView[0].Row["COUNTRY_CD2"].ToString();
                        Initialize(nCountryID, sName, sCountryCode);
                        bOK = true;
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch
            {
                bOK = false;
            }

            return bOK;
        }

        public Int32 m_nObjecrtID;
        public string m_sCountryName;
        public string m_sCountryCode;
    }
}
