using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Collections.Specialized;
using Microsoft.SqlServer.Server;

namespace ODBCWrapper
{
    public class StoredProcedure
    {
        #region Constructor
        public StoredProcedure(string sProcedureName)
        {
            m_sProcedureName = sProcedureName;
            m_Parameters = new Dictionary<string, object>();
            m_bIsWritable = false;
        }

        public void SetWritable(bool bIsWritable)
        {
            m_bIsWritable = bIsWritable;
        }

        ~StoredProcedure() { } 
        #endregion

        #region Private Fields
        private string m_sProcedureName;
        private Dictionary<string, object> m_Parameters;
        private int m_nTimeout;

        protected string m_sConnectionKey;
        protected bool m_bIsWritable;
        #endregion

        #region Private Properties
        private int LongQueryTime
        {
            get
            {
                //if (Utils.GetTcmConfigValue("QUERY_LONG") != string.Empty)
                //    return int.Parse(Utils.GetTcmConfigValue("QUERY_LONG"));
                return 2000;
            }
        } 
        #endregion      


        public void AddParameter(string sKey, object oValue)
        {
            m_Parameters.Add(sKey, oValue);
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
        }

        public void AddXMLParameter<T>(string sKey, List<T> oListValue, string colName)
        {
            m_Parameters.Add(sKey, CreateXML<T>(oListValue, colName));
        }


        public void AddIDListParameter<T>(string sKey, List<T> oListValue , string colName)
        {
            m_Parameters.Add(sKey, CreateDataTable<T>(oListValue, colName));
        }

        public void AddKeyValueListParameter<T1,T2>(string sKey, Dictionary<T1,List<T2>> oListKeyValue, string colNameKey, string colNameValue)
        {
            m_Parameters.Add(sKey, CreateDataTable<T1, T2>(oListKeyValue, colNameKey, colNameValue));
        }

        public void AddDataTableParameter<T1, T2>(string sKey, DataTable oDictValue, string colNameKey, string colNameValue)
        {
            m_Parameters.Add(sKey, CreateDataTable<T1, T2>(oDictValue, colNameKey, colNameValue));
        }   

        private object CreateDataTable(IEnumerable<int> ids, string colName)
        {
            DataTable table = new DataTable();
            table.Columns.Add(colName, typeof(Int32));
            foreach (Int32 id in ids)
            {
                table.Rows.Add(id);
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
            command.CommandText = m_sProcedureName;

            foreach (string item in m_Parameters.Keys)
            {
                command.Parameters.Add(new SqlParameter(item, m_Parameters[item]));               
            }
            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable);
            if (sConn == "")
                return null;
            using (SqlConnection con = new SqlConnection(sConn))
            {
                try
                {
                    con.Open();
                    SetLockTimeOut(con);

                    command.Connection = con;

                    DateTime dStart = DateTime.Now;

                    SqlDataReader reader = command.ExecuteReader();

                    TimeSpan t = DateTime.Now - dStart;
                    if (t.TotalMilliseconds > LongQueryTime)
                    {
                        string sMes = string.Format("Stored Procedure '{0}' Run Took {1} ms", m_sProcedureName, t.TotalMilliseconds);
                        Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net_Long");
                    }
                    else
                    {
                        if (reader.HasRows)
                        {
                            result = new DataTable();
                            result.Load(reader);
                        }
                    }

                    reader.Close();
                    reader.Dispose();
                }
                catch (Exception ex)
                {
                    string sMes = "While running : '" + m_sProcedureName + "'\r\n Exception accured: " + ex.Message;
                    Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net", ex.Message);
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
            command.CommandText = m_sProcedureName;

            foreach (string item in m_Parameters.Keys)
            {   
                command.Parameters.Add(new SqlParameter(item, m_Parameters[item]));             
            }
            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable);
            if (sConn == "")
                return null;
            using (SqlConnection con = new SqlConnection(sConn))
            {
                try
                {
                    con.Open();
                    SetLockTimeOut(con);
                    DateTime dStart = DateTime.Now;
                    command.Connection = con;

                    da.SelectCommand = command;
                    da.Fill(result);
                    con.Close();                 


                    TimeSpan t = DateTime.Now - dStart;
                    if (t.TotalMilliseconds > LongQueryTime)
                    {
                        string sMes = string.Format("Stored Procedure '{0}' Run Took {1} ms", m_sProcedureName, t.TotalMilliseconds);
                        Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net_Long");
                    }
                }
                catch (Exception ex)
                {
                    con.Close();
                    string sMes = "While running : '" + m_sProcedureName + "'\r\n Exception accured: " + ex.Message;
                    Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net", ex.Message);
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
            command.CommandText = m_sProcedureName;

            foreach (string item in m_Parameters.Keys)
            {
                command.Parameters.Add(new SqlParameter(item, m_Parameters[item]));
            }

            SqlParameter sqlReturnedValueParam = new SqlParameter();        
            sqlReturnedValueParam.Direction = ParameterDirection.ReturnValue;
            command.Parameters.Add(sqlReturnedValueParam); 

            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable);
            if (sConn == "")
                return result;
            using (SqlConnection con = new SqlConnection(sConn))
            {
                try
                {
                    con.Open();
                    SetLockTimeOut(con);
                    DateTime dStart = DateTime.Now;
                    command.Connection = con;
                    object dbRes = command.ExecuteScalar();
                    result = (T)Convert.ChangeType(sqlReturnedValueParam.Value, typeof(T));

                    con.Close();


                    TimeSpan t = DateTime.Now - dStart;
                    if (t.TotalMilliseconds > LongQueryTime)
                    {
                        string sMes = string.Format("Stored Procedure '{0}' Run Took {1} ms", m_sProcedureName, t.TotalMilliseconds);
                        Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net_Long");
                    }
                }
                catch (Exception ex)
                {
                    con.Close();
                    string sMes = "While running : '" + m_sProcedureName + "'\r\n Exception accured: " + ex.Message;
                    Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net", ex.Message);
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
            command.CommandText = m_sProcedureName;

            foreach (string item in m_Parameters.Keys)
            {
                command.Parameters.Add(new SqlParameter(item, m_Parameters[item]));
            }

            SqlParameter sqlReturnedValueParam = new SqlParameter();
            sqlReturnedValueParam.Direction = ParameterDirection.ReturnValue;
            command.Parameters.Add(sqlReturnedValueParam);

            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable);
            if (sConn == "")
                return result;
            using (SqlConnection con = new SqlConnection(sConn))
            {
                try
                {
                    con.Open();
                    SetLockTimeOut(con);
                    DateTime dStart = DateTime.Now;
                    command.Connection = con;
                    result = command.ExecuteScalar();
                    //result = (T)Convert.ChangeType(sqlReturnedValueParam.Value, typeof(T));

                    con.Close();


                    TimeSpan t = DateTime.Now - dStart;
                    if (t.TotalMilliseconds > LongQueryTime)
                    {
                        string sMes = string.Format("Stored Procedure '{0}' Run Took {1} ms", m_sProcedureName, t.TotalMilliseconds);
                        Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net_Long");
                    }
                }
                catch (Exception ex)
                {
                    con.Close();
                    string sMes = "While running : '" + m_sProcedureName + "'\r\n Exception accured: " + ex.Message;
                    Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net", ex.Message);
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
            command.CommandText = m_sProcedureName;

            foreach (string item in m_Parameters.Keys)
            {
                command.Parameters.Add(new SqlParameter(item, m_Parameters[item]));
            }

            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable);
            if (sConn == "")
                return;
            using (SqlConnection con = new SqlConnection(sConn))
            {
                try
                {
                    con.Open();
                    SetLockTimeOut(con);
                    DateTime dStart = DateTime.Now;
                    command.Connection = con;
                    command.ExecuteNonQuery(); 
                    con.Close();
                    TimeSpan t = DateTime.Now - dStart;
                    if (t.TotalMilliseconds > LongQueryTime)
                    {
                        string sMes = string.Format("Stored Procedure '{0}' Run Took {1} ms", m_sProcedureName, t.TotalMilliseconds);
                        Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net_Long");
                    }
                }
                catch (Exception ex)
                {
                    con.Close();
                    string sMes = "While running : '" + m_sProcedureName + "'\r\n Exception accured: " + ex.Message;
                    Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net", ex.Message);   
                }
            }
        }


        public DataSet ExecuteDataSetWithListParam()
        {
            DataSet result = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();

            SqlCommand command = new SqlCommand();

            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = m_sProcedureName;

            foreach (string item in m_Parameters.Keys)
            {
                Type type = m_Parameters[item].GetType();
                if (type.FullName == typeof(System.String).FullName || type.FullName == typeof(System.Boolean).FullName || type.FullName == typeof(System.Int32).FullName)
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
            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable);
            if (sConn == "")
                return null;

            using (SqlConnection con = new SqlConnection(sConn))
            {
                try
                {
                    con.Open();
                    SetLockTimeOut(con);
                    DateTime dStart = DateTime.Now;
                    command.Connection = con;

                    da.SelectCommand = command;
                    DataTable dataTable = new DataTable();
                    dataTable.BeginLoadData();
                    da.Fill(dataTable);
                    dataTable.EndLoadData();
                    result.EnforceConstraints = false;
                    result.Tables.Add(dataTable);
                    con.Close();


                    TimeSpan t = DateTime.Now - dStart;
                    if (t.TotalMilliseconds > LongQueryTime)
                    {
                        string sMes = string.Format("Stored Procedure '{0}' Run Took {1} ms", m_sProcedureName, t.TotalMilliseconds);
                        Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net_Long");
                    }
                }
                catch (Exception ex)
                {
                    con.Close();
                    string sMes = "While running : '" + m_sProcedureName + "'\r\n Exception accured: " + ex.Message;
                    Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net", ex.Message);
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
