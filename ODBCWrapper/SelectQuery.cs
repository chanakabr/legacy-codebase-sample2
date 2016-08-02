using System;
using System.Data.SqlClient;
using System.Reflection;
using KLogMonitor;

namespace ODBCWrapper
{
    /// <summary>
    /// Summary description for SelectQuery.
    /// </summary>
    public class SelectQuery : Query
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public SelectQuery()
        {
            m_myReader = null;
            command = null;
            m_bIsWritable = false;
        }

        ~SelectQuery() { }

        public override void Finish()
        {
            base.Finish();
            if (m_myReader != null)
                m_myReader.Close();

            m_myReader = null;

        }

        public SelectQuery(ref Connection conn)
            : base(ref conn)
        {
            m_myReader = null;
            command = null;
        }

        public bool NextRow()
        {
            //lock(m_crit_sec)
            //{
            if (m_myReader != null)
                return m_myReader.Read();
            else
                return false;
            //}
        }

        public string GetValue(string sName)
        {
            //lock(m_crit_sec)
            //{
            if (m_myReader != null)
            {
                for (int i = 0; i < m_myReader.FieldCount; i++)
                {
                    if (m_myReader.GetName(i).ToString().ToUpper() == sName.ToUpper())
                        return m_myReader.GetValue(i).ToString();
                }
                return "";
            }
            else
            {
                return "";
            }
            //}
        }

        public object GetObjValue(string sName)
        {
            //lock(m_crit_sec)
            //{
            if (m_myReader != null)
            {
                for (int i = 0; i < m_myReader.FieldCount; i++)
                {
                    if (m_myReader.GetName(i).ToString().ToUpper() == sName.ToUpper())
                        return m_myReader.GetValue(i);
                }
                return null;
            }
            else
            {
                return null;
            }
            //}
        }

        public bool GetValue(Int32 ind,
            ref string sName,
            ref object sValue)
        {
            //lock(m_crit_sec)
            //{
            if (m_myReader != null)
            {
                sName = m_myReader.GetName(ind).ToString().ToUpper();
                sValue = m_myReader.GetValue(ind);
                return true;
            }
            else
                return false;
            //}
        }

        public Int32 GetFieldCount()
        {
            //lock(m_crit_sec)
            //{
            if (m_myReader != null)
            {
                return m_myReader.FieldCount;
            }
            else
                return 0;
            //}
        }
        public override bool Execute()
        {
            return Execute(m_sOraStr.ToString());
        }

        private bool Execute(string oraStr)
        {
            bool bRet = true;
            m_sOraStr = new System.Text.StringBuilder(oraStr);
            int_Execute();
            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable || Utils.UseWritable);
            if (sConn == "")
            {
                log.ErrorFormat("Empty connection string. could not run query. m_sOraStr: {0}", m_sOraStr != null ? m_sOraStr.ToString() : string.Empty);
                bRet = false;
            }

            using (SqlConnection con = new SqlConnection(sConn))
            {
                try
                {
                    con.Open();
                    SetLockTimeOut(con);
                    command.Connection = con;

                    SqlQueryInfo queryInfo = Utils.GetSqlDataMonitor(command);
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = m_bIsWritable.ToString() })
                    {
                        m_myReader = command.ExecuteReader();
                    }
                }
                catch (Exception ex)
                {
                    string sMes = "While running : '" + m_sLastExecutedOraStr + "'\r\n Exception occurred: " + ex.Message;
                    log.Error(sMes, ex);
                    bRet = false;
                }
            }
            return bRet;
        }

        public static SelectQuery operator +(SelectQuery p, object sOraStr)
        {
            if (sOraStr.GetType() == System.Type.GetType("ODBCWrapper.Parameter"))
            {
                p.AddParameter(((Parameter)sOraStr).m_sParName,
                    ((Parameter)sOraStr).m_sType,
                    ((Parameter)sOraStr).m_sParVal);
            }
            else
                p.m_sOraStr.Append(" ").Append(sOraStr);

            return p;
        }

        public static implicit operator SqlDataReader(SelectQuery m)
        {
            return m.m_myReader;
        }

        protected SqlDataReader m_myReader;
    }
}
