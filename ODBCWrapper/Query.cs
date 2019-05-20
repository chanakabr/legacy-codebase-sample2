using System;
using System.Data.SqlClient;
using System.Collections;
using System.Configuration;
using KLogMonitor;
using System.Reflection;


namespace ODBCWrapper
{
    /// <summary>
    /// Summary description for Query.
    /// </summary>
    public abstract class Query
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected static Int32 m_nLongQueryTime = 0;
        private int m_nTimeout;
        protected string m_sErrorMsg;
        private Int32 m_nTop;
        protected Int32 m_nLockTimeOut;
        protected string m_sConnectionKey;
        protected string dbName;
        protected bool m_bIsWritable;
        static public Int32 GetSequence(string sSeqName)
        {
            Int32 nRet = -1;
            ODBCWrapper.DataSetSelectQuery selectQuery =
                new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ";
            selectQuery += sSeqName;
            selectQuery += ".nextval from dual";
            selectQuery.Execute("seq", true);
            if (selectQuery.Table("seq").DefaultView.Count > 0)
            {
                nRet = int.Parse(selectQuery.Table("seq").DefaultView[0].Row[0].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        protected void SetLockTimeOut(SqlConnection con)
        {
            //DirectQuery directQuery = new DirectQuery();
            //directQuery.SetLockTimeOut(ref con);
            //directQuery.Finish();
            //directQuery = null;
        }

        public string GetErrorMsg()
        {
            return m_sErrorMsg;
        }

        static public Int32 GetLockTimeOut()
        {
            //if (Utils.GetTcmConfigValue("CONNECTION_LOCK_TIMEOUT") != string.Empty)
            //    return int.Parse(Utils.GetTcmConfigValue("CONNECTION_LOCK_TIMEOUT"));
            return -1; //1000;
        }

        static public Int32 GetLongTimeQuery()
        {
            //if (Utils.GetTcmConfigValue("QUERY_LONG") != string.Empty)
            //    return int.Parse(Utils.GetTcmConfigValue("QUERY_LONG"));
            return 1000;
        }

        protected Query()
        {
            m_sErrorMsg = "";
            m_sConnectionKey = "";
            m_nLockTimeOut = GetLockTimeOut();
            m_hashTable = new object[255];
            m_nTimeout = 0;
            m_nTop = 0;
            if (m_nLongQueryTime == 0)
                m_nLongQueryTime = GetLongTimeQuery();
            m_sOraStr = new System.Text.StringBuilder();
        }

        public void SetTop(Int32 nTop)
        {
            m_nTop = nTop;
        }

        public void SetLockTimeOut(Int32 nTop)
        {
            m_nLockTimeOut = nTop;
        }

        public void SetWritable(bool bIsWritable)
        {
            m_bIsWritable = bIsWritable;
        }

        public void SetConnectionKey(string sKey, string databaseName = null)
        {
            m_sConnectionKey = sKey;
            if (!string.IsNullOrEmpty(databaseName))
                dbName = databaseName;
        }

        protected Query(ref Connection conn)
        {
            isOwnConnection = false;
        }

        //public static implicit operator Connection(Query m) 
        //{
        //return m.m_conn;
        //}

        public virtual void Finish()
        {
            //lock(m_crit_sec)
            //{
            m_hashTable = null;
            //m_conn.Finish();
            if (command != null)
                command = null;
            //}
        }

        protected string GetCachStr()
        {
            System.Text.StringBuilder sCachStr = new System.Text.StringBuilder();

            string sOraStr = m_sOraStr.ToString();
            string sToReplace = "select top " + m_nTop.ToString() + " ";
            string sDistinctToReplace = "selecttmp distinct top " + m_nTop.ToString() + " ";
            if (m_nTop != 0)
            {
                sOraStr = sOraStr.ToLower().Replace("select distinct", sDistinctToReplace);
                sOraStr = sOraStr.ToLower().Replace("select ", sToReplace);
                sOraStr = sOraStr.ToLower().Replace("selecttmp ", "select ");

            }
            sCachStr.Append(sOraStr).Append("(");
            if (table_ind > 0)
            {
                for (int i = 0; i < table_ind; i++)
                {
                    if (i > 0)
                        sCachStr.Append(",");
                    if (m_hashTable[i] != null)
                        sCachStr.Append(m_hashTable[i].ToString());
                }
            }
            sCachStr.AppendFormat(") {0}", string.IsNullOrEmpty(m_sConnectionKey) ? "CONNECTION_STRING" : m_sConnectionKey);
            return sCachStr.ToString();
        }

        protected bool int_Execute()
        {
            command = null;
            string sToReplace = "select top " + m_nTop.ToString() + " ";
            string sDistinctToReplace = "selecttmp distinct top " + m_nTop.ToString() + " ";
            if (m_nTop != 0)
            {
                string sTmp = m_sOraStr.ToString();
                sTmp = sTmp.ToLower().Replace("select distinct", sDistinctToReplace);
                sTmp = sTmp.ToLower().Replace("select ", sToReplace);
                sTmp = sTmp.ToLower().Replace("selecttmp ", "select ");
                sTmp = sTmp.ToLower().Replace("n'", "N'");
                m_sOraStr = new System.Text.StringBuilder(sTmp);
            }
            command = new SqlCommand(m_sOraStr.ToString());
            if (m_nTimeout != 0)
                command.CommandTimeout = m_nTimeout;
            if (table_ind > 0)
            {
                for (int i = 0; i < table_ind; i++)
                {
                    SqlParameter par = new SqlParameter("P" + i.ToString(), m_hashTable[i]);
                    command.Parameters.Add(par);
                    if (m_sLastParameters != "")
                        m_sLastParameters += ",";
                    if (m_hashTable[i] != null)
                        m_sLastParameters += m_hashTable[i].ToString();
                    m_hashTable[i] = null;
                }
                table_ind = 0;
            }
            //m_conn.GetConnection(ref command);
            m_sLastExecutedOraStr = m_sOraStr.ToString();
            m_sLastExecutedOraStr += "(" + m_sLastParameters + ")";
            Clean();
            return true;
        }

        public void SetTimeout(Int32 nTimeout)
        {
            m_nTimeout = nTimeout;
        }

        public virtual bool Execute()
        {
            return int_Execute();
        }

        protected virtual bool AddParameter(string parameterName, string type, object value)
        {
            m_sOraStr.Append(" ").Append(parameterName);
            m_sOraStr.Append(type);
            m_sOraStr.Append("@P").Append(table_ind.ToString());

            if (value == null)
                value = DBNull.Value;

            m_hashTable[table_ind] = value;
            table_ind++;

            Utils.CheckDBReadWrite(parameterName, value, "Query", m_bIsWritable, ref Utils.UseWritable);
            return true;
        }

        public static Query operator +(Query p, object sOraStr)
        {
            if (sOraStr.GetType() == System.Type.GetType("ODBCWrapper.Parameter"))
            {
                p.AddParameter(((Parameter)sOraStr).m_sParName,
                    ((Parameter)sOraStr).m_sType,
                    ((Parameter)sOraStr).m_sParVal);
                sOraStr = null;
            }
            else
                p.m_sOraStr.Append(" ").Append(sOraStr);

            return p;
        }

        protected virtual void Clean()
        {
            m_sOraStr = new System.Text.StringBuilder();
        }

        ~Query()
        {
            m_crit_sec = null;
            m_hashTable = null;
        }

        //protected ODBCWrapper.Connection m_conn = null;
        protected bool isOwnConnection = true;

        protected SqlCommand command;
        //protected ODBCWrapper.Connection m_conn;
        protected object m_crit_sec = new object();
        protected System.Text.StringBuilder m_sOraStr;
        protected string m_sLastExecutedOraStr = "";
        protected string m_sLastParameters = "";
        protected object[] m_hashTable = null;
        protected Int32 table_ind = 0;
    }
}
