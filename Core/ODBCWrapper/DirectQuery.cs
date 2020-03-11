using System;
using System.Data.SqlClient;
using System.Reflection;
using KLogMonitor;

namespace ODBCWrapper
{
    /// <summary>
    /// Summary description for DirectQuery.
    /// </summary>
    public class DirectQuery : Query
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public DirectQuery()
        {
            m_bIsWritable = true;
        }

        public override bool Execute()
        {
            return Execute(m_sOraStr.ToString());
        }

        public int ExecuteAffectedRows()
        {
            return ExecuteAffectedRows(m_sOraStr.ToString());
        }

        ~DirectQuery() { }

        public bool SetLockTimeOut(ref SqlConnection con)
        {
            if (m_nLockTimeOut == -1)
                return true;
            m_sErrorMsg = "";
            m_sOraStr = new System.Text.StringBuilder("SET LOCK_TIMEOUT ").Append(m_nLockTimeOut.ToString());
            int_Execute();

            try
            {
                if (con.State != System.Data.ConnectionState.Open)
                    con.Open();
                command.Connection = con;

                SqlQueryInfo queryInfo = Utils.GetSqlDataMonitor(command);
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (m_bIsWritable || Utils.UseWritable).ToString() })
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                string sMes = "While running : '" + m_sLastExecutedOraStr + "'\r\n Exception occurred: " + ex.Message;
                log.Error(sMes, ex);
                return false;
            }
            return true;
        }

        protected virtual bool Execute(string oraStr)
        {
            m_sErrorMsg = "";
            m_sOraStr = new System.Text.StringBuilder(oraStr);
            int_Execute();
            string sConn = ODBCWrapper.Connection.GetConnectionString(dbName, m_sConnectionKey, m_bIsWritable || Utils.UseWritable);
            if (sConn == "")
            {
                log.ErrorFormat("Empty connection string. could not run query. m_sOraStr: {0}", m_sOraStr != null ? m_sOraStr.ToString() : string.Empty);
                return false;
            }
            using (SqlConnection con = new SqlConnection(sConn))
            {
                oraStr = m_sOraStr.ToString();
                try
                {
                    con.Open();
                    SetLockTimeOut(con);
                    command.Connection = con;

                    SqlQueryInfo queryInfo = Utils.GetSqlDataMonitor(command);
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (m_bIsWritable || Utils.UseWritable).ToString() })
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    string sMes = "While running : '" + m_sLastExecutedOraStr + "'\r\n Exception occurred: " + ex.Message;
                    log.Error(sMes, ex);
                    return false;
                }
            }
            return true;
        }

        protected virtual int ExecuteAffectedRows(string oraStr)
        {
            int result = -1;
            m_sErrorMsg = "";
            m_sOraStr = new System.Text.StringBuilder(oraStr);
            int_Execute();
            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable || Utils.UseWritable);
            if (sConn == "")
            {
                log.ErrorFormat("Empty connection string. could not run query. m_sOraStr: {0}", m_sOraStr != null ? m_sOraStr.ToString() : string.Empty);
                return -1;
            }
            else
            {
                using (SqlConnection con = new SqlConnection(sConn))
                {
                    oraStr = m_sOraStr.ToString();
                    try
                    {
                        con.Open();
                        SetLockTimeOut(con);
                        command.Connection = con;

                        SqlQueryInfo queryInfo = Utils.GetSqlDataMonitor(command);
                        using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (m_bIsWritable || Utils.UseWritable).ToString() })
                        {
                            result = command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        string sMes = "While running : '" + m_sLastExecutedOraStr + "'\r\n Exception occurred: " + ex.Message;
                        log.Error(sMes, ex);
                        result = -1;
                    }
                }
            }
            return result;
        }

        public static DirectQuery operator +(DirectQuery p, object sOraStr)
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
    }
}
