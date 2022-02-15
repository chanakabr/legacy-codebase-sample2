using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Web;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using System.Linq;

namespace ODBCWrapper
{
    public class StoredProcedure : IRoutable
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected string dbName;

        public bool ShouldForcePrimary
        {
            get;
            set;
        }
        public bool ShouldForceSecondary
        {
            get;
            set;
        }

        public StoredProcedure(string sProcedureName, bool isVersionless = false)
        {
            if (!isVersionless)
            {
                procedureNameWithDbVersionPrefix = string.Format("{0}{1}", Utils.dBVersionPrefix, sProcedureName);
            }
            else
            {
                procedureNameWithDbVersionPrefix = sProcedureName;
            }

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

        /// <summary>
        /// by default - false. all stored procedures have versions.
        /// However there is a limited amount of unique cases where the stored procedure is version less, 
        /// Database data wise
        /// </summary>
        public bool isVersionless = false;
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
            AddParameter(key, value, false);
        }

        public void AddParameter(string key, object value, bool isUnicode)
        {
            if (value == null)
            {
                value = DBNull.Value;
            }

            if (isUnicode)
            {
                m_Parameters.Add(key, Tuple.Create(isUnicode, value));
            }
            else
            {
                m_Parameters.Add(key, value);
            }
            
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


        public void AddIDListParameter<T>(string sKey, ICollection<T> oListValue, string colName)
        {
            m_Parameters.Add(sKey, CreateDataTable<T>(oListValue, colName));

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

            if (oListValue != null)
            {
                foreach (KeyValuePair<T1, T2> obj in oListValue)
                {
                    DataRow dr = table.NewRow();
                    dr[colNameKey] = obj.Key;
                    dr[colNameValue] = obj.Value;
                    table.Rows.Add(dr);
                }
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
                StringBuilder sXml = new StringBuilder("<ROOT>");
                string sNode = "<row {0}=\"{1}\"/>";
                foreach (T id in ids)
                {
                    sXml.AppendFormat(sNode, colName, id.ToString());
                }

                sXml.Append("</ROOT>");
                return sXml.ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public void SetConnectionKey(string sKey, string databaseName = null)
        {
            m_sConnectionKey = sKey;
            if (!string.IsNullOrEmpty(databaseName))
                dbName = databaseName;
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

        public DataTable Execute(bool shouldGoToSlave = false)
        {
            DataTable result = null;

            SqlCommand command = new SqlCommand();

            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = procedureNameWithDbVersionPrefix;

            if (m_nTimeout != 0)
                command.CommandTimeout = m_nTimeout;

            foreach (string item in m_Parameters.Keys)
            {
                AddParameter(command, item);
            }
            string sConn = string.Empty;

            bool isRoutedToPrimary = m_bIsWritable || Utils.UseWritable;
            if (shouldGoToSlave)
            {
                sConn = ODBCWrapper.Connection.GetConnectionString(dbName, m_sConnectionKey, false, this);
                isRoutedToPrimary = false;
            }
            else
            {
                sConn = ODBCWrapper.Connection.GetConnectionString(dbName, m_sConnectionKey, isRoutedToPrimary, this);
            }

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
                    using (KMonitor km = new KMonitor(Phx.Lib.Log.Events.eEvent.EVENT_DATABASE, null, null, null, null) { 
                        Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (isRoutedToPrimary).ToString() })
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            result = new DataTable();
                            if (reader.HasRows)
                            {
                                result.Load(reader);
                            }
                        }
                    }

                    con.Close();
                }
                catch (Exception ex)
                {
                    string sMes = "While running : '" + procedureNameWithDbVersionPrefix + "'\r\n Exception occurred: " + ex.Message;
                    log.ErrorFormat(sMes, ex);
                    if (HttpContext.Current != null && HttpContext.Current.Items != null)
                    {
                        HttpContext.Current.Items[Utils.DATABASE_ERROR_DURING_SESSION] = true;
                    }
                    return null;
                }
            }
            return result;
        }

        public DataSet ExecuteDataSet(bool shouldGoToSlave = false)
        {
            DataSet result = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();

            SqlCommand command = new SqlCommand();

            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = procedureNameWithDbVersionPrefix;

            if (m_nTimeout != 0)
                command.CommandTimeout = m_nTimeout;

            foreach (string item in m_Parameters.Keys)
            {
                AddParameter(command, item);
            }
            string sConn = string.Empty;
            if (shouldGoToSlave)
            {
                sConn = ODBCWrapper.Connection.GetConnectionString(dbName, m_sConnectionKey, false, this);
            }
            else
            {
                sConn = ODBCWrapper.Connection.GetConnectionString(dbName, m_sConnectionKey, m_bIsWritable || Utils.UseWritable, this);
            }
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
                    using (KMonitor km = new KMonitor(Phx.Lib.Log.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (m_bIsWritable || Utils.UseWritable).ToString() })
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
                    if (HttpContext.Current != null && HttpContext.Current.Items != null)
                    {
                        HttpContext.Current.Items[Utils.DATABASE_ERROR_DURING_SESSION] = true;
                    }
                    return null;
                }
            }
            return result;
        }

        private void AddParameter(SqlCommand command, string item)
        {
            if (m_Parameters[item] is Tuple<bool, object>)
            {
                var t = (Tuple<bool, object>)m_Parameters[item];
                if (t.Item1)
                {
                    var _param = new SqlParameter(item, SqlDbType.NVarChar);
                    _param.Value = t.Item2;
                    command.Parameters.Add(_param);
                }
                else
                {
                    command.Parameters.Add(new SqlParameter(item, t.Item2));
                }
            }
            else
            {
                command.Parameters.Add(new SqlParameter(item, m_Parameters[item]));
            }
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

            if (m_nTimeout != 0)
                command.CommandTimeout = m_nTimeout;

            foreach (string item in m_Parameters.Keys)
            {
                AddParameter(command, item);
            }

            SqlParameter sqlReturnedValueParam = new SqlParameter();
            sqlReturnedValueParam.Direction = ParameterDirection.ReturnValue;
            command.Parameters.Add(sqlReturnedValueParam);

            string sConn = ODBCWrapper.Connection.GetConnectionString(dbName, m_sConnectionKey, m_bIsWritable || Utils.UseWritable, this);
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
                    using (KMonitor km = new KMonitor(Phx.Lib.Log.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (m_bIsWritable || Utils.UseWritable).ToString() })
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
                    if (HttpContext.Current != null && HttpContext.Current.Items != null)
                    {
                        HttpContext.Current.Items[Utils.DATABASE_ERROR_DURING_SESSION] = true;
                    }
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

            if (m_nTimeout != 0)
                command.CommandTimeout = m_nTimeout;

            foreach (string item in m_Parameters.Keys)
            {
                AddParameter(command, item);
            }

            SqlParameter sqlReturnedValueParam = new SqlParameter();
            sqlReturnedValueParam.Direction = ParameterDirection.ReturnValue;
            command.Parameters.Add(sqlReturnedValueParam);

            string sConn = ODBCWrapper.Connection.GetConnectionString(dbName, m_sConnectionKey, m_bIsWritable || Utils.UseWritable, this);
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
                    using (KMonitor km = new KMonitor(Phx.Lib.Log.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (m_bIsWritable || Utils.UseWritable).ToString() })
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
                    if (HttpContext.Current != null && HttpContext.Current.Items != null)
                    {
                        HttpContext.Current.Items[Utils.DATABASE_ERROR_DURING_SESSION] = true;
                    }
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

            if (m_nTimeout != 0)
                command.CommandTimeout = m_nTimeout;

            foreach (string item in m_Parameters.Keys)
            {
                AddParameter(command, item);
            }

            string sConn = ODBCWrapper.Connection.GetConnectionString(dbName, m_sConnectionKey, m_bIsWritable || Utils.UseWritable, this);
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
                    using (KMonitor km = new KMonitor(Phx.Lib.Log.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (m_bIsWritable || Utils.UseWritable).ToString() })
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

            if (m_nTimeout != 0)
                command.CommandTimeout = m_nTimeout;

            foreach (string item in m_Parameters.Keys)
            {
                if (m_Parameters[item] != null)
                {
                    Type type = m_Parameters[item].GetType();
                    if (type.FullName == typeof(System.String).FullName || type.FullName == typeof(System.Boolean).FullName || type.FullName == typeof(System.Int32).FullName
                        || type.FullName == typeof(System.DateTime).FullName)
                    {
                        AddParameter(command, item);
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
            string sConn = ODBCWrapper.Connection.GetConnectionString(dbName, m_sConnectionKey, m_bIsWritable || Utils.UseWritable, this);
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
                    using (KMonitor km = new KMonitor(Phx.Lib.Log.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (m_bIsWritable || Utils.UseWritable).ToString() })
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
                    if (HttpContext.Current != null && HttpContext.Current.Items != null)
                    {
                        HttpContext.Current.Items[Utils.DATABASE_ERROR_DURING_SESSION] = true;
                    }
                    return null;
                }
            }
            return result;
        }

        public void AddDataTableParameter(string sKey, DataTable dt)
        {
            m_Parameters.Add(sKey, dt);
        }

        /// <summary>
        /// Execute stored procedure that return value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool ExecuteReturnValue<T>(out T result)
        {
            result = default(T);

            SqlCommand command = new SqlCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = procedureNameWithDbVersionPrefix;

            if (m_nTimeout != 0)
                command.CommandTimeout = m_nTimeout;

            foreach (string item in m_Parameters.Keys)
            {
                AddParameter(command, item);
            }

            SqlParameter sqlReturnedValueParam = new SqlParameter();
            sqlReturnedValueParam.Direction = ParameterDirection.ReturnValue;
            command.Parameters.Add(sqlReturnedValueParam);

            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable || Utils.UseWritable, this);
            if (sConn == "")
            {
                log.ErrorFormat("Empty connection string. could not run query. m_sProcedureName: {0}", procedureNameWithDbVersionPrefix != null ? procedureNameWithDbVersionPrefix.ToString() : string.Empty);
                return false;
            }
            using (SqlConnection con = new SqlConnection(sConn))
            {
                try
                {
                    SqlQueryInfo queryInfo = Utils.GetSqlDataMonitor(command);
                    using (KMonitor km = new KMonitor(Phx.Lib.Log.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (m_bIsWritable || Utils.UseWritable).ToString() })
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
                    if (HttpContext.Current != null && HttpContext.Current.Items != null)
                    {
                        HttpContext.Current.Items[Utils.DATABASE_ERROR_DURING_SESSION] = true;
                    }
                    return false;
                }
            }
            return true;
        }

        public void AddOrderKeyValueListParameter<T1, T2>(string sKey, List<KeyValuePair<T1, T2>> oListValue, string colNameKey, string colNameValue)
        {
            m_Parameters.Add(sKey, CreateOrderedDataTable<T1, T2>(oListValue, colNameKey, colNameValue));

            Utils.CheckDBReadWrite(sKey, oListValue, procedureName, m_bIsWritable, ref Utils.UseWritable);
        }

        private object CreateOrderedDataTable<T1, T2>(List<KeyValuePair<T1, T2>> oListValue, string colNameKey, string colNameValue)

        {
            DataTable table = new DataTable();
            table.Columns.Add(colNameKey, typeof(T1));
            table.Columns.Add(colNameValue, typeof(T2));
            table.Columns.Add(new DataColumn()
            {
                ColumnName = "Ordered",
                DataType = System.Type.GetType("System.Int32"),
                AutoIncrement = true,
                AutoIncrementSeed = 1,
                AutoIncrementStep = 1
            });

            if (oListValue != null)
            {
                DataRow dr = null;
                foreach (KeyValuePair<T1, T2> obj in oListValue)
                {
                    dr = table.NewRow();
                    dr[colNameKey] = obj.Key;
                    dr[colNameValue] = obj.Value;
                    table.Rows.Add(dr);
                }
            }

            return table;
        }

        public void AddOrderKeyListParameter<T>(string sKey, ICollection<T> oListValue, string colName)
        {
            m_Parameters.Add(sKey, CreateOrderedKeyDataTable<T>(oListValue, colName));

            Utils.CheckDBReadWrite(sKey, oListValue, procedureName, m_bIsWritable, ref Utils.UseWritable);
        }

        private object CreateOrderedKeyDataTable<T>(IEnumerable<T> ids, string colName)

        {
            DataTable table = new DataTable();
            table.Columns.Add(colName, typeof(T));
            table.Columns.Add(new DataColumn()
            {
                ColumnName = "Ordered",
                DataType = System.Type.GetType("System.Int32"),
                AutoIncrement = true,
                AutoIncrementSeed = 1,
                AutoIncrementStep = 1
            });

            if (ids != null)
            {
                foreach (T id in ids)
                {
                    table.Rows.Add(id);
                }
            }

            return table;
        }

        public bool ShouldRouteToPrimary()
        {
            bool shouldRouteToPrimary = true;

            if (ApplicationConfiguration.Current.SqlTrafficConfiguration.ShouldUseTrafficHandler.Value)
            {
                if (ShouldForcePrimary)
                {
                    shouldRouteToPrimary = true;
                }
                else if (ShouldForceSecondary)
                {
                    shouldRouteToPrimary = false;
                }
                else
                {
                    DbPrimarySecondaryRouting routing = Utils.GetDbPrimarySecondaryRouting();

                    if (routing != null)
                    {
                        if (!string.IsNullOrEmpty(procedureName) && routing.QueryNameToShouldRouteToPrimaryMapping.ContainsKey(procedureName.ToLower()))
                        {
                            shouldRouteToPrimary = routing.QueryNameToShouldRouteToPrimaryMapping[procedureName];
                        }
                    }
                }
            }
            else
            {
                shouldRouteToPrimary = false;

                DbProceduresRouting dbSpRouting = Utils.GetDbProceduresRouting();
                if (dbSpRouting != null)
                {
                    if (!string.IsNullOrEmpty(procedureName) && dbSpRouting.ProceduresMapping.ContainsKey(procedureName))
                    {
                        ProcedureRoutingInfo procedureRoutingInfo = dbSpRouting.ProceduresMapping[procedureName];

                        if (!procedureRoutingInfo.VersionsToExclude.Contains(Utils.dBVersionPrefix.ToLower()))
                        {
                            shouldRouteToPrimary = procedureRoutingInfo.IsWritable;
                        }
                    }
                }
            }

            return shouldRouteToPrimary;
        }

        public string GetName()
        {
            return $"SP: {procedureName}";
        }
    }
}