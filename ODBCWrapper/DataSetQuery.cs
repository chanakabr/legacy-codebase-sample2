using System;
using System.Data.Odbc;
using System.Web;
using System.Web.SessionState;
using System.Configuration;
using System.Data;
using KLogMonitor;
using System.Reflection;
using System.Data.SqlClient;

namespace ODBCWrapper
{
    /// <summary>
    /// Summary description for DataSetQuery.
    /// </summary>
    public class DataSetQuery : Query
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected DataSetQuery()
        {
            m_myDataSet = new System.Data.DataSet();
            command = null;

            if (ConfigurationManager.AppSettings["ODBC_CACH_SEC"] != null &&
                ConfigurationManager.AppSettings["ODBC_CACH_SEC"].ToString() != "")
            {
                m_nCachedSec = int.Parse(ConfigurationManager.AppSettings["ODBC_CACH_SEC"].ToString());
            }
            else if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                if (HttpContext.Current.Session["ODBC_CACH_SEC"] != null)
                    m_nCachedSec = int.Parse(HttpContext.Current.Session["ODBC_CACH_SEC"].ToString());
                else
                    m_nCachedSec = 60;
            }
            else
                m_nCachedSec = 60;

        }

        public override void Finish()
        {
            base.Finish();

            //if (m_myDataSet != null)
            //    m_myDataSet.Clear();

            m_myDataSet = null;

        }

        ~DataSetQuery() { }

        public virtual System.Data.DataTable Execute(string sVirtualTableName, bool bForceQuery)
        {
            return ExecuteQuery(m_sOraStr, sVirtualTableName, bForceQuery);
        }

        public override bool Execute()
        {
            System.Data.DataTable t = ExecuteQuery(m_sOraStr, "temp", true);
            if (t == null)
                return false;
            m_myDataSet.Tables["temp"].Clear();
            return true;
        }

        protected virtual void FillQueryString(string oraStr)
        {
            m_sOraStr = oraStr;
        }

        protected virtual System.Data.DataTable ExecuteQuery(string oraStr, string sVirtualTableName, bool bForceQuery)
        {
            if (bForceQuery)
            {
                if (m_myDataSet.Tables.Contains(sVirtualTableName))
                    m_myDataSet.Tables[sVirtualTableName].Clear();
            }
            else
            {
                if (m_myDataSet.Tables.Contains(sVirtualTableName))
                {
                    Clean();
                    return m_myDataSet.Tables[sVirtualTableName];
                }
            }
            FillQueryString(oraStr);
            string sCachStr = GetCachStr();
            System.Data.DataTable dCached = SelectCacher.GetCachedDataTable(sCachStr, m_nCachedSec);
            if (dCached == null)
            {
                int_Execute();
                oraStr = m_sOraStr;
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = command;

                try
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = sVirtualTableName, QueryType = KLogEnums.eDBQueryType.UPDATE })
                    {
                        DataTable dataTable = new DataTable(sVirtualTableName);
                        dataTable.BeginLoadData();
                        adapter.Fill(dataTable);
                        dataTable.EndLoadData();
                        m_myDataSet.EnforceConstraints = false;
                        m_myDataSet.Tables.Add(dataTable);
                    }
                }
                catch (Exception ex)
                {
                    string message = "While running : '" + m_sLastExecutedOraStr + "' Exception occurred.";
                    logger.Error(message, ex);
                    adapter = null;
                    Finish();
                    return null;
                }
                adapter = null;
                if (this.GetType() == System.Type.GetType("ODBCWrapper.DataSetInsertQuery"))
                {
                    m_myDataSet.Tables.Add(sVirtualTableName);
                }
                if (m_nCachedSec > 0)
                    ODBCWrapper.SelectCacher.SetCachedDataTable(sCachStr, m_myDataSet.Tables[sVirtualTableName]);
            }
            else
            {
                dCached.TableName = sVirtualTableName;
                m_myDataSet.Tables.Add(dCached);
            }
            return m_myDataSet.Tables[sVirtualTableName];

        }

        protected override void Clean()
        {
            m_sInsertStructure = "(";
            m_sInsertValues = "(";
            m_sOraStr = "";
        }

        public System.Data.DataTable Table(string sVirtualTableName)
        {
            return m_myDataSet.Tables[sVirtualTableName];
        }

        public void InsertTable(System.Data.DataTable theNewTable)
        {
            m_myDataSet.Tables.Add(theNewTable);
        }

        public void SetCachedSec(Int32 nCachedSec)
        {
            m_nCachedSec = nCachedSec;
        }


        protected System.Data.DataSet m_myDataSet;
        protected string m_sInsertStructure = "(";
        protected string m_sInsertValues = "(";
        protected Int32 m_nCachedSec;
    }
}
