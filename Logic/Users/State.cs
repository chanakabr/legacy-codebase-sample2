using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class State
    {
        public State()
        {
            m_nObjecrtID = 0;
            m_sStateName = "";
            m_sStateCode = "";
            m_Country = null;
        }

        public bool Initialize(Int32 nStateID)
        {
            bool res = false;

            string key = string.Format("users_StateInitialize_{0}", nStateID);
            State oState;
            res = UsersCache.GetItem<State>(key, out oState);
            if (!res || oState == null )
            {
                try
                {
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "SELECT STATE_NAME, STATE_CD2, COUNTRY_ID FROM STATES WITH (NOLOCK) WHERE ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nStateID);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            string sCountryName = selectQuery.Table("query").DefaultView[0].Row["STATE_NAME"].ToString();
                            string sCountryCode = selectQuery.Table("query").DefaultView[0].Row["STATE_CD2"].ToString();
                            Int32 nCountryID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["COUNTRY_ID"].ToString());
                            res = Initialize(nStateID, sCountryName, sCountryCode, nCountryID);
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;
                    if (res)
                    {
                        UsersCache.AddItem(key, this);
                    }
                }
                catch
                {
                    res = false;
                }
            }
            else
            {
                res = Initialize(oState);
            }
            return res;
        }

        private bool Initialize(State oState)
        {
            try
            {
                m_Country = oState.m_Country;
                m_nObjecrtID = oState.m_nObjecrtID;
                m_sStateCode = oState.m_sStateCode;
                m_sStateName = oState.m_sStateName;
                return true;
            }
            catch 
            {
                return false;
            }

        }

        public bool Initialize(Int32 nStateID, string sStateName, string sStateCode , Int32 nCountryID)
        {
            m_Country = new Country();
            bool res = m_Country.Initialize(nCountryID);
            m_nObjecrtID = nStateID;
            m_sStateCode = sStateCode;
            m_sStateName = sStateName;

            return res;
        }

        public bool InitializeByCode(string sCode , Int32 nCountryID)
        {
            bool bOK = false;

            string key = string.Format("users_StateInitializeByCode_{0}_{1}", sCode, nCountryID);
            State oState;
            bOK = UsersCache.GetItem<State>(key, out oState);
            if (!bOK || oState == null)
            {
                try
                {
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "SELECT ID, STATE_NAME FROM STATES WITH (NOLOCK) WHERE ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATE_CD2", "=", sCode);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            Int32 nStateID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                            string sStateName = selectQuery.Table("query").DefaultView[0].Row["STATE_NAME"].ToString();

                            bOK = Initialize(nStateID, sStateName, sCode, nCountryID);
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
                bOK = Initialize(oState);
            }
            return bOK;
        }

        public bool InitializeByName(string sName, Int32 nCountryID)
        {
            bool bOK = false;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "SELECT ID, STATE_CD2 FROM STATES WITH (NOLOCK) WHERE ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATE_NAME", "=", sName);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        Int32 nStateID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                        string sStateCode = selectQuery.Table("query").DefaultView[0].Row["STATE_CD2"].ToString();
                        bOK = Initialize(nStateID, sName, sStateCode, nCountryID);
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
        public string m_sStateName;
        public string m_sStateCode;
        public Country m_Country;
    }
}
