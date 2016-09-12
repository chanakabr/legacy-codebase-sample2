using KLogMonitor;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace ODBCWrapper
{
    /// <summary>
    /// Summary description for InsertQuery.
    /// </summary>
    public class InsertQuery : DirectQuery
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public InsertQuery()
            : base()
        {
        }

        public InsertQuery(string sTableName)
        {
            SetTable(sTableName);
            m_bIsWritable = true;
        }

        ~InsertQuery() { }

        public void SetTable(string sTableName)
        {
            m_sOraStr = new System.Text.StringBuilder("insert into ").Append(sTableName).Append(" ");
            sInsertStructure = "(";
            sInsertValues = "(";
        }

        /// <summary>
        /// Inserts bulk of rows to a specific table(which its name represent by sTableName param) at the db
        /// at one time, the bulk of rows is taken from dtData param.
        /// the dtData datatable structure must match the structure of the destination table at the db.
        /// </summary>
        /// <param name="sTableName"></param>
        /// <param name="dtData"></param>
        public void InsertBulk(string sTableName, DataTable dtData, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default)
        {
            if (dtData != null && dtData.Rows.Count > 0)
            {
                int numRows = dtData.Rows.Count;
                string connString = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable || Utils.UseWritable);

                SqlBulkCopy bulkCopy = new SqlBulkCopy(connString, sqlBulkCopyOptions)
                {
                    DestinationTableName = sTableName,
                    BatchSize = numRows,
                    BulkCopyTimeout = 360
                };

                foreach (DataColumn col in dtData.Columns)
                {
                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                }
                bulkCopy.WriteToServer(dtData);
            }
        }

        protected override bool AddParameter(string parameterName, string type, object value)
        {
            if (sInsertStructure != "(")
                sInsertStructure += ",";
            sInsertStructure += parameterName;

            if (sInsertValues != "(")
                sInsertValues += ",";
            sInsertValues += "@P" + table_ind.ToString();

            if (value == null)
                value = DBNull.Value;

            m_hashTable[table_ind] = value;
            table_ind++;

            Utils.CheckDBReadWrite(parameterName, value, "InsertQuery", m_bIsWritable, ref Utils.UseWritable);

            return true;
        }

        protected override bool Execute(string oraStr)
        {
            m_sOraStr = new System.Text.StringBuilder(oraStr);
            m_sOraStr.Append(sInsertStructure);
            m_sOraStr.Append(") ");
            m_sOraStr.Append("VALUES ");
            m_sOraStr.Append(sInsertValues);
            m_sOraStr.Append(")");
            oraStr = m_sOraStr.ToString();
            int_Execute();
            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable || Utils.UseWritable);
            if (sConn == "")
            {
                log.ErrorFormat("Empty connection string. could not run query. m_sOraStr: {0}", m_sOraStr != null ? m_sOraStr.ToString() : string.Empty);
                return false;
            }
            using (SqlConnection con = new SqlConnection(sConn))
            {
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

        public int ExecuteAndGetId()
        {
            string oraStr = m_sOraStr.ToString();

            int id = -1;
            m_sOraStr = new System.Text.StringBuilder(oraStr);
            m_sOraStr.Append(sInsertStructure);
            m_sOraStr.Append(") ");
            // To get the ID of the inserted row
            m_sOraStr.Append("output INSERTED.ID ");
            m_sOraStr.Append("VALUES ");
            m_sOraStr.Append(sInsertValues);
            m_sOraStr.Append(")");            
            oraStr = m_sOraStr.ToString();
            int_Execute();
            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable || Utils.UseWritable);
            if (sConn == "")
            {
                log.ErrorFormat("Empty connection string. could not run query. m_sOraStr: {0}", m_sOraStr != null ? m_sOraStr.ToString() : string.Empty);
                return id;
            }
            using (SqlConnection con = new SqlConnection(sConn))
            {
                try
                {
                    con.Open();
                    SetLockTimeOut(con);
                    command.Connection = con;

                    SqlQueryInfo queryInfo = Utils.GetSqlDataMonitor(command);

                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (m_bIsWritable || Utils.UseWritable).ToString() })
                    {
                        id = (int)command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    string sMes = "While running : '" + m_sLastExecutedOraStr + "'\r\n Exception occurred: " + ex.Message;
                    log.Error(sMes, ex);
                    return -1;
                }
            }

            return id;
        }
        public static InsertQuery operator +(InsertQuery p, object sOraStr)
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

        protected string sInsertStructure = "(";
        protected string sInsertValues = "(";
    }
}
