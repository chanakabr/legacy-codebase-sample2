using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Collections.Specialized;
using Microsoft.SqlServer.Server;
using KLogMonitor;
using System.Reflection;

namespace ODBCWrapper
{
    public class StoredProcedure
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public StoredProcedure(string sProcedureName)
        {
            procedureNameWithDbVersionPrefix = string.Format("{0}{1}", Utils.dBVersionPrefix, sProcedureName);
            procedureName = sProcedureName;
            m_Parameters = new Dictionary<string, object>();
            m_bIsWritable = (sProcedureName.ToLower().StartsWith("get") && !sProcedureName.ToLower().StartsWith("getorinsert")) ? false : true;
        }

        public void SetWritable(bool bIsWritable)
        {
            m_bIsWritable = bIsWritable;
        }

        ~StoredProcedure() { }

        private string procedureNameWithDbVersionPrefix;
        private string procedureName;
        private Dictionary<string, object> m_Parameters;
        private int m_nTimeout;

        protected string m_sConnectionKey;
        protected bool m_bIsWritable;

        private int LongQueryTime
        {
            get
            {
                //if (Utils.GetTcmConfigValue("QUERY_LONG") != string.Empty)
                //    return int.Parse(Utils.GetTcmConfigValue("QUERY_LONG"));
                return 2000;
            }
        }

        public void AddParameter(string key, object value)
        {
            if (value == null)
            {
                value = DBNull.Value;
            }

            m_Parameters.Add(key, value);

            Utils.CheckDBReadWrite(key, value, procedureName, m_bIsWritable, ref Utils.UseWritable);
        }

        /// <summary>
        /// Pass null value to parameter of stored procedure in a way that it will not considered as optional parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sKey"></param>
        /// <param name="oValue"></param>
        public void AddNullableParameter<T>(string sKey, T oValue)
        {
            if (oValue != null)
            {
                m_Parameters.Add(sKey, oValue);
            }
            else
            {
                m_Parameters.Add(sKey, DBNull.Value);
            }

            Utils.CheckDBReadWrite(sKey, oValue, procedureName, m_bIsWritable, ref Utils.UseWritable);
        }

        public void AddXMLParameter<T>(string sKey, List<T> oListValue, string colName)
        {
            m_Parameters.Add(sKey, CreateXML<T>(oListValue, colName));

            Utils.CheckDBReadWrite(sKey, oListValue, procedureName, m_bIsWritable, ref Utils.UseWritable);
        }


        public void AddIDListParameter<T>(string sKey, List<T> oListValue, string colName)
        {
            m_Parameters.Add(sKey, CreateDataTable<T>(oListValue, colName));

            Utils.CheckDBReadWrite(sKey, oListValue, procedureName, m_bIsWritable, ref Utils.UseWritable);
        }

        public void AddKeyValueListParameter<T1, T2>(string sKey, Dictionary<T1, List<T2>> oListKeyValue, string colNameKey, string colNameValue)
        {
            m_Parameters.Add(sKey, CreateDataTable<T1, T2>(oListKeyValue, colNameKey, colNameValue));
        }

        public void AddKeyValueListParameter<T1, T2>(string sKey, List<KeyValuePair<T1, T2>> oListKeyValue, string colNameKey, string colNameValue)
        {
            m_Parameters.Add(sKey, CreateDataTable<T1, T2>(oListKeyValue, colNameKey, colNameValue));
        }

        public void AddDataTableParameter<T1, T2>(string sKey, DataTable oDictValue, string colNameKey, string colNameValue)
        {
            m_Parameters.Add(sKey, CreateDataTable<T1, T2>(oDictValue, colNameKey, colNameValue));
        }

        private object CreateDataTable<T1, T2>(List<KeyValuePair<T1, T2>> oListValue, string colNameKey, string colNameValue)
        {
            DataTable table = new DataTable();
            table.Columns.Add(colNameKey, typeof(T1));
            table.Columns.Add(colNameValue, typeof(T2));


            foreach (KeyValuePair<T1, T2> obj in oListValue)
            {
                DataRow dr = table.NewRow();
                dr[colNameKey] = obj.Key;
                dr[colNameValue] = obj.Value;
                table.Rows.Add(dr);
            }
            return table;
        }

        private object CreateDataTable(IEnumerable<int> ids, string colName)
        {
            DataTable table = new DataTable();
            table.Columns.Add(colName, typeof(Int32));
            foreach (Int32 id in ids)
            {
                table.Rows.Add(id);

                Utils.CheckDBReadWrite(colName, id, procedureName, m_bIsWritable, ref Utils.UseWritable);
            }
            return table;
        }

        private object CreateDataTable<T1, T2>(Dictionary<T1, List<T2>> oDictValue, string colNameKey, string colNameValue)
        {
            DataTable table = new DataTable();
            table.Columns.Add(colNameKey, typeof(T1));
            table.Columns.Add(colNameValue, typeof(T2));

            foreach (T1 oKey in oDictValue.Keys)
            {
                if (oDictValue[oKey] != null)
                {
                    foreach (T2 oValue in oDictValue[oKey])
                    {
                        DataRow dr = table.NewRow();
                        dr[colNameKey] = oKey;
                        dr[colNameValue] = oValue;
                        table.Rows.Add(dr);
                    }
                }
                else
                {
                    DataRow dr = table.NewRow();
                    dr[colNameKey] = oKey;
                    dr[colNameValue] = DBNull.Value;
                    table.Rows.Add(dr);
                }
            }



            return table;
        }

        private object CreateDataTable<T1, T2>(DataTable oDictValue, string colNameKey, string colNameValue)
        {
            DataTable table = new DataTable();
            table.Columns.Add(colNameKey, typeof(T1));
            table.Columns.Add(colNameValue, typeof(T2));

            foreach (DataRow oRow in oDictValue.Rows)
            {
                DataRow dr = table.NewRow();
                dr[colNameKey] = oRow[colNameKey];
                dr[colNameValue] = oRow[colNameValue];
                table.Rows.Add(dr);

            }
            return table;
        }
        private object CreateDataTable<T>(IEnumerable<T> ids, string colName)
        {
            DataTable table = new DataTable();
            table.Columns.Add(colName, typeof(T));
            if (ids != null)
            {
                foreach (T id in ids)
                {
                    table.Rows.Add(id);
                }
            }
            return table;
        }

        private string CreateXML<T>(IEnumerable<T> ids, string colName)
        {
            try
            {
                string sXml = "<ROOT>";
                string sNode = "<row " + colName + "=\"X\"/>";
                foreach (T id in ids)
                {
                    sXml = string.Format("{0}{1}", sXml, sNode.Replace("X", id.ToString()));
                }

                sXml = string.Format("{0}{1}", sXml, "</ROOT>");
                return sXml;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return string.Empty;
            }
        }

        public void SetConnectionKey(string sKey)
        {
            m_sConnectionKey = sKey;
        }

        protected void SetLockTimeOut(SqlConnection con)
        {
            DirectQuery directQuery = new DirectQuery();
            directQuery.SetLockTimeOut(ref con);
            directQuery.Finish();
            directQuery = null;
        }

        public void SetTimeout(Int32 nTimeout)
        {
            m_nTimeout = nTimeout;
        }

        public DataTable Execute()
        {
            DataTable result = null;

            SqlCommand command = new SqlCommand();

            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = procedureNameWithDbVersionPrefix;

            if (m_nTimeout != 0)
                command.CommandTimeout = m_nTimeout;

            foreach (string item in m_Parameters.Keys)
            {
                command.Parameters.Add(new SqlParameter(item, m_Parameters[item]));
            }
            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable || Utils.UseWritable);
            if (sConn == "")
            {
                log.ErrorFormat("Empty connection string. could not run query. m_sProcedureName: {0}", procedureNameWithDbVersionPrefix != null ? procedureNameWithDbVersionPrefix.ToString() : string.Empty);
                return null;
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
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                result = new DataTable();
                                result.Load(reader);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string sMes = "While running : '" + procedureNameWithDbVersionPrefix + "'\r\n Exception occurred: " + ex.Message;
                    log.Error(sMes, ex);
                    return null;
                }
            }
            return result;
        }

        public DataSet ExecuteDataSet()
        {
            DataSet result = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();

            SqlCommand command = new SqlCommand();

            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = procedureNameWithDbVersionPrefix;

            foreach (string item in m_Parameters.Keys)
            {
                command.Parameters.Add(new SqlParameter(item, m_Parameters[item]));
            }
            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable || Utils.UseWritable);
            if (sConn == "")
            {
                log.ErrorFormat("Empty connection string. could not run query. m_sProcedureName: {0}", procedureNameWithDbVersionPrefix != null ? procedureNameWithDbVersionPrefix.ToString() : string.Empty);
                return null;
            }
            using (SqlConnection con = new SqlConnection(sConn))
            {
                try
                {
                    SqlQueryInfo queryInfo = Utils.GetSqlDataMonitor(command);
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (m_bIsWritable || Utils.UseWritable).ToString() })
                    {
                        con.Open();
                        SetLockTimeOut(con);
                        command.Connection = con;
                        da.SelectCommand = command;
                        da.Fill(result);
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    con.Close();
                    string sMes = "While running : '" + procedureNameWithDbVersionPrefix + "'\r\n Exception occurred: " + ex.Message;
                    log.Error(sMes, ex);
                    return null;
                }
            }
            return result;
        }

        /// <summary>
        /// Execute stored procedure that return value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ExecuteReturnValue<T>()
        {
            T result = default(T);

            SqlCommand command = new SqlCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = procedureNameWithDbVersionPrefix;

            foreach (string item in m_Parameters.Keys)
            {
                command.Parameters.Add(new SqlParameter(item, m_Parameters[item]));
            }

            SqlParameter sqlReturnedValueParam = new SqlParameter();
            sqlReturnedValueParam.Direction = ParameterDirection.ReturnValue;
            command.Parameters.Add(sqlReturnedValueParam);

            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable || Utils.UseWritable);
            if (sConn == "")
            {
                log.ErrorFormat("Empty connection string. could not run query. m_sProcedureName: {0}", procedureNameWithDbVersionPrefix != null ? procedureNameWithDbVersionPrefix.ToString() : string.Empty);
                return result;
            }
            using (SqlConnection con = new SqlConnection(sConn))
            {
                try
                {
                    SqlQueryInfo queryInfo = Utils.GetSqlDataMonitor(command);
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (m_bIsWritable || Utils.UseWritable).ToString() })
                    {
                        con.Open();
                        SetLockTimeOut(con);
                        command.Connection = con;
                        object dbRes = command.ExecuteScalar();
                        result = (T)Convert.ChangeType(sqlReturnedValueParam.Value, typeof(T));
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    con.Close();
                    string sMes = "While running : '" + procedureNameWithDbVersionPrefix + "'\r\n Exception occurred: " + ex.Message;
                    log.Error(sMes, ex);
                    return result;
                }
            }
            return result;
        }

        /// <summary>
        /// Execute stored procedure that return value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public object ExecuteReturnValue()
        {
            object result = null;

            SqlCommand command = new SqlCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = procedureNameWithDbVersionPrefix;

            foreach (string item in m_Parameters.Keys)
            {
                command.Parameters.Add(new SqlParameter(item, m_Parameters[item]));
            }

            SqlParameter sqlReturnedValueParam = new SqlParameter();
            sqlReturnedValueParam.Direction = ParameterDirection.ReturnValue;
            command.Parameters.Add(sqlReturnedValueParam);

            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable || Utils.UseWritable);
            if (sConn == "")
            {
                log.ErrorFormat("Empty connection string. could not run query. m_sProcedureName: {0}", procedureNameWithDbVersionPrefix != null ? procedureNameWithDbVersionPrefix.ToString() : string.Empty);
                return result;
            }
            using (SqlConnection con = new SqlConnection(sConn))
            {
                try
                {
                    SqlQueryInfo queryInfo = Utils.GetSqlDataMonitor(command);
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (m_bIsWritable || Utils.UseWritable).ToString() })
                    {
                        con.Open();
                        SetLockTimeOut(con);
                        command.Connection = con;
                        result = command.ExecuteScalar();
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    con.Close();
                    string sMes = "While running : '" + procedureNameWithDbVersionPrefix + "'\r\n Exception occurred: " + ex.Message;
                    log.Error(sMes, ex);
                    return result;
                }
            }

            return result;
        }


        /// <summary>
        /// Execute stored prcoedure that execute some sql without return value or resultset.
        /// </summary>
        public void ExecuteNonQuery()
        {
            SqlCommand command = new SqlCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = procedureNameWithDbVersionPrefix;

            foreach (string item in m_Parameters.Keys)
            {
                command.Parameters.Add(new SqlParameter(item, m_Parameters[item]));
            }

            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable || Utils.UseWritable);
            if (sConn == "")
            {
                log.ErrorFormat("Empty connection string. could not run query. m_sProcedureName: {0}", procedureNameWithDbVersionPrefix != null ? procedureNameWithDbVersionPrefix.ToString() : string.Empty);
                return;
            }
            using (SqlConnection con = new SqlConnection(sConn))
            {
                try
                {
                    SqlQueryInfo queryInfo = Utils.GetSqlDataMonitor(command);
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (m_bIsWritable || Utils.UseWritable).ToString() })
                    {
                        con.Open();
                        SetLockTimeOut(con);
                        command.Connection = con;
                        command.ExecuteNonQuery();
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    con.Close();
                    string sMes = "While running : '" + procedureNameWithDbVersionPrefix + "'\r\n Exception occurred: " + ex.Message;
                    log.Error(sMes, ex);
                }
            }
        }


        public DataSet ExecuteDataSetWithListParam()
        {
            DataSet result = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();

            SqlCommand command = new SqlCommand();

            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = procedureNameWithDbVersionPrefix;

            foreach (string item in m_Parameters.Keys)
            {
                if (m_Parameters[item] != null)
                {
                    Type type = m_Parameters[item].GetType();
                    if (type.FullName == typeof(System.String).FullName || type.FullName == typeof(System.Boolean).FullName || type.FullName == typeof(System.Int32).FullName
                        || type.FullName == typeof(System.DateTime).FullName)
                    {
                        command.Parameters.Add(new SqlParameter(item, m_Parameters[item]));
                    }
                    else
                    {
                        List<int> ids = (List<int>)m_Parameters[item];
                        SqlParameter UrlParam = command.Parameters.AddWithValue(item, CreateDataTable(ids, "Id"));
                        UrlParam.SqlDbType = SqlDbType.Structured;
                        UrlParam.TypeName = "dbo.IDList";
                    }
                }
                else
                {
                    command.Parameters.Add(new SqlParameter(item, m_Parameters[item]));
                }
            }
            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable || Utils.UseWritable);
            if (sConn == "")
            {
                log.ErrorFormat("Empty connection string. could not run query. m_sProcedureName: {0}", procedureNameWithDbVersionPrefix != null ? procedureNameWithDbVersionPrefix.ToString() : string.Empty);
                return result;
            }

            using (SqlConnection con = new SqlConnection(sConn))
            {
                try
                {
                    SqlQueryInfo queryInfo = Utils.GetSqlDataMonitor(command);
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (m_bIsWritable || Utils.UseWritable).ToString() })
                    {
                        con.Open();
                        SetLockTimeOut(con);
                        command.Connection = con;
                        da.SelectCommand = command;
                        DataTable dataTable = new DataTable();
                        dataTable.BeginLoadData();
                        da.Fill(dataTable);
                        dataTable.EndLoadData();
                        result.EnforceConstraints = false;
                        result.Tables.Add(dataTable);
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    con.Close();
                    string sMes = "While running : '" + procedureNameWithDbVersionPrefix + "'\r\n Exception occurred: " + ex.Message;
                    log.Error(sMes, ex);
                    return null;
                }
            }
            return result;
        }

        public void AddDataTableParameter(string sKey, DataTable dt)
        {
            m_Parameters.Add(sKey, dt);
        }
    }
}
